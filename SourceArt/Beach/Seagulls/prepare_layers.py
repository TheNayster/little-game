"""Preserve the generated atlas pixels in four named editable pose layers."""
from io import BytesIO
from pathlib import Path
from zipfile import ZipFile, ZIP_STORED
from xml.etree.ElementTree import Element, SubElement, tostring
from PIL import Image
root=Path(__file__).resolve().parent
image=Image.open(root/'seagull-poses.png')
document=Element('image',{'w':str(image.width),'h':str(image.height),'name':'Silver gull poses'})
stack=SubElement(document,'stack')
with ZipFile(root/'seagull-poses.ora','w') as archive:
    archive.writestr('mimetype','image/openraster',compress_type=ZIP_STORED)
    for i,name in enumerate(('Resting','Wings up','Wings down','Glide')):
        x=(i%2)*768;y=(i//2)*512;layer=image.crop((x,y,x+768,y+512))
        pixels=BytesIO();layer.save(pixels,format='PNG');src=f'data/pose-{i}.png'
        archive.writestr(src,pixels.getvalue())
        SubElement(stack,'layer',{'name':name,'src':src,'x':str(x),'y':str(y),'opacity':'1.0','visibility':'visible','composite-op':'svg:src-over'})
    archive.writestr('stack.xml',tostring(document,encoding='utf-8'))
    archive.write(root/'seagull-poses.png','mergedimage.png')
print('Preserved four editable pose layers; generated atlas unchanged.')
