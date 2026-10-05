using System;
using System.Globalization;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class SandAttachment
    {
        // slot is read only for pre-format-four migration and the retained preview.
        public int slot=-1,version; public string id,kind,creator;
        public float x,y; public bool legacy;
        public SandAttachment Copy()=>(SandAttachment)MemberwiseClone();
    }
    [Serializable] public sealed class SandToy
    {
        public bool placed; public float x,y; public int version,reaction;
        public SandToy Copy()=>(SandToy)MemberwiseClone();
    }
    [Serializable] public sealed class SandReset
    {
        public string token; public string[] voters,approved;
        public SandReset Copy()=>string.IsNullOrEmpty(token)?null:new SandReset{token=token,voters=voters.ToArray(),approved=approved.ToArray()};
    }
    public static class SandpitPlay
    {
        public const int Schema=52,Slots=6;
        public static bool Attachment(int slot,string kind)=>slot>=0 && slot<Slots && (slot<2?kind=="flag":slot<4?kind=="shell" || kind=="pebble":kind=="door" || kind=="window");
        public static WalkPoint Socket(SandMould m,int slot)
        {
            var side=slot%2==0?-1:1;
            if(!DaycareSandpit.LongShape(m.shape))return new WalkPoint(side*(slot<2?24:slot<4?43:25),slot<2?(m.capacity==3?112:95):slot<4?8:43);
            if(m.orientation==90)return new WalkPoint(slot<4?side*18:0,slot<2?(side<0?-38:171):slot<4?(side<0?-100:88):(side<0?-76:112));
            return new WalkPoint(side*(slot<2?88:slot<4?105:m.shape=="gate"?99:76),slot<2?80:slot<4?8:32);
        }
        public static string MoveReason(SandpitState g,SandMould moving,float x,float y)
        {
            var at=DaycareSandpit.Snap(x,y,moving.shape,moving.orientation);
            if(!DaycareSandpit.Inside(x,y,moving.width,moving.depth) || !DaycareSandpit.Inside(at.X,at.Y,moving.width,moving.depth))return "outside-sandpit";
            return g.moulds.Any(m=>m.id!=moving.id && Math.Abs(m.x-at.X)<(m.width+moving.width)/2 && Math.Abs(m.y-at.Y)<(m.depth+moving.depth)/2) || g.toy?.placed==true && Math.Abs(g.toy.x-at.X)<moving.width/2+22 && Math.Abs(g.toy.y-at.Y)<moving.depth/2+21 || g.ground.Any(a=>Math.Abs(a.x-at.X)<moving.width/2+12 && Math.Abs(a.y-at.Y)<moving.depth/2+12)?"sand-spot-taken":null;
        }
        // The toy is a separate single visitor, never an inventory/hold lease.
        public static bool ToyPosition(SandpitState g,float x,float y)=>DaycareSandpit.Inside(x,y,44,42) && !g.moulds.Any(m=>Math.Abs(m.x-x)<m.width/2+22 && Math.Abs(m.y-y)<m.depth/2+21) && !(g.ground??Array.Empty<SandAttachment>()).Any(a=>Math.Abs(a.x-x)<34 && Math.Abs(a.y-y)<34);
    }
    public sealed partial class GameWorld
    {
        public bool CancelSandReset()
        {if(state.sandpit?.reset==null)return false;state.sandpit.reset=null;state.revision++;return true;}
        private void SandChanged(){state.sandpit.reset=null;}
        private string SandResetOperation(SoloCommand c,SoloPlayer p,string[] connected)
        {
            var g=state.sandpit;if(g==null || g.phase<1 || c.target!=DaycareSandpit.Target("reset",g.round))return "old-sandpit-lesson";
            var voters=(connected ?? new[]{p.id}).OrderBy(v=>v,StringComparer.Ordinal).ToArray();
            if(c.value=="reset-request"){
                if(!g.members.Any(m=>m.actor==p.id && m.attending))return "come-to-sandpit";
                if(g.reset!=null)return "reset-already-pending";
                if(g.round>=int.MaxValue-1)return "round-limit";
                g.reset=new SandReset{token=c.requestId,voters=voters,approved=Array.Empty<string>()};return null;
            }
            var vote=g.reset;if(vote==null || vote.token!=c.item || !vote.voters.SequenceEqual(voters) || !vote.voters.Contains(p.id))return "old-sand-reset";
            if(c.value=="reset-decline"){g.reset=null;return null;}
            if(c.value!="reset-yes")return "try-sand-tools";
            if(!vote.approved.Contains(p.id))vote.approved=vote.approved.Concat(new[]{p.id}).ToArray();
            if(vote.approved.Length==vote.voters.Length){g.moulds=Array.Empty<SandMould>();g.ground=Array.Empty<SandAttachment>();g.toy=new SandToy();g.round++;g.reset=null;}
            return null;
        }
        private string SandPlayOperation(SoloCommand c,SoloPlayer p,SandMould m)
        {
            var g=state.sandpit;
            if(c.value=="decorate"){
                var a=new SandAttachment{id="d-"+c.requestId,kind=c.item,creator=p.id,x=c.x,y=c.y};
                if(!m.built || !SandDecorSurface.Kind(a.kind) || a.id.Length>96)return "invalid-sand-attachment";
                if(g.ground.Concat(g.moulds.SelectMany(v=>v.attachments)).Any(v=>v.id==a.id))return "old-sand-decoration";
                var why=SandDecorSurface.AttachedReason(m,a);if(why!=null)return why;
                m.attachments=m.attachments.Concat(new[]{a}).ToArray();m.version++;SandChanged();return null;
            }
            if(c.value=="move" || c.value=="remove" || c.value=="decor-remove" || c.value=="decor-replace"){
                if(string.IsNullOrEmpty(m.creator) || m.creator!=p.id)return "not-your-sand-piece";
                var args=c.item.Split(':');
                if(!int.TryParse(args[0],NumberStyles.None,CultureInfo.InvariantCulture,out var version) || version!=m.version)return "sand-piece-changed";
                if(c.value=="move"){
                    var why=SandpitPlay.MoveReason(g,m,c.x,c.y);if(why!=null)return why;
                    var at=DaycareSandpit.Snap(c.x,c.y,m.shape,m.orientation);m.x=at.X;m.y=at.Y;m.version++;SandChanged();return null;
                }
                if(c.value=="remove"){g.moulds=g.moulds.Where(v=>v.id!=m.id).ToArray();SandChanged();return null;}
                // IDs are separate from piece identity. Legacy decoration IDs may
                // contain ':'; split only the captured version prefix.
                var colon=c.item.IndexOf(':');var rest=colon<0?"":c.item.Substring(colon+1);
                var a=m.attachments.FirstOrDefault(v=>v.id==rest);
                if(c.value=="decor-replace"){
                    var split=rest.LastIndexOf(':');if(split<0)return "invalid-sand-attachment";
                    var id=rest.Substring(0,split);var kind=rest.Substring(split+1);a=m.attachments.FirstOrDefault(v=>v.id==id);
                    if(a==null || !SandDecorSurface.Kind(kind))return "invalid-sand-attachment";
                    if(a.version==int.MaxValue)return "sand-piece-limit";
                    var replacement=new SandAttachment{id=a.id,creator=a.creator,kind=kind,x=c.x,y=c.y,version=a.version+1};
                    var why=SandDecorSurface.AttachedReason(m,replacement,a.id);if(why!=null)return why;
                    m.attachments=m.attachments.Where(v=>v.id!=a.id).Concat(new[]{replacement}).ToArray();
                }else{if(a==null)return "invalid-sand-attachment";m.attachments=m.attachments.Where(v=>v.id!=a.id).ToArray();}
                m.version++;SandChanged();return null;
            }
            return "try-sand-tools";
        }
        private string SandGroundOperation(SoloCommand c,SoloPlayer p)
        {
            var g=state.sandpit;
            if(c.value=="ground-decorate"){
                var a=new SandAttachment{id="d-"+c.requestId,creator=p.id,kind=c.item,x=c.x,y=c.y};
                if(a.id.Length>96)return "invalid-sand-attachment";
                if(g.ground.Concat(g.moulds.SelectMany(v=>v.attachments)).Any(v=>v.id==a.id))return "old-sand-decoration";
                var why=SandDecorSurface.GroundReason(g,a);if(why!=null)return why;
                g.ground=g.ground.Concat(new[]{a}).ToArray();SandChanged();return null;
            }
            var a0=g.ground.FirstOrDefault(v=>v.id==c.item);if(a0==null)return "old-sand-decoration";
            if(a0.creator!=p.id)return "not-your-sand-piece";
            if(c.value!="ground-remove")return "try-sand-tools";
            g.ground=g.ground.Where(a=>a.id!=a0.id).ToArray();SandChanged();return null;
        }
        private string SandToyOperation(SoloCommand c)
        {
            var g=state.sandpit;var toy=g.toy;
            if(toy.version==int.MaxValue || toy.reaction==int.MaxValue)return "sand-piece-limit";
            if(c.value=="toy-place"){
                if(!int.TryParse(c.item,NumberStyles.None,CultureInfo.InvariantCulture,out var version) || version!=toy.version)return "sand-toy-changed";
                if(!SandpitPlay.ToyPosition(g,c.x,c.y))return "sand-toy-spot-taken";
                toy.placed=true;toy.x=c.x;toy.y=c.y;toy.version++;SandChanged();return null;
            }
            if(c.value=="toy-react" && toy.placed){toy.reaction++;toy.version++;SandChanged();return null;}
            return "try-sand-tools";
        }
        private static void ValidateSandPlay(SoloSnapshot s,SandpitState g)
        {
            if(g.format<3)return;
            if(g.toy==null || g.toy.version<0 || g.toy.reaction<0 || !KeepyRules.Finite(g.toy.x) || !KeepyRules.Finite(g.toy.y) || g.toy.placed && !SandpitPlay.ToyPosition(g,g.toy.x,g.toy.y))throw new InvalidOperationException("Invalid sand toy checkpoint.");
            foreach(var m in g.moulds){
                if(g.format==3){if(m.attachments==null || m.attachments.Length>SandpitPlay.Slots || m.attachments.Any(a=>a==null || !SandpitPlay.Attachment(a.slot,a.kind)) || m.attachments.Select(a=>a.slot).Distinct().Count()!=m.attachments.Length || !m.built && m.attachments.Length>0 || m.decoration!=0)throw new InvalidOperationException("Invalid old sand attachments checkpoint.");}
                else if(m.attachments==null || m.attachments.Length>SandDecorSurface.PieceLimit || !m.built && m.attachments.Length>0 || m.decoration!=0 || m.attachments.Any(a=>!ValidSandDecoration(s,a) || (a.legacy? a.slot<0 || a.slot>=SandpitPlay.Slots || !SandpitPlay.Attachment(a.slot,a.kind) || a.x!=SandDecorSurface.LegacyAnchor(m,a.slot).X || a.y!=SandDecorSurface.LegacyAnchor(m,a.slot).Y:!SandDecorSurface.Supported(m,a))))throw new InvalidOperationException("Invalid positioned sand decoration checkpoint.");
            }
            if(g.format==DaycareSandpit.Format){
                if(g.ground==null || g.ground.Length>SandDecorSurface.GroundLimit || g.ground.Any(a=>!ValidSandDecoration(s,a) || string.IsNullOrEmpty(a.creator) || a.legacy || SandDecorSurface.GroundReason(g,a,a.id)!=null))throw new InvalidOperationException("Invalid ground decoration checkpoint.");
                var all=g.ground.Concat(g.moulds.SelectMany(m=>m.attachments)).ToArray();if(all.Select(a=>a.id).Distinct().Count()!=all.Length)throw new InvalidOperationException("Duplicate sand decoration identity.");
                foreach(var m in g.moulds)for(var i=0;i<m.attachments.Length;i++)if(m.attachments.Take(i).Any(b=>!(b.legacy && m.attachments[i].legacy) && SandDecorSurface.Overlap(b,m.attachments[i])))throw new InvalidOperationException("Overlapping sand decorations.");
            }
            // Unity inline serialization materializes a null class as an empty
            // object. Only this empty representation means no pending consent.
            var r=g.reset;if(r!=null && string.IsNullOrEmpty(r.token) && (r.voters==null || r.voters.Length==0) && (r.approved==null || r.approved.Length==0)){g.reset=null;r=null;}
            if(r!=null && (!Id(r.token) || r.voters==null || r.approved==null || r.voters.Length<1 || r.voters.Length>4 || r.voters.Distinct().Count()!=r.voters.Length || r.voters.Any(v=>!s.players.Any(p=>p.id==v)) || r.approved.Distinct().Count()!=r.approved.Length || r.approved.Any(a=>!r.voters.Contains(a))))throw new InvalidOperationException("Invalid sand reset checkpoint.");
        }
        private static bool ValidSandDecoration(SoloSnapshot s,SandAttachment a)=>a!=null && Id(a.id) && a.id.Length<=128 && SandDecorSurface.Kind(a.kind) && KeepyRules.Finite(a.x) && KeepyRules.Finite(a.y) && a.version>=0 && (string.IsNullOrEmpty(a.creator) || s.players.Any(p=>p.id==a.creator));
    }
}
