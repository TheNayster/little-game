# Little Weeps unused files and folders audit

October 2, 2026. Identify redundant temporary files and generated outputs without removing useful game material. The user explicitly excludes audit and review files/folders because they are being used to improve the game. This is research only: nothing was deleted, moved or renamed.

## Scope and method

Inspected the Windows game root, its LocalData and Builds inventories, and development checkout registrations. Checked maintained build/launch/server tools, local task scripts, installation records and preview selections. A targeted reference search examined 3,362 distinct text files after deduplicating identical copies, excluding generated caches, media and review/documentation trees. This is not a complete proof of every possible dynamic or manually invoked dependency; absence of a literal path alone is not enough to call a file useless.

For the strongest duplicate finding, hashed 17 transfer archives against their successful transfer receipts and checked every file in their corresponding retained exports against the export manifests. For temporary staging indexes, checked that all referenced blobs remain reachable through committed history. Compared copied C# source contents against reachable history with CRLF normalized, rather than assuming an old folder contains only disposable files.

Sizes below are logical file bytes, expressed in decimal GB/MB. They are not guaranteed reclaimed disk space. Mac folders, external signing stores, unrelated projects and user application-data directories were not audited.

## Confirmed redundant transfer files

These 17 exact files are compressed transfer intermediates under the root `LocalData` folder:

```text
ios-g3-0.0.71.tar.gz
ios-g3-0.0.72.tar.gz
ios-g3-0.0.73.tar.gz
ios-g3-0.0.74.tar.gz
ios-g3-0.0.79.tar.gz
ios-g3-0.0.82.tar.gz
ios-g3-0.0.91.tar.gz
ios-g3-0.0.92.tar.gz
ios-g3-0.0.93.tar.gz
ios-g3-0.0.94.tar.gz
ios-g3-0.0.95.tar.gz
ios-g3-0.0.98.tar.gz
ios-g3-0.0.101.tar.gz
ios-g3-0.0.110.tar.gz
ios-g3-0.0.128.tar.gz
ios-g3-0.0.171.tar.gz
ios-g3-0.0.227.tar.gz
```

Total: **5,132,959,651 bytes, or 5.13 GB**. Each archive's SHA256 matches its successful transfer receipt. Each corresponding `Builds/iOSFamilyLAN/G3-0.0.<version>` export remains present, and its full artifact manifest passes with zero missing/changed files. The transfer tool packages those export files and manifests into the archive; the successful receipt records the verified extraction. Normal gameplay, app launch and installed-device operation do not consume these transfer intermediates. Removal would give up convenient resuming of an already-completed old transfer, while retaining the verified exports and transfer records.

Do not expand this list into a wildcard deletion. Six other root archives totaling about 1.58 GB do not meet the same completed-transfer proof: G1 versions 1, 2, 16 and 20, G2 version 56, and G3 version 172. Some use older receipt formats; others lack receipts. They are retained pending a separate duplicate check. Recent archives inside active development checkouts were not added to this list.

## Other small cleanup candidates

| Exact item | Evidence | Disposition |
| --- | --- | --- |
| Root `tmp/pdfs`, and its otherwise empty `tmp` parent | Zero files and zero file bytes. | Empty-folder candidate; retain if a current task claims it before cleanup. |
| `LocalData/platform-tools-official-r36.zip` | 7,138,784 bytes; no reference in the targeted current text search. The established Android device record points to an existing installed `AppData/Local/Android/Sdk/platform-tools/adb.exe`, not this ZIP. | Download-cache candidate. Does not remove the installed ADB tools. |
| Five temporary staging indexes listed below | Their entire blob inventories remain reachable in committed history; no callers were found in the checked maintained tools/local scripts. Combined size 3,182,689 bytes. | Candidate after confirming their old tasks are closed. Keep their ledgers and all review/research material. |
| `LocalData/OutfitCommitCheck/bin` and `obj` | Generated .NET outputs totaling 1,316,665 bytes. `check-outfit-commit.py` creates the copied harness and runs dotnet again. All 37 copied C# source files were found in committed history. | Rebuildable outputs; do not remove the input index or replay script silently. |
| `LocalData/CharacterRosterRules/bin` and `obj` | Generated .NET outputs totaling 1,075,016 bytes; the local project remains present. | Rebuildable outputs only. Preserve project/source inputs and any review records. |

The five temporary indexes are:

```text
LocalData/FixedParentStartTask/fixed-start.index
LocalData/FasterSeekTask/seek.index
LocalData/SharedCoverTask/shared-cover.index
LocalData/HomeOnlyHideTask/home-only.index
LocalData/BathroomTask/bathroom.index
```

Nine indexes were inspected in total. `character-roster.index`, `full-roster.index` and `outfit-task.index` still have explicit old staging/replay script consumers, so they are not independent deletion candidates. The `pond-research-...index` belongs to research-related work and is excluded from cleanup even though its blobs are preserved. A staging index's role is metadata about a combination of files; preserved blobs alone do not prove the combination is no longer needed by a task.

## Larger generated outputs needing retirement decisions

