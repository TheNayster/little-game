using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

static partial class Program
{
    static GameWorld BedroomWorld()=>GameWorld.WithBedrooms(GameWorld.Create("first","second","third","fourth"));
    static void Upstairs(GameWorld w,string actor="first")
    {AtStairs(w,actor);Good(w,SoloAction.UseStairs,actor:actor);FinishStairs(w);}
    static void Door(GameWorld w,string target,string actor="first")
    {var p=w.ReadPlayer(actor);Good(w,SoloAction.Move,x:BedroomLayout.DoorX(p.zone,target),y:BedroomLayout.DoorY,actor:actor);Good(w,SoloAction.EnterDoor,target:target,actor:actor);}
    static void BedroomTests()
    {
        Test("bedroom migration adds four owned rooms once without altering the old home",()=>{
            var old=RoomWorld();var before=old.Snapshot();var w=GameWorld.WithBedrooms(old);var after=w.Snapshot();
            Check(after.schema==7 && after.revision==before.revision+1);
            after.schema=before.schema;after.revision=before.revision;after.bedrooms=Array.Empty<BedroomState>();
            Check(Encode(after)==Encode(before));
            Check(w.ReadBedrooms().Length==4 && w.ReadBedrooms().Select(r=>r.owner).SequenceEqual(before.players.Select(p=>p.id)));
            Check(ReferenceEquals(w,GameWorld.WithBedrooms(w)));GameWorld.Validate(w.Snapshot());
        });
        Test("four bedroom ownership follows persisted profiles across avatars and reordered roster",()=>{
            var w=BedroomWorld();var before=JsonSerializer.Serialize(w.ReadBedrooms(),Json);
            foreach(var actor in w.Snapshot().players.Select(p=>p.id))Good(w,SoloAction.ChangeAvatar,value:"orange-pup",actor:actor);
            var s=w.Snapshot();Array.Reverse(s.players);w=GameWorld.WithBedrooms(GameWorld.Restore(s));
            Check(JsonSerializer.Serialize(w.ReadBedrooms(),Json)==before);
            var detached=w.ReadBedrooms();detached[0].owner="wrong";
            Check(JsonSerializer.Serialize(w.ReadBedrooms(),Json)==before);
            var snapshot=w.Snapshot();snapshot.bedrooms[0].owner="wrong";Check(JsonSerializer.Serialize(w.ReadBedrooms(),Json)==before);
        });
        Test("one solo profile gets one owner and three guest rooms without invented profiles",()=>{
            var w=GameWorld.WithBedrooms(GameWorld.Create("only"));
            Check(w.ReadBedrooms().Count(r=>r.owner=="only")==1 && w.ReadBedrooms().Count(r=>r.owner=="")==3);
            Check(w.Snapshot().players.Length==1);GameWorld.Validate(w.Snapshot());
        });
        Test("bedroom doors require their hall entry and cannot be major destinations or stairs",()=>{
            var w=BedroomWorld();Check(!w.Apply(Command(w,SoloAction.EnterDoor,target:BedroomLayout.Id(0))).Accepted);
            Upstairs(w);Check(!w.Apply(Command(w,SoloAction.Travel,value:BedroomLayout.Id(0))).Accepted);
            Door(w,BedroomLayout.Id(0));
            Check(!w.Apply(Command(w,SoloAction.UseStairs)).Accepted);
            Check(!w.Apply(Command(w,SoloAction.EnterDoor,target:BedroomLayout.Id(1))).Accepted);
            Check(!WorldLayout.Position(BedroomLayout.Id(0),6,500,200));
        });
        Test("four players can occupy separate bedrooms then visit one absent owner's room",()=>{
            var w=BedroomWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            foreach(var actor in actors)Upstairs(w,actor);
            for(var i=0;i<4;i++)Door(w,BedroomLayout.Id(i),actors[i]);
            Check(w.Snapshot().players.Select(p=>p.zone).Distinct().Count()==4);
            foreach(var actor in actors){Door(w,HomeRooms.Landing,actor);Door(w,BedroomLayout.Id(0),actor);}
            Check(w.Snapshot().players.All(p=>p.zone==BedroomLayout.Id(0)) && w.Snapshot().players.Select(p=>p.x).Distinct().Count()==4);
            var others=w.Snapshot().players.Skip(1).Select(p=>JsonSerializer.Serialize(p,Json)).ToArray();
            Door(w,HomeRooms.Landing);Good(w,SoloAction.Travel,value:"park");
            Check(others.SequenceEqual(w.Snapshot().players.Skip(1).Select(p=>JsonSerializer.Serialize(p,Json))));
            Check(w.ReadBedrooms()[0].owner=="first");GameWorld.Validate(w.Snapshot());
        });
        Test("door commits carried identity once and rejects delayed hall input",()=>{
            var w=BedroomWorld();Good(w,SoloAction.Grab,"ball-1");Upstairs(w);
            Good(w,SoloAction.Move,x:BedroomLayout.HallX(2),y:BedroomLayout.DoorY);
            var stale=Command(w,SoloAction.Move,x:700,y:200);var door=Command(w,SoloAction.EnterDoor,target:BedroomLayout.Id(2));
            Check(w.Apply(door).Accepted && w.Apply(door).Duplicate);
            Check(!w.Apply(stale).Accepted);
            var ball=w.ReadToys().Single(t=>t.id=="ball-1");Check(ball.zone==BedroomLayout.Id(2) && ball.holder=="first");
            Good(w,SoloAction.Drop,"ball-1",x:850,y:180);Door(w,HomeRooms.Landing);Door(w,BedroomLayout.Id(2));
            Check(w.ReadToys().Single(t=>t.id=="ball-1").x==850 && w.ReadToys().Length==11);
            GameWorld.Validate(w.Snapshot());
        });
        Test("room save reopen and disconnect preserve ownership location and visitor progress",()=>{
            var w=BedroomWorld();Upstairs(w);Door(w,BedroomLayout.Id(3));
            Upstairs(w,"second");Door(w,BedroomLayout.Id(3),"second");
            var session=new FamilySession(w);Check(session.Attach(1,"first",out _) && session.Attach(2,"second",out _));
            var before=Encode(w.Snapshot());Check(session.Detach(1));Check(Encode(w.Snapshot())==before);
            var opened=GameWorld.WithBedrooms(GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(before,Json)));
            Check(Encode(opened.Snapshot())==before);Check(opened.ReadBedrooms()[3].owner=="fourth");
        });
        Test("bedroom validation refuses missing duplicated reassigned and future room data",()=>{
            var w=BedroomWorld();
            void Bad(Action<SoloSnapshot> alter){var s=w.Snapshot();alter(s);Throws(()=>GameWorld.Validate(s));}
            Bad(s=>s.bedrooms=null);Bad(s=>s.bedrooms=s.bedrooms.Take(3).ToArray());
            Bad(s=>s.bedrooms[1].id=s.bedrooms[0].id);Bad(s=>s.bedrooms[1].owner=s.bedrooms[0].owner);
            Bad(s=>s.bedrooms[1].owner="unknown");Bad(s=>s.bedrooms[1].id="home-bedroom-5");
            Bad(s=>s.schema=6);Bad(s=>s.bedrooms[1]=null);
            Bad(s=>{s.players[0].zone=BedroomLayout.Id(0);s.players[0].stairs=.5;});
        });
        Test("content 8 recovery retains four room owners and bedroom-held toy",()=>{
            var basis=RecoveryFixture();var w=GameWorld.WithBedrooms(GameWorld.Restore(basis.snapshot));
            Good(w,SoloAction.Grab,"ball-1");Upstairs(w);Door(w,BedroomLayout.Id(1));
            basis.snapshot=w.Snapshot();basis.content=8;basis.Validate(basis.family,basis.authority,basis.world);
            var bytes=RecoveryBytes(basis);Check(bytes.Length<RecoveryTransfer.MaxBytes);
            var restored=GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Encode(basis.snapshot),Json));
            Check(restored.ReadBedrooms().Select(r=>r.owner).SequenceEqual(w.ReadBedrooms().Select(r=>r.owner)));
            Check(restored.ReadToys().Single(t=>t.id=="ball-1").zone==BedroomLayout.Id(1));
            basis.content=7;Throws(()=>basis.Validate(basis.family,basis.authority,basis.world));
        });
        Test("private room visits persist separately without importing into shared origin",()=>{
            var basis=RecoveryFixture();var w=GameWorld.WithBedrooms(GameWorld.Restore(basis.snapshot));Upstairs(w);Door(w,BedroomLayout.Id(1));
            var before=Encode(w.Snapshot());var library=Library("bedroom-private",basis,"first");
            var local=library.CreateVisible(new LocalViewOrigin{family=basis.family,authority=basis.authority,world=basis.world,epoch=basis.epoch,snapshot=w.Snapshot()});
            var privateWorld=GameWorld.Restore(local.snapshot);Door(privateWorld,HomeRooms.Landing);Door(privateWorld,BedroomLayout.Id(2));
            library.Save(local,privateWorld.Snapshot());Check(library.Load(local.id).snapshot.players[0].zone==BedroomLayout.Id(2));
            Check(Encode(w.Snapshot())==before && local.snapshot.bedrooms.Length==4);
        });
    }
}
