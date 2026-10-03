"""Four encrypted Windows clients recover in-process; no physical OS claim."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from copy import deepcopy
from datetime import datetime,timezone
import hashlib
import json
import socket
import subprocess
import time
import uuid
from family_pairing import ROOT,create_family,write_record
from shared_garden_runtime import Instance,read,write,wait,require


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);args=parser.parse_args()
    folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}'
    summary=read(folder/'build-summary.json')
    require(summary and all(b['result']=='Succeeded' and b['version']==f'0.0.{args.build}' for b in summary['builds']),'Wrong build')
    for entry in read(folder/'artifact-manifest.json'):
        path=(folder/entry['path']).resolve()
        require(path.is_relative_to(folder.resolve()) and hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'],'Artifact changed')
    authority,players,_=create_family()
    class Run:pass
    run=Run();run.build=args.build;run.run_id=authority['worldId'];run.protocol=3
    run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True)
    active=[];checks=[];success=False
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def port():
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as s:s.bind(('0.0.0.0',0));return s.getsockname()[1]
    def start(record,server_port=1025):
        v=Instance.__new__(Instance);v.run=run;v.role=record['role'];v.profile=record.get('profile','');v.serial=0;v.garden_serial=0
        v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
        pair=run.path/(v.identity+'.pairing');write_record(pair,record)
        cfg=dict(runId=run.run_id,instanceId=v.identity,role=v.role,port=server_port,protocol=3,content=3,pairingPath=str(pair),presentation=True,verifyGarden=v.role=='client')
        config=run.path/(v.identity+'.config.json');write(config,cfg)
        exe=folder/('Server' if v.role=='server' else 'Client')/'LittleWeepsNetwork.exe'
        command=[str(exe),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','960','-screen-height','640']
        if v.role=='server':command+=['-batchmode','-nographics']
        startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
        v.process=subprocess.Popen(command,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
        active.append(v)
        wait(lambda:v.status() and v.status()['status']==('listening' if v.role=='server' else 'connected'),'native '+v.role,30)
        if v.role=='client':wait(lambda:v.input('inspect')['shared'],'garden ready')
        return v
    try:
        first_port=port();server=start(authority,first_port);clients=[start(p) for p in players];first=clients[0];second=clients[1]
        wait(lambda:len(server.state()['connected'])==4,'four joined')
        identities=[(c.process.pid,c.identity) for c in clients];epoch=server.state()['epoch']
        passed('four encrypted clients have current playable snapshots before input is enabled')
        def toy(name):return next(t for t in server.state()['view']['toys'] if t['id']==name)
        def settled(c):wait(lambda:not c.input('inspect')['pending'],'no pending transaction')
        first.input('press',role='bucket-1');wait(lambda:toy('bucket-1')['holder']==first.profile,'hold before suspend')
        first.input('network-pause')
        wait(lambda:len(server.state()['connected'])==3 and toy('bucket-1')['holder']=='','departure release')
        frozen=deepcopy(server.state()['view']);time.sleep(3)
        require(first.status()['status']=='reconnecting' and not first.input('inspect')['connected'],'Paused client rejoined')
        require(server.state()['view']==frozen and server.state()['epoch']==epoch,'Pause changed the world or authority')
        second.input('press',role='bucket-1');second.input('release',x=toy('tap-1')['x'],y=toy('tap-1')['y']);settled(second)
        second.input('press',role='bucket-1');second.input('release',x=toy('plant-1')['x'],y=toy('plant-1')['y']);settled(second)
        require(toy('plant-1')['water']>0,'Sibling could not keep playing')
        water=toy('plant-1')['water']
        passed('suspending one client releases its held item, stops retries and leaves siblings playing')
        first.input('release',x=450,y=100);first.input('network-resume')
        wait(lambda:first.status()['status']=='connected' and len(server.state()['connected'])==4,'same client auto rejoins',35)
        require(first.input('inspect')['connected'] and not first.input('inspect')['pending'],'Rejoin has stale input')
        require(first.state()['epoch']==epoch and first.state()['view']==server.state()['view'],'Rejoin did not fetch current world')
        require(toy('bucket-1')['holder']=='' and toy('plant-1')['water']==water,'Old drop replayed')
        passed('resuming reuses the same app/profile and receives current state without replaying its drag')
        clients[2].input('button',text='Creek');settled(clients[2])
        first.input('press',role='bucket-1');wait(lambda:toy('bucket-1')['holder']==first.profile,'hold before crash')
        server.process.kill();server.process.wait(timeout=10)
        wait(lambda:all(not c.input('inspect')['connected'] for c in clients),'all show loss')
        for c in clients:require(c.status()['status'] in ('reconnecting','rediscovering','connecting'),'No automatic recovery state')
        require(not first.input('inspect')['pending'],'Uncertain transaction remained queued')
        passed('abrupt authority loss disables mutations and clears pending actions on all four clients')
        new_port=port()
        while new_port==first_port:new_port=port()
        server=start(authority,new_port)
        wait(lambda:len(server.state()['connected'])==4 and all(c.status()['status']=='connected' for c in clients),'four in-process reconnects',55)
        new_epoch=server.state()['epoch'];require(new_epoch!=epoch,'Restart reused old process epoch')
        for c in clients:
            wait(lambda:c.state()['epoch']==new_epoch,'new authoritative snapshot')
            require(c.input('inspect')['connected'] and not c.input('inspect')['pending'],'Client did not restore safe input')
        require(identities==[(c.process.pid,c.identity) for c in clients],'A client was relaunched')
        require(toy('plant-1')['water']==water and toy('bucket-1')['holder']=='','Checkpoint or hold not recovered')
        require(next(p for p in server.state()['view']['players'] if p['id']==clients[2].profile)['zone']=='creek','Independent location lost')
        passed('all four existing processes rediscover a changed port and accept a new epoch with saved progress')
        first.input('release',x=450,y=100)
        second.input('press',role='sponge-1');second.input('release',x=680,y=140);settled(second)
        require(toy('puddle-1')['water']==2,'Post-recovery input not applied exactly once')
        require(all(c.input('inspect')['passed'] for c in clients),'Unity input error')
        passed('shared interactions work after recovery with no duplicate stale action')
        # A trusted server can revoke a member; the client must not hammer
        # admission or claim success using the last cached view.
        server.close();revoked=deepcopy(authority);revoked['members'][0]['credentialHash']='0'*64
        server=start(revoked,port())
        wait(lambda:first.status()['status']=='needs-parent','revoked enrollment rejected',55)
        wait(lambda:len(server.state()['connected'])==3,'other three remain allowed',35)
        before=read(first.out/'connection-evidence.json')['events'];time.sleep(3)
        require(first.status()['status']=='needs-parent' and not first.input('inspect')['connected'],'Revoked profile still active')
        after=read(first.out/'connection-evidence.json')['events']
        require(sum(e['phase']=='discovered' for e in before)==sum(e['phase']=='discovered' for e in after),'Rejected client keeps retrying')
        passed('explicit credential rejection stops automatic retries while the other three players rejoin')
        success=True
    finally:
        for v in reversed(active):
            if v.process.poll() is None:
                try:v.close()
                except Exception:v.process.kill();v.process.wait(timeout=10)
        write(run.path/'reconnect-result.json',dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
            scope='Four native Windows clients; simulated lifecycle input calls the shared foreground adapter. Not physical iOS suspension, iPad hosting or offline reunion.'))
        print('Evidence:',run.path/'reconnect-result.json',flush=True)


if __name__=='__main__':main()
