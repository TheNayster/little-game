# Current project decisions

**Approved September 25, 2026:** “Lets stick with pc/vps.” This record resolves the earlier requirement for iPad hosting. Read it before the goal sheet, build guide or a dated research report. A later explicit user decision takes precedence; update all three records together when scope changes.

## Multiplayer and offline authority

| Situation | Required behavior |
| --- | --- |
| Normal shared play | One designated PC server now, or the owned VPS after a controlled migration, owns the family world. Up to four iPad, iPhone and Android clients automatically join after parent enrollment. |
| Someone joins, leaves, changes character or changes area | Only that player's participation/view changes. Other players keep playing; shared props have one holder at a time. |
| A client loses the server | Continue private solo play from the latest usable visible state, without waiting for a complete server recovery checkpoint or restoring an older world over it. Discovery/retries run independently of play. |
| No connection at launch, or Play by myself selected | Load the appropriate local save and run all installed solo activities. Honor intentional solo mode. Network access is not a gameplay prerequisite. |
| Reconnection | The server's current world wins. Keep offline work in separate local saves. Do not upload, merge or replay offline world edits into the shared world. Local settings and media bookmarks remain local. |
| PC/VPS stops | Clients continue separately in solo. Restart/restore the designated server through its own recovery workflow. A client never becomes the shared server. |
| PC → VPS cutover | Preserve the family world and enrollment through a verified backup/migration; retire the old writer before enabling the new one. Never elect between PC and VPS or run two canonical writers. |
| Traveling | Offline solo is required. Shared play over a usable internet/hotspot route to the designated server is optional and follows the VPS work. No signal means separate solo games. |

**Removed from scope:** device hosting, automatic host election/switching/migration, peer-host enrollment, router-free peer-to-peer co-op, Bluetooth, automatic offline world merging and conflict-import interfaces. **G4 and AUTO-02 are retired by user decision, not completed.** Keep their identifiers for traceability; do not reuse them or treat them as gates. Retained G4 experiments are historical evidence, not instructions to resume that work.

## Features that remain

Keep both iPads as the primary devices and the older A10 iPad as the performance baseline. Keep four mixed-device players, independent areas, shared item ownership, automatic idle returns, protected personal creations, separate bedrooms and secret rooms. Keep the six-world 2D interactive dollhouse, free character changes, two movement controls, draggable interactive props, optional spoken quests, cooking, science, dinosaurs, books, local TV, playground activities and daycare imagination games. Connected bedroom changes appear to connected family members; offline bedroom changes stay local.

English comes first; Spanish remains planned. Ordinary play uses installed content and prerecorded speech, without a live AI service. AR is optional. Free packages come first; consider a purchase around $20 only for demonstrated benefit. Personal TV media stays out of Git. The unrelated Meeps game project stays separate.

## Visual presentation and menus

**Approved September 25 from eight user screenshots:** use Budge’s Bluey: Let’s Play! as the concrete scene and navigation reference. The reference uses large circular scene thumbnails on a sky/cloud background; our latest combined-chooser decision below retains the circles in a vertical places rail. The bottom-left family portrait circle opens a light-blue full-body character tray with horizontal browsing and a large down-arrow close tab. This supersedes the rotating-wheel and small portrait-drawer proposals. [Reference study and acceptance](bluey-lets-play-reference-study-2026-09-25.html).

Keep all six main worlds, including Creek. Room navigation and camera panning are local to the selecting player. Tapping a character changes that player’s avatar without changing profile, bedroom ownership, position or held item. Preserve both requested movement modes and four-device family play. Detailed illustrated panoramas, layered furniture and expressive action poses are required for the finished presentation; current prototype acceptance is not final scene/animation acceptance.

