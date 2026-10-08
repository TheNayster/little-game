"""Private native elephant feeding and restart retention; synthetic enrollment only."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import argparse,json,subprocess,time,uuid,importlib.util
from family_pairing import ROOT,create_family,write_record
from shared_garden_runtime import Instance,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
 folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}';summary=read(folder/'build-summary.json');authority,players,_=create_family()
 class Run:pass
 run=Run();run.build=args.build;run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True)
 out=run.path/'elephant-solo';out.mkdir();instances=[];checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def start():
  record=players[0];v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=record['profile'];v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
  pair=run.path/(v.identity+'.pairing');write_record(pair,record)
  cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=3,content=summary['content'],pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True)
  config=run.path/(v.identity+'.config.json');write(config,cfg)
  startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
  v.process=subprocess.Popen([str(folder/'Client/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591'],stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
  instances.append(v);wait(lambda:v.status(),'private status',30);home.ready(v);require(not v.input('inspect')['shared'],'private client joined');return v
 def inspect():return a.input('inspect')
 def animal():return inspect()['zoo']['animals'][0]
 def portion():return next(f for f in inspect()['zoo']['food'] if f['actor']==a.profile)
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 try:
  a=start();a.input('fixtureTravel',text='zoo');home.ready(a)
  a.input('touchButton',text='Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','private gate walk',30);home.ready(a)
  a.input('touchButton',text='Take leaves for elephant');wait(lambda:portion()['offered'],'private automatic food flow',30)
  wait(lambda:animal()['phase']==6 and not animal()['consumed'],'private eating',40);home.capture(a,out,'solo-eating')
  wait(lambda:animal()['consumed'],'private consumption',8);require(inspect()['elephantFinishEvents']==1,'private finish missing');home.capture(a,out,'solo-finish')
  wait(lambda:portion()['species']=='' and animal()['owner']=='','private normal routine',10)
  require(animal()['fed']==1 and inspect()['elephantCue']=='idle','private duplicate/stale cue')
  record('private native automatic collection/placement, eating, once-only finish and normal routine')
  a.input('application-pause');time.sleep(.3);a.input('application-resume');home.ready(a)
  require(animal()['fed']==1,'pause lost history');require(inspect()['elephantFinishEvents']==1,'resume replayed finish')
  a.input('fixtureTravel',text='creek');home.ready(a);require(inspect()['elephantCue']=='','exhibit switch stale cue')
  a.input('fixtureTravel',text='zoo');home.ready(a);a.input('touchButton',text='Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','return',30);home.ready(a)
  a.input('touchButton',text='Take leaves for elephant');wait(lambda:portion()['offered'],'unfinished offer',30)
  a.input('application-pause');time.sleep(.3);save=Path(inspect()['savePath']);require(save.exists(),'private save absent');a.close()
  a=start();require(animal()['fed']==1 and portion()['species']=='','restart did not clear unfinished offer/retain history')
  require(inspect()['elephantFinishEvents']==0,'restart replayed completion');home.capture(a,out,'solo-reopened')
  require(inspect()['zooCurrentExhibit']=='elephant','reopened saved world has invalid navigation')
  a.input('touchButton',text='Zoo map');require(inspect()['zooMapMarker']=='elephant','reopened map marker is incorrect')
  a.input('touchButton',text='Close Zoo map');require(not inspect()['zooMapOpen'],'reopened map did not close')
  record('pause/resume, world switch/return and real private save reopen retain history, clear unfinished offers and do not replay')
  passed=True
 finally:
  errors=[]
  for v in instances:
   if v.process.poll() is None:v.close()
   errors += [s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s]
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,nativeClients=1,authorityProcesses=0,privateSyntheticEnrollment=True,physicalDevices=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
