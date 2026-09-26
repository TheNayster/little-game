"""Read this app's device-written state without changing enrollment, play or saves."""
import argparse
import json
import re
import subprocess
from mac_connection import ROOT, SSH, OPTIONS, HOST


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', type=int, required=True)
    parser.add_argument('--label', choices=('ipad7', 'ipad9', 'iphone'), required=True)
    parser.add_argument('--phase', required=True)
    args = parser.parse_args()
    assert 71 <= args.build <= 9999 and re.fullmatch(r'[a-z0-9-]{1,48}', args.phase)
    installed = json.loads((ROOT/f'LocalData/ios-{args.build}-{args.label}-installed.json').read_text())
    assert installed['installed'] and installed['build'] == args.build
    data = dict(build=args.build, label=args.label, device=installed['device'], phase=args.phase)
    code = 'data=' + repr(data) + '\n' + r'''
import hashlib,json,pathlib,subprocess,uuid,datetime
root=pathlib.Path.home()/'Developer/LittleWeeps'
out=root/'Logs'/('ipad-family-'+str(data['build']))/(data['label']+'-'+data['phase']+'-'+uuid.uuid4().hex)
out.mkdir(parents=True)
command=['xcrun','devicectl','device','copy','from','--device',data['device'],
    '--domain-type','appDataContainer','--domain-identifier','com.littleweeps.familyplayset',
    '--source','Documents','--destination',str(out/'Documents'),'--timeout','45']
copy=subprocess.run(command,capture_output=True,text=True)
assert copy.returncode==0,'App documents could not be read; no device changes made'
docs=out/'Documents';lan=docs/'FamilyLAN'
worlds=[p for p in lan.iterdir() if p.is_dir() and (p/'latest-instance.txt').exists()]
assert len(worlds)==1,'Ambiguous enrolled world; inspect private documents'
world=worlds[0];instance=(world/'latest-instance.txt').read_text().strip()
assert len(instance)==32 and all(c in '0123456789abcdef' for c in instance)
current=world/instance
record=dict(build=data['build'],label=data['label'],phase=data['phase'],
    utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),remoteEvidence=str(out),
    enrollment=(lan/'enrollment-status.txt').read_text().strip(),instance=instance,world=world.name)
for name in ('status.json','connection-evidence.json','recovery-evidence.json','continuation-evidence.json','view.json'):
    p=current/name
    if p.exists():record[name]=json.loads(p.read_text())
assert record['status.json']['build']=='0.0.'+str(data['build']),'Installed runtime does not match requested build'
def saved(p):
    raw=p.read_bytes();header,digest,payload=raw.split(b'\n',2)
    assert header==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(payload).hexdigest().encode()==digest,'Copied checkpoint checksum failed'
    return dict(path=p.relative_to(docs).as_posix(),sha256=hashlib.sha256(raw).hexdigest(),record=json.loads(payload))
record['replicas']=[saved(p) for p in (world/'client-recovery').glob('*/world.save')]
record['adventures']=[saved(p) for p in (world/'client-adventures').glob('*/*/world.save')]
record['selections']=[saved(p) for p in (world/'client-adventures').glob('*/selection.save')]
record['solo']=[saved(p) for p in (docs/'SoloPrototype').glob('*/world.save')]
(out/'inspection.json').write_text(json.dumps(record,indent=2));print(json.dumps(record))
'''
    result = subprocess.run([str(SSH), *OPTIONS, HOST, 'python3 -'], input=code,
                            text=True, capture_output=True, timeout=75)
    if result.returncode:
        raise RuntimeError(result.stderr + '\nRead-only device inspection incomplete.')
    record = json.loads(result.stdout)
    path = ROOT/f'LocalData/ios-{args.build}-{args.label}-{args.phase}.json'
    if path.exists():
        raise FileExistsError('Use a new phase label; prior device evidence must be retained.')
    path.write_text(json.dumps(record, indent=2)+'\n')
    # Complete snapshots stay private; terminal output is a small observation.
    print(json.dumps(dict(label=args.label, phase=args.phase, enrollment=record['enrollment'],
        status=record.get('status.json'), recovery=record.get('recovery-evidence.json'),
        continuation=record.get('continuation-evidence.json'),
        replicas=len(record['replicas']), adventures=len(record['adventures']),
        selectedBranches=[s['record'].get('branch') for s in record['selections']],
        output=str(path)), indent=2))


if __name__ == '__main__':
    main()
