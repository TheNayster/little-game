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
            ElephantPlay();ElephantCare();ElephantSurprises();
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

        private static void ElephantCare()
        {
            var w=GameWorld.WithZoo(GameWorld.Create("one","two","three","four"));
            SoloResult Act(string id,SoloAction action,string value="",string item="",float x=1200){var p=w.ReadPlayer(id);return w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=action==SoloAction.Zoo?"elephant":"",item=item,x=x,y=100});}
            void Step(double seconds){while(seconds>.00001){var n=Math.Min(.1,seconds);w.AdvanceIdle(n,out _);GameWorld.Validate(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));seconds-=n;}}
            foreach(var id in new[]{"one","two","three","four"}){Need(Act(id,SoloAction.Travel,"zoo").Accepted,"entry");var p=w.ReadPlayer(id);Need(w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=SoloAction.Move,x=650,y=100}).Accepted,"move");p=w.ReadPlayer(id);Need(w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=SoloAction.Zoo,value="gate",target="zoo-savanna"}).Accepted,"gate");Act(id,SoloAction.Move);}
            foreach(var id in new[]{"one","two","three","four"})Need(Act(id,SoloAction.Zoo,"care").Accepted,"shared care request");
            Step(12);Need(w.ReadZoo().animals[0].phase==ZooPhase.Care && w.ReadZoo().careMembers.Length==4,"four share one care session");
            var session=w.ReadZoo().careSession;
            string Token(int patch,int serial,string id="one")=>session+"/"+w.ReadZoo().careMemberEpoch[Array.IndexOf(w.ReadZoo().careMembers,id)]+"/"+patch+"/"+serial;
            Need(!Act("one",SoloAction.Zoo,"brush",Token(9,1)).Accepted,"invalid patch");
            foreach(var id in new[]{"one","two","three","four"})Need(Act(id,SoloAction.Zoo,"brush",Token(0,1,id)).Accepted,"simultaneous helpers");
            Need(w.ReadZoo().careProgress[0]==4,"clamped shared patch");
            Need(!Act("one",SoloAction.Zoo,"brush",Token(1,1)).Accepted,"old serial rejected");
            Need(!Act("one",SoloAction.Zoo,"brush",Token(1,2)).Accepted,"held/rapid rejected");
            var oldToken=Token(1,2,"two");Need(Act("two",SoloAction.Zoo,"put-brush").Accepted,"put away sibling");Need(Act("two",SoloAction.Zoo,"care").Accepted,"rejoin same shared session");Need(!Act("two",SoloAction.Zoo,"brush",oldToken).Accepted,"old membership epoch rejected");Step(.4);Need(Act("two",SoloAction.Zoo,"brush",Token(2,1,"two")).Accepted,"new client serial restarts safely");
            Need(Act("two",SoloAction.Travel,"creek").Accepted && w.ReadZoo().careProgress[0]==4 && w.ReadZoo().careMembers.Length==3,"independent immediate departure");
            Act("one",SoloAction.Move,x:1560);Need(Act("one",SoloAction.Zoo,"take").Accepted,"food during care");var f=w.ReadZoo().food.Single(v=>v.actor=="one");Act("one",SoloAction.Move,x:ZooLayout.SlotX("elephant",f.slot));Need(Act("one",SoloAction.Zoo,"offer").Accepted,"offer");Step(.45);Need(w.ReadZoo().animals[0].owner=="one","care yields within .35 plus tick");
            Need(Act("three",SoloAction.Zoo,"water").Accepted,"pending water");Need(w.ReadZoo().careProgress[0]==4,"suspension retains patches");
            Step(25);Need(w.ReadZoo().animals[0].phase==ZooPhase.Care,"older care resumes before newer water");
            for(var patch=1;patch<3;patch++)for(var n=w.ReadZoo().careProgress[patch];n<4;n++){Step(.4);Need(Act("one",SoloAction.Zoo,"brush",Token(patch,3+patch*4+n)).Accepted,"solo can finish shared patches");}
            Need(w.ReadZoo().careComplete && w.ReadZoo().animals[0].fed==1,"completion independent of feeding");Step(.1);Need(w.ReadZoo().animals[0].phase==ZooPhase.CareFinish,"one appreciative transition");
            Need(!Act("one",SoloAction.Zoo,"care").Accepted,"finish protected from rapid replay");
            var copy=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));Need(copy.ReadZoo().careMembers.Length==0 && !copy.ReadZoo().careComplete && copy.ReadZoo().careSession==0 && copy.ReadZoo().animals[0].fed==1,"save clears care retains feeding");
            Step(3);Need(Act("one",SoloAction.Zoo,"care").Accepted && w.ReadZoo().careSession==session+1 && w.ReadZoo().careProgress.All(n=>n==0),"deliberate replay");
            foreach(var id in new[]{"one","three","four"})Act(id,SoloAction.Travel,"creek");Step(.1);Need(w.ReadZoo().careMembers.Length==0 && w.ReadZoo().animals[0].phase<ZooPhase.Greet,"all leave cleanup");
            Act("one",SoloAction.Travel,"zoo");var player=w.ReadPlayer("one");w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor="one",expectedRevision=w.Revision,zone=player.zone,visit=player.visit,action=SoloAction.Move,x=650,y=100});player=w.ReadPlayer("one");w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor="one",expectedRevision=w.Revision,zone=player.zone,visit=player.visit,action=SoloAction.Zoo,value="gate",target="zoo-savanna"});Act("one",SoloAction.Zoo,"care");Step(12);Need(w.ReadZoo().animals[0].phase==ZooPhase.Care,"disconnect fixture");w.ReleaseZoo("one");Need(w.ReadZoo().careMembers.Length==0 && w.ReadZoo().animals[0].phase<ZooPhase.Greet,"last disconnect cleans before empty-session tick stop");
            var legacy=w.Snapshot();legacy.zoo.careMembers=null;legacy.zoo.careProgress=null;legacy.zoo.carePatch=null;legacy.zoo.careBrushAge=null;legacy.zoo.careSession=0;GameWorld.Validate(legacy);Need(GameWorld.Restore(legacy).ReadZoo().careMembers.Length==0,"legacy additive care defaults");
            Debug.Log("ELEPHANT_CARE_JSON_PASS: shared session, simultaneous clamp, gesture/rate validation, solo completion, food suspension, ordered water, replay, immediate departure and save cleanup");
        }
        private static void ElephantSurprises()
        {
            var w=GameWorld.WithZoo(GameWorld.Create("one","two","three","four"));
            SoloResult Act(string id,SoloAction action,string value="",string target="",float x=0){var p=w.ReadPlayer(id);return w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target,x=x,y=100});}
            foreach(var id in new[]{"one","two","three","four"}){Need(Act(id,SoloAction.Travel,"zoo").Accepted,"entry");Need(Act(id,SoloAction.Move,x:650).Accepted,"gate position");Need(Act(id,SoloAction.Zoo,"gate","zoo-savanna").Accepted,"gate");}
            var before=string.Join("|",w.ReadZoo().animals.Select(a=>JsonUtility.ToJson(a)));
            Need(Act("one",SoloAction.Zoo,"surprise",ZooLayout.BirdSurprise).Accepted,"bird trigger");
            foreach(var id in new[]{"two","three","four"})Need(!Act(id,SoloAction.Zoo,"surprise",ZooLayout.BirdSurprise).Accepted,"coalesced trigger");
            Need(Act("two",SoloAction.Zoo,"surprise",ZooLayout.ButterflySurprise).Accepted,"independent flowers");
            Need(w.ReadZoo().surpriseSequence.SequenceEqual(new[]{1,1}) && w.ReadZoo().surpriseAge.All(n=>n==0),"one shared event per prop");
            Need(string.Join("|",w.ReadZoo().animals.Select(a=>JsonUtility.ToJson(a)))==before,"no retasking or random-stream change");
            var copy=w.ReadZoo();copy.surpriseAge[0]=8;Need(w.ReadZoo().surpriseAge[0]==0,"deep snapshot copy");
            Need(Act("one",SoloAction.Travel,"creek").Accepted && w.ReadZoo().surpriseAge[0]==0,"exit preserves siblings event");
            var restored=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));
            Need(restored.ReadZoo().surpriseSequence.All(n=>n==0) && restored.ReadZoo().surpriseAge.All(n=>n==10),"save clears transient effects");
            for(var i=0;i<81;i++)w.AdvanceIdle(.1,out _);
            Need(Act("two",SoloAction.Zoo,"surprise",ZooLayout.BirdSurprise).Accepted && w.ReadZoo().surpriseVariation[0]==0,"replay variation after reset");
            var legacy=w.Snapshot();legacy.zoo.surpriseSequence=null;legacy.zoo.surpriseAge=null;legacy.zoo.surpriseVariation=null;GameWorld.Validate(legacy);
            Need(GameWorld.Restore(legacy).ReadZoo().surpriseAge.All(n=>n==10),"legacy safe idle defaults");
            Debug.Log("ELEPHANT_SURPRISE_JSON_PASS: independent triggers, coalescing, unchanged animal state, deep copies, departure, reset/variation, restore and legacy defaults");
        }
        static void Need(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
