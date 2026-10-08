"""Actual private Missing Ball replay/pause/save reopening; no video."""
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
 run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'elephant-story';out.mkdir();passed=False;checks=[]
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
 def story():return inspect()['zoo']['story']
 def tap(name):a.input('touchButton',text=name);time.sleep(.15)
 def reveal(name,target):
  for _ in range(6):
   state=inspect()
   if any(c['name']==name for c in state['controls']):return
   right=target>state['cameraX'];begin,end=(900,300) if right else (300,900)
   a.input('touch-begin',role='screen',x=begin,y=120,finger=64);a.input('touch-move',role='screen',x=end,y=120,finger=64);a.input('touch-end',role='screen',x=end,y=120,finger=64);time.sleep(.5)
  home.capture(a,out,'unreachable-control');raise AssertionError('Cannot pan to '+name)
 def pickup():reveal('Collect elephant ball',story()['x']);tap('Collect elephant ball');wait(lambda:story()['carrier']==a.profile,'solo auto-walk pickup',20)
 def deliver():reveal('Return elephant ball',1390);tap('Return elephant ball');wait(lambda:story()['phase']==5,'solo full reaction',35)
 try:
  a=private();a.input('fixtureTravel',text='zoo');home.ready(a);tap('Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','private entry',30);home.ready(a)
  reveal('Start Missing Ball',1390);tap('Start Missing Ball');wait(lambda:story()['session']==1,'solo invitation',5);wait(lambda:inspect()['zoo']['animals'][0]['phase']==15,'solo clue',10)
  pickup();session=story()['session'];a.input('application-pause');time.sleep(.3);a.input('application-resume');require(story()['session']==session and story()['carrier']==a.profile,'pause lost live carry');deliver();home.capture(a,out,'solo-complete');checks.append('Solo actual start/clue/pickup/return/full reaction; pause/resume retains live ball')
  time.sleep(8.1);reveal('Start Missing Ball',1390);tap('Start Missing Ball');wait(lambda:story()['session']==2,'solo deliberate replay',5);pickup()
  a.input('application-pause');save=Path(inspect()['savePath']);require(save.exists(),'missing private checkpoint');a.close();a=private()
  require(inspect()['zone']=='zoo-savanna' and story()['phase']==0 and story()['carrier']=='','reopened stale story or lost saved location')
  require(inspect()['zoo']['animals'][0]['phase'] not in (15,16,17),'stuck saved pose');checks.append('Actual private process/save reopening retires ball ownership/story/pose and preserves location')
  tap('Zoo toy camera');tap('Take Zoo photo');wait(lambda:not inspect()['zooPhotoSaving'],'private photo',12);photos=inspect()['zooAlbum']['photos'];require(len(photos)==1,'photo persistence fixture absent');tap('Close Zoo camera');a.input('application-pause');a.close();a=private();tap('Zoo photo album');require(inspect()['zooAlbum']['photos'][0]['id']==photos[0]['id'],'existing photo album lost on story save reopening');tap('Return to Zoo');checks.append('Existing private photo album persists across actual story-world process reopening')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:
   log=v.out/'player.log'
   if log.exists():errors.extend(line for line in log.read_text(errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
