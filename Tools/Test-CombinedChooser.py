# /// script
# dependencies = ["cryptography"]
# ///
"""Combined chooser and real travel barriers through native release touch input."""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import shutil
import time
from parent_server import checkpoint_bytes
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import Run, wait, require, write


def capture(client, evidence, name):
    target = client.out / 'garden.png'
    old = target.stat().st_mtime_ns if target.exists() else 0
    state = client.input('capture')
    wait(lambda: target.exists() and target.stat().st_mtime_ns != old, 'fresh capture')
    shutil.copyfile(target, evidence / (name + '.png'))
    write(evidence / (name + '.json'), state)
    require(state['controlsInSafeArea'], 'Controls outside safe area: ' + name)
    if state['charactersOpen']:
        require(state['fullCharactersInTray'], 'Clipped character artwork: ' + name)
    return state


def swipe_places(client, upward=True):
    state = client.input('inspect')
    board = state['boardBounds']
    h = state['screenHeight']
    x = state['screenWidth'] - 120 * h / 768
    start, end = (h * .52, h * .80) if upward else (h * .80, h * .52)
    def point(y):
        return dict(x=(x - board['x']) / board['width'] * 1000,
                    y=(y - board['y']) / board['height'] * 500, finger=53)
    client.input('touch-begin', **point(start))
    # More than one moved event is necessary: the first starts ScrollRect's drag.
    for fraction in (.15, .4, .7, 1):
        client.input('touch-move', **point(start + (end - start) * fraction))
    # Hold still to stop fling inertia before releasing at a predictable location.
    time.sleep(.35)
    client.input('touch-end', **point(end))
    time.sleep(.35)
    return client.input('inspect')


def reveal(client, name):
    for _ in range(12):
        s = client.input('inspect')
        control = next((c for c in s['controls'] if c['name'] == name), None)
        if control:
            b = control['bounds']
            if b['height'] >= b['width'] * .97:
                return s
        # Garden precedes Creek. Return toward the start when looking for it.
        swipe_places(client, upward=name != 'World Garden')
    raise AssertionError('Could not reveal ' + name)


