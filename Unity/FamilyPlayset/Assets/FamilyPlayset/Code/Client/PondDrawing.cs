using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    // Lightweight meshes animate the water and lines around the painted fish.
    public sealed class PondDrawing : MaskableGraphic, IPointerClickHandler
    {
        public string kind="water";
        public Vector2 end;
        public float phase;
        public Action<Vector2> clicked;
        private readonly Color ink=new Color(.15f,.29f,.34f);
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();var scale=new Vector2(rectTransform.rect.width/200,rectTransform.rect.height/120);
            void Polygon(Color c,params Vector2[] p){var start=v.currentVertCount;foreach(var pt in p)v.AddVert(Vector2.Scale(pt,scale),c*color,Vector2.zero);for(var i=1;i<p.Length-1;i++)v.AddTriangle(start,start+i,start+i+1);}
            void Ellipse(Vector2 center,Vector2 radius,Color c){var start=v.currentVertCount;v.AddVert(Vector2.Scale(center,scale),c*color,Vector2.zero);for(var i=0;i<=32;i++){var a=i*Mathf.PI*2/32;v.AddVert(Vector2.Scale(center+new Vector2(Mathf.Cos(a)*radius.x,Mathf.Sin(a)*radius.y),scale),c*color,Vector2.zero);if(i>0)v.AddTriangle(start,start+i,start+i+1);}}
            void Line(Vector2 a,Vector2 b,float width,Color c){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Polygon(c,a+n,b+n,b-n,a-n);}
            void Arc(Vector2 center,float rx,float ry,float start,float finish,float width,Color c){var prev=center+new Vector2(Mathf.Cos(start)*rx,Mathf.Sin(start)*ry);for(var i=1;i<=28;i++){var a=Mathf.Lerp(start,finish,i/28f);var pt=center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);Line(prev,pt,width,c);prev=pt;}}
            if(kind=="fishing-icon"){
                Arc(new Vector2(16,-29),65,14,0,Mathf.PI*2,3,new Color(.28f,.62f,.72f));
                Line(new Vector2(-70,-8),new Vector2(-36,43),8,ink);Line(new Vector2(-68,-6),new Vector2(-35,43),4,new Color(.8f,.55f,.27f));
                Arc(new Vector2(-17,23),22,20,.15f,2.8f,2,ink);Line(new Vector2(5,25),new Vector2(5,-9),2,ink);Ellipse(new Vector2(5,-9),new Vector2(6,9),new Color(.98f,.4f,.3f));return;
            }
            if(kind=="feeding-icon"){
                Arc(new Vector2(4,-30),76,16,0,Mathf.PI*2,3,new Color(.28f,.62f,.72f));
                Polygon(ink,new Vector2(-63,49),new Vector2(-12,38),new Vector2(-20,7),new Vector2(-60,16));
                Polygon(new Color(.66f,.79f,.39f),new Vector2(-60,43),new Vector2(-16,34),new Vector2(-24,12),new Vector2(-57,19));
                for(var i=0;i<5;i++)Ellipse(new Vector2(-6+i*12,24-(i%3)*14),new Vector2(4,4),new Color(.73f,.42f,.18f));return;
            }
            if(kind=="pellets"){
                for(var i=0;i<5;i++){var a=i*2.4f;Ellipse(new Vector2(Mathf.Cos(a)*58,Mathf.Sin(a)*28),new Vector2(7,9),new Color(.56f,.32f,.12f));}return;
            }
            if(kind=="water"){
                var pale=new Color(.86f,.98f,1,.38f);
                for(var i=0;i<6;i++){
                    var t=Mathf.Repeat(phase*.25f+i*.167f,1);var c=new Color(pale.r,pale.g,pale.b,(1-t)*.36f);
                    Arc(new Vector2(Mathf.Sin(i*4.8f)*62,Mathf.Cos(i*3.1f)*22),8+21*t,3+8*t,.12f,2.95f,1.4f,c);
                    Arc(new Vector2(Mathf.Sin(i*4.8f)*62,Mathf.Cos(i*3.1f)*22),8+21*t,3+8*t,3.35f,6.1f,1.2f,c);
                }
                return;
            }
            if(kind=="cascade"){
                for(var i=0;i<5;i++){var y=46-Mathf.Repeat(phase*45+i*22,110);Line(new Vector2(-54+i*23,y),new Vector2(-52+i*23,y-17),6,new Color(.86f,.99f,1,.7f));}
                return;
            }
            if(kind=="rod"){
                // This mesh uses world-sized coordinates rather than icon units.
                scale=Vector2.one;var hand=new Vector2(36,92);var tip=new Vector2(75,181);
                Line(hand,tip,7,ink);Line(hand,tip,4,new Color(.84f,.56f,.28f));
                var previous=tip;for(var i=1;i<=20;i++){var t=i/20f;var point=Vector2.Lerp(tip,end,t)+Vector2.down*(Mathf.Sin(t*Mathf.PI)*25);Line(previous,point,1.8f,new Color(.94f,.95f,.89f));previous=point;}
                Ellipse(end,new Vector2(8,11),ink);Ellipse(end+new Vector2(0,3),new Vector2(5,7),new Color(.97f,.44f,.3f));return;
            }
        }
        public void OnPointerClick(PointerEventData e)
        {
            if(clicked==null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var point))return;
            clicked(point);
        }
    }
}
