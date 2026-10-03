using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static GameWorld FurnitureWorld()=>GameWorld.WithFurnishedRooms(BedroomWorld());
    static string Encode<T>(T[] values)=>System.Text.Json.JsonSerializer.Serialize(values,Json);
    static void FurnishVisit(GameWorld w,int room=0,string actor="first")
    {if(w.ReadPlayer(actor).zone=="garden")Upstairs(w,actor);if(BedroomLayout.Index(w.ReadPlayer(actor).zone)>=0)Door(w,HomeRooms.Landing,actor);Door(w,BedroomLayout.Id(room),actor);}
    static BedroomState Room(GameWorld w,int i=0)=>w.ReadBedrooms().Single(r=>r.id==BedroomLayout.Id(i));
    static void EditRoom(GameWorld w,string kind,int value,string actor="first")
    {var r=w.ReadBedrooms().Single(v=>v.id==w.ReadPlayer(actor).zone);Good(w,SoloAction.DecorateRoom,target:kind,value:value+":"+r.roomRevision,actor:actor);}
    static void ShelfDrop(GameWorld w,string item,int slot,int room=0,string actor="first")
    {Good(w,SoloAction.Drop,item,target:BedroomFurniture.Storage(BedroomLayout.Id(room),slot),x:BedroomFurniture.StorageX(Room(w,room).layout,slot),y:BedroomFurniture.StorageY(slot),actor:actor);}
    static void FurnitureTests()
    {
        Test("furniture migration adds bounded stock and room defaults once preserving prior state",()=>{
            var old=BedroomWorld();var prior=old.Snapshot();var w=GameWorld.WithFurnishedRooms(old);var s=w.Snapshot();
            Check(s.schema==8 && s.toys.Length==27 && s.bedrooms.All(r=>r.roomRevision==1));Check(ReferenceEquals(w,GameWorld.WithFurnishedRooms(w)));
            s.schema=7;s.revision=prior.revision;s.toys=s.toys.Where(t=>!BedroomFurniture.Personal(t.kind)).ToArray();
            foreach(var r in s.bedrooms){r.theme=0;r.roomRevision=0;}
            Check(Encode(s)==Encode(prior));GameWorld.Validate(w.Snapshot());
        });
        Test("legacy absent decoration strings migrate safely and source copies remain detached",()=>{
            var old=BedroomWorld().Snapshot();foreach(var priorRoom in old.bedrooms){priorRoom.undoActor=null;priorRoom.undoKind=null;}
            var w=GameWorld.WithFurnishedRooms(GameWorld.Restore(old));var r=w.ReadBedrooms()[0];r.layout=1;r.undoKind="bad";
            Check(Room(w).layout==0 && Room(w).undoKind=="");GameWorld.Validate(w.Snapshot());
        });
        Test("four furnished rooms operate independently with same-avatar ownership preserved",()=>{
            var w=FurnitureWorld();var ids=w.Snapshot().players.Select(v=>v.id).ToArray();
            for(var i=0;i<4;i++){FurnishVisit(w,i,ids[i]);Good(w,SoloAction.UseFixture,target:BedroomFurniture.Bed,actor:ids[i]);EditRoom(w,"theme",(i+1)%4,ids[i]);}
            Check(w.Snapshot().players.All(p=>p.fixture==BedroomFurniture.Bed));GameWorld.Validate(w.Snapshot());
            Check(!Walking.AdvanceLocal(w,"first",WalkMode.Stop,0,0,.1f) && w.ReadPlayer("first").fixture==BedroomFurniture.Bed);
            var session=new FamilySession(w);session.Attach(1,"first",out _);var movement=new MovementAuthority(w,session);var p=w.ReadPlayer("first");
            movement.Accept(1,new WalkInput{actor=p.id,zone=p.zone,visit=p.visit,sequence=1,mode=WalkMode.Stop},0);
            Check(!movement.Tick(.1,.1f) && w.ReadPlayer("first").fixture==BedroomFurniture.Bed);
        });
        Test("four room cushions reject only conflicting seat while owner leaving preserves guests",()=>{
            var w=FurnitureWorld();var ids=w.Snapshot().players.Select(v=>v.id).ToArray();
            for(var i=0;i<4;i++){FurnishVisit(w,0,ids[i]);Good(w,SoloAction.UseFixture,target:BedroomFurniture.Cushion(i),actor:ids[i]);}
            Check(!w.Apply(Command(w,SoloAction.UseFixture,target:BedroomFurniture.Cushion(1))).Accepted);
            var guests=Encode(w.Snapshot().players.Skip(1).ToArray());Good(w,SoloAction.Travel,value:"park");
            Check(Encode(w.Snapshot().players.Skip(1).ToArray())==guests);GameWorld.Validate(w.Snapshot());
        });
        Test("bedroom storage conserves identity through close reopen and save restoration",()=>{
            var w=FurnitureWorld();FurnishVisit(w);var id=BedroomFurniture.ToyId(0,0);
            Good(w,SoloAction.Grab,id);Check(!w.Apply(Command(w,SoloAction.Grab,id,actor:"second")).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Drop,id,target:BedroomFurniture.Storage(BedroomLayout.Id(0),0),x:1650,y:300)).Accepted);
            Good(w,SoloAction.SetFixture,target:"bedroom-chest",value:"on");ShelfDrop(w,id,0);Good(w,SoloAction.SetFixture,target:"bedroom-chest",value:"off");
            Check(!w.Apply(Command(w,SoloAction.Grab,id)).Accepted);w=GameWorld.Restore(w.Snapshot());
            Good(w,SoloAction.SetFixture,target:"bedroom-chest",value:"on");Good(w,SoloAction.Grab,id);Good(w,SoloAction.Drop,id,x:1000,y:150);
            Check(w.ReadToys().Single(t=>t.id==id).personalRoom==BedroomLayout.Id(0) && w.ReadToys().Length==27);GameWorld.Validate(w.Snapshot());
        });
        Test("two visitors cannot fill one shelf slot or duplicate personal stock",()=>{
            var w=FurnitureWorld();FurnishVisit(w);FurnishVisit(w,0,"second");var a=BedroomFurniture.ToyId(0,0);var b=BedroomFurniture.ToyId(0,1);
            Good(w,SoloAction.Grab,a);Good(w,SoloAction.Grab,b,actor:"second");ShelfDrop(w,a,8);
            Check(!w.Apply(Command(w,SoloAction.Drop,b,target:BedroomFurniture.Storage(BedroomLayout.Id(0),8),x:1990,y:440,actor:"second")).Accepted);
            Check(w.ReadToys().Single(t=>t.id==b).holder=="second");GameWorld.Validate(w.Snapshot());
        });
        Test("owner permissions reject visitors until opted in and stale decoration cannot overwrite",()=>{
            var w=FurnitureWorld();FurnishVisit(w);FurnishVisit(w,0,"second");
            Check(!w.Apply(Command(w,SoloAction.DecorateRoom,target:"theme",value:"1:"+Room(w).roomRevision,actor:"second")).Accepted);
            EditRoom(w,"together",1);var stale="2:"+Room(w).roomRevision;EditRoom(w,"theme",1,"second");
            Check(!w.Apply(Command(w,SoloAction.DecorateRoom,target:"theme",value:stale)).Accepted);
            Check(!w.Apply(Command(w,SoloAction.DecorateRoom,target:"together",value:"0:"+Room(w).roomRevision,actor:"second")).Accepted);
            EditRoom(w,"together",0);Check(Room(w).theme==1);
            Check(!w.Apply(Command(w,SoloAction.DecorateRoom,target:"layout",value:"1:"+Room(w).roomRevision,actor:"second")).Accepted);
        });
        Test("layout and undo move real supports and seated visitors without erasing newer items",()=>{
            var w=FurnitureWorld();FurnishVisit(w);FurnishVisit(w,0,"second");Good(w,SoloAction.UseFixture,target:BedroomFurniture.Bed,actor:"second");
            var id=BedroomFurniture.ToyId(0,0);EditRoom(w,"layout",1);Check(w.ReadPlayer("second").x==1450);
            Good(w,SoloAction.Grab,id);Good(w,SoloAction.Drop,id,x:900,y:150);EditRoom(w,"undo",0);
            Check(Room(w).layout==0 && w.ReadPlayer("second").fixture==BedroomFurniture.Bed && w.ReadPlayer("second").x==1050);
            Check(w.ReadToys().Single(t=>t.id==id).container=="" && w.ReadToys().Single(t=>t.id==id).x==900);GameWorld.Validate(w.Snapshot());
        });
        Test("undo belongs to the latest eligible decorator and does not replace a room snapshot",()=>{
            var w=FurnitureWorld();FurnishVisit(w);FurnishVisit(w,0,"second");EditRoom(w,"together",1);EditRoom(w,"theme",1);EditRoom(w,"theme",2,"second");
            Check(!w.Apply(Command(w,SoloAction.DecorateRoom,target:"undo",value:"0:"+Room(w).roomRevision)).Accepted);
            EditRoom(w,"undo",0,"second");Check(Room(w).theme==1 && Room(w).undoKind=="");
        });
        Test("tidy stores only this owner's loose unheld toys and preserves visitors and other rooms",()=>{
            var w=FurnitureWorld();FurnishVisit(w);FurnishVisit(w,0,"second");var a=BedroomFurniture.ToyId(0,0);var b=BedroomFurniture.ToyId(0,1);
            Good(w,SoloAction.Grab,a);Good(w,SoloAction.Drop,a,x:1100,y:160);Good(w,SoloAction.Grab,b,actor:"second");var other=Encode(w.ReadToys().Where(t=>t.zone==BedroomLayout.Id(1)).ToArray());
            EditRoom(w,"tidy",0);Check(w.ReadToys().Single(t=>t.id==a).container==BedroomFurniture.Storage(BedroomLayout.Id(0),0));
            Check(w.ReadToys().Single(t=>t.id==b).holder=="second" && Encode(w.ReadToys().Where(t=>t.zone==BedroomLayout.Id(1)).ToArray())==other);
            GameWorld.Validate(w.Snapshot());
        });
        Test("personal toys retain identity through rooms and stairs and stay home on major travel",()=>{
            var w=FurnitureWorld();FurnishVisit(w);var id=BedroomFurniture.ToyId(0,0);Good(w,SoloAction.Grab,id);Door(w,HomeRooms.Landing);Door(w,BedroomLayout.Id(1));
            Check(w.ReadToys().Single(t=>t.id==id).zone==BedroomLayout.Id(1));Good(w,SoloAction.Travel,value:"park");
            Check(w.ReadToys().Single(t=>t.id==id).zone==BedroomLayout.Id(1) && w.ReadToys().Single(t=>t.id==id).holder=="");GameWorld.Validate(w.Snapshot());
        });
        Test("authored walk corridor routes to exit without crossing fixed furniture",()=>{
            var w=FurnitureWorld();FurnishVisit(w);Good(w,SoloAction.Move,x:2200,y:150);
            for(var i=0;i<100;i++){Walking.AdvanceLocal(w,"first",WalkMode.Destination,325,420,.1f);var p=w.ReadPlayer("first");Check(p.x<=600 || p.y<=260);}
            var p2=w.ReadPlayer("first");Check(p2.x==325 && p2.y==420);Good(w,SoloAction.EnterDoor,target:HomeRooms.Landing);
        });
        Test("furnished save rejects duplicated supports and malformed personal ownership without repair",()=>{
            var w=FurnitureWorld();var s=w.Snapshot();s.toys.Last().personalRoom=BedroomLayout.Id(0);Throws(()=>GameWorld.Restore(s));
            s=w.Snapshot();s.bedrooms[0].theme=4;Throws(()=>GameWorld.Restore(s));
            s=w.Snapshot();s.bedrooms[0].undoKind="layout";s.bedrooms[0].undoActor="unknown";Throws(()=>GameWorld.Restore(s));
        });
    }
}
