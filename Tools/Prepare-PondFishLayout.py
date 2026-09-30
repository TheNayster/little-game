# /// script
# dependencies = ["pillow"]
# ///
"""Read alpha bounds in the generated atlas; save sprite rectangles, no pixel edits."""
from pathlib import Path
from PIL import Image
import json
root=Path(__file__).resolve().parents[1]
path=root/'SourceArt/Home/Pond/fish.png'
im=Image.open(path).convert('RGBA');width,height=im.size;frames=[]
for i in range(8):
    col,row=i%4,i//4;left=col*width//4;top=row*height//2;right=(col+1)*width//4;bottom=(row+1)*height//2
    alpha=im.getchannel('A').crop((left,top,right,bottom)).point(lambda a:255 if a>=64 else 0)
    x0,y0,x1,y1=alpha.getbbox();x0=max(0,x0-3);y0=max(0,y0-3);x1=min(right-left,x1+3);y1=min(bottom-top,y1+3)
    frames.append(dict(x=(left+x0)/width,y=(height-top-y1)/height,width=(x1-x0)/width,height=(y1-y0)/height))
content=json.dumps(dict(frames=frames),indent=2)
for folder in (path.parent,root/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/PondArt'):(folder/'fish-layout.json').write_text(content,encoding='utf-8')
print('Measured eight fish sprite bounds from',width,height,'pixels.')
