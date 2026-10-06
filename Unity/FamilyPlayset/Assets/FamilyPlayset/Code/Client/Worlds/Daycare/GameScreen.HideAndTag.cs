using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform clubHud,clubInvite,clubTeacher,clubActionRoot;
        private Text clubHint,clubInviteText,clubAction,clubCount;
        private TeacherWalkView clubTeacherWalk;
        private readonly NpcLocomotion clubTeacherMotion=new NpcLocomotion();
        private readonly List<(RectTransform root,GameCharacterVisual visual,NpcLocomotion motion,Text star)> clubFriends=new List<(RectTransform,GameCharacterVisual,NpcLocomotion,Text)>();
        private readonly List<(RectTransform root,Image picture,RectTransform hit)> clubCovers=new List<(RectTransform,Image,RectTransform)>();
        private readonly Dictionary<string,Text> clubHumanStars=new Dictionary<string,Text>();
        private bool clubSending,clubInviteTag;private int clubApproach=-1;
        public DaycarePlayState HideClub=>HasWorld?(Shared?shared.View.hideClub:World.ReadDaycarePlay(false)):null;
        public DaycarePlayState TagClub=>HasWorld?(Shared?shared.View.tagClub:World.ReadDaycarePlay(true)):null;
        private bool FollowingClubTeacher=>HasWorld && CurrentArea==DaycarePlay.HideZone && (HideClub?.phase==ClubPhase.Playing || HideClub?.phase==ClubPhase.Inspecting) && DaycarePlay.Member(HideClub,Actor)?.attending==true && !groundPan && (DaycarePlay.Member(HideClub,Actor).slot>=0 || DaycarePlay.Member(HideClub,Actor).found || !destination.HasValue && stickDirection.sqrMagnitude<.01f);
        private bool ClubTag=>CurrentArea==DaycarePlay.TagZone;
        private DaycarePlayState CurrentClub=>ClubTag?TagClub:HideClub;
        public string[] ClubNpcArt=>clubFriends.Select(n=>n.visual.CharacterId).ToArray();
        public Vector2[] ClubNpcPoints=>clubFriends.Select(n=>n.motion.Point).ToArray();
        public int[] ClubNpcFrames=>clubFriends.Select(n=>n.visual.ActiveView==null?-1:n.visual.ActiveView.FrameIndex).ToArray();
        public bool ClubTeacherWalking=>clubTeacherWalk!=null && clubTeacherWalk.Walking;
        public int ClubTeacherDrawing=>clubTeacherWalk==null?-1:clubTeacherWalk.Drawing;
        public Vector2 ClubTeacherPoint=>clubTeacherMotion.Point;
        private static readonly Color ClubInk=new Color(.15f,.25f,.29f);
        private void SendClub(bool tag,string op,string target="")
        {
            if(!Ready || TravelPending || clubSending)return;
            CloseMiniGames();CloseNavigation();CancelPointers();Narration.Stop();clubApproach=-1;destination=null;shared?.Walk(WalkMode.Stop);clubSending=true;
            var g=tag?TagClub:HideClub;if(target=="")target=(g?.round??0).ToString();
            void Done(SoloResult result){clubSending=false;if(!result.Accepted){homeFeedback.text=result.Outcome=="hiding-time-ended"?"Calypso is searching. Hide in the next round!":"That invitation has ended. Choose the game again.";homeFeedbackUntil=Time.unscaledTime+4;}Render();}
            if(Shared){if(!SubmitShared(SoloAction.DaycarePlay,"",target,(tag?"tag:":"hide:")+op,0,0,Done))clubSending=false;}
            else Done(Command(SoloAction.DaycarePlay,target:target,value:(tag?"tag:":"hide:")+op));
        }
        private void AddDaycarePlayChoices()
        {
            if(SceneSchema<DaycarePlay.Schema)return;
            foreach(var tag in new[]{false,true}){
                var chosenTag=tag;
                var label=MiniGameChoice("daycare",tag?"Tag with friends":"Hide & seek with Calypso",new Vector2(0,tag?-650:-550),new Vector2(700,90),()=>SendClub(chosenTag,"start"),tag?new Color(1,.87f,.61f):new Color(.76f,.9f,.99f));
                var root=(RectTransform)label.transform.parent;root.anchorMin=root.anchorMax=new Vector2(.5f,1);
                HomePicture(root,"Club game picture",new Vector2(-275,0),new Vector2(80,75),tag?banditFrames[1]:hideCovers[2]);
                label.rectTransform.anchoredPosition=new Vector2(55,0);label.rectTransform.sizeDelta=new Vector2(560,80);label.fontSize=30;label.color=ClubInk;
            }
        }
        private Sprite ClubCover(int slot,bool open=false)
        {var kind=new[]{0,1,2,3,1,0}[slot];return kind==0?hideCovers[2+(open?3:0)]:kind==1?wideCovers[2+(open?3:0)]:kind==2?wideCovers[open?3:0]:wideCovers[1+(open?3:0)];}
        private void BuildDaycarePlay()
        {
            if(SceneSchema<DaycarePlay.Schema)return;
            for(var i=0;i<6;i++){
                var slot=i;var root=Rect(Board,"Club cover "+i,Vector2.zero,Vector2.zero);
                var picture=HomePicture(root,"Club hiding cover",new Vector2(0,120),new Vector2(290,290),ClubCover(i));
                var label=Button(root,"Hide here "+(i+1),new Vector2(0,270),new Vector2(170,85),()=>RequestClubHide(slot),new Color(1,.98f,.87f));
                label.text="Hide";label.fontSize=30;label.color=ClubInk;var hit=(RectTransform)label.transform.parent;
                var glow=hit.gameObject.AddComponent<Outline>();glow.effectColor=new Color(1,.75f,.2f);glow.effectDistance=new Vector2(4,-4);
                clubCovers.Add((root,picture,hit));
            }
            for(var i=0;i<4;i++){
                var root=Rect(Board,"Club friend "+i,Vector2.zero,Vector2.zero);var visual=root.gameObject.AddComponent<GameCharacterVisual>();
                var star=Label(root,"★",50,new Vector2(0,300),new Vector2(100,70));star.color=new Color(.94f,.42f,.05f);
                clubFriends.Add((root,visual,new NpcLocomotion(),star));
            }
            foreach(var p in AllPlayersForTag()){
                var root=Rect(Board,"Club star "+p.id,Vector2.zero,new Vector2(100,70));var star=Label(root,"★",50,Vector2.zero,new Vector2(100,70));star.color=new Color(.94f,.42f,.05f);clubHumanStars[p.id]=star;
            }
            clubTeacher=Rect(Board,"Club Calypso",Vector2.zero,Vector2.zero);
            var drawing=Rect(clubTeacher,"Club Calypso drawing",Vector2.zero,Vector2.zero).gameObject.AddComponent<RawImage>();drawing.raycastTarget=false;
            clubTeacherWalk=clubTeacher.gameObject.AddComponent<TeacherWalkView>();clubTeacherWalk.Configure(drawing,calypsoTexture);
            clubHud=Panel(safe,"Daycare club instructions",Vector2.zero,new Vector2(1030,100),new Color(1,.97f,.86f),false).rectTransform;
            clubHud.anchorMin=clubHud.anchorMax=new Vector2(.5f,1);clubHud.anchoredPosition=new Vector2(0,-145);
            clubCount=Label(clubHud,"",52,new Vector2(-443,0),new Vector2(100,90));clubCount.color=ClubInk;clubCount.fontStyle=FontStyle.Bold;
            clubHint=Label(clubHud,"",27,new Vector2(-55,0),new Vector2(685,90));clubHint.color=ClubInk;
            var leave=Button(clubHud,"Back to Daycare",new Vector2(395,0),new Vector2(215,80),()=>SendClub(ClubTag,"leave"),new Color(.77f,.95f,.83f));leave.fontSize=26;leave.color=ClubInk;
            clubActionRoot=Rect(safe,"Club next action",Vector2.zero,new Vector2(350,85));clubActionRoot.anchorMin=clubActionRoot.anchorMax=new Vector2(.5f,0);clubActionRoot.anchoredPosition=new Vector2(0,85);
            clubAction=Button(clubActionRoot,"Come out",Vector2.zero,new Vector2(350,85),()=>SendClub(ClubTag,CurrentClub.phase==ClubPhase.Finished || ClubTag?"replay":"out"),new Color(1,.97f,.86f));clubAction.fontSize=30;clubAction.color=ClubInk;
            // A compact nonmodal card: siblings keep their Daycare position and
            // may keep playing, accept the invitation or dismiss it once.
            clubInvite=Panel(safe,"Daycare game invitation",Vector2.zero,new Vector2(630,215),new Color(1,.97f,.86f),true).rectTransform;
            clubInvite.anchorMin=clubInvite.anchorMax=new Vector2(1,0);clubInvite.pivot=new Vector2(1,0);clubInvite.anchoredPosition=new Vector2(-30,35);
            NavButton(clubInvite.GetComponent<Image>(),()=>{});
            clubInviteText=Label(clubInvite,"",29,new Vector2(0,48),new Vector2(590,100));clubInviteText.color=ClubInk;
            var join=Button(clubInvite,"Join friends",new Vector2(-153,-57),new Vector2(280,78),()=>SendClub(clubInviteTag,"join"),new Color(.77f,.95f,.83f));join.fontSize=29;join.color=ClubInk;
            var later=Button(clubInvite,"Not now",new Vector2(153,-57),new Vector2(280,78),()=>SendClub(clubInviteTag,"decline"),Color.white);later.fontSize=29;later.color=ClubInk;
            clubInvite.gameObject.SetActive(false);
        }
        private void RequestClubHide(int slot)
        {
            if(!Ready || MenuOpen || clubSending || CurrentArea!=DaycarePlay.HideZone || HideClub.phase!=ClubPhase.Counting)return;
            if(DaycarePlay.Member(HideClub,Actor)?.slot>=0)return;
            CancelPointers();clubApproach=slot;manualCamera=false;var p=DaycarePlay.Cover(slot);destination=new Vector2(p.X,90);
        }
        private void CheckDaycarePlayInput()
        {
            if(clubApproach<0)return;
            if(CurrentArea!=DaycarePlay.HideZone || HideClub.phase!=ClubPhase.Counting || MenuOpen || applicationPaused || stickDirection.sqrMagnitude>.1f){clubApproach=-1;destination=null;return;}
            var at=DaycarePlay.Cover(clubApproach);var p=ReadPlayer(Actor);if(Math.Abs(p.x-at.X)>14 || Math.Abs(p.y-90)>14)return;
            SendClub(false,"hide",clubApproach+"@"+HideClub.round);
        }
        private void PresentDaycarePlay()
        {
            if(clubHud==null)return;var tag=ClubTag;var g=CurrentClub;var inMap=DaycarePlay.Area(CurrentArea) && g!=null && g.round>0;
            if(HideClub!=null)ObserveDaycareReveals(HideClub);
            var own=DaycarePlay.Member(g,Actor);var playing=inMap && own?.attending==true;
            clubHud.gameObject.SetActive(playing && !MenuOpen);clubActionRoot.gameObject.SetActive(playing && !MenuOpen && (tag || g.phase==ClubPhase.Finished || own.slot>=0));
            var scale=Mathf.Min(1,safe.rect.width/1080);clubHud.localScale=clubActionRoot.localScale=Vector3.one*scale;
            if(playing){
                var counting=g.phase==ClubPhase.Counting;
                clubCount.text=counting?Math.Max(1,Math.Ceiling(g.deadline-g.clock)).ToString():tag?"★":g.phase==ClubPhase.Finished?"✓":"";
                var name=g.it==Actor?"You":g.it.StartsWith("club-npc-") && int.TryParse(g.it.Substring(9),out var index) && index<4?PlayableCharacters.Find(g.npcs[index].avatar)?.Name:"A friend";
                clubHint.text=tag?(counting?"Get ready! Chase the friend with the star.":g.it==Actor?"You have the star! Run up to a friend to tag them.":name+" has the star. Run and swap turns!"):
                    counting?"Calypso is counting. Tap a glowing hiding place!":g.phase==ClubPhase.Finished?"Calypso found everyone! Shall we hide again?":own.slot>=0?"Shh… Calypso is looking for our friends.":"Watch Calypso find our friends. Hide next round!";
                clubAction.text=tag?"New round":g.phase==ClubPhase.Finished?"Hide again":"Come out";
            }
            var dt=Mathf.Min(Time.unscaledDeltaTime,.1f);var key=(Shared?shared.View.worldId:World.WorldId)+"/"+CurrentArea+"/"+(g?.round??0);
            for(var i=0;i<6;i++){
                var cover=clubCovers[i];var show=inMap && !tag;cover.root.gameObject.SetActive(show);if(!show)continue;
                var at=DaycarePlay.Cover(i);cover.root.anchoredPosition=ToBoard(at.X,at.Y);cover.root.localScale=Vector3.one*sceneScale;
                var mine=own?.slot==i;var inspected=DaycareReveal.Any(r=>r.slot==i);
                cover.picture.sprite=ClubCover(i,mine || inspected);cover.picture.color=mine?new Color(1,1,1,.35f):Color.white;
                cover.hit.gameObject.SetActive(playing && g.phase==ClubPhase.Counting && own.slot<0 && !MenuOpen);
            }
            for(var i=0;i<4;i++){
                var friend=clubFriends[i];var exists=inMap && g.npcs.Length==4;var n=exists?g.npcs[i]:null;
                friend.root.gameObject.SetActive(exists && (!n.hidden || own?.slot==n.slot));if(!exists)continue;
                friend.visual.Select(n.avatar);var frame=friend.motion.Step(new Vector2(n.x,n.y),key+"/"+i,false,DaycarePlay.NpcSpeed*1.4f,dt);
                friend.root.anchoredPosition=ToBoard(friend.motion.Point.x,friend.motion.Point.y);friend.root.localScale=Vector3.one*sceneScale*.8f;
                if(!tag && n.hidden){frame=new CharacterFrame(CharacterPose.Sit,0,false);friend.root.anchoredPosition=ClubHiddenPoint(g,n.slot,DaycarePlay.NpcId(i));friend.root.localScale=Vector3.one*sceneScale*ClubHiddenScale(g,n.slot);}
                var reaction=!tag?DaycareReveal.FirstOrDefault(r=>r.actor==DaycarePlay.NpcId(i)):null;
                if(reaction!=null){frame=RevealFrame(reaction,false);friend.root.anchoredPosition=ToBoard(DaycarePlay.Cover(reaction.slot).X+(reaction.index-(reaction.count-1)*.5f)*68,n.y);if(reaction.count>3)friend.root.localScale=Vector3.one*sceneScale*.65f;}
                friend.visual.PresentNpcFrame(frame,dt,.8f);friend.star.gameObject.SetActive(tag && g.it==DaycarePlay.NpcId(i));
            }
            clubTeacher.gameObject.SetActive(inMap && !tag);
            if(inMap && !tag){var frame=clubTeacherMotion.Step(new Vector2(g.teacherX,g.teacherY),key+"/teacher",false,DaycarePlay.TeacherSpeed*1.4f,dt);clubTeacher.anchoredPosition=ToBoard(clubTeacherMotion.Point.x,clubTeacherMotion.Point.y);clubTeacher.localScale=Vector3.one*sceneScale*.9f;var greeting=DaycareReveal.Any(r=>Time.unscaledTime-r.start<.5f);clubTeacherWalk.Present(greeting?new CharacterFrame(CharacterPose.Wave,0,frame.FaceLeft):frame,greeting?1:g.phase==ClubPhase.Finished?3:g.phase==ClubPhase.Counting?1:0,dt,.9f);}
            foreach(var pair in clubHumanStars){var star=pair.Value;var show=inMap && tag && g.it==pair.Key;star.transform.parent.gameObject.SetActive(show);if(show){var body=pair.Key==Actor?avatar:friends.TryGetValue(pair.Key,out var f)?f.root:null;if(body!=null){var root=(RectTransform)star.transform.parent;root.anchoredPosition=body.anchoredPosition+new Vector2(0,290)*sceneScale;root.localScale=Vector3.one*sceneScale;}}}
            PresentClubHidden();
            if(!tag)PresentHumanReveals(DaycareReveal,true);
            var hideInvite=DaycarePlay.Member(HideClub,Actor)?.invited==true;var tagInvite=DaycarePlay.Member(TagClub,Actor)?.invited==true;
            var invited=CurrentArea=="daycare" && (hideInvite || tagInvite) && !MenuOpen && !applicationPaused;
            clubInvite.gameObject.SetActive(invited);clubInvite.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/900);
            if(invited){clubInviteTag=!hideInvite && tagInvite;clubInviteText.text=clubInviteTag?"Friends are playing Tag!\nJoin them and four classmates?":"Calypso is playing Hide & seek!\nJoin your friends?";clubInvite.SetAsLastSibling();}
            SortDepth();
        }
        private void PresentClubHidden()
        {
            if(!HasWorld || CurrentArea!=DaycarePlay.HideZone)return;
            var g=HideClub;var own=DaycarePlay.Member(g,Actor);var dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(g==null)return;
            {
                // The seeker never exposes unrelated hidden people; co-hiders
                // see through their own cover, as in the Home game.
                if(own?.slot>=0){avatar.anchoredPosition=ClubHiddenPoint(g,own.slot,Actor);avatar.localScale=Vector3.one*sceneScale*ClubHiddenScale(g,own.slot);characterVisual.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false),dt);}
                foreach(var f in friends){var h=DaycarePlay.Member(g,f.Key);var p=ReadPlayer(f.Key);var show=p.zone==CurrentArea && shared.Players.Contains(f.Key) && (h?.slot<0 || h==null || h.slot==own?.slot);f.Value.root.gameObject.SetActive(show);if(show && h?.slot>=0){f.Value.root.anchoredPosition=ClubHiddenPoint(g,h.slot,f.Key);f.Value.root.localScale=Vector3.one*sceneScale*ClubHiddenScale(g,h.slot);f.Value.view.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false),dt);}}
            }
        }
        private void AddDaycarePlayDepth(Action<RectTransform,float,int,string> add)
        {
            for(var i=0;i<clubCovers.Count;i++)add(clubCovers[i].root,clubCovers[i].root.anchoredPosition.y,2,"club-cover-"+i);
            for(var i=0;i<clubFriends.Count;i++)add(clubFriends[i].root,clubFriends[i].root.anchoredPosition.y,1,"club-npc-"+i);
            if(clubTeacher!=null)add(clubTeacher,clubTeacher.anchoredPosition.y,1,"club-teacher");
            foreach(var p in clubHumanStars)add((RectTransform)p.Value.transform.parent,-10000,3,"club-star-"+p.Key);
        }
        private void ResetDaycarePlay(){daycareReactions.Reset();clubHud=clubInvite=clubTeacher=clubActionRoot=null;clubTeacherWalk=null;clubCovers.Clear();clubFriends.Clear();clubHumanStars.Clear();clubSending=false;clubApproach=-1;clubTeacherMotion.Reset();}
    }
}
