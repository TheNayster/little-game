"""Qualify the deployed schema-2 writer's retained upgrade to scenic schema 3."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from copy import deepcopy
from pathlib import Path
import json
from recovery_fixture import RecoveryFixture
from parent_server import ParentServer
from server_recovery import Recovery, checkpoint, digest, QUALIFIED_BUILDS
from shared_garden_runtime import read, write, require


def main():
    fixture = RecoveryFixture(91)
    save = fixture.path / 'server-world/world.save'
    checks = []
    success = False
    try:
        require(110 in QUALIFIED_BUILDS and 99 not in QUALIFIED_BUILDS, 'Experimental writer gate regressed')
        fixture.controller.start()
        for i in range(1, 5): fixture.join(i)
        fixture.stop()
        old_backup = Recovery(fixture.run_id, 91).backup()
        before = checkpoint(save.read_bytes())
        require(before['schema'] == 2 and len(before['players']) == 4, 'Expected legacy family schema')
        expected = deepcopy(before)
        expected.update(schema=3, revision=before['revision'] + 1)
        enrollment = {p.name: p.read_bytes() for p in fixture.path.glob('*.pairing')}
        fixture.build = 110
        fixture.controller = ParentServer(fixture.run_id, 110)
        fixture.controller.start()
        require(checkpoint(save.read_bytes()) == expected, 'Upgrade changed an existing gameplay field')
        require(all((fixture.path / n).read_bytes() == v for n, v in enrollment.items()), 'Enrollment changed')
        checks.append(dict(check='91 to 110 changes only schema and one revision; every original gameplay field and protected enrollment byte retained', passed=True))
        for i in range(1, 5): fixture.join(i)
        require(fixture.controller.snapshot()['players'] == 4, 'Original family did not rejoin')
        fixture.stop()
        recovery = Recovery(fixture.run_id, 110)
        current_backup = recovery.backup()
        require(current_backup['verified'] and current_backup['build'] == 110, 'New writer backup failed')
        recovery.restore(Path(old_backup['path']), digest(save.read_bytes()))
        fixture.controller.start()
        require(checkpoint(save.read_bytes()) == expected, 'Restored legacy backup did not upgrade identically')
        fixture.stop()
        checks.append(dict(check='Four original enrolled clients rejoin 110; new backup verifies; legacy 91 backup restores and upgrades identically', passed=True))
        success = True
    finally:
        fixture.cleanup()
        result = dict(passed=success, previousBuild=91, build=110, checks=checks,
                      scope='Disposable enrolled Windows family; no real family files used')
        write(fixture.path / 'scenic-upgrade-result.json', result)
        print(json.dumps(result, indent=2))
        print('Evidence:', fixture.path / 'scenic-upgrade-result.json')


if __name__ == '__main__': main()
