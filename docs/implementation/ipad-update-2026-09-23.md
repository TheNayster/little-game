# G1 iPad update — ready for local Mac signing

23 September 2026, local time. Task: the iPad 9 in-place foundation update and media/save checks, supporting FAMILY-01 / TV-01. The phase plan still requires both iPads to pass G1 before dependent G2 work.

## Verified this session

- The Mac is reachable: macOS 26.3.1, M1 Pro / ARM64, Xcode 26.6. The ninth-generation iPad is connected, paired and has Developer Mode enabled, running iPadOS 18.6.2.
- Device inventory confirms this game's installed version is **0.0.1 / build 1**. The actual app preferences contain **1,010 taps** and the existing profile identity. A read-only `devicectl` copy preserved those preferences on the Mac before any update. [Before-update evidence](evidence/ipad-update-2026-09-23/before-update.json). The count was read from device storage; it is not a new human touch observation.
- Export **0.0.16** was archived and transferred to the separate Mac folder `~/Developer/LittleWeeps/Builds/G1-0.0.16`. Windows and Mac archive SHA-256 values match: `95cf086ebd5e8af969b3e3b156710d84033cbf332add82955adc4dcc9f003d54`. All **3,026 exported files** match their manifest hashes on the Mac. [Transfer evidence](evidence/ipad-update-2026-09-23/transfer.json).
- Xcode's Release device build completed native compilation through the signing stage, then returned **exit 65** when signing the embedded `UnityFramework.framework`: **`errSecInternalComponent`**. This repeats the previously observed remote-keychain access problem. No source/compile failure was reported as the failed build command. [Build result](evidence/ipad-update-2026-09-23/native-build.json).

Build 16 is **not signed, installed or device-qualified**. The existing iPad app and data were not removed, replaced or reset. Raw preferences, Xcode log and result bundle remain on the Mac under `~/Developer/LittleWeeps/Logs`. No private signing keys or passwords were copied into the project or sent to Windows.

## Concrete user step

A syntax-checked, executable **`Finish-Little-Weeps-16.command`** is now on the Mac desktop. It calls the already proven `Tools/Build-iOS-Mac.sh` with build 16 and the connected iPad, using its existing compile cache. It unlocks the login keychain through the Mac's local prompt, builds Release and verifies the resulting app signature. Keychain protections were not changed.

Double-click that file on the Mac. Enter the password only there if requested, and approve a codesign request if shown. Reply **ready** when it prints **Build and signature checks passed**; otherwise report its error. This step prepares the app; it does not install it.

## Resume in this order

1. Inspect the new local build exit/log and verify the app's signature, bundle ID, build 16, team and embedded provisioning dates. Do not install the incomplete remotely built app.
2. Re-read the iPad's current installed version and preferences so any intervening taps are accounted for. Install normally in place, then independently query the installed build and compare the saved tap count/profile. Never uninstall or reset as a fallback.
3. Launch build 16. Observe actual touch/audio and local-video playback/pause/skip/restart; record a bookmark and confirm it survives leaving/reopening and a later in-place update. The old build 1 had no video fixture, so 1 → 16 cannot prove retention of a pre-existing video bookmark.
4. Run the same foundation checks on the older iPad 7. Start the agreed free-provisioning refresh observation. G1 remains open; all multiplayer/content phases remain planned.
