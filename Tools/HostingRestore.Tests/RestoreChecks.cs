using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

namespace HostingRestore;

public static class RestoreChecks
{
    static void Check(bool condition) { if (!condition) throw new Exception("Assertion failed."); }
    static void Refuse(Action action)
    { try { action(); } catch { return; } throw new Exception("Invalid restore was accepted."); }
    static string Encode<T>(T value) => RestoreStage.Encode(value);
    static string Id() => Guid.NewGuid().ToString("N");
    static T Copy<T>(T value) => JsonSerializer.Deserialize<T>(Encode(value), RestoreStage.Json);
    static string Hash(string text) => RecoveryTransfer.Hash(Encoding.UTF8.GetBytes(text));
    static void WriteRecord(string path, string payload)
    { File.WriteAllText(path, "LITTLEWEEPS-SOLO-1\n" + Hash(payload) + "\n" + payload, new UTF8Encoding(false)); }
    static ExpectedCheckpoint Expected(RecoveryRecord record, string payload) => new() {
        family = record.family, authority = record.authority, world = record.world, epoch = record.epoch,
        snapshotWorld = record.snapshot.worldId, checkpoint = record.checkpoint,
        revision = record.snapshot.revision, payloadSha256 = Hash(payload), profiles = record.snapshot.players.Select(p => p.id).ToArray()
    };

    static (RecoveryRecord record, SoloCommand pour) Fixture()
    {
        var seed = SoloWorld.WithAreas(SoloWorld.Create(Id(), Id(), Id(), Id())).Snapshot();
        // Valid partial contents exercise a transfer that would be observable
        // if it ran twice after restoring its retained operation receipt.
        seed.toys.Single(t => t.id == "plant-1").water = 2;
        seed.toys.Single(t => t.id == "bucket-1").water = 3;
        seed.toys.Single(t => t.id == "puddle-1").water = 0;
        var world = SoloWorld.Restore(seed);
        SoloCommand Send(int player, SoloAction action, string item = "", string target = "", string value = "", float x = 100, float y = 100)
        {
            var p = world.ReadPlayer(seed.players[player].id);
            var c = new SoloCommand { requestId = Id(), actor = p.id, zone = p.zone, visit = p.visit,
                expectedRevision = world.Revision, action = action, item = item, target = target, value = value, x = x, y = y };
            Check(world.Apply(c).Accepted); return c;
        }
        for (var i = 0; i < 128; i++) Send(0, SoloAction.Move, x: 200 + i);
        Send(0, SoloAction.StartActivity, value: "garden");
        Send(0, SoloAction.Grab, "bucket-1");
        var pour = Send(0, SoloAction.Drop, "bucket-1", "plant-1", x: 810, y: 330);
        Send(1, SoloAction.Travel, value: "creek");
        Send(1, SoloAction.Grab, "sponge-creek");
        for (var i = 0; i < 37; i++) world.AdvanceIdle(1, out _);
        return (new RecoveryRecord { version = 1, protocol = 3, content = 3, family = Id(), authority = Id(),
            world = Id(), epoch = Id(), checkpoint = 23, snapshot = world.Snapshot() }, pour);
    }

