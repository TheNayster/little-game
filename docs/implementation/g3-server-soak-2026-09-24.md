# Sustained four-client Windows run — G3-OPS-04

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**NET-02. Passed for the scoped desktop run.** Server/client 85 ran four enrolled native Windows clients for **600.03 seconds**, with **75 action/sample cycles** on a real clock. No family world or physical device was changed. [Results](evidence/server-soak-2026-09-24/soak-result.json), [sample series](evidence/server-soak-2026-09-24/soak-samples.json).

## What was exercised

Four players repeatedly walked across the floor while two independently changed Garden/Creek areas. One client closed and rejoined using its existing enrollment; the other three remained connected to the same authority. Every sampled transport observation remained listening with receive error zero. The flower rearmed and the unused bucket returned on their ordinary timers. They were observed at 64 and 184 seconds after setup; setup itself had already started the timers, and observations were about eight seconds apart. This is not a claim that configured 65/185-second deadlines changed.

Remote motion was sampled before and after the run. Both samples showed motion on **100%** of the inspected moving render frames, versus **32.9%** for received raw position changes. Median sampled frame intervals were **16.63 / 16.67 ms**; p95 lag behind the received position was **0.213 / 0.212 seconds**. These measure the existing interpolation on this PC, not internet RTT, iPad frame time or a whole-session frame-time distribution.

After all clients left, the complete checkpoint survived an empty clean restart exactly, including player/item state, receipts and idle metadata. The test logs contained none of the selected null/index/out-of-memory/unhandled-exception signatures. This is a bounded log check, not an exhaustive proof of no defects.

## Memory observations

Private committed memory in MiB, sampled per process. Growth compares the first sample after minute one with the last sample. Player 4 deliberately restarted during the run, so that row spans two processes.

| Process | Peak private MiB | Growth after minute one MiB |
| --- | ---: | ---: |
| server | 186.87 | +0.51 |
| player-1 | 709.57 | -0.20 |
| player-2 | 713.48 | -0.19 |
| player-3 | 708.34 | +0.04 |
| player-4 | 709.94 | +1.47 |

The desktop smoke limits were below 1 GiB private memory per process and below 256 MiB growth after the first minute. Those are diagnostic fail thresholds, **not mobile budgets**. Actual observations were much narrower. The Windows clients' roughly 700 MiB figures cannot be converted into an iPad allocation claim; the A10 still needs native profiling with representative content. A ten-minute run is not a leak-free proof or a two-hour endurance qualification.

## Reproduce

```powershell
uv run --with cryptography --with psutil python Tools/Test-ServerSoak.py 85 --seconds 600
```

The harness always creates a separate protected test family and saves its evidence under ignored LocalData. It verifies the built artifacts through the normal launcher and cleans up only its own processes. It never selects the actual family for the departure/restart tests.

## Next bounded task

**G3-OPS-05: parent-page backup and crash-recovery operations.** The tools from OPS-02/03 are qualified, but activating them still requires developer commands. Add a local backup action and accurate recovery status/enable/pause controls, respecting saved parent intent and the selected deployed build. Qualify through real HTTP/browser interaction in a separate world. The real 83 server may be backed up/observed, but activation of guarded 85 still requires Windows approval and an empty-session update.

This operational step does not close G3: sustained physical-device/route/outage tests, automatic prolonged-outage local continuation, parent enrollment, independent/portable recovery and boot/service deployment remain. The required G4 iPad authority/recovery and G5 reconciliation/content sequence are unchanged. The main guide owns the current queue.
<!-- historical-record-end -->
