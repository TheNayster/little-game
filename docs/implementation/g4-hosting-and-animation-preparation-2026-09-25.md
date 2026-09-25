# Hosting preparation and the first animation study

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**Status, September 25:** the user authorized independent work while build 98 waits for device signing/acceptance, including a small character prototype. Two tracks are now explicit: **device qualification when available**, and **bounded development that does not require those devices**. This permits preparation/prototyping without declaring G3, G4 or G6 complete. No shipping Unity sources, package locks, game saves or running family server were changed by this task.

**Delivered:** an executable planned-host-handoff model with 20 passing groups and 117,649 six-event schedules; a layered character source and interactive animation workshop; a native hosting integration sequence; and room/save contracts to guide the next content work. **Follow-up G4-PREP-02 now passes real-state restoration: seven C# groups and four native Windows groups, including all four original profiles rejoining a replica-derived authority.** [Restoration evidence and limits](g4-real-state-restoration-2026-09-25.html) · [Try the character workshop](character-workshop/index.html) · [Return checklist](return-checklist-ipad-lan-2026-09-24.html) · [Main plan](../family-playset-build-guide-2026-09-23.html).

## Research applied to this work

| Primary source checked | Documented behavior | Application to this project |
| --- | --- | --- |
| [Unity session host migration](https://docs.unity.com/en-us/mps-sdk/session-host-migration) | Selecting a host does not transfer synchronized game data. The default data migration described there targets Netcode for Entities; custom snapshot storage uses Lobby. | Keep our NGO LAN controller and complete-state restoration explicit. Installing an MPS example would not complete offline LAN hosting. No new networking package was added. |
| [Apple background execution](https://developer.apple.com/documentation/uikit/extending-your-app-s-background-execution-time) | Ordinary apps may be suspended after entering the background; extra execution time is bounded. | A foreground iPad can be a candidate. A notification before backgrounding can help with transfer, but abrupt termination must also work. In-game books/TV/menus must not pause authority simulation. |
| [Raft paper](https://raft.github.io/raft.pdf), sections 2 and 5 | Quorum-based safety relies on a communicating majority. Two replicas cannot lose one and retain a majority. | Do not call a device ranking a consensus guarantee. A partition permits separate play branches; it cannot promise one globally writable toy while devices cannot communicate. Server-authoritative reunion preserves private work separately, without automatic merging. |
| [Unity 2D Animation 13.0.6](https://docs.unity3d.com/Packages/com.unity.2d.animation@13.0/manual/index.html) | The 13.x package line supports Unity 6000.3; layered PSB artwork can be imported through PSD Importer into sprite actors. | Use a layered 2D pipeline for the requested illustrated dollhouse. 13.0.6 is still a candidate, not installed or qualified in this game. The browser rig tests naming and attachment concepts first. |
| [Unity root motion](https://docs.unity3d.com/6000.3/Documentation/Manual/RootMotion.html) | Root motion can apply animation-derived movement to the character object. | Our design keeps movement in the existing world/controller rules; art animates around that result. Walking or turning animation must not issue gameplay movement or grant bucket ownership. |

The right-hand column contains project design decisions. The sources do not provide or certify our implementation. The older [technical research](../family-playset-technical-research-2026-09-23.html) remains useful, with its automatic-merge language superseded by the user's current server-wins requirement.

## G4-PREP-01: an executable planned-transfer model

Goal IDs: **AUTO-02, NET-02, FAMILY-01**. Files: `Tools/HostingModel.Tests/HandoffModel.cs`, `Program.cs`, and `README.md`. The model is outside `Assets`; prepared build 98 is unchanged.

The central invariant is **at most one writer during one planned transfer**. It has this sequence:

1. The current authority freezes new mutations and records the final complete checkpoint for a named successor.
2. The successor checks the family/world/epoch, exact checkpoint and transfer ID, then stages it durably before sending ready.
3. The source records its retirement durably **before** sending the grant. A timeout cannot undo this retirement.
4. The successor persists the grant, restores the world successfully, and only then serves as the new foreground authority.
5. Duplicates return the same result. Lost messages can be retransmitted from durable records. A restarted old host reads its retired state and cannot resume writing that authority.

If a write reports failure, the process stops being eligible to write until it rereads its journal. This handles the case where data reached storage but the caller did not receive success. Before any grant, the source may cancel and resume; after a grant, it cannot safely do so unilaterally.

**Evidence:** [20 model groups](evidence/hosting-prep-2026-09-25/model-results.json), including 117,649 enumerated six-event schedules and 705,894 checked transitions. Cases cover duplicates/reordering, lost messages, restarts, before/after-commit failures, forged identities, damaged or incomplete state, wrong epochs, background eligibility, restore failure, and the absence of a fabricated grant on hard loss.

**Limits:** the model assumes authenticated channels, one serialized controller per device, valid complete checkpoint input and atomic storage. It simulates journal commits; it does not test physical disk failure, encryption, sockets or iPad lifecycle. Its checkpoint payload is opaque fixture data; the separate [G4-PREP-02 test](g4-real-state-restoration-2026-09-25.html) now verifies actual record parsing/restoration, without integrating the model into the live transport. The model covers one cooperative transfer, not repeated transfers, hard-loss election or all possible histories. Passing it does not mean iPad hosting is implemented.

## Retired native-hosting sequence — do not implement

| Bounded task | Implementation and acceptance |
| --- | --- |
| **G4-PREP-02: complete within Windows scope** | Actual `RecoveryRecord`/`FamilySession` restoration passes seven C# and four native groups. Both areas, players, item contents, receipts and idle clocks survive; stale holds clear; old pour replay is idempotent. Invalid/future/private inputs refuse. Source stop and credential reuse are explicit test operations, not automatic mobile hosting. [Evidence](g4-real-state-restoration-2026-09-25.html). |
| **G4-01: isolate authority lifecycle from client presentation** | Separate family/player identity from which device hosts. Today the protected enrollment and discovery path bind a server authority; changing only `StartClient` to `StartHost` would fail that contract. Introduce authenticated mobile-host capabilities and endpoint rebinding without sharing a server private key across devices. Keep the local rendered view alive while NGO starts/stops. Test with two/four Windows processes first. |
| **G4-02: cooperative handoff over the real transport** | Implement the model's durable source retirement, recipient stage/restore and retry protocol. Add bounded message sizes, cancellation/reprepare, multiple consecutive transfers and failure injection around actual storage/network boundaries. Rebuild transient NGO bindings from stable game IDs. |
| **G4-03: sudden loss and simultaneous launch** | A last visible private solo branch already permits immediate local play. A replacement *shared* host needs complete validated recovery state, an explicit branch lineage, authenticated discovery and deterministic selection among communicating peers. Preserve a healthy existing session; no repeated host shuffling because a faster device appears. Distinguish an unavailable old host from an existing PC session that is merely unreachable. |
| **G4-04: automatic reunion and returning hosts** | Existing server authority wins when rejoining it; keep offline work separate. An old mobile host cannot overwrite a continuing session solely because its counter/time is larger. Bind join attempts to lineage and generation; release/cancel held gestures safely. Convergence tests must include 2+2 and 1+1 partitions, then restored communication. |
| **G4-05: both iPads as hosts** | Only after the native fixture works: run each iPad as host, open Menu/book/video while peers play, leave/force-close the host, rejoin the old host, and measure the A10 with four players in independent areas. Apple foreground/background behavior and memory/frame-time limits require these actual-device checks. |

The former independent task was **G4-01**, now retired, beginning with authenticated host identity and separation of authority lifecycle from presentation. Build 98 device checks remain a separate short task when the user returns. Production hosting integration stays gated by its own evidence.

## Personal rooms: save and item boundaries reviewed

Goal IDs: **ROOM-01, ITEM-03, WORLD-01**. This is a design review, not implemented bedrooms. The current `SoloWorld` validators still assume the prototype's two areas and five/ten toys. Adding rooms first requires versioned content definitions and a deliberate save migration; simply appending room names or spawning furniture would leave validation and recovery inconsistent.

| Record | Stable identity and behavior |
| --- | --- |
| Child's bedroom | Owned by the child's profile ID, independent of the currently selected character or hosting device. Both clients display the authoritative room when visiting it. |
| Secret room | A separate room ID linked through its small door, with both children permitted to visit. Plush toys, star sky and aurora are later presentation/content, not a second network session. |
| Placed object | Persistent object ID, definition ID, room/container ID, placement and revision. Room decoration permission is separate from a temporary grab lease. |
| Borrowed shared prop | Supply/home identity and last meaningful-use clock; eligible idle returns cannot remove an actively used item or destroy a protected creation. Station stock remains available. |
| Personal decoration / saved creation | Explicit protection and owner/profile association. It must not accidentally inherit shared-toy return timers. |
| Offline room edits | Retained in the chosen local save; never silently uploaded over the server room. Rejoining loads the server's version. |

Before building the rooms, fixtures must prove old saves migrate without missing props, duplicate operations cannot duplicate furniture, leaving one room does not unload another player's room, and a recovered host restores rooms it was not displaying.

## ART-PREP-01: first character and animation workflow

Goal IDs: **CHAR-01, ITEM-02**. [Open the workshop](character-workshop/index.html). It contains original editable vector study artwork, four motions (idle, walk, wave, carry-and-walk), facing controls, walking pace, pause and visible attachment points. [Layered SVG](character-workshop/blue-pup-rig.svg) · [Asset contract](character-workshop/asset-contract.json).

This blue pup is a rig/attachment study, **not approved final Bluey artwork or an animation already installed in the game**. It is deliberately isolated from saved worlds and networking. The bucket is attached to a named hand anchor and counter-rotated to keep its opening level. Turning mirrors the whole presentation without changing a toy ID. The preview uses local animation time, supports reduced-motion pause, and avoids advancing animation while the page is hidden.

[Preview checks](evidence/hosting-prep-2026-09-25/character-preview.json) cover all four actual browser motion controls, facing, pause, pace, visible attachment guides and a narrow layout without horizontal overflow. Declared SVG layers/pivots resolve to unique elements. Browser error logs were empty at the final check. Pixel-precision SVG matrix inspection was unavailable through the browser bridge; no such assertion or device frame-time claim is included.

Production workflow:

1. Create the actual Bluey character artwork as named layers: back/front arms and legs, torso, tail, head, eyelids, mouth shapes, and required viewing directions. Use the goal sheet's official reference board for likeness/proportions. Keep source art separate from Unity-generated imports.
2. Establish a stable floor origin and hand, mouth, seat and prop anchors. Character selection changes visual assets, not profile identity, navigation size, item ownership or quest role. Parent-sized artwork gets its own attachment offsets while sharing gameplay rules.
3. Export transparent sprite pieces or a tested layered PSB. Qualify Unity 2D Animation 13.0.6 in a dedicated test scene before changing the game package lock. Blender is optional for this flat 2D route; no 3D model is required to animate these illustrated pieces.
4. Prototype a `CharacterView` boundary around the existing procedural `SoloScreen.CreateAvatar`. Feed it displayed movement speed/direction, held-item state and activity pose. Keep bones and local blink timing out of network packets. Avoid root motion and any animation event that performs a pour/pickup.
5. Prove one finished child rig, then a larger parent rig: idle/walk/turn/wave/carry/seat, fast skin switching, hand attachment and room occlusion. Preserve the existing smooth movement underneath. Refine foot planting and directional occlusion before duplicating the full cast.
6. Measure render cost with four characters plus representative NPCs on the older iPad. Only then batch the roster and additional activity poses.

**Next art task:** the isolated Unity character-view/rig import proof, followed by one likeness review. No need to repeat 98 networking acceptance just to look at a character study. Full room/activity production still follows its save/ownership prerequisites.

## Scope and handoff

The shipping project remains build 98; server/helper remain 91. Last installed physical clients remain iPad 7 95, iPad 9/iPhone 79 and Samsung 83. No Mac password, phone connection, iPad interaction or new account was needed for this preparation. The [return checklist](return-checklist-ipad-lan-2026-09-24.html) now separates required device actions from the optional art preview and later host checks.

Work continues in small, testable steps: native replacement-state restoration now passes within its recorded scope; authority/identity separation and actual handoff adapters are next. Art can advance through the isolated import proof alongside the device queue. Neither the model, restoration experiment nor browser preview closes a foundation gate or counts as deployed iPad hosting.
<!-- historical-record-end -->
