using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    public sealed class FunfairImage:RawImage,ICanvasRaycastFilter
    {
        public bool IsRaycastLocationValid(Vector2 screenPoint,Camera eventCamera){if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screenPoint,eventCamera,out var local))return false;var r=rectTransform.rect;var uv=new Vector2(uvRect.x+(local.x-r.x)/r.width*uvRect.width,uvRect.y+(local.y-r.y)/r.height*uvRect.height);return FunfairArt.PixelVisible(texture,uv);}
    }
}
