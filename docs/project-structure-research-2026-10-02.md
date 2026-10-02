# Little Weeps project structure research and plan comparison

October 2, 2026. Research requested by the parent after the Windows organization and unused-files audits. The resulting recommendations are incorporated into the [organization phase plan](project-organization-phase-plan-2026-10-02.md). This is planning, not an executed reorganization.

## Conclusion for this game

Keep one clear current source location. Group worlds within the existing rules and presentation layers, then gradually extract world-specific responsibilities from shared classes. Give editable assets, runtime assets, operational data, reusable tools and disposable outputs distinct homes. Record who owns temporary development copies and when they can retire. Preserve useful build caches and the parent's audit/review material.

These are recommendations for Little Weeps, derived from the inspected project and the sources below. There is no single studio folder tree to copy. The important difference from the first plan is that folder grouping, code independence and output retirement are separate jobs, each with its own completion evidence.

## What established teams publicly describe

The studio accounts describe particular systems at their publication dates, not verified 2026 internal policy. Epic's Lyra is a public example project, not a disclosed Fortnite repository. Riot's League client article concerns the client application architecture; its design lessons do not establish a Unity gameplay folder layout. Rare's public talk abstract establishes workflow principles, not an inspected source tree.

| Primary evidence | What it actually establishes | Application to Little Weeps |
| --- | --- | --- |
| [Unity project organization guidance](https://unity.com/how-to/organizing-your-project) | Consistent documented naming, separation of experimental and production content, and collaboration conventions matter more than a universal layout. | Establish a small folder map and new-folder rules. Keep the requested root name; quote paths containing spaces in tools. |
| [Epic's Lyra sample](https://dev.epicgames.com/documentation/en-us/unreal-engine/lyra-sample-game-in-unreal-engine) | Shared core content coexists with feature plugins; ShooterCore owns gameplay/UI while ShooterMaps owns related maps. | Give world features discoverable homes while retaining shared systems. Do not copy Unreal plugin mechanics into Unity. |
| [Riot's Legends of Runeterra pipeline, May 2021](https://www.riotgames.com/en/news/legends-runeterra-cicd-pipeline) | A Unity game keeps client, server, assets and tools together. Persistent workspaces support caches; hash-based build planning avoids unchanged work; temporary branches/environments have owners and retirement conditions. | Keep the game together, preserve current caches, reuse proven current artifacts, and record a lifecycle for temporary copies. |
| [Riot's League client architecture](https://www.riotgames.com/en/news/architecture-league-client-update) | Shared framework changes had created conflicts between teams. Plugin ownership and explicit dependencies reduced coupling; private component data replaced a broadly shared object graph. | Extract world responsibilities behind narrow connections. A folder alone cannot give a chat independent ownership of a shared class. |
| [Rare's 2018 GDC delivery talk](https://gdcvault.com/play/1025318/Adopting-Continuous) | The public abstract describes a single-branch strategy, automated valuable checks and rapid delivery/feedback. | Keep the established completed-work integration policy and proportionate checks; do not add permanent branches per world. |

Riot's Runeterra account used hierarchical branches, while Rare described a single-branch approach. That difference is evidence against claiming that all large studios use one structure or workflow. Little Weeps should retain the parent's current integration rules rather than importing either studio's entire process.

## The inspected Little Weeps boundaries

The current main checkout is `LocalData/DinosaurWorld`, despite containing the complete game. The visible root is an older checkout with unfinished edits. The [unused-files audit](unused-files-audit-2026-10-02.md) records the local data, artifact consumers and unique-source exceptions. Neither checkout can be discarded to make the tree look tidy.

The actual Unity project uses 6000.3.24f1. Under `Unity/FamilyPlayset/Assets/FamilyPlayset/Code`, six assembly definitions already establish useful boundaries:

| Folder / assembly | Current dependencies and role to preserve |
| --- | --- |
| `Core / LittleWeeps.Core` | No Unity engine references; shared game rules/state. |
| `Adapters / LittleWeeps.Adapters` | References Core; also excludes Unity engine references. |
| `Client / LittleWeeps.Client` | References Core, Adapters, Input System and Unity UI; presentation/input. |
| `NetworkProbe / LittleWeeps.NetworkProbe` | References Core, Adapters, Client and networking/Unity packages; current network composition. |
| `Runtime / LittleWeeps.Runtime` | References Core, Input System and Unity UI. |
| `Server / LittleWeeps.Server` | References Core. |

The existing Editor code has no custom assembly definition. Preserve its editor-only compilation when moving it. An assembly definition includes scripts beneath its folder unless a nearer assembly definition/reference intervenes; Unity supports explicit Editor-only assemblies. Consequently, changing a folder's assembly ancestry is a code change, even if the source text stays identical. [Unity assembly asset rules](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-creating.html).

The specific Beach/Zoo collision is not hypothetical:

- `Core/BeachSeagulls.cs` and `Core/ZooWorld.cs` both declare parts of `SoloWorld`.
- `Client/SoloSeagulls.cs` and `Client/SoloZoo.cs` both declare parts of `SoloScreen`.
- The audit counted 43 `SoloWorld` partial declaration files and 59 `SoloScreen` partial declaration files. Separate source files can still share one class's fields, methods and dependencies.
- `Core/FamilySession.cs` directly owns a `SoloWorld`, so it participates in shared play as well as private play.

C# requires every part of a partial type to be in the same assembly/module. Creating a Beach assembly and Zoo assembly around the current partial files would break that requirement. [Microsoft partial-type rules](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods).

## Recommended code layout and extraction order

| Option | Fit for the current code | Decision |
| --- | --- | --- |
| Keep only flat Core/Client folders | Preserves compilation but leaves world ownership hard to find. | Retain the layers, improve grouping inside them. |
| Put all rules and UI under world folders with one assembly per world immediately | Appealing visually, but breaks partial-class assembly placement and risks pulling Unity into rules/server code. | Do not use as the first migration. |
| Group worlds beneath existing layers, then extract bounded feature classes | Makes ownership visible while preserving current dependencies; allows later independence based on evidence. | Selected recommendation. |

Proposed paths, not existing directories or instructions to generate an empty tree:

```text
Unity/FamilyPlayset/Assets/FamilyPlayset/Code/
  Core/
    LittleWeeps.Core.asmdef
    Shared/                  sessions, saves, common contracts
    Worlds/Beach/            Beach rules and state
    Worlds/Zoo/              Zoo rules and state
    Worlds/Daycare/           activity rule groups
  Client/
    LittleWeeps.Client.asmdef
    Shared/                  navigation, common controls, presentation services
    Worlds/Beach/            Beach drawing and interaction
    Worlds/Zoo/
    Worlds/Daycare/
  Adapters/
  Runtime/
  NetworkProbe/              retain until its connected rename is safe
  Server/
  Editor/                   retain editor-only compilation
```

Apply the same world names to Home, Park, Creek and Dinosaur when their files move. Each Daycare activity can have a subordinate folder, but shared teacher/NPC/session code belongs at its actual shared scope. This keeps rules separate from Unity presentation and preserves the current assembly definitions during the initial grouping.

After grouping, use Beach as one extraction pilot. Move seagull behavior and presentation into owned classes, with explicit inputs/results and a small shared integration point. Keep the existing authoritative family world, serialized contract and round/session behavior. Prove the pilot before repeating it in Zoo or Daycare. Dedicated feature assemblies become optional after shared partial declarations have been removed from the proposed boundary. Unity assemblies can constrain dependencies and reduce unnecessary recompilation; neither benefit follows merely from renaming folders. [Unity assembly organization](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definition-files.html).

Each work item should name its owned files and any shared integration file before editing. Common snapshot, session, navigation and composition files remain coordinated until extraction changes that fact. Different world folders cannot guarantee simultaneous chats will never need the same contract.

## Assets, source files and names

Editable artwork and audio are authored inputs even when the runtime never opens them. They belong in a traceable source collection such as `SourceArt/<World>/<Activity>` and the existing source-audio location. Imported runtime assets belong in the Unity project. External dependencies such as audio models and renderers are reusable tool inputs. None of those becomes trash merely because it is outside `Assets`.

Use world/activity names consistently across these collections. For Daycare, record the ownership map before any move:

| Current name/path family | Proposed owner label |
| --- | --- |
| `KingdomAdventure`, `Adventure` | Daycare / Story Adventure |
| `Sandpit` | Daycare / Sandcastle Club |
| `Treasure` | Daycare / Treasure Hunt |
| `Vet` | Daycare / Animal Clinic |
| Daycare hide/tag files | Daycare / Hide and Seek; Daycare / Tag |

These are organizational labels, not approved save-ID or API renames. Keep stable stored values unless a migration is separately justified. `SoloPrototype` is currently a real save-directory component; changing the visible class name does not authorize moving family saves.

Two reference systems need separate handling:

1. Unity `.meta` files hold identity/import metadata. Move each asset and its matching metadata together; preserve GUIDs. Losing identity can break scene, prefab or script references. [Unity asset metadata](https://docs.unity3d.com/6000.3/Documentation/Manual/AssetMetadata.html).
2. `Resources.Load` accepts a string path relative to a Resources folder. GUID preservation does not repair a changed string. Current examples include `BeachArt/seagull-poses`, `ZooArt/…`, `Vet/tools`, `Sandpit/…` and `Treasure/…`. Maintain an explicit old/new resource-key map and update each real caller. [Unity Resources.Load](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Resources.Load.html).

Use descriptive filenames and one documented convention for new assets. Epic's naming guide demonstrates a type/name/descriptor/variant scheme; its Unreal-specific prefixes are optional inspiration, not a Unity requirement. For this game, world/activity context and a useful descriptor should take priority over adding cryptic prefixes everywhere. Rename a connected set only after identifying its consumers. [Epic asset naming guidance](https://dev.epicgames.com/documentation/en-us/unreal-engine/recommended-asset-naming-conventions-in-unreal-engine-projects).

Unity warns that Resources can become costly as content grows. That is a reason to measure future loading problems, not proof that today's crashes or lag came from these folders. Addressables or another loading system would be a separate measured improvement, not a dependency of this cleanup. [Unity runtime resources guidance](https://docs.unity3d.com/6000.3/Documentation/Manual/LoadingResourcesatRuntime.html).

## Retention must follow purpose and consumers

Generated files have different lifetimes. Unity's asset database distinguishes authored assets from imported artifacts and regenerable Library data. Epic likewise treats derived data as disposable cache stored outside source control, with reuse intended to improve iteration. Rebuildability does not mean deletion is beneficial while a workspace is active. [Unity asset database contents](https://docs.unity3d.com/6000.3/Documentation/Manual/asset-database-contents.html), [Epic derived data cache](https://dev.epicgames.com/documentation/en-us/unreal-engine/using-derived-data-cache-in-unreal-engine).

| Category | Little Weeps examples | Proposed retirement rule |
| --- | --- | --- |
| Authored work and protected evidence | SourceArt, copied unique source, research, reviews, screenshots, implementation records | Preserve; reconcile duplicates explicitly. User review material is excluded from this cleanup. |
| Operational and recovery data | FamilyLAN, saves, enrollment, PCServer/current, backups, RecoveryJobs, signing records | Keep active selections and required recovery dependencies. Never treat these as general temporary files. |
| Current prepared/delivered artifacts | Signed mobile builds, live hotfix input, selected previous server bundle, current update manifests | Pin exact artifacts and provenance before considering other builds for retirement. |
| Useful generated caches | Active Unity Library, mobile build caches, current .NET outputs | Keep when actively used; retirement is a speed/storage tradeoff for closed tasks. |
| Completed intermediates | The 17 verified old transfer archives, unused download ZIP | Bounded cleanup after a fresh exact-path/consumer check. Retain their proven replacement and receipt where applicable. |
| Temporary task state | Staging indexes, snapshot copies, replay scripts | Confirm task closure, preserve unreconciled work and evidence, then retire only the recorded outputs. |

Deleting Library forces Unity to regenerate imports and shader work; a clean player build and clearing the entire asset cache are different operations. Preserve valid current caches to meet the parent's fast-update requirement. [Unity clean and incremental build guidance](https://docs.unity3d.com/6000.3/Documentation/Manual/build-clean-build.html).

Before retiring builds, derive a keep list from actual launcher/current-update/server/device/review consumers. The audit found that old Foundation, Solo, SharedGarden and CharacterWorkshop launchers still select older builds. Age, a prototype name and a large directory size do not override those references. Capture paths and purposes without copying private credentials into maintained docs.

For a temporary checkout, record purpose, actual source path, responsible chat/work item, open/finished state, active consumers, unique-work status and a retirement condition. Create a new checkout only when isolation is needed. This applies Riot's ownership/lifecycle lesson at family scale; it does not require Jira, cloud build farms or automatic background deletion.

## How the research changed the eight-phase plan

| Phase | Gap in the initial plan | Research-based revision now incorporated |
| --- | --- | --- |
| 1 Beach relocation | Could be mistaken for making Beach code independent. | Explicitly relocate a complete checkout, preserve all contents, record its owner/lifecycle and repair active consumers. Independence remains Phase 4. |
| 2 Main source location | A clean-looking folder transition could obscure dirty work and nested checkouts. | Select the obvious baseline only after preserving/reconciling root edits and editable sources; update each real consumer. |
| 3 Local data and cleanup | Broad categories did not provide an executable bounded retirement queue. | Add the 17-file allowlist, ZIP/empty-folder candidates, conditional indexes/outputs and four cache paths; add protected categories and consumer-based artifact retention. |
| 4 World code | Folder grouping could be confused with module separation. | Preserve six current assemblies; group beneath layers first; extract Beach responsibilities before considering feature assemblies. |
| 5 Names | A cosmetic rename could affect saves, serialization or build identity. | Require a connected rename map, compatibility handling and actual consumer checks; keep stored IDs stable by default. |
| 6 Assets | Runtime grouping alone could lose editable inputs or string resource keys. | Reconcile scattered source assets, retain GUIDs and update an explicit resource-key map; do one affected visual check. |
| 7 Tools/builds | Moving scripts changes root resolution; cleanup could destroy update speed or select stale output. | Repair portable root resolution, pin current artifact provenance, retain active caches and reuse an already verified identical current artifact. |
| 8 Future rules | A folder map alone would allow unexplained temporary copies to recur. | Add ownership/dependency declarations, new-folder purpose/lifetime rules, a checkout retirement record and a concise current entry page. |

The audited 5.13 GB of redundant transfer archives is the strongest storage finding. The 15.15 GB of four old snapshot Library caches is conditional on retirement. The roughly 90.99 GB NetworkProbe build inventory is not a deletion allowance. Exact cleanup items and protection requirements are in Phase 3 and the audit; these figures are logical bytes, not promised reclaimed space.

## Deliberate limits

No engine migration, per-minigame repository, permanent world branch hierarchy, enterprise CI platform, Addressables rollout or all-world rewrite is proposed. The public examples support explicit ownership, reproducible artifacts, stable boundaries and temporary-work retirement; they do not justify importing infrastructure that this family game does not need.

Mac folder organization remains uninspected. Manual uses outside the checked consumers may exist. Later implementation must refresh exact-path/usage checks for its own items; this research does not authorize deleting an item that has since become useful.

Next implementation remains Phase 1: relocate the Beach checkout into the game root's `LocalData/Worktrees/Beach`, with contents and active references preserved. This task only updates documentation.
