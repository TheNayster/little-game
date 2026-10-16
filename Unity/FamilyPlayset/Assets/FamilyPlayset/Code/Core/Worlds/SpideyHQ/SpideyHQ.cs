using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class SpideyMember
    {
        public string actor,role,station="",pose="idle";
        public double at,until;
        public float startX,startY;
        public int seat=-1;
        public bool declinedBattle;
        public SpideyMember Copy()=>(SpideyMember)MemberwiseClone();
    }
    [Serializable] public sealed class SpideyProp
    {
        public string id,kind,holder="",location="stock";
        public float x,y;
        public int version;
        public double releasedAt=-1;
        public SpideyProp Copy()=>(SpideyProp)MemberwiseClone();
    }
    [Serializable] public sealed class SpideyBrick
    {
        public string id,owner="";
        public int variant,turn,version;
        public float x,y;
        public SpideyBrick Copy()=>(SpideyBrick)MemberwiseClone();
    }
    [Serializable] public sealed class SpideyNpc
    {
        public string role,pose="idle";
        public float x,y,targetX,targetY;
        public double at,next;
        public SpideyNpc Copy()=>(SpideyNpc)MemberwiseClone();
    }
    [Serializable] public sealed class SpideyState
    {
        public int version=1,poster,photos,globe,dominoes,recoveries,thefts,brickSerial,puzzleImage,puzzleSize=2,puzzleRevision,puzzles;
        public double clock,nextTheft=24,battleAt=-1,globeAt=-1,photoAt=-1,dominoAt=-1,puzzleDoneAt=-1,introAt=-1,posterAt=-1;
        public string battle="idle",stolen="",photoActor="";
        public uint seed=1977;
        public SpideyMember[] members;
        public SpideyProp[] props;
        public SpideyBrick[] bricks=Array.Empty<SpideyBrick>();
        public SpideyNpc[] cast;
        public string[] pizzaQueue=Array.Empty<string>(),balloonQueue=Array.Empty<string>(),slideQueue=Array.Empty<string>(),webActors=Array.Empty<string>();
        public bool[] heroLinks=new bool[3];
        public int[] puzzlePlaced=new int[4];
        public string[] puzzleHolders=new string[4];
        public SpideyState Copy()=>new SpideyState{version=version,poster=poster,photos=photos,globe=globe,dominoes=dominoes,recoveries=recoveries,thefts=thefts,brickSerial=brickSerial,puzzleImage=puzzleImage,puzzleSize=puzzleSize,puzzleRevision=puzzleRevision,puzzles=puzzles,clock=clock,nextTheft=nextTheft,battleAt=battleAt,globeAt=globeAt,photoAt=photoAt,dominoAt=dominoAt,puzzleDoneAt=puzzleDoneAt,introAt=introAt,posterAt=posterAt,battle=battle,stolen=stolen,photoActor=photoActor,seed=seed,members=members.Select(m=>m.Copy()).ToArray(),props=props.Select(p=>p.Copy()).ToArray(),bricks=bricks.Select(p=>p.Copy()).ToArray(),cast=cast.Select(n=>n.Copy()).ToArray(),pizzaQueue=pizzaQueue.ToArray(),balloonQueue=balloonQueue.ToArray(),slideQueue=slideQueue.ToArray(),webActors=webActors.ToArray(),heroLinks=heroLinks.ToArray(),puzzlePlaced=puzzlePlaced.ToArray(),puzzleHolders=puzzleHolders.ToArray()};
    }
    public static class SpideyHQ
    {
        public const string Zone="spidey-hq";
        public const int Schema=55;
        public static readonly string[] Heroes={"Spiderman","Spiderman_Morales","Spiderman_Gwen"};
        public static readonly string[] Roles={"Spiderman","Spiderman_Morales","Spiderman_Gwen","BlueBlazerGuy"};
        public static readonly string[] Cast={"Spiderman","Spiderman_Morales","Spiderman_Gwen","GreenGoblin","PizzaGuy","BalloonSeller","LavalliereLady","BlueBlazerLady","BlueJacketBoy","SmileyJacketBoy","GroovyGranny","BlueBlazerGuy","DungareesGirl","HeartsBlouseGirl"};
        public static readonly string[] Stations={"pizza","balloon","slide","eat","sit","sleep","float","photo","web","invisible","sense","fall","puzzle","build"};
        public static float X(string station)=>station=="pizza"?1671:station=="balloon"?2890:station=="slide"?2578:station=="poster"?2149:station=="globe"?2378:station=="build"?3414:station=="puzzle"?1068:station=="photo"?2273:station=="web"?2400:station=="domino"?2420:2200;
        public static bool Point(float x,float y)=>KeepyRules.Finite(x) && KeepyRules.Finite(y) && x>=0 && x<=4800 && y>=0 && y<=500;
        public static SpideyState Create(string[] actors)=>new SpideyState{
            members=actors.Select((a,i)=>new SpideyMember{actor=a,role=Roles[i]}).ToArray(),
            props=Enumerable.Range(0,30).Select(i=>new SpideyProp{id="hq-prop-"+i,kind=i<15?"pizza":"balloon",x=X(i<15?"pizza":"balloon"),y=160}).ToArray(),
            cast=Cast.Select((role,i)=>new SpideyNpc{role=role,x=i==4?X("pizza"):i==5?X("balloon"):1450+i*130,y=180+i%3*35,targetX=1450+i*130,targetY=180+i%3*35,next=5+i*1.7}).ToArray(),
            puzzleHolders=Enumerable.Repeat("",4).ToArray()
        };
    }
    public sealed partial class GameWorld
    {
        public SpideyState ReadSpidey()=>state.spidey?.Copy();
        private static void NormalizeSpidey(SoloSnapshot s)
        {
            // JsonUtility may instantiate an absent inline object in an old save.
            // Only the completely empty legacy shape is treated as absent.
            var h=s?.spidey;if(s!=null && s.schema<SpideyHQ.Schema && h!=null && h.clock==0 && (h.members==null || h.members.Length==0) && (h.props==null || h.props.Length==0))s.spidey=null;
        }
        private static void ValidateSpidey(SoloSnapshot s)
        {
            var h=s.spidey;if(h==null && s.schema<SpideyHQ.Schema)return;
            if(h==null || h.version!=1 || !KeepyRules.Finite(h.clock) || h.clock<0 || h.poster<0 || h.poster>2 || h.globe<0 || h.globe>1 || h.photos<0 || h.dominoes<0 || h.recoveries<0 || h.thefts<0 || h.brickSerial<0 || h.puzzleRevision<0 || h.puzzles<0 || h.puzzleImage<0 || h.puzzleImage>4 || h.puzzleSize<2 || h.puzzleSize>5 ||
                h.members==null || !h.members.Select(m=>m?.actor).OrderBy(a=>a).SequenceEqual(s.players.Select(p=>p.id).OrderBy(a=>a)) || h.members.Any(m=>m==null || !SpideyHQ.Roles.Contains(m.role) || m.pose==null || m.station==null || m.station!="" && !SpideyHQ.Stations.Contains(m.station) || !KeepyRules.Finite(m.at) || !KeepyRules.Finite(m.until) || m.at<0 || m.at>h.clock || m.until<0 || !SpideyHQ.Point(m.startX,m.startY) || m.seat< -1 || m.seat>5) || h.members.Select(m=>m.role).Distinct().Count()!=h.members.Length ||
                h.props==null || h.props.Length!=30 || h.props.Where((p,i)=>p==null || p.id!="hq-prop-"+i || p.kind!=(i<15?"pizza":"balloon") || p.holder==null || p.location==null || !new[]{"stock","held","ground","goblin"}.Contains(p.location) || p.version<0 || !KeepyRules.Finite(p.releasedAt) || p.releasedAt< -1 || p.releasedAt>h.clock || !SpideyHQ.Point(p.x,p.y) || p.holder!="" && p.holder!="goblin" && !SpideyHQ.Cast.Any(role=>p.holder=="hq-npc-"+role) && !s.players.Any(a=>a.id==p.holder && a.zone==SpideyHQ.Zone)).Any() || h.props.Where(p=>p.holder!="" && p.holder!="goblin").GroupBy(p=>p.holder).Any(g=>g.Count()>1) ||
                h.cast==null || h.cast.Length!=SpideyHQ.Cast.Length || h.cast.Where((n,i)=>n==null || n.role!=SpideyHQ.Cast[i] || n.pose==null || !SpideyHQ.Point(n.x,n.y) || !SpideyHQ.Point(n.targetX,n.targetY) || !KeepyRules.Finite(n.at) || !KeepyRules.Finite(n.next) || n.at<0 || n.at>h.clock || n.next<0).Any() ||
                h.bricks==null || h.bricks.Length>96 || h.bricks.Any(b=>b==null || !Id(b.id) || b.owner==null || b.owner!="" && !s.players.Any(p=>p.id==b.owner && p.zone==SpideyHQ.Zone) || b.variant<0 || b.variant>=26 || b.turn<0 || b.turn>3 || b.version<0 || !SpideyHQ.Point(b.x,b.y)) || h.bricks.Select(b=>b.id).Distinct().Count()!=h.bricks.Length ||
                h.puzzlePlaced==null || h.puzzlePlaced.Length!=h.puzzleSize*h.puzzleSize || h.puzzlePlaced.Any(v=>v<0 || v>1) || h.puzzleHolders==null || h.puzzleHolders.Length!=h.puzzlePlaced.Length || h.puzzleHolders.Any(a=>a==null || a!="" && !s.players.Any(p=>p.id==a && p.zone==SpideyHQ.Zone)) || h.puzzleHolders.Where(a=>a!="").GroupBy(a=>a).Any(g=>g.Count()>1) ||
                h.heroLinks==null || h.heroLinks.Length!=3 || h.webActors==null || h.webActors.Distinct().Count()!=h.webActors.Length || h.webActors.Any(a=>!s.players.Any(p=>p.id==a)) || h.battle==null || !new[]{"idle","steal","fight","pull","celebrate"}.Contains(h.battle) || h.stolen==null || h.stolen!="" && !h.props.Any(p=>p.id==h.stolen) || h.photoActor==null || new[]{h.nextTheft,h.battleAt,h.globeAt,h.photoAt,h.dominoAt,h.puzzleDoneAt,h.introAt,h.posterAt}.Any(t=>!KeepyRules.Finite(t) || t< -1) ||
                new[]{h.pizzaQueue,h.balloonQueue,h.slideQueue}.Any(q=>q==null || q.Length>4 || q.Distinct().Count()!=q.Length || q.Any(a=>!s.players.Any(p=>p.id==a && p.zone==SpideyHQ.Zone))))throw new InvalidOperationException("Invalid Spidey HQ world.");
        }
        private static void RestoreSpidey(SoloSnapshot s)
        {
            var h=s.spidey;if(h==null)return;
            h.pizzaQueue=h.balloonQueue=h.slideQueue=h.webActors=Array.Empty<string>();
            foreach(var m in h.members){m.station="";m.pose="idle";m.seat=-1;}
            foreach(var p in h.props.Where(p=>p.holder!="")){p.holder="";p.location="ground";p.version++;}
            foreach(var b in h.bricks)b.owner="";for(int i=0;i<h.puzzleHolders.Length;i++)h.puzzleHolders[i]="";
            foreach(var n in h.cast){n.pose="idle";n.next=h.clock+5;}
            h.battle="idle";h.stolen="";h.nextTheft=h.clock+24;h.heroLinks=new bool[3];h.photoActor="";
        }
        public void ReleaseSpidey(string actor)
        {
            var h=state.spidey;if(h==null)return;LeaveSpideyStation(actor);
            foreach(var p in h.props.Where(p=>p.holder==actor)){var a=state.players.Single(x=>x.id==actor);p.holder="";p.location="ground";p.x=a.x;p.y=a.y;p.version++;}
            foreach(var b in h.bricks.Where(b=>b.owner==actor)){b.owner="";b.version++;}
            h.webActors=h.webActors.Where(a=>a!=actor).ToArray();for(int i=0;i<h.puzzleHolders.Length;i++)if(h.puzzleHolders[i]==actor)h.puzzleHolders[i]="";
        }
        private bool LeaveSpideyStation(string actor)
        {
            var h=state.spidey;var m=h?.members.FirstOrDefault(v=>v.actor==actor);if(m==null)return false;
            bool changed=m.station!="";m.station="";m.pose="idle";m.seat=-1;m.at=h.clock;
            h.pizzaQueue=h.pizzaQueue.Where(a=>a!=actor).ToArray();h.balloonQueue=h.balloonQueue.Where(a=>a!=actor).ToArray();h.slideQueue=h.slideQueue.Where(a=>a!=actor).ToArray();
            for(int i=0;i<h.puzzleHolders.Length;i++)if(h.puzzleHolders[i]==actor){h.puzzleHolders[i]="";h.puzzleRevision++;changed=true;}
            foreach(var brick in h.bricks.Where(b=>b.owner==actor)){brick.owner="";brick.version++;changed=true;}
            return changed;
        }
        private void SpideyPose(SpideyMember member,string pose,string station,double duration)
        {member.pose=pose;member.station=station;member.at=state.spidey.clock;member.until=member.at+duration;var p=state.players.Single(a=>a.id==member.actor);member.startX=p.x;member.startY=p.y;}
        private static double SpideyRandom(SpideyState h,double low,double high){unchecked{h.seed=h.seed*1664525u+1013904223u;}return low+(h.seed%100000)/100000d*(high-low);}
        private string SpideyOperation(SoloCommand c,SoloPlayer player,string[] connected)
        {
            var h=state.spidey;if(h==null || player.zone!=SpideyHQ.Zone)return "come-to-spidey-hq";
            var member=h.members.Single(m=>m.actor==player.id);var held=h.props.FirstOrDefault(p=>p.holder==player.id);
            bool Near(string station)=>Math.Abs(player.x-SpideyHQ.X(station))<=180;
            if(c.value=="leave"){LeaveSpideyStation(player.id);if(h.battle!="idle"){member.declinedBattle=true;h.webActors=h.webActors.Where(a=>a!=player.id).ToArray();}return null;}
            if(c.value=="role"){
                if(!SpideyHQ.Roles.Contains(c.target))return "choose-a-hero";
                var other=h.members.FirstOrDefault(m=>m!=member && m.role==c.target);
                if(other!=null && state.players.Any(p=>p.id==other.actor && p.zone==SpideyHQ.Zone && (connected==null || connected.Contains(p.id))))return "friend-already-chosen";
                if(held!=null || member.station!="")return "put-down-and-leave-first";if(other!=null)other.role=member.role;member.role=c.target;return null;
            }
            if(c.value=="queue"){
                if(!new[]{"pizza","balloon","slide"}.Contains(c.target) || !Near(c.target))return "come-closer";
                if(held!=null || member.station!="")return "hands-busy";
                if(c.target=="pizza")h.pizzaQueue=h.pizzaQueue.Concat(new[]{player.id}).ToArray();else if(c.target=="balloon")h.balloonQueue=h.balloonQueue.Concat(new[]{player.id}).ToArray();else h.slideQueue=h.slideQueue.Concat(new[]{player.id}).ToArray();
                SpideyPose(member,"idle",c.target,999);return null;
            }
            if(c.value=="claim"){
                var p=h.props.SingleOrDefault(v=>v.id==c.item);if(p==null || p.holder!="" || p.location!="ground" || held!=null || member.station!="")return "hands-busy";
                if(Math.Abs(player.x-p.x)>InteractionRadius || Math.Abs(player.y-p.y)>InteractionRadius)return "come-closer";p.holder=player.id;p.location="held";p.version++;return null;
            }
            if(c.value=="drop"){
                if(held==null || held.id!=c.item || !SpideyHQ.Point(c.x,c.y) || Math.Abs(c.x-player.x)>InteractionRadius || Math.Abs(c.y-player.y)>InteractionRadius)return "put-it-nearby";
                LeaveSpideyStation(player.id);held.holder="";held.location="ground";held.x=c.x;held.y=c.y;held.version++;return null;
            }
            if(c.value=="give"){
                var npc=h.cast.FirstOrDefault(n=>"hq-npc-"+n.role==c.target && n.role!="GreenGoblin");
                if(npc!=null){
                    if(held==null || held.id!=c.item || h.props.Any(p=>p.holder==c.target) || h.members.Any(m=>m.role==npc.role && state.players.Any(p=>p.id==m.actor && p.zone==SpideyHQ.Zone && (connected==null || connected.Contains(p.id)))) || Math.Abs(npc.x-player.x)>InteractionRadius || Math.Abs(npc.y-player.y)>InteractionRadius)return "friends-hands-busy";
                    LeaveSpideyStation(player.id);held.holder=c.target;held.version++;npc.pose=held.kind=="pizza"?"eat":"hang";npc.at=h.clock;npc.next=h.clock+5;return null;
                }
                var other=state.players.FirstOrDefault(p=>p.id==c.target && p.zone==SpideyHQ.Zone);if(held==null || held.id!=c.item || other==null || other.id==player.id || h.props.Any(p=>p.holder==other.id) || Math.Abs(other.x-player.x)>InteractionRadius || Math.Abs(other.y-player.y)>InteractionRadius)return "friends-hands-busy";
                LeaveSpideyStation(player.id);held.holder=other.id;held.version++;return null;
            }
            if(c.value=="npc-take" || c.value=="npc-greet"){
                var npc=h.cast.FirstOrDefault(n=>"hq-npc-"+n.role==c.target && n.role!="GreenGoblin");if(npc==null || Math.Abs(npc.x-player.x)>InteractionRadius || Math.Abs(npc.y-player.y)>InteractionRadius || h.members.Any(m=>m.role==npc.role && state.players.Any(p=>p.id==m.actor && p.zone==SpideyHQ.Zone && (connected==null || connected.Contains(p.id)))))return "come-closer";
                if(c.value=="npc-take"){
                    var prop=h.props.FirstOrDefault(p=>p.id==c.item && p.holder==c.target);if(held!=null || prop==null)return "hands-busy";prop.holder=player.id;prop.version++;
                }
                npc.pose="wave";npc.at=h.clock;npc.next=h.clock+3;return null;
            }
            if(c.value=="eat"){
                if(held==null || held.kind!="pizza" || member.station!="")return "bring-pizza";SpideyPose(member,"eat","eat",3);return null;
            }
            if(c.value=="float"){
                if(held==null || held.kind!="balloon" || member.station!="")return "bring-balloon";SpideyPose(member,"hang","float",12);return null;
            }
            if(c.value=="seat" || c.value=="sleep"){
                if(!int.TryParse(c.item,out var seat) || seat<0 || seat>5 || Math.Abs(player.x-SpideyHQLayout.SeatX[seat])>180 || h.members.Any(m=>m!=member && m.seat==seat))return "seat-in-use";
                LeaveSpideyStation(player.id);member.seat=seat;player.x=SpideyHQLayout.SeatX[seat];player.y=SpideyHQLayout.SeatY[seat];SpideyPose(member,c.value=="sleep"?"sleep":"sit",c.value=="sleep"?"sleep":"sit",999);return null;
            }
            if(c.value=="poster" || c.value=="globe" || c.value=="photo" || c.value=="domino"){
                if(!Near(c.value))return "come-closer";
                if(c.value=="poster"){if(h.clock-h.posterAt<.6)return "wait-for-animation";h.poster=(h.poster+1)%3;h.posterAt=h.clock;}
                if(c.value=="globe"){if(h.clock-h.globeAt<.5)return "wait-for-animation";h.globe=1-h.globe;h.globeAt=h.clock;}
                if(c.value=="photo"){if(h.clock-h.photoAt<3)return "wait-for-photo";h.photoAt=h.clock;h.photos++;h.photoActor=player.id;SpideyPose(member,"photo","photo",3);}
                if(c.value=="domino"){
                    if(h.clock-h.dominoAt<5)return "wait-for-animation";h.dominoes++;h.dominoAt=h.clock;
                    foreach(var m in h.members.Where(m=>state.players.Single(p=>p.id==m.actor).zone==SpideyHQ.Zone && Math.Abs(state.players.Single(p=>p.id==m.actor).x-player.x)<200 && m.station==""))SpideyPose(m,"balance","fall",3);
                    foreach(var n in h.cast.Where(n=>n.role!="GreenGoblin" && Math.Abs(n.x-player.x)<200)){n.pose="balance";n.at=h.clock;n.next=h.clock+3;}
                }
                return null;
            }
            if(c.value=="invisible" || c.value=="sense"){
                if(member.station!="")return "leave-activity-first";
                if(c.value=="invisible" && member.role!="Spiderman_Morales" || c.value=="sense" && member.role!="Spiderman")return "choose-that-hero-first";
                SpideyPose(member,c.value=="invisible"?"happy":"wave",c.value,c.value=="invisible"?3.5:2.1);return null;
            }
            if(c.value=="web"){
                if(h.battle!="fight" || !Near("web"))return "come-to-the-goblin";
                member.declinedBattle=false;
                h.webActors=h.webActors.Concat(new[]{player.id}).Distinct().ToArray();var role=Array.IndexOf(SpideyHQ.Heroes,member.role);if(role>=0)h.heroLinks[role]=true;else{var missing=Array.FindIndex(h.heroLinks,v=>!v);if(missing>=0)h.heroLinks[missing]=true;}
                SpideyPose(member,"web-right","web",3);return null;
            }
            if(c.value.StartsWith("brick-",StringComparison.Ordinal))return SpideyBuildOperation(c,player,member);
            if(c.value.StartsWith("puzzle-",StringComparison.Ordinal))return SpideyPuzzleOperation(c,player,member);
            return "unknown-spidey-action";
        }
        private bool AdvanceSpidey(double seconds,string[] active,out bool visible)
        {
            visible=false;var h=state.spidey;if(h==null)return false;
            var before=SpideyEventStamp(h);
            var present=state.players.Where(p=>p.zone==SpideyHQ.Zone && (active==null || active.Contains(p.id))).ToArray();
            foreach(var m in h.members.Where(m=>!present.Any(p=>p.id==m.actor)))ReleaseSpidey(m.actor);
            if(present.Length==0)return false;h.clock+=seconds;AmbientChanged=true;
            if(h.introAt<0)h.introAt=h.clock;
            foreach(var prop in h.props){
                if(prop.location=="ground" && prop.holder==""){
                    if(prop.releasedAt<0)prop.releasedAt=h.clock;
                    else if(h.clock-prop.releasedAt>=60){prop.location="stock";prop.releasedAt=-1;prop.version++;}
                }else prop.releasedAt=-1;
            }
            foreach(var member in h.members.Where(m=>m.station!="" && h.clock>=m.until)){
                var p=state.players.Single(a=>a.id==member.actor);
                if(member.station=="eat"){var food=h.props.FirstOrDefault(a=>a.holder==p.id && a.kind=="pizza");if(food!=null){food.holder="";food.location="stock";food.version++;}}
                if(member.station=="float"){SpideyPose(member,"fall","fall",1.5);continue;}
                LeaveSpideyStation(member.actor);
            }
            ServiceSpideyQueue("pizza",ref h.pizzaQueue,SpideyHQLayout.ServeDuration);ServiceSpideyQueue("balloon",ref h.balloonQueue,SpideyHQLayout.ServeDuration);ServiceSpideyQueue("slide",ref h.slideQueue,SpideyHQLayout.SlideDuration);
            foreach(var n in h.cast){
                if(h.members.Any(m=>m.role==n.role && present.Any(p=>p.id==m.actor))){foreach(var prop in h.props.Where(p=>p.holder=="hq-npc-"+n.role)){prop.holder="";prop.location="ground";prop.x=n.x;prop.y=n.y;prop.version++;}continue;}
                if(n.role=="GreenGoblin")continue;
                if(n.role=="PizzaGuy" || n.role=="BalloonSeller"){
                    if(h.clock>=n.next){var prop=h.props.FirstOrDefault(p=>p.holder=="hq-npc-"+n.role);if(prop!=null){prop.holder="";prop.location=prop.kind=="pizza"?"stock":"ground";prop.x=n.x;prop.y=n.y;prop.version++;}n.pose="idle";n.next=h.clock+5;}
                    continue;
                }
                if(h.clock>=n.next){
                    var carried=h.props.FirstOrDefault(p=>p.holder=="hq-npc-"+n.role);
                    if(carried!=null){carried.holder="";carried.location=carried.kind=="pizza"?"stock":"ground";carried.x=n.x;carried.y=n.y;carried.version++;}
                    n.at=h.clock;n.next=h.clock+SpideyRandom(h,6,13);
                    int activity=(int)SpideyRandom(h,0,5);
                    if(activity==0 || activity==1){n.targetX=SpideyHQ.X(activity==0?"pizza":"balloon");n.targetY=180;}
                    else if(activity==2){int seat=(int)SpideyRandom(h,0,6);n.targetX=SpideyHQLayout.SeatX[seat];n.targetY=SpideyHQLayout.SeatY[seat];}
                    else{n.targetX=(float)SpideyRandom(h,1400,3200);n.targetY=(float)SpideyRandom(h,100,350);}
                    n.pose="walk";
                }
                float dx=n.targetX-n.x,dy=n.targetY-n.y,dist=(float)Math.Sqrt(dx*dx+dy*dy),step=(float)(seconds*Walking.Speed*.35);
                if(n.pose=="walk" && dist>step){n.x+=dx/dist*step;n.y+=dy/dist*step;}else if(n.pose=="walk"){
                    n.x=n.targetX;n.y=n.targetY;n.pose="idle";
                    string kind=Math.Abs(n.x-SpideyHQ.X("pizza"))<1?"pizza":Math.Abs(n.x-SpideyHQ.X("balloon"))<1?"balloon":"";
                    if(kind!=""){
                        var prop=h.props.FirstOrDefault(p=>p.kind==kind && p.holder=="" && p.location=="stock");
                        // Humans in the service queue retain station priority.
                        if(prop!=null && (kind=="pizza"?h.pizzaQueue:h.balloonQueue).Length==0){prop.holder="hq-npc-"+n.role;prop.location="held";prop.version++;n.pose=kind=="pizza"?"eat":"hang";n.next=h.clock+5;}
                    }else if(SpideyHQLayout.SeatX.Any(x=>Math.Abs(x-n.x)<1)){n.pose="sit";n.next=h.clock+7;}
                }
            }
            AdvanceSpideyBattle(present);
            // Animation clocks and NPC movement use the existing 4 Hz ambient
            // path. Only discrete gameplay transitions invalidate commands and
            // request an immediate authoritative snapshot/save.
            visible=before!=SpideyEventStamp(h);
            return true;
        }
        private static string SpideyEventStamp(SpideyState h)=>h.battle+"/"+h.thefts+"/"+h.recoveries+"/"+h.pizzaQueue.Length+"/"+h.balloonQueue.Length+"/"+h.slideQueue.Length+"/"+string.Join(",",h.members.Select(m=>m.station+":"+m.pose))+"/"+string.Join(",",h.cast.Select(n=>n.pose))+"/"+string.Join(",",h.props.Select(p=>p.version));
        private void ServiceSpideyQueue(string station,ref string[] queue,double duration)
        {
            var h=state.spidey;if(queue.Length==0)return;var actor=queue[0];var m=h.members.Single(a=>a.actor==actor);var p=state.players.Single(a=>a.id==actor);
            if(m.station!=station){queue=queue.Skip(1).ToArray();return;}
            for(int i=0;i<queue.Length;i++){var waitingActor=queue[i];var waiting=state.players.Single(a=>a.id==waitingActor);waiting.x=SpideyHQLayout.Queue(station,i,false);waiting.y=SpideyHQLayout.Queue(station,i,true);}
            // The one shared service clock belongs to the head, not each client.
            if(m.pose=="idle"){
                m.at=h.clock;m.pose=station=="slide"?"hang":"wave";
                if(station!="slide"){var seller=h.cast.Single(n=>n.role==(station=="pizza"?"PizzaGuy":"BalloonSeller"));seller.pose="serve";seller.at=h.clock;seller.next=h.clock+duration;}
            }
            if(h.clock-m.at<duration)return;
            if(station!="slide"){
                var prop=h.props.FirstOrDefault(a=>a.kind==station && a.location=="stock" && a.holder=="");if(prop==null)return;
                prop.holder=actor;prop.location="held";prop.version++;
            }
            queue=queue.Skip(1).ToArray();m.station="";m.pose=station=="slide"?"land":"hold";m.at=h.clock;
            if(station=="slide"){p.x=SpideyHQLayout.SlidePoint(1,false);p.y=SpideyHQLayout.SlidePoint(1,true);}
        }
        private void AdvanceSpideyBattle(SoloPlayer[] present)
        {
            var h=state.spidey;
            if(h.battle=="idle" && h.clock>=h.nextTheft){
                var kind=h.thefts%2==0?"pizza":"balloon";var prop=h.props.FirstOrDefault(p=>p.kind==kind && p.holder=="");
                if(prop==null){h.nextTheft=h.clock+8;return;}
                prop.holder="goblin";prop.location="goblin";prop.version++;h.stolen=prop.id;h.battle="steal";h.battleAt=h.clock;h.heroLinks=new bool[3];h.webActors=Array.Empty<string>();h.thefts++;
                foreach(var m in h.members)m.declinedBattle=false;
            }else if(h.battle=="steal" && h.clock-h.battleAt>=3){h.battle="fight";h.battleAt=h.clock;}
            else if(h.battle=="fight"){
                var participating=present.Where(p=>!h.members.Single(m=>m.actor==p.id).declinedBattle).ToArray();
                if(participating.Length==0){var prop=h.props.Single(p=>p.id==h.stolen);prop.holder="";prop.location="stock";prop.version++;h.battle="idle";h.stolen="";h.nextTheft=h.clock+SpideyRandom(h,8,32);h.heroLinks=new bool[3];h.webActors=Array.Empty<string>();return;}
                // Source heroes missing from the human cast are available as NPC
                // helpers. Human participation is still required for each child
                // currently in the world; departure removes only that child.
                if(h.clock-h.battleAt>=3)for(int i=0;i<3;i++)if(!h.members.Any(m=>m.role==SpideyHQ.Heroes[i] && participating.Any(p=>p.id==m.actor)))h.heroLinks[i]=true;
                if(h.heroLinks.All(v=>v) && participating.All(p=>h.webActors.Contains(p.id))){h.battle="pull";h.battleAt=h.clock;}
            }else if(h.battle=="pull" && h.clock-h.battleAt>=3){
                var prop=h.props.Single(p=>p.id==h.stolen);prop.holder="";prop.location="ground";prop.x=2400;prop.y=160;prop.version++;h.battle="celebrate";h.battleAt=h.clock;h.recoveries++;
                foreach(var m in h.members.Where(m=>present.Any(p=>p.id==m.actor)))SpideyPose(m,"happy","",3);
            }else if(h.battle=="celebrate" && h.clock-h.battleAt>=8){h.battle="idle";h.stolen="";h.nextTheft=h.clock+SpideyRandom(h,80,150);h.webActors=Array.Empty<string>();h.heroLinks=new bool[3];}
        }
    }
}
