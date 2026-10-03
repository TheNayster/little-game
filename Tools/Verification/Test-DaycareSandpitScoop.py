"""Bounded M2 check: real mouse/touch input, one release authority/four clients."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'sandpit-scoop';out.mkdir();checks=[];clients=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['sandpit']
 def info(v):return v.input('inspect')
 def counts():return [m['scoops'] for m in state()['moulds']]
 def member(v):return next(m for m in state()['members'] if m['actor']==v.profile)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def button(v,name,mouse=False):
  wait(lambda:any(c['name']==name and c['enabled'] for c in info(v)['controls']),'enabled '+name,15)
  return v.input('button' if mouse else 'touchButton',text=name)
 def select(v,i):
  button(v,'Choose sand mould '+str(i+1));s=info(v)
  require(s['sandpitSelection']==i and s['sandpitHighlight']==i,'wrong local selection/highlight')
 def position(v,i):cmd(v,0,x=4130+i*160,y=350)
 def scoop(v,i,n,mouse=False,capture=None):
  before=info(v)['sandpitScoopEvents'][i];button(v,'Scoop with shovel',mouse)
  wait(lambda:counts()[i]==n,'one direct scoop '+str(i),15)
  if capture:home.capture(v,out,capture)
  wait(lambda:info(v)['sandpitScoopEvents'][i]>before,'accepted effect '+str(i))
 def converge():
  want=counts();wait(lambda:all([m['scoops'] for m in v.state()['view']['sandpit']['moulds']]==want for v in clients),'all clients converge')
 def concurrent(vs):
  with ThreadPoolExecutor(max_workers=len(vs)) as pool:list(pool.map(lambda v:button(v,'Scoop with shovel'),vs))
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:
   home.ready(v);cmd(v,7,value='park' if v==d else 'daycare')
   v.input('resize',x=1024 if v==c else 1280,y=768 if v==c else 591);home.ready(v)
  button(a,'Ask Calypso');button(a,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'common start')
  for i in range(4):select(a,i)
  select(a,0);select(b,1);require(info(a)['sandpitHighlight']==0,'sibling overwrote selection')
  require(counts()==[0,0,0,0],'selection mutated shared progress')
  home.capture(a,out,'shovel-and-selected-mould-phone');home.capture(c,out,'shovel-and-selected-mould-tablet')
  for v in (a,c):
   target=next(x for x in info(v)['controls'] if x['name']=='Scoop with shovel')['bounds']
   require(target['width']>=85 and target['height']>=75,'small direct touch target')
  record('four visible mould selections, independent immediate highlights and generous phone/tablet targets')
  # Actual canceled and dragged touch must never activate NavigationTap.
  for end in ('touch-cancel','touch-end'):
   a.input('touch-begin',role='ui:Scoop with shovel',finger=7,x=0,y=0)
   if end=='touch-end':a.input('touch-move',role='ui:Scoop with shovel',finger=7,x=45,y=0)
   a.input(end,role='ui:Scoop with shovel',finger=7,x=45 if end=='touch-end' else 0,y=0)
  time.sleep(.3);require(counts()==[0,0,0,0] and info(a)['sandpitApproach']==-1,'canceled/swiped touch scooped')
  cmd(a,0,x=3900,y=150);button(a,'Scoop with shovel');button(a,'Scoop with shovel');require(info(a)['sandpitApproach']==0,'approach not pending')
  select(a,1);time.sleep(1);require(counts()==[0,0,0,0] and info(a)['sandpitApproach']==-1 and info(a)['sandpitApproachRound']==-1,'switch retained old scoop')
  cmd(a,0,x=3900,y=150);button(a,'Scoop with shovel');button(a,'Scoop with shovel');button(a,'Leave');wait(lambda:not member(a)['attending'],'leave pending')
  time.sleep(1);require(counts()==[0,0,0,0] and info(a)['sandpitApproach']==-1,'leave submitted pending scoop')
  require(info(b)['sandpitHighlight']==1 and sum(m['attending'] for m in state()['members'])==2,'leave affected sibling')
  cmd(a,7,value='park');cmd(a,7,value='daycare');wait(lambda:member(a)['attending'] and any(m['actor']==a.profile and m['attending'] for m in info(a)['sandpit']['members']),'return joins');time.sleep(.8)
  select(a,0);scoop(a,0,1,mouse=True,capture='accepted-sand-transfer-phone')
  cmd(d,7,value='daycare');wait(lambda:member(d)['attending'] and info(d)['sandpit']['moulds'][0]['scoops']==1,'late fourth sees fill')
  require(info(d)['sandpitScoopEvents']==[0,0,0,0],'late join animated historical fill')
  record('mouse scoop/automatic approach, canceled/swiped touch, pending selection switch/Leave and late arrival')
  select(b,1);cmd(b,0,x=3900,y=150);button(b,'Scoop with shovel');button(b,'Scoop with shovel')
  wait(lambda:counts()[1]==2 and info(b)['sandpitApproach']==-1,'two taps retained during approach');time.sleep(.4)
  require(counts()[1]==2,'burst double-fired');scoop(b,1,3);button(b,'Tip bucket');wait(lambda:counts()[1]==0,'clear burst fixture')
  record('two rapid taps while walking produce exactly two serialized scoops')
  # Empty the first mould using the unchanged dry-tip fallback before four-way work.
  scoop(a,0,2);button(a,'Tip bucket');wait(lambda:counts()[0]==0,'existing dry-tip reset')
  for i,v in enumerate(clients):select(v,i);position(v,i)
  concurrent(clients);wait(lambda:counts()==[1,1,1,1],'four different mould scoops');converge()
  require([info(v)['sandpitHighlight'] for v in clients]==[0,1,2,3],'four selections not local')
  for i,v in enumerate(clients):
   for n in range(2,(3 if i%2 else 2)+1):scoop(v,i,n)
   before=info(v)['sandpitScoopEvents'];button(v,'Scoop with shovel');time.sleep(.7)
   require(counts()[i]==(3 if i%2 else 2) and info(v)['sandpitScoopEvents']==before,'full rejection overflow/false effect')
  record('four simultaneous independent moulds, repeated taps, exact small/big capacity and full rejection without effects')
  # Competing children on a cleared big mould, then over-subscribing its last scoop.
  button(b,'Tip bucket');wait(lambda:counts()[1]==0,'clear big mould')
  select(a,1);position(a,1);position(b,1);concurrent([a,b]);wait(lambda:counts()[1]==2,'two serialized same-mould scoops')
  concurrent([a,b]);wait(lambda:counts()[1]==3,'last space conflict');time.sleep(.7);converge()
  require(counts()[1]==3,'same-mould overflow');record('same-mould concurrent actions serialize; contested last scoop stays within capacity and clients converge')
  # Native departure/re-admission, preserving construction and local sibling selection.
  saved=counts();profile=c.profile;c.close();wait(lambda:not member(c)['attending'],'native disconnect')
  require(counts()==saved and member(a)['attending'] and info(a)['sandpitHighlight']==1,'disconnect damaged sibling')
  c=run.start('client',profile);clients[2]=c;home.ready(c);wait(lambda:member(c)['attending'],'native reconnect');converge()
  require(info(c)['sandpitScoopEvents']==[0,0,0,0],'reconnect animates historical scoops')
  require(counts()==saved,'reconnect replaced authoritative fill');record('native disconnect/reconnect retains authoritative castle and independent siblings')
  for i,v in enumerate(clients):
   select(v,i);position(v,i);button(v,'Add water');wait(lambda:state()['moulds'][i]['wet'],'water fallback')
   button(v,'Tip bucket');wait(lambda:state()['moulds'][i]['built'],'tip fallback')
  wait(lambda:state()['phase']==3,'shared completion',45)
  before=info(a)['sandpitScoopEvents'];button(a,'Scoop with shovel');time.sleep(.8)
  require(info(a)['sandpitScoopEvents']==before and all(m['built'] for m in state()['moulds']),'built rejection false effect')
  # A sibling resets while an old direct scoop is still walking towards its mould.
  select(a,3);cmd(a,0,x=3900,y=150);button(a,'Scoop with shovel');require(info(a)['sandpitApproach']==3,'reset fixture has no pending approach')
  button(b,'New lesson');wait(lambda:state()['round']==2,'new lesson');time.sleep(1)
  require(counts()==[0,0,0,0] and info(a)['sandpitApproach']==-1 and info(a)['sandpitApproachRound']==-1,'stale scoop crossed reset')
  home.capture(a,out,'new-lesson-cleared-phone');record('unchanged water/tip completion, built rejection and sibling reset cancels pending lesson-bound scoop')
  passed=True
 finally:
  if not passed:
   for i,v in enumerate(clients):
    try:home.capture(v,out,'failure-'+str(i+1))
    except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,exitCodes=[v.process.returncode for v in run.instances],scope='isolated native Windows release server/four clients; real mouse and InputSystem touches; phone/tablet captures; no physical device/live-server deployment'))
 print('PASS ALL '+str(len(checks))+' groups',flush=True)

if __name__=='__main__':main()
