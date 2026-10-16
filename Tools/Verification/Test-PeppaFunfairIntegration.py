# /// script
# dependencies = ["cryptography"]
# ///
"""Check combined-world admission, additive saved-state migration and shared Funfair input."""
import argparse,hashlib,importlib.util,re,sys,time
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('resume');a=p.parse_args();run=Run(a.build,resume=a.resume,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/('funfair-integration-'+str(a.build));out.mkdir();checks=[];passed=False;instances=[]
 require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in read(run.folder/'source-manifest.json')['files'] if f['path'].startswith('Unity/')),'Unity source differs from candidate')
 actions=[v.strip() for v in re.search(r'enum SoloAction\s*{([^}]+)}',(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text()).group(1).split(',')]
 def command(c,action,**kw):r=home.command(c,actions.index(action),**kw);require(r['accepted'],r['outcome']);home.ready(c)
 def button(c,name):wait(lambda:any(v['name']==name and v['enabled'] for v in c.input('inspect')['controls']),'enabled '+name);time.sleep(.3);c.input('touchButton',text=name);home.ready(c)
 def view():return server.state()['view']
 def note(text):checks.append(text);print('PASS '+text,flush=True)
 print('EVIDENCE '+str(out),flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];instances.extend(run.instances)
  wait(lambda:all(c.input('inspect')['visibleFunfairPlayers']==4 for c in clients),'four legacy Funfair clients')
  old=view()['funfair'];require(old['paintings']>=2 and old['slides']>=5 and old['members'][0]['paintStyle']==1 and view()['markets'] is not None,'old Funfair history lost or Markets not added');note('current combined authority and four clients add Markets to an actual prior Funfair save, preserving history and identity')
  for c in clients:command(c,'Move',x=700,y=150);button(c,'Funfair station wheel')
  wait(lambda:len(view()['funfair']['wheel']['riders'])==4,'four riders');c=clients[0];b=next(v['bounds'] for v in c.input('inspect')['controls'] if v['name']=='Funfair moving wheel');w=read(run.folder/'Client/LittleWeepsNetwork_Data/StreamingAssets/PeppaFunfair/wheel.json');r=w['pixels'];x=b['x']+(sum(v['x'] for v in w['seats'][:3])/3-r['x'])/r['width']*b['width'];y=b['y']+(sum(v['y'] for v in w['seats'][:3])/3-r['y'])/r['height']*b['height'];c.input('touch-begin',role='screen',x=x,y=y,finger=91);c.input('touch-end',role='screen',x=x,y=y,finger=91);wait(lambda:view()['funfair']['wheel']['stage']=='running','pictured wheel shared start');time.sleep(1);before=view()['funfair']['wheel']['momentum'];c.input('touch-begin',role='screen',x=x,y=y,finger=92);c.input('touch-move',role='screen',x=x+80,y=y+25,finger=92);c.input('touch-end',role='screen',x=x+80,y=y+25,finger=92);wait(lambda:view()['funfair']['wheel']['momentum']>before+10,'packed-mask wheel fast drag');home.capture(c,out,'combined-shared-wheel')
  command(c,'Travel',value='markets');wait(lambda:len(view()['funfair']['wheel']['riders'])==3,'independent exit to Markets');wait(lambda:view()['funfair']['wheel']['rounds']==old['wheel']['rounds']+1,'siblings finish',25);note('packed-mask native wheel tap/fast drag and one shared ride survive independent departure to Markets')
  for c in clients[1:]:command(c,'Travel',value='markets')
  wait(lambda:all(c.input('inspect')['zone']=='markets' and c.input('inspect')['marketsView']!='' for c in clients),'four Markets views');home.capture(clients[0],out,'combined-markets')
  for zone,field in [('hulk-birthday','visibleBirthdayPlayers'),('spidey-hq','visibleSpideyPlayers'),('peppa-funfair','visibleFunfairPlayers')]:
   for c in clients:command(c,'Travel',value=zone)
   wait(lambda:all(c.input('inspect')[field]==4 for c in clients),'four '+zone+' players')
  home.capture(clients[0],out,'combined-return');require(view()['funfair']['paintings']==old['paintings'] and view()['funfair']['slides']==old['slides'] and view()['funfair']['photoRole']==old['photoRole'],'world visits changed existing history');note('four clients visit packaged Markets, Hulk and HQ, then return with Funfair painting/photo/slide history intact')
  runId=run.run_id
  for c in clients:c.close()
  server.close();run=Run(a.build,resume=runId,extended_test_lifetime=True,graphics_api='d3d11');server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];instances.extend(run.instances);wait(lambda:all(c.input('inspect')['visibleFunfairPlayers']==4 for c in clients),'four combined saved clients');require(view()['funfair']['paintings']==old['paintings'] and view()['funfair']['members'][0]['paintStyle']==1 and view()['markets'] is not None,'combined saved state lost');note('real combined authority and four-client reopening preserve both world states');passed=True
 finally:
  run.close();errors=[line for v in instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line];write(out/'result.json',dict(passed=passed and not errors,build=a.build,checks=checks,runtimeErrors=errors,installedServerTouched=False,physicalDevices=False,ownerAcceptance=False));require(not errors,'Native runtime exception')
if __name__=='__main__':main()
