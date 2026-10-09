"""Unrecorded two-client fossil demonstration, then leave normal mouse-enabled players open."""
import sys,time,argparse,importlib.util
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).resolve().parents[1]/'Verification/Test-HomeWorld.py');home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--resume');args=p.parse_args();run=Run(args.build,resume=args.resume,interactive=True,review_controls=not args.resume,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/'fossil-review';out.mkdir(exist_ok=True)
 print('LIVE REVIEW '+str(out),flush=True)
 def f():return server.state()['view']['zoo']['fossils']
 def inspect(v):return v.input('inspect')
 def tap(v,name):
  wait(lambda:any(c['name']==name for c in inspect(v)['controls']),'visible demo control '+name,20);v.input('touchButton',text=name);time.sleep(.4);home.ready(v)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome','command rejected'));home.ready(v)
 def enter(v):
  if inspect(v)['zone']!='zoo-dinosaurs':
   cmd(v,7,value='zoo');cmd(v,0,x=1750,y=100);cmd(v,22,value='gate',target='zoo-dinosaurs')
  cmd(v,0,x=470,y=100);time.sleep(.6);tap(v,'Join fossil discovery');time.sleep(1.3)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots[:2]];a,b=clients
  if not args.resume:
   for v in clients:home.ready(v);v.input('resize',x=1280,y=591);enter(v)
   cmd(b,1,value='orange-pup');time.sleep(.6);home.capture(a,out,'two-children-start')
   tap(b,'Take leaves for brachiosaurus');wait(lambda:inspect(b)['brachiosaurusCue']=='eating' and inspect(b)['brachiosaurusMouthGap']<1,'visible supported mouth contact',40);home.capture(a,out,'two-children-brachiosaurus-feeding');wait(lambda:server.state()['view']['zoo']['animals'][4]['fed']==1,'feeding finishes',10);time.sleep(3);enter(b)
   for _ in range(3):tap(a,'Uncover or pick up fossil 0');tap(b,'Uncover or pick up fossil 0')
   tap(a,'Uncover or pick up fossil 0');require(f()['holders'][0]==a.profile,'demo pickup');home.capture(a,out,'cooperative-uncover-holder');time.sleep(1)
   tap(a,'Leave fossil discovery');require(f()['holders'][0]=='' and f()['revealed'][0]==6,'demo safe leave');tap(b,'Uncover or pick up fossil 0');tap(b,'Match fossil picture 0');enter(a)
   for i in [1,2]:
    for _ in range(3):tap(a,'Uncover or pick up fossil '+str(i));tap(b,'Uncover or pick up fossil '+str(i))
    v=a if i==1 else b;tap(v,'Uncover or pick up fossil '+str(i));tap(v,'Match fossil picture '+str(i))
   require(f()['placed']==[True]*3,'demo completion');home.capture(a,out,'two-children-completed');time.sleep(3);tap(a,'Play fossil discovery again');require(f()['placed']==[True]*3,'demo replay safeguard');tap(a,'Confirm fossil replay');require(f()['round']==2 and f()['revealed']==[0]*3,'demo new round');home.capture(a,out,'replay-ready')
   write(out/'demonstration.json',dict(build=args.build,passed=True,steps=['two visible children in corrected normal exhibit','Brachiosaurus lowered-head supported-branch feeding','two visible children uncover together','first pickup, independent leave returns available fossil','second child picks up and places returned piece','both complete picture','deliberate confirmed replay'],recordedVideo=False,liveFamilyTouched=False))
   [v.close() for v in clients];run.review_controls=False;clients=[run.start('client',s['profile']) for s in run.slots[:2]]
  # Normal players omit the test pointer component and retain physical input.
  time.sleep(4)
  require(all(v.status()['status']=='connected' and next(p for p in v.state()['view']['players'] if p['id']==v.profile)['zone']=='zoo-dinosaurs' for v in clients),'normal players not connected at corner')
  for v in clients:require('Direct3D 11' in (v.out/'player.log').read_text(errors='replace'),'review actual API')
  write(run.path/'owner-play-ready.json',dict(build=args.build,ready=True,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],profileIds=[v.profile for v in clients],round=f()['round'],graphicsApi='Direct3D11',mouseInputEnabled=True,verificationControls=False,recordedVideo=False,liveFamilyTouched=False))
  print('OWNER PLAY READY '+str(run.path),flush=True)
  while any(v.process.poll() is None for v in clients):time.sleep(.5)
 finally:run.close()
if __name__=='__main__':main()
