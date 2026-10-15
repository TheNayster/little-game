# /// script
# dependencies = ["Pillow>=10"]
# ///
"""Read-only atlas integrity/cropping inspection; never visual or gameplay acceptance."""
import argparse
import hashlib
import json
import math
import wave
from pathlib import Path
from PIL import Image


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--pack',type=Path,required=True)
    parser.add_argument('--output',type=Path,required=True)
    args=parser.parse_args();root=args.pack.resolve()
    source=root/'actor-frames.json';data=json.loads(source.read_text(encoding='utf-8'))
    errors=[];edges=[];keys=set();receipts=[]
    for row in data['actions']:
        key=(row['actor'],row['action'])
        if key in keys:errors.append({'kind':'duplicate-action','key':key})
        keys.add(key)
        file=(root/row['texture']).resolve()
        if not file.is_relative_to(root):raise ValueError('Atlas path escapes content root')
        with Image.open(file) as image:
            size=row['size'];columns=row['columns'];rows=math.ceil(row['frames']/columns)
            if image.size!=(columns*size,rows*size):errors.append({'kind':'dimensions','key':key,'size':image.size})
            alpha=image.getchannel('A')
            frame_hashes=set()
            for frame in range(row['frames']):
                x=frame%columns*size;y=frame//columns*size
                bounds=alpha.crop((x,y,x+size,y+size)).point(lambda a:255 if a>25 else 0).getbbox()
                frame_hashes.add(hashlib.sha256(image.crop((x,y,x+size,y+size)).tobytes()).hexdigest())
                if bounds is None:errors.append({'kind':'empty-frame','key':key,'frame':frame})
                elif bounds[0]==0 or bounds[1]==0 or bounds[2]==size or bounds[3]==size:edges.append({'key':key,'frame':frame,'bounds':bounds})
            if row['frames']>1 and row['action'].split('-')[0] in ['walk','run','unwrap','eat','drink','jump','serve'] and len(frame_hashes)<2:
                errors.append({'kind':'static-motion-atlas','key':key,'frames':row['frames']})
            for anchor in ['hold','leftHand','rightHand','face']:
                points=row.get(anchor,[])
                if len(points)!=row['frames'] or any(not all(math.isfinite(p[k]) for k in ['x','y']) for p in points):errors.append({'kind':'anchor-track','key':key,'anchor':anchor})
        receipts.append({'file':row['texture'],'sha256':hashlib.sha256(file.read_bytes()).hexdigest()})
    for row in data.get('cakeSprites',[]):
        file=(root/row['texture']).resolve()
        if not file.is_relative_to(root):raise ValueError('Cake path escapes content root')
        with Image.open(file) as image:
            if image.size!=(512,512) or image.getchannel('A').getbbox() is None:errors.append({'kind':'invalid-cake-sprite','name':row['name']})
        receipts.append({'file':row['texture'],'sha256':hashlib.sha256(file.read_bytes()).hexdigest()})
    prop_keys=set()
    for row in data.get('audio',[]):
        file=(root/row['file']).resolve()
        if not file.is_relative_to(root):raise ValueError('Audio path escapes content root')
        with wave.open(str(file),'rb') as pcm:
            if pcm.getsampwidth()!=2 or pcm.getnchannels() not in [1,2] or abs(pcm.getnframes()/pcm.getframerate()-row['duration'])>.001:errors.append({'kind':'invalid-audio','key':row['key']})
        digest=hashlib.sha256(file.read_bytes()).hexdigest()
        if row.get('sha256') and digest!=row['sha256']:errors.append({'kind':'audio-digest','key':row['key']})
        receipts.append({'file':row['file'],'sha256':digest})
    floor=data.get('danceFloor')
    if floor:
        if len(floor['cells'])!=36 or len(floor['colors'])!=6:errors.append({'kind':'floor-inventory'})
        with Image.open(root/floor['texture']) as image:
            if image.getchannel('A').getbbox() is None:errors.append({'kind':'empty-floor-atlas'})
        for cell in floor['cells']:
            uv=cell['uv']
            if uv['x']<0 or uv['y']<0 or uv['width']<=0 or uv['height']<=0 or uv['x']+uv['width']>1.00001 or uv['y']+uv['height']>1.00001:errors.append({'kind':'floor-uv','id':cell['sourceRendererId']})
        receipts.append({'file':floor['texture'],'sha256':hashlib.sha256((root/floor['texture']).read_bytes()).hexdigest()})
    castle=data.get('castle')
    if castle:
        width,height=int(castle['pixels']['width']),int(castle['pixels']['height'])
        with Image.open(root/castle['texture']) as image:
            if image.size!=(width*5,height):errors.append({'kind':'castle-atlas-dimensions'})
            frames={hashlib.sha256(image.crop((i*width,0,(i+1)*width,height)).tobytes()).hexdigest() for i in range(5)}
            if len(frames)!=5:errors.append({'kind':'static-castle-deformation','distinct':len(frames)})
        with Image.open(root/castle['front']) as image:
            if image.size!=(width,height) or image.getchannel('A').getbbox() is None:errors.append({'kind':'invalid-castle-arch'})
        for name in [castle['texture'],castle['front']]:receipts.append({'file':name,'sha256':hashlib.sha256((root/name).read_bytes()).hexdigest()})
    for row in data['props']:
        key=(row['name'],row.get('view','front'))
        if key in prop_keys:errors.append({'kind':'duplicate-prop','key':key})
        prop_keys.add(key)
        file=(root/row['texture']).resolve()
        if not file.is_relative_to(root):raise ValueError('Prop path escapes content root')
        with Image.open(file) as image:
            if image.size!=(256,256) or image.getchannel('A').getbbox() is None:errors.append({'kind':'invalid-prop','key':key})
        receipts.append({'file':row['texture'],'sha256':hashlib.sha256(file.read_bytes()).hexdigest()})
    report={'passed':not errors,'manifest_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),
        'actor_action_sets':len(keys),'source_actors':len({k[0] for k in keys}),
        'prop_views':len(prop_keys),'source_props':len({k[0] for k in prop_keys}),'cake_sprites':len(data.get('cakeSprites',[])),
        'errors':errors,'opaque_frame_edge_contacts':edges,'files':receipts,
        'limits':['Edge contacts require actual visual disposition; no image was modified.',
                  'PNG/anchor integrity does not establish correct motion, attachments or gameplay.']}
    args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k not in ['files','opaque_frame_edge_contacts']}|{'edge_contact_frames':len(edges)},indent=2))
    return 0 if report['passed'] else 1


if __name__=='__main__':
    raise SystemExit(main())
