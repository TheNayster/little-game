"""M4: real native Tip controls, authoritative outcomes and bounded local visuals."""
import argparse, importlib.util, time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def visual_check(build):
 run=Run(build,extended_test_lifetime=True);out=run.path/'sandpit-tip-visual';out.mkdir();passed=False
 print('EVIDENCE '+str(out),flush=True)
 try:
  server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
  require(home.command(a,7,value='daycare')['accepted'],'travel');home.ready(a);a.input('resize',x=1280,y=591)
  a.input('touchButton',text='Ask Calypso');a.input('touchButton',text='Sandcastle club');wait(lambda:server.state()['view']['sandpit']['round']==1,'start');time.sleep(.8)
  def tool(op):
   require(home.command(a,0,x=4210,y=440)['accepted'],'work')
   require(home.command(a,32,value=op,target='0@1')['accepted'],op);home.ready(a)
  def frame(name,maximum):
   wait(lambda:0<a.input('inspect')['sandpitTipEffects'][0]<maximum,'frame '+name);home.capture(a,out,name)
  tool('scoop');tool('scoop');a.input('button',text='Sand mould 1');wait(lambda:server.state()['view']['sandpit']['moulds'][0]['scoops']==0,'dry reset')
  frame('phone-dry-tipped-and-pile',1.1);frame('phone-dry-collapse',.65);time.sleep(1);require(a.input('inspect')['sandpitTipEffects']==[0]*4,'dry expiry');home.capture(a,out,'phone-empty-returned')
  a.input('resize',x=1024,y=768);tool('scoop');tool('scoop');a.input('button',text='Sand mould 1');wait(lambda:server.state()['view']['sandpit']['moulds'][0]['scoops']==0,'second dry reset')
  frame('tablet-dry-tipped-and-pile',1.1);frame('tablet-dry-collapse',.65);time.sleep(1)
  tool('water');tool('scoop');tool('scoop');a.input('button',text='Sand mould 1');wait(lambda:server.state()['view']['sandpit']['moulds'][0]['built'],'wet built');frame('tablet-wet-lift',.7);time.sleep(1)
  s=a.input('inspect');require(s['sandpitBuiltVisuals'][0] and s['sandpitTowerReveal'][0]==1 and s['sandpitTipEffects'][0]==0,'permanent tower');home.capture(a,out,'tablet-permanent-tower');passed=True
 finally:
  run.close();write(out/'result.json',dict(build=build,passed=passed,scope='final pile contrast: phone/tablet direct mouse dry tip, collapse, empty return, wet reveal and permanent tower'))
 require(passed,'Tip visual check failed');print('PASS final phone/tablet pile contrast, dry expiry and wet permanent tower',flush=True)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--visual-only',action='store_true');args=parser.parse_args()
 if args.visual_only:return visual_check(args.build)
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'sandpit-tip';out.mkdir();checks=[];clients=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['sandpit']
 def info(v):return v.input('inspect')
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
 def button(v,name):
  wait(lambda:any(c['name']==name and c['enabled'] for c in info(v)['controls']),'enabled '+name,15)
  return v.input('touchButton',text=name)
 def select(v,i):
  button(v,'Choose sand mould '+str(i+1));require(info(v)['sandpitSelection']==i,'local selection')
 def work(v,i):cmd(v,0,x=4130+i*160,y=350)
 def tool(v,op,i):
  cmd(v,0,x=4210+i*160,y=440)
  return cmd(v,32,value=op,target=str(i)+'@'+str(state()['round']))
 def fill(v,i,wet=False):
  while state()['moulds'][i]['scoops']<(3 if i%2 else 2):tool(v,'scoop',i)
  if wet:tool(v,'water',i)
 def effect(v,i,outcome):return wait(lambda:s if (s:=info(v))['sandpitTipOutcomes'][i]==outcome and s['sandpitTipEffects'][i]>0 else None,'confirmed '+outcome,8)
 def tip(v,i):select(v,i);work(v,i);button(v,'Sand mould '+str(i+1))
 def finish_except(last):
  for i in range(4):
   if i==last or state()['moulds'][i]['built']:continue
   fill(a,i,True);tool(a,'tip',i)
  wait(lambda:state()['phase']>=2,'teacher finishes demonstration',35)
 def converge():
  want=state()['moulds'];wait(lambda:all(v.state()['view']['sandpit']['moulds']==want for v in clients),'four shared end states converge')
 def reset():
  finish_except(-1);wait(lambda:state()['phase']==3,'complete lesson');button(b,'New lesson');wait(lambda:all(not m['built'] and m['scoops']==0 for m in state()['moulds']),'reset');home.ready(a);time.sleep(.3)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:home.ready(v);cmd(v,7,value='park' if v is d else 'daycare')
  for v in (a,b,d):v.input('resize',x=1280,y=591)
  c.input('resize',x=1024,y=768)
  button(a,'Ask Calypso');button(a,'Sandcastle club');wait(lambda:state()['round']==1,'start');time.sleep(.8)
  for i,v in enumerate((a,b,c)):select(v,i)
  tool(a,'scoop',0);select(a,0);work(a,0);button(a,'Sand mould 1');effect(a,0,'underfilled')
  home.capture(a,out,'phone-underfilled-wiggle')
  require(state()['moulds'][0]['scoops']==1 and not state()['moulds'][0]['built'] and info(a)['sandpitTipEvents'][0]==0,'underfilled falsely cleared/built/animated')
  time.sleep(.45);require(info(a)['sandpitTipEffects'][0]==0,'wiggle stuck')
  # Swipe/cancel the direct bucket target must not turn into a Tip.
  for cancel in (True,False):
   a.input('touch-begin',role='ui:Sand mould 1',finger=21,x=-20,y=0)
   if cancel:a.input('touch-cancel',role='ui:Sand mould 1',finger=21,x=-20,y=0)
   else:
    a.input('touch-move',role='ui:Sand mould 1',finger=21,x=45,y=0);a.input('touch-end',role='ui:Sand mould 1',finger=21,x=45,y=0)
  require(state()['moulds'][0]['scoops']==1,'drag/cancel sent Tip')
  record('underfilled direct Tip retains fill; local wiggle only; canceled/swiped bucket taps')
  fill(a,0);select(a,0);cmd(a,0,x=3900,y=150);button(a,'Sand mould 1');require(info(a)['sandpitApproach']==0,'approach missing');select(a,1);require(info(a)['sandpitApproach']==-1,'switch did not cancel Tip');time.sleep(.4);require(state()['moulds'][0]['scoops']==2 and state()['moulds'][1]['scoops']==0,'switch mutated wrong mould')
  select(a,0);cmd(a,0,x=3900,y=150);button(a,'Sand mould 1');button(a,'Leave');wait(lambda:not next(m for m in state()['members'] if m['actor']==a.profile)['attending'],'leave');require(state()['moulds'][0]['scoops']==2 and sum(m['attending'] for m in state()['members'])==2,'pending Leave damaged siblings')
  cmd(a,7,value='park');cmd(a,7,value='daycare');wait(lambda:next(m for m in state()['members'] if m['actor']==a.profile)['attending'],'rejoin');home.ready(a);time.sleep(.8)
  record('pending Tip cancels on selection/independent Leave and visit return retains sibling progress')
  before=info(a)['sandpitTipEvents'][0];tip(a,0);effect(a,0,'collapse');require(state()['moulds'][0]==dict(scoops=0,decoration=0,wet=False,built=False),'dry rule changed')
  home.capture(a,out,'phone-dry-start-turn');wait(lambda:info(a)['sandpitTipEffects'][0]<1.1,'dry inverted');home.capture(a,out,'phone-dry-inverting');wait(lambda:info(a)['sandpitTipEffects'][0]<.8,'dry collapse stage');home.capture(a,out,'phone-dry-crumbling')
  button(a,'Sand mould 1');require(info(a)['sandpitTipEvents'][0]==before+1,'repeat Tip created false second collapse')
  time.sleep(2);require(info(a)['sandpitTipEffects']==[0]*4,'collapse did not expire');home.capture(a,out,'phone-dry-returned')
  record('confirmed full dry Tip resets once, visibly tips/crumbles/returns; repeated rejected Tip has no second collapse')
  fill(c,2,True);tip(c,2);effect(c,2,'reveal');home.capture(c,out,'tablet-wet-start-turn');wait(lambda:info(c)['sandpitTipEffects'][2]<1.1,'wet inverted');home.capture(c,out,'tablet-wet-inverting');wait(lambda:info(c)['sandpitTipEffects'][2]<.7,'wet uncover');home.capture(c,out,'tablet-wet-lifting');time.sleep(2)
  s=info(c);require(s['sandpitBuiltVisuals'][2] and s['sandpitTowerReveal'][2]==1 and s['sandpitTipEffects'][2]==0,'permanent tower vanished');home.capture(c,out,'tablet-stable-tower')
  tool(c,'shell',2);require(state()['moulds'][2]['decoration']==2,'fallback decoration broken')
  cmd(d,7,value='daycare');wait(lambda:next(m for m in state()['members'] if m['actor']==d.profile)['attending'],'late fourth');time.sleep(.7)
  require(info(d)['sandpitBuiltVisuals'][2] and info(d)['sandpitTipEvents'][2]==0,'late arrival replayed/removed tower')
  record('wet direct Tip reveals permanent grounded small tower; shell fallback and late fourth baseline retained')
  # Native interleaving: client starts walking to a dry full bucket; sibling waters first.
  fill(a,1);select(a,1);cmd(a,0,x=3900,y=150);button(a,'Sand mould 2');tool(b,'water',1)
  wait(lambda:state()['moulds'][1]['built'],'water-before-Tip builds',15);effect(a,1,'reveal');home.capture(a,out,'phone-water-before-tip-reveal');time.sleep(2)
  require(info(a)['sandpitTipEvents'][1]==1,'race produced duplicate outcomes')
  # Same for the final scoop while approaching; use tray Tip fallback here.
  tool(a,'scoop',3);tool(a,'scoop',3);select(a,3);cmd(a,0,x=3900,y=150);button(a,'Tip bucket');tool(b,'scoop',3)
  wait(lambda:state()['moulds'][3]['scoops']==0,'scoop-before-Tip dry reset',15);effect(a,3,'collapse');time.sleep(2)
  record('native water-before-Tip reveals; final-scoop-before-tray-Tip collapses from actual result')
  reset()
  for i,v in enumerate(clients):fill(v,i,True);select(v,i);work(v,i)
  before=[info(v)['sandpitTipEvents'] for v in clients]
  with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(lambda pair:button(pair[1],'Sand mould '+str(pair[0]+1)),enumerate(clients)))
  wait(lambda:all(m['built'] for m in state()['moulds']),'four direct Tips');converge()
  require(all(info(v)['sandpitTipEvents']==[n+1 for n in old] for v,old in zip(clients,before)),'four separate reveals missing/duplicated')
  home.capture(a,out,'phone-four-shared-reveals');home.capture(c,out,'tablet-four-shared-reveals')
  button(d,'Leave');wait(lambda:not next(m for m in state()['members'] if m['actor']==d.profile)['attending'] and info(d)['sandpitTipEffects']==[0]*4,'accepted Leave clears local effects');require(sum(m['attending'] for m in state()['members'])==3 and all(m['built'] for m in state()['moulds']),'departure reset siblings')
  time.sleep(2);require(info(a)['sandpitTowerReveal']==[1]*4 and info(a)['sandpitBuiltVisuals']==[True]*4,'four permanent towers wrong')
  c.close();wait(lambda:not next(m for m in state()['members'] if m['actor']==c.profile)['attending'],'native disconnect')
  c=run.start('client',run.slots[2]['profile']);clients[2]=c;home.ready(c);wait(lambda:next(m for m in state()['members'] if m['actor']==c.profile)['attending'],'native reconnect');time.sleep(.5)
  require(info(c)['sandpitBuiltVisuals']==[True]*4 and info(c)['sandpitTipEvents']==[0]*4,'reconnect replayed half-reveal');home.capture(c,out,'reconnect-permanent-towers')
  record('four simultaneous different Tips; independent Leave during effects; native close/reconnect renders permanent end state')
  cmd(d,7,value='park');cmd(d,7,value='daycare');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'return fourth');reset()
  fill(a,0,True);select(a,0);select(b,0);work(a,0);work(b,0);old=info(a)['sandpitTipEvents'][0]
  with ThreadPoolExecutor(max_workers=2) as pool:list(pool.map(lambda v:button(v,'Sand mould 1'),(a,b)))
  wait(lambda:state()['moulds'][0]['built'],'same mould build');effect(a,0,'reveal');require(info(a)['sandpitTipEvents'][0]==old+1 and info(b)['sandpitTipEvents'][0]>=1,'same mould double reveal')
  before=info(a);a.input('touch-begin',role='screen',x=600,y=310,finger=42);a.input('touch-move',role='screen',x=720,y=310,finger=42);a.input('touch-end',role='screen',x=720,y=310,finger=42)
  after=info(a);require(abs(after['cameraX']-before['cameraX'])>50 and after['sandpitTipEffects'][0]>0,'pan lost active reveal');home.capture(a,out,'reveal-camera-pan');time.sleep(2);require(info(a)['sandpitTowerReveal'][0]==1,'pan/expiry removed tower')
  finish_except(3);fill(d,3,True);select(a,3);cmd(a,0,x=3000,y=300);button(a,'Tip bucket');require(info(a)['sandpitApproach']==3,'old Tip approach missing')
  tip(d,3);effect(d,3,'reveal');oldround=state()['round'];wait(lambda:state()['phase']==3,'complete');button(b,'New lesson');wait(lambda:state()['round']==oldround+1,'new lesson');time.sleep(.3)
  require(info(a)['sandpitApproach']==-1 and all(info(v)['sandpitTipEffects']==[0]*4 and info(v)['sandpitTowerReveal']==[1]*4 for v in clients),'reset retained old approach/reveal');require(all(m['scoops']==0 and not m['built'] for m in state()['moulds']),'old Tip mutated new lesson')
  home.capture(d,out,'reset-cleared-reveal');record('same-mould simultaneous Tip produces one reveal; camera pan retains it; New Lesson clears active reveal and old approach')
  passed=True
 finally:
  run.close();write(out/'result.json',dict(build=args.build,passed=passed,checks=checks))
 require(passed,'Tip runtime failed')
 print('PASS all '+str(len(checks))+' native Tip groups',flush=True)
if __name__=='__main__':main()
