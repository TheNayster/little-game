using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // A single lightweight ribbon follows the shared swash phase. The painted
    // ocean remains behind it; this mesh supplies shallow water and moving foam.
    public sealed class BeachWaterGraphic : MaskableGraphic
    {
        public double clock;public float cameraX,scale=1;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();const int segments=160;
            var water=BeachShore.WaterY(clock);
            float Edge(float x)=>(water*.45f-250)+Mathf.Sin(x*.008f+(float)clock*.65f)*5+Mathf.Sin(x*.022f-(float)clock*.3f)*2;
            Vector2 P(float x,float y)=>new Vector2((x-cameraX)*scale,y*scale);
            for(var i=0;i<segments;i++){
                var x=i*4800f/segments;var nx=(i+1)*4800f/segments;var y=Edge(x);var ny=Edge(nx);
                Quad(vh,P(x,y),P(nx,ny),P(nx,70),P(x,70),new Color(.2f,.77f,.81f,.62f),new Color(.2f,.77f,.81f,0));
                Quad(vh,P(x,y-3),P(nx,ny-3),P(nx,ny+4),P(x,y+4),new Color(1,.99f,.91f,.8f),new Color(1,1,1,.94f));
                Quad(vh,P(x,y+13),P(nx,ny+13),P(nx,ny+15),P(x,y+15),new Color(1,1,1,.32f),new Color(1,1,1,.32f));
            }
        }
        private static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color bottom,Color top)
        {
            var n=vh.currentVertCount;vh.AddVert(a,bottom,Vector2.zero);vh.AddVert(b,bottom,Vector2.zero);vh.AddVert(c,top,Vector2.zero);vh.AddVert(d,top,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
    public sealed class BeachRippleGraphic : MaskableGraphic
    {
        public float age;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();for(var ring=0;ring<3;ring++){
                var radius=12+age*40-ring*12;if(radius<0)continue;
                var alpha=Mathf.Clamp01(1-age/2)*(1-ring*.2f);var color=new Color(1,1,.93f,alpha);
                for(var i=0;i<40;i++){
                    var a=i*Mathf.PI*2/40;var b=(i+1)*Mathf.PI*2/40;var n=vh.currentVertCount;
                    Vector2 P(float t,float r)=>new Vector2(Mathf.Cos(t)*r,Mathf.Sin(t)*r*.38f);
                    vh.AddVert(P(a,radius),color,Vector2.zero);vh.AddVert(P(b,radius),color,Vector2.zero);vh.AddVert(P(b,radius+2),color,Vector2.zero);vh.AddVert(P(a,radius+2),color,Vector2.zero);
                    vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
                }
            }
        }
    }
}
