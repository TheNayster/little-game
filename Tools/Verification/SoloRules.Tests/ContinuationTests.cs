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
        Test("visible local fork preserves latest area pose item progress without a recovery checkpoint or source mutation",()=>{
            var basis=RecoveryFixture();var library=Library("visible-origin",basis);
            var latest=GameWorld.Restore(basis.snapshot);
            Good(latest,SoloAction.Travel,value:"garden",actor:"second");
            Good(latest,SoloAction.Travel,value:"creek",actor:"second");
            Good(latest,SoloAction.Move,x:805,y:160,actor:"second");
            Good(latest,SoloAction.Grab,"bucket-creek",actor:"second");
            var visible=latest.Snapshot();visible.players.Single(p=>p.id=="second").x=819;
            var toy=visible.toys.Single(t=>t.id=="bucket-creek");toy.x=710;toy.y=230;toy.water=2;
            var origin=new LocalViewOrigin{family=basis.family,authority=basis.authority,world=basis.world,epoch=basis.epoch,snapshot=visible};
            var before=JsonSerializer.Serialize(origin,Json);var record=library.CreateVisible(origin);
            Check(record.version==2 && record.basis==null && record.origin.snapshot.receipts.Length==visible.receipts.Length);
            var player=record.snapshot.players.Single(p=>p.id=="second");var local=record.snapshot.toys.Single(t=>t.id=="bucket-creek");
            Check(player.zone=="creek" && player.x==819 && player.y==160 && local.x==710 && local.y==230 && local.water==2 && local.holder=="");
            Check(record.snapshot.toys.All(t=>t.holder=="") && JsonSerializer.Serialize(origin,Json)==before);
            visible.players[0].x++;Check(JsonSerializer.Serialize(record.origin,Json)==before);
            library.Select(record.id);Check(Library("visible-origin",basis).Load(record.id).snapshot.players.Single(p=>p.id=="second").x==819);
        });
        Test("visible origins reject wrong family invalid view future versions and changed provenance without replacing saves",()=>{
            var basis=RecoveryFixture();var library=Library("visible-invalid",basis);
            var origin=new LocalViewOrigin{family=basis.family,authority=basis.authority,world=basis.world,epoch=basis.epoch,snapshot=basis.snapshot};
            var record=library.CreateVisible(origin);var path=library.PathFor(record.id);var bytes=File.ReadAllBytes(path);
            var altered=DecodeAdventure<ContinuationRecord>(JsonSerializer.Serialize(record,Json));altered.origin.snapshot.players[0].x++;
            Throws(()=>library.Save(altered,altered.snapshot));Check(bytes.SequenceEqual(File.ReadAllBytes(path)));
            origin.family=Guid.NewGuid().ToString("N");Throws(()=>library.CreateVisible(origin));origin.family=basis.family;
            origin.snapshot=GameWorld.CopySnapshot(basis.snapshot);origin.snapshot.players[0].x=float.NaN;Throws(()=>library.CreateVisible(origin));
            record.version=3;File.WriteAllText(path,Envelope(JsonSerializer.Serialize(record,Json)));Throws(()=>library.Load(record.id));
        });
        Test("legacy checkpoint adventures and newer visible adventures coexist without upgrading older saves",()=>{
            var basis=RecoveryFixture();var library=Library("visible-legacy",basis);var old=library.Create(basis);var bytes=File.ReadAllBytes(library.PathFor(old.id));
            var newer=library.CreateVisible(new LocalViewOrigin{family=basis.family,authority=basis.authority,world=basis.world,epoch=basis.epoch,snapshot=basis.snapshot});
            library.Select(newer.id);library.Select(old.id);Check(library.Load(old.id).version==1 && library.Load(newer.id).version==2);
            Check(bytes.SequenceEqual(File.ReadAllBytes(library.PathFor(old.id))) && library.Branches().Length==2);
        });
        Test("inline serializer empty optional objects are selected by version without accepting a forged recovery record",()=>{
            var basis=RecoveryFixture();var library=Library("visible-inline",basis);var old=library.Create(basis);
            old.origin=new LocalViewOrigin();library.Save(old,old.snapshot);
            var newer=library.CreateVisible(new LocalViewOrigin{family=basis.family,authority=basis.authority,world=basis.world,epoch=basis.epoch,snapshot=basis.snapshot});
            // Model Unity's inline null expansion before the first persisted write.
            var copy=DecodeAdventure<ContinuationRecord>(JsonSerializer.Serialize(newer,Json));copy.id=Guid.NewGuid().ToString("N");
            copy.snapshot.worldId=copy.id;copy.basis=new RecoveryRecord();library.Save(copy,copy.snapshot);Check(library.Load(copy.id).version==2);
            copy.basis=basis;Throws(()=>library.Save(copy,copy.snapshot));
        });
        Test("known disconnection permits local play immediately on foreground or resume",()=>{
            var clock=new OutageClock();Check(clock.Tick(0,true,true));
            Check(!clock.Tick(.01,true,false));Check(!clock.Tick(.02,false,true));
            Check(clock.Tick(1000,true,true));Check(!clock.Tick(1000.01,true,false));
            Check(clock.Tick(1000.02,true,true));Throws(()=>clock.Tick(double.NaN,true,true));
        });
        Test("outage branch keeps immutable base receipts clocks four players and both areas while releasing stale holds",()=>{
            var basis=RecoveryFixture();var original=RecoveryBytes(basis);var library=Library("adventure-base",basis);var record=library.Create(basis);
            Check(record.snapshot.worldId==record.id && record.snapshot.worldId!=basis.snapshot.worldId && record.snapshot.schema==2);
            Check(record.snapshot.players.Length==4 && record.snapshot.toys.Length==10 && record.snapshot.receipts.Length==128);
            Check(record.snapshot.toys.All(t=>t.holder=="") && record.snapshot.players.Single(p=>p.id=="second").zone=="creek");
            Check(RecoveryBytes(record.basis).SequenceEqual(original) && RecoveryBytes(basis).SequenceEqual(original));
            Check(record.snapshot.idleTimers.Any(t=>t.item=="bucket-1" && t.seconds==37));
            var world=GameWorld.Restore(record.snapshot);Good(world,SoloAction.Grab,"sponge-creek",actor:"second");Good(world,SoloAction.Drop,"sponge-creek",x:800,y:160,actor:"second");
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
            var low=GameWorld.Restore(record.snapshot).Snapshot();low.revision=0;Throws(()=>library.Save(record,low));
            Throws(()=>library.Select("../escape"));Throws(()=>Library("adventure-reject",basis,"outsider").Load(record.id));
            var foreign=CopyRecovery(basis);foreign.family=Guid.NewGuid().ToString("N");Throws(()=>library.Create(foreign));
            Check(before.SequenceEqual(File.ReadAllBytes(path)));
        });
        Test("failed adventure save or selection keeps prior progress and can recover from an interrupted pending write",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-disk",basis);var record=library.Create(basis);library.Select(record.id);
            var path=library.PathFor(record.id);var before=File.ReadAllBytes(path);var world=GameWorld.Restore(record.snapshot);Good(world,SoloAction.Move,x:900,y:100,actor:"second");
            using(var locked=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None))Throws(()=>library.Save(record,world.Snapshot()));
            Check(before.SequenceEqual(File.ReadAllBytes(path)));File.WriteAllText(path+".pending","incomplete");Check(library.Load(record.id).snapshot.revision==record.snapshot.revision);
            var selection=Path.Combine(root,"adventure-disk","selection.save");
            using(var locked=new FileStream(selection,FileMode.Open,FileAccess.Read,FileShare.None))Throws(()=>library.Select(""));
            Check(Library("adventure-disk",basis).Selected==record.id);
        });
        Test("future adventure format blocks rather than replacing it with a backup",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-future",basis);var record=library.Create(basis);library.Select(record.id);var path=library.PathFor(record.id);
            library.Save(record,record.snapshot);record.version=3;File.WriteAllText(path,Envelope(JsonSerializer.Serialize(record,Json)));
            Throws(()=>library.Load(record.id));Throws(()=>Library("adventure-future",basis));Check(File.ReadAllText(path).Contains("\"version\": 3"));
        });
        Test("selection recovers damaged JSON from backup but a future selection version blocks",()=>{
            var basis=RecoveryFixture();var library=Library("adventure-pointer",basis);var record=library.Create(basis);library.Select(record.id);library.Select("");
            var path=Path.Combine(root,"adventure-pointer","selection.save");File.WriteAllText(path,Envelope("{invalid"));
            Check(Library("adventure-pointer",basis).Selected==record.id);
            File.WriteAllText(path,Envelope("{\"version\":2,\"branch\":\"\"}"));Throws(()=>Library("adventure-pointer",basis));
        });
        Test("unsettled command export preserves request IDs independently and disconnect never replays",()=>{
            var queue=new WorldCommandQueue();var world=GameWorld.Create("first");var c=Command(world,SoloAction.Grab,"bucket-1");var calls=0;
            queue.Enqueue(c,r=>{Check(!r.Accepted);calls++;});queue.Take(world.Revision);var copy=queue.Unsettled();
            Check(copy.Length==1 && copy[0].requestId==c.requestId);copy[0].item="changed";Check(queue.Unsettled()[0].item=="bucket-1");
            queue.Disconnect();Check(calls==1 && !queue.Busy && queue.Take(world.Revision)==null && copy[0].requestId==c.requestId);
        });
    }
}
