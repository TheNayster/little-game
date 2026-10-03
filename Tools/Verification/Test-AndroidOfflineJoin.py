"""Release-emulator late joining with real NSD/DTLS, UI touches and preserved local play."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,hashlib,json,os,subprocess,time,uuid
from pathlib import Path
from shared_garden_runtime import ROOT,read,write,wait,require


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--family',required=True);p.add_argument('--build',type=int,required=True)
    p.add_argument('--windows-build',type=int,required=True);p.add_argument('--serial',default='emulator-5580')
    args=p.parse_args();require(args.serial.startswith('emulator-'),'Project emulator only')
    root=ROOT/'LocalData/FamilyLAN'/uuid.UUID(args.family).hex;public=read(root/'family.json');require(public,'Existing test enrollment required')
    evidence=ROOT/'LocalData/Verification'/('android-offline-join-'+uuid.uuid4().hex);evidence.mkdir()
    a=[str(Path(os.environ['LOCALAPPDATA'])/'Android/Sdk/platform-tools/adb.exe'),'-s',args.serial]
    package='com.littleweeps.familyplayset';data='/sdcard/Android/data/'+package+'/files';actor=public['profiles'][0]
    def adb(*cmd):return subprocess.check_output(a+list(cmd),stderr=subprocess.PIPE,timeout=45)
    adb('wait-for-device');require('1280x800' in adb('shell','wm','size').decode(),'1280x800 test viewport required')
    apk=ROOT/f'Builds/AndroidSigned/G3-0.0.{args.build}/LittleWeeps.apk';digest=hashlib.sha256(apk.read_bytes()).hexdigest()
    installed=adb('shell','pm','path',package).decode().strip()[8:]
    require(hashlib.sha256(adb('exec-out','cat',installed)).hexdigest()==digest,'Fresh release not installed')
    original=adb('exec-out','cat',data+'/SoloPrototype/family-local/world.save')
    branch=data+'/SoloPrototype/paired-'+root.name+'-'+actor+'/world.save'
    def saved():return adb('exec-out','cat',branch)
    def payload():return json.loads(saved().decode().split('\n',2)[2])
    def android_root():return data+'/FamilyLAN/'+root.name+'/'+adb('shell','cat',data+'/FamilyLAN/'+root.name+'/latest-instance.txt').decode().strip()
    def android_file(name):return json.loads(adb('exec-out','cat',android_root()+'/'+name))
    def status():return android_file('status.json')['status']
    def events(phase):return sum(e['phase']==phase for e in android_file('connection-evidence.json')['events'])
    def tap(x,y):adb('shell','input','tap',str(x),str(y))
    def menu():tap(1140,58)
    def launch():adb('shell','monkey','-p',package,'-c','android.intent.category.LAUNCHER','1')
    def capture(name):(evidence/(name+'.png')).write_bytes(adb('exec-out','screencap','-p'))
    launches=[];checks=[];success=False
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def start(player):
        raw=subprocess.check_output(['uv','run','python','Tools/Start-FamilyLAN.py','--build',str(args.windows_build),'--family',root.name,'--player',str(player),'--hidden'],cwd=ROOT,timeout=70)
        record=json.loads(raw);launches.append(record);return Path(record['output'])
    try:
        require(status() in ('discovering','rediscovering','solo-available'),'Start with the isolated server absent and app in local play')
        pid=adb('shell','pidof',package).decode().strip();tap(715,58)
        wait(lambda:payload()['players'][0]['avatar']=='orange-pup','offline touch saved',10)
        menu();time.sleep(.5);draft=saved();capture('offline-menu')
        passed('release app stays playable after discovery timeout and saves a real offline touch action')
        server=start(0);siblings=[start(i) for i in (2,3,4)]
        def state():return read(server/'view.json')
        wait(lambda:len(state()['connected'])==4 and status()=='connected','Android native late discovery',55)
        require(events('shared-presented')==0 and saved()==draft,'Menu interrupted or local branch modified')
        capture('connected-menu-still-local')
        tap(640,490);wait(lambda:events('shared-presented')==1,'safe shared presentation',15)
        require(adb('shell','pidof',package).decode().strip()==pid and saved()==draft,'Relaunch or draft overwritten')
        capture('shared-after-late-server')
        passed('actual Android NSD finds a server started later; the open menu delays switching and the same process joins on leaving it')

        shared_before=next(v for v in state()['view']['players'] if v['id']==actor).copy()
        menu();tap(640,690);wait(lambda:len(state()['connected'])==3 and events('local-restored')==1,'saved solo restored',15)
        require(saved()==draft,'Returning to solo replaced local work')
        tap(545,58);wait(lambda:payload()['players'][0]['avatar']=='blue-pup','new local action')
        require(next(v for v in state()['view']['players'] if v['id']==actor)['avatar']==shared_before['avatar'],'Local action changed shared world')
        capture('restored-solo')
        adb('shell','input','keyevent','KEYCODE_HOME');time.sleep(2);launch();time.sleep(3)
        require(adb('shell','pidof',package).decode().strip()==pid and len(state()['connected'])==3,'Explicit solo choice lost on background/foreground')
        menu();draft=saved();tap(640,690)
        wait(lambda:len(state()['connected'])==4 and events('shared-presented')==2,'family rejoin from menu',40)
        require(saved()==draft and adb('exec-out','cat',data+'/SoloPrototype/family-local/world.save')==original,'Local saves changed during shared transition')
        passed('menu solo choice restores its own draft and survives real Android background/foreground; family rejoin preserves both saves')

        adb('shell','am','force-stop',package);wait(lambda:len(state()['connected'])==3,'process departure')
        launch();wait(lambda:len(state()['connected'])==4 and events('shared-presented')==1,'cold automatic join',50)
        require(saved()==draft and adb('exec-out','cat',data+'/FamilyLAN/enrollment-status.txt').decode().strip()=='paired','Cold join lost draft or enrollment')
        capture('cold-rejoined')
        passed('cold launch defaults to automatic joining with Keystore enrollment and both saved worlds retained')
        success=True
    finally:
        if launches:
            adb('shell','am','force-stop',package)
            for record in reversed(launches):write(Path(record['output'])/'control.json',dict(serial=999,kind='quit'))
        write(evidence/'result.json',dict(passed=success,androidBuild=args.build,windowsBuild=args.windows_build,apkSha256=digest,checks=checks,
            scope='Android 15/API35 16KB x86_64 emulator with ARM translation and three Windows clients. No physical Android/iPad, host migration or offline merge claim.'))
        write(evidence/'launches.json',launches);print('Evidence: '+str(evidence),flush=True)


if __name__=='__main__':main()
