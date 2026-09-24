# Little weeps game

This is the separate home for the family's Bluey-inspired game.

**Private GitHub repository:** [TheNayster/little-weeps-game](https://github.com/TheNayster/little-weeps-game). This project uses that repository as `origin`; `main` is the shared baseline and `codex/…` branches hold development work. Source, research and versioned test evidence belong here. Git LFS stores the tracked narration and test video; generated builds, local worlds, credentials and signing keys remain excluded.

**Primary devices: both iPads. Android is secondary.** The older iPad 7 sets the minimum performance baseline. Samsung results do not substitute for iPad tests; four-player mixed-device support remains required.

Double-click `Open-BuildGuide.cmd` to reopen the local build-guide page after a reboot. It starts a loopback-only server for this game's docs. The existing research links on that page use the same server.

**Two main documents:** The Family Playset research is the feature goal sheet. The ground-up build guide is the implementation sequence, project structure, testing method and current work record.

- [The Family Playset — illustrated guide](docs/bluey-game-research-2026-09-23.html)
- [Ground-up build guide — how we will build it](docs/family-playset-build-guide-2026-09-23.html)
- [Editable build guide](docs/family-playset-build-guide-2026-09-23.md)
- [Templates, free packages and optional purchases](docs/family-playset-package-research-2026-09-23.html)
- [Full feasibility audit and proof plan](docs/family-playset-feasibility-audit-2026-09-23.html)
- [Deeper technical research and source findings](docs/family-playset-technical-research-2026-09-23.html)
- [Main research and plan](docs/bluey-game-research-2026-09-23.md)
- [Toca Boca and Piknik interaction research](docs/toca-piknik-interaction-research-2026-09-23.html)
- [Device and build research](docs/ipad-game-research-2026-09-23.md)

**Current position, audited September 24:** G1/G2/G3 have partial evidence; the user authorized provisional Windows work while earlier gates remained open. The [implementation audit and remaining-work ledger](docs/family-playset-build-guide-2026-09-23.html#19-implementation-audit-and-remaining-work) is the current status source for all 35 feature IDs. It separates passed proofs from full feature acceptance. Next: parent-approved LAN pairing/discovery, then physical-device multiplayer; required iPad hosting/recovery stays in scope.

**Playable solo garden:** double-click `Play-SoloPrototype.cmd` for Windows **0.0.67**. Tablet-shaped input and 64 → 67 save/update/recovery passed. Both physical iPads remain on solo **56**. [Current evidence](docs/implementation/g3-rejoin-recovery-2026-09-24.html).

**Shared Windows preview:** double-click `Play-SharedGarden.cmd`. Build **68** now opens four windows sharing Garden/Creek with smooth movement and a fix for the reproduced receive/rejoin stall. Four-client discard stress and eight crash/rejoin cycles passed without restarting their server. Normal and delayed-motion checks passed with the remote history buffer at 180 ms; own movement remains immediately anticipated. Closing every preview window stops its server; reopening preserves the saved world. [Research and qualification](docs/implementation/g3-rejoin-recovery-2026-09-24.html). The user confirmed the four-window preview is “working great!” Networking remains loopback-only; LAN pairing, automatic joining, iPad hosting and physical mixed-device multiplayer remain required work.

**PC networking groundwork:** isolated native server/client build **48** originally passed four simultaneous Windows clients, exclusive bucket ownership, late joins, player departure and checkpoint recovery after a forced server stop. Build **54** repeats those tests and adds the playable two-window interface above. [Original G3 preparation results](docs/implementation/g3-network-probe-2026-09-24.md).

**Windows preview available:** double-click `Play-Foundation.cmd` in this folder. It opens the verified 0.0.22 technical fixture with a saved tap counter and local-video controls. Windows restart and version-update checks passed again on 0.0.19 → 0.0.22. Paused bookmark stability also passed 60 reopenings; [fix evidence](docs/implementation/windows-bookmark-2026-09-23.md). This is not the finished game.

**First iPad launch (earlier):** signed 0.0.1 launched on iPad 9 after the user allowed developer trust. It has since been updated in place to build 20, as recorded below.

**Current iPads:** native garden **56** runs on both iPads. User play checks and exact saved-world restart results are recorded for [iPad 9](docs/implementation/ipad-garden-2026-09-24.md) and [iPad 7](docs/implementation/ipad7-garden-2026-09-24.md). Measured performance and remaining physical update/renewal gates are still open. [Windows automatic-refresh setup](docs/implementation/ipad-refresh-2026-09-23.md) retains the unresolved wireless-refresh work.

**Android/server preparation:** Family-signed Android **0.0.15** is installed on the Samsung. Touch/audio, local video, force-stop recovery and the **14 → 15** in-place update passed, preserving 3 saved taps and a 6.2-second bookmark. The wide-screen title is fixed; local signing recovery was exercised. [Phone results](docs/implementation/android-signing-and-phone-2026-09-23.md). Native 16 KB and independent-backup gates remain open. The earlier Windows Server 0.0.12 passed its headless lifecycle test; shared Windows 68 is the current networking proof above. Mobile multiplayer remains ahead.

Use the desktop **Connect Little Weeps** shortcut, or run `Connect-GameTools.ps1` in this folder. [Tool connection instructions](docs/implementation/tool-connections.md) explain the launcher and Mac build connection.

**Saved build profiles complete:** iPad/iOS export 16, Android 17, server 18 and Windows 19 all built from named profiles. Server lifecycle and Windows save/update checks passed. iOS 16 has since passed native signing, installation and touch/media/restart checks; phone 15 remains installed. [Latest profile evidence and next iPad checks](docs/implementation/build-profiles-2026-09-23.md).

Planned normal home play: devices automatically join the Windows PC server after parent setup. That networking is not implemented yet. Bluetooth has been removed from the plan and backlog.

Required: iPad hosting, automatic joining and host switching, independent areas, up to four mixed-device players, and full offline solo play. Multiplayer connectivity while traveling is an optional later want; it does not make core hosting/recovery optional.

The unrelated old Unity project remains at `C:\Users\sephi\Desktop\Meeps game`. Its assets, packages, project settings, Git history, and original tool script remain there. No old game files were imported. The desktop shortcut now uses this game's separate launcher. Future work for this game belongs in this folder.
