"""Replace an idle family authority without changing enrollment or discarding its save."""
import argparse
from datetime import datetime,timezone
import hashlib
import json
import shutil
import subprocess
import sys
import uuid
from shared_garden_runtime import ROOT,read,write,wait,require
from pc_server_installation import bundle, install, installed
from server_release import compare, network_release


def process(pid):
    command=f'Get-CimInstance Win32_Process -Filter "ProcessId = {int(pid)}" | Select-Object ProcessId,ExecutablePath,CommandLine | ConvertTo-Json -Compress'
    result=subprocess.run(['powershell.exe','-NoProfile','-Command',command],capture_output=True,text=True,check=True,timeout=15)
    return json.loads(result.stdout) if result.stdout.strip() else None


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--build',type=int,required=True);p.add_argument('--family',required=True)
    p.add_argument('--replace-compatible',action='store_true',help='Explicit server bug-fix deployment; app-only updates must not use this')
    args=p.parse_args()
    require(args.build>=83,'Persistent build required');world=uuid.UUID(args.family).hex;require(world==args.family,'Invalid world')
    root=ROOT/'LocalData/FamilyLAN'/world;latest=read(root/'latest-server.json');require(latest,'No existing server record')
    compatibility=compare(network_release(ROOT,latest['build']),network_release(ROOT,args.build))
    if compatibility['result']=='app-only' and not args.replace_compatible:
        print(json.dumps(dict(compatibility,operation='server-unchanged'),indent=2));return
    require(compatibility['result']!='review-required' or args.replace_compatible,'Shared sources changed without a compatibility change; review the server release explicitly')
    installation=installed(ROOT)
    if installation and installation['family']==world:
        from parent_server import ParentServer
        controller=ParentServer(world,installation['build'])
        require(controller.network_prepared(force=True),'Repair the reusable PC-server network permission first')
    else:
        firewall=read(ROOT/f'LocalData/Verification/server-firewall-{args.build}.json')
        require(firewall and firewall['passed'] and firewall['build']==args.build,'Complete the reviewed local-network firewall setup first')
    output=root/latest['instanceId'];native=process(latest['pid'])
    if native:
        expected=bundle(ROOT,world,latest['build'])[0]/'Server/LittleWeepsNetwork.exe'
        require(native['ExecutablePath'].lower()==str(expected).lower() and str(root/(latest['instanceId']+'.config.json')) in native['CommandLine'],'PID no longer identifies this family server')
        # The game guards this stop again, closing a player-join race after our
        # preflight. Never use an unconditional quit during occupied family play.
        from parent_server import ParentServer
        controller=ParentServer(world,latest['build'])
        snapshot=controller.snapshot()
        require(snapshot['state']=='ready' and snapshot['players']==0,'Players are connected or server status is uncertain; leave it running')
        controller.stop(snapshot['instanceId'])
        wait(lambda:process(latest['pid']) is None,'old server exits',25)
    backup=ROOT/'LocalData/ServerBackups'/f'{world}-{uuid.uuid4().hex}';backup.mkdir(parents=True)
    for path in (root/'server-world').iterdir():
        if path.is_file() and path.name!='authority.lock':shutil.copy2(path,backup/path.name)
    for path in root.glob('*.pairing'):shutil.copy2(path,backup/path.name)
    shutil.copy2(root/'family.json',backup/'family.json')
    saved=(backup/'world.save').read_text(encoding='utf-8-sig');header,digest,payload=saved.split('\n',2)
    require(header=='LITTLEWEEPS-SOLO-1' and hashlib.sha256(payload.encode()).hexdigest()==digest,'Saved checkpoint failed verification; no replacement started')
    before=json.loads(payload)
    if installation and installation['family']==world:install(ROOT,world,args.build)
    subprocess.run([sys.executable,str(ROOT/'Tools/Start-FamilyLAN.py'),'--build',str(args.build),'--family',world],check=True,timeout=60)
    new=read(root/'latest-server.json');new_output=root/new['instanceId']
    after=wait(lambda:read(new_output/'view.json'),'restored authority view')['view']
    # Compare all existing fields; additive timer metadata may be new on old saves.
    def retained(old,new):
        if isinstance(old,dict):return all(k in new and retained(v,new[k]) for k,v in old.items())
        if isinstance(old,list):return len(old)==len(new) and all(retained(a,b) for a,b in zip(old,new))
        return old==new
    # Presentation intentionally excludes durable receipts; verify the checkpoint itself.
    restored=json.loads((root/'server-world/world.save').read_text(encoding='utf-8-sig').split('\n',2)[2])
    expected=dict(before,schema=restored['schema'])
    require(restored['schema']>=before['schema'] and retained(expected,restored),'Restored checkpoint differs from the idle source; backup retained for review')
    require(new['persistentServer'] and read(new_output/'status.json')['persistentServer'],'Persistent mode not acknowledged')
    result=dict(passed=True,build=args.build,previousBuild=latest['build'],utc=datetime.now(timezone.utc).isoformat(),
                backup=str(backup),worldFieldsPreserved=True,playerCount=len(after['players']),toyCount=len(after['toys']),
                persistentServer=True,output=str(new_output),physicalRejoinTested=False)
    write(ROOT/f'LocalData/Verification/deploy-home-server-{args.build}.json',result)
    print(json.dumps(result,indent=2))


if __name__=='__main__':main()
