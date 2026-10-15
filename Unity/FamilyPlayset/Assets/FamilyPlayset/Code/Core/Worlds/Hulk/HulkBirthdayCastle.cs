using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        private static void ValidateBirthdayCastle(BirthdayState b)
        {
            foreach(var m in b.members)if(new[]{m.bounceX,m.bounceH,m.bounceVx,m.bounceVy,m.bounceTargetX,m.bounceTargetH}.Any(v=>!KeepyRules.Finite(v)) ||
                Math.Abs(m.bounceX)>3 || m.bounceH<0 || m.bounceH>6 || Math.Abs(m.bounceVx)>10.001 || Math.Abs(m.bounceVy)>10.001 || Math.Abs(m.bounceTargetX)>2.5 || m.bounceTargetH<0 || m.bounceTargetH>5.5 || m.landings<0 || m.stunts<0 ||
                new[]{m.impactAt,m.stuntAt}.Any(t=>!KeepyRules.Finite(t) || t< -1 || t>b.clock))throw new InvalidOperationException("Invalid birthday castle physics.");
        }
        private bool BirthdayCastleOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,out string outcome)
        {
            outcome=null;
            if(c.value=="station" && c.target=="castle"){
                if(held!=null){outcome="put-down-your-prop";return true;}
                if(Math.Abs(player.x-1169)>InteractionRadius || Math.Abs(player.y-100)>InteractionRadius){outcome="come-to-the-castle";return true;}
                LeaveBirthdayStation(player.id);int index=Array.IndexOf(state.birthday.members,member);
                member.station="castle";member.bounceX=(index-1.5f)*1.1f;member.bounceH=.5f;member.bounceVx=index%2==0?.7f:-.7f;member.bounceVy=6;member.bounceDrag=false;
                player.x=1169+member.bounceX*HulkCastlePixels;player.y=200;BirthdayPose(member,"Jump");return true;
            }
            if(!c.value.StartsWith("castle-",StringComparison.Ordinal))return false;
            if(member.station!="castle"){outcome="join-the-castle";return true;}
            if(c.value=="castle-release"){member.bounceDrag=false;return true;}
            if(c.value=="castle-drag"){
                if(!KeepyRules.Finite(c.x) || !KeepyRules.Finite(c.y) || c.x< -2.5f || c.x>2.5f || c.y<0 || c.y>5.5f){outcome="stay-inside-the-castle";return true;}
                member.bounceDrag=true;member.bounceTargetX=c.x;member.bounceTargetH=c.y;return true;
            }
            if(c.value=="castle-tap"){
                // Native tapVelocity=-10 pushes toward the floor. The floor
                // callback returns at least minBounce=6, with damper=.8.
                member.bounceDrag=false;member.bounceVy=-10;member.stunts++;member.stuntAt=state.birthday.clock;return true;
            }
            outcome="choose-a-castle-action";return true;
        }
        private const float HulkCastlePixels=1200f/19.5f;
        private bool AdvanceBirthdayCastle(double seconds)
        {
            var b=state.birthday;bool changed=false;
            foreach(var m in b.members.Where(m=>m.station=="castle")){
                // Small authority steps preserve the source gravity/collision
                // equations under network ticks. Each child keeps an independent
                // body in this shared castle; no personal round or reset exists.
                double remaining=seconds;
                while(remaining>0){float dt=(float)Math.Min(remaining,1d/60);remaining-=dt;
                    if(m.bounceDrag){m.bounceVx=(float)Math.Clamp((m.bounceTargetX-m.bounceX)*10,-10,10);m.bounceVy=(float)Math.Clamp((m.bounceTargetH-m.bounceH)*10,-10,10);}
                    else m.bounceVy-=9.8f*dt;
                    m.bounceX+=m.bounceVx*dt;m.bounceH+=m.bounceVy*dt;
                    if(m.bounceX< -2.5f){m.bounceX=-2.5f;m.bounceVx=Math.Abs(m.bounceVx)*.8f;}
                    if(m.bounceX>2.5f){m.bounceX=2.5f;m.bounceVx=-Math.Abs(m.bounceVx)*.8f;}
                    if(m.bounceH>5.5f){m.bounceH=5.5f;m.bounceVy=-Math.Abs(m.bounceVy)*.8f;}
                    if(m.bounceH<0){m.bounceH=0;m.bounceVy=Math.Max(6,-m.bounceVy*.8f);m.impactAt=b.clock;m.landings++;}
                    m.bounceVy=(float)Math.Clamp(m.bounceVy,-10,10);
                }
                var p=state.players.Single(p=>p.id==m.actor);p.x=1169+m.bounceX*HulkCastlePixels;p.y=200;changed=true;
            }
            // Physics is continuous. The shared ambient channel publishes at
            // its existing quarter-second cadence; marking every simulation
            // step visible would queue full-world reliable saves/publications.
            if(changed)AmbientChanged=true;
            return false;
        }
    }
}
