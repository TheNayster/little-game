"""Stable game root and the maintained source/tool relocation map."""
import json
from pathlib import Path
ROOT=next(p for p in Path(__file__).resolve().parents if (p/'Unity/FamilyPlayset/ProjectSettings/ProjectVersion.txt').is_file() and (p/'.git').exists())
TOOLS=ROOT/'Tools'
def migrated(path):
    data=json.loads((ROOT/'docs/project-layout.json').read_text(encoding='utf-8-sig'))
    for key in ('sourceMoves','toolMoves','authoringMoves'):
        rows=data.get(key,{})
        if path in rows:return ROOT/rows[path]
    return ROOT/path

def resource_path(identifier):
    data=json.loads((ROOT/'docs/project-layout.json').read_text(encoding='utf-8-sig'))
    head,sep,tail=str(identifier).partition('/')
    if head=='Scenery':
        name=Path(tail).stem
        target=data['sceneryResources'].get(name)
        if target:return ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources'/(target+Path(tail).suffix)
    prefix=data['resourceAliases'].get(head,head)
    return ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources'/prefix/tail
def authoring_path(path):
    data=json.loads((ROOT/'docs/project-layout.json').read_text(encoding='utf-8-sig'))
    for old,new in data['authoringMoves'].items():
        if path==old or path.startswith(old+'/'):return ROOT/(new+path[len(old):])
    return ROOT/path

def verification_artifact(relative):
    """Resolve only an explicitly requested release for read-only tests/previews.

    Every caller still verifies its build/version/manifests. This is not a phone
    update fallback and never selects a different build number.
    """
    rel=Path(relative)
    if rel.is_absolute() or '..' in rel.parts:raise ValueError('Expected exact relative artifact path')
    data=json.loads((ROOT/'docs/project-layout.json').read_text(encoding='utf-8-sig'))
    candidates=[ROOT/base/rel for base in data['artifactRoots'] if (ROOT/base/rel).is_dir()]
    if len(candidates)>1:
        manifests=[(p/'artifact-manifest.json').read_bytes() for p in candidates]
        if any(v!=manifests[0] for v in manifests[1:]):raise ValueError('Ambiguous release copies')
    return candidates[0] if candidates else ROOT/'Builds'/rel
