"""Picture navigation acceptance in isolated native Unity release players."""
import sys, argparse, importlib.util, time, subprocess, shutil
from pathlib import Path
sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
TRAILS=['zoo-savanna','zoo-dinosaurs','zoo-reptiles','zoo-aquarium']
GROUPS=[['elephant','giraffe','zebra','lion'],['brachiosaurus','triceratops','stegosaurus','tyrannosaurus'],['tortoise','gecko','iguana','crocodile'],['clownfish','blue-tang','zebra-shark','penguin']]

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--baseline',action='store_true');p.add_argument('--layout-only',action='store_true');args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'navigation';out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def cap(v,name):return home.capture(v,out,name)
 def inspect(v):return v.input('inspect')
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def enter(v,area):
  if inspect(v)['zone'] in TRAILS:
   cmd(v,0,x=200,y=100);cmd(v,22,value='gate',target='zoo')
  elif inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
  cmd(v,0,x=650 if TRAILS.index(area)%2==0 else 1750,y=100);time.sleep(.4)
  v.input('touchButton',text='Visit the savanna' if area==TRAILS[0] else 'Visit '+['','Dinosaur Valley','Reptile Garden','Aquarium'][TRAILS.index(area)])
  wait(lambda:inspect(v)['zone']==area,'trail entrance '+area,12);home.ready(v)
 def idle(v):return wait(lambda:not inspect(v)['zooNavigationBusy'],'navigation settled',30)
 def portion(v):return next(f for f in server.state()['view']['zoo']['food'] if f['actor']==v.profile)
 def elephant():return server.state()['view']['zoo']['animals'][0]
 def map_marker(v,expected):
  s=inspect(v);require(s['zooCurrentExhibit']==expected,'wrong map/current exhibit '+str(s['zooCurrentExhibit']))
  require(s['zooMapShownTrail']==s['zone'],'map expanded wrong trail')
  require(s['zooMapMarker']==expected,'wrong visible map marker')
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:home.ready(v);cmd(v,7,value='zoo')
  for w,h,label in [(1280,591,'phone'),(1024,768,'tablet')]:
   a.input('resize',x=w,y=h);time.sleep(.6);cap(a,label+'-entrance')
   cmd(a,0,x=650,y=100);cmd(a,22,value='gate',target=TRAILS[0]);cmd(a,0,x=1200,y=100);time.sleep(1);cap(a,label+'-elephant')
   cmd(a,0,x=200,y=100);cmd(a,22,value='gate',target='zoo')
  if args.baseline:passed=True;return
  for w,h,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
   a.input('resize',x=w,y=h);enter(a,TRAILS[0]);cmd(a,0,x=1200,y=100);time.sleep(1);cap(a,label+'-exhibit')
   a.input('touchButton',text='Zoo map');require(inspect(a)['zooMapOpen'],'map did not open');map_marker(a,'elephant');cap(a,label+'-map')
   a.input('touchButton',text='Show Reptile Garden trail');require(inspect(a)['zooMapMarker']==TRAILS[0] and inspect(a)['zone']==TRAILS[0],'browsing a map trail navigated player or hid actual location')
   a.input('touchButton',text='Show Savanna trail');map_marker(a,'elephant')
   a.input('touchButton',text='Take leaves for elephant');require(portion(a)['species']=='','map let a feeding touch pass through')
   a.input('touch-begin',role='screen',x=w*.27,y=h*.91,finger=41);a.input('touch-end',role='screen',x=w*.27,y=h*.91,finger=41)
   require(inspect(a)['zooMapOpen'] and inspect(a)['zone']==TRAILS[0],'map let entrance navigation pass through')
   a.input('escape');require(not inspect(a)['zooMapOpen'],'Escape failed')
   a.input('button',text='Zoo map');a.input('button',text='Close Zoo map');require(not inspect(a)['zooMapOpen'],'mouse close failed')
   controls=inspect(a)['controls'];write(out/(label+'-controls.json'),controls)
   for control in controls:
    if control['name'] in ('Zoo previous animal','Zoo next animal','Zoo entrance','Zoo map'):
     r=control['bounds'];require(r['width']>=44 and r['height']>=44,'small touch target '+control['name'])
     for other in controls:
      if other['name'] in ('Menu','Tap to walk','Joystick','Characters'):
       q=other['bounds'];require(min(r['x']+r['width'],q['x']+q['width'])<=max(r['x'],q['x']) or min(r['y']+r['height'],q['y']+q['height'])<=max(r['y'],q['y']),'navigation overlaps '+other['name'])
  record('phone/tablet/640x400 map and exhibit captures; modal shields feeding/navigation; real Escape and mouse close')
  if args.layout_only:
   a.input('fixtureTravel',text='creek');home.ready(a);require(not inspect(a)['zooMapOpen'],'map survived world departure')
   if not inspect(a)['joystickVisible']:a.input('touchButton',text='Tap to walk')
   require(inspect(a)['joystickVisible'],'outside-Zoo joystick missing')
   a.input('touchButton',text='Menu');require(not inspect(a)['joystickVisible'],'outside-Zoo menu lost focus')
   a.input('touchButton',text='Back to play');require(inspect(a)['joystickVisible'],'outside-Zoo focus did not restore')
   record('Zoo departure and outside-Zoo joystick/menu input focus remain valid')
   passed=True;return
  a.input('resize',x=1280,y=591)
  for t,area in enumerate(TRAILS):
   enter(a,area);cmd(a,0,x=1200,y=100);time.sleep(.8)
   for i,id in enumerate(GROUPS[t]):
    s=inspect(a);require(s['zooCurrentExhibit']==id,'incorrect current animal '+id)
    previous=GROUPS[t][i-1] if i else GROUPS[(t-1)%4][3]
    following=GROUPS[t][i+1] if i<3 else GROUPS[(t+1)%4][0]
    require((s['zooPreviousDestination'],s['zooNextDestination'])==(previous,following),'wrong destinations '+id)
    a.input('touchButton',text='Zoo map');map_marker(a,id);cap(a,'map-'+id);a.input('touchButton',text='Close Zoo map')
    if i<3:
     a.input('touchButton',text='Zoo next animal')
     for _ in range(3):a.input('touchButton',text='Zoo next animal')
     idle(a);require(inspect(a)['zooCurrentExhibit']==GROUPS[t][i+1],'rapid next skipped/stacked stops')
     a.input('touchButton',text='Zoo previous animal');idle(a);require(inspect(a)['zooCurrentExhibit']==id,'previous stop failed')
     a.input('touchButton',text='Zoo next animal');idle(a)
   cmd(a,0,x=9200,y=100);time.sleep(.6);a.input('touchButton',text='Zoo next animal');idle(a)
   require(inspect(a)['zone']==TRAILS[(t+1)%4],'forward trail boundary failed')
   cmd(a,0,x=200,y=100);time.sleep(.6);a.input('touchButton',text='Zoo previous animal');idle(a)
   require(inspect(a)['zone']==area and inspect(a)['zooCurrentExhibit']==GROUPS[t][3],'reverse trail boundary failed')
   cmd(a,0,x=200,y=100);time.sleep(.6);a.input('touchButton',text='Zoo entrance');idle(a);require(inspect(a)['zone']=='zoo','entrance return failed')
   record('four stops, next/previous walking, both boundaries and entrance return: '+area)
  record('all four entrance pictures; all 16 current/destination portraits and map locations; 24 intra-trail forward/back routes; eight boundary routes; four entrance returns; repeated next touches')
  # Four real clients share the elephant. A local map must retain all tickets,
  # and a local gate departure must release only that player's lease.
  for v in clients:enter(v,TRAILS[0]);cmd(v,0,x=1400,y=100)
  before=elephant()['fed']
  for v in clients:v.input('touchButton',text='Take leaves for elephant')
  wait(lambda:all(portion(v)['offered'] for v in clients),'four elephant offers',30)
  tickets={v.profile:portion(v)['ticket'] for v in clients};age=elephant()['age'];a.input('touchButton',text='Zoo map');time.sleep(.8)
  require(all(portion(v)['ticket']==tickets[v.profile] for v in clients),'map canceled a food ticket');cap(a,'independent-map');cap(b,'sibling-feeding-with-map-open')
  require(elephant()['age']!=age,'map paused shared animal')
  a.input('touchButton',text='Close Zoo map')
  queued=next(v for v in clients if v.profile!=elephant()['owner']);others=[v for v in clients if v!=queued]
  queued.input('touchButton',text='Zoo entrance');idle(queued)
  require(inspect(queued)['zone']=='zoo' and portion(queued)['species']=='','queued navigation did not cancel own offer')
  require(all(inspect(v)['zone']==TRAILS[0] for v in others),'navigation moved siblings')
  active=next((v for v in others if v.profile==elephant()['owner']),None)
  require(active is not None,'no active owner for departure check')
  remaining=[v for v in others if v!=active];kept={v.profile:portion(v)['ticket'] for v in remaining}
  active.input('touchButton',text='Zoo entrance');idle(active)
  require(portion(active)['species']=='','active navigation retained offer')
  require(all(portion(v)['ticket']==kept[v.profile] for v in remaining),'navigation changed sibling tickets')
  wait(lambda:all(portion(v)['species']=='' for v in remaining),'remaining elephant offers finish',85)
  require(elephant()['fed']>=before+2,'siblings did not feed end to end')
  cap(queued,'independent-entrance');cap(remaining[0],'sibling-finish')
  record('four actual shared clients: map preserves tickets; queued/active local entrance departures; siblings retain locations/tickets and finish elephant feeding')
  a.input('network-pause');time.sleep(.5);a.input('network-resume');home.ready(a)
  s=inspect(a);require(s['zooCurrentExhibit']=='entrance' if s['zone']=='zoo' else s['zooCurrentExhibit'] in sum(GROUPS,[]),'invalid reconnect navigation')
  record('native network suspension/rejoin derives valid local navigation from authoritative player state')
  if inspect(a)['zone'] in TRAILS:cmd(a,0,x=200,y=100);cmd(a,22,value='gate',target='zoo')
  a.input('sandFilm',x=45);time.sleep(.6);a.input('touchButton',text='Visit the savanna')
  wait(lambda:inspect(a)['zone']==TRAILS[0],'recorded real entrance walk',15);home.ready(a);time.sleep(1)
  a.input('touchButton',text='Zoo next animal');idle(a);time.sleep(.6)
  a.input('touchButton',text='Zoo map');time.sleep(2);a.input('touchButton',text='Close Zoo map');a.input('touchButton',text='Zoo entrance');idle(a);time.sleep(1)
  film=a.out/'sand-film';wait(lambda:(film/'times.txt').exists(),'film complete',55)
  times=[float(s) for s in (film/'times.txt').read_text().splitlines()];frames=sorted(film.glob('*.png'));require(len(frames)==len(times)>30,'film frames missing')
  concat=out/'frames.txt';concat.write_text('\n'.join(s for i,f in enumerate(frames) for s in ["file '"+f.as_posix()+"'",'duration '+str(times[i+1]-times[i] if i+1<len(times) else .125)]))
  subprocess.run([shutil.which('ffmpeg'),'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/'navigation-silent.mp4')],check=True)
  record('actual timestamped entrance → trail → next animal → map → entrance recording')
  passed=True
 finally:
  if not passed:
   for v in locals().get('clients',[]):
    if v.process.poll() is None:
     try:cap(v,'failure-'+v.profile)
     except Exception:pass
  run.close();errors=[]
  for v in run.instances:
   errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,actualClients=4,baseline=args.baseline,layoutOnly=args.layout_only,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
