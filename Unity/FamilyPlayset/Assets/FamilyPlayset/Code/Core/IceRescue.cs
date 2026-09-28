using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class IceRescue
    {
        public const int Schema=19, Cells=24;
        public const float ToyMinX=265,ToyMaxX=735,ToyMinY=215,ToyMaxY=350;
        public static string Token(IceRescueTray t)=>"ice@"+t.revision;
        public static float X(int i)=>316+i%6*74;
        public static float Y(int i)=>185+i/6*70;
        public static int Next(IceRescueState s)=>Array.FindIndex(s.cells,v=>v>0);
        public static bool Finite(float v,float min,float max)=>!float.IsNaN(v) && !float.IsInfinity(v) && v>=min && v<=max;
        public static void Settle(IceRescueState s)
        {
            if(s.cells.Sum()>.96f)return;
            s.freed=true;Array.Clear(s.cells,0,Cells);Array.Clear(s.energy,0,Cells);
        }
        public static void Validate(IceRescueTray t)
        {
            void State(IceRescueState s){
                if(s==null || s.toy<0 || s.toy>2 || !Finite(s.x,140,860) || !Finite(s.y,140,470) || s.cells==null || s.energy==null || s.cells.Length!=Cells || s.energy.Length!=Cells || s.cells.Any(v=>!Finite(v,0,1)) || s.energy.Any(v=>!Finite(v,0,2)) || s.freed!=(s.cells.Sum()<=.96f) || s.freed && (s.cells.Any(v=>v!=0) || s.energy.Any(v=>v!=0)))
                    throw new InvalidOperationException("Invalid dinosaur rescue state.");
            }
            if(t==null || t.revision<0 || t.revision>=long.MaxValue)throw new InvalidOperationException("Invalid dinosaur rescue tray.");
            if(t.previous==null || t.previous.Length>1)throw new InvalidOperationException("Invalid rescue undo.");
            State(t.current);if(t.previous.Length==1)State(t.previous[0]);
        }
    }
    [Serializable] public sealed class IceRescueState
    {
        public float[] cells=Enumerable.Repeat(1f,IceRescue.Cells).ToArray(),energy=new float[IceRescue.Cells];
        public int toy;
        public bool freed;
        public float x=500,y=330;
        public IceRescueState Copy(){var c=(IceRescueState)MemberwiseClone();c.cells=(float[])cells.Clone();c.energy=(float[])energy.Clone();return c;}
    }
    [Serializable] public sealed class IceRescueTray
    {
        public long revision;
        public IceRescueState current=new IceRescueState();
        // Empty arrays represent optional records: Unity inline serialization
        // materializes null class fields, which otherwise breaks old saves.
        public IceRescueState[] previous=Array.Empty<IceRescueState>();
        public IceRescueTray Copy()=>new IceRescueTray{revision=revision,current=current.Copy(),previous=previous.Select(v=>v.Copy()).ToArray()};
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithIceRescue(SoloWorld world)
        {
            world=WithColoringCollection(world);if(world.Schema>=IceRescue.Schema)return world;
            var s=world.Snapshot();s.schema=IceRescue.Schema;s.revision++;
            foreach(var w in s.discovery)w.ice=new[]{new IceRescueTray()};
            Validate(s);return new SoloWorld(s);
        }
        private string IceRescueOperation(SoloCommand c,DiscoveryWorkspace w)
        {
            if(state.schema<IceRescue.Schema)return "ice-upgrade-needed";
            var t=w.ice[0];if(c.target!=IceRescue.Token(t) || t.revision>=long.MaxValue-1)return "ice-changed";
            // Edit a copy, then commit: invalid/stale input cannot partly remove ice.
            var next=t.current.Copy();var op=c.value.Substring(4);
            if(op=="undo"){
                if(t.previous.Length==0)return "nothing-to-undo";
                next=t.previous[0].Copy();t.previous=Array.Empty<IceRescueState>();
            }else{
                switch(op){
                    case "chip":case "water-warm":case "water-cool":
                        if(next.freed)return "already-rescued";
                        if(!IceRescue.Finite(c.x,260,740) || !IceRescue.Finite(c.y,125,465))return "outside-ice";
                        var changed=false;
                        for(var i=0;i<IceRescue.Cells;i++){
                            var dx=c.x-IceRescue.X(i);var dy=c.y-IceRescue.Y(i);var distance=Math.Sqrt(dx*dx+dy*dy);
                            if(distance>=108 || next.cells[i]<=0)continue;
                            if(op=="chip"){next.cells[i]=Math.Max(0,next.cells[i]-(distance<52?.62f:.3f));if(next.cells[i]==0)next.energy[i]=0;}
                            else next.energy[i]=Math.Min(2,next.energy[i]+(op=="water-warm"?1.2f:.4f));
                            changed=true;
                        }
                        if(!changed)return "empty-ice";
                        IceRescue.Settle(next);break;
                    case "dinosaur":
                        if(next.cells.Any(v=>v!=1) || next.energy.Any(v=>v!=0))return "rescue-started";
                        next.toy=(next.toy+1)%3;break;
                    case "move":
                        if(!next.freed || !IceRescue.Finite(c.x,IceRescue.ToyMinX,IceRescue.ToyMaxX) || !IceRescue.Finite(c.y,IceRescue.ToyMinY,IceRescue.ToyMaxY))return "invalid-dinosaur-position";
                        next.x=c.x;next.y=c.y;break;
                    case "again":next=new IceRescueState{toy=next.toy};break;
                    default:return "unknown-ice-action";
                }
                t.previous=new[]{t.current.Copy()};
            }
            t.current=next;t.revision++;return null;
        }
        private bool AdvanceIceRescue(double seconds,out bool visible)
        {
            visible=false;if(state.schema<IceRescue.Schema)return false;var changed=false;
            foreach(var w in state.discovery){var s=w.ice[0].current;if(s.freed)continue;
                for(var i=0;i<IceRescue.Cells;i++){
                    if(s.energy[i]<=0 || s.cells[i]<=0)continue;
                    var before=s.cells[i];var used=Math.Min(s.energy[i],(float)seconds*.28f);
                    s.energy[i]=Math.Max(0,s.energy[i]-used);s.cells[i]=Math.Max(0,before-used);if(s.cells[i]==0)s.energy[i]=0;changed=true;
                    // Bounded progress publications; droplets and fragments stay local.
                    if(Math.Ceiling(before*16)!=Math.Ceiling(s.cells[i]*16))visible=true;
                }
                var was=s.freed;IceRescue.Settle(s);if(was!=s.freed)visible=true;
            }
            return changed;
        }
    }
}
