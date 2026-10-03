"""Describe and import the unchanged generated sheets; never repaint pixels."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import hashlib
import json
import shutil
from PIL import Image

ROOT = PROJECT_ROOT
SOURCE = ROOT / 'SourceArt/Characters/AnimationSheets'
DEST = ROOT / 'Unity/FamilyPlayset/Assets/FamilyPlayset/Art/Characters'
NAMES = ['idle', 'blink', 'wave-a', 'wave-b', 'walk-0', 'walk-1', 'walk-2', 'walk-3',
         'walk-4', 'walk-5', 'walk-6', 'walk-7', 'sit', 'carry', 'dance-a', 'dance-b']


def character_bounds(alpha, cell):
    left, top, right, bottom = cell
    remaining = {(x, y) for y in range(top, bottom) for x in range(left, right)
                 if alpha.getpixel((x, y)) >= 32}
    largest = set()
    while remaining:
        seed = remaining.pop(); component = {seed}; pending = [seed]
        while pending:
            x, y = pending.pop()
            for neighbor in ((x-1, y), (x+1, y), (x, y-1), (x, y+1)):
                if neighbor in remaining:
                    remaining.remove(neighbor); component.add(neighbor); pending.append(neighbor)
        if len(component) > len(largest):
            largest = component
    assert len(largest) > 1000
    return (min(x for x, y in largest), min(y for x, y in largest),
            max(x for x, y in largest)+1, max(y for x, y in largest)+1)


def describe(source, names, rows):
    image = Image.open(source)
    assert image.mode == 'RGBA'
    width, height = image.size
    alpha = image.getchannel('A')
    frames = []
    for index, pose in enumerate(names):
        col, row = index % 4, index // 4
        cell = (round(col * width / 4), round(row * height / rows),
                round((col + 1) * width / 4), round((row + 1) * height / rows))
        # Bounds ignore negligible matte noise; source pixels and their
        # original antialiased transparency remain completely unchanged.
        # Some generated ear tips enter a neighboring grid cell. Only the
        # connected character silhouette establishes its crop and floor.
        left, top, right, bottom = character_bounds(alpha, cell)
        scan_y = round(top + (bottom - top) * .33)
        solid = [x for x in range(left, right) if alpha.getpixel((x, scan_y)) >= 128]
        assert solid
        runs = []
        for x in solid:
            if not runs or x > runs[-1][-1] + 1:
                runs.append([x])
            else:
                runs[-1].append(x)
        head = max(runs, key=len)
        ground_x = (head[0] + head[-1]) / 2
        left, top = max(cell[0], left - 2), max(cell[1], top - 2)
        right, bottom_crop = min(cell[2], right + 2), min(cell[3], bottom + 2)
        frames.append({'name': pose, 'pixels': {'x': left, 'y': top, 'width': right-left, 'height': bottom_crop-top},
                       'ground': {'x': ground_x, 'y': bottom}})
    return {'source': source.relative_to(ROOT).as_posix(),
            'sourceSha256': hashlib.sha256(source.read_bytes()).hexdigest(),
            'width': width, 'height': height,
            'referenceHeight': frames[0]['pixels']['height'] - 4, 'frames': frames}


def main():
    manifest = {'schema': 2, 'styleReference': 'SourceArt/Characters/SelectedReference/bluey-movement-sheet.png',
                'characters': []}
    for name, scale in [('bluey', 1), ('bingo', .82)]:
        source = SOURCE / (name + '-actions.png')
        walk = SOURCE / (name + '-gentle-walk.png')
        character = {'id': name, 'displayName': name.title(), 'scale': scale,
                     **describe(source, NAMES, 4),
                     'walk': describe(walk, ['walk-' + str(i) for i in range(8)], 2)}
        manifest['characters'].append(character)
        shutil.copyfile(source, DEST / name / 'actions.png')
        shutil.copyfile(walk, DEST / name / 'gentle-walk.png')
    payload = (json.dumps(manifest, indent=2) + '\n').encode()
    (SOURCE / 'animation-contract.json').write_bytes(payload)
    (DEST / 'animation-contract.json').write_bytes(payload)
    print('Prepared four unchanged sheets and 48 editable frame rectangles/pivots.')


if __name__ == '__main__':
    main()
