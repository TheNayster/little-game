"""Stage 1 acceptance: disposable authority and four actual native release clients."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import argparse,importlib.util,time,shutil,subprocess,json
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--daycare-only',action='store_true');args=p.parse_args();run=Run(args.build,extended_test_lifetime=True);out=run.path/'hide-seek-stage1';out.mkdir();checks=[];passed=False;clients=[];trace=[]
 print('EVIDENCE '+str(out),flush=True)
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 def cmd(v,action,**kw):require(home.command(v,action,**kw)['accepted'],'Fixture rejected');home.ready(v)
 def tap(v,name):v.input('touchButton',text=name);home.ready(v)
 def cap(v,name):return home.capture(v,out,name)
 def state(day=False):return server.state()['view']['hideClub' if day else 'hideAndSeek']
 def member(v,day=False):return next(h for h in state(day)['members' if day else 'hiders'] if h['actor']==v.profile)
 def menu(v,day=False,tag=False):
  tap(v,'Games')
  if day:
   e=v.input('inspect');control=next(c for c in e['controls'] if c['name']=='The Adventure');b=control['bounds'];x=b['x']+b['width']*.7;y=b['y']+b['height']*.5
   v.input('touch-begin',role='screen',x=x,y=y,finger=76)
   for dy in [25,65,120,180,250,330,420]:v.input('touch-move',role='screen',x=x,y=min(e['screenHeight']-12,y+dy),finger=76)
   v.input('touch-end',role='screen',x=x,y=min(e['screenHeight']-12,y+420),finger=76)
   tap(v,'Tag with friends' if tag else 'Hide & seek with Calypso')
  else:tap(v,'Hide & seek')
 def film(v,name):
  path=v.out/'sand-film';wait(lambda:(path/'times.txt').exists(),'native film completed',55)
  times=[float(t) for t in (path/'times.txt').read_text().splitlines()];frames=sorted(path.glob('*.png'));require(len(times)==len(frames)>30,'Actual film frames')
  concat=out/(name+'.txt');concat.write_text('\n'.join(s for i,f in enumerate(frames) for s in ["file '"+f.as_posix()+"'",'duration '+str(times[i+1]-times[i] if i+1<len(times) else .125)]))
  subprocess.run([shutil.which('ffmpeg'),'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/(name+'.mp4'))],check=True)
  shutil.rmtree(path)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1024 if i==2 else 1280,y=768 if i==2 else 591);cmd(v,1,value=['blue-pup','orange-pup','muffin','chloe'][i]);cmd(v,0,x=-3150,y=50)
  if not args.daycare_only:
   menu(a);wait(lambda:state()['phase']==1,'Home countdown')
   for v in clients:tap(v,'Hide Tent');wait(lambda:member(v)['mode']==2,'Home shared hiding')
   samples=[v.input('inspect') for v in clients]
   positions=[[b['point']['x'] for b in e['revealBodies'] if b['actor'].startswith('player-') and b['visible']] for e in samples]
   require(all(len(set(p))==4 for p in positions),'Home human spacing defect');cap(a,'home-shared-hidden-phone');cap(c,'home-shared-hidden-tablet')
   a.input('sandFilm',x=27)
   wait(lambda:member(a)['mode']==3,'Home reveal',30)
   e=cap(a,'home-surprise-phone');trace.append(e)
   require(e['homeRevealEvents']==4 and len(e['revealActors'])==4,'Home exactly four reveal events')
   variants=[(e['revealActors'],e['revealVariants']) for e in [v.input('inspect') for v in clients]]
   require(len({tuple(x[1]) for x in variants})==1,'Home variants diverge')
   cap(c,'home-happy-tablet');cmd(b,0,x=-2950,y=50);tap(d,'All done')
   require(member(d)['mode']==0,'Independent leave during reaction');wait(lambda:b.input('inspect')['homePose']!='Surprise','Movement overrides reaction')
   time.sleep(1.4);require(all(v.input('inspect')['homeRevealEvents']==4 for v in clients),'Home snapshots replayed reactions')
   film(a,'home-actual-gameplay-silent');record('Home four-client shared cover: stable spacing, simultaneous unique reactions/variants, phone/tablet capture, movement and independent departure')
   # Native reconnect after discovery must baseline, rather than replay found state.
   old=c.profile;c.close();c=run.start('client',old);clients[2]=c;home.ready(c);require(c.input('inspect')['homeRevealEvents']==0,'Home reconnect replayed old discoveries')
   # Replay with separate covers uses normal current UI; placement is synthetic setup.
   for v,x in zip(clients,[-3150,-3495,-4900,-7040]):cmd(v,0,x=x,y=50)
   menu(a);wait(lambda:state()['round']==2,'Home replay')
   for v,name in zip(clients,['Tent','Wardrobe middle','Curtain','Folding screen']):tap(v,'Hide '+name);wait(lambda:member(v)['mode']==2,'Home separate hiding')
   e=a.input('inspect');require(sum(body['visible'] for body in e['revealBodies'] if body['actor'].startswith('player-'))==1,'Unrelated Home hider leaked')
   for v in clients:tap(v,'All done')
   record('Home replay/separate covers conceal unrelated humans; fresh native reconnect does not replay discoveries')
  for v in clients:cmd(v,7,value='daycare')
  menu(a,True)
  for v in clients[1:]:tap(v,'Join friends')
  g=state(True);slot=g['npcs'][0]['slot'];x=260+slot*375
  for v in clients:cmd(v,0,x=x,y=90);cmd(v,35,value='hide:hide',target=str(slot)+'@'+str(state(True)['round']));wait(lambda:member(v,True)['slot']==slot,'Daycare shared hiding')
  wait(lambda:all(n['hidden'] for n in state(True)['npcs']),'NPCs finish entering their covers',8)
  samples=[v.input('inspect') for v in clients]
  for e in samples:
   visible=[body for body in e['revealBodies'] if body['visible'] and (body['actor'].startswith('player-') or body['actor']=='club-npc-0')]
   require(len(visible)==5 and len({round(body['point']['x'],2) for body in visible})==5,'Five co-hiders overlap')
   require(not any(body['visible'] for body in e['revealBodies'] if body['actor'].startswith('club-npc-') and body['actor']!='club-npc-0'),'Unrelated hidden NPC leaked')
  cap(a,'daycare-five-hidden-phone');cap(c,'daycare-five-hidden-tablet');a.input('sandFilm',x=42)
  wait(lambda:all(actor in a.input('inspect')['revealActors'] for actor in [v.profile for v in clients]+['club-npc-0']),'Daycare crowded native reveal',40)
  e=cap(a,'daycare-five-surprise-phone');trace.append(e)
  require(all(actor in e['revealActors'] for actor in [v.profile for v in clients]+['club-npc-0']),'Five individual reactions missing')
  es=[v.input('inspect') for v in clients];require(len({tuple(e['revealVariants']) for e in es})==1,'Daycare variants differ')
  cap(c,'daycare-five-happy-tablet');beforeReconnect=c.input('inspect')['daycareRevealEvents'];c.input('network-pause');c.input('network-resume');home.ready(c);require(c.input('inspect')['daycareRevealEvents']<=beforeReconnect,'Reconnect replayed active discoveries');tap(d,'Back to Daycare');cmd(b,0,x=max(80,x-130),y=90)
  require(not member(d,True)['attending'] and member(a,True)['attending'],'Departure disrupted siblings')
  counts=[v.input('inspect')['daycareRevealEvents'] for v in clients[:3]];time.sleep(1.3)
  old=c.profile;c.close();c=run.start('client',old);clients[2]=c;home.ready(c);require(c.input('inspect')['daycareRevealEvents']==0,'Daycare reconnect replay')
  wait(lambda:state(True)['phase']==4,'Daycare complete',40);film(a,'daycare-actual-gameplay-silent')
  record('Daycare four humans plus one hidden NPC have five distinct registered positions; simultaneous discovery, consistent reactions, departure/movement/reconnect and muted phone/tablet presentation')
  # Deliberate replay, separate covers, normal NPC reveal behavior.
  tap(a,'Come out');wait(lambda:state(True)['round']==2,'Daycare replay')
  menu(d,True)
  for i,v in enumerate(clients):cmd(v,0,x=260+i*375,y=90);cmd(v,35,value='hide:hide',target=str(i)+'@'+str(state(True)['round']));wait(lambda:member(v,True)['slot']==i,'Separate Daycare cover')
  e=a.input('inspect');require(sum(body['visible'] for body in e['revealBodies'] if body['actor'].startswith('player-'))==1,'Unrelated Daycare human leaked')
  for v in clients:tap(v,'Back to Daycare')
  record('Daycare replay and separate-cover hiding retain concealment')
  menu(a,True,True)
  for v in clients[1:]:tap(v,'Join friends')
  wait(lambda:server.state()['view']['tagClub']['phase']==2,'Tag starts');before=server.state()['view']['tagClub'];time.sleep(3);after=server.state()['view']['tagClub'];require(before['npcs']!=after['npcs'],'Tag runners frozen');cap(a,'daycare-tag-phone');tap(d,'Back to Daycare');require(sum(m['attending'] for m in server.state()['view']['tagClub']['members'])==3,'Tag independent departure')
  record('Shared Daycare Tag retains animated running, common start and independent departure')
  write(out/'reaction-samples.json',trace);passed=True
 finally:
  if not passed:
   for v in clients:
    if v.process.poll() is None:
     try:cap(v,'failure-'+v.profile)
     except Exception:pass
  run.close();errors=[]
  for v in run.instances:
   text=(v.out/'player.log').read_text(errors='replace')
   errors += [str(v.out)+': '+line for line in text.splitlines() if 'Exception:' in line or 'NullReferenceException' in line]
  write(out/'result.json',dict(passed=passed and not errors,build=args.build,checks=checks,actualConcurrentNativeClients=4,liveFamilyTouched=False,soundAssetsChanged=False,audioListening=False,errors=errors,exitCodes=[v.process.returncode for v in run.instances]))
  print('RESULT '+str(out/'result.json'),flush=True)

if __name__=='__main__':main()
