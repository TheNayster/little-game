using System;
using System.Linq;
namespace LittleWeeps.Core
{
    public static class BathroomLayout
    {
        public const int Schema=39;
        public const string Area="home-bathroom";
        public const float HallX=2290,ExitX=100,Y=160;
        public static readonly string[] BathSlots={"bath-0","bath-1","bath-2","bath-3"};
        public static readonly string[] SinkSlots={"sink-0","sink-1","sink-2","sink-3"};
        public static bool Bath(string id)=>Array.IndexOf(BathSlots,id)>=0;
        public static bool Usable(string id)=>Bath(id) || Array.IndexOf(SinkSlots,id)>=0;
        public static float X(string id)=>Bath(id)?950+Array.IndexOf(BathSlots,id)*155:445+Array.IndexOf(SinkSlots,id)*65;
        public static bool Route(string source,string target)=>source==HomeRooms.Landing && target==Area || source==Area && target==HomeRooms.Landing;
    }
    public sealed partial class GameWorld
    {
        // Callers finish the active feature migration chain first. This room
        // adds a valid area/fixture contract, reusing existing player leases.
        public static GameWorld WithBathroom(GameWorld world)
        {
            if(world.Schema>=BathroomLayout.Schema)return world;
            var s=world.Snapshot();s.schema=BathroomLayout.Schema;s.revision++;
            Validate(s);return new GameWorld(s);
        }
        private string BathroomTravel(SoloPlayer p,string target)
        {
            if(state.schema<BathroomLayout.Schema || Math.Abs(p.x-(p.zone==BathroomLayout.Area?BathroomLayout.ExitX:BathroomLayout.HallX))>65 || Math.Abs(p.y-BedroomLayout.DoorY)>65)return "door-too-far";
            if(p.visit>=long.MaxValue-1)return "visit-limit";
            var slot=Array.FindIndex(state.players,v=>v.id==p.id);
            ClearFixture(p);p.activity="";p.zone=target;p.visit++;p.y=100;
            p.x=target==BathroomLayout.Area?320+slot*130:BathroomLayout.HallX-165-slot*100;
            foreach(var t in state.toys.Where(t=>t.holder==p.id)){t.zone=target;t.x=p.x;t.y=p.y;Touch(t);}
            return null;
        }
        private string ApplyBathroom(SoloCommand c,SoloPlayer p)
        {
            if(state.schema<BathroomLayout.Schema)return "wrong-area";
            if(c.action==SoloAction.LeaveFixture){ClearFixture(p);p.y=80;return null;}
            if(c.action==SoloAction.SetFixture && c.target=="bath-splash")
            {if(!BathroomLayout.Bath(p.fixture))return "invalid-fixture";p.useSeconds=0;return null;}
            if(c.action!=SoloAction.UseFixture || !BathroomLayout.Usable(c.target))return "invalid-fixture";
            if(state.players.Any(v=>v.id!=p.id && v.fixture==c.target))return "fixture-busy";
            foreach(var t in state.toys.Where(t=>t.holder==p.id)){t.holder="";t.container="";t.x=p.x;t.y=Math.Max(35,p.y-65);Touch(t);}
            p.fixture=c.target;p.useSeconds=0;p.activity="";p.x=BathroomLayout.X(c.target);p.y=BathroomLayout.Y;
            return null;
        }
    }
}
