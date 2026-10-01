using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    // The same event path receives a finger or mouse, including an off-surface
    // release. A second local finger cannot replace a gesture already in progress.
    internal sealed class VetTouchSurface : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler,IInitializePotentialDragHandler
    {
        public Action<Vector2,string,bool> Stroke; public Action End,Begin; public RectTransform Target;
        private int finger=int.MinValue;private string gesture;
        public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=false;}
        public void OnPointerDown(PointerEventData e){if(finger!=int.MinValue)return;finger=e.pointerId;gesture=Guid.NewGuid().ToString("N");Begin?.Invoke();Sample(e,true);}
        public void OnDrag(PointerEventData e){if(e.pointerId==finger)Sample(e,false);}
        private void Sample(PointerEventData e,bool first){var rect=Target!=null?Target:(RectTransform)transform;if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var p)){var n=new Vector2(p.x/rect.rect.width+.5f,p.y/rect.rect.height+.5f);if(n.x>=0 && n.x<=1 && n.y>=0 && n.y<=1)Stroke?.Invoke(n,gesture,first);}}
        public void OnPointerUp(PointerEventData e){if(e.pointerId!=finger)return;finger=int.MinValue;End?.Invoke();}
        public void Cancel(){finger=int.MinValue;End?.Invoke();}
        private void OnDisable(){Cancel();}
    }
}
