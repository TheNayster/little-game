using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class SandpitMember
    {
        public string actor; public bool attending,declined;
        public SandpitMember Copy()=>(SandpitMember)MemberwiseClone();
    }
    [Serializable] public sealed class SandMould
    {
        public int scoops,decoration; public bool wet,built;
        public SandMould Copy()=>(SandMould)MemberwiseClone();
    }
    [Serializable] public sealed class SandpitState
    {
        public int round,phase; public double started; public float teacherX,teacherY;
        public SandpitMember[] members; public SandMould[] moulds; public string[] friends;
        public SandpitState Copy()=>new SandpitState{round=round,phase=phase,started=started,teacherX=teacherX,teacherY=teacherY,members=members.Select(m=>m.Copy()).ToArray(),moulds=moulds.Select(m=>m.Copy()).ToArray(),friends=friends.ToArray()};
    }
    public static class DaycareSandpit
    {
        public const int Schema=46;
        public static WalkPoint Teacher=>new WalkPoint(4020,440);
        public static WalkPoint Place(int i)=>new WalkPoint(4210+i*160,440);
        public static WalkPoint Work(int i)=>new WalkPoint(Place(i).X-80,350);
        public static int Capacity(int i)=>i%2==0?2:3;
        public static double Arrival(SandpitState s)=>Math.Sqrt(Math.Pow(s.teacherX-Teacher.X,2)+Math.Pow((s.teacherY-Teacher.Y)*.45,2))/180;
        public static WalkPoint TeacherPoint(SandpitState s,double clock)
        {
            var t=Math.Min(1,Math.Max(0,(clock-s.started)/Math.Max(.01,Arrival(s))));
            return new WalkPoint((float)(s.teacherX+(Teacher.X-s.teacherX)*t),(float)(s.teacherY+(Teacher.Y-s.teacherY)*t));
        }
        public static double Demonstration(SandpitState s,double clock)=>clock-s.started-Arrival(s);
        public static string Hint(SandpitState s,double clock)
        {
            if(s.phase==1){var t=Demonstration(s,clock);return t<0?"Calypso is coming! Try filling your bucket while we wait.":t<4.2?"Calypso: One scoop, two scoops. The small bucket is full!":t<7?"Calypso: A little water helps the sand stick together.":"Calypso: Press, turn it over, lift! Now let's make a castle together.";}
            return s.phase==3?"Our four towers made a castle! Add flags or shells, or try another lesson.":"Fill a bucket, add water, then turn it over. Small = 2 scoops. Big = 3.";
        }
    }
    public sealed partial class SoloWorld
    {
        public SandpitState ReadSandpit()=>state.sandpit?.Copy();
        private static void NormalizeSandpit(SoloSnapshot s)
        {if(s!=null && s.schema<DaycareSandpit.Schema && s.sandpit!=null && s.sandpit.phase==0 && (s.sandpit.members==null || s.sandpit.members.Length==0))s.sandpit=null;}
        private static SandpitState NewSandpit(SoloSnapshot s)=>new SandpitState{members=s.players.Select(p=>new SandpitMember{actor=p.id}).ToArray(),moulds=Enumerable.Range(0,4).Select(i=>new SandMould()).ToArray(),friends=DaycareNpcCasts.Pick(2)};
        private static void ValidateSandpit(SoloSnapshot s)
        {
            var g=s.sandpit;if(g==null && s.schema<DaycareSandpit.Schema)return;
            if(g==null || s.daycare==null || g.round<0 || g.round>=int.MaxValue || g.phase<0 || g.phase>3 || !KeepyRules.Finite(g.started) || g.started<0 || g.started>s.daycare.clock || !WorldLayout.Position("daycare",s.schema,g.teacherX,g.teacherY) || g.members==null || !g.members.Select(m=>m?.actor).OrderBy(x=>x).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x)) || g.moulds==null || g.moulds.Length!=4 || !DaycareNpcCasts.Valid(g.friends,2))throw new InvalidOperationException("Invalid sandpit checkpoint.");
            if(g.phase==0 && g.round!=0 || g.phase>0 && g.round==0 || g.phase==3 && g.moulds.Any(m=>!m.built))throw new InvalidOperationException("Invalid sand lesson progress.");
            for(var i=0;i<4;i++){var m=g.moulds[i];if(m==null || m.scoops<0 || m.scoops>DaycareSandpit.Capacity(i) || m.decoration<0 || m.decoration>2 || m.built && (!m.wet || m.scoops!=DaycareSandpit.Capacity(i)) || !m.built && m.decoration!=0)throw new InvalidOperationException("Invalid sand mould.");}
            foreach(var m in g.members)if(m.attending && (g.phase==0 || s.players.Single(p=>p.id==m.actor).zone!="daycare"))throw new InvalidOperationException("Invalid sandpit participant.");
        }
        public bool ReleaseSandpit(string actor)
        {var m=state.sandpit?.members.FirstOrDefault(v=>v.actor==actor);if(m==null || !m.attending)return false;m.attending=false;state.revision++;return true;}
        public bool JoinSandpitGroup(string[] connected)
        {
            var g=state.sandpit;if(g==null || g.phase==0)return false;var changed=false;
            foreach(var id in connected){var p=state.players.Single(v=>v.id==id);var m=g.members.Single(v=>v.actor==id);if(p.zone!="daycare" || m.attending || m.declined)continue;ReleaseDaycare(id);state.daycare.members.Single(v=>v.actor==id).declined=true;state.kingdom.members.Single(v=>v.actor==id).declined=true;m.attending=true;var at=DaycareSandpit.Work(Array.IndexOf(g.members,m));p.x=at.X;p.y=at.Y;changed=true;}
            if(changed)state.revision++;return changed;
        }
        private string SandpitOperation(SoloCommand c,SoloPlayer p)
        {
            var g=state.sandpit;if(g==null || p.zone!="daycare")return "come-to-sandpit";var own=g.members.Single(m=>m.actor==p.id);
            if(c.value=="leave"){own.attending=false;own.declined=true;return null;}
            if(c.value=="start" || c.value=="replay"){
                if(c.value=="replay" && g.phase!=3)return "finish-our-castle-first";
                if(g.phase==0 || c.value=="replay"){
                    if(g.round>=int.MaxValue-1)return "round-limit";
                    var teacher=g.phase==0?DaycareTeacher.Point(state.daycare):DaycareSandpit.Teacher;
                    g.teacherX=teacher.X;g.teacherY=teacher.Y;g.started=state.daycare.clock;g.round++;g.phase=1;
                    if(g.round>1)g.friends=DaycareNpcCasts.Pick(2,g.friends);
                    g.moulds=Enumerable.Range(0,4).Select(i=>new SandMould()).ToArray();foreach(var m in g.members)m.declined=false;
                }
                state.kingdom.members.Single(v=>v.actor==p.id).declined=true;ReleaseDaycare(p.id);state.daycare.members.Single(m=>m.actor==p.id).declined=true;own.attending=true;own.declined=false;var at=DaycareSandpit.Work(Array.IndexOf(g.members,own));p.x=at.X;p.y=at.Y;return null;
            }
            if(!own.attending || g.phase<1 || !int.TryParse(c.target,out var index) || index<0 || index>3)return "watch-calypso-first";
            var point=DaycareSandpit.Place(index);if(Math.Abs(p.x-point.X)>100 || Math.Abs(p.y-point.Y)>120)return "walk-closer";
            var mould=g.moulds[index];
            if(c.value=="scoop"){if(mould.built || mould.scoops==DaycareSandpit.Capacity(index))return "bucket-full";mould.scoops++;return null;}
            if(c.value=="water"){if(mould.built)return "tower-built";mould.wet=true;return null;}
            if(c.value=="tip"){
                if(mould.built)return "tower-built";
                if(mould.scoops!=DaycareSandpit.Capacity(index))return "fill-bucket-first";
                if(!mould.wet){mould.scoops=0;return null;}
                mould.built=true;if(g.moulds.All(m=>m.built) && g.phase==2)g.phase=3;return null;
            }
            if((c.value=="flag" || c.value=="shell") && mould.built){mould.decoration=c.value=="flag"?1:2;return null;}
            return "try-sand-tools";
        }
        private bool AdvanceSandpit(double seconds,string[] activePlayers,out bool visible)
        {
            visible=false;var g=state.sandpit;if(g==null || g.phase!=1)return false;
            // Watching play elsewhere must not finish an empty child's lesson.
            if(!g.members.Any(m=>m.attending && (activePlayers==null || activePlayers.Contains(m.actor)))){g.started+=seconds;return true;}
            if(DaycareSandpit.Demonstration(g,state.daycare.clock)<10.6)return false;
            g.phase=g.moulds.All(m=>m.built)?3:2;visible=true;return true;
        }
    }
}
