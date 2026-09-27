using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace LittleWeeps.Client
{
    // UGUI keeps each touch attached to its original press target. A second
    // pointer cannot replace this surface's active lease.
    public sealed class SoloPointerSurface : UIBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, ICancelHandler
    {
        public SoloScreen Screen;
        public string Role;
        private int? pointer;
        private bool deferredBook;
        private UnityEngine.Vector2 bookDown;
        public bool Pressed => pointer.HasValue;
        public void OnPointerDown(PointerEventData e)
        {
            if(pointer.HasValue)return;
            deferredBook=Screen.IsBook(Role);bookDown=e.position;
            if(!deferredBook && !Screen.BeginPointer(Role,e.position))return;
            pointer = e.pointerId;
        }
        public void OnDrag(PointerEventData e)
        {
            if(pointer!=e.pointerId)return;
            if(deferredBook){if(UnityEngine.Vector2.Distance(bookDown,e.position)<UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold)return;deferredBook=false;if(!Screen.BeginPointer(Role,bookDown)){pointer=null;return;}}
            Screen.MovePointer(Role,e.position);
        }
        public void OnPointerUp(PointerEventData e)
        {
            if (pointer != e.pointerId) return;
            pointer = null;
            if (e is ExtendedPointerEventData extended && extended.device is Touchscreen touchscreen)
            {
                foreach (var touch in touchscreen.touches)
                    if (touch.touchId.ReadValue() == extended.touchId && touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                    { Screen.CancelPointer(Role); return; }
            }
            if(deferredBook){deferredBook=false;Screen.OpenBook(Role);}else Screen.EndPointer(Role,e.position);
        }
        public void OnCancel(BaseEventData e) => Cancel();
        public void Cancel() { if (pointer.HasValue) { pointer = null; deferredBook=false; Screen.CancelPointer(Role); } }
        protected override void OnDisable() { Cancel(); base.OnDisable(); }
    }
}
