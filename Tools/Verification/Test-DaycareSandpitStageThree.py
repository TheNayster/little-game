"""Stage 3 acceptance: current native players, synthetic authority, real UGUI taps.
No installed app/server or real checkpoint is used. Physical touch comfort deferred.
"""
import sys
from pathlib import Path
sys.path.insert(0, str(next(p for p in Path(__file__).resolve().parents if p.name == 'Tools')))
from project_paths import ROOT
import argparse, copy, hashlib, importlib.util, json, time
from concurrent.futures import ThreadPoolExecutor
from shared_garden_runtime import Run, read, write, wait, require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--presentation-of',metavar='OWN_PASSED_RUN_UUID',help='Recheck changed presentation/input only, reusing unchanged rules/shared evidence');parser.add_argument('--capture-of',metavar='OWN_PASSED_RUN_UUID',help='Only capture stable accepted current gameplay and local previews');args=parser.parse_args()
    require(not(args.presentation_of and args.capture_of),'choose one focused mode')
    run=Run(args.build,extended_test_lifetime=True,resume=args.presentation_of or args.capture_of)
    require(run.content==69 and read(run.folder/'build-summary.json')['schema']==51,'Stage 3 compatible build required')
    files={f['path']:f['sha256'] for f in read(run.folder/'source-manifest.json')['files']}
    for suffix in ('Core/Worlds/Daycare/DaycareSandpit.cs','Core/Shared/Layout/WorldLayout.cs',*[f'Client/Worlds/Daycare/{n}.cs' for n in ('GameScreen.SandcastleClub','GameScreen.SandcastleWater','GameScreen.SandcastleTip','SandShape')]):
        path='Unity/FamilyPlayset/Assets/FamilyPlayset/Code/'+suffix
        require(files.get(path)==hashlib.sha256((ROOT/path).read_bytes()).hexdigest(),'stale build '+suffix)
    out=run.path/('sandpit-stage-three-captures-'+str(args.build) if args.capture_of else 'sandpit-stage-three-presentation-'+str(args.build) if args.presentation_of else 'sandpit-stage-three');
    if out.exists():
        attempt=2
        while out.with_name(out.name+'-'+str(attempt)).exists():attempt+=1
        out=out.with_name(out.name+'-'+str(attempt))
    out.mkdir();checks=[];clients=[];passed=False;server=None;peak=0
    if args.presentation_of:
        prior=read(run.path/'sandpit-stage-three/result.json');require(prior and prior['gameplayPassed'],"only reuse this script's passed synthetic fixture")
        old={f['path']:f['sha256'] for f in read(ROOT/('Builds/NetworkProbe/G3-0.0.'+str(prior['build']))/'source-manifest.json')['files']}
        for path in files:
            if path.startswith('Unity/FamilyPlayset/Assets/FamilyPlayset/Code/') and path not in ('Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Daycare/GameScreen.SandcastleClub.cs',):
                require(files[path]==old.get(path),'presentation reuse contains another source change '+path)
    print('EVIDENCE '+str(out),flush=True)
    def state():return server.state()['view']['sandpit']
    def piece(id):return next(m for m in state()['moulds'] if m['id']==id)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def button(v,name):
        for _ in range(8):
            info=v.input('inspect')
            if any(c['name']==name and c['enabled'] for c in info['controls']):break
            if not info['menuOpen']:break
            x,y=info['screenWidth']*.55,info['screenHeight']*.37
            v.input('touch-begin',role='screen',x=x,y=y,finger=97)
            for dy in (30,65,110,160):v.input('touch-move',role='screen',x=x,y=y+dy,finger=97)
            v.input('touch-end',role='screen',x=x,y=y+160,finger=97)
        wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,20)
        v.input('touchButton',text=name)
    def layout(v,name):
        wait(lambda:not v.input('inspect')['pending'],'settled controls')
        info=v.input('inspect');names={c['name'] for c in info['controls']}
        require({'Round tower','Square tower','Wall','Gate','Leave sandpit','Optional teacher help'}<=names,'missing mould/help/leave')
        require(any(c['name']=='Decorate · later' and not c['enabled'] for c in info['controls']),'empty decoration feature looks active')
        for c in info['controls']:
            if c['name'] in ('Round tower','Square tower','Wall','Gate','Leave sandpit','Optional teacher help','Next sand tool','Water','Tip','Confirm sand placement','Rotate sand mould','Cancel sand placement'):
                r=c['bounds'];safe=info['safeArea']
                require(r['width']>=50 and r['height']>=44,'small essential '+c['name'])
                require(r['x']>=safe['x']-2 and r['y']>=safe['y']-2 and r['x']+r['width']<=safe['x']+safe['width']+2 and r['y']+r['height']<=safe['y']+safe['height']+2,'unsafe '+c['name'])
        home.capture(v,out,name)
    def place(v,shape,cell,rotate=False,capture=None):
        old={m['id'] for m in state()['moulds']}
        button(v,{'round':'Round tower','square':'Square tower','wall':'Wall','gate':'Gate'}[shape])
        if rotate:button(v,'Rotate sand mould')
        button(v,'Sand spot '+str(cell))
        if capture:layout(v,capture)
        button(v,'Confirm sand placement')
        wait(lambda:len(state()['moulds'])>len(old),'placed '+shape)
        id=next(m['id'] for m in state()['moulds'] if m['id'] not in old)
        wait(lambda: not (i:=v.input('inspect'))['pending'] and i['sandpitSelection']>=0 and i['sandpit']['moulds'][i['sandpitSelection']]['id']==id,'selected new shape')
        require(piece(id)['shape']==shape and piece(id)['orientation']==(90 if rotate else 0),'preview/submission disagreement')
        return id
    def build(v,id,early=False):
        if early:button(v,'Water');wait(lambda:piece(id)['wet'],'water before scoop')
        for _ in range(3):button(v,'Next sand tool')
        wait(lambda:piece(id)['scoops']==3,'three real scoop taps',35)
        if not early:button(v,'Next sand tool');wait(lambda:piece(id)['wet'],'next tool waters')
        button(v,'Next sand tool');wait(lambda:piece(id)['built'],'next tool tips',30)
        wait(lambda: any(m['id']==id and m['built'] for m in v.input('inspect')['sandpit']['moulds']),'client built shape')
        time.sleep(.4)
        wait(lambda: not any(v.input('inspect')['sandpitTipEffects']),'reveal finished')
    def command(v,action,**kw):return home.command(v,action,**kw)
    def move(v,id):
        m=piece(id);require(command(v,0,x=m['x']-60,y=m['y']-55)['accepted'],'work point')
    def tool(v,id,op):return command(v,32,value=op,target=id+'@'+str(state()['round']))
    def checkpoint(fixture):
        p=run.path/'server-world/world.save';header,digest,body=p.read_bytes().split(b'\n',2)
        require(header==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode()==digest,'owned checkpoint integrity')
        if fixture is None:return json.loads(body)
        body=json.dumps(fixture,separators=(',',':')).encode();p.write_bytes(header+b'\n'+hashlib.sha256(body).hexdigest().encode()+b'\n'+body)
    try:
        if args.presentation_of:
            fixture=checkpoint(None);legacy=[m for m in fixture['sandpit']['moulds'] if m['id'].startswith('legacy-')]
            require(len(legacy)==4,'owned presentation fixture must retain the four legacy pieces')
            fixture['sandpit']['moulds']=legacy;checkpoint(fixture)
        server=run.start('server')
        for i,slot in enumerate(run.slots):
            v=run.start('client',slot['profile']);clients.append(v);peak=max(peak,sum(c.process.poll() is None for c in clients));home.ready(v);v.input('resize',x=1280 if i<2 else 1024,y=591 if i<2 else 768);home.ready(v)
            require(command(v,1,value=('blue-pup','orange-pup','muffin','socks')[i])['accepted'],'family character choice')
            if next(p for p in server.state()['view']['players'] if p['id']==v.profile)['zone']!='daycare':require(command(v,7,value='daycare')['accepted'],'travel daycare')
        a,b,c,d=clients
        if args.capture_of:
            accepted=read(run.path/('sandpit-stage-three-presentation-'+str(args.build))/'result.json');require(accepted and accepted['gameplayPassed'] and accepted['build']==args.build,'current presentation/input must have passed')
            for v in clients:
                if not next(m for m in state()['members'] if m['actor']==v.profile)['attending']:require(command(v,32,value='start')['accepted'],'join retained creation')
                wait(lambda:not any(v.input('inspect')['sandpitTipEffects']),'baseline has no old tip effects')
            button(a,'Sand piece 5');button(c,'Sand piece 7');layout(a,'phone-connected-castle');layout(c,'tablet-connected-castle')
            button(a,'Sand piece 3');layout(a,'phone-partial-active-tools');button(c,'Sand piece 3');layout(c,'tablet-partial-active-tools')
            button(c,'Sand piece 4');layout(c,'tablet-wet-partial-active-tools')
            before=copy.deepcopy(state()['moulds'])
            button(a,'Wall');button(a,'Sand spot 9');button(a,'Rotate sand mould');layout(a,'phone-rotated-wall-preview')
            button(c,'Gate');button(c,'Sand spot 9');button(c,'Rotate sand mould');layout(c,'tablet-rotated-gate-preview')
            require(all(any(x['name']=='Confirm sand placement' and x['enabled'] for x in v.input('inspect')['controls']) for v in (a,c)),'exact independent local previews invalid')
            button(a,'Cancel sand placement');button(c,'Cancel sand placement');require(state()['moulds']==before,'preview/capture changed shared work')
            button(b,'Leave sandpit');wait(lambda:not next(m for m in state()['members'] if m['actor']==b.profile)['attending'],'final independent exit')
            require(sum(m['attending'] for m in state()['members'])==3 and state()['moulds']==before,'final departure damaged creation')
            record('Four current native clients then independent exit: stable phone/tablet castle, dry/wet partial tools, rendered flags/shells, independent rotated previews and non-mutating cancel; no transient replay')
            passed=True;print('PASS STABLE CAPTURES',flush=True);return
        if args.presentation_of:
            for v in clients:
                if not next(m for m in state()['members'] if m['actor']==v.profile)['attending']:require(command(v,32,value='start')['accepted'],'join retained creation')
            ids=[]
            for v,shape,cell,rot in ((a,'round',0,False),(b,'wall',1,False),(c,'gate',3,False),(d,'square',5,False),(a,'wall',15,True),(c,'gate',8,True)):
                id=place(v,shape,cell,rotate=rot,capture=('phone' if v in (a,b) else 'tablet')+'-'+shape+('-rotated' if rot else '')+'-preview');ids.append(id)
                if v==c and shape=='gate' and not rot:layout(v,'tablet-nearby-active-tools')
                build(v,id)
            for v,id in zip(clients,ids[:4]):
                index=next(i for i,m in enumerate(state()['moulds']) if m['id']==id);button(v,'Sand piece '+str(index+1));wait(lambda:v.input('inspect')['sandpitSelection']==index,'independent selection')
            layout(a,'phone-connected-castle');layout(c,'tablet-connected-castle')
            record('Fresh presentation build: four native clients, all moulds and both rotations through taps, unobstructed grid, connected castle and independent selections')
            # Exercise the unchanged retained-intent machinery via the new controls.
            p=place(a,'square',22);q=place(d,'round',20)
            pi=next(i for i,m in enumerate(state()['moulds']) if m['id']==p);qi=next(i for i,m in enumerate(state()['moulds']) if m['id']==q)
            require(command(a,0,x=4100,y=75)['accepted'],'distant synthetic approach')
            for _ in range(2):button(a,'Next sand tool')
            require(a.input('inspect')['sandpitApproach']>=0,'retained approach must be exercised')
            button(a,'Sand piece '+str(qi+1));time.sleep(1)
            require(piece(p)['scoops']<=1 and piece(q)['scoops']==0,'switch did not cancel pending scoops')
            r=next(c['bounds'] for c in a.input('inspect')['controls'] if c['name']=='Next sand tool');x,y=r['x']+r['width']/2,r['y']+r['height']/2
            for _ in range(8):
                a.input('touch-begin',role='screen',x=x,y=y,finger=98);a.input('touch-end',role='screen',x=x,y=y,finger=98)
            wait(lambda:piece(q)['scoops']==3,'bounded rapid useful taps',35)
            require(not piece(q)['wet'] and not piece(q)['built'],'rapid scoop tail promoted itself to Water/Tip')
            button(a,'Sand piece '+str(pi+1));time.sleep(.5);require(piece(q)['scoops']==3 and piece(p)['scoops']<=1,'rapid taps spilled into selected sibling piece')
            button(a,'Sand piece '+str(qi+1));button(a,'Tip')
            wait(lambda: a.input('inspect')['sandpitApproach']<0 and not a.input('inspect')['pending'],'dry tip reply',30)
            require(piece(q)['scoops']==3 and not piece(q)['built'],'dry Tip erased fill')
            layout(a,'phone-full-dry-retained');button(a,'Next sand tool');wait(lambda:piece(q)['wet'],'Water recovery');button(a,'Next sand tool');wait(lambda:piece(q)['built'],'Tip without refilling')
            record('New tool controls retain/cancel rapid Scoops without spill; full dry Tip keeps fill then Water/Tip builds without extra scoops')
            legacy=state()['moulds'][2];button(c,'Sand piece 3');layout(c,'tablet-migrated-partial-active-tools')
            layout(a,'phone-migrated-decorations');layout(c,'tablet-migrated-decorations')
            require(a.input('inspect')['sandpitDecorationVisuals'][:2]==[1,2],'saved flags/shells lost visually')
            record('Adjusted framing and depth ordering captured on both aspects, with legacy flags/shells and partial progress visible; physical devices deferred')
            passed=True;print('PASS TARGETED PRESENTATION',flush=True);return
        button(a,'Games');button(a,'Sandcastle club')
        wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four shared participants')
        ids=[]
        ids.append(place(a,'round',8,capture='phone-round-preview'));build(a,ids[-1],early=True)
        ids.append(place(b,'wall',9,capture='phone-wall-preview'));build(b,ids[-1])
        ids.append(place(c,'gate',11,capture='tablet-gate-preview'));layout(c,'tablet-active-gate-tools');build(c,ids[-1])
        ids.append(place(d,'square',13,capture='tablet-square-preview'));build(d,ids[-1])
        ids.append(place(a,'wall',15,rotate=True,capture='phone-rotated-wall-preview'));build(a,ids[-1])
        ids.append(place(c,'gate',16,rotate=True,capture='tablet-rotated-gate-preview'));build(c,ids[-1])
        for v,id in zip(clients,ids):
            info=v.input('inspect');require(info['sandpit']['moulds'][info['sandpitSelection']]['id']==id or v in (a,c),'local selection unexpectedly shared')
        layout(a,'phone-connected-castle');layout(c,'tablet-connected-castle')
        record('All four silhouettes plus 90-degree wall/gate built using real picture taps; early Water and continued connected construction')
        # Rotation after choosing a last-row spot invalidates the exact footprint.
        button(d,'Gate');button(d,'Sand spot 30');button(d,'Rotate sand mould')
        info=d.input('inspect');require(any(c['name']=='Confirm sand placement' and not c['enabled'] for c in info['controls']),'outside rotated preview remained confirmable')
        button(d,'Cancel sand placement')
        record('Rotated outside preview disables confirm and cancellation leaves shared work unchanged')
        x,y=4150+3*85,130+2*110+55;before=copy.deepcopy(state()['moulds'])
        with ThreadPoolExecutor(max_workers=2) as pool:
            results=list(pool.map(lambda v:command(v,32,value='place',target='place@'+str(state()['round']),item='wall:90',x=x,y=y),(a,b)))
        require(sum(r['accepted'] for r in results)==1 and len(state()['moulds'])==len(before)+1,'concurrent footprint conflict')
        contested=next(m['id'] for m in state()['moulds'] if m['id'] not in {p['id'] for p in before})
        require(all(m in state()['moulds'] for m in before),'placement conflict damaged work')
        move(a,contested);move(b,contested)
        with ThreadPoolExecutor(max_workers=2) as pool:replies=list(pool.map(lambda v:tool(v,contested,'scoop'),(a,b)))
        require(all(r['accepted'] for r in replies) and piece(contested)['scoops']==2,'same piece contributions')
        require(tool(a,contested,'scoop')['accepted'] and tool(a,contested,'water')['accepted'],'shared full wet')
        with ThreadPoolExecutor(max_workers=2) as pool:replies=list(pool.map(lambda v:tool(v,contested,'tip'),(a,b)))
        require(sum(r['accepted'] for r in replies)==1 and piece(contested)['built'],'competing tips')
        p=place(b,'square',22);q=place(d,'round',6);move(b,p);move(d,q)
        with ThreadPoolExecutor(max_workers=2) as pool:replies=list(pool.map(lambda pair:tool(pair[0],pair[1],'scoop'),((b,p),(d,q))))
        require(all(r['accepted'] for r in replies) and piece(p)['scoops']==piece(q)['scoops']==1,'different piece parallel construction')
        record('Four native clients: exact oriented placement race, cooperative Scoops, one competing Tip result, parallel different-piece contributions')
        for v in clients:
            wait(lambda: v.input('inspect')['sandpit']['moulds']==state()['moulds'],'same authoritative oriented pieces')
        before=copy.deepcopy(state()['moulds']);button(b,'Leave sandpit');wait(lambda:not next(m for m in state()['members'] if m['actor']==b.profile)['attending'],'leave')
        require(state()['moulds']==before and sum(m['attending'] for m in state()['members'])==3,'leave erased work')
        layout(a,'phone-active-tools');button(c,'Sand piece '+str(len(before)));layout(c,'tablet-active-tools')
        record('Independent departure preserves all shapes and siblings; phone shelf/tablet contextual controls captured')
        run.close();saved=checkpoint(None);expected=copy.deepcopy(saved['sandpit']['moulds'])
        server=run.start('server');require(state()['moulds']==expected,'oriented checkpoint reopening')
        a=run.start('client',run.slots[0]['profile']);clients=[a];home.ready(a);a.input('resize',x=1280,y=591);layout(a,'phone-reopened-oriented-castle')
        require(not any(a.input('inspect')['sandpitTipEffects']),'reopening replays old effects')
        record('Native authority save reopening retains oriented completed/partial pieces; reconstructed client does not replay tips')
        # Real native migration of a synthetic format-one Stage 2 castle.
        run.close();fixture=checkpoint(None);fixture['schema']=50;g=fixture['sandpit'];g['format']=1
        g['moulds']=[dict(id='stage2-flag',creator='',shape='round',x=4210,y=440,width=140,depth=100,capacity=2,scoops=2,decoration=1,version=4,wet=True,built=True),dict(id='stage2-shell',creator='',shape='round',x=4370,y=440,width=140,depth=100,capacity=3,scoops=3,decoration=2,version=5,wet=True,built=True),dict(id='stage2-partial',creator=run.slots[0]['profile'],shape='round',x=4150,y=130,width=72,depth=80,capacity=3,scoops=1,decoration=0,version=1,wet=False,built=False)]
        expected=copy.deepcopy(g['moulds']);checkpoint(fixture);server=run.start('server')
        require(state()['format']==2 and len(state()['moulds'])==3,'Stage 2 format migration')
        for m,old in zip(state()['moulds'],expected):require(all(m[k]==v for k,v in old.items()) and m['orientation']==0,'Stage 2 field loss')
        for slot,size,label in ((run.slots[0],(1280,591),'phone'),(run.slots[2],(1024,768),'tablet')):
            v=run.start('client',slot['profile']);clients.append(v);peak=max(peak,sum(c.process.poll() is None for c in clients));home.ready(v);v.input('resize',x=size[0],y=size[1]);home.ready(v)
            wait(lambda:v.input('inspect')['sandpitDecorationVisuals'][:2]==[1,2],'migrated decorations visible')
            layout(v,label+'-stage-two-migrated-decorations')
        record('Native synthetic Stage 2 save migration retains exact positions, capacities, progress and visible flags/shells on both aspects')
        run.close();fixture=checkpoint(None);fixture['schema']=49;g=fixture['sandpit'];g.update(format=0,pieceLimit=0,scoopCapacity=0,phase=2,round=7)
        g['moulds']=[dict(scoops=2,wet=True,built=True,decoration=1),dict(scoops=3,wet=True,built=True,decoration=2),dict(scoops=1,wet=False,built=False,decoration=0),dict(scoops=2,wet=True,built=False,decoration=0)]
        checkpoint(fixture);server=run.start('server')
        for i,m in enumerate(state()['moulds']):require(m['id']=='legacy-'+str(i) and m['x']==4210+i*160 and m['y']==440 and m['capacity']==2+i%2 and all(m[k]==v for k,v in g['moulds'][i].items()),'legacy migration')
        v=run.start('client',run.slots[0]['profile']);clients.append(v);home.ready(v);v.input('resize',x=1280,y=591);layout(v,'phone-legacy-migrated-decorations')
        record('Native synthetic schema49 migration retains all four legacy positions/capacities, partial dry/wet progress and decorations')
        passed=True
    finally:
        if not passed:
            for i,v in enumerate(clients):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+str(i))
                    except Exception:pass
        run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,peakNativeClients=peak,reusedNativeBuild=prior['build'] if args.presentation_of else None,scope='Stage 3 isolated native authority, synthetic saves, actual phone/tablet-aspect UGUI touches; no physical devices or deployment'))
    print('PASS ALL',flush=True)
if __name__=='__main__':main()
