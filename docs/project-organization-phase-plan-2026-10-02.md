# Little Weeps project organization phase plan

October 2, 2026. Organize the existing game so its main source, worlds, shared systems, editable assets, tools, builds and local data have clear names and predictable locations. Revised after the [unused-files audit](unused-files-audit-2026-10-02.md) and [studio/engine structure research](project-structure-research-2026-10-02.md). The research document compares every phase with public Unity, Epic, Riot and Rare evidence and distinguishes their practices from our recommendations. The user selected the Beach checkout relocation as the first implementation phase. None of the implementation phases has been performed.

## Scope and order

Complete one phase at a time and record its result here. Preserve unfinished work, editable sources, Unity asset metadata, generated builds, saves, device enrollment and signing identity. Folder maintenance does not install apps or replace the live server. Research-only instructions remain in effect for this planning task.

The audit covered the Windows project, associated Desktop folders and registered development checkouts. Mac folder organization remains uninspected; include it after resolving the requested scope. The Desktop Daycare review folder is an explicitly requested export, not an unexplained development checkout.

Operational paths and cleanup candidates below are relative to `C:\Users\sephi\Desktop\Little weeps game`, unless an absolute path is given. Maintained source paths are relative to the current source checkout, presently `LocalData\DinosaurWorld`. These are different locations; resolve the actual absolute target before any move or deletion. Later phases use Phase 2's documented source location, not a guessed directory depth.

Selected structure: keep the existing rules/presentation/server assembly boundaries, group worlds inside those layers, then extract shared-class responsibilities gradually. Keep authored assets and review evidence; retire bounded outputs by purpose and active consumers. Preserve current caches for fast updates. New folders require an actual purpose and contents, not an empty speculative hierarchy.

| Phase | Result | Status |
| --- | --- | --- |
| 1 | Move the Beach checkout inside the game folder and repair dependent paths | Next implementation phase |
| 2 | Make the obvious game folder the clear home of current source | Proposed |
| 3 | Separate local data and remove specifically verified unnecessary outputs | Proposed; cleanup queue recorded |
| 4 | Group worlds inside existing layers, then extract shared-class responsibilities | Proposed; Beach extraction pilot |
| 5 | Replace misleading production code names safely | Proposed |
| 6 | Gather editable assets and apply consistent world grouping | Proposed |
| 7 | Group reusable tools and clarify build locations | Proposed |
| 8 | Correct entry documentation and record folder rules for future chats | Proposed |

## Phase 1 Move the Beach checkout

Move this complete development checkout:

`C:\Users\sephi\Desktop\Little weeps beach seagulls`

to:

`C:\Users\sephi\Desktop\Little weeps game\LocalData\Worktrees\Beach`

`Beach` describes the whole checkout's current task area, including seagulls, waves and visitor rides. This is a copy of the entire game for development; it is not the eventual location of Beach-only gameplay files.

1. Recheck the source and destination, current branch, local changes, ignored builds and editable artwork. Confirm no active editor, build or task is using the old directory before moving it. Preserve all contents, including ignored files; do not reset, clean or recreate it from committed source.
2. Search current launchers, build tools, local task scripts, connection records and shortcuts for the old path in Windows and slash-separated forms. Identify generated Unity references separately from maintained code and configuration. Capture the current change inventory and relevant source/asset hashes before the move.
3. Use `git worktree move` so Git's checkout registration and pointer files move together. Do not use a plain Explorer move followed by guessed Git repairs. Do not merge, switch or delete the Beach branch as part of this relocation.
4. Update active references that depend on the old location. Prefer project-relative paths where possible. Update a shortcut only if it actually targets this checkout. Preserve historical reports' original paths as dated evidence and add a relocation note where useful. Recreate generated cache references only if a focused check shows they need it.
5. Verify the old Desktop directory is gone, the destination is registered correctly, the branch and local change inventory are preserved, and editable sources and ignored builds remain present. Check affected launcher/build path resolution without starting a build or installing anything. Compile only if maintained code actually changes.

Record this checkout's purpose, responsible work item/chat, active consumers and retirement condition. A complete checkout contains other worlds because it is a development copy of the game; that is separate from the feature grouping proposed in Phase 4. Keep unfinished Beach work separate from the relocation task's delivered documentation/configuration changes.

