using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform creekFishingRoot,creekFishingControls,creekFishingChoices,creekFishingCloseup,creekFishingCloseupFrame;
        private PondDrawing creekFishingWater;
        private Image creekFishingCloseupFish;
        private readonly Image[] creekFishingFish=new Image[CreekFishing.FishCount];
        private readonly Dictionary<string,PondDrawing> creekFishingLines=new Dictionary<string,PondDrawing>();
        private readonly Dictionary<string,Image> creekFishingCatches=new Dictionary<string,Image>();
        private readonly List<PondDrawing> creekFishingPellets=new List<PondDrawing>();
        private Text creekFishingHint,creekFishingCastText;
        private Button creekFishingCastButton;
        private AudioSource creekFishingAudio,creekFishingBiteAudio;
        private int creekFishingPreviousFish=-1;
        private bool creekFishingSending;
        private double creekFishingSampleClock=-1,creekFishingDisplayClock;
        private string creekFishingWorld="";
        public PondState CreekFishingGame=>HasWorld?(Shared?shared.View.creekFishing:World.ReadCreekFishing()):null;
        private bool CreekFishingCameraFollowing=>CurrentArea=="creek" && OwnCreekFishing!=null && OwnCreekFishing.mode!=PondMode.None;
        public bool CreekFishingCloseup=>creekFishingCloseup!=null && creekFishingCloseup.gameObject.activeSelf;
        public bool CreekFishingWaterPlaying=>creekFishingAudio!=null && creekFishingAudio.isPlaying && creekFishingAudio.volume>0;
        private PondRod OwnCreekFishing=>CreekFishingGame?.rods.FirstOrDefault(r=>r.actor==Actor);
        private void SendCreekFishing(string op,float x=CreekFishing.X,float y=100)
        {
            if(creekFishingSending || !Ready || TravelPending)return;
            destination=null;manualCamera=false;CancelPointers();creekFishingSending=true;
            void Done(SoloResult result){creekFishingSending=false;
                if(!result.Accepted){homeFeedback.text=result.Outcome=="pond-full"?"All four creek spots are busy.":"Try the creek again.";homeFeedbackUntil=Time.unscaledTime+2.5f;}
                else if(op.StartsWith("start-")){cameraX=CreekFishing.X;cameraArea=null;cameraVisit=-1;}
                Render();}
            if(Shared){if(!SubmitShared(SoloAction.CreekFishing,"","",op,x,y,Done))creekFishingSending=false;}
            else Done(Command(SoloAction.CreekFishing,value:op,x:x,y:y));
        }
        private void ChooseCreekFishingMiniGame(bool feed)
        {CloseMiniGames();CloseNavigation();SendCreekFishing(feed?"start-feeding":"start-fishing");}
        private void BuildCreekFishing()
        {
            if(SceneSchema<CreekFishing.Schema)return;
            creekFishingRoot=Rect(Board,"Creek fishing reach",Vector2.zero,Vector2.zero);
            for(var i=0;i<creekFishingFish.Length;i++)creekFishingFish[i]=PondFishPicture(creekFishingRoot,"Creek fish "+i,Vector2.zero,new Vector2(60,28),i%PondFishing.FishCount);
            for(var i=0;i<PondFishing.FoodLimit;i++)creekFishingPellets.Add(PondPicture(creekFishingRoot,"Fish food "+i,Vector2.zero,new Vector2(40,20),"pellets"));
            creekFishingWater=PondPicture(creekFishingRoot,"Creek water",Vector2.zero,new Vector2(CreekFishing.RadiusX*2,CreekFishing.RadiusY*2),"water");
            creekFishingWater.raycastTarget=true;creekFishingWater.clicked=point=>{
                if(MenuOpen || creekFishingSending || !Ready)return;
                if(!CreekFishing.Target(point.x,point.y))return;
                var r=OwnCreekFishing;if(r==null || r.mode==PondMode.None){SendCreekFishing("start-fishing");return;}
                if(r.cast==PondCast.Caught)return;
                SendCreekFishing(r.mode==PondMode.Feeding?"feed":"cast",CreekFishing.X+point.x,100+point.y);
            };

            foreach(var p in (Shared?shared.View.players:World.ReadPlayers())){
                creekFishingLines[p.id]=PondPicture(Board,"Creek fishing rod "+p.id,Vector2.zero,new Vector2(1,1),"rod");
                creekFishingCatches[p.id]=PondFishPicture(Board,"Held creek fish "+p.id,Vector2.zero,new Vector2(110,62));
            }
            creekFishingChoices=Rect(safe,"Creek bank activities",Vector2.zero,new Vector2(660,100));
            PondButton(creekFishingChoices,"Creek fishing",new Vector2(-165,0),new Vector2(305,86),"fishing-icon",()=>SendCreekFishing("start-fishing"),new Color(.75f,.9f,.98f));
            PondButton(creekFishingChoices,"Feed creek fish",new Vector2(165,0),new Vector2(305,86),"feeding-icon",()=>SendCreekFishing("start-feeding"),new Color(.83f,.91f,.62f));
            creekFishingControls=Panel(safe,"Creek fishing controls",Vector2.zero,new Vector2(760,150),Cream,true).rectTransform;
            creekFishingHint=Label(creekFishingControls,"",28,new Vector2(0,48),new Vector2(705,50));
            creekFishingCastText=PondButton(creekFishingControls,"Cast line",new Vector2(-190,-27),new Vector2(270,76),"fishing-icon",()=>{
                var r=OwnCreekFishing;if(r==null)return;SendCreekFishing(r.cast==PondCast.Bite?"reel":r.mode==PondMode.Feeding?"feed":"cast",CreekFishing.X+(r.slot-1.5f)*CreekFishing.Habitat.castSpacing,100);
            },new Color(.76f,.9f,.99f));creekFishingCastButton=creekFishingCastText.transform.parent.GetComponent<Button>();
            PondButton(creekFishingControls,"Switch activity",new Vector2(103,-27),new Vector2(280,76),"feeding-icon",()=>SendCreekFishing(OwnCreekFishing.mode==PondMode.Feeding?"start-fishing":"start-feeding"),new Color(.84f,.92f,.66f));
            Button(creekFishingControls,"Leave creek",new Vector2(305,-27),new Vector2(120,76),()=>SendCreekFishing("leave"),Color.white).fontSize=22;
            creekFishingCloseup=Panel(safe,"Caught fish close-up",Vector2.zero,Vector2.zero,new Color(.08f,.24f,.3f,.5f),true).rectTransform;Stretch(creekFishingCloseup);
            NavButton(creekFishingCloseup.GetComponent<Image>(),()=>SendCreekFishing("release"));
            creekFishingCloseupFrame=Panel(creekFishingCloseup,"Look at your fish",Vector2.zero,new Vector2(690,480),Cream,true).rectTransform;
            NavButton(creekFishingCloseupFrame.GetComponent<Image>(),()=>{});
            Label(creekFishingCloseupFrame,"Look what you caught!",38,new Vector2(0,175),new Vector2(620,70));
            creekFishingCloseupFish=PondFishPicture(creekFishingCloseupFrame,"Your fish",new Vector2(0,25),new Vector2(440,240));
            Label(creekFishingCloseupFrame,"Let's put it back in the creek.",27,new Vector2(0,-108),new Vector2(610,55));
            Button(creekFishingCloseupFrame,"Release",new Vector2(0,-181),new Vector2(360,77),()=>SendCreekFishing("release"),new Color(.73f,.88f,.99f));
            creekFishingCloseup.gameObject.SetActive(false);creekFishingControls.gameObject.SetActive(false);
            creekFishingAudio=gameObject.AddComponent<AudioSource>();creekFishingAudio.clip=WorldResources.Load<AudioClip>("Worlds/Home/Pond/water");creekFishingAudio.loop=true;creekFishingAudio.playOnAwake=false;creekFishingAudio.volume=0;creekFishingAudio.Play();
            creekFishingBiteAudio=gameObject.AddComponent<AudioSource>();creekFishingBiteAudio.clip=WorldResources.Load<AudioClip>("Worlds/Home/Pond/bite");creekFishingBiteAudio.playOnAwake=false;creekFishingBiteAudio.volume=.18f;
            TickCreekFishing();
        }
        private void TickCreekFishing()
        {
            var p=CreekFishingGame;if(p==null || creekFishingRoot==null)return;
            var worldId=Shared?shared.View.worldId:World.WorldId;
            if(creekFishingWorld!=worldId || p.clock<creekFishingSampleClock){creekFishingWorld=worldId;creekFishingDisplayClock=p.clock;}
            creekFishingSampleClock=p.clock;creekFishingDisplayClock=Math.Max(p.clock,Math.Min(p.clock+.12,creekFishingDisplayClock+Math.Min(.1,Time.unscaledDeltaTime)));
            var visible=CurrentArea=="creek";creekFishingRoot.gameObject.SetActive(visible);
            creekFishingRoot.anchoredPosition=new Vector2((CreekFishing.X-cameraX)*sceneScale,-102*sceneScale);creekFishingRoot.localScale=Vector3.one*sceneScale;
            var clock=(float)creekFishingDisplayClock;
            if(visible){creekFishingWater.phase=clock;creekFishingWater.SetVerticesDirty();}
            for(var i=0;i<creekFishingFish.Length;i++){
                var f=p.fish[i];var caught=p.rods.Any(r=>r.fish==i && r.cast==PondCast.Caught);var pos=PondFishing.Point(f,creekFishingDisplayClock);
                creekFishingFish[i].gameObject.SetActive(!caught);creekFishingFish[i].rectTransform.anchoredPosition=new Vector2(pos.X,pos.Y);
                creekFishingFish[i].color=new Color(.83f,.94f,.94f,.76f);
                creekFishingFish[i].rectTransform.localScale=new Vector3(f.x<f.fromX?-1:1,1,1);
            }
            for(var i=0;i<creekFishingPellets.Count;i++){
                var g=creekFishingPellets[i];g.gameObject.SetActive(i<p.food.Length);if(i<p.food.Length){var food=p.food[i];g.rectTransform.anchoredPosition=new Vector2(food.x,food.y);g.color=new Color(1,1,1,Mathf.Clamp01((float)(food.until-creekFishingDisplayClock)));}
            }
            foreach(var pair in creekFishingLines){var r=p.rods.First(v=>v.actor==pair.Key);var player=ReadPlayer(pair.Key);var g=pair.Value;
                g.gameObject.SetActive(visible && r.mode==PondMode.Fishing && r.cast!=PondCast.Caught);
                if(!g.gameObject.activeSelf)continue;
                g.rectTransform.anchoredPosition=ToBoard(player.x,player.y);g.rectTransform.localScale=Vector3.one*sceneScale;
                var floatPoint=creekFishingRoot.anchoredPosition/sceneScale+new Vector2(r.x,r.y)-g.rectTransform.anchoredPosition/sceneScale;
                if(r.cast==PondCast.Bite)floatPoint.y-=6+3*Mathf.Sin(clock*5);else floatPoint.y+=2*Mathf.Sin(clock*2);
                if(r.cast==PondCast.Ready)floatPoint=new Vector2(94,120);
                else if(r.cast==PondCast.Waiting){var t=Mathf.Clamp01((float)(creekFishingDisplayClock-r.castAt)/.55f);floatPoint=Vector2.Lerp(new Vector2(75,181),floatPoint,t)+Vector2.up*(45*Mathf.Sin(t*Mathf.PI));}
                g.end=floatPoint;g.SetVerticesDirty();
            }
            var own=OwnCreekFishing;var playing=visible && own?.mode!=PondMode.None && own!=null;
            creekFishingChoices.gameObject.SetActive(visible && Math.Abs(cameraX-CreekFishing.X)<900 && !playing && !MenuOpen);
            creekFishingChoices.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/720);creekFishingChoices.anchoredPosition=new Vector2(0,-safe.rect.height/2+67);
            var caughtOwn=playing && own.cast==PondCast.Caught;
            creekFishingCloseup.gameObject.SetActive(caughtOwn && !MiniGamesOpen && !(menu!=null && menu.activeSelf) && !CharactersOpen && !OutfitsOpen && !WorldLoading && !applicationPaused);
            if(CreekFishingCloseup){creekFishingCloseup.SetAsLastSibling();creekFishingCloseupFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/740,safe.rect.height/540);creekFishingCloseupFish.sprite=pondFishSprites[own.fish%pondFishSprites.Length];}
            creekFishingControls.gameObject.SetActive(playing && !MenuOpen);
            creekFishingControls.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/820);creekFishingControls.anchoredPosition=new Vector2(0,-safe.rect.height/2+82);
            if(playing){
                var feeding=own.mode==PondMode.Feeding;var bite=own.cast==PondCast.Bite;
                creekFishingHint.text=feeding?"Tap the water to sprinkle food.":bite?"A fish is biting! Tap Reel in.":own.cast==PondCast.Waiting || own.cast==PondCast.Approaching?"Watch your float. A fish will come!":"Tap the water or Cast line.";
                creekFishingCastText.text=feeding?"Sprinkle food":bite?"Reel in":"Cast line";
                creekFishingCastText.transform.parent.name=feeding?"Sprinkle food":bite?"Reel in":"Cast line";
                creekFishingCastButton.interactable=!creekFishingSending;
                var swap=creekFishingControls.Find("Switch activity");var text=swap.GetComponentInChildren<Text>();text.text=feeding?"Creek fishing":"Feed creek fish";
                var icon=swap.GetComponentInChildren<PondDrawing>();var iconKind=feeding?"fishing-icon":"feeding-icon";PondIcon(icon,iconKind);
                PondIcon(creekFishingCastButton.GetComponentInChildren<PondDrawing>(),feeding?"feeding-icon":"fishing-icon");
                if(bite && creekFishingPreviousFish!=own.fish && !applicationPaused)creekFishingBiteAudio.Play();creekFishingPreviousFish=bite?own.fish:-1;
            }else creekFishingPreviousFish=-1;
            foreach(var pair in creekFishingCatches){var r=p.rods.First(v=>v.actor==pair.Key);var g=pair.Value;
                g.gameObject.SetActive(visible && r.cast==PondCast.Caught);if(!g.gameObject.activeSelf)continue;
                var player=ReadPlayer(pair.Key);g.rectTransform.anchoredPosition=ToBoard(player.x,player.y)+new Vector2(65,107)*sceneScale;g.rectTransform.localScale=Vector3.one*sceneScale;
                g.sprite=pondFishSprites[r.fish%pondFishSprites.Length];
            }
            var near=visible?Mathf.Clamp01(1-Mathf.Abs(cameraX-CreekFishing.X)/1600):0;
            creekFishingAudio.volume=applicationPaused || musicMuted?0:.2f*near*ForegroundDucking;
        }
        private void AddCreekFishingDepth(Action<RectTransform,float,int,string> add)
        {
            if(creekFishingRoot!=null)add(creekFishingRoot,ToBoard(0,CreekFishing.BankY).y,0,"creekFishing");
            foreach(var pair in creekFishingLines)add(pair.Value.rectTransform,ToBoard(0,CreekFishing.BankY).y,4,"creekFishing-rod-"+pair.Key);
            foreach(var pair in creekFishingCatches)add(pair.Value.rectTransform,ToBoard(0,CreekFishing.BankY).y,4,"creekFishing-catch-"+pair.Key);
        }
        private void ResetCreekFishing()
        {
            if(creekFishingAudio!=null){creekFishingAudio.Stop();Destroy(creekFishingAudio);}if(creekFishingBiteAudio!=null)Destroy(creekFishingBiteAudio);
            creekFishingRoot=null;creekFishingControls=null;creekFishingChoices=null;creekFishingCloseup=null;creekFishingLines.Clear();creekFishingCatches.Clear();creekFishingPellets.Clear();
            Array.Clear(creekFishingFish,0,creekFishingFish.Length);creekFishingSending=false;creekFishingWorld="";creekFishingSampleClock=-1;creekFishingPreviousFish=-1;
        }
    }
}
