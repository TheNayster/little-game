using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools {
 public static class SandpitJsonTests {
  public static void Run(){
   var w=GameWorld.WithDinosaurWorld(GameWorld.Create("one","two","three","four"));
   for(var format=0;format<=3;format++){
    var old=w.Snapshot();old.schema=format==0?49:format==1?50:format==2?51:52;
    var g=old.sandpit;g.format=format;g.phase=2;g.round=7;g.teacherX=4020;g.teacherY=440;
    g.moulds=Enumerable.Range(0,4).Select(i=>{
     var shape=i==0?"round":i==1?"square":i==2?"wall":"gate";var orientation=i==3?90:0;
     if(format<2)shape="round";if(format<2)orientation=0;
     var at=format==0?DaycareSandpit.Place(i):DaycareSandpit.PiecePoint(i==2?4:i==3?7:i*2,0,shape,orientation);
     var m=new SandMould{id="kept~piece-"+i,creator=i==0?"":"one",shape=shape,orientation=orientation,x=at.X,y=at.Y,width=format==0?140:DaycareSandpit.Width(shape,orientation),depth=format==0?100:DaycareSandpit.Depth(shape,orientation),capacity=format==0?DaycareSandpit.Capacity(i):3,scoops=format==0?DaycareSandpit.Capacity(i):3,wet=true,built=true,version=i+1};
     if(format<3)m.decoration=i%2+1;
     else m.attachments=Enumerable.Range(0,6).Select(slot=>new SandAttachment{slot=slot,kind=slot<2?"flag":slot<4?"shell":"window"}).ToArray();
     return m;
    }).ToArray();
    if(format==3){g.moulds[1].built=false;g.moulds[1].scoops=2;g.moulds[1].attachments=Array.Empty<SandAttachment>();g.toy=new SandToy{placed=true,x=4092,y=460,version=2,reaction=1};}
    var before=g.moulds.Select(m=>m.Copy()).ToArray();var unrelated=JsonUtility.ToJson(old.kingdom);var toys=JsonUtility.ToJson(old.toys);
    var json=JsonUtility.ToJson(old).Replace(",\"ground\":[]","");
    w=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(json)));
    if(w.Schema!=53 || w.WorldId!=old.worldId || JsonUtility.ToJson(w.ReadKingdom())!=unrelated || JsonUtility.ToJson(w.Snapshot().toys)!=toys)throw new InvalidOperationException("Decoration migration changed unrelated data.");
    var migrated=w.ReadSandpit();
    for(var i=0;i<4;i++){
     var m=migrated.moulds[i];var prev=before[i];
     if(m.x!=prev.x || m.y!=prev.y || m.capacity!=prev.capacity || m.scoops!=prev.scoops || m.wet!=prev.wet || m.built!=prev.built || format>0 && (m.id!=prev.id || m.creator!=prev.creator || m.version!=prev.version || m.shape!=prev.shape || m.orientation!=prev.orientation))throw new InvalidOperationException("Decoration migration changed piece/progress.");
     if(m.attachments.Length!=(format<3?1:prev.attachments.Length))throw new InvalidOperationException("Decoration migration lost or duplicated props.");
     foreach(var a in m.attachments){var at=SandDecorSurface.LegacyAnchor(m,a.slot);if(a.x!=at.X || a.y!=at.Y || !a.legacy || string.IsNullOrEmpty(a.id))throw new InvalidOperationException("Decoration visibly relocated.");}
    }
    if(format==3 && JsonUtility.ToJson(migrated.toy)!=JsonUtility.ToJson(g.toy))throw new InvalidOperationException("Decoration migration lost toy.");
    var reopened=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot()))));
    if(JsonUtility.ToJson(reopened.ReadSandpit())!=JsonUtility.ToJson(migrated))throw new InvalidOperationException("Positioned decoration JSON retention/idempotence failed.");
   }
   var save=w.Snapshot();save.sandpit.ground=new[]{new SandAttachment{id="ground-proof",creator="two",kind="shell",x=4092,y=130}};
   var piece=save.sandpit.moulds[1];piece.scoops=piece.capacity;piece.wet=true;piece.built=true;piece.attachments=new[]{new SandAttachment{id="window-a",kind="window",x=-40,y=65},new SandAttachment{id="window-b",kind="window",x=0,y=100}};
   var retained=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(save)));
   if(JsonUtility.ToJson(retained.ReadSandpit())!=JsonUtility.ToJson(save.sandpit))throw new InvalidOperationException("Flexible and ground decoration JSON retention failed.");
   foreach(var name in new[]{"count","water","tip","play","crumble","done"})if(LittleWeeps.Client.WorldResources.Load<AudioClip>("Worlds/Daycare/SandcastleClub/"+name)==null)throw new InvalidOperationException("Missing sand lesson voice "+name);
   Debug.Log("SANDPIT_JSON_PASS: legacy formats0/1/2/3 to positioned format4, exact P9 anchors, identities/orientations/progress/toy/unrelated data, reopen/idempotence and flexible attached/ground fields.");
  }
 }
}
