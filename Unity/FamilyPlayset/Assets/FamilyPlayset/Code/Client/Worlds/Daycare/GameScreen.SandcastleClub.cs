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
        private RectTransform sandpitRoot,sandBuildView,sandFloor;
        private readonly List<SandShape> sandTipCues=new List<SandShape>();
        private readonly List<Image> sandHighlights=new List<Image>();private readonly List<SandShape> sandScoopEffects=new List<SandShape>();
        private SandScoopFeedback sandFeedback=new SandScoopFeedback();
        public int SandpitHighlight=>sandHighlights.FindIndex(h=>h.gameObject.activeSelf);
        public int SandpitApproach=>sandpitApproach;public int SandpitApproachRound=>sandpitApproachRound;
        public int[] SandpitScoopEvents=>sandFeedback.Events;
        private readonly List<RectTransform> sandPlaces=new List<RectTransform>();private readonly List<SandShape> sandShapes=new List<SandShape>();
        private readonly List<(RectTransform root,GameCharacterVisual visual)> sandFriends=new List<(RectTransform,GameCharacterVisual)>();
        private readonly List<Button> sandTools=new List<Button>();private readonly List<AudioClip> sandAudio=new List<AudioClip>();
        private readonly List<Text> sandNumbers=new List<Text>();private Text sandHint;
        private TeacherWalkView sandHelpTeacher;
        private bool sandpitSending;private int sandpitSelected=-1,sandpitApproach=-1,sandpitApproachRound=-1,sandpitScoopTaps,sandSpokenRound=-1;
        private string sandpitOperation,sandIntentId,sandSelectedId,sandFlightId,sandFlightOp;private int sandFlightScoops;private Button sandConfirm,sandCancel,sandNew;
        private readonly List<Button> sandCells=new List<Button>();
        private readonly List<(RectTransform root,GameCharacterVisual visual)> sandBuilders=new List<(RectTransform,GameCharacterVisual)>();
        private SandShape sandPreview;private bool sandPlacing;private int sandPreviewCell=-1,sandPreviewEpoch=-1;private float sandAck;
        public SandpitState SandpitGame=>HasWorld?(Shared?shared.View.sandpit:World.ReadSandpit()):null;
        public string[] SandpitNpcArt=>sandFriends.Select(n=>n.visual.CharacterId).ToArray();public int SandpitSelection=>sandpitSelected;
        private bool SandpitOwn=>SandpitGame?.members.Any(m=>m.actor==Actor && m.attending)==true;
        private bool SandpitTeacherActive=>SandpitGame?.phase>0 && SandpitGame.members.Any(m=>m.attending);
        // A close work surface, independent of world camera/pan. World coordinates
        // still own placement, approach and reach; this is only their UI projection.
        private Vector2 SandPoint(float x,float y)=>new Vector2((x-4445)*1.34f,(y-295)*.84f-5);
        private SandMould SandSelectedPiece=>SandpitGame?.moulds.FirstOrDefault(m=>m.id==sandSelectedId);
        private void ChooseSandpit()
        {CloseMiniGames();CloseNavigation();CloseTeacherCard();Narration.Stop();SendSandpit("start");}
        private void SandCue(string outcome)
        {
            if(sandHint==null || outcome==null)return;
            sandHint.text=outcome=="add-water-first"?"Water":outcome=="fill-bucket-first"?"Scoop":outcome=="sandpit-full"?"Full — your castle stays!":outcome=="sand-spot-taken"?"Choose a free spot":outcome=="outside-sandpit"?"Choose inside the sand":"Try again";
            sandAck=Time.unscaledTime+2;
        }
        private void SendSandpit(string op,int index=-1,int expectedRound=-1)
        {
            if(!Ready || TravelPending || sandpitSending && op!="leave")return;
            var g=SandpitGame;var id=index<0?"":sandIntentId ?? g.moulds[index].id;
            var target=index<0?"":DaycareSandpit.Target(id,expectedRound);
            sandpitSending=true;sandFlightId=id;sandFlightOp=op;sandFlightScoops=index>=0?g.moulds[index].scoops:0;CancelSandpitIntent();destination=null;CancelPointers();shared?.Walk(WalkMode.Stop);
            void Done(SoloResult r){
                sandpitSending=false;sandFlightId=sandFlightOp=null;
                if(!r.Accepted && (expectedRound<0 || SandpitOwn && SandpitGame.round==expectedRound)){
                    if(op=="tip" && (r.Outcome=="fill-bucket-first" || r.Outcome=="add-water-first"))sandTipFeedback.Cue(index,expectedRound,r.Outcome=="add-water-first"?"dry":"underfilled");
                    SandCue(r.Outcome);
                }
                Render();
            }
            if(Shared){if(!SubmitShared(SoloAction.Sandpit,"",target,op,0,0,Done))sandpitSending=false;}
            else Done(Command(SoloAction.Sandpit,target:target,value:op));
        }
        private void SelectSandMould(int i)
        {
            if(!SandpitOwn){ChooseSandpit();return;}
            if(i<0 || i>=SandpitGame.moulds.Length)return;
            CancelSandpitIntent();sandPlacing=false;sandpitSelected=i;sandSelectedId=SandpitGame.moulds[i].id;CancelPointers();TickSandpit();
        }
        private void BeginSandPlacement()
        {
            if(!Ready || !SandpitOwn || sandpitSending)return;
            CancelSandpitIntent();CancelPointers();
            if(SandpitGame.moulds.Length>=SandpitGame.pieceLimit){SandCue("sandpit-full");return;}
            sandPlacing=true;sandPreviewCell=-1;sandPreviewEpoch=SandpitGame.round;TickSandpit();
        }
        private void PreviewSandCell(int cell)
        {if(sandPlacing){sandPreviewCell=cell;SandCue(DaycareSandpit.Placement(SandpitGame,DaycareSandpit.Cell(cell%8,cell/8).X,DaycareSandpit.Cell(cell%8,cell/8).Y));TickSandpit();}}
        private void ConfirmSandPlacement()
        {
            if(!sandPlacing || sandPreviewCell<0 || sandpitSending || sandPreviewEpoch!=SandpitGame.round)return;
            var at=DaycareSandpit.Cell(sandPreviewCell%8,sandPreviewCell/8);var reason=DaycareSandpit.Placement(SandpitGame,at.X,at.Y);
            if(reason!=null){SandCue(reason);return;}
            // Captured position and epoch travel unchanged through revision retries.
            var epoch=sandPreviewEpoch;sandpitSending=true;sandAck=Time.unscaledTime+1;
            void Done(SoloResult r){sandpitSending=false;if(SandpitOwn && SandpitGame.round==epoch){
                if(r.Accepted){var i=Array.FindIndex(SandpitGame.moulds,m=>m.x==at.X && m.y==at.Y);sandPlacing=false;if(i>=0)SelectSandMould(i);}
                else SandCue(r.Outcome);
            }Render();}
            if(Shared){if(!SubmitShared(SoloAction.Sandpit,"round",DaycareSandpit.Target("place",epoch),"place",at.X,at.Y,Done))sandpitSending=false;}
            else Done(Command(SoloAction.Sandpit,item:"round",target:DaycareSandpit.Target("place",epoch),value:"place",x:at.X,y:at.Y));
        }
        private void RequestSandTool(string op)
        {
            var m=SandSelectedPiece;
            if(!Ready || MenuOpen || !SandpitOwn || sandPlacing || m==null || m.built)return;
            var useful=DaycareSandpit.UsefulScoops(m,int.MaxValue)-(sandpitSending && sandFlightId==m.id && sandFlightOp=="scoop" && m.scoops<=sandFlightScoops?1:0);
            sandAck=Time.unscaledTime+.25f;
            if(op=="scoop" && useful==0){SandCue("bucket-full");return;}
            if(op=="scoop" && sandpitOperation==op && sandIntentId==m.id && sandpitApproachRound==SandpitGame.round){
                sandpitScoopTaps=Math.Min(useful,sandpitScoopTaps+1);return;
            }
            CancelSandpitIntent();CancelPointers();sandpitApproach=Array.FindIndex(SandpitGame.moulds,p=>p.id==m.id);sandIntentId=m.id;sandpitApproachRound=SandpitGame.round;sandpitScoopTaps=op=="scoop"?1:0;sandpitOperation=op;
            var at=DaycareSandpit.Work(m);destination=new Vector2(at.X,at.Y);
        }
        private void CancelSandpitIntent()
        {
            var m=SandpitGame?.moulds.FirstOrDefault(p=>p.id==sandIntentId);
            var at=m==null?default:DaycareSandpit.Work(m);
            var ownsWalk=m!=null && destination.HasValue && destination.Value==new Vector2(at.X,at.Y);
            sandpitApproach=-1;sandpitApproachRound=-1;sandpitScoopTaps=0;sandpitOperation=sandIntentId=null;
            if(ownsWalk){destination=null;shared?.Walk(WalkMode.Stop);}
        }
        private void CheckSandpitInput()
        {
            if(sandpitApproach<0)return;
            var g=SandpitGame;var m=g?.moulds.FirstOrDefault(p=>p.id==sandIntentId);
            if(g==null || m==null || sandpitApproachRound!=g.round || CurrentArea!="daycare" || !SandpitOwn || Shared && !shared.Connected || MenuOpen || applicationPaused || stickDirection.sqrMagnitude>.1f){CancelSandpitIntent();return;}
            sandpitScoopTaps=DaycareSandpit.UsefulScoops(m,sandpitScoopTaps);
            if(m.built || sandpitOperation=="scoop" && sandpitScoopTaps==0){CancelSandpitIntent();return;}
            if(sandpitSending || ActionPending)return;
            var at=DaycareSandpit.Work(m);var p=ReadPlayer(Actor);
            if(Math.Abs(p.x-at.X)<=15 && Math.Abs(p.y-at.Y)<=25){
                var op=sandpitOperation;var index=Array.IndexOf(g.moulds,m);var round=sandpitApproachRound;var id=m.id;var remaining=op=="scoop"?Math.Max(0,sandpitScoopTaps-1):0;
                SendSandpit(op,index,round);
                if(remaining>0){sandpitApproach=index;sandpitApproachRound=round;sandpitOperation=op;sandIntentId=id;sandpitScoopTaps=remaining;}
            }
        }
        private SandShape DrawSandBucket(Transform parent,Vector2 at,int scoops,int capacity,bool wet)
        {var root=Rect(parent,"Sand bucket picture",at,new Vector2(110,170));var shape=root.gameObject.AddComponent<SandShape>();shape.raycastTarget=false;shape.scoops=scoops;shape.capacity=capacity;shape.wet=wet;return shape;}
        private Button SandPictureButton(Transform parent,string title,string kind,Vector2 at,Action action)
        {
            var label=Button(parent,title,at,new Vector2(166,100),()=>action(),Color.white);label.fontSize=22;label.rectTransform.anchoredPosition=new Vector2(0,-33);label.rectTransform.sizeDelta=new Vector2(166,30);
            var icon=Rect(label.transform.parent,"Picture "+title,new Vector2(0,10),new Vector2(100,65)).gameObject.AddComponent<SandShape>();icon.kind=kind;icon.raycastTarget=false;
            if(kind=="bucket"){icon.rectTransform.localScale=Vector3.one*.65f;icon.rectTransform.anchoredPosition=new Vector2(0,-12);}
            return label.transform.parent.GetComponent<Button>();
        }
        private void SandControlPicture(Transform parent,string kind)
        {var icon=Rect(parent,"Picture "+kind,Vector2.zero,new Vector2(100,70)).gameObject.AddComponent<SandShape>();icon.kind=kind;icon.raycastTarget=false;}
        private void SandTeacherHelp()
        {var m=SandSelectedPiece;Narration.Speak(m!=null && m.scoops==m.capacity && m.wet?"sand-tip":"sand-water");}
        private void BuildSandpit()
        {
            if(SceneSchema<DaycareSandpit.PieceSchema)return;
            // Keep a world entry point; the activity view owns large taps above it.
            sandpitRoot=Rect(Board,"Playable Daycare sandpit",Vector2.zero,new Vector2(780,170));var pit=sandpitRoot.gameObject.AddComponent<SandShape>();pit.kind="pit";pit.raycastTarget=false;
            HomeHit(sandpitRoot,"Play sandpit",new Vector2(0,-30),new Vector2(750,65),ChooseSandpit,false);
            sandBuildView=Panel(safe,"Sandcastle shared building view",Vector2.zero,new Vector2(1210,740),new Color(1,.97f,.88f),true).rectTransform;
            sandFloor=Panel(sandBuildView,"Our shared sand",new Vector2(0,32),new Vector2(1100,460),new Color(.96f,.82f,.52f),true).rectTransform;
            sandHint=Label(sandBuildView,"",24,new Vector2(0,324),new Vector2(720,45));
            var leave=Button(sandBuildView,"",new Vector2(526,322),new Vector2(105,72),()=>{sandPlacing=false;CancelSandpitIntent();Narration.Stop();SendSandpit("leave");},new Color(.8f,.91f,.9f));leave.transform.parent.name="Leave sandpit";SandControlPicture(leave.transform.parent,"back");
            var help=Button(sandBuildView,"?",new Vector2(-526,322),new Vector2(105,72),SandTeacherHelp,new Color(.82f,.91f,1));help.fontSize=40;help.transform.parent.name="Optional teacher help";
            help.rectTransform.anchoredPosition=new Vector2(28,0);help.rectTransform.sizeDelta=new Vector2(40,65);
            var teacherDrawing=Rect(help.transform.parent,"Calypso help picture",new Vector2(-18,0),new Vector2(42,58)).gameObject.AddComponent<RawImage>();teacherDrawing.raycastTarget=false;sandHelpTeacher=teacherDrawing.gameObject.AddComponent<TeacherWalkView>();sandHelpTeacher.Configure(teacherDrawing,calypsoTexture);
            // Existing picture hits, no new drag or input framework. Each grid cell
            // is 114×90 logical points; its rectangle never follows the finger.
            for(var c=0;c<32;c++){var cell=c;var at=DaycareSandpit.Cell(c%8,c/8);var label=Button(sandFloor,"+",SandPoint(at.X,at.Y),new Vector2(110,88),()=>PreviewSandCell(cell),new Color(1,1,.88f,.65f));label.fontSize=36;label.transform.parent.name="Sand spot "+c;sandCells.Add(label.transform.parent.GetComponent<Button>());}
            for(var i=0;i<DaycareSandpit.MaxPieces;i++){
                var index=i;var bucket=DrawSandBucket(sandFloor,Vector2.zero,0,3,false);sandShapes.Add(bucket);sandPlaces.Add(bucket.rectTransform);
                var highlight=Panel(bucket.transform,"Your selected sand mould",new Vector2(0,55),new Vector2(145,170),new Color(1,.7f,.05f),false);highlight.sprite=hintRing;highlight.raycastTarget=false;highlight.transform.SetAsFirstSibling();sandHighlights.Add(highlight);
                var cue=Rect(bucket.transform,"Next action picture",new Vector2(60,90),new Vector2(65,65)).gameObject.AddComponent<SandShape>();cue.kind="scoop";cue.raycastTarget=false;sandTipCues.Add(cue);
                var effect=Rect(sandFloor,"Accepted sand scoop "+(i+1),Vector2.zero,new Vector2(1400,500)).gameObject.AddComponent<SandShape>();effect.kind="sand-transfer";effect.raycastTarget=false;sandScoopEffects.Add(effect);
                sandNumbers.Add(Label(bucket.transform,"",22,new Vector2(0,-20),new Vector2(150,38)));
                HomeHit(bucket.transform,"Sand piece "+(i+1),new Vector2(0,52),new Vector2(150,165),()=>TouchSandMould(index),false);
            }
            BuildSandWater();BuildSandTip();
            sandPreview=DrawSandBucket(sandFloor,Vector2.zero,0,3,false);sandPreview.color=new Color(1,1,1,.45f);
            var confirmLabel=Button(sandBuildView,"",new Vector2(140,-302),new Vector2(166,100),ConfirmSandPlacement,new Color(.72f,.93f,.84f));sandConfirm=confirmLabel.transform.parent.GetComponent<Button>();sandConfirm.name="Confirm sand placement";SandControlPicture(sandConfirm.transform,"confirm");
            var cancelLabel=Button(sandBuildView,"",new Vector2(330,-302),new Vector2(166,100),()=>{sandPlacing=false;sandPreviewCell=-1;TickSandpit();},Color.white);sandCancel=cancelLabel.transform.parent.GetComponent<Button>();sandCancel.name="Cancel sand placement";SandControlPicture(sandCancel.transform,"cancel");
            sandNew=SandPictureButton(sandBuildView,"Build","bucket",new Vector2(-310,-302),BeginSandPlacement);
            var operations=new[]{"scoop","water","tip"};var names=new[]{"Scoop","Water","Tip"};
            for(var i=0;i<3;i++){var op=operations[i];sandTools.Add(SandPictureButton(sandBuildView,names[i],op,new Vector2(-120+i*190,-302),()=>RequestSandTool(op)));}
            // Actual family avatars live outside the selectable sand surface.
            for(var i=0;i<4;i++){var root=Rect(sandBuildView,"Family builder "+i,new Vector2(-430+i*78,-230),new Vector2(52,72));sandBuilders.Add((root,root.gameObject.AddComponent<GameCharacterVisual>()));}
            foreach(var id in new[]{"water","tip"}){var clip=WorldResources.Load<AudioClip>("Worlds/Daycare/SandcastleClub/"+id);if(clip!=null){sandAudio.Add(clip);Narration.AddClip("sand-"+id,clip);}}
            TickSandpit();
        }
        private void TickSandpit()
        {
            if(sandpitRoot==null)return;var g=SandpitGame;var visible=CurrentArea=="daycare";var joined=visible && SandpitOwn && !MenuOpen && (!Shared || shared.Connected);
            sandpitRoot.gameObject.SetActive(visible);sandpitRoot.anchoredPosition=ToBoard(4460,440);sandpitRoot.localScale=Vector3.one*sceneScale;
            sandBuildView.gameObject.SetActive(joined);sandBuildView.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/1240,safe.rect.height/770);
            if(!joined){CancelSandpitIntent();sandPlacing=false;sandSpokenRound=-1;}
            if(joined && sandSpokenRound!=g.round){sandSpokenRound=g.round;CancelSandpitIntent();sandPlacing=false;sandSelectedId=g.moulds.FirstOrDefault(m=>!m.built)?.id ?? g.moulds.FirstOrDefault()?.id;}
            sandpitSelected=Array.FindIndex(g.moulds,m=>m.id==sandSelectedId);
            sandFeedback.Observe(g,joined && !applicationPaused,Time.unscaledDeltaTime);
            for(var i=0;i<DaycareSandpit.MaxPieces;i++){
                var active=i<g.moulds.Length;var shape=sandShapes[i];shape.gameObject.SetActive(active);
                if(!active){sandScoopEffects[i].gameObject.SetActive(false);continue;}
                var m=g.moulds[i];shape.rectTransform.anchoredPosition=SandPoint(m.x,m.y);shape.rectTransform.localScale=Vector3.one*.55f;shape.scoops=m.scoops;shape.capacity=m.capacity;shape.wet=m.wet;shape.built=m.built;shape.decoration=m.decoration;shape.SetVerticesDirty();
                sandNumbers[i].text="";sandHighlights[i].gameObject.SetActive(joined && !sandPlacing && i==sandpitSelected);
                var cue=sandTipCues[i];cue.gameObject.SetActive(joined && !sandPlacing && i==sandpitSelected && !m.built);cue.kind=m.scoops<m.capacity?"scoop":!m.wet?"water":"tip";cue.SetVerticesDirty();
                var effect=sandScoopEffects[i];effect.gameObject.SetActive(joined && sandFeedback.Remaining(i)>0);effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one*.55f;
                effect.source=new Vector2(-70,-25);effect.target=new Vector2(0,m.capacity==3?75:55);effect.progress=1-sandFeedback.Remaining(i)/SandScoopFeedback.Duration;effect.SetVerticesDirty();
            }
            TickSandWater(g,joined);TickSandTip(g,joined);
            if(!joined)return;
            if(sandPreviewEpoch!=g.round)sandPlacing=false;
            for(var c=0;c<sandCells.Count;c++){var at=DaycareSandpit.Cell(c%8,c/8);var free=DaycareSandpit.Placement(g,at.X,at.Y)==null;sandCells[c].gameObject.SetActive(sandPlacing);sandCells[c].interactable=free && !sandpitSending;}
            sandPreview.gameObject.SetActive(sandPlacing && sandPreviewCell>=0);
            if(sandPreviewCell>=0){var at=DaycareSandpit.Cell(sandPreviewCell%8,sandPreviewCell/8);sandPreview.rectTransform.anchoredPosition=SandPoint(at.X,at.Y);sandPreview.rectTransform.localScale=Vector3.one*.55f;sandPreview.color=DaycareSandpit.Placement(g,at.X,at.Y)==null?new Color(1,1,1,.5f):new Color(1,.4f,.3f,.65f);sandPreview.SetVerticesDirty();}
            sandConfirm.gameObject.SetActive(sandPlacing);sandCancel.gameObject.SetActive(sandPlacing);sandConfirm.interactable=sandPreviewCell>=0 && !sandpitSending && DaycareSandpit.Placement(g,DaycareSandpit.Cell(sandPreviewCell%8,sandPreviewCell/8).X,DaycareSandpit.Cell(sandPreviewCell%8,sandPreviewCell/8).Y)==null;
            sandNew.interactable=!sandpitSending && g.moulds.Length<g.pieceLimit;
            var selected=SandSelectedPiece;var next=selected==null || selected.built?-1:selected.scoops<selected.capacity?0:!selected.wet?1:2;
            for(var i=0;i<3;i++){sandTools[i].gameObject.SetActive(!sandPlacing);sandTools[i].interactable=selected!=null && !selected.built;sandTools[i].GetComponent<Image>().color=i==next?new Color(.72f,.93f,.84f):Color.white;sandTools[i].transform.localScale=Vector3.one*(Time.unscaledTime<sandAck && sandpitOperation==new[]{"scoop","water","tip"}[i]?1.06f:1);}
            if(Time.unscaledTime>=sandAck)sandHint.text=g.moulds.Length>=g.pieceLimit?"Full · "+g.moulds.Length+"/"+g.pieceLimit:sandPlacing?"Choose a spot":selected==null?"Build":selected.built?"Build again":selected.scoops+" / "+selected.capacity+(selected.wet?" · Wet":"");
            for(var i=0;i<sandBuilders.Count;i++){var member=i<g.members.Length?g.members[i]:null;var n=sandBuilders[i];n.root.gameObject.SetActive(member?.attending==true);if(member?.attending==true){n.visual.Select(ReadPlayer(member.actor).avatar);n.visual.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false,(float)calypsoClock),Mathf.Min(Time.unscaledDeltaTime,.1f));}}
            sandBuildView.SetAsLastSibling();
            sandHelpTeacher.Present(new CharacterFrame(CharacterPose.Sit,0,false,(float)calypsoClock),1,Mathf.Min(Time.unscaledDeltaTime,.1f),1);
        }
        private void AddSandpitDepth(Action<RectTransform,float,int,string> add)
        {if(sandpitRoot!=null && sandpitRoot.gameObject.activeInHierarchy)add(sandpitRoot,sandpitRoot.anchoredPosition.y+95*sceneScale,0,"sandpit");}
        private void ResetSandpit()
        {
            ResetSandWater();ResetSandTip();ResetSandDecoration();sandpitRoot=sandBuildView=sandFloor=null;sandHelpTeacher=null;sandPlaces.Clear();sandShapes.Clear();sandHighlights.Clear();sandTipCues.Clear();sandScoopEffects.Clear();sandFeedback=new SandScoopFeedback();sandFriends.Clear();sandBuilders.Clear();sandCells.Clear();sandTools.Clear();sandNumbers.Clear();sandpitSending=false;sandpitApproach=sandpitApproachRound=-1;sandpitScoopTaps=0;sandpitOperation=sandIntentId=sandSelectedId=sandFlightId=sandFlightOp=null;sandSpokenRound=-1;sandPlacing=false;sandPreviewCell=-1;foreach(var clip in sandAudio)if(clip!=null)Resources.UnloadAsset(clip);sandAudio.Clear();
        }
    }
}
