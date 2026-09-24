# Android and dedicated-server preparation — 23 September 2026

This bounded G1 task supports FAMILY-01/JOIN-01/NET-02 and retains TV-01 regression coverage. It establishes platform builds, not networking, mobile hosting or game content.

**Current result:** family-signed Android **14 → 15** is installed and verified on the Samsung's 4 KB configuration. Saved taps and video bookmark survived restart/update; the user confirmed physical touch/audio. Build 15 fixes the clipped title. The local recovery key signed the update; independent backup and native 16 KB qualification remain open. [Phone/signing evidence](android-signing-and-phone-2026-09-23.md). Windows Server 0.0.12 passes its native lifecycle checks; iPad save/update and remaining device checks are pending.

## What exists and what has passed

| Part | Actual evidence |
| --- | --- |
| Build runner | `Tools/Build-Foundation.ps1` explicitly selects Windows, iOS, Android or WindowsServer; uses the pinned editor; preserves numbered artifacts; writes source/artifact hashes and build summaries. |
| Android module/toolchain | Unity 6000.3.24f1 Android module; user-owned JDK 17.0.18+8, NDK r27c/27.2.12479018, CMake 3.22.1, SDK platform/build-tools/platform-tools 36 and command-line tools 16. |
| Tool-path regression | `Tools/Test-AndroidToolPaths.ps1` passed in the real Unity editor: both bundled defaults and valid custom paths restore after configuration. |
| First accepted build process | 0.0.11, Unity exit 0, zero build-summary errors/warnings, 35,698,953-byte APK. Non-development IL2CPP, ARM64, minimum API 26, target API 36. |
| APK inspection | `Tools/Test-AndroidArtifact.ps1` ran against the actual APK and its hash manifest. Core identity/signature checks pass. Overall inspection exits nonzero because the additional RELRO alignment check remains unresolved. |
| Signing/device qualification | Builds 14/15 use the pinned family key; the recovery copy signed 15. Samsung launch, touch/audio, video/restart and in-place update retained observed saves; pulled APK hashes match. Independent recovery and native 16 KB remain open. The original build-11 Debug certificate probe remains emulator-only. |
| Server build | Windows 64-bit Dedicated Server / Mono 0.0.12 built with zero summary errors/warnings and complete source/artifact manifests. `ServerBootstrap.unity` is the only scene. |
| Server test | `Tools/Test-ServerFoundation.ps1 -BuildNumber 12` passed actual readiness, three heartbeats, null graphics, zero cameras and cooperative shutdown with exit 0. Networking is not implemented. |
| Windows regression | 0.0.6 → 0.0.9 passed save/restart/update/video checks; 0.0.9 remains the selected preview. |

