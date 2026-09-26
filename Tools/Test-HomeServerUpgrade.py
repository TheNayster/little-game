"""Verify the deployed scenic family upgrades additively to the home release."""
from copy import deepcopy
import json
from pathlib import Path
from recovery_fixture import RecoveryFixture
from parent_server import ParentServer
from server_recovery import Recovery, checkpoint, digest, QUALIFIED_BUILDS
from shared_garden_runtime import require, write


def expected_home(before):
    require(before['schema'] == 3, 'Expected scenic schema 3')
    after = deepcopy(before)
    after.update(schema=4, revision=before['revision'] + 1,
                 home=dict(livingRadio=False, gardenRadio=False, shedOpen=False))
    for player in after['players']:
        player.update(fixture='', useSeconds=0)
    for toy in after['toys']:
        toy['container'] = ''
    after['toys'].append(dict(id='ball-1', holder='', container='', zone='garden',
        kind=5, x=3350, y=130, water=0, wet=False, resetPending=False))
    return after


def main():
    fixture = RecoveryFixture(110)
    save = fixture.path / 'server-world/world.save'
    checks = []; success = False
    try:
        require(128 in QUALIFIED_BUILDS and 99 not in QUALIFIED_BUILDS, 'Wrong recovery gate')
        fixture.controller.start()
        for i in range(1, 5): fixture.join(i)
        fixture.stop()
        previous = Recovery(fixture.run_id, 110).backup()
        before = checkpoint(save.read_bytes())
        expected = expected_home(before)
        enrollment = {p.name: p.read_bytes() for p in fixture.path.glob('*.pairing')}
        fixture.build = 128
        fixture.controller = ParentServer(fixture.run_id, 128)
        fixture.controller.start()
        require(checkpoint(save.read_bytes()) == expected, 'Unexpected upgraded gameplay state')
        require(all((fixture.path / n).read_bytes() == v for n, v in enrollment.items()), 'Enrollment changed')
        checks.append(dict(check='110 to 128 retains all existing fields and enrollment; adds only schema-4 defaults, home ball and one revision', passed=True))
        for i in range(1, 5): fixture.join(i)
        require(fixture.controller.snapshot()['players'] == 4, 'Original clients did not rejoin')
        fixture.stop()
        recovery = Recovery(fixture.run_id, 128)
        require(recovery.backup()['verified'], 'Updated recovery backup failed')
        recovery.restore(Path(previous['path']), digest(save.read_bytes()))
        fixture.controller.start()
        require(checkpoint(save.read_bytes()) == expected, 'Legacy backup upgrade differs')
        fixture.stop()
        checks.append(dict(check='Four original enrolled clients rejoin; home backup verifies; restored 110 checkpoint upgrades identically', passed=True))
        success = True
    finally:
        fixture.cleanup()
        result = dict(passed=success, previousBuild=110, build=128, checks=checks,
            scope='Disposable enrolled Windows family; no real family files used')
        write(fixture.path / 'home-upgrade-result.json', result)
        print(json.dumps(result, indent=2))
        print('Evidence:', fixture.path / 'home-upgrade-result.json')


if __name__ == '__main__': main()
