using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum CreekBoatPhase { Ready, Floating, Docked, Returning }
    [Serializable] public sealed class CreekBoat
    {
        public string actor;
        public int slot, hull, colour, dock, trips;
        public bool passenger, flower, attending;
        public CreekBoatPhase phase;
        public double started;
        public CreekBoat Copy()=>(CreekBoat)MemberwiseClone();
    }
    [Serializable] public sealed class CreekBoatState
    {
        public double clock;
        public CreekBoat[] boats;
        public CreekBoatState Copy()=>new CreekBoatState{clock=clock,boats=boats.Select(b=>b.Copy()).ToArray()};
    }
    public static class CreekBoats
    {
        public const int Schema=37;
        public const float CentreX=1330, BankY=180;
        public const double Voyage=14, ReturnTime=5;
        public static float BankX(int slot)=>850+slot*155;
        public static WalkPoint Dock(int dock)=>new WalkPoint(dock==0?1670:2000,dock==0?-108:-87);
        public static WalkPoint Point(CreekBoat boat,double clock)
        {
            var from=new WalkPoint(720+boat.slot*110,-112+boat.slot*7);var to=Dock(boat.dock);
            if(boat.phase==CreekBoatPhase.Ready)return from;
            if(boat.phase==CreekBoatPhase.Docked)return new WalkPoint(to.X-boat.slot*47,to.Y+boat.slot*3);
            var t=Math.Max(0,Math.Min(1,(clock-boat.started)/(boat.phase==CreekBoatPhase.Returning?ReturnTime:Voyage)));
            if(boat.phase==CreekBoatPhase.Returning)t=1-t;
            // One authority clock describes the voyage on every client. Gentle
            // authored bends keep boats in the painted water and out of rocks.
            return new WalkPoint((float)(from.X+(to.X-boat.slot*47-from.X)*t),
                (float)(from.Y+(to.Y+boat.slot*3-from.Y)*t+Math.Sin(t*Math.PI)*8));
        }
    }
    public sealed partial class SoloWorld
    {
        public CreekBoatState ReadCreekBoats()=>state.creekBoats?.Copy();
        public static SoloWorld WithCreekBoats(SoloWorld world)
        {
            world=WithPond(world);if(world.Schema>=CreekBoats.Schema)return world;
            var s=world.Snapshot();s.creekBoats=new CreekBoatState{boats=s.players.Select((p,i)=>new CreekBoat{actor=p.id,slot=i,hull=i%3,colour=i%4}).ToArray()};
            s.schema=CreekBoats.Schema;s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeCreekBoats(SoloSnapshot s)
        {
            if(s!=null && s.schema<CreekBoats.Schema && s.creekBoats!=null && s.creekBoats.clock==0 &&
                (s.creekBoats.boats==null || s.creekBoats.boats.Length==0))s.creekBoats=null;
        }
        private static void ValidateCreekBoats(SoloSnapshot s)
        {
            var g=s.creekBoats;if(s.schema<CreekBoats.Schema){if(g!=null)throw new InvalidOperationException("Creek boats require schema 37.");return;}
            if(g==null || !KeepyRules.Finite(g.clock) || g.clock<0 || g.boats==null || g.boats.Length!=s.players.Length ||
                !g.boats.Select(b=>b?.actor).OrderBy(id=>id).SequenceEqual(s.players.Select(p=>p.id).OrderBy(id=>id)) ||
                !g.boats.Select(b=>b?.slot).OrderBy(v=>v).SequenceEqual(Enumerable.Range(0,g.boats.Length).Select(i=>(int?)i)))
                throw new InvalidOperationException("Invalid shared creek boat record.");
            foreach(var b in g.boats){var player=s.players.Single(p=>p.id==b.actor);
                if(b.hull<0 || b.hull>2 || b.colour<0 || b.colour>3 || b.dock<0 || b.dock>1 || b.trips<0 || b.trips==int.MaxValue ||
                    !Enum.IsDefined(typeof(CreekBoatPhase),b.phase) || !KeepyRules.Finite(b.started) || b.started<0 || b.started>g.clock ||
                    b.attending && player.zone!="creek")throw new InvalidOperationException("Invalid creek boat.");
            }
        }
        private static void SuspendCreekBoats(SoloSnapshot s)
        {
            if(s.creekBoats==null)return;var changed=false;
            foreach(var b in s.creekBoats.boats){changed|=b.attending;b.attending=false;
                // A reopened moving creation waits at a reachable landing. Never
                // restart a voyage from wall time or discard its decorations.
                if(b.phase==CreekBoatPhase.Floating){b.phase=CreekBoatPhase.Docked;changed=true;}
                else if(b.phase==CreekBoatPhase.Returning){b.phase=CreekBoatPhase.Ready;changed=true;}
            }
            if(changed)s.revision++;
        }
        private bool CancelCreekBoats(string actor)
        {var b=state.creekBoats?.boats.FirstOrDefault(v=>v.actor==actor);if(b==null || !b.attending)return false;b.attending=false;return true;}
        public bool ReleaseCreekBoats(string actor){if(!CancelCreekBoats(actor))return false;state.revision++;return true;}
        private string CreekBoatOperation(SoloCommand c,SoloPlayer player)
        {
            var g=state.creekBoats;if(g==null)return "boats-unavailable";var b=g.boats.Single(v=>v.actor==player.id);
            if(!string.IsNullOrEmpty(c.item) && c.item!=player.id)return "another-players-boat";
            if(c.value=="leave"){CancelCreekBoats(player.id);return null;}
            if(c.value=="start"){
                if(player.visit>=long.MaxValue-1)return "visit-limit";
                ReleaseHideAndSeek(player.id);TravelPlayer(player,"creek");player.x=CreekBoats.BankX(b.slot);player.y=CreekBoats.BankY;b.attending=true;return null;
            }
            if(!b.attending || player.zone!="creek" || Math.Abs(player.x-CreekBoats.BankX(b.slot))>190 || Math.Abs(player.y-CreekBoats.BankY)>130)return "come-to-boat-launch";
            if(c.value=="retrieve"){
                if(b.phase!=CreekBoatPhase.Docked)return "boat-not-docked";b.phase=CreekBoatPhase.Returning;b.started=g.clock;return null;
            }
            if(b.phase!=CreekBoatPhase.Ready)return "boat-is-sailing";
            if(c.value=="hull" || c.value=="colour" || c.value=="dock"){
                if(!int.TryParse(c.target,out var choice) || choice<0 || choice>(c.value=="hull"?2:c.value=="colour"?3:1))return "invalid-boat-choice";
                if(c.value=="hull")b.hull=choice;else if(c.value=="colour")b.colour=choice;else b.dock=choice;return null;
            }
            if(c.value=="passenger"){b.passenger=!b.passenger;return null;}
            if(c.value=="flower"){b.flower=!b.flower;return null;}
            if(c.value=="launch"){
                if(b.trips>=int.MaxValue-1)return "boat-trip-limit";b.phase=CreekBoatPhase.Floating;b.started=g.clock;return null;
            }
            return "unknown-boat-action";
        }
        private bool AdvanceCreekBoats(double seconds,string[] active,out bool visible)
        {
            visible=false;var g=state.creekBoats;if(g==null)return false;g.clock+=seconds;
            foreach(var b in g.boats){var player=state.players.Single(p=>p.id==b.actor);
                if(b.attending && (player.zone!="creek" || Math.Abs(player.x-CreekBoats.BankX(b.slot))>190 || Math.Abs(player.y-CreekBoats.BankY)>130 || active!=null && !active.Contains(b.actor)))visible|=CancelCreekBoats(b.actor);
                if(b.phase==CreekBoatPhase.Floating && g.clock-b.started>=CreekBoats.Voyage){b.phase=CreekBoatPhase.Docked;b.trips++;visible=true;}
                else if(b.phase==CreekBoatPhase.Returning && g.clock-b.started>=CreekBoats.ReturnTime){b.phase=CreekBoatPhase.Ready;visible=true;}
            }
            return true;
        }
    }
}
