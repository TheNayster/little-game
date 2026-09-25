using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

static partial class Program
{
    static T DecodeAdventure<T>(string text)
    {try{return JsonSerializer.Deserialize<T>(text,Json);}catch(JsonException e){throw new InvalidDataException("Invalid JSON",e);}}
    static ContinuationLibrary Library(string name,RecoveryRecord basis,string actor="second")=>new ContinuationLibrary(Path.Combine(root,name),basis.family,basis.authority,basis.world,actor,
        DecodeAdventure<ContinuationRecord>,value=>JsonSerializer.Serialize(value,Json),DecodeAdventure<ContinuationSelection>,value=>JsonSerializer.Serialize(value,Json));
    static void ContinuationTests()
    {
        Test("outage clock counts ten observed foreground seconds and resets on reunion",()=>{
            var clock=new OutageClock();Check(!clock.Tick(0,true,true));
            for(var i=1;i<40;i++)Check(!clock.Tick(i*.25,true,true));
            Check(clock.Tick(10,true,true));Check(!clock.Tick(11,true,false));
            clock.Tick(12,false,true);Check(!clock.Tick(1000,true,true));
            for(var i=1;i<40;i++)Check(!clock.Tick(1000+i*.25,true,true));
            Check(clock.Tick(1010,true,true));Throws(()=>clock.Tick(double.NaN,true,true));
        });
        Test("outage branch keeps immutable base receipts clocks four players and both areas while releasing stale holds",()=>{
            var basis=RecoveryFixture();var original=RecoveryBytes(basis);var library=Library("adventure-base",basis);var record=library.Create(basis);
            Check(record.snapshot.worldId==record.id && record.snapshot.worldId!=basis.snapshot.worldId && record.snapshot.schema==2);
            Check(record.snapshot.players.Length==4 && record.snapshot.toys.Length==10 && record.snapshot.receipts.Length==128);
            Check(record.snapshot.toys.All(t=>t.holder=="") && record.snapshot.players.Single(p=>p.id=="second").zone=="creek");
            Check(RecoveryBytes(record.basis).SequenceEqual(original) && RecoveryBytes(basis).SequenceEqual(original));
            Check(record.snapshot.idleTimers.Any(t=>t.item=="bucket-1" && t.seconds==37));
            var world=SoloWorld.Restore(record.snapshot);Good(world,SoloAction.Grab,"sponge-creek",actor:"second");Good(world,SoloAction.Drop,"sponge-creek",x:800,y:160,actor:"second");
            Good(world,SoloAction.Travel,value:"garden",actor:"second");library.Save(record,world.Snapshot());
            Check(RecoveryBytes(library.Load(record.id).basis).SequenceEqual(original));
        });
        Test("two outages retain distinct adventures and selection survives cold restart without touching solo or replica",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-selection",basis);
            var sentinel=Path.Combine(root,"ordinary-solo.save");File.WriteAllText(sentinel,"original solo");
            var one=library.Create(basis);library.Select(one.id);var second=Library("adventure-selection",basis);Check(second.Selected==one.id);
            var bytes=File.ReadAllBytes(library.PathFor(one.id));library.Select("");Check(Library("adventure-selection",basis).Selected=="");
            var two=library.Create(basis);library.Select(two.id);Check(two.id!=one.id && library.Branches().Length==2);
            Check(bytes.SequenceEqual(File.ReadAllBytes(library.PathFor(one.id))) && File.ReadAllText(sentinel)=="original solo");
            library.Select(one.id);Check(Library("adventure-selection",basis).Selected==one.id);
        });
        Test("adventure rejects foreign identity altered base revision rollback and path escape without replacing committed work",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-reject",basis);var record=library.Create(basis);var path=library.PathFor(record.id);var before=File.ReadAllBytes(path);
            var altered=DecodeAdventure<ContinuationRecord>(JsonSerializer.Serialize(record,Json));altered.basis.snapshot.players[0].x++;
            Throws(()=>library.Save(altered,altered.snapshot));Check(before.SequenceEqual(File.ReadAllBytes(path)));
            var low=SoloWorld.Restore(record.snapshot).Snapshot();low.revision=0;Throws(()=>library.Save(record,low));
            Throws(()=>library.Select("../escape"));Throws(()=>Library("adventure-reject",basis,"outsider").Load(record.id));
            var foreign=CopyRecovery(basis);foreign.family=Guid.NewGuid().ToString("N");Throws(()=>library.Create(foreign));
            Check(before.SequenceEqual(File.ReadAllBytes(path)));
        });
        Test("failed adventure save or selection keeps prior progress and can recover from an interrupted pending write",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-disk",basis);var record=library.Create(basis);library.Select(record.id);
            var path=library.PathFor(record.id);var before=File.ReadAllBytes(path);var world=SoloWorld.Restore(record.snapshot);Good(world,SoloAction.Move,x:900,y:100,actor:"second");
            using(var locked=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None))Throws(()=>library.Save(record,world.Snapshot()));
            Check(before.SequenceEqual(File.ReadAllBytes(path)));File.WriteAllText(path+".pending","incomplete");Check(library.Load(record.id).snapshot.revision==record.snapshot.revision);
            var selection=Path.Combine(root,"adventure-disk","selection.save");
            using(var locked=new FileStream(selection,FileMode.Open,FileAccess.Read,FileShare.None))Throws(()=>library.Select(""));
            Check(Library("adventure-disk",basis).Selected==record.id);
        });
        Test("future adventure format blocks rather than replacing it with a backup",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-future",basis);var record=library.Create(basis);library.Select(record.id);var path=library.PathFor(record.id);
            library.Save(record,record.snapshot);record.version=2;File.WriteAllText(path,Envelope(JsonSerializer.Serialize(record,Json)));
            Throws(()=>library.Load(record.id));Throws(()=>Library("adventure-future",basis));Check(File.ReadAllText(path).Contains("\"version\": 2"));
        });
        Test("selection recovers damaged JSON from backup but a future selection version blocks",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-pointer",basis);var record=library.Create(basis);library.Select(record.id);library.Select("");
            var path=Path.Combine(root,"adventure-pointer","selection.save");File.WriteAllText(path,Envelope("{invalid"));
            Check(Library("adventure-pointer",basis).Selected==record.id);
            File.WriteAllText(path,Envelope("{\"version\":2,\"branch\":\"\"}"));Throws(()=>Library("adventure-pointer",basis));
        });
        Test("unsettled command export preserves request IDs independently and disconnect never replays",()=>{
            var queue=new GardenCommandQueue();var world=SoloWorld.Create("first");var c=Command(world,SoloAction.Grab,"bucket-1");var calls=0;
            queue.Enqueue(c,r=>{Check(!r.Accepted);calls++;});queue.Take(world.Revision);var copy=queue.Unsettled();
            Check(copy.Length==1 && copy[0].requestId==c.requestId);copy[0].item="changed";Check(queue.Unsettled()[0].item=="bucket-1");
            queue.Disconnect();Check(calls==1 && !queue.Busy && queue.Take(world.Revision)==null && copy[0].requestId==c.requestId);
        });
    }
}
