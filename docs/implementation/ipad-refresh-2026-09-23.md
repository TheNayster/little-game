# G1 — Windows automatic iPad refresh preparation

23 September 2026, local time. This is the next bounded task after the completed iPad 9 build-20 update check. It supports the agreed free family-installation requirement and FAMILY-01 / TV-01 save retention. G1 remains active; the older iPad 7 is unavailable and unqualified.

## Current state

**The initial iPhone installation and recurring-refresh enrollment succeeded.** Sideloadly **0.60** shows **Done / 100%** for `LittleWeeps-0.0.20.ipa`. Its daemon's device inventory independently reports **Little Weeps 0.0.20 / build 20**, bundle ID **`com.littleweeps.familyplayset`**, installed on the connected iPhone. iTunes identifies the device as **iPhone 13 Pro Max**, running **iOS 26.6.1**. The input IPA still has the previously verified hash. [Installation and enrollment evidence](evidence/ipad-refresh-2026-09-23/iphone-install-and-enrollment.json).

The daemon database contains this game's active installation record with `one_off = 0`, no recorded error and zero failures. Its recorded fields include `known_ttl = 7` and `refresh_at_hours = 96`; these are configuration evidence, not a measured provisioning expiry or completed renewal. The daemon has a Windows user-login startup entry. **Actual automatic renewal and save retention remain pending.**

The user reported that the iPhone app worked after the launch/tap/video/audio check, then supplied the save seed **1 tap / 10.8 seconds**. These are human-reported visible values; the profile/container was not independently extracted.

**Wireless refresh is not yet working.** The Wi-Fi-sync checkbox persisted after reconnecting USB. An iTunes passcode-lock notice cleared after the user confirmed the same Wi-Fi and an unlocked phone. A subsequent **Sync → Done** completed without an error, but a second unplugged test still showed no iPhone in Sideloadly. Start remains disabled, so no refresh has been attempted. [Wireless check and save seed](evidence/ipad-refresh-2026-09-23/iphone-wireless-refresh.json).

