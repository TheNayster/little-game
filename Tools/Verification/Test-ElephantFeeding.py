"""Elephant presentation acceptance on an isolated authority and actual native clients."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import argparse,importlib.util,time,shutil,subprocess,json
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'elephant';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 def state():return server.state()['view']['zoo']
 def animal():return state()['animals'][0]
 def food(v):return next(f for f in state()['food'] if f['actor']==v.profile)
 def tap(v):v.input('touchButton',text='Take leaves for elephant')
 def inspect(v):return v.input('inspect')
 def cap(v,name):return home.capture(v,out,name)
 def enter(v):
  cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
 def finished(n,owners=None):
  seen=[];last=animal()['fed']
  def done():
   nonlocal last
   a=animal()
   if a['fed']>last:seen.append(a['owner']);last=a['fed']
   return a['fed']==n and all(f['species']=='' for f in state()['food'])
  wait(done,'portions finished',95)
  if owners is not None:require(seen==owners,'consumption order differs from offered ticket order: '+str((seen,owners)))
 def cues():
  for v in clients:
   # A server tick may precede this client's reliable snapshot. Compare the
   # rendered cue with the exact client snapshot, rather than a newer server.
   e=inspect(v);a=e['zoo']['animals'][0];f=next(f for f in e['zoo']['food'] if f['actor']==v.profile)
   if f['offered']:
    expected='waiting' if a['owner']!=v.profile else 'finished' if a['consumed'] else 'eating' if a['phase']==6 else 'approaching'
    require(e['elephantCue']==expected,str((v.profile,expected,e['elephantCue'])))
    require(e['elephantSlot']==f['slot'],'wrong local slot')
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for i,v in enumerate(clients):
   home.ready(v);v.input('resize',x=1280 if i%2==0 else 1024,y=591 if i%2==0 else 768)
   cmd(v,1,value=['blue-pup','orange-pup','muffin','chloe'][i]);enter(v)
  cap(a,'phone-idle');cap(b,'tablet-idle');before=animal()['fed']
  a.input('sandFilm',x=45);tap(a);time.sleep(.3);tap(b)
  # Real touches preserve the automatic walk, reservation and handoff.
  until=time.monotonic()+30;stages=set();tickets={}
  while not all(food(v)['offered'] for v in (a,b)) and time.monotonic()<until:
   for v in (a,b):
    f=food(v);e=inspect(v);stages.add(e['elephantCue'])
    if f['species']:
     require(f['ticket']==tickets.setdefault(v.profile,f['ticket']),'repeated touch replaced ticket')
    tap(v)
   time.sleep(.1)
  require(all(food(v)['offered'] for v in (a,b)),'automatic offer failed')
  cues();cap(a,'phone-offered');cap(b,'tablet-offered')
  wait(lambda:animal()['phase']==5,'approach',35);cues();tap(a);tap(b)
  wait(lambda:animal()['phase']==6 and not animal()['consumed'],'eating',30)
  owner=next(v for v in clients if v.profile==animal()['owner']);cues();cap(a,'phone-eating');cap(b,'tablet-eating');tap(a);tap(b)
  wait(lambda:animal()['consumed'],'confirmed consumption',8);cues()
  cap(a,'phone-finish');cap(b,'tablet-finish')
  counts=[inspect(v)['elephantFinishEvents'] for v in clients]
  require(all(n==1 for n in counts),'completion not seen once by all clients')
  for v in clients:
   for _ in range(3):require(inspect(v)['elephantFinishEvents']==1,'duplicate snapshot replayed finish')
  finished(before+2);require(all(inspect(v)['elephantFinishEvents']==2 for v in clients),'second completion missing')
  require(inspect(a)['elephantCue']=='idle','stale completion cue')
  record('two close offers: real repeat touches, ownership/queue/phase cues, one finish per consumption on four clients')
  before=animal()['fed']
  for v in clients:cmd(v,0,x=1400,y=100)
  for v in clients:tap(v)
  wait(lambda:all(food(v)['offered'] for v in clients),'four full offers',25)
  require(len({food(v)['slot'] for v in clients})==4,'overlapping slots');cues();cap(a,'four-phone');cap(b,'four-tablet')
  # The first reachable offer already has a lease while slower walkers are
  # still carrying. Preserve that lease; remaining offered tickets are FIFO.
  serving=animal()['owner'];order=[serving]+[f['actor'] for f in sorted(state()['food'],key=lambda f:f['ticket']) if f['actor']!=serving]
  for v in clients:tap(v)
  finished(before+4,order)
  record('all four native players complete exactly once in offered ticket order; repeated waiting taps preserve four spots')
  # Four offers; queued departure preserves exact sibling ticket identities.
  before=animal()['fed']
  for v in clients:cmd(v,0,x=1400,y=100)
  for v in clients:tap(v)
  wait(lambda:all(food(v)['offered'] for v in clients),'four offers',25)
  require(len({food(v)['slot'] for v in clients})==4,'overlapping slots');cues()
  queued=next(v for v in clients if v.profile!=animal()['owner']);remaining=[v for v in clients if v!=queued]
  kept={v.profile:food(v)['ticket'] for v in remaining};cmd(queued,7,value='creek')
  require(food(queued)['species']=='','queued departure retained food');require(inspect(queued)['elephantCue']=='','stale departure cue')
  require(all(food(v)['ticket']==kept[v.profile] for v in remaining),'departure reordered queue')
  serving=animal()['owner'];order=[serving]+[f['actor'] for f in sorted((food(v) for v in remaining),key=lambda f:f['ticket']) if f['actor']!=serving]
  finished(before+3,order);enter(queued)
  require(inspect(queued)['elephantCue']=='idle','return retained cue')
  record('four clients/four spots, queued departure and return preserve siblings and clear local cues')
  # Active owner departure and native disconnect/rejoin.
  before=animal()['fed']
  for v in (a,b):cmd(v,0,x=1400,y=100)
  for v in (a,b):tap(v)
  wait(lambda:all(food(v)['offered'] for v in (a,b)) and animal()['owner']!='','active pair',25)
  active=next(v for v in (a,b) if v.profile==animal()['owner']);survivor=b if active==a else a
  cmd(active,7,value='creek');finished(before+1);enter(active)
  events=inspect(active)['elephantFinishEvents'];active.input('network-pause');time.sleep(.6);active.input('network-resume');home.ready(active)
  require(inspect(active)['elephantFinishEvents']==events,'reconnect replayed finish');require(food(active)['species']=='','rejoin duplicated food')
  record('active departure allows sibling completion; network suspension/rejoin does not replay or duplicate')
  # Application lifecycle and independent other-species regression.
  for v in (a,b):v.input('application-pause')
  time.sleep(.3)
  for v in (a,b):v.input('application-resume');home.ready(v)
  cmd(a,0,x=3600,y=100);time.sleep(1.2);a.input('touchButton',text='Take leaves for giraffe')
  wait(lambda:food(a)['offered'],'giraffe offer',25);wait(lambda:state()['animals'][1]['consumed'],'giraffe consumption',40)
  cap(a,'giraffe-regression');wait(lambda:food(a)['species']=='','giraffe finish',10)
  record('pause/resume retains world; giraffe automatic feeding still completes')
  film=a.out/'sand-film';wait(lambda:(film/'times.txt').exists(),'native film complete',50)
  times=[float(s) for s in (film/'times.txt').read_text().splitlines()];frames=sorted(film.glob('*.png'));require(len(frames)==len(times)>30,'film frames')
  concat=out/'frames.txt';concat.write_text('\n'.join(s for i,f in enumerate(frames) for s in ["file '"+f.as_posix()+"'",'duration '+str(times[i+1]-times[i] if i+1<len(times) else .125)]))
  subprocess.run([shutil.which('ffmpeg'),'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/'actual-elephant-silent.mp4')],check=True)
  record('timestamped actual native gameplay recording exported')
  passed=True
 finally:
  if not passed:
   for v in clients:
    if v.process.poll() is None:
     try:cap(v,'failure-'+v.profile)
     except Exception:pass
  run.close();errors=[]
  for v in run.instances:
   log=(v.out/'player.log').read_text(encoding='utf8',errors='replace');errors.extend(s for s in log.splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,actualClients=4,physicalDevices=False,liveFamilyTouched=False))
  print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
