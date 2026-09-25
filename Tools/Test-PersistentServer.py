"""Isolated lifetime boundary, four-player restart and single-writer qualification."""
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import hashlib
import json
import socket
import subprocess
import sys
import time
import uuid
from family_pairing import ROOT, create_family, write_record
from shared_garden_runtime import Instance, read, write, wait, require


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);args=parser.parse_args()
    folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}'
    require(all(b['result']=='Succeeded' and b['version']==f'0.0.{args.build}' for b in read(folder/'build-summary.json')['builds']),'Wrong build')
    for entry in read(folder/'artifact-manifest.json'):
        path=(folder/entry['path']).resolve()
        require(path.is_relative_to(folder.resolve()) and hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'],'Artifact changed')
    active=[];checks=[];success=False
    authority,players,_=create_family()
    class Run: pass
    run=Run();run.build=args.build;run.run_id=authority['worldId'];run.protocol=3
    run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True)
    write(run.path/'family.json',dict(worldId=run.run_id))
    write_record(run.path/'authority.pairing',authority)
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def port():
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as s:s.bind(('127.0.0.1',0));return s.getsockname()[1]
    def start(record,**options):
        v=Instance.__new__(Instance);v.run=run;v.role=record['role'];v.profile=record.get('profile','');v.serial=0;v.garden_serial=0
        v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
        pair=run.path/(v.identity+'.pairing');write_record(pair,record)
        cfg=dict(runId=run.run_id,instanceId=v.identity,role=v.role,port=port(),protocol=3,content=3,pairingPath=str(pair),
                 presentation=True,verifyGarden=True,interactive=v.role=='server',persistentServer=v.role=='server')
        cfg.update(options);config=run.path/(v.identity+'.config.json');write(config,cfg)
        exe=folder/('Server' if v.role=='server' else 'Client')/'LittleWeepsNetwork.exe'
        command=[str(exe),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','960','-screen-height','640']
        if v.role=='server':command+=['-batchmode','-nographics']
        startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
        v.process=subprocess.Popen(command,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
        active.append(v);return v
    def listening(s):return s.status() and s.status()['status']=='listening'
    def settled(c):wait(lambda:not c.input('inspect')['pending'],'settled command')
    try:
        for interactive,offset in [(False,241),(True,7201)]:
            timed=start(authority,persistentServer=False,interactive=interactive,testLifetimeOffsetSeconds=offset)
            timed.process.wait(timeout=20)
            state=read(timed.out/'status.json')
            require(timed.process.returncode!=0 and state['status']=='failed' and state['reason']=='Isolated probe lifetime exceeded.','Probe deadline disabled')
        passed('both automated and interactive test deadlines still expire through the native runtime branch')
        server=start(authority,testLifetimeOffsetSeconds=7198);wait(lambda:listening(server),'persistent authority')
        before=deepcopy(server.state()['view']);time.sleep(5)
        require(server.process.poll() is None and server.status()['persistentServer'] and server.state()['view']==before,'Idle server expired or changed world')
        passed('persistent native authority crosses the simulated 7200-second boundary and stays available with no players')
        clients=[start(p) for p in players]
        for c in clients:wait(lambda:c.status() and c.status()['status']=='connected' and c.input('inspect')['shared'],'playable client',35)
        wait(lambda:len(server.state()['connected'])==4,'four players')
        first=clients[0];first.input('press',role='bucket-1');first.input('release',x=720,y=150);settled(first)
        clients[1].input('button',text='Creek');settled(clients[1]);epoch=server.state()['epoch']
        before=deepcopy(server.state()['view'])
        passed('four enrolled players join beyond the former boundary and can move shared items and travel independently')
        duplicate=start(authority);duplicate.process.wait(timeout=20)
        state=read(duplicate.out/'status.json')
        require(duplicate.process.returncode!=0 and state['status']=='failed' and 'authority.lock' in state['reason'],'Duplicate authority not refused')
        require(server.process.poll() is None and server.state()['epoch']==epoch and server.state()['view']==before,'Duplicate start disturbed authority')
        passed('a second authority is refused by the save lock without disturbing four connected clients')
        identities=[(c.process.pid,c.identity) for c in clients];server.close()
        require(server.process.returncode==0 and read(server.out/'status.json')['status']=='stopped','Unclean stop')
        wait(lambda:all(not c.input('inspect')['connected'] for c in clients),'client disconnects')
        # Exercise the real parent launcher as well as the runtime config path.
        result=subprocess.run([sys.executable,str(ROOT/'Tools/Start-FamilyLAN.py'),'--build',str(args.build),'--family',run.run_id],capture_output=True,text=True,timeout=50)
        require(result.returncode==0,'Home launcher failed: '+result.stderr)
        latest=read(run.path/'latest-server.json');out=run.path/latest['instanceId']
        try:
            wait(lambda:len(read(out/'view.json')['connected'])==4,'four automatic rejoins',55)
            after=read(out/'view.json');require(after['epoch']!=epoch and read(out/'status.json')['persistentServer'],'Not a fresh persistent authority')
            require(after['view']['players']==before['players'] and after['view']['toys']==before['toys'],'Save was not preserved')
            require(identities==[(c.process.pid,c.identity) for c in clients],'Clients were relaunched')
            passed('controlled stop flushes the world; the normal launcher restores it in persistent mode and all four existing clients rejoin')
            for c in clients:c.close()
            wait(lambda:not read(out/'view.json')['connected'],'all players leave')
            state_before=deepcopy(read(out/'view.json')['view']);time.sleep(3)
            require(read(out/'view.json')['view']==state_before and read(out/'status.json')['status']=='listening','Empty server changed the world or stopped')
            passed('last player leaving pauses world activity without shutting down the home server')
        finally:
            write(out/'control.json',dict(serial=1,kind='quit'))
            wait(lambda:read(out/'status.json')['status']=='stopped','home server stopped')
        success=True
    finally:
        for v in reversed(active):
            if v.process.poll() is None:
                try:v.close()
                except Exception:v.process.kill();v.process.wait(timeout=10)
        write(run.path/'persistent-result.json',dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
            scope='Native Windows four-client test; lifetime boundary uses an explicit verification-only elapsed offset. Not a two-hour wall-clock soak or physical-device restart acceptance.'))
        print('Evidence:',run.path/'persistent-result.json',flush=True)


if __name__=='__main__':main()
