using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Approved P9 artwork shared by the production activity and isolated development preview.
    // Existing buckets/tools/props remain SandShape; this owns no gameplay state.
    public sealed class SandcastlePrototypePiece : MaskableGraphic
    {
        public string shape;
        public int orientation;
        public float reveal=1;
        private Texture2D atlas;
        private static readonly Rect carvedWindow=new Rect(-14,34,31,68);
        public override Texture mainTexture=>atlas!=null?atlas:atlas=WorldResources.Load<Texture2D>("Worlds/Daycare/SandcastleClub/Prototype/pieces");
        // Measured pixel rectangles in the retained transparent master (top-left image coordinates).
        private static readonly Rect[] pixels={new Rect(77,59,386,420),new Rect(562,69,410,417),new Rect(1045,220,444,260),new Rect(46,603,466,321),new Rect(702,526,154,419),new Rect(1120,526,162,419)};
        public static Rect Bounds(string shape,int orientation)
        {
            var tower=!LittleWeeps.Core.DaycareSandpit.LongShape(shape);var vertical=orientation==90;
            var width=tower?140:vertical?115:280;var height=tower?210:vertical?310:shape=="wall"?105:160;var bottom=tower?-17:vertical?-82:-12;
            return new Rect(-width/2f,bottom,width,height);
        }
        public bool PaintedHit(Vector2 point)
        {
            var b=Bounds(shape,orientation);if(!b.Contains(point))return false;
            if(shape=="round" && carvedWindow.Contains(point))return true;
            var index=shape=="round"?0:shape=="square"?1:shape=="wall"?orientation==90?4:2:orientation==90?5:3;
            var p=pixels[index];var tex=(Texture2D)mainTexture;
            // Open arches and transparent gutters let a visible piece behind receive the tap.
            return tex.GetPixelBilinear((p.x+(point.x-b.xMin)/b.width*p.width)/tex.width,1-(p.y+(1-(point.y-b.yMin)/b.height)*p.height)/tex.height).a>.3f;
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var texture=mainTexture;if(texture==null)return;
            var index=shape=="round"?0:shape=="square"?1:shape=="wall"?orientation==90?4:2:orientation==90?5:3;
            var p=pixels[index];var uv=new Rect(p.x/texture.width,1-(p.y+p.height)/texture.height,p.width/texture.width,p.height/texture.height);
            var bounds=Bounds(shape,orientation);var width=bounds.width;var height=bounds.height;var bottom=bounds.yMin;
            var h=height*Mathf.Clamp01(reveal);
            var a=new Vector2(-width/2f,bottom);var b=new Vector2(width/2f,bottom+h);
            if(index==0){
                // The generated slit has alpha; give the carved recess its own opaque sand shade.
                // It remains part of this selectable piece, unlike the deliberately open gate arch.
                var max=Mathf.Min(carvedWindow.yMax,b.y);if(max>carvedWindow.yMin){
                    var ink=new Vector2(320f/texture.width,1-160f/texture.height);var shade=new Color(.64f,.42f,.16f,color.a);
                    mesh.AddVert(new Vector3(carvedWindow.xMin,carvedWindow.yMin),shade,ink);mesh.AddVert(new Vector3(carvedWindow.xMin,max),shade,ink);mesh.AddVert(new Vector3(carvedWindow.xMax,max),shade,ink);mesh.AddVert(new Vector3(carvedWindow.xMax,carvedWindow.yMin),shade,ink);mesh.AddTriangle(0,1,2);mesh.AddTriangle(2,3,0);
                }
            }
            var first=mesh.currentVertCount;
            mesh.AddVert(new Vector3(a.x,a.y),color,new Vector2(uv.xMin,uv.yMin));mesh.AddVert(new Vector3(a.x,b.y),color,new Vector2(uv.xMin,uv.yMin+uv.height*reveal));mesh.AddVert(new Vector3(b.x,b.y),color,new Vector2(uv.xMax,uv.yMin+uv.height*reveal));mesh.AddVert(new Vector3(b.x,a.y),color,new Vector2(uv.xMax,uv.yMin));mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first+2,first+3,first);
        }
    }
}
