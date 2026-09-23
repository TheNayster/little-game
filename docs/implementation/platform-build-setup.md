# Android and dedicated-server preparation — 23 September 2026

This bounded G1 task supports FAMILY-01/JOIN-01/NET-02 and retains TV-01 regression coverage. It establishes platform builds, not networking, mobile hosting or game content.

**Current result:** the matching Android Unity module is installed. Fresh Android **0.0.11** built successfully with complete manifests. APK identity, SDK levels, ARM64, signature, ZIP alignment and ELF load alignment checks pass. A stricter 16 KB RELRO-end check flags six native libraries and remains unresolved; the APK has not run on an Android device. The separate Windows Server module is still absent after Windows canceled its administrator prompt before installation began. The iPad trust issue is resolved and a launch command succeeded; physical tap/save observations remain pending.

## What exists and what has passed

| Part | Actual evidence |
| --- | --- |
| Build runner | `Tools/Build-Foundation.ps1` explicitly selects Windows, iOS, Android or WindowsServer; uses the pinned editor; preserves numbered artifacts; writes source/artifact hashes and build summaries. |
| Android module/toolchain | Unity 6000.3.24f1 Android module; user-owned JDK 17.0.18+8, NDK r27c/27.2.12479018, CMake 3.22.1, SDK platform/build-tools/platform-tools 36 and command-line tools 16. |
| Tool-path regression | `Tools/Test-AndroidToolPaths.ps1` passed in the real Unity editor: both bundled defaults and valid custom paths restore after configuration. |
| First accepted build process | 0.0.11, Unity exit 0, zero build-summary errors/warnings, 35,698,953-byte APK. Non-development IL2CPP, ARM64, minimum API 26, target API 36. |
| APK inspection | `Tools/Test-AndroidArtifact.ps1` ran against the actual APK and its hash manifest. Core identity/signature checks pass. Overall inspection exits nonzero because the additional RELRO alignment check remains unresolved. |
| Signing/device qualification | This build-only probe uses the default Android Debug certificate. No phone install, actual touch, mobile video or update/save retention is qualified. Stable family signing and recovery remain separate gates. |
| Server source | Dedicated Server target configuration and `ServerBootstrap.unity` exist. The server assembly is excluded from ordinary client builds. No native server artifact has been built. |
| Server test | `Tools/Test-ServerFoundation.ps1` is syntax-checked, not runtime-qualified. It requires readiness, three heartbeats, null graphics, zero cameras and cooperative shutdown. |
| Windows regression | 0.0.6 → 0.0.9 passed save/restart/update/video checks; 0.0.9 remains the selected preview. |

