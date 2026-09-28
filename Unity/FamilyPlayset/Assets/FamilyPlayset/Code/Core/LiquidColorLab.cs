using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class LiquidColorLab
    {
        public const int Schema=21,Capacity=12;
        public static readonly string[] Names={"Red","Yellow","Blue"};
        private static readonly int[,] Anchors={{245,247,237},{64,130,220},{248,212,55},{60,172,113},{231,67,88},{153,90,193},{242,137,52},{125,101,80}};
        public static int Volume(LiquidColorState s)=>s.water+s.parts.Sum();
        public static string Token(LiquidColorTray t)=>"colors@"+t.revision;
        // Authored RYB interpolation matches the reviewed prototype. These are
        // liquid/pigment approximations, not additive lights or spectral dye data.
        public static int[] Color(LiquidColorState s)
        {
            var peak=s.parts.Max();if(peak==0)return new[]{226,243,246};
            var r=s.parts[0]/(double)peak;var y=s.parts[1]/(double)peak;var b=s.parts[2]/(double)peak;var rgb=new double[3];
            for(var i=0;i<8;i++){var weight=((i&4)!=0?r:1-r)*((i&2)!=0?y:1-y)*((i&1)!=0?b:1-b);for(var c=0;c<3;c++)rgb[c]+=Anchors[i,c]*weight;}
            var dilution=s.parts.Sum()/(double)Volume(s);var clear=new[]{238,247,247};
            return rgb.Select((v,i)=>(int)Math.Floor(v*dilution+clear[i]*(1-dilution)+.5)).ToArray();
        }
        public static string Name(LiquidColorState s)
        {
            var active=Enumerable.Range(0,3).Where(i=>s.parts[i]>0).ToArray();
            if(active.Length==0)return s.water>0?"Clear water":"Choose two colors";
            if(active.Length==3)return s.parts.Min()/(double)s.parts.Max()>=.65?"Earthy brown":"Mixed colors";
            if(active.Length==1)return Names[active[0]];
            return active.Contains(0)?active.Contains(1)?"Orange":"Purple":"Green";
        }
        public static void Validate(LiquidColorTray t)
        {
            void State(LiquidColorState s){if(s==null || s.parts==null || s.parts.Length!=3 || s.parts.Any(v=>v<0 || v>Capacity) || s.water<0 || s.water>Capacity || Volume(s)>Capacity)throw new InvalidOperationException("Invalid liquid color mixture.");}
            if(t==null || t.revision<0 || t.revision>=long.MaxValue || t.previous==null || t.previous.Length>1)throw new InvalidOperationException("Invalid liquid color tray.");
            State(t.current);if(t.previous.Length>0)State(t.previous[0]);
        }
    }
    [Serializable] public sealed class LiquidColorState
    {
        public int[] parts=new int[3];
        public int water;
        public LiquidColorState Copy()=>new LiquidColorState{parts=(int[])parts.Clone(),water=water};
    }
    [Serializable] public sealed class LiquidColorTray
    {
        public long revision;
        public LiquidColorState current=new LiquidColorState();
        public LiquidColorState[] previous=Array.Empty<LiquidColorState>();
        public LiquidColorTray Copy()=>new LiquidColorTray{revision=revision,current=current.Copy(),previous=previous.Select(s=>s.Copy()).ToArray()};
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithLiquidColors(SoloWorld world)
        {
            world=WithBubbleLab(world);if(world.Schema>=LiquidColorLab.Schema)return world;
            var s=world.Snapshot();s.schema=LiquidColorLab.Schema;s.revision++;
            foreach(var w in s.discovery)w.liquid=new[]{new LiquidColorTray()};
            Validate(s);return new SoloWorld(s);
        }
        private string LiquidColorOperation(SoloCommand c,DiscoveryWorkspace w)
        {
            if(state.schema<LiquidColorLab.Schema)return "color-upgrade-needed";
            var t=w.liquid[0];if(c.target!=LiquidColorLab.Token(t) || t.revision>=long.MaxValue-1)return "colors-changed";
            var s=t.current.Copy();var op=c.value.Substring(7);
            if(op=="undo"){
                if(t.previous.Length==0)return "nothing-to-undo";
                s=t.previous[0].Copy();t.previous=Array.Empty<LiquidColorState>();
            }else{
                switch(op){
                    case "red":case "yellow":case "blue":case "water":
                        if(LiquidColorLab.Volume(s)>=LiquidColorLab.Capacity)return "color-beaker-full";
                        if(op=="water")s.water++;else s.parts[op=="red"?0:op=="yellow"?1:2]++;break;
                    case "again":if(LiquidColorLab.Volume(s)==0)return "color-beaker-empty";s=new LiquidColorState();break;
                    default:return "unknown-color-action";
                }
                t.previous=new[]{t.current.Copy()};
            }
            t.current=s;t.revision++;return null;
        }
    }
}