Completion requires a recorded relocation result, repaired active references and successful focused checks. List any unresolved active reference explicitly. The previous directory remains the rollback destination if the move needs reversing; never discard content to undo it.

## Phase 2 Establish the main source location

The visible game root is currently an older development checkout with substantial local edits. The checkout on `main` is inside `LocalData\DinosaurWorld` and contains the whole current game. Its name incorrectly implies a single world.

Inventory and preserve the root's unfinished work first, including ignored source art and local build inputs. Reconcile ownership and source differences before selecting the safe checkout transition. Put the current shared baseline in the obvious game location and retain unfinished work in clearly identified temporary checkouts. Update editor targets, launchers and maintained paths, including local scripts that explicitly reference `LocalData/DinosaurWorld`. Record where each nested registered checkout remains; do not move/delete its containing root blindly. Keep the parent's chosen `Little weeps game` root name and handle spaces correctly in scripts.

Completion: one documented main source location; the normal launcher opens its `Unity\FamilyPlayset` project; unfinished edits and editable assets remain recoverable; no build or task silently uses the displaced checkout. Do not force a clean root with reset, deletion or a directory swap that breaks nested checkouts.

## Phase 3 Separate local data and retire unnecessary outputs

Use the [unused files audit](unused-files-audit-2026-10-02.md) before deciding what to retain or relocate. The user excludes audit/review material from cleanup. Start only with explicitly verified duplicate transfer files and bounded temporary outputs; preserve unique source, saves, recovery inputs and review dependencies. The audit is research, not a deletion receipt.

Root `LocalData` had 275 loose files during the audit, alongside source checkouts, device records, server installations, backups and experiments. Organize these after Phase 2 resolves the main source path.

Proposed categories are `Worktrees`, `Devices`, `Server`, `Backups`, `PreparedUpdates`, `Verification` and `Temporary`. Create only categories that have actual contents. Keep these local and excluded from source commits. Classify each item before moving it; filenames and age alone do not prove something is disposable. Server paths such as `PCServer/current` and device/signing records have active consumers and need coordinated reference changes. Do not relocate live operational data during occupied family play.

### 3A Verified redundant intermediates first

The audit hashed these 17 exact root `LocalData` transfer archives against successful receipts and verified every file in their retained `Builds/iOSFamilyLAN/G3-0.0.<version>` exports against manifests:

```text
LocalData/ios-g3-0.0.71.tar.gz
LocalData/ios-g3-0.0.72.tar.gz
LocalData/ios-g3-0.0.73.tar.gz
LocalData/ios-g3-0.0.74.tar.gz
LocalData/ios-g3-0.0.79.tar.gz
LocalData/ios-g3-0.0.82.tar.gz
LocalData/ios-g3-0.0.91.tar.gz
LocalData/ios-g3-0.0.92.tar.gz
LocalData/ios-g3-0.0.93.tar.gz
LocalData/ios-g3-0.0.94.tar.gz
LocalData/ios-g3-0.0.95.tar.gz
LocalData/ios-g3-0.0.98.tar.gz
LocalData/ios-g3-0.0.101.tar.gz
LocalData/ios-g3-0.0.110.tar.gz
LocalData/ios-g3-0.0.128.tar.gz
LocalData/ios-g3-0.0.171.tar.gz
LocalData/ios-g3-0.0.227.tar.gz
```

They total **5,132,959,651 logical bytes (5.13 GB)**. Before removal, recheck that receipts/replacements remain valid and no current transfer needs an archive. Preserve the retained exports and transfer receipts. Do not substitute a wildcard or extend this allowance to the six unverified older archives or any recent active-checkout archive.

Also check `LocalData/platform-tools-official-r36.zip` (7,138,784 bytes): current device tooling uses the installed Android SDK ADB, not this downloaded ZIP. Keep the installed SDK. Remove root `tmp/pdfs` and its `tmp` parent only if still empty and unclaimed. Protected review content appearing inside either directory cancels its empty-folder disposition.

### 3B Closed-task metadata and small generated outputs

Confirm task closure and no current caller before retiring these five staging indexes. Their blobs are preserved in committed history, but that alone does not preserve a task's selected combination of files:

```text
LocalData/FixedParentStartTask/fixed-start.index
LocalData/FasterSeekTask/seek.index
LocalData/SharedCoverTask/shared-cover.index
LocalData/HomeOnlyHideTask/home-only.index
LocalData/BathroomTask/bathroom.index
```