def arrived(client, zone):
    return wait(lambda: (s if not (s := client.input('inspect'))['worldLoading']
                        and not s['pending'] and s['zone'] == zone else None), 'arrival in ' + zone)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    run = Run(args.build)
    evidence = run.path / 'combined-chooser'
    evidence.mkdir()
    checks = []
    fixture = None
    def passed(name, **extra):
        checks.append(dict(check=name, passed=True, **extra))
        print('PASS ' + name, flush=True)
    try:
        server = run.start('server')
        first = run.start('client', 'player-1')
        second = run.start('client', 'player-2')
        def player(id): return next(p for p in server.state()['view']['players'] if p['id'] == id)
        def toy(id): return next(t for t in server.state()['view']['toys'] if t['id'] == id)
        def tap(name): return first.input('touchButton', text=name)
        def settled(c): return wait(lambda: not c.input('inspect')['pending'] and not c.input('inspect')['dragging'], 'settled action')

        first.input('resize', x=1024, y=768)
        before = dict(player(first.profile))
        tap('Characters')
        s = capture(first, evidence, 'tablet-chooser')
        require(s['charactersOpen'] and s['worldsOpen'], 'Combined menu did not open')
        require(not next(c for c in s['controls'] if c['name'] == 'World Heeler Home')['enabled'], 'Unbuilt home enabled')
        tap('World Heeler Home')
        require(first.input('inspect')['charactersOpen'], 'Unbuilt home entered')
        tap('Bingo'); settled(first)
        wait(lambda: player(first.profile)['avatar'] == 'orange-pup', 'Bingo acknowledged')
        after = player(first.profile)
        require({k:v for k,v in before.items() if k != 'avatar'} == {k:v for k,v in after.items() if k != 'avatar'}, 'Avatar changed stable state')
        require(first.input('inspect')['characterLayers'] == 11, 'Layered character not displayed')
        first.input('touch-begin', role='ui:Bluey', finger=31)
        first.input('touch-move', role='ui:Bluey', x=-120, finger=31)
        first.input('touch-end', role='ui:Bluey', x=-120, finger=31)
        first.input('touch-begin', role='ui:Bluey', finger=32)
        first.input('touch-cancel', role='ui:Bluey', finger=32)
        require(player(first.profile)['avatar'] == 'orange-pup', 'Swipe or cancellation selected an avatar')
        passed('one chooser shows vertical places and full-body horizontal cast; switching preserves identity; canceled/swiped touches do not select')

        tap('Close characters'); tap('Tap to walk')
        first.input('touch-begin', role='stick', x=45, finger=40)
        wait(lambda: player(first.profile)['x'] > before['x'] + 5, 'joystick walk')
        tap('Characters'); time.sleep(.6)
        stopped = dict(player(first.profile)); time.sleep(.3)
        require(player(first.profile) == stopped and not first.input('inspect')['joystickVisible'], 'Menu failed to stop joystick')
        first.input('touch-end', role='stick', finger=40)
        tap('Close characters')
        require(first.input('inspect')['joystickVisible'] and player(first.profile) == stopped, 'Old walking gesture replayed')
        tap('Tap to walk')
        before_bucket = dict(toy('bucket-1'))
        first.input('touch-begin', role='bucket-1', finger=41)
        tap('Characters'); settled(first)
        first.input('touch-cancel', role='bucket-1', finger=41)
        require(toy('bucket-1') == before_bucket, 'Opening menu changed bucket or stranded a lease')
        passed('menu cancels movement and pickup safely; arrow restores controls')

        second.input('press', x=910, y=50); second.input('release', x=910, y=50)
        wait(lambda: abs(player(second.profile)['x'] - 910) < 2, 'sibling walks')
        second.input('press', role='bucket-1'); second.input('release', x=150, y=340); settled(second)
        require(toy('bucket-1')['water'] == 3, 'Sibling could not use bucket')
        reveal(first, 'World Creek')
        require(player(first.profile)['zone'] == 'garden' and first.input('inspect')['charactersOpen'], 'Vertical browse caused travel')
        capture(first, evidence, 'tablet-chooser-scrolled')
        tap('World Creek'); s = arrived(first, 'creek')
        require(s['travelStages'] == ['loading-screen','gestures-settled','travel-requested','authority-confirmed','destination-presented','ready'], 'Travel missed readiness barrier')
        write(evidence / 'shared-arrival.json', s)
        require(player(second.profile)['zone'] == 'garden' and toy('bucket-1')['water'] == 3, 'Travel interrupted sibling or reset objects')
        tap('Worlds'); reveal(first, 'World Garden'); tap('World Garden'); arrived(first, 'garden')
        require(toy('bucket-1')['water'] == 3, 'Round trip reset area state')
        passed('vertical browsing does not travel; shared round trip waits for acknowledgement and preserves independent sibling play')

        first.input('resize', x=1280, y=591)
        tap('Characters'); capture(first, evidence, 'phone-chooser')
        reveal(first, 'World Creek'); capture(first, evidence, 'phone-chooser-scrolled')
        # Lose the acknowledgement after the server applies travel. The loading
        # screen must not declare success based solely on the snapshot arriving.
        first.input('drop-acks'); tap('World Creek')
        wait(lambda: player(first.profile)['zone'] == 'creek', 'server applies dropped-ack travel')
        s = capture(first, evidence, 'phone-loading')
        require(s['worldLoading'] and 'ready' not in s['travelStages'], 'Unacknowledged travel looked complete')
        first.input('press', x=820, y=50); first.input('release', x=820, y=50)
        second.input('press', x=500, y=50); second.input('release', x=500, y=50)
        wait(lambda: abs(player(second.profile)['x'] - 500) < 2, 'sibling continues during loading')
        require(first.input('inspect')['worldLoading'], 'Loading screen accepted background input')
        s = wait(lambda: (s if (s := first.input('inspect'))['loadingFailure'] else None), 'missing ack recovery', 20)
        capture(first, evidence, 'phone-loading-error')
        require('failed' in s['travelStages'] and 'ready' not in s['travelStages'], 'Failure reported ready')
        tap('Back to my game')
        require(not first.input('inspect')['worldLoading'], 'Failure trapped player on loading screen')
        passed('phone layout and loading touch shield; missing acknowledgement stays pending then offers recovery; sibling remains playable')

        fixture = RecoveryFixture(args.build)
        solo = fixture.launch(1)
        s = wait(lambda: (s if (s := solo.input('inspect'))['ready'] and not s['shared'] else None), 'offline solo ready', 45)
        solo.input('touchButton', text='Characters'); reveal(solo, 'World Creek')
        solo.input('touchButton', text='World Creek'); s = arrived(solo, 'creek')
        require(s['travelStages'] == ['loading-screen','gestures-settled','departure-saved','travel-requested','local-arrival','arrival-saved','destination-presented','ready'], 'Solo travel did not save both boundaries')
        raw = checkpoint_bytes(Path(s['savePath']))[0]
        header, digest, body = raw.split(b'\n', 2)
        require(header == b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode() == digest, 'Invalid solo checkpoint')
        save = json.loads(body)
        write(evidence / 'solo-arrival.json', dict(build=args.build, zone=s['zone'], travelStages=s['travelStages'], checkpointVerified=True, schema=save.get('schema')))
        solo.close()
        reopened = fixture.launch(1)
        s = wait(lambda: (s if (s := reopened.input('inspect'))['ready'] and not s['shared'] else None), 'solo reopen', 45)
        require(s['zone'] == 'creek', 'Reopen lost destination')
        passed('offline travel saves departure and arrival; verified checkpoint reopens in Creek')
        result = dict(passed=True, build=args.build, utc=datetime.now(timezone.utc).isoformat(), runId=run.run_id,
                      checks=checks, physicalDevicesAccessed=False, liveFamilyServerAccessed=False, evidence=str(evidence))
        write(evidence / 'result.json', result)
        print('Evidence: ' + str(evidence), flush=True)
    except Exception as error:
        write(evidence / 'result.json', dict(passed=False, build=args.build, checks=checks, error=str(error)))
        print('FAIL ' + str(error) + '; evidence: ' + str(evidence), flush=True)
        raise
    finally:
        if fixture: fixture.cleanup()
        run.close()


if __name__ == '__main__': main()
