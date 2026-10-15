# /// script
# dependencies = ["cryptography"]
# ///
"""Actual private birthday visit, NPC band and retained save; disposable enrollment."""
import argparse, hashlib, importlib.util, json, os, shutil, socket, subprocess, sys, time, uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from family_pairing import ROOT,create_family,write_record
from shared_garden_runtime import Run,Instance,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('--art',type=Path,required=True);args=p.parse_args()
    os.environ['LITTLE_WEEPS_BIRTHDAY_ART']=str(args.art.resolve())
    run=Run(args.build,extended_test_lifetime=True);authority,players,_=create_family();slot=players[0]
    run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'hulk-birthday';out.mkdir();passed=False;checks=[];audio=None
    print('EVIDENCE '+str(out),flush=True)
    source=read(run.folder/'source-manifest.json')
    require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in source['files'] if f['path'].startswith('Unity/')),'Source differs from private candidate')
    def start():
        v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=slot['profile'];v.serial=v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
        pair=run.path/(v.identity+'.pairing');write_record(pair,slot)
        config=run.path/(v.identity+'.config.json');write(config,dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True,testAudible=True))
        si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
        v.process=subprocess.Popen([str(run.folder/'Client/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','800'],startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW)
        run.instances.append(v);wait(lambda:v.status(),'private startup');home.ready(v);return v
    def state():return a.input('inspect')
    def tap(name):
        wait(lambda:any(c['name']==name and c['enabled'] for c in state()['controls']),'private button '+name,20)
        if name.startswith('Birthday prop '):a.input('touchBirthdayProp',text=name.removeprefix('Birthday prop '))
        else:a.input('touchButton',text=name)
        home.ready(a)
    def prop():return next(p for p in state()['birthday']['props'] if p['id']=='birthday-gift-0')
    try:
        a=start();require(not state()['shared'],'Private test connected to an authority')
        a.input('fixtureTravel',text='hulk-birthday');home.ready(a)
        tap('Birthday prop birthday-gift-0');wait(lambda:prop()['holder']==a.profile,'private original pickup',20)
        tap('Open present with Hulk');wait(lambda:prop()['opened'],'private gift outcome',15)
        if prop()['content']==0:tap('Eat snack');wait(lambda:prop()['holder']=='','private consumption',10)
        else:tap('Birthday prop birthday-gift-0');wait(lambda:prop()['holder']=='','private drop')
        before=state()['birthday'];checks.append('One real private client picks up and opens its original gift with Hulk')
        # Pan the real background to the stage, then let the activity's normal
        # approach walk there. Ground taps beneath moving guests are ambiguous.
        if state()['joystickVisible']:tap('Tap to walk')
        for _ in range(4):
            a.input('touch-begin',role='screen',x=900,y=100,finger=73)
            for sx in [800,650,500,300]:a.input('touch-move',role='screen',x=sx,y=100,finger=73)
            a.input('touch-end',role='screen',x=300,y=100,finger=73);time.sleep(.2)
        tap('Play band together')
        wait(lambda:state()['birthday']['bandRoundAt']>=0,'private common birthday song',20)
        wait(lambda:sum(1 for n in state()['birthday']['cast'][:4] if n['station'] in ['keyboard','guitar','drums','dance'])>=3,'unused roles fill with original NPCs',15)
        wav=out/'actual-loopback.wav';audio=subprocess.Popen([shutil.which('ffmpeg'),'-y','-hide_banner','-loglevel','error','-f','dshow','-i','audio=virtual-audio-capturer','-t','24','-c:a','pcm_s16le',str(wav)],stdin=subprocess.PIPE,stdout=subprocess.DEVNULL,stderr=open(out/'audio-capture.log','w'))
        a.input('sandFilm',x=20)
        wait(lambda:state()['birthday']['bandRounds']>0,'private NPC band finishes',25)
        home.capture(a,out,'solo-band-with-original-companions')
        wait(lambda:(a.out/'sand-film/times.txt').exists(),'actual normal-time frame capture',25)
        audio.communicate(b'q\n',timeout=15);require(audio.returncode==0 and wav.stat().st_size>44,'Actual audio capture failed');audio=None
        times=[float(t) for t in (a.out/'sand-film/times.txt').read_text().splitlines()];frames=sorted((a.out/'sand-film').glob('*.png'));concat=out/'frames.txt'
        concat.write_text(''.join("file '"+f.as_posix()+"'\nduration "+str(times[i+1]-times[i] if i+1<len(times) else .125)+'\n' for i,f in enumerate(frames)),encoding='utf-8')
        subprocess.run([shutil.which('ffmpeg'),'-y','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-i',str(wav),'-c:v','libx264','-pix_fmt','yuv420p','-c:a','aac','-shortest',str(out/'actual-solo-band.mp4')],check=True)
        checks.append('One private player and three source companions finish one band; actual timed frames and loopback audio captured, subjective listening pending')
        a.input('application-pause');save=Path(state()['savePath']);require(save.is_file(),'Private checkpoint absent');a.close();a=start()
        after=state()['birthday'];require(not state()['shared'] and state()['zone']=='hulk-birthday','Private mode or saved world lost')
        require(after['giftCursor']==before['giftCursor'] and [p['id'] for p in after['props']]==[p['id'] for p in before['props']],'Private restart lost original prop identities or gift progress')
        require(after['bandRounds']>0 and not after['bandActors'] and after['bandRoundAt']<0,'Private restart retained invalid active band or lost completion')
        home.capture(a,out,'private-save-reopened');checks.append('Actual private process reopen retains gift progress, original objects and band completion with transient participation cleared')
        require(run.run_id in str(save),'Private save does not belong to this generated family')
        server=Instance.__new__(Instance);server.run=run;server.role='server';server.profile='';server.serial=server.garden_serial=0;server.identity=uuid.uuid4().hex;server.out=run.path/server.identity;server.out.mkdir()
        pair=run.path/(server.identity+'.pairing');write_record(pair,authority)
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as sock:sock.bind(('127.0.0.1',0));port=sock.getsockname()[1]
        config=run.path/(server.identity+'.config.json');write(config,dict(runId=run.run_id,instanceId=server.identity,role='server',port=port,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=False,interactive=True))
        si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
        server.process=subprocess.Popen([str(run.folder/'Server/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(server.out/'player.log'),'-batchmode','-nographics'],startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW);run.instances.append(server)
        wait(lambda:server.status() and server.status()['status']=='listening','generated family authority listening')
        wait(lambda:state()['shared'],'actual private-to-authority automatic handoff',40)
        fresh=server.state()['view']['birthday'];require(fresh['giftCursor']==0 and fresh['bandRounds']==0,'Private birthday progress was uploaded into authority')
        checkpoint=json.loads(save.read_text(encoding='utf-8').split('\n',2)[2])
        require(checkpoint['birthday']['giftCursor']==before['giftCursor'] and checkpoint['birthday']['bandRounds']>0,'Automatic handoff changed the private checkpoint')
        tap('Menu');tap('Play by myself');wait(lambda:not state()['shared'],'explicit private return',20)
        require(state()['birthday']['giftCursor']==before['giftCursor'] and state()['birthday']['bandRounds']>0,'Private birthday progress did not return')
        require(server.state()['view']['birthday']['giftCursor']==0 and server.state()['view']['birthday']['bandRounds']==0,'Returning private changed shared progress')
        checks.append('One real paired client automatically joins its generated authority without uploading private birthday progress; explicit solo return restores the separate private visit')

        passed=True
    finally:
        if audio is not None:audio.communicate(b'q\n',timeout=15)
        try:write(out/'last-client-state.json',state())
        except (RuntimeError,OSError):pass
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,checks=checks,build=args.build,runtimeErrors=errors,physicalDevices=False,ownerAcceptance=False,audioListening=False,installedFamilyTouched=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
