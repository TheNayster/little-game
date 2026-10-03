"""Initial offline play -> native encrypted family discovery without relaunch or lost drafts."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import hashlib
from pathlib import Path
import socket
import subprocess
import time
import uuid
from family_pairing import ROOT, create_family, write_record
from shared_garden_runtime import Instance, read, write, wait, require


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);args=parser.parse_args()
    folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}'
    summary=read(folder/'build-summary.json')
    require(summary and all(b['result']=='Succeeded' and b['errors']==0 and b['version']==f'0.0.{args.build}' for b in summary['builds']),'Wrong build')
    for e in read(folder/'artifact-manifest.json'):
        p=(folder/e['path']).resolve()
        require(p.is_relative_to(folder.resolve()) and hashlib.sha256(p.read_bytes()).hexdigest()==e['sha256'],'Artifact changed')
    authority,players,_=create_family()
    class Run:pass
    run=Run();run.build=args.build;run.run_id=authority['worldId'];run.protocol=3
    run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True)
    active=[];checks=[];success=False;blocked=None
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def start(record):
        v=Instance.__new__(Instance);v.run=run;v.role=record['role'];v.profile=record.get('profile','');v.serial=0;v.garden_serial=0
        v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
        pair=run.path/(v.identity+'.pairing');write_record(pair,record)
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as s:s.bind(('0.0.0.0',0));port=s.getsockname()[1]
        cfg=dict(runId=run.run_id,instanceId=v.identity,role=v.role,port=port if v.role=='server' else 1025,protocol=3,content=3,
                 pairingPath=str(pair),presentation=True,verifyGarden=v.role=='client')
        config=run.path/(v.identity+'.config.json');write(config,cfg)
        exe=folder/('Server' if v.role=='server' else 'Client')/'LittleWeepsNetwork.exe'
        command=[str(exe),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','960','-screen-height','640']
        if v.role=='server':command+=['-batchmode','-nographics']
        startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
        v.process=subprocess.Popen(command,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
        active.append(v)
        wait(lambda:v.status(),'status',30)
        if v.role=='server':wait(lambda:v.status()['status']=='listening','server listening')
        else:wait(lambda:v.input('inspect')['ready'],'immediate local presentation')
        return v
    def inspect(v):return v.input('inspect')
    def shared(v):return inspect(v)['shared']
    def settle(v):wait(lambda:not inspect(v)['pending'],'settled transaction')
    def local(v):
        import json
        data=json.loads(Path(inspect(v)['savePath']).read_text().split('\n',2)[2])
        # Eligible play now advances housekeeping clocks even without moving an
        # item. Compare retained gameplay here; timer persistence has its own tests.
        data.pop('idleTimers',None)
        return data
    def item(view,name):return next(t for t in view['toys'] if t['id']==name)
    def mode(v,label):v.input('button',text='Menu');v.input('button',text=label)
    try:
        first=start(players[0]);pid=first.process.pid
        require(not shared(first) and first.status()['status']=='discovering','Local play waited for a failed network attempt')
        first.input('button',text='Orange pup')
        first.input('press',role='bucket-1');first.input('release',x=170,y=300)
        wait(lambda:item(local(first),'bucket-1')['water']>0,'saved offline bucket')
        wait(lambda:first.status()['status']=='solo-available','first timeout',20)
        first.input('press',role='bucket-1')
        passed('paired cold launch is playable while discovery runs and remains playable after its first timeout')

        server=start(authority);siblings=[start(p) for p in players[1:]]
        wait(lambda:all(shared(c) for c in siblings) and len(server.state()['connected'])==4,'four admitted',45)
        time.sleep(2)
        require(not shared(first) and inspect(first)['dragging']=='bucket-1','Active local drag interrupted')
        require(item(server.state()['view'],'plant-1')['water']==0,'Local branch entered authority')
        passed('server starting later is found in the same process; a held local toy delays the presentation switch')

        first.input('release',x=820,y=300);first.input('button',text='Menu')
        wait(lambda:item(local(first),'plant-1')['water']>0,'local watering saved')
        original=deepcopy(local(first));time.sleep(2)
        require(not shared(first) and inspect(first)['menuOpen'],'Open menu was interrupted')
        # Real filesystem failure, scoped to this new test world's staged save.
        # Never rename/delete existing saves or inject a bypass into game code.
        save=Path(inspect(first)['savePath']);blocked=Path(str(save)+'.pending')
        require(run.run_id in str(save) and not blocked.exists(),'Unsafe staged-path test')
        blocked.mkdir();first.input('button',text='Back to play');time.sleep(4)
        require(not shared(first) and local(first)==original,'Failed checkpoint abandoned local play')
        blocked.rmdir();blocked=None
        wait(lambda:shared(first),'automatic safe switch after disk recovers',10)
        require(local(first)==original,'Offline checkpoint overwritten during join')
        require(first.process.pid==pid and item(server.state()['view'],'plant-1')['water']==0,'Relaunch or implicit merge')
        passed('menu and failed durable checkpoint block the switch; recovery joins automatically without overwriting either world')

        for cycle in range(3):
            mode(first,'Play by myself');wait(lambda:not shared(first),'return to saved solo')
            layout=first.input('resize',x=1560,y=720)
            require(layout['controlsInSafeArea'] and layout['boardLayoutWidth']>1500,'Solo phone layout after switching')
            wait(lambda:len(server.state()['connected'])==3,'solo choice releases family slot')
            require(local(first)==original and inspect(first)['visiblePlayers']==1,'Saved local branch lost')
            time.sleep(2);require(not shared(first) and first.status()['status']=='solo-selected','Explicit solo choice ignored')
            peer=siblings[0];peer.input('button',text='Orange pup' if cycle%2==0 else 'Blue pup');settle(peer)
            mode(first,'Find my family');wait(lambda:shared(first),'menu rejoins current family',30)
            wait(lambda:len(server.state()['connected'])==4,'four back')
            e=inspect(first)
            layout=first.input('resize',x=1024,y=768)
            require(layout['controlsInSafeArea'] and abs(layout['boardLayoutWidth']-1120)<.1,'Shared tablet layout after switching')
            require(e['canvases']==1 and e['narrators']==1 and e['audioSources']==1,'Presentation/audio leaked across switches')
            require(local(first)==original and not e['pending'] and not e['dragging'],'Local save/input changed')
        passed('three solo/family round trips preserve gameplay, resize phone/tablet layouts and keep one canvas/narrator/audio source')

        mode(first,'Play by myself');wait(lambda:not shared(first),'local before offline pause')
        mode(first,'Find my family');first.input('network-pause');time.sleep(3)
        require(not shared(first) and not inspect(first)['connected'],'Paused initial-local client rejoined')
        first.input('network-resume');wait(lambda:shared(first),'resumed local joins family',30)
        require(first.process.pid==pid and local(first)==original,'Resume changed process or draft')
        passed('foreground adapter stops local discovery/admission while suspended and resumes without restarting play')

        mode(first,'Play by myself');wait(lambda:not shared(first),'persisted local before cold reopen')
        first.close();first=start(players[0]);wait(lambda:shared(first),'cold reopen automatic join',30)
        mode(first,'Play by myself');wait(lambda:not shared(first),'cold reopen draft access')
        require(local(first)==original,'Cold reopen lost local work')
        passed('app close/reopen defaults to automatic family discovery and the saved solo branch remains accessible')

        revoked=deepcopy(authority);revoked['members'][0]['credentialHash']='0'*64
        server.close();server=start(revoked)
        mode(first,'Find my family');wait(lambda:first.status()['status']=='needs-parent','cold admission revocation',40)
        require(not shared(first) and inspect(first)['ready'],'Rejected admission removed solo play')
        events=read(first.out/'connection-evidence.json')['events'];time.sleep(3)
        require(sum(e['phase']=='discovered' for e in read(first.out/'connection-evidence.json')['events'])==sum(e['phase']=='discovered' for e in events),'Revocation retried')
        require(local(first)==original,'Denied join overwrote solo branch')
        passed('explicit rejection stops retry attempts while keeping saved offline play usable')
        success=True
    finally:
        if blocked is not None and blocked.is_dir():blocked.rmdir()
        for v in reversed(active):
            if v.process.poll() is None:
                try:v.close()
                except Exception:v.process.kill();v.process.wait(timeout=10)
        write(run.path/'offline-join-result.json',dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
            scope='Native Windows Bonjour/DTLS and actual UI input. Foreground adapter is simulated; no physical mobile, host migration or offline merge claim.'))
        print('Evidence:',run.path/'offline-join-result.json',flush=True)


if __name__=='__main__':main()
