"""Elephant-only pilot: native release UI, isolated four-player authority, no video."""
import sys,time,argparse,importlib.util
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--review',action='store_true');p.add_argument('--lifecycle-only',action='store_true');p.add_argument('--priority-only',action='store_true');args=p.parse_args()
 run=Run(args.build,interactive=args.review,review_controls=args.review,extended_test_lifetime=True);out=run.path/'elephant-snack';out.mkdir();clients=[];checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def inspect(v):return v.input('inspect')
 def z():return server.state()['view']['zoo']
 def food(v):return next(f for f in z()['food'] if f['actor']==v.profile)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome'));home.ready(v);return r
 def tap(v,name):
  wait(lambda:not inspect(v)['pending'],'input ready',10);v.input('touchButton',text=name);home.ready(v);time.sleep(.12)
 def enter(v):
  if inspect(v)['zone']=='zoo-savanna':
   cmd(v,0,x=2020,y=100);time.sleep(.6);return
  if inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
  cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=2020,y=100);time.sleep(.6)
 def open_bowl(v):
  cmd(v,0,x=2020,y=100);time.sleep(.6);tap(v,'Prepare elephant snack');wait(lambda:inspect(v)['elephantSnackOpen'],'preparation panel',15)
 def add(v,k):
  before=len(food(v)['pieces']);tap(v,['Add snack leaves','Add snack hay'][k]);wait(lambda:len(food(v)['pieces'])==before+1,'piece added',5)
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 def finished(count):wait(lambda:z()['animals'][0]['fed']==count and all(f['species']=='' for f in z()['food']),'feeding completes',160)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots[:2 if args.review else 4]];a,b=clients[:2]
  for v in clients:home.ready(v);v.input('resize',x=1280,y=591);enter(v)
  if args.review:
   cmd(b,1,value='orange-pup');open_bowl(a);add(a,0);add(a,1);tap(a,'Remove snack piece 0');add(a,0);home.capture(a,out,'live-changed-bowl');tap(a,'Carry elephant snack')
   cmd(b,0,x=1560,y=100);time.sleep(.6);tap(b,'Take leaves for elephant');wait(lambda:food(a)['offered'] and food(b)['offered'],'mixed review offers',30)
   home.capture(a,out,'live-mixed-feeding');finished(2);cmd(a,0,x=1700,y=100);time.sleep(.8);home.capture(a,out,'live-owner-ready')
   write(run.path/'live-review-ready.json',dict(build=args.build,ready=True,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],recordedVideo=False,liveFamilyTouched=False))
   print('READY '+str(run.path),flush=True)
   while any(v.process.poll() is None for v in clients):time.sleep(.5)
   passed=True;return
  c,d=clients[2:];base=0 if args.lifecycle_only or args.priority_only else 4
  if not args.lifecycle_only and not args.priority_only:
   open_bowl(a);ticket=z()['nextTicket'];tap(a,'Carry elephant snack');require(food(a)['preparing'] and z()['nextTicket']==ticket,'empty bowl leased food')
   add(a,0);add(a,1);add(a,0);tap(a,'Add snack hay');require(food(a)['pieces']==[0,1,0],'maximum exceeded')
   tap(a,'Remove snack piece 1');require(food(a)['pieces']==[0,0],'wrong piece removed');tap(a,'Clear snack bowl');require(food(a)['pieces']==[],'clear failed');add(a,1)
   stale=str(food(a)['prepEpoch'])+'/'+str(food(a)['edit']-1)+'/0';r=home.command(a,22,value='snack-add',target='elephant',item=stale);require(not r['accepted'] and food(a)['pieces']==[1],'duplicate edit changed bowl')
   before=z()['waterSequence'];tap(a,'Splash button');require(z()['waterSequence']==before,'panel passed pump input');seq=z()['surpriseSequence'][:];tap(a,'Butterfly flowers');require(z()['surpriseSequence']==seq,'panel passed surprise input');require(not any(c['name']=='Zoo map' for c in inspect(a)['controls']),'navigation remains reachable under panel')
   home.capture(a,out,'preparation-phone');record('Actual solo-in-shared selection/removal/clear, empty and maximum cues, panel shields habitat and navigation')
   for v,pieces in [(b,[0]),(c,[0,1]),(d,[1,0,1])]:
    open_bowl(v)
    for k in pieces:add(v,k)
   require([food(v)['pieces'] for v in clients]==[[1],[0],[0,1],[1,0,1]],'players overwrite bowls');require(z()['nextTicket']==ticket,'opening reserves ticket')
   for v in clients:require(all(next(f for f in inspect(v)['zoo']['food'] if f['actor']==w.profile)['pieces']==food(w)['pieces'] for w in clients),'snapshot bowl mismatch')
   record('Four actual clients prepare distinct bowls, deep ownership and no early tickets')
   for w,h,label in [(1024,768,'tablet'),(640,400,'small-phone')]:
    a.input('resize',x=w,y=h);s=home.capture(a,out,'preparation-'+label)
    for name in ['Add snack leaves','Add snack hay','Remove snack piece 0','Clear snack bowl','Carry elephant snack','Close snack preparation']:
     control=next(c for c in s['controls'] if c['name']==name);require(control['bounds']['width']>=44 and control['bounds']['height']>=44,'small target '+name)
   a.input('resize',x=1280,y=591);record('Native phone/tablet/small-phone simulated layouts retain large panel targets')
   # Cancel two bowls explicitly, then use ordinary bucket UI in the same queue.
   for v in [b,d]:tap(v,'Close snack preparation');cmd(v,0,x=1560,y=100);time.sleep(.5)
   tap(d,'Zoo map');tap(d,'Prepare elephant snack');require(not inspect(d)['elephantSnackOpen'] and not food(d)['preparing'],'map passed station input');tap(d,'Close Zoo map');open_bowl(d);d.input('escape');home.ready(d);require(not inspect(d)['elephantSnackOpen'] and not food(d)['preparing'],'Escape did not cancel');cmd(d,0,x=1560,y=100);time.sleep(.5)
   tap(b,'Take leaves for elephant');wait(lambda:food(b)['ticket']>0,'first bucket lease',10)
   token=str(food(a)['prepEpoch'])+'/'+str(food(a)['edit'])+'/0';tap(a,'Carry elephant snack');wait(lambda:food(a)['ticket']>0,'first snack lease',10)
   tap(d,'Take leaves for elephant');wait(lambda:food(d)['ticket']>0,'second bucket lease',10);tap(c,'Carry elephant snack');wait(lambda:food(c)['ticket']>0,'second snack lease',10)
   require(food(b)['ticket']<food(a)['ticket']<food(d)['ticket']<food(c)['ticket'],'mixed ticket order changed')
   for op in ['snack-serve','snack-add','snack-begin']:
    r=home.command(a,22,value=op,target='elephant',item=token);require(not r['accepted'],'committed edit/submission accepted')
   count=z()['nextTicket'];tap(a,'Prepare elephant snack');require(z()['nextTicket']==count and not inspect(a)['elephantSnackOpen'],'active offer duplicated by opening')
   wait(lambda:all(food(v)['offered'] for v in clients),'four offered portions',30);home.capture(a,out,'four-mixed-offers');require(all(next(f for f in inspect(v)['zoo']['food'] if f['actor']==a.profile)['pieces']==[1] for v in clients),'shared offered snack appearance state differs')
   eaters=[];until=time.monotonic()+160
   while z()['animals'][0]['fed']<4:
    require(time.monotonic()<until,'queue stalled')
    animal=z()['animals'][0]
    if animal['owner'] and animal['phase']==6 and (not eaters or eaters[-1]!=animal['owner']):eaters.append(animal['owner']);home.capture(a,out,'eat-'+str(len(eaters)))
    time.sleep(.15)
   finished(4);require(all(inspect(v)['elephantFinishEvents']==4 for v in clients),'duplicate/missing feeding finish');require(eaters[:4]==[v.profile for v in [b,a,d,c]],'mixed consumption order '+str(eaters));require(all(food(v)['pieces']==[] for v in clients),'serving not cleared');record('Mixed bucket/snack queue order, committed guards, visible handoff, four exact consumptions and cleanup')
  if not args.priority_only:
   open_bowl(a);add(a,0);open_bowl(b);add(b,1);cmd(a,7,value='creek');require(not food(a)['preparing'] and food(b)['preparing'],'departure cleared sibling')
   b.input('network-pause');time.sleep(.3);b.input('network-resume');home.ready(b);require(food(b)['preparing'],'brief reconnect lost valid bowl');tap(b,'Close snack preparation') if inspect(b)['elephantSnackOpen'] else None
   try:b.close()
   except Exception:
    write(out/'close-failure.json',dict(returncode=b.process.returncode,status=b.status(),log=str(b.out/'player.log')));raise
   b=run.start('client',run.slots[1]['profile']);clients[1]=b;home.ready(b);require(not food(b)['preparing'],'actual departure retained preparation');enter(b)
   d.close();d=run.start('client',run.slots[3]['profile']);clients[3]=d;home.ready(d);require(all(f['species']=='' for f in inspect(d)['zoo']['food']),'late join stale offers');enter(d)
   record('Independent exhibit departure, brief transport pause/rejoin, actual disconnect and late join cleanup')
  enter(a);cmd(a,0,x=1200,y=100);time.sleep(.5);tap(a,'Choose elephant brush');wait(lambda:z()['animals'][0]['phase']==13 and inspect(a)['zoo']['animals'][0]['phase']==13 and not inspect(a)['pending'],'care session and client readiness',40);time.sleep(.25)
  animal=z()['animals'][0];x=animal['fromX']-70;y=animal['fromY']+202/.45;a.input('touch-begin',x=x,y=y,finger=61);a.input('touch-end',x=x,y=y,finger=61);wait(lambda:z()['careProgress'][0]>0,'care progress',5);progress=z()['careProgress'][:]
  open_bowl(b);add(b,1);tap(b,'Carry elephant snack');wait(lambda:z()['animals'][0]['fed']==base+1,'prepared food wins over care',80);wait(lambda:z()['animals'][0]['phase']==13,'care resumes',35);require(z()['careProgress']==progress,'care progress lost');tap(a,'Put elephant brush away')
  cmd(a,0,x=1000,y=100);time.sleep(.5);tap(a,'Splash button');wait(lambda:z()['animals'][0]['phase']==10,'water session',40)
  open_bowl(b);add(b,0);tap(b,'Carry elephant snack');finished(base+2);record('Prepared snacks preempt care/water using existing scheduler; shared care progress resumes')
  # A fresh view avoids stale manually panned camera in this unchanged exhibit.
  cmd(c,0,x=4000,y=100);time.sleep(.6);tap(c,'Take leaves for giraffe');wait(lambda:z()['animals'][1]['fed']==1,'unchanged giraffe',80);home.capture(c,out,'giraffe-regression');record('Unchanged giraffe feeding regression on four actual clients')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:
   errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,nativeClients=len(clients),physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
