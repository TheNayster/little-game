using System;
using System.Collections.Generic;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ClubPhase { Idle, Counting, Playing, Inspecting, Finished }
    [Serializable] public sealed class ClubMember
    {
        public string actor; public bool attending, invited, declined, found;
        public int slot=-1; public double safeUntil;
        public ClubMember Copy()=>(ClubMember)MemberwiseClone();
    }
    [Serializable] public sealed class ClubNpc
    {
        public string avatar; public float x,y=100; public int slot=-1; public bool hidden,found;
        public ClubNpc Copy()=>(ClubNpc)MemberwiseClone();
    }
    [Serializable] public sealed class DaycarePlayState
    {
        public int round,cursor,turns; public ClubPhase phase;
        public double clock,deadline,grace,inspection; public float teacherX=1200,teacherY=90;
        public string it=""; public int[] order=Array.Empty<int>();
        public ClubMember[] members; public ClubNpc[] npcs=Array.Empty<ClubNpc>();
        public DaycarePlayState Copy(){var g=(DaycarePlayState)MemberwiseClone();g.members=members.Select(m=>m.Copy()).ToArray();g.npcs=npcs.Select(n=>n.Copy()).ToArray();g.order=order.ToArray();return g;}
    }
    // These are internal Daycare maps. Home's broadcast hiding and Park's
    // presence-based Tag remain independent of the invitation-based clubs.
    public static class DaycarePlay
    {
        public const int Schema=49;
        public const string HideZone="daycare-hide",TagZone="daycare-tag";
        public const float NpcSpeed=300,TeacherSpeed=350,TagRadius=110;
        public static bool Area(string zone)=>zone==HideZone || zone==TagZone;
        public static string Zone(bool tag)=>tag?TagZone:HideZone;
        public static WalkPoint Cover(int slot)=>new WalkPoint(260+slot*375,185);
        public static string CoverName(int slot)=>new[]{"Tent","Bush","Screen","Blanket bench","Bush","Tent"}[slot];
        public static ClubMember Member(DaycarePlayState g,string actor)=>g?.members.FirstOrDefault(m=>m.actor==actor);
        public static string NpcId(int i)=>"club-npc-"+i;
        public static WalkPoint Floor(string zone,float x,float y)=>Area(zone)?new WalkPoint(Math.Max(80,Math.Min(2320,x)),Math.Max(60,Math.Min(240,y))):new WalkPoint(x,y);
        public static bool Point(float x,float y)=>KeepyRules.Finite(x) && KeepyRules.Finite(y) && x>=80 && x<=2320 && y>=60 && y<=240;
        public static void Move(ClubNpc n,WalkPoint goal,double dt)
        {var p=Towards(new WalkPoint(n.x,n.y),goal,NpcSpeed,dt);n.x=p.X;n.y=p.Y;}
        public static WalkPoint Towards(WalkPoint p,WalkPoint goal,float speed,double dt)
        {var dx=goal.X-p.X;var dy=goal.Y-p.Y;var length=Math.Sqrt(dx*dx+dy*dy);var step=Math.Min(length,speed*dt);return length==0?goal:new WalkPoint(p.X+(float)(dx/length*step),p.Y+(float)(dy/length*step));}
    }
    public sealed partial class SoloWorld
    {
        private readonly Dictionary<string,WalkPoint> clubPrevious=new Dictionary<string,WalkPoint>();
        public DaycarePlayState ReadDaycarePlay(bool tag)=>(tag?state.tagClub:state.hideClub)?.Copy();
        private static DaycarePlayState NewDaycarePlay(SoloSnapshot s)=>new DaycarePlayState{members=s.players.Select(p=>new ClubMember{actor=p.id}).ToArray()};
        private static void NormalizeDaycarePlay(SoloSnapshot s)
        {if(s.schema>=DaycarePlay.Schema)return;if(s.hideClub?.members==null || s.hideClub.members.Length==0)s.hideClub=null;if(s.tagClub?.members==null || s.tagClub.members.Length==0)s.tagClub=null;}
        private static void ValidateDaycarePlay(SoloSnapshot s)
        {
            foreach(var tag in new[]{false,true}){
                var g=tag?s.tagClub:s.hideClub;if(g==null && s.schema<DaycarePlay.Schema)continue;
                if(g==null || g.round<0 || g.round==int.MaxValue || !Enum.IsDefined(typeof(ClubPhase),g.phase) || !KeepyRules.Finite(g.clock) || g.clock<0 || !KeepyRules.Finite(g.deadline) || g.deadline<0 || !KeepyRules.Finite(g.grace) || g.grace<0 || !KeepyRules.Finite(g.inspection) || g.inspection<0 || !DaycarePlay.Point(g.teacherX,g.teacherY) || g.cursor<0 || g.cursor>6 || g.turns<0 || g.members==null || !g.members.Select(m=>m?.actor).OrderBy(x=>x).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x)) || g.npcs==null || g.order==null || g.it==null)throw new InvalidOperationException("Invalid Daycare club.");
                if(g.round==0?(g.phase!=ClubPhase.Idle || g.npcs.Length!=0 || g.order.Length!=0):(g.npcs.Length!=4 || !DaycareNpcCasts.Valid(g.npcs.Select(n=>n?.avatar).ToArray(),4) || g.order.Length!=6 || !g.order.OrderBy(i=>i).SequenceEqual(Enumerable.Range(0,6))))throw new InvalidOperationException("Invalid club cast or search route.");
                foreach(var m in g.members)if(m==null || m.slot< -1 || m.slot>5 || m.slot>=0 && (!m.attending || m.found || tag) || m.attending && (g.round==0 || s.players.Single(p=>p.id==m.actor).zone!=DaycarePlay.Zone(tag)) || m.invited && (m.attending || m.declined) || !KeepyRules.Finite(m.safeUntil) || m.safeUntil<0)throw new InvalidOperationException("Invalid club participant.");
                foreach(var n in g.npcs)if(n==null || !DaycarePlay.Point(n.x,n.y) || n.slot< -1 || n.slot>5 || n.hidden && (n.slot<0 || n.found || tag))throw new InvalidOperationException("Invalid club NPC.");
                if(tag && g.round>0 && g.it!="" && !g.npcs.Select((n,i)=>DaycarePlay.NpcId(i)).Contains(g.it) && !g.members.Any(m=>m.attending && m.actor==g.it))throw new InvalidOperationException("Missing Tag chaser.");
                if(!tag && g.phase==ClubPhase.Inspecting && g.cursor>=6 || tag && g.phase!=ClubPhase.Idle && g.phase!=ClubPhase.Counting && g.phase!=ClubPhase.Playing)throw new InvalidOperationException("Invalid club search phase.");
                if(!tag && g.npcs.Select(n=>n.slot).Where(i=>i>=0).Distinct().Count()!=g.npcs.Count(n=>n.slot>=0))throw new InvalidOperationException("NPCs must choose distinct hiding places.");
            }
        }
        private void ExitDaycarePlay(SoloPlayer p)
        {
            foreach(var g in new[]{state.hideClub,state.tagClub}){
                var m=DaycarePlay.Member(g,p.id);if(m==null)continue;
                if(m.attending){m.declined=true;m.attending=false;m.slot=-1;m.found=false;if(g.it==p.id){g.it=DaycarePlay.NpcId(0);g.grace=g.clock+ParkTag.GraceSeconds;}}
                m.invited=false;
            }
        }
        public bool ReleaseDaycarePlay(string actor)
        {
            var changed=false;foreach(var g in new[]{state.hideClub,state.tagClub}){var m=DaycarePlay.Member(g,actor);if(m==null || !m.attending)continue;m.attending=false;m.slot=-1;m.found=false;if(g.it==actor){g.it=DaycarePlay.NpcId(0);g.grace=g.clock+ParkTag.GraceSeconds;}changed=true;}
            if(changed)state.revision++;return changed;
        }
        // Invite only admitted players currently in Daycare. A pending/declined
        // invitation never travels them, and cannot continually pop up again.
        public bool InviteDaycarePlay(string[] connected)
        {
            var changed=false;
            foreach(var tag in new[]{false,true}){
                var g=tag?state.tagClub:state.hideClub;if(g==null || g.round==0 || !g.members.Any(m=>m.attending && connected.Contains(m.actor)) && !state.players.Any(p=>connected.Contains(p.id) && p.zone==DaycarePlay.Zone(tag)))continue;
                foreach(var p in state.players.Where(p=>connected.Contains(p.id) && p.zone=="daycare")){
                    var m=DaycarePlay.Member(g,p.id);if(m.attending || m.invited || m.declined)continue;
                    m.invited=true;DeclineOtherDaycare(p.id);if(state.vet!=null)state.vet.members.Single(v=>v.actor==p.id).declined=true;if(state.treasure!=null)state.treasure.members.Single(v=>v.actor==p.id).declined=true;changed=true;
                }
                // A reconnect to the actual activity resumes participation;
                // this is different from accepting a Daycare invitation.
                foreach(var p in state.players.Where(p=>connected.Contains(p.id) && p.zone==DaycarePlay.Zone(tag))){var m=DaycarePlay.Member(g,p.id);if(!m.attending){m.attending=true;m.declined=false;changed=true;}}
            }
            if(changed)state.revision++;return changed;
        }
        private static void ClubArrival(SoloPlayer p,DaycarePlayState g,ClubMember m,bool tag)
        {if(tag){var i=Array.IndexOf(g.members,m);p.x=240+i*220;p.y=90+i%2*90;}}
        private string DaycarePlayOperation(SoloCommand c,SoloPlayer p)
        {
            var parts=c.value.Split(':');if(parts.Length!=2 || parts[0]!="hide" && parts[0]!="tag")return "invalid-club-action";
            var tag=parts[0]=="tag";var op=parts[1];var g=tag?state.tagClub:state.hideClub;var m=DaycarePlay.Member(g,p.id);if(g==null || m==null)return "club-unavailable";
            if(op=="decline"){if(!m.invited || c.target!=g.round.ToString())return "invitation-ended";m.invited=false;m.declined=true;return null;}
            if(op=="leave"){if(!m.attending)return "not-in-club";TravelPlayer(p,"daycare");DeclineOtherDaycare(p.id);return null;}
            if(op=="start" || op=="replay"){
                if(p.zone!="daycare" && !(m.attending && p.zone==DaycarePlay.Zone(tag)))return "come-to-daycare";
                if(op=="replay" && !tag && g.phase!=ClubPhase.Finished)return "friends-still-hiding";
                if(g.round==0 || op=="replay" || g.phase==ClubPhase.Finished){
                    if(g.round>=int.MaxValue-1)return "round-limit";
                    var random=new Random(Guid.NewGuid().GetHashCode());var cast=DaycareNpcCasts.Pick(4,g.npcs.Select(n=>n.avatar).ToArray());
                    g.round++;g.phase=ClubPhase.Counting;g.deadline=g.clock+(tag?ParkTag.CountSeconds:HideAndSeek.CountSeconds);g.cursor=0;g.turns=0;g.inspection=0;g.teacherX=1200;g.teacherY=90;g.it=tag?p.id:"";g.grace=g.deadline+ParkTag.GraceSeconds;
                    g.order=Enumerable.Range(0,6).OrderBy(i=>random.Next()).ToArray();var slots=Enumerable.Range(0,6).OrderBy(i=>random.Next()).Take(4).ToArray();
                    g.npcs=cast.Select((id,i)=>new ClubNpc{avatar=id,x=560+i*340,y=100+i%2*70,slot=tag?-1:slots[i]}).ToArray();
                    foreach(var v in g.members){v.slot=-1;v.found=false;v.invited=false;v.declined=false;v.safeUntil=g.grace;}
                    clubPrevious.Clear();
                }
                var previousClub=tag?state.hideClub:state.tagClub;
                foreach(var invited in previousClub.members.Where(v=>v.invited)){invited.invited=false;invited.declined=true;}
                if(p.zone!=DaycarePlay.Zone(tag)){TravelPlayer(p,DaycarePlay.Zone(tag));ClubArrival(p,g,m,tag);}m.attending=true;m.invited=false;m.declined=false;DeclineOtherDaycare(p.id);return null;
            }
            if(c.target!=g.round.ToString() && op!="hide")return "old-club-round";
            if(op=="join"){
                if(p.zone!="daycare" || !m.invited)return "invitation-ended";
                TravelPlayer(p,DaycarePlay.Zone(tag));ClubArrival(p,g,m,tag);m.attending=true;m.invited=false;m.declined=false;m.safeUntil=g.clock+ParkTag.GraceSeconds;DeclineOtherDaycare(p.id);return null;
            }
            if(!m.attending || p.zone!=DaycarePlay.Zone(tag))return "not-in-club";
            if(!tag && op=="out"){m.slot=-1;m.found=false;p.y=90;return null;}
            if(tag || op!="hide" || g.phase!=ClubPhase.Counting)return "hiding-time-ended";
            var spot=c.target.Split('@');if(spot.Length!=2 || !int.TryParse(spot[0],out var slot) || slot<0 || slot>5 || spot[1]!=g.round.ToString())return "old-hiding-place";
            var at=DaycarePlay.Cover(slot);if(Math.Abs(p.x-at.X)>35 || Math.Abs(p.y-90)>35)return "walk-to-hide-space";
            m.slot=slot;m.found=false;p.x=at.X;p.y=at.Y;return null;
        }
        private bool AdvanceDaycarePlay(double seconds,string[] active,out bool visible)
        {
            visible=false;var changed=false;
            foreach(var tag in new[]{false,true}){
                var g=tag?state.tagClub:state.hideClub;if(g==null || g.round==0)continue;
                foreach(var m in g.members.Where(m=>m.attending && (state.players.Single(p=>p.id==m.actor).zone!=DaycarePlay.Zone(tag) || active!=null && !active.Contains(m.actor))).ToArray())visible|=ReleaseDaycarePlay(m.actor);
                if(!g.members.Any(m=>m.attending) || g.phase==ClubPhase.Finished)continue;
                var remaining=seconds;while(remaining>1e-9){var dt=Math.Min(.05,remaining);remaining-=dt;g.clock+=dt;changed=true;
                    if(tag){AdvanceClubTag(g,dt,ref visible);continue;}
                    var phase=g.phase;
                    if(phase==ClubPhase.Counting){
                        foreach(var n in g.npcs.Where(n=>!n.hidden)){var goal=DaycarePlay.Cover(n.slot);DaycarePlay.Move(n,goal,dt);if(Math.Abs(n.x-goal.X)<.01 && Math.Abs(n.y-goal.Y)<.01){n.hidden=true;visible=true;}}
                        if(g.clock>=g.deadline){g.phase=ClubPhase.Playing;visible=true;}
                    }else if(g.cursor<6){
                        var goal=DaycarePlay.Cover(g.order[g.cursor]);var target=new WalkPoint(goal.X,90);
                        if(phase==ClubPhase.Playing){var at=DaycarePlay.Towards(new WalkPoint(g.teacherX,g.teacherY),target,DaycarePlay.TeacherSpeed,dt);g.teacherX=at.X;g.teacherY=at.Y;if(Math.Abs(at.X-target.X)<.01){g.phase=ClubPhase.Inspecting;g.inspection=g.clock+.8;visible=true;}}
                        else if(g.clock>=g.inspection){
                            foreach(var n in g.npcs.Where(n=>n.hidden && n.slot==g.order[g.cursor])){n.hidden=false;n.found=true;n.y=90;}
                            foreach(var h in g.members.Where(h=>h.attending && h.slot==g.order[g.cursor])){h.found=true;h.slot=-1;var p=state.players.Single(p=>p.id==h.actor);p.y=90;}
                            g.cursor++;g.phase=ClubPhase.Playing;visible=true;
                        }
                        if(g.npcs.All(n=>n.found) && !g.members.Any(m=>m.attending && m.slot>=0)){g.phase=ClubPhase.Finished;visible=true;}
                    }else{g.phase=ClubPhase.Finished;visible=true;}
                }
            }
            return changed;
        }
        private void AdvanceClubTag(DaycarePlayState g,double dt,ref bool visible)
        {
            if(g.phase==ClubPhase.Counting){if(g.clock<g.deadline)return;g.phase=ClubPhase.Playing;visible=true;clubPrevious.Clear();}
            var points=g.members.Where(m=>m.attending).ToDictionary(m=>m.actor,m=>{var p=state.players.Single(v=>v.id==m.actor);return new WalkPoint(p.x,p.y);});
            for(var i=0;i<4;i++)points[DaycarePlay.NpcId(i)]=new WalkPoint(g.npcs[i].x,g.npcs[i].y);
            if(!points.ContainsKey(g.it)){g.it=DaycarePlay.NpcId(0);g.grace=g.clock+ParkTag.GraceSeconds;visible=true;}
            for(var i=0;i<4;i++){
                var id=DaycarePlay.NpcId(i);var n=g.npcs[i];WalkPoint goal;
                if(g.it==id){var target=points.Where(v=>v.Key!=id).OrderBy(v=>v.Key.StartsWith("club-npc-")?1:0).ThenBy(v=>Math.Pow(v.Value.X-n.x,2)+Math.Pow(v.Value.Y-n.y,2)).First();goal=target.Value;}
                else{
                    var chaser=points[g.it];var dx=n.x-chaser.X;var dy=n.y-chaser.Y;
                    // Runners loop back when children chase for a while, keeping
                    // a gentle tag attainable instead of endless perfect evasion.
                    var offer=!g.it.StartsWith("club-npc-") && (int)(g.clock+i*2)%9>=6;
                    if(offer)goal=new WalkPoint(chaser.X+(i%2==0?130:-130),chaser.Y);
                    else{if(Math.Abs(dx)<30)dx=(i%2==0?1:-1)*180;goal=DaycarePlay.Floor(DaycarePlay.TagZone,n.x+Math.Sign(dx)*330,n.y+Math.Sign(dy)*100);if(Math.Abs(goal.X-n.x)<10)goal=new WalkPoint(1200,100+i%2*100);}
                }
                DaycarePlay.Move(n,goal,dt);points[id]=new WalkPoint(n.x,n.y);
            }
            if(g.clock>=g.grace){var at=points[g.it];var old=clubPrevious.TryGetValue(g.it,out var origin)?origin:at;
                var target=points.Where(v=>v.Key!=g.it && (v.Key.StartsWith("club-npc-") || DaycarePlay.Member(g,v.Key).safeUntil<=g.clock))
                    .Select(v=>new {id=v.Key,d=ParkTag.SweptDistance(old,at,clubPrevious.TryGetValue(v.Key,out var before)?before:v.Value,v.Value)})
                    .Where(v=>v.d<=DaycarePlay.TagRadius).OrderBy(v=>v.d).FirstOrDefault();
                if(target!=null){g.it=target.id;g.grace=g.clock+ParkTag.GraceSeconds;g.turns=g.turns==int.MaxValue?0:g.turns+1;visible=true;}
            }
            clubPrevious.Clear();foreach(var pair in points)clubPrevious[pair.Key]=pair.Value;
        }
        private bool ExitClubCover(string actor)
        {var h=DaycarePlay.Member(state.hideClub,actor);if(h==null || h.slot<0)return false;h.slot=-1;h.found=false;return true;}
        private void AfterDaycarePlayAction(SoloCommand c,SoloPlayer p)
        {if(c.action==SoloAction.Move && ExitClubCover(p.id))p.y=Math.Max(60,p.y);}
    }
}