Combined index size: 3,182,689 bytes. Keep task ledgers/review records. Keep `character-roster.index`, `full-roster.index` and `outfit-task.index` while their staging/replay scripts consume them; the research-related pond index is excluded from cleanup.

Generated outputs under `LocalData/OutfitCommitCheck/bin`, `LocalData/OutfitCommitCheck/obj`, `LocalData/CharacterRosterRules/bin` and `LocalData/CharacterRosterRules/obj` total about 2.39 MB and can be rebuilt from retained inputs. Recheck idleness and preserve harness projects, source, replay scripts and required indexes. Do not remove their entire parent task folders.

### 3C Retire large outputs only after resolving their use

| Exact path | Audit size | Retirement condition |
| --- | --- | --- |
| `LocalData/PondBuild/snapshot/Unity/FamilyPlayset/Library` | 3.76 GB | Snapshot task is closed/idle; accept slower replay; preserve source/assets/evidence. |
| `LocalData/RosterBuild/Unity/FamilyPlayset/Library` | 3.62 GB | Cache only; surrounding source is not proven redundant and must remain. |
| `LocalData/WheelsBuild/snapshot/Unity/FamilyPlayset/Library` | 3.68 GB | Snapshot task is closed/idle; preserve editable assets/evidence. |
| `LocalData/BoatSmoothingTask/source/Unity/FamilyPlayset/Library` | 4.08 GB | Retire its replay use or accept rebuilding; preserve inputs/evidence. |
| `LocalData/Installers` | 4.62 GB | Decide whether offline repair installers should be retained; do not remove installed toolchains. |
| `Builds/Abandoned-ipad-host-0.0.99` | 200 MB | Retire unused runtime binaries only; preserve abandonment markers, manifests, reviews and separately archived source patch. |

The four Library paths total **15.15 GB**, conditionally reclaimable. Preserve active main/AdventureRepair and mobile build caches. Unity cache deletion adds import/shader work; being regenerable is not enough to justify slowing current preparation. See the research's [retention comparison](project-structure-research-2026-10-02.md#retention-must-follow-purpose-and-consumers).

The roughly 26.14 GB of builds in those snapshot trees and 90.99 GB in root `Builds/NetworkProbe` are inventories, **not deletion lists**. Establish an exact artifact keep list before choosing old builds: currently prepared artifacts, installed-device artifacts, live server/hotfix inputs, selected rollback bundles and review/replay dependencies. Retire or update a launcher before deleting the build it selects. Foundation22, Solo67, SharedGarden68 and CharacterWorkshop `art-prep02-006` still had consumers in the audit.

### 3D Protected content and execution evidence

Exclude the parent's audit/review/research files, screenshots, exports and implementation evidence from cleanup. Keep Verification/incident records in this pass. Never delete a parent directory that would remove any of these protected children.

Preserve the three not-proven-redundant RosterBuild sources (`SoloNavigation.cs`, `SoloScreen.cs`, `WorldLayout.cs`) and reconcile them separately. Keep dirty/registered checkouts, editable/reference art, personal media, AudioStudio/MusicStudio inputs, AndroidAVD, FamilyLAN/SharedGarden records, saves, enrollment, signing identity, backups, RecoveryJobs, PCServer selections, history.bundle and the abandoned source patch. Absence of a runtime literal reference does not establish that authored work has no purpose.

For every execution batch, record an explicit allowlist of absolute targets, reason, retained dependency/consumer check and before/after logical bytes. Verify targets stay within the intended game subtree and have not become active; use literal paths and no wildcard parent purge. Reuse the completed audit rather than repeating unrelated scans. A new active consumer or source discrepancy removes that item from the batch until resolved. Report actual results, not estimated reclaimed capacity as an accomplished cleanup.

The game root also has an empty `Assets` plus untracked Unity `Packages` and `ProjectSettings`, alongside the actual project under `Unity\FamilyPlayset`. Confirm their origin and consumers, then archive confirmed accidental root-project files locally so only the real Unity project is presented. Do not touch the unrelated `Meeps game` project.

Completion: executed cleanup batches have exact removal receipts and retained dependencies still resolve; retained local folders have a documented purpose; active scripts resolve relocated data; live data, authored work and protected evidence remain intact. No duplicate root Unity project remains ambiguous. Conditional items left in place have a specific reason, not a blanket "temporary" label.

