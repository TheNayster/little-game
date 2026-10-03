using System;
using System.Globalization;
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
        // Empty creator deliberately means legacy/shared. Coordinates and footprint
        // survive later editing; array order is presentation only, never command identity.
        public string id,creator,shape;
        public float x,y,width,depth;
        public int capacity,scoops,decoration,version; public bool wet,built;
        public SandMould Copy()=>(SandMould)MemberwiseClone();
    }
    [Serializable] public sealed class SandpitState
    {
        public int format,pieceLimit,scoopCapacity,round,phase; public double started; public float teacherX,teacherY;
        public SandpitMember[] members; public SandMould[] moulds; public string[] friends;
        public SandpitState Copy()=>new SandpitState{format=format,pieceLimit=pieceLimit,scoopCapacity=scoopCapacity,round=round,phase=phase,started=started,teacherX=teacherX,teacherY=teacherY,members=members.Select(m=>m.Copy()).ToArray(),moulds=moulds.Select(m=>m.Copy()).ToArray(),friends=friends.ToArray()};
    }
    public static class DaycareSandpit
    {
        public const int Schema=46, PieceSchema=50, MaxPieces=16, DefaultScoops=3, Columns=8, Rows=4;
        public const float Left=4070,Right=4800,Bottom=70,Top=540,PieceWidth=72,PieceDepth=80;
        public static WalkPoint Teacher=>new WalkPoint(4020,440);
        // These functions remain solely to read legacy format-zero checkpoints.
        public static WalkPoint Place(int i)=>new WalkPoint(4210+i*160,440);
        public static WalkPoint Work(int i)=>new WalkPoint(Place(i).X-80,350);
        public static int Capacity(int i)=>i%2==0?2:3;
        public static WalkPoint Place(SandMould m)=>new WalkPoint(m.x,m.y);
        public static WalkPoint Work(SandMould m)=>new WalkPoint(m.x-60,m.y-55);
        public static WalkPoint Cell(int col,int row)=>new WalkPoint(4150+col*85,130+row*110);
        public static WalkPoint Snap(float x,float y)=>Cell((int)Math.Round((x-4150)/85), (int)Math.Round((y-130)/110));
        public static string Target(string id,int epoch)=>id+"@"+epoch.ToString(CultureInfo.InvariantCulture);
        public static string Target(int index,int epoch)=>Target("legacy-"+index,epoch);
        public static bool Inside(float x,float y,float width,float depth)=>KeepyRules.Finite(x) && KeepyRules.Finite(y) && x-width/2>=Left && x+width/2<=Right && y-depth/2>=Bottom && y+depth/2<=Top;
        public static string Placement(SandpitState s,float x,float y)
        {
            if(s.moulds.Length>=s.pieceLimit)return "sandpit-full";
            if(!Inside(x,y,PieceWidth,PieceDepth) || !Inside(Snap(x,y).X,Snap(x,y).Y,PieceWidth,PieceDepth))return "outside-sandpit";
            var at=Snap(x,y);
            return s.moulds.Any(m=>Math.Abs(m.x-at.X)<(m.width+PieceWidth)/2 && Math.Abs(m.y-at.Y)<(m.depth+PieceDepth)/2)?"sand-spot-taken":null;
        }
        public static int UsefulScoops(SandMould m,int retained)=>m==null || m.built?0:Math.Min(Math.Max(0,retained),m.capacity-m.scoops);
        public static double Arrival(SandpitState s)=>Math.Sqrt(Math.Pow(s.teacherX-Teacher.X,2)+Math.Pow((s.teacherY-Teacher.Y)*.45,2))/180;
        public static WalkPoint TeacherPoint(SandpitState s,double clock)
        {
            var t=Math.Min(1,Math.Max(0,(clock-s.started)/Math.Max(.01,Arrival(s))));
            return new WalkPoint((float)(s.teacherX+(Teacher.X-s.teacherX)*t),(float)(s.teacherY+(Teacher.Y-s.teacherY)*t));
        }
        public static double Demonstration(SandpitState s,double clock)=>clock-s.started-Arrival(s);
        public static string Hint(SandpitState s,double clock)=>"Choose a spot. Scoop, water, then tip!";
    }
    public sealed partial class GameWorld
    {
        public SandpitState ReadSandpit()=>state.sandpit?.Copy();
        private static void NormalizeSandpit(SoloSnapshot s)
        {if(s!=null && s.schema<DaycareSandpit.Schema && s.sandpit!=null && s.sandpit.phase==0 && (s.sandpit.members==null || s.sandpit.members.Length==0))s.sandpit=null;}
        private static SandpitState NewSandpit(SoloSnapshot s)=>new SandpitState{format=1,pieceLimit=DaycareSandpit.MaxPieces,scoopCapacity=DaycareSandpit.DefaultScoops,members=s.players.Select(p=>new SandpitMember{actor=p.id}).ToArray(),moulds=Array.Empty<SandMould>(),friends=s.daycare.guests.Take(2).ToArray()};
        private static GameWorld WithSandpitPieces(GameWorld world)
        {
            if(world.state.sandpit?.format==1)return world;
            var s=world.Snapshot();
            if(s.sandpit==null)s.sandpit=NewSandpit(s);
            else {
                var g=s.sandpit;g.format=1;g.pieceLimit=DaycareSandpit.MaxPieces;g.scoopCapacity=DaycareSandpit.DefaultScoops;
                for(var i=0;i<g.moulds.Length;i++){var m=g.moulds[i];var at=DaycareSandpit.Place(i);m.id="legacy-"+i;m.creator="";m.shape="round";m.x=at.X;m.y=at.Y;m.width=140;m.depth=100;m.capacity=DaycareSandpit.Capacity(i);m.version=0;}
            }
            s.revision++;return new GameWorld(s);
        }
        private static void ValidateSandpit(SoloSnapshot s)
        {
            var g=s.sandpit;if(g==null && s.schema<DaycareSandpit.Schema)return;
            if(g==null || s.daycare==null || g.round<0 || g.round>=int.MaxValue || g.phase<0 || g.phase>3 || !KeepyRules.Finite(g.started) || g.started<0 || g.started>s.daycare.clock || !WorldLayout.Position("daycare",s.schema,g.teacherX,g.teacherY) || g.members==null || !g.members.Select(m=>m?.actor).OrderBy(x=>x).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x)) || g.moulds==null || !DaycareNpcCasts.Valid(g.friends,2))throw new InvalidOperationException("Invalid sandpit checkpoint.");
            var legacy=g.format==0 && s.schema<DaycareSandpit.PieceSchema;
            if(legacy?g.moulds.Length!=4:g.format!=1 || g.pieceLimit<1 || g.pieceLimit>DaycareSandpit.MaxPieces || g.scoopCapacity<1 || g.scoopCapacity>8 || g.moulds.Length>g.pieceLimit)throw new InvalidOperationException("Invalid sand piece limits.");
            if(g.phase==0 && g.round!=0 || g.phase>0 && g.round==0 || legacy && g.phase==3 && g.moulds.Any(m=>m==null || !m.built))throw new InvalidOperationException("Invalid sand progress.");
            for(var i=0;i<g.moulds.Length;i++){
                var m=g.moulds[i];var capacity=legacy?DaycareSandpit.Capacity(i):m?.capacity ?? 0;
                if(m==null || capacity<1 || capacity>8 || m.scoops<0 || m.scoops>capacity || m.decoration<0 || m.decoration>2 || m.built && (!m.wet || m.scoops!=capacity) || !m.built && m.decoration!=0)throw new InvalidOperationException("Invalid sand mould.");
                if(!legacy && (!Id(m.id) || m.id.Length>96 || m.id.Contains("@") || m.version<0 || m.shape!="round" || !string.IsNullOrEmpty(m.creator) && !s.players.Any(p=>p.id==m.creator) || !KeepyRules.Finite(m.width) || !KeepyRules.Finite(m.depth) || m.width<DaycareSandpit.PieceWidth || m.width>140 || m.depth<DaycareSandpit.PieceDepth || m.depth>100 || !DaycareSandpit.Inside(m.x,m.y,m.width,m.depth) || g.moulds.Take(i).Any(p=>p.id==m.id || Math.Abs(p.x-m.x)<(p.width+m.width)/2 && Math.Abs(p.y-m.y)<(p.depth+m.depth)/2)))throw new InvalidOperationException("Invalid sand piece identity or footprint.");
            }
            foreach(var m in g.members)if(m.attending && (g.phase==0 || s.players.Single(p=>p.id==m.actor).zone!="daycare"))throw new InvalidOperationException("Invalid sandpit participant.");
        }
        public bool ReleaseSandpit(string actor)
        {var m=state.sandpit?.members.FirstOrDefault(v=>v.actor==actor);if(m==null || !m.attending)return false;m.attending=false;state.revision++;return true;}
        private void ArriveSandpit(SoloPlayer p,SandpitMember m)
        {
            ReleaseDaycare(p.id);state.daycare.members.Single(v=>v.actor==p.id).declined=true;state.kingdom.members.Single(v=>v.actor==p.id).declined=true;m.attending=true;
            p.x=4100+Array.IndexOf(state.sandpit.members,m)*100;p.y=75;
        }
        public bool JoinSandpitGroup(string[] connected)
        {
            var g=state.sandpit;if(g==null || g.phase==0)return false;var changed=false;
            foreach(var id in connected){var p=state.players.Single(v=>v.id==id);var m=g.members.Single(v=>v.actor==id);if(p.zone!="daycare" || m.attending || m.declined)continue;ArriveSandpit(p,m);changed=true;}
            if(changed)state.revision++;return changed;
        }
        private string SandpitOperation(SoloCommand c,SoloPlayer p)
        {
            var g=state.sandpit;if(g==null || p.zone!="daycare")return "come-to-sandpit";var own=g.members.Single(m=>m.actor==p.id);
            if(c.value=="leave"){own.attending=false;own.declined=true;return null;}
            if(c.value=="start" || c.value=="replay"){
                // Entry, completion and old replay commands never erase a creation.
                if(g.phase==0){if(g.round>=int.MaxValue-1)return "round-limit";var teacher=DaycareTeacher.Point(state.daycare);g.teacherX=teacher.X;g.teacherY=teacher.Y;g.started=state.daycare.clock;g.round++;g.phase=1;foreach(var m in g.members)m.declined=false;}
                ArriveSandpit(p,own);own.declined=false;return null;
            }
            if(!own.attending || g.phase<1)return "come-to-sandpit";
            var parts=c.target.Split('@');
            if(parts.Length!=2 || !int.TryParse(parts[1],NumberStyles.None,CultureInfo.InvariantCulture,out var epoch) || epoch<1 || epoch!=g.round)return "old-sandpit-lesson";
            if(c.value=="place"){
                if(parts[0]!="place" || c.item!="round")return "try-sand-tools";
                var reason=DaycareSandpit.Placement(g,c.x,c.y);if(reason!=null)return reason;
                var at=DaycareSandpit.Snap(c.x,c.y);
                var id="p-"+c.requestId;if(id.Length>96 || id.Contains("@"))return "invalid-command";if(g.moulds.Any(m=>m.id==id))return "sand-spot-taken";
                g.moulds=g.moulds.Concat(new[]{new SandMould{id=id,creator=p.id,shape="round",x=at.X,y=at.Y,width=DaycareSandpit.PieceWidth,depth=DaycareSandpit.PieceDepth,capacity=g.scoopCapacity}}).ToArray();return null;
            }
            var mould=g.moulds.FirstOrDefault(m=>m.id==parts[0]);if(mould==null)return "old-sandpit-piece";
            var point=DaycareSandpit.Place(mould);if(Math.Abs(p.x-point.X)>100 || Math.Abs(p.y-point.Y)>120)return "walk-closer";
            if(c.value=="water" && mould.wet && !mould.built)return null;
            if(mould.version==int.MaxValue)return "sand-piece-limit";
            if(c.value=="scoop"){if(mould.built || mould.scoops==mould.capacity)return "bucket-full";mould.scoops++;mould.version++;return null;}
            if(c.value=="water"){if(mould.built)return "tower-built";mould.wet=true;mould.version++;return null;}
            if(c.value=="tip"){if(mould.built)return "tower-built";if(mould.scoops!=mould.capacity)return "fill-bucket-first";if(!mould.wet)return "add-water-first";mould.built=true;mould.version++;return null;}
            // Existing saved decorations and old valid commands retain their meaning.
            if((c.value=="flag" || c.value=="shell") && mould.built){var decoration=c.value=="flag"?1:2;if(mould.decoration!=decoration){mould.decoration=decoration;mould.version++;}return null;}
            return "try-sand-tools";
        }
        private bool AdvanceSandpit(double seconds,string[] activePlayers,out bool visible)
        {
            visible=false;var g=state.sandpit;if(g==null || g.phase!=1)return false;
            if(!g.members.Any(m=>m.attending && (activePlayers==null || activePlayers.Contains(m.actor)))){g.started+=seconds;return true;}
            if(DaycareSandpit.Demonstration(g,state.daycare.clock)<10.6)return false;
            g.phase=2;visible=true;return true;
        }
    }
}
