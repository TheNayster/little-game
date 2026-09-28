using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Small resolution-independent pictures keep the translucent card separate
    // from its opaque symbol and label. No extra textures or media are loaded.
    public sealed class ReaderPictureControl : MaskableGraphic
    {
        public string Icon = "card";
        private static readonly Color Ink = new Color(.19f,.40f,.40f);
        private Vector3 V(Vector2 p) { var r=rectTransform.rect;return new Vector3(r.xMin+p.x*r.width/100,r.yMax-p.y*r.height/100); }
        private void Poly(VertexHelper h,Color c,params Vector2[] p)
        {var start=h.currentVertCount;foreach(var v in p)h.AddVert(V(v),c,Vector2.zero);for(var i=1;i<p.Length-1;i++)h.AddTriangle(start,start+i,start+i+1);}
        private void Line(VertexHelper h,float x,float y,float xx,float yy,float width,Color c)
        {var a=new Vector2(x,y);var b=new Vector2(xx,yy);var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width/2;Poly(h,c,a+n,b+n,b-n,a-n);}
        private void Round(VertexHelper h,float x,float y,float w,float height,float radius,Color c)
        {
            // Account for aspect ratio so wide cards have circular corners.
            var rx=radius*rectTransform.rect.height/Mathf.Max(1,rectTransform.rect.width);rx=Mathf.Min(rx,w/2);
            var p=new Vector2[28];for(var corner=0;corner<4;corner++)for(var i=0;i<7;i++){
                var a=(corner*90+i*15)*Mathf.Deg2Rad;var cx=corner==0 || corner==3?x+w-rx:x+rx;var cy=corner<2?y+height-radius:y+radius;
                p[corner*7+i]=new Vector2(cx+Mathf.Cos(a)*rx,cy+Mathf.Sin(a)*radius);
            }Poly(h,c,p);
        }
        private void Arc(VertexHelper h,float cx,float cy,float r,float start,float end,float width,Color c)
        {for(var i=0;i<24;i++){var a=Mathf.Lerp(start,end,i/24f)*Mathf.Deg2Rad;var b=Mathf.Lerp(start,end,(i+1)/24f)*Mathf.Deg2Rad;Line(h,cx+Mathf.Cos(a)*r,cy+Mathf.Sin(a)*r,cx+Mathf.Cos(b)*r,cy+Mathf.Sin(b)*r,width,c);}}
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var gold=new Color(.91f,.72f,.36f);var paper=new Color(1,.97f,.81f);
            if(Icon=="card"){Round(h,0,0,100,100,19,color);return;}
            if(Icon=="play" || Icon=="pause" || Icon=="cancel"){
                Arc(h,50,50,38,0,360,6,Ink);
                if(Icon=="play")Poly(h,Ink,new Vector2(41,27),new Vector2(70,50),new Vector2(41,73));
                else if(Icon=="pause"){Round(h,34,29,12,42,3,Ink);Round(h,56,29,12,42,3,Ink);}
                else {Line(h,35,35,65,65,7,Ink);Line(h,65,35,35,65,7,Ink);}return;
            }
            if(Icon=="back" || Icon=="next"){var direction=Icon=="back"?-1:1;Line(h,50-direction*12,22,50+direction*12,50,9,Ink);Line(h,50+direction*12,50,50-direction*12,78,9,Ink);return;}
            if(Icon=="replay"){Arc(h,50,51,31,-125,185,7,Ink);Poly(h,Ink,new Vector2(17,10),new Vector2(44,28),new Vector2(17,34));return;}
            if(Icon=="sound" || Icon=="voice"){
                Poly(h,gold,new Vector2(13,40),new Vector2(33,40),new Vector2(53,23),new Vector2(53,78),new Vector2(33,60),new Vector2(13,60));
                Line(h,53,23,53,78,5,Ink);Arc(h,48,50,25,-48,48,5,Ink);Arc(h,48,50,39,-48,48,5,Ink);return;
            }
            if(Icon=="home"){
                Round(h,23,39,55,46,5,gold);Poly(h,Ink,new Vector2(12,43),new Vector2(50,12),new Vector2(88,43));Round(h,43,56,16,29,2,Ink);return;
            }
            if(Icon=="more"){
                Round(h,13,31,74,51,9,gold);Line(h,34,31,34,17,5,Ink);Line(h,34,17,65,17,5,Ink);Line(h,65,17,65,31,5,Ink);Line(h,16,45,84,45,5,Ink);Round(h,44,38,13,21,3,Ink);return;
            }
            // Open-book picture is also used for words and automatic reading.
            Poly(h,paper,new Vector2(9,17),new Vector2(46,22),new Vector2(50,29),new Vector2(54,22),new Vector2(91,17),new Vector2(91,82),new Vector2(54,86),new Vector2(50,91),new Vector2(46,86),new Vector2(9,82));
            Line(h,9,17,9,82,4,Ink);Line(h,91,17,91,82,4,Ink);Line(h,50,29,50,91,4,Ink);
            Line(h,9,17,46,22,4,Ink);Line(h,54,22,91,17,4,Ink);Line(h,9,82,46,86,4,Ink);Line(h,54,86,91,82,4,Ink);
            for(var i=0;i<3;i++){Line(h,20,36+i*15,38,38+i*15,3,Ink);Line(h,63,38+i*15,80,36+i*15,3,Ink);}
        }
    }
}
