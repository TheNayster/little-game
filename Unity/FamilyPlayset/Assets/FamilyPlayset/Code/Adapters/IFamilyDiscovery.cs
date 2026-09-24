using System;

namespace LittleWeeps.Adapters
{
    public sealed class FamilyEndpoint
    {
        public string address;
        public ushort port;
        public uint networkInterface;
    }
    // Platform adapters find endpoints; only the encrypted game handshake admits
    // a player. No discovery result is an authorization decision.
    public interface IFamilyDiscovery : IDisposable
    {
        int Error {get;}
        void Browse();
        void Tick(double clock);
        FamilyEndpoint Take();
    }
}