    public static int Run(string directory)
    {
        var root = Path.GetFullPath(directory);
        Check(!Directory.Exists(root) && !File.Exists(root)); Directory.CreateDirectory(root);
        var results = new List<object>(); var failures = 0;
        void Test(string name, Action<string, RecoveryRecord, ExpectedCheckpoint, SoloCommand> body)
        {
            var folder = Path.Combine(root, "case-" + results.Count); Directory.CreateDirectory(folder);
            var (record, pour) = Fixture(); var payload = Encode(record); var expected = Expected(record, payload);
            WriteRecord(Path.Combine(folder, "replica.save"), payload);
            try { body(folder, record, expected, pour); results.Add(new { name, passed = true }); Console.WriteLine("PASS " + name); }
            catch (Exception error) { failures++; results.Add(new { name, passed = false, error = error.ToString() }); Console.WriteLine("FAIL " + name + ": " + error.Message); }
        }
        Test("complete two-area state, partial bucket, completed actions and 128 receipts restore; stale holds clear once", (dir, r, e, unusedCommand) => {
            var source = Path.Combine(dir, "replica.save"); var before = File.ReadAllBytes(source);
            var state = RestoreStage.Publish(source, e, Path.Combine(dir, "replacement"));
            var wanted = Copy(r.snapshot); wanted.revision++;
            foreach (var t in wanted.toys.Where(t => !string.IsNullOrEmpty(t.holder))) { t.holder = ""; t.resetPending = false; }
            Check(Encode(state) == Encode(wanted)); Check(File.ReadAllBytes(source).SequenceEqual(before));
            Check(state.toys.Single(t => t.id == "bucket-1").water == 2 && state.receipts.Length == 128);
            Check(state.toys.Single(t => t.id == "plant-1").water == 3 && state.toys.Single(t => t.id == "puddle-1").water == 0);
            Check(state.players.Select(p => p.zone).Distinct().Count() == 2 && state.idleTimers.All(t => t.seconds == 37));
            Check(Encode(SoloWorld.Restore(state).Snapshot()) == Encode(state));
        });
        Test("four fresh connections resume; repeated pour is duplicate and conflicting reuse is refused without mutation", (dir, r, e, pour) => {
            var restored = RestoreStage.Publish(Path.Combine(dir, "replica.save"), e, Path.Combine(dir, "replacement"));
            var session = new FamilySession(SoloWorld.Restore(restored)); Check(session.ConnectedPlayers.Length == 0);
            for (ulong i = 0; i < 4; i++) Check(session.Attach(100 + i, restored.players[i].id, out _));
            Check(!session.Attach(999, restored.players[0].id, out _));
            var before = Encode(session.Checkpoint()); var result = session.Submit(100, pour);
            Check(result.Accepted && result.Duplicate && result.Outcome == "plant-watered" && Encode(session.Checkpoint()) == before);
            var changed = Copy(pour); changed.x++; result = session.Submit(100, changed);
            Check(!result.Accepted && result.Outcome == "request-id-reused" && Encode(session.Checkpoint()) == before);
            Check(!session.Submit(999, pour).Accepted && !session.Submit(101, pour).Accepted);
            var player = session.Checkpoint().players[1];
            result = session.Submit(101, new SoloCommand { requestId = Id(), actor = player.id, expectedRevision = restored.revision,
                zone = player.zone, visit = player.visit, action = SoloAction.Grab, item = "sponge-creek" });
            Check(result.Accepted && session.Checkpoint().toys.Single(t => t.id == "sponge-creek").holder == player.id);
            Check(session.Checkpoint().toys.Select(t => t.id).Distinct().Count() == 10);
        });
        Test("idle clocks survive but do not advance while empty; restored activity resets only after remaining active time", (dir, r, e, unusedCommand) => {
            var s = RestoreStage.Publish(Path.Combine(dir, "replica.save"), e, Path.Combine(dir, "replacement"));
            var session = new FamilySession(SoloWorld.Restore(s)); var before = Encode(session.Checkpoint());
            for (var i = 0; i < 200; i++) session.AdvanceIdle(1, out _);
            Check(before == Encode(session.Checkpoint())); Check(session.Attach(1, s.players[0].id, out _));
            for (var i = 0; i < 23; i++) session.AdvanceIdle(1, out _);
            var plant = session.Checkpoint().toys.Single(t => t.id == "plant-1"); Check(plant.water == 3 && plant.resetPending);
            for (var i = 0; i < 5; i++) session.AdvanceIdle(1, out _);
            Check(session.Checkpoint().toys.Single(t => t.id == "plant-1").water == 0);
            Check(session.Checkpoint().toys.Single(t => t.id == "bucket-1").water == 2);
        });
        Test("wrong lineage, epoch, serial, revision, roster or pinned hash cannot publish an authority", (dir, r, e, unusedCommand) => {
            var variants = new List<ExpectedCheckpoint>();
            foreach (var field in new[] { "family", "authority", "world", "epoch", "snapshotWorld" }) {
                var x = Copy(e); typeof(ExpectedCheckpoint).GetField(field).SetValue(x, Id()); variants.Add(x);
            }
            var v = Copy(e); v.checkpoint--; variants.Add(v); v = Copy(e); v.revision--; variants.Add(v);
            v = Copy(e); v.profiles[0] = Id(); variants.Add(v); v = Copy(e); v.payloadSha256 = new string('0', 64); variants.Add(v);
            foreach (var x in variants) Refuse(() => RestoreStage.Publish(Path.Combine(dir, "replica.save"), x, Path.Combine(dir, "bad")));
            Check(!Directory.Exists(Path.Combine(dir, "bad")));
        });
        Test("missing arrays or defaulted nested fields, duplicate fields, private adventure, client view and malformed states refuse", (dir, r, e, unusedCommand) => {
            var originals = Encode(r); var variants = new List<string>();
            foreach (var field in new[] { "receipts", "idleTimers", "players", "toys" }) {
                var n = JsonNode.Parse(originals); n["snapshot"].AsObject().Remove(field); variants.Add(n.ToJsonString());
            }
            var missing = JsonNode.Parse(originals); missing["snapshot"]["toys"][0].AsObject().Remove("water"); variants.Add(missing.ToJsonString());
            variants.Add(originals.Replace("\"version\": 1", "\"version\": 1, \"version\": 1"));
            variants.Add("{\"version\":2,\"localViewOrigin\":{\"world\":\"private\"},\"snapshot\":" + Encode(r.snapshot) + "}");
            variants.Add(Encode(new FamilySession(SoloWorld.Restore(r.snapshot)).View()));
            var invalid = Copy(r); invalid.snapshot.toys[0].water = 4; variants.Add(Encode(invalid));
            invalid = Copy(r); invalid.snapshot.idleTimers = null; variants.Add(Encode(invalid));
            foreach (var payload in variants) {
                WriteRecord(Path.Combine(dir, "bad.save"), payload); var expected = Copy(e); expected.payloadSha256 = Hash(payload);
                Refuse(() => RestoreStage.Publish(Path.Combine(dir, "bad.save"), expected, Path.Combine(dir, "bad")));
            }
            Check(!Directory.Exists(Path.Combine(dir, "bad")) && File.ReadAllText(Path.Combine(dir, "replica.save")).EndsWith(originals));
        });
        Test("future formats, truncated or damaged envelopes and oversized records refuse without older-backup substitution", (dir, r, e, unusedCommand) => {
            var path = Path.Combine(dir, "replica.save"); var original = File.ReadAllText(path); File.Copy(path, path + ".bak");
            foreach (var raw in new[] { original[..^10], original + "x", original.Replace("LITTLEWEEPS-SOLO-1", "LITTLEWEEPS-SOLO-2") }) {
                File.WriteAllText(path, raw); Refuse(() => RestoreStage.Publish(path, e, Path.Combine(dir, "bad")));
            }
            foreach (var kind in new[] { "version", "protocol", "content", "schema" }) {
                var future = Copy(r);
                if (kind == "schema") future.snapshot.schema = 3;
                else typeof(RecoveryRecord).GetField(kind).SetValue(future, kind == "version" ? 2 : 4);
                var payload = Encode(future); WriteRecord(path, payload); var expected = Copy(e); expected.payloadSha256 = Hash(payload);
                Refuse(() => RestoreStage.Publish(path, expected, Path.Combine(dir, "bad")));
            }
            WriteRecord(path, new string(' ', RecoveryTransfer.MaxBytes + 1));
            Refuse(() => RestoreStage.Publish(path, e, Path.Combine(dir, "bad")));
            Check(!Directory.Exists(Path.Combine(dir, "bad")) && File.ReadAllText(path + ".bak") == original);
        });
        Test("existing destination, interrupted publication and concurrent creator keep previous data untouched", (dir, r, e, unusedCommand) => {
            var source = Path.Combine(dir, "replica.save"); var target = Path.Combine(dir, "replacement");
            RestoreStage.Publish(source, e, target); var before = File.ReadAllBytes(Path.Combine(target, "world.save"));
            Refuse(() => RestoreStage.Publish(source, e, target)); Check(File.ReadAllBytes(Path.Combine(target, "world.save")).SequenceEqual(before));
            var interrupted = Path.Combine(dir, "interrupted");
            Refuse(() => RestoreStage.Publish(source, e, interrupted, () => throw new IOException("Injected interruption")));
            Check(!Directory.Exists(interrupted));
            var race = Path.Combine(dir, "race");
            Refuse(() => RestoreStage.Publish(source, e, race, () => { Directory.CreateDirectory(race); File.WriteAllText(Path.Combine(race, "sentinel"), "retained"); }));
            Check(File.ReadAllText(Path.Combine(race, "sentinel")) == "retained" && Directory.GetFiles(race).Length == 1);
        });
        File.WriteAllText(Path.Combine(root, "result.json"), Encode(new {
            passed = failures == 0, utc = DateTime.UtcNow, checks = results,
            scope = "Production C# RecoveryRecord, SoloWorld, FamilySession and CheckpointStore on Windows/.NET; isolated restore staging only. Not Unity transport, mobile authority or authenticated host grants."
        }));
        return failures == 0 ? 0 : 1;
    }
}
