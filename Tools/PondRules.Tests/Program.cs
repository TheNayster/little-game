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
        Console.WriteLine("ALL PASS");
    }
}
