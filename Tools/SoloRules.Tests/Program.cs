using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

static class Program
{
    static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
    static readonly List<object> Results = new List<object>();
    static string root;
    static int failures;
    static int Main(string[] args)
    {
        root = Path.GetFullPath(args.Length == 1 ? args[0] : throw new ArgumentException("Pass a new isolated evidence directory."));
        if (Directory.Exists(root)) throw new IOException("Evidence directory already exists.");
        Directory.CreateDirectory(root);
        Test("one holder; other player cannot steal or release", () => {
            var w = SoloWorld.Create("first", "second");
            Good(w, SoloAction.Grab, "bucket-1");
            var before = Encode(w.Snapshot());
            Check(!w.Apply(Command(w, SoloAction.Grab, "bucket-1", actor:"second")).Accepted);
            Check(!w.Apply(Command(w, SoloAction.CancelGrab, "bucket-1", actor:"second")).Accepted);
            Check(Encode(w.Snapshot()) == before);
        });
        Test("one child holds one prop; unrelated movement and avatar switch survive", () => {
            var w = SoloWorld.Create("first"); Good(w, SoloAction.Grab, "bucket-1");
            Check(!w.Apply(Command(w, SoloAction.Grab, "sponge-1")).Accepted);
            Good(w, SoloAction.Move, x:230, y:210); Good(w, SoloAction.ChangeAvatar, value:"orange-pup");
            var s = w.Snapshot(); Check(s.players[0].x == 230 && s.players[0].avatar == "orange-pup" && Toy(w,"bucket-1").holder == "first");
        });
        Test("fill/pour conserves water and duplicate pour does not apply twice", () => {
            var w = SoloWorld.Create("first"); Fill(w);
            Good(w, SoloAction.Grab, "bucket-1");
            var pour = Command(w, SoloAction.Drop, "bucket-1", "plant-1", x:810,y:330);
            Check(w.Apply(pour).Accepted); var revision = w.Revision;
            var duplicate = w.Apply(pour); Check(duplicate.Accepted && duplicate.Duplicate && w.Revision == revision);
            Check(Toy(w,"bucket-1").water == 0 && Toy(w,"plant-1").water == 3);
            Fill(w); Good(w, SoloAction.Grab, "bucket-1"); Good(w, SoloAction.Drop, "bucket-1", "plant-1", x:810,y:330);
            Check(Toy(w,"bucket-1").water == 3 && Toy(w,"plant-1").water == 3);
        });
        Test("duplicate accepted action remains idempotent after restored checkpoint", () => {
            var w = SoloWorld.Create("first"); var c = Command(w,SoloAction.Move,x:100,y:100);
            Check(w.Apply(c).Accepted); var restored = SoloWorld.Restore(Decode(Encode(w.Snapshot())));
            Check(restored.Apply(c).Duplicate && restored.Revision == w.Revision);
            c.x = 300; Check(!restored.Apply(c).Accepted);
        });
        Test("stale and invalid commands leave world unchanged", () => {
            var w = SoloWorld.Create("first"); var stale = Command(w,SoloAction.Move,x:1,y:1); Good(w,SoloAction.Move,x:20,y:20);
            var before=Encode(w.Snapshot()); Check(!w.Apply(stale).Accepted);
            foreach(var bad in new[]{float.NaN,float.PositiveInfinity,-1,1001}) Check(!w.Apply(Command(w,SoloAction.Move,x:bad,y:10)).Accepted);
            Check(!w.Apply(Command(w,SoloAction.ChangeAvatar,value:"missing")).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Move,actor:"stranger")).Accepted);
            Check(Encode(w.Snapshot())==before);
        });
        Test("remote target drop rejects then cancellation restores original location", () => {
            var w=SoloWorld.Create("first"); var initial=Toy(w,"bucket-1"); Good(w,SoloAction.Grab,"bucket-1");
            Check(!w.Apply(Command(w,SoloAction.Drop,"bucket-1","tap-1",x:500,y:300)).Accepted);
            Good(w,SoloAction.CancelGrab,"bucket-1"); var item=Toy(w,"bucket-1");
            Check(item.x==initial.x && item.y==initial.y && item.holder=="" && item.water==0);
        });
        Test("cleanup works without a quest and activities can be freely switched/left", () => {
            var w=SoloWorld.Create("first");
            for(var i=0;i<4;i++){Good(w,SoloAction.Grab,"sponge-1");Good(w,SoloAction.Drop,"sponge-1","puddle-1",x:680,y:140);}
            Check(Toy(w,"puddle-1").water==0 && Toy(w,"sponge-1").wet);
            Good(w,SoloAction.StartActivity,value:"garden"); Good(w,SoloAction.StartActivity,value:"cleanup"); Good(w,SoloAction.LeaveActivity);
            Check(w.Snapshot().players[0].activity=="" && Toy(w,"puddle-1").water==0);
        });
        Test("restore releases transient grabs and snapshot mutation cannot change authority", () => {
            var w=SoloWorld.Create("first"); Good(w,SoloAction.Grab,"bucket-1"); var snapshot=w.Snapshot();
            var restored=SoloWorld.Restore(snapshot); Check(Toy(restored,"bucket-1").holder=="");
            snapshot.toys[0].water=100; Check(Toy(w,"bucket-1").water==0);
        });
        Test("invalid snapshots are rejected", () => {
            foreach(var mutate in new Action<SoloSnapshot>[] {s=>s.schema=2,s=>s.players[0].x=float.NaN,s=>s.toys[0].water=4,
                s=>s.toys[1].id=s.toys[0].id,s=>s.toys[0].holder="missing",s=>s.receipts=null,s=>s.players[0].avatar=null})
            {var s=SoloWorld.Create("first").Snapshot();mutate(s);Throws(()=>SoloWorld.Restore(s));}
        });
        Test("checkpoint replacement retains previous valid version", () => {
            var store=Store("replace"); var w=SoloWorld.Create("first"); store.Save(Encode(w.Snapshot()));
            Good(w,SoloAction.Move,x:300,y:200);store.Save(Encode(w.Snapshot()));
            Check(Decode(store.Load().Payload).players[0].x==300);
            var old=new CheckpointStore(Path.Combine(root,"replace","world.save.bak"),Validate);
            Check(Decode(old.Load().Payload).players[0].x==420);
        });
        Test("truncated main recovers backup; next save preserves damaged evidence", () => {
            var store=Store("corrupt");var w=SoloWorld.Create("first");store.Save(Encode(w.Snapshot()));
            Good(w,SoloAction.Move,x:222,y:200);store.Save(Encode(w.Snapshot()));
            var path=Path.Combine(root,"corrupt","world.save");File.WriteAllText(path,"broken");
            var restored=store.Load();Check(restored.Status==CheckpointStatus.Recovered && Decode(restored.Payload).players[0].x==420);
            store.Save(restored.Payload);Check(store.Load().Status==CheckpointStatus.Loaded);
            Check(Directory.GetFiles(Path.GetDirectoryName(path),"*.damaged-*").Length==1);
        });
        Test("interrupted staging never displaces the committed checkpoint", () => {
            var store=Store("staging");var w=SoloWorld.Create("first");store.Save(Encode(w.Snapshot()));
            File.WriteAllText(Path.Combine(root,"staging","world.save.pending"),"partial");
            Check(store.Load().Status==CheckpointStatus.Loaded);
        });
        Test("first-write staged recovery stays recoverable during next save", () => {
            var store=Store("first-write");var payload=Encode(SoloWorld.Create("first").Snapshot());store.Save(payload);
            var path=Path.Combine(root,"first-write","world.save");File.Move(path,path+".pending");
            Check(store.Load().Status==CheckpointStatus.Recovered);store.Save(payload);
            Check(store.Load().Status==CheckpointStatus.Loaded && File.Exists(path+".bak"));
        });
        Test("unknown envelope and payload versions never fall back or overwrite", () => {
            foreach(var payloadVersion in new[]{false,true}){
                var name=payloadVersion?"future-payload":"future-header";var store=Store(name);var w=SoloWorld.Create("first");
                var payload=Encode(w.Snapshot());store.Save(payload);store.Save(payload);
                var path=Path.Combine(root,name,"world.save");
                if(payloadVersion){var s=w.Snapshot();s.schema=99;File.WriteAllText(path,Envelope(Encode(s)));}
                else File.WriteAllText(path,File.ReadAllText(path).Replace("LITTLEWEEPS-SOLO-1","LITTLEWEEPS-SOLO-99"));
                var bytes=File.ReadAllBytes(path);Check(store.Load().Status==CheckpointStatus.Unsupported);Throws(()=>store.Save(payload));
                Check(bytes.SequenceEqual(File.ReadAllBytes(path)));
            }
        });
        Test("checksum mismatch and wholly corrupt saves are never silently reset", () => {
            var store=Store("bad-checksum");var payload=Encode(SoloWorld.Create("first").Snapshot());store.Save(payload);
            var path=Path.Combine(root,"bad-checksum","world.save");File.AppendAllText(path,"tamper");
            Check(store.Load().Status==CheckpointStatus.Corrupt);Throws(()=>store.Save(payload));
        });
        Test("random legal and rejected actions retain invariants and bounded receipts", () => {
            var w=SoloWorld.Create("first","second"); var rng=new Random(4701);
            for(var i=0;i<3000;i++){
                var action=(SoloAction)rng.Next(7);var c=Command(w,action,rng.Next(2)==0?"bucket-1":"sponge-1",x:rng.Next(1001),y:rng.Next(501),actor:rng.Next(2)==0?"first":"second",
                    value:action==SoloAction.ChangeAvatar?"orange-pup":"garden");
                w.Apply(c);SoloWorld.Validate(w.Snapshot());
            }
            Check(w.Snapshot().receipts.Length<=128);
        });
        File.WriteAllText(Path.Combine(root,"results.json"),JsonSerializer.Serialize(new{utc=DateTime.UtcNow,checks=Results},Json));
        return failures == 0 ? 0 : 1;
    }
    static void Test(string name,Action body){try{body();Results.Add(new{name,passed=true,error=""});Console.WriteLine("PASS "+name);}catch(Exception e){failures++;Results.Add(new{name,passed=false,error=e.ToString()});Console.WriteLine("FAIL "+name+": "+e.Message);}}
    static void Check(bool condition){if(!condition)throw new Exception("Assertion failed.");}
    static void Throws(Action body){try{body();}catch{return;}throw new Exception("Expected rejection.");}
    static SoloCommand Command(SoloWorld w,SoloAction action,string item="",string target="",string value="",float x=0,float y=0,string actor="first")=>new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,expectedRevision=w.Revision,action=action,item=item,target=target,value=value,x=x,y=y};
    static void Good(SoloWorld w,SoloAction action,string item="",string target="",string value="",float x=0,float y=0){var r=w.Apply(Command(w,action,item,target,value,x,y));Check(r.Accepted);}
    static SoloToy Toy(SoloWorld w,string id)=>w.Snapshot().toys.Single(t=>t.id==id);
    static void Fill(SoloWorld w){Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","tap-1",x:150,y:340);}
    static string Encode(SoloSnapshot s)=>JsonSerializer.Serialize(s,Json);
    static SoloSnapshot Decode(string s)=>JsonSerializer.Deserialize<SoloSnapshot>(s,Json);
    static bool Validate(string payload){try{var s=Decode(payload);if(s!=null&&s.schema>1)throw new NotSupportedException();SoloWorld.Validate(s);return true;}catch(JsonException){return false;}catch(InvalidOperationException){return false;}}
    static CheckpointStore Store(string name)=>new CheckpointStore(Path.Combine(root,name,"world.save"),Validate);
    static string Envelope(string payload)=>"LITTLEWEEPS-SOLO-1\n"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant()+"\n"+payload;
}
