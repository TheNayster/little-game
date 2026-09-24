"""Four native Windows players: independent travel, current holds and durable areas."""
import argparse
from copy import deepcopy
from datetime import datetime, timezone
import uuid
from shared_garden_runtime import Run, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    run = Run(args.build)
    require(run.protocol >= 2, 'Area build required')
    checks = []

    def passed(name):
        checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)

    try:
        server = run.start('server')
        first = run.start('client', 'player-1'); second = run.start('client', 'player-2')
        def world(): return server.state()['view']
        def toy(name): return next(t for t in world()['toys'] if t['id'] == name)
        def player(client): return next(p for p in world()['players'] if p['id'] == client.profile)
        def settled(client):
            return wait(lambda: (v if not (v := client.input('inspect'))['pending'] and not v['dragging'] else None), 'settled ' + client.profile)
        def travel(client, area):
            client.input('touchButton', text=area.title())
            wait(lambda: player(client)['zone'] == area, 'authority travel to ' + area)
            v = settled(client); require(v['zone'] == area, 'Local area disagrees with authority')
            require(len(v['toys']) == 5 and all(t['id'].endswith('-1' if area == 'garden' else '-creek') for t in v['toys']), 'Wrong area toys visible')
        def send(client, action, **overrides):
            rid = uuid.uuid4().hex; p = player(client)
            cmd = dict(requestId=rid, actor=client.profile, expectedRevision=world()['revision'], zone=p['zone'], visit=p['visit'],
                       action=action, item='', target='', value='', x=0, y=0)
            cmd.update(overrides); client.serial += 1
            write(client.out/'control.json', dict(serial=client.serial, kind='command', request=dict(requestId=rid, protocol=run.protocol, command=cmd)))
            return wait(lambda: read(client.out/('reply-' + rid + '.json')), 'explicit area command')

        first.input('button', text='Splash cleanup'); settled(first)
        first.input('press', role='sponge-1'); wait(lambda: toy('sponge-1')['holder'] == first.profile, 'sponge hold')
        sibling_before = deepcopy(player(first)); sponge_before = deepcopy(toy('sponge-1'))
        travel(second, 'creek')
        require(player(first) == sibling_before and toy('sponge-1') == sponge_before, 'Travel interrupted remaining player')
        require(first.input('inspect')['visiblePlayers'] == 1 and second.input('inspect')['visiblePlayers'] == 1, 'Cross-area avatar rendered')
        first.input('release', x=680, y=140); settled(first)
        require(toy('puddle-1')['water'] == 2 and toy('puddle-creek')['water'] == 3, 'Remaining activity or separate content reset')
        first.input('capture'); second.input('capture')
        wait(lambda: (first.out/'garden.png').exists() and (second.out/'garden.png').exists(), 'separated views capture')
        passed('one player travels while sibling holds a sponge; remaining cleanup continues in its existing area')

        before = deepcopy(world())
        denied = send(second, 2, item='bucket-1')
        require(not denied['accepted'] and denied['outcome'] == 'wrong-area' and world() == before, 'Remote grab changed a room')
        denied = send(second, 7, value='missing')
        require(not denied['accepted'] and world() == before, 'Invalid destination changed state')
        passed('authority rejects remote item access and unknown destinations without mutation')

        second.input('press', role='bucket-creek'); second.input('release', x=150, y=340); settled(second)
        second.input('press', role='bucket-creek'); wait(lambda: not second.input('inspect')['pending'], 'held filled creek bucket')
        second.input('move', x=485, y=260)
        travel(first, 'creek')
        arrival = first.input('inspect')
        require(arrival['visiblePlayers'] == 2 and toy('bucket-creek')['water'] == 3, 'Arrival did not share current state')
        label = next(t['label'] for t in arrival['toys'] if t['id'] == 'bucket-creek')
        require('Player 2 has it' in label, 'Arrival missed current holder')
        denied = first.input('press', role='bucket-creek'); first.input('release', role='bucket-creek')
        require('friend is using' in denied['feedback'] and toy('bucket-creek')['holder'] == second.profile, 'Arrival allowed stealing')
        second.input('release', x=810, y=330); settled(second)
        require(toy('plant-creek')['water'] == 3 and toy('plant-1')['water'] == 0, 'Pour affected wrong area')
        passed('arrival sees current filled held bucket and shared ownership; pour affects only that area')

        # Tap travel with another finger before the 250 ms pickup ack resolves.
        first.input('touch-begin', role='bucket-creek', finger=17)
        travel(first, 'garden')
        first.input('touch-cancel', x=400, y=200, finger=17)
        require(toy('bucket-creek')['holder'] == '' and len(world()['toys']) == 10, 'Travel left an invisible hold or cloned a tool')
        stale_visit = player(first)['visit']; old_zone = player(first)['zone']
        travel(first, 'creek'); travel(first, 'garden')
        before = deepcopy(world())
        denied = send(first, 0, zone=old_zone, visit=stale_visit, x=920, y=400)
        require(not denied['accepted'] and denied['outcome'] == 'stale-area' and world() == before, 'Prior visit action accepted after returning')
        passed('second-finger travel during pending pickup settles hold; obsolete visit commands stay rejected')

        third = run.start('client', 'player-3'); fourth = run.start('client', 'player-4')
        travel(third, 'creek')
        wait(lambda: all(c.input('inspect')['visiblePlayers'] == 2 for c in (first, second, third, fourth)), 'four players separated two per area')
        garden_before = deepcopy([t for t in world()['toys'] if t['zone'] == 'garden'])
        travel(first, 'creek'); travel(fourth, 'creek')
        wait(lambda: all(c.input('inspect')['visiblePlayers'] == 4 for c in (first, second, third, fourth)), 'gather all four')
        require([t for t in world()['toys'] if t['zone'] == 'garden'] == garden_before, 'Empty garden reset')
        travel(first, 'garden')
        require([t for t in world()['toys'] if t['zone'] == 'garden'] == garden_before, 'Reentry rebuilt garden')
        passed('four clients separate and gather; empty area and revisits preserve exact item state')

        # Latest button choice wins even while the first transition is pending.
        first.input('touchButton', text='Creek'); first.input('touchButton', text='Garden')
        settled(first); require(player(first)['zone'] == 'garden' and first.input('inspect')['zone'] == 'garden', 'Rapid travel left obsolete destination')
        require(first.input('inspect')['visiblePlayers'] == 1, 'Rapid travel left a phantom avatar')
        passed('rapid destination changes finish at latest requested area with one avatar membership')

        second.input('press', role='sponge-creek'); wait(lambda: not second.input('inspect')['pending'], 'hold before close')
        second.process.kill(); second.process.wait(timeout=10)
        wait(lambda: toy('sponge-creek')['holder'] == '' and second.profile not in server.state()['connected'], 'departed holder cleared')
        third.input('press', role='sponge-creek'); third.input('release', x=680, y=140); settled(third)
        require(toy('puddle-creek')['water'] == 2, 'Remaining creek player blocked')
        second = run.start('client', 'player-2')
        require(second.input('inspect')['zone'] == 'creek' and player(second)['zone'] == 'creek', 'Rejoin lost committed area')
        passed('closing and rejoining restores current area while remaining players continue')

        committed = deepcopy(world()); epoch = server.state()['epoch']
        for c in (first, second, third, fourth): c.close()
        wait(lambda: not server.state()['connected'], 'clients departed')
        server.close()
        server = run.start('server'); first = run.start('client', 'player-1'); second = run.start('client', 'player-2')
        require(world() == committed and server.state()['epoch'] != epoch, 'Restart changed saved areas')
        require(first.input('inspect')['zone'] == 'garden' and second.input('inspect')['zone'] == 'creek', 'Restart presentation lost area')
        passed('authority restart and client rejoin preserve complete world and separate player locations')
        write(run.path/'result.json', dict(passed=True, build=args.build, runId=run.run_id, utc=datetime.now(timezone.utc).isoformat(), checks=checks,
              worldId=world()['worldId'], physicalDevicesAccessed=False, loopbackOnly=True, automaticRejoin=False, captures=[str(c.out/'garden.png') for c in run.instances if (c.out/'garden.png').exists()]))
        print('PASS independent areas. Evidence: ' + str(run.path), flush=True)
    except Exception as error:
        write(run.path/'result.json', dict(passed=False, build=args.build, runId=run.run_id, checks=checks, error=str(error)))
        print('FAIL ' + str(error) + '. Evidence: ' + str(run.path), flush=True); raise
    finally:
        run.close()


if __name__ == '__main__': main()
