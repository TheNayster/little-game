# Peppa Funfair world phase plan

Owner selection **PEP-01**, October 16, 2026. Goal IDs: WORLD-02 / FAMILY-01 / ITEM-02. Build a functioning DUPLO Peppa Pig Funfair adaptation inside Little Weeps, with its recognizable setting, character actions and five selected activities available to up to four family players together.

**Status: Planned.** This request creates the implementation plan. Phases 0–8 remain pending; next is Phase 0 recovery. [Selected worlds tracker](../selected-worlds-tracker.md) records the other selected worlds. Implementation of those worlds is outside this plan.

## Research and available inputs

The publisher's [app description and version history](https://apps.apple.com/us/app/lego-duplo-peppa-pig/id6474300843?platform=ipad), checked October 16, identifies teacups, a Ferris wheel, the Helter Skelter slide, Hook a Flower prizes and face painting. These establish the required activity set, not exact timings, controls, character cast or internal rules. Physical LEGO Funfair sets and the separate Peppa theme-park apps do not establish this app's behavior.

Existing local source package: `com.storytoys.lego.duplo.peppa.pig.googleplay`, version **14.0.0**, version code **537**. The saved package folder is `LocalData/Research/DuploMarvel/packages/com.storytoys.lego.duplo.peppa.pig.googleplay/`. Retain this original acquisition location; do not rename it because its parent mentions Marvel.

| Saved artifact | Size | Confirmed inventory |
| --- | --- | --- |
| base.apk | 22,698,513 bytes | Includes Unity global-metadata.dat |
| split_config.arm64_v8a.apk | 32,130,106 bytes | Includes arm64 libil2cpp.so |
| split_UnityDataAssetPack.apk | 233,657,021 bytes | Includes assets/aa/catalog.bin and catalog.hash |

Existing initial REA inspection: `LocalData/Research/DuploMarvel/peppa-rea-inspection.json`, evidence `ev_26b922b6490c2450244d9b88221e7f58c90e5ecc4209e1151e5b5bee535ee208`. Its base APK identity is SHA256 `461712d9e40ee026f93b33be3c906d84f3a2d8d884395ee32f0faa3f7da41beb`. Its Android wrapper inspection does not recover the Unity world's native gameplay. The exact Funfair scene and dependency coverage are not yet established.

Reuse the [Hulk](hulk-birthday-world-phase-plan-2026-10-15.md) and [Spidey HQ](spidey-headquarters-world-phase-plan-2026-10-15.md) recovery, binding, rendering, package-receipt and isolated-preview methods. Verify Peppa's actual engine version, animator parameters and component conventions; Marvel's values are not defaults for Peppa.

## Completion contract

Recover the shipped scene, resources, configuration, animation and relevant native behavior. Observation and public descriptions supplement this evidence; they do not replace package recovery. Decompiled empty stubs are structural references, not original C# logic. Mark recovered behavior, deliberate family adaptations and unresolved questions separately.

Each activity's coverage row must include source IDs, input, eligible characters, approach and contact points, action animation, props, audio, start/intermediate/finish states, replay, interruption and persistence. Preserve smaller confirmed interactions discovered in the scene. Functioning buttons without character performance and object contact do not meet completion.

Use the existing 2D game pipeline and PC/VPS authority. Shared rides have one boarding group, common start and cycle; four personal timers do not satisfy shared play. Shared activities retain common station state and progress while protecting independent departures. Private offline progress remains private when joining the authority. Keep the existing Hulk and HQ worlds functioning.

Record any simplification of the source's 3D motion, physics or effects before marking its activity complete. The purpose is a playable family reconstruction, not a claim to recovered original source or identical engine behavior. No change to the official app's purchases or unlocks is part of this work.

## Required activity coverage

| Activity | Mechanics and character actions to resolve | Family implementation and acceptance |
| --- | --- | --- |
| Teacups | Boarding/seat anchors, cup versus platform rotation, input effect, cycle/stop, seated poses and reactions. | Four people share one ride and start. Source-informed motion keeps bodies attached to seats. One departure releases its slot while siblings finish; replay works. |
| Ferris wheel | Loading order, cabin pivots/orientation, passenger capacity, wheel control, stop and unload. | One shared wheel and boarding group; adapt capacity explicitly if necessary. Inspect full revolutions, passenger contact, independent departure and safe return to free play. |
| Helter Skelter | Approach/climb, top entry, authored slide path, queue, seated slide pose, descent and landing. | One fair shared queue, visible waiting players and source-informed turns. A child leaving does not cancel later siblings; all can repeat without stuck poses. |
| Hook a Flower | Rod/hand attachment, aiming/cast/hook input, moving targets, catch rules, prize choice/handoff and finish reactions. | One common booth and target/prize state with fair access. Four children contribute to its session; simultaneous and stale inputs cannot claim the same target or duplicate a prize. |
| Face painting | Chair/contact, available designs/colours, tools, application gesture, face surface, reveal and removal/replay. | One shared service station, fair queue, bounded ownership of the current design, visible painting and reveal. Designs follow the correct character and preserve siblings' work when someone leaves. |
| Environment and supporting play | Exact cast, attendants, idle/walking/seating, food/props, scenery, music, effects and any additional scene-bound activities. | Match confirmed interactions and keep decorative scenery distinct. Add required coverage for newly discovered activities instead of silently omitting them. |

