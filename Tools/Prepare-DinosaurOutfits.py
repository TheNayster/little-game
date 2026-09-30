"""Describe and copy the approved costume pixels; never repaint source art."""
from pathlib import Path
import importlib.util,json,shutil
ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'SourceArt/Characters/Outfits/Dinosaur'
DEST=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Art/Outfits/Dinosaur'
spec=importlib.util.spec_from_file_location('sheets',ROOT/'Tools/Prepare-CharacterSheets.py')
sheets=importlib.util.module_from_spec(spec);spec.loader.exec_module(sheets)
characters=[]
for name,scale in [('bluey',1),('bingo',.82)]:
    source=SOURCE/(name+'.png');described=sheets.describe(source,sheets.NAMES,4)
    walk={**described,'frames':described['frames'][4:12]}
    characters.append({'id':name,'displayName':name.title()+' dinosaur','scale':scale,**described,'walk':walk})
    folder=DEST/name;folder.mkdir(parents=True,exist_ok=True)
    shutil.copyfile(source,folder/'actions.png')
payload=json.dumps({'outfit':'dinosaur','characters':characters},indent=2)+'\n'
(SOURCE/'animation-contract.json').write_text(payload)
(DEST/'animation-contract.json').write_text(payload)
print('Prepared unchanged Bluey/Bingo onesie sheets: 16 poses and eight walk mappings each.')
