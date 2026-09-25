# Mobile recovery builds prepared on Windows

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**G3-REC-04 · NET-02 / AUTO-01 · September 25, 2026 · build 91.** A fresh family-signed Android release and matching iPad/iPhone Xcode export are prepared. Both contain the recovery code already tested in Windows 91. **Nothing was installed, and the Mac, physical devices and family authority were not accessed or changed.** Last recorded deployments remain PC/Samsung 83 and iPads/iPhone 79.

## What is ready

| Deliverable | Result and evidence |
| --- | --- |
| Android ARM64 release | Unity 6000.3.24f1, non-development build 0.0.91; zero errors/warnings. Existing `com.littleweeps.familyplayset` identity, min API 26 / target API 36 and pinned family signer retained. [Build](evidence/mobile-recovery-builds-2026-09-25/android-build-91.json), [signing](evidence/mobile-recovery-builds-2026-09-25/android-signing-91.json), [artifact hash](evidence/mobile-recovery-builds-2026-09-25/android-artifact-91.json). |
| iPad/iPhone Xcode export | Non-development 0.0.91 export; zero errors/warnings and 3,094 artifact files hash-verified. iOS 15 minimum, ARM64, both device families and the existing landscape orientation retained. **Native Mac compile/link, signing and installation are pending.** [Export](evidence/mobile-recovery-builds-2026-09-25/ios-export-91.json). |
| Matched game code | All **148 C# files** match the qualified Windows 91 manifest and current source. Native iOS/Android bridge sources and both package files match across all three builds. No runtime code changed in this task. [Artifact/source inspection](evidence/mobile-recovery-builds-2026-09-25/mobile-inspection-91.json). |
| Native bridge preparation | Android's actual DEX contains both family discovery/enrollment classes. The iOS export has the unchanged Objective-C++ bridge, ARC build configuration, Security framework, Bonjour service and local-network explanation. Generated IL2CPP includes checkpoint replication, local continuation and interrupted-archive retries. This checks export inclusion, **not successful iOS linking or device behavior**. |
| Return checklist | Updated with specific setup steps, older-iPad-first testing, signing prompts, independent backup choice and renewal follow-up. [Your to-do list](return-checklist-ipad-lan-2026-09-24.html). |

Local prepared files:

- Android: `Builds/AndroidSigned/G3-0.0.91/LittleWeeps.apk`.
- iPad/iPhone: `Builds/iOSFamilyLAN/G3-0.0.91/Xcode/`.
- Matching Windows client/authority: `Builds/NetworkProbe/G3-0.0.91/`.

These generated files and signing material stay outside Git. The [preparation record](evidence/mobile-recovery-builds-2026-09-25/preparation.json) retains manifest hashes and helper provenance. Windows was compiled before its prior commit; the mobile builds reference that committed change. Their differing source-commit labels do not imply different game code: the exact C# and native-bridge bytes were compared.

## Android qualification still open

The [strict Android inspection](evidence/mobile-recovery-builds-2026-09-25/android-inspection-91.json) **failed its RELRO 16 KB check in five native libraries**. Identity/signature, ARM64 ABI, non-debuggable status, SDK levels, ZIP alignment and ELF LOAD alignment passed. The failure is retained as `passed: false`; no library was patched and no gate was weakened.

The existing [bounded geometry diagnostic](evidence/mobile-recovery-builds-2026-09-25/android-relro-91.json) found no declared writable-range overlap issues in the seven libraries, consistent with the earlier [16 KB investigation](android-16kb-review-2026-09-23.md). That does not override the strict result or prove native ARM64 16 KB compatibility. The family's Samsung was last recorded using 4 KB pages; this new APK still needs its own in-place update, launch and retained-data checks. No new emulator or physical-device pass is claimed.

## Next task and acceptance

**G3-REC-04 is complete as artifact preparation, with the Android 16 KB qualification issue explicitly retained. It is not a mobile release or a completed G3 gate.** `Test-AndroidArtifact.ps1` now accepts the family LAN build path while preserving its existing checks. `Test-FamilyMobileArtifacts.py` provides repeatable cross-build hash, identity and export/bridge inspection. No game behavior or transport package was changed; Windows runtime checks were not unnecessarily repeated.

**Next: G3-REC-05 — native Apple build/signing and coordinated mobile recovery qualification.** When the Mac and older iPad are available, verify the export before transfer, compile/link it, check the device and signing profile, take fresh backups and update in place. Qualify iPad 7 first, then iPad 9 and both phones. Use an isolated family test session for intentional outages; reserve actual authority deployment for an empty session with a fresh verified backup and the matching reviewed network setup.

Physical acceptance includes initial server-absent launch, complete recovery checkpoints, actual background/lock and cold resume, local interaction during an outage, saving before reunion, reopening the preserved adventure, current controls/reset cues and measured sustained mixed-device performance. Treat the previous physical passes as history, not evidence that build 91 already passed them. Checkpoints and separate adventures remain prerequisites; **G4 iPad hosting and G5 reconciliation/rooms/item policy remain required**. Unattended renewal and an independent, portable backup/restore also remain open. VPS deployment stays later.

[Main plan](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) · [Return checklist](return-checklist-ipad-lan-2026-09-24.html)
<!-- historical-record-end -->
