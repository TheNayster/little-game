using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform keepyRoot,keepyBalloon,keepyShadow;
        private BalloonGraphic keepyArt;
        private KeepyState keepyShown;
        public KeepyState Keepy=>HasWorld?(Shared?shared.View.keepy:World.ReadKeepy()):null;
        public Vector2 KeepyBalloonPoint=>keepyBalloon==null?Vector2.zero:keepyBalloon.anchoredPosition;
        private void BuildKeepy()
        {
            keepyShown=null;
            keepyRoot=Rect(Board,"Keepy Uppy balloon",Vector2.zero,Vector2.zero);
            keepyShadow=Panel(keepyRoot,"Balloon ground shadow",new Vector2(0,-45),new Vector2(52,12),new Color(.28f,.38f,.27f,.22f),false,true).rectTransform;
            keepyBalloon=Rect(keepyRoot,"Red balloon",Vector2.zero,new Vector2(80,132));
            keepyArt=keepyBalloon.gameObject.AddComponent<BalloonGraphic>();keepyArt.raycastTarget=false;
            // A balloon tap is independent of the other finger's joystick or
            // object drag. Keep its gesture lease and tap-to-walk destination.
            HomeHit(keepyBalloon,"Play Keepy Uppy",Vector2.zero,new Vector2(100,105),()=>StartActivity(KeepyRules.Activity),cancelPointers:false);
            TickKeepy();
        }
        private void TickKeepy()
        {
            if(keepyRoot==null)return;
            var b=Keepy;var available=b!=null && CurrentArea=="garden";
            keepyRoot.gameObject.SetActive(available);
            if(!available)return;
            // Smooth the shared snapshots for presentation only. A hit/round
            // boundary snaps to its authoritative contact, never invents a tap.
            if(keepyShown==null || keepyShown.round!=b.round || keepyShown.hitSerial!=b.hitSerial || keepyShown.phase!=b.phase)
            {keepyShown=b.Copy();}
            else
            {
                var blend=1-Mathf.Exp(-Time.unscaledDeltaTime*22);
                keepyShown.x=Mathf.Lerp(keepyShown.x,b.x,blend);
                keepyShown.height=Mathf.Lerp(keepyShown.height,b.height,blend);
                keepyShown.y=b.y;
            }
            keepyRoot.anchoredPosition=ToBoard(keepyShown.x,keepyShown.y);keepyRoot.localScale=Vector3.one*sceneScale;
            keepyBalloon.anchoredPosition=new Vector2(0,keepyShown.height-45);
            keepyBalloon.localRotation=Quaternion.Euler(0,0,-b.vx*.055f+Mathf.Sin((float)b.elapsed*2)*3);
            var squash=b.hitAge<.14?Mathf.Sin((float)b.hitAge/.14f*Mathf.PI)*.07f:0;
            keepyBalloon.localScale=new Vector3(1+squash,1-squash,1);
            keepyArt.Sway((float)b.elapsed*2);
            keepyShadow.localScale=Vector3.one*Mathf.Lerp(1,.5f,Mathf.Clamp01(b.height/400));
        }
    }
}
