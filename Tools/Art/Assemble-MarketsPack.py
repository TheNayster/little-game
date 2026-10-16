"""BLU-01: package reviewed Markets source bakes and sound with hash provenance.

Consumer: Worlds/Markets Resources. Retain while shipped media uses these inputs.
This copies asset data only, never recovered executable code or private saves.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil

ROOT=Path(__file__).resolve().parents[2]
def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ('views','prefabs','ponies','export','icecream','pony-props'):parser.add_argument('--'+name,type=Path,required=True)
    args=parser.parse_args()
    destination=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Markets'
    destination.mkdir(parents=True,exist_ok=True)
    receipt={'owner':'BLU-01 Markets','format':1,'purpose':'Recovered data flattened to the existing 2D pipeline; family behavior is handwritten','files':[]}
    def copy(source,relative):
        if not source.is_file():raise FileNotFoundError(source)
        target=destination/relative;target.parent.mkdir(parents=True,exist_ok=True)
        if not target.exists() or digest(target)!=digest(source):shutil.copyfile(source,target)
        source_root=ROOT if source.resolve().is_relative_to(ROOT) else next(parent.parent for parent in source.resolve().parents if parent.name=='LocalData')
        receipt['files'].append({'path':relative,'sha256':digest(target),'bytes':target.stat().st_size,'source':source.resolve().relative_to(source_root).as_posix(),'sourceSha256':digest(source)})
    for name in ('smoothie','flowers','bubbles','ponies','music'):copy(args.views/(name+'.png'),'Art/'+name+'.png')
    # Keep the recovered Scene3 stall/rail rather than substituting flowers.
    copy(args.icecream/'icecream.png','Art/icecream.png')
    copy(args.pony_props/'pony-apple.png','Art/pony-apple.png')
    for source in sorted(args.prefabs.glob('*.png')):
        if 'SubSceneController' not in source.name and 'Background' not in source.name and 'BG_' not in source.name:copy(source,'Art/'+source.name)
    for source in sorted(args.views.glob('pony-*.png')):copy(source,'Art/'+source.name)
    variants=json.loads((args.ponies/'pony-variants.json').read_text(encoding='utf-8'))
    if variants['fullVariants']!=180:raise ValueError('Expected all180 source appearance combinations')
    for row in variants['variants']:
        source=args.ponies/row['file']
        if digest(source)!=row['sha256']:raise ValueError('Pony bake receipt mismatch: '+source.name)
        if row['file'].startswith(('pony-body-','pony-eat-body-')):copy(source,'Art/'+source.name)
    discoveries=json.loads((destination/'discovery-receipt.json').read_text(encoding='utf-8'))
    for row in discoveries['files']:
        suffix=row['source'].split('/ExportedProject/',1)[1]
        copy(args.export/suffix,row['path'])
    audio=args.export/'Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Audio'
    for source in sorted(audio.rglob('*.ogg')):copy(source,'Audio/'+source.name)
    for name in ('one','two','three'):
        for layer in ('one','two','three','four','five'):
            suffix='_sax' if name=='three' and layer=='five' else ''
            if not (destination/f'Audio/mus_mkd_busker_song_{name}_layer_{layer}{suffix}.ogg').is_file():raise ValueError('Missing source music layer')
    (destination/'content-receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'files':len(receipt['files']),'bytes':sum(f['bytes'] for f in receipt['files']),'receipt':str(destination/'content-receipt.json')}))
if __name__=='__main__':main()
