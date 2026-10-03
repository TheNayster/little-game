"""Check staggered 79/newer client/server updates in isolated loopback worlds."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime,timezone
from shared_garden_runtime import ROOT,Run,wait,require,write


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);args=p.parse_args()
    checks=[]
    for server_build,client_build in [(args.build,79),(79,args.build),(args.build,83),(83,args.build)]:
        authority=Run(server_build);guest=None
        try:
            server=authority.start('server')
            guest=Run(client_build,resume=authority.run_id);guest.port=authority.port
            client=guest.start('client','player-1')
            require(client.input('inspect')['connected'],'Mixed-version admission failed')
            client.input('press',role='bucket-1');client.input('release',x=150,y=340)
            wait(lambda:next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['water']==3,'mixed-version fill')
            wait(lambda:next(t for t in client.state()['view']['toys'] if t['id']=='bucket-1')['water']==3,'mixed-version shared view')
            if args.build>=86:
                # Capability is additive: older peers must receive no recovery
                # extension traffic and must not be credited with a replica.
                require(not (client.out/'recovery-evidence.json').exists(), 'Legacy peer unexpectedly negotiated recovery')
                require(not (server.out/'recovery-evidence.json').exists(), 'Legacy peer received recovery credit')
            checks.append(dict(serverBuild=server_build,clientBuild=client_build,passed=True,runId=authority.run_id))
            print(f'PASS server {server_build} / client {client_build}: admission, pickup, fill and shared snapshot',flush=True)
        finally:
            if guest:guest.close()
            authority.close()
    path=ROOT/f'LocalData/Verification/garden-compatibility-{args.build}.json'
    require(not path.exists(),'Keep previous evidence; choose a fresh output/build')
    write(path,dict(passed=True,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
        scope='79/83 staggered-update admission and basic interaction. On recovery-capable builds, legacy peers negotiate no recovery traffic or durable credit. 79 lacks reset cues/timers.',physicalDevicesAccessed=False))
    print('Evidence: '+str(path),flush=True)


if __name__=='__main__':main()
