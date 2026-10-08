# Elephant optional snack preparation - milestone 8, October 8, 2026

Scope: WORLD-02 / FAMILY-01 / ITEM-02. Elephant-only optional pretend snack pilot. Owner accepted milestone7 bird/butterfly surprises in this task. Existing bucket, picture navigation, low trays, personality, water, brushing and discoveries remain. Stop after this pilot; no other animals or later activities.

## Controls and appearance

Compact preparation table: x2020, foreground path, after the x1740 brush basket and below the flowers. Existing controls/spots remain in place. Ordinary walking/panning reveals the table on narrow views. Tap its bowl/food pictures to walk there automatically and open your own modal panel.

Large Leaves and Hay pictures reuse existing food geometry. Elephant species data explicitly accepts both catalog plant foods for this pretend activity; this is a game design rule, not dietary advice. All one-to-three-piece combinations are valid. Tap to add, tap a bowl slot to remove, use empty-bowl/away-arrow to clear, and bowl/right-arrow to carry. X or Back/Escape cancels. Empty bowls show a visual prompt and disable carrying; three pieces disable adding while removal/clear remain available. No recipes, timers, score, penalty or error popup.

Each piece settles into one of three slots. Identical pictures and blue bowl follow the child, rest at the assigned low tray and lift through the approved trunk handoff before consumption. One bowl uses the unchanged four-second Eat segment and one history increment regardless of piece count. The existing once-only finish clears that serving. Another bowl needs deliberate selection.

Editable UI geometry: GameScreen.ElephantSnack.cs and [SVG source](../../../SourceArt/Zoo/Playable/elephant-snack.svg). Existing atlas/scenery pixels, GUIDs, audio and prefab references remain. No new sound or elephant controller. Full panel shield consumes input, hides navigation and stops only local held movement; authority and siblings continue.

## Authority and transitions

| State/action | Rule |
|---|---|
| Open | Zoo command validates elephant exhibit, station proximity and empty hands. Begin creates actor-owned epoch with zero pieces and no ticket/slot. Reopening is idempotent. |
| Edit | Exact actor-owned bowl, epoch and current edit required. Only Leaves/Hay; at most3 pieces; checked removal slot. Accepted edits increment edit number. Stale commands cannot double-add or change a newer bowl. Existing receipts also deduplicate retransmission. |
| Carry | Nonempty validated bowl gets one ticket/free spot through LeaseZooFood, the same helper as bucket take. Preparation never buys priority. Edits lock. |
| Offer/eat | Existing ZooWalk/offer, FIFO scheduler, low tray, lift, consumption and finish. No separate queue. Chosen pieces stay in authoritative snapshots for every client. |
| Active food + station | Offered food keeps its status; carried food resumes its existing offering walk. Cannot silently replace, duplicate or reserve extra spots. |
| Cancel/switch | X/Escape clears only this unfinished bowl; bucket selection then works normally. One explicit cancellation, no dialog. |
| Leave/disconnect | Only that actor's transient bowl/offer clears. Walking out is valid; next authority tick clears preparation as it does care. Siblings retain their bowls/tickets/progress. |
| Brief reconnect | Bowl survives while authority retains participant. Disconnected panel closes; deliberate station selection resumes it. Actual departure clears it. Rejoin/late join sees current servings without old finish replay. |
| Private pause/reopen | Pause retains live bowl. Actual reopen uses SuspendZoo to clear editable/committed snacks, retaining feeding history/unrelated saved data. No UI objects are saved. |

ZooFood owns bounded pieces/preparing/epoch/edit; copies clone pieces. ZooState nextPrep is an additive epoch high-water hint. Old JSON defaults to empty pieces/zero hints. No durable recipe/inventory or schema migration. Content76 replaces75 for changed shared commands/snapshots/acceptance rules; schema53/protocol3 remain. Live family server463/content72 and devices are untouched; later rollout needs separate authorization.

Prepared food uses ordinary feeding priority. Existing .35-second care/.6-second splash transition bounds and care resumption apply. One bounded existing command per deliberate panel tap; no new effect queue, presentation broadcast or subscriptions.

## Verification

Standard495 release client/server: zero errors/warnings; all2388 Unity inputs match its source manifest. Unity JSON gates cover allowed/unsupported foods, one-to-three limits, exact remove/clear, duplicate edit epoch, deep copies, zero early tickets, mixed bucket/snack leases, committed guards, walk-out/return, departure, restore and two exact consumptions. Existing feeding/personality/water/care/surprise/full-game gates also pass.

