# Go for a Drive world phase plan

Created October 16, 2026. Selected world **BLU-02**, the third entry in the [selected worlds tracker](../selected-worlds-tracker.md). Goal IDs: WORLD-02 / FAMILY-01 / ITEM-02.

Create a complete playable Go for a Drive world in Little Weeps, with up to four family players taking one road trip together. Include packing the car, travelling with Dad to the Big Peanut, the Grey Nomads encounter, window games, arrival and replay. Preserve the established 2D presentation, recognizable Bluey characters and independent travel between existing worlds.

**Status: PC ready.** All implementation phases are technically complete through packaged PC delivery. Owner visual/motion/audio review and physical-device/live-server delivery remain separate. The October 16 authorization to finish all phases and preserve other chats' work was followed.

## Scope and evidence

The [official LEGO Bluey listing](https://play.google.com/store/apps/details?id=com.storytoys.lego.bluey.googleplay), checked October 16, describes Bluey and Dad travelling to the Big Peanut, packing their car, keeping ahead of the Grey Nomads and making entertainment at the window. The listing calls the pack Let's Go for a Drive. Keep BLU-02 stable when its exact installed title and identity are resolved.

These descriptions establish required activities. Exact objects, controls, routes, cast, timing, animations and incidental interactions must come from world inspection. Four-player rules below are proposed family adaptations. Do not infer steering, racing, drawing tools, scoring, building activities or extra destinations from the description alone.

Follow the [current decisions](../current-decisions.md), [creation workflow](../ai/GAME_CREATION_WORKFLOW.md), [folder guide](../project-folder-guide.md) and [build guide](../family-playset-build-guide-2026-09-23.md). Reuse recovery and delivery methods from Hulk and Spidey HQ where appropriate, keeping their mechanics and package identities separate.

## Completion contract

- One authoritative journey and common progress for up to four players. Everyone can contribute; four separate car trips or personal minigame rounds do not meet the goal.
- Dad remains the driving character unless recovered behavior establishes another role. Family avatars retain their identity and useful actions without requiring four human drivers or displacing Dad.
- A pictured invitation and one shared Start begin the participating group's trip. Do not require four players or a synchronized gesture. Late joiners enter the current stage; departure removes only that player, and returning players can rejoin without restarting siblings.
- NPC assistance fills necessary missing roles in solo/smaller groups without taking a human player's owned object or completing their active interaction unexpectedly.
- Every interaction includes character actions, approach/contact, intermediate feedback, finish reaction, replay and interruption behavior. Buttons and static pictures alone are insufficient.
- PC/VPS remains the shared authority. Offline solo stays private; reconnect loads server state without importing offline edits. Saved checkpoints and transient inputs have explicit boundaries.
- Inspect actual full views and normal-speed motion, and listen before claiming audio quality. Technical completion, owner review and device/live-server delivery remain separate.

## Activity coverage

Phase 0 expands this table with source identities, controls, eligible actors, object ownership, contact anchors, animation/audio, completion/replay, interruption, persistence and exact validation receipts. Add newly recovered interactions to the relevant phase.

| ID | Required activity | Shared outcome | Phase | Evidence now |
| --- | --- | --- | --- | --- |
| DRIVE-01 | Pack the car | Players prepare one car using recovered items/placement rules without duplicated objects. | 2 | Recovered six source sets; shared native packing passed |
| DRIVE-02 | Road trip with Dad | One route and stage visible to everyone, with useful passenger actions. | 3 | Three source lanes/stops; native common journey and saved roadside recovery passed |
| DRIVE-03 | Grey Nomads | One encounter with common progress and fair replay/recovery. | 4 | Recovered net-overtake/difficulty rules; native traffic contribution passed |
| DRIVE-04 | Window entertainment | Every recovered window activity, with concurrent contribution and preserved sibling progress. | 5 | Fifteen original patterns; shared physical rewards and looping stickers passed |
| DRIVE-05 | Big Peanut arrival | Common destination/finish, deliberate replay and confirmed destination interactions. | 6 | Twelve source pieces, original palette, saved creation, car tumble and shared replay passed |
| DRIVE-06 | Smaller interactions and character life | Recovered car props, reactions and incidental actions. | 1–6 | Recovered source props/actions/audio; hand anchors and real pictured controls integrated |

## Phase 0 Inspect the exact world

Inspect the retained LEGO Bluey package collection and existing REA records read-only first. Resolve the installed version, pack identity, scene, catalogs, bundles and dependency closure. Keep this app separate from Bluey Let's Play and the unrelated old Unity project. Record exact local paths, hashes, scene/object identities and missing dependencies; package collection does not establish world recovery.

Trace active scene controllers, prefabs, localization, route data, car/luggage objects, cast, clips, sounds, camera transitions and finish/reset events. Use the established REA workflow where a material behavior remains unresolved. Distinguish executable behavior from empty exported stubs, deprecated types and marketing descriptions. Keep exports/private captures in established ignored LocalData research locations.

Resolve packing objects and valid placements, what advances travel, how the Grey Nomads respond, every window game/control, arrival/return, and optional building or other play actually present. Label confirmed behavior, proposed 2D/family adaptations and unresolved details separately. No official-app entitlement changes are part of reconstruction.

**Exit:** exact world/dependencies identified; every activity has an evidence row and acceptance scenario. Missing content has a specific blocker and bounded effect on later phases. Filenames alone do not establish recovery.

## Phase 1 Establish the world and character actions

Confirm the actual Unity project and live provider readiness before edits. Use world-owned rules, presentation and resources beneath `Unity/FamilyPlayset/Assets/FamilyPlayset`, following `Code/Core/Worlds/<World>`, `Code/Client/Worlds/<World>` and `Resources/Worlds/<World>`. Choose the stable world key after inspection, record ownership/consumers in `project-layout.json`, and retain editable art/audio and provenance. Coordinate GameWorld, GameScreen, FamilySession, navigation and shared dispatch deliberately.

Build the recovered departure area, car exterior/interior, travel view and destination in the existing 2D pipeline. Preserve characteristic scenery, layering and cast. Fit four identified participants without obscuring controls or reducing anyone to a spectator. Implement required walking/approach, pickup/carry/place, car entry/seating, passenger gestures, Dad's driving pose and reactions. Verify facing, hand/object contact, seated alignment, occlusion, cancellation and idle recovery.

**Exit:** navigable departure/car foundation, four identifiable participants and focused normal-speed action/contact and full-view control review. Record remaining activity-specific artwork.

## Phase 2 Pack one car together

Implement recovered item selection, pickup, carrying, placement, removal/revision and packing completion. Keep the same visible object through handoffs, with pictured valid destinations and preparation feedback. Do not impose a packing puzzle if the source uses open-ended placement.

Use authoritative claims/revision checks so two players cannot duplicate a carried or packed item. Other children retain useful available actions. Departure releases only that player's transient claim and safely resolves their carried object; completed packing remains. Establish common readiness/start without allowing an incidental tap to erase siblings' preparation.

**Exit:** complete packing/start/replay loop, invalid/repeated-input handling and a focused four-player contested-item/departure check. Object identities and completion agree across clients.

## Phase 3 Travel with Dad in one shared car

Implement recovered route progression, scenery, cameras and car/passenger actions. Dad's role and human controls follow inspected behavior. Use one server-owned stage and any common clock; clients cannot advance private copies. Smooth presentation locally while keeping interaction outcomes authoritative.

Handle late join, departure, area change and reconnect at departure, in transit and at arrival. Players can leave independently while siblings continue. Solo/smaller groups finish with appropriate assistance. Define last-participant exit behavior so an empty world does not advance unattended or block the next visit.

**Exit:** playable packing-to-destination route with synchronized transitions, useful four-player actions, independent departure and late join into current progress. Full encounter/window mechanics follow in Phases 4–5.

## Phase 4 Add the Grey Nomads encounter

Implement the recovered trigger, vehicles/cast, movement, inputs, progress, feedback, outcome and replay. Establish whether it is a timing challenge, scripted interruption or another form of play before choosing controls. Preserve its playful tone without adding unsupported collision penalties, competition or fail conditions.

Adapt the actions for four contributors to one encounter without conflicting vehicle control. Show accepted contributions and common progress. Apply repeated/stale inputs once, release departing roles and provide assistance only when needed. Retry preserves the intended checkpoint without duplicating vehicles or rewards.

**Exit:** natural trigger through result/retry in one shared journey, including an active participant leaving while others finish. Record the exact family adaptation.

## Phase 5 Add every window activity

Implement the complete recovered inventory: tools/props, gestures, effects, sounds and stop/replay. Choose shared ownership or separate contribution areas within one common activity according to the interaction. Do not assume window entertainment means drawing, spotting or a particular game.

Rounds use common invitation/start and shared progression/results; open-ended play can remain untimed. Contested tools, repeated inputs and departures cannot erase sibling contributions. Verify touch/mouse controls at actual travel-view scale with clear ownership cues.

**Exit:** every window activity has a complete input/action/result/replay loop; four players contribute together and interruption/reconnect preserves appropriate common state.

## Phase 6 Finish arrival replay and saved progress

Complete the Big Peanut scene, common arrival reaction, confirmed destination interactions and return/replay. Include source-confirmed building and smaller interactions in coverage. Reset deliberately and consistently; local Return/Leave cannot reset another player's trip.

Define the saved checkpoint contract for packing, journey stage, completed activities, kept creations and destination state. Claims, held inputs, temporary effects and reservations release or restore safely. Reopening cannot duplicate objects or repeat finish rewards. Record schema/content/protocol changes for actual contract changes; presentation alone does not warrant a compatibility bump.

**Exit:** all activities form a complete loop. Actual save/process reopening covers an interrupted trip and an older save without this world; private-to-shared handoff preserves authority boundaries.

## Phase 7 Verify the complete family visit

Compile after a coherent edit group using the active provider/build procedure. Run one focused current-source pass with four actual clients: shared packing/start, travel, Grey Nomads, window play, arrival/replay, late join, independent world departure and reconnect. Include changed save/admission contracts and solo/NPC behavior; reuse valid source-matching phase evidence. Shared helper changes require one relevant existing-world regression.

Review full PC and representative phone/tablet-shaped views during loading, departure, seated play, travel, feedback and arrival. Check recognizable art, pictured controls, contact and normal-speed motion. Listen for sound claims. Layout simulation does not qualify physical devices.

**Exit:** every required row has implementation/runtime receipts and source/build identity, with real failures resolved or explicitly blocking readiness. Owner visual/motion/audio review is separate.

## Phase 8 Package and deliver the PC candidate

Package required art/audio and verify the current-source manifest. Provide a configured isolated saved-family PC launcher for one to four players using the established preview pattern. Preserve real saves, enrollment, installed apps, live authority and other preview selections. Verify durable readiness and packaged trip behavior, beyond process creation.

Update this plan, tracker and build guide with results, adaptations, limits and the launcher. Integrate checked source into main, push and verify origin/main, then remove only a finished task branch if needed. Requested device/live-server delivery is separate; re-read `server-update-policy.md` before rollout and coordinate genuine compatibility changes.

**Exit:** packaged PC candidate, verified launcher/content and linked technical evidence. Only then mark BLU-02 PC ready, with owner review and deployment recorded separately.

## Current phase record

| Phase | Status | Required evidence |
| --- | --- | --- |
| 0 Inspection | Complete static recovery | Retained 7.1.0 package, four scenes, exact source props/rigs/patterns, native method mapping; evidence below |
| 1 Foundation/actions | Technically complete | Source rigs/actions/hand anchors integrated; four distinct avatars inspected; owner motion/audio review open |
| 2 Packing | Complete | Native original-item claims/roof placement; authority checks cover stale claims, spills and waves |
| 3 Journey | Complete | One car, shared stops, independent departure, partial fuel and actual roadside process recovery |
| 4 Grey Nomads | Complete | Actual four-client lane/boost controls plus rule-checked net overtakes and source thresholds |
| 5 Window activities | Complete | All fifteen source patterns, stationary shared drags, physical rewards, sticker cycles and retained rewards |
| 6 Arrival/persistence | Complete | Native cooperative statue/free building, process reopening, tumble/rebuild, consent replay and private/authority separation |
| 7 Verification | Technically complete | Scoped runtime receipts below; full PC and phone/tablet-shaped views inspected; owner review separate |
| 8 PC delivery | Complete | Candidate587, 438-file media receipt, checked one-to-four-player launcher and main source delivery |

Current bounded implementation task is complete. Use the configured PC launcher for owner review. Device or live-server rollout requires its separate requested delivery and the current server-update policy.

## Recovery and implementation checkpoint October 16

The retained target is `com.storytoys.lego.bluey.googleplay` 7.1.0 (1551), Unity 6000.0.58f2. Its embedded split contains the main `SCN_00_GoForADrive` and four complete subscenes: Imperfect Packing, Drive Time, Window Games and Build Big Thing. The original offers four replayable activity scenes; the common family journey linking them is an explicit adaptation.

[Recovery tools and provenance](../../Tools/Art/GoForADrive/README.md) preserve APK/export hashes, scene identities, source pointers, rendering recipes and unresolved execution details. Ignored `LocalData/Research/GoForADrive/Native/findings.txt` links the independently mapped and decoded 1,428 selected ARM64 methods. These are static findings, not original C# or runtime acceptance.

The final immutable content input is `LocalData/Research/GoForADrive/Content-fixtures`, with adjacent `Content-fixtures-receipt.json`: 185 picture keys, 174 action strips and 139 audio keys across 438 files. Earlier packs are preserved. Corrected transparent fixtures use the original toilet tree/dog sign, petrol pumps/canopy/shop and roadworks brick/spade/boulder group. Three additional traffic vehicles and all fifteen window patterns are included. Each pattern's obstacle-only and reward-only layers compose byte-for-byte to the original full pattern. Exact source collider/reward projections exclude the broad input-surface collider.

| Activity | Recovered scope and behavior | Implemented family adaptation |
| --- | --- | --- |
| Packing | Six sets of three original items; waves use three times the wave number; difficulty `[1,2,2,2,3,3,3,4]`, looping from index four. Attachment positions affect car balance; the source supports warning/bounce and buckaroo. | One shared set of objects, revision-checked hands and twelve pictured roof positions. A shared lever/tilt projection replaces 3D balance physics; spilled items remain reusable. Shared Go follows completed preparation. |
| Driving | Three lanes; Nomad plus recycling truck, tractor and truck; net overtaking can rise or fall. Difficulty counters `[2,4,6,6,12,18]`; source stop order is Toilet, Petrol, Roadworks, cycling. | One navigator lease, passenger cheering/horn actions and common progress. Three stops form this trip; later trips progress through the recovered difficulty list. Exact original boost/countdown wiring remains partially unresolved, so the 2D encounter timing is an adaptation. |
| Roadside play | Toilet, petrol fixture/nozzle, cow, rubble and traffic light. Source nozzle connects/disconnects charging; charging declares 4.5 seconds. | Shared stop completion, protected use, preserved partial fuel and direct hand/nozzle contact. A simple pictured hose substitutes for the original flexible LineRenderer. |
| Window play | Fifteen ordered animal/hay/tractor patterns, actual reward colliders, single/double jump and dragging. Obstacle hits disqualify that pattern's star. Ten sticker positions wrap; each full cycle adds 0.5 speed, hits subtract 0.5 with minimum three. | All four help control one source hand. Drag ownership expires safely; any participant can help with a second jump. Collision uses the recovered projections in 2D. Progress remains shared and open-ended. |
| Big Peanut | Twelve original statue pieces and their exact source-camera layers/anchors; thirty-six palette variants; 300-brick limit, inspiration sign and car tumble. | A shared snapped statue and free building board retain object IDs and contested edit ownership. A moving car causes the tumble; an independent Leave never erases construction. Replay requires active participants' consent. |

Bluey and Bandit are the original activity cast. Bingo and Chilli use their actual shared-package DUPLO rigs as family additions. Recovered hand-angle/bend/wave layers supply documented carry/build/throw presentations; literal driving and child toilet clips remain distinguished from these recipes. Actor hand/face anchors are sampled; ordinary prop grips use an explicit center-at-hand adaptation, while the nozzle uses its original projected logical hole.

The earlier focused Unity authority check passed migration, shared invitation, claims, departure/rejoin, packing waves, stops/nozzle retention, window rewards, saved construction and replay consent. Subsequent balance, source collision and car-tumble changes require the next matching check. Standalone .NET execution was blocked by Windows Application Control; the signed Unity editor runs the same rule checks without changing policy. Whole-player compilation, four-client behavior and packaged readiness remain pending at this checkpoint. No devices, real saves or installed server changed.

## Technical acceptance and PC delivery October 16

**Drive-only source delivery uses schema57/content91/protocol3 and drive format2.** Candidate587 was built from the combined working checkout at schema58, which also contains another chat's uncommitted Markets fields. Its isolated preview saves retain that combined schema. The delivered 77-file Drive-only core was independently compiled and rule-tested at schema57 so this commit does not prematurely claim the later Markets migration. The candidate is accepted for the unchanged Drive activity/save rules and tested presentation; it is not a byte-for-byte clean-main artifact or an installed-server replacement. Compatibility changed for the new authoritative activity/save contract and corrected shared-hand rules, not for artwork.

[Play Go for a Drive](../../Tools/Launch/Play-GoForADrive.ps1) opens a separate saved family, default two players, supporting one through four. The selected ignored configuration is `LocalData/go-for-a-drive-preview.json`. Close the preview windows to stop its separate authority. The launcher preserves its own family on reopening and does not change the installed family selection.

| Evidence | Actual result and scope |
| --- | --- |
| `LocalData/Logs/build-network-583.log` | Standard whole-project prebuild gate and both release players passed. Later changes affect Drive presentation/input handling; no later Drive authority/save changes. |
| `LocalData/Logs/build-network-587.log` and `Builds/NetworkProbe/G3-0.0.587/build-summary.json` | Final focused Drive plus Spidey HQ/Hulk regression checks passed; server and client both succeeded with zero errors/warnings. |
| `LocalData/GoForADrive/delivery-rule-check-schema57.log` | Signed Unity independently compiled and passed all Drive checks against the exact 77-file Drive-only core delivery projection, excluding unfinished Markets/Funfair source. Migration, invalid snapshots, stale ownership, spills/waves, stops, common double-jump, source collisions, ten-sticker speed increase, statue/copy/restore, empty-world pause and consent replay passed. |
| `LocalData/SharedGarden/d9fb00f8d98841aea7ccb7ae7f9c0976/drive/` | Candidate586: one real Start joined four clients; all three original objects were carried and packed through actual controls. Native traffic, toilet and partial-fuel/departure paths reached the final roadworks checkpoint. |
| Same family, `drive-continue/` | Four fresh processes resumed the saved roadworks checkpoint and completed its common stop. The initial window route collected nine physical stars over all fifteen patterns; its ten-star assertion failed, retaining the actual progress. |
| Same family, `drive-window-continue-1/result.json` | Candidate586 passed all fifteen source patterns through real four-contributor drags, a full looping sticker cycle/speed increase, cooperative twelve-piece Peanut, palette/free-board placement and native disconnect/reconnect. Zero runtime exceptions. |
| Same family, `drive-reopen-1/result.json` | Candidate587 passed actual saved tumble reopening, real rotated-brick rebuilding, retained common rewards/IDs, native reconnect, moving-car tumble retaining every piece, and all four replay choices starting one common next packing trip while retaining the creation. Zero runtime exceptions. |
| `LocalData/FamilyLAN/1c36b14eb4ad45028d235acc3dbefda4/go-for-a-drive/result.json` | Candidate585 actual private process reopening, stable center-roof item, automatic private-to-authority handoff without uploading private packing, and explicit solo return all passed. Reused for unchanged save/handoff contracts. |
| `LocalData/go-for-a-drive-launcher-check.json` | Final configured four-player packaged launcher remained ready beyond startup. |

The first native checks found overlapping statue targets and a hidden-hand release callback flooding shared input; both were corrected before acceptance. A test clicked the roadworks light during the preceding rubble cooldown; its saved checkpoint was resumed and finished rather than resetting the family. Window test gestures were corrected to cross the real UGUI drag threshold, retain stationary input and acknowledge release; rewards still require actual source-collider contact. The generic inspection catalog omits rotated rectangles; the native test presses the verified rotated Button's real screen center. An off-center single-bag private fixture correctly spilled and was replaced with a stable center placement for retention testing. Earlier failed/intermediate records remain preserved.

Candidate587 changes only destination avatar positions from586: the statue and palette no longer cover the participating cast. Actual PC and 1280×591/1024×768 destination views were inspected, with four distinct avatars clear. Source sprites/rig clips, fixed hand anchors, source-camera statue layers, transient walking/throw presentations and contact are integrated. Free building uses the original source palette pictures as 2D pieces. Simplified balance/lanes/hose/collisions and pose recipes remain explicit family adaptations; exact original boost/countdown wiring and secondary material effects are not claimed restored. Owner normal-speed motion/audio approval and physical-device qualification remain open; no listening approval is claimed.

The final media pack is 88,627,079 bytes. Manifest SHA-256: `cc08f112b635a15b7b7119dcad4ecaa495c5840cb39c6eb61e9f2cb0350894e1`; receipt SHA-256: `3de5d99a9f6bbcd00632813844f3c36ff75760fda762b5cf972b4ae6df568e00`. Packaging verifies every input and candidate hash, preserves source media and records final artifact hashes. APKs, original exports, previous packs, captures, generated probes and family saves remain ignored and retained.

The shared checkout also contains other chats' unfinished Markets/Funfair files. Their working files and tracked/untracked work were preserved. Only Drive source and its own shared integration hunks are delivered. The combined candidate is accepted for Drive; unrelated draft worlds are not qualified by these receipts. Documentation consistency found one unrelated unclassified Markets inventory record; the retained report is `LocalData/GoForADrive/docs-validation-concurrent.json`, with no Drive-document errors. No installed app, live server, signing, enrollment or real family data was changed.
