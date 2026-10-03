using System;
using System.Linq;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class RampCourse
    {
        public int surface;
        public int[] points={210,150,760,220,860,280,230,345,140,410,700,470};
        public RampCourse Copy()=>new RampCourse{surface=surface,points=(int[])points.Clone()};
    }
    [Serializable] public sealed class RampState
    {
        public RampCourse course=new RampCourse();
        public bool released;
        public double elapsed;
        public RampState Copy()=>new RampState{course=course.Copy(),released=released,elapsed=elapsed};
    }
    [Serializable] public sealed class RampTray
    {
        public long revision;
        public RampState current=new RampState();
        public RampState[] previous=Array.Empty<RampState>();
        public RampCourse[] kept=Array.Empty<RampCourse>();
        public RampTray Copy()=>new RampTray{revision=revision,current=current.Copy(),previous=previous.Select(s=>s.Copy()).ToArray(),kept=kept.Select(c=>c.Copy()).ToArray()};
    }
    public readonly struct MarblePoint
    {
        public readonly double x,y,roll;
        public MarblePoint(double x,double y,double roll){this.x=x;this.y=y;this.roll=roll;}
    }
    public sealed class MarblePath
    {
        public readonly MarblePoint[] points;
        public readonly int contacts;
        public double Duration=>(points.Length-1)*MarbleRamps.Step;
        public bool Caught=>points[points.Length-1].y>=541 && points[points.Length-1].x>=650 && points[points.Length-1].x<=850;
        public MarblePath(MarblePoint[] points,int contacts){this.points=points;this.contacts=contacts;}
        public MarblePoint At(double seconds){var pos=Math.Max(0,Math.Min(points.Length-1,seconds/MarbleRamps.Step));var i=(int)pos;var a=points[i];var b=points[Math.Min(i+1,points.Length-1)];var t=pos-i;return new MarblePoint(a.x+(b.x-a.x)*t,a.y+(b.y-a.y)*t,a.roll+(b.roll-a.roll)*t);}
    }
    public static class MarbleRamps
    {
        public const int Schema=27;
        public const double Step=1d/120,MaxSeconds=45,Radius=12;
        // Unity's JSON reader can round a completed double a few ULPs past
        // the simulated endpoint. Accept one nanosecond without extending play.
        public const double TimeTolerance=1e-9;
        public static readonly string[] Surfaces={"Smooth wood","Soft felt","Ribbed rubber"};
        private static readonly Dictionary<string,MarblePath> paths=new Dictionary<string,MarblePath>();
        public static string Token(RampTray t)=>"ramps@"+t.revision;
        public static bool Same(RampCourse a,RampCourse b)=>a.surface==b.surface && a.points.SequenceEqual(b.points);
        public static bool Running(RampState s)=>s.released && s.elapsed<Path(s.course).Duration-TimeTolerance;
        public static RampCourse RestingCourse(RampTray t)=>t.kept.Length==0?new RampCourse():t.kept[0].Copy();
        public static bool Temporary(RampTray t)=>t.current.released || !Same(t.current.course,RestingCourse(t));
        public static void Move(RampCourse c,int handle,int x,int y)
        {
            var ramp=handle/2;var pair=handle%2==0?handle+1:handle-1;var left=(ramp!=1)==(handle%2==0);
            c.points[handle*2]=Math.Max(left?70:c.points[pair*2]+160,Math.Min(left?c.points[pair*2]-160:930,(int)Math.Round(x/10d)*10));
            c.points[handle*2+1]=Math.Max(80+ramp*130,Math.Min(240+ramp*130,(int)Math.Round(y/10d)*10));
        }
        public static void ValidateCourse(RampCourse c)
        {
            if(c==null || c.surface<0 || c.surface>=Surfaces.Length || c.points==null || c.points.Length!=12)throw new InvalidOperationException("Invalid ramp course.");
            for(var i=0;i<6;i++)if(c.points[i*2]<70 || c.points[i*2]>930 || c.points[i*2+1]<80+i/2*130 || c.points[i*2+1]>240+i/2*130)throw new InvalidOperationException("Ramp outside board.");
            for(var i=0;i<3;i++)if((c.points[i*4+2]-c.points[i*4])*(i==1?-1:1)<160)throw new InvalidOperationException("Ramp too short.");
        }
        public static void Validate(RampTray t)
        {
            if(t==null || t.revision<0 || t.revision>=long.MaxValue || t.previous==null || t.previous.Length>1 || t.kept==null || t.kept.Length>1)throw new InvalidOperationException("Invalid ramp tray.");
            foreach(var c in t.kept)ValidateCourse(c);
            foreach(var s in new[]{t.current}.Concat(t.previous)){
                if(s==null)throw new InvalidOperationException("Missing ramp state.");ValidateCourse(s.course);
                if(double.IsNaN(s.elapsed) || double.IsInfinity(s.elapsed) || s.elapsed<0 || s.elapsed>Path(s.course).Duration+TimeTolerance || !s.released && s.elapsed!=0)throw new InvalidOperationException("Invalid marble release.");
            }
        }
        // The same bounded fixed-step trajectory drives authority completion and
        // client pictures. Cache edited geometry, rather than replaying physics
        // every rendered frame or sending a full world for each marble position.
        public static MarblePath Path(RampCourse c)
        {
            var key=c.surface+":"+string.Join(",",c.points);
            lock(paths){if(paths.TryGetValue(key,out var found))return found;}
            var p=c.points;var x=p[0]+18d;var y=p[1]-24d;double vx=0,vy=0,roll=0;var list=new List<MarblePoint>{new MarblePoint(x,y,0)};var contacts=0;
            var drag=new[]{.12,1.1,2.4}[c.surface];
            for(var step=0;step<(int)(MaxSeconds/Step);step++){
                var oldX=x;var oldY=y;vy+=420*Step;x+=vx*Step;y+=vy*Step;
                // Visible stops at the high ends catch the preceding ramp's exit.
                for(var i=1;i<3;i++){var ax=p[i*4];var ay=p[i*4+1];var sign=i==1?1:-1;var edge=ax-sign*Radius;
                    if(y>ay-110 && y<ay+20 && (oldX-edge)*sign<=0 && (x-edge)*sign>0){x=edge;vx=-sign*Math.Abs(vx)*.55;}}
                for(var i=0;i<3;i++){
                    var ax=p[i*4];var ay=p[i*4+1];var dx=p[i*4+2]-ax;var dy=p[i*4+3]-ay;var u=(x-ax)/dx;if(u<0 || u>1)continue;
                    var top=ay+u*dy-Radius;
                    if(y>=top && oldY<=top+20 && vy>=-20){y=top;var length=Math.Sqrt(dx*dx+dy*dy);var acceleration=420*dy/length;vx+=(acceleration*dx/length-vx*drag)*Step;vy=vx*dy/dx;contacts|=1<<i;}
                }
                if(x<55 || x>945){x=Math.Max(55,Math.Min(945,x));vx=-vx*.4;}
                if(y>=542)y=542;
                roll+=(x-oldX)/Radius*180/Math.PI;list.Add(new MarblePoint(x,y,roll));if(y>=542)break;
            }
            var path=new MarblePath(list.ToArray(),contacts);
            lock(paths){if(paths.Count>=16)paths.Clear();paths[key]=path;}return path;
        }
    }
    public sealed partial class GameWorld
    {
        public static GameWorld WithMarbleRamps(GameWorld world)
        {
            world=WithMealPreparation(world);if(world.Schema>=MarbleRamps.Schema)return world;
            var s=world.Snapshot();var cues=HomeTidying.CueKeys(s);s.schema=MarbleRamps.Schema;s.revision++;
            foreach(var w in s.discovery)w.ramps=new[]{new RampTray()};
            var keys=HomeTidying.Keys(s);s.homeTidyCues=cues.Select(k=>Array.IndexOf(keys,k)).ToArray();
            Validate(s);return new GameWorld(s);
        }
        private string RampOperation(SoloCommand c,DiscoveryWorkspace w)
        {
            if(state.schema<MarbleRamps.Schema)return "ramps-upgrade-needed";
            var t=w.ramps[0];if(c.target!=MarbleRamps.Token(t) || t.revision>=long.MaxValue-1)return "ramps-changed";
            var s=t.current.Copy();var op=c.value.Substring(5);
            if(op=="keep"){t.kept=new[]{s.course.Copy()};t.revision++;return null;}
            if(op=="undo"){if(t.previous.Length==0)return "nothing-to-undo";s=t.previous[0].Copy();t.previous=Array.Empty<RampState>();}
            else{
                switch(op){
                    case "roll":s.released=true;s.elapsed=0;break;
                    case "again":s.released=false;s.elapsed=0;break;
                    case "starter":s=new RampState();break;
                    case "restore":if(t.kept.Length==0)return "no-kept-course";s=new RampState{course=t.kept[0].Copy()};break;
                    default:
                        if(op.StartsWith("surface:") && int.TryParse(op.Substring(8),out var surface) && surface>=0 && surface<3){s.course.surface=surface;}
                        else if(op.StartsWith("move:") && int.TryParse(op.Substring(5),out var handle) && handle>=0 && handle<6 && KeepyRules.Finite(c.x) && KeepyRules.Finite(c.y) && c.x>=0 && c.x<=1000 && c.y>=0 && c.y<=600)MarbleRamps.Move(s.course,handle,(int)c.x,(int)c.y);
                        else return "invalid-ramp-edit";
                        s.released=false;s.elapsed=0;break;
                }
                t.previous=new[]{t.current.Copy()};
            }
            t.current=s;t.revision++;return null;
        }
        private bool AdvanceMarbleRamps(double seconds,out bool visible)
        {
            visible=false;if(state.schema<MarbleRamps.Schema)return false;var changed=false;
            foreach(var w in state.discovery){var s=w.ramps[0].current;if(!MarbleRamps.Running(s))continue;
                var duration=MarbleRamps.Path(s.course).Duration;s.elapsed=Math.Min(duration,s.elapsed+seconds);changed=true;if(s.elapsed==duration)visible=true;
            }
            return changed;
        }
    }
}
