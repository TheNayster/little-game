using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class KingdomJsonTests
    {
        public static void Run()
        {
            var old=SoloWorld.WithOutfits(SoloWorld.Create("one","two","three","four"));
            var prior=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot()));SoloWorld.Validate(prior);
            var world=SoloWorld.WithKingdom(SoloWorld.Restore(prior));
            var p=world.ReadPlayer("one");
            void Do(SoloAction action,string value){p=world.ReadPlayer("one");var r=world.Apply(new SoloCommand{actor=p.id,requestId=Guid.NewGuid().ToString("N"),zone=p.zone,visit=p.visit,expectedRevision=world.Revision,action=action,value=value});if(!r.Accepted)throw new InvalidOperationException(r.Outcome);}
            Do(SoloAction.Travel,"daycare");Do(SoloAction.Kingdom,"start");world.AdvanceIdle(1,out _);
            var snapshot=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(world.Snapshot()));SoloWorld.Validate(snapshot);
            if(snapshot.kingdom.round!=1 || !snapshot.kingdom.members[0].attending || snapshot.kingdom.members.Skip(1).Any(m=>m.attending) || snapshot.worldId!=prior.worldId)throw new InvalidOperationException("Unity JSON lost shared kingdom identity or membership.");
            if(SoloWorld.Restore(snapshot).ReadKingdom().round!=1)throw new InvalidOperationException("Kingdom checkpoint did not reopen.");
            if(Resources.Load<Texture2D>("Scenery/daycare-adventure")==null)throw new InvalidOperationException("Missing kingdom panorama.");
            Debug.Log("Kingdom JSON passed: additive upgrade, four memberships, authoritative clock, saved round and retained world.");
        }
    }
}
