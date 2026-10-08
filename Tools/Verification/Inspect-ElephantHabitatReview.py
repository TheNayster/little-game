"""Inspect an already-ready, task-owned native review through its existing controls."""
import argparse,json,time,importlib.util
from pathlib import Path
from types import SimpleNamespace
import sys
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,Instance,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('run');args=p.parse_args()
 run=Run(args.build,resume=args.run,interactive=True,review_controls=True,extended_test_lifetime=True,graphics_api='d3d11')
 ready=read(run.path/'live-review-ready.json');require(ready and ready['ready'] and ready['build']==args.build,'Exact ready review required')
 clients=[]
 for folder in run.path.iterdir():
  if not folder.is_dir():continue
  status=read(folder/'status.json');evidence=read(folder/'garden-evidence.json')
  if not status or status['pid'] not in ready['clientPids']:continue
  config=read(run.path/(folder.name+'.config.json'));require(config and config['runId']==args.run and config['verifyGarden'],'Exact owned verification client required');evidence=evidence or dict(actor=config['profile'],serial=0)
  v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=evidence['actor'];v.out=folder;v.identity=folder.name;v.process=SimpleNamespace(pid=status['pid']);v.serial=max(evidence['serial'],(read(folder/'control.json') or {}).get('serial',0))+1000
  v.garden_serial=max(evidence['serial'],(read(folder/'garden-control.json') or {}).get('serial',0));home.ready(v);clients.append(v)
 require(len(clients)==2,'Exactly the two owned review clients required')
 out=run.path/'elephant-habitat'
 def inspect(v):return v.input('inspect')
 def tap(v,name):wait(lambda:not inspect(v)['pending'],'ready input',8);v.input('touchButton',text=name);time.sleep(.15)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 for v in clients:
  if inspect(v)['elephantHabitatOpen']:tap(v,'Cancel habitat preview')
 a=clients[0];before=inspect(a)['zoo']['habitat']['props']
 post=next(p for p in before if p['kind']==1);owner=next(v for v in clients if v.profile==post['creator'])
 cmd(owner,0,x=550,y=100);time.sleep(.8);tap(owner,'Decorate elephant habitat');index=next(i for i,p in enumerate(inspect(owner)['zoo']['habitat']['props']) if p['id']==post['id']);tap(owner,'Move habitat prop '+str(index))
 wait(lambda:not inspect(owner)['zoo']['habitat']['usingId'],'short safe release',30);tap(owner,'Habitat slot '+str(post['slot']));tap(owner,'Confirm habitat placement')
 # Re-notice the existing post, preserving stable ID, creator and position.
 def post_use():
  s=inspect(owner);h=s['zoo']['habitat'];return s if h['usingId']==post['id'] and s['zoo']['animals'][0]['phase']==19 else None
 sample=wait(post_use,'visible post rubbing',30);home.capture(owner,out,'final-post-rub');time.sleep(.65);home.capture(owner,out,'final-post-rub-later')
 original=[(p['id'],p['creator'],p['kind'],p['slot']) for p in before]
 for width,height,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
  owner.input('resize',x=width,y=height);cmd(owner,0,x=550,y=100);time.sleep(.7);tap(owner,'Decorate elephant habitat');tap(owner,'Move habitat prop '+str(index));tap(owner,'Habitat slot '+str(post['slot']));time.sleep(.3)
  home.capture(owner,out,'final-preview-'+label);tap(owner,'Cancel habitat preview')
  require([(p['id'],p['creator'],p['kind'],p['slot']) for p in inspect(owner)['zoo']['habitat']['props']]==original,'local preview changed arrangement')
 for v in clients:v.input('resize',x=1280,y=591);cmd(v,0,x=1200,y=100)
 time.sleep(.8);home.capture(owner,out,'final-owner-ready')
 samples=[inspect(v) for v in clients]
 require(all(s['ready'] and not s['pending'] and not s['elephantHabitatOpen'] and s['zone']=='zoo-savanna' for s in samples),'Review not ready')
 write(run.path/'final-live-readback.json',dict(build=args.build,ready=True,graphicsApi='Direct3D11',clientPids=ready['clientPids'],propIds=[p['id'] for p in before],checks=['Actual post rubbing visible','Local previews/occupied shapes/place-check on three native layouts','Preview cancellation preserves placement','Both clients ready with no pending command'],recordedVideo=False,liveFamilyTouched=False))
 print('FINAL READY '+str(run.path),flush=True)
if __name__=='__main__':main()
