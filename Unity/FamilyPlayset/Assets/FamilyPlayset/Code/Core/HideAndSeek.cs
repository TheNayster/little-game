using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum HidePhase { Idle, Counting, Waiting, Walking, Inspecting, Found }
    public enum HiderMode { Away, Preparing, Hidden, Found }
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
        public int round, direction=1, cursor, target=-1, pass;
        public HidePhase phase;
        public double age, count, clock;
        public float x=HideAndSeek.StartX;
        public HiderState[] hiders=Array.Empty<HiderState>();
        public HideState Copy(){var h=(HideState)MemberwiseClone();h.hiders=hiders.Select(p=>p.Copy()).ToArray();return h;}
    }
    // Fixed covers and a clear foreground rail form the compact downstairs
    // navigation graph. Loose toys and other players never obstruct safe exits.
    public static class HideAndSeek
    {
        public const int Schema=28;
        public const double CountSeconds=20, InspectSeconds=1.5, ReactionSeconds=1.25;
        public const float StartX=-3660, RailY=50, Speed=280;
        public static readonly float[] SlotX={-4900,-4050,-3870,-3560,-3430,-3150};
        public static readonly string[] Names={"Curtain","Sofa left","Sofa right","Wardrobe left","Wardrobe right","Tent"};
        public static readonly int[] Props={0,1,1,2,2,3};
        public static HiderState Player(HideState s,string actor)=>s?.hiders.FirstOrDefault(p=>p.actor==actor);
        public static bool Hidden(HideState s,string actor)=>Player(s,actor)?.mode==HiderMode.Hidden;
        public static bool Zone(SoloPlayer p)=>p.zone=="garden" && p.stairs==0 && p.x>=-5090 && p.x<=-2990;
        public static bool Eligible(HiderState p)=>p.mode==HiderMode.Hidden && p.preparation<=0;
        // Only round/pass/direction enter route selection. Occupancy is read
        // exclusively after arriving and completing a visible inspection.
        public static int NextSlot(HideState s)=>s.direction>0?s.cursor:5-s.cursor;
        public static bool Finite(double n)=>!double.IsNaN(n) && !double.IsInfinity(n);
        public static void Validate(HideState h,string[] actors)
        {
            if(h==null || h.hiders==null || h.hiders.Length!=actors.Length || !h.hiders.Select(p=>p?.actor).OrderBy(x=>x).SequenceEqual(actors.OrderBy(x=>x)) ||
                h.round<0 || h.direction!=1 && h.direction!=-1 || h.cursor<0 || h.cursor>5 || h.target< -1 || h.target>5 || h.pass<0 ||
                !Enum.IsDefined(typeof(HidePhase),h.phase) || !Finite(h.age) || h.age<0 || h.age>300 || !Finite(h.count) || h.count<0 || h.count>CountSeconds ||
                !Finite(h.clock) || h.clock<0 || h.clock>86400 || !Finite(h.x) || h.x<SlotX[0] || h.x>SlotX[5])throw new InvalidOperationException("Invalid hide-and-seek state.");
            foreach(var p in h.hiders)
                if(!Enum.IsDefined(typeof(HiderMode),p.mode) || p.cycle<0 || p.slot< -1 || p.slot>5 || (p.mode==HiderMode.Hidden)!=(p.slot>=0) ||
                    !Finite(p.preparation) || p.preparation<0 || p.preparation>CountSeconds || !Finite(p.idle) || p.idle<0 || p.idle>300)
                    throw new InvalidOperationException("Invalid hider state.");
            if(h.hiders.Where(p=>p.slot>=0).GroupBy(p=>p.slot).Any(g=>g.Count()>1))throw new InvalidOperationException("Hide space occupied twice.");
            if((h.phase==HidePhase.Walking || h.phase==HidePhase.Inspecting) && h.target<0)throw new InvalidOperationException("Missing inspection target.");
        }
    }
    public sealed partial class SoloWorld
    {
        public HideState ReadHideAndSeek()=>state.hideAndSeek?.Copy();
        public static SoloWorld WithHideAndSeek(SoloWorld world)
        {
            world=WithMarbleRamps(world);if(world.Schema>=HideAndSeek.Schema)return world;
            var s=world.Snapshot();s.schema=HideAndSeek.Schema;s.revision++;
            s.hideAndSeek=new HideState{hiders=s.players.Select(p=>new HiderState{actor=p.id}).ToArray()};
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
            HideAndSeek.Validate(s.hideAndSeek,s.players.Select(p=>p.id).ToArray());
            foreach(var h in s.hideAndSeek.hiders.Where(p=>p.mode==HiderMode.Hidden))
            {var p=s.players.Single(v=>v.id==h.actor);if(!HideAndSeek.Zone(p) || p.fixture!="" || p.x!=HideAndSeek.SlotX[h.slot] || p.y!=320)throw new InvalidOperationException("Hidden occupant is outside cover.");}
        }
        private void ExitHide(SoloPlayer player,HiderState h,bool placeExit)
        {
            if(h.slot>=0 && placeExit){player.x=HideAndSeek.SlotX[h.slot];player.y=HideAndSeek.RailY;
                foreach(var t in state.toys.Where(t=>t.holder==player.id)){t.x=player.x;t.y=player.y;}}
            h.slot=-1;h.mode=HiderMode.Preparing;h.idle=0;
        }
        public bool ReleaseHideAndSeek(string actor)
        {
            var h=HideAndSeek.Player(state.hideAndSeek,actor);if(h==null || h.mode==HiderMode.Away)return false;
            ExitHide(state.players.Single(p=>p.id==actor),h,true);h.mode=HiderMode.Away;h.preparation=0;FinishEmptyHide();state.revision++;return true;
        }
        private void FinishEmptyHide()
        {var s=state.hideAndSeek;if(s!=null && !(s.phase==HidePhase.Found && s.age<HideAndSeek.ReactionSeconds) && s.hiders.All(p=>p.mode==HiderMode.Away || p.mode==HiderMode.Found)){s.phase=HidePhase.Idle;s.age=0;s.count=0;s.target=-1;}}
        private string HideOperation(SoloCommand c,SoloPlayer player)
        {
            var s=state.hideAndSeek;if(s==null)return "hiding-upgrade-needed";var h=HideAndSeek.Player(s,player.id);
            if(c.value=="leave"){ExitHide(player,h,true);h.mode=HiderMode.Away;h.preparation=0;FinishEmptyHide();return null;}
            if(!HideAndSeek.Zone(player))return "come-to-living-room";
            if(c.value=="join")
            {
                if(h.mode==HiderMode.Preparing || h.mode==HiderMode.Hidden)return null;
                if(s.round==int.MaxValue || h.cycle==int.MaxValue)return "round-limit";
                if(s.phase==HidePhase.Idle){s.round++;s.direction=s.round%2==0?-1:1;s.cursor=0;s.pass=0;s.target=-1;s.x=HideAndSeek.StartX;s.count=HideAndSeek.CountSeconds;s.age=0;s.clock=0;s.phase=HidePhase.Counting;}
                h.cycle++;h.mode=HiderMode.Preparing;h.slot=-1;h.preparation=HideAndSeek.CountSeconds;h.idle=0;return null;
            }
            if(h.mode==HiderMode.Away || h.mode==HiderMode.Found)return "join-first";
            if(c.value=="out"){ExitHide(player,h,true);return null;}
            if(c.value!="hide" || !int.TryParse(c.target,out var slot) || slot<0 || slot>=6)return "invalid-hide-space";
            if(s.hiders.Any(p=>p.actor!=player.id && p.slot==slot))return "hide-space-busy";
            if(Math.Abs(player.x-HideAndSeek.SlotX[slot])>170)return "walk-to-hide-space";
            ClearFixture(player);player.activity="";player.x=HideAndSeek.SlotX[slot];player.y=320;
            h.slot=slot;h.mode=HiderMode.Hidden;h.idle=0;
            foreach(var t in state.toys.Where(t=>t.holder==player.id)){t.x=player.x;t.y=player.y;}
            return null;
        }
        private void AfterHideAction(SoloCommand c,SoloPlayer player)
        {
            var h=HideAndSeek.Player(state.hideAndSeek,player.id);if(h==null || h.mode==HiderMode.Away)return;
            h.idle=0;
            if(!HideAndSeek.Zone(player)){ExitHide(player,h,false);h.mode=HiderMode.Away;h.preparation=0;FinishEmptyHide();}
            else if(c.action!=SoloAction.HideAndSeek && c.action!=SoloAction.ChangeAvatar && h.mode==HiderMode.Hidden)
                ExitHide(player,h,c.action!=SoloAction.Move);
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
                if(!HideAndSeek.Zone(player) || active!=null && !active.Contains(h.actor)){ExitHide(player,h,player.zone=="garden");h.mode=HiderMode.Away;h.preparation=0;visible=true;}
                else {h.preparation=Math.Max(0,h.preparation-seconds);if(h.mode!=HiderMode.Hidden){h.idle=Math.Min(300,h.idle+seconds);if(h.idle>=300){h.mode=HiderMode.Away;visible=true;}}}
                changed=true;
            }
            if(s.phase==HidePhase.Idle)return changed;
            s.clock=Math.Min(86400,s.clock+seconds);s.age=Math.Min(300,s.age+seconds);changed=true;
            var oldPhase=s.phase;FinishEmptyHide();
            if(s.phase==HidePhase.Idle){visible|=oldPhase!=s.phase;return true;}
            if(s.phase==HidePhase.Counting){s.count=Math.Max(0,s.count-seconds);if(s.count==0){s.phase=HidePhase.Waiting;s.age=0;}}
            else if(s.phase==HidePhase.Waiting){if(s.hiders.Any(HideAndSeek.Eligible)){s.target=HideAndSeek.NextSlot(s);s.phase=HidePhase.Walking;s.age=0;}}
            else if(s.phase==HidePhase.Walking)
            {
                var goal=HideAndSeek.SlotX[s.target];var delta=goal-s.x;var step=(float)(HideAndSeek.Speed*seconds);
                s.x=Math.Abs(delta)<=step?goal:s.x+Math.Sign(delta)*step;
                if(s.x==goal){s.phase=HidePhase.Inspecting;s.age=0;}
            }
            else if(s.phase==HidePhase.Inspecting && s.age>=HideAndSeek.InspectSeconds)
            {
                var found=s.hiders.FirstOrDefault(p=>p.slot==s.target && HideAndSeek.Eligible(p));
                if(found!=null){ExitHide(state.players.Single(p=>p.id==found.actor),found,true);found.mode=HiderMode.Found;found.preparation=0;s.phase=HidePhase.Found;s.age=0;}
                else NextHideInspection();
            }
            else if(s.phase==HidePhase.Found && s.age>=HideAndSeek.ReactionSeconds)NextHideInspection();
            visible|=s.phase!=oldPhase;return changed;
        }
        private void NextHideInspection()
        {
            var s=state.hideAndSeek;s.cursor++;if(s.cursor==6){s.cursor=0;s.direction=-s.direction;s.pass=Math.Min(int.MaxValue,s.pass+1);}
            s.phase=HidePhase.Waiting;s.age=0;s.target=-1;
        }
        private static void SuspendRestoredHide(SoloSnapshot s)
        {
            if(s.hideAndSeek==null)return;
            var changed=s.hideAndSeek.hiders.Any(h=>h.mode!=HiderMode.Away);
            foreach(var h in s.hideAndSeek.hiders){if(h.slot>=0){var p=s.players.Single(v=>v.id==h.actor);p.x=HideAndSeek.SlotX[h.slot];p.y=HideAndSeek.RailY;foreach(var t in s.toys.Where(t=>t.holder==p.id)){t.x=p.x;t.y=p.y;}}h.slot=-1;h.mode=HiderMode.Away;h.preparation=0;h.idle=0;}
            s.hideAndSeek.phase=HidePhase.Idle;s.hideAndSeek.target=-1;s.hideAndSeek.age=0;s.hideAndSeek.count=0;if(changed)s.revision++;
        }
    }
}
