using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static partial class Program
{
    static void HomeTests()
    {
        Test("legacy Unity inline empty home metadata remains compatible",()=>{
            var legacy=SoloWorld.WithScenery(SoloWorld.Create("first")).Snapshot();legacy.home=new HomeState();
            SoloWorld.Validate(legacy);var w=SoloWorld.WithHome(SoloWorld.Restore(legacy));Check(w.Schema==4);
            legacy.home.shedOpen=true;Throws(()=>SoloWorld.Validate(legacy));
        });
        Test("home upgrade preserves identities, receipts, water and all prior positions",()=>{
            var old=SoloWorld.WithScenery(SoloWorld.Create("first","second"));Fill(old);var before=old.Snapshot();
            var w=SoloWorld.WithHome(old);var s=w.Snapshot();Check(s.schema==4 && s.worldId==before.worldId && s.toys.Length==11);
            Check(JsonSerializer.Serialize(before.toys,Json)==JsonSerializer.Serialize(s.toys.Take(10),Json));
            Check(JsonSerializer.Serialize(before.players,Json)==JsonSerializer.Serialize(s.players,Json));
            Check(JsonSerializer.Serialize(before.receipts,Json)==JsonSerializer.Serialize(s.receipts,Json));
            Check(ReferenceEquals(w,SoloWorld.WithHome(w)));SoloWorld.Validate(s);
        });
        Test("seat slots reject competing children atomically and repeated entry is idempotent",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first","second"));var c=Command(w,SoloAction.UseFixture,target:"sofa-left");
            Check(w.Apply(c).Accepted);var before=JsonSerializer.Serialize(w.Snapshot(),Json);
            Check(w.Apply(c).Duplicate);
            Check(!w.Apply(Command(w,SoloAction.UseFixture,target:"sofa-left",actor:"second")).Accepted);
            Check(JsonSerializer.Serialize(w.Snapshot(),Json)==before);
            Good(w,SoloAction.UseFixture,target:"sofa-right",actor:"second");Good(w,SoloAction.ChangeAvatar,value:"orange-pup");
            Check(w.ReadPlayer("first").fixture=="sofa-left" && w.ReadPlayer("second").fixture=="sofa-right");
        });
        Test("four players fit both original-size fixtures; release affects only their own spot",()=>{
            var ids=new[]{"first","second","third","fourth"};
            var w=SoloWorld.WithKeepyUppy(SoloWorld.Create(ids));
            foreach(var group in new[]{HomeLayout.SofaSlots,HomeLayout.TrampolineSlots})
            {
                for(var i=0;i<4;i++)Good(w,SoloAction.UseFixture,target:group[i],actor:ids[i]);
                Check(w.Snapshot().players.Select(p=>p.fixture).Distinct().Count()==4);
                Check(w.Snapshot().players.Max(p=>p.x)-w.Snapshot().players.Min(p=>p.x)<=220);
                Good(w,SoloAction.LeaveFixture,actor:"second");
                Check(w.Snapshot().players.Count(p=>p.fixture!="")==3);
                SoloWorld.Validate(w.Snapshot());
            }
        });
        Test("ride entry settles a held full bucket and walking releases only that player's slot",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first","second"));Fill(w);Good(w,SoloAction.Grab,"bucket-1");
            Good(w,SoloAction.UseFixture,target:"trampoline-left");Good(w,SoloAction.UseFixture,target:"sofa-right",actor:"second");
            var toy=w.ReadToys().Single(t=>t.id=="bucket-1");Check(toy.holder=="" && toy.water==3);
            Walking.AdvanceLocal(w,"first",WalkMode.Direction,1,0,.05f);
            Check(w.ReadPlayer("first").fixture=="" && w.ReadPlayer("second").fixture=="sofa-right");SoloWorld.Validate(w.Snapshot());
        });
        Test("travel, departure and restoration safely release temporary home occupancy",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first","second"));var family=new FamilySession(w);Check(family.Attach(1,"first",out _));
            Good(w,SoloAction.UseFixture,target:"trampoline-left");for(var i=0;i<20;i++)w.AdvanceIdle(.1,out _);
            Check(w.ReadPlayer("first").useSeconds>1);Check(SoloWorld.Restore(w.Snapshot()).ReadPlayer("first").fixture=="");
            family.Detach(1);Check(w.ReadPlayer("first").fixture=="");
            Good(w,SoloAction.UseFixture,target:"sofa-left");Good(w,SoloAction.Travel,value:"creek");Check(w.ReadPlayer("first").fixture=="");
            Check(!w.Apply(Command(w,SoloAction.UseFixture,target:"sofa-left")).Accepted);
        });
        Test("shed keeps exact item and contents through close, travel, reload and retrieval",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first","second"));Fill(w);
            Good(w,SoloAction.SetFixture,target:"shed",value:"on");Good(w,SoloAction.Grab,"bucket-1");
            Good(w,SoloAction.Drop,"bucket-1","shed-0",x:HomeLayout.StorageX(0),y:HomeLayout.StorageY(0));
            Good(w,SoloAction.Grab,"ball-1");Good(w,SoloAction.Drop,"ball-1","shed-2",x:HomeLayout.StorageX(2),y:HomeLayout.StorageY(2));
            Good(w,SoloAction.SetFixture,target:"shed",value:"off");Good(w,SoloAction.Travel,value:"creek");Good(w,SoloAction.Travel,value:"garden");
            w=SoloWorld.Restore(w.Snapshot());Check(!w.ReadHome().shedOpen && w.ReadToys().Count(t=>t.container!="")==2);
            Check(!w.Apply(Command(w,SoloAction.Grab,"bucket-1")).Accepted);
            Good(w,SoloAction.SetFixture,target:"shed",value:"on");Good(w,SoloAction.Grab,"bucket-1");
            Check(w.ReadToys().Single(t=>t.id=="bucket-1").water==3 && w.ReadToys().Single(t=>t.id=="bucket-1").container=="");
        });
        Test("storage close/drop and capacity races reject without losing the held item",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first","second"));Good(w,SoloAction.Grab,"ball-1");
            var before=JsonSerializer.Serialize(w.Snapshot(),Json);
            Check(!w.Apply(Command(w,SoloAction.Drop,"ball-1","shed-0",x:HomeLayout.StorageX(0),y:430)).Accepted);
            Check(JsonSerializer.Serialize(w.Snapshot(),Json)==before);
            Good(w,SoloAction.SetFixture,target:"shed",value:"on");Good(w,SoloAction.Drop,"ball-1","shed-0",x:HomeLayout.StorageX(0),y:430);
            Good(w,SoloAction.Grab,"bucket-1",actor:"second");
            Check(!w.Apply(Command(w,SoloAction.Drop,"bucket-1","shed-0",x:HomeLayout.StorageX(0),y:430,actor:"second")).Accepted);
            Check(w.ReadToys().Single(t=>t.id=="bucket-1").holder=="second");SoloWorld.Validate(w.Snapshot());
        });
        Test("stored borrowed tools retain return policy; protected home ball stays stored",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first"));Fill(w);Good(w,SoloAction.SetFixture,target:"shed",value:"on");
            Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","shed-0",x:HomeLayout.StorageX(0),y:430);
            Good(w,SoloAction.Grab,"ball-1");Good(w,SoloAction.Drop,"ball-1","shed-2",x:HomeLayout.StorageX(2),y:160);
            for(var i=0;i<100;i++)w.AdvanceIdle(1,out _);w=SoloWorld.Restore(w.Snapshot());
            for(var i=0;i<86;i++)w.AdvanceIdle(1,out _);
            var bucket=w.ReadToys().Single(t=>t.id=="bucket-1");Check(bucket.container=="" && bucket.x==360 && bucket.water==0);
            Check(w.ReadToys().Single(t=>t.id=="ball-1").container=="shed-2");SoloWorld.Validate(w.Snapshot());
        });
        Test("radio state persists independently and proximity does not mutate players",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first","second"));Good(w,SoloAction.SetFixture,target:"radio-living",value:"on");
            Good(w,SoloAction.Move,x:-3500,y:100);Check(HomeLayout.RadioNear(w.ReadHome(),w.ReadPlayer("first")));
            Check(!HomeLayout.RadioNear(w.ReadHome(),w.ReadPlayer("second")));
            w=SoloWorld.Restore(w.Snapshot());Check(w.ReadHome().livingRadio);
            var copy=w.ReadHome();copy.livingRadio=false;Check(w.ReadHome().livingRadio);
        });
        Test("home save validation rejects duplicate storage, holders in storage and malformed leases",()=>{
            var w=SoloWorld.WithHome(SoloWorld.Create("first","second"));Good(w,SoloAction.UseFixture,target:"sofa-left");
            var s=w.Snapshot();s.players[1]=s.players[0].Copy();s.players[1].id="second";Throws(()=>SoloWorld.Validate(s));
            s=w.Snapshot();s.players[0].useSeconds=double.NaN;Throws(()=>SoloWorld.Validate(s));
            s=w.Snapshot();s.toys[0].container="shed-0";Throws(()=>SoloWorld.Validate(s));
            s=w.Snapshot();s.home=null;Throws(()=>SoloWorld.Validate(s));
            s=w.Snapshot();var ball=s.toys.Single(t=>t.kind==ToyKind.Ball);ball.zone="creek";ball.id="ball-creek";Throws(()=>SoloWorld.Validate(s));
        });
        Test("content 5 recovery validates home state and refuses mismatched generation",()=>{
            var r=RecoveryFixture();r.snapshot=SoloWorld.WithHome(SoloWorld.Restore(r.snapshot)).Snapshot();r.content=5;
            r.Validate(r.family,r.authority,r.world);r.content=4;Throws(()=>r.Validate(r.family,r.authority,r.world));
        });
    }
}
