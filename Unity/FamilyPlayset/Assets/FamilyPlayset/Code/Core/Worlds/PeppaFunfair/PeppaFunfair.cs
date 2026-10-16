using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class FunfairMember
    {
        public string actor,role,station="",pose="idle";
        public double at,until;
        public int seat=-1,paintStyle,paintRevision,slidePose,photos;
        public int[] paint=new int[64],prizes=new int[4];
        public FunfairMember Copy(){var c=(FunfairMember)MemberwiseClone();c.paint=paint.ToArray();c.prizes=prizes.ToArray();return c;}
    }
    [Serializable] public sealed class FunfairRide
    {
        public string stage="idle";
        public string[] riders=Array.Empty<string>();
        public double at;
        public float angle,momentum,speed;
        public int rounds;
        public FunfairRide Copy(){var c=(FunfairRide)MemberwiseClone();c.riders=riders.ToArray();return c;}
    }
    [Serializable] public sealed class FunfairProp
    {
        public string id,kind,holder="",location="stock";
        public int variant,version;
        public float x,y;
        public double releasedAt=-1;
        public FunfairProp Copy()=>(FunfairProp)MemberwiseClone();
    }
    [Serializable] public sealed class FunfairNpc
    {
        public string role,pose="idle";public int section;public float x,y,targetX;public double at,next;
        public FunfairNpc Copy()=>(FunfairNpc)MemberwiseClone();
    }
    [Serializable] public sealed class FunfairState
    {
        public int version=1,prizeSerial,catches,paintings,slides,hookRound,hookTarget=-1,hookRevision,paintStyle=1,paintRevision,smoothieMask,smoothies;
        public double clock,hookAt=-1,paintAt=-1,smoothieAt=-1,photoAt=-1;
        public string hookStage="idle",hookActor="",paintActor="",photoActor="",photoRole="";
        public int photoPose;
        public float aimX=.5f,aimY=.5f;
        public int[] paintDraft=new int[64];
        public string[] slideQueue=Array.Empty<string>(),hookQueue=Array.Empty<string>(),paintQueue=Array.Empty<string>();
        public FunfairRide wheel=new FunfairRide(),teacups=new FunfairRide();
        public FunfairMember[] members;public FunfairProp[] props;public FunfairNpc[] npcs;
        public FunfairState Copy(){var c=(FunfairState)MemberwiseClone();c.members=members.Select(v=>v.Copy()).ToArray();c.props=props.Select(v=>v.Copy()).ToArray();c.npcs=npcs.Select(v=>v.Copy()).ToArray();c.wheel=wheel.Copy();c.teacups=teacups.Copy();c.slideQueue=slideQueue.ToArray();c.hookQueue=hookQueue.ToArray();c.paintQueue=paintQueue.ToArray();c.paintDraft=paintDraft.ToArray();return c;}
    }
    public static partial class PeppaFunfair
    {
        public const string Zone="peppa-funfair";
        // Integrating code must reconcile this additive boundary with concurrent worlds.
        public const int Schema=59;
        public static readonly string[] Roles={"Peppa","George","Danny","Candy","Mummy","Daddy"};
        public static readonly string[] Stations={"wheel","teacups","slide","hook","paint","drink","float","fall","react"};
        public static float X(string key)=>key=="wheel"?700:key=="teacups"?300:key=="smoothie"?150:key=="hook"?1800:key=="slide"?3000:key=="paint"?4200:600;
        public static bool Point(float x,float y)=>KeepyRules.Finite(x) && KeepyRules.Finite(y) && x>=0 && x<=4800 && y>=0 && y<=500;
        public static FunfairState Create(string[] actors)=>new FunfairState{
            members=actors.Select((a,i)=>new FunfairMember{actor=a,role=Roles[i]}).ToArray(),
            props=Enumerable.Range(0,12).Select(i=>new FunfairProp{id="fair-prop-"+i,kind=i<8?"drink":"balloon",variant=i%4,x=i<8?150:3000,y=150}).ToArray(),
            npcs=Enumerable.Range(0,4).SelectMany(s=>Roles.Concat(new[]{"MrsRabbit"}).Select((r,i)=>new FunfairNpc{role=r,section=s,x=s*1200+170+i*130,targetX=s*1200+170+i*130,y=150+i%3*35,next=5+i*1.3})).ToArray()
        };
        static FunfairMember Member(FunfairState h,string actor)=>h.members.Single(m=>m.actor==actor);
        static void Pose(FunfairState h,FunfairMember m,string station,string pose,double seconds){m.station=station;m.pose=pose;m.at=h.clock;m.until=h.clock+seconds;}
        static FunfairRide Ride(FunfairState h,string key)=>key=="wheel"?h.wheel:h.teacups;
        public static void Leave(FunfairState h,string actor)
        {
            if(h==null)return;var m=Member(h,actor);m.station="";m.pose="idle";m.seat=-1;m.at=h.clock;
            h.wheel.riders=h.wheel.riders.Where(a=>a!=actor).ToArray();h.teacups.riders=h.teacups.riders.Where(a=>a!=actor).ToArray();h.slideQueue=h.slideQueue.Where(a=>a!=actor).ToArray();h.hookQueue=h.hookQueue.Where(a=>a!=actor).ToArray();h.paintQueue=h.paintQueue.Where(a=>a!=actor).ToArray();
            if(h.hookActor==actor){h.hookActor="";h.hookStage="idle";h.hookTarget=-1;h.hookRevision++;}
            if(h.paintActor==actor){h.paintActor="";h.paintDraft=new int[64];h.paintRevision++;}
        }
        public static void Release(FunfairState h,string actor,SoloPlayer[] players)
        {
            if(h==null)return;Leave(h,actor);var p=players.Single(a=>a.id==actor);
            foreach(var prop in h.props.Where(v=>v.holder==actor)){prop.holder="";prop.location="ground";prop.x=p.x;prop.y=p.y;prop.releasedAt=h.clock;prop.version++;}
        }
        public static void Restore(FunfairState h)
        {
            if(h==null)return;foreach(var m in h.members){m.station="";m.pose="idle";m.seat=-1;m.at=h.clock;}
            foreach(var prop in h.props.Where(p=>p.holder!="")){prop.holder="";prop.location="stock";prop.releasedAt=-1;prop.version++;}
            foreach(var r in new[]{h.wheel,h.teacups}){r.riders=Array.Empty<string>();r.stage="idle";r.speed=r.momentum=0;}
            // A completed photo is history, rather than a live station lease.
            // Keep its subject when clearing interrupted rides and booth edits.
            h.slideQueue=h.hookQueue=h.paintQueue=Array.Empty<string>();h.hookActor=h.paintActor="";h.hookStage="idle";h.hookTarget=-1;h.paintDraft=new int[64];h.smoothieMask=0;h.smoothieAt=-1;
        }
        public static string Apply(FunfairState h,SoloPlayer[] players,SoloCommand c,string[] connected)
        {
            var p=players.FirstOrDefault(a=>a.id==c.actor);if(h==null || p==null || p.zone!=Zone)return "come-to-funfair";
            var m=Member(h,p.id);var held=h.props.FirstOrDefault(v=>v.holder==p.id);bool Near(string key)=>Math.Abs(p.x-X(key))<190;
            if(c.value=="leave"){Leave(h,p.id);return null;}
            if(c.value=="role"){
                if(!Roles.Contains(c.target) || m.station!="" || held!=null)return "leave-activity-first";var other=h.members.FirstOrDefault(v=>v!=m && v.role==c.target);
                if(other!=null && players.Any(a=>a.id==other.actor && a.zone==Zone && (connected==null || connected.Contains(a.id))))return "friend-already-chosen";
                if(other!=null)other.role=m.role;m.role=c.target;return null;
            }
            if(c.value=="board"){
                if(c.target!="wheel" && c.target!="teacups" || !Near(c.target))return "come-closer";var ride=Ride(h,c.target);
                if(m.station!="" || held!=null || ride.riders.Length>=4 || ride.stage!="idle" && ride.stage!="boarding")return "wait-for-the-ride";
                int seat=Enumerable.Range(0,4).First(i=>!h.members.Any(v=>v.station==c.target && v.seat==i));ride.riders=ride.riders.Concat(new[]{p.id}).ToArray();ride.stage="boarding";m.seat=seat;Pose(h,m,c.target,"sit",999);return null;
            }
            if(c.value=="ride-start" || c.value=="ride-spin"){
                if(c.target!="wheel" && c.target!="teacups")return "choose-a-ride";var ride=Ride(h,c.target);if(!ride.riders.Contains(p.id))return "board-the-ride";
                if(c.value=="ride-start"){if(ride.stage!="boarding")return "wait-for-the-ride";ride.stage="countdown";ride.at=h.clock;ride.momentum=90;}
                else{if(ride.stage!="running")return "wait-for-the-ride";ride.momentum=Math.Min(180,ride.momentum+90);}return null;
            }
            if(c.value=="slide-join" || c.value=="hook-join" || c.value=="paint-join"){
                string key=c.value.Split('-')[0];if(!Near(key))return "come-closer";if(m.station!="")return "leave-activity-first";if(key=="hook" && held!=null)return "put-down-first";
                if(key=="slide" && m.role=="Mummy")return "choose-a-slide-character";
                if(key=="slide")h.slideQueue=h.slideQueue.Concat(new[]{p.id}).ToArray();if(key=="hook")h.hookQueue=h.hookQueue.Concat(new[]{p.id}).ToArray();if(key=="paint")h.paintQueue=h.paintQueue.Concat(new[]{p.id}).ToArray();Pose(h,m,key,"idle",999);return null;
            }
            if(c.value=="slide-pose"){
                if(m.station!="slide" || !int.TryParse(c.item,out var pose) || pose<0 || pose>2)return "choose-a-slide-pose";m.slidePose=pose;return null;
            }
            if(c.value=="hook-aim" || c.value=="hook-pull"){
                if(h.hookActor!=p.id || h.hookStage!="aim")return "wait-your-turn";
                if(!KeepyRules.Finite(c.x) || !KeepyRules.Finite(c.y) || c.x<0 || c.x>1 || c.y<0 || c.y>1)return "aim-at-a-flower";
                if(c.value=="hook-pull"){
                    if(!int.TryParse(c.item,out var target) || target<0 || target>=15 || !int.TryParse(c.target,out var round) || round!=h.hookRound)return "stale-flower";
                    var x=FlowerX(h.clock,target);var y=FlowerY(h.clock,target);if(Math.Abs(c.x-x)>.12 || Math.Abs(c.y-y)>.12)return "try-again";
                    h.hookTarget=target;h.hookStage="pull";h.hookAt=h.clock;h.hookRevision++;Pose(h,m,"hook","pull",999);
                }h.aimX=c.x;h.aimY=c.y;return null;
            }
            if(c.value=="paint-style"){
                if(m.station!="paint" || h.paintActor=="" || !int.TryParse(c.item,out var style) || style<1 || style>3 || h.paintDraft.Any(v=>v>0))return "choose-before-painting";h.paintStyle=style;h.paintRevision++;return null;
            }
            if(c.value=="paint-stroke"){
                if(m.station!="paint" || h.paintActor=="" || !int.TryParse(c.item,out var colour) || colour<1 || colour>8 || !int.TryParse(c.target,out var revision) || revision!=h.paintRevision)return "stale-paint";
                if(!KeepyRules.Finite(c.x) || !KeepyRules.Finite(c.y) || c.x<0 || c.x>=8 || c.y<0 || c.y>=8)return "paint-on-the-face";
                h.paintDraft[(int)c.y*8+(int)c.x]=colour;h.paintRevision++;h.paintAt=h.clock;return null;
            }
            if(c.value=="paint-done"){
                if(m.station!="paint" || h.paintActor=="" || !h.paintDraft.Any(v=>v>0))return "paint-first";var recipient=Member(h,h.paintActor);recipient.paintStyle=h.paintStyle;recipient.paint=h.paintDraft.ToArray();recipient.paintRevision++;h.paintings++;
                h.paintQueue=h.paintQueue.Where(a=>a!=recipient.actor).ToArray();Pose(h,recipient,"react",h.paintStyle==1?"tiger":h.paintStyle==2?"dinosaur":"fairy",3);h.paintActor="";h.paintDraft=new int[64];h.paintRevision++;return null;
            }
            if(c.value=="paint-clear"){
                if(m.station!="")return "leave-activity-first";m.paintStyle=0;m.paint=new int[64];m.paintRevision++;return null;
            }
            // The shipped machine is tapped to produce a cup; it does not open
            // a fruit-selection menu. Keep old recipe commands for saved probes.
            if(c.value=="smoothie-machine"){
                if(!Near("smoothie") || m.station!="" || held!=null)return "come-to-the-mixer";
                if(h.smoothieAt>=0)return "wait-for-a-cup";
                if(h.smoothieMask==8){var cup=h.props.FirstOrDefault(v=>v.kind=="drink"&&v.location=="stock");if(cup==null)return "wait-for-a-cup";cup.holder=p.id;cup.location="held";cup.version++;h.smoothieMask=0;h.smoothies++;}
                else{h.smoothieMask=7;h.smoothieAt=h.clock;}
                return null;
            }
            if(c.value=="smoothie-fruit"){
                if(!Near("smoothie") || !int.TryParse(c.item,out var fruit) || fruit<0 || fruit>2 || h.smoothieAt>=0)return "come-to-the-mixer";h.smoothieMask|=1<<fruit;return null;
            }
            if(c.value=="smoothie-blend"){
                if(!Near("smoothie") || h.smoothieMask!=7 || h.smoothieAt>=0)return "add-the-fruit";h.smoothieAt=h.clock;return null;
            }
            if(c.value=="drink-take" || c.value=="balloon-take"){
                string kind=c.value=="drink-take"?"drink":"balloon";if(!Near(kind=="drink"?"smoothie":"slide") || held!=null || m.station!="")return "hands-busy";if(kind=="drink" && h.smoothieMask!=8)return "blend-the-smoothie";
                var prop=h.props.FirstOrDefault(v=>v.kind==kind && v.location=="stock");if(prop==null)return "wait-for-a-cup";prop.holder=p.id;prop.location="held";prop.version++;if(kind=="drink"){h.smoothieMask=0;h.smoothies++;}return null;
            }
            if(c.value=="drink" || c.value=="float"){
                if(held==null || held.kind!=(c.value=="drink"?"drink":"balloon") || m.station!="")return "bring-the-prop";Pose(h,m,c.value,c.value=="drink"?"drink":"hold",c.value=="drink"?2.1:8);return null;
            }
            if(c.value=="drop"){
                if(held==null)return "hands-empty";Leave(h,p.id);held.holder="";held.location="ground";held.x=Math.Min(4800,p.x+25);held.y=p.y;held.releasedAt=h.clock;held.version++;return null;
            }
            if(c.value=="take"){
                var prop=h.props.FirstOrDefault(v=>v.id==c.item);if(held!=null || prop==null || prop.location!="ground" || Math.Abs(prop.x-p.x)>90 || Math.Abs(prop.y-p.y)>90)return "come-closer";prop.holder=p.id;prop.location="held";prop.releasedAt=-1;prop.version++;return null;
            }
            if(c.value=="give"){
                var friend=players.FirstOrDefault(v=>v.id==c.target && v.zone==Zone);if(held==null || friend==null || friend.id==p.id || h.props.Any(v=>v.holder==friend.id) || Math.Abs(friend.x-p.x)>90 || Math.Abs(friend.y-p.y)>90)return "come-to-your-friend";held.holder=friend.id;held.version++;return null;
            }
            return "unknown-funfair-action";
        }
        public static float FlowerX(double clock,int target)=>(float)(.12+(target%5)*.18+.035*Math.Sin(clock*.7+target));
        public static float FlowerY(double clock,int target)=>(float)(.2+(target/5)*.27+.025*Math.Cos(clock*.8+target));
        public static bool Advance(FunfairState h,SoloPlayer[] players,double seconds,string[] connected,out bool visible)
        {
            visible=false;if(h==null)return false;var present=players.Where(p=>p.zone==Zone && (connected==null || connected.Contains(p.id))).ToArray();foreach(var m in h.members.Where(m=>!present.Any(p=>p.id==m.actor)))Release(h,m.actor,players);if(present.Length==0)return false;
            string before=Stamp(h);h.clock+=seconds;
            foreach(var key in new[]{"wheel","teacups"}){
                var ride=Ride(h,key);if(ride.riders.Length==0){ride.stage="idle";ride.speed=ride.momentum=0;continue;}
                if(ride.stage=="countdown" && h.clock-ride.at>=3){ride.stage="running";ride.at=h.clock;}
                if(ride.stage=="running"){
                    // Native RotateWithMomentum clamps momentum to maxSpeed,
                    // then multiplies the angular step by acceleration. It is
                    // not a velocity smoothing coefficient.
                    ride.speed=ride.momentum>=45?Math.Min(ride.momentum,100)*3.25f:0;ride.angle=(ride.angle+ride.speed*(float)seconds)%360;ride.momentum=Math.Max(0,ride.momentum-(float)(75*seconds));
                    if(key=="wheel" && ride.momentum<45)ride.angle=PreferredAngle(ride.angle,(float)(33*seconds));
                    if(h.clock-ride.at>=20 || h.clock-ride.at>=6 && ride.momentum<45){ride.stage="finish";ride.at=h.clock;ride.rounds++;}
                }else if(ride.stage=="finish"){if(key=="wheel")ride.angle=PreferredAngle(ride.angle,(float)(33*seconds));if(h.clock-ride.at>=2){foreach(var id in ride.riders.ToArray()){var member=Member(h,id);member.station="";member.pose="happy";member.at=h.clock;member.seat=-1;}ride.riders=Array.Empty<string>();ride.stage="idle";ride.speed=ride.momentum=0;}}
            }
            if(h.slideQueue.Length>0){var member=Member(h,h.slideQueue[0]);if(member.pose=="idle"){member.pose="climb";member.at=h.clock;}if(h.clock-member.at>=17){h.slideQueue=h.slideQueue.Skip(1).ToArray();member.station="";member.pose="land";member.at=h.clock;member.photos++;h.slides++;h.photoActor=member.actor;h.photoRole=member.role;h.photoPose=member.slidePose;h.photoAt=h.clock;}}
            if(h.hookActor=="" && h.hookQueue.Length>0){h.hookActor=h.hookQueue[0];h.hookStage="cast";h.hookAt=h.clock;h.hookRound++;h.hookRevision++;Pose(h,Member(h,h.hookActor),"hook","cast",999);}
            if(h.hookStage=="cast" && h.clock-h.hookAt>=1.5){h.hookStage="aim";Pose(h,Member(h,h.hookActor),"hook","fish",999);}
            if(h.hookStage=="pull" && h.clock-h.hookAt>=1.5){var member=Member(h,h.hookActor);int prize=h.hookTarget%4;member.prizes[prize]++;h.prizeSerial++;h.catches++;h.hookStage="reward";h.hookAt=h.clock;h.hookRevision++;Pose(h,member,"hook","fish-happy",999);}
            if(h.hookStage=="reward" && h.clock-h.hookAt>=2){var member=Member(h,h.hookActor);h.hookQueue=h.hookQueue.Where(a=>a!=h.hookActor).ToArray();member.station="";member.pose="happy";member.at=h.clock;h.hookActor="";h.hookStage="idle";h.hookTarget=-1;h.hookRevision++;}
            if(h.paintActor=="" && h.paintQueue.Length>0){h.paintActor=h.paintQueue[0];h.paintDraft=new int[64];h.paintRevision++;}
            if(h.smoothieAt>=0 && h.clock-h.smoothieAt>=1.5){h.smoothieMask=8;h.smoothieAt=-1;}
            foreach(var m in h.members.Where(m=>m.station!="" && h.clock>=m.until)){
                if(m.station=="drink"){var prop=h.props.FirstOrDefault(p=>p.holder==m.actor && p.kind=="drink");if(prop!=null){prop.holder="";prop.location="stock";prop.version++;}}
                if(m.station=="float"){Pose(h,m,"fall","fall",1.5);continue;}Leave(h,m.actor);
            }
            foreach(var prop in h.props.Where(p=>p.location=="ground" && p.holder=="" && p.releasedAt>=0 && h.clock-p.releasedAt>=60)){prop.location="stock";prop.releasedAt=-1;prop.version++;}
            foreach(var npc in h.npcs){if(h.clock>=npc.next){npc.targetX=npc.section*1200+140+(float)((h.clock*37+npc.role.Length*83)%900);npc.pose="walk";npc.at=h.clock;npc.next=h.clock+8+npc.role.Length;}
                float dx=npc.targetX-npc.x,step=(float)(seconds*Walking.Speed*.25);if(Math.Abs(dx)>step)npc.x+=Math.Sign(dx)*step;else{npc.x=npc.targetX;npc.pose="idle";}}
            visible=before!=Stamp(h);return true;
        }
        static string Stamp(FunfairState h)=>h.wheel.stage+"/"+h.teacups.stage+"/"+h.slideQueue.Length+"/"+h.hookRevision+"/"+h.paintRevision+"/"+h.smoothieMask+"/"+string.Join(",",h.members.Select(m=>m.station+":"+m.pose))+"/"+string.Join(",",h.props.Select(p=>p.version));
        static float PreferredAngle(float angle,float step){float Delta(float to)=>(to-angle+540)%360-180;float target=new[]{30f,150f,270f}.OrderBy(a=>Math.Abs(Delta(a))).First();float d=Delta(target);return (angle+Math.Sign(d)*Math.Min(step,Math.Abs(d))+360)%360;}
    }
}
