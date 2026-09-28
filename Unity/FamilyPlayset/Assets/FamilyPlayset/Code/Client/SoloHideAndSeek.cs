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
        private readonly List<Sprite> hideSprites=new List<Sprite>();
        private Sprite[] banditFrames,hideCovers;
        private readonly List<RectTransform> hideProps=new List<RectTransform>();
        private readonly List<Image> hidePictures=new List<Image>();
        private readonly List<RectTransform> hideHits=new List<RectTransform>();
        private RectTransform banditRoot,hideHud,hideCard,hideCardFrame;
        private Image banditPicture;
        private Text hideCount;
        private Text hideStatus,hideExit,hideDone,hideStart;
        private int hideApproach=-1,lastHideCycle=-1,lastHideCount=-1;
        private HiderMode lastHideMode;
        private bool hideSending;
        private float banditX;
        private Texture2D banditTexture,hideTexture;
        private AudioSource hideChime;private AudioClip hideTick,hideFound;
        private readonly List<AudioClip> hideAudio=new List<AudioClip>();
        private HideState hideFrame;private int hideFrameNumber=-1;private long hideRevision=-1;
        public HideState HideGame {get {if(!HasWorld)return null;if(Shared)return shared.View.hideAndSeek;
            if(hideFrameNumber!=Time.frameCount || hideRevision!=World.Revision){hideFrame=World.ReadHideAndSeek();hideFrameNumber=Time.frameCount;hideRevision=World.Revision;}return hideFrame;}}
        private bool HideCardOpen=>hideCard!=null && hideCard.gameObject.activeSelf;
        private HiderState OwnHider=>HideAndSeek.Player(HideGame,Actor);
        private Vector2 HideEntry=>new Vector2(HideAndSeek.SlotX[Mathf.Clamp(hideApproach,0,5)],HideAndSeek.RailY);
        private static readonly float[] HidePropX={HideAndSeek.SlotX[0],HomeLayout.SofaX,-3495,-3150};
        private static readonly Vector2[] HideSizes={new Vector2(240,325),new Vector2(570,285),new Vector2(325,325),new Vector2(260,280)};
        private Sprite[] HideAtlas(Texture2D texture)
        {
            var sprites=new Sprite[6];var w=texture.width/3;var h=texture.height/2;
            for(var i=0;i<6;i++){sprites[i]=Sprite.Create(texture,new Rect(i%3*w,(1-i/3)*h,w,h),new Vector2(.5f,.5f));hideSprites.Add(sprites[i]);}return sprites;
        }
        private void BuildHideAndSeek()
        {
            if(SceneSchema<HideAndSeek.Schema)return;
            banditTexture=Resources.Load<Texture2D>("HideAndSeek/bandit");hideTexture=Resources.Load<Texture2D>("HideAndSeek/covers");
            banditFrames=HideAtlas(banditTexture);hideCovers=HideAtlas(hideTexture);
            for(var i=0;i<4;i++)
            {
                var root=Rect(Board,"Hiding "+new[]{"curtain","sofa nook","wardrobe","tent"}[i],Vector2.zero,Vector2.zero);hideProps.Add(root);
                hidePictures.Add(i==1?null:HomePicture(root,"Hiding cover",new Vector2(0,i==0?142:i==2?115:95),HideSizes[i],hideCovers[i==0?0:i==2?1:2]));
            }
            for(var slot=0;slot<6;slot++)
            {
                var index=slot;var prop=HideAndSeek.Props[slot];
                var hit=Panel(hideProps[prop],"Hide "+HideAndSeek.Names[slot],new Vector2(HideAndSeek.SlotX[slot]-HidePropX[prop],prop==1?170:30),new Vector2(prop==2?125:150,66),new Color(.93f,.98f,1,.9f),true);
                NavButton(hit,()=>RequestHide(index));
                var icon=HomePicture(hit.transform,"Pictured cover",new Vector2(-44,0),new Vector2(44,44),prop==1?homeObjects["Home sofa"].root.GetComponentInChildren<Image>().sprite:hideCovers[prop==0?0:prop==2?1:2]);
                Label(hit.transform,"Hide",23,new Vector2(22,0),new Vector2(80,54));hideHits.Add(hit.rectTransform);
            }
            banditRoot=Rect(Board,"Bandit seeker NPC",Vector2.zero,Vector2.zero);banditX=HideAndSeek.StartX;
            banditPicture=HomePicture(banditRoot,"Bandit pose",new Vector2(0,93),new Vector2(260,260),banditFrames[0]);
            HomeHit(banditRoot,"Bandit invitation tap",new Vector2(0,100),new Vector2(190,270),ShowHideCard,false);
            hideHud=Rect(safe,"Hide and seek controls",Vector2.zero,new Vector2(850,84));hideHud.anchorMin=hideHud.anchorMax=new Vector2(.5f,1);hideHud.anchoredPosition=new Vector2(0,-60);
            var card=Panel(hideHud,"Hide status card",new Vector2(0,0),new Vector2(370,76),new Color(1,.98f,.9f,.94f));
            hideStatus=Label(card.transform,"",24,new Vector2(37,0),new Vector2(270,70));
            hideCount=Label(card.transform,"",42,new Vector2(-134,0),new Vector2(90,70));hideCount.fontStyle=FontStyle.Bold;
            hideStart=Button(hideHud,"Hide & seek",Vector2.zero,new Vector2(265,78),ShowHideCard,new Color(.76f,.9f,.99f));
            HomePicture(hideStart.transform.parent,"Bandit button picture",new Vector2(-96,0),new Vector2(68,68),banditFrames[3]);hideStart.rectTransform.anchoredPosition=new Vector2(27,0);hideStart.rectTransform.sizeDelta=new Vector2(196,78);
            hideExit=Button(hideHud,"Come out",new Vector2(289,0),new Vector2(186,76),()=>{if(OwnHider?.mode==HiderMode.Found)SendHide("join");else if(OwnHider?.slot>=0)SendHide("out");},new Color(.75f,.9f,.99f));
            hideDone=Button(hideHud,"All done",new Vector2(-289,0),new Vector2(186,76),()=>SendHide("leave"),new Color(.97f,.88f,.72f));
            hideCard=Panel(safe,"Hide and seek invitation",Vector2.zero,Vector2.zero,new Color(.08f,.2f,.29f,.55f),true).rectTransform;Stretch(hideCard);
            hideCardFrame=Panel(hideCard,"Play card",Vector2.zero,new Vector2(820,510),new Color(1,.97f,.86f),true).rectTransform;
            HomePicture(hideCardFrame,"Bandit invitation",new Vector2(-235,15),new Vector2(280,280),banditFrames[5]);
            Label(hideCardFrame,"Hide & seek",46,new Vector2(115,164),new Vector2(520,76)).fontStyle=FontStyle.Bold;
            Label(hideCardFrame,"Bandit will count to 20.\nTap a hiding place!",30,new Vector2(115,55),new Vector2(510,115));
            for(var i=0;i<3;i++)HomePicture(hideCardFrame,"Hide choice picture "+i,new Vector2(0+i*116,-65),new Vector2(95,95),hideCovers[i]);
            Button(hideCardFrame,"Play with Bandit",new Vector2(105,-185),new Vector2(375,92),()=>{hideCard.gameObject.SetActive(false);SendHide("join");},new Color(.6f,.84f,.96f));
            Button(hideCardFrame,"Back",new Vector2(-261,-185),new Vector2(208,92),()=>hideCard.gameObject.SetActive(false),Color.white);
            hideCard.gameObject.SetActive(false);
            hideChime=gameObject.AddComponent<AudioSource>();hideChime.playOnAwake=false;hideChime.spatialBlend=0;
            hideTick=Resources.Load<AudioClip>("HideAndSeek/tick");hideFound=Resources.Load<AudioClip>("HideAndSeek/reveal");hideAudio.Add(hideTick);hideAudio.Add(hideFound);
            foreach(var id in new[]{"start","ready","found","count1","count2","count3","count4","count5"}){var clip=Resources.Load<AudioClip>("HideAndSeek/"+id);if(clip!=null){hideAudio.Add(clip);Narration.AddClip("hide-"+id,clip);}}
        }
        private void ShowHideCard()
        {
            if(!Ready || MenuOpen || TravelPending || !HideAndSeek.Zone(ReadPlayer(Actor)))return;
            CancelPointers();hideApproach=-1;hideCard.gameObject.SetActive(true);hideCard.SetAsLastSibling();
        }
        private void RequestHide(int slot)
        {
            if(!Ready || MenuOpen || hideSending)return;
            if(OwnHider==null || OwnHider.mode==HiderMode.Away || OwnHider.mode==HiderMode.Found){ShowHideCard();return;}
            hideApproach=slot;manualCamera=false;destination=HideEntry;
        }
        private void SendHide(string op,int slot=-1)
        {
            if(!Ready || hideSending)return;hideSending=true;
            void Done(SoloResult result){hideSending=false;if(result.Accepted && op=="join"){lastHideCycle=-1;Narration.Speak("hide-start");}else if(!result.Accepted){homeFeedback.text=result.Outcome=="hide-space-busy"?"Someone is there. Pick another hiding spot!":"Try that hiding spot again.";homeFeedbackUntil=Time.unscaledTime+3;}if(HasWorld)Render();}
            if(Shared)SubmitShared(SoloAction.HideAndSeek,"",slot.ToString(),op,0,0,Done);else Done(Command(SoloAction.HideAndSeek,target:slot.ToString(),value:op));
        }
        private void CheckHideInput()
        {
            if(hideApproach<0)return;
            if(!Ready || MenuOpen || TravelPending || applicationPaused || !HideAndSeek.Zone(ReadPlayer(Actor)) || stickDirection.sqrMagnitude>.1f){hideApproach=-1;destination=null;return;}
            var p=ReadPlayer(Actor);if(Math.Abs(p.x-HideEntry.x)>12 || Math.Abs(p.y-HideEntry.y)>12)return;
            var slot=hideApproach;hideApproach=-1;destination=null;shared?.Walk(WalkMode.Stop);SendHide("hide",slot);
        }
        private void PresentHideAndSeek()
        {
            var s=HideGame;if(s==null || banditRoot==null)return;var own=HideAndSeek.Player(s,Actor);var active=own.mode!=HiderMode.Away;
            var shown=CurrentArea=="garden";banditRoot.gameObject.SetActive(shown);
            banditRoot.Find("Bandit invitation tap").gameObject.SetActive(!active);
            var oldX=banditX;banditX=Mathf.MoveTowards(banditX,s.x,HideAndSeek.Speed*1.5f*Time.unscaledDeltaTime);if(Mathf.Abs(banditX-s.x)>800)banditX=s.x;
            banditRoot.anchoredPosition=ToBoard(banditX,HideAndSeek.RailY);banditRoot.localScale=Vector3.one*sceneScale;
            var frame=s.phase==HidePhase.Counting?3:s.phase==HidePhase.Inspecting?4:s.phase==HidePhase.Found?5:s.phase==HidePhase.Walking?1+(int)(s.clock*3)%2:0;
            banditPicture.sprite=banditFrames[frame];if(Mathf.Abs(banditX-oldX)>.001f)banditPicture.rectTransform.localScale=new Vector3(banditX<oldX?-1:1,1,1);
            var sofaMine=own.slot==1 || own.slot==2;
            var sofaRoot=homeObjects["Home sofa"].root;
            sofaRoot.GetComponentInChildren<Image>().color=sofaMine?new Color(1,1,1,.4f):Color.white;
            homeFronts["Home sofa"].GetComponentInChildren<HomeArtPart>().color=sofaMine?new Color(1,1,1,.4f):Color.white;
            for(var i=0;i<4;i++)
            {
                var root=hideProps[i];root.gameObject.SetActive(shown);root.anchoredPosition=ToBoard(HidePropX[i],i==1?HomeLayout.SofaY:245);root.localScale=Vector3.one*sceneScale;
                if(i==1)continue;
                var mine=own.slot>=0 && HideAndSeek.Props[own.slot]==i;
                var inspecting=s.target>=0 && HideAndSeek.Props[s.target]==i && s.phase==HidePhase.Inspecting && s.age>.7;
                var sprite=i==0?0:i==2?1:2;hidePictures[i].sprite=hideCovers[sprite+(mine || inspecting?3:0)];
                hidePictures[i].color=mine?new Color(1,1,1,.42f):Color.white;
            }
            foreach(var friend in friends){var p=ReadPlayer(friend.Key);friend.Value.root.gameObject.SetActive(p.zone==CurrentArea && shared.Players.Contains(friend.Key) && !HideAndSeek.Hidden(s,friend.Key));}
            if(own.mode==HiderMode.Hidden){avatar.anchoredPosition=ToBoard(HideAndSeek.SlotX[own.slot],320);characterVisual.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false),Time.unscaledDeltaTime);}
            for(var i=0;i<6;i++)hideHits[i].gameObject.SetActive(shown && active && own.mode!=HiderMode.Found && own.slot<0 && !MenuOpen);
            hideHud.anchoredPosition=new Vector2(0,active?-142:-60);
            var inZone=HideAndSeek.Zone(ReadPlayer(Actor));hideHud.gameObject.SetActive(inZone && !MenuOpen);
            hideStart.transform.parent.gameObject.SetActive(!active);hideStatus.transform.parent.gameObject.SetActive(active);
            hideExit.transform.parent.gameObject.SetActive(active);hideDone.transform.parent.gameObject.SetActive(active);
            hideExit.text=own.mode==HiderMode.Found?"Hide again":own.slot>=0?"Come out":"Pick a spot";
            var exitButton=hideExit.transform.parent.GetComponent<Button>();exitButton.interactable=own.mode==HiderMode.Found || own.slot>=0;
            var count=(int)Math.Ceiling(own.preparation);
            hideCount.text=count>0?count.ToString():own.mode==HiderMode.Found?"✓":"";
            hideStatus.text=own.mode==HiderMode.Found?"Found you!":count>0?(own.slot>=0?"You're hidden!":"Time to hide!"):own.slot>=0?"Shh... you're hidden!":"Tap a hiding spot";
            if(active && own.cycle==lastHideCycle && !applicationPaused && !MenuOpen && !BookSpeaking && (!Shared || shared.Connected))
            {
                if(own.mode==HiderMode.Found && lastHideMode!=HiderMode.Found){Narration.Speak("hide-found");PlayHideChime(hideFound);}
                else if(count!=lastHideCount && count<=5 && count>0 && !Narration.Speaking){Narration.Speak("hide-count"+count);PlayHideChime(hideTick);}
                else if(count==0 && lastHideCount==1 && !Narration.Speaking)Narration.Speak("hide-ready");
            }
            lastHideCycle=own.cycle;lastHideMode=own.mode;lastHideCount=count;
            if(HideCardOpen)hideCardFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/900,safe.rect.height/600);
            if(!inZone && HideCardOpen)hideCard.gameObject.SetActive(false);
            SortDepth();
        }
        private void PlayHideChime(AudioClip clip)
        {if(hideChime==null || clip==null || !Narration.VoiceEnabled || musicMuted || VerifyRun!=null || familyTestMuted || Shared && shared.MutedTest)return;hideChime.Stop();hideChime.clip=clip;hideChime.volume=.35f;hideChime.Play();}
        private void AddHideDepth(Action<RectTransform,float,int,string> add)
        {
            if(banditRoot==null)return;add(banditRoot,ToBoard(banditX,HideAndSeek.RailY).y,3,"bandit");
            for(var i=0;i<hideProps.Count;i++)add(hideProps[i],ToBoard(HidePropX[i],i==1?HomeLayout.SofaY:245).y,4,"hide-cover-"+i);
        }
        private void ResetHideAndSeek()
        {
            hideFrame=null;hideFrameNumber=-1;hideRevision=-1;hideApproach=-1;hideSending=false;lastHideCycle=-1;lastHideCount=-1;hideProps.Clear();hidePictures.Clear();hideHits.Clear();
            if(hideChime!=null){hideChime.Stop();Destroy(hideChime);hideChime=null;}
            foreach(var sprite in hideSprites)Destroy(sprite);hideSprites.Clear();
            foreach(var clip in hideAudio)Resources.UnloadAsset(clip);hideAudio.Clear();
            if(banditTexture!=null)Resources.UnloadAsset(banditTexture);if(hideTexture!=null)Resources.UnloadAsset(hideTexture);
            banditRoot=null;hideHud=null;hideCard=null;
        }
    }
}
