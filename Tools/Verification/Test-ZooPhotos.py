"""Milestone9: native local album UI, isolated authority, no video/deployment."""
import argparse, hashlib, importlib.util, json, sys, time
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--review',action='store_true');p.add_argument('--basic',action='store_true');args=p.parse_args()
 run=Run(args.build,interactive=args.review,review_controls=args.review,extended_test_lifetime=True)
 out=run.path/'zoo-photos';out.mkdir();checks=[];passed=False;clients=[]
 print('EVIDENCE '+str(out),flush=True)
 def inspect(v):return v.input('inspect')
 def tap(v,name):v.input('touchButton',text=name);time.sleep(.12)
 def cmd(v,action,**kw):
  r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome','command rejected'));home.ready(v)
 def enter(v,area='zoo-savanna',x=1200):
  if inspect(v)['zone']!=area:
   if inspect(v)['zone'].startswith('zoo-'):
    cmd(v,0,x=200,y=100);cmd(v,22,value='gate',target='zoo')
   elif inspect(v)['zone']!='zoo':cmd(v,7,value='zoo')
   cmd(v,0,x=650 if area=='zoo-savanna' else 1750,y=100);cmd(v,22,value='gate',target=area)
  cmd(v,0,x=x,y=100);time.sleep(.8)
 def album(v):return inspect(v)['zooAlbum']
 def take(v):
  before=len(album(v)['photos']);tap(v,'Take Zoo photo')
  wait(lambda: not inspect(v)['zooPhotoSaving'],'photo saving',12)
  require(len(album(v)['photos'])==before+1,'photo not saved '+inspect(v)['zooPhotoFeedback'])
  return album(v)['photos'][-1]
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots[:2 if args.review else 4]];a,b=clients[:2]
  for v in clients:home.ready(v);v.input('resize',x=1280,y=591);enter(v)
  cmd(b,1,value='orange-pup')
  tap(a,'Zoo toy camera');first=take(a);time.sleep(.3);home.capture(a,out,'camera-confirmation-phone')
  directory=Path(inspect(a)['zooAlbumPath']);original=directory/(first['id']+'.jpg');require(original.exists(),'missing original')
  (out/'first-original.jpg').write_bytes(original.read_bytes());digest=hashlib.sha256(original.read_bytes()).hexdigest()
  tap(a,'Camera to album');tap(a,'Open Zoo photo 0');tap(a,'Add Zoo sticker 1');tap(a,'Move sticker right');require(album(a)['photos'][0]['stickers']==[dict(kind=1,cell=5)],'sticker move failed')
  home.capture(a,out,'sticker-editor-phone');tap(a,'Remove selected sticker');require(album(a)['photos'][0]['stickers']==[],'sticker removal failed');tap(a,'Add Zoo sticker 0');tap(a,'Move sticker left');tap(a,'Return to Zoo')
  require(hashlib.sha256(original.read_bytes()).hexdigest()==digest,'sticker edit changed original')
  tap(b,'Zoo photo album');require(album(b)['photos']==[] and inspect(b)['zooAlbumPath']!=str(directory),'local profile isolation failed');tap(b,'Return to Zoo')
  require(not inspect(b)['zooPhotoOpen'],'other UI changed');record('Actual elephant capture; independent local profile/UI; sticker add/move/remove keeps original bytes')
  tap(a,'Zoo toy camera');second=take(a);tap(a,'Camera to album');tap(a,'Open Zoo photo 1');tap(a,'Move photo left');require(album(a)['photos'][0]['id']==second['id'],'photo reorder failed');tap(a,'Return to Zoo')
  a.close();a=run.start('client',run.slots[0]['profile']);clients[0]=a;home.ready(a);enter(a);tap(a,'Zoo photo album')
  require(album(a)['photos'][0]['id']==second['id'] and album(a)['photos'][1]['stickers']==[dict(kind=0,cell=3)],'actual restart lost reorder/sticker edit')
  require(hashlib.sha256(original.read_bytes()).hexdigest()==digest,'restart changed original');home.capture(a,out,'reopened-album-phone')
  record('Actual process restart retains order/stickers and immutable original')
  if args.review:
   tap(a,'Return to Zoo')
   cmd(a,0,x=1700,y=100);cmd(b,0,x=1560,y=100);time.sleep(.8);tap(b,'Take leaves for elephant');tap(a,'Zoo photo album');tap(a,'Open Zoo photo 0');tap(a,'Add Zoo sticker 2');tap(a,'Remove selected sticker');tap(a,'Return to Zoo')
   wait(lambda:next(f for f in server.state()['view']['zoo']['food'] if f['actor']==b.profile)['offered'],'independent live player feeding',30)
   tap(a,'Zoo toy camera');take(a);tap(a,'Close Zoo camera');home.capture(a,out,'live-owner-ready')
   write(run.path/'live-review-ready.json',dict(build=args.build,ready=True,serverPid=server.process.pid,clientPids=[a.process.pid,b.process.pid],recordedVideo=False,liveFamilyTouched=False))
   print('READY '+str(run.path),flush=True)
   while any(v.process.poll() is None for v in [a,b]):time.sleep(.5)
   passed=True;return
  if args.basic:passed=True;return
  for w,h,label in [(1024,768,'tablet'),(640,400,'small-phone')]:
   a.input('resize',x=w,y=h);home.capture(a,out,'album-'+label);tap(a,'Open Zoo photo 0');home.capture(a,out,'editor-'+label)
   for c in inspect(a)['controls']:
    if c['name'].startswith(('Add Zoo sticker','Move sticker','Return to Zoo','Select Zoo sticker')):
     require(c['bounds']['width']>=43.9 and c['bounds']['height']>=43.9,'small target '+c['name'])
   controls=inspect(a)['controls'];remove=next(c['bounds'] for c in controls if c['name']=='Remove selected sticker')
   for c in controls:
    if c['name'].startswith('Add Zoo sticker'):
     r=c['bounds'];require(min(r['x']+r['width'],remove['x']+remove['width'])<=max(r['x'],remove['x']) or min(r['y']+r['height'],remove['y']+remove['height'])<=max(r['y'],remove['y']),'palette/remove overlap '+c['name'])
   tap(a,'Back to album grid')
  a.input('resize',x=1280,y=591);tap(a,'Open Zoo photo 0');tap(a,'Remove Zoo photo');a.input('escape');require(len(album(a)['photos'])==2,'Escape removed photo');tap(a,'Remove Zoo photo');tap(a,'Confirm remove Zoo photo');require(len(album(a)['photos'])==1,'confirmed removal failed');tap(a,'Undo removed photo');require(album(a)['photos'][0]['id']==second['id'],'undo order failed');tap(a,'Return to Zoo')
  record('Native simulated phone/tablet/small-phone targets; separated confirmation, Escape and exact undo')
  enter(a,'zoo-aquarium',1200);tap(a,'Zoo toy camera');fish=take(a);(out/'aquarium-original.jpg').write_bytes((directory/(fish['id']+'.jpg')).read_bytes());tap(a,'Close Zoo camera');enter(a)
  record('Non-elephant Aquarium actual moment captured through common view')
  tap(a,'Zoo toy camera');before=len(album(a)['photos']);a.input('photoShutterBurst');wait(lambda:not inspect(a)['zooPhotoSaving'],'burst completes');require(len(album(a)['photos'])==before+1,'overlapping captures from same-frame shutter burst');tap(a,'Close Zoo camera')
  tap(a,'Zoo photo album');a.input('application-focus-loss');require(not inspect(a)['zooPhotoOpen'],'focus loss kept overlay');a.input('application-focus-gain');home.ready(a);record('Twenty same-frame shutter callbacks produce one capture; focus loss safely restores input')
  # One client frames photos while three siblings retain shared activities.
  for v in clients:enter(v)
  for v in clients[1:]:cmd(v,0,x=1560,y=100);time.sleep(.8);tap(v,'Take leaves for elephant')
  wait(lambda:all(next(f for f in server.state()['view']['zoo']['food'] if f['actor']==v.profile)['offered'] for v in clients[1:]),'sibling offerings',35)
  fed=server.state()['view']['zoo']['animals'][0]['fed'];tap(a,'Zoo toy camera');take(a)
  wait(lambda:server.state()['view']['zoo']['animals'][0]['phase']==6,'feeding while framing',65);feeding=take(a);(out/'feeding-original.jpg').write_bytes((directory/(feeding['id']+'.jpg')).read_bytes());tap(a,'Close Zoo camera')
  wait(lambda:all(f['species']=='' for f in server.state()['view']['zoo']['food']),'queue clears',160)
  require(server.state()['view']['zoo']['animals'][0]['fed']==fed+3,'camera changed sibling feeding accounting')
  cmd(a,0,x=1000,y=100);time.sleep(.6);tap(a,'Splash button');tap(a,'Zoo toy camera');wait(lambda:server.state()['view']['zoo']['animals'][0]['phase']==10,'actual splash moment',25);water=take(a);(out/'water-original.jpg').write_bytes((directory/(water['id']+'.jpg')).read_bytes());tap(a,'Close Zoo camera')
  tap(b,'Choose elephant brush');wait(lambda:server.state()['view']['zoo']['animals'][0]['phase']==13,'care active',45)
  tap(a,'Choose elephant brush');wait(lambda:a.profile in server.state()['view']['zoo']['careMembers'],'local helper membership');tap(a,'Zoo toy camera');care=take(a);(out/'care-original.jpg').write_bytes((directory/(care['id']+'.jpg')).read_bytes());tap(a,'Close Zoo camera');require(a.profile in server.state()['view']['zoo']['careMembers'] and b.profile in server.state()['view']['zoo']['careMembers'],'camera canceled helping');tap(a,'Put elephant brush away');tap(b,'Put elephant brush away')
  cmd(clients[-1],7,value='creek');require(inspect(a)['zone']=='zoo-savanna','sibling departure moved camera player');record('Four actual clients continue feeding, water and brushing; independent sibling departure')
  tap(a,'Zoo toy camera')
  while len(album(a)['photos'])<24:take(a)
  tap(a,'Take Zoo photo');require(len(album(a)['photos'])==24 and 'full' in inspect(a)['zooPhotoFeedback'].lower(),'full album overwrote picture or missing feedback')
  tap(a,'Camera to album');home.capture(a,out,'full-album');tap(a,'Next album page');tap(a,'Open Zoo photo 0');tap(a,'Add Zoo sticker 2');require(album(a)['photos'][6]['stickers']==[dict(kind=2,cell=4)],'next page opened wrong picture');tap(a,'Back to album grid');tap(a,'Previous album page');tap(a,'Open Zoo photo 0');tap(a,'Remove Zoo photo');tap(a,'Confirm remove Zoo photo');tap(a,'Undo removed photo');require(len(album(a)['photos'])==24,'full album undo failed');tap(a,'Return to Zoo')
  record('24-photo bound, explicit full feedback, no overwrite and confirmed remove/undo')
  samples=[]
  for i in range(10):
   tap(a,'Zoo photo album');tap(a,'Open Zoo photo 0');tap(a,'Return to Zoo');time.sleep(.15);s=inspect(a);require(s['zooPhotoLoadedTextures']<=1,'decoded images retained after closing');samples.append(dict(allocated=s['allocatedMemory'],managed=s['managedMemory'],textures=s['zooPhotoLoadedTextures'],captureMilliseconds=s['zooPhotoCaptureMilliseconds'],saveMilliseconds=s['zooPhotoSaveMilliseconds']))
  write(out/'memory.json',samples);record('Ten repeated album/editor visits release decoded full images and thumbnails')
  # Touches targeted under the local overlay cannot activate habitat controls.
  tap(a,'Zoo photo album');ticket=server.state()['view']['zoo']['nextTicket'];tap(a,'Take leaves for elephant');require(server.state()['view']['zoo']['nextTicket']==ticket,'overlay passed habitat input');a.input('application-pause');require(not inspect(a)['zooPhotoOpen'],'pause did not close overlay');a.input('application-resume');home.ready(a)
  tap(a,'Zoo photo album');cmd(a,7,value='creek');require(not inspect(a)['zooPhotoOpen'],'world change kept overlay');enter(a)
  record('Modal habitat shielding, pause/resume and world-change closure')
  # Faults are confined to this generated isolated album, never family saves.
  a.close();latest=json.loads(max(directory.glob('album-*.json'),key=lambda p:json.loads(p.read_text())['generation']).read_text());healthy=latest['photos'][2]['id']
  (directory/(latest['photos'][0]['id']+'.jpg')).write_bytes(b'broken jpeg');(directory/(latest['photos'][1]['id']+'-thumb.jpg')).unlink()
  (directory/'album-0.json.pending').write_text('{interrupted');game_save=server.state()['view']['worldId']
  a=run.start('client',run.slots[0]['profile']);clients[0]=a;home.ready(a);enter(a);tap(a,'Zoo photo album');require(len(album(a)['photos'])==24,'fault destroyed healthy metadata');tap(a,'Open Zoo photo 0');tap(a,'Back to album grid');tap(a,'Open Zoo photo 2');home.capture(a,out,'fault-healthy-photo');require((directory/(healthy+'.jpg')).exists() and server.state()['view']['worldId']==game_save,'fault erased healthy picture or world');tap(a,'Return to Zoo')
  record('Missing thumbnail, corrupt full image and interrupted manifest preserve healthy entries/world')
  sizes=[p.stat().st_size for p in directory.glob('*.jpg')];write(out/'storage.json',dict(photoFiles=len(sizes),totalBytes=sum(sizes),largestBytes=max(sizes),dimensions=[(p['width'],p['height']) for p in album(a)['photos']]))
  if args.review:
   # Keep two real windows ready for owner input; no film or live authority.
   for v in clients[2:]:v.close()
   home.capture(a,out,'live-owner-ready');write(run.path/'live-review-ready.json',dict(build=args.build,ready=True,serverPid=server.process.pid,clientPids=[a.process.pid,b.process.pid],recordedVideo=False,liveFamilyTouched=False))
   print('READY '+str(run.path),flush=True)
   while any(v.process.poll() is None for v in [a,b]):time.sleep(.5)
  passed=True
 finally:
  run.close();errors=[]
  for v in run.instances:errors.extend(s for s in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in s or 'error CS' in s)
  write(out/'results.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,physicalDevices=False,recordedVideo=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
