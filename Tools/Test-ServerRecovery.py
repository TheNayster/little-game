"""Native isolated backup, exact restore, rollback and interrupted-recovery acceptance."""
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import json
from pathlib import Path
import subprocess
import time
import sys
import uuid
import runpy
import server_recovery
from recovery_fixture import RecoveryFixture
from server_recovery import Recovery, OperationError, encoded, digest, unpack, checkpoint, recover_missing, ENROLLMENT, FILES, SAVES
from shared_garden_runtime import ROOT, read, write, wait, require


def native_equivalent(a,b,key=''):
    # Restored FILE bytes remain exact (asserted before launch). Unity's JSON
    # round-trip may alter the last bit of a double clock, not gameplay state.
    if isinstance(a,dict):return isinstance(b,dict) and a.keys()==b.keys() and all(native_equivalent(v,b[k],k) for k,v in a.items())
    if isinstance(a,list):return isinstance(b,list) and len(a)==len(b) and all(native_equivalent(x,y) for x,y in zip(a,b))
    if key in ('elapsed','remainder','hitAge','seconds','useSeconds') and isinstance(a,(float,int)) and isinstance(b,(float,int)):
        return abs(a-b)<=1e-9
    return a==b

def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int)
    parser.add_argument('--qualify-build', action='store_true', help='Test a candidate only inside this disposable enrolled family; does not change the production recovery gate.')
    args = parser.parse_args(); fixture = RecoveryFixture(args.build)
    if args.build not in server_recovery.QUALIFIED_BUILDS:
        require(args.qualify_build and fixture.controller.isolated, 'Candidate recovery requires an isolated qualification run')
        server_recovery.MAX_QUALIFIED_BUILD = args.build
        server_recovery.QUALIFIED_BUILDS = server_recovery.QUALIFIED_BUILDS | {args.build}
    summary = read(ROOT / f'Builds/NetworkProbe/G3-0.0.{args.build}/build-summary.json')
    contract = summary['contract']
    scenic = contract >= 5
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
        if summary['schema'] >= 16:
            command = runpy.run_path(str(ROOT / 'Tools/Test-HomeWorld.py'))['command']
            for client in clients:
                prior = next(p for p in client.state()['view']['players'] if p['id'] == client.profile)
                require(command(client,0,x=-6590,y=200)['accepted'],'Discovery recovery placement')
                for op in ('cargo-add','red','fill:0:5'):
                    require(command(client,19,item=client.profile,target='0@0',value=op)['accepted'],'Discovery recovery state')
                if summary['schema'] >= 17:
                    # Distinct, settled contents in every variant must survive
                    # byte-exact backup/restore with the coloring and room data.
                    for mode,ingredients in enumerate(((1,2),(5,0),(2,6,8),(4,4,2))):
                        for revision,ingredient in enumerate(ingredients):
                            require(command(client,19,item=client.profile,target=f'mix:{mode}@{revision}',value=f'mix:{mode}:add:{ingredient}',x=1)['accepted'],'Mixing recovery state')
                if summary['schema']>=18:
                    for page in range(6,18):
                        require(command(client,19,item=client.profile,target=f'{page}@0',value='fill:0:3')['accepted'],'Expanded coloring recovery state')
                if summary['schema']>=21:
                    for revision,op in enumerate(['red','yellow','water']):
                        require(command(client,19,item=client.profile,target=f'colors@{revision}',value='liquid:'+op)['accepted'],'Liquid color recovery state')
                if summary['schema']>=20:
                    for revision,op in enumerate(['water','soap','stir','dip','shape']):
                        require(command(client,19,item=client.profile,target=f'bubbles@{revision}',value='bubble:'+op)['accepted'],'Bubble mixture recovery state')
                if summary['schema']>=19:
                    require(command(client,19,item=client.profile,target='ice@0',value='ice:chip',x=316,y=185)['accepted'],'Ice recovery progress')
                require(command(client,0,x=prior['x'],y=prior['y'])['accepted'],'Discovery recovery return')
            time.sleep(.7)
        drop(clients[0], 730, 160)
        if scenic:
            travel = runpy.run_path(str(ROOT / 'Tools/Test-ScenicWorlds.py'))['travel']
            travel(clients[1], 'creek'); travel(clients[2], 'home'); travel(clients[3], 'beach')
        else:
            clients[1].input('button', text='Creek'); wait(lambda: not clients[1].input('inspect')['pending'], 'travel')
        if contract >= 6:
            command = runpy.run_path(str(ROOT / 'Tools/Test-HomeWorld.py'))['command']
            for action, values in [(10,dict(target='shed',value='on')),(2,dict(item='ball-1')),
                (3,dict(item='ball-1',target='shed-2',x=3465,y=160)),(10,dict(target='shed',value='off')),
                (10,dict(target='radio-living',value='on'))]:
                require(command(clients[0],action,**values)['accepted'],'Home recovery fixture command failed')
        if contract >= 8:
            # Prepare the upstairs occupant before tossing: traversal takes longer
            # than one unattended balloon flight, which this fixture must pause.
            require(command(clients[2],0,x=-2980,y=420)['accepted'],'Upstairs recovery entry')
            time.sleep(.5)
            clients[2].input('inspect')
            clients[2].input('touchButton',text='Stair entry')
            wait(lambda:next(p for p in clients[2].state()['view']['players'] if p['id']==clients[2].profile)['zone']=='home-upstairs','Recovery upstairs arrival')
        if contract >= 9:
            require(command(clients[2],0,x=1601,y=420)['accepted'],'Bedroom recovery entry')
            time.sleep(.5);clients[2].input('touchButton',text='Enter bedroom 3')
            wait(lambda:next(p for p in clients[2].state()['view']['players'] if p['id']==clients[2].profile)['zone']=='home-bedroom-3','Recovery bedroom arrival')
        if contract >= 7:
            if contract >= 10:
                room=next(r for r in clients[2].state()['view']['bedrooms'] if r['id']=='home-bedroom-3')
                for action,values in [(14,dict(target='theme',value='3:'+str(room['roomRevision']))),
                    (10,dict(target='bedroom-chest',value='on')),(2,dict(item='home-bedroom-3-toy-0')),
                    (3,dict(item='home-bedroom-3-toy-0',target='home-bedroom-3/chest-0',x=1545,y=300)),
                    (10,dict(target='bedroom-chest',value='off'))]:
                    require(command(clients[2],action,**values)['accepted'],'Furniture recovery fixture failed')
            if contract >= 11:
                require(command(clients[2],0,x=2310,y=220)['accepted'],'Secret recovery approach')
                time.sleep(.8);clients[2].input('touchButton',text='Secret star door')
                wait(lambda:next(p for p in clients[2].state()['view']['players'] if p['id']==clients[2].profile)['zone']=='home-secret-3','Recovery secret arrival')
                time.sleep(.5)
                require(command(clients[2],15,target='active',value='0:2')['accepted'],'Archive occupied secret before recovery')
            require(command(clients[0],5,value='keepy-uppy')['accepted'],'Recovery balloon toss failed')
            require(command(clients[0],7,value='park')['accepted'],'Recovery departure failed')
            if contract < 8:
                require(command(clients[2],7,value='park')['accepted'],'Recovery home pause failed')
        live = recovery.backup(); bundle, files, body = unpack(Path(live['path']))
        require(set(files) == set(FILES) and body['receipts'] and {p['zone'] for p in body['players']} == ({'park', 'creek', 'beach', 'home-secret-3'} if contract >= 11 else {'park', 'creek', 'beach', 'home-bedroom-3'} if contract >= 9 else {'park', 'creek', 'beach', 'home-upstairs'} if contract >= 8 else {'park', 'creek', 'beach'} if contract >= 7 else {'garden', 'creek', 'beach'} if scenic else {'garden', 'creek'}), 'Incomplete backup')
        if scenic: require(body['schema'] == (summary['schema'] if contract >= 8 else 5 if contract >= 7 else 4 if contract >= 6 else 3) and ((body['keepy']['x'] >= 0 if args.build>=131 else body['keepy']['x'] < 0) if contract >= 7 else any(p['x'] < 0 for p in body['players'])), 'Scenic coordinates missing from backup')
        original_instance = server.snapshot()['instanceId']
        refused(lambda: recovery.restore(Path(live['path']), digest(save.read_bytes())))
        require(server.snapshot()['instanceId'] == original_instance and server.snapshot()['players'] == 4, 'Backup/restore attempt interrupted play')
        passed('live backup verifies protected enrollment and full multi-area/four-player checkpoint; active restore refuses without interrupting play')
        fixture.stop(); baseline = recovery.backup(); baseline_path = Path(baseline['path'])
        baseline_bytes = save.read_bytes()
        server.start(); client = fixture.join(1)
        if contract >= 7:travel(client,'garden')
        drop(client, 410, 200); fixture.stop()
        before = saved(); require(before['world.save'] != baseline_bytes, 'Test did not change the world')
        refused(lambda: recovery.restore(baseline_path, 'stale-save-hash'))
        require(saved() == before, 'Stale restore changed files')
        result = recovery.restore(baseline_path, digest(before['world.save']))
        require(save.read_bytes() == baseline_bytes and not recovery.marker.exists(), 'Restore differs')
        server.start()
        require(native_equivalent(checkpoint(save.read_bytes()),checkpoint(baseline_bytes)), 'Native load changed released checkpoint')
        clients = [fixture.join(i) for i in range(1, 5)]
        state = read(fixture.path / server.snapshot()['instanceId'] / 'view.json')
        restored = checkpoint(save.read_bytes()); original = checkpoint(baseline_bytes)
        require(restored['players'] == original['players'] and restored['toys'] == original['toys'] and restored['receipts'] == original['receipts'], 'Native identity/item/receipt restore differs')
        if contract >= 6:
            require(restored['home']==original['home'] and restored['home']['livingRadio'] and next(t for t in restored['toys'] if t['id']=='ball-1')['container']=='shed-2','Home storage/radio recovery differs')
        if contract >= 7:require(native_equivalent(restored['keepy'],original['keepy']) and restored['keepy']['phase']==1,'Paused in-flight balloon recovery differs')
        if contract >= 9:require(restored['bedrooms']==original['bedrooms'] and len(restored['bedrooms'])==4,'Bedroom ownership recovery differs')
        if contract >= 10:require(restored['bedrooms'][2]['theme']==3 and next(t for t in restored['toys'] if t['id']=='home-bedroom-3-toy-0')['container']=='home-bedroom-3/chest-0','Furniture decoration/storage recovery differs')
        if contract >= 11:require(restored['secrets']==original['secrets'] and restored['secrets'][2]['created'] and not restored['secrets'][2]['active'] and len(restored['toys'])==(118 if contract>=17 else 117 if contract>=15 else 64 if contract>=14 else 39 if contract>=13 else 37 if contract>=12 else 33),'Archived secret and plush recovery differs')
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
        v = deepcopy(source); invalid = deepcopy(original); invalid['schema'] = original['schema'] + 1
        payload = json.dumps(invalid); v['files']['world.save'] = encoded(('LITTLEWEEPS-SOLO-1\n' + digest(payload.encode()) + '\n' + payload).encode()); cases.append(('future-schema', v))
        if contract >= 8:
            v = deepcopy(source); v['build'] = 131; cases.append(('schema-newer-than-source-build', v))
        for name, value in cases:
            path = invalid_dir / (name + '.lwbackup'); path.write_text(json.dumps(value), encoding='utf-8')
            refused(lambda: recovery.restore(path, digest(save.read_bytes())))
            require(saved() == before and not recovery.marker.exists(), 'Invalid backup damaged usable state: ' + name)
        truncated = invalid_dir / 'truncated.lwbackup'; truncated.write_bytes(b'{"files":')
        refused(lambda: recovery.restore(truncated, digest(save.read_bytes())))
        passed(f'{len(cases)+1} corrupt, incompatible, wrong-family/enrollment, invalid-state and unsafe-file-name backups refuse before touching usable saves')
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
        logs=list(fixture.path.rglob('player.log'))+list(archive.rglob('player.log'))
        require(all('send queue full' not in p.read_text(errors='replace').lower() for p in logs),'Packet queue overflow during four-player recovery')
        passed('encrypted four-player recovery completes without packet-queue overflow')
        success = True
    finally:
        fixture.cleanup()
        result = dict(passed=success, build=args.build, utc=datetime.now(timezone.utc).isoformat(), checks=checks,
                      scope='Separate enrolled Windows world and four native Windows clients; same-user DPAPI backup/restore. No family world restoration, physical-device, independent-storage or cross-machine recovery claim.')
        write(fixture.path / 'recovery-result.json', result)
        print('Evidence:', fixture.path / 'recovery-result.json', flush=True)


if __name__ == '__main__': main()
