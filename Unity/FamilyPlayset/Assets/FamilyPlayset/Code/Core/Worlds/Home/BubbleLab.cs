using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class BubbleLab
    {
        public const int Schema=20, Capacity=18, Lifetime=14;
        public static string Token(BubbleTray t)=>"bubbles@"+t.revision;
        public static string Next(BubbleState s)=>!s.water?"water":!s.soap?"soap":!s.mixed?"stir":s.film>0?(s.floating.Length+(s.big?1:3)>Capacity?"pop":"blow"):s.solution>0?"dip":"refill";
        public static double X(SoapBubble b,double clock)=>785-b.slot*28-38*b.air*(1-Math.Exp(-.2*Math.Max(0,clock-b.born)))/.2;
        public static double Y(SoapBubble b,double clock){var age=Math.Max(0,clock-b.born);return 245-b.slot*12-(22+10*b.slot)*age-age*age;}
        public static bool Alive(SoapBubble b,double clock)=>clock-b.born<Lifetime && Y(b,clock)+b.radius>80;
        public static void Validate(BubbleTray t)
        {
            bool Number(double v,double min,double max)=>!double.IsNaN(v) && !double.IsInfinity(v) && v>=min && v<=max;
            void State(BubbleState s){
                if(s==null || !Number(s.clock,0,1e9) || s.solution<0 || s.solution>16 || s.film<0 || s.film>4 || s.mixed && (!s.water || !s.soap) || !s.mixed && (s.solution!=0 || s.film!=0) || s.soap && !s.water || s.floating==null || s.floating.Length>Capacity || s.floating.Any(b=>b==null) || s.floating.Select(b=>b.id).Distinct().Count()!=s.floating.Length)
                    throw new InvalidOperationException("Invalid bubble mixture.");
                foreach(var b in s.floating)if(b.id<1 || b.id>t.serial || !Number(b.born,0,s.clock) || s.clock-b.born>Lifetime || b.slot<0 || b.slot>2 || b.air<1 || b.air>2 || !(b.radius==70+8*b.air || b.radius==22+9*b.air+(b.slot*7)%15))throw new InvalidOperationException("Invalid soap bubble.");
            }
            if(t==null || t.revision<0 || t.revision>=long.MaxValue || t.serial<0 || t.serial>1000000000 || t.previous==null || t.previous.Length>1)throw new InvalidOperationException("Invalid bubble tray.");
            State(t.current);if(t.previous.Length>0)State(t.previous[0]);
        }
    }
    [Serializable] public sealed class SoapBubble
    {
        public int id,slot,air,radius;
        public double born;
        public SoapBubble Copy()=>(SoapBubble)MemberwiseClone();
    }
    [Serializable] public sealed class BubbleState
    {
        public bool water,soap,mixed,big,square,strong;
        public int solution,film;
        public double clock;
        public SoapBubble[] floating=Array.Empty<SoapBubble>();
        public BubbleState Copy(){var c=(BubbleState)MemberwiseClone();c.floating=floating.Select(b=>b.Copy()).ToArray();return c;}
    }
    [Serializable] public sealed class BubbleTray
    {
        public long revision;
        // IDs never rewind on undo/reset, so late pop messages cannot hit a new bubble.
        public int serial;
        public BubbleState current=new BubbleState();
        public BubbleState[] previous=Array.Empty<BubbleState>();
        public BubbleTray Copy()=>new BubbleTray{revision=revision,serial=serial,current=current.Copy(),previous=previous.Select(s=>s.Copy()).ToArray()};
    }
    public sealed partial class GameWorld
    {
        public static GameWorld WithBubbleLab(GameWorld world)
        {
            world=WithIceRescue(world);if(world.Schema>=BubbleLab.Schema)return world;
            var s=world.Snapshot();s.schema=BubbleLab.Schema;s.revision++;
            foreach(var w in s.discovery)w.bubbles=new[]{new BubbleTray()};
            Validate(s);return new GameWorld(s);
        }
        private string BubbleOperation(SoloCommand c,DiscoveryWorkspace w)
        {
            if(state.schema<BubbleLab.Schema)return "bubble-upgrade-needed";
            var t=w.bubbles[0];if(c.target!=BubbleLab.Token(t) || t.revision>=long.MaxValue-1)return "bubbles-changed";
            var s=t.current.Copy();var op=c.value.Substring(7);var serial=t.serial;
            if(op=="undo"){
                if(t.previous.Length==0)return "nothing-to-undo";
                s=t.previous[0].Copy();t.previous=Array.Empty<BubbleState>();
            }else{
                switch(op){
                    case "water":if(s.water)return "water-already-added";s.water=true;break;
                    case "soap":if(!s.water || s.soap)return "need-water";s.soap=true;break;
                    case "stir":if(!s.water || !s.soap || s.mixed)return "need-soap-and-water";s.mixed=true;s.solution=16;break;
                    case "dip":if(!s.mixed || s.solution<=0 || s.film>0)return "need-mixture";s.solution--;s.film=4;break;
                    case "blow":
                        if(!s.mixed || s.film<=0)return "dip-wand-first";
                        var count=s.big?1:3;if(s.floating.Length+count>BubbleLab.Capacity || serial>1000000000-count)return "bubble-tray-full";
                        var air=s.strong?2:1;
                        s.floating=s.floating.Concat(Enumerable.Range(0,count).Select(i=>new SoapBubble{id=++serial,slot=i,air=air,radius=s.big?70+8*air:22+9*air+(i*7)%15,born=s.clock})).ToArray();
                        s.film=Math.Max(0,s.film-(s.big?2:1));break;
                    case "refill":
                        if(s.solution!=0 || s.film!=0)return "mixture-remains";
                        s.water=s.soap=s.mixed=false;break;
                    case "size":s.big=!s.big;break;
                    case "shape":s.square=!s.square;break;
                    case "air":s.strong=!s.strong;break;
                    case "again":s=new BubbleState{big=s.big,square=s.square,strong=s.strong};break;
                    default:
                        if(!op.StartsWith("pop:") || !int.TryParse(op.Substring(4),out var id) || !s.floating.Any(b=>b.id==id))return "bubble-gone";
                        s.floating=s.floating.Where(b=>b.id!=id).ToArray();break;
                }
                t.previous=new[]{t.current.Copy()};
            }
            t.current=s;t.serial=serial;t.revision++;return null;
        }
        private bool AdvanceBubbleLab(double seconds,out bool visible)
        {
            visible=false;if(state.schema<BubbleLab.Schema)return false;var changed=false;
            foreach(var w in state.discovery){var s=w.bubbles[0].current;if(s.floating.Length==0)continue;
                // Keep persisted clocks bounded without changing any particle age.
                if(s.clock>1e9-BubbleLab.Lifetime-seconds){var offset=s.clock-BubbleLab.Lifetime;s.clock-=offset;foreach(var b in s.floating)b.born-=offset;}
                s.clock+=seconds;changed=true;var count=s.floating.Length;s.floating=s.floating.Where(b=>BubbleLab.Alive(b,s.clock)).ToArray();
                // Only expiry changes the reliable world revision. Clients draw
                // the same analytic paths locally, without per-frame bubble packets.
                if(s.floating.Length!=count)visible=true;
            }
            return changed;
        }
    }
}
