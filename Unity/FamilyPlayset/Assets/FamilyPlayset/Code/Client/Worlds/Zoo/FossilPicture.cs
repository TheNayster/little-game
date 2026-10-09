using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    // Editable pretend-picture geometry, mirrored in SourceArt/Zoo/Playable/fossil-picture.svg.
    // These three chunky sections are a toy picture, not an anatomical skeleton.
    public sealed class FossilPicture : MaskableGraphic
    {
        public int piece;
        public bool assembled;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var scale=assembled?rectTransform.rect.width/360:rectTransform.rect.width/160;
            var center=assembled?Vector2.zero:new Vector2(piece==0?-130:piece==1?-10:115,piece==0?70:piece==1?60:115);
            Vector2 P(Vector2 p)=>(p-center)*scale;
            void Tri(Vector2 a,Vector2 b,Vector2 c,Color tint){var start=vh.currentVertCount;vh.AddVert(P(a),tint,Vector2.zero);vh.AddVert(P(b),tint,Vector2.zero);vh.AddVert(P(c),tint,Vector2.zero);vh.AddTriangle(start,start+1,start+2);}
            void Box(float x,float y,float w,float h,Color tint){var a=new Vector2(x-w/2,y-h/2);var b=new Vector2(x+w/2,y-h/2);var c=new Vector2(x+w/2,y+h/2);var d=new Vector2(x-w/2,y+h/2);Tri(a,b,c,tint);Tri(a,c,d,tint);}
            void Oval(float x,float y,float w,float h,Color tint){var c=new Vector2(x,y);for(var i=0;i<32;i++){var a=i*Mathf.PI*2/32;var b=(i+1)*Mathf.PI*2/32;Tri(c,c+new Vector2(Mathf.Cos(a)*w/2,Mathf.Sin(a)*h/2),c+new Vector2(Mathf.Cos(b)*w/2,Mathf.Sin(b)*h/2),tint);}}
            var edge=new Color(.31f,.38f,.32f,color.a);var fill=color;
            if(piece==0){Tri(new Vector2(-180,74),new Vector2(-80,95),new Vector2(-80,48),edge);Tri(new Vector2(-167,74),new Vector2(-86,88),new Vector2(-86,56),fill);}
            if(piece==1){Box(-65,28,35,55,edge);Box(38,28,35,55,edge);Oval(-10,74,184,94,edge);Box(-65,30,23,47,fill);Box(38,30,23,47,fill);Oval(-10,74,172,82,fill);}
            if(piece==2){Box(88,100,43,97,edge);Oval(119,155,88,46,edge);Box(88,102,31,95,fill);Oval(119,155,76,34,fill);Oval(140,160,6,6,edge);}
        }
    }
}
