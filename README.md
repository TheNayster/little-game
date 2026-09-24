# Little weeps game

This is the separate home for the family's Bluey-inspired game.

**Primary devices: both iPads. Android is secondary.** The older iPad 7 sets the minimum performance baseline. Samsung results do not substitute for iPad tests; four-player mixed-device support remains required.

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

G1 foundation setup is in progress. See [the current implementation record](docs/implementation/g1-status.md) for remaining device checks. The user authorized provisional Windows-only G2 work while those checks remain open.

**Playable garden prototype:** double-click `Play-SoloPrototype.cmd`. It opens verified Windows **0.0.37** with tap/joystick walking, two placeholder pups, bucket watering, sponge cleanup, optional activities, English hints and recoverable local saves. Windows input/update/recovery and an isolated Android emulator update passed. [G2 results and limits](docs/implementation/g2-status.md). Physical-device qualification and multiplayer remain ahead.

**Windows preview available:** double-click `Play-Foundation.cmd` in this folder. It opens the verified 0.0.22 technical fixture with a saved tap counter and local-video controls. Windows restart and version-update checks passed again on 0.0.19 → 0.0.22. Paused bookmark stability also passed 60 reopenings; [fix evidence](docs/implementation/windows-bookmark-2026-09-23.md). This is not the finished game.

**First iPad launch (earlier):** signed 0.0.1 launched on iPad 9 after the user allowed developer trust. It has since been updated in place to build 20, as recorded below.

**Current iPad update:** signed build 20 is installed and launched on iPad 9. The 16 → 20 update retained 1,019 taps, the same profile and the latest 10.3-second bookmark. The user verified the resumed position and play/pause, start-over and skip controls. The subsequent bookmark is 8.9 seconds. The older iPad 7 is currently unavailable. Next: [Windows automatic-refresh setup](docs/implementation/ipad-refresh-2026-09-23.md); the desktop shortcut **Set up Little Weeps iPad refresh** opens the prepared files. [Evidence and next steps](docs/implementation/ipad-update-2026-09-23.md).

**Android/server preparation:** Family-signed Android **0.0.15** is installed on the Samsung. Touch/audio, local video, force-stop recovery and the **14 → 15** in-place update passed, preserving 3 saved taps and a 6.2-second bookmark. The wide-screen title is fixed; local signing recovery was exercised. [Phone results](docs/implementation/android-signing-and-phone-2026-09-23.md). Native 16 KB and independent-backup gates remain open. Windows Server 0.0.12 passes its headless lifecycle test; multiplayer remains ahead.

Use the desktop **Connect Little Weeps** shortcut, or run `Connect-GameTools.ps1` in this folder. [Tool connection instructions](docs/implementation/tool-connections.md) explain the launcher and Mac build connection.

**Saved build profiles complete:** iPad/iOS export 16, Android 17, server 18 and Windows 19 all built from named profiles. Server lifecycle and Windows save/update checks passed. iOS 16 has since passed native signing, installation and touch/media/restart checks; phone 15 remains installed. [Latest profile evidence and next iPad checks](docs/implementation/build-profiles-2026-09-23.md).

Planned normal home play: devices automatically join the Windows PC server after parent setup. That networking is not implemented yet. Bluetooth has been removed from the plan and backlog.

Required: iPad hosting, automatic joining and host switching, independent areas, up to four mixed-device players, and full offline solo play. Multiplayer connectivity while traveling is an optional later want; it does not make core hosting/recovery optional.

The unrelated old Unity project remains at `C:\Users\sephi\Desktop\Meeps game`. Its assets, packages, project settings, Git history, and original tool script remain there. No old game files were imported. The desktop shortcut now uses this game's separate launcher. Future work for this game belongs in this folder.
