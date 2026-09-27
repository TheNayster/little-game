using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Internal Home places preserve the existing garden identity and coordinates.
    // They are never top-level travel destinations or allocated by a visit.
    public static class HomeRooms
    {
        public const int Schema=6;
        public const string Landing="home-upstairs";
        public const double StairDuration=2.4;
        public const float LowerX=-2980, LowerY=420, UpperX=540, UpperY=400;
        public static bool Internal(string zone)=>zone==Landing || BedroomLayout.Index(zone)>=0 || SecretRooms.Index(zone)>=0;
        public static bool Property(string zone)=>zone=="garden" || Internal(zone);
        public static bool StairArea(string zone)=>zone=="garden" || zone==Landing;
        public static float EntryX(string zone)=>zone=="garden"?LowerX:UpperX;
        public static float EntryY(string zone)=>zone=="garden"?LowerY:UpperY;
        public static bool NearEntry(SoloPlayer p)=>StairArea(p.zone) &&
            Math.Abs(p.x-EntryX(p.zone))<=65 && Math.Abs(p.y-EntryY(p.zone))<=65;
        public static bool ValidTransit(SoloPlayer p)=>KeepyRules.Finite(p.stairs) && p.stairs>=0 && p.stairs<StairDuration &&
            (p.stairs==0 || StairArea(p.zone) && p.x==EntryX(p.zone) && p.y==EntryY(p.zone) &&
             string.IsNullOrEmpty(p.fixture) && p.activity=="");
    }
    [Serializable] public sealed class BedroomState
    {
        public string id, owner;
        public int theme,layout,undoBefore,undoAfter;
        public long roomRevision;
        public bool chestOpen,lampOn,decorateTogether;
        public string undoKind="",undoActor="";
        public BedroomState Copy(){var copy=(BedroomState)MemberwiseClone();copy.undoKind=copy.undoKind??"";copy.undoActor=copy.undoActor??"";return copy;}
    }
    public static class BedroomLayout
    {
        public const int Schema=7;
        public const float ExitX=325, DoorY=420;
        public static string Id(int index)=>index>=0 && index<4?"home-bedroom-"+(index+1):"";
        public static int Index(string area)
        {for(var i=0;i<4;i++)if(area==Id(i))return i;return -1;}
        public static float HallX(int index)=>index==0?710:index==1?1132:index==2?1601:2066;
        public static bool Route(string source,string target)=>source==HomeRooms.Landing && Index(target)>=0 || Index(source)>=0 && target==HomeRooms.Landing;
        public static float DoorX(string source,string target)=>source==HomeRooms.Landing?HallX(Index(target)):ExitX;
        public static bool NearDoor(SoloPlayer p,string target)=>Route(p.zone,target) &&
            Math.Abs(p.x-DoorX(p.zone,target))<=65 && Math.Abs(p.y-DoorY)<=65;
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithBedrooms(SoloWorld world)
        {
            world=WithUpstairs(world);
            if(world.Schema>=BedroomLayout.Schema)return world;
            var copy=world.Snapshot();copy.schema=BedroomLayout.Schema;copy.revision++;
            // The saved roster is established by enrollment, never connection or
            // avatar order. Solo knows only its own profile; spare rooms stay
            // unassigned guest rooms rather than inventing family identities.
            copy.bedrooms=Enumerable.Range(0,4).Select(i=>new BedroomState{
                id=BedroomLayout.Id(i),owner=i<copy.players.Length?copy.players[i].id:""}).ToArray();
            Validate(copy);return new SoloWorld(copy);
        }
        public BedroomState[] ReadBedrooms()=>state.bedrooms.Select(r=>r.Copy()).ToArray();
        private static void ValidateBedrooms(SoloSnapshot s)
        {
            if(s.schema<BedroomLayout.Schema)
            {if(s.bedrooms!=null && s.bedrooms.Length!=0)throw new InvalidOperationException("Bedrooms require schema 7.");return;}
            if(s.bedrooms==null || s.bedrooms.Length!=4 || s.bedrooms.Any(r=>r==null || r.owner==null || BedroomLayout.Index(r.id)<0) ||
                s.bedrooms.Select(r=>r.id).Distinct().Count()!=4 ||
                !s.bedrooms.Where(r=>r.owner!="").Select(r=>r.owner).OrderBy(id=>id,StringComparer.Ordinal)
                    .SequenceEqual(s.players.Select(p=>p.id).OrderBy(id=>id,StringComparer.Ordinal)))
                throw new InvalidOperationException("Invalid bedroom ownership.");
        }
        private string EnterDoor(SoloPlayer p,string target,string revision)
        {
            if(SecretRooms.Route(p.zone,target))return SecretTravel(p,target,revision);
            if(state.schema<BedroomLayout.Schema || !BedroomLayout.NearDoor(p,target))return "door-too-far";
            if(p.visit>=long.MaxValue-1)return "visit-limit";
            var source=p.zone;var slot=Array.FindIndex(state.players,v=>v.id==p.id);
            ClearFixture(p);p.activity="";p.zone=target;p.visit++;p.y=250;
            p.x=target==HomeRooms.Landing?BedroomLayout.HallX(BedroomLayout.Index(source))-165+slot*110:480+slot*130;
            foreach(var toy in state.toys.Where(t=>t.holder==p.id))
            {toy.zone=target;toy.x=p.x;toy.y=p.y;Touch(toy);}
            return null;
        }
        public static SoloWorld WithUpstairs(SoloWorld world)
        {
            world=WithKeepyUppy(world);
            if(world.Schema>=HomeRooms.Schema)return world;
            var copy=world.Snapshot();copy.schema=HomeRooms.Schema;copy.revision++;
            Validate(copy);return new SoloWorld(copy);
        }
        private string BeginStairs(SoloPlayer p)
        {
            if(state.schema<HomeRooms.Schema || !HomeRooms.NearEntry(p))return "stairs-too-far";
            if(p.stairs>0)return "already-on-stairs";
            // Reserve both the departure generation and the eventual arrival.
            if(p.visit>=long.MaxValue-2)return "visit-limit";
            ClearFixture(p);p.activity="";p.x=HomeRooms.EntryX(p.zone);p.y=HomeRooms.EntryY(p.zone);
            p.stairs=.000001;p.visit++;
            foreach(var t in state.toys.Where(t=>t.holder==p.id))
            {t.x=p.x;t.y=p.y;Touch(t);}
            return null;
        }
        public bool CancelStairs(string actor)
        {
            var p=state.players.FirstOrDefault(v=>v.id==actor);
            if(p==null || p.stairs==0)return false;
            // Source coordinates remain a valid endpoint throughout traversal.
            p.stairs=0;state.revision++;return true;
        }
        private bool AdvanceStairs(double seconds,string[] activePlayers,out bool committed)
        {
            committed=false;var changed=false;
            foreach(var p in state.players.Where(p=>p.stairs>0))
            {
                if(activePlayers!=null && !activePlayers.Contains(p.id))continue;
                p.stairs+=seconds;changed=true;
                if(p.stairs<HomeRooms.StairDuration)continue;
                var goingUp=p.zone=="garden";
                p.zone=goingUp?HomeRooms.Landing:"garden";p.visit++;p.stairs=0;
                // Stagger four arrivals along the clear floor, outside the trigger.
                var slot=Array.FindIndex(state.players,v=>v.id==p.id);
                p.x=(goingUp?HomeRooms.UpperX:HomeRooms.LowerX)+85+slot*110;
                p.y=250;
                foreach(var t in state.toys.Where(t=>t.holder==p.id))
                {t.zone=p.zone;t.x=p.x;t.y=p.y;Touch(t);}
                committed=true;
            }
            return changed;
        }
        private static bool ValidToyLocation(SoloToy t,int schema)
        {
            if(t.kind==ToyKind.Book)return schema>=HomeBooks.FirstSchema && HomeBooks.Index(t.id)>=0 && HomeRooms.Property(t.zone);
            if(BedroomFurniture.Personal(t.kind))return schema>=BedroomFurniture.Schema && BedroomLayout.Index(t.personalRoom)>=0 && HomeRooms.Property(t.zone);
            var origin=t.id==t.kind.ToString().ToLowerInvariant()+"-1"?"garden":
                t.id==t.kind.ToString().ToLowerInvariant()+"-creek"?"creek":"";
            return origin!="" && (AreaOf(t.zone)==origin || schema>=HomeRooms.Schema && origin=="garden" &&
                Carryable(t.kind) && HomeRooms.Internal(t.zone));
        }
    }
}
