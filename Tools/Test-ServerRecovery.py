"""Native isolated backup, exact restore, rollback and interrupted-recovery acceptance."""
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import json
from pathlib import Path
import subprocess
import sys
import uuid
import runpy
import server_recovery
from recovery_fixture import RecoveryFixture
from server_recovery import Recovery, OperationError, encoded, digest, unpack, checkpoint, recover_missing, ENROLLMENT, FILES, SAVES
from shared_garden_runtime import ROOT, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int)
    parser.add_argument('--qualify-build', action='store_true', help='Test a candidate only inside this disposable enrolled family; does not change the production recovery gate.')
    args = parser.parse_args(); fixture = RecoveryFixture(args.build)
    if args.build not in server_recovery.QUALIFIED_BUILDS:
        require(args.qualify_build and fixture.controller.isolated, 'Candidate recovery requires an isolated qualification run')
        server_recovery.MAX_QUALIFIED_BUILD = args.build
        server_recovery.QUALIFIED_BUILDS = server_recovery.QUALIFIED_BUILDS | {args.build}
    scenic = read(ROOT / f'Builds/NetworkProbe/G3-0.0.{args.build}/build-summary.json')['contract'] >= 5
    recovery = Recovery(fixture.run_id, args.build)
    checks = []; success = False; server = fixture.controller
    save = fixture.path / 'server-world/world.save'
    def passed(name):
        checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    def refused(call):
        try: call()
        except OperationError: return
        raise AssertionError('Unsafe recovery was accepted')
    def saved(): return {n: (save.parent / n).read_bytes() if (save.parent / n).exists() else None for n in SAVES}
    def drop(client, x, y):
        client.input('press', role='bucket-1'); client.input('release', x=x, y=y)
        wait(lambda: not client.input('inspect')['pending'], 'settled item action')
    try:
        server.start(); clients = [fixture.join(i) for i in range(1, 5)]
        drop(clients[0], 730, 160)
        if scenic:
            travel = runpy.run_path(str(ROOT / 'Tools/Test-ScenicWorlds.py'))['travel']
            travel(clients[1], 'creek'); travel(clients[2], 'home'); travel(clients[3], 'beach')
        else:
            clients[1].input('button', text='Creek'); wait(lambda: not clients[1].input('inspect')['pending'], 'travel')
        live = recovery.backup(); bundle, files, body = unpack(Path(live['path']))
        require(set(files) == set(FILES) and body['receipts'] and {p['zone'] for p in body['players']} == ({'garden', 'creek', 'beach'} if scenic else {'garden', 'creek'}), 'Incomplete backup')
        if scenic: require(body['schema'] == 3 and any(p['x'] < 0 for p in body['players']), 'Scenic coordinates missing from backup')
        original_instance = server.snapshot()['instanceId']
        refused(lambda: recovery.restore(Path(live['path']), digest(save.read_bytes())))
        require(server.snapshot()['instanceId'] == original_instance and server.snapshot()['players'] == 4, 'Backup/restore attempt interrupted play')
        passed('live backup verifies protected enrollment and full multi-area/four-player checkpoint; active restore refuses without interrupting play')
        fixture.stop(); baseline = recovery.backup(); baseline_path = Path(baseline['path'])
        baseline_bytes = save.read_bytes()
        server.start(); client = fixture.join(1); drop(client, 410, 200); fixture.stop()
        before = saved(); require(before['world.save'] != baseline_bytes, 'Test did not change the world')
        refused(lambda: recovery.restore(baseline_path, 'stale-save-hash'))
        require(saved() == before, 'Stale restore changed files')
        result = recovery.restore(baseline_path, digest(before['world.save']))
        require(save.read_bytes() == baseline_bytes and not recovery.marker.exists(), 'Restore differs')
        server.start()
        require(checkpoint(save.read_bytes()) == checkpoint(baseline_bytes), 'Native load changed released checkpoint')
        clients = [fixture.join(i) for i in range(1, 5)]
        state = read(fixture.path / server.snapshot()['instanceId'] / 'view.json')
        restored = checkpoint(save.read_bytes()); original = checkpoint(baseline_bytes)
        require(restored['players'] == original['players'] and restored['toys'] == original['toys'] and restored['receipts'] == original['receipts'], 'Native identity/item/receipt restore differs')
        require(len(state['connected']) == 4, 'Saved enrollment did not reconnect')
        fixture.stop()
        recovery.rollback(result['rollbackJob'], digest(save.read_bytes()))
        require(saved() == before, 'Rollback did not preserve all prior save bytes')
        passed('stale decision refused; exact checkpoint restored; four original enrolled clients rejoin; rollback restores all previous save paths')
        source = json.loads(baseline_path.read_bytes()); invalid_dir = fixture.path / 'invalid-backups'; invalid_dir.mkdir()
        cases = []
        v = deepcopy(source); v['files']['world.save']['sha256'] = '0' * 64; cases.append(('checksum', v))
        v = deepcopy(source); v['version'] = 2; cases.append(('format', v))
        v = deepcopy(source); v['build'] = server_recovery.MAX_QUALIFIED_BUILD + 1; cases.append(('future-build', v))
        v = deepcopy(source); v['files']['../world.save'] = v['files'].pop('world.save'); cases.append(('path', v))
        v = deepcopy(source); v['world'] = uuid.uuid4().hex; cases.append(('wrong-family', v))
        v = deepcopy(source); v['files']['player-1.pairing'] = v['files']['player-2.pairing']; cases.append(('wrong-enrollment', v))
        v = deepcopy(source); invalid = deepcopy(original); invalid['toys'][0]['water'] = 100
        payload = json.dumps(invalid); raw = ('LITTLEWEEPS-SOLO-1\n' + digest(payload.encode()) + '\n' + payload).encode()
        v['files']['world.save'] = encoded(raw); cases.append(('invalid-game-state', v))
        v = deepcopy(source); invalid['schema'] = 4
        payload = json.dumps(invalid); v['files']['world.save'] = encoded(('LITTLEWEEPS-SOLO-1\n' + digest(payload.encode()) + '\n' + payload).encode()); cases.append(('future-schema', v))
        for name, value in cases:
            path = invalid_dir / (name + '.lwbackup'); path.write_text(json.dumps(value), encoding='utf-8')
            refused(lambda: recovery.restore(path, digest(save.read_bytes())))
            require(saved() == before and not recovery.marker.exists(), 'Invalid backup damaged usable state: ' + name)
        truncated = invalid_dir / 'truncated.lwbackup'; truncated.write_bytes(b'{"files":')
        refused(lambda: recovery.restore(truncated, digest(save.read_bytes())))
        passed('nine corrupt, incompatible, wrong-family/enrollment, invalid-state and unsafe-file-name backups refuse before touching usable saves')
        def crash(stage):
            if stage == 'world.save': raise RuntimeError('Simulated recovery process interruption')
        try: recovery.restore(baseline_path, digest(save.read_bytes()), fault=crash)
        except RuntimeError: pass
        require(recovery.marker.exists() and server.snapshot()['state'] == 'unreachable', 'Incomplete restore was not gated')
        refused(server.start)
        launcher = subprocess.run([sys.executable, str(ROOT / 'Tools/Start-FamilyLAN.py'), '--build', str(args.build), '--family', fixture.run_id], capture_output=True, timeout=30)
        require(launcher.returncode != 0, 'Launcher ignored recovery marker')
        direct = fixture.launch(); direct.process.wait(timeout=25)
        native = read(direct.out / 'status.json')
        require(direct.process.returncode != 0 and native['status'] == 'failed' and 'recovery is incomplete' in native['reason'], 'Native recovery guard absent')
        recovery.rollback()
        require(saved() == before and not recovery.marker.exists(), 'Interrupted restore rollback differs')
        server.start(); fixture.stop()
        passed('interruption after replacing primary blocks parent, launcher and direct native startup; explicit rollback restores all old bytes and permits restart')
        save.write_bytes(b'isolated test corruption')
        repair = recovery.restore(baseline_path, digest(save.read_bytes()))
        require(save.read_bytes() == baseline_bytes, 'Corrupt primary was not repairable')
        recovery.rollback(repair['rollbackJob'], digest(save.read_bytes()))
        require(save.read_bytes() == b'isolated test corruption', 'Corrupt preimage lost')
        recovery.restore(baseline_path, digest(save.read_bytes()))
        server.start(); fixture.stop()
        passed('corrupt current primary can be repaired while exact damaged originals remain recoverable; repaired world starts normally')
        refused(lambda: recover_missing(baseline_path, fixture.run_id))
        # Simulate loss only by retaining this stopped test world's entire old
        # directory. Check both resolved paths before the directory move.
        archive = ROOT / 'LocalData/RecoveryJobs' / ('missing-test-' + uuid.uuid4().hex)
        require(fixture.path.resolve().is_relative_to((ROOT / 'LocalData/FamilyLAN').resolve())
                and fixture.path.name == fixture.run_id
                and archive.resolve().is_relative_to((ROOT / 'LocalData/RecoveryJobs').resolve())
                and not server.processes(), 'Unsafe test directory move')
        fixture.path.rename(archive)
        recover_missing(baseline_path, fixture.run_id)
        require(save.read_bytes() == baseline_bytes and all((fixture.path / n).read_bytes() == (archive / n).read_bytes() for n in ENROLLMENT), 'Missing-world reconstruction differs')
        server.start(); clients = [fixture.join(i) for i in range(1, 5)]
        require(server.snapshot()['players'] == 4, 'Reconstructed enrollment did not admit the original players')
        fixture.stop()
        passed('missing entire test world reconstructed from the bundle with byte-identical protected enrollment; original four players admitted; existing-directory replacement refused')
        success = True
    finally:
        fixture.cleanup()
        result = dict(passed=success, build=args.build, utc=datetime.now(timezone.utc).isoformat(), checks=checks,
                      scope='Separate enrolled Windows world and four native Windows clients; same-user DPAPI backup/restore. No family world restoration, physical-device, independent-storage or cross-machine recovery claim.')
        write(fixture.path / 'recovery-result.json', result)
        print('Evidence:', fixture.path / 'recovery-result.json', flush=True)


if __name__ == '__main__': main()
