using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum GullPhase { Resting, Notice, TakeOff, Flying, Landing }
    [Serializable] public sealed class SeagullState
    {
        public GullPhase phase;
        public double clock, age, calmUntil;
        public int from, to, trailFrom, flights;
        public string[] nearby=Array.Empty<string>();
        public SeagullState Copy(){var s=(SeagullState)MemberwiseClone();s.nearby=(string[])nearby.Clone();return s;}
    }
    public static class BeachSeagulls
    {
        // Schema 36 is reserved for the concurrent pond; do not reuse its ID.
        public const int Schema=37, Count=6;
        public const float Radius=165, TapRadius=290, GroundY=235;
        public const double CalmSeconds=3;
        public static readonly float[] Patches={900,1580,2220};
        public static double Duration(GullPhase phase)=>phase==GullPhase.Notice?.55:phase==GullPhase.TakeOff?1.15:phase==GullPhase.Flying?3.4:phase==GullPhase.Landing?1.4:double.MaxValue;
        public static bool Near(SoloPlayer p,int patch,float radius)=>p.zone=="beach" && Distance(p.x,p.y,Patches[patch],GroundY)<=radius*radius;
        private static float Distance(float x,float y,float tx,float ty)=>(x-tx)*(x-tx)+(y-ty)*(y-ty);
        public static WalkPoint Point(SeagullState s,int bird,double ahead,out float height)
        {
            var t=(float)Math.Max(0,Math.Min(1,(s.age+ahead)/Duration(s.phase)));
            var eased=t*t*(3-2*t);var x=Patches[s.from];height=0;
            if(s.phase==GullPhase.TakeOff){x+=Math.Sign(s.to-s.from)*70*eased;height=155*eased;}
            if(s.phase==GullPhase.Flying){x+=(Patches[s.to]-x)*eased;height=155+(float)Math.Sin(t*Math.PI)*85;}
            if(s.phase==GullPhase.Landing){x=Patches[s.to];height=155*(1-eased);}
            // Every bird has a stable offset; shared state chooses the route,
            // while these authored paths add no client-side random decisions.
            if(s.phase==GullPhase.Flying)x+=Math.Sign(s.to-s.from)*70*(1-eased);
            x+=(bird-2.5f)*58;height+=s.phase==GullPhase.Flying?(float)Math.Sin(t*5+bird*.7)*9:0;
            return new WalkPoint(x,GroundY+(bird%2==0?-18:18));
        }
    }
    public sealed partial class SoloWorld
    {
        public SeagullState ReadSeagulls()=>state.seagulls?.Copy();
        public static SoloWorld WithSeagulls(SoloWorld world)
        {
            world=WithOutfits(world);if(world.Schema>=BeachSeagulls.Schema && world.state.seagulls!=null)return world;
            var s=world.Snapshot();s.seagulls=new SeagullState();s.schema=Math.Max(s.schema,BeachSeagulls.Schema);s.revision++;
            Validate(s);return new SoloWorld(s);
        }
        private static void ValidateSeagulls(SoloSnapshot s)
        {
            if(s.seagulls==null && s.schema<WorldLayout.Schema)return;
            // Unity can deserialize an omitted nested record as all defaults.
            if(s.schema<BeachSeagulls.Schema){
                var old=s.seagulls;if(old!=null && old.phase==0 && old.clock==0 && old.age==0 && old.calmUntil==0 && old.from==0 && old.to==0 && old.flights==0 && (old.nearby==null || old.nearby.Length==0))s.seagulls=null;
                if(s.seagulls!=null)throw new InvalidOperationException("Seagulls require schema 37.");return;
            }
            var g=s.seagulls;
            if(g==null || !Enum.IsDefined(typeof(GullPhase),g.phase) || !KeepyRules.Finite(g.clock) || g.clock<0 ||
                !KeepyRules.Finite(g.age) || g.age<0 || !KeepyRules.Finite(g.calmUntil) || g.calmUntil<0 ||
                g.from<0 || g.from>=3 || g.to<0 || g.to>=3 || g.trailFrom<0 || g.trailFrom>=3 || g.flights<0 ||
                g.phase!=GullPhase.Resting && (g.from==g.to || g.age>BeachSeagulls.Duration(g.phase)+.00001) ||
                g.nearby==null || g.nearby.Length>4 || g.nearby.Distinct().Count()!=g.nearby.Length || g.nearby.Any(id=>!s.players.Any(p=>p.id==id)))
                throw new InvalidOperationException("Invalid shared seagull flock.");
        }
        private void StartSeagulls(SoloPlayer[] watchers)
        {
            var g=state.seagulls;
            g.trailFrom=g.from;
            // Prefer the patch with most breathing room for all beach players.
            g.to=Enumerable.Range(0,3).Where(i=>i!=g.from).OrderByDescending(i=>watchers.Length==0?Math.Abs(i-g.from):watchers.Min(p=>(p.x-BeachSeagulls.Patches[i])*(p.x-BeachSeagulls.Patches[i])+(p.y-BeachSeagulls.GroundY)*(p.y-BeachSeagulls.GroundY))).ThenBy(i=>i).First();
            g.phase=GullPhase.Notice;g.age=0;g.flights=g.flights==int.MaxValue?0:g.flights+1;
        }
        private string SeagullOperation(SoloCommand c,SoloPlayer p)
        {
            if(c.value.StartsWith("ride-",StringComparison.Ordinal))return WaveRideOperation(c,p);
            if(c.value=="ripple")return ShoreOperation(c,p);
            var g=state.seagulls;if(g==null || c.value!="hello")return "unknown-beach-action";
            if(c.target!="flock" || !BeachSeagulls.Near(p,g.from,BeachSeagulls.TapRadius))return "walk-near-the-birds";
            if(g.phase!=GullPhase.Resting || g.clock<g.calmUntil)return "let-birds-settle";
            StartSeagulls(state.players.Where(v=>v.zone=="beach").ToArray());return null;
        }
        private bool AdvanceSeagulls(double dt,string[] active,out bool visible)
        {
            visible=false;var g=state.seagulls;if(g==null)return false;
            var watchers=state.players.Where(p=>p.zone=="beach" && (active==null || active.Contains(p.id))).ToArray();
            if(watchers.Length==0){var changed=g.nearby.Length>0;g.nearby=Array.Empty<string>();return changed;}
            g.clock+=dt;
            var near=watchers.Where(p=>BeachSeagulls.Near(p,g.from,BeachSeagulls.Radius)).Select(p=>p.id).OrderBy(id=>id,StringComparer.Ordinal).ToArray();
            if(g.phase==GullPhase.Resting && g.clock>=g.calmUntil && near.Any(id=>!g.nearby.Contains(id))){StartSeagulls(watchers);visible=true;}
            g.nearby=near;
            if(g.phase==GullPhase.Resting){g.age=0;return true;}
            g.age+=dt;
            while(g.phase!=GullPhase.Resting && g.age>=BeachSeagulls.Duration(g.phase)){
                g.age-=BeachSeagulls.Duration(g.phase);visible=true;
                if(g.phase==GullPhase.Landing){g.phase=GullPhase.Resting;g.age=0;g.from=g.to;g.calmUntil=g.clock+BeachSeagulls.CalmSeconds;
                    g.nearby=watchers.Where(p=>BeachSeagulls.Near(p,g.from,BeachSeagulls.Radius)).Select(p=>p.id).OrderBy(id=>id,StringComparer.Ordinal).ToArray();}
                else g.phase++;
            }
            return true;
        }
    }
}
