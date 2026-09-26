# Both iPads and the family home server — build 128

September 26, 2026 · **FAMILY-01 / NET-02 / ART-HOME-02 delivery**

Both iPads are updated in place from 101 to 128 and have joined the original family world on the PC server, now also 128. Samsung already has the matching 128 app and also rejoined; both iPads and Android were observed on the same live authority epoch, with three physical players connected. No Android reinstall was needed. The iPhone remains 101 and was not part of this requested update.

## Retained iPad updates

The fresh release Apple export contains the current home composition, selected Bluey/Bingo artwork, combined Home/backyard navigation and accepted **420 units/second (2-times)** movement. Every current and future character uses that shared speed. All 751 Unity source/art/settings files matched the export manifest; all 3,095 export files matched after transfer to the Mac. Subsequent sprite metadata normalization only removed importer whitespace.

The first native compile reached signing but macOS returned `errSecInternalComponent`. The user completed the prepared Mac desktop signing helper locally. The resulting release passed strict code-signature, bundle/team identity, profile coverage for both iPads and all six native networking/enrollment bridge checks. All 39 signed payload files were hashed and rechecked before installation. [Source verification](evidence/family-home-rollout-2026-09-26/apple-source.json) · [Signature verification](evidence/family-home-rollout-2026-09-26/apple-signature.json).

| Device | Update | Retention before first launch | Native runtime |
| --- | --- | --- | --- |
| Gabriel’s iPad 7 | 101 → 128 | All 536 existing Documents files byte-identical; all previous preferences retained | 128, paired, connected to original family |
| Eduardo’s iPad 9 | 101 → 128 | All 1,491 existing Documents files byte-identical; all previous preferences retained | 128, paired, connected to original family |

Backups were copied to both Mac and Windows and hash-verified before installation. No app uninstall, reset, new enrollment or save replacement was used. iPad 7’s app inventory query remains unavailable; its successful installation/launch and device-written runtime 128 establish the installed version. These checks do not substitute for a visual review or sustained frame-time/memory measurements. [Device checks](evidence/family-home-rollout-2026-09-26/device-delivery.json).

## Server and recovery

The idle 110 authority received verified live and stopped-world backups. Automatic restarting was paused during the guarded save/stop; the old parent helper was closed only after verifying its process identity. Build 128 then loaded the original shared world.

Every prior gameplay field and protected enrollment byte remained intact. The migration changed schema 3 → 4, revision 2784 → 2785, and added the default home fixture state, empty fixture/container fields and the single home ball. All four saved players and the ten existing props remain; there are now eleven props. No offline client state was imported.

The user approved Windows’s administrator prompt for a local-subnet UDP rule tied to the exact new server executable. Persistent server and parent helper both run 128, advertise content 5, and report healthy automatic recovery. Both original iPad profiles and Android joined, giving three concurrent physical clients. [Deployment](evidence/family-home-rollout-2026-09-26/server-deployment.json).

Recovery permits 128 explicitly while retaining qualified older writers and excluding intermediate experimental builds. Backups continue to validate schema 4. Scoped isolated qualification passed:

- [Six native recovery groups](evidence/family-home-rollout-2026-09-26/native-recovery.json), including home radio/storage preservation, exact restoration, rollback, interruption guards and original four-client rejoin.
- [Two 110 → 128 upgrade groups](evidence/family-home-rollout-2026-09-26/retained-upgrade.json), including identical migration after restoring a legacy 110 backup.
- [Six portable-recovery groups](evidence/family-home-rollout-2026-09-26/portable-recovery.json), including authenticated round trip, refusal cases, interrupted recovery and reconstruction with original clients.

These tests used separate disposable Windows families; they do not qualify a second-PC disaster restore. The live family world was upgraded, never restored from test data.

## Remaining work

Keep the PC server available during shared play. Sustained mixed-device/A10 performance, focused outage/rejoin tests, automatic Apple renewal and iPhone delivery remain open. No full G3/G5/G6 phase completion is claimed. Next content work remains the kitchen’s separate surfaces, cupboards and appliance interiors; other worlds remain scenic.
