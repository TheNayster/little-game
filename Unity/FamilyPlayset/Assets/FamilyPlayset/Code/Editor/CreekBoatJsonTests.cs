using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class CreekBoatJsonTests
    {
        public static void Run()
        {
            var old=GameWorld.WithPond(GameWorld.Create("one","two","three","four"));
            var legacy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot()));GameWorld.Validate(legacy);
            var world=GameWorld.WithCreekBoats(GameWorld.Restore(legacy));
            foreach(var action in new[]{"start","passenger","flower","launch"}){var p=world.ReadPlayer("one");
                if(!world.Apply(new SoloCommand{actor=p.id,requestId=Guid.NewGuid().ToString("N"),zone=p.zone,visit=p.visit,expectedRevision=world.Revision,action=SoloAction.CreekBoat,value=action}).Accepted)throw new InvalidOperationException("Native creek boat action failed.");}
            var state=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(world.Snapshot()));GameWorld.Validate(state);
            var restored=GameWorld.Restore(state);var boat=restored.ReadCreekBoats().boats.Single(b=>b.actor=="one");
            if(boat.phase!=CreekBoatPhase.Docked || !boat.passenger || !boat.flower || boat.attending || restored.WorldId!=old.WorldId)throw new InvalidOperationException("Unity JSON lost boat creation or retained a temporary lease.");
            if(LittleWeeps.Client.WorldResources.Load<Texture2D>("Worlds/Creek/Boats/boats")==null)throw new InvalidOperationException("Creek boat atlas not imported as 2D.");
            Debug.Log("Creek boat JSON checks passed: additive old-save upgrade, decorated boats and reachable recovery dock.");
        }
    }
}
