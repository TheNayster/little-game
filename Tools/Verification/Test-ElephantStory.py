"""Missing Ball pilot: actual native players, isolated authority, no video."""
import sys,time,argparse,importlib.util,math
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--review',action='store_true');p.add_argument('--basic',action='store_true');p.add_argument('--d3d11',action='store_true');args=p.parse_args()
 run=Run(args.build,interactive=args.review,review_controls=args.review,extended_test_lifetime=True,graphics_api='d3d11' if args.d3d11 else None)
 out=run.path/'elephant-story';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def inspect(v):return v.input('inspect')
 def z():return server.state()['view']['zoo']
 def s():return z()['story']
 def animal():return z()['animals'][0]
 def food(v):return next(f for f in z()['food'] if f['actor']==v.profile)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
 def tap(v,name):
  wait(lambda:not inspect(v)['pending'],'tap ready',10);v.input('touchButton',text=name);time.sleep(.15)
 def enter(v,x=1200):
  if inspect(v)['zone']!='zoo-savanna':
   if inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
   cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna')
  cmd(v,0,x=x,y=100);time.sleep(.7)
 def start(v):
  old=s()['session'];cmd(v,0,x=1390,y=100);time.sleep(.7);tap(v,'Start Missing Ball');wait(lambda:s()['session']>old,'deliberate start',5)
 def pickup(v):
  cmd(v,0,x=s()['x']-100,y=100);time.sleep(.7);tap(v,'Collect elephant ball');wait(lambda:s()['carrier']==v.profile,'actual auto-walk pickup',15)
 def deliver(v):
  cmd(v,0,x=1290,y=100);time.sleep(.7);tap(v,'Return elephant ball');wait(lambda:s()['phase']>=3,'actual auto-walk return',15)
 def complete():wait(lambda:s()['phase']==5,'quiet completion',35)
 def offer(v):
  cmd(v,0,x=1560,y=100);cmd(v,22,value='take',target='elephant');f=food(v);cmd(v,0,x=1200+(f['slot']-1.5)*100,y=100);cmd(v,22,value='offer',target='elephant')
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots[:2 if args.review else 4]];a,b=clients[:2]
  for v in clients:home.ready(v);v.input('resize',x=1280,y=591);enter(v)
  cmd(b,1,value='orange-pup')
  start(a);wait(lambda:animal()['phase']==15,'introductory clue',10);home.capture(a,out,'clue-phone')
  if args.review:
   cmd(a,0,x=s()['x']-120,y=100);time.sleep(.8);home.capture(a,out,'partly-hidden-phone')
   a.input('resize',x=640,y=400);time.sleep(.6);home.capture(a,out,'partly-hidden-small-phone');a.input('resize',x=1280,y=591);time.sleep(.6)
  pickup(a);home.capture(a,out,'carried-phone');tap(a,'Put elephant ball down');wait(lambda:s()['carrier']=='','put down',5)
  pickup(b);deliver(b);wait(lambda:s()['phase']==4,'friendly trunk reaction',30);home.capture(b,out,'reaction-phone')
  sample=inspect(b);write(out/'contact.json',sample);complete();record('Actual start/clue/discovery/pickup/put-down/sibling-return/reaction/completion')
  time.sleep(8.1);start(a);pickup(a)
  cmd(b,0,x=1560,y=100);cmd(b,22,value='take',target='elephant');f=food(b);cmd(b,0,x=1200+(f['slot']-1.5)*100,y=100)
  deliver(a);wait(lambda:s()['phase']==4,'second reaction',30)
  require(animal()['phase']==17,'reaction ended before priority fixture');before_offer=time.monotonic();cmd(b,22,value='offer',target='elephant');wait(lambda:animal()['owner']==b.profile,'food priority',3);write(out/'food-priority.json',dict(secondsToObservedOwner=time.monotonic()-before_offer,session=s()['session']))
  wait(lambda:s()['phase']==5,'no replay of interrupted reaction',3)
  wait(lambda:animal()['consumed'],'food completes',40);record('Deliberate replay and food takes priority over return acknowledgment')
  if args.review:
   wait(lambda:animal()['owner']=='','food finishes',10);time.sleep(8.1);cmd(a,0,x=1390,y=100);cmd(b,0,x=1200,y=100);time.sleep(.8)
   home.capture(a,out,'owner-ready');write(run.path/'live-review-ready.json',dict(build=args.build,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],ready=True,checks=checks,recordedVideo=False,liveFamilyTouched=False))
   print('READY '+str(run.path),flush=True)
   while any(v.process.poll() is None for v in clients):time.sleep(.5)
   passed=True;return
  if args.basic:passed=True;return
  # Every authored location on phone/tablet layouts, real taps and walks.
  wait(lambda:animal()['owner']=='','food finished',12);time.sleep(8.1);spots=set()
  for i in range(2):
   start(a);spots.add(s()['hidingSpot'])
   for width,height,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
    a.input('resize',x=width,y=height);cmd(a,0,x=s()['x'],y=100);time.sleep(.8);home.capture(a,out,f'spot-{s()["hidingSpot"]}-{label}')
    pickup(a);tap(a,'Put elephant ball down');wait(lambda:s()['carrier']=='','layout put down',5)
   pickup(a);deliver(a);complete();time.sleep(8.1)
  require(spots=={0,1},'not all hiding spots tested');record('Both hiding spots reachable by actual taps/walks on phone/tablet/small-phone layouts')
  for v in clients:v.input('resize',x=1280,y=591);cmd(v,0,x=1390,y=100)
  time.sleep(.8);prior=s()['nextSession']
  with ThreadPoolExecutor(4) as pool:list(pool.map(lambda v:tap(v,'Start Missing Ball'),clients))
  wait(lambda:s()['session']==prior+1,'one simultaneous session',5);session=s()['session']
  for v in clients:cmd(v,0,x=s()['x'],y=100)
  time.sleep(.8)
  with ThreadPoolExecutor(4) as pool:list(pool.map(lambda v:tap(v,'Collect elephant ball'),clients))
  wait(lambda:bool(s()['carrier']),'simultaneous pickup',15);carrier=next(v for v in clients if v.profile==s()['carrier']);require(s()['session']==session,'duplicate session')
  for v in clients:cmd(v,0,x=1390,y=100)
  time.sleep(.8)
  with ThreadPoolExecutor(4) as pool:list(pool.map(lambda v:tap(v,'Return elephant ball'),clients))
  complete();require(s()['session']==session and s()['carrier']=='','duplicate return');record('Four simultaneous starts/pickups/returns: one stable session, ball and completion')
  time.sleep(8.1);start(a);pickup(a);cmd(a,7,value='creek');wait(lambda:s()['carrier']=='','carrier leaves',5);pickup(b)
  b.close();wait(lambda:s()['carrier']=='','carrier disconnect',10);b=run.start('client',run.slots[1]['profile']);clients[1]=b;home.ready(b);enter(b);require(s()['session']>0 and s()['carrier']=='','reconnect restored stale owner')
  d=clients[-1];d.close();d=run.start('client',run.slots[-1]['profile']);clients[-1]=d;home.ready(d);enter(d);require(inspect(d)['zoo']['story']['session']==s()['session'],'late join mismatch')
  pickup(d);deliver(d);complete();record('Carrier travel/disconnect, reconnect and late join retain one reachable ball and current state')
  enter(a);time.sleep(8.1);start(a);pickup(a)
  # Server rejects replacing a ball with a bucket or a prepared snack.
  for op in ['take','snack-begin']:
   r=home.command(a,22,value=op,target='elephant');require(not r['accepted'] and r['outcome']=='hands-full','ball silently replaced')
  tap(a,'Put elephant ball down');wait(lambda:s()['carrier']=='','switch put down',5)
  offer(a);ticket=food(a)['ticket'];r=home.command(a,22,value='story-pickup',target=str(s()['session']));require(not r['accepted'] and food(a)['ticket']==ticket,'story lost food')
  wait(lambda:food(a)['species']=='','food retained through consumption',45)
  cmd(a,0,x=2020,y=100);time.sleep(.8);tap(a,'Prepare elephant snack');wait(lambda:inspect(a)['elephantSnackOpen'],'snack opens',15);tap(a,'Add snack leaves');tap(a,'Carry elephant snack');wait(lambda:food(a)['species']=='elephant','snack carry',8)
  wait(lambda:food(a)['offered'],'snack offer',20);wait(lambda:food(a)['species']=='','snack retained through consumption',45)
  pickup(a);deliver(a);complete();record('Ball/bucket/prepared-snack switches preserve committed food and require explicit ball put-down')
  time.sleep(8.1);start(a)
  tap(b,'Choose elephant brush');wait(lambda:b.profile in z()['careMembers'],'brush helper',5)
  cmd(a,0,x=1100,y=100);time.sleep(.8);tap(a,'Splash button')
  cmd(a,0,x=480,y=100);time.sleep(.8);tap(a,'Rustling leaves');cmd(a,0,x=1920,y=100);time.sleep(.8);tap(a,'Butterfly flowers')
  pickup(a);deliver(a);require(b.profile in z()['careMembers'],'story canceled care');offer(clients[2]);wait(lambda:animal()['owner']==clients[2].profile,'food during story/care/water',2)
  tap(b,'Put elephant brush away');wait(lambda:animal()['owner']=='','coexisting feed finishes',45);complete();record('Story alongside care, water and both surprises; food priority and sibling care retained')
  time.sleep(8.1)
  for button,close in [('Zoo toy camera','Close Zoo camera'),('Zoo photo album','Return to Zoo'),('Zoo map','Close Zoo map')]:
   cmd(a,0,x=1390,y=100);time.sleep(.8);before=s()['session'];tap(a,button)
   # Synthetic control-name tapping still routes through the real raycaster.
   a.input('touchButton',text='Start Missing Ball');time.sleep(.3);require(s()['session']==before,'overlay leaked story tap');tap(a,close)
  start(a);pickup(a);session=s()['session'];a.input('application-pause');time.sleep(.3);a.input('application-resume');require(s()['session']==session,'pause restarted story')
  for v in clients:cmd(v,7,value='creek')
  wait(lambda:s()['phase']==0 and s()['carrier']=='','all leave clears',5);record('Map/camera/album shield story taps; pause/resume and all-player exhibit exit reset safely')
  enter(a,4000);before=z()['animals'][1]['fed'];tap(a,'Take leaves for giraffe');wait(lambda:z()['animals'][1]['fed']==before+1,'unchanged giraffe feeds',55)
  record('Unchanged giraffe feeding and Zoo navigation regression')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:
   log=v.out/'player.log'
   if log.exists():errors.extend(line for line in log.read_text(errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
