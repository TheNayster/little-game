using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEditor;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class ZooJsonTests
    {
        public static void Run()
        {
            foreach(var species in ZooCatalog.All){Need(LittleWeeps.Client.WorldResources.Load<Texture2D>("Worlds/Zoo/Art/"+species.id)!=null,"Missing 2D atlas "+species.id);Need(LittleWeeps.Client.WorldResources.Load<Texture2D>("Scenery/zoo-"+species.id)!=null,"Missing 2D habitat "+species.id);}
            var old=GameWorld.WithPark(GameWorld.Create("one","two","three","four"));var before=old.Snapshot();var w=GameWorld.WithZoo(old);var s=w.Snapshot();
            s.schema=before.schema;s.revision--;s.zoo=null;Need(JsonUtility.ToJson(s)==JsonUtility.ToJson(before),"Additive migration");
            void Act(string who,SoloAction action,string value="",string target="",float x=0,float y=0){var p=w.ReadPlayer(who);var r=w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=who,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target,x=x,y=y});Need(r.Accepted,r.Outcome);}
            foreach(var p in before.players){Act(p.id,SoloAction.Travel,"zoo");Act(p.id,SoloAction.Move,x:650,y:100);Act(p.id,SoloAction.Zoo,"gate","zoo-savanna");
                Act(p.id,SoloAction.Move,x:ZooLayout.BucketX("elephant"),y:100);Act(p.id,SoloAction.Zoo,"take","elephant");var f=w.ReadZoo().food.Single(v=>v.actor==p.id);
                Act(p.id,SoloAction.Move,x:ZooLayout.SlotX("elephant",f.slot),y:100);Act(p.id,SoloAction.Zoo,"offer","elephant");}
            for(var i=0;i<100;i++){w.AdvanceIdle(1,out _);GameWorld.Validate(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));}
            Need(w.ReadZoo().animals[0].fed==4 && w.ReadZoo().food.All(f=>f.species==""),"Four exact consumptions");
            var retained=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));Need(retained.ReadZoo().animals[0].fed==4,"Retention");
            Act("one",SoloAction.Move,x:1200,y:490);Need(w.ReadPlayer("one").y==120,"Visitor boundary");
            Act("one",SoloAction.Move,x:ZooLayout.BucketX("elephant"),y:100);Act("one",SoloAction.Zoo,"take","elephant");
            var pending=w.ReadZoo().food.Single(f=>f.actor=="one");Act("one",SoloAction.Move,x:ZooLayout.SlotX("elephant",pending.slot),y:100);Act("one",SoloAction.Zoo,"offer","elephant");
            var unfinished=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));
            Need(unfinished.ReadZoo().food.All(f=>f.species=="") && unfinished.ReadZoo().animals[0].fed==4,"Restore clears unfinished offers and retains consumption history");
            ElephantPlay();
            Debug.Log("ZOO_JSON_PASS: additive migration, four portions, uint RNG JSON, retention and visitor boundary");
        }
        private static void ElephantPlay()
        {
            var w=GameWorld.WithZoo(GameWorld.Create("one","two","three","four"));
            SoloResult Act(string actor,SoloAction action,string value="",string target="",float x=0){var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target,x=x,y=100});}
            void Step(double seconds){while(seconds>0){var n=Math.Min(.1,seconds);w.AdvanceIdle(n,out _);GameWorld.Validate(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));seconds-=n;}}
            foreach(var id in new[]{"one","two","three","four"}){
                Need(Act(id,SoloAction.Travel,"zoo").Accepted,"entry");Need(Act(id,SoloAction.Move,x:650).Accepted,"gate position");Need(Act(id,SoloAction.Zoo,"gate","zoo-savanna").Accepted,"gate");
            }
            Step(.1);Need(w.ReadZoo().animals[0].phase==ZooPhase.Greet,"arrival greeting");var greet=w.ReadZoo().animals[0].sequence;
            Step(3);Need(w.ReadZoo().animals[0].sequence>greet && w.ReadZoo().greetingCooldown>0,"greeting finishes/rate limit");
            Need(Act("one",SoloAction.Zoo,"water","elephant").Accepted,"water accepted");
            Need(!Act("two",SoloAction.Zoo,"water","elephant").Accepted && w.ReadZoo().waterSequence==1,"shared pump rate limit");
            Step(1.3);Need(!Act("one",SoloAction.Zoo,"water","elephant").Accepted,"per-child fair cooldown");
            Need(Act("two",SoloAction.Zoo,"water","elephant").Accepted,"another child can pump");
            for(var i=0;i<180 && w.ReadZoo().animals[0].phase!=ZooPhase.Splash;i++)Step(.1);
            Need(w.ReadZoo().animals[0].phase==ZooPhase.Splash,"elephant reaches water");
            Need(Act("one",SoloAction.Move,x:ZooLayout.BucketX("elephant")).Accepted,"bucket");Need(Act("one",SoloAction.Zoo,"take","elephant").Accepted,"portion");
            var f=w.ReadZoo().food.Single(v=>v.actor=="one");Need(Act("one",SoloAction.Move,x:ZooLayout.SlotX("elephant",f.slot)).Accepted,"offering place");Need(Act("one",SoloAction.Zoo,"offer","elephant").Accepted,"offer during splash");
            Step(.7);Need(w.ReadZoo().animals[0].owner=="one","feeding wins within .6s plus authority tick");
            Step(1.5);Need(Act("three",SoloAction.Zoo,"water","elephant").Accepted,"busy water tap accepted");
            Need(w.ReadZoo().waterPending,"busy elephant retains single pending request");
            var restored=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));
            Need(!restored.ReadZoo().waterPending && restored.ReadZoo().waterSequence==0 && restored.ReadZoo().animals[0].phase<ZooPhase.Greet,"restore clears play/effects");
            for(var i=0;i<300 && w.ReadZoo().animals[0].fed==0;i++)Step(.1);
            Need(w.ReadZoo().animals[0].fed==1,"feeding not starved");
            Step(1);Need(w.ReadZoo().animals[0].phase==ZooPhase.Eat,"approved eating finish retained");
            Step(3);Need(w.ReadZoo().animals[0].phase==ZooPhase.WaterWalk || w.ReadZoo().animals[0].phase==ZooPhase.Splash,"pending play follows finish");
            foreach(var id in new[]{"one","two","three","four"})Need(Act(id,SoloAction.Travel,"creek").Accepted,"leave");
            Step(.1);Need(!w.ReadZoo().waterPending && w.ReadZoo().animals[0].phase<ZooPhase.Greet,"departure clears play");
            Debug.Log("ELEPHANT_PLAY_JSON_PASS: greeting, fair/shared cooldowns, splash feeding delay, pending priority, retained finish, JSON restore and departure");
        }
        static void Need(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
