using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ZooPhase { Rest, Wander, Browse, Drink, Notice, Approach, Eat, Greet, Curious, WaterWalk, Splash, WaterReturn, CareWalk, Care, CareFinish }
    [Serializable] public sealed class ZooAnimal
    {
        public string species, owner="";
        public uint random;
        public int sequence, fed, previous=-1;
        public ZooPhase phase;
        public double age, duration=5;
        public float fromX,fromY,toX,toY;
        public bool consumed;
        public ZooAnimal Copy()=>(ZooAnimal)MemberwiseClone();
    }
    [Serializable] public sealed class ZooFood
    {
        public string actor,species="";
        public int slot=-1;
        public long ticket;
        public bool offered;
        public bool preparing;
        public long prepEpoch,edit;
        public int[] pieces=Array.Empty<int>();
        public ZooFood Copy(){var f=(ZooFood)MemberwiseClone();f.pieces=(int[])pieces.Clone();return f;}
        public void Clear(){species="";slot=-1;ticket=0;offered=false;preparing=false;prepEpoch=0;edit=0;pieces=Array.Empty<int>();}
    }
    [Serializable] public sealed class ZooState
    {
        public ZooAnimal[] animals;
        public ZooFood[] food;
        public long nextTicket;
        public long nextPrep;
        // Additive snapshot hints, cleared on restore. They never represent a
        // saved chore, ownership lease, or a backlog of play requests.
        public int[] surpriseSequence=new int[2];
        public double[] surpriseAge={10,10};
        public int[] surpriseVariation=new int[2];
        public int waterSequence;
        public double waterAge=10, waterCooldown, waterPendingSeconds, greetingCooldown, curiousCooldown=14;
        public bool waterPending;
        public int careSession;
        public string[] careMembers=Array.Empty<string>();
        public int[] careProgress=new int[3];
        public int[] carePatch=new int[4];
        public int[] careMemberEpoch=new int[4];
        public int nextCareEpoch;
        public double[] careBrushAge={10,10,10,10};
        public bool careComplete,careReacted;
        public double careSeconds;
        public long careOrder,waterOrder,nextPlayOrder;
        public ZooState Copy(){var z=(ZooState)MemberwiseClone();z.surpriseSequence=(int[])surpriseSequence.Clone();z.surpriseAge=(double[])surpriseAge.Clone();z.surpriseVariation=(int[])surpriseVariation.Clone();z.animals=animals.Select(a=>a.Copy()).ToArray();z.food=food.Select(f=>f.Copy()).ToArray();z.careMembers=(string[])careMembers.Clone();z.careProgress=(int[])careProgress.Clone();z.carePatch=(int[])carePatch.Clone();z.careMemberEpoch=(int[])careMemberEpoch.Clone();z.careBrushAge=(double[])careBrushAge.Clone();return z;}
    }
    public static class ZooLayout
    {
        public const int Schema=34;
        public const string Entrance="zoo",Savanna="zoo-savanna";
        // Center of the main entrance arch, on its clear foreground path.
        public const float EntranceArrivalX=1200;
        public const float WaterX=820, WaterY=320, WaterPlayX=1110;
        public const float CareX=1200, CareY=320, BrushX=1740;
        public const float SnackX=2020;
        public const string BirdSurprise="elephant-leaves", ButterflySurprise="elephant-flowers";
        public const double SurpriseReset=8;
        public const double CareFeedDelay=.35;
        public const double WaterCooldown=1.2, WaterExpiry=30, SplashFeedDelay=.6;
        public static readonly string[] Species=ZooCatalog.All.Select(s=>s.id).ToArray();
        public static bool Area(string id)=>id==Entrance || ZooCatalog.Trail(id);
        public static float Center(string species)=>ZooCatalog.Get(species).Center;
        public static float BucketX(string species)=>Center(species)+360;
        public static float SlotX(string species,int slot)=>Center(species)+(slot-1.5f)*100;
        public static WalkPoint Floor(string area,float x,float y)=>new WalkPoint(x,Area(area)?Math.Max(35,Math.Min(120,y)):y);
        public static WalkPoint Point(ZooAnimal a,double extra=0)
        {
            var moving=a.phase==ZooPhase.Wander || a.phase==ZooPhase.Approach || a.phase==ZooPhase.WaterWalk || a.phase==ZooPhase.WaterReturn || a.phase==ZooPhase.CareWalk;
            var t=moving?(float)Math.Max(0,Math.Min(1,(a.age+extra)/a.duration)):0;
            return new WalkPoint(a.fromX+(a.toX-a.fromX)*t,a.fromY+(a.toY-a.fromY)*t);
        }
        // The persisted stream is chosen once by the authority. Presentation
        // evaluates its segments; joining clients never roll their own route.
        internal static double Roll(ZooAnimal a)
        {uint n=a.random;n^=n<<13;n^=n>>17;n^=n<<5;a.random=n;return n/(double)uint.MaxValue;}
        internal static void Segment(ZooAnimal a,ZooPhase phase,float x,float y,double duration)
        {
            var p=Point(a);a.fromX=p.X;a.fromY=p.Y;a.toX=x;a.toY=y;a.phase=phase;a.age=0;a.duration=Math.Max(.1,duration);
            if(a.sequence==int.MaxValue)a.sequence=0;else a.sequence++;
        }
        internal static void Routine(ZooAnimal a)
        {
            var info=ZooCatalog.Get(a.species);var roll=Roll(a);var next=roll<.52?1:roll<.75?2:roll<.9?3:0;
            if(next==a.previous)next=(next+1+(int)(Roll(a)*3))%4;a.previous=next;a.owner="";a.consumed=false;
            var p=Point(a);var center=Center(a.species);
            if(next==1){var x=info.MinX+(float)Roll(a)*(info.MaxX-info.MinX);var y=300+(float)Roll(a)*(info.MaxY-300);
                var distance=Math.Sqrt((x-p.X)*(x-p.X)+(y-p.Y)*(y-p.Y));Segment(a,ZooPhase.Wander,x,y,Math.Max(3,distance/(info.speed*(.8+Roll(a)*.4))));}
            else Segment(a,(ZooPhase)next,p.X,p.Y,4+Roll(a)*8);
        }
    }
    public sealed partial class GameWorld
    {
        private readonly System.Collections.Generic.HashSet<string> elephantVisitors=new System.Collections.Generic.HashSet<string>();
        private readonly System.Collections.Generic.Dictionary<string,double> elephantPumps=new System.Collections.Generic.Dictionary<string,double>();
        private double elephantClock;
        private readonly System.Collections.Generic.Dictionary<string,double> elephantBrushAt=new System.Collections.Generic.Dictionary<string,double>();
        private readonly System.Collections.Generic.Dictionary<string,long> elephantBrushSerial=new System.Collections.Generic.Dictionary<string,long>();
        private float elephantReturnX,elephantReturnY;
        private static bool AtElephant(SoloPlayer p)=>p.zone==ZooLayout.Savanna && p.x>=200 && p.x<2200;
        public ZooState ReadZoo()=>state.zoo?.Copy();
        public static GameWorld WithZoo(GameWorld world)
        {
            world=WithPark(world);if(world.Schema>=ZooCatalog.Schema && world.state.zoo?.animals?.Length==16)return world;
            var s=world.Snapshot();uint seed=2166136261;
            foreach(var ch in s.worldId)seed=unchecked((seed^ch)*16777619);
            var existing=s.zoo;
            s.zoo=new ZooState{nextTicket=existing?.nextTicket??0,animals=ZooLayout.Species.Select((id,i)=>existing?.animals.FirstOrDefault(a=>a.species==id)?.Copy()??new ZooAnimal{species=id,random=(seed^(uint)(i+1)*2654435761u)|1u,
                fromX=ZooLayout.Center(id)-Math.Min(200,ZooCatalog.Get(id).Radius),toX=ZooLayout.Center(id)-Math.Min(200,ZooCatalog.Get(id).Radius),fromY=360,toY=360,duration=3+i*2}).ToArray(),food=existing?.food.Select(f=>f.Copy()).ToArray()??s.players.Select(p=>new ZooFood{actor=p.id}).ToArray()};
            s.schema=Math.Max(s.schema,ZooCatalog.Schema);s.revision++;Validate(s);return new GameWorld(s);
        }
        private static void NormalizeZooInline(SoloSnapshot s)
        {
            if(s?.zoo!=null){
                if(s.zoo.food!=null)foreach(var f in s.zoo.food)if(f!=null && f.pieces==null)f.pieces=Array.Empty<int>();
                if(s.zoo.surpriseSequence==null)s.zoo.surpriseSequence=new int[2];
                if(s.zoo.surpriseAge==null)s.zoo.surpriseAge=new double[]{10,10};
                if(s.zoo.surpriseVariation==null)s.zoo.surpriseVariation=new int[2];
            }
            if(s?.zoo!=null && s.zoo.careSession==0 && !s.zoo.careComplete && s.zoo.careSeconds==0){
                if(s.zoo.careMembers==null)s.zoo.careMembers=Array.Empty<string>();
                if(s.zoo.careProgress==null)s.zoo.careProgress=new int[3];
                if(s.zoo.careMemberEpoch==null)s.zoo.careMemberEpoch=new int[4];
                if(s.zoo.carePatch==null)s.zoo.carePatch=new int[4];
                if(s.zoo.careBrushAge==null)s.zoo.careBrushAge=new double[]{10,10,10,10};
            }
            if(s!=null && s.schema<WorldLayout.Schema && s.zoo!=null && (s.zoo.animals==null || s.zoo.animals.Length==0) &&
                (s.zoo.food==null || s.zoo.food.Length==0) && s.zoo.nextTicket==0)s.zoo=null;
        }
        private static void ValidateZoo(SoloSnapshot s)
        {
            if(s.zoo==null && s.schema<WorldLayout.Schema)return;
            if(s.schema<ZooLayout.Schema){if(s.zoo!=null || s.players.Any(p=>ZooLayout.Area(p.zone)))throw new InvalidOperationException("Zoo requires schema 34.");return;}
            var z=s.zoo;
            if(z==null || z.animals==null || !(z.animals.Length==16 || s.schema<WorldLayout.Schema && z.animals.Length==2) || !z.animals.Select(a=>a?.species).SequenceEqual(z.animals.Length==2?ZooLayout.Species.Take(2):ZooLayout.Species) ||
                z.food==null || !z.food.Select(f=>f?.actor).OrderBy(id=>id).SequenceEqual(s.players.Select(p=>p.id).OrderBy(id=>id)) ||
                z.nextTicket<0 || z.nextTicket==long.MaxValue)throw new InvalidOperationException("Invalid zoo record.");
            if(z.surpriseSequence==null || z.surpriseSequence.Length!=2 || z.surpriseSequence.Any(n=>n<0) || z.surpriseAge==null || z.surpriseAge.Length!=2 || z.surpriseAge.Any(n=>!ZooClock(n,10)) || z.surpriseVariation==null || z.surpriseVariation.Length!=2 || z.surpriseVariation.Any(n=>n<0 || n>1))throw new InvalidOperationException("Invalid Zoo surprise hints.");
            if(z.waterSequence<0 || !ZooClock(z.waterAge,10) || !ZooClock(z.waterCooldown,ZooLayout.WaterCooldown) ||
                !ZooClock(z.waterPendingSeconds,ZooLayout.WaterExpiry) || !ZooClock(z.greetingCooldown,30) || !ZooClock(z.curiousCooldown,40) ||
                z.waterPending!=(z.waterPendingSeconds>0))throw new InvalidOperationException("Invalid elephant play hints.");
            if(z.careSession<0 || z.careMembers==null || z.careMembers.Length>4 || z.careMembers.Distinct().Count()!=z.careMembers.Length ||
                z.careMembers.Any(id=>!s.players.Any(p=>p.id==id)) || z.careProgress==null || z.careProgress.Length!=3 || z.careProgress.Any(n=>n<0 || n>4) || z.carePatch==null || z.carePatch.Length!=4 || z.carePatch.Any(n=>n<0 || n>2) || z.careMemberEpoch==null || z.careMemberEpoch.Length!=4 || z.nextCareEpoch<0 || z.careMemberEpoch.Any(n=>n<0 || n>z.nextCareEpoch) || z.careBrushAge==null || z.careBrushAge.Length!=4 || z.careBrushAge.Any(n=>!ZooClock(n,10)) ||
                !ZooClock(z.careSeconds,90) || z.careComplete && (z.careSession==0 || z.careProgress.Any(n=>n!=4)) ||
                z.careOrder<0 || z.waterOrder<0 || z.nextPlayOrder<0 || z.nextPlayOrder==long.MaxValue || z.careOrder>z.nextPlayOrder || z.waterOrder>z.nextPlayOrder)
                throw new InvalidOperationException("Invalid elephant care hints.");
            foreach(var a in z.animals){var info=ZooCatalog.Get(a.species);
                if(a.random==0 || a.sequence<0 || a.fed<0 || a.previous< -1 || a.previous>3 || !Enum.IsDefined(typeof(ZooPhase),a.phase) ||
                    !HideAndSeek.Finite(a.age) || !HideAndSeek.Finite(a.duration) || a.age<0 || a.duration<.1 || a.duration>100 || a.age>a.duration+1 ||
                    !ZooPoint(a.fromX,a.fromY,info) || !ZooPoint(a.toX,a.toY,info) || a.owner==null || a.owner!="" && !s.players.Any(p=>p.id==a.owner) ||
                    (a.phase==ZooPhase.Notice || a.phase==ZooPhase.Approach || a.phase==ZooPhase.Eat)!=(a.owner!="") || a.consumed && a.phase!=ZooPhase.Eat ||
                    a.phase>=ZooPhase.Greet && a.species!="elephant")
                    throw new InvalidOperationException("Invalid zoo animal.");
                if(a.owner!="" && !z.food.Any(f=>f.actor==a.owner && f.species==a.species && f.offered))throw new InvalidOperationException("Missing food offer.");}
            foreach(var f in z.food){var p=s.players.Single(v=>v.id==f.actor);
                if(z.nextPrep<0 || z.nextPrep==long.MaxValue || f.pieces==null || f.pieces.Length>3 || f.pieces.Any(n=>!ZooCatalog.Get("elephant").AcceptsSnack(n)) ||
                    f.prepEpoch<0 || f.prepEpoch>z.nextPrep || f.edit<0 || f.edit==long.MaxValue ||
                    f.preparing && (f.species!="" || f.prepEpoch==0 || p.zone!=ZooLayout.Savanna) ||
                    f.pieces.Length>0 && !f.preparing && f.species!="elephant" ||
                    f.species!="" && f.preparing || f.species=="" && !f.preparing && (f.pieces.Length!=0 || f.prepEpoch!=0 || f.edit!=0))
                    throw new InvalidOperationException("Invalid elephant snack.");
                if(f.species==null || f.species!="" && !ZooLayout.Species.Contains(f.species) ||
                    f.species=="" && (f.slot!=-1 || f.ticket!=0 || f.offered) || f.species!="" && (f.slot<0 || f.slot>3 || f.ticket<=0 || f.ticket>z.nextTicket || p.zone!=ZooCatalog.Get(f.species).area))
                    throw new InvalidOperationException("Invalid zoo portion.");}
            if(z.food.Where(f=>f.species!="").GroupBy(f=>f.species+"/"+f.slot).Any(g=>g.Count()>1) ||
                z.food.Where(f=>f.ticket>0).GroupBy(f=>f.ticket).Any(g=>g.Count()>1))throw new InvalidOperationException("Duplicate zoo food lease.");
        }
        private static bool ZooClock(double n,double max)=>HideAndSeek.Finite(n) && n>=0 && n<=max;
        private static bool ZooPoint(float x,float y,ZooSpecies info)=>!float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && x>=info.MinX && x<=info.MaxX && y>=info.MinY && y<=info.MaxY;
        private static void SuspendZoo(SoloSnapshot s)
        {
            if(s.zoo==null)return;
            foreach(var a in s.zoo.animals.Where(a=>a.owner!="" || a.phase>=ZooPhase.Greet))ZooLayout.Routine(a);
            foreach(var f in s.zoo.food)f.Clear();
            s.zoo.surpriseSequence=new int[2];s.zoo.surpriseAge=new double[]{10,10};s.zoo.surpriseVariation=new int[2];
            s.zoo.waterSequence=0;s.zoo.waterAge=10;s.zoo.waterCooldown=0;s.zoo.waterPending=false;s.zoo.waterPendingSeconds=0;
            ClearElephantCare(s.zoo);s.zoo.careSession=0;s.zoo.nextCareEpoch=0;s.zoo.nextPlayOrder=0;s.zoo.waterOrder=0;
            s.zoo.greetingCooldown=30;s.zoo.curiousCooldown=14;s.revision++;
        }
        private bool CancelZoo(string actor)
        {
            var f=state.zoo?.food.FirstOrDefault(v=>v.actor==actor);if(f==null || f.species=="" && !f.preparing)return false;
            foreach(var a in state.zoo.animals.Where(a=>a.owner==actor))ZooLayout.Routine(a);f.Clear();return true;
        }
        public bool ReleaseZoo(string actor){
            var changed=CancelZoo(actor);var z=state.zoo;if(z==null)return changed;
            changed|=z.careMembers.Contains(actor);LeaveElephantCare(actor);elephantVisitors.Remove(actor);
            // An empty FamilySession stops ticking, so the last disconnect must
            // release transient routines here rather than wait for AdvanceZoo.
            if(elephantVisitors.Count==0 && z.careMembers.Length==0){
                changed|=z.waterPending || z.animals[0].phase>=ZooPhase.Greet;
                z.waterPending=false;z.waterPendingSeconds=0;
                if(z.animals[0].phase>=ZooPhase.Greet)ZooLayout.Routine(z.animals[0]);
            }
            if(changed)state.revision++;return changed;
        }
        private string ZooOperation(SoloCommand c,SoloPlayer p)
        {
            var z=state.zoo;if(z==null)return "zoo-upgrade-needed";
            if(c.value=="gate"){
                if(!ZooCatalog.Gate(p.zone,c.target,out var gateX,out var arrival))return "unknown-zoo-gate";
                if(Math.Abs(p.x-gateX)>200 || p.y>160)return "walk-to-zoo-gate";
                if(p.visit>=long.MaxValue-1)return "visit-limit";TravelPlayer(p,c.target);p.x=arrival;return null;
            }
            if(c.value=="return"){CancelZoo(p.id);LeaveElephantCare(p.id);return null;}
            if(c.value=="surprise"){
                var i=c.target==ZooLayout.BirdSurprise?0:c.target==ZooLayout.ButterflySurprise?1:-1;
                if(i<0)return "unknown-zoo-surprise";
                if(!AtElephant(p))return "come-to-exhibit";
                if(z.surpriseAge[i]<ZooLayout.SurpriseReset)return "surprise-resting";
                if(z.surpriseSequence[i]==int.MaxValue)return "surprise-limit";
                // Independent from the animal's random stream and scheduling.
                z.surpriseSequence[i]++;z.surpriseVariation[i]=z.surpriseSequence[i]%2;z.surpriseAge[i]=0;return null;
            }
            if(!ZooLayout.Species.Contains(c.target) || p.zone!=ZooCatalog.Get(c.target).area)return "come-to-exhibit";
            if(c.value=="care" || c.value=="brush" || c.value=="put-brush")return ElephantCareOperation(c,p);
            if(c.value=="water"){
                if(c.target!="elephant" || !AtElephant(p))return "come-to-exhibit";
                if(z.waterCooldown>0 || elephantPumps.TryGetValue(p.id,out var nextPump) && nextPump>elephantClock)return "water-resting";
                if(z.waterSequence==int.MaxValue || z.nextPlayOrder>=long.MaxValue-1)return "water-limit";
                z.waterSequence++;z.waterAge=0;z.waterCooldown=ZooLayout.WaterCooldown;elephantPumps[p.id]=elephantClock+2.4;
                var elephant=z.animals[0];
                if(elephant.phase!=ZooPhase.WaterWalk && elephant.phase!=ZooPhase.Splash){if(!z.waterPending)z.waterOrder=++z.nextPlayOrder;z.waterPending=true;z.waterPendingSeconds=ZooLayout.WaterExpiry;}
                return null;
            }
            var food=z.food.Single(f=>f.actor==p.id);
            if(c.value.StartsWith("snack-",StringComparison.Ordinal))return ElephantSnackOperation(c,p,food);
            if(c.value=="take"){
                if(food.species!="" || food.preparing || state.toys.Any(t=>t.holder==p.id))return "hands-full";
                if(Math.Abs(p.x-ZooLayout.BucketX(c.target))>200 || p.y>160)return "walk-to-food-bucket";
                return LeaseZooFood(food,c.target);
            }
            if(c.value=="offer"){
                if(food.species!=c.target || Math.Abs(p.x-ZooLayout.SlotX(c.target,food.slot))>70 || p.y>160)return "walk-to-feed-spot";
                food.offered=true;return null;
            }
            return "unknown-zoo-action";
        }
        private bool AdvanceZoo(double seconds,string[] active,out bool visible)
        {
            visible=false;var z=state.zoo;if(z==null)return false;
            for(var i=0;i<2;i++)z.surpriseAge[i]=Math.Min(10,z.surpriseAge[i]+seconds);
            elephantClock+=seconds;for(var i=0;i<4;i++)z.careBrushAge[i]=Math.Min(10,z.careBrushAge[i]+seconds);z.waterAge=Math.Min(10,z.waterAge+seconds);z.waterCooldown=Math.Max(0,z.waterCooldown-seconds);
            z.greetingCooldown=Math.Max(0,z.greetingCooldown-seconds);z.curiousCooldown=Math.Max(0,z.curiousCooldown-seconds);
            var arrived=false;var nearby=false;
            foreach(var f in z.food.Where(f=>f.preparing).ToArray()){
                var p=state.players.Single(v=>v.id==f.actor);
                if(!AtElephant(p) || active!=null && !active.Contains(p.id))visible|=CancelZoo(p.id);
            }
            foreach(var id in z.careMembers.ToArray()){
                var player=state.players.Single(v=>v.id==id);
                if(!AtElephant(player) || active!=null && !active.Contains(id)) {LeaveElephantCare(id);visible=true;}
            }
            if(z.careMembers.Length>0){z.careSeconds=Math.Max(0,z.careSeconds-seconds);if(z.careSeconds==0){ClearElephantCare(z);AmbientChanged=true;}}
            foreach(var p in state.players){
                var present=AtElephant(p) && (active==null || active.Contains(p.id));
                if(present){nearby=true;if(elephantVisitors.Add(p.id))arrived=true;}else elephantVisitors.Remove(p.id);
            }
            if(z.waterPending){z.waterPendingSeconds=nearby?Math.Max(0,z.waterPendingSeconds-seconds):0;if(z.waterPendingSeconds==0){z.waterPending=false;AmbientChanged=true;}}
            foreach(var f in z.food.Where(f=>f.species!="").ToArray()){
                var p=state.players.Single(v=>v.id==f.actor);
                if(p.zone!=ZooCatalog.Get(f.species).area || active!=null && !active.Contains(p.id) || f.offered && (Math.Abs(p.x-ZooLayout.SlotX(f.species,f.slot))>160 || p.y>160))visible|=CancelZoo(p.id);
            }
            foreach(var a in z.animals){
                if(a.species=="elephant" && (a.phase>=ZooPhase.CareWalk && z.careMembers.Length==0 || !nearby && a.phase>=ZooPhase.Greet)){ZooLayout.Routine(a);AmbientChanged=true;}
                if(a.owner=="" && active!=null && !state.players.Any(p=>p.zone==ZooCatalog.Get(a.species).area && active.Contains(p.id)))continue;
                a.age+=seconds;
                if(a.owner==""){
                    var next=z.food.Where(f=>f.species==a.species && f.offered).OrderBy(f=>f.ticket).FirstOrDefault();
                    // Splash is the only atomic play transition; at most .6s
                    // remains before food wins. Walking/greetings yield now.
                    if(next!=null && !(a.phase==ZooPhase.Splash && a.age<ZooLayout.SplashFeedDelay || (a.phase==ZooPhase.Care || a.phase==ZooPhase.CareFinish) && a.age<ZooLayout.CareFeedDelay)){
                        var point=ZooLayout.Point(a);a.owner=next.actor;a.consumed=false;ZooLayout.Segment(a,ZooPhase.Notice,point.X,point.Y,1.2);visible=true;continue;}
                    if(a.species=="elephant" && nearby && next==null && a.phase<ZooPhase.Greet){
                        var point=ZooLayout.Point(a);
                        if(z.careMembers.Length>0 && (!z.careComplete || !z.careReacted) && (!z.waterPending || z.careOrder<z.waterOrder)){
                            ZooLayout.Segment(a,ZooPhase.CareWalk,ZooLayout.CareX,ZooLayout.CareY,WaterWalkTime(point,ZooLayout.CareX,ZooLayout.CareY));AmbientChanged=true;continue;
                        }
                        if(z.waterPending){
                            z.waterPending=false;z.waterPendingSeconds=0;elephantReturnX=point.X;elephantReturnY=point.Y;
                            ZooLayout.Segment(a,ZooPhase.WaterWalk,ZooLayout.WaterPlayX,ZooLayout.WaterY,WaterWalkTime(point,ZooLayout.WaterPlayX,ZooLayout.WaterY));AmbientChanged=true;continue;
                        }
                        if(arrived && z.greetingCooldown==0){ZooLayout.Segment(a,ZooPhase.Greet,point.X,point.Y,2.2);z.greetingCooldown=30;AmbientChanged=true;continue;}
                        if(z.curiousCooldown==0){ZooLayout.Segment(a,ZooPhase.Curious,point.X,point.Y,3.2);z.curiousCooldown=20+ZooLayout.Roll(a)*18;AmbientChanged=true;continue;}
                    }
                }
                if(a.phase==ZooPhase.Eat && !a.consumed && a.age>=1.4){a.consumed=true;if(a.fed<int.MaxValue)a.fed++;visible=true;}
                if(a.phase==ZooPhase.Care && z.careMembers.Length>0){
                    if(z.careComplete){z.careReacted=true;var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.CareFinish,point.X,point.Y,2);AmbientChanged=true;}
                    else a.age=Math.Min(a.age,a.duration);
                    continue;
                }
                if(a.age<a.duration)continue;
                var feeding=a.owner!="";
                if(a.phase==ZooPhase.Notice){var f=z.food.Single(v=>v.actor==a.owner);var point=ZooLayout.Point(a);var info=ZooCatalog.Get(a.species);var x=ZooLayout.SlotX(a.species,f.slot)+65-info.mouthX;
                    var distance=Math.Sqrt((point.X-x)*(point.X-x)+(point.Y-info.FeedY)*(point.Y-info.FeedY));ZooLayout.Segment(a,ZooPhase.Approach,x,info.FeedY,Math.Max(1.5,distance/(info.approachSpeed*1.4f)));}
                else if(a.phase==ZooPhase.Approach){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Eat,point.X,point.Y,4);}
                else if(a.phase==ZooPhase.CareWalk){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Care,point.X,point.Y,90);}
                else if(a.phase==ZooPhase.WaterWalk){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Splash,point.X,point.Y,1.8);}
                else if(a.phase==ZooPhase.Splash){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.WaterReturn,elephantReturnX,elephantReturnY,WaterWalkTime(point,elephantReturnX,elephantReturnY));}
                else {if(a.owner!="")z.food.Single(v=>v.actor==a.owner).Clear();ZooLayout.Routine(a);}
                // Ambient routes do not conflict with a child's food/travel
                // transaction. Feeding lease transitions still advance revision.
                if(feeding)visible=true;else AmbientChanged=true;
            }
            return true;
        }
        private static void ClearElephantCare(ZooState z){z.careMembers=Array.Empty<string>();z.careProgress=new int[3];z.carePatch=new int[4];z.careMemberEpoch=new int[4];z.careBrushAge=new double[]{10,10,10,10};z.careComplete=false;z.careReacted=false;z.careSeconds=0;z.careOrder=0;}
        private void LeaveElephantCare(string actor){var z=state.zoo;if(z==null)return;var old=z.careMembers;var patches=z.carePatch;var ages=z.careBrushAge;var epochs=z.careMemberEpoch;z.careMembers=old.Where(id=>id!=actor).ToArray();z.carePatch=new int[4];z.careMemberEpoch=new int[4];z.careBrushAge=new double[]{10,10,10,10};for(var i=0;i<z.careMembers.Length;i++){z.carePatch[i]=patches[Array.IndexOf(old,z.careMembers[i])];z.careBrushAge[i]=ages[Array.IndexOf(old,z.careMembers[i])];z.careMemberEpoch[i]=epochs[Array.IndexOf(old,z.careMembers[i])];}if(z.careMembers.Length==0){ClearElephantCare(z);if(z.animals[0].phase>=ZooPhase.CareWalk)ZooLayout.Routine(z.animals[0]);}}
        private string ElephantCareOperation(SoloCommand c,SoloPlayer p)
        {
            var z=state.zoo;
            if(c.target!="elephant" || !AtElephant(p))return "come-to-exhibit";
            if(c.value=="put-brush"){LeaveElephantCare(p.id);return null;}
            if(c.value=="care"){
                if(z.careMembers.Contains(p.id) && !z.careComplete)return null;
                if(z.careComplete && z.animals[0].phase==ZooPhase.CareFinish)return "care-finishing";
                if(z.careMembers.Length==0 || z.careComplete){
                    if(z.careSession==int.MaxValue || z.nextPlayOrder>=long.MaxValue-1)return "care-limit";
                    z.careSession++;z.careProgress=new int[3];z.carePatch=new int[4];z.careMemberEpoch=new int[4];z.careBrushAge=new double[]{10,10,10,10};z.careComplete=false;z.careReacted=false;z.careOrder=++z.nextPlayOrder;
                    elephantBrushSerial.Clear();elephantBrushAt.Clear();
                }
                if(!z.careMembers.Contains(p.id)){
                    if(z.nextCareEpoch==int.MaxValue)return "care-limit";
                    var index=z.careMembers.Length;z.careMembers=z.careMembers.Concat(new[]{p.id}).ToArray();z.careMemberEpoch[index]=++z.nextCareEpoch;
                    elephantBrushSerial.Remove(p.id);elephantBrushAt.Remove(p.id);
                }
                z.careSeconds=90;return null;
            }
            // Session + monotonic gesture serial rejects old/reordered/repeated
            // input without retaining an unbounded gesture/message history.
            var parts=(c.item??"").Split('/');
            var member=Array.IndexOf(z.careMembers,p.id);
            if(parts.Length!=4 || member<0 || !int.TryParse(parts[1],out var epoch) || epoch!=z.careMemberEpoch[member] || !int.TryParse(parts[0],out var session) || session!=z.careSession ||
                !int.TryParse(parts[2],out var patch) || patch<0 || patch>=3 || !long.TryParse(parts[3],out var serial) || serial<=0 ||
                !z.careMembers.Contains(p.id) || z.careComplete || z.animals[0].phase!=ZooPhase.Care)return "care-unavailable";
            if(elephantBrushSerial.TryGetValue(p.id,out var last) && serial<=last)return "care-repeat";
            elephantBrushSerial[p.id]=serial;
            if(elephantBrushAt.TryGetValue(p.id,out var next) && next>elephantClock)return "gentle-brush";
            elephantBrushAt[p.id]=elephantClock+.35;
            if(z.careProgress[patch]>=4)return "patch-clear";
            z.carePatch[Array.IndexOf(z.careMembers,p.id)]=patch;z.careBrushAge[Array.IndexOf(z.careMembers,p.id)]=0;
            z.careProgress[patch]=Math.Min(4,z.careProgress[patch]+1);z.careSeconds=90;
            z.careComplete=z.careProgress.All(n=>n==4);return null;
        }
        private static double WaterWalkTime(WalkPoint from,float x,float y)=>Math.Max(.5,Math.Sqrt((from.X-x)*(from.X-x)+(from.Y-y)*(from.Y-y))/80);
    }
}
