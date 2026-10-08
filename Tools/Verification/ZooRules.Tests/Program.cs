using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static class Program
{
    static JsonSerializerOptions json=new(){IncludeFields=true};
    static void Check(bool value,string name){if(!value)throw new Exception(name);}
    static string Encode(SoloSnapshot s)=>JsonSerializer.Serialize(s,json);
    static SoloCommand Command(GameWorld w,string actor,SoloAction action,string value="",string target="",float x=0,float y=0)
    {var p=w.ReadPlayer(actor);return new(){requestId=Guid.NewGuid().ToString("N"),actor=actor,zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y};}
    static void Act(GameWorld w,string actor,SoloAction action,string value="",string target="",float x=0,float y=0)
    {var result=w.Apply(Command(w,actor,action,value,target,x,y));Check(result.Accepted,result.Outcome);}
    static void Tick(GameWorld w,int seconds){for(int i=0;i<seconds;i++){w.AdvanceIdle(1,out _);GameWorld.Validate(w.Snapshot());}}
    static void Enter(GameWorld w,string actor)
    {Act(w,actor,SoloAction.Travel,"zoo");Act(w,actor,SoloAction.Move,x:650,y:100);Act(w,actor,SoloAction.Zoo,"gate","zoo-savanna");}
    static void Offer(GameWorld w,string actor,string species)
    {Act(w,actor,SoloAction.Move,x:ZooLayout.BucketX(species),y:100);Act(w,actor,SoloAction.Zoo,"take",species);var food=w.ReadZoo().food.Single(f=>f.actor==actor);Act(w,actor,SoloAction.Move,x:ZooLayout.SlotX(species,food.slot),y:100);Act(w,actor,SoloAction.Zoo,"offer",species);}
    static void Main()
    {
        var old=GameWorld.WithPark(GameWorld.Create("one","two","three","four"));var before=old.Snapshot();var w=GameWorld.WithZoo(old);var migrated=w.Snapshot();
        Check(w.Schema==35 && migrated.zoo.animals.Length==16,"Zoo migration");migrated.schema=before.schema;migrated.revision--;migrated.zoo=null;Check(Encode(migrated)==Encode(before),"migration must preserve the old world");
        var detached=w.ReadZoo();detached.animals[0].fed=77;Check(w.ReadZoo().animals[0].fed==0,"detached reads");Console.WriteLine("PASS additive migration and detached state");
        var firstSlice=w.Snapshot();firstSlice.schema=34;firstSlice.zoo.animals=firstSlice.zoo.animals.Take(2).ToArray();firstSlice.zoo.animals[0].fed=12;
        var priorZoo=Encode(new SoloSnapshot{zoo=firstSlice.zoo});var expanded=GameWorld.WithZoo(GameWorld.Restore(firstSlice));var expansion=expanded.Snapshot();
        Check(expansion.schema==35 && expansion.zoo.animals.Length==16,"Sixteen-species migration");
        // Restore now always retires Zoo transient play (00083a2), advancing
        // revision once even when the historical fixture has no food lease.
        // Expansion adds its own revision. Keep the full snapshot comparison.
        Check(expansion.revision==firstSlice.revision+2,"Restore and expansion each advance revision once");
        expansion.schema=34;expansion.revision-=2;expansion.zoo.animals=expansion.zoo.animals.Take(2).ToArray();
        Check(Encode(expansion)==Encode(firstSlice),"First slice retains existing world and animals");
        Check(expanded.ReadZoo().animals[0].fed==12,"Feeding history survives expansion");Console.WriteLine("PASS schema 34 retains elephant/giraffe and adds fourteen animals without moving possessions");
        foreach(var actor in before.players.Select(p=>p.id))Enter(w,actor);
        Act(w,"one",SoloAction.Move,x:1200,y:490);Check(w.ReadPlayer("one").y==120,"visitor boundary");
        // World IDs seed the random routine; an 80-second sample can contain
        // only two long routes. Observe several bounded cycles for every seed.
        var routes=Enumerable.Range(0,600).Select(i=>{Tick(w,1);var a=w.ReadZoo().animals[0];return a.toX+"/"+a.toY;}).Distinct().Count();Check(routes>3,"varied destinations");
        var checkpoint=Encode(w.Snapshot());
        var clone=GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(checkpoint,json));
        var twin=GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(checkpoint,json));
        Check(clone.ReadZoo().greetingCooldown==30 && clone.ReadZoo().curiousCooldown==14,"Restore applies intentional elephant play cooldowns");
        Check(clone.ReadZoo().animals.Skip(1).Select(a=>a.random).SequenceEqual(w.ReadZoo().animals.Skip(1).Select(a=>a.random)),"Restore preserves ordinary animal random streams");
        Tick(w,10);Tick(clone,10);Tick(twin,10);
        Check(Encode(clone.ReadZooSnapshot())==Encode(twin.ReadZooSnapshot()),"identical persisted checkpoints produce identical full Zoo state after restore");
        Check(JsonSerializer.Serialize(w.ReadZoo().animals.Skip(1),json)==JsonSerializer.Serialize(clone.ReadZoo().animals.Skip(1),json),"ordinary animal routes continue identically across restore");
        Console.WriteLine("PASS varied bounded routes and repeatable persisted authority stream with transient play reset");
        foreach(var actor in before.players.Select(p=>p.id))Offer(w,actor,"elephant");
        Check(w.ReadZoo().food.Select(f=>f.slot).Distinct().Count()==4,"four independent slots");
        var duplicate=Command(w,"one",SoloAction.Zoo,"offer","elephant");Check(w.Apply(duplicate).Accepted,"offer");Tick(w,1);Check(w.Apply(duplicate).Duplicate,"request idempotency");
        Tick(w,95);Check(w.ReadZoo().animals[0].fed==4 && w.ReadZoo().food.All(f=>f.species==""),"four portions exactly once");Console.WriteLine("PASS four shared feed slots and one consumption per portion");
        Offer(w,"one","elephant");Offer(w,"two","elephant");Tick(w,2);var sibling=w.ReadZoo().food.Single(f=>f.actor=="two").ticket;
        Act(w,"one",SoloAction.Travel,"creek");Check(w.ReadZoo().food.Single(f=>f.actor=="one").species=="" && w.ReadZoo().food.Single(f=>f.actor=="two").ticket==sibling,"independent departure");Tick(w,50);
        Check(w.ReadZoo().animals[0].fed==5,"only remaining portion consumed");Offer(w,"three","giraffe");Tick(w,2);var restored=GameWorld.Restore(w.Snapshot());Check(restored.ReadZoo().food.All(f=>f.species==""),"restore cancels transient food leases");Check(restored.ReadZoo().animals[0].fed==5,"restore preserves feeding history");Console.WriteLine("PASS departure and restore retain sibling state and completed feeding");
        var session=new FamilySession(w);Check(session.Attach(1,"three",out _),"attach");session.Detach(1);Check(w.ReadZoo().food.Single(f=>f.actor=="three").species=="","disconnect food release");Console.WriteLine("PASS disconnected player releases only their food");
        foreach(var info in ZooCatalog.All){
            if(ZooCatalog.Trail(w.ReadPlayer("one").zone)){Act(w,"one",SoloAction.Move,x:200,y:100);Act(w,"one",SoloAction.Zoo,"gate","zoo");}else if(w.ReadPlayer("one").zone!="zoo")Act(w,"one",SoloAction.Travel,"zoo");Act(w,"one",SoloAction.Move,x:ZooCatalog.EntranceX(info.area),y:100);Act(w,"one",SoloAction.Zoo,"gate",info.area);
            for(var slot=0;slot<4;slot++){var target=ZooLayout.SlotX(info.id,slot)+65-info.mouthX;Check(target>=info.MinX && target<=info.MaxX,"Mouth reaches every offer place: "+info.id);}
            var fed=w.ReadZoo().animals.Single(a=>a.species==info.id).fed;Offer(w,"one",info.id);Tick(w,100);
            Check(w.ReadZoo().animals.Single(a=>a.species==info.id).fed==fed+1,"One exact portion for "+info.id);
            Check(w.ReadZoo().food.Single(f=>f.actor=="one").species=="","Released portion for "+info.id);
            Act(w,"one",SoloAction.Move,x:9200,y:100);Act(w,"one",SoloAction.Zoo,"gate",ZooCatalog.Next(info.area));Check(w.ReadPlayer("one").x==420,"Next trail arrival");
            Act(w,"one",SoloAction.Move,x:200,y:100);Act(w,"one",SoloAction.Zoo,"gate",info.area);Check(w.ReadPlayer("one").x==9000,"Reverse trail arrival");
        }
        Console.WriteLine("PASS all sixteen foods consumed once, all trails and bidirectional ring gates");
        Tick(w,600);Check(w.ReadZoo().animals.All(a=>a.sequence>2),"Every animal chooses new routines");Console.WriteLine("PASS every animal varies its bounded routine independently");LittleWeeps.EditorTools.ElephantStoryTests.Run();LittleWeeps.EditorTools.ElephantHabitatTests.Run();
    }
    // Compare only Zoo fields: ordinary Home clocks are outside this focused check.
    static SoloSnapshot ReadZooSnapshot(this GameWorld w)=>new(){zoo=w.ReadZoo()};
}
