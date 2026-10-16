# Peppa Funfair world phase plan

Owner selection **PEP-01**, October 16, 2026. Goal IDs: WORLD-02 / FAMILY-01 / ITEM-02. Build a functioning DUPLO Peppa Pig Funfair adaptation inside Little Weeps, with its recognizable setting, character actions and five selected activities available to up to four family players together.

**Status: all PC implementation phases technically complete; combined release685 checked and integrated on main.** Phases0–7 are technically verified. Phase8 packaging and the checked one-to-four-player launcher pass; the combined Markets/Funfair source is checked and integrated on main. [Selected worlds tracker](../selected-worlds-tracker.md) records the other selected worlds. Physical-device rollout and owner visual/audio acceptance remain separate.

## Research and available inputs

The publisher's [app description and version history](https://apps.apple.com/us/app/lego-duplo-peppa-pig/id6474300843?platform=ipad), checked October 16, identifies teacups, a Ferris wheel, the Helter Skelter slide, Hook a Flower prizes and face painting. These establish the required activity set, not exact timings, controls, character cast or internal rules. Physical LEGO Funfair sets and the separate Peppa theme-park apps do not establish this app's behavior.

Existing local source package: `com.storytoys.lego.duplo.peppa.pig.googleplay`, version **14.0.0**, version code **537**. The saved package folder is `LocalData/Research/DuploMarvel/packages/com.storytoys.lego.duplo.peppa.pig.googleplay/`. Retain this original acquisition location; do not rename it because its parent mentions Marvel.

| Saved artifact | Size | Confirmed inventory |
| --- | --- | --- |
| base.apk | 22,698,513 bytes | Includes Unity global-metadata.dat |
| split_config.arm64_v8a.apk | 32,130,106 bytes | Includes arm64 libil2cpp.so |
| split_UnityDataAssetPack.apk | 233,657,021 bytes | Includes assets/aa/catalog.bin and catalog.hash |

Existing initial REA inspection: `LocalData/Research/DuploMarvel/peppa-rea-inspection.json`, evidence `ev_26b922b6490c2450244d9b88221e7f58c90e5ecc4209e1151e5b5bee535ee208`. Its base APK identity is SHA256 `461712d9e40ee026f93b33be3c906d84f3a2d8d884395ee32f0faa3f7da41beb`. Its Android wrapper inspection does not recover the Unity world's native gameplay. At that initial wrapper-only inspection, Funfair scene/dependency coverage was not established; the completed Phase0 recovery below supersedes that limit.

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
| 0 Exact world recovery | Complete with explicit adaptation | One main/four activity scenes,14 bundles, zero scoped parse/dependency errors; [source inventory](peppa-funfair-source-inventory-2026-10-16.md). Separate teacup controller was not found. |
| 1 World and shared state | Technically verified | Authoritative commands, shared clocks/queues, deep snapshots, legacy additive migration and Unity JSON restore pass. |
| 2 Character actions | Technically verified | Seven recovered roles,25 scene instances,236 strips, original hand/face anchors, role selection and source-eligible slide cast. Native body/tool contact inspected. |
| 3 Three rides | Technically verified | Four actual clients board/start/spin both rides; independent exit preserves siblings. Four slide turns, three poses and photos pass. |
| 4 Hook a Flower and prizes | Technically verified | Native cast/rod/tether, shared queue, moving target input and one source-type prize per catch; duplicate/stale authority checks pass. |
| 5 Face painting | Updated layout passed594; final source check pending | Three touch contributors paint the original round stencil using six source colours and a cycling icon; reveal/persistence passed.596 also applies the source design detail to the recipient. |
| 6 Complete world presentation | Technical review complete | PC and phone/tablet-shaped views inspected; eight decoded clips, section music and source props packaged. Subjective motion/audio approval remains with owner. |
| 7 Shared play and recovery checks | Passed |592 activity assertions;593 four-client repair/reopen/late-join and Hulk/HQ visits;593 actual private reopen/automatic authority handoff/solo return. No runtime exceptions in final runs. |
| 8 Packaging and source delivery | Complete for PC |685 packages the current source and292 Funfair media files alongside Markets/Hulk/HQ. Four current clients preserve the earlier save, share direct wheel input, travel between worlds and reopen the authority. |

Next bounded task: **owner play/audio review of the checked PC preview. Physical-device rollout and installed-family updates remain separate, unperformed work.**


## Technical acceptance — October16

The [source inventory and adaptation record](peppa-funfair-source-inventory-2026-10-16.md) resolves the exact recovered pack and explains the family mechanics. Production uses the existing2D pipeline and PC authority. The separate teacup ride is added using recovered cabin art; it is not presented as a recovered publisher controller. The three-cabin wheel explicitly accommodates four children. Shared positive-spin cycles, projected slide curves, the2D fishing tether and bounded face surface are family adaptations. Mummy is not in the source slide cast and must select another available role for that activity; parents without a fairy spin use their source happy pose.

