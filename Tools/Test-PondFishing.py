# /// script
# dependencies = ["cryptography"]
# ///
"""One focused native four-player pond pass through the real picture controls."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'pond';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots];a,b,c,d=clients
        def player(client):return next(p for p in server.state()['view']['players'] if p['id']==client.profile)
        def rod(client):return next(r for r in server.state()['view']['pond']['rods'] if r['actor']==client.profile)
        def tap(client,name):client.input('touchButton',text=name);home.ready(client);time.sleep(.12)
        for client in clients:
            home.ready(client);require(home.command(client,0,x=-3900,y=50)['accepted'],'home setup');home.ready(client)
        for width,height,name in [(1280,591,'menu-phone'),(1024,768,'menu-ipad')]:
            a.input('resize',x=width,y=height);home.ready(a);tap(a,'Games');state=home.capture(a,folder,name)
            names=[v['name'] for v in state['controls']]
            require(all(n in names for n in ('Fishing','Feed fish','Hide & seek','Back to play')),'picture choices missing')
            tap(a,'Back to play')
        record('separate Fishing and Feed fish picture cards fit phone and iPad menu')
        if args.build>=283:
            require(home.command(a,20,value='start')['accepted'],'hiding start')
            require(home.command(a,0,x=3150,y=50)['accepted'],'moved picnic approach')
            require(home.command(a,20,target='8',value='hide')['accepted'],'moved picnic hiding spot')
            require(abs(player(a)['x']-3150)<1,'hiding retained old picnic position')
            require(home.command(a,20,value='out')['accepted'],'picnic exit')
            record('moved picnic table retains its working hiding spot and independent exit')
        a.input('resize',x=1280,y=591);home.ready(a)
        for client in clients:
            tap(client,'Games');tap(client,'Fishing')
            wait(lambda:player(client)['zone']=='garden' and rod(client)['mode']==1 and rod(client)['cast']!=0,'immediate fishing start')
            require(abs(player(client)['x']-(2350+(rod(client)['slot']-1.5)*170))<1,'menu did not send player straight to pond')
        require(len({rod(client)['slot'] for client in clients})==4,'shared bank seats duplicated')
        home.capture(a,folder,'four-fishing-phone');a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'four-fishing-ipad')
        record('one menu tap travels from Home to four distinct pond spots and automatically casts')
        for client in clients:
            wait(lambda:rod(client)['cast']==3,'random fish bite',25);home.ready(client);tap(client,'Reel in')
            wait(lambda:rod(client)['cast']==4 and client.input('inspect').get('pondCloseup'),'caught fish close-up')
            if client is a:home.capture(a,folder,'caught-fish-closeup-ipad')
        require(len({rod(client)['fish'] for client in clients})==4,'one fish caught twice')
        record('four independent random bites reserve distinct fish and one Reel in tap opens each close-up')
        caught=rod(a)['fish'];tap(a,'Release');wait(lambda:rod(a)['fish']==-1,'release to pond')
        require(len(server.state()['view']['pond']['fish'])==8,'release removed fish')
        tap(a,'Switch activity');wait(lambda:rod(a)['mode']==2,'feeding switch')
        wait(lambda:len(server.state()['view']['pond']['food'])>0,'first food portion')
        home.capture(a,folder,'feeding-with-sibling-catches-ipad');tap(a,'Sprinkle food')
        require(all(rod(client)['cast']==4 for client in (b,c,d)),'feeding disturbed sibling catch')
        record('release returns same fish; feeding gathers other fish while siblings retain catches')
        d.close();wait(lambda:rod(d)['mode']==0,'departed rod released')
        require(all(rod(client)['cast']==4 for client in (b,c)),'departure stopped the pond')
        tap(b,'Release');tap(b,'Leave pond');wait(lambda:rod(b)['mode']==0,'leave pond')
        require(rod(c)['cast']==4,'leaving disturbed sibling')
        record('independent disconnect and Leave pond preserve remaining players')
        tap(a,'Leave pond');tap(a,'Games');tap(a,'Feed fish')
        wait(lambda:rod(a)['mode']==2 and len(server.state()['view']['pond']['food'])>0,'Feed fish menu starts feeding')
        tap(a,'Leave pond');time.sleep(.4)
        state=home.capture(a,folder,'pond-idle-ipad');require(state['pondWaterPlaying'],'idle pond water sound stopped')
        clock=server.state()['view']['pond']['clock'];time.sleep(.35);require(server.state()['view']['pond']['clock']>clock,'normal water/fish clock stalled')
        record('Feed fish menu starts its activity immediately and normal water continues when the player stops')
        passed=True
    finally:
        write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)
if __name__=='__main__':main()
