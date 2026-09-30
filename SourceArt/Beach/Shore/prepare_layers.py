"""Store generated pixels byte-for-byte in a named editable OpenRaster layer."""
from pathlib import Path
from zipfile import ZipFile, ZIP_STORED
from xml.etree.ElementTree import Element, SubElement, tostring
from PIL import Image
root=Path(__file__).resolve().parent
for path in root.glob('*.png'):
    if path.stem.endswith('-poses-v2'):
        continue  # These use the three named layers in prepare_model_sheets.py.
    with Image.open(path) as image:
        w,h=image.size
        if image.mode=='RGBA':
            alpha=image.getchannel('A')
            low,high=alpha.getextrema()
            assert low==0 and high>=250, f'Missing transparency: {path}'
        document=Element('image',{'w':str(w),'h':str(h),'name':path.stem})
        stack=SubElement(document,'stack')
        SubElement(stack,'layer',{'name':'Generated '+path.stem,'src':'data/art.png','x':'0','y':'0','opacity':'1.0','visibility':'visible','composite-op':'svg:src-over'})
        with ZipFile(path.with_suffix('.ora'),'w') as archive:
            archive.writestr('mimetype','image/openraster',compress_type=ZIP_STORED)
            archive.writestr('stack.xml',tostring(document,encoding='utf-8'))
            archive.write(path,'data/art.png');archive.write(path,'mergedimage.png')
        print(path.name,image.size,'alpha bounds',image.getchannel('A').getbbox() if image.mode=='RGBA' else 'opaque')
