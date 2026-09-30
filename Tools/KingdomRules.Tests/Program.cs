using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program
{
    static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
    static SoloResult Cmd(SoloWorld w,string actor,SoloAction action,string value="",string target="",float x=0,float y=0){var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y});}
    static void Do(SoloWorld w,string actor,string op,string target=""){if(target!=""){var at=KingdomAdventure.Prop(target);Require(Cmd(w,actor,SoloAction.Move,x:at.X,y:at.Y).Accepted,"movement");}var r=Cmd(w,actor,SoloAction.Kingdom,op,target);Require(r.Accepted,op+" "+r.Outcome);SoloWorld.Validate(w.Snapshot());}
    static void Main()
    {
        var old=SoloWorld.WithOutfits(SoloWorld.Create("one","two","three","four"));var prior=old.Snapshot();var w=SoloWorld.WithKingdom(old);var s=w.Snapshot();Require(s.worldId==prior.worldId && s.toys.Select(t=>t.id).SequenceEqual(prior.toys.Select(t=>t.id)),"migration lost records");
        foreach(var p in s.players)Require(Cmd(w,p.id,SoloAction.Travel,"daycare").Accepted,"daycare travel");
        Do(w,"one","start");Do(w,"two","start");var round=w.ReadKingdom().round;
        Require(w.ReadPlayer("three").zone=="daycare" && w.ReadKingdom().members.Count(m=>m.attending)==2,"forced player entry");
        for(int i=0;i<4;i++)w.AdvanceIdle(1,out _);Do(w,"one","fruit","fruit-0");
        Do(w,"three","start");Do(w,"four","start");Require(w.ReadKingdom().round==round && w.ReadKingdom().supplies==1,"late join restarted story");
        Do(w,"two","fruit","fruit-1");Do(w,"three","fruit","fruit-2");Require(w.ReadKingdom().phase==KingdomPhase.Bridge,"shared supply progression");
        var checkpoint=JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(w.Snapshot(),new JsonSerializerOptions{IncludeFields=true}),new JsonSerializerOptions{IncludeFields=true});SoloWorld.Validate(checkpoint);var reopened=SoloWorld.Restore(checkpoint);Require(reopened.ReadKingdom().supplies==7 && reopened.ReadKingdom().round==round,"JSON reopen lost story");
        Do(w,"four","board","board-0");Do(w,"one","leave");Require(w.ReadKingdom().boards==1 && w.ReadKingdom().members.Count(m=>m.attending)==3,"departure changed shared state");
        Do(w,"two","board","board-1");Do(w,"three","board","board-2");Do(w,"two","toss","ball");Do(w,"four","wand","wand");
        Do(w,"three","wake","friend-0");Do(w,"four","wake","friend-1");Do(w,"two","wake","friend-2");Require(w.ReadKingdom().phase==KingdomPhase.Feast && w.ReadKingdom().rescued==7,"story completion");
        for(int i=0;i<6;i++)w.AdvanceIdle(1,out _);Do(w,"three","replay");Require(w.ReadKingdom().round==round+1 && w.ReadKingdom().supplies==0,"explicit shared replay");
        var family=new FamilySession(w);for(ulong i=1;i<=4;i++)Require(family.Attach(i,new[]{"one","two","three","four"}[i-1],out _),"admission");
        family.Detach(3);Require(!w.ReadKingdom().members.Single(m=>m.actor=="three").attending && w.ReadKingdom().members.Single(m=>m.actor=="two").attending,"disconnect affected sibling");
        foreach(var m in w.ReadKingdom().members.Where(m=>m.attending))w.ReleaseKingdom(m.actor);var clock=w.ReadKingdom().clock;w.AdvanceIdle(1,out _);Require(w.ReadKingdom().clock==clock,"empty story clock ran");
        Do(w,"one","start");Require(w.ReadKingdom().round==round+1,"resume rerolled round");
        Require(!Cmd(w,"one",SoloAction.Kingdom,"wand","wand").Accepted,"out-of-step action accepted");
        var boundary=w.Snapshot();boundary.kingdom.phase=KingdomPhase.Queen;boundary.kingdom.supplies=boundary.kingdom.boards=7;boundary.kingdom.started=0;boundary.kingdom.clock=15.7447122;boundary.kingdom.distractedUntil=23.7447122;SoloWorld.Validate(boundary);
        Require(!WorldLayout.Destination(KingdomAdventure.Zone),"story added main world bubble");
        Console.WriteLine("PASS migration, four shared contributors, late join, independent exit/disconnect, JSON checkpoint, empty suspension, resume, replay and invalid action.");
    }
}
