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

## Evidence and build order

**Last verified deployment:** PC server/helper 91; iPad 7 build 95; iPad 9 and iPhone 79; Samsung 83. These are recorded versions, not a claim that devices or the server were contacted during this audit. **Prepared client candidate: 98**, with Windows, Android-emulator and native Apple compilation evidence; physical installation/acceptance is pending. The unfinished device-host 99 experiment is archived locally and excluded from deployment.

Build 95's smoother offline walking and offline reopening passed on the older iPad. Build 98's no-rewind transition still needs the focused physical check and rollout. Four-device admission and scoped shared play already passed; sustained updated-device performance is still open.

The installed project uses Unity **6000.3.24f1**, NGO **2.13.2**, Transport **2.7.4**, Input System **1.20.0**, URP **17.3.0** and uGUI **2.0.0**. The manifest and lockfile are the installation source of truth. Older package tables are research candidates, not upgrade instructions.

1. Finish G3's focused 98 device/offline/rejoin qualification when devices and the user are available.
2. Meanwhile, **ART-PREP-02** can integrate the existing layered character study into an isolated Unity view with idle/walk/wave/carry states and a hand anchor. Keep authority and saves unchanged; use existing movement data. This does not require device hosting or completion of every deployment task.
3. **G5** establishes versioned bedroom, creation and reusable item contracts, safe idle returns and separate local/server persistence. It includes no offline merge engine.
4. **G6** finishes one polished home/backyard slice, then **G7** expands the content. Retain the remaining G1–G3, G5 and G8–G9 release checks; no G4 gate exists.

Unattended Apple renewal, independent backup/restore, sustained device performance and Android native 16 KB qualification remain unresolved gates within their proper release scope. VPS details can wait. They must not be described as finished, or prevent isolated art/content preparation that does not depend on them.

## Where to continue

- [Goal sheet](bluey-game-research-2026-09-23.html): the intended game.
- [Build guide and current work record](family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis): implementation sequence and evidence.
- [Current return checklist](implementation/return-checklist-ipad-lan-2026-09-24.html): only the user actions still needed.
- [Architecture audit](implementation/pc-vps-plan-audit-2026-09-25.html): scope, corrections and verification of this decision.

Dated implementation reports preserve what happened. Their old “next” instructions and superseded requirements are historical; follow the current records above.
