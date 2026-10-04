using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace LittleWeeps.Client
{
    // Approved P9 presentation only. Authority, commands, reach, editing and reset
    // remain in the existing activity; no fixture or replacement session lives here.
    public sealed partial class GameScreen
    {
        private readonly List<SandcastlePrototypePiece> sandApprovedPieces=new List<SandcastlePrototypePiece>();
        private readonly List<List<SandShape>> sandApprovedProps=new List<List<SandShape>>();
        private readonly List<Outline> sandEligibleOutlines=new List<Outline>();
        private SandcastleActivitySurface sandDirectSurface;
        private RectTransform sandDirectCue;private Text sandDirectCueLabel;private float sandDirectCueUntil;
        private int sandDirectGeneration;private Vector2 sandDirectPoint;
        private bool sandDirectDragging,sandDirectDragOnSand;
        public bool SandPlacementActive=>sandPlacing;
        public bool SandLocalPending=>sandpitSending;
        public bool SandDirectCueVisible=>sandDirectCue!=null && sandDirectCue.gameObject.activeInHierarchy;
        public string SandDecorationChoice=>sandPlayMode=="decorate"?sandDecorKind:"";
        public Vector2[] SandAttachmentScreenPoints=>SandSelectedPiece==null?Array.Empty<Vector2>():Enumerable.Range(0,SandpitPlay.Slots).Select(s=>RectTransformUtility.WorldToScreenPoint(null,sandFloor.TransformPoint(SandPoint(SandSelectedPiece.x,SandSelectedPiece.y)+SandApprovedAnchor(SandSelectedPiece,s)*SandcastleProjection.DepthScale(SandSelectedPiece.y)))).ToArray();
        private SandcastlePrototypePiece sandApprovedPreview;
        private Sprite sandApprovedPanel;
        private Texture2D sandParticipation;
        private readonly Dictionary<string,(string op,float until)> sandParticipantCues=new Dictionary<string,(string,float)>();
        private readonly Dictionary<string,int> sandParticipantEvents=new Dictionary<string,int>();
        public string[] SandBuilderActors=>SandpitGame?.members.Where(m=>m.attending).Select(m=>m.actor).ToArray()??Array.Empty<string>();
        public int[] SandBuilderEvents=>SandBuilderActors.Select(a=>sandParticipantEvents.TryGetValue(a,out var n)?n:0).ToArray();
        public string[] SandBuilderReactions=>SandBuilderActors.Select(a=>sandParticipantCues.TryGetValue(a,out var c) && c.until>Time.unscaledTime?c.op:"ready").ToArray();
        public Vector2 SandScreenPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,sandFloor.TransformPoint(SandPoint(x,y)));
        public Vector2[] SandPieceScreenPoints=>SandpitGame.moulds.Select(m=>SandScreenPoint(m.x,m.y)).ToArray();
        private Image SandRoundPanel(Transform parent,string name,Vector2 at,Vector2 size,Color color)
        {var image=Panel(parent,name,at,size,color,false);image.sprite=sandApprovedPanel;image.type=Image.Type.Sliced;return image;}
        private void BuildApprovedSand()
        {
            var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(var x=0;x<64;x++)for(var y=0;y<64;y++){var dx=Mathf.Max(16-x,0,x-47);var dy=Mathf.Max(16-y,0,y-47);tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(16-Mathf.Sqrt(dx*dx+dy*dy))));}
            tex.Apply();sandApprovedPanel=Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(17,17,17,17));
            sandParticipation=WorldResources.Load<Texture2D>("Worlds/Daycare/SandcastleClub/Prototype/participation");
            sandDirectSurface=sandFloor.gameObject.AddComponent<SandcastleActivitySurface>();sandDirectSurface.Click=SandSurfaceTap;sandDirectSurface.Generation=()=>sandDirectGeneration;
            for(var i=0;i<sandShapes.Count;i++){
                var picture=Rect(sandShapes[i].transform,"Approved separate castle piece",Vector2.zero,new Vector2(160,300)).gameObject.AddComponent<SandcastlePrototypePiece>();picture.raycastTarget=false;sandApprovedPieces.Add(picture);sandApprovedProps.Add(new List<SandShape>());
                var outline=picture.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.03f,.4f,.64f,.95f);outline.effectDistance=new Vector2(5,-5);outline.enabled=false;sandEligibleOutlines.Add(outline);
            }
            sandApprovedPreview=Rect(sandPreview.transform,"Approved shape preview",Vector2.zero,new Vector2(160,300)).gameObject.AddComponent<SandcastlePrototypePiece>();sandApprovedPreview.raycastTarget=false;sandPreview.enabled=false;
            var tray=SandRoundPanel(sandBuildView,"Illustrated shape and decoration shelf",new Vector2(0,-398),new Vector2(1370,136),new Color(1,.97f,.87f));tray.transform.SetSiblingIndex(sandFloor.GetSiblingIndex()+1);
            var tools=SandRoundPanel(sandBuildView,"Illustrated contextual tools",new Vector2(480,-248),new Vector2(365,112),new Color(.94f,.98f,1));tools.transform.SetSiblingIndex(tray.transform.GetSiblingIndex()+1);
            foreach(var b in sandBuildView.GetComponentsInChildren<Button>(true)){
                var image=b.GetComponent<Image>();if(image==null || b.transform.IsChildOf(sandFloor))continue;
                image.sprite=sandApprovedPanel;image.type=Image.Type.Sliced;var colors=b.colors;colors.disabledColor=new Color(.58f,.62f,.65f,.65f);colors.pressedColor=new Color(.75f,.87f,1);b.colors=colors;
            }
            for(var i=0;i<4;i++){
                var b=sandMouldButtons[i];b.GetComponentInChildren<SandShape>().enabled=false;
                var picture=Rect(b.transform,"Approved mould picture",new Vector2(0,-27),new Vector2(160,300)).gameObject.AddComponent<SandcastlePrototypePiece>();picture.shape=new[]{"round","square","wall","gate"}[i];picture.raycastTarget=false;picture.rectTransform.localScale=Vector3.one*(i<2?.38f:.38f);
                var label=b.GetComponentInChildren<Text>();label.text=new[]{"Round","Square","Wall","Gate"}[i];label.rectTransform.anchoredPosition=new Vector2(0,-39);
                var shape=picture.shape;SandTrayDrag(b,()=>BeginSandPlacement(shape));
            }
            for(var i=0;i<5;i++){var kind=new[]{"flag","shell","pebble","door","window"}[i];SandTrayDrag(sandDecorCards[i],()=>SandChooseDecoration(kind));}
            sandDirectCue=SandRoundPanel(sandFloor,"Brief local sand feedback",Vector2.zero,new Vector2(280,72),new Color(.08f,.32f,.43f,.96f)).rectTransform;
            sandDirectCueLabel=Label(sandDirectCue,"",22,Vector2.zero,new Vector2(265,64));sandDirectCueLabel.color=Color.white;
            foreach(var graphic in sandDirectCue.GetComponentsInChildren<Graphic>())graphic.raycastTarget=false;sandDirectCue.gameObject.SetActive(false);
            var build=sandBuildView.Find("Build");var buildLabel=build.GetComponentInChildren<Text>();buildLabel.rectTransform.anchoredPosition=new Vector2(0,-38);
            var buildIcon=Rect(build,"Build picture",new Vector2(0,-23),new Vector2(160,240)).gameObject.AddComponent<SandcastlePrototypePiece>();buildIcon.shape="round";buildIcon.raycastTarget=false;buildIcon.rectTransform.localScale=Vector3.one*.32f;
            var decorate=sandBuildView.Find("Decorate");decorate.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(0,-38);
            foreach(var pair in new[]{("flag",-20f),("shell",30f)}){var icon=Rect(decorate,"Decorate "+pair.Item1,new Vector2(pair.Item2,15),new Vector2(100,70)).gameObject.AddComponent<SandShape>();icon.kind=pair.Item1;icon.raycastTarget=false;icon.rectTransform.localScale=Vector3.one*.8f;}
            sandBuildView.Find("Small sand action hint").GetComponent<Image>().sprite=sandApprovedPanel;sandBuildView.Find("Small sand action hint").GetComponent<Image>().type=Image.Type.Sliced;
            // The pit's alpha hides lower bodies; these graphics never own input.
            for(var i=0;i<4;i++){sandBuilders[i].root.SetSiblingIndex(sandPitArt.transform.GetSiblingIndex());foreach(var graphic in sandBuilders[i].root.GetComponentsInChildren<Graphic>())graphic.raycastTarget=false;}
        }
        private Color SandMouldColor(int i)
        {return new[]{new Color(.65f,.87f,1),new Color(.78f,.91f,.69f),new Color(1,.87f,.6f),new Color(.86f,.76f,1)}[i];}
        private void SandControlLayout(Button button,Vector2 at,Vector2 size)
        {var r=(RectTransform)button.transform;r.anchoredPosition=at;r.sizeDelta=size;}
        private void LayoutApprovedSand()
        {
            sandBuildView.sizeDelta=new Vector2(1400,1000);sandBuildView.localScale=Vector3.one*Mathf.Min(safe.rect.width/1420,safe.rect.height/950);
            sandScenery.rectTransform.sizeDelta=safe.rect.size/sandBuildView.localScale.x;
            sandPitArt.rectTransform.anchoredPosition=new Vector2(0,-40);sandPitArt.rectTransform.sizeDelta=new Vector2(1560,860);
            sandFloor.anchoredPosition=Vector2.zero;sandFloor.localScale=Vector3.one;sandFloor.sizeDelta=new Vector2(1450,620);
            
            for(var i=0;i<4;i++){SandControlLayout(sandMouldButtons[i],new Vector2(-500+i*207,-398),new Vector2(185,113));if(!sandPlacing || sandMould!=new[]{"round","square","wall","gate"}[i])sandMouldButtons[i].GetComponent<Image>().color=SandMouldColor(i);}
            for(var i=0;i<5;i++)SandControlLayout(sandDecorCards[i],new Vector2(-520+i*165,-398),new Vector2(152,113));
            SandControlLayout(sandDecorCards[5],new Vector2(555,-248),new Vector2(152,104));
            SandControlLayout(sandBuildView.Find("Build").GetComponent<Button>(),new Vector2(370,-398),new Vector2(150,110));
            SandControlLayout(sandBuildView.Find("Decorate").GetComponent<Button>(),new Vector2(555,-398),new Vector2(175,110));
            sandBuildView.Find("Build").GetComponent<Image>().color=!sandDecorating?new Color(.57f,.85f,1):new Color(.94f,.96f,.96f);
            sandBuildView.Find("Decorate").GetComponent<Image>().color=sandDecorating?new Color(.75f,.91f,.63f):new Color(.94f,.96f,.96f);
            for(var i=0;i<3;i++){SandControlLayout(sandTools[i],new Vector2(364+i*116,-248),new Vector2(108,96));sandTools[i].GetComponent<Image>().color=new[]{new Color(1,.9f,.58f),new Color(.64f,.9f,1),new Color(.86f,.73f,1)}[i];}
            SandControlLayout(sandConfirm,new Vector2(367,-248),new Vector2(108,96));SandControlLayout(sandRotate,new Vector2(480,-248),new Vector2(108,96));SandControlLayout(sandCancel,new Vector2(593,-248),new Vector2(108,96));
            SandControlLayout(sandEdit,new Vector2(-625,310),new Vector2(110,80));SandControlLayout(sandFamilyReset,new Vector2(625,404),new Vector2(125,80));
            SandControlLayout(sandPlayCancel,new Vector2(365,-248),new Vector2(108,96));
            SandControlLayout(sandBuildView.Find("Optional teacher help").GetComponent<Button>(),new Vector2(625,310),new Vector2(110,80));SandControlLayout(sandBuildView.Find("Leave sandpit").GetComponent<Button>(),new Vector2(-625,404),new Vector2(110,80));
            var hint=sandBuildView.Find("Small sand action hint").GetComponent<RectTransform>();hint.anchoredPosition=new Vector2(-260,-285);hint.sizeDelta=new Vector2(850,46);sandHint.rectTransform.anchoredPosition=hint.anchoredPosition;sandHint.rectTransform.sizeDelta=new Vector2(850,50);
            for(var i=0;i<4;i++){sandBuilders[i].root.anchoredPosition=new Vector2(-465+i*310,240);sandBuilders[i].root.localScale=Vector3.one*new[]{1.22f,1.13f,1.1f,.96f}[i];}
            var contextual=sandBuildView.Find("Illustrated contextual tools");if(contextual!=null)contextual.gameObject.SetActive(sandPlacing || sandTools.Any(b=>b.gameObject.activeSelf) || sandPlayMode=="decorate");
        }
        private void TickApprovedPieces(SandpitState g)
        {
            for(var i=0;i<g.moulds.Length;i++){
                var m=g.moulds[i];var picture=sandApprovedPieces[i];sandShapes[i].enabled=!m.built;picture.enabled=m.built;picture.shape=m.shape;picture.orientation=m.orientation;picture.reveal=sandShapes[i].towerReveal;picture.SetVerticesDirty();
                var props=sandApprovedProps[i];while(props.Count<m.attachments.Length){var prop=Rect(sandShapes[i].transform,"Separate attached decoration",Vector2.zero,new Vector2(160,240)).gameObject.AddComponent<SandShape>();prop.kind="attachment";prop.raycastTarget=false;props.Add(prop);}
                for(var a=0;a<props.Count;a++){var prop=props[a];prop.gameObject.SetActive(a<m.attachments.Length);if(a>=m.attachments.Length)continue;prop.decorationKind=m.attachments[a].kind;prop.rectTransform.anchoredPosition=SandApprovedAnchor(m,m.attachments[a].slot);prop.SetVerticesDirty();}
            }
            if(sandPreviewCell!=-1){sandApprovedPreview.shape=sandMould;sandApprovedPreview.orientation=sandOrientation;sandApprovedPreview.color=sandPreview.color;sandApprovedPreview.SetVerticesDirty();}
        }
        private Vector2 SandApprovedAnchor(SandMould m,int slot)
        {
            var side=slot%2==0?-1:1;
            if(!DaycareSandpit.LongShape(m.shape))return new Vector2(side*(slot<2?22:slot<4?59:25),slot<2?176:slot<4?-12:76);
            if(m.orientation==90)return slot<2?new Vector2(side*34,side<0?104:215):slot<4?new Vector2(side*42,side<0?-78:87):new Vector2(side*28,side<0?5:120);
            return new Vector2(side*(slot<2?m.shape=="gate"?116:93:slot<4?140:116),slot<2?m.shape=="gate"?131:75:slot<4?-13:43);
        }
        private void SandSurfaceTap(Vector2 point)
        {
            if(!SandDirectReady)return;sandDirectPoint=point;
            var logical=SandcastleProjection.Inverse(point);
            if(sandPlacing){
                // Snap once locally; exact captured coordinates are also used by
                // authority retries. Never search another cell after a conflict.
                var at=DaycareSandpit.Snap(logical.x,logical.y,sandMould,sandOrientation);sandPreviewLogical=new Vector2(at.X,at.Y);sandPreviewCell=-2;
                var why=SandPlacementReason(-2);
                if(logical.x<DaycareSandpit.Left || logical.x>DaycareSandpit.Right || logical.y<DaycareSandpit.Bottom || logical.y>DaycareSandpit.Top)why="outside-sandpit";
                if(why!=null){SandCueAt(why,point);TickSandpit();return;}
                if(!SandEditing)ConfirmSandPlacement();TickSandpit();return;
            }
            if(sandPlayMode=="decorate"){SandDecorateAt(point);return;}
            if(sandPlayMode=="toy"){if(SandpitPlay.ToyPosition(SandpitGame,logical.x,logical.y))SandPlaySend("toy-place","toy",sandToyVersion.ToString(),logical.x,logical.y);else SandCue("sand-spot-taken");return;}
            foreach(var i in Enumerable.Range(0,SandpitGame.moulds.Length).OrderBy(i=>SandpitGame.moulds[i].y)){
                var m=SandpitGame.moulds[i];var local=(point-SandPoint(m.x,m.y))/SandcastleProjection.DepthScale(m.y);
                if(m.built?sandApprovedPieces[i].PaintedHit(local):new Rect(-65,-12,140,125).Contains(local)){TouchSandMould(i);return;}
            }
        }
        private bool SandDirectReady=>Ready && SandpitOwn && CurrentArea=="daycare" && !TravelPending && !MenuOpen && !applicationPaused && !sandpitSending && (!Shared || shared.Connected) && sandBuildView.gameObject.activeInHierarchy && !sandEditSheet.gameObject.activeSelf && !sandConfirmSheet.gameObject.activeSelf && SandpitGame.reset==null;
        private void ClearSandDirectInput()
        {sandDirectGeneration++;sandPlacing=false;sandPreviewCell=-1;sandPlayMode="";sandSlot=-1;sandDirectDragging=sandDirectDragOnSand=false;sandDirectCueUntil=0;}
        private void SandCueAt(string why,Vector2 point)
        {SandCue(why);sandDirectCueUntil=Time.unscaledTime+1.4f;sandDirectCue.anchoredPosition=new Vector2(Mathf.Clamp(point.x,-565,565),Mathf.Clamp(point.y+55,-210,290));sandDirectCueLabel.text=sandHint.text;}
        private int SandAvailableSlot(SandMould m,Vector2 local)
        {
            // Current sockets remain the saved representation. Touch chooses
            // the nearest EMPTY compatible socket within a generous painted area.
            return Enumerable.Range(0,SandpitPlay.Slots).Where(s=>SandpitPlay.Attachment(s,sandDecorKind) && !m.attachments.Any(a=>a.slot==s) && Vector2.Distance(SandApprovedAnchor(m,s),local)<=140)
                .OrderBy(s=>(SandApprovedAnchor(m,s)-local).sqrMagnitude).DefaultIfEmpty(-1).First();
        }
        private void SandDecorateAt(Vector2 point)
        {
            var g=SandpitGame;var index=-1;var best=float.MaxValue;
            foreach(var i in Enumerable.Range(0,g.moulds.Length).OrderBy(i=>g.moulds[i].y)){
                var m=g.moulds[i];var local=(point-SandPoint(m.x,m.y))/SandcastleProjection.DepthScale(m.y);
                // A painted foreground piece owns its tap, including rejection.
                if(m.built?sandApprovedPieces[i].PaintedHit(local):new Rect(-65,-12,140,125).Contains(local)){index=i;break;}
                if(!m.built)continue;
                var distance=Enumerable.Range(0,SandpitPlay.Slots).Where(s=>SandpitPlay.Attachment(s,sandDecorKind)).Select(s=>Vector2.Distance(local,SandApprovedAnchor(m,s))).DefaultIfEmpty(float.MaxValue).Min();
                if(distance<=140 && distance<best){index=i;best=distance;}
            }
            if(index<0){SandCueAt("choose-sand-region",point);return;}
            var piece=g.moulds[index];if(!piece.built){SandCueAt("finish-piece-first",point);return;}
            var p=(point-SandPoint(piece.x,piece.y))/SandcastleProjection.DepthScale(piece.y);var slot=SandAvailableSlot(piece,p);
            if(slot<0){var empty=Enumerable.Range(0,SandpitPlay.Slots).Any(s=>SandpitPlay.Attachment(s,sandDecorKind) && !piece.attachments.Any(a=>a.slot==s));SandCueAt(empty?"choose-sand-region":"sand-slot-taken",point);return;}
            SelectSandMould(index);sandDirectPoint=point;
            SandPlaySend("decorate",piece.id,slot+":"+sandDecorKind,epoch:g.round);
        }
        private void TickSandDirectInput(bool joined)
        {
            for(var i=0;i<sandEligibleOutlines.Count;i++){
                var m=i<SandpitGame.moulds.Length?SandpitGame.moulds[i]:null;
                sandEligibleOutlines[i].enabled=joined && sandPlayMode=="decorate" && m?.built==true && Enumerable.Range(0,SandpitPlay.Slots).Any(s=>SandpitPlay.Attachment(s,sandDecorKind) && !m.attachments.Any(a=>a.slot==s));
            }
            for(var i=0;i<5;i++)sandDecorCards[i].GetComponent<Image>().color=sandPlayMode=="decorate" && sandDecorKind==new[]{"flag","shell","pebble","door","window"}[i]?new Color(1,.8f,.36f):Color.white;
            sandDirectCue.gameObject.SetActive(joined && Time.unscaledTime<sandDirectCueUntil);if(sandDirectCue.gameObject.activeSelf)sandDirectCue.SetAsLastSibling();
        }
        private void SandTrayDrag(Button button,Action choose)
        {
            var drag=button.gameObject.AddComponent<SandcastleTrayDrag>();
            drag.Begin=()=>{
                if(!button.IsInteractable() || !SandDirectReady)return false;
                var previous=sandPlacing?sandMould:"";var rotation=sandOrientation;choose();
                if(sandPlacing && previous==sandMould)sandOrientation=rotation;
                sandDirectDragging=true;return true;
            };
            drag.Move=e=>{
                sandDirectDragOnSand=sandDirectDragging && SandDirectReady && sandDirectSurface.LocalPoint(e,out sandDirectPoint);
                if(sandDirectDragOnSand && sandPlacing){var p=SandcastleProjection.Inverse(sandDirectPoint);var at=DaycareSandpit.Snap(p.x,p.y,sandMould,sandOrientation);sandPreviewLogical=new Vector2(at.X,at.Y);sandPreviewCell=-2;}
                else if(sandPlacing)sandPreviewCell=-1;
                TickSandpit();
            };
            drag.End=e=>{
                var point=Vector2.zero;var place=sandDirectDragging && SandDirectReady && !SandcastleActivitySurface.Canceled(e) && sandDirectSurface.LocalPoint(e,out point);
                sandDirectDragging=sandDirectDragOnSand=false;
                if(place)SandSurfaceTap(point);else ClearSandDirectInput();TickSandpit();
            };
            drag.Cancel=()=>{if(sandDirectDragging)ClearSandDirectInput();};
        }
        private void SandParticipantResponse(string actor,string op)
        {
            if(!SandpitOwn || applicationPaused || !SandpitGame.members.Any(m=>m.actor==actor && m.attending))return;
            if(op!="scoop" && op!="water" && op!="tip" && op!="decorate" && op!="place" && op!="toy-react")return;
            sandParticipantCues[actor]=(op,Time.unscaledTime+(op=="water"?1.35f:op=="tip"?1.8f:.85f));sandParticipantEvents[actor]=sandParticipantEvents.TryGetValue(actor,out var n)?n+1:1;
        }
        private long sandResponseRevision=-1;private bool sandResponseJoined;private int sandResponseRound=-1;
        private bool SandLocalFeedback=>!Shared || !(shared is ISandcastleResponses feed) || !feed.SandcastleFeedbackAvailable;
        private void TickSandParticipantFeedback(SandpitState g,bool joined)
        {
            var events=Shared && shared is ISandcastleResponses feed?feed.SandcastleResponses:Array.Empty<SandcastleResponse>();
            var baseline=!sandResponseJoined || !joined || applicationPaused || sandResponseRound!=g.round;
            if(!baseline)foreach(var response in events.Where(r=>r.revision>sandResponseRevision).OrderBy(r=>r.revision))if(response.round==g.round)SandParticipantResponse(response.actor,response.operation);
            if(events.Length>0)sandResponseRevision=Math.Max(sandResponseRevision,events.Max(r=>r.revision));
            sandResponseJoined=joined && !applicationPaused;sandResponseRound=g.round;if(baseline)sandParticipantCues.Clear();
        }
        private void TickApprovedCast(SandpitState g,bool joined)
        {
            TickSandParticipantFeedback(g,joined);var names=new[]{"blue-pup","orange-pup","muffin","socks"};
            if(!joined){sandParticipantCues.Clear();return;}
            for(var i=0;i<4;i++){
                var member=i<g.members.Length?g.members[i]:null;var v=sandBuilders[i].visual;sandBuilders[i].root.gameObject.SetActive(member?.attending==true);if(member?.attending!=true)continue;
                var name=ReadPlayer(member.actor).avatar;v.Select(name);var row=Array.IndexOf(names,name);var op=sandParticipantCues.TryGetValue(member.actor,out var cue) && cue.until>Time.unscaledTime?cue.op:"ready";
                var view=v.ActiveView;view.transform.localScale=Vector3.one;view.transform.localRotation=Quaternion.identity;((RectTransform)view.transform).anchoredPosition=new Vector2(0,-45);
                if(row<0 || op=="tip" || op=="decorate" || op=="place" || op=="toy-react"){
                    ((RectTransform)view.transform).anchoredPosition=Vector2.zero;v.PresentFrame(new CharacterFrame(op=="ready"?CharacterPose.Sit:CharacterPose.Wave,0,i>1),Mathf.Min(Time.unscaledDeltaTime,.1f));continue;
                }
                var state=op=="water"?2:op=="scoop"?1:0;var crop=SandParticipationCrop(row,state);view.PresentRiding(sandParticipation,crop,new Vector2(crop.x+crop.width*.62f,crop.yMax-6),crop.height,i>1);
                var duration=op=="water"?1.35f:.85f;var age=duration-(cue.until-Time.unscaledTime);view.transform.localRotation=Quaternion.Euler(0,0,state>0?Mathf.Sin(age/duration*Mathf.PI)*-4:0);
            }
        }
        private Rect SandParticipationCrop(int row,int state)
        {
            var rects=new[]{new Rect(45,12,265,348),new Rect(402,19,256,343),new Rect(761,15,280,347),new Rect(48,378,261,334),new Rect(404,362,257,352),new Rect(757,362,296,362),new Rect(48,730,264,344),new Rect(403,731,258,345),new Rect(764,724,283,356),new Rect(48,1087,274,342),new Rect(414,1094,251,335),new Rect(774,1093,290,339)};return rects[row*3+state];
        }
        private void ResetApprovedSand()
        {ClearSandDirectInput();sandDirectSurface=null;sandDirectCue=null;sandDirectCueLabel=null;sandEligibleOutlines.Clear();sandResponseJoined=false;sandResponseRevision=-1;sandResponseRound=-1;sandApprovedPieces.Clear();sandApprovedProps.Clear();sandParticipantCues.Clear();sandParticipantEvents.Clear();if(sandApprovedPanel!=null){Destroy(sandApprovedPanel.texture);Destroy(sandApprovedPanel);}sandApprovedPanel=null;sandParticipation=null;}
    }
    public sealed class SandcastleActivitySurface : UIBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler,ICancelHandler
    {
        public Action<Vector2> Click;
        public Func<int> Generation;private int lease;
        private int? pointer;private Vector2 down;private bool moved;
        public bool LocalPoint(PointerEventData e,out Vector2 point)
        {point=default;return e.pointerCurrentRaycast.gameObject!=null && e.pointerCurrentRaycast.gameObject.GetComponentInParent<SandcastleActivitySurface>()==this && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out point);}
        public static bool Canceled(PointerEventData e)
        {if(e is ExtendedPointerEventData extended && extended.device is Touchscreen touchscreen)return touchscreen.touches.Any(t=>t.touchId.ReadValue()==extended.touchId && t.phase.ReadValue()==UnityEngine.InputSystem.TouchPhase.Canceled);return false;}
        public void OnPointerDown(PointerEventData e){if(pointer.HasValue || e.button!=PointerEventData.InputButton.Left)return;pointer=e.pointerId;lease=Generation?.Invoke()??0;down=e.position;moved=false;}
        public void OnDrag(PointerEventData e){if(pointer==e.pointerId && Vector2.Distance(down,e.position)>EventSystem.current.pixelDragThreshold)moved=true;}
        public void OnPointerUp(PointerEventData e){if(pointer!=e.pointerId)return;pointer=null;if(lease==(Generation?.Invoke()??0) && !moved && !Canceled(e) && LocalPoint(e,out var point))Click?.Invoke(point);}
        public void OnCancel(BaseEventData e){pointer=null;}
        protected override void OnDisable(){pointer=null;base.OnDisable();}
    }
    public sealed class SandcastleTrayDrag : UIBehaviour,IPointerDownHandler,IPointerUpHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler
    {
        public Func<bool> Begin;public Action<PointerEventData> Move,End;public Action Cancel;
        private int? pointer;private bool dragging;
        public void OnPointerDown(PointerEventData e){if(!pointer.HasValue)pointer=e.pointerId;}
        public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId && !dragging)pointer=null;}
        public void OnBeginDrag(PointerEventData e){if(pointer!=e.pointerId)return;e.eligibleForClick=false;dragging=Begin?.Invoke()==true;}
        public void OnDrag(PointerEventData e){if(pointer==e.pointerId && dragging)Move?.Invoke(e);}
        public void OnEndDrag(PointerEventData e){if(pointer!=e.pointerId)return;pointer=null;var active=dragging;dragging=false;e.eligibleForClick=false;if(active)End?.Invoke(e);}
        public void OnCancel(BaseEventData e){pointer=null;dragging=false;Cancel?.Invoke();}
        protected override void OnDisable(){OnCancel(null);base.OnDisable();}
    }
}
