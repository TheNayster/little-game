using System;
using System.Linq;
#if !UNITY_EDITOR
using System.Text.Json;
#endif
using LittleWeeps.Core;

public static class HulkBirthdayRuleChecks
{
    static readonly string[] Actors={"one","two","three","four"};
    static void Need(bool value,string why){if(!value)throw new Exception(why);}
    static SoloResult Send(GameWorld world,string actor,SoloAction action,string value="",string item="",string target="",float x=0,float y=0,string[] connected=null)
    {
        var p=world.ReadPlayer(actor);return world.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),expectedRevision=world.Revision,actor=actor,
            zone=p.zone,visit=p.visit,action=action,value=value,item=item,target=target,x=x,y=y},connected);
    }
    static void Main()=>Run();
    public static void Run()
    {
#if UNITY_EDITOR
        var legacy=GameWorld.WithZoo(GameWorld.Create(Actors));
        GameWorld.Validate(UnityEngine.JsonUtility.FromJson<SoloSnapshot>(UnityEngine.JsonUtility.ToJson(legacy.Snapshot())));
#endif
        var old=GameWorld.WithDinosaurWorld(GameWorld.Create(Actors)).Snapshot();
        // Simulate the preceding shipped schema without regenerating existing worlds.
        old.schema=53;old.birthday=null;
        var before=old.toys.Select(t=>t.id).ToArray();var world=GameWorld.WithBirthday(GameWorld.Restore(old));
        Need(world.Schema==54 && world.ReadBirthday().members.Length==4,"additive birthday migration");
        Need(world.ReadToys().Select(t=>t.id).SequenceEqual(before),"existing inventory retained");
        foreach(var actor in Actors){Need(Send(world,actor,SoloAction.Travel,HulkBirthday.Zone).Accepted,"party arrival");Need(Send(world,actor,SoloAction.Move,x:650,y:160).Accepted,"walk to gift");}
        Need(Send(world,"one",SoloAction.Birthday,"claim","birthday-gift-0").Accepted,"first prop claim");
        Need(!Send(world,"two",SoloAction.Birthday,"claim","birthday-gift-0").Accepted,"contended claim rejected");
        Need(!Send(world,"one",SoloAction.Birthday,"claim","birthday-gift-1").Accepted,"holding slot blocks second prop");
        Need(!Send(world,"one",SoloAction.Birthday,"give","birthday-gift-0","two",connected:new[]{"one","three","four"}).Accepted,"handoff to disconnected profile rejected");
        Need(Send(world,"one",SoloAction.Birthday,"give","birthday-gift-0","two").Accepted,"atomic handoff");
        Need(world.ReadBirthday().props.Single(p=>p.id=="birthday-gift-0").holder=="two","one shared owner");
        var detached=world.Snapshot();detached.birthday.props[0].style=3;
        Need(world.ReadBirthday().props[0].style==0,"snapshot deep copy");
        Need(Send(world,"three",SoloAction.Birthday,"station",target:"guitar").Accepted,"instrument claim");
        Need(!Send(world,"four",SoloAction.Birthday,"station",target:"guitar").Accepted,"instrument contention");
        Need(Send(world,"four",SoloAction.Birthday,"station",target:"drums").Accepted,"second shared instrument");
        world.AdvanceIdle(.5,out _,Actors);var song=world.ReadBirthday().songAt;
        world.ReleaseBirthday("three");Need(world.ReadBirthday().songAt==song && world.ReadBirthday().members.Single(m=>m.actor=="four").station=="drums","independent instrument departure");
        Need(Send(world,"two",SoloAction.Travel,"park").Accepted,"independent world departure");
        Need(world.ReadBirthday().props[0].holder=="" && world.ReadBirthday().members.Single(m=>m.actor=="four").station=="drums","departure releases own prop only");
        Need(Send(world,"one",SoloAction.Birthday,"claim","birthday-gift-0").Accepted,"released gift reusable");
#if UNITY_EDITOR
        var checkpoint=UnityEngine.JsonUtility.FromJson<SoloSnapshot>(UnityEngine.JsonUtility.ToJson(world.Snapshot()));
#else
        var options=new JsonSerializerOptions{IncludeFields=true};
        var checkpoint=JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(world.Snapshot(),options),options);
#endif
        var restored=GameWorld.Restore(checkpoint);GameWorld.Validate(restored.Snapshot());
        Need(restored.ReadBirthday().props[0].holder=="" && restored.ReadBirthday().songAt<0,"restore cancels transient leases");
        Need(restored.ReadBirthday().props[0].x==650 && restored.ReadBirthday().props[0].y==160,"restore retains carried prop at player");
        var invalid=world.Snapshot();invalid.birthday.props[0].id="invented";
        try{GameWorld.Validate(invalid);throw new Exception("invalid prop identity accepted");}catch(InvalidOperationException){}
        Console.WriteLine("PASS: four-member authority fixtures; migration, claims, handoff, snapshot isolation, instrument sharing, departure and JSON restore. Not a four-client runtime test.");
    }
}
