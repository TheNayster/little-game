"""Verify and copy this game's fresh G3 Xcode export to the paired build Mac."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tarfile
from mac_connection import HOST,SSH,SCP,OPTIONS

ROOT=PROJECT_ROOT
REMOTE='/Users/nayster/Developer/LittleWeeps'


def sha(path):
    h=hashlib.sha256()
    with path.open('rb') as f:
        for b in iter(lambda:f.read(1048576),b''):h.update(b)
    return h.hexdigest()


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('--resume-archive',action='store_true');args=p.parse_args()
    if not 71<=args.build<=9999:raise ValueError('Fresh G3 build required')
    folder=ROOT/f'Builds/iOSFamilyLAN/G3-0.0.{args.build}'
    summary=json.loads((folder/'build-summary.json').read_text(encoding='utf-8-sig'))
    assert summary['result']=='Succeeded' and not summary['development'] and summary['version']==f'0.0.{args.build}'
    manifest=json.loads((folder/'artifact-manifest.json').read_text(encoding='utf-8-sig'))
    for entry in manifest:
        path=(folder/entry['path']).resolve()
        assert path.is_relative_to(folder.resolve()) and sha(path)==entry['sha256'],'Export changed'
    archive=ROOT/f'LocalData/ios-g3-0.0.{args.build}.tar.gz'
    if archive.exists() and not args.resume_archive:raise FileExistsError('Archive already exists; inspect prior transfer before retrying')
    def permissions(info):
        # Windows loses Unix executable bits. Give execution only to shipped
        # tools/scripts, not to arbitrary art/data or every exported file.
        name=Path(info.name).name
        if info.isdir() or name in ('il2cpp','bee_backend','MapFileParser') or name.endswith('.sh'):info.mode=0o755
        else:info.mode=0o644
        return info
    if not args.resume_archive:
        print('Packing verified Xcode export',flush=True)
        with tarfile.open(archive,'w:gz',compresslevel=1) as tar:
            for name in ('Xcode','build-summary.json','artifact-manifest.json','source-manifest.json'):
                tar.add(folder/name,arcname=name,filter=permissions)
    remote_folder=f'{REMOTE}/Builds/G3-0.0.{args.build}'
    if not args.resume_archive:
        subprocess.run([str(SSH),*OPTIONS,HOST,f'test ! -e {remote_folder} && mkdir -p {remote_folder}'],check=True)
        print('Transferring archive',flush=True)
        subprocess.run([str(SCP),*OPTIONS,str(archive),f'{HOST}:{remote_folder}/export.tar.gz'],check=True)
    digest=sha(archive)
    # This remote code is fixed, with a validated integer build and hex digest.
    script=f'''import hashlib,json,pathlib,tarfile
r=pathlib.Path({remote_folder!r})
archive=r/'export.tar.gz'
assert hashlib.sha256(archive.read_bytes()).hexdigest()=={digest!r},'Archive hash mismatch'
assert not (r/'Xcode').exists(),'Never overwrite an existing export'
with tarfile.open(archive) as tar:
    for member in tar.getmembers():
        assert (r/member.name).resolve().is_relative_to(r) and (member.isfile() or member.isdir())
    # Apple's bundled Python 3.9.6 predates extraction filters. The verified
    # archive is ours; every member above must be a contained regular file/dir.
    if hasattr(tarfile,'data_filter'):tar.extractall(r,filter='data')
    else:tar.extractall(r)
manifest=json.loads((r/'artifact-manifest.json').read_text(encoding='utf-8-sig'))
for entry in manifest:
    path=(r/entry['path']).resolve()
    assert path.is_relative_to(r) and hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'],'Export file mismatch'
print(json.dumps(dict(build={args.build},archiveSha256={digest!r},filesVerified=len(manifest),remoteRoot=str(r))))
'''
    result=subprocess.run([str(SSH),*OPTIONS,HOST,'python3 -'],input=script,text=True,capture_output=True)
    if result.returncode:raise RuntimeError('Remote export verification failed: '+result.stderr)
    record=json.loads(result.stdout)
    (ROOT/f'LocalData/ios-{args.build}-transfer.json').write_text(json.dumps(record,indent=2)+'\n')
    print(result.stdout.strip(),flush=True)


if __name__=='__main__':main()
