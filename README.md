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

**Current evidence:** Samsung now runs **119**, while both iPads and iPhone run **101**, with layered Bluey/Bingo and restored solo Creek navigation. The user accepts the Android prototype, noting stiff animation. Native travel/update/recovery, retained in-place device updates and Apple runtime/enrollment checks pass. Earlier, the original family server **91** was started on request and all four players joined; current server 110 delivery is recorded below. [Implementation and evidence](docs/implementation/character-phone-switch-2026-09-25.html).

**Scenic/device baseline:** [six long scenic worlds in Windows 110](docs/implementation/scenic-worlds-2026-09-25.html), with twelve illustrated sections, continuous Home/Backyard, local panning/follow cameras and bounded texture loading. The original six entries passed 81 core checks and seven native test groups; revision 118 now combines house/backyard into one of five destinations. Android 110 previously passed installed scenery/save checks; [Android 115 now delivers the first home features](docs/implementation/android-home-update-2026-09-26.html) with retained prior item data and visible home verified. [Server/helper 110 is now deployed](docs/implementation/family-scenic-rollout-2026-09-25.html) with the original shared world retained and recovery qualified. The server remains content 4, while the current phone/content 5 plays solo; coordinated shared delivery is pending. Apple 101 delivery stays deferred. **Next content work:** correct integrated home scenery/objects below, then kitchen and sustained home development. No new other-world activities were added.

**Latest implementation:** [HOME-01 / Windows 114](docs/implementation/home-interactions-2026-09-25.html) adds two sofa seats, two trampoline spots, radio music with automatic dancing, independent music mute and four-slot shed storage. The final release passes 92 core checks and five native interaction groups, including multiplayer pose release and offline reopen. [The report](docs/implementation/home-interactions-2026-09-25.html#house-layout-and-next-feature-passes) maps every remaining home room/activity; kitchen storage and food serving follow the integrated scene-art correction. The current phone has these home features; server 110 and Apple 101 remain unchanged.

**Walking and chooser feedback:** the user rejected the 116 walk. [Second investigation](docs/implementation/walk-animation-research-2026-09-26.html) and [revision 118](docs/implementation/walk-animation-2026-09-26.html) correct cadence, facing, arm timing and crossing legs, add a complete-cycle preview, combine Home/Backyard into one menu destination and keep the active character visible above the tray. Native walk, phone/tablet chooser and home regression checks pass. Android 118 is installed with visible home and retained saves verified; user visual acceptance remains open.

**Latest research:** [Background, furniture and object layers](docs/implementation/home-scene-layer-research-2026-09-26.html) confirms the duplicate sofa/trampoline/shed problem in the current art pipeline. The plan now starts ART-HOME-02 with clean room bases and one coordinated visual placement per usable object, including rear/front layers and proper seat/storage overlap. [The art inventory](SourceArt/Home/scene-layer-plan.json) maps the four home/backyard panels. Research and planning are complete; game artwork/runtime correction is pending.

**Open release checks:** unattended Apple renewal, independent backup/restore, sustained updated-device performance and Android native 16 KB qualification. VPS deployment is planned, not installed. Historical builds, probes and G4 experiments remain documented for traceability; their old task queues do not override the current plan. Unfinished host-99 work is archived locally and must not be deployed.

Use the desktop **Connect Little Weeps** shortcut, or run `Connect-GameTools.ps1` in this folder. [Tool connection instructions](docs/implementation/tool-connections.md) explain the launcher and Mac build connection.

The unrelated old Unity project remains at `C:\Users\sephi\Desktop\Meeps game`. Its assets, packages, project settings, Git history, and original tool script remain there. No old game files were imported. The desktop shortcut now uses this game's separate launcher. Future work for this game belongs in this folder.

**Latest character correction:** [Side-view walking 119](docs/implementation/profile-walk-2026-09-26.html) gives Bluey/Bingo profile bodies facing left/right and front poses at rest. Native animation/home/chooser checks pass. Android 119 is installed with saved records retained; visible phone check awaits unlock. The layered home-scene correction remains next.

**Latest visual direction:** the user selected the [earlier Bluey movement sheet](SourceArt/Characters/SelectedReference/README.md). The exact PNG is recovered; prepare its native animation proof and integration before resuming home scene cleanup. Installed build 119 is unchanged by this source recovery.
