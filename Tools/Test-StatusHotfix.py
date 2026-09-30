"""Four baseline clients on an isolated hotfix server; deliberately lock observations."""
import json
from pathlib import Path
import socket
import subprocess
import time
import uuid

from family_pairing import create_family,write_record
from parent_server import read as shared_read
from shared_garden_runtime import ROOT,Instance,write,wait,require


def main():
    authority,players,_=create_family();family=authority['worldId'];root=ROOT/'LocalData/FamilyLAN'/family;root.mkdir()
    write(root/'family.json',dict(worldId=family,purpose='isolated parent-control acceptance'))
    write_record(root/'authority.pairing',authority)
    for i,p in enumerate(players,1):write_record(root/f'player-{i}.pairing',p)
    class Run:pass
    run=Run();run.build=227;run.run_id=family;run.path=root
    with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as probe:probe.bind(('0.0.0.0',0));port=probe.getsockname()[1]
    instances=[]
    def start(role,index=0):
        v=Instance.__new__(Instance);v.run=run;v.role=role;v.profile=players[index-1]['profile'] if index else ''
        v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=root/v.identity;v.out.mkdir()
        cfg=root/(v.identity+'.config.json');write(cfg,dict(runId=family,instanceId=v.identity,role=role,port=port if role=='server' else 1025,protocol=3,content=31,
            pairingPath=str(root/(f'player-{index}.pairing'if index else 'authority.pairing')),presentation=True,verifyGarden=role=='client',interactive=True,persistentServer=role=='server'))
        exe=ROOT/'Builds/ServerHotfix/227-status-io-1/Server/LittleWeepsNetwork.exe' if role=='server' else ROOT/'Builds/NetworkProbe/G3-0.0.227/Client/LittleWeepsNetwork.exe'
        args=[str(exe),'-familyNetworkConfig',str(cfg),'-logFile',str(v.out/'player.log')]
        args+=['-batchmode','-nographics'] if role=='server' else ['-screen-fullscreen','0','-screen-width','800','-screen-height','480']
        startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
        v.process=subprocess.Popen(args,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
        instances.append(v);wait(lambda:(s:=shared_read(v.out/'status.json')) and s['status']==('listening'if role=='server'else 'connected'),role+' ready',35)
        return v
    try:
        server=start('server');clients=[start('client',i) for i in range(1,5)]
        wait(lambda:len(shared_read(server.out/'view.json')['connected'])==4,'four joined')
        before=json.loads((root/'server-world/world.save').read_text(encoding='utf-8-sig').split('\n',2)[2])
        lock=server.out/'view.json'
        with lock.open('rb') as held:
            # Ordinary Python open denies deletion on Windows, reproducing the
            # ReplaceFile failure during actual Unity server observation writes.
            time.sleep(2)
            require(server.process.poll() is None,'Server crashed under locked view')
            for c in clients:require(c.process.poll() is None and c.input('inspect')['shared'],'Client lost shared play')
        wait(lambda:(m:=shared_read(server.out/'motion-stats.json')) and m.get('diagnosticWriteConflicts',0)>0,'conflict counted')
        require(len(shared_read(server.out/'view.json')['connected'])==4,'All players retained')
        clients[0].close();wait(lambda:len(shared_read(server.out/'view.json')['connected'])==3,'independent leave')
        after=json.loads((root/'server-world/world.save').read_text(encoding='utf-8-sig').split('\n',2)[2])
        require(before['schema']==after['schema']==30,'Schema changed')
        evidence=dict(passed=True,fourClients=True,heldViewSeconds=2,sameAuthorityPid=True,diagnosticConflicts=shared_read(server.out/'motion-stats.json')['diagnosticWriteConflicts'],
            independentDeparture=True,baselineContent=31,baselineSchema=30,saveStillReadable=True,physicalDevicesTested=False)
        write(ROOT/'Builds/ServerHotfix/227-status-io-1/validation.json',evidence);print(json.dumps(evidence))
    finally:
        for v in reversed(instances):
            if v.process.poll() is None:
                try:v.close()
                except Exception:v.process.kill();v.process.wait(timeout=10)


if __name__=='__main__':main()
