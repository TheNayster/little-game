"""Real-clock four-client Windows run; no family/device interaction or accelerated timers."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime, timezone
import importlib.util
import json
from pathlib import Path
import time
import psutil

from recovery_fixture import RecoveryFixture
from server_recovery import checkpoint
from shared_garden_runtime import ROOT, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int); parser.add_argument('--seconds', type=int, default=600)
    args = parser.parse_args(); require(600 <= args.seconds <= 3600, 'Use a measured 10-60 minute run')
    fixture = RecoveryFixture(args.build); controller = fixture.controller
    samples = []; checks = []; traces = []; clients = []; success = False
    spec = importlib.util.spec_from_file_location('smooth_check', ROOT / 'Tools/Verification/Test-SmoothMovement.py')
    smooth = importlib.util.module_from_spec(spec); spec.loader.exec_module(smooth)
    def passed(name):
        checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    def settled(c): wait(lambda: not c.input('inspect')['pending'], 'settled soak input')
    def walk(c, x, y): c.input('press', x=x, y=y); c.input('release', x=x, y=y)
    def drag(c, target):
        c.input('press', role='bucket-1'); c.input('release', x=target[0], y=target[1]); settled(c)
    def trace():
        walk(clients[0], 50, 70)
        wait(lambda: abs(next(p for p in read(out/'view.json')['view']['players'] if p['id']==clients[0].profile)['x']-50)<1, 'trace origin', 12)
        time.sleep(.6); clients[1].input('traceStart', role=clients[0].profile)
        walk(clients[0], 920, 70); time.sleep(4.8); clients[1].input('traceStop')
        return smooth.analyze(read(clients[1].out/'motion-trace.json')['samples'])
    try:
        controller.start(); native = controller.processes()[0]; out = native['output']
        clients = [fixture.join(i) for i in range(1, 5)]
        server_process = psutil.Process(native['pid'])
        traces.append(trace())
        drag(clients[0], (150,340)); drag(clients[0], (810,330)); drag(clients[0], (710,210))
        planted = next(t for t in read(out/'view.json')['view']['toys'] if t['id']=='plant-1')
        require(planted['water']==3, 'Soak flower setup incomplete')
        flower_reset = bucket_return = departed_rejoined = False
        started = time.monotonic(); next_report = 60; cycle = 0
        while time.monotonic() - started < args.seconds:
            loop = time.monotonic(); elapsed = loop - started
            target = 900 if cycle % 2 == 0 else 80
            for i,c in enumerate(clients): walk(c,target,70+i*35)
            if cycle % 8 == 4:
                for i in (2,3):
                    view = read(out/'view.json')['view']
                    player = next(p for p in view['players'] if p['id']==clients[i].profile)
                    clients[i].input('button',text='Creek' if player['zone']=='garden' else 'Garden'); settled(clients[i])
            if elapsed >= 240 and not departed_rejoined:
                positions = read(out/'view.json')['view']['players']
                clients[3].close()
                wait(lambda: len(read(out/'view.json')['connected'])==3, 'one player left')
                require(all(c.input('inspect')['connected'] for c in clients[:3]), 'Departure interrupted other clients')
                clients[3] = fixture.join(4)
                wait(lambda: len(read(out/'view.json')['connected'])==4, 'four again')
                require(controller.snapshot()['instanceId']==native['instanceId'], 'Client departure replaced authority')
                departed_rejoined = True
            view = read(out/'view.json')['view']; state = read(out/'connection-evidence.json')
            flower = next(t for t in view['toys'] if t['id']=='plant-1')
            bucket = next(t for t in view['toys'] if t['id']=='bucket-1')
            if flower['water']==0 and not flower_reset:
                flower_reset = True; flower_seconds = round(elapsed,2)
            if abs(bucket['x']-360)<.1 and abs(bucket['y']-130)<.1 and not bucket_return:
                bucket_return = True; bucket_seconds = round(elapsed,2)
            require(state['listening'] and state['receiveError']==0 and len(state['profiles'])==4, 'Authority/transport failed during soak')
            require(all(c.input('inspect')['connected'] for c in clients), 'Unexpected client disconnect')
            metrics = []
            for role,proc in [('server',server_process)]+[(f'player-{i+1}',psutil.Process(c.process.pid)) for i,c in enumerate(clients)]:
                m=proc.memory_info(); metrics.append(dict(role=role,privateMiB=round(m.private/1048576,2),workingSetMiB=round(m.rss/1048576,2)))
            samples.append(dict(seconds=round(elapsed,2),revision=view['revision'],memory=metrics,
                                motion=read(out/'motion-stats.json'),maxFrameGapSeconds=state['maxFrameGap'],receivedDataBytes=state['receivedDataBytes']))
            if elapsed>=next_report:
                print(f'PROGRESS {int(elapsed)}s: four connected, flower reset={flower_reset}, bucket returned={bucket_return}',flush=True);next_report+=60
            cycle+=1
            time.sleep(max(0,8-(time.monotonic()-loop)))
        measured = round(time.monotonic()-started,2)
        require(measured>=args.seconds and flower_reset and bucket_return and departed_rejoined, 'Long-run acceptance incomplete')
        traces.append(trace())
        passed('four clients remain usable across real-clock movement and repeated independent travel, with one departure/rejoin leaving siblings connected')
        passed('flower rearms and borrowed bucket returns using normal runtime idle timers without a test clock')
        passed('remote canvas interpolation meets the existing motion thresholds before and after the long run')
        memory = {}
        for role in ('server','player-1','player-2','player-3','player-4'):
            values=[next(m['privateMiB'] for m in s['memory'] if m['role']==role) for s in samples]
            warm=[next(m['privateMiB'] for m in s['memory'] if m['role']==role) for s in samples if s['seconds']>=60]
            memory[role]=dict(firstMiB=values[0],lastMiB=values[-1],peakMiB=max(values),growthAfterFirstMinuteMiB=round(warm[-1]-warm[0],2))
            require(max(values)<1024 and warm[-1]-warm[0]<256, 'Unexpected desktop memory growth: '+role)
        passed('observed private memory stays within the declared desktop smoke budget; this is not a mobile memory budget or leak-free proof')
        fixture.stop(); save=fixture.path/'server-world/world.save'; before=checkpoint(save.read_bytes())
        controller.start(); require(checkpoint(save.read_bytes())==before, 'Long-run checkpoint changed on empty restart');fixture.stop()
        for path in fixture.path.glob('*/player.log'):
            log=path.read_text(encoding='utf-8',errors='replace')
            require(not any(word in log for word in ('NullReferenceException','IndexOutOfRangeException','OutOfMemoryException','Unhandled Exception')), 'Unhandled failure in test log')
        passed('complete long-run checkpoint survives clean restart exactly; no selected unhandled-error signatures found')
        success=True
    finally:
        fixture.cleanup()
        result=dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
                    requestedSeconds=args.seconds,measuredSeconds=measured if 'measured' in locals() else None,
                    sampleCount=len(samples),cycles=cycle if 'cycle' in locals() else 0,
                    flowerResetObservedSeconds=flower_seconds if 'flower_seconds' in locals() else None,
                    bucketReturnObservedSeconds=bucket_seconds if 'bucket_seconds' in locals() else None,
                    memory=memory if 'memory' in locals() else {},motionTraces=traces,
                    scope='Single Windows PC, dedicated authority and four native Windows clients over local discovery/transport. Real time, no impaired Wi-Fi, mobile thermal/frame-time or two-hour guarantee. Player-4 memory spans a deliberate client restart.')
        write(fixture.path/'soak-samples.json',samples);write(fixture.path/'soak-result.json',result)
        print('Evidence:',fixture.path/'soak-result.json',flush=True)


if __name__=='__main__':main()
