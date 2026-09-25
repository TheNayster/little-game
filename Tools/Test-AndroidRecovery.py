"""Release Android emulator recovery with three Windows siblings; no physical devices.

Uses ordinary ADB touch/Home/launch actions against the retained project AVD.
Only the historical Android fixture authority may be interrupted. The Android
APK is hash-checked release code without test hooks or modified enrollment.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import time
import uuid

import psutil
from parent_server import ParentServer
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import ROOT, read, write, wait, require

TEST_WORLD = 'd21633f381e74b17a226b083a22ffbaa'
AVD = 'LittleWeeps_G3_AndroidLAN'
PACKAGE = 'com.littleweeps.familyplayset'


def envelope(raw):
    header, checksum, body = raw.split(b'\n', 2)
    require(header == b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode() == checksum,
            'Invalid save envelope')
    return json.loads(body)


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--build', type=int, required=True); p.add_argument('--serial', default='emulator-5580')
    args = p.parse_args(); require(re.fullmatch(r'emulator-\d+', args.serial), 'Emulator only')
    base = [str(Path(os.environ['LOCALAPPDATA']) / 'Android/Sdk/platform-tools/adb.exe'), '-s', args.serial]
    def adb(*cmd): return subprocess.check_output(base + list(cmd), stderr=subprocess.PIPE, timeout=45)
    require(adb('emu', 'avd', 'name').decode().splitlines()[0] == AVD, 'Wrong retained AVD')
    require('1280x800' in adb('shell', 'wm', 'size').decode(), 'Qualified emulator viewport required')
    data = '/sdcard/Android/data/' + PACKAGE + '/files'
    remote = data + '/FamilyLAN/' + TEST_WORLD
    apk = ROOT / f'Builds/AndroidSigned/G3-0.0.{args.build}/LittleWeeps.apk'
    apk_hash = hashlib.sha256(apk.read_bytes()).hexdigest()
    manifest = read(apk.parent / 'artifact-manifest.json')
    entries = manifest if isinstance(manifest, list) else [manifest]
    require(next(v['sha256'] for v in entries if v['path'] == apk.name) == apk_hash, 'APK artifact changed')
    installed = adb('shell', 'pm', 'path', PACKAGE).decode().strip()
    require(re.fullmatch(r'package:/data/app/[a-zA-Z0-9_+=~/\-.]+/base\.apk', installed), 'Unexpected installed package')
    require(hashlib.sha256(adb('exec-out', 'cat', installed[8:])).hexdigest() == apk_hash, 'Intended release not installed')
    f = RecoveryFixture.__new__(RecoveryFixture)
    f.build, f.run_id, f.path = args.build, TEST_WORLD, ROOT / 'LocalData/FamilyLAN' / TEST_WORLD
    prior = read(f.path / 'android-test-launches.json')
    require(len(prior) == 4 and all(v['worldId'] == TEST_WORLD for v in prior), 'Historical isolated Android fixture required')
    public = read(f.path / 'family.json')
    f.players = [dict(profile=v) for v in public['profiles']]; f.instances = []
    f.controller = ParentServer(TEST_WORLD, args.build)
    # This retained fixture predates RecoveryFixture's public purpose marker.
    # The exact AVD/world/history allowlist above identifies it as a test world;
    # use the normal isolated-test controller path without changing family
    # metadata, firewall rules or the production parent startup gate.
    f.controller.isolated = True
    actor = public['profiles'][0]
    evidence = ROOT / 'LocalData/Verification' / ('android-recovery-' + uuid.uuid4().hex); evidence.mkdir()
    checks = []; observations = {}; success = False
    def passed(name): checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    def raw(path): return adb('exec-out', 'cat', path)
    def payload(path): return envelope(raw(path))
    def app_path(): return remote + '/' + raw(remote + '/latest-instance.txt').decode().strip()
    def app_file(name):
        try: return json.loads(raw(app_path() + '/' + name))
        except json.JSONDecodeError: return None
    def events(phase):
        record = app_file('connection-evidence.json') or {}
        return sum(e['phase'] == phase for e in record.get('events', []))
    def state():
        latest = read(f.path / 'latest-server.json')
        return read(f.path / latest['instanceId'] / 'view.json')
    def toy(value, name='bucket-1'): return next(t for t in value['toys'] if t['id'] == name)
    def player(value, who=actor): return next(v for v in value['players'] if v['id'] == who)
    def tap(x,y): adb('shell','input','tap',str(x),str(y))
    def menu(): tap(1140,58)
    def launch(): adb('shell','monkey','-p',PACKAGE,'-c','android.intent.category.LAUNCHER','1')
    def screen(name): (evidence / (name + '.png')).write_bytes(adb('exec-out','screencap','-p'))
    def point(t): return str(round(80 + 1.12*t['x'])), str(round(676-t['y']))
    def drag(value, name, x, y):
        adb('shell','input','swipe',*point(toy(value,name)),str(round(80+1.12*x)),str(round(676-y)),'900')
    def connected(count=4):
        return wait(lambda: state() and len(state()['connected']) == count and
                    (app_file('status.json') or {}).get('status') == 'connected', 'Android and siblings connect', 60)
    replica = remote + '/client-recovery/' + actor + '/world.save'
    adventures = remote + '/client-adventures/' + actor
    def selected(): return payload(adventures + '/selection.save')['branch']
    def branch_path(identity):
        require(uuid.UUID(identity).hex == identity, 'Invalid adventure identity')
        return adventures + '/' + identity + '/world.save'
    def local_new(previous=''):
        def current():
            e = app_file('continuation-evidence.json') or {}
            # A prior foreground transition can leave historical evidence after
            # reunion. Require the currently selected branch, not a stale event.
            return e['branch'] if e.get('status') == 'local-continuation' and e.get('branch') != previous and selected()==e.get('branch') else None
        return wait(current, 'Android automatic local adventure', 65)
    def kill_test_authority():
        native = f.controller.processes()
        require(len(native)==1 and native[0]['output'].is_relative_to(f.path) and native[0]['build']==args.build,
                'Only this fixture authority can be interrupted')
        psutil.Process(native[0]['pid']).terminate()
        wait(lambda:not f.controller.processes(), 'test authority exit')
    try:
        launch(); f.controller.start(); clients=[f.join(i) for i in (2,3,4)]; connected()
        require(raw(data+'/FamilyLAN/enrollment-status.txt').strip()==b'paired', 'Keystore identity lost')
        solo=data+'/SoloPrototype/paired-'+TEST_WORLD+'-'+actor+'/world.save'
        original=data+'/SoloPrototype/family-local/world.save'
        solo_before=raw(solo); original_before=raw(original)
        before=player(state()['view']).copy(); tap(645 if before['x']>550 else 950,470)
        wait(lambda:abs(player(state()['view'])['x']-before['x'])>50,'release touch movement')
        clients[1].input('button',text='Creek')
        wait(lambda:player(state()['view'],clients[1].profile)['zone']=='creek','sibling independent travel')
        # Enough idempotent actions to force a multi-message, receipts-rich replica.
        history=envelope((f.path/'server-world/world.save').read_bytes())['receipts']
        for i in range(max(4,132-len(history))):
            # A UI-control reply only means input was delivered. Wait for the
            # authoritative change before the next toggle, rather than dropping
            # setup clicks against the normal in-flight command guard.
            desired='orange-pup' if player(state()['view'],clients[0].profile)['avatar']=='blue-pup' else 'blue-pup'
            clients[0].input('button',text='Blue pup' if desired=='blue-pup' else 'Orange pup')
            wait(lambda:player(state()['view'],clients[0].profile)['avatar']==desired
                 and not clients[0].input('inspect')['pending'],'accepted receipt setup action',10)
        wait(lambda:(app_file('recovery-evidence.json') or {}).get('receipts')==128,'full Android receipt checkpoint',70)
        drag(state()['view'],'bucket-1',310,180)
        wait(lambda:abs(toy(state()['view'])['x']-310)<2,'ordinary shared Android drag',20)
        wait(lambda:len(payload(replica)['snapshot']['idleTimers'])>0
             and abs(toy(payload(replica)['snapshot'])['x']-310)<2,'active item timer in Android replica',60)
        r=payload(replica)
        require(r['world']==TEST_WORLD and len(r['snapshot']['players'])==4 and len(r['snapshot']['toys'])==10
                and len(r['snapshot']['receipts'])==128 and len(raw(replica))>16384, 'Incomplete checkpoint')
        require({v['zone'] for v in r['snapshot']['players']}=={'garden','creek'}, 'Offscreen area missing')
        observations['replica']=app_file('recovery-evidence.json');screen('four-player-checkpoint')
        passed('retained Keystore identity joins three Windows siblings; ordinary walking/dragging works and Android durably stores both areas, four players, 128 receipts and an active item timer')

        pid=adb('shell','pidof',PACKAGE).decode().strip(); epoch=state()['epoch']
        adb('shell','input','keyevent','KEYCODE_HOME')
        wait(lambda:len(state()['connected'])==3,'Android actual background detach',25)
        desired='orange-pup' if player(state()['view'],clients[0].profile)['avatar']=='blue-pup' else 'blue-pup'
        clients[0].input('button',text='Orange pup' if desired=='orange-pup' else 'Blue pup')
        wait(lambda:player(state()['view'],clients[0].profile)['avatar']==desired,'sibling play during background')
        require(state()['epoch']==epoch,'Background restarted server')
        launch();connected()
        require(adb('shell','pidof',PACKAGE).decode().strip()==pid,'Expected warm resume')
        wait(lambda:player(payload(replica)['snapshot'],clients[0].profile)['avatar']==desired,'fresh checkpoint after resume',60)
        screen('warm-resume')
        passed('real Android Home/foreground cycle preserves the process and enrollment; three siblings continue and resumed checkpoint includes their changes')

        clients[1].input('press',role='sponge-creek')
        wait(lambda:toy(payload(replica)['snapshot'],'sponge-creek')['holder']==clients[1].profile,'replicated remote hold',55)
        kill_test_authority(); lost=time.monotonic(); clients[1].input('release',role='sponge-creek')
        branch=local_new(); observations['outageToLocalSeconds']=round(time.monotonic()-lost,3)
        path=branch_path(branch); record=payload(path)
        require(record['actor']==actor and record['origin']['world']==TEST_WORLD and selected()==branch
                and all(not v['holder'] for v in record['snapshot']['toys']), 'Wrong recovery lineage or stuck hold')
        drag(record['snapshot'],'bucket-1',730,210)
        wait(lambda:abs(toy(payload(path)['snapshot'])['x']-730)<2,'Android local drop saves',20)
        screen('local-outage-play')
        require(raw(original)==original_before and raw(solo)==solo_before,'Outage changed existing solo saves')
        passed('abrupt fixture-authority loss automatically opens an Android local adventure from its received view, releases stale holds and saves ordinary drag input')

        adb('shell','am','force-stop',PACKAGE); launch()
        wait(lambda: (app_file('status.json') or {}).get('pid')!=int(pid),'new Android process',30)
        wait(lambda:selected()==branch,'same selected adventure after cold launch',20)
        # A changed save caused by a normal touch proves the reopened scene uses
        # the adventure, not merely that an old file still exists on disk.
        desired='orange-pup' if player(payload(path)['snapshot'])['avatar']=='blue-pup' else 'blue-pup'
        tap(715 if desired=='orange-pup' else 545,58)
        wait(lambda:player(payload(path)['snapshot'])['avatar']==desired,'cold adventure responds to touch',20)
        require(abs(toy(payload(path)['snapshot'])['x']-730)<2,'Cold restart lost local drop')
        screen('cold-offline-resume')
        passed('force-stop and cold offline launch reopen the same changed adventure; existing toy placement and new touch edits survive')

        menu();screen('menu-before-reunion'); before_presented=events('shared-presented')
        basis=payload(path)['origin']; f.controller.start(); connected()
        require(events('shared-presented')==before_presented,'Reunion interrupted open menu')
        tap(640,490);wait(lambda:events('shared-presented')>before_presented,'safe family presentation',25)
        require(payload(path)['origin']==basis and abs(toy(payload(path)['snapshot'])['x']-730)<2,'Reunion overwrote adventure')
        require(abs(toy(state()['view'])['x']-730)>2,'Local drop silently replaced authority item')
        screen('safe-family-reunion')
        menu();tap(860,690);screen('saved-adventures-menu');tap(640,295)
        wait(lambda:len(state()['connected'])==3 and selected()==branch,'saved adventure opens without siblings leaving',25)
        drag(payload(path)['snapshot'],'bucket-1',650,230)
        wait(lambda:abs(toy(payload(path)['snapshot'])['x']-650)<2,'reopened adventure saves touch',20)
        require(all(c.input('inspect')['shared'] for c in clients),'Adventure selection interrupted siblings')
        menu();tap(860,690);tap(640,605)
        wait(lambda:selected()=='','original solo selection',15)
        require(raw(solo)==solo_before and raw(original)==original_before,'Original solo progress lost')
        menu();tap(420,690);connected()
        retained=raw(path); previous_epoch=state()['epoch']
        wait(lambda:payload(replica)['epoch']==previous_epoch,'checkpoint from recovered authority',60)
        passed('reunion waits for the open menu, preserves local changes separately, and saved-adventure/original-solo controls work while siblings continue')

        kill_test_authority(); again=local_new(previous=branch)
        require(again!=branch and raw(path)==retained and payload(branch_path(again))['origin']['epoch']==previous_epoch,
                'Second outage replaced earlier work or used wrong base')
        screen('second-distinct-adventure')
        passed('another outage creates a distinct adventure from the newer authority while keeping the earlier adventure byte for byte')
        success=True
    finally:
        try:
            screen('final' if success else 'failure')
            pid=adb('shell','pidof',PACKAGE).decode().strip()
            if pid: (evidence/'android-process.log').write_bytes(adb('logcat','-d','--pid='+pid))
            adb('pull',remote+'/client-adventures',str(evidence/'adventures'))
            adb('pull',remote+'/client-recovery',str(evidence/'recovery'))
        finally:
            adb('shell','am','force-stop',PACKAGE); f.cleanup()
            write(evidence/'result.json',dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),
                  apkSha256=apk_hash,checks=checks,observations=observations,
                  scope='Retained Android API35 16 KB x86_64 AVD using ARM translation and three native Windows clients. Isolated test authority only; no physical device, native ARM64 16 KB, iPad hosting, automatic branch merge or live family deployment qualification.'))
            print('Evidence:',evidence,flush=True)


if __name__=='__main__':main()
