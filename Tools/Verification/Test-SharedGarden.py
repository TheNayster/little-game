"""Two native UGUI gardens connected to one isolated server; no mobile devices."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime, timezone
import time
from shared_garden_runtime import Run, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int); args = parser.parse_args()
    run = Run(args.build); checks = []

    def passed(name, **kw):
        checks.append(dict(check=name, passed=True, **kw)); print('PASS ' + name, flush=True)

    try:
        server = run.start('server'); first = run.start('client', 'player-1'); second = run.start('client', 'player-2')
        def toy(name): return next(t for t in server.state()['view']['toys'] if t['id'] == name)
        def player(name): return next(p for p in server.state()['view']['players'] if p['id'] == name)
        def settled(c): return wait(lambda: not c.input('inspect')['pending'] and c.input('inspect')['dragging'] == '', 'settled UI')
        def all_match():
            expected = server.state()['view']
            wait(lambda: first.state()['view'] == expected and second.state()['view'] == expected, 'replicated world')
        wait(lambda: first.input('inspect')['visiblePlayers'] == 2 and second.input('inspect')['visiblePlayers'] == 2, 'both characters drawn')
        passed('two native gardens draw both connected players')

        first.input('press', role='bucket-1')
        early = first.input('release', x=150, y=340)
        require(early['pending'], 'Quick release did not exercise delayed pickup approval')
        settled(first); wait(lambda: toy('bucket-1')['water'] == 3 and toy('bucket-1')['holder'] == '', 'early-release fill')
        all_match(); passed('release before pickup acknowledgement finishes exactly once without a stuck hold')

        first.input('press', role='bucket-1'); wait(lambda: not first.input('inspect')['pending'], 'grab acknowledged')
        require(toy('bucket-1')['holder'] == first.profile, 'Pickup not admitted')
        first.input('move', x=480, y=280)
        def remote_bucket_arrived():
            rendered = next(t for t in second.input('inspect')['toys'] if t['id'] == 'bucket-1')
            return abs(rendered['position']['x'] - 480) < 2 and abs(rendered['position']['y'] - 280) < 2 and 'Player 1 has it' in rendered['label']
        # Remote poses intentionally render behind the authority (180 ms). A
        # 150 ms sleep depended on incidental test/IO overhead to pass. Require
        # the actual position AND ownership, with a bounded failure deadline.
        wait(remote_bucket_arrived, 'sibling rendered accepted drag and holder', seconds=1.5)
        blocked = second.input('press', role='bucket-1'); second.input('release', role='bucket-1')
        require('friend is using' in blocked['feedback'] and toy('bucket-1')['holder'] == first.profile, 'Contested UI pickup did not explain rejection')
        first.input('release', x=810, y=330); settled(first)
        require(toy('plant-1')['water'] == 3 and toy('bucket-1')['water'] == 0, 'Shared pour failed'); all_match()
        passed('sibling sees held toy moving; contention is explained; confirmed pour replicates')

        before = dict(toy('bucket-1'))
        first.input('touch-begin', role='bucket-1', finger=17)
        first.input('touch-cancel', x=150, y=340, finger=17); settled(first)
        require(toy('bucket-1') == before, 'Canceled touch filled or moved the bucket')
        first.input('touch-begin', role='bucket-1', finger=18)
        menu = first.input('touchButton', text='Menu'); require(menu['menuOpen'], 'Second finger did not open menu'); settled(first)
        require(toy('bucket-1') == before, 'Menu during approval changed bucket')
        first.input('touch-cancel', role='bucket-1', finger=18)
        first.input('touchButton', text='Back to play'); passed('canceled touch and menu during pickup approval release the lease without interacting')

        second.input('button', text='Orange pup'); settled(second)
        require(player(second.profile)['avatar'] == 'orange-pup', 'Avatar switch did not reach authority')
        second.input('press', x=500, y=65); second.input('release', x=500, y=65)
        wait(lambda: abs(player(second.profile)['x']-500)<2 and abs(player(second.profile)['y']-65)<2, 'tap walking', seconds=20)
        def friend_arrived():
            friend = next(p for p in first.input('inspect')['players'] if p['id'] == second.profile)
            return abs(friend['position']['x']-500)<2 and friend['visible']
        wait(friend_arrived, 'interpolated sibling render settles at destination', seconds=2)
        passed('tap walking and avatar switching are confirmed and visible to the other player')

        second.input('button', text='Tap to walk')
        before_x = player(second.profile)['x']
        second.input('touch-begin', role='stick', x=45, finger=31)
        second.input('touch-begin', role='sponge-1', finger=32)
        wait(lambda: player(second.profile)['x'] > before_x+10 and toy('sponge-1')['holder'] == second.profile, 'walking while holding with another finger')
        second.input('touch-move', x=680, y=140, finger=32)
        second.input('touch-end', x=680, y=140, finger=32)
        second.input('touch-end', role='stick', finger=31); settled(second)
        require(toy('puddle-1')['water'] == 2 and toy('sponge-1')['holder'] == '', 'Two-finger network cleanup failed')
        passed('joystick walking and toy dragging use separate touches through the shared session')

        second.input('touchButton', text='Splash cleanup'); settled(second)
        second.input('touchButton', text='Free play'); settled(second)
        require(player(second.profile)['activity'] == '' and toy('plant-1')['water'] == 3, 'Activity exit reset shared play')
        first.input('capture'); second.input('capture')
        wait(lambda: (first.out/'garden.png').exists() and (second.out/'garden.png').exists(), 'garden images')
        passed('activities can be freely started and left; both garden views captured')

        first.input('press', role='bucket-1'); wait(lambda: not first.input('inspect')['pending'], 'hold before departure')
        first.process.kill(); first.process.wait(timeout=10)
        wait(lambda: toy('bucket-1')['holder'] == '' and first.profile not in server.state()['connected'], 'departed player cleanup')
        wait(lambda: second.input('inspect')['visiblePlayers'] == 1, 'departed avatar hidden')
        second.input('press', role='sponge-1'); second.input('release', x=680, y=140); settled(second)
        require(toy('puddle-1')['water'] == 1, 'Remaining player could not clean up')
        passed('closing one game window releases its toy; remaining garden keeps accepting interactions')

        returning = run.start('client', first.profile)
        wait(lambda: returning.input('inspect')['visiblePlayers'] == 2, 'returning garden')
        require(returning.state()['view'] == server.state()['view'], 'Returning UI got an old world')
        passed('returning garden renders the current shared world')
        server.process.kill(); server.process.wait(timeout=10)
        wait(lambda: not second.input('inspect')['connected'], 'loss shown')
        denied = second.input('press', role='sponge-1'); second.input('release', x=680, y=140)
        require(not denied['pending'] and 'connection stopped' in denied['feedback'].lower(), 'Disconnected view allowed an optimistic mutation')
        passed('server loss leaves a clear inactive view without pretending actions were saved')
        write(run.path/'result.json',dict(passed=True,build=args.build,runId=run.run_id,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
                                         delayedAcknowledgementMs=250,physicalDevicesAccessed=False,automaticRejoin=False))
        print('PASS shared garden. Evidence: '+str(run.path),flush=True)
    except Exception as error:
        write(run.path/'result.json',dict(passed=False,build=args.build,runId=run.run_id,checks=checks,error=str(error)))
        print('FAIL '+str(error)+'. Evidence: '+str(run.path),flush=True);raise
    finally:
        run.close()


if __name__ == '__main__': main()
