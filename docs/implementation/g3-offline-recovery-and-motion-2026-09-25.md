# Offline controls and walking: research, fixes and qualification

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
September 25, 2026 · G3-REC-05 · goals NET-02 / AUTO-01 / JOIN-01 / FAMILY-01.

**Follow-up after the 93 device test:** the older iPad continued without reopening, but its 10+ second delay and periodic walking jerk remain open. The user clarified server-authoritative reunion without merging offline changes. [Current research and build 94](g3-offline-authority-and-hitch-review-2026-09-25.html) supersedes the pending-feedback and ten-second policy below; this report retains the 93 investigation and evidence.

## The reported problem

The latest physical report is **all four devices stop accepting controls after Wi-Fi is turned off until the app is closed and reopened; offline walking then looks choppy, while connected play is smooth**. Turning Wi-Fi back on reconnects successfully. This remains a physical qualification blocker. Earlier one-off iPad 7 recovery successes do not override that report. [Exact feedback](evidence/ipad91-2026-09-25/all-device-offline-feedback.json).

Installed during those tests: iPad 7 build 91, iPad 9/iPhone 79, Samsung 83, PC authority 91. The older clients lack the newer continuation implementation, but that does not explain away the reported build 91 failure. No replacement is qualified on a device simply because it builds.

## Research completed before the replacement edits

The provisional 92 walking changes preceded the user's research-first instruction. Further editing stopped while the following primary documentation and the installed package source were reviewed. Implementation then resumed with isolated reproductions. No provisional 92 device installation was performed.

