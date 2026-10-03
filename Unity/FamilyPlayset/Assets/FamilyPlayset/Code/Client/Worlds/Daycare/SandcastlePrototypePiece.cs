using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Only the isolated prototype consumes this set. Existing buckets/tools/props remain SandShape.
    public sealed class SandcastlePrototypePiece : MaskableGraphic
    {
        public string shape;
        public int orientation;
        public float reveal=1;
        private Texture2D atlas;
        public override Texture mainTexture=>atlas!=null?atlas:atlas=WorldResources.Load<Texture2D>("Worlds/Daycare/SandcastleClub/Prototype/pieces");
        // Measured pixel rectangles in the retained transparent master (top-left image coordinates).
        private static readonly Rect[] pixels={new Rect(87,82,382,400),new Rect(561,112,425,388),new Rect(1038,228,463,268),new Rect(30,606,522,330),new Rect(590,582,415,364),new Rect(1036,579,479,364)};
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var texture=mainTexture;if(texture==null)return;
            var index=shape=="round"?0:shape=="square"?1:shape=="wall"?orientation==90?4:2:orientation==90?5:3;
            var p=pixels[index];var uv=new Rect(p.x/texture.width,1-(p.y+p.height)/texture.height,p.width/texture.width,p.height/texture.height);
            var vertical=orientation==90;var width=index<2?128:vertical?84:276;var height=index<2?150:vertical?295:shape=="wall"?90:145;var bottom=vertical?-105:-7;
            var h=height*Mathf.Clamp01(reveal);
            var a=new Vector2(-width/2f,bottom);var b=new Vector2(width/2f,bottom+h);
            mesh.AddVert(new Vector3(a.x,a.y),color,new Vector2(uv.xMin,uv.yMin));mesh.AddVert(new Vector3(a.x,b.y),color,new Vector2(uv.xMin,uv.yMin+uv.height*reveal));mesh.AddVert(new Vector3(b.x,b.y),color,new Vector2(uv.xMax,uv.yMin+uv.height*reveal));mesh.AddVert(new Vector3(b.x,a.y),color,new Vector2(uv.xMax,uv.yMin));mesh.AddTriangle(0,1,2);mesh.AddTriangle(2,3,0);
        }
    }
}
