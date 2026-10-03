"""G4-PREP-02: native restoration from a client's complete replica, not server.save.

Creates a disposable enrolled family, stops only that authority, retains its old
save, and starts a replacement from a copied client checkpoint. Reuses the lab's
PC authority credentials deliberately: mobile host capabilities are NOT tested.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import subprocess
import uuid

import psutil
from recovery_fixture import RecoveryFixture
from parent_server import checkpoint_bytes
from shared_garden_runtime import ROOT, read, write, wait, require


def payload(path):
    raw = checkpoint_bytes(path)[0]
    header, digest, body = raw.split(b'\n', 2)
    require(header == b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode() == digest, 'Invalid checkpoint')
    return json.loads(body), body, raw


def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int)
    args = parser.parse_args()
    helper = ROOT / 'Tools/Verification/HostingRestore.Tests/bin/Release/net9.0/HostingRestore.Tests.dll'
    require(helper.is_file(), 'Build the restoration check tool first')
    fixture = RecoveryFixture(args.build); passed = False; checks = []; observations = {}
    def ok(name):
        checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    out = None
    def command(client, action=0, original=None, outcome=None, duplicate=False, accepted=True, **kw):
        if original is None:
            current = read(out / 'view.json')['view']; player = next(p for p in current['players'] if p['id'] == client.profile)
            identity = uuid.uuid4().hex
            data = dict(requestId=identity, actor=client.profile, action=action, expectedRevision=current['revision'],
                        zone=player['zone'], visit=player['visit'], item='', target='', value='', x=0, y=0)
            data.update(kw); request = dict(requestId=identity, protocol=3, command=data)
        else:
            request = deepcopy(original); identity = request['requestId']
        client.serial += 1
        write(client.out / 'control.json', dict(serial=client.serial, kind='command', request=request))
        epoch = read(out / 'view.json')['epoch']
        response = wait(lambda: (v if (v := read(client.out / ('reply-' + identity + '.json')))
                                and v['epoch'] == epoch and v['duplicate'] == duplicate
                                and (outcome is None or v['outcome'] == outcome) else None), 'native command response')
        require(response['accepted'] == accepted, 'Unexpected command result: ' + response['outcome'])
        return request
    def stage(source, expected_path, target, accepted=True):
        result = subprocess.run(['dotnet', str(helper), 'stage', str(source), str(expected_path), str(target)],
                                capture_output=True, text=True, timeout=30, creationflags=subprocess.CREATE_NO_WINDOW)
        require((result.returncode == 0) == accepted, 'Restoration tool result differs: ' + result.stderr)
    try:
        fixture.controller.start(); native = fixture.controller.processes()[0]; out = native['output']
        clients = [fixture.join(i) for i in range(1, 5)]
        for i in range(130): command(clients[0], x=250 + i, y=100)
        command(clients[0], action=5, value='garden')
        command(clients[0], action=2, item='bucket-1')
        command(clients[0], action=3, item='bucket-1', target='tap-1', x=150, y=340)
        command(clients[0], action=2, item='bucket-1')
        pour = command(clients[0], action=3, item='bucket-1', target='plant-1', x=810, y=330)
        # Refill after the retained pour so a replay can be checked against a
        # nonempty bucket as well as an unchanged receipt set/revision.
        command(clients[0], action=2, item='bucket-1')
        command(clients[0], action=3, item='bucket-1', target='tap-1', x=150, y=340)
        for _ in range(3):
            command(clients[2], action=2, item='sponge-1')
            command(clients[2], action=3, item='sponge-1', target='puddle-1', x=680, y=140)
        command(clients[1], action=7, value='creek')
        command(clients[1], action=2, item='bucket-creek')
        command(clients[1], action=3, item='bucket-creek', target='tap-creek', x=150, y=340)
        command(clients[1], action=2, item='sponge-creek')
        revision = read(out / 'view.json')['view']['revision']
        replica = fixture.path / 'client-recovery' / clients[3].profile / 'world.save'
        wait(lambda: replica.exists() and payload(replica)[0]['snapshot']['revision'] == revision
             and any(t['seconds'] > 1 for t in payload(replica)[0]['snapshot']['idleTimers']), 'complete native replica', 50)
        record, body, raw = payload(replica)
        snapshot = record['snapshot']; private = fixture.path / 'restore-experiment'; private.mkdir()
        source = private / 'client-replica.save'; source.write_bytes(raw)
        expected_path = private / 'expected.json'
        write(expected_path, dict(family=record['family'], authority=record['authority'], world=record['world'],
              epoch=record['epoch'], snapshotWorld=snapshot['worldId'], checkpoint=record['checkpoint'], revision=snapshot['revision'],
              profiles=[p['profile'] for p in fixture.players], payloadSha256=hashlib.sha256(body).hexdigest()))
        require(len(snapshot['receipts']) == 128 and len(snapshot['toys']) == 10 and len(body) > 16384
                and {p['zone'] for p in snapshot['players']} == {'garden', 'creek'}, 'Incomplete test coverage')
        require(next(t for t in snapshot['toys'] if t['id'] == 'sponge-creek')['holder'] == clients[1].profile, 'Expected stale hold missing')
        require(any(r['requestId'] == pour['requestId'] for r in snapshot['receipts']), 'Pour receipt not retained')
        wait(lambda: any(p['profile'] == clients[3].profile and p['checkpoint'] >= record['checkpoint']
                         for p in read(out / 'recovery-evidence.json')['peers']), 'source acknowledges durable replica')
        observations.update(replicaBytes=len(body), receipts=128, players=4, objects=10, timers=len(snapshot['idleTimers']))
        ok('actual enrolled client durably stores a multi-chunk world with both areas, four profiles, ten objects, completed actions, idle clocks and 128 receipts')

        # Crash only the authority whose config resolves to this random fixture.
        require(native in fixture.controller.processes() and fixture.path.name == fixture.run_id
                and fixture.path.resolve().parent == (ROOT / 'LocalData/FamilyLAN').resolve(), 'Unsafe test process selection')
        psutil.Process(native['pid']).terminate()
        wait(lambda: not fixture.controller.processes(), 'isolated source exit')
        local = wait(lambda: (s if not (s := clients[0].input('inspect'))['shared'] and s['adventure'] else None), 'private local continuation', 20)
        private_save = Path(local['savePath'])
        require(private_save.resolve().is_relative_to(fixture.path.resolve()) and private_save.is_file(), 'Unexpected private adventure path')
        # The open offline game is a legitimate writer (its idle clock advances).
        # Pin an immutable copy for refusal, then compare gameplay/provenance
        # across cold reopening separately from the elapsed local play clock.
        private_record, _, private_raw = payload(private_save)
        private_input = private / 'private-adventure.save'; private_input.write_bytes(private_raw)
        stage(private_input, expected_path, private / 'must-not-promote-private', accepted=False)
        require(not (private / 'must-not-promote-private').exists() and private_input.read_bytes() == private_raw, 'Private input altered or promoted')
        for client in clients: client.close()
        require(not fixture.controller.processes(), 'Source must remain stopped before restoration')
        original_dir = fixture.path / 'server-world'; retained = private / 'retained-source-authority'
        require(original_dir.resolve().parent == fixture.path.resolve() and retained.resolve().is_relative_to(private.resolve()), 'Unsafe fixture archive path')
        original_dir.rename(retained)
        stage(source, expected_path, original_dir)
        restored, _, _ = payload(original_dir / 'world.save')
        wanted = deepcopy(snapshot); wanted['revision'] += 1
        held_ids = {t['id'] for t in wanted['toys'] if t['holder']}
        for toy in wanted['toys']:
            if toy['id'] in held_ids: toy['holder'] = ''; toy['resetPending'] = False
        wanted['idleTimers'] = [t for t in wanted['idleTimers'] if t['item'] not in held_ids]
        require(restored == wanted and source.read_bytes() == raw, 'Restore lost state or changed its source')
        stage(source, expected_path, original_dir, accepted=False)
        require(payload(original_dir / 'world.save')[0] == restored, 'Create-only retry replaced authority')
        ok('source stopped and retained; client replica alone restores exact world with one hold-release revision; actual private adventure refuses promotion')

        fixture.controller.start(); replacement = fixture.controller.processes()[0]; out = replacement['output']
        require(replacement['pid'] != native['pid'] and replacement['instanceId'] != native['instanceId'], 'Not a replacement process')
        require(read(out / 'view.json')['epoch'] != record['epoch'], 'Old transport epoch reused')
        require(payload(original_dir / 'world.save')[0] == restored, 'Native startup changed restored world')
        clients = [fixture.join(i) for i in range(1, 5)]
        for client in clients:
            wait(lambda: client.state()['view']['revision'] >= restored['revision'], 'restored view received')
            view = client.state()['view']
            require(all(view[k] == restored[k] for k in ('worldId', 'players', 'toys')), 'Restored client view differs')
        require(fixture.controller.snapshot()['players'] == 4, 'Four original enrolled profiles did not rejoin')
        private_rejoined, _, private_before = payload(private_save)
        def without_idle(record):
            value = deepcopy(record); value['snapshot'].pop('idleTimers'); return value
        require(without_idle(private_rejoined) == without_idle(private_record), 'Reopening/rejoining changed private gameplay or provenance')
        prior_timers = {t['item']: t['seconds'] for t in private_record['snapshot']['idleTimers']}
        require(all(t['seconds'] >= prior_timers.get(t['item'], 0) for t in private_rejoined['snapshot']['idleTimers']), 'Private idle clock rolled back')
        before = payload(original_dir / 'world.save')[0]
        command(clients[0], original=pour, duplicate=True, outcome='plant-watered')
        after = payload(original_dir / 'world.save')[0]
        require(all(before[k] == after[k] for k in ('revision', 'players', 'toys', 'receipts')), 'Duplicate pour mutated authority')
        changed = deepcopy(pour); changed['command']['x'] += 1
        command(clients[0], original=changed, outcome='request-id-reused', accepted=False)
        require(payload(original_dir / 'world.save')[0]['receipts'] == before['receipts'], 'Conflicting replay changed receipt history')
        ok('fresh native authority loads replica-derived world; four original enrolled profiles rejoin; retained pour is idempotent and altered reuse is rejected over transport')

        command(clients[1], action=2, item='sponge-creek')
        command(clients[1], action=3, item='sponge-creek', x=550, y=110)
        command(clients[0], action=2, item='bucket-1')
        command(clients[0], action=3, item='bucket-1', x=400, y=120)
        command(clients[0], x=505, y=150)
        command(clients[1], x=605, y=160)
        now = payload(original_dir / 'world.save')[0]
        require(len(now['toys']) == len({t['id'] for t in now['toys']}) == 10, 'Object duplication')
        require(next(t for t in now['toys'] if t['id'] == 'bucket-1')['water'] == 3, 'Bucket contents lost')
        require(next(t for t in now['toys'] if t['id'] == 'plant-1')['water'] == 3
                and next(t for t in now['toys'] if t['id'] == 'puddle-1')['water'] == 0, 'Completed interactions changed')
        for client in clients:
            rpath = fixture.path / 'client-recovery' / client.profile / 'world.save'
            wait(lambda: payload(rpath)[0]['epoch'] == read(out / 'view.json')['epoch']
                 and payload(rpath)[0]['snapshot']['revision'] >= now['revision'], 'replacement checkpoint durable on client', 50)
        require(checkpoint_bytes(private_save)[0] == private_before, 'Reunion changed archived private adventure')
        ok('both areas accept new movement and item actions, stale hold is reusable, object IDs stay unique and all four clients durably replicate the new authority epoch')
        passed = True
    finally:
        fixture.cleanup()
        result = dict(passed=passed, build=args.build, utc=datetime.now(timezone.utc).isoformat(), checks=checks, observations=observations,
            scope='Isolated Windows dedicated authority plus four Windows clients over enrolled transport. Source stopped explicitly; same lab PC authority credentials reused. No live family, mobile authority capabilities, election, automatic handoff, partition fencing, or device qualification.')
        write(fixture.path / 'replacement-authority-result.json', result)
        print('Evidence:', fixture.path / 'replacement-authority-result.json', flush=True)


if __name__ == '__main__': main()
