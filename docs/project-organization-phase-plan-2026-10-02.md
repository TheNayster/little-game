# Little Weeps project organization phase plan

October 2, 2026. Organize the existing game so its main source, worlds, shared systems, editable assets, tools, builds and local data have clear names and predictable locations. This plan follows the Windows folder and naming audit. The user selected the Beach checkout relocation as the first implementation phase. This document records the plan; the relocation and later changes have not been performed.

## Scope and order

Complete one phase at a time and record its result here. Preserve unfinished work, editable sources, Unity asset metadata, generated builds, saves, device enrollment and signing identity. Folder maintenance does not install apps or replace the live server. Research-only instructions remain in effect for this planning task.

The audit covered the Windows project, associated Desktop folders and registered development checkouts. Mac folder organization remains uninspected; include it after resolving the requested scope. The Desktop Daycare review folder is an explicitly requested export, not an unexplained development checkout.

| Phase | Result | Status |
| --- | --- | --- |
| 1 | Move the Beach checkout inside the game folder and repair dependent paths | Next implementation phase |
| 2 | Make the obvious game folder the clear home of current source | Proposed |
| 3 | Separate local development copies, operational data and temporary records | Proposed |
| 4 | Group code by world and identify shared connections | Proposed |
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

Completion requires a recorded relocation result, repaired active references and successful focused checks. List any unresolved active reference explicitly. The previous directory remains the rollback destination if the move needs reversing; never discard content to undo it.

## Phase 2 Establish the main source location

The visible game root is currently an older development checkout with substantial local edits. The checkout on `main` is inside `LocalData\DinosaurWorld` and contains the whole current game. Its name incorrectly implies a single world.

Inventory and preserve the root's unfinished work first. Reconcile ownership and source differences before selecting the safe checkout transition. Put the current shared baseline in the obvious game location and retain unfinished work in clearly identified temporary checkouts. Update editor targets, launchers and maintained paths, including local scripts that explicitly reference `LocalData/DinosaurWorld`.

Completion: one documented main source location; the normal launcher opens its `Unity\FamilyPlayset` project; unfinished edits and editable assets remain recoverable; no build or task silently uses the displaced checkout. Do not force a clean root with reset, deletion or a directory swap that breaks nested checkouts.

## Phase 3 Separate local data by purpose

Root `LocalData` had 275 loose files during the audit, alongside source checkouts, device records, server installations, backups and experiments. Organize these after Phase 2 resolves the main source path.

Proposed categories are `Worktrees`, `Devices`, `Server`, `Backups`, `PreparedUpdates`, `Verification` and `Temporary`. Keep these local and excluded from source commits. Classify each item before moving it; filenames and age alone do not prove something is disposable. Server paths such as `PCServer/current` and device/signing records have active consumers and need coordinated reference changes. Do not relocate live operational data during occupied family play.

The game root also has an empty `Assets` plus untracked Unity `Packages` and `ProjectSettings`, alongside the actual project under `Unity\FamilyPlayset`. Confirm their origin and consumers, then archive confirmed accidental root-project files locally so only the real Unity project is presented. Do not touch the unrelated `Meeps game` project.

Completion: every retained local folder has one documented purpose; current scripts resolve the new paths; live data and unfinished work are preserved; no duplicate root Unity project remains ambiguous.

## Phase 4 Organize world code and shared systems

Current world code is mixed under `Code/Core` and `Code/Client`. Proposed grouping keeps rules and presentation separate while grouping each consistently by Home, Park, Creek, Beach, Daycare, Zoo and Dinosaur. Shared state, sessions, saves, navigation and common UI receive clear shared locations.

Start by mapping files to their actual worlds and shared consumers. For example, `KingdomAdventure.cs` and `TreasureHunt.cs` belong to Daycare, while `SoloWorld.cs` connects all worlds. The audit found 43 files declaring parts of `SoloWorld` and 59 declaring parts of `SoloScreen`; separate files currently contribute to shared classes. Moving folders alone will not make them independent modules.

