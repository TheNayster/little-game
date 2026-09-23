# Little weeps game

This is the separate home for the family's Bluey-inspired game.

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

G1 foundation setup is in progress. See [the current implementation record](docs/implementation/g1-status.md) for what exists and what is still unverified. No playable game or device qualification is complete yet.

**Windows preview available:** double-click `Play-Foundation.cmd` in this folder. It opens the verified 0.0.9 technical fixture with a saved tap counter and local-video controls. Windows restart and version-update checks passed again on 0.0.6 → 0.0.9; the earlier forced-close check passed on 0.0.6. This is not the finished game. Mac/iPad work is deferred while the user is away.

**Android/server preparation:** build commands and a separate server scene are in place. Windows canceled the platform installer launch, so Android/server binaries and native checks remain pending. [Exact status and continuation steps](docs/implementation/platform-build-setup.md).

Use the desktop **Connect Little Weeps** shortcut, or run `Connect-GameTools.ps1` in this folder. [Tool connection instructions](docs/implementation/tool-connections.md) explain the launcher and Mac build connection.

Normal home play: devices automatically join the Windows PC server after parent setup. Bluetooth has been removed from the plan and backlog.

Required: iPad hosting, automatic joining and host switching, independent areas, up to four mixed-device players, and full offline solo play. Multiplayer connectivity while traveling is an optional later want; it does not make core hosting/recovery optional.

The unrelated old Unity project remains at `C:\Users\sephi\Desktop\Meeps game`. Its assets, packages, project settings, Git history, and original tool script remain there. No old game files were imported. The desktop shortcut now uses this game's separate launcher. Future work for this game belongs in this folder.
