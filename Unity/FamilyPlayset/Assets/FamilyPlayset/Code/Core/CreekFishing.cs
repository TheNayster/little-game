using System;
using System.Linq;
namespace LittleWeeps.Core
{
    // Habitat geometry changes; the pond's authority rules and fish identities do not.
    public sealed class FishingHabitat
    {
        public readonly string zone;
        public readonly float x,bankY,bankSpacing,radiusX,radiusY,castSpacing;
        public readonly int fishCount;
        public FishingHabitat(string zone,float x,float bankY,float bankSpacing,float radiusX,float radiusY,int fishCount,float castSpacing=0)
        {this.zone=zone;this.x=x;this.bankY=bankY;this.bankSpacing=bankSpacing;this.radiusX=radiusX;this.radiusY=radiusY;this.fishCount=fishCount;this.castSpacing=castSpacing>0?castSpacing:bankSpacing*.706f;}
        public float BankX(int slot)=>x+(slot-1.5f)*bankSpacing;
        public bool Target(float px,float py)=>KeepyRules.Finite(px) && KeepyRules.Finite(py) && px*px/(radiusX*radiusX)+py*py/(radiusY*radiusY)<=.82;
    }
    public static class CreekFishing
    {
        public const int Schema=38,FishCount=20;
        public const float X=3600,BankY=180,WaterY=330,WaterLift=0,RadiusX=920,RadiusY=28;
        public static readonly FishingHabitat Habitat=new FishingHabitat("creek",X,BankY,260,RadiusX,RadiusY,FishCount);
        public static float BankX(int slot)=>Habitat.BankX(slot);
        public static bool Target(float x,float y)=>Habitat.Target(x,y);
    }
    public sealed partial class SoloWorld
    {
        public PondState ReadCreekFishing()=>state.creekFishing?.Copy();
        public static SoloWorld WithCreekFishing(SoloWorld world)
        {
            world=WithCreekBoats(world);if(world.Schema>=CreekFishing.Schema)return world;
            var s=world.Snapshot();uint seed=2166136261;
            foreach(var ch in s.worldId+"/creek-fishing")seed=unchecked((seed^ch)*16777619);
            s.creekFishing=new PondState{random=seed|1u,fish=Enumerable.Range(0,CreekFishing.FishCount).Select(i=>new PondFish{id=i,random=(seed^(uint)(i+1)*2654435761u)|1u}).ToArray(),rods=s.players.Select(p=>new PondRod{actor=p.id}).ToArray()};
            foreach(var f in s.creekFishing.fish)PondFishing.Swim(f,0,CreekFishing.Habitat);
            s.schema=CreekFishing.Schema;s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void NormalizeCreekFishing(SoloSnapshot s)
        {
            var p=s?.creekFishing;
            if(s!=null && s.schema<CreekFishing.Schema && p!=null && p.clock==0 && p.random==0 && p.nextFood==0 &&
                (p.fish==null || p.fish.Length==0) && (p.rods==null || p.rods.Length==0) && (p.food==null || p.food.Length==0))s.creekFishing=null;
        }
        private bool CancelCreekFishing(string actor)=>CancelFishing(actor,state.creekFishing,CreekFishing.Habitat);
        public bool ReleaseCreekFishing(string actor){if(!CancelCreekFishing(actor))return false;state.revision++;return true;}
        private void AfterCreekFishingAction(SoloCommand c,SoloPlayer player)
        {
            if(c.action==SoloAction.CreekFishing || c.action==SoloAction.ChangeAvatar || c.action==SoloAction.ChangeOutfit || c.action==SoloAction.Roar)return;
            if(c.action==SoloAction.Move || c.action==SoloAction.Travel || c.action==SoloAction.UseFixture || c.action==SoloAction.UseStairs || c.action==SoloAction.EnterDoor || c.action==SoloAction.StartActivity || c.action==SoloAction.HideAndSeek || c.action==SoloAction.Grab)CancelCreekFishing(player.id);
        }
    }
}
