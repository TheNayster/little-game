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
    public sealed partial class GameWorld
    {
        public SeagullState ReadSeagulls()=>state.seagulls?.Copy();
        public static GameWorld WithSeagulls(GameWorld world)
        {
            world=WithOutfits(world);if(world.Schema>=BeachSeagulls.Schema && world.state.seagulls!=null)return world;
            var s=world.Snapshot();s.seagulls=new SeagullState();s.schema=Math.Max(s.schema,BeachSeagulls.Schema);s.revision++;
            Validate(s);return new GameWorld(s);
        }
        private static void ValidateSeagulls(SoloSnapshot s)=>Worlds.Beach.BeachFlockRules.Validate(s);
        private void StartSeagulls(SoloPlayer[] watchers)=>Worlds.Beach.BeachFlockRules.Start(state.seagulls,watchers);
        private string SeagullOperation(SoloCommand c,SoloPlayer p)
        {
            if(c.value.StartsWith("ride-",StringComparison.Ordinal))return WaveRideOperation(c,p);
            if(c.value=="ripple")return ShoreOperation(c,p);
            var g=state.seagulls;if(g==null || c.value!="hello")return "unknown-beach-action";
            if(c.target!="flock" || !BeachSeagulls.Near(p,g.from,BeachSeagulls.TapRadius))return "walk-near-the-birds";
            if(g.phase!=GullPhase.Resting || g.clock<g.calmUntil)return "let-birds-settle";
            StartSeagulls(state.players.Where(v=>v.zone=="beach").ToArray());return null;
        }
        private bool AdvanceSeagulls(double dt,string[] active,out bool visible)=>Worlds.Beach.BeachFlockRules.Advance(state.seagulls,dt,state.players,active,out visible);
    }
}
