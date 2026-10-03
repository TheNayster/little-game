using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static GameWorld SecretWorld()=>GameWorld.WithSecretRooms(FurnitureWorld());
    static SecretRoomState Secret(GameWorld w,int i=0)=>w.ReadSecrets().Single(r=>r.id==SecretRooms.Id(i));
    static void SecretEdit(GameWorld w,string target,int value=0,string actor="first",int index=0)
    {Good(w,SoloAction.SecretRoom,target:target,value:value+":"+Secret(w,index).entranceRevision,actor:actor);}
    static void MakeSecret(GameWorld w,int i=0,string actor="first")
    {FurnishVisit(w,i,actor);Good(w,SoloAction.Move,x:SecretRooms.DoorX(0),y:220,actor:actor);SecretEdit(w,"create",actor:actor,index:i);}
    static void EnterSecret(GameWorld w,int i=0,string actor="first")
    {Good(w,SoloAction.Move,x:SecretRooms.DoorX(Secret(w,i).slot),y:220,actor:actor);Good(w,SoloAction.EnterDoor,target:SecretRooms.Id(i),value:Secret(w,i).entranceRevision.ToString(),actor:actor);}
    static void SecretTests()
    {
        Test("secret migration appends four reserved records preserving every schema-eight field",()=>{
            var old=FurnitureWorld();var before=old.Snapshot();var w=GameWorld.WithSecretRooms(old);var after=w.Snapshot();
            Check(after.schema==9 && after.secrets.Length==4 && after.secrets.All(r=>!r.created) && after.toys.Length==27);
            after.schema=8;after.revision=before.revision;after.secrets=Array.Empty<SecretRoomState>();Check(Encode(after)==Encode(before));
            Check(ReferenceEquals(w,GameWorld.WithSecretRooms(w)));var copy=w.ReadSecrets();copy[0].furniture.theme=3;Check(Secret(w).furniture.theme==0);
        });
        Test("four owner creates add exactly six persistent plush each and repeat never refills",()=>{
            var w=SecretWorld();var players=w.Snapshot().players;
            for(var i=0;i<4;i++){MakeSecret(w,i,players[i].id);EnterSecret(w,i,players[i].id);}
            Check(w.ReadToys().Length==51 && w.Snapshot().players.Select(p=>p.zone).Distinct().Count()==4);GameWorld.Validate(w.Snapshot());
            var stock=Encode(w.ReadToys());SecretEdit(w,"create");Check(Encode(w.ReadToys())==stock);Good(w,SoloAction.ChangeAvatar,value:"orange-pup");Check(Secret(w).owner=="first");
        });
        Test("secret creation requires owner and proximity while reveal has stable hysteresis",()=>{
            var w=SecretWorld();FurnishVisit(w);Check(!w.Apply(Command(w,SoloAction.SecretRoom,target:"create",value:"0:1")).Accepted);
            FurnishVisit(w,0,"second");Good(w,SoloAction.Move,x:2310,y:220,actor:"second");Check(!w.Apply(Command(w,SoloAction.SecretRoom,target:"create",value:"0:1",actor:"second")).Accepted);
            Check(!SecretRooms.Reveal(false,325,420,0) && SecretRooms.Reveal(false,2100,220,0));Check(!SecretRooms.Reveal(false,2000,220,0) && SecretRooms.Reveal(true,2000,220,0));
        });
        Test("secret entry rejects stale entrance revisions and visits without moving anyone",()=>{
            var w=SecretWorld();MakeSecret(w);var entry=Command(w,SoloAction.EnterDoor,target:SecretRooms.Id(0),value:Secret(w).entranceRevision.ToString());
            SecretEdit(w,"slot",1);entry.expectedRevision=w.Revision;var before=Encode(w.Snapshot());Check(!w.Apply(entry).Accepted && Encode(w.Snapshot())==before);
            EnterSecret(w);var stale=Command(w,SoloAction.ReturnBedroom);Good(w,SoloAction.ReturnBedroom);stale.expectedRevision=w.Revision;Check(!w.Apply(stale).Accepted);
        });
        Test("four fort places are independent and saved furniture arrangement carries occupants",()=>{
            var w=SecretWorld();MakeSecret(w);var players=w.Snapshot().players;
            for(var i=0;i<4;i++){if(i>0)FurnishVisit(w,0,players[i].id);EnterSecret(w,0,players[i].id);Good(w,SoloAction.UseFixture,target:SecretRooms.Fort(i),actor:players[i].id);}
            Check(!w.Apply(Command(w,SoloAction.UseFixture,target:SecretRooms.Fort(1))).Accepted);var r=Secret(w).furniture;
            Good(w,SoloAction.DecorateRoom,target:"layout",value:"1:"+r.roomRevision);Check(w.ReadPlayer("second").x==BedroomFurniture.SeatX(SecretRooms.Fort(1),1));
            var others=Encode(w.Snapshot().players.Skip(1).ToArray());Good(w,SoloAction.ReturnBedroom);Check(Encode(w.Snapshot().players.Skip(1).ToArray())==others);GameWorld.Validate(w.Snapshot());
        });
        Test("all four cushions coexist and secret-only fort cannot appear in a bedroom",()=>{
            var w=SecretWorld();MakeSecret(w);Check(!w.Apply(Command(w,SoloAction.UseFixture,target:SecretRooms.Fort(0))).Accepted);EnterSecret(w);
            Check(!w.Apply(Command(w,SoloAction.UseFixture,target:BedroomFurniture.Bed)).Accepted);
            var players=w.Snapshot().players;for(var i=0;i<4;i++){if(i>0){FurnishVisit(w,0,players[i].id);EnterSecret(w,0,players[i].id);}Good(w,SoloAction.UseFixture,target:BedroomFurniture.Cushion(i),actor:players[i].id);}
            GameWorld.Validate(w.Snapshot());
        });
        Test("hiding and moving an occupied entrance preserves contents and normal exit after owner leaves",()=>{
            var w=SecretWorld();MakeSecret(w);EnterSecret(w);FurnishVisit(w,0,"second");EnterSecret(w,0,"second");var stock=Encode(w.ReadToys());
            SecretEdit(w,"active",0);SecretEdit(w,"slot",1);Good(w,SoloAction.ReturnBedroom);Good(w,SoloAction.Travel,value:"park");
            w=GameWorld.Restore(w.Snapshot());Good(w,SoloAction.Move,x:325,y:420,actor:"second");Good(w,SoloAction.EnterDoor,target:BedroomLayout.Id(0),actor:"second");
            Check(w.ReadPlayer("second").zone==BedroomLayout.Id(0) && Encode(w.ReadToys())==stock && !Secret(w).active);GameWorld.Validate(w.Snapshot());
        });
        Test("held plush crosses exit and entry exactly once and closed chest retains same object",()=>{
            var w=SecretWorld();MakeSecret(w);EnterSecret(w);var id=SecretRooms.ToyId(0,0);Good(w,SoloAction.Grab,id);Good(w,SoloAction.ReturnBedroom);
            Check(w.ReadToys().Single(t=>t.id==id).holder=="first" && w.ReadToys().Single(t=>t.id==id).zone==BedroomLayout.Id(0));EnterSecret(w);
            Good(w,SoloAction.Drop,id,target:BedroomFurniture.Storage(SecretRooms.Id(0),2),x:BedroomFurniture.StorageX(0,2),y:BedroomFurniture.StorageY(2));
            Good(w,SoloAction.SetFixture,target:"bedroom-chest",value:"off");w=GameWorld.Restore(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Grab,id)).Accepted);
            Good(w,SoloAction.SetFixture,target:"bedroom-chest",value:"on");Good(w,SoloAction.Grab,id);Check(w.ReadToys().Length==33);GameWorld.Validate(w.Snapshot());
        });
        Test("secret decoration permissions undo and tidy protect parent-owned toys and visitors",()=>{
            var w=SecretWorld();MakeSecret(w);EnterSecret(w);FurnishVisit(w,0,"second");EnterSecret(w,0,"second");var r=Secret(w).furniture;
            Check(!w.Apply(Command(w,SoloAction.DecorateRoom,target:"theme",value:"1:"+r.roomRevision,actor:"second")).Accepted);
            Good(w,SoloAction.DecorateRoom,target:"together",value:"1:"+r.roomRevision);r=Secret(w).furniture;
            Good(w,SoloAction.DecorateRoom,target:"theme",value:"2:"+r.roomRevision,actor:"second");r=Secret(w).furniture;
            Good(w,SoloAction.DecorateRoom,target:"undo",value:"0:"+r.roomRevision,actor:"second");Check(Secret(w).furniture.theme==0);
            var id=SecretRooms.ToyId(0,0);Good(w,SoloAction.Grab,id);Good(w,SoloAction.Drop,id,x:1100,y:150);r=Secret(w).furniture;
            Good(w,SoloAction.DecorateRoom,target:"tidy",value:"0:"+r.roomRevision);Check(w.ReadToys().Single(t=>t.id==id).container!="");GameWorld.Validate(w.Snapshot());
        });
        Test("secret save rejects altered parents ownership uncreated occupants and plush duplication",()=>{
            var w=SecretWorld();var s=w.Snapshot();s.secrets[0].parent=BedroomLayout.Id(1);Throws(()=>GameWorld.Restore(s));
            s=w.Snapshot();s.players[0].zone=SecretRooms.Id(0);Throws(()=>GameWorld.Restore(s));
            MakeSecret(w);s=w.Snapshot();s.toys.Last().personalRoom=BedroomLayout.Id(1);Throws(()=>GameWorld.Restore(s));
            s=w.Snapshot();s.secrets[0].furniture.owner="second";Throws(()=>GameWorld.Restore(s));
            s=w.Snapshot();s.secrets[0].entranceRevision=long.MaxValue;Throws(()=>GameWorld.Restore(s));
        });
    }
}
