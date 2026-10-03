using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum WaveRidePhase { Idle, Boarding, Countdown, Riding, Finished }
    [Serializable] public sealed class WaveRider
    {
        public string actor;public SeaVisitor visitor;public int seat,waves;public float returnX,returnY;
        public WaveRider Copy()=>(WaveRider)MemberwiseClone();
    }
    [Serializable] public sealed class WaveRideState
    {
        public WaveRidePhase phase;public int round,splashes;public double age;public float x=1200;
        public WaveRider[] riders=Array.Empty<WaveRider>();
        public WaveRideState Copy(){var s=(WaveRideState)MemberwiseClone();s.riders=riders.Select(r=>r.Copy()).ToArray();return s;}
    }
    public static class BeachWaveRide
    {
        // Schema44 adds sea rides after the integrated Daycare job fields in43.
        public const int Schema=44,Content=58;
        public const double Countdown=3,Duration=24;
        public static WaveRider Rider(WaveRideState s,string actor)=>s?.riders.FirstOrDefault(r=>r.actor==actor);
        public static int Wave(double age)=>Math.Min(2,(int)(Math.Max(0,age)/8));
        public static bool SplashWindow(double age)=>age>=0 && age<Duration && age%8>=1 && age%8<=5;
        public static float Lift(double age)=>(float)(70*Math.Sin(Math.PI*Math.Max(0,Math.Min(1,(age%8-1)/4))));
    }
    public sealed partial class GameWorld
    {
        public static GameWorld WithWaveRides(GameWorld world)
        {
            world=WithDinosaurWorld(world);if(world.Schema>=BeachWaveRide.Schema)return world;
            var s=world.Snapshot();s.shore.ride=new WaveRideState();s.schema=BeachWaveRide.Schema;s.revision++;Validate(s);return new GameWorld(s);
        }
        private static void ValidateWaveRide(SoloSnapshot s)
        {
            var g=s.shore?.ride;
            if(s.schema<BeachWaveRide.Schema){
                if(g!=null && g.phase==WaveRidePhase.Idle && g.round==0 && g.age==0 && (g.riders==null || g.riders.Length==0))s.shore.ride=null;
                else if(g!=null)throw new InvalidOperationException("Wave ride requires schema 44.");return;
            }
            if(g==null || !Enum.IsDefined(typeof(WaveRidePhase),g.phase) || g.round<0 || g.splashes<0 || g.splashes>12 || !KeepyRules.Finite(g.age) || g.age<0 || g.age>BeachWaveRide.Countdown+BeachWaveRide.Duration || !KeepyRules.Finite(g.x) || g.x<650 || g.x>4150 ||
                g.riders==null || g.riders.Length>4 || g.riders.Any(r=>r==null || !s.players.Any(p=>p.id==r.actor && p.zone=="beach" && p.fixture=="" && p.activity=="") || !Enum.IsDefined(typeof(SeaVisitor),r.visitor) || r.seat<0 || r.seat>3 || r.waves<0 || r.waves>7 || !WorldLayout.Position("beach",s.schema,r.returnX,r.returnY)) ||
                g.riders.Select(r=>r.actor).Distinct().Count()!=g.riders.Length || g.riders.Select(r=>r.seat).Distinct().Count()!=g.riders.Length ||
                ((g.phase==WaveRidePhase.Idle || g.phase==WaveRidePhase.Finished) && g.riders.Length!=0) || (g.phase==WaveRidePhase.Boarding && g.age!=0) || (g.phase==WaveRidePhase.Countdown && g.age>=BeachWaveRide.Countdown) || (g.phase==WaveRidePhase.Riding && g.age<BeachWaveRide.Countdown))
                throw new InvalidOperationException("Invalid shared wave ride.");
        }
        private bool LeaveWaveRide(SoloPlayer p)
        {
            var g=state.shore?.ride;var rider=BeachWaveRide.Rider(g,p.id);if(rider==null)return false;
            p.x=rider.returnX;p.y=rider.returnY;g.riders=g.riders.Where(r=>r.actor!=p.id).ToArray();
            if(g.riders.Length==0){g.phase=WaveRidePhase.Finished;g.age=BeachWaveRide.Countdown+BeachWaveRide.Duration;}
            state.shore.serial++;return true;
        }
        public void ReleaseWaveRide(string actor)
        {var p=state.players.FirstOrDefault(v=>v.id==actor);if(p!=null && LeaveWaveRide(p))state.revision++;}
        private string WaveRideOperation(SoloCommand c,SoloPlayer p)
        {
            var g=state.shore?.ride;if(g==null || p.zone!="beach")return "ride-at-the-beach";
            var own=BeachWaveRide.Rider(g,p.id);
            if(c.value=="ride-leave"){LeaveWaveRide(p);return null;}
            if(c.value=="ride-join"){
                if(g.phase==WaveRidePhase.Countdown || g.phase==WaveRidePhase.Riding)return "join-next-ride";
                if(!int.TryParse(c.target,out var kind) || kind<0 || kind>2)return "unknown-visitor";
                if(own!=null){own.visitor=(SeaVisitor)kind;state.shore.serial++;return null;}
                if(g.riders.Length>=4)return "ride-full";
                if(g.phase!=WaveRidePhase.Boarding){g.round=g.round==int.MaxValue?1:g.round+1;g.phase=WaveRidePhase.Boarding;g.age=0;g.splashes=0;g.x=Math.Max(650,Math.Min(4150,p.x));}
                var slot=Enumerable.Range(0,4).First(i=>!g.riders.Any(r=>r.seat==i));
                var r=new WaveRider{actor=p.id,visitor=(SeaVisitor)kind,seat=slot,returnX=p.x,returnY=p.y};
                foreach(var item in state.toys.Where(t=>t.holder==p.id)){item.holder="";item.x=p.x;item.y=p.y;Touch(item);}
                ClearFixture(p);p.activity="";g.riders=g.riders.Concat(new[]{r}).ToArray();p.x=g.x;p.y=430;state.shore.serial++;return null;
            }
            if(c.value=="ride-start"){
                if(own==null || g.phase!=WaveRidePhase.Boarding || c.target!=g.round.ToString())return "ride-not-ready";
                g.phase=WaveRidePhase.Countdown;g.age=0;state.shore.serial++;return null;
            }
            if(c.value=="ride-wave"){
                var age=g.age-BeachWaveRide.Countdown;var bit=1<<BeachWaveRide.Wave(age);
                if(own==null || g.phase!=WaveRidePhase.Riding || !BeachWaveRide.SplashWindow(age) || (own.waves&bit)!=0 || c.target!=g.round.ToString())return "wait-for-wave";
                own.waves|=bit;g.splashes++;state.shore.serial++;return null;
            }
            return "unknown-ride-action";
        }
        private bool AdvanceWaveRide(double dt,string[] active,out bool visible)
        {
            visible=false;var g=state.shore?.ride;if(g==null)return false;
            foreach(var r in g.riders.ToArray()){
                var p=state.players.First(v=>v.id==r.actor);
                if(p.zone!="beach" || active!=null && !active.Contains(p.id)){LeaveWaveRide(p);visible=true;}
            }
            if(g.phase!=WaveRidePhase.Countdown && g.phase!=WaveRidePhase.Riding)return visible;
            g.age=Math.Min(BeachWaveRide.Countdown+BeachWaveRide.Duration,g.age+dt);
            if(g.phase==WaveRidePhase.Countdown && g.age>=BeachWaveRide.Countdown){g.phase=WaveRidePhase.Riding;state.shore.serial++;visible=true;}
            if(g.age>=BeachWaveRide.Countdown+BeachWaveRide.Duration){
                foreach(var r in g.riders.ToArray())LeaveWaveRide(state.players.First(p=>p.id==r.actor));visible=true;
            }
            return true;
        }
    }
}