A targeted network check received a Bonjour advertisement matching the phone's name and a TCP response on port 62078. This establishes a reachable advertised endpoint, not authenticated installation access. Existing Bonjour/iTunes inbound rules cover the current Windows network profile; firewall settings were not changed. Apple Mobile Device Service and Bonjour were restarted and verified running, but a read-only query to the native Apple device service still returned an empty device list. iTunes was reopened; Sideloadly remained unable to detect the phone. Restarting AMDS is a documented recognition troubleshooting step, not a guaranteed fix. [Apple guidance](https://support.apple.com/en-gb/102347). The service-type reference came from [pymobiledevice3's Bonjour source](https://raw.githubusercontent.com/doronz88/pymobiledevice3/master/pymobiledevice3/bonjour.py).

The next check is one iPhone restart, local passcode unlock and another cable-free detection attempt. Neither iPad has been enrolled through Windows yet. USB installation, recurring-refresh enrollment and Wi-Fi discovery are tracked separately; no automatic renewal or refresh-retention result is claimed.

Installed support components are iTunes **12.13.11.1**, Apple Mobile Device Support **20.0.0.35**, Apple Application Support **8.7 (both architectures)** and Bonjour **3.0.0.10**. Device-support and Bonjour services were verified running. [Support checks](evidence/ipad-refresh-2026-09-23/support-and-sideloadly-check.json).

The iCloud application itself did **not** complete installation: Windows logged **1722**, installer exit **1603**, at **`RuniCloudUpgrade` / `iCloud.exe /upgrade`**. The separate support-package installations succeeded before this failure and remain installed. The underlying cause of that custom-action failure is not established. Raw setup logs are in ignored `LocalData/iPadRefresh/Logs`. The later successful Sideloadly authentication/installation demonstrates that this setup could complete the current task despite that separate installer failure; it does not establish that the iCloud app is repaired.

### Setup history

The earlier Windows Installer automation block was respected. The user completed the support/Sideloadly installers and manually selected the IPA when the file chooser could not be reliably controlled. The first preflight verified the explicit app ID, no name/version overrides, no tweak injection/custom entitlements, zero Info.plist changes, Apple ID Sideload with Local authentication support, and the visible **AutoRefresh [enabled]** tooltip. [Preflight evidence](evidence/ipad-refresh-2026-09-23/sideloadly-install-preflight.json).

The user then reported that entering their email and pressing Enter closed the app. The main process exited while its daemon stayed running; no matching crash entry was found in the inspected recent Windows Application log. The cause remains unknown. Reopening restored the app but lost its selected IPA; the explicit bundle ID was restored and the user reloaded the file and completed Start/sign-in locally. [Exit/reopen evidence](evidence/ipad-refresh-2026-09-23/sideloadly-reopen-after-enter.json). The latest successful result supersedes that earlier pending state. No password or verification code was collected or entered by the agent.

The desktop shortcut **Set up Little Weeps iPad refresh** opens:

`C:\Users\sephi\Desktop\Little weeps game\LocalData\iPadRefresh\Setup`

That folder contains `READ ME FIRST.txt`, three numbered installer shortcuts and `LittleWeeps-0.0.20.ipa`. The setup files remain inside this game's ignored local-data directory. The unrelated old project is untouched.

## Current test device — iPhone

The user cannot bring the iPad to Windows now and has connected their iPhone instead. Continue this Windows refresh setup on the iPhone first; both iPads remain required later. Windows reports an **Apple iPhone** portable device and Apple Mobile Device USB devices with status **OK**. Sideloadly subsequently read the device name and iOS **26.6.1** over USB. Subsequent iTunes inspection identified iPhone 13 Pro Max, and the daemon verified build 20 installed. The previously reported A-number is A2484. Cable-free discovery and actual automatic refresh remain unverified. [USB detection evidence](evidence/ipad-refresh-2026-09-23/iphone-usb-detection.json).

Build 20 declares both iPhone and iPad device families and minimum iOS 15.0. The IPA can be the input for an iPhone installation, but the current Xcode profile was verified for the iPad only. Sideloadly must sign/provision for the connected iPhone through the same Apple Account; do not assume the existing iPad signature authorizes a direct iPhone install. That Sideloadly signing/installation has now succeeded. The user reported the app worked after the launch/tap/video/audio check. Detailed layout, exact save values and retention through refresh still need evidence.

## What was verified

- The registry and current-user Store-package inventory returned no matches for Sideloadly, iTunes, iCloud, Apple Devices, Apple Mobile Device Support or Bonjour before setup. This is the observed inventory, not proof that no stray executable exists anywhere on disk.
- The three installers came from the official Sideloadly page's download links. Apple's iTunes **12.13.11.1** and iCloud **7.21.0.23** installers have valid Apple Authenticode signatures. The official Sideloadly installer is **unsigned**; its exact downloaded hash is recorded. No publisher-signature claim is made for it. [Download sources, sizes, signatures and hashes](evidence/ipad-refresh-2026-09-23/download-verification.json).
- The IPA contains the already tested, signed build **20**, bundle ID **`com.littleweeps.familyplayset`**. The copied app passed strict/deep signature verification on the Mac, the ZIP passed integrity checking, and its executable hash matches the tested native build. The Mac and Windows IPA hashes match: **`cbfc39d51acd4f7b6d22001804719f907ea3d2c18255bcb3e01e4e1180da5ecd`**. [Package evidence](evidence/ipad-refresh-2026-09-23/package-verification.json).
- This is packaging the existing app for renewal, not a new game build or a claim that reinstalling identical bytes constitutes an update.

## Why this route

The agreed first choice is Sideloadly on the always-on Windows PC. Its vendor requires the desktop iTunes/iCloud components. [Official downloads and prerequisites](https://sideloadly.io/).

Sideloadly documents automatic refresh through its background daemon while the paired device is reachable by USB or Wi-Fi. Initial Windows Wi-Fi setup needs USB pairing and iTunes' Wi-Fi-sync option. Preserving the app requires the same Apple Account and bundle ID. These are vendor capabilities; this household's enrollment, renewal and data retention still need actual-device proof. [Vendor FAQ](https://sideloadly.io/faq).

## Next checks

1. The user-reported seed is **1 tap / 10.8 seconds**. Preserve these values during the refresh check; do not treat the report as an independently read profile/container snapshot.
2. USB remains unplugged after a second test. Sync → Done and service restarts have completed, but discovery still fails. Await the requested iPhone restart/passcode unlock, then recheck detection. The vendor FAQ calls for the same network and the device screen to be on; both were confirmed before the failed check. Do not mark wireless refresh ready from the checkbox alone.
3. Once there is a known save seed, test an in-place refresh and verify the same count/profile/bookmark afterward. Record the resulting provisioning expiry if available. A manual refresh is separate from observing a later automatic cycle.
4. Observe a real automatic refresh with a later expiry, successful launch and retained progress. The daemon enrollment and Windows startup entry are established, but no later automatic cycle has been observed yet.
5. Enroll iPad 9 and the older iPad separately when available, retaining the same Xcode Apple Account and `com.littleweeps.familyplayset`. Capture each device's current saves before enrollment. The last iPad 9 values were **1,019 taps / 8.9 seconds** with the original profile; account for any later play. [Completed iPad check](ipad-update-2026-09-23.md). Never uninstall or change identity to work around an error.

The original iPad Xcode profile expires **30 September 2026 at 21:15:06 UTC**. Its expiry did not move during the 16 → 20 update. The iPad is not yet enrolled for Windows refresh; the iPhone enrollment is separate. Long periods away from the refresh PC remain a separate deployment constraint; optional travel networking is not part of this task.
