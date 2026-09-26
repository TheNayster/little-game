using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform stairControl;
        private Text stairCaption;
        private HomeArtPart stairFront;
        private bool stairApproach,stairSubmitted;
        private string stairSource;
        private float stairDeadline;
        private readonly Dictionary<string,(long visit,double elapsed,int frame)> stairVisuals=new Dictionary<string,(long,double,int)>();
        public bool StairBusy=>Ready && (stairSubmitted || ReadPlayer(Actor).stairs>0);
        private string StairPreload=>stairSource=="garden"?"home-upstairs":stairSource==HomeRooms.Landing?"home-living":null;
        private void BuildRooms()
        {
            if(SceneSchema<HomeRooms.Schema)return;
            stairControl=Rect(Board,"Home stairs",Vector2.zero,new Vector2(170,105));
            var hit=Plain(stairControl,"Stair entry",new Vector2(0,45),new Vector2(180,210),Color.clear,true);
            hit.canvasRenderer.cullTransparentMesh=false;
            NavButton(hit,RequestStairs);
            stairCaption=Label(stairControl,"Upstairs ↑",22,new Vector2(0,-30),new Vector2(180,40));
            stairFront=Rect(Board,"Stair foreground",Vector2.zero,Vector2.zero).gameObject.AddComponent<HomeArtPart>();
            stairFront.raycastTarget=false;
        }
        public void RequestStairs()
        {
            if(!Ready || MenuOpen || TravelPending || WorldLoading || StairBusy || doorSubmitted || !HomeRooms.StairArea(CurrentArea))return;
            CancelDoorApproach();
            stairSource=CurrentArea;stairApproach=true;stairDeadline=Time.realtimeSinceStartup+30;
            manualCamera=false;destination=new Vector2(HomeRooms.EntryX(CurrentArea),HomeRooms.EntryY(CurrentArea));
        }
        private void CancelStairApproach()
        {stairApproach=false;stairSubmitted=false;stairSource=null;}
        private void CheckStairInput()
        {
            if(stairControl==null || !Ready)return;
            var p=ReadPlayer(Actor);
            if(MenuOpen || WorldLoading || TravelPending || applicationPaused || Shared && !shared.Connected)
            {if(stairApproach){CancelStairApproach();destination=null;}return;}
            if(p.stairs>0)return;
            if(stairSource!=null && p.zone!=stairSource)
            {CancelStairApproach();destination=null;stickDirection=Vector2.zero;return;}
            if(!stairApproach && !stairSubmitted && HomeRooms.NearEntry(p) && JoystickMode && stickDirection.y>.4f)RequestStairs();
            if(!stairApproach || stairSubmitted)return;
            if(Time.realtimeSinceStartup>stairDeadline)
            {
                CancelStairApproach();destination=null;
                homeFeedback.text="The stairs aren't ready yet. Try again.";homeFeedbackUntil=Time.unscaledTime+3;return;
            }
            if(Vector2.Distance(new Vector2(p.x,p.y),destination??new Vector2(HomeRooms.EntryX(p.zone),HomeRooms.EntryY(p.zone)))>12)return;
            if(!scenicImages.ContainsKey(StairPreload))return;
            stairApproach=false;stairSubmitted=true;destination=null;stickDirection=Vector2.zero;shared?.Walk(WalkMode.Stop);
            void Done(SoloResult r)
            {
                stairSubmitted=false;
                if(!r.Accepted){CancelStairApproach();homeFeedback.text="Try the stairs again.";homeFeedbackUntil=Time.unscaledTime+3;}
                Render();
            }
            if(Shared)SubmitShared(SoloAction.UseStairs,"","","",0,0,Done);
            else Done(Command(SoloAction.UseStairs));
        }
        private Vector2 StairPoint(SoloPlayer player)
        {
            if(player.stairs==0){stairVisuals.Remove(player.id);return new Vector2(player.x,player.y);}
            if(!stairVisuals.TryGetValue(player.id,out var sample) || sample.visit!=player.visit)
                sample=(player.visit,player.stairs,-1);
            // Smooth only a short interval of authoritative progress; never
            // commit a room or extrapolate an outage from the visual timer.
            var elapsed=sample.frame==Time.frameCount?sample.elapsed:Math.Min(HomeRooms.StairDuration-.001,Math.Max(player.stairs,
                Math.Min(player.stairs+.1,sample.elapsed+Math.Min(Time.unscaledDeltaTime,.05f))));
            stairVisuals[player.id]=(player.visit,elapsed,Time.frameCount);
            var t=(float)(elapsed/HomeRooms.StairDuration);
            return player.zone=="garden"?new Vector2(player.x-450*t,player.y+(465*t+40*Mathf.Min(1,t*8))/.45f):
                new Vector2(player.x-380*t,player.y-210*t/.45f);
        }
        private void PresentRooms()
        {
            if(stairControl==null || !Ready)return;
            var own=ReadPlayer(Actor);var property=HomeRooms.StairArea(CurrentArea);
            stairControl.gameObject.SetActive(property && own.stairs==0 && !WorldLoading);
            if(property)
            {
                stairControl.anchoredPosition=ToBoard(HomeRooms.EntryX(CurrentArea),HomeRooms.EntryY(CurrentArea));
                stairControl.localScale=Vector3.one*sceneScale;
                stairCaption.text=stairApproach?"Getting ready…":CurrentArea=="garden"?"Upstairs ↑":"Downstairs ↓";
            }
            var active=Shared?shared.View.players.Where(p=>p.zone==CurrentArea && shared.Players.Contains(p.id)).ToArray():new[]{own};
            foreach(var p in active.Where(p=>p.stairs>0))
            {
                var point=StairPoint(p);var root=p.id==Actor?avatar:friends.TryGetValue(p.id,out var friend)?friend.root:null;
                if(root!=null)root.anchoredPosition=ToBoard(point.x,point.y);
                foreach(var toy in ReadToys().Where(t=>t.holder==p.id))
                    toys[toy.id].anchoredPosition=ToBoard(point.x,point.y)+new Vector2(0,65)*sceneScale;
            }
            var imageId=CurrentArea=="garden"?"home-living":"home-upstairs";
            var hasFront=property && active.Any(p=>p.stairs>0) && scenicImages.ContainsKey(imageId);
            stairFront.gameObject.SetActive(hasFront);
            if(hasFront)
            {
                var image=scenicImages[imageId];stairFront.rectTransform.anchoredPosition=image.rectTransform.anchoredPosition;
                stairFront.rectTransform.sizeDelta=image.rectTransform.sizeDelta;
                stairFront.Configure((Texture2D)image.texture,CurrentArea=="garden"?LowerStairCover:UpperStairCover);
            }
            SortDepth();
        }
        // Convex patches reuse the exact panorama pixels, keeping rail covers
        // registered without a duplicate bitmap or opaque rectangular cutout.
        private static HomeArtPart.Polygon Patch(params float[] xy)=>new HomeArtPart.Polygon{
            points=Enumerable.Range(0,xy.Length/2).Select(i=>new Vector2(xy[i*2]/2048f,xy[i*2+1]/683f)).ToArray()};
        private static readonly HomeArtPart.Polygon[] LowerStairCover={
            Patch(1133,0,1179,0,1490,307,1490,346),
            Patch(1473,312,1500,326,1500,382,1473,382),
            Patch(1468,350,1598,350,1598,376,1468,388),
            Patch(1235,0,1267,0,1501,207,1555,207,1568,229,1490,231),
            Patch(1539,223,1570,223,1570,353,1539,353)
        };
        private static readonly HomeArtPart.Polygon[] UpperStairCover={
            Patch(0,281,398,281,398,301,0,301),Patch(383,267,435,251,435,430,383,447),
            Patch(0,435,385,435,385,447,0,447),
            Patch(0,447,435,447,435,683,0,683),
            Patch(11,300,31,300,31,437,11,437),Patch(56,300,78,300,78,437,56,437),
            Patch(104,300,127,300,127,437,104,437),Patch(151,300,174,300,174,437,151,437),
            Patch(197,300,221,300,221,437,197,437),Patch(243,300,267,300,267,437,243,437),
            Patch(289,300,313,300,313,437,289,437),Patch(337,300,360,300,360,437,337,437)
        };
    }
}
