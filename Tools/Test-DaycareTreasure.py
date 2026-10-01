"""One focused four-player original treasure hunt through real release UI touches."""
import argparse,importlib.util,time,re,json,math
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args();run=Run(args.build,extended_test_lifetime=True);runs=[run];out=run.path/'treasure';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['treasure']
 def member(v):return next(m for m in state()['members'] if m['actor']==v.profile)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def button(v,name):wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,20);v.input('touchButton',text=name)
 def phase(n):wait(lambda:state()['phase']==n,'shared phase '+str(n),25)
 def action(v,i,predicate,why):button(v,'Treasure action '+str(i+1));wait(predicate,why,25)
 def capture(v,name):
  wait(lambda:v.input('inspect')['treasure']['phase']==state()['phase'],'client phase settles for screenshot');home.capture(v,out,name)
 def players_ready():
  for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1024 if i==2 else 1280,y=768 if i==2 else 591);home.ready(v)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients;players_ready()
  for v in clients:cmd(v,1,value='blue-pup');cmd(v,7,value='park' if v==d else 'daycare')
  wait(lambda:a.input('inspect')['worldMusicTrack']=='daycare','Daycare score');button(a,'Games');capture(a,'daycare-hunt-menu-phone');button(a,'Treasure hunt');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'common travel/start');cast=state()['friends'];g=state();
  for v in clients[:3]:home.ready(v);require(v.input('inspect')['zone']=='daycare-treasure','Starting did not travel group')
  capture(a,'island-story-phone');button(a,'Start exploring!');phase(2)
  if not a.input('inspect')['joystickVisible']:button(a,'Tap to walk')
  button(a,'Picture / riddle');wait(lambda:a.input('inspect')['treasureMystery'],'optional riddle');button(a,'Open our treasure map');capture(a,'island-map-phone');button(a,'Follow our clue');wait(lambda:a.input('inspect')['treasureApproach']=='','picture help walks to hiding place',25);require(state()['phase']==2 and not any(state()['lifted']),'Help completed the puzzle')
  a.input('frameRate',x=60);a.input('treasureMotionStart');time.sleep(4);a.input('treasureMotionStop');trace=json.loads((a.out/'treasure-motion.json').read_text(encoding='utf-8'))['samples'];require(len(trace)>50,'Insufficient actual rendered motion samples');metrics=[]
  for i in range(3):
   still=sum(s['speeds'][i]<1 for s in trace[5:])/len(trace[5:]);switches=sum((u['speeds'][i]>1)!=(v['speeds'][i]>1) for u,v in zip(trace[5:],trace[6:]));require(still<.15 and switches<=4,'NPC walk/still flicker: '+str((i,still,switches)))
   steps=[math.hypot(v['points'][i]['x']-u['points'][i]['x'],v['points'][i]['y']-u['points'][i]['y'])/(v['time']-u['time']) for u,v in zip(trace,trace[1:]) if v['time']>u['time']];require(max(steps)<60,'NPC patrol speed spike');holds=[];hold=0;last=None
   for sample in trace:
    # These model sheets use four distinct poses duplicated into eight slots.
    drawing=sample['drawings'][i]//2
    if sample['speeds'][i]>15:
     if drawing==last:hold+=sample['dt']
     else:holds.append(hold);hold=sample['dt'];last=drawing
    else:holds.append(hold);hold=0;last=None
   holds.append(hold);require(max(holds)<1.1,'Slow NPC animation holds a pose for seconds');require(len(set(s['drawings'][i] for s in trace))>=4,'NPC gait does not advance');metrics.append(dict(npc=i,stillFraction=still,walkStillSwitches=switches,maxRenderedSpeed=max(steps),maxWalkPoseHoldSeconds=max(holds),drawingIndices=sorted(set(s['drawings'][i] for s in trace))))
  write(out/'npc-motion.json',dict(build=args.build,samples=len(trace),seconds=trace[-1]['time']-trace[0]['time'],metrics=metrics));info=a.input('inspect');wait(lambda:(s:=a.input('inspect'))['worldMusicTrack']=='treasure' and s['worldMusicPlaying'] and s['worldMusicSignal']>0,'new island score audible');record('frame-by-frame NPC motion has no packet jumps or rapid walk/still flicker; brighter Daycare and dedicated treasure music play')

  art=dict(re.findall(r'new Entry\("([^\"]+)\",\"([^\"]+)\"',Path('Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/PlayableCharacters.cs').read_text(encoding='utf-8')));require(info['treasureNpcArt']==[art[n] for n in cast] and len(set(info['treasureNpcArt']))==3,'Varied saved NPC art mismatch');record('one menu start travels the group; actual riddle/map/help controls work in joystick mode without solving; moving varied NPCs ignore all-Bluey players')
  wrong=(g['shell']+1)%3;action(a,wrong,lambda:state()['lifted'][wrong],'wrong shell opens');require(state()['phase']==2 and state()['lastFind']==1,'Wrong shell penalized progress');capture(a,'friendly-wrong-shell-phone')
  action(b,g['shell'],lambda:state()['lifted'][g['shell']],'right shell opens');action(a,g['shell'],lambda:state()['phase']==3,'another child takes visible map fragment');capture(a,'first-shared-map-piece-phone')
  cmd(d,7,value='daycare');wait(lambda:member(d)['attending'],'late fourth joins');require(state()['friends']==cast and state()['phase']==3,'Arrival reset hunt');home.ready(d);record('real lift/take pictures share the first map fragment; wrong find is gentle; late fourth travels into the saved hunt')
  action(c,g['pot'],lambda:state()['lifted'][3+g['pot']],'flower pot opens');action(d,g['pot'],lambda:state()['phase']==4,'fourth takes second map fragment');capture(c,'windchime-puzzle-tablet')
  button(a,'Treasure main action');wait(lambda:(s:=a.input('inspect'))['treasureDemoNote']>=0 and s['treasureTonePlaying'],'real windchime audio and picture demonstration',15);time.sleep(2.8);capture(a,'windchime-pictures-phone');wrongnote=(g['tune'][0]+1)%3;action(a,wrongnote,lambda:state()['tuneStep']==0 and state()['lastFind']==3,'wrong tune gently restarts')
  action(a,g['tune'][0],lambda:state()['tuneStep']==1,'first shared note');action(b,g['tune'][1],lambda:state()['tuneStep']==2,'second shared note');button(c,'Return');wait(lambda:not member(c)['attending'],'independent return');require(sum(m['attending'] for m in state()['members'])==3 and state()['tuneStep']==2,'Return erased partial song');record('four children contribute to one hunt; shared tune remembers two notes when a sibling returns independently')
  b.close();wait(lambda:not member(b)['attending'],'disconnect releases only one');b=run.start('client',b.profile);clients[1]=b;home.ready(b);require(member(b)['attending'] and state()['tuneStep']==2 and state()['friends']==cast,'Reconnect lost checkpoint');record('native disconnect/reconnect retains partial tune and cast')
  before=state();run.close();require(all(v.process.returncode==0 for v in run.instances),'Initial lab shutdown abnormal');run=Run(args.build,resume=run.run_id,extended_test_lifetime=True);runs.append(run);server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients;players_ready();require(state()['phase']==4 and state()['tuneStep']==2 and state()['friends']==cast and state()['lifted']==before['lifted'],'Saved authority reopened incorrectly');require(not member(c)['attending'],'Explicit return lost across reopening');record('actual saved server reopens the partial hunt, map discoveries, cast and independent return without reset')
  action(d,g['tune'][2],lambda:state()['phase']==5,'last note reveals full map');button(a,'Open our treasure map');capture(a,'completed-map-phone');button(a,'Back to exploring');wrongsite=(g['site']+1)%3;find_at=state()['findAt'];action(a,wrongsite,lambda:state()['findAt']>find_at and state()['lastFind']>0,'wrong dig');require(state()['phase']==5,'Wrong dig erased map');action(b,g['site'],lambda:state()['phase']==6,'dig uncovers chest');capture(b,'treasure-chest-locks-phone')
  button(b,'Treasure main action');wait(lambda:'Match the chest pictures' in b.input('inspect')['treasureFeedback'],'incorrect locks give hint');require(state()['phase']==6,'Wrong code opened chest')
  for i in range(3):
   for n in range(g['tune'][i]):action(a if i%2==0 else d,i,lambda i=i,n=n:state()['wheels'][i]==n+1,'shared chest picture')
  button(a,'Treasure main action');phase(7);time.sleep(3);capture(a,'shared-treasure-ending-phone');capture(d,'shared-treasure-ending-fourth-player');record('real map/dig/lock/open controls complete one chest; wrong places and lock guesses preserve progress')
  cmd(c,7,value='park');cmd(c,7,value='daycare');wait(lambda:member(c)['attending'],'return joins completed hunt');require(state()['phase']==7 and state()['friends']==cast,'Return reset ending');home.ready(c);capture(c,'shared-ending-tablet')
  button(a,'Treasure main action');wait(lambda:state()['round']==2,'deliberate replay');require(not set(state()['friends'])&set(cast) and not any(state()['lifted']) and sum(m['attending'] for m in state()['members'])==4,'New hunt did not reset one shared round');record('world return retains ending; deliberate New hunt rerolls distinct NPCs and starts one shared story');passed=True
 finally:
  if not passed:
   for i,v in enumerate(clients):
    if v.process.poll() is not None:continue
    try:home.capture(v,out,'failure-'+str(i+1))
    except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,exitCodes=[v.process.returncode for r in runs for v in r.instances],scope='isolated release server/four clients, actual phone/tablet picture touches, shared discoveries/tune/locks, varied NPC motion, late entry, independent Return, disconnect/reconnect, saved-server reopen and deliberate replay; no device/live-server update'))
 print('PASS ALL '+str(len(checks))+' groups',flush=True)
if __name__=='__main__':main()
