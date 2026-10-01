# /// script
# dependencies = ["cryptography"]
# ///
"""Shared cover capacity, transparent co-hiders, one inspection and independent exit."""
import argparse,importlib.util,time
from pathlib import Path
import shared_garden_runtime as runtime
from shared_garden_runtime import Run,wait,require,write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--snapshot',type=Path);args=p.parse_args()
    if args.snapshot:runtime.ROOT=args.snapshot.resolve()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'shared-cover';out.mkdir();checks=[]
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def state():return server.state()['view']['hideAndSeek']
        def hider(v):return next(h for h in state()['hiders'] if h['actor']==v.profile)
        def send(v,act,**kw):
            reply=home.command(v,act,**kw);require(reply['accepted'],'Command rejected: '+str(reply));home.ready(v)
        for v,avatar in zip(clients,('blue-pup','orange-pup','muffin','socks')):home.ready(v);send(v,1,value=avatar);send(v,0,x=-3560,y=50)
        a.input('resize',x=1280,y=591);b.input('resize',x=1024,y=768);home.ready(a);home.ready(b)
        a.input('touchButton',text='Games');a.input('touchButton',text='Hide & seek');wait(lambda:state()['phase']==1,'shared hiding window')
        for v in clients:send(v,20,target='3',value='hide')
        require(all(h['mode']==2 and h['slot']==3 for h in state()['hiders']),'Four cannot share the wardrobe')
        for v in clients:
            view=v.input('inspect');expected={'Friend-'+p.profile for p in clients if p!=v};require(expected<=set(view['homeDrawOrder']) and not view['menuOpen'],'Co-hiders are not visible directly in the shared cover')
        home.capture(a,out,'four-sharing-phone');home.capture(b,out,'four-sharing-ipad')
        checks.append('All four share an existing cover and see all four children through the transparent cover at phone/iPad sizes')
        # A different hiding area remains private, and a voluntary exit releases
        # only its own occupancy; the remaining two share the same round.
        send(d,0,x=4310,y=50);send(d,20,target='9',value='hide');send(c,20,value='leave')
        wait(lambda:'Friend-'+b.profile in a.input('inspect')['homeDrawOrder'],'transparent co-hider remains after departure')
        isolated=d.input('inspect');require('Friend-'+a.profile not in isolated['homeDrawOrder'] and 'Friend-'+b.profile not in isolated['homeDrawOrder'],'Other cover reveals hidden children')
        require(hider(c)['mode']==0 and hider(a)['mode']==2 and hider(b)['mode']==2,'Departure interrupted siblings')
        checks.append('Same-cover visibility is private; moving or leaving preserves the other co-hiders')
        wait(lambda:hider(a)['mode']==3 or hider(b)['mode']==3,'shared wardrobe inspection',23)
        require(hider(a)['mode']==3 and hider(b)['mode']==3 and hider(d)['mode']==2,'Parent inspection did not find both wardrobe occupants together')
        wait(lambda:state()['x']>0,'parent continues toward remaining hider',10)
        view=a.input('inspect');require(abs(view['cameraX']-view['hideAndSeek']['x'])<350,'Found co-hiders stopped watching parent')
        wait(lambda:hider(d)['mode']==3,'distant remaining hider found',15)
        require(hider(c)['mode']==0,'Departed child found')
        checks.append('One physical inspection finds all occupants together; parent camera continues through the remaining search')
        write(out/'result.json',dict(passed=True,build=args.build,content=run.content,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        for check in checks:print('PASS '+check,flush=True)
    finally:run.close()

if __name__=='__main__':main()
