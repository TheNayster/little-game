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
        public int version, style, content;
        public bool opened, fake;
        public BirthdayProp Copy()=>(BirthdayProp)MemberwiseClone();
    }
    [Serializable] public sealed class BirthdayNpc
    {
        public string id,role,controller="",phase="idle";
        public bool active;
        public float x,y,targetX,targetY;
        public double actionAt,leaveAt;
        public BirthdayNpc Copy()=>(BirthdayNpc)MemberwiseClone();
    }
    [Serializable] public sealed class BirthdayState
    {
        public int version=3, visit, cakeBase, track, villain=-1, celebrations,guestCursor,giftCursor;
        public double clock, songAt=-1, villainAt=-1, nextTrouble=60, introAt=-1, reactionAt=-1;
        public string reaction="Idle", stolen="";
        public bool powered=true, cakeReady, introDone;
        public BirthdayMember[] members;
        public BirthdayProp[] props;
        public BirthdayNpc[] cast;
        public int[] guestOrder;
        public double nextGuest=25;
        public double giftAt=-1;
        public string giftId="",giftActor="";
        public float[] stickerX=Array.Empty<float>(),stickerY=Array.Empty<float>();
        public int[] stickers=Array.Empty<int>();
        public BirthdayState Copy()=>new BirthdayState{version=version,visit=visit,cakeBase=cakeBase,track=track,villain=villain,celebrations=celebrations,
            clock=clock,songAt=songAt,villainAt=villainAt,nextTrouble=nextTrouble,introAt=introAt,reactionAt=reactionAt,reaction=reaction,stolen=stolen,
            powered=powered,cakeReady=cakeReady,introDone=introDone,members=members.Select(m=>m.Copy()).ToArray(),props=props.Select(p=>p.Copy()).ToArray(),stickers=stickers.ToArray(),
            cast=cast.Select(n=>n.Copy()).ToArray(),guestOrder=guestOrder.ToArray(),guestCursor=guestCursor,nextGuest=nextGuest,
            giftAt=giftAt,giftId=giftId,giftActor=giftActor,giftCursor=giftCursor,stickerX=stickerX.ToArray(),stickerY=stickerY.ToArray()};
    }
    public static class HulkBirthday
    {
        public const string Zone="hulk-birthday";
        public const int Schema=54;
        public static readonly string[] Roles={"Hulk_NewNew","Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW"};
        public static readonly string[] Guests={"HatPoliceGuy_NewNew","BusDriver_newNew","BlondePoliceLady_NewNew","BlueBlazerGuy_NewNew","BlueBlazerLady_NewNew","ConstructionGuy_02_NewNew","ConstructionLady_01_NewNew","GlassesGuy_NewNew","GroovyGranny_NewNew","JeanJacketGuy_NewNew","LavalliereLady_NewNew","RedStripedShirtGuy_NewNew","ConstructionGuy_03_NewNew"};
        public static readonly string[] Stations={"guitar","keyboard","drums","dance","castle","unwrap","eat","drink"};
        public static readonly string[] Contents={"CakeActorVariant_CR","BoulderActor_v2","FlowerActor_v2","PumpkinActor_v2","BookActor_v2","TeddyActor","CatActor_NewNew","Pug"};
        public static bool Instrument(string station)=>station=="guitar" || station=="keyboard" || station=="drums";
        public static bool Point(float x,float y)=>KeepyRules.Finite(x) && KeepyRules.Finite(y) && x>=0 && x<=4800 && y>=0 && y<=500;
        public static BirthdayState Create(string[] actors)=>new BirthdayState {
            members=actors.Select((id,i)=>new BirthdayMember{actor=id,role=Roles[i]}).ToArray(),
            cast=CreateCast(),guestOrder=Enumerable.Range(0,Guests.Length).ToArray(),
            props=Enumerable.Range(0,8).Select(i=>new BirthdayProp{id="birthday-gift-"+i,kind="gift",style=i%4,fake=i==7,x=650+i*48,y=160})
                .Concat(new[]{new BirthdayProp{id="birthday-battery",kind="battery",x=2800,y=180,location="socket"}})
                .Concat(Enumerable.Range(0,4).Select(i=>new BirthdayProp{id="birthday-food-"+i,kind="food",style=i,x=2400+i*60,y=170,location="stock"}))
                .Concat(Enumerable.Range(0,4).Select(i=>new BirthdayProp{id="birthday-drink-"+i,kind="drink",style=i,x=2600+i*55,y=170,location="stock"}))
                .Concat(Enumerable.Range(0,8).Select(i=>new BirthdayProp{id="birthday-brick-"+i,kind="brick",style=i%4,x=4050+i*55,y=140}))
                .Concat(Enumerable.Range(8,2).Select(i=>new BirthdayProp{id="birthday-gift-"+i,kind="gift",style=i%4,x=2300,y=180,location="stock"})).ToArray()
        };
        public static BirthdayNpc[] CreateCast()=>Roles.Select((role,i)=>new BirthdayNpc{id="birthday-companion-"+i,role=role,active=true,x=2300+i*125,y=180+i*25})
            .Concat(Enumerable.Range(0,3).Select(i=>new BirthdayNpc{id="birthday-guest-"+i,role=Guests[i],x=0,y=180})).ToArray();
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
            // The checked foundation candidate has real prop identities. Upgrade
            // additively; never rebuild its inventory or replace existing progress.
            if(snapshot?.birthday!=null && b.version==1 && b.props!=null && b.props.Length==25){
                b.cast=HulkBirthday.CreateCast();b.guestOrder=Enumerable.Range(0,HulkBirthday.Guests.Length).ToArray();b.nextGuest=b.clock+25;
                b.props=b.props.Concat(HulkBirthday.Create(Array.Empty<string>()).props.Skip(25)).ToArray();b.version=2;
            }
            if(snapshot?.birthday!=null && b.version==2){
                b.giftAt=-1;b.giftId="";b.giftActor="";b.stickerX=(b.stickers??Array.Empty<int>()).Select((_,i)=>.3f+i%4*.12f).ToArray();b.stickerY=b.stickerX.Select(_=>.5f).ToArray();b.version=3;
                foreach(var prop in b.props.Where(p=>(p.kind=="food" || p.kind=="drink") && p.version==0 && p.holder==""))prop.location="stock";
            }
        }
        private static void RestoreBirthday(SoloSnapshot snapshot)
        {
            var b=snapshot.birthday;if(b==null)return;
            var changed=b.songAt>=0 || b.members.Any(m=>m.station!="");
            foreach(var member in b.members){member.station="";member.pose="Idle";}
            CancelBirthdayUnwrap(b);
            foreach(var npc in b.cast)npc.controller="";
            foreach(var prop in b.props.Where(p=>b.members.Any(m=>m.actor==p.holder))){
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
            if(b==null || b.version!=3 || b.visit<0 || b.cakeBase<0 || b.cakeBase>2 || b.track<0 || b.track>2 || b.villain< -1 || b.villain>3 || b.celebrations<0 ||
                !KeepyRules.Finite(b.clock) || b.clock<0 || !KeepyRules.Finite(b.nextTrouble) || b.nextTrouble<0 || b.stolen==null || b.reaction==null ||
                new[]{b.songAt,b.villainAt,b.introAt,b.reactionAt}.Any(t=>!KeepyRules.Finite(t) || t< -1 || t>b.clock) || b.stickers==null || b.stickers.Length>32 || b.stickers.Any(n=>n<0 || n>31) ||
                b.members==null || !b.members.Select(m=>m?.actor).OrderBy(x=>x).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x)) ||
                b.members.Select(m=>m.role).Distinct().Count()!=b.members.Length || b.members.Any(m=>!HulkBirthday.Roles.Contains(m.role) || m.station==null || m.pose==null || !KeepyRules.Finite(m.actionAt) || m.actionAt<0 || m.actionAt>b.clock || m.station!="" && !HulkBirthday.Stations.Contains(m.station)) ||
                b.cast==null || b.cast.Length!=7 || b.cast.Where((n,i)=>n==null || n.id!=HulkBirthday.CreateCast()[i].id || n.role==null || !(i<4?n.role==HulkBirthday.Roles[i]:HulkBirthday.Guests.Contains(n.role)) || n.controller==null || n.controller!="" && !s.players.Any(p=>p.id==n.controller) || n.phase==null || !new[]{"idle","enter","wave","leave"}.Contains(n.phase) || !HulkBirthday.Point(n.x,n.y) || !HulkBirthday.Point(n.targetX,n.targetY) || !KeepyRules.Finite(n.actionAt) || n.actionAt<0 || n.actionAt>b.clock || !KeepyRules.Finite(n.leaveAt) || n.leaveAt<0).Any() ||
                b.guestOrder==null || !b.guestOrder.OrderBy(n=>n).SequenceEqual(Enumerable.Range(0,HulkBirthday.Guests.Length)) || b.guestCursor<0 || !KeepyRules.Finite(b.nextGuest) || b.nextGuest<0 ||
                b.props==null || b.props.Length!=27 || b.props.Any(p=>p==null || !Id(p.id) || p.holder==null || p.location==null || !HulkBirthday.Point(p.x,p.y) || p.version<0 || p.style<0 || p.style>3 || !new[]{"gift","battery","food","drink","brick"}.Contains(p.kind)) ||
                !b.props.Select(p=>p.id+":"+p.kind).SequenceEqual(HulkBirthday.Create(s.players.Select(p=>p.id).ToArray()).props.Select(p=>p.id+":"+p.kind)) ||
                b.props.Any(p=>p.holder!="" && p.holder!="villain" && !b.cast.Any(n=>n.active && n.id==p.holder) && (!b.members.Any(m=>m.actor==p.holder) || s.players.Single(m=>m.id==p.holder).zone!=HulkBirthday.Zone)) ||
                b.props.Where(p=>p.holder!="").GroupBy(p=>p.holder).Any(g=>g.Count()>1) || b.members.Where(m=>HulkBirthday.Instrument(m.station)).GroupBy(m=>m.station).Any(g=>g.Count()>1))
                throw new InvalidOperationException("Invalid Hulk birthday world.");
            ValidateBirthdayParty(b);
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
            if(BirthdayPartyOperation(c,player,member,held,out var partyOutcome))return partyOutcome;
            if(c.value=="take"){
                var npc=b.cast.FirstOrDefault(n=>n.id==c.target && n.active && n.controller=="");
                if(npc==null || prop==null || prop.holder!=npc.id || prop.location=="unwrapping" || held!=null || member.station!="")return "hands-busy";
                if(Math.Abs(npc.x-player.x)>InteractionRadius || Math.Abs(npc.y-player.y)>InteractionRadius)return "come-closer";
                prop.holder=player.id;prop.location="held";prop.version++;BirthdayPose(member,"Lift");npc.phase="wave";npc.actionAt=b.clock;return null;
            }
            if(c.value=="role") {
                if(!HulkBirthday.Roles.Contains(c.target))return "choose-a-party-friend";
                if(b.members.Any(m=>m.actor!=player.id && m.role==c.target))return "friend-already-chosen";
                if(held!=null || member.station!="")return "put-down-and-leave-first";
                member.role=c.target;member.pose="Idle";member.actionAt=b.clock;return null;
            }
            if(c.value=="claim") {
                if(prop==null || prop.holder!="" || prop.location=="unwrapping" || prop.location=="stock")return "prop-already-in-use";
                if(held!=null || member.station!="")return "hands-busy";
                if(Math.Abs(prop.x-player.x)>InteractionRadius || Math.Abs(prop.y-player.y)>InteractionRadius)return "come-closer";
                prop.holder=player.id;prop.location="held";prop.version++;BirthdayPose(member,"Lift");return null;
            }
            if(c.value=="drop") {
                if(held==null || held.id!=c.item || held.location=="unwrapping" || member.station=="eat" || member.station=="drink")return "not-holding-that";
                if(!HulkBirthday.Point(c.x,c.y) || Math.Abs(c.x-player.x)>InteractionRadius || Math.Abs(c.y-player.y)>InteractionRadius)return "put-it-nearby";
                held.holder="";held.location="ground";held.x=c.x;held.y=c.y;held.version++;BirthdayPose(member,"Idle");return null;
            }
            if(c.value=="give") {
                var npc=b.cast.FirstOrDefault(n=>n.id==c.target && n.active && n.controller=="");
                if(npc!=null){
                    if(held==null || held.id!=c.item || b.props.Any(p=>p.holder==npc.id))return "friends-hands-busy";
                    if(Math.Abs(npc.x-player.x)>InteractionRadius || Math.Abs(npc.y-player.y)>InteractionRadius)return "come-closer";
                    held.holder=npc.id;held.version++;npc.phase="wave";npc.actionAt=b.clock;BirthdayPose(member,"Idle");return null;
                }
                var other=b.members.SingleOrDefault(m=>m.actor==c.target);
                var receiver=state.players.SingleOrDefault(p=>p.id==c.target);
                if(held==null || held.id!=c.item || other==null || other==member || receiver.zone!=HulkBirthday.Zone || connected!=null && !connected.Contains(other.actor))return "choose-a-party-helper";
                if(other.station!="" || b.props.Any(p=>p.holder==other.actor))return "friends-hands-busy";
                if(Math.Abs(receiver.x-player.x)>InteractionRadius || Math.Abs(receiver.y-player.y)>InteractionRadius)return "come-closer";
                // One authority transaction transfers the prop; no release/claim gap.
                held.holder=other.actor;held.version++;BirthdayPose(member,"Idle");BirthdayPose(other,"Lift");return null;
            }
            if(c.value=="station") {
                if(c.target!="" && !new[]{"guitar","keyboard","drums","dance","castle"}.Contains(c.target))return "choose-a-party-activity";
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
            if(member.station=="unwrap")CancelBirthdayUnwrap(state.birthday);
            member.station="";BirthdayPose(member,"Idle");return true;
        }
        public bool ReleaseBirthday(string actor)
        {
            var b=state.birthday;var member=b?.members.FirstOrDefault(m=>m.actor==actor);if(member==null)return false;
            var player=state.players.Single(p=>p.id==actor);var changed=member.station!="";
            if(b.giftActor==actor)CancelBirthdayUnwrap(b);
            var companion=b.cast.First(n=>n.role==member.role);
            if(companion.controller==actor){companion.x=player.x;companion.y=player.y;companion.controller="";changed=true;}
            member.station="";member.pose="Idle";
            foreach(var prop in b.props.Where(p=>p.holder==actor)){prop.holder="";prop.location="ground";prop.x=player.x;prop.y=player.y;prop.version++;changed=true;}
            if(changed)state.revision++;return changed;
        }
        private bool AdvanceBirthday(double seconds,string[] active,out bool visible)
        {
            visible=false;var b=state.birthday;
            if(b==null || !state.players.Any(p=>p.zone==HulkBirthday.Zone && (active==null || active.Contains(p.id))))return false;
            b.clock+=seconds;
            visible|=AdvanceBirthdayCast(seconds,active);
            visible|=AdvanceBirthdayParty();
            foreach(var m in b.members)if(m.station!="" && (state.players.Single(p=>p.id==m.actor).zone!=HulkBirthday.Zone || active!=null && !active.Contains(m.actor))){ReleaseBirthday(m.actor);visible=true;}
            if(b.songAt>=0 && !b.members.Any(m=>HulkBirthday.Instrument(m.station))){b.songAt=-1;visible=true;}
            return true;
        }
        private bool AdvanceBirthdayCast(double seconds,string[] active)
        {
            var b=state.birthday;bool changed=false;
            foreach(var npc in b.cast.Take(4)){
                var member=b.members.FirstOrDefault(m=>m.role==npc.role && state.players.Any(p=>p.id==m.actor && p.zone==HulkBirthday.Zone && (active==null || active.Contains(p.id))));
                string controller=member?.actor??"";
                if(controller!=npc.controller){
                    // One role has one body. If a family member takes control,
                    // transfer its NPC prop atomically or leave it safely nearby.
                    var prop=b.props.SingleOrDefault(p=>p.holder==npc.id);
                    if(controller!="" && prop!=null){
                        prop.holder=b.props.Any(p=>p.holder==controller)?"":controller;prop.location=prop.holder==""?"ground":"held";prop.x=npc.x;prop.y=npc.y;prop.version++;
                    }
                    npc.controller=controller;changed=true;
                }
                if(member!=null){var player=state.players.Single(p=>p.id==member.actor);npc.x=player.x;npc.y=player.y;}
            }
            if(b.introAt<0){
                b.introAt=b.clock;
                // Source strategy randomizes a unique ordered thirteen-person
                // pool. Store the resulting order, rather than rerolling on join.
                int seed=17;unchecked{foreach(char c in state.worldId)seed=seed*31+c;}
                var random=new Random(seed);
                for(int i=b.guestOrder.Length-1;i>0;i--){int j=random.Next(i+1);int old=b.guestOrder[i];b.guestOrder[i]=b.guestOrder[j];b.guestOrder[j]=old;}
                BirthdaySpawnGuest(0);BirthdaySpawnGuest(1);changed=true;
            }
            if(!b.introDone && b.clock-b.introAt>=6){b.introDone=true;changed=true;}
            if(b.clock>=b.nextGuest){
                var available=Array.FindIndex(b.cast,4,3,n=>!n.active);
                if(available>=0)changed|=BirthdaySpawnGuest(available-4);
                b.nextGuest=b.clock+20+(b.guestCursor*7%11);changed=true;
            }
            foreach(var npc in b.cast){
                if(!npc.active || npc.controller!="")continue;
                if(npc.phase=="enter" || npc.phase=="leave"){
                    double dx=npc.targetX-npc.x,dy=npc.targetY-npc.y,distance=Math.Sqrt(dx*dx+dy*dy),step=Math.Min(distance,Walking.Speed*seconds);
                    if(distance>.001){npc.x+=(float)(dx/distance*step);npc.y+=(float)(dy/distance*step);changed=true;}
                    if(distance<=step+.01){
                        if(npc.phase=="leave"){
                            foreach(var prop in b.props.Where(p=>p.holder==npc.id)){prop.holder="";prop.location="table";prop.x=2380;prop.y=180;prop.version++;}
                            npc.active=false;npc.phase="idle";
                        }else {npc.phase="wave";npc.actionAt=b.clock;}
                        changed=true;
                    }
                }else if(npc.id.StartsWith("birthday-guest-",StringComparison.Ordinal)){
                    if(b.clock-npc.actionAt>=3 && npc.phase=="wave"){
                        foreach(var prop in b.props.Where(p=>p.holder==npc.id)){prop.holder="";prop.location="table";prop.x=npc.x+45;prop.y=npc.y;prop.version++;}
                        npc.phase="idle";changed=true;
                    }
                    if(b.clock>=npc.leaveAt){npc.phase="leave";npc.targetX=npc.x<2400?0:4800;npc.targetY=180;changed=true;}
                }
            }
            return changed;
        }
        private bool BirthdaySpawnGuest(int slot)
        {
            var b=state.birthday;var gift=b.props.FirstOrDefault(p=>p.kind=="gift" && p.location=="stock" && p.holder=="");
            if(gift==null)return false;
            var npc=b.cast[4+slot];npc.active=true;npc.controller="";npc.phase="enter";npc.role=HulkBirthday.Guests[b.guestOrder[b.guestCursor++%b.guestOrder.Length]];
            npc.x=slot%2==0?0:4800;npc.y=180;npc.targetX=2450+slot*120;npc.targetY=180+slot*40;npc.actionAt=b.clock;npc.leaveAt=b.clock+40;
            gift.holder=npc.id;gift.location="held";gift.version++;return true;
        }
    }
}
