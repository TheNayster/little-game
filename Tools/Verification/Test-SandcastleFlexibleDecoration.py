"""Owned synthetic saves, four actual native clients, production touch/drag input.
DAY-01/LEARN-01/FAMILY-01: flexible surfaces and persistence, no deployment.
"""
import argparse,copy,hashlib,importlib.util,json,shutil,subprocess,sys,time
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,read,write,wait,require
from parent_server import checkpoint_bytes
s=importlib.util.spec_from_file_location('six',Path(__file__).with_name('Test-SandcastleStageSix.py'));six=importlib.util.module_from_spec(s);s.loader.exec_module(six)
home=six.home

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);ap.add_argument('--depth-resume');args=ap.parse_args()
    run=Run(args.build,extended_test_lifetime=True,resume=args.depth_resume);six.verify_inputs(run.folder)
    require(run.content==71 and read(run.folder/'build-summary.json')['schema']==53,'positioned surface contract')
    out=run.path/'sandcastle-flexible-decoration';out=out/'depth-input' if args.depth_resume else out;out.mkdir(exist_ok=bool(args.depth_resume));checks=[];clients=[];passed=False;server=None
    print('EVIDENCE '+str(out),flush=True)
    def state():return server.state()['view']['sandpit']
    def piece(id):return next(m for m in state()['moulds'] if m['id']==id)
    def button(v,name):six.button(v,name)
    def settled(v):return wait(lambda:(i if not (i:=v.input('inspect'))['pending'] and not i['sandLocalPending'] else None),'settled')
    def tap(v,x,y,finger=71):
        for op in ('touch-begin','touch-end'):v.input(op,role='screen',x=x,y=y,finger=finger)
    def xy(v,x,y):
        i=v.input('inspect');r=next(c['bounds'] for c in i['controls'] if c['name']=='Build');scale=r['width']/150
        cx=r['x']+r['width']/2-370*scale;cy=r['y']+r['height']/2+398*scale;t=(y-70)/470
        return cx+((x-4070)/730*2-1)*(685-189*t)*scale,cy+(-185+385*t)*scale
    def local(v,id,x,y):
        m=piece(id);p=xy(v,m['x'],m['y']);r=next(c['bounds'] for c in v.input('inspect')['controls'] if c['name']=='Build');scale=r['width']/150*(1.14-.28*(m['y']-70)/470)
        return p[0]+x*scale,p[1]+y*scale
    def floor(v,x,y):tap(v,*xy(v,x,y))
    def capture(v,name):
        i=settled(v);require(i['sandIllustrated'],'P9 artwork');require(not any(c['name'].startswith('Sand attachment ') or c['name']=='Confirm sand decoration' for c in i['controls']),'old slots/Attach')
        home.capture(v,out,name)
    def record(label):checks.append(label);print('PASS '+label,flush=True)
    def choose(v,kind):
        button(v,'Decorate');button(v,kind.title());require(v.input('inspect')['sandDecorationChoice']==kind,'choice')
    def decor(v,id,kind,x,y):
        choose(v,kind);old={a['id'] for a in piece(id)['attachments']};tap(v,*local(v,id,x,y))
        a=wait(lambda:next((a for a in piece(id)['attachments'] if a['id'] not in old),None),'actual flexible '+kind)
        require(settled(v)['sandDecorationChoice']==kind,'repeat choice');return a
    def ground(v,kind,x,y):
        choose(v,kind);old={a['id'] for a in state()['ground']};floor(v,x,y)
        a=wait(lambda:next((a for a in state()['ground'] if a['id'] not in old),None),'actual ground '+kind);settled(v);return a
    def place(v,shape,col,row,rotate=False):
        old={m['id'] for m in state()['moulds']};button(v,'Build');button(v,dict(round='Round tower',square='Square tower',wall='Wall',gate='Gate')[shape])
        if rotate:button(v,'Rotate sand mould')
        floor(v,4150+col*85+(42.5 if shape in ('wall','gate') and not rotate else 0),130+row*110+(55 if rotate else 0))
        m=wait(lambda:next((m for m in state()['moulds'] if m['id'] not in old),None),'direct mould');settled(v)
        for _ in range(3):button(v,'Next sand tool')
        wait(lambda:piece(m['id'])['scoops']==3,'scoops',30);button(v,'Water');wait(lambda:piece(m['id'])['wet'],'water');button(v,'Tip');wait(lambda:piece(m['id'])['built'],'tip',30);settled(v);return m['id']
    def cp(v,name):
        r=next(c['bounds'] for c in v.input('inspect')['controls'] if c['name']==name);return r['x']+r['width']/2,r['y']+r['height']/2
    def ownsave():
        require(all(v.process.poll() is not None for v in run.instances),'owned writers must stop')
        data,_=checkpoint_bytes(run.path/'server-world/world.save');return json.loads(data.decode('utf-8-sig').split('\n',2)[2])
    def starts():
        nonlocal server,clients
        server=run.start('server');clients=[]
        for j,slot in enumerate(run.slots):
            v=run.start('client',slot['profile']);clients.append(v);home.ready(v);v.input('resize',x=1280 if j<2 else 1024,y=592 if j<2 else 768);home.ready(v)
        return clients
    try:
        a,b,c,d=starts()
        for j,v in enumerate(clients):require(home.command(v,1,value=('blue-pup','orange-pup','muffin','socks')[j])['accepted'],'avatar');require(args.depth_resume or home.command(v,7,value='daycare')['accepted'],'daycare')
        if not any(x['name']=='Build' for x in a.input('inspect')['controls']):
            button(a,'Games');button(a,'Sandcastle club')
        wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four together')
        if args.depth_resume:
            require(not state()['moulds'],'depth fixture starts from approved reset, no real data')
            back=place(a,'wall',0,1);front=place(b,'round',1,0)
            decor(a,back,'window',-100,35);decor(b,front,'window',0,95)
            for v in (a,c):
                button(v,'Build');tap(v,*local(v,back,-100,35));require(v.input('inspect')['sandpit']['moulds'][v.input('inspect')['sandpitSelection']]['id']==back,'visible background select')
                tap(v,*local(v,front,0,95));require(v.input('inspect')['sandpit']['moulds'][v.input('inspect')['sandpitSelection']]['id']==front,'painted foreground select')
                capture(v,'phone-depth-targets' if v is a else 'tablet-depth-targets')
            record('Actual phone/tablet taps target visible background wall and overlapping foreground tower for decoration and selection')
            passed=True;return
        capture(a,'phone-empty');a.input('sandFilm',x=60)
        wall=place(a,'wall',0,0);square=place(b,'square',3,0);round0=place(c,'round',5,0)
        w0=decor(a,wall,'window',-90,35);w1=decor(a,wall,'window',-35,35)
        require(abs(w0['x']-w1['x'])>45,'two windows distinct tap positions');capture(a,'phone-varied-windows')
        decor(b,wall,'window',25,35);decor(d,wall,'window',85,30)
        decor(b,wall,'flag',-110,83);decor(c,wall,'flag',-20,77);decor(d,wall,'flag',70,63)
        capture(c,'tablet-varied-windows-flags')
        record('Four actual native clients contribute distinct windows and flags on one wall without overwriting')
        decor(a,square,'shell',-35,40);decor(b,square,'pebble',25,105)
        decor(c,round0,'shell',-35,35);decor(d,round0,'pebble',25,105)
        g0=ground(a,'shell',4405,515);ground(a,'shell',4190,470);ground(b,'pebble',4380,470);ground(c,'shell',4760,130)
        edgeProp=decor(c,round0,'window',-58,100);require(abs(edgeProp['x']+58)<=22 and abs(edgeProp['y']-100)<=22,'near-edge clamp stays local')
        capture(a,'phone-attached-and-sand-props');record('Several shells/pebbles stay on supported piece surfaces and independent open sand')
        gate=place(d,'gate',0,2);vertical=place(c,'wall',4,2,True);vGate=place(b,'gate',6,1,True)
        decor(d,gate,'window',-100,55);decor(d,gate,'shell',100,45)
        decor(c,vertical,'window',-18,-25);decor(c,vertical,'window',-18,85)
        decor(b,vGate,'pebble',22,85);decor(b,vGate,'shell',25,150)
        before=copy.deepcopy(state()['moulds']);choose(d,'window');tap(d,*local(d,gate,0,40));settled(d)
        require(state()['moulds']==before and d.input('inspect')['sandDirectCueVisible'],'open arch must reject face decoration')
        capture(d,'tablet-gate-hole-cue');capture(c,'tablet-rotated-surfaces');record('Both wall/gate orientations support varied props and leave open arches clear')
        # Real drag preview follows the same resolved destination and a cancelled
        # touch/UI drop cannot create a prop. No placement helper is called.
        choose(a,'window');tx,ty=cp(a,'Window');px,py=local(a,square,-35,105)
        old=copy.deepcopy(state()['moulds']);a.input('touch-begin',role='screen',x=tx,y=ty,finger=81);a.input('touch-move',role='screen',x=px,y=py,finger=81)
        capture(a,'phone-decoration-drag-preview');a.input('touch-cancel',role='screen',x=px,y=py,finger=81);settled(a);require(state()['moulds']==old,'cancelled drag')
        tx,ty=cp(a,'Window');a.input('touch-begin',role='screen',x=tx,y=ty,finger=82);a.input('touch-move',role='screen',x=px,y=py,finger=82);ux,uy=cp(a,'Build');a.input('touch-end',role='screen',x=ux,y=uy,finger=82);settled(a);require(state()['moulds']==old,'UI release')
        tx,ty=cp(a,'Window');a.input('touch-begin',role='screen',x=tx,y=ty,finger=83);a.input('touch-move',role='screen',x=px,y=py,finger=83);a.input('touch-end',role='screen',x=px,y=py,finger=83)
        wait(lambda:len(piece(square)['attachments'])==3,'actual drag');settled(a);capture(a,'phone-dragged-window')
        choose(a,'shell');before=copy.deepcopy(state()['ground']);floor(a,4800,540);settled(a);require(state()['ground']==before,'boundary');capture(a,'phone-boundary-cue')
        record('Actual phone drag/preview, cancelled touch, UI release and boundary tap preserve work')
        # Intentional same point is a true conflict; a different point survives.
        for v in (b,c):choose(v,'window')
        targets=[local(v,square,35,35) for v in (b,c)];old=copy.deepcopy(piece(square)['attachments'])
        with ThreadPoolExecutor(2) as pool:list(pool.map(lambda q:tap(q[0],*q[1]),zip((b,c),targets)))
        settled(b);settled(c);require(len(piece(square)['attachments'])==len(old)+1 and all(x in piece(square)['attachments'] for x in old),'competing true overlap')
        newest=next(x for x in piece(square)['attachments'] if x not in old);rid=newest['id'][2:]
        raw,_=checkpoint_bytes(run.path/'server-world/world.save');saved=json.loads(raw.decode('utf-8-sig').split('\n',2)[2]);receipt=next(r for r in saved['receipts'] if r['requestId']==rid);p=receipt['fingerprint'].split('|')
        command=dict(requestId=rid,actor=p[0],item=p[1],target=p[2],value=p[3],action=int(p[4]),expectedRevision=int(p[5]),x=float(p[6]),y=float(p[7]),zone=p[8],visit=int(p[9]))
        sender=next(v for v in clients if v.profile==command['actor']);sender.serial+=1;write(sender.out/'control.json',dict(serial=sender.serial,kind='command',request=dict(requestId=rid,protocol=3,command=command)))
        wait(lambda:(r if (r:=read(sender.out/('reply-'+rid+'.json'))) and r['duplicate'] else None),'exact duplicate delivery');require(len(piece(square)['attachments'])==len(old)+1,'retry duplicate')
        for v in clients:wait(lambda:v.state()['view']['sandpit']['moulds']==state()['moulds'],'converged')
        record('Competing overlap and exact command redelivery yield one prop and four converged clients')
        # Owned wall moves; attached relative coordinates remain exact, ground independent.
        attached=copy.deepcopy(piece(wall)['attachments']);grounds=copy.deepcopy(state()['ground']);button(a,'Build');six.select(a,wall);button(a,'Edit');button(a,'Move');floor(a,4362.5,350);button(a,'Confirm sand placement')
        wait(lambda:piece(wall)['x']==4362.5 and piece(wall)['y']==350,'piece move');settled(a)
        require(piece(wall)['attachments']==attached and state()['ground']==grounds,'attached follow/ground stay');capture(a,'phone-moved-piece');capture(c,'tablet-shared-result')
        record('Actual owner Move keeps piece-relative decorations and ground positions independent')
        expected=copy.deepcopy(state());button(d,'Leave sandpit');wait(lambda:not next(m for m in state()['members'] if m['actor']==d.profile)['attending'],'leave')
        d.close();wait(lambda:len(server.state()['connected'])==3,'departure');d=run.start('client',d.profile);clients[-1]=d;home.ready(d);require(home.command(d,32,value='start')['accepted'],'rejoin')
        require(state()['moulds']==expected['moulds'] and state()['ground']==expected['ground'],'reconnect retention');record('Independent departure/reconnect retains positions while three builders continue')
        wait(lambda:(a.out/'sand-film/times.txt').exists(),'actual recording',60)
        ts=[float(t) for t in (a.out/'sand-film/times.txt').read_text().splitlines()];frames=sorted((a.out/'sand-film').glob('*.png'));concat=out/'frames.txt';concat.write_text(''.join("file '"+p.as_posix()+"'\nduration "+str(ts[j+1]-ts[j] if j+1<len(ts) else .125)+'\n' for j,p in enumerate(frames)))
        subprocess.run([shutil.which('ffmpeg'),'-y','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-an',str(out/'actual-flexible-decoration-silent.mp4')],check=True)
        run.close();a,b,c,d=starts()
        require(state()['moulds']==expected['moulds'] and state()['ground']==expected['ground'],'server save reopen');capture(c,'tablet-reopened');record('Native server checkpoint reopening retains stable decoration IDs and exact saved positions')
        # Ground selection/editing uses its creator, attached edits use piece owner.
        button(a,'Build');floor(a,g0['x'],g0['y']);button(a,'Edit');button(a,'Remove');capture(a,'phone-ground-removal-confirmation');button(a,'Keep it');button(a,'Back')
        require(state()['ground']==expected['ground'],'cancel ground remove');r=home.command(b,32,value='ground-remove',target='ground@'+str(state()['round']),item=g0['id']);require(not r['accepted'] and r['outcome']=='not-your-sand-piece','ground creator')
        r=home.command(b,32,value='decor-remove',target=wall+'@'+str(state()['round']),item=str(piece(wall)['version'])+':'+piece(wall)['attachments'][0]['id']);require(not r['accepted'] and r['outcome']=='not-your-sand-piece','piece owner')
        button(a,'Edit');button(a,'Remove');button(a,'Yes, this only');wait(lambda:not any(x['id']==g0['id'] for x in state()['ground']),'owner removes ground');require(len(state()['ground'])==len(expected['ground'])-1,'only this prop')
        record('Native creator/owner permissions and ground Remove confirmation protect others\u2019 work')
        # Only stopped synthetic state is converted to a genuine old-format fixture.
        run.close();legacy=ownsave();g=legacy['sandpit'];legacy['schema']=52;g['format']=3;g.pop('ground',None)
        for m in g['moulds']:m['attachments']=[dict(slot=i,kind=('flag' if i<2 else 'shell' if i<4 else 'window')) for i in range(6)]
        beforepieces=copy.deepcopy(g['moulds']);other=copy.deepcopy(legacy['kingdom']);payload=json.dumps(legacy,separators=(',',':')).encode();savefile=run.path/'server-world/world.save';savefile.write_bytes(b'LITTLEWEEPS-SOLO-1\n'+hashlib.sha256(payload).hexdigest().encode()+b'\n'+payload)
        a,b,c,d=starts();require(server.state()['view']['schema']==53,'native migrate53')
        for prev,m in zip(beforepieces,state()['moulds']):
            require(all(m[k]==v for k,v in prev.items() if k!='attachments'),'piece fields retained')
            require(len(m['attachments'])==6 and len({x['id'] for x in m['attachments']})==6 and all(x['legacy'] for x in m['attachments']),'six props retained once')
        require(server.state()['view']['kingdom']==other,'unrelated world retained')
        # Shared reconnect may already restore the active production activity.
        for v in (a,c):
            if not any(x['name']=='Build' for x in v.input('inspect')['controls']):
                require(home.command(v,32,value='start')['accepted'],'resume migrated activity')
        capture(a,'phone-migrated');capture(c,'tablet-migrated')
        record('Native format3 migration retains every piece field, all six visible anchors and unrelated world data')
        ground(a,'shell',4405,515);button(a,'Build');button(a,'Family reset');wait(lambda:bool(state()['reset']['token']),'reset vote');button(c,'Keep castle');wait(lambda:not state()['reset']['token'],'decline')
        require(len(state()['moulds'])==len(beforepieces) and len(state()['ground'])==1,'decline retains new ground and migrated attachments')
        button(a,'Family reset');wait(lambda:bool(state()['reset']['token']),'new reset');require(len(state()['reset']['voters'])==4,'four explicit voters')
        for v in clients:button(v,'Clear sandpit')
        wait(lambda:not state()['moulds'] and not state()['ground'],'clear complete creation');record('Four actual explicit reset votes clear new ground/attached state; decline preserves it')
        passed=True
    finally:
        if not passed:
            for j,v in enumerate(clients):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+str(j))
                    except Exception:pass
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in line or 'NullReference' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=args.build,actualSimultaneousClients=4,checks=checks,runtimeErrors=errors,recording=None if args.depth_resume else 'actual-flexible-decoration-silent.mp4',physicalDeviceRun=False,liveDataModified=False))
        require(not errors,'runtime errors '+str(errors[:3]))
    print('PASS '+str(out),flush=True)

if __name__=='__main__':main()
