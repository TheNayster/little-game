using System;
using System.IO;
using System.Text;
using LittleWeeps.Core;

namespace LittleWeeps.Adapters
{
    // Distinct from the child's solo draft and from the rendered network view.
    // Never run SoloWorld.Restore here: it intentionally releases held props.
    public sealed class RecoveryReplica
    {
        private readonly CheckpointStore store;
        private readonly Func<string, RecoveryRecord> decode;
        private readonly string family, authority, world;
        public RecoveryRecord Last { get; private set; }
        public RecoveryReplica(string path, string family, string authority, string world, Func<string, RecoveryRecord> decode)
        {
            this.decode = decode; this.family = family; this.authority = authority; this.world = world;
            store = new CheckpointStore(path, Valid);
            var current = store.Load();
            if(current.Status == CheckpointStatus.Corrupt || current.Status == CheckpointStatus.Unsupported)
                throw new InvalidDataException("Stored recovery checkpoint requires attention.");
            if(current.Payload != null) Last = Read(current.Payload);
        }
        private RecoveryRecord Read(string payload)
        {
            if(payload == null || Encoding.UTF8.GetByteCount(payload) > RecoveryTransfer.MaxBytes) throw new InvalidDataException("Recovery size limit.");
            var record = decode(payload);
            if(record == null) throw new InvalidDataException("Missing recovery record.");
            record.Validate(family, authority, world); return record;
        }
        private bool Valid(string payload)
        {
            try { Read(payload); return true; }
            catch(ArgumentException) { return false; }
            catch(InvalidOperationException) { return false; }
            catch(InvalidDataException) { return false; }
        }
        public RecoveryRecord Commit(byte[] bytes, string epoch, string snapshotWorld, string[] profiles)
        {
            if(bytes == null || bytes.Length > RecoveryTransfer.MaxBytes) throw new InvalidDataException("Recovery size limit.");
            var payload = new UTF8Encoding(false, true).GetString(bytes);
            var record = Read(payload);
            record.AcceptAfter(null, epoch, snapshotWorld, profiles);
            // A reconnect can resend the same durable checkpoint. Acknowledge
            // exact bytes without another disk write; changed bytes are conflict.
            if(Last != null && record.epoch == Last.epoch && record.checkpoint == Last.checkpoint)
            {
                if(record.epoch != epoch || record.snapshot.worldId != snapshotWorld || store.Load().Payload != payload)
                    throw new InvalidDataException("Conflicting checkpoint replay.");
                return Last;
            }
            record.AcceptAfter(Last, epoch, snapshotWorld, profiles);
            store.Save(payload);
            var verified = store.Load();
            if(verified.Status != CheckpointStatus.Loaded || verified.Payload != payload) throw new IOException("Recovery write not verified.");
            Last = record; return record;
        }
    }
}
