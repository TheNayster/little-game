# G1 iPad update — build 20 installed, saved video position retained

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
23 September 2026, local time. Task: the iPad 9 in-place foundation update and media/save checks, supporting FAMILY-01 / TV-01. The phase plan still requires both iPads to pass G1 before dependent G2 work.

## Current result — 16 → 20 update passed

The local Mac helper completed build 20 with exit 0. Strict/deep signature verification passed, and its bundle/build, signing team, device eligibility and unexpired provisioning profile were checked before installation. The profile expiry is still **30 September 2026 at 21:15:06 UTC**; automatic renewal is not configured by this update. [Build-20 signed artifact](evidence/ipad-update-2026-09-23/ios-20-signed-artifact.json).

A fresh read immediately before installation found **1,019 taps**, the same profile and a **10.3-second bookmark**. The video position had changed since the earlier 9.4-second restart check, so this newer value was used as the update baseline. [Before build-20 update](evidence/ipad-update-2026-09-23/ios-20-before-update.json).

The normal **16 → 20 in-place update passed**. Independent device inventory confirmed installed version **0.0.20 / build 20**; actual preferences retained all three values exactly. Launching build 20 succeeded and retained them again. No uninstall, reset or downgrade was used. This proves preservation of an existing video bookmark across an installed iPad update, beyond the earlier tap/profile-only 1 → 16 check. [Update check](evidence/ipad-update-2026-09-23/ios-20-update-check.json), [launch and saved values](evidence/ipad-update-2026-09-23/ios-20-launch-check.json).

The user confirmed **“yes they all work!”** for the visible 10.3-second resume, START OVER returning to zero, playback/pause and +2 SECONDS. A subsequent device read retained the same profile and 1,019 taps, with the newly chosen **8.9-second bookmark**. [Physical checks](evidence/ipad-update-2026-09-23/ios-20-human-check.json). This bounded iPad 9 fixture task is complete. The older iPad 7 remains unavailable; its qualification and the renewal observation remain open. G1 is still active.

## Earlier build-16 result

The user completed local signing. Xcode returned 0, strict/deep signature verification passed, and the app reports the expected game bundle ID, **build 16**, ARM64/device configuration and the same signing team as build 1. The embedded provisioning profile still expires **30 September 2026 at 21:15:06 UTC**; this update did not establish automatic renewal. [Signed-artifact evidence](evidence/ipad-update-2026-09-23/signed-artifact.json).

The normal **1 → 16 in-place install passed**. A fresh device query verified 0.0.16, the saved counter remained **1,010**, and the original profile ID was unchanged. Launch succeeded. There was no uninstall, reset or downgrade. [Update check](evidence/ipad-update-2026-09-23/update-check.json), [launch](evidence/ipad-update-2026-09-23/launch-check.json).

The user answered **“yes both worked”** to the physical touch and moving-video/audible-tone check. Actual device preferences then contained **1,019 taps** and a **9.4-second video bookmark**. Terminating and relaunching this app preserved those values and the same profile. The user then confirmed **“Yes, paused around 9.4 seconds”** on the reopened screen. Both storage retention and the visible resumed position passed. [Human check](evidence/ipad-update-2026-09-23/human-check.json), [restart check](evidence/ipad-update-2026-09-23/restart-check.json).

Build **20** tests an existing video bookmark across an update. Build 1 had no video fixture, so the first update cannot prove that case. The older iPad 7 is currently unavailable, per the user; its qualification and free-renewal checks remain open. [Availability record](evidence/ipad-update-2026-09-23/older-ipad-availability.json). G1 is not complete.

## Build 20 — export and earlier signing handoff

The saved iPad Foundation profile produced a fresh non-development **0.0.20** export with zero reported Unity build errors/warnings. The archive hash matched on Windows and Mac, and all **3,025 exported files** matched their manifest hashes on the Mac. [Export](evidence/ipad-update-2026-09-23/ios-20-export.json), [transfer](evidence/ipad-update-2026-09-23/ios-20-transfer.json).

The earlier remote Xcode Release build reached signing, then returned **65** at `UnityFramework.framework` with **`errSecInternalComponent`**. The executable, syntax-checked **`Finish-Little-Weeps-20.command`** was prepared on the Mac desktop to use the existing local signing helper and compile cache. The user completed that step and replied ready; its exit and signature were independently verified before the successful update recorded above. Keychain protections were not changed. The remote failure remains historical evidence, not the current build status. [Earlier remote result](evidence/ipad-update-2026-09-23/ios-20-native-build.json).

## Earlier preparation and signing handoff

- The Mac is reachable: macOS 26.3.1, M1 Pro / ARM64, Xcode 26.6. The ninth-generation iPad is connected, paired and has Developer Mode enabled, running iPadOS 18.6.2.
- Device inventory confirms this game's installed version is **0.0.1 / build 1**. The actual app preferences contain **1,010 taps** and the existing profile identity. A read-only `devicectl` copy preserved those preferences on the Mac before any update. [Before-update evidence](evidence/ipad-update-2026-09-23/before-update.json). The count was read from device storage; it is not a new human touch observation.
- Export **0.0.16** was archived and transferred to the separate Mac folder `~/Developer/LittleWeeps/Builds/G1-0.0.16`. Windows and Mac archive SHA-256 values match: `95cf086ebd5e8af969b3e3b156710d84033cbf332add82955adc4dcc9f003d54`. All **3,026 exported files** match their manifest hashes on the Mac. [Transfer evidence](evidence/ipad-update-2026-09-23/transfer.json).
- Xcode's Release device build completed native compilation through the signing stage, then returned **exit 65** when signing the embedded `UnityFramework.framework`: **`errSecInternalComponent`**. This repeats the previously observed remote-keychain access problem. No source/compile failure was reported as the failed build command. [Build result](evidence/ipad-update-2026-09-23/native-build.json).

At that earlier handoff, build 16 was not yet signed or installed. The local retry and installation are recorded above. Raw preferences, Xcode logs and result bundles remain on the Mac under `~/Developer/LittleWeeps/Logs`. No private signing keys or passwords were copied into the project or sent to Windows.

## Completed local user step

A syntax-checked, executable **`Finish-Little-Weeps-16.command`** is now on the Mac desktop. It calls the already proven `Tools/Build-iOS-Mac.sh` with build 16 and the connected iPad, using its existing compile cache. It unlocks the login keychain through the Mac's local prompt, builds Release and verifies the resulting app signature. Keychain protections were not changed.

The user ran this helper and replied **ready**. Its successful exit was checked independently before installation. The helper only prepared/signed the app; installation occurred afterward through the verified device workflow above.

## Remaining checks

1. Begin the agreed Windows automatic-refresh setup and verify enrollment, actual renewal and save retention. The native build/update/media fixture checks on iPad 9 are complete; they do not qualify the finished game or prove automatic renewal.
2. Qualify the older iPad 7 when it is available. G1 remains open; all multiplayer/content phases remain planned.
<!-- historical-record-end -->
