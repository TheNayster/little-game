"""Read-only backup of this app's Documents and preferences before an in-place iPad update."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,json,re,subprocess
from pathlib import Path
from mac_connection import ROOT,SSH,OPTIONS,HOST,SCP


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--device',required=True)
    p.add_argument('--label',choices=('ipad7','ipad9','iphone'),required=True);p.add_argument('--build',type=int,required=True)
    p.add_argument('--copy-timeout',type=int,default=40,help='Seconds per app-data copy; paired Wi-Fi can need longer than USB.')
    args=p.parse_args();assert re.fullmatch(r'[A-Za-z0-9-]{20,64}',args.device) and 56<=args.build<=9999
    assert 40<=args.copy_timeout<=180
    data=dict(device=args.device,label=args.label,build=args.build,copyTimeout=args.copy_timeout)
    code='data='+repr(data)+'\n'+r'''
import pathlib,subprocess,datetime,json,hashlib,plistlib,uuid
base=pathlib.Path.home()/'Developer/LittleWeeps'
out=base/'Backups'/('before-'+str(data['build'])+'-'+data['label']+'-'+uuid.uuid4().hex)
out.mkdir(parents=True,exist_ok=False)
bundle='com.littleweeps.familyplayset'
for source,dest in [('Documents','Documents'),('Library/Preferences/'+bundle+'.plist','preferences.plist')]:
    r=subprocess.run(['xcrun','devicectl','device','copy','from','--device',data['device'],'--domain-type','appDataContainer',
        '--domain-identifier',bundle,'--source',source,'--destination',str(out/dest),'--timeout',str(data['copyTimeout']),'--json-output',str(out/(dest+'.copy.json'))],capture_output=True,text=True)
    (out/(dest+'.copy.log')).write_text(r.stdout+r.stderr)
    assert r.returncode==0,'App data copy failed; no update may proceed'
# An enrolled phone can have only its paired solo branch, without ever opening
# family-local. Verify every existing solo world instead of requiring that name.
saves=sorted((out/'Documents/SoloPrototype').glob('*/world.save'))
assert saves,'No existing solo worlds found; inspect the backup before updating'
verified=[]
for save in saves:
    raw=save.read_bytes();header,digest,payload=raw.split(b'\n',2)
    assert header==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(payload).hexdigest().encode()==digest,'Save checksum failed'
    world=json.loads(payload);assert world['schema'] in (1,2,3,4,5) and world['players'] and world['toys'],'Unexpected solo save'
    verified.append(dict(path=save.relative_to(out).as_posix(),revision=world['revision'],schema=world['schema']))
preferences=plistlib.loads((out/'preferences.plist').read_bytes());assert isinstance(preferences,dict)
files=[dict(path=str(f.relative_to(out)),sha256=hashlib.sha256(f.read_bytes()).hexdigest()) for f in out.rglob('*') if f.is_file() and not f.name.endswith(('.copy.log','.copy.json'))]
record=dict(passed=True,forBuild=data['build'],label=data['label'],device=data['device'],remoteBackup=str(out),files=files,
    savedWorldRevision=verified[0]['revision'],verifiedSoloWorlds=verified,settings={k:v for k,v in preferences.items() if k.startswith('solo.prototype.')})
(out/'backup.json').write_text(json.dumps(record,indent=2));print(json.dumps(record))
'''
    r=subprocess.run([str(SSH),*OPTIONS,HOST,'python3 -'],input=code,text=True,capture_output=True,timeout=2*args.copy_timeout+20)
    if r.returncode:raise RuntimeError(r.stderr+'\nBackup incomplete; no install performed')
    record=json.loads(r.stdout);dest=ROOT/'LocalData/iPadBackups'/Path(record['remoteBackup']).name
    dest.parent.mkdir(parents=True,exist_ok=True)
    subprocess.run([str(SCP),*OPTIONS,'-r',HOST+':'+record['remoteBackup'],str(dest)],check=True,timeout=120)
    import hashlib
    for entry in record['files']:
        f=(dest/entry['path']).resolve();assert f.is_relative_to(dest.resolve()) and hashlib.sha256(f.read_bytes()).hexdigest()==entry['sha256']
    print(json.dumps(dict(passed=True,forBuild=args.build,label=args.label,filesVerified=len(record['files']),localBackup=str(dest),remoteBackup=record['remoteBackup'],savedWorldRevision=record['savedWorldRevision'])))


if __name__=='__main__':main()