## Phase 0 Recover the exact Funfair world

Verify hashes and package identities against the retained receipts. Inspect the saved catalog and resolve the Funfair pack, exact scene GUID, dependencies and available bundles. Use existing local content first; identify any missing dependencies and their established delivery paths. Keep originals immutable and isolate extraction/import tools from production assets.

Create a scoped evidence inventory at planned ignored `LocalData/Research/PeppaPig/world-tests/Funfair/`. Its purpose is PEP-01 recovery; consumers are the future Funfair asset pipeline and verification tools. Preserve it while the world or its provenance depends on it. Record export settings, tool versions, scene/resource hashes, references, cast, animation controllers, hand/seat/tool anchors, sound bindings and missing or unresolved content.

Trace scene-bound activity controllers and native methods where rules or timing remain unclear. Resolve especially teacup control, Ferris loading/orientation, slide path, hook/catch/prize logic and painting gestures/design binding. Imported assets must be inspected, not accepted from filenames alone. Do not ask the owner to purchase/open the locked pack as a recovery prerequisite. If content remains unavailable, record the exact gap rather than replacing it with an unacknowledged guess.

**Exit:** exact scene identity and dependency coverage; all five activities and discovered extras mapped to evidence, actions and acceptance scenarios. Unresolved behavior is explicit.

## Phase 1 Establish the world and shared state

Confirm current shared integration points and ownership before editing. Plan world-owned rules under `Code/Core/Worlds/PeppaFunfair`, presentation under `Code/Client/Worlds/PeppaFunfair`, and shipped resources under `Resources/Worlds/PeppaFunfair`, beneath the existing Unity asset root. Add actual consumers to `docs/project-layout.json` when those folders are created. Recovered media follows the established ignored research and checked local packaging policy.

Add a recognizable world card, arrival/return locations, camera bounds and the recovered environment in the existing 2D pipeline. Keep four arrivals readable. Introduce authoritative station/seat/queue/object identities and explicit transient versus saved state. Coordinate GameWorld, GameScreen, FamilySession, dispatch and navigation; do not assume world folders isolate shared files. Choose compatibility/schema changes against the current source when implementing, not a preassigned build number.

**Exit:** four actual clients can enter, move, identify their player, change areas independently and return to one common Funfair. Existing source-world migration remains valid.

## Phase 2 Recover and integrate character performance

Bind the scene-confirmed Peppa cast and attendants with idle, walking/facing, greeting, boarding, seated riding, climbing, sliding, landing, holding/using tools, prize receiving and painting/reveal reactions. Include start, interruption and return-to-idle transitions. Do not invent a cast of four solely from the app description; resolve source characters and provide an explicit four-player role/helper policy.

Preserve hand, face, foot, seat and prop anchors through baking/rendering. Keep correct orientation while rides move and while characters turn. Expose clear pictured interactions and player markers suitable for the youngest child. Preserve editable source/provenance and label any necessary new adaptation artwork.

**Exit:** actual native free-play and action/contact checks at normal speed; source characters are recognizable, four participants are distinguishable and no required action is silently replaced by an unrelated idle pose.

## Phase 3 Complete the three rides

Implement teacups, Ferris wheel and Helter Skelter from the recovered paths, pivots, controls and transitions. Reuse existing seating/ride/rendering helpers only where their contracts fit. Each ride owns one authoritative boarding/queue/cycle state, pictured readiness/start feedback and fair replay. Loading and rider departure must preserve seat identities and other participants' progress.

For each ride, check the complete approach → board/climb → moving action → finish → unload/land loop. Test four people, staggered arrival, a child leaving while active and the remaining group finishing. Verify attachments and occlusion throughout motion; captures of stationary seats alone do not qualify ride contact.

**Exit:** all three rides work through normal controls in a representative four-client visit, including replay and independent departure, with focused source rule checks for their state transitions.

## Phase 4 Complete Hook a Flower and prizes

