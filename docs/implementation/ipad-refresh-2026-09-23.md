# G1 — Windows automatic iPad refresh preparation

23 September 2026, local time. This is the next bounded task after the completed iPad 9 build-20 update check. It supports the agreed free family-installation requirement and FAMILY-01 / TV-01 save retention. G1 remains active; the older iPad 7 is unavailable and unqualified.

## Current state

**Prepared, not enrolled or proven.** The Windows setup files and tested IPA are ready. The iTunes installer was launched, but its controls could not be automated: the computer-control tool returned **`product policy blocks this app: msiexec.exe`**. No attempt was made to bypass that restriction. The user then reported iTunes installed and signed in. A fresh registry check confirmed **iTunes 12.13.11.1** and **Apple Mobile Device Support 20.0.0.35**; the mobile-device service is running. [Installed-tool inventory](evidence/ipad-refresh-2026-09-23/windows-installed-tools.json). iCloud support and Sideloadly are not yet present in that inventory, and trusted USB/Wi-Fi pairing and Sideloadly authentication remain unverified. The user has the remaining numbered setup steps; no duplicate installer was started.

The desktop shortcut **Set up Little Weeps iPad refresh** opens:

`C:\Users\sephi\Desktop\Little weeps game\LocalData\iPadRefresh\Setup`

That folder contains `READ ME FIRST.txt`, three numbered installer shortcuts and `LittleWeeps-0.0.20.ipa`. The setup files remain inside this game's ignored local-data directory. The unrelated old project is untouched.

## Current test device — iPhone

The user cannot bring the iPad to Windows now and has connected their iPhone instead. Continue this Windows refresh setup on the iPhone first; both iPads remain required later. Windows reports an **Apple iPhone** portable device and Apple Mobile Device USB devices with status **OK**. That verifies USB detection, not trusted pairing, the exact iOS version, app installation or automatic refresh. The previously reported model is A2484; read the live device identity before enrollment. [USB detection evidence](evidence/ipad-refresh-2026-09-23/iphone-usb-detection.json).

Build 20 declares both iPhone and iPad device families and minimum iOS 15.0. The IPA can be the input for an iPhone installation, but the current Xcode profile was verified for the iPad only. Sideloadly must sign/provision for the connected iPhone through the same Apple Account; do not assume the existing iPad signature authorizes a direct iPhone install. Actual iPhone launch, layout, touch/audio/video and save/update checks remain pending.

## What was verified

- The registry and current-user Store-package inventory returned no matches for Sideloadly, iTunes, iCloud, Apple Devices, Apple Mobile Device Support or Bonjour before setup. This is the observed inventory, not proof that no stray executable exists anywhere on disk.
- The three installers came from the official Sideloadly page's download links. Apple's iTunes **12.13.11.1** and iCloud **7.21.0.23** installers have valid Apple Authenticode signatures. The official Sideloadly installer is **unsigned**; its exact downloaded hash is recorded. No publisher-signature claim is made for it. [Download sources, sizes, signatures and hashes](evidence/ipad-refresh-2026-09-23/download-verification.json).
- The IPA contains the already tested, signed build **20**, bundle ID **`com.littleweeps.familyplayset`**. The copied app passed strict/deep signature verification on the Mac, the ZIP passed integrity checking, and its executable hash matches the tested native build. The Mac and Windows IPA hashes match: **`cbfc39d51acd4f7b6d22001804719f907ea3d2c18255bcb3e01e4e1180da5ecd`**. [Package evidence](evidence/ipad-refresh-2026-09-23/package-verification.json).
- This is packaging the existing app for renewal, not a new game build or a claim that reinstalling identical bytes constitutes an update.

## Why this route

The agreed first choice is Sideloadly on the always-on Windows PC. Its vendor requires the desktop iTunes/iCloud components. [Official downloads and prerequisites](https://sideloadly.io/).

Sideloadly documents automatic refresh through its background daemon while the paired device is reachable by USB or Wi-Fi. Initial Windows Wi-Fi setup needs USB pairing and iTunes' Wi-Fi-sync option. Preserving the app requires the same Apple Account and bundle ID. These are vendor capabilities; this household's enrollment, renewal and data retention still need actual-device proof. [Vendor FAQ](https://sideloadly.io/faq).

## Resume after the user's setup reply

1. Verify the installed Apple support components and Sideloadly, then identify the connected iPhone (current target) or later iPad 9. Do not treat the installer launch as successful installation. If an installer requests a restart or reports an error, record it before continuing.
2. Finish trusted USB pairing and enable the device's Wi-Fi-sync option in iTunes. Do not enable iCloud photos/files or unrelated media synchronization. Confirm the correct device appears to Sideloadly over Wi-Fi after the USB setup.
3. Use the existing Xcode Apple Account and retain `com.littleweeps.familyplayset`. The user enters their password and any verification code directly in the local app. Do not collect credentials in chat or logs. Load the exact verified IPA and enable automatic refresh; do not delete the existing app or change its identity to work around an error.
4. Check whether this game is already installed on the selected device. Preserve its current data if present; a first iPhone install needs a new observed seed and later retention check. On the iPad, capture the latest saved count/profile/bookmark before enrollment. The last observed values after build-20 controls were **1,019 taps / 8.9 seconds**, with the original profile. [Completed iPad check](ipad-update-2026-09-23.md). Compare after the first Sideloadly installation and launch; any intervening play must be accounted for.
5. Record daemon enrollment, Windows sign-in startup, connection method and the resulting profile expiry. Test refresh over Wi-Fi. A manual refresh or enrollment alone is not proof of a later automatic cycle.
6. Observe a real automatic refresh with a later expiry, successful app launch and retained progress. Repeat on the older iPad when available. Keep renewal explicitly pending until this evidence exists.

The currently installed Xcode profile expires **30 September 2026 at 21:15:06 UTC**. Its expiry did not move during the 16 → 20 update. The helper has not yet made this installation self-renewing. Long periods away from the refresh PC remain a separate deployment constraint; optional travel networking is not part of this task.
