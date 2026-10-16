# Spidey HQ world phase plan

Created October 15, 2026. Goal IDs: WORLD-02 / FAMILY-01 / ITEM-02.

Build the second world the owner selected: **Spidey's headquarters**, following Hulk's birthday. The goal is a complete playable family adaptation of its activities, characters and actions in Little Weeps, for up to four players together. Preserve the existing 2D game and the finished Hulk world. Phase 0 static recovery/inventory is complete as recorded below; no Spidey HQ production gameplay or runtime acceptance is claimed here.

## Evidence and scope

The recovered v23.1 package identifies `SpideyHQSingleBundle`, title **Spidey HQ**, scene GUID `ebf211a5142fa834a8a81dbee999ced2`. Its English localization describes pizza, balloons, a slippery slide, Green Goblin stealing pizza, teamwork by Spidey/Spin/Ghost Spider, distinct hero powers, building an HQ and jigsaw puzzles. The localization advertises six activities; that number does not establish six specific scene controllers or justify dropping smaller interactions.

Primary local evidence is under ignored `LocalData/Research/DuploMarvel/marvel-complete-hulk-project/ExportedProject/Assets/`:

- `Resources/playpacks/V1.0_SpideyHQSingleBundle_PlayPack.asset`: identity, original scene GUID, preview references and localization keys.
- `Resources/I2Languages.asset`, `SpideyHQ/` terms: the described play loops, cast and teamwork tips.
- `DuploMarvel/AddressableAssets/Remote/Prefabs/V1.0/`: HQ slide, poster and backdrop activities; globe, handheld camera, claw arm, newspaper and pink-block views/set pieces. Their existence establishes candidate objects, not complete interactions or placement.
- `DuploMarvel/AddressableAssets/Remote/ScriptableObjects/V1.2/BuildYourOwnSettings/SpideyHq_BYO_Settings.asset`: candidate building configuration requiring dependency inspection.
- `Scripts/Assembly-CSharp/StoryToys/deprecated/SpideyHQDirector.cs`: structural fields/methods mention civilians, boredom/sleep, benches, pizza, balloons and walking routes. Related globe/poster types expose effect/animation settings. These are empty method stubs in a deprecated namespace; active scene bindings and native behavior must be established before using them as current rules.

At planning time, the later full Hulk export contained only eight scenes and lacked HQ. Phase 0 subsequently recovered the exact scene into a separate research export; see the checkpoint below. Native mapping receipts verify selected method ranges, not complete HQ logic. Original C# gameplay bodies have not been recovered.

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

The owner authorized all phases. Technical implementation and packaged PC acceptance are recorded in the final section below; the dated Phase 0 checkpoint preserves its original static-only evidence. Owner play/motion/audio acceptance and physical-device rollout remain separate. Hulk's [existing phase record](hulk-birthday-world-phase-plan-2026-10-15.md) remains authoritative for its own delivered world.

## Phase 0 recovery checkpoint October 15

The pack's scene GUID resolves to `Assets/DuploMarvel/PlayPacks/SpideyAndFriends/Scenes/saf_Park.unity`. Its catalog dependency closure contains 299 locations and 226 remote bundles, with no missing catalog keys. Reused 224 existing Hulk-research bundles and downloaded the two missing SpideyAndFriends bundles from their retained publisher catalog URLs: 309,910 bytes in total. All 226 remote bundles now have local UnityFS files and SHA256 identities; no phone, purchase, official-app unlock or account change was needed.

A separate task-owned AssetRipper session exported `LocalData/Research/DuploMarvel/marvel-complete-spidey-project/ExportedProject` using Unity fallback 6000.0.58f2. The exporter reported finished assets/assemblies/post-export and the task-owned process was then closed. The earlier recovery session and production Unity project were preserved.

The read-only scene scan found 10,387 serialized objects, 2,813 GameObjects, 2,735 script components, 14 actor components and 30 activity roots. All 30 static activity closures completed within the configured bound; 1,374 reachable files were indexed, including 186 animation clips and seven animation controllers. There were no parse errors, duplicate meta GUIDs, exporter placeholders or missing-reference statuses. The one unresolved script is an explicitly disabled component with a null script. Of the inspected references, 4,292 target files have unverified imported subobject identities; that category is not a missing file and still requires import/runtime qualification.

