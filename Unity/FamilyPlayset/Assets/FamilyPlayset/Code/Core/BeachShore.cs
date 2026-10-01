using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum SeaVisitor { Whale, Dolphin, Mermaid }
    [Serializable] public sealed class SandPrint
    {
        public string actor; public float x,y,angle; public bool left;
        public double born,washed=-1;
        public SandPrint Copy()=>(SandPrint)MemberwiseClone();
    }
    [Serializable] public sealed class ShoreWalker
    {
        public string actor; public float remainder; public bool left;
        public ShoreWalker Copy()=>(ShoreWalker)MemberwiseClone();
    }
    [Serializable] public sealed class ShoreRipple
    {
        public string actor;public float x,y;public double born;
        public ShoreRipple Copy()=>(ShoreRipple)MemberwiseClone();
    }
    [Serializable] public sealed class BeachShoreState
    {
        public double clock,nextVisit,visitStarted;public uint random;public int visits;public long serial;
        public SeaVisitor visitor;public float visitX;public bool right=true;
        public SandPrint[] prints=Array.Empty<SandPrint>();
        public ShoreWalker[] walkers=Array.Empty<ShoreWalker>();
        public ShoreRipple[] ripples=Array.Empty<ShoreRipple>();
        public BeachShoreState Copy(){var s=(BeachShoreState)MemberwiseClone();s.prints=prints.Select(p=>p.Copy()).ToArray();s.walkers=walkers.Select(p=>p.Copy()).ToArray();s.ripples=ripples.Select(p=>p.Copy()).ToArray();return s;}
    }
    public static class BeachShore
    {
        // IDs above the concurrent pond/boats/wheels candidates; integration
        // must retain every candidate's fields rather than reuse their schemas.
        public const int Schema=40,Content=46,PrintsPerPlayer=48;
        public const double WaveSeconds=9,PrintSeconds=180,WashSeconds=1.3,VisitSeconds=4.8,MinInterval=18,MaxInterval=42;
        public const float WetY=430,SeaY=565,Step=30;
        public static float WaterY(double clock)=>SeaY-(SeaY-WetY)*(float)(.5-.5*Math.Cos(clock*2*Math.PI/WaveSeconds));
        public static bool Visiting(BeachShoreState s)=>s!=null && s.visits>0 && s.clock-s.visitStarted<VisitSeconds;
        public static float PrintAlpha(SandPrint p,double clock)=>(float)Math.Max(0,Math.Min(1,p.washed>=0?1-(clock-p.washed)/WashSeconds:(PrintSeconds-(clock-p.born))/12));
        internal static double Random(BeachShoreState s){var x=s.random;x^=x<<13;x^=x>>17;x^=x<<5;s.random=x;return x/(double)uint.MaxValue;}
    }
    public sealed partial class SoloWorld
    {
        public BeachShoreState ReadShore()=>state.shore?.Copy();
        public static SoloWorld WithShore(SoloWorld world)
        {
            world=WithSeagulls(world);if(world.Schema>=BeachShore.Schema && world.state.shore!=null)return world;
            var s=world.Snapshot();var seed=2166136261u;
            foreach(var ch in s.worldId)seed=unchecked((seed^ch)*16777619u);
            s.shore=new BeachShoreState{random=seed==0?1:seed};
            s.shore.nextVisit=BeachShore.MinInterval+BeachShore.Random(s.shore)*(BeachShore.MaxInterval-BeachShore.MinInterval);
            s.schema=Math.Max(s.schema,BeachShore.Schema);s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void ValidateShore(SoloSnapshot s)
        {
            if(s.shore==null && s.schema<WorldLayout.Schema)return;
            if(s.schema<BeachShore.Schema){
                // JsonUtility may materialize absent nested records as zeroes.
                var old=s.shore;if(old!=null && old.clock==0 && old.random==0 && old.nextVisit==0 && old.visits==0 && (old.prints==null || old.prints.Length==0))s.shore=null;
                if(s.shore!=null)throw new InvalidOperationException("Shore requires schema 40.");return;
            }
            var g=s.shore;
            bool Number(double v)=>KeepyRules.Finite(v) && v>=0;
            bool Actor(string id)=>s.players.Any(p=>p.id==id);
            bool Point(float x,float y)=>WorldLayout.Position("beach",s.schema,x,y);
            if(g==null || !Number(g.clock) || !Number(g.nextVisit) || g.nextVisit<g.clock || g.nextVisit-g.clock>BeachShore.MaxInterval+BeachShore.VisitSeconds+.01 || !Number(g.visitStarted) || g.visitStarted>g.clock || g.random==0 || g.visits<0 || !Enum.IsDefined(typeof(SeaVisitor),g.visitor) || !KeepyRules.Finite(g.visitX) || g.visitX<0 || g.visitX>4800 ||
                g.prints==null || g.prints.Length>4*BeachShore.PrintsPerPlayer || g.prints.Any(p=>p==null || !Actor(p.actor) || !Point(p.x,p.y) || !KeepyRules.Finite(p.angle) || Math.Abs(p.angle)>180 || !Number(p.born) || p.born>g.clock || !KeepyRules.Finite(p.washed) || p.washed< -1 || p.washed>g.clock) || g.prints.GroupBy(p=>p.actor).Any(p=>p.Count()>BeachShore.PrintsPerPlayer) ||
                g.walkers==null || g.walkers.Length>4 || g.walkers.Any(p=>p==null || !Actor(p.actor) || !KeepyRules.Finite(p.remainder) || p.remainder<0 || p.remainder>=BeachShore.Step) || g.walkers.Select(p=>p.actor).Distinct().Count()!=g.walkers.Length ||
                g.ripples==null || g.ripples.Length>4 || g.ripples.Any(p=>p==null || !Actor(p.actor) || !Point(p.x,p.y) || p.y<BeachShore.WetY || !Number(p.born) || p.born>g.clock) || g.ripples.Select(p=>p.actor).Distinct().Count()!=g.ripples.Length)
                throw new InvalidOperationException("Invalid shared beach shore.");
        }
        private void BeachFootsteps(SoloPlayer p,float x,float y)
        {
            var g=state.shore;if(g==null || p.zone!="beach" || p.fixture!="")return;
            var dx=x-p.x;var dy=y-p.y;var distance=(float)Math.Sqrt(dx*dx+dy*dy);if(distance<.001)return;
            var walker=g.walkers.FirstOrDefault(w=>w.actor==p.id);
            if(walker==null){walker=new ShoreWalker{actor=p.id};g.walkers=g.walkers.Concat(new[]{walker}).ToArray();}
            // Commands can reposition a verifier/player. Do not draw a trail
            // across a whole panorama for that jump; ordinary motion is sampled.
            if(distance>250){walker.remainder=0;return;}
            var list=g.prints.ToList();var angle=(float)(Math.Atan2(-dx,dy)*180/Math.PI);
            for(var at=BeachShore.Step-walker.remainder;at<=distance+.001f;at+=BeachShore.Step){
                var t=at/distance;walker.left=!walker.left;
                var py=p.y+dy*t;list.Add(new SandPrint{actor=p.id,x=p.x+dx*t,y=py,angle=angle,left=walker.left,born=g.clock,washed=py>=BeachShore.WaterY(g.clock)?g.clock:-1});
                if(list.Count(v=>v.actor==p.id)>BeachShore.PrintsPerPlayer)list.RemoveAt(list.FindIndex(v=>v.actor==p.id));
            }
            walker.remainder=(walker.remainder+distance)%BeachShore.Step;if(list.Count!=g.prints.Length || list.LastOrDefault()?.born==g.clock)g.serial++;g.prints=list.ToArray();
        }
        private string ShoreOperation(SoloCommand c,SoloPlayer p)
        {
            var g=state.shore;if(g==null || c.value!="ripple")return "unknown-beach-action";
            if(p.zone!="beach" || c.target!="water" || !WorldLayout.Position("beach",state.schema,c.x,c.y) || c.y<BeachShore.WetY || Math.Abs(c.x-p.x)>320 || Math.Abs(c.y-p.y)>320)return "walk-near-the-water";
            var previous=g.ripples.FirstOrDefault(v=>v.actor==p.id);
            if(previous!=null && g.clock-previous.born<.35)return "ripple-settling";
            g.ripples=g.ripples.Where(v=>v.actor!=p.id).Concat(new[]{new ShoreRipple{actor=p.id,x=c.x,y=c.y,born=g.clock}}).ToArray();g.serial++;return null;
        }
        private bool AdvanceShore(double dt,string[] active,out bool visible)
        {
            visible=false;var g=state.shore;if(g==null)return false;
            var watchers=state.players.Where(p=>p.zone=="beach" && (active==null || active.Contains(p.id))).OrderBy(p=>p.id,StringComparer.Ordinal).ToArray();
            if(watchers.Length==0)return false;
            g.clock+=dt;var water=BeachShore.WaterY(g.clock);
            foreach(var print in g.prints)if(print.washed<0 && print.y>=BeachShore.WetY && print.y>=water){print.washed=g.clock;g.serial++;}
            var prints=g.prints.Where(p=>BeachShore.PrintAlpha(p,g.clock)>0).ToArray();var ripples=g.ripples.Where(p=>g.clock-p.born<2).ToArray();
            if(prints.Length!=g.prints.Length || ripples.Length!=g.ripples.Length)g.serial++;g.prints=prints;g.ripples=ripples;
            if(g.clock>=g.nextVisit){
                var kind=(int)(BeachShore.Random(g)*3);kind=Math.Min(2,kind);
                if(g.visits>0 && kind==(int)g.visitor)kind=(kind+1+(int)(BeachShore.Random(g)*2))%3;
                g.visitor=(SeaVisitor)kind;g.visitStarted=g.clock;g.visits=g.visits==int.MaxValue?1:g.visits+1;
                // Anchor near a currently occupied stretch. This is a single
                // world event; another player's area exit cannot reseed it.
                var anchor=watchers[Math.Min(watchers.Length-1,(int)(BeachShore.Random(g)*watchers.Length))];
                g.visitX=Math.Max(260,Math.Min(4540,anchor.x+(float)(BeachShore.Random(g)-.5)*500));g.right=BeachShore.Random(g)>.5;
                g.nextVisit=g.clock+BeachShore.VisitSeconds+BeachShore.MinInterval+BeachShore.Random(g)*(BeachShore.MaxInterval-BeachShore.MinInterval);g.serial++;visible=true;
            }
            return true;
        }
    }
}
