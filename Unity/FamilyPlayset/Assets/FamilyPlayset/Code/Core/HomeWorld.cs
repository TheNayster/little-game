using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class HomeState
    {
        public bool livingRadio, gardenRadio, shedOpen;
        public HomeState Copy() => (HomeState)MemberwiseClone();
    }
    // Authored anchors are shared by authority and presentation. Home and garden
    // are one property; these are slots, never independently spawned objects.
    public static class HomeLayout
    {
        public const float SofaX=-3960, SofaY=200, TrampolineX=1650, TrampolineY=220;
        public const float ShedX=3550, ShedY=300;
        // Preserve the original outer anchors and fit two closer places between
        // them. Furniture art and its front/rear depth masks stay the same size.
        public static readonly string[] SofaSlots={"sofa-left","sofa-middle-left","sofa-middle-right","sofa-right"};
        public static readonly string[] TrampolineSlots={"trampoline-left","trampoline-middle-left","trampoline-middle-right","trampoline-right"};
        public static bool Seat(string id)=>Array.IndexOf(SofaSlots,id)>=0 || Kitchen.Seat(id);
        public static bool Bounce(string id)=>Array.IndexOf(TrampolineSlots,id)>=0;
        public static bool Usable(string id)=>Seat(id) || Bounce(id);
        public static float X(string id,int schema=WorldLayout.Schema)
        {
            if(Kitchen.Seat(id))return Kitchen.SeatX(id,schema);
            var seated=Seat(id);var index=Array.IndexOf(seated?SofaSlots:TrampolineSlots,id);
            return (seated?SofaX:TrampolineX)+(index*2/3f-1)*(seated?100:110);
        }
        public static float Y(string id)=>Kitchen.Seat(id)?220:Seat(id)?SofaY:TrampolineY;
        public static int StorageSlot(string id)=>id=="shed-0"?0:id=="shed-1"?1:id=="shed-2"?2:id=="shed-3"?3:-1;
        public static float StorageX(int slot)=>ShedX+(slot%2==0?-85:85);
        public static float StorageY(int slot)=>slot<2?430:160;
        public static bool RadioNear(HomeState home,SoloPlayer p)=>home!=null && p.zone=="garden" &&
            (home.livingRadio && Math.Abs(p.x-(-3480))<640 || home.gardenRadio && Math.Abs(p.x-1230)<640);
    }
    public sealed partial class SoloWorld
    {
        public static bool Carryable(ToyKind kind)=>kind==ToyKind.Bucket || kind==ToyKind.Sponge || kind==ToyKind.Ball || BedroomFurniture.Personal(kind) || Kitchen.Kind(kind);
        public HomeState ReadHome()=>state.home?.Copy();
        public static SoloWorld WithHome(SoloWorld world)
        {
            world=WithScenery(world);
            if(world.Schema>=4)return world;
            var copy=world.Snapshot();copy.schema=4;copy.home=new HomeState();
            copy.toys=copy.toys.Concat(new[]{new SoloToy{id="ball-1",kind=ToyKind.Ball,x=3350,y=130}}).ToArray();
            copy.revision++;Validate(copy);return new SoloWorld(copy);
        }
        private void ClearFixture(SoloPlayer p)
        {SettleCuddle(p);p.fixture="";p.useSeconds=0;p.rideStarted=0;}
        // A lease is temporary, including across app suspension, travel and
        // recovery. The furniture and stored item state are durable.
        public bool ReleaseFixture(string actor)
        {
            var p=state.players.FirstOrDefault(v=>v.id==actor);
            if(p==null || string.IsNullOrEmpty(p.fixture))return false;
            ClearFixture(p);state.revision++;return true;
        }
        private bool AdvanceHome(double seconds)
        {
            var changed=false;
            foreach(var p in state.players.Where(p=>!string.IsNullOrEmpty(p.fixture)))
            {p.useSeconds=Math.Min(86400,p.useSeconds+seconds);changed=true;}
            return changed;
        }
        private static void ValidateHome(SoloSnapshot s)
        {
            if(s.schema<4)
            {
                if(s.home!=null && (s.home.livingRadio || s.home.gardenRadio || s.home.shedOpen) || s.toys.Any(t=>t.kind==ToyKind.Ball || !string.IsNullOrEmpty(t.container)) ||
                    s.players.Any(p=>!string.IsNullOrEmpty(p.fixture) || p.useSeconds!=0))throw new InvalidOperationException("Home state requires schema 4.");
                return;
            }
            if(s.home==null || s.toys.Count(t=>t.kind==ToyKind.Ball)!=1 || !s.toys.Any(t=>t.id=="ball-1" && t.kind==ToyKind.Ball && (t.zone=="garden" || s.schema>=HomeRooms.Schema && HomeRooms.Internal(t.zone))))throw new InvalidOperationException("Missing home state or fixed home toy.");
            foreach(var p in s.players)
                if(double.IsNaN(p.useSeconds) || double.IsInfinity(p.useSeconds) || p.useSeconds<0 || p.useSeconds>86400 ||
                    (string.IsNullOrEmpty(p.fixture)?p.useSeconds!=0:
                     (BedroomFurniture.Seat(p.fixture)?s.schema<BedroomFurniture.Schema || !SecretRooms.Furnished(p.zone) || (SecretRooms.Index(p.zone)>=0?p.fixture==BedroomFurniture.Bed:SecretRooms.FortIndex(p.fixture)>=0) ||
                      p.x!=BedroomFurniture.SeatX(p.fixture,SecretRooms.Furnishings(s).Single(r=>r.id==p.zone).layout) || p.y!=BedroomFurniture.SeatY(p.fixture):
                      ParkWheels.Usable(p.fixture)?s.schema<ParkPlay.Schema || !ParkWheels.Valid(p):
                      ParkPlay.Usable(p.fixture)?s.schema<ParkPlay.Schema || p.zone!="park" || p.x!=ParkPlay.X(p.fixture) || p.y!=ParkPlay.Y(p.fixture):
                      !HomeLayout.Usable(p.fixture) || Kitchen.Seat(p.fixture) && s.schema<Kitchen.Schema || p.zone!="garden" || p.x!=HomeLayout.X(p.fixture,s.schema) || p.y!=HomeLayout.Y(p.fixture)) || s.toys.Any(t=>t.holder==p.id && !(s.schema>=RoomPlay.Schema && BedroomFurniture.Seat(p.fixture) && t.kind==ToyKind.Plush))))
                    throw new InvalidOperationException("Invalid home occupancy.");
            if(s.players.Where(p=>!string.IsNullOrEmpty(p.fixture)).GroupBy(p=>p.zone+"/"+p.fixture).Any(g=>g.Count()>1))
                throw new InvalidOperationException("Home slot has two occupants.");
            foreach(var t in s.toys.Where(t=>!string.IsNullOrEmpty(t.container)))
            {
                if(Kitchen.Slot(t.container,out var kitchenGroup,out var kitchenSlot)){if(s.schema<Kitchen.Schema || t.zone!="garden" || t.holder!="" || !Carryable(t.kind) || t.x!=Kitchen.X(kitchenGroup,kitchenSlot,s.schema) || t.y!=Kitchen.Y(kitchenGroup,kitchenSlot))throw new InvalidOperationException("Invalid kitchen support.");continue;}
                if(RoomPlay.Parent(t)!="")continue;
                var playSlot=RoomPlay.Slot(t.zone,t.container);
                if(playSlot>=0){if(s.schema<RoomPlay.Schema || t.holder!="" || t.x!=RoomPlay.X(playSlot) || t.y!=RoomPlay.Y(playSlot) || t.kind!=(playSlot<4?ToyKind.Plush:playSlot==8?ToyKind.TeaPot:ToyKind.TeaCup))throw new InvalidOperationException("Invalid room play support.");continue;}
                var bookSlot=HomeBooks.Slot(t.container);
                if(bookSlot>=0){if(s.schema<HomeBooks.FirstSchema || t.kind!=ToyKind.Book || t.zone!="garden" || t.holder!="" || t.x!=HomeBooks.X(bookSlot) || t.y!=HomeBooks.Y(bookSlot))throw new InvalidOperationException("Invalid book support.");continue;}
                var bedroomSlot=BedroomFurniture.Slot(t.zone,t.container);
                if(bedroomSlot>=0)
                {
                    var room=SecretRooms.Furnishings(s).Single(r=>r.id==t.zone);
                    if(s.schema<BedroomFurniture.Schema || !Carryable(t.kind) || t.holder!="" || t.x!=BedroomFurniture.StorageX(room.layout,bedroomSlot) || t.y!=BedroomFurniture.StorageY(bedroomSlot))throw new InvalidOperationException("Invalid bedroom support.");
                    continue;
                }
                var slot=HomeLayout.StorageSlot(t.container);
                if(slot<0 || t.zone!="garden" || !Carryable(t.kind) || !string.IsNullOrEmpty(t.holder) ||
                    t.x!=HomeLayout.StorageX(slot) || t.y!=HomeLayout.StorageY(slot))throw new InvalidOperationException("Invalid storage parent.");
            }
            if(s.toys.Where(t=>!string.IsNullOrEmpty(t.container)).GroupBy(t=>t.container).Any(g=>g.Count()>1))
                throw new InvalidOperationException("Storage slot has two items.");
        }
        // Returns a rejection without mutation, or commits a validated action.
        private string ApplyHome(SoloCommand c,SoloPlayer player)
        {
            if(player.zone=="park")return ApplyParkFixture(c,player);
            if(SecretRooms.Furnished(player.zone))return ApplyBedroomFixture(c,player);
            if(state.home==null || player.zone!="garden")return "wrong-area";
            if(c.action==SoloAction.LeaveFixture){ClearFixture(player);return null;}
            if(c.action==SoloAction.UseFixture)
            {
                if(!HomeLayout.Usable(c.target) || Kitchen.Seat(c.target) && state.schema<Kitchen.Schema)return "invalid-fixture";
                if(state.players.Any(p=>p.id!=player.id && p.fixture==c.target))return "fixture-busy";
                // Simple Play uses direct, safe placement. Held props settle on
                // the floor before entry in the SAME transaction, contents intact.
                foreach(var t in state.toys.Where(t=>t.holder==player.id))
                {t.holder="";t.container="";t.x=player.x;t.y=Math.Max(35,player.y-65);Touch(t);}
                player.fixture=c.target;player.useSeconds=0;player.activity="";
                player.x=HomeLayout.X(c.target,state.schema);player.y=HomeLayout.Y(c.target);return null;
            }
            if(c.action==SoloAction.SetFixture)
            {
                if(c.value!="on" && c.value!="off")return "invalid-fixture-state";
                var on=c.value=="on";
                if(c.target=="radio-living")state.home.livingRadio=on;
                else if(c.target=="radio-garden")state.home.gardenRadio=on;
                else if(c.target=="shed")state.home.shedOpen=on;
                else return "invalid-fixture";
                return null;
            }
            return "unknown-action";
        }
    }
}
