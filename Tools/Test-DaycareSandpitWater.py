"""M3 only: authoritative Water and real native pointer/pour checks, four clients."""
import argparse, importlib.util, time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def camera_check(build):
 run=Run(build,extended_test_lifetime=True);out=run.path/'sandpit-water-camera';out.mkdir();passed=False
 print('EVIDENCE '+str(out),flush=True)
 try:
  server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
  require(home.command(a,7,value='daycare')['accepted'],'travel');home.ready(a);a.input('resize',x=1280,y=591)
  a.input('touchButton',text='Ask Calypso');a.input('touchButton',text='Sandcastle club')
  wait(lambda:server.state()['view']['sandpit']['round']==1,'start');home.ready(a);time.sleep(.8)
  a.input('button',text='Water with watering can')
  wait(lambda:server.state()['view']['sandpit']['moulds'][0]['wet'],'water')
  # Authority can write its view before the client receives that snapshot.
  before=wait(lambda:s if (s:=a.input('inspect'))['sandpitWaterEffects'][0]>0 else None,'client observes active pour',5)
  a.input('touch-begin',role='screen',x=600,y=310,finger=42)
  # The sandpit sits at the world's right edge. Pan inward, not into its clamp.
  a.input('touch-move',role='screen',x=720,y=310,finger=42)
  a.input('touch-end',role='screen',x=720,y=310,finger=42)
  after=a.input('inspect')
  require(abs(after['cameraX']-before['cameraX'])>50 and after['sandpitWaterEffects'][0]>0 and after['sandpitWetVisuals'][0],'camera pan lost active pour/wet state')
  home.capture(a,out,'pour-after-camera-pan');time.sleep(1.3)
  require(a.input('inspect')['sandpitWaterEffects']==[0]*4,'pour persisted forever')
  home.capture(a,out,'can-returned-after-pan');passed=True
 finally:
  run.close();write(out/'result.json',dict(build=build,passed=passed,scope='single isolated release client: real touch camera pan during confirmed Water, active effect and wet graphic retained, then expiry/floor return'))
 print('PASS camera pan during Water and bounded effect expiry',flush=True)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--camera-only',action='store_true');args=parser.parse_args()
 if args.camera_only:return camera_check(args.build)
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'sandpit-water';out.mkdir();checks=[];clients=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['sandpit']
 def info(v):return v.input('inspect')
 def wet():return [m['wet'] for m in state()['moulds']]
 def counts():return [m['scoops'] for m in state()['moulds']]
 def member(v):return next(m for m in state()['members'] if m['actor']==v.profile)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def button(v,name,mouse=False):
  wait(lambda:any(c['name']==name and c['enabled'] for c in info(v)['controls']),'enabled '+name,15)
  return v.input('button' if mouse else 'touchButton',text=name)
 def select(v,i):
  button(v,'Sand mould '+str(i+1));s=info(v)
  require(s['sandpitSelection']==i and s['sandpitHighlight']==i,'wrong local selection/highlight')
 def position(v,i):cmd(v,0,x=4130+i*160,y=350)
 def water(v,i,mouse=False):
  before=info(v)['sandpitWaterEvents'][i];button(v,'Water with watering can',mouse)
  wait(lambda:wet()[i],'direct water '+str(i),15)
  wait(lambda:info(v)['sandpitWaterEvents'][i]==before+1,'accepted transition effect '+str(i))
 def scoop(v,i,n):button(v,'Scoop with shovel');wait(lambda:counts()[i]==n,'direct scoop '+str(i),15)
 def converge():
  want=wet();n=counts()
  wait(lambda:all([m['wet'] for m in v.state()['view']['sandpit']['moulds']]==want and [m['scoops'] for m in v.state()['view']['sandpit']['moulds']]==n for v in clients),'all shared fills converge')
  require(all(info(v)['sandpitWetVisuals']==want for v in clients),'graphics contradict authoritative wet state')
 def overlap(a,b):return min(a['x']+a['width'],b['x']+b['width'])>max(a['x'],b['x']) and min(a['y']+a['height'],b['y']+b['height'])>max(a['y'],b['y'])
 def concurrent(vs):
  with ThreadPoolExecutor(max_workers=len(vs)) as pool:list(pool.map(lambda v:button(v,'Water with watering can'),vs))
 def finish(v):
  for i in range(4):
   select(v,i);position(v,i)
   for n in range(counts()[i]+1,(3 if i%2 else 2)+1):scoop(v,i,n)
   if not wet()[i]:button(v,'Add water');wait(lambda:wet()[i],'fallback water')
   button(v,'Tip bucket');wait(lambda:state()['moulds'][i]['built'],'existing tip')
  wait(lambda:state()['phase']==3,'existing shared completion',45)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:
   home.ready(v);cmd(v,7,value='park' if v==d else 'daycare')
   v.input('resize',x=1024 if v==c else 1280,y=768 if v==c else 591);home.ready(v)
  button(a,'Ask Calypso');button(a,'Sandcastle club');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'common start')
  select(a,0);select(b,1);require(info(a)['sandpitHighlight']==0,'sibling overwrote selection')
  for v in (a,c):
   controls=info(v)['controls'];can=next(x['bounds'] for x in controls if x['name']=='Water with watering can')
   require(can['width']>=100 and can['height']>=85,'small watering target')
   require(not any(overlap(can,x['bounds']) for x in controls if x['name']=='Scoop with shovel' or x['name'].startswith('Sand mould ')),'watering target overlaps scoop/mould selection')
  home.capture(a,out,'watering-can-dry-phone');home.capture(c,out,'watering-can-dry-tablet')
  for end in ('touch-cancel','touch-end'):
   a.input('touch-begin',role='ui:Water with watering can',finger=8,x=0,y=0)
   if end=='touch-end':a.input('touch-move',role='ui:Water with watering can',finger=8,x=45,y=0)
   a.input(end,role='ui:Water with watering can',finger=8,x=45 if end=='touch-end' else 0,y=0)
  time.sleep(.3);require(wet()==[False]*4 and info(a)['sandpitApproach']==-1,'canceled/swiped touch watered')
  record('visible grounded can, generous nonoverlapping phone/tablet targets, independent highlights and canceled/swiped touches')
  cmd(a,0,x=3900,y=150);button(a,'Water with watering can');require(info(a)['sandpitApproach']==0,'Water approach not pending')
  select(a,1);time.sleep(.8);require(wet()==[False]*4 and info(a)['sandpitApproachRound']==-1,'switch retained watering intent')
  cmd(a,0,x=3900,y=150);button(a,'Water with watering can');button(a,'Leave');wait(lambda:not member(a)['attending'],'leave pending')
  time.sleep(.8);require(wet()==[False]*4 and info(a)['sandpitApproach']==-1 and all(x==0 for x in info(a)['sandpitWaterEffects']),'Leave retained Water/effect')
  require(info(b)['sandpitHighlight']==1 and member(b)['attending'],'Leave affected sibling')
  cmd(a,7,value='park');cmd(a,7,value='daycare');wait(lambda:member(a)['attending'] and any(m['actor']==a.profile and m['attending'] for m in info(a)['sandpit']['members']),'return joins');time.sleep(.8)
  select(a,0);water(a,0,mouse=True);require(counts()[0]==0,'Water before sand invented scoops')
  time.sleep(.2);home.capture(a,out,'pour-empty-small-phone');home.capture(c,out,'pour-empty-small-tablet')
  cmd(d,7,value='daycare');wait(lambda:member(d)['attending'] and info(d)['sandpit']['moulds'][0]['wet'],'late join sees water')
  require(info(d)['sandpitWaterEvents']==[0]*4,'late join animated old water')
  record('pending switch/Leave cancel Water, actual mouse water before sand, accepted pour and late join wet baseline')
  select(b,1);select(c,1);position(b,1);position(c,1)
  before=[info(v)['sandpitWaterEvents'][1] for v in clients];concurrent([b,c]);wait(lambda:wet()[1],'same-mould water')
  b.input('resize',x=1024,y=768);home.capture(b,out,'same-mould-pour-after-layout-change');b.input('resize',x=1280,y=591)
  converge();require([info(v)['sandpitWaterEvents'][1] for v in clients]==[x+1 for x in before],'same-mould water fabricated duplicate transitions')
  before=info(b)['sandpitWaterEvents'];button(b,'Water with watering can');button(b,'Water with watering can');time.sleep(.6)
  require(wet()[1] and info(b)['sandpitWaterEvents']==before,'redundant water restarted transition');record('two-client same-mould water converges with one transition; repeated wet water creates no new pour; layout changes retain targeting')
  select(c,2);position(c,2);scoop(c,2,1);select(b,2);water(b,2);scoop(c,2,2);converge()
  require(counts()[2]==2 and wet()[2],'Scoop-Water-Scoop interleave corrupted state');home.capture(c,out,'wet-filled-sand-tablet')
  record('real direct Scoop-Water-Scoop preserves full scoop count and visible damp sand')
  # Complete through retained controls, then a sibling resets during Water approach.
  finish(a);select(a,3);cmd(a,0,x=3900,y=150);before=info(a)['sandpitWaterEvents'];button(a,'Water with watering can')
  require(info(a)['sandpitApproach']==3,'reset fixture has no pending Water');button(b,'New lesson');wait(lambda:state()['round']==2,'new lesson');time.sleep(.8)
  require(wet()==[False]*4 and counts()==[0]*4 and info(a)['sandpitApproachRound']==-1 and info(a)['sandpitWaterEvents']==before and all(x==0 for x in info(a)['sandpitWaterEffects']),'old Water crossed lesson boundary')
  for i,v in enumerate(clients):select(v,i);position(v,i)
  concurrent(clients);wait(lambda:wet()==[True]*4,'four different mould waters');converge()
  require([info(v)['sandpitHighlight'] for v in clients]==[0,1,2,3] and counts()==[0]*4,'four children selection/count conflict')
  home.capture(a,out,'four-shared-wet-moulds-phone');record('New Lesson cancels pending Water/effects; four children independently water four shared empty moulds')
  button(d,'Leave');wait(lambda:not member(d)['attending'],'independent departure');time.sleep(.3)
  require(wet()==[True]*4 and info(a)['sandpitHighlight']==0 and member(a)['attending'] and all(x==0 for x in info(d)['sandpitWaterEffects']),'departure changed siblings or stuck pour')
  profile=c.profile;c.close();wait(lambda:not member(c)['attending'],'native disconnect')
  c=run.start('client',profile);clients[2]=c;home.ready(c);wait(lambda:member(c)['attending'],'native reconnect');converge()
  require(info(c)['sandpitWaterEvents']==[0]*4,'reconnect poured historical state');record('independent departure and actual native close/reconnect preserve wet progress and clear local animation')
  finish(a);select(a,0);position(a,0);before=info(a)['sandpitWaterEvents'];button(a,'Water with watering can');time.sleep(.8)
  require(info(a)['sandpitWaterEvents']==before and all(m['built'] and m['wet'] for m in state()['moulds']),'built rejection fabricated pour')
  home.capture(a,out,'built-water-rejected-phone');record('one child completes shared castle with retained fallback; built mould rejects direct Water without false feedback')
  passed=True
 finally:
  if not passed:
   for i,v in enumerate(clients):
    try:home.capture(v,out,'failure-'+str(i+1))
    except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,exitCodes=[v.process.returncode for v in run.instances],scope='isolated native release authority/four clients; actual mouse/InputSystem touches; phone/tablet pour/layout captures; no devices/live-server update'))
 print('PASS ALL '+str(len(checks))+' groups',flush=True)

if __name__=='__main__':main()
