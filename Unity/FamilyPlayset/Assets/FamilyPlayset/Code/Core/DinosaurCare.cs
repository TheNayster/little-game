using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum DinosaurCarePhase { None, Held, Queued, Approach, Eat, Pet, Done }
    [Serializable] public sealed class DinosaurCare
    {
        public string actor,species="",kind="";
        public int slot=-1;
        public long ticket;
        public float x,y=100;
        public DinosaurCarePhase phase;
        public double started;
        public bool consumed;
        public DinosaurCare Copy()=>(DinosaurCare)MemberwiseClone();
        public void Clear(){species="";kind="";slot=-1;ticket=0;phase=DinosaurCarePhase.None;started=0;consumed=false;x=0;y=100;}
    }
    public static class DinosaurCareRules
    {
        public const int Schema=37;
        public static float BucketX(string species)=>Math.Min(4400,900+Array.IndexOf(DinosaurRides.Species,species)*1200);
        // Contact sockets are fitted to the existing right-facing idle artwork.
        public static float MouthX(string species)=>species=="brachiosaurus"?235:species=="tyrannosaurus"?210:195;
        public static float MouthHeight(string species)=>species=="brachiosaurus"?375:species=="tyrannosaurus"?205:species=="triceratops"?100:215;
        public static float PetX(string species)=>species=="brachiosaurus"?110:65;
        public static bool Active(DinosaurCare f)=>f.phase==DinosaurCarePhase.Approach || f.phase==DinosaurCarePhase.Eat || f.phase==DinosaurCarePhase.Pet;
        public static float TargetY(DinosaurCare f)=>Math.Max(70,Math.Min(230,f.y+50));
        public static float TargetX(DinosaurCare f)=>f.kind=="feed"?f.x+60-MouthX(f.species):f.x-PetX(f.species);
    }
    public sealed partial class SoloWorld
    {
        private static void ValidateDinosaurCare(SoloSnapshot s)
        {
            var d=s.dinosaurWorld;
            if(s.schema<DinosaurCareRules.Schema){if(d.care!=null && d.care.Length>0 || d.nextCareTicket!=0 || d.animals.Any(a=>a.fed!=0 || a.petted!=0))throw new InvalidOperationException("Dinosaur care requires schema 37.");return;}
            if(d.care==null || !d.care.Select(f=>f?.actor).OrderBy(x=>x).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x)) || d.nextCareTicket<0 || d.nextCareTicket==long.MaxValue)throw new InvalidOperationException("Invalid dinosaur care.");
            foreach(var f in d.care){
                if(f.species==null || f.kind==null || !Enum.IsDefined(typeof(DinosaurCarePhase),f.phase) || !HideAndSeek.Finite(f.started) || f.started<0 || f.started>d.clock)throw new InvalidOperationException("Invalid care record.");
                if(f.phase==DinosaurCarePhase.None){if(f.species!="" || f.kind!="" || f.slot!=-1 || f.ticket!=0 || f.consumed)throw new InvalidOperationException("Invalid empty care.");continue;}
                var p=s.players.Single(v=>v.id==f.actor);
                if(!DinosaurRides.Species.Contains(f.species) || (f.kind!="feed" && f.kind!="pet") || f.slot<0 || f.slot>3 || f.ticket<=0 || f.ticket>d.nextCareTicket || !DinosaurRides.Point(f.x,f.y) || !DinosaurRides.Point(DinosaurCareRules.TargetX(f),DinosaurCareRules.TargetY(f)) || p.zone!=DinosaurRides.Area || p.fixture!="" || f.kind=="pet" && (f.phase==DinosaurCarePhase.Held || f.phase==DinosaurCarePhase.Eat) || f.kind=="feed" && f.phase==DinosaurCarePhase.Pet || f.consumed && f.phase!=DinosaurCarePhase.Eat && f.phase!=DinosaurCarePhase.Pet && f.phase!=DinosaurCarePhase.Done)throw new InvalidOperationException("Invalid care lease.");
            }
            var leases=d.care.Where(f=>f.phase!=DinosaurCarePhase.None);
            if(leases.GroupBy(f=>f.ticket).Any(g=>g.Count()>1) || leases.GroupBy(f=>f.species+"/"+f.slot).Any(g=>g.Count()>1) || leases.Where(DinosaurCareRules.Active).GroupBy(f=>f.species).Any(g=>g.Count()>1) || leases.Any(f=>s.players.Any(p=>p.fixture==DinosaurRides.Fixture(f.species))))throw new InvalidOperationException("Duplicate dinosaur care or ridden care.");
        }
        private static void SuspendDinosaurCare(SoloSnapshot s)
        {
            if(s.dinosaurWorld?.care==null || !s.dinosaurWorld.care.Any(f=>f.phase!=DinosaurCarePhase.None))return;
            foreach(var f in s.dinosaurWorld.care)f.Clear();
            foreach(var a in s.dinosaurWorld.animals){a.wandering=false;a.pause=4;}
            s.revision++;
        }
        private bool CancelDinosaurCare(string actor)
        {
            var f=state.dinosaurWorld?.care?.FirstOrDefault(v=>v.actor==actor);if(f==null || f.phase==DinosaurCarePhase.None)return false;
            if(DinosaurCareRules.Active(f)){var a=state.dinosaurWorld.animals.Single(v=>v.species==f.species);a.wandering=false;a.pause=4;}
            f.Clear();return true;
        }
        public bool ReleaseDinosaurCare(string actor){if(!CancelDinosaurCare(actor))return false;state.revision++;return true;}
        private string DinosaurCareOperation(SoloCommand c,SoloPlayer p)
        {
            var d=state.dinosaurWorld;if(d.care==null)return "dinosaur-care-upgrade-needed";
            var f=d.care.Single(v=>v.actor==p.id);
            if(c.value=="cancel-care"){CancelDinosaurCare(p.id);return null;}
            if(!DinosaurRides.Species.Contains(c.target))return "unknown-dinosaur";
            if(f.phase!=DinosaurCarePhase.None && c.value!="offer")return null; // Repeated taps retain exactly one portion/turn.
            if(p.fixture!="")return "get-off-first";
            if(c.value=="offer"){
                if(f.species!=c.target || f.kind!="feed")return "take-food-first";
                if(f.phase!=DinosaurCarePhase.Held)return null;
                if(Math.Abs(p.x-f.x)>65 || Math.Abs(p.y-f.y)>35)return "walk-to-care-spot";
                f.phase=DinosaurCarePhase.Queued;f.started=d.clock;return null;
            }
            if(state.players.Any(v=>v.fixture==DinosaurRides.Fixture(c.target)))return "dinosaur-being-ridden";
            if(state.toys.Any(t=>t.holder==p.id))return "put-down-toy";
            var a=d.animals.Single(v=>v.species==c.target);
            if(c.value=="take" && Math.Abs(p.x-DinosaurCareRules.BucketX(c.target))>160)return "walk-to-food-bucket";
            if(c.value=="pet" && (p.x<510 || p.x>4400 || Math.Abs(p.x-a.x)>330 || Math.Abs(p.y-a.y)>160))return "come-closer";
            var slot=Enumerable.Range(0,4).FirstOrDefault(i=>!d.care.Any(v=>v.species==c.target && v.slot==i));
            if(d.care.Any(v=>v.species==c.target && v.slot==slot))return "care-spots-busy";
            if(d.nextCareTicket>=long.MaxValue-1)return "care-limit";
            f.species=c.target;f.kind=c.value=="pet"?"pet":"feed";f.slot=slot;f.ticket=++d.nextCareTicket;f.y=100;
            var prior=d.care.FirstOrDefault(v=>v.actor!=p.id && v.species==c.target && v.kind=="feed" && v.phase!=DinosaurCarePhase.Done);
            var center=prior!=null?prior.x-(prior.slot-1.5f)*75:Math.Max(622.5f,Math.Min(4287.5f,a.x+DinosaurCareRules.MouthX(c.target)-60));
            if(prior==null && Math.Abs(center-DinosaurCareRules.BucketX(c.target))<185)center=Math.Max(622.5f,Math.Min(4287.5f,DinosaurCareRules.BucketX(c.target)-260));
            f.x=center+(slot-1.5f)*75;
            // Freeze wandering while children walk to their reserved contact spots.
            a.wandering=false;a.pause=4;
            f.phase=DinosaurCarePhase.Held;f.started=d.clock;
            if(c.value=="pet"){f.phase=DinosaurCarePhase.Queued;f.x=p.x;f.y=p.y;}
            return null;
        }
        private bool AdvanceDinosaurCare(double seconds,string[] active)
        {
            var d=state.dinosaurWorld;if(d.care==null)return false;var changed=false;
            foreach(var f in d.care.Where(f=>f.phase!=DinosaurCarePhase.None).ToArray()){
                var p=state.players.Single(v=>v.id==f.actor);
                if(p.zone!=DinosaurRides.Area || p.fixture!="" || active!=null && !active.Contains(p.id) || f.phase!=DinosaurCarePhase.Held && (Math.Abs(p.x-f.x)>160 || Math.Abs(p.y-f.y)>100) || f.phase==DinosaurCarePhase.Held && d.clock-f.started>60)changed|=CancelDinosaurCare(p.id);
                else if(f.phase==DinosaurCarePhase.Done && d.clock-f.started>=1.5){f.Clear();changed=true;}
            }
            return changed;
        }
        private bool AdvanceCaredDinosaur(DinosaurMount a,double seconds,ref bool visible)
        {
            var d=state.dinosaurWorld;if(d.care==null)return false;
            var f=d.care.FirstOrDefault(v=>v.species==a.species && DinosaurCareRules.Active(v));
            if(f==null){f=d.care.Where(v=>v.species==a.species && v.phase==DinosaurCarePhase.Queued).OrderBy(v=>v.ticket).FirstOrDefault();
                if(f!=null){f.phase=DinosaurCarePhase.Approach;f.started=d.clock;visible=true;}}
            if(f==null)return d.care.Any(v=>v.species==a.species && v.phase!=DinosaurCarePhase.Done); // Held food keeps the approach socket stable.
            a.left=false;a.wandering=false;
            if(f.phase==DinosaurCarePhase.Approach){
                var x=DinosaurCareRules.TargetX(f);var y=DinosaurCareRules.TargetY(f);var dx=x-a.x;var dy=y-a.y;var length=Math.Sqrt(dx*dx+dy*dy);var step=Math.Min(length,seconds*140);
                if(length>0){a.x+=(float)(dx/length*step);a.y+=(float)(dy/length*step);a.left=dx<0;}
                a.targetX=a.x;a.targetY=a.y;
                if(length<=step+.01){a.x=x;a.y=y;a.left=false;f.phase=f.kind=="feed"?DinosaurCarePhase.Eat:DinosaurCarePhase.Pet;f.started=d.clock;visible=true;}
            }else{
                var age=d.clock-f.started;
                if(!f.consumed && age>=1.2){f.consumed=true;if(f.kind=="feed"){if(a.fed<int.MaxValue)a.fed++;}else if(a.petted<int.MaxValue)a.petted++;visible=true;}
                if(age>=3){f.phase=DinosaurCarePhase.Done;f.started=d.clock;a.pause=4;visible=true;}
            }
            return true;
        }
    }
}
