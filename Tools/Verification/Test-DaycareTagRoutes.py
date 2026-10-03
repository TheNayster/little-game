"""Focused Daycare Tag route check with four native release clients."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
 run=Run(args.build,extended_test_lifetime=True);out=run.path/'tag-routes';out.mkdir();clients=[];checks=[];traces=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['tagClub']
 def cmd(v,action,**kw):require(home.command(v,action,**kw)['accepted'],'fixture command failed');home.ready(v)
 def tap(v,name):v.input('touchButton',text=name);home.ready(v);time.sleep(.12)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1024 if i==2 else 1280,y=768 if i==2 else 591);cmd(v,7,value='daycare')
  cmd(a,35,value='tag:start');wait(lambda:state()['round']==1,'shared Tag starts')
  cast=[n['avatar'] for n in state()['npcs']]
  for v in [b,c,d]:wait(lambda:any(m['actor']==v.profile and m['invited'] for m in state()['members']),'peer invitation');tap(v,'Join friends')
  wait(lambda:all(m['attending'] for m in state()['members']),'four joined humans');wait(lambda:state()['phase']==2,'common countdown')
  require(len(set(cast))==4,'four distinct NPCs')
  # Keep all children against alternate walls while the authority runs real NPC
  # routing and contact turns. Only player placement uses fixture commands.
  for i,v in enumerate(clients):cmd(v,0,x=80 if i%2==0 else 2320,y=60 if i<2 else 240)
  start=time.monotonic();edgeRuns=[0.]*4;maxEdges=[0.]*4;last=None
  while time.monotonic()-start<32:
   t=state();now=time.monotonic();positions=[dict(x=n['x'],y=n['y']) for n in t['npcs']]
   if last:
    dt=now-last[0]
    for i,n in enumerate(positions):
     # A chasing NPC can legitimately arrive at the stationary human. Runners
     # must depart the edge rather than alternate tiny inward/outward steps.
     edge=n['x']<180 or n['x']>2220 or n['y']<70 or n['y']>230
     edgeRuns[i]=edgeRuns[i]+dt if edge and t['it']!='club-npc-'+str(i) and last[1]==t['it'] else 0.
     maxEdges[i]=max(maxEdges[i],edgeRuns[i])
   require(all(80<=n['x']<=2320 and 60<=n['y']<=240 for n in positions),'NPC escaped floor')
   traces.append(dict(seconds=round(now-start,3),it=t['it'] if t['it'].startswith('club-npc-') else 'human',positions=positions))
   last=(now,t['it']);time.sleep(.2)
  require(max(maxEdges)<1.8,'runner pinned to an edge: '+str(maxEdges))
  spans=[]
  for i in range(4):
   xs=[t['positions'][i]['x'] for t in traces];ys=[t['positions'][i]['y'] for t in traces]
   spans.append(dict(x=max(xs)-min(xs),y=max(ys)-min(ys)))
  require(all(s['x']>700 and s['y']>50 for s in spans),'NPCs lack interior/lane variety: '+str(spans))
  frames=[]
  for _ in range(8):frames.append(a.input('inspect')['clubNpcFrames']);time.sleep(.1)
  require(any(f!=frames[0] for f in frames[1:]),'walking drawings frozen')
  home.capture(a,out,'tag-routes-phone');home.capture(c,out,'tag-routes-tablet')
  record('four joined humans and four varied NPCs; 32-second wall-chaser trace stays bounded, runners leave edges and cross multiple lanes, walking frames animate')
  tap(d,'Back to Daycare');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'independent exit')
  require(state()['round']==1 and [n['avatar'] for n in state()['npcs']]==cast,'departure restarted round or cast')
  turns=state()['turns'];until=time.monotonic()+12
  while state()['turns']==turns and time.monotonic()<until:
   t=state();i=next(i for i in range(4) if t['it']!='club-npc-'+str(i));n=t['npcs'][i];cmd(a,0,x=n['x'],y=n['y']);time.sleep(.25)
  require(state()['turns']>turns,'routes prevent Tag contact')
  record('independent exit keeps siblings and NPC cast in the same round; authority contact still transfers the star')
  write(out/'routes.json',dict(maxRunnerEdgeSeconds=maxEdges,spans=spans,traces=traces,frames=frames));passed=True
 finally:
  run.close();write(out/'result.json',dict(build=args.build,passed=passed,checks=checks,exitCodes=[v.process.returncode for v in run.instances],scope='isolated release server and four Windows native clients; no physical device or live-server update'))
 print('ALL PASS',flush=True)

if __name__=='__main__':main()
