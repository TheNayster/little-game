# /// script
# dependencies = ["cryptography"]
# ///
"""Focused packaged Markets presentation check; reuses source-identical583Core acceptance."""
import argparse,hashlib,importlib.util,json,sys,time,uuid,shutil,subprocess
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
control_spec=importlib.util.spec_from_file_location('markets_controls',Path(__file__).with_name('Test-MarketsControls.py'));controls=importlib.util.module_from_spec(control_spec);control_spec.loader.exec_module(controls)
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('--final-quick',action='store_true');a=p.parse_args();record=read(ROOT/'LocalData/markets-last-checked-run.json');require(record,'Prior shared acceptance required')
 run=Run(a.build,resume=record['runId'],extended_test_lifetime=True,graphics_api='d3d11');out=run.path/('markets-presentation-'+str(a.build));out=out if not out.exists() else run.path/(out.name+'-retry-'+uuid.uuid4().hex[:8]);out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 prior=read(ROOT/'Builds/NetworkProbe'/('G3-0.0.'+str(record['build']))/'source-manifest.json');current=read(run.folder/'source-manifest.json')
 def core(d):return {f['path']:f['sha256'] for f in d['files'] if '/Code/Core/' in f['path']}
 require(core(prior)==core(current),'Core changed; prior activity acceptance cannot be reused')
 def cmd(c,action,**kw):
  v=home.command(c,action,**kw);require(v['accepted'],v['outcome']);home.ready(c);return v
 def tap(c,name):
  controls.MarketsTouches(c).ensure(name)
  home.ready(c);wait(lambda:any(x['name']==name and x['enabled'] for x in c.input('inspect')['controls']),'enabled '+name)
  before=set(c.out.glob('reply-*.json'));c.input('touchButton',text=name)
  v=wait(lambda:next((v for p in c.out.glob('reply-*.json') if p not in before and (v:=read(p)) and v['outcome']!='stale-revision'),None),'ack '+name,25);require(v['accepted'],name+': '+v['outcome']);home.ready(c)
 def join(c,station,x):
  cmd(c,0,x=x,y=170);tap(c,'Markets visit '+station)
  wait(lambda:any(m['actor']==c.profile and m['station']==station for m in c.input('inspect')['markets']['members']),'joined '+station)
 def idle(station):wait(lambda:not next(t for t in server.state()['view']['markets']['stations'] if t['id']==station)['workingActor'],'station finish')
 def care_film(c):
  folder=out/'actual-care-frames';folder.mkdir();frames=[];began=time.monotonic()
  while time.monotonic()-began<5.8:
   requested=time.monotonic();name='frame-'+str(len(frames)).zfill(3);state=home.capture(c,folder,name);completed=time.monotonic()
   frames.append(dict(file=name+'.png',requestedAt=requested-began,completedAt=completed-began,representedAt=(requested+completed)/2-began,clock=state['markets']['clock'],actorPose=next(m['pose'] for m in state['markets']['members'] if m['actor']==c.profile)))
   time.sleep(max(0,.125-(completed-requested)))
  require(len(frames)>=8,'Insufficient native care frames')
  require(any(f['actorPose'].startswith('feed-') for f in frames) and frames[-1]['actorPose']=='idle','Care film must include feeding contact and return to idle')
  write(folder/'actual-monotonic-frame-times.json',dict(wallSeconds=time.monotonic()-began,frames=frames,audio=False,timeScaleChanged=False))
  lines=[]
  for i,f in enumerate(frames):
   duration=frames[i+1]['representedAt']-f['representedAt'] if i+1<len(frames) else .125
   lines.extend(["file '"+f['file']+"'",'duration '+format(duration,'.6f')])
  lines.append("file '"+frames[-1]['file']+"'");concat=folder/'actual-care.ffconcat';concat.write_text('\n'.join(lines)+'\n',encoding='utf-8')
  encoder=shutil.which('ffmpeg');require(encoder,'FFmpeg required for requested normal-time native care film')
  subprocess.run([encoder,'-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(concat),'-an','-vf','pad=ceil(iw/2)*2:ceil(ih/2)*2','-fps_mode','vfr','-c:v','libx264','-pix_fmt','yuv420p','-movflags','+faststart',str(out/'actual-pony-care-silent.mp4')],check=True,creationflags=subprocess.CREATE_NO_WINDOW)
  for i in range(3):
   for suffix in ('.png','.json'):shutil.copyfile(folder/('frame-'+str(i).zfill(3)+suffix),out/('pony-contact-'+str(i)+suffix))
  late=min((f for f in frames if f['actorPose'].startswith('feed-')),key=lambda f:abs(f['representedAt']-2.5))
  for suffix in ('.png','.json'):shutil.copyfile(folder/(Path(late['file']).stem+suffix),out/('pony-contact-late'+suffix))
  print('CARE_FILM '+str(out/'actual-pony-care-silent.mp4'),flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
  for c in clients:home.ready(c)
  for c,avatar in zip(clients,['blue-pup','orange-pup','chilli','bandit']):cmd(c,1,value=avatar)
  actor=clients[0]
  for c in clients:join(c,'buttermilk',3300)
  tap(actor,'Markets Food')
  care_film(actor)
  idle('buttermilk');tap(clients[1],'Markets Water');home.capture(clients[1],out,'bingo-water-contact');idle('buttermilk')
  tap(clients[2],'Markets Brush');home.capture(clients[2],out,'adult-brush-contact');idle('buttermilk')
  checks.append('Four mixed family drawings, named player badges, measured-hand food/water/brush contact and source pony poses in packaged client')
  for c in clients:join(c,'music',2700)
  tap(actor,'Markets Play')
  for song in ([3] if a.final_quick else [1,2,3]):
   tap(actor,'Markets Song '+str(song))
   for _ in range(3 if a.final_quick else 12):tap(actor,'Markets Drums')
   tap(actor,'Markets Guitar');home.capture(actor,out,'busker-song-'+str(song))
  checks.append('Packaged source song/guitar controls with adapted Bandit performer' if a.final_quick else 'All three source music selections/layers and guitar controls with the adapted Bandit performer')
  for station,x in [('smoothie',550),('flowers',1100),('icecream',4200)]:
   join(actor,station,x)
   if station=='icecream':
    for friend in clients[1:]:join(friend,station,x)
   if a.final_quick and station=='icecream':
    # Earlier presentation runs intentionally save an unfinished reference
    # stack. Clear it explicitly through its pictured control before replay.
    if next(t for t in server.state()['view']['markets']['stations'] if t['id']=='icecream')['parts']:tap(actor,'Markets Start again')
    tap(actor,'Markets Chocolate');tap(actor,'Markets Add scoop');idle('icecream')
   home.capture(actor,out,station+'-pc')
  actor.input('resize',x=1280,y=591);home.ready(actor);home.capture(actor,out,'phone-shaped')
  actor.input('resize',x=1024,y=768);home.ready(actor);home.capture(actor,out,'tablet-shaped')
  checks.append('Packaged stall/building views and phone/tablet-shaped safe controls; no physical-device qualification')
  passed=True
 finally:
  run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
  write(out/'result.json',dict(passed=passed and not errors,build=a.build,checks=checks,runtimeErrors=errors,reusedCoreBuild=record['build'],physicalDevices=False,ownerAcceptance=False,audioListening=False,installedFamilyTouched=False));require(not errors,'Runtime exception')
if __name__=='__main__':main()