The actor cast is Spiderman, Spiderman_Morales, Spiderman_Gwen, GreenGoblin, PizzaGuy, BalloonSeller and eight civilians: LavalliereLady, BlueBlazerLady, BlueJacketBoy, SmileyJacketBoy, GroovyGranny, BlueBlazerGuy, DungareesGirl and HeartsBlouseGirl. Hero actors contain 58/60/57 attached script components respectively. Their scene-bound legacy `ActorPropHandler` peg mappings differ from Hulk's newer inventory activity; retain that distinction when adapting shared item rules. Each hero has a layered animator binding plus hand-specific web projectile/holding components. Spin/Morales has the active invisibility component; do not assign that power to Ghost Spider from name-based assumptions.

The earlier 192 HQ-related native ranges were rechecked against current original-binary bytes. A further focused controller selection produced 1,227 independently mapped ranges, all fully decoded by Capstone. Selection includes shared classes, nested types and similarly named controller versions; it is not a count of HQ-exclusive methods. The producer attempted 1,239 methods with a 120-method-per-type cap, so absence from this extraction is not absence from the app. These are native instructions and structural evidence, not recovered original C# or a completed semantic reconstruction.

### Activity coverage and implementation acceptance

IDs below are exported component file IDs in `saf_Park.unity`. This historical table defines the recovered scope; current implementation and runtime acceptance supersede its original pending status in the final section below. Family adaptations apply the four-player completion contract above. Exact runtime timings, trigger priorities and interruption transitions must be traced at the affected implementation phase; serialized numbers alone do not establish their semantics.

| Scope | Source bindings and recovered actions | Required implementation acceptance |
| --- | --- | --- |
| Hero arrival and free movement | New intro 8586; WalkOnTouch, dragging, pickup, back-to-screen and five back-to-visible roots; actor-specific layered animators | Arrival, walking/facing, pickup/drag/contact and safe interruptions; four recognizable participants with working hero-role access |
| Pizza stall | VendingStallActivity 9392, queue 9508, seller 9974, counter/spawn pegs 8943/9873, original pizza prefab | Shared queue, visible seller handoff, one held pizza, eating/finish/replay and independent departure |
| Balloons and floating | VendingStallActivity 9393, queue 9507, seller 9543; FloatWithPropsActivity 8597 and SlowFallWithPropActivity 8573 | Balloon pickup, hand bindings, floating/bobbing, release and slow landing; no stuck floating actor or duplicated prop after exit |
| Slide | SlideActivity 8557, queue 10164, SlideComponent and SlideAnimation | Approach, queue, slide contact/descent and landing; one child leaving preserves the others' turn |
| Civilian life and seating | Enabled SpideyHQDirector 8571 despite deprecated namespace; six OnTapMoveToPegActivity seating roots, idle/flee roots | Walking, boredom, sleeping/waking, seating, pizza/balloon interaction and villain reactions; interrupt safely for direct play |
| Poster | SpideyHqPosterInteraction 9111; three original poster textures, scaler, particle and PosterTap sound event | Change the actual picture with flip/fade, sound and effect; shared selection persists according to the chosen world-save contract |
| Globe | SpideyHqGlobeInteraction 8577; three dots, tap animator and GlobeTap event | Complete tap/dot/reaction loop with repeated input handled; preserve bindings and source effect timing after native trace |
| Camera | CameraStandActivity 10079; touch 9586, peg collection 9737, photo display 9326, take-photo behavior descriptor | Hero positioning, camera handling, actual pictured result and feedback; closing/leaving does not disrupt siblings |
| Domino knock-over | DominoeEffectActivity 9381; epicenter 3606, range field 1.4, balance/fall descriptors | Confirm triggering path, then reproduce nearby balance/fall/recovery reactions and safe replay |
| Goblin theft and web fight | GreenGoblinActivity 10042 and recovery 10330; pizza and balloon theft refs, five battle centers and three hero finish positions per center | Natural theft of either item, hero contribution, web contact/hold/pull, same-item recovery, celebration and smoke/vanish/replay; four players and NPC role fallback |
| Hero powers | SpideySenseController 8558, three web shooting/pull/hold/thwip bindings; Spin InvisibilityBehaviour 10236 | Actual actor-specific senses/webs/invisibility, facing and hand contact; correct activation/cleanup when leaving or an encounter ends |
| Build an HQ | BuildYourOwnController 8746; HQ settings, six base plates, eight brick categories containing 26 variants | Shared snapping/placement/removal/reset and saved creations; retain HQ-specific palette/poster/globe/newspaper bricks and explain 2D adaptation |
| Jigsaws | JigsawController 10270, popup 9848 and billboard; five original art images, two masks, four difficulty fields valued 2/3/4/5 | Recover actual piece-count interpretation, shared selection/drag/snap/completion/replay and independent exits; no generic substitute artwork |
| Remaining set pieces and audio | Full scene configuration, reachable dependencies and 30 closures retain backdrop, claw-arm, newspaper, pink-block, invitation, animation and sound bindings | Distinguish interactive objects from scenery, close every confirmed interaction and inspect complete world presentation at normal speed |

