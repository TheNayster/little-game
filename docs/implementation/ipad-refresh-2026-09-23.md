# G1 — Windows signing-refresh qualification

23 September 2026, local time. This is the next bounded task after the completed iPad 9 build-20 update check. It supports the agreed free family-installation requirement and FAMILY-01 / TV-01 save retention. G1 remains active; the older iPad 7 is unavailable and unqualified.

## Current state

**The initial iPhone installation and recurring-refresh enrollment succeeded.** Sideloadly **0.60** shows **Done / 100%** for `LittleWeeps-0.0.20.ipa`. Its daemon's device inventory independently reports **Little Weeps 0.0.20 / build 20**, bundle ID **`com.littleweeps.familyplayset`**, installed on the connected iPhone. iTunes identifies the device as **iPhone 13 Pro Max**, running **iOS 26.6.1**. The input IPA still has the previously verified hash. [Installation and enrollment evidence](evidence/ipad-refresh-2026-09-23/iphone-install-and-enrollment.json).

The daemon database contains this game's active installation record with `one_off = 0`, no recorded error and zero failures. Its recorded fields include `known_ttl = 7` and `refresh_at_hours = 96`; these describe configuration rather than proving a future automatic cycle. The daemon has a Windows user-login startup entry. **A manual USB signing refresh has now succeeded; wireless detection and an unattended renewal cycle remain unverified.**

The user reported that the iPhone app worked after the launch/tap/video/audio check, then supplied the save seed **1 tap / 10.8 seconds**. These initial values were human reports; the preferences were not independently read until after the signing refresh.

**Wireless refresh is not yet working.** The Wi-Fi-sync checkbox persisted after reconnecting USB. An iTunes passcode-lock notice cleared after the user confirmed the same Wi-Fi and an unlocked phone. A subsequent **Sync → Done** completed without an error, but a second unplugged test still showed no iPhone in Sideloadly. Start was disabled during the wireless checks, so no wireless refresh was attempted. [Wireless check and save seed](evidence/ipad-refresh-2026-09-23/iphone-wireless-refresh.json).

