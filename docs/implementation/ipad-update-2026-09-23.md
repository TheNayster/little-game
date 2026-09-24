# G1 iPad update — build 16 installed, touch/media and restart checked

23 September 2026, local time. Task: the iPad 9 in-place foundation update and media/save checks, supporting FAMILY-01 / TV-01. The phase plan still requires both iPads to pass G1 before dependent G2 work.

## Current result after the local signing step

The user completed local signing. Xcode returned 0, strict/deep signature verification passed, and the app reports the expected game bundle ID, **build 16**, ARM64/device configuration and the same signing team as build 1. The embedded provisioning profile still expires **30 September 2026 at 21:15:06 UTC**; this update did not establish automatic renewal. [Signed-artifact evidence](evidence/ipad-update-2026-09-23/signed-artifact.json).

The normal **1 → 16 in-place install passed**. A fresh device query verified 0.0.16, the saved counter remained **1,010**, and the original profile ID was unchanged. Launch succeeded. There was no uninstall, reset or downgrade. [Update check](evidence/ipad-update-2026-09-23/update-check.json), [launch](evidence/ipad-update-2026-09-23/launch-check.json).

The user answered **“yes both worked”** to the physical touch and moving-video/audible-tone check. Actual device preferences then contained **1,019 taps** and a **9.4-second video bookmark**. Terminating and relaunching this app preserved those values and the same profile. The user then confirmed **“Yes, paused around 9.4 seconds”** on the reopened screen. Both storage retention and the visible resumed position passed. [Human check](evidence/ipad-update-2026-09-23/human-check.json), [restart check](evidence/ipad-update-2026-09-23/restart-check.json).

Build **20** tests an existing video bookmark across an update. Build 1 had no video fixture, so the first update cannot prove that case. The older iPad 7 is currently unavailable, per the user; its qualification and free-renewal checks remain open. [Availability record](evidence/ipad-update-2026-09-23/older-ipad-availability.json). G1 is not complete.

## Build 20 — prepared, awaiting local signing

The saved iPad Foundation profile produced a fresh non-development **0.0.20** export with zero reported Unity build errors/warnings. The archive hash matched on Windows and Mac, and all **3,025 exported files** matched their manifest hashes on the Mac. [Export](evidence/ipad-update-2026-09-23/ios-20-export.json), [transfer](evidence/ipad-update-2026-09-23/ios-20-transfer.json).

Xcode Release compilation reached signing, then returned **65** at `UnityFramework.framework` with **`errSecInternalComponent`**. This is the same remote keychain-access failure as build 16; build 20 has not passed signing or been installed. Build 16 remains on the iPad. The executable, syntax-checked **`Finish-Little-Weeps-20.command`** is prepared on the Mac desktop to use the existing local signing helper and compile cache. The user has been asked to run it and reply ready. Keychain protections were not changed. [Native build result](evidence/ipad-update-2026-09-23/ios-20-native-build.json).

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

1. After the local build-20 helper succeeds, verify its exit, bundle/build/team, signature and provisioning profile. Capture the latest installed version and preferences before installing it normally over 16. Compare the current tap count, profile and existing video bookmark after install/relaunch; never uninstall/reset as a fallback. The latest observed baseline is 1,019 taps / 9.4 seconds, but read it again in case the user has played meanwhile.
2. Finish physical skip/start-over checks after preserving the update-test baseline; confirm the post-update paused position on screen.
3. Qualify the older iPad 7 when it is available. Start the agreed free-provisioning refresh observation. G1 remains open; all multiplayer/content phases remain planned.
