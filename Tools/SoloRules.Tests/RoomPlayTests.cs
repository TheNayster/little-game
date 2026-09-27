using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld PlayWorld()=>SoloWorld.WithRoomPlay(SoloWorld.WithBooks(SecretWorld()));
    static void PlayDrop(SoloWorld w,string id,int slot,string actor="first")
    {Good(w,SoloAction.Drop,id,target:RoomPlay.Support(w.ReadPlayer(actor).zone,slot),x:RoomPlay.X(slot),y:RoomPlay.Y(slot),actor:actor);}
    static void RoomPlayTests()
    {
        Test("room play migration preserves every old item and adds exactly five tea pieces per furnished room once",()=>{
            var old=SoloWorld.WithBooks(SecretWorld());MakeSecret(old);var before=old.Snapshot();var w=SoloWorld.WithRoomPlay(old);
            Check(w.Schema==12 && w.ReadToys().Length==before.toys.Length+25);
            Check(Encode(w.ReadToys().Where(t=>!RoomPlay.Tea(t.kind)).ToArray())==Encode(before.toys));
            Check(ReferenceEquals(w,SoloWorld.WithRoomPlay(w)));SoloWorld.Validate(w.Snapshot());
            var snap=w.Snapshot();snap.schema=11;Throws(()=>SoloWorld.Validate(snap));
        });
        Test("four secret rooms add bounded play stock through creation without duplicate resets",()=>{
            var w=PlayWorld();var actors=w.Snapshot().players;
            for(var i=0;i<4;i++){MakeSecret(w,i,actors[i].id);EnterSecret(w,i,actors[i].id);}
            Check(w.ReadToys().Length==97);var before=Encode(w.ReadToys());SecretEdit(w,"create");Check(Encode(w.ReadToys())==before);
            SoloWorld.Validate(w.Snapshot());
        });
        Test("four independent cuddles retain one plush holder and departure only settles that player's toy",()=>{
            var w=PlayWorld();MakeSecret(w);EnterSecret(w);var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++){
                if(i>0){FurnishVisit(w,0,actors[i]);EnterSecret(w,0,actors[i]);}
                Good(w,SoloAction.RoomObject,SecretRooms.ToyId(0,i),actor:actors[i]);
            }
            Check(w.Snapshot().players.All(p=>BedroomFurniture.Seat(p.fixture)));
            Check(w.ReadToys().Count(t=>t.holder!="")==4);SoloWorld.Validate(w.Snapshot());
            var peers=Encode(w.Snapshot().players.Skip(1).ToArray());Good(w,SoloAction.Move,x:750,y:180);
            Check(Toy(w,SecretRooms.ToyId(0,0)).holder=="" && Encode(w.Snapshot().players.Skip(1).ToArray())==peers);
            Check(!w.Apply(Command(w,SoloAction.RoomObject,SecretRooms.ToyId(0,1))).Accepted);
            var restored=SoloWorld.Restore(w.Snapshot());Check(restored.ReadToys().All(t=>t.holder==""));SoloWorld.Validate(restored.Snapshot());
        });
        Test("cuddled plush can be picked up again and carried through a door without duplicate holders",()=>{
            var w=PlayWorld();FurnishVisit(w);var id=BedroomFurniture.ToyId(0,0);
            Good(w,SoloAction.RoomObject,id);Good(w,SoloAction.Grab,id);Check(w.ReadPlayer("first").fixture=="" && Toy(w,id).holder=="first");
            Door(w,HomeRooms.Landing);Check(Toy(w,id).zone==HomeRooms.Landing && Toy(w,id).holder=="first");SoloWorld.Validate(w.Snapshot());
        });
        Test("four plush holders can move into fort seats then disconnect independently",()=>{
            var w=PlayWorld();MakeSecret(w);EnterSecret(w);var players=w.Snapshot().players;
            for(var i=0;i<4;i++){
                var actor=players[i].id;if(i>0){FurnishVisit(w,0,actor);EnterSecret(w,0,actor);}
                Good(w,SoloAction.RoomObject,SecretRooms.ToyId(0,i),actor:actor);
                Good(w,SoloAction.UseFixture,target:SecretRooms.Fort(i),actor:actor);
            }
            SoloWorld.Validate(w.Snapshot());var others=Encode(w.ReadToys().Where(t=>t.holder!="first").ToArray());
            var session=new FamilySession(w);session.Attach(1,"first",out _);session.Detach(1);
            Check(Toy(w,SecretRooms.ToyId(0,0)).holder=="" && w.Snapshot().players.Skip(1).All(p=>SecretRooms.FortIndex(p.fixture)>=0));
            Check(w.ReadToys().Count(t=>t.holder!="")==3);SoloWorld.Validate(w.Snapshot());
        });
        Test("maximum room stock fits bounded family views and full receipt recovery",()=>{
            var w=PlayWorld();var players=w.Snapshot().players;
            for(var i=0;i<4;i++){MakeSecret(w,i,players[i].id);EnterSecret(w,i,players[i].id);}
            for(var i=0;i<128;i++)Good(w,SoloAction.Move,x:800+i,y:180);
            var session=new FamilySession(w);var view=System.Text.Json.JsonSerializer.Serialize(session.View(),new System.Text.Json.JsonSerializerOptions{IncludeFields=true});
            // Leave headroom for the envelope, four poses, long real profile IDs
            // and integer counters. Receipt history travels in recovery chunks.
            Check(System.Text.Encoding.UTF8.GetByteCount(view)<28000);
            var record=RecoveryFixture();record.snapshot=w.Snapshot();record.content=13;
            record.Validate(record.family,record.authority,record.world);Check(RecoveryBytes(record).Length<RecoveryTransfer.MaxBytes);
        });
        Test("tucked toys persist through hidden entrances and cold reopen with occupied support races rejected",()=>{
            var w=PlayWorld();MakeSecret(w);EnterSecret(w);var id=SecretRooms.ToyId(0,0);var other=SecretRooms.ToyId(0,1);
            Good(w,SoloAction.Grab,id);PlayDrop(w,id,0);Good(w,SoloAction.Grab,other);
            var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Drop,other,target:RoomPlay.Support(SecretRooms.Id(0),0),x:700,y:265)).Accepted);Check(Encode(w.Snapshot())==before);
            PlayDrop(w,other,1);SecretEdit(w,"active",0);var stock=Encode(w.ReadToys());Good(w,SoloAction.ReturnBedroom);
            w=SoloWorld.Restore(w.Snapshot());Check(Encode(w.ReadToys())==stock);SoloWorld.Validate(w.Snapshot());
        });
        Test("bounded stacks persist protect creations from tidy and settle dependents atomically",()=>{
            var w=PlayWorld();FurnishVisit(w);var ids=Enumerable.Range(0,4).Select(i=>BedroomFurniture.ToyId(0,i)).ToArray();
            Good(w,SoloAction.Grab,ids[2]);Good(w,SoloAction.Drop,ids[2],x:950,y:180);
            Good(w,SoloAction.Grab,ids[3]);Good(w,SoloAction.Drop,ids[3],target:RoomPlay.Stack(ids[2]),x:950,y:180);
            Good(w,SoloAction.Grab,ids[0]);Good(w,SoloAction.Drop,ids[0],target:RoomPlay.Stack(ids[3]),x:950,y:180);
            Good(w,SoloAction.Grab,ids[1]);Check(!w.Apply(Command(w,SoloAction.Drop,ids[1],target:RoomPlay.Stack(ids[0]),x:950,y:180)).Accepted);
            Good(w,SoloAction.CancelGrab,ids[1]);EditRoom(w,"tidy",0);Check(Toy(w,ids[2]).container=="");
            w=SoloWorld.Restore(w.Snapshot());Check(Toy(w,ids[0]).container==RoomPlay.Stack(ids[3]));SoloWorld.Validate(w.Snapshot());
            Good(w,SoloAction.Grab,ids[2]);Check(ids.Where(id=>id!=ids[1]).All(id=>Toy(w,id).container==""));Check(w.ReadToys().Count(t=>t.holder!="")==1);SoloWorld.Validate(w.Snapshot());
        });
        Test("stack validation rejects cycles held parents cross-room supports and duplicate children",()=>{
            var w=PlayWorld();FurnishVisit(w);var a=BedroomFurniture.ToyId(0,0);var b=BedroomFurniture.ToyId(0,1);
            Good(w,SoloAction.Grab,a);Good(w,SoloAction.Drop,a,x:900,y:180);Good(w,SoloAction.Grab,b);Good(w,SoloAction.Drop,b,target:RoomPlay.Stack(a),x:900,y:180);
            void Bad(Action<SoloSnapshot> alter){var s=w.Snapshot();alter(s);Throws(()=>SoloWorld.Validate(s));}
            Bad(s=>s.toys.Single(t=>t.id==a).container=RoomPlay.Stack(b));
            Bad(s=>s.toys.Single(t=>t.id==a).holder="first");Bad(s=>s.toys.Single(t=>t.id==b).zone=BedroomLayout.Id(1));
            Bad(s=>s.toys.Single(t=>t.id==b).container="stack/missing");
        });
        Test("four tea cups have independent drink state and pouring cannot race a held cup or duplicate liquid",()=>{
            var w=PlayWorld();var players=w.Snapshot().players;for(var i=0;i<4;i++)FurnishVisit(w,0,players[i].id);
            var room=BedroomLayout.Id(0);var pot=RoomPlay.TeaId(room,4);var cup=RoomPlay.TeaId(room,0);
            Good(w,SoloAction.Grab,cup,actor:"second");Good(w,SoloAction.Grab,pot);
            Check(!w.Apply(Command(w,SoloAction.Drop,pot,target:cup,x:RoomPlay.X(4),y:RoomPlay.Y(4))).Accepted);Check(Toy(w,pot).water==3);
            PlayDrop(w,cup,4,"second");
            for(var i=0;i<4;i++){
                if(i>0)Good(w,SoloAction.Grab,pot);if(i==3)Good(w,SoloAction.RoomObject,pot);
                Good(w,SoloAction.Drop,pot,target:RoomPlay.TeaId(room,i),x:RoomPlay.X(4+i),y:RoomPlay.Y(4+i));
                Good(w,SoloAction.RoomObject,RoomPlay.TeaId(room,i),actor:players[i].id);
            }
            Check(w.ReadToys().Where(t=>t.kind==ToyKind.TeaCup).All(t=>t.water==0));SoloWorld.Validate(w.Snapshot());
        });
        Test("bedding rug picture and lamp choices enforce owner Together permissions with field-only undo",()=>{
            var w=PlayWorld();FurnishVisit(w);FurnishVisit(w,0,"second");
            Check(!w.Apply(Command(w,SoloAction.DecorateRoom,target:"bedding",value:"2:1",actor:"second")).Accepted);
            foreach(var field in new[]{"bedding","rug","picture","lamp"})EditRoom(w,field,2);
            EditRoom(w,"undo",0);Check(Room(w).lamp==0 && Room(w).bedding==2 && Room(w).picture==2);
            EditRoom(w,"together",1);EditRoom(w,"bedding",3,"second");EditRoom(w,"undo",0,"second");Check(Room(w).bedding==2);
            w=SoloWorld.Restore(w.Snapshot());SoloWorld.Validate(w.Snapshot());
        });
    }
}
