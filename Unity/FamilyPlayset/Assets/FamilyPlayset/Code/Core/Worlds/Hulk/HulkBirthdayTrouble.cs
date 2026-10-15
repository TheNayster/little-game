using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class BirthdayVillain
    {
        public string id,role,phase="mingle",item="";
        public float x,y,targetX,targetY;
        public double phaseAt,trapUntil=-1;
        public string[] webbers=Array.Empty<string>();
        public BirthdayVillain Copy(){var copy=(BirthdayVillain)MemberwiseClone();copy.webbers=webbers.ToArray();return copy;}
    }
    public sealed partial class GameWorld
    {
        private static int BirthdayRandom(BirthdayState b,int count){b.troubleSeed=unchecked(b.troubleSeed*1664525+1013904223);return(int)(b.troubleSeed%(uint)count);}
        private static void ValidateBirthdayTrouble(SoloSnapshot snapshot)
        {
            var b=snapshot.birthday;var defaults=HulkBirthday.CreateVillains();
            if(b.troublesStopped<0 || b.heroActor==null || b.heroTarget==null || b.helperActor==null || b.helperTarget==null || b.helperActor!="" && (!b.cast.Skip(1).Take(3).Any(n=>n.id==b.helperActor) || !b.villains.Any(v=>v.id==b.helperTarget)) || new[]{b.heroAt,b.vendorTapAt}.Any(t=>!KeepyRules.Finite(t) || t< -1 || t>b.clock) ||
                new[]{b.nextTheft,b.nextElectro,b.nextHide,b.nextStageElectro}.Any(t=>!KeepyRules.Finite(t) || t<0) ||
                b.villains.Where((v,i)=>v==null || v.id!=defaults[i].id || v.role!=defaults[i].role || v.item==null || !new[]{"mingle","approach","fake-approach","run","hidden","trapped","launched","cooldown"}.Contains(v.phase) || !HulkBirthday.Point(v.x,v.y) || !HulkBirthday.Point(v.targetX,v.targetY) ||
                    !KeepyRules.Finite(v.phaseAt) || v.phaseAt<0 || v.phaseAt>b.clock || !KeepyRules.Finite(v.trapUntil) || v.trapUntil< -1 || v.webbers==null || v.webbers.Length>3 || v.webbers.Distinct().Count()!=v.webbers.Length || v.webbers.Any(id=>!b.members.Any(m=>m.actor==id && m.role!=HulkBirthday.Roles[0]) && !b.cast.Take(4).Any(n=>n.id==id && n.role!=HulkBirthday.Roles[0])) ||
                    v.item!="" && !b.props.Any(p=>p.id==v.item) || v.phase=="trapped" && (v.webbers.Length==0 || v.trapUntil<b.clock)).Any() ||
                b.heroActor!="" && (!b.members.Any(m=>m.actor==b.heroActor && m.role==HulkBirthday.Roles[0]) && !b.cast.Take(1).Any(n=>n.id==b.heroActor) || !b.villains.Any(v=>v.id==b.heroTarget)) || b.heroAt>=0 && b.heroActor=="" ||
                b.props.Any(p=>!KeepyRules.Finite(p.fakeAt) || p.fakeAt< -1 || !KeepyRules.Finite(p.petAt) || p.petAt< -1 || p.petAt>b.clock || !HulkBirthday.Point(p.petHomeX,p.petHomeY)))throw new InvalidOperationException("Invalid birthday trouble state.");
        }
        private static void ResetBirthdayTrouble(BirthdayState b)
        {
            foreach(var v in b.villains){
                foreach(var prop in b.props.Where(p=>p.holder==v.id)){prop.holder="";prop.location="ground";prop.x=v.x;prop.y=v.y;prop.version++;}
                v.phase="mingle";v.webbers=Array.Empty<string>();v.trapUntil=-1;v.item="";v.phaseAt=b.clock;
            }
            foreach(var npc in b.cast.Where(n=>n.station.StartsWith("web",StringComparison.Ordinal) || n.station.StartsWith("smash",StringComparison.Ordinal))){npc.station="";npc.phase="idle";}
            b.heroAt=b.vendorTapAt=-1;b.heroActor=b.heroTarget=b.helperActor=b.helperTarget="";b.villain=-1;b.stolen="";
        }
        private void LeaveBirthdayTrouble(string actor)
        {
            var b=state.birthday;
            foreach(var v in b.villains){v.webbers=v.webbers.Where(id=>id!=actor).ToArray();if(v.phase=="trapped" && v.webbers.Length==0){v.phase="run";v.phaseAt=b.clock;v.trapUntil=-1;v.targetX=v.x<2400?500:4634;v.targetY=35;}}
            if(b.heroActor==actor){b.heroAt=-1;b.heroActor=b.heroTarget="";}
        }
        private void FinishBirthdayWebs(BirthdayVillain villain)
        {
            var b=state.birthday;
            foreach(var id in villain.webbers){var m=b.members.FirstOrDefault(m=>m.actor==id);if(m?.station=="web")m.station="";var n=b.cast.FirstOrDefault(n=>n.id==id);if(n?.station=="web")n.station="";}
            villain.webbers=Array.Empty<string>();villain.trapUntil=-1;
        }
        private void ReleaseVillainItem(BirthdayVillain v)
        {
            foreach(var prop in state.birthday.props.Where(p=>p.holder==v.id)){prop.holder="";prop.location="ground";prop.x=v.x;prop.y=v.y;prop.version++;}
            v.item="";state.birthday.stolen="";
        }
        private void StartBirthdayWeb(BirthdayVillain v,string actor)
        {
            var b=state.birthday;
            bool trapped=v.phase=="trapped";double until=v.trapUntil;
            var m=b.members.FirstOrDefault(m=>m.actor==actor);if(m!=null){LeaveBirthdayStation(actor);m.station="web";BirthdayPose(m,"Web");}
            else {var n=b.cast.Single(n=>n.id==actor);LeaveBirthdayBand(n.id);n.station="web";n.phase="idle";n.actionAt=b.clock;}
            if(v.phase!="trapped"){v.phase="trapped";v.phaseAt=b.clock;v.trapUntil=trapped?until:b.clock+15;}
            // LeaveBirthdayStation removes prior web membership; append only
            // after that cleanup when a human switches from another activity.
            if(!v.webbers.Contains(actor))v.webbers=v.webbers.Append(actor).ToArray();
            b.villain=Array.IndexOf(b.villains,v);b.villainAt=b.clock;
        }
        private bool BirthdayTroubleOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,out string outcome)
        {
            var b=state.birthday;outcome=null;
            if(c.value=="vendor-tap"){b.vendorTapAt=b.clock;return true;}
            if(c.value!="peek" && c.value!="web" && c.value!="smash" && c.value!="help")return false;
            var v=b.villains.FirstOrDefault(v=>v.id==c.target);
            if(v==null || v.phase=="cooldown" || v.phase=="launched"){outcome="find-a-villain";return true;}
            if(b.villain>=0 && b.villains[b.villain]!=v){outcome="help-with-the-shared-rescue";return true;}
            if(held!=null){outcome="put-down-your-prop";return true;}
            if(Math.Abs(player.x-v.x)>700 || Math.Abs(player.y-v.y)>350){outcome="come-closer";return true;}
            if(c.value=="peek"){
                if(v.phase!="hidden"){outcome="already-found";return true;}
                v.phase="run";v.phaseAt=b.clock;v.targetX=Math.Abs(v.x-500)<100?1600:Math.Abs(v.x-4634)<100?3200:v.x<2400?500:4634;v.targetY=35;return true;
            }
            if(c.value=="web"){
                if(member.role==HulkBirthday.Roles[0]){outcome="ask-a-spider-friend";return true;}
                StartBirthdayWeb(v,player.id);return true;
            }
            if(v.phase!="trapped"){
                if(b.helperActor!="" && b.helperTarget==v.id)return true;
                var helper=b.cast.Skip(1).Take(3).FirstOrDefault(n=>n.controller=="" && n.station=="" && !b.props.Any(p=>p.holder==n.id));
                if(helper==null){outcome="ask-a-spider-friend";return true;}
                if(Math.Abs(helper.x-v.x)<=700)StartBirthdayWeb(v,helper.id);
                else if(b.helperActor==""){b.helperActor=helper.id;b.helperTarget=v.id;b.villain=Array.IndexOf(b.villains,v);b.villainAt=b.clock;helper.station="web-wait";helper.phase="hero-walk";helper.targetX=Math.Clamp(v.x+(helper.x<v.x?-250:250),0,4800);helper.targetY=v.y;}
                return true;
            }
            if(member.role!=HulkBirthday.Roles[0]){outcome="hulk-can-finish-the-rescue";return true;}
            if(b.heroActor!=""){outcome="hulk-is-already-helping";return true;}
            if(Math.Abs(player.x-v.x)>300){outcome="come-closer";return true;}
            LeaveBirthdayStation(player.id);member.station="smash";BirthdayPose(member,"Smash");b.heroActor=player.id;b.heroTarget=v.id;b.heroAt=b.clock;return true;
        }
        private bool AdvanceBirthdayTrouble(double seconds,string[] connected)
        {
            var b=state.birthday;bool changed=false;
            if(b.heroActor!="" || b.villains.Any(v=>v.phase=="trapped" || v.phase=="launched"))AmbientChanged=true;
            var available=b.props.Where(p=>p.kind=="gift" && !p.fake && !p.opened && p.holder=="" && p.location=="table").ToArray();
            void Begin(BirthdayVillain v,BirthdayProp item,bool fake=false){b.villain=Array.IndexOf(b.villains,v);b.villainAt=b.clock;v.item=item.id;v.phase=fake?"fake-approach":"approach";v.phaseAt=b.clock;v.targetX=item.x;v.targetY=item.y;if(fake){item.holder=v.id;item.location="held";v.targetX=2380;v.targetY=180;}changed=true;}
            if(b.clock>=b.nextTheft){
                b.nextTheft=b.clock+30+BirthdayRandom(b,16);
                var candidates=b.villains.Where(v=>v.role!=HulkBirthday.VillainRoles[2] && (v.phase=="mingle" || v.phase=="hidden") && !b.props.Any(p=>p.holder==v.id) && !state.players.Any(p=>p.zone==HulkBirthday.Zone && (connected==null || connected.Contains(p.id)) && Math.Abs(p.x-v.x)<600)).ToArray();
                if(b.introDone && b.villain<0 && available.Length>0 && candidates.Length>0)Begin(candidates[BirthdayRandom(b,candidates.Length)],available[BirthdayRandom(b,available.Length)]);
            }
            bool stage=b.members.Any(m=>HulkBirthday.Instrument(m.station) || m.station=="dance") || b.cast.Any(n=>HulkBirthday.Instrument(n.station) || n.station=="dance");
            bool stageChance=false;if(!stage)b.nextStageElectro=b.clock+7.5;else if(b.clock>=b.nextStageElectro){b.nextStageElectro=b.clock+7.5;stageChance=BirthdayRandom(b,100)<25;}
            bool electroDue=b.clock>=b.nextElectro;if(electroDue)b.nextElectro=b.clock+60+BirthdayRandom(b,31);
            var electro=b.villains[2];var battery=b.props.Single(p=>p.kind=="battery");
            if(b.introDone && b.villain<0 && (stageChance || electroDue && available.Length>0) && battery.location=="socket" && battery.holder=="" && (electro.phase=="mingle" || electro.phase=="hidden"))Begin(electro,battery);
            if(b.clock>=b.nextHide){
                b.nextHide=b.clock+15;
                foreach(var v in b.villains.Where(v=>v.phase=="mingle")){v.phase="hidden";v.phaseAt=b.clock;changed=true;}
            }
            foreach(var v in b.villains){
                if(v.phase=="trapped"){
                    v.webbers=v.webbers.Where(id=>b.members.Any(m=>m.actor==id && m.station=="web" && state.players.Single(p=>p.id==id).zone==HulkBirthday.Zone && (connected==null || connected.Contains(id))) || b.cast.Any(n=>n.id==id && n.controller=="" && n.station=="web")).ToArray();
                    if(v.webbers.Length==0 || b.clock>=v.trapUntil){FinishBirthdayWebs(v);v.phase="run";v.phaseAt=b.clock;v.targetX=v.x<2400?500:4634;v.targetY=35;changed=true;}
                }
                if(v.phase=="approach" || v.phase=="fake-approach" || v.phase=="run"){
                    float dx=v.targetX-v.x,dy=v.targetY-v.y;double distance=Math.Sqrt(dx*dx+dy*dy),step=Math.Min(distance,Walking.Speed*seconds);
                    if(distance>.01){v.x+=(float)(dx/distance*step);v.y+=(float)(dy/distance*step);AmbientChanged=true;}
                    if(distance<=step+.1){
                        if(v.phase=="approach"){
                            var item=b.props.Single(p=>p.id==v.item);
                            if(item.holder=="" && (item.kind=="battery" && item.location=="socket" || item.kind=="gift" && item.location=="table" && !item.opened)){
                                item.holder=v.id;item.location="held";item.version++;b.stolen=item.id;if(item.kind=="battery")b.powered=false;
                                v.phase="run";v.targetX=v.x<2400?500:4634;v.targetY=35;
                            }else {v.phase="mingle";v.item="";b.villain=-1;}
                        }else if(v.phase=="fake-approach"){
                            var item=b.props.Single(p=>p.id==v.item);item.holder="";item.location="table";item.x=2380;item.y=180;item.version++;v.item="";v.phase="mingle";b.villain=-1;
                        }else {v.phase="hidden";}
                        v.phaseAt=b.clock;changed=true;
                    }
                }else if(v.phase=="launched" && b.clock-v.phaseAt>=2.5){v.phase="cooldown";v.phaseAt=b.clock;changed=true;}
                else if(v.phase=="cooldown" && b.clock-v.phaseAt>=15){v.phase="mingle";v.phaseAt=b.clock;b.villain=-1;changed=true;
                    var fake=b.props.FirstOrDefault(p=>p.fake && p.holder=="" && !p.opened);if(fake!=null){v.item=fake.id;Begin(v,fake,true);}}
            }
            var trapped=b.villains.FirstOrDefault(v=>v.phase=="trapped");
            if(b.helperActor!=""){
                var helper=b.cast.Single(n=>n.id==b.helperActor);var target=b.villains.Single(v=>v.id==b.helperTarget);
                if(helper.controller!="" || b.props.Any(p=>p.holder==helper.id) || target.phase=="cooldown" || target.phase=="launched"){
                    helper.station="";if(helper.phase=="hero-walk")helper.phase="idle";b.helperActor=b.helperTarget="";changed=true;
                }else if(Math.Abs(helper.x-target.x)<650){StartBirthdayWeb(target,helper.id);b.helperActor=b.helperTarget="";trapped=target;changed=true;}
                else {helper.targetX=Math.Clamp(target.x+(helper.x<target.x?-250:250),0,4800);helper.targetY=target.y;}
            }
            if(trapped!=null && b.heroActor==""){
                var hulk=b.cast[0];if(hulk.controller=="" && !b.props.Any(p=>p.holder==hulk.id)){
                    LeaveBirthdayBand(hulk.id);hulk.station="smash-wait";hulk.phase="hero-walk";hulk.targetX=Math.Clamp(trapped.x-100,0,4800);hulk.targetY=trapped.y;b.heroActor=hulk.id;b.heroTarget=trapped.id;b.heroAt=-1;changed=true;
                }
            }
            if(b.heroActor!=""){
                var npc=b.cast.FirstOrDefault(n=>n.id==b.heroActor);var human=b.members.FirstOrDefault(m=>m.actor==b.heroActor);var target=b.villains.Single(v=>v.id==b.heroTarget);
                bool present=npc!=null?npc.controller=="":human!=null && human.station=="smash" && state.players.Single(p=>p.id==human.actor).zone==HulkBirthday.Zone && (connected==null || connected.Contains(human.actor));
                if(!present || target.phase!="trapped"){
                    if(npc!=null){npc.station="";if(npc.phase=="hero-walk")npc.phase="idle";}if(human!=null && human.station=="smash")human.station="";b.heroActor=b.heroTarget="";b.heroAt=-1;changed=true;
                }else if(npc!=null && b.heroAt<0 && Math.Abs(npc.x-target.x)<300){npc.phase="idle";npc.station="smash";npc.actionAt=b.clock;b.heroAt=b.clock;changed=true;}
                else if(b.heroAt>=0 && b.clock-b.heroAt>=2.10){
                    ReleaseVillainItem(target);FinishBirthdayWebs(target);target.phase="launched";target.phaseAt=b.clock;b.troublesStopped++;b.reaction="rescue-happy";b.reactionAt=b.clock;
                    if(npc!=null){npc.station="";npc.x=Math.Clamp(target.x-60,0,4800);npc.y=target.y;}
                    if(human!=null){human.station="";var p=state.players.Single(p=>p.id==human.actor);p.x=Math.Clamp(target.x-60,0,4800);p.y=target.y;BirthdayPose(human,"Happy");}b.heroActor=b.heroTarget="";b.heroAt=-1;changed=true;
                }
            }
            foreach(var prop in b.props.Where(p=>p.fake && (b.members.Any(m=>m.actor==p.holder) || b.cast.Any(n=>n.id==p.holder)))){
                var player=state.players.FirstOrDefault(p=>p.id==prop.holder);var member=b.members.FirstOrDefault(m=>m.actor==prop.holder);var npc=b.cast.FirstOrDefault(n=>n.id==prop.holder);
                float x=player?.x??npc.x,y=player?.y??npc.y;bool idle=member!=null?member.station=="":npc.station=="" && (npc.phase=="idle" || npc.phase=="wave");
                if(prop.fakeAt<0 || Math.Abs(prop.x-x)+Math.Abs(prop.y-y)>1 || !idle){prop.fakeAt=b.clock+5+BirthdayRandom(b,11);prop.x=x;prop.y=y;}
                else if(prop.fakeAt>=0 && b.clock>=prop.fakeAt){prop.holder="";prop.location="stock";prop.fakeAt=-1;prop.opened=false;prop.version++;b.reaction="negative";b.reactionAt=b.clock;changed=true;}
            }
            return changed;
        }
    }
}