- [Source matching and combined scoped acceptance](../../../LocalData/ElephantSnack/source-and-acceptance.json).
- [Four actual495 clients: bowl/input/layout/mixed-queue completed groups](../../../LocalData/SharedGarden/8300aebed4a943c39bfed691bb25d793/elephant-snack/results.json). Actual touch selection/removal/clear, empty/max, stale edits, four different bowls, no early tickets, X/Escape, preparation/map shields, committed guards, all clients' matching snack snapshots, mixed FIFO turns, exactly four consumptions/finish effects and clearing pass.
- [Four actual495 clients: completed lifecycle group](../../../LocalData/SharedGarden/9c96c9ca690e48b7b8210b9bf3f0877f/elephant-snack/results.json). Independent departure, brief pause/rejoin retaining authority membership, actual disconnect clearing preparation, and late joining pass.
- [Fresh four actual495 clients: priority and unchanged giraffe](../../../LocalData/SharedGarden/5336a7f756b7450bbc2b8acdce586af8/elephant-snack/results.json). Prepared food preempts actual brushing and water; existing shared patch progress resumes; unchanged giraffe feeding passes. Whole focused run passes without runtime errors.
- [Private actual495 gameplay and process/save reopening](../../../LocalData/FamilyLAN/5ac1bd9487fd4aa4b51573228a37e253/elephant-snack/results.json). Editable and committed servings clear on separate actual reopenings; history retains; old finish effects/UI do not replay. Whole private run passes.
- [Phone panel](../../../LocalData/SharedGarden/8300aebed4a943c39bfed691bb25d793/elephant-snack/preparation-phone.png), [tablet](../../../LocalData/SharedGarden/8300aebed4a943c39bfed691bb25d793/elephant-snack/preparation-tablet.png), [small phone](../../../LocalData/SharedGarden/8300aebed4a943c39bfed691bb25d793/elephant-snack/preparation-small-phone.png), [mixed offerings](../../../LocalData/SharedGarden/8300aebed4a943c39bfed691bb25d793/elephant-snack/four-mixed-offers.png). Complete native1280x591,1024x768,640x400 views inspected; panel targets remain at least44x44 pixels. Private carried/offered views and live demonstration supplement these static captures.

Acceptance combines completed groups, without relabeling failed whole runs. Earlier fixtures failed on hidden navigation lookup, initially offscreen private movement, redundant reconnect travel and server-only care readiness before the client rendered the side. Corrected fixtures retain checks and wait for actual client readiness. One495 run reported a nonzero native client exit on explicit close after reconnect; stopped receipt/normal shutdown logs had no managed errors, and the subsequent four-client close/rejoin plus final focused run did not reproduce it. Its cause remains unidentified and failed evidence is retained. No new security setting, teardown workaround or gameplay bypass was introduced.493 private and completed scoped groups,494 standard build, and all earlier failed runs remain historical evidence rather than proof of final-source gameplay.

Windows native phone/tablet/small-phone views are simulated layouts, not physical-device tests. Previously Application-Control-blocked standalone .NET Zoo executable remains separate/unrun; security unchanged. No video. Technical checks and owner visual/motion acceptance are distinct.

## Changed files

Under Unity/FamilyPlayset/Assets/FamilyPlayset/Code: Core/Worlds/Zoo/ElephantSnack.cs and metadata, ZooWorld.cs, ZooSpecies.cs, Shared/Layout/WorldLayout.cs; Client/Worlds/Zoo/GameScreen.ElephantSnack.cs and metadata, GameScreen.Zoo.cs, Shared/Sessions/GameScreen.cs; Editor/ZooJsonTests.cs; Networking/FamilyGameVerification.cs.

Also SourceArt/Zoo/Playable/elephant-snack.svg/README; Tools/Verification/Test-ElephantSnack.py and Test-ElephantSnackSolo.py; standard build-assigned ProjectSettings; this audit and maintained links. No scene/prefab YAML edit. Pre-existing untracked material remains outside task commits.

## Live review and delivery

Unrecorded two-player495 demonstration completed using Tools/Launch/Review-ElephantSnack.py: Bluey selects leaves/hay, removes a piece and adds a replacement, carries the resulting bowl, and feeds alongside Bingo's ordinary bucket. Both offerings complete exactly once. Both connected windows remain in the elephant exhibit, no modal/pending action, controls released, empty hands and station available for owner input. Isolated authority ends after both windows close.

[Live ready receipt](../../../LocalData/SharedGarden/7caedd000e214174a69605a01776e45c/live-review-ready.json), [final native view](../../../LocalData/SharedGarden/7caedd000e214174a69605a01776e45c/elephant-snack/live-owner-ready.png), [changed bowl](../../../LocalData/SharedGarden/7caedd000e214174a69605a01776e45c/elephant-snack/live-changed-bowl.png), [mixed feeding](../../../LocalData/SharedGarden/7caedd000e214174a69605a01776e45c/elephant-snack/live-mixed-feeding.png). Both readbacks report0.0.495, ready, no pending action, zoo-savanna and preparation closed. Live demonstration is separate from owner appearance/motion acceptance, which remains open.

Test-PlanConsistency passes. Delivery uses the existing main branch and private origin; task files only. Pre-existing untracked root Packages/ProjectSettings, audits, evidence, documents and unfinished worktrees remain preserved. No task branch was created. Stop at milestone8.
