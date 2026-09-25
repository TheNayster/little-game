using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class RecoveryRecord
    {
        public int version, protocol, content;
        public string family, authority, world, epoch;
        public long checkpoint;
        public SoloSnapshot snapshot;

        public void Validate(string expectedFamily, string expectedAuthority, string expectedWorld)
        {
            if(version > 1 || protocol > 3 || content > 3 || snapshot?.schema > 2)
                throw new NotSupportedException("Newer recovery format.");
            if(version != 1 || protocol != 3 || content != 3 || family != expectedFamily || authority != expectedAuthority ||
                world != expectedWorld || !FamilyPairing.Id(epoch) || checkpoint < 1 || checkpoint == long.MaxValue)
                throw new InvalidDataException("Recovery identity or version mismatch.");
            SoloWorld.Validate(snapshot);
            if(snapshot.schema != 2 || snapshot.players.Length != 4 || snapshot.idleTimers == null)
                throw new InvalidDataException("Incomplete shared recovery state.");
        }

        public void AcceptAfter(RecoveryRecord previous, string expectedEpoch, string snapshotWorld, string[] profiles)
        {
            if(epoch != expectedEpoch || snapshot.worldId != snapshotWorld || profiles == null ||
                !snapshot.players.Select(p => p.id).OrderBy(id => id, StringComparer.Ordinal).SequenceEqual(profiles.OrderBy(id => id, StringComparer.Ordinal)))
                throw new InvalidDataException("Checkpoint does not match the admitted world.");
            // Revision alone cannot order position/timer checkpoints: those can
            // change without a transaction. The epoch-local serial orders both.
            if(previous != null && (snapshot.worldId != previous.snapshot.worldId || snapshot.revision < previous.snapshot.revision ||
                (epoch == previous.epoch && checkpoint <= previous.checkpoint)))
                throw new InvalidDataException("Stale or conflicting recovery checkpoint.");
        }
    }

    [Serializable] public sealed class RecoveryChunk
    {
        public string transfer, epoch, hash, data;
        public int index, total;
    }
    [Serializable] public sealed class RecoveryAck
    {
        public string transfer, epoch, hash;
        public int next;
        public bool durable;
    }

    // Application-level chunks keep the reliable transport queue bounded. Only
    // one transfer is assembled at a time; incomplete bytes are never a save.
    public sealed class RecoveryTransfer
    {
        public const int ChunkBytes = 3072, MaxBytes = 256 * 1024;
        public const double Timeout = 40;
        private byte[] bytes;
        private string transfer, epoch, hash;
        private double started;
        public int Next { get; private set; }
        public bool Complete => bytes != null && Next == Count(bytes.Length);
        public static int Count(int length) => (length + ChunkBytes - 1) / ChunkBytes;
        public static string Hash(byte[] value)
        { using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(value)).Replace("-", "").ToLowerInvariant(); }
        public void Reset() { bytes = null; transfer = epoch = hash = null; Next = 0; }
        public static RecoveryChunk Chunk(byte[] source, string identity, string generation, int index)
        {
            if(source == null || source.Length < 1 || source.Length > MaxBytes || index < 0 || index >= Count(source.Length))
                throw new InvalidDataException("Invalid recovery chunk source.");
            return new RecoveryChunk { transfer = identity, epoch = generation, hash = Hash(source), index = index,
                total = source.Length, data = Convert.ToBase64String(source, index * ChunkBytes, Math.Min(ChunkBytes, source.Length - index * ChunkBytes)) };
        }
        public byte[] Add(RecoveryChunk part, string expectedEpoch, double now)
        {
            if(part == null || !FamilyPairing.Id(part.transfer) || part.epoch != expectedEpoch || !FamilyPairing.Id(part.epoch) ||
                !FamilyPairing.Hex(part.hash) || part.total < 1 || part.total > MaxBytes || part.index < 0 || part.index >= Count(part.total) ||
                part.data == null || part.data.Length > ChunkBytes * 4 / 3 || double.IsNaN(now) || double.IsInfinity(now))
                throw new InvalidDataException("Invalid recovery chunk header.");
            var decoded = Convert.FromBase64String(part.data);
            if(decoded.Length != Math.Min(ChunkBytes, part.total - part.index * ChunkBytes)) throw new InvalidDataException("Wrong chunk size.");
            if(bytes != null && now - started > Timeout) Reset();
            if(bytes == null || transfer != part.transfer)
            {
                if(part.index != 0) throw new InvalidDataException("Missing checkpoint start.");
                bytes = new byte[part.total]; transfer = part.transfer; epoch = part.epoch; hash = part.hash; Next = 0; started = now;
            }
            if(bytes.Length != part.total || epoch != part.epoch || hash != part.hash || part.index > Next)
                throw new InvalidDataException("Mixed or out-of-order recovery chunks.");
            var offset = part.index * ChunkBytes;
            if(part.index < Next)
            {
                for(var i = 0; i < decoded.Length; i++) if(bytes[offset + i] != decoded[i]) throw new InvalidDataException("Changed duplicate chunk.");
            }
            else { Buffer.BlockCopy(decoded, 0, bytes, offset, decoded.Length); Next++; }
            if(!Complete) return null;
            if(Hash(bytes) != hash) { Reset(); throw new InvalidDataException("Recovery checksum mismatch."); }
            return (byte[])bytes.Clone();
        }
    }
}
