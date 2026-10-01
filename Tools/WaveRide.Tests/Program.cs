using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program
{
    static void Check(bool value,string why){if(!value)throw new Exception(why);}
    static SoloResult Do(SoloWorld w,string id,string op,string target="",float x=0,float y=0,SoloAction action=SoloAction.Beach)=>w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,zone=w.ReadPlayer(id).zone,visit=w.ReadPlayer(id).visit,expectedRevision=w.Revision,action=action,value=op,target=target,x=x,y=y});
    static void Tick(FamilySession f,double seconds){while(seconds>.000001){var dt=Math.Min(.05,seconds);f.AdvanceIdle(dt,out _);seconds-=dt;}}
    static void Main(string[] args)
    {
        var options=new JsonSerializerOptions{IncludeFields=true};var legacy=SoloWorld.WithShore(SoloWorld.Create("a","b","c","d"));var old=legacy.Snapshot();var w=SoloWorld.WithWaveRides(legacy);
        var s=w.Snapshot();Check(s.schema==43 && JsonSerializer.Serialize(old.players,options)==JsonSerializer.Serialize(s.players,options) && JsonSerializer.Serialize(old.toys,options)==JsonSerializer.Serialize(s.toys,options) && s.shore.random==old.shore.random && s.shore.nextVisit==old.shore.nextVisit,"migration retains family/items/shore schedule");
        var f=new FamilySession(w);for(ulong i=1;i<=4;i++){Check(f.Attach(i,s.players[i-1].id,out _),"connect");Check(Do(w,s.players[i-1].id,"beach",action:SoloAction.Travel).Accepted,"beach");}
        Check(!Do(w,"a","ride-start","0").Accepted,"cannot start unboarded");
        for(var i=0;i<4;i++)Check(Do(w,s.players[i].id,"ride-join",(i%3).ToString()).Accepted,"four ready together");
        var g=w.ReadShore().ride;Check(g.riders.Length==4 && g.riders.Select(r=>r.seat).Distinct().Count()==4 && g.riders.Select(r=>r.visitor).Distinct().Count()==3,"four seats/all choices");
        Check(Do(w,"a","ride-join","2").Accepted && w.ReadShore().ride.riders.Length==4,"choice change retains seat");Check(!Do(w,"a","ride-start","0").Accepted,"stale start rejected");
        Check(Do(w,"a","ride-start",g.round.ToString()).Accepted,"one common start");Tick(f,1);Check(w.ReadShore().ride.phase==WaveRidePhase.Countdown && w.ReadShore().ride.age>0,"common countdown");
        var json=JsonSerializer.Serialize(w.Snapshot(),options);var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(json,options));Check(restored.ReadShore().ride.age==w.ReadShore().ride.age && restored.ReadShore().ride.riders.Length==4,"checkpoint retains convoy and exact countdown");
        Tick(f,4.2);g=w.ReadShore().ride;Check(g.phase==WaveRidePhase.Riding && BeachWaveRide.SplashWindow(g.age-3),"shared first wave");
        foreach(var p in s.players)Check(Do(w,p.id,"ride-wave",g.round.ToString()).Accepted,"each child splashes once");Check(!Do(w,"a","ride-wave",g.round.ToString()).Accepted && w.ReadShore().ride.splashes==4,"duplicate taps cannot inflate reward");
        var exit=g.riders.First(r=>r.actor=="a");Check(Do(w,"a","ride-leave").Accepted && w.ReadPlayer("a").x==exit.returnX && w.ReadPlayer("a").y==exit.returnY,"explicit leave returns original beach point");
        var remaining=w.ReadShore().ride;Check(remaining.riders.Length==3 && remaining.age==g.age && remaining.round==g.round,"exit retains common progress");
        Check(Do(w,"b","creek",action:SoloAction.Travel).Accepted,"independent travel");f.Detach(3);Check(w.ReadShore().ride.riders.Length==1 && w.ReadShore().ride.riders[0].actor=="d","independent disconnect");
        Check(!Do(w,"a","ride-join","0").Accepted,"no personal late round");Tick(f,22);Check(w.ReadShore().ride.phase==WaveRidePhase.Finished && w.ReadShore().ride.riders.Length==0 && w.ReadPlayer("d").y==100,"automatic return to boarding beach point");
        Check(Do(w,"a","ride-join","1").Accepted && w.ReadShore().ride.round==g.round+1,"deliberate replay opens new common lobby");
        Check(Do(w,"a","",x:1500,y:200,action:SoloAction.Move).Accepted && w.ReadShore().ride.riders.Length==0,"moving exits only own ride");
        var copy=restored.ReadShore();copy.ride.riders[0].visitor=SeaVisitor.Whale;Check(restored.ReadShore().ride.riders[0].visitor==SeaVisitor.Mermaid,"nested read isolation");
        var bad=restored.Snapshot();bad.shore.ride.riders[1].seat=bad.shore.ride.riders[0].seat;try{SoloWorld.Validate(bad);throw new Exception("duplicate seat accepted");}catch(InvalidOperationException){}
        var evidence=new{passed=true,schema=43,content=55,checks=new[]{"additive migration and existing shore timing", "four ready together and all visitors", "shared start/countdown and stale start rejection", "checkpoint reopen and nested isolation", "common waves and bounded cooperative splash", "explicit leave/travel/disconnect protect sibling progression", "late boarding rejected", "automatic return and deliberate replay", "moving exit and invalid duplicate seat"}};
        if(args.Length>0){Directory.CreateDirectory(args[0]);File.WriteAllText(Path.Combine(args[0],"core.json"),JsonSerializer.Serialize(evidence,new JsonSerializerOptions{WriteIndented=true}));}Console.WriteLine("PASS wave ride core: migration, four-seat common round, splash, saves and independent exits.");
    }
}
