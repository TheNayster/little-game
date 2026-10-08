using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ZooPhase { Rest, Wander, Browse, Drink, Notice, Approach, Eat, Greet, Curious, WaterWalk, Splash, WaterReturn }
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
        public ZooFood Copy()=>(ZooFood)MemberwiseClone();
        public void Clear(){species="";slot=-1;ticket=0;offered=false;}
    }
    [Serializable] public sealed class ZooState
    {
        public ZooAnimal[] animals;
        public ZooFood[] food;
        public long nextTicket;
        // Additive snapshot hints, cleared on restore. They never represent a
        // saved chore, ownership lease, or a backlog of play requests.
        public int waterSequence;
        public double waterAge=10, waterCooldown, waterPendingSeconds, greetingCooldown, curiousCooldown=14;
        public bool waterPending;
        public ZooState Copy(){var z=(ZooState)MemberwiseClone();z.animals=animals.Select(a=>a.Copy()).ToArray();z.food=food.Select(f=>f.Copy()).ToArray();return z;}
    }
    public static class ZooLayout
    {
        public const int Schema=34;
        public const string Entrance="zoo",Savanna="zoo-savanna";
        // Center of the main entrance arch, on its clear foreground path.
        public const float EntranceArrivalX=1200;
        public const float WaterX=820, WaterY=320, WaterPlayX=1110;
        public const double WaterCooldown=1.2, WaterExpiry=30, SplashFeedDelay=.6;
        public static readonly string[] Species=ZooCatalog.All.Select(s=>s.id).ToArray();
        public static bool Area(string id)=>id==Entrance || ZooCatalog.Trail(id);
        public static float Center(string species)=>ZooCatalog.Get(species).Center;
        public static float BucketX(string species)=>Center(species)+360;
        public static float SlotX(string species,int slot)=>Center(species)+(slot-1.5f)*100;
        public static WalkPoint Floor(string area,float x,float y)=>new WalkPoint(x,Area(area)?Math.Max(35,Math.Min(120,y)):y);
        public static WalkPoint Point(ZooAnimal a,double extra=0)
        {
            var moving=a.phase==ZooPhase.Wander || a.phase==ZooPhase.Approach || a.phase==ZooPhase.WaterWalk || a.phase==ZooPhase.WaterReturn;
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
            if(z.waterSequence<0 || !ZooClock(z.waterAge,10) || !ZooClock(z.waterCooldown,ZooLayout.WaterCooldown) ||
                !ZooClock(z.waterPendingSeconds,ZooLayout.WaterExpiry) || !ZooClock(z.greetingCooldown,30) || !ZooClock(z.curiousCooldown,40) ||
                z.waterPending!=(z.waterPendingSeconds>0))throw new InvalidOperationException("Invalid elephant play hints.");
            foreach(var a in z.animals){var info=ZooCatalog.Get(a.species);
                if(a.random==0 || a.sequence<0 || a.fed<0 || a.previous< -1 || a.previous>3 || !Enum.IsDefined(typeof(ZooPhase),a.phase) ||
                    !HideAndSeek.Finite(a.age) || !HideAndSeek.Finite(a.duration) || a.age<0 || a.duration<.1 || a.duration>100 || a.age>a.duration+1 ||
                    !ZooPoint(a.fromX,a.fromY,info) || !ZooPoint(a.toX,a.toY,info) || a.owner==null || a.owner!="" && !s.players.Any(p=>p.id==a.owner) ||
                    (a.phase==ZooPhase.Notice || a.phase==ZooPhase.Approach || a.phase==ZooPhase.Eat)!=(a.owner!="") || a.consumed && a.phase!=ZooPhase.Eat ||
                    a.phase>=ZooPhase.Greet && a.species!="elephant")
                    throw new InvalidOperationException("Invalid zoo animal.");
                if(a.owner!="" && !z.food.Any(f=>f.actor==a.owner && f.species==a.species && f.offered))throw new InvalidOperationException("Missing food offer.");}
            foreach(var f in z.food){var p=s.players.Single(v=>v.id==f.actor);
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
            s.zoo.waterSequence=0;s.zoo.waterAge=10;s.zoo.waterCooldown=0;s.zoo.waterPending=false;s.zoo.waterPendingSeconds=0;
            s.zoo.greetingCooldown=30;s.zoo.curiousCooldown=14;s.revision++;
        }
        private bool CancelZoo(string actor)
        {
            var f=state.zoo?.food.FirstOrDefault(v=>v.actor==actor);if(f==null || f.species=="")return false;
            foreach(var a in state.zoo.animals.Where(a=>a.owner==actor))ZooLayout.Routine(a);f.Clear();return true;
        }
        public bool ReleaseZoo(string actor){if(!CancelZoo(actor))return false;state.revision++;return true;}
        private string ZooOperation(SoloCommand c,SoloPlayer p)
        {
            var z=state.zoo;if(z==null)return "zoo-upgrade-needed";
            if(c.value=="gate"){
                if(!ZooCatalog.Gate(p.zone,c.target,out var gateX,out var arrival))return "unknown-zoo-gate";
                if(Math.Abs(p.x-gateX)>200 || p.y>160)return "walk-to-zoo-gate";
                if(p.visit>=long.MaxValue-1)return "visit-limit";TravelPlayer(p,c.target);p.x=arrival;return null;
            }
            if(c.value=="return"){CancelZoo(p.id);return null;}
            if(!ZooLayout.Species.Contains(c.target) || p.zone!=ZooCatalog.Get(c.target).area)return "come-to-exhibit";
            if(c.value=="water"){
                if(c.target!="elephant" || !AtElephant(p))return "come-to-exhibit";
                if(z.waterCooldown>0 || elephantPumps.TryGetValue(p.id,out var nextPump) && nextPump>elephantClock)return "water-resting";
                if(z.waterSequence==int.MaxValue)return "water-limit";
                z.waterSequence++;z.waterAge=0;z.waterCooldown=ZooLayout.WaterCooldown;elephantPumps[p.id]=elephantClock+2.4;
                var elephant=z.animals[0];
                if(elephant.phase!=ZooPhase.WaterWalk && elephant.phase!=ZooPhase.Splash){z.waterPending=true;z.waterPendingSeconds=ZooLayout.WaterExpiry;}
                return null;
            }
            var food=z.food.Single(f=>f.actor==p.id);
            if(c.value=="take"){
                if(food.species!="" || state.toys.Any(t=>t.holder==p.id))return "hands-full";
                if(Math.Abs(p.x-ZooLayout.BucketX(c.target))>200 || p.y>160)return "walk-to-food-bucket";
                var slot=Enumerable.Range(0,4).FirstOrDefault(i=>!z.food.Any(f=>f.species==c.target && f.slot==i));
                if(z.food.Any(f=>f.species==c.target && f.slot==slot))return "all-feed-spots-busy";
                if(z.nextTicket>=long.MaxValue-1)return "portion-limit";
                food.species=c.target;food.slot=slot;food.ticket=++z.nextTicket;return null;
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
            elephantClock+=seconds;z.waterAge=Math.Min(10,z.waterAge+seconds);z.waterCooldown=Math.Max(0,z.waterCooldown-seconds);
            z.greetingCooldown=Math.Max(0,z.greetingCooldown-seconds);z.curiousCooldown=Math.Max(0,z.curiousCooldown-seconds);
            var arrived=false;var nearby=false;
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
                if(a.species=="elephant" && !nearby && a.phase>=ZooPhase.Greet){ZooLayout.Routine(a);AmbientChanged=true;}
                if(a.owner=="" && active!=null && !state.players.Any(p=>p.zone==ZooCatalog.Get(a.species).area && active.Contains(p.id)))continue;
                a.age+=seconds;
                if(a.owner==""){
                    var next=z.food.Where(f=>f.species==a.species && f.offered).OrderBy(f=>f.ticket).FirstOrDefault();
                    // Splash is the only atomic play transition; at most .6s
                    // remains before food wins. Walking/greetings yield now.
                    if(next!=null && !(a.phase==ZooPhase.Splash && a.age<ZooLayout.SplashFeedDelay)){
                        var point=ZooLayout.Point(a);a.owner=next.actor;a.consumed=false;ZooLayout.Segment(a,ZooPhase.Notice,point.X,point.Y,1.2);visible=true;continue;}
                    if(a.species=="elephant" && nearby && next==null && a.phase<ZooPhase.Greet){
                        var point=ZooLayout.Point(a);
                        if(z.waterPending){
                            z.waterPending=false;z.waterPendingSeconds=0;elephantReturnX=point.X;elephantReturnY=point.Y;
                            ZooLayout.Segment(a,ZooPhase.WaterWalk,ZooLayout.WaterPlayX,ZooLayout.WaterY,WaterWalkTime(point,ZooLayout.WaterPlayX,ZooLayout.WaterY));AmbientChanged=true;continue;
                        }
                        if(arrived && z.greetingCooldown==0){ZooLayout.Segment(a,ZooPhase.Greet,point.X,point.Y,2.2);z.greetingCooldown=30;AmbientChanged=true;continue;}
                        if(z.curiousCooldown==0){ZooLayout.Segment(a,ZooPhase.Curious,point.X,point.Y,3.2);z.curiousCooldown=20+ZooLayout.Roll(a)*18;AmbientChanged=true;continue;}
                    }
                }
                if(a.phase==ZooPhase.Eat && !a.consumed && a.age>=1.4){a.consumed=true;if(a.fed<int.MaxValue)a.fed++;visible=true;}
                if(a.age<a.duration)continue;
                var feeding=a.owner!="";
                if(a.phase==ZooPhase.Notice){var f=z.food.Single(v=>v.actor==a.owner);var point=ZooLayout.Point(a);var info=ZooCatalog.Get(a.species);var x=ZooLayout.SlotX(a.species,f.slot)+65-info.mouthX;
                    var distance=Math.Sqrt((point.X-x)*(point.X-x)+(point.Y-info.FeedY)*(point.Y-info.FeedY));ZooLayout.Segment(a,ZooPhase.Approach,x,info.FeedY,Math.Max(1.5,distance/(info.approachSpeed*1.4f)));}
                else if(a.phase==ZooPhase.Approach){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Eat,point.X,point.Y,4);}
                else if(a.phase==ZooPhase.WaterWalk){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Splash,point.X,point.Y,1.8);}
                else if(a.phase==ZooPhase.Splash){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.WaterReturn,elephantReturnX,elephantReturnY,WaterWalkTime(point,elephantReturnX,elephantReturnY));}
                else {if(a.owner!="")z.food.Single(v=>v.actor==a.owner).Clear();ZooLayout.Routine(a);}
                // Ambient routes do not conflict with a child's food/travel
                // transaction. Feeding lease transitions still advance revision.
                if(feeding)visible=true;else AmbientChanged=true;
            }
            return true;
        }
        private static double WaterWalkTime(WalkPoint from,float x,float y)=>Math.Max(.5,Math.Sqrt((from.X-x)*(from.X-x)+(from.Y-y)*(from.Y-y))/80);
    }
}
