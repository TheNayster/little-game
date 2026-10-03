"""Clinic regressions: choose every waiting animal with full beds; retain room across patient changes and re-entry."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,importlib.util,time
from pathlib import Path
from PIL import Image
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'vet-selection';out.mkdir();clients=[];checks=[];roomChecks=[];passed=False;finger=90
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['vet']
 def patient(i):return state()['patients'][i]
 def cmd(v,a,**kw):require(home.command(v,a,**kw)['accepted'],'command rejected');home.ready(v)
 def tap(v,name):
  wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name)
  v.input('touchButton',text=name);home.ready(v);time.sleep(.12)
 def select(v,i):
  tap(v,'Friends');p=patient(i);tap(v,'Vet welcome '+str(i) if p['bed']==-1 else 'Vet bed '+str(p['bed']))
  wait(lambda:v.input('inspect')['vetSelected']==i,'selected patient '+str(i));require(v.input('inspect')['vetBackdropReady'],'room texture missing')
 def capture(v,name):
  require(v.input('inspect')['vetBackdropReady'],'background texture unloaded')
  home.capture(v,out,name);im=Image.open(out/(name+'.png')).convert('RGB');w,h=im.size
  pixels=list(im.crop((0,int(h*.18),int(w*.07),int(h*.76))).getdata());white=sum(min(p)>245 for p in pixels)/len(pixels)
  require(white<.3,'white background returned: '+str(white));roomChecks.append(dict(capture=name,whiteMarginFraction=white))
 def stroke(v,i,tool,n,field):
  nonlocal finger;finger+=1;tap(v,'Vet tool '+tool)
  cx=.58 if i==6 else .55 if i>=4 else .47;cy=.35 if i==6 else .25 if i>=4 else .42
  x=cx+(-.12 if n==0 else .12 if n==1 else 0);y=cy+(.06 if n==0 else .03 if n==1 else -.12)
  before=patient(i)[field][n] if isinstance(patient(i)[field],list) else patient(i)[field]
  v.input('touch-begin',role='vet',x=x,y=y,finger=finger);v.input('touch-end',role='vet',x=x,y=y,finger=finger)
  wait(lambda:(patient(i)[field][n] if isinstance(patient(i)[field],list) else patient(i)[field])>before,'real waiting-patient care')
  wait(lambda:v.input('inspect')['vetQueued']==0 and not v.input('inspect')['pending'],'gesture reply settled')
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1024 if i==2 else 1280,y=768 if i==2 else 591);cmd(v,7,value='daycare')
  tap(a,'Games');tap(a,'Animal care clinic');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'four shared helpers');time.sleep(2.3)
  beds=[p['bed'] for p in state()['patients']];cast=state()['friends'];require(sum(b>=0 for b in beds)==4,'initial beds')
  capture(a,'initial-puppy-phone')
  for v,i in zip(clients,[2,3,6,7]):
   select(v,i);tool='wash' if patient(i)['needs']&1 else 'brush';field='washed' if tool=='wash' else 'brushed';stroke(v,i,tool,0,field)
  require([p['bed'] for p in state()['patients']]==beds,'waiting treatment displaced another patient')
  capture(c,'waiting-brachiosaurus-tablet');capture(d,'waiting-parasaurolophus-phone')
  record('all four previously blocked waiting pets/dinosaurs are selectable and accept real care with all beds occupied; four helpers retain independent selections and original patients')
  select(b,2);stroke(b,2,'brush',0,'brushed');require(patient(2)['brushed'][0]==2,'two helpers do not share waiting care')
  # Complete a waiting rabbit without clearing a bed first.
  select(a,2)
  for n in range(3):
   while patient(2)['brushed'][n]<3:stroke(a,2,'brush',n,'brushed')
  stroke(a,2,'bandage',2,'bandaged')
  while patient(2)['cuddles']<3:stroke(a,2,'cuddle',2,'cuddles')
  capture(a,'comfortable-waiting-rabbit-phone');tap(a,'Send friend home');wait(lambda:patient(2)['bed']==-2,'waiting friend goes home')
  require(sum(p['bed']>=0 for p in state()['patients'])==4,'waiting completion changed care beds')
  record('two players share waiting care; brush, bandage, cuddles and All better complete a waiting rabbit without evicting any of four bed patients')
  for i in [0,1,0,4,1,5,0,1]:select(a,i)
  capture(a,'kitten-after-switches-phone');capture(c,'dinosaur-after-switches-tablet')
  for n in range(3):
   tap(a,'Return to Daycare');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'independent departure')
   require(patient(3)['washed'][0]==1 and state()['friends']==cast,'exit reset progress or cast')
   cmd(a,7,value='garden');cmd(a,7,value='daycare');cmd(a,34,value='start');wait(lambda:sum(m['attending'] for m in state()['members'])==4,'explicit return')
   select(a,0);select(a,1);capture(a,'kitten-after-return-'+str(n))
  a.input('application-pause');a.input('application-resume');home.ready(a);capture(a,'room-after-resume-phone')
  record('dog/cat/dinosaur switching, three leave-and-return cycles and pause/resume retain a colored room; independent exits preserve siblings, waiting care and NPC cast')
  write(out/'room-checks.json',roomChecks);passed=True
 finally:
  if not passed:
   for i,v in enumerate(clients):
    if v.process.poll() is None:
     try:home.capture(v,out,'failure-'+str(i))
     except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,passed=passed,checks=checks,exitCodes=[v.process.returncode for v in run.instances],scope='one disposable authority and four native Windows release clients at phone/tablet dimensions; actual patient buttons and care touches; no physical device/live-server install'))
 print('ALL PASS',flush=True)

if __name__=='__main__':main()
