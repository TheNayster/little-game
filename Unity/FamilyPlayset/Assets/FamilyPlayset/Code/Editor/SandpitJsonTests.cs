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
   if(w.ReadSandpit().moulds.Length!=0)throw new InvalidOperationException("A new creative sandpit must start empty.");
   // Exercise a real pre-Stage-2 checkpoint, then reopen migrated and newly placed pieces.
   var s=w.Snapshot();s.schema=DaycareSandpit.PieceSchema-1;s.sandpit.format=0;s.sandpit.pieceLimit=0;s.sandpit.scoopCapacity=0;
   s.sandpit.round=1;s.sandpit.phase=3;s.sandpit.teacherX=4020;s.sandpit.teacherY=440;
   s.sandpit.moulds=Enumerable.Range(0,4).Select(i=>new SandMould{scoops=DaycareSandpit.Capacity(i),wet=true,built=true,decoration=i%2+1}).ToArray();
   var migrated=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s))));
   var legacy=migrated.ReadSandpit();
   if(legacy.format!=1 || legacy.moulds.Length!=4 || legacy.round!=1 || legacy.phase!=3)throw new InvalidOperationException("Legacy castle migration lost shared progress.");
   for(var i=0;i<4;i++){var m=legacy.moulds[i];var at=DaycareSandpit.Place(i);if(m.id!="legacy-"+i || !string.IsNullOrEmpty(m.creator) || m.x!=at.X || m.y!=at.Y || m.capacity!=DaycareSandpit.Capacity(i) || m.scoops!=m.capacity || !m.wet || !m.built || m.decoration!=i%2+1)throw new InvalidOperationException("Legacy castle migration lost location, fill or decoration.");}
   s=migrated.Snapshot();var place=DaycareSandpit.Cell(0,0);
   s.sandpit.moulds=s.sandpit.moulds.Concat(new[]{new SandMould{id="round-piece",creator="one",shape="round",x=place.X,y=place.Y,width=DaycareSandpit.PieceWidth,depth=DaycareSandpit.PieceDepth,capacity=DaycareSandpit.DefaultScoops,scoops=2,version=2}}).ToArray();
   var reopened=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s)));if(JsonUtility.ToJson(reopened.ReadSandpit())!=JsonUtility.ToJson(s.sandpit))throw new InvalidOperationException("Sand castle JSON retention failed.");
   foreach(var name in new[]{"count","water","tip","play","crumble","done"})if(LittleWeeps.Client.WorldResources.Load<AudioClip>("Worlds/Daycare/SandcastleClub/"+name)==null)throw new InvalidOperationException("Missing sand lesson voice "+name);
   Debug.Log("SANDPIT_JSON_PASS: additive45 and legacy49 migrations, empty creative start, earlier Adventure retained, legacy/new piece/cast roundtrip and six teaching clips.");
  }
 }
}
