using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class ParkState
    {
        public double clock, angle, speed, targetSpeed, waterUntil;
        public ParkState Copy() => (ParkState)MemberwiseClone();
    }
    // Ride coordinates are stable leases. Animated height never enters walking,
    // inventory or collision coordinates; every client evaluates the same clock.
    public static class ParkPlay
    {
        public const int Schema=33;
        public const double SlideSeconds=5, TurnSpeed=.55;
        public const float FountainX=3070, FountainY=250;
        public static readonly string[] Stations={"slide","swing","roundabout","bench","picnic"};
        public static string Slot(string station,int seat)=>"park-"+station+"-"+seat;
        public static int Seat(string id,string station)
        {for(var i=0;i<4;i++)if(id==Slot(station,i))return i;return -1;}
        public static string Station(string id)
        {foreach(var station in Stations)if(Seat(id,station)>=0)return station;return "";}
        public static bool Usable(string id)=>Station(id)!="";
        public static float Center(string station)=>station=="slide"?650:station=="swing"?1510:station=="roundabout"?2440:station=="bench"?4230:3630;
        public static float X(string id)
        {var station=Station(id);var seat=Seat(id,station);return Center(station)+(station=="swing"?(seat-1.5f)*180:(seat-1.5f)*72);}
        public static float Y(string id)=>Station(id)=="slide"?200:Station(id)=="roundabout"?180:220;
        public static double SwingAngle(double age,int seat)
        {
            // Small-angle pendulum: T=2*pi*sqrt(L/g), here an authored 2.2m
            // equivalent and <=15 degrees. Ease in from the resting seat.
            return .24*Math.Min(1,Math.Max(0,age)/1.2)*Math.Sin(age*Math.Sqrt(9.81/2.2));
        }
        public static double TurnAngle(ParkState state,double ahead)
        {var decay=Math.Exp(-2*Math.Max(0,ahead));return state.angle+state.targetSpeed*ahead+(state.speed-state.targetSpeed)*(1-decay)/2;}
    }
    public sealed partial class SoloWorld
    {
        public ParkState ReadPark()=>state.park?.Copy();
        public static SoloWorld WithPark(SoloWorld world)
        {
            world=WithHideAndSeek(world);if(world.Schema>=ParkPlay.Schema)return world;
            var s=world.Snapshot();s.schema=ParkPlay.Schema;s.park=new ParkState();
            s.toys=s.toys.Concat(new[]{
                new SoloToy{id="tap-park",kind=ToyKind.Tap,zone="park",x=ParkPlay.FountainX,y=ParkPlay.FountainY},
                new SoloToy{id="bucket-park",kind=ToyKind.Bucket,zone="park",x=ParkPlay.FountainX+140,y=130}}).ToArray();
            s.revision++;Validate(s);return new SoloWorld(s);
        }
        public static void ValidatePark(SoloSnapshot s)
        {
            if(s.schema<ParkPlay.Schema && s.park!=null && s.park.clock==0 && s.park.angle==0 && s.park.speed==0 && s.park.targetSpeed==0 && s.park.waterUntil==0)s.park=null;
            if(s.schema<ParkPlay.Schema){if(s.park!=null)throw new InvalidOperationException("Park state requires schema 33.");return;}
            var p=s.park;
            if(p==null || !KeepyRules.Finite(p.clock) || p.clock<0 || !KeepyRules.Finite(p.angle) || p.angle<0 || p.angle>Math.PI*2 ||
                !KeepyRules.Finite(p.speed) || p.speed<0 || p.speed>ParkPlay.TurnSpeed || (p.targetSpeed!=0 && p.targetSpeed!=ParkPlay.TurnSpeed) ||
                !KeepyRules.Finite(p.waterUntil) || p.waterUntil<0 || !s.toys.Any(t=>t.id=="tap-park" && t.kind==ToyKind.Tap && t.zone=="park" && t.x==ParkPlay.FountainX && t.y==ParkPlay.FountainY) ||
                !s.toys.Any(t=>t.id=="bucket-park" && t.kind==ToyKind.Bucket && t.zone=="park"))throw new InvalidOperationException("Invalid park state.");
            foreach(var player in s.players)
                if(!KeepyRules.Finite(player.rideStarted) || player.rideStarted<0 || player.rideStarted>p.clock || (!ParkPlay.Usable(player.fixture) && player.rideStarted!=0))throw new InvalidOperationException("Invalid park ride clock.");
        }
        private string ApplyParkFixture(SoloCommand c,SoloPlayer p)
        {
            if(state.park==null)return "wrong-area";
            if(c.action==SoloAction.LeaveFixture){ClearFixture(p);return null;}
            if(c.action!=SoloAction.UseFixture || !ParkPlay.Usable(c.target))return "invalid-fixture";
            if(state.players.Any(v=>v.id!=p.id && v.zone=="park" && v.fixture==c.target))return "fixture-busy";
            foreach(var t in state.toys.Where(t=>t.holder==p.id)){t.holder="";t.container="";t.x=p.x;t.y=Math.Max(35,p.y-65);Touch(t);}
            ClearFixture(p);p.fixture=c.target;p.activity="";p.rideStarted=state.park.clock;p.x=ParkPlay.X(c.target);p.y=ParkPlay.Y(c.target);
            if(ParkPlay.Station(c.target)=="roundabout")state.park.targetSpeed=ParkPlay.TurnSpeed;
            return null;
        }
        private string ParkOperation(SoloCommand c,SoloPlayer p)
        {
            if(state.park==null || p.zone!="park")return "wrong-area";
            if(c.value=="water"){state.park.waterUntil=state.park.clock+4;return null;}
            if(c.value=="turn" || c.value=="stop"){state.park.targetSpeed=c.value=="turn"?ParkPlay.TurnSpeed:0;return null;}
            return "invalid-park-action";
        }
        private bool AdvancePark(double dt,out bool visible)
        {
            visible=false;var park=state.park;if(park==null)return false;
            park.angle=ParkPlay.TurnAngle(park,dt)%(Math.PI*2);
            park.speed=park.targetSpeed+(park.speed-park.targetSpeed)*Math.Exp(-2*dt);park.clock+=dt;
            foreach(var p in state.players.Where(p=>ParkPlay.Station(p.fixture)=="slide"))
                if(park.clock-p.rideStarted>=ParkPlay.SlideSeconds+ParkPlay.Seat(p.fixture,"slide")*.35){ClearFixture(p);p.x=ParkPlay.Center("slide")+294;p.y=198;visible=true;}
            return true;
        }
    }
}
