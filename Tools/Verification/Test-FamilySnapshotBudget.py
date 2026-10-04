"""Focused native regression using an explicitly supplied checkpoint COPY.

The source checkpoint is read only. All authority, credentials, commands and
captures stay in the existing ignored isolated-run directory; no live clients.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import time

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run, read, write, wait, require

spec = importlib.util.spec_from_file_location('home', Path(__file__).with_name('Test-HomeWorld.py'))
home = importlib.util.module_from_spec(spec)
spec.loader.exec_module(home)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--build', type=int, required=True)
    parser.add_argument('--checkpoint-copy', type=Path, required=True)
    args = parser.parse_args()
    raw = args.checkpoint_copy.read_bytes()
    header, checksum, payload = raw.decode('utf-8-sig').split('\n', 2)
    require(header == 'LITTLEWEEPS-SOLO-1' and hashlib.sha256(payload.encode()).hexdigest() == checksum, 'checkpoint checksum')
    seed = json.loads(payload)
    run = Run(args.build, extended_test_lifetime=True)
    require(len(seed['players']) == 4, 'four-profile fixture required')
    for slot, player in zip(run.slots, seed['players']):
        slot['profile'] = player['id']
    write(run.path / 'slots.json', run.slots)
    (run.path / 'server-world').mkdir()
    (run.path / 'server-world/world.save').write_bytes(raw)
    checks = []
    passed = False
    peak_bytes = 0
    try:
        server = run.start('server')
        clients = [run.start('client', slot['profile']) for slot in run.slots]

        def command(client, action, **kw):
            result = home.command(client, action, **kw)
            require(result['accepted'], result['outcome'])
            return result

        for client in clients:
            home.ready(client)
            player = next(p for p in client.state()['view']['players'] if p['id'] == client.profile)
            if player['zone'] != 'daycare':
                command(client, 7, value='daycare')
            command(client, 32, value='start')
        # Fill the existing pit through real authority commands, preserving its
        # old pieces. A full decorated scene exceeds the old wire budget.
        round_id = server.state()['view']['sandpit']['round']
        for row in range(4):
            for col in range(8):
                if len(server.state()['view']['sandpit']['moulds']) == 16:
                    break
                client = clients[(row * 8 + col) % 4]
                result = home.command(client, 32, value='place', item='round', target=f'place@{round_id}', x=4130+85*col, y=130+110*row)
                require(result['accepted'] or result['outcome'] == 'sand-spot-taken', 'legal fixture placement: '+result['outcome'])
        require(len(server.state()['view']['sandpit']['moulds']) == 16, 'full pit')
        for index, piece in enumerate(server.state()['view']['sandpit']['moulds']):
            client = clients[index % 4]
            target = f"{piece['id']}@{round_id}"
            command(client, 0, x=piece['x'], y=piece['y'])
            if not piece['built']:
                for _ in range(piece['capacity']-piece['scoops']):
                    command(client, 32, value='scoop', target=target)
                command(client, 32, value='water', target=target)
                command(client, 32, value='tip', target=target)
            occupied = {a['slot'] for a in piece['attachments']}
            for slot, kind in ((0,'flag'), (1,'flag'), (2,'shell'), (3,'pebble'), (4,'door'), (5,'window')):
                if slot not in occupied:
                    command(client, 32, value='decorate', target=target, item=f'{slot}:{kind}')
        checks.append('four actual clients contribute to retained/full decorated pit')
        until = time.monotonic()+20
        while time.monotonic() < until:
            snapshot = server.state()
            size = len(json.dumps(snapshot, separators=(',', ':'), ensure_ascii=False).encode())
            peak_bytes = max(peak_bytes, size)
            require(len(snapshot['connected']) == 4, 'four connected clients')
            for client in clients:
                require(client.state()['view']['sandpit'] == snapshot['view']['sandpit'], 'consistent shared castle')
            time.sleep(.5)
        require(131072 < peak_bytes < 262140, 'regression fixture must exceed old budget and fit new bounded budget')
        checks.append('oversized reliable snapshots remain synchronized during activity ticks')
        clients[-1].close()
        wait(lambda:len(server.state()['connected']) == 3, 'independent departure')
        returning = run.start('client', clients[-1].profile)
        wait(lambda:len(server.state()['connected']) == 4, 'reconnect')
        require(returning.state()['view']['sandpit'] == server.state()['view']['sandpit'], 'reconnect retains castle')
        checks.append('independent departure and reconnect preserve creation')
        passed = True
    finally:
        run.close()
        errors = [line for instance in run.instances for line in (instance.out/'player.log').read_text(encoding='utf-8', errors='replace').splitlines()
                  if 'Exception:' in line or 'send queue full' in line or 'exceeds probe limit' in line]
        write(run.path/'snapshot-budget-result.json', dict(passed=passed and not errors, build=args.build, actualSimultaneousClients=4,
              peakCompactSnapshotBytes=peak_bytes, checks=checks, runtimeErrors=errors, liveDataModified=False))
        require(not errors, 'runtime errors: '+str(errors[:3]))
    print('PASS snapshot budget; four actual native clients: '+str(run.path), flush=True)


if __name__ == '__main__':
    main()
