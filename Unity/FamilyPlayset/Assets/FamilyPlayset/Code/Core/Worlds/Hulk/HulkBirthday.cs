using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class BirthdayMember
    {
        public string actor, role, station="", pose="Idle";
        public double actionAt;
        public BirthdayMember Copy()=>(BirthdayMember)MemberwiseClone();
    }
    [Serializable] public sealed class BirthdayProp
    {
        public string id, kind, holder="", location="ground";
        public float x,y;
        public int version, style;
        public bool opened, fake;
        public BirthdayProp Copy()=>(BirthdayProp)MemberwiseClone();
    }
    [Serializable] public sealed class BirthdayState
    {
        public int version=1, visit, cakeBase, track, villain=-1, celebrations;
        public double clock, songAt=-1, villainAt=-1, nextTrouble=60, introAt=-1, reactionAt=-1;
        public string reaction="Idle", stolen="";
        public bool powered=true, cakeReady, introDone;
        public BirthdayMember[] members;
        public BirthdayProp[] props;
        public int[] stickers=Array.Empty<int>();
        public BirthdayState Copy()=>new BirthdayState{version=version,visit=visit,cakeBase=cakeBase,track=track,villain=villain,celebrations=celebrations,
            clock=clock,songAt=songAt,villainAt=villainAt,nextTrouble=nextTrouble,introAt=introAt,reactionAt=reactionAt,reaction=reaction,stolen=stolen,
            powered=powered,cakeReady=cakeReady,introDone=introDone,members=members.Select(m=>m.Copy()).ToArray(),props=props.Select(p=>p.Copy()).ToArray(),stickers=stickers.ToArray()};
    }
    public static class HulkBirthday
    {
        public const string Zone="hulk-birthday";
        public const int Schema=54;
        public static readonly string[] Roles={"Hulk_NewNew","Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW"};
        public static readonly string[] Stations={"guitar","keyboard","drums","dance","castle"};
        public static bool Instrument(string station)=>station=="guitar" || station=="keyboard" || station=="drums";
        public static bool Point(float x,float y)=>KeepyRules.Finite(x) && KeepyRules.Finite(y) && x>=0 && x<=4800 && y>=0 && y<=500;
        public static BirthdayState Create(string[] actors)=>new BirthdayState {
            members=actors.Select((id,i)=>new BirthdayMember{actor=id,role=Roles[i]}).ToArray(),
            props=Enumerable.Range(0,8).Select(i=>new BirthdayProp{id="birthday-gift-"+i,kind="gift",style=i%4,fake=i==7,x=650+i*48,y=160})
                .Concat(new[]{new BirthdayProp{id="birthday-battery",kind="battery",x=2800,y=180,location="socket"}})
                .Concat(Enumerable.Range(0,4).Select(i=>new BirthdayProp{id="birthday-food-"+i,kind="food",style=i,x=1700+i*60,y=170}))
                .Concat(Enumerable.Range(0,4).Select(i=>new BirthdayProp{id="birthday-drink-"+i,kind="drink",style=i,x=2000+i*55,y=170}))
                .Concat(Enumerable.Range(0,8).Select(i=>new BirthdayProp{id="birthday-brick-"+i,kind="brick",style=i%4,x=4050+i*55,y=140})).ToArray()
        };
    }
    public sealed partial class GameWorld
    {
        private static void NormalizeBirthday(SoloSnapshot snapshot)
        {
            var b=snapshot?.birthday;
            // JsonUtility materializes an absent inline class as an empty object.
            // Only a pre-birthday save with no birthday content is normalized;
            // current-format or populated records still undergo strict validation.
            if(snapshot!=null && snapshot.schema<HulkBirthday.Schema && b!=null && b.clock==0 && b.visit==0 && !b.cakeReady && !b.introDone && b.celebrations==0 &&
                (b.members==null || b.members.Length==0) && (b.props==null || b.props.Length==0) && (b.stickers==null || b.stickers.Length==0))snapshot.birthday=null;
        }
        private static void RestoreBirthday(SoloSnapshot snapshot)
        {
            var b=snapshot.birthday;if(b==null)return;
            var changed=b.songAt>=0 || b.members.Any(m=>m.station!="");
            foreach(var member in b.members){member.station="";member.pose="Idle";}
            foreach(var prop in b.props.Where(p=>p.holder!="" && p.holder!="villain")){
                var owner=snapshot.players.Single(p=>p.id==prop.holder);
                prop.x=owner.x;prop.y=owner.y;prop.holder="";prop.location="ground";prop.version++;changed=true;
            }
            b.songAt=-1;if(changed)snapshot.revision++;
        }
        public static GameWorld WithBirthday(GameWorld world)
        {
            world=WithDinosaurWorld(world);
            if(world.state.birthday!=null)return world;
            var snapshot=world.Snapshot();snapshot.birthday=HulkBirthday.Create(snapshot.players.Select(p=>p.id).ToArray());
            snapshot.revision++;Validate(snapshot);return new GameWorld(snapshot);
        }
        public BirthdayState ReadBirthday()=>state.birthday?.Copy();
        private static void ValidateBirthday(SoloSnapshot s)
        {
            var b=s.birthday;if(b==null && s.schema<HulkBirthday.Schema)return;
            if(b==null || b.version!=1 || b.visit<0 || b.cakeBase<0 || b.cakeBase>2 || b.track<0 || b.track>2 || b.villain< -1 || b.villain>3 || b.celebrations<0 ||
                !KeepyRules.Finite(b.clock) || b.clock<0 || !KeepyRules.Finite(b.nextTrouble) || b.nextTrouble<0 || b.stolen==null || b.reaction==null ||
                new[]{b.songAt,b.villainAt,b.introAt,b.reactionAt}.Any(t=>!KeepyRules.Finite(t) || t< -1 || t>b.clock) || b.stickers==null || b.stickers.Length>32 || b.stickers.Any(n=>n<0 || n>31) ||
                b.members==null || !b.members.Select(m=>m?.actor).OrderBy(x=>x).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x)) ||
                b.members.Select(m=>m.role).Distinct().Count()!=b.members.Length || b.members.Any(m=>!HulkBirthday.Roles.Contains(m.role) || m.station==null || m.pose==null || !KeepyRules.Finite(m.actionAt) || m.actionAt<0 || m.actionAt>b.clock || m.station!="" && !HulkBirthday.Stations.Contains(m.station)) ||
                b.props==null || b.props.Length!=25 || b.props.Any(p=>p==null || !Id(p.id) || p.holder==null || p.location==null || !HulkBirthday.Point(p.x,p.y) || p.version<0 || p.style<0 || p.style>3 || !new[]{"gift","battery","food","drink","brick"}.Contains(p.kind)) ||
                !b.props.Select(p=>p.id+":"+p.kind).SequenceEqual(HulkBirthday.Create(s.players.Select(p=>p.id).ToArray()).props.Select(p=>p.id+":"+p.kind)) ||
                b.props.Any(p=>p.holder!="" && p.holder!="villain" && (!b.members.Any(m=>m.actor==p.holder) || s.players.Single(m=>m.id==p.holder).zone!=HulkBirthday.Zone)) ||
                b.members.Any(m=>b.props.Count(p=>p.holder==m.actor)>1) || b.members.Where(m=>HulkBirthday.Instrument(m.station)).GroupBy(m=>m.station).Any(g=>g.Count()>1))
                throw new InvalidOperationException("Invalid Hulk birthday world.");
        }
        // The source Holding slot blocks the hand/drink/tool slots. This first
        // shared slice exposes that full-body slot only, never unrestricted hands.
        // All multiplayer arbitration is a Little Weeps adaptation.
        private string BirthdayOperation(SoloCommand c,SoloPlayer player,string[] connected)
        {
            var b=state.birthday;if(b==null || player.zone!=HulkBirthday.Zone)return "come-to-the-party";
            var member=b.members.Single(m=>m.actor==player.id);
            var held=b.props.SingleOrDefault(p=>p.holder==player.id);
            var prop=b.props.SingleOrDefault(p=>p.id==c.item);
            if(c.value=="role") {
                if(!HulkBirthday.Roles.Contains(c.target))return "choose-a-party-friend";
                if(b.members.Any(m=>m.actor!=player.id && m.role==c.target))return "friend-already-chosen";
                if(held!=null || member.station!="")return "put-down-and-leave-first";
                member.role=c.target;member.pose="Idle";member.actionAt=b.clock;return null;
            }
            if(c.value=="claim") {
                if(prop==null || prop.holder!="" || prop.location=="unwrapping")return "prop-already-in-use";
                if(held!=null || member.station!="")return "hands-busy";
                if(Math.Abs(prop.x-player.x)>InteractionRadius || Math.Abs(prop.y-player.y)>InteractionRadius)return "come-closer";
                prop.holder=player.id;prop.location="held";prop.version++;BirthdayPose(member,"Lift");return null;
            }
            if(c.value=="drop") {
                if(held==null || held.id!=c.item)return "not-holding-that";
                if(!HulkBirthday.Point(c.x,c.y) || Math.Abs(c.x-player.x)>InteractionRadius || Math.Abs(c.y-player.y)>InteractionRadius)return "put-it-nearby";
                held.holder="";held.location="ground";held.x=c.x;held.y=c.y;held.version++;BirthdayPose(member,"Idle");return null;
            }
            if(c.value=="give") {
                var other=b.members.SingleOrDefault(m=>m.actor==c.target);
                var receiver=state.players.SingleOrDefault(p=>p.id==c.target);
                if(held==null || held.id!=c.item || other==null || other==member || receiver.zone!=HulkBirthday.Zone || connected!=null && !connected.Contains(other.actor))return "choose-a-party-helper";
                if(other.station!="" || b.props.Any(p=>p.holder==other.actor))return "friends-hands-busy";
                if(Math.Abs(receiver.x-player.x)>InteractionRadius || Math.Abs(receiver.y-player.y)>InteractionRadius)return "come-closer";
                // One authority transaction transfers the prop; no release/claim gap.
                held.holder=other.actor;held.version++;BirthdayPose(member,"Idle");BirthdayPose(other,"Lift");return null;
            }
            if(c.value=="station") {
                if(c.target!="" && !HulkBirthday.Stations.Contains(c.target))return "choose-a-party-activity";
                if(held!=null)return "put-down-your-prop";
                if(HulkBirthday.Instrument(c.target) && b.members.Any(m=>m!=member && m.station==c.target))return "instrument-in-use";
                member.station=c.target;BirthdayPose(member,c.target=="castle"?"Jump":c.target=="dance"?"Dance":HulkBirthday.Instrument(c.target)?"PlayInstrument":"Idle");
                if(HulkBirthday.Instrument(c.target) && b.songAt<0)b.songAt=b.clock;
                return null;
            }
            return "birthday-action-unavailable";
        }
        private void BirthdayPose(BirthdayMember member,string pose){member.pose=pose;member.actionAt=state.birthday.clock;}
        private bool LeaveBirthdayStation(string actor)
        {
            var member=state.birthday?.members.FirstOrDefault(m=>m.actor==actor);
            if(member==null || member.station=="")return false;
            member.station="";BirthdayPose(member,"Idle");return true;
        }
        public bool ReleaseBirthday(string actor)
        {
            var b=state.birthday;var member=b?.members.FirstOrDefault(m=>m.actor==actor);if(member==null)return false;
            var player=state.players.Single(p=>p.id==actor);var changed=member.station!="";
            member.station="";member.pose="Idle";
            foreach(var prop in b.props.Where(p=>p.holder==actor)){prop.holder="";prop.location="ground";prop.x=player.x;prop.y=player.y;prop.version++;changed=true;}
            if(changed)state.revision++;return changed;
        }
        private bool AdvanceBirthday(double seconds,string[] active,out bool visible)
        {
            visible=false;var b=state.birthday;
            if(b==null || !state.players.Any(p=>p.zone==HulkBirthday.Zone && (active==null || active.Contains(p.id))))return false;
            b.clock+=seconds;
            foreach(var m in b.members)if(m.station!="" && (state.players.Single(p=>p.id==m.actor).zone!=HulkBirthday.Zone || active!=null && !active.Contains(m.actor))){ReleaseBirthday(m.actor);visible=true;}
            if(b.songAt>=0 && !b.members.Any(m=>HulkBirthday.Instrument(m.station))){b.songAt=-1;visible=true;}
            return true;
        }
    }
}
