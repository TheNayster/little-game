using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools {
 public static class SandpitJsonTests {
  public static void Run(){
   var w=GameWorld.WithDinosaurWorld(GameWorld.Create("one","two","three","four"));var old=w.Snapshot();old.schema=45;old.sandpit=null;old.treasure=null;
   var prior=JsonUtility.ToJson(old.kingdom);w=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old))));
   if(w.Schema!=WorldLayout.Schema || JsonUtility.ToJson(w.ReadKingdom())!=prior || w.Snapshot().worldId!=old.worldId)throw new InvalidOperationException("Sandpit migration changed earlier world.");
   var s=w.Snapshot();s.sandpit.round=1;s.sandpit.phase=3;s.sandpit.teacherX=4020;s.sandpit.teacherY=440;
   for(var i=0;i<4;i++){s.sandpit.moulds[i].scoops=DaycareSandpit.Capacity(i);s.sandpit.moulds[i].wet=true;s.sandpit.moulds[i].built=true;s.sandpit.moulds[i].decoration=i%2+1;}
   var reopened=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s)));if(JsonUtility.ToJson(reopened.ReadSandpit())!=JsonUtility.ToJson(s.sandpit))throw new InvalidOperationException("Sand castle JSON retention failed.");
   foreach(var name in new[]{"count","water","tip","play","crumble","done"})if(LittleWeeps.Client.WorldResources.Load<AudioClip>("Worlds/Daycare/SandcastleClub/"+name)==null)throw new InvalidOperationException("Missing sand lesson voice "+name);
   Debug.Log("SANDPIT_JSON_PASS: additive45 migration, earlier Adventure retained, complete castle/decorations/cast roundtrip and six teaching clips.");
  }
 }
}
