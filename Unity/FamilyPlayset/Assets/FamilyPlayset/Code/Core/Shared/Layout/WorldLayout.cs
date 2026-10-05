using System;

namespace LittleWeeps.Core
{
    // Home is an arrival shortcut into the same property as Garden. Keeping
    // its coordinates left of the original garden preserves every saved prop.
    public static class WorldLayout
    {
        // Retain the completed worlds and add the shared animal care clinic.
        // Sandpit construction targets include the lesson round; old clients must not mix.
        public const int Schema=53, ScenerySchema=3, Content=72;
        public const float TileWidth=2400, SceneHeight=800;
        public static bool Area(string id)=>id=="garden" || id=="creek" || id=="park" || id=="beach" || id=="daycare" || id==KingdomAdventure.Zone || id==TreasureHunt.Zone || id==DaycareVet.Zone || DaycarePlay.Area(id) || HomeRooms.Internal(id) || id==DinosaurRides.Area || ZooLayout.Area(id);
        public static bool Destination(string id)=>id=="home" || Area(id) && !HomeRooms.Internal(id) && !ZooCatalog.Trail(id) && id!=KingdomAdventure.Zone && id!=TreasureHunt.Zone && id!=DaycareVet.Zone && !DaycarePlay.Area(id);
        public static string Canonical(string id)=>id=="home"?"garden":id;
        public static string Place(SoloPlayer player)=>player.zone==KingdomAdventure.Zone || player.zone==TreasureHunt.Zone || player.zone==DaycareVet.Zone || DaycarePlay.Area(player.zone)?"daycare":ZooCatalog.Trail(player.zone)?ZooLayout.Entrance:HomeRooms.Internal(player.zone) || player.zone=="garden" && player.x<0?"home":player.zone;
        public static float MinX(string area,int schema=Schema)=>schema>=ScenerySchema && area=="garden"?(schema>=Discovery.Schema?Discovery.MinX:-4800):0;
        public static float MaxX(string area,int schema=Schema)=>HomeRooms.Internal(area) || area==ZooLayout.Entrance || area==KingdomAdventure.Zone || area==DaycareVet.Zone || DaycarePlay.Area(area)?2400:ZooCatalog.Trail(area) && schema>=ZooCatalog.Schema?9600:schema>=ScenerySchema?4800:GameWorld.Width;
        public static float ArrivalX(string destination)=>destination=="home"?-4380:destination==ZooLayout.Entrance?ZooLayout.EntranceArrivalX:420;
        public static bool Position(string area,int schema,float x,float y)=>Area(area) && (!DaycarePlay.Area(area) || schema>=DaycarePlay.Schema) && (area!=DaycareVet.Zone || schema>=DaycareVet.Schema) && (area!=TreasureHunt.Zone || schema>=TreasureHunt.Schema) && (area!=KingdomAdventure.Zone || schema>=KingdomAdventure.Schema) && (area!=DinosaurRides.Area || schema>=DinosaurRides.Schema) && (!ZooLayout.Area(area) || schema>=ZooLayout.Schema) && (!ZooCatalog.Trail(area) || area==ZooLayout.Savanna || schema>=ZooCatalog.Schema) && (!HomeRooms.Internal(area) || schema>=HomeRooms.Schema) &&
            (area!=BathroomLayout.Area || schema>=BathroomLayout.Schema) && (BedroomLayout.Index(area)<0 || schema>=BedroomLayout.Schema) && (SecretRooms.Index(area)<0 || schema>=SecretRooms.Schema) &&
            !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) &&
            x>=MinX(area,schema) && x<=MaxX(area,schema) && y>=0 && y<=GameWorld.Height;
    }
}
