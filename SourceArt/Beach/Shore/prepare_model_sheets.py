"""Measure generated sprite cells and preserve their pixels as editable layers."""
from pathlib import Path
from zipfile import ZipFile, ZIP_STORED
from io import BytesIO
from xml.etree.ElementTree import Element, SubElement, tostring
import json, shutil
from PIL import Image
root=Path(__file__).resolve().parent
runtime=root.parents[2]/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Beach/Art'
names=('whale','dolphin','mermaid');poses=('Emerging','Airborne','Re-entering')
records=[]
for name in names:
    path=root/(name+'-poses-v2.png')
    with Image.open(path) as image:
        w,h=image.size
        assert image.mode=='RGBA',f'No alpha channel: {name}'
        low,high=image.getchannel('A').getextrema()
        assert low==0 and high>=250,f'No transparent cutout: {name}'
        frames=[]
        # The mermaid's first tail extends a few pixels below the requested
        # midpoint. Its clear gap is at row 532; retain every original pixel.
        divider=round(h*532/1024) if name=='mermaid' else h//2
        document=Element('image',{'w':str(w),'h':str(h),'name':name+' water jump poses'})
        stack=SubElement(document,'stack')
        with ZipFile(path.with_suffix('.ora'),'w') as archive:
            archive.writestr('mimetype','image/openraster',compress_type=ZIP_STORED)
            for i,pose in enumerate(poses):
                x0=(i%2)*w//2;x1=((i%2)+1)*w//2;y0=0 if i<2 else divider;y1=divider if i<2 else h
                # Cell extraction stores existing pixels; it never redraws or
                # changes the generated appearance, colors or alpha.
                cell=image.crop((x0,y0,x1,y1));alpha=cell.getchannel('A')
                bounds=alpha.point(lambda a:255 if a>=16 else 0).getbbox()
                assert bounds is not None,f'Empty pose: {name}/{pose}'
                left,top,right,bottom=bounds
                assert left>2 and right<cell.width-2 and top>2 and bottom<cell.height-2,f'Pose crosses cell edge: {name}/{pose}'
                frames.append(dict(x=x0+left,y=y0+top,width=right-left,height=bottom-top))
                src=f'data/pose-{i}.png';pixels=BytesIO();cell.save(pixels,format='PNG')
                archive.writestr(src,pixels.getvalue())
                SubElement(stack,'layer',{'name':pose,'src':src,'x':str(x0),'y':str(y0),'opacity':'1.0','visibility':'visible','composite-op':'svg:src-over'})
            archive.writestr('stack.xml',tostring(document,encoding='utf-8'))
            archive.write(path,'mergedimage.png')
        records.append(dict(name=name,width=w,height=h,frames=frames))
        shutil.copyfile(path,runtime/path.name)
        print(name,image.size,frames)
manifest=dict(sheets=records)
(root/'frames-v2.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
(runtime/'sea-visitor-frames-v2.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Preserved nine original pose drawings in three named editable sheets.')
