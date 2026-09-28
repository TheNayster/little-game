using System;
using System.Linq;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class HomeIdleTimer
    {
        public string key;
        public double seconds;
        public HomeIdleTimer Copy()=>(HomeIdleTimer)MemberwiseClone();
    }
    public static class HomeTidying
    {
        public const int Schema=22;
        public const double IdleSeconds=300, CueSeconds=5;
        public static string Item(string id)=>"item/"+id;
        public static string Lab(string owner,string lab)=>"lab/"+owner+"/"+lab;
        internal static readonly Dictionary<string,SoloToy> Origins=Kitchen.Stock().Concat(new[]{CakeFlow.MixStock()}).ToDictionary(t=>t.id);
        public static readonly string[] Labs={"float","magnets","lights","mix0","mix1","mix2","mix3","ice","bubble","liquid"};
        public static string[] Keys(SoloSnapshot s)=>s.toys.Where(t=>Kitchen.Kind(t.kind) || t.kind==ToyKind.Book).Select(t=>Item(t.id)).Concat(new[]{"kitchen","balloon"}).Concat(s.players.SelectMany(p=>Labs.Select(l=>Lab(p.id,l)))).OrderBy(k=>k,StringComparer.Ordinal).ToArray();
        public static string[] CueKeys(SoloSnapshot view){var keys=Keys(view);return (view.homeTidyCues??Array.Empty<int>()).Where(i=>i>=0 && i<keys.Length).Select(i=>keys[i]).ToArray();}
        public static string LabAction(string op)
        {
            if(op.StartsWith("visit:") && Labs.Contains(op.Substring(6)))return op.Substring(6);
            if(op.StartsWith("liquid:"))return "liquid";
            if(op.StartsWith("bubble:"))return "bubble";
            if(op.StartsWith("ice:"))return "ice";
            if(op.StartsWith("mix:")){var parts=op.Split(':');return parts.Length>2?"mix"+parts[1]:"";}
            if(new[]{"cargo-add","cargo-remove","narrow","wide","lift","reset-float"}.Contains(op))return "float";
            if(new[]{"red","green","blue","reset-lights"}.Contains(op))return "lights";
            if(new[]{"magnet","iron","wood","plastic","aluminum","reset-magnets"}.Contains(op))return "magnets";
            return ""; // Coloring and its undo history are personal creations.
        }
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithHomeTidying(SoloWorld world)
        {
            world=WithLiquidColors(world);if(world.Schema>=HomeTidying.Schema)return world;
            var s=world.Snapshot();s.schema=HomeTidying.Schema;s.revision++;
            // Existing placements and creations get a full grace period, never a
            // wall-clock catch-up or a reset merely because the save was upgraded.
            s.homeIdleTimers=Array.Empty<HomeIdleTimer>();Validate(s);return new SoloWorld(s);
        }
        public string[] ReadTidyCues()=>state.homeIdleTimers.Where(t=>t.seconds>=HomeTidying.IdleSeconds).Select(t=>t.key).ToArray();
        private static void ValidateHomeTidying(SoloSnapshot s)
        {
            var timers=s.homeIdleTimers??Array.Empty<HomeIdleTimer>();
            if(s.schema<HomeTidying.Schema){if(timers.Length>0 || (s.homeTidyCues?.Length??0)>0)throw new InvalidOperationException("Home tidying requires schema 22.");return;}
            var keys=new HashSet<string>(s.toys.Where(t=>Kitchen.Kind(t.kind) || t.kind==ToyKind.Book).Select(t=>HomeTidying.Item(t.id)));
            keys.Add("kitchen");keys.Add("balloon");
            foreach(var p in s.players)foreach(var lab in HomeTidying.Labs)keys.Add(HomeTidying.Lab(p.id,lab));
            var cues=s.homeTidyCues??Array.Empty<int>();
            if(cues.Any(i=>i<0 || i>=keys.Count) || cues.Distinct().Count()!=cues.Length)throw new InvalidOperationException("Invalid Home tidying cues.");
            var seen=new HashSet<string>();
            foreach(var t in timers)if(t==null || !keys.Contains(t.key??"") || !seen.Add(t.key) || double.IsNaN(t.seconds) || double.IsInfinity(t.seconds) || t.seconds<0 || t.seconds>HomeTidying.IdleSeconds+HomeTidying.CueSeconds)
                throw new InvalidOperationException("Invalid Home inactivity timer.");
        }
        private void TouchTidy(string key)
        {state.homeIdleTimers=state.homeIdleTimers.Where(t=>t.key!=key).ToArray();}
        private void TouchHomeAction(SoloCommand c,SoloPlayer p)
        {
            if(state.schema<HomeTidying.Schema)return;
            if(c.action==SoloAction.Discovery){var lab=HomeTidying.LabAction(c.value);if(lab!="")TouchTidy(HomeTidying.Lab(p.id,lab));}
            if(c.action==SoloAction.Kitchen || (c.action==SoloAction.Drop || c.action==SoloAction.Grab) && state.toys.Any(t=>Kitchen.Kind(t.kind) && (t.id==c.item || t.id==c.target)))
            {
                TouchTidy("kitchen");
                foreach(var t in state.toys.Where(t=>Kitchen.Kind(t.kind) && (t.id==c.item || t.id==c.target)))TouchTidy(HomeTidying.Item(t.id));
            }
        }
        private bool AdvanceHomeTidying(double seconds,out bool visible)
        {
            visible=false;if(state.schema<HomeTidying.Schema)return false;
            var changed=false;var show=false;
            void Age(string key,bool eligible,Action reset)
            {
                var timer=state.homeIdleTimers.FirstOrDefault(t=>t.key==key);
                if(!eligible){if(timer!=null){show|=timer.seconds>=HomeTidying.IdleSeconds;TouchTidy(key);changed=true;}return;}
                if(timer==null){timer=new HomeIdleTimer{key=key};state.homeIdleTimers=state.homeIdleTimers.Concat(new[]{timer}).ToArray();}
                var wasPending=timer.seconds>=HomeTidying.IdleSeconds;
                timer.seconds=Math.Min(HomeTidying.IdleSeconds+HomeTidying.CueSeconds,timer.seconds+seconds);changed=true;
                if(timer.seconds>=HomeTidying.IdleSeconds+HomeTidying.CueSeconds){reset();TouchTidy(key);show=true;}
                else if(!wasPending && timer.seconds>=HomeTidying.IdleSeconds)show=true;
            }
            // Return the existing instance. Food, ingredient counts and batches
            // remain intact; occupied supports defer cleanup instead of stacking.
            var origins=HomeTidying.Origins;
            foreach(var t in state.toys.Where(t=>Kitchen.Kind(t.kind) || t.kind==ToyKind.Book))
            {
                string anchor;float x,y;
                if(t.kind==ToyKind.Book){var i=HomeBooks.Index(t.id);anchor=HomeBooks.Support(i);x=HomeBooks.X(i);y=HomeBooks.Y(i);}
                else {var origin=origins[t.id];anchor=origin.container;Kitchen.Slot(anchor,out var group,out var slot);x=Kitchen.X(group,slot,state.schema);y=Kitchen.Y(group,slot);}
                var heating=Kitchen.Heating(t);
                var away=t.zone!="garden" || t.container!=anchor || t.x!=x || t.y!=y;
                var emptyDirty=t.kitchen!=null && t.kitchen.dirty && (t.kitchen.dish==null || t.kitchen.dish.portions==0);
                var free=!state.toys.Any(o=>o!=t && o.container==anchor);
                Age(HomeTidying.Item(t.id),(away || emptyDirty) && t.holder=="" && !heating && free,()=>{
                    t.zone="garden";t.container=anchor;t.x=x;t.y=y;
                    if(emptyDirty){t.kitchen.dish=null;t.kitchen.dirty=false;t.kitchen.cook="";}
                });
            }
            var open=state.kitchen.fridgeOpen || state.kitchen.ovenOpen || state.kitchen.waterOn || state.kitchen.cupboards.Any(v=>v);
            Age("kitchen",open && !state.toys.Any(t=>Kitchen.Kind(t.kind) && t.holder!=""),()=>{state.kitchen.fridgeOpen=state.kitchen.ovenOpen=state.kitchen.waterOn=false;Array.Clear(state.kitchen.cupboards,0,4);});
            var b=state.keepy;
            Age("balloon",b.phase==0 && (b.x!=KeepyRules.SpawnX || b.y!=KeepyRules.SpawnY),()=>{b.x=b.centerX=KeepyRules.SpawnX;b.y=KeepyRules.SpawnY;});
            foreach(var w in state.discovery)
            {
                Age(HomeTidying.Lab(w.owner,"float"),w.cargo!=0 || w.wide || w.outOfWater,()=>{w.cargo=0;w.wide=false;w.outOfWater=false;});
                Age(HomeTidying.Lab(w.owner,"magnets"),w.magnetX!=400 || w.magnetY!=110 || w.ironX!=140 || w.ironY!=315,()=>{w.magnetX=400;w.magnetY=110;w.ironX=140;w.ironY=315;});
                Age(HomeTidying.Lab(w.owner,"lights"),w.lights!=0,()=>w.lights=0);
                for(var mode=0;mode<Mixing.Modes;mode++){
                    var index=mode;var t=w.mixtures[index];
                    Age(HomeTidying.Lab(w.owner,"mix"+index),t.revision<long.MaxValue-1 && (Mixing.Total(t)>0 || t.spill) && t.reaction==0 && t.foam==0 && t.stir==0 && t.poke==0,()=>w.mixtures[index]=new MixingTray{revision=t.revision+1,volcano=t.volcano});
                }
                var ice=w.ice[0];
                Age(HomeTidying.Lab(w.owner,"ice"),ice.revision<long.MaxValue-1 && (ice.current.cells.Any(v=>v!=1) || ice.current.x!=500 || ice.current.y!=330) && ice.current.energy.All(v=>v==0),()=>{ice.previous=new[]{ice.current.Copy()};ice.current=new IceRescueState{toy=ice.current.toy};ice.revision++;});
                var bubble=w.bubbles[0];var bs=bubble.current;
                Age(HomeTidying.Lab(w.owner,"bubble"),bubble.revision<long.MaxValue-1 && bs.water && bs.floating.Length==0,()=>{bubble.previous=new[]{bs.Copy()};bubble.current=new BubbleState{big=bs.big,square=bs.square,strong=bs.strong};bubble.revision++;});
                var liquid=w.liquid[0];
                Age(HomeTidying.Lab(w.owner,"liquid"),liquid.revision<long.MaxValue-1 && LiquidColorLab.Volume(liquid.current)>0,()=>{liquid.previous=new[]{liquid.current.Copy()};liquid.current=new LiquidColorState();liquid.revision++;});
            }
            visible=show;return changed;
        }
    }
}
