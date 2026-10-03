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
   if(legacy.format!=DaycareSandpit.Format || legacy.moulds.Length!=4 || legacy.round!=1 || legacy.phase!=3)throw new InvalidOperationException("Legacy castle migration lost shared progress.");
   for(var i=0;i<4;i++){var m=legacy.moulds[i];var at=DaycareSandpit.Place(i);if(m.id!="legacy-"+i || !string.IsNullOrEmpty(m.creator) || m.x!=at.X || m.y!=at.Y || m.capacity!=DaycareSandpit.Capacity(i) || m.scoops!=m.capacity || !m.wet || !m.built || m.decoration!=0 || m.attachments.Length!=1 || m.attachments[0].kind!=(i%2==0?"flag":"shell"))throw new InvalidOperationException("Legacy castle migration lost location, fill or decoration.");}
   s=migrated.Snapshot();var place=DaycareSandpit.Cell(0,0);
   s.sandpit.moulds=s.sandpit.moulds.Concat(new[]{new SandMould{id="round-piece",creator="one",shape="round",x=place.X,y=place.Y,width=DaycareSandpit.PieceWidth,depth=DaycareSandpit.PieceDepth,capacity=DaycareSandpit.DefaultScoops,scoops=2,version=2}}).ToArray();
   var reopened=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s)));if(JsonUtility.ToJson(reopened.ReadSandpit())!=JsonUtility.ToJson(s.sandpit))throw new InvalidOperationException("Sand castle JSON retention failed.");
   var stageTwo=reopened.Snapshot();stageTwo.schema=50;stageTwo.sandpit.format=1;
   foreach(var m in stageTwo.sandpit.moulds){m.decoration=m.attachments.Length==0?0:m.attachments[0].kind=="flag"?1:2;m.attachments=null;}
   var stageTwoPieces=JsonUtility.ToJson(stageTwo.sandpit);
   var upgraded=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(stageTwo))));
   stageTwo.sandpit.format=DaycareSandpit.Format;foreach(var m in stageTwo.sandpit.moulds){m.attachments=m.decoration==0?Array.Empty<SandAttachment>():new[]{new SandAttachment{slot=m.decoration==1?0:2,kind=m.decoration==1?"flag":"shell"}};m.decoration=0;}
   if(JsonUtility.ToJson(upgraded.ReadSandpit())!=JsonUtility.ToJson(stageTwo.sandpit))throw new InvalidOperationException("Stage 2 castle fields changed during Stage 3 migration.");
   s=upgraded.Snapshot();
   foreach(var choice in new[]{"square","wall","gate:90"}){
    DaycareSandpit.ReadChoice(choice,out var shape,out var orientation);var at=DaycareSandpit.PiecePoint(shape=="square"?5:shape=="wall"?2:7,0,shape,orientation);
    s.sandpit.moulds=s.sandpit.moulds.Concat(new[]{new SandMould{id=choice,creator="two",shape=shape,orientation=orientation,x=at.X,y=at.Y,width=DaycareSandpit.Width(shape,orientation),depth=DaycareSandpit.Depth(shape,orientation),capacity=3,scoops=3,wet=true,built=true}}).ToArray();
   }
   // Real format-two checkpoint: legacy integer decor becomes one attachment.
   var priorThree=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));priorThree.schema=51;priorThree.sandpit.format=2;
   foreach(var m in priorThree.sandpit.moulds){m.decoration=m.attachments.Length>0?(m.attachments[0].kind=="flag"?1:2):0;m.attachments=null;}
   // Omit fields exactly as a real Stage 3 save does. Null/empty current fields
   // are not sufficient: Unity handles absent inline classes differently.
   var sandJson=JsonUtility.ToJson(priorThree.sandpit);var oldSandJson=sandJson.Replace(",\"attachments\":[]","").Replace(",\"toy\":"+JsonUtility.ToJson(priorThree.sandpit.toy),"").Replace(",\"reset\":"+JsonUtility.ToJson(priorThree.sandpit.reset),"");
   if(oldSandJson.Contains("\"toy\"") || oldSandJson.Contains("\"reset\"") || oldSandJson.Contains("\"attachments\""))throw new InvalidOperationException("Stage 3 fixture contains Stage 4 fields.");
   var oldJson=JsonUtility.ToJson(priorThree).Replace(sandJson,oldSandJson);
   var stageFour=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(oldJson)));
   if(JsonUtility.ToJson(stageFour.ReadSandpit().moulds)!=JsonUtility.ToJson(s.sandpit.moulds))throw new InvalidOperationException("Stage 3 attachment migration changed existing fields.");
   s.sandpit.moulds[0].attachments=s.sandpit.moulds[0].attachments.Concat(new[]{new SandAttachment{slot=2,kind="shell"},new SandAttachment{slot=4,kind="door"}}).ToArray();
   s.sandpit.toy=new SandToy{placed=true,x=4092,y=130,version=2,reaction=1};
   reopened=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s)));
   if(JsonUtility.ToJson(reopened.ReadSandpit())!=JsonUtility.ToJson(s.sandpit))throw new InvalidOperationException("Stage 4 combined attachments and toy failed Unity JSON reopening.");
   foreach(var name in new[]{"count","water","tip","play","crumble","done"})if(LittleWeeps.Client.WorldResources.Load<AudioClip>("Worlds/Daycare/SandcastleClub/"+name)==null)throw new InvalidOperationException("Missing sand lesson voice "+name);
   Debug.Log("SANDPIT_JSON_PASS: additive45, legacy49 and Stage2 format1 migration, round/square/wall/rotated gate Unity JSON retention, earlier Adventure retained and six teaching clips; Stage 3 attachment conversion and Stage 4 combined decorations/toy retention.");
  }
 }
}
