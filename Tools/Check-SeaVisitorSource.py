"""Verify the art-only follow-up against its actual build and prior rules."""
import hashlib,json
from pathlib import Path
root=Path(__file__).resolve().parent.parent
out=root/'docs/implementation/evidence/sea-visitor-models-2026-09-30';out.mkdir(parents=True,exist_ok=True)
new=json.loads((root/'Builds/NetworkProbe/G3-0.0.307/source-manifest.json').read_text(encoding='utf-8-sig'))
old=json.loads((root/'Builds/NetworkProbe/G3-0.0.305/source-manifest.json').read_text(encoding='utf-8-sig'))
prior={p['path']:p['sha256'] for p in old['files']};records=[]
for item in new['files']:
    p=item['path']
    if '/Code/' in p and p.endswith('.cs') or '/Resources/BeachArt/' in p and (p.endswith('-v2.png') or p.endswith('-v2.json')):
        actual=hashlib.sha256((root/p).read_bytes()).hexdigest()
        assert actual==item['sha256'],f'Built source changed: {p}'
        records.append(dict(path=p,sha256=actual,sameAs305=prior.get(p)==actual))
changed=[p['path'] for p in records if p['path'].endswith('.cs') and not p['sameAs305']]
assert set(changed)=={'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloBeachShore.cs','Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenVerification.cs'},changed
assert all(p['sameAs305'] for p in records if '/Code/Core/' in p['path'] or p['path'].endswith('/NetworkProbe.cs'))
(out/'source-check.json').write_text(json.dumps(dict(passed=True,build=307,schema=40,content=46,sharedRulesAndNetworkIdenticalTo305=True,changedCode=changed,files=records),indent=2),encoding='utf-8')
print('PASS built runtime/source art; shared rules and network source unchanged.')
