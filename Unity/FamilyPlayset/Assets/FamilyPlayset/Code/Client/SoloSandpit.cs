using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform sandpitRoot,sandpitBelt,sandpitHud,sandpitDemo,sandpitWall,sandShovel;
        private readonly List<SandShape> sandTipCues=new List<SandShape>();
        private readonly List<Image> sandHighlights=new List<Image>();private readonly List<SandShape> sandScoopEffects=new List<SandShape>();
        private SandScoopFeedback sandFeedback=new SandScoopFeedback();
        public int SandpitHighlight=>sandHighlights.FindIndex(h=>h.gameObject.activeSelf);
        public int SandpitApproach=>sandpitApproach;public int SandpitApproachRound=>sandpitApproachRound;
        public int[] SandpitScoopEvents=>sandFeedback.Events;
        private readonly List<RectTransform> sandPlaces=new List<RectTransform>();private readonly List<SandShape> sandShapes=new List<SandShape>();
        private readonly List<(RectTransform root,GameCharacterVisual visual)> sandFriends=new List<(RectTransform,GameCharacterVisual)>();
        private readonly List<Button> sandTools=new List<Button>();private readonly List<AudioClip> sandAudio=new List<AudioClip>();
        private readonly List<Text> sandNumbers=new List<Text>();private Text sandHint,sandSelected;
        private SandShape sandDemoShape;private bool sandpitSending;private int sandpitSelected,sandpitApproach=-1,sandpitApproachRound=-1,sandpitScoopTaps,sandSpoken=-1,sandSpokenRound=-1;
        private string sandpitOperation;private Button sandReplay;
        public SandpitState SandpitGame=>HasWorld?(Shared?shared.View.sandpit:World.ReadSandpit()):null;
        public string[] SandpitNpcArt=>sandFriends.Select(n=>n.visual.CharacterId).ToArray();public int SandpitSelection=>sandpitSelected;
        private bool SandpitOwn=>SandpitGame?.members.Any(m=>m.actor==Actor && m.attending)==true;
        private bool SandpitTeacherActive=>SandpitGame?.phase>0 && SandpitGame.members.Any(m=>m.attending);
        private void ChooseSandpit()
        {CloseMiniGames();CloseNavigation();CloseTeacherCard();Narration.Stop();SendSandpit("start");}
        private void SendSandpit(string op,int index=-1,int expectedRound=-1)
        {
            if(!Ready || TravelPending || sandpitSending && op!="leave")return;
            var target=index<0?"":DaycareSandpit.Target(index,expectedRound);
            sandpitSending=true;sandpitApproach=-1;sandpitApproachRound=-1;sandpitScoopTaps=0;sandpitOperation=null;destination=null;CancelPointers();shared?.Walk(WalkMode.Stop);
            void Done(SoloResult r){sandpitSending=false;if(!r.Accepted && (expectedRound<0 || SandpitOwn && SandpitGame.round==expectedRound)){if(op=="tip" && r.Outcome=="fill-bucket-first")sandTipFeedback.Underfilled(index,expectedRound);homeFeedback.text=r.Outcome=="fill-bucket-first"?"Add scoops until the bucket is full.":"Try another sand tool.";homeFeedbackUntil=Time.unscaledTime+3;}Render();}
            if(Shared){if(!SubmitShared(SoloAction.Sandpit,"",target,op,0,0,Done))sandpitSending=false;}
            else Done(Command(SoloAction.Sandpit,target:target,value:op));
        }
        private void SelectSandMould(int i)
        {if(!SandpitOwn){ChooseSandpit();return;}CancelSandpitIntent();sandpitSelected=i;CancelPointers();manualCamera=false;TickSandpit();}
        private void RequestSandTool(string op)
        {
            if(!Ready || MenuOpen || !SandpitOwn || SandpitGame.phase<1)return;
            // Each completed scoop tap is retained while walking/waiting. Other tools
            // and selection changes replace the intent; no capacity rule lives here.
            if(op=="scoop" && sandpitOperation==op && sandpitApproach==sandpitSelected && sandpitApproachRound==SandpitGame.round){sandpitScoopTaps++;return;}
            // A visible shared result can precede its reply; retain the next tap.
            CancelPointers();sandpitApproach=sandpitSelected;sandpitApproachRound=SandpitGame.round;sandpitScoopTaps=op=="scoop"?1:0;sandpitOperation=op;manualCamera=false;var at=DaycareSandpit.Work(sandpitApproach);destination=new Vector2(at.X,at.Y);
        }
        private void CancelSandpitIntent()
        {
            // Only stop the automatic walk owned by this intent, not a new destination.
            var at=sandpitApproach>=0?DaycareSandpit.Work(sandpitApproach):default;
            var ownsWalk=sandpitApproach>=0 && destination.HasValue && destination.Value==new Vector2(at.X,at.Y);
            sandpitApproach=-1;sandpitApproachRound=-1;sandpitScoopTaps=0;sandpitOperation=null;
            if(ownsWalk){destination=null;shared?.Walk(WalkMode.Stop);}
        }
        private void CheckSandpitInput()
        {
            if(sandpitApproach<0)return;
            if(SandpitGame==null || sandpitApproachRound!=SandpitGame.round){CancelSandpitIntent();return;}
            if(CurrentArea!="daycare" || !SandpitOwn || MenuOpen || applicationPaused || stickDirection.sqrMagnitude>.1f){CancelSandpitIntent();return;}
            if(sandpitSending || ActionPending)return;var at=DaycareSandpit.Work(sandpitApproach);var p=ReadPlayer(Actor);
            if(Math.Abs(p.x-at.X)<=15 && Math.Abs(p.y-at.Y)<=25){
                var op=sandpitOperation;var index=sandpitApproach;var round=sandpitApproachRound;var remaining=op=="scoop"?Math.Max(0,sandpitScoopTaps-1):0;
                SendSandpit(op,index,round);
                // Reuse the same one-command-at-a-time authority path for retained taps.
                if(remaining>0){sandpitApproach=index;sandpitApproachRound=round;sandpitOperation=op;sandpitScoopTaps=remaining;}
            }
        }
        private SandShape DrawSandBucket(Transform parent,Vector2 at,int scoops,int capacity,bool wet)
        {var root=Rect(parent,"Sand bucket picture",at,new Vector2(110,170));var shape=root.gameObject.AddComponent<SandShape>();shape.raycastTarget=false;shape.scoops=scoops;shape.capacity=capacity;shape.wet=wet;return shape;}
        private void BuildSandpit()
        {
            if(SceneSchema<DaycareSandpit.Schema)return;
            sandpitRoot=Rect(Board,"Playable Daycare sandpit",Vector2.zero,new Vector2(780,170));var pit=sandpitRoot.gameObject.AddComponent<SandShape>();pit.kind="pit";pit.raycastTarget=false;
            sandpitWall=Rect(Board,"Shared sandcastle walls",Vector2.zero,new Vector2(520,60));var wall=sandpitWall.gameObject.AddComponent<SandShape>();wall.kind="wall";wall.raycastTarget=false;
            HomeHit(sandpitRoot,"Play sandpit",new Vector2(0,-30),new Vector2(750,65),ChooseSandpit,false);
            sandShovel=Rect(Board,"Sandpit shovel and sand",Vector2.zero,new Vector2(180,160));var shovel=sandShovel.gameObject.AddComponent<SandShape>();shovel.kind="shovel";shovel.raycastTarget=false;
            HomeHit(sandShovel,"Scoop with shovel",new Vector2(0,40),new Vector2(180,150),()=>RequestSandTool("scoop"),false);
            for(var i=0;i<4;i++){
                var index=i;var bucket=DrawSandBucket(Board,Vector2.zero,0,DaycareSandpit.Capacity(i),false);sandShapes.Add(bucket);sandPlaces.Add(bucket.rectTransform);
                var highlight=Panel(bucket.transform,"Your selected sand mould",new Vector2(0,55),new Vector2(145,170),new Color(1,.75f,.1f),false);highlight.sprite=hintRing;highlight.raycastTarget=false;highlight.transform.SetAsFirstSibling();sandHighlights.Add(highlight);
                var cue=Rect(bucket.transform,"Tap selected mould to tip",new Vector2(52,95),new Vector2(65,65)).gameObject.AddComponent<SandShape>();cue.kind="tip-cue";cue.raycastTarget=false;sandTipCues.Add(cue);
                var effect=Rect(Board,"Accepted sand scoop "+(i+1),Vector2.zero,new Vector2(1400,500)).gameObject.AddComponent<SandShape>();effect.kind="sand-transfer";effect.raycastTarget=false;sandScoopEffects.Add(effect);
                sandNumbers.Add(Label(bucket.transform,"",20,new Vector2(0,-22),new Vector2(150,40)));
                HomeHit(bucket.transform,"Sand mould "+(i+1),new Vector2(0,50),new Vector2(140,155),()=>TouchSandMould(index),false);
            }
            BuildSandWater();BuildSandTip();
            sandpitDemo=Rect(Board,"Calypso sand demonstration",Vector2.zero,new Vector2(120,150));sandDemoShape=DrawSandBucket(sandpitDemo,Vector2.zero,0,2,false);
            for(var i=0;i<2;i++){var root=Rect(Board,"Sandpit classmate "+i,Vector2.zero,new Vector2(160,220));var visual=root.gameObject.AddComponent<GameCharacterVisual>();sandFriends.Add((root,visual));var prop=DrawSandBucket(root,new Vector2(62,15),2,2,true);prop.rectTransform.localScale=Vector3.one*.45f;}
            sandpitHud=Panel(safe,"Calypso sand lesson",Vector2.zero,new Vector2(890,83),new Color(1,.97f,.86f,.97f),false).rectTransform;sandpitHud.anchorMin=sandpitHud.anchorMax=new Vector2(.5f,1);sandpitHud.anchoredPosition=new Vector2(0,-158);
            sandHint=Label(sandpitHud,"",25,Vector2.zero,new Vector2(850,80));
            sandpitBelt=Panel(safe,"Sand tools",Vector2.zero,new Vector2(950,165),new Color(1,.97f,.86f,.97f),false).rectTransform;sandpitBelt.anchorMin=sandpitBelt.anchorMax=new Vector2(.5f,0);sandpitBelt.anchoredPosition=new Vector2(0,90);
            for(var i=0;i<4;i++){var index=i;var label=Button(sandpitBelt,(i%2==0?"Small ":"Big ")+(i+1),new Vector2(-315+i*128,50),new Vector2(119,52),()=>SelectSandMould(index),new Color(.8f,.9f,.96f));label.fontSize=24;label.transform.parent.name="Choose sand mould "+(i+1);}
            sandSelected=Label(sandpitBelt,"",20,new Vector2(260,50),new Vector2(340,52));
            var operations=new[]{"scoop","water","tip","flag","shell"};var names=new[]{"Scoop sand","Add water","Tip bucket","Add flag","Add shell"};
            for(var i=0;i<5;i++){var op=operations[i];var label=Button(sandpitBelt,names[i],new Vector2(-370+i*150,-30),new Vector2(142,95),()=>RequestSandTool(op),Color.white);label.fontSize=20;label.rectTransform.anchoredPosition=new Vector2(0,-29);label.rectTransform.sizeDelta=new Vector2(142,33);var icon=Rect(label.transform.parent,"Sand tool picture",new Vector2(0,14),new Vector2(100,65)).gameObject.AddComponent<SandShape>();icon.kind=op;icon.raycastTarget=false;sandTools.Add(label.transform.parent.GetComponent<Button>());}
            Button(sandpitBelt,"Leave",new Vector2(375,-30),new Vector2(135,95),()=>{Narration.Stop();SendSandpit("leave");},new Color(.94f,.87f,.79f)).fontSize=24;
            var replay=Button(sandpitBelt,"New lesson",new Vector2(285,50),new Vector2(315,52),()=>SendSandpit("replay"),new Color(.82f,.91f,.62f));sandReplay=replay.transform.parent.GetComponent<Button>();
            foreach(var id in new[]{"count","water","tip","play","crumble","done"}){var clip=Resources.Load<AudioClip>("Sandpit/"+id);if(clip!=null){sandAudio.Add(clip);Narration.AddClip("sand-"+id,clip);}}
            TickSandpit();
        }
        private void TickSandpit()
        {
            if(sandpitRoot==null)return;var g=SandpitGame;var visible=CurrentArea=="daycare";var joined=visible && SandpitOwn && !MenuOpen;
            if(sandpitApproach>=0 && sandpitApproachRound!=g.round)CancelSandpitIntent();
            if(joined && sandSpokenRound!=g.round){sandSpokenRound=g.round;sandSpoken=-1;sandpitSelected=Math.Max(0,Array.FindIndex(g.members,m=>m.actor==Actor));}
            sandFeedback.Observe(g,joined && !applicationPaused,Time.unscaledDeltaTime);
            sandShovel.gameObject.SetActive(joined);sandShovel.anchoredPosition=ToBoard(4100,220);sandShovel.localScale=Vector3.one*sceneScale;
            sandpitRoot.gameObject.SetActive(visible);sandpitRoot.anchoredPosition=ToBoard(4460,440);sandpitRoot.localScale=Vector3.one*sceneScale;
            sandpitWall.gameObject.SetActive(visible && g.phase==3);sandpitWall.anchoredPosition=ToBoard(4450,440)+new Vector2(0,16)*sceneScale;sandpitWall.localScale=Vector3.one*sceneScale;
            sandpitBelt.gameObject.SetActive(joined);sandpitHud.gameObject.SetActive(joined);var scale=Mathf.Min(1,safe.rect.width/1050);sandpitBelt.localScale=sandpitHud.localScale=Vector3.one*scale;
            for(var i=0;i<4;i++){var m=g.moulds[i];var shape=sandShapes[i];var at=DaycareSandpit.Place(i);shape.gameObject.SetActive(visible);shape.rectTransform.anchoredPosition=ToBoard(at.X,at.Y)+new Vector2(0,16)*sceneScale;shape.rectTransform.localScale=Vector3.one*sceneScale;shape.scoops=m.scoops;shape.capacity=DaycareSandpit.Capacity(i);shape.wet=m.wet;shape.built=m.built;shape.decoration=m.decoration;shape.SetVerticesDirty();sandNumbers[i].text=m.built?"Tower "+(i+1):(i%2==0?"Small":"Big")+" · "+m.scoops+" / "+shape.capacity;}
            for(var i=0;i<4;i++){
                sandHighlights[i].gameObject.SetActive(joined && i==sandpitSelected);
                sandTipCues[i].gameObject.SetActive(joined && i==sandpitSelected && !g.moulds[i].built);
                var effect=sandScoopEffects[i];effect.gameObject.SetActive(joined && sandFeedback.Remaining(i)>0);
                effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one*sceneScale;
                var at=DaycareSandpit.Place(i);effect.source=new Vector2(4100-at.X,(220-at.Y)*.45f-16);effect.target=new Vector2(0,DaycareSandpit.Capacity(i)==3?75:55);effect.progress=1-sandFeedback.Remaining(i)/SandScoopFeedback.Duration;effect.SetVerticesDirty();
            }
            TickSandWater(g,joined);TickSandTip(g,joined);
            var demo=DaycareSandpit.Demonstration(g,calypsoClock);sandpitDemo.gameObject.SetActive(visible && SandpitTeacherActive && demo>=0);sandpitDemo.anchoredPosition=ToBoard(3890,440)+new Vector2(0,10)*sceneScale;sandpitDemo.localScale=Vector3.one*sceneScale*.75f;sandDemoShape.scoops=demo<1?0:demo<2?1:2;sandDemoShape.wet=demo>=4.2;sandDemoShape.built=demo>=7;sandDemoShape.SetVerticesDirty();
            for(var i=0;i<2;i++){var n=sandFriends[i];n.root.gameObject.SetActive(visible && g.phase>0);if(!visible || g.phase==0)continue;n.visual.Select(g.friends[i]);n.root.anchoredPosition=ToBoard(4260+i*320,500);n.root.localScale=Vector3.one*sceneScale*.65f;n.visual.PresentFrame(new CharacterFrame(g.phase==3?CharacterPose.Dance:CharacterPose.Sit,0,i==1,(float)calypsoClock),Mathf.Min(Time.unscaledDeltaTime,.1f));}
            if(!joined){sandSpoken=-1;return;}
            sandHint.text=DaycareSandpit.Hint(g,calypsoClock);var selected=g.moulds[sandpitSelected];sandSelected.text="Bucket "+(sandpitSelected+1)+" · "+(selected.built?"Decorate your tower!":selected.wet?"Damp sand":"Dry sand");
            for(var i=0;i<5;i++)sandTools[i].interactable=g.phase>=1 && (i<3?!selected.built:selected.built);
            sandSelected.gameObject.SetActive(g.phase!=3);sandReplay.gameObject.SetActive(g.phase==3);sandReplay.interactable=!sandpitSending;
            var cue=g.phase==3?5:g.phase>=2?4:demo<0?-1:demo<4.2?0:demo<7?1:2;
            if(cue>=0 && cue!=sandSpoken){sandSpoken=cue;Narration.Speak("sand-"+new[]{"count","water","tip","tip","play","done"}[cue]);}
        }
        private void AddSandpitDepth(Action<RectTransform,float,int,string> add)
        {if(sandpitRoot==null || !sandpitRoot.gameObject.activeInHierarchy)return;AddSandWaterDepth(add);AddSandTipDepth(add);add(sandpitRoot,sandpitRoot.anchoredPosition.y+95*sceneScale,0,"sandpit");if(sandShovel.gameObject.activeSelf)add(sandShovel,sandShovel.anchoredPosition.y,3,"sand-shovel");foreach(var effect in sandScoopEffects)if(effect.gameObject.activeSelf)add(effect.rectTransform,sandShovel.anchoredPosition.y,4,"sand-transfer");if(sandpitWall.gameObject.activeSelf)add(sandpitWall,sandpitWall.anchoredPosition.y+1,0,"castle-walls");foreach(var root in sandPlaces)add(root,root.anchoredPosition.y,3,"sand-mould");foreach(var n in sandFriends)if(n.root.gameObject.activeSelf)add(n.root,n.root.anchoredPosition.y,2,"sand-friend");if(sandpitDemo.gameObject.activeSelf)add(sandpitDemo,sandpitDemo.anchoredPosition.y,4,"teacher-demo");}
        private void ResetSandpit()
        {ResetSandWater();ResetSandTip();sandpitRoot=sandpitBelt=sandpitHud=sandpitDemo=sandpitWall=sandShovel=null;sandPlaces.Clear();sandShapes.Clear();sandHighlights.Clear();sandTipCues.Clear();sandScoopEffects.Clear();sandFeedback=new SandScoopFeedback();sandFriends.Clear();sandTools.Clear();sandNumbers.Clear();sandpitSending=false;sandpitApproach=sandpitApproachRound=-1;sandpitScoopTaps=0;sandpitOperation=null;sandSpoken=sandSpokenRound=-1;foreach(var clip in sandAudio)if(clip!=null)Resources.UnloadAsset(clip);sandAudio.Clear();}
    }
}
