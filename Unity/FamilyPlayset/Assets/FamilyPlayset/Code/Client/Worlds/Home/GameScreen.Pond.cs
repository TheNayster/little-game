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
        private RectTransform pondRoot,pondControls,pondChoices,pondCloseup,pondCloseupFrame;
        private Texture2D pondTexture,pondFishTexture;
        private Sprite[] pondFishSprites;
        [Serializable] private sealed class FishLayout {public FishFrame[] frames;}
        [Serializable] private sealed class FishFrame {public float x,y,width,height;}
        private Sprite pondSprite;
        private PondDrawing pondWater,pondCascade;
        private Image pondCloseupFish;
        private readonly Image[] pondFish=new Image[PondFishing.FishCount];
        private readonly Dictionary<string,PondDrawing> pondLines=new Dictionary<string,PondDrawing>();
        private readonly Dictionary<string,Image> pondCatches=new Dictionary<string,Image>();
        private readonly List<PondDrawing> pondPellets=new List<PondDrawing>();
        private Text pondHint,pondCastText;
        private Button pondCastButton;
        private AudioSource pondAudio,pondBiteAudio;
        private int pondPreviousFish=-1;
        private bool pondSending;
        private double pondDisplayClock;
        private readonly BoatVisualClock pondClock=new BoatVisualClock();
        public double PondVisualClock=>pondDisplayClock;
        public double PondVisualTime=>pondClock.SampleTime;
        public PondState PondGame=>HasWorld?(Shared?shared.View.pond:World.ReadPond()):null;
        private bool PondCameraFollowing=>CurrentArea=="garden" && OwnPond!=null && OwnPond.mode!=PondMode.None;
        public bool PondCloseup=>pondCloseup!=null && pondCloseup.gameObject.activeSelf;
        public bool PondWaterPlaying=>pondAudio!=null && pondAudio.isPlaying && pondAudio.volume>0;
        private PondRod OwnPond=>PondGame?.rods.FirstOrDefault(r=>r.actor==Actor);
        private void EnsurePondFishArt()
        {
            if(pondFishSprites!=null)return;
            pondFishTexture=WorldResources.Load<Texture2D>("Worlds/Home/Pond/fish");
            var layout=JsonUtility.FromJson<FishLayout>(WorldResources.Load<TextAsset>("Worlds/Home/Pond/fish-layout").text);
            pondFishSprites=new Sprite[layout.frames.Length];
            for(var i=0;i<layout.frames.Length;i++){
                var f=layout.frames[i];pondFishSprites[i]=Sprite.Create(pondFishTexture,new Rect(f.x*pondFishTexture.width,f.y*pondFishTexture.height,f.width*pondFishTexture.width,f.height*pondFishTexture.height),new Vector2(.5f,.5f));
            }
        }
        private Image PondFishPicture(Transform parent,string name,Vector2 position,Vector2 size,int fish=0)
        {EnsurePondFishArt();var g=HomePicture(parent,name,position,size,pondFishSprites[fish]);g.preserveAspect=true;return g;}
        private PondDrawing PondPicture(Transform parent,string name,Vector2 position,Vector2 size,string kind)
        {
            var g=Rect(parent,name,position,size).gameObject.AddComponent<PondDrawing>();g.kind=kind;g.raycastTarget=false;
            if(kind.EndsWith("-icon"))PondFishPicture(g.transform,"Painted fish",new Vector2(size.x*.12f,-size.y*.23f),new Vector2(size.x*.58f,size.y*.57f),kind=="feeding-icon"?2:0);
            return g;
        }
        private void PondIcon(PondDrawing icon,string kind)
        {
            if(icon.kind==kind)return;icon.kind=kind;icon.SetVerticesDirty();
            icon.GetComponentInChildren<Image>().sprite=pondFishSprites[kind=="feeding-icon"?2:0];
        }
        private Text PondButton(Transform parent,string name,Vector2 pos,Vector2 size,string icon,Action click,Color fill)
        {
            var label=Button(parent,name,pos,size,()=>click(),fill);var root=(RectTransform)label.transform.parent;
            PondPicture(root,name+" picture",new Vector2(-size.x*.29f,0),new Vector2(85,64),icon);
            label.rectTransform.anchoredPosition=new Vector2(42,0);label.rectTransform.sizeDelta=new Vector2(size.x-100,size.y);label.fontSize=26;
            return label;
        }
        private void SendPond(string op,float x=PondFishing.X,float y=100)
        {
            if(pondSending || !Ready || TravelPending)return;
            destination=null;manualCamera=false;CancelPointers();pondSending=true;
            void Done(SoloResult result){pondSending=false;
                if(!result.Accepted){homeFeedback.text=result.Outcome=="pond-full"?"All four pond spots are busy.":"Try the pond again.";homeFeedbackUntil=Time.unscaledTime+2.5f;}
                else if(op.StartsWith("start-")){cameraX=PondFishing.X;cameraArea=null;cameraVisit=-1;}
                Render();}
            if(Shared){if(!SubmitShared(SoloAction.Pond,"","",op,x,y,Done))pondSending=false;}
            else Done(Command(SoloAction.Pond,value:op,x:x,y:y));
        }
        private void ChoosePondMiniGame(bool feed)
        {CloseMiniGames();CloseNavigation();SendPond(feed?"start-feeding":"start-fishing");}
        private void BuildPond()
        {
            if(SceneSchema<PondFishing.Schema)return;
            pondTexture=WorldResources.Load<Texture2D>("Worlds/Home/Pond/pond");
            if(pondTexture==null)throw new InvalidOperationException("Missing pond artwork.");
            pondSprite=Sprite.Create(pondTexture,new Rect(0,0,pondTexture.width,pondTexture.height),new Vector2(.5f,.5f));
            pondRoot=Rect(Board,"Backyard fish pond",Vector2.zero,Vector2.zero);
            HomePicture(pondRoot,"Stone pond and waterfall",new Vector2(0,65),new Vector2(1100,550),pondSprite);
            for(var i=0;i<pondFish.Length;i++)pondFish[i]=PondFishPicture(pondRoot,"Pond fish "+i,Vector2.zero,new Vector2(82,46),i);
            for(var i=0;i<PondFishing.FoodLimit;i++)pondPellets.Add(PondPicture(pondRoot,"Fish food "+i,Vector2.zero,new Vector2(40,20),"pellets"));
            pondWater=PondPicture(pondRoot,"Moving pond water",Vector2.zero,new Vector2(PondFishing.RadiusX*2,PondFishing.RadiusY*2),"water");
            pondWater.raycastTarget=true;pondWater.clicked=point=>{
                if(MenuOpen || pondSending || !Ready)return;
                if(!PondFishing.Target(point.x,point.y))return;
                var r=OwnPond;if(r==null || r.mode==PondMode.None){SendPond("start-fishing");return;}
                if(r.cast==PondCast.Caught)return;
                SendPond(r.mode==PondMode.Feeding?"feed":"cast",PondFishing.X+point.x,100+point.y);
            };
            pondCascade=PondPicture(pondRoot,"Flowing waterfall",new Vector2(35,100),new Vector2(40,73),"cascade");
            foreach(var p in (Shared?shared.View.players:World.ReadPlayers())){
                pondLines[p.id]=PondPicture(Board,"Fishing rod "+p.id,Vector2.zero,new Vector2(1,1),"rod");
                pondCatches[p.id]=PondFishPicture(Board,"Held pond fish "+p.id,Vector2.zero,new Vector2(110,62));
            }
            pondChoices=Rect(safe,"Pond activities",Vector2.zero,new Vector2(660,100));
            PondButton(pondChoices,"Fishing",new Vector2(-165,0),new Vector2(305,86),"fishing-icon",()=>SendPond("start-fishing"),new Color(.75f,.9f,.98f));
            PondButton(pondChoices,"Feed fish",new Vector2(165,0),new Vector2(305,86),"feeding-icon",()=>SendPond("start-feeding"),new Color(.83f,.91f,.62f));
            pondControls=Panel(safe,"Pond play controls",Vector2.zero,new Vector2(760,150),Cream,true).rectTransform;
            pondHint=Label(pondControls,"",28,new Vector2(0,48),new Vector2(705,50));
            pondCastText=PondButton(pondControls,"Cast line",new Vector2(-190,-27),new Vector2(270,76),"fishing-icon",()=>{
                var r=OwnPond;if(r==null)return;SendPond(r.cast==PondCast.Bite?"reel":r.mode==PondMode.Feeding?"feed":"cast",PondFishing.X+(r.slot-1.5f)*120,100);
            },new Color(.76f,.9f,.99f));pondCastButton=pondCastText.transform.parent.GetComponent<Button>();
            PondButton(pondControls,"Switch activity",new Vector2(103,-27),new Vector2(280,76),"feeding-icon",()=>SendPond(OwnPond.mode==PondMode.Feeding?"start-fishing":"start-feeding"),new Color(.84f,.92f,.66f));
            Button(pondControls,"Leave pond",new Vector2(305,-27),new Vector2(120,76),()=>SendPond("leave"),Color.white).fontSize=22;
            pondCloseup=Panel(safe,"Caught fish close-up",Vector2.zero,Vector2.zero,new Color(.08f,.24f,.3f,.5f),true).rectTransform;Stretch(pondCloseup);
            NavButton(pondCloseup.GetComponent<Image>(),()=>SendPond("release"));
            pondCloseupFrame=Panel(pondCloseup,"Look at your fish",Vector2.zero,new Vector2(690,480),Cream,true).rectTransform;
            NavButton(pondCloseupFrame.GetComponent<Image>(),()=>{});
            Label(pondCloseupFrame,"Look what you caught!",38,new Vector2(0,175),new Vector2(620,70));
            pondCloseupFish=PondFishPicture(pondCloseupFrame,"Your fish",new Vector2(0,25),new Vector2(440,240));
            Label(pondCloseupFrame,"Let's put it back in the pond.",27,new Vector2(0,-108),new Vector2(610,55));
            Button(pondCloseupFrame,"Release",new Vector2(0,-181),new Vector2(360,77),()=>SendPond("release"),new Color(.73f,.88f,.99f));
            pondCloseup.gameObject.SetActive(false);pondControls.gameObject.SetActive(false);
            pondAudio=gameObject.AddComponent<AudioSource>();pondAudio.clip=WorldResources.Load<AudioClip>("Worlds/Home/Pond/water");pondAudio.loop=true;pondAudio.playOnAwake=false;pondAudio.volume=0;pondAudio.Play();
            pondBiteAudio=gameObject.AddComponent<AudioSource>();pondBiteAudio.clip=WorldResources.Load<AudioClip>("Worlds/Home/Pond/bite");pondBiteAudio.playOnAwake=false;pondBiteAudio.volume=.18f;
            TickPond();
        }
        private void TickPond()
        {
            var p=PondGame;if(p==null || pondRoot==null)return;
            var worldId=Shared?shared.View.worldId:World.WorldId;
            // Routes already describe the whole swim. Full world snapshots only
            // arrive on changes, so a short prediction cap freezes valid routes.
            // Correct timing gradually; keep bites, catches and routes authoritative.
            pondDisplayClock=pondClock.Sample(worldId,p.clock,Time.realtimeSinceStartupAsDouble,Shared && !applicationPaused);
            var visible=CurrentArea=="garden";pondRoot.gameObject.SetActive(visible);
            pondRoot.anchoredPosition=ToBoard(PondFishing.X,PondFishing.WaterY)+Vector2.up*(PondFishing.WaterLift*sceneScale);pondRoot.localScale=Vector3.one*sceneScale;
            var clock=(float)pondDisplayClock;
            if(visible){pondWater.phase=clock;pondWater.SetVerticesDirty();pondCascade.phase=clock;pondCascade.SetVerticesDirty();}
            for(var i=0;i<pondFish.Length;i++){
                var f=p.fish[i];var caught=p.rods.Any(r=>r.fish==i && r.cast==PondCast.Caught);var pos=PondFishing.Point(f,pondDisplayClock);
                pondFish[i].gameObject.SetActive(!caught);pondFish[i].rectTransform.anchoredPosition=new Vector2(pos.X,pos.Y);
                pondFish[i].rectTransform.localScale=new Vector3(f.x<f.fromX?-1:1,1,1);
            }
            for(var i=0;i<pondPellets.Count;i++){
                var g=pondPellets[i];g.gameObject.SetActive(i<p.food.Length);if(i<p.food.Length){var food=p.food[i];g.rectTransform.anchoredPosition=new Vector2(food.x,food.y);g.color=new Color(1,1,1,Mathf.Clamp01((float)(food.until-pondDisplayClock)));}
            }
            foreach(var pair in pondLines){var r=p.rods.First(v=>v.actor==pair.Key);var player=ReadPlayer(pair.Key);var g=pair.Value;
                g.gameObject.SetActive(visible && r.mode==PondMode.Fishing && r.cast!=PondCast.Caught);
                if(!g.gameObject.activeSelf)continue;
                g.rectTransform.anchoredPosition=ToBoard(player.x,player.y);g.rectTransform.localScale=Vector3.one*sceneScale;
                var floatPoint=pondRoot.anchoredPosition/sceneScale+new Vector2(r.x,r.y)-g.rectTransform.anchoredPosition/sceneScale;
                if(r.cast==PondCast.Bite)floatPoint.y-=6+3*Mathf.Sin(clock*5);else floatPoint.y+=2*Mathf.Sin(clock*2);
                if(r.cast==PondCast.Ready)floatPoint=new Vector2(94,120);
                else if(r.cast==PondCast.Waiting){var t=Mathf.Clamp01((float)(pondDisplayClock-r.castAt)/.55f);floatPoint=Vector2.Lerp(new Vector2(75,181),floatPoint,t)+Vector2.up*(45*Mathf.Sin(t*Mathf.PI));}
                g.end=floatPoint;g.SetVerticesDirty();
            }
            var own=OwnPond;var playing=visible && own?.mode!=PondMode.None && own!=null;
            pondChoices.gameObject.SetActive(visible && Math.Abs(cameraX-PondFishing.X)<900 && !playing && !MenuOpen);
            pondChoices.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/720);pondChoices.anchoredPosition=new Vector2(0,-safe.rect.height/2+67);
            var caughtOwn=playing && own.cast==PondCast.Caught;
            pondCloseup.gameObject.SetActive(caughtOwn && !MiniGamesOpen && !(menu!=null && menu.activeSelf) && !CharactersOpen && !OutfitsOpen && !WorldLoading && !applicationPaused);
            if(PondCloseup){pondCloseup.SetAsLastSibling();pondCloseupFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/740,safe.rect.height/540);pondCloseupFish.sprite=pondFishSprites[own.fish];}
            pondControls.gameObject.SetActive(playing && !MenuOpen);
            pondControls.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/820);pondControls.anchoredPosition=new Vector2(0,-safe.rect.height/2+82);
            if(playing){
                var feeding=own.mode==PondMode.Feeding;var bite=own.cast==PondCast.Bite;
                pondHint.text=feeding?"Tap the water to sprinkle food.":bite?"A fish is biting! Tap Reel in.":own.cast==PondCast.Waiting || own.cast==PondCast.Approaching?"Watch your float. A fish will come!":"Tap the water or Cast line.";
                pondCastText.text=feeding?"Sprinkle food":bite?"Reel in":"Cast line";
                pondCastText.transform.parent.name=feeding?"Sprinkle food":bite?"Reel in":"Cast line";
                pondCastButton.interactable=!pondSending;
                var swap=pondControls.Find("Switch activity");var text=swap.GetComponentInChildren<Text>();text.text=feeding?"Fishing":"Feed fish";
                var icon=swap.GetComponentInChildren<PondDrawing>();var iconKind=feeding?"fishing-icon":"feeding-icon";PondIcon(icon,iconKind);
                PondIcon(pondCastButton.GetComponentInChildren<PondDrawing>(),feeding?"feeding-icon":"fishing-icon");
                if(bite && pondPreviousFish!=own.fish && !applicationPaused)pondBiteAudio.Play();pondPreviousFish=bite?own.fish:-1;
            }else pondPreviousFish=-1;
            foreach(var pair in pondCatches){var r=p.rods.First(v=>v.actor==pair.Key);var g=pair.Value;
                g.gameObject.SetActive(visible && r.cast==PondCast.Caught);if(!g.gameObject.activeSelf)continue;
                var player=ReadPlayer(pair.Key);g.rectTransform.anchoredPosition=ToBoard(player.x,player.y)+new Vector2(65,107)*sceneScale;g.rectTransform.localScale=Vector3.one*sceneScale;
                g.sprite=pondFishSprites[r.fish];
            }
            var near=visible?Mathf.Clamp01(1-Mathf.Abs(cameraX-PondFishing.X)/1600):0;
            pondAudio.volume=applicationPaused || musicMuted?0:.2f*near*ForegroundDucking;
        }
        private void AddPondDepth(Action<RectTransform,float,int,string> add)
        {
            if(pondRoot!=null)add(pondRoot,ToBoard(0,PondFishing.BankY).y,0,"pond");
            foreach(var pair in pondLines)add(pair.Value.rectTransform,ToBoard(0,PondFishing.BankY).y,4,"pond-rod-"+pair.Key);
            foreach(var pair in pondCatches)add(pair.Value.rectTransform,ToBoard(0,PondFishing.BankY).y,4,"pond-catch-"+pair.Key);
        }
        private void ResetPond()
        {
            if(pondFishSprites!=null)foreach(var fish in pondFishSprites)Destroy(fish);
            pondFishSprites=null;if(pondFishTexture!=null)Resources.UnloadAsset(pondFishTexture);pondFishTexture=null;
            if(pondSprite!=null)Destroy(pondSprite);if(pondTexture!=null)Resources.UnloadAsset(pondTexture);
            if(pondAudio!=null){pondAudio.Stop();Destroy(pondAudio);}if(pondBiteAudio!=null)Destroy(pondBiteAudio);
            pondSprite=null;pondTexture=null;pondRoot=null;pondControls=null;pondChoices=null;pondCloseup=null;pondLines.Clear();pondCatches.Clear();pondPellets.Clear();
            Array.Clear(pondFish,0,pondFish.Length);pondSending=false;pondClock.Reset();pondPreviousFish=-1;
        }
    }
}
