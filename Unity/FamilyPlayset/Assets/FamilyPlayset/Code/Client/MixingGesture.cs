using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    public sealed class MixingGesture:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public Action<Vector2> Begin,Move;
        public Action<bool> End;
        private int? pointer;private Vector2 origin;private bool moved;
        public void OnPointerDown(PointerEventData e){if(pointer.HasValue)return;pointer=e.pointerId;origin=e.position;moved=false;Begin?.Invoke(e.position);}
        public void OnDrag(PointerEventData e){if(pointer!=e.pointerId)return;moved|=Vector2.Distance(origin,e.position)>10;Move?.Invoke(e.position);}
        public void OnPointerUp(PointerEventData e){if(pointer!=e.pointerId)return;pointer=null;End?.Invoke(moved);}
        private void OnDisable(){if(pointer.HasValue){pointer=null;End?.Invoke(true);}}
    }
}
