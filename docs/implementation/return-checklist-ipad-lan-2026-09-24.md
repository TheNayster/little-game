# Current checks — older iPad 91; server/Android 83; newer iPad/iPhone 79

**Current return session, September 25:** native Apple build/signing and the older iPad 79 → 91 update passed. All 263 saved-document files and existing preferences were preserved, and the iPad automatically rejoined the original server. [Update evidence](g3-ipad91-qualification-2026-09-25.html). The next immediate action is the short physical controls check on that iPad. The server and other devices have not been updated in this session.

**Completed while you were away:** password-protected portable server recovery passed with a separate test family and all four original test players. The project Android emulator also passed a retained 79 → 91 update and [six recovery checks with three Windows players](g3-android-recovery-2026-09-25.html). The [parent page also gained protected-backup download and file checking](g3-parent-portable-backups-2026-09-25.html), tested with a separate four-player family. The emulator and three Windows players also passed a [ten-minute build 91 recovery run](g3-sustained-recovery-play-2026-09-25.html), including normal item resets and saved-adventure preservation. The [optional Windows sign-in helper](g3-signin-startup-2026-09-25.html) also passes isolated native/browser checks, including recovery of four original players and preserved Stop/Pause choices. Your actual Startup folder, live world and physical devices were not changed, and no family passphrase was created.

## Your to-do list when you are home

**Current steps. No passwords or verification codes need to be sent in chat.**

1. **Older iPad:** keep it connected to the Mac and unlocked. On build **0.0.91**, check walking, dragging the bucket to the tap/plant, Listen and full-screen visibility. Reply to the pending controls check, then leave the app open.
2. **Next devices:** have the newer **iPad 9 / A2602** and its USB cable ready after we finish the older-iPad checks. Android and iPhone follow. Tell me if anyone else starts playing before we coordinate a server update. No old signing shortcut needs to be rerun.
3. **Choose a backup destination:** an external drive or another computer, separate from this PC's current game folder. Send the destination when convenient. The portable encrypted backup/reconstruction tool now passes isolated Windows tests. We still need a real copy on that destination and a restore on another Windows account/computer. Once I relaunch the updated parent helper, use **Download protected backup**, move the downloaded file to your chosen destination, then use **Check backup file** on that copy. Choose and save the passphrase privately; do not send it here. That file check does not replace the later restore test. [The new controls](g3-parent-portable-backups-2026-09-25.html).

**After the older iPad controls check, we will do these together:**

- On the older iPad first, check offline start, local play during an outage, closing/reopening, and family reunion with the local adventure still available. I will arrange the recovery check and give exact short steps without interrupting other players; do not shut down the family server yourself to simulate a failure.
- Repeat the relevant recovery checks on the newer iPad, then Samsung and iPhone. Check actual lock/background behavior, four-player movement and the updated phone layout/reset cues. Existing successful smoke tests stay recorded.
- Update the family authority and enable its qualified recovery controls during an **empty session**. The current parent page still selects operations build 85 and recovery is Off. I will handle the matching build and backups; approve a Windows prompt only if the reviewed setup actually needs one. I will deliberately retire the old parent helper while preserving the game and settings; the new launcher refuses to replace an older running helper automatically.
- After that, if you want the helper to reopen when this Windows account signs in, use **Add sign-in startup**. We will then test an actual sign-out/sign-in or reboot together. This has only been tested using isolated shortcuts so far; it does not wake the PC or run before login. Removing the shortcut leaves current play/recovery running, and saved Stop/Pause choices remain respected.

**Still pending:** reliable unattended renewal on both iPads. Keep the USB cables available; Wi-Fi refresh has not passed. The last recorded development profile expires October 1, so we will check the installed profile before updating.

VPS details can wait until the reliability and deployment gates are ready. Focus on the older iPad first.

Both iPads were **updated in place and launched on 79**. Their earlier garden saves and settings were preserved, verified backups are on both computers, and each iPad has its own Keychain family identity. Signing for that earlier build is complete. The new 91 candidate has now completed native build/signing and updated iPad 7; iPad 9 is still pending. Do not rerun an old signing shortcut. [Earlier installation evidence](g3-ipad-79-2026-09-24.html).

**Earlier four-device milestone: both iPads, Samsung and iPhone have joined the same PC shared world.** Native discovery and admission are verified, and the user confirms all devices work. After everyone left, the user authorized the PC server and Samsung update to 83. Those updates completed with iPads/iPhone on 79 at that time; the older iPad is now 91 as recorded above. [Four-device record](g3-phones-79-2026-09-24.html).

