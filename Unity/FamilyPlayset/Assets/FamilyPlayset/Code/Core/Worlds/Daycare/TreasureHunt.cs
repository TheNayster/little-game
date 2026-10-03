using System;
using System.Linq;
namespace LittleWeeps.Core
{
    [Serializable] public sealed class TreasureMember
    { public string actor; public bool attending,declined; public TreasureMember Copy()=>(TreasureMember)MemberwiseClone(); }
    [Serializable] public sealed class TreasureState
    {
        public int round,phase,shell,pot,site,tuneStep,lastFind; public double clock,started,demoAt,findAt;
        public bool[] lifted; public int[] tune,wheels; public string[] friends; public TreasureMember[] members;
        public TreasureState Copy()=>new TreasureState{round=round,phase=phase,shell=shell,pot=pot,site=site,tuneStep=tuneStep,lastFind=lastFind,clock=clock,started=started,demoAt=demoAt,findAt=findAt,lifted=lifted.ToArray(),tune=tune.ToArray(),wheels=wheels.ToArray(),friends=friends.ToArray(),members=members.Select(m=>m.Copy()).ToArray()};
    }
    public static class TreasureHunt
    {
        public const int Schema=47; public const string Zone="daycare-treasure";
        public static string Symbol(int i)=>new[]{"Shell","Leaf","Star"}[i];
        public static string Landmark(int i)=>new[]{"Palm cove","Flower grove","Windchime lookout"}[i];
        public static WalkPoint Cover(int i)=>i<3?new WalkPoint(900+i*190,320):new WalkPoint(2430+(i-3)*190,320);
        public static WalkPoint Bell=>new WalkPoint(3900,360);
        public static WalkPoint Dig(int i)=>new WalkPoint(new[]{1080,2640,4270}[i],160);
        public static WalkPoint Point(TreasureState s,string target)
        {if(target.StartsWith("cover-") && int.TryParse(target.Substring(6),out var c) && c>=0 && c<6)return Cover(c);if(target.StartsWith("dig-") && int.TryParse(target.Substring(4),out var d) && d>=0 && d<3)return Dig(d);return target=="chest"?Dig(s.site):Bell;}
        public static string Next(TreasureState s)=>s.phase<=2?"cover-"+s.shell:s.phase==3?"cover-"+(3+s.pot):s.phase==4?"bell":s.phase==5?"dig-"+s.site:"chest";
        public static string PictureClue(TreasureState s)=>s.phase==1?"The wind scattered our map! Help our three friends find the pieces.":s.phase==2?"Look under the RED STRIPED shell. Lift it, then pick up the map piece.":s.phase==3?"Look under the pot with THREE PURPLE flowers. Lift it, then pick up the map piece.":s.phase==4?"Listen to our friend's windchimes. Tap the three pictures in the same order.":s.phase==5?"Our map is whole! Dig at "+Landmark(s.site)+".":s.phase==6?"Match the chest's pictures to map pieces 1, 2 and 3. Then open it!":"We found Calypso's surprise! Everyone helped. Explore our island or start a new hunt.";
        public static string MysteryClue(TreasureState s)=>s.phase==2?"I wear red stripes and hide beside two plain shells. What's beneath me?":s.phase==3?"My flowers match the tree. Count one, two, three. Which pot am I?":s.phase==4?"A breeze makes a song. Remember the order to wake the sleeping chimes.":s.phase==5?"The map points to "+Landmark(s.site)+". Find the mound and see what lies below.":PictureClue(s);
        public static WalkPoint Friend(TreasureState s,int i,double clock=-1)
        {
            if(s.phase==7){var at=Dig(s.site);return new WalkPoint(at.X-170+i*150,280);}
            if(i==2 && s.phase==4)return new WalkPoint(4110,440);
            var x=new[]{720f,2300f,4110f}[i];var y=new[]{430f,440f,440f}[i];var t=clock<0?s.clock:clock;
            return new WalkPoint(x+(float)Math.Sin(t*.65+i)*65,y+(float)Math.Sin(t*.4+i)*20);
        }
    }
    public sealed partial class GameWorld
    {
        public TreasureState ReadTreasure()=>state.treasure?.Copy();
        private static void NormalizeTreasure(SoloSnapshot s)
        {if(s!=null && s.schema<TreasureHunt.Schema && s.treasure!=null && s.treasure.round==0 && (s.treasure.members==null || s.treasure.members.Length==0))s.treasure=null;}
        private static TreasureState NewTreasure(SoloSnapshot s)=>new TreasureState{lifted=new bool[6],tune=new[]{0,1,2},wheels=new int[3],demoAt=-10,friends=DaycareNpcCasts.Pick(3),members=s.players.Select(p=>new TreasureMember{actor=p.id}).ToArray()};
        private static void ValidateTreasure(SoloSnapshot s)
        {
            var g=s.treasure;if(g==null && s.schema<TreasureHunt.Schema)return;
            if(g==null || g.round<0 || g.round>=int.MaxValue || g.phase<0 || g.phase>7 || (g.phase==0)!=(g.round==0) || g.shell<0 || g.shell>2 || g.pot<0 || g.pot>2 || g.site<0 || g.site>2 || g.tuneStep<0 || g.tuneStep>3 || g.lastFind<0 || g.lastFind>3 || !KeepyRules.Finite(g.clock) || g.clock<0 || !KeepyRules.Finite(g.started) || g.started<0 || g.started>g.clock || !KeepyRules.Finite(g.demoAt) || g.demoAt < -10 || g.demoAt>g.clock || !KeepyRules.Finite(g.findAt) || g.findAt<0 || g.findAt>g.clock || g.lifted==null || g.lifted.Length!=6 || g.tune==null || !g.tune.OrderBy(i=>i).SequenceEqual(new[]{0,1,2}) || g.wheels==null || g.wheels.Length!=3 || g.wheels.Any(i=>i<0 || i>2) || !DaycareNpcCasts.Valid(g.friends,3) || g.members==null || !g.members.Select(m=>m?.actor).OrderBy(i=>i).SequenceEqual(s.players.Select(p=>p.id).OrderBy(i=>i)))throw new InvalidOperationException("Invalid treasure hunt checkpoint.");
            if(g.phase>=3 && !g.lifted[g.shell] || g.phase>=4 && !g.lifted[3+g.pot] || g.phase>=5 && g.tuneStep!=3 || g.phase==7 && !g.wheels.SequenceEqual(g.tune))throw new InvalidOperationException("Invalid treasure discovery progress.");
            foreach(var m in g.members)if(m.attending && (g.phase==0 || s.players.Single(p=>p.id==m.actor).zone!=TreasureHunt.Zone))throw new InvalidOperationException("Invalid treasure participant.");
        }
        public bool ReleaseTreasure(string actor)
        {var m=state.treasure?.members.FirstOrDefault(v=>v.actor==actor);if(m==null || !m.attending)return false;m.attending=false;state.revision++;return true;}
        public bool JoinTreasureGroup(string[] connected)
        {
            var g=state.treasure;if(g==null || g.phase==0 || !g.members.Any(m=>m.attending && connected.Contains(m.actor)) && !state.players.Any(p=>p.zone==TreasureHunt.Zone && connected.Contains(p.id)))return false;var changed=false;
            foreach(var id in connected){var p=state.players.Single(v=>v.id==id);var m=g.members.Single(v=>v.actor==id);if(m.attending || m.declined || DaycarePlay.Member(state.hideClub,id)?.invited==true || DaycarePlay.Member(state.tagClub,id)?.invited==true || p.zone!="daycare" && p.zone!=TreasureHunt.Zone)continue;TravelPlayer(p,TreasureHunt.Zone);m.attending=true;m.declined=false;var at=TreasureHunt.Point(g,TreasureHunt.Next(g));p.x=Math.Max(120,at.X-150+Array.IndexOf(g.members,m)*55);p.y=230;DeclineOtherDaycare(p.id);changed=true;}
            if(changed)state.revision++;return changed;
        }
        private void DeclineOtherDaycare(string actor)
        {state.kingdom.members.Single(m=>m.actor==actor).declined=true;state.daycare.members.Single(m=>m.actor==actor).declined=true;state.sandpit.members.Single(m=>m.actor==actor).declined=true;if(state.vet!=null && !state.vet.members.Single(m=>m.actor==actor).attending)state.vet.members.Single(m=>m.actor==actor).declined=true;if(state.treasure!=null && !state.treasure.members.Single(m=>m.actor==actor).attending)state.treasure.members.Single(m=>m.actor==actor).declined=true;}
        private void TreasurePhase(int phase){state.treasure.phase=phase;state.treasure.started=state.treasure.clock;if(phase==4)state.treasure.demoAt=state.treasure.clock;}
        private string TreasureOperation(SoloCommand c,SoloPlayer p)
        {
            var g=state.treasure;if(g==null)return "hunt-unavailable";var own=g.members.Single(m=>m.actor==p.id);
            if(c.value=="leave"){if(p.zone!=TreasureHunt.Zone)return "wrong-area";TravelPlayer(p,"daycare");own.attending=false;own.declined=true;DeclineOtherDaycare(p.id);return null;}
            if(c.value=="start" || c.value=="replay"){
                if(p.zone!="daycare" && p.zone!=TreasureHunt.Zone)return "come-to-daycare";
                if(c.value=="replay" && g.phase!=7)return "finish-our-hunt-first";
                if(g.phase==0 || c.value=="replay"){
                    if(g.round>=int.MaxValue-1)return "round-limit";var random=new Random(Guid.NewGuid().GetHashCode());g.round++;g.shell=random.Next(3);g.pot=random.Next(3);g.site=random.Next(3);g.tune=new[]{0,1,2}.OrderBy(i=>random.Next()).ToArray();g.wheels=new int[3];g.lifted=new bool[6];g.tuneStep=g.lastFind=0;g.demoAt=-10;g.friends=DaycareNpcCasts.Pick(3,g.friends);foreach(var m in g.members)m.declined=false;TreasurePhase(1);
                }
                if(p.zone!=TreasureHunt.Zone)TravelPlayer(p,TreasureHunt.Zone);own.attending=true;own.declined=false;DeclineOtherDaycare(p.id);var at=TreasureHunt.Point(g,TreasureHunt.Next(g));p.x=Math.Max(120,at.X-170);p.y=200;return null;
            }
            if(p.zone!=TreasureHunt.Zone || !own.attending)return "not-in-hunt";
            if(c.value=="begin" && g.phase==1){TreasurePhase(2);return null;}
            if(c.value=="listen" && g.phase==4){g.demoAt=g.clock;return null;}
            var point=c.value=="wheel"?TreasureHunt.Dig(g.site):TreasureHunt.Point(g,c.target);if(Math.Abs(p.x-point.X)>135 || Math.Abs(p.y-point.Y)>140)return "walk-closer";
            if(c.target.StartsWith("cover-") && int.TryParse(c.target.Substring(6),out var cover) && cover>=0 && cover<6 && (g.phase==2 && cover<3 || g.phase==3 && cover>=3)){
                var correct=cover==(g.phase==2?g.shell:3+g.pot);
                if(c.value=="lift"){g.lifted[cover]=true;g.lastFind=correct?0:cover<3?1:2;g.findAt=g.clock;return null;}
                if(c.value=="take" && correct && g.lifted[cover]){TreasurePhase(g.phase+1);g.lastFind=0;return null;}
                return "lift-and-look";
            }
            if(c.value=="note" && g.phase==4 && int.TryParse(c.target,out var note) && note>=0 && note<3){if(note==g.tune[g.tuneStep]){g.tuneStep++;if(g.tuneStep==3)TreasurePhase(5);}else{g.tuneStep=0;g.demoAt=g.clock;g.lastFind=3;g.findAt=g.clock;}return null;}
            if(c.value=="dig" && g.phase==5 && c.target.StartsWith("dig-") && int.TryParse(c.target.Substring(4),out var site) && site>=0 && site<3){if(site==g.site){TreasurePhase(6);g.lastFind=0;}else{g.lastFind=site%2==0?1:2;g.findAt=g.clock;}return null;}
            if(c.value=="wheel" && g.phase==6 && int.TryParse(c.target,out var wheel) && wheel>=0 && wheel<3){g.wheels[wheel]=(g.wheels[wheel]+1)%3;return null;}
            if(c.value=="open" && g.phase==6 && c.target=="chest"){if(!g.wheels.SequenceEqual(g.tune))return "match-map-pictures";TreasurePhase(7);return null;}
            return "follow-current-clue";
        }
        private bool AdvanceTreasure(double seconds,string[] active,out bool visible)
        {visible=false;var g=state.treasure;if(g==null || !g.members.Any(m=>m.attending && (active==null || active.Contains(m.actor))))return false;g.clock+=seconds;return true;}
    }
}
