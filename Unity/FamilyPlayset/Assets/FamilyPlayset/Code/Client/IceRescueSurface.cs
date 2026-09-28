using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // A bounded mesh in the same 1000x600 coordinates as the accepted playground.
    public sealed class IceRescueSurface:MaskableGraphic,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public string Icon="";
        public IceRescueState State;
        public Func<bool> CanInteract;
        public Action<Vector2,bool> Touch;
        public Action<Vector2> MoveToy;
        public bool Calm,Water;
        public float StrikeTime=-10;
        public Vector2 Strike;
        private int? pointer;
        private Vector2 last;
        private bool toyGesture;
        public void Cancel(){pointer=null;toyGesture=false;}
        protected override void OnDisable(){Cancel();base.OnDisable();}
        private Vector2 Point(PointerEventData e){RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);var r=rectTransform.rect;return new Vector2((p.x-r.xMin)/r.width*1000,(r.yMax-p.y)/r.height*600);}
        public void OnPointerDown(PointerEventData e){
            if(pointer.HasValue || !(CanInteract?.Invoke()??false) || State==null)return;
            last=Point(e);toyGesture=State.freed;
            if(toyGesture && (Mathf.Abs(last.x-State.x)>160 || Mathf.Abs(last.y-State.y)>130))return;
            pointer=e.pointerId;if(!toyGesture)Touch?.Invoke(last,false);
        }
        public void OnDrag(PointerEventData e){if(pointer!=e.pointerId || !(CanInteract?.Invoke()??false))return;var p=Point(e);if(!toyGesture && Vector2.Distance(last,p)>40){last=p;Touch?.Invoke(p,true);}}
        public void OnPointerUp(PointerEventData e){if(pointer!=e.pointerId)return;pointer=null;if(toyGesture && (CanInteract?.Invoke()??false)){var p=Point(e);MoveToy?.Invoke(new Vector2(Mathf.Clamp(p.x,IceRescue.ToyMinX,IceRescue.ToyMaxX),Mathf.Clamp(p.y,IceRescue.ToyMinY,IceRescue.ToyMaxY)));}toyGesture=false;}
        private Vector3 V(Vector2 p){var r=rectTransform.rect;return new Vector3(r.xMin+p.x/1000*r.width,r.yMax-p.y/600*r.height);}
        private void Poly(VertexHelper h,Color c,params Vector2[] p){var start=h.currentVertCount;foreach(var v in p)h.AddVert(V(v),c,Vector2.zero);for(var i=1;i<p.Length-1;i++)h.AddTriangle(start,start+i,start+i+1);}
        private void Ellipse(VertexHelper h,float x,float y,float rx,float ry,Color c){var p=new Vector2[32];for(var i=0;i<p.Length;i++){var a=i*Mathf.PI*2/p.Length;p[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}Poly(h,c,p);}
        private void Line(VertexHelper h,Vector2 a,Vector2 b,float width,Color c){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width/2;Poly(h,c,a+n,b+n,b-n,a-n);}
        private void Round(VertexHelper h,float x,float y,float w,float height,float radius,Color c){var p=new Vector2[28];for(var corner=0;corner<4;corner++)for(var i=0;i<7;i++){var a=(corner*90+i*15)*Mathf.Deg2Rad;var cx=corner==0 || corner==3?x+w-radius:x+radius;var cy=corner<2?y+height-radius:y+radius;p[corner*7+i]=new Vector2(cx+Mathf.Cos(a)*radius,cy+Mathf.Sin(a)*radius);}Poly(h,c,p);}
        private void Hammer(VertexHelper h,float x,float y,float size){Round(h,x-size*.1f,y,size*.2f,size*.9f,size*.07f,new Color(.67f,.42f,.23f));Round(h,x-size*.42f,y-size*.13f,size*.84f,size*.34f,size*.08f,new Color(.27f,.46f,.54f));Line(h,new Vector2(x-size*.3f,y-size*.04f),new Vector2(x+size*.28f,y-size*.04f),size*.035f,new Color(.76f,.89f,.89f));}
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var pale=new Color(.86f,.97f,.99f);var teal=new Color(.22f,.55f,.58f);
            if(Icon=="card"){Round(h,8,12,984,580,85,color);return;}
            if(Icon=="hammer"){Hammer(h,500,145,340);return;}
            if(Icon=="water"){Round(h,270,190,390,320,70,new Color(.42f,.76f,.91f));Poly(h,teal,new Vector2(630,260),new Vector2(830,175),new Vector2(695,380));Line(h,new Vector2(280,245),new Vector2(210,375),55,teal);Ellipse(h,455,200,188,40,pale);return;}
            if(Icon=="dinosaur"){Ellipse(h,470,355,215,135,new Color(.58f,.74f,.37f));Ellipse(h,620,190,110,92,new Color(.65f,.8f,.44f));Round(h,550,175,95,190,35,new Color(.58f,.74f,.37f));Poly(h,new Color(.58f,.74f,.37f),new Vector2(330,335),new Vector2(125,200),new Vector2(320,440));Round(h,405,390,85,150,25,teal);Round(h,570,390,85,150,25,teal);Ellipse(h,645,166,13,16,Color.black);return;}
            if(Icon=="tray"){Ellipse(h,500,470,377,53,new Color(.25f,.45f,.48f,.15f));Round(h,86,83,828,424,84,new Color(.26f,.6f,.63f));Round(h,101,91,798,390,73,new Color(.87f,.96f,.93f));Ellipse(h,500,440,333,30,new Color(.42f,.77f,.85f,.22f));return;}
            if(State==null)return;
            for(var i=0;i<IceRescue.Cells;i++){
                var amount=State.cells[i];if(amount<=0)continue;var x=IceRescue.X(i);var y=IceRescue.Y(i);
                var opacity=.25f+amount*.45f;
                Poly(h,new Color(.42f,.78f,.93f,opacity),new Vector2(x-38,y-36),new Vector2(x+36,y-35),new Vector2(x+39,y+34),new Vector2(x-35,y+37));
                Poly(h,new Color(.9f,.99f,1,.15f+amount*.26f),new Vector2(x-37,y-35),new Vector2(x+34,y-35),new Vector2(x-33,y+32));
                Line(h,new Vector2(x-34,y-30),new Vector2(x+29,y-30),2,new Color(1,1,1,.5f));
                if(amount<.8f){var crack=new Color(.25f,.53f,.69f,.7f);Line(h,new Vector2(x-34,y-22),new Vector2(x+8,y+8),3,crack);Line(h,new Vector2(x+8,y+8),new Vector2(x-4,y+34),3,crack);Line(h,new Vector2(x+8,y+8),new Vector2(x+34,y-4),3,crack);}
                if(State.energy[i]>0)Ellipse(h,x+16,y-7,7,11,new Color(.32f,.7f,.92f,.68f));
            }
            var age=Time.unscaledTime-StrikeTime;
            if(!Calm && age>=0 && age<.4f){
                if(Water){for(var i=0;i<5;i++)Ellipse(h,Strike.x+(i-2)*12,Strike.y-70+age*200+i*5,5,9,new Color(.26f,.68f,.9f,1-age*2));}
                else {Hammer(h,Strike.x+30,Strike.y-85+Mathf.Sin(age*16)*15,75);for(var i=0;i<6;i++){var x=Strike.x+Mathf.Cos(i)*age*190;var y=Strike.y-70*age+180*age*age+Mathf.Sin(i)*30;Poly(h,new Color(.58f,.86f,1,1-age*2),new Vector2(x,y),new Vector2(x+12,y+5),new Vector2(x+4,y+15));}}
            }
        }
    }
}
