"""Unrecorded live milestone 7 demo; isolated authority; leaves review windows ready."""
import sys,time,argparse,importlib.util
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).resolve().parents[1]/'Verification/Test-HomeWorld.py')
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
 run=Run(args.build,interactive=True,review_controls=True,test_audible=True);clients=[]
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome'));home.ready(v)
 def inspect(v):return v.input('inspect')
 def tap(v,i):
  cmd(v,0,x=1000 if i==0 else 1600,y=100);time.sleep(.8)
  wait(lambda:not inspect(v)['pending'],'demo input ready',8);v.input('touchButton',text=['Rustling leaves','Butterfly flowers'][i])
 try:
  server=run.start('server')
  for slot in run.slots[:2]:
   v=run.start('client',slot['profile']);clients.append(v);home.ready(v);v.input('resize',x=1280,y=591)
   cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
  a,b=clients;cmd(b,1,value='orange-pup')
  print('LIVE: revealing the bird, then the butterflies.',flush=True)
  tap(a,0);time.sleep(5);tap(a,1);time.sleep(6)
  print('LIVE: repeat discoveries while Bingo brushes.',flush=True)
  cmd(b,0,x=1200,y=100);time.sleep(.8);b.input('touchButton',text='Choose elephant brush')
  wait(lambda:inspect(b)['zoo']['animals'][0]['phase']==13,'demo brushing',35)
  for i in range(2):
   tap(a,i)
   animal=inspect(b)['zoo']['animals'][0];x=animal['fromX']-70;y=animal['fromY']+202/.45
   b.input('touch-begin',x=x,y=y,finger=61);b.input('touch-move',x=x+24,y=y,finger=61);b.input('touch-end',x=x+24,y=y,finger=61);time.sleep(5)
  b.input('touchButton',text='Put elephant brush away');cmd(a,0,x=1450,y=100);time.sleep(.8)
  home.capture(a,run.path,'live-owner-ready')
  write(run.path/'live-review-ready.json',dict(build=args.build,runId=run.run_id,serverPid=server.process.pid,clientPids=[v.process.pid for v in clients],ready=True,recordedVideo=False,liveFamilyTouched=False))
  print('READY: elephant surprises available for owner review. '+str(run.path),flush=True)
  while any(v.process.poll() is None for v in clients):time.sleep(.5)
 finally:run.close()
if __name__=='__main__':main()
