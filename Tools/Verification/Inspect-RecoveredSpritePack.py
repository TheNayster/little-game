# /// script
# dependencies = ["Pillow>=10"]
# ///
"""Read-only atlas integrity/cropping inspection; never visual or gameplay acceptance."""
import argparse
import hashlib
import json
import math
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
            for frame in range(row['frames']):
                x=frame%columns*size;y=frame//columns*size
                bounds=alpha.crop((x,y,x+size,y+size)).point(lambda a:255 if a>25 else 0).getbbox()
                if bounds is None:errors.append({'kind':'empty-frame','key':key,'frame':frame})
                elif bounds[0]==0 or bounds[1]==0 or bounds[2]==size or bounds[3]==size:edges.append({'key':key,'frame':frame,'bounds':bounds})
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
