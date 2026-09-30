using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform bathroomDoor,bathroomExit,bathroomHits,bathroomControls,bathWaterRoot;
        private HomeArtPart bathFront;
        private Text bathEnterLabel,bathHint;
        private readonly Image[] bathRipples=new Image[4];
        private readonly Image[] bathDrops=new Image[12];
        private Image sinkStream;
        private void BathroomUse(bool sink)
        {
            if(!Ready || CurrentArea!=BathroomLayout.Area || MenuOpen || WorldLoading)return;
            destination=null;CancelPointers();shared?.Walk(WalkMode.Stop);
            var slots=sink?BathroomLayout.SinkSlots:BathroomLayout.BathSlots;
            var players=Shared?shared.View.players:World.ReadPlayers();
            var own=ReadPlayer(Actor);
            if(slots.Contains(own.fixture)){HomeAction(SoloAction.LeaveFixture,own.fixture);return;}
            var free=slots.FirstOrDefault(id=>!players.Any(p=>p.fixture==id));
            if(free==null){homeFeedback.text="All four spots are busy.";homeFeedbackUntil=Time.unscaledTime+2;return;}
            HomeAction(SoloAction.UseFixture,free);
        }
        private void BuildBathroom()
        {
            if(SceneSchema<BathroomLayout.Schema)return;
            bathroomDoor=Rect(Board,"Bathroom doorway",Vector2.zero,Vector2.zero);
            HomeHit(bathroomDoor,"Enter bathroom",new Vector2(0,170),new Vector2(165,345),()=>RequestBedroom(BathroomLayout.Area));
            var plaque=Panel(bathroomDoor,"Bathroom marker",new Vector2(0,215),new Vector2(160,45),new Color(.72f,.9f,.9f));
            Label(plaque.transform,"Bathroom",19,Vector2.zero,new Vector2(154,42));
            bathroomExit=Rect(Board,"Bathroom hallway door",Vector2.zero,Vector2.zero);
            HomeHit(bathroomExit,"Leave bathroom",new Vector2(0,165),new Vector2(205,345),()=>RequestBedroom(HomeRooms.Landing));
            Label(bathroomExit,"Hallway ←",22,new Vector2(0,-25),new Vector2(185,40));
            bathroomHits=Rect(Board,"Bathroom fixture touches",Vector2.zero,Vector2.zero);
            HomeHit(bathroomHits,"Use bath",new Vector2(1190,340*.45f-250),new Vector2(680,150),()=>BathroomUse(false));
            HomeHit(bathroomHits,"Use sink",new Vector2(510,350*.45f-250),new Vector2(240,160),()=>BathroomUse(true));
            bathWaterRoot=Rect(Board,"Shared bath water",Vector2.zero,Vector2.zero);
            Panel(bathWaterRoot,"Bath water",new Vector2(1190,-118),new Vector2(587,22),new Color(.42f,.83f,.86f,.94f),false,true);
            for(var i=0;i<4;i++)
            {
                bathRipples[i]=Panel(bathWaterRoot,"Bath ripple "+i,Vector2.zero,new Vector2(55,12),new Color(.85f,1,1,.8f),false,true);
                for(var k=0;k<3;k++)bathDrops[i*3+k]=Panel(bathWaterRoot,"Splash "+i+" "+k,Vector2.zero,new Vector2(8,12),new Color(.48f,.86f,.95f),false,true);
            }
            sinkStream=Panel(bathWaterRoot,"Running sink water",new Vector2(509,-118),new Vector2(8,27),new Color(.47f,.84f,.94f,.85f));
            bathFront=Rect(Board,"Bath foreground",Vector2.zero,Vector2.zero).gameObject.AddComponent<HomeArtPart>();
            // Reuse the authored panorama pixels to hide bodies behind the bath,
            // keeping the rim, handles and water registered with the room art.
            bathFront.raycastTarget=false;
            bathroomControls=Panel(safe,"Bathroom play controls",Vector2.zero,new Vector2(720,112),Cream,true).rectTransform;
            bathHint=Label(bathroomControls,"Tap the bath or sink to play.",23,new Vector2(0,35),new Vector2(680,40));
            bathEnterLabel=Button(bathroomControls,"Bath",new Vector2(-225,-18),new Vector2(185,57),()=>BathroomUse(false),new Color(.71f,.89f,.96f));
            Button(bathroomControls,"Wash hands",new Vector2(-15,-18),new Vector2(220,57),()=>BathroomUse(true),new Color(.72f,.89f,.82f));
            Button(bathroomControls,"Splash",new Vector2(210,-18),new Vector2(180,57),()=>{
                if(BathroomLayout.Bath(ReadPlayer(Actor).fixture))HomeAction(SoloAction.SetFixture,"bath-splash");
                else BathroomUse(false);
            },new Color(.87f,.81f,.96f));
        }
        private void PresentBathroom()
        {
            if(bathroomDoor==null || !Ready)return;
            var active=!WorldLoading && !MenuOpen;
            bathroomDoor.gameObject.SetActive(CurrentArea==HomeRooms.Landing && active);
            bathroomDoor.anchoredPosition=ToBoard(BathroomLayout.HallX,BedroomLayout.DoorY);bathroomDoor.localScale=Vector3.one*sceneScale;
            var inside=CurrentArea==BathroomLayout.Area && active;
            bathroomExit.gameObject.SetActive(inside);bathroomHits.gameObject.SetActive(inside);bathroomControls.gameObject.SetActive(inside);
            bathWaterRoot.gameObject.SetActive(inside);bathFront.gameObject.SetActive(inside);
            if(!inside)return;
            bathroomExit.anchoredPosition=ToBoard(BathroomLayout.ExitX,BedroomLayout.DoorY);bathroomExit.localScale=Vector3.one*sceneScale;
            bathroomHits.anchoredPosition=new Vector2(-cameraX*sceneScale,0);bathroomHits.localScale=Vector3.one*sceneScale;
            bathWaterRoot.anchoredPosition=new Vector2(-cameraX*sceneScale,0);bathWaterRoot.localScale=Vector3.one*sceneScale;
            if(scenicImages.TryGetValue("home-bathroom",out var art))
            {
                bathFront.rectTransform.anchoredPosition=art.rectTransform.anchoredPosition;bathFront.rectTransform.sizeDelta=art.rectTransform.sizeDelta;
                bathFront.Configure((Texture2D)art.texture,new[]{new HomeArtPart.Polygon{points=new[]{new Vector2(.36f,.657f),new Vector2(.628f,.657f),new Vector2(.621f,.823f),new Vector2(.597f,.843f),new Vector2(.383f,.843f),new Vector2(.364f,.786f)}}});
            }
            bathroomControls.anchoredPosition=new Vector2(0,-safe.rect.height*.5f+70);
            bathroomControls.localScale=Vector3.one*Mathf.Min(1,(safe.rect.width-215)/720);
            var own=ReadPlayer(Actor);bathEnterLabel.text=BathroomLayout.Bath(own.fixture)?"Get out & dry":"Bath";
            bathHint.text=BathroomLayout.Bath(own.fixture)?"Splash together! Tap Get out & dry when you're ready.":BathroomLayout.Usable(own.fixture)?"Washing hands. Tap Wash hands to finish.":"Tap the bath or sink to play.";
            var players=Shared?shared.View.players:World.ReadPlayers();
            sinkStream.gameObject.SetActive(players.Any(p=>p.zone==BathroomLayout.Area && BathroomLayout.Usable(p.fixture) && !BathroomLayout.Bath(p.fixture)));
            for(var i=0;i<4;i++)
            {
                var p=players.FirstOrDefault(v=>v.zone==BathroomLayout.Area && v.fixture==BathroomLayout.BathSlots[i]);
                var phase=p==null?0:(float)(p.useSeconds%1.7);
                bathRipples[i].gameObject.SetActive(p!=null);
                bathRipples[i].rectTransform.anchoredPosition=new Vector2(BathroomLayout.X(BathroomLayout.BathSlots[i]),-117+Mathf.Sin(phase*4)*2);
                bathRipples[i].rectTransform.sizeDelta=new Vector2(45+phase*25,5);
                for(var k=0;k<3;k++)
                {
                    var g=bathDrops[i*3+k];g.gameObject.SetActive(p!=null && phase<.65f);
                    g.rectTransform.anchoredPosition=new Vector2(BathroomLayout.X(BathroomLayout.BathSlots[i])+(k-1)*(12+phase*20),-117+Mathf.Sin(phase/.65f*Mathf.PI)*(25+k*8));
                }
            }
        }
        private void AddBathroomDepth(Action<RectTransform,float,int,string> add)
        {
            if(CurrentArea!=BathroomLayout.Area || bathWaterRoot==null)return;
            add(bathWaterRoot,ToBoard(0,BathroomLayout.Y).y,0,"bath-water");
            add(bathFront.rectTransform,ToBoard(0,BathroomLayout.Y).y,2,"bath-front");
        }
        private void ResetBathroom()
        {bathroomDoor=bathroomExit=bathroomHits=bathroomControls=bathWaterRoot=null;bathFront=null;sinkStream=null;}
    }
}
