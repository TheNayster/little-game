# The Beach world audit

Created October 2, 2026. Not reviewed. Scope and starting inventory prepared; current screenshots and findings pending.

This is the dedicated audit file for The Beach. Follow the [six world screenshot phase](../../six-world-screenshot-phase-2026-10-02.md) and use the [activity review template](../activity-review-template.md) for each activity. Initial inventory is based on main source `096ea65` and existing project records, not a new gameplay review. Latest recorded completed client build at setup is 414; installed devices and captured builds must be recorded separately when the audit runs. Preserve unfinished work in other checkouts.

## World scope

World menu ID: `beach`. Review the wave activity and direct shore interactions. Record open exploration separately from activities with an ending.

## Starting activity inventory

| Activity | Entry or interaction type | Audit status |
| --- | --- | --- |
| Ride the waves | Games menu | Not reviewed |
| Shore, footprints, water reactions and visitors | Direct world interaction | Not reviewed |

This is a starting list, not certification of complete feature coverage. At review time reconcile the current Games menu, direct scene interactions, internal areas and planned wishlist entries. Do not describe planned content as playable.

## Stories and completion steps

Pending. Add one activity review per inventory row, with the actual story, player inputs, meaningful choices, exact completion or open-play outcome, replay and sibling contribution.

## Buttons and layout

Pending current review. Map general navigation, activity controls, scene taps, gestures, help and exits. Record their actual placement and behavior with screenshots. Check the wave start, player readiness, visible timing or steering prompts, finish and independent departure. Review camera framing at the shore, visitor approach and whether water effects make direct play understandable.

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

- [combined-world-release-360-2026-09-30.md](../../implementation/combined-world-release-360-2026-09-30.md)

Source entry points:

- [SoloWaveRide.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloWaveRide.cs)
- [SoloBeachShore.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloBeachShore.cs)

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

Use this folder to review and fix The Beach independently. Follow [this world phase plan](phase-plan.md), trace behavior through [code paths](code-paths.md), and record screenshot-backed problems in [findings](findings.md). Put this world's raw captures and captions in [screenshots](screenshots/README.md). Shared menu and capture files are in the [common code map](../code-path-map.md).

Screenshot collection has not started. A future fix should name its finding, source files and focused check; preserve other worlds and unrelated unfinished edits.
