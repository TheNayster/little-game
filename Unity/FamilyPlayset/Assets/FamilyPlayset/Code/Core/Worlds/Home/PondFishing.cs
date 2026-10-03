using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum PondMode { None, Fishing, Feeding }
    public enum PondCast { Ready, Waiting, Approaching, Bite, Caught }
    [Serializable] public sealed class PondRod
    {
        public string actor;
        public int slot=-1, fish=-1;
        public PondMode mode;
        public PondCast cast;
        public float x,y;
        public double due,castAt;
        public PondRod Copy()=>(PondRod)MemberwiseClone();
        public void Clear(){slot=-1;fish=-1;mode=PondMode.None;cast=PondCast.Ready;due=0;castAt=0;}
    }
    [Serializable] public sealed class PondFish
    {
        public int id;
        public uint random;
        public float fromX,fromY,x,y;
        public double started,duration=4;
        public PondFish Copy()=>(PondFish)MemberwiseClone();
    }
    [Serializable] public sealed class PondFood
    {
        public long id;
        public float x,y;
        public double until;
        public int fish=-1;
        public PondFood Copy()=>(PondFood)MemberwiseClone();
    }
    [Serializable] public sealed class PondState
    {
        public double clock;
        public uint random;
        public long nextFood;
        public PondFish[] fish;
        public PondRod[] rods;
        public PondFood[] food=Array.Empty<PondFood>();
        public PondState Copy()=>new PondState{clock=clock,random=random,nextFood=nextFood,fish=fish.Select(f=>f.Copy()).ToArray(),rods=rods.Select(r=>r.Copy()).ToArray(),food=food.Select(f=>f.Copy()).ToArray()};
    }
    public static class PondFishing
    {
        public const int Schema=36, FishCount=8, FoodLimit=8;
        public const float X=2350, BankY=220, WaterY=440, WaterLift=115, RadiusX=380, RadiusY=45;
        public static float BankX(int slot)=>X+(slot-1.5f)*170;
        public static readonly FishingHabitat Home=new FishingHabitat("garden",X,BankY,170,RadiusX,RadiusY,FishCount,120);
        public static bool Target(float x,float y)=>Home.Target(x,y);
        public static WalkPoint Point(PondFish f,double clock)
        {
            var t=(float)Math.Max(0,Math.Min(1,(clock-f.started)/f.duration));t=t*t*(3-2*t);
            return new WalkPoint(f.fromX+(f.x-f.fromX)*t,f.fromY+(f.y-f.fromY)*t);
        }
        internal static double Roll(ref uint random)
        {random^=random<<13;random^=random>>17;random^=random<<5;return random/(double)uint.MaxValue;}
        internal static void Route(PondFish f,double clock,float x,float y,double duration)
        {var p=Point(f,clock);f.fromX=p.X;f.fromY=p.Y;f.x=x;f.y=y;f.started=clock;f.duration=duration;}
        internal static void Swim(PondFish f,double clock,FishingHabitat habitat=null)
        {
            habitat=habitat??Home;var angle=Roll(ref f.random)*Math.PI*2;var radius=Math.Sqrt(Roll(ref f.random))*.78;
            Route(f,clock,(float)(Math.Cos(angle)*habitat.radiusX*radius),(float)(Math.Sin(angle)*habitat.radiusY*radius),3+Roll(ref f.random)*5);
        }
    }
    public sealed partial class GameWorld
    {
        public PondState ReadPond()=>state.pond?.Copy();
        public static GameWorld WithPond(GameWorld world)
        {
            world=WithOutfits(world);if(world.Schema>=PondFishing.Schema && world.state.pond!=null)return world;
            var s=world.Snapshot();uint seed=2166136261;
            foreach(var ch in s.worldId)seed=unchecked((seed^ch)*16777619);
            s.pond=new PondState{random=seed|1u,fish=Enumerable.Range(0,PondFishing.FishCount).Select(i=>new PondFish{id=i,random=(seed^(uint)(i+1)*2654435761u)|1u,fromX=(i-3.5f)*65,x=(i-3.5f)*65}).ToArray(),rods=s.players.Select(p=>new PondRod{actor=p.id}).ToArray()};
            foreach(var f in s.pond.fish)PondFishing.Swim(f,0);
            foreach(var h in s.hideAndSeek.hiders.Where(h=>h.slot==8)){var player=s.players.Single(p=>p.id==h.actor);player.x=HideAndSeek.CoverX(8,PondFishing.Schema);foreach(var toy in s.toys.Where(t=>t.holder==player.id))toy.x=player.x;}
            s.schema=Math.Max(s.schema,PondFishing.Schema);s.revision++;Validate(s);return new GameWorld(s);
        }
        private static void NormalizePond(SoloSnapshot s)
        {
            if(s!=null && s.schema<WorldLayout.Schema && s.pond!=null && s.pond.clock==0 && s.pond.random==0 && s.pond.nextFood==0 &&
                (s.pond.fish==null || s.pond.fish.Length==0) && (s.pond.rods==null || s.pond.rods.Length==0) && (s.pond.food==null || s.pond.food.Length==0))s.pond=null;
        }
        private static void ValidatePond(SoloSnapshot s)
        {
            ValidateFishing(s,s.pond,PondFishing.Home,PondFishing.Schema);
        }
        private static void ValidateFishing(SoloSnapshot s,PondState p,FishingHabitat habitat,int schema)
        {
            if(p==null && s.schema<WorldLayout.Schema)return;
            if(s.schema<schema){if(p!=null)throw new InvalidOperationException("Fishing habitat requires schema "+schema+".");return;}
            if(p==null || !KeepyRules.Finite(p.clock) || p.clock<0 || p.random==0 || p.nextFood<0 || p.nextFood==long.MaxValue ||
                p.fish==null || p.fish.Length!=habitat.fishCount || !p.fish.Select(f=>f?.id).SequenceEqual(Enumerable.Range(0,habitat.fishCount).Select(i=>(int?)i)) ||
                p.rods==null || !p.rods.Select(r=>r?.actor).OrderBy(id=>id).SequenceEqual(s.players.Select(v=>v.id).OrderBy(id=>id)) || p.food==null || p.food.Length>PondFishing.FoodLimit)
                throw new InvalidOperationException("Invalid pond record.");
            foreach(var f in p.fish)
                if(f.random==0 || !habitat.Target(f.fromX,f.fromY) || !habitat.Target(f.x,f.y) || !KeepyRules.Finite(f.started) || f.started<0 || f.started>p.clock || !KeepyRules.Finite(f.duration) || f.duration<.1 || f.duration>12)throw new InvalidOperationException("Invalid pond fish.");
            foreach(var r in p.rods){var player=s.players.Single(v=>v.id==r.actor);
                if(!Enum.IsDefined(typeof(PondMode),r.mode) || !Enum.IsDefined(typeof(PondCast),r.cast) || r.slot< -1 || r.slot>3 || r.fish< -1 || r.fish>=habitat.fishCount || !habitat.Target(r.x,r.y) ||
                    !KeepyRules.Finite(r.due) || r.due<0 || !KeepyRules.Finite(r.castAt) || r.castAt<0 || r.castAt>p.clock || r.mode==PondMode.None && (r.slot!=-1 || r.fish!=-1 || r.cast!=PondCast.Ready || r.due!=0) ||
                    r.mode!=PondMode.None && (r.slot<0 || player.zone!=habitat.zone) || r.mode==PondMode.Feeding && (r.cast!=PondCast.Ready || r.fish!=-1) ||
                    (r.cast==PondCast.Approaching || r.cast==PondCast.Bite || r.cast==PondCast.Caught)!=(r.fish>=0))throw new InvalidOperationException("Invalid pond rod.");}
            if(p.rods.Where(r=>r.slot>=0).GroupBy(r=>r.slot).Any(g=>g.Count()>1) || p.rods.Where(r=>r.fish>=0).GroupBy(r=>r.fish).Any(g=>g.Count()>1))throw new InvalidOperationException("Duplicate pond lease.");
            foreach(var f in p.food)
                if(f==null || f.id<=0 || f.id>p.nextFood || !habitat.Target(f.x,f.y) || !KeepyRules.Finite(f.until) || f.until<=p.clock || f.until>p.clock+12 || f.fish< -1 || f.fish>=habitat.fishCount || p.rods.Any(r=>r.fish==f.fish && f.fish>=0))throw new InvalidOperationException("Invalid pond food.");
            if(p.food.GroupBy(f=>f.id).Any(g=>g.Count()>1) || p.food.Where(f=>f.fish>=0).GroupBy(f=>f.fish).Any(g=>g.Count()>1))throw new InvalidOperationException("Duplicate pond food.");
        }
        private static void SuspendPond(SoloSnapshot s)
        {
            SuspendFishing(s,s.pond);
        }
        private static void SuspendFishing(SoloSnapshot s,PondState p)
        {
            if(p==null)return;var changed=p.rods.Any(r=>r.mode!=PondMode.None) || p.food.Length>0;
            foreach(var r in p.rods)r.Clear();p.food=Array.Empty<PondFood>();if(changed)s.revision++;
        }
        private bool CancelPond(string actor)=>CancelFishing(actor,state.pond,PondFishing.Home);
        private bool CancelFishing(string actor,PondState p,FishingHabitat habitat)
        {
            var r=p?.rods.FirstOrDefault(v=>v.actor==actor);if(r==null || r.mode==PondMode.None)return false;
            if(r.fish>=0)PondFishing.Swim(p.fish[r.fish],p.clock,habitat);r.Clear();return true;
        }
        public bool ReleasePond(string actor){if(!CancelPond(actor))return false;state.revision++;return true;}
        private static void CastFishing(PondState p,PondRod r,float x,float y)
        {r.x=x;r.y=y;r.cast=PondCast.Waiting;r.fish=-1;r.castAt=p.clock;r.due=p.clock+4+PondFishing.Roll(ref p.random)*6;}
        private static string FeedFishing(PondState p,float x,float y)
        {
            if(p.food.Length>=PondFishing.FoodLimit)return null;
            if(p.nextFood>=long.MaxValue-1)return "food-limit";
            var fish=p.fish.FirstOrDefault(f=>!p.rods.Any(v=>v.fish==f.id) && !p.food.Any(v=>v.fish==f.id));
            var portion=new PondFood{id=++p.nextFood,x=x,y=y,until=p.clock+6,fish=fish?.id??-1};
            if(fish!=null)PondFishing.Route(fish,p.clock,x,y,1.5);
            p.food=p.food.Concat(new[]{portion}).ToArray();return null;
        }
        private string PondOperation(SoloCommand c,SoloPlayer player)=>FishingOperation(c,player,state.pond,PondFishing.Home);
        private string FishingOperation(SoloCommand c,SoloPlayer player,PondState p,FishingHabitat habitat)
        {
            if(p==null)return "pond-unavailable";var r=p.rods.Single(v=>v.actor==player.id);
            if(c.value=="leave"){CancelFishing(player.id,p,habitat);return null;}
            if(c.value=="start-fishing" || c.value=="start-feeding"){
                if(player.visit>=long.MaxValue-1)return "visit-limit";
                var slot=r.slot>=0?r.slot:Enumerable.Range(0,4).FirstOrDefault(i=>!p.rods.Any(v=>v.slot==i));
                if(p.rods.Any(v=>v.actor!=player.id && v.slot==slot))return "pond-full";
                // One authority command settles held objects, changes area, assigns
                // the shared bank lease and starts play. No client-side teleport.
                ReleaseHideAndSeek(player.id);TravelPlayer(player,habitat.zone);player.x=habitat.BankX(slot);player.y=habitat.bankY;
                r.slot=slot;r.mode=c.value=="start-fishing"?PondMode.Fishing:PondMode.Feeding;
                if(r.mode==PondMode.Fishing)CastFishing(p,r,(slot-1.5f)*habitat.castSpacing,0);
                else {r.x=(slot-1.5f)*habitat.castSpacing;r.y=0;return FeedFishing(p,r.x,r.y);}
                return null;
            }
            if(r.mode==PondMode.None || player.zone!=habitat.zone)return "come-to-pond";
            if(c.value=="release"){
                if(r.cast!=PondCast.Caught)return "no-catch";
                PondFishing.Swim(p.fish[r.fish],p.clock,habitat);r.fish=-1;r.cast=PondCast.Ready;r.due=0;return null;
            }
            if(c.value=="reel"){
                if(r.mode!=PondMode.Fishing || r.cast!=PondCast.Bite)return "wait-for-bite";
                r.cast=PondCast.Caught;r.due=0;return null;
            }
            var x=c.x-habitat.x;var y=c.y-100;
            if(!habitat.Target(x,y))return "cast-into-pond";
            if(c.value=="cast"){
                if(r.mode!=PondMode.Fishing || r.cast==PondCast.Caught)return "release-first";
                if(r.fish>=0)PondFishing.Swim(p.fish[r.fish],p.clock,habitat);CastFishing(p,r,x,y);return null;
            }
            if(c.value=="feed"){
                if(r.mode!=PondMode.Feeding)return "choose-feed-fish";
                // Bounded effects still acknowledge a child's tap. Existing food
                // gets time to be eaten rather than multiplying without a limit.
                return FeedFishing(p,x,y);
            }
            return "unknown-pond-action";
        }
        private bool AdvancePond(double seconds,string[] active,out bool visible)=>AdvanceFishing(state.pond,PondFishing.Home,seconds,active,out visible);
        private bool AdvanceFishing(PondState p,FishingHabitat habitat,double seconds,string[] active,out bool visible)
        {
            visible=false;if(p==null || active!=null && !state.players.Any(v=>v.zone==habitat.zone && active.Contains(v.id)))return false;p.clock+=seconds;
            foreach(var r in p.rods.Where(v=>v.mode!=PondMode.None).ToArray()){
                var player=state.players.Single(v=>v.id==r.actor);
                if(player.zone!=habitat.zone || Math.Abs(player.x-habitat.BankX(r.slot))>95 || Math.Abs(player.y-habitat.bankY)>80 || active!=null && !active.Contains(r.actor)){visible|=CancelFishing(r.actor,p,habitat);continue;}
                if(r.cast==PondCast.Waiting && p.clock>=r.due){
                    var available=p.fish.Where(f=>!p.rods.Any(v=>v.fish==f.id) && !p.food.Any(v=>v.fish==f.id)).ToArray();
                    if(available.Length==0)available=p.fish.Where(f=>!p.rods.Any(v=>v.fish==f.id)).ToArray();
                    if(available.Length==0){r.due=p.clock+1;continue;}
                    var fish=available[Math.Min(available.Length-1,(int)(PondFishing.Roll(ref p.random)*available.Length))];
                    p.food=p.food.Where(f=>f.fish!=fish.id).ToArray();
                    r.fish=fish.id;r.cast=PondCast.Approaching;r.due=p.clock+1.1;PondFishing.Route(fish,p.clock,r.x,r.y,1.1);visible=true;
                }
                else if(r.cast==PondCast.Approaching && p.clock>=r.due){r.cast=PondCast.Bite;r.due=p.clock+12;visible=true;}
                else if(r.cast==PondCast.Bite && p.clock>=r.due){PondFishing.Swim(p.fish[r.fish],p.clock,habitat);CastFishing(p,r,r.x,r.y);visible=true;}
            }
            var expired=p.food.Where(f=>p.clock>=f.until || f.fish>=0 && p.clock>=p.fish[f.fish].started+2.8).ToArray();
            if(expired.Length>0){p.food=p.food.Except(expired).ToArray();visible=true;}
            foreach(var f in p.fish)
                if(!p.rods.Any(r=>r.fish==f.id) && !p.food.Any(v=>v.fish==f.id) && p.clock>=f.started+f.duration){PondFishing.Swim(f,p.clock,habitat);AmbientChanged=true;}
            return true;
        }
        private void AfterPondAction(SoloCommand c,SoloPlayer player)
        {
            if(c.action==SoloAction.Pond || c.action==SoloAction.ChangeAvatar || c.action==SoloAction.ChangeOutfit || c.action==SoloAction.Roar)return;
            if(c.action==SoloAction.Move || c.action==SoloAction.Travel || c.action==SoloAction.UseFixture || c.action==SoloAction.UseStairs || c.action==SoloAction.EnterDoor || c.action==SoloAction.StartActivity || c.action==SoloAction.HideAndSeek || c.action==SoloAction.Grab)CancelPond(player.id);
        }
    }
}
