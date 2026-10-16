"""Append source-bound traffic, all window patterns and petrol grip to a fresh local pack."""
import argparse,hashlib,json,shutil
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--base',type=Path,required=True);p.add_argument('--base-receipt',type=Path,required=True);p.add_argument('--patterns',type=Path,required=True);p.add_argument('--output',type=Path,required=True)
a=p.parse_args();base=a.base.resolve();extra=a.patterns.resolve();out=a.output.resolve()
if out.exists():p.error('Preserve existing outputs; select a new directory.')
sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
receipt=json.loads(a.base_receipt.read_text());assert receipt and len({x['path'] for x in receipt})==len(receipt)
for row in receipt:
 f=(base/row['path']).resolve();assert f.is_relative_to(base) and sha(f)==row['sha256'],'Base changed: '+row['path']
m=json.loads((base/'drive-content.json').read_text());new=json.loads((extra/'pattern-content.json').read_text());shutil.copytree(base,out)
keys={x['key'] for x in m['pictures']}
for row in new['pictures']:
 assert row['key'] not in keys,'Duplicate picture '+row['key'];keys.add(row['key']);f=(extra/row['file']).resolve();assert f.is_relative_to(extra) and f.is_file()
 destination=out/row['file'];assert not destination.exists(),'Picture filename collision';shutil.copyfile(f,destination);m['pictures'].append(row)
m['windowPatterns']=new['patterns'];m['windowPatternLimits']=new['limits'];m['additionalTraffic']=json.loads((Path(__file__).resolve().parent/'PatternInputs.json').read_text())['vehicles'];m['nozzleGrip']={'key':'nozzle','handAlignment':2,'handHoldingAngle':0.5,'holdingAngleOverrideWeightWhenWalking':0.6,'additionalSnapDistance':0.3,'gripEvidence':'Source DriveTimeHoseHandle.hole points to Environment/Petrol_Handle/Group/Connectivity/Hole_Logical. Picture holdPivot projects that exact transform.'}
shutil.copyfile(extra/'pattern-content.json',out/'pattern-content.json');(out/'drive-content.json').write_text(json.dumps(m,indent=2)+'\n')
final=[dict(path=f.relative_to(out).as_posix(),sha256=sha(f),bytes=f.stat().st_size) for f in sorted(out.rglob('*')) if f.is_file()];out.with_name(out.name+'-receipt.json').write_text(json.dumps(final,indent=2)+'\n')
print(json.dumps(dict(pictures=len(m['pictures']),patterns=len(m['windowPatterns']),vehicles=len(m['additionalTraffic']),files=len(final),bytes=sum(x['bytes'] for x in final))))