## Earlier iPad 79 evidence — new 91 checks are tracked above

1. **Done: both apps open, local-network permission allowed, both joined.** This passed during the earlier session.
2. **Done by user report: smooth two-way movement and one shared bucket.** Both iPads show the other player moving; the bucket can only be held by one player, then dropped and picked up by the other. [Recorded feedback](evidence/ipad79-2026-09-24/movement-bucket-feedback.json).
3. **Done by user report: independent areas and reunion.** One player visited Creek while the other kept playing in Garden; they saw each other again on reunion. [Recorded feedback](evidence/ipad79-2026-09-24/independent-travel-feedback.json). Shared-build joystick plus dragging, Listen, remembered settings and full-screen layout still need their separate check.
4. **Done: leave and return on both iPads.** The user confirms locking/unlocking and also closing/reopening while the sibling continues; movement and bucket pickup work afterward. Native reads confirm retained identities and automatic rejoin from new app processes to the same PC authority. [Evidence](evidence/ipad79-2026-09-24/lock-cold-rejoin.json).
5. **Passed on the tested iPad: saved solo round trip.** The user confirmed easy family rejoin with server positions and restoration of the solo toy position when returning to Play by myself. [Feedback](evidence/ipad79-2026-09-24/solo-roundtrip-feedback.json). Which iPad was used and repetition on both were not specified. Further testing was paused at the end of that session; the user has now returned. Initial joining after a server-absent start and remaining multi-touch/layout/voice/settings checks remain deferred.

The previous unpaired solo garden, paired-device solo draft and server world are separate. The update preserves the old save; it does not merge it into the family garden. Windows 90 qualifies automatic local adventures and safe reunion; Windows 91 adds failure/retry evidence. Physical device qualification and full offline reconciliation still need their planned work.

## Phone milestone — completed within the recorded scope

6. **Done: Samsung 15 → 79 in place.** Original signing identity, installed APK hash, native enrollment/discovery and automatic family admission verified. Full layout, walking, bucket fill/pour and Listen passed by user report. Accessible files were backed up; exact private foundation-preference retention was not independently read back.
7. **Done: iPhone 20 → 79 in place and all four connected.** USB Sideloadly update completed, native version matched and all three prior preference values remained. The fourth identity was consumed into Keychain and joined automatically. User reports the full layout, walking, bucket drag and Listen work. Server evidence confirms four distinct physical clients together. This does not complete sustained four-device gameplay/lifecycle acceptance.

## Phone layout and resets — Android/server 83 applied

The user requested less phone side space and timers for unused items/completed quests. [Build 82](g3-phone-layout-resets-2026-09-24.html) widens the phone floor, keeps tablet geometry, returns idle bucket/sponge after 180 + 5 seconds and rearms completed flower/puddle after 60 + 5 seconds. Held tools stay protected. Windows, signed Android and unsigned native iOS builds pass, together with scoped regressions. Apple signing/installation remains. The later [server and Samsung 83 update](g3-persistent-server-2026-09-24.html) is now applied after user authorization: the authority has active timers and no home-session cutoff, Android has the wider layout/cues, and its native screenshot was inspected. iPhone/iPads remain 79 and their updates still await availability.

## Completed work and the next device session

Completed while away, using separate test worlds:

- [Verified server backup/restore and rollback](g3-server-recovery-2026-09-24.html).
- [Bounded crash recovery](g3-server-supervision-2026-09-24.html), including four original clients rejoining and deliberate-stop protection.
- [Ten-minute four-player Windows run](g3-server-soak-2026-09-24.html), covering movement, travel, timers, rejoin and save restart.
- [Parent backup/recovery buttons and status](g3-parent-recovery-panel-2026-09-24.html) in **Little Weeps Server** on the desktop. Your actual 83 world was backed up through this page and remains its original process; recovery activation is still Off.
- [Complete client recovery checkpoints](g3-client-recovery-2026-09-24.html) in Windows 88, preserving receipts, idle timers and the separate solo draft. This is not installed on your devices yet.