Move one world at a time with matching Unity `.meta` files, preserving assembly boundaries. Update test projects and tools that explicitly list source paths. Document the shared connection points and coordinate changes to them. Consider extracting those connections into smaller modules only as separately bounded behavior-preserving changes.

Completion per world: all its files can be located by the world name; shared connections are identified; compilation and the affected activity's focused check pass. Avoid a whole-game rewrite.

## Phase 5 Clarify production names

Review `SoloWorld`, `SoloScreen`, `NetworkProbe`, `NetworkGardenSession`, `IGardenSession`, `FamilyPlayset` and the Foundation/Solo launcher names against their actual roles. `FamilySession` directly uses `SoloWorld`, so the name no longer describes only private solo play. Select clear names from actual responsibilities before renaming.

Handle one connected set at a time. Preserve enum values, serialized fields, assembly/type references and Unity script GUIDs unless a specific migration is implemented. `SoloPrototype` is an actual save-directory component in `SoloScreen.cs`; it cannot be changed as an ordinary cosmetic folder rename. Retain compatibility paths or migrate existing saves explicitly if that storage name is changed.

Completion: each renamed set compiles, affected entry points resolve, and any changed serialized or save contract has a focused retention check. A naming change must not silently require a new family or fresh saves.

## Phase 6 Gather and group editable assets

Beach has 27 files under its checkout's `SourceArt/Beach`, while that folder is absent from the current main checkout. Park and Creek editable source folders exist in the visible root but are also absent from main. Determine which are unique, current, already copied elsewhere or historical before consolidating them. Do not assume absent files are lost or that differently named copies are identical.

Use the same world names across editable artwork, source audio and runtime assets. Daycare currently spans `Daycare`, `Adventure`, `Sandpit`, `Treasure` and `Vet`; document their activity ownership and proposed destinations together. Keep shared characters, books, common UI and music clearly separate.

Move runtime assets with their `.meta` files. Update actual string-based consumers such as `Resources.Load("Vet/tools")`, `Sandpit/...` and `Treasure/...`, importers and source README links. Preserve personal media and signing material outside source commits.

Completion: current editable sources are accounted for, each activity has traceable source and runtime assets, affected resources load, and the moved activity receives one focused visual check.

## Phase 7 Group tools and builds

Current main had 241 tracked loose files under `Tools` during the audit. Classify reusable tooling into Build, Install, Server, Content and Verification, with platform subdivisions where useful. Keep one-off incident scripts with their local incident records rather than promoting them all to permanent tools.

Moving a tool can change code that derives the project root from its script's parent directory. Update root resolution deliberately, along with launcher targets, source manifests, test-project includes and Mac transfer paths. Establish a common platform/release build layout and a clear current prepared-update record. Keep historical artifacts distinct from the current prepared artifacts; preserve source and signing provenance.

Completion: affected tools resolve the intended project and data directories, build and install entry points remain clear, and no relocation falls back to an old artifact. Actual builds, device installations or server updates occur only within their requested task scope.

## Phase 8 Refresh the entry documents and folder rules

Replace stale current-status text in the README, including its build-128 rollout and former repository name. Keep detailed dated reports as history, with a short current entry page linking the source location, Unity project, current decisions, build/install tools and folder map.

Record the selected naming convention, world ownership, shared-file coordination and temporary-checkout location in project instructions. Do not leave completed work in unexplained Desktop checkouts. Record each completed phase here with exact locations, repaired consumers, checks and remaining work. Complete any requested Mac inventory before proposing Mac relocations.

Completion: a new chat or the parent can identify the main source, each world, current operational tools and temporary work without knowing old task names. Phase results distinguish proposed, changed and verified work.

## Current execution record

Planning only. No relocation, rename, source refactor, asset migration, build, device installation or live-server change has been performed by this task. The next implementation phase is the Beach checkout move described in Phase 1.
