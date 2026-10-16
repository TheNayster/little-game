using System;
using System.Linq;
namespace LittleWeeps.Core
{
    public static partial class PeppaFunfair
    {
        public static void Validate(FunfairState h,string[] actors)
        {
            void Need(bool ok){if(!ok)throw new InvalidOperationException("Invalid Funfair state.");}
            bool Finite(double v)=>!double.IsNaN(v)&&!double.IsInfinity(v);
            Need(h!=null && h.version==1 && Finite(h.clock) && h.clock>=0);
            Need(h.members!=null && h.members.Length==actors.Length && h.members.All(m=>m!=null && actors.Contains(m.actor)) && h.members.Select(m=>m.actor).Distinct().Count()==actors.Length);
            Need(h.members.Select(m=>m.role).Distinct().Count()==h.members.Length);
            foreach(var m in h.members)Need(Roles.Contains(m.role) && (m.station=="" || Stations.Contains(m.station)) && m.paint?.Length==64 && m.paint.All(v=>v>=0&&v<=8) && m.prizes?.Length==4 && m.prizes.All(v=>v>=0) && m.slidePose>=0 && m.slidePose<=2 && m.paintStyle>=0 && m.paintStyle<=3 && m.seat>=-1 && m.seat<4 && Finite(m.at) && Finite(m.until));
            foreach(var queue in new[]{h.slideQueue,h.hookQueue,h.paintQueue})Need(queue!=null && queue.Length<=actors.Length && queue.Distinct().Count()==queue.Length && queue.All(actors.Contains));
            Need(h.paintDraft?.Length==64 && h.paintDraft.All(v=>v>=0 && v<=8) && h.paintStyle>=1 && h.paintStyle<=3);
            Need(h.hookActor=="" || h.hookQueue.FirstOrDefault()==h.hookActor);Need(h.paintActor=="" || h.paintQueue.FirstOrDefault()==h.paintActor);
            Need((string.IsNullOrEmpty(h.photoRole)||Roles.Contains(h.photoRole)) && h.photoPose>=0 && h.photoPose<=2 && (h.photoActor==""||actors.Contains(h.photoActor)));
            Need(new[]{"idle","cast","aim","pull","reward"}.Contains(h.hookStage) && h.hookTarget>=-1 && h.hookTarget<15 && h.aimX>=0 && h.aimX<=1 && h.aimY>=0 && h.aimY<=1);
            foreach(var pair in new[]{(h.wheel,"wheel"),(h.teacups,"teacups")}){
                var r=pair.Item1;Need(r!=null && new[]{"idle","boarding","countdown","running","finish"}.Contains(r.stage) && r.riders!=null && r.riders.Length<=4 && r.riders.Distinct().Count()==r.riders.Length && r.riders.All(actors.Contains));
                Need(KeepyRules.Finite(r.angle) && r.angle>=0 && r.angle<360 && KeepyRules.Finite(r.speed) && r.speed>=0 && r.speed<=325.01f && KeepyRules.Finite(r.momentum) && r.momentum>=0 && r.momentum<=180 && r.rounds>=0);
                Need(r.riders.All(a=>h.members.Any(m=>m.actor==a && m.station==pair.Item2 && m.seat>=0)) && h.members.Where(m=>m.station==pair.Item2).Select(m=>m.seat).Distinct().Count()==r.riders.Length);
            }
            Need(h.props?.Length==12 && h.props.Select(p=>p.id).Distinct().Count()==12);
            foreach(var p in h.props)Need(p!=null && (p.kind=="drink" || p.kind=="balloon") && new[]{"stock","held","ground"}.Contains(p.location) && (p.holder=="" || actors.Contains(p.holder)) && (p.location=="held")== (p.holder!="") && Point(p.x,p.y));
            Need(h.props.Where(p=>p.holder!="").GroupBy(p=>p.holder).All(g=>g.Count()==1));
            Need(h.npcs?.Length==28 && h.npcs.All(n=>n!=null && (Roles.Contains(n.role)||n.role=="MrsRabbit") && n.section>=0 && n.section<4 && Point(n.x,n.y) && KeepyRules.Finite(n.targetX)));
        }
    }
}
