using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class BedroomState
    {
        public int version = 1;
        public string id, owner;
        public int wall, bedding;
        public bool decorateTogether;
        public BedroomState Copy() => (BedroomState)MemberwiseClone();
    }

    public sealed partial class SoloWorld
    {
        public static readonly string[] BedroomIds = { "bedroom-a", "bedroom-b" };
        private static readonly ToyKind[] BedroomKinds = { ToyKind.Bed, ToyKind.Rug, ToyKind.Shelf, ToyKind.Basket,
            ToyKind.Cushion, ToyKind.Picture, ToyKind.Lamp, ToyKind.Plush };
        public static bool IsBedroom(string zone) => zone == "bedroom-a" || zone == "bedroom-b";
        public static bool Personal(ToyKind kind) => kind >= ToyKind.Bed && kind <= ToyKind.Plush;
        public static bool Movable(ToyKind kind) => kind == ToyKind.Bucket || kind == ToyKind.Sponge || Personal(kind);
        public static string AreaName(string zone) => zone == "bedroom-a" ? "Bedroom 1" : zone == "bedroom-b" ? "Bedroom 2" : zone == "creek" ? "Creek" : "Garden";

        // Schema 3 appends two rooms and stable personal item instances. The
        // enrolled roster order assigns rooms once; avatar selection never does.
        public static SoloWorld WithBedrooms(SoloWorld world)
        {
            if (world.state.schema == 3) return world;
            var copy = WithAreas(world).Snapshot();
            copy.schema = 3;
            copy.bedrooms = BedroomIds.Select((id, i) => new BedroomState { id = id,
                owner = i < copy.players.Length ? copy.players[i].id : "", wall = i, bedding = i }).ToArray();
            copy.toys = copy.toys.Concat(copy.bedrooms.SelectMany(room => BedroomKinds.Select(kind =>
            {
                var p = BedroomHome(kind);
                return new SoloToy { id = kind.ToString().ToLowerInvariant() + "-" + room.id,
                    kind = kind, zone = room.id, home = room.id, x = p.x, y = p.y };
            }))).ToArray();
            copy.revision++;
            Validate(copy); return new SoloWorld(copy);
        }
        public BedroomState ReadBedroom(string id) => state.bedrooms?.FirstOrDefault(r => r.id == id)?.Copy();
        private bool CanDecorate(string actor, string room) => state.bedrooms != null &&
            state.bedrooms.Any(r => r.id == room && (r.owner == actor || r.decorateTogether));
        private static (float x, float y) BedroomHome(ToyKind kind)
        {
            switch (kind)
            {
                case ToyKind.Bed: return (165, 335);
                case ToyKind.Rug: return (490, 210);
                case ToyKind.Shelf: return (780, 365);
                case ToyKind.Basket: return (835, 195);
                case ToyKind.Cushion: return (660, 200);
                case ToyKind.Picture: return (470, 425);
                case ToyKind.Lamp: return (315, 365);
                default: return (470, 305);
            }
        }
        // Keep the doorway and low walking strip free. Large furniture has
        // fitting slots; rugs/plush may overlap furniture intentionally.
        private static bool BedroomPlacement(ToyKind kind, float x, float y)
        {
            if (!Position(x,y) || x < 100 || x > 900 || y < 170 || y > 440) return false;
            if (kind == ToyKind.Bed) return (x <= 220 || x >= 780) && y >= 300 && y <= 370;
            if (kind == ToyKind.Picture) return y >= 400;
            return true;
        }
        private static void ValidateBedrooms(SoloSnapshot s)
        {
            if (s.schema < 3)
            {
                if (s.bedrooms != null && s.bedrooms.Length != 0) throw new InvalidOperationException("Rooms require schema 3.");
                return;
            }
            if (s.bedrooms == null || s.bedrooms.Length != 2 ||
                !s.bedrooms.Select(r => r?.id).SequenceEqual(BedroomIds)) throw new InvalidOperationException("Invalid bedroom catalog.");
            for (var i=0;i<2;i++)
            {
                var room=s.bedrooms[i];
                if (room.version != 1 || room.owner != (i < s.players.Length ? s.players[i].id : "") ||
                    room.wall < 0 || room.wall > 3 || room.bedding < 0 || room.bedding > 3 ||
                    (room.owner == "" && room.decorateTogether)) throw new InvalidOperationException("Invalid bedroom owner or style.");
                foreach (var kind in BedroomKinds)
                {
                    var item=s.toys.SingleOrDefault(t => t != null && t.kind == kind && t.home == room.id);
                    if (item == null || item.id != kind.ToString().ToLowerInvariant()+"-"+room.id ||
                        item.zone != room.id || !BedroomPlacement(kind,item.x,item.y) || item.water != 0 || item.wet || item.resetPending)
                        throw new InvalidOperationException("Invalid personal room item.");
                }
            }
        }
    }
}
