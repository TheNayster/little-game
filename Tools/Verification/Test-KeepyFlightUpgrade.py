# /// script
# dependencies = ["cryptography"]
# ///
"""Move a build-150 resting balloon once; preserve an isolated enrolled family."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from copy import deepcopy
from pathlib import Path
from parent_server import ParentServer
from recovery_fixture import RecoveryFixture
from server_recovery import Recovery, checkpoint, digest
import server_recovery
from shared_garden_runtime import require, write


def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    require(args.build>=151,'Expected the clear-lawn flight update')
    f=RecoveryFixture(150);save=f.path/'server-world/world.save';checks=[];success=False
    # Qualification is limited to this process and disposable family.
    server_recovery.QUALIFIED_BUILDS|={150,args.build};server_recovery.MAX_QUALIFIED_BUILD=args.build
    try:
        f.controller.start()
        for i in range(1,5):f.join(i)
        f.stop();old=Recovery(f.run_id,150).backup();before=checkpoint(save.read_bytes())
        require(before['keepy']['phase']==0 and 'spawnRevision' not in before['keepy'],'Expected an actual legacy resting balloon')
        expected=deepcopy(before);expected['revision']+=1
        expected['keepy'].update(spawnRevision=1,x=2450,y=150,centerX=2450,height=28,vx=0,vz=0,remainder=0,lastHitter='',hitAge=10,elapsed=0)
        keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build);f.controller.start()
        require(checkpoint(save.read_bytes())==expected,'Upgrade changed more than balloon layout and revision')
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        for i in range(1,5):f.join(i)
        require(f.controller.snapshot()['players']==4,'Original four profiles did not rejoin')
        f.stop();checks.append(dict(check='legacy resting balloon relocates once; all other saved state and enrollment remain exact; four profiles rejoin',passed=True))
        r=Recovery(f.run_id,args.build);require(r.backup()['verified'],'Candidate backup failed')
        r.restore(Path(old['path']),digest(save.read_bytes()));f.controller.start()
        require(checkpoint(save.read_bytes())==expected,'Restored legacy backup migrates differently')
        f.stop();checks.append(dict(check='restored build-150 backup receives exactly the same migration',passed=True));success=True
    finally:
        f.cleanup();result=dict(passed=success,previousBuild=150,build=args.build,checks=checks,liveFamilyServerAccessed=False)
        write(f.path/'keepy-flight-upgrade-result.json',result);print(result,flush=True);print('EVIDENCE '+str(f.path/'keepy-flight-upgrade-result.json'),flush=True)


if __name__=='__main__':main()
