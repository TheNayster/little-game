using System;

namespace LittleWeeps.Core
{
    // Home is an arrival shortcut into the same property as Garden. Keeping
    // its coordinates left of the original garden preserves every saved prop.
    public static class WorldLayout
    {
        // Content 36 includes both the Zoo milestone and the prepared roster.
        public const int Schema=34, ScenerySchema=3, Content=36;
        public const float TileWidth=2400, SceneHeight=800;
        public static bool Area(string id)=>id=="garden" || id=="creek" || id=="park" || id=="beach" || id=="daycare" || HomeRooms.Internal(id) || ZooLayout.Area(id);
        public static bool Destination(string id)=>id=="home" || Area(id) && !HomeRooms.Internal(id) && id!=ZooLayout.Savanna;
        public static string Canonical(string id)=>id=="home"?"garden":id;
        public static string Place(SoloPlayer player)=>player.zone==ZooLayout.Savanna?ZooLayout.Entrance:HomeRooms.Internal(player.zone) || player.zone=="garden" && player.x<0?"home":player.zone;
        public static float MinX(string area,int schema=Schema)=>schema>=ScenerySchema && area=="garden"?(schema>=Discovery.Schema?Discovery.MinX:-4800):0;
        public static float MaxX(string area,int schema=Schema)=>HomeRooms.Internal(area) || area==ZooLayout.Entrance?2400:schema>=ScenerySchema?4800:SoloWorld.Width;
        public static float ArrivalX(string destination)=>destination=="home"?-4380:420;
        public static bool Position(string area,int schema,float x,float y)=>Area(area) && (!ZooLayout.Area(area) || schema>=ZooLayout.Schema) && (!HomeRooms.Internal(area) || schema>=HomeRooms.Schema) &&
            (BedroomLayout.Index(area)<0 || schema>=BedroomLayout.Schema) && (SecretRooms.Index(area)<0 || schema>=SecretRooms.Schema) &&
            !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) &&
            x>=MinX(area,schema) && x<=MaxX(area,schema) && y>=0 && y<=SoloWorld.Height;
    }
}
