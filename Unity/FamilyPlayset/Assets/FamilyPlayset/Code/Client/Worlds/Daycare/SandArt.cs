using UnityEngine;

namespace LittleWeeps.Client
{
    // UV registration is kept separate from saved footprints. Source atlas and
    // exact rectangles are retained in SourceArt/Daycare/Sandpit/StageFive.
    internal static class SandArt
    {
        private static Texture2D atlas;
        public static Texture2D Atlas {
            get {
                if(atlas!=null)return atlas;
                var source=WorldResources.Load<Texture2D>("Worlds/Daycare/SandcastleClub/objects");
                if(source==null)return null;
                atlas=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);
                atlas.name="Sandcastle illustration and solid ink";atlas.filterMode=FilterMode.Bilinear;atlas.wrapMode=TextureWrapMode.Clamp;
                atlas.SetPixels32(source.GetPixels32());
                // One tiny white swatch lets UI meshes share the illustrated
                // texture while drawing fill, particles and vector controls.
                for(var x=0;x<4;x++)for(var y=0;y<4;y++)atlas.SetPixel(x,y,Color.white);
                atlas.Apply(false,true);return atlas;
            }
        }
        public static Vector2 Ink=>new Vector2(2f/Atlas.width,2f/Atlas.height);
        private static readonly Rect[] pixels={
            new Rect(43,68,242,239),new Rect(330,78,264,228),new Rect(601,135,295,166),new Rect(900,104,342,205),
            new Rect(29,397,257,234),new Rect(318,388,288,235),new Rect(640,375,258,257),new Rect(969,345,242,303),
            new Rect(24,697,289,243),new Rect(307,677,294,267),new Rect(640,669,275,278),new Rect(936,704,272,246),
            new Rect(60,1035,217,172),new Rect(350,973,240,234),new Rect(646,984,246,225),new Rect(920,1063,306,151)
        };
        public static Rect UV(int index){var p=pixels[index];return new Rect(p.x/Atlas.width,1-(p.y+p.height)/Atlas.height,p.width/Atlas.width,p.height/Atlas.height);}
        public static void Release(){if(atlas!=null)Object.Destroy(atlas);atlas=null;}
    }
}
