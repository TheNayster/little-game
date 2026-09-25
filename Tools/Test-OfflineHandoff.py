"""Exercise outage handoff with held input, an open menu and an older authority."""
import argparse
from datetime import datetime, timezone
from pathlib import Path
import time
import json
import psutil
from recovery_fixture import RecoveryFixture
from parent_server import ParentServer, checkpoint_bytes
from shared_garden_runtime import wait, require, write


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('build', type=int)
    p.add_argument('--observe', action='store_true')
    p.add_argument('--scenario', choices=('held-touch','open-menu','no-checkpoint'))
    args = p.parse_args()
    checks = []
    for scenario in ([args.scenario] if args.scenario else ('held-touch', 'open-menu', 'no-checkpoint')):
        f = RecoveryFixture(args.build)
        if scenario == 'no-checkpoint':
            f.controller = ParentServer(f.run_id, 85)
        error = None
        try:
            f.controller.start(); c = f.join(1)
            def inspect(): return c.input('inspect')
            if scenario != 'no-checkpoint':
                wait(lambda: (f.path/'client-recovery'/c.profile/'world.save').exists(), 'full checkpoint', 50)
            solo = Path(inspect()['savePath']); original = checkpoint_bytes(solo)[0]
            if scenario == 'held-touch':
                c.input('button', text='Tap to walk')
                c.input('touch-begin', role='stick', finger=11, x=60)
            elif scenario == 'open-menu':
                c.input('button', text='Menu')
            native = f.controller.processes()[0]
            psutil.Process(native['pid']).terminate()
            wait(lambda: not f.controller.processes(), 'isolated authority exit')
            start = time.monotonic()
            disconnected_at = None
            while time.monotonic()-start < 22:
                state = inspect()
                if not state['connected'] and disconnected_at is None: disconnected_at = time.monotonic()
                if not state['shared']: break
                time.sleep(.15)
            require(not state['shared'], 'Controls remain bound to a disconnected authority')
            local_at = time.monotonic()
            require(local_at - start < 5, 'Authority loss kept local play unavailable for five seconds')
            require(disconnected_at is not None and local_at - disconnected_at < 2,
                    'Known disconnection still imposed a local-play delay')
            require(bool(state['adventure']) == (scenario != 'no-checkpoint'), 'Wrong local save selected')
            if scenario == 'open-menu':
                require(state['menuOpen'], 'Recovery unexpectedly closed the menu')
                c.input('button', text='Back to play')
            if scenario == 'held-touch':
                c.input('touch-end', role='stick', finger=11, x=60)
            c.input('press', role='bucket-1'); c.input('release', x=720, y=200)
            require(not inspect()['pending'] and not inspect()['dragging'], 'Offline controls remain stuck')
            def saved_drop():
                saved=json.loads(checkpoint_bytes(Path(inspect()['savePath']))[0].split(b'\n',2)[2])
                world=saved.get('snapshot',saved)
                return any(t['id']=='bucket-1' and abs(t['x']-720)<.1 and not t['holder'] for t in world['toys'])
            wait(saved_drop, 'offline drag actually saved', 10)
            if scenario != 'no-checkpoint':
                require(checkpoint_bytes(solo)[0] == original, 'Handoff overwrote original solo')
            f.controller.start()
            wait(lambda: inspect()['shared'] and inspect()['connected'], 'automatic reunion', 55)
        except Exception as exc:
            error = str(exc)
        finally:
            f.cleanup()
            result = dict(build=args.build, scenario=scenario, passed=error is None, error=error,
                          utc=datetime.now(timezone.utc).isoformat(), observeOnly=args.observe,
                          scope='Isolated Windows DTLS authority loss and real UGUI touch. Not physical Wi-Fi loss.')
            checks.append(result); write(f.path/'offline-handoff-result.json', result)
            print(result, 'Evidence:', f.path/'offline-handoff-result.json', flush=True)
    require(args.observe or all(c['passed'] for c in checks), 'Offline handoff failed')


if __name__ == '__main__': main()
