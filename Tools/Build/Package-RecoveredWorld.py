"""Attach a hash-checked recovered world pack to a fresh local Windows candidate."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sys
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from project_paths import ROOT


def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,value):p.write_text(json.dumps(value,indent=2)+'\n',encoding='utf-8')


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--build',type=int,required=True);p.add_argument('--content',type=int,required=True);p.add_argument('--world',choices=['SpideyHQ','HulkBirthday'],required=True);p.add_argument('--art',type=Path,required=True);p.add_argument('--receipt',type=Path,required=True);a=p.parse_args()
    build=ROOT/f'Builds/NetworkProbe/G3-0.0.{a.build}';art=a.art.resolve();receipt=read(a.receipt)
    assert read(build/'build-summary.json')['content']==a.content,'Wrong shared content contract'
    for row in read(build/'artifact-manifest.json'):
        f=(build/row['path']).resolve();assert f.is_relative_to(build.resolve()) and sha(f)==row['sha256'],'Candidate artifact changed'
    assert receipt and len({r['path'] for r in receipt})==len(receipt),'Missing/duplicate receipt paths'
    for row in receipt:
        f=(art/row['path']).resolve();assert f.is_relative_to(art) and sha(f)==row['sha256'],'Research content changed: '+row['path']
    destination=build/'Client/LittleWeepsNetwork_Data/StreamingAssets'/a.world
    if destination.exists():
        assert all(sha(destination/r['path'])==r['sha256'] for r in receipt),'Preserve differing existing package'
        print('Already packaged with matching bytes');return
    stage=destination.with_name(a.world+'-packaging');stage.mkdir(parents=True,exist_ok=False)
    for row in receipt:
        target=stage/row['path'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(art/row['path'],target);assert sha(target)==row['sha256']
    stage.rename(destination)
    write(build/(a.world+'-assets.json'),receipt)
    write(build/(a.world+'-content-receipt.json'),dict(build=a.build,content=a.content,world=a.world,files=len(receipt),bytes=sum((destination/r['path']).stat().st_size for r in receipt),manifestSha256=sha(destination/'actor-frames.json'),sourceReceipt=str(a.receipt.resolve()),packagerSha256=sha(Path(__file__)),installedServerTouched=False))
    excluded={'artifact-manifest.json','source-manifest.json','build-summary.json'}
    write(build/'artifact-manifest.json',[dict(path=f.relative_to(build).as_posix(),sha256=sha(f)) for f in sorted(build.rglob('*')) if f.is_file() and f.name not in excluded])
    print(json.dumps(read(build/(a.world+'-content-receipt.json'))))


if __name__=='__main__':main()
