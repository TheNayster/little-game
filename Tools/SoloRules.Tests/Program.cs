using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

static partial class Program
{
    static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
    static readonly List<object> Results = new List<object>();
    static string root;
    static int failures;
    static int Main(string[] args)
    {
        var nativeBonjour=args.Length==2 && args[1]=="--bonjour";
        root = Path.GetFullPath(args.Length == 1 || nativeBonjour ? args[0] : throw new ArgumentException("Pass a new isolated evidence directory and optional --bonjour."));
        if (Directory.Exists(root)) throw new IOException("Evidence directory already exists.");
        Directory.CreateDirectory(root);
        RecoveryTests();
        ContinuationTests();
        Test("local walking advances on irregular render frames and preserves transaction receipts",()=>{
            foreach(var twoAreas in new[]{false,true})
            {
                var w=SoloWorld.Create("first","second");if(twoAreas)w=SoloWorld.WithAreas(w);
                Good(w,SoloAction.Grab,"bucket-1");var before=w.Snapshot();var start=w.ReadPlayer("first");
                float elapsed=0;
                for(var i=0;i<60;i++)
                {var dt=i%2==0?.016f:.017f;elapsed+=dt;Check(Walking.AdvanceLocal(w,"first",WalkMode.Direction,1,0,dt));}
                var p=w.ReadPlayer("first");Check(Math.Abs(p.x-start.x-210*elapsed)<.01f && p.y==start.y);
                Check(w.Revision==before.revision && JsonSerializer.Serialize(w.Snapshot().receipts,Json)==JsonSerializer.Serialize(before.receipts,Json));
                Check(JsonSerializer.Serialize(w.ReadToys(),Json)==JsonSerializer.Serialize(before.toys,Json));
                var stopped=p.x;Check(!Walking.AdvanceLocal(w,"first",WalkMode.Stop,0,0,.05f) && w.ReadPlayer("first").x==stopped);
                Check(SoloWorld.Restore(w.Snapshot()).ReadPlayer("first").x==stopped);
            }
        });
        Test("local destination walking stops exactly and rejects invalid deltas without mutation",()=>{
            var w=SoloWorld.Create("first");var p=w.ReadPlayer("first");var target=p.x+1;
            Check(Walking.AdvanceLocal(w,"first",WalkMode.Destination,target,p.y,.033f));
            Check(w.ReadPlayer("first").x==target && !Walking.AdvanceLocal(w,"first",WalkMode.Destination,target,p.y,.033f));
            var before=JsonSerializer.Serialize(w.Snapshot(),Json);
            foreach(var dt in new[]{-.01f,float.NaN,float.PositiveInfinity,.2f})Throws(()=>Walking.AdvanceLocal(w,"first",WalkMode.Direction,1,0,dt));
            Throws(()=>Walking.AdvanceLocal(w,"first",WalkMode.Direction,float.NaN,0,.02f));
            Throws(()=>Walking.AdvanceLocal(w,"first",(WalkMode)99,0,0,.02f));
            Check(JsonSerializer.Serialize(w.Snapshot(),Json)==before);
        });
        Test("home authority has no lifetime cutoff while isolated probes retain exact deadlines",()=>{
            foreach(var elapsed in new[]{0d,240d,241d,7200d,7201d,86400d,2592000d})
                Check(!SessionLifetime.Expired(elapsed,true,true));
            Check(!SessionLifetime.Expired(7200,true,false) && SessionLifetime.Expired(7200.001,true,false));
            Check(!SessionLifetime.Expired(240,false,false) && SessionLifetime.Expired(240.001,false,false));
            Throws(()=>SessionLifetime.Expired(double.NaN,true,true));
            Throws(()=>SessionLifetime.Expired(-1,true,true));
        });
        Test("idle tools return the same instance after grace and cue without changing players or receipts",()=>{
            var w=SoloWorld.Create("first","second");Fill(w);
            var original=w.Snapshot();Advance(w,179);Check(!Toy(w,"bucket-1").resetPending && Toy(w,"bucket-1").water==3);
            Advance(w,1);Check(Toy(w,"bucket-1").resetPending);Advance(w,4);Check(Toy(w,"bucket-1").water==3);
            Advance(w,1);var t=Toy(w,"bucket-1");Check(t.x==360 && t.y==130 && t.water==0 && !t.resetPending && t.holder=="");
            Check(w.Snapshot().toys.Select(t=>t.id).SequenceEqual(original.toys.Select(t=>t.id)));
            Check(JsonSerializer.Serialize(original.players,Json)==JsonSerializer.Serialize(w.Snapshot().players,Json));
            Check(JsonSerializer.Serialize(original.receipts,Json)==JsonSerializer.Serialize(w.Snapshot().receipts,Json));
            var revision=w.Revision;Advance(w,200);Check(w.Revision==revision);
        });
        Test("picking up during the return cue cancels it and a held tool never expires",()=>{
            var w=SoloWorld.Create("first","second");Fill(w);Advance(w,183);
            Good(w,SoloAction.Grab,"bucket-1",actor:"second");Advance(w,250);
            Check(Toy(w,"bucket-1").holder=="second" && Toy(w,"bucket-1").water==3 && !Toy(w,"bucket-1").resetPending);
            Good(w,SoloAction.CancelGrab,"bucket-1",actor:"second");Advance(w,184);Check(Toy(w,"bucket-1").water==3);Advance(w,1);Check(Toy(w,"bucket-1").water==0);
        });
        Test("completed garden activities rearm and work again without clearing partial progress",()=>{
            var w=SoloWorld.Create("first");Fill(w);Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","plant-1",x:810,y:330);
            for(var i=0;i<3;i++){Good(w,SoloAction.Grab,"sponge-1");Good(w,SoloAction.Drop,"sponge-1","puddle-1",x:680,y:140);}
            Good(w,SoloAction.StartActivity,value:"cleanup");Advance(w,59);Check(Toy(w,"plant-1").water==3 && Toy(w,"puddle-1").water==0);
            Advance(w,1);Check(Toy(w,"plant-1").resetPending && Toy(w,"puddle-1").resetPending);
            Advance(w,5);Check(Toy(w,"plant-1").water==0 && Toy(w,"puddle-1").water==3 && w.ReadPlayer("first").activity=="cleanup");
            Good(w,SoloAction.Grab,"sponge-1");Good(w,SoloAction.Drop,"sponge-1","puddle-1",x:680,y:140);Advance(w,200);
            Check(Toy(w,"puddle-1").water==2 && !Toy(w,"sponge-1").wet);
            Fill(w);Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","plant-1",x:810,y:330);Check(Toy(w,"plant-1").water==3);
        });
        Test("another child's matching held tool protects a completed station but not unrelated areas",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first","second"));Fill(w);Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","plant-1",x:810,y:330);
            Advance(w,63);Good(w,SoloAction.Grab,"bucket-1",actor:"second");Advance(w,100);Check(Toy(w,"plant-1").water==3 && !Toy(w,"plant-1").resetPending);
            Good(w,SoloAction.Travel,value:"creek",actor:"second");Good(w,SoloAction.Grab,"bucket-creek",actor:"second");Advance(w,65);
            Check(Toy(w,"plant-1").water==0 && Toy(w,"bucket-creek").holder=="second" && w.ReadPlayer("second").zone=="creek");
        });
        Test("idle elapsed time survives restore and the entire family's absence pauses it",()=>{
            var w=SoloWorld.Create("first");Fill(w);Advance(w,100);var saved=Encode(w.Snapshot());
            var restored=SoloWorld.Restore(Decode(saved));var session=new FamilySession(restored);
            for(var i=0;i<300;i++)Check(!session.AdvanceIdle(1,out _));Check(Encode(restored.Snapshot())==saved);
            session.Attach(1,"first",out _);for(var i=0;i<84;i++)session.AdvanceIdle(1,out _);Check(Toy(restored,"bucket-1").water==3);
            session.AdvanceIdle(1,out _);Check(Toy(restored,"bucket-1").water==0 && session.View().idleTimers.Length==0);
        });
        Test("idle revision protects pending commands and replay cannot undo a committed return",()=>{
            var w=SoloWorld.Create("first");Fill(w);Advance(w,179);var old=Command(w,SoloAction.Grab,"bucket-1");Advance(w,6);
            Check(!w.Apply(old).Accepted);var c=Command(w,SoloAction.Grab,"bucket-1");Check(w.Apply(c).Accepted);var revision=w.Revision;
            Check(w.Apply(c).Duplicate && w.Revision==revision && Toy(w,"bucket-1").holder=="first");
        });
        Test("legacy saves retain placements and progress while invalid timer records are rejected",()=>{
            var w=SoloWorld.Create("first");Fill(w);var s=w.Snapshot();s.idleTimers=null;
            var restored=SoloWorld.Restore(s);Check(Toy(restored,"bucket-1").water==3 && restored.Snapshot().idleTimers.Length==0);
            s.idleTimers=new[]{new GardenIdleTimer{item="bucket-1",seconds=double.NaN}};Throws(()=>SoloWorld.Validate(s));
            s.idleTimers=new[]{new GardenIdleTimer{item="missing",seconds=5}};Throws(()=>SoloWorld.Validate(s));
            s.idleTimers=new[]{new GardenIdleTimer{item="tap-1",seconds=5}};Throws(()=>SoloWorld.Validate(s));
            Throws(()=>w.AdvanceIdle(100,out _));Throws(()=>w.AdvanceIdle(-1,out _));Throws(()=>w.AdvanceIdle(double.NaN,out _));
        });
        Test("family admission rejects another family authority world profile or credential",()=>{
            var family=Guid.NewGuid().ToString("N");var authority=Guid.NewGuid().ToString("N");var world=Guid.NewGuid().ToString("N");
            var secret=new string('a',64);var roster=Enumerable.Range(0,4).Select(i=>new FamilyMember{profile=Guid.NewGuid().ToString("N"),credentialHash=FamilyPairing.Hash(secret)}).ToArray();
            var paired=new FamilyPairing{familyId=family,authorityId=authority,worldId=world,role="server",serverName="lw-"+authority+".local",caCertificate="fixture",certificate="fixture",privateKey="fixture",members=roster};
            paired.Validate();var player=roster[0].profile;
            Check(paired.Admits(family,authority,world,player,secret));
            Check(!paired.Admits(Guid.NewGuid().ToString("N"),authority,world,player,secret));
            Check(!paired.Admits(family,Guid.NewGuid().ToString("N"),world,player,secret));
            Check(!paired.Admits(family,authority,Guid.NewGuid().ToString("N"),player,secret));
            Check(!paired.Admits(family,authority,world,Guid.NewGuid().ToString("N"),secret));
            Check(!paired.Admits(family,authority,world,player,new string('b',64)));
            Check(!paired.Admits(family,authority,world,player,null));
            Check(!paired.Admits(family,authority,world,player,new string('a',100000)));
            paired.members[0].credentialHash=new string('b',64);Check(!paired.Admits(family,authority,world,player,secret));
        });
        Test("discovery matching is bounded to enrolled authority family world and versions",()=>{
            var paired=new FamilyPairing{familyId=Guid.NewGuid().ToString("N"),authorityId=Guid.NewGuid().ToString("N"),worldId=Guid.NewGuid().ToString("N")};
            var ad=new FamilyAdvertisement{family=paired.familyId,authority=paired.authorityId,world=paired.worldId,schema=1,protocol=3,content=3};
            Check(ad.Matches(paired,3,3));Check(!ad.Matches(paired,2,3));Check(!ad.Matches(paired,3,2));
            ad.authority=Guid.NewGuid().ToString("N");Check(!ad.Matches(paired,3,3));ad.authority=paired.authorityId;
            ad.family=Guid.NewGuid().ToString("N");Check(!ad.Matches(paired,3,3));ad.family=paired.familyId;
            ad.world=Guid.NewGuid().ToString("N");Check(!ad.Matches(paired,3,3));
        });
        Test("player enrollment cannot contain authority private material or a malformed identity",()=>{
            var paired=new FamilyPairing{familyId=Guid.NewGuid().ToString("N"),authorityId=Guid.NewGuid().ToString("N"),worldId=Guid.NewGuid().ToString("N"),role="client",profile=Guid.NewGuid().ToString("N"),credential=new string('a',64),caCertificate="fixture"};
            paired.serverName="lw-"+paired.authorityId+".local";paired.Validate();
            paired.privateKey="server key";Throws(paired.Validate);paired.privateKey=null;
            paired.members=new[]{new FamilyMember()};Throws(paired.Validate);paired.members=null;
            paired.serverName="another.local";Throws(paired.Validate);
        });
        if(nativeBonjour)Test("native Bonjour advertises and resolves only the enrolled authority without leaked handles",()=>{
            var pair=new FamilyPairing{familyId=Guid.NewGuid().ToString("N"),authorityId=Guid.NewGuid().ToString("N"),worldId=Guid.NewGuid().ToString("N")};
            using var advertiser=new WindowsBonjour(pair,3,3);using var browser=new WindowsBonjour(pair,3,3);
            advertiser.Advertise(49199);browser.Browse();var clock=System.Diagnostics.Stopwatch.StartNew();FamilyEndpoint endpoint=null;
            while(clock.Elapsed.TotalSeconds<12 && endpoint==null){advertiser.Tick(clock.Elapsed.TotalSeconds);browser.Tick(clock.Elapsed.TotalSeconds);endpoint=browser.Take();System.Threading.Thread.Sleep(10);}
            Check(advertiser.Registered && endpoint!=null && endpoint.port==49199 && System.Net.IPAddress.TryParse(endpoint.address,out _));
            advertiser.Dispose();browser.Dispose();Check(advertiser.HandleCount==0 && browser.HandleCount==0);
        });
        Test("walking advances at server speed independent of packet count and transactions",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first","second"));var s=new FamilySession(w);s.Attach(1,"first",out _);s.Attach(2,"second",out _);var m=new MovementAuthority(w,s);
            var revision=w.Revision;var receiptCount=w.Snapshot().receipts.Length;var start=w.ReadPlayer("first").x;
            for(var i=1;i<=30;i++)
            {Check(m.Accept(1,new WalkInput{actor="first",zone="garden",sequence=i,mode=WalkMode.Direction,x=1},i/30.0));m.Tick(i/30.0,1f/30);}
            Check(Math.Abs(w.ReadPlayer("first").x-start-210)<.01 && w.Revision==revision && w.Snapshot().receipts.Length==receiptCount);
            Good(w,SoloAction.Grab,"bucket-1",actor:"second");Check(Toy(w,"bucket-1").holder=="second");
        });
        Test("walking rejects forged old and previous-visit inputs then times out safely",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first","second"));var s=new FamilySession(w);s.Attach(1,"first",out _);var m=new MovementAuthority(w,s);
            var input=new WalkInput{actor="first",zone="garden",sequence=4,mode=WalkMode.Direction,x=1,y=1};
            Check(!m.Accept(2,input,0));Check(m.Accept(1,input,0));Check(!m.Accept(1,input,0));input.actor="second";input.sequence=5;Check(!m.Accept(1,input,0));
            var before=w.ReadPlayer("first");m.Tick(.1,1f/30);var after=w.ReadPlayer("first");
            Check(Math.Abs(Math.Sqrt(Math.Pow(after.x-before.x,2)+Math.Pow(after.y-before.y,2))-7)<.001);
            Check(!m.Tick(.5,1f/30));Good(w,SoloAction.Travel,value:"creek");input.actor="first";Check(!m.Accept(1,input,.6));
            var entered=Encode(w.Snapshot());Check(!m.Tick(.6,1f/30) && Encode(w.Snapshot())==entered);
        });
        Test("stop sequence overrides delayed walking and disconnect cannot move an avatar",()=>{
            var w=SoloWorld.Create("first");var s=new FamilySession(w);s.Attach(1,"first",out _);var m=new MovementAuthority(w,s);
            Check(m.Accept(1,new WalkInput{actor="first",zone="garden",sequence=9,mode=WalkMode.Stop},0));
            Check(!m.Accept(1,new WalkInput{actor="first",zone="garden",sequence=8,mode=WalkMode.Direction,x=1},.1));Check(!m.Tick(.1,.03f));
            Check(m.Accept(1,new WalkInput{actor="first",zone="garden",sequence=10,mode=WalkMode.Direction,x=1},.2));s.Detach(1);Check(!m.Tick(.2,.03f));
        });
        Test("destination walking lands exactly and floor bounds retain finite positions",()=>{
            var goal=new WalkInput{mode=WalkMode.Destination,x=421,y=200};var point=Walking.Step(420,200,goal,.033f);Check(point.X==421 && point.Y==200);
            var diagonal=new WalkInput{mode=WalkMode.Direction,x=1000,y=1000};point=Walking.Step(960,455,diagonal,.033f);Check(point.X==960 && point.Y==455);
        });
        Test("timestamp interpolation smooths sparse samples and never extrapolates an outage",()=>{
            var b=new MotionBuffer();b.Add("garden:0",10,100,200);b.Add("garden:0",10.1,121,200);
            Check(Math.Abs(b.Sample(10.05).X-110.5)<.001 && b.Sample(11).X==121 && b.Sample(9).X==100);
            Check(!b.Add("garden:0",10.05,999,200));b.Add("creek:1",10.2,420,100);Check(b.Count==1 && b.Sample(10).X==420);
            for(var i=0;i<100;i++)b.Add("creek:1",11+i*.05f,i,100);Check(b.Count<=32);
        });
        Test("area upgrade preserves existing garden and runs only once",()=>{
            var old=SoloWorld.Create("first","second");Fill(old);Good(old,SoloAction.StartActivity,value:"garden");
            var before=old.Snapshot();var w=SoloWorld.WithAreas(SoloWorld.Restore(Decode(Encode(before))));var after=w.Snapshot();
            Check(after.schema==2 && after.worldId==before.worldId && after.revision==before.revision+1);
            Check(JsonSerializer.Serialize(after.players,Json)==JsonSerializer.Serialize(before.players,Json));
            Check(JsonSerializer.Serialize(after.toys.Take(5),Json)==JsonSerializer.Serialize(before.toys,Json));
            Check(JsonSerializer.Serialize(after.receipts,Json)==JsonSerializer.Serialize(before.receipts,Json));
            Check(Encode(SoloWorld.WithAreas(w).Snapshot())==Encode(after));
            var legacy=Decode(Encode(before));foreach(var p in legacy.players)p.zone=null;foreach(var t in legacy.toys)t.zone=null;
            Check(SoloWorld.WithAreas(SoloWorld.Restore(legacy)).ReadPlayer("first").zone=="garden");
        });
        Test("travel settles own station tool and never changes the sibling",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first","second"));Fill(w);Good(w,SoloAction.Grab,"bucket-1");
            Good(w,SoloAction.Grab,"sponge-1",actor:"second");Good(w,SoloAction.StartActivity,value:"cleanup",actor:"second");
            var sibling=JsonSerializer.Serialize(w.ReadPlayer("second"),Json);var sponge=JsonSerializer.Serialize(Toy(w,"sponge-1"),Json);
            var travel=Command(w,SoloAction.Travel,value:"creek");Check(w.Apply(travel).Accepted);var after=Encode(w.Snapshot());
            Check(w.Apply(travel).Duplicate && Encode(w.Snapshot())==after);
            Check(w.ReadPlayer("first").zone=="creek" && w.ReadPlayer("first").visit==1);
            Check(Toy(w,"bucket-1").holder=="" && Toy(w,"bucket-1").water==3 && Toy(w,"bucket-1").x==360);
            Check(JsonSerializer.Serialize(w.ReadPlayer("second"),Json)==sibling && JsonSerializer.Serialize(Toy(w,"sponge-1"),Json)==sponge);
        });
        Test("cross-area grabs drops and hints cannot affect remote toys",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first","second"));Good(w,SoloAction.Travel,value:"creek");
            Check(w.Apply(Command(w,SoloAction.Grab,"bucket-1")).Outcome=="wrong-area");Good(w,SoloAction.Grab,"bucket-creek");
            var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Drop,"bucket-creek","tap-1",x:150,y:340)).Accepted);
            Check(Encode(w.Snapshot())==before && !w.HasUsefulInteraction("bucket-creek","tap-1"));
            Good(w,SoloAction.Drop,"bucket-creek","tap-creek",x:150,y:340);Check(Toy(w,"bucket-creek").water==3 && Toy(w,"bucket-1").water==0);
        });
        Test("old visit input stays rejected after revision rebase and returning",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first"));var oldMove=Command(w,SoloAction.Move,x:900);
            Good(w,SoloAction.Travel,value:"creek");Good(w,SoloAction.Travel,value:"garden");oldMove.expectedRevision=w.Revision;
            var before=Encode(w.Snapshot());Check(w.Apply(oldMove).Outcome=="stale-area" && Encode(w.Snapshot())==before);
            Good(w,SoloAction.Move,x:800);Check(w.ReadPlayer("first").x==800 && w.ReadPlayer("first").visit==2);
        });
        Test("invalid travel changes nothing and empty areas retain progress",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first"));Fill(w);Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","plant-1",x:810,y:330);
            var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Travel,value:"missing")).Accepted && Encode(w.Snapshot())==before);
            var toys=JsonSerializer.Serialize(w.ReadToys(),Json);Good(w,SoloAction.Travel,value:"creek");Good(w,SoloAction.Travel,value:"garden");
            Check(JsonSerializer.Serialize(w.ReadToys(),Json)==toys && Toy(w,"plant-1").water==3);
        });
        Test("four-player area saves retain every location and detach releases only its hold",()=>{
            var w=SoloWorld.WithAreas(SoloWorld.Create("first","second","third","fourth"));var session=new FamilySession(w);
            session.Attach(1,"first",out _);session.Attach(2,"second",out _);session.Attach(3,"third",out _);session.Attach(4,"fourth",out _);
            Good(w,SoloAction.Travel,value:"creek");Good(w,SoloAction.Travel,value:"creek",actor:"third");
            Good(w,SoloAction.Grab,"bucket-creek");Good(w,SoloAction.Grab,"bucket-1",actor:"second");
            Check(session.Detach(1) && Toy(w,"bucket-creek").holder=="" && Toy(w,"bucket-1").holder=="second");
            var restored=SoloWorld.Restore(Decode(Encode(w.Snapshot())));Check(restored.ReadToys().Length==10 && restored.ReadToys().All(t=>t.holder==""));
            Check(restored.ReadPlayer("first").zone=="creek" && restored.ReadPlayer("third").zone=="creek" && restored.ReadPlayer("second").zone=="garden");
            var bad=restored.Snapshot();bad.toys[0].zone="creek";Throws(()=>SoloWorld.Validate(bad));
        });
        Test("client queue waits for acknowledgement before releasing a pending pickup",()=>{
            var q=new GardenCommandQueue();var w=SoloWorld.Create("first");var done=false;
            var grab=Command(w,SoloAction.Grab,"bucket-1");q.Enqueue(grab,r=>done=r.Accepted);
            Check(q.Take(4).requestId==grab.requestId && q.Take(4)==null);
            var cancel=Command(w,SoloAction.CancelGrab,"bucket-1");q.Enqueue(cancel,null);
            Check(q.Take(5)==null && !q.Complete("another-request",new SoloResult(true,"accepted",5)));
            q.Complete(grab.requestId,new SoloResult(true,"accepted",5));Check(done && q.Take(5).action==SoloAction.CancelGrab);
        });
        Test("explicit stale rejection rebases but an unknown result keeps its identity",()=>{
            var q=new GardenCommandQueue();var w=SoloWorld.Create("first");var callbacks=0;
            var cmd=Command(w,SoloAction.Drop,"bucket-1","tap-1",x:150,y:340);q.Enqueue(cmd,_=>callbacks++);
            Check(q.Take(9).expectedRevision==9 && q.Take(10)==null);
            q.Complete(cmd.requestId,new SoloResult(false,"stale-revision",10));
            var retry=q.Take(10);Check(retry.requestId==cmd.requestId && retry.expectedRevision==10 && callbacks==0);
            q.Complete(cmd.requestId,new SoloResult(true,"bucket-filled",11));Check(callbacks==1 && !q.Busy);
            Check(!q.Complete(cmd.requestId,new SoloResult(true,"bucket-filled",11)) && callbacks==1);
        });
        Test("queued movement coalesces without replacing a drop and cancellation has priority",()=>{
            var q=new GardenCommandQueue();var w=SoloWorld.Create("first");var replaced=false;
            q.Enqueue(Command(w,SoloAction.Move,x:10),r=>replaced=r.Outcome=="superseded");
            var drop=Command(w,SoloAction.Drop,"bucket-1");q.Enqueue(drop,null);
            q.Enqueue(Command(w,SoloAction.Move,x:20),null);Check(replaced);
            var cancel=Command(w,SoloAction.CancelGrab,"sponge-1");q.Enqueue(cancel,null);
            Check(q.Take(0).requestId==cancel.requestId);q.Complete(cancel.requestId,new SoloResult(true,"accepted",1));
            Check(q.Take(1).requestId==drop.requestId);q.Complete(drop.requestId,new SoloResult(true,"accepted",2));
            Check(q.Take(2).x==20);
        });
        Test("disconnect settles callbacks once and never sends queued old actions",()=>{
            var q=new GardenCommandQueue();var w=SoloWorld.Create("first");var failed=0;
            q.Enqueue(Command(w,SoloAction.Grab,"bucket-1"),r=>{if(r.Outcome=="disconnected")failed++;});
            q.Enqueue(Command(w,SoloAction.Drop,"bucket-1"),r=>{if(r.Outcome=="disconnected")failed++;});
            q.Take(0);q.Disconnect();q.Disconnect();Check(failed==2 && !q.Busy && q.Take(2)==null);
        });
        Test("cancellation survives sustained stale responses while ordinary actions stop",()=>{
            var q=new GardenCommandQueue();var w=SoloWorld.Create("first");var failures=0;
            q.Enqueue(Command(w,SoloAction.Grab,"bucket-1"),r=>{if(!r.Accepted)failures++;});
            for(var i=0;i<9;i++){var c=q.Take(i);q.Complete(c.requestId,new SoloResult(false,"stale-revision",i+1));}
            Check(failures==1 && !q.Busy);
            q.Enqueue(Command(w,SoloAction.CancelGrab,"bucket-1"),r=>failures++);
            for(var i=0;i<12;i++){var c=q.Take(i);q.Complete(c.requestId,new SoloResult(false,"stale-revision",i+1));}
            Check(q.Busy && failures==1);q.Disconnect();Check(failures==2 && !q.Busy);
        });
        Test("session admission prevents duplicate and unknown player connections",()=>{
            var w=SoloWorld.Create("first","second","third","fourth");var session=new FamilySession(w);
            Check(session.Attach(1,"first",out _));Check(!session.Attach(2,"first",out _));
            Check(!session.Attach(2,"unknown",out _));Check(!session.Attach(1,"second",out _));
            Check(session.Attach(2,"second",out _) && session.Attach(3,"third",out _) && session.Attach(4,"fourth",out _));
            Check(session.ConnectedPlayers.Length==4 && !session.Attach(5,"fifth",out _));
        });
        Test("session binds commands to the admitted player",()=>{
            var w=SoloWorld.Create("first","second");var session=new FamilySession(w);Check(session.Attach(1,"first",out _));
            var before=Encode(w.Snapshot());Check(!session.Submit(1,Command(w,SoloAction.Move,actor:"second",x:20,y:30)).Accepted);
            Check(!session.Submit(2,Command(w,SoloAction.Move,x:20,y:30)).Accepted);Check(Encode(w.Snapshot())==before);
        });
        Test("departing player releases only their prop and preserves the sibling",()=>{
            var w=SoloWorld.Create("first","second");var session=new FamilySession(w);session.Attach(1,"first",out _);session.Attach(2,"second",out _);
            Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Grab,"sponge-1",actor:"second");Good(w,SoloAction.StartActivity,value:"cleanup",actor:"second");
            var sibling=w.ReadPlayer("second");Check(session.Detach(1));Check(!session.Detach(1));
            Check(Toy(w,"bucket-1").holder=="" && Toy(w,"sponge-1").holder=="second" && w.ReadPlayer("second").activity==sibling.activity);
            Check(session.Attach(3,"first",out _) && session.ConnectedPlayers.Length==2);
        });
        Test("accepted duplicate stays idempotent after connection replacement",()=>{
            var w=SoloWorld.Create("first");var session=new FamilySession(w);session.Attach(1,"first",out _);
            var cmd=Command(w,SoloAction.Move,x:77,y:88);Check(session.Submit(1,cmd).Accepted);
            session.Detach(1);session.Attach(9,"first",out _);var revision=w.Revision;
            Check(session.Submit(9,cmd).Duplicate && w.Revision==revision && !session.Submit(1,cmd).Accepted);
            Check(session.View().receipts.Length==0 && session.Checkpoint().receipts.Length==1);
        });
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
        Test("picture hints match actual productive interactions without mutating state", () => {
            foreach(var item in new[]{"bucket-1","sponge-1"})
            foreach(var target in new[]{"tap-1","plant-1","puddle-1"})
            for(var bucket=0;bucket<=3;bucket++)for(var amount=0;amount<=3;amount++)
            {
                var saved=SoloWorld.Create("first").Snapshot();
                saved.toys.First(t=>t.id=="bucket-1").water=bucket;
                saved.toys.First(t=>t.id=="plant-1").water=amount;
                saved.toys.First(t=>t.id=="puddle-1").water=amount;
                var w=SoloWorld.Restore(saved);var unchanged=Encode(w.Snapshot());
                var hint=w.HasUsefulInteraction(item,target);Check(Encode(w.Snapshot())==unchanged);
                var before=w.ReadToys();var station=Toy(w,target);
                Good(w,SoloAction.Grab,item);Good(w,SoloAction.Drop,item,target,x:station.x,y:station.y);
                var after=w.ReadToys();var changed=before.Where((t,index)=>t.water!=after[index].water || t.wet!=after[index].wet).Any();
                Check(hint==changed);
            }
            var fresh=SoloWorld.Create("first");Check(!fresh.HasUsefulInteraction("missing","tap-1") && !fresh.HasUsefulInteraction("bucket-1","bucket-1"));
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
        Test("existing save directory cannot be mistaken for a missing world", () => {
            var store=Store("directory-at-save");var payload=Encode(SoloWorld.Create("first").Snapshot());store.Save(payload);
            var path=Path.Combine(root,"directory-at-save","world.save");
            File.Move(path,path+".preserved");Directory.CreateDirectory(path);
            Throws(()=>store.Load());Throws(()=>store.Save(payload));
            Check(File.ReadAllText(path+".preserved")==Envelope(payload));
        });
        Test("unreadable primary must not silently roll back to older backup", () => {
            var store=Store("locked-save");var w=SoloWorld.Create("first");store.Save(Encode(w.Snapshot()));
            Good(w,SoloAction.Move,x:222,y:200);var current=Encode(w.Snapshot());store.Save(current);
            var path=Path.Combine(root,"locked-save","world.save");
            using(var held=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None))
            {Throws(()=>store.Load());Throws(()=>store.Save(Encode(w.Snapshot())));}
            Check(store.Load().Payload==current);
        });
        Test("presentation copies cannot mutate player or toy authority", () => {
            var w=SoloWorld.Create("first","second");var before=Encode(w.Snapshot());
            var player=w.ReadPlayer("second");player.avatar="orange-pup";player.x=2;
            var toys=w.ReadToys();toys[0].water=3;toys[0].holder="second";toys[1]=null;
            Check(Encode(w.Snapshot())==before);Throws(()=>w.ReadPlayer("unknown"));
        });
        Test("presentation reads avoid copying checkpoint command history", () => {
            var w=SoloWorld.Create("first");
            for(var i=0;i<128;i++)Good(w,SoloAction.Move,x:i,y:100);
            var before=Encode(w.Snapshot());
            var oldIdle=Measure(()=>w.Snapshot().players[0].x);
            var newIdle=Measure(()=>w.ReadPlayer("first").x);
            var oldRender=Measure(()=>{var s=w.Snapshot();return s.players[0].x+s.toys[0].x;});
            var newRender=Measure(()=>w.ReadPlayer("first").x+w.ReadToys()[0].x);
            Check(oldIdle.checksum==newIdle.checksum && oldRender.checksum==newRender.checksum);
            Check(newIdle.bytes*10<oldIdle.bytes && newRender.bytes*10<oldRender.bytes);
            Check(Encode(w.Snapshot())==before);
            File.WriteAllText(Path.Combine(root,"presentation-allocations.json"),JsonSerializer.Serialize(new{
                runtime=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                iterations=1000,receipts=128,baselineIdleBytes=oldIdle.bytes,currentIdleBytes=newIdle.bytes,
                baselineRenderReadBytes=oldRender.bytes,currentRenderReadBytes=newRender.bytes,
                measuredScope="Pure C# state reads only; excludes Unity rendering, commands and serialization.",
                iPadPerformanceQualified=false},Json));
        });
        File.WriteAllText(Path.Combine(root,"results.json"),JsonSerializer.Serialize(new{utc=DateTime.UtcNow,checks=Results},Json));
        return failures == 0 ? 0 : 1;
    }
    static void Test(string name,Action body){try{body();Results.Add(new{name,passed=true,error=""});Console.WriteLine("PASS "+name);}catch(Exception e){failures++;Results.Add(new{name,passed=false,error=e.ToString()});Console.WriteLine("FAIL "+name+": "+e.Message);}}
    static (long bytes,float checksum) Measure(Func<float> read)
    {
        for(var i=0;i<100;i++)read();
        var before=GC.GetAllocatedBytesForCurrentThread();var sum=0f;
        for(var i=0;i<1000;i++)sum+=read();
        return(GC.GetAllocatedBytesForCurrentThread()-before,sum);
    }
    static void Check(bool condition){if(!condition)throw new Exception("Assertion failed.");}
    static void Throws(Action body){try{body();}catch{return;}throw new Exception("Expected rejection.");}
    static SoloCommand Command(SoloWorld w,SoloAction action,string item="",string target="",string value="",float x=0,float y=0,string actor="first")
    {var p=w.Snapshot().players.FirstOrDefault(p=>p.id==actor);return new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,expectedRevision=w.Revision,zone=p?.zone??"garden",visit=p?.visit??0,action=action,item=item,target=target,value=value,x=x,y=y};}
    static void Good(SoloWorld w,SoloAction action,string item="",string target="",string value="",float x=0,float y=0,string actor="first"){var r=w.Apply(Command(w,action,item,target,value,x,y,actor));Check(r.Accepted);}
    static SoloToy Toy(SoloWorld w,string id)=>w.Snapshot().toys.Single(t=>t.id==id);
    static void Advance(SoloWorld w,int seconds){for(var i=0;i<seconds;i++)w.AdvanceIdle(1,out _);}
    static void Fill(SoloWorld w){Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","tap-1",x:150,y:340);}
    static string Encode(SoloSnapshot s)=>JsonSerializer.Serialize(s,Json);
    static SoloSnapshot Decode(string s)=>JsonSerializer.Deserialize<SoloSnapshot>(s,Json);
    static bool Validate(string payload){try{var s=Decode(payload);if(s!=null&&s.schema>1)throw new NotSupportedException();SoloWorld.Validate(s);return true;}catch(JsonException){return false;}catch(InvalidOperationException){return false;}}
    static CheckpointStore Store(string name)=>new CheckpointStore(Path.Combine(root,name,"world.save"),Validate);
    static string Envelope(string payload)=>"LITTLEWEEPS-SOLO-1\n"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant()+"\n"+payload;
}
