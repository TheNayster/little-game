# Outage failure checks without physical devices

**G3-REC-03 · NET-02 / AUTO-01 · September 25, 2026 · Windows build 91.** The four planned failure areas now have native Windows evidence. The new build fixes one defect: when saving unresolved actions fails during disconnect, their original IDs and payload are retained in memory and the same archive is retried. The queue is cancelled without replaying those actions.

## What changed and what passed

Previously, an archive write failure only produced a warning before the live command queue was cleared. That could forget the unresolved intentions. Build 91 retains the serialized batch and its unique path before clearing the queue, retries at two-second intervals, and removes it only after a verified write/read-back. Retrying an archive never sends a gameplay command.

The [five native failure groups](evidence/outage-failures-2026-09-25/native-91.json) use four enrolled clients and an isolated encrypted authority:

1. A real pour is accepted by the authority while its client completion is suppressed. A filesystem obstruction prevents archiving on disconnect. The retained batch survives repeated failure, writes after the obstruction is removed using the same request ID, and remains unchanged after reunion. The pour is not applied twice; the other clients stay connected.
2. A Windows file lock prevents replacement of the selected-adventure pointer. The old pointer and solo save remain unchanged; presentation does not falsely switch. Once unlocked, the same pending adventure opens without creating a second branch.
3. A backgrounded client does not start an adventure because wall-clock time passed. After calling the foreground adapter, it waits the required observed foreground interval; the recorded transition was **10.125 seconds after resume** in this run.
4. A file lock prevents saving a changed local adventure while the server returns. The client retains its live local work and postpones reunion; the other three rejoin. Removing the lock commits the local drop before entering the shared world.
5. The native authority rejects a test enrollment with a wrong credential. The rejected client can still move toys and save locally; the child menu does not create an authentication retry loop, and the other three players keep playing.

The first harness attempt addressed the tap as an input surface even though it is a drawn target. The harness was corrected to use the scene's actual target coordinates. The first motion regression also failed its concurrent-transfer assertion: completion-based recovery counters did not increase during that short walk. Recovery has a five-second per-peer offer cooldown, so the harness now waits for idle checkpoint convergence and cooldown expiration before starting measured motion. It retains the before/after counters and keeps the same motion and transfer thresholds. [Initial attempts and corrections](evidence/outage-failures-2026-09-25/initial-attempts.json) remain recorded; neither failed attempt is counted as a pass. All worlds were isolated from family data.

## Build and regression evidence

| Evidence | Scope |
| --- | --- |
| [Windows client/server 91 build](evidence/outage-failures-2026-09-25/build-91.json) | Unity 6000.3.24f1; zero errors/warnings. [148 current C# sources match the build manifest](evidence/outage-failures-2026-09-25/source-provenance.json). |
| [67 core/storage checks](evidence/outage-failures-2026-09-25/rules.json) | Existing save, identity, ownership, timing and continuation rules pass. The new archive retry is covered by the native failure test above. |
| [Six continuation regression groups](evidence/outage-failures-2026-09-25/continuation-91.json) | Four clients, authority loss, non-first actor/local travel, held-toy client crash/cold resume, safe reunion, original solo, saved-adventure reopening and repeated outages. |
| [Legacy compatibility](evidence/outage-failures-2026-09-25/compatibility-91.json) | Server/client 91/79, 79/91, 91/83 and 83/91 admission and basic interaction. |
| [Four-player motion](evidence/outage-failures-2026-09-25/motion-91.json) | Desktop moving-frame, correction and lag checks with recovery traffic, plus menu cancellation and stopped-position restart. |
| [Server recovery regression](evidence/outage-failures-2026-09-25/server-recovery-91.json) | Isolated backup, exact restore/rollback, bad archives, interrupted restore, damaged-primary repair and missing-directory reconstruction. |
| [Scope and harness provenance](evidence/outage-failures-2026-09-25/scope.json) | Test worlds only; no family-server update, phone/iPad/Mac interaction or enrolled-family save changes. |

The corrected four-player run measured **28 recovery chunks during the trace**, a **4.203-second** walk, **five** authority checkpoint writes, and a **16.66 ms** median remote render interval. The remote character advanced on every sampled moving frame, versus 32.9% for raw received positions; measured 95th-percentile visual lag behind received state was 0.208 seconds. [Transfer readiness](evidence/outage-failures-2026-09-25/motion-transfer-readiness.json) and [before/after counters](evidence/outage-failures-2026-09-25/motion-transfer-measurement.json) support the overlap claim. These are desktop measurements, not A10 or end-to-end network latency results.

## Limits and next step

The completion suppression is an explicit test hook after a real accepted native action; it is not a whole-network packet-loss emulator. The lifecycle test calls the same foreground adapter used by the app; Windows continues executing, so this does not qualify real iPad suspension. Filesystem tests deny directory creation or atomic file replacement; they do not physically fill a device's disk.

An unresolved action is **not an accepted-event journal**. If the app is killed while the disk still refuses every archive write, an in-memory batch cannot survive that kill. The authority's durable data and the client's last verified checkpoint remain the recovery sources. Archives are never automatically replayed or merged. Separate adventures remain separate, and automatic reunion can still wait for an interaction boundary during continuous input.

**G3-REC-03 is complete within this Windows scope. G1/G2/G3 remain partial.** This is not iPad hosting, election/handoff, automatic reconciliation, prolonged real-network partition qualification, sustained A10 profiling or a mobile release. The ten-minute earlier soak remains a build-85 Windows result.

**Next: G3-REC-04 — prepare coordinated mobile recovery artifacts on Windows**, retaining app identities, signing and saves. Android release and iOS export preparation can proceed without physical devices. Native iOS compilation/signing and physical acceptance are separate steps; do not install or qualify unseen devices by inference. Test the older iPad first when the user is home, followed by the newer iPad and both phones. Keep server activation, independent backup and unattended renewal tracked; both iPads hosting stays G4 and reconciliation/rooms/item policy stays G5.

[Current build plan](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) · [Short return checklist](return-checklist-ipad-lan-2026-09-24.html)
