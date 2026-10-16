# /// script
# dependencies = ["cryptography"]
# ///
"""Capture the painted-face/held-prop depth repair on a current native candidate."""
import argparse,hashlib,importlib.util,re,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('resume');a=p.parse_args();run=Run(a.build,resume=a.resume,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/('funfair-presentation-'+str(a.build));out.mkdir();passed=False
 require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in read(run.folder/'source-manifest.json')['files'] if f['path'].startswith('Unity/')),'Source differs from current candidate')
 actions=[v.strip() for v in re.search(r'enum SoloAction\s*{([^}]+)}',(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text()).group(1).split(',')]
 def command(action,**kw):r=home.command(c,actions.index(action),**kw);require(r['accepted'],r['outcome']);home.ready(c)
 def button(name):wait(lambda:any(v['name']==name and v['enabled'] for v in c.input('inspect')['controls']),'enabled '+name);c.input('touchButton',text=name);home.ready(c)
 try:
  s=run.start('server');c=run.start('client','player-1');home.ready(c);require(c.input('inspect')['funfair']['members'][0]['paintStyle']>0,'Saved source design missing')
  command('Move',x=4200,y=150);home.capture(c,out,'saved-painted-face')
  command('Move',x=150,y=150);button('Funfair station smoothie');wait(lambda:s.state()['view']['funfair']['smoothieMask']==8,'source machine cup');button('Funfair station smoothie');wait(lambda:any(p['holder']==c.profile and p['kind']=='drink' for p in s.state()['view']['funfair']['props']),'held cup');home.capture(c,out,'held-cup-in-front');button('Funfair tool Drink');home.capture(c,out,'drink-contact')
  for size,name in [((1280,591),'phone'),((1024,768),'tablet')]:c.input('resize',x=size[0],y=size[1]);home.ready(c);home.capture(c,out,name)
  passed=True
 finally:
  run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line];write(out/'result.json',dict(passed=passed and not errors,build=a.build,runtimeErrors=errors,ownerVisualAcceptance=False,physicalDevices=False,installedServerTouched=False));require(not errors,'Native exception')
if __name__=='__main__':main()
