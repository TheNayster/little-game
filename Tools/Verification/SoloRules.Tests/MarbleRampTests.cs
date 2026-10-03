using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static GameWorld RampWorld()=>GameWorld.WithMarbleRamps(DiscoveryWorld());
    static RampTray Ramps(GameWorld w,string actor="first")=>Workspace(w,actor).ramps[0];
    static SoloResult Ramp(GameWorld w,string op,string actor="first",float x=0,float y=0)=>w.Apply(Command(w,SoloAction.Discovery,actor,MarbleRamps.Token(Ramps(w,actor)),"ramp:"+op,x:x,y:y,actor:actor));
    static void MarbleRampTests()
    {
        Test("completed ramp time survives Unity double rounding without allowing an out of bounds release",()=>{
            var course=new RampCourse();MarbleRamps.Move(course,5,680,440);var duration=MarbleRamps.Path(course).Duration;
            var tray=new RampTray{current=new RampState{course=course,released=true,elapsed=14.816666666666668}};
            Check(Math.Abs(tray.current.elapsed-duration)<1e-12);MarbleRamps.Validate(tray);Check(!MarbleRamps.Running(tray.current));
            tray.current.elapsed=duration-1e-12;MarbleRamps.Validate(tray);Check(!MarbleRamps.Running(tray.current));
            tray.current.elapsed=duration+1e-6;Throws(()=>MarbleRamps.Validate(tray));
        });
        Test("starter marble course contacts all three ramps and reaches its visible basket",()=>{
            var path=MarbleRamps.Path(new RampCourse());Check(path.contacts==7 && path.Caught && path.Duration>2 && path.Duration<45);
            Check(path.points.All(p=>p.x>=55 && p.x<=945 && p.y>=40 && p.y<=542));
            System.IO.File.WriteAllText(System.IO.Path.Combine(root,"marble-path.txt"),"Starter duration "+path.Duration+", samples "+path.points.Length+", final "+path.points.Last().x+", "+path.points.Last().y);
        });
        Test("changing surface alone changes motion and steeper first ramps accelerate the marble",()=>{
            var smooth=new RampCourse();var felt=smooth.Copy();felt.surface=1;var rough=smooth.Copy();rough.surface=2;
            var a=MarbleRamps.Path(smooth).At(4);var b=MarbleRamps.Path(felt).At(4);var c=MarbleRamps.Path(rough).At(4);Check(a.x>b.x && b.x>c.x && smooth.points.SequenceEqual(felt.points));
            var shallow=smooth.Copy();shallow.points[3]=160;Check(MarbleRamps.Path(smooth).At(3).x>MarbleRamps.Path(shallow).At(3).x);
            var altered=smooth.Copy();MarbleRamps.Move(altered,2,700,230);Check(!MarbleRamps.Path(altered).points.SequenceEqual(MarbleRamps.Path(smooth).points));
        });
        Test("ramp migration appends owned workspaces and remaps existing cleanup cues by identity",()=>{
            var old=GameWorld.WithMealPreparation(DiscoveryWorld());Check(Discover(old,"fill:0:5").Accepted);var s=old.Snapshot();s.homeTidyCues=new[]{Array.IndexOf(HomeTidying.Keys(s),HomeTidying.Lab("first","liquid"))};old=GameWorld.Restore(s);var before=old.Snapshot();var cues=HomeTidying.CueKeys(before);
            var w=GameWorld.WithMarbleRamps(old);var next=w.Snapshot();Check(next.schema==27 && next.discovery.All(d=>d.ramps.Length==1) && HomeTidying.CueKeys(next).SequenceEqual(cues));next.schema=26;next.revision--;next.homeTidyCues=before.homeTidyCues;foreach(var d in next.discovery)d.ramps=Array.Empty<RampTray>();Check(Encode(next)==Encode(before));
        });
        Test("four owned releases continue on departure and independent edits do not reset siblings",()=>{
            var w=RampWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();for(var i=0;i<4;i++){Check(Ramp(w,"surface:"+(i%3),actors[i]).Accepted);Check(Ramp(w,"roll",actors[i]).Accepted);}Advance(w,2);var other=Encode(w.ReadDiscovery().Skip(1).ToArray());Check(Ramp(w,"move:0",x:220,y:110).Accepted && !Ramps(w).current.released);Check(other==Encode(w.ReadDiscovery().Skip(1).ToArray()));
            Good(w,SoloAction.Travel,value:"park",actor:actors[3]);Advance(w,50);Check(!MarbleRamps.Running(Ramps(w,actors[3]).current));Check(!Ramp(w,"roll",actors[3]).Accepted);var s=w.Snapshot();Check(Encode(GameWorld.Restore(Decode(Encode(s))).Snapshot())==Encode(s));
        });
        Test("one marble and saved partial trajectory survive repeated requests and cold restore",()=>{
            var w=RampWorld();var c=Command(w,SoloAction.Discovery,"first",MarbleRamps.Token(Ramps(w)),"ramp:roll");Check(w.Apply(c).Accepted);Advance(w,2);var before=CreationJson(Ramps(w));Check(w.Apply(c).Duplicate && before==CreationJson(Ramps(w)));w=GameWorld.Restore(Decode(Encode(w.Snapshot())));Check(before==CreationJson(Ramps(w)));
            var a=RampWorld();var b=RampWorld();Check(Ramp(a,"roll").Accepted && Ramp(b,"roll").Accepted);for(var i=0;i<50;i++)a.AdvanceIdle(.1,out _);Advance(b,5);Check(Math.Abs(Ramps(a).current.elapsed-Ramps(b).current.elapsed)<1e-8);
        });
        Test("kept course survives reset and five minute cleanup with independent undo",()=>{
            var w=RampWorld();Check(Ramp(w,"move:0",x:250,y:100).Accepted && Ramp(w,"keep").Accepted);var kept=CreationJson(Ramps(w).kept);Check(Ramp(w,"surface:2").Accepted);var temporary=CreationJson(Ramps(w).current);Advance(w,306);Check(kept==CreationJson(Ramps(w).kept) && MarbleRamps.Same(Ramps(w).current.course,Ramps(w).kept[0]));Check(Ramp(w,"undo").Accepted && CreationJson(Ramps(w).current)==temporary);Check(Ramp(w,"starter").Accepted && kept==CreationJson(Ramps(w).kept));Check(Ramp(w,"restore").Accepted && MarbleRamps.Same(Ramps(w).current.course,Ramps(w).kept[0]));
            Check(!MarbleRamps.Temporary(Ramps(w)));w=GameWorld.Restore(w.Snapshot());Check(kept==CreationJson(Ramps(w).kept));
        });
        Test("ramp ownership stale gestures and malformed coordinates cannot replace another course",()=>{
            var w=RampWorld();var token=MarbleRamps.Token(Ramps(w));Check(Ramp(w,"surface:1").Accepted);var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Discovery,"first",token,"ramp:move:1",x:500,y:200)).Accepted);Check(!w.Apply(Command(w,SoloAction.Discovery,"second",MarbleRamps.Token(Ramps(w,"second")),"ramp:roll")).Accepted);
            foreach(var op in new[]{"move:6","surface:3","bad","restore"})Check(!Ramp(w,op).Accepted);Check(!Ramp(w,"move:0",x:float.NaN,y:100).Accepted && before==Encode(w.Snapshot()));
        });
        Test("ramp copies validation and bounded edits protect all current undo and kept geometry",()=>{
            var w=RampWorld();Check(Ramp(w,"keep").Accepted && Ramp(w,"move:0",x:999,y:0).Accepted);MarbleRamps.Validate(Ramps(w));var original=w.Snapshot();var s=w.Snapshot();s.discovery[0].ramps[0].kept[0].points[0]=900;Check(Ramps(w).kept[0].points[0]==210);
            foreach(var edit in new Action<RampTray>[] {t=>t.current.course.points=null,t=>t.current.course.points[2]=t.current.course.points[0],t=>t.current.course.surface=4,t=>t.current.elapsed=double.NaN,t=>t.previous=new[]{t.current.Copy(),t.current.Copy()},t=>t.kept[0].points[1]=-1,t=>t.revision=long.MaxValue}){var copy=GameWorld.CopySnapshot(original);edit(copy.discovery[0].ramps[0]);Throws(()=>GameWorld.Validate(copy));}
            for(var i=0;i<6;i++){var c=new RampCourse();MarbleRamps.Move(c,i,i%2*1000,i%2*600);MarbleRamps.ValidateCourse(c);Check(MarbleRamps.Path(c).points.Length<=5401);}
        });
    }
}
