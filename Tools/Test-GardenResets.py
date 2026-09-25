"""Real-time reset replication and wide-layout checks on isolated Windows clients."""
import argparse
from datetime import datetime, timezone
import time
from shared_garden_runtime import Run, wait, require, write


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build',type=int)
    parser.add_argument('--layout-only',action='store_true',help='Qualify layout-only follow-up changes without repeating the three-minute timer check')
    args=parser.parse_args()
    run=Run(args.build);checks=[]
    def passed(name,**data):
        checks.append(dict(check=name,passed=True,**data));print('PASS '+name,flush=True)
    try:
        server=run.start('server');first=run.start('client','player-1');second=run.start('client','player-2')
        def toy(client,name):return next(t for t in client.state()['view']['toys'] if t['id']==name)
        def settled(client):
            def idle():
                value=client.input('inspect');return not value['pending'] and not value['dragging']
            wait(idle,'settled input')
        def drag(client,name,x,y):
            client.input('press',role=name);client.input('release',x=x,y=y);settled(client)
        layouts=[]
        for name,width,height in [('tablet',1024,768),('phone',1560,720),('phone-short',1280,600),('tablet-return',1024,768)]:
            first.input('resize',x=width,y=height)
            sample=first.input('inspect');require(sample['controlsInSafeArea'],'Control outside safe area')
            require(sample['screenWidth']==width and sample['screenHeight']==height,'OS changed test size')
            board=sample['boardBounds'];fraction=board['width']/sample['safeArea']['width']
            if name.startswith('phone'):require(fraction>.92,'Phone floor is still pillarboxed')
            else:require(abs(sample['boardLayoutWidth']-1120)<.1,'Tablet layout changed')
            first.input('capture');wait(lambda:(first.out/'garden.png').exists(),'capture')
            (run.path/(name+'.png')).write_bytes((first.out/'garden.png').read_bytes())
            layouts.append(dict(name=name,width=width,height=height,boardFraction=fraction,controlsInSafeArea=True,boardLayoutWidth=sample['boardLayoutWidth']))
        passed('wide phone floor uses over 92 percent of safe width; tablet geometry remains 1120',layouts=layouts)
        if args.layout_only:
            write(run.path/'result.json',dict(passed=True,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
                scope='Layout only; no timer retest in this run.',physicalDevicesAccessed=False,existingSaveOrLiveServerModified=False))
            print('Evidence: '+str(run.path),flush=True)
            return
        first.input('resize',x=1560,y=720)
        drag(first,'bucket-1',150,340);drag(first,'bucket-1',810,330)
        for _ in range(3):drag(second,'sponge-1',680,140)
        require(toy(server,'plant-1')['water']==3 and toy(server,'puddle-1')['water']==0,'Activities did not complete')
        # Refill after the pour so the bucket's three-minute idle deadline is clear.
        drag(first,'bucket-1',150,340);tool_start=time.monotonic()
        initial_ids=[t['id'] for t in server.state()['view']['toys']]
        wait(lambda:toy(second,'plant-1')['resetPending'],'flower repeat cue',seconds=70)
        require(toy(second,'plant-1')['water']==3,'Flower reset before cue')
        passed('completed flower broadcasts a visible cue before rearming')
        wait(lambda:toy(first,'plant-1')['water']==0 and toy(second,'puddle-1')['water']==3,'both activities rearmed',seconds=15)
        passed('flower and cleanup rearm on both clients after the real one-minute grace')
        # A different area's held item must survive the garden's housekeeping.
        second.input('button',text='Creek');settled(second)
        second.input('press',role='bucket-creek');wait(lambda:toy(server,'bucket-creek')['holder']==second.profile,'creek hold')
        wait(lambda:toy(first,'bucket-1')['resetPending'],'tool return cue',seconds=125)
        require(toy(server,'bucket-1')['water']==3,'Tool contents changed before return')
        first.input('capture');(run.path/'tool-cue.png').write_bytes((first.out/'garden.png').read_bytes())
        passed('unused tool broadcasts its cue after the real three-minute grace',elapsed=round(time.monotonic()-tool_start,2))
        wait(lambda:toy(first,'bucket-1')['x']==360 and toy(first,'bucket-1')['y']==130 and toy(first,'bucket-1')['water']==0,'same tool returns',seconds=12)
        require([t['id'] for t in server.state()['view']['toys']]==initial_ids,'Item identity/supply changed')
        require(toy(server,'bucket-creek')['holder']==second.profile,'Reset interrupted sibling in Creek')
        passed('same bucket returns on both clients without disturbing the sibling hold',elapsed=round(time.monotonic()-tool_start,2))
        second.input('release',role='bucket-creek');settled(second)
        drag(first,'bucket-1',150,340);drag(first,'bucket-1',810,330)
        require(toy(server,'plant-1')['water']==3,'Second garden round failed')
        passed('watering plays again after automatic reset')
        write(run.path/'result.json',dict(passed=True,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
              physicalDevicesAccessed=False,clockAcceleration=False,existingSaveOrLiveServerModified=False))
        print('Evidence: '+str(run.path),flush=True)
    except Exception as error:
        write(run.path/'result.json',dict(passed=False,build=args.build,checks=checks,error=str(error)))
        print('FAIL '+str(error)+'; evidence: '+str(run.path),flush=True);raise
    finally:run.close()


if __name__=='__main__':main()
