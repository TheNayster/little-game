# Hulk birthday world phase plan

Created October 15, 2026. Goal: a fully functioning Hulk birthday world in Little Weeps, with recognizable character actions, usable activities and up to four family players playing together. The current recovery viewer is a research baseline; it does not satisfy this goal.

The target is **HulksBirthdayParty**, displayed in the installed app as **Spidey & His Amazing Friends: Hulk’s Birthday**. It is distinct from the older **Hulk Smash** pack. Spidey HQ remains a separate recovery task and is not a dependency of this world.

Related project goals: WORLD-02, FAMILY-01 and ITEM-02. Follow [current decisions](../current-decisions.md), the [creation workflow](../ai/GAME_CREATION_WORKFLOW.md), [folder ownership](../project-folder-guide.md) and [server update policy](../server-update-policy.md). This request creates the plan; the later implementation phases are not marked started.

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

**Status: partial research.** The scene and viewer exist; the full roster, behavior graph and missing-asset manifest do not.

Resolve each reachable activity into its actors, props, triggers, actions, clips, sounds and cleanup paths. Traverse linked quests and behavior descriptors; include small tappables rather than relying only on named headline activities. Map script GUIDs and missing external assets to catalog keys and bundle dependencies. Classify each as recovered, recoverable, replacement required or unresolved. Identify why 65 of the scene’s 4,507 brick-colour components were not applied to renderers in the viewer.

Extend native analysis only for the next relevant controllers, beginning with character/prop services and the bouncy castle update path. Keep the verified ELF address correction; do not use the known incorrect Cpp2IL printed instruction bodies. For every behavioral claim distinguish serialized wiring, traced native semantics and proposed multiplayer adaptation.

**Done when:** every feature above has a mapped owner and dependency list, the source-confirmed cast and action matrix are listed, missing content has an explicit resolution path, and unknowns cannot disappear behind placeholder methods. This inventory determines the final small-interaction count; no unsupported “all activities recovered” claim.

### Phase 1 Establish the world and shared runtime foundation

**Depends on: 0.** Build a small Little Weeps integration slice with the recovered entrance, one actual actor and one grabbable prop. Retain the existing 2D production pipeline. Evaluate rendering recovered content into layers and animation frames, with correct depth, hit targets and attachment anchors. The 3D recovery viewer remains a reference tool; it does not authorize converting the game’s production architecture to 3D. Record any concrete fidelity gap before expanding the asset pipeline.

Create the world’s shared state, stable actor/prop IDs, validated commands, participation, reservations and save versioning through existing GameWorld/GameScreen and session patterns. The PC/VPS owns shared rules and clocks. Keep camera framing local. Map family profile identities to the confirmed playable roles without assuming every source NPC is player-controllable or spawning a birthday Hulk separately for each client.

**Done when:** four actual clients see the same actor and prop, one player’s claim is visible to the others, conflicting claims resolve once, and leaving releases only that player’s participation. Existing areas and private offline saves remain intact. This establishes the foundation; it does not close later activity acceptance.

### Phase 2 Recover character animation and action behavior

**Depends on: 0–1.** Reconnect recovered meshes/rigs, clips, controllers, masks, transitions, animation events and attachment points in the isolated reference project. Reconstruct missing behavior code from the mapped services and native evidence. Export the chosen compatible presentation into Little Weeps while preserving editable sources.

Build an action matrix per confirmed actor: idle/look, walk/turn, pick up/carry/put down, give/receive, celebrate/disappointed, jump/land, and the source-linked special actions needed later. Separate a reusable movement/holding action from a character-specific smash, throw, web action or stunt. Check normal-speed motion, hand contact, feet, pivots, facing, depth and cancellation. Visual callbacks must not cause duplicate authoritative effects.

**Done when:** Hulk and the first source-confirmed companion perform an end-to-end walk → pick up → carry → give/release → react sequence visibly and correctly. The remaining required cast/action rows must be completed before whole-world acceptance. A clip preview, static pose or sliding sprite is insufficient.

### Phase 3 Restore introduction guests and shared props

**Depends on: 1–2.** Reconstruct the introduction, invitations, guest arrival/departure, prop anchors, snap pegs and activity arbitration. Trace spawner limits and cleanup rules from source data; guest turnover must not duplicate players or discard an owned gift. Late arrivals join the current shared visit; they do not restart everyone’s introduction.

Implement gift provision, carrying, handoff and table placement with visible character actions. Resolve simultaneous claims using the shared authority. Activity transitions must release reservations and safely handle held props.

**Done when:** players can enter together, bring and place gifts, hand one to another participant, and leave independently while the party continues. Guests arrive and leave without permanently consuming actor or prop capacity. The introductory sequence hands control back to open play.

### Phase 4 Complete gifts cake food and drink

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

The next implementation task is **Phase 0: finish the cast/action map and missing-dependency manifest**, with a focused Hulk pickup/carry/handoff trace as the first behavior slice. The existing viewer is reused for comparison and does not need another setup or broad rebuild first. Do not require purchasing access to the official locked worlds to begin this work.

For each completed task record: phase and feature, exact source references, files changed, source/build revision, actual interaction and client count checked, remaining limits and the next task. Update this plan’s phase status and the build guide’s work record. Keep future production files within the existing world ownership pattern under `Code/Core/Worlds`, `Code/Client/Worlds` and `Resources/Worlds`; add the exact new world paths to maintained layout metadata when implementation creates them.

Current completion: **research baseline and recovery viewer only**. Phases 1–9 remain planned. This document adds no game runtime changes and makes no claim that the character actions match yet.
