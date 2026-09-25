# Current checks — server/Android 83; Apple clients 79

**Updated September 25 — no action is needed while you are away.** Windows 91 has passed the isolated outage failure matrix and related regressions. Matching Android 91 and the iPad/iPhone 91 Xcode export are now [prepared on Windows](g3-mobile-recovery-builds-2026-09-25.html). Native Apple compilation/signing and all physical updates/tests remain pending. The last recorded deployed versions above have not changed. Android's separate 16 KB qualification issue remains open.

**Completed while you were away:** password-protected portable server recovery passed with a separate test family and all four original test players. Your live world and devices were not changed, and no family passphrase was created.

## Your to-do list when you are home

**Start with these setup items. No passwords or verification codes need to be sent in chat.**

1. **Mac and older iPad:** turn on the Mac, leave it awake on home Wi-Fi, connect the older **iPad 7 / A2197** by USB and unlock it. Keep Little Weeps closed on the iPad's Home Screen for the update. The newer **iPad 9 / A2602** comes next; having its cable ready helps.
2. **Tell me the devices are ready and whether anyone is playing.** I will verify backups, signing, installed versions and the prepared candidate before an in-place update. If a Mac password or signing prompt appears during that session, enter it on the Mac. Nothing needs to be approved while you are away.
3. **Choose a backup destination:** an external drive or another computer, separate from this PC's current game folder. Send the destination when convenient. The portable encrypted backup/reconstruction tool now passes isolated Windows tests. We still need a real copy on that destination and a restore on another Windows account/computer. Choose and save its passphrase privately when we export it; do not send it here. [What is ready and what remains](g3-portable-recovery-2026-09-25.html).

**After I install and launch the checked candidate, we will do these together:**

- On the older iPad first, check offline start, local play during an outage, closing/reopening, and family reunion with the local adventure still available. I will arrange the isolated test server and give the exact short steps; do not shut down the family server yourself to simulate a failure.
- Repeat the relevant recovery checks on the newer iPad, then Samsung and iPhone. Check actual lock/background behavior, four-player movement and the updated phone layout/reset cues. Existing successful smoke tests stay recorded.
- Update the family authority and enable its qualified recovery controls during an **empty session**. The current parent page still selects operations build 85 and recovery is Off. I will handle the matching build and backups; approve a Windows prompt only if the reviewed setup actually needs one.

**Still pending:** reliable unattended renewal on both iPads. Keep the USB cables available; Wi-Fi refresh has not passed. The last recorded development profile expires October 1, so we will check the installed profile before updating.

VPS details can wait until the reliability and deployment gates are ready. There is no need to reconnect devices or troubleshoot anything remotely right now.

Both iPads were **updated in place and launched on 79**. Their earlier garden saves and settings were preserved, verified backups are on both computers, and each iPad has its own Keychain family identity. Signing for that earlier build is complete. The new 91 candidate needs its own verified native build/signing session; do not rerun an old signing shortcut. [Earlier installation evidence](g3-ipad-79-2026-09-24.html).

**Both iPads, Samsung and iPhone have joined the same PC shared world.** Native discovery and admission are verified, and the user confirms all devices work. After everyone left, the user authorized the PC server and Samsung update to 83. Those updates are complete; iPads/iPhone remain 79. [Four-device record](g3-phones-79-2026-09-24.html).

## iPad progress — further hands-on checks paused at user request

1. **Done: both apps open, local-network permission allowed, both joined.** No further iPad action is currently requested.
2. **Done by user report: smooth two-way movement and one shared bucket.** Both iPads show the other player moving; the bucket can only be held by one player, then dropped and picked up by the other. [Recorded feedback](evidence/ipad79-2026-09-24/movement-bucket-feedback.json).
3. **Done by user report: independent areas and reunion.** One player visited Creek while the other kept playing in Garden; they saw each other again on reunion. [Recorded feedback](evidence/ipad79-2026-09-24/independent-travel-feedback.json). Shared-build joystick plus dragging, Listen, remembered settings and full-screen layout still need their separate check.
4. **Done: leave and return on both iPads.** The user confirms locking/unlocking and also closing/reopening while the sibling continues; movement and bucket pickup work afterward. Native reads confirm retained identities and automatic rejoin from new app processes to the same PC authority. [Evidence](evidence/ipad79-2026-09-24/lock-cold-rejoin.json).
5. **Passed on the tested iPad: saved solo round trip.** The user confirmed easy family rejoin with server positions and restoration of the solo toy position when returning to Play by myself. [Feedback](evidence/ipad79-2026-09-24/solo-roundtrip-feedback.json). Which iPad was used and repetition on both were not specified. Further iPad time is paused. Initial joining after a server-absent start and remaining multi-touch/layout/voice/settings checks remain deferred.

