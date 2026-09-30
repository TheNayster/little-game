# /// script
# dependencies = ["cryptography"]
# ///
"""One focused native four-player creek-boat pass through real touch controls."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'creek-boats';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots];a,b,c,d=clients
        def boats():return server.state()['view']['creekBoats']['boats']
        def boat(client):return next(v for v in boats() if v['actor']==client.profile)
        def tap(client,name):client.input('touchButton',text=name);home.ready(client);time.sleep(.15)
        for client in clients:
            home.travel(client,'creek');tap(client,'Boats');wait(lambda:boat(client)['attending'],'shared launch participation')
            wait(lambda:client.input('inspect')['ownCreekBoatInView'],'ready boat visible at launch')
        require(len({v['slot'] for v in boats()})==4,'launch slots duplicated')
        for index,client in enumerate(clients):
            tap(client,'Decorate boat');wait(lambda:client.input('inspect')['boatWorkshopOpen'],'boat workshop')
            tap(client,['Leaf boat','Bark boat','Paper boat','Bark boat'][index]);tap(client,'Boat colour '+str(index))
            tap(client,'Leaf passenger');tap(client,'Flower decoration')
            if index%2:tap(client,'Choose landing')
            if client is a:
                for width,height,name in [(1280,591,'workshop-phone'),(1024,768,'workshop-ipad')]:
                    a.input('resize',x=width,y=height);home.ready(a);home.capture(a,folder,name)
            tap(client,'Back to creek')
        require(all(v['passenger'] and v['flower'] for v in boats()),'saved decorations missing')
        require({v['hull'] for v in boats()}=={0,1,2},'all three hulls not usable')
        require({v['dock'] for v in boats()}=={0,1},'landing choice missing')
        record('four players prepare leaf, bark and paper boats, colours, passengers, flowers and two landings')
        for client in clients:tap(client,'Launch boat');wait(lambda:boat(client)['phase']==1,'boat launch')
        a.input('resize',x=1280,y=591);home.ready(a);state=home.capture(a,folder,'four-boats-phone')
        require(state['visibleCreekBoats']==4,'four boats not rendered together')
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'four-boats-ipad')
        for client in (a,b,c):
            view=client.input('inspect')['creekBoats'];require(all(v['passenger'] and v['flower'] for v in view['boats']),'shared snapshot lost attachments')
        record('four decorated boats render in one river with replicated routes and attachments')
        d.close();wait(lambda:not boat(d)['attending'],'departing participant released')
        require(all(boat(client)['attending'] for client in (a,b,c)),'sibling participation reset')
        wait(lambda:all(v['phase']==2 for v in boats()),'reachable dock arrival',25)
        require(all(v['trips']==1 and v['passenger'] and v['flower'] for v in boats()),'arrival lost creation')
        home.capture(a,folder,'docked-boats-ipad')
        b.input('resize',x=1024,y=768);home.ready(b)
        wait(lambda:b.input('inspect')['ownCreekBoatInView'],'far landing visible on iPad')
        home.capture(b,folder,'far-landing-ipad')
        record('departing player leaves siblings active and all boats complete their voyage to reachable docks')
        tap(a,'Bring boat back');wait(lambda:boat(a)['phase']==0,'return to launch',10)
        require(boat(b)['phase']==2 and boat(c)['phase']==2,'retrieval reset a sibling boat')
        require(boat(a)['passenger'] and boat(a)['flower'],'retrieval lost attachments')
        tap(a,'Decorate boat');tap(a,'Paper boat');tap(a,'Launch decorated boat')
        wait(lambda:boat(a)['phase']==1 and not a.input('inspect')['boatWorkshopOpen'],'relaunch and close workshop')
        home.travel(b,'park');require(not boat(b)['attending'] and boat(c)['attending'],'independent travel reset sibling')
        record('retrieve, change hull and relaunch retain decorations; independent travel leaves sibling play intact')
        passed=True
    finally:
        write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)

if __name__=='__main__':main()
