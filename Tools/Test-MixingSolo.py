# /// script
# dependencies = ["cryptography"]
# ///
"""Unserved disposable enrollment: native solo chemistry, other activity and reopen."""
import argparse, json, subprocess, time, uuid
from pathlib import Path
from family_pairing import ROOT, create_family, write_record
from shared_garden_runtime import Instance, read, write, wait, require

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}'
    summary=read(folder/'build-summary.json');require(summary['schema'] in (17,18),'Wrong mixing candidate')
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
        def tray(mode=0):return world()['discovery'][0]['mixtures'][mode]
        def button(name):a.input('touchButton',text=name);time.sleep(.16)
        def supply(slot):
            x=-430 if slot<3 else 430;y=145-slot%3*148
            a.input('touch-begin',role='mixing',x=x,y=y,finger=74);a.input('touch-end',role='mixing',x=x,y=y,finger=74)
        if not a.input('inspect')['joystickVisible']:button('Tap to walk')
        a.input('touch-begin',role='stick',x=-65,finger=73)
        wait(lambda:world()['players'][0]['x']<-6400,'walk to science in private solo',seconds=40)
        a.input('touch-end',role='stick',x=-65,finger=73);time.sleep(.7)
        button('Science bench');button('Mix & discover')
        supply(1);supply(1);supply(1);supply(3);supply(0);supply(0);supply(0)
        wait(lambda:tray()['reacted']==3 and tray()['foam']>0,'saved private reaction')
        record('unserved installed-style launch supports walking and mixing with private saves')
        before=tray()['reaction'];button('Floating boats')
        wait(lambda:tray()['reaction']<before-1,'chemistry advances under another activity',seconds=6)
        button('Mix & discover');button('Sound on');button('Gentle motion')
        wait(lambda:tray()['reaction']==0 and tray()['foam']==0,'private reaction settles',seconds=18)
        expected=tray();button('Back to Home');a.close();a=start()
        button('Science bench');button('Mix & discover')
        state=a.input('inspect');require(tray()==expected,'Offline reopen replayed or erased reaction')
        require('Sound off' in state['visibleText'] and 'Calm motion' in state['visibleText'],'Local preferences were not retained')
        record('solo chemistry advances while viewing boats and restores settled amounts and local preferences after relaunch')
        passed=True
    finally:
        for v in active:v.close()
        write(run.path/'mixing-solo-results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(run.path/'mixing-solo-results.json'),flush=True)

if __name__=='__main__':main()
