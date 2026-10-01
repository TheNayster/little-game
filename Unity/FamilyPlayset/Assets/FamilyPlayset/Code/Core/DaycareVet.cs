using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class VetMember
    {
        public string actor,gesture="",tool=""; public bool attending,declined;
        public int patient=-1; public double last=-1; public float x,y;
        public VetMember Copy()=>(VetMember)MemberwiseClone();
    }
    [Serializable] public sealed class VetPatient
    {
        public int bed=-1,needs,cuddles; public int[] washed,brushed; public bool bandaged;
        public double arrived,reaction; public int reactions;
        public VetPatient Copy()=>new VetPatient{bed=bed,needs=needs,cuddles=cuddles,washed=washed.ToArray(),brushed=brushed.ToArray(),bandaged=bandaged,arrived=arrived,reaction=reaction,reactions=reactions};
    }
    [Serializable] public sealed class VetState
    {
        public int round; public double clock; public VetMember[] members; public VetPatient[] patients; public string[] friends;
        public VetState Copy()=>new VetState{round=round,clock=clock,members=members.Select(m=>m.Copy()).ToArray(),patients=patients.Select(p=>p.Copy()).ToArray(),friends=friends.ToArray()};
    }
    public static class DaycareVet
    {
        public const int Schema=48; public const string Zone="daycare-vet";
        public static readonly string[] Pets={"puppy","kitten","rabbit","guinea pig"};
        public static string Species(int i)=>i<4?Pets[i]:DinosaurRides.Species[i-4];
        public static string Name(int i)=>new[]{"Sunny","Pebble","Clover","Pip","Rex","Tilly","Fern","Poppy"}[i];
        public static string Kind(int i)=>i<4?Pets[i]:new[]{"little T. rex","little triceratops","little brachiosaurus","little parasaurolophus"}[i-4];
        public static bool Needs(VetPatient p,int tool)=>(p.needs&(1<<tool))!=0;
        public static bool Done(VetPatient p,int tool)=>!Needs(p,tool) || (tool==0?p.washed.All(n=>n==3):tool==1?p.brushed.All(n=>n==3):tool==2?p.bandaged:p.cuddles==3);
        public static bool Ready(VetPatient p)=>Enumerable.Range(0,4).All(t=>Done(p,t));
        // Normalized interaction landmarks are shared by rendering and authority.
        // They sit on each animal's body, rather than on its transparent margins.
        public static WalkPoint Spot(int patient,int spot)
        {
            var cx=patient==6?.58f:patient>=4?.55f:.47f; var cy=patient==6?.35f:patient>=4?.25f:.42f;
            return new WalkPoint(cx+(spot==0?-.12f:spot==1?.12f:0),cy+(spot==0?.06f:spot==1?.03f:-.12f));
        }
        public static WalkPoint Patch(int patient)=>Spot(patient,2);
        public static string Visit(int i)=>new[]{"Sunny splashed in a muddy puddle.","Pebble explored a prickly hedge.","Clover has tangled fur after playing outside.","Pip rolled through the garden.","Rex tumbled while chasing a ball.","Tilly played in a dusty sandpit.","Fern brushed past a prickly bush.","Poppy had a muddy garden adventure."}[i];
        public static int NextTool(VetPatient p)=>Enumerable.Range(0,4).FirstOrDefault(t=>!Done(p,t));
        public static string Hint(int i,VetPatient p,int tool)
        {
            if(Ready(p))return Name(i)+" feels lovely! Send them home and welcome another friend.";
            if(!Needs(p,tool))return Name(i)+" doesn't need that. Choose a care picture above.";
            if(Done(p,tool))return "Lovely work! Choose a care picture without a tick.";
            return tool==0?"Rub the sponge over the muddy spots. Watch them wash away.":tool==1?(i<4?"Gently brush the tufts of tangled fur.":"Gently brush away the dusty patches."):tool==2?!Done(p,0)?"Wash the muddy spots before covering the little scratch.":"Drag a bandage onto the little peach scratch.":"Stroke or tap your friend gently to give three cuddles.";
        }
    }
    public sealed partial class SoloWorld
    {
        public VetState ReadVet()=>state.vet?.Copy();
        private static VetState NewVet(SoloSnapshot s)=>new VetState{members=s.players.Select(p=>new VetMember{actor=p.id}).ToArray(),patients=VetPatients(),friends=DaycareNpcCasts.Pick(2)};
        private static VetPatient[] VetPatients()=>Enumerable.Range(0,8).Select(i=>new VetPatient{needs=new[]{11,13,14,15,13,11,14,15}[i],washed=new int[3],brushed=new int[3]}).ToArray();
        private static void NormalizeVet(SoloSnapshot s){if(s!=null && s.schema<DaycareVet.Schema && s.vet!=null && s.vet.round==0 && (s.vet.members==null || s.vet.members.Length==0))s.vet=null;}
        private static void ValidateVet(SoloSnapshot s)
        {
            var g=s.vet;if(g==null && s.schema<DaycareVet.Schema)return;
            if(g==null || g.round<0 || g.round>=int.MaxValue || !KeepyRules.Finite(g.clock) || g.clock<0 || !DaycareNpcCasts.Valid(g.friends,2) || g.members==null || !g.members.Select(m=>m?.actor).OrderBy(x=>x).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x)) || g.patients==null || g.patients.Length!=8)throw new InvalidOperationException("Invalid animal clinic.");
            if(g.patients.Where(p=>p!=null && p.bed>=0).Select(p=>p.bed).Distinct().Count()!=g.patients.Count(p=>p!=null && p.bed>=0))throw new InvalidOperationException("Two clinic patients in one bed.");
            foreach(var p in g.patients)if(p==null || p.bed< -2 || p.bed>3 || p.needs<8 || p.needs>15 || p.washed==null || p.brushed==null || p.washed.Length!=3 || p.brushed.Length!=3 || p.washed.Concat(p.brushed).Any(n=>n<0 || n>3) || p.cuddles<0 || p.cuddles>3 || p.bed== -2 && !DaycareVet.Ready(p) || !KeepyRules.Finite(p.arrived) || p.arrived<0 || p.arrived>g.clock || !KeepyRules.Finite(p.reaction) || p.reaction<0 || p.reaction>g.clock || p.reactions<0)throw new InvalidOperationException("Invalid patient care.");
            foreach(var m in g.members)if(m==null || m.gesture==null || m.gesture.Length>128 || m.tool==null || m.patient< -1 || m.patient>7 || !KeepyRules.Finite(m.last) || m.last< -1 || m.last>g.clock || !KeepyRules.Finite(m.x) || !KeepyRules.Finite(m.y) || m.x<0 || m.x>1 || m.y<0 || m.y>1 || m.attending && (g.round==0 || s.players.Single(p=>p.id==m.actor).zone!=DaycareVet.Zone))throw new InvalidOperationException("Invalid clinic helper.");
        }
        public bool ReleaseVet(string actor){var m=state.vet?.members.FirstOrDefault(v=>v.actor==actor);if(m==null || !m.attending)return false;m.attending=false;m.gesture="";state.revision++;return true;}
        public bool JoinVetGroup(string[] connected)
        {
            var g=state.vet;if(g==null || g.round==0 || !g.members.Any(m=>m.attending && connected.Contains(m.actor)) && !state.players.Any(p=>p.zone==DaycareVet.Zone && connected.Contains(p.id)))return false;var changed=false;
            foreach(var id in connected){var p=state.players.Single(v=>v.id==id);var m=g.members.Single(v=>v.actor==id);if(m.attending || m.declined || DaycarePlay.Member(state.hideClub,id)?.invited==true || DaycarePlay.Member(state.tagClub,id)?.invited==true || p.zone!="daycare" && p.zone!=DaycareVet.Zone)continue;TravelPlayer(p,DaycareVet.Zone);m.attending=true;m.declined=false;DeclineOtherDaycare(id);changed=true;}
            if(changed)state.revision++;return changed;
        }
        private string VetOperation(SoloCommand c,SoloPlayer p)
        {
            var g=state.vet;if(g==null)return "clinic-unavailable";var m=g.members.Single(v=>v.actor==p.id);
            if(c.value=="leave"){if(p.zone!=DaycareVet.Zone)return "wrong-area";TravelPlayer(p,"daycare");m.attending=false;m.declined=true;DeclineOtherDaycare(p.id);return null;}
            if(c.value=="start" || c.value=="replay"){
                if(p.zone!="daycare" && p.zone!=DaycareVet.Zone)return "come-to-daycare";
                if(c.value=="replay" && !g.patients.All(a=>a.bed== -2))return "care-for-our-friends-first";
                if(g.round==0 || c.value=="replay"){
                    if(g.round>=int.MaxValue-1)return "round-limit";g.round++;g.patients=VetPatients();var first=new[]{0,4,1,5};for(var i=0;i<4;i++){g.patients[first[i]].bed=i;g.patients[first[i]].arrived=g.clock;}
                    g.friends=DaycareNpcCasts.Pick(2,g.friends);foreach(var v in g.members){v.declined=false;v.gesture="";v.patient=-1;v.last=-1;}
                }
                if(p.zone!=DaycareVet.Zone)TravelPlayer(p,DaycareVet.Zone);m.attending=true;m.declined=false;DeclineOtherDaycare(p.id);return null;
            }
            if(p.zone!=DaycareVet.Zone || !m.attending)return "not-in-clinic";
            var parts=c.target.Split('@');if(parts.Length!=2 || !int.TryParse(parts[0],out var index) || index<0 || index>7 || !int.TryParse(parts[1],out var round) || round!=g.round)return "old-patient-card";
            var patient=g.patients[index];
            if(c.value=="welcome"){
                if(patient.bed!= -1)return "friend-already-welcomed";var bed=Enumerable.Range(0,4).Where(b=>!g.patients.Any(a=>a.bed==b)).DefaultIfEmpty(-1).First();if(bed<0)return "care-beds-full";patient.bed=bed;patient.arrived=g.clock;return null;
            }
            if(patient.bed<0)return "choose-a-care-bed";
            if(c.value=="home"){if(!DaycareVet.Ready(patient))return "check-the-care-pictures";patient.bed=-2;return null;}
            var tool=Array.IndexOf(new[]{"wash","brush","bandage","cuddle"},c.value);if(tool<0 || c.x<0 || c.x>1 || c.y<0 || c.y>1 || c.item.Length==0)return "choose-a-care-tool";
            if(!DaycareVet.Needs(patient,tool))return "try-the-picture-card";
            if(tool==2 && !DaycareVet.Done(patient,0))return "wash-before-bandage";
            if(g.clock-m.last<.12 || m.patient==index && m.tool==c.value && m.gesture==c.item && Math.Sqrt(Math.Pow(m.x-c.x,2)+Math.Pow(m.y-c.y,2))<.025)return "gentle-strokes";
            var spot=tool==2?2:Enumerable.Range(0,3).OrderBy(i=>Math.Pow(DaycareVet.Spot(index,i).X-c.x,2)+Math.Pow(DaycareVet.Spot(index,i).Y-c.y,2)).First();var at=DaycareVet.Spot(index,spot);
            var radius=tool==3?.38:.16;if(Math.Sqrt(Math.Pow(at.X-c.x,2)+Math.Pow(at.Y-c.y,2))>radius)return "touch-the-pictured-spot";
            m.last=g.clock;m.patient=index;m.tool=c.value;m.gesture=c.item;m.x=c.x;m.y=c.y;
            if(tool==0)patient.washed[spot]=Math.Min(3,patient.washed[spot]+1);else if(tool==1)patient.brushed[spot]=Math.Min(3,patient.brushed[spot]+1);else if(tool==2)patient.bandaged=true;else patient.cuddles=Math.Min(3,patient.cuddles+1);
            patient.reaction=g.clock;if(patient.reactions<int.MaxValue)patient.reactions++;return null;
        }
        private bool AdvanceVet(double seconds,string[] active,out bool visible){visible=false;var g=state.vet;if(g==null || !g.members.Any(m=>m.attending && (active==null || active.Contains(m.actor))))return false;g.clock+=seconds;return true;}
    }
}
