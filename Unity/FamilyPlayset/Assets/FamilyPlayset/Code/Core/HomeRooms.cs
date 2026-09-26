using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Internal Home places preserve the existing garden identity and coordinates.
    // They are never top-level travel destinations. Bedroom ownership/furnishings
    // build on this boundary in the next slice; no rooms are allocated on entry.
    public static class HomeRooms
    {
        public const int Schema=6;
        public const string Landing="home-upstairs";
        public const double StairDuration=2.4;
        public const float LowerX=-2980, LowerY=420, UpperX=540, UpperY=400;
        public static bool Internal(string zone)=>zone==Landing;
        public static bool Property(string zone)=>zone=="garden" || Internal(zone);
        public static float EntryX(string zone)=>zone=="garden"?LowerX:UpperX;
        public static float EntryY(string zone)=>zone=="garden"?LowerY:UpperY;
        public static bool NearEntry(SoloPlayer p)=>Property(p.zone) &&
            Math.Abs(p.x-EntryX(p.zone))<=65 && Math.Abs(p.y-EntryY(p.zone))<=65;
        public static bool ValidTransit(SoloPlayer p)=>KeepyRules.Finite(p.stairs) && p.stairs>=0 && p.stairs<StairDuration &&
            (p.stairs==0 || Property(p.zone) && p.x==EntryX(p.zone) && p.y==EntryY(p.zone) &&
             string.IsNullOrEmpty(p.fixture) && p.activity=="");
    }
    public sealed partial class SoloWorld
    {
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
            var origin=t.id==t.kind.ToString().ToLowerInvariant()+"-1"?"garden":
                t.id==t.kind.ToString().ToLowerInvariant()+"-creek"?"creek":"";
            return origin!="" && (AreaOf(t.zone)==origin || schema>=HomeRooms.Schema && origin=="garden" &&
                Carryable(t.kind) && HomeRooms.Internal(t.zone));
        }
    }
}
