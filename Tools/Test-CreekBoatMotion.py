# /// script
# dependencies = ["cryptography"]
# ///
"""Four-player boat motion regression, measuring actual native UI frames."""
import argparse,importlib.util,time
from pathlib import Path
import shared_garden_runtime as runtime
from shared_garden_runtime import Run,wait,require,write,read
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def check_trace(client,folder,name,moving):
    client.input('boatTraceStop');trace=read(client.out/'boat-trace.json');samples=trace['samples'];rates=[];max_speed=0
    moving_pairs=[(a,b) for a,b in zip(samples,samples[1:]) if a['phase']==b['phase']==moving and .001<b['time']-a['time']<.15]
    require(len(moving_pairs)>50,'Too few per-frame moving samples')
    require(all(b['clock']>a['clock'] for a,b in moving_pairs),'Boat visual clock froze or reversed')
    # Script LateUpdate order can vary by milliseconds. Measure rate across
    # 100 ms spans, while checking every individual frame for a freeze.
    for index,first in enumerate(samples):
        if first['phase']!=moving:continue
        for last in samples[index+1:]:
            dt=last['time']-first['time']
            if dt<.1:continue
            if dt<.2 and last['phase']==moving:
                rates.append((last['clock']-first['clock'])/dt)
                max_speed=max(max_speed,abs(last['position']['x']-first['position']['x'])/dt)
            break
    require(len(rates)>50,'Too few moving spans')
    require(min(rates)>.85 and max(rates)<1.15,'Boat visual clock froze or snapped')
    # Retrieval covers the same route in five seconds, so its speed is higher.
    require(max_speed<(420 if moving==3 else 160),'Boat drawing jumped in world space')
    result=dict(samples=len(moving_pairs),movingSpans=len(rates),frozenFrames=0,minimumClockRate=min(rates),maximumClockRate=max(rates),maximumWorldSpeed=max_speed)
    write(folder/(name+'.json'),result);print('PASS '+name+' '+str(result),flush=True)
    return result

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--source-root',type=Path);args=parser.parse_args()
    if args.source_root:runtime.ROOT=args.source_root.resolve()
    run=Run(args.build)
    folder=run.path/'boat-motion';folder.mkdir();passed=False;checks={};print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def boat(client):return next(v for v in server.state()['view']['creekBoats']['boats'] if v['actor']==client.profile)
        def tap(client,name):client.input('touchButton',text=name);home.ready(client)
        for client in clients:home.travel(client,'creek');tap(client,'Boats');wait(lambda:boat(client)['attending'],'bank attendance')
        a.input('resize',x=1280,y=591);home.ready(a);b.input('resize',x=1024,y=768);home.ready(b)
        a.input('boatTraceStart',role=a.profile);b.input('boatTraceStart',role=b.profile)
        for client in clients:tap(client,'Launch boat');wait(lambda:boat(client)['phase']==1,'shared launch')
        time.sleep(2);home.capture(a,folder,'sailing-phone');home.capture(b,folder,'sailing-ipad')
        d.close();wait(lambda:not boat(d)['attending'],'independent departure');require(boat(a)['attending'] and boat(b)['attending'] and boat(c)['attending'],'Departure stopped sibling play')
        wait(lambda:all(boat(client)['phase']==2 for client in clients),'all boats dock',25)
        checks['phone-voyage']=check_trace(a,folder,'phone-voyage',1);checks['ipad-voyage']=check_trace(b,folder,'ipad-voyage',1)
        require(all(boat(client)['trips']==1 for client in clients),'Departure prevented arrival')
        a.input('boatTraceStart',role=a.profile);tap(a,'Bring boat back');wait(lambda:boat(a)['phase']==0,'retrieve',10)
        checks['phone-retrieval']=check_trace(a,folder,'phone-retrieval',3)
        require(boat(b)['phase']==2 and boat(c)['phase']==2,'Retrieval reset sibling boat');home.capture(a,folder,'returned-phone');passed=True
    finally:write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)
if __name__=='__main__':main()