Evidence: [Android build](evidence/android-first-build-2026-09-23/build-summary.json), [APK inspection](evidence/android-first-build-2026-09-23/apk-inspection.json), [tool-path regression](evidence/android-first-build-2026-09-23/tool-paths.json), [Windows checks](g1-status.md#androidserver-build-preparation-and-windows-regression), [iPad launch](evidence/ipad-g1-2026-09-23/launch-after-trust.json).

**Latest profile check:** iPad/iOS, Android, Windows Server and Windows client now all have saved Build Profile assets and fresh explicit-profile builds (16, 17, 18 and 19 respectively). Server 18 lifecycle and Windows 9 → 19 save/video/update checks pass. Android 17's strict 16 KB compatibility gate remains open; phone 15 remains installed. The iPad export still needs native compilation and actual iPad tests. [Full profile evidence](build-profiles-2026-09-23.md). iPads are primary; Android is secondary. G1 is not complete. The first-build records below remain historical evidence.

## Defects caught during the first Android build

0.0.10 compiled successfully, then the build wrapper failed during cleanup. Unity's External Tools getter returned a bundled JDK path that did not exist; assigning that path back as a custom JDK caused an exception. The fix restores bundled defaults using Unity's documented null setter. The integration check exercised this exact case and custom paths; fresh 0.0.11 then completed with full manifests. Build 10 remains preserved as an incomplete process result, not substituted for 11.

The APK inspector initially expected the old `sdkVersion` label; the installed aapt2 36 tool actually emits `minSdkVersion`. The corrected parser verified the real minimum value of 26. It did not loosen the expected SDK requirement.

The extra page-size inspection follows [Google's current native-library guidance](https://developer.android.com/guide/practices/page-sizes#check-alignment). ZIP alignment and all seven libraries' ELF LOAD alignment pass at 16 KB. The RELRO-end calculation flags `libc++_shared.so`, `libgame.so`, `libil2cpp.so`, `libmain.so`, `libswappywrapper.so` and `libunity.so`; their exact headers are archived alongside the inspection JSON. This is an unresolved static finding, **not an observed crash** on the family's phone. Validate the check against the actual segment layout and a 16 KB runtime before deciding on a supported fix. Do not claim complete Android compatibility, silently suppress the finding, replace native libraries by hand, or change the pinned NDK/editor speculatively.

Raw evidence: `LocalData/Logs/build-Android-10-20260923-233847.log`, `build-Android-11-20260923-234651.log`, `LocalData/Verification/android-tools-56fca9f521b34479b8bf4a56c5eeb24f`, and `LocalData/Verification/android-40e115f759ca4675b047f95b0dcb2385`. Builds remain under `Builds/Android/G1-0.0.11`; only sanitized evidence is tracked.

## Server installation and first native run

The cached server installer matches the official 6000.3.24f1 release MD5 and a valid Unity Technologies SF signature. An earlier elevation attempt was canceled. The user subsequently confirmed approving the Windows prompt, and fresh inspection verified the installed `win64_server_nondevelopment_mono` module with file version 6000.3.24.5143451. No additional installation is needed. [Module evidence](evidence/server-g1-2026-09-23/module.json).

`Tools/Build-Foundation.ps1 -Target WindowsServer -BuildNumber 12` completed with Unity exit 0, zero build-summary errors/warnings and a non-development Dedicated Server / Mono artifact. `Tools/Test-ServerFoundation.ps1 -BuildNumber 12` then verified hashes, headless startup, three advancing heartbeats and cooperative shutdown with exit 0. Run ID: `5123c88f5b5449ad91cee48d248ac63d`. Records show build 0.0.12, null graphics, zero cameras and `networkingImplemented: false`. [Build](evidence/server-g1-2026-09-23/build-summary.json), [verification](evidence/server-g1-2026-09-23/verification.json), [ready](evidence/server-g1-2026-09-23/ready.json), [heartbeat](evidence/server-g1-2026-09-23/heartbeat.json), [stopped](evidence/server-g1-2026-09-23/stopped.json).

The test process is stopped. No network listener, firewall change, automatic service, discovery, shared world or host migration was added. `Install-ServerModule.cmd` remains a setup/recovery helper and should report the module already present. To repeat after relevant source changes, use fresh build numbers with the same build/test commands. No unchanged repeat run is needed now. Next Windows-side work is the Android compatibility finding; device save/update observations and saved platform UI profiles remain separate G1 work.

## Verified user-owned Android dependencies

Tools live at `C:\Users\sephi\AppData\Local\LittleWeeps\Toolchains\Unity-6000.3.24f1`, outside the administrator-owned editor and outside Git. `Tools/android-toolchain-6000.3.24f1.json` pins seven official archive URLs, sizes, SHA-256 values and extraction layouts. `Tools/Prepare-AndroidToolchain.py` verifies all archives before extraction, refuses differing existing files and registers exact SDK packages. A second preparation run passed. [Setup evidence](evidence/android-toolchain-2026-09-23/setup.json) and [SDK inventory](evidence/android-toolchain-2026-09-23/sdk-inventory.txt) describe the earlier tool-preparation stage; the newer APK results above supersede its then-missing module state.

The first Android build proved the configured custom paths can compile the application. The regression check also proves supported API restoration of bundled/default and custom paths. The build refuses to replace a configured custom signing key with the probe certificate. That refusal is code-reviewed, not a tested family-key workflow.

Stable private Android signing and the local recovery/update checks are now implemented and verified on the Samsung. Before family distribution, verify independent recovery storage and remaining device compatibility. No other project's signing material is used. Both iPads, mixed-device multiplayer, iPad hosting/recovery and offline play remain required; none is removed by this task.

Sources: [exact Unity release](https://unity.com/releases/editor/whats-new/6000.3.24f1), [Android dependency versions](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/supported-dependency-versions), [custom dependencies](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/customize-dependencies), [JDK setter and bundled defaults](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Android.AndroidExternalToolsSettings-jdkRootPath.html), [Dedicated Server build target](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-build.html). Runtime and artifact evidence above establishes what actually passed.
