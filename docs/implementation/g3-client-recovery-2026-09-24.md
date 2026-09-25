# Durable client recovery checkpoints — G3-REC-01

**Goal IDs: NET-02 / AUTO-01; prerequisite shared with G4. Implemented and qualified within Windows scope on build 88.** This milestone adds complete checkpoint delivery and durable client storage. It does not switch a disconnected child to local play yet. The existing manual solo branch remains separate. The family server and phones have not been updated as part of this work.

## Why a separate recovery stream

The rendered shared-world view deliberately omits command receipts and idle clocks. Recovering from that picture could repeat an interaction or restart a return timer. `NetworkRecovery.cs` instead captures the complete immutable snapshot only after the authority's existing checkpoint write succeeds. It includes both current areas, all four profiles, props/contents/holds, visits, activity choices, all retained receipts and idle timers.

Each record also carries recovery format version, wire/content versions, enrolled family/authority/world identities, the checkpoint world's identity, authority epoch and an epoch-local checkpoint serial. A world revision alone cannot order walking or timer updates, because those can change without an inventory transaction. The serial handles those changes; a new authenticated epoch is accepted only with matching lineage/roster and a nondecreasing world revision. Deliberately restoring an older authority behind a client's recovery revision requires a future explicit reconciliation decision; it does not silently replace the newer replica.

This is the **entire current bounded prototype checkpoint**, including its maximum 128 receipts. It is not an unbounded operation journal or a final content-scale save format. Larger worlds and a committed-event tail still need their own protocol, storage and performance work.

## Transfer and storage contract

- Additive capability version 1 in the admission hello and state. Wire/content stay at 3. A server sends recovery traffic only to clients that requested version 1; new clients connected to older 79/83 authorities keep ordinary play without claiming recovery coverage.
- At most 256 KiB per complete record. Split into 3 KiB chunks, one outstanding chunk per recipient, one chunk globally per 50 ms, rotating among the four clients. A new offer waits five seconds after the previous durable acknowledgment. An incomplete transfer expires after 40 seconds; a failed attempt retries later. These are initial engineering budgets, not a promise of five-second maximum data loss.
- Each transfer carries a unique ID, epoch, exact total size and SHA-256 of the complete serialized record. Length, ordering, duplicate content, identity, format, roster and game-state validation must all pass. No partial transfer becomes a save.
- Store under the enrolled world's `client-recovery/<profile>/world.save`, using the existing flushed, staged replacement with checksum and previous valid backup. Never call the gameplay restore routine while merely saving a replica: doing so would release currently held toys and change the snapshot.
- The last acknowledgment means **saved and read back on that client**. The authority records that particular checkpoint's durable coverage; it does not treat a rendered state or a chunk acknowledgment as equivalent. Exact reconnect replays are idempotent. A failed disk write receives no durable credit, leaves the previous valid copy in place and does not stop other players.
- The child's separate solo save is preserved. No automatic fork, merge, authority election or host transfer occurs here. No authority private keys or other players' enrollment credentials are replicated.

