using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum KingdomPhase { Ready, Welcome, Supplies, Bridge, Queen, Rescue, Feast }
    [Serializable] public sealed class KingdomMember
    {
        public string actor;
        public int role;
        public bool attending;
        public KingdomMember Copy()=>(KingdomMember)MemberwiseClone();
    }
    [Serializable] public sealed class KingdomState
    {
        public int round, supplies, boards, rescued;
        public KingdomPhase phase;
        public double clock, started, distractedUntil;
        public KingdomMember[] members;
        public KingdomState Copy()=>new KingdomState{round=round,supplies=supplies,boards=boards,rescued=rescued,phase=phase,clock=clock,started=started,distractedUntil=distractedUntil,members=members.Select(m=>m.Copy()).ToArray()};
    }
    public static class KingdomAdventure
    {
        public const int Schema=38;
        public const string Zone="imagination-adventure";
        public static readonly string[] Cast={"chloe","coco","terrier-1","terrier-2","terrier-3","winton","honey","rusty","mackenzie"};
        public static readonly string[] Roles={"Explorer","Builder","Wand helper","Picnic helper"};
        public static WalkPoint Prop(string id)
        {
            switch(id){case "fruit-0":return new WalkPoint(710,180);case "fruit-1":return new WalkPoint(890,260);case "fruit-2":return new WalkPoint(1070,180);
                case "board-0":return new WalkPoint(1240,180);case "board-1":return new WalkPoint(1370,200);case "board-2":return new WalkPoint(1500,180);
                case "ball":return new WalkPoint(1840,190);case "wand":return new WalkPoint(2130,210);
                case "friend-0":return new WalkPoint(1750,330);case "friend-1":return new WalkPoint(1930,340);case "friend-2":return new WalkPoint(2300,330);
                default:return new WalkPoint(-1000,-1000);}
        }
        public static bool Frozen(KingdomState s,int index)=>index>=6 && s.phase<KingdomPhase.Rescue || index>=6 && s.phase==KingdomPhase.Rescue && (s.rescued&(1<<(index-6)))==0;
        public static WalkPoint NpcPoint(KingdomState s,int index)
        {
            var home=new[]{new WalkPoint(350,310),new WalkPoint(1120,350),new WalkPoint(1230,330),new WalkPoint(1390,340),new WalkPoint(1540,330),new WalkPoint(2110,340),Prop("friend-0"),Prop("friend-1"),Prop("friend-2")}[index];
            var age=Math.Max(0,s.clock-s.started);
            // Routes derive from the authoritative phase clock, so joining does
            // not replay an entrance and a suspended story cannot drift locally.
            if(s.phase==KingdomPhase.Feast){var t=Math.Min(1,age/5);return new WalkPoint((float)(home.X+(400+index*135-home.X)*t),(float)(home.Y+(260+index%2*45-home.Y)*t));}
            if(index==5 && s.phase==KingdomPhase.Queen && s.distractedUntil>s.clock)return new WalkPoint((float)(2110+Math.Sin(age*2)*65),340);
            if(index<6 && s.phase!=KingdomPhase.Ready)return new WalkPoint((float)(home.X+Math.Sin(s.clock*.45+index)*35),home.Y);
            return home;
        }
    }
    public sealed partial class SoloWorld
    {
        public KingdomState ReadKingdom()=>state.kingdom?.Copy();
        public static SoloWorld WithKingdom(SoloWorld world)
        {
            world=WithOutfits(world);if(world.Schema>=KingdomAdventure.Schema)return world;
            var s=world.Snapshot();s.kingdom=new KingdomState{members=s.players.Select((p,i)=>new KingdomMember{actor=p.id,role=i}).ToArray()};
            s.schema=KingdomAdventure.Schema;s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeKingdom(SoloSnapshot s)
        {if(s!=null && s.schema<KingdomAdventure.Schema && s.kingdom!=null && s.kingdom.round==0 && s.kingdom.clock==0 && (s.kingdom.members==null || s.kingdom.members.Length==0))s.kingdom=null;}
        private static void ValidateKingdom(SoloSnapshot s)
        {
            var g=s.kingdom;if(s.schema<KingdomAdventure.Schema){if(g!=null || s.players.Any(p=>p.zone==KingdomAdventure.Zone))throw new InvalidOperationException("Kingdom requires schema 38.");return;}
            if(g==null || !Enum.IsDefined(typeof(KingdomPhase),g.phase) || g.round<0 || g.round==int.MaxValue || !KeepyRules.Finite(g.clock) || g.clock<0 || !KeepyRules.Finite(g.started) || g.started<0 || g.started>g.clock || !KeepyRules.Finite(g.distractedUntil) || g.distractedUntil<0 || g.distractedUntil>g.clock+8 ||
                g.supplies<0 || g.supplies>7 || g.boards<0 || g.boards>7 || g.rescued<0 || g.rescued>7 || g.members==null || !g.members.Select(m=>m?.actor).OrderBy(v=>v).SequenceEqual(s.players.Select(p=>p.id).OrderBy(v=>v)))throw new InvalidOperationException("Invalid kingdom checkpoint.");
            foreach(var m in g.members)if(m.role<0 || m.role>3 || m.attending && (s.players.Single(p=>p.id==m.actor).zone!=KingdomAdventure.Zone || g.phase==KingdomPhase.Ready))throw new InvalidOperationException("Invalid kingdom participant.");
            if(g.phase>=KingdomPhase.Bridge && g.supplies!=7 || g.phase>=KingdomPhase.Queen && g.boards!=7 || g.phase==KingdomPhase.Feast && g.rescued!=7 || g.phase==KingdomPhase.Ready && (g.round!=0 || g.supplies!=0 || g.boards!=0 || g.rescued!=0))throw new InvalidOperationException("Invalid kingdom progression.");
        }
        public bool ReleaseKingdom(string actor)
        {var m=state.kingdom?.members.FirstOrDefault(v=>v.actor==actor);if(m==null || !m.attending)return false;m.attending=false;state.revision++;return true;}
        private void KingdomPhaseTo(KingdomPhase phase){state.kingdom.phase=phase;state.kingdom.started=state.kingdom.clock;}
        private string KingdomOperation(SoloCommand c,SoloPlayer player)
        {
            var g=state.kingdom;if(g==null)return "kingdom-unavailable";var m=g.members.Single(v=>v.actor==player.id);
            if(c.value=="leave"){
                if(player.zone!=KingdomAdventure.Zone)return "wrong-area";TravelPlayer(player,"daycare");return null;}
            if(c.value=="start" || c.value=="replay"){
                if(player.zone!="daycare" && player.zone!=KingdomAdventure.Zone)return "wrong-area";
                if(c.value=="replay" && (g.phase!=KingdomPhase.Feast || g.clock-g.started<6))return "story-not-finished";
                if(g.phase==KingdomPhase.Ready || c.value=="replay"){
                    if(g.round>=int.MaxValue-1)return "round-limit";g.round++;g.supplies=g.boards=g.rescued=0;g.distractedUntil=0;KingdomPhaseTo(KingdomPhase.Welcome);}
                if(player.zone!=KingdomAdventure.Zone)TravelPlayer(player,KingdomAdventure.Zone);
                m.attending=true;player.x=420+Array.IndexOf(g.members,m)*75;player.y=150;return null;}
            if(player.zone!=KingdomAdventure.Zone || !m.attending)return "not-in-adventure";
            if(c.value=="role"){m.role=(m.role+1)%4;return null;}
            var point=KingdomAdventure.Prop(c.target);if(Math.Abs(player.x-point.X)>130 || Math.Abs(player.y-point.Y)>140)return "walk-closer";
            if(c.value=="fruit" && g.phase==KingdomPhase.Supplies && c.target.StartsWith("fruit-") && int.TryParse(c.target.Substring(6),out var fruit) && fruit>=0 && fruit<3){g.supplies|=1<<fruit;if(g.supplies==7)KingdomPhaseTo(KingdomPhase.Bridge);return null;}
            if(c.value=="board" && g.phase==KingdomPhase.Bridge && c.target.StartsWith("board-") && int.TryParse(c.target.Substring(6),out var board) && board>=0 && board<3){g.boards|=1<<board;if(g.boards==7)KingdomPhaseTo(KingdomPhase.Queen);return null;}
            if(c.value=="toss" && c.target=="ball" && g.phase==KingdomPhase.Queen){g.distractedUntil=g.clock+8;return null;}
            if(c.value=="wand" && c.target=="wand" && g.phase==KingdomPhase.Queen && g.distractedUntil>g.clock){KingdomPhaseTo(KingdomPhase.Rescue);return null;}
            if(c.value=="wake" && g.phase==KingdomPhase.Rescue && c.target.StartsWith("friend-") && int.TryParse(c.target.Substring(7),out var friend) && friend>=0 && friend<3){g.rescued|=1<<friend;if(g.rescued==7)KingdomPhaseTo(KingdomPhase.Feast);return null;}
            return "try-current-story-step";
        }
        private bool AdvanceKingdom(double seconds,string[] activePlayers,out bool visible)
        {
            visible=false;var g=state.kingdom;if(g==null || !g.members.Any(m=>m.attending && (activePlayers==null || activePlayers.Contains(m.actor))))return false;
            g.clock+=seconds;if(g.phase==KingdomPhase.Welcome && g.clock-g.started>=3)KingdomPhaseTo(KingdomPhase.Supplies);visible=true;return true;
        }
    }
}
