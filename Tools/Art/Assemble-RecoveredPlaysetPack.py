"""Assemble checked HQ sprites, rig strips, pictures and PCM audio locally.

Recovered media remains ignored. A new destination and explicit source closure
avoid silently replacing an earlier pack or importing unrelated media.
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
    p=argparse.ArgumentParser(description=__doc__)
    for key in ['base','pictures','export','inspection','output']:p.add_argument('--'+key,type=Path,required=True)
    a=p.parse_args();output=a.output.resolve();project=a.export.resolve()
    if output.exists():p.error('Choose a new immutable output directory')
    manifest=read(a.base/'actor-frames.json');inputs=read(a.inspection/'input-hashes.json')
    assert not manifest['missingActions'],'Resolve missing requested actions first'
    assert len(manifest['actions'])>=448 and len(manifest['props'])>=12,'Unexpected HQ rig/prop scope'
    for relative,expected in inputs.items():assert sha(project/relative)==expected,'Recovered input changed: '+relative
    shutil.copytree(a.base,output);pictures=[];audio=[]
    for row in read(a.pictures/'pictures.json')['pictures']:
        assert sha(project/row['source'])==row['sourceSha256']
        target=output/'Pictures'/row['file'];target.parent.mkdir(exist_ok=True);shutil.copyfile(a.pictures/row['file'],target)
        pictures.append(dict(key=row['key'],file=target.relative_to(output).as_posix(),source=row['source'],sourceSha256=row['sourceSha256']))
    dependencies=read(a.inspection/'dependencies.json')
    for row in dependencies:
        if row['from_id']=='10270' and row['from_path']==manifest['sourceScene'] and row['field'].startswith('textures.'):
            relative=row['target'][0];index=int(row['field'].split('.')[1]);target=output/'Pictures'/('puzzle-'+str(index)+Path(relative).suffix);shutil.copyfile(project/relative,target)
            pictures.append(dict(key='puzzle-'+str(index),file=target.relative_to(output).as_posix(),source=relative,sourceSha256=sha(project/relative)))
    assert len(pictures)==34 and len({r['key'] for r in pictures})==34
    names={'music':'mus_BG_SpideyHQ.ogg','camera':'sfx_Ldm_camera_click.ogg','globe':'sfx_Ldm_signal_tap_02.ogg','fall':'sfx_Ldm_slide_down_bump.ogg','slide':'sfx_Ldm_slide_down.ogg','web':'sfx_Ldm_webShoot.ogg','balloon':'sfx_Ldm_balloon_tap_01.ogg','happy':'vo_Ldm_character_celebrate_short_01.ogg','poster':'sfx_Ldm_photo_swish.ogg'}
    for key,name in names.items():
        matches=[relative for relative in inputs if Path(relative).name==name];assert len(matches)==1,(key,matches)
        source=project/matches[0];target=output/'Audio'/(key+'.wav');target.parent.mkdir(exist_ok=True)
        subprocess.run(['ffmpeg','-v','error','-nostdin','-i',str(source),'-ar','22050','-ac','2','-c:a','pcm_s16le',str(target)],check=True)
        with wave.open(str(target)) as sound:duration=sound.getnframes()/sound.getframerate()
        audio.append(dict(key=key,file=target.relative_to(output).as_posix(),source=matches[0],sourceSha256=sha(source),duration=duration))
    content=dict(format=1,pictures=pictures,audio=audio,limits=['Globe toy-tap feedback is an explicit family presentation adaptation using a recovered common effect.','Visual and subjective audio acceptance remain separate from byte/format validation.'])
    (output/'hq-content.json').write_text(json.dumps(content,indent=2),encoding='utf-8')
    receipt=[dict(path=file.relative_to(output).as_posix(),sha256=sha(file),bytes=file.stat().st_size) for file in sorted(output.rglob('*')) if file.is_file()]
    (output.parent/(output.name+'-receipt.json')).write_text(json.dumps(receipt,indent=2),encoding='utf-8')
    print(json.dumps(dict(files=len(receipt),bytes=sum(r['bytes'] for r in receipt),pictures=len(pictures),audio=len(audio),actions=len(manifest['actions']))))


if __name__=='__main__':main()
