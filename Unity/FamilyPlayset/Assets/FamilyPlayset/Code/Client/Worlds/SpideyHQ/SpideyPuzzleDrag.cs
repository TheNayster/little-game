using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    public sealed class SpideyPuzzleDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public Action<PointerEventData> begin,drag,end;
        public void OnBeginDrag(PointerEventData data){data.eligibleForClick=false;begin?.Invoke(data);}
        public void OnDrag(PointerEventData data){drag?.Invoke(data);}
        public void OnEndDrag(PointerEventData data){end?.Invoke(data);}
    }
}
