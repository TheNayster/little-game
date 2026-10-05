using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ToyKind { Bucket, Sponge, Tap, Plant, Puddle, Ball, Plush, Block, Book, TeaCup, TeaPot, Ingredient, KitchenTool, Cookware, Plate }
    public enum SoloAction { Move, ChangeAvatar, Grab, Drop, CancelGrab, StartActivity, LeaveActivity, Travel, UseFixture, LeaveFixture, SetFixture, UseStairs, CancelStairs, EnterDoor, DecorateRoom, SecretRoom, ReturnBedroom, RoomObject, Kitchen, Discovery, HideAndSeek, Park, Zoo, Dinosaur, ChangeOutfit, Roar, Pond, CreekBoat, CreekFishing, Kingdom, Daycare, Beach, Sandpit, Treasure, Vet, DaycarePlay }
    [Serializable] public sealed class SoloPlayer
    {
        public string id, avatar = "blue-pup", activity = "";
        public string outfit="", outfitColor="green";
        public int roar;
        public string zone = "garden";
        public long visit;
        public string fixture = "";
        public double useSeconds, rideStarted;
        public double stairs;
        public float x = 420, y = 200;
        public SoloPlayer Copy() => (SoloPlayer)MemberwiseClone();
    }
    [Serializable] public sealed class SoloToy
    {
        public string id, holder = "", container = "", personalRoom = "";
        public string zone = "garden";
        public ToyKind kind;
        public float x, y;
        public int water;
        public bool wet;
        public bool resetPending;
        public KitchenItem kitchen;
        public SoloToy Copy(){var t=(SoloToy)MemberwiseClone();t.kitchen=kitchen?.Copy();return t;}
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
        public string homeCreations="";
        public HomeState home;
        public ParkState park;
        public ZooState zoo;
        public DinosaurWorldState dinosaurWorld;
        public PondState pond;
        public CreekBoatState creekBoats;
        public PondState creekFishing;
        public SeagullState seagulls;
        public BeachShoreState shore;
        public HideState hideAndSeek;
        public KingdomState kingdom;
        public DaycareState daycare;
        public SandpitState sandpit;
        public TreasureState treasure;
        public VetState vet;
        public DaycarePlayState hideClub,tagClub;
        public KitchenState kitchen;
        public DiscoveryWorkspace[] discovery=Array.Empty<DiscoveryWorkspace>();
        public KeepyState keepy;
        public BedroomState[] bedrooms=Array.Empty<BedroomState>();
        public SecretRoomState[] secrets=Array.Empty<SecretRoomState>();
        public SoloPlayer[] players;
        public SoloToy[] toys;
        public SoloReceipt[] receipts = Array.Empty<SoloReceipt>();
        // Additive, optional maintenance metadata. Old saves begin a fresh grace
        // period; item IDs, contents, placements and receipts are unchanged.
        public GardenIdleTimer[] idleTimers = Array.Empty<GardenIdleTimer>();
        public HomeIdleTimer[] homeIdleTimers = Array.Empty<HomeIdleTimer>();
        // Compact presentation hints; elapsed clocks remain authority-only.
        public int[] homeTidyCues = Array.Empty<int>();
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
    public sealed partial class GameWorld
    {
        public const float Width = 1000, Height = 500, InteractionRadius = 90;
        public const double ToolIdleSeconds=180, ActivityIdleSeconds=60, ResetCueSeconds=5;
        private readonly SoloSnapshot state;
        public long Revision => state.revision;
        public int Schema => state.schema;
        public string WorldId=>state.worldId;
        public SoloPlayer[] ReadPlayers()=>state.players.Select(p=>p.Copy()).ToArray();
        private GameWorld(SoloSnapshot snapshot) { state = snapshot; }
        public static GameWorld Create(params string[] playerIds)
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
        public static GameWorld Restore(SoloSnapshot snapshot)
        {
            Validate(snapshot);
            var copy = Clone(snapshot);
            // Consent is tied to live connections, never resumed from a checkpoint.
            if(copy.sandpit?.reset!=null){copy.sandpit.reset=null;copy.revision++;}
            SuspendRestoredTag(copy);SuspendRestoredHide(copy);SuspendPond(copy);SuspendCreekBoats(copy);SuspendFishing(copy,copy.creekFishing);SuspendZoo(copy);SuspendDinosaurCare(copy);
            copy.homeTidyCues=Array.Empty<int>();
            // A pointer lease never survives closing the app or a recovered save.
            if (copy.toys.Any(t => !string.IsNullOrEmpty(t.holder)))
            {
                foreach (var toy in copy.toys.Where(t=>!string.IsNullOrEmpty(t.holder)))
                {toy.holder = "";toy.resetPending=false;copy.idleTimers=copy.idleTimers.Where(c=>c.item!=toy.id).ToArray();}
                copy.revision++;
            }
            if(copy.players.Any(p=>!string.IsNullOrEmpty(p.fixture)))
            {foreach(var p in copy.players){p.fixture="";p.useSeconds=0;p.rideStarted=0;}copy.revision++;}
            if(copy.players.Any(p=>p.stairs>0))
            {foreach(var p in copy.players)p.stairs=0;copy.revision++;}
            return new GameWorld(copy);
        }
        // Additive area upgrade for shared and local play. Existing
        // identities, placements, progress and receipts are never regenerated.
        public static GameWorld WithAreas(GameWorld world)
        {
            var copy=world.Snapshot();
            if(copy.schema>=2)return world;
            copy.schema=2;
            var creek=Create("template").ReadToys();
            foreach(var toy in creek){toy.zone="creek";toy.id=toy.kind.ToString().ToLowerInvariant()+"-creek";}
            copy.toys=copy.toys.Concat(creek).ToArray();copy.revision++;
            Validate(copy);return new GameWorld(copy);
        }
        public static string AreaOf(string zone)=>string.IsNullOrEmpty(zone)?"garden":zone;
        public static GameWorld WithScenery(GameWorld world)
        {
            world=WithAreas(world);
            if(world.Schema>=WorldLayout.ScenerySchema)return world;
            var copy=world.Snapshot();copy.schema=WorldLayout.ScenerySchema;copy.revision++;
            Validate(copy);return new GameWorld(copy);
        }
        public static bool KnownArea(string zone)=>WorldLayout.Area(zone);
        public SoloSnapshot Snapshot() => Clone(state);
        public static SoloSnapshot CopySnapshot(SoloSnapshot snapshot){Validate(snapshot);return Clone(snapshot);}
        // Called by server/local walking authorities. Position is
        // recoverable/coalesced state, not an inventory transaction or receipt.
        internal bool SetWalkingPosition(string actor,string zone,long visit,float x,float y)
        {
            var p=state.players.FirstOrDefault(v=>v.id==actor);
            if(p==null || p.stairs>0 || p.zone!=zone || p.visit!=visit || !WorldLayout.Position(zone,state.schema,x,y))return false;
            // Stopping on a mattress must retain its occupancy. Apply the floor
            // corridor only when walking changes the requested point.
            if(p.x==x && p.y==y)return false;
            if(state.schema>=BedroomFurniture.Schema && SecretRooms.Furnished(zone)){var floor=BedroomFurniture.Floor(x,y);x=floor.X;y=floor.Y;}
            var dinoFloor=DinosaurRides.Floor(zone,x,y);x=dinoFloor.X;y=dinoFloor.Y;
            var zooFloor=ZooLayout.Floor(zone,x,y);x=zooFloor.X;y=zooFloor.Y;
            var ridingFloor=ParkWheels.Floor(p,x,y);x=ridingFloor.X;y=ridingFloor.Y;
            if(p.x==x && p.y==y)return false;
            if(LeaveWaveRide(p))state.revision++;
            if(!string.IsNullOrEmpty(p.fixture) && !ParkWheels.Usable(p.fixture) && !DinosaurRides.Usable(p.fixture)){ClearFixture(p);state.revision++;}
            var h=HideAndSeek.Player(state.hideAndSeek,actor);
            if(h!=null && h.mode!=HiderMode.Away){h.idle=0;if(h.mode==HiderMode.Hidden){ExitHide(p,h,false);state.revision++;}}
            if(ExitClubCover(actor))state.revision++;
            BeachFootsteps(p,x,y);var previousX=p.x;p.x=x;p.y=y;SyncDinosaurRider(p,previousX);return true;
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
            var copy=new SoloSnapshot { schema = s.schema, revision = s.revision, worldId = s.worldId, homeCreations=s.homeCreations,home=s.home?.Copy(),seagulls=s.seagulls?.Copy(),shore=s.shore?.Copy(),kingdom=s.kingdom?.Copy(),daycare=s.daycare?.Copy(),sandpit=s.sandpit?.Copy(),treasure=s.treasure?.Copy(),vet=s.vet?.Copy(),hideClub=s.hideClub?.Copy(),tagClub=s.tagClub?.Copy(),park=s.park?.Copy(),zoo=s.zoo?.Copy(),dinosaurWorld=s.dinosaurWorld?.Copy(),pond=s.pond?.Copy(),creekBoats=s.creekBoats?.Copy(),creekFishing=s.creekFishing?.Copy(),hideAndSeek=s.hideAndSeek?.Copy(),kitchen=s.kitchen?.Copy(),discovery=(s.discovery??Array.Empty<DiscoveryWorkspace>()).Select(w=>w.Copy()).ToArray(),keepy=s.keepy?.Copy(),
                players=s.players.Select(p=>p.Copy()).ToArray(),toys=s.toys.Select(t=>t.Copy()).ToArray(),receipts=s.receipts.Select(r=>r.Copy()).ToArray(),
                bedrooms=(s.bedrooms??Array.Empty<BedroomState>()).Select(r=>r.Copy()).ToArray(),
                secrets=(s.secrets??Array.Empty<SecretRoomState>()).Select(r=>r.Copy()).ToArray(),
                idleTimers=(s.idleTimers??Array.Empty<GardenIdleTimer>()).Select(t=>t.Copy()).ToArray(),
                homeIdleTimers=(s.homeIdleTimers??Array.Empty<HomeIdleTimer>()).Select(t=>t.Copy()).ToArray(),homeTidyCues=(int[])(s.homeTidyCues??Array.Empty<int>()).Clone() };
            foreach(var p in copy.players)p.zone=AreaOf(p.zone);
            foreach(var t in copy.toys)t.zone=AreaOf(t.zone);
            return copy;
        }
        private static bool Id(string s) => !string.IsNullOrWhiteSpace(s) && s.Length <= 128 && !s.Contains("|");
        public static bool Position(float x, float y) => !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && x >= 0 && x <= Width && y >= 0 && y <= Height;
        private static bool Avatar(string s) => PlayableCharacters.Contains(s);
        private static bool Activity(string s) => s == "" || s == "garden" || s == "cleanup";
        public static void Validate(SoloSnapshot s)
        {
            NormalizeKitchenInline(s);
            NormalizeHideInline(s);NormalizeZooInline(s);NormalizeOutfits(s);NormalizePond(s);NormalizeCreekBoats(s);NormalizeCreekFishing(s);
            NormalizeDinosaurInline(s);
            NormalizeKingdom(s);NormalizeDaycare(s);NormalizeSandpit(s);NormalizeTreasure(s);NormalizeVet(s);NormalizeDaycarePlay(s);
            NormalizeHideInline(s);NormalizeOutfits(s);
            if (s == null || s.schema < 1 || s.schema > WorldLayout.Schema) throw new InvalidOperationException("Unsupported solo save schema.");
            if (!Id(s.worldId) || s.revision < 0 || s.revision == long.MaxValue || s.players == null || s.players.Length < 1 || s.players.Length > 4 ||
                s.toys == null || s.toys.Length != (s.schema==1?5:s.schema<4?10:s.schema<BedroomFurniture.Schema?11:27+(s.schema>=SecretRooms.Schema?(s.secrets??Array.Empty<SecretRoomState>()).Count(r=>r!=null && r.created)*6:0)+HomeBooks.ExtraStock(s.schema)+RoomPlay.ExtraStock(s)+(s.schema>=Kitchen.Schema?Kitchen.StockCount:0)+(s.schema>=CakeFlow.Schema?1:0)+(s.schema>=ParkPlay.Schema?2:0)) || s.receipts == null || s.receipts.Length > 128) throw new InvalidOperationException("Invalid solo world record.");
            var ids = new HashSet<string>();
            foreach (var p in s.players)
                if (p == null || !Id(p.id) || !ids.Add(p.id) || !Avatar(p.avatar) || !ValidOutfit(p,s.schema) || !Activity(p.activity) || !WorldLayout.Position(AreaOf(p.zone),s.schema,p.x,p.y) ||
                    !ValidArea(p.zone,s.schema) || !HomeRooms.ValidTransit(p) || s.schema<HomeRooms.Schema && p.stairs!=0 || p.visit<0 || p.visit==long.MaxValue || (s.schema==1 && p.visit!=0)) throw new InvalidOperationException("Invalid player record.");
            ids.Clear();
            foreach (var t in s.toys)
                if (t == null || !ids.Add(t.id ?? "") || !Enum.IsDefined(typeof(ToyKind), t.kind) || !ValidArea(t.zone,s.schema) || !ValidToyLocation(t,s.schema) ||
                    !WorldLayout.Position(AreaOf(t.zone),s.schema,t.x,t.y) || t.water < 0 || t.water > 3 ||
                    ((t.kind == ToyKind.Tap || t.kind == ToyKind.Sponge || t.kind == ToyKind.Ball) && t.water != 0) ||
                    (t.kind != ToyKind.Sponge && t.wet) ||
                    (!string.IsNullOrEmpty(t.holder) && (!Carryable(t.kind) || !s.players.Any(p => p.id == t.holder && AreaOf(p.zone)==AreaOf(t.zone)))))
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
                    double.IsNaN(timer.seconds) || double.IsInfinity(timer.seconds) || timer.seconds<0 || timer.seconds>(s.schema>=HomeTidying.Schema?HomeTidying.IdleSeconds:ToolIdleSeconds)+ResetCueSeconds)
                    throw new InvalidOperationException("Invalid idle timer.");
            ValidateHideAndSeek(s);ValidateCreations(s);ValidateHomeTidying(s);ValidateBedrooms(s);ValidateSecrets(s);ValidateBooks(s);ValidateFurnishings(s);ValidateRoomPlay(s);ValidateKitchen(s);ValidateDiscovery(s);ValidateHome(s);ValidateKeepy(s);ValidatePark(s);ValidateZoo(s);ValidatePond(s);ValidateCreekBoats(s);ValidateFishing(s,s.creekFishing,CreekFishing.Habitat,CreekFishing.Schema);ValidateDinosaurWorld(s);ValidateKingdom(s);ValidateDaycare(s);ValidateNpcCasts(s);ValidateSeagulls(s);ValidateShore(s);ValidateWaveRide(s);ValidateSandpit(s);ValidateTreasure(s);ValidateVet(s);ValidateDaycarePlay(s);
        }
        private static bool ValidArea(string zone,int schema)=>schema==1?AreaOf(zone)=="garden":schema==2?zone=="garden" || zone=="creek":KnownArea(zone);
        private void Touch(SoloToy toy)
        {
            if(toy==null)return;
            TouchTidy(HomeTidying.Item(toy.id));
            toy.resetPending=false;state.idleTimers=state.idleTimers.Where(t=>t.item!=toy.id).ToArray();
        }
        private bool IdleEligible(SoloToy toy)
        {
            if(!string.IsNullOrEmpty(toy.holder))return false;
            if(toy.id=="bucket-park")return toy.x!=ParkPlay.FountainX+140 || toy.y!=130 || toy.water!=0;
            if(toy.kind==ToyKind.Bucket)return HomeRooms.Internal(toy.zone) || toy.x!=360 || toy.y!=130 || toy.water!=0;
            if(toy.kind==ToyKind.Sponge)return HomeRooms.Internal(toy.zone) || toy.x!=560 || toy.y!=120 || toy.wet;
            if(toy.kind==ToyKind.Plant || toy.kind==ToyKind.Puddle)
            {
                var tool=toy.kind==ToyKind.Plant?ToyKind.Bucket:ToyKind.Sponge;
                if(state.toys.Any(t=>t.zone==toy.zone && t.kind==tool && !string.IsNullOrEmpty(t.holder)))return false;
                return toy.kind==ToyKind.Plant?toy.water==3:toy.water==0;
            }
            return false;
        }
        // Only the authority advances eligible PLAY time. No wall-clock catch-up,
        // client timers, room wipes or new item instances. Home cleanup is separate
        // from the legacy garden rules and never clears personal creations.
        public bool AmbientChanged {get;private set;}
        public bool AdvanceIdle(double seconds,out bool visibleChange,string[] activePlayers=null)
        {
            if(double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds<0 || seconds>1)throw new ArgumentOutOfRangeException(nameof(seconds));
            AmbientChanged=false;visibleChange=false;if(seconds==0)return false;
            if(state.revision>=long.MaxValue-1)throw new InvalidOperationException("World revision limit reached.");
            AdvanceRoarCooldowns(seconds);
            var changed=AdvanceHome(seconds);
            changed|=AdvanceDinosaurWorld(seconds,activePlayers,out var dinoVisible);visibleChange|=dinoVisible;
            changed|=AdvancePond(seconds,activePlayers,out var pondVisible);visibleChange|=pondVisible;
            changed|=AdvanceFishing(state.creekFishing,CreekFishing.Habitat,seconds,activePlayers,out var creekFishVisible);visibleChange|=creekFishVisible;
            changed|=AdvanceCreekBoats(seconds,activePlayers,out var boatVisible);visibleChange|=boatVisible;
            changed|=AdvanceZoo(seconds,activePlayers,out var zooVisible);visibleChange|=zooVisible;
            changed|=AdvanceTag(seconds,activePlayers,out var tagVisible);visibleChange|=tagVisible;
            changed|=AdvanceTreasure(seconds,activePlayers,out var treasureVisible);visibleChange|=treasureVisible;
            changed|=AdvanceVet(seconds,activePlayers,out var vetVisible);visibleChange|=vetVisible;
            changed|=AdvanceDaycarePlay(seconds,activePlayers,out var clubVisible);visibleChange|=clubVisible;
            changed|=AdvanceKingdom(seconds,activePlayers,out var kingdomVisible);visibleChange|=kingdomVisible;
            var daycareAdvanced=AdvanceDaycare(seconds,activePlayers,out var daycareVisible);changed|=daycareAdvanced;visibleChange|=daycareVisible;
            if(daycareAdvanced){changed|=AdvanceSandpit(seconds,activePlayers,out var sandVisible);visibleChange|=sandVisible;}
            changed|=AdvanceSeagulls(seconds,activePlayers,out var gullVisible);visibleChange|=gullVisible;
            changed|=AdvanceShore(seconds,activePlayers,out var shoreVisible);visibleChange|=shoreVisible;
            changed|=AdvancePark(seconds,out var parkVisible);visibleChange|=parkVisible;
            changed|=AdvanceHideAndSeek(seconds,activePlayers,out var hideVisible);visibleChange|=hideVisible;
            changed|=AdvanceKitchen(seconds,out var kitchenVisible);visibleChange|=kitchenVisible;
            changed|=AdvanceMixing(seconds,out var mixingVisible);visibleChange|=mixingVisible;
            changed|=AdvanceIceRescue(seconds,out var iceVisible);visibleChange|=iceVisible;
            changed|=AdvanceBubbleLab(seconds,out var bubblesVisible);visibleChange|=bubblesVisible;
            changed|=AdvanceMarbleRamps(seconds,out var rampsVisible);visibleChange|=rampsVisible;
            changed|=AdvanceKeepy(seconds,activePlayers);
            changed|=AdvanceHomeTidying(seconds,out var tidyVisible);visibleChange|=tidyVisible;
            changed|=AdvanceStairs(seconds,activePlayers,out var roomCommitted);visibleChange|=roomCommitted;
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
                var grace=state.schema>=HomeTidying.Schema?HomeTidying.IdleSeconds:toy.kind==ToyKind.Bucket || toy.kind==ToyKind.Sponge?ToolIdleSeconds:ActivityIdleSeconds;
                if(timer.seconds>=grace+ResetCueSeconds)
                {
                    // Eligibility was rechecked this tick, including another
                    // player's hold. Preserve the exact shared object identity.
                    if(toy.kind==ToyKind.Bucket){toy.container="";if(HomeRooms.Internal(toy.zone))toy.zone="garden";toy.x=toy.id=="bucket-park"?ParkPlay.FountainX+140:360;toy.y=130;toy.water=0;}
                    if(toy.kind==ToyKind.Sponge){toy.container="";if(HomeRooms.Internal(toy.zone))toy.zone="garden";toy.x=560;toy.y=120;toy.wet=false;}
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
        private void TravelPlayer(SoloPlayer player,string destination)
        {
            // Declining an activity protects this visit. Returning from another
            // world is a fresh arrival and may join the family's current game.
            var daycareArrival=destination=="daycare" && WorldLayout.Place(player)!="daycare";
            LeaveWaveRide(player);ExitDaycarePlay(player);
            // Settle station tools at their racks; personal items and food stay
            // at departure. This is shared by normal travel and accepted invites.
            foreach(var held in state.toys.Where(t=>t.holder==player.id))
            {held.holder="";if(BedroomFurniture.Personal(held.kind) || Kitchen.Kind(held.kind)){held.x=player.x;held.y=Math.Max(35,Math.Min(250,player.y-65));Touch(held);continue;}if(HomeRooms.Internal(held.zone))held.zone="garden";held.x=held.kind==ToyKind.Ball?3350:held.kind==ToyKind.Bucket?360:560;held.y=held.kind==ToyKind.Sponge?120:130;Touch(held);}
            if(state.daycare!=null){var picnic=state.daycare.members.Single(m=>m.actor==player.id);if(picnic.attending)picnic.declined=true;picnic.attending=false;picnic.carryingPlate=false;}if(state.kingdom!=null){var member=state.kingdom.members.Single(m=>m.actor==player.id);if(member.attending)member.declined=true;member.attending=false;member.carrying="";}
            if(state.sandpit!=null){var sand=state.sandpit.members.Single(m=>m.actor==player.id);if(sand.attending)sand.declined=true;sand.attending=false;if(daycareArrival)sand.declined=false;}
            if(state.vet!=null){var helper=state.vet.members.Single(m=>m.actor==player.id);if(helper.attending)helper.declined=true;helper.attending=false;helper.gesture="";if(daycareArrival)helper.declined=false;}
            if(state.treasure!=null){var hunt=state.treasure.members.Single(m=>m.actor==player.id);if(hunt.attending)hunt.declined=true;hunt.attending=false;if(daycareArrival)hunt.declined=false;}
            if(daycareArrival){if(state.daycare!=null)state.daycare.members.Single(m=>m.actor==player.id).declined=false;if(state.kingdom!=null)state.kingdom.members.Single(m=>m.actor==player.id).declined=false;}
            CancelCreekFishing(player.id);CancelCreekBoats(player.id);CancelPond(player.id);CancelZoo(player.id);CancelDinosaurCare(player.id);ClearFixture(player);player.zone=WorldLayout.Canonical(destination);player.visit++;player.x=WorldLayout.ArrivalX(destination);player.y=100;player.activity="";
        }
        public SoloResult Apply(SoloCommand c,string[] connectedPlayers=null)
        {
            SoloResult Reject(string reason) => new SoloResult(false, reason, Revision);
            // Sandcastle attachments use bounded local art coordinates; ground
            // props use its pit (which extends past the walking floor's y=500).
            // Every unrelated command retains ordinary world-area validation.
            if (c == null || !Id(c.requestId) || !Id(c.actor) || !Enum.IsDefined(typeof(SoloAction), c.action) ||
                c.item == null || c.target == null || c.value == null || c.item.Length > 128 || c.target.Length > 128 || c.value.Length > 128 ||
                c.item.Contains("|") || c.target.Contains("|") || c.value.Contains("|") || !ValidArea(c.zone,state.schema) || c.visit<0 ||
                !(c.action==SoloAction.Sandpit && (c.value=="decorate" || c.value=="decor-replace") ? KeepyRules.Finite(c.x) && KeepyRules.Finite(c.y) && Math.Abs(c.x)<=200 && c.y>=-120 && c.y<=300 : c.action==SoloAction.Sandpit && c.value=="ground-decorate" ? DaycareSandpit.Inside(c.x,c.y,28,28) : WorldLayout.Position(c.zone,state.schema,c.x,c.y))) return Reject("invalid-command");
            var previous = state.receipts.FirstOrDefault(r => r.requestId == c.requestId);
            if (previous != null) return previous.fingerprint == c.Fingerprint() ? new SoloResult(true, previous.outcome, previous.revision, true) : Reject("request-id-reused");
            var player = state.players.FirstOrDefault(p => p.id == c.actor);
            if (player == null) return Reject("unknown-player");
            // Check visit before revision rebasing: an old-area action must not
            // become valid when a player leaves and later returns to that area.
            if(c.zone!=player.zone || c.visit!=player.visit)return Reject("stale-area");
            if (c.expectedRevision != Revision || Revision >= long.MaxValue - 1) return Reject("stale-revision");
            if(player.stairs>0 && c.action!=SoloAction.CancelStairs && c.action!=SoloAction.CancelGrab && c.action!=SoloAction.ChangeAvatar)return Reject("on-stairs");
            var item = state.toys.FirstOrDefault(t => t.id == c.item);
            if(item!=null && item.zone!=player.zone)return Reject("wrong-area");
            var outcome = "accepted";
            switch (c.action)
            {
                case SoloAction.UseStairs:
                    var stairError=BeginStairs(player);if(stairError!=null)return Reject(stairError);outcome="stairs-started";break;
                case SoloAction.CancelStairs:
                    player.stairs=0;break;
                case SoloAction.EnterDoor:
                    var doorError=EnterDoor(player,c.target,c.value);if(doorError!=null)return Reject(doorError);outcome="room-entered";break;
                case SoloAction.SecretRoom:
                    var secretError=ChangeSecret(c,player);if(secretError!=null)return Reject(secretError);outcome="secret-updated";break;
                case SoloAction.ReturnBedroom:
                    var exitError=SecretTravel(player,SecretRooms.Parent(player.zone),"",true);if(exitError!=null)return Reject(exitError);outcome="room-entered";break;
                case SoloAction.DecorateRoom:
                    var decorError=DecorateBedroom(c,player);if(decorError!=null)return Reject(decorError);outcome="room-decorated";break;
                case SoloAction.Dinosaur:
                    var dinoError=DinosaurOperation(c,player);if(dinoError!=null)return Reject(dinoError);outcome="dinosaur-played";break;
                case SoloAction.CreekBoat:
                    var boatError=CreekBoatOperation(c,player);if(boatError!=null)return Reject(boatError);outcome="boat-played";break;
                case SoloAction.CreekFishing:
                    var creekFishError=FishingOperation(c,player,state.creekFishing,CreekFishing.Habitat);if(creekFishError!=null)return Reject(creekFishError);outcome="creek-fished";break;
                case SoloAction.Pond:
                    var pondError=PondOperation(c,player);if(pondError!=null)return Reject(pondError);outcome="pond-played";break;
                case SoloAction.Zoo:
                    var zooError=ZooOperation(c,player);if(zooError!=null)return Reject(zooError);outcome="zoo-played";break;
                case SoloAction.DaycarePlay:
                    var clubError=DaycarePlayOperation(c,player);if(clubError!=null)return Reject(clubError);outcome="daycare-club-played";break;
                case SoloAction.Vet:
                    var vetError=VetOperation(c,player);if(vetError!=null)return Reject(vetError);outcome="animal-cared-for";break;
                case SoloAction.Treasure:
                    var huntError=TreasureOperation(c,player);if(huntError!=null)return Reject(huntError);outcome="treasure-played";break;
                case SoloAction.Sandpit:
                    var sandpitError=SandpitOperation(c,player,connectedPlayers);if(sandpitError!=null)return Reject(sandpitError);outcome="sandpit-played";break;
                case SoloAction.Daycare:
                    var daycareError=DaycareOperation(c,player);if(daycareError!=null)return Reject(daycareError);outcome="daycare-played";break;
                case SoloAction.Kingdom:
                    var kingdomError=KingdomOperation(c,player);if(kingdomError!=null)return Reject(kingdomError);outcome="kingdom-played";break;
                case SoloAction.Park:
                    var parkError=ParkOperation(c,player);if(parkError!=null)return Reject(parkError);outcome="park-played";break;
                case SoloAction.Beach:
                    var gullError=SeagullOperation(c,player);if(gullError!=null)return Reject(gullError);outcome="seagulls-noticed";break;
                case SoloAction.HideAndSeek:
                    var hideError=HideOperation(c,player);if(hideError!=null)return Reject(hideError);outcome="hiding-changed";break;
                case SoloAction.Discovery:
                    var discoveryError=DiscoveryOperation(c,player);if(discoveryError!=null)return Reject(discoveryError);outcome="discovery-changed";break;
                case SoloAction.Kitchen:
                    var kitchenError=KitchenOperation(c,player,item);if(kitchenError!=null)return Reject(kitchenError);outcome="kitchen-changed";break;
                case SoloAction.RoomObject:
                    var playError=ApplyRoomObject(c,player,item);if(playError!=null)return Reject(playError);outcome="room-played";break;
                case SoloAction.Travel:
                    if(state.schema<2 || !(state.schema>=WorldLayout.ScenerySchema?WorldLayout.Destination(c.value):c.value=="garden" || c.value=="creek"))return Reject("unknown-area");
                    if(c.value==WorldLayout.Place(player))return Reject("already-there");
                    if(player.visit>=long.MaxValue-1)return Reject("visit-limit");
                    TravelPlayer(player,c.value);outcome="area-entered";break;
                case SoloAction.Move:
                    LeaveWaveRide(player);
                    if(!ParkWheels.Usable(player.fixture) && !DinosaurRides.Usable(player.fixture))ClearFixture(player);var floorPoint=state.schema>=BedroomFurniture.Schema && SecretRooms.Furnished(player.zone)?BedroomFurniture.Floor(c.x,c.y):new WalkPoint(c.x,c.y);
                    floorPoint=ParkWheels.Floor(player,floorPoint.X,floorPoint.Y);floorPoint=ZooLayout.Floor(player.zone,floorPoint.X,floorPoint.Y);floorPoint=DinosaurRides.Floor(player.zone,floorPoint.X,floorPoint.Y);BeachFootsteps(player,floorPoint.X,floorPoint.Y);var oldX=player.x;player.x=floorPoint.X;player.y=floorPoint.Y;SyncDinosaurRider(player,oldX);break;
                case SoloAction.ChangeAvatar:
                    if (!Avatar(c.value)) return Reject("unknown-avatar");
                    player.avatar = c.value;
                    if(!CharacterOutfits.Available(player.avatar))player.outfit="";
                    break;
                case SoloAction.ChangeOutfit:
                    var outfitError=ApplyOutfit(player,c);if(outfitError!=null)return Reject(outfitError);outcome="outfit-changed";break;
                case SoloAction.Roar:
                    var roarError=ApplyRoar(player);if(roarError!=null)return Reject(roarError);outcome="roared";break;
                case SoloAction.StartActivity:
                    if(player.zone!="garden" && player.zone!="creek")return Reject("unknown-activity");
                    if ((!Activity(c.value) && c.value!=KeepyRules.Activity) || c.value == "") return Reject("unknown-activity");
                    if(c.value==KeepyRules.Activity){var keepyError=StartKeepy(player);if(keepyError!=null)return Reject(keepyError);}
                    else {ClearFixture(player);player.activity = c.value;} break;
                case SoloAction.LeaveActivity: player.activity = ""; break;
                case SoloAction.Grab:
                    if (item == null || !Carryable(item.kind)) return Reject("not-movable");
                    if ((item.holder!="" && !(state.schema>=RoomPlay.Schema && item.holder==player.id && BedroomFurniture.Seat(player.fixture))) || state.toys.Any(t => t.holder == c.actor && t!=item)) return Reject("already-held");
                    if(!StorageOpen(item))return Reject("storage-closed");
                    SettleStack(item);ClearFixture(player);item.container="";item.holder = c.actor;Touch(item);
                    foreach(var station in state.toys.Where(t=>t.zone==item.zone && (item.kind==ToyKind.Bucket && t.kind==ToyKind.Plant || item.kind==ToyKind.Sponge && t.kind==ToyKind.Puddle)))Touch(station);
                    break;
                case SoloAction.CancelGrab:
                    if (item == null || item.holder != c.actor) return Reject("not-holder");
                    item.holder = "";Touch(item);break;
                case SoloAction.Drop:
                    if (item == null || item.holder != c.actor) return Reject("not-holder");
                    if(Kitchen.Slot(c.target,out var kitchenGroup,out var kitchenSlot)){var stored=StoreKitchen(c,player,item,kitchenGroup,kitchenSlot);if(stored!=null)return Reject(stored);outcome="kitchen-stored";break;}
                    var playSlot=RoomPlay.Slot(player.zone,c.target);
                    if(playSlot>=0){var stored=StoreRoomPlay(c,player,item,playSlot);if(stored!=null)return Reject(stored);outcome="item-tucked-or-served";break;}
                    if(c.target.StartsWith("stack/",StringComparison.Ordinal)){var stacked=StackToy(c,player,item);if(stacked!=null)return Reject(stacked);outcome="toy-stacked";break;}
                    var bookSlot=HomeBooks.Slot(c.target);
                    if(bookSlot>=0){var stored=StoreBook(c,player,item,bookSlot);if(stored!=null)return Reject(stored);outcome="item-stored";break;}
                    var bedroomSlot=BedroomFurniture.Slot(player.zone,c.target);
                    if(bedroomSlot>=0){var stored=StoreInBedroom(c,player,item,bedroomSlot);if(stored!=null)return Reject(stored);outcome="item-stored";break;}
                    var slot=HomeLayout.StorageSlot(c.target);
                    if(slot>=0)
                    {
                        if(state.home==null || player.zone!="garden" || !state.home.shedOpen)return Reject("storage-closed");
                        if(state.toys.Any(t=>t.container==c.target))return Reject("storage-full");
                        if(Math.Abs(c.x-HomeLayout.StorageX(slot))>120 || Math.Abs(c.y-HomeLayout.StorageY(slot))>120)return Reject("target-too-far");
                        item.container=c.target;item.x=HomeLayout.StorageX(slot);item.y=HomeLayout.StorageY(slot);
                        item.holder="";Touch(item);outcome="item-stored";break;
                    }
                    var target = state.toys.FirstOrDefault(t => t.id == c.target);
                    if (c.target != "" && (target == null || target == item || target.zone!=player.zone)) return Reject("invalid-target");
                    if (target != null && ((target.x - c.x) * (target.x - c.x) + (target.y - c.y) * (target.y - c.y) > InteractionRadius * InteractionRadius)) return Reject("target-too-far");
                    if(item.kind==ToyKind.Ingredient && target?.kind==ToyKind.Cookware){var add=KitchenOperation(new SoloCommand{value="add",target=target.id,x=c.x,y=c.y},player,item);if(add!=null)return Reject(add);item.holder="";outcome="ingredient-added";break;}
                    if(item.kind==ToyKind.Cookware && target?.kind==ToyKind.Plate){var served=KitchenOperation(new SoloCommand{value="serve",target=target.id},player,item);if(served!=null)return Reject(served);item.holder="";outcome="food-served";break;}
                    if(item.kind==ToyKind.KitchenTool && target?.kind==ToyKind.Cookware){var step=Kitchen.Next(target.kitchen.dish);if(Kitchen.Tool(step)!=item.kitchen.definition)return Reject("use-matching-tool");var prepared=KitchenOperation(new SoloCommand{value=step},player,target);if(prepared!=null)return Reject(prepared);ReturnKitchenTool(item,player);outcome="food-prepared";break;}
                    if(item.kind==ToyKind.TeaPot && target?.kind==ToyKind.TeaCup){var poured=PourTea(item,target,player);if(poured!=null)return Reject(poured);outcome="pretend-tea-poured";break;}
                    if (item.kind == ToyKind.Bucket && target?.kind == ToyKind.Tap) { item.water = 3;if(target.id=="tap-park")state.park.waterUntil=state.park.clock+4; outcome = "bucket-filled"; }
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
                case SoloAction.UseFixture:
                case SoloAction.LeaveFixture:
                case SoloAction.SetFixture:
                    var error=ApplyHome(c,player);if(error!=null)return Reject(error);outcome="home-changed";break;
                default: return Reject("unknown-action");
            }
            AfterCreekFishingAction(c,player);AfterPondAction(c,player);AfterHideAction(c,player);
            AfterTagAction(c,player);AfterDaycarePlayAction(c,player);
            TouchHomeAction(c,player);
            state.revision++;
            var receipt = new SoloReceipt { requestId = c.requestId, fingerprint = c.Fingerprint(), outcome = outcome, revision = Revision };
            state.receipts = state.receipts.Skip(Math.Max(0, state.receipts.Length - 127)).Concat(new[] { receipt }).ToArray();
            return new SoloResult(true, outcome, Revision);
        }
    }
}
