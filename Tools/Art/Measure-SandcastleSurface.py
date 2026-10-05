"""Regenerate Sandcastle's bounded alpha support map from retained P9 artwork.
DAY-01/LEARN-01; requires Pillow. Run from project root. Does not modify art.
"""
from pathlib import Path
from PIL import Image
import base64,hashlib
p=Path('Unity/FamilyPlayset/Assets/FamilyPlayset');im=Image.open(p/'Resources/Worlds/Daycare/SandcastleClub/Prototype/pieces.png')
rects=[(77,59,386,420,-70,-17,140,210),(562,69,410,417,-70,-17,140,210),(1045,220,444,260,-140,-12,280,105),(46,603,466,321,-140,-12,280,160),(702,526,154,419,-57.5,-82,115,310),(1120,526,162,419,-57.5,-82,115,310)]
arrays=[]
for shape,(px,py,pw,ph,bx,by,bw,bh) in enumerate(rects):
 bits=bytearray((151*161+7)//8)
 for row in range(161):
  y=-90+row*2
  for col in range(151):
   x=-150+col*2
   if bx<=x<bx+bw and by<=y<by+bh:
    a=im.getpixel((int(px+(x-bx)/bw*pw),int(py+(1-(y-by)/bh)*ph)))[3]
    if shape==0 and -14<=x<17 and 34<=y<102:a=255
    if a>77:
     i=row*151+col;bits[i//8]|=1<<(i%8)
 arrays.append(base64.b64encode(bits).decode())
text='''using System;

namespace LittleWeeps.Core
{
    // Measured alpha support from the unchanged P9 pieces.png, SHA256 '''+hashlib.sha256((p/'Resources/Worlds/Daycare/SandcastleClub/Prototype/pieces.png').read_bytes()).hexdigest()+'''.
    // Two-local-unit grid; the round carved recess is opaque exactly as in the
    // retained renderer. This data validates support, never chooses fixed slots.
    internal static class SandDecorPaint
    {
        private const int Columns=151,Rows=161;
        private static readonly byte[][] masks={
'''+',\n'.join('            Convert.FromBase64String("'+a+'")' for a in arrays)+'''
        };
        private static bool Paint(SandMould m,float x,float y)
        {
            var col=(int)Math.Round((x+150)/2);var row=(int)Math.Round((y+90)/2);
            if(col<0 || col>=Columns || row<0 || row>=Rows)return false;
            var shape=m.shape=="round"?0:m.shape=="square"?1:m.shape=="wall"?(m.orientation==0?2:4):(m.orientation==0?3:5);
            var i=row*Columns+col;return (masks[shape][i/8] & (1<<(i%8)))!=0;
        }
        public static bool Fits(SandMould m,string kind,float x,float y)
        {
            if(kind=="flag")return Paint(m,x-2,y+2) && Paint(m,x+2,y+2);
            var left=kind=="shell"?-24:kind=="door"?-13:-16;var right=kind=="shell"?24:kind=="door"?13:16;
            var bottom=kind=="shell"?-13:kind=="door"?-20:kind=="pebble"?-8:-14;
            var top=kind=="shell"?25:kind=="door"?25:16;
            foreach(var dx in new[]{left,0,right})foreach(var dy in new[]{bottom,0,top})if(!Paint(m,x+dx,y+dy))return false;
            return true;
        }
    }
}
'''
(p/'Code/Core/Worlds/Daycare/SandDecorPaint.cs').write_text(text,encoding='utf8')

print('Measured six P9 silhouettes; source support data written, artwork unchanged.')
