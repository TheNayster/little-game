"""Confirm the checked native artifact contains current scoped runtime source."""
import argparse,hashlib,json
from pathlib import Path
root=Path(__file__).resolve().parent.parent
parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
folder=root/f'Builds/NetworkProbe/G3-0.0.{args.build}'
manifest=json.loads((folder/'source-manifest.json').read_text(encoding='utf-8-sig'))
prior=json.loads((root/'Builds/NetworkProbe/G3-0.0.307/source-manifest.json').read_text(encoding='utf-8-sig'))
old={p['path']:p['sha256'] for p in prior['files']};records=[]
for item in manifest['files']:
    path=item['path']
    if '/Code/' in path and path.endswith('.cs') or '/Resources/BeachArt/' in path and not path.endswith('.meta'):
        actual=hashlib.sha256((root/path).read_bytes()).hexdigest()
        assert actual==item['sha256'],f'Built source changed: {path}'
        records.append(dict(path=path,sha256=actual,sameAs307=old.get(path)==actual))
changed={Path(p['path']).name for p in records if p['path'].endswith('.cs') and not p['sameAs307']}
expected={'BeachWaveRide.cs','BeachShore.cs','BeachSeagulls.cs','FamilySession.cs','SoloWorld.cs','WorldLayout.cs','SoloScreen.cs','SoloHome.cs','SoloMiniGames.cs','SoloWaveRide.cs','SoloScenery.cs','GameCharacterVisual.cs','NetworkProbeBuild.cs','WaveRideJsonTests.cs','NetworkGardenVerification.cs','NetworkProbe.cs'}
assert changed==expected,changed^expected
assert all(p['sameAs307'] for p in records if '/Resources/BeachArt/' in p['path']),'Visitor artwork changed'
summary=json.loads((folder/'build-summary.json').read_text(encoding='utf-8-sig'))
assert summary['content']==55 and summary['schema']==43 and all(b['result']=='Succeeded' and b['errors']==0 for b in summary['builds'])
out=root/'docs/implementation/evidence/beach-wave-ride-2026-09-30';out.mkdir(parents=True,exist_ok=True)
(out/'source-check.json').write_text(json.dumps(dict(passed=True,build=args.build,schema=43,content=55,changedCode=sorted(changed),visitorArtworkIdenticalTo307=True,files=records),indent=2),encoding='utf-8')
print('PASS current runtime matches built artifact; changes scoped to wave ride and visitor artwork preserved.')
