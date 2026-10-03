# /// script
# dependencies = ["cryptography"]
# ///
"""Measure the actual pond drawing across sparse full snapshots on four clients."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, math, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, read, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def trace(client,folder,name):
    client.input('pondTraceStop');samples=read(client.out/'pond-trace.json')['samples']
    pairs=[(a,b) for a,b in zip(samples,samples[1:]) if .001<b['time']-a['time']<.15]
    require(len(pairs)>100,'Too few native frames')
    moving=0;frozen=0;max_jump=0;route_changes=0
    for a,b in pairs:
        dt=b['time']-a['time'];rate=(b['clock']-a['clock'])/dt
        require(.899<rate<1.101,'Fish drawing clock froze, reversed or snapped')
        for i,(p,q) in enumerate(zip(a['positions'],b['positions'])):
            require((q['x']/380)**2+(q['y']/45)**2<=1.001,'Fish left water')
            if not(a['visible'][i] and b['visible'][i]):continue
            distance=math.hypot(q['x']-p['x'],q['y']-p['y'])
            if a['routes'][i]!=b['routes'][i]:
                route_changes+=1;max_jump=max(max_jump,distance)
            elif a['routes'][i]+.1<a['clock']<b['clock']<a['routes'][i]+a['durations'][i]-.1:
                moving+=1
                if distance<.00001:frozen+=1
    require(moving>300 and frozen==0,'Active swim route has frozen drawing frames')
    # A frame of an authored 1.1-second bite route can move quickly; a late
    # reroute must not teleport a fish across the pond.
    require(max_jump<30,'Route update teleported fish')
    require(route_changes>8,'Not enough real route transitions')
    result=dict(frames=len(pairs),movingFishFrames=moving,frozenFishFrames=frozen,routeChanges=route_changes,maximumRouteChangeDistance=max_jump)
    write(folder/(name+'.json'),result);print('PASS '+name+' '+str(result),flush=True)
    return result

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'pond-motion';folder.mkdir();passed=False;checks={}
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def rod(client):return next(r for r in server.state()['view']['pond']['rods'] if r['actor']==client.profile)
        def tap(client,label):client.input('touchButton',text=label);home.ready(client)
        for client in clients:
            home.ready(client);tap(client,'Games');tap(client,'Fishing')
            wait(lambda:rod(client)['mode']==1,'pond fishing start')
        require(len({rod(client)['slot'] for client in clients})==4,'Shared bank seats duplicated')
        a.input('resize',x=1280,y=591);home.ready(a);b.input('resize',x=1024,y=768);home.ready(b)
        for client in (a,b):client.input('pondTraceStart')
        tap(c,'Switch activity');wait(lambda:rod(c)['mode']==2,'feeding switch');tap(c,'Sprinkle food');time.sleep(3)
        home.capture(a,folder,'phone-fish');home.capture(b,folder,'ipad-fish')
        d.close();wait(lambda:rod(d)['mode']==0,'independent departure')
        require(rod(a)['mode']==1 and rod(b)['mode']==1 and rod(c)['mode']==2,'Departure interrupted siblings')
        wait(lambda:rod(a)['cast']==3,'bite',22);home.ready(a);tap(a,'Reel in')
        wait(lambda:rod(a)['cast']==4,'catch');tap(a,'Release')
        wait(lambda:rod(a)['fish']==-1,'release');tap(c,'Sprinkle food');time.sleep(3)
        checks['phone']=trace(a,folder,'phone');checks['ipad']=trace(b,folder,'ipad');passed=True
        print('PASS shared fishing/feeding, catch/release and independent departure',flush=True)
    finally:write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)

if __name__=='__main__':main()
