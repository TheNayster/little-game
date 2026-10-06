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
        private readonly List<Sprite> hideSprites=new List<Sprite>();
        private Sprite[] banditFrames,chilliFrames,hideCovers,wideCovers;
        private readonly List<RectTransform> hideProps=new List<RectTransform>();
        private readonly List<Image> hidePictures=new List<Image>();
        private readonly List<RectTransform> hideHits=new List<RectTransform>();
        private readonly List<Outline> hideGlows=new List<Outline>();
        private RectTransform banditRoot,hideHud,hideCard,hideCardFrame,hideBigCountRoot;
        private Image banditPicture,hideInvitePicture,hideParentPicture;
        private Text hideInviteText,hidePlay;
        private Text hideCount,hideBigCount;
        private Text hideStatus,hideExit,hideDone,hideBack,hideRequest;
        private RectTransform hideRequestRoot;
        private int hideCardRound=-1;
        private readonly List<Image> hideReadyPictures=new List<Image>();
        private int hideApproach=-1,lastHideCycle=-1,lastHideCount=-1;
        private HiderMode lastHideMode;
        private bool hideSending;
        private float banditX;
        private Texture2D banditTexture,chilliTexture,hideTexture,wideHideTexture;
        private AudioSource hideChime;private AudioClip hideTick,hideFound;
        private readonly List<AudioClip> hideAudio=new List<AudioClip>();
        private HideState hideFrame;private int hideFrameNumber=-1;private long hideRevision=-1;
        public HideState HideGame {get {if(!HasWorld)return null;if(Shared)return shared.View.hideAndSeek;
            if(hideFrameNumber!=Time.frameCount || hideRevision!=World.Revision){hideFrame=World.ReadHideAndSeek();hideFrameNumber=Time.frameCount;hideRevision=World.Revision;}return hideFrame;}}
        // The shared round stays on the authority, but every entry/prompt belongs to Home.
        private bool HideInHomeWorld=>HasWorld && MiniGamesWorld=="home";
        private bool HideCardOpen=>hideCard!=null && hideCard.gameObject.activeSelf;
        private HiderState OwnHider=>HideAndSeek.Player(HideGame,Actor);
        // Found players keep watching the shared search until its final reaction finishes.
        private bool FollowingHideParent=>HasWorld && CurrentArea=="garden" && HideGame.phase!=HidePhase.Idle &&
            (OwnHider?.mode==HiderMode.Hidden || OwnHider?.mode==HiderMode.Found);
        private Vector2 HideEntry=>new Vector2(HideAndSeek.CoverX(Mathf.Clamp(hideApproach,0,HideAndSeek.SlotX.Length-1),SceneSchema),HideAndSeek.RailY);
        private float[] HidePropX=>new[]{HideAndSeek.SlotX[0],HomeLayout.SofaX,-3495,-3150,-7040,Kitchen.DiningX,HideAndSeek.CoverX(8,SceneSchema),4310f};
        private static readonly Vector2[] HideSizes={new Vector2(240,325),new Vector2(570,285),new Vector2(325,325),new Vector2(260,280),new Vector2(300,300),new Vector2(900,300),new Vector2(350,300),new Vector2(350,310)};
        private static float HideGround(int prop)=>prop==1?HomeLayout.SofaY:prop==4 || prop==6?120:prop==5?130:prop==7?180:245;
        private Sprite HideCoverSprite(int prop,bool open=false)
        {
            if(prop==1)return homeObjects["Home sofa"].root.GetComponentInChildren<Image>().sprite;
            if(prop==5)return KitchenSprite("dining");
            if(prop>=4)return wideCovers[(prop==4?0:prop==6?1:2)+(open?3:0)];
            return hideCovers[(prop==0?0:prop==2?1:2)+(open?3:0)];
        }
        private Sprite[] HideAtlas(Texture2D texture)
        {
            var sprites=new Sprite[6];var w=texture.width/3;var h=texture.height/2;
            for(var i=0;i<6;i++){sprites[i]=Sprite.Create(texture,new Rect(i%3*w,(1-i/3)*h,w,h),new Vector2(.5f,.5f));hideSprites.Add(sprites[i]);}return sprites;
        }
        private void BuildHideAndSeek()
        {
            if(SceneSchema<HideAndSeek.ExpansionSchema)return;
            banditTexture=WorldResources.Load<Texture2D>("Worlds/Home/HideAndSeek/bandit");hideTexture=WorldResources.Load<Texture2D>("Worlds/Home/HideAndSeek/covers");
            chilliTexture=WorldResources.Load<Texture2D>("Worlds/Home/HideAndSeek/chilli");wideHideTexture=WorldResources.Load<Texture2D>("Worlds/Home/HideAndSeek/first-level-covers");
            banditFrames=HideAtlas(banditTexture);chilliFrames=HideAtlas(chilliTexture);hideCovers=HideAtlas(hideTexture);wideCovers=HideAtlas(wideHideTexture);
            for(var i=0;i<HidePropX.Length;i++)
            {
                var root=Rect(Board,"Hiding "+new[]{"curtain","sofa nook","wardrobe","tent","folding screen","dining nook","blanket bench","garden bush"}[i],Vector2.zero,Vector2.zero);hideProps.Add(root);
                hidePictures.Add(i==1 || i==5?null:HomePicture(root,"Hiding cover",new Vector2(0,i==0?142:i==2?115:i>=4?110:95),HideSizes[i],HideCoverSprite(i)));
            }
            for(var slot=0;slot<HideAndSeek.SlotX.Length;slot++)
            {
                var index=slot;var prop=HideAndSeek.Props[slot];
                var places=Enumerable.Range(0,HideAndSeek.SlotX.Length).Where(i=>HideAndSeek.Props[i]==prop).OrderBy(i=>HideAndSeek.CoverX(i,SceneSchema)).ToArray();
                var column=Array.IndexOf(places,slot)-1;
                var width=prop==1 || prop==5?145:92;
                var hit=Panel(hideProps[prop],"Hide "+HideAndSeek.Names[slot],new Vector2(column*(width+5),prop==1?185:prop==5?105:40),new Vector2(width,100),new Color(.93f,.98f,1,.97f),true);
                var glow=hit.gameObject.AddComponent<Outline>();glow.effectDistance=new Vector2(5,-5);hideGlows.Add(glow);
                NavButton(hit,()=>RequestHide(index));
                HomePicture(hit.transform,"Pictured cover",new Vector2(0,16),new Vector2(58,52),HideCoverSprite(prop));
                Label(hit.transform,"Hide",24,new Vector2(0,-27),new Vector2(width,38)).fontStyle=FontStyle.Bold;hideHits.Add(hit.rectTransform);
            }
            banditRoot=Rect(Board,"Bandit seeker NPC",Vector2.zero,Vector2.zero);banditX=HideAndSeek.StartX;
            banditPicture=HomePicture(banditRoot,"Bandit pose",new Vector2(0,93),new Vector2(260,260),banditFrames[0]);
            HomeHit(banditRoot,"Bandit invitation tap",new Vector2(0,100),new Vector2(190,270),ShowHideCard,false);
            hideHud=Rect(safe,"Hide and seek controls",Vector2.zero,new Vector2(850,84));hideHud.anchorMin=hideHud.anchorMax=new Vector2(.5f,1);hideHud.anchoredPosition=new Vector2(0,-60);
            var card=Panel(hideHud,"Hide status card",new Vector2(0,0),new Vector2(370,76),new Color(1,.98f,.9f,.94f));
            hideStatus=Label(card.transform,"",24,new Vector2(37,0),new Vector2(270,70));
            hideCount=Label(card.transform,"",42,new Vector2(-134,0),new Vector2(90,70));hideCount.fontStyle=FontStyle.Bold;
            hideParentPicture=HomePicture(card.transform,"Current parent",new Vector2(-134,0),new Vector2(70,70),banditFrames[0]);
            BuildMiniGames();
            hideExit=Button(hideHud,"Come out",new Vector2(289,0),new Vector2(186,76),()=>{if(OwnHider?.mode==HiderMode.Found && HideAndSeek.NextRound(HideGame))ShowHideCard();else if(OwnHider?.slot>=0)SendHide("out");},new Color(.75f,.9f,.99f));
            hideDone=Button(hideHud,"All done",new Vector2(-289,0),new Vector2(186,76),()=>SendHide("leave"),new Color(.97f,.88f,.72f));
            hideBigCountRoot=Rect(safe,"Big hide countdown",Vector2.zero,new Vector2(250,185));hideBigCountRoot.anchorMin=hideBigCountRoot.anchorMax=new Vector2(.5f,.60f);hideBigCountRoot.anchoredPosition=Vector2.zero;
            hideBigCount=Label(hideBigCountRoot,HideAndSeek.CountSeconds.ToString(),144,Vector2.zero,new Vector2(250,185));hideBigCount.fontStyle=FontStyle.Bold;
            var countOutline=hideBigCount.gameObject.AddComponent<Outline>();countOutline.effectColor=new Color(1,.98f,.87f,.95f);countOutline.effectDistance=new Vector2(3,-3);
            // Every participant sees the same server count; labels never block movement.
            hideBigCountRoot.gameObject.SetActive(false);
            hideCard=Panel(safe,"Hide and seek invitation",Vector2.zero,Vector2.zero,new Color(.08f,.2f,.29f,.55f),true).rectTransform;Stretch(hideCard);
            hideCardFrame=Panel(hideCard,"Play card",Vector2.zero,new Vector2(820,510),new Color(1,.97f,.86f),true).rectTransform;
            hideInvitePicture=HomePicture(hideCardFrame,"Parent invitation",new Vector2(-235,15),new Vector2(280,280),banditFrames[5]);
            Label(hideCardFrame,"Hide & seek",46,new Vector2(115,164),new Vector2(520,76)).fontStyle=FontStyle.Bold;
            hideInviteText=Label(hideCardFrame,"Bandit will count to "+HideAndSeek.CountSeconds+".\nTap a hiding place!",30,new Vector2(115,55),new Vector2(510,115));
            for(var i=0;i<4;i++)
            {var badge=Panel(hideCardFrame,"Ready player "+i,new Vector2(-30+i*100,-65),new Vector2(78,78),Color.white,false,true);hideReadyPictures.Add(badge);Label(badge.transform,(i+1).ToString(),34,Vector2.zero,new Vector2(70,70));}
            hidePlay=Button(hideCardFrame,"Play hide and seek",new Vector2(105,-185),new Vector2(375,92),HideCardAction,new Color(.6f,.84f,.96f));
            hideBack=Button(hideCardFrame,"Back",new Vector2(-261,-185),new Vector2(208,92),()=>{hideCard.gameObject.SetActive(false);if(OwnHider?.mode==HiderMode.Invited || OwnHider?.mode==HiderMode.Ready)SendHide("leave");},Color.white);
            hideCard.gameObject.SetActive(false);
            hideRequestRoot=Rect(safe,"Family hide invitation",Vector2.zero,new Vector2(540,84));hideRequestRoot.anchorMin=hideRequestRoot.anchorMax=new Vector2(.5f,1);hideRequestRoot.anchoredPosition=new Vector2(0,-65);
            hideRequest=Button(hideRequestRoot,"Join family hide and seek",Vector2.zero,new Vector2(520,84),ShowHideCard,new Color(.6f,.84f,.96f));
            HomePicture(hideRequest.transform.parent,"Invitation parent",new Vector2(-212,0),new Vector2(68,68),banditFrames[5]);hideRequest.rectTransform.anchoredPosition=new Vector2(36,0);hideRequest.rectTransform.sizeDelta=new Vector2(420,84);
            hideRequestRoot.gameObject.SetActive(false);
            hideChime=gameObject.AddComponent<AudioSource>();hideChime.playOnAwake=false;hideChime.spatialBlend=0;
            hideTick=WorldResources.Load<AudioClip>("Worlds/Home/HideAndSeek/tick");hideFound=WorldResources.Load<AudioClip>("Worlds/Home/HideAndSeek/reveal");hideAudio.Add(hideTick);hideAudio.Add(hideFound);
            foreach(var id in new[]{"start","ready","found","count1","count2","count3","count4","count5"}){var clip=WorldResources.Load<AudioClip>("Worlds/Home/HideAndSeek/"+id);if(clip!=null){hideAudio.Add(clip);Narration.AddClip("hide-"+id,clip);}}
        }
        private HiderState[] SharedHiders(HideState s,HiderState own)=>own?.mode==HiderMode.Hidden?
            s.hiders.Where(h=>h.mode==HiderMode.Hidden && HideAndSeek.SameCover(h.slot,own.slot)).OrderBy(h=>h.actor,StringComparer.Ordinal).ToArray():Array.Empty<HiderState>();
        private void PresentHiddenPlayers()
        {
            var s=HideGame;if(s==null)return;var own=HideAndSeek.Player(s,Actor);var together=SharedHiders(s,own);
            // Reliable replies also redraw friends before LateUpdate. Apply this
            // same cover rule there, so generic player rendering cannot expose
            // unrelated hiders or stack co-hiders on their authority coordinates.
            foreach(var friend in friends){var p=ReadPlayer(friend.Key);var sharedCover=together.Any(h=>h.actor==friend.Key);friend.Value.root.gameObject.SetActive(p.zone==CurrentArea && shared.Players.Contains(friend.Key) && (!HideAndSeek.Hidden(s,friend.Key) || sharedCover));}
            for(var i=0;i<together.Length;i++){
                var h=together[i];var prop=HideAndSeek.Props[h.slot];
                // Four seated drawings exceeded the small tent/curtain width.
                var crowded=together.Length==4 && HideSizes[prop].x<400;
                var gap=crowded?Mathf.Min(60,(HideSizes[prop].x-100)/3):70;
                var point=ToBoard(HidePropX[prop]+(i-(together.Length-1)*.5f)*gap,HideAndSeek.HiddenY(h.slot));
                if(h.actor==Actor){avatar.anchoredPosition=point;if(crowded)avatar.localScale=Vector3.one*sceneScale*.65f;characterVisual.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false),Time.unscaledDeltaTime);}
                else if(friends.TryGetValue(h.actor,out var friend)){friend.root.anchoredPosition=point;if(crowded)friend.root.localScale=Vector3.one*sceneScale*.65f;friend.view.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false),Time.unscaledDeltaTime);}
            }
        }
        private void ShowHideCard()
        {
            if(!Ready || TravelPending || WorldLoading)return;
            if(!HideInHomeWorld)return;
            CancelPointers();CloseBook();CloseDiscovery();CloseKitchen();CloseNavigation();if(bookLibrary!=null)bookLibrary.gameObject.SetActive(false);
            hideApproach=-1;hideCardRound=HideGame.round;hideCard.gameObject.SetActive(true);hideCard.SetAsLastSibling();
        }
        private void HideCardAction()
        {
            var s=HideGame;if(s==null || !HideInHomeWorld)return;
            if(s.phase==HidePhase.Counting)SendHide("join");
            else if(HideAndSeek.NextRound(s))SendHide("invite");
        }
        private void RequestHide(int slot)
        {
            if(!Ready || MenuOpen || hideSending)return;
            if(HideGame?.phase!=HidePhase.Counting){ShowHideCard();return;}
            hideApproach=slot;manualCamera=false;destination=HideEntry;
        }
        private void SendHide(string op,int slot=-1)
        {
            if(!Ready || hideSending || !HideInHomeWorld && op!="leave" && op!="out")return;hideSending=true;
            void Done(SoloResult result){hideSending=false;if(result.Accepted && (op=="invite" || op=="join")){hideCardRound=HideGame.round;hideCard.gameObject.SetActive(false);}else if(!result.Accepted){homeFeedback.text=result.Outcome=="hide-space-busy"?"That hiding area is full.":result.Outcome=="invitation-ended" || result.Outcome=="round-in-progress" || result.Outcome=="hiding-time-ended"?"This round has started. Join the next one!":"Please try again.";homeFeedbackUntil=Time.unscaledTime+3;}if(HasWorld)Render();}
            var target=(op=="join" || op=="start"?hideCardRound:slot).ToString();
            if(Shared){if(!SubmitShared(SoloAction.HideAndSeek,"",target,op,0,0,Done))hideSending=false;}else Done(Command(SoloAction.HideAndSeek,target:target,value:op));
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
            var s=HideGame;if(s==null || banditRoot==null)return;ObserveHomeReveals(s);var own=HideAndSeek.Player(s,Actor);var active=HideAndSeek.Playing(own);
            var counting=s.phase==HidePhase.Counting;var hiddenCount=s.hiders.Count(h=>h.mode==HiderMode.Hidden);
            var inHome=HideInHomeWorld;
            if(HideCardOpen && (!inHome || !counting && !HideAndSeek.NextRound(s)))hideCard.gameObject.SetActive(false);
            var shown=CurrentArea=="garden";banditRoot.gameObject.SetActive(shown);
            banditRoot.Find("Bandit invitation tap").gameObject.SetActive(!active);
            banditX=Mathf.MoveTowards(banditX,s.x,HideAndSeek.Speed*1.5f*Time.unscaledDeltaTime);if(Mathf.Abs(banditX-s.x)>800)banditX=s.x;
            banditRoot.anchoredPosition=ToBoard(banditX,HideAndSeek.RailY);banditRoot.localScale=Vector3.one*sceneScale;
            var frame=s.phase==HidePhase.Counting?3:s.phase==HidePhase.Inspecting?4:s.phase==HidePhase.Found?5:s.phase==HidePhase.Walking?1+(int)(s.clock*5)%2:0;
            var parent=HideAndSeek.Parent(s,s.phase==HidePhase.Idle);var frames=parent=="Chilli"?chilliFrames:banditFrames;
            if(HomeReveal.Length>0 && s.phase!=HidePhase.Walking)frame=5;
            banditPicture.sprite=frames[frame];banditPicture.rectTransform.localScale=new Vector3(HideAndSeek.Facing(s),1,1);
            var invitedParent=HideAndSeek.Parent(s,true);var invitationFrames=invitedParent=="Chilli"?chilliFrames:banditFrames;
            hideInvitePicture.sprite=invitationFrames[5];hideParentPicture.sprite=frames[0];
            PresentMiniGames(invitationFrames[5]);
            hideInviteText.text=counting?parent+" is counting!\nHide before zero to join.":
                HideAndSeek.NextRound(s)?invitedParent+" will count to 15.\nEveryone can hide to join!":parent+" is finding the hidden players.\nJoin the next round!";
            hidePlay.text=counting?"Go hide":HideAndSeek.NextRound(s)?"Start hide & seek":"Round in progress";
            hidePlay.transform.parent.GetComponent<Button>().interactable=!hideSending && (counting || HideAndSeek.NextRound(s));
            hideBack.text="Back";
            for(var i=0;i<hideReadyPictures.Count;i++)hideReadyPictures[i].color=i<hiddenCount?new Color(.58f,.89f,.66f):new Color(.82f,.84f,.84f);
            hideRequestRoot.gameObject.SetActive(inHome && counting && own.mode==HiderMode.Away && !MenuOpen && !applicationPaused);
            hideRequestRoot.anchoredPosition=new Vector2(0,HideAndSeek.Zone(ReadPlayer(Actor))?-150:-65);
            hideRequest.text="Hide before zero — Go hide!";
            if(hideRequestRoot.gameObject.activeSelf){hideRequestRoot.SetAsLastSibling();hideRequest.transform.parent.GetComponent<Button>().interactable=!hideSending;}
            var sofaMine=HideAndSeek.SameCover(own.slot,1) || HomeReveal.Any(r=>r.slot==1);
            var sofaRoot=homeObjects["Home sofa"].root;
            sofaRoot.GetComponentInChildren<Image>().color=sofaMine?new Color(1,1,1,.4f):Color.white;
            homeFronts["Home sofa"].GetComponentInChildren<HomeArtPart>().color=sofaMine?new Color(1,1,1,.4f):Color.white;
            for(var i=0;i<HidePropX.Length;i++)
            {
                var root=hideProps[i];root.gameObject.SetActive(shown);root.anchoredPosition=ToBoard(HidePropX[i],HideGround(i));root.localScale=Vector3.one*sceneScale;
                if(i==1 || i==5)continue;
                var mine=own.slot>=0 && HideAndSeek.Props[own.slot]==i;
                var inspecting=HomeReveal.Any(r=>r.slot==i);
                hidePictures[i].sprite=HideCoverSprite(i,mine || inspecting);
                hidePictures[i].color=mine?new Color(1,1,1,.42f):Color.white;
            }
            var diningMine=HideAndSeek.SameCover(own.slot,7) || HomeReveal.Any(r=>r.slot==5);
            kitchenFixtures["dining"].root.GetComponentInChildren<Image>().color=diningMine?new Color(1,1,1,.42f):Color.white;
            kitchenDiningFront.GetComponentInChildren<HomeArtPart>().color=diningMine?new Color(1,1,1,.42f):Color.white;
            PresentHiddenPlayers();
            PresentHumanReveals(HomeReveal,false);
            for(var i=0;i<HideAndSeek.SlotX.Length;i++)
            {
                var visible=shown && counting && own.slot<0 && !MenuOpen;hideHits[i].gameObject.SetActive(visible);
                if(visible)hideGlows[i].effectColor=new Color(1,.77f,.24f,quietStill ? .65f : .65f+.15f*Mathf.Sin(Time.unscaledTime*2));
            }
            hideHud.anchoredPosition=new Vector2(0,active?-142:-60);
            var inZone=HideAndSeek.Zone(ReadPlayer(Actor));hideHud.gameObject.SetActive(inZone && !MenuOpen && !hideRequestRoot.gameObject.activeSelf);
            hideHud.gameObject.SetActive(hideHud.gameObject.activeSelf && active);hideStatus.transform.parent.gameObject.SetActive(active);
            hideExit.transform.parent.gameObject.SetActive(active);hideDone.transform.parent.gameObject.SetActive(active);
            hideExit.text=own.mode==HiderMode.Found?(HideAndSeek.NextRound(s)?"Play together":"Friends hiding"):own.slot>=0?"Come out":"Pick a spot";
            var exitButton=hideExit.transform.parent.GetComponent<Button>();exitButton.interactable=own.mode==HiderMode.Found && HideAndSeek.NextRound(s) || own.slot>=0;
            var count=s.phase==HidePhase.Counting?(int)Math.Ceiling(s.count):0;
            hideBigCountRoot.gameObject.SetActive(inHome && count>0 && !WorldLoading && !applicationPaused && !HideCardOpen && !MiniGamesOpen);hideBigCount.text=count>0?count.ToString():"";
            hideCount.text=own.mode==HiderMode.Found?"✓":"";hideParentPicture.gameObject.SetActive(own.mode!=HiderMode.Found);
            hideStatus.text=own.mode==HiderMode.Found?"Found you!":count>0?(own.slot>=0?"You're hidden!":"Time to hide!"):own.slot>=0?(parent+(s.phase==HidePhase.Looking?" is looking around":" is searching")+"\nYou're hidden!"):"Tap a hiding spot";
            if(inHome && (active || counting) && own.cycle==lastHideCycle && !applicationPaused && !MenuOpen && !BookSpeaking && (!Shared || shared.Connected))
            {
                if(own.mode==HiderMode.Found && lastHideMode!=HiderMode.Found && HomeReveal.Any(r=>r.actor==Actor)){Narration.Speak("hide-found");PlayHideChime(hideFound);}
                else if(count!=lastHideCount && count<=5 && count>0 && !Narration.Speaking){Narration.Speak("hide-count"+count);PlayHideChime(hideTick);}
                else if(count==0 && lastHideCount==1 && !Narration.Speaking)Narration.Speak("hide-ready");
            }
            lastHideCycle=own.cycle;lastHideMode=own.mode;lastHideCount=count;
            if(HideCardOpen)hideCardFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/900,safe.rect.height/600);
            if(!inZone && HideCardOpen && !counting)hideCard.gameObject.SetActive(false);
            SortDepth();
        }
        private void PlayHideChime(AudioClip clip)
        {if(hideChime==null || clip==null || !Narration.VoiceEnabled || musicMuted || VerifyRun!=null || familyTestMuted || Shared && shared.MutedTest)return;hideChime.Stop();hideChime.clip=clip;hideChime.volume=.35f;hideChime.Play();}
        private void AddHideDepth(Action<RectTransform,float,int,string> add)
        {
            if(banditRoot==null)return;add(banditRoot,ToBoard(banditX,HideAndSeek.RailY).y,3,"bandit");
            for(var i=0;i<hideProps.Count;i++)add(hideProps[i],ToBoard(HidePropX[i],HideGround(i)).y,4,"hide-cover-"+i);
        }
        private void ResetHideAndSeek()
        {
            ResetMiniGames();homeReactions.Reset();
            hideFrame=null;hideFrameNumber=-1;hideRevision=-1;hideApproach=-1;hideSending=false;lastHideCycle=-1;lastHideCount=-1;hideCardRound=-1;hideReadyPictures.Clear();hideProps.Clear();hidePictures.Clear();hideHits.Clear();hideGlows.Clear();
            if(hideChime!=null){hideChime.Stop();Destroy(hideChime);hideChime=null;}
            foreach(var sprite in hideSprites)Destroy(sprite);hideSprites.Clear();
            foreach(var clip in hideAudio)Resources.UnloadAsset(clip);hideAudio.Clear();
            if(chilliTexture!=null)Resources.UnloadAsset(chilliTexture);if(wideHideTexture!=null)Resources.UnloadAsset(wideHideTexture);if(banditTexture!=null)Resources.UnloadAsset(banditTexture);if(hideTexture!=null)Resources.UnloadAsset(hideTexture);
            banditRoot=null;hideHud=null;hideCard=null;hideBigCountRoot=null;hideBigCount=null;
        }
    }
}
