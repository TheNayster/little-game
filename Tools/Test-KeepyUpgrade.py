"""128 to Keepy Uppy migration, using only a disposable enrolled family."""
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
    f=RecoveryFixture(128);save=f.path/'server-world/world.save';checks=[];success=False
    try:
        f.controller.start()
        for i in range(1,5):f.join(i)
        f.stop();old=Recovery(f.run_id,128).backup();before=checkpoint(save.read_bytes())
        expected=deepcopy(before);expected.update(schema=5,revision=before['revision']+1,
            keepy=dict(phase=0,round=0,hitSerial=0,x=-4240,y=180,height=28,vx=0,vz=0,centerX=0,elapsed=0,remainder=0,hitAge=10,lastHitter='',hitLeft=False))
        keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build);f.controller.start()
        require(checkpoint(save.read_bytes())==expected,'Upgrade changed existing home state')
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        for i in range(1,5):f.join(i)
        require(f.controller.snapshot()['players']==4,'Original four profiles did not join')
        f.stop();checks.append(dict(check='128 upgrade adds only balloon defaults, schema and one revision; four original profiles rejoin',passed=True))
        server_recovery.QUALIFIED_BUILDS|={args.build};server_recovery.MAX_QUALIFIED_BUILD=args.build
        r=Recovery(f.run_id,args.build);require(r.backup()['verified'],'Candidate backup failed')
        r.restore(Path(old['path']),digest(save.read_bytes()));f.controller.start()
        require(checkpoint(save.read_bytes())==expected,'Restored 128 checkpoint upgrades differently')
        f.stop();checks.append(dict(check='updated backup verifies; restored legacy checkpoint upgrades identically',passed=True));success=True
    finally:
        f.cleanup();result=dict(passed=success,previousBuild=128,build=args.build,checks=checks,liveFamilyServerAccessed=False)
        write(f.path/'keepy-upgrade-result.json',result);print(result,flush=True);print('EVIDENCE '+str(f.path/'keepy-upgrade-result.json'),flush=True)
if __name__=='__main__':main()
