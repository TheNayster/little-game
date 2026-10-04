"""Only remaining current P9 depth/oriented anchors and native reconnect checks.
Uses the completed integration's OWN synthetic checkpoint; restores it when stopped.
"""
import argparse,hashlib,json,importlib.util,sys,copy,time
from pathlib import Path
sys.path.insert(0,str(next(p for p in Path(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT
from shared_garden_runtime import Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);ap.add_argument('--of',required=True);a=ap.parse_args();run=Run(a.build,resume=a.of,extended_test_lifetime=True)
    prior=read(run.path/'sandcastle-p9-integration-acceptance.json');require(prior and prior['passed'] and prior['build']==a.build,'completed owned current-source run')
    for f in read(run.folder/'source-manifest.json')['files']:
        if f['path'].startswith('Unity/'):require(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'],'stale source')
    out=run.path/'sandcastle-p9-depth-reconnect';suffix=1
    while out.exists():suffix+=1;out=run.path/('sandcastle-p9-depth-reconnect-'+str(suffix))
    out.mkdir();p=run.path/'server-world/world.save';backup=p.read_bytes();magic,digest,payload=backup.split(b'\n',2);require(hashlib.sha256(payload).hexdigest().encode()==digest,'owned checksum')
    source=next(read(f)['view'] for f in run.path.glob('*/view.json') if len(read(f).get('view',{}).get('sandpit',{}).get('moulds',[]))==16)
    body=json.dumps(source,separators=(',',':')).encode();p.write_bytes(magic+b'\n'+hashlib.sha256(body).hexdigest().encode()+b'\n'+body)
    print('EVIDENCE '+str(out),flush=True);clients=[];passed=False;checks=[]
    def state():return server.state()['view']['sandpit']
    def command(v,action,**kw):return home.command(v,action,**kw)
    def play(v,op,id,item=''):return command(v,32,value=op,target=id+'@'+str(state()['round']),item=item)
    def button(v,name):wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name);v.input('touchButton',text=name)
    def select(v,id):
        s=v.input('inspect');i=next(i for i,m in enumerate(s['sandpit']['moulds']) if m['id']==id);m=s['sandpit']['moulds'][i];p=s['sandPieceScreenPoints'][i];r=next(c['bounds'] for c in s['controls'] if c['name']=='Build');scale=r['width']/150;depth=1.14+(.86-1.14)*(m['y']-70)/470
        ox=0 if m['shape'] in ('round','square') or m['orientation']==90 else -100;oy=165 if m['shape'] in ('round','square') else 140 if m['orientation']==90 else 40
        for action in ('touch-begin','touch-end'):v.input(action,role='screen',x=p['x']+ox*depth*scale,y=p['y']+oy*depth*scale,finger=71)
        wait(lambda:v.input('inspect')['sandpitSelection']==i,'depth painted-face selection')
    try:
        server=run.start('server')
        for i,slot in enumerate(run.slots):
            v=run.start('client',slot['profile']);clients.append(v);home.ready(v);v.input('resize',x=1280 if i<2 else 1024,y=591 if i<2 else 768);home.ready(v)
            if next(q for q in v.state()['view']['players'] if q['id']==v.profile)['zone']!='daycare':require(command(v,7,value='daycare')['accepted'],'test visitor returns')
            require(command(v,32,value='start')['accepted'],'join preserved creation')
        a1,b,c,d=clients
        back=next(m['id'] for m in state()['moulds'] if m['shape']=='round' and m['x']==4235 and m['y']==460);front=next(m['id'] for m in state()['moulds'] if m['x']==4235 and m['y']==130)
        for v,label in ((a1,'phone'),(c,'tablet')):
            select(v,back);home.capture(v,out,label+'-back-piece-selection');select(v,front);home.capture(v,out,label+'-front-piece-selection')
        checks.append('Actual phone/tablet painted-face taps select visible rear and foreground pieces in full16 without rim/cast/shadow interception')
        wall=next(m for m in state()['moulds'] if m['orientation']==90 and m['x']==4235);owner=next(v for v in clients if v.profile==wall['creator']);require(play(owner,'remove',wall['id'],str(wall['version']))['accepted'],'owned test wall remove')
        require(command(owner,32,value='place',target='place@'+str(state()['round']),item='gate:90',x=wall['x'],y=wall['y'])['accepted'],'legal gate90 replacement')
        gate=next(m for m in state()['moulds'] if m['shape']=='gate' and m['orientation']==90);require(command(owner,0,x=gate['x']-60,y=gate['y']-55)['accepted'],'gate work')
        for op in ('water','scoop','scoop','scoop','tip'):require(play(owner,op,gate['id'])['accepted'],'oriented gate build')
        for item in ('0:flag','1:flag','2:shell','4:door'):require(play(owner,'decorate',gate['id'],item)['accepted'],'oriented anchor')
        time.sleep(2)
        for v,label in ((a1,'phone'),(c,'tablet')):
            select(v,gate['id']);button(v,'Decorate');home.capture(v,out,label+'-gate90-combined-anchors')
        expected=copy.deepcopy(state()['moulds']);toy=copy.deepcopy(state()['toy']);b.close();wait(lambda:len(server.state()['connected'])==3,'three remaining native clients');require(state()['moulds']==expected and state()['toy']==toy,'network departure preserved creation')
        b=run.start('client',run.slots[1]['profile']);clients[1]=b;home.ready(b);b.input('resize',x=1280,y=591);require(command(b,32,value='start')['accepted'],'reconnect join');wait(lambda:b.input('inspect')['sandpit']['moulds']==expected,'authoritative current castle on reconnect');info=b.input('inspect');require(not any(info['sandBuilderEvents']) and all(op=='ready' for op in info['sandBuilderReactions']) and info['sandAudioEvents']==0,'no historical reconnect feedback');home.capture(b,out,'phone-native-reconnect')
        checks += ['Actual legal gate90 rebuild and combined near/far flag/base/face anchors at phone/tablet aspects','One native client disconnects while three remain, then a new current client reconnects to the same authority; creation/toy retained and no historical feedback']
        passed=True
    finally:
        if not passed:
            for i,v in enumerate(clients):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+str(i))
                    except Exception:pass
        run.close();p.write_bytes(backup);errors=[]
        for v in run.instances:errors += [s for s in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in s or 'NullReference' in s]
        write(out/'result.json',dict(passed=passed and not errors,build=a.build,checks=checks,peakActualNativeClients=4,clientDisconnectAndReconnect=True,runtimeErrors=errors,originalSyntheticCheckpointRestored=True));require(not errors,'native errors')
    print('PASS '+str(checks),flush=True)
if __name__=='__main__':main()
