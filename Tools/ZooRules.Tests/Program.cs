using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static class Program
{
    static JsonSerializerOptions json=new(){IncludeFields=true};
    static void Check(bool value,string name){if(!value)throw new Exception(name);}
    static string Encode(SoloSnapshot s)=>JsonSerializer.Serialize(s,json);
    static SoloCommand Command(SoloWorld w,string actor,SoloAction action,string value="",string target="",float x=0,float y=0)
    {var p=w.ReadPlayer(actor);return new(){requestId=Guid.NewGuid().ToString("N"),actor=actor,zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y};}
    static void Act(SoloWorld w,string actor,SoloAction action,string value="",string target="",float x=0,float y=0)
    {var result=w.Apply(Command(w,actor,action,value,target,x,y));Check(result.Accepted,result.Outcome);}
    static void Tick(SoloWorld w,int seconds){for(int i=0;i<seconds;i++){w.AdvanceIdle(1,out _);SoloWorld.Validate(w.Snapshot());}}
    static void Enter(SoloWorld w,string actor)
    {Act(w,actor,SoloAction.Travel,"zoo");Act(w,actor,SoloAction.Move,x:1200,y:100);Act(w,actor,SoloAction.Zoo,"gate","zoo-savanna");}
    static void Offer(SoloWorld w,string actor,string species)
    {Act(w,actor,SoloAction.Move,x:ZooLayout.BucketX(species),y:100);Act(w,actor,SoloAction.Zoo,"take",species);var food=w.ReadZoo().food.Single(f=>f.actor==actor);Act(w,actor,SoloAction.Move,x:ZooLayout.SlotX(species,food.slot),y:100);Act(w,actor,SoloAction.Zoo,"offer",species);}
    static void Main()
    {
        var old=SoloWorld.WithPark(SoloWorld.Create("one","two","three","four"));var before=old.Snapshot();var w=SoloWorld.WithZoo(old);var migrated=w.Snapshot();
        Check(w.Schema==34 && migrated.zoo.animals.Length==2,"Zoo migration");migrated.schema=before.schema;migrated.revision--;migrated.zoo=null;Check(Encode(migrated)==Encode(before),"migration must preserve the old world");
        var detached=w.ReadZoo();detached.animals[0].fed=77;Check(w.ReadZoo().animals[0].fed==0,"detached reads");Console.WriteLine("PASS additive migration and detached state");
        foreach(var actor in before.players.Select(p=>p.id))Enter(w,actor);
        Act(w,"one",SoloAction.Move,x:1200,y:490);Check(w.ReadPlayer("one").y==120,"visitor boundary");
        var routes=Enumerable.Range(0,80).Select(i=>{Tick(w,1);var a=w.ReadZoo().animals[0];return a.toX+"/"+a.toY;}).Distinct().Count();Check(routes>3,"varied destinations");
        var clone=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Encode(w.Snapshot()),json));Tick(w,10);Tick(clone,10);Check(Encode(w.ReadZooSnapshot())==Encode(clone.ReadZooSnapshot()),"persisted random choices");Console.WriteLine("PASS varied bounded routes and repeatable persisted authority stream");
        foreach(var actor in before.players.Select(p=>p.id))Offer(w,actor,"elephant");
        Check(w.ReadZoo().food.Select(f=>f.slot).Distinct().Count()==4,"four independent slots");
        var duplicate=Command(w,"one",SoloAction.Zoo,"offer","elephant");Check(w.Apply(duplicate).Accepted,"offer");Tick(w,1);Check(w.Apply(duplicate).Duplicate,"request idempotency");
        Tick(w,95);Check(w.ReadZoo().animals[0].fed==4 && w.ReadZoo().food.All(f=>f.species==""),"four portions exactly once");Console.WriteLine("PASS four shared feed slots and one consumption per portion");
        Offer(w,"one","elephant");Offer(w,"two","elephant");Tick(w,2);var sibling=w.ReadZoo().food.Single(f=>f.actor=="two").ticket;
        Act(w,"one",SoloAction.Travel,"creek");Check(w.ReadZoo().food.Single(f=>f.actor=="one").species=="" && w.ReadZoo().food.Single(f=>f.actor=="two").ticket==sibling,"independent departure");Tick(w,50);
        Check(w.ReadZoo().animals[0].fed==5,"only remaining portion consumed");Offer(w,"three","giraffe");Tick(w,2);var restored=SoloWorld.Restore(w.Snapshot());Check(restored.ReadZoo().food.All(f=>f.species==""),"restore cancels transient food leases");Check(restored.ReadZoo().animals[0].fed==5,"restore preserves feeding history");Console.WriteLine("PASS departure and restore retain sibling state and completed feeding");
        var session=new FamilySession(w);Check(session.Attach(1,"three",out _),"attach");session.Detach(1);Check(w.ReadZoo().food.Single(f=>f.actor=="three").species=="","disconnect food release");Console.WriteLine("PASS disconnected player releases only their food");
    }
    // Compare only Zoo fields: ordinary Home clocks are outside this focused check.
    static SoloSnapshot ReadZooSnapshot(this SoloWorld w)=>new(){zoo=w.ReadZoo()};
}
