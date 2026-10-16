# /// script
# dependencies = ["cryptography"]
# ///
"""Private Funfair retention, NPC residents and authority separation using disposable enrollment."""
import argparse, hashlib, importlib.util, json, os, shutil, socket, subprocess, sys, time, uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from family_pairing import ROOT,create_family,write_record
from shared_garden_runtime import Run,Instance,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');authority,players,_=create_family();slot=players[0]
    run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True);out=run.path/'peppa-funfair';out.mkdir();passed=False;checks=[]
    print('EVIDENCE '+str(out),flush=True)
    source=read(run.folder/'source-manifest.json')
    require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in source['files'] if f['path'].startswith('Unity/')),'Source differs from private candidate')
    def start():
        v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=slot['profile'];v.serial=v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
        pair=run.path/(v.identity+'.pairing');write_record(pair,slot)
        config=run.path/(v.identity+'.config.json');write(config,dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True,testAudible=True))
        si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
        v.process=subprocess.Popen([str(run.folder/'Client/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-force-d3d11','-screen-fullscreen','0','-screen-width','1280','-screen-height','800'],startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW)
        run.instances.append(v);wait(lambda:v.status(),'private startup');home.ready(v);return v
    def state():return a.input('inspect')
    def tap(name):
        wait(lambda:any(c['name']==name and c['enabled'] for c in state()['controls']),'private button '+name,20)
        a.input('touchButton',text=name)
        home.ready(a)
    try:
        a=start();require(not state()['shared'],'Private client unexpectedly shared')
        a.input('fixtureTravel',text='peppa-funfair');home.ready(a)
        require(state()['visibleFunfairPlayers']==1,'Expected one private player')
        tap('Funfair station wheel');wait(lambda:len(state()['funfair']['wheel']['riders'])==1,'private boarding');tap('Funfair tool Start');wait(lambda:state()['funfair']['wheel']['rounds']==1,'private ride result',20);wait(lambda:state()['funfair']['wheel']['stage']=='idle','private unload')
        before=state()['funfair'];checks.append('One private player completes a shared-rule ride with the recovered resident cast present')
        home.capture(a,out,'private-funfair');a.input('application-pause');save=Path(state()['savePath']);require(save.is_file(),'Private checkpoint absent');a.close();a=start()
        after=state()['funfair'];require(not state()['shared'] and state()['zone']=='peppa-funfair','Private mode/world lost');require(after['wheel']['rounds']==1 and after['wheel']['riders']==[],'Private result or occupancy differs after reopen');checks.append('Actual private process reopening retains ride history and clears transient seats')
        require(run.run_id in str(save),'Private save does not belong to this generated family')
        server=Instance.__new__(Instance);server.run=run;server.role='server';server.profile='';server.serial=server.garden_serial=0;server.identity=uuid.uuid4().hex;server.out=run.path/server.identity;server.out.mkdir()
        pair=run.path/(server.identity+'.pairing');write_record(pair,authority)
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as sock:sock.bind(('127.0.0.1',0));port=sock.getsockname()[1]
        config=run.path/(server.identity+'.config.json');write(config,dict(runId=run.run_id,instanceId=server.identity,role='server',port=port,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=False,interactive=True))
        si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
        server.process=subprocess.Popen([str(run.folder/'Server/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(server.out/'player.log'),'-batchmode','-nographics'],startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW);run.instances.append(server)
        wait(lambda:server.status() and server.status()['status']=='listening','generated family authority listening')
        wait(lambda:state()['shared'],'actual private-to-authority automatic handoff',40)
        fresh=server.state()['view']['funfair'];require(fresh['wheel']['rounds']==0,'Private Funfair progress was uploaded into authority')
        checkpoint=json.loads(save.read_text(encoding='utf-8').split('\n',2)[2])
        require(checkpoint['funfair']['wheel']['rounds']==1,'Automatic handoff changed the private checkpoint')
        tap('Menu');tap('Play by myself');wait(lambda:not state()['shared'],'explicit private return',20)
        require(state()['funfair']['wheel']['rounds']==1,'Private Funfair progress did not return')
        require(server.state()['view']['funfair']['wheel']['rounds']==0,'Returning private changed shared progress')
        checks.append('One real paired client automatically joins its generated authority without uploading private Funfair progress; explicit solo return restores the separate private visit')

        passed=True
    finally:
        try:write(out/'last-client-state.json',state())
        except (RuntimeError,OSError):pass
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,checks=checks,build=args.build,runtimeErrors=errors,physicalDevices=False,ownerAcceptance=False,audioListening=False,installedFamilyTouched=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
