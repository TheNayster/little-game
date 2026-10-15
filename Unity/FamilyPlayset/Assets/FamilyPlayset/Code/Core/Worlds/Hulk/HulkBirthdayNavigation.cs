using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class BirthdayNavigation
    {
        // Projected pegs and endpoints from HBP_Main. Floor positions remain in
        // the existing walkable schema; these separate toy anchors include roofs.
        public static readonly float[] SeatX={43,-55,-182,2564,4003,5025,3801,-182,4387,3886,-162,3922,5025,5025,4345,-182,4259,141,4956};
        public static readonly float[] SeatY={3417,3417,3296,163,-50,3229,-7,3340,4,-12,3417,21,3185,3141,75,3385,15,3417,3243};
        public static readonly int[][] Groups={new[]{0,1,2,7,10,15,17},new[]{5,12,13,18},new[]{3},new[]{4,6,9,11},new[]{8,14,16}};
        public static readonly float[] StartX={4775,281},StartY={3680,3838},EndX={60,4924},EndY={1651,1722};
        public static readonly double[] RideSeconds={9.76,9.63}; // source 3D endpoint distance / speed 8
        public static bool Seated(BirthdayMember m)=>m.seat>=0;
        public static void Point(BirthdayMember m,double clock,out float x,out float y)
        {
            x=0;y=0;
            if(m.station=="zipline"){
                int r=m.route;float t=(float)Math.Clamp((clock-m.actionAt)/RideSeconds[r],0,1);
                x=StartX[r]+(EndX[r]-StartX[r])*t;y=StartY[r]+(EndY[r]-StartY[r])*t;
            }else if(m.seat>=0){x=SeatX[m.seat];y=SeatY[m.seat];}
        }
    }
    public sealed partial class GameWorld
    {
        private static void ValidateBirthdayNavigation(BirthdayState b)
        {
            if(b.members.Any(m=>m.seat< -1 || m.seat>=19 || m.route< -1 || m.route>1 ||
                (m.station=="sit" || m.station=="sleep") && m.seat<0 || m.station=="zipline" && m.route<0 ||
                m.seat>=0 && !new[]{"sit","sleep","eat","drink","unwrap"}.Contains(m.station) || m.route>=0 && m.station!="zipline") ||
                b.members.Where(m=>m.seat>=0).GroupBy(m=>m.seat).Any(g=>g.Count()>1))throw new InvalidOperationException("Invalid birthday seating or zipline.");
        }
        private bool BirthdayNavigationOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,out string outcome)
        {
            outcome=null;var b=state.birthday;
            if(c.value=="seat"){
                if(!int.TryParse(c.target,out int group) || group<0 || group>=BirthdayNavigation.Groups.Length){outcome="choose-a-seat";return true;}
                var free=BirthdayNavigation.Groups[group].Where(i=>!b.members.Any(m=>m!=member && m.seat==i)).ToArray();
                if(free.Length==0){outcome="seat-in-use";return true;}
                // Tapping again cycles actual free pegs, instead of placing many
                // overlapping controls on the small original chairs.
                int seat=free.FirstOrDefault(i=>i>member.seat);if(!free.Contains(seat))seat=free[0];
                LeaveBirthdayStation(player.id);member.seat=seat;member.route=-1;member.station="sit";BirthdayPose(member,"Sit");
                player.x=Math.Clamp(BirthdayNavigation.SeatX[seat],0,4800);player.y=100;return true;
            }
            if(c.value=="nap"){
                if(member.seat<0 || held!=null){outcome="sit-with-empty-hands";return true;}
                member.station=member.station=="sleep"?"sit":"sleep";BirthdayPose(member,member.station);return true;
            }
            if(c.value!="zipline")return false;
            if(!int.TryParse(c.target,out int route) || route<0 || route>1){outcome="choose-a-zipline";return true;}
            if(held!=null){outcome="put-down-your-prop";return true;}
            LeaveBirthdayStation(player.id);member.seat=-1;member.route=route;member.station="zipline";BirthdayPose(member,"Hang");
            player.x=Math.Clamp(BirthdayNavigation.StartX[route],0,4800);player.y=100;return true;
        }
        private bool AdvanceBirthdayNavigation()
        {
            var b=state.birthday;bool changed=false;
            foreach(var m in b.members.Where(m=>m.station=="zipline")){
                if(b.clock-m.actionAt<BirthdayNavigation.RideSeconds[m.route]){AmbientChanged=true;continue;}
                var p=state.players.Single(p=>p.id==m.actor);p.x=Math.Clamp(BirthdayNavigation.EndX[m.route],0,4800);p.y=100;
                m.route=-1;m.station="";BirthdayPose(m,"Happy");changed=true;
            }
            return changed;
        }
    }
}
