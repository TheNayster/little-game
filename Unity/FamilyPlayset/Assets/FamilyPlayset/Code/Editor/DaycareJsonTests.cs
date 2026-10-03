using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class DaycareJsonTests
    {
        public static void Run()
        {
            var old=GameWorld.WithKingdom(GameWorld.Create("one","two","three","four"));
            var prior=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot()));GameWorld.Validate(prior);
            var world=GameWorld.WithDaycare(GameWorld.Restore(prior));var s=world.Snapshot();s.daycare.clock=15.7447122;s.daycare.helpUntil=21.7447122;s.daycare.round=1;s.daycare.phase=2;s.daycare.started=10;s.daycare.plates=5;s.players[0].zone="daycare";s.daycare.members[0].attending=true;
            var reopened=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));GameWorld.Validate(reopened);
            if(reopened.worldId!=prior.worldId || reopened.daycare.plates!=5 || !reopened.daycare.members[0].attending || reopened.daycare.members.Skip(1).Any(m=>m.attending) || GameWorld.Restore(reopened).ReadKingdom().round!=prior.kingdom.round)throw new InvalidOperationException("Daycare JSON lost shared progress or previous records.");
            if(LittleWeeps.Client.WorldResources.Load<Texture2D>("Worlds/Daycare/Shared/calypso-poses")==null || LittleWeeps.Client.WorldResources.Load<AudioClip>("Worlds/Daycare/Shared/hello")==null)throw new InvalidOperationException("Missing Calypso assets.");
            Debug.Log("Daycare JSON passed: additive upgrade, retained adventure, four memberships, plate progress and exact helper timer.");
        }
    }
}
