# /// script
# dependencies = ["cryptography"]
# ///
"""Unserved disposable enrollment: native solo dinosaur rescue, melting and reopen."""
import argparse, json, subprocess, time, uuid
from pathlib import Path
from family_pairing import ROOT, create_family, write_record
from shared_garden_runtime import Instance, read, write, wait, require

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}'
    summary=read(folder/'build-summary.json');require(summary['schema'] ==19,'Wrong rescue candidate')
    authority,players,_=create_family()
    class Run:pass
    run=Run();run.build=args.build;run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True)
    active=[];checks=[];passed=False;print('EVIDENCE '+str(run.path),flush=True)
    def start():
        record=players[0];v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=record['profile'];v.serial=0;v.garden_serial=0
        v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
        pair=run.path/(v.identity+'.pairing');write_record(pair,record)
        cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=3,content=summary['content'],pairingPath=str(pair),presentation=True,verifyGarden=True)
        config=run.path/(v.identity+'.config.json');write(config,cfg)
        command=[str(folder/'Client/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591']
        startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
        v.process=subprocess.Popen(command,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW);active.append(v)
        wait(lambda:v.status(),'solo status',30);wait(lambda:v.input('inspect')['ready'],'solo ready');return v
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        a=start();require(not a.input('inspect')['shared'],'Unexpected family join')
        save=Path(a.input('inspect')['savePath'])
        def world():
            for attempt in range(6):
                try:return json.loads(save.read_bytes().split(b'\n',2)[2])
                except PermissionError:
                    if attempt==5:raise
                    time.sleep(.025)
        def tray():return world()['discovery'][0]['ice'][0]
        def button(name):a.input('touchButton',text=name);time.sleep(.16)
        if not a.input('inspect')['joystickVisible']:button('Tap to walk')
        a.input('touch-begin',role='stick',x=-65,finger=73)
        wait(lambda:world()['players'][0]['x']<-6400,'walk to science in private solo',seconds=40)
        a.input('touch-end',role='stick',x=-65,finger=73);time.sleep(.7)
        button('Science bench');button('Dinosaur rescue');time.sleep(1)
        button('Ice Chip');wait(lambda:tray()['current']['cells'][0]<1,'saved private hammer tap')
        button('Ice More');button('Ice Sound');button('Ice Calm');button('Ice Done')
        button('Ice Water');wait(lambda:any(tray()['current']['energy']),'saved water')
        before=tray()['current']['cells'][0];button('Floating boats')
        wait(lambda:tray()['current']['cells'][0]<before-.1,'solo melt under another activity')
        wait(lambda:not any(tray()['current']['energy']),'solo water settles',seconds=15)
        expected=tray();button('Back to Home');a.close();a=start()
        button('Science bench');button('Dinosaur rescue');time.sleep(.5)
        require(tray()==expected,'Solo reopen changed rescue')
        button('Ice More');info=a.input('inspect')
        require('Sound off' in info['visibleText'] and 'Calm effects' in info['visibleText'],'Local preferences missing')
        record('private rescue saves hammer and melting progress and reopens with local sound and calm preferences')
        for v in active:require('Exception:' not in (v.out/'player.log').read_text(errors='replace'),'Native solo exception')
        passed=True
    finally:
        for v in active:v.close()
        write(run.path/'ice-rescue-solo-results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(run.path/'ice-rescue-solo-results.json'),flush=True)

if __name__=='__main__':main()
