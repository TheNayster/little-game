"""Measure tap/joystick canvas walking in an isolated offline adventure."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime, timezone
import hashlib
import math
from pathlib import Path
import time
import psutil
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import read, write, wait, require


def measure(samples):
    pairs = [(a, b) for a, b in zip(samples, samples[1:])
             if 200 < a['visual']['x'] < 700 and 200 < b['visual']['x'] < 700]
    require(len(pairs) >= 20, 'Insufficient moving canvas samples')
    dt = [b['time'] - a['time'] for a, b in pairs]
    dx = [b['visual']['x'] - a['visual']['x'] for a, b in pairs]
    require(all(t > 0 for t in dt), 'Invalid motion clock')
    speeds = sorted(d / t for d, t in zip(dx, dt))
    return dict(samples=len(pairs), movingFrameFraction=sum(d > .05 for d in dx) / len(dx),
                meanSpeed=sum(dx) / sum(dt), p95Speed=speeds[int((len(speeds)-1)*.95)],
                worstBackwardStep=min(dx), medianFrameMs=sorted(dt)[len(dt)//2]*1000)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    parser.add_argument('--observe', action='store_true', help='Record a failing baseline without claiming a pass')
    parser.add_argument('--fps', type=int, choices=(30, 60), help='Explicit test frame target; requires build 92 or later')
    parser.add_argument('--expected-speed', type=float, default=210, help='Requested gameplay speed for this build')
    args = parser.parse_args()
    fixture = RecoveryFixture(args.build); checks = []; error = None
    def inspect(): return client.input('inspect')
    def point():
        return next(p['position'] for p in inspect()['players'] if p['id'] == client.profile)
    # Board input uses logical coordinates when role is omitted.
    def walk_to(x, y):
        client.input('press', x=x, y=y); client.input('release', x=x, y=y)
        wait(lambda: math.hypot(point()['x']-x, point()['y']-y) < 2, 'local destination', 15)
    try:
        fixture.controller.start(); client = fixture.join(1)
        wait(lambda: (fixture.path/'client-recovery'/client.profile/'world.save').exists(), 'full replica', 45)
        native = fixture.controller.processes()[0]
        psutil.Process(native['pid']).terminate()
        wait(lambda: not fixture.controller.processes(), 'isolated authority exit')
        wait(lambda: (v := inspect())['adventure'] and not v['shared'], 'offline adventure', 45)
        if args.fps: client.input('frameRate', x=args.fps)
        # Fresh test profiles default to tap walking; both controls use actual UGUI input.
        for mode in ('tap', 'joystick'):
            walk_to(110, 220)
            client.input('traceStart', role=client.profile)
            if mode == 'tap':
                # Keep the destination inside the current scenic camera's
                # viewport; the old lab's x=860 target is now off-screen.
                client.input('press', x=780, y=220); client.input('release', x=780, y=220)
            else:
                client.input('button', text='Tap to walk')
                client.input('press', role='stick', x=62, y=0)
            wait(lambda: point()['x'] > 750, mode+' walk', 15)
            if mode == 'joystick': client.input('release', role='stick', x=62, y=0)
            client.input('traceStop')
            trace = read(client.out/'motion-trace.json')
            write(fixture.path/(mode+'-motion.json'), trace)
            metrics = measure(trace['samples'])
            passed = (metrics['movingFrameFraction'] > .9 and args.expected_speed * (190/210) < metrics['meanSpeed'] < args.expected_speed * (230/210)
                      and metrics['p95Speed'] < args.expected_speed * (260/210) and metrics['worstBackwardStep'] > -.1)
            checks.append(dict(control=mode, passed=passed, **metrics))
            print(mode, checks[-1], flush=True)
        client.input('button', text='Menu')
        require(inspect()['menuOpen'], 'Menu did not open')
        stopped = point(); time.sleep(.3)
        after = point()
        # Scenic world-to-canvas projection introduces subpixel float noise
        # while the camera settles, even when the character has stopped.
        require(math.hypot(after['x']-stopped['x'], after['y']-stopped['y']) < .01,
                'Menu failed to cancel local walking')
    except Exception as exc:
        error = str(exc)
        raise
    finally:
        fixture.cleanup()
        passed = error is None and len(checks) == 2 and all(c['passed'] for c in checks)
        result = dict(passed=passed, build=args.build, utc=datetime.now(timezone.utc).isoformat(),
                      checks=checks, error=error, observeOnly=args.observe, requestedFrameRate=args.fps, expectedSpeed=args.expected_speed,
                      harnessSha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
                      scope='Native Windows canvas/input in one isolated offline adventure; not measured A10 smoothness.')
        write(fixture.path/'local-motion-result.json', result)
        print('Evidence:', fixture.path/'local-motion-result.json', flush=True)
    require(passed or args.observe, 'Local movement did not meet frame cadence/speed checks')


if __name__ == '__main__': main()
