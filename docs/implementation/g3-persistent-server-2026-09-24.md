# Persistent home server — build 83

Goal **NET-02**, G3. User request: explain the two-hour cutoff and make normal home hosting keep running during family play.

## Why it stopped

The Windows networking prototype shared one lifetime check between automated probes (240 seconds) and interactive sessions (7,200 seconds). `Start-FamilyLAN.py` launched the family authority as an interactive session, so the laboratory deadline also stopped the home server. This was a test-runner limit, not a Unity, iPad, multiplayer or licensing requirement. The phone/timer correction in build 82 did not change it.

## What changed

- Normal enrolled home-server launches now explicitly request **persistent hosting**, and verify that the server acknowledged it. There is no time limit or empty-player shutdown in that mode.
- Automated probes retain their original deadlines. Persistent mode requires an enrolled, interactive authority; a client or unpaired test cannot silently opt into it.
- With nobody connected, garden activity timers pause while the server remains available. This is separate from process lifetime.
- The existing single-writer save lock, durable checkpoints and controlled quit path remain. A second authority cannot open the active save.
- `Restart-FamilyServer.py` refuses to replace an occupied server, checks process identity, stops an idle server cleanly, backs up its save/enrollment locally, verifies the checkpoint and compares all existing saved fields after restoration. It never uninstalls clients or creates new family identities.
- A reviewed administrator helper grants only the selected server executable inbound UDP from **LocalSubnet**. It disables only the matching build's first-run block rules. It does not turn off Windows Firewall, change other apps' rules or open a router port.

The PC must remain powered on, awake and connected. This does not yet install a Windows sign-in task, service or crash supervisor, and does not implement required iPad hosting. "Persistent" means the server no longer exits on a test timer; it is not a guarantee against operating-system shutdowns or power loss.

## Evidence

| Check | Result |
| --- | --- |
| Core rules | **52 passed**, including exact timed-test boundaries and persistent lifetime values through 30 days. [Results](evidence/persistent-server-2026-09-24/core-52.json). |
| Windows build | Client and dedicated server **0.0.83** built successfully with zero build errors. [Summary](evidence/persistent-server-2026-09-24/windows-build-83.json). |
| Runtime lifetime and recovery | **6 checks passed**: timed probes still expire; persistent runtime crosses the former boundary while empty; four players join and interact; a duplicate writer is refused; controlled restart preserves state and all four client processes rejoin; the last departure leaves an available unchanged world. [Results](evidence/persistent-server-2026-09-24/native-lifecycle-83.json). |
| Staggered client updates | **83 server / 79 client** and **79 server / 83 client** pass admission, pickup, filling and shared state. [Results](evidence/persistent-server-2026-09-24/compatibility-83.json). |

The native cutoff test supplies an explicit verification-only elapsed offset to the same lifetime branch. It is **not a real two-hour soak**. The production launcher supplies no offset. No new physical-device restart/soak acceptance is claimed. Build 83 includes build 82's garden timers; older 79 clients can interact with that state but do not display the new return cue. Their wider phone layout still requires a client update.

## Deployment status

**Prepared, not yet applied.** Windows currently blocks inbound network access for the new executable. The administrator approval prompt was canceled; no firewall change was applied. The original build 79 server reached its existing limit at approximately 9:26 PM CDT on September 24 with **zero connected players**. Its save checksum was verified and a separate local backup preserves all four players, ten objects and existing progress. [Pending deployment record](evidence/persistent-server-2026-09-24/deployment-pending.json). Build 83's normal deployment must wait for the Windows permission, then use the original family and preserve its checkpoint.

The helper and idle-replacement workflow are prepared. The native launcher/restart behavior is tested above; the full production replacement script still requires an actual approved deployment. Automatic startup after PC restart, long-duration measurements and physical-device rejoin confirmation remain open. G1/G2/G3 are not declared complete.

## Next

Approve the scoped Windows firewall helper, start build 83 against the saved family using the idle-replacement tool, and verify the restored save and mobile reconnects. Then update phone clients for the prepared layout/return cues, continue the device/lifecycle checklist, and add parent-facing start/stop/status and sign-in startup. Keep the full goal sheet and required G4 iPad hosting/recovery unchanged.
