using System;
using System.Linq;
using LittleWeeps.Core;
using LittleWeeps.Client;

static class Program
{
    static void Require(bool value,string text){if(!value)throw new Exception(text);}
    static void Main()
    {
        HidingChecks();
        // Actual pond routes last 3-8 seconds; full snapshots publish on events,
        // not on every simulation/render frame. Compare both clocks on one route.
        var fish=new PondFish{fromX=-250,x=250,fromY=-20,y=20,started=0,duration=8};
        var clock=new BoatVisualClock();double old=0,last=0,sample=0;int frozen=0;
        clock.Sample("home",0,0,true);
        for(var frame=1;frame<=420;frame++){
            var now=frame/60.0;
            // Irregular full snapshots, including a three-second quiet interval.
            if(frame==61 || frame==181 || frame==363)sample=now;
            var oldBefore=old;old=Math.Max(sample,Math.Min(sample+.12,old+1.0/60));
            var visual=clock.Sample("home",sample,now,true);
            var before=PondFishing.Point(fish,last);var after=PondFishing.Point(fish,visual);
            Require(visual>last && visual-last<=1.1/60+1e-9,"Continuous swim froze, reversed or jumped");
            Require(after.X>before.X && PondFishing.Target(after.X,after.Y),"Fish stopped or left water mid-route");
            Require(clock.Sample("home",sample,now,true)==visual,"Second render advanced the fish twice");
            if(old==oldBefore)frozen++;
            last=visual;
        }
        Require(frozen>300,"Old cap did not reproduce the observed long pauses");
        var previous=clock.Sample("home",7,7.01,true);
        var corrected=clock.Sample("home",7.4,7.02,true);
        Require(corrected>previous && corrected-previous<=.0110001,"New receipt snapped fish time");
        Require(clock.Sample("home",.5,7.03,true)==.5,"Authority restart was not reset");
        Require(clock.Sample("other",80,7.04,true)==80,"World switch retained old time");
        Require(clock.Sample("other",80,8,false)==80,"Paused/solo clock predicted shared movement");
        Require(clock.Sample("other",80.1,8.1,false)==80.1,"Solo simulation was not followed exactly");
        clock.Reset();Require(clock.Sample("home",4,10,true)==4,"Reopen retained time");
        Console.WriteLine($"PASS old clock reproduces {frozen} stalled frames; continuous fish routes, sparse/jittered updates, receipt correction, duplicate render, restart, world switch, pause and solo checks");
    }
    static void HidingChecks()
    {
        Require(HideAndSeek.SlotX.Length==24 && HideAndSeek.Props.GroupBy(p=>p).All(g=>g.Count()==3),"Missing hiding places");
        for(var cover=0;cover<8;cover++){
            var world=SoloWorld.WithPond(SoloWorld.Create("one","two","three","four"));
            var places=Enumerable.Range(0,24).Where(i=>HideAndSeek.Props[i]==cover).ToArray();
            var actors=world.ReadPlayers().Select(p=>p.id).ToArray();
            SoloResult Send(string actor,SoloAction action,string op="",int slot=-1,float x=0){var p=world.ReadPlayer(actor);return world.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=world.Revision,zone=p.zone,visit=p.visit,action=action,value=op,target=slot.ToString(),x=x,y=HideAndSeek.RailY});}
            Require(Send("one",SoloAction.HideAndSeek,"start").Accepted,"Shared count failed");
            for(var i=0;i<4;i++){
                var slot=places[i%3];Require(Send(actors[i],SoloAction.Move,x:HideAndSeek.CoverX(slot,world.Schema)).Accepted,"Approach failed");
                Require(Send(actors[i],SoloAction.HideAndSeek,"hide",slot).Accepted,"New place failed");
                Require(HideAndSeek.SameCover(slot,places[0]),"Cover grouping lost neighbour");
            }
            var saved=world.Snapshot();SoloWorld.Restore(saved); // existing fields, additive slot IDs
            Require(Send("four",SoloAction.HideAndSeek,"out").Accepted,"Independent exit failed");
            for(var t=0;t<60 && (t<16 || world.ReadHideAndSeek().phase!=HidePhase.Idle);t++)world.AdvanceIdle(1,out _,actors);
            var game=world.ReadHideAndSeek();
            Require(game.hiders.Count(h=>h.mode==HiderMode.Found)==3 && HideAndSeek.Player(game,"four").mode==HiderMode.Away,"Physical cover inspection missed co-hiders or exit");
            Require((game.visited & HideAndSeek.CoverMask(places[0]))==HideAndSeek.CoverMask(places[0]),"Parent rechecks same cover");
        }
        Require(!HideAndSeek.SameCover(3,5) && !HideAndSeek.SameCover(-1,3),"Unrelated hidden players exposed");
        Console.WriteLine("PASS 24 hiding places across eight covers, four co-hiders in different places, shared inspection, independent exit and save validation");
    }
}
