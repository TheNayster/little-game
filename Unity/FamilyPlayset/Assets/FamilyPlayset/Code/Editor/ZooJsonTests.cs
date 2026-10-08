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
            Debug.Log("ZOO_JSON_PASS: additive migration, four portions, uint RNG JSON, retention and visitor boundary");
        }
        static void Need(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
