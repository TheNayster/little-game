# /// script
# dependencies = ["cryptography"]
# ///
"""Repeated real food taps, faster approaches and a shared four-player queue."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, math, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'zoo-repeat-food';out.mkdir();checks=[];passed=False
    def state():return server.state()['view']['zoo']
    def animal():return state()['animals'][0]
    def portion(v):return next(f for f in state()['food'] if f['actor']==v.profile)
    def cmd(v,action,**kw):
        result=home.command(v,action,**kw);require(result['accepted'],str(result));home.ready(v)
    def tap(v):v.input('touchButton',text='Take leaves for elephant')
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        for i,v in enumerate(clients):
            home.ready(v);v.input('resize',x=1280 if i%2==0 else 1024,y=591 if i%2==0 else 768)
            cmd(v,7,value='zoo');cmd(v,0,x=650,y=100);cmd(v,22,value='gate',target='zoo-savanna');cmd(v,0,x=1200,y=100)
        a,b,c,d=clients;time.sleep(.8);before=animal()['fed'];tap(a);tap(a)
        ticket=None;observed=set();until=time.monotonic()+30
        while not portion(a)['offered'] and time.monotonic()<until:
            f=portion(a)
            if f['species']:
                if ticket is None:ticket=f['ticket']
                require(f['ticket']==ticket,'Tap replaced reserved food');observed.add('walking-to-offer')
            else:observed.add('walking-to-bucket')
            tap(a);time.sleep(.12)
        f=portion(a);require(f['offered'] and f['species']=='elephant','Repeated taps stopped automatic offering')
        ticket=f['ticket'];slot=f['slot']
        for phase,label in [(5,'approaching'),(6,'eating')]:
            wait(lambda:animal()['phase']==phase and animal()['owner']==a.profile,label,25)
            if phase==5:
                snapshot=animal();distance=math.hypot(snapshot['toX']-snapshot['fromX'],snapshot['toY']-snapshot['fromY'])
                expected=max(1.5,distance/(42*1.4))
                require(abs(snapshot['duration']-expected)<.002,'Authority did not apply faster approach')
                write(out/'approach.json',dict(distance=distance,duration=snapshot['duration'],previousDuration=max(2,distance/42),speedMultiplier=1.4))
            tap(a);tap(a);f=portion(a)
            require(f['species']=='elephant' and f['ticket']==ticket and f['slot']==slot and f['offered'],'Repeat tap canceled '+label)
            observed.add(label)
        home.capture(a,out,'feeding-after-repeat-taps')
        wait(lambda:animal()['fed']==before+1 and portion(a)['species']=='','exactly one completed portion',12)
        require({'walking-to-bucket','walking-to-offer'}<=observed,'Missed walking stage coverage')
        record('repeat taps during both walks, approach and eating preserve one portion; authority approaches 40% faster')
        before=animal()['fed']
        for v in clients:cmd(v,0,x=1560,y=100)
        time.sleep(.6)
        for v in clients:tap(v);tap(v)
        wait(lambda:all(portion(v)['offered'] for v in clients),'four food offers',20)
        tickets={v.profile:portion(v)['ticket'] for v in clients}
        require(len({portion(v)['slot'] for v in clients})==4,'Offering places overlap')
        for v in clients:
            tap(v);require(portion(v)['ticket']==tickets[v.profile] and portion(v)['offered'],'Repeat tap changed shared queue')
        home.capture(d,out,'four-player-repeat-taps')
        cmd(b,7,value='creek');require(portion(b)['species']=='','Leaving retained canceled food')
        require(all(portion(v)['ticket']==tickets[v.profile] for v in (a,c,d)),'Departure changed sibling queue')
        wait(lambda:animal()['fed']==before+3 and all(portion(v)['species']=='' for v in (a,c,d)),'siblings finish once',70)
        record('four-player repeat taps preserve tickets/slots; one leaves and three siblings complete once')
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,observedStages=sorted(locals().get('observed',[])),liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
