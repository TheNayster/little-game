using System;

namespace LittleWeeps.Core
{
    // Home is an arrival shortcut into the same property as Garden. Keeping
    // its coordinates left of the original garden preserves every saved prop.
    public static class WorldLayout
    {
        // Content 36 admits the prepared Heeler roster's additional avatar IDs.
        // Existing save fields/schema stay intact; older authorities must not
        // admit clients that can select characters they cannot validate.
        // Content 39 admits the complete 37-character research roster.
        // Content 41 requires the replicated outfit fields and roar commands.
        // Content 42 adds authoritative shared pond fishing/feeding and the moved picnic cover.
        // Content 44 adds the shared kingdom story zone, commands and saved progress.
        public const int Schema=38, ScenerySchema=3, Content=45;
        public const float TileWidth=2400, SceneHeight=800;
        public static bool Area(string id)=>id=="garden" || id=="creek" || id=="park" || id=="beach" || id=="daycare" || id==KingdomAdventure.Zone || HomeRooms.Internal(id);
        public static bool Destination(string id)=>id=="home" || Area(id) && !HomeRooms.Internal(id) && id!=KingdomAdventure.Zone;
        public static string Canonical(string id)=>id=="home"?"garden":id;
        public static string Place(SoloPlayer player)=>player.zone==KingdomAdventure.Zone?"daycare":HomeRooms.Internal(player.zone) || player.zone=="garden" && player.x<0?"home":player.zone;
        public static float MinX(string area,int schema=Schema)=>schema>=ScenerySchema && area=="garden"?(schema>=Discovery.Schema?Discovery.MinX:-4800):0;
        public static float MaxX(string area,int schema=Schema)=>HomeRooms.Internal(area) || area==KingdomAdventure.Zone?2400:schema>=ScenerySchema?4800:SoloWorld.Width;
        public static float ArrivalX(string destination)=>destination=="home"?-4380:420;
        public static bool Position(string area,int schema,float x,float y)=>Area(area) && (area!=KingdomAdventure.Zone || schema>=KingdomAdventure.Schema) && (!HomeRooms.Internal(area) || schema>=HomeRooms.Schema) &&
            (BedroomLayout.Index(area)<0 || schema>=BedroomLayout.Schema) && (SecretRooms.Index(area)<0 || schema>=SecretRooms.Schema) &&
            !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) &&
            x>=MinX(area,schema) && x<=MaxX(area,schema) && y>=0 && y<=SoloWorld.Height;
    }
}
