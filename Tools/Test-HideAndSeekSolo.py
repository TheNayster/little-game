# /// script
# dependencies = ["cryptography"]
# ///
"""Unserved disposable enrollment: native solo hide-and-seek and safe reopen."""
import argparse, json, subprocess, time, uuid
from pathlib import Path
from family_pairing import ROOT, create_family, write_record
from shared_garden_runtime import Instance, read, write, wait, require

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}'
    summary=read(folder/'build-summary.json');require(summary['schema'] ==30,'Wrong hiding candidate')
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
        def hider():return world()['hideAndSeek']['hiders'][0]
        def button(name):a.input('touchButton',text=name);time.sleep(.2)
        a.input('fixtureTravel',text='home');time.sleep(1)
        button('Hide & seek');button('Play hide and seek')
        wait(lambda:hider()['mode']==1,'private joining')
        button('Hide Curtain');wait(lambda:hider()['mode']==2,'private hide')
        time.sleep(3);remaining=hider()['preparation'];require(remaining<14,'Private fifteen-second countdown stalled')
        wait(lambda:hider()['mode']==3,'private friendly find',100)
        record('private solo has working pictured entry auto-approach countdown inspection and friendly find')
        button('Come out');wait(lambda:hider()['mode']==1,'private hide again');require(world()['hideAndSeek']['round']==2,'Chilli missing from private second turn')
        button('Hide Curtain');wait(lambda:hider()['mode']==2,'private re-entry')
        time.sleep(1.2);before=world();a.close();a=start();time.sleep(1)
        require(hider()['mode']==0 and hider()['slot']==-1,'Private reopen resumed stale role')
        after=world();require(after['toys']==before['toys'] and after['bedrooms']==before['bedrooms'] and after['homeCreations']==before['homeCreations'],'Reopen changed durable content')
        record('private cold reopen releases hiding safely while preserving toys rooms and creations')
        for v in active:require('Exception:' not in (v.out/'player.log').read_text(errors='replace'),'Native solo exception')
        passed=True
    finally:
        for v in active:v.close()
        write(run.path/'hide-and-seek-solo-results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(run.path/'hide-and-seek-solo-results.json'),flush=True)

if __name__=='__main__':main()
