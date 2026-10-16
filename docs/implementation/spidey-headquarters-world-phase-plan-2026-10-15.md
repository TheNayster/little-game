# Spidey HQ world phase plan

Created October 15, 2026. Goal IDs: WORLD-02 / FAMILY-01 / ITEM-02.

Build the second world the owner selected: **Spidey's headquarters**, following Hulk's birthday. The goal is a complete playable family adaptation of its activities, characters and actions in Little Weeps, for up to four players together. Preserve the existing 2D game and the finished Hulk world. This is the implementation plan; no Spidey HQ production implementation or runtime acceptance is claimed here.

## Evidence and scope

The recovered v23.1 package identifies `SpideyHQSingleBundle`, title **Spidey HQ**, scene GUID `ebf211a5142fa834a8a81dbee999ced2`. Its English localization describes pizza, balloons, a slippery slide, Green Goblin stealing pizza, teamwork by Spidey/Spin/Ghost Spider, distinct hero powers, building an HQ and jigsaw puzzles. The localization advertises six activities; that number does not establish six specific scene controllers or justify dropping smaller interactions.

Primary local evidence is under ignored `LocalData/Research/DuploMarvel/marvel-complete-hulk-project/ExportedProject/Assets/`:

- `Resources/playpacks/V1.0_SpideyHQSingleBundle_PlayPack.asset`: identity, original scene GUID, preview references and localization keys.
- `Resources/I2Languages.asset`, `SpideyHQ/` terms: the described play loops, cast and teamwork tips.
- `DuploMarvel/AddressableAssets/Remote/Prefabs/V1.0/`: HQ slide, poster and backdrop activities; globe, handheld camera, claw arm, newspaper and pink-block views/set pieces. Their existence establishes candidate objects, not complete interactions or placement.
- `DuploMarvel/AddressableAssets/Remote/ScriptableObjects/V1.2/BuildYourOwnSettings/SpideyHq_BYO_Settings.asset`: candidate building configuration requiring dependency inspection.
- `Scripts/Assembly-CSharp/StoryToys/deprecated/SpideyHQDirector.cs`: structural fields/methods mention civilians, boredom/sleep, benches, pizza, balloons and walking routes. Related globe/poster types expose effect/animation settings. These are empty method stubs in a deprecated namespace; active scene bindings and native behavior must be established before using them as current rules.

The later full Hulk export still contains only eight scenes and **does not contain the HQ scene**. It does contain more HQ-specific prefabs than the earlier recovery. The existing two-world native mapping receipt verifies selected method ranges, not complete HQ logic. Original C# gameplay bodies have not been recovered.

