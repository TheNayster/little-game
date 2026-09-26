# Family scenery rollout — build 110

**September 25, 2026 · FAMILY-01 / SCENIC-01 deployment.** The PC authority and parent helper now run **110**, matching the Samsung's scenic content 4. The original family world, four saved players, ten props and protected enrollment remain intact. The Apple devices still run 101; their fresh 110 export is prepared, with signing and device delivery pending.

## Applied server update

The old authority was idle. A verified backup was created before stopping it, automatic recovery was paused for cutover, and the old helper was closed. The authority stopped through its guarded save/quit path. Another verified stopped-world backup retained the final schema-2 state. The release launcher checked the new build's artifact manifest before starting 110.

The live upgrade matched the expected state exactly: **only schema 2 → 3 and revision 2778 → 2779 changed**. Every other saved gameplay field, player and item record remained unchanged. A new build-110 recovery bundle verified successfully. The parent helper settings now select 110, and its previously enabled crash recovery resumed healthy. Windows granted local-subnet UDP access to the exact build-110 server executable; no internet-facing firewall rule was added.

The phone's Android update was already completed and checked in the [scenery report](scenic-worlds-2026-09-25.html#android-phone-delivery). It can use the matching authority when opened in family mode. A fresh physical connection after this cutover has not yet been observed; the last phone evidence was from before the server update while it was unavailable. Private solo play is kept separate and is never uploaded to the server.

[Server deployment evidence](evidence/family-scenic-rollout-2026-09-25/server-deployment.json).

## Recovery qualification

- **Six native recovery groups pass on 110:** multi-area/four-player backup (including negative Home coordinates and Beach); active restore refusal; exact restore and rollback; invalid backup refusal; interrupted-restore startup blocking; missing-family reconstruction and original enrollment rejoin.
- **Two native upgrade groups pass:** an isolated 91 family upgrades with only schema/revision changes, all four original clients reconnect, and restoring a legacy 91 backup into 110 produces the same retained world.
- **Six portable recovery groups pass:** encrypted round trip, verification without the original DPAPI unprotect service, malformed/wrong-password/invalid-state refusal, interruption/concurrent-destination handling, reconstruction and four-client rejoin, and refusal to read a recovery passphrase from redirected input. These are same-PC isolated tests, not an independent-machine disaster-recovery qualification.

The recovery gate now allows the previously qualified 83–91 writers and **110 explicitly**. Intermediate experimental builds, including device-host build 99, remain excluded. This applies to local and portable recovery. Tests now use the scenic world chooser and reject schema 4 as unsupported.

[Native recovery](evidence/family-scenic-rollout-2026-09-25/native-recovery.json) · [Retained upgrade](evidence/family-scenic-rollout-2026-09-25/native-upgrade.json) · [Portable recovery](evidence/family-scenic-rollout-2026-09-25/portable-recovery.json).

## Apple delivery status

The fresh non-development iOS 110 export contains the matching game code and panoramas. All **3,095 exported files** were hash-verified after transfer to the paired Mac. Native compilation/linking reached codesigning; unattended signing stopped with `errSecInternalComponent`. The prepared Mac desktop helper **Finish-Little-Weeps-110.command** lets the user unlock the keychain and approve signing locally. No unsigned or old app was substituted.

Gabriel's iPad 7 backup succeeded over paired Wi-Fi with **535 files verified** on the Mac and Windows. The iPhone's USB backup verified **46 files**. Eduardo's iPad 9 repeatedly timed out during app-data copying, including a 120-second retry; it has not been updated. Device helpers now permit bounded longer copy/operation timeouts for wireless delivery while retaining the same backup, signature, version and data-preservation checks.

[Fresh Apple export](evidence/family-scenic-rollout-2026-09-25/apple-export.json) · [Verified backups](evidence/family-scenic-rollout-2026-09-25/apple-backups.json).

## Deferred Apple updates

The user asked to leave the Apple updates for later. No further signing, backup retries or Apple installation will proceed until they resume this work. Existing Apple apps remain installed and usable in solo. The server and Android delivery are complete; a fresh physical shared connection remains unobserved.

## Remaining work

Finish Mac signing; verify profile coverage for each device; complete the iPad 9 backup; install Apple 110 in place; verify each native runtime, saved Documents/preferences and original enrollment; observe physical shared joining with Android. The server is already updated and running. No new activities or artwork were changed during deployment. Older-iPad sustained memory/frame-time testing and visual approval remain open.
