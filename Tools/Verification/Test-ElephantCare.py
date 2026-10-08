"""Elephant care on isolated native release clients; no video or live-family changes."""
import sys,argparse,importlib.util,time,uuid,subprocess
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,Instance,wait,require,write,read,ROOT
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def regression(build):
 run=Run(build,extended_test_lifetime=True);out=run.path/'elephant-care-regression';out.mkdir();passed=False;checks=[]
 print('EVIDENCE '+str(out),flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a=clients[0]
  for v in clients:
   home.ready(v);v.input('resize',x=1280,y=591);cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
  cmd(a,0,x=4000,y=100);time.sleep(.8)
  a.input('touchButton',text='Take leaves for giraffe');wait(lambda:any(f['actor']==a.profile and f['offered'] for f in server.state()['view']['zoo']['food']),'giraffe offered',30)
  wait(lambda:server.state()['view']['zoo']['animals'][1]['consumed'],'unchanged giraffe consumption',45);home.capture(a,out,'giraffe-regression');checks.append('Actual native giraffe tap-to-feed after fixture camera settles')
  cmd(a,0,x=1200,y=100);time.sleep(.8)
  a.input('touchButton',text='Hear elephant');checks.append('Ordinary elephant-call control remains usable')
  for v in clients:
   wait(lambda:not v.input('inspect')['pending'],'selection ready',8);v.input('touchButton',text='Choose elephant brush');wait(lambda:v.profile in server.state()['view']['zoo']['careMembers'],'care membership',5)
  wait(lambda:server.state()['view']['zoo']['animals'][0]['phase']==13,'four care helpers',25)
  session=server.state()['view']['zoo']['careSession']
  for _ in range(3):a.input('touchButton',text='Choose elephant brush')
  require(server.state()['view']['zoo']['careSession']==session,'rapid selection restarted active care')
  for v,fps in zip(clients[:2],(30,60)):
   v.input('frameRate',x=fps);wait(lambda:not v.input('inspect')['pending'] and v.input('inspect')['zoo']['animals'][0]['phase']==13,'frame-rate helper ready',8)
   v.input('touch-begin',x=1130,y=320+202/.45,finger=62);v.input('touch-move',x=1150,y=320+202/.45,finger=62);v.input('touch-end',x=1150,y=320+202/.45,finger=62);time.sleep(.4)
  wait(lambda:server.state()['view']['zoo']['careProgress'][0]==2,'one gesture credit at 30 and 60fps',5);checks.append('Rapid tool selection preserves session; 30fps/60fps native strokes each contribute one credit')
  for v in clients:v.close()
  wait(lambda:not server.state()['connected'] and not server.state()['view']['zoo']['careMembers'] and server.state()['view']['zoo']['animals'][0]['phase']<7,'empty connected family releases care immediately',5)
  checks.append('Four actual client disconnects clear care before empty-session ticking stops');passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=build,checks=checks,runtimeErrors=errors,actualClients=4,recordedVideo=False,physicalDevices=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--solo',action='store_true');p.add_argument('--regression-only',action='store_true');args=p.parse_args()
 if args.regression_only:return regression(args.build)
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'elephant-care';out.mkdir();checks=[];clients=[];passed=False;server=None;serials={};delay=None
 print('EVIDENCE '+str(out),flush=True)
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
 def inspect(v):return v.input('inspect')
 def z():return inspect(clients[0])['zoo'] if args.solo else server.state()['view']['zoo']
 def animal():return z()['animals'][0]
 def cap(v,name):return home.capture(v,out,name)
 def enter(v):
  if args.solo:
   v.input('fixtureTravel',text='zoo');home.ready(v);v.input('touchButton',text='Visit the savanna');wait(lambda:inspect(v)['zone']=='zoo-savanna','private gate',30);home.ready(v)
   v.input('touch-begin',x=1200,y=100,finger=63);v.input('touch-end',x=1200,y=100,finger=63);wait(lambda:abs(inspect(v)['players'][0]['position']['x']-1200)<10,'private walk to care neighborhood',8);time.sleep(.5)
  else:cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
 def select(v):
  wait(lambda:not inspect(v)['pending'],'selection ready',8);time.sleep(.2);v.input('touchButton',text='Choose elephant brush')
  if not inspect(v)['zooMapOpen']:wait(lambda:v.profile in z()['careMembers'],'brush selection accepted',5)
 def care():
  wait(lambda:animal()['phase']==13 and inspect(clients[0])['zoo']['animals'][0]['phase']==13,'elephant presents side',30);time.sleep(.2)
 def touch(v,patch,stroke=False,hold=False):
  wait(lambda:not inspect(v)['pending'],'gesture ready',8)
  dx,dy=[(-70,202),(22,234),(105,160)][patch];a=animal();x,y=a['fromX']+dx,a['fromY']+dy/.45
  v.input('touch-begin',x=x,y=y,finger=61)
  if stroke:v.input('touch-move',x=x+24,y=y+4/.45,finger=61)
  if hold:
   samples=[inspect(v)];time.sleep(.12);samples.append(inspect(v));time.sleep(.12);samples.append(inspect(v))
   positions=[sample['elephantBrushPositions'][0]['x'] for sample in samples if sample['elephantBrushPositions']]
   require(len(positions)>1 and max(positions)-min(positions)>2,'brush has no side-to-side motion')
   for sample in samples:
    require(all(color['r']>.9 for color in sample['zooRouteColors']),'top route tint flickered during brush')
   write(out/'brush-motion-and-route-tints.json',samples);time.sleep(.5);v.input('touch-move',x=x+10,y=y,finger=61)
  v.input('touch-end',x=x+24 if stroke else x,y=y,finger=61);time.sleep(.4)
 def finish(v):
  for patch in range(3):
   for attempt in range(8):
    if z()['careProgress'][patch]>=4:break
    touch(v,patch,stroke=patch%2==1)
   require(z()['careProgress'][patch]==4,'patch did not accept real touches')
  wait(lambda:z()['careComplete'],'care complete',5)
 def offer(v):
  cmd(v,0,x=1560,y=100);cmd(v,22,value='take',target='elephant');f=next(f for f in z()['food'] if f['actor']==v.profile);cmd(v,0,x=1200+(f['slot']-1.5)*100,y=100);cmd(v,22,value='offer',target='elephant')
 def fooddone(n):wait(lambda:animal()['fed']==n and animal()['owner']=='','feeding complete',65)
 try:
  if args.solo:
   from family_pairing import create_family,write_record
   authority,players,_=create_family();slot=players[0];run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'elephant-care';out.mkdir();print('PRIVATE EVIDENCE '+str(out),flush=True)
   def private():
    v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=slot['profile'];v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
    pair=run.path/(v.identity+'.pairing');write_record(pair,slot)
    cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True)
    config=run.path/(v.identity+'.config.json');write(config,cfg)
    startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
    v.process=subprocess.Popen([str(run.folder/'Client/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591'],startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
    run.instances.append(v);wait(lambda:v.status(),'private startup',30);home.ready(v);return v
   a=private();clients=[a]
  else:
   server=run.start('server');a=run.start('client',run.slots[0]['profile']);clients=[a];home.ready(a);a.input('resize',x=1280,y=591)
  enter(a);select(a);care();cap(a,'solo-start');before=inspect(a)['elephantCareFinishEvents'];touch(a,0,hold=True)
  require(z()['careProgress'][0]==1,'held input multiplied progress');cap(a,'one-patch-fading');finish(a)
  wait(lambda:animal()['phase']==14,'appreciation',5);cap(a,'solo-thank-you');require(inspect(a)['elephantCareFinishEvents']==before+1,'completion did not occur exactly once')
  time.sleep(2.4);require(inspect(a)['elephantCareFinishEvents']==before+1 and animal()['fed']==0,'extra finish/false feeding')
  a.input('touchButton',text='Put elephant brush away');wait(lambda:not z()['careMembers'],'put away',4);select(a);care();require(z()['careProgress']==[0,0,0],'replay not deliberate reset')
  record('solo tap/stroke/held input, progressive fade, once-only appreciation, put-away and deliberate replay')
  if args.solo:
   touch(a,1,stroke=True);a.input('application-pause');a.input('application-resume');home.ready(a);require(z()['careProgress'][1]==1,'pause lost patches')
   save=Path(inspect(a)['savePath']);require(save.exists(),'private save missing');a.input('application-pause');time.sleep(.3);a.close();a=private();clients=[a]
   require(not z()['careMembers'] and z()['careSession']==0 and animal()['phase']<7 and inspect(a)['elephantCareFinishEvents']==0,'save restored transient care');cap(a,'private-reopened')
   record('private pause/resume and real saved-world process reopening clear transient care without old effects')
  else:
   clients.extend(run.start('client',slot['profile']) for slot in run.slots[1:]);b,c,d=clients[1:]
   for v in (b,c,d):home.ready(v);v.input('resize',x=1280,y=591);enter(v);select(v)
   require(len(z()['careMembers'])==4,'four helpers did not join shared session')
   for v in clients:wait(lambda:not inspect(v)['pending'] and len(inspect(v)['zoo']['careMembers'])==4,'all clients ready with shared care',8)
   time.sleep(.3)
   with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(lambda v:touch(v,0),clients))
   wait(lambda:z()['careProgress'][0]==4,'simultaneous shared patch clamp',5);require(not z()['careComplete'],'unexpected completion');cap(a,'four-helper-patch')
   touch(b,1,stroke=True);touch(a,1);require(z()['careProgress'][1]==2,'two players do not combine')
   session=z()['careSession'];epoch=z()['careMemberEpoch'][z()['careMembers'].index(a.profile)];r=home.command(a,22,value='brush',target='elephant',item=f'{session}/{epoch}/99/9000');require(not r['accepted'],'invalid patch accepted')
   r=home.command(a,22,value='brush',target='elephant',item=f'{session-1}/{epoch}/1/9000');require(not r['accepted'],'old session accepted')
   r=home.command(a,22,value='brush',target='elephant',item=f'{session}/{epoch}/1/1');require(not r['accepted'],'repeated gesture accepted')
   record('two/four native helpers combine progress; simultaneous patch inputs clamp; invalid/stale/repeated requests rejected')
   a.input('touchButton',text='Zoo map');require(inspect(a)['zooMapOpen'],'map failed');progress=z()['careProgress'];touch(a,2);select(a);require(z()['careProgress']==progress,'map input brushed through');a.input('touchButton',text='Close Zoo map')
   a.input('application-pause');a.input('application-resume');home.ready(a);require(z()['careProgress']==progress,'pause changed shared care')
   a.input('network-pause');time.sleep(.25);a.input('network-resume');home.ready(a);require(z()['careProgress']==progress,'reconnect changed shared patches');seed=inspect(a)['elephantCareFinishEvents']
   d.close();d=run.start('client',d.profile);clients[3]=d;home.ready(d);require(inspect(d)['elephantCareFinishEvents']==0 and inspect(d)['zoo']['careProgress']==progress,'late join effects/progress')
   select(d);care();wait(lambda:not inspect(d)['pending'],'late helper ready',8);touch(d,2);require(z()['careProgress'][2]==1,'reconnected helper cannot brush with fresh serial');progress=z()['careProgress'];cmd(b,7,value='creek');wait(lambda:b.profile not in z()['careMembers'],'departure',3);require(z()['careProgress']==progress,'departure reset siblings')
   record('map blocks care and selection; pause/reconnect/late join retain patches without effects; one exit preserves siblings')
   fed=animal()['fed'];offer(c);start=time.monotonic();wait(lambda:animal()['owner']==c.profile,'food takes over',3);delay=time.monotonic()-start;require(delay<.85,'feeding delayed excessively');require(z()['careProgress']==progress,'food reset care')
   a.input('touchButton',text='Splash button');wait(lambda:z()['waterPending'],'water queued during care suspension',4);cap(a,'feeding-suspends-care');fooddone(fed+1);care();require(not animal()['owner'],'care/feeding simultaneous')
   record('food during care wins within .35 seconds plus authority tick; approved eating finish retained; older care resumes ahead of pending water')
   finish(a);wait(lambda:animal()['phase']==14,'shared appreciation',4);cap(a,'shared-complete');time.sleep(2.5);require(inspect(a)['elephantCareFinishEvents']==seed+1,'shared duplicate appreciation')
   wait(lambda:animal()['phase'] in (9,10,11),'water after care',10);select(a);require(animal()['phase'] in (9,10,11),'care interrupted water');care()
   for v in clients:
    if inspect(v)['zone']=='zoo-savanna':cmd(v,7,value='creek')
   wait(lambda:not z()['careMembers'] and not z()['waterPending'] and animal()['phase']<7,'all depart',4)
   enter(a);offer(a);select(a);require(animal()['phase']!=13,'care interrupted food before care');fooddone(fed+2);care()
   finish(a);wait(lambda:animal()['phase']==14,'finish phase',3);offer(a);wait(lambda:animal()['owner']==a.profile,'food during finish',3);fooddone(fed+3)
   record('water/requests serialize; all leave clears transient care; food before care and during finish takes priority')
   select(a);care()
   for w,h,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
    a.input('resize',x=w,y=h);cmd(a,0,x=1200,y=100);time.sleep(.4);s=cap(a,label+'-care')
    control=next(c for c in s['controls'] if c['name']=='Choose elephant brush');require(control['bounds']['width']>=44 and control['bounds']['height']>=44,'brush target too small '+label)
   record('native phone/tablet/small-phone simulated layouts checked at full viewport; no physical-device deployment')
   a.input('resize',x=1280,y=591);a.input('touchButton',text='Put elephant brush away');a.input('touchButton',text='Hear elephant');cmd(a,0,x=4000,y=100);time.sleep(.8);a.input('touchButton',text='Take leaves for giraffe');wait(lambda:z()['animals'][1]['consumed'],'unchanged giraffe feeding',60);cap(a,'giraffe-regression')
   record('ordinary elephant call, retained pool/pump/trays/navigation and unchanged giraffe feeding')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,actualClients=len(clients),private=args.solo,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False,feedingObservedDelay=delay))
  print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
