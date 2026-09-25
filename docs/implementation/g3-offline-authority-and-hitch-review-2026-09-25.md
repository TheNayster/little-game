# Offline play, server authority and the remaining walking hitch

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
September 25, 2026 · G3-REC-05 · NET-02 / AUTO-01 / JOIN-01 / FAMILY-01 / TRAVEL-01

## Current product decision

The user clarified that offline play may differ from online play. **The server's world wins on reconnection.** A local outage does not prove that the server failed: other children may still be playing there. Offline movement, toys and activities run locally without server acknowledgments. Rejoining downloads the current server world and does not upload or replay offline actions. Existing solo saves and saved adventures remain on their device.

This supersedes earlier requirements for automatic merging of arbitrary offline edits, including bedroom edits, into the server world. Connected bedroom synchronization, full solo content, four-player mixed-device play, independent travel and required iPad hosting remain in scope. Recovering a shared host from a complete checkpoint is a separate G4 responsibility.

## Primary research reviewed before editing

| Source | Finding and application |
| --- | --- |
| [Unity client/server topology](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/terms-concepts/client-server.html) | The server has ultimate authority. Family play uses its current world; local divergence does not become shared truth. |
| [NGO disconnect/shutdown lifecycle](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/components/core/networkmanager.html#disconnecting-and-shutting-down) | Disconnect callbacks expose loss and network subsystems stop during shutdown. Use ordinary Unity lifecycle for local presentation. Wait for network shutdown before starting another connection, not before local controls. |
| [Transport timeout configuration](https://docs.unity3d.com/Packages/com.unity.transport@2.7/api/Unity.Networking.Transport.NetworkConfigParameter.html) | Heartbeats/inactivity timeouts detect silent loss. Preserve the existing 2.5-second transport timeout; remove the additional ten-second application wait. Detection still takes time. |
| [Unity reachability](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-internetReachability.html) | An available interface does not prove a route to this server. Cellular availability or a Wi-Fi icon cannot override authenticated transport state. |
| [Apple responsiveness](https://developer.apple.com/documentation/xcode/improving-app-responsiveness), [hangs versus hitches](https://developer.apple.com/documentation/xcode/understanding-user-interface-responsiveness/) | Busy or blocked main-thread work can miss rendering deadlines. Correlate actual frame gaps with work; CPU samples alone do not establish disk waiting or a visible jerk's cause. |
| [Apple filesystem performance](https://developer.apple.com/documentation/foundation/improving-performance-and-stability-when-accessing-the-file-system), [disk writes](https://developer.apple.com/documentation/xcode/reducing-disk-writes) | Repeated serialization, atomic replacement and forced synchronization can increase cost. Overlapping writers are unsafe. Measure our single-writer path; do not replace it blindly with concurrent tasks. |
| [Unity JsonUtility background-thread contract](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/JsonUtility.ToJson.html) | Background serialization supports data that is not concurrently modified. Diagnostic exports copy plain samples, then serialize on a worker. No live Unity objects or world state are passed to it. |
| [Unity File Access Profiler](https://docs.unity3d.com/6000.3/Documentation/Manual/profiler-file-access-module.html) | File duration, frame and thread context support attribution. Measure save/network work against subsequent frame gaps before choosing a hitch repair. |

Scope: Unity 6000.3.24f1, installed NGO 2.13.2, Transport 2.7.4 patch 1. Online NGO 2.13 currently serves 2.13.3; no package update is part of this task. Unity does not prescribe this game's offline merge policy. Removing the extra wait follows the user's requirement and the documented separation of networking from local presentation.

## Confirmed code and physical observations

Build **93** on iPad 7 continued offline without reopening, but controls returned only after **10+ seconds**. The user reports smoother motion between jerks roughly every two seconds, with both tap and joystick. Dragged objects were already smooth.

`OutageClock.DelaySeconds=10` imposed a foreground wait after loss. It was an application policy, not a reconciliation requirement. **94 source removes this wait.** Already-local play stays local while discovery runs; a disconnected shared presentation triggers continuation. Foreground resume can continue immediately after known loss. Native checks must still measure loading and total detection-to-play latency.

The implemented reunion path never replays offline changes to the server. It saves the local branch separately, then presents the current server state.

## Actual iPad profile and limits

The PID-attached recording failed. A second all-process Time Profiler recording completed after the SSH timeout: **35.772590 seconds**, iPad 7 / iPadOS 18.7.10, LittleWeeps PID 1022 / build 93. Analysis filters to this app. Its UnityFramework dSYM UUID matches the captured binary: `38467A64-ACBD-3CEC-ACA1-168C10DCAD4B`.

There are **21,363 app CPU samples**, including **6,962 main-thread samples**. Symbols identify rendering, local Update/idle work, network diagnostics and Bonjour discovery. No sampled stack names contain save/checkpoint functions. **This recording does not establish autosaving as the cause.** It lacks synchronized walking input and file-wait measurements; inclusive stack counts overlap and are not frame timings.

94 adds bounded in-memory frame history: gap, local/shared state, walking state, position, collection counter, save duration and network-update duration. Export happens on Menu/pause or an isolated test request, using detached data on a worker. It adds no recurring diagnostic file write during walking. A frame's gap covers the preceding interval: work in frame N must be compared to the gap entering N+1. This measures the issue; **the hitch is not yet claimed fixed**.

## Qualification

Installed: iPad 7 **94**, iPad 9/iPhone **79**, Samsung **83**, authority/helper **91**. The 94 update succeeded. iPadOS initially denied launch with its signature/entitlements/profile-trust security message; the user reported that internet was required. A subsequent USB inspection verifies the actual **0.0.94** runtime, paired enrollment and loaded local adventure. The initial launch block has cleared; physical walking and repeated offline-launch acceptance remain pending.

Apple's [provisioning update documentation](https://developer.apple.com/help/account/provisioning-profiles/provisioning-profile-updates) describes first-launch PPQ certificate verification for development/ad-hoc apps on qualifying teams. This check happens before game code runs. Its portal-based offline-profile option is not established as available to our free Personal Team. Do not promise that editing gameplay removes Apple's signing checks, or that a successful running offline session proves every future offline cold launch. Check initial verification with internet after installation, then separately qualify Wi-Fi loss and offline cold reopening. [Personal Team expiry rules](https://developer.apple.com/help/account/basics/about-your-developer-account) remain a separate deployment constraint.

The 93 → 94 update followed a verified 299-file backup. Of 298 Documents files, 297 were byte-identical, including all ten world-save files; every existing preference was retained. The only changed document was the old app's live connection diagnostic: elapsed time, data age and one appended reconnect event. That exact difference was reviewed without reinstalling or resetting anything. [Signed update evidence](evidence/offline94-2026-09-25/ipad94-update.json).

Required checks: native held-touch/menu/missing-replica loss, foreground resume, unchanged server authority on reunion, retained iPad update, real Wi-Fi loss without reopening, and measured A10 offline walking. Then qualify newer iPad and phones, repeated loss/reunion and sustained mixed play. Keep G3-REC-05 open.

### Build 94 completed engineering checks

- [69 core/storage checks](evidence/offline94-2026-09-25/core94.json) pass, including immediate foreground eligibility and unchanged transaction rules.
- [Three native handoff cases](evidence/offline94-2026-09-25/handoff94.json) pass. Each asserts less than five seconds from authority exit to local presentation and less than two seconds from observed disconnection. These are Windows bounds, not physical iPad timings.
- [Five failure-boundary groups](evidence/offline94-2026-09-25/failure-boundaries94.json) pass. Known-loss foreground resume took **0.156 seconds**. Missing acknowledgment was not replayed; save failure did not interrupt siblings.
- [Six four-client continuation groups](evidence/offline94-2026-09-25/continuation94.json) pass. Offline-first play, cold reopen, separate saves, server-authoritative reunion and repeated outages retain their tested behavior.
- [Offline native Windows movement](evidence/offline94-2026-09-25/movement94.json) advances every sampled moving frame for both controls at a 60 fps target. This does not resolve the reported physical hitch.
- [Native iOS Release compilation/linking](evidence/offline94-2026-09-25/ios94-native.json) passes with all six Apple bridge symbols. Signing, in-place installation and runtime 94 identity now pass. The walking hitch remains unresolved pending a physical timing capture.

The exported Windows timing capture contains 1,614 frames, 673 moving offline frames and 14 measured saves, demonstrating that the new diagnostic covers the walking/save interval. Its timing values are not substituted for measurements on A10. The [93 feedback](evidence/offline94-2026-09-25/ipad93-feedback.json) and [scoped physical CPU profile](evidence/offline94-2026-09-25/ipad93-profile-summary.json) remain recorded separately.

## Build 94 physical feedback and focused follow-up

The user reports that Wi-Fi-loss controls now work well without restarting. No stopwatch duration was supplied, so this is a qualitative improvement, not a measured latency pass. **Walking still jerks every 1.5–2 seconds with both controls.** The transition also visibly jumps to older state. Neither defect is accepted.

Source review confirms `NetworkContinuation.TryLocalContinuation` reads `OpenRecoveryReplica().Last`, then `SoloScreen.TryContinue` creates the local adventure from that durable checkpoint. This can precede the currently displayed state. `NetworkGardenSession.Disconnected` also clears movement interpolation/anticipation history. These are distinct possible contributors to a transition jump; do not confuse either with a periodic hitch after local play starts.

A local continuation and a replacement shared host have different requirements. The former can fork the latest validated client state and remain private; the latter still needs a complete authoritative checkpoint and G4 election rules. Unity's documented server-authority model applies to connected shared play; it does not require a local fork to rewind to the last disk checkpoint. **Design direction:** retain the latest displayed world/character state at disconnection, release obsolete shared holds, and start an independent local world. Never upload that fork or replay its pending commands on reunion. Existing solo/adventure files stay accessible and must not be deleted or silently overwritten. This direction follows the user requirement; it is not a Unity-provided automatic feature or an implemented fix yet.

Before implementation, add regression coverage for a last visible pose newer than the disk checkpoint, changed room/item state, a held object, no complete checkpoint yet, separate prior solo saves, and reunion while a sibling remains online. Assert world/character continuity independently of server recovery correctness.

The save path is another concrete review target: local `SoloScreen.Update` calls synchronous `TrySaveNow` on a dirty one-second cadence; `ContinuationLibrary.Save` repeatedly serializes, validates and reads the full record; `CheckpointStore.Save` performs a forced file flush. Online client walking does not execute that local save path. [Apple's filesystem guidance](https://developer.apple.com/documentation/foundation/improving-performance-and-stability-when-accessing-the-file-system) documents main-thread blocking/hitches and coordinated asynchronous I/O; [Unity's serializer contract](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/JsonUtility.ToJson.html) permits background serialization of detached data. This establishes an architectural risk, **not measured causation for this iPad hitch**. If timing confirms it, use an ordered bounded writer over immutable snapshots, retain atomic validation/backup and observable failures, and test pause/quit/session-switch durability. Merely lengthening the autosave interval or smoothing over a stalled main thread is not an acceptable repair.

The first retrieved 94 timing file was exported at **20:09:33 UTC**, contained 982 frames and **zero moving samples**. It predates the requested walking export and is excluded from causal analysis. A fresh walking → Menu export has been requested. Do not report this empty movement interval as a successful or failed walking measurement.

## Measured walking cause and build 95 repair

The subsequent **20:18:45 UTC** export contains 4,096 frames and **1,908 offline moving intervals**, representing 33.42 seconds of movement. **All 26 walking gaps of at least 50 ms follow main-thread save calls longer than 20 ms.** The largest gap is **82.90 ms**, with a preceding **79.66 ms** save; median save time during walking is **52.87 ms**. The largest moving gap without a save is **32.74 ms**. This identifies the synchronous save path as the dominant recurring stall in this capture. It does not isolate serialization versus filesystem blocking versus garbage collection inside that path. [Sanitized measurements](evidence/offline94-2026-09-25/ipad94-walking-baseline.json).

The separate 90-second Instruments Time Profiler recording completed successfully. Its requested start was 20:18:26 UTC, so only the tail of the game capture can overlap it. Do not describe the entire 33.42-second game movement sample as simultaneously CPU-profiled. The frame/work measurement above supplies the actual stall attribution.

95 changes the architecture of periodic local saving:

- Copy a plain-data world snapshot on the gameplay thread; serialize, validate, hash and commit it on one background writer. Unity objects/UI remain on the gameplay thread, following [Unity's threading restrictions](https://docs.unity.com/en-us/engine/6000.3/manual/scripting/optimization/programming-best-practices) and its documented background JSON support.
- Permit only one in-flight write. Newer live changes remain dirty and are coalesced into the next snapshot; there is no unbounded task queue or overlapping writer to the same save.
- Poll completion during ordinary frames. Report “saved” only after completion, and return failed work to dirty state. At menu, pause, quit, destruction and session-switch boundaries, finish the previous write before committing newer state or changing saves.
- Keep existing validation, checksums, forced flush, atomic replacement and previous-good backup. Remove the redundant JSON round-trip that cloned the old continuation snapshot immediately before discarding it.

This applies [Apple's recommendation to avoid main-thread file I/O and coordinate asynchronous access](https://developer.apple.com/documentation/foundation/improving-performance-and-stability-when-accessing-the-file-system). The save interval and walking speed are unchanged. Lifecycle barriers may still take time; they are not presented as zero-latency operations. Sudden process termination can still lose work since the last completed commit, while recovery retains the last valid file.

**Scope:** 95 targets the measured periodic save stall. It does not repair the separate old-checkpoint transition jump. That remains the next bounded repair; do not mark G3-REC-05 complete or claim physical smoothness before comparing a fresh iPad capture.

95 engineering qualification passes: [72 core/storage checks](evidence/offline95-2026-09-25/core95.json), [five native failure-boundary groups](evidence/offline95-2026-09-25/failure-boundaries95.json), [six four-client continuation groups](evidence/offline95-2026-09-25/continuation95.json), [tap/joystick movement](evidence/offline95-2026-09-25/movement95.json), [410 matching runtime/package source files](evidence/offline95-2026-09-25/source-parity95.json), and [native Apple compilation/linking](evidence/offline95-2026-09-25/ios95-native.json). Tests include writer exclusion, failed-write retry with intact backup, lifecycle ordering, save-denied reunion, cold restart, independent saved adventures and authoritative reunion. The Mac signing check also passed; installation and physical comparison are the next gate. These are scoped engineering results, not an A10 smoothness pass. Background deserialization uses plain data under [Unity's FromJson contract](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/jsonutility/fromjson).

**95 installation:** completed after a verified 307-file backup. All 12 world-save files, 305 of 306 Documents files, and all existing preferences remain unchanged. Only the old live connection diagnostic advanced its time/data-age and appended one reconnect event; that exact difference was reviewed. There was no second installation or data reset. [Update evidence](evidence/offline95-2026-09-25/ipad95-update.json). Apple currently denies launch with its signature/entitlements/profile-trust message while the device is offline; signed artifact checks passed. Online developer verification and then actual runtime 95 inspection are required before the next walking comparison. This launch constraint is separate from the game's offline controls and must not be described as fixed by moving saves to a worker.

**Subsequent launch:** the user turned Wi-Fi on and opened the app successfully. USB inspection verifies **0.0.95**, paired enrollment and retained adventure selection. The initial launch block has cleared. A fresh offline walking → Menu capture is now requested; physical smoothness remains unqualified.


## Build 95 physical walking and offline reopening

The user reports **“menu open its way smoother!”** on the older iPad. Its 20:34:31 UTC recording contains 2,407 frames across local/shared/local play. The final offline segment begins at the last recorded shared-to-local transition, lasts 19.17 seconds, and contains 1,117 moving intervals (18.74 seconds). It has **zero walking gaps of at least 50 ms**; the largest is **28.57 ms**. Main-thread save work during that walking segment has a **0.185 ms median and 2.881 ms maximum**, versus the earlier 94 capture's 52.87 ms median and 79.66 ms maximum. This supports the save-stall repair on A10. It measures main-thread cost, not total worker disk-write duration. [Full scoped comparison](evidence/offline95-2026-09-25/ipad95-walking-comparison.json).

The full recording is retained in the comparison, including two earlier outliers. One continuous walking gap is **52.05 ms**, following **48.16 ms of network-update work** and no save; its exact network sub-operation still needs attribution. Another gap is **1.99 seconds** at a moving-to-stationary boundary with no position change. The capture lacks explicit lifecycle markers, so its cause is not established and it is not silently removed from full-capture statistics. The final Menu save costs 27.18 ms while stationary. These are not evidence of universally hitch-free play.

The user then confirmed that, with Wi-Fi still off, closing and reopening the app **opened successfully, retained toy positions, and allowed smooth walking**. A subsequent read-only USB inspection verifies runtime 95 in a new process, paired enrollment, local availability and readable checksummed saves, with both original solo files still present. This is a scoped offline cold-launch/save pass after online developer verification, not a guarantee against future Apple signing checks. [Cold-reopen evidence](evidence/offline95-2026-09-25/ipad95-cold-reopen.json).

**Current acceptance:** the measured periodic save hitch is substantially improved and this device's offline reopening passes. G3-REC-05 remains open. Next fix the separate rewind to an older checkpoint at Wi-Fi loss, using the last validated visible state as the private local starting point; never upload that fork to the server. Retain the smaller network-work spike for focused attribution and qualify the final changes on the newer iPad and phones before broader acceptance.


## Next repair in progress: continue the visible local world

The user authorized unattended engineering and replacement preparation after the 95 pass. Before editing, the current disconnect/save paths and [Unity client/server authority](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/terms-concepts/client-server.html) were reviewed again. [Unity client anticipation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/advanced-topics/client-anticipation.html) distinguishes visual state from authoritative state; that distinction matters at this boundary. Online documentation currently serves NGO 2.13.3; installed 2.13.2 remains pinned.

Application design: capture the latest validated received state and the local character/held prop positions actually displayed before disconnect callbacks clear interpolation and cancel input. Store a version-2 **private view origin**, explicitly separate from `RecoveryRecord`; no fake server checkpoint, receipt replay or upload. A first disconnect can therefore continue even before a full recovery replica arrives. Existing version-1 adventures remain readable, immutable origins remain checked, and the original solo saves stay separate. Use a fresh private identity for each outage and release stale holds. Clear abandoned transition selection when rejoining, so a later outage cannot reopen an earlier pending branch. The full shared checkpoint and mobile host-election requirements remain unchanged.

This is the game's design based on the authority/visual-state distinction and user requirements, not a feature Unity automatically provides. Acceptance requires native latest-room/pose/held-item continuity, no-replica loss, cold reopening, old-save compatibility, failure handling and server-authoritative reunion. Runtime device installation/acceptance will be recorded separately. Candidate build 96 is an engineering test; final matched platform artifacts will use a later build number.


The new native regression reproduces the old behavior on build 95 against an older server without recovery replication: the Creek visit rewinds to an earlier solo area. Candidate 96 exposed a serializer mismatch before deployment. [Unity serialization rules](https://docs.unity3d.com/6000.3/Documentation/Manual/script-serialization-rules.html) explain that inline custom classes cannot represent null and may become empty objects. The record version now selects the active origin; empty unused fields are accepted, but a v2 private view cannot claim a real recovery checkpoint. Provenance comparisons use the active format only. A specific regression covers this alongside legacy-save compatibility. Candidates 96/97 are engineering intermediates; build 98 is the current replacement candidate. No mobile device has received these candidates.


## Build 98 results and return handoff

**Implemented and tested:** disconnect now captures the latest received view and the displayed local character/held-object positions before pointer cancellation and interpolation cleanup. The private local branch retains the current area and item contents; stale holds are released. It does not load an older checkpoint or claim server-recovery authority. New outage records use version 2; existing version-1 adventures and solo files remain readable and separate. Reunion saves local work separately and loads the server world. These changes include the 95 background writer.

| Evidence | Result and limit |
| --- | --- |
| [Build 95 regression baseline](evidence/offline98-2026-09-25/rewind95-baseline.json) | Reproduces Creek-to-old-solo rewind when a full checkpoint is unavailable. |
| [76 core/storage checks](evidence/offline98-2026-09-25/core98.json) | Pass, including latest view/room/pose/content preservation, stale-hold release, origin immutability, old/new save coexistence and inline-serializer empty objects. |
| [Two native visible-state cases](evidence/offline98-2026-09-25/visible98.json) | Pass with current and older/no-replica servers. Actual transport loss while dragging retains the visible pose and toy, cold reopen succeeds and reunion never uploads the local drop. Local presentation starts about 2.6–2.7 seconds after authority termination in these Windows runs; physical Wi-Fi timing is not claimed. |
| [Three held-touch/menu/no-checkpoint cases](evidence/offline98-2026-09-25/handoff98.json) | Pass; no application ten-second wait or forced restart. |
| [Five failure-boundary groups](evidence/offline98-2026-09-25/failure-boundaries98.json) and [six four-client groups](evidence/offline98-2026-09-25/continuation98.json) | Pass, including blocked save/selection, unknown acknowledgment, sibling continuity, repeated outages and retained original solo saves. |
| [Tap and joystick movement](evidence/offline98-2026-09-25/movement98.json) | Every sampled moving frame advances at the 60 fps Windows target. Not a new A10 performance measurement. |
| [Matching source](evidence/offline98-2026-09-25/source-parity98.json) | 410 runtime/package files match across Windows, signed Android and Apple export. Later test-tool fixes do not change the runtime payload. |
| [Android emulator update](evidence/offline98-2026-09-25/android98-update.json) and [six recovery groups](evidence/offline98-2026-09-25/android98-recovery.json) | Exact signed 91 → 98 update, saves retained, Keystore admission with three Windows siblings, actual Home/resume, local play, cold restart and authoritative reunion all pass. This x86_64 AVD uses ARM translation. |
| [Android artifact inspection](evidence/offline98-2026-09-25/android98-artifact.json) | Identity/signature, ZIP and ELF LOAD alignment pass. The existing strict 16 KB RELRO gate still fails; native ARM64 16 KB compatibility is not certified. |
| [Apple native compile/link](evidence/offline98-2026-09-25/ios98-native.json) | Pass with all six native bridge symbols and iPad/iPhone device families. |
| [Physical deployment status](evidence/offline98-2026-09-25/deployment98-status.json) | No physical 98 installation yet. Unattended Mac signing fails at codesign with `errSecInternalComponent`; the prepared desktop helper targets iPad 7. Samsung's saved ADB address is unreachable. User/device setup remains on the return checklist. |

Test-tool corrections were required and retained: Android polling now waits for the **currently selected** local adventure instead of a historical continuation event after warm resume; the first run failed that stale-event assertion, then all six groups passed with the corrected observation. Emulator backup paths were shortened after `adb pull` hit Windows path length limits; the subsequent verified update preserved saved bytes. Neither correction alters gameplay or bypasses an install/data check.

**Next:** sign and install 98 in place, qualify the transition on iPad 7, then update the newer iPad and phones and run the focused four-player checks. Keep the smaller previously measured network-work spike and sustained device performance open. G3 is not complete, G4 iPad hosting/automatic switching is not implemented, and G5 rooms/creations/full item policies remain required. No live family server restart or update occurred. [Updated return checklist](return-checklist-ipad-lan-2026-09-24.html).
<!-- historical-record-end -->
