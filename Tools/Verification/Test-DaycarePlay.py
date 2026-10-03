"""Two invitation-based Daycare games, using four native release clients and real UI touches."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--presentation',action='store_true');args=parser.parse_args()
 run=Run(args.build,extended_test_lifetime=True);runs=[run];out=run.path/'daycare-play';out.mkdir();checks=[];clients=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state(tag=False):return server.state()['view']['tagClub' if tag else 'hideClub']
 def player(v):return next(p for p in server.state()['view']['players'] if p['id']==v.profile)
 def member(v,tag=False):return next(m for m in state(tag)['members'] if m['actor']==v.profile)
 def cmd(v,action,**kw):require(home.command(v,action,**kw)['accepted'],'fixture command failed');home.ready(v)
 def tap(v,name):v.input('touchButton',text=name);home.ready(v);time.sleep(.12)
 def capture(v,name):return home.capture(v,out,name)
 def menu(v,tag=False):
  tap(v,'Games');e=v.input('inspect');control=next(c for c in e['controls'] if c['name']=='The Adventure');bounds=control['bounds'];x=bounds['x']+bounds['width']*.7;y=bounds['y']+bounds['height']*.5
  v.input('touch-begin',role='screen',x=x,y=y,finger=76)
  for dy in [25,65,120,180,250,330,420]:v.input('touch-move',role='screen',x=x,y=min(e['screenHeight']-12,y+dy),finger=76)
  v.input('touch-end',role='screen',x=x,y=min(e['screenHeight']-12,y+420),finger=76)
  time.sleep(.25);capture(v,'tag-menu' if tag else 'hide-menu')
  tap(v,'Tag with friends' if tag else 'Hide & seek with Calypso')
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1024 if i==2 else 1280,y=768 if i==2 else 591);cmd(v,7,value='daycare')
  menu(a);wait(lambda:state()['round']==1,'Hide menu actually starts game');cast=[n['avatar'] for n in state()['npcs']];spots=[n['slot'] for n in state()['npcs']]
  require(len(set(cast))==4 and len(set(spots))==4,'four distinct NPC friends and hiding choices')
  require(member(a)['attending'] and all(member(v)['invited'] and player(v)['zone']=='daycare' for v in [b,c,d]),'starter travels; siblings only invited')
  capture(b,'small-hide-invitation-phone');tap(b,'Join friends');tap(c,'Not now');tap(d,'Join friends')
  wait(lambda:member(b)['attending'] and member(d)['attending'] and member(c)['declined'],'accept and decline actual cards')
  require(player(c)['zone']=='daycare','decline moved sibling');cmd(a,1,value=cast[0])
  # Each picture button walks to a real cover before entering; no fixture hide commands.
  tap(a,'Hide here 1');wait(lambda:member(a)['slot']==0,'starter walks and hides',8)
  tap(b,'Hide here 2');wait(lambda:member(b)['slot']==1,'second human hides',8)
  capture(a,'co-hiding-friends-phone');capture(d,'npc-hiding-phone')
  record('Daycare scroll menu opens a separate map; four varied NPCs move into distinct random covers; compact invitations accept/decline without forced travel; a human can match an NPC avatar')
  # The fourth child can explicitly choose the same game after dismissing its card.
  menu(c);wait(lambda:member(c)['attending'],'declined player explicitly joins current game');require([n['avatar'] for n in state()['npcs']]==cast,'join rerolled NPCs')
  wait(lambda:state()['phase']!=1,'one shared fifteen-second countdown',22)
  samples=[]
  for _ in range(10):
   e=d.input('inspect');samples.append(dict(point=e['clubTeacherPoint'],walking=e['clubTeacherWalking'],drawing=e['clubTeacherDrawing']));time.sleep(.1)
  require(any(s['walking'] and s['drawing']>=4 for s in samples),'Calypso must walk with real walking drawings')
  capture(d,'calypso-seeks-phone');capture(c,'calypso-seeks-tablet')
  # Move one hider out during search. This withdraws only that child.
  cmd(b,0,x=640,y=90);require(member(b)['slot']==-1,'moving did not leave cover')
  tap(d,'Back to Daycare');wait(lambda:not member(d)['attending'],'independent return');require(sum(m['attending'] for m in state()['members'])==3,'return removed siblings')
  wait(lambda:state()['phase']==4,'Calypso physically searches and finds friends',40)
  require(all(n['found'] for n in state()['npcs']) and member(a)['found'],'teacher never found real participants');wait(lambda:a.input('inspect')['hideClub']['phase']==4,'finished shared view');capture(a,'hide-round-finished-phone')
  record('four humans share one round and teacher walk cycle; Calypso visits covers and reveals four NPCs and a human; moving out/independent Return protect the other players')
  if args.presentation:
   x=state()['teacherX'];cmd(c,0,x=max(80,x-160),y=90);time.sleep(.8);capture(c,'calypso-complete-pose-tablet')
   write(out/'motion.json',dict(teacher=samples));passed=True;return
  tap(a,'Come out');wait(lambda:state()['round']==2,'deliberate new hiding round');require(not set(cast)&set(n['avatar'] for n in state()['npcs']),'new round did not choose other classmates')
  savedCast=[n['avatar'] for n in state()['npcs']];savedSpots=[n['slot'] for n in state()['npcs']]
  for v in [a,b,c]:
   tap(v,'Back to Daycare');wait(lambda:player(v)['zone']=='daycare','return before choosing next game')
  menu(a,True);wait(lambda:state(True)['round']==1,'Tag menu actually starts game')
  require(not any(m['invited'] for m in state()['members']),'old Hide invitations mask the new Tag card')
  tagCast=[n['avatar'] for n in state(True)['npcs']];before=[(n['x'],n['y']) for n in state(True)['npcs']]
  for v in [b,c,d]:wait(lambda:member(v,True)['invited'],'Tag invitation');tap(v,'Join friends')
  wait(lambda:all(m['attending'] for m in state(True)['members']),'four Tag members');wait(lambda:state(True)['phase']==2,'common Tag start');time.sleep(.8)
  require(before!=[(n['x'],n['y']) for n in state(True)['npcs']],'NPC runners do not move')
  traces=[]
  for _ in range(8):
   e=a.input('inspect');traces.append(dict(points=e['clubNpcPoints'],frames=e['clubNpcFrames']));time.sleep(.1)
  require(any(traces[i]['frames']!=traces[0]['frames'] for i in range(1,len(traces))),'moving classmates have frozen walking art')
  capture(a,'tag-running-phone');capture(c,'tag-running-tablet')
  # Exercise actual steering towards a runner using the same ground touch as children.
  n=state(True)['npcs'][0];e=a.input('inspect');camera=e['cameraX'];scale=e['screenHeight']/800
  # Native ground helper accepts world coordinates via the board role.
  a.input('touch-begin',role='',x=n['x'],y=n['y'],finger=81);a.input('touch-end',role='',x=n['x'],y=n['y'],finger=81)
  # Contact fixtures shorten the wait after real steering has been exercised.
  turns=state(True)['turns'];deadline=time.monotonic()+12
  while state(True)['turns']==turns and time.monotonic()<deadline:
   t=state(True);idx=next((i for i in range(4) if 'club-npc-'+str(i)!=t['it']),0);n=t['npcs'][idx];cmd(a,0,x=n['x'],y=n['y']);time.sleep(.25)
  require(state(True)['turns']>turns,'Tag contact never swapped the star')
  tap(d,'Back to Daycare');wait(lambda:not member(d,True)['attending'],'independent Tag return');require(sum(m['attending'] for m in state(True)['members'])==3,'Tag return disturbed sibling group')
  b.close();wait(lambda:not member(b,True)['attending'],'Tag disconnect release');b=run.start('client',b.profile);clients[1]=b;home.ready(b);wait(lambda:member(b,True)['attending'],'Tag reconnect current game')
  require([n['avatar'] for n in state(True)['npcs']]==tagCast,'reconnect rerolled Tag friends')
  record('Tag supports four invited humans plus four animated NPCs, authority contact swaps the star, and independent departure/reconnect retain the common cast')
  # Stop and reopen the disposable server to verify saved cast/cover migration.
  run.close();run=Run(args.build,resume=run.run_id,extended_test_lifetime=True);runs.append(run);server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
  for v in clients:home.ready(v)
  require(state()['round']==2 and [n['avatar'] for n in state()['npcs']]==savedCast and [n['slot'] for n in state()['npcs']]==savedSpots,'saved hiding cast/choices changed')
  require([n['avatar'] for n in state(True)['npcs']]==tagCast and sum(m['attending'] for m in state(True)['members'])==3,'saved Tag membership/cast changed')
  record('cold saved-server reopening preserves both NPC casts, random hiding choices, common rounds and independent exits')
  write(out/'motion.json',dict(teacher=samples,runners=traces));passed=True
 finally:
  if not passed:
   for i,v in enumerate(clients):
    if v.process.poll() is None:
     try:capture(v,'failure-'+str(i))
     except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,passed=passed,checks=checks,exitCodes=[v.process.returncode for r in runs for v in r.instances],scope=('focused corrected teacher artwork, camera and hiding replay controls; rules/Core/network unchanged from fully checked411; ' if args.presentation else '')+'isolated release authority and four Windows native clients with actual phone/tablet UI touches; physical devices and live server unchanged'))
 print('ALL PASS',flush=True)

if __name__=='__main__':main()
