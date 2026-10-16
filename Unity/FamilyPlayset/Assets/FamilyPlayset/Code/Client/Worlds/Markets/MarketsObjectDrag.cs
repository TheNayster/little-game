using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    // Reusable source-object gesture adapter. The source stays on its peg while
    // the screen owns a non-raycasting ghost; only a valid release dispatches
    // an existing authoritative action. Tap remains the accessible alternate.
    // Pointer identity prevents a second finger from stealing an active drag;
    // cancellation/disable never commits the half-finished gesture.
    public sealed class MarketsObjectDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler
    {
        public Action<PointerEventData> begin,drag,end;
        public Action cancel;
        private int pointer=int.MinValue;
        public void OnBeginDrag(PointerEventData data){if(pointer!=int.MinValue)return;pointer=data.pointerId;data.eligibleForClick=false;begin?.Invoke(data);drag?.Invoke(data);}
        public void OnDrag(PointerEventData data){if(pointer==data.pointerId)drag?.Invoke(data);}
        public void OnEndDrag(PointerEventData data){if(pointer!=data.pointerId)return;pointer=int.MinValue;data.eligibleForClick=false;end?.Invoke(data);}
        public void OnCancel(BaseEventData data){pointer=int.MinValue;cancel?.Invoke();}
        private void OnDisable(){pointer=int.MinValue;cancel?.Invoke();}
    }
}
