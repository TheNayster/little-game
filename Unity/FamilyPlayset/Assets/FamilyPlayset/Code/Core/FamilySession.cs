using System;
using System.Collections.Generic;
using System.Linq;

namespace LittleWeeps.Core
{
    // Connection lifetime around the same rules used by solo play. Authentication
    // is an adapter responsibility; transport IDs are never persistent player IDs.
    public sealed class FamilySession
    {
        private readonly SoloWorld world;
        private readonly Dictionary<ulong,string> connections=new Dictionary<ulong,string>();
        public FamilySession(SoloWorld world){this.world=world??throw new ArgumentNullException(nameof(world));}
        public long Revision=>world.Revision;
        public bool AmbientChanged=>world.AmbientChanged;
        public string[] ConnectedPlayers=>connections.Values.OrderBy(id=>id,StringComparer.Ordinal).ToArray();
        public bool TryPlayer(ulong connection,out string profile)=>connections.TryGetValue(connection,out profile);
        public bool Attach(ulong connection,string profile,out string reason)
        {
            reason="";
            if(connections.ContainsKey(connection)){reason="connection-in-use";return false;}
            if(!world.Snapshot().players.Any(p=>p.id==profile)){reason="unknown-profile";return false;}
            if(connections.ContainsValue(profile)){reason="profile-already-connected";return false;}
            if(connections.Count>=4){reason="family-full";return false;}
            connections.Add(connection,profile);world.JoinKingdomGroup(ConnectedPlayers);world.JoinPicnicGroup(ConnectedPlayers);world.JoinSandpitGroup(ConnectedPlayers);return true;
        }
        public SoloResult Submit(ulong connection,SoloCommand command)
        {
            if(!connections.TryGetValue(connection,out var profile))return new SoloResult(false,"not-connected",world.Revision);
            if(command==null || command.actor!=profile)return new SoloResult(false,"wrong-player",world.Revision);
            var result=world.Apply(command);
            if(result.Accepted && command.action==SoloAction.Park && command.value=="tag-join")
                world.IncludeNearbyTagPlayers(ConnectedPlayers);
            if(result.Accepted && command.action==SoloAction.Kingdom && (command.value=="start" || command.value=="replay")){world.JoinKingdomGroup(ConnectedPlayers);return new SoloResult(true,result.Outcome,world.Revision,result.Duplicate);}
            if(result.Accepted && command.action==SoloAction.Daycare && (command.value=="start" || command.value=="replay")){world.JoinPicnicGroup(ConnectedPlayers);return new SoloResult(true,result.Outcome,world.Revision,result.Duplicate);}
            if(result.Accepted && command.action==SoloAction.Sandpit && (command.value=="start" || command.value=="replay")){world.JoinSandpitGroup(ConnectedPlayers);return new SoloResult(true,result.Outcome,world.Revision,result.Duplicate);}
            return result;
        }
        public bool Detach(ulong connection)
        {
            if(!connections.TryGetValue(connection,out var profile))return false;
            connections.Remove(connection);
            world.ReleaseCreekFishing(profile);world.ReleaseCreekBoats(profile);world.ReleasePond(profile);world.ReleaseHideAndSeek(profile);world.ReleaseZoo(profile);
            world.ReleaseTag(profile);
            world.ReleaseDinosaurCare(profile);
            world.ReleaseSandpit(profile);world.ReleaseDaycare(profile);world.ReleaseKingdom(profile);
            world.ReleaseWaveRide(profile);
            world.ReleaseFixture(profile);
            world.CancelStairs(profile);
            foreach(var toy in world.ReadToys().Where(t=>t.holder==profile))
            {
                var player=world.ReadPlayer(profile);
                var result=world.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=profile,
                    expectedRevision=world.Revision,zone=player.zone,visit=player.visit,action=SoloAction.CancelGrab,item=toy.id});
                if(!result.Accepted)throw new InvalidOperationException("Could not release disconnected player's hold.");
            }
            return true;
        }
        public SoloSnapshot Checkpoint()=>world.Snapshot();
        public bool AdvanceIdle(double seconds,out bool visibleChange)
        {
            visibleChange=false;
            // An empty family world does not age items while everybody is away.
            if(connections.Count==0)return false;
            var joined=world.JoinKingdomGroup(ConnectedPlayers);joined|=world.JoinPicnicGroup(ConnectedPlayers);joined|=world.JoinSandpitGroup(ConnectedPlayers);var advanced=world.AdvanceIdle(seconds,out visibleChange,ConnectedPlayers);visibleChange|=joined;return joined || advanced;
        }
        // A client view is not a successor/recovery checkpoint: receipts stay on
        // the authority. Full recovery replication is a later, separate contract.
        public SoloSnapshot View()
        {
            var state=world.Snapshot();state.receipts=Array.Empty<SoloReceipt>();state.idleTimers=Array.Empty<GardenIdleTimer>();
            var keys=HomeTidying.Keys(state);
            state.homeTidyCues=state.homeIdleTimers.Where(t=>t.seconds>=HomeTidying.IdleSeconds).Select(t=>Array.IndexOf(keys,t.key)).ToArray();
            state.homeIdleTimers=Array.Empty<HomeIdleTimer>();return state;
        }
    }
}
