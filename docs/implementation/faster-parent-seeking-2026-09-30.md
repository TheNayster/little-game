# Faster parent seeking — September 30

**Done for HIDE-01 / HIDE-03:** Bandit and Chilli now head toward the nearest remaining hider and check unvisited hiding spots along the way. They skip unnecessary trips into empty areas behind them or beyond that hider. Finding still requires walking to and inspecting the cover.

Parent walking increases from 420 to 900 units/second. Inspections decrease from 1.35 to 0.55 seconds, looking pauses from 0.9–1.2 to 0.25–0.35 seconds, and found reactions from 1.25 to 0.6 seconds. The walking animation and cover-opening timing follow the quicker pace.

The common **15-second hiding window** remains unchanged. Up to four share one round; Bandit and Chilli alternate. Coming out or leaving withdraws only that player, without interrupting the remaining hiders or permitting mid-search re-hiding.

Windows release **320** client/server compiled with zero errors and warnings. Focused rules checked physical movement/inspection, both travel directions, nearest-hider routing and independent departure: far-right search **16.35 seconds**, far-left **7.85 seconds**, and a distributed family round **18.55 seconds**, measured after the hiding window. The native four-client round found all remaining hiders in **18.7 seconds**, after one player withdrew, while inspecting intermediate covers and avoiding the opposite-side detour. Desktop (960×640) and iPad-size (1024×768) captures show safe controls and the faster walking pose. These are local native-player checks; physical devices were not updated.

[Native result](evidence/faster-parent-seeking-2026-09-30/result.json) · [Desktop search](evidence/faster-parent-seeking-2026-09-30/faster-search-desktop.png) · [Tablet search](evidence/faster-parent-seeking-2026-09-30/faster-search-ipad.png)

Shared rules require **content 49** for a future coordinated rollout; save schema stays **39**, with no new fields or migration. The live family server remains unchanged. Preserve the main integration hold and concurrent work. Configurable clues, parent speech and upstairs searching remain planned. Next: family pacing playtest or the next requested feature.
