using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ElephantStoryPhase { Available, Searching, Carried, Returned, Reacting, Complete }
    [Serializable] public sealed class ElephantStoryState
    {
        public long session, nextSession;
        public ElephantStoryPhase phase;
        public int hidingSpot;
        public string carrier="";
        public bool cluePending;
        public double resetAge;
        public float x=560,y=180;
        public ElephantStoryState Copy()=>(ElephantStoryState)MemberwiseClone();
    }
    public static class ElephantStory
    {
        public const float BasketX=1390, BasketY=400, ElephantX=1190, ElephantY=320;
        public const double ReplayDelay=8, ClueDuration=1.1, ReactionDuration=1.8;
        public static bool AnimalPhase(ZooPhase phase)=>phase==ZooPhase.StoryClue || phase==ZooPhase.StoryWalk || phase==ZooPhase.StoryReact;
        public static WalkPoint Spot(int i)=>i==0?new WalkPoint(560,180):new WalkPoint(2130,740);
    }
    public sealed partial class GameWorld
    {
        private bool StoryHeld(string actor)=>state.zoo?.story?.carrier==actor;
        private static void ResetStory(ZooState z)
        {var high=z.story?.nextSession??0;z.story=new ElephantStoryState{nextSession=high};}
        private bool DropStory(string actor)
        {
            var s=state.zoo?.story;if(s==null || s.carrier!=actor)return false;
            var p=state.players.Single(v=>v.id==actor);var spot=ElephantStory.Spot(p.x<1200?0:1);
            s.x=spot.X;s.y=spot.Y;s.carrier="";s.phase=ElephantStoryPhase.Searching;return true;
        }
        private void EndStoryIfEmpty()
        {
            if(state.zoo==null || elephantVisitors.Count>0)return;
            var z=state.zoo;if(ElephantStory.AnimalPhase(z.animals[0].phase))ZooLayout.Routine(z.animals[0]);ResetStory(z);
        }
        private string StoryOperation(SoloCommand c,SoloPlayer p)
        {
            if(!AtElephant(p))return "come-to-exhibit";
            var z=state.zoo;var s=z.story;
            if(c.value=="story-start"){
                if(s.phase!=ElephantStoryPhase.Available && s.phase!=ElephantStoryPhase.Complete)return null;
                if(s.phase==ElephantStoryPhase.Complete && s.resetAge<ElephantStory.ReplayDelay)return "story-resting";
                if(s.nextSession>=long.MaxValue-1)return "story-limit";
                var high=++s.nextSession;
                // Once per session; never advances the animal random stream.
                uint seed=2166136261;foreach(var ch in state.worldId)seed=unchecked((seed^ch)*16777619);
                var spot=(int)((seed+(ulong)high)%2);var point=ElephantStory.Spot(spot);
                z.story=new ElephantStoryState{session=high,nextSession=high,phase=ElephantStoryPhase.Searching,hidingSpot=spot,x=point.X,y=point.Y,cluePending=true};return null;
            }
            if(!long.TryParse(c.target,out var session) || session!=s.session || session==0)return "story-changed";
            if(c.value=="story-drop")return DropStory(p.id)?null:"story-not-carrier";
            if(c.value=="story-pickup"){
                if(s.phase!=ElephantStoryPhase.Searching || s.carrier!="")return "story-helping";
                if(Math.Abs(p.x-s.x)>160 || p.y>160)return "story-closer";
                var f=z.food.Single(v=>v.actor==p.id);
                if(f.species!="" || f.preparing || z.careMembers.Contains(p.id) || state.toys.Any(t=>t.holder==p.id))return "hands-full";
                s.phase=ElephantStoryPhase.Carried;s.carrier=p.id;return null;
            }
            if(c.value=="story-return"){
                if(s.phase!=ElephantStoryPhase.Carried || s.carrier!=p.id)return "story-not-carrier";
                if(Math.Abs(p.x-ElephantStory.BasketX)>160 || p.y>160)return "story-closer";
                s.carrier="";s.cluePending=false;s.phase=ElephantStoryPhase.Returned;return null;
            }
            return "unknown-story-action";
        }
        private bool AdvanceStory(double seconds,string[] active,bool nearby)
        {
            var z=state.zoo;var s=z.story;var changed=false;
            if(s.carrier!=""){
                var p=state.players.Single(v=>v.id==s.carrier);
                if(!AtElephant(p) || active!=null && !active.Contains(p.id))changed|=DropStory(p.id);
            }
            if(!nearby && s.phase!=ElephantStoryPhase.Available){EndStoryIfEmpty();return true;}
            if(s.phase==ElephantStoryPhase.Complete)s.resetAge=Math.Min(ElephantStory.ReplayDelay,s.resetAge+seconds);
            // Food can consume a short acknowledgment; never replay it later.
            if(s.phase==ElephantStoryPhase.Reacting && z.animals[0].phase!=ZooPhase.StoryReact){s.phase=ElephantStoryPhase.Complete;s.resetAge=0;changed=true;}
            return changed;
        }
        private static void ValidateStory(SoloSnapshot snapshot)
        {
            var s=snapshot.zoo.story;
            if(s==null || s.session<0 || s.nextSession<s.session || s.nextSession>=long.MaxValue || !Enum.IsDefined(typeof(ElephantStoryPhase),s.phase) ||
                s.hidingSpot<0 || s.hidingSpot>1 || !ZooClock(s.resetAge,ElephantStory.ReplayDelay) || s.carrier==null ||
                s.phase!=ElephantStoryPhase.Available && s.session==0 || (s.phase==ElephantStoryPhase.Carried)!=(s.carrier!="") ||
                !HideAndSeek.Finite(s.x) || !HideAndSeek.Finite(s.y) || s.x<500 || s.x>2200 || s.y<100 || s.y>740)
                throw new InvalidOperationException("Invalid elephant story.");
            if(s.carrier!=""){
                var p=snapshot.players.SingleOrDefault(v=>v.id==s.carrier);
                if(p==null || !AtElephant(p) || snapshot.zoo.food.Any(f=>f.actor==s.carrier && (f.species!="" || f.preparing)) ||
                    snapshot.zoo.careMembers.Contains(s.carrier) || snapshot.toys.Any(t=>t.holder==s.carrier))throw new InvalidOperationException("Invalid story carrier.");
            }
        }
    }
}
