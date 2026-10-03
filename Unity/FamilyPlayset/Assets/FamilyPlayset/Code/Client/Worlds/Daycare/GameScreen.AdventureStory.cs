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
        private RectTransform kingdomEncounterControls,kingdomTalk;
        private readonly List<Text> kingdomBubbles=new List<Text>();
        private readonly List<RectTransform> kingdomLandmarks=new List<RectTransform>();
        private int kingdomTalkPhase=-1,kingdomTalkRound=-1;
        private void PresentKingdomHop(string actor,GameCharacterVisual visual)
        {
            var g=KingdomGame;var member=g?.members.FirstOrDefault(m=>m.actor==actor);if(member?.attending!=true || ReadPlayer(actor).zone!=KingdomAdventure.Zone || g.crossing!=2)return;
            var age=kingdomDisplayClock-member.hopAt;
            if(age>=0 && age<.75 && visual.ActiveView!=null)((RectTransform)visual.ActiveView.transform).anchoredPosition=new Vector2(0,-45+(float)Math.Sin(age/.75*Math.PI)*70);
        }
        private void PresentKingdomHops()
        {PresentKingdomHop(Actor,characterVisual);foreach(var friend in friends)if(friend.Value.root.gameObject.activeSelf)PresentKingdomHop(friend.Key,friend.Value.view);}
        private void BuildKingdomStoryControls()
        {
            kingdomEncounterControls=Rect(safe,"Adventure encounters",Vector2.zero,new Vector2(680,66));
            kingdomEncounterControls.anchorMin=kingdomEncounterControls.anchorMax=new Vector2(.5f,1);kingdomEncounterControls.anchoredPosition=new Vector2(0,-247);
            Button(kingdomEncounterControls,"Talk to a friend",new Vector2(-165,0),new Vector2(300,60),()=>RequestKingdom("npc-"+KingdomStoryFriend(),"talk"),new Color(.85f,.92f,.98f)).fontSize=24;
            Button(kingdomEncounterControls,"Story map",new Vector2(165,0),new Vector2(300,60),ShowKingdomMap,Color.white).fontSize=24;
            for(var i=1;i<9;i++)kingdomBubbles.Add(Label(kingdomNpcs[i].root,"",19,new Vector2(0,218),new Vector2(205,90)));
            var titles=new[]{"Castle camp","Fruit orchard","River crossing","Queen's grove"};var xs=new[]{440,900,1650,2160};
            for(var i=0;i<4;i++){var sign=Panel(Board,"Story landmark "+i,Vector2.zero,new Vector2(230,60),new Color(1,.98f,.88f,.92f),false).rectTransform;Label(sign,titles[i],24,Vector2.zero,new Vector2(225,55));sign.anchoredPosition=ToBoard(xs[i],590);kingdomLandmarks.Add(sign);}
            foreach(var name in new[]{"talk-food","talk-river","talk-queen","talk-rescue","talk-thanks","talk-frozen","talk-invited","talk-helping","talk-castle","ending-kind","ending-ball"}){var clip=WorldResources.Load<AudioClip>("Worlds/Daycare/StoryAdventure/"+name);if(clip!=null){kingdomAudio.Add(clip);Narration.AddClip("kingdom-"+name,clip);}}
        }
        private int KingdomStoryFriend()
        {
            var g=KingdomGame;if(g.phase==KingdomPhase.Supplies)return 1;if(g.phase==KingdomPhase.Bridge)return 2;if(g.phase==KingdomPhase.Queen)return 5;
            if(g.phase==KingdomPhase.Rescue)for(var i=0;i<3;i++)if((g.rescued&(1<<i))==0)return 6+i;
            return 0;
        }
        private void CloseKingdomTalk()
        {if(kingdomTalk!=null){kingdomTalk.gameObject.SetActive(false);Destroy(kingdomTalk.gameObject);kingdomTalk=null;}kingdomTalkPhase=-1;}
        private void KingdomTalkChoice(string op,string target)
        {CloseKingdomTalk();Narration.Stop();if(op=="explore")return;if(op=="begin"){SendKingdom(op);return;}RequestKingdom(target,op);}
        private RectTransform KingdomCard(string title,string words,int friend)
        {
            CloseKingdomTalk();CancelPointers();destination=null;shared?.Walk(WalkMode.Stop);
            var g=KingdomGame;kingdomTalkPhase=(int)g.phase;kingdomTalkRound=g.round;
            kingdomTalk=Panel(safe,"Adventure conversation",Vector2.zero,new Vector2(900,490),new Color(1,.98f,.9f,.99f),true).rectTransform;
            kingdomTalk.localScale=Vector3.one*Mathf.Min(safe.rect.width/950,safe.rect.height/570);
            Label(kingdomTalk,title,32,new Vector2(0,192),new Vector2(830,60));
            if(friend>=0){var art=WorldResources.Load<CharacterMenuArt>("Shared/Characters/Menu/"+PlayableCharacters.Find(g.npcCast[friend]).ArtId);var picture=Rect(kingdomTalk,"Talking friend portrait",new Vector2(-315,0),art.size).gameObject.AddComponent<RawImage>();picture.texture=art.texture;picture.rectTransform.pivot=art.pivot;picture.raycastTarget=false;}
            Label(kingdomTalk,words,29,new Vector2(friend>=0?90:0,30),new Vector2(friend>=0?580:800,255));
            return kingdomTalk;
        }
        private void ShowKingdomTalk(int index)
        {
            if(!KingdomArea || MenuOpen || KingdomOwn?.attending!=true)return;
            var g=KingdomGame;var name=KingdomAdventure.Name(g,index);string text,clip;var choices=new List<(string label,string op,string target)>();
            if(index==0){text=g.phase==KingdomPhase.Feast?KingdomAdventure.Ending(g):"I'm the Kindly Queen in our pretend kingdom! The other queen froze three of our friends. Can you bring them home? Our orchard friend and river helpers know the way.";clip=g.phase==KingdomPhase.Feast?g.queenPlan==2?"ending-kind":"ending-ball":"talk-castle";choices.Add(("Let's explore!",g.phase==KingdomPhase.Welcome?"begin":"explore",""));}
            else if(index==1 && g.phase==KingdomPhase.Supplies){text=g.foodHelp?"I'll keep packing fruit. You can gather some too, or explore our kingdom while I help!":"The kingdom needs food for the feast. You can carry fruit to our basket. Would you like me to help pack while you explore?";clip="talk-food";if(!g.foodHelp)choices.Add(("Help me pack","help-pack","npc-1"));choices.Add(("I'll gather food","explore",""));}
            else if(index>=2 && index<=4 && g.phase==KingdomPhase.Bridge){text=g.boards!=0?"Our crossing is taking shape! Keep going and we'll meet you on the other side.":"The bridge washed away! We can build it with these planks, or hop over the big stepping stones. Which way shall we go?";clip="talk-river";if(g.boards==0){choices.Add(("Build a bridge","bridge-route","npc-2"));choices.Add(("Hop on stones","stone-route","npc-2"));}else choices.Add(("Let's cross!","explore",""));}
            else if(index==5 && g.phase==KingdomPhase.Queen){text=g.queenPlan==2?"You invited me? Thank you! I'll meet everyone at the feast. You may borrow my wand to wake our friends.":"I'm the Greedy Queen! I froze everyone because I wasn't invited. Are you here to take my wand? Hmm... I do love playing ball.";clip=g.queenPlan==2?"talk-invited":"talk-queen";if(g.queenPlan!=2){choices.Add(("Invite her to feast","invite-queen","npc-5"));choices.Add(("Play ball","toss","ball"));}else choices.Add(("Let's help our friends","explore",""));}
            else if(index>=6 && KingdomAdventure.Frozen(g,index)){text=g.phase==KingdomPhase.Rescue?"Brrr! I'm stuck in a pretend spell. You've found the wand! Shall we try a sparkle spell or a silly dancing spell?":"I'm frozen in a pretend spell! Find the queen's wand beyond the river. Please come back for me!";clip=g.phase==KingdomPhase.Rescue?"talk-rescue":"talk-frozen";if(g.phase==KingdomPhase.Rescue){choices.Add(("Sparkle spell","wake","friend-"+(index-6)));choices.Add(("Silly spell","wake-joke","friend-"+(index-6)));}else choices.Add(("We'll help you!","explore",""));}
            else {text=index>=6?"I'm awake! Thank you for coming back for me. Let's take all our friends to the feast!":"We're helping the kingdom together. Talk to our next friend, or explore and see what you can find!";clip=index>=6?"talk-thanks":"talk-helping";choices.Add(("Keep exploring","explore",""));}
            var card=KingdomCard(name+" · "+(index==0?"Kindly Queen":index==5?"Greedy Queen":index>=6?"Our friend":index==1?"Orchard friend":"River helper"),text,index);
            for(var i=0;i<choices.Count;i++){var choice=choices[i];Button(card,choice.label,new Vector2(choices.Count==1?0:(i==0?-210:210),-148),new Vector2(390,70),()=>KingdomTalkChoice(choice.op,choice.target),new Color(.82f,.91f,.67f)).fontSize=27;}
            Button(card,"Listen",new Vector2(-205,-218),new Vector2(240,46),()=>Narration.Speak("kingdom-"+clip),Color.white).fontSize=22;
            Button(card,"Back to exploring",new Vector2(180,-218),new Vector2(350,46),CloseKingdomTalk,Color.white).fontSize=22;
            Narration.Stop();Narration.Speak("kingdom-"+clip);
        }
        private void ShowKingdomMap()
        {
            if(!KingdomArea || MenuOpen)return;var g=KingdomGame;
            var words="OUR QUEST: wake three frozen friends and bring everyone home.\n\n"+(g.phase>KingdomPhase.Supplies?"Food packed. ":"Visit the orchard. ")+(g.phase>KingdomPhase.Bridge?(g.crossing==2?"We crossed the stones. ":"We built the bridge. "):"Find a way across the river. ")+(g.phase>=KingdomPhase.Rescue?(g.queenPlan==2?"The queen is invited! ":"We found the wand! "):"Meet the queen. ")+"\nFriends awake: "+Enumerable.Range(0,3).Count(i=>(g.rescued&(1<<i))!=0)+" / 3";
            var card=KingdomCard("Our kingdom story",words,-1);
            var choices=new[]{("Castle",0),("Orchard",1),("River",2),("Queen",3)};
            for(var i=0;i<choices.Length;i++){var choice=choices[i];var at=new Vector2(-315+i*210,-155);Button(card,choice.Item1,at,new Vector2(195,65),()=>RequestKingdom("site-"+choice.Item2,"visit"),new Color(.81f,.9f,.96f)).fontSize=23;}
            Button(card,"Back to exploring",new Vector2(0,-222),new Vector2(330,46),CloseKingdomTalk,Color.white).fontSize=22;
        }
        private void DrawKingdomCrossingProp(RectTransform prop,bool stone)
        {
            var plank=prop.Find("Bridge plank");if(plank!=null)plank.gameObject.SetActive(!stone);
            var picture=prop.Find("Stepping stone");if(stone && picture==null){picture=Panel(prop,"Stepping stone",new Vector2(0,25),new Vector2(95,55),new Color(.43f,.68f,.73f),false,true).transform;Label(picture,"Hop!",22,Vector2.zero,new Vector2(85,40));}
            if(picture!=null)picture.gameObject.SetActive(stone);
            var control=prop.GetComponentInChildren<Button>();if(control!=null && prop.name.StartsWith("Adventure prop board-"))control.name=(stone?"Hop stepping stone ":"Place bridge plank ")+(int.Parse(prop.name.Substring(prop.name.Length-1))+1);
        }
        private void TickKingdomStory(bool visible,double clock)
        {
            if(kingdomEncounterControls==null)return;var g=KingdomGame;
            kingdomEncounterControls.gameObject.SetActive(visible && g.phase!=KingdomPhase.Welcome && !MenuOpen);kingdomEncounterControls.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/1100);
            if(kingdomTalk!=null && (!visible || MenuOpen || kingdomTalkPhase!=(int)g.phase || kingdomTalkRound!=g.round))CloseKingdomTalk();
            if(kingdomTalk!=null)kingdomTalk.SetAsLastSibling();
            for(var i=0;i<kingdomLandmarks.Count;i++){var sign=kingdomLandmarks[i];sign.gameObject.SetActive(visible);if(visible){sign.anchoredPosition=ToBoard(new[]{440,900,1650,2160}[i],590);sign.localScale=Vector3.one*sceneScale;}}
            if(!visible)return;
            for(var i=1;i<9;i++){kingdomBubbles[i-1].text=i==1?(g.foodHelp && g.phase==KingdomPhase.Supplies?"I'll help pack!":"Talk to me!"):i>=2 && i<=4?(g.crossing==2?"Try the stones!":"Need a crossing?"):i==5?(g.queenPlan==2?"I'm invited!":"Who goes there?"):KingdomAdventure.Frozen(g,i)?"Please help us!":"Thank you!";}
            if(g.phase==KingdomPhase.Bridge)kingdomHint.text=g.crossing==0?"Talk to the river helpers. Which way shall we cross?":g.crossing==2?"Hop across the three big stepping stones!":"Bring planks. Our helpers will build with you!";
            if(g.phase==KingdomPhase.Queen)kingdomHint.text=g.queenPlan==2?(clock-g.queenAt<4?"The queen is joining our feast. Wait for her wand!":"The queen shared her wand! Take it to our friends."):g.queenPlan==0?"Meet the Greedy Queen. What does she want?":g.distractedUntil>clock?"She's chasing the ball! Borrow her wand.":"Try another throw, or talk to the queen.";
            if(g.phase==KingdomPhase.Feast)kingdomHint.text=g.queenPlan==2?"Our friends are awake. Even the queen belongs at our feast!":"Our friends are awake. The kingdom can celebrate!";
        }
        private void ResetKingdomStory()
        {CloseKingdomTalk();kingdomEncounterControls=null;kingdomBubbles.Clear();kingdomLandmarks.Clear();}
    }
}
