"""Assemble an immutable local research pack; never commit recovered content.

Owner: Hulk world recovery. Consumed by BirthdayArt and the focused native test.
Retire when this research pack is replaced by the game's final content packaging.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import wave


def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ['base','output']:parser.add_argument('--'+name,type=Path,required=True)
    for name in ['cakes','outcomes','audio-project']:parser.add_argument('--'+name,type=Path)
    args=parser.parse_args()
    base,output=args.base.resolve(),args.output.resolve()
    if bool(args.cakes)!=bool(args.outcomes):parser.error('--cakes and --outcomes must be supplied together')
    if output.exists():raise ValueError('Preserve existing packs: output must be a new directory')
    manifest=read(base/'actor-frames.json')
    extra=read(args.outcomes/'outcome-props.json') if args.outcomes else {'props':[]}
    cake=read(args.cakes/'cake-sprites.json') if args.cakes else {'sprites':[]}
    keys={(p['name'],p['view']) for p in manifest['props']}
    for prop in extra['props']:
        if (prop['name'],prop['view']) in keys:raise ValueError('Duplicate prop view')
        keys.add((prop['name'],prop['view']))
    shutil.copytree(base,output)
    for prop in extra['props']:
        relative=Path(prop['texture'])
        if relative.is_absolute() or '..' in relative.parts:raise ValueError('Invalid prop path')
        source=args.outcomes/relative;target=output/relative
        target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(source,target)
        manifest['props'].append(prop)
    # Select actual decoration sprites, excluding editor controls and screenshots.
    selected=[]
    for sprite in cake['sprites']:
        name=sprite['name']
        if not any(part in name for part in ['cake_plate_','candle lit','_cream_','_lolly_','_sprinkles','_star_']):continue
        source=args.cakes/sprite['texture'];relative='Cake/'+sprite['texture']
        target=output/relative;target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(source,target)
        selected.append(dict(sprite,texture=relative,category='base' if 'cake_plate_' in name else 'sticker'))
    if args.cakes:
        if sum(s['category']=='base' for s in selected)!=3:raise ValueError('Three source cake bases required')
        manifest['cakeSprites']=selected
    audio=[]
    if args.audio_project:
        music=args.audio_project/'Assets/DuploMarvel/AddressableAssets/Remote/Audio/V20.0/Music'
        sources={'guitar':'mus_Ldm_hbp_music_room_miles_guitar_correct.ogg','keyboard':'mus_Ldm_hbp_music_room_spidey_keyboard_correct.ogg',
                 'drums':'mus_Ldm_hpp_music_room_gwen_drums_correct.ogg','electric':'mus_Ldm_hbp_happy_birthday_electric.wav',
                 'acoustic':'mus_Ldm_hbp_happy_birthday_acoustic.wav','intro':'mus_Ldm_hbp_intro_sting.wav','main':'mus_Ldm_hbp_main_loop.wav'}
        for key,name in sources.items():
            source=music/name;target=output/'Audio'/f'{key}.wav';target.parent.mkdir(exist_ok=True)
            subprocess.run(['ffmpeg','-v','error','-nostdin','-i',str(source),'-c:a','pcm_s16le',str(target)],check=True)
            with wave.open(str(target),'rb') as pcm:
                if pcm.getsampwidth()!=2 or pcm.getnchannels()!=2 or pcm.getframerate()!=22050:raise ValueError('Unexpected source audio format')
                duration=pcm.getnframes()/pcm.getframerate()
            audio.append({'key':key,'file':'Audio/'+key+'.wav','duration':duration,'source':source.relative_to(args.audio_project).as_posix(),
                          'source_sha256':sha(source),'sha256':sha(target)})
        manifest['audio']=audio
    (output/'actor-frames.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    receipt={'base_manifest_sha256':sha(base/'actor-frames.json'),'cake_manifest_sha256':sha(args.cakes/'cake-sprites.json') if args.cakes else None,
             'outcome_manifest_sha256':sha(args.outcomes/'outcome-props.json') if args.outcomes else None,'assembler_sha256':sha(Path(__file__)),
             'manifest_sha256':sha(output/'actor-frames.json')}
    (output/'assembly-receipt.json').write_text(json.dumps(receipt,indent=2),encoding='utf-8')
    print(json.dumps({'props':len(manifest['props']),'cake_sprites':len(manifest.get('cakeSprites',[])),'audio':len(audio),'manifest_sha256':receipt['manifest_sha256']}))


if __name__=='__main__':main()
