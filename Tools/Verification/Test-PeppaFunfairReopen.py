# /// script
# dependencies = ["cryptography"]
# ///
"""Verify the observed saved-photo repair, face surface and late four-client catch-up."""
import argparse,hashlib,importlib.util,re,sys,time
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('resume');a=p.parse_args();run=Run(a.build,resume=a.resume,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/('funfair-reopen-'+str(a.build));out.mkdir();checks=[];instances=[];passed=False
 require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in read(run.folder/'source-manifest.json')['files'] if f['path'].startswith('Unity/')),'Unity source differs from candidate')
 actions=[v.strip() for v in re.search(r'enum SoloAction\s*{([^}]+)}',(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text()).group(1).split(',')]
 def command(c,action,**kw):r=home.command(c,actions.index(action),**kw);require(r['accepted'],r['outcome']);home.ready(c)
 def button(c,name):
  wait(lambda:any(v['name']==name and v['enabled'] for v in c.input('inspect')['controls']),'enabled '+name);previous=None;stable=0;deadline=time.monotonic()+6
  while time.monotonic()<deadline:
   b=next(v['bounds'] for v in c.input('inspect')['controls'] if v['name']==name);point=(b['x'],b['y']);stable=stable+1 if previous and abs(previous[0]-point[0])+abs(previous[1]-point[1])<2 else 0
   if stable>=2:break
   previous=point;time.sleep(.12)
  c.input('touchButton',text=name);home.ready(c)
 def record(text):checks.append(text);print('PASS '+text,flush=True)
 def view():return server.state()['view']['funfair']
 print('EVIDENCE '+str(out),flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];instances.extend(run.instances)
  wait(lambda:all(c.input('inspect')['visibleFunfairPlayers']==4 for c in clients),'four saved Funfair clients');require(view()['paintings']==1 and view()['slides']==4,'existing activity results lost');record('four fresh clients reopen the exact failed photo checkpoint without the startup exception')
  wait(lambda:clients[0].input('inspect')['funfairMusic']=='slide' and clients[0].input('inspect')['funfairMusicPlaying'],'decoded section music')
  for c in clients:command(c,'Move',x=4200,y=150);button(c,'Funfair station paint');wait(lambda:any(m['actor']==c.profile and m['station']=='paint' for m in view()['members']),'shared paint queue')
  button(clients[1],'Funfair activity Tiger')
  for i,c in enumerate(clients[1:]):
   controls=c.input('inspect')['controls'];first=next(v['bounds'] for v in controls if v['name']=='Funfair colour 1');second=next(v['bounds'] for v in controls if v['name']=='Funfair colour 2');s=(second['x']-first['x'])/65;x=first['x']+(170+i*40)*s;y=first['y']+185*s;c.input('touch-begin',role='screen',x=x,y=y,finger=60+i);c.input('touch-end',role='screen',x=x,y=y,finger=60+i);wait(lambda:sum(v>0 for v in view()['paintDraft'])>=i+1,'stroke over recovered face')
  home.capture(clients[1],out,'paint-real-head');button(clients[1],'Funfair activity All done!');wait(lambda:view()['paintings']==2,'shared tiger reveal');home.capture(clients[0],out,'painted-reaction')
  for c in clients:button(c,'Funfair activity Close');button(c,'Funfair tool Leave')
  record('three native touch contributors paint the recovered head surface; source tiger reaction and saved design follow the recipient')
  c=clients[0];command(c,'Move',x=3000,y=150);button(c,'Funfair station slide');button(c,'Funfair activity Spinning');button(c,'Funfair activity Close');last=clients.pop();last.close();wait(lambda:len(server.state()['connected'])==3,'independent disconnect');late=run.start('client',last.profile);clients.append(late);instances.append(late);wait(lambda:all(v.input('inspect')['visibleFunfairPlayers']==4 for v in clients),'late four-client catch-up');wait(lambda:view()['slides']==5,'new completed photo',30);require(view()['photoRole']=='Peppa' and view()['photoPose']==2,'photo subject was not frozen at capture');record('late reconnect preserves siblings and the new shared slide/photo result')
  button(c,'Funfair tool Cast');button(c,'Funfair activity Mummy');wait(lambda:view()['members'][0]['role']=='Mummy','cast selection');require(view()['photoRole']=='Peppa','cast change rewrote the completed photo');home.capture(c,out,'cast-keeps-original-photo')
  for size,name in [((1280,591),'phone'),((1024,768),'tablet')]:c.input('resize',x=size[0],y=size[1]);home.ready(c);home.capture(c,out,name)
  expected=view().copy();runId=run.run_id
  for c in clients:c.close()
  server.close();run=Run(a.build,resume=runId,extended_test_lifetime=True,graphics_api='d3d11');server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];instances.extend(run.instances);wait(lambda:all(c.input('inspect')['visibleFunfairPlayers']==4 for c in clients),'four restored views');require(view()['paintings']==2 and view()['slides']==5 and view()['photoRole']=='Peppa' and view()['members'][0]['role']=='Mummy' and view()['members'][0]['paintStyle']==1 and view()['members'][0]['prizes']==expected['members'][0]['prizes'],'reopened history or identity differs');home.capture(clients[0],out,'saved-photo-and-character');record('real authority and four-client reopening retain paint, prize counts, ride/slide history and the original photo after a cast change')
  for zone,field in [('hulk-birthday','visibleBirthdayPlayers'),('spidey-hq','visibleSpideyPlayers')]:
   for c in clients:command(c,'Travel',value=zone)
   wait(lambda:all(c.input('inspect')[field]==4 for c in clients),'four packaged '+zone+' players');home.capture(clients[0],out,'regression-'+zone)
  for c in clients:command(c,'Travel',value='peppa-funfair')
  wait(lambda:all(c.input('inspect')['visibleFunfairPlayers']==4 for c in clients),'return from existing worlds');record('four clients also enter the packaged Hulk/HQ worlds and return with Funfair history retained');passed=True
 finally:
  run.close();errors=[line for v in instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line];write(out/'result.json',dict(passed=passed and not errors,build=a.build,checks=checks,runtimeErrors=errors,physicalDevices=False,installedServerTouched=False,ownerAcceptance=False,audioListening=False));require(not errors,'Native runtime exception; see result')
if __name__=='__main__':main()