## Phase 4 Organize world code and shared systems

### 4A Group worlds without changing assembly membership

Current world code is mixed under `Code/Core` and `Code/Client`. Group consistently by Home, Park, Creek, Beach, Daycare, Zoo and Dinosaur under `Code/Core/Worlds/<World>` and `Code/Client/Worlds/<World>`. Put shared contracts/state/session/save rules under Core's shared scope and navigation/common presentation under Client's shared scope. Keep the existing Core, Adapters, Client, NetworkProbe, Runtime and Server assembly definitions and references. Preserve Core/Adapters' Unity-free boundary and Editor code's editor-only compilation.

Start by mapping files to their actual worlds and shared consumers. For example, `KingdomAdventure.cs` and `TreasureHunt.cs` belong to Daycare, while `SoloWorld.cs` connects all worlds. The audit found 43 files declaring parts of `SoloWorld` and 59 declaring parts of `SoloScreen`; separate files currently contribute to shared classes. Moving folders alone will not make them independent modules.

`Core/BeachSeagulls.cs` and `Core/ZooWorld.cs` both declare `SoloWorld`; `Client/SoloSeagulls.cs` and `Client/SoloZoo.cs` both declare `SoloScreen`. All parts of each partial type must remain in one assembly. Do not add per-world assembly definitions around these files. The [research explains the C#/Unity constraint and alternatives](project-structure-research-2026-10-02.md#recommended-code-layout-and-extraction-order).

Move one world at a time with matching Unity `.meta` files, preserving assembly membership. Update test projects, build/source manifests and tools that explicitly list source paths. Document each world's owned files and shared integration consumers before edits. Do not move Editor scripts beneath a runtime assembly without retaining an explicit Editor-only boundary.

### 4B Extract one world before repeating the pattern

Pilot Beach: extract its rules/state operations and drawing/input responsibilities into owned classes through narrow interfaces or method calls. Keep authoritative family state and existing snapshot/save/message contracts intact unless a separate bounded migration is required. Reduce edits needed in shared `SoloWorld`/`SoloScreen`; do not replace both classes wholesale. Name the remaining shared integration files and coordinate ownership when multiple chats need them.

Only after that pilot proves useful, apply the pattern to Zoo and Daycare. Feature assemblies are an optional later step when the feature no longer depends on partial declarations spread across assemblies. Folder moves, class extraction and assembly changes should have separate reviewable results, making failures easy to locate.

Completion per grouping: files are discoverable by world name, assembly membership is preserved, shared connections are named, and compilation plus the affected activity's focused check pass. Completion per extraction: the world-owned responsibilities no longer require routine edits to the shared class; one representative four-player check includes independent departure where shared behavior changes. Check saves/contracts only if changed or a relevant failure occurs. Avoid a whole-game rewrite or unrelated all-world tests.

## Phase 5 Clarify production names

Review `SoloWorld`, `SoloScreen`, `NetworkProbe`, `NetworkGardenSession`, `IGardenSession`, `FamilyPlayset` and the Foundation/Solo launcher names against their actual roles. `FamilySession` directly uses `SoloWorld`, so the name no longer describes only private solo play. Select clear names from actual responsibilities before renaming. Candidate labels such as `GameWorld`, `GameScreen` or `FamilyNetworkBootstrap` are proposals, not approved blanket substitutions; extraction may reveal a more precise responsibility.

Handle one connected set at a time with an old/new name and consumer map. Preserve enum values, serialized fields, assembly/type references, reflection callers, build entry names and Unity script GUIDs unless a specific migration is implemented. New C# names should describe responsibility and match the project's naming conventions. Asset descriptors should be readable and consistent; do not copy Unreal prefix lists mechanically. `SoloPrototype` is an actual save-directory component in `SoloScreen.cs`; it cannot be changed as an ordinary cosmetic folder rename. Retain compatibility paths or migrate existing saves explicitly if that storage name is changed.

Completion: each renamed set compiles, affected entry points resolve, and any changed serialized or save contract has a focused retention check. A naming change must not silently require a new family or fresh saves.

## Phase 6 Gather and group editable assets

Beach has 27 files under its checkout's `SourceArt/Beach`, while that folder is absent from the current main checkout. Park and Creek editable source folders exist in the visible root but are also absent from main. Determine which are unique, current, already copied elsewhere or historical before consolidating them. Do not assume absent files are lost or that differently named copies are identical.

Use the same world names across editable artwork, source audio and runtime assets. Daycare currently spans `Daycare`, `Adventure`, `Sandpit`, `Treasure` and `Vet`; map these to Daycare's Story Adventure, Sandcastle Club, Treasure Hunt and Animal Clinic ownership before selecting exact destinations. Group activities beneath their world; keep shared characters, books, common UI and music at their actual shared scope. Preserve existing stored IDs and source recordings. Do not rename every folder in one batch.

Move runtime assets with their `.meta` files and unchanged GUIDs. Maintain a separate old/new resource-key map: GUID preservation does not repair `Resources.Load` strings. Update actual consumers such as `Vet/tools`, `Sandpit/...`, `Treasure/...`, `BeachArt/seagull-poses` and `ZooArt/...`, importers and source README links. Do not assume a matching filename proves a matching editable source. Preserve personal media and signing material outside source commits. A switch to Addressables is outside this organization task; consider it only for a measured loading problem.

Completion: current editable sources are accounted for, each activity has traceable source and runtime assets, affected resources load, and the moved activity receives one focused visual check.

## Phase 7 Group tools and builds

Current main had 241 tracked loose files under `Tools` during the audit. Classify reusable tooling into Build, Install, Server, Content and Verification, with platform subdivisions where useful. Keep one-off incident scripts with their local incident records rather than promoting them all to permanent tools.

Moving a tool can change code that derives the project root from its script's parent directory. Update root resolution deliberately, along with launcher targets, source manifests, test-project includes and Mac transfer paths. Prefer an explicit project-root argument or a shared resolver based on existing project markers over fixed parent-depth guesses or absolute Desktop checkout paths. Verify it selects the intended checkout even when another registered checkout exists beneath the game root.

Establish a common platform/release build layout and a clear current prepared-update record. Keep version/protocol/content/schema metadata rather than changing build identity for cosmetic naming. Record exact artifact path, source fingerprint, signed identity, build outcome and required transfer state; retain historical artifacts separately and pin delivered/current/rollback consumers before retirement. Preserve source and signing provenance.

For future preparation, build the requested current artifact once or reuse an already verified identical current artifact. Preserve current caches; do not rebuild identical APK bytes or clear Library as routine folder maintenance. Mark a platform ready only when its required build/signing/transfer work is complete and its target device is identified from maintained records. A disconnected device remains a stated connection blocker. Preparation should leave installation/launch as the remaining work; actual device connection and transfer speed can still prevent a guaranteed one-minute install. No stale artifact substitution or data-erasing fallback.

Completion: affected tools resolve the intended project and data directories, build and install entry points remain clear, and no relocation falls back to an old artifact. Actual builds, device installations or server updates occur only within their requested task scope.

## Phase 8 Refresh the entry documents and folder rules

Replace stale current-status text in the README, including its build-128 rollout and former repository name. Keep detailed dated reports as history, with a short current entry page linking the source location, Unity project, current decisions, build/install tools and folder map.

Record the selected naming convention, world ownership, shared-file coordination and temporary-checkout location in project instructions. Before creating a folder, state its purpose, owner, consumers and intended lifetime and reuse an existing suitable location. New development checkouts belong under `LocalData/Worktrees`, only when isolation is needed. Record each checkout's real source location and open/finished status; finish integration before retiring it, with ignored assets and review material accounted for. No automatic destructive cleanup job is proposed.

Do not leave completed work in unexplained Desktop checkouts. Record each completed phase here with exact locations, repaired consumers, checks and remaining work. Document ongoing shared-file ownership rather than claiming world folders eliminate every cross-world edit. Complete any requested Mac inventory before proposing Mac relocations.

Completion: a new chat or the parent can identify the main source, each world, current operational tools and temporary work without knowing old task names. Phase results distinguish proposed, changed and verified work.

## Current execution record

Planning only. October 2 revision adds the cleanup audit's exact candidates/protections and incorporates studio/engine research into every phase. Documentation paths and changes are checked as documentation; no game qualification is implied. No deletion, relocation, rename, source refactor, asset migration, build, device installation or live-server change has been performed by this task. The next implementation phase is the Beach checkout move described in Phase 1.
