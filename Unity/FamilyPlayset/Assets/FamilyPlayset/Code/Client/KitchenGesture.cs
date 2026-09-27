using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    // Big tap alternatives and forgiving worktop gestures share one authority
    // operation. A drag never manufactures a second ingredient world object.
    public sealed class KitchenGesture:MonoBehaviour,IPointerDownHandler,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public SoloScreen Screen;
        public string Ingredient;
        private bool dragged;
        public void OnPointerDown(PointerEventData e){dragged=false;}
        public void OnPointerClick(PointerEventData e){if(dragged){dragged=false;return;}if(Ingredient==null)Screen.KitchenStepGesture();else Screen.AddKitchenIngredient(Ingredient);}
        public void OnBeginDrag(PointerEventData e){dragged=true;}
        public void OnDrag(PointerEventData e){}
        public void OnEndDrag(PointerEventData e){if(Ingredient==null)Screen.KitchenStepGesture();else Screen.AddKitchenIngredient(Ingredient,e.position);}
    }
}
