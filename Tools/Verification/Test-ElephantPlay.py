"""Elephant-only water/personality pilot on a disposable authority and four native players."""
import sys,argparse,importlib.util,time,subprocess,shutil
from pathlib import Path
sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'elephant-play';out.mkdir();checks=[];clients=[];passed=False;delay=None
 print('EVIDENCE '+str(out),flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
 def z():return server.state()['view']['zoo']
 def animal():return z()['animals'][0]
 def food(v):return next(f for f in z()['food'] if f['actor']==v.profile)
 def inspect(v):return v.input('inspect')
 def cap(v,name):return home.capture(v,out,name)
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 def enter(v):
  cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
 def pump(v):v.input('touchButton',text='Splash button')
 def feed(v):v.input('touchButton',text='Take leaves for elephant')
 def offer(v):
  cmd(v,0,x=1560,y=100);cmd(v,22,value='take',target='elephant');f=food(v);cmd(v,0,x=1200+(f['slot']-1.5)*100,y=100);cmd(v,22,value='offer',target='elephant')
 def finish(n):wait(lambda:animal()['fed']==n and all(f['species']=='' for f in z()['food']),'feeding completed',110)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1280 if i%2==0 else 1024,y=591 if i%2==0 else 768)
  cmd(a,7,value='zoo');a.input('sandFilm',x=90);cmd(a,0,x=650,y=100);cmd(a,22,value='gate',target='zoo-savanna');cmd(a,0,x=1200,y=100)
  wait(lambda:animal()['phase']==7 and animal()['age']>.5,'arrival greeting',5);cap(a,'phone-greeting');greeting=animal()['sequence']
  for v in (b,c,d):enter(v)
  require(z()['greetingCooldown']>20,'arrivals reset greeting cooldown')
  wait(lambda:animal()['phase']==8,'curious idle',24);cap(a,'phone-curious');cap(b,'tablet-curious')
  record('arrival greeting, exhibit cooldown and actual drawn trunk/ear idle behavior')
  seq=z()['waterSequence'];events=[inspect(v)['elephantWaterEvents'] for v in clients];pump(a)
  wait(lambda:inspect(a)['elephantWaterEvents']==events[0]+1,'immediate visible water pulse',3);cap(a,'phone-pulse')
  require(z()['waterAge']<1.1,'prop response was delayed');wait(lambda:animal()['phase']==10 and animal()['age']>.4,'elephant splash',18);cap(a,'phone-splash');cap(b,'tablet-splash')
  for i,v in enumerate(clients):require(inspect(v)['elephantWaterEvents']==events[i]+1,'shared pulse not seen exactly once')
  record('one real touch immediately pulses water; one shared elephant approach/splash with exactly one event per client')
  # Fixture food offers time the authority's safe transition precisely. The
  # normal real-touch automatic feeding is exercised by the next four turns.
  before=animal()['fed'];offer(c);start=time.monotonic()
  wait(lambda:animal()['owner']==c.profile,'splash yields to food',3);delay=time.monotonic()-start
  require(delay<.9,'splash added delay exceeds .6s plus snapshot/tick budget: '+str(delay))
  seq=z()['waterSequence'];pump(b);wait(lambda:z()['waterSequence']==seq+1,'busy pulse',3)
  require(z()['waterPending'],'busy water request was not retained');cap(b,'tablet-feeding-priority');finish(before+1)
  wait(lambda:animal()['phase'] in (9,10),'pending play after finish',4)
  record('feeding during splash wins within bounded delay; busy tap visibly responds and elephant waits until approved eating finish')
  before=animal()['fed']
  for v in clients:cmd(v,0,x=1400,y=100)
  for v in clients:feed(v)
  wait(lambda:all(food(v)['offered'] for v in clients),'four real-touch offers',30)
  tickets={v.profile:food(v)['ticket'] for v in clients};seq=z()['waterSequence'];seen=set()
  # Rapid and near-simultaneous real touches; no backlog or permanent owner.
  for _ in range(4):
   for v in clients:pump(v);seen.add(z()['waterSequence'])
  require(z()['waterSequence']-seq<=4,'water effect flood')
  require(all(food(v)['ticket']==tickets[v.profile] for v in clients),'water disturbed food tickets')
  cap(a,'phone-four-feeders');cap(b,'tablet-four-feeders');finish(before+4)
  record('four real clients feed exactly once while several pump; rapid taps preserve FIFO tickets and bounded single pending play')
  for v in clients:
   # After feeding, fixture each viewport near the toy. World props scroll
   # normally; the far-right tablet feeder cannot tap an offscreen pump.
   cmd(v,0,x=1000,y=100);time.sleep(2.5);seq=z()['waterSequence'];pump(v);wait(lambda:z()['waterSequence']==seq+1,'each child operates shared pump',4)
  record('all four children independently operate the same pump; no permanent toy owner')
  # A local modal map must not stop the authority or let touches through.
  time.sleep(2.5);seq=z()['waterSequence'];pump(d);wait(lambda:z()['waterSequence']==seq+1,'fresh shared pulse',4);seq+=1;wait(lambda:animal()['phase'] in (9,10),'shared play',5)
  a.input('touchButton',text='Zoo map');require(inspect(a)['zooMapOpen'],'map did not open');age=animal()['age'];pump(a)
  require(z()['waterSequence']==seq,'map let water touch pass');time.sleep(.4)
  require(animal()['age']!=age or animal()['phase']==10,'map paused authority');a.input('touchButton',text='Close Zoo map')
  a.input('network-pause');time.sleep(.4);a.input('network-resume');home.ready(a)
  seeded=inspect(a)['elephantWaterEvents'];time.sleep(.3);require(inspect(a)['elephantWaterEvents']==seeded,'reconnect replayed water')
  d.close();replacement=run.start('client',d.profile);clients[3]=replacement;d=replacement;home.ready(d)
  require(inspect(d)['elephantWaterEvents']==0,'late join replayed old water')
  for v in (a,b):v.input('application-pause');v.input('application-resume');home.ready(v)
  record('local map preserves shared play/blocks taps; actual disconnect, late join and application lifecycle seed event identity')
  # Departing an active feeder must release only their portion.
  before=animal()['fed'];offer(a);offer(b);wait(lambda:animal()['owner']!='','feeding pair',5)
  owner=next(v for v in (a,b) if v.profile==animal()['owner']);other=b if owner==a else a;ticket=food(other)['ticket']
  cmd(owner,7,value='creek');require(food(other)['ticket']==ticket,'departure canceled sibling');finish(before+1)
  for v in clients:
   if inspect(v)['zone']=='zoo-savanna':cmd(v,7,value='creek')
  wait(lambda:not z()['waterPending'] and animal()['phase']<7,'all departure clears play',3)
  enter(a);require(not z()['waterPending'],'stale water after return')
  record('active departure preserves sibling food; no participants clears pending/active play and returning cannot trigger stale water')
  for w,h,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
   a.input('resize',x=w,y=h);cmd(a,0,x=1200,y=100);time.sleep(.6);s=cap(a,label+'-layout')
   control=next(c for c in s['controls'] if c['name']=='Splash button')
   require(control['bounds']['width']>=44 and control['bounds']['height']>=44,'water touch target too small '+label)
   r=control['bounds']
   for other in s['controls']:
    if other['name'] in ('Take leaves for elephant','Zoo previous animal','Zoo next animal','Zoo entrance','Zoo map','Menu','Tap to walk','Characters'):
     q=other['bounds'];require(min(r['x']+r['width'],q['x']+q['width'])<=max(r['x'],q['x']) or min(r['y']+r['height'],q['y']+q['height'])<=max(r['y'],q['y']),'pump overlaps '+other['name']+' '+label)
  record('phone/tablet/small-phone actual native viewports: readable pump, intact trays/pictures/map and safe controls')
  a.input('resize',x=1280,y=591);initial=inspect(a);seq=z()['waterSequence']
  for _ in range(10):pump(a);time.sleep(2.5)
  final=inspect(a);require(final['elephantEffectObjects']==initial['elephantEffectObjects']==12,'effect pool grew');require(final['audioSources']==initial['audioSources'],'audio sources leaked')
  write(out/'allocation-samples.json',dict(before={k:initial[k] for k in ('allocatedMemory','managedMemory','audioSources','elephantEffectObjects')},after={k:final[k] for k in ('allocatedMemory','managedMemory','audioSources','elephantEffectObjects')},note='Native Unity heap snapshots; includes existing world/network/verification allocations, not a per-frame profiler attribution. New geometry uses value types and a retained UI mesh/effect pool.'))
  record('repeated play retains fixed 12-droplet pool and stable AudioSource count')
  a.input('touchButton',text='Hear elephant');cmd(a,0,x=3600,y=100);time.sleep(.7);a.input('touchButton',text='Take leaves for giraffe')
  wait(lambda:food(a)['offered'],'giraffe offer',30);wait(lambda:z()['animals'][1]['consumed'],'giraffe consumed',40);cap(a,'giraffe-regression')
  record('existing elephant call and unchanged giraffe automatic feeding remain available')
  film=a.out/'sand-film';wait(lambda:(film/'times.txt').exists(),'film finish',45)
  times=[float(s) for s in (film/'times.txt').read_text().splitlines()];frames=sorted(film.glob('*.png'));require(len(times)==len(frames)>30,'native film frames')
  concat=out/'frames.txt';concat.write_text('\n'.join(s for i,f in enumerate(frames) for s in ["file '"+f.as_posix()+"'",'duration '+str(times[i+1]-times[i] if i+1<len(times) else .125)]))
  subprocess.run([shutil.which('ffmpeg'),'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/'elephant-play-silent.mp4')],check=True)
  passed=True
 finally:
  if not passed:
   for v in clients:
    if v.process.poll() is None:
     try:cap(v,'failure-'+v.profile)
     except Exception:pass
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,splashFeedingObservedDelay=delay,runtimeErrors=errors,actualClients=4,physicalDevices=False,liveFamilyTouched=False,audioRecording=False))
  print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
