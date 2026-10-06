using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly RevealReactions homeReactions=new RevealReactions(),daycareReactions=new RevealReactions();
        public string[] RevealActors=>homeReactions.Active(Time.unscaledTime).Concat(daycareReactions.Active(Time.unscaledTime)).Select(r=>r.actor).ToArray();
        public int[] RevealVariants=>homeReactions.Active(Time.unscaledTime).Concat(daycareReactions.Active(Time.unscaledTime)).Select(r=>r.variant).ToArray();
        [Serializable] public sealed class RevealBody {public string actor,pose;public Vector2 point;public float scale;public bool visible,expression;}
        public RevealBody[] RevealBodies {
            get {
                RevealBody Body(string id,RectTransform root,GameCharacterVisual visual)=>new RevealBody{actor=id,point=root.anchoredPosition,scale=root.localScale.x/sceneScale,visible=root.gameObject.activeInHierarchy,pose=visual.Frame.Pose.ToString(),expression=visual.ActiveView!=null && visual.ActiveView.RevealExpressionVisible};
                var list=new System.Collections.Generic.List<RevealBody>{Body(Actor,avatar,characterVisual)};
                list.AddRange(friends.Select(f=>Body(f.Key,f.Value.root,f.Value.view)));
                list.AddRange(clubFriends.Select((f,i)=>Body(DaycarePlay.NpcId(i),f.root,f.visual)));
                return list.ToArray();
            }
        }
        public int HomeRevealEvents=>homeReactions.Events;
        public int DaycareRevealEvents=>daycareReactions.Events;
        private bool RevealReady=>!applicationPaused && (!Shared || shared.Connected);
        private string RevealWorld=>Shared?shared.View.worldId:World.WorldId;
        private RevealReactions.Reaction[] HomeReveal=>homeReactions.Active(Time.unscaledTime);
        private RevealReactions.Reaction[] DaycareReveal=>daycareReactions.Active(Time.unscaledTime);
        private void ObserveHomeReveals(HideState s)
        {
            // Props can contain multiple legacy slot IDs: group them as one cover.
            homeReactions.Observe(RevealWorld+"/home/"+s.round,s.round,s.hiders.Select(h=>new RevealReactions.Occupant(h.actor,h.slot<0?-1:HideAndSeek.Props[h.slot],h.mode==HiderMode.Found)),Time.unscaledTime,RevealReady && CurrentArea=="garden");
        }
        private void ObserveDaycareReveals(DaycarePlayState s)
        {
            daycareReactions.Observe(RevealWorld+"/daycare/"+s.round,s.round,
                s.members.Select(m=>new RevealReactions.Occupant(m.actor,m.attending?m.slot:-1,m.attending && m.found))
                .Concat(s.npcs.Select((n,i)=>new RevealReactions.Occupant(DaycarePlay.NpcId(i),n.hidden?n.slot:-1,n.found))),
                Time.unscaledTime,RevealReady && CurrentArea==DaycarePlay.HideZone);
        }
        private static CharacterFrame RevealFrame(RevealReactions.Reaction reaction,bool left)
        {
            var age=Time.unscaledTime-reaction.start;
            var pose=age<RevealReactions.SurpriseSeconds?CharacterPose.Surprise:reaction.variant==2?CharacterPose.Dance:CharacterPose.Wave;
            return new CharacterFrame(pose,0,reaction.variant==1?!left:left,age);
        }
        private void PresentHumanReveals(RevealReactions.Reaction[] reactions,bool daycare)
        {
            foreach(var r in reactions){
                if(r.actor.StartsWith("club-npc-",StringComparison.Ordinal))continue;
                var p=ReadPlayer(r.actor);if(p==null || p.zone!=CurrentArea || !string.IsNullOrEmpty(p.fixture) || p.activity!="" || p.stairs>0)continue;
                var visual=r.actor==Actor?characterVisual:friends.TryGetValue(r.actor,out var friend)?friend.view:null;
                var root=r.actor==Actor?avatar:friends.TryGetValue(r.actor,out var f)?f.root:null;
                if(visual==null || root==null || !root.gameObject.activeInHierarchy || visual.Frame.Speed>1)continue;
                if(r.actor==Actor && (destination.HasValue || stickDirection.sqrMagnitude>.01f || TravelPending))continue;
                var x=daycare?DaycarePlay.Cover(r.slot).X:HidePropX[r.slot];
                // Spread only still spectators; input and real walking always win.
                root.anchoredPosition=ToBoard(x+(r.index-(r.count-1)*.5f)*(daycare?68:70),p.y);
                if(daycare && r.count>3)root.localScale=Vector3.one*sceneScale*.65f;
                visual.PresentFrame(RevealFrame(r,false),0);
            }
        }
        private string[] ClubCoverOccupants(DaycarePlayState s,int slot)=>s.members.Where(m=>m.attending && m.slot==slot).Select(m=>m.actor)
            .Concat(s.npcs.Select((n,i)=>(n,i)).Where(p=>p.n.hidden && p.n.slot==slot).Select(p=>DaycarePlay.NpcId(p.i))).OrderBy(id=>id,StringComparer.Ordinal).ToArray();
        private Vector2 ClubHiddenPoint(DaycarePlayState s,int slot,string actor)
        {
            var people=ClubCoverOccupants(s,slot);var index=Array.IndexOf(people,actor);
            return ToBoard(DaycarePlay.Cover(slot).X+(index-(people.Length-1)*.5f)*Mathf.Min(76,208f/Mathf.Max(1,people.Length-1)),185);
        }
        private float ClubHiddenScale(DaycarePlayState s,int slot)=>Mathf.Min(.8f,2.75f/Mathf.Max(1,ClubCoverOccupants(s,slot).Length));
    }
}
