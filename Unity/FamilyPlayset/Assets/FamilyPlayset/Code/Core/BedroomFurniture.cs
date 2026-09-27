using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Fixed, reversible arrangements keep the doorway and front walk corridor
    // clear. Furniture is saved per room; its sprite hierarchy is only a view.
    public static class BedroomFurniture
    {
        public const int Schema=8;
        public const string Bed="bedroom-bed";
        public static bool Personal(ToyKind kind)=>kind==ToyKind.Plush || kind==ToyKind.Block || kind==ToyKind.Book || RoomPlay.Tea(kind);
        public static string ToyId(int room,int slot)=>BedroomLayout.Id(room)+"-toy-"+slot;
        public static string Cushion(int index)=>"bedroom-cushion-"+index;
        public static int CushionIndex(string id)
        {for(var i=0;i<4;i++)if(id==Cushion(i))return i;return -1;}
        public static bool Seat(string id)=>id==Bed || CushionIndex(id)>=0 || SecretRooms.FortIndex(id)>=0;
        public static float BedX(int layout)=>layout==0?1050:1450;
        // Leave the far-right wall clear for the later magical secret entrance.
        public static float ShelfX(int layout)=>layout==0?2010:900;
        public static float ChestX(int layout)=>layout==0?1650:1990;
        public static float SeatX(string id,int layout)=>SecretRooms.FortIndex(id)>=0?BedX(layout)-240+SecretRooms.FortIndex(id)*160:id==Bed?BedX(layout):(layout==0?820:1150)+CushionIndex(id)*200;
        public static float SeatY(string id)=>id==Bed || SecretRooms.FortIndex(id)>=0?350:95;
        public static string Storage(string room,int slot)=>room+"/"+(slot<8?"chest-"+slot:"shelf-"+(slot-8));
        public static int Slot(string room,string id)
        {if(!SecretRooms.Furnished(room))return -1;for(var i=0;i<12;i++)if(id==Storage(room,i))return i;return -1;}
        public static float StorageX(int layout,int slot)=>slot<8?ChestX(layout)+(slot%4-1.5f)*70:ShelfX(layout)+((slot-8)%2==0?-110:110);
        public static float StorageY(int slot)=>slot<8?300+slot/4*75:360+(slot-8)/2*80;
        public static WalkPoint Floor(float x,float y)=>new WalkPoint(x,x>600?Math.Min(260,y):y);
        public static WalkInput Route(SoloPlayer p,WalkInput input,int schema)
        {
            if(schema<Schema || !SecretRooms.Furnished(p.zone) || input==null || input.mode!=WalkMode.Destination)return input;
            var copy=input.Copy();var target=Floor(copy.x,copy.y);copy.x=target.X;copy.y=target.Y;
            // Use the clear front corridor before entering/leaving the left
            // doorway lane; never walk diagonally through a bed or shelf.
            if((p.x>600 && copy.y>260) || (p.y>260 && copy.x>600))
            {copy.x=500;copy.y=220;}
            return copy;
        }
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithFurnishedRooms(SoloWorld world)
        {
            world=WithBedrooms(world);if(world.Schema>=BedroomFurniture.Schema)return world;
            var s=world.Snapshot();s.schema=BedroomFurniture.Schema;s.revision++;
            for(var i=0;i<4;i++){var r=s.bedrooms.Single(v=>v.id==BedroomLayout.Id(i));r.theme=i;r.roomRevision=1;}
            // A bounded versioned addition to the existing item registry keeps
            // one holder/identity and the established drag/network/save lane.
            // These starter toys do not stand in for the full dinosaur catalog.
            s.toys=s.toys.Concat(Enumerable.Range(0,4).SelectMany(i=>Enumerable.Range(0,4).Select(j=>new SoloToy{
                id=BedroomFurniture.ToyId(i,j),kind=j<2?ToyKind.Plush:ToyKind.Block,
                personalRoom=BedroomLayout.Id(i),zone=BedroomLayout.Id(i),container=BedroomFurniture.Storage(BedroomLayout.Id(i),8+j),
                x=BedroomFurniture.StorageX(0,8+j),y=BedroomFurniture.StorageY(8+j)}))).ToArray();
            Validate(s);return new SoloWorld(s);
        }
        private static void ValidateFurnishings(SoloSnapshot s)
        {
            if(s.schema<BedroomFurniture.Schema)
            {
                if(s.toys.Any(t=>BedroomFurniture.Personal(t.kind) || !string.IsNullOrEmpty(t.personalRoom)) ||
                    (s.bedrooms??Array.Empty<BedroomState>()).Any(r=>r.roomRevision!=0 || r.theme!=0 || r.layout!=0 || r.chestOpen || r.lampOn || r.decorateTogether || !string.IsNullOrEmpty(r.undoKind) || !string.IsNullOrEmpty(r.undoActor) || r.undoBefore!=0 || r.undoAfter!=0))
                    throw new InvalidOperationException("Furnishings require schema 8.");return;
            }
            foreach(var r in SecretRooms.Furnishings(s))
            {
                if(r.theme<0 || r.theme>3 || r.layout<0 || r.layout>1 || r.roomRevision<1 || r.roomRevision>=long.MaxValue ||
                    r.undoKind==null || r.undoActor==null || r.undoBefore<0 || r.undoBefore>3 || r.undoAfter<0 || r.undoAfter>3 ||
                    (r.undoKind==""?(r.undoActor!="" || r.undoBefore!=0 || r.undoAfter!=0):
                    (r.undoKind!="theme" && r.undoKind!="layout" && !(s.schema>=RoomPlay.Schema && RoomPlay.Decoration(r.undoKind))) || !s.players.Any(p=>p.id==r.undoActor) ||
                    RoomPlay.DecorValue(r,r.undoKind)!=r.undoAfter || r.undoKind=="layout" && (r.undoBefore>1 || r.undoAfter>1)))
                    throw new InvalidOperationException("Invalid furnishing state.");
            }
            for(var i=0;i<4;i++)for(var j=0;j<4;j++)
            {
                var t=s.toys.SingleOrDefault(v=>v.id==BedroomFurniture.ToyId(i,j));
                if(t==null || t.kind!=(j<2?ToyKind.Plush:ToyKind.Block) || t.personalRoom!=BedroomLayout.Id(i) || t.water!=0 || t.wet || t.resetPending)
                    throw new InvalidOperationException("Missing or altered personal toy.");
            }
            if(s.toys.Any(t=>t.kind!=ToyKind.Book && BedroomFurniture.Personal(t.kind)!=!string.IsNullOrEmpty(t.personalRoom)))throw new InvalidOperationException("Invalid personal item identity.");
        }
        private bool StorageOpen(SoloToy toy)
        {
            if(RoomPlay.Parent(toy)!="" || RoomPlay.Slot(toy.zone,toy.container)>=0 || string.IsNullOrEmpty(toy.container) || state.schema>=HomeBooks.FirstSchema && HomeBooks.Slot(toy.container)>=0)return true;
            if(HomeLayout.StorageSlot(toy.container)>=0)return state.home?.shedOpen==true;
            var room=SecretRooms.Furnishings(state).SingleOrDefault(r=>r.id==toy.zone);var slot=BedroomFurniture.Slot(toy.zone,toy.container);
            return state.schema>=BedroomFurniture.Schema && room!=null && slot>=0 && (slot>=8 || room.chestOpen);
        }
        private string StoreInBedroom(SoloCommand c,SoloPlayer player,SoloToy item,int slot)
        {
            var room=SecretRooms.Furnishings(state).Single(r=>r.id==player.zone);
            if(state.schema<BedroomFurniture.Schema || slot<8 && !room.chestOpen)return "storage-closed";
            if(state.toys.Any(t=>t.container==c.target))return "storage-full";
            if(Math.Abs(c.x-BedroomFurniture.StorageX(room.layout,slot))>80 || Math.Abs(c.y-BedroomFurniture.StorageY(slot))>80)return "target-too-far";
            item.container=c.target;item.holder="";item.x=BedroomFurniture.StorageX(room.layout,slot);item.y=BedroomFurniture.StorageY(slot);Touch(item);return null;
        }
        private string ApplyBedroomFixture(SoloCommand c,SoloPlayer p)
        {
            if(state.schema<BedroomFurniture.Schema)return "wrong-area";
            var room=SecretRooms.Furnishings(state).Single(r=>r.id==p.zone);
            if(c.action==SoloAction.LeaveFixture){ClearFixture(p);return null;}
            if(c.action==SoloAction.UseFixture)
            {
                if(!BedroomFurniture.Seat(c.target) || (SecretRooms.Index(p.zone)>=0?c.target==BedroomFurniture.Bed:SecretRooms.FortIndex(c.target)>=0))return "invalid-fixture";
                if(state.players.Any(v=>v.id!=p.id && v.zone==p.zone && v.fixture==c.target))return "fixture-busy";
                var cuddle=state.schema>=RoomPlay.Schema?state.toys.FirstOrDefault(t=>t.holder==p.id && t.kind==ToyKind.Plush):null;
                foreach(var t in state.toys.Where(v=>v.holder==p.id && v!=cuddle)){t.holder="";t.x=p.x;t.y=Math.Max(35,Math.Min(250,p.y-65));Touch(t);}
                ClearFixture(p);p.fixture=c.target;p.activity="";p.x=BedroomFurniture.SeatX(c.target,room.layout);p.y=BedroomFurniture.SeatY(c.target);if(cuddle!=null){cuddle.holder=p.id;cuddle.x=p.x;cuddle.y=Math.Max(35,Math.Min(250,p.y-65));}return null;
            }
            if(c.action==SoloAction.SetFixture)
            {
                if(c.value!="on" && c.value!="off")return "invalid-fixture-state";
                if(c.target=="bedroom-chest")room.chestOpen=c.value=="on";
                else if(c.target=="bedroom-lamp")room.lampOn=c.value=="on";
                else return "invalid-fixture";
                return null;
            }
            return "unknown-action";
        }
        private string DecorateBedroom(SoloCommand c,SoloPlayer p)
        {
            if(state.schema<BedroomFurniture.Schema || !SecretRooms.Furnished(p.zone))return "wrong-area";
            var room=SecretRooms.Furnishings(state).Single(r=>r.id==p.zone);var owner=room.owner==p.id;
            if(!owner && (!room.decorateTogether || c.target=="together" || c.target=="tidy"))return "owner-only";
            var parts=c.value.Split(':');
            if(parts.Length!=2 || !int.TryParse(parts[0],out var value) || !long.TryParse(parts[1],out var expected) || expected!=room.roomRevision || room.roomRevision>=long.MaxValue-1)return "room-changed";
            if(c.target=="tidy")
            {
                var loose=state.toys.Where(t=>t.zone==room.id && t.personalRoom==(SecretRooms.Index(room.id)>=0?SecretRooms.Parent(room.id):room.id) && t.holder=="" && t.container=="" && !state.toys.Any(child=>child.container==RoomPlay.Stack(t.id))).ToArray();
                var free=Enumerable.Range(0,8).Where(i=>!state.toys.Any(t=>t.container==BedroomFurniture.Storage(room.id,i))).ToArray();
                if(loose.Length>free.Length)return "storage-full";
                for(var i=0;i<loose.Length;i++){var t=loose[i];t.container=BedroomFurniture.Storage(room.id,free[i]);t.x=BedroomFurniture.StorageX(room.layout,free[i]);t.y=BedroomFurniture.StorageY(free[i]);Touch(t);}
                room.chestOpen=true;return null;
            }
            if(c.target=="together")
            {if(value<0 || value>1)return "invalid-decoration";room.decorateTogether=value==1;ClearRoomUndo(room);room.roomRevision++;return null;}
            if(c.target=="undo")
            {
                if(room.undoKind=="" || room.undoActor!=p.id)return "nothing-to-undo";
                if(room.undoKind=="layout")MoveRoomLayout(room,room.undoBefore);else RoomPlay.SetDecor(room,room.undoKind,room.undoBefore);
                ClearRoomUndo(room);room.roomRevision++;return null;
            }
            if(c.target!="theme" && c.target!="layout" && !(state.schema>=RoomPlay.Schema && RoomPlay.Decoration(c.target)) || value<0 || value>(c.target=="layout"?1:3))return "invalid-decoration";
            var before=RoomPlay.DecorValue(room,c.target);
            if(before==value)return "already-there";
            if(c.target=="layout")MoveRoomLayout(room,value);else RoomPlay.SetDecor(room,c.target,value);
            room.undoKind=c.target;room.undoActor=p.id;room.undoBefore=before;room.undoAfter=value;room.roomRevision++;return null;
        }
        private static void ClearRoomUndo(BedroomState room)
        {room.undoKind=room.undoActor="";room.undoBefore=room.undoAfter=0;}
        private void MoveRoomLayout(BedroomState room,int layout)
        {
            room.layout=layout;
            foreach(var p in state.players.Where(p=>p.zone==room.id && BedroomFurniture.Seat(p.fixture)))
            {p.x=BedroomFurniture.SeatX(p.fixture,layout);p.y=BedroomFurniture.SeatY(p.fixture);foreach(var held in state.toys.Where(t=>t.holder==p.id)){held.x=p.x;held.y=Math.Max(35,Math.Min(250,p.y-65));}}
            foreach(var t in state.toys.Where(t=>t.zone==room.id))
            {var slot=BedroomFurniture.Slot(room.id,t.container);if(slot>=0){t.x=BedroomFurniture.StorageX(layout,slot);t.y=BedroomFurniture.StorageY(slot);}}
        }
    }
}
