using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

namespace HostingRestore;

// Lab-only bridge from a client's complete replica to a NEW authority directory.
// This proves state restoration, not authorization to become a network host.
public sealed class ExpectedCheckpoint
{
    public string family, authority, world, epoch, snapshotWorld, payloadSha256;
    public string[] profiles;
    public long checkpoint, revision;
}

public static class RestoreStage
{
    public static readonly JsonSerializerOptions Json = new()
    { IncludeFields = true, WriteIndented = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    public static string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);

    static void Require(bool condition, string message)
    { if (!condition) throw new InvalidDataException(message); }

    static void Fields(JsonElement item, string names)
    {
        Require(item.ValueKind == JsonValueKind.Object, "Missing checkpoint object.");
        var actual = item.EnumerateObject().Select(p => p.Name).ToArray();
        var expected = names.Split(' ');
        Require(actual.Length == expected.Length && actual.Distinct().Count() == actual.Length &&
            actual.OrderBy(n => n, StringComparer.Ordinal).SequenceEqual(expected.OrderBy(n => n, StringComparer.Ordinal)),
            "Incomplete, duplicate or unsupported checkpoint fields.");
    }

    public static RecoveryRecord DecodeComplete(string payload)
    {
        Require(Encoding.UTF8.GetByteCount(payload) <= RecoveryTransfer.MaxBytes, "Checkpoint too large.");
        using var doc = JsonDocument.Parse(payload);
        var r = doc.RootElement;
        Fields(r, "version protocol content family authority world epoch checkpoint snapshot");
        var s = r.GetProperty("snapshot");
        Fields(s, "schema revision worldId players toys receipts idleTimers");
        // DTO field initializers make a missing receipts/timers property appear
        // as an empty array. A successor must not mistake missing data for empty.
        foreach (var (array, fields) in new[] {
            ("players", "id avatar activity zone visit x y"),
            ("toys", "id holder zone kind x y water wet resetPending"),
            ("receipts", "requestId fingerprint outcome revision"),
            ("idleTimers", "item seconds") })
        {
            Require(s.GetProperty(array).ValueKind == JsonValueKind.Array, "Missing checkpoint array.");
            foreach (var entry in s.GetProperty(array).EnumerateArray()) Fields(entry, fields);
        }
        return JsonSerializer.Deserialize<RecoveryRecord>(payload, Json);
    }

    public static RecoveryRecord Read(string source, ExpectedCheckpoint expected)
    {
        Require(expected != null && new[] { expected.family, expected.authority, expected.world,
                expected.epoch, expected.snapshotWorld }.All(FamilyPairing.Id) &&
                FamilyPairing.Hex(expected.payloadSha256) && expected.checkpoint > 0 && expected.revision >= 0 &&
                expected.profiles?.Length == 4 && expected.profiles.All(FamilyPairing.Id) &&
                expected.profiles.Distinct().Count() == 4, "Invalid expected checkpoint identity.");
        var store = new CheckpointStore(source, payload => {
            var record = DecodeComplete(payload);
            record.Validate(expected.family, expected.authority, expected.world);
            return true;
        });
        var loaded = store.Load();
        // This input is a pinned immutable transfer, not the running client's
        // save slot. Never quietly promote an older backup or staged fragment.
        Require(loaded.Status == CheckpointStatus.Loaded && loaded.Source == Path.GetFullPath(source),
            "The selected complete checkpoint is unavailable.");
        Require(RecoveryTransfer.Hash(Encoding.UTF8.GetBytes(loaded.Payload)) == expected.payloadSha256,
            "Selected checkpoint hash differs.");
        var value = DecodeComplete(loaded.Payload);
        value.Validate(expected.family, expected.authority, expected.world);
        value.AcceptAfter(null, expected.epoch, expected.snapshotWorld, expected.profiles);
        Require(value.checkpoint == expected.checkpoint && value.snapshot.revision == expected.revision,
            "Selected checkpoint order differs.");
        return value;
    }

    public static SoloSnapshot Publish(string source, ExpectedCheckpoint expected, string destination, Action beforePublish = null)
    {
        var record = Read(source, expected);
        // Restore clones the original state and releases pointer holds once.
        // No transport connection ID, drag preview or movement queue is reused.
        var session = new FamilySession(SoloWorld.Restore(record.snapshot));
        var snapshot = session.Checkpoint();
        var target = Path.GetFullPath(destination);
        Require(Directory.Exists(Path.GetDirectoryName(target)) && !Directory.Exists(target) && !File.Exists(target),
            "Use a new authority directory under an existing parent.");
        var staged = target + ".staged-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(staged);
        var store = new CheckpointStore(Path.Combine(staged, "world.save"), payload => {
            var s = JsonSerializer.Deserialize<SoloSnapshot>(payload, Json);
            SoloWorld.Validate(s); return s.schema == 2;
        });
        var serialized = Encode(snapshot);
        store.Save(serialized);
        Require(store.Load().Payload == serialized, "Staged authority verification failed.");
        File.WriteAllText(Path.Combine(staged, "restore-summary.json"), Encode(new {
            scope = "Isolated Windows restoration experiment; no hosting grant or enrollment included.",
            sourceHash = expected.payloadSha256, sourceEpoch = record.epoch, sourceCheckpoint = record.checkpoint,
            sourceRevision = record.snapshot.revision, restoredRevision = snapshot.revision,
            players = snapshot.players.Length, objects = snapshot.toys.Length, receipts = snapshot.receipts.Length,
            releasedHolds = record.snapshot.toys.Where(t => !string.IsNullOrEmpty(t.holder)).Select(t => t.id).ToArray(),
            activeConnections = session.ConnectedPlayers.Length
        }), new UTF8Encoding(false));
        beforePublish?.Invoke();
        // Create-only publication: a concurrent destination or interrupted stage
        // cannot replace progress. Retain failed stages for inspection, not startup.
        Directory.Move(staged, target);
        return snapshot;
    }
}
