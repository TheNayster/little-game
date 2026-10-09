"""Scoped Savanna presentation/authority checks in four actual D3D11 clients; no video."""
import sys,time,argparse,importlib.util
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
SPECIES={'giraffe':(1,'leaves'),'zebra':(2,'hay'),'lion':(3,'meat')}
def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--species',choices=SPECIES);p.add_argument('--quick',action='store_true');args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/'savanna';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def inspect(v):return v.input('inspect')
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def rec(s):checks.append(s);print('PASS '+s,flush=True)
 def z():return server.state()['view']['zoo']
 def food(v):return next(f for f in z()['food'] if f['actor']==v.profile)
 def enter(v,center):
  if inspect(v)['zone']!='zoo-savanna':
   if inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
   cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna')
  cmd(v,0,x=center,y=100);time.sleep(.4)
 def tap(v):v.input('touchButton',text='Take '+kind+' for '+species)
 def a():return z()['animals'][panel]
 def cue(v):
  # Network snapshot receipt and screen rendering may occur on adjacent frames.
  # Wait briefly for that frame boundary, then compare the client's own state.
  for _ in range(8):
   e=inspect(v);animal=e['zoo']['animals'][panel];f=next(f for f in e['zoo']['food'] if f['actor']==v.profile)
   expected='idle' if f['species']!=species else 'carrying' if not f['offered'] else 'waiting' if animal['owner']!=v.profile else 'finished' if animal['consumed'] else 'eating' if animal['phase']==6 else 'approaching'
   if e['savannaCue']==expected or f['species']!=species and e['savannaCue'] in ('collecting',''):return e
   time.sleep(.03)
  require(False,str((species,v.profile,expected,e['savannaCue'])))
 def finish(before,n,order=None):
  last=before;seen=[]
  def done():
   nonlocal last
   animal=a()
   if animal['fed']>last:seen.append(animal['owner']);last=animal['fed']
   return animal['fed']==before+n and all(food(v)['species']=='' for v in clients)
  wait(done,'finish '+species,120)
  if order is not None:require(seen==order,str(('queue order',seen,order)))
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
  for i,v in enumerate(clients):
   cmd(v,1,value=['blue-pup','orange-pup','muffin','chloe'][i]);w,h=(640,400) if i==2 else (1280,591) if i%2==0 else (1024,768);v.input('resize',x=w,y=h)
  for species in ([args.species] if args.species else SPECIES):
   panel,kind=SPECIES[species];center=1200+panel*2400
   for i,v in enumerate(clients):enter(v,center+(i-1.5)*100)
   for w,h,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
    clients[0].input('resize',x=w,y=h);time.sleep(.7);home.capture(clients[0],out,species+'-normal-'+label)
   clients[0].input('resize',x=1280,y=591);time.sleep(.7)
   for v in clients:enter(v,center+200)
   before=a()['fed'];events=[inspect(v)['savannaFinishEvents'][panel-1] for v in clients]
   for v in clients:tap(v)
   wait(lambda:all(food(v)['species']==species for v in clients),'four reserved '+species,25);tickets={v.profile:food(v)['ticket'] for v in clients}
   for v in clients:tap(v);tap(v)
   require({v.profile:food(v)['ticket'] for v in clients}==tickets,'repeat pickup replaced ticket')
   wait(lambda:all(food(v)['offered'] for v in clients),'four offered',25);require(len({food(v)['slot'] for v in clients})==4,'merged offering spots')
   home.capture(clients[2],out,species+'-offering-small-phone')
   serving=a()['owner'];order=[serving]+[f['actor'] for f in sorted(z()['food'],key=lambda f:f['ticket']) if f['actor']!=serving]
   for v in clients:cue(v);tap(v)
   wait(lambda:a()['phase']==5,'approach',40)
   for v in clients:cue(v);tap(v)
   wait(lambda:a()['phase']==6 and not a()['consumed'],'eating',40)
   active=next(v for v in clients if v.profile==a()['owner']);tap(active)
   wait(lambda:a()['age']>.5 and not a()['consumed'],'mouth contact',5)
   # Resize before feeding; resizing during the short Eat phase can miss the
   # actual food and accidentally photograph the finish pose under its name.
   for v,label in [(clients[0],'phone'),(clients[1],'tablet')]:
    e=home.capture(v,out,species+'-contact-'+label);animal=e['zoo']['animals'][panel]
    require(animal['phase']==6 and not animal['consumed'] and e['savannaMouthGap']<1,'capture missed mouth contact')
   wait(lambda:a()['consumed'],'consumption',5);e=cue(active);require(e['savannaCue']=='finished','eating cue after consumed')
   for v,label in [(clients[0],'phone'),(clients[1],'tablet'),(clients[2],'small-phone')]:home.capture(v,out,species+'-finish-'+label)
   # First consumption was already observed above; verify the remaining FIFO.
   finish(before+1,3,order[1:]);require(all(inspect(v)['savannaFinishEvents'][panel-1]==events[i]+4 for i,v in enumerate(clients)),'finish missing/repeated')
   rec(species+': four portraits/slots, real repeated pickup/wait/approach/eat taps, accurate phases, mouth contact, exactly four FIFO consumptions and finish effects')
   if args.quick:continue
   for departure in ['waiting','approaching','eating']:
    for v in clients:enter(v,center+200)
    before=a()['fed']
    for v in clients:tap(v)
    wait(lambda:all(food(v)['offered'] for v in clients),'departure queue',25)
    if departure=='waiting':leaver=next(v for v in clients if v.profile!=a()['owner'])
    else:
     wait(lambda:a()['phase']==(5 if departure=='approaching' else 6) and not a()['consumed'],'departure phase',45);leaver=next(v for v in clients if v.profile==a()['owner'])
    kept={v.profile:food(v)['ticket'] for v in clients if v!=leaver};cmd(leaver,7,value='creek')
    require(food(leaver)['species']=='' and all(food(v)['ticket']==kept[v.profile] for v in clients if v!=leaver),'departure changed siblings')
    finish(before,3);enter(leaver,center);require(inspect(leaver)['savannaCue']=='idle','stuck return cue')
    rec(species+': '+departure+' departure cancels only own portion; three siblings finish and return clears controls')
   # Use a real process disconnect/rejoin. VerifyFamilyForeground is for paired
   # family sessions and does not disconnect this token-based loopback fixture.
   before=a()['fed'];v=clients[0];tap(v);wait(lambda:food(v)['offered'],'reconnect offer',25)
   v.close();wait(lambda:food(v)['species']=='','actual disconnect cancels own lease',8)
   v=run.start('client',run.slots[0]['profile']);clients[0]=v;home.ready(v)
   cue(v);require(food(v)['species']=='' and inspect(v)['savannaFinishEvents'][panel-1]==0,'disconnect/rejoin duplicated food or finish')
   events=inspect(v)['savannaFinishEvents'][panel-1];v.input('application-pause');v.input('application-resume');home.ready(v);require(inspect(v)['savannaFinishEvents'][panel-1]==events,'resume replay')
   enter(v,center)
   tap(v);wait(lambda:a()['fed']==before+1,'after reconnect fresh feed',65);wait(lambda:food(v)['species']=='','fresh feed releases',10)
   # A fresh client joining completed history must seed, then stay idle.
   v.close();v=run.start('client',run.slots[0]['profile']);clients[0]=v;home.ready(v);enter(v,center);require(inspect(v)['savannaFinishEvents'][panel-1]==0,'history replay on late join')
   sibling=clients[1];tap(sibling);wait(lambda:food(sibling)['offered'],'late join active offer',25);ticket=food(sibling)['ticket']
   v.close();v=run.start('client',run.slots[0]['profile']);clients[0]=v;home.ready(v);enter(v,center)
   require(food(sibling)['ticket']==ticket and inspect(v)['savannaFinishEvents'][panel-1]==0,'late join changed active offering/replayed finish')
   wait(lambda:food(sibling)['species']=='','late join sibling completion',65);require(a()['fed']==before+2,'late join duplicate consumption')
   v.input('touchButton',text='Zoo map');v.input('touchButton',text='Close Zoo map');cmd(v,7,value='creek');enter(v,center);require(not inspect(v)['pending'] and inspect(v)['savannaCue']=='idle','navigation stuck')
   rec(species+': disconnect/reconnect, completed and active late joins, pause and navigation restore accurate state without old food or finish replay')
  passed=True
 finally:
  if not passed:
   for v in clients:
    if v.process.poll() is None:
     try:home.capture(v,out,'failure-'+v.profile)
     except Exception:pass
  run.close();errors=[];apis=[]
  for v in run.instances:
   lines=(v.out/'player.log').read_text(errors='replace').splitlines();errors.extend(l for l in lines if 'Exception:' in l or 'error CS' in l)
   if v.role=='client':require(any('Direct3D 11' in l for l in lines),'wrong graphics API');apis.append(dict(instance=v.identity,api='Direct3D11',exit=v.process.returncode))
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,clients=apis,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