Unity documents reliable fragmented delivery and warns that reliable streams can delay later traffic when earlier packets are missing. That supports using bounded, paced chunks and keeping continuous motion on its existing unreliable stream. Our chunk sizes, cadence and acknowledgment protocol are custom decisions, qualified here on Windows. The installed baseline remains NGO **2.13.2** and Transport **2.7.4**; no package upgrade was made. [Unity delivery modes](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/api/Unity.Netcode.NetworkDelivery.html), [Transport 2.7 pipelines](https://docs.unity3d.com/Packages/com.unity.transport@2.7/manual/pipelines-usage.html).

## Verification

| Qualification | Result and limits |
| --- | --- |
| [Windows server and client build 88](evidence/client-recovery-2026-09-24/build-88.json) | Unity 6000.3.24f1; both succeeded with zero warnings/errors. All 144 current Unity C# files match the build's recorded hashes. [Source provenance](evidence/client-recovery-2026-09-24/source-provenance.json). |
| [59 core/storage checks](evidence/client-recovery-2026-09-24/rules.json) | All pass, including seven new recovery groups. Maximum-size assembly, duplicates, partial/reordered/mixed/corrupt/truncated transfers, timeout, stale serial/revision, wrong family/authority/world/roster, exact replay, newer formats, denied writes and preserved separate draft are exercised. |
| [Four native recovery groups](evidence/client-recovery-2026-09-24/native-88.json) | Four enrolled Windows clients receive a 22,457-byte checkpoint with 128 receipts, two areas and a held prop. Denying one client's disk access gives no false durable acknowledgment and does not interrupt siblings. Client/server restart resumes replication; authority loss leaves committed replicas intact. The manual solo draft stays byte-identical across rejoining. |
| Client write observations | The four recorded durable commit/read-back observations were 4.63–5.74 ms on this Windows PC. These are individual observations, not p95 values, a full profiling run or an A10 budget. |
| [Four staggered-version combinations](evidence/client-recovery-2026-09-24/compatibility-88.json) | 88 authority with 79/83 client, and 79/83 authority with 88 client: admission, pickup/fill and shared view pass. Legacy peers negotiate no recovery traffic and receive no durable-replica credit. |
| [Four-player native motion](evidence/client-recovery-2026-09-24/motion-88.json) | The receipt window was filled before the measurement. Four players walked concurrently; 28 recovery chunks were observed during the trace window. Remote moving-frame fraction 1.0, median frame 16.65 ms, p95 visual speed 242.39 against the existing <420 budget, and p95 visual delay behind received state 0.211 seconds against <0.4. Menu cancellation, diagnostic-file contention and exact stopped-position restart pass too. This is loopback desktop motion, not physical Wi-Fi or a sustained mobile run. |
| [Six server recovery groups repeated on 88](evidence/client-recovery-2026-09-24/server-recovery-88.json) | Live backup, exact restore/rollback, invalid archive refusal, interrupted-recovery guards, damaged-primary repair and missing-directory reconstruction all pass. The local recovery tool's qualified source-build ceiling advances to 88. This remains same-user Windows recovery. |
| [Actual deployment scope](evidence/client-recovery-2026-09-24/deployment-scope.json) | Original family 83 process/instance still listening; revision 1866 checkpoint byte-identical to the verified backup. Parent configuration stays 85; crash supervision remains Off. No phone, iPad or Mac access. |

The first native harness used the wrong numeric value for the existing Travel command; that setup request was correctly rejected. Correcting the test enum value allowed the four-client run to pass. Review then tightened replay roster checks, unknown nested-format handling and failed-transfer backoff. Final qualification uses fresh binaries containing those changes.

## Deployment and remaining work

The original actual family server **83** was not restarted, replaced or restored. The parent control page still targets prepared **85** for its reviewed activation step; automatic crash recovery is Off until that deployment is approved. New client recovery is a later coordinated server/client update and has not been exported to iOS or Android in this milestone.

Device-specific atomic-file behavior, write/frame cost on the A10, packet loss/route changes, sustained physical-device replication, iPad authority roles, event-tail replication and independent backups remain open. The ten-minute 85 run is still a desktop result, not a soak of this new protocol. Checksums detect corruption; authenticated enrollment/DTLS supplies trust on the family connection.

**Next bounded task: G3-REC-02 — automatic local continuation after prolonged loss using this last verified complete checkpoint.** Preserve the current solo draft, create an explicitly identified separate continuation branch, settle stale holds and unconfirmed actions safely, and make startup/reconnect handling understand that branch. Do not automatically discard it on reunion. G4 election/handoff and G5 conflict reconciliation remain required. Keep the goal sheet's no-lobby, usable-toys and recoverable-work behavior; do not substitute an old solo picture and call the requirement complete.

[Main build guide](../family-playset-build-guide-2026-09-23.html#9-first-implementation-work-queue) · [Return checklist](return-checklist-ipad-lan-2026-09-24.html)
