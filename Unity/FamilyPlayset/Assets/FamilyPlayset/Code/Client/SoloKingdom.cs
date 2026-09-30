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
        private readonly List<(RectTransform root,GameCharacterVisual visual)> kingdomNpcs=new List<(RectTransform,GameCharacterVisual)>();
        private readonly Dictionary<string,RectTransform> kingdomProps=new Dictionary<string,RectTransform>();
        private RectTransform kingdomHud,kingdomControls;
        private Text kingdomHint,kingdomRole;
        private Button kingdomReplay;private Button kingdomNext;
        private string kingdomApproach="",kingdomOperation="";
        private bool kingdomSending;
        private int kingdomSpoken=-1,kingdomSpokenRound=-1;
        private readonly List<AudioClip> kingdomAudio=new List<AudioClip>();
        public KingdomState KingdomGame=>HasWorld?(Shared?shared.View.kingdom:World.ReadKingdom()):null;
        public int VisibleKingdomNpcs=>kingdomNpcs.Count(v=>v.root!=null && v.root.gameObject.activeInHierarchy);
        private KingdomMember KingdomOwn=>KingdomGame?.members.FirstOrDefault(m=>m.actor==Actor);
        private bool KingdomArea=>CurrentArea==KingdomAdventure.Zone;
        private void SendKingdom(string op,string target="")
        {
            if(!Ready || TravelPending || kingdomSending)return;
            kingdomSending=true;destination=null;kingdomApproach="";manualCamera=false;CancelPointers();shared?.Walk(WalkMode.Stop);
            void Done(SoloResult result){kingdomSending=false;if(!result.Accepted){homeFeedback.text="Try the glowing picture again.";homeFeedbackUntil=Time.unscaledTime+2;}Render();}
            if(Shared){if(!SubmitShared(SoloAction.Kingdom,"",target,op,0,0,Done))kingdomSending=false;}
            else Done(Command(SoloAction.Kingdom,target:target,value:op));
        }
        private void ChooseKingdom()
        {CloseMiniGames();CloseNavigation();Narration.Stop();SendKingdom("start");}
        private void RequestKingdom(string target,string op)
        {
            if(!Ready || MenuOpen || kingdomSending || KingdomOwn?.attending!=true)return;
            var point=KingdomAdventure.Prop(target);kingdomApproach=target;kingdomOperation=op;manualCamera=false;destination=new Vector2(point.X,point.Y);
        }
        private void CheckKingdomInput()
        {
            if(kingdomApproach=="")return;
            if(!KingdomArea || MenuOpen || applicationPaused || stickDirection.sqrMagnitude>.1f){kingdomApproach="";destination=null;return;}
            var p=ReadPlayer(Actor);var point=KingdomAdventure.Prop(kingdomApproach);
            if(Math.Abs(p.x-point.X)>65 || Math.Abs(p.y-point.Y)>65)return;
            var target=kingdomApproach;var op=kingdomOperation;SendKingdom(op,target);
        }
        private void BuildKingdom()
        {
            if(SceneSchema<KingdomAdventure.Schema)return;
            for(var i=0;i<KingdomAdventure.Cast.Length;i++){
                var root=Rect(Board,"Adventure NPC "+KingdomAdventure.Cast[i],Vector2.zero,new Vector2(160,220));
                var visual=root.gameObject.AddComponent<GameCharacterVisual>();visual.Select(KingdomAdventure.Cast[i]);
                kingdomNpcs.Add((root,visual));
                if(i==0 || i==5){var crown=Panel(root,"Pretend crown",new Vector2(0,142),new Vector2(80,20),new Color(1,.82f,.24f),false);for(var j=0;j<3;j++)Panel(crown.transform,"Crown point",new Vector2((j-1)*27,14),new Vector2(18,25),new Color(1,.82f,.24f),false);}
                if(i>=6){var index=i-6;HomeHit(root,"Wake friend "+(index+1),new Vector2(0,65),new Vector2(180,225),()=>RequestKingdom("friend-"+index,"wake"),false);}
                root.gameObject.SetActive(false);
            }
            foreach(var id in new[]{"fruit-0","fruit-1","fruit-2","board-0","board-1","board-2","ball","wand"}){
                var key=id;var op=id.StartsWith("fruit")?"fruit":id.StartsWith("board")?"board":id=="ball"?"toss":"wand";
                var root=Rect(Board,"Adventure prop "+id,Vector2.zero,new Vector2(145,145));kingdomProps.Add(id,root);
                Panel(root,"Useful story prop glow",new Vector2(0,45),new Vector2(155,145),new Color(1,.93f,.45f,.24f),false,true);
                if(id.StartsWith("fruit"))HomePicture(root,"Picnic fruit",new Vector2(0,45),new Vector2(125,125),IngredientSprite(id=="fruit-0"?"banana":id=="fruit-1"?"strawberry":"pineapple")).preserveAspect=true;
                else if(id.StartsWith("board")){var plank=Panel(root,"Bridge plank",new Vector2(0,40),new Vector2(150,45),new Color(.71f,.44f,.22f),false);plank.rectTransform.localRotation=Quaternion.Euler(0,0,-12);Plain(plank.transform,"Wood grain",Vector2.zero,new Vector2(125,4),new Color(.48f,.29f,.14f));}
                else if(id=="ball"){Panel(root,"Magic tennis ball",new Vector2(0,40),new Vector2(95,95),new Color(.79f,.96f,.26f),false,true);Plain(root,"Tennis ball seam",new Vector2(0,40),new Vector2(72,5),Color.white);}
                else{var wand=Plain(root,"Magic wand",new Vector2(0,45),new Vector2(13,110),new Color(.68f,.38f,.9f));wand.rectTransform.localRotation=Quaternion.Euler(0,0,-25);Panel(root,"Wand sparkle",new Vector2(23,98),new Vector2(42,42),new Color(1,.83f,.19f),false,true);}
                HomeHit(root,id=="ball"?"Toss magic ball":id=="wand"?"Take magic wand":id.StartsWith("fruit")?"Gather fruit "+(int.Parse(id.Substring(6))+1):"Place bridge plank "+(int.Parse(id.Substring(6))+1),new Vector2(0,50),new Vector2(175,170),()=>RequestKingdom(key,op),false);
                root.gameObject.SetActive(false);
            }
            kingdomHud=Panel(safe,"Adventure story hint",Vector2.zero,new Vector2(780,94),new Color(1,.98f,.88f,.97f),false).rectTransform;
            kingdomHud.anchorMin=kingdomHud.anchorMax=new Vector2(.5f,1);kingdomHud.anchoredPosition=new Vector2(0,-155);
            kingdomHint=Label(kingdomHud,"",30,new Vector2(-50,0),new Vector2(620,86));
            var next=Button(kingdomHud,">",new Vector2(320,0),new Vector2(100,76),KingdomNextTask,new Color(.82f,.91f,.62f));next.fontSize=44;kingdomNext=next.transform.parent.GetComponent<Button>();kingdomNext.name="Next adventure task";
            kingdomControls=Rect(safe,"Adventure controls",Vector2.zero,new Vector2(860,80));kingdomControls.anchorMin=kingdomControls.anchorMax=new Vector2(.5f,0);kingdomControls.anchoredPosition=new Vector2(0,65);
            kingdomRole=Button(kingdomControls,"Change role",new Vector2(-290,0),new Vector2(235,70),()=>SendKingdom(KingdomOwn?.attending==true?"role":"start"),new Color(.81f,.89f,.99f));kingdomRole.fontSize=24;
            Button(kingdomControls,"Listen again",new Vector2(-50,0),new Vector2(215,70),SpeakKingdom,Color.white).fontSize=24;
            Button(kingdomControls,"Return to daycare",new Vector2(202,0),new Vector2(275,70),()=>{Narration.Stop();SendKingdom("leave");},Color.white).fontSize=24;
            var replay=Button(kingdomHud,"Play again",new Vector2(0,-95),new Vector2(185,70),()=>SendKingdom("replay"),new Color(.85f,.92f,.63f));replay.fontSize=23;kingdomReplay=replay.transform.parent.GetComponent<Button>();
            for(var i=1;i<=6;i++){var clip=Resources.Load<AudioClip>("Adventure/step-"+i);if(clip!=null){kingdomAudio.Add(clip);Narration.AddClip("kingdom-"+i,clip);}}
            TickKingdom();
        }
        private void KingdomNextTask()
        {
            var g=KingdomGame;if(g==null)return;
            if(g.phase==KingdomPhase.Supplies){for(var i=0;i<3;i++)if((g.supplies&(1<<i))==0){RequestKingdom("fruit-"+i,"fruit");return;}}
            if(g.phase==KingdomPhase.Bridge){for(var i=0;i<3;i++)if((g.boards&(1<<i))==0){RequestKingdom("board-"+i,"board");return;}}
            if(g.phase==KingdomPhase.Queen){RequestKingdom(g.distractedUntil>g.clock?"wand":"ball",g.distractedUntil>g.clock?"wand":"toss");return;}
            if(g.phase==KingdomPhase.Rescue){for(var i=0;i<3;i++)if((g.rescued&(1<<i))==0){RequestKingdom("friend-"+i,"wake");return;}}
        }
        private void SpeakKingdom(){if(KingdomGame!=null && KingdomArea)Narration.Speak("kingdom-"+(int)KingdomGame.phase);}
        private void TickKingdom()
        {
            if(kingdomHud==null)return;var g=KingdomGame;var visible=KingdomArea && g!=null;
            kingdomHud.gameObject.SetActive(visible);kingdomControls.gameObject.SetActive(visible);
            var scale=Mathf.Min(1,safe.rect.width/1100);kingdomHud.localScale=kingdomControls.localScale=Vector3.one*scale;
            for(var i=0;i<kingdomNpcs.Count;i++){
                var npc=kingdomNpcs[i];npc.root.gameObject.SetActive(visible);if(!visible)continue;
                var point=KingdomAdventure.NpcPoint(g,i);npc.root.anchoredPosition=ToBoard(point.X,point.Y);npc.root.localScale=Vector3.one*sceneScale*.85f;
                var frozen=KingdomAdventure.Frozen(g,i);npc.visual.Present(npc.root.anchoredPosition,"kingdom/"+g.round+"/"+i,false,Time.unscaledDeltaTime);
                if(frozen || g.phase==KingdomPhase.Feast && g.clock-g.started>=5 || i==0 && g.phase==KingdomPhase.Welcome || i==1 && g.phase==KingdomPhase.Supplies || i>=2 && i<=4 && g.phase==KingdomPhase.Bridge)
                    npc.visual.PresentFrame(new CharacterFrame(frozen?CharacterPose.Idle:g.phase==KingdomPhase.Feast?CharacterPose.Dance:CharacterPose.Wave,0,i%2==0,(float)(g.clock-g.started)),Time.unscaledDeltaTime);
                foreach(var graphic in npc.visual.ActiveView.GetComponentsInChildren<Image>())graphic.color=frozen?new Color(.55f,.76f,.95f):Color.white;
            }
            foreach(var pair in kingdomProps){var id=pair.Key;var active=visible && (id.StartsWith("fruit")?g.phase==KingdomPhase.Supplies && (g.supplies&(1<<int.Parse(id.Substring(6))))==0:id.StartsWith("board")?g.phase==KingdomPhase.Bridge && (g.boards&(1<<int.Parse(id.Substring(6))))==0:id=="ball"?g.phase==KingdomPhase.Queen:g.phase==KingdomPhase.Queen && g.distractedUntil>g.clock);
                pair.Value.gameObject.SetActive(active);if(active){var point=KingdomAdventure.Prop(id);var x=point.X;var y=point.Y;if(id=="ball" && g.distractedUntil>g.clock){var t=Mathf.Clamp01((float)(8-(g.distractedUntil-g.clock))/.8f);x=Mathf.Lerp(x,2200,t);y+=Mathf.Sin(t*Mathf.PI)*250;}pair.Value.anchoredPosition=ToBoard(x,y);pair.Value.localScale=Vector3.one*sceneScale;}}
            if(!visible){kingdomSpoken=-1;kingdomApproach="";return;}
            var lines=new[]{"","Our kingdom needs a picnic! Let's help together.","Tap the fruit to gather our picnic supplies.","Tap the planks to open our bridge.",g.distractedUntil>g.clock?"The queen is watching the ball. Take the wand!":"Toss the magic ball, then take the queen's wand.","Tap our frozen friends to wake them with the wand.","You saved the kingdom! Let's have a picnic together."};
            kingdomHint.text=g.phase==KingdomPhase.Welcome?"The Adventure · "+Math.Max(1,3-(int)(g.clock-g.started))+"\n"+lines[(int)g.phase]:lines[(int)g.phase];
            kingdomRole.text=KingdomOwn?.attending==true?KingdomAdventure.Roles[KingdomOwn.role]:"Join adventure";
            kingdomNext.gameObject.SetActive(g.phase>=KingdomPhase.Supplies && g.phase<KingdomPhase.Feast);
            kingdomReplay.gameObject.SetActive(g.phase==KingdomPhase.Feast);kingdomReplay.interactable=!kingdomSending && g.clock-g.started>=6;
            if(kingdomSpoken!=(int)g.phase || kingdomSpokenRound!=g.round){kingdomSpoken=(int)g.phase;kingdomSpokenRound=g.round;Narration.Stop();SpeakKingdom();}
        }
        private void AddKingdomDepth(Action<RectTransform,float,int,string> add)
        {foreach(var npc in kingdomNpcs)if(npc.root.gameObject.activeSelf)add(npc.root,npc.root.anchoredPosition.y,3,npc.root.name);foreach(var prop in kingdomProps)if(prop.Value.gameObject.activeSelf)add(prop.Value,prop.Value.anchoredPosition.y,3,prop.Key);}
        private void ResetKingdom()
        {kingdomNpcs.Clear();kingdomProps.Clear();kingdomHud=null;kingdomControls=null;kingdomApproach="";kingdomSending=false;kingdomSpoken=-1;foreach(var clip in kingdomAudio)if(clip!=null)Resources.UnloadAsset(clip);kingdomAudio.Clear();}
    }
}
