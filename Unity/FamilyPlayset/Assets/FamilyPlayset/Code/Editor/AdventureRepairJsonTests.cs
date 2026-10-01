using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools {
 public static class AdventureRepairJsonTests {
  public static void Run(){
   var world=SoloWorld.WithDinosaurWorld(SoloWorld.Create("one","two","three","four"));var old=world.Snapshot();old.schema=42;old.kingdom.boardAt=null;old.kingdom.wakeAt=null;foreach(var member in old.kingdom.members)member.carrying=null;
   world=SoloWorld.WithDinosaurWorld(SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old))));var snapshot=world.Snapshot();snapshot.kingdom.phase=KingdomPhase.Supplies;snapshot.kingdom.round=1;snapshot.kingdom.clock=4;snapshot.kingdom.started=3;snapshot.kingdom.members[0].attending=true;snapshot.kingdom.members[0].carrying="fruit-0";snapshot.players[0].zone=KingdomAdventure.Zone;
   var reopened=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(snapshot));SoloWorld.Validate(reopened);
   if(reopened.schema!=WorldLayout.Schema || reopened.worldId!=old.worldId || reopened.kingdom.members[0].carrying!="fruit-0" || reopened.kingdom.boardAt.Length!=3 || !reopened.kingdom.npcCast.SequenceEqual(old.kingdom.npcCast))throw new InvalidOperationException("Adventure upgrade or held-prop JSON lost saved data.");
   Debug.Log("Adventure repair JSON passed: retained42 world/cast, carried fruit and combined job clocks.");
  }
 }
}
