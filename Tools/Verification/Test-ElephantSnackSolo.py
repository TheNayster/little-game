"""Private elephant snack native UI and actual saved-world process reopening."""
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
 run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'elephant-snack';out.mkdir();passed=False;checks=[]
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
 def food():return next(f for f in inspect()['zoo']['food'] if f['actor']==a.profile)
 def tap(name):
  wait(lambda:not inspect()['pending'],'private input ready',8);a.input('touchButton',text=name);home.ready(a);time.sleep(.12)
 def open_bowl():
  # Actual screen touch moves to the station; ordinary camera follows.
  a.input('touch-begin',x=1700,y=100,finger=63);a.input('touch-end',x=1700,y=100,finger=63);time.sleep(2)
  tap('Prepare elephant snack');wait(lambda:inspect()['elephantSnackOpen'],'private panel',20)
 try:
  a=private();a.input('fixtureTravel',text='zoo');home.ready(a);tap('Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','private entry',30);home.ready(a)
  open_bowl();tap('Carry elephant snack');require(food()['preparing'] and food()['species']=='','empty bowl served')
  tap('Add snack leaves');tap('Add snack hay');tap('Remove snack piece 0');require(food()['pieces']==[1],'private removal')
  tap('Clear snack bowl');require(food()['pieces']==[],'private clear');tap('Add snack hay');tap('Add snack leaves');home.capture(a,out,'private-prepared');tap('Carry elephant snack')
  wait(lambda:food()['offered'],'private handoff',25);require(food()['pieces']==[1,0],'ingredients changed on handoff');home.capture(a,out,'private-offered')
  wait(lambda:inspect()['zoo']['animals'][0]['consumed'],'private consumption',65);require(inspect()['zoo']['animals'][0]['fed']==1,'private accounting');home.capture(a,out,'private-consumed')
  wait(lambda:food()['species']=='','private finish',10);require(food()['pieces']==[],'private bowl not cleared')
  checks.append('Private actual UI selection, empty bowl, removal, clear, serving, original ingredients and single consumption')
  open_bowl();tap('Add snack leaves');tap('Add snack hay');a.input('application-pause');time.sleep(.3);a.input('application-resume');home.ready(a)
  require(food()['preparing'] and food()['pieces']==[0,1],'pause lost editable bowl')
  a.input('application-pause');time.sleep(.3);save=Path(inspect()['savePath']);require(save.exists(),'private save missing');a.close();a=private()
  require(food()['species']=='' and not food()['preparing'] and food()['pieces']==[] and not inspect()['elephantSnackOpen'],'restore retained unfinished bowl/UI');require(inspect()['zoo']['animals'][0]['fed']==1 and inspect()['elephantFinishEvents']==0,'restore lost history/replayed finish')
  home.capture(a,out,'private-reopened');checks.append('Actual private process/save reopening clears unfinished preparation and retains history without effects')
  open_bowl();tap('Add snack hay');tap('Carry elephant snack');wait(lambda:food()['offered'],'unfinished committed serving',25)
  a.input('application-pause');time.sleep(.3);a.close();a=private();require(food()['species']=='' and food()['pieces']==[] and inspect()['zoo']['animals'][0]['fed']==1,'restore retained committed snack or lost history')
  checks.append('Actual private save reopen also clears committed snack using existing transient-offer policy')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,private=True,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
