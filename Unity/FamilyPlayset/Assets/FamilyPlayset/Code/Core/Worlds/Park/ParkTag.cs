using System;
using System.Collections.Generic;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum TagPhase { Idle, Counting, Playing }
    [Serializable] public sealed class TagMember
    {
        public string actor;
        public double safeUntil;
        public TagMember Copy()=>(TagMember)MemberwiseClone();
    }
    [Serializable] public sealed class TagState
    {
        public TagPhase phase;
        public double clock, starts, grace;
        public string it="";
        public int turns;
        public float npcX=1400, npcY=70;
        public TagMember[] members=Array.Empty<TagMember>();
        public TagState Copy(){var t=(TagState)MemberwiseClone();t.members=members.Select(m=>m.Copy()).ToArray();return t;}
    }
    public static class ParkTag
    {
        public const string Npc="tag-bandit";
        public const double CountSeconds=3, GraceSeconds=2.5;
        public const float Radius=145, NpcSpeed=220;
        public static bool Member(TagState t,string id)=>t?.members.Any(m=>m.actor==id)==true;
        public static bool Eligible(SoloPlayer p)=>p.zone=="park" && p.fixture=="" && p.stairs==0;
        public static WalkPoint NpcPoint(TagState t)=>new WalkPoint(t.npcX,t.npcY);
        // Closest relative position during an authority sample, not visual
        // overlap on one client. Crossings cannot tunnel between two samples.
        public static float SweptDistance(WalkPoint a0,WalkPoint a1,WalkPoint b0,WalkPoint b1)
        {
            var x=a0.X-b0.X;var y=a0.Y-b0.Y;
            var dx=a1.X-b1.X-x;var dy=a1.Y-b1.Y-y;var length=dx*dx+dy*dy;
            var u=length>0?Math.Max(0,Math.Min(1,-(x*dx+y*dy)/length)):0;
            return (float)Math.Sqrt((x+u*dx)*(x+u*dx)+(y+u*dy)*(y+u*dy));
        }
    }
    public sealed partial class GameWorld
    {
        private readonly HashSet<string> tagOptOut=new HashSet<string>();
        private readonly Dictionary<string,WalkPoint> tagPrevious=new Dictionary<string,WalkPoint>();
        public TagState ReadTag()=>state.park?.tag?.Copy();
        public static GameWorld WithTag(GameWorld world)
        {
            world=WithPond(world);if(world.state.park.tag!=null)return world;
            var s=world.Snapshot();s.park.tag=new TagState();s.revision++;Validate(s);return new GameWorld(s);
        }
        private static void SuspendRestoredTag(SoloSnapshot s)
        {if(s.park?.tag!=null && s.park.tag.phase!=TagPhase.Idle){s.park.tag=new TagState{clock=s.park.tag.clock};s.revision++;}}
        private static void ValidateTag(SoloSnapshot s)
        {
            var t=s.park?.tag;if(t==null)return; // Old park records have no optional session.
            if(!Enum.IsDefined(typeof(TagPhase),t.phase) || !KeepyRules.Finite(t.clock) || t.clock<0 || !KeepyRules.Finite(t.starts) || t.starts<0 ||
                !KeepyRules.Finite(t.grace) || t.grace<0 || !KeepyRules.Finite(t.npcX) || t.npcX<40 || t.npcX>4760 || !KeepyRules.Finite(t.npcY) || t.npcY<40 || t.npcY>320 ||
                t.turns<0 || t.members==null || t.members.Length>4 || t.members.Select(m=>m?.actor).Distinct().Count()!=t.members.Length ||
                t.members.Any(m=>m==null || !s.players.Any(p=>p.id==m.actor) || !KeepyRules.Finite(m.safeUntil) || m.safeUntil<0) ||
                (t.phase==TagPhase.Idle?(t.members.Length!=0 || t.it!=""):(t.members.Length==0 || !(ParkTag.Member(t,t.it) || t.members.Length==1 && t.it==ParkTag.Npc))))
                throw new InvalidOperationException("Invalid park tag session.");
        }
        private string TagOperation(SoloCommand c,SoloPlayer p)
        {
            if(state.park?.tag==null || p.zone!="park")return "wrong-area";
            var t=state.park.tag;
            if(c.value=="tag-leave"){tagOptOut.Add(p.id);CancelTag(p.id);return null;}
            if(c.value!="tag-join")return "invalid-tag-action";
            tagOptOut.Remove(p.id);if(ParkTag.Member(t,p.id))return null;
            AddTagMember(p);return null;
        }
        private void AddTagMember(SoloPlayer p)
        {
            var t=state.park.tag;
            // Joining a game releases only this child's support and held props.
            ClearFixture(p);p.activity="";
            foreach(var toy in state.toys.Where(v=>v.holder==p.id)){toy.holder="";toy.x=p.x;toy.y=Math.Max(35,p.y-65);Touch(toy);}
            if(t.phase==TagPhase.Idle){t.phase=TagPhase.Counting;t.starts=t.clock+ParkTag.CountSeconds;t.it=p.id;t.turns=0;tagOptOut.Clear();t.npcX=Math.Max(40,Math.Min(4760,p.x+220));t.npcY=Math.Max(40,Math.Min(320,p.y));tagPrevious.Clear();}
            t.members=t.members.Concat(new[]{new TagMember{actor=p.id,safeUntil=t.clock+ParkTag.GraceSeconds}}).ToArray();
            if(t.it==ParkTag.Npc && t.members.Length>1){t.it=t.members[0].actor;t.grace=t.clock+ParkTag.GraceSeconds;}
            tagPrevious.Remove(p.id);
        }
        // Presence comes from the authority's admitted connections, never all
        // saved profiles. Solo keeps its one real player and NPC fallback.
        private bool IncludeTagPlayers(string[] connected)
        {
            var t=state.park?.tag;if(t==null || t.phase==TagPhase.Idle || connected==null)return false;
            var changed=false;
            foreach(var p in state.players.Where(p=>connected.Contains(p.id) && p.zone=="park" && p.stairs==0))
                if(!tagOptOut.Contains(p.id) && !ParkTag.Member(t,p.id)){AddTagMember(p);changed=true;}
            return changed;
        }
        public void IncludeNearbyTagPlayers(string[] connected)
        {if(IncludeTagPlayers(connected))state.revision++;}
        private bool CancelTag(string actor)
        {
            var t=state.park?.tag;if(!ParkTag.Member(t,actor))return false;
            t.members=t.members.Where(m=>m.actor!=actor).ToArray();tagPrevious.Remove(actor);
            if(t.members.Length==0){t.phase=TagPhase.Idle;t.it="";tagPrevious.Clear();}
            else if(t.it==actor || t.it==ParkTag.Npc && t.members.Length>1){t.it=t.members[0].actor;t.grace=t.clock+ParkTag.GraceSeconds;}
            if(t.members.Length==1){var child=state.players.Single(p=>p.id==t.members[0].actor);t.npcX=Math.Max(40,Math.Min(4760,child.x+220));t.npcY=Math.Max(40,Math.Min(320,child.y));tagPrevious.Remove(ParkTag.Npc);}
            return true;
        }
        public bool ReleaseTag(string actor)
        {tagOptOut.Remove(actor);var changed=CancelTag(actor);if(changed)state.revision++;return changed;}
        private void AfterTagAction(SoloCommand c,SoloPlayer p)
        {
            if(p.zone!="park")tagOptOut.Remove(p.id);
            if(!ParkTag.Member(state.park?.tag,p.id))return;
            if(!ParkTag.Eligible(p) || c.action==SoloAction.Grab || c.action==SoloAction.StartActivity || c.action==SoloAction.LeaveActivity){CancelTag(p.id);if(p.zone=="park")tagOptOut.Add(p.id);else tagOptOut.Remove(p.id);}
        }
        private bool AdvanceTag(double dt,string[] active,out bool visible)
        {
            visible=false;var t=state.park?.tag;if(t==null || t.phase==TagPhase.Idle)return false;
            var connected=active;active=active??t.members.Select(m=>m.actor).ToArray();
            foreach(var m in t.members.ToArray())if(!active.Contains(m.actor) || !ParkTag.Eligible(state.players.Single(p=>p.id==m.actor)))visible|=CancelTag(m.actor);
            if(t.phase==TagPhase.Idle)return visible;
            visible|=IncludeTagPlayers(connected);
            t.clock+=dt;
            if(t.phase==TagPhase.Counting){if(t.clock<t.starts)return true;visible=true;t.phase=TagPhase.Playing;t.grace=t.clock+ParkTag.GraceSeconds;tagPrevious.Clear();}
            var points=t.members.ToDictionary(m=>m.actor,m=>{var p=state.players.Single(v=>v.id==m.actor);return new WalkPoint(p.x,p.y);});
            if(t.members.Length==1){
                var child=points[t.members[0].actor];var dx=child.X-t.npcX;var dy=child.Y-t.npcY;
                // The playmate circles nearby; when the child is chasing, it
                // periodically approaches to offer an attainable gentle tag.
                if(t.it!=ParkTag.Npc && ((int)t.clock%8)<5){dx=-dx+(dx>=0?-170:170);dy=70-t.npcY;}
                var length=Math.Sqrt(dx*dx+dy*dy);var step=Math.Min(length,ParkTag.NpcSpeed*dt);
                if(length>0){t.npcX=Math.Max(40,Math.Min(4760,t.npcX+(float)(dx/length*step)));t.npcY=Math.Max(40,Math.Min(320,t.npcY+(float)(dy/length*step)));}
                // Clock and NPC movement ride the existing park motion stream.
                // Only role/phase changes need a durable revision and full publish.
                points[ParkTag.Npc]=ParkTag.NpcPoint(t);
            }
            if(t.clock>=t.grace){
                var origin=points[t.it];var previous=tagPrevious.TryGetValue(t.it,out var old)?old:origin;
                var target=points.Where(v=>v.Key!=t.it && (v.Key==ParkTag.Npc || t.members.Single(m=>m.actor==v.Key).safeUntil<=t.clock))
                    .Select(v=>new {id=v.Key,distance=ParkTag.SweptDistance(previous,origin,tagPrevious.TryGetValue(v.Key,out var prior)?prior:v.Value,v.Value)})
                    .Where(v=>v.distance<=ParkTag.Radius).OrderBy(v=>v.distance).ThenBy(v=>v.id,StringComparer.Ordinal).FirstOrDefault();
                if(target!=null){t.it=target.id;t.grace=t.clock+ParkTag.GraceSeconds;t.turns=t.turns==int.MaxValue?0:t.turns+1;visible=true;}
            }
            tagPrevious.Clear();foreach(var pair in points)tagPrevious[pair.Key]=pair.Value;
            return true;
        }
    }
}
