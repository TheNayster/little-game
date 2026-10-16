# /// script
# dependencies = ["cryptography"]
# ///
"""Private road-trip retention and authority separation using disposable enrollment."""
import argparse, hashlib, importlib.util, json, os, shutil, socket, subprocess, sys, time, uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from family_pairing import ROOT,create_family,write_record
from shared_garden_runtime import Run,Instance,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);args=p.parse_args()
    os.environ['LITTLE_WEEPS_DRIVE_ART']=str(ROOT/'LocalData/Research/GoForADrive/Content-fixtures')
    run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');authority,players,_=create_family();slot=players[0]
    run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'go-for-a-drive';out.mkdir();passed=False;checks=[]
    print('EVIDENCE '+str(out),flush=True)
    source=read(run.folder/'source-manifest.json')
    require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in source['files'] if f['path'].startswith('Unity/') and not any('/Worlds/'+w+'/' in f['path'] for w in ('Markets','PeppaFunfair'))),'Drive/shared source differs from private candidate')
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
        a.input('fixtureTravel',text='go-for-a-drive');home.ready(a)
        tap('Drive start');tap('Drive luggage luggage-0');tap('Drive car peg 1')
        wait(lambda:state()['drive']['items'][0]['slot']==1,'one private packed item')
        before=state()['drive'];home.capture(a,out,'private-packing')
        a.input('application-pause');save=Path(state()['savePath']);require(save.is_file(),'Private checkpoint absent');a.close();a=start()
        after=state()['drive'];require(not state()['shared'] and state()['zone']=='go-for-a-drive','Private mode/world lost')
        require(after['begun'] and after['items'][0]['slot']==1 and after['items'][0]['id']==before['items'][0]['id'],'Private packing progress lost')
        require(not any(v['holder'] for v in after['items']),'Private transient owner retained')
        checks.append('Actual private process reopening retains original luggage and packing checkpoint, clearing transient ownership')
        require(run.run_id in str(save),'Private save does not belong to this generated family')
        server=Instance.__new__(Instance);server.run=run;server.role='server';server.profile='';server.serial=server.garden_serial=0;server.identity=uuid.uuid4().hex;server.out=run.path/server.identity;server.out.mkdir()
        pair=run.path/(server.identity+'.pairing');write_record(pair,authority)
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as sock:sock.bind(('127.0.0.1',0));port=sock.getsockname()[1]
        config=run.path/(server.identity+'.config.json');write(config,dict(runId=run.run_id,instanceId=server.identity,role='server',port=port,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=False,interactive=True))
        si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
        server.process=subprocess.Popen([str(run.folder/'Server/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(server.out/'player.log'),'-batchmode','-nographics'],startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW);run.instances.append(server)
        wait(lambda:server.status() and server.status()['status']=='listening','generated family authority listening')
        wait(lambda:state()['shared'],'actual private-to-authority automatic handoff',40)
        fresh=server.state()['view']['drive'];require(not fresh['begun'] and all(v['slot']==-1 for v in fresh['items']),'Private progress uploaded into authority')
        checkpoint=json.loads(save.read_text(encoding='utf-8').split('\n',2)[2])
        require(checkpoint['drive']['begun'] and checkpoint['drive']['items'][0]['slot']==1,'Automatic handoff changed private checkpoint')
        tap('Menu');tap('Play by myself');wait(lambda:not state()['shared'],'explicit private return',20)
        require(state()['drive']['begun'] and state()['drive']['items'][0]['slot']==1,'Private trip did not return')
        require(not server.state()['view']['drive']['begun'],'Returning private changed authority')
        checks.append('Real automatic private-to-authority handoff preserves separate saves; explicit private return restores the original packed item')

        passed=True
    finally:
        try:write(out/'last-client-state.json',state())
        except (RuntimeError,OSError):pass
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,checks=checks,build=args.build,runtimeErrors=errors,physicalDevices=False,ownerAcceptance=False,audioListening=False,installedFamilyTouched=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
