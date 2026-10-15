"""Create an isolated asset-only Unity research project without running recovered code.

The destination must be new. Originals, their metadata and any existing probe stay
untouched. Handwritten compatibility readers/tools are supplied separately.
"""
import argparse
import json
from pathlib import Path
import shutil


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--export', type=Path, required=True)
    parser.add_argument('--template', type=Path, required=True)
    parser.add_argument('--destination', type=Path, required=True)
    parser.add_argument('--inspection', type=Path, help='Use the inspector input-hashes closure to avoid unrelated assets.')
    args = parser.parse_args()
    source, template, destination = (p.resolve() for p in (args.export, args.template, args.destination))
    if not (source / 'Assets').is_dir() or not (template / 'ProjectSettings').is_dir():
        parser.error('Export Assets and template ProjectSettings must exist.')
    if destination.exists():
        parser.error('Destination exists; inspect it instead of overwriting a probe.')
    destination.mkdir(parents=True)
    # Third-party executable scripts, DLLs and generated editor tooling are never
    # copied. Native Unity assets retain their original GUIDs and references.
    blocked = {'Scripts', 'Plugins', 'Editor'}
    count = 0
    selected = set(json.loads((args.inspection / 'input-hashes.json').read_text(encoding='utf-8'))) if args.inspection else None
    for path in (source / 'Assets').rglob('*'):
        relative = path.relative_to(source / 'Assets')
        original = path.with_suffix('') if path.suffix == '.meta' else path
        if selected is not None and original.relative_to(source).as_posix() not in selected:
            continue
        if any(part in blocked for part in relative.parts) or path.suffix.lower() in {'.cs', '.dll', '.exe', '.asmdef', '.asmref'}:
            continue
        if path.is_file():
            target = destination / 'Assets' / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(path, target)
            count += 1
    shutil.copytree(template / 'ProjectSettings', destination / 'ProjectSettings')
    (destination / 'Packages').mkdir()
    # This probe loads native assets with our readers, never decompiled assemblies.
    modules = ['audio', 'animation', 'physics', 'imgui', 'ui', 'particlesystem']
    manifest = {'dependencies': {f'com.unity.modules.{name}': '1.0.0' for name in modules}}
    (destination / 'Packages/manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    research = Path(__file__).resolve().parents[1] / 'UnityEditor'
    target = destination / 'Assets/Research'
    target.mkdir(parents=True, exist_ok=True)
    for name in ['LegoBrickMaterial.cs', 'RecoveredBrick.shader', 'RecoveredSprite.shader']:
        for suffix in ['', '.meta']:
            shutil.copy2(research / (name + suffix), target / (name + suffix))
    receipt = {'export': str(source), 'template': str(template), 'destination': str(destination),
               'copied_asset_files': count, 'excluded_directories': sorted(blocked),
               'purpose': 'Isolated asset-only recovery probe; original game logic is not running.'}
    (destination / 'probe-preparation.json').write_text(json.dumps(receipt, indent=2), encoding='utf-8')
    print(json.dumps(receipt))


if __name__ == '__main__':
    main()
