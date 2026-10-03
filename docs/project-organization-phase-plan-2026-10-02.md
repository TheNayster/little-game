# Little Weeps project organization phase plan

October 2, 2026. Organize the existing game so its main source, worlds, shared systems, editable assets, tools, builds and local data have clear names and predictable locations. Revised after the [unused-files audit](unused-files-audit-2026-10-02.md) and [studio/engine structure research](project-structure-research-2026-10-02.md). The research document compares every phase with public Unity, Epic, Riot and Rare evidence and distinguishes their practices from our recommendations. All eight phases have implementation results below. Windows cleanup and native Unity validation retain the explicit external blockers; these are not marked complete.

## Scope and order

Complete one phase at a time, run the required game/server checks below before continuing, and record its result here. Preserve unfinished work, editable sources, Unity asset metadata, generated builds, saves, device enrollment and signing identity. Folder maintenance does not install apps or replace the live server. The parent authorized starting implementation after the research was complete.

The audit covered the Windows project, associated Desktop folders and registered development checkouts. Mac folder organization remains uninspected; include it after resolving the requested scope. The Desktop Daycare review folder is an explicitly requested export, not an unexplained development checkout.

Operational paths and cleanup candidates below are relative to `C:\Users\sephi\Desktop\Little weeps game`, unless an absolute path is given. Maintained source paths are relative to the current source checkout, now the visible game root. Resolve the actual absolute target before any move or deletion; the former DinosaurWorld checkout is now Worktrees/PreviousMain.

Selected structure: keep the existing rules/presentation/server assembly boundaries, group worlds inside those layers, then extract shared-class responsibilities gradually. Keep authored assets and review evidence; retire bounded outputs by purpose and active consumers. Preserve current caches for fast updates. New folders require an actual purpose and contents, not an empty speculative hierarchy.

## Required game and server checks after every phase

User instruction, October 2: test the game and server after each organization phase to catch regressions before continuing. Implementation, compilation, GUID preservation and path checks alone do not complete a phase. Record **implementation status** and **game/server acceptance** separately.

Run one bounded verification pass after each phase, before the next phase. Resolve a failure before continuing. If an external blocker prevents a required test, leave the phase **awaiting runtime acceptance**, record the blocker and do not silently advance or claim success. Expand checks only for that phase's affected systems or an observed failure.

1. **Game smoke check:** launch the intended Windows client from the phase's actual source and normal project/launcher paths. Confirm the menu, character, movement, world travel and one shared activity work; exercise the affected screen/buttons and confirm its required art/audio loads. Check the logs for exceptions, missing scripts/resources and unexpected exits. Code/asset changes require a fresh successful build with matching source evidence. For relocation, cleanup or documentation with unchanged runtime inputs, an already verified identical artifact may be reused after checking its identity and affected paths; an older build cannot validate newer source.
2. **Server and multiplayer check:** start the matching candidate authority in an isolated temporary family on loopback, connect four independent clients, and verify compatible admission, shared state/progress, independent travel/departure and reconnect. Remaining siblings must keep their activity. Confirm the server stays alive and its test checkpoint reopens after a graceful stop/start. Do not use the real family save or publish the candidate to the live server for these checks.
3. **Installed server health:** read the current selection/process, endpoint, status, checkpoint metadata and recent error logs using the established [server update policy](server-update-policy.md). Confirm the installed authority and parent helper still run with the established endpoint/enrollment and no newly observed repeated crash/error. This read-only check is separate from candidate-server testing and does not prove every live gameplay flow. Do not restart occupied play, change firewall/settings, replace the server, install apps or reset/clear saves as a test shortcut.
4. **Evidence and decision:** record the source commit plus dirty-source fingerprint if applicable, exact artifact/version, affected worlds/activities, game result, isolated server/four-client result, installed-server health result, log/evidence location and any failed/blocked/untested items. Use **pass**, **fail**, **blocked** or **not run** explicitly. Only mark phase acceptance passed when its required checks actually pass.

### Additional checks for each phase

