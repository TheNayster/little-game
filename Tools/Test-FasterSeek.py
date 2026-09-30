# /// script
# dependencies = ["cryptography"]
# ///
"""Focused four-client seeker pacing and independent-departure check."""
import argparse, importlib.util, time
from pathlib import Path
import shared_garden_runtime as runtime
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--snapshot',type=Path);args=p.parse_args()
    if args.snapshot:runtime.ROOT=args.snapshot.resolve()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'faster-seek';out.mkdir();checks=[]
    print('EVIDENCE '+str(out),flush=True)
    def send(c,action,**kw):require(home.command(c,action,**kw)['accepted'],'Hide command rejected')
    def passed(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def state():return server.state()['view']['hideAndSeek']
        def hider(v):return next(h for h in state()['hiders'] if h['actor']==v.profile)
        for v,x in zip(clients,[-3870,4310,3150,-470]):home.ready(v);send(v,0,x=x,y=50)
        a.input('resize',x=1280,y=591);c.input('resize',x=1024,y=768);home.ready(a);home.ready(c)
        # Start through the real Home mini-game card, then opt all four into cover.
        a.input('touchButton',text='Games');a.input('touchButton',text='Hide & seek')
        wait(lambda:state()['phase']==1,'common hiding window');round_id=state()['round']
        require(state()['count']>13,'Fifteen-second window shortened')
        for v,slot in zip(clients,[2,9,8,7]):send(v,20,target=str(slot),value='hide')
        require(all(h['mode']==2 for h in state()['hiders']),'Not four simultaneous hiders')
        wait(lambda:state()['phase']!=1,'hiding deadline',20);started=time.monotonic()
        send(d,20,value='leave');require(hider(d)['mode']==0 and hider(b)['mode']==2 and hider(c)['mode']==2 and state()['round']==round_id,'Departure restarted search')
        passed('All four share the fifteen-second hiding window; one departure preserves the others')
        wait(lambda:state()['target']==7 and state()['phase']==3,'parent heading through dining room',10)
        home.capture(b,out,'faster-search-desktop');home.capture(c,out,'faster-search-ipad')
        wait(lambda:all(hider(v)['mode']==3 for v in (a,b,c)),'remaining hiders found',22)
        elapsed=time.monotonic()-started;mask=state()['visited']
        require(elapsed<22,'Search still slow')
        require(all(mask&(1<<i) for i in (2,3,4,5,7,8,9)) and not any(mask&(1<<i) for i in (0,1,6)),'Route skipped intermediate covers or detoured away')
        require(hider(d)['mode']==0,'Departed player found');passed('Parent heads toward distant hiders and inspects covers along the way')
        write(out/'result.json',dict(passed=True,build=args.build,checks=checks,searchSeconds=round(elapsed,2),visitedMask=mask,content=run.content,physicalDevicesTested=False,liveFamilyTouched=False))
    finally:run.close()

if __name__=='__main__':main()
