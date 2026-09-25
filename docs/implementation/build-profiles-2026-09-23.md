# G1 saved build profiles — iPad first

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
23 September 2026 (local time; evidence timestamps use UTC).

The two iPads are the primary devices; Android is secondary. The older A2197 iPad 7 remains the minimum performance baseline. This task supports the G1 build route for FAMILY-01 and TV-01; it implements no multiplayer or game content.

## What changed

Unity 6000.3.24f1's Build Profiles window created three new assets: **iPad Foundation**, **Android Foundation** and **Windows Server Foundation**. They join **Windows Foundation** in `Unity/FamilyPlayset/Assets/BuildProfiles`. The iPad profile is an iOS device build supporting both iPad and iPhone.

Every foundation build now passes its saved profile explicitly to `BuildPipeline.BuildPlayer`. Before building, it verifies platform/subtarget, the single intended scene, non-development settings, shared Player Settings/defines and APK output where applicable. It refuses a wrong profile instead of silently fixing or using the last selected platform. The build result independently checks platform, development status and Windows player/server subtarget. The PowerShell wrapper checks the profile recorded in the result and now hashes iOS exports too.

The three client profiles contain `Bootstrap.unity`; the dedicated server contains only `ServerBootstrap.unity`. Android's normal Unity output remains an intermediate; `Build-AndroidFamily.ps1` signs an unchanged copy with this game's protected family key. Signing secrets remain outside the project and profile assets.

Unity's server-profile creation installed its required **Dedicated Server 2.0.2** package and generated role/content-selection settings. The manifest and lockfile pin it. No automatic component stripping was enabled. Build logs report Client for the current fixtures and Server for the headless fixture. These labels do not implement networking. Before G3 mobile-host qualification, explicitly audit both client and server code/content availability on the iPads; mobile hosting and automatic recovery remain required.

Implementation was checked against the installed editor API and [Unity's BuildPlayerWithProfileOptions documentation](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPlayerWithProfileOptions.html). Unity 6000.3 does not expose public BuildProfile target/settings getters, so validation reads the observed serialized fields and fails if their schema changes. Profiles were created through the supported UI, not fabricated from assumed YAML.

## Actual results

| Check | Observed result |
| --- | --- |
| Profile preflight | All four profiles passed. In-memory clones with a wrong target, missing bootstrap scene or development flag were rejected. Compilation console had no errors/warnings. [Record](evidence/build-profiles-2026-09-23/profile-validation.json). |
| iPad/iOS 0.0.16 | Export succeeded first, non-development, zero summary errors/warnings. All 3,026 exported files matched their recorded hashes. Xcode project includes the game bundle ID, iPhone/iPad families, ARM64 and iOS 15.0 minimum. [Build](evidence/build-profiles-2026-09-23/ios-build-summary.json), [export check](evidence/build-profiles-2026-09-23/ios-export-check.json). This is not a native Xcode compile or device install. |
| Android 0.0.17 | Saved-profile build and family signing succeeded with zero summary errors/warnings. Identity, certificate, ARM64, SDK 26/36, non-debuggable status and ZIP/ELF LOAD alignment passed. [Build](evidence/build-profiles-2026-09-23/android-build-summary.json), [signing](evidence/build-profiles-2026-09-23/android-signing.json). |
| Android strict compatibility check | Still fails on the six previously tracked raw RELRO-end alignment findings. No new native 16 KB device qualification was performed. The gate is not waived. [Inspection](evidence/build-profiles-2026-09-23/android-inspection.json), [prior diagnostic explanation](android-16kb-review-2026-09-23.md). Build 17 was not installed; the phone retains tested build 15 and its data. |
| Windows Server 0.0.18 | Non-development dedicated-server build succeeded with zero summary errors/warnings. Verified startup, three advancing heartbeats, null graphics, zero cameras and cooperative exit 0. [Build](evidence/build-profiles-2026-09-23/server-build-summary.json), [ready](evidence/build-profiles-2026-09-23/server-ready.json), [heartbeat](evidence/build-profiles-2026-09-23/server-heartbeat.json), [stopped](evidence/build-profiles-2026-09-23/server-stopped.json). Networking remains unimplemented. |
| Windows 0.0.19 | Fresh saved-profile build succeeded with zero summary errors/warnings. The native 9 → 19 seed/restart/update check passed save identity, tap count, local-video decoding and bookmark retention. [Build](evidence/build-profiles-2026-09-23/windows-build-summary.json), [seed](evidence/build-profiles-2026-09-23/windows-seed.json), [restart](evidence/build-profiles-2026-09-23/windows-resume.json), [update](evidence/build-profiles-2026-09-23/windows-update.json). |

Full build/source/artifact manifests remain under `Builds/<platform>/G1-0.0.N`; raw logs stay in `LocalData/Logs`. These are foundation fixtures, not a finished game. No Mac or mobile-device actions were required for this task.

## Next gate

Prioritize the actual iPads: compile/sign export 16 using the established Mac route, update the iPad 9 in place and observe touch/audio, video and saved values; then install and run the same foundation on the older iPad 7. Start and observe the agreed free-provisioning refresh route. Independent recovery backup and native Android 16 KB qualification remain open. G1 is still in progress; iPad performance, hosting/recovery and the final save system cannot be certified from these desktop builds.
<!-- historical-record-end -->
