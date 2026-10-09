"""Prepare two normal-input D3D11 clients for unrecorded Savanna review."""
import sys,time,argparse,importlib.util
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).resolve().parents[1]/'Verification/Test-HomeWorld.py');home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--resume');args=p.parse_args()
 run=Run(args.build,resume=args.resume,interactive=True,review_controls=not args.resume,extended_test_lifetime=True,graphics_api='d3d11')
 print('REVIEW '+str(run.path),flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots[:2]]
  if not args.resume:
   for i,v in enumerate(clients):
    for action,kw in [(1,dict(value='blue-pup' if i==0 else 'orange-pup')),(7,dict(value='zoo')),(0,dict(x=650,y=100)),(22,dict(value='gate',target='zoo-savanna')),(0,dict(x=3550+i*100,y=100))]:
     r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
    v.close()
   run.review_controls=False;clients=[run.start('client',s['profile']) for s in run.slots[:2]]
  time.sleep(4)
  require(all(v.status()['status']=='connected' and next(p for p in v.state()['view']['players'] if p['id']==v.profile)['zone']=='zoo-savanna' for v in clients),'review not ready')
  for v in clients:require('Direct3D 11' in (v.out/'player.log').read_text(errors='replace'),'actual API')
  write(run.path/'owner-play-ready.json',dict(build=args.build,ready=True,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],clientOutputs=[str(v.out) for v in clients],graphicsApi='Direct3D11',mouseInputEnabled=True,verificationControls=False,recordedVideo=False,liveFamilyTouched=False))
  print('OWNER PLAY READY '+str(run.path),flush=True)
  while any(v.process.poll() is None for v in clients):time.sleep(.5)
 finally:run.close()
if __name__=='__main__':main()
