# Garden build 56 on iPad 9 — native update and first device checks passed

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
24 September 2026. Bounded task: install and qualify the prepared G2 solo garden on the primary available iPad, supporting CHAR-01, ACT-01, the local interaction foundation for ITEM-02, and preservation of existing TV-01 data. This does not implement mobile multiplayer or complete G1/G2.

## Installed result

The connected device is the ninth-generation iPad (`iPad12,1`), running **iPadOS 18.6.2 as read from the device**. The Mac reports macOS 26.3.1, M1 Pro/ARM64 and Xcode 26.6. It is paired over USB with Developer Mode enabled.

Windows rechecked all **3,083 export files** for solo **0.0.56**. The transferred archive and every export file matched their recorded hashes on the Mac, under the separate `~/Developer/LittleWeeps/Builds/G2-0.0.56` directory. [Transfer verification](evidence/ipad-garden-2026-09-24/transfer-verified.json).

The local Terminal helper completed the native Release build with exit 0. Strict/deep signature verification, original signing team/application identity, connected-device eligibility and profile expiry checks passed. The app targets iPad/iPhone, ARM64 and minimum iOS 15.0. The profile still expires **30 September 2026 at 21:15:06 UTC**; this is not proof of automatic renewal. [Signed artifact](evidence/ipad-garden-2026-09-24/signed-artifact.json).

An immediate pre-update read captured the existing G1 preferences. The normal **20 → 56 in-place installation** succeeded. Independent device inventory reported **0.0.56 / build 56**. All pre-update preferences remained exactly equal, including **1,019 taps**, the original profile and the **8.899999618530273-second** video bookmark. No uninstall, reset or downgrade was used. The garden stores its own progress separately from the older diagnostic preferences. [Update verification](evidence/ipad-garden-2026-09-24/update-check.json).

The native app launch succeeded, and another preference read retained the same old values. Device storage also contains the new `Documents/SoloPrototype/family-local/world.save` and its backup. File presence alone is not a completed garden save/reopen test. [Launch check](evidence/ipad-garden-2026-09-24/launch-check.json).

## First physical checks passed; remaining checks

The user answered **“yup everything works”** to version/full-layout, tap walking, bucket → tap → plant and audible Listen instructions. A subsequent read of the actual garden save passed its header and SHA-256 integrity check. Revision 239 contained the blue pup, fully watered plant (3), cleaned puddle (0), empty bucket and released props. [Physical checks](evidence/ipad-garden-2026-09-24/human-check.json).

The user then closed the app from the app switcher and reopened it, confirming the garden still looked the same. The re-read checkpoint matched the **complete pre-restart snapshot and its payload hash exactly**, at revision 239. This qualifies a real iPad garden restart; it does not qualify a future garden-to-garden app update or interrupted-write recovery. [Restart verification](evidence/ipad-garden-2026-09-24/restart-check.json).

The user also confirmed **“works like a charm”** for joystick movement with one finger while dragging the bucket with another, and opening Menu while holding the bucket then returning and picking it up again. No stuck movement or bucket was reported. [Physical multitouch/menu check](evidence/ipad-garden-2026-09-24/touch-check.json).

The user confirmed that Voice off silenced Listen, screen locking while holding the bucket allowed normal pickup after returning, and the settings appeared retained after reopening. A device preference read confirmed `solo.prototype.voice = 0`, but the movement setting read `solo.prototype.joystick = 0` (Tap to walk). Because the user was also doing an offline walking check, the later value does not establish whether Joystick failed to persist or the mode was changed afterward. A controlled Joystick baseline/restart was requested; do not mark that exact preference passed yet.

The user separately confirmed walking and dragging after turning Wi-Fi off in Settings and reopening the garden. This is a physical offline-solo check, not a networking or host-recovery test. [Lifecycle/settings/offline observations](evidence/ipad-garden-2026-09-24/lifecycle-offline-check.json).

**Controlled settings follow-up passed:** the user selected the visible joystick and left the app alone. A fresh read confirmed Joystick = 1 and Voice = 0. Terminating/relaunching this app retained every preference exactly, and the user confirmed the circular joystick was still visible. The user then turned Voice on and confirmed it stayed quiet until Listen spoke normally. The final device read confirmed Joystick = 1, Voice = 1 and all original G1 preferences unchanged. The earlier Tap value is not counted as a reproduced persistence defect. [Settings and visible restart evidence](evidence/ipad-garden-2026-09-24/joystick-restart-check.json).

This bounded installation/first-device-check task is complete. Additional character/activity/hint checks, child usability, the older iPad, sustained performance and the other platform/renewal gates remain open. Follow the [device checklist](g2-device-checklist.md), splitting the checks into small groups. The tested garden restart does not substitute for a future in-place update retaining an already developed garden. Neither G1 nor G2 is declared complete.

This is the **solo** garden. Shared Windows preview 57 remains a separate loopback-only prototype. No LAN clients, iPad hosting, automatic joining/switching or four-device result is claimed. The iPad 7, secondary phones and renewal qualification remain open.

## Build and tooling notes

`Tools/Build-iOS-Mac.sh` now accepts an optional fourth argument (`G1` by default, or `G2`) and verifies the requested export version. Existing three-argument G1 usage is preserved. The current launch uses `56`, the established family team/device, and `G2`.

Two local attempts started during preparation. The second attempt at 17:14:00 UTC returned Xcode 65 because the first was already writing the same build database. The original 17:12:58 build continued and succeeded; no failed output was installed. The helper now acquires a per-export directory lock before asking for the signing password, records the owning PID, and keeps a per-run exit record. The updated helper passed Mac shell syntax validation and was deployed after the original build finished; the already accepted native build used the earlier phase-aware helper.

The install verification script initially placed launch options after the bundle identifier, which `devicectl` interpreted as app arguments. That launch command exited 64 after installation and update checks had already succeeded. The corrected option order launched the installed build successfully; no second installation was needed.

Raw device preferences, inventory, launch/install results, signed-file manifest and native build logs remain under this game's Mac `Logs/ipad-garden-56-20260924` and `Logs/g2-ios-0.0.56-local-*` paths. No passwords/private signing keys were copied into this project. The original unrelated Meeps project was not touched.
<!-- historical-record-end -->
