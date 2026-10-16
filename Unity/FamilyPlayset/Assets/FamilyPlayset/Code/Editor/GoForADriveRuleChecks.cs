using System;
using System.Linq;
using LittleWeeps.Core;

public static class GoForADriveRuleChecks
{
    private static readonly string[] Actors={"one","two","three","four"};
    private static void Need(bool ok,string message){if(!ok)throw new Exception("Drive: "+message);}
    private static SoloResult Send(GameWorld w,string actor,string op,string item="",string target="",float x=0,float y=0)
    {var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=SoloAction.Drive,value=op,item=item,target=target,x=x,y=y},Actors);}
    private static void Act(GameWorld w,string actor,string op,string item="",string target="",float x=0,float y=0)=>Need(Send(w,actor,op,item,target,x,y).Accepted,op);
    private static void Travel(GameWorld w,string actor,string zone)
    {var p=w.ReadPlayer(actor);Need(w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=SoloAction.Travel,value=zone},Actors).Accepted,"travel");}
    private static void Tick(GameWorld w,double seconds,string[] active=null)
    {for(double t=0;t<seconds;t+=.1)w.AdvanceIdle(Math.Min(.1,seconds-t),out _,active??Actors);GameWorld.Validate(w.Snapshot());}
    private static void PackWave(GameWorld w)
    {
        foreach(var p in w.ReadDrive().items.Where(p=>p.slot<0)){
            string actor=Actors[int.Parse(p.id.Substring(8))%4];Act(w,actor,"pack-claim",p.id,p.revision.ToString());
            var held=w.ReadDrive().items.Single(v=>v.id==p.id);Act(w,actor,"pack-place",p.id,held.revision.ToString(),int.Parse(p.id.Substring(8)));
        }
        Tick(w,1.5);
    }
#if !UNITY_EDITOR
    public static void Main()=>Run();
