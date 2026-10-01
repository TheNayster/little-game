using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum KingdomPhase { Ready, Welcome, Supplies, Bridge, Queen, Rescue, Feast }
    [Serializable] public sealed class KingdomMember
    {
        public string actor;
        public int role;
        public bool attending,declined;
        public string carrying="";
        public KingdomMember Copy(){var copy=(KingdomMember)MemberwiseClone();copy.carrying=carrying??"";return copy;}
    }
    [Serializable] public sealed class KingdomState
    {
        public int round, supplies, boards, rescued;
        public KingdomPhase phase;
        public double clock, started, distractedUntil;
        public KingdomMember[] members;
        public string[] npcCast;
        public double[] boardAt=new double[3],wakeAt=new double[3];
        public KingdomState Copy()=>new KingdomState{round=round,supplies=supplies,boards=boards,rescued=rescued,phase=phase,clock=clock,started=started,distractedUntil=distractedUntil,boardAt=boardAt?.Length==3?boardAt.ToArray():new double[3],wakeAt=wakeAt?.Length==3?wakeAt.ToArray():new double[3],npcCast=npcCast?.ToArray(),members=members.Select(m=>m.Copy()).ToArray()};
    }
    public static class KingdomAdventure
    {
        public const int Schema=38;
        public const string Zone="imagination-adventure";
        public static readonly string[] Roles={"Explorer","Builder","Wand helper","Picnic helper"};
        public static WalkPoint Prop(string id)
        {
            switch(id){case "fruit-0":return new WalkPoint(710,180);case "fruit-1":return new WalkPoint(890,260);case "fruit-2":return new WalkPoint(1070,180);
                case "board-0":return new WalkPoint(1240,180);case "board-1":return new WalkPoint(1370,200);case "board-2":return new WalkPoint(1500,180);
                case "basket":return new WalkPoint(440,100);case "ball":return new WalkPoint(1840,190);case "wand":return new WalkPoint(2130,210);
                case "friend-0":return new WalkPoint(1750,330);case "friend-1":return new WalkPoint(1930,340);case "friend-2":return new WalkPoint(2300,330);
                default:return new WalkPoint(-1000,-1000);}
        }
        public static bool Frozen(KingdomState s,int index)=>index>=6 && s.phase<KingdomPhase.Rescue || index>=6 && s.phase==KingdomPhase.Rescue && (s.rescued&(1<<(index-6)))==0;
        private static WalkPoint Along(WalkPoint a,WalkPoint b,double fraction){var t=Math.Max(0,Math.Min(1,fraction));return new WalkPoint((float)(a.X+(b.X-a.X)*t),(float)(a.Y+(b.Y-a.Y)*t));}
        public static WalkPoint NpcPoint(KingdomState s,int index)=>NpcPoint(s,index,s.clock);
        public static WalkPoint NpcPoint(KingdomState s,int index,double clock)
        {
            var home=new[]{new WalkPoint(350,310),new WalkPoint(1120,350),new WalkPoint(1230,330),new WalkPoint(1390,340),new WalkPoint(1540,330),new WalkPoint(2110,340),Prop("friend-0"),Prop("friend-1"),Prop("friend-2")}[index];
            var age=Math.Max(0,clock-s.started);
            // Routes derive from the authoritative phase clock, so joining does
            // not replay an entrance and a suspended story cannot drift locally.
            if(s.phase==KingdomPhase.Feast){
                // Continue from the completed job, rather than returning to the
                // original entrance when the phase clock starts over.
                var from=index==0?new WalkPoint(1900,280):index==1?new WalkPoint(450,250):index>=2 && index<=4?new WalkPoint(1630+(index-2)*100,290):index>=6?Along(home,new WalkPoint(1480+(index-6)*100,245),(s.started-(s.wakeAt?[index-6]??0))/3):home;
                return Along(from,new WalkPoint(400+index*135,260+index%2*45),age/5);
            }
            if(index==0){var goal=s.phase==KingdomPhase.Supplies?new WalkPoint(950,300):s.phase==KingdomPhase.Bridge?new WalkPoint(1650,300):s.phase>=KingdomPhase.Queen?new WalkPoint(1900,280):home;var from=s.phase==KingdomPhase.Bridge?new WalkPoint(950,300):s.phase==KingdomPhase.Queen?new WalkPoint(1650,300):s.phase==KingdomPhase.Rescue?goal:home;return Along(from,goal,age/4);}
            if(index==1 && s.phase==KingdomPhase.Supplies){var apple=new WalkPoint(650,280);return age<3?Along(home,apple,age/3):Along(apple,new WalkPoint(450,250),(age-3)/3);}
            if(index==1 && s.phase>KingdomPhase.Supplies)return new WalkPoint(450,250);
            if(index>=2 && index<=4 && (s.boards&(1<<(index-2)))!=0)return Along(home,new WalkPoint(1630+(index-2)*100,290),(clock-(s.boardAt?[index-2]??0))/2);
            if(index==5 && s.phase==KingdomPhase.Queen && s.distractedUntil>s.clock)return Along(home,new WalkPoint(2240,280),(clock-(s.distractedUntil-8))/1.5);
            if(index>=6 && s.phase==KingdomPhase.Rescue && (s.rescued&(1<<(index-6)))!=0)return Along(home,new WalkPoint(1480+(index-6)*100,245),(clock-(s.wakeAt?[index-6]??0))/3);
            return home;
        }
        public static string NpcJob(KingdomState s,int index,double clock)
        {
            if(s.phase==KingdomPhase.Feast)return "Celebrate";
            if(index==0)return "Guide";
            if(index==1)return s.phase==KingdomPhase.Supplies && clock-s.started>=3 && clock-s.started<6?"Carry fruit":"Picnic helper";
            if(index>=2 && index<=4)return (s.boards&(1<<(index-2)))==0?"Bridge helper":clock-(s.boardAt?[index-2]??0)<2?"Carry plank":"Build bridge";
            if(index==5)return s.distractedUntil>clock?"Chase ball":"Guard wand";
            return Frozen(s,index)?"Frozen friend":"Rescued friend";
        }
    }
    public sealed partial class SoloWorld
    {
        public KingdomState ReadKingdom()=>state.kingdom?.Copy();
        public static SoloWorld WithKingdom(SoloWorld world)
        {
            world=WithPond(world);if(world.Schema>=KingdomAdventure.Schema && world.state.kingdom!=null)return world;
            var s=world.Snapshot();s.kingdom=new KingdomState{members=s.players.Select((p,i)=>new KingdomMember{actor=p.id,role=i}).ToArray()};
            s.schema=Math.Max(s.schema,KingdomAdventure.Schema);s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeKingdom(SoloSnapshot s)
        {if(s!=null && s.schema<WorldLayout.Schema && s.kingdom!=null && s.kingdom.round==0 && s.kingdom.clock==0 && (s.kingdom.members==null || s.kingdom.members.Length==0))s.kingdom=null;}
        private static void ValidateKingdom(SoloSnapshot s)
        {
            // Unity JSON rounds double timers; allow one millisecond at the
            // eight-second boundary so a valid toss remains saveable.
            var g=s.kingdom;if(g==null && s.schema<WorldLayout.Schema)return;if(s.schema<KingdomAdventure.Schema){if(g!=null || s.players.Any(p=>p.zone==KingdomAdventure.Zone))throw new InvalidOperationException("Kingdom requires schema 38.");return;}
            if(g==null || !Enum.IsDefined(typeof(KingdomPhase),g.phase) || g.round<0 || g.round==int.MaxValue || !KeepyRules.Finite(g.clock) || g.clock<0 || !KeepyRules.Finite(g.started) || g.started<0 || g.started>g.clock || !KeepyRules.Finite(g.distractedUntil) || g.distractedUntil<0 || g.distractedUntil>g.clock+8.001 ||
                g.supplies<0 || g.supplies>7 || g.boards<0 || g.boards>7 || g.rescued<0 || g.rescued>7 || g.members==null || !g.members.Select(m=>m?.actor).OrderBy(v=>v).SequenceEqual(s.players.Select(p=>p.id).OrderBy(v=>v)))throw new InvalidOperationException("Invalid kingdom checkpoint.");
            foreach(var m in g.members)if(m.role<0 || m.role>3 || m.attending && (s.players.Single(p=>p.id==m.actor).zone!=KingdomAdventure.Zone || g.phase==KingdomPhase.Ready) || !string.IsNullOrEmpty(m.carrying) && (!m.attending || m.carrying!="wand" && !new[]{"fruit-0","fruit-1","fruit-2"}.Contains(m.carrying)))throw new InvalidOperationException("Invalid kingdom participant.");
            if(g.members.Where(m=>!string.IsNullOrEmpty(m.carrying)).GroupBy(m=>m.carrying).Any(group=>group.Count()>1))throw new InvalidOperationException("Duplicate adventure hold.");
            foreach(var times in new[]{g.boardAt,g.wakeAt})if(s.schema>=43 && (times==null || times.Length!=3) || times!=null && times.Length>0 && (times.Length!=3 || times.Any(t=>!KeepyRules.Finite(t) || t<0 || t>g.clock+.001)))throw new InvalidOperationException("Invalid adventure job time.");
            if(g.phase>=KingdomPhase.Bridge && g.supplies!=7 || g.phase>=KingdomPhase.Queen && g.boards!=7 || g.phase==KingdomPhase.Feast && g.rescued!=7 || g.phase==KingdomPhase.Ready && (g.round!=0 || g.supplies!=0 || g.boards!=0 || g.rescued!=0))throw new InvalidOperationException("Invalid kingdom progression.");
        }
        public bool ReleaseKingdom(string actor)
        {var m=state.kingdom?.members.FirstOrDefault(v=>v.actor==actor);if(m==null || !m.attending)return false;m.attending=false;m.carrying="";state.revision++;return true;}
        public bool JoinKingdomGroup(string[] connected)
        {
            var g=state.kingdom;if(g==null || g.phase==KingdomPhase.Ready || !g.members.Any(m=>m.attending && connected.Contains(m.actor)) && !state.players.Any(p=>p.zone==KingdomAdventure.Zone && connected.Contains(p.id)))return false;var changed=false;
            foreach(var id in connected){var p=state.players.Single(v=>v.id==id);var m=g.members.Single(v=>v.actor==id);if(m.attending || m.declined || p.zone!="daycare" && p.zone!=KingdomAdventure.Zone)continue;
                TravelPlayer(p,KingdomAdventure.Zone);m.attending=true;p.x=420+Array.IndexOf(g.members,m)*75;p.y=150;changed=true;}
            if(changed)state.revision++;return changed;
        }
        private void KingdomPhaseTo(KingdomPhase phase){state.kingdom.phase=phase;state.kingdom.started=state.kingdom.clock;}
        private string KingdomOperation(SoloCommand c,SoloPlayer player)
        {
            var g=state.kingdom;if(g==null)return "kingdom-unavailable";var m=g.members.Single(v=>v.actor==player.id);
            if(c.value=="leave"){
                if(player.zone!=KingdomAdventure.Zone)return "wrong-area";m.declined=true;TravelPlayer(player,"daycare");return null;}
            if(c.value=="start" || c.value=="replay"){
                if(player.zone!="daycare" && player.zone!=KingdomAdventure.Zone)return "wrong-area";
                if(c.value=="replay" && (g.phase!=KingdomPhase.Feast || g.clock-g.started<6))return "story-not-finished";
                if(g.phase==KingdomPhase.Ready || c.value=="replay"){
                    if(g.round>=int.MaxValue-1)return "round-limit";if(state.schema>=DaycareNpcCasts.Schema)g.npcCast=DaycareNpcCasts.Pick(9,g.npcCast);
                    g.round++;g.supplies=g.boards=g.rescued=0;g.distractedUntil=0;g.boardAt=new double[3];g.wakeAt=new double[3];foreach(var member in g.members){member.declined=false;member.carrying="";}KingdomPhaseTo(KingdomPhase.Welcome);}
                if(player.zone!=KingdomAdventure.Zone)TravelPlayer(player,KingdomAdventure.Zone);
                m.attending=true;m.declined=false;player.x=420+Array.IndexOf(g.members,m)*75;player.y=150;return null;}
            if(player.zone!=KingdomAdventure.Zone || !m.attending)return "not-in-adventure";
            if(c.value=="begin" && g.phase==KingdomPhase.Welcome){KingdomPhaseTo(KingdomPhase.Supplies);return null;}
            if(c.value=="role"){m.role=(m.role+1)%4;return null;}
            var point=KingdomAdventure.Prop(c.target);if(Math.Abs(player.x-point.X)>130 || Math.Abs(player.y-point.Y)>140)return "walk-closer";
            if(c.value=="fruit" && g.phase==KingdomPhase.Supplies && c.target.StartsWith("fruit-") && int.TryParse(c.target.Substring(6),out var fruit) && fruit>=0 && fruit<3){if(!string.IsNullOrEmpty(m.carrying))return "bring-fruit-to-basket";if((g.supplies&(1<<fruit))!=0 || g.members.Any(member=>member.carrying==c.target))return "fruit-already-taken";m.carrying=c.target;return null;}
            if(c.value=="deliver" && c.target=="basket" && g.phase==KingdomPhase.Supplies && (m.carrying??"").StartsWith("fruit-")){g.supplies|=1<<int.Parse(m.carrying.Substring(6));m.carrying="";if(g.supplies==7)KingdomPhaseTo(KingdomPhase.Bridge);return null;}
            if(c.value=="board" && g.phase==KingdomPhase.Bridge && c.target.StartsWith("board-") && int.TryParse(c.target.Substring(6),out var board) && board>=0 && board<3){if((g.boards&(1<<board))!=0)return "plank-already-placed";g.boards|=1<<board;g.boardAt[board]=g.clock;if(g.boards==7)KingdomPhaseTo(KingdomPhase.Queen);return null;}
            if(c.value=="toss" && c.target=="ball" && g.phase==KingdomPhase.Queen){g.distractedUntil=g.clock+8;return null;}
            if(c.value=="wand" && c.target=="wand" && g.phase==KingdomPhase.Queen && g.distractedUntil>g.clock){m.carrying="wand";KingdomPhaseTo(KingdomPhase.Rescue);return null;}
            if(c.value=="wake" && g.phase==KingdomPhase.Rescue && c.target.StartsWith("friend-") && int.TryParse(c.target.Substring(7),out var friend) && friend>=0 && friend<3){if((g.rescued&(1<<friend))!=0)return "friend-already-awake";g.rescued|=1<<friend;g.wakeAt[friend]=g.clock;if(g.rescued==7){foreach(var member in g.members)member.carrying="";KingdomPhaseTo(KingdomPhase.Feast);}return null;}
            return "try-current-story-step";
        }
        private bool AdvanceKingdom(double seconds,string[] activePlayers,out bool visible)
        {
            visible=false;var g=state.kingdom;if(g==null || !g.members.Any(m=>m.attending && (activePlayers==null || activePlayers.Contains(m.actor))))return false;
            var before=g.clock;g.clock+=seconds;
            if(g.phase==KingdomPhase.Welcome && g.clock-g.started>=30){KingdomPhaseTo(KingdomPhase.Supplies);visible=true;}
            else if(g.phase==KingdomPhase.Queen && before<g.distractedUntil && g.clock>=g.distractedUntil)visible=true;
            // Clock samples use the existing motion lane; only transitions
            // advance revision so shared join/action commands can settle.
            return true;
        }
    }
}
