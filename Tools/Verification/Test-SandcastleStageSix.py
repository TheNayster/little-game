"""Stage6 gaps only: Move preview, actual audible cues and private/shared separation.
Synthetic lab credentials/checkpoints; no installed apps or family server operations.
"""
import argparse,copy,hashlib,importlib.util,json,shutil,socket,subprocess,sys,time,uuid
from pathlib import Path
sys.path.insert(0,str(next(p for p in Path(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT
from shared_garden_runtime import Run,Instance,read,write,wait,require
from family_pairing import create_family,write_record
s=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(s);s.loader.exec_module(home)

def verify_inputs(folder):
    for f in read(folder/'source-manifest.json')['files']:
        if not f['path'].startswith('Unity/'):continue
        data=(ROOT/f['path']).read_bytes()
        if hashlib.sha256(data).hexdigest()==f['sha256']:continue
        # Git's documented text eol rules may normalize delivery checkout bytes.
        # Only text line endings are equivalent; code/assets still must match.
        require(Path(f['path']).suffix in ('.cs','.meta','.asset') and
                hashlib.sha256(data.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')).hexdigest()==f['sha256'],'stale '+f['path'])

def button(v,name):
    for _ in range(8):
        info=v.input('inspect')
        if any(c['name']==name and c['enabled'] for c in info['controls']):break
        if not info['menuOpen']:break
        x,y=info['screenWidth']*.55,info['screenHeight']*.37
        v.input('touch-begin',role='screen',x=x,y=y,finger=97)
        for dy in (30,65,110,160):v.input('touch-move',role='screen',x=x,y=y+dy,finger=97)
        v.input('touch-end',role='screen',x=x,y=y+160,finger=97)
    wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name)
    v.input('touchButton',text=name)
def tap(v,x,y):
    for a in ('touch-begin','touch-end'):v.input(a,role='screen',x=x,y=y,finger=71)
def floor(v,x,y):
    i=v.input('inspect');r=next(c['bounds'] for c in i['controls'] if c['name']=='Build');s=r['width']/150
    cx=r['x']+r['width']/2-370*s;cy=r['y']+r['height']/2+398*s;t=(y-70)/470
    tap(v,cx+((x-4070)/730*2-1)*(685+(496-685)*t)*s,cy+(-185+385*t)*s)
def select(v,id):
    i=v.input('inspect');j=next(j for j,m in enumerate(i['sandpit']['moulds']) if m['id']==id)
    if i['sandpitSelection']==j:return
    m=i['sandpit']['moulds'][j];p=i['sandPieceScreenPoints'][j];r=next(c['bounds'] for c in i['controls'] if c['name']=='Build');s=r['width']/150;d=1.14-.28*(m['y']-70)/470
    ox=-100 if m['built'] and m['shape'] in ('wall','gate') and m['orientation']==0 else 0
    oy=60 if not m['built'] else 165 if m['shape'] in ('round','square') else 40 if m['orientation']==0 else 140
    tap(v,p['x']+ox*d*s,p['y']+oy*d*s)
    wait(lambda:v.input('inspect')['sandpitSelection']==j,'painted selection')
def place(v,shape,x,y,rotation=False):
    before={m['id'] for m in v.input('inspect')['sandpit']['moulds']}
    button(v,'Build')
    button(v,{'round':'Round tower','square':'Square tower','wall':'Wall','gate':'Gate'}[shape])
    if rotation:button(v,'Rotate sand mould')
    floor(v,x,y);button(v,'Confirm sand placement')
    return wait(lambda:next((m for m in v.input('inspect')['sandpit']['moulds'] if m['id'] not in before),None),'placed')['id']
def audio_sample(v):
    i=v.input('inspect')
    return dict(time=time.time(),events=i['sandAudioEvents'],voices=i['sandActiveVoices'],volumes=i['sandVoiceVolumes'],clips=i['sandVoiceClips'],muted=i['musicMuted'])

def finish_owned(args):
    """Recover completed assertions/frames after the documented odd-height exporter stop."""
    run=Run(args.build,resume=args.finish_of,extended_test_lifetime=True);out=run.path/'sandcastle-stage-six'
    prior=read(out/'result.json');require(prior and prior['build']==args.build and len(prior['checks'])==1 and not prior['runtimeErrors'],'owned partial result')
    phone=read(out/'phone-current-building.json');require(phone['build']==f'0.0.{args.build}' and phone['sandAudioEvents']==12 and not phone['musicMuted'],'completed audio journey capture')
    require((out/'frames.txt').exists() and (out/'actual-gameplay-loopback.wav').stat().st_size>44,'existing actual capture')
    verify_inputs(run.folder)
    subprocess.run([shutil.which('ffmpeg'),'-y','-loglevel','error','-f','concat','-safe','0','-i',str(out/'frames.txt'),'-ss','1','-i',str(out/'actual-gameplay-loopback.wav'),'-map','0:v:0','-map','1:a:0','-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-c:a','aac','-shortest',str(out/'actual-gameplay-with-audio.mp4')],check=True)
    passed=False
    try:
        server=run.start('server');run.motion_conditions={'testAudible':True};a=run.start('client',run.slots[0]['profile']);home.ready(a);require(home.command(a,32,value='start')['accepted'],'reconnect');time.sleep(1);require(a.input('inspect')['sandAudioEvents']==0,'no audible reconnect replay');home.capture(a,out,'phone-audible-reconnect');passed=True
    finally:
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in line or 'NullReference' in line]
        write(out/'finished-acceptance.json',dict(passed=passed and not errors,build=args.build,reusedMoveRun=prior['reusedMoveRun'],priorJourneyPeakActualNativeClients=2,continuationPeakActualNativeClients=1,runtimeErrors=errors,audioAssertions='Original execution reached export after all cue/rate/volume/mute/entry assertions; samples not serialized by interrupted exporter',audioEventsAfterJourney=phone['sandAudioEvents'],audioListening=False,recording='actual-gameplay-with-audio.mp4',historicalExporter='1280x591 odd height; existing frames padded by one pixel and exported, no gameplay rerun'));require(not errors,'runtime errors')
    print('PASS completed owned recording and actual audible reconnect '+str(out),flush=True)

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);ap.add_argument('--reuse-move-check');ap.add_argument('--finish-of');args=ap.parse_args()
    if args.finish_of:finish_owned(args);return
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'sandcastle-stage-six';out.mkdir();checks=[];samples=[];cue_checks=[];passed=False;capture=None
    verify_inputs(run.folder)
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');run.motion_conditions={'testAudible':True};a=run.start('client',run.slots[0]['profile']);run.motion_conditions={};b=run.start('client',run.slots[1]['profile'])
        for v in (a,b):
            home.ready(v);require(home.command(v,7,value='daycare')['accepted'],'daycare')
        button(a,'Games');button(a,'Sandcastle club')
        wait(lambda:sum(m['attending'] for m in server.state()['view']['sandpit']['members'])==2,'both auto joined')
        a.input('resize',x=1280,y=591);b.input('resize',x=1024,y=768)
        g=lambda:server.state()['view']['sandpit']
        def play(v,op,id,item='',**kw):return home.command(v,32,value=op,target=id+'@'+str(g()['round']),item=item,**kw)
        def build_fixture(v,id):
            m=next(m for m in g()['moulds'] if m['id']==id);require(home.command(v,0,x=m['x']-60,y=m['y']-55)['accepted'],'work')
            for op in ('water','scoop','scoop','scoop','tip'):require(play(v,op,id)['accepted'],op)
            time.sleep(.7)
        # Current actual input checks only the Move control change. Rules stay unchanged.
        move_cases=(('wall',False,4192.5,130),('gate',False,4362.5,130),('wall',True,4575,185),('gate',True,4745,185))
        if args.reuse_move_check:
            prior=read(ROOT/'LocalData/SharedGarden'/uuid.UUID(args.reuse_move_check).hex/'sandcastle-stage-six/result.json')
            require(prior and prior['build']==args.build and len(prior['checks'])==1 and not prior['runtimeErrors'] and prior['checks'][0].startswith('450 real phone taps'),'owned matching passed Move group')
        for index,(shape,rot,x,y) in enumerate(() if args.reuse_move_check else move_cases):
            id=place(a,shape,x,y,rot);build_fixture(a,id);select(a,id);button(a,'Edit');button(a,'Move')
            require(not any(c['name']=='Rotate sand mould' for c in a.input('inspect')['controls']),'Move has no unsupported rotation')
            floor(a,x,y+220);home.capture(a,out,'phone-move-'+shape+('-90' if rot else '-0'));button(a,'Confirm sand placement')
            wait(lambda:next(m for m in g()['moulds'] if m['id']==id)['y']>y,'move accepted')
            m=next(m for m in g()['moulds'] if m['id']==id);require(m['orientation']==(90 if rot else 0),'orientation retained')
        checks.append('450 real phone taps: new wall/gate rotations remain available; Move for both orientations matches accepted saved footprint, without misleading Rotate')
        # Capture the installed loopback route only during this bounded audible review.
        wav=out/'actual-gameplay-loopback.wav';capture=subprocess.Popen([shutil.which('ffmpeg'),'-y','-hide_banner','-loglevel','error','-f','dshow','-i','audio=virtual-audio-capturer','-t','70','-c:a','pcm_s16le',str(wav)],stdin=subprocess.PIPE,stdout=subprocess.DEVNULL,stderr=open(out/'audio-capture.log','w'))
        time.sleep(1);film_audio_offset=1;film_start=time.time();a.input('sandFilm',x=60)
        id=place(a,'round',4150,130);select(a,id);time.sleep(.8)
        def cue(name,fn,expected=True):
            if expected:wait(lambda:audio_sample(a)['voices']==0,'free audio voices');time.sleep(.2)
            before=audio_sample(a);fn();after=wait(lambda:(s if (s:=audio_sample(a))['events']>before['events'] else None),'actual '+name,10) if expected else audio_sample(a)
            for _ in range(5):samples.append(audio_sample(a));time.sleep(.04)
            if not expected:time.sleep(.7);after=audio_sample(a);require(after['events']==before['events'] and after['voices']==0,'mute suppresses '+name)
            cue_checks.append(dict(cue=name,eventsBefore=before['events'],eventsAfter=after['events'],expectedAudible=expected));time.sleep(.4)
        cue('pour',lambda:button(a,'Water'))
        cue('scoop',lambda:button(a,'Next sand tool'))
        cue('rapid taps',lambda:[a.input('touchButton',text='Next sand tool') for _ in range(8)])
        wait(lambda:next(m for m in g()['moulds'] if m['id']==id)['scoops']==3,'bounded fill')
        cue('reveal',lambda:button(a,'Tip'))
        button(a,'Decorate');button(a,'Flag');cue('decoration',lambda:button(a,'Confirm sand decoration'))
        button(a,'Dinosaur');floor(a,4470,460);wait(lambda:g()['toy']['placed'],'tap places toy');time.sleep(.5)
        cue('dinosaur',lambda:button(a,'React sand dinosaur'))
        # Two actual clients contribute together; audible client remains bounded.
        id2=place(a,'square',4320,130);select(a,id2);m=next(m for m in g()['moulds'] if m['id']==id2)
        for v in (a,b):require(home.command(v,0,x=m['x']-60,y=m['y']-55)['accepted'],'shared work')
        from concurrent.futures import ThreadPoolExecutor
        cue('simultaneous contributions',lambda:list(ThreadPoolExecutor(2).map(lambda v:play(v,'scoop',id2),(a,b))))
        samples.append(audio_sample(a));require(all(s['voices']<=2 and all(0<=v<=.22001 for v in s['volumes']) for s in samples),'bounded two-voice gain')
        button(a,'Menu');button(a,'Music setting');button(a,'Back to play');select(a,id2)
        cue('music mute',lambda:button(a,'Water'),False)
        button(a,'Menu');button(a,'Music setting');button(a,'Voice setting');button(a,'Back to play')
        cue('voice off',lambda:button(a,'Next sand tool'),False)
        button(a,'Menu');button(a,'Voice setting');button(a,'Back to play');time.sleep(.7)
        count=audio_sample(a)['events'];button(a,'Leave sandpit');button(a,'Games');button(a,'Sandcastle club');time.sleep(1)
        require(audio_sample(a)['events']==count,'entry does not replay')
        home.capture(a,out,'phone-current-building');home.capture(b,out,'tablet-current-building')
        remaining=60-(time.time()-film_start)
        if remaining>0:time.sleep(remaining+1)
        capture.communicate(b'q\n',timeout=15);require(capture.returncode==0 and wav.exists(),'actual capture route')
        times=a.out/'sand-film/times.txt';require(times.exists(),'completed real film')
        ts=[float(t) for t in times.read_text().splitlines()];frames=sorted(times.parent.glob('*.png'));concat=out/'frames.txt'
        concat.write_text(''.join("file '"+p.as_posix()+"'\nduration "+str(ts[j+1]-ts[j] if j+1<len(ts) else .125)+'\n' for j,p in enumerate(frames)))
        write(out/'audio-technical.json',dict(cues=cue_checks,samples=samples,maxVoices=max(s['voices'] for s in samples),clipVolumeLimit=.22,actualListening=False,source='bounded installed virtual-audio-capturer loopback; one audible client, other lab client muted',filmFrames=len(frames),filmAudioOffsetSeconds=film_audio_offset))
        subprocess.run([shutil.which('ffmpeg'),'-y','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-ss',str(film_audio_offset),'-i',str(wav),'-map','0:v:0','-map','1:a:0','-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-c:a','aac','-shortest',str(out/'actual-gameplay-with-audio.mp4')],check=True)
        checks.append('450 actual audio playback counters/voice gain: pour/scoop/reveal/decor/dinosaur, rapid and two-client simultaneous contributions; music/voice mute and no entry replay; actual audio-track film, listening pending')
        b.close();run.motion_conditions={'testAudible':True};a.close();a=run.start('client',run.slots[0]['profile']);home.ready(a);require(home.command(a,32,value='start')['accepted'],'reconnect');time.sleep(1);require(a.input('inspect')['sandAudioEvents']==0,'audible reconnect no replay');checks.append('450 fresh audible native reconnect has zero historical cue events')
        passed=True
    finally:
        if capture and capture.poll() is None:capture.communicate(b'q\n',timeout=15)
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in line or 'NullReference' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=args.build,checks=checks,peakActualNativeClients=2,runtimeErrors=errors,audioListening=False,reusedMoveRun=args.reuse_move_check));require(not errors,'native errors')
    print('PASS '+str(checks),flush=True)
if __name__=='__main__':main()
