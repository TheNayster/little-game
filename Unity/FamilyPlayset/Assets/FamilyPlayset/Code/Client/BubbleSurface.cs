using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed class BubbleSurface:MaskableGraphic,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public string Layer="scene",Icon="water",Effect="";
        public BubbleState State;
        public double Clock;
        public float EffectAt=-20;
        public Vector2 PopAt;
        public float PopRadius;
        public bool Calm;
        public Func<bool> CanInteract;
        public Action<Vector2> Tap;
        private int? pointer;
        private Vector2 down;
        private bool moved;
        public void Cancel(){pointer=null;moved=false;}
        protected override void OnDisable(){Cancel();base.OnDisable();}
        private Vector2 Point(PointerEventData e){RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);var r=rectTransform.rect;return new Vector2((p.x-r.xMin)/r.width*1000,(r.yMax-p.y)/r.height*600);}
        public void OnPointerDown(PointerEventData e){if(pointer.HasValue || !(CanInteract?.Invoke()??false))return;pointer=e.pointerId;down=Point(e);moved=false;}
        public void OnDrag(PointerEventData e){if(pointer==e.pointerId && Vector2.Distance(down,Point(e))>25)moved=true;}
        public void OnPointerUp(PointerEventData e){if(pointer!=e.pointerId)return;pointer=null;if(!moved && (CanInteract?.Invoke()??false))Tap?.Invoke(Point(e));}
        private Vector3 V(Vector2 p){var r=rectTransform.rect;return new Vector3(r.xMin+p.x/1000*r.width,r.yMax-p.y/600*r.height);}
        private void Poly(VertexHelper h,Color c,params Vector2[] p){var start=h.currentVertCount;foreach(var v in p)h.AddVert(V(v),c,Vector2.zero);for(var i=1;i<p.Length-1;i++)h.AddTriangle(start,start+i,start+i+1);}
        private void Oval(VertexHelper h,float x,float y,float rx,float ry,Color c){var p=new Vector2[40];for(var i=0;i<p.Length;i++){var a=i*Mathf.PI*2/p.Length;p[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}Poly(h,c,p);}
        private void Line(VertexHelper h,Vector2 a,Vector2 b,float width,Color c){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width/2;Poly(h,c,a+n,b+n,b-n,a-n);}
        private void Arc(VertexHelper h,float x,float y,float radius,float from,float to,float width,Color c){for(var i=0;i<24;i++){var a=Mathf.Lerp(from,to,i/24f);var b=Mathf.Lerp(from,to,(i+1)/24f);Line(h,new Vector2(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius),new Vector2(x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius),width,c);}}
        private void Bubble(VertexHelper h,float x,float y,float r,float opacity=1){
            var start=h.currentVertCount;h.AddVert(V(new Vector2(x-r*.2f,y-r*.3f)),new Color(.95f,1,1,.04f*opacity),Vector2.zero);
            for(var i=0;i<=48;i++){var a=i*Mathf.PI*2/48;h.AddVert(V(new Vector2(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r)),new Color(.72f,.87f,.96f,.23f*opacity),Vector2.zero);if(i>0)h.AddTriangle(start,start+i,start+i+1);}
            Arc(h,x,y,r,0,Mathf.PI*2,2.3f,new Color(.96f,1,1,.9f*opacity));
            var colors=new[]{new Color(.96f,.7f,.84f),new Color(1,.92f,.65f),new Color(.53f,.88f,.81f),new Color(.7f,.7f,.96f)};
            for(var i=0;i<4;i++){var c=colors[i];c.a=.8f*opacity;Arc(h,x,y,r*.94f,i*1.57f+.12f,i*1.57f+1.35f,4,c);}
            Arc(h,x,y,r*.78f,3.7f,4.8f,Mathf.Max(2,r*.05f),new Color(1,1,1,.95f*opacity));Oval(h,x+r*.35f,y+r*.5f,r*.08f,r*.045f,new Color(1,1,1,.8f*opacity));
        }
        private void Wand(VertexHelper h,float x,float y,float size,bool square,bool film){
            var wood=new Color(.75f,.57f,.29f);Line(h,new Vector2(x-size*.2f,y+size*.85f),new Vector2(x,y+size*.3f),size*.065f,wood);
            if(square){var r=size*.31f;var p=new[]{new Vector2(x-r,y-r),new Vector2(x+r,y-r),new Vector2(x+r,y+r),new Vector2(x-r,y+r)};if(film)Poly(h,new Color(.72f,.87f,.95f,.35f),p);for(var i=0;i<4;i++)Line(h,p[i],p[(i+1)%4],size*.04f,new Color(.94f,.76f,.39f));}
            else{if(film)Bubble(h,x,y,size*.32f,.6f);Arc(h,x,y,size*.32f,0,Mathf.PI*2,size*.04f,new Color(.94f,.76f,.39f));}
        }
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var age=Time.unscaledTime-EffectAt;
            if(Layer=="icon"){
                if(Icon=="blow" || Icon=="size" || Icon=="pop"){Bubble(h,430,260,140);Bubble(h,675,180,85);Bubble(h,620,430,65);}
                else if(Icon=="shape" || Icon=="dip")Wand(h,500,180,500,State?.square??false,true);
                else if(Icon=="stir"){Line(h,new Vector2(650,90),new Vector2(410,400),65,new Color(.69f,.5f,.3f));Oval(h,400,416,92,62,new Color(.87f,.72f,.47f));}
                else if(Icon=="air"){for(var i=0;i<3;i++)Line(h,new Vector2(260,160+i*120),new Vector2(760-i*60,160+i*120),35,new Color(.48f,.75f,.77f));}
                else if(Icon=="refill"){Arc(h,500,300,180,.5f,5.8f,38,new Color(.32f,.66f,.62f));Poly(h,new Color(.32f,.66f,.62f),new Vector2(660,160),new Vector2(805,160),new Vector2(735,295));}
                return;
            }
            if(State==null)return;
            if(Layer=="liquid"){
                if(State.water){var level=State.mixed?360+(16-State.solution)*2:358;var c=new Color(.42f,.8f,.91f,.65f);Poly(h,c,new Vector2(325,level),new Vector2(655,level),new Vector2(620,432),new Vector2(585,459),new Vector2(395,459),new Vector2(360,432));Oval(h,490,level,165,40,new Color(.65f,.92f,.94f,.8f));
                    if(State.soap)for(var i=0;i<12;i++){var a=i*2.4f;Oval(h,490+Mathf.Cos(a)*(35+i*8),level+Mathf.Sin(a)*(8+i),6+i%3*3,5+i%3*2,new Color(.97f,1,1,.86f));}
                    if(!Calm && Effect=="stir" && age<.8f)for(var i=0;i<3;i++)Arc(h,490,level,40+i*30,age*8+i,age*8+i+2,3,new Color(1,1,1,.6f));}
                return;
            }
            var dip=!Calm && Effect=="dip" && age<.6f?Mathf.Sin(Mathf.Clamp01(age/.6f)*Mathf.PI):0;
            Wand(h,Mathf.Lerp(790,490,dip),Mathf.Lerp(245,360,dip),200,State.square,State.film>0);
            var spoon=!Calm && Effect=="stir" && age<.8f?Mathf.Sin(age*18)*35:0;
            Line(h,new Vector2(575+spoon,262),new Vector2(500+spoon,395),12,new Color(.71f,.51f,.3f));Oval(h,495+spoon,402,20,13,new Color(.86f,.71f,.49f));
            if(!Calm && age<.55f && (Effect=="water" || Effect=="soap")){var start=Effect=="water"?new Vector2(240,185):new Vector2(220,330);var c=Effect=="water"?new Color(.36f,.76f,.95f,.7f):new Color(.95f,.83f,.46f,.8f);Line(h,start,new Vector2(460,365),Effect=="water"?13:7,c);}
            foreach(var b in State.floating)if(BubbleLab.Alive(b,Clock))Bubble(h,(float)BubbleLab.X(b,Clock),(float)BubbleLab.Y(b,Clock),b.radius);
            if(!Calm && Effect=="pop" && age<.35f)for(var i=0;i<8;i++){var a=i*Mathf.PI/4;var r=PopRadius+age*65;Line(h,PopAt+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r),PopAt+new Vector2(Mathf.Cos(a)*(r+13),Mathf.Sin(a)*(r+13)),3,new Color(1,.93f,.66f,1-age/.35f));}
        }
    }
}
