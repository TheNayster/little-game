using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        private static void ResetBirthdayBand(BirthdayState b)
        {
            foreach(var npc in b.cast.Where(n=>HulkBirthday.Instrument(n.station) || n.station=="dance")){npc.station="";if(npc.phase=="band-walk")npc.phase="idle";}
            b.bandInviteAt=b.bandRoundAt=-1;b.bandActors=Array.Empty<string>();b.songAt=-1;b.bandBeat=0;
        }
        private static void ValidateBirthdayBand(SoloSnapshot s)
        {
            var b=s.birthday;
            var bodies=b.members.Select(m=>m.actor).Concat(b.cast.Select(n=>n.id)).ToArray();
            if(b.bandRounds<0 || b.lightStep<0 || !KeepyRules.Finite(b.bandBeat) || b.bandBeat<0 || b.bandBeat>b.clock+.001 || b.bandActors==null || b.bandActors.Length>4 || b.bandActors.Distinct().Count()!=b.bandActors.Length || b.bandActors.Any(id=>!bodies.Contains(id)) ||
                new[]{b.bandInviteAt,b.bandEndedAt}.Any(t=>!KeepyRules.Finite(t) || t< -1 || t>b.clock) || !KeepyRules.Finite(b.bandRoundAt) || b.bandRoundAt< -1 || b.bandRoundAt>b.clock+3.001 ||
                new[]{"guitar","keyboard","drums"}.Any(station=>b.members.Count(m=>m.station==station)+b.cast.Count(n=>n.station==station)>1))throw new InvalidOperationException("Invalid shared birthday band.");
        }
        private void LeaveBirthdayBand(string actor)
        {
            var b=state.birthday;b.bandActors=b.bandActors.Where(id=>id!=actor).ToArray();
        }
        private bool BirthdayBandOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,string[] connected,out string outcome)
        {
            var b=state.birthday;outcome=null;
            if(c.value=="socket"){
                if(held==null || held.kind!="battery"){outcome="bring-the-battery";return true;}
                if(Math.Abs(player.x-HulkBirthday.BatteryX)>InteractionRadius || Math.Abs(player.y-HulkBirthday.BatteryY)>InteractionRadius){outcome="come-to-the-battery-socket";return true;}
                held.holder="";held.location="socket";held.x=HulkBirthday.BatteryX;held.y=HulkBirthday.BatteryY;held.version++;b.powered=true;return true;
            }
            bool band=c.value=="band" || c.value=="band-replay";
            bool instrument=c.value=="station" && (HulkBirthday.Instrument(c.target) || c.target=="dance");
            if(!band && !instrument)return false;
            string station=band?HulkBirthday.RoleInstrument(member.role):c.target;
            if(held!=null){outcome="put-down-your-prop";return true;}
            int spot=Array.IndexOf(b.members,member);
            if(Math.Abs(player.x-HulkBirthday.StageX(station,spot))>InteractionRadius || Math.Abs(player.y-HulkBirthday.StageY(station,spot))>InteractionRadius){outcome="come-to-the-band";return true;}
            if(HulkBirthday.Instrument(station) && b.members.Any(m=>m!=member && m.station==station)){outcome="instrument-in-use";return true;}
            if(b.bandInviteAt<0)b.bandActors=Array.Empty<string>();
            foreach(var npc in b.cast.Where(n=>n.role==member.role)){
                npc.station="";if(npc.phase=="band-walk")npc.phase="idle";LeaveBirthdayBand(npc.id);
            }
            // An NPC yields its instrument to a family member. The common song
            // clock is deliberately untouched by that change of participation.
            foreach(var npc in b.cast.Where(n=>n.station==station && HulkBirthday.Instrument(station))){npc.station="";npc.phase="idle";LeaveBirthdayBand(npc.id);}
            LeaveBirthdayStation(player.id);
            member.station=station;BirthdayPose(member,HulkBirthday.Instrument(station)?"PlayInstrument":"Dance");
            player.x=HulkBirthday.StageX(station,spot);player.y=HulkBirthday.StageY(station,spot);
            if(b.bandInviteAt<0)b.bandInviteAt=b.clock;
            if(b.songAt<0){b.songAt=b.clock;b.bandBeat=0;}
            if(b.lightSeed==0){uint seed=2166136261;foreach(char ch in state.worldId)seed=unchecked((seed^ch)*16777619);b.lightSeed=seed|1;}
            if(!b.bandActors.Contains(player.id))b.bandActors=b.bandActors.Append(player.id).ToArray();
            foreach(var npc in b.cast.Take(4)){
                bool human=b.members.Any(m=>m.role==npc.role && state.players.Any(p=>p.id==m.actor && p.zone==HulkBirthday.Zone && (connected==null || connected.Contains(p.id))));
                string target=HulkBirthday.RoleInstrument(npc.role);
                if(human || npc.station!="" || b.props.Any(p=>p.holder==npc.id) || HulkBirthday.Instrument(target) && (b.members.Any(m=>m.station==target) || b.cast.Any(n=>n.station==target)))continue;
                npc.station=target;npc.targetX=HulkBirthday.StageX(target);npc.targetY=HulkBirthday.StageY(target);npc.phase="band-walk";npc.actionAt=b.clock;
                if(!b.bandActors.Contains(npc.id))b.bandActors=b.bandActors.Append(npc.id).ToArray();
            }
            return true;
        }
        private bool AdvanceBirthdayBand(double seconds)
        {
            var b=state.birthday;bool changed=false;
            var battery=b.props.Single(p=>p.kind=="battery");bool power=battery.holder=="" && battery.location=="socket";
            if(power!=b.powered){b.powered=power;changed=true;}
            if(b.songAt>=0)b.bandBeat+=seconds*(b.powered?1:.5);
            bool Playing(string role,string station)=>b.members.Any(m=>m.role==role && m.station==station) || b.cast.Any(n=>n.role==role && n.controller=="" && n.station==station && n.phase!="band-walk");
            bool correct=Playing(HulkBirthday.Roles[1],"keyboard") && Playing(HulkBirthday.Roles[2],"guitar") && Playing(HulkBirthday.Roles[3],"drums");
            if(b.bandInviteAt>=0 && b.bandRoundAt<0 && correct){b.bandRoundAt=b.clock+3;changed=true;}
            if(b.bandRoundAt>=0 && b.clock>=b.bandRoundAt+HulkBirthday.BandSongSeconds){
                var participants=b.bandActors.ToArray();
                foreach(var member in b.members.Where(m=>HulkBirthday.Instrument(m.station) || m.station=="dance")){member.station="";BirthdayPose(member,"Happy");}
                foreach(var npc in b.cast.Where(n=>HulkBirthday.Instrument(n.station) || n.station=="dance")){npc.station="";npc.phase="idle";}
                b.bandRounds++;b.bandEndedAt=b.clock;b.bandRoundAt=b.bandInviteAt=b.songAt=-1;b.bandActors=participants;b.reaction="band-happy";b.reactionAt=b.clock;changed=true;
            }
            bool occupied=b.members.Any(m=>HulkBirthday.Instrument(m.station) || m.station=="dance") || b.cast.Any(n=>(HulkBirthday.Instrument(n.station) || n.station=="dance") && n.phase!="band-walk");
            if(!occupied && b.bandActors.Length==0 && b.bandRoundAt>=0){b.bandRoundAt=b.bandInviteAt=-1;changed=true;}
            if(occupied && b.songAt<0){b.songAt=b.clock;b.bandBeat=0;changed=true;}
            if(!occupied && b.bandRoundAt<0 && b.songAt>=0){b.songAt=-1;changed=true;}
            if(b.powered && (occupied || b.bandRoundAt>=0)){
                int step=(int)Math.Max(0,Math.Floor((b.clock-Math.Max(0,b.songAt))*2));
                if(step!=b.lightStep){b.lightStep=step;changed=true;}
            }
            return changed;
        }
    }
}
