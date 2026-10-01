using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class DinosaurMount
    {
        public string species;
        public uint random;
        public float x,y=150,targetX,targetY=150;
        public bool left,wandering;
        public double pause=4,lastCall=-10;
        public int calls,fed,petted;
        public DinosaurMount Copy()=>(DinosaurMount)MemberwiseClone();
    }
    [Serializable] public sealed class DinosaurWorldState
    {
        public double clock;
        public DinosaurMount[] animals;
        public DinosaurCare[] care;public long nextCareTicket;
        public DinosaurWorldState Copy()=>new DinosaurWorldState{clock=clock,animals=animals.Select(a=>a.Copy()).ToArray(),care=care?.Select(f=>f.Copy()).ToArray(),nextCareTicket=nextCareTicket};
    }
    public static class DinosaurRides
    {
        public const int Schema=36;
        public const string Area="dinosaur-world";
        public static readonly string[] Species={"tyrannosaurus","triceratops","brachiosaurus","parasaurolophus"};
        public static string Fixture(string species)=>"dinosaur-"+species;
        public static string SpeciesOf(string fixture)=>Species.FirstOrDefault(s=>Fixture(s)==fixture)??"";
        public static bool Usable(string fixture)=>SpeciesOf(fixture)!="";
        public static WalkPoint Floor(string area,float x,float y)=>area==Area?new WalkPoint(Math.Max(280,Math.Min(4520,x)),Math.Max(70,Math.Min(230,y))):new WalkPoint(x,y);
        public static WalkPoint Step(SoloPlayer p,float x,float y,WalkInput input,float dt,int schema)
        {
            // Both authority and client prediction use the same bounds. Ordinary
            // walking tuning is unchanged; riding uses the accepted baseline too.
            var next=ParkWheels.Step(p,x,y,input,dt,schema);
            return Floor(p.zone,next.X,next.Y);
        }
        internal static double Roll(DinosaurMount a)
        {var n=a.random;n^=n<<13;n^=n>>17;n^=n<<5;a.random=n;return n/(double)uint.MaxValue;}
        public static bool Point(float x,float y)=>!float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && x>=280 && x<=4520 && y>=70 && y<=230;
    }
    public sealed partial class SoloWorld
    {
        public DinosaurWorldState ReadDinosaurWorld()=>state.dinosaurWorld?.Copy();
        public static SoloWorld WithDinosaurWorld(SoloWorld world)
        {
            // Upgrade a ride-only checkpoint before another module raises its
            // schema above the old care boundary. Retain the animals and RNG.
            if(world.state.dinosaurWorld!=null && (world.state.dinosaurWorld.care==null || world.state.dinosaurWorld.care.Length==0)){
                var legacy=world.Snapshot();legacy.dinosaurWorld.care=legacy.players.Select(p=>new DinosaurCare{actor=p.id}).ToArray();
                legacy.schema=Math.Max(legacy.schema,DinosaurCareRules.Schema);legacy.revision++;Validate(legacy);world=new SoloWorld(legacy);
            }
            // Older releases used overlapping schema numbers in separate branches.
            // Ensure each saved module by its own presence, then publish format 42.
            world=WithCreekFishing(world);world=WithNpcCasts(world);world=WithShore(world);
            world=WithStationTidying(world);world=WithBathroom(world);world=WithTag(world);world=WithZoo(world);
            if(world.Schema>=WorldLayout.Schema)return world;
            var s=world.Snapshot();uint seed=2166136261;
            foreach(var ch in s.worldId)seed=unchecked((seed^ch)*16777619);
            if(s.dinosaurWorld==null)s.dinosaurWorld=new DinosaurWorldState{animals=DinosaurRides.Species.Select((id,i)=>new DinosaurMount{species=id,random=(seed^(uint)(i+1)*2654435761u)|1u,x=600+i*1200,targetX=600+i*1200,pause=3+i*2}).ToArray()};
            if(s.dinosaurWorld.care==null || s.dinosaurWorld.care.Length==0)s.dinosaurWorld.care=s.players.Select(p=>new DinosaurCare{actor=p.id}).ToArray();
            s.schema=WorldLayout.Schema;s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeDinosaurInline(SoloSnapshot s)
        {if(s!=null && s.schema<WorldLayout.Schema && s.dinosaurWorld!=null && s.dinosaurWorld.clock==0 && (s.dinosaurWorld.animals==null || s.dinosaurWorld.animals.Length==0))s.dinosaurWorld=null;}
        private static void ValidateDinosaurWorld(SoloSnapshot s)
        {
            if(s.dinosaurWorld==null && s.schema<WorldLayout.Schema)return;
            if(s.schema<DinosaurRides.Schema){if(s.dinosaurWorld!=null || s.players.Any(p=>p.zone==DinosaurRides.Area || DinosaurRides.Usable(p.fixture)))throw new InvalidOperationException("Dinosaur World requires schema 36.");return;}
            var d=s.dinosaurWorld;
            if(d==null || !HideAndSeek.Finite(d.clock) || d.clock<0 || d.animals==null || !d.animals.Select(a=>a?.species).SequenceEqual(DinosaurRides.Species))throw new InvalidOperationException("Invalid Dinosaur World.");
            foreach(var a in d.animals){
                if(a.random==0 || !DinosaurRides.Point(a.x,a.y) || !DinosaurRides.Point(a.targetX,a.targetY) || !HideAndSeek.Finite(a.pause) || a.pause<0 || a.pause>20 || !HideAndSeek.Finite(a.lastCall) || a.lastCall< -10 || a.lastCall>d.clock || a.calls<0 || a.fed<0 || a.petted<0)throw new InvalidOperationException("Invalid dinosaur mount.");
                var rider=s.players.FirstOrDefault(p=>p.fixture==DinosaurRides.Fixture(a.species));
                if(rider!=null && (rider.zone!=DinosaurRides.Area || rider.x!=a.x || rider.y!=a.y))throw new InvalidOperationException("Dinosaur and rider differ.");
            }
            ValidateDinosaurCare(s);
        }
        // Occupancy lives in the existing per-player lease, so disconnect,
        // travel and restored saves release only their own rider automatically.
        private string DinosaurOperation(SoloCommand c,SoloPlayer p)
        {
            var d=state.dinosaurWorld;if(d==null || p.zone!=DinosaurRides.Area)return "come-to-dinosaur-world";
            if(c.value=="take" || c.value=="offer" || c.value=="pet" || c.value=="cancel-care")return DinosaurCareOperation(c,p);
            if(c.value=="off"){
                if(!DinosaurRides.Usable(p.fixture))return "not-riding";
                ClearFixture(p);p.y=Math.Max(70,p.y-65);return null;
            }
            var a=d.animals.FirstOrDefault(v=>v.species==c.target);if(a==null)return "unknown-dinosaur";
            if(c.value=="mount"){
                var fixture=DinosaurRides.Fixture(a.species);
                if(p.fixture==fixture)return null; // A second tap never gets off.
                if(state.players.Any(v=>v.id!=p.id && v.fixture==fixture))return "dinosaur-busy";
                if(d.care!=null && d.care.Any(f=>f.species==a.species))return "dinosaur-taking-care";
                if(state.toys.Any(t=>t.holder==p.id))return "put-down-toy";
                CancelDinosaurCare(p.id);ClearFixture(p);p.fixture=fixture;p.useSeconds=0;p.rideStarted=0;p.x=a.x;p.y=a.y;a.wandering=false;a.pause=4;
                if(d.clock-a.lastCall>=3){a.lastCall=d.clock;if(a.calls<int.MaxValue)a.calls++;}return null;
            }
            if(c.value=="call"){
                if(p.fixture!=DinosaurRides.Fixture(a.species) && Math.Abs(p.x-a.x)>600)return "come-closer";
                if(d.clock-a.lastCall<3)return "dinosaur-speaking";
                a.lastCall=d.clock;if(a.calls<int.MaxValue)a.calls++;return null;
            }
            return "unknown-dinosaur-action";
        }
        private void SyncDinosaurRider(SoloPlayer p,float previousX)
        {
            if(!DinosaurRides.Usable(p.fixture) || state.dinosaurWorld==null)return;
            var a=state.dinosaurWorld.animals.Single(v=>DinosaurRides.Fixture(v.species)==p.fixture);
            if(Math.Abs(p.x-previousX)>.01)a.left=p.x<previousX;
            a.x=p.x;a.y=p.y;a.targetX=p.x;a.targetY=p.y;a.wandering=false;a.pause=4;
        }
        private bool AdvanceDinosaurWorld(double seconds,string[] active,out bool visible)
        {
            visible=false;var d=state.dinosaurWorld;if(d==null)return false;d.clock+=seconds;
            foreach(var p in state.players.Where(p=>DinosaurRides.Usable(p.fixture)))if(active!=null && !active.Contains(p.id)){ClearFixture(p);visible=true;}
            visible|=AdvanceDinosaurCare(seconds,active);
            foreach(var a in d.animals){
                if(AdvanceCaredDinosaur(a,seconds,ref visible))continue;
                if(state.players.Any(p=>p.fixture==DinosaurRides.Fixture(a.species)))continue;
                if(a.pause>0){a.pause=Math.Max(0,a.pause-seconds);continue;}
                if(!a.wandering){
                    // A fresh bounded choice around the current resting point:
                    // no looping route or client-side random divergence.
                    var point=DinosaurRides.Floor(DinosaurRides.Area,a.x-300+(float)DinosaurRides.Roll(a)*600,90+(float)DinosaurRides.Roll(a)*110);
                    a.targetX=point.X;a.targetY=point.Y;a.left=a.targetX<a.x;a.wandering=true;
                }
                var dx=a.targetX-a.x;var dy=a.targetY-a.y;var distance=Math.Sqrt(dx*dx+dy*dy);var step=Math.Min(distance,seconds*42);
                if(distance>0){a.x+=(float)(dx/distance*step);a.y+=(float)(dy/distance*step);}
                if(distance<=step+.01){a.x=a.targetX;a.y=a.targetY;a.wandering=false;a.pause=4+DinosaurRides.Roll(a)*8;}
                // Motion packets carry these continuous positions; no revision churn.
            }
            return true;
        }
    }
}
