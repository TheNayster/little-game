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
        private readonly List<CharacterMotion> kingdomNpcMotion=new List<CharacterMotion>();
        private readonly List<Vector2> kingdomNpcPoints=new List<Vector2>();
        private readonly List<Image> kingdomNpcCargo=new List<Image>();
        private readonly Dictionary<string,Image> kingdomHolds=new Dictionary<string,Image>();
        private readonly List<Image> kingdomBasketFruit=new List<Image>();
        private readonly List<RectTransform> kingdomBridge=new List<RectTransform>();
        private RectTransform kingdomBasket;private Image kingdomApple;private Text kingdomGuide;
        private RectTransform kingdomStory;private RawImage kingdomQueenPortrait;private int kingdomPortraitRound=-1;
        private int kingdomFrame=-1,kingdomClockRound=-1,kingdomClockPhase=-1;private double kingdomClockSample;private float kingdomClockAt;
        public KingdomState KingdomGame=>HasWorld?(Shared?shared.View.kingdom:World.ReadKingdom()):null;
        public string[] KingdomNpcArt=>kingdomNpcs.Select(n=>n.visual.CharacterId).ToArray();
        public int VisibleKingdomNpcs=>kingdomNpcs.Count(v=>v.root!=null && v.root.gameObject.activeInHierarchy);
        public string[] KingdomNpcJobs=>KingdomGame==null?Array.Empty<string>():Enumerable.Range(0,9).Select(i=>KingdomAdventure.NpcJob(KingdomGame,i,KingdomGame.clock)).ToArray();
        public string[] KingdomNpcPoses=>kingdomNpcs.Select(n=>n.visual.Frame.Pose.ToString()).ToArray();
        public Vector2[] KingdomNpcPoints=>kingdomNpcPoints.ToArray();
        public string KingdomApproach=>kingdomApproach;
        private KingdomMember KingdomOwn=>KingdomGame?.members.FirstOrDefault(m=>m.actor==Actor);
        private bool KingdomArea=>CurrentArea==KingdomAdventure.Zone;
        private void SendKingdom(string op,string target="")
        {
            if(!Ready || TravelPending || kingdomSending && op!="leave")return;
            kingdomSending=true;destination=null;kingdomApproach="";manualCamera=false;CancelPointers();shared?.Walk(WalkMode.Stop);
            void Done(SoloResult result){kingdomSending=false;if(!result.Accepted){homeFeedback.text=result.Outcome=="fruit-already-taken"?"Your friend has that fruit. Pick another!":result.Outcome=="bring-fruit-to-basket"?"Bring your fruit to the basket first.":"Try the glowing picture again.";homeFeedbackUntil=Time.unscaledTime+2;}Render();}
            if(Shared){if(!SubmitShared(SoloAction.Kingdom,"",target,op,0,0,Done))kingdomSending=false;}
            else Done(Command(SoloAction.Kingdom,target:target,value:op));
        }
        private void ChooseKingdom()
        {CloseMiniGames();CloseNavigation();Narration.Stop();SendKingdom("start");}
        private void RequestKingdom(string target,string op)
        {
            if(!Ready || MenuOpen || KingdomOwn?.attending!=true)return;
            CloseKingdomTalk();CancelPointers();var point=KingdomAdventure.ActionPoint(KingdomGame,target);kingdomApproach=target;kingdomOperation=op;manualCamera=false;destination=new Vector2(point.X,point.Y);
        }
        private void CheckKingdomInput()
        {
            if(kingdomApproach=="")return;
            if(!KingdomArea || MenuOpen || applicationPaused || stickDirection.sqrMagnitude>.1f){kingdomApproach="";destination=null;return;}
            // Preserve the next pictured action while the earlier pickup reply
            // catches up with the visible shared hold. Never send it twice.
            if(kingdomSending || ActionPending)return;
            var p=ReadPlayer(Actor);var point=KingdomAdventure.ActionPoint(KingdomGame,kingdomApproach);
            destination=new Vector2(point.X,point.Y);
            if(Math.Abs(p.x-point.X)>65 || Math.Abs(p.y-point.Y)>65)return;
            var target=kingdomApproach;var op=kingdomOperation;if(op=="visit"){kingdomApproach="";destination=null;shared?.Walk(WalkMode.Stop);return;}if(op=="talk"){kingdomApproach="";destination=null;shared?.Walk(WalkMode.Stop);ShowKingdomTalk(int.Parse(target.Substring(4)));}else SendKingdom(op,target);
        }
        private void BuildKingdom()
        {
            if(SceneSchema<KingdomAdventure.Schema)return;
            for(var i=0;i<9;i++){
                var root=Rect(Board,"Adventure NPC role "+i,Vector2.zero,new Vector2(160,220));
                root.gameObject.SetActive(false);
                var visual=root.gameObject.AddComponent<GameCharacterVisual>();visual.Select(KingdomGame.npcCast[i]);
                kingdomNpcs.Add((root,visual));
                kingdomNpcMotion.Add(new CharacterMotion());kingdomNpcPoints.Add(Vector2.zero);
                var cargo=HomePicture(root,"Friend's story prop",new Vector2(40,60),new Vector2(75,65),IngredientSprite("banana"));kingdomNpcCargo.Add(cargo);
                if(i==0 || i==5){var crown=Panel(root,"Pretend crown",new Vector2(0,142),new Vector2(80,20),new Color(1,.82f,.24f),false);for(var j=0;j<3;j++)Panel(crown.transform,"Crown point",new Vector2((j-1)*27,14),new Vector2(18,25),new Color(1,.82f,.24f),false);}
                if(i>=6){var index=i-6;HomeHit(root,"Wake friend "+(index+1),new Vector2(0,65),new Vector2(180,225),()=>RequestKingdom("friend-"+index,"wake"),false);}
                var friendIndex=i;HomeHit(root,"Talk to adventure friend "+(i+1),new Vector2(0,192),new Vector2(155,55),()=>RequestKingdom("npc-"+friendIndex,"talk"),false);
                root.gameObject.SetActive(false);
            }
            kingdomGuide=Label(kingdomNpcs[0].root,"Follow me!",20,new Vector2(0,205),new Vector2(230,65));
            kingdomBasket=Rect(Board,"Shared picnic basket",Vector2.zero,new Vector2(190,130));
            Panel(kingdomBasket,"Basket glow",new Vector2(0,35),new Vector2(185,135),new Color(1,.93f,.45f,.25f),false,true);
            var bowl=Panel(kingdomBasket,"Basket",new Vector2(0,20),new Vector2(160,75),new Color(.72f,.44f,.22f),false);Plain(bowl.transform,"Basket weave",Vector2.zero,new Vector2(145,6),new Color(.9f,.68f,.39f));
            for(var i=0;i<3;i++)kingdomBasketFruit.Add(HomePicture(kingdomBasket,"Packed fruit "+i,new Vector2((i-1)*48,62),new Vector2(56,56),IngredientSprite(i==0?"banana":i==1?"strawberry":"pineapple")));
            kingdomApple=Panel(kingdomBasket,"Helper's apple",new Vector2(64,63),new Vector2(29,29),new Color(.88f,.26f,.25f),false,true);
            HomeHit(kingdomBasket,"Put fruit in basket",new Vector2(0,40),new Vector2(200,150),()=>RequestKingdom("basket","deliver"),false);
            for(var i=0;i<3;i++){var plank=Panel(Board,"Built bridge plank "+i,Vector2.zero,new Vector2(108,60),new Color(.68f,.43f,.23f),false).rectTransform;kingdomBridge.Add(plank);Plain(plank,"Plank grain",Vector2.zero,new Vector2(85,4),new Color(.48f,.29f,.14f));}
            foreach(var member in KingdomGame.members){var held=HomePicture(Board,"Carried Adventure prop "+member.actor,Vector2.zero,new Vector2(75,75),IngredientSprite("banana"));kingdomHolds.Add(member.actor,held);}
            foreach(var id in new[]{"fruit-0","fruit-1","fruit-2","board-0","board-1","board-2","ball","wand"}){
                var key=id;var op=id.StartsWith("fruit")?"fruit":id.StartsWith("board")?"board":id=="ball"?"toss":"wand";
                var root=Rect(Board,"Adventure prop "+id,Vector2.zero,new Vector2(145,145));kingdomProps.Add(id,root);
                Panel(root,"Useful story prop glow",new Vector2(0,45),new Vector2(155,145),new Color(1,.93f,.45f,.24f),false,true);
                if(id.StartsWith("fruit"))HomePicture(root,"Picnic fruit",new Vector2(0,45),new Vector2(125,125),IngredientSprite(id=="fruit-0"?"banana":id=="fruit-1"?"strawberry":"pineapple")).preserveAspect=true;
                else if(id.StartsWith("board")){var plank=Panel(root,"Bridge plank",new Vector2(0,40),new Vector2(150,45),new Color(.71f,.44f,.22f),false);plank.rectTransform.localRotation=Quaternion.Euler(0,0,-12);Plain(plank.transform,"Wood grain",Vector2.zero,new Vector2(125,4),new Color(.48f,.29f,.14f));}
                else if(id=="ball"){Panel(root,"Magic tennis ball",new Vector2(0,40),new Vector2(95,95),new Color(.79f,.96f,.26f),false,true);Plain(root,"Tennis ball seam",new Vector2(0,40),new Vector2(72,5),Color.white);}
                else{var wand=Plain(root,"Magic wand",new Vector2(0,45),new Vector2(13,110),new Color(.68f,.38f,.9f));wand.rectTransform.localRotation=Quaternion.Euler(0,0,-25);Panel(root,"Wand sparkle",new Vector2(23,98),new Vector2(42,42),new Color(1,.83f,.19f),false,true);}
                HomeHit(root,id=="ball"?"Toss magic ball":id=="wand"?"Take magic wand":id.StartsWith("fruit")?"Gather fruit "+(int.Parse(id.Substring(6))+1):"Place bridge plank "+(int.Parse(id.Substring(6))+1),new Vector2(0,50),new Vector2(175,170),()=>RequestKingdom(key,key.StartsWith("board") && KingdomGame.crossing==2?"hop":op),false);
                root.gameObject.SetActive(false);
            }
            kingdomHud=Panel(safe,"Adventure story hint",Vector2.zero,new Vector2(780,94),new Color(1,.98f,.88f,.97f),false).rectTransform;
            kingdomStory=Panel(safe,"The Adventure story opening",Vector2.zero,new Vector2(850,440),new Color(1,.98f,.9f,.99f),true).rectTransform;
            Label(kingdomStory,"The kingdom of frozen friends",38,new Vector2(0,165),new Vector2(760,60));
            kingdomQueenPortrait=Rect(kingdomStory,"Our queen",new Vector2(-300,-85),new Vector2(160,210)).gameObject.AddComponent<RawImage>();kingdomQueenPortrait.raycastTarget=false;
            Label(kingdomStory,"The queen needs your help!\nThree friends are frozen beyond the river.\n\nExplore the orchard, meet the river helpers and find out why the other queen took the magic wand. How will YOU save the kingdom?",28,new Vector2(105,15),new Vector2(580,240));
            Button(kingdomStory,"Listen to the story",new Vector2(-170,-155),new Vector2(300,70),SpeakKingdom,Color.white).fontSize=25;
            Button(kingdomStory,"Let's go!",new Vector2(175,-155),new Vector2(300,70),()=>SendKingdom("begin"),new Color(.82f,.91f,.62f)).fontSize=28;
            kingdomHud.anchorMin=kingdomHud.anchorMax=new Vector2(.5f,1);kingdomHud.anchoredPosition=new Vector2(0,-155);
            kingdomHint=Label(kingdomHud,"",30,new Vector2(-50,0),new Vector2(620,86));
            var next=Button(kingdomHud,">",new Vector2(320,0),new Vector2(100,76),KingdomNextTask,new Color(.82f,.91f,.62f));next.fontSize=44;kingdomNext=next.transform.parent.GetComponent<Button>();kingdomNext.name="Next adventure task";
            kingdomControls=Rect(safe,"Adventure controls",Vector2.zero,new Vector2(860,80));kingdomControls.anchorMin=kingdomControls.anchorMax=new Vector2(.5f,0);kingdomControls.anchoredPosition=new Vector2(0,65);
            kingdomRole=Button(kingdomControls,"Change role",new Vector2(-290,0),new Vector2(235,70),()=>SendKingdom(KingdomOwn?.attending==true?"role":"start"),new Color(.81f,.89f,.99f));kingdomRole.fontSize=24;
            Button(kingdomControls,"Listen again",new Vector2(-50,0),new Vector2(215,70),SpeakKingdom,Color.white).fontSize=24;
            Button(kingdomControls,"Return to daycare",new Vector2(202,0),new Vector2(275,70),()=>{Narration.Stop();SendKingdom("leave");},Color.white).fontSize=24;
            var replay=Button(kingdomHud,"Play again",new Vector2(0,-170),new Vector2(220,60),()=>SendKingdom("replay"),new Color(.85f,.92f,.63f));replay.fontSize=23;kingdomReplay=replay.transform.parent.GetComponent<Button>();
            for(var i=1;i<=6;i++){var clip=Resources.Load<AudioClip>("Adventure/step-"+i);if(clip!=null){kingdomAudio.Add(clip);Narration.AddClip("kingdom-"+i,clip);}}
            BuildKingdomStoryControls();
            TickKingdom();
        }
        private void KingdomNextTask()
        {
            var g=KingdomGame;if(g==null)return;
            if(g.phase==KingdomPhase.Supplies){if(!string.IsNullOrEmpty(KingdomOwn?.carrying)){RequestKingdom("basket","deliver");return;}for(var i=0;i<3;i++)if((g.supplies&(1<<i))==0 && !g.members.Any(m=>m.carrying=="fruit-"+i)){RequestKingdom("fruit-"+i,"fruit");return;}}
            if(g.phase==KingdomPhase.Bridge){if(g.crossing==0){RequestKingdom("npc-2","talk");return;}for(var i=0;i<3;i++)if((g.boards&(1<<i))==0){RequestKingdom("board-"+i,g.crossing==2?"hop":"board");return;}}
            if(g.phase==KingdomPhase.Queen){if(g.queenPlan==0){RequestKingdom("npc-5","talk");return;}var wand=g.distractedUntil>g.clock || g.queenPlan==2 && g.clock-g.queenAt>=4;if(g.queenPlan==2 && !wand)return;RequestKingdom(wand?"wand":"ball",wand?"wand":"toss");return;}
            if(g.phase==KingdomPhase.Rescue){for(var i=0;i<3;i++)if((g.rescued&(1<<i))==0){RequestKingdom("friend-"+i,"wake");return;}}
        }
        private void SpeakKingdom(){if(KingdomGame!=null && KingdomArea)Narration.Speak(KingdomGame.phase==KingdomPhase.Feast?"kingdom-"+(KingdomGame.queenPlan==2?"ending-kind":"ending-ball"):"kingdom-"+(int)KingdomGame.phase);}
        private void TickKingdom()
        {
            if(kingdomHud==null)return;var g=KingdomGame;var visible=KingdomArea && g!=null;
            if(kingdomFrame==Time.frameCount)return;kingdomFrame=Time.frameCount;
            // Extrapolate only a bounded presentation clock. Packet-sized jumps
            // must not alternate idle/walk; gameplay still uses the server clock.
            if(g!=null && (g.clock!=kingdomClockSample || g.round!=kingdomClockRound || (int)g.phase!=kingdomClockPhase)){kingdomClockSample=g.clock;kingdomClockAt=Time.unscaledTime;kingdomClockRound=g.round;kingdomClockPhase=(int)g.phase;}
            var clock=g==null?0:g.clock+(Shared && shared.Connected?Math.Min(.35,Time.unscaledTime-kingdomClockAt):0);var dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
            kingdomHud.gameObject.SetActive(visible);kingdomControls.gameObject.SetActive(visible);
            var opening=visible && g.phase==KingdomPhase.Welcome && !MenuOpen;kingdomStory.gameObject.SetActive(opening);kingdomStory.localScale=Vector3.one*Mathf.Min(safe.rect.width/900,safe.rect.height/530);
            if(opening){kingdomStory.SetAsLastSibling();if(kingdomPortraitRound!=g.round){var portrait=Resources.Load<CharacterMenuArt>("CharacterMenu/"+PlayableCharacters.Find(g.npcCast[0]).ArtId);kingdomQueenPortrait.texture=portrait.texture;kingdomQueenPortrait.rectTransform.sizeDelta=portrait.size;kingdomQueenPortrait.rectTransform.pivot=portrait.pivot;kingdomPortraitRound=g.round;}}
            var scale=Mathf.Min(1,safe.rect.width/1100);kingdomHud.localScale=kingdomControls.localScale=Vector3.one*scale;
            for(var i=0;i<kingdomNpcs.Count;i++){
                var npc=kingdomNpcs[i];npc.root.gameObject.SetActive(visible);if(!visible)continue;
                npc.visual.Select(g.npcCast[i]);
                var point=KingdomAdventure.NpcPoint(g,i,clock);var worldPoint=new Vector2(point.X,point.Y);
                if(kingdomNpcPoints[i]==Vector2.zero)kingdomNpcPoints[i]=worldPoint;else kingdomNpcPoints[i]=Vector2.MoveTowards(kingdomNpcPoints[i],worldPoint,230*dt);
                worldPoint=kingdomNpcPoints[i];npc.root.anchoredPosition=ToBoard(worldPoint.x,worldPoint.y);npc.root.localScale=Vector3.one*sceneScale*.85f;
                var job=KingdomAdventure.NpcJob(g,i,clock);var frozen=KingdomAdventure.Frozen(g,i);var carrying=job=="Carry fruit" || job=="Carry plank";
                var frame=kingdomNpcMotion[i].Observe(worldPoint,"kingdom/"+g.round+"/"+i,carrying,false,dt);
                var joyful=i>=6 && g.rescueStyle[i-6]==2 && clock-g.wakeAt[i-6]<6;
                var pose=frozen?CharacterPose.Idle:(g.phase==KingdomPhase.Feast || joyful) && frame.Speed<=1?CharacterPose.Dance:frame.Speed>1?frame.Pose:carrying?CharacterPose.Carry:job=="Build bridge" || job=="Show stepping stones" || i==0 && g.phase==KingdomPhase.Welcome?CharacterPose.Wave:CharacterPose.Idle;
                // Choose one final pose in world coordinates; camera movement
                // cannot invent NPC travel, and no pose is presented twice.
                npc.visual.PresentFrame(new CharacterFrame(pose,frame.Speed,frame.FaceLeft,(float)(clock-g.started),frame.Travel,frame.ResetMotion),dt);
                var cargo=kingdomNpcCargo[i];cargo.gameObject.SetActive(carrying);cargo.sprite=job=="Carry fruit"?IngredientSprite("strawberry"):null;cargo.color=job=="Carry fruit"?Color.white:new Color(.7f,.44f,.23f);cargo.rectTransform.sizeDelta=job=="Carry fruit"?new Vector2(60,60):new Vector2(115,35);
                foreach(var graphic in npc.visual.ActiveView.GetComponentsInChildren<Image>())graphic.color=frozen?new Color(.55f,.76f,.95f):Color.white;
            }
            kingdomBasket.gameObject.SetActive(visible);if(visible){var basket=KingdomAdventure.Prop("basket");kingdomBasket.anchoredPosition=ToBoard(basket.X,basket.Y);kingdomBasket.localScale=Vector3.one*sceneScale;for(var i=0;i<3;i++)kingdomBasketFruit[i].gameObject.SetActive((g.supplies&(1<<i))!=0);kingdomApple.gameObject.SetActive(g.phase>KingdomPhase.Supplies || g.phase==KingdomPhase.Supplies && clock-g.started>=6);kingdomGuide.text=g.phase==KingdomPhase.Welcome?"Let's find picnic food!":g.phase==KingdomPhase.Supplies?"Bring it to our basket!":g.phase==KingdomPhase.Bridge?"Let's build the crossing!":g.phase==KingdomPhase.Queen?"The wand is guarded!":g.phase==KingdomPhase.Rescue?"Wake our friends!":"A feast for everyone!";}
            for(var i=0;i<3;i++){kingdomBridge[i].gameObject.SetActive(visible && (g.boards&(1<<i))!=0);if(visible){kingdomBridge[i].anchoredPosition=ToBoard(1640+i*100,190);kingdomBridge[i].localScale=Vector3.one*sceneScale;kingdomBridge[i].GetComponent<Image>().color=g.crossing==2?new Color(.42f,.65f,.72f):new Color(.68f,.43f,.23f);}}
            foreach(var pair in kingdomHolds){var member=g?.members.FirstOrDefault(m=>m.actor==pair.Key);var held=visible && member?.attending==true && !string.IsNullOrEmpty(member.carrying);pair.Value.gameObject.SetActive(held);if(!held)continue;var player=ReadPlayer(pair.Key);var at=Shared?shared.VisualPosition(pair.Key):new Vector2(player.x,player.y);pair.Value.rectTransform.anchoredPosition=ToBoard(at.x,at.y)+new Vector2(45,90)*sceneScale;pair.Value.rectTransform.localScale=Vector3.one*sceneScale;pair.Value.sprite=member.carrying=="wand"?null:IngredientSprite(member.carrying=="fruit-0"?"banana":member.carrying=="fruit-1"?"strawberry":"pineapple");pair.Value.color=member.carrying=="wand"?new Color(.77f,.42f,.93f):Color.white;pair.Value.rectTransform.sizeDelta=member.carrying=="wand"?new Vector2(15,85):new Vector2(75,75);}
            foreach(var pair in kingdomProps){var id=pair.Key;var active=visible && (id.StartsWith("fruit")?g.phase==KingdomPhase.Supplies && (g.supplies&(1<<int.Parse(id.Substring(6))))==0 && !g.members.Any(m=>m.carrying==id):id.StartsWith("board")?g.phase==KingdomPhase.Bridge && (g.boards&(1<<int.Parse(id.Substring(6))))==0:id=="ball"?g.phase==KingdomPhase.Queen && g.queenPlan!=2:g.phase==KingdomPhase.Queen && (g.distractedUntil>g.clock || g.queenPlan==2 && g.clock-g.queenAt>=4));
                pair.Value.gameObject.SetActive(active);if(active){var point=KingdomAdventure.ActionPoint(g,id);var x=point.X;var y=point.Y;if(id=="ball" && g.distractedUntil>g.clock){var t=Mathf.Clamp01((float)(8-(g.distractedUntil-g.clock))/.8f);x=Mathf.Lerp(x,2200,t);y+=Mathf.Sin(t*Mathf.PI)*250;}pair.Value.anchoredPosition=ToBoard(x,y);pair.Value.localScale=Vector3.one*sceneScale;if(id.StartsWith("board"))DrawKingdomCrossingProp(pair.Value,g.crossing==2);}}
            if(!visible){TickKingdomStory(false,clock);kingdomSpoken=-1;kingdomApproach="";return;}
            var lines=new[]{"","Our kingdom needs a picnic! Let's help together.","Tap the fruit to gather our picnic supplies.","Tap the planks to open our bridge.",g.distractedUntil>g.clock?"The queen is watching the ball. Take the wand!":"Toss the magic ball, then take the queen's wand.","Tap our frozen friends to wake them with the wand.","You saved the kingdom! Let's have a picnic together."};
            kingdomHint.text=g.phase==KingdomPhase.Welcome?"Listen to our quest, or tap Let's go!":g.phase==KingdomPhase.Supplies && !string.IsNullOrEmpty(KingdomOwn?.carrying)?"You've got the fruit! Tap our glowing basket.":g.phase==KingdomPhase.Supplies?"First, pack food for the feast. Tap a fruit.":g.phase==KingdomPhase.Bridge?"The river blocks our path. Help the bridge builders!":g.phase==KingdomPhase.Rescue?"The wand breaks the spell. Wake our three friends!":lines[(int)g.phase];
            kingdomRole.text=KingdomOwn?.attending==true?KingdomAdventure.Roles[KingdomOwn.role]:"Join adventure";
            kingdomNext.gameObject.SetActive(g.phase>=KingdomPhase.Supplies && g.phase<KingdomPhase.Feast);
            kingdomReplay.gameObject.SetActive(g.phase==KingdomPhase.Feast);kingdomReplay.interactable=!kingdomSending && g.clock-g.started>=6;
            TickKingdomStory(visible,clock);
            if(kingdomSpoken!=(int)g.phase || kingdomSpokenRound!=g.round){kingdomSpoken=(int)g.phase;kingdomSpokenRound=g.round;Narration.Stop();SpeakKingdom();}
        }
        private void AddKingdomDepth(Action<RectTransform,float,int,string> add)
        {foreach(var npc in kingdomNpcs)if(npc.root.gameObject.activeSelf)add(npc.root,npc.root.anchoredPosition.y,3,npc.root.name);foreach(var prop in kingdomProps)if(prop.Value.gameObject.activeSelf)add(prop.Value,prop.Value.anchoredPosition.y,3,prop.Key);if(kingdomBasket?.gameObject.activeSelf==true)add(kingdomBasket,kingdomBasket.anchoredPosition.y,3,"story-basket");foreach(var plank in kingdomBridge)if(plank.gameObject.activeSelf)add(plank,plank.anchoredPosition.y,2,plank.name);foreach(var pair in kingdomHolds)if(pair.Value.gameObject.activeSelf)add(pair.Value.rectTransform,pair.Value.rectTransform.anchoredPosition.y-90*sceneScale,4,pair.Key);}
        private void ResetKingdom()
        {ResetKingdomStory();kingdomNpcs.Clear();kingdomProps.Clear();kingdomNpcMotion.Clear();kingdomNpcPoints.Clear();kingdomNpcCargo.Clear();kingdomHolds.Clear();kingdomBasketFruit.Clear();kingdomBridge.Clear();kingdomBasket=null;kingdomStory=null;kingdomPortraitRound=-1;kingdomHud=null;kingdomControls=null;kingdomApproach="";kingdomSending=false;kingdomSpoken=-1;kingdomFrame=-1;foreach(var clip in kingdomAudio)if(clip!=null)Resources.UnloadAsset(clip);kingdomAudio.Clear();}
    }
}
