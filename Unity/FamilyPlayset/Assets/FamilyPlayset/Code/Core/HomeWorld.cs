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
        public static bool Seat(string id)=>id=="sofa-left" || id=="sofa-right";
        public static bool Bounce(string id)=>id=="trampoline-left" || id=="trampoline-right";
        public static bool Usable(string id)=>Seat(id) || Bounce(id);
        public static float X(string id)=>Seat(id)?SofaX+(id=="sofa-left"?-100:100):TrampolineX+(id=="trampoline-left"?-110:110);
        public static float Y(string id)=>Seat(id)?SofaY:TrampolineY;
        public static int StorageSlot(string id)=>id=="shed-0"?0:id=="shed-1"?1:id=="shed-2"?2:id=="shed-3"?3:-1;
        public static float StorageX(int slot)=>ShedX+(slot%2==0?-85:85);
        public static float StorageY(int slot)=>slot<2?430:160;
        public static bool RadioNear(HomeState home,SoloPlayer p)=>home!=null && p.zone=="garden" &&
            (home.livingRadio && Math.Abs(p.x-(-3480))<640 || home.gardenRadio && Math.Abs(p.x-1230)<640);
    }
    public sealed partial class SoloWorld
    {
        public static bool Carryable(ToyKind kind)=>kind==ToyKind.Bucket || kind==ToyKind.Sponge || kind==ToyKind.Ball;
        public HomeState ReadHome()=>state.home?.Copy();
        public static SoloWorld WithHome(SoloWorld world)
        {
            world=WithScenery(world);
            if(world.Schema>=4)return world;
            var copy=world.Snapshot();copy.schema=4;copy.home=new HomeState();
            copy.toys=copy.toys.Concat(new[]{new SoloToy{id="ball-1",kind=ToyKind.Ball,x=3350,y=130}}).ToArray();
            copy.revision++;Validate(copy);return new SoloWorld(copy);
        }
        private static void ClearFixture(SoloPlayer p)
        {p.fixture="";p.useSeconds=0;}
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
            if(s.home==null || s.toys.Count(t=>t.kind==ToyKind.Ball)!=1 || !s.toys.Any(t=>t.id=="ball-1" && t.kind==ToyKind.Ball && t.zone=="garden"))throw new InvalidOperationException("Missing home state or fixed home toy.");
            foreach(var p in s.players)
                if(double.IsNaN(p.useSeconds) || double.IsInfinity(p.useSeconds) || p.useSeconds<0 || p.useSeconds>86400 ||
                    (string.IsNullOrEmpty(p.fixture)?p.useSeconds!=0:!HomeLayout.Usable(p.fixture) || p.zone!="garden" ||
                     p.x!=HomeLayout.X(p.fixture) || p.y!=HomeLayout.Y(p.fixture) || s.toys.Any(t=>t.holder==p.id)))
                    throw new InvalidOperationException("Invalid home occupancy.");
            if(s.players.Where(p=>!string.IsNullOrEmpty(p.fixture)).GroupBy(p=>p.fixture).Any(g=>g.Count()>1))
                throw new InvalidOperationException("Home slot has two occupants.");
            foreach(var t in s.toys.Where(t=>!string.IsNullOrEmpty(t.container)))
            {
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
            if(state.home==null || player.zone!="garden")return "wrong-area";
            if(c.action==SoloAction.LeaveFixture){ClearFixture(player);return null;}
            if(c.action==SoloAction.UseFixture)
            {
                if(!HomeLayout.Usable(c.target))return "invalid-fixture";
                if(state.players.Any(p=>p.id!=player.id && p.fixture==c.target))return "fixture-busy";
                // Simple Play uses direct, safe placement. Held props settle on
                // the floor before entry in the SAME transaction, contents intact.
                foreach(var t in state.toys.Where(t=>t.holder==player.id))
                {t.holder="";t.container="";t.x=player.x;t.y=Math.Max(35,player.y-65);Touch(t);}
                player.fixture=c.target;player.useSeconds=0;player.activity="";
                player.x=HomeLayout.X(c.target);player.y=HomeLayout.Y(c.target);return null;
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
