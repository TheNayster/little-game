"""Open normal mouse-enabled players in the already-verified isolated Zoo family."""
import argparse,json,time,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,read,write,require

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('run');args=p.parse_args()
 run=Run(args.build,resume=args.run,interactive=True,review_controls=False,extended_test_lifetime=True,graphics_api='d3d11')
 evidence=read(run.path/'final-live-readback.json');require(evidence and evidence['ready'] and evidence['build']==args.build,'Completed exact-build inspection required')
 checkpoint=run.path/'server-world/world.save';require(checkpoint.is_file(),'Existing isolated world required')
 before=json.loads(checkpoint.read_text(encoding='utf-8').split('\n',2)[2])['zoo']['habitat']['props']
 require(len(before)==2,'Expected existing two-prop arrangement')
 server=run.start('server');require(server.state()['view']['zoo']['habitat']['props']==before,'Reopen changed arrangement')
 clients=[run.start('client',slot['profile']) for slot in run.slots[:2]]
 time.sleep(3)
 require(all(v.status()['status']=='connected' and next(p for p in v.state()['view']['players'] if p['id']==v.profile)['zone']=='zoo-savanna' for v in clients),'Normal review clients not ready')
 # verifyGarden=false omits the synthetic-pointer test component. Physical
 # mouse/pen input stays enabled, as in the production game.
 write(run.path/'owner-play-ready.json',dict(build=args.build,ready=True,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],profileIds=[v.profile for v in clients],propIds=[p['id'] for p in before],graphicsApi='Direct3D11',mouseInputEnabled=True,verificationControls=False,recordedVideo=False,liveFamilyTouched=False))
 print('OWNER PLAY READY '+str(run.path),flush=True)
 try:
  while any(v.process.poll() is None for v in clients):time.sleep(.5)
 finally:run.close()
if __name__=='__main__':main()
