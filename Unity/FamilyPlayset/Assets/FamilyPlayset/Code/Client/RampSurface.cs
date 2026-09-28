using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed class RampSurface:MaskableGraphic,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public RampCourse Course,Preview;
        public Func<bool> CanInteract;
        public Func<string> CurrentToken;
        public Action<int,Vector2,string> Commit;
        public int Selected;
        public bool Calm;
        public MarblePath Path;
        public double Clock;
        public bool Released;
        private int? pointer;
        private string token;
        private Vector2 start;
        private bool armed,placing,moved;
        public string GestureToken=>pointer.HasValue?token:null;
        public void Cancel(){pointer=null;Preview=null;armed=placing=moved=false;SetVerticesDirty();}
        protected override void OnDisable(){Cancel();base.OnDisable();}
        private Vector2 Point(PointerEventData e){RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);return new Vector2(p.x+500,300-p.y);}
        public void OnPointerDown(PointerEventData e)
        {
            if(pointer.HasValue || Course==null || !(CanInteract?.Invoke()??false))return;
            var p=Point(e);var index=-1;var distance=46f;
            for(var i=0;i<6;i++){var d=Vector2.Distance(p,new Vector2(Course.points[i*2],Course.points[i*2+1]));if(d<distance){distance=d;index=i;}}
            if(index<0 && !armed)return;
            // A nearby second tap is a small adjustment of the selected handle,
            // even when it remains inside that handle's generous touch target.
            placing=armed && (index<0 || index==Selected && distance>8);if(index>=0)Selected=index;
            pointer=e.pointerId;token=CurrentToken();start=p;moved=false;Preview=Course.Copy();SetVerticesDirty();
        }
        public void OnDrag(PointerEventData e)
        {
            if(pointer!=e.pointerId || !(CanInteract?.Invoke()??false))return;
            if(token!=CurrentToken()){Cancel();return;}
            var p=Point(e);moved|=Vector2.Distance(p,start)>8;if(moved){MarbleRamps.Move(Preview,Selected,(int)p.x,(int)p.y);SetVerticesDirty();}
        }
        public void OnPointerUp(PointerEventData e)
        {
            if(pointer!=e.pointerId)return;pointer=null;var p=Point(e);
            // Small endpoint adjustments may remain below the UI module's
            // screen-pixel drag threshold. Still honor their final board position.
            moved|=Vector2.Distance(p,start)>8;
            var send=(moved || placing) && (CanInteract?.Invoke()??false) && token==CurrentToken() && p.x>=0 && p.x<=1000 && p.y>=0 && p.y<=600;
            armed=!send;Preview=null;if(send){armed=false;Commit?.Invoke(Selected,p,token);}SetVerticesDirty();
        }
        private static Vector3 V(float x,float y)=>new Vector3(x-500,300-y);
        private void Line(VertexHelper h,Vector2 a,Vector2 b,float width,Color c){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width/2;var k=h.currentVertCount;foreach(var p in new[]{a+n,b+n,b-n,a-n})h.AddVert(V(p.x,p.y),c,Vector2.zero);h.AddTriangle(k,k+1,k+2);h.AddTriangle(k,k+2,k+3);}
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var c=Preview??Course;if(c==null)return;
            var x=c.points[Selected*2];var y=c.points[Selected*2+1];var at=h.currentVertCount;h.AddVert(V(x,y),new Color(1,.78f,.22f,.25f),Vector2.zero);
            for(var i=0;i<=32;i++){var a=i*Mathf.PI/16;h.AddVert(V(x+Mathf.Cos(a)*39,y+Mathf.Sin(a)*39),new Color(1,.78f,.22f,.25f),Vector2.zero);if(i>0)h.AddTriangle(at,at+i,at+i+1);}
            if(!Calm && Released && Path!=null && Preview==null){var previous=Path.At(Math.Max(0,Clock-.8));for(var i=1;i<=16;i++){var p=Path.At(Math.Max(0,Clock-.8+i*.05));Line(h,new Vector2((float)previous.x,(float)previous.y),new Vector2((float)p.x,(float)p.y),3,new Color(.23f,.57f,.75f,.2f));previous=p;}}
        }
    }
}
