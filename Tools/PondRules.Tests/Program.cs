using System;
using System.Linq;
using LittleWeeps.Core;

static class Program
{
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    static SoloResult Send(SoloWorld w,string actor,string op,string request=null)
    {var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=request??Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=SoloAction.Pond,value=op,x=PondFishing.X,y=100});}
    static void Tick(SoloWorld w,double seconds){for(var t=0.0;t<seconds;t+=.05)w.AdvanceIdle(.05,out _,w.ReadPlayers().Select(p=>p.id).ToArray());}
    static PondRod Rod(SoloWorld w,string id)=>w.ReadPond().rods.Single(r=>r.actor==id);
    static void CreekChecks()
    {
        var old=SoloWorld.WithCreekBoats(SoloWorld.Create("one","two","three","four"));
        Require(Send(old,"one","start-feeding").Accepted,"Home setup");
        var before=old.Snapshot();var w=SoloWorld.WithCreekFishing(old);var s=w.Snapshot();
        Require(s.schema==38 && s.worldId==before.worldId && s.pond.nextFood==before.pond.nextFood && s.pond.rods[0].mode==PondMode.Feeding && s.creekBoats.boats.Length==4,"Creek upgrade lost existing activity");
        Require(s.creekFishing.fish.Length==20 && CreekFishing.RadiusX>PondFishing.RadiusX*2,"Creek isn't expanded");
        SoloResult Creek(string actor,string op,float x=3600,float y=100){var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=SoloAction.CreekFishing,value=op,x=x,y=y});}
        PondRod R(string actor)=>w.ReadCreekFishing().rods.Single(r=>r.actor==actor);
        foreach(var p in w.ReadPlayers()){Require(Creek(p.id,"start-fishing").Accepted,"Creek start");Require(w.ReadPlayer(p.id).zone=="creek" && Math.Abs(w.ReadPlayer(p.id).x-CreekFishing.BankX(R(p.id).slot))<1,"Bank placement");}
        Require(w.ReadPond().rods.All(r=>r.mode==PondMode.None),"Travel retained backyard lease");
        Require(Creek("one","cast",4250,100).Accepted,"Wide creek cast rejected");Require(!Creek("one","cast",3600,250).Accepted,"Dry-bank cast accepted");
        Tick(w,11.2);Require(w.ReadCreekFishing().rods.All(r=>r.cast==PondCast.Bite),"Creek bites failed");
        foreach(var p in w.ReadPlayers())Require(Creek(p.id,"reel").Accepted,"Creek reel");
        Require(w.ReadCreekFishing().rods.Select(r=>r.fish).Distinct().Count()==4,"Duplicated creek catch");
        Require(Creek("one","release").Accepted && Creek("one","start-feeding").Accepted,"Creek release/feed");
        for(var i=0;i<15;i++)Require(Creek("one","feed").Accepted,"Bounded creek feeding rejected");
        Require(w.ReadCreekFishing().food.Length<=8 && R("two").cast==PondCast.Caught,"Food growth or sibling catch lost");
        var family=new FamilySession(w);for(ulong i=1;i<=4;i++)Require(family.Attach(i,new[]{"one","two","three","four"}[i-1],out _),"Attach creek");
        family.Detach(4);Require(R("four").mode==PondMode.None && R("three").cast==PondCast.Caught,"Departure reset creek");
        var saved=w.Snapshot();var restored=SoloWorld.Restore(saved);SoloWorld.Validate(restored.Snapshot());
        Require(restored.ReadCreekFishing().fish.Length==20 && restored.ReadCreekFishing().rods.All(r=>r.mode==PondMode.None) && saved.creekFishing.rods[2].cast==PondCast.Caught,"Creek reopen or snapshot mutated");
        Require(Send(w,"two","start-fishing").Accepted && R("two").mode==PondMode.None && R("three").cast==PondCast.Caught,"Home travel disturbed creek sibling");
        var bad=w.Snapshot();bad.creekFishing.fish[0].x=9999;var rejected=false;try{SoloWorld.Validate(bad);}catch(InvalidOperationException){rejected=true;}Require(rejected,"Invalid creek geometry accepted");
        Console.WriteLine("PASS creek upgrade, 20 shared fish, wide casts, four exclusive catches, feeding, departure, home separation and reopen");
    }
    static void Main()
    {
        var old=SoloWorld.WithOutfits(SoloWorld.Create("one","two","three","four"));var before=old.Snapshot();
        var w=SoloWorld.WithPond(old);var s=w.Snapshot();
        Require(s.schema==36 && s.worldId==before.worldId && s.toys.Select(t=>t.id).SequenceEqual(before.toys.Select(t=>t.id)),"Additive migration lost existing objects");
        Require(s.players.Select(p=>p.avatar).SequenceEqual(before.players.Select(p=>p.avatar)),"Migration changed avatars");
        SoloWorld.Validate(s);Console.WriteLine("PASS additive migration preserves world and existing objects");
        var picnic=before;var hidden=picnic.hideAndSeek.hiders[0];hidden.slot=8;hidden.mode=HiderMode.Hidden;
        picnic.players[0].x=2910;picnic.players[0].y=HideAndSeek.HiddenY(8);SoloWorld.Validate(picnic);
        var moved=SoloWorld.WithPond(SoloWorld.Restore(picnic));
        Require(HideAndSeek.CoverX(8,before.schema)==2910 && HideAndSeek.CoverX(8,moved.Schema)==3150,"Picnic anchor migration lost legacy position");
        var migrated=SoloWorld.WithPond(old).Snapshot();SoloWorld.Validate(migrated);
        Console.WriteLine("PASS legacy picnic checkpoint reopens and new layout uses the moved cover");
        foreach(var id in new[]{"one","two","three","four"}){
            Require(Send(w,id,"start-fishing").Accepted,"Start rejected");var p=w.ReadPlayer(id);var r=Rod(w,id);
            Require(p.zone=="garden" && Math.Abs(p.x-PondFishing.BankX(r.slot))<1 && r.cast==PondCast.Waiting,"Menu start failed to place and cast");
        }
        Require(w.ReadPond().rods.Select(r=>r.slot).Distinct().Count()==4,"Duplicate bank positions");
        var waits=w.ReadPond().rods.Select(r=>r.due).ToArray();Require(waits.All(t=>t>=4 && t<=10) && waits.Distinct().Count()>1,"Bites aren't varied and bounded");
        Tick(w,11.2);s=w.Snapshot();SoloWorld.Validate(s);
        Require(s.pond.rods.All(r=>r.cast==PondCast.Bite) && s.pond.rods.Select(r=>r.fish).Distinct().Count()==4,"Four-player bites disagree or duplicate a fish");
        foreach(var id in new[]{"one","two","three","four"})Require(Send(w,id,"reel").Accepted,"Easy reel rejected");
        Require(w.ReadPond().rods.All(r=>r.cast==PondCast.Caught),"Shared catches missing");Console.WriteLine("PASS four shared casts, varied bounded bites, exclusive catches and easy reel");
        var idOne=Rod(w,"one").fish;Require(Send(w,"one","release").Accepted && Rod(w,"one").fish==-1,"Release failed");
        Require(w.ReadPond().fish.Single(f=>f.id==idOne)!=null && Rod(w,"two").cast==PondCast.Caught,"Release lost fish or disturbed siblings");
        Require(Send(w,"one","start-feeding").Accepted,"Feeding switch failed");
        var command=new SoloCommand{actor="one",requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone="garden",visit=w.ReadPlayer("one").visit,action=SoloAction.Pond,value="feed",x=PondFishing.X,y=100};
        Require(w.Apply(command).Accepted,"Feeding rejected");var count=w.ReadPond().food.Length;
        Require(w.Apply(command).Duplicate && w.ReadPond().food.Length==count,"Replayed feed made more food");
        Require(w.ReadPond().food.All(f=>!w.ReadPond().rods.Any(r=>r.fish>=0 && r.fish==f.fish)),"Caught fish also feeding");
        Tick(w,3);Require(w.ReadPond().food.Length==0 && Rod(w,"two").cast==PondCast.Caught,"Food not eaten or sibling catch lost");Console.WriteLine("PASS feeding coexists with fishing, consumes once and clears pellets");
        var family=new FamilySession(w);for(ulong i=1;i<=4;i++)Require(family.Attach(i,new[]{"one","two","three","four"}[i-1],out _),"Attach failed");
        Require(family.Detach(4) && Rod(w,"four").mode==PondMode.None && Rod(w,"two").cast==PondCast.Caught,"Departure disturbed the group");
        var p2=w.ReadPlayer("two");Require(w.Apply(new SoloCommand{actor="two",requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p2.zone,visit=p2.visit,action=SoloAction.Move,x=1000,y=100}).Accepted,"Move failed");
        Require(Rod(w,"two").mode==PondMode.None && Rod(w,"three").cast==PondCast.Caught,"Walking away failed to release independently");
        Console.WriteLine("PASS independent disconnect and walking away return fish without stopping siblings");
        var checkpoint=w.Snapshot();var restored=SoloWorld.Restore(checkpoint);Require(restored.ReadPond().rods.All(r=>r.mode==PondMode.None) && restored.ReadPond().food.Length==0 && restored.ReadPond().fish.Length==8,"Restore retained temporary catch/food");
        Require(checkpoint.pond.rods.Single(r=>r.actor=="three").cast==PondCast.Caught,"Restore mutated input checkpoint");
        s=w.Snapshot();s.pond.rods.Single(r=>r.actor=="one").slot=Rod(w,"three").slot;
        var invalid=false;try{SoloWorld.Validate(s);}catch(InvalidOperationException){invalid=true;}Require(invalid,"Corrupt shared leases accepted");
        Console.WriteLine("PASS restore releases temporary leases and validation rejects duplicate pond slots");
        CreekChecks();
        Console.WriteLine("ALL PASS");
    }
}