| Phase | Game checks beyond the common smoke | Server and data checks beyond the common smoke |
| --- | --- | --- |
| 1 Beach checkout move | Verify the relocated project's launcher/source resolution and one Beach encounter from an artifact matching that checkout | Verify relocated test-tool/server paths and preserve the checkout's working changes; no live deployment |
| 2 Main source location | Open the root Unity project and normal launchers; check travel across all seven worlds from the selected current source | Confirm authority/helper selection still resolves the established installation; preserved worktrees and save locations remain available |
| 3 Cleanup | Launch using retained artifacts and open the activities whose outputs/dependencies were candidates for removal | Confirm no removed item is needed by server startup, recovery or retained checkpoint/backup consumers; preserve live/current/rollback inputs |
| 4 World grouping and Beach extraction | Native Beach touch, flock animation, landing/footprints and hide-on-travel; quick world/menu traversal catches shared dispatcher regressions | Four clients share one flock encounter; an independent departure does not reset siblings; save/reopen retains state |
| 5 Production names | Load the existing scene/prefabs with no missing scripts; reopen a prior compatible isolated checkpoint | Verify unchanged message/save identities and compatible client admission, rather than assuming successful compilation proves migration |
| 6 Asset grouping | Open every world and each linked mini game once; confirm its required images, buttons and sounds load, including saved book resource IDs | Shared activity views agree across clients; reconnect/reopen retains world and activity state; exercise changed completion flows |
| 7 Tools and artifact locations | Execute the normal Windows build/preview path; verify Android/iPad artifact/source/signature selection and affected command paths without installing | Verify server/helper/watchdog/recovery path resolution and exact artifact provenance; installations/rollouts remain separately authorized |
| 8 Documentation and ownership | Follow the documented project/build/launch paths and run plan/link consistency; reuse unchanged verified runtime inputs where valid | Follow documented read-only health commands; require an evidence-backed consolidated final acceptance record for the organization work |

### Current acceptance gap and catch-up check

The completed organization work has preservation/path/GUID checks, seven-assembly compilation, thirteen tool-project compilations and a focused four-player **Core rules** Beach check. Those results do not establish native UI/resource loading or a running candidate/live server pass. No post-organization native game/server regression pass or installed-server health check was completed. Later Sandcastle Stage 2 source is also awaiting native acceptance; the catch-up build must contain the actual current source, not organization-era source or historical Windows423.

Full Unity build424 failed because Windows Application Control blocked Unity's Bee.Tools.dll; a Daycare test apphost was also blocked. All native game/isolated-server acceptance in this plan remains **blocked/not run** until a fresh current-source build and required checks succeed. The archive/cache removal blocker is a separate cleanup status, not a runtime-test result.

Run **one consolidated catch-up pass** covering the phase risks above against the current source when the build blocker is resolved. Record it as a catch-up result; do not invent eight historical after-phase passes or repeat identical checks eight times. New phase work must use the checks between phases from now on. This documentation change adds the requirement; it does not claim the missing tests ran or authorize a live rollout.

| Phase | Result | Implementation status |
| --- | --- | --- |
| 1 | Move the Beach checkout inside the game folder and repair dependent paths | Relocation/preservation verified October 2; runtime acceptance pending |
| 2 | Make the obvious game folder the clear home of current source | Implemented; preservation verified |
| 3 | Separate local data and remove specifically verified unnecessary outputs | Partial: 17 archives removed; remaining removals blocked |
| 4 | Group worlds inside existing layers, then extract shared-class responsibilities | Implemented; compile/rules pass; native view check blocked |
| 5 | Replace misleading production code names safely | Implemented; assembly compile passes |
| 6 | Gather editable assets and apply consistent world grouping | Implemented; 970 asset/meta byte checks pass |
| 7 | Group reusable tools and clarify build locations | Implemented; portability checks; full build blocked |
| 8 | Correct entry documentation and record folder rules for future chats | Implemented; current entry/ownership records |

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

Before Phase2, the visible root held older unfinished edits and main lived inside LocalData/DinosaurWorld. Phase2 preserved that work and established the visible root as current source; see the implementation record.

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

### Phase 1 completed October 2

Moved the complete checkout using `git worktree move` from `C:\Users\sephi\Desktop\Little weeps beach seagulls` to `C:\Users\sephi\Desktop\Little weeps game\LocalData\Worktrees\Beach`. The old directory is absent and Git's registration/backlink resolves the new directory. No active Beach editor/build process was found; the Beach chat was not loaded and its saved chat directory was the shared `Meeps game` entry point, so no chat-directory relocation was required.

