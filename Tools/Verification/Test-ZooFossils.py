"""Milestone12: actual four-player authority/UI/save/reconnect checks; no video or deployment."""
import sys,time,json,argparse,importlib.util
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--review',action='store_true');args=p.parse_args()
 run=Run(args.build,interactive=args.review,review_controls=args.review,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/'fossils';out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def inspect(v):return v.input('inspect')
 def f():return server.state()['view']['zoo']['fossils']
 def rec(s):checks.append(s);print('PASS '+s,flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome','command rejected'));home.ready(v);return r
 def play(v,op,piece=-1,item=None):
  state=f();n=state['members'].index(v.profile) if v.profile in state['members'] else -1
  token=item or str(state['round'])+'/'+str(state['epochs'][n] if n>=0 else 0)+'/'+str(time.monotonic_ns())
  return home.command(v,22,value='fossil-'+op,target=str(piece),item=token)
 def tap(v,name):
  wait(lambda:not (st:=inspect(v))['pending'] and any(c['name']==name for c in st['controls']),'visible tap readiness '+name,20)
  touched=v.input('touchButton',text=name)
  if 'fossil' in name.lower():require(touched['lastTouchTargets'][0]==name,'fossil target intercepted: '+str(touched['lastTouchTargets']))
  time.sleep(.22);home.ready(v)
 def enter(v):
  if inspect(v)['zone']!='zoo-dinosaurs':
   if inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
   cmd(v,0,x=1750,y=100);cmd(v,22,value='gate',target='zoo-dinosaurs')
  cmd(v,0,x=470,y=100);time.sleep(.7);tap(v,'Join fossil discovery');wait(lambda:v.profile in f()['members'],'join',8);time.sleep(1.3)
 def brush(v,piece):tap(v,'Uncover or pick up fossil '+str(piece))
 def pickup(v,piece):
  tap(v,'Uncover or pick up fossil '+str(piece));wait(lambda:f()['holders'][piece]==v.profile,'pickup',6)
 def place(v,piece):
  tap(v,'Match fossil picture '+str(piece));wait(lambda:f()['placed'][piece],'placement',6)
 def replay(v):
  wait(lambda:f()['celebrationAge']>=2.2,'settled display',6);before=f()['round']
  for width,height,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
   v.input('resize',x=width,y=height);time.sleep(.5)
   initial=next(c['bounds'] for c in inspect(v)['controls'] if c['name']=='Play fossil discovery again')
   tap(v,'Play fossil discovery again')
   choices={c['name']:c['bounds'] for c in inspect(v)['controls']}
   check=choices['Confirm fossil replay'];cancel=choices['Keep dinosaur picture']
   require(initial['y']+initial['height']<=check['y'] or check['y']+check['height']<=initial['y'],'replay confirmation overlaps at '+label)
   for bounds in [check,cancel]:require(bounds['x']>=0 and bounds['y']>=0 and bounds['x']+bounds['width']<=width and bounds['y']+bounds['height']<=height,'replay choice cropped at '+label)
   home.capture(v,out,'replay-confirmation-'+label);tap(v,'Keep dinosaur picture');require(f()['round']==before,'layout review changed round')
  v.input('resize',x=1280,y=591);time.sleep(.5)
  original=next(c['bounds'] for c in inspect(v)['controls'] if c['name']=='Play fossil discovery again')
  tap(v,'Play fossil discovery again');require(f()['round']==before,'one tap erased display')
  confirm=next(c['bounds'] for c in inspect(v)['controls'] if c['name']=='Confirm fossil replay')
  require(original['x']+original['width']<=confirm['x'] or confirm['x']+confirm['width']<=original['x'] or original['y']+original['height']<=confirm['y'] or confirm['y']+confirm['height']<=original['y'],'replay and confirmation hit areas overlap')
  # A second tap in the upper part of Replay reproduced accidental erasure
  # in516. Exercise the real input path, then explicitly choose confirmation.
  v.input('touch-begin',role='screen',x=original['x']+original['width']/2,y=original['y']+original['height']*.8,finger=81)
  v.input('touch-end',role='screen',x=original['x']+original['width']/2,y=original['y']+original['height']*.8,finger=81)
  require(f()['round']==before,'repeated replay-area tap erased display')
  time.sleep(5.3)
  require(any(c['name']=='Confirm fossil replay' for c in inspect(v)['controls']) and f()['round']==before,'slow pictured choice expired or erased display')
  tap(v,'Keep dinosaur picture');require(f()['round']==before,'cancel erased display');tap(v,'Play fossil discovery again');tap(v,'Confirm fossil replay');wait(lambda:f()['round']==before+1,'new round',6)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots[:2 if args.review else 4]];a,b=clients[:2]
  for v in clients:home.ready(v);v.input('resize',x=1280,y=591);enter(v)
  cmd(b,1,value='orange-pup');time.sleep(.5)
  positions=[next(p for p in server.state()['view']['players'] if p['id']==v.profile)['x'] for v in clients];require(len(set(round(x) for x in positions))==len(clients),'stacked approach spots');rec('Distinct actual walking positions; sibling participation does not move existing children')
  # First round solo reveal/pickup/wrong-match/place and protected replay.
  for i in range(3):
   for _ in range(6):brush(a,i)
   require(f()['revealed'][i]==6,'solo uncover');pickup(a,i)
   if i==0:
    tap(a,'Match fossil picture 1');require(f()['holders'][0]==a.profile and not f()['placed'][1],'invalid placement lost piece');home.capture(a,out,'held-piece-and-picture-hint')
   place(a,i)
  wait(lambda:inspect(a)['fossilCelebrations']==1,'once-only completion',6);home.capture(a,out,'solo-completed');replay(a);rec('Solo real tap reveal/pickup/forgiving wrong placement/completion; pictured replay requires separate confirmation')
  # Same shared progress, no personal rounds. All actual players contribute.
  for i in range(3):
   for n in range(6):brush(clients[n%len(clients)],i)
  home.capture(a,out,'cooperative-uncovered')
  with ThreadPoolExecutor(2) as pool:list(pool.map(lambda v:tap(v,'Uncover or pick up fossil 0'),[a,b]))
  require(f()['holders'][0] in (a.profile,b.profile),'claim missing');winner=next(v for v in [a,b] if v.profile==f()['holders'][0]);loser=next(v for v in [a,b] if v!=winner)
  require(not play(loser,'pickup',0)['accepted'],'second acquire');rec('Cooperative uncover on one authority; competing UI pickup and repeated claims yield one holder')
  before=f();require(not play(winner,'place',1)['accepted'] and f()['holders']==before['holders'],'wrong request changed piece');require(not play(a,'replay')['accepted'],'midround reset');require(not play(a,'brush',99)['accepted'],'bad patch');require(not play(a,'pickup',-1)['accepted'],'bad pickup')
  for _ in range(5):play(loser,'pickup',0)
  require(f()['holders'][0]==winner.profile,'rapid claims steal piece');rec('Rapid duplicate/invalid inputs and midround replay retain all progress')
  tap(winner,'Leave fossil discovery');wait(lambda:f()['holders'][0]=='','safe leave',6);require(f()['revealed']==[6,6,6],'leave erased discoveries');enter(winner);pickup(winner,0);winner.close();wait(lambda:f()['holders'][0]=='','disconnect releases lock',10)
  winner=run.start('client',winner.profile);clients=[winner if v.profile==winner.profile else v for v in clients];a,b=clients[:2];home.ready(winner);enter(winner);require(f()['revealed']==[6,6,6],'reconnect erased progress');rec('Actual leave and client disconnect return held piece safely; reconnect sees unfinished discoveries')
  # Save with a held piece and partial placements, reopen processes.
  pickup(a,0);place(a,0);pickup(b,1);before=f();profiles=[v.profile for v in clients];[v.close() for v in clients];server.close();server=run.start('server');clients=[run.start('client',id) for id in profiles];a,b=clients[:2]
  require(f()['revealed']==before['revealed'] and f()['placed']==before['placed'] and f()['holders']==['','',''],'partial checkpoint lost progress or retained lock')
  for v in clients:home.ready(v);enter(v)
  rec('Actual authority save/process reopening preserves partial discovery/placements and normalizes all transient locks')
  # Exit by walking, trail and world navigation while holding.
  pickup(a,1);cmd(a,0,x=1200,y=100);require(f()['holders'][1]=='','walkaway lock');enter(a);pickup(a,1);cmd(a,7,value='creek');require(f()['holders'][1]=='' and f()['placed'][0],'navigation lock');enter(a);rec('Walking out and world/trail departure release only own piece and retain earlier placement')
  pickup(a,1);place(a,1);pickup(b,2);place(b,2);wait(lambda:f()['celebrationAge']>=2.2,'completion settled',6)
  complete=f();profiles=[v.profile for v in clients];[v.close() for v in clients];server.close();server=run.start('server');clients=[run.start('client',id) for id in profiles];a,b=clients[:2]
  for v in clients:home.ready(v);enter(v)
  require(f()['placed']==[True]*3 and f()['round']==complete['round'],'completed display missing');require(all(inspect(v)['fossilCelebrations']==0 for v in clients),'old reaction replayed');rec('Actual completed save reopening/late join retains assembled display without replaying celebration')
  # Two reset packets against one round, plus delayed round and participant token.
  old=f();items={v.profile:str(old['round'])+'/'+str(old['epochs'][old['members'].index(v.profile)])+'/99999' for v in [a,b]}
  with ThreadPoolExecutor(2) as pool:replies=list(pool.map(lambda v:play(v,'replay',item=items[v.profile]),[a,b]))
  require(sum(r['accepted'] for r in replies)==1 and f()['round']==old['round']+1,'duplicate resets');require(not play(a,'brush',0,items[a.profile])['accepted'] and f()['revealed']==[0]*3,'stale round accepted');rec('Simultaneous replay requests reset once; late previous-round packets cannot change new progress')
  # Rapid brushing deduplicates serials and bounds individual rate, but accepts help.
  state=f();token=str(state['round'])+'/'+str(state['epochs'][state['members'].index(a.profile)])+'/100000';r=play(a,'brush',0,token);require(r['accepted'],'first brush');time.sleep(.2);require(not play(a,'brush',0,token)['accepted'] and f()['revealed'][0]==1,'duplicate brush serial')
  oldtoken=token;tap(a,'Leave fossil discovery');enter(a);require(not play(a,'brush',0,oldtoken)['accepted'],'old visit accepted');rec('Repeated brush serial and earlier-visit token reject without reversing progress')
  # Native layouts/input focus with all siblings still at the corner.
  for w,h,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
   a.input('resize',x=w,y=h);time.sleep(.7);home.capture(a,out,'partial-'+label)
   st=inspect(a);names={c['name']:c['bounds'] for c in st['controls']};require(all('Uncover or pick up fossil '+str(i) in names and 'Match fossil picture '+str(i) in names for i in range(3)),'layout loses target')
  a.input('resize',x=1280,y=591);time.sleep(.5)
  for name,close in [('Zoo map','Close Zoo map'),('Zoo toy camera','Close Zoo camera'),('Zoo photo album','Return to Zoo')]:
   tap(a,name);before=f()['revealed'];a.input('touchButton',text='Uncover or pick up fossil 0');time.sleep(.3);require(f()['revealed']==before,'modal leaked brush');tap(a,close)
  a.input('application-pause');before=f()['revealed'];a.input('touchButton',text='Uncover or pick up fossil 0');require(f()['revealed']==before,'paused input leaked');a.input('application-resume');home.ready(a);enter(a) if a.profile not in f()['members'] else None
  a.input('application-focus-loss');a.input('application-focus-gain');home.ready(a);rec('Native simulated phone/tablet/small-phone layouts, map/camera/album shielding and pause/focus lifecycle')
  # Animal feeding continues while other helpers remain. Food is not discarded.
  fed=server.state()['view']['zoo']['animals'][4]['fed'];cmd(a,0,x=1560,y=100);cmd(a,22,value='take',target='brachiosaurus');food=next(v for v in server.state()['view']['zoo']['food'] if v['actor']==a.profile);ticket=food['ticket'];cmd(a,0,x=470,y=100);require(not play(a,'join')['accepted'] and next(v for v in server.state()['view']['zoo']['food'] if v['actor']==a.profile)['ticket']==ticket,'join discarded food')
  cmd(a,0,x=1200+(food['slot']-1.5)*100,y=100);cmd(a,22,value='offer',target='brachiosaurus');wait(lambda:server.state()['view']['zoo']['animals'][4]['fed']==fed+1,'normal dinosaur feeding',50);require(b.profile in f()['members'],'feeding moved sibling');rec('Actual Brachiosaurus feeding finishes with occupied fossil activity; food transition refusal preserves ticket')
  passed=True
 finally:
  run.close();errors=[];apis=[]
  for v in run.instances:
   log=v.out/'player.log'
   if log.exists():
    lines=log.read_text(errors='replace').splitlines();errors.extend(l for l in lines if 'Exception:' in l or 'error CS' in l)
    if v.role=='client':require(any('Direct3D 11' in l for l in lines),'actual API missing');apis.append(dict(instance=v.identity,api='Direct3D 11',exit=v.process.returncode))
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,clients=apis,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
