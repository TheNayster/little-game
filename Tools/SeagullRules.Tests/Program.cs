using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program
{
    static void Check(bool b,string message){if(!b)throw new Exception(message);}
    static SoloResult Do(SoloWorld w,string actor,SoloAction action,string value="",float x=0,float y=0)=>w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,expectedRevision=w.Revision,zone=w.ReadPlayer(actor).zone,visit=w.ReadPlayer(actor).visit,action=action,value=value,target=action==SoloAction.Beach?"flock":"",x=x,y=y});
    static void Tick(FamilySession f,double seconds){while(seconds>.000001){var dt=Math.Min(.05,seconds);f.AdvanceIdle(dt,out _);seconds-=dt;}}
    static void Main(string[] args)
    {
        var old=SoloWorld.WithOutfits(SoloWorld.Create("a","b","c","d"));Do(old,"a",SoloAction.Move,x:1200,y:200);
        var before=old.Snapshot();var w=SoloWorld.WithSeagulls(old);var s=w.Snapshot();
        Check(s.worldId==before.worldId && s.schema==37 && s.players[0].x==1200 && s.players[0].avatar==before.players[0].avatar && s.toys.Select(t=>t.id).SequenceEqual(before.toys.Select(t=>t.id)),"additive identities, player position and objects");
        var jsonOptions=new JsonSerializerOptions{IncludeFields=true};
        Check(JsonSerializer.Serialize(s.bedrooms,jsonOptions)==JsonSerializer.Serialize(before.bedrooms,jsonOptions) && JsonSerializer.Serialize(s.secrets,jsonOptions)==JsonSerializer.Serialize(before.secrets,jsonOptions) && s.homeCreations==before.homeCreations,"preserve rooms and creations");
        var f=new FamilySession(w);for(ulong i=1;i<=4;i++)Check(f.Attach(i,s.players[i-1].id,out _),"attach four");
        foreach(var p in s.players){Check(Do(w,p.id,SoloAction.Travel,"beach").Accepted,"beach travel");Do(w,p.id,SoloAction.Move,x:900,y:235);}
        Tick(f,.05);Check(w.ReadSeagulls().phase==GullPhase.Notice && w.ReadSeagulls().flights==1,"one shared approach by four");
        var destination=w.ReadSeagulls().to;
        Check(!Do(w,"b",SoloAction.Beach,"hello").Accepted && w.ReadSeagulls().flights==1,"repeated taps do not restart flight");
        Tick(f,1);Check(w.ReadSeagulls().phase==GullPhase.TakeOff,"takeoff phase");
        Do(w,"a",SoloAction.Travel,"creek");f.Detach(2);Tick(f,2);Check(w.ReadSeagulls().phase==GullPhase.Flying && w.ReadSeagulls().to==destination,"flight continues after travel and disconnection");
        Tick(f,4);Check(w.ReadSeagulls().phase==GullPhase.Resting && w.ReadSeagulls().from==destination,"lands in bounded patch");
        Do(w,"c",SoloAction.Move,x:BeachSeagulls.Patches[destination],y:235);Tick(f,6);
        Check(w.ReadSeagulls().phase==GullPhase.Resting && w.ReadSeagulls().flights==1,"standing next to landed birds does not permanently chase them");
        Check(Do(w,"c",SoloAction.Beach,"hello").Accepted,"nearby tap starts fresh encounter after calm");
        Tick(f,1.7);var saved=w.Snapshot();var json=JsonSerializer.Serialize(saved,jsonOptions);var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(json,jsonOptions));
        Check(restored.ReadSeagulls().phase==saved.seagulls.phase && restored.ReadSeagulls().to==saved.seagulls.to && restored.ReadSeagulls().flights==2,"mid-flight JSON reopen preserves flock");
        var copy=w.ReadSeagulls();copy.nearby=new[]{"fake"};Check(!w.ReadSeagulls().nearby.Contains("fake"),"read copies isolate state");
        var invalid=w.Snapshot();invalid.seagulls.age=double.NaN;try{SoloWorld.Validate(invalid);throw new Exception("accepted NaN");}catch(InvalidOperationException){}
        Check(!Do(w,"a",SoloAction.Beach,"hello").Accepted,"other worlds cannot tap birds");
        foreach(var phase in new[]{GullPhase.TakeOff,GullPhase.Flying,GullPhase.Landing}){
            var g=w.ReadSeagulls();g.phase=phase;g.age=BeachSeagulls.Duration(phase);var point=BeachSeagulls.Point(g,0,0,out var height);
            Check(point.X>=0 && point.X<=4800 && height>=-10 && height<=250,"bounded flight artwork path");
        }
        if(args.Length>0){Directory.CreateDirectory(args[0]);File.WriteAllText(Path.Combine(args[0],"core.json"),JsonSerializer.Serialize(new{passed=true,schema=37,content=42,checks=new[]{"additive migration and retained rooms/objects","one flock for four approaches","flight spam ignored","independent travel/disconnect","landing and calm while player remains near","nearby tap","mid-flight JSON reopen","copy isolation and malformed-state rejection","other-world tap rejection","bounded flight path"}}));}
        Console.WriteLine("PASS Seagull surprise: migration, shared approach, flight, landing, calm, independent exits and reopen");
    }
}
