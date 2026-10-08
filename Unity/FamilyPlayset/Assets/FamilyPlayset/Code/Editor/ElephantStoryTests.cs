using System;
using System.Linq;
using LittleWeeps.Core;
#if UNITY_EDITOR
using UnityEngine;
#else
using System.Text.Json;
#endif
namespace LittleWeeps.EditorTools
{
    public static class ElephantStoryTests
    {
        private static void Need(bool b,string name){if(!b)throw new Exception("Missing Ball: "+name);}
        private static SoloSnapshot RoundTrip(SoloSnapshot s)
        {
#if UNITY_EDITOR
            return JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));
#else
            var json=new JsonSerializerOptions{IncludeFields=true};return JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(s,json),json);
#endif
        }
        public static void Run()
        {
            var w=GameWorld.WithZoo(GameWorld.Create("one","two","three","four"));
            SoloResult Act(string id,SoloAction action,string value="",string target="",float x=1200){var p=w.ReadPlayer(id);return w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=100});}
            void Step(double n){while(n>.00001){var dt=Math.Min(.1,n);w.AdvanceIdle(dt,out _);GameWorld.Validate(RoundTrip(w.Snapshot()));n-=dt;}}
            ElephantStoryState Story()=>w.ReadZoo().story;
            ZooAnimal Animal()=>w.ReadZoo().animals[0];
            void Pick(string id){Need(Act(id,SoloAction.Move,x:Story().x).Accepted,"reachable spot");Need(Act(id,SoloAction.Zoo,"story-pickup",Story().session.ToString()).Accepted,"pickup");}
            void Return(string id){Need(Act(id,SoloAction.Move,x:ElephantStory.BasketX).Accepted,"return path");Need(Act(id,SoloAction.Zoo,"story-return",Story().session.ToString()).Accepted,"accepted return");}
            void Offer(string id){Need(Act(id,SoloAction.Move,x:ZooLayout.BucketX("elephant")).Accepted,"bucket position");Need(Act(id,SoloAction.Zoo,"take","elephant").Accepted,"food");var f=w.ReadZoo().food.Single(v=>v.actor==id);Need(Act(id,SoloAction.Move,x:ZooLayout.SlotX("elephant",f.slot)).Accepted,"tray position");Need(Act(id,SoloAction.Zoo,"offer","elephant").Accepted,"offer");}
            foreach(var id in new[]{"one","two","three","four"}){Need(Act(id,SoloAction.Travel,"zoo").Accepted,"entry");Need(Act(id,SoloAction.Move,x:650).Accepted,"gate");Need(Act(id,SoloAction.Zoo,"gate","zoo-savanna").Accepted,"exhibit");}
            Step(4);var sequence=Animal().sequence;
            foreach(var id in new[]{"one","two","three","four"})Need(Act(id,SoloAction.Zoo,"story-start","elephant").Accepted,"coalesced start");
            Need(Story().session==1 && Animal().sequence==sequence,"one session without immediate retask");
            var copy=w.ReadZoo();copy.story.carrier="forged";Need(Story().carrier=="","detached story");
            Pick("one");Need(!Act("two",SoloAction.Zoo,"story-pickup","1").Accepted && Story().carrier=="one","atomic pickup");
            Need(!Act("one",SoloAction.Zoo,"take","elephant").Accepted && !Act("one",SoloAction.Zoo,"snack-begin","elephant").Accepted && !Act("one",SoloAction.Zoo,"care","elephant").Accepted,"one carried item");
            Need(Act("one",SoloAction.Zoo,"story-drop","1").Accepted,"put down");Pick("two");Return("two");
            Need(Story().phase==ElephantStoryPhase.Returned && Story().carrier=="","accepted while busy");
            for(var i=0;i<300 && Story().phase!=ElephantStoryPhase.Reacting;i++)Step(.1);
            Need(Story().phase==ElephantStoryPhase.Reacting,"bounded reaction eventually scheduled");
            Offer("three");Step(.1);Need(Animal().owner=="three","story adds at most one authority tick to feeding");
            Step(.1);Need(Story().phase==ElephantStoryPhase.Complete,"preempted acknowledgment consumed once");
            Need(!Act("one",SoloAction.Zoo,"story-start","elephant").Accepted,"quiet replay reset");Step(100);
            Need(Act("one",SoloAction.Zoo,"story-start","elephant").Accepted && Story().session==2,"deliberate replay");
            Need(Story().hidingSpot!=copy.story.hidingSpot,"both configured hiding spots");
            Need(!Act("one",SoloAction.Zoo,"story-pickup","1").Accepted,"old session rejected");
            Pick("one");var restore=GameWorld.Restore(RoundTrip(w.Snapshot()));Need(restore.ReadZoo().story.phase==ElephantStoryPhase.Available && restore.ReadZoo().story.carrier=="" && !ElephantStory.AnimalPhase(restore.ReadZoo().animals[0].phase),"save clears story/pose");
            Need(restore.ReadZoo().animals[0].fed==w.ReadZoo().animals[0].fed,"save preserves feeding");
            Need(Act("one",SoloAction.Move,x:2400).Accepted && Story().carrier=="","ineligible walking releases");Pick("two");
            Need(Act("two",SoloAction.Travel,"creek").Accepted && Story().carrier=="","departure releases");
            Pick("three");w.ReleaseZoo("three");Need(Story().carrier=="" && Story().phase==ElephantStoryPhase.Searching,"disconnect retains ball for sibling");
            foreach(var id in new[]{"one","two","three","four"})w.ReleaseZoo(id);
            Need(Story().phase==ElephantStoryPhase.Available,"all leave clears synchronously");
#if UNITY_EDITOR
            Debug.Log("ELEPHANT_STORY_PASS: one session/ball, atomic ownership, explicit handoff, item exclusion, replay/session guards, food priority, departure/disconnect/restore");
#else
            Console.WriteLine("PASS elephant story authority, carrying, replay, scheduling and recovery");
#endif
        }
    }
}
