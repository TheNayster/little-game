# /// script
# dependencies = ["cryptography"]
# ///
"""Check the separate hide invitation/countdown during a live Home round."""
import argparse, importlib.util, time
from pathlib import Path
import shared_garden_runtime as runtime
from shared_garden_runtime import Run,wait,require,write,read

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--snapshot',type=Path);args=p.parse_args()
    if args.snapshot:runtime.ROOT=args.snapshot.resolve()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'home-only-hide';out.mkdir();checks=[]
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');a=run.start('client','player-1');b=run.start('client','player-2')
        def state():return server.state()['view']['hideAndSeek']
        def send(v,act,**kw):
            reply=home.command(v,act,**kw);require(reply['accepted'],'Command rejected: '+str(reply));home.ready(v)
        def names(v):return {c['name'] for c in v.input('inspect')['controls']}
        def start():
            if state()['phase']==1:wait(lambda:state()['phase']==0,'empty window finishes',17)
            a.input('touchButton',text='Games');a.input('touchButton',text='Hide & seek');home.ready(a)
            wait(lambda:state()['phase']==1,'Home hiding window')
        a.input('resize',x=1280,y=591);b.input('resize',x=1024,y=768)
        home.ready(a);home.ready(b);start()
        require('Join family hide and seek' in names(b),'Home invitation missing')
        b.input('touchButton',text='Join family hide and seek');home.ready(b)
        require('Play hide and seek' in names(b),'Home play card missing')
        send(b,7,value='daycare')
        require('Play hide and seek' not in names(b),'Home play card survived travel')
        checks.append('Home invitation works and its open card closes when leaving Home')
        # Some isolated earlier source baselines predate the Zoo destination.
        source=runtime.ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/WorldLayout.cs'
        worlds=('daycare','park','creek','beach')+(('zoo',) if 'ZooLayout.Area' in source.read_text(encoding='utf-8') else ())
        for world in worlds:
            if state()['phase']!=1 or state()['count']<4:start()
            if world!='daycare':send(b,7,value=world)
            s=b.input('inspect');controls={c['name'] for c in s['controls']}
            require(state()['phase']==1,'Cross-world check needs an active countdown')
            require(not controls & {'Join family hide and seek','Play hide and seek','Come out','All done'},'Home controls in '+world)
            require(not any(t.strip().isdigit() and 1<=int(t.strip())<=15 for t in s['visibleText']),'Home countdown in '+world)
            require(not any('hide' in t.lower() or 'counting!' in t.lower() for t in s['visibleText']),'Home prompt in '+world)
            b.input('touchButton',text='Games');home.ready(b);controls=names(b)
            require(not controls & {'Hide & seek','Fishing','Feed fish'},'Home game cards in '+world)
            require('Back to play' in controls,'Games menu missing in '+world)
            b.input('touchButton',text='Back to play');home.ready(b)
            checks.append('No Home cards, invitation or countdown during active Home round: '+world)
            print('PASS '+checks[-1],flush=True)
            if world=='daycare':home.capture(b,out,'daycare-during-home-countdown')
        # Home still owns the running round; traveling away does not restart it.
        if state()['phase']!=1 or state()['count']<3:start()
        send(b,7,value='home');require('Join family hide and seek' in names(b),'Invitation missing on return Home')
        home.capture(a,out,'home-countdown')
        c=run.start('client','player-3');d=run.start('client','player-4')
        clients=(a,b,c,d)
        if state()['phase']==1:wait(lambda:state()['phase']==0,'previous empty window ends',17)
        for v,x in zip(clients,(-3870,4310,3150,-470)):send(v,0,x=x,y=50)
        start()
        for v,slot in zip(clients,(2,9,8,7)):send(v,20,target=str(slot),value='hide')
        def hider(v):return next(h for h in state()['hiders'] if h['actor']==v.profile)
        wait(lambda:hider(a)['mode']==3 and state()['x']>-1500,'first found child watches distant search',28)
        home.ready(a);view=a.input('inspect')
        require(hider(b)['mode']==2 and hider(a)['mode']==3,'Camera check needs found and hidden siblings')
        require(abs(view['cameraX']-view['hideAndSeek']['x'])<350 and view['cameraX']>-1800,'Found child camera returned to their hiding spot')
        home.capture(a,out,'found-child-watching-parent')
        # A voluntary exit releases only that spectator; the others keep following.
        send(d,20,value='leave');wait(lambda:hider(d)['mode']==0,'independent departure')
        require(hider(b)['mode']==2 and hider(c)['mode']==2,'Departure disturbed hidden siblings')
        wait(lambda:state()['phase']==0 and all(hider(v)['mode']==3 for v in (a,b,c)),'full search finishes',24)
        home.ready(a);view=a.input('inspect');require(view['cameraX']<-3000,'Camera did not return after full round')
        checks.append('Found hiders follow the parent until the full round ends; departure is independent and normal camera returns afterward')
        print('PASS '+checks[-1],flush=True)
        write(out/'result.json',dict(passed=True,build=args.build,checks=checks,content=run.content,worldsTested=list(worlds),liveFamilyTouched=False,physicalDevicesTested=False))
    finally:run.close()

if __name__=='__main__':main()
