"""Qualify the installed home server's additive catch-up using a disposable family."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,json
from pathlib import Path
from recovery_fixture import RecoveryFixture
from parent_server import ParentServer
from server_recovery import Recovery,checkpoint,digest
import server_recovery
from shared_garden_runtime import require,write

def prior_shape(new,old):
    if isinstance(old,dict):return {k:prior_shape(new[k],v) for k,v in old.items()}
    if isinstance(old,list):
        require(len(new)>=len(old),'Existing list entries lost')
        return [prior_shape(n,v) for n,v in zip(new,old)]
    return new

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    f=RecoveryFixture(128);checks=[];passed=False;save=f.path/'server-world/world.save'
    require(f.controller.isolated,'Disposable enrolled family required')
    server_recovery.QUALIFIED_BUILDS|={args.build};server_recovery.MAX_QUALIFIED_BUILD=max(args.build,server_recovery.MAX_QUALIFIED_BUILD)
    print('EVIDENCE '+str(f.path/'home-catchup.json'),flush=True)
    try:
        f.controller.start();f.stop();backup=Recovery(f.run_id,128).backup()
        before=checkpoint(save.read_bytes());require(before['schema']==4,'Expected deployed Home schema')
        enrollment={p.name:p.read_bytes() for p in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build);f.controller.start();after=checkpoint(save.read_bytes())
        require(after['schema']==15 and len(after['toys'])==107 and not any(r['created'] for r in after['secrets']),'Catch-up content missing')
        for key in before:
            if key not in ('schema','revision'):require(prior_shape(after[key],before[key])==before[key],'Prior gameplay field changed: '+key)
        require(all((f.path/n).read_bytes()==b for n,b in enrollment.items()),'Enrollment changed')
        for i in range(1,5):f.join(i)
        require(f.controller.snapshot()['players']==4,'Original four profiles did not rejoin')
        f.stop();require(Recovery(f.run_id,args.build).backup()['verified'],'New backup failed')
        checks.append('128 schema-4 world upgrades to 171 schema-15 with every previous field and protected enrollment retained; all four original players join; new backup validates')
        Recovery(f.run_id,args.build).restore(Path(backup['path']),digest(save.read_bytes()))
        f.controller.start();restored=checkpoint(save.read_bytes())
        require(restored==after,'Restored 128 backup did not migrate identically');f.stop()
        checks.append('Restoring the qualified 128 backup repeats the identical native additive migration')
        passed=True
    finally:
        f.cleanup();write(f.path/'home-catchup.json',dict(passed=passed,previousBuild=128,build=args.build,checks=checks,liveFamilyTouched=False));print('RESULT '+str(f.path/'home-catchup.json'),flush=True)
if __name__=='__main__':main()
