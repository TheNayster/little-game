"""Fossil pilot private release UI, actual checkpoint reopening and shared separation."""
import sys,time,argparse,importlib.util,uuid,subprocess,json,hashlib,secrets
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,Instance,wait,require,write,ROOT
from family_pairing import create_family,write_record
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args();run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');authority,players,_=create_family();slot=players[0]
 run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'fossils';out.mkdir();passed=False;checks=[]
 print('PRIVATE EVIDENCE '+str(out),flush=True)
 def private():
  v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=slot['profile'];v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir();pair=run.path/(v.identity+'.pairing');write_record(pair,slot)
  cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True)
  config=run.path/(v.identity+'.config.json');write(config,cfg);startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
  v.process=subprocess.Popen([str(run.folder/'Client/LittleWeepsNetwork.exe'),'-force-d3d11','-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591'],startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW);run.instances.append(v);wait(lambda:v.status(),'private startup',30);home.ready(v);return v
 def inspect():return a.input('inspect')
 def f():return inspect()['zoo']['fossils']
 def tap(name):
  wait(lambda:any(c['name']==name for c in inspect()['controls']),'visible private control '+name,20);a.input('touchButton',text=name);time.sleep(.3);home.ready(a)
 def rec(s):checks.append(s);print('PASS '+s,flush=True)
 def close_save():
  a.input('application-pause');save=Path(inspect()['savePath']);require(save.exists() and run.run_id in str(save),'private save missing or outside scratch');a.close();return save
 def modify(save,fn):
  header,digest,payload=save.read_text().split('\n',2);data=json.loads(payload);fn(data);payload=json.dumps(data,separators=(',',':'));save.write_bytes((header+'\n'+hashlib.sha256(payload.encode()).hexdigest()+'\n'+payload).encode())
 try:
  a=private();a.input('fixtureTravel',text='zoo');home.ready(a);tap('Visit Dinosaur Valley');wait(lambda:inspect()['zone']=='zoo-dinosaurs','private dinosaur entry',30);home.ready(a);time.sleep(.6);tap('Join fossil discovery');wait(lambda:slot['profile'] in f()['members'],'private join',8);time.sleep(1.3)
  for _ in range(6):tap('Uncover or pick up fossil 0')
  tap('Uncover or pick up fossil 0');require(f()['holders'][0]==slot['profile'],'private pickup');tap('Match fossil picture 1');require(f()['holders'][0]==slot['profile'],'private wrong match');tap('Match fossil picture 0')
  for _ in range(3):tap('Uncover or pick up fossil 1')
  before=f();home.capture(a,out,'private-partial');save=close_save();a=private();require(f()['revealed']==before['revealed'] and f()['placed']==before['placed'] and f()['holders']==['','',''],'partial private reopen');rec('Solo actual reveal/pickup/wrong placement; real private save reopening retains partial progress')
  tap('Join fossil discovery');time.sleep(1.3)
  for i,count in [(1,3),(2,6)]:
   for _ in range(count):tap('Uncover or pick up fossil '+str(i))
   tap('Uncover or pick up fossil '+str(i));tap('Match fossil picture '+str(i))
  require(f()['placed']==[True]*3,'private complete');wait(lambda:inspect()['fossilCelebrations']==1,'private celebration',5);home.capture(a,out,'private-completed');save=close_save();a=private();require(f()['placed']==[True]*3 and inspect()['fossilCelebrations']==0,'completed reopen or old reaction');rec('Solo completes all sections; actual completed save reopening retains display and retires old animation')
  tap('Join fossil discovery');time.sleep(1.3);tap('Play fossil discovery again');require(f()['placed']==[True]*3,'unconfirmed replay erased display');tap('Keep dinosaur picture');tap('Play fossil discovery again');tap('Confirm fossil replay');require(f()['round']==2 and f()['revealed']==[0]*3,'private replay');rec('Private pictured replay confirmation/cancel and authoritative next-round identity')
  for _ in range(2):tap('Uncover or pick up fossil 2')
  private_state=f();save=close_save();run.slots=[dict(profile=p['profile'],token=secrets.token_hex(32)) for p in players];server=run.start('server');shared=run.start('client',slot['profile']);home.ready(shared);require(server.state()['view']['zoo']['fossils']['revealed']==[0]*3,'private discoveries imported into shared');shared.close();server.close();a=private();require(f()['revealed']==private_state['revealed'] and f()['round']==2,'shared overwrote private');rec('Same-profile actual private/shared process launches keep discovery rounds and progress separate')
  save=close_save();modify(save,lambda d:d['zoo'].pop('fossils'));a=private();require(f()['round']==1 and f()['revealed']==[0]*3 and len(inspect()['zoo']['animals'])==16,'legacy default missing');rec('Actual older checkpoint without fossil field loads existing Zoo unchanged and starts empty pilot')
  passed=True
 finally:
  run.close();errors=[];apis=[]
  for v in run.instances:
   log=v.out/'player.log'
   if log.exists():
    lines=log.read_text(errors='replace').splitlines();errors.extend(l for l in lines if 'Exception:' in l or 'error CS' in l)
    if v.role=='client':require(any('Direct3D 11' in l for l in lines),'actual graphics API');apis.append(dict(instance=v.identity,api='Direct3D 11',exit=v.process.returncode))
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,clients=apis,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
