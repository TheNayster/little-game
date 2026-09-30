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
        public string[] ConnectedPlayers=>connections.Values.OrderBy(id=>id,StringComparer.Ordinal).ToArray();
        public bool TryPlayer(ulong connection,out string profile)=>connections.TryGetValue(connection,out profile);
        public bool Attach(ulong connection,string profile,out string reason)
        {
            reason="";
            if(connections.ContainsKey(connection)){reason="connection-in-use";return false;}
            if(!world.Snapshot().players.Any(p=>p.id==profile)){reason="unknown-profile";return false;}
            if(connections.ContainsValue(profile)){reason="profile-already-connected";return false;}
            if(connections.Count>=4){reason="family-full";return false;}
            connections.Add(connection,profile);return true;
        }
        public SoloResult Submit(ulong connection,SoloCommand command)
        {
            if(!connections.TryGetValue(connection,out var profile))return new SoloResult(false,"not-connected",world.Revision);
            if(command==null || command.actor!=profile)return new SoloResult(false,"wrong-player",world.Revision);
            return world.Apply(command);
        }
        public bool Detach(ulong connection)
        {
            if(!connections.TryGetValue(connection,out var profile))return false;
            connections.Remove(connection);
            world.ReleasePond(profile);world.ReleaseKingdom(profile);world.ReleaseHideAndSeek(profile);
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
            return connections.Count>0 && world.AdvanceIdle(seconds,out visibleChange,ConnectedPlayers);
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
