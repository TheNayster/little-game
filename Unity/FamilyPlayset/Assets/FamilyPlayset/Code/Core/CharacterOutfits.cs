using System;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
        // Outfit IDs are independent of avatar IDs. Enable more characters only
        // after their costume sheets are prepared; chooser/save rules are shared.
    public static class CharacterOutfits
    {
        public const int Schema=35;
        public const double RoarSeconds=3.2;
        public sealed class Entry
        {
            public readonly string Id, Name;
            public readonly bool CanRoar;
            public Entry(string id,string name,bool canRoar=false){Id=id;Name=name;CanRoar=canRoar;}
        }
        public static readonly IReadOnlyList<Entry> All=Array.AsReadOnly(new[]{
            new Entry("","Normal clothes"),new Entry("dinosaur","Dinosaur",true)
        });
        public static readonly IReadOnlyList<string> Colors=Array.AsReadOnly(new[]{"pink","blue","green","red"});
        public static Entry Find(string id){foreach(var entry in All)if(entry.Id==id)return entry;return null;}
        public static bool Color(string id){foreach(var color in Colors)if(color==id)return true;return false;}
        public static bool CanRoar(SoloPlayer p)=>Available(p.avatar) && Find(p.outfit??"")?.CanRoar==true;
        public static bool Available(string avatar)=>avatar=="blue-pup" || avatar=="orange-pup";
    }

    public sealed partial class SoloWorld
    {
        private readonly Dictionary<string,double> roarCooldowns=new Dictionary<string,double>();
        public static SoloWorld WithOutfits(SoloWorld world)
        {
            // The standalone wardrobe milestone composes over Park/Home.
            // Additional region migrations can wrap this step during integration.
            world=WithPark(world);if(world.Schema>=CharacterOutfits.Schema)return world;
            var copy=world.Snapshot();copy.schema=CharacterOutfits.Schema;copy.revision++;
            Validate(copy);return new SoloWorld(copy);
        }
        private static void NormalizeOutfits(SoloSnapshot s)
        {
            if(s?.players==null)return;
            // Older Unity JSON omits these additive fields (sometimes as null).
            foreach(var p in s.players)if(p!=null){p.outfit=p.outfit??"";p.outfitColor=p.outfitColor??"green";}
        }
        private static bool ValidOutfit(SoloPlayer p,int schema)=>CharacterOutfits.Find(p.outfit)!=null &&
            CharacterOutfits.Color(p.outfitColor) && (p.outfit=="" || CharacterOutfits.Available(p.avatar)) && p.roar>=0 && (schema>=CharacterOutfits.Schema || p.outfit=="" && p.roar==0);
        private void AdvanceRoarCooldowns(double seconds)
        {foreach(var id in new List<string>(roarCooldowns.Keys))roarCooldowns[id]=Math.Max(0,roarCooldowns[id]-seconds);}
        private string ApplyOutfit(SoloPlayer p,SoloCommand c)
        {
            if(state.schema<CharacterOutfits.Schema)return "outfits-unavailable";
            if(!CharacterOutfits.Available(p.avatar))return "outfits-unavailable";
            if(CharacterOutfits.Find(c.value)==null || !CharacterOutfits.Color(c.target))return "unknown-outfit";
            p.outfit=c.value;p.outfitColor=c.target;return null;
        }
        private string ApplyRoar(SoloPlayer p)
        {
            if(state.schema<CharacterOutfits.Schema || !CharacterOutfits.CanRoar(p))return "not-a-dinosaur";
            if(roarCooldowns.TryGetValue(p.id,out var seconds) && seconds>0)return "roar-resting";
            if(p.roar==int.MaxValue)return "roar-limit";
            p.roar++;roarCooldowns[p.id]=CharacterOutfits.RoarSeconds;return null;
        }
    }
}
