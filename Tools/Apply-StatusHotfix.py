"""Install the tested baseline-227 status fix only on a verified empty authority."""
import hashlib
from pathlib import Path
import shutil
import uuid
import argparse

from parent_bootstrap import controller_for
from parent_server import operation_lock
from pc_server_installation import home,installed,installation_lock,verify_bundle
from server_release import load
from shared_garden_runtime import ROOT,write,wait


def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--idle-test-devices',action='store_true',help='Use only after the parent explicitly permits disconnecting idle test devices')
    args=parser.parse_args()
    hotfix=ROOT/'Builds/ServerHotfix/227-status-io-1'
    receipt=load(hotfix/'hotfix.json');validation=load(hotfix/'validation.json')
    relative='Server/LittleWeepsNetwork_Data/Managed/LittleWeeps.NetworkProbe.dll'
    if not validation.get('passed') or not validation.get('fourClients') or receipt.get('changedArtifacts')!=[relative]:
        raise RuntimeError('Qualified status-only hotfix required')
    verify_bundle(hotfix,'Server')
    if sha(hotfix/relative)!=receipt['assemblySha256']:raise RuntimeError('Hotfix assembly changed')
    c=controller_for();installation=installed(ROOT)
    if not installation or installation['family']!=c.family or installation['build']!=227:
        raise RuntimeError('This fix applies only to the installed baseline 227 authority')
    if installation.get('statusHotfix')==receipt['id']:
        verify_bundle(home(ROOT)/'current','Server');print('Status hotfix already installed');return
    snapshot=c.snapshot()
    if snapshot['state']!='ready' or (snapshot['players']!=0 and not args.idle_test_devices):
        raise RuntimeError('Family play is occupied or uncertain. No server was stopped; leave family play briefly first.')
    if not c.network_prepared(force=True):raise RuntimeError('Existing network permission could not be verified')
    with installation_lock(ROOT):
        # Native stop-if-empty closes the join race after this preflight. Leave
        # the saved retry intent alone if the occupied-stop guard refuses.
        if args.idle_test_devices:
            # Explicit parent authorization for idle test devices. Use the
            # game's graceful stop/save path, never terminate the authority.
            with c.mutex,operation_lock(c.root):
                fresh=c.snapshot()
                if fresh['state']!='ready' or fresh['instanceId']!=snapshot['instanceId']:raise RuntimeError('Authority changed; no action taken')
                c.save_intent(False)
                output=c.root/fresh['instanceId'];control=output/'control.json'
                prior=load(control) if control.exists() else {}
                write(control,dict(serial=prior.get('serial',0)+1,kind='quit'))
                wait(lambda:c.snapshot()['state']=='stopped','authorized graceful stop',20)
                if c.snapshot()['save']['state']!='verified':raise RuntimeError('Checkpoint not verified after stop')
        else:c.stop(snapshot['instanceId'])
        current=home(ROOT)/'current';verify_bundle(current,'Server')
        save=c.root/'server-world/world.save';save_hash=sha(save)
        backup=home(ROOT)/('status-backup-'+uuid.uuid4().hex);backup.mkdir()
        for path in (current/relative,current/'artifact-manifest.json',home(ROOT)/'installation.json'):
            shutil.copy2(path,backup/path.name)
        next_record=dict(installation,statusHotfix=receipt['id'],statusHotfixSha256=receipt['assemblySha256'])
        write(home(ROOT)/'installation.pending.json',dict(kind='status-hotfix',next=next_record,previous=installation,backup=str(backup)))
        target=current/relative;staged=target.with_suffix('.hotfix-next')
        shutil.copy2(hotfix/relative,staged);staged.replace(target)
        manifest=load(current/'artifact-manifest.json')
        for row in manifest:
            if row['path']==relative:row['sha256']=receipt['assemblySha256']
        write(current/'artifact-manifest.json',manifest)
        verify_bundle(current,'Server')
        write(current/'status-hotfix.json',receipt)
        write(home(ROOT)/'installation.json',next_record)
        if sha(save)!=save_hash:raise RuntimeError('Saved-world bytes changed during publication; inspect before starting')
        (home(ROOT)/'installation.pending.json').unlink()
    result=c.start();status=c.snapshot()
    write(hotfix/'deployment.json',dict(passed=status['state']=='ready',result=result['result'],build=227,statusHotfix=receipt['id'],
          portPreserved=True,firewallChanged=False,saveUnchangedDuringPublication=True,checkpoint=status['save']['state'],players=status.get('players')))
    print('Status hotfix installed; original save, enrollment, port and firewall retained; server '+status['state'])


if __name__=='__main__':main()
