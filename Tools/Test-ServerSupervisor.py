"""Bounded policy plus actual four-client crash/rejoin in a disposable enrolled world."""
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import json
import os
import signal
import subprocess
import sys
import time
from unittest.mock import patch

from parent_server import OperationError, operation_lock
from recovery_fixture import RecoveryFixture
from server_recovery import Recovery, digest, checkpoint
from server_supervisor import Supervisor
from shared_garden_runtime import ROOT, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int)
    args = parser.parse_args(); fixture = RecoveryFixture(args.build)
    controller = fixture.controller; checks = []; success = False; watcher = None
    clock = [time.time()]
    supervisor = Supervisor(controller, clock=lambda: clock[0])
    save = fixture.path / 'server-world/world.save'
    def passed(name):
        checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    try:
        require(supervisor.tick()['status'] == 'paused' and not controller.processes(), 'Helper adopted an unenrolled run intent')
        controller.start(); clients = [fixture.join(i) for i in range(1, 5)]
        clients[0].input('press', role='bucket-1'); clients[0].input('release', x=730, y=160)
        clients[1].input('button', text='Creek')
        wait(lambda: all(not c.input('inspect')['pending'] for c in clients[:2]), 'settled shared changes')
        watcher = subprocess.Popen([sys.executable, str(ROOT / 'Tools/Server-Supervisor.py'), '--family', fixture.run_id, '--build', str(args.build)],
                                   stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                                   creationflags=subprocess.CREATE_NO_WINDOW)
        wait(lambda: read(supervisor.path) and read(supervisor.path)['status'] == 'healthy', 'real supervisor loop healthy')
        before = checkpoint(save.read_bytes()); identities = [(c.process.pid, c.identity) for c in clients]
        native = controller.processes()[0]
        require(native['output'].parent == fixture.path and native['build'] == args.build, 'Kill target is not this disposable fixture')
        start_time = time.monotonic(); os.kill(native['pid'], signal.SIGTERM)
        wait(lambda: not controller.processes(), 'test authority actually exits')
        wait(lambda: read(supervisor.path)['status'] == 'waiting', 'real supervisor backoff')
        wait(lambda: controller.snapshot()['players'] == 4 and all(c.input('inspect')['connected'] for c in clients), 'four existing clients automatically rejoin', 60)
        elapsed = round(time.monotonic() - start_time, 2)
        after = checkpoint(save.read_bytes()); replacement = controller.snapshot()['instanceId']
        require(replacement != native['instanceId'] and len(controller.processes()) == 1 and identities == [(c.process.pid, c.identity) for c in clients], 'Duplicate host or relaunched client')
        require(all(before[key] == after[key] for key in ('worldId', 'players', 'toys', 'receipts')), 'Saved state differs after actual process loss')
        clients[2].input('press', role='sponge-1'); clients[2].input('release', x=570, y=210)
        wait(lambda: not clients[2].input('inspect')['pending'], 'play after recovery')
        passed('actual native authority termination recovers one enrolled server and all four existing clients rejoin with identities/items/receipts preserved and usable interactions')
        fixture.stop()
        wait(lambda: read(supervisor.path)['status'] == 'paused', 'real supervisor respects parent stop')
        watcher.terminate(); watcher.wait(timeout=15); watcher = None
        clock[0] = time.time() + 100
        require(supervisor.tick()['status'] == 'paused' and not controller.processes(), 'Deliberate stop was undone')
        passed('parent guarded Stop persists paused recovery and the helper leaves the stopped world alone')
        controller.start(); fixture.stop()
        controller.save_intent(True); stale = read(fixture.path / 'server-intent.json')
        controller.pause_recovery()
        try: controller.start(expected_intent=stale)
        except OperationError: pass
        else: raise AssertionError('Stale automatic start undid parent preference')
        require(not controller.processes(), 'Stale preference started a process')
        with operation_lock(fixture.path, 'supervisor.lock'):
            process = subprocess.run([sys.executable, str(ROOT / 'Tools/Server-Supervisor.py'), '--family', fixture.run_id, '--build', str(args.build)], capture_output=True, timeout=20)
            require(process.returncode != 0, 'Second helper acquired supervisor lock')
        passed('stale restart intent is refused inside parent-operation lock and a duplicate supervisor process exits')
        controller.save_intent(True)
        # Verify policy without killing/rebuilding dozens of native authorities.
        stopped = controller.snapshot(); require(stopped['state'] == 'stopped', 'Expected stopped fixture')
        attempts = []
        def failed_start(**kwargs):
            attempts.append(kwargs)
            if len(attempts) == 2: raise subprocess.TimeoutExpired('isolated launch fixture', 65)
            raise OperationError('test launch failure')
        with patch.object(controller, 'start', side_effect=failed_start):
            for _ in range(8):
                supervisor.tick(); clock[0] += 65
        state = supervisor.tick()
        require(len(attempts) == 3 and state['status'] == 'blocked' and state['attempts'] == 3 and not controller.processes(), 'Restart loop was not bounded')
        recreated = Supervisor(controller, clock=lambda: clock[0])
        require(recreated.tick()['status'] == 'blocked', 'New helper bypassed retry accounting')
        controller.save_intent(True)
        require(recreated.tick()['status'] == 'waiting', 'Explicit parent restart did not renew budget')
        passed('three failed launches exhaust a persistent retry budget; recreating the helper cannot bypass it; explicit parent Start renews intent')
        current = save.read_bytes(); save.write_bytes(b'isolated supervisor corruption')
        require(supervisor.tick()['status'] == 'blocked' and not controller.processes(), 'Corrupt save started')
        save.write_bytes(current)
        write(fixture.path / 'recovery.pending.json', dict(test=True))
        require(supervisor.tick()['status'] == 'blocked' and not controller.processes(), 'Interrupted restore started')
        (fixture.path / 'recovery.pending.json').unlink()
        with patch.object(controller, 'snapshot', return_value=dict(stopped, state='unreachable', canStart=False)):
            require(supervisor.tick()['status'] == 'blocked', 'Uncertain running process was replaced')
        with patch.object(controller, 'snapshot', return_value=dict(stopped, save=dict(state='missing'), canStart=True)):
            require(supervisor.tick()['status'] == 'blocked', 'Missing save silently reset')
        passed('corrupt/missing saves, pending recovery and uncertain process health block automatic restart instead of resetting progress or killing a process')
        recovery = Recovery(fixture.run_id, args.build); bundle = recovery.backup()
        controller.save_intent(True)
        recovery.restore(__import__('pathlib').Path(bundle['path']), digest(save.read_bytes()))
        require(supervisor.tick()['status'] == 'paused' and not controller.processes(), 'Restore automatically restarted the server')
        controller.start()
        require(supervisor.tick()['status'] == 'healthy', 'Manual restart failed')
        # Ten healthy minutes are a simulated policy boundary, not a soak claim.
        state = read(supervisor.path); state['attempts'] = 3; write(supervisor.path, state)
        clock[0] += 601
        require(supervisor.tick()['attempts'] == 0, 'Healthy interval did not reset failure budget')
        fixture.stop()
        passed('restore disables automatic restart; manual start resumes it; ten healthy minutes reset the retry budget at the simulated policy boundary')
        success = True
    finally:
        if watcher and watcher.poll() is None:
            watcher.terminate(); watcher.wait(timeout=15)
        fixture.cleanup()
        write(fixture.path / 'supervisor-result.json', dict(passed=success, build=args.build, utc=datetime.now(timezone.utc).isoformat(), checks=checks,
              measuredCrashToFourRejoinSeconds=elapsed if 'elapsed' in locals() else None,
              nativeCrashBackoffClockAccelerated=False, policyBoundaryClockAccelerated=True,
              scope='Isolated Windows process-crash with actual supervisor CLI/backoff and four native clients. Failed-retry/healthy-time policy boundaries use an injected clock; not a ten-minute soak, OS boot/service, physical-device or live-family deployment test.'))
        print('Evidence:', fixture.path / 'supervisor-result.json', flush=True)


if __name__ == '__main__': main()
