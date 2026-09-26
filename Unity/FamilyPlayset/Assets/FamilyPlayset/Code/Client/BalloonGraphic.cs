using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Small vector UI mesh: one lightweight, original balloon, no texture RAM.
    public sealed class BalloonGraphic : MaskableGraphic
    {
        private float sway;
        public void Sway(float value){if(Mathf.Abs(sway-value)<.02f)return;sway=value;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            void Oval(Vector2 c,Vector2 radius,Color tint)
            {
                var first=vh.currentVertCount;vh.AddVert(c,tint,Vector2.zero);
                for(var i=0;i<=48;i++)
                {var a=i*Mathf.PI*2/48;vh.AddVert(c+new Vector2(Mathf.Cos(a)*radius.x,Mathf.Sin(a)*radius.y),tint,Vector2.zero);}
                for(var i=0;i<48;i++)vh.AddTriangle(first,first+i+1,first+i+2);
            }
            void Line(Vector2 a,Vector2 b,float width,Color tint)
            {
                var n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;var start=vh.currentVertCount;
                foreach(var p in new[]{a-n,a+n,b+n,b-n})vh.AddVert(p,tint,Vector2.zero);
                vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
            }
            var ink=new Color(.48f,.15f,.23f);
            for(var i=0;i<8;i++)
            {
                Vector2 Point(int n)=>new Vector2(Mathf.Sin(n*.63f+sway)*n*.6f,-32-n*4);
                Line(Point(i),Point(i+1),1.4f,new Color(.51f,.43f,.39f,.8f));
            }
            Oval(Vector2.zero,new Vector2(27,33),ink);
            Oval(new Vector2(0,1),new Vector2(24.5f,30),new Color(.96f,.28f,.37f));
            Oval(new Vector2(-8,12),new Vector2(7,12),new Color(1,.65f,.65f,.8f));
            var knot=vh.currentVertCount;
            foreach(var p in new[]{new Vector2(0,-29),new Vector2(-5,-37),new Vector2(5,-37)})vh.AddVert(p,ink,Vector2.zero);
            vh.AddTriangle(knot,knot+1,knot+2);
        }
    }
}