The Goblin scene configuration supports both stolen item types, five battle centers, cooldown pairs 8–32 and 80–150, pull duration 3 and recovery celebration duration 8. Treat these as field values until their native use is followed. The original three-position finish formations need an explicit four-player family adaptation; do not hide or remove the fourth participant. The building and puzzle systems are scene controllers outside the default `Activity` suffix inventory and are included explicitly in the coverage above.

### Retained evidence and checks

Ignored evidence root: `LocalData/Research/DuploMarvel/world-tests/SpideyHQ/Phase0/`. It contains `catalog-local-closure.json`, `spidey-download-receipt.json`, export/load receipts and logs, `Inventory/` reference/actor/activity/closure/configuration/hash files, `binding-summary.json`, `existing-native-coverage.json` and `native-verified/independent-verification.json`. The recovered scene SHA256 is `3b428b542b5d160808c5dba39c2a3301bd08925c89fe6fd00553c68a324ce7e3`.

The reusable [catalog inspector](../../Tools/Verification/Inspect-RecoveredAddressableCatalog.py) resolves the original GUID, detects alias cycles and records local bundle hashes without changing inputs. The [world inspector](../../Tools/Verification/Inspect-RecoveredUnityWorld.py) now accepts extra activity-type patterns and records legacy prop handlers while retaining its existing default activity selection. Both ran against the recovered HQ inputs; the scene hash remained unchanged. Native mapping and byte/decode checks passed. This checkpoint establishes static recovery/inventory acceptance only: no new production game build, visual/audio approval, device installation or live-server rollout.


## Technical delivery October 15

The owner authorized completing all phases. The resulting **family adaptation** retains the recovered HQ setting, cast, pictures, props and actions, with shared four-player rules in the established 2D game. This is reconstructed code, not the publisher's original C# or a one-to-one simulation of its 3D engine.

| Phase | Delivered scope and technical acceptance |
| --- | --- |
| 0 | Exact scene/dependency/actor/native inventory above; source files retained and hashed. |
| 1 | HQ world card, authoritative state, additive schema54→55 migration, spaced four-person arrival, role choices, original character strips and hand tracks. Native four-player entry and current editor rules pass. |
| 2 | One pizza queue and balloon queue, seller serving, one held object, eating, floating/slow fall, six source seat pegs, sleep/wake, poster flip, globe dots, actual pictured camera result and nearby tumble. Native queue/item/station controls and interruption checks pass. |
| 3 | One natural theft of pizza or balloon, same-object return, shared web/pull/recovery, correct Spin invisibility and Spidey sense. Four human contributions and private missing-role NPC assistance checked. Explicit Leave/area departure removes only that participant; all declining ends without a reward. |
| 4 | Original26-variant palette, shared placement/rotation/removal, per-brick edit ownership and saved IDs. Five original pictures with4/9/16/25 pieces, common board, real drag/snap plus tap fallback, revision/ownership checks and replay. Four-client completion and contested-edit rules pass. |
| 5 | Source-informed civilian movement/food/balloon/seats, hand-specific carry poses, recovered soundtrack/effects, ambient network coalescing, saved authority/private reopening and private→shared separation. Native PC and phone/tablet-shaped full views inspected; subjective owner audio/motion acceptance remains separate. |
| 6 | Packaged574 Windows client/server, isolated saved-family launcher for1–4 players, hash receipts, maintained guide/decision/layout records and main delivery. Launcher verifies durable readiness and source characters from packaged media. |

### Explicit family adaptations and limits

The original single-player3D interaction model becomes pictured approach buttons, optional puzzle dragging, source-projected2D characters and a common authoritative activity. The fourth participant is a recovered civilian helper with web contribution; absent hero roles are NPC-assisted. Fourth queue slots extend the third authored segment. The slide follows seven projected source path points over the serialized1.25seconds; stall service uses the recovered17/12second serve clip. Six seat coordinates come from actual serialized peg transforms.