- [Automatic local continuation and saved adventures](g3-local-continuation-2026-09-25.html) in Windows 90: four-client outage/restart, preserved solo progress, reopenable adventures and safe automatic reunion pass. No devices were updated.
- [Outage failure checks](g3-outage-failures-2026-09-25.html) in Windows 91: accepted actions with missing completion, denied archive/selection/branch writes, observed foreground timing and rejected admission pass. Unresolved archives retry safely after disk access returns; they never replay actions. Related continuation, movement, compatibility and restore regressions pass.
- [Matching mobile artifact preparation](g3-mobile-recovery-builds-2026-09-25.html): family-signed Android 91 and iPad/iPhone 91 export built and source/bridge-checked on Windows. Android's strict 16 KB failure is retained; no mobile runtime or native Mac build is claimed.

- [Portable encrypted server recovery](g3-portable-recovery-2026-09-25.html): six isolated portable groups and six local-recovery regressions pass; four original test clients rejoin the recovered world. Actual off-PC storage and second-account/computer recovery still need qualification.

- [Android release recovery in the emulator](g3-android-recovery-2026-09-25.html): retained update, checkpoint/item clocks, Android warm/cold lifecycle, local outage play, safe reunion and two distinct adventures pass with three Windows siblings. No phone/iPad was updated.

- [Parent portable-backup controls](g3-parent-portable-backups-2026-09-25.html): six HTTP/native cases plus actual browser download/file checking and safe errors pass. Existing backup/recovery controls also pass their four regression groups. The actual helper will be relaunched deliberately in the return session.

- [Ten-minute mixed emulator/Windows recovery run](g3-sustained-recovery-play-2026-09-25.html) on 91: five acceptance groups cover movement/travel, normal resets, full durable checkpoints, Android Home/warm return, remote motion, diagnostic memory and seven retained local saves. Actual iPad performance and physical mixed play remain open.

- [Optional Windows sign-in startup](g3-signin-startup-2026-09-25.html): six native groups, ten existing backup/recovery regressions and browser controls pass. The test shortcut recovered a lost helper/authority and all four original players in 19.33 seconds. No real account Startup entry or OS sign-in/reboot was performed.

**Active: G3-REC-05 older-iPad recovery qualification and coordinated remaining updates.** Native compile/signing and iPad 7 data-preserving update now pass. Physical recovery still needs the matching server; iPad hosting and reconciliation remain required.

Remaining parent tasks:

1. Complete any required Windows network approval for the selected prepared server, then update it during an empty session and enable recovery through the parent page. The page is still configured for qualified operations build 85; newer recovery/continuation 91 must be selected and qualified for deployment deliberately. No prompt is being left waiting overnight.
2. Choose an independent backup destination. The new encrypted portable format and reconstruction pass isolated Windows tests; create the actual family copy there and verify a restore on another account/computer. Existing `.lwbackup` files remain tied to this Windows account.
3. Provide the VPS connection/OS details when convenient. Deployment stays behind server reliability.
4. Keep the deferred Apple cue/layout, automatic renewal and sustained device checks below on the list. Prepared recovery builds and emulator checks do not replace physical qualification or A10 write/frame measurements. No repeat of the already-passed smoke checks is needed.

[Persistent server 83](g3-persistent-server-2026-09-24.html) is deployed after Windows approval. The original save/enrollment is preserved, and Samsung updated in place and automatically joined. The native screenshot shows the wider layout. Both original local save files are byte-identical; the paired branch retained items/identity while recording walking commands. No Apple device was accessed. Optional sign-in startup now has isolated evidence, while actual activation/sign-in remains open. Sustained desktop/emulator runs are recorded above; physical long-duration and broader lifecycle acceptance remain open.

## Additional physical checks still open

- When devices are available for testing, finish detailed Samsung/iPhone shared-item, independent-area and lifecycle checks; repeat solo restoration per device as needed.
- Exercise a server-absent initial launch followed by late joining, remaining current-build iPad multi-touch/settings checks, and sustained four-device frame-time/memory measurements.
- Report any actual play issue as it occurs; already-passed controls do not need routine repetition.

## Still tracked

- Unattended app renewal on both iPads is unproven; the earlier iPhone USB renewal did not qualify Wi-Fi renewal. The last recorded development profile expires October 1, 2026; installed signing/profile status must be checked before the next update.
- Sustained performance on the older A10 iPad and observing the children use the controls remain open.
- Both iPads hosting, host switching/recovery, full offline reconciliation and the remaining game content are still in the main plan.

These checks will be handled one short step at a time. Do not delete the app or reset its data to troubleshoot a connection.
