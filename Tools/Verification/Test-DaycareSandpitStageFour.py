"""Stage 4: real UGUI touches, four native clients and disposable authority saves.
Owned synthetic evidence only; no installed server, enrollment or family saves.
"""
import sys
from pathlib import Path
sys.path.insert(0,str(next(p for p in Path(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT
import argparse,copy,hashlib,importlib.util,time
from concurrent.futures import ThreadPoolExecutor
from shared_garden_runtime import Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);ap.add_argument('--continue-of');ap.add_argument('--reopen-of');args=ap.parse_args();require(not (args.continue_of and args.reopen_of),'choose one owned continuation')
    run=Run(args.build,extended_test_lifetime=True,resume=args.continue_of or args.reopen_of)
    require(run.content==70 and read(run.folder/'build-summary.json')['schema']==52,'Stage 4 compatible build required')
    files={f['path']:f['sha256'] for f in read(run.folder/'source-manifest.json')['files']}
    for path,digest in files.items():
        if path.startswith('Unity/FamilyPlayset/Assets/FamilyPlayset/Code/'):
            require(hashlib.sha256((ROOT/path).read_bytes()).hexdigest()==digest,'stale source '+path)
    out=run.path/('sandpit-stage-four-reopen-'+str(args.build) if args.reopen_of else 'sandpit-stage-four-'+str(args.build) if args.continue_of else 'sandpit-stage-four');out.mkdir();clients=[];checks=[];peak=0;passed=False;server=None;prior=None
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
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def capture(v,name):
        wait(lambda:not v.input('inspect')['pending'],'settled capture')
        info=v.input('inspect')
        for control in info['controls']:
            if control['name'] in ('Build','Decorate','Flag','Shell','Pebble','Door','Window','Dinosaur','Edit','Family reset','Confirm sand decoration','Cancel sand play','Move','Remove','Back','Yes, this only','Keep it','Clear sandpit','Keep castle','Leave sandpit','Optional teacher help') or control['name'].startswith('Sand attachment '):
                r=control['bounds'];safe=info['safeArea']
                require(r['width']>=44 and r['height']>=44,'tiny target '+control['name'])
                require(r['x']>=safe['x']-2 and r['y']>=safe['y']-2 and r['x']+r['width']<=safe['x']+safe['width']+2 and r['y']+r['height']<=safe['y']+safe['height']+2,'clipped control '+control['name'])
        home.capture(v,out,name)
    def select(v,id):
        button(v,'Sand piece '+str(next(i for i,m in enumerate(state()['moulds']) if m['id']==id)+1))
        wait(lambda:(i:=v.input('inspect'))['sandpitSelection']>=0 and i['sandpit']['moulds'][i['sandpitSelection']]['id']==id,'select exact stable piece')
    def place(v,title,cell,rotate=False):
        before={m['id'] for m in state()['moulds']};button(v,'Build');button(v,title)
        if rotate:button(v,'Rotate sand mould')
        button(v,'Sand spot '+str(cell));button(v,'Confirm sand placement')
        wait(lambda:len(state()['moulds'])>len(before),'piece placed')
        id=next(m['id'] for m in state()['moulds'] if m['id'] not in before)
        wait(lambda:not v.input('inspect')['pending'],'placement settled');return id
    def build(v,id):
        select(v,id);button(v,'Water');wait(lambda:piece(id)['wet'],'early water')
        for _ in range(3):button(v,'Next sand tool')
        wait(lambda:piece(id)['scoops']==3,'shared scoops',35);button(v,'Next sand tool');wait(lambda:piece(id)['built'],'tip',30)
        wait(lambda:not any(v.input('inspect')['sandpitTipEffects']),'tip finished')
    def decor(v,id,name):
        select(v,id);button(v,'Decorate');button(v,name);button(v,'Confirm sand decoration')
        wait(lambda: any(a['kind']==name.lower() for a in piece(id)['attachments']),'placed '+name)
        wait(lambda:not v.input('inspect')['pending'],'decoration settled')
    def start_clients():
        nonlocal clients,peak
        clients=[]
        for i,slot in enumerate(run.slots):
            v=run.start('client',slot['profile']);clients.append(v);peak=max(peak,len(clients));home.ready(v)
            v.input('resize',x=1280 if i<2 else 1024,y=591 if i<2 else 768);home.ready(v)
        return clients
    def checkpoint(value=None):
        require(all(p.process.poll() is not None for p in run.instances),'stop owned writers before synthetic edit')
        p=run.path/'server-world/world.save';data=p.read_bytes();magic,digest,payload=data.split(b'\n',2)
        require(magic==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(payload).hexdigest().encode()==digest,'own synthetic checkpoint digest')
        if value is None:
            import json
            return json.loads(payload)
        import json
        payload=json.dumps(value,separators=(',',':')).encode();p.write_bytes(magic+b'\n'+hashlib.sha256(payload).hexdigest().encode()+b'\n'+payload)
    if args.reopen_of:
        # Resume only this script's finished synthetic Stage 4 creation. The 440
        # scenario reached all nine PASS groups, then its metadata writer failed.
        # Preserve that history; exercise only current presentation/reopening now.
        reused=read(run.path/'sandpit-stage-four/result.json')
        require(reused and reused['build']==439 and len(reused['checks'])==5,'owned accepted gameplay fixture')
        for name in ('phone-stage-three-migrated','tablet-legacy-migrated','tablet-connected-reset','phone-connected-decorated-castle'):
            capture_info=read(run.path/'sandpit-stage-four-440'/(name+'.json'))
            require(capture_info and capture_info['build']=='0.0.440' and capture_info['passed'],'missing actual 440 capture '+name)
        previous={f['path']:f['sha256'] for f in read(ROOT/'Builds/NetworkProbe/G3-0.0.440/source-manifest.json')['files']}
        for path,digest in files.items():
            if path.startswith('Unity/FamilyPlayset/Assets/FamilyPlayset/Code/') and not path.endswith(('Client/Worlds/Daycare/GameScreen.SandcastlePlay.cs','Client/Worlds/Daycare/SandShape.cs')):require(previous.get(path)==digest,'unreviewed source change '+path)
        try:
            saved=checkpoint();expected=copy.deepcopy(saved['sandpit']['moulds']);toy=copy.deepcopy(saved['sandpit']['toy'])
            require(saved['schema']==52 and saved['sandpit']['format']==3 and len(expected)==4 and toy['placed'],'finished synthetic connected creation required')
            server=run.start('server');require(state()['moulds']==expected and state()['toy']==toy,'current save reopening')
            a,b,c,d=start_clients()
            for v in clients:
                if not next(m for m in state()['members'] if m['actor']==v.profile)['attending']:require(cmd(v,32,value='start')['accepted'],'rejoin saved creation')
            first=expected[0]['id'];select(a,first);button(a,'Decorate');button(a,'Dinosaur');button(a,'Sand toy spot 19');wait(lambda:state()['toy']['y']==350,'clear floor toy location')
            button(c,'React sand dinosaur');wait(lambda:state()['toy']['reaction']==toy['reaction']+1,'current friendly reaction')
            version=state()['toy']['version']
            with ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(lambda pair:play(pair[0],'toy-place','toy',str(version),x=pair[1],y=350),((a,4405),(b,4490))))
            require(sum(r['accepted'] for r in results)==1,'competing toy manipulations did not have one winner')
            button(d,'Decorate');button(d,'Dinosaur');button(d,'Sand toy spot 19');wait(lambda:state()['toy']['x']==4405 and state()['toy']['y']==350,'final toy ground location')
            for v in clients:wait(lambda:v.input('inspect')['sandpit']['toy']==state()['toy'] and v.input('inspect')['sandpit']['moulds']==expected,'four consistent current views')
            # Exercise explicit occupied-prop replacement and prop-only removal
            # through the real secondary confirmation controls, then restore it.
            untouched=copy.deepcopy(expected[1:])
            select(a,first);button(a,'Decorate');button(a,'Window');button(a,'Sand attachment 4');button(a,'Confirm sand decoration');capture(a,'phone-decoration-replace-confirmation');button(a,'Yes, this only')
            wait(lambda:any(x['slot']==4 and x['kind']=='window' for x in piece(first)['attachments']),'explicit replacement')
            button(a,'Door');button(a,'Sand attachment 4');button(a,'Confirm sand decoration');button(a,'Yes, this only');wait(lambda:any(x['slot']==4 and x['kind']=='door' for x in piece(first)['attachments']),'restore door')
            button(a,'Edit');button(a,'Edit sand attachment 3');button(a,'Remove');button(a,'Keep it');require(any(x['slot']==3 for x in piece(first)['attachments']),'prop removal cancellation')
            button(a,'Remove');button(a,'Yes, this only');wait(lambda:not any(x['slot']==3 for x in piece(first)['attachments']),'one prop removed');button(a,'Pebble');button(a,'Confirm sand decoration');wait(lambda:any(x['slot']==3 and x['kind']=='pebble' for x in piece(first)['attachments']),'restore pebble')
            require(state()['moulds'][1:]==untouched,'prop edit altered siblings');expected=copy.deepcopy(state()['moulds'])
            select(a,first);button(a,'Decorate');capture(a,'phone-connected-decorated-castle');select(c,expected[2]['id']);button(c,'Decorate');capture(c,'tablet-connected-decorated-castle')
            button(a,'Optional teacher help');capture(a,'phone-help-attachment-preview');button(a,'Cancel sand play')
            button(a,'Edit');capture(a,'phone-connected-edit-controls');button(a,'Back')
            button(a,'Family reset');capture(a,'phone-connected-reset');capture(c,'tablet-connected-reset');button(a,'Clear sandpit');wait(lambda:len(state()['reset']['approved'])==1,'one explicit approval');capture(c,'tablet-one-reset-agreement');button(c,'Keep castle');wait(lambda:not state()['reset']['token'],'current reset cancellation')
            before=copy.deepcopy(state()['toy']);button(b,'Leave sandpit');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'current independent departure');require(state()['moulds']==expected and state()['toy']==before,'departure lost current work');require(cmd(b,32,value='start')['accepted'],'current independent rejoin')
            record('Four current clients: saved connected castle, shared reaction and first-wins toy manipulation, tap controls, explicit occupied-prop replacement/removal/cancel, ground/depth presentation, help/edit/reset pictures and independent departure/rejoin')
            run.close();saved=checkpoint();toy=copy.deepcopy(saved['sandpit']['toy']);server=run.start('server');require(state()['moulds']==expected and state()['toy']==toy,'final current authority reopen')
            a=run.start('client',run.slots[0]['profile']);clients=[a];home.ready(a);a.input('resize',x=1280,y=591);button(a,'Decorate');capture(a,'phone-reopened-connected-castle')
            require(state()['moulds']==expected and state()['toy']==toy,'client reconstruction changed final creation')
            record('Final current build saves/reopens decorated castle and accepted toy state; fresh native client shows retained props and toy without replay')
            passed=True
        finally:
            run.close()
            import re
            failures=[]
            for instance in run.instances:
                log=(instance.out/'player.log').read_text(encoding='utf-8',errors='replace')
                if re.search(r'Exception:|Fatal|NullReference|InvalidOperationException',log):failures.append(instance.identity)
            require(not failures,'current native errors '+str(failures))
            write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,peakNativeClients=peak,nativeLogCount=len(run.instances),runtimeErrors=failures,reusedNativeBuilds=[439,440],previous440ReceiptFailure='All nine scenario assertions reached PASS; prior variable shadowing failed only result.json formatting. Captures and observed output preserved; writer fixed.',scope='Focused current presentation, shared toy concurrency and actual save reopening; prior unchanged full Stage 4 gameplay/migration/reset evidence reused'))
        print('PASS CURRENT REOPEN AND PRESENTATION',flush=True);return
    try:
        if args.continue_of:
            prior=read(run.path/'sandpit-stage-four/result.json')
            require(prior and len(prior['checks'])==5 and prior['peakNativeClients']==4 and prior['build']==439,'only continue the owned five-group accepted scenario')
            old_files={f['path']:f['sha256'] for f in read(ROOT/('Builds/NetworkProbe/G3-0.0.'+str(prior['build']))/'source-manifest.json')['files']}
            changed=('Core/Worlds/Daycare/DaycareSandpitPlay.cs','Editor/SandpitJsonTests.cs')
            for path,digest in files.items():
                if path.startswith('Unity/FamilyPlayset/Assets/FamilyPlayset/Code/') and not any(path.endswith(t) for t in changed):require(old_files.get(path)==digest,'unexpected change in reused gameplay '+path)
            saved=checkpoint();require(saved['schema']==51 and saved['sandpit']['format']==2 and len(saved['sandpit']['moulds'])==6,'expected owned Stage 3 migration fixture')
            ids=[m['id'] for m in saved['sandpit']['moulds']];first=ids[0];checks=prior['checks'].copy()
            print('REUSE native439 five passed groups; remaining migration/reset and current captures run on '+str(args.build),flush=True)
        else:
            server=run.start('server');a,b,c,d=start_clients()
            for i,v in enumerate(clients):
                require(cmd(v,1,value=('blue-pup','orange-pup','muffin','socks')[i])['accepted'],'avatar choice')
                require(cmd(v,7,value='daycare')['accepted'],'travel daycare')
            button(a,'Games');button(a,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four joined')
            ids=[]
            for v,title,cell,rotate in ((a,'Round tower',0,False),(b,'Square tower',2,False),(c,'Wall',4,False),(a,'Gate',16,False),(b,'Wall',20,True),(c,'Gate',23,True)):
                id=place(v,title,cell,rotate);build(v,id);ids.append(id)
            first=ids[0]
            for name in ('Flag','Shell','Pebble','Door','Window'):decor(a,first,name)
            require(len(piece(first)['attachments'])==5,'five coexist')
            select(a,first);button(a,'Decorate');button(a,'Flag');capture(a,'phone-attachment-preview');button(a,'Cancel sand play')
            for id in ids[1:]:decor(b,id,'Flag');decor(c,id,'Shell')
            select(c,ids[3]);button(c,'Decorate');button(c,'Door');capture(c,'tablet-gate-attachment-preview');button(c,'Confirm sand decoration')
            wait(lambda:len(piece(ids[3])['attachments'])==3,'gate door')
            record('All six shape/orientations built through taps, combined flags/shells, all five pictured decorations and suggested attachments; gate opening captured')
            select(a,first);button(a,'Edit');capture(a,'phone-piece-edit');button(a,'Move');button(a,'Sand spot 9');capture(a,'phone-move-preview');button(a,'Confirm sand placement')
            wait(lambda:piece(first)['x']==4235 and piece(first)['y']==240,'own piece moved');require(len(piece(first)['attachments'])==5,'move lost attachments')
            require(not play(b,'remove',first,str(piece(first)['version']))['accepted'],'ordinary other owner removal')
            m=piece(first);old_version=m['version'];require(play(b,'decorate',first,'1:flag')['accepted'],'intervening contribution')
            before=copy.deepcopy(state()['moulds']);require(not play(a,'remove',first,str(old_version))['accepted'] and state()['moulds']==before,'overtaken removal discarded contribution')
            require(play(a,'decor-remove',first,str(piece(first)['version'])+':1')['accepted'],'free race slot')
            with ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(lambda v:play(v,'decorate',first,'1:flag'),(b,c)))
            require(sum(r['accepted'] for r in results)==1 and len(piece(first)['attachments'])==6,'same slot race')
            for slot in (2,3):require(play(a,'decor-remove',first,str(piece(first)['version'])+':'+str(slot))['accepted'],'free distinct sockets')
            with ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(lambda args:play(args[0],'decorate',first,args[2]+':'+args[1]),((b,'shell','2'),(c,'pebble','3'))))
            require(all(r['accepted'] for r in results) and len(piece(first)['attachments'])==6,'distinct sockets coexist')
            record('Owner move retains work; sibling destructive edit and overtaken owner edit rejected; actual simultaneous same-slot first win and distinct-slot cooperative placement')
            button(a,'Decorate');button(a,'Dinosaur');button(a,'Sand toy spot 14');wait(lambda:state()['toy']['placed'],'toy placed')
            button(c,'React sand dinosaur');wait(lambda:state()['toy']['reaction']==1,'shared toy reaction')
            button(d,'Decorate');button(d,'Dinosaur');button(d,'Sand toy spot 11');wait(lambda:state()['toy']['x']==4405,'toy moved using taps')
            select(a,first);button(a,'Decorate');capture(a,'phone-combined-decorations-and-toy');select(c,ids[3]);button(c,'Decorate');capture(c,'tablet-decorations-and-toy')
            before=copy.deepcopy(state()['moulds']);button(b,'Leave sandpit');wait(lambda:not next(m for m in state()['members'] if m['actor']==b.profile)['attending'],'independent departure')
            require(state()['moulds']==before and state()['toy']['placed'] and sum(m['attending'] for m in state()['members'])==3,'leave erased family work')
            require(cmd(b,32,value='start')['accepted'],'independent rejoin')
            extra=place(d,'Round tower',6);select(d,extra);button(d,'Edit');button(d,'Remove');capture(d,'tablet-remove-confirmation');button(d,'Keep it');require(any(m['id']==extra for m in state()['moulds']),'cancel erased work')
            button(d,'Remove');button(d,'Yes, this only');wait(lambda:len(state()['moulds'])==6,'confirmed one-piece removal');require(state()['moulds']==before and state()['toy']['placed'],'remove affected siblings/toy')
            record('Shared toy placed/moved/reacted through taps; continued construction, independent Leave/rejoin and explicit removal cancellation/confirmation preserve siblings')
            for v in clients:wait(lambda:v.input('inspect')['sandpit']['moulds']==state()['moulds'] and v.input('inspect')['sandpit']['toy']==state()['toy'],'four consistent decorated views')
            require(cmd(d,7,value='park')['accepted'],'outside world voter')
            button(a,'Family reset');wait(lambda:bool(state()['reset']['token']),'pending global reset');capture(a,'phone-family-reset');capture(d,'tablet-outside-world-reset')
            button(d,'Keep castle');wait(lambda:not state()['reset']['token'],'decline cancels');require(state()['moulds']==before,'decline damaged work')
            button(a,'Family reset');button(a,'Clear sandpit');button(b,'Clear sandpit');require(len(state()['moulds'])==6,'partial consent cleared')
            require(play(c,'toy-react','toy')['accepted'],'intervening toy reaction');wait(lambda:not state()['reset']['token'],'toy cancels vote')
            button(a,'Family reset');d.close();wait(lambda:not state()['reset']['token'],'disconnect cancels vote')
            d=run.start('client',run.slots[3]['profile']);clients[3]=d;home.ready(d);d.input('resize',x=1024,y=768)
            record('Global picture consent reaches player at Park; decline, partial agreement, toy change and disconnect never erase the creation')
            run.close();saved=checkpoint();expected=copy.deepcopy(saved['sandpit']['moulds']);expected_toy=copy.deepcopy(saved['sandpit']['toy']);server=run.start('server')
            require(state()['moulds']==expected and state()['toy']==expected_toy,'save reopen lost decorations/toy');a,b,c,d=start_clients()
            record('Native authority save reopening retains complete decorated/oriented castle and toy placement/reaction version')
        # Real format-two native migration, with old flags/shells on all six shapes.
        run.close();fixture=checkpoint();fixture['schema']=51;g=fixture['sandpit'];g['format']=2;g.pop('toy',None);g.pop('reset',None)
        for i,m in enumerate(g['moulds']):m['decoration']=1+i%2;m.pop('attachments',None)
        old=copy.deepcopy(g['moulds']);world_id=fixture['worldId'];checkpoint(fixture);server=run.start('server');require(state()['format']==3,'format two upgrade')
        for m,prior_piece in zip(state()['moulds'],old):require(all(m[k]==v for k,v in prior_piece.items() if k!='decoration') and m['decoration']==0 and m['attachments']==[dict(slot=0 if prior_piece['decoration']==1 else 2,kind='flag' if prior_piece['decoration']==1 else 'shell')],'Stage 3 retention/decoration mapping')
        require(server.state()['view']['worldId']==world_id,'world identity migration');a,b,c,d=start_clients()
        for v in clients:
            if next(p for p in server.state()['view']['players'] if p['id']==v.profile)['zone']!='daycare':require(cmd(v,7,value='daycare')['accepted'],'return to daycare')
            require(cmd(v,32,value='start')['accepted'],'join migrated castle')
        select(a,first);button(a,'Decorate');capture(a,'phone-stage-three-migrated');select(c,ids[3]);button(c,'Decorate');capture(c,'tablet-stage-three-migrated')
        record('Native Stage 3 migration retains all shapes/orientation/positions/capacities/progress/creators/versions and maps every old decoration once')
        run.close();fixture=checkpoint();fixture['schema']=49;g=fixture['sandpit'];g.update(format=0,pieceLimit=0,scoopCapacity=0,phase=2,round=7);g.pop('toy',None);g.pop('reset',None)
        g['moulds']=[dict(scoops=2,wet=True,built=True,decoration=1),dict(scoops=3,wet=True,built=True,decoration=2),dict(scoops=1,wet=False,built=False,decoration=0),dict(scoops=2,wet=True,built=False,decoration=0)]
        checkpoint(fixture);server=run.start('server')
        for i,m in enumerate(state()['moulds']):require(m['id']=='legacy-'+str(i) and m['x']==4210+i*160 and m['y']==440 and m['capacity']==2+i%2 and all(m[k]==v for k,v in g['moulds'][i].items() if k!='decoration') and len(m['attachments'])==(1 if i<2 else 0),'legacy retention')
        a,b,c,d=start_clients()
        for v in clients:require(cmd(v,32,value='start')['accepted'],'legacy join')
        select(a,'legacy-0');button(a,'Decorate');capture(a,'phone-legacy-migrated');select(c,'legacy-1');button(c,'Decorate');capture(c,'tablet-legacy-migrated')
        require(not any(x['name']=='Edit' and x['enabled'] for x in a.input('inspect')['controls']),'unknown creator edit available')
        before=copy.deepcopy(state()['moulds']);require(not play(a,'remove','legacy-0',str(piece('legacy-0')['version']))['accepted'] and state()['moulds']==before,'unknown creator destructive control')
        record('Native schema49 migration preserves four legacy coordinates/capacities/dry-wet progress and visible flag/shell; unknown creator edit stays unavailable')
        require(cmd(d,7,value='park')['accepted'],'global final voter elsewhere');button(a,'Family reset');epoch=state()['round'];token=state()['reset']['token'];view=server.state()['view'];unrelated=copy.deepcopy(view['toys']);world_id=view['worldId']
        for v in clients:button(v,'Clear sandpit')
        wait(lambda:state()['round']==epoch+1 and not state()['moulds'],'all agree reset')
        require(not state()['toy']['placed'] and server.state()['view']['toys']==unrelated and server.state()['view']['worldId']==world_id,'reset touched unrelated saved data')
        require(not cmd(a,32,value='place',target='place@'+str(epoch),item='round',x=4150,y=130)['accepted'],'stale epoch after reset')
        capture(a,'phone-reset-continued-building');capture(c,'tablet-reset-continued-building')
        record('All four explicit picture approvals including Park clear only Sandcastle; old pending epoch rejected, Build/Decorate remain available')
        # Capture a connected pretend-play creation after reset using the complete tap route.
        require(cmd(d,7,value='daycare')['accepted'] and cmd(d,32,value='start')['accepted'],'fourth player returns independently')
        connected=[]
        for v,title,cell in ((a,'Round tower',0),(b,'Wall',1),(c,'Gate',3),(d,'Square tower',5)):
            id=place(v,title,cell);build(v,id);connected.append(id)
        for name in ('Flag','Shell','Pebble','Door','Window'):decor(a,connected[0],name)
        decor(b,connected[1],'Flag');decor(c,connected[2],'Shell');decor(d,connected[3],'Flag')
        button(d,'Decorate');button(d,'Dinosaur');button(d,'Sand toy spot 11');wait(lambda:state()['toy']['placed'],'continued toy play')
        select(a,connected[0]);button(a,'Decorate');capture(a,'phone-connected-decorated-castle');select(c,connected[2]);button(c,'Decorate');capture(c,'tablet-connected-decorated-castle')
        button(a,'Edit');capture(a,'phone-connected-edit-controls');button(a,'Back');button(a,'Family reset');capture(a,'phone-connected-reset');capture(c,'tablet-connected-reset');button(c,'Keep castle');wait(lambda:not state()['reset']['token'],'retain final family castle')
        before=copy.deepcopy(state()['moulds']);button(b,'Leave sandpit');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'final independent leave');require(state()['moulds']==before and state()['toy']['placed'],'continued family castle erased');require(cmd(b,32,value='start')['accepted'],'final family rejoin')
        for v in clients:wait(lambda:v.input('inspect')['sandpit']['moulds']==state()['moulds'],'final four consistent views')
        record('Current phone/tablet captures: four-player connected decorated castle, toy, edit/reset controls, continued post-reset tap building and independent Leave/rejoin')
        passed=True
    finally:
        if not passed:
            for i,v in enumerate(clients):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+str(i))
                    except Exception:pass
        run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,peakNativeClients=peak,reusedNativeBuild=prior['build'] if prior else None,scope='Stage 4 isolated authority, synthetic saves, actual phone/tablet aspect UGUI touch; physical comfort/performance deferred'))
    print('PASS ALL',flush=True)
if __name__=='__main__':main()
