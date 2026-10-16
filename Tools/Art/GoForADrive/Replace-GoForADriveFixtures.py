"""Replace only three reviewed source fixture picture rows in a new immutable media pack."""
import argparse,hashlib,json,shutil
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('--base',type=Path,required=True);p.add_argument('--base-receipt',type=Path,required=True);p.add_argument('--fixtures',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
base=a.base.resolve();source=a.fixtures.resolve();out=a.output.resolve();sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
if out.exists():p.error('Select a fresh output; preserve all previous packs.')
receipt=json.loads(a.base_receipt.read_text());assert receipt and len({r['path'] for r in receipt})==len(receipt)
for r in receipt:
 f=(base/r['path']).resolve();assert f.is_relative_to(base) and sha(f)==r['sha256'],'Base changed: '+r['path']
m=json.loads((base/'drive-content.json').read_text());fixtures=json.loads((source/'fixture-content.json').read_text());assert {r['key'] for r in fixtures['pictures']}=={'toilet','rubble','petrol-stop'}
shutil.copytree(base,out);corrections=[]
for row in fixtures['pictures']:
 target=next(r for r in m['pictures'] if r['key']==row['key']);previous=target.copy();new=(source/row['file']).resolve();assert new.is_relative_to(source) and new.is_file();assert not (out/row['file']).exists(),'New fixture filename must be unique';shutil.copyfile(new,out/row['file']);target.clear();target.update(row)
 selection=(source/(row['key']+'-selection.txt')).read_text().split('RENDERERS')[0].strip().splitlines();corrections.append(dict(key=row['key'],source=row['source'],sourceSha256=row['sourceSha256'],selectedPaths=selection,previousFile=previous['file'],correctedFile=row['file']))
m['fixtureCorrections']=corrections;m['fixtureLimits']=fixtures['limits'];(out/'drive-content.json').write_text(json.dumps(m,indent=2)+'\n');shutil.copyfile(source/'fixture-content.json',out/'fixture-content.json')
final=[dict(path=f.relative_to(out).as_posix(),sha256=sha(f),bytes=f.stat().st_size) for f in sorted(out.rglob('*')) if f.is_file()];out.with_name(out.name+'-receipt.json').write_text(json.dumps(final,indent=2)+'\n');print(json.dumps(dict(pictures=len(m['pictures']),files=len(final),bytes=sum(r['bytes'] for r in final),corrected=corrections)))
