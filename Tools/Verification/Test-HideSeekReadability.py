"""Focused Stage 1 visual pass; four release clients, disposable loopback saves."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import argparse,importlib.util,time,shutil,subprocess,json
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--world',choices=['home','daycare'],required=True);p.add_argument('--baseline',action='store_true');p.add_argument('--controls-only',action='store_true');args=p.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'hide-seek-readability';out.mkdir();clients=[];checks=[];passed=False;print('EVIDENCE '+str(out),flush=True)
 def cmd(v,action,**kw):require(home.command(v,action,**kw)['accepted'],'Fixture rejected');home.ready(v)
 def tap(v,name):v.input('touchButton',text=name);home.ready(v)
 def hideTap(v,name):
  prior=[None]
  def visible():
   e=v.input('inspect');c=next((c for c in e['controls'] if c['name']==name),None)
   if not c or not c['enabled']:return False
   stable=prior[0] is not None and abs(e['cameraX']-prior[0])<2;prior[0]=e['cameraX']
   b=c['bounds'];return stable and 0<b['x']+b['width']/2<e['screenWidth'] and 0<b['y']+b['height']/2<e['screenHeight']
  wait(visible,'On-screen hide control',3);tap(v,name)
 def cap(v,name):return home.capture(v,out,args.world+'-'+name)
 def state():return server.state()['view']['hideClub' if args.world=='daycare' else 'hideAndSeek']
 def member(v):return next(h for h in state()['members' if args.world=='daycare' else 'hiders'] if h['actor']==v.profile)
 def menu(v,tag=False):
  tap(v,'Games')
  if args.world=='home':tap(v,'Hide & seek');return
  e=v.input('inspect');b=next(c for c in e['controls'] if c['name']=='The Adventure')['bounds'];x=b['x']+b['width']*.7;y=b['y']+b['height']*.5
  v.input('touch-begin',role='screen',x=x,y=y,finger=76)
  for dy in [25,65,120,180,250,330,420]:v.input('touch-move',role='screen',x=x,y=min(e['screenHeight']-12,y+dy),finger=76)
  v.input('touch-end',role='screen',x=x,y=min(e['screenHeight']-12,y+420),finger=76);tap(v,'Tag with friends' if tag else 'Hide & seek with Calypso')
 def film(v,name):
  path=v.out/'sand-film';wait(lambda:(path/'times.txt').exists(),'native film complete',55);times=[float(t) for t in (path/'times.txt').read_text().splitlines()];frames=sorted(path.glob('*.png'));require(len(times)==len(frames)>30,'Film missing')
  concat=out/(name+'.txt');concat.write_text('\n'.join(s for i,f in enumerate(frames) for s in ["file '"+f.as_posix()+"'",'duration '+str(times[i+1]-times[i] if i+1<len(times) else .125)]))
  subprocess.run([shutil.which('ffmpeg'),'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/(name+'.mp4'))],check=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for i,v in enumerate(clients):
   home.ready(v);v.input('resize',x=1024 if i in [2,3] else 1280,y=768 if i in [2,3] else 591);cmd(v,1,value=['blue-pup','orange-pup','muffin','chloe'][i]);cmd(v,0,x=-3150,y=50)
  if args.world=='daycare':
   for v in clients:cmd(v,7,value='daycare')
  menu(a)
  if not args.baseline and not args.controls_only:a.input('sandFilm',x=42);c.input('sandFilm',x=42)
  if args.world=='daycare':
   for v in clients[1:]:tap(v,'Join friends')
   # Include an edge cover when available to exercise bounded reveal framing.
   g=state();edge=[i for i,n in enumerate(g['npcs']) if n['slot'] in [0,5]];npc=min(edge or list(range(len(g['npcs']))),key=lambda i:g['order'].index(g['npcs'][i]['slot']));slot=g['npcs'][npc]['slot'];x=260+slot*375
   for v in clients:cmd(v,0,x=x,y=90)
   for v in clients:hideTap(v,'Hide here '+str(slot+1));wait(lambda:member(v)['slot']==slot,'UI cover entry',5)
   wait(lambda:all(n['hidden'] for n in state()['npcs']),'NPC cover entry',8)
  else:
   slot=3
   for v in clients:tap(v,'Hide Tent');wait(lambda:member(v)['mode']==2,'UI Home cover entry')
  wanted=[v.profile for v in clients]+(['club-npc-'+str(npc)] if args.world=='daycare' else [])
  snapshots=[v.input('inspect') for v in clients]
  authorityBefore={p['id']:p['x'] for p in server.state()['view']['players'] if p['id'] in wanted}
  for e in snapshots:
   visible=[b for b in e['revealBodies'] if b['visible'] and (b['actor'].startswith('player-') or b['actor'] in wanted)]
   require(len(visible)==len(wanted) and len({round(b['point']['x'],2) for b in visible})==len(wanted),'Shared hidden drawings overlap')
   if args.world=='daycare':require(not any(b['visible'] for b in e['revealBodies'] if b['actor'].startswith('club-npc-') and b['actor'] not in wanted),'Unrelated hidden NPC leaked')
  cap(a,'hidden-phone');cap(c,'hidden-tablet')
  if args.baseline:a.input('sandFilm',x=42);c.input('sandFilm',x=42)
  def rendered():
   e=a.input('inspect');return e if set(wanted).issubset(e['revealActors']) else None
  e=wait(rendered,'actual rendered crowded reveal',40);started=time.monotonic();timings={}
  authorityReveal={p['id']:p['x'] for p in server.state()['view']['players'] if p['id'] in wanted};require(authorityReveal==authorityBefore,'Reveal presentation moved authoritative X positions');write(out/'authority-positions.json',dict(before=authorityBefore,reveal=authorityReveal,visual=e['revealBodies']))
  if args.controls_only:
   before=d.input('inspect');require(before['revealActors'],'No active reaction before movement');d.input('touchButton',text='Tap to walk');d.input('touch-begin',role='stick',x=45,finger=25);moving=wait(lambda:(e if (e:=d.input('inspect'))['homePose']=='Walk' else None),'Immediate reaction movement');require(a.input('inspect')['revealActors'],'Sibling response ended before movement assertion');d.input('touch-end',role='stick',finger=25)
   tap(b,'Back to Daycare' if args.world=='daycare' else 'All done');require(a.input('inspect')['revealActors'],'Sibling response interrupted by departure');checks.append('Actual joystick movement and independent departure while sibling reveal remains active; unchanged authoritative X before input')
   cap(a,'during-movement');write(out/'immediate-movement.json',dict(before=before['revealBodies'],moving=moving['revealBodies'],secondsAfterObservedReveal=time.monotonic()-started));passed=True;return
  cap(a,'surprise-phone');cap(c,'surprise-tablet')
  cameraAtReveal=c.input('inspect')['cameraX'];teacherAtReveal=state().get('teacherX')
  if args.baseline:time.sleep(.65);cap(a,'happy-phone');cap(c,'happy-tablet')
  if not args.baseline:
   # Normal touch drag on clear floor must keep manual camera control.
   endX=810 if args.world=='daycare' and slot==0 else 1090
   before=b.input('inspect')['cameraX'];b.input('touch-begin',role='screen',x=950,y=65,finger=24);b.input('touch-move',role='screen',x=endX,y=65,finger=24);b.input('touch-end',role='screen',x=endX,y=65,finger=24)
   manual=b.input('inspect')['cameraX'];require(abs(manual-before)>40,'Manual camera drag ignored');time.sleep(.15);require(abs(b.input('inspect')['cameraX']-manual)<3,'Seeker took camera back');timings['manualCamera']=time.monotonic()-started
   before=b.input('inspect');timings['reconnectStartedDuringReaction']=bool(a.input('inspect')['revealActors']);b.input('network-pause');b.input('network-resume');home.ready(b);after=b.input('inspect');key='daycareRevealEvents' if args.world=='daycare' else 'homeRevealEvents';require(after[key]<=before[key],'Reconnect replays old reveals');timings['reconnect']=time.monotonic()-started
   cap(a,'happy-phone')
   # A sibling exits while the other clients finish the complete visual response.
   require(a.input('inspect')['revealActors'],'Reaction ended before departure check')
   tap(d,'Back to Daycare' if args.world=='daycare' else 'All done')
   timings['departure']=time.monotonic()-started
   b.input('touchButton',text='Tap to walk');b.input('touch-begin',role='stick',x=45,finger=22);wait(lambda:b.input('inspect')['homePose']=='Walk','movement overrides reveal');b.input('touch-end',role='stick',finger=22);timings['movement']=time.monotonic()-started
   cap(c,'happy-tablet')
   time.sleep(.4);a.input('touchButton',text='Tap to walk');a.input('touch-begin',role='stick',x=45,finger=27);wait(lambda:a.input('inspect')['homePose']=='Walk','post-reveal pointer movement');time.sleep(.25);cap(a,'moving-phone');a.input('touch-end',role='stick',finger=27)
   require(timings['reconnectStartedDuringReaction'],'Reconnect started after the visual response');write(out/'interaction-timing.json',timings)
   if args.world=='daycare':
    cameraAfter=c.input('inspect')['cameraX'];teacherAfter=state()['teacherX'];require(abs(cameraAfter-cameraAtReveal)<3,'Remote search took the found child camera');write(out/'independent-camera.json',dict(cameraAtReveal=cameraAtReveal,cameraAfter=cameraAfter,teacherAtReveal=teacherAtReveal,teacherAfter=teacherAfter,teacherMoved=abs(teacherAfter-teacherAtReveal)>10))
   checks.append('Four actual clients; UI cover entry; manual camera and reconnect start during reveal; departure and post-response pointer movement (see interaction timing)')
  film(a,args.world+'-phone-normal-speed');film(c,args.world+'-tablet-normal-speed')
  if not args.baseline and args.world=='home':
   # Both supported costumes are genuinely equipped on native actors. Gallery
   # uses the same Wear/Present pipeline at actual world drawing scales.
   for v,color in [(a,'green'),(b,'blue')]:cmd(v,24,value='dinosaur',target=color)
   a.input('resize',x=1280,y=591);a.input('revealOutfits');wait(lambda:(a.out/'reveal-outfits-2.png').exists(),'phone costume registration gallery')
   for f in a.out.glob('reveal-outfits-*.png'):shutil.copyfile(f,out/('phone-'+f.name))
   c.input('revealOutfits');wait(lambda:(c.out/'reveal-outfits-2.png').exists(),'tablet costume gallery')
   for f in c.out.glob('reveal-outfits-*.png'):shutil.copyfile(f,out/('tablet-'+f.name))
   for v in clients:
    if member(v)['mode']!=0:tap(v,'All done')
    cmd(v,0,x=-3150,y=50)
   menu(a)
   for v in clients:tap(v,'Hide Tent');wait(lambda:member(v)['mode']==2,'Equipped replay hiding')
   wait(rendered,'Equipped actual discovery',35);cap(a,'equipped-surprise-phone');cap(c,'equipped-surprise-tablet');time.sleep(.8);cap(a,'equipped-happy-phone');cap(c,'equipped-happy-tablet')
   checks.append('Actual equipped Bluey/Bingo dinosaur reveal, green/blue; normal Muffin/Chloe; both-facing surprise/happy galleries at .65/.95/1 scale on both layouts')
   for v,x in zip(clients,[-3150,-3495,-4900,-7040]):cmd(v,0,x=x,y=50)
   menu(a)
   for v,name in zip(clients,['Tent','Wardrobe middle','Curtain','Folding screen']):tap(v,'Hide '+name);wait(lambda:member(v)['mode']==2,'Separate Home cover')
   e=cap(a,'concealment-phone');require(sum(b['visible'] for b in e['revealBodies'] if b['actor'].startswith('player-'))==1,'Unrelated Home hider visible');checks.append('Separate Home hiding retains concealment')
  if not args.baseline and args.world=='daycare':
   if state()['phase']==4:tap(a,'Come out')
   else:wait(lambda:state()['phase']==4,'Finish shared Daycare round',40);tap(a,'Come out')
   for v in clients[1:]:
    if not member(v)['attending']:menu(v)
   for i,v in enumerate(clients):cmd(v,0,x=260+i*375,y=90)
   for i,v in enumerate(clients):hideTap(v,'Hide here '+str(i+1));wait(lambda:member(v)['slot']==i,'Separate Daycare cover')
   e=cap(a,'concealment-phone');require(sum(b['visible'] for b in e['revealBodies'] if b['actor'].startswith('player-'))==1,'Unrelated Daycare hider visible');checks.append('Separate Daycare hiding retains concealment')
   for v in clients:tap(v,'Back to Daycare')
   menu(a,True)
   for v in clients[1:]:tap(v,'Join friends')
   wait(lambda:server.state()['view']['tagClub']['phase']==2,'Tag starts');before=server.state()['view']['tagClub'];time.sleep(1);after=server.state()['view']['tagClub'];require(before['npcs']!=after['npcs'],'Tag motion regressed');cap(a,'tag-phone');tap(d,'Back to Daycare');require(sum(m['attending'] for m in server.state()['view']['tagClub']['members'])==3,'Tag exit regressed');checks.append('Daycare Tag ordinary native movement and independent departure')
  passed=True;write(out/'interaction-timing.json',timings)
  checks.append('Actual phone 1280x591 and tablet 1024x768 native capture; '+str(len(wanted))+' co-hiders')
 finally:
  if not passed:
   for v in clients:
    if v.process.poll() is None:
     try:cap(v,'failure-'+v.profile)
     except Exception:pass
  run.close();errors=[]
  for v in run.instances:
   text=(v.out/'player.log').read_text(errors='replace');errors += [line for line in text.splitlines() if 'Exception:' in line or 'NullReferenceException' in line]
  write(out/'result.json',dict(passed=passed and not errors,build=args.build,world=args.world,baseline=args.baseline,checks=checks,actualConcurrentNativeClients=4,errors=errors,exitCodes=[v.process.returncode for v in run.instances],setup='Authority avatar/world/position fixtures; Games/invitations/hiding via normal touch controls',parentVisualApproval='pending',audioListening=False));print('RESULT '+str(out/'result.json'),flush=True)

if __name__=='__main__':main()
