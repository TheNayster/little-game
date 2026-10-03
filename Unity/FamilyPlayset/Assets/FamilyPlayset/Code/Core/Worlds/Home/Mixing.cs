using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Units are reviewed toy portions, not a real-world laboratory recipe.
    public static class Mixing
    {
        public const int Schema=17, Modes=4, Ingredients=10, Capacity=64;
        public static readonly string[] Names={"Fizz & foam","Color changing","Oil & water","Squishy oobleck"};
        public static readonly string[] Labels={"Vinegar","Baking soda","Water","Soap","Cornstarch","Cabbage liquid","Oil","Red drops","Blue drops","Yellow drops"};
        public static readonly int[][] Supplies={new[]{0,1,2,3,7,8},new[]{5,0,1,2},new[]{2,6,7,8,9},new[]{4,2,7,8,9}};
        public static MixingTray[] Create()=>Enumerable.Range(0,Modes).Select(_=>new MixingTray()).ToArray();
        public static string Token(int mode,MixingTray t)=>"mix:"+mode+"@"+t.revision;
        public static int Total(MixingTray t)=>t.amounts.Sum();
        public static int Water(MixingTray t)=>t.amounts[0]+t.amounts[2]+t.amounts[5]+t.amounts[7]+t.amounts[8]+t.amounts[9];
        public static bool Oobleck(MixingTray t)=>t.amounts[4]>=2 && Water(t)>=1 && (float)t.amounts[4]/Water(t)>=1.2f && (float)t.amounts[4]/Water(t)<=3.2f;
        public static string Result(int mode,MixingTray t)
        {
            if(Total(t)==0)return mode==0?"Scoop baking soda, then pour vinegar!":mode==1?"Start with purple cabbage liquid.":mode==2?"Pour water and oil. Add a colored drop!":"Scoop cornstarch, then add a little water.";
            if(mode==0)return t.reaction>0?(t.amounts[3]>0?"Fizzy bubbles are making fluffy foam!":"The ingredients meet and make gas bubbles!"):t.reacted>0?"The fizz has settled. Try another portion or rinse.":"Try adding "+(t.amounts[0]>0?"baking soda.":"vinegar to baking soda.");
            if(mode==1)return t.amounts[5]==0?"Add cabbage liquid to see a color change.":t.amounts[0]>t.amounts[1]?"Vinegar turns the cabbage liquid pink.":t.amounts[1]>t.amounts[0]?"Baking soda turns the cabbage liquid blue-green.":"Your cabbage liquid is purple.";
            if(mode==2)return t.amounts[6]>0 && Water(t)>0?(t.stir>0?"Stirred droplets slowly separate again.":"Oil floats above the colored water."):"Try water, oil and a colored drop.";
            return Oobleck(t)?(t.poke>0?"A quick tap meets resistance!":"Move slowly to make it flow; tap quickly to test it."):t.amounts[4]==0?"Add cornstarch to make a squishy mixture.":Water(t)==0 || t.amounts[4]>Water(t)*3.2f?"Still powdery. Add a little water.":"Quite runny! Try another scoop of cornstarch.";
        }
        public static void Validate(MixingTray t,int mode)
        {
            bool Time(double x,double max)=>!double.IsNaN(x) && !double.IsInfinity(x) && x>=0 && x<=max;
            if(t==null || t.revision<0 || t.revision>=long.MaxValue || t.amounts==null || t.amounts.Length!=Ingredients || t.amounts.Any(n=>n<0 || n>32) || Total(t)>Capacity ||
                Enumerable.Range(0,Ingredients).Any(i=>t.amounts[i]>0 && !Supplies[mode].Contains(i)) || t.reacted<0 || t.reacted>Math.Min(t.amounts[0],t.amounts[1]) ||
                !Time(t.reaction,12) || !Time(t.foam,24) || !Time(t.stir,8) || !Time(t.poke,1) || (t.reaction>0 && t.reacted==0) || (t.foam>0 && (t.reacted==0 || t.amounts[3]==0)) ||
                (mode>1 && (t.reacted!=0 || t.reaction!=0 || t.foam!=0)) || (mode!=0 && (t.volcano || t.spill)))
                throw new InvalidOperationException("Invalid mixing tray.");
        }
    }
    [Serializable] public sealed class MixingTray
    {
        public long revision;
        public int[] amounts=new int[Mixing.Ingredients];
        public int reacted;
        public double reaction,foam,stir,poke;
        public bool volcano,spill;
        public MixingTray Copy(){var c=(MixingTray)MemberwiseClone();c.amounts=(int[])amounts.Clone();return c;}
    }
    public sealed partial class GameWorld
    {
        public static GameWorld WithMixing(GameWorld world)
        {
            world=WithDiscovery(world);if(world.Schema>=Mixing.Schema)return world;
            var s=world.Snapshot();s.schema=Mixing.Schema;s.revision++;
            foreach(var w in s.discovery)w.mixtures=Mixing.Create();
            Validate(s);return new GameWorld(s);
        }
        private string MixingOperation(SoloCommand c,DiscoveryWorkspace w)
        {
            if(state.schema<Mixing.Schema)return "mixing-upgrade-needed";
            var args=c.value.Split(':');
            if(args.Length<3 || !int.TryParse(args[1],out var mode) || mode<0 || mode>=Mixing.Modes)return "invalid-mixing-mode";
            var t=w.mixtures[mode];
            if(c.target!=Mixing.Token(mode,t) || t.revision>=long.MaxValue-1)return "mixture-changed";
            var op=args[2];
            if(op=="add")
            {
                if(args.Length!=4 || !int.TryParse(args[3],out var ingredient) || !Mixing.Supplies[mode].Contains(ingredient) || float.IsNaN(c.x) || float.IsInfinity(c.x) || c.x<1 || c.x>4 || c.x!=(int)c.x)return "invalid-mixing-ingredient";
                var amount=(int)c.x;if(t.amounts[ingredient]+amount>32 || Mixing.Total(t)+amount>Mixing.Capacity)return "mixing-full";
                t.amounts[ingredient]+=amount;
                if(mode<=1){
                    var produced=Math.Min(t.amounts[0],t.amounts[1])-t.reacted;
                    if(produced>0){t.reacted+=produced;t.reaction=Math.Min(12,t.reaction+produced*2.4);}
                    if(t.reaction>0 && t.amounts[3]>0)t.foam=Math.Min(24,Math.Max(t.foam,t.reaction+6));
                    if(mode==0 && t.amounts[3]>0 && t.reacted>=4 && t.reaction>0)t.spill=true;
                }
            }
            else
            {
                if(args.Length!=3)return "invalid-mixing-action";
                switch(op){
                    case "rinse":w.mixtures[mode]=new MixingTray{revision=t.revision+1,volcano=t.volcano};return null;
                    case "wipe":t.spill=false;break;
                    case "stir":if(Mixing.Total(t)==0)return "mixing-empty";t.stir=8;break;
                    case "poke":if(mode!=3 || Mixing.Total(t)==0)return "mixing-empty";t.poke=1;break;
                    case "vessel":if(mode!=0)return "invalid-mixing-action";t.volcano=!t.volcano;break;
                    default:return "invalid-mixing-action";
                }
            }
            t.revision++;return null;
        }
        private bool AdvanceMixing(double seconds,out bool visible)
        {
            visible=false;if(state.schema<Mixing.Schema)return false;var changed=false;var publish=false;
            foreach(var w in state.discovery)foreach(var t in w.mixtures){
                // Publish at most twice per second per phase, plus the settled
                // transition. Clients animate locally; no per-bubble messages.
                double Decay(double v){var next=Math.Max(0,v-seconds);if(next!=v)changed=true;if(Math.Ceiling(next*2)!=Math.Ceiling(v*2))publish=true;return next;}
                t.reaction=Decay(t.reaction);t.foam=Decay(t.foam);t.stir=Decay(t.stir);t.poke=Decay(t.poke);
            }
            visible=publish;return changed;
        }
    }
}
