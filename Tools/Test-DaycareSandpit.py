"""One focused shared sandcastle lesson using real release UI touches."""
import argparse,importlib.util,time,re
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args();run=Run(args.build,extended_test_lifetime=True);out=run.path/'sandpit';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['sandpit']
 def member(v):return next(m for m in state()['members'] if m['actor']==v.profile)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def button(v,name):wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,20);v.input('touchButton',text=name)
 def tool(v,i,name,key,value):button(v,'Choose sand mould '+str(i+1));button(v,name);wait(lambda:state()['moulds'][i][key]==value,name+' shared '+str(i),18)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:home.ready(v);cmd(v,7,value='park' if v==d else 'daycare');v.input('resize',x=1280 if v!=c else 1024,y=591 if v!=c else 768);home.ready(v)
  button(a,'Games');home.capture(a,out,'daycare-three-games-phone');button(a,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'common start');cast=state()['friends'];home.capture(a,out,'sandpit-start-phone')
  if not a.input('inspect')['joystickVisible']:button(a,'Tap to walk')
  wait(lambda:abs(a.input('inspect')['calypsoWorldPoint']['x']-4020)<10 and not a.input('inspect')['calypsoMoving'],'Calypso reaches her teaching place',35);time.sleep(1.5);home.capture(a,out,'calypso-demonstration-phone')
  # Repeat scoop after its visible result, before the delayed native acknowledgement.
  tool(a,0,'Scoop sand','scoops',1);button(a,'Scoop sand');wait(lambda:state()['moulds'][0]['scoops']==2,'rapid second scoop');button(a,'Tip bucket');wait(lambda:state()['moulds'][0]['scoops']==0,'dry sand crumbled');require(not state()['moulds'][0]['built'],'Dry sand made a tower');record('real scoop/tip pictures work in joystick mode, including rapid next taps and dry sand crumbling')
  cmd(d,7,value='daycare');wait(lambda:member(d)['attending'],'late fourth arrival');require(state()['friends']==cast,'Late arrival rerolled friends')
  wait(lambda:state()['phase']==2,'Calypso arrival and live demonstration',45);info=a.input('inspect');require(abs(info['calypsoWorldPoint']['x']-4020)<30 and not info['calypsoMoving'],'Calypso failed to join');require(info['sandpitNpcArt']==[dict(re.findall(r'new Entry\("([^"]+)","([^"]+)"',Path('Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/PlayableCharacters.cs').read_text(encoding='utf-8')))[n] for n in cast] and len(set(info['sandpitNpcArt']))==2,'Classmates do not match saved varied cast');home.capture(c,out,'teacher-and-tools-tablet');record('Calypso comes to the real sandpit and demonstrates; a late fourth player shares the same lesson and varied classmates')
  for i,v in enumerate(clients):
   for n in range(2 if i%2==0 else 3):tool(v,i,'Scoop sand','scoops',n+1)
   tool(v,i,'Add water','wet',True);tool(v,i,'Tip bucket','built',True);tool(v,i,'Add flag' if i%2==0 else 'Add shell','decoration',1 if i%2==0 else 2)
   if i==2:
    button(c,'Leave');wait(lambda:not member(c)['attending'],'independent departure');require(sum(m['attending'] for m in state()['members'])==3 and state()['moulds'][2]['built'],'Leaving erased siblings or creation')
  wait(lambda:state()['phase']==3,'one shared finished castle');require(all(m['built'] and m['wet'] and m['decoration'] for m in state()['moulds']),'Castle decorations missing');home.capture(a,out,'shared-sandcastle-phone');home.capture(d,out,'shared-sandcastle-fourth-player');record('four players build one castle with small/big buckets, water and decorations; independent exit keeps the three siblings and all towers')
  cmd(c,7,value='park');cmd(c,7,value='daycare');wait(lambda:member(c)['attending'],'return joins completed castle');require(state()['friends']==cast and state()['phase']==3,'Return reset the castle');home.capture(c,out,'shared-castle-tablet');record('world departure and return retain the same shared castle and cast')
  button(a,'New lesson');wait(lambda:state()['round']==2,'deliberate new lesson');require(not set(state()['friends'])&set(cast) and all(not m['built'] and m['scoops']==0 for m in state()['moulds']),'New lesson did not reset tools/select new classmates');record('only a deliberate new lesson clears the castle and chooses different classmates');passed=True
 finally:
  if not passed:
   for i,v in enumerate(clients):
    try:home.capture(v,out,'failure-'+str(i+1))
    except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,exitCodes=[v.process.returncode for v in run.instances],scope='isolated release server/four clients; actual phone/tablet picture touches, rapid queued actions, teacher demonstration, late arrival, independent exit/return and new lesson; no device/live-server update'))
 print('PASS ALL '+str(len(checks))+' groups',flush=True)
if __name__=='__main__':main()
