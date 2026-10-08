"""Private offline native surprise replay and real process/save reopening."""
import sys,time,argparse,importlib.util,uuid,subprocess
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,Instance,wait,require,write,ROOT
from family_pairing import create_family,write_record
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True);authority,players,_=create_family();slot=players[0]
 run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'elephant-surprises';out.mkdir();passed=False;checks=[]
 print('PRIVATE EVIDENCE '+str(out),flush=True)
 def private():
  v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=slot['profile'];v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
  pair=run.path/(v.identity+'.pairing');write_record(pair,slot)
  cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True)
  config=run.path/(v.identity+'.config.json');write(config,cfg)
  startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
  v.process=subprocess.Popen([str(run.folder/'Client/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591'],startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
  run.instances.append(v);wait(lambda:v.status(),'private startup',30);home.ready(v);return v
 def inspect():return a.input('inspect')
 def tap(i):
  control=next(c for c in inspect()['controls'] if c['name']==['Rustling leaves','Butterfly flowers'][i])
  if control['bounds']['width']<80:
   x=1000 if i==0 else 1600
   a.input('touch-begin',x=x,y=100,finger=63);a.input('touch-end',x=x,y=100,finger=63);time.sleep(2)
  wait(lambda:not inspect()['pending'],'private input ready',8);a.input('touchButton',text=['Rustling leaves','Butterfly flowers'][i])
 try:
  a=private();a.input('fixtureTravel',text='zoo');home.ready(a);a.input('touchButton',text='Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','private entry',30);home.ready(a)
  a.input('touch-begin',x=1200,y=100,finger=63);a.input('touch-end',x=1200,y=100,finger=63);time.sleep(3)
  for repeat in range(2):
   for i in range(2):tap(i)
   wait(lambda:inspect()['zoo']['surpriseSequence']==[repeat+1,repeat+1],'private triggers',5)
   home.capture(a,out,'private-replay-'+str(repeat));time.sleep(8.2)
  checks.append('Both single-tap private surprises reset and replay without server connection')
  for i in range(2):tap(i)
  a.input('application-pause');a.input('application-resume');home.ready(a)
  sounds=inspect()['surpriseSounds'];time.sleep(.3);require(inspect()['surpriseSounds']==sounds,'resume replayed sound')
  save=Path(inspect()['savePath']);require(save.exists(),'real private save missing')
  a.input('application-pause');time.sleep(.3);a.close();a=private()
  z=inspect()['zoo'];require(z['surpriseSequence']==[0,0] and z['surpriseAge']==[10,10] and inspect()['surpriseSounds']==0,'restore replayed transient visitor')
  home.capture(a,out,'private-reopened');checks.append('Pause/resume and actual save/process reopen produce idle props without stale audio')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,private=True,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
