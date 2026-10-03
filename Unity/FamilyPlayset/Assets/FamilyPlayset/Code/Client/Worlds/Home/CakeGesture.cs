using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    // One finger owns this local gesture. A stage token prevents its tail, a
    // second finger, or a late network reply from working on the next activity.
    public sealed class CakeGesture:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public GameScreen Screen;
        private int pointer=int.MinValue;
        private int clickPointer=int.MinValue;
        private bool dragged;
        private Vector2 last,start;
        private string token;
        public void OnPointerDown(PointerEventData e){if(pointer!=int.MinValue)return;pointer=clickPointer=e.pointerId;dragged=false;last=start=e.position;token=Screen.CakeToken;}
        public void OnBeginDrag(PointerEventData e){if(e.pointerId==pointer)dragged=true;}
        public void OnDrag(PointerEventData e){if(e.pointerId!=pointer)return;Screen.CakeStroke(token,last,e.position,false,start);last=e.position;}
        public void OnEndDrag(PointerEventData e){if(e.pointerId!=pointer)return;Screen.CakeStroke(token,last,e.position,true,start);pointer=int.MinValue;}
        public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer && !dragged)pointer=int.MinValue;}
        public void OnPointerClick(PointerEventData e){if(e.pointerId==clickPointer && !dragged && pointer==int.MinValue && token==Screen.CakeToken){clickPointer=int.MinValue;Screen.CakeTap();}}
        private void OnDisable(){pointer=int.MinValue;token=null;}
    }
}
