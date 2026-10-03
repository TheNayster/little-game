# Heeler Home world audit

Created October 2, 2026. Not reviewed. Scope and starting inventory prepared; current screenshots and findings pending.

This is the dedicated audit file for Heeler Home. Follow the [six world screenshot phase](../../six-world-screenshot-phase-2026-10-02.md) and use the [activity review template](../activity-review-template.md) for each activity. Initial inventory is based on main source `096ea65` and existing project records, not a new gameplay review. Latest recorded completed client build at setup is 414; installed devices and captured builds must be recorded separately when the audit runs. Preserve unfinished work in other checkouts.

## World scope

World menu ID: `home`. Home rooms, the connected garden and personal bedrooms are one destination. Include upstairs navigation and direct object play, not only the Games menu.

## Starting activity inventory

| Activity | Entry or interaction type | Audit status |
| --- | --- | --- |
| Hide & seek | Games menu | Not reviewed |
| Fishing and Feed fish | Games menu at the Home pond | Not reviewed |
| Books and quiet play | Direct interaction | Not reviewed |
| Science and coloring | Direct interaction | Not reviewed |
| Cooking and kept creations | Direct interaction | Not reviewed |
| Sofa, trampoline and other room or garden fixtures | Direct interaction | Not reviewed |

This is a starting list, not certification of complete feature coverage. At review time reconcile the current Games menu, direct scene interactions, internal areas and planned wishlist entries. Do not describe planned content as playable.

## Stories and completion steps

Pending. Add one activity review per inventory row, with the actual story, player inputs, meaningful choices, exact completion or open-play outcome, replay and sibling contribution.

## Buttons and layout

Pending current review. Map general navigation, activity controls, scene taps, gestures, help and exits. Record their actual placement and behavior with screenshots. Check that a child can identify usable furniture, return between rooms and find the next action. Review pond targets, hiding cues, science tool trays, book controls and kitchen stage changes separately. Preserve ownership of personal drawings and stored food.

## NPC purpose and behavior

Pending current review. Record who is present, what each NPC or animal does, how the player recognizes its purpose, whether it reacts to progress, and where crowding or blocked objects occur. Include the relevant saved cast and attendance behavior.

## Screenshot evidence

No new screenshots captured for this world by this setup task. Existing report screenshots below are research leads only until their build and unchanged source are verified.

- [ ] Record reviewed source commit, build, capture date, viewport and player count.
- [ ] Entry menu and world overview.
- [ ] Active interaction for each activity.
- [ ] Important choice, mistake or helpful feedback where present.
- [ ] Completion, replay or open-play outcome.
- [ ] One useful tablet or four-player composition view with independent departure where relevant.

## Research basis

These existing records provide starting evidence and primary-source links. Their old release status and next-step instructions are historical. Read [current decisions](../../current-decisions.md) before acting.

- [home-interactions-2026-09-25.md](../../implementation/home-interactions-2026-09-25.md)
- [home-books-2026-09-26.md](../../implementation/home-books-2026-09-26.md)
- [home-discovery-2026-09-27.md](../../implementation/home-discovery-2026-09-27.md)
- [home-kitchen-2026-09-27.md](../../implementation/home-kitchen-2026-09-27.md)
- [home-creation-storage-2026-09-28.md](../../implementation/home-creation-storage-2026-09-28.md)
- [home-pond-and-hiding-2026-10-01.md](../../implementation/home-pond-and-hiding-2026-10-01.md)

Source entry points:

- [SoloHome.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Home/GameScreen.Home.cs)
- [SoloBooks.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Home/GameScreen.Books.cs)
- [SoloMiniGames.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Shared/Navigation/GameScreen.MiniGames.cs)

## Current findings

No current visual or child-play findings yet. Do not copy Daycare problems into this world without evidence. Use the activity template to give each finding an ID, screenshot, consequence and proposed fix.

## Improvement priorities

Pending current review. Rank observed usability problems before optional gameplay additions. Record effort and whether a change affects artwork, client controls, shared rules or saved data. Keep one bounded improvement active after the audit.

## Completion and handoff

- [ ] Every current activity has a plain-language story and real steps.
- [ ] Required progress and optional play are distinguished.
- [ ] Actual buttons and gestures are explained with pictures.
- [ ] NPC or animal roles, framing and four-player spacing are reviewed.
- [ ] Findings cite evidence and proposals are clearly labeled.
- [ ] Source/build and evidence limits are current.
- [ ] Summary and relative screenshot links are ready for the parent or ChatGPT Classic.

Next action: run the world review phase for this world and replace pending sections with evidence.

## Independent world workspace

Use this folder to review and fix Heeler Home independently. Follow [this world phase plan](phase-plan.md), trace behavior through [code paths](code-paths.md), and record screenshot-backed problems in [findings](findings.md). Put this world's raw captures and captions in [screenshots](screenshots/README.md). Shared menu and capture files are in the [common code map](../code-path-map.md).

Screenshot collection has not started. A future fix should name its finding, source files and focused check; preserve other worlds and unrelated unfinished edits.
