"""Milestone11: four actual isolated native clients, real UI raycasts, no video."""
import sys,time,argparse,importlib.util
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--review',action='store_true');p.add_argument('--followup',action='store_true');p.add_argument('--coexist-only',action='store_true');args=p.parse_args();args.followup |= args.coexist_only
 run=Run(args.build,interactive=args.review,review_controls=args.review,extended_test_lifetime=True,graphics_api='d3d11')
 out=run.path/'elephant-habitat';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def inspect(v):return v.input('inspect')
 def z():return server.state()['view']['zoo']
 def h():return z()['habitat']
 def animal():return z()['animals'][0]
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
 def tap(v,name):
  wait(lambda:not inspect(v)['pending'],'ready tap',10);v.input('touchButton',text=name);time.sleep(.18)
 def enter(v,x=700):
  if inspect(v)['zone']!='zoo-savanna':
   if inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
   cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna')
  cmd(v,0,x=x,y=100);time.sleep(.7)
 def open(v,kind):
  enter(v,750);tap(v,'Decorate elephant habitat');wait(lambda:inspect(v)['elephantHabitatOpen'],'open basket',5);tap(v,'Choose habitat '+str(kind))
 def preview(v,slot):
  tap(v,'Habitat slot '+str(slot));wait(lambda:inspect(v)['elephantHabitatPreview']==slot,'local preview',5)
 def place(v,kind,slot):
  before=len(h()['props']);open(v,kind);preview(v,slot);bounds=next(c['bounds'] for c in inspect(v)['controls'] if c['name']=='Confirm habitat placement');tap(v,'Confirm habitat placement');v.input('touch-begin',role='screen',x=bounds['x']+bounds['width']/2,y=bounds['y']+bounds['height']/2,finger=77);v.input('touch-end',role='screen',x=bounds['x']+bounds['width']/2,y=bounds['y']+bounds['height']/2,finger=77);wait(lambda:len(h()['props'])==before+1,'committed placement',6);return h()['props'][-1]
 def remove(v,prop):
  wait(lambda:not(h()['usingId']==prop['id'] and animal()['phase'] in (18,19,20)),'prop released',30)
  cmd(v,22,value='habitat-remove',target='-1',item=prop['id']+'/'+str(prop['revision']))
 def prepare(v):
  f=next(f for f in z()['food'] if f['actor']==v.profile)
  if f['species']=='':cmd(v,0,x=1560,y=100);cmd(v,22,value='take',target='elephant');f=next(f for f in z()['food'] if f['actor']==v.profile)
  cmd(v,0,x=1200+(f['slot']-1.5)*100,y=100)
 def offer(v):
  prepare(v);cmd(v,22,value='offer',target='elephant')
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots[:2 if args.review else 4]];a,b=clients[:2]
  for v in clients:home.ready(v);v.input('resize',x=1280,y=591);enter(v)
  cmd(b,1,value='orange-pup')
  if not args.followup:
   open(a,1);preview(a,0);require(h()['props']==[],'preview changed authority');tap(a,'Cancel habitat preview');require(h()['props']==[],'cancel changed habitat');record('Picture selection and local preview cancellation leave authority unchanged')
   # Same slot claims with independently prepared previews; exactly one winner.
   for v in clients:open(v,1);preview(v,0)
   with ThreadPoolExecutor(len(clients)) as pool:list(pool.map(lambda v:tap(v,'Confirm habitat placement'),clients))
   wait(lambda:len(h()['props'])==1,'one atomic winner',8);time.sleep(.6);require(len(h()['props'])==1,'duplicate confirmation')
   winner=next(v for v in clients if v.profile==h()['props'][0]['creator']);first=h()['props'][0]
   for v in clients:
    if inspect(v)['elephantHabitatOpen']:tap(v,'Cancel habitat preview')
   other=next(v for v in clients if v!=winner);second=place(other,2,2)
   record('Simultaneous native claims yield one stable prop; another child independently places a compatible outer prop')
   r=home.command(other,22,value='habitat-remove',target='-1',item=first['id']+'/0');require(not r['accepted'] and r['outcome']=='habitat-friend','friend removed prop')
   r=home.command(winner,22,value='habitat-move',target='1',item=first['id']+'/0');require(not r['accepted'] and h()['props'][0]['slot']==0,'overlap move lost original')
   record('Persistent creator ownership and rejected overlap moves preserve original placement')
   prepare(other);wait(lambda:animal()['phase']==19,'elephant uses placed prop',55);used=next(p for p in h()['props'] if p['id']==h()['usingId']);user=next(v for v in clients if v.profile==used['creator'])
   home.capture(a,out,'two-props-use-phone')
   r=home.command(user,22,value='habitat-remove',target='-1',item=used['id']+'/'+str(used['revision']));require(not r['accepted'] and r['outcome']=='habitat-using','support moved while in use')
   t=time.monotonic();offer(other);wait(lambda:animal()['owner']==other.profile,'feeding priority',3);write(out/'food-priority.json',dict(observedSeconds=time.monotonic()-t))
   wait(lambda:animal()['owner']=='','food completes',45);record('Elephant uses a committed prop; in-use removal rejects; offered food takes priority')
   if args.review:
    # Actual authoritative save reopening in an isolated server process.
    before=h()['props'];profiles=[v.profile for v in clients];[v.close() for v in clients];server.close();server=run.start('server');clients=[run.start('client',profile) for profile in profiles];a,b=clients[:2];wait(lambda:server.state() and server.state().get('view'),'reopen authority',35)
    require(h()['props']==before,'save/reopen lost arrangement');
    for v in clients:home.ready(v);enter(v,1200)
    home.capture(a,out,'owner-ready');record('Actual isolated server save/reopen preserves both creators and arrangement')
    write(run.path/'live-review-ready.json',dict(build=args.build,checks=checks,ready=True,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],graphicsApi='Direct3D11',recordedVideo=False,liveFamilyTouched=False))
    print('READY '+str(run.path),flush=True)
    while any(v.process.poll() is None for v in clients):time.sleep(.5)
    passed=True;return
   remove(winner,first);remove(other,second)
   # Each legal slot/type: real picture taps, actual authority approaches and use.
   for kind,slots in [(0,[1]),(1,[0,1]),(2,[1,2])]:
    for slot in slots:
     prepare(b);prop=place(a,kind,slot);require(len(h()['props'])==1,'repeat confirm duplicated prop')
     wait(lambda:animal()['phase']==19 and h()['usingId']==prop['id'],'legal use anchor',55)
     home.capture(a,out,f'use-kind-{kind}-slot-{slot}');require(animal()['phase']==19,'use must still be active before offered food');cmd(b,22,value='offer',target='elephant');wait(lambda:animal()['owner']==b.profile,'per-kind food priority',3);wait(lambda:animal()['owner']=='','feed completes',45)
     for width,height,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
      a.input('resize',x=width,y=height);cmd(a,0,x=1200,y=100);time.sleep(.5);home.capture(a,out,f'kind-{kind}-slot-{slot}-{label}')
     a.input('resize',x=1280,y=591);remove(a,prop)
   record('All five legal slot/prop combinations; native phone/tablet layouts, repeated confirms and feeding during each behavior')
  if not args.coexist_only:
   prop=place(a,1,0);offer(b);wait(lambda:animal()['owner']==b.profile,'edit while food busy',3)
   cmd(a,0,x=1050,y=100);time.sleep(.8);open(a,1);tap(a,'Move habitat prop 0');wait(lambda:inspect(a)['elephantHabitatOpen'],'edit opens',5)
   preview(a,1);tap(a,'Cancel habitat preview');require(h()['props'][0]['slot']==0,'cancel moved original')
   open(a,1);tap(a,'Move habitat prop 0');preview(a,1);tap(a,'Confirm habitat placement');wait(lambda:h()['props'][0]['slot']==1,'move accepted',6)
   stale=home.command(a,22,value='habitat-move',target='0',item=prop['id']+'/0');require(not stale['accepted'] and h()['props'][0]['slot']==1,'stale edit accepted')
   open(a,1);tap(a,'Move habitat prop 0');wait(lambda:not h()['usingId'],'safe edit exit before return',30);tap(a,'Return habitat prop to basket');wait(lambda:len(h()['props'])==0,'actual basket return',6);record('Actual move/return controls, move cancellation and stale edit rejection')
   prop=place(a,1,0);a.close();wait(lambda:len(h()['props'])==1,'disconnect preserves placement',8)
   r=home.command(b,22,value='habitat-remove',target='-1',item=prop['id']+'/0');require(not r['accepted'],'unavailable creator grants no destructive rights')
   a=run.start('client',run.slots[0]['profile']);clients[0]=a;home.ready(a);enter(a);require(h()['props'][0]['creator']==a.profile,'creator reconnect changed identity');remove(a,prop)
   record('Creator disconnect/reconnect preserves arrangements; unavailable creator has no implicit edit transfer')
   for button,close in [('Zoo toy camera','Close Zoo camera'),('Zoo photo album','Return to Zoo'),('Zoo map','Close Zoo map')]:
    enter(a);tap(a,button);a.input('touchButton',text='Decorate elephant habitat');time.sleep(.25);require(not inspect(a)['elephantHabitatOpen'],'overlay leaked placement input');tap(a,close)
   record('Map/camera/album shields underlying placement')
   prop=place(a,2,2);before=h()['props'];profiles=[v.profile for v in clients];[v.close() for v in clients];server.close();server=run.start('server');clients=[run.start('client',profile) for profile in profiles];a,b=clients[:2];wait(lambda:server.state() and server.state().get('view'),'reopened authority',35);require(h()['props']==before,'real save lost props')
   for v in clients:home.ready(v)
   record('Actual authoritative process save/reopen retains committed placement')
  else:place(a,2,2)
  # Existing activities coexist with a saved committed leafy prop.
  for v in clients:enter(v,1200)
  arrangement=h()['props'];cmd(a,0,x=1390,y=100);time.sleep(.7);tap(a,'Start Missing Ball');wait(lambda:z()['story']['session']>0,'story with habitat',5)
  cmd(b,0,x=1740,y=100);time.sleep(.7);tap(b,'Choose elephant brush');wait(lambda:b.profile in z()['careMembers'],'care with habitat',5)
  cmd(clients[2],0,x=690,y=100);time.sleep(.7);water=z()['waterSequence'];tap(clients[2],'Splash button');wait(lambda:z()['waterSequence']>water,'water with habitat',5)
  cmd(clients[2],0,x=480,y=100);time.sleep(.7);tap(clients[2],'Rustling leaves')
  cmd(clients[3],0,x=1920,y=100);time.sleep(.7);tap(clients[3],'Butterfly flowers')
  story=z()['story'];cmd(a,0,x=story['x'],y=100);time.sleep(.7);tap(a,'Collect elephant ball');wait(lambda:z()['story']['carrier']==a.profile,'story pickup alongside props',20)
  cmd(a,0,x=1390,y=100);time.sleep(.7);tap(a,'Return elephant ball');wait(lambda:z()['story']['phase']>=3,'story return alongside care',20)
  require(b.profile in z()['careMembers'],'habitat/story lost sibling care');offer(clients[3]);wait(lambda:animal()['owner']==clients[3].profile,'food during all activities',3)
  tap(b,'Put elephant brush away');wait(lambda:animal()['owner']=='','coexisting food finishes',45)
  cmd(b,0,x=2020,y=100);time.sleep(.7);tap(b,'Prepare elephant snack');wait(lambda:inspect(b)['elephantSnackOpen'],'snack with habitat',20);tap(b,'Add snack leaves');tap(b,'Carry elephant snack')
  wait(lambda:next(f for f in z()['food'] if f['actor']==b.profile)['species']=='elephant','existing snack queue',8)
  wait(lambda:next(f for f in z()['food'] if f['actor']==b.profile)['species']=='','snack finishes through queue',55)
  wait(lambda:z()['story']['phase']==5,'story acknowledgment eventually finishes',45);require(h()['props']==arrangement,'existing play altered placement')
  record('Saved habitat coexists with care, water, snacks, Missing Ball, both surprises and shared feeding')
  enter(a,4000);before=z()['animals'][1]['fed'];tap(a,'Take leaves for giraffe');wait(lambda:z()['animals'][1]['fed']==before+1,'unchanged giraffe feeds',55);record('Unchanged giraffe shared-code regression')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:
   log=v.out/'player.log'
   if log.exists():errors.extend(line for line in log.read_text(errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,physicalDevices=False,graphicsApi='Direct3D11',recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
