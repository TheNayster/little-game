using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

static partial class Program
{
    static RecoveryRecord RecoveryFixture()
    {
        var w=SoloWorld.WithAreas(SoloWorld.Create("first","second","third","fourth"));
        for(var i=0;i<128;i++)Good(w,SoloAction.Move,x:300+i,y:120);
        Fill(w);Advance(w,37);Good(w,SoloAction.Travel,value:"creek",actor:"second");Good(w,SoloAction.Grab,"sponge-creek",actor:"second");
        return new RecoveryRecord{version=1,protocol=3,content=3,family=Guid.NewGuid().ToString("N"),authority=Guid.NewGuid().ToString("N"),
            world=Guid.NewGuid().ToString("N"),epoch=Guid.NewGuid().ToString("N"),checkpoint=1,snapshot=w.Snapshot()};
    }
    static byte[] RecoveryBytes(RecoveryRecord record)=>Encoding.UTF8.GetBytes(JsonSerializer.Serialize(record,Json));
    static RecoveryRecord RecoveryDecode(string text)
    {try{return JsonSerializer.Deserialize<RecoveryRecord>(text,Json);}catch(JsonException e){throw new InvalidDataException("Invalid JSON",e);}}
    static RecoveryRecord CopyRecovery(RecoveryRecord record)=>RecoveryDecode(Encoding.UTF8.GetString(RecoveryBytes(record)));
    static RecoveryReplica Replica(string path,RecoveryRecord r)=>new RecoveryReplica(path,r.family,r.authority,r.world,RecoveryDecode);
    static RecoveryRecord CommitRecovery(RecoveryReplica replica,RecoveryRecord record)=>replica.Commit(RecoveryBytes(record),record.epoch,record.snapshot.worldId,record.snapshot.players.Select(p=>p.id).ToArray());
    static void RecoveryTests()
    {
        Test("recovery chunks retain four players both areas 128 receipts idle clocks and a held prop exactly",()=>{
            var r=RecoveryFixture();var bytes=RecoveryBytes(r);Check(bytes.Length>16384);
            var t=new RecoveryTransfer();var id=Guid.NewGuid().ToString("N");byte[] result=null;
            for(var i=0;i<RecoveryTransfer.Count(bytes.Length);i++){
                result=t.Add(RecoveryTransfer.Chunk(bytes,id,r.epoch,i),r.epoch,i*.1);
                Check((result!=null)==(i==RecoveryTransfer.Count(bytes.Length)-1));
            }
            Check(result.SequenceEqual(bytes));var path=Path.Combine(root,"replica-complete","world.save");
            var replica=Replica(path,r);var saved=replica.Commit(result,r.epoch,r.snapshot.worldId,r.snapshot.players.Select(p=>p.id).ToArray());
            Check(RecoveryBytes(saved).SequenceEqual(bytes));Check(saved.snapshot.receipts.Length==128 && saved.snapshot.idleTimers.Length>0 && saved.snapshot.toys.Any(t=>t.holder=="second"));
            Check(RecoveryBytes(Replica(path,r).Last).SequenceEqual(bytes));
        });
        Test("partial duplicate reordered mixed truncated and corrupt transfers never become complete saves",()=>{
            var r=RecoveryFixture();var bytes=RecoveryBytes(r);var id=Guid.NewGuid().ToString("N");var t=new RecoveryTransfer();
            var a=RecoveryTransfer.Chunk(bytes,id,r.epoch,0);Check(t.Add(a,r.epoch,0)==null && t.Add(a,r.epoch,.1)==null && t.Next==1);
            Throws(()=>t.Add(RecoveryTransfer.Chunk(bytes,id,r.epoch,2),r.epoch,.2));
            var mixed=RecoveryTransfer.Chunk(bytes,Guid.NewGuid().ToString("N"),r.epoch,1);Throws(()=>t.Add(mixed,r.epoch,.3));
            a.data=Convert.ToBase64String(new byte[RecoveryTransfer.ChunkBytes]);Throws(()=>t.Add(a,r.epoch,.4));
            var shortChunk=RecoveryTransfer.Chunk(bytes,id,r.epoch,1);shortChunk.data="AA==";Throws(()=>t.Add(shortChunk,r.epoch,.5));
            t.Reset();var corrupt=(byte[])bytes.Clone();corrupt[^1]^=1;
            for(var i=0;i<RecoveryTransfer.Count(bytes.Length);i++){
                var chunk=RecoveryTransfer.Chunk(corrupt,id,r.epoch,i);chunk.hash=RecoveryTransfer.Hash(bytes);
                if(i+1==RecoveryTransfer.Count(bytes.Length))Throws(()=>t.Add(chunk,r.epoch,i*.1));else Check(t.Add(chunk,r.epoch,i*.1)==null);
            }
            Check(!t.Complete);
        });
        Test("recovery size bounds timeout and epoch changes discard incomplete data",()=>{
            var r=RecoveryFixture();var bytes=RecoveryBytes(r);var id=Guid.NewGuid().ToString("N");var t=new RecoveryTransfer();
            var a=RecoveryTransfer.Chunk(bytes,id,r.epoch,0);a.total=RecoveryTransfer.MaxBytes+1;Throws(()=>t.Add(a,r.epoch,0));
            a=RecoveryTransfer.Chunk(bytes,id,r.epoch,0);Throws(()=>t.Add(a,Guid.NewGuid().ToString("N"),0));
            t.Add(a,r.epoch,0);Throws(()=>t.Add(RecoveryTransfer.Chunk(bytes,id,r.epoch,1),r.epoch,41));Check(t.Next==0);
            var max=new byte[RecoveryTransfer.MaxBytes];new Random(431).NextBytes(max);t.Reset();byte[] result=null;
            for(var i=0;i<RecoveryTransfer.Count(max.Length);i++)result=t.Add(RecoveryTransfer.Chunk(max,id,r.epoch,i),r.epoch,i*.1);
            Check(result.SequenceEqual(max));Throws(()=>RecoveryTransfer.Chunk(new byte[RecoveryTransfer.MaxBytes+1],id,r.epoch,0));
        });
        Test("stale serial revision lineage family authority roster and future formats preserve a good replica",()=>{
            var r=RecoveryFixture();r.checkpoint=2;var path=Path.Combine(root,"replica-reject","world.save");var replica=Replica(path,r);CommitRecovery(replica,r);var before=File.ReadAllBytes(path);
            Action<Action<RecoveryRecord>> reject=change=>{var bad=CopyRecovery(r);bad.checkpoint++;change(bad);Throws(()=>CommitRecovery(replica,bad));Check(before.SequenceEqual(File.ReadAllBytes(path)));};
            reject(v=>v.checkpoint=1);reject(v=>v.snapshot.revision--);reject(v=>v.snapshot.worldId=Guid.NewGuid().ToString("N"));
            reject(v=>v.family=Guid.NewGuid().ToString("N"));reject(v=>v.authority=Guid.NewGuid().ToString("N"));reject(v=>v.world=Guid.NewGuid().ToString("N"));
            reject(v=>v.version=2);reject(v=>v.protocol=2);reject(v=>v.snapshot.idleTimers=null);reject(v=>v.snapshot.toys[0].water=9);
            var roster=r.snapshot.players.Select(p=>p.id).ToArray();roster[0]="outsider";
            Throws(()=>replica.Commit(RecoveryBytes(r),r.epoch,r.snapshot.worldId,roster));
            var good=CopyRecovery(r);good.checkpoint++;Throws(()=>replica.Commit(RecoveryBytes(good),r.epoch,r.snapshot.worldId,roster));
            Throws(()=>replica.Commit(RecoveryBytes(good),Guid.NewGuid().ToString("N"),r.snapshot.worldId,r.snapshot.players.Select(p=>p.id).ToArray()));
            Check(before.SequenceEqual(File.ReadAllBytes(path)));
        });
        Test("equal-revision movement timers ordered by serial and exact reconnect replay is idempotent",()=>{
            var r=RecoveryFixture();var path=Path.Combine(root,"replica-order","world.save");var replica=Replica(path,r);CommitRecovery(replica,r);
            var next=CopyRecovery(r);next.checkpoint++;next.snapshot.players[0].x+=1;next.snapshot.idleTimers[0].seconds+=1;CommitRecovery(replica,next);
            var bytes=File.ReadAllBytes(path);replica=Replica(path,r);CommitRecovery(replica,next);Check(bytes.SequenceEqual(File.ReadAllBytes(path)));
            var conflict=CopyRecovery(next);conflict.snapshot.players[0].x+=1;Throws(()=>CommitRecovery(replica,conflict));Check(bytes.SequenceEqual(File.ReadAllBytes(path)));
            next.epoch=Guid.NewGuid().ToString("N");next.checkpoint=1;CommitRecovery(replica,next);Check(replica.Last.epoch==next.epoch);
        });
        Test("failed client write is not acknowledged and does not overwrite separate solo progress",()=>{
            var r=RecoveryFixture();var folder=Path.Combine(root,"replica-write");var path=Path.Combine(folder,"world.save");var replica=Replica(path,r);CommitRecovery(replica,r);
            var draft=Path.Combine(folder,"solo.save");File.WriteAllText(draft,"separate child's draft");var before=File.ReadAllBytes(path);
            r.checkpoint++;using(var locked=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None))Throws(()=>CommitRecovery(replica,r));
            Check(before.SequenceEqual(File.ReadAllBytes(path)) && File.ReadAllText(draft)=="separate child's draft" && replica.Last.checkpoint==1);
            File.WriteAllText(path+".pending","interrupted staged bytes");Check(Replica(path,r).Last.checkpoint==1);
            CommitRecovery(replica,r);File.WriteAllText(path,"damaged primary");Check(Replica(path,r).Last.checkpoint==1);
            File.WriteAllText(path+".bak","also damaged");Throws(()=>Replica(path,r));Check(File.ReadAllText(path)=="damaged primary");
        });
        Test("newer nested recovery schema protocol or content cannot roll back to an older backup",()=>{
            foreach(var kind in new[]{"schema","protocol","content"}){
                var r=RecoveryFixture();var path=Path.Combine(root,"replica-future-"+kind,"world.save");var replica=Replica(path,r);
                CommitRecovery(replica,r);r.checkpoint++;CommitRecovery(replica,r);
                if(kind=="schema")r.snapshot.schema=WorldLayout.Schema+1;else if(kind=="protocol")r.protocol=4;else r.content=WorldLayout.Content+1;
                var future=Envelope(Encoding.UTF8.GetString(RecoveryBytes(r)));File.WriteAllText(path,future);
                Throws(()=>Replica(path,r));Check(File.ReadAllText(path)==future);
            }
        });
    }
}
