"""Measure four native clients walking together and one leaving independently."""
import argparse
import importlib.util
from pathlib import Path
import uuid
from shared_garden_runtime import Run, read, write, wait, require

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build',type=int)
    parser.add_argument('--expected-speed',type=float,required=True)
    args=parser.parse_args();run=Run(args.build,extended_test_lifetime=True)
    for slot in run.slots:slot['profile']=uuid.uuid4().hex
    out=run.path/'movement-speed';out.mkdir();metrics=[];passed=False
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        def player(c):return next(p for p in server.state()['view']['players'] if p['id']==c.profile)
        for c in clients:
            home.travel(c,'park');home.ready(c)
            c.input('traceStart',role=c.profile)
            c.input('touchButton',text='Tap to walk')
            c.input('press',role='stick',x=62,y=0)
        wait(lambda:all(player(c)['x']>1750 for c in clients),'four simultaneous walkers',15)
        # Ending one participation must leave three independent controls active.
        first=clients[0];first.input('release',role='stick',x=62,y=0)
        first.input('traceStop')
        before={c.profile:player(c)['x'] for c in clients[1:]}
        home.travel(first,'creek')
        require(player(first)['zone']=='creek','Independent travel failed')
        for c in clients[1:]:
            require(player(c)['zone']=='park' and player(c)['x']>before[c.profile]+30,'Sibling walking interrupted')
            c.input('release',role='stick',x=62,y=0);c.input('traceStop')
        for c in clients:
            trace=read(c.out/'motion-trace.json');write(out/(c.profile+'.json'),trace)
            samples=[s for s in trace['samples'] if 650<s['visual']['x']<1400]
            require(len(samples)>20,'Insufficient movement samples')
            elapsed=samples[-1]['time']-samples[0]['time']
            speed=(samples[-1]['visual']['x']-samples[0]['visual']['x'])/elapsed
            backward=min(b['visual']['x']-a['visual']['x'] for a,b in zip(samples,samples[1:]))
            require(abs(speed-args.expected_speed)<args.expected_speed*.1 and backward>-.5,'Unexpected speed or backward correction')
            metrics.append(dict(player=c.profile,samples=len(samples),measuredSpeed=round(speed,2),worstBackwardStep=round(backward,3)))
        passed=True;print('PASS four shared walkers at requested speed; independent travel preserves sibling movement',flush=True)
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,expectedSpeed=args.expected_speed,metrics=metrics,liveFamilyTouched=False))
        print('EVIDENCE '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
