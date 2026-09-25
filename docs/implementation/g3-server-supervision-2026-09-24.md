# Bounded server crash recovery — G3-OPS-03

**NET-02. Implemented and qualified in a disposable Windows world.** An actual server process was terminated; the real supervisor command, using its normal backoff clock, started one replacement from the saved world. All four already-running clients rejoined automatically and could use shared items. The observed crash-to-four-rejoin interval was **19.34 seconds**. This is one local test result, not a guaranteed recovery time or uninterrupted simulation during server loss.

The actual family's server remains **83**, untouched and running. The helper is **not enabled on that world**. Prepared runtime **85** includes parent occupied-stop and interrupted-recovery guards. Its reviewed Windows network approval/deployment is still pending while the user is away.

## Policy

- `Tools/Server-Supervisor.py` watches one explicitly selected enrolled world. It never kills a process, resets a save, creates replacement enrollment or silently adopts a different build.
- Parent Start records an atomic, flushed restart preference. Parent Stop turns it off before issuing the guarded stop. A refusal restores the previous preference; an uncertain stop leaves automatic recovery paused for review. Backup restore/rollback also pause it.
- Automatic starts recheck the exact preference revision inside the same operation lock as parent actions. A supervisor working from stale information cannot undo a later parent Stop/Pause.
- Confirmed process exit with a verified checkpoint and configured executable triggers a delayed retry. Three attempts are allowed, with increasing delays. Accounting persists across helper recreation; repeated failure requires parent inspection. Ten healthy minutes reset the failure count. The native save lock and controller lock retain one writer, and a separate supervisor lock permits only one helper per family.
- Missing/corrupt checkpoints, incomplete restores, uncertain process identity/heartbeat and unprepared network permission block automatic startup. An existing but unresponsive process needs diagnosis; it is not forcibly killed.
- Closing or pausing the helper leaves current players alone. `--pause` disables future automatic starts without disconnecting anyone.

## Evidence

| Qualification | Result |
| --- | --- |
| Actual process loss | Real supervisor CLI/backoff and four native Windows clients. One replacement authority, same world/player/item/receipt state, original client processes rejoin, later interaction works. [Six qualification groups](evidence/server-supervision-2026-09-24/native-85.json). |
| Parent intent | Guarded stop remains stopped; stale automatic intent rejected; second supervisor process cannot acquire the helper lock. |
| Restart limits | Three failed starts exhaust persistent accounting. Synthetic timeout check confirms launcher timeouts use the same bounded policy. [Timeout evidence](evidence/server-supervision-2026-09-24/timeout-85.json). |
| Save/recovery boundaries | Corrupt/missing saves, pending recovery and uncertain process state block restarts. Restore leaves supervision paused. |
| Parent-control regression | All ten real HTTP/native parent checks pass on 85, including four-player occupied-stop refusal, exact checkpoint restart and stale-instance protection. [Regression evidence](evidence/server-supervision-2026-09-24/parent-85.json). |

Failure-budget and ten-minute policy boundaries use an injected clock. The primary native crash/rejoin measurement uses real elapsed time and the actual supervisor process. No physical-device recovery, real ten-minute soak, PC reboot/logoff/service operation, hung-process recovery or VPS behavior is established by this milestone.

## Activation and deployment

This helper is a local process, not an installed Windows service or boot task. It needs the signed-in Windows identity that can unlock the protected enrollment. Do not count it as “recovers after reboot” or “works after logout.” After controlled 85 deployment and a deliberate parent Start, run the following through the project’s hidden-process launcher context:

```powershell
uv run python Tools/Server-Supervisor.py --family <enrolled-family-id> --build 85
```

An already-running legacy world is not adopted or upgraded. For a pause:

```powershell
uv run python Tools/Server-Supervisor.py --family <enrolled-family-id> --build 85 --pause
```

The per-family `supervisor-state.json` reports checked time, status, message and attempts. Intent/accounting remain private under `LocalData`, are excluded from backup reconstruction, and are not server credentials. The parent browser's Start/Stop endpoints preserve this preference, but the current browser does not launch or supervise the helper itself. A parent-facing activation/status integration and boot/service deployment remain explicit follow-up work.

**Next bounded task: G3-OPS-04, a measured longer four-client Windows run.** Exercise movement, area changes, item interactions and save/rejoin under a real clock, record process memory/observations, and distinguish desktop evidence from pending sustained A10/iPad and mixed-device measurements. Independent backup destination, portable enrollment, VPS readiness and mandatory G4 iPad hosting remain in sequence.
