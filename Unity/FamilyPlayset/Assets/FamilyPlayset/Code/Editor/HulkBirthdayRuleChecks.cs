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
        var foundation=restored.Snapshot();foundation.birthday.version=1;foundation.birthday.cast=null;foundation.birthday.guestOrder=null;foundation.birthday.props=foundation.birthday.props.Take(25).ToArray();
        var upgraded=GameWorld.Restore(foundation);
        Need(upgraded.ReadBirthday().version==3 && upgraded.ReadBirthday().props.Length==27 && upgraded.ReadBirthday().props[0].id=="birthday-gift-0","foundation migration preserves existing props");
        var party=GameWorld.WithBirthday(GameWorld.Create(Actors));
        Send(party,"one",SoloAction.Travel,HulkBirthday.Zone);
        party.AdvanceIdle(.5,out _,new[]{"one"});
        var started=party.ReadBirthday();var intro=started.introAt;
        Need(started.cast.Count(n=>n.id.StartsWith("birthday-guest-") && n.active)==2 && started.guestOrder.Distinct().Count()==13,"one shared intro selects two unique civilian guests");
        for(int i=0;i<20;i++)party.AdvanceIdle(.5,out _,new[]{"one"});
        var arrived=party.ReadBirthday();
        Need(arrived.introDone && arrived.props.Count(p=>p.location=="table")==2,"guests arrive and deliver both gifts");
        Send(party,"two",SoloAction.Travel,HulkBirthday.Zone);party.AdvanceIdle(.5,out _,new[]{"one","two"});
        Need(party.ReadBirthday().introAt==intro,"late arrival does not restart introduction");
        var companion=party.ReadBirthday().cast.Single(n=>n.role==HulkBirthday.Roles[1]);
        Need(companion.controller=="two","one role is controlled by one joining player");
        party.ReleaseBirthday("two");Need(party.ReadBirthday().cast.Single(n=>n.id==companion.id).controller=="","departed role remains as companion");
        for(int i=0;i<110;i++)party.AdvanceIdle(.5,out _,new[]{"one"});
        Need(party.ReadBirthday().cast.Skip(4).All(n=>!n.active) && party.ReadBirthday().props.Count(p=>p.location=="table")==2,"guests depart without losing delivered gifts");
        GameWorld.Validate(party.Snapshot());
        foreach(var actor in Actors){Send(party,actor,SoloAction.Travel,HulkBirthday.Zone);Send(party,actor,SoloAction.Move,x:2400,y:200);}
        party.AdvanceIdle(.5,out _,Actors);
        Need(Send(party,"one",SoloAction.Birthday,"cake-base","1").Accepted,"shared cake base");
        Need(Send(party,"two",SoloAction.Birthday,"cake-sticker","0",x:.4f,y:.5f).Accepted,"second player adds candle");
        Need(Send(party,"three",SoloAction.Birthday,"cake-sticker","11",x:.6f,y:.5f).Accepted,"third player decorates same cake");
        Need(party.ReadBirthday().stickers.Length==2,"one shared creation");
        Need(Send(party,"four",SoloAction.Birthday,"cake-finish").Accepted,"fourth player finishes shared cake");
        foreach(var actor in Actors)Need(Send(party,actor,SoloAction.Birthday,"serve").Accepted,"four portions from one cake");
        foreach(var actor in Actors)Need(Send(party,actor,SoloAction.Birthday,"eat").Accepted,"shared snack consumption");
        for(int i=0;i<6;i++)party.AdvanceIdle(.5,out _,Actors);
        Need(party.ReadBirthday().props.Where(p=>p.kind=="food").All(p=>p.location=="stock" && p.holder==""),"consumed portions return to bounded supply");
        Need(party.ReadBirthday().cakeReady && party.ReadBirthday().stickers.Length==2,"eating does not erase family cake");
        var giftWorld=GameWorld.WithBirthday(GameWorld.Create(Actors));
        foreach(var actor in Actors){Send(giftWorld,actor,SoloAction.Travel,HulkBirthday.Zone);Send(giftWorld,actor,SoloAction.Move,x:650,y:160);}
        giftWorld.AdvanceIdle(.5,out _,Actors);
        Need(Send(giftWorld,"two",SoloAction.Birthday,"claim","birthday-gift-0").Accepted,"helper brings birthday gift");
        Need(Send(giftWorld,"two",SoloAction.Birthday,"unwrap").Accepted,"one shared Hulk unwrap transaction");
        Need(!Send(giftWorld,"three",SoloAction.Birthday,"unwrap").Accepted,"second unwrap cannot start in parallel");
        for(int i=0;i<6;i++)giftWorld.AdvanceIdle(.5,out _,Actors);
        Need(giftWorld.ReadBirthday().props[0].opened && giftWorld.ReadBirthday().props[0].content==0 && giftWorld.ReadBirthday().props[0].holder=="one","Hulk reveals the actual cake outcome once");
        Need(!Send(giftWorld,"one",SoloAction.Birthday,"unwrap").Accepted,"opened gift cannot open twice");
        Need(Send(giftWorld,"one",SoloAction.Birthday,"eat").Accepted,"Hulk eats revealed cake");
        for(int i=0;i<6;i++)giftWorld.AdvanceIdle(.5,out _,Actors);
        Need(giftWorld.ReadBirthday().props[0].location=="stock" && !giftWorld.ReadBirthday().props[0].opened,"consumed gift returns to bounded guest supply");
        var savedParty=GameWorld.Restore(party.Snapshot());Need(savedParty.ReadBirthday().stickers.Length==2 && savedParty.ReadBirthday().stickerX[0]==.4f,"cake placement survives restore");
        GameWorld.Validate(giftWorld.Snapshot());
        var replay=GameWorld.WithBirthday(GameWorld.Create(Actors));
        foreach(var actor in Actors){Send(replay,actor,SoloAction.Travel,HulkBirthday.Zone);Send(replay,actor,SoloAction.Move,x:650,y:160);}
        replay.AdvanceIdle(.5,out _,Actors);Send(replay,"one",SoloAction.Birthday,"claim","birthday-gift-0");
        for(int i=0;i<8;i++){
            Need(Send(replay,"one",SoloAction.Birthday,"unwrap").Accepted,"repeated source outcome starts");
            for(int frame=0;frame<6;frame++)replay.AdvanceIdle(.5,out _,Actors);
            var shown=replay.ReadBirthday();Need(shown.props[0].opened && shown.props[0].content==i,"all eight mapped outcomes can appear");
            Need(shown.reaction==(i==2 || i==3 || i==4?"negative":"happy"),"source dislike traits drive Hulk reaction");
            Need(Send(replay,"one",SoloAction.Birthday,"wrap").Accepted,"explicit bounded replay keeps the same object");
        }
        Need(Send(replay,"one",SoloAction.Birthday,"unwrap").Accepted,"interruptible unwrap starts");
        Need(Send(replay,"one",SoloAction.Move,x:680,y:160).Accepted,"walking away interrupts own unwrap");
        Need(replay.ReadBirthday().giftAt<0 && replay.ReadBirthday().props[0].holder=="one" && !replay.ReadBirthday().props[0].opened,"cancellation preserves one unopened held gift");
        GameWorld.Validate(replay.Snapshot());
        var invalid=world.Snapshot();invalid.birthday.props[0].id="invented";
        try{GameWorld.Validate(invalid);throw new Exception("invalid prop identity accepted");}catch(InvalidOperationException){}
        Console.WriteLine("PASS: authority fixtures; migration, claims, handoff, snapshot isolation, instrument sharing, intro, guests, late join, departure and JSON restore. Not a four-client runtime test.");
    }
}
