using System;
using System.Linq;
namespace LittleWeeps.Core
{
    [Serializable] public sealed class ZooFossilState
    {
        public int format=1;
        public long round=1,nextEpoch;
        public int[] revealed=new int[3];
        public bool[] placed=new bool[3];
        public string[] holders={"","",""},members={"","","",""};
        public long[] epochs=new long[4],serials=new long[4];
        public double[] brushAge={10,10,10,10};
        public double celebrationAge=10;
        public bool Complete=>placed.All(v=>v);
        public ZooFossilState Copy(){var f=(ZooFossilState)MemberwiseClone();f.revealed=(int[])revealed.Clone();f.placed=(bool[])placed.Clone();f.holders=(string[])holders.Clone();f.members=(string[])members.Clone();f.epochs=(long[])epochs.Clone();f.serials=(long[])serials.Clone();f.brushAge=(double[])brushAge.Clone();return f;}
    }
    public static class ZooFossils
    {
        // One side corner of the existing first exhibit; feeding starts at1050.
        public const float X=420,TrayY=580,BoardY=780;
        public const int RevealSteps=6;
        public static WalkPoint Spot(int slot)=>new WalkPoint(220+slot*145,45);
        public static bool Nearby(SoloPlayer p)=>p.zone==ZooCatalog.Dinosaurs && p.x>=160 && p.x<=880;
    }
    public sealed partial class GameWorld
    {
        private bool FossilHeld(string actor)=>state.zoo?.fossils?.holders.Contains(actor)==true;
        private bool LeaveFossils(string actor)
        {
            var f=state.zoo?.fossils;if(f==null)return false;var changed=false;
            for(var i=0;i<3;i++)if(f.holders[i]==actor){f.holders[i]="";changed=true;}
            for(var i=0;i<4;i++)if(f.members[i]==actor){f.members[i]="";f.epochs[i]=0;f.serials[i]=0;f.brushAge[i]=10;changed=true;}
            return changed;
        }
        private string FossilOperation(SoloCommand c,SoloPlayer p)
        {
            var f=state.zoo.fossils;
            var bits=(c.item??"").Split('/');
            if(bits.Length!=3 || !long.TryParse(bits[0],out var round) || round!=f.round || !long.TryParse(bits[1],out var epoch) || !long.TryParse(bits[2],out var serial))return "fossil-changed";
            if(!ZooFossils.Nearby(p))return "come-to-fossils";
            var member=Array.IndexOf(f.members,p.id);
            if(c.value=="fossil-join"){
                if(member>=0)return null;
                if(epoch!=0 || f.nextEpoch>=long.MaxValue-1)return "fossil-changed";
                member=Array.IndexOf(f.members,"");if(member<0)return "fossil-helping";
                // Food is never silently canceled. Finish/return it explicitly.
                var food=state.zoo.food.Single(v=>v.actor==p.id);
                if(food.species!="" || food.preparing || StoryHeld(p.id) || state.toys.Any(t=>t.holder==p.id))return "hands-full";
                f.members[member]=p.id;f.epochs[member]=++f.nextEpoch;return null;
            }
            if(member<0 || epoch!=f.epochs[member])return "fossil-changed";
            if(c.value=="fossil-leave"){LeaveFossils(p.id);return null;}
            if(c.value=="fossil-drop"){
                for(var i=0;i<3;i++)if(f.holders[i]==p.id)f.holders[i]="";return null;
            }
            if(c.value=="fossil-replay"){
                if(!f.Complete || f.holders.Any(h=>h!="") || f.celebrationAge<2.2)return "fossil-resting";
                if(f.round>=long.MaxValue-1)return "fossil-limit";
                // Atomic replacement plus expected round coalesces competing resets.
                state.zoo.fossils=new ZooFossilState{round=f.round+1,nextEpoch=f.nextEpoch,members=(string[])f.members.Clone(),epochs=(long[])f.epochs.Clone()};return null;
            }
            if(!int.TryParse(c.target,out var piece) || piece<0 || piece>=3)return "fossil-piece";
            if(c.value=="fossil-brush"){
                if(f.Complete || f.placed[piece] || f.revealed[piece]>=ZooFossils.RevealSteps)return "fossil-revealed";
                if(FossilHeld(p.id))return "hands-full";
                if(serial<=f.serials[member] || serial<1 || f.brushAge[member]<.16)return "fossil-gentle";
                f.serials[member]=serial;f.brushAge[member]=0;f.revealed[piece]++;return null;
            }
            if(c.value=="fossil-pickup"){
                if(f.placed[piece] || f.revealed[piece]<ZooFossils.RevealSteps || f.holders[piece]!="")return "fossil-helping";
                var food=state.zoo.food.Single(v=>v.actor==p.id);
                if(FossilHeld(p.id) || food.species!="" || food.preparing || state.toys.Any(t=>t.holder==p.id))return "hands-full";
                f.holders[piece]=p.id;return null;
            }
            if(c.value=="fossil-place"){
                if(f.holders[piece]!=p.id || f.placed[piece])return "fossil-match";
                f.holders[piece]="";f.placed[piece]=true;if(f.Complete)f.celebrationAge=0;return null;
            }
            return "unknown-fossil-action";
        }
        private static void NormalizeFossils(ZooState z)
        {
            if(z.fossils==null){z.fossils=new ZooFossilState();return;}
            var f=z.fossils;
            if(f.format!=1)throw new InvalidOperationException("Unsupported fossil save format.");
            if(f.revealed==null)f.revealed=new int[3];if(f.placed==null)f.placed=new bool[3];
            if(f.holders==null)f.holders=new string[]{"","",""};if(f.members==null)f.members=new string[]{"","","",""};
            if(f.epochs==null)f.epochs=new long[4];if(f.serials==null)f.serials=new long[4];if(f.brushAge==null)f.brushAge=new double[]{10,10,10,10};
        }
        private static void ValidateFossils(SoloSnapshot s)
        {
            var f=s.zoo.fossils;
            if(f==null || f.format!=1 || f.round<1 || f.round>=long.MaxValue || f.nextEpoch<0 || f.nextEpoch>=long.MaxValue ||
                f.revealed.Length!=3 || f.revealed.Any(n=>n<0 || n>6) || f.placed.Length!=3 || f.holders.Length!=3 || f.members.Length!=4 || f.epochs.Length!=4 || f.serials.Length!=4 || f.brushAge.Length!=4 ||
                f.members.Any(id=>id==null || id!="" && !s.players.Any(p=>p.id==id && ZooFossils.Nearby(p))) || f.members.Where(id=>id!="").Distinct().Count()!=f.members.Count(id=>id!="") ||
                f.holders.Any(id=>id==null || id!="" && !f.members.Contains(id)) || f.holders.Where(id=>id!="").Distinct().Count()!=f.holders.Count(id=>id!="") || !ZooClock(f.celebrationAge,10))throw new InvalidOperationException("Invalid fossil discovery.");
            for(var i=0;i<4;i++)if(f.epochs[i]<0 || f.epochs[i]>f.nextEpoch || (f.members[i]!="")!=(f.epochs[i]>0) || f.serials[i]<0 || !ZooClock(f.brushAge[i],10))throw new InvalidOperationException("Invalid fossil helper.");
            for(var i=0;i<3;i++)if((f.placed[i] || f.holders[i]!="") && f.revealed[i]!=6 || f.placed[i] && f.holders[i]!="")throw new InvalidOperationException("Invalid fossil piece.");
            foreach(var id in f.holders.Where(id=>id!=""))if(s.zoo.food.Any(v=>v.actor==id && (v.species!="" || v.preparing)) || s.toys.Any(t=>t.holder==id))throw new InvalidOperationException("Fossil hands full.");
        }
        private static void SuspendFossils(ZooState z)
        {
            var f=z.fossils;f.holders=new string[]{"","",""};f.members=new string[]{"","","",""};f.epochs=new long[4];f.serials=new long[4];f.brushAge=new double[]{10,10,10,10};f.celebrationAge=10;
        }
        private bool AdvanceFossils(double seconds,string[] active)
        {
            var f=state.zoo.fossils;var changed=false;f.celebrationAge=Math.Min(10,f.celebrationAge+seconds);
            for(var i=0;i<4;i++){
                f.brushAge[i]=Math.Min(10,f.brushAge[i]+seconds);var id=f.members[i];
                if(id!="" && (!ZooFossils.Nearby(state.players.Single(p=>p.id==id)) || active!=null && !active.Contains(id)))changed|=LeaveFossils(id);
            }
            return changed;
        }
    }
}
