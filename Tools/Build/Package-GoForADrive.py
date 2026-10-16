"""Package a checked Go for a Drive content receipt into a fresh PC candidate."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sys
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from project_paths import ROOT

def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def write(path,value):path.write_text(json.dumps(value,indent=2)+'\n',encoding='utf-8')

def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--build',type=int,required=True);p.add_argument('--art',type=Path,required=True);p.add_argument('--receipt',type=Path,required=True)
    a=p.parse_args();build=ROOT/f'Builds/NetworkProbe/G3-0.0.{a.build}';art=a.art.resolve();receipt=read(a.receipt)
    assert art.is_relative_to(ROOT) and receipt,'Missing project-local content receipt'
    summary=read(build/'build-summary.json');assert summary['builds'] and all(r['result']=='Succeeded' for r in summary['builds']),'Candidate build failed'
    for row in read(build/'artifact-manifest.json'):
        path=(build/row['path']).resolve();assert path.is_relative_to(build) and sha(path)==row['sha256'],'Candidate bytes changed'
    assert len({r['path'] for r in receipt})==len(receipt),'Duplicate receipt path'
    for row in receipt:
        path=(art/row['path']).resolve();assert path.is_relative_to(art) and sha(path)==row['sha256'],'Content changed: '+row['path']
    manifest=read(art/'drive-content.json')
    assert manifest['format']==1 and manifest['actions'] and manifest['audio'],'Incomplete drive pack'
    assert {'packing-background','driving-background','window-background','building-background','car','nomad','peanut'}<={r['key'] for r in manifest['pictures']},'Missing required scenery'
    destination=build/'Client/LittleWeepsNetwork_Data/StreamingAssets/GoForADrive'
    if destination.exists():
        assert all(sha(destination/r['path'])==r['sha256'] for r in receipt),'Existing package differs; preserve it'
        print('Already packaged identical Go for a Drive content');return
    stage=destination.with_name('GoForADrive-packaging');stage.mkdir(parents=True,exist_ok=False)
    for row in receipt:
        target=stage/row['path'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(art/row['path'],target)
        assert sha(target)==row['sha256']
    stage.rename(destination)
    record=dict(build=a.build,world='GoForADrive',content=summary['content'],files=len(receipt),bytes=sum((destination/r['path']).stat().st_size for r in receipt),manifestSha256=sha(destination/'drive-content.json'),sourceReceipt=str(a.receipt.resolve()),installedServerTouched=False)
    write(build/'GoForADrive-assets.json',receipt);write(build/'GoForADrive-content-receipt.json',record)
    excluded={'artifact-manifest.json','source-manifest.json','build-summary.json'}
    write(build/'artifact-manifest.json',[dict(path=f.relative_to(build).as_posix(),sha256=sha(f)) for f in sorted(build.rglob('*')) if f.is_file() and f.name not in excluded])
    print(json.dumps(record))

if __name__=='__main__':main()