Evidence: [Android build](evidence/android-first-build-2026-09-23/build-summary.json), [APK inspection](evidence/android-first-build-2026-09-23/apk-inspection.json), [tool-path regression](evidence/android-first-build-2026-09-23/tool-paths.json), [Windows checks](g1-status.md#androidserver-build-preparation-and-windows-regression), [iPad launch](evidence/ipad-g1-2026-09-23/launch-after-trust.json).

Only Windows has a saved UI Build Profile asset. Android/server commands currently use explicit build configurations; their saved UI profiles remain to be created. G1 is not complete.

## Defects caught during the first Android build

0.0.10 compiled successfully, then the build wrapper failed during cleanup. Unity's External Tools getter returned a bundled JDK path that did not exist; assigning that path back as a custom JDK caused an exception. The fix restores bundled defaults using Unity's documented null setter. The integration check exercised this exact case and custom paths; fresh 0.0.11 then completed with full manifests. Build 10 remains preserved as an incomplete process result, not substituted for 11.

The APK inspector initially expected the old `sdkVersion` label; the installed aapt2 36 tool actually emits `minSdkVersion`. The corrected parser verified the real minimum value of 26. It did not loosen the expected SDK requirement.

The extra page-size inspection follows [Google's current native-library guidance](https://developer.android.com/guide/practices/page-sizes#check-alignment). ZIP alignment and all seven libraries' ELF LOAD alignment pass at 16 KB. The RELRO-end calculation flags `libc++_shared.so`, `libgame.so`, `libil2cpp.so`, `libmain.so`, `libswappywrapper.so` and `libunity.so`; their exact headers are archived alongside the inspection JSON. This is an unresolved static finding, **not an observed crash** on the family's phone. Validate the check against the actual segment layout and a 16 KB runtime before deciding on a supported fix. Do not claim complete Android compatibility, silently suppress the finding, replace native libraries by hand, or change the pinned NDK/editor speculatively.

Raw evidence: `LocalData/Logs/build-Android-10-20260923-233847.log`, `build-Android-11-20260923-234651.log`, `LocalData/Verification/android-tools-56fca9f521b34479b8bf4a56c5eeb24f`, and `LocalData/Verification/android-40e115f759ca4675b047f95b0dcb2385`. Builds remain under `Builds/Android/G1-0.0.11`; only sanitized evidence is tracked.

## Remaining server installation

The user completed the Android installer; inspection confirms `AndroidPlayer` is present. Windows Dedicated Server is a separate module. Its cached installer matches the official 6000.3.24f1 release MD5 and a valid Unity Technologies SF signature. A silent launch with ordinary Windows elevation returned “The operation was canceled by the user” before starting. This is not a compiler failure or an automatic approval-review rejection. No elevation prompt or installer is left running.

When available, close Unity and double-click **`Install-ServerModule.cmd`** in the game root. This helper verifies the exact version, checksum and publisher before asking Windows to run the standard installer. Click Yes on Windows' prompt. The helper checks the expected server module file afterward. Its syntax is verified; its successful installation path has not yet run. It neither launches a game server nor changes firewall rules.

Next, build using an unused number: `Tools/Build-Foundation.ps1 -Target WindowsServer -BuildNumber N`, then `Tools/Test-ServerFoundation.ps1 -BuildNumber N`. A passing result establishes the dedicated process lifecycle only. Source readiness records explicitly state `networkingImplemented: false`; there is no network listener, discovery, shared world or host migration yet.

## Verified user-owned Android dependencies

Tools live at `C:\Users\sephi\AppData\Local\LittleWeeps\Toolchains\Unity-6000.3.24f1`, outside the administrator-owned editor and outside Git. `Tools/android-toolchain-6000.3.24f1.json` pins seven official archive URLs, sizes, SHA-256 values and extraction layouts. `Tools/Prepare-AndroidToolchain.py` verifies all archives before extraction, refuses differing existing files and registers exact SDK packages. A second preparation run passed. [Setup evidence](evidence/android-toolchain-2026-09-23/setup.json) and [SDK inventory](evidence/android-toolchain-2026-09-23/sdk-inventory.txt) describe the earlier tool-preparation stage; the newer APK results above supersede its then-missing module state.

The first Android build proved the configured custom paths can compile the application. The regression check also proves supported API restoration of bundled/default and custom paths. The build refuses to replace a configured custom signing key with the probe certificate. That refusal is code-reviewed, not a tested family-key workflow.

Before family distribution: establish a separate stable private Android signing key outside Git, verify recovery storage, resolve device compatibility, and prove an in-place signed update preserves observed saved data. No other project's signing material is used. Both iPads, mixed-device multiplayer, iPad hosting/recovery and offline play remain required; none is removed by this task.

Sources: [exact Unity release](https://unity.com/releases/editor/whats-new/6000.3.24f1), [Android dependency versions](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/supported-dependency-versions), [custom dependencies](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/customize-dependencies), [JDK setter and bundled defaults](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Android.AndroidExternalToolsSettings-jdkRootPath.html), [Dedicated Server build target](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-build.html). Runtime and artifact evidence above establishes what actually passed.
