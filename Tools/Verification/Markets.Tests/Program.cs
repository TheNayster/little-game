using System;
using System.Linq;
using LittleWeeps.Core;

static class MarketsChecks
{
    static readonly string[] Actors={"one","two","three","four"};
    static SoloPlayer[] Players()=>Actors.Select(a=>new SoloPlayer{id=a,zone=MarketsRules.Zone}).ToArray();
    static void Need(bool test,string why){if(!test)throw new Exception(why);}
    static void Main()
    {
        var players=Players();var state=MarketsRules.Create(Actors);
        void Act(string actor,string op,string item="",string target="",string value="")=>Need(MarketsRules.Apply(state,actor,op,item,target,value,players)==null,op);
        void Tick(double dt){MarketsRules.Advance(state,dt,players);MarketsRules.Validate(state,players);}
        MarketsStation Station(string id)=>state.stations.Single(t=>t.id==id);
        void Reject(string actor,string op,string item="",string target="",string value="")=>Need(MarketsRules.Apply(state,actor,op,item,target,value,players)!=null,"must reject "+op);
        MarketsRules.Validate(state,players);
        foreach(var actor in Actors)Act(actor,"join",target:"smoothie");
        for(int i=0;i<4;i++){Act(Actors[i],"ingredient",MarketsRules.Fruit[i]);Tick(.7);}
        Need(Station("smoothie").parts.Length==4,"all four contributed to one smoothie");
        Act("one","mix");Need(state.items.Length==0&&Station("smoothie").completed==0&&Station("smoothie").pendingKind=="smoothie","mix starts without premature output");
        Tick(1);Need(state.items.Length==0&&Station("smoothie").parts.Length==4,"ingredients retained until finish");
        Reject("one","claim","market-item-1",value:"0");Tick(2.1);Need(Station("smoothie").completed==1&&state.items.Length==1,"one common result after finish");
        Tick(.1);Need(Station("smoothie").completed==1&&state.items.Length==1,"finish produces and counts once");
        var smoothie=state.items.Single();Act("one","claim",smoothie.id,value:"0");
        Reject("two","claim",smoothie.id,value:"0");Need(state.items.Single().holder=="one","contested claim retained owner");
        Act("one","give",smoothie.id,"two","1");Reject("one","consume",smoothie.id,value:"1");
        Act("two","consume",smoothie.id,value:"2");Need(state.items.Length==0,"consume once");
        Act("one","join",target:"flowers");Act("two","join",target:"flowers");
        Act("one","flower","coral");Tick(.7);Act("two","flower","red");Tick(.7);Act("one","bouquet");Tick(1.1);
        Need(state.items.Single().choice=="coral+red","shared flower stack retained both choices");
        foreach(string choice in MarketsRules.Cakes){Act("three","cake",choice);Tick(1.1);}
        Need(Station("cakes").completed==3,"all cupcake variants");
        foreach(var actor in Actors)Act(actor,"join",target:"bubbles");
        for(int i=0;i<4;i++){Act(Actors[i],"bubble",(i+1).ToString());Tick(.6);}
        var bubble=state.bubbles[0].id.ToString();Act("four","pop",bubble);Reject("one","pop",bubble);Need(state.popped==1,"shared pop once");
        foreach(var actor in Actors)Act(actor,"join",target:"music");
        Act("one","music-start");for(int i=0;i<12;i++)Act(Actors[i%4],"beat",(i%3).ToString());
        Need(state.layers==5&&state.members.All(m=>m.pose=="dance"),"shared layered dance");
        for(int i=0;i<3;i++){Act("one","song",i.ToString());Act("two","guitar");}
        Act("two","music-stop");Need(state.playing&&Station("music").queue.Length==3,"independent music exit");
        Act("one","join",target:"buttermilk");Act("one","care","food","buttermilk","apple");
        Need(state.ponies[0].needs==0&&state.ponies[0].rounds==0&&Station("buttermilk").completed==0,"care waits for actual completion");
        Tick(.3);Act("one","leave");Tick(5);Need(state.ponies[0].needs==0&&Station("buttermilk").completed==0,"departure cancels uncontacted care");
        Act("one","join",target:"buttermilk");Act("one","care","wash","buttermilk");MarketsRules.Restore(state);Tick(4);Need(state.ponies[0].washed==0,"load cancels unfinished wash");
        foreach(var pony in new[]{"buttermilk","biscuit"}){
            foreach(var actor in Actors)Act(actor,"join",target:pony);
            for(int i=0;i<4;i++){Act(Actors[i],"care",MarketsRules.Care[i],pony,i==0?"apple":"");Tick(4.1);}
            Need(state.ponies.Single(p=>p.id==pony).needs==15,"shared four-role pony care");
            Act("one","care","wash",pony);Tick(3.1);
            // Leave queue helpers without removing their durable contributions.
            Act("three","leave");Act("four","leave");
            for(int i=0;i<6;i++){var next=Station(pony).queue[0];Act(next,"makeover","mane",pony,i.ToString());Tick(1.1);}
            var p=state.ponies.Single(x=>x.id==pony);Need(p.mane==5&&p.tail==5&&p.washed==1&&p.rounds==1,"linked unicorn and care state");
            string leader=Station(pony).queue[0];Act(leader,"pony-reset",target:pony);Tick(1.1);
            Need(state.ponies.Single(x=>x.id==pony).needs==0&&state.ponies.Single(x=>x.id==pony).rounds==1,"deliberate shared replay retains completed rounds");
            for(int i=0;i<4;i++){leader=Station(pony).queue[0];Act(leader,"care",MarketsRules.Care[i],pony,i==0?"carrot":"");Tick(4.1);}
        }
        foreach(var actor in Actors)Act(actor,"join",target:"icecream");
        for(int order=0;order<3;order++){
            var station=Station("icecream");Act(station.queue[0],"ice-select",MarketsRules.IceCreamNames[order],value:station.revision.ToString());
            for(int i=0;i<MarketsRules.Recipes[order].Length;i++){
                station=Station("icecream");string actor=station.queue[0],revision=station.revision.ToString();
                Reject(actor,"ice-add","wrong",value:revision);Need(Station("icecream").revision==int.Parse(revision),"reject does not increment revision");
                Act(actor,"ice-add",MarketsRules.Recipes[order][i],value:revision);Tick(.8);
                Reject(actor,"ice-add",MarketsRules.Recipes[order][0],value:revision);
            }
        }
        Need(Station("icecream").completed==3&&state.items.Count(i=>i.kind=="icecream")==3,"all exact source blueprint sequences");
        // The recipe queue stays intact while all siblings build one durable
        // creation. Authority revisions serialize conflicting slot requests.
        for(int i=0;i<4;i++)Act(Actors[i],"build-add",MarketsRules.Bricks[i],i.ToString(),Station("icecream").revision.ToString());
        Need(state.bricks.Length==4&&state.bricks.Select(b=>b.creator).Distinct().Count()==4,"four creators share one board");
        string boardRevision=Station("icecream").revision.ToString();
        Reject("two","build-add",MarketsRules.Bricks[4],"0",boardRevision);
        Need(Station("icecream").revision.ToString()==boardRevision&&state.bricks.Length==4,"contested slot rejected atomically");
        Act("one","build-add",MarketsRules.Bricks[4],"4",boardRevision);
        Reject("two","build-add",MarketsRules.Bricks[5],"5",boardRevision);
        var own=state.bricks[0];string ownId=own.id;
        Reject("two","build-move",ownId,"7","0");
        Reject("two","build-turn",ownId,"1","0");
        Reject("two","build-remove",ownId,value:"0");
        Act("one","build-move",ownId,"15","0");
        Reject("one","build-turn",ownId,"1","0");
        Reject("one","build-move",ownId,"1","1");
        Act("one","build-turn",ownId,"3","1");
        Need(state.bricks.Single(b=>b.id==ownId).slot==15&&state.bricks.Single(b=>b.id==ownId).turn==3&&state.bricks.Single(b=>b.id==ownId).revision==2,"creator move and quarter-turn revisions");
        Reject("one","build-clear",value:boardRevision);
        Act("one","build-clear",value:Station("icecream").revision.ToString());
        Need(state.bricks.Length==3&&state.bricks.All(b=>b.creator!="one"),"clear preserves siblings' work");
        for(int i=0;i<MarketsRules.Bricks.Length;i++){
            string actor=Actors[i%4];int slot=Enumerable.Range(0,16).First(n=>state.bricks.All(b=>b.slot!=n));
            if(state.bricks.Length==16)break;
            Act(actor,"build-add",MarketsRules.Bricks[i],slot.ToString(),Station("icecream").revision.ToString());
            var added=state.bricks.Last();Act(actor,"build-turn",added.id,(i%4).ToString(),"0");
            Act(actor,"build-remove",added.id,value:"1");
        }
        // Keep one of every recovered source variant as durable reopen evidence.
        foreach(string actor in Actors){if(state.bricks.Any(b=>b.creator==actor))Act(actor,"build-clear",value:Station("icecream").revision.ToString());}
        for(int i=0;i<MarketsRules.Bricks.Length;i++)Act(Actors[i%4],"build-add",MarketsRules.Bricks[i],i.ToString(),Station("icecream").revision.ToString());
        Act("one","build-add",MarketsRules.Bricks[0],"14",Station("icecream").revision.ToString());
        Act("two","build-add",MarketsRules.Bricks[1],"15",Station("icecream").revision.ToString());
        Reject("three","build-add",MarketsRules.Bricks[2],"16",Station("icecream").revision.ToString());
        Need(state.bricks.Length==16&&state.bricks.Select(b=>b.variant).Distinct().Count()==14,"bounded board and all fourteen source variants");
        Act("one","sign");Act("two","umbrella");Act("three","longdog");Act("four","photo");
        var durable=state.Copy();var claimed=state.items.First();Act("one","claim",claimed.id,value:claimed.revision.ToString());
        MarketsRules.Release(state,"one",players,dropItems:false);Need(state.items.Single(i=>i.id==claimed.id).holder=="one","walking releases station but retains product");
        players[0].zone="garden";MarketsRules.Release(state,"one",players);Tick(.1);
        Need(state.items.All(i=>i.holder!="one")&&Station("icecream").queue.Length==3&&Station("icecream").completed==3,"departure preserves shared creations and siblings");
        var restored=state.Copy();MarketsRules.Restore(restored);MarketsRules.Validate(restored,players);
        Need(restored.members.All(m=>m.station=="")&&restored.items.All(i=>i.holder=="")&&restored.stations.All(t=>t.queue.Length==0)&&restored.stations.Single(t=>t.id=="icecream").completed==3&&restored.ponies.All(p=>p.needs==15),"restore preserves durable results and frees claims");
        Need(restored.bricks.Length==16&&restored.bricks.Count(b=>b.creator=="one")==5,"departed creator's bricks survive restore");
        var jsonOptions=new System.Text.Json.JsonSerializerOptions{IncludeFields=true};
        string saved=System.Text.Json.JsonSerializer.Serialize(restored,jsonOptions);
        var reopened=System.Text.Json.JsonSerializer.Deserialize<MarketsState>(saved,jsonOptions);
        MarketsRules.Restore(reopened);MarketsRules.Validate(reopened,players);
        Need(reopened.bricks.Length==16&&reopened.bricks.Select(b=>b.id).SequenceEqual(state.bricks.Select(b=>b.id)),"save/reopen retains stable brick IDs");
        reopened.bricks[0].turn=2;Need(state.bricks[0].turn==0,"reopen independent brick objects");
        restored.bricks[0].slot=15;Need(state.bricks[0].slot==0,"deep copy protects brick state");
        restored.ponies[0].mane=0;Need(state.ponies[0].mane==5,"deep copy protects live state");
        var bad=state.Copy();bad.clock=double.NaN;try{MarketsRules.Validate(bad,players);throw new Exception("NaN accepted");}catch(InvalidOperationException){}
        bad=state.Copy();bad.items[0].holder="unknown";try{MarketsRules.Validate(bad,players);throw new Exception("unknown holder accepted");}catch(InvalidOperationException){}
        bad=state.Copy();bad.stations[0].parts=new[]{"not-fruit"};try{MarketsRules.Validate(bad,players);throw new Exception("invalid ingredient accepted");}catch(InvalidOperationException){}
        bad=state.Copy();bad.bricks[0].slot=bad.bricks[1].slot;try{MarketsRules.Validate(bad,players);throw new Exception("duplicate brick slot accepted");}catch(InvalidOperationException){}
        bad=state.Copy();bad.bricks[0].variant="unrecovered";try{MarketsRules.Validate(bad,players);throw new Exception("invalid brick variant accepted");}catch(InvalidOperationException){}
        bad=state.Copy();bad.bricks[0].creator="unknown";try{MarketsRules.Validate(bad,players);throw new Exception("invalid brick creator accepted");}catch(InvalidOperationException){}
        bad=state.Copy();bad.bricks[0].turn=4;try{MarketsRules.Validate(bad,players);throw new Exception("invalid brick rotation accepted");}catch(InvalidOperationException){}
        bad=state.Copy();bad.bricks[0].id="market-brick-999999";try{MarketsRules.Validate(bad,players);throw new Exception("invalid brick ID accepted");}catch(InvalidOperationException){}
        Need(!MarketsRules.Advance(state,double.NaN,players)&&!MarketsRules.Advance(state,-1,players),"reject invalid tick");
        Need(MarketsRules.Normalize(null,Actors).members.Length==4,"absent state migration");
        var pending=MarketsRules.Create(Actors);var pendingPlayers=Players();
        void PendingAct(string a,string op,string item="",string target="",string value="")=>Need(MarketsRules.Apply(pending,a,op,item,target,value,pendingPlayers)==null,"pending "+op);
        void PendingTick(double dt){MarketsRules.Advance(pending,dt,pendingPlayers);MarketsRules.Validate(pending,pendingPlayers);}
        PendingAct("one","ingredient","apple");PendingTick(.7);PendingAct("one","mix");
        var pendingSave=System.Text.Json.JsonSerializer.Deserialize<MarketsState>(System.Text.Json.JsonSerializer.Serialize(pending,jsonOptions),jsonOptions);
        MarketsRules.Validate(pendingSave,pendingPlayers);MarketsRules.Restore(pendingSave);MarketsRules.Validate(pendingSave,pendingPlayers);
        Need(pendingSave.items.Length==0&&pendingSave.stations[0].parts.SequenceEqual(new[]{"apple"})&&pendingSave.stations[0].pendingKind=="","reopen cancels unfinished result while retaining ingredients");
        pendingPlayers[0].zone="garden";PendingTick(3.1);
        Need(pending.items.Length==0&&pending.stations[0].completed==0&&pending.stations[0].parts.SequenceEqual(new[]{"apple"}),"departure cancels own pending mix without ingredient loss");
        PendingAct("two","mix");PendingTick(3.1);Need(pending.items.Length==1&&pending.stations[0].completed==1,"sibling can complete retained shared inputs");
        PendingAct("two","cake","pink");
        Need(MarketsRules.Apply(pending,"two","claim",pending.items[0].id,"","0",pendingPlayers)=="finish-your-action","working hands cannot claim another item");
        PendingTick(1.1);Need(pending.items.Length==2&&pending.stations.Single(t=>t.id=="cakes").completed==1,"cake delivered on finish");
        for(int i=0;i<MarketsRules.Recipes[0].Length;i++){
            PendingAct("three","ice-add",MarketsRules.Recipes[0][i],value:pending.stations.Last().revision.ToString());
            if(i+1<MarketsRules.Recipes[0].Length)PendingTick(.8);
        }
        Need(pending.items.Length==2&&pending.stations.Last().pendingKind=="icecream","recipe final contact deferred");
        PendingAct("three","leave");Need(pending.stations.Last().parts.Length==MarketsRules.Recipes[0].Length-1,"cancelled final recipe step retains earlier placements");
        PendingAct("four","ice-add",MarketsRules.Recipes[0].Last(),value:pending.stations.Last().revision.ToString());PendingTick(.8);
        Need(pending.items.Length==3&&pending.stations.Last().completed==1,"sibling finishes cancelled final recipe step once");
#if !MARKETS_ISOLATED
        IntegratedChecks();
#endif
        Console.WriteLine("PASS Markets shared stalls, revision claims/handoffs, bubbles, layered dance/exit, two cooperative pony loops/makeover, three exact ice cream blueprints, fourteen-variant four-player free building, exclusive revision edits/contested slots, departure, save/reopen, deep copy and malformed state");
    }
#if !MARKETS_ISOLATED
    static void IntegratedChecks()
    {
        var options=new System.Text.Json.JsonSerializerOptions{IncludeFields=true};
        string Json(object obj)=>System.Text.Json.JsonSerializer.Serialize(obj,options);
        var initial=GameWorld.Create(Actors);var before=initial.Snapshot();
        var world=GameWorld.WithDinosaurWorld(initial);var upgraded=world.Snapshot();
        Need(upgraded.schema==WorldLayout.Schema&&upgraded.markets.members.Select(m=>m.actor).SequenceEqual(Actors),"current upgrade initializes Markets");
        Need(upgraded.worldId==before.worldId&&Json(upgraded.players)==Json(before.players)&&Json(upgraded.toys.Take(before.toys.Length).ToArray())==Json(before.toys),"initial upgrade preserves identity, actors and existing toys");
        var old=world.Snapshot();old.schema=GameWorld.MarketsSchema-1;old.markets=null;
        var legacy=GameWorld.Restore(old);old=legacy.Snapshot();
        var oldWorlds=Json(new{old.spidey,old.birthday,old.dinosaurWorld,old.zoo,old.sandpit,old.kingdom,old.daycare,old.toys,old.players});
        world=GameWorld.WithDinosaurWorld(legacy);var current=world.Snapshot();
        Need(Json(new{current.spidey,current.birthday,current.dinosaurWorld,current.zoo,current.sandpit,current.kingdom,current.daycare,current.toys,current.players})==oldWorlds,"Markets migration preserves existing world modules");
        var idleWorld=GameWorld.Restore(world.Snapshot());
        var idlePlayer=idleWorld.ReadPlayer(Actors[0]);
        var enter=idleWorld.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),expectedRevision=idleWorld.Revision,actor=idlePlayer.id,zone=idlePlayer.zone,visit=idlePlayer.visit,action=SoloAction.Travel,value="markets"});
        Need(enter.Accepted,"idle Markets fixture arrival");long idleRevision=idleWorld.Revision;
        idleWorld.AdvanceIdle(.05,out var idleVisible,new[]{Actors[0]});
        Need(!idleVisible&&idleWorld.Revision==idleRevision&&idleWorld.AmbientChanged,"Markets animation clock uses ambient path without invalidating commands");
        var family=new FamilySession(world);
        for(int i=0;i<4;i++)Need(family.Attach((ulong)i+1,Actors[i],out _),"integrated four-client attach");
        SoloCommand Command(int actor,SoloAction action,string value="",string item="",string target="",float x=0,float y=0){
            var p=world.ReadPlayer(Actors[actor]);
            return new SoloCommand{requestId=Guid.NewGuid().ToString("N"),expectedRevision=world.Revision,actor=p.id,zone=p.zone,visit=p.visit,action=action,value=value,item=item,target=target,x=x,y=y};
        }
        SoloResult Send(int actor,SoloCommand command){var result=family.Submit((ulong)actor+1,command);GameWorld.Validate(world.Snapshot());return result;}
        void Act(int actor,SoloAction action,string value="",string item="",string target="",float x=0,float y=0){var result=Send(actor,Command(actor,action,value,item,target,x,y));Need(result.Accepted,"integrated "+value+": "+result.Outcome);}
        void Tick(int count){for(int i=0;i<count;i++){world.AdvanceIdle(.25,out _,Actors);GameWorld.Validate(world.Snapshot());}}
        for(int i=0;i<4;i++)Act(i,SoloAction.Travel,MarketsRules.Zone);
        Act(0,SoloAction.Move,x:4200,y:100);
        Need(Send(0,Command(0,SoloAction.Markets,"ingredient",MarketsRules.Fruit[0])).Outcome=="come-closer","Markets command enforces station proximity");
        Act(0,SoloAction.Move,x:550,y:100);
        var add=Command(0,SoloAction.Markets,"ingredient",MarketsRules.Fruit[0]);Need(Send(0,add).Accepted,"ingredient envelope routed");
        string once=Json(world.Snapshot());var duplicate=Send(0,add);
        Need(duplicate.Accepted&&duplicate.Duplicate&&Json(world.Snapshot())==once&&world.ReadMarkets().stations[0].parts.Length==1,"duplicate receipt does not repeat ingredient");
        Tick(3);Act(0,SoloAction.Markets,"mix");
        Need(world.ReadMarkets().items.Length==0&&world.ReadMarkets().stations[0].pendingKind=="smoothie","integrated pending output");
        var pending=world.Snapshot();var reopened=GameWorld.Restore(System.Text.Json.JsonSerializer.Deserialize<SoloSnapshot>(Json(pending),options));
        Need(reopened.ReadMarkets().items.Length==0&&reopened.ReadMarkets().stations[0].parts.SequenceEqual(new[]{"apple"})&&reopened.ReadMarkets().stations[0].pendingKind=="","full snapshot reopen cancels pending output and retains shared inputs");
        Tick(13);var product=world.ReadMarkets().items.Single();Act(0,SoloAction.Markets,"claim:"+product.revision,product.id);
        Act(0,SoloAction.Travel,"garden");
        Need(world.ReadMarkets().items.Single().holder==""&&world.ReadPlayer("two").zone==MarketsRules.Zone,"travel releases held product without changing sibling area");
        Act(1,SoloAction.Move,x:550,y:100);product=world.ReadMarkets().items.Single();Act(1,SoloAction.Markets,"claim:"+product.revision,product.id);
        for(int i=2;i<4;i++){
            Act(i,SoloAction.Move,x:4200,y:100);
            var revision=world.ReadMarkets().stations.Last().revision;
            Act(i,SoloAction.Markets,"build-add:"+revision,MarketsRules.Bricks[i],i.ToString());
        }
        var checkpoint=world.Snapshot();var read=world.ReadMarkets();checkpoint.markets.bricks[0].slot=15;checkpoint.markets.stations[0].parts=new[]{"banana"};read.bricks[0].turn=3;
        Need(world.ReadMarkets().bricks[0].slot==2&&world.ReadMarkets().bricks[0].turn==0&&world.ReadMarkets().stations[0].parts.Length==0,"snapshot and read APIs deep-copy Markets arrays and objects");
        var bad=world.Snapshot();bad.markets=null;bool rejected=false;try{GameWorld.Restore(bad);}catch(InvalidOperationException){rejected=true;}Need(rejected,"modern missing Markets rejected");
        bad=world.Snapshot();bad.markets.bricks[0].creator="unknown";rejected=false;try{GameWorld.Restore(bad);}catch(InvalidOperationException){rejected=true;}Need(rejected,"modern malformed Markets rejected");
        Need(family.Detach(2),"integrated detach");
        Need(world.ReadMarkets().items.Single().holder==""&&world.ReadMarkets().bricks.Length==2,"detach releases only held product and preserves sibling creations");
        var own=world.ReadMarkets().bricks.Single(b=>b.creator=="three");Act(2,SoloAction.Markets,"build-turn:"+own.revision,own.id,"2");
        Need(world.ReadMarkets().bricks.Single(b=>b.id==own.id).turn==2&&world.ReadPlayer("four").zone==MarketsRules.Zone,"siblings continue after independent travel and detach");
        var saved=GameWorld.Restore(System.Text.Json.JsonSerializer.Deserialize<SoloSnapshot>(Json(world.Snapshot()),options));GameWorld.Validate(saved.Snapshot());
        Need(saved.ReadMarkets().items.Length==1&&saved.ReadMarkets().bricks.Length==2&&saved.ReadMarkets().items.All(i=>i.holder==""),"full durable snapshot reopen retains finished products and shared creation");
        Console.WriteLine("PASS integrated GameWorld Markets migration/preservation, Travel/Markets proximity and op:value routing, duplicate receipt, deep copy, strict modern state, pending restore, independent travel/detach and durable reopen");
    }
#endif
}
