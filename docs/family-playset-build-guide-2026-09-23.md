# The Family Playset — ground-up build guide

**Production guide · September 23, 2026 · Foundation implementation in progress**

**Purpose:** build the game in a deliberate order, with a foundation that supports the complete Family Playset goal sheet. Start with small, testable systems; prove the difficult requirements; finish a representative playable area; then produce content in batches. A phase is complete because its acceptance checks passed, not because its scripts or pictures exist.

**Current position:** **G1 remains open; provisional G2 Windows-only work is authorized while physical devices are unavailable.** The garden prototype now has movement, draggable interacting toys, avatar switching, optional spoken prompts and local save recovery. `Play-SoloPrototype.cmd` opens verified Windows 43; `Play-Foundation.cmd` keeps the separate G1 video fixture at 22. An isolated Android emulator passed restart and 30 → 45 update retention; static 16 KB findings remain open. The primary iPads have not run this garden yet. Older-iPad qualification, native ARM64 16 KB, independent backup and renewal remain open; multiplayer is not implemented. See [G2 results and next task](implementation/g2-status.md), [G1 status](implementation/g1-status.md) and the [iPad evidence](implementation/ipad-update-2026-09-23.md).

[Open the feature goal sheet](bluey-game-research-2026-09-23.html) · [Package research](family-playset-package-research-2026-09-23.html) · [Technical evidence](family-playset-technical-research-2026-09-23.html) · [Feasibility audit](family-playset-feasibility-audit-2026-09-23.html)

## 1. Two documents with different jobs

| Document | Its job | When it changes |
| --- | --- | --- |
| **The Family Playset research — goal sheet** | What the finished game should do: characters, worlds, activities, controls and family-play behavior | When the user adds, clarifies or changes a requirement |
| **This build guide** | How we organize and implement those requirements: dependencies, architecture, phase order, tests and completion evidence | When implementation evidence improves the method or the user changes priorities |
| Supporting research reports | Why a technical choice is plausible, its sources, package candidates and unresolved limitations | When new evidence resolves an uncertainty or changes a candidate |
| Work record inside this guide / later implementation records | What actually exists, what passed, the current phase and next bounded task | At the end of each meaningful work session |

The user's latest instructions take priority. The goal sheet controls product scope; this guide controls the default build sequence. Earlier milestone lists in the research are background to this consolidated sequence. A later phase means **not yet**, not **removed**. If a required feature fails its proof, record the failure and revisit the design; do not quietly weaken the requirement.

