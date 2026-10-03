using UnityEngine;
namespace LittleWeeps.Client
{
    // Keeps saved book/art IDs readable after the resource-folder migration.
    public static class WorldResources
    {
        public static string Resolve(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.StartsWith("BeachArt/", System.StringComparison.Ordinal)) return "Worlds/Beach/Art/" + path.Substring(9);
            if (path.StartsWith("CreekBoatArt/", System.StringComparison.Ordinal)) return "Worlds/Creek/Boats/" + path.Substring(13);
            if (path.StartsWith("ParkArt/", System.StringComparison.Ordinal)) return "Worlds/Park/Art/" + path.Substring(8);
            if (path.StartsWith("ParkTag/", System.StringComparison.Ordinal)) return "Worlds/Park/Tag/" + path.Substring(8);
            if (path.StartsWith("ZooArt/", System.StringComparison.Ordinal)) return "Worlds/Zoo/Art/" + path.Substring(7);
            if (path.StartsWith("ZooAudio/", System.StringComparison.Ordinal)) return "Worlds/Zoo/Audio/" + path.Substring(9);
            if (path.StartsWith("DinosaurWorld/", System.StringComparison.Ordinal)) return "Worlds/Dinosaur/Art/" + path.Substring(14);
            if (path.StartsWith("DinosaurWorldAudio/", System.StringComparison.Ordinal)) return "Worlds/Dinosaur/Audio/" + path.Substring(19);
            if (path.StartsWith("Daycare/", System.StringComparison.Ordinal)) return "Worlds/Daycare/Shared/" + path.Substring(8);
            if (path.StartsWith("Adventure/", System.StringComparison.Ordinal)) return "Worlds/Daycare/StoryAdventure/" + path.Substring(10);
            if (path.StartsWith("Sandpit/", System.StringComparison.Ordinal)) return "Worlds/Daycare/SandcastleClub/" + path.Substring(8);
            if (path.StartsWith("Treasure/", System.StringComparison.Ordinal)) return "Worlds/Daycare/TreasureHunt/" + path.Substring(9);
            if (path.StartsWith("Vet/", System.StringComparison.Ordinal)) return "Worlds/Daycare/AnimalClinic/" + path.Substring(4);
            if (path.StartsWith("HomeArt/", System.StringComparison.Ordinal)) return "Worlds/Home/Art/" + path.Substring(8);
            if (path.StartsWith("BathroomArt/", System.StringComparison.Ordinal)) return "Worlds/Home/Bathroom/" + path.Substring(12);
            if (path.StartsWith("BedroomArt/", System.StringComparison.Ordinal)) return "Worlds/Home/Bedrooms/" + path.Substring(11);
            if (path.StartsWith("Books/", System.StringComparison.Ordinal)) return "Worlds/Home/Books/" + path.Substring(6);
            if (path.StartsWith("Discovery/", System.StringComparison.Ordinal)) return "Worlds/Home/Discovery/" + path.Substring(10);
            if (path.StartsWith("HideAndSeek/", System.StringComparison.Ordinal)) return "Worlds/Home/HideAndSeek/" + path.Substring(12);
            if (path.StartsWith("Kitchen/", System.StringComparison.Ordinal)) return "Worlds/Home/Kitchen/" + path.Substring(8);
            if (path.StartsWith("PondArt/", System.StringComparison.Ordinal)) return "Worlds/Home/Pond/" + path.Substring(8);
            if (path.StartsWith("RoomPlay/", System.StringComparison.Ordinal)) return "Worlds/Home/RoomPlay/" + path.Substring(9);
            if (path.StartsWith("SecretArt/", System.StringComparison.Ordinal)) return "Worlds/Home/SecretRooms/" + path.Substring(10);
            if (path.StartsWith("CharacterArt/", System.StringComparison.Ordinal)) return "Shared/Characters/Art/" + path.Substring(13);
            if (path.StartsWith("CharacterMenu/", System.StringComparison.Ordinal)) return "Shared/Characters/Menu/" + path.Substring(14);
            if (path.StartsWith("CharacterOutfitArt/", System.StringComparison.Ordinal)) return "Shared/Characters/OutfitArt/" + path.Substring(19);
            if (path.StartsWith("CharacterOutfits/", System.StringComparison.Ordinal)) return "Shared/Characters/Outfits/" + path.Substring(17);
            if (path.StartsWith("WorldMenu/", System.StringComparison.Ordinal)) return "Shared/UI/WorldMenu/" + path.Substring(10);
            if (path.StartsWith("SoloNarration/", System.StringComparison.Ordinal)) return "Shared/Audio/Narration/" + path.Substring(14);
            if (path.StartsWith("WorldMusic/", System.StringComparison.Ordinal)) return "Shared/Audio/WorldMusic/" + path.Substring(11);
            if (path.StartsWith("Scenery/", System.StringComparison.Ordinal))
            {
                var name=path.Substring(8);
                var world=name.StartsWith("beach-")?"Beach":name.StartsWith("creek-")?"Creek":name.StartsWith("park-")?"Park":name.StartsWith("zoo-")?"Zoo":name.StartsWith("dinosaur-")?"Dinosaur":name.StartsWith("daycare-") || name.StartsWith("treasure-")?"Daycare":"Home";
                return "Worlds/"+world+"/Scenery/"+name;
            }
            return path;
        }
        public static T Load<T>(string path) where T:Object => Resources.Load<T>(Resolve(path));
        public static T[] LoadAll<T>(string path) where T:Object => Resources.LoadAll<T>(Resolve(path));
        public static ResourceRequest LoadAsync<T>(string path) where T:Object => Resources.LoadAsync<T>(Resolve(path));
    }
}
