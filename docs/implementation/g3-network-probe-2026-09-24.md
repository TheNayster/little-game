# G3 preparation — one PC authority and four Windows clients

24 September 2026. Bounded task: **FAMILY-01, JOIN-01, ITEM-02**. The user requested continued work while away from home, without the Mac or physical mobile devices. This is provisional G3 preparation; G1/G2 device and child-observation gates remain open, and G3 is not complete.

## What this implements

An isolated native Windows Dedicated Server and separate native Windows clients exchange commands and snapshots through Unity Netcode for GameObjects and Unity Transport. They use the same garden rules as solo play: stable player/object identities, one holder per prop, water quantities, character changes and optional activities. The test controls submit through each client's actual network connection; they do not edit the server's world file to simulate multiplayer.

`FamilySession` binds a transient transport connection to an admitted player profile. A client cannot act as another player. Disconnecting a player releases that player's held prop without resetting another player's position, activity or held toy. Accepted mutations are checkpointed before the authority acknowledges and broadcasts them. A restarted authority loads that checkpoint, retains command receipts, clears stale holds and uses a new authority epoch.

The visible solo garden is separate. This probe has no gameplay interface, automatic pairing, discovery, mobile client transport or host selection. Its listener is explicitly restricted to **127.0.0.1**. Every test creates its own random credentials and save folder. No firewall, router, normal garden save or physical installation is changed.

## Package decision and sources

Unity remains pinned to **6000.3.24f1**. Added direct pins are **Netcode for GameObjects 2.13.2** and **Unity Transport 2.7.4**. The existing resolved Burst 1.8.30, Collections 2.6.8, Mathematics 1.3.3 and Mono.Cecil 1.11.6 versions did not change. Both manifest and lockfile are tracked.

The earlier package report identified a status discrepancy around NGO 2.13.2/2.13.3. This bounded implementation uses 2.13.2 and records its local results; it does not certify the combination for either iPad or Android. The current editor catalog identifies Transport 2.7.4 as released. Package metadata was also checked directly against Unity's registry and the installed package source before implementing the adapters. [NGO editor catalog](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.netcode.gameobjects.html), [Transport editor catalog](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.transport.html).

The adapter uses documented connection approval and named custom messages. Runtime-created NetworkManagers need an explicit NetworkConfig; a plain nested command also needs serializable metadata for Unity's JSON serializer. Both were caught by native runs, corrected and preserved in failed-run evidence. [Connection approval](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/connection-approval.html), [custom messages](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/advanced-topics/message-system/custom-messages.html).

Approval tokens in this harness are test credentials, not a finished authentication or encryption design. Do not expose this probe on the LAN or internet. The family pairing/secure-session work needs its own implementation and acceptance tests before that boundary changes.

## Acceptance record

**Native build 48 passed all 10 network scenarios**, using one dedicated server and up to four simultaneous separate client processes. Both release-configured Windows binaries built with zero summary errors/warnings. **All 25 core/save/session tests passed.** [Network result](evidence/network-probe-2026-09-24/network-48.json), [build identities](evidence/network-probe-2026-09-24/build-48.json), [rules](evidence/network-probe-2026-09-24/rules-25.json).

| Scenario | Observed result |
| --- | --- |
| Admission | Wrong token/version, duplicate profile and unknown fifth profile rejected; the two existing players' world stayed unchanged |
| Simultaneous bucket pickup | Exactly one player won; another could not steal it; changing avatar retained the hold |
| Third/fourth late join | All four clients converged on the same filled bucket and its current holder |
| Actor spoofing | A valid fourth player could not release another player's prop by supplying that player's ID |
| Independent actions and duplicate delivery | Another player moved/started cleanup/held the sponge; replaying a completed pour did not transfer water twice |
| Abrupt client loss | After deliberately terminating one test client, remaining clients continued commands; only the departing holder's bucket was released, observed after 2.516 seconds in this run |
| Return and ordinary leave | Returning profile retained avatar/receipt history; graceful departure allowed the remaining player to continue moving |
| Server hard stop and recovery | Committed world survived, stale holds cleared, authority epoch changed; explicit test-driven reconnection and replay of the earlier pour preserved state |

