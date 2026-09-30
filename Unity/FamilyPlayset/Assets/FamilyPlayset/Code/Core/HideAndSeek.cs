using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum HidePhase { Idle, Counting, Waiting, Walking, Inspecting, Found, Looking, Lobby }
    public enum HiderMode { Away, Preparing, Hidden, Found, Invited, Ready }
    [Serializable] public sealed class HiderState
    {
        public string actor;
        public HiderMode mode;
        public int slot=-1, cycle;
        public double preparation, idle;
        public HiderState Copy()=>(HiderState)MemberwiseClone();
    }
    [Serializable] public sealed class HideState
    {
        public int round, direction=1, cursor, target=-1, pass, visited;
        public HidePhase phase;
        public double age, count, clock, walked;
        public float x=HideAndSeek.StartX;
        public string organizer="";
        public HiderState[] hiders=Array.Empty<HiderState>();
        public HideState Copy(){var h=(HideState)MemberwiseClone();h.hiders=hiders.Select(p=>p.Copy()).ToArray();return h;}
    }
    // The connected first level has a clear foreground walking rail. Authored
    // cover points, rather than hidden player coordinates, drive search choices.
    public static class HideAndSeek
    {
        public const int Schema=28, ExpansionSchema=29, CountSchema=30, TogetherSchema=31, HidingWindowSchema=32;
        public const double CountSeconds=15, InspectSeconds=1.35, ReactionSeconds=1.25;
        public const float StartX=-3660, RailY=50, Speed=420, GlanceDistance=1100;
        // Preserve the six original slot IDs for schema-28 saves.
        public static readonly float[] SlotX={-4900,-4050,-3870,-3560,-3430,-3150,-7040,-470,2910,4310};
        public static readonly float[] GroundY={245,200,200,245,245,245,120,130,120,180};
        public static readonly string[] Names={"Curtain","Sofa left","Sofa right","Wardrobe left","Wardrobe right","Tent","Folding screen","Dining table","Blanket bench","Garden bush"};
        public static readonly int[] Props={0,1,1,2,2,3,4,5,6,7};
        public const int AllChecked=(1<<10)-1;
        public static float HiddenY(int slot)=>slot<6?320:slot==7?150:GroundY[slot]+65;
        public static HiderState Player(HideState s,string actor)=>s?.hiders.FirstOrDefault(p=>p.actor==actor);
        public static bool Hidden(HideState s,string actor)=>Player(s,actor)?.mode==HiderMode.Hidden;
        public static bool Zone(SoloPlayer p)=>p.zone=="garden" && p.stairs==0;
        public static bool Eligible(HiderState p)=>p.mode==HiderMode.Hidden && p.preparation<=0;
        public static bool Complete(HideState s)=>s.hiders.All(p=>p.mode==HiderMode.Away || p.mode==HiderMode.Found);
        public static bool NextRound(HideState s)=>s.phase==HidePhase.Idle || s.phase!=HidePhase.Counting && Complete(s);
        public static bool Playing(HiderState h)=>h!=null && (h.mode==HiderMode.Preparing || h.mode==HiderMode.Hidden || h.mode==HiderMode.Found);
        public static string Parent(HideState s,bool preview=false)=>((s.round+(preview && NextRound(s)?1:0))%2==0 && s.round+(preview && NextRound(s)?1:0)>0)?"Chilli":"Bandit";
        public static double LookSeconds(HideState s)=>.9+.15*(((long)s.round+s.pass+(s.target+1))%3);
        public static int Facing(HideState s)=>s.phase==HidePhase.Looking && s.age<LookSeconds(s)*.5?-s.direction:s.direction;
        // The small repeatable variation changes nearby choices each round.
        // No occupancy, identity or hidden coordinates are an input.
        public static int NextSlot(HideState s)
        {
            var best=-1;var score=double.MaxValue;
            for(var i=0;i<SlotX.Length;i++)
            {
                if((s.visited & (1<<i))!=0)continue;
                var variation=((long)s.round*73+(long)s.pass*47+i*109)%61;
                var remaining=Enumerable.Range(0,SlotX.Length).Where(j=>j!=i && (s.visited & (1<<j))==0).Select(j=>SlotX[j]).ToArray();
                var travel=Math.Abs(SlotX[i]-s.x);
                // On this one-dimensional rail, the remaining extreme covers
                // give a cheap tour estimate that avoids crossing Home twice.
                var rest=remaining.Length==0?0:remaining.Max()-remaining.Min()+Math.Min(Math.Abs(SlotX[i]-remaining.Min()),Math.Abs(SlotX[i]-remaining.Max()));
                var value=travel*1.35+rest+variation;
                if(value<score){score=value;best=i;}
            }
            return best;
        }
        public static bool Finite(double n)=>!double.IsNaN(n) && !double.IsInfinity(n);
        public static void Validate(HideState h,string[] actors,int schema=HidingWindowSchema)
        {
            var expanded=schema>=ExpansionSchema;var slots=expanded?SlotX.Length:6;var maxCount=schema>=CountSchema?CountSeconds:expanded?10:20;
            if(h==null || h.hiders==null || h.hiders.Length!=actors.Length || !h.hiders.Select(p=>p?.actor).OrderBy(x=>x).SequenceEqual(actors.OrderBy(x=>x)) ||
                h.round<0 || h.direction!=1 && h.direction!=-1 || h.cursor<0 || h.cursor>=slots || h.target< -1 || h.target>=slots || h.pass<0 || h.visited<0 || h.visited>AllChecked || !Finite(h.walked) || h.walked<0 || h.walked>GlanceDistance+1 ||
                !Enum.IsDefined(typeof(HidePhase),h.phase) || !Finite(h.age) || h.age<0 || h.age>300 || !Finite(h.count) || h.count<0 || h.count>maxCount ||
                !Finite(h.clock) || h.clock<0 || h.clock>86400 || !Finite(h.x) || h.x<(expanded?Discovery.MinX:-4900) || h.x>(expanded?4800:-3150) || !expanded && (h.phase==HidePhase.Looking || h.visited!=0 || h.walked!=0))throw new InvalidOperationException("Invalid hide-and-seek state.");
            foreach(var p in h.hiders)
                if(!Enum.IsDefined(typeof(HiderMode),p.mode) || p.cycle<0 || p.slot< -1 || p.slot>=slots || (p.mode==HiderMode.Hidden)!=(p.slot>=0) ||
                    !Finite(p.preparation) || p.preparation<0 || p.preparation>maxCount || !Finite(p.idle) || p.idle<0 || p.idle>300)
                    throw new InvalidOperationException("Invalid hider state.");
            if(h.hiders.Where(p=>p.slot>=0).GroupBy(p=>p.slot).Any(g=>g.Count()>1))throw new InvalidOperationException("Hide space occupied twice.");
            if((h.phase==HidePhase.Walking || h.phase==HidePhase.Inspecting) && h.target<0)throw new InvalidOperationException("Missing inspection target.");
            if(schema>=TogetherSchema && (h.hiders.Any(p=>p.preparation!=0) ||
                h.phase==HidePhase.Lobby && (h.count!=0 || !h.hiders.Any(p=>p.actor==h.organizer && p.mode==HiderMode.Ready) || h.hiders.Any(Playing)) ||
                h.phase!=HidePhase.Lobby && (!string.IsNullOrEmpty(h.organizer) || h.hiders.Any(p=>p.mode==HiderMode.Ready || p.mode==HiderMode.Invited))))
                throw new InvalidOperationException("Invalid shared hiding invitation.");
        }
    }
    public sealed partial class SoloWorld
    {
        public HideState ReadHideAndSeek()=>state.hideAndSeek?.Copy();
        public static SoloWorld WithHideAndSeek(SoloWorld world)
        {
            world=WithMarbleRamps(world);if(world.Schema>=HideAndSeek.HidingWindowSchema)return world;
            var s=world.Snapshot();
            if(s.schema<HideAndSeek.Schema)s.hideAndSeek=new HideState{hiders=s.players.Select(p=>new HiderState{actor=p.id}).ToArray()};
            else SuspendRestoredHide(s);
            s.hideAndSeek.visited=0;s.hideAndSeek.walked=0;s.hideAndSeek.organizer="";s.schema=HideAndSeek.HidingWindowSchema;s.revision++;
            Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeHideInline(SoloSnapshot s)
        {
            var h=s?.hideAndSeek;
            if(s!=null && s.schema<HideAndSeek.Schema && h!=null && h.round==0 && h.phase==HidePhase.Idle && h.count==0 && h.age==0 && h.clock==0 && (h.hiders==null || h.hiders.Length==0))s.hideAndSeek=null;
        }
        private static void ValidateHideAndSeek(SoloSnapshot s)
        {
            if(s.schema<HideAndSeek.Schema){if(s.hideAndSeek!=null)throw new InvalidOperationException("Hiding requires schema 28.");return;}
            HideAndSeek.Validate(s.hideAndSeek,s.players.Select(p=>p.id).ToArray(),s.schema);
            foreach(var h in s.hideAndSeek.hiders.Where(p=>p.mode==HiderMode.Hidden))
            {var p=s.players.Single(v=>v.id==h.actor);if(!HideAndSeek.Zone(p) || p.fixture!="" || p.x!=HideAndSeek.SlotX[h.slot] || p.y!=(s.schema>=HideAndSeek.ExpansionSchema?HideAndSeek.HiddenY(h.slot):320))throw new InvalidOperationException("Hidden occupant is outside cover.");}
        }
        private void ExitHide(SoloPlayer player,HiderState h,bool placeExit)
        {
            if(h.slot>=0 && placeExit){player.x=HideAndSeek.SlotX[h.slot];player.y=HideAndSeek.RailY;
                foreach(var t in state.toys.Where(t=>t.holder==player.id)){t.x=player.x;t.y=player.y;}}
            h.slot=-1;h.mode=state.hideAndSeek.phase==HidePhase.Counting?HiderMode.Preparing:HiderMode.Away;h.idle=0;
        }
        public bool ReleaseHideAndSeek(string actor)
        {
            var h=HideAndSeek.Player(state.hideAndSeek,actor);if(h==null || h.mode==HiderMode.Away)return false;
            ExitHide(state.players.Single(p=>p.id==actor),h,true);h.mode=HiderMode.Away;h.preparation=0;FinishEmptyHide();state.revision++;return true;
        }
        private void FinishEmptyHide()
        {
            var s=state.hideAndSeek;if(s==null)return;
            // The hiding window stays open even when nobody has joined yet.
            if(s.phase==HidePhase.Counting)return;
            if(s.phase==HidePhase.Lobby)
            {
                var ready=s.hiders.Where(p=>p.mode==HiderMode.Ready).ToArray();
                if(ready.Length>0){if(!ready.Any(p=>p.actor==s.organizer))s.organizer=ready[0].actor;return;}
                foreach(var h in s.hiders)h.mode=HiderMode.Away;
            }
            if(!(s.phase==HidePhase.Found && s.age<HideAndSeek.ReactionSeconds) && HideAndSeek.Complete(s))
            {s.phase=HidePhase.Idle;s.age=0;s.count=0;s.target=-1;s.organizer="";}
        }
        private string JoinHideCount(SoloPlayer player,HiderState h)
        {
            if(!HideAndSeek.Zone(player))
            {
                if(player.visit>=long.MaxValue-1)return "visit-limit";
                // Choosing Go hide is an explicit trip downstairs. Reuse
                // ordinary travel's item-return rules; never move other players.
                TravelPlayer(player,"home");
            }
            if(h.mode!=HiderMode.Hidden)h.mode=HiderMode.Preparing;
            h.preparation=0;h.idle=0;return null;
        }
        private string HideOperation(SoloCommand c,SoloPlayer player)
        {
            var s=state.hideAndSeek;if(s==null)return "hiding-upgrade-needed";var h=HideAndSeek.Player(s,player.id);
            if(c.value=="leave"){ExitHide(player,h,true);h.mode=HiderMode.Away;h.preparation=0;FinishEmptyHide();return null;}
            if(c.value=="invite" || c.value=="start")
            {
                if(!HideAndSeek.NextRound(s))return "round-in-progress";
                if(!HideAndSeek.Zone(player))return "come-to-first-level";
                if(s.round==int.MaxValue)return "round-limit";
                s.round++;s.direction=s.round%2==0?-1:1;s.cursor=0;s.pass=0;s.visited=0;s.walked=0;s.target=-1;
                s.x=Math.Max(Discovery.MinX+40,Math.Min(4760,player.x+220));s.count=HideAndSeek.CountSeconds;s.age=0;s.clock=0;s.phase=HidePhase.Counting;s.organizer="";
                foreach(var peer in s.hiders){peer.mode=HiderMode.Away;peer.slot=-1;peer.preparation=0;peer.idle=0;}
                return null;
            }
            if(c.value=="join")
            {
                if(s.phase!=HidePhase.Counting || c.target!=s.round.ToString())return "invitation-ended";
                return JoinHideCount(player,h);
            }
            if(!HideAndSeek.Zone(player))return "come-to-first-level";
            if(c.value=="out"){ExitHide(player,h,true);FinishEmptyHide();return null;}
            if(s.phase!=HidePhase.Counting)return "hiding-time-ended";
            if(c.value!="hide" || !int.TryParse(c.target,out var slot) || slot<0 || slot>=HideAndSeek.SlotX.Length)return "invalid-hide-space";
            if(s.hiders.Any(p=>p.actor!=player.id && p.slot==slot))return "hide-space-busy";
            if(Math.Abs(player.x-HideAndSeek.SlotX[slot])>170)return "walk-to-hide-space";
            if(h.mode!=HiderMode.Hidden && h.cycle==int.MaxValue)return "round-limit";
            ClearFixture(player);player.activity="";player.x=HideAndSeek.SlotX[slot];player.y=HideAndSeek.HiddenY(slot);
            if(h.mode!=HiderMode.Hidden)h.cycle++;
            h.slot=slot;h.mode=HiderMode.Hidden;h.idle=0;
            foreach(var t in state.toys.Where(t=>t.holder==player.id)){t.x=player.x;t.y=player.y;}
            return null;
        }
        private void AfterHideAction(SoloCommand c,SoloPlayer player)
        {
            var h=HideAndSeek.Player(state.hideAndSeek,player.id);if(h==null || h.mode==HiderMode.Away)return;
            h.idle=0;
            if(h.mode==HiderMode.Invited)return;
            if(!HideAndSeek.Zone(player)){ExitHide(player,h,false);h.mode=HiderMode.Away;h.preparation=0;FinishEmptyHide();}
            else if(c.action!=SoloAction.HideAndSeek && c.action!=SoloAction.ChangeAvatar && h.mode==HiderMode.Hidden)
            {ExitHide(player,h,c.action!=SoloAction.Move);FinishEmptyHide();}
        }
        private bool AdvanceHideAndSeek(double seconds,string[] active,out bool visible)
        {
            visible=false;var changed=false;
            while(seconds>1e-9){var step=Math.Min(.05,seconds);changed|=StepHideAndSeek(step,active,out var v);visible|=v;seconds-=step;}
            return changed;
        }
        private bool StepHideAndSeek(double seconds,string[] active,out bool visible)
        {
            visible=false;var s=state.hideAndSeek;if(s==null)return false;
            var changed=false;
            foreach(var h in s.hiders.Where(p=>p.mode!=HiderMode.Away))
            {
                var player=state.players.Single(p=>p.id==h.actor);
                if(h.mode!=HiderMode.Invited && !HideAndSeek.Zone(player) || active!=null && !active.Contains(h.actor)){ExitHide(player,h,player.zone=="garden");h.mode=HiderMode.Away;h.preparation=0;visible=true;}
                else if(h.mode!=HiderMode.Hidden){h.idle=Math.Min(300,h.idle+seconds);if(h.idle>=300){h.mode=HiderMode.Away;visible=true;}}
                changed=true;
            }
            if(s.phase==HidePhase.Idle)return changed;
            s.clock=Math.Min(86400,s.clock+seconds);s.age=Math.Min(300,s.age+seconds);changed=true;
            var oldPhase=s.phase;var oldOrganizer=s.organizer;FinishEmptyHide();visible|=oldOrganizer!=s.organizer;
            if(s.phase==HidePhase.Idle){visible|=oldPhase!=s.phase;return true;}
            if(s.phase==HidePhase.Counting){s.count=Math.Max(0,s.count-seconds);if(s.count==0){
                // Only players actually inside cover at the deadline are sought.
                foreach(var h in s.hiders.Where(p=>p.mode!=HiderMode.Hidden)){h.mode=HiderMode.Away;h.preparation=0;}
                s.phase=HidePhase.Looking;s.age=0;FinishEmptyHide();}}
            else if(s.phase==HidePhase.Waiting)
            {
                if(s.hiders.Any(HideAndSeek.Eligible))
                {
                    if(s.visited==HideAndSeek.AllChecked){s.visited=0;if(s.pass<int.MaxValue)s.pass++;}
                    s.target=HideAndSeek.NextSlot(s);s.direction=HideAndSeek.SlotX[s.target]<s.x?-1:1;s.walked=0;s.phase=HidePhase.Walking;s.age=0;
                }
            }
            else if(s.phase==HidePhase.Looking && s.age>=HideAndSeek.LookSeconds(s))
            {s.phase=s.target>=0?HidePhase.Walking:HidePhase.Waiting;s.age=0;}
            else if(s.phase==HidePhase.Walking)
            {
                var goal=HideAndSeek.SlotX[s.target];var delta=goal-s.x;var step=(float)(HideAndSeek.Speed*seconds);
                var before=s.x;s.x=Math.Abs(delta)<=step?goal:s.x+Math.Sign(delta)*step;s.walked+=Math.Abs(s.x-before);
                if(s.x==goal){s.phase=HidePhase.Inspecting;s.age=0;s.walked=0;}
                else if(s.walked>=HideAndSeek.GlanceDistance){s.phase=HidePhase.Looking;s.age=0;s.walked=0;}
            }
            else if(s.phase==HidePhase.Inspecting && s.age>=HideAndSeek.InspectSeconds)
            {
                s.visited|=1<<s.target;s.cursor=(s.cursor+1)%HideAndSeek.SlotX.Length;
                var found=s.hiders.FirstOrDefault(p=>p.slot==s.target && HideAndSeek.Eligible(p));
                if(found!=null){ExitHide(state.players.Single(p=>p.id==found.actor),found,true);found.mode=HiderMode.Found;found.preparation=0;s.phase=HidePhase.Found;s.age=0;}
                else NextHideInspection();
            }
            else if(s.phase==HidePhase.Found && s.age>=HideAndSeek.ReactionSeconds)NextHideInspection();
            visible|=s.phase!=oldPhase;return changed;
        }
        private void NextHideInspection()
        {
            var s=state.hideAndSeek;s.phase=HidePhase.Looking;s.age=0;s.target=-1;s.walked=0;
        }
        private static void SuspendRestoredHide(SoloSnapshot s)
        {
            if(s.hideAndSeek==null)return;
            var changed=s.hideAndSeek.hiders.Any(h=>h.mode!=HiderMode.Away);
            foreach(var h in s.hideAndSeek.hiders){if(h.slot>=0){var p=s.players.Single(v=>v.id==h.actor);p.x=HideAndSeek.SlotX[h.slot];p.y=HideAndSeek.RailY;foreach(var t in s.toys.Where(t=>t.holder==p.id)){t.x=p.x;t.y=p.y;}}h.slot=-1;h.mode=HiderMode.Away;h.preparation=0;h.idle=0;}
            s.hideAndSeek.phase=HidePhase.Idle;s.hideAndSeek.organizer="";s.hideAndSeek.target=-1;s.hideAndSeek.age=0;s.hideAndSeek.count=0;if(changed)s.revision++;
        }
    }
}
