"""Start a verified home server (no time limit) or player for an enrolled family."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import socket
import uuid
from family_pairing import ROOT
from shared_garden_runtime import read,write,wait,require
from pc_server_installation import bundle, installation_lock, verify_bundle
from server_release import network_release


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--build',type=int,required=True);p.add_argument('--family',required=True)
    p.add_argument('--player',type=int,choices=range(0,5),default=0,help='0 is the dedicated authority; 1-4 launch a player')
    p.add_argument('--port',type=int,default=0,help='0 reuses the persistent game port; the first launch selects one');p.add_argument('--hidden',action='store_true');args=p.parse_args()
    world=uuid.UUID(args.family).hex;require(args.family==world and (args.port==0 or 1024<=args.port<=65535),'Invalid family/port')
    root=ROOT/'LocalData/FamilyLAN'/world;public=read(root/'family.json')
    require(public and public['worldId']==world,'Create parent enrollment first; no new identity was created')
    require(not (root/'recovery.pending.json').exists(),'Finish or roll back the interrupted recovery before starting this world')
    role='server' if args.player==0 else 'client'
    folder,installation=bundle(ROOT,world,args.build) if role=='server' else (ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}',None)
    summary=read(folder/'build-summary.json')
    require(summary and len(summary['builds'])==2 and all(b['result']=='Succeeded' and b['version']==f'0.0.{args.build}' and b['errors']==0 for b in summary['builds']),'Wrong build summary')
    verify_bundle(folder,'Server' if role=='server' else 'Client')
    release=network_release(ROOT,args.build)
    identity=uuid.uuid4().hex
    require(role!='server' or args.build>=83,'Persistent home hosting requires build 83 or later; older builds retain the test deadline')
    if role=='server':
        if installation:
            require(args.port in (0,installation['port']),'Keep the installed game port; endpoint changes require an explicit installation')
            args.port=installation['port']
        elif args.port==0:
            endpoint=read(root/'server-network.json') or read(root/'latest-server.json')
            if endpoint:args.port=endpoint['port']
        # Fail before opening a save if an explicitly requested port is busy.
        # The game still checks bind errors and holds its own authority lock.
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as probe:
            probe.setsockopt(socket.SOL_SOCKET,socket.SO_EXCLUSIVEADDRUSE,1)
            probe.bind(('0.0.0.0',args.port));args.port=probe.getsockname()[1]
    elif args.port==0:args.port=1025
    paired=root/('authority.pairing' if role=='server' else f'player-{args.player}.pairing');require(paired.is_file(),'Missing protected enrollment')
    config=dict(runId=world,instanceId=identity,role=role,port=args.port,protocol=release['protocol'],content=release['content'],
                pairingPath=str(paired),presentation=True,interactive=True,persistentServer=role=='server')
    output=root/identity;output.mkdir();path=root/(identity+'.config.json');write(path,config)
    exe=folder/('Server' if role=='server' else 'Client')/'LittleWeepsNetwork.exe'
    command=[str(exe),'-familyNetworkConfig',str(path),'-logFile',str(output/'player.log')]
    if role=='server':command+=['-batchmode','-nographics']
    else:command+=['-screen-fullscreen','0','-screen-width','960','-screen-height','640']
    startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0 if role=='server' or args.hidden else 1
    process=subprocess.Popen(command,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
    write(output/'launcher.json',dict(pid=process.pid,role=role,build=args.build,instanceId=identity,worldId=world))
    def ready():
        state=read(output/'status.json')
        if state:
            require(state['status']!='failed','Game startup failed; see its local status file')
            if role=='server' and state['status']=='listening':require(state.get('persistentServer') is True,'Server did not acknowledge persistent mode')
        return state and state['status'] in ('listening','connected','solo-available')
    try:wait(ready,'family '+role,seconds=35)
    except Exception:
        if process.poll() is None:
            write(output/'control.json',dict(serial=1,kind='quit'))
        raise
    write(root/('latest-server.json' if role=='server' else f'latest-player-{args.player}.json'),dict(instanceId=identity,pid=process.pid,port=args.port,build=args.build,persistentServer=role=='server'))
    if role=='server':write(root/'server-network.json',dict(format=1,port=args.port))
    print(json.dumps(dict(role=role,build=args.build,worldId=world,pid=process.pid,status=read(output/'status.json')['status'],output=str(output))))


if __name__=='__main__':
    # Serializes publication and launch across chats/helpers. The game's own
    # authority lock remains the final single-writer guard for the saved world.
    with installation_lock(ROOT):main()
