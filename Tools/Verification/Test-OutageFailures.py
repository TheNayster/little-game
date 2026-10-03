"""Qualify outage failure boundaries in disposable native Windows worlds only."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from contextlib import contextmanager
import ctypes
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import time
import psutil

from family_pairing import write_record
from parent_server import checkpoint_bytes
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import read, require, wait, write


def payload(path):
    header, digest, body = checkpoint_bytes(path)[0].split(b'\n', 2)
    require(header == b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode() == digest, 'Invalid checkpoint')
    return json.loads(body)


@contextmanager
def deny_replace(path):
    # Read sharing permits observation, but denies the game's atomic replacement.
    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    kernel.CreateFileW.argtypes = [ctypes.c_wchar_p, ctypes.c_ulong, ctypes.c_ulong, ctypes.c_void_p, ctypes.c_ulong, ctypes.c_ulong, ctypes.c_void_p]
    kernel.CreateFileW.restype = ctypes.c_void_p
    kernel.CloseHandle.argtypes = [ctypes.c_void_p]
    handle = kernel.CreateFileW(str(path), 0x80000000, 1, None, 3, 0x80, None)
    require(handle != ctypes.c_void_p(-1).value, 'Cannot lock isolated save')
    try:
        yield
    finally:
        kernel.CloseHandle(handle)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    f = RecoveryFixture(args.build)
    checks, observations = [], {}
    success = False

    def passed(name):
        checks.append(dict(check=name, passed=True))
        print('PASS ' + name, flush=True)

    def inspect(c): return c.input('inspect')
    def toy(s, name): return next(t for t in s['toys'] if t['id'] == name)
    def replica(c): return f.path / 'client-recovery' / c.profile / 'world.save'
    def library(c): return f.path / 'client-adventures' / c.profile
    def shared(c): return wait(lambda: (v if (v := inspect(c))['shared'] and v['connected'] else None), 'shared presentation', 55)
    def local(c): return wait(lambda: (v if not (v := inspect(c))['shared'] and v['adventure'] else None), 'saved local adventure', 55)
    def settled(c): return wait(lambda: not inspect(c)['pending'], 'settled input')
    def drag(c, name, **destination):
        c.input('press', role=name)
        settled(c)
        c.input('release', **destination)
        settled(c)

    try:
        f.controller.start()
        clients = [f.join(i) for i in range(1, 5)]
        for c in clients:
            wait(lambda: replica(c).exists(), 'initial complete checkpoint', 50)
        native = f.controller.processes()[0]
        output = native['output']
        first, second, third, fourth = clients

        # The authority accepts a real pour while this client's command completion
        # is suppressed. Ordinary snapshots continue: this is not whole-link loss.
        drag(first, 'bucket-1', x=150, y=340)
        first.input('press', role='bucket-1')
        settled(first)
        first.input('drop-acks')
        first.input('release', x=810, y=330)
        request = wait(lambda: inspect(first)['pendingRequest'], 'unknown command outcome')
        accepted = wait(lambda: (s if (s := payload(f.path / 'server-world/world.save')) and any(r['requestId'] == request for r in s['receipts']) else None), 'accepted authority receipt')
        water = toy(accepted, 'plant-1')['water']
        require(water > 0, 'Lost acknowledgment scenario did not water the plant')
        blocker = library(first) / 'interrupted'
        require(not blocker.exists(), 'Unexpected test archive already exists')
        blocker.write_text('isolated test denies archive directory creation', encoding='utf-8')
        first.input('network-pause')
        wait(lambda: inspect(first)['pendingArchives'] == 1, 'failed archive retained')
        time.sleep(3)
        require(inspect(first)['pendingArchives'] == 1 and not inspect(first)['pending'], 'Archive was forgotten or live queue did not cancel')
        require(all(inspect(c)['connected'] for c in clients[1:]), 'One archive failure interrupted siblings')
        blocker.unlink()  # Only our named sentinel in this newly-created fixture.
        wait(lambda: inspect(first)['pendingArchives'] == 0, 'archive retry succeeds', 8)
        archives = list(blocker.glob('*.save'))
        require(len(archives) == 1, 'Retry created duplicate archives')
        archive = payload(archives[0])
        require(archive['profile'] == first.profile and archive['world'] == f.run_id and any(c['requestId'] == request for c in archive['commands']), 'Unresolved identity not retained')
        archive_bytes = checkpoint_bytes(archives[0])[0]
        first.input('restore-acks')
        first.input('network-resume')
        shared(first)
        time.sleep(2)
        current = payload(f.path / 'server-world/world.save')
        require(toy(current, 'plant-1')['water'] == water and len([r for r in current['receipts'] if r['requestId'] == request]) == 1, 'Reunion replayed the uncertain pour')
        require(checkpoint_bytes(archives[0])[0] == archive_bytes, 'Reunion rewrote unresolved archive')
        observations['lostAck'] = dict(authorityAccepted=True, archiveRetried=True, originalRequestRetained=True, replayed=False)
        passed('accepted pour with missing completion is archived after a real write denial using the original ID; siblings continue and reunion never replays it')

        # One client is backgrounded before authority loss. Another cannot commit
        # its selected-adventure pointer. Neither may invent a successful switch.
        second_selection = library(second) / 'selection.save'
        second_pointer = checkpoint_bytes(second_selection)[0]
        original_solo = Path(inspect(second)['savePath'])
        original_solo_bytes = checkpoint_bytes(original_solo)[0]
        fourth.input('network-pause')
        with deny_replace(second_selection):
            psutil.Process(native['pid']).terminate()
            wait(lambda: not f.controller.processes(), 'isolated authority terminated')
            local(first)
            local(third)
            wait(lambda: len([p for p in library(second).glob('*/world.save') if p.parent.name != 'interrupted']) == 1, 'branch created before blocked selection', 30)
            require(inspect(second)['shared'] and not inspect(second)['adventure'], 'Failed pointer write still replaced presentation')
            require(checkpoint_bytes(second_selection)[0] == second_pointer and checkpoint_bytes(original_solo)[0] == original_solo_bytes, 'Denied selection altered previous data')
            pending_branch = [p.parent.name for p in library(second).glob('*/world.save') if p.parent.name != 'interrupted'][0]
            # At least fourteen real seconds disconnected while backgrounded.
            time.sleep(3)
            require(not inspect(fourth)['adventure'], 'Background time started a local adventure')
        recovered = local(second)
        require(recovered['adventure'] == pending_branch and len(list(library(second).glob('*/world.save'))) == 1, 'Selection retry duplicated its pending adventure')
        require(checkpoint_bytes(original_solo)[0] == original_solo_bytes, 'Selection retry replaced old solo')
        passed('denied selection replacement preserves previous pointer and solo save; retry opens the same pending adventure without duplicate worlds')

        resumed = time.monotonic()
        fourth.input('network-resume')
        local(fourth)
        foreground_seconds = time.monotonic() - resumed
        require(foreground_seconds < 2, 'Known loss still adds an input-lock delay on foreground resume')
        observations['foregroundSecondsAfterResume'] = round(foreground_seconds, 3)
        passed('native lifecycle adapter leaves background presentation alone and enables local play within two seconds of foreground resume')

        # Deny replacement of the active local branch while it has a new drop.
        # Other clients rejoin the authority; this client must retain live local
        # input and wait until it can commit, rather than discard the drop.
        local_state = inspect(first)
        branch = Path(local_state['savePath'])
        before = checkpoint_bytes(branch)[0]
        with deny_replace(branch):
            drag(first, 'bucket-1', x=790, y=205)
            f.controller.start()
            shared(second)
            shared(third)
            shared(fourth)
            wait(lambda: inspect(first)['connected'], 'network reconnect despite branch disk failure', 55)
            time.sleep(3)
            require(not inspect(first)['shared'] and inspect(first)['adventure'] == local_state['adventure'], 'Reunion discarded unsaved local branch')
            require(checkpoint_bytes(branch)[0] == before, 'Denied save replaced committed branch')
            require(all(inspect(c)['shared'] for c in clients[1:]), 'Blocked branch stopped siblings')
        shared(first)
        saved = payload(branch)
        require(abs(toy(saved['snapshot'], 'bucket-1')['x'] - 790) < .1, 'Successful reunion lost pending local drop')
        passed('denied local branch save postpones reunion without losing live work or stopping siblings; releasing the lock commits the drop before joining')

        # A valid test pairing container with the wrong credential exercises
        # native server rejection. Never alter the real family's enrollment.
        fourth.close()
        rejected_pairing = f.path / 'rejected-test-player.pairing'
        bad = dict(f.players[3], credential='0' * 64)
        write_record(rejected_pairing, bad)
        rejected = f.launch(4, pairing_path=rejected_pairing)
        wait(lambda: (rejected.status() or {}).get('status') == 'needs-parent', 'explicit admission rejection', 50)
        wait(lambda: inspect(rejected)['ready'], 'local play after rejection')
        require(not inspect(rejected)['shared'], 'Rejected client entered shared world')
        before_events = read(rejected.out / 'connection-evidence.json')['events']
        attempts = len([e for e in before_events if e['phase'] == 'reconnect-scheduled'])
        drag(rejected, 'sponge-1', x=810, y=175)
        solo = Path(inspect(rejected)['savePath'])
        wait(lambda: abs(toy(payload(solo), 'sponge-1')['x'] - 810) < .1, 'rejected client local save')
        rejected.input('button', text='Menu')
        rejected.input('button', text='Find my family')
        time.sleep(6)
        events = read(rejected.out / 'connection-evidence.json')['events']
        require(len([e for e in events if e['phase'] == 'reconnect-scheduled']) == attempts, 'Rejected enrollment entered retry loop')
        require(not inspect(rejected)['connected'] and all(inspect(c)['shared'] for c in clients[:3]), 'Rejection disturbed shared family play')
        passed('native admission rejection leaves solo input and saves usable, does not retry through the child menu and does not disrupt the other three players')
        success = True
    finally:
        f.cleanup()
        result = dict(passed=success, build=args.build, utc=datetime.now(timezone.utc).isoformat(), checks=checks, observations=observations,
                      scope='Isolated Windows native clients and DTLS authority. Completion suppression and lifecycle-adapter calls are test hooks, not physical packet loss or iPad suspension. Real filesystem replacement denial. No family server or physical devices.')
        write(f.path / 'outage-failures-result.json', result)
        print('Evidence:', f.path / 'outage-failures-result.json', flush=True)


if __name__ == '__main__':
    main()
