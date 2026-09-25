# Parent backup and recovery panel — G3-OPS-05

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**NET-02. Implemented, tested and active for monitoring/backups on this PC.** The existing **Little Weeps Server** desktop shortcut now opens an updated page with **Back up world**, **Enable recovery**, and **Pause recovery**, plus last-backup verification and actual helper status. The actual family server remains its original **83** process. It was backed up through the browser, not stopped or replaced. Guarded Stop/recovery activation are correctly disabled until the prepared **85** deployment and reviewed network setup.

## Behavior

- **Back up world** uses the qualified OPS-02 workflow, reads back and validates the archive, then records the returned hash and completion time. Subsequent status reads recheck that file's hash; a changed or missing archive is not displayed as verified. The bundle and protected enrollment stay in ignored local storage. The browser does not receive enrollment keys, player credentials or file contents.
- **Enable recovery** requires a ready server matching the selected build, build 85 or later, a verified checkpoint and the build's recorded network setup. It establishes parent intent and starts the OPS-03 helper under its single-supervisor lock. It never automatically upgrades a different running build.
- **Pause recovery** disables future automatic restarts while leaving current players connected. Parent **Stop server** retains the occupied-stop guard and immediately reports the resulting paused restart preference. A stale automatic request cannot undo it.
- Closing the browser page does not stop the control service, supervisor or game. Restarting the control service resumes an earlier explicit helper opt-in while preserving the separate parent Start/Stop preference. A deliberately stopped game remains stopped. This is a local helper process, not installed boot/logoff service coverage.
- Routine browser actions do not include destructive restore. Restore remains the inspected, expected-hash workflow with a retained rollback preimage.

The localhost-only capability, Host/Origin checks, bounded JSON requests, no GET mutations and disabled browser cross-origin access remain in place. No credentials are added to the page or Git.

## Evidence

[Four HTTP/native qualification groups](evidence/parent-recovery-panel-2026-09-24/native-85.json) pass with a disposable enrolled world and four native Windows clients:

1. Verified complete backup during play; access checks retained; no routine restore endpoint or credential exposure.
2. Wrong-build/unprepared-network activation refusal; actual parent-managed helper recovery after a real test-server termination; all four original clients rejoin.
3. Pause leaves clients connected, occupied stop stays protected, explicit stop survives control-service recreation, and parent Start resumes supervision.
4. Last-backup state persists and an altered bundle is displayed as changed until its original bytes return.

Browser interaction separately passed **Start → Back up → Verified → Enable → Watching → Pause → Off → Stop → saved/stopped** in an isolated world. The panel layout was visually inspected. Then only the actual parent's old control helper was refreshed; its game authority was not touched. The actual family page reports **Ready / build 83 / backup verified / recovery Off**, with activation and Stop disabled. [Browser/deployment scope](evidence/parent-recovery-panel-2026-09-24/ui-and-family.json).

The first integration-test attempt compared durable action receipts to the intentionally receipt-free presentation view. The harness was corrected to compare the complete checkpoint; the corrected native run passed. No family world or device was part of that failed test.

## When the parent returns

Approve the reviewed Windows network setup for prepared 85, apply it during an empty family session while preserving the existing world, then enable recovery on the page. The dashboard is already selected for build 85 but refuses to replace or supervise the older running authority. No Windows approval prompt was opened while the user slept. No phone, iPad or Mac was used.

Independent storage/portable credential recovery, Windows boot/service deployment, sustained physical-device behavior and iPad authority recovery remain separate gates. Local backup verification is not independent disaster recovery.

## Next implementation task

**G3-REC-01: a complete, versioned, durable client recovery checkpoint as the prerequisite to prolonged-outage local continuation.** The existing view omits receipts and is not a complete authority backup. Preserve family/authority/world identity, revision/epoch, all areas, players, items, receipts and idle metadata; validate size/version/checksum and store atomically. Negotiate support so existing 79/83 clients retain their current behavior. Prove rejection of partial, stale, mismatched and invalid transfers without replacing a good checkpoint.

This brings forward the data prerequisite shared by the G3 outage flow and G4 hosting; it does **not** declare G4 complete or replace iPad election/handoff/partition recovery. Do not implement automatic switching by silently replacing the shared picture with the older solo draft. Preserve that draft separately, then implement the local recovery transition after complete-state delivery is qualified. Branch reconciliation remains G5. [Main guide](../family-playset-build-guide-2026-09-23.html#9-first-implementation-work-queue).
<!-- historical-record-end -->
