# /// script
# dependencies = ["cryptography"]
# ///
"""Focused real-client check of world-local Games entries and stale-menu closure."""
import argparse,importlib.util
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'world-menu';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');a=run.start('client','player-1');home.ready(a)
        def player():return next(p for p in server.state()['view']['players'] if p['id']==a.profile)
        def tap(name):a.input('touchButton',text=name);home.ready(a)
        def names():return {c['name'] for c in a.input('inspect')['controls']}
        def menu(world,has_home):
            tap('Games');shown=names();expected={'Hide & seek','Fishing','Feed fish'}
            require('Back to play' in shown,'Games menu failed to open at '+world)
            require(expected<=shown if has_home else not expected & shown,'Cross-world mini games at '+world)
            if world in ('home','daycare'):home.capture(a,folder,world+'-menu')
            tap('Back to play');require(player()['zone']==('garden' if world=='home' else world),'Opening menu traveled from '+world)
            checks.append('Current-world cards only: '+world);print('PASS '+checks[-1],flush=True)
        a.input('resize',x=1280,y=591);home.ready(a)
        require(home.command(a,0,x=-3900,y=50)['accepted'],'Home fixture');home.ready(a);menu('home',True)
        require(home.command(a,0,x=2350,y=220)['accepted'],'Backyard fixture');home.ready(a);menu('home',True)
        tap('Games');tap('Fishing')
        wait(lambda:any(r['actor']==a.profile and r['mode']==1 for r in server.state()['view']['pond']['rods']),'Home fishing launch')
        require(player()['zone']=='garden','Home game crossed world');require(home.command(a,25,value='leave')['accepted'],'Pond exit')
        require(home.command(a,0,x=-2980,y=420)['accepted'],'Stair approach');require(home.command(a,11)['accepted'],'Upstairs entry')
        wait(lambda:player()['zone']=='home-upstairs','Landing');home.ready(a);menu('home-upstairs',True)
        for world in ('park','creek','beach','daycare','zoo'):
            require(home.command(a,7,value=world)['accepted'],'Travel '+world);home.ready(a)
            if world=='daycare':a.input('resize',x=1024,y=768);home.ready(a)
            menu(world,False)
        tap('Games');require(home.command(a,7,value='daycare')['accepted'],'Travel with menu open');home.ready(a)
        wait(lambda:'Back to play' not in names(),'Stale menu closes on world change')
        require(home.command(a,7,value='home')['accepted'],'Return Home');home.ready(a);menu('home',True)
        checks.append('Home selection still starts fishing; a world change dismisses the old menu')
        print('PASS '+checks[-1],flush=True);passed=True
    finally:
        write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)
if __name__=='__main__':main()
