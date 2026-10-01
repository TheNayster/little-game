using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class DaycareMember
    {
        public string actor; public bool attending,carryingPlate,declined;
        public DaycareMember Copy()=>(DaycareMember)MemberwiseClone();
    }
    [Serializable] public sealed class DaycareState
    {
        public double clock, started, helpUntil;
        public int round, phase, plates;
        public DaycareMember[] members;
        public string[] guests;
        public DaycareState Copy()=>new DaycareState{clock=clock,started=started,helpUntil=helpUntil,round=round,phase=phase,plates=plates,guests=guests?.ToArray(),members=members.Select(m=>m.Copy()).ToArray()};
    }
    public static class DaycareTeacher
    {
        public const int Schema=39;
        public static readonly WalkPoint[] Anchors={new WalkPoint(600,240),new WalkPoint(920,260),new WalkPoint(1700,260),new WalkPoint(1400,260),new WalkPoint(1050,260)};
        public static int Routine(DaycareState s)=>Routine(s,s.clock);
        public static int Routine(DaycareState s,double clock)=>s.helpUntil>clock?3:(int)(clock/18)%5;
        public static WalkPoint Point(DaycareState s)=>Point(s,s.clock);
        public static WalkPoint Point(DaycareState s,double clock)
        {
            var routine=(int)(clock/18)%5;var target=s.helpUntil>clock?Anchors[3]:Anchors[routine];
            var from=Anchors[(routine+4)%5];var t=s.helpUntil>clock?1:Math.Min(1,(clock%18)/3);
            return new WalkPoint((float)(from.X+(target.X-from.X)*t),(float)(from.Y+(target.Y-from.Y)*t));
        }
        public static WalkPoint Plate(int index)=>new WalkPoint(1220+index*150,180);
        public static WalkPoint Tray=>new WalkPoint(970,140);
        public static int Count(int mask){var count=0;for(var i=0;i<4;i++)if((mask&(1<<i))!=0)count++;return count;}
    }
    public sealed partial class SoloWorld
    {
        public DaycareState ReadDaycare()=>state.daycare?.Copy();
        public static SoloWorld WithDaycare(SoloWorld world)
        {
            world=WithKingdom(world);if(world.Schema>=DaycareTeacher.Schema && world.state.daycare!=null)return world;
            var s=world.Snapshot();s.daycare=new DaycareState{members=s.players.Select(p=>new DaycareMember{actor=p.id}).ToArray()};s.schema=Math.Max(s.schema,DaycareTeacher.Schema);s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeDaycare(SoloSnapshot s)
        {if(s!=null && s.schema<WorldLayout.Schema && s.daycare!=null && s.daycare.round==0 && s.daycare.clock==0 && (s.daycare.members==null || s.daycare.members.Length==0))s.daycare=null;}
        private static void ValidateDaycare(SoloSnapshot s)
        {
            var d=s.daycare;if(d==null && s.schema<WorldLayout.Schema)return;if(s.schema<DaycareTeacher.Schema){if(d!=null)throw new InvalidOperationException("Daycare requires schema 39.");return;}
            if(d==null || !KeepyRules.Finite(d.clock) || d.clock<0 || !KeepyRules.Finite(d.started) || d.started<0 || d.started>d.clock || !KeepyRules.Finite(d.helpUntil) || d.helpUntil<0 || d.helpUntil>d.clock+6.001 || d.round<0 || d.round>=int.MaxValue || d.phase<0 || d.phase>3 || d.plates<0 || d.plates>15 || d.members==null || !d.members.Select(m=>m?.actor).OrderBy(v=>v).SequenceEqual(s.players.Select(p=>p.id).OrderBy(v=>v)))throw new InvalidOperationException("Invalid daycare checkpoint.");
            if(d.phase==0 && (d.round!=0 || d.plates!=0) || d.phase==3 && d.plates!=15 || d.phase>0 && d.round==0)throw new InvalidOperationException("Invalid picnic progress.");
            foreach(var m in d.members)if(m.attending && (d.phase==0 || s.players.Single(p=>p.id==m.actor).zone!="daycare") || m.carryingPlate && (!m.attending || d.phase!=2))throw new InvalidOperationException("Invalid picnic participant.");
        }
        public bool ReleaseDaycare(string actor)
        {var m=state.daycare?.members.FirstOrDefault(v=>v.actor==actor);if(m==null || !m.attending)return false;m.attending=false;m.carryingPlate=false;state.revision++;return true;}
        public bool JoinPicnicGroup(string[] connected)
        {var d=state.daycare;if(d==null || d.phase==0 || !d.members.Any(m=>m.attending && connected.Contains(m.actor)))return false;var changed=false;foreach(var id in connected){var p=state.players.Single(v=>v.id==id);var m=d.members.Single(v=>v.actor==id);if(WorldLayout.Place(p)!="daycare" || state.sandpit?.members.Any(v=>v.actor==id && v.attending)==true || m.attending || m.declined)continue;if(p.zone==KingdomAdventure.Zone)TravelPlayer(p,"daycare");m.attending=true;p.x=1120+Array.IndexOf(d.members,m)*160;p.y=110;changed=true;}if(changed)state.revision++;return changed;}
        private string DaycareOperation(SoloCommand c,SoloPlayer p)
        {
            var d=state.daycare;if(d==null)return "daycare-unavailable";if(p.zone!="daycare")return "wrong-area";var member=d.members.Single(m=>m.actor==p.id);
            if(c.value=="help"){d.helpUntil=d.clock+6;return null;}
            if(c.value=="leave"){member.attending=false;member.carryingPlate=false;member.declined=true;return null;}
            if(c.value=="start" || c.value=="replay"){
                if(c.value=="replay" && (d.phase!=3 || d.clock-d.started<2))return "picnic-not-finished";
                if(d.phase==0 || c.value=="replay"){if(d.round>=int.MaxValue-1)return "round-limit";if(state.schema>=DaycareNpcCasts.Schema)d.guests=DaycareNpcCasts.Pick(4,d.guests);d.round++;d.phase=1;d.plates=0;d.started=d.clock;foreach(var m in d.members){m.carryingPlate=false;m.declined=false;}}
                ReleaseSandpit(p.id);if(state.sandpit!=null)state.sandpit.members.Single(m=>m.actor==p.id).declined=true;member.attending=true;member.declined=false;p.x=1120+Array.IndexOf(d.members,member)*160;p.y=110;return null;
            }
            if(c.value=="take-plate" && member.attending && d.phase==2){var tray=DaycareTeacher.Tray;if(Math.Abs(p.x-tray.X)>130 || Math.Abs(p.y-tray.Y)>140)return "walk-closer";member.carryingPlate=true;return null;}
            if(c.value!="plate" || !member.attending || d.phase!=2 || !int.TryParse(c.target,out var index) || index<0 || index>3)return "try-current-picnic-step";
            var point=DaycareTeacher.Plate(index);if(Math.Abs(p.x-point.X)>130 || Math.Abs(p.y-point.Y)>140)return "walk-closer";
            if((d.plates&(1<<index))!=0)return "plate-already-placed";
            if(!member.carryingPlate)return "take-a-plate-first";
            member.carryingPlate=false;d.plates|=1<<index;if(d.plates==15){foreach(var m in d.members)m.carryingPlate=false;d.phase=3;d.started=d.clock;}return null;
        }
        private bool AdvanceDaycare(double seconds,string[] activePlayers,out bool visible)
        {
            visible=false;var d=state.daycare;if(d==null || !state.players.Any(p=>p.zone=="daycare" && (activePlayers==null || activePlayers.Contains(p.id))))return false;
            var before=d.clock;var hasPicnic=d.members.Any(m=>m.attending && (activePlayers==null || activePlayers.Contains(m.actor)));
            // Ambient teacher time is shared, but an empty picnic keeps its welcome checkpoint.
            d.clock+=seconds;if(!hasPicnic && d.phase==1)d.started+=seconds;
            if(d.phase==1 && hasPicnic && d.clock-d.started>=3){d.phase=2;d.started=d.clock;visible=true;}
            if((int)(before/18)!=(int)(d.clock/18) || before<d.helpUntil && d.clock>=d.helpUntil)visible=true;
            return true;
        }
    }
}