**Further user clarification:** Bluey: Let’s Play! is the base experience for an expanded private family version. No public publishing or monetization is planned. Character–object behavior is explicit scope: proper sitting, trampoline jumping, dancing and other context-specific actions, with expressive entry/use/exit poses. The [expanded study](bluey-lets-play-reference-study-2026-09-25.html#18-characterobject-interaction-specification) maps locations, cast, games, sound, interactions and the family upgrades. Independent implementation and private use are not a legal guarantee; the study records the limited copyright guidance without treating it as a completed rights clearance.

**Item storage clarification:** item–item play is explicit scope, including putting toys/tools in the shed, baskets, drawers and other compatible containers; placing/stacking objects and combining ingredients. Stored items retain identity, contents and creations after closing, travel and save/reopen, subject to the recorded borrowed-item return policy. [Storage contract](bluey-lets-play-reference-study-2026-09-25.html#19-itemitem-interactions-containers-and-shed-storage).

**Explicit radio behavior:** radio on → music plays → nearby idle characters automatically dance; radio off → music stops and characters settle. Walking or starting another action overrides dancing, and busy siblings keep their activity. Preserve mute, character-switch, save/reopen and shared/offline rules. This is ordinary object-driven free play; musical statues is an optional game layered on top. [Detailed reaction and acceptance contract](bluey-lets-play-reference-study-2026-09-25.html#20-sound-expression-and-pace).


**Connected house and backyard — user clarification, September 25:** Home and Backyard are entrances into one continuous family property, from the front of the house through its fully usable rooms, kitchen/dining area and veranda to the far backyard and shed. Sideways exploration should feel like one long dollhouse level; doors/stairs connect bedroom and other room branches without returning to the world menu. Retain cooking, living/TV, reading, science/dinosaur play, personal bedrooms, secret rooms, garden equipment, radio/dancing and durable shed storage. The two bubbles are arrival shortcuts into the same persistent property, not duplicate houses or independent copies of its items. Each player has an independent camera and can remain indoors while another explores outside. Art/room chunks may load around the camera for the older iPad, but crossing between them must preserve object IDs, held items, container contents and authority. This is the required G5/G6 layout, not a claim that the full property is implemented by the menu milestone.


**World-art sequence — latest user direction:** first build attractive Bluey-style walkable scenery for all six worlds. Add no new activity features to the other worlds yet. After those visual shells, concentrate sustained room and interaction development on Bluey's connected house/backyard property. Keep existing functioning play and the full feature backlog; defer other-world feature implementation rather than delete it.


**Combined chooser — latest user decision, September 25:** the lower-right family circle opens one menu (moved away from the left joystick after the user’s phone test), with full-body characters browsing horizontally along the bottom and circular place thumbnails browsing vertically down the side. The down arrow closes both. Tapping an available place opens a loading screen on that device, prepares the destination and its current state, then enables play only when ready. This supersedes the separate full-screen staggered world browser as our navigation layout; R6 still guides the circular scene artwork. No second Play button is required. Browsing either axis must not accidentally select an entry. Other players continue independently.

**Long worlds and loading:** every world needs substantial horizontal walking space. The user selected a loading screen for major-world travel from the combined chooser. Retain a bounded cache of nearby presentation sections and lightweight persistent world state; release distant visuals/audio rather than keeping all six finished worlds resident. Connected house/backyard section loading remains a future implementation task, with continuity and actual memory/frame-time budgets measured on iPad 7. Separate loading controls working memory, not installed storage. Native 105 provides the transition and readiness barrier for the two existing resident prototype areas; it does not yet stream or unload scenic assets. Addressables is not installed. [Unity 6.3 background scene loading](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html) · [Unity asset memory management](https://docs.unity3d.com/Packages/com.unity.addressables@2.7/manual/memory-assets.html).

## Evidence and build order

**Character correction, September 25:** after rejecting the generic pup, the user requested the corrected layered Bluey/Bingo artwork in the real game. On Android build **100** they reported it works great and looks great for a prototype, while noting stiff movement. Prototype appearance and integration are accepted; final animation polish and the complete roster remain open. Build **101** restores Garden/Creek navigation in ordinary solo and upgrades old saves additively. [Implementation and retained-state evidence](implementation/character-phone-switch-2026-09-25.html). Official references and editable source art remain required.

**Last verified deployment:** PC server/helper **91**; Samsung **107**, iPad 7, iPad 9 and iPhone **101**. [Android 107 menu update and retained saves](implementation/combined-chooser-2026-09-25.html#phone-control-placement-builds-106-and-107). All clients updated in place with saved data retained. Apple runtime 101 and original enrollment are verified; the iPhone required restoring its existing protected player connection after switching signing routes. The stopped server was started with its original world and four players joined. [Delivery evidence](implementation/character-phone-switch-2026-09-25.html). The unfinished device-host 99 experiment remains excluded.

Build 95's smoother offline walking and offline reopening passed on the older iPad. The build 98 continuity repair included in 101 still needs focused physical outage/rejoin acceptance. The Android 101 character/Creek check is scoped evidence, not sustained mixed-device performance.

The installed project uses Unity **6000.3.24f1**, NGO **2.13.2**, Transport **2.7.4**, Input System **1.20.0**, URP **17.3.0** and uGUI **2.0.0**. The manifest and lockfile are the installation source of truth. Older package tables are research candidates, not upgrade instructions.

1. Requested client **101** rollout is complete. Continue focused physical G3 outage/rejoin acceptance and sustained performance when the user tests; preserve saved data and enrollment.
2. **ART-PREP-02 / CHAR-01:** the corrected Bluey/Bingo workshop and real-game integration pass scoped technical checks, and the user accepts the Android prototype. Retain animation stiffness, additional views and final polish as open work.
3. **NAV-REF-02** passes [native Windows 105 checks](implementation/combined-chooser-2026-09-25.html): combined horizontal characters and vertical places, safe gestures, local loading, shared acknowledgement/failure handling and offline save/reopen. It builds on [NAV-REF-01 Windows 104](implementation/reference-menus-2026-09-25.html). Samsung now has 107; Apple clients remain on 101. Scenic streaming is pending. The initial G5 bedroom rule work remains a tested development checkpoint, with screen/network/save-adapter integration pending.
4. **G5** establishes versioned bedroom, creation and reusable item contracts, safe idle returns and separate local/server persistence. It includes no offline merge engine.
5. **G6** finishes one polished home/backyard slice, then **G7** expands the content. Retain the remaining G1–G3, G5 and G8–G9 release checks; no G4 gate exists.

Unattended Apple renewal, independent backup/restore, sustained device performance and Android native 16 KB qualification remain unresolved gates within their proper release scope. VPS details can wait. They must not be described as finished, or prevent isolated art/content preparation that does not depend on them.

## Where to continue

- [Goal sheet](bluey-game-research-2026-09-23.html): the intended game.
- [Build guide and current work record](family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis): implementation sequence and evidence.
- [Current return checklist](implementation/return-checklist-ipad-lan-2026-09-24.html): only the user actions still needed.
- [Architecture audit](implementation/pc-vps-plan-audit-2026-09-25.html): scope, corrections and verification of this decision.

Dated implementation reports preserve what happened. Their old “next” instructions and superseded requirements are historical; follow the current records above.
