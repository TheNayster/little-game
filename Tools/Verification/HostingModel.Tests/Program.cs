using System.Text.Json;
using LittleWeeps.HostingModel;

var results = new List<object>(); var failures = 0;
var root = Path.GetFullPath(args.Length == 1 ? args[0] : throw new ArgumentException("Pass a NEW isolated evidence directory"));
if (Directory.Exists(root)) throw new IOException("Refusing to replace evidence");
Directory.CreateDirectory(root);
void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
void Test(string name, Action action)
{
    try { action(); results.Add(new { name, passed = true }); Console.WriteLine("PASS " + name); }
    catch (Exception e) { failures++; results.Add(new { name, passed = false, error = e.Message }); Console.WriteLine("FAIL " + name + ": " + e.Message); }
}
void Fails(Action action) { try { action(); } catch (IOException) { return; } catch (InvalidOperationException) { return; } throw new Exception("Expected rejection"); }
Checkpoint Final() { const string p = "two areas; four actors; ten props; receipts; idle clocks; final accepted revision 42";
    return new("family", "world", "old-epoch", 42, true, p, Checkpoint.Hash(p)); }
(HandoffNode a, HandoffNode b) Pair()
{
    var a = new HandoffNode(new(new("ipad-a", "family", "world", "old-epoch", Phase.Serving)));
    var b = new HandoffNode(new(new("ipad-b", "family", "world", "old-epoch", Phase.Replica)));
    a.RestoreWorld(true); return (a,b);
}
Transfer Prepare(HandoffNode a) => a.Prepare("ipad-b", Final(), "transfer-1", "next-epoch");

Test("planned handoff keeps one writer; exact checkpoint retained; restore required", () => {
    var (a,b) = Pair(); var t = Prepare(a); Check(!a.CanWrite && !b.CanWrite,"Preparation must quiesce writes");
    Check(b.Stage(t,"ipad-a"),"Stage"); var grant = a.FenceAndGrant(b.Ready()!,"ipad-b")!;
    Check(a.State.Phase == Phase.Retired && !a.CanWrite,"Source not durably retired");
    Check(b.AcceptGrant(grant,"ipad-a") && !b.CanWrite,"Must restore before serving"); b.RestoreWorld(true);
    Check(b.CanWrite && b.State.Transfer!.State == Final() && !a.CanWrite,"Wrong successor state");
});
Test("late duplicate ready and grant are idempotent; source cannot cancel after grant", () => {
    var (a,b)=Pair();var t=Prepare(a);b.Stage(t,"ipad-a");var g=a.FenceAndGrant(t,"ipad-b")!;
    Check(a.FenceAndGrant(t,"ipad-b")==null && !a.CancelBeforeGrant(),"Retired source reopened");
    b.AcceptGrant(g,"ipad-a");b.RestoreWorld(true);var bytes=b.Disk.Bytes;
    Check(b.AcceptGrant(g,"ipad-a") && b.Disk.Bytes==bytes && b.CanWrite,"Duplicate changed durable state");
});
Test("cancel before grant permits old writer; a late ready cannot resurrect transfer", () => {
    var(a,b)=Pair();var t=Prepare(a);b.Stage(t,"ipad-a");Check(a.CancelBeforeGrant() && a.CanWrite,"Cancel");
    Check(a.FenceAndGrant(t,"ipad-b")==null && a.RetryGrant()==null && !b.CanWrite,"Late ready granted after cancellation");
});
Test("lost ready or grant can be retried after process restart without restoring old writer", () => {
    var(a,b)=Pair();var t=Prepare(a);b.Stage(t,"ipad-a");b=b.Restart();
    var g=a.FenceAndGrant(b.Ready()!,"ipad-b");a=a.Restart();a.RestoreWorld(true);
    Check(!a.CanWrite && a.RetryGrant()==g,"Retirement lost on restart");
    b.AcceptGrant(a.RetryGrant()!,"ipad-a");b=b.Restart();b.RestoreWorld(true);Check(b.CanWrite,"Successor cannot resume");
});
Test("unknown, foreign, damaged, old-epoch and private-view offers cannot stage", () => {
    var(a,b)=Pair();var t=Prepare(a);var before=b.Disk.Bytes;
    var invalid=new[]{t with {Target="someone-else"},t with {NextEpoch="old-epoch"},t with {Id=""},
        t with {State=t.State with {Family="foreign"}},t with {State=t.State with {World="other"}},
        t with {State=t.State with {Epoch="stale"}},t with {State=t.State with {CompleteAuthorityState=false}},
        t with {State=t.State with {Payload="changed"}},t with {State=t.State with {Revision=-1}}};
    foreach(var bad in invalid)Check(!b.Stage(bad,"ipad-a"),"Invalid offer accepted");
    Check(!b.Stage(t,"untrusted-peer") && b.Disk.Bytes==before,"Unauthenticated channel accepted");
});
Test("grant must bind exact target, sender, checkpoint and staged generation", () => {
    var(a,b)=Pair();var t=Prepare(a);b.Stage(t,"ipad-a");
    foreach(var bad in new[]{t with {Id="other-transfer"},t with {NextEpoch="later"},t with {Target="other"},
        t with {State=t.State with {Revision=43}}})Check(!b.AcceptGrant(bad,"ipad-a"),"Mismatched grant accepted");
    Check(!b.AcceptGrant(t,"someone-else") && !b.CanWrite,"Forged grant");
});
Test("grant before ready/staging is refused", () => {
    var(a,b)=Pair();var t=Prepare(a);Check(!b.AcceptGrant(t,"ipad-a") && !b.CanWrite,"Unstaged grant accepted");
});
Test("background app is not eligible; failed world restore cannot advertise a writer", () => {
    var(a,b)=Pair();var t=Prepare(a);b.Foreground=false;Check(!b.Stage(t,"ipad-a"),"Background stage");
    b.Foreground=true;b.Stage(t,"ipad-a");a.FenceAndGrant(t,"ipad-b");b.AcceptGrant(t,"ipad-a");
    b.RestoreWorld(false);Check(!b.CanWrite,"Failed restoration is writable");b.RestoreWorld(true);
    b.Foreground=false;Check(!b.CanWrite,"Background host still writing");
});
foreach(var step in new[]{"prepare","stage","fence","accept"}) foreach(var fault in new[]{WriteFault.BeforeCommit,WriteFault.AfterCommit})
Test($"{step}: {fault} does not allow two writers and can recover from durable state", () => {
    var(a,b)=Pair();Transfer? t=null;
    if(step=="prepare") {a.Disk.NextFault=fault;Fails(()=>Prepare(a));a=a.Restart();a.RestoreWorld(true);Check(!b.CanWrite,"Replica became writer");}
    else {
        t=Prepare(a);
        if(step=="stage"){b.Disk.NextFault=fault;Fails(()=>b.Stage(t,"ipad-a"));b=b.Restart();}
        else {b.Stage(t,"ipad-a");if(step=="fence"){a.Disk.NextFault=fault;Fails(()=>a.FenceAndGrant(t,"ipad-b"));a=a.Restart();}
        else {a.FenceAndGrant(t,"ipad-b");b.Disk.NextFault=fault;Fails(()=>b.AcceptGrant(t,"ipad-a"));b=b.Restart();}}
        // Finish only the existing transfer; never mint a new epoch on timeout.
        if(b.State.Phase==Phase.Replica)b.Stage(t,"ipad-a");
        if(a.State.Phase==Phase.Frozen)a.FenceAndGrant(b.Ready()!,"ipad-b");
        if(b.State.Phase==Phase.Staged)b.AcceptGrant(a.RetryGrant()!,"ipad-a");
        a.RestoreWorld(true);b.RestoreWorld(true);Check(b.CanWrite && !a.CanWrite,"Durable retry failed");
    }
    Check(!(a.CanWrite&&b.CanWrite),"Two writers after fault");
});
Test("hard loss has no fabricated grant; private offline play is a separate mechanism", () => {
    var(a,b)=Pair();a.Foreground=false;
    Check(b.Ready()==null && b.RetryGrant()==null && !b.CanWrite,"Timeout invented authority");
    b.RestoreWorld(true);Check(!b.CanWrite,"Replica promoted without a protocol");
});
foreach(var fault in new[]{WriteFault.BeforeCommit,WriteFault.AfterCommit})
Test($"cancel: {fault} cannot issue a late grant or permit two writers", () => {
    var(a,b)=Pair();var t=Prepare(a);b.Stage(t,"ipad-a");a.Disk.NextFault=fault;Fails(()=>a.CancelBeforeGrant());
    Check(!a.CanWrite && !b.CanWrite,"Ambiguous cancel did not stop writes");a=a.Restart();a.RestoreWorld(true);
    if(a.State.Phase==Phase.Frozen){a.FenceAndGrant(b.Ready()!,"ipad-b");b.AcceptGrant(a.RetryGrant()!,"ipad-a");b.RestoreWorld(true);}
    else Check(a.FenceAndGrant(t,"ipad-b")==null,"Committed cancel emitted a late grant");
    Check(a.CanWrite!=b.CanWrite,"Cancel recovery did not leave exactly one writer");
});

