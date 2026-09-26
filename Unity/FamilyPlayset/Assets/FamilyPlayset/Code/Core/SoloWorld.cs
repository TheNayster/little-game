using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ToyKind { Bucket, Sponge, Tap, Plant, Puddle }
    public enum SoloAction { Move, ChangeAvatar, Grab, Drop, CancelGrab, StartActivity, LeaveActivity, Travel }
    [Serializable] public sealed class SoloPlayer
    {
        public string id, avatar = "blue-pup", activity = "";
        public string zone = "garden";
        public long visit;
        public float x = 420, y = 200;
        public SoloPlayer Copy() => (SoloPlayer)MemberwiseClone();
    }
    [Serializable] public sealed class SoloToy
    {
        public string id, holder = "";
        public string zone = "garden";
        public ToyKind kind;
        public float x, y;
        public int water;
        public bool wet;
        public bool resetPending;
        public SoloToy Copy() => (SoloToy)MemberwiseClone();
    }
    [Serializable] public sealed class GardenIdleTimer
    {
        public string item;
        public double seconds;
        public GardenIdleTimer Copy() => (GardenIdleTimer)MemberwiseClone();
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
        // Additive, optional maintenance metadata. Old saves begin a fresh grace
        // period; item IDs, contents, placements and receipts are unchanged.
        public GardenIdleTimer[] idleTimers = Array.Empty<GardenIdleTimer>();
    }
    [Serializable] public sealed class SoloCommand
    {
        public string requestId, actor, item = "", target = "", value = "";
        public long expectedRevision;
        public string zone = "garden";
        public long visit;
        public SoloAction action;
        public float x, y;
        internal string Fingerprint() => string.Join("|", new[] { actor, item, target, value,
            ((int)action).ToString(CultureInfo.InvariantCulture), expectedRevision.ToString(CultureInfo.InvariantCulture),
            x.ToString("R", CultureInfo.InvariantCulture), y.ToString("R", CultureInfo.InvariantCulture), zone,
            visit.ToString(CultureInfo.InvariantCulture) });
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
        public const double ToolIdleSeconds=180, ActivityIdleSeconds=60, ResetCueSeconds=5;
        private readonly SoloSnapshot state;
        public long Revision => state.revision;
        public int Schema => state.schema;
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
                foreach (var toy in copy.toys.Where(t=>!string.IsNullOrEmpty(t.holder)))
                {toy.holder = "";toy.resetPending=false;copy.idleTimers=copy.idleTimers.Where(c=>c.item!=toy.id).ToArray();}
                copy.revision++;
            }
            return new SoloWorld(copy);
        }
        // Additive area upgrade for shared and local play. Existing
        // identities, placements, progress and receipts are never regenerated.
        public static SoloWorld WithAreas(SoloWorld world)
        {
            var copy=world.Snapshot();
            if(copy.schema>=2)return world;
            copy.schema=2;
            var creek=Create("template").ReadToys();
            foreach(var toy in creek){toy.zone="creek";toy.id=toy.kind.ToString().ToLowerInvariant()+"-creek";}
            copy.toys=copy.toys.Concat(creek).ToArray();copy.revision++;
            Validate(copy);return new SoloWorld(copy);
        }
        public static string AreaOf(string zone)=>string.IsNullOrEmpty(zone)?"garden":zone;
        public static SoloWorld WithScenery(SoloWorld world)
        {
            world=WithAreas(world);
            if(world.Schema>=WorldLayout.Schema)return world;
            var copy=world.Snapshot();copy.schema=WorldLayout.Schema;copy.revision++;
            Validate(copy);return new SoloWorld(copy);
        }
        public static bool KnownArea(string zone)=>WorldLayout.Area(zone);
        public SoloSnapshot Snapshot() => Clone(state);
        public static SoloSnapshot CopySnapshot(SoloSnapshot snapshot){Validate(snapshot);return Clone(snapshot);}
        // Called by server/local walking authorities. Position is
        // recoverable/coalesced state, not an inventory transaction or receipt.
        internal bool SetWalkingPosition(string actor,string zone,long visit,float x,float y)
        {
            var p=state.players.FirstOrDefault(v=>v.id==actor);
            if(p==null || p.zone!=zone || p.visit!=visit || !WorldLayout.Position(zone,state.schema,x,y))return false;
            if(p.x==x && p.y==y)return false;p.x=x;p.y=y;return true;
        }
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
            return HasUsefulInteraction(item,target);
        }
        public static bool HasUsefulInteraction(SoloToy item,SoloToy target)
        {
            if(item==null || target==null || item==target || AreaOf(item.zone)!=AreaOf(target.zone))return false;
            if(item.kind==ToyKind.Bucket)
                return target.kind==ToyKind.Tap?item.water<3:target.kind==ToyKind.Plant && item.water>0 && target.water<3;
            return item.kind==ToyKind.Sponge && target.kind==ToyKind.Puddle && target.water>0;
        }
        private static SoloSnapshot Clone(SoloSnapshot s)
        {
            var copy=new SoloSnapshot { schema = s.schema, revision = s.revision, worldId = s.worldId,
                players=s.players.Select(p=>p.Copy()).ToArray(),toys=s.toys.Select(t=>t.Copy()).ToArray(),receipts=s.receipts.Select(r=>r.Copy()).ToArray(),
                idleTimers=(s.idleTimers??Array.Empty<GardenIdleTimer>()).Select(t=>t.Copy()).ToArray() };
            foreach(var p in copy.players)p.zone=AreaOf(p.zone);
            foreach(var t in copy.toys)t.zone=AreaOf(t.zone);
            return copy;
        }
        private static bool Id(string s) => !string.IsNullOrWhiteSpace(s) && s.Length <= 128 && !s.Contains("|");
        public static bool Position(float x, float y) => !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && x >= 0 && x <= Width && y >= 0 && y <= Height;
        private static bool Avatar(string s) => s == "blue-pup" || s == "orange-pup";
        private static bool Activity(string s) => s == "" || s == "garden" || s == "cleanup";
        public static void Validate(SoloSnapshot s)
        {
            if (s == null || s.schema < 1 || s.schema > WorldLayout.Schema) throw new InvalidOperationException("Unsupported solo save schema.");
            if (!Id(s.worldId) || s.revision < 0 || s.revision == long.MaxValue || s.players == null || s.players.Length < 1 || s.players.Length > 4 ||
                s.toys == null || s.toys.Length != (s.schema==1?5:10) || s.receipts == null || s.receipts.Length > 128) throw new InvalidOperationException("Invalid solo world record.");
            var ids = new HashSet<string>();
            foreach (var p in s.players)
                if (p == null || !Id(p.id) || !ids.Add(p.id) || !Avatar(p.avatar) || !Activity(p.activity) || !WorldLayout.Position(AreaOf(p.zone),s.schema,p.x,p.y) ||
                    !ValidArea(p.zone,s.schema) || p.visit<0 || p.visit==long.MaxValue || (s.schema==1 && p.visit!=0)) throw new InvalidOperationException("Invalid player record.");
            ids.Clear();
            foreach (var t in s.toys)
                if (t == null || !ids.Add(t.id ?? "") || !Enum.IsDefined(typeof(ToyKind), t.kind) || !ValidArea(t.zone,s.schema) || (AreaOf(t.zone)!="garden" && AreaOf(t.zone)!="creek") ||
                    t.id != t.kind.ToString().ToLowerInvariant() + (AreaOf(t.zone)=="garden"?"-1":"-creek") ||
                    !WorldLayout.Position(AreaOf(t.zone),s.schema,t.x,t.y) || t.water < 0 || t.water > 3 ||
                    ((t.kind == ToyKind.Tap || t.kind == ToyKind.Sponge) && t.water != 0) ||
                    (t.kind != ToyKind.Sponge && t.wet) ||
                    (!string.IsNullOrEmpty(t.holder) && (t.kind != ToyKind.Bucket && t.kind != ToyKind.Sponge || !s.players.Any(p => p.id == t.holder && AreaOf(p.zone)==AreaOf(t.zone)))))
                    throw new InvalidOperationException("Invalid toy record.");
            if (s.toys.Where(t => !string.IsNullOrEmpty(t.holder)).GroupBy(t => t.holder).Any(g => g.Count() > 1)) throw new InvalidOperationException("One player cannot hold two toys.");
            ids.Clear();
            foreach (var r in s.receipts)
                if (r == null || !Id(r.requestId) || !ids.Add(r.requestId) || string.IsNullOrEmpty(r.fingerprint) || r.fingerprint.Length > 1024 ||
                    string.IsNullOrEmpty(r.outcome) || r.outcome.Length > 128 || r.revision < 1 || r.revision > s.revision) throw new InvalidOperationException("Invalid command receipt.");
            ids.Clear();
            if(s.idleTimers!=null && s.idleTimers.Length>s.toys.Length)throw new InvalidOperationException("Too many idle timers.");
            foreach(var timer in s.idleTimers??Array.Empty<GardenIdleTimer>())
                if(timer==null || !ids.Add(timer.item??"") || !s.toys.Any(t=>t.id==timer.item && t.kind!=ToyKind.Tap) ||
                    double.IsNaN(timer.seconds) || double.IsInfinity(timer.seconds) || timer.seconds<0 || timer.seconds>ToolIdleSeconds+ResetCueSeconds)
                    throw new InvalidOperationException("Invalid idle timer.");
        }
        private static bool ValidArea(string zone,int schema)=>schema==1?AreaOf(zone)=="garden":schema==2?zone=="garden" || zone=="creek":KnownArea(zone);
        private void Touch(SoloToy toy)
        {
            if(toy==null)return;
            toy.resetPending=false;state.idleTimers=state.idleTimers.Where(t=>t.item!=toy.id).ToArray();
        }
        private bool IdleEligible(SoloToy toy)
        {
            if(!string.IsNullOrEmpty(toy.holder))return false;
            if(toy.kind==ToyKind.Bucket)return toy.x!=360 || toy.y!=130 || toy.water!=0;
            if(toy.kind==ToyKind.Sponge)return toy.x!=560 || toy.y!=120 || toy.wet;
            if(toy.kind==ToyKind.Plant || toy.kind==ToyKind.Puddle)
            {
                var tool=toy.kind==ToyKind.Plant?ToyKind.Bucket:ToyKind.Sponge;
                if(state.toys.Any(t=>t.zone==toy.zone && t.kind==tool && !string.IsNullOrEmpty(t.holder)))return false;
                return toy.kind==ToyKind.Plant?toy.water==3:toy.water==0;
            }
            return false;
        }
        // Only the authority advances eligible PLAY time. No wall-clock catch-up,
        // client timers, room wipes or new item instances. This deliberately covers
        // the garden fixture only; personal toys/creations need their later policy.
        public bool AdvanceIdle(double seconds,out bool visibleChange)
        {
            if(double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds<0 || seconds>1)throw new ArgumentOutOfRangeException(nameof(seconds));
            visibleChange=false;if(seconds==0)return false;
            if(state.revision>=long.MaxValue-1)throw new InvalidOperationException("World revision limit reached.");
            var changed=false;
            foreach(var toy in state.toys)
            {
                var timer=state.idleTimers.FirstOrDefault(t=>t.item==toy.id);
                if(!IdleEligible(toy))
                {
                    if(timer!=null || toy.resetPending){visibleChange|=toy.resetPending;Touch(toy);changed=true;}
                    continue;
                }
                if(timer==null){timer=new GardenIdleTimer{item=toy.id};state.idleTimers=state.idleTimers.Concat(new[]{timer}).ToArray();}
                timer.seconds+=seconds;changed=true;
                var grace=toy.kind==ToyKind.Bucket || toy.kind==ToyKind.Sponge?ToolIdleSeconds:ActivityIdleSeconds;
                if(timer.seconds>=grace+ResetCueSeconds)
                {
                    // Eligibility was rechecked this tick, including another
                    // player's hold. Preserve the exact shared object identity.
                    if(toy.kind==ToyKind.Bucket){toy.x=360;toy.y=130;toy.water=0;}
                    if(toy.kind==ToyKind.Sponge){toy.x=560;toy.y=120;toy.wet=false;}
                    if(toy.kind==ToyKind.Plant)toy.water=0;
                    if(toy.kind==ToyKind.Puddle)toy.water=3;
                    Touch(toy);visibleChange=true;
                }
                else if(timer.seconds>=grace && !toy.resetPending){toy.resetPending=true;visibleChange=true;}
            }
            // Clock checkpoints do not invalidate an in-flight command every
            // frame. Only a visible authoritative transition advances revision.
            if(visibleChange)state.revision++;
            return changed;
        }
        public SoloResult Apply(SoloCommand c)
        {
            SoloResult Reject(string reason) => new SoloResult(false, reason, Revision);
            if (c == null || !Id(c.requestId) || !Id(c.actor) || !Enum.IsDefined(typeof(SoloAction), c.action) ||
                c.item == null || c.target == null || c.value == null || c.item.Length > 128 || c.target.Length > 128 || c.value.Length > 128 ||
                c.item.Contains("|") || c.target.Contains("|") || c.value.Contains("|") || !ValidArea(c.zone,state.schema) || c.visit<0 || !WorldLayout.Position(c.zone,state.schema,c.x,c.y)) return Reject("invalid-command");
            var previous = state.receipts.FirstOrDefault(r => r.requestId == c.requestId);
            if (previous != null) return previous.fingerprint == c.Fingerprint() ? new SoloResult(true, previous.outcome, previous.revision, true) : Reject("request-id-reused");
            var player = state.players.FirstOrDefault(p => p.id == c.actor);
            if (player == null) return Reject("unknown-player");
            // Check visit before revision rebasing: an old-area action must not
            // become valid when a player leaves and later returns to that area.
            if(c.zone!=player.zone || c.visit!=player.visit)return Reject("stale-area");
            if (c.expectedRevision != Revision || Revision >= long.MaxValue - 1) return Reject("stale-revision");
            var item = state.toys.FirstOrDefault(t => t.id == c.item);
            if(item!=null && item.zone!=player.zone)return Reject("wrong-area");
            var outcome = "accepted";
            switch (c.action)
            {
                case SoloAction.Travel:
                    if(state.schema<2 || !(state.schema>=WorldLayout.Schema?WorldLayout.Destination(c.value):c.value=="garden" || c.value=="creek"))return Reject("unknown-area");
                    if(c.value==WorldLayout.Place(player))return Reject("already-there");
                    if(player.visit>=long.MaxValue-1)return Reject("visit-limit");
                    // These are essential station tools. Settle a live hold at
                    // its rack, preserving water; no new instance is spawned.
                    foreach(var held in state.toys.Where(t=>t.holder==c.actor))
                    {held.holder="";held.x=held.kind==ToyKind.Bucket?360:560;held.y=held.kind==ToyKind.Bucket?130:120;Touch(held);}
                    player.zone=WorldLayout.Canonical(c.value);player.visit++;player.x=WorldLayout.ArrivalX(c.value);player.y=100;player.activity="";outcome="area-entered";break;
                case SoloAction.Move: player.x = c.x; player.y = c.y; break;
                case SoloAction.ChangeAvatar:
                    if (!Avatar(c.value)) return Reject("unknown-avatar");
                    player.avatar = c.value; break;
                case SoloAction.StartActivity:
                    if(player.zone!="garden" && player.zone!="creek")return Reject("unknown-activity");
                    if (!Activity(c.value) || c.value == "") return Reject("unknown-activity");
                    player.activity = c.value; break;
                case SoloAction.LeaveActivity: player.activity = ""; break;
                case SoloAction.Grab:
                    if (item == null || item.kind != ToyKind.Bucket && item.kind != ToyKind.Sponge) return Reject("not-movable");
                    if (!string.IsNullOrEmpty(item.holder) || state.toys.Any(t => t.holder == c.actor)) return Reject("already-held");
                    item.holder = c.actor;Touch(item);
                    foreach(var station in state.toys.Where(t=>t.zone==item.zone && (item.kind==ToyKind.Bucket && t.kind==ToyKind.Plant || item.kind==ToyKind.Sponge && t.kind==ToyKind.Puddle)))Touch(station);
                    break;
                case SoloAction.CancelGrab:
                    if (item == null || item.holder != c.actor) return Reject("not-holder");
                    item.holder = "";Touch(item);break;
                case SoloAction.Drop:
                    if (item == null || item.holder != c.actor) return Reject("not-holder");
                    var target = state.toys.FirstOrDefault(t => t.id == c.target);
                    if (c.target != "" && (target == null || target == item || target.zone!=player.zone)) return Reject("invalid-target");
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
                    item.holder = "";Touch(item);Touch(target);break;
                default: return Reject("unknown-action");
            }
            state.revision++;
            var receipt = new SoloReceipt { requestId = c.requestId, fingerprint = c.Fingerprint(), outcome = outcome, revision = Revision };
            state.receipts = state.receipts.Skip(Math.Max(0, state.receipts.Length - 127)).Concat(new[] { receipt }).ToArray();
            return new SoloResult(true, outcome, Revision);
        }
    }
}
