"""Actual private D3D11 clients: Savanna feeding, saves and offline/shared separation."""
import sys,time,argparse,importlib.util,uuid,subprocess
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,Instance,wait,require,write,ROOT
from family_pairing import create_family,write_record
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--species',choices=['giraffe','zebra','lion']);args=p.parse_args();run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');authority,players,_=create_family();slot=players[0]
 run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'savanna';out.mkdir();passed=False;checks=[]
 print('PRIVATE EVIDENCE '+str(out),flush=True)
 def private():
  v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=slot['profile'];v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir();pair=run.path/(v.identity+'.pairing');write_record(pair,slot)
  cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True)
  config=run.path/(v.identity+'.config.json');write(config,cfg);startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
  v.process=subprocess.Popen([str(run.folder/'Client/LittleWeepsNetwork.exe'),'-force-d3d11','-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591'],startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW);run.instances.append(v);wait(lambda:v.status(),'private startup',30);home.ready(v);require(not v.input('inspect')['shared'],'unexpected shared world');return v
 def inspect():return a.input('inspect')
 def z():return inspect()['zoo']
 def animal():return z()['animals'][panel]
 def food():return next(f for f in z()['food'] if f['actor']==a.profile)
 def tap(name):a.input('touchButton',text=name);time.sleep(.12)
 def rec(s):checks.append(s);print('PASS '+s,flush=True)
 def close_save():
  a.input('application-pause');time.sleep(.3);save=Path(inspect()['savePath']);require(save.exists(),'actual checkpoint absent');a.close();return save
 try:
  a=private();a.input('fixtureTravel',text='zoo');home.ready(a);tap('Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','private gate',30);home.ready(a)
  for species,panel,kind in [('giraffe',1,'leaves'),('zebra',2,'hay'),('lion',3,'meat')]:
   if args.species and species!=args.species:continue
   while inspect()['zooCurrentExhibit']!=species:
    tap('Zoo next animal');wait(lambda:not inspect()['zooNavigationBusy'],'private exhibit navigation',25);home.ready(a)
   before=animal()['fed'];tap('Take '+kind+' for '+species)
   wait(lambda:food()['offered'],'private carry/offer',25);wait(lambda:animal()['phase']==6 and animal()['age']>.65 and not animal()['consumed'],'private mouth',45)
   require(inspect()['savannaCue']=='eating' and inspect()['savannaMouthGap']<1,'private cue/contact');home.capture(a,out,species+'-solo-eating')
   wait(lambda:animal()['consumed'],'private consumed',6);require(inspect()['savannaFinishEvents'][panel-1]==1,'private finish');wait(lambda:food()['species']=='','private release',10)
   require(animal()['fed']==before+1 and inspect()['savannaCue']=='idle','duplicate/stale private food')
   a.input('application-pause');a.input('application-resume');home.ready(a);require(inspect()['savannaFinishEvents'][panel-1]==1,'pause replay')
   close_save();a=private();require(animal()['fed']==before+1 and food()['species']=='' and inspect()['savannaFinishEvents'][panel-1]==0,'completed save lost/replayed')
   tap('Take '+kind+' for '+species);wait(lambda:food()['offered'],'unfinished private offer',25);close_save();a=private()
   require(animal()['fed']==before+1 and food()['species']=='' and inspect()['savannaFinishEvents'][panel-1]==0,'unfinished save replayed/retained lease');home.capture(a,out,species+'-private-reopened')
   tap('Zoo map');tap('Close Zoo map');require(not inspect()['pending'] and not inspect()['zooMapOpen'],'reopened modal stuck')
   rec(species+': solo actual touchscreen pickup through mouth/contact/finish, pause and actual completed/unfinished save reopening')
  retained=[v['fed'] for v in z()['animals']];close_save()
  # Same profile, two real runtimes; private feeding cannot upload to authority.
  run.slots=[dict(profile=p['profile'],token=uuid.uuid4().hex+uuid.uuid4().hex) for p in players]
  server=run.start('server');shared=run.start('client',slot['profile']);home.ready(shared);require(all(v['fed']==0 for v in server.state()['view']['zoo']['animals']),'private history imported');shared.close();server.close();a=private();require([v['fed'] for v in z()['animals']]==retained,'shared overwrote private');rec('same-profile private/shared process launches preserve separate worlds and feeding history')
  passed=True
 finally:
  run.close();errors=[];apis=[]
  for v in run.instances:
   lines=(v.out/'player.log').read_text(errors='replace').splitlines();errors.extend(l for l in lines if 'Exception:' in l or 'error CS' in l)
   if v.role=='client':require(any('Direct3D 11' in l for l in lines),'actual API');apis.append(dict(instance=v.identity,api='Direct3D11',exit=v.process.returncode))
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,clients=apis,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
