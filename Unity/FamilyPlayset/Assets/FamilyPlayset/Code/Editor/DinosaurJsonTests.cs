using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class DinosaurJsonTests
    {
        public static void Run()
        {
            foreach(var id in DinosaurRides.Species){Need(Resources.Load<Texture2D>("DinosaurWorld/"+id)!=null,"2D atlas "+id);Need(Resources.Load<AudioClip>("DinosaurWorldAudio/"+id)!=null,"book call "+id);}
            Need(Resources.Load<Sprite>("WorldMenu/dinosaur-world")!=null,"picture menu");Need(Resources.Load<Texture2D>("DinosaurWorld/rider-poses")!=null,"rider artwork");
            var old=SoloWorld.WithZoo(SoloWorld.Create("one","two","three","four"));var before=old.Snapshot();var w=SoloWorld.WithDinosaurWorld(old);var s=w.Snapshot();s.schema=before.schema;s.revision--;s.dinosaurWorld=null;Need(JsonUtility.ToJson(s)==JsonUtility.ToJson(before),"additive migration");
            for(var t=0;t<7;t++)w.AdvanceIdle(1,out _);
            for(var i=0;i<4;i++){var who=before.players[i].id;
                void Act(SoloAction action,string value,string target=""){var p=w.ReadPlayer(who);var r=w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=who,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target});Need(r.Accepted,r.Outcome);}
                Act(SoloAction.Travel,DinosaurRides.Area);Act(SoloAction.Dinosaur,"mount",DinosaurRides.Species[i]);}
            var encoded=JsonUtility.ToJson(w.Snapshot());var decoded=JsonUtility.FromJson<SoloSnapshot>(encoded);SoloWorld.Validate(decoded);var recovered=SoloWorld.Restore(decoded);
            Need(recovered.ReadPlayers().All(p=>p.fixture==""),"leases released");Need(JsonUtility.ToJson(recovered.ReadDinosaurWorld())==JsonUtility.ToJson(w.ReadDinosaurWorld()),"positions/RNG retained");
            Debug.Log("DINOSAUR_JSON_PASS: assets, additive migration, four mounts, uint RNG roundtrip and released recovery leases");
        }
        static void Need(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
