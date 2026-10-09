"""Brachiosaurus presentation regression: actual release clients, queue and pictured socket."""
import sys,time,argparse,importlib.util
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--elephant-only',action='store_true');args=p.parse_args();run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/'brachiosaurus-layout';out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def inspect(v):return v.input('inspect')
 def z():return server.state()['view']['zoo']
 def animal():return z()['animals'][4]
 def food(v):return next(f for f in z()['food'] if f['actor']==v.profile)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome','command failed'));home.ready(v)
 def tap(v,name):v.input('touchButton',text=name);time.sleep(.17)
 def rec(s):checks.append(s);print('PASS '+s,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:
   home.ready(v);v.input('resize',x=1280,y=591);cmd(v,7,value='zoo');cmd(v,0,x=1750,y=100);cmd(v,22,value='gate',target='zoo-dinosaurs');cmd(v,0,x=470,y=100);time.sleep(.4)
  if not args.elephant_only:
   cmd(b,1,value='orange-pup');home.capture(a,out,'normal-phone')
   for w,h,label in [(1280,591,'phone'),(1024,768,'tablet'),(640,400,'small-phone')]:
    a.input('resize',x=w,y=h);time.sleep(.6);home.capture(a,out,'normal-'+label)
   a.input('resize',x=1280,y=591);time.sleep(.5)
   for v in clients:tap(v,'Take leaves for brachiosaurus')
   wait(lambda:all(food(v)['species']=='brachiosaurus' for v in clients),'four browse queue leases',15);require(len({food(v)['slot'] for v in clients})==4,'offering slots merged')
   tickets={v.profile:food(v)['ticket'] for v in clients}
   for v in clients:tap(v,'Take leaves for brachiosaurus');tap(v,'Take leaves for brachiosaurus')
   require({v.profile:food(v)['ticket'] for v in clients}==tickets,'repeat taps replaced tickets');rec('Four preserved authoritative offering slots, ownership pictures and repeated food taps retain one ticket per player')
   wait(lambda:animal()['phase']==6 and animal()['age']>.6 and not animal()['consumed'],'lowered-head browse contact',30)
   active=next(v for v in clients if v.profile==animal()['owner']);wait(lambda:(st if (st:=inspect(active))['brachiosaurusCue']=='eating' and st['brachiosaurusMouthGap']<1 else None),'supported branch/food mouth contact',3);home.capture(active,out,'actual-lowered-head-feeding-phone');rec('Actual feeding phase uses articulated original neck; branch and leaves reach the calibrated mouth socket')
   kept={v.profile:food(v)['ticket'] for v in clients if v!=active};cmd(active,7,value='creek');require(food(active)['species']=='' and all(food(v)['ticket']==kept[v.profile] for v in clients if v!=active),'independent exit lost queue');wait(lambda:all(food(v)['species']=='' for v in clients if v!=active),'siblings finish browse queue',85);require(animal()['fed']==3,'remaining portions did not consume once');rec('Active departure releases only its portion; three siblings finish normally')
  # Show final pilot at rest and compare the approved elephant on the same build.
  for v in clients:
   if inspect(v)['zone'].startswith('zoo-'):
    cmd(v,0,x=200,y=100);cmd(v,22,value='gate',target='zoo')
   elif inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
   cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
  home.capture(a,out,'approved-elephant-same-build');before=z()['animals'][0]['fed'];tap(a,'Take leaves for elephant');wait(lambda:z()['animals'][0]['fed']==before+1,'approved elephant consumption',55);rec('Approved elephant low-tray feeding still completes on identical release')
  passed=True
 finally:
  run.close();errors=[];apis=[]
  for v in run.instances:
   lines=(v.out/'player.log').read_text(errors='replace').splitlines();errors.extend(l for l in lines if 'Exception:' in l or 'error CS' in l)
   if v.role=='client':require(any('Direct3D 11' in l for l in lines),'actual graphics API');apis.append(dict(instance=v.identity,api='Direct3D 11',exit=v.process.returncode))
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,clients=apis,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
