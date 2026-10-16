# /// script
# dependencies = ["cryptography"]
# ///
"""Private Markets process retention and authority separation using disposable enrollment."""
import argparse, hashlib, importlib.util, json, os, shutil, socket, subprocess, sys, time, uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from family_pairing import ROOT,create_family,write_record
from shared_garden_runtime import Run,Instance,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
control_spec=importlib.util.spec_from_file_location('markets_controls',Path(__file__).with_name('Test-MarketsControls.py'));controls=importlib.util.module_from_spec(control_spec);control_spec.loader.exec_module(controls)

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');authority,players,_=create_family();slot=players[0]
    run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True);out=run.path/'markets';out.mkdir();passed=False;checks=[]
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
        controls.MarketsTouches(a).ensure(name)
        wait(lambda:any(c['name']==name and c['enabled'] for c in state()['controls']),'private button '+name,20)
        a.input('touchButton',text=name)
        home.ready(a)
    try:
        a=start();require(not state()['shared'],'Private client unexpectedly shared')
        a.input('fixtureTravel',text='markets');home.ready(a)
        tap('Markets visit icecream')
        wait(lambda:any(m['actor']==a.profile and m['station']=='icecream' for m in state()['markets']['members']),'private ice cream approach',25)
        tap('Markets build slot 6');wait(lambda:len(state()['markets']['bricks'])==1,'private shared-board creation')
        tap('Markets Chocolate')
        for _ in range(5):
            tap('Markets Add scoop')
            wait(lambda:next(t for t in state()['markets']['stations'] if t['id']=='icecream')['workingActor']=='','private recipe placement finish',10)
        wait(lambda:any(i['kind']=='icecream' for i in state()['markets']['items']),'private recipe finished product')
        before=state()['markets'];checks.append('Private player completes one original recipe and creates a durable source brick through native controls')
        home.capture(a,out,'private-markets-finished');a.input('application-pause');save=Path(state()['savePath']);require(save.is_file(),'Private checkpoint absent');a.close();a=start()
        after=state()['markets'];require(not state()['shared'] and state()['zone']=='markets','Private mode/world lost')
        require(after['bricks']==before['bricks'] and len(after['items'])==1 and after['items'][0]['id']==before['items'][0]['id'],'Private Markets creation or finished product lost')
        require(all(not t['queue'] and not t['workingActor'] and not t['pendingKind'] for t in after['stations']),'Transient private station ownership survived reopen')
        checks.append('Actual private process reopening retains exact creation and finished product while clearing transient ownership')
        require(run.run_id in str(save),'Private save does not belong to this generated family')
        server=Instance.__new__(Instance);server.run=run;server.role='server';server.profile='';server.serial=server.garden_serial=0;server.identity=uuid.uuid4().hex;server.out=run.path/server.identity;server.out.mkdir()
        pair=run.path/(server.identity+'.pairing');write_record(pair,authority)
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as sock:sock.bind(('127.0.0.1',0));port=sock.getsockname()[1]
        config=run.path/(server.identity+'.config.json');write(config,dict(runId=run.run_id,instanceId=server.identity,role='server',port=port,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=False,interactive=True))
        si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
        server.process=subprocess.Popen([str(run.folder/'Server/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(server.out/'player.log'),'-batchmode','-nographics'],startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW);run.instances.append(server)
        wait(lambda:server.status() and server.status()['status']=='listening','generated family authority listening')
        wait(lambda:state()['shared'],'actual private-to-authority automatic handoff',40)
        fresh=server.state()['view']['markets'];require(not fresh['bricks'] and not fresh['items'],'Private Markets progress uploaded into authority')
        checkpoint=json.loads(save.read_text(encoding='utf-8').split('\n',2)[2])
        require(checkpoint['markets']['bricks']==before['bricks'] and checkpoint['markets']['items'][0]['id']==before['items'][0]['id'],'Automatic handoff changed private checkpoint')
        tap('Menu');tap('Play by myself');wait(lambda:not state()['shared'],'explicit private return',20)
        require(state()['markets']['bricks']==before['bricks'] and state()['markets']['items'][0]['id']==before['items'][0]['id'],'Private Markets progress did not return')
        require(not server.state()['view']['markets']['bricks'] and not server.state()['view']['markets']['items'],'Returning private changed shared progress')
        checks.append('Actual automatic authority join keeps private Markets progress separate; explicit solo return restores the saved private visit')

        passed=True
    finally:
        try:write(out/'last-client-state.json',state())
        except (RuntimeError,OSError):pass
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,checks=checks,build=args.build,runtimeErrors=errors,physicalDevices=False,ownerAcceptance=False,audioListening=False,installedFamilyTouched=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
