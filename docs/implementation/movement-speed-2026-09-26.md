# Faster character travel — builds 124–125

September 26, 2026 · **CHAR-01 / WORLD-01 / WORLD-02**. The user accepted the calmer 123 walk, then requested **1.5 times faster movement** before the background/object correction.

## Build 124 — first speed increase

Walking speed increases from **210 to 315 floor units/second**. Tap destinations, joystick input, private solo and matching-build shared authority all use the same movement function. Diagonal input remains normalized, partial joystick input remains proportional, destinations stop exactly and existing floor bounds still apply. Character choice does not alter speed. No world, item, save schema, enrollment, camera or input-layout change is included.

The selected Bluey/Bingo artwork is unchanged. The visual two-step distance increases from **160 to 240** so full-input animation cadence remains **2.625 steps/second**, matching the calmer rhythm the user preferred in 123. This intentionally separates the requested travel speed from more energetic limb motion. It does not claim precise planted-foot matching; that prior cartoon animation limitation remains open. Radio, sit and trampoline timing are unchanged.

## Build 124 verification

[Core rules](evidence/movement-speed-2026-09-26/core-rules.json) cover private/shared movement, expected distance, diagonal normalization, exact destination arrival, bounds, cancellation, input expiry and existing save/item rules. Existing tests were adjusted to the newly requested speed; the native local-motion harness accepts an explicit expected speed while keeping its earlier default for historical builds.

Commands: `dotnet run --project Tools/SoloRules.Tests --configuration Release -- LocalData/Verification/faster-walking-124-core`; `Tools/Build-NetworkProbe.ps1 -BuildNumber 124`; `Tools/Test-LocalMovement.py 124 --expected-speed 315`; `Tools/Test-WalkAnimation.py 124`; `Tools/Build-AndroidLAN.ps1 -BuildNumber 124`.

**Passed:** all 92 core checks; non-development Windows and Android builds; native sheet playback at 30/60/120 fps, complete-cycle preview, Bluey/Bingo walking/reversal/stopping and carrying. [Animation evidence](evidence/movement-speed-2026-09-26/native-animation.json) and [sheet checks](evidence/movement-speed-2026-09-26/native-sheet-checks.json).

[Native speed measurements](evidence/movement-speed-2026-09-26/native-speed.json) at a 60 fps target measured **314.75** floor units/second for tap walking and **314.53** for joystick, against a 315 target. Both traces moved on every sampled frame with no backwards step. These are Windows measurements, not sustained phone/iPad performance claims. The old harness initially targeted x=860 just outside the scenic viewport and timed out; moving its destination inside the viewport resolved that input issue. Its exact float comparison also rejected subpixel camera projection noise after stopping; the final check requires an open menu and less than 0.01 floor unit drift.

**Android delivered:** build 124 replaced 123 in place, with matching signing certificate and an exact installed APK hash. The launched phone visibly shows Bluey in Heeler Home with the radio and controls. Both solo saves, five adventure saves and the selection record are byte-identical before/after (eight records). [Install/save evidence](evidence/movement-speed-2026-09-26/android-install.json). All 747 Unity source/art/settings files matched the build manifest before install; later changes are the verification harness, documentation and normalization of Unity-generated metadata whitespace. [Source evidence](evidence/movement-speed-2026-09-26/build-source.json).

The live server/helper remains **110**, Apple devices **101**; the phone is currently solo pending the separate coordinated content-5 rollout. Temporary native verification worlds do not touch the live family world. **ART-HOME-02 living-room composition remains the next content task.**

## Build 125 — faster again after phone feedback

The user tried 124 and requested another increase. Travel is now **420 floor units/second**: twice the original 210 and one third faster than 124. `WalkStride` rises to **320**, retaining the accepted 123 artwork and full-input cadence of **2.625 steps/second**. Both controls and characters still share the same normalized movement rule.

[All 92 core checks pass at 420](evidence/movement-speed-2026-09-26/core-rules-125.json), including travel distance, diagonal normalization, exact stopping, expiry and save/item invariants. The native trace measurements above remain build-124 evidence; they are not measurements of 125. A fresh non-development Android 125 build passed, using the same family signing certificate. The installer verified the old identity, updated in place and pulled back the installed APK to confirm its exact hash. [Build source](evidence/movement-speed-2026-09-26/build-source-125.json) matched all 747 Unity source/art/settings files before install. Later metadata whitespace normalization has no runtime effect.

[Phone delivery and saves](evidence/movement-speed-2026-09-26/android-install-125.json): the exact app launched and visibly displayed Bluey in Heeler Home with the sofa, radio and controls. All eight saved records remain: seven are byte-identical; the active solo world differs only in position, advancing revision and command history while the user was playing. All other persisted content is identical. No save was reset or replaced. Physical travel-speed measurement was not repeated; the final 420 value is checked by the core movement tests and exact installed source/artifact verification. The user still needs to judge this revised pace.
