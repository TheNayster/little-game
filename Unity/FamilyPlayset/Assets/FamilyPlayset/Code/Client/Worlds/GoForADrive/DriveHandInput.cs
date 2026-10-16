using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed class DriveHandInput:Button,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public Action tapped,released;
        public Action<float> moved;
        public RectTransform surface;
        private float sentAt=-1,endedAt=-1,lastHeight;
        private bool dragging;
        private void Update(){if(dragging && IsActive() && IsInteractable() && Time.unscaledTime-sentAt>.2f){sentAt=Time.unscaledTime;moved?.Invoke(lastHeight);}}
        protected override void OnDisable(){bool wasDragging=dragging;dragging=false;if(wasDragging)released?.Invoke();base.OnDisable();}
        public override void OnPointerClick(PointerEventData e){if(IsActive() && IsInteractable() && Time.unscaledTime-endedAt>.2f)tapped?.Invoke();}
        public void OnBeginDrag(PointerEventData e){dragging=true;sentAt=-1;OnDrag(e);}
        public void OnDrag(PointerEventData e)
        {
            if(Time.unscaledTime-sentAt<.12f)return;
            if(surface && RectTransformUtility.ScreenPointToLocalPointInRectangle(surface,e.position,e.pressEventCamera,out var point)){
                sentAt=Time.unscaledTime;lastHeight=Mathf.Clamp01((point.y+60)/300);moved?.Invoke(lastHeight);
            }
        }
        public void OnEndDrag(PointerEventData e){dragging=false;endedAt=Time.unscaledTime;released?.Invoke();}
    }
}