#endif
    public static void Run()
    {
        var w=GameWorld.WithDinosaurWorld(GameWorld.Create(Actors));Need(w.ReadDrive()!=null,"additive world creation");
        var old=w.Snapshot();old.schema=55;old.drive=null;var upgraded=GameWorld.WithDinosaurWorld(GameWorld.Restore(old));Need(upgraded.ReadDrive()!=null && upgraded.ReadBirthday().visit==old.birthday.visit,"older shared save retains existing worlds");
        foreach(var actor in Actors)Travel(w,actor,GoForADrive.Zone);Act(w,"one","start");Need(w.ReadDrive().members.All(m=>m.joined),"one start joins four players here");
        var item=w.ReadDrive().items[0];Act(w,"one","pack-claim",item.id,item.revision.ToString());Need(!Send(w,"two","pack-claim",item.id,item.revision.ToString()).Accepted,"contested original object has one owner");
        var held=w.ReadDrive().items[0];Need(!Send(w,"one","pack-place",held.id,"999",0).Accepted,"stale item revision rejected");
        Act(w,"one","pack-place",held.id,held.revision.ToString(),0);Travel(w,"one","garden");Need(w.ReadDrive().members.Skip(1).All(m=>m.joined) && w.ReadDrive().items[0].slot==0,"departure retains siblings and packed object");
        Travel(w,"one",GoForADrive.Zone);Need(w.ReadDrive().members[0].joined,"return joins ongoing trip");PackWave(w);Need(w.ReadDrive().packingClear,"first source difficulty needs one three-item wave");
        Act(w,"one","next");Need(w.ReadDrive().stage==1,"one common journey");
        for(int stop=0;stop<3;stop++){
            Need(GoForADrive.RoadTarget(w.ReadDrive())==new[]{2,4,6}[stop],"source overtaking difficulty");
            while(w.ReadDrive().stop==""){
                var d=w.ReadDrive();Act(w,"one","lane",((d.nomadLane+1)%3).ToString());Act(w,"two","boost");Tick(w,1.6);
            }
            Need(w.ReadDrive().stop==GoForADrive.Stops[stop],"source toilet/petrol/roadworks order");
            if(stop==0){Act(w,"one","toilet");Need(!Send(w,"two","toilet").Accepted,"one toilet queue slot");Tick(w,2.1);}
            if(stop==1){
                Act(w,"two","fuel");Tick(w,2);float partial=w.ReadDrive().fuel;Travel(w,"two","garden");Need(w.ReadDrive().hoseHolder=="" && w.ReadDrive().fuel==partial,"nozzle departure releases ownership and retains partial fill");
                Act(w,"three","fuel");Tick(w,3);Need(w.ReadDrive().fuel==1,"shared source-duration fill completes");Travel(w,"two",GoForADrive.Zone);
            }
            if(stop==2){foreach(var actor in Actors){Act(w,actor,"rubble");Tick(w,.8);}Act(w,"four","light");}
            Need(w.ReadDrive().stopDone,"common stop completion");Act(w,"one","continue");
        }
        Act(w,"two","next");Need(w.ReadDrive().stage==2,"window stage is shared");
        Act(w,"one","jump");Tick(w,.2);Act(w,"two","jump");Need(w.ReadDrive().armJumpStage==2 && !Send(w,"three","jump").Accepted,"family shares one single/double jump; third leap blocked");Tick(w,2);
        Need(!w.ReadDrive().windowEligible && w.ReadDrive().stars==0,"source obstacle hit removes this pattern's reward eligibility");
        while(w.ReadDrive().obstacle==0)Tick(w,.1);
        for(int i=0;i<12;i++){
            var pattern=GoForADriveWindowLayout.Patterns[w.ReadDrive().obstacleKind];int serial=w.ReadDrive().obstacle;string actor=Actors[i%4];
            float clearDistance=pattern.span*GoForADriveWindowLayout.PixelsPerUnit*.5f+pattern.obstacle.x+pattern.obstacle.width+8;
            while(w.ReadDrive().windowDistance<clearDistance){Act(w,actor,"hand",y:.65f);Tick(w,.2);}
            Act(w,actor,"hand-release");while(w.ReadDrive().obstacle==serial)Tick(w,.1);
        }
        Need(w.ReadDrive().stars>10 && w.ReadDrive().stickers.Count(v=>v>0)==10,"source sticker rewards wrap without ending window play");
        Need(w.ReadDrive().windowSpeed>3,"full sticker cycle increases source scroll speed");
        Act(w,"one","leave");Tick(w,.2);Need(!w.ReadDrive().members[0].joined && w.ReadDrive().members.Skip(1).All(m=>m.joined),"explicit leave is respected for this visit");Act(w,"one","join");
        Act(w,"three","next");Need(w.ReadDrive().bricks.Length==12,"source twelve-piece statue");
        foreach(var b in w.ReadDrive().bricks){Act(w,"one","brick-claim",b.id,b.revision.ToString());var current=w.ReadDrive().bricks.Single(v=>v.id==b.id);Act(w,"one","brick-place",b.id,current.revision.ToString(),GoForADriveLayout.StatueX[b.variant],GoForADriveLayout.StatueY[b.variant]);}
        Need(w.ReadDrive().statueBuilt && w.ReadDrive().statues==1,"one shared statue result");
        var copy=w.Snapshot();copy.drive.bricks[0].variant=35;Need(w.ReadDrive().bricks[0].variant==0,"detached nested snapshot");
        var restored=GameWorld.Restore(w.Snapshot());Need(restored.ReadDrive().statueBuilt && restored.ReadDrive().members.All(m=>!m.joined),"save retains creation and clears transient participants");
        Act(w,"one","smash");Tick(w,.4);Need(w.ReadDrive().statueBuilt,"car approaches the intact statue before contact");Tick(w,.7);Need(!w.ReadDrive().statueBuilt && w.ReadDrive().bricks.Length==12,"car tumble retains all twelve reusable pieces");
        Act(w,"two","free-build");Act(w,"three","brick-new","47");var palette=w.ReadDrive().bricks.Last();Act(w,"three","brick-place",palette.id,palette.revision.ToString(),180,550);Need(w.ReadDrive().bricks.Last().y==550,"free build board extends beyond walking floor without changing movement bounds");
        double clock=w.ReadDrive().clock;foreach(var a in Actors)Travel(w,a,"garden");Tick(w,10);Need(w.ReadDrive().clock==clock,"empty world never catches up unattended");
        foreach(var a in Actors)Travel(w,a,GoForADrive.Zone);foreach(var a in Actors.Take(3))Act(w,a,"replay");Need(w.ReadDrive().stage==3,"shared replay awaits remaining family");Act(w,"four","replay");Need(w.ReadDrive().trip==1 && w.ReadDrive().difficulty==1,"shared replay starts next source packing difficulty");PackWave(w);Need(w.ReadDrive().packingWave==2 && !w.ReadDrive().packingClear,"next difficulty grows to six source objects");PackWave(w);Need(w.ReadDrive().packingClear,"second packing wave finishes");
        var invalid=w.Snapshot();invalid.drive.fuel=float.NaN;bool rejected=false;try{GameWorld.Restore(invalid);}catch(InvalidOperationException){rejected=true;}Need(rejected,"invalid nonfinite snapshot rejected");
        Console.WriteLine("PASS Drive migration, four-person invitation, original object claims, stale inputs, independent exits/rejoin, packing waves, net overtaking, stops/nozzle retention, double jump/wrapping stickers, statue save/copy, empty-world pause and replay consent");
    }
}
