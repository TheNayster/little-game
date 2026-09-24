"""Upgrade an isolated native build-57 world without changing existing progress."""
import argparse
from copy import deepcopy
import hashlib
import json
from shared_garden_runtime import Run, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int); args = parser.parse_args()
    old = Run(57); updated = None
    try:
        server = old.start('server'); client = old.start('client', 'player-1')
        client.input('press', role='bucket-1'); client.input('release', x=150, y=340)
        wait(lambda: not client.input('inspect')['pending'] and not client.input('inspect')['dragging'], 'old bucket fill')
        client.input('button', text='Orange pup')
        wait(lambda: not client.input('inspect')['pending'], 'old avatar save')
        client.close(); wait(lambda: not server.state()['connected'], 'old client left'); server.close()
        checkpoint = old.path/'server-world/world.save'; before_bytes = checkpoint.read_bytes()
        before = json.loads(before_bytes.decode('utf-8').split('\n', 2)[2])
        require(before['schema'] == 1 and next(t for t in before['toys'] if t['id'] == 'bucket-1')['water'] == 3, 'Invalid migration fixture')
        (old.path/'before-area-upgrade.save').write_bytes(before_bytes)
        updated = Run(args.build, resume=old.run_id); server = updated.start('server'); client = updated.start('client', 'player-1')
        after = server.state()['view']; expected = deepcopy(before)
        require(after['schema'] == 2 and after['worldId'] == before['worldId'] and after['revision'] == before['revision']+1, 'World identity or migration revision changed incorrectly')
        for p in expected['players']: p.update(zone='garden', visit=0)
        for t in expected['toys']: t.update(zone='garden')
        require(after['players'] == expected['players'] and after['toys'][:5] == expected['toys'] and len(after['toys']) == 10, 'Upgrade changed old progress')
        durable = json.loads(checkpoint.read_text(encoding='utf-8').split('\n', 2)[2])
        require(durable['receipts'] == before['receipts'], 'Migration lost command receipts')
        client.close(); wait(lambda: not server.state()['connected'], 'new client left'); server.close()
        server = updated.start('server')
        require(server.state()['view'] == after, 'Second start migrated twice or regenerated props')
        write(old.path/'upgrade-result.json', dict(passed=True, fromBuild=57, toBuild=args.build, runId=old.run_id, worldId=after['worldId'],
              originalSha256=hashlib.sha256(before_bytes).hexdigest(), originalPlayersAndToysPreserved=True, receiptsPreserved=True,
              secondRestartExact=True, normalFamilySaveAccessed=False))
        print('PASS native 57 -> '+str(args.build)+' upgrade and second restart. Evidence: '+str(old.path), flush=True)
    finally:
        if updated: updated.close()
        old.close()


if __name__ == '__main__': main()