[Apple's game feature](https://apps.apple.com/us/mac/story/id1613533386) independently describes changing Spidey's picture, taking a photo with the friends, balloons and teamwork to recover stolen pizza. Its cat rescue and car wash sections describe other adventures; do not import those into HQ without scene evidence. The [current app listing](https://apps.apple.com/us/app/lego-duplo-marvel/id1573101225) supports the overall open-ended play premise, not exact HQ timings. Physical LEGO set descriptions are not executable-app specifications.

## Completion contract

Every recovered HQ activity needs a coverage row containing its source object/controller, input, eligible characters, approach/contact anchors, action animation, audio, intermediate states, finish/replay behavior, interruption and persistence. Label recovered behavior, family adaptation and unresolved behavior separately. Character gestures and object contact are part of completion; functioning buttons alone are insufficient.

Use one authoritative world and one progress state per shared activity. A round has a shared invitation/start and common progression, with independent exits that preserve siblings' play. Four players must be able to help together despite the source's three named heroes: family participants may control/helper-share hero actions through an explicit ownership rule. NPC support fills missing roles in solo or small groups. Never require a fourth child to watch or split the group into four personal rounds.

Keep PC/VPS authority, private offline saves and authoritative reconnect separation. No unlock/payment changes to the official app are part of this work. Research and reconstruction do not depend on asking the owner to purchase or open the locked world.

## Phase 0 — Recover the world and lock its activity inventory

Resolve the scene GUID through the retained addressables catalog and bundle/dependency indexes. First inspect existing local bundles; recover available missing content through established tooling and accessible publisher delivery paths. Record unavailable dependencies explicitly. Reuse the correct Unity 6000.0.58f2 export settings and existing REA/native workflow; do not reinstall tools or assume the Hulk dependency closure includes HQ.

Run the existing recovered-world inspector/reference resolver on the actual scene when available. Trace scene-bound activities, nested prefabs, actor casts, events, powers, prop ownership, puzzles, building settings, animation clips, sound maps and previews. Map native methods only for unresolved behavior that affects implementation, using independently verified ELF offsets. Do not substitute empty C# stubs for logic or infer live behavior from deprecated type names.

Deliver one bounded ignored `world-tests/SpideyHQ/` evidence inventory with hashes and missing references, and a concise tracked coverage table in this plan or a linked audit. Account for every advertised activity and incidental interaction. If the scene remains unavailable, retain the specific blocker and implement only explicitly supported adaptations after implementation is requested; do not silently replace recovery with observation-based guessing.

**Exit:** exact target and dependency coverage established; each activity has an evidence level, required actions and acceptance scenario. Newly discovered activities update later phases instead of disappearing from scope.

## Phase 1 — World foundation and character actions

Create world-owned rules, screens and resources under the existing Unity asset root's `Code/Core/Worlds/SpideyHQ`, `Code/Client/Worlds/SpideyHQ` and `Resources/Worlds/SpideyHQ`. Confirm existing ownership before creating folders; record consumers in `docs/project-layout.json`. Coordinate shared navigation, GameWorld/GameScreen, FamilySession and protocol dispatch deliberately.

Recover and adapt the HQ environment into the established 2D pipeline, retaining editable sources and asset provenance. Reuse Hulk's proven actor binding/rendering, anchor sampling, art inspection and package receipt methods where appropriate; keep birthday-specific activities out of the new world. Extract a shared helper only when both consumers actually need it.

Bind Spidey, Spin/Miles and Ghost Spider plus confirmed NPCs. Implement source-informed walking, idle, greetings, pickup/carry, use, eat, surprise, success and each verified hero power. Include facing, hand/foot/object alignment, approach routes, occlusion, interruption and return-to-idle. Preserve family-player identification and existing cast choices; selecting a family avatar must not remove access to required hero abilities.

**Exit:** navigable HQ, four distinct participants and a focused normal-speed action/contact review; no remaining placeholder action needed by the first activity.

## Phase 2 — Everyday HQ play and small interactions

Implement the confirmed pizza and balloon acquisition/use/release loops, slide entry/descent/landing, and applicable seating/civilian routines. Follow recovered ownership/queues, prop appearance, consumption, reactions and cleanup. Shared stations need fair access without resetting other participants when one leaves.

Implement the changing poster, photo interaction and globe response after their active bindings are confirmed. Inventory the claw arm, newspaper, pink block and backdrop objects individually: reproduce confirmed interactions and retain scenery where they are purely decorative. Reuse the existing local photo foundation only if its behavior fits; an in-world camera does not automatically require publishing images or a new album system.

**Exit:** every Phase 2 coverage row has a complete input-to-finish/replay loop, appropriate character action/audio and a targeted shared departure/interruption check.

## Phase 3 — Green Goblin and cooperative hero powers

Recover the actual theft trigger, target/route selection, hero powers, web/aim/contact rules, capture conditions, stolen-item recovery, reactions and reset. Do not copy Hulk's villain timer assumptions into HQ. Trace how the three source heroes contribute and distinguish confirmed powers from guesses.

Adapt the encounter into one shared event: common villain/item state, clear pictured help cues, bounded concurrent inputs, shared completion and visible return of the same stolen object. Support four human contributors and NPC assistance when fewer children play. Someone leaving, switching areas or disconnecting must release only their participation and allow the others to finish. Repeated/stale inputs must not duplicate pizza, villain instances or rewards.

**Exit:** actual four-client encounter from natural trigger through recovered item, replay and independent departure; separate solo/small-group check for missing-role assistance.

## Phase 4 — Build an HQ and solve the puzzles

Resolve the HQ-specific brick palette, inspiration image, bounds, snapping/rotation, removal/reset and any authored feedback. Adapt its virtual 3D brick play to the established 2D building presentation with clear depth/layer rules; document that adaptation. Preserve editable creations and object identities in the appropriate world save.

Recover the HQ jigsaw images, piece layouts, placement/snap rules, completion effects and replay selection. Do not ship generic replacement puzzles while marking the recovered ones complete. Provide a common puzzle with shared piece ownership and revision checks, shared completion and fair release on departure. All four can contribute to one construction or puzzle.

**Exit:** four-client contested-piece/build-edit check, shared completion/replay and saved creation/puzzle reopening. Exact persistent/transient boundaries follow the Phase 0 evidence and family design.

## Phase 5 — Finish world behavior and verify recovery

Close the coverage table, including background character life, secondary props, discoveries, audio and any Phase 0 additions. Review full views at PC and representative phone/tablet-shaped resolutions; inspect normal-speed actions and contact. Listen to the actual output before claiming subjective audio quality. Windows layout simulation does not qualify a physical device.

Run one focused current-source verification pass: compilation; a representative four-client mixed visit spanning the new activities, late join, independent area change/departure and reconnect; actual saved-family/process reopening; private solo and offline-to-shared separation. Test changed save/schema/admission contracts and one relevant Hulk regression when shared helpers changed. Use genuine normal-input interaction for acceptance; identify any diagnostic fixtures separately.

Record source/build hashes, content receipt, exact runs and failures beside the coverage table. Technical acceptance and owner visual/motion/audio acceptance remain distinct. Do not broaden into unrelated Zoo or organization qualification.

**Exit:** no uncovered required interaction or unresolved gameplay failure; current-source runtime evidence supports the declared scope and remaining owner/device limits are explicit.

## Phase 6 — Package and deliver the playable candidate

Package all required art/audio with a verified manifest; a source-only art pack is not a playable delivery. Provide a configured isolated saved-family PC launcher using the established preview pattern, supporting one to four players and preserving the Hulk preview and real family saves. Verify durable readiness, packaged content and launch behavior.

Update the main build guide, this coverage record and relevant decisions. Review/stage only owned files, integrate checked source into `main`, push and verify `origin/main`, then remove only a finished task branch if one was needed. Keep generated builds/research and private data ignored.

Read the current server rollout policy before any later deployment. New shared state requires an explicit compatibility/migration decision; do not preassign build/content numbers or replace the installed authority as part of source delivery. Phone installation and live-server rollout remain separate requested operations.

**Exit:** usable current-source PC candidate and launcher, verified Git delivery, concise evidence/limitations and owner review links.

## Current status and next task

Planning complete. Production phases 0–6 remain unimplemented/unaccepted for Spidey HQ. Today's bounded read confirmed the exact pack identity, local description, HQ-specific assets and the missing scene in the later export; it did not establish a complete dependency closure or original gameplay code.

Next implementation task: **Phase 0 only as the initial bounded work item**, followed by the evidence-derived dependency sequence above when implementation is requested. Keep one phase active at a time and update its actual acceptance before continuing. Hulk's [existing phase record](hulk-birthday-world-phase-plan-2026-10-15.md) remains authoritative for its own delivered world.
