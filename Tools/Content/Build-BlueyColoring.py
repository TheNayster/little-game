"""Import preserved official PDFs into bounded coloring regions. No redraw/retouch.
Run: uv run --with pymupdf --with pillow --with numpy --with scipy python Tools/Content/Build-BlueyColoring.py
The line art is an exact PDF rasterization. Region masks are interaction metadata.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import hashlib,json,struct,urllib.request,uuid
from pathlib import Path
import pymupdf as fitz
import numpy as np
from PIL import Image
from scipy import ndimage
ROOT=PROJECT_ROOT
SRC=ROOT/'SourceArt/Home/Discovery/Coloring'
OUT=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Home/Discovery/Coloring'
OUT.mkdir(parents=True,exist_ok=True)
catalog=json.loads((SRC/'sources.json').read_text())
base_meta=(OUT.parent/'mixing-props.png.meta').read_text()
counts=[]
for item in catalog['pages']:
 pdf=SRC/(item['id']+'.pdf')
 if not pdf.exists():pdf.write_bytes(urllib.request.urlopen(item['url']).read())
 doc=fitz.open(pdf)
 item['pdfPage']=0
 page=doc[item['pdfPage']];scale=1024/max(page.rect.width,page.rect.height)
 pix=page.get_pixmap(matrix=fitz.Matrix(scale,scale),alpha=False)
 rgb=np.frombuffer(pix.samples,np.uint8).reshape(pix.height,pix.width,3)
 # White components have fixed, reproducible IDs; ignore tiny lettering and
 # the outer page margin, never alter the original line art.
 ink=rgb.min(axis=2)<190
 labels,n=ndimage.label(~ink)
 sizes=np.bincount(labels.ravel());edge=set(np.concatenate((labels[0],labels[-1],labels[:,0],labels[:,-1])))
 ids=[i for i in range(1,n+1) if sizes[i]>=180 and i not in edge]
 assert len(ids)<255,(item['id'],len(ids))
 lookup=np.zeros(n+1,np.uint8)
 for k,i in enumerate(ids):lookup[i]=k+1
 mask=lookup[labels]
 stem=OUT/item['id'];Image.fromarray(rgb).save(stem.with_suffix('.png'))
 # Raw byte mask matches PDF pixels top to bottom, RLE not needed: asset
 # bundles compress it and loading never recomputes flood fills.
 stem.with_suffix('.bytes').write_bytes(struct.pack('<HH',pix.width,pix.height)+mask.tobytes())
 thumb=Image.fromarray(rgb);thumb.thumbnail((180,240));thumb.save(OUT/(item['id']+'-thumb.png'))
 for png in [stem.with_suffix('.png'),OUT/(item['id']+'-thumb.png')]:
  meta=png.with_suffix('.png.meta')
  if not meta.exists():
   import re
   value=re.sub(r'guid: \w+', 'guid: '+uuid.uuid4().hex,base_meta,count=1).replace('textureCompression: 1','textureCompression: 0').replace('textureFormat: 50','textureFormat: -1').replace('overridden: 1','overridden: 0')
   if png==stem.with_suffix('.png'):value=value.replace('enableMipMap: 0','enableMipMap: 1').replace('filterMode: 1','filterMode: 2')
   meta.write_text(value)
 item.update(width=pix.width,height=pix.height,regions=len(ids),sha256=hashlib.sha256(pdf.read_bytes()).hexdigest())
 counts.append(len(ids));print(item['name'],pix.width,pix.height,len(ids),flush=True)
(OUT/'catalog.json').write_text(json.dumps(catalog,indent=2))
(SRC/'manifest.json').write_text(json.dumps(catalog,indent=2))
print('COUNTS',counts)
