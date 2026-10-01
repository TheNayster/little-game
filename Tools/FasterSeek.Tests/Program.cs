using System;
using System.Linq;
using LittleWeeps.Core;

static class Program
{
    static void Check(bool b,string message){if(!b)throw new Exception(message);}
    static SoloResult Send(SoloWorld w,string actor,SoloAction action,string value="",string target="",float x=0)
    {var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target,x=x,y=50});}
    static void Good(SoloResult r)=>Check(r.Accepted,r.Outcome);
    static void Advance(SoloWorld w,double seconds){while(seconds>1e-9){var step=Math.Min(.05,seconds);w.AdvanceIdle(step,out _);seconds-=step;}}
    static SoloWorld Start(params int[] slots)
    {
        var w=SoloWorld.WithPond(SoloWorld.Create("one","two","three","four"));
        Good(Send(w,"one",SoloAction.Move,x:-3880));Good(Send(w,"one",SoloAction.HideAndSeek,"start"));
        for(var i=0;i<slots.Length;i++)
        {var id=w.ReadPlayers()[i].id;Good(Send(w,id,SoloAction.Move,x:HideAndSeek.CoverX(slots[i],w.Schema)));Good(Send(w,id,SoloAction.HideAndSeek,"hide",slots[i].ToString()));}
        Advance(w,14.9);Check(w.ReadHideAndSeek().phase==HidePhase.Counting,"Hiding window shortened");
        w.AdvanceIdle(.2,out _);Check(w.ReadHideAndSeek().count==0,"Shared hiding window failed");return w;
    }
    static double Finish(SoloWorld w)
    {
        var elapsed=0d;var maxStep=0f;var inspected=0;
        while(w.ReadHideAndSeek().hiders.Any(HideAndSeek.Eligible) && elapsed<30)
        {
            var before=w.ReadHideAndSeek();w.AdvanceIdle(.05,out _);var after=w.ReadHideAndSeek();elapsed+=.05;
            maxStep=Math.Max(maxStep,Math.Abs(after.x-before.x));
            Check(Math.Abs(after.x-before.x)<=HideAndSeek.Speed*.05+.01,"Parent teleported");
            if(after.visited!=before.visited){Check(before.phase==HidePhase.Inspecting,"Cover skipped inspection");Check(after.x==HideAndSeek.CoverX(before.target,w.Schema),"Found from a distance");inspected++;}
        }
        Check(!w.ReadHideAndSeek().hiders.Any(HideAndSeek.Eligible),"Search too slow");
        Check(maxStep>=44.9 && inspected>0,"Parent did not use faster walking/physical inspections");SoloWorld.Validate(w.Snapshot());return elapsed;
    }
    static void Main()
    {
        foreach(var organizerX in new[]{-7040f,4310f,-3880f})
        {
            var w=SoloWorld.WithPond(SoloWorld.Create("one","two","three","four"));
            Good(Send(w,"one",SoloAction.Move,x:organizerX));var positions=w.ReadPlayers().Select(p=>p.x).ToArray();
            Good(Send(w,"one",SoloAction.HideAndSeek,"start"));
            Check(w.ReadHideAndSeek().x==HideAndSeek.StartX && w.ReadPlayers().Select(p=>p.x).SequenceEqual(positions),"Seeker spawned beside organizer or moved children");
            Advance(w,10);Check(w.ReadHideAndSeek().x==HideAndSeek.StartX && w.ReadHideAndSeek().phase==HidePhase.Counting,"Seeker drifted while counting");
            Advance(w,6);Good(Send(w,"two",SoloAction.Move,x:organizerX>0?-7040:4310));Good(Send(w,"two",SoloAction.HideAndSeek,"start"));
            Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Chilli" && w.ReadHideAndSeek().x==HideAndSeek.StartX,"Other parent used player-relative start");
        }
        Console.WriteLine("PASS far-left/far-right organizers, stationary counting, unchanged children and both parents start at the authored Home spot");
        for(var slot=0;slot<HideAndSeek.SlotX.Length;slot++)
        {
            var shared=Start(slot,slot,slot,slot);SoloWorld.Validate(shared.Snapshot());
            Check(shared.ReadHideAndSeek().hiders.Count(h=>h.slot==slot)==4,"Existing cover rejected shared occupants");
            var ticks=0;
            while(shared.ReadHideAndSeek().hiders.All(h=>h.mode!=HiderMode.Found) && ticks++<600)shared.AdvanceIdle(.05,out _);
            Check(shared.ReadHideAndSeek().hiders.All(h=>h.mode==HiderMode.Found) && (shared.ReadHideAndSeek().visited&(1<<slot))!=0,"One inspection did not find the entire shared cover");
        }
        var exit=Start(5,5,5,5);Good(Send(exit,"one",SoloAction.HideAndSeek,"leave"));
        Check(exit.ReadHideAndSeek().hiders.Count(h=>h.mode==HiderMode.Hidden)==3,"Shared-cover exit removed siblings");Finish(exit);
        Check(HideAndSeek.Player(exit.ReadHideAndSeek(),"one").mode==HiderMode.Away && exit.ReadHideAndSeek().hiders.Count(h=>h.mode==HiderMode.Found)==3,"Parent found withdrawn hider or lost siblings");
        Console.WriteLine("PASS all ten covers hold four, one inspection finds the whole group, and independent departure preserves three co-hiders");
        var right=Start(9);var rightSeconds=Finish(right);var mask=right.ReadHideAndSeek().visited;
        foreach(var slot in new[]{3,4,5,7,8,9})Check((mask&(1<<slot))!=0,"Skipped cover on way right: "+slot);
        foreach(var slot in new[]{0,1,2,6})Check((mask&(1<<slot))==0,"Unnecessary left detour: "+slot);
        Check(rightSeconds<18,"Far-right search took too long");Console.WriteLine($"PASS far-right route and physical checks: {rightSeconds:F2}s after hiding window");
        var left=Start(6);var leftSeconds=Finish(left);mask=left.ReadHideAndSeek().visited;
        foreach(var slot in new[]{2,1,0,6})Check((mask&(1<<slot))!=0,"Skipped cover on way left: "+slot);
        Check((mask&((1<<3)|(1<<9)))==0 && leftSeconds<10,"Left search detoured/too slow");Console.WriteLine($"PASS far-left route: {leftSeconds:F2}s");
        var family=Start(2,9,8,7);family.AdvanceIdle(.4,out _);Check(family.ReadHideAndSeek().target==2,"Nearest hider did not guide first target");
        Good(Send(family,"four",SoloAction.HideAndSeek,"leave"));Check(HideAndSeek.Player(family.ReadHideAndSeek(),"two").mode==HiderMode.Hidden,"Departure disturbed sibling");
        var familySeconds=Finish(family);Check(HideAndSeek.Player(family.ReadHideAndSeek(),"four").mode==HiderMode.Away,"Parent found departed player");
        Check(familySeconds<22,"Shared search too slow");family.AdvanceIdle(1,out _);Good(Send(family,"one",SoloAction.HideAndSeek,"start"));
        Check(HideAndSeek.Parent(family.ReadHideAndSeek())=="Chilli" && family.ReadHideAndSeek().count==15,"Parent turn/count changed");
        Console.WriteLine($"PASS four shared hiders, independent departure, nearest target and alternating parent: {familySeconds:F2}s");
    }
}
