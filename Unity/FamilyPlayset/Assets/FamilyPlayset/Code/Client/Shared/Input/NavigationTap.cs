using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // ScrollRect takes drags from these buttons. A canceled touch or a swipe
    // must never become an avatar change or destination selection on release.
    public sealed class NavigationTap : Button
    {
        private Vector2 origin;
        public override void OnPointerDown(PointerEventData e)
        { origin = e.position; base.OnPointerDown(e); }
        public override void OnPointerClick(PointerEventData e)
        {
            var scale = GetComponentInParent<Canvas>().scaleFactor;
            if (Vector2.Distance(origin, e.position) > 12 * scale) return;
            if (e is ExtendedPointerEventData extended && extended.device is Touchscreen screen)
                foreach (var touch in screen.touches)
                    if (touch.touchId.ReadValue() == extended.touchId && touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled) return;
            base.OnPointerClick(e);
        }
    }
}
