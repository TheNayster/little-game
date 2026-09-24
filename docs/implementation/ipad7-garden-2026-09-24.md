# Older iPad garden — first play/restart checks passed, inventory diagnostic open

Bounded task: install the verified solo garden export 56 on the family's iPad 7 and begin actual-device checks. This supports FAMILY-01's device baseline and the G2 control, interaction and saved-play foundation. It does not qualify the full multiplayer game or complete G1/G2.

## Device setup

The older tablet is **iPad (7th generation), iPad7,11 / A2197**, running **iPadOS 18.7.10 as read from the device**. This corrects the earlier reported OS assignment: the iPad 9 actually runs 18.6.2, while this iPad 7 runs 18.7.10.

The Mac initially discovered the iPad over USB but reported it unpaired. `devicectl manage pair` succeeded after the user trusted the Mac. The Developer Mode setting then became available; the user enabled it and restarted. A fresh Mac inventory confirmed **Developer Mode enabled, paired, wired and connected**. [Initial pairing](evidence/ipad7-garden-2026-09-24/setup.json), [ready device](evidence/ipad7-garden-2026-09-24/device-ready.json). The exact game bundle was absent before any installation; there was no existing game save to replace.

## Native build, first install and launch

The existing iPad 9 profile did not include this iPad 7, so it could not be used as proof of install eligibility here. All **3,083 original export files** were reverified on the Mac. The local signing helper built the same export for this device, using **`Builds/G2-0.0.56/DeviceBuilds/ipad7`** for its separate DerivedData/product output. [Export recheck](evidence/ipad7-garden-2026-09-24/export-recheck.json).

Xcode returned **0 / BUILD SUCCEEDED**. Strict/deep signature checks, the original family team/application identity, ARM64 architecture and device inclusion passed. The new profile expires **1 October 2026 at 17:41:17 UTC**. Every file of the previously qualified iPad 9 product still matched its original manifest. [Signed artifact](evidence/ipad7-garden-2026-09-24/signed-artifact.json).

The install command reported success for `com.littleweeps.familyplayset`, which was absent immediately beforehand. Subsequent app-inventory reads failed with a CoreDevice StreamingAction communication error. Launch initially returned **CoreDevice 10002 / FBS Security**, listing invalid signature, inadequate entitlements or an untrusted profile. The user approved the developer profile under **Settings → General → VPN & Device Management** and reported **“app opened.”** Native launch of the exact bundle then also succeeded, with no rebuild or repeated installation. That resolves the launch-trust step. [Earlier status](evidence/ipad7-garden-2026-09-24/native-status-before-trust.json), [install and launch verification](evidence/ipad7-garden-2026-09-24/install-launch-check.json).

**Open tooling limit:** installed-app enumeration still returns the StreamingAction error after trust, including with a different supported filter. The prepared and hash-verified artifact is 0.0.56; the installer receipt identifies the exact bundle, and its native launch and app-data reads work. A separate installed-version inventory query has **not** passed. Do not turn that failure into either a claim of successful inventory or an app crash. No other device settings were changed to work around it.

`Tools/Build-iOS-Mac.sh` accepts an optional fifth output tag such as `ipad7`. Omitting it preserves the existing product path. Device tags permit only letters, numbers, underscores and hyphens, and each device output has separate logs/exit records. The lock remains at the export level because Xcode/Unity may also write shared intermediate files there. The helper passed Mac shell syntax validation before launch.

## Physical play and saved-data results

The user reported **“did all the tests from the other ipad and its running great.”** This records their repetition of the iPad 9 checklist (layout, touch/movement and dragging, watering/cleanup, speech, menu/lock behavior, settings, reopen and offline play) as a user-reported pass. Each substep was not separately prompted/observed again. Smooth play is a subjective report, not measured frame pacing, memory or long-session stability. [Human report and device save](evidence/ipad7-garden-2026-09-24/human-check.json).

The actual save passed its header/hash integrity check at **revision 439**: blue pup, fully watered plant (3), cleaned puddle (0), refilled bucket (3) and no held props. A second read confirmed the save was unchanged before the automated reopen. Terminating and relaunching this game's process preserved **every save byte and every recorded preference**. Joystick remained 1. There is no stored voice override on this iPad; the effective default is Voice on. Unlike the iPad 9 controlled mute/re-enable sequence, a specific persisted off→on override was not independently demonstrated here. [Exact restart result](evidence/ipad7-garden-2026-09-24/restart-check.json).

The bounded first-install/play/restart work is recorded with the installed-app inventory limitation above. Sustained quantitative performance, child usability, extra character/activity/hint checks, native media coverage, garden-to-garden updates, renewal and multiplayer/hosting still have their own gates. Neither G1 nor G2 is complete. The next bounded Windows coding task remains independent logical areas and per-player travel.

Raw inventory, native logs and verification outputs stay under this game's Mac `Logs/ipad7-garden-56-20260924` and `Logs/g2-ios-0.0.56-ipad7-*` paths. Signing keys and passwords remain on the Mac. The unrelated old project is untouched.
