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
        private readonly List<RectTransform> sandPieceHits=new List<RectTransform>();
        private readonly List<(RectTransform root,GameCharacterVisual visual)> sandFriends=new List<(RectTransform,GameCharacterVisual)>();
        private readonly List<Button> sandTools=new List<Button>();private readonly List<AudioClip> sandAudio=new List<AudioClip>();
        private readonly List<Text> sandNumbers=new List<Text>();private Text sandHint;
        private TeacherWalkView sandHelpTeacher;
        private bool sandpitSending;private int sandpitSelected=-1,sandpitApproach=-1,sandpitApproachRound=-1,sandpitScoopTaps,sandSpokenRound=-1;
        private string sandpitOperation,sandIntentId,sandSelectedId,sandFlightId,sandFlightOp;private int sandFlightScoops;private Button sandConfirm,sandCancel,sandRotate;
        private string sandMould="round";private int sandOrientation;private readonly List<Button> sandMouldButtons=new List<Button>();private Text sandNextLabel;private SandShape sandNextPicture;
        private readonly List<Button> sandCells=new List<Button>();
        private readonly List<(RectTransform root,GameCharacterVisual visual)> sandBuilders=new List<(RectTransform,GameCharacterVisual)>();
        private SandShape sandPreview;private bool sandPlacing;private int sandPreviewCell=-1,sandPreviewEpoch=-1;private float sandAck;
        public SandpitState SandpitGame=>HasWorld?(Shared?shared.View.sandpit:World.ReadSandpit()):null;
        public string[] SandpitNpcArt=>sandFriends.Select(n=>n.visual.CharacterId).ToArray();public int SandpitSelection=>sandpitSelected;
        private bool SandpitOwn=>SandpitGame?.members.Any(m=>m.actor==Actor && m.attending)==true;
        private bool SandpitTeacherActive=>SandpitGame?.phase>0 && SandpitGame.members.Any(m=>m.attending);
        // A close work surface, independent of world camera/pan. World coordinates
        // still own placement, approach and reach; this is only their UI projection.
        private Vector2 SandPoint(float x,float y)=>new Vector2((x-4445)*1.46f,(y-295)*.93f-32);
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
            CancelSandpitIntent();sandPlacing=false;sandpitSelected=i;sandSelectedId=SandpitGame.moulds[i].id;if(sandPlayMode=="decorate")SuggestSandSlot();CancelPointers();TickSandpit();
        }
        private void BeginSandPlacement(string shape)
        {
            if(!Ready || !SandpitOwn || sandpitSending)return;
            CancelSandpitIntent();CancelPointers();
            if(SandpitGame.moulds.Length>=SandpitGame.pieceLimit){SandCue("sandpit-full");return;}
            sandDecorating=false;sandPlayMode="";sandEditSheet.gameObject.SetActive(false);sandMould=shape;sandOrientation=0;sandPlacing=true;sandPreviewCell=-1;sandPreviewEpoch=SandpitGame.round;TickSandpit();
        }
        private void PreviewSandCell(int cell)
        {if(sandPlacing){sandPreviewCell=cell;SandCue(SandPlacementReason(cell));TickSandpit();}}
        private WalkPoint SandPreviewPoint(int cell)=>DaycareSandpit.PiecePoint(cell%8,cell/8,sandMould,sandOrientation);
        private string SandPlacementReason(int cell){var at=SandPreviewPoint(cell);return SandEditing?SandEditPiece==null?"old-sandpit-piece":SandpitPlay.MoveReason(SandpitGame,SandEditPiece,at.X,at.Y):DaycareSandpit.Placement(SandpitGame,at.X,at.Y,sandMould,sandOrientation);}
        private void RotateSandPlacement(){if(!sandPlacing || sandpitSending)return;sandOrientation=sandOrientation==0?90:0;TickSandpit();}
        private void ConfirmSandPlacement()
        {
            if(!sandPlacing || sandPreviewCell<0 || sandpitSending || sandPreviewEpoch!=SandpitGame.round)return;
            if(SandEditing){var moved=SandPreviewPoint(sandPreviewCell);var why=SandPlacementReason(sandPreviewCell);if(why!=null){SandCue(why);return;}sandPlacing=false;SandPlaySend("move",sandEditId,sandEditVersion.ToString(),moved.X,moved.Y,sandEditEpoch);return;}
            var at=SandPreviewPoint(sandPreviewCell);var reason=SandPlacementReason(sandPreviewCell);
            if(reason!=null){SandCue(reason);return;}
            // Captured position and epoch travel unchanged through revision retries.
            var epoch=sandPreviewEpoch;var choice=DaycareSandpit.Choice(sandMould,sandOrientation);sandpitSending=true;sandAck=Time.unscaledTime+1;
            void Done(SoloResult r){sandpitSending=false;if(SandpitOwn && SandpitGame.round==epoch){
                if(r.Accepted){var i=Array.FindIndex(SandpitGame.moulds,m=>m.x==at.X && m.y==at.Y);sandPlacing=false;sandAck=0;if(i>=0)SelectSandMould(i);}
                else SandCue(r.Outcome);
            }Render();}
            if(Shared){if(!SubmitShared(SoloAction.Sandpit,choice,DaycareSandpit.Target("place",epoch),"place",at.X,at.Y,Done))sandpitSending=false;}
            else Done(Command(SoloAction.Sandpit,item:choice,target:DaycareSandpit.Target("place",epoch),value:"place",x:at.X,y:at.Y));
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
            var label=Button(parent,title,at,new Vector2(136,100),()=>action(),Color.white);label.fontSize=22;label.rectTransform.anchoredPosition=new Vector2(0,-33);label.rectTransform.sizeDelta=new Vector2(136,30);
            var icon=Rect(label.transform.parent,"Picture "+title,new Vector2(0,10),new Vector2(100,65)).gameObject.AddComponent<SandShape>();icon.kind=kind;icon.raycastTarget=false;
            if(kind=="mould-icon"){icon.mould=title=="Round tower"?"round":title=="Square tower"?"square":title=="Wall"?"wall":"gate";icon.rectTransform.localScale=Vector3.one*(DaycareSandpit.LongShape(icon.mould)?.45f:.65f);icon.rectTransform.anchoredPosition=new Vector2(0,-18);}
            if(kind=="bucket"){icon.rectTransform.localScale=Vector3.one*.65f;icon.rectTransform.anchoredPosition=new Vector2(0,-12);}
            return label.transform.parent.GetComponent<Button>();
        }
        private void SandControlPicture(Transform parent,string kind)
        {var icon=Rect(parent,"Picture "+kind,Vector2.zero,new Vector2(100,70)).gameObject.AddComponent<SandShape>();icon.kind=kind;icon.raycastTarget=false;}
        private void SandTeacherHelp()
        {var m=SandSelectedPiece;if(m?.built==true){SandChooseDecoration("flag");return;}Narration.Speak(m!=null && m.scoops==m.capacity && m.wet?"sand-tip":"sand-water");}
        private void BuildSandpit()
        {
            if(SceneSchema<DaycareSandpit.PieceSchema)return;
            // Keep a world entry point; the activity view owns large taps above it.
            sandpitRoot=Rect(Board,"Playable Daycare sandpit",Vector2.zero,new Vector2(780,170));var pit=sandpitRoot.gameObject.AddComponent<SandShape>();pit.kind="pit";pit.raycastTarget=false;
            HomeHit(sandpitRoot,"Play sandpit",new Vector2(0,-30),new Vector2(750,65),ChooseSandpit,false);
            sandBuildView=Panel(safe,"Sandcastle shared building view",Vector2.zero,new Vector2(1210,740),new Color(1,.97f,.88f),true).rectTransform;
            Panel(sandBuildView,"Sandpit wooden rim",new Vector2(0,32),new Vector2(1060,498),new Color(.64f,.43f,.25f),false);
            sandFloor=Panel(sandBuildView,"Our shared sand",new Vector2(0,32),new Vector2(1030,470),new Color(.96f,.82f,.52f),true).rectTransform;
            sandHint=Label(sandBuildView,"",24,new Vector2(0,324),new Vector2(720,45));
            var leave=Button(sandBuildView,"",new Vector2(526,322),new Vector2(105,72),()=>{sandPlacing=false;CancelSandpitIntent();Narration.Stop();SendSandpit("leave");},new Color(.8f,.91f,.9f));leave.transform.parent.name="Leave sandpit";SandControlPicture(leave.transform.parent,"back");
            var help=Button(sandBuildView,"?",new Vector2(-526,322),new Vector2(105,72),SandTeacherHelp,new Color(.82f,.91f,1));help.fontSize=40;help.transform.parent.name="Optional teacher help";
            help.rectTransform.anchoredPosition=new Vector2(28,0);help.rectTransform.sizeDelta=new Vector2(40,65);
            // The world teacher presenter resets its own drawing to world size.
            // Scale its parent so the optional-help picture stays inside the hit.
            var teacherBadge=Rect(help.transform.parent,"Calypso help badge",new Vector2(-18,-26),new Vector2(42,58));teacherBadge.localScale=Vector3.one*.18f;
            var teacherDrawing=Rect(teacherBadge,"Calypso help picture",Vector2.zero,new Vector2(42,58)).gameObject.AddComponent<RawImage>();teacherDrawing.raycastTarget=false;sandHelpTeacher=teacherDrawing.gameObject.AddComponent<TeacherWalkView>();sandHelpTeacher.Configure(teacherDrawing,calypsoTexture);
            // Existing picture hits, no new drag or input framework. Each grid cell
            // is 120 by 96 logical points; its rectangle never follows the finger.
            for(var c=0;c<32;c++){var cell=c;var at=DaycareSandpit.Cell(c%8,c/8);var label=Button(sandFloor,"+",SandPoint(at.X,at.Y),new Vector2(120,96),()=>PreviewSandCell(cell),new Color(1,1,.88f,.65f));label.fontSize=36;label.transform.parent.name="Sand spot "+c;sandCells.Add(label.transform.parent.GetComponent<Button>());}
            for(var i=0;i<DaycareSandpit.MaxPieces;i++){
                var index=i;var bucket=DrawSandBucket(sandFloor,Vector2.zero,0,3,false);sandShapes.Add(bucket);sandPlaces.Add(bucket.rectTransform);
                var highlight=Panel(bucket.transform,"Your selected sand mould",new Vector2(0,46),new Vector2(128,125),new Color(1,.7f,.05f),false);highlight.sprite=hintRing;highlight.raycastTarget=false;highlight.transform.SetAsFirstSibling();sandHighlights.Add(highlight);
                var cue=Rect(bucket.transform,"Next action picture",new Vector2(0,135),new Vector2(65,65)).gameObject.AddComponent<SandShape>();cue.kind="selection";cue.raycastTarget=false;sandTipCues.Add(cue);
                var effect=Rect(sandFloor,"Accepted sand scoop "+(i+1),Vector2.zero,new Vector2(1400,500)).gameObject.AddComponent<SandShape>();effect.kind="sand-transfer";effect.raycastTarget=false;sandScoopEffects.Add(effect);
                sandNumbers.Add(Label(bucket.transform,"",22,new Vector2(0,-18),new Vector2(140,30)));
                HomeHit(sandFloor,"Sand piece "+(i+1),Vector2.zero,new Vector2(150,165),()=>TouchSandMould(index),false);
                sandPieceHits.Add((RectTransform)sandFloor.Find("Sand piece "+(i+1)));
            }
            BuildSandWater();BuildSandTip();
            sandPreview=DrawSandBucket(sandFloor,Vector2.zero,0,3,false);sandPreview.color=new Color(1,1,1,.45f);
            var buildTab=Button(sandBuildView,"Build",new Vector2(-440,-234),new Vector2(150,64),()=>SandTray(false),new Color(.98f,.73f,.31f));buildTab.fontSize=22;

            var shapes=new[]{"round","square","wall","gate"};var titles=new[]{"Round tower","Square tower","Wall","Gate"};
            for(var i=0;i<4;i++){var shape=shapes[i];sandMouldButtons.Add(SandPictureButton(sandBuildView,titles[i],"mould-icon",new Vector2(-435+i*150,-318),()=>BeginSandPlacement(shape)));}
            var confirmLabel=Button(sandBuildView,"",new Vector2(210,-302),new Vector2(136,100),ConfirmSandPlacement,new Color(.53f,.88f,.71f));sandConfirm=confirmLabel.transform.parent.GetComponent<Button>();sandConfirm.name="Confirm sand placement";SandControlPicture(sandConfirm.transform,"confirm");
            var cancelLabel=Button(sandBuildView,"",new Vector2(510,-302),new Vector2(136,100),()=>{sandPlacing=false;sandPreviewCell=-1;TickSandpit();},new Color(1,.82f,.73f));sandCancel=cancelLabel.transform.parent.GetComponent<Button>();sandCancel.name="Cancel sand placement";SandControlPicture(sandCancel.transform,"cancel");
            var rotateLabel=Button(sandBuildView,"",new Vector2(360,-302),new Vector2(136,100),RotateSandPlacement,new Color(.69f,.82f,1));sandRotate=rotateLabel.transform.parent.GetComponent<Button>();sandRotate.name="Rotate sand mould";SandControlPicture(sandRotate.transform,"rotate");
            var next=SandPictureButton(sandBuildView,"Scoop","scoop",new Vector2(210,-302),()=>RequestSandTool(sandNextPicture.kind));sandTools.Add(next);next.name="Next sand tool";
            sandNextLabel=next.GetComponentInChildren<Text>();sandNextPicture=next.GetComponentsInChildren<SandShape>().First();
            sandTools.Add(SandPictureButton(sandBuildView,"Water","water",new Vector2(360,-302),()=>RequestSandTool("water")));
            sandTools.Add(SandPictureButton(sandBuildView,"Tip","tip",new Vector2(510,-302),()=>RequestSandTool("tip")));
            // Rim seats stay outside both the lattice and footer hit regions.
            for(var i=0;i<4;i++){var root=Rect(sandBuildView,"Family builder "+i,new Vector2(i<2?-560:560,i%2==0?110:-100),new Vector2(90,130));root.localScale=Vector3.one*.6f;sandBuilders.Add((root,root.gameObject.AddComponent<GameCharacterVisual>()));}
            foreach(var id in new[]{"water","tip"}){var clip=WorldResources.Load<AudioClip>("Worlds/Daycare/SandcastleClub/"+id);if(clip!=null){sandAudio.Add(clip);Narration.AddClip("sand-"+id,clip);}}
            BuildSandPlay();TickSandpit();
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
                var active=i<g.moulds.Length;var shape=sandShapes[i];shape.gameObject.SetActive(active);sandPieceHits[i].gameObject.SetActive(active);
                if(!active){sandScoopEffects[i].gameObject.SetActive(false);continue;}
                var m=g.moulds[i];shape.rectTransform.anchoredPosition=SandPoint(m.x,m.y);shape.rectTransform.localScale=Vector3.one;shape.mould=m.shape;shape.orientation=m.orientation;shape.scoops=m.scoops;shape.capacity=m.capacity;shape.wet=m.wet;shape.built=m.built;shape.decoration=m.decoration;shape.attachments=m.attachments;shape.SetVerticesDirty();
                sandNumbers[i].text=i==sandpitSelected && !sandPlacing && !m.built?m.scoops+" / "+m.capacity:"";
                var hit=sandPieceHits[i];hit.sizeDelta=new Vector2(m.width*1.46f+12,m.depth*.93f+85);hit.anchoredPosition=shape.rectTransform.anchoredPosition+new Vector2(0,40);hit.GetComponent<Graphic>().raycastTarget=!sandPlacing;
                sandHighlights[i].rectTransform.sizeDelta=new Vector2(m.width*1.46f+18,m.depth*.93f+55);sandHighlights[i].gameObject.SetActive(joined && !sandPlacing && i==sandpitSelected);
                var cue=sandTipCues[i];cue.gameObject.SetActive(joined && !sandPlacing && i==sandpitSelected);cue.kind="selection";cue.SetVerticesDirty();
                var effect=sandScoopEffects[i];effect.gameObject.SetActive(joined && sandFeedback.Remaining(i)>0);effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one;
                effect.source=new Vector2(-70,-25);effect.target=new Vector2(0,m.capacity==3?75:55);effect.progress=1-sandFeedback.Remaining(i)/SandScoopFeedback.Duration;effect.SetVerticesDirty();
            }
            // Farther pieces paint first; a newly added back-row gate cannot
            // paint through a nearer tower merely because its ID is newer.
            foreach(var i in Enumerable.Range(0,g.moulds.Length).OrderByDescending(i=>g.moulds[i].y)){sandShapes[i].rectTransform.SetAsLastSibling();sandPieceHits[i].SetAsLastSibling();}
            // Show the active bucket's fill in front of nearby completed pieces,
            // while its separate hit retains ground-depth selection priority.
            if(joined && !sandPlacing && sandpitSelected>=0 && !g.moulds[sandpitSelected].built)sandShapes[sandpitSelected].rectTransform.SetAsLastSibling();
            TickSandWater(g,joined);TickSandTip(g,joined);
            foreach(var effect in sandScoopEffects.Concat(sandWaterEffects).Concat(sandTipEffects))if(effect.gameObject.activeSelf)effect.rectTransform.SetAsLastSibling();
            if(!joined){TickSandPlay(g,false);return;}
            if(sandPlacing && sandPreviewEpoch!=g.round){sandPlacing=false;sandPlayMode="";}
            for(var c=0;c<sandCells.Count;c++){var free=SandPlacementReason(c)==null;sandCells[c].gameObject.SetActive(sandPlacing);sandCells[c].interactable=free && !sandpitSending;}
            sandPreview.gameObject.SetActive(sandPlacing && sandPreviewCell>=0);
            if(sandPlacing)sandPreview.rectTransform.SetAsLastSibling();
            if(sandPreviewCell>=0){var at=SandPreviewPoint(sandPreviewCell);sandPreview.rectTransform.anchoredPosition=SandPoint(at.X,at.Y);sandPreview.rectTransform.localScale=Vector3.one;sandPreview.mould=sandMould;sandPreview.orientation=sandOrientation;sandPreview.kind="mould-icon";sandPreview.color=SandPlacementReason(sandPreviewCell)==null?new Color(1,1,1,.65f):new Color(1,.4f,.3f,.65f);sandPreview.SetVerticesDirty();}
            sandConfirm.gameObject.SetActive(sandPlacing);sandCancel.gameObject.SetActive(sandPlacing);sandRotate.gameObject.SetActive(sandPlacing && DaycareSandpit.LongShape(sandMould));sandRotate.interactable=!sandpitSending;
            sandConfirm.interactable=sandPreviewCell>=0 && !sandpitSending && SandPlacementReason(sandPreviewCell)==null;
            for(var i=0;i<4;i++){sandMouldButtons[i].interactable=!sandpitSending && g.moulds.Length<g.pieceLimit;sandMouldButtons[i].GetComponent<Image>().color=sandPlacing && sandMould==new[]{"round","square","wall","gate"}[i]?new Color(1,.79f,.35f):new Color(1,.91f,.68f);}
            var selected=SandSelectedPiece;var next=selected==null || selected.built?-1:selected.scoops<selected.capacity?0:!selected.wet?1:2;
            var tools=next>=0 && !sandPlacing;
            sandTools[0].gameObject.SetActive(tools);sandTools[1].gameObject.SetActive(tools && next==0 && !selected.wet);sandTools[2].gameObject.SetActive(tools && next!=2);
            if(tools){
                var op=new[]{"scoop","water","tip"}[next];sandNextLabel.text=new[]{"Scoop","Water","Tip"}[next];sandNextPicture.kind=op;sandNextPicture.SetVerticesDirty();sandTools[0].GetComponent<Image>().color=new Color(.51f,.87f,.73f);
                // Tablet: beside the bucket only if the hit is clear. Phone/crowded
                // castle: the fixed shelf keeps every essential action unobstructed.
                var at=SandPoint(selected.x,selected.y)+sandFloor.anchoredPosition;var side=(at.x>270?-1:1)*(next==1?-1:1);var near=at+new Vector2(side*(selected.width*1.46f/2+90),25);
                var clear=safe.rect.width/safe.rect.height<1.6f && Mathf.Abs(near.x)<435 && near.y>-120 && near.y<190 && !g.moulds.Any(m=>m.id!=selected.id && Mathf.Abs(SandPoint(m.x,m.y).x+sandFloor.anchoredPosition.x-near.x)<m.width*1.46f/2+80 && Mathf.Abs(SandPoint(m.x,m.y).y+sandFloor.anchoredPosition.y+45-near.y)<m.depth*.93f/2+90);
                // Each tool keeps its own shelf spot; a burst at the old Scoop
                // hit cannot become Water/Tip when fill reaches capacity. Nearby
                // tools alternate sides for the same reason, with shelf fallback.
                ((RectTransform)sandTools[0].transform).anchoredPosition=clear?near:new Vector2(210+150*next,-302);
                for(var i=0;i<3;i++)sandTools[i].interactable=true;
            }
            if(Time.unscaledTime>=sandAck)sandHint.text=g.moulds.Length>=g.pieceLimit?"Full · "+g.moulds.Length+"/"+g.pieceLimit:sandPlacing?"Choose a spot":selected==null?"Build":selected.built?"Build again":selected.scoops+" / "+selected.capacity+(selected.wet?" · Wet":"");
            for(var i=0;i<sandBuilders.Count;i++){var member=i<g.members.Length?g.members[i]:null;var n=sandBuilders[i];n.root.gameObject.SetActive(member?.attending==true);if(member?.attending==true){n.visual.Select(ReadPlayer(member.actor).avatar);n.visual.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false,(float)calypsoClock),Mathf.Min(Time.unscaledDeltaTime,.1f));}}
            TickSandPlay(g,joined);sandBuildView.SetAsLastSibling();
            sandHelpTeacher.Present(new CharacterFrame(CharacterPose.Sit,0,false,(float)calypsoClock),1,Mathf.Min(Time.unscaledDeltaTime,.1f),1);
        }
        private void AddSandpitDepth(Action<RectTransform,float,int,string> add)
        {if(sandpitRoot!=null && sandpitRoot.gameObject.activeInHierarchy)add(sandpitRoot,sandpitRoot.anchoredPosition.y+95*sceneScale,0,"sandpit");}
        private void ResetSandpit()
        {
            ResetSandPlay();ResetSandWater();ResetSandTip();ResetSandDecoration();sandpitRoot=sandBuildView=sandFloor=null;sandHelpTeacher=null;sandPlaces.Clear();sandShapes.Clear();sandPieceHits.Clear();sandHighlights.Clear();sandTipCues.Clear();sandScoopEffects.Clear();sandFeedback=new SandScoopFeedback();sandFriends.Clear();sandBuilders.Clear();sandCells.Clear();sandTools.Clear();sandMouldButtons.Clear();sandMould="round";sandOrientation=0;sandNumbers.Clear();sandpitSending=false;sandpitApproach=sandpitApproachRound=-1;sandpitScoopTaps=0;sandpitOperation=sandIntentId=sandSelectedId=sandFlightId=sandFlightOp=null;sandSpokenRound=-1;sandPlacing=false;sandPreviewCell=-1;foreach(var clip in sandAudio)if(clip!=null)Resources.UnloadAsset(clip);sandAudio.Clear();
        }
    }
}