Recover the actual booth layout, rod and flower motion, input/aiming model, hook/contact conditions, missed attempts, prize catalog and handoff. Implement a pictured playable booth with visible tool motion and character reactions. Adapt to a common four-person session with shared booth/target state and bounded ownership; follow source concurrency where it permits several rods, otherwise use one fair shared service queue.

Keep prize identities authoritative. Implement the source-confirmed holding, placing, giving or display behavior, and define its saved state. Test simultaneous catches, repeated delivery/stale commands, changing areas during an attempt and replay without exhausted or duplicated targets. Do not substitute Marvel's water-gun aiming code for an unverified Peppa hook mechanic.

**Exit:** four participants can use the same booth session, see catches and prizes, leave independently and replay; contested-target and prize-identity checks pass.

## Phase 5 Complete face painting

Recover and implement design selection, tool/application gestures, chair/camera framing, character-specific face masks, colour changes, finish reveal and source-supported reset/removal. Keep face art attached during facing changes, normal motion and any compatible ride animations. If original projection requires a 2D adaptation, document the mapping and inspect it on every supported painted character.

Use a common station and fair shared service state. Define whether children paint themselves, each other or an NPC from the source evidence and the family adaptation. An interrupted painter releases only the active edit/turn; completed designs and siblings' queued participation remain valid. Persist only valid bounded design references and colours, not arbitrary generated file paths.

**Exit:** normal touch/mouse input produces a visible finished design on the intended character; all four can participate in one station session, independent exits work and reopening preserves the chosen persistent design contract.

## Phase 6 Finish the complete world

Close every recovered supporting-play row: attendants and civilian life, incidental props, confirmed food or building activities, discoveries, idle/reaction loops, scenery, soundtrack and interaction effects. Add source-discovered activities to this plan with their own checks before closure. Do not import rides or mini-games from Marvel's similarly named Funfair.

Review complete PC and phone/tablet-shaped views for pictured controls, character scale, depth, tool contact, motion and readable shared cues. Inspect normal-speed actions and moving seat attachments. Respect existing mute/pause/ducking settings; listen before claiming audio quality. Avoid introducing recording workflows unless requested. Resolve any concrete defects found in this pass.

**Exit:** the activity coverage table has no silently omitted required interaction; source behavior and family adaptations are distinguished, and actual visual/audio evidence is recorded with its limits.

## Phase 7 Verify shared play and recovery

Run the focused editor/rule checks and one representative native four-client visit across the rides and booths. Include shared start/progression, a late arrival, a contested interaction, an independent area exit/disconnection and fresh reconnect. Use actual touch/mouse controls for activity acceptance and identify any fixture setup separately.

Reopen an isolated saved authority and fresh clients: preserve completed designs, prize identities and any creations/progress selected for persistence. Normalize interrupted rides, queues and edit leases safely. Check one private visit, its actual process reopening, automatic shared join without private imports, and explicit return to the separate private save. Verify affected migration/admission contracts. Run a focused Hulk/HQ regression only for shared helpers changed by this work.

**Exit:** compilation, changed gameplay, representative four-client behavior, saves/reopening and private/shared separation pass on matching source. Repeat only failed or newly changed paths; retain failed evidence without relabeling it passed. Physical-device and owner acceptance are separate from Windows layout simulation.

## Phase 8 Package and deliver

Package all required Funfair art/audio with a checked manifest into a fresh current-source PC candidate. Verify provenance, file hashes and packaged loading without a research-directory override. Provide a configured one-to-four-player saved-family preview launcher, preserving the Hulk and Spidey HQ preview families and real family data. Confirm durable readiness and the actual intended world, not just process creation.

Update this plan, the selected-world tracker, main build guide and relevant decisions with exact completed scope, evidence, adaptations, remaining limits and usable launcher links. Stage only owned source/docs; integrate checked work into main, push and verify origin/main, and remove only a finished task branch if one was needed. Preserve unrelated untracked work and ignored recovered media.

**Exit:** a usable packaged PC world and checked launcher, completed technical acceptance record and verified source delivery. Device installation or live-server replacement follows a separate requested rollout and the current server policy; this plan does not authorize either.

## Phase status

| Phase | Status | Evidence |
| --- | --- | --- |
| 0 Exact world recovery | Pending | Initial APK inspection exists; Funfair scene recovery pending |
| 1 World and shared state | Pending | — |
| 2 Character actions | Pending | — |
| 3 Three rides | Pending | — |
| 4 Hook a Flower and prizes | Pending | — |
| 5 Face painting | Pending | — |
| 6 Complete world presentation | Pending | — |
| 7 Shared play and recovery checks | Pending | — |
| 8 Packaging and source delivery | Pending | — |

Next bounded task: **Phase 0, resolve and inspect Peppa Funfair's exact shipped scene and dependency closure.**
