# /// script
# dependencies = ["cryptography"]
# ///
"""One focused native bathroom pass: real doors/touches, four leases and departure."""
from pathlib import Path
import argparse,importlib.util,time
import shared_garden_runtime as runtime
from shared_garden_runtime import Run,wait,require,write

ROOT=Path(__file__).resolve().parent.parent
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--snapshot',type=Path);args=p.parse_args()
    if args.snapshot:runtime.ROOT=args.snapshot.resolve()
    run=Run(args.build);folder=run.path/'bathroom';folder.mkdir();checks=[]
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client','player-'+str(i)) for i in range(1,5)]
        def players():return server.state()['view']['players']
        def player(c):return next(v for v in players() if v['id']==c.profile)
        def send(c,act,**kw):require(home.command(c,act,**kw)['accepted'],'Bathroom command rejected')
        a=clients[0];a.input('resize',x=1280,y=591)
        for c in clients:
            home.travel(c,'home');send(c,0,x=-2980,y=420);send(c,11)
            wait(lambda:player(c)['zone']=='home-upstairs','upstairs arrival');home.ready(c)
            send(c,0,x=2290,y=420)
        home.capture(a,folder,'hallway-door-phone')
        for i,c in enumerate(clients):
            c.input('touchButton',text='Enter bathroom')
            wait(lambda:player(c)['zone']=='home-bathroom','real bathroom door');home.ready(c)
            wait(lambda:sum(v['fixture'].startswith('bath-') for v in c.state()['view']['players'])==i,'bath lease view')
            c.input('touchButton',text='Bath');wait(lambda:player(c)['fixture'].startswith('bath-'),'real bath button')
        require(len({v['fixture'] for v in players()})==4,'Bath places overlap')
        time.sleep(.6);home.capture(a,folder,'four-in-bath-phone')
        a.input('resize',x=1024,y=768);home.ready(a);time.sleep(.4);home.capture(a,folder,'four-in-bath-ipad')
        passed('Hallway picture door and four shared bath places at phone/iPad sizes')
        a.input('touchButton',text='Splash');home.ready(a)
        require(all(v['fixture'].startswith('bath-') for v in players()),'Splash displaced the group')
        a.input('touchButton',text='Bath');wait(lambda:player(a)['fixture']=='','bath exit')
        a.input('touchButton',text='Wash hands');wait(lambda:player(a)['fixture'].startswith('sink-'),'sink entry');home.ready(a)
        a.input('resize',x=1280,y=591);home.ready(a);time.sleep(.4);home.capture(a,folder,'sink-and-three-bathers-phone')
        passed('Splash, get out and sink coexist with sibling bath play')
        a.input('touchButton',text='Wash hands');wait(lambda:player(a)['fixture']=='','sink finish')
        a.input('touchButton',text='Leave bathroom');wait(lambda:player(a)['zone']=='home-upstairs','real hallway exit');home.ready(a)
        require(all(player(c)['zone']=='home-bathroom' and player(c)['fixture'].startswith('bath-') for c in clients[1:]),'Hallway exit moved siblings')
        clients[3].close();wait(lambda:player(clients[3])['fixture']=='','disconnect lease release')
        require(player(clients[1])['fixture'].startswith('bath-') and player(clients[2])['fixture'].startswith('bath-'),'Disconnect cleared remaining bathers')
        passed('Independent hallway exit and disconnect preserve remaining bathers')
        write(folder/'result.json',dict(build=args.build,checks=checks,passed=True,limits=['No device installation','Laundry, clothing wet/dry state, movable bath toys and shower interaction remain planned']))
    finally:run.close()

if __name__=='__main__':main()
