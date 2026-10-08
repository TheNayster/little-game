"""Milestone 7: isolated native clients, real mouse/touch dispatch, no video."""
import sys,time,argparse,importlib.util
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('care',Path(__file__).with_name('Test-ElephantCare.py'))
care=importlib.util.module_from_spec(spec);spec.loader.exec_module(care);home=care.home

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--regression-only',action='store_true');args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True,motion_conditions=dict(testAudible=True));out=run.path/'elephant-surprises';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome'));home.ready(v);return r
 def inspect(v):return v.input('inspect')
 def frame(v,i):
  for _ in range(8):
   s=inspect(v);control=next((c for c in s['controls'] if c['name']==['Rustling leaves','Butterfly flowers'][i]),None)
   if control and control['bounds']['width']>=80:return
   w,h=s['screenWidth'],s['screenHeight'];direction=1 if (480 if i==0 else 1920)<s['cameraX'] else -1
   v.input('touch-begin',role='screen',x=w*.5,y=h*.24,finger=64)
   v.input('touch-move',role='screen',x=w*(.5+.3*direction),y=h*.24,finger=64)
   v.input('touch-end',role='screen',x=w*(.5+.3*direction),y=h*.24,finger=64);time.sleep(.3)
  raise AssertionError('prop could not be reached by ordinary camera pan')
 def z():return server.state()['view']['zoo']
 def enter(v):
  if inspect(v)['zone']=='zoo-savanna':
   cmd(v,0,x=1200,y=100);time.sleep(.4);return
  cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100);time.sleep(.4)
 def tap(v,i,mouse=False):
  if not inspect(v)['zooMapOpen']:
   frame(v,i)
  wait(lambda:not inspect(v)['pending'],'tap ready',8);v.input('button' if mouse else 'touchButton',text=['Rustling leaves','Butterfly flowers'][i])
 def idle(i):wait(lambda:z()['surpriseAge'][i]>=8,'prop reset',12)
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:home.ready(v);v.input('resize',x=1280,y=591);enter(v)
  if args.regression_only:
   cmd(a,0,x=4000,y=100);time.sleep(1)
   a.input('touchButton',text='Take leaves for giraffe');wait(lambda:z()['animals'][1]['consumed'],'giraffe regression',65)
   home.capture(a,out,'giraffe-regression');record('Unchanged giraffe shared feeding passes on four actual clients');passed=True;return
  initial=inspect(a);objects=initial['surpriseObjects']
  for i in range(2):
   before=z()['surpriseSequence'][i]
   with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(lambda v:tap(v,i),clients))
   wait(lambda:z()['surpriseSequence'][i]==before+1,'one coalesced event',5)
   for v in clients:wait(lambda:inspect(v)['zoo']['surpriseSequence'][i]==before+1,'same shared event',5)
   age=z()['surpriseAge'][i];seq=z()['surpriseSequence'][i]
   for _ in range(3):tap(a,i)
   require(z()['surpriseSequence'][i]==seq and z()['surpriseAge'][i]>=age,'active taps restart/duplicate')
   home.capture(a,out,'bird' if i==0 else 'butterflies')
  require(z()['surpriseSequence']==[1,1],'independent sequences')
  record('Four actual players tap each prop; exactly one event each, same authoritative variation, independent running and no restart')
  audioSources=inspect(a)['audioSources'];require(0<inspect(a)['surpriseSounds']<=2,'sound flood or no accepted event sound')
  idle(0);idle(1);tap(a,0,True);tap(b,1)
  sounds=inspect(a)['surpriseSounds'];seq=z()['surpriseSequence'][:]
  a.input('network-pause');time.sleep(.25);a.input('network-resume');home.ready(a)
  require(inspect(a)['surpriseSounds']==sounds,'reconnect replayed sound')
  cmd(b,7,value='creek');require(z()['surpriseSequence']==seq,'departure cleared event')
  prior=inspect(b)['surpriseSounds'];enter(b);require(inspect(b)['surpriseSounds']==prior,'return replayed visitor sound')
  d.close();d=run.start('client',run.slots[3]['profile']);clients[3]=d;home.ready(d);enter(d)
  require(inspect(d)['surpriseSounds']==0,'late join replayed history')
  record('Mouse and touch replay, departure/return, native reconnect and late joining preserve valid transient state without stale sound')
  idle(0);idle(1);before=z()['surpriseSequence'][:]
  for i in range(2):
   frame(a,i)
   a.input('touchButton',text='Zoo map');require(inspect(a)['zooMapOpen'],'map not open')
   tap(a,i);require(z()['surpriseSequence']==before,'map passed taps through')
   a.input('touchButton',text='Close Zoo map');require(z()['surpriseSequence']==before,'map close triggered a prop')
  cmd(a,0,x=1200,y=100);time.sleep(.8)
  a.input('touchButton',text='Choose elephant brush');wait(lambda:z()['animals'][0]['phase']==13,'care side',35)
  for i in range(2):tap(b,i)
  aa=z()['animals'][0];x=aa['fromX']-70;y=aa['fromY']+202/.45
  a.input('touch-begin',x=x,y=y,finger=61);a.input('touch-end',x=x,y=y,finger=61)
  wait(lambda:z()['careProgress'][0]==1,'care alongside surprise',5)
  wait(lambda:not inspect(c)['pending'],'water ready',8);c.input('touchButton',text='Splash button')
  wait(lambda:z()['waterSequence']>0,'water alongside surprise',5)
  wait(lambda:not inspect(d)['pending'],'feeding ready',8);d.input('touchButton',text='Take leaves for elephant')
  wait(lambda:z()['animals'][0]['consumed'],'feeding priority unchanged',65)
  wait(lambda:z()['animals'][0]['phase']==13,'care resumes after feed',25)
  record('Map shields habitat; brushing, water and actual tap-to-feed work alongside surprises; food still preempts and care resumes')
  for w,h,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
   a.input('resize',x=w,y=h);cmd(a,0,x=1200,y=100);time.sleep(.7)
   evidence=home.capture(a,out,label)
   for i,name in enumerate(['Rustling leaves','Butterfly flowers']):
    frame(a,i);evidence=home.capture(a,out,label+'-'+str(i))
    control=next(c for c in evidence['controls'] if c['name']==name)
    require(control['bounds']['width']>=44 and control['bounds']['height']>=44,'small target '+name)
  a.input('resize',x=1280,y=591)
  for i in range(2):idle(i);tap(a,i)
  time.sleep(6.4);require(inspect(a)['surpriseObjects']==objects and inspect(a)['audioSources']==audioSources,'effect objects/audio sources leaked')
  record('Phone/tablet/small-phone native simulated layouts, forgiving targets, repeated play with constant object count')
  a.input('touchButton',text='Put elephant brush away');cmd(a,7,value='creek');enter(a);cmd(a,0,x=4000,y=100);time.sleep(.8)
  a.input('touchButton',text='Take leaves for giraffe');wait(lambda:z()['animals'][1]['consumed'],'giraffe regression',65)
  home.capture(a,out,'giraffe-regression');record('Unchanged giraffe shared feeding passes')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,actualClients=4,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