| Location | Measured amount | What must be resolved first |
| --- | --- | --- |
| `LocalData/PondBuild/snapshot/Unity/FamilyPlayset/Library` | 3.76 GB | Generated cache for an old copied build snapshot. Local pond build scripts still name that snapshot; confirm it is idle and accept rebuilding the cache if replayed. Keep source, assets, docs, screenshots and manifests. |
| `LocalData/RosterBuild/Unity/FamilyPlayset/Library` | 3.62 GB | Cache can be recreated, but the surrounding folder contains source not preserved by the history check. Never remove the whole folder as cache. |
| `LocalData/WheelsBuild/snapshot/Unity/FamilyPlayset/Library` | 3.68 GB | Generated cache; confirm the snapshot is retired and idle. Keep editable artwork and review material. |
| `LocalData/BoatSmoothingTask/source/Unity/FamilyPlayset/Library` | 4.08 GB | Generated cache; task-local replay scripts still refer to this source copy. Confirm retirement before cleanup. |
| `LocalData/Installers` | 4.62 GB | Downloaded installers, not the installed toolchain. Main Unity installer alone is 4.13 GB. Potentially useful for offline repair/reinstallation; decide whether to retain that convenience. |
| `Builds/Abandoned-ipad-host-0.0.99` | 200 MB | Explicit `ABANDONED.txt` says mobile hosting was retired and this build was never deployed. No maintained-tool reference was found. Runtime binaries are retirement candidates; keep historical markers/manifests and the separate archived source patch. |
| Root `Library`, `Logs`, `Packages`, `ProjectSettings`, empty `Assets` | About 17.1 MB plus generated settings | These form the extra Unity project identified in the organization audit. Current build/connector tools select `Unity/FamilyPlayset`. Confirm origin and any manual Unity Hub entry before archiving/removing root-project residue; preserve the real Unity project's settings. |

The four listed snapshot Library caches total **15.15 GB**. They are regenerable, not proof that their entire containing folders are useless. Keep the current main/AdventureRepair Unity caches and mobile build caches so ordinary device preparation does not become slower.

Those four snapshot trees also contain about **26.14 GB of generated builds**. Root `Builds/NetworkProbe` alone contains 201 build directories totaling 90.99 GB. These are storage inventories, not an approved deletion list: older builds can still support review, replay and rollback. Any retirement must keep current prepared artifacts, installed-device artifacts, live-server/hotfix inputs, retained rollback artifacts and binaries referenced by review/test tools.

## Files that looked temporary but must be retained

- **All audit/review material**, including the Desktop Daycare review export, world audits, screenshots, research and implementation evidence. Do not delete a containing task folder if that would remove protected files inside it. Preserve Verification and incident records in this pass.
- **RosterBuild source.** Three exact C# contents were not found in currently reachable committed history after line-ending normalization: `Code/Client/SoloNavigation.cs`, `Code/Client/SoloScreen.cs` and `Code/Core/WorldLayout.cs` under its Unity project. This does not establish a missing feature; it establishes that the source copy is not safely proven redundant. Preserve and reconcile it before any whole-folder retirement.
- **Registered development checkouts**, including Beach, AdventureRepair, AdventureWork, ZooFull and ZooBuild/snapshot. Several retain local source/metadata changes. Move or reconcile them under the organization plan rather than deleting them as temporary folders. Two detached checkouts are still registered checkouts, not ordinary cache directories.
- **Editable/reference assets and personal media.** Beach, Park and Creek source artwork was already found scattered between copies. Character references, audio drafts and source recordings also have purposes outside direct runtime references.
- **AudioStudio and MusicStudio dependencies.** AudioStudio is 21.57 GB, but maintained generators explicitly use its models and virtual environments. World music generation uses the MusicStudio renderer/font. These folders are reusable tools, not unexplained output.
- **AndroidAVD.** About 10.01 GB; the Android emulator configuration explicitly selects it. Removing it would remove established test devices and their data.
- **FamilyLAN, SharedGarden, NetworkProbe local records, backups and recovery jobs.** These contain saves, enrollment, screenshots or rollback inputs. `parent_server.py` resolves the family under FamilyLAN; `server_recovery.py` directly reads RecoveryJobs for rollback. Large counts of GUID folders do not make them disposable.
- **PCServer/current, its metadata and selected previous bundle.** The installation record points to the live executable and `previous-270227512db34566b4cdecefb7fb4401`. The installer stores previous bundles for rollback. Do not delete the PCServer directory or every `previous-*` folder as a batch.
- **GitMaintenance/history.bundle and AbandonedWork source patch.** They preserve history/recovery information, including work that was deliberately retired. This is different from keeping obsolete runtime binaries.
- **Older preview builds selected by existing launchers.** Root `Play-Foundation.cmd` selects build 22 via `latest-windows-preview.json`; `Play-SoloPrototype.cmd` selects 67; `Play-SharedGarden.cmd` selects 68. CharacterWorkshop still selects `art-prep02-006`. Deleting all old builds would break these launchers. Their retirement belongs with the tool/launcher organization phase.

## Cleanup sequence proposed for the organization plan

Keep the Beach move as Phase 1. Within Phase 3, start with the explicit verified transfer-archive list, the unused download ZIP and genuinely empty folders. Then consider closed-task staging metadata and rebuildable old-task outputs. Handle larger snapshot caches only after task retirement is established. Decide on installer retention separately. Preserve protected material even when the enclosing directory contains otherwise redundant outputs.

Before any later deletion, recheck the exact absolute path, its consumers and whether another task has started using it. Keep cleanup restricted to recorded items, verify retained dependencies still resolve, and report actual reclaimed space. A new source discrepancy or an active reference removes an item from the cleanup list until resolved. No deletion, source/build change, device operation or live-server operation was performed by this audit.
