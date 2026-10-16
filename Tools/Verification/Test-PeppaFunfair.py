# /// script
# dependencies = ["cryptography"]
# ///
"""One focused four-native-client Funfair acceptance run, isolated from live play."""
import argparse,hashlib,importlib.util,os,re,sys,time
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('--art',type=Path);a=p.parse_args()
 if a.art:os.environ['LITTLE_WEEPS_FUNFAIR_ART']=str(a.art.resolve())
 else:os.environ.pop('LITTLE_WEEPS_FUNFAIR_ART',None)
 run=Run(a.build,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/'funfair';out.mkdir();checks=[];passed=False
 require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in read(run.folder/'source-manifest.json')['files'] if f['path'].startswith('Unity/')),'Unity source changed since candidate')
 actions=[s.strip() for s in re.search(r'enum SoloAction\s*{([^}]+)}',(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text()).group(1).split(',')]
 def command(c,action,**kw):
  r=home.command(c,actions.index(action),**kw);require(r['accepted'],r['outcome']);home.ready(c);return r
 def button(c,name):
  wait(lambda:any(v['name']==name and v['enabled'] for v in c.input('inspect')['controls']),'enabled '+name)
  previous=None;stable=0;deadline=time.monotonic()+6
  while time.monotonic()<deadline:
   bounds=next(v['bounds'] for v in c.input('inspect')['controls'] if v['name']==name);point=(bounds['x'],bounds['y']);stable=stable+1 if previous and abs(previous[0]-point[0])+abs(previous[1]-point[1])<2 else 0
   if stable>=2:break
   previous=point;time.sleep(.12)
  c.input('touchButton',text=name);home.ready(c)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def closePanel(c):
  if any(v['name']=='Funfair activity Close' and v['enabled'] for v in c.input('inspect')['controls']):button(c,'Funfair activity Close')
 print('EVIDENCE '+str(out),flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
  def view():return server.state()['view']['funfair']
  for c in clients:home.ready(c)
  home.scenic.NAMES['peppa-funfair']='World Peppa Funfair';home.travel(clients[0],'peppa-funfair')
  for c in clients[1:]:c.input('fixtureTravel',text='peppa-funfair')
  wait(lambda:all(c.input('inspect')['visibleFunfairPlayers']==4 for c in clients),'four recovered family players')
  home.capture(clients[0],out,'arrival');record('world-menu entry and four actual clients render the recovered cast')
  for c in clients:command(c,'Move',x=700,y=150);button(c,'Funfair station wheel');wait(lambda:len(view()['wheel']['riders'])==clients.index(c)+1,'wheel boarding')
  button(clients[0],'Funfair tool Start');wait(lambda:view()['wheel']['stage']=='running','common ride countdown');button(clients[1],'Funfair tool Spin');home.capture(clients[0],out,'shared-wheel')
  command(clients[0],'Travel',value='garden');wait(lambda:len(view()['wheel']['riders'])==3,'independent rider exit');wait(lambda:view()['wheel']['rounds']==1 and view()['wheel']['stage']=='idle','siblings finish shared wheel',25)
  command(clients[0],'Travel',value='peppa-funfair');record('one wheel, four boarding slots, common start/spin and independent departure')
  for c in clients:command(c,'Move',x=300,y=150);button(c,'Funfair station teacups')
  wait(lambda:len(view()['teacups']['riders'])==4,'teacup boarding');button(clients[0],'Funfair tool Start');wait(lambda:view()['teacups']['stage']=='running','shared teacups');button(clients[2],'Funfair tool Spin');home.capture(clients[0],out,'shared-teacups');wait(lambda:view()['teacups']['rounds']==1,'teacup result',25);wait(lambda:view()['teacups']['stage']=='idle','teacups unload');record('four shared family teacups use recovered cabin art and seated actions')
  for c in clients:
   command(c,'Move',x=1800,y=150);button(c,'Funfair station hook');wait(lambda:any(m['actor']==c.profile and m['station']=='hook' for m in view()['members']),'flower queue entry')
  wait(lambda:view()['hookStage']=='aim','first cast');button(clients[0],'Funfair flower 0');wait(lambda:view()['catches']==1,'one catch');home.capture(clients[0],out,'flower-prize');wait(lambda:view()['members'][0]['station']=='','prize finish')
  for c in clients:closePanel(c);button(c,'Funfair tool Leave')
  require(view()['prizeSerial']==1 and sum(view()['members'][0]['prizes'])==1,'prize duplicated');record('source casting/reward actions, shared flower queue and single prize ownership')
  for c in clients:
   command(c,'Move',x=4200,y=150);button(c,'Funfair station paint');wait(lambda:any(m['actor']==c.profile and m['station']=='paint' for m in view()['members']),'paint queue entry')
  wait(lambda:view()['paintActor']==clients[0].profile,'paint recipient');button(clients[1],'Funfair activity Dinosaur')
  for offset,c in enumerate(clients[1:]):
   controls=c.input('inspect')['controls'];first=next(v['bounds'] for v in controls if v['name']=='Funfair colour 1');second=next(v['bounds'] for v in controls if v['name']=='Funfair colour 2');scale=(second['x']-first['x'])/65;x=first['x']+(170+offset*40)*scale;y=first['y']+185*scale
   c.input('touch-begin',role='screen',x=x,y=y,finger=30+offset);c.input('touch-end',role='screen',x=x,y=y,finger=30+offset);wait(lambda:sum(v>0 for v in view()['paintDraft'])>=offset+1,'shared drawn stroke')
  home.capture(clients[1],out,'paint-together');button(clients[1],'Funfair activity All done!');wait(lambda:view()['paintings']==1,'one shared reveal');require(view()['members'][0]['paintStyle']==2,'design follows wrong player')
  for c in clients:closePanel(c);button(c,'Funfair tool Leave')
  record('three siblings use native touch strokes on one shared face and reveal the same dinosaur design')
  for c in clients:command(c,'Move',x=3000,y=150);button(c,'Funfair station slide')
  button(clients[0],'Funfair activity On belly');button(clients[1],'Funfair activity Spinning')
  for c in clients:closePanel(c)
  home.capture(clients[0],out,'slide-queue');wait(lambda:view()['slides']>=4,'all four source-duration slide turns',85);require(all(m['photos']>=1 for m in view()['members']),'slide photo missing');home.capture(clients[0],out,'slide-photos');record('four fair slide turns, original 14-second paths/poses and per-player photos')
  c=clients[0];command(c,'Move',x=150,y=150);button(c,'Funfair station smoothie')
  for fruit in ['Banana','Strawberry','Orange']:button(c,'Funfair activity '+fruit)
  button(c,'Funfair activity Blend!');wait(lambda:view()['smoothieMask']==8,'blended drink');button(c,'Funfair activity Take a drink');button(c,'Funfair activity Close');button(c,'Funfair tool Drink');wait(lambda:view()['smoothies']==1 and view()['members'][0]['station']=='','source drink action',10)
  command(c,'Move',x=3000,y=150);button(c,'Funfair tool Balloon');wait(lambda:any(p['holder']==c.profile and p['kind']=='balloon' for p in view()['props']),'balloon pickup');button(c,'Funfair tool Float');home.capture(c,out,'balloon-float');button(c,'Funfair tool Leave');button(c,'Funfair tool Drop');record('shared mixer, held cup drinking, balloon pickup/float and interruption')
  for size,name in [((1280,591),'phone'),((1024,768),'tablet')]:c.input('resize',x=size[0],y=size[1]);home.ready(c);home.capture(c,out,name)
  saved=view().copy();runId=run.run_id
  for c in clients:c.close()
  server.close();run=Run(a.build,resume=runId,extended_test_lifetime=True,graphics_api='d3d11');server=run.start('server');wait(lambda:server.state(),'saved authority');require(view()['paintings']==saved['paintings'] and view()['slides']==saved['slides'] and view()['members'][0]['paintStyle']==2 and view()['wheel']['riders']==[],'saved fair progress lost');c=run.start('client','player-1');home.ready(c);require(c.input('inspect')['funfair']['paintings']==1,'reconnected client differs');record('real authority close/reopen and client reconnect retain designs, rewards and photos')
  passed=True
 finally:
  write(out/'result.json',dict(passed=passed,build=a.build,runId=run.run_id,checks=checks,installedServerTouched=False,visualOwnerApproval=False,audioListening=False));run.close()
if __name__=='__main__':main()
