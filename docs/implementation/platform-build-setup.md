# Android and dedicated-server preparation — 23 September 2026

This is the current bounded G1 build task. It supports later FAMILY-01/JOIN-01/NET-02 and retains TV-01 regression coverage. It does not implement networking, mobile hosting or game content.

## What exists

- `Tools/Build-Foundation.ps1` accepts Windows, iOS, Android and WindowsServer. It uses the exact project editor, stops when required modules are missing, selects a target explicitly, preserves numbered outputs, and writes source/artifact evidence. A build-runner hang was fixed by waiting for Unity itself rather than its persistent Roslyn compiler child process.
- Android configuration: IL2CPP, ARM64, minimum API 26, target API 36, APK output and non-development player. This initial **build-only probe uses the default Android debug certificate**. It is not an approved family release and must not be used as a shortcut around establishing a stable private signing key and testing in-place updates. No APK exists yet.
- WindowsServer configuration: Windows 64-bit Dedicated Server subtarget, Mono, non-development, `ServerBootstrap.unity` only. The scene has one server component and no camera/UI/video player. The server assembly is excluded from ordinary client players.
- `Tools/Test-ServerFoundation.ps1` checks the exact artifact hashes, launches its own headless process in a unique evidence directory, waits for three heartbeats, requests cooperative shutdown, and checks build identity, null graphics and zero cameras. Failures stop the test; timeout cleanup targets only the process it created. **This script has passed PowerShell syntax validation, not a native server run.**
- Server readiness records explicitly say `networkingImplemented: false`. There is no listener, firewall change, auto-start service, discovery, host migration or shared-world implementation.

## Actual acceptance evidence

Windows **0.0.9** built with zero reported errors/warnings and the saved Windows Foundation profile. `ServerBootstrap.unity` was authored through Unity's scene API; ordinary Windows output contains `LittleWeeps.Core.dll` and `LittleWeeps.Runtime.dll`, excluding the server assembly.

Run `b18b274af9c4413da4fd162155fc692b` seeded 0.0.6, restarted it, then launched 0.0.9. All phases passed with the same profile `41b1086505c040e98d2f4cb0d5987c51`, 3 saved taps, a 4-second video bookmark and at least 15 decoded frames. [Build](evidence/windows-platform-setup-2026-09-23/windows-build.json), [seed](evidence/windows-platform-setup-2026-09-23/seed.json), [restart](evidence/windows-platform-setup-2026-09-23/resume.json), [new version](evidence/windows-platform-setup-2026-09-23/update.json).

Raw logs and full source/artifact manifests are under `LocalData/Logs`, `LocalData/Verification` and `Builds/Windows/G1-0.0.9`. Build 0.0.7 compiled, but its wrapper stalled waiting for the compiler child and did not finish its manifests. Build 0.0.8 exposed Unity's restriction on adding a scene beside an unsaved untitled scene; the batch scene setup was corrected. Neither is the selected verified preview. `Play-Foundation.cmd` now selects 0.0.9.

## Installation blocker and exact next work

The cached Android and Windows Server installers both matched the official Unity 6000.3.24f1 release metadata MD5 and had valid Unity Technologies SF Authenticode signatures. The Android installer launch returned a canceled operation from Windows; no platform module was installed. The server installer was not attempted after that failure. Do not describe this as a compiler or game-code failure, a successful install, or an automatic approval-review rejection.

The intended editor remains **6000.3.24f1 / 4e7b9b5b6244**. Do not copy platform modules from another patch or switch the project to the older installed editor to get around the missing module.

For the next session when Windows installation can be completed:

1. Save/close this Unity project. Install Android Build Support and Windows Dedicated Server Support for the exact editor. The cached installers are under `%APPDATA%\UnityHub\downloads`; the normal Windows installer may require local elevation. Hub previously refused `install-modules` for the manually recovered editor, so inspect that state before retrying it.
2. Complete and verify the matching Android dependencies: OpenJDK 17 (release archive 17.0.18+8), NDK r27c / 27.2.12479018, SDK build-tools 36.0.0, platform-tools 36.0.0, command-line tools 16.0, platform 36 and CMake 3.22.1. Archives are already cached. One unrelated cached platform-37 archive is incomplete; do not use it or select the newest platform implicitly. Current build configuration pins API 36.
3. Create saved Android and Windows Server UI Build Profiles. The current commands use explicit build configurations; only the Windows client currently has a saved profile asset.
4. Run `Tools/Build-Foundation.ps1 -Target Android -BuildNumber N` and `Tools/Build-Foundation.ps1 -Target WindowsServer -BuildNumber M` with unused numbers. Inspect the APK with `aapt2`/`apksigner`: package, version, min/target SDK, ARM64 libraries, debuggable flag and actual signing certificate. Archive the observed results; do not infer them from settings alone.
5. Run `Tools/Test-ServerFoundation.ps1 -BuildNumber M` and require readiness, advancing heartbeats and clean shutdown. This proves process setup only. Multiplayer acceptance belongs to later gates.
6. Before device qualification, establish the separate game's stable private Android signing key outside Git and verify recovery storage. Build the intended family-signed artifact, then test install/update and data retention on the real device when available. The G1 probe's default certificate is not that release identity.

Mac, iPad and Android device installation were not used in this task. Physical touch, mobile video behavior, mobile save retention, older-iPad performance, server execution and all multiplayer requirements remain unqualified.

Implementation sources: [Unity's version-specific Android dependencies](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/supported-dependency-versions), [Unity Dedicated Server build target](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-build.html), [Unity Android build settings](https://docs.unity3d.com/6000.3/Documentation/Manual/android-build-settings.html), [exact editor release](https://unity.com/releases/editor/whats-new/6000.3.24f1). The settings and workflow are our implementation; only the acceptance evidence above establishes tested behavior.
