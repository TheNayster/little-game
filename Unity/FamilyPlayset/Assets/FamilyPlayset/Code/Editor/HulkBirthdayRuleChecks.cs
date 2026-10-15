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
    static GameWorld QuietBirthday()
    {
        var snapshot=GameWorld.WithBirthday(GameWorld.Create(Actors)).Snapshot();
        // Activity-specific fixtures defer autonomous trouble. Separate checks
        // below exercise its real filters/timers and cooperative recovery.
        snapshot.birthday.nextTheft=snapshot.birthday.nextElectro=snapshot.birthday.nextHide=snapshot.birthday.nextStageElectro=1000000;
        return GameWorld.Restore(snapshot);
    }
    static void CheckTrouble()
    {
        var seed=QuietBirthday();
        foreach(var actor in Actors){Send(seed,actor,SoloAction.Travel,HulkBirthday.Zone);Send(seed,actor,SoloAction.Move,x:2400,y:180);}
        foreach(int villainIndex in new[]{0,1,3}){
        string villainId="birthday-villain-"+villainIndex;
        var snapshot=seed.Snapshot();var b=snapshot.birthday;b.introDone=true;b.nextTheft=0;foreach(var villain in b.villains) villain.x=2400;b.villains[villainIndex].x=100;
        b.props[25].location="table";b.props[25].x=2450;b.props[25].y=180;
        var theft=GameWorld.Restore(snapshot);theft.AdvanceIdle(.5,out _,Actors);
        Need(theft.ReadBirthday().villain==villainIndex && theft.ReadBirthday().villains[villainIndex].phase=="approach","source theft filters select an offscreen non-Electro with a table gift");
        for(int i=0;i<11;i++)theft.AdvanceIdle(.5,out _,Actors);
        Need(theft.ReadBirthday().props[25].holder==villainId,"one original gift stolen atomically");
        Need(Send(theft,"two",SoloAction.Birthday,"web",target:villainId).Accepted,"first spider catches shared thief");
        double trap=theft.ReadBirthday().villains[villainIndex].trapUntil;
        Need(Send(theft,"three",SoloAction.Birthday,"web",target:villainId).Accepted && theft.ReadBirthday().villains[villainIndex].trapUntil==trap,"second spider helps the same trap without resetting it");
        Send(theft,"two",SoloAction.Travel,"garden");
        Need(theft.ReadBirthday().villains[villainIndex].webbers.SequenceEqual(new[]{"three"}) && theft.ReadBirthday().villains[villainIndex].phase=="trapped","one webber leaves independently");
        var captured=theft.ReadBirthday().villains[villainIndex];Send(theft,"one",SoloAction.Move,x:captured.x-100,y:captured.y);
        Need(Send(theft,"one",SoloAction.Birthday,"smash",target:villainId).Accepted,"Hulk starts cooperative rescue");
        for(int i=0;i<5;i++)theft.AdvanceIdle(.5,out _,Actors);
        Need(theft.ReadBirthday().troublesStopped==1 && theft.ReadBirthday().props[25].holder=="" && theft.ReadBirthday().villains[villainIndex].phase=="launched","one shared impact releases the actual stolen gift");GameWorld.Validate(theft.Snapshot());
        }
        var electroSeed=seed.Snapshot();electroSeed.birthday.introDone=true;electroSeed.birthday.nextElectro=0;electroSeed.birthday.villains[2].x=HulkBirthday.BatteryX;electroSeed.birthday.villains[2].y=HulkBirthday.BatteryY;
        electroSeed.birthday.props[25].location="table";electroSeed.birthday.props[25].x=2450;
        foreach(var p in electroSeed.players){p.x=HulkBirthday.BatteryX;p.y=HulkBirthday.BatteryY;}
        var power=GameWorld.Restore(electroSeed);power.AdvanceIdle(.5,out _,Actors);
        Need(!power.ReadBirthday().powered && power.ReadBirthday().props.Single(p=>p.kind=="battery").holder=="birthday-villain-2","Electro steals the same battery and changes band power");
        Need(Send(power,"two",SoloAction.Birthday,"web",target:"birthday-villain-2").Accepted && Send(power,"one",SoloAction.Birthday,"smash",target:"birthday-villain-2").Accepted,"cooperative battery rescue");
        for(int i=0;i<5;i++)power.AdvanceIdle(.5,out _,Actors);
        Need(Send(power,"one",SoloAction.Birthday,"claim","birthday-battery").Accepted && Send(power,"one",SoloAction.Birthday,"socket").Accepted && power.ReadBirthday().powered,"recovered battery returns to its socket");GameWorld.Validate(power.Snapshot());
        var fake=QuietBirthday();Send(fake,"one",SoloAction.Travel,HulkBirthday.Zone);Send(fake,"one",SoloAction.Move,x:986,y:160);Send(fake,"one",SoloAction.Birthday,"claim","birthday-gift-7");
        for(int i=0;i<40;i++)fake.AdvanceIdle(.5,out _,Actors);
        Need(fake.ReadBirthday().props[7].holder=="" && fake.ReadBirthday().props[7].location=="stock" && !fake.ReadBirthday().props[7].opened,"idle fake present releases safely without inventing a book outcome");
        var soloSnapshot=GameWorld.WithBirthday(GameWorld.Create(new[]{"solo"})).Snapshot();
        soloSnapshot.birthday.nextTheft=soloSnapshot.birthday.nextElectro=soloSnapshot.birthday.nextHide=1000000;
        var solo=GameWorld.Restore(soloSnapshot);Send(solo,"solo",SoloAction.Travel,HulkBirthday.Zone);Send(solo,"solo",SoloAction.Move,x:400,y:180);solo.AdvanceIdle(.5,out _,new[]{"solo"});
        Need(Send(solo,"solo",SoloAction.Birthday,"help",target:"birthday-villain-0").Accepted && solo.ReadBirthday().helperActor!="","Hulk requests a walking NPC spider helper");
        for(int i=0;i<12;i++)solo.AdvanceIdle(.5,out _,new[]{"solo"});
        Need(solo.ReadBirthday().villains[0].phase=="trapped" && solo.ReadBirthday().helperActor=="","NPC walks into range and webs the same villain");
        Need(Send(solo,"solo",SoloAction.Birthday,"smash",target:"birthday-villain-0").Accepted,"solo Hulk can finish with the NPC helper");
        for(int i=0;i<5;i++)solo.AdvanceIdle(.5,out _,new[]{"solo"});
        Need(solo.ReadBirthday().troublesStopped==1,"NPC assistance completes one rescue");GameWorld.Validate(solo.Snapshot());
        var pets=seed.Snapshot();for(int i=0;i<2;i++){var p=pets.birthday.props[5+i];p.opened=true;p.content=6+i;p.holder="";p.location="ground";p.x=2000+i*100;p.y=180;p.petAt=-1;}
        var animals=GameWorld.Restore(pets);for(int i=0;i<3;i++)animals.AdvanceIdle(.5,out _,Actors);
        Need(animals.ReadBirthday().props[5].petMoving && animals.ReadBirthday().props[5].x>2020 && animals.ReadBirthday().props[6].x>2120,"the two bounded gift pets explore after release");GameWorld.Validate(animals.Snapshot());
    }
    static void CheckNavigationAndThrows()
    {
        var w=QuietBirthday();foreach(var actor in Actors)Send(w,actor,SoloAction.Travel,HulkBirthday.Zone);
        foreach(var actor in Actors)Need(Send(w,actor,SoloAction.Birthday,"seat",target:"1").Accepted,"four children use different actual roof pegs");
        Need(w.ReadBirthday().members.Select(m=>m.seat).Distinct().Count()==4,"no duplicate seat ownership");
        Need(Send(w,"one",SoloAction.Birthday,"nap").Accepted && w.ReadBirthday().members[0].station=="sleep","source sleep action on a seat");
        Send(w,"one",SoloAction.Travel,"garden");Need(w.ReadBirthday().members.Skip(1).All(m=>m.station=="sit"),"one leaves without removing siblings");
        foreach(var actor in Actors.Skip(1))Need(Send(w,actor,SoloAction.Birthday,"zipline",target:"0").Accepted,"shared zipline accepts independent riders");
        w.AdvanceIdle(.5,out _,Actors);Send(w,"two",SoloAction.Travel,"garden");
        Need(w.ReadBirthday().members.Skip(2).All(m=>m.station=="zipline"),"one zipline departure leaves other rides running");
        for(int i=0;i<20;i++)w.AdvanceIdle(.5,out _,Actors);
        Need(w.ReadBirthday().members.All(m=>m.route<0),"rides finish at source endpoint without leases");GameWorld.Validate(w.Snapshot());
        var t=QuietBirthday();foreach(var actor in Actors){Send(t,actor,SoloAction.Travel,HulkBirthday.Zone);Send(t,actor,SoloAction.Move,x:700,y:160);}
        Need(Send(t,"two",SoloAction.Birthday,"claim","birthday-gift-0").Accepted,"spider holds original present");
        Need(Send(t,"two",SoloAction.Birthday,"throw").Accepted,"spider throws it to Hulk");
        Need(!Send(t,"three",SoloAction.Birthday,"claim","birthday-gift-0").Accepted,"in-flight object cannot be claimed");
        for(int i=0;i<4;i++)t.AdvanceIdle(.5,out _,Actors);
        Need(t.ReadBirthday().props[0].holder=="one","Hulk catches same original object");
        Send(t,"one",SoloAction.Birthday,"drop","birthday-gift-0",x:700,y:160);
        Send(t,"two",SoloAction.Birthday,"claim","birthday-gift-0");Send(t,"two",SoloAction.Birthday,"throw");Send(t,"one",SoloAction.Travel,"garden");
        for(int i=0;i<4;i++)t.AdvanceIdle(.5,out _,Actors);
        Need(t.ReadBirthday().props[0].holder=="" && t.ReadBirthday().props[0].location=="ground","departed receiver safely misses same object");
        Send(t,"one",SoloAction.Travel,HulkBirthday.Zone);Send(t,"one",SoloAction.Move,x:HulkBirthday.BatteryX,y:HulkBirthday.BatteryY);
        Need(Send(t,"one",SoloAction.Birthday,"claim","birthday-battery").Accepted && Send(t,"one",SoloAction.Birthday,"throw").Accepted,"Hulk throws the battery to its tower");
        for(int i=0;i<4;i++)t.AdvanceIdle(.5,out _,Actors);
        Need(t.ReadBirthday().powered && t.ReadBirthday().props.Single(p=>p.kind=="battery").location=="socket","same battery powers tower after landing");
        Send(t,"one",SoloAction.Move,x:700,y:160);Need(Send(t,"one",SoloAction.Birthday,"claim","birthday-gift-0").Accepted,"Hulk reuses the missed gift");
        Need(Send(t,"one",SoloAction.Birthday,"seat",target:"2").Accepted && Send(t,"one",SoloAction.Birthday,"unwrap").Accepted,"seated Hulk opens a present");
        Need(Send(t,"one",SoloAction.Birthday,"station",target:"").Accepted && t.ReadBirthday().giftAt<0 && t.ReadBirthday().members[0].seat<0 && !t.ReadBirthday().props[0].opened && t.ReadBirthday().props[0].location=="held","explicit seat departure cancels unfinished unwrap without changing gift outcome");
        Send(t,"three",SoloAction.Birthday,"seat",target:"0");
        var reopened=GameWorld.Restore(t.Snapshot());Need(reopened.ReadBirthday().members.All(m=>m.seat<0 && m.route<0),"save reopening clears transient seats and rides");GameWorld.Validate(reopened.Snapshot());
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
        Send(world,"three",SoloAction.Move,x:3229,y:191);Send(world,"four",SoloAction.Move,x:3229,y:191);
        Need(Send(world,"three",SoloAction.Birthday,"station",target:"guitar").Accepted,"instrument claim");
        Need(!Send(world,"four",SoloAction.Birthday,"station",target:"guitar").Accepted,"instrument contention");
        Send(world,"four",SoloAction.Move,x:2985,y:249);
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
        Need(upgraded.ReadBirthday().version==9 && upgraded.ReadBirthday().props.Length==27 && upgraded.ReadBirthday().props[0].id=="birthday-gift-0","foundation migration preserves existing props");
        var party=QuietBirthday();
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
        var giftWorld=QuietBirthday();
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
        var replay=QuietBirthday();
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
        var stall=QuietBirthday();
        for(int i=0;i<4;i++){
            Send(stall,Actors[i],SoloAction.Travel,HulkBirthday.Zone);Send(stall,Actors[i],SoloAction.Move,x:HulkBirthday.QueueX[0],y:HulkBirthday.QueueY[0]);
            Need(Send(stall,Actors[i],SoloAction.Birthday,"queue").Accepted,"four family members join one vendor queue");
        }
        stall.AdvanceIdle(.5,out _,Actors);var service=stall.ReadBirthday().vendorAt;
        Need(stall.ReadBirthday().vendorQueue.SequenceEqual(Actors) && stall.ReadBirthday().props.Count(p=>p.location=="counter")==1,"one counter reservation for one shared service");
        Need(!Send(stall,"four",SoloAction.Birthday,"claim",stall.ReadBirthday().vendorItem).Accepted,"queue reservation cannot be stolen");
        var waitingRestore=GameWorld.Restore(stall.Snapshot());
        Need(waitingRestore.ReadBirthday().vendorQueue.Length==0 && waitingRestore.ReadBirthday().props.All(p=>p.location!="counter"),"restore safely cancels transient queue/counter");
        Send(stall,"three",SoloAction.Move,x:4100,y:60);
        Need(stall.ReadBirthday().vendorAt==service && !stall.ReadBirthday().vendorQueue.Contains("three"),"independent departure retains siblings' service progress");
        for(int i=0;i<18;i++)stall.AdvanceIdle(.5,out _,Actors);
        Need(stall.ReadBirthday().vendorCount==3 && stall.ReadBirthday().props.Count(p=>p.kind=="food" && p.holder!="")==3,"three patrons each receive one portion");
        Send(stall,"three",SoloAction.Move,x:HulkBirthday.QueueX[0],y:HulkBirthday.QueueY[0]);Send(stall,"three",SoloAction.Birthday,"queue");
        for(int i=0;i<6;i++)stall.AdvanceIdle(.5,out _,Actors);
        Need(stall.ReadBirthday().vendorCount==4 && stall.ReadBirthday().vendorQueue.Length==0 && stall.ReadBirthday().props.Count(p=>p.kind=="food" && p.holder!="")==4,"returning fourth patron shares the same bounded supply");
        GameWorld.Validate(stall.Snapshot());
        var band=QuietBirthday();
        for(int i=0;i<4;i++){
            Send(band,Actors[i],SoloAction.Travel,HulkBirthday.Zone);
            string station=HulkBirthday.RoleInstrument(HulkBirthday.Roles[i]);
            Send(band,Actors[i],SoloAction.Move,x:HulkBirthday.StageX(station,i),y:HulkBirthday.StageY(station,i));
            Need(Send(band,Actors[i],SoloAction.Birthday,"band").Accepted,"shared band admission");
        }
        band.AdvanceIdle(.5,out _,Actors);var round=band.ReadBirthday().bandRoundAt;
        Need(round>=0 && band.ReadBirthday().bandActors.Length==4,"one four-player band countdown");
        Send(band,"one",SoloAction.Move,x:HulkBirthday.BatteryX,y:HulkBirthday.BatteryY);
        Need(Send(band,"one",SoloAction.Birthday,"claim","birthday-battery").Accepted,"remove shared battery");
        band.AdvanceIdle(.5,out _,Actors);
        Need(!band.ReadBirthday().powered && band.ReadBirthday().bandRoundAt==round,"battery changes arrangement without restarting siblings");
        Need(Send(band,"one",SoloAction.Birthday,"socket").Accepted,"return battery");
        for(int i=0;i<42;i++)band.AdvanceIdle(.5,out _,Actors);
        Need(band.ReadBirthday().bandRounds==1 && band.ReadBirthday().bandRoundAt<0,"one common band completion");
        GameWorld.Validate(band.Snapshot());
        var castle=QuietBirthday();
        foreach(var actor in Actors){Send(castle,actor,SoloAction.Travel,HulkBirthday.Zone);Send(castle,actor,SoloAction.Move,x:1169,y:100);Need(Send(castle,actor,SoloAction.Birthday,"station",target:"castle").Accepted,"four players share one castle");}
        for(int i=0;i<10;i++)castle.AdvanceIdle(.5,out _,Actors);
        Need(castle.ReadBirthday().members.All(m=>m.landings>0 && m.bounceH>=0 && m.bounceH<=5.5),"gravity and bounded floor rebounds");
        Send(castle,"one",SoloAction.Birthday,"castle-drag",x:2.4f,y:4);castle.AdvanceIdle(.5,out _,Actors);
        Need(castle.ReadBirthday().members[0].bounceX>1,"drag applies bounded source-unit velocity");
        Send(castle,"one",SoloAction.Birthday,"castle-release");Send(castle,"one",SoloAction.Birthday,"castle-tap");castle.AdvanceIdle(.5,out _,Actors);
        Need(castle.ReadBirthday().members[0].stunts==1,"tap drives source downward impulse and stunt");
        Send(castle,"one",SoloAction.Move,x:1169,y:35);
        Need(castle.ReadBirthday().members[0].station=="" && castle.ReadBirthday().members.Skip(1).All(m=>m.station=="castle"),"one castle exit preserves siblings");
        Need(GameWorld.Restore(castle.Snapshot()).ReadBirthday().members.All(m=>m.station=="" && !m.bounceDrag),"restore clears only transient castle participation");
        GameWorld.Validate(castle.Snapshot());
        var play=QuietBirthday();
        foreach(var actor in Actors){Send(play,actor,SoloAction.Travel,HulkBirthday.Zone);Send(play,actor,SoloAction.Move,x:4050,y:140);}
        Need(Send(play,"one",SoloAction.Birthday,"claim","birthday-brick-0").Accepted,"building brick pickup");
        Need(Send(play,"one",SoloAction.Birthday,"brick-turn").Accepted && Send(play,"one",SoloAction.Birthday,"brick-color").Accepted,"shared brick rotation and colour");
        Need(Send(play,"one",SoloAction.Birthday,"stack","birthday-brick-1").Accepted,"peg brick onto one shared support");
        Need(!Send(play,"two",SoloAction.Birthday,"claim","birthday-brick-1").Accepted,"occupied support cannot move under a sibling's construction");
        var construction=GameWorld.Restore(play.Snapshot());Need(construction.ReadBirthday().props.Single(p=>p.id=="birthday-brick-0").support=="birthday-brick-1","saved construction retains pegs");
        Need(Send(play,"two",SoloAction.Birthday,"claim","birthday-brick-0").Accepted,"another sibling unpegs the top brick");
        Send(play,"one",SoloAction.Move,x:1850,y:150);Need(Send(play,"one",SoloAction.Birthday,"bubbles").Accepted,"source machine shared switch");play.AdvanceIdle(.5,out _,Actors);
        int bubble=BirthdayPlay.LastBubble(play.ReadBirthday());Need(Send(play,"one",SoloAction.Birthday,"bubble-pop",bubble.ToString()).Accepted,"bubble popped once");
        Need(!Send(play,"two",SoloAction.Birthday,"bubble-pop",bubble.ToString()).Accepted,"competing bubble pop cannot duplicate event");
        GameWorld.Validate(play.Snapshot());
        CheckTrouble();
        CheckNavigationAndThrows();
        var invalid=world.Snapshot();invalid.birthday.props[0].id="invented";
        try{GameWorld.Validate(invalid);throw new Exception("invalid prop identity accepted");}catch(InvalidOperationException){}
        Console.WriteLine("PASS: authority fixtures; migration/JSON, shared ownership, gifts/cake/queue, band/castle/building, all villain roles, independent web departure, battery recovery, fake gift, pets and NPC assistance. Not a four-client runtime test.");
    }
}
