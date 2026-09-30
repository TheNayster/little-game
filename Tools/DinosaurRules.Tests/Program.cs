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
    static void Main()
    {
        var old=SoloWorld.WithZoo(SoloWorld.Create("one","two","three","four"));var before=old.Snapshot();var w=SoloWorld.WithDinosaurWorld(old);var migrated=w.Snapshot();
        Need(migrated.schema==36 && migrated.dinosaurWorld.animals.Length==4,"migration");migrated.schema=before.schema;migrated.revision--;migrated.dinosaurWorld=null;Need(Encode(migrated)==Encode(before),"old state changed");
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
        var bad=w.Snapshot();bad.dinosaurWorld.animals[0].x=float.NaN;try{SoloWorld.Validate(bad);throw new Exception("invalid state accepted");}catch(InvalidOperationException){}Console.WriteLine("PASS malformed dinosaur state rejected");
    }
}
