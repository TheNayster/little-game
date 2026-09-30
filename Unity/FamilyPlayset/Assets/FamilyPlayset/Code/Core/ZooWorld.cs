using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ZooPhase { Rest, Wander, Browse, Drink, Notice, Approach, Eat }
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
        public ZooState Copy()=>new ZooState{nextTicket=nextTicket,animals=animals.Select(a=>a.Copy()).ToArray(),food=food.Select(f=>f.Copy()).ToArray()};
    }
    public static class ZooLayout
    {
        public const int Schema=34;
        public const string Entrance="zoo",Savanna="zoo-savanna";
        public static readonly string[] Species={"elephant","giraffe"};
        public static bool Area(string id)=>id==Entrance || id==Savanna;
        public static float Center(string species)=>species=="elephant"?1200:3600;
        public static float BucketX(string species)=>Center(species)+360;
        public static float SlotX(string species,int slot)=>Center(species)+(slot-1.5f)*100;
        public static WalkPoint Floor(string area,float x,float y)=>new WalkPoint(x,Area(area)?Math.Max(35,Math.Min(120,y)):y);
        public static WalkPoint Point(ZooAnimal a,double extra=0)
        {
            var moving=a.phase==ZooPhase.Wander || a.phase==ZooPhase.Approach;
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
            var roll=Roll(a);var next=roll<.55?1:roll<.85?2:0;
            if(next==a.previous)next=(next+1+(int)(Roll(a)*2))%3;a.previous=next;a.owner="";a.consumed=false;
            var p=Point(a);var center=Center(a.species);
            if(next==1){var x=center-600+(float)Roll(a)*1200;var y=300+(float)Roll(a)*120;
                var distance=Math.Sqrt((x-p.X)*(x-p.X)+(y-p.Y)*(y-p.Y));Segment(a,ZooPhase.Wander,x,y,Math.Max(3,distance/(38+Roll(a)*18)));}
            else Segment(a,(ZooPhase)next,p.X,p.Y,4+Roll(a)*8);
        }
    }
    public sealed partial class SoloWorld
    {
        public ZooState ReadZoo()=>state.zoo?.Copy();
        public static SoloWorld WithZoo(SoloWorld world)
        {
            world=WithPark(world);if(world.Schema>=ZooLayout.Schema)return world;
            var s=world.Snapshot();uint seed=2166136261;
            foreach(var ch in s.worldId)seed=unchecked((seed^ch)*16777619);
            s.zoo=new ZooState{animals=ZooLayout.Species.Select((id,i)=>new ZooAnimal{species=id,random=(seed^(uint)(i+1)*2654435761u)|1u,
                fromX=ZooLayout.Center(id)-300,toX=ZooLayout.Center(id)-300,fromY=360,toY=360,duration=3+i*2}).ToArray(),food=s.players.Select(p=>new ZooFood{actor=p.id}).ToArray()};
            s.schema=ZooLayout.Schema;s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeZooInline(SoloSnapshot s)
        {
            if(s!=null && s.schema<ZooLayout.Schema && s.zoo!=null && (s.zoo.animals==null || s.zoo.animals.Length==0) &&
                (s.zoo.food==null || s.zoo.food.Length==0) && s.zoo.nextTicket==0)s.zoo=null;
        }
        private static void ValidateZoo(SoloSnapshot s)
        {
            if(s.schema<ZooLayout.Schema){if(s.zoo!=null || s.players.Any(p=>ZooLayout.Area(p.zone)))throw new InvalidOperationException("Zoo requires schema 34.");return;}
            var z=s.zoo;
            if(z==null || z.animals==null || z.animals.Length!=2 || !z.animals.Select(a=>a?.species).SequenceEqual(ZooLayout.Species) ||
                z.food==null || !z.food.Select(f=>f?.actor).OrderBy(id=>id).SequenceEqual(s.players.Select(p=>p.id).OrderBy(id=>id)) ||
                z.nextTicket<0 || z.nextTicket==long.MaxValue)throw new InvalidOperationException("Invalid zoo record.");
            foreach(var a in z.animals){var c=ZooLayout.Center(a.species);
                if(a.random==0 || a.sequence<0 || a.fed<0 || a.previous< -1 || a.previous>3 || !Enum.IsDefined(typeof(ZooPhase),a.phase) ||
                    !HideAndSeek.Finite(a.age) || !HideAndSeek.Finite(a.duration) || a.age<0 || a.duration<.1 || a.duration>100 || a.age>a.duration+1 ||
                    !ZooPoint(a.fromX,a.fromY,c) || !ZooPoint(a.toX,a.toY,c) || a.owner==null || a.owner!="" && !s.players.Any(p=>p.id==a.owner) ||
                    (a.phase==ZooPhase.Notice || a.phase==ZooPhase.Approach || a.phase==ZooPhase.Eat)!=(a.owner!="") || a.consumed && a.phase!=ZooPhase.Eat)
                    throw new InvalidOperationException("Invalid zoo animal.");
                if(a.owner!="" && !z.food.Any(f=>f.actor==a.owner && f.species==a.species && f.offered))throw new InvalidOperationException("Missing food offer.");}
            foreach(var f in z.food){var p=s.players.Single(v=>v.id==f.actor);
                if(f.species==null || f.species!="" && !ZooLayout.Species.Contains(f.species) ||
                    f.species=="" && (f.slot!=-1 || f.ticket!=0 || f.offered) || f.species!="" && (f.slot<0 || f.slot>3 || f.ticket<=0 || f.ticket>z.nextTicket || p.zone!=ZooLayout.Savanna))
                    throw new InvalidOperationException("Invalid zoo portion.");}
            if(z.food.Where(f=>f.species!="").GroupBy(f=>f.species+"/"+f.slot).Any(g=>g.Count()>1) ||
                z.food.Where(f=>f.ticket>0).GroupBy(f=>f.ticket).Any(g=>g.Count()>1))throw new InvalidOperationException("Duplicate zoo food lease.");
        }
        private static bool ZooPoint(float x,float y,float center)=>!float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && x>=center-600 && x<=center+600 && y>=250 && y<=420;
        private static void SuspendZoo(SoloSnapshot s)
        {
            if(s.zoo==null || !s.zoo.food.Any(f=>f.species!=""))return;
            foreach(var a in s.zoo.animals.Where(a=>a.owner!=""))ZooLayout.Routine(a);
            foreach(var f in s.zoo.food)f.Clear();s.revision++;
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
                if(!ZooLayout.Area(p.zone) || c.target!=(p.zone==ZooLayout.Entrance?ZooLayout.Savanna:ZooLayout.Entrance))return "unknown-zoo-gate";
                var gateX=p.zone==ZooLayout.Entrance?1200:200;
                if(Math.Abs(p.x-gateX)>200 || p.y>160)return "walk-to-zoo-gate";
                if(p.visit>=long.MaxValue-1)return "visit-limit";TravelPlayer(p,c.target);return null;
            }
            if(c.value=="return"){CancelZoo(p.id);return null;}
            if(p.zone!=ZooLayout.Savanna || !ZooLayout.Species.Contains(c.target))return "come-to-exhibit";
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
            foreach(var f in z.food.Where(f=>f.species!="").ToArray()){
                var p=state.players.Single(v=>v.id==f.actor);
                if(p.zone!=ZooLayout.Savanna || active!=null && !active.Contains(p.id) || f.offered && (Math.Abs(p.x-ZooLayout.SlotX(f.species,f.slot))>160 || p.y>160))visible|=CancelZoo(p.id);
            }
            foreach(var a in z.animals){
                a.age+=seconds;
                if(a.owner==""){
                    var next=z.food.Where(f=>f.species==a.species && f.offered).OrderBy(f=>f.ticket).FirstOrDefault();
                    if(next!=null){var point=ZooLayout.Point(a);a.owner=next.actor;a.consumed=false;ZooLayout.Segment(a,ZooPhase.Notice,point.X,point.Y,1.2);visible=true;continue;}
                }
                if(a.phase==ZooPhase.Eat && !a.consumed && a.age>=1.4){a.consumed=true;if(a.fed<int.MaxValue)a.fed++;visible=true;}
                if(a.age<a.duration)continue;
                if(a.phase==ZooPhase.Notice){var f=z.food.Single(v=>v.actor==a.owner);var point=ZooLayout.Point(a);var x=ZooLayout.SlotX(a.species,f.slot)-(a.species=="giraffe"?80:100);
                    var distance=Math.Sqrt((point.X-x)*(point.X-x)+(point.Y-270)*(point.Y-270));ZooLayout.Segment(a,ZooPhase.Approach,x,270,Math.Max(2,distance/42));}
                else if(a.phase==ZooPhase.Approach){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Eat,point.X,point.Y,4);}
                else {if(a.owner!="")z.food.Single(v=>v.actor==a.owner).Clear();ZooLayout.Routine(a);}
                visible=true;
            }
            return true;
        }
    }
}
