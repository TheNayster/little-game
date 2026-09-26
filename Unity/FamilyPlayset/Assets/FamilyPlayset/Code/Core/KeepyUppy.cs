using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class KeepyState
    {
        // One balloon per home. Height is separate from the floor's depth axis.
        public int phase, round, hitSerial;
        public float x=-4240, y=180, height=28, vx, vz, centerX;
        public double elapsed, remainder, hitAge=10;
        public string lastHitter="";
        public bool hitLeft;
        public KeepyState Copy()=>(KeepyState)MemberwiseClone();
    }
    public static class KeepyRules
    {
        public const string Activity="keepy-uppy";
        public const double Step=1.0/60;
        public const float Radius=28, ReachX=76, ReachDepth=95, HalfWidth=440;
        public static float HandHeight(string avatar)=>avatar=="orange-pup"?108:132;
        public static bool Under(SoloPlayer p,KeepyState b)=>p.zone=="garden" &&
            string.IsNullOrEmpty(p.fixture) && Math.Abs(p.x-b.x)<=ReachX && Math.Abs(p.y-b.y)<=ReachDepth;
        public static bool Finite(double n)=>!double.IsNaN(n) && !double.IsInfinity(n);
    }
    public sealed partial class SoloWorld
    {
        public KeepyState ReadKeepy()=>state.keepy?.Copy();
        public static SoloWorld WithKeepyUppy(SoloWorld world)
        {
            world=WithHome(world);if(world.Schema>=5)return world;
            var copy=world.Snapshot();copy.schema=5;copy.keepy=new KeepyState();copy.revision++;
            Validate(copy);return new SoloWorld(copy);
        }
        private string StartKeepy(SoloPlayer p)
        {
            if(state.keepy==null || p.zone!="garden")return "wrong-area";
            var b=state.keepy;
            if(b.phase!=1)
            {
                if(b.round>=1000000)return "round-limit";
                b.round++;b.phase=1;b.elapsed=0;b.remainder=0;b.lastHitter="";b.hitAge=10;
                b.centerX=Math.Max(-4300,Math.Min(4300,b.x));
                b.height=KeepyRules.Radius;b.vz=310;b.vx=55;
            }
            return null;
        }
        public static void ValidateKeepy(SoloSnapshot s)
        {
            var b=s.keepy;
            if(s.schema<5)
            {
                // Unity can deserialize an absent inline class as its defaults.
                if(s.players.Any(p=>p.activity==KeepyRules.Activity) || b!=null && (b.phase!=0 || b.round!=0 || b.hitSerial!=0))
                    throw new InvalidOperationException("Keepy Uppy requires schema 5.");
                return;
            }
            if(b==null || b.phase<0 || b.phase>1 || b.round<0 || b.round>1000000 || b.hitSerial<0 || b.hitSerial>100000000 ||
                !WorldLayout.Position("garden",5,b.x,b.y) || !KeepyRules.Finite(b.centerX) || Math.Abs(b.centerX)>4300 ||
                !KeepyRules.Finite(b.height) || b.height<28 || b.height>600 || !KeepyRules.Finite(b.vx) || Math.Abs(b.vx)>200 ||
                !KeepyRules.Finite(b.vz) || Math.Abs(b.vz)>350 || !KeepyRules.Finite(b.elapsed) || b.elapsed<0 || b.elapsed>86400 ||
                !KeepyRules.Finite(b.remainder) || b.remainder<0 || b.remainder>=KeepyRules.Step+.000001 ||
                !KeepyRules.Finite(b.hitAge) || b.hitAge<0 || b.hitAge>10 || b.lastHitter==null ||
                b.lastHitter!="" && !s.players.Any(p=>p.id==b.lastHitter) ||
                s.players.Any(p=>p.activity==KeepyRules.Activity && (p.zone!="garden" || !string.IsNullOrEmpty(p.fixture))))
                throw new InvalidOperationException("Invalid Keepy Uppy state.");
        }
        private bool AdvanceKeepy(double seconds,string[] activePlayers)
        {
            var b=state.keepy;
            if(b==null || b.phase!=1 || !state.players.Any(p=>p.zone=="garden" && (activePlayers==null || activePlayers.Contains(p.id))))return false;
            b.remainder+=seconds;
            while(b.remainder+1e-10>=KeepyRules.Step && b.phase==1)
            {
                b.remainder=Math.Max(0,b.remainder-KeepyRules.Step);
                var dt=(float)KeepyRules.Step;b.elapsed=Math.Min(86400,b.elapsed+KeepyRules.Step);b.hitAge=Math.Min(10,b.hitAge+KeepyRules.Step);
                // Air-filled balloon: gravity opposed by quadratic drag, with
                // a small deterministic breeze. Tuning is in game units.
                var oldHeight=b.height;
                b.vz+=(-120-.004f*b.vz*Math.Abs(b.vz))*dt;
                b.vx+=((float)Math.Sin(b.elapsed*1.25+b.round)*12-.32f*b.vx)*dt;
                b.height+=b.vz*dt;b.x+=b.vx*dt;
                var ceiling=480-b.y*.45f;
                if(b.height>ceiling){b.height=ceiling;b.vz=-Math.Abs(b.vz)*.25f;}
                var left=Math.Max(-4760,b.centerX-KeepyRules.HalfWidth);var right=Math.Min(4760,b.centerX+KeepyRules.HalfWidth);
                if(b.x<left){b.x=left;b.vx=Math.Abs(b.vx)*.65f;}
                if(b.x>right){b.x=right;b.vx=-Math.Abs(b.vx)*.65f;}
                // Only a descending crossing hits. The closest eligible child
                // wins ties stably, so two siblings never add two impulses.
                SoloPlayer hit=null;var distance=float.MaxValue;
                if(b.vz<0 && b.hitAge>.45)
                    foreach(var p in state.players)
                    {
                        var contact=KeepyRules.HandHeight(p.avatar)+KeepyRules.Radius+(p.y-b.y)*.45f;
                        if(activePlayers!=null && !activePlayers.Contains(p.id) || !KeepyRules.Under(p,b) || state.toys.Any(t=>t.holder==p.id) || oldHeight<contact || b.height>contact)continue;
                        var d=Math.Abs(p.x-b.x);
                        if(d<distance || d==distance && string.CompareOrdinal(p.id,hit?.id)<0){hit=p;distance=d;}
                    }
                if(hit!=null)
                {
                    b.height=KeepyRules.HandHeight(hit.avatar)+KeepyRules.Radius+(hit.y-b.y)*.45f;b.vz=245;
                    var offset=b.x-hit.x;var sign=Math.Abs(offset)>12?Math.Sign(offset):(b.hitSerial%2==0?1:-1);
                    b.vx=sign*(65+Math.Min(45,Math.Abs(offset)*.6f));
                    b.hitSerial=Math.Min(100000000,b.hitSerial+1);
                    b.lastHitter=hit.id;b.hitAge=0;b.hitLeft=sign<0;
                }
                if(b.height<=KeepyRules.Radius || b.elapsed>=86400)
                {b.height=KeepyRules.Radius;b.vx=0;b.vz=0;b.phase=0;b.remainder=0;}
            }
            return true;
        }
    }
}
