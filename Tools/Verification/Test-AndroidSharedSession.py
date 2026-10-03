"""Qualify an already enrolled release APK with three isolated Windows test clients.

Run only on the project-owned 1280x800 emulator after launching an isolated family.
No debug hooks, packet relay, or test credentials are inserted into the Android APK.
ADB supplies ordinary touch/Home/launch input; server observations verify outcomes.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, hashlib, json, os, re, subprocess, time, uuid
from pathlib import Path
from shared_garden_runtime import ROOT,read,write,wait,require

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--serial',required=True);p.add_argument('--family',required=True);p.add_argument('--build',type=int,required=True)
    args=p.parse_args();require(re.fullmatch(r'emulator-\d+',args.serial),'Emulator only')
    root=ROOT/'LocalData/FamilyLAN'/uuid.UUID(args.family).hex;public=read(root/'family.json')
    launches=read(root/'android-test-launches.json');server=Path(launches[0]['output']);others=[Path(x['output']) for x in launches[1:]]
    require(len(others)==3 and all(x.is_relative_to(root) for x in [server,*others]),'Isolated family outputs required')
    evidence=ROOT/'LocalData/Verification'/('android-shared-'+uuid.uuid4().hex);evidence.mkdir()
    package='com.littleweeps.familyplayset';base=[str(Path(os.environ['LOCALAPPDATA'])/'Android/Sdk/platform-tools/adb.exe'),'-s',args.serial]
    def adb(*command):return subprocess.check_output(base+list(command),stderr=subprocess.PIPE,timeout=40)
    adb('wait-for-device');require('1280x800' in adb('shell','wm','size').decode(),'Test touch coordinates require the qualified emulator viewport')
    apk=ROOT/f'Builds/AndroidSigned/G3-0.0.{args.build}/LittleWeeps.apk';installed=adb('shell','pm','path',package).decode().strip()[8:]
    apk_hash=hashlib.sha256(apk.read_bytes()).hexdigest();require(hashlib.sha256(adb('exec-out','cat',installed)).hexdigest()==apk_hash,'Exact installed artifact required')
    data='/sdcard/Android/data/'+package+'/files';old_solo=adb('exec-out','cat',data+'/SoloPrototype/family-local/world.save')
    cases=[];result=dict(androidBuild=args.build,windowsBuild=launches[0]['build'],apkSha256=apk_hash,scope='Android 15 emulator ARM translation plus three native Windows clients, direct NSD/DTLS to PC; no relay',checks=cases)
    def passed(name):cases.append(dict(name=name,passed=True));write(evidence/'result.json',result);print('PASS '+name,flush=True)
    def state():return read(server/'view.json')
    actor=public['profiles'][0]
    def player(which=actor):return next(p for p in state()['view']['players'] if p['id']==which)
    def toy(name):return next(t for t in state()['view']['toys'] if t['id']==name)
    def point(value):return str(round(80+1.12*value['x'])),str(round(676-value['y']))
    def snap(name):(evidence/(name+'.png')).write_bytes(adb('exec-out','screencap','-p'))
    def launch():adb('shell','monkey','-p',package,'-c','android.intent.category.LAUNCHER','1')
    def android_path():return data+'/FamilyLAN/'+root.name+'/'+adb('shell','cat',data+'/FamilyLAN/'+root.name+'/latest-instance.txt').decode().strip()
    try:
        wait(lambda:len(state()['connected'])==4,'four current players')
        status=json.loads(adb('exec-out','cat',android_path()+'/status.json'));require(status['status']=='connected','Android has not received its snapshot')
        passed('Release Android client discovers the PC and joins three Windows players over enrolled DTLS')
        before=player().copy();adb('shell','input','tap','650','438')
        wait(lambda:abs(player()['x']-before['x'])>60,'Android touch movement')
        time.sleep(2)
        wait(lambda:all(abs(next(p for p in read(x/'view.json')['view']['players'] if p['id']==actor)['x']-player()['x'])<5 for x in others),'Windows clients see Android movement')
        passed('Ordinary Android touch movement reaches the server and all three Windows views')
        start=point(toy('bucket-1'));end=point(toy('tap-1'));adb('shell','input','swipe',*start,*end,'1000')
        wait(lambda:toy('bucket-1')['water']>0,'Android fill bucket')
        time.sleep(.5);start=point(toy('bucket-1'));end=point(toy('plant-1'));adb('shell','input','swipe',*start,*end,'1000')
        wait(lambda:toy('plant-1')['water']>0 and toy('bucket-1')['holder']=='','Android waters plant')
        wait(lambda:all(next(t for t in read(x/'view.json')['view']['toys'] if t['id']=='plant-1')['water']==toy('plant-1')['water'] for x in others),'Shared watering state')
        passed('Dragging on Android fills and pours the single shared bucket; all clients see the result')
        snap('four-player-garden')
        adb('shell','input','tap','1145','137');wait(lambda:player()['zone']=='creek','Android travels to Creek')
        require(all(player(p)['zone']=='garden' for p in public['profiles'][1:]),'Android travel moved a sibling')
        snap('independent-creek');adb('shell','input','tap','957','137');wait(lambda:player()['zone']=='garden','Android returns')
        passed('Android travels to Creek and back while the three other players stay in Garden')
        pid=adb('shell','pidof',package).decode().strip();epoch=state()['epoch']
        adb('shell','input','keyevent','KEYCODE_HOME');wait(lambda:len(state()['connected'])==3,'Real Android activity pause')
        sibling=public['profiles'][1];current=state();person=next(p for p in current['view']['players'] if p['id']==sibling)
        request=uuid.uuid4().hex
        write(others[0]/'control.json',dict(serial=1,kind='command',request=dict(requestId=request,protocol=3,command=dict(requestId=request,actor=sibling,expectedRevision=current['view']['revision'],action=1,value='orange-pup',zone=person['zone'],visit=person['visit']))))
        wait(lambda:player(sibling)['avatar']=='orange-pup','Sibling action while Android is paused')
        require(state()['epoch']==epoch,'Pause restarted the authority');passed('Backgrounding Android detaches only that player; a Windows sibling keeps changing the world')
        launch();wait(lambda:len(state()['connected'])==4,'Android foreground auto-rejoin',40)
        wait(lambda:json.loads(adb('exec-out','cat',android_path()+'/status.json'))['status']=='connected','Android fresh snapshot')
        require(adb('shell','pidof',package).decode().strip()==pid,'Resume relaunched the app instead of reconnecting')
        view=json.loads(adb('exec-out','cat',android_path()+'/view.json'))
        require(next(p for p in view['view']['players'] if p['id']==sibling)['avatar']=='orange-pup','Resumed view is stale')
        passed('The same Android process automatically rejoins and receives the sibling changes')
        adb('shell','am','force-stop',package);wait(lambda:len(state()['connected'])==3,'Android process loss')
        launch();wait(lambda:len(state()['connected'])==4,'Cold relaunch with persisted enrollment',40)
        require(adb('shell','cat',data+'/FamilyLAN/enrollment-status.txt').decode().strip()=='paired','Pairing lost')
        require(adb('exec-out','cat',data+'/SoloPrototype/family-local/world.save')==old_solo,'Shared play modified the earlier solo save')
        passed('Force-stop/relaunch retains Keystore pairing, re-enters the same world, and preserves the original solo save')
        time.sleep(1);snap('rejoined');result['passed']=True
        (evidence/'server-final.json').write_text(json.dumps(state(),indent=2))
        (evidence/'android-connection.json').write_bytes(adb('exec-out','cat',android_path()+'/connection-evidence.json'))
        current_pid=adb('shell','pidof',package).decode().strip()
        (evidence/'android-current-process.log').write_bytes(adb('logcat','-d','--pid='+current_pid))
    except Exception as e:result['passed']=False;result['failure']=str(e);snap('failure');raise
    finally:write(evidence/'result.json',result);print('Evidence: '+str(evidence),flush=True)

if __name__=='__main__':main()