| Primary source reviewed | Finding | Decision for this game |
| --- | --- | --- |
| [Unity per-frame updates](https://docs.unity3d.com/6000.3/Documentation/Manual/time-per-frame-updates.html), [unscaledDeltaTime](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Time-unscaledDeltaTime.html) | Motion must use elapsed time, with updates aligned to rendering. | Advance local walking each rendered frame using bounded unscaled delta time. Keep transactional item actions and durable saves separate. |
| [Mobile targetFrameRate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-targetFrameRate.html) | iOS/Android use the application target, ignoring vSyncCount; the achievable rate depends on refresh divisors and workload. Desktop software caps can have their own pacing artifacts. | Use a consistent 60 fps target and test motion at both 30 and 60. A target is not a measured device guarantee. The previous two initializers requested different rates, so the physical old rate cannot be inferred from one assignment. |
| [Transport 2.7 timeout configuration](https://docs.unity3d.com/Packages/com.unity.transport@2.7/api/Unity.Networking.Transport.NetworkConfigParameter.html) | Inactivity detection emits a disconnect; heartbeats prevent an otherwise quiet connection timing out. | Retain 2,500 ms disconnect/400 ms heartbeat configuration. A transport timeout does not itself restore the local game presentation. |
| [NGO NetworkManager lifecycle](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/components/core/networkmanager.html) | Shutdown is asynchronous. Unexpected disconnections must be handled by the application. | Retain the durable world outside network objects, archive uncertain requests, and let discovery retry without keeping input dependent on a dead authority. Check shutdown completion before starting another client. |
| [Input System focus changes](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/device-background-focus-changes.html), [background behavior](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/background-behavior.html) | Mobile input is tied to foreground state. Resuming synchronizes or resets device state; canceled input must not remain an active gesture. | Cancel game-owned gestures at connection/lifecycle boundaries. A finger still physically pressed may delay joining live play, but must not indefinitely block recovery from already-canceled shared input. Do not globally reset every input device as a workaround. |
| [OnApplicationPause](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html), [internetReachability](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-internetReachability.html) | Pause/focus callbacks vary by platform; a reported network interface does not prove a route to this server. | Exercise background/resume separately from server loss. Keep transport/application evidence authoritative; do not treat cellular connectivity or a Wi-Fi icon as proof the family server is reachable. |
| [Unity UI optimization](https://unity.com/how-to/unity-ui-optimization-tips), [managed memory](https://docs.unity3d.com/6000.3/Documentation/Manual/performance-managed-memory-introduction.html) | Canvas rebuilds and allocations can contribute CPU/GC cost. | Avoid full-screen rendering and transaction-receipt creation for each local walking step. Measure remaining allocation/canvas cost rather than importing another networking package. |
| [Unity File Access Profiler](https://docs.unity3d.com/6000.3/Documentation/Manual/profiler-file-access-module.html), [Apple responsiveness](https://developer.apple.com/documentation/xcode/improving-app-responsiveness), [Apple disk writes](https://developer.apple.com/documentation/xcode/reducing-disk-writes) | File operations can occupy the main thread; unnecessary writes increase cost. | Preserve atomic checksummed saves and read-back verification. Avoid rewriting unchanged solo saves. Profile actual offline autosave cost before designing a background writer; never remove durability to hide a hitch. |
| [Apple game performance analysis](https://developer.apple.com/documentation/xcode/analyzing-the-performance-of-your-metal-app), [Android frame pacing](https://developer.android.com/games/sdk/frame-pacing) | Frame timing needs platform measurements; Android pacing is integrated with supported Unity versions. | Qualify the A10 iPad first, then the remaining devices. A Windows motion trace is useful regression evidence, not an iPad frame-time or thermal measurement. |

Version scope: Unity 6000.3.24f1, Input System 1.20.0, installed NGO 2.13.2 and embedded Transport 2.7.4 patch 1. The online NGO 2.13 documentation currently resolves to 2.13.3; the relevant shutdown/disconnect behavior was also checked in the installed 2.13.2 package documentation. No package upgrade is part of this fix.

## Findings in our code and tests

**Confirmed cadence defect:** the offline controller used a `now + 1/30` deadline and skipped rendering frames before that deadline. A native Windows 91 reproduction advanced the character in about 33–35% of observed frames, bunching movement despite an average walking speed near the intended 210 units/second. Objects being dragged did not use that gate. This explains a code-level difference between smooth dragging/networked presentation and uneven local walking; it is not a claim that we have measured every device's frame timings.

**Confirmed blocked transition:** native UGUI touch held across isolated authority loss keeps the old session on screen beyond the recovery threshold in candidate 92. The shared input is canceled, but the session-switch guard still checks raw pressed fingers. An open menu also excludes automatic recovery, and missing complete checkpoints return without detaching a previously shared presentation. These are distinct paths to a dead shared screen. Our earlier tests released the held input before checking recovery and therefore missed that condition.

**Save regression explained:** the failed candidate 92 cold-resume test retained all toys, players and revision, but an unnecessary load/serialize/save changed an idle timer from `3.1677519013883284` to `3.167751901388329`. The retained main and backup files differ in this timer precision. The fix keeps clean solo saves untouched, while restored interrupted holds still count as changes that must be saved. The exact-preservation assertion remains in place; it is not replaced with a permissive comparison.

**Still hypotheses:** a physical lifecycle event, sustained file-I/O stalls, or another device-specific input problem could contribute. Recovery checkpoint writes of 25–40 ms were sampled on the older iPad; those are not offline-autosave measurements. Do not claim async saving, a faster server, a new transport, or a fixed update rate would cure the remaining physical issue without measurements.

## Replacement behavior

1. Walk locally every rendered frame using the same speed and bounds as shared motion. Only avatar position/depth change during walking; ordinary item rules still use acknowledged transactions.
2. After the existing ten foreground seconds of disconnection, canceled shared gestures cannot hold recovery hostage. Live reunion still waits for real local gestures, pending commands and a successful save.
3. Preserve an open menu during recovery. Returning to play then operates the local world.
4. If there is a complete verified checkpoint, create a separate saved offline adventure. If none exists, reopen the existing solo world while reconnect attempts continue. Never fabricate a complete world from an incomplete network view.
5. Keep the family authority's world canonical when reunited. Retain offline work separately, do not replay uncertain pours, and do not overwrite the original solo save just by opening an adventure.

## Acceptance and current limits

Candidate 92 is superseded, not deployed. **Replacement 93 passes Windows qualification, native iOS compilation/signing and a retained iPad 7 update.** The older iPad runtime reports 0.0.93 and has rejoined with a durable full checkpoint. Human Wi-Fi-loss/movement checks remain pending. Keep **G3-REC-05 open** until actual Wi-Fi-off tests pass on the replacement devices. [Qualification summary](evidence/offline93-2026-09-25/qualification93.json).

| Measured check | Result / evidence |
| --- | --- |
| Offline 91 baseline | Tap 34.9% / joystick 33.1% of moving frames advanced; [failing cadence reproduction](evidence/offline93-2026-09-25/motion91-baseline.json). |
| Offline 93, 30 fps target | Both controls advanced on every sampled moving frame; mean speeds 210.28 / 210.14 units/s. [Metrics](evidence/offline93-2026-09-25/motion93-30fps.json). |
| Offline 93, 60 fps target | Both controls advanced on every sampled moving frame; mean speeds 210.09 / 210.14 units/s. [Metrics](evidence/offline93-2026-09-25/motion93-60fps.json). |
| Held touch, menu, no complete checkpoint | All three [baseline 92 cases fail](evidence/offline93-2026-09-25/handoff92.json); all three [replacement 93 cases pass](evidence/offline93-2026-09-25/handoff93.json), including durable local interaction and automatic reunion. No-checkpoint case uses authority 85. |
| Save preservation and recovery | [69 core/storage checks](evidence/offline93-2026-09-25/core93.json), [six four-client continuation groups](evidence/offline93-2026-09-25/continuation93.json) and [five failure-boundary groups](evidence/offline93-2026-09-25/failure-regressions93.json) pass. The [92 decimal rewrite diagnosis](evidence/offline93-2026-09-25/save92-diagnosis.json) remains recorded. |
| Four-player shared movement | Every sampled moving frame advanced; p95 visual speed 250.82 units/s, p95 visual lag behind received state 0.207 seconds. Menu stopped authority walking. [Native canvas regression](evidence/offline93-2026-09-25/shared-four93.json). This lag metric is not end-to-end network latency. |
| iOS Release compilation/linking | Verified 3,093 export files before/after transfer; native compile/link and six bridge symbols pass. Signing/install not implied. [Native check](evidence/offline93-2026-09-25/ios93-native.json) · [Build artifact hashes](evidence/offline93-2026-09-25/artifacts93.json). |

All movement metrics above come from isolated native Windows clients, not physical phones/tablets. The tests used separate enrolled worlds and did not kill or reset the live family server.

- Core walking/state/storage rules, exact solo retention and interrupted-hold recovery.
- Native offline tap and joystick walking at 30 and 60 fps targets; verify continuous canvas movement, speed, stopping and menu cancellation.
- Held touch, open menu, and older authority without full replicas; recover without killing the app and automatically rejoin.
- Four-client continuation, cold reopen, repeated outage and distinct adventures, denied writes, uncertain acknowledged outcomes and lifecycle-adapter regression tests.
- On iPad 7 first: Wi-Fi off in Settings and Control Center, held joystick/bucket, menu open, background/resume, repeat off/on, local movement and saved work. Then iPad 9, Samsung and iPhone with exact build numbers recorded.
- Measure A10 frame timing, save cost and sustained behavior if visible stutter remains. Do not close the physical report based on desktop evidence alone.

This work preserves the full goal sheet: four mixed-device players, independent travel, item ownership, required future iPad hosting/handoff and explicit reunion rules. It does not implement G4 host election or G5 merging.

## Actual deployment of the replacement

The older iPad **91 → 93** update is installed and launched. A **292-file** backup was verified on Mac and Windows first. Installation retained **eight world-save/backup files and every prior preference**. The installer's stricter all-document comparison stopped because one running-process connection log updated four timing/network counters between backup and installation: **290 of 291 Documents files were byte-identical**. The exact observed log difference was reviewed; no other document changed, no second installation occurred and no data was reset. [Signed build](evidence/offline93-2026-09-25/ios93-signed.json) · [Retention review](evidence/offline93-2026-09-25/ipad7-update93.json).

The actual app reports **0.0.93**, retained Keychain pairing, successful family admission, and a durable complete checkpoint with 128 receipts; the previous offline adventure is still present. Its one sampled checkpoint write was 14.5921 ms, not a sustained performance measurement. [Runtime evidence](evidence/offline93-2026-09-25/ipad7-initial93.json).

Android 93 is prepared with the existing family signature; identity/signature and ZIP/LOAD alignment pass. **Strict 16 KB RELRO still fails**, as before; no phone installation or native 16 KB qualification is claimed. [Artifact check](evidence/offline93-2026-09-25/android93-inspection.json). **169 runtime/plugin/package source files match across the Windows, iOS and Android builds and the current checkout.** [Source parity](evidence/offline93-2026-09-25/source-parity93.json).

The family authority/helper stays on 91 with healthy recovery. iPad 9/iPhone remain 79 and Samsung remains 83. Physical acceptance and coordinated remaining client updates follow; do not infer their older applications contain these fixes.
<!-- historical-record-end -->
