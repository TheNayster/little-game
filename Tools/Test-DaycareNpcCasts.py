"""Check random casts in both Daycare games with four disposable native clients."""
import argparse,importlib.util,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args();run=Run(args.build,extended_test_lifetime=True);out=run.path/'npc-casts';out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state(key):return server.state()['view'][key]
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r['outcome']);home.ready(v)
 def button(v,name):
  wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,15);v.input('touchButton',text=name);time.sleep(.2);home.ready(v)
 def menu(v,name):button(v,'Games');button(v,name)
 def cast(key):return state(key)['npcCast' if key=='kingdom' else 'guests']
 def verify(key,clients):
  ids=cast(key);n=9 if key=='kingdom' else 4
  faces=['terriers' if i.startswith('terrier-') else i for i in ids]
  require(len(ids)==n and len(set(faces))==n and not set(ids)&{'bandit','chilli'},'Repeated/unexpected NPC cast')
  expected=[{'blue-pup':'bluey','orange-pup':'bingo'}.get(i,i) for i in ids];art='kingdomNpcArt' if key=='kingdom' else 'picnicNpcArt'
  for v in clients:wait(lambda:v.input('inspect')[art]==expected,'same rendered '+key+' cast on '+v.profile,15)
  write(out/(key+'-cast.json'),dict(avatarIds=ids,renderedArt=expected,players=[v.profile for v in clients]));return ids
 def plate(v,mask):
  wait(lambda:v.input('inspect')['daycare']['phase']==2,'picnic placement phase',15);button(v,'Next picnic plate');wait(lambda:state('daycare')['plates']&mask,'shared plate',14)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:home.ready(v);cmd(v,7,value='daycare')
  a.input('resize',x=1280,y=591);d.input('resize',x=1024,y=768);home.ready(a);home.ready(d)
  menu(a,'Picnic counting');menu(b,'Picnic counting');menu(c,'Picnic counting');wait(lambda:state('daycare')['phase']==2,'welcome');plate(a,1);menu(d,'Picnic counting')
  picnic=verify('daycare',clients);cmd(a,1,value=picnic[0]);cmd(b,1,value=picnic[0]);require(cast('daycare')==picnic,'Matching players changed picnic cast');verify('daycare',clients);home.capture(d,out,'varied-picnic-tablet');record('four players see four distinct random guests; late joins and two matching player avatars preserve them')
  plate(b,2);button(a,'Leave picnic');b.close();wait(lambda:not next(m for m in state('daycare')['members'] if m['actor']==b.profile)['attending'],'independent disconnect');require(cast('daycare')==picnic and state('daycare')['plates']==3,'Departure changed cast/progress');plate(c,4);menu(a,'Picnic counting');plate(d,8)
  wait(lambda:state('daycare')['phase']==3,'picnic complete');button(a,'Set it again');wait(lambda:cast('daycare')!=picnic,'new picnic cast');new_picnic=verify('daycare',[a,c,d]);require(not set(picnic)&set(new_picnic),'Replay repeated old guests');home.capture(d,out,'new-picnic-cast-tablet');record('independent leave/disconnect retains cast; shared replay draws four new guests')
  for v in (a,c,d):button(v,'Leave picnic')
  b=run.start('client',run.slots[1]['profile']);home.ready(b);clients=[a,b,c,d]
  menu(a,'The Adventure');menu(b,'The Adventure');wait(lambda:state('kingdom')['phase']==2,'fruit phase');wait(lambda:a.input('inspect')['kingdom']['phase']==2,'client fruit phase');button(a,'Next adventure task');wait(lambda:state('kingdom')['supplies']==1,'one shared fruit');menu(c,'The Adventure');menu(d,'The Adventure')
  story=verify('kingdom',clients);cmd(c,1,value=story[0]);cmd(d,1,value=story[0]);require(cast('kingdom')==story,'Matching players changed Adventure cast');verify('kingdom',clients);home.capture(d,out,'varied-adventure-tablet');record('nine distinct random Adventure roles render identically on four clients despite two players matching an NPC')
  button(a,'Return to daycare');d.close();wait(lambda:not next(m for m in state('kingdom')['members'] if m['actor']==d.profile)['attending'],'story disconnect');menu(a,'The Adventure');require(cast('kingdom')==story and state('kingdom')['supplies']==1,'Story exit/rejoin changed cast/progress');verify('kingdom',[a,b,c]);record('Adventure independent exits and rejoin preserve the shared cast and progress')
  for v in (a,b,c):button(v,'Return to daycare');v.close()
  server.close();server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a);require(cast('kingdom')==story and cast('daycare')==new_picnic,'Reopen rerolled saved casts');menu(a,'The Adventure');verify('kingdom',[a]);require(state('kingdom')['supplies']==1,'Reopen lost story progress');button(a,'Return to daycare');menu(a,'Picnic counting');verify('daycare',[a]);record('native saved reopening preserves both exact NPC casts and the Adventure checkpoint');passed=True
 finally:
  write(out/'result.json',dict(build=args.build,passed=passed,checks=checks,scope='disposable Windows authority and four native clients; no device or live-server update'));run.close()
 print('PASS ALL '+str(len(checks))+' groups',flush=True)
if __name__=='__main__':main()
