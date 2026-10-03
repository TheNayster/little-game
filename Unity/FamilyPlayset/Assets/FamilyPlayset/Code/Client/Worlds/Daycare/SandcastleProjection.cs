using UnityEngine;
using LittleWeeps.Core;

namespace LittleWeeps.Client
{
    // Display-only trapezoid. The authority's logical lattice and footprints stay unchanged.
    public static class SandcastleProjection
    {
        public static Vector2 Project(float x,float y)
        {
            var v=(y-DaycareSandpit.Bottom)/(DaycareSandpit.Top-DaycareSandpit.Bottom);
            var half=Mathf.Lerp(615,445,v);
            return new Vector2(((x-DaycareSandpit.Left)/(DaycareSandpit.Right-DaycareSandpit.Left)*2-1)*half,Mathf.Lerp(-250,200,v));
        }
        public static Vector2 Inverse(Vector2 p)
        {
            var v=(p.y+250)/450;
            var half=Mathf.LerpUnclamped(615,445,v);
            return new Vector2(DaycareSandpit.Left+(p.x/half+1)*.5f*(DaycareSandpit.Right-DaycareSandpit.Left),DaycareSandpit.Bottom+v*(DaycareSandpit.Top-DaycareSandpit.Bottom));
        }
        public static float DepthScale(float y)=>Mathf.Lerp(1.14f,.86f,(y-DaycareSandpit.Bottom)/(DaycareSandpit.Top-DaycareSandpit.Bottom));
    }
}
