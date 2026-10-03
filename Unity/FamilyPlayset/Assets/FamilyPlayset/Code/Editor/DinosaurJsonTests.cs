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
            foreach(var id in DinosaurRides.Species){Need(LittleWeeps.Client.WorldResources.Load<Texture2D>("Worlds/Dinosaur/Art/"+id)!=null,"2D atlas "+id);Need(LittleWeeps.Client.WorldResources.Load<AudioClip>("Worlds/Dinosaur/Audio/"+id)!=null,"book call "+id);}
            Need(LittleWeeps.Client.WorldResources.Load<Sprite>("Shared/UI/WorldMenu/dinosaur-world")!=null,"picture menu");Need(LittleWeeps.Client.WorldResources.Load<Texture2D>("Worlds/Dinosaur/Art/rider-poses")!=null,"rider artwork");
            var old=GameWorld.WithZoo(GameWorld.Create("one","two","three","four"));var before=old.Snapshot();var w=GameWorld.WithDinosaurWorld(old);var s=w.Snapshot();Need(s.worldId==before.worldId && JsonUtility.ToJson(s.home)==JsonUtility.ToJson(before.home) && s.toys.Select(t=>JsonUtility.ToJson(t)).SequenceEqual(before.toys.Select(t=>JsonUtility.ToJson(t))),"combined additive migration retains home and inventory");
            for(var t=0;t<7;t++)w.AdvanceIdle(1,out _);
            for(var i=0;i<4;i++){var who=before.players[i].id;
                void Act(SoloAction action,string value,string target=""){var p=w.ReadPlayer(who);var r=w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=who,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target});Need(r.Accepted,r.Outcome);}
                Act(SoloAction.Travel,DinosaurRides.Area);Act(SoloAction.Dinosaur,"mount",DinosaurRides.Species[i]);}
            var encoded=JsonUtility.ToJson(w.Snapshot());var decoded=JsonUtility.FromJson<SoloSnapshot>(encoded);GameWorld.Validate(decoded);var recovered=GameWorld.Restore(decoded);
            Need(recovered.ReadPlayers().All(p=>p.fixture==""),"leases released");Need(JsonUtility.ToJson(recovered.ReadDinosaurWorld())==JsonUtility.ToJson(w.ReadDinosaurWorld()),"positions/RNG retained");
            var careWorld=GameWorld.WithDinosaurWorld(GameWorld.Create("one","two","three","four"));
            foreach(var who in before.players.Select(p=>p.id)){
                var i=Array.IndexOf(before.players.Select(p=>p.id).ToArray(),who);var id=DinosaurRides.Species[i];
                void CareAct(SoloAction action,string value="",string target="",float x=0,float y=0){var p=careWorld.ReadPlayer(who);var r=careWorld.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=who,expectedRevision=careWorld.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target,x=x,y=y});Need(r.Accepted,r.Outcome);}
                CareAct(SoloAction.Travel,DinosaurRides.Area);CareAct(SoloAction.Move,x:DinosaurCareRules.BucketX(id),y:100);CareAct(SoloAction.Dinosaur,"take",id);
                var f=careWorld.ReadDinosaurWorld().care[i];CareAct(SoloAction.Move,x:f.x,y:f.y);CareAct(SoloAction.Dinosaur,"offer",id);
            }
            for(var t=0;t<120;t++)careWorld.AdvanceIdle(.1,out _);
            Need(careWorld.ReadDinosaurWorld().animals.All(a=>a.fed==1),"four feeding counts");
            var careDecoded=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(careWorld.Snapshot()));GameWorld.Validate(careDecoded);var careRestored=GameWorld.Restore(careDecoded);
            Need(careRestored.ReadDinosaurWorld().animals.All(a=>a.fed==1) && careRestored.ReadDinosaurWorld().care.All(f=>f.phase==DinosaurCarePhase.None),"care JSON recovery");
            var version36=careWorld.Snapshot();version36.schema=36;version36.creekBoats=null;version36.creekFishing=null;version36.kingdom=null;version36.daycare=null;version36.sandpit=null;version36.treasure=null;version36.seagulls=null;version36.shore=null;version36.dinosaurWorld.care=null;version36.dinosaurWorld.nextCareTicket=0;foreach(var animal in version36.dinosaurWorld.animals)animal.fed=0;
            var prior36=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(version36)));var upgraded=GameWorld.WithDinosaurWorld(prior36);
            Need(upgraded.Schema==WorldLayout.Schema && upgraded.ReadDinosaurWorld().care.Length==4,"schema 36 care upgrade");
            Debug.Log("DINOSAUR_CARE_JSON_PASS: four meals, care records/progress, recovery and schema 36 upgrade");
            Debug.Log("DINOSAUR_JSON_PASS: assets, additive migration, four mounts, uint RNG roundtrip and released recovery leases");
        }
        static void Need(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
