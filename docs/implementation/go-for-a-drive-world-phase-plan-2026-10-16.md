# Go for a Drive world phase plan

Created October 16, 2026. Selected world **BLU-02**, the third entry in the [selected worlds tracker](../selected-worlds-tracker.md). Goal IDs: WORLD-02 / FAMILY-01 / ITEM-02.

Create a complete playable Go for a Drive world in Little Weeps, with up to four family players taking one road trip together. Include packing the car, travelling with Dad to the Big Peanut, the Grey Nomads encounter, window games, arrival and replay. Preserve the established 2D presentation, recognizable Bluey characters and independent travel between existing worlds.

**Status: Planned.** This request creates the implementation plan. All phases below remain pending. Planning this entry does not set an implementation order for the other selected worlds or authorize device/live-server deployment.

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
| DRIVE-01 | Pack the car | Players prepare one car using recovered items/placement rules without duplicated objects. | 2 | Official description; exact interactions pending |
| DRIVE-02 | Road trip with Dad | One route and stage visible to everyone, with useful passenger actions. | 3 | Official description; route/controls pending |
| DRIVE-03 | Grey Nomads | One encounter with common progress and fair replay/recovery. | 4 | Official description; rules pending |
| DRIVE-04 | Window entertainment | Every recovered window activity, with concurrent contribution and preserved sibling progress. | 5 | Official description; inventory pending |
| DRIVE-05 | Big Peanut arrival | Common destination/finish, deliberate replay and confirmed destination interactions. | 6 | Destination confirmed; interactions pending |
| DRIVE-06 | Smaller interactions and character life | Recovered car props, reactions and incidental actions. | 1–6 | Inventory pending |

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
| 0 Inspection | Pending | Exact world/dependencies and complete activity map |
| 1 Foundation/actions | Pending | Four-participant view and normal-speed contact review |
| 2 Packing | Pending | Shared loop and contested-item/departure check |
| 3 Journey | Pending | Common travel, late join and independent exit |
| 4 Grey Nomads | Pending | Encounter, shared result and retry |
| 5 Window activities | Pending | Complete inventory, concurrent contribution and interruption |
| 6 Arrival/persistence | Pending | Full loop, saved reopening and private/shared separation |
| 7 Verification | Pending | Current-source four-client visit and presentation review |
| 8 PC delivery | Pending | Manifest, launcher, durable readiness and pushed source |

Next implementation action, when requested: Phase 0, starting with retained LEGO Bluey packages and existing inspection records. Keep one bounded implementation task active and record recovered scope before production gameplay.