The native JigsawController CreatePuzzle method loads its difficulty at offset0xb0 and squares it (`0x039CB1FC–0x039CB21C`), supporting4/9/16/25 pieces. Our pieces use rectangular crops and fixed shared targets with a.35-cell snap tolerance, rather than the source's shaped masks and freely merged groups. Building uses a2D snap mat rather than3D stacking. Goblin uses one common encounter center and a four-person formation, rather than cycling all five original3D battle centers. Source3D particle effects, autonomous-director priorities and physics are represented by the reconstructed2D actions/feedback; exact particle choreography and original AI are not claimed. Camera results are an actual local world picture, not a persistent shared album. Balloon/ground objects return to stock after60seconds unused so unattended props cannot exhaust play.

The legacy HQ animator uses `isHoldingItem`, `walkSpeed` and the left balloon parameter; Hulk's newer hold parameters do not apply. The bake now has526 action strips,24 prop views, zero missing requested clips, zero bake exceptions and no missing-parameter warnings. Content-v4 contains601 files /68,169,496bytes,34 pictures and9PCM audio files. Its actor manifest SHA256 is `432936488cad18d0a68c896000a27e9878c7dccc0a8819f219d6cba339a15952`. Recovered media stays ignored; it is packaged locally, not committed. The unchanged Hulk pack is986files /137,244,911bytes with manifest `0976e6ad29848178ccfce2b3409a906b6a447ef74ffd172d316bb42198242a9e`.

### Evidence, failures and reuse

- Full standard569 prebuild gate and focused572/573/574 HQ/Hulk rules ran under Unity6000.3.24f1. Final574 client/server release builds have zero errors/warnings. The optional SDK harness hit Windows Application Control; Unity's actual editor rule checks passed without changing security.
- Native572 activity run: `LocalData/SharedGarden/7c3df04a57d94cba852b095e889ec8a7/spidey-hq/result.json`. Eight activity groups passed, including four-person queue, natural web recovery, real puzzle drag and shared completion, building, poster/globe/photo, balloon/slide/seat/sleep/powers and independent departure/return. Phone/tablet-shaped captures show the actual native UI. Its overall result remains failed because saved-HQ startup exposed an initialization error; it is not relabeled passed.
- Earlier569 exposed per-frame snapshot flooding, corrected with ambient coalescing;570 exposed camera-moving test taps;571 exposed transparent prop hitboxes/overlapping globe controls. Failed evidence remains retained at the checkpoint paths in repository history and LocalData.573's first startup guard exposed an uninitialized position lookup;574 guards both creation and position readiness.
- The final source change from572 to574 is limited to the saved-HQ depth initialization guards and build version; accepted activity evidence is reused for unchanged paths. Final574 saved-world/four-client catch-up and private-mode runs are recorded below. No new full unrelated world qualification is claimed.

- Final574 saved HQ/four-client reopening and disconnect/rejoin: LocalData/SharedGarden/7c3df04a57d94cba852b095e889ec8a7/spidey-hq-reopen-574/result.json — passed, zero runtime exceptions.\n- Final574 private NPC-assisted recovery, brick creation, actual private process reopening and automatic authority handoff/explicit solo return: LocalData/FamilyLAN/85545ec4a6a34baa98e80780672fd2d8/spidey-hq/result.json — passed, zero runtime exceptions; private progress never entered the authority.\n- Packaged574 two-player launcher: LocalData/spidey-hq-launcher-check.json, disposable run 11e602c479ef45f4a587ede539bbec1b — passed durable readiness/source visibility; all task-owned test processes closed. Final source check matches all2,478 Unity files in the574 source manifest. Packaging receipts are Builds/NetworkProbe/G3-0.0.574/SpideyHQ-content-receipt.json and HulkBirthday-content-receipt.json.

Use [Play-SpideyHQ.ps1](../../Tools/Launch/Play-SpideyHQ.ps1) (`-Players 1` through `-Players 4`, default2), backed by its own ignored `LocalData/spidey-hq-preview.json` and saved preview family. D3D11 avoids the previously recorded DX12 startup issue; this task does not claim to fix DX12. Windows phone/tablet-shaped captures are layout simulations. Physical devices, owner play/motion/audio approval and a coordinated update of the installed authority/apps are unperformed and separate. Real family saves, enrollment and the existing Hulk preview selection are preserved.
