# Project folder guide

Current source root: `C:\Users\sephi\Desktop\Little weeps game`. Unity project: `Unity/FamilyPlayset`. This location contains the whole game. World folders divide responsibilities inside the existing assemblies; they are not separate complete projects.

## World ownership

Each of Home, Park, Creek, Beach, Daycare, Zoo and Dinosaur has `Code/Core/Worlds/<World>` for rules, `Code/Client/Worlds/<World>` for presentation and `Resources/Worlds/<World>` for shipped assets. Paths are relative to `Unity/FamilyPlayset/Assets/FamilyPlayset`. Editable inputs live in the corresponding `SourceArt/<World>` and `SourceAudio/<World>` folders where present. Shared characters, menu artwork, narration and world music live under `Shared`. SourceArt's `Scenery` folder retains original multi-world authoring provenance.

Daycare includes StoryAdventure, SandcastleClub, TreasureHunt, AnimalClinic, Calypso/counting and HideAndTag. These activity names replace ambiguous Sandpit/Vet/Kingdom screen filenames; existing saved IDs and commands remain compatible. Home's garden pond remains under Home; Creek's fishing has its own rules/view and intentionally reuses the pond water/bite assets. Shared use is explicit rather than duplicating the same sound.

`GameWorld` replaces the production type named SoloWorld. `GameScreen` replaces SoloScreen. Their world partials still share one assembly. Beach flock transitions now belong to `Core/Worlds/Beach/BeachFlockRules.cs`; flock visuals belong to `Client/Worlds/Beach/SeagullFlockView.cs`. Shared dispatch, player state, networking and navigation still require coordination between world tasks. Do not claim that folders alone eliminate those overlaps.

The maintained [migration/ownership manifest](project-layout.json) maps old source, tool, asset and authoring paths. `WorldResources` translates persisted older asset IDs; existing GUIDs and serialized save/message field names remain intact. `SoloSnapshot`, enum values, `SoloPrototype` save paths, namespace/assembly identities and wire identifiers are compatibility names, not forgotten cleanup.

## Tools and generated outputs

Tools are grouped by actual purpose. Shared imported Python modules remain at `Tools` so the live parent helper/watchdog keeps its established paths. `Tools/parent-ui`, `Tools/UnityEditor`, signing/toolchain pins and integration connectors also remain in place. Four small root PowerShell entry points preserve named parameter binding for established update callers. This is the explicit compatibility exception, not permission to create new loose scripts. `project_paths.py` resolves roots and migration aliases.

Existing `Builds/NetworkProbe/G3-0.0.<number>`, AndroidSigned and iOSFamilyLAN layouts retain manifests and consumers. Do not rename a delivered release to make it appear current. Current preview selection stays in the existing LocalData records. A new build must use a fresh number and verified current source; directory organization does not prepare or install a phone update.

## Local data and lifecycle

| Location | Purpose and retirement |
| --- | --- |
| `LocalData/Worktrees/Beach` | Unfinished Beach work; reconcile source/assets and reviews before retirement |
| `LocalData/Worktrees/HomeScience` | Preserved former-root work on its original branch, including accidental root Unity project; reconcile unique work before retirement |
| `LocalData/Worktrees/PreviousMain` | Detached historical replay checkout; retain old task inputs and review evidence |
| Other existing LocalData registered checkouts | Preserve unfinished owner work; consult the phase record before changing paths |
| `LocalData/PCServer` and current server/recovery records | Live operational consumers; stable locations until a requested coordinated rollout |
| Device/signing/FamilyLAN/SharedGarden/Recovery records | Private operational data; never delete as cosmetic cleanup |
| `LocalData/Verification`, `LocalData/Logs` | Focused checks and diagnostics; protect referenced review evidence |
| AudioStudio, MusicStudio, AVD/toolchain directories | Reusable large dependencies; keep for fast builds/content generation |

New temporary development copies belong under `LocalData/Worktrees/<Purpose>` and must identify purpose, owner/work item, consumers and retirement condition. Keep complete development checkouts distinct from world source folders. Preserve all audit/review/research exports, images, evidence, current caches, selected/signed releases, offline repair installers, backups and saves. No arbitrary Desktop source copies.

## Verified organization and limits

Seventeen redundant iPad transfer archives were removed after matching transfer receipts and all retained export hashes: 5,132,959,651 logical bytes. Four retired caches plus the ADB ZIP/empty temporary folders remain because automatic approval review rejected removal with `blocked by policy`. No alternate deletion method was used.

All seven assemblies compile using Unity's actual project defines/references, and the focused Beach four-player rules/checkpoint test passes. 970 moved runtime asset/meta files preserve their bytes. Full Unity candidate424 and native visual verification are blocked by Windows Application Control on Bee.Tools.dll; this is not an installed release. The phase record lists remaining conditional cleanup and verification. Mac folder inventory remains outside the inspected Windows scope.

Final path/identity check: 162 original script metadata hashes preserved; 1,058 Unity asset GUIDs unique; all219 tool destinations and138 project source includes resolve. All13 .NET tool projects compile. Plan consistency passes for3,880 local links. Historical Windows423 is retained at `LocalData/Worktrees/PreviousMain/Builds/NetworkProbe/G3-0.0.423`; read-only verification/preview lookup resolves the exact requested release, never a different build number. Install tools retain their explicit source/version/signature verification and do not use this archive lookup.
