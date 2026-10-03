using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // The saved quantities are authoritative; streams, swirls and interpolation
    // are bounded local feedback and never create physics/network particles.
    public sealed class LiquidColorSurface:MaskableGraphic,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public LiquidColorState State;
        public bool Front,Calm,Icon;
        public int PourIndex=-1;
        public float ChangedAt=-20;
        public Color FromColor;
        public float FromVolume;
        public Func<bool> CanInteract;
        public Action<int> Pour;
        private int? pointer;
        private Vector2 down;
        private bool moved;
        public static Color Hue(int index)=>index==0?new Color(.906f,.263f,.345f):index==1?new Color(.973f,.831f,.216f):index==2?new Color(.251f,.510f,.863f):new Color(.76f,.9f,.94f);
        public static Vector2 Bottle(int index)=>index==0?new Vector2(180,280):index==1?new Vector2(500,150):new Vector2(820,280);
        public static Color Mixture(LiquidColorState s){var c=LiquidColorLab.Color(s);return new Color(c[0]/255f,c[1]/255f,c[2]/255f);}
        public float Blend=>Calm?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.unscaledTime-ChangedAt)/.85f));
        public void Cancel(){pointer=null;moved=false;}
        protected override void OnDisable(){Cancel();base.OnDisable();}
        private Vector2 Point(PointerEventData e){RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);var r=rectTransform.rect;return new Vector2((p.x-r.xMin)/r.width*1000,(r.yMax-p.y)/r.height*600);}
        public void OnPointerDown(PointerEventData e){if(pointer.HasValue || !(CanInteract?.Invoke()??false))return;pointer=e.pointerId;down=Point(e);moved=false;}
        public void OnDrag(PointerEventData e){if(pointer==e.pointerId && Vector2.Distance(down,Point(e))>25)moved=true;}
        public void OnPointerUp(PointerEventData e){if(pointer!=e.pointerId)return;pointer=null;if(moved || !(CanInteract?.Invoke()??false))return;var p=Point(e);for(var i=0;i<3;i++){var b=Bottle(i);if(Mathf.Abs(p.x-b.x)<78 && Mathf.Abs(p.y-b.y)<104){Pour?.Invoke(i);break;}}}
        private Vector3 V(float x,float y){var r=rectTransform.rect;return new Vector3(r.xMin+x/1000*r.width,r.yMax-y/600*r.height);}
        private void Quad(VertexHelper h,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color){var i=h.currentVertCount;h.AddVert(V(a.x,a.y),color,Vector2.zero);h.AddVert(V(b.x,b.y),color,Vector2.zero);h.AddVert(V(c.x,c.y),color,Vector2.zero);h.AddVert(V(d.x,d.y),color,Vector2.zero);h.AddTriangle(i,i+1,i+2);h.AddTriangle(i,i+2,i+3);}
        private void Line(VertexHelper h,Vector2 a,Vector2 b,float width,Color c){var n=new Vector2(a.y-b.y,b.x-a.x).normalized*width/2;Quad(h,a+n,b+n,b-n,a-n,c);}
        private void Oval(VertexHelper h,float x,float y,float rx,float ry,Color c){var start=h.currentVertCount;h.AddVert(V(x,y),c,Vector2.zero);for(var i=0;i<=48;i++){var a=i*Mathf.PI*2/48;h.AddVert(V(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry),c,Vector2.zero);if(i>0)h.AddTriangle(start,start+i,start+i+1);}}
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();if(Icon){for(var i=0;i<3;i++){Oval(h,220+i*280,300,112,165,Hue(i));Oval(h,188+i*280,228,25,42,new Color(1,1,1,.65f));}return;}
            if(State==null)return;var age=Time.unscaledTime-ChangedAt;var blend=Blend;var final=Mixture(State);var color=Color.Lerp(FromColor,final,blend);var volume=Mathf.Lerp(FromVolume,LiquidColorLab.Volume(State),blend);
            if(!Front){
                if(volume<.01f)return;var level=454-volume/12*112;var half=105+(454-level)*.48f;
                Quad(h,new Vector2(490-half,level),new Vector2(490+half,level),new Vector2(586,460),new Vector2(394,460),color);
                Oval(h,490,level,half,Mathf.Min(23,5+volume*1.5f),Color.Lerp(color,Color.white,.2f));
                if(!Calm && age<.9f)for(var band=0;band<3;band++)if(State.parts[band]>0){var tint=Hue(band);tint.a=(1-blend)*.55f;var y=level+12+(458-level-20)*(band+1)/4;for(var i=0;i<22;i++){var x=405+i*8;Line(h,new Vector2(x,y+Mathf.Sin(i*.24f+age*5+band)*5),new Vector2(x+8,y+Mathf.Sin((i+1)*.24f+age*5+band)*5),4,tint);}}
                return;
            }
            if(!Calm && PourIndex>=0 && age<.6f){var start=PourIndex<3?Bottle(PourIndex):new Vector2(745,130);var end=new Vector2(490,365);var c=Hue(PourIndex);c.a=Mathf.Clamp01((.6f-age)*5);var last=start;for(var i=1;i<=28;i++){var t=i/28f;var p=Vector2.Lerp(start,end,t);p.y-=Mathf.Sin(t*Mathf.PI)*50;Line(h,last,p,8,c);last=p;}for(var i=0;i<6;i++)Oval(h,470+i*8,367+Mathf.Sin(i+age*15)*8,3,4,c);}
            var filled=LiquidColorLab.Volume(State);var index=0;
            for(var i=0;i<12;i++){var c=new Color(.9f,.92f,.87f);if(i<filled){while(index<3 && i>=Count(index))index++;c=index<3?Hue(index):Hue(3);}Oval(h,358+i*24,480,8,8,new Color(.35f,.5f,.45f,.7f));Oval(h,358+i*24,480,6,6,c);}
        }
        private int Count(int index){var n=0;for(var i=0;i<=index;i++)n+=State.parts[i];return n;}
    }
}
