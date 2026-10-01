using System;
using System.Linq;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    public static class DaycareNpcCasts
    {
        public const int Schema=40;
        private static string Family(string avatar)=>avatar.StartsWith("terrier-")?"terriers":PlayableCharacters.Find(avatar)?.ArtId;
        public static bool Valid(string[] cast,int count)=>cast!=null && cast.Length==count && cast.All(id=>PlayableCharacters.Contains(id) && id!="bandit" && id!="chilli") && cast.Select(Family).Distinct().Count()==count;
        public static string[] Pick(int count,string[] previous=null)
        {
            // Only the authority draws a cast. Player avatars never affect the
            // pool; avoid last round's NPC faces and save the actual shared IDs.
            var excluded=(previous??Array.Empty<string>()).Select(Family).ToHashSet();
            var pool=PlayableCharacters.All.Where(e=>e.AvatarId!="bandit" && e.AvatarId!="chilli" && !excluded.Contains(Family(e.AvatarId))).Select(e=>e.AvatarId).ToArray();
            var random=new Random(Guid.NewGuid().GetHashCode());
            for(var i=pool.Length-1;i>0;i--){var j=random.Next(i+1);var temp=pool[i];pool[i]=pool[j];pool[j]=temp;}
            var cast=new List<string>();var families=new HashSet<string>();
            foreach(var id in pool)if(families.Add(Family(id))){cast.Add(id);if(cast.Count==count)break;}
            if(!Valid(cast.ToArray(),count))throw new InvalidOperationException("Insufficient prepared Daycare NPC variety.");
            return cast.ToArray();
        }
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithNpcCasts(SoloWorld world)
        {
            world=WithDaycare(world);if(world.Schema>=DaycareNpcCasts.Schema && DaycareNpcCasts.Valid(world.state.kingdom?.npcCast,9) && DaycareNpcCasts.Valid(world.state.daycare?.guests,4))return world;
            var s=world.Snapshot();s.kingdom.npcCast=DaycareNpcCasts.Pick(9);s.daycare.guests=DaycareNpcCasts.Pick(4);
            s.schema=Math.Max(s.schema,DaycareNpcCasts.Schema);s.revision++;Validate(s);return new SoloWorld(s);
        }
        private static void ValidateNpcCasts(SoloSnapshot s)
        {if(s.schema>=WorldLayout.Schema && (!DaycareNpcCasts.Valid(s.kingdom?.npcCast,9) || !DaycareNpcCasts.Valid(s.daycare?.guests,4)))throw new InvalidOperationException("Invalid saved Daycare NPC cast.");}
    }
}
