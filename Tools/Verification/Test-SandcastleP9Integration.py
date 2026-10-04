"""Approved P9 production integration: four actual native clients, synthetic authority only."""
import sys, argparse, hashlib, importlib.util, time, subprocess, shutil, copy, json
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(next(p for p in Path(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT
from shared_garden_runtime import Run, read, write, wait, require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);ap.add_argument('--continue-of');args=ap.parse_args()
    run=Run(args.build,extended_test_lifetime=True,resume=args.continue_of);out=run.path/('sandcastle-p9-integration-continuation' if args.continue_of else 'sandcastle-p9-integration');out.mkdir();clients=[];checks=[];passed=False;peak=0
    prior=read(run.path/'sandcastle-p9-integration/result.json') if args.continue_of else None
    if args.continue_of:
        require(prior and prior['build']==args.build and prior['peakActualNativeClients']==4 and len(prior['checks'])==5 and not prior['runtimeErrors'],'owned checked current-source continuation only');checks=prior['checks'].copy()
    require(run.content==70 and read(run.folder/'build-summary.json')['schema']==52,'unchanged compatibility')
    for f in read(run.folder/'source-manifest.json')['files']:
        if f['path'].startswith('Unity/'):require(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'],'stale input '+f['path'])
    print('EVIDENCE '+str(out),flush=True)
    def state():return server.state()['view']['sandpit']
    def piece(id):return next(m for m in state()['moulds'] if m['id']==id)
    def cmd(v,action,**kw):return home.command(v,action,**kw)
    def play(v,op,id,item='',**kw):return cmd(v,32,value=op,target=id+'@'+str(state()['round']),item=item,**kw)
    def settled(v):return wait(lambda:(s if not (s:=v.input('inspect'))['pending'] else None),'settled')
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
    def tap(v,x,y):
        v.input('touch-begin',role='screen',x=x,y=y,finger=71);v.input('touch-end',role='screen',x=x,y=y,finger=71)
    def screen(v,x,y):
        info=v.input('inspect');r=next(c['bounds'] for c in info['controls'] if c['name']=='Build');scale=r['width']/150
        cx=r['x']+r['width']/2-370*scale;cy=r['y']+r['height']/2+398*scale
        t=(y-70)/470;half=685+(496-685)*t
        return cx+((x-4070)/730*2-1)*half*scale,cy+(-185+385*t)*scale,scale
    def tap_world(v,x,y):
        px,py,_=screen(v,x,y);tap(v,px,py)
    def select(v,id):
        info=v.input('inspect');i=next(i for i,m in enumerate(info['sandpit']['moulds']) if m['id']==id)
        if info['sandpitSelection']==i:return
        m=piece(id);_,_,s=screen(v,m['x'],m['y']);point=info['sandPieceScreenPoints'][i];depth=1.14+(.86-1.14)*(m['y']-70)/470
        # Pick opaque painted faces, rather than rectangular artwork holes.
        ox=0 if not m['built'] or m['shape'] not in ('wall','gate') else (-100 if m['orientation']==0 else 0)
        oy=60 if not m['built'] else (100 if m['shape'] in ('round','square') else 35 if m['orientation']==0 else 140)
        tap(v,point['x']+ox*depth*s,point['y']+oy*depth*s)
        wait(lambda:(j:=v.input('inspect'))['sandpitSelection']>=0 and j['sandpit']['moulds'][j['sandpitSelection']]['id']==id,'painted selection '+id)
    def capture(v,name):
        info=settled(v);essential={'Build','Decorate','Flag','Shell','Pebble','Door','Window','Dinosaur','Round tower','Square tower','Wall','Gate','Next sand tool','Water','Tip','Confirm sand placement','Rotate sand mould','Cancel sand placement','Confirm sand decoration','Cancel sand play','Leave sandpit','Optional teacher help','Edit','Family reset'}
        require(info['sandIllustrated'],'illustrated production missing')
        for c in info['controls']:
            if c['name'] in essential:
                r,s=c['bounds'],info['safeArea'];require(min(r['width'],r['height'])>=44,'tiny '+c['name']);require(r['x']>=s['x']-2 and r['y']>=s['y']-2 and r['x']+r['width']<=s['x']+s['width']+2 and r['y']+r['height']<=s['y']+s['height']+2,'clipped '+c['name'])
        home.capture(v,out,name)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def place(v,shape,col,row,rot=0,capt=None):
        before={m['id'] for m in state()['moulds']};button(v,'Build');button(v,dict(round='Round tower',square='Square tower',wall='Wall',gate='Gate')[shape])
        if rot:button(v,'Rotate sand mould')
        x,y=4150+85*col+(42.5 if shape in ('wall','gate') and rot==0 else 0),130+110*row+(55 if shape in ('wall','gate') and rot==90 else 0)
        tap_world(v,x,y)
        if capt:capture(v,capt)
        button(v,'Confirm sand placement');wait(lambda:len(state()['moulds'])>len(before),'place');settled(v)
        return next(m['id'] for m in state()['moulds'] if m['id'] not in before)
    def work(v,id):
        m=piece(id);require(cmd(v,0,x=m['x']-60,y=m['y']-55)['accepted'],'work position')
    def build(v,id):
        select(v,id);button(v,'Water');wait(lambda:piece(id)['wet'],'early water',30)
        for _ in range(3):button(v,'Next sand tool')
        wait(lambda:piece(id)['scoops']==3,'fill',30);button(v,'Tip');wait(lambda:piece(id)['built'],'tip',30);time.sleep(2)
    def decor(v,id,name):
        select(v,id);button(v,'Decorate');button(v,name);button(v,'Confirm sand decoration');wait(lambda:any(a['kind']==name.lower() for a in piece(id)['attachments']),'decoration')
    def parallel(fn,pairs):
        with ThreadPoolExecutor(len(pairs)) as pool:return list(pool.map(fn,pairs))
    try:
        server=run.start('server')
        if not args.continue_of:
            for i,slot in enumerate(run.slots):
                v=run.start('client',slot['profile']);clients.append(v);peak=len(clients);home.ready(v);v.input('resize',x=1280 if i<2 else 1024,y=591 if i<2 else 768);home.ready(v)
                require(cmd(v,1,value=('blue-pup','orange-pup','muffin','socks')[i])['accepted'],'avatar');require(cmd(v,7,value='daycare')['accepted'],'daycare')
            a,b,c,d=clients;button(a,'Games');button(a,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four joined');require(not state()['moulds'],'fresh pit must be empty')
            capture(a,'phone-normal-entry');capture(c,'tablet-normal-entry');record('Normal Daycare Games entry: empty shared pit, four admitted real profiles, no preview fixture or developer controls')
            for shape in ('wall','gate'):
                for rot in (0,90):
                    for col,row in ((0,0),(6 if rot==0 else 7,0),(0,3 if rot==0 else 2),(6 if rot==0 else 7,3 if rot==0 else 2)):
                        button(a,'Build');button(a,'Wall' if shape=='wall' else 'Gate')
                        if rot:button(a,'Rotate sand mould')
                        tap_world(a,4150+85*col+(42.5 if rot==0 else 0),130+110*row+(55 if rot==90 else 0))
                        require(any(c['name']=='Confirm sand placement' and c['enabled'] for c in a.input('inspect')['controls']),'edge projection '+shape+str((rot,col,row)))
                        button(a,'Cancel sand placement')
            record('Continuous inverse tap previews: both wall/gate orientations at all four legal edges; cancel keeps empty state')
            first=place(a,'square',1,0,capt='phone-placement-confirm');select(c,first);capture(c,'tablet-placement-active-tools')
            a.input('sandFilm',x=45);button(a,'Water');wait(lambda:piece(first)['wet'],'water-before-scoop',30);button(a,'Tip');settled(a);time.sleep(.5);require(piece(first)['scoops']==0 and not piece(first)['built'],'underfill retained')
            button(a,'Next sand tool');wait(lambda:piece(first)['scoops']==1,'first actual scoop',30);capture(a,'phone-active-building')
            wall=place(b,'wall',2,0);gate=place(c,'gate',4,0);round_id=place(d,'round',6,0)
            local=[v.input('inspect')['sandpitSelection'] for v in clients];require(len(set(local))>=3,'independent local selection')
            work(a,first);work(b,first);work(c,gate)
            before=d.input('inspect')['sandBuilderEvents'];results=parallel(lambda pair:play(pair[0],'scoop',pair[1]),[(a,first),(b,first),(c,gate)])
            require(all(r['accepted'] for r in results),'parallel contributions');wait(lambda:piece(first)['scoops']==3 and piece(gate)['scoops']==1,'shared contributions')
            info=d.input('inspect');write(out/'participant-attribution.json',info)
            require(all(info['sandBuilderEvents'][i]>before[i] for i in (0,1,2)) and info['sandBuilderEvents'][3]==before[3],'correct contributor reactions')
            results=parallel(lambda v:play(v,'tip',first),[a,b]);require(sum(r['accepted'] for r in results)==1 and piece(first)['built'],'competing tip first winner')
            after=d.input('inspect')['sandBuilderEvents'];require(sum(after)-sum(info['sandBuilderEvents'])==1,'rejected tip reaction')
            for v in clients:wait(lambda:v.input('inspect')['sandpit']['moulds']==state()['moulds'],'four same state')
            record('Parallel builds/shared scoops, independent selection, correct three-actor feedback, one winning Tip and no rejected-success reaction')
            select(b,wall);work(b,wall)
            for _ in range(7):b.input('touchButton',text='Next sand tool')
            wait(lambda:piece(wall)['scoops']==3,'bounded rapid scoops',30);button(b,'Tip');settled(b);time.sleep(.5);require(not piece(wall)['built'] and piece(wall)['scoops']==3,'full dry recovery')
            button(b,'Water');wait(lambda:piece(wall)['wet'],'dry water',30);button(b,'Tip');wait(lambda:piece(wall)['built'],'dry recovery tip',30)
            # The first scaffold gate retains one accepted scoop from another child.
            select(c,gate);button(c,'Water');wait(lambda:piece(gate)['wet'],'gate early water',30)
            for _ in range(2):button(c,'Next sand tool')
            wait(lambda:piece(gate)['scoops']==3,'remaining gate scoops',30);button(c,'Tip');wait(lambda:piece(gate)['built'],'gate tip',30);build(d,round_id)
            for name in ('Flag','Shell','Pebble','Door','Window'):decor(a,first,name)
            work(b,first);work(c,first);results=parallel(lambda v:play(v,'decorate',first,'1:flag'),[b,c]);require(sum(r['accepted'] for r in results)==1 and len(piece(first)['attachments'])==6,'competing attachment first winner')
            # Tap-based selection of back and front painted pieces on both aspects.
            select(a,gate);select(c,first);capture(a,'phone-input-built-connected-castle');capture(c,'tablet-input-built-connected-castle')
            record('Actual rapid Scoop taps bounded; underfill and full dry recovery retain sand; continued four-shape build and all five coexisting props, same-socket race reconciles')
            # Legal P9 connected layout is an explicitly authored test setup, not production startup.
            specs=[('round',1,3,0),('wall',2,3,0),('wall',4,3,0),('square',6,3,0),('wall',1,1,90),('wall',6,1,90),('square',3,2,0),('round',4,2,0),('wall',2,1,0),('wall',4,1,0)]
            for i,(shape,col,row,rot) in enumerate(specs):
                v=clients[i%4];id=place(v,shape,col,row,rot,capt=('phone-rotated-wall-edge' if i==4 else 'tablet-rotated-wall-edge' if i==5 else None));work(v,id)
                require(play(v,'water',id)['accepted'],'fixture water')
                for _ in range(3):require(play(v,'scoop',id)['accepted'],'fixture scoop')
                require(play(v,'tip',id)['accepted'],'fixture tip');require(play(v,'decorate',id,'0:flag')['accepted'] and play(v,'decorate',id,'2:shell')['accepted'],'fixture props')
            time.sleep(2);select(a,first);button(a,'Decorate');capture(a,'phone-connected-decorated-four-participants');select(c,gate);button(c,'Decorate');capture(c,'tablet-connected-decorated-four-participants')
            # Both gate orientations and every edge are covered by matching P9 pure projection evidence;
            # this integration separately exercises continuous input and versioned move near a free edge.
            button(a,'Dinosaur');tap_world(a,4150,180);wait(lambda:state()['toy']['placed'],'tap toy');reaction=state()['toy']['reaction'];button(c,'React sand dinosaur');wait(lambda:state()['toy']['reaction']>reaction,'shared toy reaction')
            capture(a,'phone-shared-toy-play');capture(c,'tablet-shared-toy-play');record('Legal connected 14-piece production castle with four clients, depth selection and tap-place/shared dinosaur reaction')
        else:
            require(len(state()['moulds'])==14,'owned connected creation')
            for i,slot in enumerate(run.slots):
                v=run.start('client',slot['profile']);clients.append(v);peak=len(clients);home.ready(v);v.input('resize',x=1280 if i<2 else 1024,y=591 if i<2 else 768);home.ready(v)
                if not next(m for m in state()['members'] if m['actor']==v.profile)['attending']:require(cmd(v,32,value='start')['accepted'],'continuation join')
            a,b,c,d=clients;first=next(m['id'] for m in state()['moulds'] if m['x']==4235 and m['y']==130);gate=next(m['id'] for m in state()['moulds'] if m['shape']=='gate')
            select(a,first);button(a,'Decorate');select(c,gate);button(c,'Decorate');capture(a,'phone-connected-decorated-four-participants');capture(c,'tablet-connected-decorated-four-participants')
            a.input('sandFilm',x=30)
        # Keep the placement-race site clear: the toy intentionally blocks nearby pieces.
        button(a,'Decorate');button(a,'Dinosaur');tap_world(a,4096,350);wait(lambda:abs(state()['toy']['x']-4096)<1,'toy moved to clear rim');settled(a)
        button(a,'Cancel sand play') if any(c['name']=='Cancel sand play' and c['enabled'] for c in a.input('inspect')['controls']) else None
        select(a,first);button(a,'Edit');capture(a,'phone-owner-edit');button(a,'Move');tap_world(a,4150,130);capture(a,'phone-owner-move-preview');button(a,'Confirm sand placement');wait(lambda:piece(first)['x']==4150,'owner inverse move');require(len(piece(first)['attachments'])==6,'move kept props');button(a,'Edit');button(a,'Move');tap_world(a,4235,130);button(a,'Confirm sand placement');wait(lambda:piece(first)['x']==4235,'restore owner move')
        require(not play(b,'remove',first,str(piece(first)['version']))['accepted'],'sibling remove blocked')
        old=piece(first)['version'];require(play(a,'decor-remove',first,str(old)+':1')['accepted'],'free one socket');require(play(b,'decorate',first,'1:flag')['accepted'],'intervening contribution');before=copy.deepcopy(state()['moulds']);require(not play(a,'remove',first,str(old))['accepted'] and state()['moulds']==before,'stale owner edit protection')
        # Competing placement in the same otherwise free lattice location.
        results=parallel(lambda v:cmd(v,32,value='place',target='place@'+str(state()['round']),item='round',x=4150,y=130),[a,b]);require(sum(r['accepted'] for r in results)==1,'competing placement first winner');extra=next(m for m in state()['moulds'] if m['x']==4150 and m['y']==130)
        owner=next(v for v in clients if v.profile==extra['creator']);select(owner,extra['id']);button(owner,'Edit');button(owner,'Remove');capture(owner,'confirmed-removal-dialog');button(owner,'Keep it');require(any(m['id']==extra['id'] for m in state()['moulds']),'cancel kept piece');button(owner,'Remove');button(owner,'Yes, this only');wait(lambda:len(state()['moulds'])==14,'one confirmed removal')
        button(b,'Leave sandpit');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'independent leave');require(state()['moulds']==before,'leave retains shared castle');button(b,'Games');button(b,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'independent rejoin');require(all(x=='ready' for x in b.input('inspect')['sandBuilderReactions']),'rejoin no historical reaction')
        require(cmd(d,7,value='park')['accepted'],'other-world voter');button(a,'Family reset');capture(a,'phone-reset-consent');capture(d,'tablet-other-world-reset-consent');button(a,'Clear sandpit');require(len(state()['moulds'])==14,'partial consent kept castle');button(d,'Keep castle');wait(lambda:not state()['reset']['token'],'other-world decline');require(state()['moulds']==before,'decline kept castle')
        require(cmd(d,7,value='daycare')['accepted'] and cmd(d,32,value='start')['accepted'],'return fourth')
        record('Reachable creator editing and confirmation/cancellation; sibling/stale owner edit blocked; competing placement; independent Leave/re-entry; reset includes connected Park voter and decline/partial consent preserves castle')
        # Full16 remains useful, with one intentionally partial bucket retained for reopening.
        partial=place(a,'round',0,0);work(a,partial);require(play(a,'scoop',partial)['accepted'],'partial save')
        last=place(b,'round',7,0);work(b,last);require(play(b,'water',last)['accepted'],'last water')
        for _ in range(3):require(play(b,'scoop',last)['accepted'],'last scoop')
        require(play(b,'tip',last)['accepted'],'last tip');time.sleep(2);button(a,'Build');button(c,'Build');capture(a,'phone-full-sixteen');capture(c,'tablet-full-sixteen');require(len(state()['moulds'])==16,'cap')
        expected=copy.deepcopy(state()['moulds']);toy=copy.deepcopy(state()['toy']);run.close();server=run.start('server');require(state()['moulds']==expected and state()['toy']==toy,'authority save reopen')
        a=run.start('client',run.slots[0]['profile']);clients=[a];home.ready(a);a.input('resize',x=1280,y=591);require(cmd(a,32,value='start')['accepted'],'reopened join');home.ready(a);capture(a,'phone-reopened-real-creation');info=a.input('inspect');require(not any(info['sandBuilderEvents']) and all(r=='ready' for r in info['sandBuilderReactions']) and info['sandActiveEffects']==0,'reopen no historical feedback');require(info['sandAudioEvents']==0,'test mute')
        record('Full16 phone/tablet controls, current authority save reopen retains all coordinates/shapes/owners/progress/props/toy; fresh client has no historical reaction or sound')
        for slot in run.slots[1:]:
            v=run.start('client',slot['profile']);clients.append(v);home.ready(v);require(cmd(v,32,value='start')['accepted'],'reopened sibling join')
        a,b,c,d=clients;require(cmd(d,7,value='park')['accepted'],'reopened other-world voter');unrelated=copy.deepcopy(server.state()['view']['toys']);epoch=state()['round'];button(a,'Family reset')
        for v in clients:button(v,'Clear sandpit')
        wait(lambda:state()['round']==epoch+1 and not state()['moulds'],'all connected explicitly agreed');require(server.state()['view']['toys']==unrelated and not state()['toy']['placed'],'reset scoped');capture(a,'phone-unanimous-reset-empty');require(not cmd(a,32,value='place',target='place@'+str(epoch),item='round',x=4150,y=130)['accepted'],'old epoch rejection')
        record('All four explicit reset agreements including Park clear only the shared sandcastle; continued empty controls and old-epoch rejection')
        film=(next(p for p in run.path.glob('*/sand-film') if (p/'times.txt').exists()) if args.continue_of else run.instances[1].out/'sand-film');wait(lambda:(film/'times.txt').exists(),'film complete',50);times=[float(t) for t in (film/'times.txt').read_text().splitlines()];frames=sorted(film.glob('*.png'));require(len(times)==len(frames)>50,'actual film frames')
        concat=out/'film-input.txt';lines=[]
        for i,frame in enumerate(frames):lines.extend(["file '"+frame.as_posix()+"'",'duration '+str(times[i+1]-times[i] if i+1<len(times) else .125)])
        concat.write_text('\n'.join(lines),encoding='utf8');subprocess.run([shutil.which('ffmpeg'),'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/'actual-integrated-gameplay-silent.mp4')],check=True)
        passed=True
    finally:
        if not passed:
            for i,v in enumerate(clients):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+str(i))
                    except Exception:pass
        run.close();errors=[]
        for v in run.instances:
            errors.extend(line for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in line or 'NullReference' in line)
        write(out/'result.json',dict(passed=passed and not errors,build=args.build,runId=run.run_id,reusedCurrentSourceGroups=len(prior['checks']) if prior else 0,checks=checks,peakActualNativeClients=peak,nativeProcesses=len(run.instances),runtimeErrors=errors,audioListening=False,recording='actual-integrated-gameplay-silent.mp4'))
        require(not errors,'runtime errors '+str(errors[:3]))
if __name__=='__main__':main()
