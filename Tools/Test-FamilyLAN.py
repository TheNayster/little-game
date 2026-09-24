"""Native Windows Bonjour + DTLS admission proof; fresh isolated family/save only."""
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import hashlib
import json
import os
import shutil
from pathlib import Path
import socket
import subprocess
import uuid
from family_pairing import ROOT, create_family, write_record
from shared_garden_runtime import Instance, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    folder = ROOT / f'Builds/NetworkProbe/G3-0.0.{args.build}'
    summary = read(folder/'build-summary.json')
    require(summary and all(b['result']=='Succeeded' and b['version']==f'0.0.{args.build}' for b in summary['builds']), 'Wrong build')
    for entry in read(folder/'artifact-manifest.json'):
        path = (folder/entry['path']).resolve()
        require(path.is_relative_to(folder.resolve()) and hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'], 'Artifact changed')
    authority, players, _ = create_family()
    class Run: pass
    run = Run(); run.build=args.build; run.run_id=authority['worldId']; run.protocol=3
    run.path=ROOT/'LocalData/FamilyLAN'/run.run_id; run.path.mkdir(parents=True)
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as available:
        available.bind(('0.0.0.0',0)); port=available.getsockname()[1]
    active=[]; checks=[]
    def passed(name):
        checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def start(record, solo_run=None, **overrides):
        instance=Instance.__new__(Instance)
        instance.run=run;instance.role=record['role'];instance.profile=record.get('profile','');instance.serial=0;instance.garden_serial=0
        instance.identity=uuid.uuid4().hex;instance.out=run.path/instance.identity;instance.out.mkdir()
        pairing=run.path/(instance.identity+'.pairing');write_record(pairing,record)
        cfg=dict(runId=run.run_id,instanceId=instance.identity,role=instance.role,profile=instance.profile,
                 port=port if instance.role=='server' else 1025,protocol=3,content=3,pairingPath=str(pairing),
                 presentation=True,verifyGarden=instance.role=='client' and not solo_run)
        cfg.update(overrides)
        config=run.path/(instance.identity+'.config.json');write(config,cfg)
        exe=folder/('Server' if instance.role=='server' else 'Client')/'LittleWeepsNetwork.exe'
        command=[str(exe),'-familyNetworkConfig',str(config),'-logFile',str(instance.out/'player.log'),'-screen-fullscreen','0','-screen-width','960','-screen-height','640']
        if instance.role=='server': command+=['-batchmode','-nographics']
        if solo_run: command+=['-soloVerify','input','-soloRun',solo_run]
        startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
        instance.process=subprocess.Popen(command,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,
                                         startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
        active.append(instance);return instance
    try:
        server=start(authority)
        wait(lambda: server.status() and server.status()['status']=='listening','encrypted server')
        clients=[]
        for player in players:
            client=start(player);wait(lambda: client.state(),'native discovery and DTLS snapshot',30)
            wait(lambda: client.input('inspect')['shared'],'playable encrypted garden');clients.append(client)
        wait(lambda: len(server.state()['connected'])==4,'four admitted players')
        require(all(c.input('inspect')['visiblePlayers']==4 for c in clients),'Four avatars visible')
        passed('four enrolled clients discover the server without a configured IP or correct port and join over DTLS')
        clients[0].input('button',text='Creek')
        wait(lambda: next(p for p in server.state()['view']['players'] if p['id']==clients[0].profile)['zone']=='creek','independent travel')
        clients[1].input('press',role='bucket-1')
        wait(lambda: next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder']==clients[1].profile,'exclusive bucket')
        clients[2].input('press',role='bucket-1');clients[2].input('release',x=500,y=200)
        require(next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder']==clients[1].profile,'Bucket stolen')
        clients[1].input('release',x=500,y=200)
        wait(lambda: not next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder'],'bucket released')
        passed('encrypted clients preserve independent travel and one-holder item ownership')
        before=deepcopy(server.state()['view'])
        # Discovery must not be a source of trust, even with four open slots' IDs.
        variants=[]
        bad=deepcopy(players[0]);bad['credential']='0'*64;variants.append(('wrong credential',bad,{}))
        bad=deepcopy(players[0]);bad['profile']=uuid.uuid4().hex;variants.append(('unpaired profile',bad,{}))
        variants.append(('duplicate profile',players[0],{}))
        variants.append(('incompatible protocol',players[0],dict(protocol=99)))
        bad=deepcopy(players[0]);bad['familyId']=uuid.uuid4().hex;variants.append(('wrong family advertisement',bad,{}))
        other,_,_=create_family();bad=deepcopy(players[0]);bad['caCertificate']=other['caCertificate'];variants.append(('untrusted server certificate',bad,{}))
        for name,record,options in variants:
            guest=start(record,**options)
            wait(lambda: guest.status() and guest.status()['status'] in ('solo-available','needs-parent'),name+' denied with solo fallback',30)
            require(not guest.state(),name+' got shared state')
            # The fallback writes only its distinct local branch, not shared/old solo saves.
            require(server.state()['view']==before and len(server.state()['connected'])==4,name+' changed authority')
            guest.close();passed(name+' cannot enter or mutate the world; local play remains available')
        clients[3].close()
        wait(lambda: len(server.state()['connected'])==3,'fourth leaves')
        replacement=start(players[3]);wait(lambda: replacement.state(),'repeat launch auto discovers')
        require(server.state()['epoch']==replacement.state()['epoch'],'Authority changed')
        passed('leaving and relaunching a paired client discovers the same live authority without disturbing the others')
        for client in clients[:3]: client.close()
        replacement.close();server.close()
        solo_run=uuid.uuid4().hex
        missing=start(players[0],solo_run=solo_run)
        missing.process.wait(timeout=90)
        solo_folder=Path(os.environ['USERPROFILE'])/'AppData/LocalLow/Little Weeps/Little Weeps/SoloPrototype'/solo_run
        result=read(solo_folder/'input.json')
        require(missing.process.returncode==0 and result and result['passed'] and result['runId']==solo_run,'Offline play input verification failed')
        shutil.copyfile(solo_folder/'input.json',run.path/'offline-input.json')
        passed('absent server fallback passes actual solo touch, joystick, dragging, menu, watering and audio checks')
        sensitive=[p['credential'] for p in players]+[authority['privateKey']]
        for path in run.path.rglob('*'):
            if path.is_file() and path.suffix in ('.log','.json'):
                data=path.read_text(encoding='utf-8',errors='replace')
                require(not any(s in data for s in sensitive),'Private enrollment leaked to plaintext artifacts')
        passed('credentials and authority private key absent from configs, observations and logs')
    finally:
        for instance in reversed(active):
            if instance.process.poll() is None:
                try: instance.close()
                except Exception: instance.process.kill();instance.process.wait(timeout=10)
        write(run.path/'result.json',dict(utc=datetime.now(timezone.utc).isoformat(),build=args.build,runId=run.run_id,checks=checks,
              scope='Windows native Bonjour/DTLS proof on one PC, IPv4; no physical device or automatic recovery qualification'))
        print('Evidence:',run.path/'result.json',flush=True)


if __name__=='__main__': main()