The first isolated Funfair source was schema59/content93/protocol3. After Drive integration and direct machine input, the layout candidate is schema60/content95/protocol3; final combined-world admission is still pending concurrent main integration. GameWorld migration, validation, cloning, dispatch, time advancement, disconnect/travel release, menu/input routing and snapshots are connected. Ride occupancy and booth edits clear on restore while designs, prize counts, slide/photo history and the completed photo's frozen role/pose remain. A cast change cannot rewrite an earlier photo.

| Evidence | Actual coverage |
| --- | --- |
| Native592: `LocalData/SharedGarden/59f7c04f7b4f4a78a303f3cea220040b/funfair/result.json` | Four native clients pass menu/cast, both shared rides, independent rider exit, Hook a Flower, three-person painting, four slide turns/photos, smoothies, cup drinking and balloons. The overall run remained failed because saved-client startup then threw; its passed activity assertions are retained separately. |
| Native593 repair: same family, `funfair-reopen-593/result.json` | Pass, zero runtime exceptions. Four fresh clients reopen the exact failed checkpoint, paint the actual recovered head, reveal the source reaction, disconnect/rejoin during a sibling's slide, close/reopen the authority and all four clients, preserve a photo across Peppa→Mummy selection, visit packaged Hulk/HQ and return. |
| Private593: `LocalData/FamilyLAN/9a81c377ebbd45f48f2a314cfc467098/peppa-funfair/result.json` | Actual private ride and process reopen; generated paired authority starts and the same real client joins automatically without importing private progress; explicit solo return restores its private result. Zero runtime exceptions. |
| Focused editor/.NET checks | Funfair ownership, common rides/queues, duplicate catches, concurrent painting revisions, handoff/drop, independent departure, deep snapshots, completed-photo retention and additive legacy migration pass. Unity JSON gate passes. Existing Hulk authority fixtures pass on this source. |
| Launcher593 | `LocalData/peppa-funfair-launcher-check.json`: four packaged clients remain ready and show four recovered characters; no research-directory art override. |

The observed saved-client exception came from clearing the completed photo subject while retaining its timestamp. Restore now preserves that history; presentation also tolerates older empty subjects. Photo role/pose are frozen at capture. Other repaired defects were missing face-atlas transforms, rig-origin face anchors, wheel-over-rider drawing, tall scenery cropping, generic-avatar flashes and crowded character-picker controls. Build593 showed a recovered head beneath translucent strokes; the owner-requested original UI audit below supersedes that interface with the shipped round stencil layout.

Release593's Windows client and dedicated server use nondevelopment release profiles with zero errors/warnings. The Funfair pack has289 files /71,473,497 bytes; actor manifest SHA256 `8e04587e290bc42f936c0b8a67d724e54a48db5cd278d917c185f8e8432225f0`. The same candidate also packages the checked unchanged Hulk568 and HQ574 media. Raw packages, publisher code, exports, generated media, credentials, local builds and saves remain ignored.

The checked entry point is [Play-PeppaFunfair.ps1](../../Tools/Launch/Play-PeppaFunfair.ps1), default two players, optional `-Players 1` through `-Players 4`. Its own saved family and preview records preserve the existing Hulk/HQ launcher families and installed authority. Technical acceptance does not claim physical phones/tablets, subjective audio listening, identical original3D physics/shaders, owner visual approval or installation/live-server publication. No official-app purchase state was changed.


### Owner gameplay/layout correction

