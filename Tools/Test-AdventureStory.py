"""Focused story-choice play through real native controls on four clients."""
import argparse,importlib.util,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--basket-only',action='store_true');parser.add_argument('--replay-only',action='store_true');parser.add_argument('--resume');args=parser.parse_args();run=Run(args.build,resume=args.resume,extended_test_lifetime=True);out=run.path/('adventure-story-reopen' if args.replay_only else 'adventure-story');out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['kingdom']
 def member(v):return next(m for m in state()['members'] if m['actor']==v.profile)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def button(v,name):wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,20);v.input('touchButton',text=name);time.sleep(.15);home.ready(v)
 def story(v):button(v,'Games');button(v,'The Adventure')
 def talk(v,choice):button(v,'Talk to a friend');button(v,choice)
 def nexttask(v,key,mask):button(v,'Next adventure task');wait(lambda:state()[key]&mask,'guided story action '+key,18)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  if args.replay_only:
   for v in clients:home.ready(v)
   saved=state();cast=state()['npcCast'];require(saved['phase']==6 and saved['queenPlan']==2 and saved['crossing']==2,'Prior shared ending did not reopen')
   record('a separate native reopening retains the complete kind-story checkpoint and saved cast')
  else:
   for v in clients:home.ready(v);cmd(v,7,value='park' if v==d else 'daycare');v.input('resize',x=1280,y=591);home.ready(v)
   if args.basket_only:
    cmd(d,7,value='daycare');story(a);button(a,"Let's go!");a.input('resize',x=960,y=640);home.ready(a);button(a,'Gather fruit 1');wait(lambda:member(a)['carrying']=='fruit-0','basket repro fruit',18);button(a,'Put fruit in basket');write(out/'basket-input.json',a.input('inspect'));wait(lambda:state()['supplies']&1,'basket direct touch',18);record('four overlapping arrivals at 960 by 640 retain direct basket delivery');passed=True;return
   story(a);wait(lambda:sum(m['attending'] for m in state()['members'])==3,'three same-world participants');cast=state()['npcCast'];home.capture(a,out,'kingdom-opening-phone');button(a,"Let's go!");wait(lambda:state()['phase']==2,'orchard chapter')
   for v in (a,b,c):
    if not v.input('inspect')['joystickVisible']:button(v,'Tap to walk')
   # One child gathers while another asks a real NPC to pack. The helper must
   # visibly work and must never consume the fruit in the child's hands.
   button(a,'Gather fruit 1');wait(lambda:member(a)['carrying']=='fruit-0','held fruit',18)
   button(b,'Talk to a friend');wait(lambda:any(c['name']=='Help me pack' for c in b.input('inspect')['controls']),'orchard conversation',18);info=b.input('inspect');require(info['tagCue']=='talk-food' and info['tagSpeaking'],'Orchard conversation is not speaking');home.capture(b,out,'orchard-conversation-phone');button(b,'Help me pack');wait(lambda:state()['foodHelp'],'shared helper choice');wait(lambda:state()['supplies']==6,'NPC packs the two unheld fruits',20);require(member(a)['carrying']=='fruit-0','NPC stole held fruit');button(a,'Put fruit in basket');wait(lambda:state()['phase']==3,'river chapter')
   record('spoken orchard conversation starts a shared NPC packing job while preserving a sibling fruit hold')
   cmd(d,7,value='daycare');wait(lambda:member(d)['attending'],'late fourth arrival');require(state()['foodHelp'] and state()['npcCast']==cast,'Late arrival reset shared story')
   button(c,'Talk to a friend');wait(lambda:any(x['name']=='Hop on stones' for x in c.input('inspect')['controls']),'river conversation',18);home.capture(c,out,'river-choice-phone');button(c,'Hop on stones');wait(lambda:state()['crossing']==2,'stone route')
   button(b,'Return to daycare');wait(lambda:not member(b)['attending'],'independent departure');require(sum(m['attending'] for m in state()['members'])==3,'Sibling participation reset')
   for i,v in enumerate((a,c,d)):nexttask(v,'boards',1<<i)
   wait(lambda:state()['phase']==4,'queen encounter');require(state()['crossing']==2 and any(m['hopAt']>=0 for m in state()['members']),'Stepping stones have no hop behavior')
   record('one shared stone route includes a late fourth player and remains completable after independent departure')
   button(a,'Talk to a friend');wait(lambda:any(x['name']=='Invite her to feast' for x in a.input('inspect')['controls']),'queen conversation',18);home.capture(a,out,'queen-conversation-phone');button(a,'Invite her to feast');wait(lambda:state()['queenPlan']==2,'queen invitation');wait(lambda:a.input('inspect')['kingdomNpcJobs'][5]=='Join our feast','queen job changed');time.sleep(4.2);home.capture(a,out,'queen-shares-wand-phone');button(c,'Next adventure task');wait(lambda:state()['phase']==5,'shared wand',18)
   require(state()['queenPlan']==2 and state()['distractedUntil']==0,'Invitation secretly uses timed ball challenge');record('talking to the lonely queen changes her job and shares the wand without a ball timer')
   button(a,'Talk to a friend');wait(lambda:any(x['name']=='Silly spell' for x in a.input('inspect')['controls']),'rescue conversation',18);home.capture(a,out,'rescue-spell-choice-phone');button(a,'Silly spell');wait(lambda:state()['rescued']&1,'silly spell',18)
   nexttask(c,'rescued',2);nexttask(d,'rescued',4);wait(lambda:state()['phase']==6,'common ending');require(state()['rescueStyle'][0]==2,'Silly spell not retained');time.sleep(6);home.capture(c,out,'kind-kingdom-ending-phone');button(c,'Story map');home.capture(c,out,'shared-story-journal-phone');button(c,'Back to exploring')
   record('rescue conversations offer two spells and the shared ending/journal retain the chosen story')
   # Reopen the exact checkpoint, then play the other branch on a deliberate
   # replay. This is the second path of this same focused activity check.
   saved=state();
   for v in clients:v.close()
   server.close();server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
   for v in clients:home.ready(v)
  story(a);require(state()['queenPlan']==2 and state()['crossing']==2 and state()['npcCast']==cast,'Reopened story lost choices/cast');button(a,'Play again');wait(lambda:state()['round']==saved['round']+1,'deliberate replay');require(state()['crossing']==0 and state()['queenPlan']==0 and not set(cast)&set(state()['npcCast']),'Replay did not reset story/new friends')
  for v in clients:
   if not member(v)['attending']:cmd(v,7,value='daycare')
  button(a,"Let's go!");wait(lambda:state()['phase']==2,'replay orchard')
  for i,v in enumerate((a,b,c)):
   button(v,'Gather fruit '+str(i+1));wait(lambda:member(v)['carrying']=='fruit-'+str(i),'manual replay pickup',18);button(v,'Put fruit in basket');wait(lambda:state()['supplies']&(1<<i),'manual replay delivery',18)
  talk(a,'Build a bridge');wait(lambda:state()['crossing']==1,'bridge route')
  for i,v in enumerate((a,b,c)):nexttask(v,'boards',1<<i)
  wait(lambda:state()['phase']==4,'ball-route queen');talk(d,'Play ball');wait(lambda:state()['distractedUntil']>state()['clock'],'ball distraction',18);button(d,'Next adventure task');wait(lambda:state()['phase']==5,'ball-route wand',18)
  for i,v in enumerate((a,b,c)):nexttask(v,'rescued',1<<i)
  wait(lambda:state()['phase']==6,'second path ending');require(state()['queenPlan']==1 and state()['crossing']==1 and not state()['foodHelp'],'Second route did not change story');home.capture(d,out,'bridge-ball-ending-tablet');record('checkpoint reopening retains choices; replay picks new NPCs and the manual food/bridge/ball path finishes with a different story')
  passed=True
 finally:
  if not passed:
   for index,v in enumerate(locals().get('clients',[])):
    try:home.capture(v,out,'failure-client-'+str(index+1))
    except Exception:pass
  write(out/'result.json',dict(build=args.build,runId=run.run_id,passed=passed,checks=checks,scope='two story paths on one isolated native four-client activity pass; real UI conversations and props, late entry, independent departure, saved choices and reopening; no device/live-server update'));run.close()
 print('PASS ALL '+str(len(checks))+' groups',flush=True)
if __name__=='__main__':main()
