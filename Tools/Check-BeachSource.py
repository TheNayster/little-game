"""Check only the changed runtime paths against the two recorded candidates."""
import hashlib,json
from pathlib import Path
root=Path(__file__).resolve().parent.parent
out=root/'docs/implementation/evidence/beach-waves-2026-09-30'
source=json.loads((root/'Builds/NetworkProbe/G3-0.0.305/source-manifest.json').read_text(encoding='utf-8-sig'))
previous=json.loads((root/'Builds/NetworkProbe/G3-0.0.304/source-manifest.json').read_text(encoding='utf-8-sig'))
prior={p['path']:p['sha256'] for p in previous['files']}
records=[]
for item in source['files']:
    p=item['path']
    if '/Code/' in p and p.endswith('.cs') or '/Resources/BeachArt/' in p and p.endswith('.png') or '/Resources/Scenery/beach-' in p and p.endswith('.png'):
        actual=hashlib.sha256((root/p).read_bytes()).hexdigest()
        assert actual==item['sha256'],f'Built source changed: {p}'
        records.append(dict(path=p,sha256=actual,sameAs304=prior.get(p)==actual))
changed=[p['path'] for p in records if not p['sameAs304']]
assert changed==['Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/GameCharacterVisual.cs'],changed
(out/'source-check.json').write_text(json.dumps(dict(passed=True,build=305,sharedRulesNetworkAndArtIdenticalTo304=True,onlyChangedRuntimePath=changed,files=records),indent=2),encoding='utf-8')
lines=(root/'LocalData/Logs/build-network-305.log').read_text(encoding='utf-8',errors='replace').splitlines()
(out/'unity-json-check.txt').write_text('\n'.join(line for line in lines if 'Unity JSON:' in line)+'\n',encoding='utf-8')
print('PASS final runtime source hashes; only beach floor adapter differs from native 304.')
