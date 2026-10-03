# /// script
# dependencies = ["cryptography"]
# ///
"""Capture actual home walking and qualify the visual rig in isolated release players."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import importlib.util
import shutil
import time
from pathlib import Path
from shared_garden_runtime import Run, read, write, wait, require

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--baseline',action='store_true');args=parser.parse_args()
    run=Run(args.build);folder=run.path/'walk';folder.mkdir();checks=[]
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');a=run.start('client','player-1');b=run.start('client','player-2')
        for client in (a,b):
            client.input('resize',x=1280,y=720);home.ready(client);home.travel(client,'home')
        if not args.baseline:
            a.input('walkChecks');rig=read(a.out/'walk-checks.json');require(rig and rig['passed'],'Native rig checks missing');write(folder/'rig-checks.json',rig)
            checks.append('native sheet player: eight frames, 30/60/120 fps, home poses, switches, stops, travel and facing' if args.build>=120 else 'native rig: contact, clearance, 30/60/120 fps, stops, travel and facing')
            if args.build>=117:
                a.input('walkFilm');wait(lambda:(a.out/'walk-film/complete.json').exists(),'complete animation preview',120)
                shutil.copytree(a.out/'walk-film',folder/'film')
                checks.append('180-frame native fixed-time preview: both characters, starts/stops and reversal')
        for avatar,name in [('blue-pup','bluey'),('orange-pup','bingo')]:
            require(home.command(a,1,value=avatar)['accepted'],'Character selection failed')
            require(home.command(a,0,x=-4300,y=110)['accepted'],'Home start failed');time.sleep(.7)
            a.input('touchButton',text='Tap to walk')
            a.input('traceStart',role='player-1');b.input('traceStart',role='player-1')
            a.input('touch-begin',role='stick',x=55,finger=60)
            time.sleep(.5)
            for i in range(9):
                home.capture(a,folder,f'{name}-walk-{i:02}');time.sleep(.025)
            a.input('touch-end',role='stick',finger=60);time.sleep(.3)
            a.input('traceStop');b.input('traceStop')
            shutil.copyfile(a.out/'motion-trace.json',folder/f'{name}-local-trace.json');shutil.copyfile(b.out/'motion-trace.json',folder/f'{name}-remote-trace.json')
            home.capture(a,folder,f'{name}-stopped');require(a.input('inspect')['homePose']=='Idle','Walk failed to settle')
            a.input('touch-begin',role='stick',x=-55,finger=61);time.sleep(.4);home.capture(a,folder,f'{name}-left')
            a.input('touch-end',role='stick',finger=61);time.sleep(.3)
            a.input('touchButton',text='Tap to walk')
            checks.append(name+' actual home walking, reversal and stopped poses; local/remote traces')
        if not args.baseline:
            require(home.command(a,2,item='bucket-1')['accepted'],'Carry pickup failed')
            a.input('touchButton',text='Tap to walk');a.input('touch-begin',role='stick',x=30,finger=62);time.sleep(.5)
            require(a.input('inspect')['homePose']=='Carry','Carrying walk pose lost');home.capture(a,folder,'carrying-walk')
            a.input('touch-end',role='stick',finger=62);time.sleep(.3)
            require(next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder']=='player-1','Animation changed item ownership')
            checks.append('half-speed carrying walk preserves the one real bucket holder')
        write(folder/'result.json',dict(passed=True,build=args.build,baseline=args.baseline,checks=checks,liveFamilyTouched=False,physicalDevicesTouched=False))
        print('RESULT '+str(folder/'result.json'),flush=True)
    finally:run.close()

if __name__=='__main__':main()