Preservation check: all 37,819 file inventory entries (13,864,498,929 logical bytes) and 3,577 directories match the before-move inventory. SHA256 checks pass for 2,585 selected files covering source, assets and tools. Branch `codex/beach-waves`, original commit `8af74bc6ea8c3f696029ce178bd6fef372b23769` and all 96 local change entries are unchanged. Ignored builds, local data, source art/audio and protected review material moved with the checkout.

One task-local consumer required repair: `LocalData/DinosaurWorld/LocalData/PhoneIntegration336/beach.py`. It now resolves Beach beneath the shared repository location obtained from Git rather than an absolute Desktop path. Python syntax and only its path-initialization statements were checked; the historical integration loop was **not** executed. Root/main project instructions now record the new path for future chats. Current launchers/build tools already derive paths from their checkout; the relocated Unity project path resolves and PowerShell entry-point syntax passes. No shortcut targeting the old directory was found in the inspected Windows shortcut locations. Dated build/evidence records retain their original paths as historical provenance.

The final tool scan also found old absolute paths in generated `.NET obj` restore/source-link metadata for three local test projects. No checked maintained tool uses a `--no-restore` invocation of these projects; normal restore regenerates that metadata when the tests are next built. These caches and existing binaries were preserved rather than editing historical compiler metadata or running unrelated tests. Unity generated caches were likewise preserved; this move did not require launching an editor.

Private preservation receipt: `C:\Users\sephi\Desktop\Little weeps game\LocalData\Verification\beach-relocation-2026-10-02.json`. It contains the before inventory/hashes and verification results, not credentials or an installation claim.

Purpose/owner: Beach development work, associated with the **Beach** chat and `codex/beach-waves`. Status: relocated, with local work still retained. Known consumers: checkout-relative build/tool launchers and the repaired task-local integration script. Retirement requires reconciling unfinished edits and editable/ignored assets, accounting for protected evidence, and completing any remaining source delivery. The relocation did not merge, switch or delete this task branch.

No game code, asset identity, save path or assembly changed. No cache purge, build, device install or live-server operation occurred. Verification covered preservation and path resolution; it does not claim that every world/minigame was playtested. Next: Phase 2, preserve/reconcile the dirty visible-root work before establishing the obvious main source location. Cleanup Phase 3 has not started.


## Phases 2 through 8 implementation record

The user authorized continuing all phases. Current maintained source paths are relative to `C:\Users\sephi\Desktop\Little weeps game`. Original phase descriptions and dated receipts above retain their planning/history context.

### Phase 2 source location

Preserved 57,464 files/16,927,742,624 logical bytes, 2,980 selected hashes and all135 tracked modifications in `LocalData/Worktrees/HomeScience` on codex/home-science-coloring. Accidental root Assets/Packages/ProjectSettings/Library/Logs/UserSettings are preserved in its AccidentalRootUnityProject folder. Copied missing protected review docs back to their root locations. Root source now uses the current shared baseline; active main Unity cache moved with it for fast builds. Former DinosaurWorld main is detached at b3c42c1 in `LocalData/Worktrees/PreviousMain`; official worktree repair and normalized source comparison passed. Ignored incident replay scripts now target PreviousMain; current WorldReview source scripts target the visible root. No historical deployment loop was executed. Private receipt: `LocalData/Verification/organization-phase2-2026-10-02.json`. Other unfinished registered checkouts and unique source remain retained.

### Phase 3 bounded cleanup

Deleted exactly the17 listed iPad transfer archives after fresh SHA256 receipt matches and full verification of52,599 retained export manifest entries. Removed5,132,959,651 logical bytes. Successful exports and receipts remain. Private result: `LocalData/Verification/organization-phase3-cleanup-2026-10-02.json`. Automatic approval review rejected the four explicit retired Library removals and the platform-tools ZIP/empty tmp batch with `blocked by policy`; all remain intact. No alternate removal path was used. Thefour caches total15.15GB; keep them marked blocked rather than claiming reclaimed space. Small .NET outputs, five staging indexes, offline installers, selected builds/current caches, live server/rollback, saves, signing/enrollment, recovery, editable inputs and every audit/review remain protected or conditional. Operational LocalData paths remain stable where active consumers depend on them.

