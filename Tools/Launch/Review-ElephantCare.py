"""Live, unrecorded elephant review on a disposable local family; never the installed server."""
import sys,argparse,importlib.util,time
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('care',Path(__file__).resolve().parents[1]/'Verification/Test-ElephantCare.py')
care=importlib.util.module_from_spec(spec);spec.loader.exec_module(care)
home=care.home

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
 run=Run(args.build,interactive=True,review_controls=True);clients=[];greeting=False
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome','rejected command'));home.ready(v)
 def inspect(v):return v.input('inspect')
 def brush(v,patch):
  wait(lambda:not inspect(v)['pending'],'touch ready',8)
  dx,dy=[(-70,202),(22,234),(105,160)][patch];a=inspect(v)['zoo']['animals'][0];x,y=a['fromX']+dx,a['fromY']+dy/.45
  v.input('touch-begin',x=x,y=y,finger=61);v.input('touch-move',x=x+24,y=y,finger=61);v.input('touch-end',x=x+24,y=y,finger=61);time.sleep(.7)
 try:
  server=run.start('server')
  for slot in run.slots[:2]:
   v=run.start('client',slot['profile']);clients.append(v);home.ready(v);v.input('resize',x=1280,y=591)
   cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
   if len(clients)==1:greeting=inspect(v)["zoo"]["animals"][0]["phase"]==7
  a,b=clients
  cmd(b,1,value="orange-pup");cmd(b,0,x=1300,y=100)
  time.sleep(1);a.input('touchButton',text='Choose elephant brush');wait(lambda:inspect(a)['zoo']['animals'][0]['phase']==13,'care side',30);time.sleep(2)
  for _ in range(4):brush(a,0)
  b.input('touchButton',text='Choose elephant brush');wait(lambda:len(inspect(a)['zoo']['careMembers'])==2,'second helper',5);time.sleep(1);brush(b,1)
  print('LIVE: shared brushing shown; offering food now.',flush=True)
  wait(lambda:not inspect(a)['pending'],'feeding ready',8);a.input('touchButton',text='Take leaves for elephant')
  wait(lambda:inspect(a)['zoo']['animals'][0]['consumed'],'food served',55)
  wait(lambda:inspect(a)['zoo']['animals'][0]['phase']==13,'care safely resumed',15)
  for patch in (1,2):
   while inspect(a)['zoo']['careProgress'][patch]<4:brush(a,patch)
  wait(lambda:inspect(a)['zoo']['careComplete'],'care complete',5);time.sleep(3)
  for v in clients:
   wait(lambda:not inspect(v)['pending'],'put away ready',8);v.input('touchButton',text='Put elephant brush away');time.sleep(.4)
  wait(lambda:not server.state()['view']['zoo']['careMembers'] and server.state()['view']['zoo']['animals'][0]['phase']<7,'normal schedule',5)
  write(run.path/'live-review-ready.json',dict(build=args.build,runId=run.run_id,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],ready=True,greetingObserved=greeting,recordedVideo=False,liveFamilyTouched=False))
  print('READY: elephant exhibit open for owner review. '+str(run.path),flush=True)
  while any(v.process.poll() is None for v in clients):time.sleep(.5)
 finally:run.close()
if __name__=='__main__':main()
