# Hulk birthday world phase plan

Created October 15, 2026. Goal: a fully functioning Hulk birthday world in Little Weeps, with recognizable character actions, usable activities and up to four family players playing together. The current recovery viewer is a research baseline; it does not satisfy this goal.

The target is **HulksBirthdayParty**, displayed in the installed app as **Spidey & His Amazing Friends: Hulk’s Birthday**. It is distinct from the older **Hulk Smash** pack. Spidey HQ remains a separate recovery task and is not a dependency of this world.

Related project goals: WORLD-02, FAMILY-01 and ITEM-02. Follow [current decisions](../current-decisions.md), the [creation workflow](../ai/GAME_CREATION_WORKFLOW.md), [folder ownership](../project-folder-guide.md) and [server update policy](../server-update-policy.md). The owner subsequently authorized implementation and explicitly requested continuing through the phases until finished. Continue the implementation; an inventory checkpoint is not the requested outcome. Device installation and live-server replacement remain separate from source implementation.

## Research basis

### Online findings

The publisher’s App Store history describes a birthday setting with gifts, cakes, a band, a bouncy castle and villains attempting to take the gifts. These are required content areas. [StoryToys App Store listing and version history](https://apps.apple.com/us/app/lego-duplo-marvel/id1573101225).

StoryToys describes its play packs as open-ended pretend play and discovery with problem solving. Preserve the ability to wander, repeat activities and change interests; do not turn the birthday into a mandatory linear campaign. [Official game description](https://storytoys.com/apps/lego-duplo-marvel/).

Google Play also describes building and exploration, and says previously downloaded content can work offline. For Little Weeps this informs local asset availability; its private offline saves and authoritative shared session remain separate under existing project rules. The store description does not establish a four-player network mode in the original app. Cooperative multiplayer here is a Little Weeps adaptation. [Official Android listing](https://play.google.com/store/apps/details?id=com.storytoys.lego.duplo.marvel.spiderman.avengers.kids.preschool.free.googleplay&hl=en).

Unity’s documentation separates clips from the controller state machine and describes animation events for timed actions. Recovering a clip is therefore only one part of matching an action: transitions, attachments, contact moments and callbacks must also work. [Animator Controllers](https://docs.unity3d.com/6000.0/Documentation/Manual/class-AnimatorController.html), [animation events](https://docs.unity3d.com/6000.0/Documentation/Manual/AnimationEventsOnImportedClips.html).

Sources checked October 15, 2026. No gameplay video was reviewed for this plan. Online descriptions establish expected features; package data and traced native methods remain the primary basis for reconstructing behavior. Online footage, if later useful, can check presentation without replacing the requested reverse-engineering work.

### Installed game findings

The inspected Android package is StoryToys’ installed Google Play version 23.1, code 2196. Its Unity version is 6000.0.58f2. The separate recovery viewer uses Unity 6000.3.24f1.

| Evidence | Established result | Limit |
| --- | --- | --- |
| Original HBP_Main scene | 15,801 objects and 9,620 serialized script components | Many services and external references still require reconstruction |
| Recovery viewer | 4,442 renderer colour records applied; entrance and stage render with replacement shaders | This is a derived scene, not the original running game |
| Native method extraction | 343 selected Hulk and Spidey method ranges independently mapped and decoded | Decoding is not full semantic recovery or original C# source |
| Cake controller | Three input mappings; selected sprite is assigned to its renderer | The three actual cake sprites are unresolved external references; current icons are substitutes |
| Band controller | Three track mappings, battery state, electric/acoustic sources and celebration events | Exact combination and interruption rules still need tracing |
| Bouncy castle | Entry/exit points, bounce parameters, bounds, animation descriptors and deformation indices | Its native update/cleanup logic has not yet been reconstructed |
| Character and quest wiring | Intro, gifts, reactions, battery handling, dancing, web actions, food/drink and actor spawning are present | A component name alone does not prove the complete behavior |
| Current player build | Builds with zero errors and 245 warnings; viewer and cake input inspected | Warning causes require disposition; no full-world or multiplayer acceptance |

The extracted localization source independently confirms the birthday pack’s identity and themes: gifts for Hulk, party food, music and designing a cake. It is in `Assets/Resources/I2Languages.asset`, under `HulksParty/`, inside the original exported project. Do not confuse those entries with `HulkSmash/`.

Local evidence remains under ignored `LocalData/Research/DuploMarvel/`:

- `world-tests/current-recovery-status.txt`: current results, caveats and viewer receipt.
- `world-tests/HulkBirthday/components.json` and `scene-summary.json`: serialized component wiring.
- `world-tests/HulkBirthday/planning-activity-inventory.json`: activity objects selected during this planning task.
- `two-world-native/`: corrected native bytes, disassembly and independent verification.
- `marvel-cpp2il-structure/`: type/field information with empty method bodies.
- `marvel-version-fixed-project/ExportedProject/`: preserved original export.
- `world-tests/UnitySceneProbe/`: editable derived viewer and handwritten reconstruction code.

Keep recovered third-party files and generated builds in ignored research storage. Commit maintained plans and our implementation through the normal project workflow. Preserve the original export and its identifiers for comparison.

## What fully functioning means

Children can enter the birthday world, recognize Hulk and the confirmed cast, move and interact with them, give and open gifts, create a cake, play music, dance, bounce, deal with villains and continue free play. Actions must visibly connect to their effects: a hand holds the gift, a character reaches its destination, an instrument is actually played and a jump lands correctly.

Four connected family players share one world, one NPC cast, the same props and common activity state. They can help each other, join an ongoing activity and leave independently. Activities with rounds use a common start and shared progress; continuous free play does not gain an unnecessary readiness lobby. Four isolated solo games do not meet this goal.

Each required interaction needs a source reference, reconstructed rules, working character presentation and a targeted play check. Missing details remain open work. Replacement visuals may differ from the original, but must preserve the interaction and remain clearly identified until reviewed. A substitute cake icon or decorative stage cannot close the corresponding feature.

## Feature coverage

“Confirmed” below describes evidence for scope, not implementation completion. Scene names and field values require linked code tracing before exact behavior is claimed.

| Required area | Evidence and work to cover | Phase |
| --- | --- | --- |
| World entry and introduction | HBPIntroActivity, camera path, actor placements, held props and intro-complete event | 1–3 |
| Cast and basic actions | Actor references, rigs/controllers, walking, turning, idle, picking up, carrying, releasing and interrupted actions | 2–3 |
| Gifts and Hulk reactions | ProvideGiftActivity, table placement, open/unveil, positive/negative reactions, bad/fake gifts and cleanup | 3–4, 7 |
| Cake creation | CakeStickerMapController, StickerController, sticker surfaces and source cake assets | 4 |
| Food and drink | NewEatActivity, NewDrinkActivity, vendor/patron queue and linked props | 4 |
| Band | GuitarActivity, KeyboardActivity, DrumsActivity, three track mappings and actor participation | 5 |
| Power and dance floor | Battery movement/drop, electric/acoustic sources, dance actions, light groups and music events | 5, 7 |
| Bouncy castle | BouncyCastleActivity; enter, bounce, drag/tap, stunt descriptors, bounds, deform and leave | 6 |
| Building and small interactions | BapByoController, peg-based activity, source structures and all reachable tappable objects | 0, 6 |
| Villain behavior | Three HBPVillainControllers; gift theft, hiding/running, trapping, escape and recovery | 7 |
| Hero special actions | Scene-linked web/throw/smash/leap behaviors that participate in birthday activities | 2, 7 |
| Guests and world life | ActorSpawnerController, invitation, arrival/departure, limits and background reactions | 3, 8 |
| Shared play and persistence | Little Weeps adaptation: authority, possession, common progress, reconnect and retained world state | 1, every activity, 8 |
| Finished presentation | Camera, sound, touch/mouse controls, transitions, feedback and family play review | Every phase, 8–9 |

## Phases and completion checks

### Phase 0 Complete the evidence and dependency inventory

**Status: in progress.** The October 15 inventory checkpoint below establishes the scene cast, serialized activity graph and missing-reference identities. Dynamic guest coverage and complete action semantics remain open.

Resolve each reachable activity into its actors, props, triggers, actions, clips, sounds and cleanup paths. Traverse linked quests and behavior descriptors; include small tappables rather than relying only on named headline activities. Map script GUIDs and missing external assets to catalog keys and bundle dependencies. Classify each as recovered, recoverable, replacement required or unresolved. The apparent 65-component colour gap is now explained below; conflicting duplicate colour records still require deliberate handling.

Extend native analysis only for the next relevant controllers, beginning with character/prop services and the bouncy castle update path. Keep the verified ELF address correction; do not use the known incorrect Cpp2IL printed instruction bodies. For every behavioral claim distinguish serialized wiring, traced native semantics and proposed multiplayer adaptation.

**Done when:** every feature above has a mapped owner and dependency list, the source-confirmed cast and action matrix are listed, missing content has an explicit resolution path, and unknowns cannot disappear behind placeholder methods. This inventory determines the final small-interaction count; no unsupported “all activities recovered” claim.

### Phase 1 Establish the world and shared runtime foundation

**Status: entrance/actor/prop slice accepted on Windows539.** Four real release clients passed shared entry, walk-to-pickup, competing claims, human handoff, actual tap-to-walk carrying, independent disconnect, rendered ownership and reuse. The full party activities remain required. Mobile packaging and full-world persistence are later checks.

**Depends on: 0.** Build a small Little Weeps integration slice with the recovered entrance, one actual actor and one grabbable prop. Retain the existing 2D production pipeline. Evaluate rendering recovered content into layers and animation frames, with correct depth, hit targets and attachment anchors. The 3D recovery viewer remains a reference tool; it does not authorize converting the game’s production architecture to 3D. Record any concrete fidelity gap before expanding the asset pipeline.

Create the world’s shared state, stable actor/prop IDs, validated commands, participation, reservations and save versioning through existing GameWorld/GameScreen and session patterns. The PC/VPS owns shared rules and clocks. Keep camera framing local. Map family profile identities to the confirmed playable roles without assuming every source NPC is player-controllable or spawning a birthday Hulk separately for each client.

**Done when:** four actual clients see the same actor and prop, one player’s claim is visible to the others, conflicting claims resolve once, and leaving releases only that player’s participation. Existing areas and private offline saves remain intact. This establishes the foundation; it does not close later activity acceptance.

### Phase 2 Recover character animation and action behavior

**Status: shared locomotion/holding and handoff reactions accepted Windows542–544.** Activity-specific action rows are checked with their consuming phase; full-cast action coverage and owner normal-speed review remain open.

**Depends on: 0–1.** Reconnect recovered meshes/rigs, clips, controllers, masks, transitions, animation events and attachment points in the isolated reference project. Reconstruct missing behavior code from the mapped services and native evidence. Export the chosen compatible presentation into Little Weeps while preserving editable sources.

Build an action matrix per confirmed actor: idle/look, walk/turn, pick up/carry/put down, give/receive, celebrate/disappointed, jump/land, and the source-linked special actions needed later. Separate a reusable movement/holding action from a character-specific smash, throw, web action or stunt. Check normal-speed motion, hand contact, feet, pivots, facing, depth and cancellation. Visual callbacks must not cause duplicate authoritative effects.

**Done when:** Hulk and the first source-confirmed companion perform an end-to-end walk → pick up → carry → give/release → react sequence visibly and correctly. The remaining required cast/action rows must be completed before whole-world acceptance. A clip preview, static pose or sliding sprite is insufficient.

### Phase 3 Restore introduction guests and shared props

**Status: shared introduction, civilian arrival/delivery/departure and NPC exchange slice accepted Windows544.** Later activity arbitration remains with its consuming phase.

**Depends on: 1–2.** Reconstruct the introduction, invitations, guest arrival/departure, prop anchors, snap pegs and activity arbitration. Trace spawner limits and cleanup rules from source data; guest turnover must not duplicate players or discard an owned gift. Late arrivals join the current shared visit; they do not restart everyone’s introduction.

Implement gift provision, carrying, handoff and table placement with visible character actions. Resolve simultaneous claims using the shared authority. Activity transitions must release reservations and safely handle held props.

**Done when:** players can enter together, bring and place gifts, hand one to another participant, and leave independently while the party continues. Guests arrive and leave without permanently consuming actor or prop capacity. The introductory sequence hands control back to open play.

### Phase 4 Complete gifts cake food and drink

**Status: shared cake/consumption accepted Windows546 and gift replay/cancellation accepted Windows548. Source squirrel-stall queue remains in progress.** No full-world completion claimed.

**Depends on: 2–3.** Trace the whole gift sequence: ordinary and source-confirmed bad/fake gifts, reveal, Hulk’s response, consumed/destroyed wrappers and repeat play. Avoid guessing that every “negative” reaction is the same animation or outcome.

Replace the camera-overlay cake proof with the real creation flow: source-confirmed base choices, sticker/decor placement, input surfaces, finished cake and the linked party interaction. Resolve the missing original cake references or make complete replacement assets for those roles. The three recovered mappings establish three choices at that controller; they do not prove the full decoration workflow. Restore food/drink acquisition, queueing, holding, consumption and cleanup from the linked scene logic.

**Done when:** a family can make and use a cake and give/open gifts with readable character reactions. Two players acting on the same object cannot duplicate it, corrupt the decoration state or strand the others. Food/drink can be acquired and used again after an interrupted interaction. Cake icon switching alone does not pass.

### Phase 5 Complete the band power and dancing

**Depends on: 2–4.** Reconstruct guitar, keyboard and drums, instrument claims, character placement/playing, track eligibility, battery handling, electric/acoustic behavior, celebration and return to world music. Trace the exact conditions in HbpBandActivity; do not infer them from field names or the partially decoded boolean callbacks.

Use one shared musical timeline and authoritative participation. Clients derive local playback from shared state; late joiners align to it without starting another song. The fourth family player can contribute through a compatible source-confirmed role such as power or dance while the three instruments are occupied. Restore dance/light behavior with clear on/off and departure cleanup. Electro’s interference is completed in Phase 7.

**Done when:** instrument contributions visibly and audibly work together, the traced power transitions work, the dance floor reacts correctly, and one player leaving does not restart the song for everyone. Check actual listening separately from technical playback and synchronization assertions.

### Phase 6 Complete the bouncy castle building and remaining play

**Depends on: 2–4.** Trace BouncyCastleActivity’s update, input, entry/exit and cleanup methods. Use the recovered bounds and physics fields as inputs; do not translate Unity scene units directly into Little Weeps screen units. Match the intended bounce feel, wall/floor response, available stunts, character animation and castle deformation.

Restore source-confirmed building/peg interactions and finish the Phase 0 list of minor discoveries and tappables. An object is complete only when its intended reaction, repeat use and cleanup work. Building scope must follow the birthday pack’s actual dependencies rather than importing the separate Build & Play product wholesale.

**Done when:** four players can participate in one castle activity with independent entry/exit and no trapped actor, lost held prop or personal timer. Building and each inventoried minor interaction have a focused check. Stop spawning extra guests or objects at the verified capacity.

### Phase 7 Complete villains hero responses and recovery

**Depends on: 3–6.** Reconstruct gift theft/hiding/running, fake-gift paths, battery interference, hero responses, trapping, escaping and restoration of normal party play. Trace filters, priorities and trigger conditions before using the serialized controller intervals of 15–15, 60–90 and 30–45 seconds. Those intervals are not proof that all three timers run unconditionally or that they form a fixed schedule.

Connect the required smash, web, leap and throw actions with correct actor/prop contact and reactions. Keep one shared villain event and one set of affected objects. Players can help the same recovery; one leaving must not abandon a gift inside an unreachable actor state.

**Done when:** each confirmed villain loop can start, be interrupted, resolve and repeat without lost gifts, duplicate rewards, stuck cast or a forced world reset. Demonstrate a cooperative recovery and the confirmed Hulk happy/disappointed reactions in normal play.

### Phase 8 Verify the complete shared world and retained progress

**Depends on: all prior phases.** Run a representative four-client family visit covering introduction, gifts, cake, band/dance, bouncy castle and a villain event. Include different players in different areas, joining an active activity, independent departure and reconnect. Retest only affected paths after fixes rather than replaying every unrelated world.

Specify saved versus transient state: persistent creations and prop ownership/progress must survive the supported save/reopen path; temporary poses, guest reservations and timers need safe restoration or cancellation. Reconnecting loads the authoritative world and does not upload private offline changes. Test the affected schema migration with a copied existing save.

**Done when:** the coverage matrix has no unimplemented required row, no placeholder action passing as finished, and a real four-player visit can move freely between working activities. Retain a playable current-source candidate and concise evidence with source/build identity. Unresolved reconstruction warnings affecting behavior or assets keep the relevant feature open.

### Phase 9 Finish presentation and prepare delivery

**Depends on: 8.** Inspect the complete world at relevant PC, phone and tablet layouts: character readability, movement, hit targets, drag/tap alternatives, occlusion, transitions, sound and pause/resume behavior. Use the weakest currently supported device as a real performance check when deployment is authorized. Measure the recovered asset and animation cost; optimize only demonstrated problems.

Integrate checked source into main and push under the existing branch lifecycle. Record shared-content/schema changes and prepare compatible client/server artifacts through the established build flow. Re-read the server policy before any rollout. This plan does not itself install an app or replace the live server.

**Done when:** the family can play the whole world and the owner has reviewed the visible character actions and activities; technical checks, owner review and any separately requested deployment have individual records. Do not describe a source-only candidate as installed or declare the world complete while a required feature remains absent.

## Work order and records

Sequence: **0 → 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9**. Resolve cross-phase dependencies in the current bounded task rather than creating overlapping world rewrites. Shared-state support begins in Phase 1 and accompanies each activity; multiplayer is not postponed to the final phase.

The active implementation task is **Phase4: gifts and shared cake creation**, building on the checked directional and guest slices. Continue through the remaining band, castle, villain, persistence and presentation phases. The original viewer/export remain comparison evidence. Do not require purchasing access to the official locked worlds to continue.

For each completed task record: phase and feature, exact source references, files changed, source/build revision, actual interaction and client count checked, remaining limits and the next task. Update this plan’s phase status and the build guide’s work record. Keep future production files within the existing world ownership pattern under `Code/Core/Worlds`, `Code/Client/Worlds` and `Resources/Worlds`; add the exact new world paths to maintained layout metadata when implementation creates them.

Current completion: **dependency recovery, shared2D foundation, directional holding/actions and the introduction/guest slice**. Phase4 is active; later activity and whole-world checks remain required. The full birthday world is not complete. Original checkpoint records below retain their historical limits; the later recovery record supersedes their missing-assets blocker.

## Complete dependency recovery and first shared slice October 15

The shipped binary Addressables catalog was decoded with the matching official Addressables 2.7.2 package. Its Hulk scene dependency closure contains299 locations and225 remote bundles. All225 were retrieved from the publisher URLs in that catalog, totaling382,344,465 bytes, with UnityFS signatures, response lengths and SHA256 receipts. This did not unlock or modify the installed app. Original APK/export copies remain preserved.

The combined export is `LocalData/Research/DuploMarvel/marvel-complete-hulk-project/ExportedProject`. Its scene SHA256 is `23075a913e3c8b611f8b7782889374c9110d5bea99484124faea900a5228bbf0`. The read-only inspection now reaches3,393 files and finds **zero placeholder/missing reference usages**, versus3,319 previously. File presence alone does not establish imported subobject identity; the additional Unity audit directly loaded all eight character controllers and found866 clip bindings, no missing meshes and no missing bones. It does not execute the original game logic.

The cake controller's recovered sprites are the green, brown and pink **plates**. They are not three finished cakes. The former viewer icons remain an explicitly superseded substitute test. The recovered BuildACake directory contains41 Sprite assets and their texture files, including editor content. Unity rendering confirms that the three `cake_plate` sprites depict cake bases on plates; decoration is separate. Do not count asset/texture pairs as two choices or call the bases fully decorated cakes.

### Character presentation and holding

[Probe preparation](../../Tools/Art/Prepare-RecoveredUnityProbe.py), [exact attachment bindings](../../Tools/Art/Build-RecoveredActorBindings.py), [Unity asset audit](../../Tools/UnityEditor/RecoveredWorldAssetProbe.cs) and [2D baking](../../Tools/UnityEditor/RecoveredBirthdayBake.cs) keep recovered files and generated imagery in ignored research storage. The probe excludes recovered executable scripts/assemblies. Replacement rendering shaders remain identified as replacements.

The bake resolves actual per-character override clips and produces135 clip action sets plus eight controller-driven hold/carry sets for the four companion roles. All143 sets have exact hand, hold and face-anchor tracks. Twenty-five source scene props have rendered cutouts; gift hold/base pivots use their source anchors. The fixed orthographic world projection produces four adjoining1200×800 scenery tiles. The production game remains2D.

An additional58 concrete native ranges, including HoldPresetSo and AnimatorComponent, pass independent ELF mapping and complete ARM64 decoding. HoldPresetSo.get_HoldPresets at `0x3947710` divides the serialized percentages by100. Default_holdPreset selects both arms,50% vertical,0% horizontal and100% spine. The actual controller is used for carry/hold sampling. Legacy `Lift` strings in actor components are not automatically assumed to exist as physical controller parameters.

Static bindings, sampled clips and controller evaluation do not accept the full action matrix. Runtime walk/carry/handoff, facing, cancellation, normal-speed presentation and later activity actions still require their feature checks. No owner visual acceptance has been claimed.

### Guests and shared foundation

The exact guest strategy selects13 civilian prefabs: HatPoliceGuy, BlondePoliceLady, BlueBlazerGuy, BlueBlazerLady, ConstructionGuy_02, ConstructionGuy_03, ConstructionLady_01, GlassesGuy, GroovyGranny, JeanJacketGuy, LavalliereLady, RedStripedShirtGuy and BusDriver. The source enables randomized ordering. Its spawner config specifies actorLimit3, presentLimit10 and20–30-second intervals. The civilian filter requires its civilian trait and excludes Holding; the present filter requires its present trait. These filtered limits must not be presented as an unconditional cap on every character in the world. Guest arrival/departure gameplay remains Phase3 work.

The new Hulk-owned Core and Client folders implement the first authority/2D slice: stable prop IDs, one authoritative hold transaction, competing-claim rejection, atomic human handoff, role/instrument reservations, independent release, detached snapshots and restore cleanup. Schema54/content83 are an **undeployed candidate** change. The local Windows research content pack is selected with `LITTLE_WEEPS_BIRTHDAY_ART`; source delivery does not include proprietary recovered assets or qualify mobile packaging.

Focused four-member rules passed inside Unity, including additive migration, claims, handoff, deep copies, instrument departure and JSON restore. Windows blocked the standalone .NET test artifact through Application Control; no security setting changed. The normal game build also exposed and fixed the empty inline birthday object produced by JsonUtility on legacy snapshots. The existing Sandpit migration check now compares against WorldLayout.Schema while preserving its identity/progress assertions.

Windows536 and537 built. Four actual clients passed entry and pickup contention.536 exposed normal-avatar overlap;537 corrected it, but the handoff tap still hit overlapping image bounds. The next candidate adds face-anchor handoff targets and alpha-aware prop hit testing. These are failed/incomplete handoff acceptance runs, not whole-world passes. Local evidence is under `LocalData/SharedGarden/99e9da57f2b249f3a92da1c536fa92fa/hulk-birthday` and `15654299aef8496baf632660bd0f72e8/hulk-birthday`.

### Shared foundation accepted Windows539

The four-client check passes at `LocalData/SharedGarden/3beda662771d42688b0ce7aebba357dd/hulk-birthday/result.json`. It uses actual UGUI touches and actual walking inputs for pickup, handoff and carrying, rejects a second client's conflicting claim, disconnects the recipient, confirms only three active player presentations remain, and picks the released gift up again. Every client confirms the rendered owner after pickup; captures wait for the relevant rendered state. The handoff and final re-pickup views were inspected. Disconnected profiles return to the NPC presentation instead of lingering at the former player position; handoffs to disconnected profiles are rejected.

The verifier touches a prop's visible centre rather than assuming its source ground/hold pivot is a valid tap point. Snapshot placement fixtures and real walking/touch interactions remain distinct in the test. The source/build and separate recovered-pack hashes are retained with the run.536–538 evidence remains preserved, including the completed538 foundation run at `2493000b6a7542f1b9e8a77e90630fad`;539 adds the departure presentation correction and explicit rendered-state checks.

The standard whole-game prebuild checks passed with538. Subsequent birthday-only follow-ups can use `Tools/Build/Build-FamilyGame.ps1 -BuildNumber N -FocusedBirthday`: identical release build profiles, focused `HulkBirthdayRuleChecks` including legacy JSON and disconnected-receiver rejection, and the separate four-client check. This does not claim to rerun unrelated whole-game checks. The linked standalone test project remains under `Tools/Verification/HulkBirthday.Tests`; Windows Application Control blocks its execution here, so Unity runs the same checks. These Hulk-owned checks remain while the world consumes this contract and retire only with a replacement contract/test suite.

Current bounded task: Phase4 gifts and shared cake creation. The action/guest slices below supersede the earlier next-task entry. Cake construction, food/drink, working band/audio, castle physics/building, villain loops and whole-world persistence/presentation acceptance remain required. The additional record fields do not constitute implemented activities. No device installation or installed-server operation occurred. Retain checked research content packs when producing later variants so earlier evidence remains reproducible.

## Phase 0 inventory checkpoint October 15

The first authorized implementation task adds two read-only research tools: [exported world inspection](../../Tools/Verification/Inspect-RecoveredUnityWorld.py) and [original reference resolution](../../Tools/Verification/Resolve-RecoveredUnityReferences.py). Generated records and recovered source content remain in ignored `LocalData/Research/DuploMarvel/world-tests/HulkBirthday/Phase0/`.

### Cast and activity map

The exported scene contains 33 MonoActor components: eight named Marvel characters, one squirrel and 24 prop actors. The Marvel actor objects are Hulk_NewNew, Spiderman_NewNEW, Spin_NewNEW, Gwen_NewNEW, GreenGoblin_NewNew, BlackCat_NewNew, Electro_NewNew_BAP and DocOck_NewNEW. These are confirmed scene identities, not a claim that every character is player-controllable. A police guest prefab is referenced externally; the complete dynamic guest pool still needs its spawning/selection rules traced.

The inspector parsed 58,490 scene documents, including the previously established 15,801 GameObjects and 9,620 script components. It followed references through 1,940 exported files and generated static closures for all 128 activity components selected by its activity/controller rules, with no closure limit reached. It expands action GameObjects and MonoActor service components but excludes unrelated transform ancestry. This is complete traversal under those rules, not proof of runtime behavior or an exhaustive count of every tappable. Dependency injection, dynamic loads and runtime-selected services still require separate traces.

`actor-action-map.json` records each actor’s configured behaviors, inventory slots and controller binding. `activities.json` and `activity-closures.json` record activity owners and linked static dependencies. Assembly-bound scripts retain their DLL and fileID rather than receiving guessed type names.

### Hulk holding behavior

Hulk’s inventory component is scene object 49715, on actor object 1177. It defines seven named slots: Holding, Drink, Camera, RightHand, Wrench, WateringCan and LeftHand. Slot conflicts are explicit peg lists. Its anchor component separately identifies hold, left-hand and right-hand attachment transforms. Animator configuration uses `isHolding` and `armBlend`; the lift behavior has `Lift` and `StopLifting` triggers. Those data are available even though the controller asset is absent from the checked bundle directory.

The focused native pass attempted 258 members. The 249 concrete native ranges all passed independent ELF byte mapping and full ARM64 decoding. The nine members without native bodies are interface declarations, not nine unexplained recovery failures. Registration metadata also supplies a native entry/name index; instruction bodies still use the corrected ELF mapping rather than upstream Cpp2IL printed bytes.

| Verified native path | Finding | Reconstruction consequence |
| --- | --- | --- |
| NewActorInventoryActivity.HoldActor at `0x3B8C398` | Compares the current owner of the inventory’s actor with the supplied activity; obtains the requested slot and returns failure on the rejected path | Preserve activity ownership and explicit slot selection |
| HoldSlot.HoldActor at `0x3C25898` | Checks slot availability, invokes the snap/attachment operation and only updates the held-actor state after success | Attaching a visible prop alone is not a successful hold transaction |
| HoldSlot.GetIsPegOpen at `0x3C25768` | Checks its peg and the configured blocking-peg collection | Do not replace the slot system with an unrestricted single held-item field |
| HoldSlot.RemoveActor at `0x3C25A30` | Clears the held state and invokes the peg release path | Release must update both logical possession and attachment |
| NewActorInventoryActivity.RequestHoldActor at `0x3B8D9A4` | Native body returns false | This apparently suitable wrapper is not the usable holding path |
| NewActorInventoryActivity.OfferActorHolding at `0x3B8C558` | Native body constructs NotImplementedException | Do not use metadata names alone as evidence of working functionality |

The full casting, persistent-behavior cleanup and cross-actor handoff transaction are not yet closed. No animation or pickup interaction was executed in this checkpoint. Native decoding establishes these code paths, not their visible acceptance in Little Weeps.

### Missing assets and the colour count

The exported dependency graph contains 3,319 placeholder usages. Of these, 3,318 are in HBP_Main; the remaining usage is a material on the referenced BnP_BasePlate_2x4x1 prefab. Reading the original birthday bundle recovers the original PPtr identity for every one of those 3,318 scene usages. They target 86 serialized CAB files. These are references and internal files, not 3,318 distinct assets or 86 missing games.

All 138 bundles in the copied APK’s `assets/aa/Android` directory were indexed successfully, exposing 147 serialized files. None of the 86 target CABs appears in that directory. This establishes a gap in those copied addressable bundles; it does not establish absence from every possible player-data archive, app cache or remote download. The known phone endpoint refused the connection during this pass, so its cache was not inspected.

Two concrete targets now have exact identities:

- Hulk’s Animator 47274 references controller pathID `8868263673400657418` in `CAB-6fdfa5ff4fa22c51ec50c0151955caab`.
- Cake controller 54845 references three sprite pathIDs in `CAB-c49f73b098bf11082c82ae4521c2c9aa`; `original-pointer-crosswalk.json` records each field and ID.

Use those identities to check remaining player-data content and catalog/download dependencies. If a required source asset remains unavailable, reconstruct the required animation or visual role explicitly and check the difference; do not silently pass an icon or generic controller as recovered original content. Remaining native behavior can be traced independently of asset acquisition.

The 4,507 brick-colour components belong to **4,442 distinct GameObjects**. Sixty-five objects each contain a second colour component. No colour component lacks a renderer. The viewer counted owners, so the earlier apparent missing-renderer explanation was incorrect. Some duplicate values differ; preserve them in the evidence and choose the effective material deliberately when reconstructing presentation.

### Checkpoint verification and remaining work

Both tools completed against the real recovered inputs. There were no YAML parse failures, duplicate meta GUIDs, unresolved local object references or truncated activity closures. Exact types for assembly-bound MonoScripts remain unclassified. Scene SHA256 remains `ee28fbcd1623430ef05735e059de538b515109fd0f64f1dfc4208d9ae548bc73`; original bundle SHA256 remains `776fa95dac34d89e202d60c9bc2070037ab2c6d0a786bf6598bcef027292e0e2`.

Next: locate or reconstruct the mapped controller/clip bindings; trace the guest strategy and complete Hulk’s transfer/release sequence; then exercise the first character action slice. Phase 0 remains active until those gaps have usable behavior and dependency dispositions. No new Unity build, family-world source change, device installation or live-server operation occurred in this checkpoint.

### Directional actions and shared guests checked Windows542–544

Windows542 passes four actual clients with the v3 pack at `LocalData/SharedGarden/f54f10d9d3714459a0fa25d8e4000d4f/hulk-birthday/result.json`. Right/left carry samples advance through multiple source frames, mirrors follow their anchors, the recipient reacts while holding, and departure/reuse pass. A real frozen-animation defect was fixed: cosmetic playback now uses the local frame clock rather than waiting for authority snapshots. Source clip duration governs loop length. Earlier540/541 runs remain failed evidence; a screenshot taken during the short walk also interrupted sampling.

Windows543 and544 pass the extended four-client introduction/guest/NPC checks. Latest checked evidence: `LocalData/SharedGarden/e28e87e9d0dd4c27b3015093c313255b/hulk-birthday/result.json`. Two recovered civilians arrive carrying the two additional stable gifts, deliver them, and leave without deleting the gifts. Returning players keep the shared introduction. A departing profile's role continues as one companion body; actual face-button touches give and take the same gift through that companion. The Unity authority fixture also covers guest departure, saved randomized unique order, candidate-v1 additive migration and retained inventory.

The immutable v4/v5 packs contain575 action sets across21 source actors. V5 adds held orientations to reach150 prop views across25 source props. Atlas/anchor checks find no empty frames, missing tracks or detected clipping. Guest order uses all13 source civilian identities. Source20–30-second intervals and the ten-present budget inform the reconstruction; three fixed guest slots and forty-second visits are explicit family multiplayer adaptations, not recovered native timing claims. The source introduction's six-second camera-pan duration informs a six-second shared welcome; local cameras remain independent and the whole party is not restarted on join.

Per-world candidate format2 adds four persistent companion records and three guest records. The world's schema54/content83 remain undeployed. Human control replaces the same role's NPC presentation; NPC-held props transfer atomically or drop safely when that role becomes player controlled. Raw recovered content remains local and ignored. Full action/activity semantics, original audio, complete occlusion and owner visual approval remain open. Current held-gift captures still obscure faces in the flat projection. A pending client-only adjustment lowers bulky gifts for readable2D holding while preserving their source anchors in the research pack; it is an intentional fidelity adaptation and awaits the next visual check.

Phase4 research maps the source gift strategy to cake, boulder, flower, pumpkin, book, teddy, cat and pug; cat and pug each have a one-instance limit. Hulk's dislike source selects Pumpkin, Flower and Book traits. Cake export renders41 source Sprite assets; the production selection will exclude screenshot/editor-control sprites. New cake/outcome assembly tools are preparation, not accepted gameplay. Drink supply is not established by the birthday dependency graph: if the recovered common FrostyDrink actor is offered, record it as an explicit family addition, not a source birthday stall.

### Shared cake and first gift outcome checked Windows546

Four actual clients passed the shared cake workflow at `LocalData/SharedGarden/dd3cd5f41e5a4f5cb875662d8e40d195/hulk-birthday/result.json`: all four place decorations on one cake, one finishes it, all receive/consume bounded portions, a cold drink is consumed, and Hulk unwraps and eats the first gift's cake. Pickup contention, transfer, directional carry, disconnect and reuse pass in the same run. The cake editor, unwrap and final carrying captures were inspected; the flattened held-gift adjustment now leaves Hulk's face visible. Fixture placement and gift setup remain distinguished from actual touches.

Candidate per-world format3 adds saved sticker positions and the shared unwrap reservation. JSON/restore checks retain cake state and cancel transient leases. The source gives eight outcome identities and three disliked traits; Unity547 additionally passes every outcome, bounded rewrapping, reaction selection and movement cancellation. Source limits on the two pet types remain enforced. `Wrap again` is an explicit family replay adaptation that reuses an existing gift instead of accumulating unlimited loose contents. Windows548 passes the replay, negative reaction, cancellation and walk-to-Hulk follow-up with four real clients at `LocalData/SharedGarden/ba69de61d17b4cddb11b82e5d67a40f2/hulk-birthday/result.json`.

The v6 immutable pack has575 action sets,204 prop views across34 source props, three source cake bases and26 decorations. Source textures and generated imagery stay ignored. The recovered common FrostyDrink is an explicit family addition. Source sprites named `cake_plate` are cake bases on plates, confirmed by actual Unity rendering; previous plate-only wording is corrected.

Native546 initially reached all four decorations but the verifier raced a pending client acknowledgement. A subsequent run reached the revealed cake before its client-side button appeared. The verifier now waits for visible, enabled controls as well as authority state, and failure logging no longer skips process cleanup after an input assertion. The one affected isolated run's five processes were stopped by exact run identity; the installed family server was untouched. These failed runs do not replace the passing run above.

Phase5 preparation resolves Spin/guitar, Spidey/keyboard and Gwen/drums from source trait GUIDs and maps the two birthday arrangements plus three16-second instrument recordings through their actual HbpAudioSource/AudioSource references. FFprobe decodes all seven birthday music files, including intro/main. V7 prepares PCM copies with source/output hashes; no band gameplay or subjective audio approval is established by that conversion.

The Phase4 source stall is now explicitly resolved: StallVendorActivity54159 references seller54831, the scene Squirrel, cake variantCR as its sold item and variantVR as its additional item. StallPatronQueueActivity56879 consumes ActorQueue54240 with three source waiting spots. The four-player reconstruction must add its fourth shared waiting place deliberately, preserve one seller/service sequence and allow independent departure. Direct cake sharing does not close this queue row.
