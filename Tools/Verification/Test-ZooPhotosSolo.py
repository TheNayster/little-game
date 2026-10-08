"""Native private album/save restart and same-profile shared separation."""
import sys,time,argparse,importlib.util,uuid,subprocess,hashlib,secrets
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,Instance,wait,require,write,ROOT
from family_pairing import create_family,write_record
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True);authority,players,_=create_family();slot=players[0]
 run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'zoo-photos';out.mkdir();passed=False;checks=[]
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
 def tap(name):a.input('touchButton',text=name);time.sleep(.15)
 try:
  a=private();a.input('fixtureTravel',text='zoo');home.ready(a);tap('Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','private entry',30);home.ready(a)
  tap('Zoo toy camera');tap('Take Zoo photo');wait(lambda:not inspect()['zooPhotoSaving'],'private capture');s=inspect();require(len(s['zooAlbum']['photos'])==1,'private photo absent')
  directory=Path(s['zooAlbumPath']);id=s['zooAlbum']['photos'][0]['id'];original=directory/(id+'.jpg');digest=hashlib.sha256(original.read_bytes()).hexdigest()
  tap('Camera to album');tap('Open Zoo photo 0');tap('Add Zoo sticker 3');tap('Move sticker up');require(inspect()['zooAlbum']['photos'][0]['stickers']==[dict(kind=3,cell=1)],'private paw add/move failed');tap('Return to Zoo');a.input('application-pause');save=Path(inspect()['savePath']);require(save.exists(),'private game checkpoint absent');a.close()
  a=private();require(not inspect()['shared'],'private reopened as shared');require(inspect()['zone']=='zoo-savanna','saved Zoo location missing');home.ready(a);tap('Zoo photo album');s=inspect();require(Path(s['zooAlbumPath'])==directory and s['zooAlbum']['photos'][0]['stickers']==[dict(kind=3,cell=1)],'private process restart lost photo/edit')
  require(hashlib.sha256(original.read_bytes()).hexdigest()==digest and save.exists(),'private original/save lost');home.capture(a,out,'private-reopened');tap('Return to Zoo');a.close();checks.append('Actual private capture/sticker process restart retains immutable original and existing game checkpoint')
  # Same stable actor in an isolated shared lab still has a separate album.
  run.slots=[dict(profile=p['profile'],token=secrets.token_hex(32)) for p in players]
  server=run.start('server');a=run.start('client',slot['profile']);home.ready(a)
  r=home.command(a,7,value='zoo');require(r['accepted'],'shared Zoo travel');home.ready(a);tap('Zoo photo album');s=inspect();require(s['shared'] and s['zooAlbumPath']!=str(directory) and s['zooAlbum']['photos']==[],'private album leaked into shared play');require(original.exists() and save.exists(),'shared album changed private data');checks.append('Same stable profile has a separate empty shared album; private imagery and game save remain')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
