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
    public static class ZooFossilTests
    {
        static void Need(bool ok,string s){if(!ok)throw new Exception("Fossils: "+s);}
        static SoloSnapshot Json(SoloSnapshot s){
#if UNITY_EDITOR
            return JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));
#else
            var o=new JsonSerializerOptions{IncludeFields=true};return JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(s,o),o);
#endif
        }
        public static void Run()
        {
            var w=GameWorld.WithZoo(GameWorld.Create("one","two","three","four"));var ids=new[]{"one","two","three","four"};
            SoloResult Act(string id,SoloAction a,string v="",string t="",string item="",float x=470){var p=w.ReadPlayer(id);return w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=a,value=v,target=t,item=item,x=x,y=100});}
            ZooFossilState F()=>w.ReadZoo().fossils;
            long serial=0;
            SoloResult Play(string id,string op,int piece=-1,string item=null){var f=F();var n=Array.IndexOf(f.members,id);return Act(id,SoloAction.Zoo,"fossil-"+op,piece.ToString(),item??(f.round+"/"+(n<0?0:f.epochs[n])+"/"+(++serial)));}
            void Step(){w.AdvanceIdle(.2,out _);GameWorld.Validate(Json(w.Snapshot()));}
            foreach(var id in ids){Need(Act(id,SoloAction.Travel,"zoo").Accepted,"travel");Need(Act(id,SoloAction.Move,x:1750).Accepted,"gate walk");Need(Act(id,SoloAction.Zoo,"gate",ZooCatalog.Dinosaurs).Accepted,"gate");}
            foreach(var id in ids)Need(Play(id,"join").Accepted,"four join");Need(F().members.Distinct().Count()==4,"four distinct positions");
            Need(!Play("one","replay").Accepted,"no midround erasure");
            for(var i=0;i<3;i++)for(var n=0;n<6;n++){Need(Play(ids[n%4],"brush",i).Accepted,"cooperative uncover");Step();}
            var detached=F();detached.revealed[0]=0;Need(F().revealed[0]==6,"detached arrays");
            Need(Play("one","pickup",0).Accepted,"pickup");Need(!Play("two","pickup",0).Accepted,"atomic claim");
            Need(!Play("one","place",1).Accepted && F().holders[0]=="one","wrong match retains earned piece");
            Need(!Act("one",SoloAction.Zoo,"take","brachiosaurus",x:1560).Accepted,"food while holding rejected");
            var saved=Json(w.Snapshot());var restored=GameWorld.Restore(saved);Need(restored.ReadZoo().fossils.revealed.All(n=>n==6) && restored.ReadZoo().fossils.holders.All(id=>id=="") && restored.ReadZoo().fossils.members.All(id=>id==""),"partial reopen, no holder locks");
            Need(Play("one","drop").Accepted && Play("two","pickup",0).Accepted,"put down and sibling pickup");
            w.ReleaseZoo("two");Need(F().holders[0]=="" && F().members.Contains("one"),"disconnect releases only own lock");Need(Play("two","join").Accepted,"rejoin");
            Need(Play("one","pickup",0).Accepted && Play("one","place",0).Accepted,"place once");Need(!Play("one","place",0).Accepted,"duplicate place");
            Need(Play("two","pickup",1).Accepted,"next pickup");Need(Act("two",SoloAction.Move,x:1200).Accepted && F().holders[1]=="","walking away releases");
            Need(Play("three","pickup",1).Accepted && Act("three",SoloAction.Travel,"creek").Accepted && F().holders[1]=="" && F().placed[0],"travel preserves sibling placement");
            foreach(var i in new[]{1,2})Need(Play("four","pickup",i).Accepted && Play("four","place",i).Accepted,"finish");Need(F().Complete,"completed display");
            var completed=GameWorld.Restore(Json(w.Snapshot()));Need(completed.ReadZoo().fossils.Complete && completed.ReadZoo().fossils.celebrationAge==10,"completed restore no old reaction");
            var old=F().round+"/"+F().epochs[0]+"/99";for(var i=0;i<12;i++)Step();Need(Play("one","replay").Accepted,"replay");Need(!Play("one","replay",item:old).Accepted && F().round==2,"two resets one round");Need(!Play("one","brush",0,old).Accepted && F().revealed[0]==0,"late round packets");
            var epoch=F().epochs[0];Need(Play("one","leave").Accepted && Play("one","join").Accepted,"independent leave/rejoin");Need(!Play("one","brush",0,"2/"+epoch+"/999").Accepted,"late visit packet");
            var token="2/"+F().epochs[Array.IndexOf(F().members,"one")]+"/1000";Need(Play("one","brush",0,token).Accepted,"one stroke");Step();Need(!Play("one","brush",0,token).Accepted && F().revealed[0]==1,"repeated stroke safe");
            Need(Act("one",SoloAction.Travel,"creek").Accepted,"exit");Need(Act("one",SoloAction.Travel,"zoo").Accepted && Act("one",SoloAction.Move,x:1750).Accepted && Act("one",SoloAction.Zoo,"gate",ZooCatalog.Dinosaurs).Accepted,"return");Need(Act("one",SoloAction.Move,x:1560).Accepted && Act("one",SoloAction.Zoo,"take","brachiosaurus").Accepted,"food queued");var ticket=w.ReadZoo().food.Single(f=>f.actor=="one").ticket;Need(Act("one",SoloAction.Move,x:470).Accepted && !Play("one","join").Accepted && w.ReadZoo().food.Single(f=>f.actor=="one").ticket==ticket,"food transition never discards queue");
            var legacy=Json(w.Snapshot());legacy.zoo.fossils=null;var legacyWorld=GameWorld.Restore(Json(legacy));Need(legacyWorld.ReadZoo().fossils.round==1 && legacyWorld.ReadZoo().animals.Length==16,"legacy absent field");
#if UNITY_EDITOR
            Debug.Log("ZOO_FOSSILS_PASS: solo/cooperation/claims/invalid/stale/drop/disconnect/travel/save/replay/food/legacy");
#else
            Console.WriteLine("PASS Zoo fossil authority, cooperative claims, lifecycle, replay, legacy/save and food guards");
#endif
        }
    }
}
