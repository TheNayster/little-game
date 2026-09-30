"""Import unchanged model sheets and describe editable runtime frame mappings."""
from pathlib import Path
import importlib.util
import json
import shutil
import re
import uuid

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'SourceArt/Characters/ModelSheets'
DEST = ROOT / 'Unity/FamilyPlayset/Assets/FamilyPlayset/Art/Characters'
spec = importlib.util.spec_from_file_location('sheets', ROOT / 'Tools/Prepare-CharacterSheets.py')
sheets = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sheets)
NAMES = ['front', 'three-quarter', 'profile', 'back', 'blink', 'wave', 'sit', 'carry',
         'contact-right', 'passing-left', 'contact-left', 'passing-right',
         'dance-left', 'dance-right', 'reach', 'happy']
RUNTIME = [1, 4, 5, 5, 8, 8, 9, 9, 10, 10, 11, 11, 6, 7, 12, 13]
REGISTRY = ROOT / 'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/PlayableCharacters.cs'


def main():
    contract_path = ROOT / 'SourceArt/Characters/AnimationSheets/animation-contract.json'
    contract = json.loads(contract_path.read_text())
    existing = {c['id']: c for c in contract['characters']}
    cast = re.findall(r'new Entry\("[^"]+","([^"]+)","([^"]+)",([.\d]+)f\)', REGISTRY.read_text())
    assert len(cast) == 37 and len({entry[0] for entry in cast}) == 37
    for name, display_name, scale in cast:
        if name in ('bluey', 'bingo'):continue
        scale=float(scale)
        source = SOURCE / (name + '.png')
        described = json.loads((SOURCE / (name + '-frames.json')).read_text())
        assert described['sourceSha256'] == sheets.hashlib.sha256(source.read_bytes()).hexdigest()
        raw = described['frames']
        described['referenceHeight'] = raw[1]['pixels']['height'] - 4
        runtime = [{**raw[index], 'name': pose} for index, pose in zip(RUNTIME, sheets.NAMES)]
        walk = {**described, 'frames': [{**raw[index], 'name': 'walk-' + str(i)}
                for i, index in enumerate(RUNTIME[4:12])]}
        character = {'id': name, 'displayName': display_name, 'scale': scale,
                     **described, 'frames': runtime, 'walk': walk}
        existing[name] = character
        folder = DEST / name
        folder.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, folder / 'actions.png')
        shutil.copyfile(source, folder / 'gentle-walk.png')
        shutil.copyfile(DEST / 'bluey/ground-shadow.png', folder / 'ground-shadow.png')
        shadow_meta = folder / 'ground-shadow.png.meta'
        if not shadow_meta.exists():
            template=(DEST / 'bluey/ground-shadow.png.meta').read_text()
            shadow_meta.write_text(re.sub(r'^guid: \w+', 'guid: '+uuid.uuid4().hex, template, flags=re.M))
    contract['characters'] = list(existing.values())
    payload = json.dumps(contract, indent=2) + '\n'
    contract_path.write_text(payload)
    (DEST / 'animation-contract.json').write_text(payload)
    assert len(contract['characters']) == 37
    print('Prepared all 37 runtime characters; existing Bluey/Bingo art preserved.')


if __name__ == '__main__':
    main()
