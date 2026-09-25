# A replacement host restores the real shared world

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**G4-PREP-02 is complete within Windows experiment scope.** A fresh native build-98 authority now has a verified test path from a client's actual complete recovery record. Four original player profiles rejoin it, with both areas and their item state preserved. This closes the next hosting prerequisite; automatic iPad hosting is still ahead.

Goal IDs: **AUTO-02, NET-02, FAMILY-01, WORLD-01**. Reviewed against goal-sheet sections 45, 46 and 50 and the [hosting preparation sequence](g4-hosting-and-animation-preparation-2026-09-25.html). The user's current server-wins rule remains authoritative: private offline edits are saved separately and never automatically uploaded into the family world.

[Main plan](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) · [Return checklist](return-checklist-ipad-lan-2026-09-24.html) · [Character workshop](character-workshop/index.html).

## What changed

`Tools/HostingRestore.Tests/` links the existing production `RecoveryRecord`, `SoloWorld`, `FamilySession` and `CheckpointStore` code into a Windows test tool. `Tools/Test-ReplacementAuthority.py` drives real Unity server/client processes in a disposable enrolled family. No shipping Unity code, packages, prepared build 98, physical app or real family save was changed.

The test captures a **client's durable complete replica**, including 128 retained command receipts and idle clocks. It retains the original test server directory, then constructs a new authority directory from that replica alone. It does not obtain replacement world state from the old server save.

The import pins the selected family, authority, world, epoch, snapshot-world identity, roster, checkpoint serial, revision and payload hash. It validates complete JSON fields as well as the actual game's rules. This matters because default-valued C# fields could otherwise make an omitted receipts/timers property look empty. This strict importer is a lab tool; a shipping mobile import boundary is not yet implemented.

Restoring releases stale pointer holds once. The restored revision advances once when a hold existed; item IDs, contents, player positions/areas/activities, retained receipts and unaffected idle clocks remain. Connections start empty and reconnect by stable profile identity, not old transport IDs. Publication creates a new directory after verification; it refuses to replace an existing destination.

## Evidence

| Check | Result and exact scope |
| --- | --- |
| [Production C# rules/storage](evidence/hosting-restore-2026-09-25/core-results.json) | **7 groups pass.** Two areas, four players, ten items, partially filled bucket, completed plant/cleanup, 128 receipts, stale-hold release, restored connections and operation replay. Empty worlds pause clocks; occupied play resumes the remaining idle interval. |
| Malformed and conflicting inputs | Those groups reject wrong family/authority/world/epoch/roster/order/hash; missing/duplicate/unsupported fields; invalid water/timers; private/view formats; future versions; damaged/truncated/oversized envelopes. A valid backup cannot silently replace the explicitly selected input. |
| Publication failures | Existing destination, injected interruption before publication and concurrent destination creation refuse replacement. Unpublished stages are retained. This is not physical power-loss qualification. |
| [Native Unity transport and storage](evidence/hosting-restore-2026-09-25/native-results.json) | **4 groups pass on 98.** Actual multi-chunk client record, fresh native authority, all four original enrolled profiles, both areas, retained action history and idle clocks. New interactions and replacement-epoch replication work after restoration. |
| Duplicate watering over transport | Reusing the accepted request returns its retained duplicate result without mutating the world; changing that request under the same ID is rejected. Coverage is the current 128-receipt window, not unlimited history. |
| Actual private adventure | A generated version-2 offline adventure is rejected as authority input. Its gameplay and provenance survive reopening/reunion, and subsequent shared operations leave its saved bytes unchanged. |
| [Prepared build consistency](evidence/hosting-restore-2026-09-25/source-match.json) | **410 runtime/package files** still match the native build-98 source manifest. No replacement device build is required for this test-only work. |

The first native run passed restoration and duplicate-operation checks, then exposed an overstrict test comparison: it treated a private save's advancing idle timers as an overwrite. Inspection found timer-only changes in those offline saves; the local lifecycle code saves on exit/reopen. The corrected check pins an immutable copy for import refusal, compares gameplay/provenance across reopening, allows forward elapsed local clocks, then compares exact bytes after reunion. The complete rerun passes. No gameplay patch was made to satisfy that test.

Private raw runs remain in ignored LocalData. The linked records contain counts, results and scope only, without enrollment credentials or saved family content. All disposable native processes were closed after the test.

## Research and limits applied

The previous [primary-source review](g4-hosting-and-animation-preparation-2026-09-25.html) established that host selection and synchronized-state transfer are separate responsibilities. This task verifies the second prerequisite using our actual game code instead of assuming a host-migration checkbox transfers everything. [Unity session host migration](https://docs.unity.com/en-us/mps-sdk/session-host-migration).

The source was explicitly stopped before the replacement started. The experiment reused that disposable family's PC authority credentials. It therefore does **not** establish mobile host permissions, automatic election, source retirement over a live transport, hard-loss fencing, or safe partition convergence. A selected replica may trail the latest live source state; planned handoff still requires the final frozen checkpoint and its acknowledged identity.

A valid checksum and selected-record metadata do not themselves authorize a device to host. The retired G4-01 proposal would introduce authenticated mobile host identity/capabilities and endpoint changes without distributing the PC's private key. G4-02 then connects the [planned-transfer model](g4-hosting-and-animation-preparation-2026-09-25.html) to actual journals/messages and repeated transfers.

Neither this Windows result nor the model replaces Apple lifecycle and A10 performance checks. Both iPads must eventually host with peers playing in independent areas, including while the host opens an in-game menu/book/video and when its app backgrounds or closes. Apple's background execution remains bounded. [Apple background execution](https://developer.apple.com/documentation/uikit/extending-your-app-s-background-execution-time).

## Next work and the user's checklist

**Retired task (not next): G4-01 — separate authority lifecycle and authenticated host identity from the local player's presentation.** Begin with the existing protected enrollment/discovery contracts and a bounded Windows adapter proof. Then wire cooperative transfer, hard-loss branch selection and reunion in the recorded order. Do not simply change `StartClient` to `StartHost` or promote a private adventure.

The optional isolated Unity character-view/import proof remains available as the next art task. Its browser [animation study](character-workshop/index.html) is ready for review now; final Bluey artwork and in-game rig integration remain unfinished.

The return checklist still has the same focused physical work: sign/install prepared **98**, test the older iPad's visible-state continuity and offline movement, update the other devices in place, then perform a short four-player check. Last verified installed clients remain iPad 7 **95**, iPad 9/iPhone **79**, Samsung **83**; last deployed server/helper **91**. No new user action was added by this restoration task, and no live-server availability check is claimed here.
<!-- historical-record-end -->
