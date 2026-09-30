# /// script
# dependencies = ["cryptography"]
# ///
"""Focused four-client check of creek fishing using real picture controls."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'creek-fishing';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def view():return server.state()['view']
        def rod(client):return next(r for r in view()['creekFishing']['rods'] if r['actor']==client.profile)
        def tap(client,name):client.input('touchButton',text=name);home.ready(client);time.sleep(.12)
        for client in clients:home.ready(client);home.travel(client,'creek')
        for width,height,name in [(1280,591,'creek-menu-phone'),(1024,768,'creek-menu-ipad')]:
            a.input('resize',x=width,y=height);home.ready(a);tap(a,'Games');state=home.capture(a,folder,name)
            names=[v['name'] for v in state['controls']];require('Creek fishing' in names and 'Feed creek fish' in names,'Creek picture cards missing')
            require('Hide & seek' not in names,'Home menu leaked into creek');tap(a,'Back to play')
        record('creek-only picture cards fit phone and iPad')
        for client in clients:
            tap(client,'Games');tap(client,'Creek fishing');wait(lambda:rod(client)['mode']==1,'creek start')
            player=next(p for p in view()['players'] if p['id']==client.profile)
            require(player['zone']=='creek' and abs(player['x']-(3600+(rod(client)['slot']-1.5)*260))<1,'Creek bank placement')
        require(len({rod(client)['slot'] for client in clients})==4 and len(view()['creekFishing']['fish'])==20,'Expanded shared reach missing')
        for width,height,name in [(1280,591,'four-fishing-phone'),(1024,768,'four-fishing-ipad')]:
            a.input('resize',x=width,y=height);home.ready(a);home.capture(a,folder,name)
        record('four shared bank spots auto-cast into one 20-fish creek')
        for client in clients:
            wait(lambda:rod(client)['cast']==3,'creek bite',25);wait(lambda:any(v['name']=='Reel in' for v in client.input('inspect')['controls']),'visible Reel in control',25);tap(client,'Reel in')
            wait(lambda:rod(client)['cast']==4 and client.input('inspect')['creekFishingCloseup'],'creek catch close-up')
        require(len({rod(client)['fish'] for client in clients})==4,'Duplicate creek catch')
        home.capture(a,folder,'caught-fish-ipad');record('four distinct random bites, easy reel and painted catch close-up')
        tap(a,'Release');tap(a,'Switch activity');wait(lambda:rod(a)['mode']==2,'feeding switch');tap(a,'Sprinkle food')
        require(all(rod(client)['cast']==4 for client in (b,c,d)),'Feeding reset sibling catch')
        home.capture(a,folder,'feeding-with-catches-ipad');record('release and feeding coexist with sibling catches')
        d.close();wait(lambda:rod(d)['mode']==0,'independent departure');require(rod(c)['cast']==4,'Departure stopped sibling')
        tap(b,'Release');tap(b,'Leave creek');tap(b,'Games');tap(b,'Feed creek fish');wait(lambda:rod(b)['mode']==2,'direct feeding card')
        require(home.command(b,25,value='start-fishing',x=2350,y=100)['accepted'],'Home pond start')
        wait(lambda:rod(b)['mode']==0,'creek lease released on home travel');require(rod(c)['cast']==4,'Home pond disturbed creek')
        require(view()['pond']['fish'] and len(view()['pond']['fish'])==8,'Home pond overwritten')
        record('departure, direct feeding and Homeworld pond preserve creek siblings and separate fish')
        tap(a,'Leave creek');require(home.command(a,26,value='start')['accepted'],'boat start');home.ready(a)
        require(rod(a)['mode']==0 and rod(c)['cast']==4,'Boats disturbed creek fishing')
        home.capture(a,folder,'boats-still-playable-phone');record('existing boats remain playable alongside creek fishing')
        passed=True
    finally:write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)
if __name__=='__main__':main()
