"""Ten-minute release Android/Windows play with normal clocks and retained data.

Only the historical project AVD/test family is allowed. No physical-device,
native ARM64, impaired-Wi-Fi or iPad-performance claim follows from this run.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import importlib.util
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

WORLD = 'd21633f381e74b17a226b083a22ffbaa'
AVD = 'LittleWeeps_G3_AndroidLAN'
PACKAGE = 'com.littleweeps.familyplayset'


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build',type=int,required=True)
    parser.add_argument('--seconds',type=int,default=600)
    parser.add_argument('--serial',default='emulator-5580')
    args=parser.parse_args()
    require(600<=args.seconds<=1800 and re.fullmatch(r'emulator-\d+',args.serial),'Bounded emulator run required')
    base=[str(Path(os.environ['LOCALAPPDATA'])/'Android/Sdk/platform-tools/adb.exe'),'-s',args.serial]
    def adb(*a): return subprocess.check_output(base+list(a),stderr=subprocess.PIPE,timeout=45)
    require(adb('emu','avd','name').decode().splitlines()[0]==AVD,'Wrong project AVD')
    require(adb('shell','getprop','sys.boot_completed').strip()==b'1' and '1280x800' in adb('shell','wm','size').decode(),'Qualified AVD not ready')
    apk=ROOT/f'Builds/AndroidSigned/G3-0.0.{args.build}/LittleWeeps.apk'
    apk_hash=hashlib.sha256(apk.read_bytes()).hexdigest();manifest=read(apk.parent/'artifact-manifest.json')
    require(next(e['sha256'] for e in (manifest if isinstance(manifest,list) else [manifest]) if e['path']==apk.name)==apk_hash,'APK changed')
    installed=adb('shell','pm','path',PACKAGE).decode().strip()
    require(re.fullmatch(r'package:/data/app/[a-zA-Z0-9_+=~/\-.]+/base\.apk',installed),'Unexpected installed APK')
    require(hashlib.sha256(adb('exec-out','cat',installed[8:])).hexdigest()==apk_hash,'Wrong installed release')
    print('SETUP: installed release hash verified.',flush=True)
    f=RecoveryFixture.__new__(RecoveryFixture)
    f.build,f.run_id,f.path=args.build,WORLD,ROOT/'LocalData/FamilyLAN'/WORLD
    prior=read(f.path/'android-test-launches.json')
    require(len(prior)==4 and all(p['worldId']==WORLD for p in prior),'Historical test-world allowlist failed')
    public=read(f.path/'family.json');f.players=[dict(profile=p) for p in public['profiles']];f.instances=[]
    f.controller=ParentServer(WORLD,args.build)
    # This exact legacy AVD/family predates the newer isolated-purpose marker.
    # Use the existing isolated controller path without modifying public metadata.
    f.controller.isolated=True;actor=public['profiles'][0]
    data='/sdcard/Android/data/'+PACKAGE+'/files';remote=data+'/FamilyLAN/'+WORLD
    replica=remote+'/client-recovery/'+actor+'/world.save'
    evidence=ROOT/'LocalData/Verification'/('android-soak-'+uuid.uuid4().hex);evidence.mkdir()
    spec=importlib.util.spec_from_file_location('smooth',ROOT/'Tools/Test-SmoothMovement.py')
    smooth=importlib.util.module_from_spec(spec);spec.loader.exec_module(smooth)
    checks=[];samples=[];android_memory=[];traces=[];success=False;clients=[]
    def passed(name): checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def raw(path): return adb('exec-out','cat',path)
    def decode(value):
        header,digest,body=value.split(b'\n',2)
        require(header==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode()==digest,'Invalid durable envelope')
        return json.loads(body)
    def envelope(path): return decode(raw(path))
    def android_file(name):
        instance=raw(remote+'/latest-instance.txt').decode().strip()
        return json.loads(raw(remote+'/'+instance+'/'+name))
    def state(): return read(out/'view.json')
    def toy(view,name='bucket-1'): return next(t for t in view['toys'] if t['id']==name)
    def player(view,profile): return next(p for p in view['players'] if p['id']==profile)
    def walk(c,x,y): c.input('press',x=x,y=y);c.input('release',x=x,y=y)
    def drag(c,x,y):
        c.input('press',role='bucket-1');c.input('release',x=x,y=y)
        wait(lambda:not c.input('inspect')['pending'],'accepted drag')
    def launch(): adb('shell','monkey','-p',PACKAGE,'-c','android.intent.category.LAUNCHER','1')
    def connected():
        return wait(lambda:len(state()['connected'])==4 and android_file('status.json')['status']=='connected', 'all four connected',60)
    def trace():
        for c in clients[:2]:
            if player(state()['view'],c.profile)['zone']!='garden': c.input('button',text='Garden')
        wait(lambda:all(player(state()['view'],c.profile)['zone']=='garden' for c in clients[:2]),'trace area')
        walk(clients[0],50,70)
        wait(lambda:abs(player(state()['view'],clients[0].profile)['x']-50)<1,'trace start')
        time.sleep(.6);clients[1].input('traceStart',role=clients[0].profile)
        walk(clients[0],920,70);time.sleep(4.8);clients[1].input('traceStop')
        return smooth.analyze(read(clients[1].out/'motion-trace.json')['samples'])
    # Initial offline presentation can legitimately advance a selected local
    # adventure's timers before family admission. Keep both snapshots, but test
    # byte-preservation from confirmed shared presentation, not cold launch.
    local_paths=[data+'/SoloPrototype/family-local/world.save',data+'/SoloPrototype/paired-'+WORLD+'-'+actor+'/world.save']
    adventures=remote+'/client-adventures/'+actor
    found=adb('shell','find',adventures,'-name','world.save').decode().splitlines()
    require(found and all(p.startswith(adventures+'/') and p.endswith('/world.save') for p in found),'Retained adventures missing')
    local_paths+=found
    def capture_local(stage):
        records={}
        for i,p in enumerate(local_paths):
            value=raw(p);decode(value)
            (evidence/f'{stage}-local-{i}.save').write_bytes(value)
            records[p]=hashlib.sha256(value).hexdigest()
        write(evidence/(stage+'-local-hashes.json'),records)
        return records
    startup_hashes=capture_local('startup');before_hashes={};startup_changes=[];error=None
    previous_instance=raw(remote+'/latest-instance.txt').decode().strip()
    try:
        f.controller.start();native=f.controller.processes()[0];out=native['output']
        launch();clients=[f.join(i) for i in (2,3,4)];connected()
        wait(lambda:raw(remote+'/latest-instance.txt').decode().strip()!=previous_instance and
             any(e['phase']=='shared-presented' for e in android_file('connection-evidence.json').get('events',[])),
             'fresh Android shared presentation',60)
        before_hashes=capture_local('joined')
        for i,p in enumerate(local_paths):
            if startup_hashes[p]==before_hashes[p]:continue
            a=decode((evidence/f'startup-local-{i}.save').read_bytes())
            b=decode((evidence/f'joined-local-{i}.save').read_bytes())
            fields=[]
            for key in a.keys()|b.keys():
                if a.get(key)==b.get(key):continue
                if key=='snapshot':fields.extend('snapshot.'+k for k in a[key].keys()|b[key].keys() if a[key].get(k)!=b[key].get(k))
                else:fields.append(key)
            startup_changes.append(dict(localIndex=i,changedFields=sorted(fields)))
        print('SETUP: Android and three Windows clients connected.',flush=True)
        require(android_file('status.json')['build']==f'0.0.{args.build}','Runtime version differs')
        pid=adb('shell','pidof',PACKAGE).decode().strip()
        # Confirm fresh actions; retained full history keeps recovery multi-message.
        for _ in range(4):
            target='orange-pup' if player(state()['view'],clients[0].profile)['avatar']=='blue-pup' else 'blue-pup'
            clients[0].input('button',text='Orange pup' if target=='orange-pup' else 'Blue pup')
            wait(lambda:player(state()['view'],clients[0].profile)['avatar']==target and not clients[0].input('inspect')['pending'],'receipt setup')
        wait(lambda:android_file('recovery-evidence.json').get('receipts')==128,'full Android replica',60)
        traces.append(trace())
        print('SETUP: baseline motion passed.',flush=True)
        drag(clients[0],150,340);drag(clients[0],810,330)
        wait(lambda:toy(state()['view'],'plant-1')['water']==3,'full flower')
        planted=time.monotonic();drag(clients[0],710,210);borrowed=time.monotonic()
        started=time.monotonic();cycle=0;next_report=60;next_memory=0
        print('START: ten-minute normal-clock run.',flush=True)
        flower_at=bucket_at=None;rejoined=False;last_durable=-1;last_durable_at=started
        checkpoint_ids=set();gaps=[]
        while time.monotonic()-started<args.seconds:
            loop=time.monotonic();elapsed=loop-started;x=900 if cycle%2==0 else 80
            for i,c in enumerate(clients):walk(c,x,70+35*i)
            adb('shell','input','tap',str(round(80+1.12*x)),str(676-175))
            if cycle%5==2:
                c=clients[2];zone=player(state()['view'],c.profile)['zone']
                c.input('button',text='Creek' if zone=='garden' else 'Garden')
                wait(lambda:player(state()['view'],c.profile)['zone']!=zone,'independent travel')
            if elapsed>=240 and not rejoined:
                adb('shell','input','keyevent','KEYCODE_HOME')
                wait(lambda:len(state()['connected'])==3,'Android background departure',25)
                require(all(c.input('inspect')['connected'] for c in clients),'Background interrupted siblings')
                clients[0].input('button',text='Orange pup' if player(state()['view'],clients[0].profile)['avatar']=='blue-pup' else 'Blue pup')
                time.sleep(2);launch();connected()
                require(adb('shell','pidof',PACKAGE).decode().strip()==pid,'Unexpected cold restart')
                rejoined=True
            view=state()['view'];connection=read(out/'connection-evidence.json')
            require(connection['listening'] and connection['receiveError']==0 and len(state()['connected'])==4,'Authority/transport lost play')
            require(all(c.input('inspect')['connected'] for c in clients) and android_file('status.json')['status']=='connected','Unexpected departure')
            recovery=android_file('recovery-evidence.json')
            require(recovery['status']=='durable' and recovery['receipts']==128 and recovery['bytes']>16384,'Android replica invalid')
            now=time.monotonic();cid=recovery['checkpoint'];checkpoint_ids.add(cid)
            if cid!=last_durable:
                if last_durable!=-1:gaps.append(round(now-last_durable_at,3))
                last_durable_at=now;last_durable=cid
            require(now-last_durable_at<60,'Durable Android checkpoint stalled for one minute')
            if flower_at is None and toy(view,'plant-1')['water']==0:flower_at=round(now-planted,2)
            if bucket_at is None and abs(toy(view)['x']-360)<1 and abs(toy(view)['y']-130)<1:bucket_at=round(now-borrowed,2)
            if elapsed>=next_memory:
                mem=adb('shell','dumpsys','meminfo',PACKAGE).decode()
                pss=re.search(r'TOTAL PSS:\s+(\d+)',mem)
                require(pss is not None,'Android memory report unavailable')
                android_memory.append(dict(seconds=round(elapsed,2),pssMiB=round(int(pss[1])/1024,2)))
                next_memory=elapsed+30
            memory=[]
            for label,process in [('server',psutil.Process(native['pid']))]+[(f'windows-{i+1}',psutil.Process(c.process.pid)) for i,c in enumerate(clients)]:
                info=process.memory_info();memory.append(dict(role=label,privateMiB=round(info.private/1048576,2)))
            samples.append(dict(seconds=round(elapsed,2),revision=view['revision'],androidX=player(view,actor)['x'],
                recovery=recovery,memory=memory,serverMaxFrameGapSeconds=connection['maxFrameGap'],
                serverMotion=read(out/'motion-stats.json'),receivedDataBytes=connection['receivedDataBytes']))
            if elapsed>=next_report:
                print(f'PROGRESS {int(elapsed)}s: four connected; Android checkpoint {cid}; flower={flower_at is not None}, bucket={bucket_at is not None}',flush=True)
                next_report+=60
            cycle+=1;time.sleep(max(0,8-(time.monotonic()-loop)))
        measured=round(time.monotonic()-started,2)
        require(measured>=args.seconds and flower_at and bucket_at and rejoined,'Required sustained activities missing')
        require(max(s['androidX'] for s in samples)-min(s['androidX'] for s in samples)>500,'Android touch did not move through the shared world')
        require(len(checkpoint_ids)>=20,'Insufficient repeated durable recovery transfers')
        require(f.controller.snapshot()['instanceId']==native['instanceId'],'Authority replaced during play')
        passed('ten real minutes of four-player movement and independent travel, plus actual Android Home/warm return, preserve the same authority and active siblings')
        passed('normal flower and borrowed-bucket timers complete while Android repeatedly commits full 128-receipt recovery checkpoints')
        traces.append(trace());passed('remote Windows canvas movement passes existing interpolation thresholds before and after the mixed-platform run')
        memory_summary={}
        for role in ('server','windows-1','windows-2','windows-3'):
            values=[next(m['privateMiB'] for m in s['memory'] if m['role']==role) for s in samples]
            warm=[next(m['privateMiB'] for m in s['memory'] if m['role']==role) for s in samples if s['seconds']>=60]
            memory_summary[role]=dict(firstMiB=values[0],lastMiB=values[-1],peakMiB=max(values),growthAfterMinuteMiB=round(warm[-1]-warm[0],2))
            require(max(values)<1024 and warm[-1]-warm[0]<256,'Desktop memory smoke budget exceeded')
        warm=[m['pssMiB'] for m in android_memory if m['seconds']>=60]
        memory_summary['android-emulator']=dict(firstPssMiB=android_memory[0]['pssMiB'],lastPssMiB=android_memory[-1]['pssMiB'],peakPssMiB=max(m['pssMiB'] for m in android_memory),growthAfterMinuteMiB=round(warm[-1]-warm[0],2))
        require(memory_summary['android-emulator']['peakPssMiB']<1024 and warm[-1]-warm[0]<256,'Emulator memory smoke budget exceeded')
        passed('desktop private memory and emulated Android PSS remain inside explicit 1 GiB / 256 MiB growth smoke limits; these are not A10 budgets')
        # Settle movement and wait for a durable replica of the final gameplay revision.
        time.sleep(6);revision=state()['view']['revision']
        wait(lambda:android_file('recovery-evidence.json')['revision']>=revision,'final durable Android revision',60)
        record=envelope(replica)
        require(record['snapshot']['revision']>=revision and len(record['snapshot']['receipts'])==128,'Incomplete final durable world')
        after_hashes=capture_local('final')
        require(after_hashes==before_hashes,'Shared run changed retained solo or adventures after confirmed admission')
        (evidence/'android-final.png').write_bytes(adb('exec-out','screencap','-p'))
        log=adb('logcat','-d','--pid='+pid).decode(errors='replace');(evidence/'android-process.log').write_text(log,encoding='utf-8')
        require(not re.search(r'FATAL EXCEPTION|Fatal signal|SIGSEGV|OutOfMemory|ANR in',log),'Selected Android failure signature found')
        adb('shell','am','force-stop',PACKAGE);f.stop()
        before=(f.path/'server-world/world.save').read_bytes();f.controller.start()
        require((f.path/'server-world/world.save').read_bytes()==before,'Empty restart changed stopped checkpoint')
        f.stop();passed('final full replica covers completed play; solo/adventures remain byte-identical since shared admission and stopped authority save survives restart exactly')
        success=True
    except Exception as e:
        error=type(e).__name__+': '+str(e);raise
    finally:
        adb('shell','am','force-stop',PACKAGE);f.cleanup()
        write(evidence/'samples.json',samples);write(evidence/'android-memory.json',android_memory)
        write(evidence/'result.json',dict(passed=success,build=args.build,apkSha256=apk_hash,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
            requestedSeconds=args.seconds,measuredSeconds=locals().get('measured'),cycles=locals().get('cycle',0),
            flowerResetSeconds=locals().get('flower_at'),bucketReturnSeconds=locals().get('bucket_at'),
            durableAndroidCheckpoints=len(locals().get('checkpoint_ids',set())),maxObservedCheckpointGapSeconds=max(locals().get('gaps',[0]) or [0]),
            motionTraces=traces,memory=locals().get('memory_summary',{}),retainedLocalSaves=len(before_hashes),
            startupLocalChanges=startup_changes,localPreservationBaseline='fresh confirmed shared presentation',error=error,
            scope='One Windows PC; Android API35 16 KB x86_64 emulator with ARM translation plus three native Windows clients. No impaired Wi-Fi, physical input/audio/thermal/A10 frame-time, native ARM64 16 KB, iPad host or two-hour uptime qualification.'))
        print('Evidence:',evidence,flush=True)


if __name__=='__main__':main()