// Exercise duplicate, reordered, dropped and retried protocol messages together
// with crashes. ModelDisk serialization survives restart; no wall-clock election.
long schedules=0, transitions=0;
Test("exhaustive six-event schedules preserve at most one planned-handoff writer", () => {
    const int choices=7, depth=6;var total=(int)Math.Pow(choices,depth);
    for(var schedule=0;schedule<total;schedule++) {
        var(a,b)=Pair();var t=Prepare(a);Transfer? grant=null;var n=schedule;
        for(var k=0;k<depth;k++,n/=choices) {
            switch(n%choices) {
                case 0:b.Stage(t,"ipad-a");break;
                case 1:if(b.Ready() is {} r)grant=a.FenceAndGrant(r,"ipad-b")??grant;break;
                case 2:if(grant is not null)b.AcceptGrant(grant,"ipad-a");break;
                case 3:a=a.Restart();a.RestoreWorld(true);break;
                case 4:b=b.Restart();b.RestoreWorld(true);break;
                case 5:grant=a.RetryGrant()??grant;break;
                case 6:a.CancelBeforeGrant();break;
            }
            b.RestoreWorld(true);Check(!(a.CanWrite&&b.CanWrite),$"Two writers in schedule {schedule}, event {k}");transitions++;
        }
        schedules++;
    }
});
var result=new {passed=failures==0,utc=DateTimeOffset.UtcNow,checks=results,schedules,transitions,
    scope="Isolated executable planned-handoff model with simulated atomic journals/authenticated sender identities. Not sockets, cryptographic authentication, real disk fault injection, hard-loss election, iPad hosting or mobile qualification."};
File.WriteAllText(Path.Combine(root,"result.json"),JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"{results.Count} groups; {schedules} schedules; {transitions} transitions; failures={failures}");
return failures==0?0:1;
