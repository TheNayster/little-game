# Upstairs and four bedrooms — implementation research

September 26, 2026. **Research baseline completed before implementation.** The user subsequently authorized building from it; see the separate [BED-1 implementation and evidence](upstairs-foundation-2026-09-26.html). Findings and proposed later-room systems below are not all implemented.

The user selected this order: make the existing stairs work, add a second floor and finish four personal bedrooms; then build the four optional secret rooms. This supersedes the earlier kitchen-first queue. Kitchen and the complete Home backlog remain required. This report recommends how to implement that request in the existing game; it does not claim a canonical television-house floor plan or completed gameplay.

[Home feature tracker](../home-world-feature-tracker.html) · [Current decisions](../current-decisions.md) · [Bedroom requirements, chapter 32](../bluey-game-research-2026-09-23.html#32-four-personal-bedrooms-with-shared-updates) · [Secret rooms, chapter 33](../bluey-game-research-2026-09-23.html#33-secret-plush-rooms-with-stars-and-northern-lights) · [Readable report](upstairs-bedrooms-research-2026-09-26.html)

## 1. Recommended result and boundaries

Build a usable stair route from the current living room to an upstairs landing and hall. Put four separate bedroom doors along that hall. Each door leads to one persistent profile-owned bedroom, with enough floor and seating space for four visitors. Keep the lower house and backyard connected as they are now. Upstairs remains inside the existing Heeler Home destination.

The proposed layout is:

```text
Living room — kitchen/dining — veranda — backyard — far shed
     |
working stairs, up and down
     |
upstairs landing — hall — four bedroom doors
                           |    |    |    |
                           A    B    C    D
                           ·    ·    ·    ·
                 later: optional secret room per bedroom
```

The diagram describes connections, not final dimensions. Preserve the accepted character scale; make the hall long enough to fit four recognizable doors without shrinking the people or crowding touch targets. A bedroom can reuse a common architectural art set while retaining its own furniture, colors, ownership and saved contents. Final themes and names can be chosen later without changing room identity.

**First bounded implementation:** one working two-way staircase and usable upstairs landing, backed by the room-transition and save contract. Prove four independent travelers and carried-item continuity there before multiplying doorways and room furnishings. This is the first part of the bedroom task, not permission to call four empty rooms complete.

Secret entrances, plush rooms and aurora effects follow the bedroom stage. Do not add inactive star-door buttons during this stage. Kitchen, bathroom/laundry, books, TV, dinosaurs, science and outdoor additions remain in the tracker.

## 2. Research basis and observed starting point

The requirements were read from current decisions, the build guide/work record, research chapters 31–35 and 47–51, the Home tracker, scene-layer research, and retained Home/navigation reports. Focused read-only inspection identified the integration boundaries below; this was not a comprehensive code audit, gameplay test or live device/server check. Baseline: main commit `f0ce483bcf0985287b47eb3ddaf6a314613778e8` before these documentation changes.

| Observed evidence | Consequence for this task |
| --- | --- |
| The living-room panorama visibly contains a flattened painted stairway. [Current image](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Scenery/home-living.png). | New interaction must align with those steps and separate foreground rails/cover where needed. An invisible doorway alone would not make the stairs visibly usable. |
| [WorldLayout](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/WorldLayout.cs) records schema 5/content 6, five persistent area names, and Home as the negative-X part of `garden`. | Add internal Home places without moving the existing downstairs coordinates, renaming saved garden objects or adding world-menu destinations. |
| [SharedMovement](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/SharedMovement.cs) uses bounded floor-plane movement; [SoloScenery](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloScenery.cs) supplies a horizontal camera and depth projection. | Floor-depth Y is not literal elevation. Putting an upstairs character at a large Y coordinate would not establish another floor or a legal stair route. |
| [SoloWorld](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/SoloWorld.cs) major-world travel releases held toys; its snapshot, clone and validators enumerate limited existing state. | Internal-room transfer needs explicit carry-preserving behavior, new persistent room data, and matching validation/serialization. Reusing world travel unchanged fails the requirement. |
| [FamilySession](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/FamilySession.cs) maps connections to stable profiles; [FamilyPairing](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/FamilyPairing.cs) supports four enrolled identities. | Reuse family identity; never derive ownership from avatar, network connection number or arrival order. |
| [NetworkProbe](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkProbe.cs) disables NGO scene management and uses custom messaging; [IGardenSession](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/IGardenSession.cs) separates the client from solo/network sessions. | Extend the existing room-state and session approach. A new global scene-loading architecture is unnecessary for four bedrooms. |
| [HomeArtPart](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/HomeArtPart.cs) is a uGUI RawImage; scenery uses Resources loading with at most three resident/pending panorama requests. | Use the current Canvas layering and bounded presentation loader. SpriteRenderer sorting instructions are not a direct replacement for this renderer. |
| The work record describes earlier isolated bedroom rules; the current integrated Home has no completed four-bedroom screen/network/save feature. | Reuse ideas only after verifying compatibility. Historical rule tests cannot qualify the new rooms. |

Installed versions inspected for research: Unity 6000.3.24f1, uGUI 2.0.0 and NGO 2.13.2. Public documentation below matches the corresponding engine/package series. No package upgrade is proposed.

## 3. Room architecture and alternatives

**Recommended:** a small logical Home room graph held by the world authority, with independently loaded client presentation. Define stable places for the existing downstairs property, upstairs hall and four bedrooms. Define doors/stairs as links with explicit source/destination anchors, legal walking regions and allowed traversal directions. Persist room and object identities separately from art filenames.

Keep existing `garden` identity and downstairs coordinates intact. Add an internal place identifier, or an equivalent versioned location record, rather than silently replacing all existing zone semantics. Actor and item locations must resolve to the same place. Every check that currently compares only a zone must also distinguish upstairs, downstairs and bedroom interiors. Room changes need a new visit/generation so delayed movement or drag commands from the previous room cannot affect the new one.

The server retains lightweight data for all rooms even when no client draws them. Each client shows its own room and nearby visual transition pieces; remote avatars are visible/audible only when spatially relevant. Four separate bedroom views must not require four active presentation trees on every phone. Owner departure does not unload another visitor's room or reset its contents.

| Approach | Assessment |
| --- | --- |
| Logical places with the existing custom session/snapshot model | Best initial fit. Reuses independent player movement, local camera and server authority. Requires explicit room contracts and migration. |
| Client-local additive Unity scenes | Possible later for more complex authored content. Still needs the same ownership, state transfer, readiness, audio and lifetime rules; additive loading alone does not solve them. |
| NGO synchronized scene loads for each floor change | Poor default for this task. NGO's scene manager normally coordinates scene loading with connected clients; selective arrangements require deliberate validation/design. Keep the current disabled scene-management setting for this slice. [NGO 2.13 scene behavior](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/using-networkscenemanager.html). |
| Full NavMesh/3D conversion | Disproportionate to the current Canvas-based dollhouse. NavMesh links join compatible navigation meshes; they are not a drop-in stair path for the custom movement system. [Unity navigation links](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshLink.html). |

These are project design judgments, not vendor prescriptions. NGO custom messages can continue carrying authoritative commands and state through the existing adapter; adding bedrooms does not itself require network-spawning every piece of furniture. [CustomMessagingManager API](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/api/Unity.Netcode.CustomMessagingManager.html).

## 4. What working stairs should mean

Provide both existing movement styles: steer onto a generous stair entry region, or deliberately tap the stair/landing target and walk to it. A tap on an upstairs destination must route via the stairs, not draw a straight line through walls. Start only when the character reaches the proper entry anchor. Keep the joystick clear and use large touch regions; Apple's game guidance recommends aiming for 44 × 44 points on iPhone/iPad. Points must be translated through the actual Canvas/device scale, not mistaken for raw texture pixels. [Apple game input guidance](https://developer.apple.com/videos/play/wwdc2024/10085/).

Author a visible foot-contact path over the illustrated treads. Map progress along it into screen position/elevation and cover order; retain normal movement tuning and accepted character art. Characters walk up and down with plausible foot placement and pass behind the correct rail. Avoid stretching the walking sprite or speeding its cadence to disguise a mismatched staircase.

Use a short local camera move or restrained transition at the landing boundary. The player should see the character use the stairway and then arrive on a coherent second floor. A button that instantly swaps the background is insufficient by itself. Reduced motion can minimize camera travel while preserving clear departure and arrival cues.

Transit state belongs to each actor. Four players can ascend, descend or pass in opposite directions without a whole-stair lock. Use forgiving pass-through or short independent offsets rather than blocking bodies that can trap someone. Provide four safe arrival positions clear of the return trigger; do not immediately bounce arrivals back downstairs. Preserve the fact that one person can stay downstairs while another walks upstairs.

Furniture placement must never block the stair entrance, hall corridor or bedroom exits. In rooms, introduce authored legal walk areas and simple obstacle-aware routing sufficient for the actual furniture layout. Do not promise arbitrary navigation based on the present rectangular floor clamp. For this bounded slice, fixed safe furniture slots and short authored routes are simpler to qualify than unconstrained furniture movement.

## 5. Independent travel as one reliable operation

Use the existing request identity, actor/profile authentication and stale-visit protection as the starting pattern. Recommended transaction:

1. **Prepare locally:** request the target presentation before leaving. Keep play at the source usable while loading; show feedback only on the traveling device.
2. **Validate at authority:** actor still exists at the source place/visit, is at the correct entry, can use that route, and has a compatible destination. A request must not teleport from anywhere in the house.
3. **Begin per-player transit:** settle incompatible fixture actions for that actor only. Preserve an allowed carried item and its identity. Display traversal from authoritative progress/endpoints, with client smoothing as presentation.
4. **Commit once:** when traversal reaches the prepared boundary, atomically update actor place/position/visit and carried-item location. Choose a safe destination anchor. No temporary item clone is created at either end.
5. **Acknowledge and present:** show the committed destination and resume ordinary input. A repeated request returns the same result. Old movement/drag packets cannot act in the destination.

Loading failure or cancellation before commitment leaves the player at a valid source endpoint. After commitment, an acknowledgement timeout must query/resume the authoritative result; it must not blindly move the player back while the server has already moved them forward. Returning then uses the normal reverse route. Define the cancellation boundary explicitly.

Backgrounding, disconnecting or restarting during transit must resolve to a valid saved endpoint, or a deliberately supported resumable route state. Never persist only an interpolated screen coordinate that reopens in midair. Other players continue their own activities. If connection loss starts private solo, use the latest usable visible state with its new room data; later shared rejoin still loads server authority without uploading offline edits.

Internal Home travel keeps carried personal toys and compatible borrowed props. Scope borrowed-item return rules to the relevant activity/compound rather than interpreting every bedroom doorway as major-world departure. Unsupported oversized/fixed furniture should stay installed with clear feedback. Carrying a container must preserve its contents once supported; if nested containers are not yet implemented, do not silently imply that they work.

## 6. Four persistent profiles and functional bedrooms

Allocate one bedroom identity per stable family profile. Map the four existing enrolled profiles explicitly and persist that mapping. A/B/C/D in the tracker are labels, not device assignments or a join-order algorithm. If a supported older record already contains room ownership, preserve it and add only missing rooms. Provision absent family members' rooms without requiring them all to connect at once. A visitor never allocates a replacement room.

For private solo, preserve a separate world/save namespace and the known profile identities. Reconnecting must not promote private room edits into the shared family house. Two devices using the same profile should follow existing session-admission rules, not receive duplicate bedrooms. Changing all four avatars to Bluey should leave ownership and doors unchanged.

Every bedroom needs:

- A usable bed, rug, low shelf, toy basket/chest, reading cushion and clear four-person play/seating area.
- Interactive bedding/lamp/picture/decor choices and real shelf/storage supports. Furniture that can be used must be separate from painted architecture.
- Personal storage and saved creations with bounded loose-item stock; no automatic deletion of the oldest treasured item.
- Owner-controlled permanent decoration, optional **Decorate together**, forgiving placement previews, safe exits and undo.
- Visitors who can sit and play while the owner is elsewhere or offline; leaving never clears other occupants.
- Gentle owner-initiated tidy help that stores eligible loose items without touching another room, held objects or fixed displays.

Permanent decoration requests must check owner/permission and current object/room revision on the server. Lock only the affected operation/object for the short commit, not the whole room. Undo should invert an eligible prior operation after checking intervening changes; restoring an old full-room snapshot could erase a visitor's later work. Toggling Decorate together off must reject new unauthorized edits without deleting prior accepted work.

Bedroom reading places and personal collections retain their links to the six-book and 20-dinosaur catalogs. Those media/toy systems are unfinished separate Home work. A furnished room milestone may record these dependencies as open, but must not mark H-33 or the full reading experience complete merely because a shelf and placeholder exist. Do not add dead book/TV controls. The four rooms should share full implemented bedroom functionality; one polished room plus three inert copies does not meet this stage.

## 7. Save contracts, objects and migration

Suggested persistent records are a Home place registry, bedroom owner mapping, per-room revision/permissions, furniture instances and placements, room-item instances, and each player's current place and safe position. Use stable definition IDs for reusable furniture/art and stable instance IDs for actual possessions. Each movable item has exactly one location: a holder, legal surface, container or floor placement. Validate no container cycles, repeated instance IDs, missing parents or cross-place holds.

The existing toy validator assumes a small fixed catalog and count. Do not force bedroom furniture and personal possessions through those assumptions without replacing them deliberately. An additive versioned room-item collection is a reasonable initial boundary; it still needs a unified check against duplicate/contradictory item identity across legacy and new collections. Future kitchen transformations can build on this contract without being implemented now.

Use explicit serializable data fields and flat instance references/IDs. Unity's JsonUtility serializes structured fields, does not directly support Dictionary, and follows Unity's container restrictions. Runtime lookup dictionaries can be rebuilt from serialized lists. Avoid shared mutable graph references that silently serialize as multiple values. [JSON serialization](https://docs.unity3d.com/6000.3/Documentation/Manual/json-serialization.html), [serialization rules](https://docs.unity3d.com/6000.3/Documentation/Manual/script-serialization-rules.html).

Extend every snapshot consumer deliberately: creation, validation, deep clone, save/load, network view, private continuation, recovery checkpoint and migration. A shallow room list copy is unsafe when [SoloSaving](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSaving.cs) serializes a captured snapshot on a worker. The worker must receive an immutable/deep-copied state for that revision.

Migration starts from supported existing schemas, including current schema 5/content 6. Reserve the next actual schema/content/build values during implementation after checking current repository activity. Preserve world ID, enrollment, legacy object IDs/contents, fixture state and Keepy state. Add room records once; reopening or repeating migration must not produce extras. Validate after conversion, retain a recoverable prior save, and follow existing durable write/recovery handling rather than replacing live files ad hoc.

Measure expanded snapshot size against the existing transport, recovery and continuation limits before adding room inventories. Four rooms do not justify unbounded prop lists or broadcasts on every drag frame. Commit meaningful drops/style choices, replicate accepted changes, and bound preview traffic. A larger recovery payload must be tested for fragmentation/reassembly, stale/missing chunks, interrupted checkpoints and supported-version rejection. Old clients should receive the established compatibility response, never partial unknown room state.

## 8. Artwork, layering and camera

Build on [Home scene-layer research](home-scene-layer-research-2026-09-26.html) and the [Home art contract](../../SourceArt/Home/README.md). First draw a layout/contact map over the current staircase reference: entry feet position, path, tread contacts, rail mask, landing height and upstairs continuation. Review that geometry with actual Bluey/Bingo poses before final room art.

For stairs and each bedroom, separate architecture/rear furniture, occupants/held items, and front furniture/rail/blanket parts. Shelves and baskets need genuine support/interior regions; foreground bed covers must cover the appropriate part of a resting character. Avoid painting the same usable bed into the wall image and then adding another interactive bed over it.

The current world is drawn with uGUI. Canvas hierarchy controls overlap, with later siblings drawing on top; use that existing mechanism for local front/rear parts. Do not assume SpriteRenderer sorting layers affect RawImage children. [uGUI 2.0 Canvas rendering](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UICanvas.html).

Keep a floor-specific camera with a local stair transition, rather than squeezing two whole floors into one phone screen. The upstairs hall can pan horizontally and rooms can be separate local views. Preserve the viewport adjustment above the open chooser, safe-area margins and lower-left joystick/lower-right family control. Test hall ends, stair arrivals and furniture interactions both with the chooser open and closed.

Later art production should inspect the accepted sources and use the image-generation workflow for new/edited raster assets. This research generates no art and does not change accepted character sheets. New drafts require visual review at phone and 4:3 tablet sizes.

## 9. Loading and older-iPad limits

Keep authoritative room state lightweight and independent of art residency. Reuse one architectural texture set across bedroom variants where practical; independent ownership does not require four duplicate copies of a shared texture. Load current presentation and only the necessary transition/neighbor art. Preserve the existing maximum of three loaded/requested panorama textures while measuring the added fixture/UI budget separately.

Resources.LoadAsync provides asynchronous asset requests; it does not by itself define ownership or a memory budget. Track outstanding requests across view changes and prevent duplicate loads. Do not release an asset still referenced by an active room/transition or pending user. Resources.UnloadAsset invalidates the unloaded object; later loading creates a new instance. [LoadAsync](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Resources.LoadAsync.html), [UnloadAsset](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Resources.UnloadAsset.html).

For scale, a hypothetical 3072 × 1024 RGBA32 image requires 12 MiB of pixel storage before mipmaps or extra copies; three require 36 MiB. This is arithmetic, not a measurement of the current game. Mipmaps, readable CPU copies, fixture atlases, render targets and driver overhead can increase the total. Disable texture Read/Write where runtime pixel access is unnecessary because it retains an additional CPU-accessible copy. [TextureImporter.isReadable](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/TextureImporter-isReadable.html).

Choose platform compression supported by the actual target devices and visually check the art. Unsupported compression can fall back to uncompressed data; Crunch reduces stored/download size rather than guaranteeing a smaller runtime texture. Do not assume a smaller APK proves lower memory use. [Unity texture compression](https://docs.unity3d.com/6000.3/Documentation/Manual/texture-compression-fundamentals.html).

Measure cold/warm stair transitions, repeated room loops, total/texture/managed memory, allocations, frame-time spikes and app background/reopen on the physical A10 iPad. Compare against the accepted baseline and look for growth after repeated unload cycles. Windows/native test success cannot supply those measurements. Unity's profiler exposes memory categories useful for this investigation. [Memory Profiler module](https://docs.unity3d.com/6000.3/Documentation/Manual/ProfilerMemory.html). Numeric room budgets should be set from that baseline rather than invented in this report.

## 10. Bounded implementation sequence

**Follow-up instruction:** the user requested equally detailed secret-room construction/system research and then implementation based on the findings. Sections 13–14 below provide that design. Once research is recorded, start BED-1; do not implement secret rooms ahead of the bedrooms. The research evidence remains separate from subsequent engineering evidence.

Each row is a separate reviewable slice; only one should be active. Research status does not mark any row implemented.

| Slice | Concrete deliverable | Gate before expanding |
| --- | --- | --- |
| BED-1: stairs and landing | Versioned internal-place/transition foundation; visible two-way stair traversal; usable upstairs landing; carried-item continuity. | Four simultaneous travelers, opposite directions, one staying downstairs, repeated requests, failed preparation, save/reopen and disconnect at transition boundaries. Existing Home unchanged. |
| BED-2: four owned destinations | Hall with four functional doors; stable profile-to-bedroom mapping; distinct saved rooms and safe exits. | Four in one room and four in separate rooms; absent owner; same avatar; reconnect and migration without duplication/reassignment. Empty-room progress remains partial. |
| BED-3: functional furnishings | Full usable furniture, storage, decoration/permissions/undo, safe placement and tidy behavior in all four bedrooms. | Conflicting edits/grabs, visiting, personal creation protection, serialized contents and restart. Catalogue/media dependencies explicitly retained. |
| BED-4: bedroom qualification | Integrated art/contact review, regression evidence, compatible recovery/rollout artifacts and available device checks. | Mark only actually qualified bedroom requirements complete. Record physical/A10 or broader catalog gaps individually; no blanket Home completion. |
| SECRET: next feature stage | Four optional persistent secret rooms, using proven room contracts. | Begin after the bedroom stage; retain safe exits, four visitors, calm preferences and entrance-change preservation. |

The first implementation task should identify WORLD-01, WORLD-02, ROOM-01, NET-02 and relevant item/persistence goals through their existing ledger mappings; tracker entries G-06, H-01, H-31 and HOME-BED-A–D remain the practical checklist. BED-1–4 above are work-slice labels, not replacements for the master goal IDs.

## 11. Acceptance cases to prepare before implementation

| Area | Required evidence |
| --- | --- |
| Stairs and touch | Both movement styles; clear entry/exit; actual visible traversal and contact; no through-wall route; four people in opposing directions; no immediate return-trigger loop. |
| Independence | A upstairs, B downstairs, C outside, D in a bedroom; all continue. Four enter/leave the same bedroom; owner departure leaves visitors and room intact. |
| Identity | All choose the same character; change devices and join order; owner mapping unchanged. No duplicate profile admission creates extra rooms. |
| Carried objects | Carry, enter/exit, store, close, reopen and retrieve the same instance. Late join sees one correct holder/location. Concurrent grabs accept one result. |
| Decoration | Visitor edit denied unless enabled; permitted simultaneous edits remain coherent; undo does not erase unrelated later work; exit and arrival areas stay clear. |
| Travel failures | Duplicate/stale requests; delayed movement from old visit; cancel/load failure; lost acknowledgement before/after commit; app pause/kill; disconnect mid-stair. Valid endpoint and one item instance every time. |
| Persistence | Supported old saves, repeated migration, absent owner, server restart and private offline reopen. Existing yard fixtures, shed contents, balloon and enrollment retained. |
| Shared recovery | Room data in live snapshots, local continuation and recovery; payload bounds; restart on correct schema; reconnect loads server state without importing private decoration. |
| Presentation | Existing accepted characters/controls, correct layer overlap, four seats/play positions per bedroom, coherent upstairs art, narrow phone and 4:3 layouts. User visual acceptance separately recorded. |
| Performance | Repeated routes and full rooms on A10 with mixed clients; peak/steady memory and frame times; lifecycle behavior; no monotonic texture growth. |
| Regression and delivery | Sofa/trampoline/radio/shed/Keepy, chooser and major travel continue. Actual candidate's recovery checks and in-place retained-data rollout when devices are available. |

Use disposable worlds for automated/core/native checks. Device availability does not justify probing historical endpoints repeatedly or modifying the live family save. No build number, deployment or physical pass is assigned by this research.

## 12. Decisions resolved and evidence still needed

Resolved for planning: four bedrooms upstairs; real stairs and a second floor first; four optional secret rooms next; stable profile ownership; independent travel; one Home destination; existing custom authority/presentation approach; full backlog retained. These are enough to start the bounded stair/landing slice after this research phase.

Still to establish during implementation: exact stair geometry/contact masks; hall/door spacing on device; final room themes; precise inventory and room record schema; measured memory/payload budgets; artwork acceptance; physical mixed-device and outage/recovery qualification. None should be represented as already tested.

Research validation checks documentation, source traceability and retained checklist scope. It does not certify game behavior. See [source register](evidence/upstairs-bedrooms-research-2026-09-26/sources.json), [research checks](evidence/upstairs-bedrooms-research-2026-09-26/research-validation.json) and [plan/link checks](evidence/upstairs-bedrooms-research-2026-09-26/docs-validation.json). The HTML companion is rendered and structurally checked; browser visual rendering is not claimed.

## 13. How to construct the bedroom assets and reusable systems

Use a **room definition** for installed geometry/art and a **room instance** for saved state. The definition specifies walk bounds, entry/exit anchors, wall slots, support surfaces, furniture anchors, front-cover polygons and optional activity points. The instance supplies its owner, chosen decor, furniture/item instances, permissions and revision. This allows all four rooms to share tested geometry while looking and behaving independently. Loading a definition must never recreate saved possessions.

Author rooms at the existing scene coordinate scale so accepted character size remains consistent. Start with a wide bedroom composition that can pan when necessary; do not choose final dimensions until the bed, four visitors, entry/exit and readable interactive targets fit at phone and tablet sizes. Use a room layout diagram with bounding boxes before generating polished artwork. The wall/floor junction, feet contacts, mattress surface, shelf levels and basket opening must agree across the definition and image layers.

The reusable room builder should instantiate these parts:

| Part | Construction and behavior |
| --- | --- |
| Architecture | Opaque wall/floor/trim art with furniture-free support locations; decorative objects can be painted only if deliberately noninteractive. Windows and wall openings remain consistent with the chosen perspective. |
| Furniture rear/body | Bed frame/mattress, shelf back, basket body and seat bodies. A furniture instance has an installed anchor and declared capabilities, not just a click rectangle. |
| Support/interior | Valid placement slots with item-size/quantity rules and a local coordinate system. Stored items retain identity; closing a basket hides its contents without deleting them. |
| Occupants and items | Render actual player and item state between the correct furniture parts. Occupancy is per usable position; different players can use different furniture simultaneously. |
| Front cover | Bed blanket/footboard, shelf lip or basket rim drawn over the appropriate content. Cover hit regions should not swallow unrelated movement or exit input. |
| Interaction targets | Separate generous hit shapes and explicit action predicates: sit/rest, leave, open/close, store/retrieve, lamp, choose decor. An unavailable action gives a clear local response. |
| Local presentation | Current camera, hover/placement preview, selected palette and menu state. These are not shared room mutations. |

Prefer a small capability set over unique code for every bedroom: `Rest/Sit`, `Surface`, `Container`, `Light`, `DecorSlot`, `Door` and `PersonalDisplay`. A bed can have both occupancy and a plush support slot; validation must prevent impossible overlap. A basket's capacity is real and visible. Tidy help moves eligible items through the same validated storage operation; it does not secretly delete them. Room decorations are saved at successful commit, not while a finger is moving a preview.

A proposed component boundary is: pure room definitions/state/commands in Core; authority/session integration beside existing world commands; a client room view builder; an asset loader with shared-resource lifetime tracking; and presentation adapters for furniture occupancy and cover layers. These are responsibilities, not a requirement for one file per noun. Reuse accepted controls and character visual code. Installed room definitions and save migrations must also be available offline and in headless authority builds without loading textures.

Initial bedroom art can share line weight, pale walls, timber trim and daylight style with the living room. Give each room independently selectable bedding, rug, wall pictures and lamp state. Avoid assigning a permanent “Bluey room” based on avatar; doors should identify the profile using a saved name/icon. All four doors need equal capabilities and space. Decorative themes are editable choices, not different entitlement levels for parents and children.

During art production, create a clean architectural base first, then furniture parts against transparent backgrounds or clean extraction plates with matching geometry. Inspect parts at final scale and assemble a contact proof using the actual character sheets. Record origin/anchor/cover definitions in the art manifest. Final review includes empty room, four visitors, occupied bed, open/filled/closed basket and a fully decorated room. This prevents attractive isolated artwork from becoming unusable in the game.

## 14. Secret-room construction and systems, for the next stage

Each bedroom may gain one little star-marked entrance **at the far back of the room, away from the main entrance**, per the user's latest correction. It stays hidden until the local character approaches, then gently reveals with a glowing star, bounded sparkles and shimmer. Use separate enter/leave proximity thresholds to prevent flicker; no collision/input target while hidden, and no teleport on proximity alone. Each viewer's reveal is local and independent. Reduced-motion keeps a quiet static glow. Reserve a clear far-end wall and approach area in both authored furniture layouts; do not put the secret door beside the hallway door. The interior exit stays visible regardless of reveal or owner presence.

The secret room is an optional persistent instance owned by the same profile, not a randomized new room on every entry. Reuse the bedroom room/door/item contracts, adding only entrance lifecycle and calm presentation. Every secret room supports four visitors and a shared saved layout.

### Creation, entrance changes and reliable exit

The owner's **Make a secret room** action submits an idempotent create request. The authority derives/resolves the unique owned-room mapping, creates missing state once, chooses a valid bedroom wall slot, and publishes the room/entrance together. A repeat or reconnect returns that same room. Two requests cannot spend stock twice or create two secret rooms. Visitors can enter an existing room without being allowed to create/move another person's entrance.

Store the secret room identity separately from its entrance record: owner, parent-bedroom identity, entrance active/archived state, wall slot/style and revision. Moving the mini-door changes this record, never room contents. Archive removes the outside entrance affordance but retains the entire room. Reject new entry using an old entrance revision after it is moved/archived; resolve already committed travel normally. Do not delete or evict occupants to tidy the doorway.

Every interior has a visible exit plus a local return-home control. Resolve exit against the persistent parent bedroom, not a screen position copied from an obsolete doorway. If the external door has moved, use its new safe arrival region; if archived or unusable, use the parent's permanent fallback arrival region. If a damaged record cannot resolve the parent, use a validated upstairs landing/Home anchor and preserve the room contents for recovery. A player can always leave, including while the owner is offline, the entrance is archived or another player edits its style.

Entrance placement uses prevalidated wall slots, keeping bedroom furniture and exit routes clear. Do not implement arbitrary freehand portal placement before reliable route/placement validation exists. A local loading failure follows the same before/after-commit rules as ordinary doors. Secret-room creation itself must survive a save/restart before first entry.

### Physical layout and quiet activities

Use a cozy illustrated room with a deep-blue sky/ceiling, central rug, four cushion/beanbag places, low bookshelf, plush basket, cuddle pillow and blanket fort. Keep the visible exit in a stable location near the foreground walk strip. The central floor must still fit four characters and carried toys; large plush piles cannot obscure every route. A fort needs a defined interior/cover region and an always-available leave action, not a painted tent with an unexplained disappearance.

Reuse bedroom seating, surfaces, storage, personal display and decor permissions. Plush carrying, stacking and tucking-in use the same item identities and bounded location rules. Decide capacity from the actual illustrated shelf/basket/fort rather than unlimited spawning. All planned plush types remain selectable without instantiating every type at once. Reading reuses the eventual local reader: each visitor has their own page/narration state and explicit Play. Until that subsystem exists, record the reading dependency; do not represent static book artwork as working narration.

Entering a quiet room withdraws only that actor from an ordinary hiding activity when the hiding system is integrated. It must not end the round or reset other hiders. Surprise searches, cleanup demands, countdowns and scoring do not belong here. Any future opt-in larger search must preserve this safe exit and individual participation contract.

### Aurora, light, audio and local preferences

Build the sky as one painted base and a small number of low-opacity green/purple ribbons. Slow texture-coordinate motion or simple vertex motion can supply a gentle aurora. A still aurora is the reduced-motion state, not a blank room. Pool a small bounded number of short twinkles from deliberate star taps. Avoid real volumetric lighting, full-screen bloom, a looping video sky or dense particle clouds for this room.

Store shared decor choices, plush/furniture state and agreed room theme in the room instance. Store brightness, motion reduction, music/effects volume and reader bookmarks in each profile's local preferences. One player's mute or still-sky choice must not turn everyone else's sky/audio off. Entering never starts book narration. Local ambience should stop/fade when leaving or backgrounding and should respect the established audio preference on reopen.

Aurora materials/meshes are presentation assets. Rendering and preference adapters must not drive authoritative room simulation. Reuse the same base textures across four secret rooms and enable effects only in the viewer's current room. Inspect transparency overlap and peak memory on A10. The rendering/loading constraints and primary sources in sections 8–9 apply here too; this is a proposed lightweight design, not a measured performance claim.

### Specific secret-room qualification

Test create/repeat/reconnect; four profiles creating separately; four visitors in one room; four different secret rooms concurrently; move entrance while someone prepares to enter; archive while visitors remain; exit after owner disconnect; failed art load; restart with entrance archived and someone inside; held plush continuity; container uniqueness; and persistence of every saved room layout. Check local brightness/motion/audio independence, explicit narration, no unexpected hiding participation and four usable cushion positions. Preserve the room and its contents across every entrance change.

These cases become the next feature stage after the bedrooms. The shared foundation should make that stage smaller, while keeping its distinct entrance lifecycle and calm-play requirements explicit.
