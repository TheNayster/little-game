"""M5 only: native direct Flag/Shell, authoritative replacement and four players."""
import argparse, importlib.util, time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'sandpit-decoration';out.mkdir();checks=[];clients=[];passed=False;conflict=None
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['sandpit']
 def info(v):return v.input('inspect')
 def decorations():return [m['decoration'] for m in state()['moulds']]
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
 def button(v,name,mouse=False):
  wait(lambda:any(c['name']==name and c['enabled'] for c in info(v)['controls']),'enabled '+name,15)
  return v.input('button' if mouse else 'touchButton',text=name)
 def select(v,i):
  button(v,'Choose sand mould '+str(i+1));s=info(v);require(s['sandpitSelection']==i and s['sandpitHighlight']==i,'local selection/highlight')
 def work(v,i):cmd(v,0,x=4130+i*160,y=350)
 def tool(v,op,i):
  cmd(v,0,x=4210+i*160,y=440);return cmd(v,32,value=op,target=str(i)+'@'+str(state()['round']))
 def build(v,i):
  while state()['moulds'][i]['scoops']<(3 if i%2 else 2):tool(v,'scoop',i)
  tool(v,'water',i);tool(v,'tip',i)
 def source(op):return 'Decorate with '+op
 def effect(v,i,value):return wait(lambda:s if (s:=info(v))['sandpitDecorationEffects'][i]>0 and s['sandpitDecorationEffectValues'][i]==value else None,'confirmed decoration placement',8)
 def decorate(v,i,op,mouse=False,stay=False):
  select(v,i);work(v,i);old=info(v)['sandpitDecorationEvents'][i];button(v,source(op),mouse)
  want=1 if op=='flag' else 2;wait(lambda:decorations()[i]==want,'authority '+op)
  wait(lambda:info(v)['sandpitDecorationEvents'][i]==old+1,'new accepted transition');effect(v,i,want)
  if not stay:time.sleep(1.05)
 def converge():
  want=decorations();wait(lambda:all([m['decoration'] for m in v.state()['view']['sandpit']['moulds']]==want for v in clients),'four clients converge')
  require(all(info(v)['sandpitDecorationVisuals']==want for v in clients),'permanent graphics contradict authority')
 def overlap(x,y):return min(x['x']+x['width'],y['x']+y['width'])>max(x['x'],y['x']) and min(x['y']+x['height'],y['y']+y['height'])>max(x['y'],y['y'])
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:home.ready(v);cmd(v,7,value='park' if v is d else 'daycare')
  for v in (a,b,d):v.input('resize',x=1280,y=591)
  c.input('resize',x=1024,y=768)
  button(a,'Ask Calypso');button(a,'Sandcastle club');wait(lambda:state()['round']==1,'start');time.sleep(.8)
  for i,v in enumerate((a,b,c)):select(v,i)
  for v in (a,c):
   controls={q['name']:q['bounds'] for q in info(v)['controls']}
   for name in (source('flag'),source('shell')):
    target=controls[name];require(target['width']>=115 and target['height']>=95,'small decoration target')
    for other in (source('shell') if name==source('flag') else source('flag'),'Scoop with shovel','Water with watering can'):
     require(not overlap(target,controls[other]),'overlapping world tools')
  work(a,0)
  for i,op in enumerate(('flag','shell')):
   button(a,source(op));wait(lambda:info(a)['sandpitDecorationWiggles'][i]>0,'invalid source wiggle')
   require(decorations()==[0]*4 and info(a)['sandpitDecorationEvents']==[0]*4,'invalid request made fake success');time.sleep(.5)
  for op in ('flag','shell'):
   for end in ('touch-cancel','touch-end'):
    a.input('touch-begin',role='ui:'+source(op),finger=7,x=0,y=0)
    if end=='touch-end':a.input('touch-move',role='ui:'+source(op),finger=7,x=45,y=0)
    a.input(end,role='ui:'+source(op),finger=7,x=45 if end=='touch-end' else 0,y=0)
  require(info(a)['sandpitApproach']==-1 and info(a)['sandpitDecorationWiggles']==[0]*2 and decorations()==[0]*4,'swipe/cancel sent decoration')
  home.capture(a,out,'phone-world-flag-shell');home.capture(c,out,'tablet-world-flag-shell')
  record('grounded Flag/Shell, nonoverlapping generous phone/tablet targets; rejected unbuilt wiggle and canceled/swiped input')
  build(a,0);build(a,1);time.sleep(2)
  button(a,'Sand mould 2');require(info(a)['sandpitSelection']==1,'world built selection');button(a,'Sand mould 1');button(a,'Sand mould 1')
  require(info(a)['sandpitSelection']==0 and info(a)['sandpitApproach']==-1 and info(b)['sandpitSelection']==1 and info(c)['sandpitSelection']==2,'built tower tap/selection interfered with sibling')
  decorate(a,0,'flag',stay=True);home.capture(a,out,'phone-flag-moving');time.sleep(1.05);home.capture(a,out,'phone-small-tower-flag')
  old=info(a)['sandpitDecorationEvents'];button(a,source('flag'));home.ready(a);time.sleep(.4);require(info(a)['sandpitDecorationEvents']==old,'redundant Flag replayed placement')
  decorate(a,0,'shell',mouse=True);home.capture(a,out,'phone-flag-replaced-with-shell')
  decorate(a,0,'flag');home.capture(a,out,'phone-shell-replaced-with-flag')
  decorate(c,1,'shell');home.capture(c,out,'tablet-big-tower-shell')
  button(c,'Add flag');wait(lambda:decorations()[1]==1,'tray Flag');time.sleep(1.05);button(c,'Add shell');wait(lambda:decorations()[1]==2,'tray Shell');time.sleep(1.05)
  require(info(a)['sandpitDecorationHidden']==[False]*4 and info(c)['sandpitDecorationHidden']==[False]*4,'temporary effect hid permanent prop')
  record('built towers selectable locally; direct touch Flag, mouse Shell, both replacements, redundant input and tray fallbacks on small/big towers')
  select(a,0);cmd(a,0,x=3900,y=150);button(a,source('shell'));require(info(a)['sandpitApproach']==0,'pending shell missing');button(a,'Sand mould 2');require(info(a)['sandpitApproach']==-1,'world tower switch retained old intent');time.sleep(.4)
  require(decorations()[:2]==[1,2],'switch decorated A or B')
  select(a,1);cmd(a,0,x=3900,y=150);button(a,source('flag'));button(a,'Leave');wait(lambda:not next(m for m in state()['members'] if m['actor']==a.profile)['attending'],'accepted Leave');time.sleep(.2)
  require(decorations()[:2]==[1,2] and info(a)['sandpitDecorationEffects']==[0]*4 and info(b)['sandpitSelection']==1,'pending Leave reset sibling/selection/effect')
  cmd(a,7,value='park');cmd(a,7,value='daycare');wait(lambda:next(m for m in state()['members'] if m['actor']==a.profile)['attending'] and next(m for m in info(a)['sandpit']['members'] if m['actor']==a.profile)['attending'],'return');time.sleep(.8)
  require(info(a)['sandpitDecorationVisuals'][:2]==[1,2] and info(a)['sandpitDecorationEffects']==[0]*4,'re-entry lost/replayed prop')
  cmd(d,7,value='daycare');wait(lambda:next(m for m in state()['members'] if m['actor']==d.profile)['attending'],'late fourth');time.sleep(.6)
  require(info(d)['sandpitDecorationVisuals'][:2]==[1,2] and info(d)['sandpitDecorationEvents']==[0]*4,'late join lost/replayed decorations');home.capture(d,out,'late-join-final-flag-shell')
  record('pending world selection switch/Leave cancels intent; re-entry and late fourth restore final Flag/Shell without replay')
  build(a,2);build(a,3);time.sleep(2)
  for i,v in enumerate(clients):select(v,i);work(v,i)
  before=[info(v)['sandpitDecorationEvents'] for v in clients];ops=('shell','flag','shell','flag')
  with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(lambda pair:button(pair[1],source(ops[pair[0]])),enumerate(clients)))
  wait(lambda:decorations()==[2,1,2,1],'four mixed decorations');converge()
  require(all(info(v)['sandpitDecorationEvents']==[n+1 for n in old] for v,old in zip(clients,before)),'four independent effects missing/duplicate')
  require([info(v)['sandpitHighlight'] for v in clients]==[0,1,2,3],'shared selection appeared');time.sleep(1.05)
  require(all(info(v)['sandpitDecorationEffects']==[0]*4 and info(v)['sandpitDecorationHidden']==[False]*4 for v in clients),'placement expiry stuck')
  home.capture(a,out,'phone-four-mixed-decorations');home.capture(c,out,'tablet-four-mixed-decorations')
  record('one child constructs all towers; four simultaneous mixed decoration slots converge with independent selection and bounded effects')
  select(a,0);select(b,0);work(a,0);work(b,0)
  with ThreadPoolExecutor(max_workers=2) as pool:list(pool.map(lambda pair:button(pair[0],source(pair[1])),((a,'flag'),(b,'shell'))))
  home.ready(a);home.ready(b);time.sleep(1.1);conflict=decorations()[0];require(conflict in (1,2),'invalid conflict result');converge()
  require(all(info(v)['sandpitDecorationEffects'][0]==0 and not info(v)['sandpitDecorationHidden'][0] for v in clients),'conflict left temporary props')
  home.capture(a,out,'same-tower-authoritative-winner')
  # Replace again while its earlier moving prop is still active.
  first='flag' if decorations()[0]==2 else 'shell';decorate(a,0,first,stay=True);second='shell' if first=='flag' else 'flag';button(a,source(second));value=1 if second=='flag' else 2
  wait(lambda:decorations()[0]==value,'replacement while active');effect(a,0,value);home.capture(a,out,'latest-replacement-only');time.sleep(1.05)
  require(info(a)['sandpitDecorationVisuals'][0]==value and not info(a)['sandpitDecorationHidden'][0],'stale moving prop won')
  record('same-tower simultaneous Flag/Shell follows serialized authority; active replacement supersedes earlier prop cleanly')
  decorate(a,0,'shell' if decorations()[0]==1 else 'flag',stay=True);saved=decorations();button(d,'Leave')
  wait(lambda:not next(m for m in state()['members'] if m['actor']==d.profile)['attending'] and info(d)['sandpitDecorationEffects']==[0]*4,'Leave during placement clears local props')
  require(decorations()==saved and sum(m['attending'] for m in state()['members'])==3 and info(b)['sandpitSelection']==0,'departure changed castle/sibling');time.sleep(1.1)
  c.close();wait(lambda:not next(m for m in state()['members'] if m['actor']==c.profile)['attending'],'native disconnect')
  c=run.start('client',run.slots[2]['profile']);clients[2]=c;home.ready(c);wait(lambda:next(m for m in state()['members'] if m['actor']==c.profile)['attending'],'native reconnect');time.sleep(.5)
  require(info(c)['sandpitDecorationVisuals']==saved and info(c)['sandpitDecorationEvents']==[0]*4 and info(c)['sandpitDecorationEffects']==[0]*4,'reconnect lost/replayed props');home.capture(c,out,'native-reconnect-final-decorations')
  cmd(d,7,value='park');cmd(d,7,value='daycare');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four return');time.sleep(.5)
  decorate(a,0,'shell' if decorations()[0]==1 else 'flag',stay=True);before=info(a)
  a.input('touch-begin',role='screen',x=600,y=310,finger=42);a.input('touch-move',role='screen',x=720,y=310,finger=42);a.input('touch-end',role='screen',x=720,y=310,finger=42)
  after=info(a);require(abs(after['cameraX']-before['cameraX'])>50 and after['sandpitDecorationEffects'][0]>0,'pan lost moving decoration');home.capture(a,out,'placement-camera-pan');time.sleep(1.1)
  require(not info(a)['sandpitDecorationHidden'][0] and info(a)['sandpitDecorationVisuals']==decorations(),'pan/expiry detached final decoration')
  record('independent departure during placement preserves siblings; native reconnect baseline and camera movement retain attached props')
  select(a,3);cmd(a,0,x=3900,y=150);button(a,source('shell'));require(info(a)['sandpitApproach']==3,'old decoration approach missing')
  decorate(b,0,'shell' if decorations()[0]==1 else 'flag',stay=True);oldround=state()['round'];wait(lambda:state()['phase']==3,'completed lesson');button(b,'New lesson');wait(lambda:state()['round']==oldround+1,'reset');time.sleep(.35)
  require(info(a)['sandpitApproach']==-1 and decorations()==[0]*4 and all(not m['built'] for m in state()['moulds']),'old approach decorated reset')
  require(all(info(v)['sandpitDecorationEffects']==[0]*4 and info(v)['sandpitDecorationHidden']==[False]*4 for v in clients),'reset left stuck decoration');home.capture(b,out,'new-lesson-cleared-placement')
  record('New Lesson clears authoritative decorations, old pending approach and active placement without mutating new lesson')
  passed=True
 finally:
  run.close();write(out/'result.json',dict(build=args.build,passed=passed,checks=checks,sameTowerFinalDecoration=conflict))
 require(passed,'decoration runtime failed');print('PASS all '+str(len(checks))+' native decoration groups',flush=True)
if __name__=='__main__':main()
