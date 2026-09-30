using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static class Program
{
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static CreekBoat Boat(SoloWorld w,string actor)=>w.ReadCreekBoats().boats.Single(b=>b.actor==actor);
    static SoloCommand Command(SoloWorld w,string actor,string action,string target="",string item="")
    {var p=w.ReadPlayer(actor);return new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=SoloAction.CreekBoat,value=action,target=target,item=item};}
    static void Send(SoloWorld w,string actor,string action,string target="")=>Require(w.Apply(Command(w,actor,action,target)).Accepted,"Rejected "+action);
    static void Tick(SoloWorld w,double seconds){for(double t=0;t<seconds;t+=.05)w.AdvanceIdle(Math.Min(.05,seconds-t),out _);}
    static void Main()
    {
        var old=SoloWorld.WithPond(SoloWorld.Create("one","two","three","four"));var before=old.Snapshot();
        var world=SoloWorld.WithCreekBoats(old);var after=world.Snapshot();
        Require(after.schema==37 && after.worldId==before.worldId && after.players.Select(p=>p.id).SequenceEqual(before.players.Select(p=>p.id)) && after.toys.Select(t=>t.id).SequenceEqual(before.toys.Select(t=>t.id)),"Migration replaced existing records");
        Require(JsonSerializer.Serialize(after.pond,new JsonSerializerOptions{IncludeFields=true})==JsonSerializer.Serialize(before.pond,new JsonSerializerOptions{IncludeFields=true}),"Migration changed pond");
        SoloWorld.Validate(after);Console.WriteLine("PASS additive migration retains family, objects and pond");
        var actors=new[]{"one","two","three","four"};
        foreach(var actor in actors){Send(world,actor,"start");Send(world,actor,"hull",(Array.IndexOf(actors,actor)%3).ToString());Send(world,actor,"passenger");Send(world,actor,"flower");Send(world,actor,"dock",(Array.IndexOf(actors,actor)%2).ToString());}
        Require(world.ReadCreekBoats().boats.All(b=>b.attending && b.passenger && b.flower),"Shared boat preparation lost decorations");
        Require(!world.Apply(Command(world,"one","flower",item:"two")).Accepted,"Sibling creation stolen");
        Require(!world.Apply(Command(world,"one","dock","99")).Accepted,"Invalid destination accepted");
        var request=Command(world,"one","colour","2");Require(world.Apply(request).Accepted && world.Apply(request).Duplicate,"Boat action replay failed");
        Require(Boat(world,"one").colour==2,"Colour missing");
        foreach(var actor in actors)Send(world,actor,"launch");
        Require(world.ReadCreekBoats().boats.All(b=>b.phase==CreekBoatPhase.Floating),"Boats not in common river");
        Require(!world.Apply(Command(world,"one","hull","2")).Accepted && !world.Apply(Command(world,"one","retrieve")).Accepted,"In-flight boat was reset");
        Tick(world,5);var copied=world.Snapshot();copied.creekBoats.boats[1].flower=false;
        Require(Boat(world,"two").flower,"View modified authoritative creation");
        var positions=world.ReadCreekBoats().boats.Select(b=>CreekBoats.Point(b,world.ReadCreekBoats().clock)).ToArray();Require(positions.Select(p=>p.X).Distinct().Count()==4,"Four boats overlap on one route");
        var family=new FamilySession(world);for(ulong i=1;i<=4;i++)Require(family.Attach(i,actors[i-1],out _),"Attach failed");
        Require(family.Detach(4) && !Boat(world,"four").attending && Boat(world,"four").phase==CreekBoatPhase.Floating && Boat(world,"two").attending,"Departure disrupted the river");
        Send(world,"one","leave");Tick(world,10);
        Require(world.ReadCreekBoats().boats.All(b=>b.phase==CreekBoatPhase.Docked && b.trips==1 && b.passenger && b.flower),"Boats lost creation or failed to dock after departure");
        Console.WriteLine("PASS four shared boats, two docks, protected edits and independent departure");
        Send(world,"two","retrieve");Tick(world,5.1);Require(Boat(world,"two").phase==CreekBoatPhase.Ready && Boat(world,"two").flower && Boat(world,"three").phase==CreekBoatPhase.Docked,"Retrieval reset sibling or decoration");
        Send(world,"two","launch");Tick(world,1);
        var options=new JsonSerializerOptions{IncludeFields=true};var json=JsonSerializer.Serialize(world.Snapshot(),options);var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(json,options));
        Require(Boat(restored,"two").phase==CreekBoatPhase.Docked && Boat(restored,"two").passenger && Boat(restored,"two").flower && restored.ReadCreekBoats().boats.All(b=>!b.attending),"Reopen lost creation or retained temporary participation");
        Require(restored.WorldId==before.worldId && restored.ReadToys().Select(t=>t.id).SequenceEqual(before.toys.Select(t=>t.id)),"Reopen replaced existing toys");
        SoloWorld.Validate(restored.Snapshot());Console.WriteLine("PASS retrieve, relaunch and saved creations reopen at reachable docks");
        var stale=Command(world,"three","retrieve");var p=world.ReadPlayer("three");
        Require(world.Apply(new SoloCommand{actor=p.id,requestId=Guid.NewGuid().ToString("N"),expectedRevision=world.Revision,zone=p.zone,visit=p.visit,action=SoloAction.Travel,value="park"}).Accepted,"Travel rejected");
        Require(!Boat(world,"three").attending && !world.Apply(stale).Accepted && Boat(world,"two").phase==CreekBoatPhase.Floating,"Travel accepted stale creek input or stopped a sibling");
        Console.WriteLine("PASS independent travel rejects stale creek actions");
    }
}
