using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        private static void ValidateBirthdayThrows(BirthdayState b)
        {
            foreach(var p in b.props)if(!KeepyRules.Finite(p.flightAt) || p.flightAt< -1 || p.flightAt>b.clock || p.flightTarget==null || p.flightSender==null ||
                !HulkBirthday.Point(p.flightX,p.flightY) || !HulkBirthday.Point(p.flightEndX,p.flightEndY) ||
                p.location=="flying" && (p.flightAt<0 || p.holder!="" || !b.members.Any(m=>m.actor==p.flightSender) ||
                    p.flightTarget!="socket" && !b.members.Any(m=>m.actor==p.flightTarget) && !b.cast.Take(1).Any(n=>n.id==p.flightTarget)))
                throw new InvalidOperationException("Invalid birthday throw.");
        }
        private static void ResetBirthdayThrows(BirthdayState b)
        {
            foreach(var p in b.props.Where(p=>p.location=="flying")){p.x=p.flightEndX;p.y=p.flightEndY;p.location="ground";p.holder="";p.flightAt=-1;p.flightTarget=p.flightSender="";p.version++;}
        }
        private bool BirthdayThrowOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,string[] connected,out string outcome)
        {
            outcome=null;if(c.value!="throw")return false;var b=state.birthday;
            if(held==null || held.location!="held" || member.station!=""){outcome="hold-a-prop-on-the-floor";return true;}
            string target;float x,y;
            if(member.role==HulkBirthday.Roles[0]){
                if(held.kind!="battery"){outcome="hulk-throws-the-battery";return true;}
                target="socket";x=HulkBirthday.BatteryX;y=HulkBirthday.BatteryY;
            }else{
                var hulk=b.cast[0];var humanHulk=b.members.FirstOrDefault(m=>m.role==HulkBirthday.Roles[0] && state.players.Any(p=>p.id==m.actor && p.zone==HulkBirthday.Zone) && (connected==null || connected.Contains(m.actor)));target=humanHulk?.actor??hulk.id;
                var recipient=state.players.FirstOrDefault(p=>p.id==target);x=recipient?.x??hulk.x;y=recipient?.y??hulk.y;
                var human=b.members.FirstOrDefault(m=>m.actor==target);
                if(human!=null && human.station!="" || human==null && hulk.station!="" || b.props.Any(p=>p.holder==target || p.location=="flying" && p.flightTarget==target)){outcome="hulks-hands-are-busy";return true;}
            }
            if(Math.Abs(player.x-x)>1800){outcome="bring-it-closer";return true;}
            // Source HoldSlot protection and throw-apex callbacks become one
            // authority-owned flight. A receiver departure safely drops this
            // same object; it never creates a replacement or relies on a frame.
            held.flightX=player.x;held.flightY=player.y;held.flightEndX=x;held.flightEndY=y;held.flightAt=b.clock;held.flightTarget=target;held.flightSender=player.id;
            held.holder="";held.location="flying";held.petAt=held.fakeAt=-1;held.version++;member.station="throw";BirthdayPose(member,"Throw");return true;
        }
        private bool AdvanceBirthdayThrows(string[] connected)
        {
            var b=state.birthday;bool changed=false;
            foreach(var prop in b.props.Where(p=>p.location=="flying")){
                if(b.clock-prop.flightAt<1.75){AmbientChanged=true;continue;}
                string target=prop.flightTarget;var human=b.members.FirstOrDefault(m=>m.actor==target);var npc=b.cast.FirstOrDefault(n=>n.id==target);
                bool available=human!=null?state.players.Any(p=>p.id==target && p.zone==HulkBirthday.Zone) && (connected==null || connected.Contains(target)) && human.station=="":npc!=null && npc.controller=="" && npc.station=="";
                prop.holder=available && !b.props.Any(p=>p.holder==target)?target:"";prop.location=prop.holder==""?"ground":"held";prop.x=prop.flightEndX;prop.y=prop.flightEndY;
                if(target=="socket"){prop.location="socket";b.powered=true;}
                if(human!=null && prop.holder==target)BirthdayPose(human,"Lift");
                var sender=b.members.First(m=>m.actor==prop.flightSender);if(sender.station=="throw"){sender.station="";BirthdayPose(sender,"Happy");}
                prop.flightAt=-1;prop.flightTarget=prop.flightSender="";prop.version++;changed=true;
            }
            return changed;
        }
    }
}
