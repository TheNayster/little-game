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
    public static class ElephantHabitatTests
    {
        private static void Need(bool b,string name){if(!b)throw new Exception("Elephant habitat: "+name);}
        private static SoloSnapshot Json(SoloSnapshot s){
#if UNITY_EDITOR
            return JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));
#else
            var opt=new JsonSerializerOptions{IncludeFields=true};return JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(s,opt),opt);
#endif
        }
        public static void Run()
        {
            var w=GameWorld.WithZoo(GameWorld.Create("one","two","three","four"));
            SoloResult Act(string id,SoloAction a,string value="",string target="",string item="",float x=1200)=>w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,zone=w.ReadPlayer(id).zone,visit=w.ReadPlayer(id).visit,expectedRevision=w.Revision,action=a,value=value,target=target,item=item,x=x,y=100});
            void Step(double n){while(n>.00001){var dt=Math.Min(.1,n);w.AdvanceIdle(dt,out _);GameWorld.Validate(Json(w.Snapshot()));n-=dt;}}
            ElephantHabitatState H()=>w.ReadZoo().habitat;
            ZooAnimal A()=>w.ReadZoo().animals[0];
            foreach(var id in new[]{"one","two","three","four"}){Need(Act(id,SoloAction.Travel,"zoo").Accepted,"entry");Need(Act(id,SoloAction.Move,x:650).Accepted,"gate path");Need(Act(id,SoloAction.Zoo,"gate","zoo-savanna").Accepted,"gate");}
            Step(5);
            for(var k=0;k<3;k++)for(var slot=0;slot<3;slot++){
                if(!ElephantHabitat.Legal((ElephantPropKind)k,slot))continue;
                Need(Act("one",SoloAction.Zoo,"habitat-place",slot.ToString(),k+"/"+H().nextId).Accepted,"legal combination");var p=H().props.Single();
                Need(!Act("two",SoloAction.Zoo,"habitat-move","2",p.id+"/0").Accepted,"creator ownership");
                Need(!Act("two",SoloAction.Zoo,"habitat-place",slot.ToString(),k+"/"+H().nextId).Accepted,"occupied slot atomic rejection");
                for(var i=0;i<500 && A().phase!=ZooPhase.PropUse;i++)Step(.1);
                Need(A().phase==ZooPhase.PropUse && H().usingId==p.id,"scheduler eventually uses actual placement");var anchor=ElephantHabitat.Anchor(p);var point=ZooLayout.Point(A());Need(Math.Abs(point.X-anchor.X)<.01 && point.Y==anchor.Y,"actual use anchor");
                Need(!Act("one",SoloAction.Zoo,"habitat-remove","-1",p.id+"/0").Accepted && H().props.Length==1,"support cannot vanish");
                var detached=H();detached.props[0].slot=99;Need(H().props[0].slot==slot,"detached placement");
                Need(Act("two",SoloAction.Move,x:1560).Accepted && Act("two",SoloAction.Zoo,"take","elephant").Accepted,"food during prop use");var f=w.ReadZoo().food.Single(v=>v.actor=="two");
                Need(Act("two",SoloAction.Move,x:ZooLayout.SlotX("elephant",f.slot)).Accepted && Act("two",SoloAction.Zoo,"offer","elephant").Accepted,"offer");Step(.1);Need(A().owner=="two","maximum added feeding delay one tick");
                Step(20);Need(Act("one",SoloAction.Zoo,"habitat-remove","-1",p.id+"/0").Accepted,"return basket");Need(H().props.Length==0,"bounded removal");
            }
            Need(Act("one",SoloAction.Zoo,"habitat-place","0","1/"+H().nextId).Accepted,"first outer prop");
            Need(Act("two",SoloAction.Zoo,"habitat-place","2","2/"+H().nextId).Accepted,"second child outer prop");
            var first=H().props[0];Need(!Act("one",SoloAction.Zoo,"habitat-move","1",first.id+"/0").Accepted && H().props[0].slot==0,"rejected move preserves original");
            Need(!Act("three",SoloAction.Zoo,"habitat-place","1","0/"+H().nextId).Accepted,"canopy entire envelope");
            var baseline=w.ReadZoo().animals[0].fed;var saved=Json(w.Snapshot());var restored=GameWorld.Restore(saved);
            Need(restored.ReadZoo().habitat.props.Length==2 && restored.ReadZoo().habitat.props[0].creator=="one" && restored.ReadZoo().animals[0].fed==baseline,"versioned save roundtrip retains placements and progress");
            Need(restored.ReadZoo().habitat.usingId=="" && restored.ReadZoo().habitat.pendingId=="","restore no routines/previews");
            w.ReleaseZoo("one");Need(H().props.Length==2 && H().props[0].creator=="one","creator disconnect preserves property");
            var malformed=Json(saved);malformed.zoo.habitat.props=malformed.zoo.habitat.props.Concat(new[]{new ElephantProp{id="bad",slot=99},malformed.zoo.habitat.props[0].Copy(),new ElephantProp{id="habitat-100",creator="one",kind=(ElephantPropKind)99}}).ToArray();
            var repaired=GameWorld.Restore(malformed);Need(repaired.ReadZoo().habitat.props.Length==2 && repaired.ReadZoo().habitat.recoveredEntries==3 && repaired.ReadZoo().animals[0].fed==baseline,"malformed entries isolated and diagnosed");
            saved.zoo.habitat=null;var legacy=GameWorld.Restore(Json(saved));Need(legacy.ReadZoo().habitat.props.Length==0 && legacy.ReadZoo().animals[0].fed==baseline,"old saves empty pilot without progress loss");
#if UNITY_EDITOR
            Debug.Log("ELEPHANT_HABITAT_PASS: legal envelopes/anchors, food priority, in-use guard, atomic conflicts/creator ownership, save/legacy/malformed recovery");
#else
            Console.WriteLine("PASS elephant habitat authority, envelopes, food priority, creator ownership and save recovery");
#endif
        }
    }
}
