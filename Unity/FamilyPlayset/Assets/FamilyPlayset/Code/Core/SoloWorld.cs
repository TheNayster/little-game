using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ToyKind { Bucket, Sponge, Tap, Plant, Puddle }
    public enum SoloAction { Move, ChangeAvatar, Grab, Drop, CancelGrab, StartActivity, LeaveActivity }
    [Serializable] public sealed class SoloPlayer
    {
        public string id, avatar = "blue-pup", activity = "";
        public float x = 420, y = 200;
        public SoloPlayer Copy() => (SoloPlayer)MemberwiseClone();
    }
    [Serializable] public sealed class SoloToy
    {
        public string id, holder = "";
        public ToyKind kind;
        public float x, y;
        public int water;
        public bool wet;
        public SoloToy Copy() => (SoloToy)MemberwiseClone();
    }
    [Serializable] public sealed class SoloReceipt
    {
        public string requestId, fingerprint, outcome;
        public long revision;
        public SoloReceipt Copy() => (SoloReceipt)MemberwiseClone();
    }
    [Serializable] public sealed class SoloSnapshot
    {
        public int schema = 1;
        public long revision;
        public string worldId;
        public SoloPlayer[] players;
        public SoloToy[] toys;
        public SoloReceipt[] receipts = Array.Empty<SoloReceipt>();
    }
    [Serializable] public sealed class SoloCommand
    {
        public string requestId, actor, item = "", target = "", value = "";
        public long expectedRevision;
        public SoloAction action;
        public float x, y;
        internal string Fingerprint() => string.Join("|", new[] { actor, item, target, value,
            ((int)action).ToString(CultureInfo.InvariantCulture), expectedRevision.ToString(CultureInfo.InvariantCulture),
            x.ToString("R", CultureInfo.InvariantCulture), y.ToString("R", CultureInfo.InvariantCulture) });
    }
    public readonly struct SoloResult
    {
        public readonly bool Accepted, Duplicate;
        public readonly string Outcome;
        public readonly long Revision;
        public SoloResult(bool accepted, string outcome, long revision, bool duplicate = false)
        { Accepted = accepted; Outcome = outcome; Revision = revision; Duplicate = duplicate; }
    }

    // A small authority for the solo prototype. Presentation never directly mutates it.
    // Network admission, rate limits and cross-device reconciliation belong to later gates.
    public sealed class SoloWorld
    {
        public const float Width = 1000, Height = 500, InteractionRadius = 90;
        private readonly SoloSnapshot state;
        public long Revision => state.revision;
        private SoloWorld(SoloSnapshot snapshot) { state = snapshot; }
        public static SoloWorld Create(params string[] playerIds)
        {
            var s = new SoloSnapshot { worldId = Guid.NewGuid().ToString("N"), players = playerIds.Select(id => new SoloPlayer { id = id }).ToArray(),
                toys = new[] {
                    new SoloToy { id = "bucket-1", kind = ToyKind.Bucket, x = 360, y = 130 },
                    new SoloToy { id = "sponge-1", kind = ToyKind.Sponge, x = 560, y = 120 },
                    new SoloToy { id = "tap-1", kind = ToyKind.Tap, x = 150, y = 340 },
                    new SoloToy { id = "plant-1", kind = ToyKind.Plant, x = 810, y = 330 },
                    new SoloToy { id = "puddle-1", kind = ToyKind.Puddle, x = 680, y = 140, water = 3 }
                } };
            return Restore(s);
        }
        public static SoloWorld Restore(SoloSnapshot snapshot)
        {
            Validate(snapshot);
            var copy = Clone(snapshot);
            // A pointer lease never survives closing the app or a recovered save.
            if (copy.toys.Any(t => !string.IsNullOrEmpty(t.holder)))
            {
                foreach (var toy in copy.toys) toy.holder = "";
                copy.revision++;
            }
            return new SoloWorld(copy);
        }
        public SoloSnapshot Snapshot() => Clone(state);
        // Presentation reads do not need the durable command receipt history.
        // Return detached copies so a view cannot mutate the authority.
        public SoloPlayer ReadPlayer(string id)
        {
            foreach(var player in state.players)if(player.id==id)return player.Copy();
            throw new ArgumentException("Unknown player.",nameof(id));
        }
        public SoloToy[] ReadToys()
        {
            var copies=new SoloToy[state.toys.Length];
            for(var i=0;i<copies.Length;i++)copies[i]=state.toys[i].Copy();
            return copies;
        }
        // A read-only picture hint, not permission to grab/drop or bypass distance.
        // Show combinations that currently change water/cleanup state.
        public bool HasUsefulInteraction(string itemId,string targetId)
        {
            SoloToy item=null,target=null;
            foreach(var toy in state.toys){if(toy.id==itemId)item=toy;if(toy.id==targetId)target=toy;}
            if(item==null || target==null || item==target)return false;
            if(item.kind==ToyKind.Bucket)
                return target.kind==ToyKind.Tap?item.water<3:target.kind==ToyKind.Plant && item.water>0 && target.water<3;
            return item.kind==ToyKind.Sponge && target.kind==ToyKind.Puddle && target.water>0;
        }
        private static SoloSnapshot Clone(SoloSnapshot s) => new SoloSnapshot { schema = s.schema, revision = s.revision, worldId = s.worldId,
            players = s.players.Select(p => p.Copy()).ToArray(), toys = s.toys.Select(t => t.Copy()).ToArray(), receipts = s.receipts.Select(r => r.Copy()).ToArray() };
        private static bool Id(string s) => !string.IsNullOrWhiteSpace(s) && s.Length <= 128 && !s.Contains("|");
        public static bool Position(float x, float y) => !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && x >= 0 && x <= Width && y >= 0 && y <= Height;
        private static bool Avatar(string s) => s == "blue-pup" || s == "orange-pup";
        private static bool Activity(string s) => s == "" || s == "garden" || s == "cleanup";
        public static void Validate(SoloSnapshot s)
        {
            if (s == null || s.schema != 1) throw new InvalidOperationException("Unsupported solo save schema.");
            if (!Id(s.worldId) || s.revision < 0 || s.revision == long.MaxValue || s.players == null || s.players.Length < 1 || s.players.Length > 4 ||
                s.toys == null || s.toys.Length != 5 || s.receipts == null || s.receipts.Length > 128) throw new InvalidOperationException("Invalid solo world record.");
            var ids = new HashSet<string>();
            foreach (var p in s.players)
                if (p == null || !Id(p.id) || !ids.Add(p.id) || !Avatar(p.avatar) || !Activity(p.activity) || !Position(p.x, p.y)) throw new InvalidOperationException("Invalid player record.");
            ids.Clear();
            foreach (var t in s.toys)
                if (t == null || !ids.Add(t.id ?? "") || !Enum.IsDefined(typeof(ToyKind), t.kind) || t.id != t.kind.ToString().ToLowerInvariant() + "-1" ||
                    !Position(t.x, t.y) || t.water < 0 || t.water > 3 ||
                    ((t.kind == ToyKind.Tap || t.kind == ToyKind.Sponge) && t.water != 0) ||
                    (t.kind != ToyKind.Sponge && t.wet) ||
                    (!string.IsNullOrEmpty(t.holder) && (t.kind != ToyKind.Bucket && t.kind != ToyKind.Sponge || !s.players.Any(p => p.id == t.holder))))
                    throw new InvalidOperationException("Invalid toy record.");
            if (s.toys.Where(t => !string.IsNullOrEmpty(t.holder)).GroupBy(t => t.holder).Any(g => g.Count() > 1)) throw new InvalidOperationException("One player cannot hold two toys.");
            ids.Clear();
            foreach (var r in s.receipts)
                if (r == null || !Id(r.requestId) || !ids.Add(r.requestId) || string.IsNullOrEmpty(r.fingerprint) || r.fingerprint.Length > 1024 ||
                    string.IsNullOrEmpty(r.outcome) || r.outcome.Length > 128 || r.revision < 1 || r.revision > s.revision) throw new InvalidOperationException("Invalid command receipt.");
        }
        public SoloResult Apply(SoloCommand c)
        {
            SoloResult Reject(string reason) => new SoloResult(false, reason, Revision);
            if (c == null || !Id(c.requestId) || !Id(c.actor) || !Enum.IsDefined(typeof(SoloAction), c.action) ||
                c.item == null || c.target == null || c.value == null || c.item.Length > 128 || c.target.Length > 128 || c.value.Length > 128 ||
                c.item.Contains("|") || c.target.Contains("|") || c.value.Contains("|") || !Position(c.x, c.y)) return Reject("invalid-command");
            var previous = state.receipts.FirstOrDefault(r => r.requestId == c.requestId);
            if (previous != null) return previous.fingerprint == c.Fingerprint() ? new SoloResult(true, previous.outcome, previous.revision, true) : Reject("request-id-reused");
            var player = state.players.FirstOrDefault(p => p.id == c.actor);
            if (player == null) return Reject("unknown-player");
            if (c.expectedRevision != Revision || Revision >= long.MaxValue - 1) return Reject("stale-revision");
            var item = state.toys.FirstOrDefault(t => t.id == c.item);
            var outcome = "accepted";
            switch (c.action)
            {
                case SoloAction.Move: player.x = c.x; player.y = c.y; break;
                case SoloAction.ChangeAvatar:
                    if (!Avatar(c.value)) return Reject("unknown-avatar");
                    player.avatar = c.value; break;
                case SoloAction.StartActivity:
                    if (!Activity(c.value) || c.value == "") return Reject("unknown-activity");
                    player.activity = c.value; break;
                case SoloAction.LeaveActivity: player.activity = ""; break;
                case SoloAction.Grab:
                    if (item == null || item.kind != ToyKind.Bucket && item.kind != ToyKind.Sponge) return Reject("not-movable");
                    if (!string.IsNullOrEmpty(item.holder) || state.toys.Any(t => t.holder == c.actor)) return Reject("already-held");
                    item.holder = c.actor; break;
                case SoloAction.CancelGrab:
                    if (item == null || item.holder != c.actor) return Reject("not-holder");
                    item.holder = ""; break;
                case SoloAction.Drop:
                    if (item == null || item.holder != c.actor) return Reject("not-holder");
                    var target = state.toys.FirstOrDefault(t => t.id == c.target);
                    if (c.target != "" && (target == null || target == item)) return Reject("invalid-target");
                    if (target != null && ((target.x - c.x) * (target.x - c.x) + (target.y - c.y) * (target.y - c.y) > InteractionRadius * InteractionRadius)) return Reject("target-too-far");
                    if (item.kind == ToyKind.Bucket && target?.kind == ToyKind.Tap) { item.water = 3; outcome = "bucket-filled"; }
                    if (item.kind == ToyKind.Bucket && target?.kind == ToyKind.Plant)
                    {
                        var amount = Math.Min(item.water, 3 - target.water);
                        item.water -= amount; target.water += amount;
                        outcome = amount > 0 ? "plant-watered" : "no-water-transferred";
                    }
                    if (item.kind == ToyKind.Sponge && target?.kind == ToyKind.Puddle)
                    { if (target.water > 0) { target.water--; item.wet = true; outcome = "puddle-cleaned"; } }
                    item.x = c.x; item.y = c.y;
                    // Set an interacted-with prop just in front of the station so
                    // its illustration remains readable and the prop is reachable.
                    if (outcome == "bucket-filled" || outcome == "plant-watered" || outcome == "no-water-transferred" || outcome == "puddle-cleaned")
                        item.y = Math.Max(35, c.y - 70);
                    item.holder = ""; break;
                default: return Reject("unknown-action");
            }
            state.revision++;
            var receipt = new SoloReceipt { requestId = c.requestId, fingerprint = c.Fingerprint(), outcome = outcome, revision = Revision };
            state.receipts = state.receipts.Skip(Math.Max(0, state.receipts.Length - 127)).Concat(new[] { receipt }).ToArray();
            return new SoloResult(true, outcome, Revision);
        }
    }
}
