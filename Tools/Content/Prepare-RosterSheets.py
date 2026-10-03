"""Import unchanged model sheets and describe editable runtime frame mappings."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import importlib.util
import json
import shutil

ROOT = PROJECT_ROOT
SOURCE = ROOT / 'SourceArt/Characters/ModelSheets'
DEST = ROOT / 'Unity/FamilyPlayset/Assets/FamilyPlayset/Art/Characters'
spec = importlib.util.spec_from_file_location('sheets', ROOT / 'Tools/Content/Prepare-CharacterSheets.py')
sheets = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sheets)
NAMES = ['front', 'three-quarter', 'profile', 'back', 'blink', 'wave', 'sit', 'carry',
         'contact-right', 'passing-left', 'contact-left', 'passing-right',
         'dance-left', 'dance-right', 'reach', 'happy']
RUNTIME = [1, 4, 5, 5, 8, 8, 9, 9, 10, 10, 11, 11, 6, 7, 12, 13]
FIRST = [('muffin', .80), ('socks', .66), ('bandit', 1.35), ('chilli', 1.25)]


def main():
    contract_path = ROOT / 'SourceArt/Characters/AnimationSheets/animation-contract.json'
    contract = json.loads(contract_path.read_text())
    existing = {c['id']: c for c in contract['characters']}
    for name, scale in FIRST:
        source = SOURCE / (name + '.png')
        described = sheets.describe(source, NAMES, 4)
        raw = described['frames']
        described['referenceHeight'] = raw[1]['pixels']['height'] - 4
        runtime = [{**raw[index], 'name': pose} for index, pose in zip(RUNTIME, sheets.NAMES)]
        walk = {**described, 'frames': [{**raw[index], 'name': 'walk-' + str(i)}
                for i, index in enumerate(RUNTIME[4:12])]}
        character = {'id': name, 'displayName': name.title(), 'scale': scale,
                     **described, 'frames': runtime, 'walk': walk}
        existing[name] = character
        folder = DEST / name
        folder.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, folder / 'actions.png')
        shutil.copyfile(source, folder / 'gentle-walk.png')
        shutil.copyfile(DEST / 'bluey/ground-shadow.png', folder / 'ground-shadow.png')
        (SOURCE / (name + '-frames.json')).write_text(json.dumps(described, indent=2) + '\n')
    contract['characters'] = list(existing.values())
    payload = json.dumps(contract, indent=2) + '\n'
    contract_path.write_text(payload)
    (DEST / 'animation-contract.json').write_text(payload)
    print('Prepared four model-sheet characters; existing Bluey/Bingo entries preserved.')


if __name__ == '__main__':
    main()
