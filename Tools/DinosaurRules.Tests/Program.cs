using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
static class Program
{
    static JsonSerializerOptions json=new(){IncludeFields=true};
    static void Need(bool value,string name){if(!value)throw new Exception(name);}
    static string Encode(object s)=>JsonSerializer.Serialize(s,json);
    static SoloResult Cmd(SoloWorld w,string who,SoloAction action,string value="",string target="",float x=0,float y=0)
    {var p=w.ReadPlayer(who);return w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=who,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target,x=x,y=y});}
    static void Act(SoloWorld w,string who,SoloAction action,string value="",string target="",float x=0,float y=0)
    {var r=Cmd(w,who,action,value,target,x,y);Need(r.Accepted,r.Outcome);SoloWorld.Validate(w.Snapshot());}
    static void Tick(SoloWorld w,int seconds){for(var i=0;i<seconds;i++){w.AdvanceIdle(1,out _);SoloWorld.Validate(w.Snapshot());}}
    static void CareTests()
    {
        var w=SoloWorld.WithDinosaurWorld(SoloWorld.Create("one","two","three","four"));
        var ids=w.ReadPlayers().Select(p=>p.id).ToArray();
        foreach(var who in ids)Act(w,who,SoloAction.Travel,DinosaurRides.Area);
        for(var i=0;i<4;i++){
            var who=ids[i];var species=DinosaurRides.Species[i];
            Act(w,who,SoloAction.Move,x:DinosaurCareRules.BucketX(species),y:100);
            Act(w,who,SoloAction.Dinosaur,"take",species);var f=w.ReadDinosaurWorld().care[i];
            Act(w,who,SoloAction.Dinosaur,"take",species);Need(w.ReadDinosaurWorld().nextCareTicket==i+1,"repeated take created a second portion");
            Act(w,who,SoloAction.Move,x:f.x,y:f.y);Act(w,who,SoloAction.Dinosaur,"offer",species);
            Act(w,who,SoloAction.Dinosaur,"offer",species);
            Need(!Cmd(w,who,SoloAction.Dinosaur,"mount",species).Accepted,"mount stole cared animal");
        }
        Tick(w,15);Need(w.ReadDinosaurWorld().animals.All(a=>a.fed==1),"all four species did not eat exactly once");
        Need(w.ReadDinosaurWorld().care.All(f=>f.phase==DinosaurCarePhase.None),"feeding did not finish");Console.WriteLine("PASS all four species feed once; repeated food taps keep one turn; riding cannot steal care");
        for(var i=0;i<4;i++){
            var a=w.ReadDinosaurWorld().animals[i];var x=Math.Max(510,Math.Min(4400,a.x+DinosaurCareRules.PetX(a.species)));
            Act(w,ids[i],SoloAction.Move,x:x,y:100);Act(w,ids[i],SoloAction.Dinosaur,"pet",a.species);Act(w,ids[i],SoloAction.Dinosaur,"pet",a.species);
        }
        Tick(w,12);Need(w.ReadDinosaurWorld().animals.All(a=>a.petted==1),"four simultaneous petters");Console.WriteLine("PASS all four species accept shared petting, one gentle turn per repeated tap");
        var saved=w.Snapshot();var old=SoloWorld.Restore(saved);var s=old.Snapshot();s.schema=36;s.dinosaurWorld.care=null;s.dinosaurWorld.nextCareTicket=0;foreach(var a in s.dinosaurWorld.animals){a.fed=0;a.petted=0;}
        var recovered36=SoloWorld.Restore(s);var upgrade=SoloWorld.WithDinosaurWorld(recovered36).Snapshot();Need(upgrade.schema==37 && upgrade.dinosaurWorld.care.Length==4,"36 migration");Need(upgrade.dinosaurWorld.animals.Zip(s.dinosaurWorld.animals,(a,b)=>a.x==b.x && a.y==b.y && a.random==b.random).All(x=>x),"migration moved dinosaurs");
        var species0="triceratops";
        foreach(var who in ids){Act(w,who,SoloAction.Move,x:DinosaurCareRules.BucketX(species0),y:100);Act(w,who,SoloAction.Dinosaur,"take",species0);var f=w.ReadDinosaurWorld().care.Single(v=>v.actor==who);Act(w,who,SoloAction.Move,x:f.x,y:f.y);Act(w,who,SoloAction.Dinosaur,"offer",species0);}
        Need(w.ReadDinosaurWorld().care.Select(f=>f.x).Distinct().Count()==4,"shared contact spots overlap");
        Tick(w,1);var checkpoint=w.Snapshot();var reopened=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Encode(checkpoint),json));Need(reopened.ReadDinosaurWorld().care.All(f=>f.phase==DinosaurCarePhase.None),"recovery stale care");Need(reopened.ReadDinosaurWorld().animals[1].fed==checkpoint.dinosaurWorld.animals[1].fed,"recovery erased meals");Console.WriteLine("PASS schema 36 additive upgrade and care JSON recovery retain progress, clear temporary turns");
        var session=new FamilySession(w);session.Attach(1,"one",out _);session.Attach(2,"two",out _);session.Attach(3,"three",out _);session.Attach(4,"four",out _);session.Detach(1);
        Act(w,"two",SoloAction.Travel,"creek");Need(w.ReadDinosaurWorld().care.Count(f=>f.phase!=DinosaurCarePhase.None)==2,"departure removed siblings");
        Tick(w,24);Need(w.ReadDinosaurWorld().animals[1].fed==3,"remaining queued siblings did not each finish");Console.WriteLine("PASS four-player queue and independent disconnect/travel preserve two sibling completions");
        Act(w,"three",SoloAction.Dinosaur,"mount",species0);Need(!Cmd(w,"four",SoloAction.Dinosaur,"pet",species0).Accepted,"petted ridden animal");Act(w,"three",SoloAction.Dinosaur,"off");
        var a0=w.ReadDinosaurWorld().animals[1];Act(w,"four",SoloAction.Move,x:Math.Max(510,Math.Min(4400,a0.x+65)),y:100);Act(w,"four",SoloAction.Dinosaur,"pet",species0);
        w.AdvanceIdle(.1,out _,new[]{"three"});Need(w.ReadDinosaurWorld().care.All(f=>f.phase==DinosaurCarePhase.None),"inactive player care stayed live");
        a0=w.ReadDinosaurWorld().animals[1];Act(w,"three",SoloAction.Move,x:Math.Max(510,Math.Min(4400,a0.x+65)),y:230);Act(w,"three",SoloAction.Dinosaur,"pet",species0);Tick(w,10);Need(w.ReadDinosaurWorld().animals.All(a=>DinosaurRides.Point(a.x,a.y)),"petting at rear edge leaves floor");
        var bad=w.Snapshot();bad.dinosaurWorld.care[0].phase=DinosaurCarePhase.Eat;try{SoloWorld.Validate(bad);throw new Exception("invalid care accepted");}catch(InvalidOperationException){}Console.WriteLine("PASS ridden-dinosaur rejection, inactive-player cleanup and malformed care rejection");
    }
    static void Main()
    {
        var old=SoloWorld.WithZoo(SoloWorld.Create("one","two","three","four"));var before=old.Snapshot();var w=SoloWorld.WithDinosaurWorld(old);var migrated=w.Snapshot();
        Need(migrated.schema==37 && migrated.dinosaurWorld.animals.Length==4,"migration");migrated.schema=before.schema;migrated.revision--;migrated.dinosaurWorld=null;Need(Encode(migrated)==Encode(before),"old state changed");
        var detached=w.ReadDinosaurWorld();detached.animals[0].x=999;Need(w.ReadDinosaurWorld().animals[0].x==600,"detached reads");Console.WriteLine("PASS additive migration retains the complete previous world; detached reads");
        Tick(w,7); // Mount after simulation has already run, as real clients do.
        foreach(var p in before.players)Act(w,p.id,SoloAction.Travel,DinosaurRides.Area);
        Act(w,"one",SoloAction.Dinosaur,"mount","tyrannosaurus");Need(!Cmd(w,"two",SoloAction.Dinosaur,"mount","tyrannosaurus").Accepted,"busy mount stolen");
        Act(w,"one",SoloAction.Dinosaur,"mount","tyrannosaurus");Need(w.ReadPlayer("one").fixture==DinosaurRides.Fixture("tyrannosaurus"),"repeat tap dismounts");
        for(var i=1;i<4;i++)Act(w,before.players[i].id,SoloAction.Dinosaur,"mount",DinosaurRides.Species[i]);Need(w.ReadPlayers().Select(p=>p.fixture).Distinct().Count()==4,"four shared leases");Console.WriteLine("PASS four simultaneous mounts, exclusive occupancy and repeat-tap retention");
        var session=new FamilySession(w);Need(session.Attach(1,"one",out _),"session");var movement=new MovementAuthority(w,session);var p1=w.ReadPlayer("one");
        Need(movement.Accept(1,new WalkInput{actor="one",zone=p1.zone,visit=p1.visit,sequence=1,mode=WalkMode.Direction,x=-1},0),"input");movement.Tick(.1,.1f);
        Need(w.ReadPlayer("one").x<p1.x && w.ReadPlayer("one").fixture!="" && w.ReadDinosaurWorld().animals[0].left,"mounted continuous motion");var a=w.ReadDinosaurWorld().animals[0];Need(a.x==w.ReadPlayer("one").x,"rider attachment");var stopped=a.x;movement.Tick(1,.1f);Need(w.ReadDinosaurWorld().animals[0].x==stopped,"stale input moved mount");
        Act(w,"one",SoloAction.Move,x:0,y:490);Need(w.ReadPlayer("one").x==280 && w.ReadPlayer("one").y==230,"safe bounds");Console.WriteLine("PASS shared movement retains mount, turns, times out and clamps at valley edges");
        Need(!Cmd(w,"one",SoloAction.Dinosaur,"call","tyrannosaurus").Accepted,"call spam");Tick(w,4);Act(w,"one",SoloAction.Dinosaur,"call","tyrannosaurus");Need(w.ReadDinosaurWorld().animals[0].calls==2,"one event per call");Console.WriteLine("PASS replicated call sequence and three-second cooldown");
        Act(w,"two",SoloAction.Dinosaur,"off");Need(w.ReadPlayer("two").fixture=="" && w.ReadPlayer("three").fixture!="","off interrupted sibling");Act(w,"three",SoloAction.Travel,"creek");Need(w.ReadPlayer("four").fixture!="","travel interrupted sibling");session.Detach(1);Need(w.ReadPlayer("one").fixture=="" && w.ReadPlayer("four").fixture!="","disconnect interrupted sibling");Console.WriteLine("PASS get-off, area departure and disconnect release only the participating rider");
        var saved=w.Snapshot();var recovered=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Encode(saved),json));Need(recovered.ReadPlayers().All(p=>p.fixture==""),"restore retained stale lease");Need(Encode(recovered.ReadDinosaurWorld())==Encode(saved.dinosaurWorld),"recovery changed dinosaur positions/RNG");Console.WriteLine("PASS JSON/recovery retains dinosaur positions and releases temporary rider leases");
        Act(w,"four",SoloAction.Dinosaur,"off");var copy=SoloWorld.Restore(w.Snapshot());var destinations=Enumerable.Range(0,100).Select(i=>{Tick(w,1);Tick(copy,1);return w.ReadDinosaurWorld().animals[0].targetX;}).Distinct().Count();Need(destinations>3 && Encode(w.ReadDinosaurWorld())==Encode(copy.ReadDinosaurWorld()),"wander random persistence");Console.WriteLine("PASS varied bounded roaming and repeatable authority-owned random stream");
        var bad=w.Snapshot();bad.dinosaurWorld.animals[0].x=float.NaN;try{SoloWorld.Validate(bad);throw new Exception("invalid state accepted");}catch(InvalidOperationException){}Console.WriteLine("PASS malformed dinosaur state rejected");CareTests();
    }
}
