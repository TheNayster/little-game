using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program
{
    static void Check(bool b,string message){if(!b)throw new Exception(message);}
    static SoloResult Do(SoloWorld w,string actor,SoloAction action,string value="",float x=0,float y=0)=>w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,expectedRevision=w.Revision,zone=w.ReadPlayer(actor).zone,visit=w.ReadPlayer(actor).visit,action=action,value=value,target=action==SoloAction.Beach?"water":"",x=x,y=y});
    static void Tick(FamilySession f,double seconds){while(seconds>.000001){var dt=Math.Min(.05,seconds);f.AdvanceIdle(dt,out _);seconds-=dt;}}
    static void Main(string[] args)
    {
        var old=SoloWorld.WithSeagulls(SoloWorld.Create("a","b","c","d"));var before=old.Snapshot();var w=SoloWorld.WithShore(old);var s=w.Snapshot();var options=new JsonSerializerOptions{IncludeFields=true};
        Check(s.schema==40 && s.worldId==before.worldId && JsonSerializer.Serialize(s.players,options)==JsonSerializer.Serialize(before.players,options) && JsonSerializer.Serialize(s.toys,options)==JsonSerializer.Serialize(before.toys,options) && JsonSerializer.Serialize(s.bedrooms,options)==JsonSerializer.Serialize(before.bedrooms,options),"additive migration retains profiles, rooms and objects");
        var f=new FamilySession(w);for(ulong i=1;i<=4;i++)Check(f.Attach(i,s.players[i-1].id,out _),"attach four");
        foreach(var p in s.players){Check(Do(w,p.id,SoloAction.Travel,"beach").Accepted,"travel");Do(w,p.id,SoloAction.Move,x:550,y:235);for(var i=1;i<=8;i++)Do(w,p.id,SoloAction.Move,x:550+i*35,y:235);}
        Check(w.ReadShore().prints.Select(p=>p.actor).Distinct().Count()==4,"four trails in one shared state");Check(w.ReadShore().prints.Any(p=>p.left) && w.ReadShore().prints.Any(p=>!p.left),"alternate paws");
        Do(w,"a",SoloAction.Move,x:900,y:480);for(var i=1;i<=5;i++)Do(w,"a",SoloAction.Move,x:900+i*35,y:480);
        Check(w.ReadShore().prints.Any(p=>p.y>=430),"wet prints created");Tick(f,11);Check(w.ReadShore().prints.Any(p=>p.y<430) && !w.ReadShore().prints.Any(p=>p.y>=430),"one wave erases wet prints and retains dry prints");
        Do(w,"c",SoloAction.Move,x:1050,y:410);Check(Do(w,"c",SoloAction.Beach,"ripple",x:1050,y:480).Accepted,"shared nearby ripple");Check(!Do(w,"c",SoloAction.Beach,"ripple",x:4500,y:480).Accepted,"distant water tap rejected");Tick(f,2.1);Check(w.ReadShore().ripples.Length==0,"bounded ripple lifetime");
        var first=w.ReadShore().nextVisit;Tick(f,first-w.ReadShore().clock+.1);var g=w.ReadShore();Check(g.visits==1 && BeachShore.Visiting(g),"random first visitor");
        Do(w,"a",SoloAction.Travel,"creek");f.Detach(2);Tick(f,1);Check(w.ReadShore().visits==1 && w.ReadShore().visitX==g.visitX,"independent travel/disconnect retain common event");
        var saved=w.Snapshot();var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(saved,options),options));
        Check(restored.ReadShore().random==w.ReadShore().random && restored.ReadShore().nextVisit==w.ReadShore().nextVisit && restored.ReadShore().prints.Length==w.ReadShore().prints.Length,"save reopen retains random schedule and trails");
        var seen=new System.Collections.Generic.HashSet<SeaVisitor>{g.visitor};var intervals=new System.Collections.Generic.List<double>();
        for(var i=0;i<30;i++){var prev=w.ReadShore();var delay=prev.nextVisit-prev.visitStarted-BeachShore.VisitSeconds;Check(delay>=18 && delay<=42,"bounded random gap");intervals.Add(delay);Tick(f,prev.nextVisit-prev.clock+.05);var next=w.ReadShore();Check(next.visitor!=prev.visitor,"no immediate repeated type");seen.Add(next.visitor);}
        Check(seen.Count==3 && intervals.Distinct().Count()>10,"all three types and random intervals");
        for(var i=0;i<300;i++)Do(w,"d",SoloAction.Move,x:1000+(i%2)*35,y:235);Check(w.ReadShore().prints.Count(p=>p.actor=="d")<=48,"per-child bounded trail");
        Do(w,"c",SoloAction.Travel,"creek");Do(w,"d",SoloAction.Travel,"garden");var paused=w.ReadShore().clock;Tick(f,10);Check(w.ReadShore().clock==paused,"empty beach pauses without catchup");
        var bad=w.Snapshot();bad.shore.clock=double.NaN;try{SoloWorld.Validate(bad);throw new Exception("NaN accepted");}catch(InvalidOperationException){}
        var copy=w.ReadShore();copy.prints=Array.Empty<SandPrint>();Check(w.ReadShore().prints.Length>0,"read isolation");
        if(args.Length>0){Directory.CreateDirectory(args[0]);File.WriteAllText(Path.Combine(args[0],"core.json"),JsonSerializer.Serialize(new{passed=true,schema=40,content=46,randomIntervals=intervals,checks=new[]{"additive migration", "four shared trails and alternating paws","wet-only wave washing","nearby ripple and distant rejection","random three-type schedule with no repeat","independent departure","JSON reopen random schedule and trails","48 prints per player","empty beach pause","validation and copy isolation"}},new JsonSerializerOptions{WriteIndented=true}));}
        Console.WriteLine("PASS shore core: migration, four trails, wet-only washing, ripples, randomized sightings, departure, save reopen, bounds and pause.");
    }
}
