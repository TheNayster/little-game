# Little Weeps

A private Unity game for the family. Up to four players share Home, Park, Creek, Beach, Daycare, Zoo and Dinosaur worlds. The PC/VPS owns the shared world; offline solo stays private and reconnecting loads the server world.

The current source is here: `Unity/FamilyPlayset`. Open that project with the version in `ProjectSettings/ProjectVersion.txt` (currently 6000.3.24f1). `LocalData/Worktrees/Beach` and `HomeScience` preserve unfinished work; `PreviousMain` preserves historical replay inputs.

Start with [current decisions](docs/current-decisions.md), [the folder guide](docs/project-folder-guide.md), [the build guide](docs/family-playset-build-guide-2026-09-23.md), and [the organization phase record](docs/project-organization-phase-plan-2026-10-02.md). Read [server update policy](docs/server-update-policy.md) before any rollout. The private source repository is [little-game](https://github.com/TheNayster/little-game).

| Location | Contents |
| --- | --- |
| `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/<World>` | World rules |
| `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/<World>` | World screens and interaction |
| `Code/Core/Shared`, `Code/Client/Shared` | Shared state, navigation, characters and services |
| `Code/Networking`, `Code/Adapters`, `Code/Server` | Family connection, storage/platform adapters and authority |
| `Code/Legacy/Runtime` | Retained foundation prototype |
| `Resources/Worlds/<World>`, `Resources/Shared` | Runtime artwork, scenery and audio (beneath the same Unity asset root) |
| `SourceArt`, `SourceAudio` | Editable inputs and provenance |
| `Tools/Build`, `Tools/Devices`, `Tools/Content`, `Tools/Documentation`, `Tools/Launch`, `Tools/Verification` | Reusable tools by purpose |
| `Builds` | Ignored generated exports and selected release artifacts |
| `LocalData` | Ignored devices, saves, live server, recovery, evidence and development copies |
| `docs`, Desktop review exports | Protected plans, research and reviews |

Build a Windows validation release with `Tools/Build/Build-FamilyGame.ps1 -BuildNumber <fresh-number>`. Established Android/iPad build/install entry points in `Tools` forward to the grouped implementations. Tools derive the root from their own location; no phone installation or server replacement occurs simply because source was reorganized.

Windows 423 remains the previous successful release. Organization changes compile against the Unity assembly references and pass the focused Beach rules check; candidate 424's full Unity build is blocked by Windows Application Control on Unity's `Bee.Tools.dll`. It is not a delivered app. Preserve existing signed artifacts, enrollment, save paths and current build caches.
