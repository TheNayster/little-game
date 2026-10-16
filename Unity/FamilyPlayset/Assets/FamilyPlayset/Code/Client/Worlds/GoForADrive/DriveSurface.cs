using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed class DriveSurface:Button
    {
        public Action<Vector2> clicked;
        public override void OnPointerClick(PointerEventData eventData)
        {
            var rect=transform as RectTransform;
            if(rect && RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,eventData.position,eventData.pressEventCamera,out var point))clicked?.Invoke(point);
        }
    }
}
