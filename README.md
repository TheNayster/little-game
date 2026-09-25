# Little weeps game

This is the separate home for the family's Bluey-inspired game.

**Private GitHub repository:** [TheNayster/little-weeps-game](https://github.com/TheNayster/little-weeps-game). This project uses that repository as `origin`; `main` is the shared baseline and `codex/…` branches hold development work. Source, research and versioned test evidence belong here. Git LFS stores the tracked narration and test video; generated builds, local worlds, credentials and signing keys remain excluded.

**Git working routine:** complete focused changes, run the relevant checks, update the plan/evidence, commit with clear explanations, and push to this private repository. Verify the uploaded commit and keep `main` current with completed work. The persistent workflow is recorded in [AGENTS.md](AGENTS.md).

**Personal TV videos:** use [Media/TV](Media/TV/README.md). Its contents are excluded from GitHub except the folder instructions, including nested folders, thumbnails and subtitles. The game's TV importer remains planned; this creates its local source folder only.

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

**Current architecture:** one PC server now, the owned VPS after a controlled migration. Up to four iPad/iPhone/Android clients join automatically after enrollment. Devices never host. Offline solo stays private; the server's world wins on reconnect. G4/AUTO-02 device hosting is retired by explicit user decision. [Read the current decisions](docs/current-decisions.md) before using dated research or checklists.

**Current evidence:** last deployed server/helper 91; last verified iPad 7 build 95, iPad 9/iPhone 79, Samsung 83. Four physical devices have joined and scoped shared-play checks passed. Smoother offline walking and offline reopening passed on iPad 7 build 95. Client candidate 98 repairs the transition to an older saved state and passed Windows/emulator checks and native Apple compilation; signing and physical acceptance remain pending. This audit did not contact the live server/devices.

**Next work:** the [current return checklist](docs/implementation/return-checklist-ipad-lan-2026-09-24.html) covers the focused 98 installation and device checks. While those wait, ART-PREP-02 can integrate the [character workshop](docs/implementation/character-workshop/index.html) into an isolated Unity view. G5 then adds persistent rooms, creations and reusable item policies. There is no device-hosting prerequisite for art work. The [phase and feature ledger](docs/family-playset-build-guide-2026-09-23.html#19-implementation-audit-and-remaining-work) separates scoped proofs from full acceptance.

**Open release checks:** unattended Apple renewal, independent backup/restore, sustained updated-device performance and Android native 16 KB qualification. VPS deployment is planned, not installed. Historical builds, probes and G4 experiments remain documented for traceability; their old task queues do not override the current plan. Unfinished host-99 work is archived locally and must not be deployed.

Use the desktop **Connect Little Weeps** shortcut, or run `Connect-GameTools.ps1` in this folder. [Tool connection instructions](docs/implementation/tool-connections.md) explain the launcher and Mac build connection.

The unrelated old Unity project remains at `C:\Users\sephi\Desktop\Meeps game`. Its assets, packages, project settings, Git history, and original tool script remain there. No old game files were imported. The desktop shortcut now uses this game's separate launcher. Future work for this game belongs in this folder.
