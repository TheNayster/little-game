using System;
using System.Linq;
namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        // Earlier isolated world branches used overlapping format numbers. A
        // missing module is additive only before the combined schema and when
        // no saved player occupies that world; current-schema loss stays invalid.
        public const int MarketsSchema=58;
        public MarketsState ReadMarkets()=>state.markets?.Copy();
        private static void NormalizeMarkets(SoloSnapshot s)
        {
            if(s!=null && s.schema<WorldLayout.Schema && !s.players.Any(p=>p.zone==MarketsRules.Zone) && s.markets!=null && s.markets.clock==0 && (s.markets.members==null || s.markets.members.Length==0))s.markets=null;
        }
        private static void ValidateMarkets(SoloSnapshot s)
        {if(s.markets==null && s.schema<WorldLayout.Schema && !s.players.Any(p=>p.zone==MarketsRules.Zone))return;MarketsRules.Validate(s.markets,s.players);}
        private static void RestoreMarkets(SoloSnapshot s){MarketsRules.Restore(s.markets);}
        public void ReleaseMarkets(string actor)
        {if(state.markets==null)return;MarketsRules.Release(state.markets,actor,state.players);state.revision++;}
        private bool LeaveMarketsStation(string actor)
        {
            var m=state.markets?.members.FirstOrDefault(p=>p.actor==actor);
            if(m==null || m.station=="")return false;
            MarketsRules.Release(state.markets,actor,state.players,false);return true;
        }
        private bool AdvanceMarkets(double seconds,string[] active,out bool visible)
        {
            visible=false;if(state.markets==null)return false;
            var players=state.players.Where(p=>active==null || active.Contains(p.id)).ToArray();
            if(!players.Any(p=>p.zone==MarketsRules.Zone))return false;
            string before=MarketsEventStamp(state.markets);
            foreach(var m in state.markets.members.Where(m=>!players.Any(p=>p.id==m.actor&&p.zone==MarketsRules.Zone)&&(m.station!=""||state.markets.items.Any(i=>i.holder==m.actor))))MarketsRules.Release(state.markets,m.actor);
            bool advanced=MarketsRules.Advance(state.markets,seconds,state.players);
            // Clock and prop interpolation use the existing ambient snapshot
            // path. Only discrete transitions invalidate in-flight commands.
            AmbientChanged|=advanced;visible=before!=MarketsEventStamp(state.markets);return advanced;
        }
        private static string MarketsEventStamp(MarketsState h)=>string.Join(",",h.members.Select(m=>m.station+":"+m.pose))+"/"+string.Join(",",h.stations.Select(t=>t.workingActor+":"+t.completed+":"+string.Join("-",t.queue)))+"/"+string.Join(",",h.items.Select(i=>i.id+":"+i.revision))+"/"+string.Join(",",h.bubbles.Select(b=>b.id));
        private string MarketsOperation(SoloCommand c,SoloPlayer p)
        {
            if(p.zone!=MarketsRules.Zone || state.markets==null)return "come-to-markets";
            var parts=(c.value??"").Split(new[]{':'},2);string op=parts[0],value=parts.Length>1?parts[1]:"";
            string station=op=="join"?c.target:op=="ingredient"||op=="mix"?"smoothie":op=="flower"||op=="bouquet"?"flowers":op=="cake"?"cakes":op=="bubble"||op=="pop"?"bubbles":op=="care"||op=="makeover"||op=="pony-reset"?c.target:op.StartsWith("ice-",StringComparison.Ordinal)||op.StartsWith("build-",StringComparison.Ordinal)?"icecream":op.StartsWith("music-",StringComparison.Ordinal)||op=="song"||op=="beat"||op=="guitar"?"music":"";
            var item=state.markets.items.FirstOrDefault(i=>i.id==c.item);
            if(op=="claim"&&item!=null)station=item.station;
            if(station!="" && (!MarketsRules.StationIds.Contains(station)||Math.Abs(p.x-MarketsRules.X(station))>300||p.y>340))return "come-closer";
            if(op=="give"){
                var other=state.players.FirstOrDefault(a=>a.id==c.target&&a.zone==p.zone);
                if(other==null||Math.Abs(p.x-other.x)>180||Math.Abs(p.y-other.y)>150)return "come-closer";
            }
            var error=MarketsRules.Apply(state.markets,p.id,op,c.item,c.target,value,state.players);
            if(error==null&&op=="join"){
                // Four readable contact spots extend the source's two actors.
                int index=Array.FindIndex(state.players,a=>a.id==p.id);p.x=MarketsRules.X(c.target)-225+index*150;p.y=300;
            }
            return error;
        }
    }
}