The 2.516-second observation is one loopback measurement, not a mobile/network guarantee. Automatic reconnect and host replacement are not implemented. The runner terminated all its processes afterward; no game server remains listening.

The existing garden was rebuilt as **49** after adding the packages. Tablet-shaped Windows input, two-pointer arbitration, hints and muted narration checks passed; the isolated **43 → 49** update and damaged-primary recovery suite preserved progress. [Input](evidence/network-probe-2026-09-24/solo-input-49.json), [update](evidence/network-probe-2026-09-24/solo-update-49.json), [recovery](evidence/network-probe-2026-09-24/solo-recover-49.json). Its presentation and normal save location remain unchanged.

Refreshed **iPad/iPhone solo export 50** also passed Windows-side inspection after the package additions: all **3,083** artifact hashes, the original app ID, ARM64, iPhone/iPad device families, minimum iOS 15.0 and generated IL2CPP game code were verified. [Inspection](evidence/network-probe-2026-09-24/ios-export-50.json). The export is at `Builds/iOSSolo/G2-0.0.50/Xcode`. It supersedes export 44 as the prepared solo export; it is unsigned, uncompiled in Xcode and not installed. This is not an iOS networking build or device networking proof.

Builds **46 and 47** are failed runtime attempts, not accepted multiplayer artifacts: 46 exposed missing runtime NetworkConfig initialization; 47 exposed command serialization omission. [Failure 46](evidence/network-probe-2026-09-24/failed-46.json), [failure 47](evidence/network-probe-2026-09-24/failed-47.json). The exact successful harness revision additionally checks receipt replay after server restart; it was extended after the native build, so [both harness hashes](evidence/network-probe-2026-09-24/harness-48.json) are recorded separately from [build-time sources](evidence/network-probe-2026-09-24/source-48.json). [Artifact hashes](evidence/network-probe-2026-09-24/artifacts-48.json) pin both executable trees.

## Deliberate limits

- One small logical garden; no independent rooms or cross-area carrying yet.
- Four separate Windows processes are not four physical platforms, actual touches or an iPad performance test.
- Commands use the prototype's global expected revision. Concurrent unrelated actions can be rejected as stale. A production client needs an explicit retry/rebase policy or finer conflict boundaries; silently ignoring these responses is not acceptable.
- Full snapshots after changes, disk writes per mutation and the short test disconnect timeout are qualification choices, not measured mobile bandwidth, latency, frame-time or storage budgets.
- Visible snapshots omit command receipts. They are not successor checkpoints. Full recovery replication, authority election, fencing, automatic host switching, offline reunion and partition handling remain required later work.
- The bounded 128-entry receipt history covers this probe's retry window. It is not an unlimited journal or a promise that arbitrarily old requests can be replayed safely.
- The harness drives restart and rejoin explicitly. It does not prove automatic reconnect, uninterrupted play when the server itself stops, or mobile background hosting.
- Earlier iPad export 44 and Android emulator artifact 45 predate these package additions. Their earlier results remain valid for those exact artifacts, not the new package configuration. Solo export 50 now checks the updated iOS export path only; Android has not been rebuilt with this package configuration yet.

## Repeat and continue

With this Unity project closed, build using a fresh number, then run the native test:

~~~powershell
.\Tools\Build-NetworkProbe.ps1 -BuildNumber N
python .\Tools\Test-NetworkProbe.py N
~~~

N must be at least 46 and unused. Both executable trees are hash-verified before launch. Run-specific credentials, checkpoints, logs and process evidence remain ignored under `LocalData/NetworkProbe/<run-id>`. Source/artifact manifests and native builds are under `Builds/NetworkProbe/G3-0.0.N`. The runner shuts down only processes it created.

Next Windows-only task, after this probe passes: connect the existing garden presentation to two isolated local clients and demonstrate acknowledged pickup/drop and sibling motion with clear pending/rejected-input handling. Keep pairing/discovery, a second logical area and physical mixed-device testing as separate bounded tasks. Resume [iPad/G2 checks](g2-device-checklist.md) when the devices are available; iPad hosting and automatic recovery remain mandatory G4/G5 work.
