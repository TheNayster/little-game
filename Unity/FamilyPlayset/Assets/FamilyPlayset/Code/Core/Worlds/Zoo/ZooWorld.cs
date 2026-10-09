using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ZooPhase { Rest, Wander, Browse, Drink, Notice, Approach, Eat, Greet, Curious, WaterWalk, Splash, WaterReturn, CareWalk, Care, CareFinish, LegacyStoryClue, LegacyStoryWalk, LegacyStoryReact, LegacyPropWalk, LegacyPropUse, LegacyPropExit }
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
        public string prepSpecies="";
        public long prepEpoch,edit;
        public int[] pieces=Array.Empty<int>();
        public ZooFood Copy(){var f=(ZooFood)MemberwiseClone();f.pieces=(int[])pieces.Clone();return f;}
        public void Clear(){species="";prepSpecies="";slot=-1;ticket=0;offered=false;preparing=false;prepEpoch=0;edit=0;pieces=Array.Empty<int>();}
    }
    [Serializable] public class ZooActivity
    {
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
        public ZooActivity CopyActivity(){var z=(ZooActivity)MemberwiseClone();CloneHints(z);return z;}
        protected void CloneHints(ZooActivity z){z.surpriseSequence=(int[])surpriseSequence.Clone();z.surpriseAge=(double[])surpriseAge.Clone();z.surpriseVariation=(int[])surpriseVariation.Clone();z.careMembers=(string[])careMembers.Clone();z.careProgress=(int[])careProgress.Clone();z.carePatch=(int[])carePatch.Clone();z.careMemberEpoch=(int[])careMemberEpoch.Clone();z.careBrushAge=(double[])careBrushAge.Clone();}
    }
    [Serializable] public sealed class ZooState : ZooActivity
    {
        public ZooAnimal[] animals;
        public ZooFood[] food;
        public long nextTicket;
        public long nextPrep;
        public ZooFossilState fossils=new ZooFossilState();
        public ZooActivity[] exhibits=Enumerable.Range(0,15).Select(_=>new ZooActivity()).ToArray();
        public ZooActivity Activity(string species){var i=Array.IndexOf(ZooLayout.Species,species);return i==0?this:exhibits[i-1];}
        public ZooState Copy(){var z=(ZooState)MemberwiseClone();CloneHints(z);z.fossils=fossils.Copy();z.animals=animals.Select(a=>a.Copy()).ToArray();z.food=food.Select(f=>f.Copy()).ToArray();z.exhibits=exhibits.Select(e=>e.CopyActivity()).ToArray();return z;}

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
        private sealed class ZooRuntime
        {
            public readonly System.Collections.Generic.HashSet<string> visitors=new System.Collections.Generic.HashSet<string>();
            public readonly System.Collections.Generic.Dictionary<string,double> pumps=new System.Collections.Generic.Dictionary<string,double>(),brushAt=new System.Collections.Generic.Dictionary<string,double>();
            public readonly System.Collections.Generic.Dictionary<string,long> brushSerial=new System.Collections.Generic.Dictionary<string,long>();
            public double clock;public float returnX,returnY;
        }
        private readonly System.Collections.Generic.Dictionary<string,ZooRuntime> zooRuntime=new System.Collections.Generic.Dictionary<string,ZooRuntime>();
        private ZooRuntime Runtime(string species){if(!zooRuntime.TryGetValue(species,out var r)){r=new ZooRuntime();zooRuntime.Add(species,r);}return r;}
        private static bool AtExhibit(SoloPlayer p,string species){var info=ZooCatalog.Get(species);return p.zone==info.area && p.x>=info.Center-1000 && p.x<info.Center+1000;}
        private static string PreparationSpecies(SoloPlayer p)=>ZooCatalog.All.FirstOrDefault(v=>AtExhibit(p,v.id))?.id??"";
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
                NormalizeFossils(s.zoo);
                if(s.zoo.exhibits==null || s.zoo.exhibits.Length==0)s.zoo.exhibits=Enumerable.Range(0,15).Select(_=>new ZooActivity()).ToArray();
                // The old story/habitat JSON fields are intentionally unbound.
                // Retain enum numbers for saves, but release their animal locks
                // before validation without touching feeding history or RNG.
                foreach(var a in s.zoo.animals??Array.Empty<ZooAnimal>()){
                    if(a==null || a.phase<ZooPhase.LegacyStoryClue || a.phase>ZooPhase.LegacyPropExit)continue;
                    var info=ZooCatalog.All.FirstOrDefault(v=>v.id==a.species);if(info==null)continue;
                    a.phase=ZooPhase.Rest;a.owner="";a.consumed=false;a.age=0;a.duration=3;
                    a.fromX=a.toX=Math.Max(info.MinX,Math.Min(info.MaxX,a.fromX));
                    a.fromY=a.toY=Math.Max(info.MinY,Math.Min(info.MaxY,a.fromY));
                }
                if(s.zoo.food!=null)foreach(var f in s.zoo.food)if(f!=null){if(f.pieces==null)f.pieces=Array.Empty<int>();if(f.preparing && string.IsNullOrEmpty(f.prepSpecies))f.prepSpecies="elephant";}
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
            ValidateFossils(s);
            if(z.exhibits==null || z.exhibits.Length!=15 || z.exhibits.Any(h=>h==null))throw new InvalidOperationException("Invalid Zoo exhibits.");
            foreach(var h in new ZooActivity[]{z}.Concat(z.exhibits)){
            if(h.surpriseSequence==null || h.surpriseSequence.Length!=2 || h.surpriseSequence.Any(n=>n<0) || h.surpriseAge==null || h.surpriseAge.Length!=2 || h.surpriseAge.Any(n=>!ZooClock(n,10)) || h.surpriseVariation==null || h.surpriseVariation.Length!=2 || h.surpriseVariation.Any(n=>n<0 || n>1))throw new InvalidOperationException("Invalid Zoo surprise hints.");
            if(h.waterSequence<0 || !ZooClock(h.waterAge,10) || !ZooClock(h.waterCooldown,ZooLayout.WaterCooldown) ||
                !ZooClock(h.waterPendingSeconds,ZooLayout.WaterExpiry) || !ZooClock(h.greetingCooldown,30) || !ZooClock(h.curiousCooldown,40) ||
                h.waterPending!=(h.waterPendingSeconds>0))throw new InvalidOperationException("Invalid elephant play hints.");
            if(h.careSession<0 || h.careMembers==null || h.careMembers.Length>4 || h.careMembers.Distinct().Count()!=h.careMembers.Length ||
                h.careMembers.Any(id=>!s.players.Any(p=>p.id==id)) || h.careProgress==null || h.careProgress.Length!=3 || h.careProgress.Any(n=>n<0 || n>4) || h.carePatch==null || h.carePatch.Length!=4 || h.carePatch.Any(n=>n<0 || n>2) || h.careMemberEpoch==null || h.careMemberEpoch.Length!=4 || h.nextCareEpoch<0 || h.careMemberEpoch.Any(n=>n<0 || n>h.nextCareEpoch) || h.careBrushAge==null || h.careBrushAge.Length!=4 || h.careBrushAge.Any(n=>!ZooClock(n,10)) ||
                !ZooClock(h.careSeconds,90) || h.careComplete && (h.careSession==0 || h.careProgress.Any(n=>n!=4)) ||
                h.careOrder<0 || h.waterOrder<0 || h.nextPlayOrder<0 || h.nextPlayOrder==long.MaxValue || h.careOrder>h.nextPlayOrder || h.waterOrder>h.nextPlayOrder)
                throw new InvalidOperationException("Invalid elephant care hints.");
            }
            foreach(var a in z.animals){var info=ZooCatalog.Get(a.species);
                if(a.random==0 || a.sequence<0 || a.fed<0 || a.previous< -1 || a.previous>3 || !Enum.IsDefined(typeof(ZooPhase),a.phase) ||
                    !HideAndSeek.Finite(a.age) || !HideAndSeek.Finite(a.duration) || a.age<0 || a.duration<.1 || a.duration>100 || a.age>a.duration+1 ||
                    !ZooPoint(a.fromX,a.fromY,info) || !ZooPoint(a.toX,a.toY,info) || a.owner==null || a.owner!="" && !s.players.Any(p=>p.id==a.owner) ||
                    (a.phase==ZooPhase.Notice || a.phase==ZooPhase.Approach || a.phase==ZooPhase.Eat)!=(a.owner!="") || a.consumed && a.phase!=ZooPhase.Eat ||
                    a.phase>=ZooPhase.LegacyStoryClue)
                    throw new InvalidOperationException("Invalid zoo animal.");
                if(a.owner!="" && !z.food.Any(f=>f.actor==a.owner && f.species==a.species && f.offered))throw new InvalidOperationException("Missing food offer.");}
            foreach(var f in z.food){var p=s.players.Single(v=>v.id==f.actor);
                var snackSpecies=f.preparing?f.prepSpecies:f.species;
                if(z.nextPrep<0 || z.nextPrep==long.MaxValue || f.pieces==null || f.pieces.Length>3 || f.pieces.Length>0 && (!ZooLayout.Species.Contains(snackSpecies) || f.pieces.Any(n=>!ZooCatalog.Get(snackSpecies).AcceptsSnack(n))) ||
                    f.prepEpoch<0 || f.prepEpoch>z.nextPrep || f.edit<0 || f.edit==long.MaxValue ||
                    f.preparing && (f.species!="" || f.prepEpoch==0 || !ZooLayout.Species.Contains(f.prepSpecies) || p.zone!=ZooCatalog.Get(f.prepSpecies).area) ||
                    f.pieces.Length>0 && !f.preparing && f.species=="" ||
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
            SuspendFossils(s.zoo);
            foreach(var a in s.zoo.animals.Where(a=>a.owner!="" || a.phase>=ZooPhase.Greet))ZooLayout.Routine(a);
            foreach(var f in s.zoo.food)f.Clear();
            foreach(var h in new ZooActivity[]{s.zoo}.Concat(s.zoo.exhibits)){
            h.surpriseSequence=new int[2];h.surpriseAge=new double[]{10,10};h.surpriseVariation=new int[2];
            h.waterSequence=0;h.waterAge=10;h.waterCooldown=0;h.waterPending=false;h.waterPendingSeconds=0;
            ClearElephantCare(h);h.careSession=0;h.nextCareEpoch=0;h.nextPlayOrder=0;h.waterOrder=0;
            h.greetingCooldown=30;h.curiousCooldown=14;
            }
            s.revision++;
        }
        private bool CancelZoo(string actor)
        {
            var f=state.zoo?.food.FirstOrDefault(v=>v.actor==actor);if(f==null || f.species=="" && !f.preparing)return false;
            foreach(var a in state.zoo.animals.Where(a=>a.owner==actor))ZooLayout.Routine(a);f.Clear();return true;
        }
        public bool ReleaseZoo(string actor){
            var changed=CancelZoo(actor)|LeaveFossils(actor);var z=state.zoo;if(z==null)return changed;
            foreach(var animal in z.animals){var id=animal.species;var h=z.Activity(id);var r=Runtime(id);
                changed|=h.careMembers.Contains(actor);LeaveElephantCare(actor,id);r.visitors.Remove(actor);r.pumps.Remove(actor);r.brushAt.Remove(actor);r.brushSerial.Remove(actor);
                if(r.visitors.Count==0 && h.careMembers.Length==0){changed|=h.waterPending || animal.phase>=ZooPhase.Greet;h.waterPending=false;h.waterPendingSeconds=0;if(animal.phase>=ZooPhase.Greet)ZooLayout.Routine(animal);}
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
            if(c.value.StartsWith("fossil-",StringComparison.Ordinal))return FossilOperation(c,p);
            // Reject stale clients/receipts; no removed activity can acquire a lock.
            if(c.value.StartsWith("habitat-",StringComparison.Ordinal) || c.value.StartsWith("story-",StringComparison.Ordinal))return "activity-removed";
            if(c.value=="return"){CancelZoo(p.id);LeaveElephantCare(p.id);LeaveFossils(p.id);return null;}
            if(c.value=="surprise"){
                var parts=(c.target??"").Split('/');var species=c.target==ZooLayout.BirdSurprise || c.target==ZooLayout.ButterflySurprise?"elephant":parts[0];
                var i=c.target==ZooLayout.BirdSurprise?0:c.target==ZooLayout.ButterflySurprise?1:parts.Length==2 && int.TryParse(parts[1],out var slot)?slot:-1;
                if(!ZooLayout.Species.Contains(species) || i<0 || i>1)return "unknown-zoo-surprise";
                if(!AtExhibit(p,species))return "come-to-exhibit";var h=z.Activity(species);
                if(h.surpriseAge[i]<ZooLayout.SurpriseReset)return "surprise-resting";
                if(h.surpriseSequence[i]==int.MaxValue)return "surprise-limit";
                h.surpriseSequence[i]++;h.surpriseVariation[i]=h.surpriseSequence[i]%2;h.surpriseAge[i]=0;
                // Feeding/care retain their locks; a free animal acknowledges a
                // discovery with its own intact greeting drawing.
                var animal=z.animals.First(a=>a.species==species);
                if(species!="elephant" && animal.owner=="" && animal.phase<ZooPhase.Greet){var point=ZooLayout.Point(animal);ZooLayout.Segment(animal,ZooPhase.Greet,point.X,point.Y,2.2);}
                return null;
            }
            if(!ZooLayout.Species.Contains(c.target) || p.zone!=ZooCatalog.Get(c.target).area)return "come-to-exhibit";
            if(c.value=="care" || c.value=="brush" || c.value=="put-brush")return ElephantCareOperation(c,p);
            if(c.value=="water"){
                if(!AtExhibit(p,c.target))return "come-to-exhibit";var h=z.Activity(c.target);var r=Runtime(c.target);
                if(h.waterCooldown>0 || r.pumps.TryGetValue(p.id,out var nextPump) && nextPump>r.clock)return "water-resting";
                if(h.waterSequence==int.MaxValue || h.nextPlayOrder>=long.MaxValue-1)return "water-limit";
                h.waterSequence++;h.waterAge=0;h.waterCooldown=ZooLayout.WaterCooldown;r.pumps[p.id]=r.clock+2.4;
                var animal=z.animals.First(v=>v.species==c.target);
                if(animal.phase!=ZooPhase.WaterWalk && animal.phase!=ZooPhase.Splash){if(!h.waterPending)h.waterOrder=++h.nextPlayOrder;h.waterPending=true;h.waterPendingSeconds=ZooLayout.WaterExpiry;}
                return null;
            }
            var food=z.food.Single(f=>f.actor==p.id);
            if(c.value.StartsWith("snack-",StringComparison.Ordinal))return ElephantSnackOperation(c,p,food);
            if(c.value=="take"){
                if(FossilHeld(p.id) || food.species!="" || food.preparing || state.toys.Any(t=>t.holder==p.id))return "hands-full";
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
            visible|=AdvanceFossils(seconds,active);
            foreach(var f in z.food.Where(f=>f.preparing).ToArray()){
                var p=state.players.Single(v=>v.id==f.actor);
                if(!AtExhibit(p,f.prepSpecies) || active!=null && !active.Contains(p.id))visible|=CancelZoo(p.id);
            }
            foreach(var f in z.food.Where(f=>f.species!="").ToArray()){
                var p=state.players.Single(v=>v.id==f.actor);
                if(p.zone!=ZooCatalog.Get(f.species).area || active!=null && !active.Contains(p.id) || f.offered && (Math.Abs(p.x-ZooLayout.SlotX(f.species,f.slot))>160 || p.y>160))visible|=CancelZoo(p.id);
            }
            foreach(var a in z.animals){var h=z.Activity(a.species);var r=Runtime(a.species);var info=ZooCatalog.Get(a.species);
            for(var i=0;i<2;i++)h.surpriseAge[i]=Math.Min(10,h.surpriseAge[i]+seconds);
            r.clock+=seconds;for(var i=0;i<4;i++)h.careBrushAge[i]=Math.Min(10,h.careBrushAge[i]+seconds);h.waterAge=Math.Min(10,h.waterAge+seconds);h.waterCooldown=Math.Max(0,h.waterCooldown-seconds);
            h.greetingCooldown=Math.Max(0,h.greetingCooldown-seconds);h.curiousCooldown=Math.Max(0,h.curiousCooldown-seconds);
            var arrived=false;var nearby=false;
            foreach(var id in h.careMembers.ToArray()){
                var player=state.players.Single(v=>v.id==id);
                if(!AtExhibit(player,a.species) || active!=null && !active.Contains(id)) {LeaveElephantCare(id,a.species);visible=true;}
            }
            if(h.careMembers.Length>0){h.careSeconds=Math.Max(0,h.careSeconds-seconds);if(h.careSeconds==0){ClearElephantCare(h);AmbientChanged=true;}}
            foreach(var p in state.players){
                var present=AtExhibit(p,a.species) && (active==null || active.Contains(p.id));
                if(present){nearby=true;if(r.visitors.Add(p.id))arrived=true;}else r.visitors.Remove(p.id);
            }
            if(h.waterPending){h.waterPendingSeconds=nearby?Math.Max(0,h.waterPendingSeconds-seconds):0;if(h.waterPendingSeconds==0){h.waterPending=false;AmbientChanged=true;}}

                if((a.phase>=ZooPhase.CareWalk && a.phase<=ZooPhase.CareFinish && h.careMembers.Length==0 || !nearby && a.phase>=ZooPhase.Greet)){ZooLayout.Routine(a);AmbientChanged=true;}
                if(a.owner=="" && active!=null && !state.players.Any(p=>p.zone==ZooCatalog.Get(a.species).area && active.Contains(p.id)))continue;
                a.age+=seconds;
                if(a.owner==""){
                    var next=z.food.Where(f=>f.species==a.species && f.offered).OrderBy(f=>f.ticket).FirstOrDefault();
                    // Splash is the only atomic play transition; at most .6s
                    // remains before food wins. Walking/greetings yield now.
                    if(next!=null && !(a.phase==ZooPhase.Splash && a.age<ZooLayout.SplashFeedDelay || (a.phase==ZooPhase.Care || a.phase==ZooPhase.CareFinish) && a.age<ZooLayout.CareFeedDelay)){
                        var point=ZooLayout.Point(a);a.owner=next.actor;a.consumed=false;ZooLayout.Segment(a,ZooPhase.Notice,point.X,point.Y,1.2);visible=true;continue;}
                    if(nearby && next==null && a.phase<ZooPhase.Greet){
                        var point=ZooLayout.Point(a);
                        if(h.careMembers.Length>0 && (!h.careComplete || !h.careReacted) && (!h.waterPending || h.careOrder<h.waterOrder)){
                            ZooLayout.Segment(a,ZooPhase.CareWalk,info.Center,info.ActivityY,WaterWalkTime(point,info.Center,info.ActivityY));AmbientChanged=true;continue;
                        }
                        if(h.waterPending){
                            h.waterPending=false;h.waterPendingSeconds=0;r.returnX=point.X;r.returnY=point.Y;
                            ZooLayout.Segment(a,ZooPhase.WaterWalk,info.PlayX,info.ActivityY,WaterWalkTime(point,info.PlayX,info.ActivityY));AmbientChanged=true;continue;
                        }
                        if(arrived && h.greetingCooldown==0){ZooLayout.Segment(a,ZooPhase.Greet,point.X,point.Y,2.2);h.greetingCooldown=30;AmbientChanged=true;continue;}
                        if(h.curiousCooldown==0){ZooLayout.Segment(a,ZooPhase.Curious,point.X,point.Y,3.2);h.curiousCooldown=20+ZooLayout.Roll(a)*18;AmbientChanged=true;continue;}
                    }
                }
                if(a.phase==ZooPhase.Eat && !a.consumed && a.age>=1.4){a.consumed=true;if(a.fed<int.MaxValue)a.fed++;visible=true;}
                if(a.phase==ZooPhase.Care && h.careMembers.Length>0){
                    if(h.careComplete){h.careReacted=true;var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.CareFinish,point.X,point.Y,2);AmbientChanged=true;}
                    else a.age=Math.Min(a.age,a.duration);
                    continue;
                }
                if(a.age<a.duration)continue;
                var feeding=a.owner!="";
                if(a.phase==ZooPhase.Notice){var f=z.food.Single(v=>v.actor==a.owner);var point=ZooLayout.Point(a);var x=ZooLayout.SlotX(a.species,f.slot)+65-info.mouthX;
                    var distance=Math.Sqrt((point.X-x)*(point.X-x)+(point.Y-info.FeedY)*(point.Y-info.FeedY));ZooLayout.Segment(a,ZooPhase.Approach,x,info.FeedY,Math.Max(1.5,distance/(info.approachSpeed*1.4f)));}
                else if(a.phase==ZooPhase.Approach){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Eat,point.X,point.Y,4);}
                else if(a.phase==ZooPhase.CareWalk){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Care,point.X,point.Y,90);}
                else if(a.phase==ZooPhase.WaterWalk){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.Splash,point.X,point.Y,1.8);}
                else if(a.phase==ZooPhase.Splash){var point=ZooLayout.Point(a);ZooLayout.Segment(a,ZooPhase.WaterReturn,r.returnX,r.returnY,WaterWalkTime(point,r.returnX,r.returnY));}
                else {if(a.owner!="")z.food.Single(v=>v.actor==a.owner).Clear();ZooLayout.Routine(a);}
                // Ambient routes do not conflict with a child's food/travel
                // transaction. Feeding lease transitions still advance revision.
                if(feeding)visible=true;else AmbientChanged=true;
            }
            return true;
        }
        private static void ClearElephantCare(ZooActivity z){z.careMembers=Array.Empty<string>();z.careProgress=new int[3];z.carePatch=new int[4];z.careMemberEpoch=new int[4];z.careBrushAge=new double[]{10,10,10,10};z.careComplete=false;z.careReacted=false;z.careSeconds=0;z.careOrder=0;}
        private void LeaveElephantCare(string actor){if(state.zoo!=null)foreach(var a in state.zoo.animals)LeaveElephantCare(actor,a.species);}
        private void LeaveElephantCare(string actor,string species){if(state.zoo==null)return;var z=state.zoo.Activity(species);var animal=state.zoo.animals.First(a=>a.species==species);var old=z.careMembers;var patches=z.carePatch;var ages=z.careBrushAge;var epochs=z.careMemberEpoch;z.careMembers=old.Where(id=>id!=actor).ToArray();z.carePatch=new int[4];z.careMemberEpoch=new int[4];z.careBrushAge=new double[]{10,10,10,10};for(var i=0;i<z.careMembers.Length;i++){z.carePatch[i]=patches[Array.IndexOf(old,z.careMembers[i])];z.careBrushAge[i]=ages[Array.IndexOf(old,z.careMembers[i])];z.careMemberEpoch[i]=epochs[Array.IndexOf(old,z.careMembers[i])];}if(z.careMembers.Length==0){ClearElephantCare(z);if(animal.phase>=ZooPhase.CareWalk && animal.phase<=ZooPhase.CareFinish)ZooLayout.Routine(animal);}}
        private string ElephantCareOperation(SoloCommand c,SoloPlayer p)
        {
            var z=state.zoo.Activity(c.target);var r=Runtime(c.target);var animal=state.zoo.animals.First(a=>a.species==c.target);
            if(!AtExhibit(p,c.target))return "come-to-exhibit";
            if(c.value=="put-brush"){LeaveElephantCare(p.id,c.target);return null;}
            if(c.value=="care"){
                if(state.zoo.food.Any(f=>f.actor==p.id && (f.species!="" || f.preparing)) || state.toys.Any(t=>t.holder==p.id) || FossilHeld(p.id))return "hands-full";
                foreach(var other in state.zoo.animals.Where(v=>v.species!=c.target))LeaveElephantCare(p.id,other.species);
                if(z.careMembers.Contains(p.id) && !z.careComplete)return null;
                if(z.careComplete && animal.phase==ZooPhase.CareFinish)return "care-finishing";
                if(z.careMembers.Length==0 || z.careComplete){
                    if(z.careSession==int.MaxValue || z.nextPlayOrder>=long.MaxValue-1)return "care-limit";
                    z.careSession++;z.careProgress=new int[3];z.carePatch=new int[4];z.careMemberEpoch=new int[4];z.careBrushAge=new double[]{10,10,10,10};z.careComplete=false;z.careReacted=false;z.careOrder=++z.nextPlayOrder;
                    r.brushSerial.Clear();r.brushAt.Clear();
                }
                if(!z.careMembers.Contains(p.id)){
                    if(z.nextCareEpoch==int.MaxValue)return "care-limit";
                    var index=z.careMembers.Length;z.careMembers=z.careMembers.Concat(new[]{p.id}).ToArray();z.careMemberEpoch[index]=++z.nextCareEpoch;
                    r.brushSerial.Remove(p.id);r.brushAt.Remove(p.id);
                }
                z.careSeconds=90;return null;
            }
            // Session + monotonic gesture serial rejects old/reordered/repeated
            // input without retaining an unbounded gesture/message history.
            var parts=(c.item??"").Split('/');
            var member=Array.IndexOf(z.careMembers,p.id);
            if(parts.Length!=4 || member<0 || !int.TryParse(parts[1],out var epoch) || epoch!=z.careMemberEpoch[member] || !int.TryParse(parts[0],out var session) || session!=z.careSession ||
                !int.TryParse(parts[2],out var patch) || patch<0 || patch>=3 || !long.TryParse(parts[3],out var serial) || serial<=0 ||
                !z.careMembers.Contains(p.id) || z.careComplete || animal.phase!=ZooPhase.Care)return "care-unavailable";
            if(r.brushSerial.TryGetValue(p.id,out var last) && serial<=last)return "care-repeat";
            r.brushSerial[p.id]=serial;
            if(r.brushAt.TryGetValue(p.id,out var next) && next>r.clock)return "gentle-brush";
            r.brushAt[p.id]=r.clock+.35;
            if(z.careProgress[patch]>=4)return "patch-clear";
            z.carePatch[Array.IndexOf(z.careMembers,p.id)]=patch;z.careBrushAge[Array.IndexOf(z.careMembers,p.id)]=0;
            z.careProgress[patch]=Math.Min(4,z.careProgress[patch]+1);z.careSeconds=90;
            z.careComplete=z.careProgress.All(n=>n==4);return null;
        }
        private static double WaterWalkTime(WalkPoint from,float x,float y)=>Math.Max(.5,Math.Sqrt((from.X-x)*(from.X-x)+(from.Y-y)*(from.Y-y))/80);
    }
}
