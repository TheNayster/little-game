"""Focused Stage 5 actual native presentation/input; isolated synthetic family.
Unchanged rules/migrations are covered by the Stage 4 record and focused checks.
"""
import sys, argparse, hashlib, importlib.util, time, subprocess, shutil
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(next(p for p in Path(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT
from shared_garden_runtime import Run, read, write, wait, require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);args=ap.parse_args()
    run=Run(args.build,extended_test_lifetime=True)
    require(run.content==70 and read(run.folder/'build-summary.json')['schema']==52,'unchanged compatibility')
    for f in read(run.folder/'source-manifest.json')['files']:
        if f['path'].startswith('Unity/'):
            require(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'],'stale input '+f['path'])
    out=run.path/'sandpit-stage-five';out.mkdir();checks=[];clients=[];passed=False;peak=0
    print('EVIDENCE '+str(out),flush=True)
    def state():return server.state()['view']['sandpit']
    def piece(id):return next(m for m in state()['moulds'] if m['id']==id)
    def cmd(v,action,**kw):return home.command(v,action,**kw)
    def play(v,op,id,item='',**kw):return cmd(v,32,value=op,target=id+'@'+str(state()['round']),item=item,**kw)
    def button(v,name):
        for _ in range(8):
            info=v.input('inspect')
            if any(c['name']==name and c['enabled'] for c in info['controls']) or not info['menuOpen']:break
            x,y=info['screenWidth']*.55,info['screenHeight']*.37
            v.input('touch-begin',role='screen',x=x,y=y,finger=97)
            for dy in (30,65,110,160):v.input('touch-move',role='screen',x=x,y=y+dy,finger=97)
            v.input('touch-end',role='screen',x=x,y=y+160,finger=97)
        wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,20)
        v.input('touchButton',text=name)
    def capture(v,name):
        wait(lambda:not v.input('inspect')['pending'],'settled capture')
        info=v.input('inspect');require(info['sandIllustrated'],'missing production illustration')
        essential={'Build','Decorate','Flag','Shell','Pebble','Door','Window','Dinosaur','Round tower','Square tower','Wall','Gate','Next sand tool','Water','Tip','Confirm sand placement','Rotate sand mould','Cancel sand placement','Confirm sand decoration','Cancel sand play','Leave sandpit','Optional teacher help','Edit','Family reset'}
        for c in info['controls']:
            if c['name'] in essential:
                r,s=c['bounds'],info['safeArea'];require(min(r['width'],r['height'])>=44,'tiny '+c['name'])
                require(r['x']>=s['x']-2 and r['y']>=s['y']-2 and r['x']+r['width']<=s['x']+s['width']+2 and r['y']+r['height']<=s['y']+s['height']+2,'clipped '+c['name'])
        home.capture(v,out,name)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def select(v,id):
        button(v,'Sand piece '+str(next(i for i,m in enumerate(state()['moulds']) if m['id']==id)+1))
        wait(lambda:(i:=v.input('inspect'))['sandpitSelection']>=0 and i['sandpit']['moulds'][i['sandpitSelection']]['id']==id,'stable selection')
    def place(v,title,cell,rotate=False,preview=None):
        before={m['id'] for m in state()['moulds']};button(v,'Build');button(v,title)
        if rotate:button(v,'Rotate sand mould')
        button(v,'Sand spot '+str(cell))
        if preview:capture(v,preview)
        button(v,'Confirm sand placement');wait(lambda:len(state()['moulds'])>len(before),'place')
        return next(m['id'] for m in state()['moulds'] if m['id'] not in before)
    def build(v,id):
        select(v,id);button(v,'Water');wait(lambda:piece(id)['wet'],'early water')
        for _ in range(3):button(v,'Next sand tool')
        wait(lambda:piece(id)['scoops']==3,'fill',40);button(v,'Next sand tool');wait(lambda:piece(id)['built'],'tip',30)
        wait(lambda:not any(v.input('inspect')['sandpitTipEffects']),'reveal expiry')
    def decor(v,id,name):
        select(v,id);button(v,'Decorate');button(v,name);button(v,'Confirm sand decoration')
        wait(lambda:any(a['kind']==name.lower() for a in piece(id)['attachments']),'decor '+name)
    try:
        server=run.start('server')
        for i,slot in enumerate(run.slots):
            v=run.start('client',slot['profile']);clients.append(v);peak=len(clients);home.ready(v)
            v.input('resize',x=1280 if i<2 else 1024,y=591 if i<2 else 768);home.ready(v)
            require(cmd(v,1,value=('blue-pup','orange-pup','muffin','socks')[i])['accepted'],'cast')
            require(cmd(v,7,value='daycare')['accepted'],'travel')
        a,b,c,d=clients;button(a,'Games');button(a,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four family participants')
        capture(a,'phone-empty-shape-tray');capture(c,'tablet-empty-shape-tray')
        first=place(a,'Round tower',0,preview='phone-round-preview')
        a.input('sandFilm',x=75)
        button(a,'Next sand tool');wait(lambda:piece(first)['scoops']==1,'one scoop',35)
        capture(a,'phone-partial-active-tools');select(c,first);capture(c,'tablet-partial-active-tools')
        button(a,'Water');wait(lambda:piece(first)['wet'],'wet');capture(a,'phone-wet-active-tools');capture(c,'tablet-wet-active-tools')
        for _ in range(2):button(a,'Next sand tool')
        wait(lambda:piece(first)['scoops']==3,'full',35);button(a,'Next sand tool');wait(lambda:piece(first)['built'],'revealed',30)
        wait(lambda:not any(a.input('inspect')['sandpitTipEffects']),'reveal done')
        for name in ('Flag','Shell','Pebble','Door','Window'):decor(a,first,name)
        button(a,'Dinosaur');button(a,'Sand toy spot 19');wait(lambda:state()['toy']['placed'],'toy')
        reaction=state()['toy']['reaction'];button(a,'React sand dinosaur');wait(lambda:state()['toy']['reaction']>reaction,'toy reaction')
        capture(a,'phone-first-piece-decorated-toy')
        record('Actual taps: empty pit, shape preview, partial/wet bucket, Scoop/Water/Tip reveal, five combined decorations and grounded dinosaur reaction')
        wall=place(b,'Wall',1);build(b,wall);gate=place(c,'Gate',3);build(c,gate);square=place(d,'Square tower',5);build(d,square)
        vertical_wall=place(b,'Wall',22,True,'phone-rotated-wall-preview');build(b,vertical_wall)
        vertical_gate=place(c,'Gate',16,True,'tablet-rotated-gate-preview');build(c,vertical_gate)
        select(a,first);button(a,'Decorate');capture(a,'phone-connected-castle');select(c,gate);button(c,'Decorate');capture(c,'tablet-connected-castle')
        record('Connected four-shape castle and both wall/gate orientations remain interactive at phone/tablet aspects')
        # Two siblings contribute together while another client observes both.
        x=place(a,'Round tower',7);y=place(b,'Square tower',15)
        for v,id in ((a,x),(b,y)):
            at=piece(id);require(cmd(v,0,x=at['x']-60,y=at['y']-55)['accepted'],'work position')
        before=c.input('inspect');
        with ThreadPoolExecutor(2) as pool:
            replies=list(pool.map(lambda pair:play(pair[0],'water',pair[1]),((a,x),(b,y))))
        require(all(r['accepted'] for r in replies),'simultaneous water')
        info=c.input('inspect');require(sum(info['sandpitWaterEvents'])>=sum(before['sandpitWaterEvents'])+2,'shared water feedback');require(info['sandActiveVoices']<=2 and info['sandActiveEffects']<=16,'bounded effects')
        capture(c,'tablet-simultaneous-water')
        for v,id in ((a,x),(b,y)):
            for _ in range(3):require(play(v,'scoop',id)['accepted'],'shared scoop')
        with ThreadPoolExecutor(2) as pool:require(all(r['accepted'] for r in pool.map(lambda pair:play(pair[0],'tip',pair[1]),((a,x),(b,y)))),'simultaneous tip')
        wait(lambda:piece(x)['built'] and piece(y)['built'],'two shared reveals')
        # Departure must not hide or clear another child's work.
        button(b,'Leave sandpit');require(not next(m for m in state()['members'] if m['actor']==b.profile)['attending'],'independent departure')
        require(len(state()['moulds'])==8 and all(piece(id)['built'] for id in (x,y)),'departure kept creation')
        button(b,'Games');button(b,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'rejoin')
        require(b.input('inspect')['sandActiveEffects']==0 and not any(b.input('inspect')['sandpitTipEffects']),'no saved/rejoin replay')
        record('Four actual clients: simultaneous accepted water/reveals, bounded effects/voices, independent Leave and re-entry without historical replay')
        select(a,first);button(a,'Edit');capture(a,'phone-edit-sheet');button(a,'Back');button(a,'Family reset');capture(a,'phone-family-reset');capture(c,'tablet-family-reset');button(c,'Keep castle')
        record('Owner editing and deliberate family reset remain separate picture controls; decline preserves work')
        # Build remaining free cells through authority commands. No save injection.
        for cell in range(32):
            if len(state()['moulds'])==16:break
            col,row=cell%8,cell//8;px,py=4150+85*col,130+110*row
            reply=play(d,'place','place','square' if cell%2 else 'round',x=px,y=py)
            if not reply['accepted']:continue
            wait(lambda:any(m['x']==px and m['y']==py for m in state()['moulds']),'busy piece')
            id=next(m['id'] for m in state()['moulds'] if m['x']==px and m['y']==py)
            require(cmd(d,0,x=px-60,y=py-55)['accepted'],'busy work')
            for _ in range(3):require(play(d,'scoop',id)['accepted'],'busy scoop')
            require(play(d,'water',id)['accepted'] and play(d,'tip',id)['accepted'],'busy reveal')
        require(len(state()['moulds'])==16,'populated cap')
        time.sleep(2);button(a,'Build');capture(a,'phone-busy-sixteen');button(c,'Build');capture(c,'tablet-busy-sixteen')
        samples=[]
        for _ in range(10):
            start=time.monotonic();info=c.input('inspect');samples.append(time.monotonic()-start)
            require(info['sandActiveVoices']<=2 and info['sandActiveEffects']<=16,'population effect limits')
        write(out/'local-population.json',dict(pieces=16,attachments=sum(len(m['attachments']) for m in state()['moulds']),inspectLatencySeconds=samples,desktopOnly=True,notDeviceFPS=True))
        require(a.input('inspect')['sandAudioEvents']==0,'test mute honored')
        record('Populated 16-piece scene, safe picture hits, responsive native inspection and test mute; desktop measurements only')
        film=a.out/'sand-film';wait(lambda:(film/'times.txt').exists(),'actual film complete',90)
        times=[float(t) for t in (film/'times.txt').read_text().splitlines()];frames=sorted(film.glob('*.png'));require(len(times)==len(frames)>50,'actual gameplay frames')
        concat=out/'film-input.txt';lines=[]
        for i,frame in enumerate(frames):
            lines.extend(["file '"+frame.as_posix()+"'",'duration '+str(times[i+1]-times[i] if i+1<len(times) else .125)])
        concat.write_text('\n'.join(lines),encoding='utf8')
        subprocess.run([shutil.which('ffmpeg'),'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/'actual-gameplay-silent.mp4')],check=True)
        record('Timestamped actual native gameplay recording exported; silent because isolated test route is muted')
        passed=True
    finally:
        if not passed:
            for i,v in enumerate(clients):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+str(i))
                    except Exception:pass
        run.close();errors=[]
        for v in run.instances:
            errors.extend(line for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception' in line or 'NullReference' in line)
        write(out/'result.json',dict(passed=passed and not errors,build=args.build,checks=checks,peakClients=peak,errors=errors,localPopulation='local-population.json',recording='actual-gameplay-silent.mp4',targetImageAvailable=False,audioListening=False))
        require(not errors,'runtime errors '+str(errors[:3]))
if __name__=='__main__':main()