### Phases 4 and 5 source ownership and names

Moved162 existing C# files and their .meta files into Core/Client world/shared folders across all seven worlds, preserving assembly identities. BeachFlockRules owns validation/transitions; SeagullFlockView owns flock artwork/animation/footprints, with narrow shared dispatch and widget delegates. Production GameWorld/GameScreen/IWorldSession/NetworkWorldSession/FamilyNetworkBootstrap/FamilyGameBuild and related pointer/narration names replace misleading prototype names. Activity screen filenames identify SandcastleClub, AnimalClinic, StoryAdventure, TreasureHunt and HideAndTag. Networking replaces the Code/NetworkProbe folder; the foundation prototype is explicitly Legacy/Runtime. FamilyNetwork scene and descriptive build profiles preserve their GUIDs. MovedFrom attributes retain Unity class identity where applicable. Snapshot fields/enums, saved IDs, SoloPrototype save paths, wire identifiers, protocol3/content67/schema49 and assembly/namespace identities remain compatible. The manifest records exact mappings and shared-file coordination; no per-world assembly was invented for partial classes.

### Phase 6 assets

Grouped runtime artwork/audio by world and shared ownership; distributed44 scenery resources to their owning worlds. Verified all970 moved resource/meta files byte-for-byte, including GUID-bearing metadata. WorldResources translates persisted older resource IDs and dynamic scenery paths; current source and importers use new paths. Gathered42 editable Beach/Park/Creek files (36.88MB) from preserved worktrees without overwriting existing main inputs. Grouped14 editable art/audio paths, retaining prompts/frame maps and updating generators. Private resource receipt: `LocalData/Verification/organization-assets-2026-10-02.json`. SourceScenery authoring provenance remains shared; no personal media was gathered.

### Phase 7 tools and artifact consumers

Grouped219 tool files/projects by Build, Devices, Content, Documentation, Launch and Verification. Python imports/root resolution and PowerShell sibling/root paths follow the new locations. Test-project source includes are updated for nested world folders and new tool depth. Existing live imported server/helper modules, parent-ui, signing/toolchain pins and connectors remain at Tools root as documented operational exceptions. Four established mobile/build entry points are parameter-preserving shims. Maintained manifests and historical source classifiers understand old/new source layouts. Existing generated Builds/NetworkProbe, AndroidSigned and iOSFamilyLAN layouts and selected preview records stay stable; historical builds are never relabeled as current.

### Phase 8 ownership and verification

README, AGENTS, folder guide, migration manifest, current decisions and build-guide work record now describe actual locations, ownership, compatibility exceptions and folder lifecycle. Maintained source links/renderers are updated; dated claims/raw evidence remain dated. No review folders were deleted.

Verification: all230 C# files across Core, Adapters, Client, networking, retained Runtime, Server and Editor compile using the project's Unity defines/references. Focused Beach rules check passes for four-player shared flight, independent travel, landing, empty-area pause and checkpoint retention. Python syntax and PowerShell parsing pass; resource/GUID/path/tool checks are recorded. A full Unity build424 was attempted; Windows Application Control rejects installed Unity Bee.Tools.dll (0x800711C7), confirmed in CodeIntegrity events. Candidate424 is not a successful/delivered artifact. Native four-client view/asset loading remains pending that external blocker. No mobile build/install or live-server rollout occurred. A Daycare test apphost was also blocked by the same Windows policy; its project compiled, but this is not a runtime pass.

Remaining: resolve the Windows Application Control build block through the machine's authorized policy/support process, then run the prepared targeted native check against a fresh successful release. Remaining exact cleanup stays blocked/conditional as listed; never delete protected reviews or unique work to claim all phases complete. Mac organization was not inspected or changed.

Final path/identity check: 162 original script metadata hashes preserved; 1,058 Unity asset GUIDs unique; all219 tool destinations and138 project source includes resolve. All13 .NET tool projects compile. Plan consistency passes for3,891 local links. Historical Windows423 is retained at `LocalData/Worktrees/PreviousMain/Builds/NetworkProbe/G3-0.0.423`; read-only verification/preview lookup resolves the exact requested release, never a different build number. Install tools retain their explicit source/version/signature verification and do not use this archive lookup.
