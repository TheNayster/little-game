# Shared-play regressions after the 171 family rollout

September 27, 2026. Active correction on `codex/home-kitchen`; Home remains Partial.

The user reported repeated server/reconnection interruptions, silent books and a frozen cooking countdown after the iPads and original family server moved from 128 to 171. The update preserved saves and enrollment, but the earlier tests did not establish sustained physical multiplayer or audible iPad book playback. These reports are unresolved acceptance failures, not completed features.

## Findings and applied correction

| Symptom | Evidence | Correction in candidate 172 |
| --- | --- | --- |
| Cooking countdown stays still during shared play | A new four-client release-player test fails on 171: server heat advances, while all four client heat values remain zero until completion. Existing tests checked the final cooked state, not intermediate text. | Send authoritative heat samples every 100 ms, keyed to the exact dish identity. All four clients receive intermediate progress; the visible countdown now changes from 8 through 1 in the native test. Completion remains a reliable server-owned state change. |
| Repeated reconnects | The same production server process/instance remained running. Server logs contain client protocol timeouts and repeated packet-queue-full warnings. Drag previews were broadcasting the enlarged full Home snapshot on each update. Some iPad stalls also coincided with unsuccessful audio debugger attachments; those attempts were stopped and no debugger remains attached. | Negotiate a small activity stream. Send individual disposable drag/heat samples instead of whole-world drag snapshots; preserve reliable transactions, exclusive holders, saved state and recovery. Set the transport packet queue to 256 to leave room beyond four reliable windows. Older clients retain their existing full-state format at a bounded cadence during rollout. No claim that this explains every observed timeout. |
| Silent books | Installed source assets and voice preference were present, but physical audible output has not been established. Resource completion was treated as clip readiness, and `Play()` was immediately treated as completed speech if playback had not started yet. | Explicitly load audio data, wait for readiness and observe playback starting before allowing completion/page turning. Add bounded local audio diagnostics for clip state, samples, listener/source state and iOS output route/volume. Diagnostics neither attach a debugger nor change mute, routing or device volume. Physical audio cause/acceptance remains open. |

The save schema/content remain 15/16. No player profile, enrollment, room, ingredient, creation or bookmark is reset. Progress samples can update only the same dish identity or active holding lease; delayed reliable snapshots cannot rewind newer progress. The server remains the sole shared authority and private offline edits remain separate.

## Research actually applied

- [Unity's network optimization guidance](https://mp-docs.dl.it.unity3d.com/netcode/2.3.2/learn/bossroom/optimizing-bossroom/): reduce bandwidth and use disposable updates for transient motion; size packet queues for all reliable windows and control traffic. Checked against this project's installed NGO 2.13.2 `UnityTransport` implementation, which retains unsent packets and has distinct packet/send-queue limits.
- [Unity 6000.3 AudioClip.LoadAudioData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioClip.LoadAudioData.html): a loaded clip object and loaded audio data are separate. Playback may be delayed by loading. The reader now checks audio readiness and observed playback rather than assuming a `Play()` call proves sound.

## Qualification and delivery

- [171 countdown regression](evidence/shared-play172-2026-09-27/countdown171-failure.json) reproduced in four isolated clients.
- [172 countdown test](evidence/shared-play172-2026-09-27/countdown172.json) passes for all four clients, including the actual displayed countdown.
- Six reader titles pass cold load, deliberate playback, pause/resume sample retention and close cancellation in the Windows native player. Test audio is muted; this does not prove audible iPad speakers.
- Three minutes of four simultaneous native object drags passed with no disconnects or send-queue errors. [Native reader/drag evidence](evidence/shared-play172-2026-09-27/native-shared-play.json). All six final-172 native recovery groups pass. [Recovery evidence](evidence/shared-play172-2026-09-27/recovery172.json). A second countdown run with a retained 171 client also passes, including that client’s intermediate progress. [Mixed-version evidence](evidence/shared-play172-2026-09-27/mixed-countdown172-171.json). Physical-device qualification remains open.
- Windows release client/server 172, signed Android 172 and iOS export 172 are built. The Mac stopped responding before the new iOS export could be transferred/signed.
- The complete native chocolate-cake flow passes again on 172: four independent picks, partial mixing/pouring, independent bakes, filling/stacking/icing and four conserved servings. [Cake-flow evidence](evidence/shared-play172-2026-09-27/cake-flow172.json).
- The live 171 world has a fresh verified backup at revision 3902. The Windows administrator prompt for candidate 172's local-network rule was canceled; no firewall change or server replacement occurred. At the latest check, two players remained connected. Deployment waits for the players to leave and the required Windows permission.
- Production server and installed devices remain 171. The live world is left running while players are connected. iPhone remains 101.

Do not promote this development lineage to main or claim every recent Home feature works in shared play. The candidate's scoped native regression/recovery checks pass. Remaining work is the Mac native build/signing, coordinated in-place delivery, physical countdown/connection/audio verification and sustained older-iPad play. Remote held-object visual smoothness also needs physical review; the traffic stress test checks transport health rather than rendered motion quality.
