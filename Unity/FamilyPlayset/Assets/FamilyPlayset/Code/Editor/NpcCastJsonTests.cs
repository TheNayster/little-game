using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools {
 public static class NpcCastJsonTests {
  public static void Run(){
   var old=SoloWorld.WithDaycare(SoloWorld.Create("one","two","three","four"));var prior=old.Snapshot();prior.daycare.round=1;prior.daycare.phase=2;prior.daycare.plates=5;
   var upgraded=SoloWorld.WithNpcCasts(SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(prior))));var saved=upgraded.Snapshot();var reopened=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(saved));SoloWorld.Validate(reopened);
   if(!DaycareNpcCasts.Valid(reopened.kingdom.npcCast,9) || !DaycareNpcCasts.Valid(reopened.daycare.guests,4) || !saved.kingdom.npcCast.SequenceEqual(reopened.kingdom.npcCast) || !saved.daycare.guests.SequenceEqual(reopened.daycare.guests) || reopened.daycare.plates!=5 || reopened.worldId!=prior.worldId)throw new InvalidOperationException("NPC cast or existing progress lost in Unity JSON upgrade/reopening.");
   Debug.Log("NPC cast JSON passed: distinct shared casts, retained progress and exact identities after reopening.");
  }
 }
}
