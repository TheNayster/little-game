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
        public string[] ConnectedPlayers=>connections.Values.OrderBy(id=>id,StringComparer.Ordinal).ToArray();
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
            foreach(var toy in world.ReadToys().Where(t=>t.holder==profile))
            {
                var result=world.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=profile,
                    expectedRevision=world.Revision,action=SoloAction.CancelGrab,item=toy.id});
                if(!result.Accepted)throw new InvalidOperationException("Could not release disconnected player's hold.");
            }
            return true;
        }
        public SoloSnapshot Checkpoint()=>world.Snapshot();
        // A client view is not a successor/recovery checkpoint: receipts stay on
        // the authority. Full recovery replication is a later, separate contract.
        public SoloSnapshot View()
        {
            var state=world.Snapshot();state.receipts=Array.Empty<SoloReceipt>();return state;
        }
    }
}
