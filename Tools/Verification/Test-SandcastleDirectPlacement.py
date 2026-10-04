"""Direct placement in the integrated production activity; isolated four-client authority."""
import sys, argparse, importlib.util, time, json, copy, subprocess, shutil
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run, read, write, wait, require
from parent_server import checkpoint_bytes
spec=importlib.util.spec_from_file_location('six', Path(__file__).with_name('Test-SandcastleStageSix.py'))
six=importlib.util.module_from_spec(spec);spec.loader.exec_module(six)
home=six.home


def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);args=ap.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'sandcastle-direct-placement';out.mkdir()
    six.verify_inputs(run.folder);clients=[];checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def state():return server.state()['view']['sandpit']
    def piece(id):return next(m for m in state()['moulds'] if m['id']==id)
    def button(v,name):six.button(v,name)
    def tap(v,x,y,finger=71):
        for action in ('touch-begin','touch-end'):v.input(action,role='screen',x=x,y=y,finger=finger)
    def screen(v,x,y):
        info=v.input('inspect');r=next(c['bounds'] for c in info['controls'] if c['name']=='Build');s=r['width']/150
        cx=r['x']+r['width']/2-370*s;cy=r['y']+r['height']/2+398*s;t=(y-70)/470
        return cx+((x-4070)/730*2-1)*(685-189*t)*s,cy+(-185+385*t)*s
    def floor(v,x,y):tap(v,*screen(v,x,y))
    def settled(v):return wait(lambda:(s if not (s:=v.input('inspect'))['pending'] and not s['sandLocalPending'] else None),'settled')
    def controlpoint(v,name):
        r=next(c['bounds'] for c in v.input('inspect')['controls'] if c['name']==name);return r['x']+r['width']/2,r['y']+r['height']/2
    def capture(v,name):
        info=settled(v);require(info['sandIllustrated'],'production P9 required')
        require(not any(c['name'].startswith('Sand attachment ') or c['name']=='Confirm sand decoration' for c in info['controls']),'old decoration controls')
        home.capture(v,out,name)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def choose(v,shape,rotate=False):
        button(v,'Build');button(v,dict(round='Round tower',square='Square tower',wall='Wall',gate='Gate')[shape])
        if rotate:button(v,'Rotate sand mould')
        i=v.input('inspect');require(i['sandPlacementActive'],'armed mould');require(not any(c['name']=='Confirm sand placement' for c in i['controls']),'routine checkmark present')
    def place(v,shape,x,y,rotate=False):
        before={m['id'] for m in state()['moulds']};choose(v,shape,rotate);floor(v,x,y)
        m=wait(lambda:next((m for m in state()['moulds'] if m['id'] not in before),None),'direct bucket')
        i=settled(v);require(not i['sandPlacementActive'] and i['sandpit']['moulds'][i['sandpitSelection']]['id']==m['id'],'new bucket selected/tools')
        return m['id']
    def build(v,id):
        six.select(v,id);button(v,'Next sand tool');wait(lambda:piece(id)['scoops']==1,'scoop',30)
        button(v,'Next sand tool');button(v,'Next sand tool');wait(lambda:piece(id)['scoops']==3,'bounded scoop',30)
        button(v,'Water');wait(lambda:piece(id)['wet'],'water',30)
        button(v,'Tip');wait(lambda:piece(id)['built'],'tip',30);settled(v)
    def decoration(v,id,kind,slot):
        if v.input('inspect')['sandDecorationChoice']:button(v,'Build')
        six.select(v,id);button(v,'Decorate');button(v,kind.title())
        i=v.input('inspect');p=i['sandAttachmentScreenPoints'][slot];tap(v,p['x'],p['y'])
        wait(lambda:any(a['slot']==slot and a['kind']==kind for a in piece(id)['attachments']),'direct '+kind)
        require(settled(v)['sandDecorationChoice']==kind,'decoration remains chosen')
    try:
        server=run.start('server')
        for j,slot in enumerate(run.slots):
            v=run.start('client',slot['profile']);clients.append(v);home.ready(v)
            v.input('resize',x=1280 if j<2 else 1024,y=592 if j<2 else 768);home.ready(v)
            require(home.command(v,1,value=('blue-pup','orange-pup','muffin','socks')[j])['accepted'],'avatar')
            require(home.command(v,7,value='daycare')['accepted'],'daycare')
        a,b,c,d=clients;button(a,'Games');button(a,'Sandcastle club')
        wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four participants')
        capture(a,'phone-empty');capture(c,'tablet-empty')
        a.input('sandFilm',x=45)
        first=place(a,'square',4235,130);capture(a,'phone-direct-bucket-tools')
        before=copy.deepcopy(state()['moulds']);floor(a,4490,130);require(state()['moulds']==before,'second sand tap created bucket')
        build(a,first);capture(a,'phone-built-through-input')
        decoration(a,first,'flag',0);capture(a,'phone-direct-decoration')
        p=a.input('inspect')['sandAttachmentScreenPoints'][1];tap(a,p['x'],p['y'])
        wait(lambda:len(piece(first)['attachments'])==2,'intentional repeated flag')
        settled(a);before=copy.deepcopy(piece(first)['attachments']);tap(a,p['x'],p['y']);require(a.input('inspect')['sandDirectCueVisible'],'full socket cue visible');settled(a)
        require(piece(first)['attachments']==before,'occupied sockets overwritten')
        capture(a,'phone-full-flag-gentle-cue')
        decoration(a,first,'shell',2);capture(a,'phone-combined-decoration')
        # Actual decoration art follows a tray drag; cancellation is transaction-free.
        previous=copy.deepcopy(piece(first)['attachments']);sx,sy=controlpoint(a,'Shell');anchor=a.input('inspect')['sandAttachmentScreenPoints'][3]
        a.input('touch-begin',role='screen',x=sx,y=sy,finger=81)
        a.input('touch-move',role='screen',x=anchor['x'],y=anchor['y'],finger=81)
        capture(a,'phone-decoration-drag-preview')
        a.input('touch-cancel',role='screen',x=anchor['x'],y=anchor['y'],finger=81)
        require(piece(first)['attachments']==previous,'cancelled decoration drag changed work')
        button(a,'Shell');sx,sy=controlpoint(a,'Shell')
        a.input('touch-begin',role='screen',x=sx,y=sy,finger=82)
        a.input('touch-move',role='screen',x=anchor['x'],y=anchor['y'],finger=82)
        a.input('touch-end',role='screen',x=anchor['x'],y=anchor['y'],finger=82)
        wait(lambda:any(q['slot']==3 for q in piece(first)['attachments']),'direct decoration drag');settled(a)
        require(len(piece(first)['attachments'])==len(previous)+1,'decoration drag sent extra commands')
        capture(a,'phone-combined-decoration-dragged')
        record('Phone: shape/tap creates and selects bucket; Scoop/Water/Tip; direct repeated flags/shell; full sockets cue without overwrite')
        second=place(b,'round',4490,130);third=place(c,'wall',4277.5,350);fourth=place(d,'gate',4660,295,True)
        for v,id in ((b,second),(c,third),(d,fourth)):build(v,id)
        decoration(b,second,'pebble',2);decoration(c,third,'door',4);decoration(d,fourth,'window',4)
        capture(c,'tablet-built-through-input');capture(d,'tablet-direct-decoration')
        record('Four actual clients directly place/build/decorate independent round/square/wall/rotated-gate pieces in one creation')
        # Invalid snapped requests keep every existing piece and do not search
        # for an alternative destination. Raw boundary taps also reject safely.
        before=copy.deepcopy(state()['moulds']);choose(a,'round');floor(a,4235,130);settled(a)
        require(state()['moulds']==before,'occupied placement relocated or damaged work');capture(a,'phone-occupied-cue')
        button(a,'Cancel sand placement');choose(c,'wall');floor(c,4787.5,460);settled(c)
        require(state()['moulds']==before,'out-of-bounds placement changed work');capture(c,'tablet-boundary-cue');button(c,'Cancel sand placement')
        record('Occupied and boundary direct taps preserve existing work with local feedback')
        # A cancelled surface gesture is not a click. Tray-origin movements
        # released over the drawer or another UI control are never sand taps.
        choose(a,'round');px,py=screen(a,4320,240)
        a.input('touch-begin',role='screen',x=px,y=py,finger=72)
        a.input('touch-move',role='screen',x=px+25,y=py+10,finger=72)
        a.input('touch-cancel',role='screen',x=px+25,y=py+10,finger=72)
        require(state()['moulds']==before,'cancelled gesture placed');button(a,'Cancel sand placement')
        def controlpoint(v,name):
            r=next(c['bounds'] for c in v.input('inspect')['controls'] if c['name']==name);return r['x']+r['width']/2,r['y']+r['height']/2
        button(a,'Build');tx,ty=controlpoint(a,'Round tower')
        a.input('touch-begin',role='screen',x=tx,y=ty,finger=73)
        a.input('touch-move',role='screen',x=tx+50,y=ty,finger=73)
        a.input('touch-end',role='screen',x=tx+50,y=ty,finger=73)
        require(state()['moulds']==before,'tray swipe placed')
        tx,ty=controlpoint(a,'Round tower');ux,uy=controlpoint(a,'Build')
        a.input('touch-begin',role='screen',x=tx,y=ty,finger=74)
        a.input('touch-move',role='screen',x=px,y=py,finger=74)
        capture(a,'phone-optional-drag-preview')
        a.input('touch-move',role='screen',x=ux,y=uy,finger=74)
        a.input('touch-end',role='screen',x=ux,y=uy,finger=74)
        require(state()['moulds']==before,'UI release placed')
        record('Cancelled sand gestures, tray swipe and UI drag release submit no placement')
        # The optional drag route must complete, with exactly one transaction.
        tx,ty=controlpoint(a,'Round tower');px,py=screen(a,4320,240)
        a.input('touch-begin',role='screen',x=tx,y=ty,finger=75)
        a.input('touch-move',role='screen',x=px,y=py,finger=75)
        a.input('touch-end',role='screen',x=px,y=py,finger=75)
        extra=wait(lambda:next((m for m in state()['moulds'] if m['id'] not in {m['id'] for m in before}),None),'optional drag placed')
        require(len(state()['moulds'])==len(before)+1 and not settled(a)['sandPlacementActive'],'drag transaction count')
        record('Optional tray drag submits one bucket, using the same captured snap as taps')
        # Destructive controls retain their explicit safeguards.
        button(a,'Build');six.select(a,first);button(a,'Edit');button(a,'Remove')
        require(any(q['name']=='Yes, this only' for q in a.input('inspect')['controls']),'destructive confirmation missing')
        capture(a,'phone-destructive-confirmation-retained');button(a,'Keep it');button(a,'Back')
        expected=copy.deepcopy(state()['moulds']);button(a,'Family reset');wait(lambda:bool(state()['reset']['token']),'reset consent')
        require(len(state()['reset']['voters'])==4,'family reset voters');button(c,'Keep castle')
        wait(lambda:not state()['reset']['token'],'reset declined');require(state()['moulds']==expected,'reset decline changed work')
        record('Removal and family reset still require explicit consent; cancellation keeps the castle')
        # Rapid taps after the accepted drop cannot repeat an armed mould.
        old=len(state()['moulds']);choose(b,'round');px,py=screen(b,4405,350)
        for _ in range(3):tap(b,px,py,finger=76)
        wait(lambda:len(state()['moulds'])==old+1,'rapid placement');settled(b)
        require(len(state()['moulds'])==old+1,'rapid taps duplicated bucket')
        newest=next(m for m in state()['moulds'] if m['x']==4405 and m['y']==350)
        rid=newest['id'][2:];raw,_=checkpoint_bytes(run.path/'server-world/world.save');saved=json.loads(raw.decode('utf-8-sig').split('\n',2)[2]);receipt=next(r for r in saved['receipts'] if r['requestId']==rid)
        parts=receipt['fingerprint'].split('|')
        duplicate=dict(requestId=rid,actor=parts[0],item=parts[1],target=parts[2],value=parts[3],action=int(parts[4]),expectedRevision=int(parts[5]),x=float(parts[6]),y=float(parts[7]),zone=parts[8],visit=int(parts[9]))
        b.serial+=1;write(b.out/'control.json',dict(serial=b.serial,kind='command',request=dict(requestId=rid,protocol=3,command=duplicate)))
        wait(lambda:(v if (v:=read(b.out/('reply-'+rid+'.json'))) and v['duplicate'] else None),'duplicate network delivery')
        require(len(state()['moulds'])==old+1,'network retry duplicated bucket')
        record('Rapid taps create one bucket; exact acknowledged command redelivery is duplicate-safe')
        # Both users intentionally target the same open region. Depending on
        # snapshot arrival this rejects one or assigns two distinct empty slots.
        for v in (b,c):button(v,'Build');six.select(v,first);button(v,'Decorate');button(v,'Window')
        target=b.input('inspect')['sandAttachmentScreenPoints'][5];target_c=c.input('inspect')['sandAttachmentScreenPoints'][5]
        previous=copy.deepcopy(piece(first)['attachments'])
        with ThreadPoolExecutor(2) as pool:
            list(pool.map(lambda q:tap(q[0],q[1]['x'],q[1]['y']),((b,target),(c,target_c))))
        settled(b);settled(c);after=piece(first)['attachments']
        require(all(a in after for a in previous) and len({x['slot'] for x in after})==len(after) and len(after)<=len(previous)+2,'competing decoration lost work')
        for v in clients:require(v.state()['view']['sandpit']['moulds']==state()['moulds'],'shared creation diverged')
        capture(a,'phone-shared-result');capture(c,'tablet-shared-result')
        record('Competing direct decorations never overwrite; all four clients converge')
        # Selection/armed local state must disappear across independent exit.
        choose(d,'wall');button(d,'Leave sandpit');wait(lambda:not next(m for m in state()['members'] if m['actor']==d.profile)['attending'],'independent leave')
        button(d,'Games');button(d,'Sandcastle club');require(not settled(d)['sandPlacementActive'] and not d.input('inspect')['sandDecorationChoice'],'stale placement after re-entry')
        expected=copy.deepcopy(state()['moulds']);home.command(d,7,value='park');home.ready(d);home.command(d,7,value='daycare');home.command(d,32,value='start');home.ready(d)
        require(state()['moulds']==expected and not d.input('inspect')['sandPlacementActive'],'activity change lost work/intent')
        record('Leave/re-entry and activity changes clear local placement without changing the shared castle')
        choose(d,'round');d.close();wait(lambda:len(server.state()['connected'])==3,'native disconnect')
        returning=run.start('client',d.profile);clients[-1]=returning;home.ready(returning)
        require(home.command(returning,32,value='start')['accepted'],'return after disconnect');home.ready(returning)
        require(not returning.input('inspect')['sandPlacementActive'] and state()['moulds']==expected,'disconnect retained stale intent/lost creation')
        record('Actual disconnect/reconnect clears local intent while three siblings keep their creation')
        wait(lambda:(a.out/'sand-film/times.txt').exists(),'actual gameplay recording',60)
        ts=[float(t) for t in (a.out/'sand-film/times.txt').read_text().splitlines()];frames=sorted((a.out/'sand-film').glob('*.png'))
        concat=out/'frames.txt';concat.write_text(''.join("file '"+p.as_posix()+"'\nduration "+str(ts[j+1]-ts[j] if j+1<len(ts) else .125)+'\n' for j,p in enumerate(frames)))
        subprocess.run([shutil.which('ffmpeg'),'-y','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-an',str(out/'actual-direct-placement-silent.mp4')],check=True)
        passed=True
    finally:
        if not passed:
            for j,v in enumerate(clients):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+str(j))
                    except Exception:pass
        run.close()
        errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in line or 'NullReference' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=args.build,actualSimultaneousClients=4,checks=checks,runtimeErrors=errors,recording='actual-direct-placement-silent.mp4',physicalDeviceRun=False,liveDataModified=False))
        require(not errors,'runtime errors: '+str(errors[:3]))
    print('PASS '+str(out),flush=True)

if __name__=='__main__':main()
