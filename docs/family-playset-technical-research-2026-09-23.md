# The Family Playset — deeper technical research

**Reviewed September 23, 2026.** This follows the [full feasibility audit](family-playset-feasibility-audit-2026-09-23.html) and addresses the parts that needed stronger evidence. It incorporates your latest preference: the iPads automatically connect to the Windows PC server when the game opens. The other iPad, Android phone, or iPhone joins that same world when opened later.

**Current scope — September 25:** PC/VPS is the sole shared authority; four mobile clients join automatically. Offline solo stays private, and server state wins on reconnect without importing offline edits. G4/AUTO-02 device hosting is retired. [Current decisions](current-decisions.md) control scope; the [build guide](family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) records current implementation. Technical sources and candidate comparisons below retain their original research date; this update is not a new external-source verification.


**Package-status clarification from the subsequent review:** “released v2.13.3 source” below refers to a published GitHub release. The 6000.3 editor catalog labels that version differently. The [package review](family-playset-package-research-2026-09-23.html#4-networking-stability-a-real-documentation-mismatch) compares it with the editor-release baseline, NGO 2.13.2, and explains why the exact NGO/Transport set still needs qualification. None of this report's source inspection establishes a tested production lockfile.

This review combines official platform/package documentation, released source code, selected development-source excerpts, and primary distributed-systems research. The findings below distinguish **documented behavior**, **source-confirmed behavior**, **our proposed design**, and **device proof still needed**. No Unity project, server, networking prototype, or on-device benchmark was created or run. Downloaded source is research material, not imported game code.

## 1. Automatic PC connection is the normal path

A dedicated server is a separate process that owns the world and accepts clients. A listen server combines that authority with one player's app. The distinction matters: closing an iPad client does not remove the dedicated PC server. This architecture directly fits your online-game comparison. [FishNet's explanation of dedicated and listen servers](https://fish-networking.gitbook.io/docs/guides/high-level-overview/networking-models)

**Proposed everyday flow:**

1. The PC server starts, restores the family world, opens its game port, and advertises availability on the home network.
2. Child one opens the game. It restores their local profile, discovers the paired PC, authenticates, and joins without a Host/Join menu.
3. Child two opens later and independently joins the same server. They can start in their own room, visit the first child, or go elsewhere.
4. Either child closes their app. The server releases their temporary grabs and activity roles; everybody else's world continues.
5. A returning child receives the current state, including moved objects and finished activities. They do not bring an old room snapshot that overwrites the ongoing world.

The PC may connect by Ethernet and the mobile devices by Wi-Fi, provided the router permits communication between them. This local path does not require internet, a cloud account, Relay, or router port forwarding. That is an architectural consequence of using a reachable local server, not a claim that the family's router has already passed testing. Guest-network isolation and Windows firewall rules must be checked during setup.

“Automatic” means **after one parent setup**: permit local networking, pair the family devices, establish stable child/adult profiles, and configure the PC server's startup and local firewall access. Discovery names alone are not authentication. Use a paired device credential and protocol/content checks, rather than trusting any service named “Family Playset.” No child should repeatedly enter addresses or approve invitations.

| Event | Required behavior | Host migration needed? |
| --- | --- | --- |
| Another player opens the game | Add their connection and personal view | No |
| One player leaves the beach for the creek | Change only that player's zone subscription and view | No |
| One iPad closes while the PC serves | Release its transient interactions; others continue | No |
| The PC/VPS crashes or becomes unreachable | Continue private solo; restore designated server through its own recovery workflow | No |
| A returning client has offline edits | Load server world, retain private save separately | No |

No client competes to host. Planned PC→VPS migration retires the old writer before the new one starts; a failed connection never promotes a device.
A designated dedicated authority keeps client lifecycle independent of the shared world. Planned migration is a parent operation, never automatic device promotion.

## 2. The selected network path

**Decision: Bluetooth is removed from the feature plan and backlog at your request.** There is no Bluetooth implementation, discovery feature, transport experiment or test milestone to build.

Use Wi-Fi for mobile devices and either Ethernet or Wi-Fi for the PC. Unity Transport's built-in driver uses UDP sockets or WebSockets, fitting the ordinary local-network path. [Unity Transport 2.6 documentation](https://docs.unity3d.com/Packages/com.unity.transport@2.6/manual/index.html)

| Situation | Selected behavior | Priority |
| --- | --- | --- |
| PC reachable on the home network | Automatically join the paired PC's world | Normal required path |
| PC/VPS unavailable | Independent private solo on each device | Required |
| No usable shared connection, or child chooses solo | Full local play with saved progress | Required |
| Phone hotspot or remote connection to home PC | Qualify separately after the core game works | Optional travel feature |

## 3. Discovery across Windows, iPad and Android

Use a common DNS-SD service contract with native adapters. Windows provides `DnsServiceRegister`; its service registration is asynchronous and tied to the process lifetime. Android's `NsdManager` provides DNS-SD registration, discovery and resolution across platforms. These are concrete platform primitives for automatic server discovery, not complete game/session managers. [Microsoft service registration](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceregister), [Android NSD guide](https://developer.android.com/develop/connectivity/wifi/use-nsd)

On iPadOS 18, use supported Bonjour/Network framework APIs and declare the service and local-network purpose in the app. Apple's privacy guidance distinguishes system Bonjour from an app directly sending raw multicast/broadcast packets; the latter has additional entitlement requirements. A downloaded discovery script must be inspected before assuming it works under the intended signing setup. [Apple local-network privacy](https://developer.apple.com/documentation/technotes/tn3179-understanding-local-network-privacy)

**Source finding:** Mirror's inspected `NetworkDiscoveryBase` creates `UdpClient` instances with `EnableBroadcast = true` and sends discovery toward `IPAddress.Broadcast`. That is why we should not copy its discovery sample unchanged as our iPad setup. This finding does not invalidate Mirror's gameplay networking. [Pinned Mirror discovery source](https://github.com/MirrorNetworking/Mirror/blob/c4f3739966e151f405be1762d33502794fd034ff/Assets/Mirror/Components/Discovery/NetworkDiscoveryBase.cs#L167)

**Proposed adapter contract:** advertise and resolve a service; report endpoint changes/loss; stop on suspension; restart on foreground; surface denied permission distinctly from “no server found.” Advertise only compact, non-secret metadata such as protocol version, world identifier and readiness. Authenticate after connection. Re-resolve changing addresses instead of making children enter IP addresses. Bonjour on ordinary LAN must not be confused with Apple's separate peer-to-peer Wi-Fi transport.

Use one client connection controller, authenticate the designated server and ignore stale attempt callbacks. Keep bounded foreground retry work independent of movement and local saves. No mobile host negotiation is needed.

## 4. What the networking packages actually solve

| Candidate | Verified useful part | Missing or limiting part | Assessment |
| --- | --- | --- | --- |
| NGO + Unity Transport | GameObject networking, server/client model and cross-platform IP transport | Exact package status/version qualification, custom LAN discovery, durable world state and client lifecycle qualification remain work | Retain as the first prototype candidate |
| Mirror | Actual additive-level sample with per-player scene transitions and scene interest management | No built-in host migration in its documented FAQ; broadcast discovery needs replacement for our chosen route | Useful reference or alternative, not an automatic recovery shortcut |
| FishNet | Dedicated/listen servers and connection-specific scene management | Its documentation explicitly says host migration is not built in | Viable alternative to measure if NGO hits a demonstrated blocker |
| Photon Fusion 2 | Documented reconnect and host-migration samples | The demonstrated migration path uses Photon coordination; application state restoration still needs code | Strong reference, but not evidence of a drop-in offline LAN solution |

[NGO package documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/index.html), [Mirror FAQ](https://mirror-networking.gitbook.io/docs/manual/faq), [FishNet networking models](https://fish-networking.gitbook.io/docs/guides/high-level-overview/networking-models), [Photon reconnect/migration sample](https://doc.photonengine.com/fusion/v2/technical-samples/fusion-disconnect-reconnect)

Unity's MPS documentation separates **host election** from **state migration**. Its default migration implementation is for Netcode for Entities; for NGO it points to Distributed Authority. Crucially, the custom migration snapshot APIs described there store/retrieve snapshots through **Lobby**. Adding `IMigrationDataHandler` is therefore not itself an offline LAN migration implementation. Our inference: keep local recovery independent of cloud sessions instead of treating an MPS example as the whole solution. [Unity session migration](https://docs.unity.com/en-us/mps-sdk/session-host-migration)

Photon's reconnect example likewise distinguishes a reconnect token from migration. Its migration procedure requires snapshots, shutting down the old runner, creating another runner, and reconstructing network objects; the sample unloads and reloads its scene. Built-in assistance does not establish zero visible interruption or automatic preservation of all arbitrary game data. [Photon's actual migration procedure](https://doc.photonengine.com/fusion/v2/technical-samples/fusion-disconnect-reconnect)

**Version finding:** the inspected NGO development head declares package **3.0.1**, Unity **6000.7.0b1**, and different transport/Entities dependencies. It is not the package to paste into our proposed Unity 6.3 project. I separately checked the released **v2.13.3** source: its manifest declares Unity **6000.0** and Transport **2.6.0**. Its release notes include scene synchronization and Rigidbody2D fixes. This makes 2.13.3 a concrete package candidate to qualify with 6.3, not a certified lockfile. [Development manifest](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/297c8b0d38ab666741752a1db43a1a4a61294a08/com.unity.netcode.gameobjects/package.json), [Released manifest](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/ddd715da4695a278a143d9af3530fd60a3814b73/com.unity.netcode.gameobjects/package.json), [v2.13.3 release](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases/tag/v2.13.3)

Keep Unity 6.3 LTS as the editor branch candidate. Recheck its available patches and renderer issues when creating the project. No editor/package was installed or upgraded during this research.

## 5. Independent areas have an actual source example

Mirror's additive-level portal sends scene messages through **one player's connection**, moves that player's server object into the destination scene, then adds that player back. Its scene interest manager compares actual scene identities. This is concrete evidence for the architectural pattern: one server, different player locations, shared visibility when players meet. [Portal source](https://github.com/MirrorNetworking/Mirror/blob/c4f3739966e151f405be1762d33502794fd034ff/Assets/Mirror/Examples/AdditiveLevels/Scripts/Portal.cs#L92), [Scene interest source](https://github.com/MirrorNetworking/Mirror/blob/c4f3739966e151f405be1762d33502794fd034ff/Assets/Mirror/Components/InterestManagement/Scene/SceneInterestManagement.cs)

It is a reference, not our finished scene loader: the example loads its subscenes on the server, uses Physics3D, and explicitly avoids unloading the server's subscene when the local host player travels. Copying all visual scenes onto an A10 client could defeat the memory plan. NGO requires its own implementation; Mirror API calls cannot simply be pasted into NGO. [Example manager source](https://github.com/MirrorNetworking/Mirror/blob/c4f3739966e151f405be1762d33502794fd034ff/Assets/Mirror/Examples/AdditiveLevels/Scripts/AdditiveLevelsNetworkManager.cs#L42)

FishNet's scene documentation supplies another useful constraint: loading by name with stacking enabled creates separate instances; loading the existing handle joins that instance. Connection-scene unload policy also affects whether an empty scene stays loaded. This reinforces the need to distinguish **a world identity** from a loaded scene or display name. [FishNet stacking](https://fish-networking.gitbook.io/docs/guides/features/scene-management/scene-stacking), [Scene load data](https://fish-networking.gitbook.io/docs/guides/features/scene-management/scene-data/sceneloaddata)

**Our proposed implementation:** one ordinary `zoneId` for the creek, one for the playground, and stable separate IDs for each bedroom/secret room. The authority keeps logical state for all of them. Clients load detailed art for their own view. A zone transition has a destination-ready acknowledgement and a current snapshot/revision before the arriving child can manipulate its toys. The sibling's zone and activity do not reload. Separate imagination activities can have explicit instance IDs; a recovery conflict must never create an invisible second ordinary playground.
The PC/VPS keeps every logical area persistent; clients load detailed art only for their own view. A per-client visual subscription is not a server backup, and a complete recovery replica is not required before offline input works.
Two replication channels serve different purposes: nearby visual updates keep the screen responsive; compact recovery state supports server disaster recovery. Object visibility is not a substitute for the second channel. [NGO visibility rules](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/object-visibility.html)

<a id="6-host-recovery-needs-state-outside-network-objects"></a>
## 6. Server recovery needs state outside network objects

**Released-source finding:** NGO v2.13.3's shutdown path calls `DespawnAndDestroyNetworkObjects`, disposes networking services, and clears the spawn manager. The spawn code destroys ordinary dynamically spawned objects unless a prefab handler controls their destruction. `DontDestroyWithOwner` concerns a departing object's owner; it does not preserve the entire world through a network-manager shutdown. [Shutdown source](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/ddd715da4695a278a143d9af3530fd60a3814b73/com.unity.netcode.gameobjects/Runtime/Core/NetworkManager.cs#L1659), [Spawn cleanup](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/ddd715da4695a278a143d9af3530fd60a3814b73/com.unity.netcode.gameobjects/Runtime/Spawning/NetworkSpawnManager.cs#L1488), [Owner-disconnect flag](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/ddd715da4695a278a143d9af3530fd60a3814b73/com.unity.netcode.gameobjects/Runtime/Core/NetworkObject.cs#L1387)

**Design consequence:** persistent entity IDs, room layouts, contents, activity checkpoints and accepted operation IDs must live in a separate serializable model. NetworkObjects present that model during a connection; they cannot be its only durable copy. A network ID is not a permanent toy identity.

| Recovery case | Proposed procedure | Limit to prove |
| --- | --- | --- |
| Planned PC→VPS migration | Verify backup/restore and enrollment; retire old writer before enabling new one | Never allow two canonical writers |
| Server crash | Client continues privately; dedicated-server workflow restarts/restores canonical world | Cannot promise persistence for data never saved |
| Client loses its route | Private local continuation; bounded retry | No live shared actions across a missing route |
| Client reconnects | Authenticate and load current server world; retain private saves | Never import/replay offline edits |

No mobile device is a shared authority. Apple background limits remain relevant to client save/network lifecycle, not a requirement to implement host handoff. [Apple lifecycle](https://developer.apple.com/documentation/uikit/extending-your-app-s-background-execution-time).

The earlier Raft/host-election inference is retired with device hosting. Independent solo is not a consensus participant and has no promise of merging into shared state. The server stays canonical; a timeout cannot prove it stopped. [Raft reference](https://raft.github.io/raft.pdf).

No exact recovery delay or guaranteed loss window is established. Snapshot intervals, acknowledgement policy, load times and packet-loss detection need measurements. Ordinary departure from the PC-hosted game should not trigger this recovery path at all.

## 7. Shared toys, automatic returns and save conflicts

The inspected Unity Playground pickup script reads a keyboard key, searches pickup-tagged objects, and deliberately calls another player's `Drop()` before taking the object. That directly conflicts with your exclusive bucket rule. It also lacks network arbitration and durable toy state. Use it to understand a local interaction, not as a qualified multiplayer item template. [Pinned pickup source](https://github.com/Unity-Technologies/UnityPlayground/blob/3d8acd7432ee115f28e05f2b1af39fa783376b4a/Assets/UnityTechnologies/Playground/Scripts/Gameplay/PickUpAndHold.cs)

**Proposed item transaction:** a client requests an action with stable actor/entity IDs, a unique operation ID, and the expected item revision. The authority validates location, current holder and capabilities; commits the change once; broadcasts the resulting state; persists the operation. Two simultaneous pickup requests are ordered by the authority, so only one receives the hold. Retry returns the existing result instead of duplicating a pour or recipe.

For pouring, update source quantity and destination quantity as one operation. Use an authored liquid quantity/composition model and a cosmetic stream; the outcome must not depend on every device simulating identical particles. For movement, clients may smooth the picture, but smoothing must not grant ownership. A hand pose can change instantly with the avatar without creating another bucket.

**Automatic unused-item return is your confirmed preference.** The specific idle interval, cue duration and room loan budget remain tuning proposals. Define content categories before applying timers:

| Item type | Return policy | Protection |
| --- | --- | --- |
| Shared borrowed toy/tool | Eligible after a period of genuine inactivity | Never reclaim while held, being used, or part of a protected active operation |
| Fixed station equipment | Stays at its station/allowed zone | Cannot empty the kitchen by carrying its fixtures home |
| Personal decoration or unlocked personal copy | Persists in its owner's saved room | Not counted as a borrowed communal original |
| Saved cake, drawing, sandcastle or build | Persist as a creation record with stable identity | Returning a plate/tray cannot erase its creation |
| Temporary consumable/effect | Explicit replenishment/cleanup rule | Never mistaken for a durable personal possession |

An idle return is an authority transaction, not a client countdown teleport. Recheck idleness and revision when it executes. If a child picks the toy up during the warning, cancel the pending return. Check nested container contents for held/protected items. Move protected creations to a valid saved container/workspace before returning equipment; if there is nowhere valid, defer the return instead of deleting work. Late join sees the committed result; a packet retry must not create a replacement plus an original.

Offline save preservation is separate from shared-world convergence. The earlier local-first/CRDT references are background research, not a requirement to implement automatic game-state merging. [Original local-first reference](https://www.inkandswitch.com/essay/local-first/).

**Current persistence rules:**

- Connected bedroom edits are server transactions; offline bedroom edits stay private.
- Private cakes/drawings remain local, without automatic creation import.
- Rejoin uses the one canonical communal bucket; private copies never upload.
- Connected retries recognize operation IDs; abandoned/offline requests never replay on rejoin.
- Preserve versioned private saves separately through updates and server rejoin.

SQLite offers atomic transactions that can protect a compound local save from partial writes, subject to its documented storage assumptions. It does not itself synchronize devices. It is a storage candidate to compare with a small versioned snapshot/journal implementation; a Unity iOS IL2CPP/Android binding still needs qualification. Keep each device's save local rather than sharing one live database file over the network. [SQLite atomic commit](https://www.sqlite.org/atomiccommit.html)

## 8. Older-iPad performance and the headless-server constraint

Apple documents that an app's available memory limit can change during its lifecycle and need not equal physical device RAM. That rules out presenting a guessed fixed memory number as an established crash threshold for this game. The earlier 400 MiB goal and prop/NPC limits are starting budgets, not measurements. [Apple available-memory API](https://developer.apple.com/documentation/os/os_proc_available_memory)

Unity's Dedicated Server target disables audio and removes graphics-related work/assets. **Design consequence:** cooking timers, hide-and-seek logic, object capabilities and NPC tasks must run without playing audio, rendering a sprite, or receiving local touch input on the server. Store gameplay durations/anchors/collision shapes in logical content data; client speech and animation follow committed events. [Unity 6.3 server optimizations](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-optimizations.html)

Each client renders only its current view; PC/VPS runs necessary logical activities in all occupied areas. Local books/video/settings cannot pause siblings. Solo runs installed activities independently.

**Required measurements, not completed tests:**

| Workload | Why it differs |
| --- | --- |
| A10 client with PC server | Tests drawing, input, local assets and network presentation |
| A10 offline solo | Tests local rules, animation and saving without networking |
| Four players and many props in one room | Stresses visible characters, sorting, animation and item contention |
| A10 client or solo reading/watching media | Tests loading/decoder/audio costs without pausing siblings |
| Repeated room changes and character swaps | Reveals retained assets and accumulating references |
| Late join or recovery during those workloads | Exposes temporary snapshot/loading memory spikes |

Measure frame-time spikes, main-thread stalls, native/managed memory, thermal behavior and save/recovery correctness on release builds. Keep the 30 FPS target, but do not label it achieved from minimum engine requirements. Package samples and editor play mode do not certify sustained A10 client/solo performance.

## 9. What changed in confidence and what to build first

| Claim | Evidence level after this pass | Remaining proof |
| --- | --- | --- |
| PC-hosted clients can come and go independently | Documented server architecture; clear design fit | Real mixed-device automatic join/leave test |
| Clients can occupy different areas and meet later | Source-confirmed reference pattern | Our NGO loader, state persistence and A10 client/solo cost |
| Automatic LAN discovery has native cross-platform building blocks | Official Windows/Apple/Android APIs | Native bridges, permissions, firewall and actual router |
| A host-election call restores the world | Contradicted by migration docs and NGO cleanup source | Separate model, checkpoints and restoration |
| An existing pickup template already enforces our toy ownership | Contradicted by inspected Playground script | Authoritative item transactions |
| Conflicting offline worlds merge automatically without policy | Not established by CRDT/local-first research | Separate private saves; no automatic imports |
| Four-player A10 client performance | Sustained updated-device workload remains open | Physical frame-time and memory tests |

Continue the existing dedicated-server foundation. Four-device admission and scoped shared play have passed; current 98 continuity and rollout need physical qualification. Isolated character art can advance meanwhile, then G5 rooms/items before integrated content expansion. See the build guide for the actual next task.

A plain prototype is enough to expose these risks; producing six finished worlds before these tests would not make the networking claims better supported. No defensible calendar estimate follows from these sources. Measure the first implemented slice and asset workflow before estimating the complete production schedule.

## 10. Reproducible evidence and boundaries

Selected source was saved under [deep-source-review-2026-09-23](bluey-research/deep-source-review-2026-09-23/review-notes.md). The manifests record original paths, revisions, source URLs, byte lengths and SHA-256 hashes. Review was targeted to the cited methods/manifests/licenses, not an exhaustive audit of entire repositories.

| Repository/material | Revision inspected | Scope |
| --- | --- | --- |
| NGO released v2.13.3 | `ddd715da4695a278a143d9af3530fd60a3814b73` | Package dependencies; shutdown/despawn; owner-disconnect distinction |
| NGO development head | `297c8b0d38ab666741752a1db43a1a4a61294a08` | Same areas; detected incompatible newer editor/dependency baseline |
| Mirror development head | `c4f3739966e151f405be1762d33502794fd034ff` | Additive-level portal/manager, scene visibility, broadcast discovery, project version and license |
| FishNet development head | `7c4a6448d48ed65b154ebd5615c86fb9001da1cf` | Manifest/license, paired with official scene and migration documentation |
| Unity Playground | `3d8acd7432ee115f28e05f2b1af39fa783376b4a` | Pickup behavior, project version and license |

Official documentation and primary papers are linked beside the claims they support. Some Apple/Unity pages returned JavaScript shells or fetch errors through one access method; successful indexed documentation, vendor clarifications and pinned code were used where available. A failed fetch is not evidence that a feature is absent. Research does not replace device testing, and proposals such as timers and content budgets remain explicitly unqualified.

Only this game's research directory was changed. The unrelated old project and its Unity/Blender connector remain outside this work.
