"""Isolated portable backup/reconstruction acceptance with four original clients."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import json
from pathlib import Path
import secrets
import subprocess
import sys
import uuid
import runpy
from unittest.mock import patch

import portable_recovery as portable
import server_recovery as local
from family_pairing import unprotect
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import ROOT, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int)
    args = parser.parse_args(); fixture = RecoveryFixture(args.build)
    recovery = local.Recovery(fixture.run_id, args.build)
    job = ROOT / 'LocalData/RecoveryJobs' / ('portable-test-' + uuid.uuid4().hex)
    job.mkdir(parents=True)
    password = secrets.token_urlsafe(36)  # Ephemeral test-only secret; never logged/persisted.
    checks = []; success = False
    def passed(name):
        checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    def refused(call):
        try: call()
        except (local.OperationError, OSError): return
        raise AssertionError('Unsafe portable recovery was accepted')
    save = fixture.path / 'server-world/world.save'
    try:
        fixture.controller.start(); clients = [fixture.join(i) for i in range(1, 5)]
        clients[0].input('press', role='bucket-1'); clients[0].input('release', x=720, y=180)
        if read(ROOT / f'Builds/NetworkProbe/G3-0.0.{args.build}/build-summary.json')['contract'] >= 5:
            runpy.run_path(str(ROOT / 'Tools/Verification/Test-ScenicWorlds.py'))['travel'](clients[1], 'creek')
        else:
            clients[1].input('button', text='Creek')
        wait(lambda: all(not c.input('inspect')['pending'] for c in clients), 'settled two-area actions')
        live = recovery.backup(job)
        active = fixture.controller.snapshot()
        encrypted = job / 'family.lwportable'
        result = portable.export(Path(live['path']), encrypted, password)
        refused(lambda: portable.recover_missing(encrypted, password, fixture.run_id, args.build))
        require(fixture.controller.snapshot()['instanceId'] == active['instanceId']
                and fixture.controller.snapshot()['players'] == 4, 'Export/recovery refusal interrupted players')
        fixture.stop()
        baseline = recovery.backup(job); baseline_path = Path(baseline['path'])
        local_bundle, original_files, body = local.unpack(baseline_path)
        baseline_bytes = original_files['world.save']
        require(body['receipts'] and {p['zone'] for p in body['players']} == {'garden', 'creek'}, 'Incomplete fixture')
        encrypted = job / 'stopped.lwportable'
        result = portable.export(baseline_path, encrypted, password)
        raw = encrypted.read_bytes()
        bundle, files, records, restored_body = portable.load(encrypted, password)
        require(restored_body == body and files['world.save'] == baseline_bytes, 'Export changed world')
        private_strings = [r['privateKey'].encode() for r in records.values() if 'privateKey' in r]
        private_strings += [r['credential'].encode() for r in records.values() if 'credential' in r]
        require(all(s not in raw for s in private_strings)
                and fixture.run_id.encode() not in raw and baseline_bytes not in raw, 'Plaintext in portable output')
        second = job / 'second.lwportable'; portable.export(baseline_path, second, password)
        require(raw != second.read_bytes(), 'Repeated export reused ciphertext')
        refused(lambda: portable.export(baseline_path, encrypted, password))
        require(encrypted.read_bytes() == raw, 'Create-only export replaced backup')
        passed('live export leaves four players connected; encrypted round trip preserves full two-area checkpoint; unique exports and create-only files')

        # Decrypt and validate without invoking either original DPAPI unprotect
        # entry point. This proves format independence, not a second-PC install.
        with patch.object(portable, 'unprotect', side_effect=AssertionError('Original DPAPI unavailable')), \
                patch.object(local, 'unprotect', side_effect=AssertionError('Original DPAPI unavailable')):
            require(portable.verify(encrypted, password)['verified'], 'Portable verification required DPAPI')
        passed('portable decryption and full enrollment/game validation succeed with DPAPI unprotect unavailable')

        before = {n: (fixture.path / n).read_bytes() for n in local.ENROLLMENT}
        before_save = save.read_bytes()
        refused(lambda: portable.verify(encrypted, secrets.token_urlsafe(36)))
        invalid = job / 'invalid.lwportable'
        bad_raws = [raw[:-1], raw[:portable.HEADER_SIZE], b'bad header' + raw,
                    raw[:len(portable.MAGIC)] + bytes([raw[len(portable.MAGIC)] ^ 1]) + raw[len(portable.MAGIC) + 1:],
                    raw[:-1] + bytes([raw[-1] ^ 1]), b'x' * (portable.FILE_LIMIT + 1)]
        for value in bad_raws:
            invalid.write_bytes(value); refused(lambda: portable.recover_missing(invalid, password, fixture.run_id, args.build))
        # Unsupported format is rejected before allocating KDF memory.
        invalid.write_bytes(raw.replace(portable.MAGIC, portable.MAGIC.replace(b'-1', b'-2'), 1))
        with patch.object(portable, 'key', side_effect=AssertionError('Unsupported profile derived a key')):
            refused(lambda: portable.verify(invalid, password))
        cases = []
        v = deepcopy(bundle); v['version'] = 2; cases.append(v)
        v = deepcopy(bundle); v['build'] = local.MAX_QUALIFIED_BUILD + 1; cases.append(v)
        v = deepcopy(bundle); v['world'] = uuid.uuid4().hex; cases.append(v)
        v = deepcopy(bundle); v['revision'] += 1; cases.append(v)
        v = deepcopy(bundle); v['files']['../world.save'] = v['files'].pop('world.save'); cases.append(v)
        v = deepcopy(bundle); v['files'].pop('issuer.pairing'); cases.append(v)
        v = deepcopy(bundle); v['files']['player-1.pairing'] = v['files']['player-2.pairing']; cases.append(v)
        v = deepcopy(bundle); record = deepcopy(records['authority.pairing']); record['members'].append(record['members'][0])
        v['files']['authority.pairing'] = local.encoded(local.json_bytes(record)); cases.append(v)
        v = deepcopy(bundle); record = deepcopy(records['issuer.pairing']); record['privateKey'] = records['authority.pairing']['privateKey']
        v['files']['issuer.pairing'] = local.encoded(local.json_bytes(record)); cases.append(v)
        v = deepcopy(bundle); world = deepcopy(body); world['toys'][0]['water'] = 100
        payload = json.dumps(world)
        v['files']['world.save'] = local.encoded(('LITTLEWEEPS-SOLO-1\n' + local.digest(payload.encode()) + '\n' + payload).encode()); cases.append(v)
        for value in cases:
            invalid.write_bytes(portable.seal(local.json_bytes(value), password))
            refused(lambda: portable.recover_missing(invalid, password, fixture.run_id, args.build))
        duplicate = local.json_bytes(bundle).replace(b'"version": 1', b'"version": 1, "version": 1', 1)
        invalid.write_bytes(portable.seal(duplicate, password)); refused(lambda: portable.verify(invalid, password))
        refused(lambda: portable.recover_missing(encrypted, password, uuid.uuid4().hex, args.build))
        refused(lambda: portable.recover_missing(encrypted, password, fixture.run_id, args.build - 1))
        require(before_save == save.read_bytes() and before == {n: (fixture.path / n).read_bytes() for n in local.ENROLLMENT}, 'Refused input changed files')
        passed('wrong password, seven malformed/tampered/oversized envelopes, eleven authenticated invalid payloads, wrong family/build and existing-family recovery refuse without changing saves')

        # Retain only this stopped disposable world. Check both resolved paths
        # before a move; never enumerate or move any other family directory.
        archive = job / 'retained-original'
        require(fixture.path.resolve().is_relative_to((ROOT / 'LocalData/FamilyLAN').resolve())
                and fixture.path.name == fixture.run_id and archive.resolve().is_relative_to(job.resolve())
                and not fixture.controller.processes(), 'Unsafe fixture move')
        fixture.path.rename(archive)
        def crash_at(stage):
            def fail(current):
                if current == stage: raise OSError('Simulated staging interruption')
            return fail
        for stage in ('enrollment', 'publish'):
            refused(lambda: portable.recover_missing(encrypted, password, fixture.run_id, args.build, fault=crash_at(stage)))
            require(not fixture.path.exists(), 'Interrupted staging published a family')
        def concurrent_creator(stage):
            if stage == 'publish':
                fixture.path.mkdir(); (fixture.path / 'sentinel').write_bytes(b'retained concurrent destination')
        refused(lambda: portable.recover_missing(encrypted, password, fixture.run_id, args.build, fault=concurrent_creator))
        require((fixture.path / 'sentinel').read_bytes() == b'retained concurrent destination'
                and sorted(p.name for p in fixture.path.iterdir()) == ['sentinel'], 'Concurrent destination was replaced')
        race_archive = job / 'retained-concurrent-destination'
        require(fixture.path.resolve().is_relative_to((ROOT / 'LocalData/FamilyLAN').resolve())
                and race_archive.resolve().is_relative_to(job.resolve()), 'Unsafe race fixture move')
        fixture.path.rename(race_archive)
        passed('interruptions after protected enrollment and before publication leave no startable family; concurrent destination wins without replacement')

        original_protected = {original_files[n] for n in portable.PAIRINGS}
        def destination_only(raw):
            require(raw not in original_protected, 'Recovery depended on original DPAPI ciphertext')
            return unprotect(raw)
        with patch.object(local, 'unprotect', side_effect=destination_only), \
                patch.object(portable, 'unprotect', side_effect=AssertionError('Source DPAPI unavailable')):
            result = portable.recover_missing(encrypted, password, fixture.run_id, args.build)
        require(result['reconstructed'] and not result['started'] and save.read_bytes() == baseline_bytes
                and (save.parent / 'world.save.bak').read_bytes() == baseline_bytes, 'Reconstruction changed checkpoint or started')
        require(all(unprotect((fixture.path / n).read_bytes()) == records[n] for n in portable.PAIRINGS)
                and all((fixture.path / n).read_bytes() != original_files[n] for n in portable.PAIRINGS), 'Reprotected identities differ')
        require((fixture.path / 'family.json').read_bytes() == files['family.json'], 'Public identity changed')
        fixture.controller.start()
        for i in range(1, 5):
            client = fixture.launch(i, pairing_path=archive / f'player-{i}.pairing')
            wait(lambda: client.status() and client.status()['status'] == 'connected'
                 and client.input('inspect')['shared'], 'original enrolled client joins recovered server', 45)
        require(fixture.controller.snapshot()['players'] == 4, 'Original players failed to join')
        current = local.checkpoint(save.read_bytes())
        require(current['players'] == body['players'] and current['toys'] == body['toys']
                and current['receipts'] == body['receipts'], 'Restored positions/items/receipts changed')
        fixture.stop()
        passed('missing family reconstructed with newly protected credentials and exact checkpoint; all four original enrolled clients rejoin with positions/items/receipts preserved')

        cli = subprocess.run([sys.executable, str(ROOT / 'Tools/Portable-Recovery.py'), 'verify', '--backup', str(encrypted)],
                             input=b'not-an-accepted-password-channel\n', capture_output=True, timeout=15)
        require(cli.returncode != 0 and b'interactive terminal' in cli.stderr
                and b'not-an-accepted-password-channel' not in cli.stdout + cli.stderr, 'CLI accepted/echoed redirected passphrase')
        passed('CLI refuses redirected passphrases instead of falling back to echoed input')
        success = True
    finally:
        fixture.cleanup()
        result = dict(passed=success, build=args.build, utc=datetime.now(timezone.utc).isoformat(), checks=checks,
                      scope='Isolated Windows world and four original Windows clients. Password envelope verified without original DPAPI access and restored to new DPAPI blobs on the same account/PC. No real-family export, off-PC storage, second-account/machine, mobile, VPS or live-world recovery qualification.')
        write(job / 'result.json', result)
        print('Evidence:', job / 'result.json', flush=True)


if __name__ == '__main__': main()