A targeted network check received a Bonjour advertisement matching the phone's name and a TCP response on port 62078. This establishes a reachable advertised endpoint, not authenticated installation access. Existing Bonjour/iTunes inbound rules cover the current Windows network profile; firewall settings were not changed. Apple Mobile Device Service and Bonjour were restarted and verified running, but a read-only query to the native Apple device service still returned an empty device list. iTunes was reopened; Sideloadly remained unable to detect the phone. Restarting AMDS is a documented recognition troubleshooting step, not a guaranteed fix. [Apple guidance](https://support.apple.com/en-gb/102347). The service-type reference came from [pymobiledevice3's Bonjour source](https://raw.githubusercontent.com/doronz88/pymobiledevice3/master/pymobiledevice3/bonjour.py).

The first iPhone restart did not restore detection. The user then set Private Wi-Fi Address to **Off** for Acorn and completed Sideloadly's USB pairing repair, including the phone's local Trust prompt. Sideloadly reported successful repair, but the subsequent unplugged check still showed **no devices** and disabled Start. The user also restarted and unlocked the phone **after** changing the address setting; detection still failed. Finally, the user restarted Windows and reopened both programs. At **04:16:58 UTC on 24 September**, the native Apple device list was empty and a fresh Sideloadly screenshot still showed no device. Both support services were running. Neither iPad has been enrolled through Windows yet. The subsequent manual USB results below are separate from these failed wireless checks.

The **manual USB refresh** completed after the user reconnected the phone. The original IPA hash was reverified and the package reloaded after Sideloadly restarted. Its automatic bundle-ID option defaulted back on; it was turned off again to preserve `com.littleweeps.familyplayset`. Sideloadly signed/uploaded the app, then stayed at Installing / 0% for several minutes. The user reported the app was open; returning Home was requested. Installation subsequently completed, but the cause of the delay is not established. A fresh screenshot and accessibility state show **Done / 100%**. The daemon now records **23 September at 23:30:56 CDT** as the successful update, retains recurring enrollment, and reports no error/failures. [USB test record](evidence/ipad-refresh-2026-09-23/iphone-usb-refresh.json).

### USB refresh and controlled relaunch results

| Check | Observed result |
| --- | --- |
| Installed identity after refresh | A read-only lookup on the phone returns `com.littleweeps.familyplayset`, version 0.0.20 / build 20. This was a signing refresh of the same game content. |
| Launch and tap retention | User reopened the app and reported **1 tap**, matching the original seed. Direct reading of this game's preferences also returns 1. |
| Initial bookmark comparison | User reported **Saved 11.1s**, versus the earlier rounded 10.8s report. The exact post-refresh stored value was **11.0666666031s**. No preference snapshot was taken immediately before refresh, and the app had been opened in between; this does not establish exact bookmark retention across refresh or prove that signing caused the difference. |
| Controlled close/reopen | With that direct baseline, the user swiped the paused app away and reopened it without playback. They again reported **1 tap / Saved 11.1s**. Direct reading confirms the same profile hash and tap count, and **11.1000003815s**, a **0.0333338s** difference (one frame). This passes the existing less-than-0.25s saved-bookmark tolerance. The test tolerance and runtime code were not changed. |
| Provisioning evidence | The phone contains a matching profile created **24 September 04:25:16 UTC**, expiring **1 October 04:25:16 UTC** (**30 September 23:25:16 CDT**), with this iPhone included. The previous matching profile is also present (03:31:48 UTC creation/expiry times). These are installed-profile queries, not an extracted embedded profile comparison. |
| Automatic enrollment | One active recurring record remains (`one_off = 0`), with the new success timestamp, no error and zero failures. A transient one-off record existed while installation was running and was gone at completion. |

Manual USB signing/installation and post-refresh launch are verified. Tap retention and the controlled relaunch are verified separately. Full unattended renewal, wireless refresh and an exact bookmark comparison across signing remain open. For the next signing test, read the preference baseline immediately before the update. Code review notes that pause/quit checkpoints write decoder time even when the video was already paused; investigate further if drift accumulates, without claiming that this caused the earlier rounded difference.

Installed support components are iTunes **12.13.11.1**, Apple Mobile Device Support **20.0.0.35**, Apple Application Support **8.7 (both architectures)** and Bonjour **3.0.0.10**. Device-support and Bonjour services were verified running. [Support checks](evidence/ipad-refresh-2026-09-23/support-and-sideloadly-check.json).

The iCloud application itself did **not** complete installation: Windows logged **1722**, installer exit **1603**, at **`RuniCloudUpgrade` / `iCloud.exe /upgrade`**. The separate support-package installations succeeded before this failure and remain installed. The underlying cause of that custom-action failure is not established. Raw setup logs are in ignored `LocalData/iPadRefresh/Logs`. The later successful Sideloadly authentication/installation demonstrates that this setup could complete the current task despite that separate installer failure; it does not establish that the iCloud app is repaired.

### Source review: private-address workaround

At the user's request, reviewed firsthand reports of matching iTunes/Sideloadly wireless failures beginning with iOS 26.4. Several participants recovered detection after setting **Private Wi-Fi Address to Off** for their home network; others reported no improvement. Treat this as a candidate workaround, not a universal fix or a verified diagnosis for our iOS 26.6.1 phone. [Original report and follow-ups](https://www.reddit.com/r/sideloaded/comments/1s8ng76/comment/odqhtq4/), [Sideloadly discussion](https://www.reddit.com/r/sideloadly/comments/1s3my98/ios_264_and_wifi_device_detection_not_working/).

Apple documents the distinction: **Off** uses the hardware MAC address; **Fixed** uses a private address that does not rotate. The option is configured per network. The user confirmed Off for the home network, but did not report the prior setting. Other networks are outside this change. If the completed trial does not help, restore the user's prior setting if known; do not invent its value or assume reduced address privacy is necessary. No phone privacy setting was changed by the agent. [Apple setting instructions and behavior](https://support.apple.com/en-us/102509).

The cited Sideloadly discussion includes recovery after restarting devices following this change, alongside failures. Our phone restart after the changed setting and the user's subsequent Windows restart both failed to restore discovery. The standard restart checks are complete; do not repeat them without new evidence.

Read-only diagnostics found that the Wi-Fi MAC metadata in this phone's local pairing record differs from the MAC-shaped identifier in the matching-name Bonjour advertisement, even after normalizing punctuation and case. That difference alone does not prove corruption or the cause of the failure. No pairing records, certificates or keys were changed or copied into evidence.

### More specific discovery investigation

The `idevice` project's documentation and implementation describe iOS 26.4+ Bonjour discovery using an `identifier` and `authTag`, with a tag derived from the pairing record's HostID. This offers a more relevant check than comparing MAC strings alone. [Project documentation](https://docs.rs/idevice/0.1.68/idevice/mdns/index.html), [implementation](https://docs.rs/idevice/0.1.68/src/idevice/mdns.rs.html).

At **04:09:51 UTC**, a fresh LAN advertisement with the phone's hostname contained those fields. A local, read-only calculation using only this phone's existing pairing record matched its advertised tag. Only booleans/counts were retained; the HostID, tags, keys and certificates were not printed or saved into research evidence. This supports recognition of an advertisement associated with this PC's pairing; it is **not** a successful installation session or refresh. The MAC comparison still differed.

This narrows the investigation to discovery/connection handling but does not establish an exact Windows defect. In particular, the installed MobileDevice.dll contains the newer matching-function/tag strings; it would be unsupported to claim the installed library necessarily lacks the protocol just because the phone is missing from the UI. Sideloadly's official changelog still lists **0.60.0** as latest, matching the installed version. No confirmed Sideloadly update fixing this specific case was found in that changelog. [Release notes](https://sideloadly.io/changelog). Do not patch pairing credentials, replace system libraries, reset the phone, or substitute another sideloading stack based on a speculative cause.

After Windows restarted, the user showed an LSA compatibility warning for Bonjour's `mdnsNSP.dll`. The file has a valid Apple signature and Bonjour's service remains running. The warning concerns loading this module inside the protected Local Security Authority process; it does not establish that Bonjour as a whole is disabled or prove the cause of the Wi-Fi failure. The user was told to dismiss the notices and keep LSA protection enabled. No security setting was changed. [Microsoft explanation](https://support.microsoft.com/en-us/windows/security/windows-security/device-security-in-the-windows-security-app).

### Setup history

The earlier Windows Installer automation block was respected. The user completed the support/Sideloadly installers and manually selected the IPA when the file chooser could not be reliably controlled. The first preflight verified the explicit app ID, no name/version overrides, no tweak injection/custom entitlements, zero Info.plist changes, Apple ID Sideload with Local authentication support, and the visible **AutoRefresh [enabled]** tooltip. [Preflight evidence](evidence/ipad-refresh-2026-09-23/sideloadly-install-preflight.json).

The user then reported that entering their email and pressing Enter closed the app. The main process exited while its daemon stayed running; no matching crash entry was found in the inspected recent Windows Application log. The cause remains unknown. Reopening restored the app but lost its selected IPA; the explicit bundle ID was restored and the user reloaded the file and completed Start/sign-in locally. [Exit/reopen evidence](evidence/ipad-refresh-2026-09-23/sideloadly-reopen-after-enter.json). The latest successful result supersedes that earlier pending state. No password or verification code was collected or entered by the agent.

The desktop shortcut **Set up Little Weeps iPad refresh** opens:

`C:\Users\sephi\Desktop\Little weeps game\LocalData\iPadRefresh\Setup`

That folder contains `READ ME FIRST.txt`, three numbered installer shortcuts and `LittleWeeps-0.0.20.ipa`. The setup files remain inside this game's ignored local-data directory. The unrelated old project is untouched.

## Current test device — iPhone

The user cannot bring the iPad to Windows now and has connected their iPhone instead. Continue this Windows refresh setup on the iPhone first; both iPads remain required later. Windows reports an **Apple iPhone** portable device and Apple Mobile Device USB devices with status **OK**. Sideloadly subsequently read the device name and iOS **26.6.1** over USB. Subsequent iTunes inspection identified iPhone 13 Pro Max, and the daemon verified build 20 installed. The previously reported A-number is A2484. Cable-free discovery and actual automatic refresh remain unverified. [USB detection evidence](evidence/ipad-refresh-2026-09-23/iphone-usb-detection.json).

Build 20 declares both iPhone and iPad device families and minimum iOS 15.0. The IPA can be the input for an iPhone installation, but the current Xcode profile was verified for the iPad only. Sideloadly must sign/provision for the connected iPhone through the same Apple Account; do not assume the existing iPad signature authorizes a direct iPhone install. That Sideloadly signing/installation has now succeeded. The user reported the app worked after the launch/tap/video/audio check. The USB refresh and controlled relaunch results above now establish launch/tap retention and current saved values; the exact bookmark comparison across signing remains qualified as described.

## What was verified

- The registry and current-user Store-package inventory returned no matches for Sideloadly, iTunes, iCloud, Apple Devices, Apple Mobile Device Support or Bonjour before setup. This is the observed inventory, not proof that no stray executable exists anywhere on disk.
- The three installers came from the official Sideloadly page's download links. Apple's iTunes **12.13.11.1** and iCloud **7.21.0.23** installers have valid Apple Authenticode signatures. The official Sideloadly installer is **unsigned**; its exact downloaded hash is recorded. No publisher-signature claim is made for it. [Download sources, sizes, signatures and hashes](evidence/ipad-refresh-2026-09-23/download-verification.json).
- The IPA contains the already tested, signed build **20**, bundle ID **`com.littleweeps.familyplayset`**. The copied app passed strict/deep signature verification on the Mac, the ZIP passed integrity checking, and its executable hash matches the tested native build. The Mac and Windows IPA hashes match: **`cbfc39d51acd4f7b6d22001804719f907ea3d2c18255bcb3e01e4e1180da5ecd`**. [Package evidence](evidence/ipad-refresh-2026-09-23/package-verification.json).
- This is packaging the existing app for renewal, not a new game build or a claim that reinstalling identical bytes constitutes an update.

## Why this route

The agreed first choice is Sideloadly on the always-on Windows PC. Its vendor requires the desktop iTunes/iCloud components. [Official downloads and prerequisites](https://sideloadly.io/).

Sideloadly documents automatic refresh through its background daemon while the paired device is reachable by USB or Wi-Fi. Initial Windows Wi-Fi setup needs USB pairing and iTunes' Wi-Fi-sync option. Preserving the app requires the same Apple Account and bundle ID. These are vendor capabilities; this household's enrollment, renewal and data retention still need actual-device proof. [Vendor FAQ](https://sideloadly.io/faq).

## Next checks

1. Current verified iPhone values are **1 tap / Saved 11.1s** with the same profile across controlled relaunch. Capture a fresh exact preference snapshot immediately before the next signing update to close the initial bookmark-comparison gap; account for any later play.
2. Wireless discovery remains unresolved after all recorded restarts and re-pairing. Do not mark wireless ready from a checkbox, repair message, advertisement match or the successful USB refresh. The iPads running iPadOS 18 require their own tests; this iPhone failure does not establish that they will fail. The Private Wi-Fi Address trial did not improve detection; the user's original setting was not reported and must not be guessed when restoring their preference.
3. The USB signing/installation and controlled relaunch checks above are complete. Do not reinstall the same bytes again merely to claim an app-content update. Keep exact bookmark retention through signing distinct from the separately passed post-refresh relaunch.
4. Observe a real automatic refresh with a later expiry, successful launch and retained progress. The daemon enrollment and Windows startup entry are established, but no later automatic cycle has been observed yet.
5. Enroll iPad 9 and the older iPad separately when available, retaining the same Xcode Apple Account and `com.littleweeps.familyplayset`. Capture each device's current saves before enrollment. The last iPad 9 values were **1,019 taps / 8.9 seconds** with the original profile; account for any later play. [Completed iPad check](ipad-update-2026-09-23.md). Never uninstall or change identity to work around an error.

The original iPad Xcode profile expires **30 September 2026 at 21:15:06 UTC**. Its expiry did not move during the 16 → 20 update. The iPad is not yet enrolled for Windows refresh; the iPhone enrollment is separate. Long periods away from the refresh PC remain a separate deployment constraint; optional travel networking is not part of this task.
