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
        private readonly Dictionary<int,(RectTransform root,Text caption)> bedroomDoors=new Dictionary<int,(RectTransform,Text)>();
        private RectTransform bedroomExit;
        private Text bedroomTitle;
        private string doorSource,doorTarget;
        private float doorDeadline;
        private bool doorApproach,doorSubmitted;
        private string DoorPreload=>doorTarget==HomeRooms.Landing?"home-upstairs":BedroomLayout.Index(doorTarget)>=0?"home-bedroom":null;
        private Vector2 DoorEntry=>new Vector2(BedroomLayout.DoorX(doorSource,doorTarget),BedroomLayout.DoorY);
        public BedroomState[] Bedrooms=>HasWorld?(Shared?shared.View.bedrooms:World.ReadBedrooms()):Array.Empty<BedroomState>();

        private void BuildBedrooms()
        {
            if(SceneSchema<BedroomLayout.Schema)return;
            var colors=new[]{new Color(.65f,.84f,.81f),new Color(.64f,.73f,.89f),new Color(.73f,.8f,.6f),new Color(.78f,.71f,.87f)};
            for(var i=0;i<4;i++)
            {
                var index=i;var root=Rect(Board,"Bedroom door "+(i+1),Vector2.zero,Vector2.zero);
                var hit=Plain(root,"Enter bedroom "+(i+1),new Vector2(0,170),new Vector2(195,350),Color.clear,true);
                hit.canvasRenderer.cullTransparentMesh=false;NavButton(hit,()=>RequestBedroom(BedroomLayout.Id(index)));
                var plaque=Panel(root,"Room marker",new Vector2(0,215),new Vector2(150,44),colors[i]);
                var label=Label(plaque.transform,"Room "+(i+1),20,Vector2.zero,new Vector2(150,42));
                bedroomDoors.Add(i,(root,label));
            }
            bedroomExit=Rect(Board,"Bedroom exit",Vector2.zero,Vector2.zero);
            var exit=Plain(bedroomExit,"Return to hallway",new Vector2(0,170),new Vector2(195,350),Color.clear,true);
            exit.canvasRenderer.cullTransparentMesh=false;NavButton(exit,()=>RequestBedroom(HomeRooms.Landing));
            Label(bedroomExit,"Hallway ←",22,new Vector2(0,-25),new Vector2(180,42));
            bedroomTitle=Label(Board,"",24,Vector2.zero,new Vector2(470,55));
        }
        public void RequestBedroom(string target)
        {
            if(!Ready || MenuOpen || TravelPending || WorldLoading || StairBusy || doorSubmitted ||
                !BedroomLayout.Route(CurrentArea,target))return;
            CancelStairApproach();doorSource=CurrentArea;doorTarget=target;doorApproach=true;
            doorDeadline=Time.realtimeSinceStartup+30;manualCamera=false;destination=DoorEntry;
        }
        private void CancelDoorApproach()
        {doorSource=doorTarget=null;doorApproach=doorSubmitted=false;}
        private void CheckDoorInput()
        {
            if(bedroomExit==null || !Ready)return;
            if(MenuOpen || TravelPending || WorldLoading || applicationPaused || Shared && !shared.Connected)
            {if(doorApproach){CancelDoorApproach();destination=null;}return;}
            if(doorSource!=null && doorSource!=CurrentArea)
            {CancelDoorApproach();destination=null;stickDirection=Vector2.zero;return;}
            if(!doorApproach || doorSubmitted)return;
            if(Time.realtimeSinceStartup>doorDeadline)
            {
                CancelDoorApproach();destination=null;homeFeedback.text="The room isn't ready yet. Try again.";homeFeedbackUntil=Time.unscaledTime+3;return;
            }
            var own=ReadPlayer(Actor);
            if(Vector2.Distance(new Vector2(own.x,own.y),DoorEntry)>12 || !scenicImages.ContainsKey(DoorPreload))return;
            doorApproach=false;doorSubmitted=true;destination=null;stickDirection=Vector2.zero;shared?.Walk(WalkMode.Stop);
            void Done(SoloResult result)
            {
                doorSubmitted=false;
                if(!result.Accepted){CancelDoorApproach();homeFeedback.text="Try the doorway again.";homeFeedbackUntil=Time.unscaledTime+3;}
                Render();
            }
            if(Shared)SubmitShared(SoloAction.EnterDoor,"",doorTarget,"",0,0,Done);
            else Done(Command(SoloAction.EnterDoor,target:doorTarget));
        }
        private void PresentBedrooms()
        {
            if(bedroomExit==null || !Ready)return;
            foreach(var pair in bedroomDoors)
            {
                var room=Bedrooms.First(r=>r.id==BedroomLayout.Id(pair.Key));
                pair.Value.root.gameObject.SetActive(CurrentArea==HomeRooms.Landing && !WorldLoading);
                pair.Value.root.anchoredPosition=ToBoard(BedroomLayout.HallX(pair.Key),BedroomLayout.DoorY);
                pair.Value.root.localScale=Vector3.one*sceneScale;
                pair.Value.caption.text=room.owner==Actor?"Your room":"Room "+(pair.Key+1);
            }
            var inside=BedroomLayout.Index(CurrentArea)>=0;
            bedroomExit.gameObject.SetActive(inside && !WorldLoading);bedroomTitle.gameObject.SetActive(inside && !WorldLoading);
            if(!inside)return;
            bedroomExit.anchoredPosition=ToBoard(BedroomLayout.ExitX,BedroomLayout.DoorY);bedroomExit.localScale=Vector3.one*sceneScale;
            var current=Bedrooms.First(r=>r.id==CurrentArea);
            bedroomTitle.text=current.owner==Actor?"Your bedroom":"Bedroom "+(BedroomLayout.Index(CurrentArea)+1);
            bedroomTitle.rectTransform.anchoredPosition=ToBoard(700,450)+new Vector2(0,210)*sceneScale;
            bedroomTitle.rectTransform.localScale=Vector3.one*sceneScale;
        }
        private void ResetBedrooms()
        {CancelDoorApproach();bedroomDoors.Clear();bedroomExit=null;bedroomTitle=null;}
    }
}