The previous unpaired solo garden, paired-device solo draft and server world are separate. The update preserves the old save; it does not merge it into the family garden. Windows 90 qualifies automatic local adventures and safe reunion; Windows 91 adds failure/retry evidence. Physical device qualification and full offline reconciliation still need their planned work.

## Phone milestone — completed within the recorded scope

6. **Done: Samsung 15 → 79 in place.** Original signing identity, installed APK hash, native enrollment/discovery and automatic family admission verified. Full layout, walking, bucket fill/pour and Listen passed by user report. Accessible files were backed up; exact private foundation-preference retention was not independently read back.
7. **Done: iPhone 20 → 79 in place and all four connected.** USB Sideloadly update completed, native version matched and all three prior preference values remained. The fourth identity was consumed into Keychain and joined automatically. User reports the full layout, walking, bucket drag and Listen work. Server evidence confirms four distinct physical clients together. This does not complete sustained four-device gameplay/lifecycle acceptance.

## Phone layout and resets — Android/server 83 applied

The user requested less phone side space and timers for unused items/completed quests. [Build 82](g3-phone-layout-resets-2026-09-24.html) widens the phone floor, keeps tablet geometry, returns idle bucket/sponge after 180 + 5 seconds and rearms completed flower/puddle after 60 + 5 seconds. Held tools stay protected. Windows, signed Android and unsigned native iOS builds pass, together with scoped regressions. Apple signing/installation remains. The later [server and Samsung 83 update](g3-persistent-server-2026-09-24.html) is now applied after user authorization: the authority has active timers and no home-session cutoff, Android has the wider layout/cues, and its native screenshot was inspected. iPhone/iPads remain 79 and their updates still await availability.

## Next Windows task

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

**Next: G3-REC-05 native Apple compile/signing and iPad-first recovery qualification.** Prepared artifacts are ready for this later session. Physical qualification waits for you; iPad hosting and reconciliation remain required.

When you return:

1. Complete any required Windows network approval for the selected prepared server, then update it during an empty session and enable recovery through the parent page. The page is still configured for qualified operations build 85; newer recovery/continuation 91 must be selected and qualified for deployment deliberately. No prompt is being left waiting overnight.
2. Choose an independent backup destination. The new encrypted portable format and reconstruction pass isolated Windows tests; create the actual family copy there and verify a restore on another account/computer. Existing `.lwbackup` files remain tied to this Windows account.
3. Provide the VPS connection/OS details when convenient. Deployment stays behind server reliability.
4. Keep the deferred Apple cue/layout, automatic renewal and sustained device checks below on the list. Client recovery replication and local adventures also need mobile builds and A10 write/frame measurements. No repeat of the already-passed smoke checks is needed.

[Persistent server 83](g3-persistent-server-2026-09-24.html) is deployed after Windows approval. The original save/enrollment is preserved, and Samsung updated in place and automatically joined. The native screenshot shows the wider layout. Both original local save files are byte-identical; the paired branch retained items/identity while recording walking commands. No Apple device was accessed. Sign-in startup/status, long-duration soak and broader lifecycle acceptance remain open.

## Deferred hands-on checks — no action requested now

- When devices are available for testing, finish detailed Samsung/iPhone shared-item, independent-area and lifecycle checks; repeat solo restoration per device as needed.
- Exercise a server-absent initial launch followed by late joining, remaining current-build iPad multi-touch/settings checks, and sustained four-device frame-time/memory measurements.
- Report any actual play issue as it occurs; already-passed controls do not need routine repetition.

## Still tracked

- Unattended app renewal on both iPads is unproven; the earlier iPhone USB renewal did not qualify Wi-Fi renewal. The last recorded development profile expires October 1, 2026; installed signing/profile status must be checked before the next update.
- Sustained performance on the older A10 iPad and observing the children use the controls remain open.
- Both iPads hosting, host switching/recovery, full offline reconciliation and the remaining game content are still in the main plan.

These checks will be handled one short step at a time. Do not delete the app or reset its data to troubleshoot a connection.
