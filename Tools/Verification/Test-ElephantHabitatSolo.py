"""Private native habitat UI, real saves, legacy/malformed recovery and shared isolation."""
import sys,time,argparse,importlib.util,uuid,subprocess,json,hashlib,secrets
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,Instance,wait,require,write,ROOT
from family_pairing import create_family,write_record
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--recovery-only',action='store_true');args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11');authority,players,_=create_family();slot=players[0]
 run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir();out=run.path/'elephant-habitat';out.mkdir();passed=False;checks=[]
 print('PRIVATE EVIDENCE '+str(out),flush=True)
 def private():
  v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=slot['profile'];v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
  pair=run.path/(v.identity+'.pairing');write_record(pair,slot)
  cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=run.protocol,content=run.content,pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True)
  config=run.path/(v.identity+'.config.json');write(config,cfg)
  startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
  v.process=subprocess.Popen([str(run.folder/'Client/LittleWeepsNetwork.exe'),'-force-d3d11','-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591'],startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW)
  run.instances.append(v);wait(lambda:v.status(),'private startup',30);home.ready(v);return v
 def inspect():return a.input('inspect')
 def h():return inspect()['zoo']['habitat']
 def tap(name):a.input('touchButton',text=name);time.sleep(.18)
 def reveal(name,target):
  for _ in range(7):
   st=inspect()
   if any(c['name']==name for c in st['controls']):return
   right=target>st['cameraX'];begin,end=(900,300) if right else (300,900)
   a.input('touch-begin',role='screen',x=begin,y=120,finger=64);a.input('touch-move',role='screen',x=end,y=120,finger=64);a.input('touch-end',role='screen',x=end,y=120,finger=64);time.sleep(.5)
  raise AssertionError('Cannot reveal '+name)
 def open(kind):
  reveal('Decorate elephant habitat',330);tap('Decorate elephant habitat');wait(lambda:inspect()['elephantHabitatOpen'],'private basket',5);tap('Choose habitat '+str(kind))
 def edit():
  wait(lambda:h()['usingId']=='','private use finished',30);reveal('Decorate elephant habitat',330);tap('Decorate elephant habitat');wait(lambda:inspect()['elephantHabitatOpen'],'edit basket opens',5);tap('Move habitat prop 0')
 def save_close():
  a.input('application-pause');save=Path(inspect()['savePath']);require(save.exists(),'checkpoint missing');a.close();return save
 def modify(save,fn):
  require(run.run_id in str(save),'outside isolated private save');header,digest,payload=save.read_text(encoding='utf-8').split('\n',2);data=json.loads(payload);fn(data);payload=json.dumps(data,separators=(',',':'));save.write_bytes((header+'\n'+hashlib.sha256(payload.encode()).hexdigest()+'\n'+payload).encode('utf-8'))
 try:
  a=private();a.input('fixtureTravel',text='zoo');home.ready(a);tap('Visit the savanna');wait(lambda:inspect()['zone']=='zoo-savanna','private gate',30);home.ready(a)
  for kind,initial,nextslot in ([] if args.recovery_only else [(0,1,1),(1,0,1),(2,1,2)]):
   open(kind);tap('Habitat slot '+str(initial));require(h()['props']==[],'private preview committed');tap('Cancel habitat preview');require(h()['props']==[],'private cancel committed')
   open(kind);tap('Habitat slot '+str(initial));tap('Confirm habitat placement');wait(lambda:len(h()['props'])==1,'private commit',5)
   wait(lambda:inspect()['zoo']['animals'][0]['phase']==19,'private use',55);home.capture(a,out,'private-kind-'+str(kind))
   edit();tap('Habitat slot '+str(nextslot));tap('Confirm habitat placement');wait(lambda:h()['props'][0]['slot']==nextslot,'private move',5)
   edit();tap('Return habitat prop to basket');wait(lambda:h()['props']==[],'private return',5)
  if not args.recovery_only:checks.append('Solo real controls: all prop choices, preview/cancel, use, moving and returning each type')
  open(2);tap('Habitat slot 2');tap('Confirm habitat placement');wait(lambda:len(h()['props'])==1,'persist placement',5);before=h()['props'];save=save_close();a=private();require(h()['props']==before and inspect()['elephantHabitatPreview']==-1,'real save reopening lost committed state')
  checks.append('Actual private process/save reopening retains committed props and clears previews/use')
  # Modify only this authenticated scratch checkpoint, preserving its header/hash.
  save=save_close()
  def corrupt(data):data['zoo']['habitat']['props'] += [dict(id='bad',creator=slot['profile'],kind=99,slot=99,revision=0),data['zoo']['habitat']['props'][0].copy()]
  modify(save,corrupt);a=private();require(h()['props']==before and h()['recoveredEntries']==2,'malformed entry recovery lost healthy props');checks.append('Actual malformed placement save load isolates invalid/duplicate entries with diagnostics')
  save=save_close();modify(save,lambda data:data['zoo'].pop('habitat'));a=private();require(h()['props']==[],'legacy absent field not empty');checks.append('Actual legacy save reopening supplies an empty decoration area')
  open(1);tap('Habitat slot 0');tap('Confirm habitat placement');wait(lambda:len(h()['props'])==1,'private separation seed',5);private_props=h()['props'];save_close()
  run.slots=[dict(profile=p['profile'],token=secrets.token_hex(32)) for p in players];server=run.start('server');shared=run.start('client',slot['profile']);home.ready(shared);require(server.state()['view']['zoo']['habitat']['props']==[],'offline props imported into shared world');shared.close();server.close();a=private();require(h()['props']==private_props,'shared play overwrote private arrangement');checks.append('Private/shared server saves remain separate across actual process launches')
  for _ in range(15):open(2);tap('Habitat slot 2');tap('Cancel habitat preview')
  require(len(h()['props'])==1 and inspect()['elephantHabitatPreview']==-1,'preview cleanup changed placement');checks.append('Repeated preview/cancel cleanup retains bounded committed state')
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:
   log=v.out/'player.log'
   if log.exists():errors.extend(line for line in log.read_text(errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,physicalDevices=False,graphicsApi='Direct3D11',recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