Original Funfair gameplay footage and serialized UI were inspected after the owner follow-up. The [source inventory audit](peppa-funfair-source-inventory-2026-10-16.md#original-gameplay-and-button-layout-audit--owner-follow-up) records exact button positions, palette, callbacks, viewed timestamps, corrections and remaining family adaptations. The earlier593 PC evidence does not qualify this changed layout. Build594 passed the full four-native-client activity/reopen run with the corrected layout (`LocalData/SharedGarden/707de453520c498db7ca013f8c2e44e7/funfair/result.json`). Build595 then exposed a fast-flick defect in the new direct wheel gesture: its initial drag displacement was ignored.596 measures from pressPosition and is in verification. The same correction group queues fishing release across an in-flight aim, aligns the fisher with the pond edge, applies source face details and keeps automated pointer isolation out of visible preview windows. Final main delivery follows successful checks and safe reconciliation with the other active world chat.


#### Current layout verification checkpoint

596 completes the direct-gesture, common painting, late reconnect, full authority/four-client reopen and packaged Hulk/HQ run with zero runtime errors.597 completes the targeted face/held-cup depth captures and private/shared separation run. Reviewed captures show the source tiger details on the saved recipient and the cup in front of the body. The597 launcher attempt exposed a fourth-client protocol timeout during a2.9s wheel alpha-load stall; the local packed-mask correction is compiled/pixel-checked and awaiting a fresh native build. This is a remaining packaging check, not a passed launcher result. Main integration must preserve the owner’s subsequently committed Drive removal and concurrent Markets work. Existing593 launcher acceptance remains historical.


#### Verified launcher and current packaging

598 passes the same four-client packaged launcher that timed out in597. `LocalData/peppa-funfair-launcher-check.json` records the durable ready state for family27dd7b7d2ca041c39949114d82275c90. Actual wheel loads measured132/137/140/144ms with the packed hit mask; no runtime exception was logged. The native candidate also preserves the committed Drive removal. Its schema60/content96 reflects the changed action ordering; this is isolated preview evidence, not a live-family server/app rollout. Content-v6 contains292 files /73,934,054 bytes and the unchanged actor-manifest hash above. The remaining work is integration with the completed Markets source, additive old-fork save checks and one combined four-client check before main delivery.


### Combined final PC candidate685 — October16

The completed Markets commitcfa3d0d is merged, and the canceled Drive world remains removed. Current source is schema61/content97/protocol3: preserve Markets action39, retain the removed action38 hole and append Funfair40. The compatibility change is for the shared world state/action contract, not artwork. Any future installed-family update needs the project’s coordinated server/app policy; this task changes no installed authority or devices.

Both earlier isolated save shapes migrate additively. A Funfair60 checkpoint without Markets gains Markets while retaining its world ID, ride results and design/history. A Markets checkpoint without Funfair gains Funfair while retaining photos, completed work and pony rounds. Missing/empty new modules are allowed only before the combined format and when no saved player occupies the absent world; current-format loss and an occupied absent module remain invalid. Full-Core fixtures and native Unity JSON gates pass, as do both existing hero-world gates.

The first combined build684 stopped before producing binaries because ignored Markets media had not followed the Git merge. The exact570 receipt-matching images/audio and their available metadata were copied from the main checkout.685 then passes the media hash/import gate and both nondevelopment Windows release profiles with zero errors/warnings. Recovered media remains ignored; no private data or other chat’s source was copied over.

`LocalData/SharedGarden/707de453520c498db7ca013f8c2e44e7/funfair-integration-685/result.json` passes with zero runtime errors: four actual current clients reopen the earlier Funfair save and add Markets; pictured wheel tap/fast drag controls one shared ride; one person leaves for Markets while siblings finish; all four visit Markets, packaged Hulk and HQ and return with paint/photo/slide history intact; the real combined authority and all four clients close/reopen with both world states retained. The unchanged activity rules retain594/596 coverage;597 supplies the face/cup visual and private/shared separation evidence. This combined pass is scoped to changed state/admission/navigation and independent departure.

The pre-isolation root prototypes were preserved twice, with hashes, then22 unchanged task-owned files were moved into ignored `LocalData/Research/PeppaPig/world-tests/Funfair/root-files-before-integration/moved-from-root` to allow safe main integration. The pre-existing generated project version589 is retained in commitf7040e0; the fresh combined build intentionally advances it through the established Unity build API. Unrelated tracked/untracked work, local saves, media, signing, enrollment and other preview families remain preserved.


The final685 launcher also passes four-client durable readiness: `LocalData/peppa-funfair-launcher-check.json`, family411e32eb6d894f6b9f124c3274053364. It selects the packaged685 client/server and the292-file Funfair receipt. Visible play windows use normal mouse input; only hidden setup/check clients use the fixture. The native candidate is ready for source delivery and the separate owner play/audio review.


### Main checkout delivery

Checked source is integrated on main. The main checkout contains the exact2236 sealed685 artifact files, verified by SHA256 after copying from the isolated build. All2527 Unity inputs match the checked candidate:2405 byte-for-byte and122 text files differing only by Git newline/BOM normalization, with no other source/asset differences. `LocalData/peppa-funfair-artifact-copy.json` records the comparison. No tracked input was rewritten to manufacture byte equality.

The main-root configured launcher passes a fresh four-client durable-readiness run for generated family7e60bf1ba1964e029a97510304515fb8; see main-root `LocalData/peppa-funfair-launcher-check.json`. The selected current candidate is685 and its own separate saved preview family is ready for normal mouse-controlled play. Existing Hulk/HQ/Markets selections and the installed authority are preserved. Detailed earlier native evidence remains under `LocalData/Worktrees/PeppaFunfair/LocalData/SharedGarden` and `LocalData/Worktrees/PeppaFunfair/LocalData/FamilyLAN`; the isolated checkout is retained for these ignored builds/saves/evidence after branch cleanup.

Tracked source delivery and preserved untracked work are separate: only this task’s checked source, integration changes and records are committed. Unrelated pre-existing untracked review/source folders remain untouched. Source delivery is PC-qualified, not physical-phone/tablet installation, subjective audio approval or live-server deployment.
