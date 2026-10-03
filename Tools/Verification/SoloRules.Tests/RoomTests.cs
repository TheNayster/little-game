using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

static partial class Program
{
    static GameWorld RoomWorld()=>GameWorld.WithUpstairs(GameWorld.Create("first","second","third","fourth"));
    static void AtStairs(GameWorld w,string actor="first")
    {var p=w.ReadPlayer(actor);Good(w,SoloAction.Move,x:HomeRooms.EntryX(p.zone),y:HomeRooms.EntryY(p.zone),actor:actor);}
    static void FinishStairs(GameWorld w)
    {for(var i=0;i<30;i++)w.AdvanceIdle(.1,out _);}
    static void RoomTests()
    {
        Test("upstairs migration is additive and repeatable with all four profiles",()=>{
            var old=GameWorld.WithKeepyUppy(GameWorld.Create("first","second","third","fourth"));
            Good(old,SoloAction.SetFixture,target:"radio-living",value:"on");
            var before=old.Snapshot();var w=GameWorld.WithUpstairs(old);var after=w.Snapshot();
            Check(after.schema==6 && after.revision==before.revision+1 && after.worldId==before.worldId);
            Check(JsonSerializer.Serialize(before.players,Json)==JsonSerializer.Serialize(after.players,Json));
            Check(JsonSerializer.Serialize(before.toys,Json)==JsonSerializer.Serialize(after.toys,Json));
            Check(JsonSerializer.Serialize(before.home,Json)==JsonSerializer.Serialize(after.home,Json));
            Check(JsonSerializer.Serialize(before.keepy,Json)==JsonSerializer.Serialize(after.keepy,Json));
            Check(JsonSerializer.Serialize(before.receipts,Json)==JsonSerializer.Serialize(after.receipts,Json));
            Check(ReferenceEquals(w,GameWorld.WithUpstairs(w)));
        });
        Test("internal stairs require proximity and cannot be reached through world travel",()=>{
            var w=RoomWorld();var before=Encode(w.Snapshot());
            Check(!w.Apply(Command(w,SoloAction.UseStairs)).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Travel,value:HomeRooms.Landing)).Accepted);
            Check(before==Encode(w.Snapshot()));
            var old=GameWorld.WithKeepyUppy(GameWorld.Create("first"));AtStairs(old);
            Check(!old.Apply(Command(old,SoloAction.UseStairs)).Accepted);
            Check(!WorldLayout.Position(HomeRooms.Landing,5,500,200));
        });
        Test("four people traverse stairs independently without moving a downstairs sibling",()=>{
            var w=RoomWorld();AtStairs(w);var sibling=JsonSerializer.Serialize(w.ReadPlayer("second"),Json);
            Good(w,SoloAction.UseStairs);FinishStairs(w);
            Check(w.ReadPlayer("first").zone==HomeRooms.Landing && sibling==JsonSerializer.Serialize(w.ReadPlayer("second"),Json));
            foreach(var id in new[]{"first","second","third","fourth"}){AtStairs(w,id);Good(w,SoloAction.UseStairs,actor:id);}
            FinishStairs(w);
            Check(w.ReadPlayer("first").zone=="garden");
            Check(w.Snapshot().players.Count(p=>p.zone==HomeRooms.Landing)==3);
            Check(w.Snapshot().players.Where(p=>p.zone==HomeRooms.Landing).Select(p=>p.x).Distinct().Count()==3);
            GameWorld.Validate(w.Snapshot());
        });
        Test("stair transfer commits carried items once and rejects late origin commands",()=>{
            var w=RoomWorld();AtStairs(w);Good(w,SoloAction.Grab,"bucket-1");
            var stale=Command(w,SoloAction.Move,x:100,y:100);var start=Command(w,SoloAction.UseStairs);
            Check(w.Apply(start).Accepted);Check(w.Apply(start).Duplicate);
            Check(!w.Apply(stale).Accepted);Check(!Walking.AdvanceLocal(w,"first",WalkMode.Direction,1,0,.1f));
            FinishStairs(w);var p=w.ReadPlayer("first");var toy=w.ReadToys().Single(t=>t.id=="bucket-1");
            Check(p.zone==HomeRooms.Landing && toy.zone==p.zone && toy.holder==p.id && toy.x==p.x);
            Check(w.Apply(start).Duplicate && w.ReadPlayer("first").zone==HomeRooms.Landing);
            Good(w,SoloAction.Drop,"bucket-1",x:700,y:200);
            var reopened=GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Encode(w.Snapshot()),Json));
            Check(reopened.ReadToys().Single(t=>t.id==toy.id).zone==HomeRooms.Landing);
            Check(reopened.ReadToys().Length==11);GameWorld.Validate(reopened.Snapshot());
        });
        Test("stair checkpoint restore and detach settle only that actor at source endpoint",()=>{
            var w=RoomWorld();AtStairs(w);AtStairs(w,"second");
            var session=new FamilySession(w);Check(session.Attach(1,"first",out _) && session.Attach(2,"second",out _));
            Good(w,SoloAction.Grab,"ball-1");Good(w,SoloAction.UseStairs);Good(w,SoloAction.UseStairs,actor:"second");
            w.AdvanceIdle(.7,out _);var snapshot=w.Snapshot();var reopened=GameWorld.Restore(snapshot);
            Check(reopened.ReadPlayer("first").stairs==0 && reopened.ReadPlayer("first").zone=="garden");
            Check(reopened.ReadToys().Single(t=>t.id=="ball-1").holder=="");
            Check(session.Detach(1));Check(w.ReadPlayer("first").stairs==0 && w.ReadPlayer("second").stairs>0);
            FinishStairs(w);Check(w.ReadPlayer("first").zone=="garden" && w.ReadPlayer("second").zone==HomeRooms.Landing);
            GameWorld.Validate(w.Snapshot());
        });
        Test("cancel before commit keeps source while cancel after commit cannot rewind",()=>{
            var w=RoomWorld();AtStairs(w);Good(w,SoloAction.UseStairs);w.AdvanceIdle(.8,out _);
            Good(w,SoloAction.CancelStairs);FinishStairs(w);Check(w.ReadPlayer("first").zone=="garden");
            Good(w,SoloAction.UseStairs);FinishStairs(w);var before=w.ReadPlayer("first");
            Good(w,SoloAction.CancelStairs);Check(w.ReadPlayer("first").zone==before.zone && w.ReadPlayer("first").visit==before.visit);
        });
        Test("upstairs props stay distinct and major travel returns only held borrowed stock",()=>{
            var w=RoomWorld();AtStairs(w);Good(w,SoloAction.Grab,"ball-1");Good(w,SoloAction.UseStairs);FinishStairs(w);
            Good(w,SoloAction.Drop,"ball-1",x:800,y:160);Good(w,SoloAction.Travel,value:"creek");
            Check(w.ReadToys().Single(t=>t.id=="ball-1").zone==HomeRooms.Landing);
            Good(w,SoloAction.Travel,value:"home");AtStairs(w);Good(w,SoloAction.Grab,"sponge-1");Good(w,SoloAction.UseStairs);FinishStairs(w);
            Good(w,SoloAction.Travel,value:"park");var sponge=w.ReadToys().Single(t=>t.id=="sponge-1");
            Check(sponge.zone=="garden" && sponge.holder=="" && sponge.x==560);GameWorld.Validate(w.Snapshot());
        });
        Test("room validators reject fabricated transit and cross-room ownership",()=>{
            var w=RoomWorld();
            foreach(var bad in new[]{double.NaN,double.PositiveInfinity,-1,HomeRooms.StairDuration})
            {var s=w.Snapshot();s.players[0].stairs=bad;var rejected=false;try{GameWorld.Validate(s);}catch(InvalidOperationException){rejected=true;}Check(rejected);}
            var moved=w.Snapshot();moved.toys.First(t=>t.kind==ToyKind.Tap).zone=HomeRooms.Landing;
            var failed=false;try{GameWorld.Validate(moved);}catch(InvalidOperationException){failed=true;}Check(failed);
            AtStairs(w);Good(w,SoloAction.UseStairs);FinishStairs(w);
            Check(WorldLayout.Place(w.ReadPlayer("first"))=="home");
            Check(!w.Apply(Command(w,SoloAction.Grab,"bucket-1")).Accepted);
            var clone=w.Snapshot();clone.players[0].x=100;Check(w.ReadPlayer("first").x!=100);
        });
        Test("content 7 recovery retains upstairs items and source-endpoint transit",()=>{
            var basis=RecoveryFixture();var w=GameWorld.WithUpstairs(GameWorld.Restore(basis.snapshot));
            AtStairs(w);Good(w,SoloAction.Grab,"ball-1");Good(w,SoloAction.UseStairs);FinishStairs(w);
            Good(w,SoloAction.Drop,"ball-1",x:900,y:250);AtStairs(w);Good(w,SoloAction.UseStairs);w.AdvanceIdle(.5,out _);
            basis.snapshot=w.Snapshot();basis.content=7;basis.Validate(basis.family,basis.authority,basis.world);
            var bytes=RecoveryBytes(basis);var replica=Replica(Path.Combine(root,"upstairs-recovery","world.save"),basis);
            var retained=replica.Commit(bytes,basis.epoch,basis.snapshot.worldId,basis.snapshot.players.Select(p=>p.id).ToArray());
            Check(RecoveryBytes(retained).SequenceEqual(bytes));
            var restored=GameWorld.Restore(retained.snapshot);
            Check(restored.ReadPlayer("first").zone==HomeRooms.Landing && restored.ReadPlayer("first").stairs==0);
            Check(restored.ReadToys().Single(t=>t.id=="ball-1").zone==HomeRooms.Landing);
            basis.content=6;Throws(()=>basis.Validate(basis.family,basis.authority,basis.world));
        });
        Test("private upstairs continuation preserves local items without mutating shared origin",()=>{
            var basis=RecoveryFixture();var w=GameWorld.WithUpstairs(GameWorld.Restore(basis.snapshot));
            AtStairs(w);Good(w,SoloAction.Grab,"ball-1");Good(w,SoloAction.UseStairs);FinishStairs(w);
            var source=w.Snapshot();var before=Encode(source);var library=Library("upstairs-private",basis,"first");
            var local=library.CreateVisible(new LocalViewOrigin{family=basis.family,authority=basis.authority,world=basis.world,epoch=basis.epoch,snapshot=source});
            Check(local.snapshot.players.First(p=>p.id=="first").zone==HomeRooms.Landing);
            var privateWorld=GameWorld.Restore(local.snapshot);Good(privateWorld,SoloAction.Grab,"ball-1");
            Good(privateWorld,SoloAction.Drop,"ball-1",x:1200,y:150);library.Save(local,privateWorld.Snapshot());
            Check(library.Load(local.id).snapshot.toys.Single(t=>t.id=="ball-1").x==1200);
            Check(Encode(source)==before && Encode(w.Snapshot())==before);
        });
    }
}