Keep the current feature IDs from [goal-sheet section 16](bluey-game-research-2026-09-23.html#16-expanded-feature-tracker). Each implementation task names its goal IDs and acceptance evidence. General requirements without existing IDs—such as build setup, input, menu accessibility and installation—are explicitly covered by this guide too.

## 2. What professional teams do, and how we apply it

There is no single process used by every company. This guide adapts published production guidance and a commercial Unity studio's account to a small family project.

Unity describes pre-production, production, post-production and ongoing operations; its pre-production guidance emphasizes establishing the design and resolving decisions before large-scale creation. Unity's USC course uses milestones and a **vertical slice**, a small playable portion that represents the intended final experience. Our technical proofs precede the polished slice because the hardest requirements are shared state and mobile recovery. [Unity production cycle](https://learn.unity.com/course/job-seeker-s-guide-to-learning-unity/tutorial/the-real-time-production-cycle), [Unity/USC milestones](https://learn.unity.com/course/design-and-publish-your-original-game-unity-usc-games-unlocked/unit/milestones)

In its published Unity workflow, **Mega Cat Studios** describes focused components, explicit dependencies, smaller additive scenes, reusable prefabs and asset validation. This is useful evidence from a working studio, not proof that its exact architecture fits our game. We adopt the separation of responsibilities and adapt it for independent rooms, older iPads and a headless PC server. [Mega Cat Studios workflow account](https://unity.com/blog/scaling-workflows-lessons-from-medium-to-large-projects)

| Common production discipline | Our concrete implementation |
| --- | --- |
| Maintain a design document | Keep the existing Family Playset as the goal sheet |
| Prototype the expensive uncertainties | Prove physical-device builds, saves, four-device play, independent rooms and iPad host recovery early |
| Establish a repeatable production pipeline | One asset format, stable content IDs, reproducible builds, versioned saves and content validation |
| Finish a representative vertical slice | A small home/backyard experience with polished touch, speech, toys and interruption recovery |
| Produce in batches | Expand reusable activity families into the six worlds and requested content |
| Review quality continuously | Automated rule tests, real-device checks and short playtests with each child throughout development |
| Maintain deployed software | Preserve signing identities, saves, media, diagnostics and a tested update route |

These disciplines do not require a commercial launch. Our delivery target is the family's devices. Responsibilities still exist: the user directs the experience and evaluates it with the children; implementation work covers engineering, asset integration, testing and documentation. The guide does not assume a staffed studio or paid cloud services.

## 3. Foundation requirements we must preserve

The [goal sheet](bluey-game-research-2026-09-23.html#1-your-requirements-now-recorded) remains authoritative. These requirements have architectural consequences from the first prototype:

- **One game, three execution modes:** offline solo, an iPad hosting, or the Windows PC serving the shared world. Reuse the same world rules in all three.
- **One to four mixed-device players:** iPad, iPhone and Android; a dedicated PC consumes no human slot. Both iPads must be capable hosts when needed.
- **Automatic ordinary-network joining and recovery:** one-time parent setup, then no child Host/Join workflow. The PC is preferred when starting a session; an established healthy host remains stable.
- **Independent places and activities:** entering the playground finds its current occupants and objects; nobody else's travel unloads or pauses that world.
- **Interactive shared items:** one connected-world holder per instance, consistent contents, safe carrying and return policies, and protected creations.
- **Complete solo capability without internet:** local art, reviewed speech and installed media; suitable NPC substitutes; save/reunion behavior designed in from the start.
- **Each child's identity survives avatar changes:** room ownership, settings, activity role and video bookmarks do not belong to whichever character skin is selected.
- **Ages 3 and 6:** speech and picture instructions, two assistance styles, joystick or tap walking, dragging in either mode, and unrestricted exit from activities.
- **Six destinations:** Heeler Home, Backyard Garden, Creek, Playground & Park, Beach and Daycare. Bedrooms, secret rooms and story spaces are destinations within that world model.
- **Travel connectivity is optional later; travel solo is required.** Bluetooth is removed. English comes first; the data and UI must support Spanish, with reviewed content added in its planned batch.

Automatic recovery cannot promise an invisible transition when a hosting device disappears. Disconnected devices cannot enforce one live holder between them. Those known limits require preserved checkpoints, explicit branches and reunion rules; they do not justify deleting the hosting requirement. See the [source-backed recovery review](family-playset-technical-research-2026-09-23.html).

## 4. Project home, folders and source control

**Only this game's home:** `C:\Users\sephi\Desktop\Little weeps game`. The existing `Meeps game` project and its assets remain unrelated. The desktop Connect Unity + Blender shortcut now points to this game's separate launcher; the old project's script is unchanged. Verify the editor's actual project path before using a connection.

The following is the **proposed structure to create during implementation**, not a claim these folders already exist:

```text
Little weeps game/
  AGENTS.md
  README.md
  docs/                         Goal sheet, this guide, research, decisions
    implementation/             Current work, test evidence, build records
  Unity/FamilyPlayset/
    Assets/
      FamilyPlayset/
        Code/
          Core/                 Identities, world rules, commands and records
          Runtime/              Session orchestration and service interfaces
          Adapters/             Saves, networking and platform services
          Client/               Input, UI, cameras, animation, audio and video
          Server/               Dedicated-server startup and configuration
          Editor/               Build scripts and content validators
        Content/                Characters, items, recipes, activities, locales
        Scenes/                 Bootstrap, shell and small area views
        Art/                    Game-ready sprites and animations
        Audio/                  Game-ready speech, music and effects
        UI/                     Shared prefabs, icons and layouts
        Tests/                  Focused rule and integration tests
      ThirdParty/               Clearly separated vendor assets
      BuildProfiles/            Checked-in platform/profile assets
    Packages/
    ProjectSettings/
  SourceArt/                    Editable illustration/animation sources
  SourceAudio/                  Scripts and approved source recordings
  Tools/                        Build, validation and local workflow helpers
  Builds/                       Generated builds; excluded from source history
  LocalData/                    Local test worlds/media; excluded from source history
```

Use **Git** with a known-good main branch and small `codex/…` work branches. Keep one bounded change per branch; review before merging. Use a local private repository first; choose a backup/remote destination deliberately rather than publishing assets automatically. A version-control repository and an independent recoverable backup serve different purposes.

Commit `Assets` with their `.meta` files, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings`, build scripts and documentation. Use visible metadata and text serialization for suitable Unity assets. Do not commit `Library`, `Temp`, build output, editor logs, credentials, signing keys or personal imported video libraries. Move assets through Unity, or move their matching metadata too. Unity uses metadata IDs to maintain asset references; losing them can break those references. [Unity asset metadata](https://docs.unity3d.com/6000.3/Documentation/Manual/AssetMetadata.html), [Project organization guidance](https://unity.com/how-to/organizing-your-project)

Choose Git LFS patterns for large binary art/audio before a substantial asset import, and verify that the chosen backup/remote stores the actual LFS objects as well as pointers. Keep small text assets in ordinary Git. Do not synchronize a live Unity project by having two editors share a cloud-synced Library folder. [Git LFS](https://git-lfs.com/), [Unity project directory guidance](https://docs.unity.com/en-us/engine/6000.0/manual/get-started/project-configuration/default-directories)

Use stable names, consistent case across Windows/Mac, import presets, and one ownership location for each content definition. Do not create alternative copies named final2/final-new to resolve confusion; source history and explicit versions should explain the change.

## 5. Architecture before feature expansion

**Proposed design:** a modular Unity application with a compact, serializable world model. Keep the number of modules modest; introduce a boundary when it protects a real responsibility, not an interface for every method.

```text
Touch / activity UI
        |
        v
Command request ------> local authority OR network adapter to authority
                                |
                         validate world rules
                                |
                         accepted state change
                          /       |         \
                         v        v          v
                    client view  save     peer updates + recovery state
                    and audio    records

Offline solo, iPad host and PC server use the same world-rule code.
```

| Module | Owns | Must not become responsible for |
| --- | --- | --- |
| Core | Stable IDs, object state, inventory/containers, commands, invariants, activity facts | Unity scenes, video playback, editor APIs, network SDK objects |
| Runtime | Session state machine, activity lifecycle, world services, cancellation and logical clocks | Drawing the screen or platform-specific permission UI |
| Adapters | Persistence, transport, service discovery, native file import and lifecycle notifications | A second implementation of cooking, pickup or quest rules |
| Client | Touch arbitration, character presentation, camera, menus, narration, book/video player | Declaring a shared action successful without authority validation |
| Server | PC bootstrap, configuration, durable world storage and logical simulation | A requirement to render sprites, play sound or finish an animation |
| Editor/tools | Content authoring checks, import conventions, builds and diagnostics | Runtime dependencies on UnityEditor |

Use assembly definitions to enforce useful dependency directions and isolate editor/platform code. Start with these broad responsibilities; split further only for a demonstrated dependency or build-time benefit. The core should be testable without Unity rendering or the network SDK. Unity documents that assembly definitions restrict dependencies and can isolate code compilation. [Assembly definitions](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-intro.html)

**Scene model:** a persistent bootstrap/session root plus local area views loaded as needed. The authority retains logical state for every occupied area, including areas its own camera cannot see. Logical bounds, hiding spots, seats and interaction anchors cannot depend on reading pixels or a loaded render texture. A global scene replacement must not transport all players together.

**Content model:** ScriptableObjects can author character/item/recipe definitions; a separate runtime model stores the current bucket fill, child's creation or quest state. Do not use a mutable shared definition asset as the save for two children. Unity's player does not save edits back into those authored assets as a persistence system. [ScriptableObject data behavior](https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html)

**No multiplayer retrofit:** even the solo bucket prototype sends an action through the same rule boundary that a remote player will use. Presentation may immediately highlight or animate a tentative action, but durable results come from accepted world state. We are not relying on identical 2D physics results on every platform.

## 6. Data ownership and contracts to settle early

These are proposed implementation contracts, drawn from the existing shared-world and recovery requirements. They should receive small tests before large scenes exist.

| Data | Owner and persistence rule |
| --- | --- |
| Child profile, assistance, preferred avatar | Stable profile identity; avatar swaps do not create a new person |
| Local UI/audio settings and TV bookmarks | Per-device/profile storage; separate from the communal world; a bookmark also identifies the exact clip content |
| Ordinary room, object instance and current holder | Current world authority; stable room/object IDs; one canonical connected-world instance |
| Bedrooms and protected creations | World state with explicit creator/room ownership and a recoverable saved representation |
| Shared-loan returns and station stock | Authority-managed policy by item category; returns cannot erase protected contents or active work |
| Quest and story participation | Activity/session identity plus each participant's role; one departure does not reset remaining participants |
| Recovery state | Compact full-world checkpoint/journal independent of per-client visual visibility |
| Imported movies and voice/art files | Local content files; not ordinary gameplay network packets |

Define separate versions for **application build**, **network protocol**, **content catalog**, and **save schema**. A later app update might change one without changing all four. Reject an incompatible connection gracefully while retaining solo play; never interpret an unknown save schema as a blank world.

Commands carry a request ID, player identity, relevant object/room identity and expected state/revision. The authority orders accepted actions and rejects stale ownership. Repeated delivery must not pour twice, duplicate a fish or complete the same return twice. Movement/drag visual updates can be coalesced; final placements and creations require explicit accepted results.

Saving distinguishes state accepted in memory, state replicated to a successor, and state successfully persisted. Do not describe an in-memory acknowledgement as crash-proof. Use versioned records, validated checkpoints, a last-known-good backup and an explicit migration path; keep a small journal where needed for recovery. Checkpoint frequency and durability boundaries are prototype decisions to measure.

An interrupted write must preserve the previous valid save. A failed migration must preserve the original data. Fixture saves from earlier versions belong in the tests. Avoid storing Unity instance IDs or network-spawn IDs as permanent identity. A standard serializer does not supply these policies for us.

Disconnected play creates identified branches. On reunion, validate operations and preserve conflicting creations explicitly; do not silently create two normal playgrounds with the same identity. Do not select the winner using an untrusted device clock alone. The [technical report](family-playset-technical-research-2026-09-23.html#7-shared-toys-automatic-returns-and-save-conflicts) records the supporting reasoning and limitations.

## 7. Android, iOS and PC build pipeline

**Working baseline:** Unity 6000.3.24f1 remains pinned. NGO 2.13.2 and Unity Transport 2.7.4 are now installed and locked for the isolated Windows network probe. Native server/four-client tests passed on build 48, and the existing Windows garden passed regression on 49. [Exact package decision and limits](implementation/g3-network-probe-2026-09-24.md). The [package report](family-playset-package-research-2026-09-23.html) records the earlier NGO 2.13.2/2.13.3 status difference; these new Windows results do not certify mobile networking or replace actual-device qualification.

Keep everyday creation on **Windows**. Build Android and the Windows server there. Our verified export route also generates the iOS Xcode project on Windows and transfers that export to the **M1 Pro Mac** for native compilation/signing. Unity does not need to be installed on the Mac for this route; if the Mac later exports directly from source, use the identical Unity patch. Record source revision, export hash and native build result together. [Unity iOS build process](https://docs.unity3d.com/6000.3/Documentation/Manual/iphone-BuildProcess.html)

| Build profile to establish | Purpose | Initial output / check |
| --- | --- | --- |
| Windows development client | Fast authoring and interaction checks | Playable client; not mobile qualification |
| Android development | Profiling and native integration on the Samsung | Diagnostic build with clear identity |
| Android family release | Actual personal-use game | Signed release APK, tested as an in-place update |
| iOS development | Profiling and native integration | Xcode device build on the real iPads |
| iOS family release configuration | Actual personal-use game with agreed signing route | Release-configured app using available development signing; test without debugger |
| Windows Dedicated Server | Persistent home-world authority | Headless server with local config, saves and logs |

Unity Build Profiles are versionable assets for different platform/configuration combinations. Save these settings instead of relying on whatever someone last selected in the editor. Command-line builds must name the project and intended build profile/target explicitly. The first automation can be local scripts on Windows and Mac; a paid cloud build service is not required by this plan. [Build Profiles](https://docs.unity3d.com/6000.3/Documentation/Manual/build-profiles.html), [Command-line builds](https://docs.unity3d.com/6000.3/Documentation/Manual/build-command-line.html)

Freeze the editor version, resolved package lockfile, Android toolchain supplied for that editor, Mac/Xcode versions, scripting backend, architectures and graphics settings after qualification. Commit the manifest and lockfile; package upgrades are deliberate changes with affected-device retesting. [Unity package lock files](https://docs.unity3d.com/6000.3/Documentation/Manual/upm-conflicts-auto.html)

Each artifact record includes source commit, build number, platform/profile, toolchain versions, protocol/content/save versions, checksum, logs, native symbols and passed tests. Windows and Mac produce platform-specific artifacts from the same source revision; their output files are not expected to be identical.

Keep app identifiers and signing identities stable for updates. Store signing material outside the repository with a recoverable backup. Do not use another project's Android key/app identity. For personal Android installation, the current developer-verification FAQ preserves ADB development installation; qualify that route on the Samsung rather than assuming a store account is needed. [Android signing](https://developer.android.com/studio/publish/app-signing), [Android ADB installation guidance](https://developer.android.com/developer-verification/guides/faq)

Start verifying the agreed free iPad renewal process in G1 and let its real renewal-cycle observation run alongside later development. It must pass before unattended family deployment. Automatic refresh reduces manual reinstall work; it does not make free provisioning permanent. The existing [installation audit](family-playset-feasibility-audit-2026-09-23.html#3-the-limits-that-must-remain-honest) remains applicable.

## 8. Ordered phases and completion gates

**Default dependency chain:** G0 → G1 → G2 → G3 → G4 → G5 → G6 → G7 → G8 → G9. Each phase may contain several small work sessions. Begin the next dependent phase when the current exit checks pass. A long-running signing-renewal observation may overlap, but its unresolved result remains recorded.

| Gate | Main deliverable | Complete only when | Hold back until it passes |
| --- | --- | --- | --- |
| **G0 — production plan** | Goal sheet, this guide, risk/coverage records | Scope and sequence are recorded, documents link correctly, implementation status is honest | Implementation not yet started in this research task |
| **G1 — project and devices** | Isolated project, build profiles, tiny native scene, save and update smoke test | Both iPads and Android launch; Mac build route works; a release update retains a seeded save; exact toolchain recorded; renewal check started | Large asset imports, final rooms and mini-game production |
| **G2 — reusable solo rules** | Touch router, two placeholder avatars, bucket/tap/plant/sponge, versioned state | Both movement choices and drag ownership work; free exit and avatar switch preserve state; offline cold launch and interrupted-save recovery pass | Multiple bespoke activity implementations |
| **G3 — PC shared world** | PC server, automatic paired joining, two basic areas, four mixed clients | Four humans on both iPads + Android + iPhone; independent travel; late join; single-holder races; client departure leaves others playing | Treating a two-client one-room demo as complete multiplayer |
| **G4 — required iPad hosting** | Automatic mobile authority selection, handoff and abrupt-loss recovery | Each iPad hosts with PC absent; simultaneous launch and returning host converge; recovery restores both areas; hosting while opening media keeps peers active | Broad content production if the A10/host architecture remains unproven |
| **G5 — durable family world** | Separate bedrooms, saved creation, offline branch/reunion and item-return rules | Disjoint/conflicting edits survive; duplicate commands do not duplicate props; unused loans return without losing creations; stock stays usable | Scaling furniture/recipes before persistence and ownership are reliable |
| **G6 — polished vertical slice** | A representative home/backyard experience with finished controls, art and voice | Both children can independently play; integrated media/NPC/activity tests pass; sustained A10 client and host performance is acceptable | Batch-producing every character and world |
| **G7 — content production** | Ordered batches expanding the proven slice to the goal sheet | Each batch has complete assets, narration, data, exit/save/co-op behavior and tests; unbuilt items remain visibly pending | Starting all worlds/stories at once or claiming the full goal sheet is done |
| **G8 — family release** | Versioned, signed, tested installation set and recovery instructions | Target devices pass release checks, refresh/update preserve data, backups restore, server startup works; known issues recorded | Giving an unqualified debug build to the children as the finished release |
| **G9 — maintenance and later options** | Small updates with preserved saves and measured fixes | Each update passes its affected checks and can be traced to a source/artifact record | Unrelated refactors inside a bug fix; optional travel work displacing a required failure |

### G1: prove the build and update route

Inventory the real Android OS, Mac macOS/Xcode, free storage and current Unity installations. Create the new project in the proposed subfolder. Verify any editor connector targets it. Start with plain shapes, a large touch button, an obvious build number and a tiny persisted value. Establish an initial character-independent profile ID and separate development/family data paths if identities differ.

Build and launch on both iPads and Android; also establish the iPhone as the fourth test client before G3 can pass. Make a second release-configured build that visibly changes the build number while preserving the test save. Record the signing identity and installation route. A simulator or editor launch does not satisfy the device check.

### G2: prove feel and the rule boundary

Use the same command path for grabbing, filling, pouring, washing and placing. Test one finger on a joystick and another on a toy, interrupted drags, accidental double taps, opening a menu mid-action and changing character while holding something. Add one optional spoken picture prompt after free object play works. Decide the room's walkable plane and depth sorting with this example before creating many backgrounds.

### G3–G5: prove continuity while the world is small

Keep the test world small enough to inspect. Add a PC authority and native discovery/permission adapters. Then add independent areas, four clients, proper snapshots and object exclusivity. Parent pairing establishes trust; advertisements alone do not authorize world changes.

Next test each iPad as host, planned handoff, hard termination, network partition and host return. Keep the full compact recovery state separate from visible objects. G5 adds actual conflicting edits, protected creations, containers and automatic loan returns. Existing [networking evidence](family-playset-technical-research-2026-09-23.html) guides the design, but these gates require new runtime proof.

### G6: the representative finished slice

Include one home area and the garden, two finished character rigs plus a representative parent-size rig, switching/holding/seating anchors, one complete cooking recipe, one cleanup interaction, pond fishing, one parent-led hiding round and the shared activity invitation UI. Add a small narrated book, one interactive dinosaur, one science example, a local-video library with resumable playback, a bedroom and a secret-room sample. Existing technical test fixtures already cover two owned bedrooms; the slice adds their usable presentation.

These small examples exercise the content pipeline and cross-system interactions. They do not fulfill the full roster, all 15 recipes or nine imagination stories. Demonstrate a role-based activity and NPC substitution before scaling daycare stories. G6 may have sub-batches, but its cross-system acceptance is one gate.

### G7–G9: expand, release and maintain

Use the batch order in section 12. A small private playable may be shared when its applicable release checks pass; label its incomplete content honestly. “Full goal sheet complete” is reserved for the implemented required scope, not the first family alpha. Estimate later batches from the measured time to create and test the slice, rather than inventing calendar dates now.

## 9. First implementation work queue

**G1 has started.** This remains the ordered work queue; per-task progress and evidence are recorded in the [live G1 record](implementation/g1-status.md). Complete one bounded task at a time. Build/signing records must contain actual paths and versions; never present untested commands as an already-working workflow.

| Order | Task | Concrete result |
| --- | --- | --- |
| 1 | Verify this project's boundary and device/tool inventory | Exact new project path; installed editor, Android OS, macOS/Xcode and storage recorded |
| 2 | Establish source control and backup conventions | Correct repository root, ignores, large-file policy, protected signing storage and recoverable baseline |
| 3 | Create Universal 2D and qualify initial packages | Same editor patch on Windows/Mac; small manifest/lockfile; no unrelated old assets |
| 4 | Set identities and build profiles | Stable app identifiers, build-number scheme and platform profiles |
| 5 | Create bootstrap, core/runtime separation and one test scene | Large touch target, visible build ID, persistent sample value; no full scene catalog |
| 6 | Make Android and native iPad builds | Each target launches; a tiny local video sample is checked before media-heavy design |
| 7 | Test an in-place release update | New build visibly present; prior sample data remains; no uninstall/reset fallback |
| 8 | Start free iPad signing-renewal observation | Actual refresh route documented; result remains pending until a real cycle preserves launch/data |
| 9 | Establish repeatable build/test entry points | Explicit project/profile, fresh artifact identity, logs and meaningful failure status |
| 10 | Record G1 exit evidence and advance the work record | Known-good revision, device results and next G2 task; unresolved blockers remain visible |

## 10. The working method that prevents jumping around

Use a lightweight board: **Backlog → Ready → In progress → Verification → Done**, plus **Blocked** with the exact missing input or failing check. Keep one main implementation task in progress. A small independent task may proceed during a build/long observation only if it does not bypass a dependency or mix unrelated changes.

### Before coding: ready means specific

A task is ready when it names the goal-sheet requirement, its phase, a concrete user-visible result, the existing system it extends, and a testable completion condition. If it changes shared state, decide what happens on duplicate input, cancellation, departure and restoration before writing the happy path.

```text
Task: G2-004 — Fill and pour the bucket
Goal references: object rules; ITEM-02; later ITEM-03
Depends on: stable object IDs, touch router, command boundary
Result: a child fills a bucket and waters a plant in solo play
Owns: container rule + its presentation adapter
Does not yet finish: all recipes, shared-world recovery, full garden art
Acceptance: quantities remain valid; interrupted drag is safe;
            reload restores the accepted result; prompt is optional
Evidence: source revision, rule test result, device/build and short observation
Status: pending
```

The same feature later receives G3–G5 network/recovery coverage. This prevents a solo-only result being marked fully accepted for multiplayer.

### While working: follow the dependency

Read the active phase and relevant goal-sheet sections. Inspect existing code before creating a new manager or abstraction. Implement the smallest end-to-end behavior, including its appropriate save and failure path. Keep vendor packages separate and change them through a documented wrapper or patch if necessary. Defer unrelated cleanup to a named backlog item.

New ideas go into the goal sheet/backlog first; then map them to the earliest phase with the necessary foundation. A bug that breaks the current gate takes priority. If investigation reveals a design problem, record its cause and affected gate rather than hiding it behind a workaround in every mini-game.

### Done means evidence

1. The requested behavior works in its named scope; placeholders and unsupported cases are explicit.
2. Relevant invariants, cancellation, persistence and multiplayer cases pass at this phase.
3. The affected platform build succeeds. Device-sensitive changes are checked on the affected physical devices.
4. Code/content is reviewed, stable IDs and references are valid, and no unrelated project changes are included.
5. Goal IDs, build/revision, tests, remaining limitations and the next task are recorded.

Cosmetic reversible edits do not need elaborate new unit tests. Ownership, save migration and recovery need meaningful tests that can catch a real fault. Passing tests is not a reason to keep repeating the same checks without a new change or concern.

At each handoff, record **current phase, completed task, exact revision/artifacts, checks passed, open issue, and next bounded task**. Mark a phase complete only when its exit criteria have evidence. There is no routine approval ceremony between reversible implementation steps; obtain a user decision when scope, cost, irreversible data handling or another genuine authorization boundary changes.

## 11. Art, audio and content production pipeline

Complete one example of each difficult asset type before producing dozens. For this flat illustrated style, layered 2D art is primary; Blender remains available for a specific useful task rather than becoming a required 3D pipeline.

### Interaction standards shared by every area

Use consistent large icons, repeatable spoken hints, visual feedback and an obvious exit. Each touch belongs to one UI control, walk command or draggable prop until release/cancellation. Opening a book, TV or settings panel must not set the entire shared world's time scale to zero. Language, movement mode and assistance settings remain local to the child.

Apple's game-design guidance recommends aiming for 44-by-44-point touch targets on iPhone/iPad. Treat that as general platform guidance, not a preschool usability result or a literal Unity-pixel value. Use larger, well-spaced critical controls and check their physical size and reach on the actual devices with both children. Phone layouts need intentional placement, not simply a shrunk tablet screen. [Apple game touch-design guidance](https://developer.apple.com/videos/play/wwdc2024/10085/)

### Asset standards

| Asset type | Authoring standard to establish | Acceptance before reuse |
| --- | --- | --- |
| Character | Consistent scale, pivot, facing, draw layers, idle/walk/use/hold poses and hand/seat/hiding anchors | Short and tall representative rigs work with shared controls, props and furniture |
| Room | Drawn perspective plus an explicit walkable plane, depth order, portals and interaction anchors | Characters pass correctly behind/in front of furniture; travel does not unload shared logical state |
| Prop | Stable ID, capabilities, collider/hit area, container/attachment rules, home/return category and state illustrations | Works from each input mode and cannot become duplicated, stranded or permanently invisible |
| Activity | Definition, invitation, goals, optional hints, roles/NPC substitutes and leave/resume policy | It remains enjoyable in free play and survives participants joining/leaving |
| Speech | Stable line ID, English script, speaker, pronunciation review and linked visual cue | Understandable on device; speech cancellation and priority work; a missing line has a safe fallback |
| Book | Page data, narrated lines, hotspots and small animations | Reading can pause/leave/reopen without overlapping voices or affecting another player |
| Video | Stable clip identity/hash, thumbnail, supported local encoding and bookmark schema | Controls, offline playback, seek and relaunch resume work on actual devices |
| Localization | Locale tables and translated audio references | UI accommodates text/audio differences; Spanish receives its own content review |

Generate or record voices outside the running game, then review and import them. A live LLM or ComfyUI server is not a dependency of ordinary play. Check dinosaur names and educational content before recording a large batch. Retain scripts and source audio so corrections do not require reverse engineering finished clips.

Use shared prefabs and data definitions for reusable stations. A new pizza should mostly be a new recipe/content entry after cooking rules exist. Add custom code only for genuinely different behavior. Validate missing IDs, duplicate IDs, invalid references, missing English narration, oversized textures, missing exit paths and unclassified return policies before a content batch can ship.

Addressables groups should separate always-needed UI, area art, character content and language/media sets. Gameplay required offline must be present locally. Each load has an owner and a matching release; cancelling a transition must not leak its handles or instantiate a late result into the wrong room. Unity's Addressables documentation describes reference counting and explicit releases; unloading can also depend on bundle references, so measure actual memory recovery. [Addressables memory management](https://docs.unity3d.com/Packages/com.unity.addressables@2.10/manual/memory-assets.html)

## 12. Content batch order after the foundations pass

These batches preserve the complete goal sheet while avoiding simultaneous unfinished worlds. Finish a batch's playable behavior and checks before opening another large content batch. Limited shared-asset preparation can run alongside it.

| Batch | Content | Reused systems and completion evidence |
| --- | --- | --- |
| **G7-A — complete home and garden** | Requested kitchen variants, cleanup, fishpond, science, dinosaurs, books, TV, both personal rooms and secret rooms | Build from G6 examples; all **five pizzas, five cakes and five meals** work as complete interactions; room changes and item returns preserve creations |
| **G7-B — outdoor worlds** | Beach, Creek, Playground & Park; usable equipment, collection, fishing, building, tag and hiding | Reuse collection/container/seat/construction rules; independent player travel and enough usable stations for four |
| **G7-C — daycare and learning** | Teacher routines, classmates, optional day with 2–3 persisted activity invitations, reading/math/music/science stations | Invitations do not block free play; late join does not reroll another child's day; NPC schedules release resources safely |
| **G7-D — imagination stories** | Calypso, Helicopter, Wild Girls, Early Baby, Typewriter, Mums and Dads, The Adventure, Space and Explorers | Complete one story first, including role/NPC substitution and exit/resume; then build the other eight from the proven pattern |
| **G7-E — roster and activity catalog completion** | Remaining requested child characters, parent choices and selected show-derived activities | Extend rig families; switching never changes identity; duplicate favorites work; production tracker distinguishes requested scope from proposed extras |
| **G7-F — Spanish content pass** | Reviewed Spanish lines, narration, labels and suitable learning variants | English family builds can arrive first; language switching remains local to each player; translated educational material is reviewed separately |

Necessary characters enter earlier batches as needed; G7-E closes the remaining roster gaps rather than withholding character choice until the end. The six-world wheel remains extensible from G2, but playable builds should not present broken entry buttons to content that does not exist yet.

Counts such as six dinosaur books, 20 dinosaur types and eight science stations are proposed production batches in the goal sheet. Track their approved scope explicitly. The 15 cooking variants and nine named imagination stories are concrete requested content. Do not inflate progress by counting the same fishing system in two places as two finished engines.

## 13. Test strategy and the evidence required

Use a small test pyramid adapted to this game's risks: many fast state-rule checks, fewer system integration checks, and targeted real-device sessions. Unity's Test Framework supports Edit/Play Mode tests; use the core version tied to the selected editor, not a separately guessed version from an old tutorial. [Unity 6.3 Test Framework](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.test-framework.html)

| Test layer | Examples that matter | Evidence |
| --- | --- | --- |
| Core rules | Two requests cannot own the same bucket; repeated pour request is harmless; protected cake survives a plate return | Focused automated results with meaningful assertions |
| Persistence | Partial write, corrupted latest save, old schema, missing content ID, journal replay and conflicting offline creations | Saved fixtures, migrated output and recovery outcomes |
| Unity integration | Touch/joystick arbitration, visual rebinding, load cancellation, quest event subscription and avatar swap | Play Mode/native scene checks |
| Network integration | Four players, two or more zones, snapshots, stale messages, partition/rejoin and returning host | Per-device session logs linked to the same run |
| Physical lifecycle | Suspend/resume, lock, hard-close, permissions, local discovery, iPad host loss and video decoder behavior | Exact device/OS/build and expected/actual result |
| Child usability | Child can choose, move, interact, hear instructions, leave and return without reading | Short observed session; record confusion and adjust design |
| Release/update | Signed release, debugger detached, seeded saves/media/bookmarks retained across update/renewal | Installed version, visible content/data and matching artifact record |

Apple specifically recommends testing release builds, actual devices, app updates and operation without the debugger; debugger attachment changes lifecycle/watchdog behavior. Our family signing route may differ from a store release, but those testing principles still apply. [Apple release testing](https://developer.apple.com/documentation/xcode/testing-a-release-build)

Android's core quality guidance covers lifecycle, media, stability and responsiveness. Apply relevant checks to this game; its store/account requirements are not being imported as product features. In particular, exercise lock/unlock, app switching, interrupted audio/video, layout changes and restoration. [Android core quality](https://developer.android.com/docs/quality-guidelines/core-app-quality)

### Required device matrix

| Device | Required role coverage |
| --- | --- |
| A2197 iPad 7 / reported iPadOS 18.6.2 | Solo, PC client, mobile host, host recovery, media while hosting and lowest-performance acceptance |
| A2602 iPad 9 / reported iPadOS 18.7.10 | Same gameplay and both client/host roles; repeat handoffs in both directions |
| SM-S948U1 Android phone | Solo, cross-platform client, native discovery, phone UI, background/resume and in-place update; installed OS to record in G1 |
| A2484 iPhone / reported iOS 26.6.1 | Fourth simultaneous human, phone layout, joining/leaving and newer-OS compatibility |
| Windows PC | Headless startup, saves, restart, all four clients and failure/recovery scenarios |

The four-human test uses all four mobile devices. The PC server is additional. Android/iPhone authority roles can be evaluated later if desired; the mandatory mobile-host proof is specifically both iPads. Editor clones accelerate iteration but cannot replace this matrix.

An acceptance record should contain test ID, goal ID, source/build versions, device/OS, starting save, steps, expected/actual result, result status and attached logs where useful. A short video may illustrate the result, but the test record still needs reproducible steps.

## 14. Performance, interruptions and diagnostics

Use the A10 iPad as the minimum performance target from G1. Keep the existing **30 FPS target** as a proposed game acceptance target and measure frame-time spikes as well as averages. A 30 FPS frame budget is about 33.3 ms; it is not permission to block touch handling for an entire loading operation. Phone/tablet layouts and graphics quality can differ while rules and content identities remain the same.

Profile three distinct situations: four people together, four people in separate areas while A10 hosts, and A10 hosting while a local book/video is open. Repeat room and media changes to expose retained references. Existing memory, prop and NPC numbers in the goal sheet remain hypotheses until these measurements establish practical budgets. [Unity Profiler](https://docs.unity3d.com/6000.3/Documentation/Manual/Profiler.html)

Use lightweight logic for offscreen NPCs/areas. Do not run their full visual animation/particles just because a guest is there; also do not suspend the guest's activity. Pool high-frequency effects where profiling warrants it, cap repeated interactions and decode one video per device. Avoid using an animation callback as the only timer for cooking or cleanup: a headless server has no need for its renderer/audio. [Dedicated Server optimizations](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-optimizations.html)

Every asynchronous operation needs an owner and cancellation/generation check: scene load, narration, video prepare/seek, connection attempt and save write. A callback from an abandoned room must not teleport a character, start old audio or overwrite a newer save.

Keep local structured diagnostics: build/protocol versions, authority generation, zone/activity IDs, request ID, state revision, load failure and save result. Retain native symbols for the exact release. Apple jetsam reports help distinguish memory-pressure termination from ordinary crashes; Android Logcat helps trace platform errors. Investigate freezes as well as crashes, including Android ANRs. [Apple jetsam reports](https://developer.apple.com/documentation/xcode/identifying-high-memory-use-with-jetsam-event-reports), [Android Logcat](https://developer.android.com/studio/debug/logcat), [Android ANRs](https://developer.android.com/topic/performance/issues/anr)

The TV's bookmarks get their own test path: save on meaningful transitions and approximately every five seconds during playback, as proposed in [goal-sheet section 27](bluey-game-research-2026-09-23.html#27-tv-corner-and-local-video-library). A crash restores the last successful checkpoint. Bookmark writes must not depend on the PC being online or reset when the child changes character.

## 15. Family delivery, updates and recovery

G8 uses the same approved source revision and platform profiles whose evidence was recorded. Install the intended fresh artifact, verify its version, launch it and inspect the seeded or existing data. A successful installer message alone does not establish that the right build or saved world is present.

Use disposable test data or a separate development identity for destructive recovery experiments. Do not test a clean install by removing the children's actual app and its saved rooms. Before a save-format migration, preserve a recoverable backup. Forward migrations and supported recovery paths are explicit; an older app may not understand a newer save, so “roll back the APK” is not a complete recovery policy.

A family-release checklist includes:

- Both iPads plus phones launch and can play offline; required art/audio and installed media are available without downloads.
- PC auto-join, four players, independent rooms, mobile hosting/recovery and creation preservation pass in the supported configuration.
- Menus and video/book overlays remain local; other players continue their activities.
- Current English content is understandable; unavailable content is not represented as complete.
- An in-place update and the real free iPad refresh cycle retain saves, personal rooms and imported media/bookmarks.
- PC startup, world backup and restore are documented and tested; native logs/symbols can identify a failure.

Keep backups for world/profile records, imported media manifests/files where needed, source assets and signing material. Test one restore into a separate test location. A copy that has never been restored is not evidence of a working recovery procedure.

The normal server runs privately on the home network. External PC access/hotspot qualification remains a later option. The core implementation does not require public hosting, cloud accounts for the children, or internet access to generate ordinary speech.

## 16. Risks, decisions and change control

| Risk or unanswered question | Resolve at | Decision rule |
| --- | --- | --- |
| Actual Android OS, Mac/Xcode and native build compatibility | G1 | Record and test actual installed tools; do not treat reported hardware as a completed toolchain |
| NGO status/dependency mismatch | G1 candidate setup; G3 network qualification | Inspect exact editor/package status and relevant fixes; lock only a passing combination |
| A10 hosting several independently active areas | G3–G4 | Measure; reduce unnecessary simulation/render/load cost first; report a remaining hard limit rather than silently dropping iPad hosting |
| Host termination and disconnected edits | G4–G5 | Demonstrate coherent recovery and explicit conflict preservation before content expansion |
| Shared props accumulate in bedrooms or returns destroy creations | G5 | Validate categories, containers, station stock and creation transfer as one policy |
| Free iPad installation expires despite intended automatic refresh | Starts G1; release blocker at G8 | Prove the actual renewal path; record reachable-device requirements and data preservation |
| Scope exceeds available production time | G6–G7 | Use measured slice throughput; keep unbuilt scope visible; release labeled smaller batches |
| Young child cannot operate controls or understand prompts | G2 and G6 | Simplify interactions and assistance based on observed use before copying the UI everywhere |
| Native media/import/navigation component fails on a target | Small proof before its content batch | Use a qualified alternative behind the same interface; record the replacement and affected tests |

Record major choices as short decision notes: **context → choice → alternatives considered → reason → consequence → evidence → revisit trigger**. Keep a stable decision ID. Initial decisions are: new isolated Universal 2D project; one authoritative rule model; independent logical rooms; local media/bookmarks; free-first packages; early mobile recovery proof; and the G0–G9 sequence.

Changing a package or pipeline must have a concrete reason. Do not replace the network framework, save format, scene model or artwork pipeline mid-feature without identifying migration work and retesting affected contracts. The user can change the goal; the guide then records the effect on dependencies rather than scattering partial edits across unrelated systems.

## 17. Coverage of the Family Playset goal sheet

Every feature ID in the current section-16 tracker has a route below. The phase shown introduces/proves it; full content completion still needs its G7 batch and G8 release checks. This is a coverage map, not a completion report.

| Goal IDs / requirement | Foundation and proof | Content completion |
| --- | --- | --- |
| CHAR-01, CHAR-02 — switching, unrestricted favorites, parents | G2 identity/rig boundary; G3 replication; G6 representative parent rig | G7-A/E roster and all required interactions |
| FAMILY-01, JOIN-01, NET-02 — four devices, drop-in play, PC continuity | G3 real four-client shared world | Rechecked across G6/G7 activities |
| AUTO-01, AUTO-02 — automatic discovery, both iPads hosting/recovering | G3 native discovery; G4–G5 recovery/reunion | Mandatory G8 regression |
| WORLD-01, WORLD-02 — independent travel and non-interruption | G3 separate logical areas; G4 offscreen host proof | All six worlds in G7 |
| ITEM-02 — exclusive shared holder | G2 rule; G3 race/rejoin; G5 recovery | Every shared prop |
| ITEM-03, STOCK-01, ROOM-02 — returns, stock and usable bedrooms | G5 full category/container policy | G7-A and every new station |
| ROOM-01, SECRET-01 — separate bedrooms and shared visits | G5 owned layout/reunion; G6 presentation | G7-A both bedrooms/secret rooms |
| ACT-01 — optional easy quests | G2 optional prompt; G6 reusable lifecycle | All applicable G7 activities |
| COOK-01 — five pizzas, five cakes, five meals | G2 capabilities; G6 one complete recipe | G7-A all 15, including PIZ/CAK/MEAL catalog entries |
| FISH-01, CLEAN-01 — fishpond and cleanup | G6 one polished example of each | G7-A/B variants and cancellation |
| HIDE-01, HIDE-02, HIDE-03, NPC-01 — hiding, clues, seekers and routines | G5 ownership concepts; G6 parent seeker and NPC task/role integration | G7-A/B child-seeker variant and furniture coverage |
| BOOK-01, TV-01 — narrated books and resumable local videos | G1 media smoke check; G6 complete controllers and persistence | G7-A approved books/library import workflow |
| DINO-01, DINO-02, LAB-01 — toys, discovery and science | G2 interaction families; G6 representative examples | G7-A approved collections/stations |
| OUT-01 — beach, creek and usable playground | G3 independent zones; G6 shared capabilities | G7-B all selected outdoor activities |
| DAY-01, LEARN-01 — daycare and learning | G6 invitations/NPC/activity foundations | G7-C optional day and reviewed stations |
| IMG-01 — nine imagination stories | G6 role/NPC substitution proof | G7-D all nine requested stories |
| CAT-01 — show activity catalog | G6 reusable activity/data pipeline | G7-E selected approved catalog content |
| TRAVEL-01 — full offline solo and returning progress | G2 cold offline play; G5 branch/reunion | G7 offline coverage for every solo-capable feature |
| REMOTE-01 — optional away-from-home PC connection | Existing research retained | G9 separate optional qualification; does not gate travel solo |
| Controls, menus, six-world wheel, speech and settings | G1–G2 shell/input; G6 usable presentation | G7 complete content/language settings |
| English/Spanish | Stable locale IDs early; English in G6 | G7-F reviewed Spanish, following English delivery |
| Signing, saves, crashes and updates | G1 workflow; continuous G2–G7 checks | G8 release acceptance and G9 maintenance |
| Optional AR | Not a dependency of the 2D foundation | G9 separate experiment if pursued |

The current tracker contains 35 top-level feature IDs. The coverage validator checks that all appear in this map. The full research includes additional detail, proposed counts and acceptance conditions; follow the linked goal-sheet sections when implementing each row rather than treating this summary as a replacement specification.

## 18. Current work record and research basis

**Device priority:** iPads first, Android second. The A2197 iPad 7 is the minimum performance baseline; A2602 iPad 9 is the other primary play device. Test core controls, layouts, media, saves and hosting on both. Android and optional iPhone testing remain part of the mixed-device acceptance gates.

| Field | Current value |
| --- | --- |
| Goal sheet | Main Family Playset research, 54 sections, including the latest TV bookmark clarification |
| Production guide | This document; ordered method and acceptance gates |
| Implementation state | G1: iPad 9 signed 16 → 20 update retained 1,019 taps/profile and the current 10.3-second bookmark; visible resume and start-over/play/pause/skip passed. The subsequent bookmark is 8.9 seconds. The bounded iPad 9 update task is complete. Samsung retains tested 15; Android 17 was built/inspected only. Server 18 and Windows 19 checks passed. Older iPad 7, native Android 16 KB, independent recovery and renewal gates remain open. |
| Current research deliverable | G0 guide, coverage map and source review; document validation recorded separately |
| Active implementation gate | G1 — project and device foundation; [live record](implementation/g1-status.md) |
| Current bounded task | Completed provisional G2 garden rules/input/checkpoint prototype: Windows 35, emulator 30, 16 core tests, full input integration and save/update recovery. Build 36 also passed layout/input at three tablet/phone-shaped viewports. [Evidence and limits](implementation/g2-status.md). Windows 37 reduces presentation allocation, passes 18 core tests and 35 → 37 recovery. Windows-only iPad export 38 passed inspection; native Mac build/signing and iPad garden checks remain pending. Windows 39 passed native crash recovery and unreadable-save checks; 20 core tests pass. [Recovery task](implementation/g2-recovery-2026-09-24.md). Windows 41 completed local voice settings/restart and update checks; refreshed iPad export 42 passed Windows inspection. [Settings task](implementation/g2-settings-2026-09-24.md). Windows 43 completed visual drop-target hints; 21 core tests and 41 → 43 garden/settings updates passed. Current iPad export 44 passed inspection; Android emulator 30 → 45 preserved progress and verified visible hints/restart. [Picture-hint task](implementation/g2-picture-hints-2026-09-24.md). The physical iPad/child checklist remains open. The user requested continued Windows work while away: provisional G3 loopback PC server/four-client probe **48** now passes all 10 native scenarios for FAMILY-01, JOIN-01 and ITEM-02; **25** core/save/session tests pass. Solo regression **49** passed tablet input and **43 → 49** saved-play update/recovery. Refreshed unsigned **iPad export 50** passed Windows inspection of all 3,083 files; native compilation/signing and device checks remain pending. [Networking evidence and explicit limits](implementation/g3-network-probe-2026-09-24.md). No Mac or physical mobile device was accessed; G1–G3 are not declared passed. Next bounded Windows task: connect the garden presentation to two local clients with visible pending/rejected-action handling. G1/G2 device gates remain open. G1 video 22 passed [bookmark checks](implementation/windows-bookmark-2026-09-23.md); iPhone USB refresh passed but Wi-Fi/unattended renewal remain [unresolved](implementation/ipad-refresh-2026-09-23.md). |
| Package budget | $0 starting setup; optional paid candidates around $20 require a concrete benefit and purchase authorization |
| Open technical evidence | Exact package set, native build path, refresh cycle, A10 hosting/recovery and save reconciliation |
| Scope preserved | Four mixed devices, independent rooms, both iPads hosting, automatic recovery, full offline solo and the complete content goal sheet |

This research used the existing 54-section goal sheet, its feature tracker, architecture/media chapters and supporting audit/package/network reports. Fresh primary-source review covered Unity's production/milestone guidance, Mega Cat Studios' published workflow, Unity project/assembly/data/build/package documentation, Apple release/device/touch guidance, Android lifecycle/signing/installation guidance, Git LFS and Addressables memory management. Citations appear next to the findings they support.

The folder `docs/bluey-research/build-guide-review-2026-09-23` records source URLs, retrieval evidence and selected excerpts. Existing package versions remain candidates from the package review; this pass did not import or certify them. The precise folder structure, task method, phase order, coverage mapping and acceptance gates are **our proposed implementation plan**, not a claim that a vendor or every company prescribes them.

No calendar promise is made. After G6, use actual asset/task throughput and discovered defect work to estimate subsequent batches. Keep the work record current so the next session starts from evidence and the next dependency, not from memory or whichever feature seems most interesting that day.
