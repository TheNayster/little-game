using System;
using System.Linq;
using LittleWeeps.Core;

public static class SpideyHQRuleChecks
{
    static readonly string[] Actors={"one","two","three","four"};
    static void Need(bool ok,string message){if(!ok)throw new Exception("HQ: "+message);}
    static SoloResult Send(GameWorld w,string actor,SoloAction action,string value="",string item="",string target="",float x=0,float y=0)
    {var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,item=item,target=target,x=x,y=y},Actors);}
    static void Move(GameWorld w,string a,float x,float y=160)=>Need(Send(w,a,SoloAction.Move,x:x,y:y).Accepted,"move");
    static void Act(GameWorld w,string a,string op,string item="",string target="",float x=0,float y=0)=>Need(Send(w,a,SoloAction.Spidey,op,item,target,x,y).Accepted,op);
    static void Tick(GameWorld w,double seconds){for(double at=0;at<seconds;at+=.25)w.AdvanceIdle(.25,out _,Actors);GameWorld.Validate(w.Snapshot());}
#if !UNITY_EDITOR
    public static void Main()=>Run();
#endif
    public static void Run()
    {
        var legacy=GameWorld.WithBirthday(GameWorld.Create(Actors)).Snapshot();legacy.schema=54;legacy.spidey=null;var oldBirthday=legacy.birthday.visit;
        var world=GameWorld.WithDinosaurWorld(GameWorld.Restore(legacy));Need(world.Schema==WorldLayout.Schema && world.ReadSpidey()!=null && world.ReadBirthday().visit==oldBirthday,"additive legacy migration");
        foreach(var actor in Actors){Need(Send(world,actor,SoloAction.Travel,SpideyHQ.Zone).Accepted,"arrival");Move(world,actor,SpideyHQ.X("pizza"));Act(world,actor,"queue",target:"pizza");}
        Tick(world,13);Need(world.ReadSpidey().props.Count(p=>Actors.Contains(p.holder))==4,"one queue serves four human holders");
        foreach(var actor in Actors)Act(world,actor,"eat");Tick(world,4);Need(world.ReadSpidey().props.All(p=>!Actors.Contains(p.holder)),"consumption releases all hands");
        Move(world,"one",SpideyHQ.X("balloon"));Act(world,"one","queue",target:"balloon");Tick(world,3);Act(world,"one","float");Need(world.ReadSpidey().members[0].station=="float","balloon floating");
        Need(Send(world,"one",SoloAction.Travel,"garden").Accepted,"independent exit");Need(world.ReadSpidey().props.All(p=>p.holder!="one"),"departure releases prop");
        Send(world,"one",SoloAction.Travel,SpideyHQ.Zone);foreach(var a in Actors)Move(world,a,2400);
        Tick(world,15);Need(world.ReadSpidey().battle=="fight","natural theft begins");
        foreach(var a in Actors)Act(world,a,"web");Tick(world,4);Need(world.ReadSpidey().recoveries==1 && world.ReadSpidey().props.Single(p=>p.id==world.ReadSpidey().stolen).holder=="","four people finish one stolen-item recovery");
        foreach(var a in Actors)Move(world,a,SpideyHQ.X("puzzle"));foreach(var a in Actors)Act(world,a,"puzzle-join");
        for(int i=0;i<4;i++){Act(world,Actors[i],"puzzle-claim",i.ToString());var h=world.ReadSpidey();Act(world,Actors[i],"puzzle-place",i.ToString(),h.puzzleRevision.ToString(),i%2,i/2);}
        Need(world.ReadSpidey().puzzles==1,"shared four-piece completion");
        Act(world,"two","puzzle-select","4","5");Act(world,"two","puzzle-claim","12");Need(!Send(world,"three",SoloAction.Spidey,"puzzle-claim","12").Accepted,"piece has one owner");
        Send(world,"two",SoloAction.Travel,"garden");Need(world.ReadSpidey().puzzleHolders[12]=="" && world.ReadSpidey().members[2].station=="puzzle","one puzzle departure keeps siblings");
        Move(world,"one",SpideyHQ.X("build"));Act(world,"one","brick-new","25");var brick=world.ReadSpidey().bricks.Single();Act(world,"one","brick-turn",brick.id,brick.version.ToString());brick=world.ReadSpidey().bricks.Single();Act(world,"one","brick-place",brick.id,brick.version.ToString(),3400,160);
        Move(world,"three",SpideyHQ.X("build"));Move(world,"four",SpideyHQ.X("build"));Act(world,"three","brick-claim",brick.id);Need(!Send(world,"four",SoloAction.Spidey,"brick-claim",brick.id).Accepted,"one editor per brick");
        var snapshot=world.Snapshot();var copy=GameWorld.CopySnapshot(snapshot);copy.spidey.bricks[0].variant=0;Need(world.ReadSpidey().bricks[0].variant==25,"snapshot copy does not share mutable world arrays");
        var restored=GameWorld.Restore(snapshot);Need(restored.ReadSpidey().bricks.Single().owner=="" && restored.ReadSpidey().bricks.Single().turn==1 && restored.ReadSpidey().puzzleImage==4,"restore preserves creations and clears transient ownership");
        var layout=GameWorld.WithDinosaurWorld(GameWorld.Create(Actors));foreach(var a in Actors)Send(layout,a,SoloAction.Travel,SpideyHQ.Zone);
        Move(layout,"one",SpideyHQLayout.SeatX[0]);Act(layout,"one","seat","0");Need(Math.Abs(layout.ReadPlayer("one").y-SpideyHQLayout.SeatY[0])<.01,"authored seat contact");Act(layout,"one","leave");
        foreach(var a in Actors){Move(layout,a,SpideyHQ.X("slide"));Act(layout,a,"queue",target:"slide");}
        Tick(layout,.25);Need(Math.Abs(layout.ReadPlayer("four").x-SpideyHQLayout.Queue("slide",3,false))<.01,"fourth shared queue slot");
        Send(layout,"two",SoloAction.Travel,"garden");Tick(layout,6);Need(layout.ReadSpidey().slideQueue.Length==0 && Math.Abs(layout.ReadPlayer("four").x-SpideyHQLayout.SlidePoint(1,false))<.01,"slide siblings finish after independent departure");
        Tick(layout,22);Need(layout.ReadSpidey().battle=="fight","decline encounter begins");Act(layout,"one","leave");Act(layout,"three","leave");Act(layout,"four","leave");Tick(layout,1);Need(layout.ReadSpidey().battle=="idle" && layout.ReadSpidey().recoveries==0,"all declining ends encounter without reward");
        var stale=layout.Snapshot();stale.spidey.props[0].location="ground";stale.spidey.props[0].holder="";stale.spidey.clock+=70;stale.spidey.props[0].releasedAt=0;stale.spidey.nextTheft=stale.spidey.clock+100;
        var restock=GameWorld.Restore(stale);Tick(restock,.25);Need(restock.ReadSpidey().props[0].location=="stock","abandoned prop returns to reusable stock");
        GameWorld.Validate(restored.Snapshot());Console.WriteLine("PASS Spidey HQ migration, four-person queue, props/float/exit, natural cooperative theft, shared puzzle, contested edits, authored layout, declined encounter, restock and restoration");
    }
}
