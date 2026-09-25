# Parent server controls — G3-OPS-01

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**NET-02 / AUTO-01. Implemented and qualified in an isolated Windows world.** A new **Little Weeps Server** desktop shortcut opens the parent page. It shows fresh server readiness, connected-player count and the time/checksum of the last saved checkpoint. The live family server remains on **83**; the guarded-stop runtime is prepared as **84** and has not replaced the family's running process.

## What the parent page does

- Distinguishes **ready**, **stopped**, and **status unavailable**. A process existing is insufficient: its executable/config/instance/PID must match, and its native transport observation must be fresh and listening. Missing or stale observations lock the controls.
- Shows up to four connected family members as a count. It reads the actual checkpoint timestamp and verifies its checksum; it does not expose player credentials or imply an independent backup exists.
- Starts only the selected, already enrolled world with the existing artifact-verifying persistent launcher. Repeated starts reuse the existing authority. A cross-process operation lock and the game's existing save lock protect against duplicate starts.
- Stops only an empty authority. The browser/backend first check players, and **the server checks again on its own simulation thread** before saving and stopping. A new arrival blocks the operation. A stale page cannot stop a replacement server instance. There is no force-stop button.
- Keeps the game running when the page closes. The parent page binds only to loopback, requires its per-run local session capability for status/actions, checks Host/Origin and performs no mutations via GET.

Checkpoint observation opens a read handle with read/write/delete sharing so it does not deny the game's checkpoint replacement. A deterministic test replaces the file while that observer handle is held and verifies both the previous open snapshot and the new file. The test uses the Windows replacement operation corresponding to the game's `File.Replace`, rather than Python's differently behaving rename operation. [Microsoft CreateFile sharing documentation](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew), [ReplaceFile documentation](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-replacefilew).

The UI deliberately reports a save checksum, not a successful backup restoration. Process status is local readiness, not proof that a phone's network route or firewall is working. Full startup/crash supervision and internet/VPS controls remain separate work.

## Evidence

| Check | Recorded result |
| --- | --- |
| Windows build | Server/client **0.0.84**, zero build errors/warnings; all compiled C# files match the current source. [Build record](evidence/parent-controls-2026-09-24/build-84.json). |
| Parent operations | **10 checks passed** through the real local HTTP/controller and native server: access boundary, production network-setup gate, nonblocking save reads, clean start/save, repeat start/operation lock, mismatched PID/stale heartbeat, occupied stop refusal with four clients continuing bucket/travel play, empty clean stop, corrupt-checkpoint refusal, and exact checkpoint restart plus stale-instance rejection. [Native evidence](evidence/parent-controls-2026-09-24/native-84.json). |
| Existing clients | **84 server / 79 client** and **79 server / 84 client** pass admission, pickup, fill and shared state in isolated Windows processes. This does not retest physical devices. [Compatibility](evidence/parent-controls-2026-09-24/compatibility-84.json). |
| Browser and actual family status | Start → Ready → Stop → saved/stopped verified by browser interaction on the isolated world; page layout inspected. The real family page then reported its original ready 83 authority. Production starts remain blocked until the prepared executable's Windows network setup is recorded. [UI/deployment scope](evidence/parent-controls-2026-09-24/ui-and-deployment.json). |

The native tests created their own protected enrollment and save folder. No live family stop, world restore, identity replacement, device update or remote deployment occurred. A deliberate corrupt-save test altered only its newly created, stopped test world and restored its original bytes; this is not the pending independent-backup restore drill.

## Use and deployment boundary

Double-click **Little Weeps Server** on the Windows desktop. Its launcher reuses the running page or starts a hidden local helper. The selected family/build settings and page session information stay in ignored `LocalData/ParentServer`; no credentials or local session tokens are committed. Closing the browser tab leaves both the helper and game server available; this is not a sign-in or boot service.

The current 83 server can be monitored, but its Stop button stays disabled because that runtime has only the earlier unconditional test quit path. **To activate the new guarded stop for the actual family, apply the prepared 84 server during an empty session, with the reviewed Windows network permission for its exact executable.** Preserve the existing enrollment/checkpoint through the existing controlled deployment workflow. This deployment was intentionally not performed while building/testing the controls. No client update is required for the new local stop protocol; Apple cue/layout updates remain their own pending task.

## Next bounded task

**G3-OPS-02: backup/restore workflow and an isolated recovery drill.** Preserve the checkpoint plus required protected enrollment, verify a restored isolated instance against saved identities/items/receipts, and document a safe failure path. An independent destination still needs to be selected and qualified; local copies alone do not meet that goal. Apply the prepared server-controls update separately when the empty-session/network-setup conditions are met. Crash supervision, sustained/device/outage testing, the VPS milestone, and required G4 iPad hosting/recovery retain their place in the [main build guide](../family-playset-build-guide-2026-09-23.html#9-first-implementation-work-queue).
<!-- historical-record-end -->
