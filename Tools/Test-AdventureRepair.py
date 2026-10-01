"""Reproduce the reported controls/group flow using real native UGUI touches."""
import argparse,importlib.util,time,json
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--teacher-only',action='store_true');parser.add_argument('--join-only',action='store_true');parser.add_argument('--resume');args=parser.parse_args();run=Run(args.build,resume=args.resume,extended_test_lifetime=True);out=run.path/('teacher-repair' if args.teacher_only else 'returning-group' if args.join_only else 'adventure-repair');out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['kingdom']
 def player(v):return next(p for p in server.state()['view']['players'] if p['id']==v.profile)
 def member(v):return next(m for m in state()['members'] if m['actor']==v.profile)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def command(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def button(v,name):
  wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,18);v.input('touchButton',text=name);time.sleep(.2);home.ready(v)
 def travel(v,place):
  button(v,'Worlds');name=home.scenic.NAMES[place]
  for _ in range(12):
   info=v.input('inspect');control=next((c for c in info['controls'] if c['name']==name),None)
   if control and control['bounds']['height']>=control['bounds']['width']*.97:break
   home.scenic.swipe(v,place=='daycare')
  else:raise AssertionError('Could not reveal '+place)
  button(v,name);home.ready(v)
 def story(v):button(v,'Games');button(v,'The Adventure')
 def task(v,name,key,mask):
  # First use the pictured arrow if the actual prop is outside the viewport.
  info=v.input('inspect');control=next((c for c in info['controls'] if c['name']==name),None)
  if control is None or not 0<=control['bounds']['x']+control['bounds']['width']/2<=info['screenWidth']:
   button(v,'Next adventure task');wait(lambda:state()[key]&mask,'guided '+name,18)
  else:button(v,name);wait(lambda:state()[key]&mask,'touched '+name,18)
 def check_teacher(v):
  # Sample a complete ambient teacher travel segment; world movement must stay
  # bounded and her standing travel drawing must not alternate with sitting.
  wait(lambda:v.input('inspect')['daycare']['clock']%18<1,'teacher route departure',22)
  v.input('frameRate',x=60);v.input('treasureMotionStart');samples=[];times=[]
  for index in range(30):
   samples.append(v.input('inspect'));times.append(time.monotonic());time.sleep(.06)
   if index==3 and samples[-1]['calypsoMoving']:home.capture(v,out,'teacher-in-travel-pose')
   if index==10:
    width=samples[-1]['screenWidth'];height=samples[-1]['screenHeight'];v.input('touch-begin',role='screen',x=width*.8,y=height*.78,finger=42);v.input('touch-move',role='screen',x=width*.6,y=height*.78,finger=42);v.input('touch-end',role='screen',x=width*.4,y=height*.78,finger=42)
  v.input('treasureMotionStop');moving=[v for v in samples if v['calypsoMoving']];require(len(moving)>5,'Teacher route did not move');require(all(v['calypsoWalkPlaying'] and v['calypsoDrawing']>=4 for v in moving),'Teacher glides without walking artwork');require(len(set(v['calypsoDrawing'] for v in moving))>=4,'Teacher feet do not advance')
  for index,(before,after) in enumerate(zip(samples,samples[1:])):require(abs(after['calypsoWorldPoint']['x']-before['calypsoWorldPoint']['x'])<=180*(times[index+1]-times[index])+8,'Teacher teleported between frames')
  require(abs(samples[-1]['cameraX']-samples[0]['cameraX'])>100,'Teacher check did not include camera panning')
  wait(lambda:not v.input('inspect')['calypsoMoving'],'teacher arrives and stops',10);require(not v.input('inspect')['calypsoWalkPlaying'],'Teacher walks at rest');home.capture(v,out,'teacher-stopped-phone');write(out/'teacher-motion.json',[{k:v[k] for k in ('calypsoWorldPoint','calypsoMoving','calypsoPose','calypsoDrawing','calypsoWalkPlaying','cameraX','daycareRoutine')} for v in samples]);record('Calypso follows a bounded route with animated walking feet, including camera panning')
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  if args.teacher_only:
   for v in clients:home.ready(v);command(v,7,value='daycare');v.input('resize',x=1280,y=591);home.ready(v)
   check_teacher(a);passed=True;return
  for v in clients:home.ready(v);command(v,7,value='park' if v==d else 'daycare');v.input('resize',x=1280,y=591);home.ready(v)
  for v in (a,b,c):
   if not v.input('inspect')['joystickVisible']:button(v,'Tap to walk')
  story(a);wait(lambda:sum(m['attending'] for m in state()['members'])==3,'automatic same-world group');require(player(d)['zone']=='park','Other-world player pulled away')
  for v in (a,b,c):home.ready(v);require(player(v)['zone']=='imagination-adventure','Same-world sibling left behind')
  if args.join_only:
   cast=state()['npcCast'];round_id=state()['round'];travel(d,'daycare');wait(lambda:member(d)['attending'],'late entry');travel(d,'park');wait(lambda:player(d)['zone']=='park','independent world departure');travel(d,'daycare');wait(lambda:member(d)['attending'] and player(d)['zone']=='imagination-adventure','automatic world return');require(state()['round']==round_id and state()['npcCast']==cast,'Return reset shared cast/round');home.capture(d,out,'returned-to-shared-adventure');button(d,'Return to daycare');time.sleep(.6);require(player(d)['zone']=='daycare' and not member(d)['attending'] and sum(m['attending'] for m in state()['members'])==3,'Explicit activity exit was pulled back');record('four-player automatic entry and world departure/return retain one shared cast/round; explicit activity exit stays independent');passed=True;return
  home.capture(a,out,'story-opening-phone');button(a,"Let's go!");wait(lambda:state()['phase']==2,'shared food stage');record('one start automatically joins all three Daycare players; the Park player stays in Park')
  for i,v in enumerate((a,b,c)):
   wait(lambda:v.input('inspect')['kingdom']['phase']==2,'client food stage');button(v,'Gather fruit '+str(i+1));wait(lambda:member(v)['carrying']=='fruit-'+str(i),'joystick pickup',18)
  require(state()['supplies']==0,'Picked fruit counted before delivery');require(all(v.input('inspect')['homePose']=='Carry' for v in (a,b,c)),'Fruit is not carried visibly');home.capture(a,out,'three-fruit-carriers-phone')
  for i,v in enumerate((a,b,c)):button(v,'Put fruit in basket');wait(lambda:state()['supplies']&(1<<i),'basket delivery',18)
  record('direct fruit touches work in joystick mode; three visible exclusive holds become three basket deliveries')
  travel(d,'daycare');wait(lambda:member(d)['attending'] and player(d)['zone']=='imagination-adventure','late same-world automatic join');home.ready(d);require(state()['supplies']==7,'Late join reset food');travel(d,'park');wait(lambda:player(d)['zone']=='park','independent world departure');travel(d,'daycare');wait(lambda:member(d)['attending'] and player(d)['zone']=='imagination-adventure','automatic world return');require(state()['supplies']==7,'World return reset food');record('late travel through the real world menu automatically joins the current Adventure')
  task(a,'Place bridge plank 1','boards',1);before=a.input('inspect')['kingdomNpcPoints'][2];time.sleep(2.4);after=a.input('inspect')['kingdomNpcPoints'][2];require(after['x']>before['x']+100,'NPC builder did not carry the delivered plank to the crossing');home.capture(a,out,'builder-carrying-plank-phone')
  button(b,'Return to daycare');time.sleep(.5);require(player(b)['zone']=='daycare' and member(c)['attending'],'Explicit exit pulled back or reset sibling');task(c,'Place bridge plank 2','boards',2);task(d,'Place bridge plank 3','boards',4);wait(lambda:state()['phase']==4,'wand guard stage')
  wait(lambda:c.input('inspect')['kingdom']['phase']==4,'queen stage reaches client');button(c,'Next adventure task');button(c,'Play ball');wait(lambda:state()['distractedUntil']>state()['clock'],'queen distracted');button(d,'Next adventure task');wait(lambda:state()['phase']==5,'shared wand recovery',18)
  task(a,'Wake friend 1','rescued',1);home.capture(a,out,'rescued-friend-phone');d.close();wait(lambda:not member(d)['attending'],'independent disconnect');story(b);task(c,'Wake friend 2','rescued',2);task(b,'Wake friend 3','rescued',4);wait(lambda:state()['phase']==6,'common feast');home.capture(c,out,'group-feast-phone');record('builders carry and construct the crossing; guard follows the ball; rescues and common feast work after independent exit/disconnect')
  # A stationary scene must not oscillate between camera-generated walk/idle.
  wait(lambda:all(p=='Dance' for p in c.input('inspect')['kingdomNpcPoses']),'NPCs arrive at the feast',20);poses=[]
  for index in range(12):
   poses.append(c.input('inspect')['kingdomNpcPoses']);time.sleep(.05)
   if index==4:
    c.input('touch-begin',role='screen',x=1100,y=200,finger=43);c.input('touch-move',role='screen',x=800,y=200,finger=43);c.input('touch-end',role='screen',x=550,y=200,finger=43)
  write(out/'settled-npc-poses.json',poses);require(all(all(p=='Dance' for p in sample) for sample in poses),'Settled NPCs flicker between incompatible poses');record('all nine NPCs remain in the settled celebration pose across twelve samples')
  cast=state()['npcCast'];round_id=state()['round']
  for v in (a,b,c):button(v,'Return to daycare');v.close()
  server.close();server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a);story(a);require(state()['npcCast']==cast and state()['rescued']==7 and state()['round']==round_id,'Reopening lost checkpoint/cast');button(a,'Play again');wait(lambda:state()['round']==round_id+1,'shared replay');require(not set(cast)&set(state()['npcCast']),'Replay repeated NPC cast');record('native reopening retains the shared ending/cast; deliberate replay selects new friends')
  button(a,'Return to daycare');b,c,d=[run.start('client',slot['profile']) for slot in run.slots[1:]];clients=[a,b,c,d]
  for v in clients:
   home.ready(v);v.input('resize',x=1280,y=591);home.ready(v)
   if v.input('inspect')['zone']=='imagination-adventure':button(v,'Return to daycare')
   if not v.input('inspect')['joystickVisible']:button(v,'Tap to walk')
  button(a,'Games');button(a,'Picnic counting');wait(lambda:a.input('inspect')['daycare']['phase']==2,'picnic welcome');wait(lambda:sum(m['attending'] for m in server.state()['view']['daycare']['members'])==4,'four-player automatic picnic group');home.capture(a,out,'picnic-plate-stack-phone')
  def held(v):return next(m for m in server.state()['view']['daycare']['members'] if m['actor']==v.profile)['carryingPlate']
  for v in clients:button(v,'Take picnic plate');wait(lambda:held(v),'picnic plate pickup',18)
  home.capture(a,out,'carried-picnic-plates-phone');cast=server.state()['view']['daycare']['guests'];button(c,'Leave picnic');wait(lambda:not held(c),'independent plate release');require(held(a) and held(b) and held(d),'Leaving player cleared sibling plates');require(server.state()['view']['daycare']['guests']==cast,'Departure rerolled NPCs')
  for i,v in enumerate((a,b,d)):button(v,'Place picnic plate '+str(i+1));wait(lambda:server.state()['view']['daycare']['plates']&(1<<i),'picnic joystick placement',18)
  button(d,'Next picnic plate');wait(lambda:held(d),'guided plate pickup',18);button(d,'Next picnic plate');wait(lambda:server.state()['view']['daycare']['plates']==15,'fourth counted plate',18)
  require(not next(m for m in server.state()['view']['daycare']['members'] if m['actor']==c.profile)['attending'],'Explicit picnic exit was pulled back');home.capture(a,out,'four-counted-plates-phone');record('all four players automatically join picnic, carry visible plates and share four counted places; independent exit retains sibling plates and NPC cast')
  for v in (a,b,d):button(v,'Leave picnic')
  check_teacher(a)
  passed=True
 finally:
  if not passed:
   for index,v in enumerate(locals().get('clients',[])):
    try:home.capture(v,out,'failure-client-'+str(index+1))
    except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,passed=passed,checks=checks,exitCodes=[v.process.returncode for v in run.instances],teacherOnly=args.teacher_only,joinOnly=args.join_only,scope='disposable release server and four native clients, direct UGUI fruit/basket/world-menu controls; no live server/device installation'))
 print('PASS ALL '+str(len(checks))+' groups',flush=True)
if __name__=='__main__':main()
