using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld KeepyWorld()=>SoloWorld.WithKeepyUppy(SoloWorld.Create("first","second"));
    static void TickKeepy(SoloWorld w,double duration,double dt=1.0/60,bool follow=false,string actor="first",string[] active=null)
    {
        for(var time=0.0;time<duration-1e-8;time+=dt)
        {
            if(follow){var b=w.ReadKeepy();Good(w,SoloAction.Move,x:b.x,y:b.y,actor:actor);}
            w.AdvanceIdle(Math.Min(dt,duration-time),out _,active??new[]{actor});
        }
    }
    static void KeepyTests()
    {
        Test("keepy upgrade preserves home, all players, props and receipts",()=>{
            var old=SoloWorld.WithHome(SoloWorld.Create("first","second"));Good(old,SoloAction.SetFixture,target:"radio-living",value:"on");
            var before=old.Snapshot();var w=SoloWorld.WithKeepyUppy(old);var after=w.Snapshot();
            Check(after.schema==5 && after.keepy.phase==0 && after.revision==before.revision+1 && after.worldId==before.worldId);
            Check(JsonSerializer.Serialize(before.players,Json)==JsonSerializer.Serialize(after.players,Json));
            Check(JsonSerializer.Serialize(before.toys,Json)==JsonSerializer.Serialize(after.toys,Json));
            Check(JsonSerializer.Serialize(before.receipts,Json)==JsonSerializer.Serialize(after.receipts,Json));
            Check(after.home.livingRadio && ReferenceEquals(w,SoloWorld.WithKeepyUppy(w)));
        });
        Test("keepy belongs only to home; tapping preserves current activity, position and held item",()=>{
            var w=KeepyWorld();Good(w,SoloAction.Travel,value:"creek");var before=JsonSerializer.Serialize(w.Snapshot(),Json);
            Check(!w.Apply(Command(w,SoloAction.StartActivity,value:KeepyRules.Activity)).Accepted);
            Check(before==JsonSerializer.Serialize(w.Snapshot(),Json));Good(w,SoloAction.Travel,value:"garden");Good(w,SoloAction.Grab,"bucket-1");
            var player=JsonSerializer.Serialize(w.ReadPlayer("first"),Json);
            Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);
            Check(w.ReadKeepy().phase==1 && w.ReadKeepy().vz>0);
            Check(JsonSerializer.Serialize(w.ReadPlayer("first"),Json)==player && w.Snapshot().toys.First(t=>t.id=="bucket-1").holder=="first");
        });
        Test("walking under balloon produces repeated single taps with drifting air-filled flight",()=>{
            foreach(var avatar in new[]{"blue-pup","orange-pup"})
            {
                var w=KeepyWorld();Good(w,SoloAction.ChangeAvatar,value:avatar);Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);
                TickKeepy(w,18,follow:true);var b=w.ReadKeepy();
                Check(b.phase==1 && b.hitSerial>=4 && b.hitSerial<=9 && b.lastHitter=="first" && b.x!=-4240);
                Check(Walking.Speed==420);SoloWorld.Validate(w.Snapshot());
            }
        });
        Test("overlapping siblings create one authoritative hit and cannot reset a running round",()=>{
            var w=KeepyWorld();Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);var round=w.ReadKeepy().round;
            Good(w,SoloAction.StartActivity,value:KeepyRules.Activity,actor:"second");Check(w.ReadKeepy().round==round);
            for(var i=0;i<300 && w.ReadKeepy().hitSerial==0;i++)
            {var b=w.ReadKeepy();Good(w,SoloAction.Move,x:b.x,y:b.y);Good(w,SoloAction.Move,x:b.x,y:b.y,actor:"second");w.AdvanceIdle(1.0/60,out _);}
            var hit=w.ReadKeepy();Check(hit.hitSerial==1 && hit.lastHitter=="first");
            TickKeepy(w,.3,follow:true);Check(w.ReadKeepy().hitSerial==1);
        });
        Test("wrong floor depth and inactive characters cannot hit; ground rest has no automatic restart",()=>{
            var w=KeepyWorld();Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);Good(w,SoloAction.Move,x:-4240,y:500);
            TickKeepy(w,8);Check(w.ReadKeepy().phase==0 && w.ReadKeepy().hitSerial==0);var b=w.ReadKeepy();
            TickKeepy(w,3);Check(w.ReadKeepy().round==b.round && w.ReadKeepy().phase==0);
            Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);Check(w.ReadKeepy().phase==1 && w.ReadKeepy().round==b.round+1);
        });
        Test("empty home pauses flight; reopen resumes the saved balloon without catch-up",()=>{
            var w=KeepyWorld();Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);TickKeepy(w,8,follow:true);
            var b=w.ReadKeepy();TickKeepy(w,10,active:Array.Empty<string>());
            Check(JsonSerializer.Serialize(b,Json)==JsonSerializer.Serialize(w.ReadKeepy(),Json));
            w=SoloWorld.Restore(w.Snapshot());Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);
            Check(w.ReadKeepy().round==b.round && w.ReadKeepy().hitSerial==b.hitSerial && w.ReadKeepy().height==b.height);
        });
        Test("detached profiles cannot ghost-hit; connected sibling can play without joining an activity",()=>{
            var w=KeepyWorld();var family=new FamilySession(w);Check(family.Attach(1,"first",out _) && family.Attach(2,"second",out _));
            Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);Check(family.Detach(1));
            for(var i=0;i<420;i++)
            {
                var b=w.ReadKeepy();Good(w,SoloAction.Move,x:b.x,y:b.y);Good(w,SoloAction.Move,x:b.x,y:b.y,actor:"second");
                family.AdvanceIdle(1.0/60,out _);
            }
            Check(w.ReadKeepy().hitSerial>=1 && w.ReadKeepy().lastHitter=="second");
            Good(w,SoloAction.Travel,value:"creek",actor:"second");var saved=w.ReadKeepy();
            for(var i=0;i<120;i++)family.AdvanceIdle(1.0/60,out _);
            Check(w.ReadKeepy().elapsed==saved.elapsed);
        });
        Test("occupied hands and seats do not invent arm contacts",()=>{
            foreach(var busy in new[]{"toy","seat"})
            {
                var w=KeepyWorld();Good(w,SoloAction.StartActivity,value:KeepyRules.Activity);
                if(busy=="toy")Good(w,SoloAction.Grab,"bucket-1");else Good(w,SoloAction.UseFixture,target:"sofa-left");
                for(var i=0;i<500;i++)
                {
                    var b=w.ReadKeepy();
                    if(busy=="toy"){Good(w,SoloAction.Move,x:b.x,y:b.y);}
                    w.AdvanceIdle(1.0/60,out _,new[]{"first"});
                }
                Check(w.ReadKeepy().hitSerial==0 && w.ReadKeepy().phase==0);
            }
        });
        Test("fixed balloon steps are independent of render rate and save fractions",()=>{
            var a=KeepyWorld();Good(a,SoloAction.StartActivity,value:KeepyRules.Activity);
            var snapshot=a.Snapshot();var b=SoloWorld.Restore(snapshot);Good(b,SoloAction.StartActivity,value:KeepyRules.Activity);
            Good(a,SoloAction.Move,x:1000,y:200);Good(b,SoloAction.Move,x:1000,y:200);
            TickKeepy(a,2,1.0/60);TickKeepy(b,2,1.0/30);
            Check(Math.Abs(a.ReadKeepy().height-b.ReadKeepy().height)<.001 && Math.Abs(a.ReadKeepy().x-b.ReadKeepy().x)<.001);
        });
        Test("keepy malformed clocks, floor points, ownership and future schema refuse",()=>{
            var w=KeepyWorld();
            foreach(var change in new Action<SoloSnapshot>[] {s=>s.keepy.height=float.NaN,s=>s.keepy.vz=999,s=>s.keepy.remainder=1,
                s=>s.keepy.lastHitter="missing",s=>s.keepy.phase=2,s=>s.schema=6,s=>s.keepy=null,
                s=>{s.players[0].activity=KeepyRules.Activity;s.players[0].zone="beach";}})
            {var s=w.Snapshot();change(s);Throws(()=>SoloWorld.Validate(s));}
            var r=RecoveryFixture();r.snapshot=SoloWorld.WithKeepyUppy(SoloWorld.Restore(r.snapshot)).Snapshot();r.content=6;
            r.Validate(r.family,r.authority,r.world);r.content=5;Throws(()=>r.Validate(r.family,r.authority,r.world));
        });
    }
}
