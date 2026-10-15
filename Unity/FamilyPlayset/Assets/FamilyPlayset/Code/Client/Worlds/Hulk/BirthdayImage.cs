using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // An atlas frame's transparent padding must not steal a sibling's touch.
    public sealed class BirthdayImage:RawImage
    {
        public override bool Raycast(Vector2 screenPoint,Camera eventCamera)
        {
            if(!base.Raycast(screenPoint,eventCamera) || !RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screenPoint,eventCamera,out var point))return false;
            var rect=rectTransform.rect;if(rect.width<=0 || rect.height<=0)return false;
            var uv=new Vector2(uvRect.x+(point.x-rect.xMin)/rect.width*uvRect.width,uvRect.y+(point.y-rect.yMin)/rect.height*uvRect.height);
            return BirthdayArt.PixelVisible(texture,uv);
        }
    }
}
