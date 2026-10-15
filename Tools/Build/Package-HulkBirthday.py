"""Attach checked local birthday content to a fresh Windows client artifact.

Owner: Hulk birthday delivery. Consumed by BirthdayArt's StreamingAssets reader.
Recovered content and receipts stay in ignored builds; maintained source stays in Git.
"""
import argparse,hashlib,json,shutil,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from project_paths import ROOT

def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d):p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--build',type=int,required=True);p.add_argument('--art',type=Path,required=True);p.add_argument('--receipt',type=Path,required=True);args=p.parse_args()
    build=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}';art=args.art.resolve();receipt=read(args.receipt)
    assert read(build/'build-summary.json')['content']==84,'Wrong birthday contract'
    for row in read(build/'artifact-manifest.json'):
        file=(build/row['path']).resolve();assert file.is_relative_to(build.resolve()) and sha(file)==row['sha256'],'Existing artifact differs: '+row['path']
    for row in receipt:
        file=(art/row['path']).resolve();assert file.is_relative_to(art) and sha(file)==row['sha256'],'Content differs from checked receipt: '+row['path']
    destination=build/'Client/LittleWeepsNetwork_Data/StreamingAssets/HulkBirthday'
    if destination.exists():
        assert all(sha(destination/row['path'])==row['sha256'] for row in receipt),'Preserve the existing different package'
        print('Already packaged with identical checked bytes');return
    stage=destination.with_name('HulkBirthday-packaging');stage.mkdir(parents=True,exist_ok=False)
    for row in receipt:
        file=stage/row['path'];file.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(art/row['path'],file);assert sha(file)==row['sha256']
    stage.rename(destination)
    write(build/'birthday-assets.json',receipt)
    write(build/'birthday-content-receipt.json',dict(build=args.build,manifestSha256=sha(destination/'actor-frames.json'),files=len(receipt),bytes=sum((destination/r['path']).stat().st_size for r in receipt),sourceReceipt=str(args.receipt),packagerSha256=sha(Path(__file__)),installedServerTouched=False))
    exclusions={'artifact-manifest.json','source-manifest.json','build-summary.json'}
    write(build/'artifact-manifest.json',[dict(path=f.relative_to(build).as_posix(),sha256=sha(f)) for f in sorted(build.rglob('*')) if f.is_file() and f.name not in exclusions])
    print(json.dumps(read(build/'birthday-content-receipt.json')))

if __name__=='__main__':main()
