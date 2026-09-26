"""Parent USB enrollment into the installed iPad client's create-only Keychain store."""
import argparse,hashlib,json,subprocess,uuid
from pathlib import Path
from family_pairing import read_record
from mac_connection import ROOT,SSH,OPTIONS,HOST


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--build',type=int,required=True)
    p.add_argument('--label',choices=('ipad7','ipad9','iphone'),required=True);p.add_argument('--family',required=True)
    p.add_argument('--restore-backup',type=Path,help='Verify the original client identity against its retained pre-update backup')
    p.add_argument('--player',type=int,choices=range(1,5),required=True);args=p.parse_args()
    assert uuid.UUID(args.family).hex==args.family
    installed=json.loads((ROOT/f'LocalData/ios-{args.build}-{args.label}-installed.json').read_text(encoding='utf-8'))
    assert installed['installed'] and installed['signedArtifactVerified'] and installed['build']==args.build
    pair=read_record(ROOT/'LocalData/FamilyLAN'/args.family/f'player-{args.player}.pairing')
    assert pair['role']=='client' and pair['worldId']==args.family and not pair.get('privateKey') and not pair.get('members')
    if args.restore_backup:
        # A signing-route change can leave the previous Keychain group unreadable.
        # Restore only the protected original player matching this device's save.
        backup=args.restore_backup.resolve()
        assert backup.is_relative_to((ROOT/'LocalData/iPadBackups').resolve())
        before=json.loads((backup/'backup.json').read_text(encoding='utf-8'))
        assert before['passed'] and before['device']==installed['device'] and before['forBuild']==args.build
        for entry in before['files']:
            file=(backup/entry['path']).resolve()
            assert file.is_relative_to(backup) and hashlib.sha256(file.read_bytes()).hexdigest()==entry['sha256']
        save=backup/'Documents/SoloPrototype'/('paired-'+pair['worldId']+'-'+pair['profile'])/'world.save'
        raw=save.read_bytes();header,digest,payload=raw.split(b'\n',2)
        assert header==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(payload).hexdigest().encode()==digest
        assert any(player['id']==pair['profile'] for player in json.loads(payload)['players'])
        assert (backup/'Documents/FamilyLAN/enrollment-status.txt').read_text().strip()=='paired'
    data=dict(build=args.build,label=args.label,device=installed['device'],pair=pair)
    # The secret travels over SSH stdin, never an argument, Windows file, log,
    # source asset or public advertisement. The Mac staging directory is private.
    code='data='+repr(data)+'\n'+r'''
import pathlib,subprocess,json,tempfile,os
bundle='com.littleweeps.familyplayset'
with tempfile.TemporaryDirectory(prefix='little-weeps-enroll-') as temp:
    root=pathlib.Path(temp);root.chmod(0o700)
    def run(cmd):
        r=subprocess.run(['xcrun','devicectl',*cmd[:3],'--device',data['device'],'--timeout','35',*cmd[3:]],capture_output=True)
        assert r.returncode==0,'Device transfer/launch failed; enrollment not confirmed'
    run(['device','copy','from','--domain-type','appDataContainer','--domain-identifier',bundle,'--source','Documents/FamilyLAN','--destination',str(root/'before')])
    assert (root/'before/enrollment-status.txt').read_text().strip()=='unpaired','Existing enrollment needs review; no identity replaced'
    assert not list((root/'before').rglob('enrollment.json')),'Existing enrollment inbox must be resolved first'
    source=root/'enrollment.json'
    with source.open('x',encoding='utf-8') as stream:
        os.chmod(source,0o600);json.dump(data['pair'],stream,separators=(',',':'))
    run(['device','copy','to','--domain-type','appDataContainer','--domain-identifier',bundle,'--source',str(source),'--destination','Documents/FamilyLAN/enrollment.json'])
    source.unlink()
    run(['device','process','launch','--terminate-existing',bundle])
print(json.dumps(dict(staged=True,build=data['build'],label=data['label'],next='Verify paired status, consumed inbox and current build/session; staging is not enrollment success')))
'''
    r=subprocess.run([str(SSH),*OPTIONS,HOST,'python3 -'],input=code,text=True,capture_output=True,timeout=120)
    if r.returncode:raise RuntimeError('Enrollment step failed; inspect device state before retrying. '+r.stderr)
    print(r.stdout)


if __name__=='__main__':main()
