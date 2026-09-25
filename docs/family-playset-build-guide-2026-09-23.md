# The Family Playset — ground-up build guide

**Production guide · September 23, 2026 · Implementation audit updated September 24, 2026**

**Purpose:** build the game in a deliberate order, with a foundation that supports the complete Family Playset goal sheet. Start with small, testable systems; prove the difficult requirements; finish a representative playable area; then produce content in batches. A phase is complete because its acceptance checks passed, not because its scripts or pictures exist.

**Current position:** **The foundation follows the goal sheet; the full game is still being built.** All four mobile devices have connected together to the family PC authority. The user confirmed working controls. The PC server and Samsung are now on build 83: persistent hosting is active, the saved family world is preserved, and Android has the wider layout and reset cues. iPads and iPhone remain on 79. Two-iPad movement, shared bucket handling, independent travel and rejoining have scoped physical evidence. G1/G2/G3 remain partial: sustained device measurements, everyday recovery/setup, Windows sign-in startup, iPad hosting, durable personal rooms and content remain ahead. [Completed work, all 35 feature statuses and remaining gates](#19-implementation-audit-and-remaining-work) · [Next tasks in order](#9-first-implementation-work-queue).

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

**GitHub destination confirmed September 24:** [TheNayster/little-weeps-game](https://github.com/TheNayster/little-weeps-game) is this game's private remote, named `origin`. Previously the project's Git history existed only locally. Use `main` for the shared baseline and retain the `codex/…` development branches. Git LFS accompanies the tracked narration/video. This source repository does not back up ignored local worlds, builds, personal media or signing material; those still require the separate backup/restore workflow.

**Personal TV folder prepared September 24 — TV-01:** `Media/TV/` at the project root is reserved for the family's video library. The root `.gitignore` excludes all its contents and subfolders except its README, including clips, posters, subtitles and local catalogs. This is local folder preparation only; automatic import, the in-game library and device media delivery remain unbuilt. Keep the generated foundation playback test clip tracked separately.

The following is the **target structure**. The isolated Unity project, build profiles, tools and Core/Runtime/Adapters/Client/Server/Editor code boundaries now exist. A separate NetworkProbe assembly contains the provisional networking adapter. The current game assets are mainly Code, Resources and Scenes; the full Content/Art/Audio/UI/Tests and editable source-asset pipeline below is still to be established as needed, before content production. Do not read this tree as a completed inventory:

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

**Working baseline:** Unity **6000.3.24f1** and NGO **2.13.2** remain pinned. Unity Transport is **2.7.4 with local embedded patch 1**, which releases completed receive buffers on discard paths; this is not an official Unity release. Shared Windows 68 and separate solo 67 are the latest qualified PC builds. [Exact local patch, provenance and qualification](implementation/g3-rejoin-recovery-2026-09-24.html). The [package report](family-playset-package-research-2026-09-23.html) retains earlier package research. Mobile IL2CPP builds, Wi-Fi play and physical-device networking still need qualification.

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

**Actual sequence so far:** the user authorized provisional Windows G2/G3 work while device and renewal checks remained open. Both iPads subsequently passed the solo garden checks. This overlap produced useful evidence; it did not close G1, G2 or G3. The current gate ledger is in section 19. Do not begin broad content production while required mobile hosting/recovery remains unproven.

| Gate | Main deliverable | Complete only when | Hold back until it passes |
| --- | --- | --- | --- |
| **G0 — production plan** | Goal sheet, this guide, risk/coverage records | Scope and sequence are recorded, documents link correctly, implementation status is honest | Unscoped implementation without a requirement and acceptance check |
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

Include one home area and the garden, two finished character rigs plus a representative parent-size rig, switching/holding/seating anchors, one complete cooking recipe, one cleanup interaction, pond fishing, one parent-led hiding round and the shared activity invitation UI. Add a small narrated book, one interactive dinosaur, one science example, a local-video library with resumable playback, a bedroom and a secret-room sample. Two owned bedrooms must first be implemented and tested in G5; they do not exist in the current garden fixtures. G6 adds their usable presentation.

These small examples exercise the content pipeline and cross-system interactions. They do not fulfill the full roster, all 15 recipes or nine imagination stories. Demonstrate a role-based activity and NPC substitution before scaling daycare stories. G6 may have sub-batches, but its cross-system acceptance is one gate.

### G7–G9: expand, release and maintain

Use the batch order in section 12. A small private playable may be shared when its applicable release checks pass; label its incomplete content honestly. “Full goal sheet complete” is reserved for the implemented required scope, not the first family alpha. Estimate later batches from the measured time to create and test the slice, rather than inventing calendar dates now.

## 9. First implementation work queue

**Current queue, updated September 25.** The original startup work is largely present: separate project/repository, recorded tool versions, stable app identities, named build profiles, repeatable builders, native device launches, and scoped save/update proofs. Automatic renewal and independent backup/restore remain open. Section 19 separates completed checks from full gate completion; older implementation records retain their dated evidence.

Complete one bounded task at a time. Each row below will take multiple tasks; it is a dependency order, not permission to start them all together.

**Completed user-directed correction:** build 83 on the PC server and Samsung includes the wider phone floor and the garden's idle/repeat timers. Native Android layout and save/admission checks passed; iPad/iPhone clients remain 79 and still need the cue/layout update. This advances a narrow part of ITEM-03; the full G5 world policy remains in sequence. [Record](implementation/g3-phone-layout-resets-2026-09-24.html).

| Order | Next work | Required result before moving on |
| --- | --- | --- |
| **1 — next bounded task** | **G3-REC-03: outage failure matrix (NET-02 / AUTO-01).** Windows 90 now continues locally from a verified checkpoint, retains separate adventures and safely rejoins. | Qualify native lost acknowledgments/archived intentions, denied branch/selection writes, foreground timing and rejected admission. Then prepare mobile recovery builds and physical acceptance. G4 election/handoff and G5 reconciliation remain required. |
| 2 | Complete G3 reliability and the remaining mixed-device matrix, one task at a time | Activate qualified local recovery/supervision and complete measured sustained/device runs. Then close phone lifecycle/ownership/travel, per-device solo coverage, server-absent initial joining, mobile prolonged-loss automatic local qualification, parent pairing and route changes. Both iPads already pass scoped movement/bucket/travel/rejoin, and all four devices were concurrently admitted; do not repeat those as unstarted work. [Current checklist](implementation/return-checklist-ipad-lan-2026-09-24.html). |
| 3 | G4 full recovery-state replication and both iPads hosting | With PC absent, each iPad can become authority; simultaneous launch selects one; planned handoff, abrupt loss and a returning old host recover coherently. A surviving client needs a complete recovery checkpoint, not just its visible world. Media/area changes must not stop host simulation. |
| 4 | G5 durable personal world and item policies | Two owned bedrooms, saved creations, containers, idle returns, station stock and offline branch/reunion. Test conflicting edits and duplicate/replayed operations without destroying either child's work. |
| 5 | G6 finish one representative slice | Establish content definitions, authored floor/occlusion, character rigs, picture menus/assistance, activities, NPCs, media and reviewed speech. Prove the small integrated examples in section 8 with both children before copying them into all worlds. |
| 6 | G7 batches A–F, then G8 family release | Expand the approved content in section 12; close device, renewal, update, offline and recovery checks. Keep missing content visible until its acceptance passes. |

**Foundation checks to close alongside that sequence:** complete free renewal on both iPads and observe an actual unattended refresh; choose an independent backup destination and prove a restore; finish native Android 16 KB qualification; complete remaining current-build phone acceptance; measure sustained frame times/memory on the A10. Both phone updates to 79 have passed. G2 also needs structured child usability, assistance styles and authored movement/interaction contracts before its gate closes. The user reports children are playing; that alone is not an observed usability study. These are tracked work, not a reason to repeat already-passed smoke checks every session.

**G3-OPS-01 result:** the parent page, safe lifecycle actions and isolated acceptance are complete within scope. It uses fresh native observations and validated process identity, verifies the checkpoint checksum, rejects duplicate starts, and has an authoritative occupied-stop guard. The real dashboard monitors existing 83 without stopping it. [Implementation and test record](implementation/g3-parent-controls-2026-09-24.html).

**Deployment follow-up:** guarded Stop requires runtime 84 or later; prepared 85 also guards interrupted restores. Apply that prepared server only in an empty family session, after its exact executable has the reviewed Windows network permission; preserve the enrolled world through the existing controlled update path. Until then, the page intentionally disables Stop on 83. This is pending activation, not a failed runtime test. No client update is needed for the local control protocol.

**G3-OPS-02 result:** same-user Windows backups, exact restore/rollback, missing-directory reconstruction and fail-closed interrupted recovery are implemented and qualified in a separate four-player world. The actual 83 world was backed up without stopping it. Independent storage and portable credentials remain unqualified. [Recovery record](implementation/g3-server-recovery-2026-09-24.html).

**G3-OPS-03 result:** the real supervisor recovered an intentionally terminated test authority and all four existing Windows clients rejoined in an observed 19.34 seconds. Parent stop/restore intent, duplicate-helper refusal, retry limits and blocked-save cases pass. This is not deployed supervision, boot/service startup or physical-device recovery. [Record](implementation/g3-server-supervision-2026-09-24.html).

**G3-OPS-04 result:** 600.03 real seconds, 75 cycles, four native Windows clients, movement/travel, ordinary item/station timers, client departure/rejoin and exact save restart passed. Motion traces before/after meet existing thresholds; memory observations are desktop-only. [Run record](implementation/g3-server-soak-2026-09-24.html).

**G3-OPS-05 result:** the parent page now supplies verified local backup and guarded recovery enable/pause/status. Four HTTP/native groups and browser actions pass; the actual family page was refreshed and backed up while the original 83 authority stayed running. Activation still needs controlled 85 deployment. [Panel record](implementation/g3-parent-recovery-panel-2026-09-24.html).

**G3-REC-01 result:** Windows 88 adds negotiated, bounded transfer of complete authority checkpoints and durable client storage, including all retained receipts and idle clocks. Partial/stale/wrong-world/future-format data is rejected; durable credit follows successful write/read-back. Four-client loss/restart and disk-failure checks, legacy 79/83 compatibility and movement checks are recorded in the [client recovery report](implementation/g3-client-recovery-2026-09-24.html). The previous solo draft remains separate. This is not deployed to devices. G3-REC-02 below now implements the local transition on Windows. G4 still owns iPad election/handoff/partition recovery, and G5 owns reconciliation.

**G3-REC-02 result:** Windows 90 automatically creates a distinct local adventure from the last verified shared checkpoint after ten observed foreground seconds disconnected. Four-client crash/restart, gesture-safe reunion, reopenable saved work, two outage histories and retained original solo pass. Unconfirmed commands are archived as unresolved intentions and never automatically replayed. This is not mobile deployment, host migration or conflict merging. [Record and limits](implementation/g3-local-continuation-2026-09-25.html).

**Device follow-up when available:** update the Apple clients for return cues and the iPhone's wider layout, preserve their identities/saves, and check tablet geometry remains unchanged. Authority 83 already runs the shared timers for older 79 clients; those clients do not display the new cue, and their offline solo fixtures still use their older code. Phone 83 has a native layout/admission/save check, but no new human multitouch/audio acceptance. Native ARM64 16 KB remains a separate Android portability check; the family's actual Samsung uses 4 KB pages and already runs the release.

**Future hosting decision:** the owned VPS is the preferred shared-world deployment target after the server reliability gate. Prepare remote client endpoints and a controlled world/credential migration as a separate milestone; this need not wait for all game content. Travel co-op still depends on internet and stays lower priority than core reliability; AR remains optional. Full offline solo, ordinary-network automatic joining, both iPads hosting and host recovery remain required. Bluetooth remains removed. [VPS plan](implementation/vps-hosting-plan-2026-09-24.html).

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
| A2197 iPad 7 / device-observed iPadOS 18.7.10 on 24 September | Solo, PC client, mobile host, host recovery, media while hosting and lowest-performance acceptance |
| A2602 iPad 9 / device-observed iPadOS 18.6.2 on 24 September | Same gameplay and both client/host roles; repeat handoffs in both directions |
| SM-S948U1 Android phone / observed Android 16, API 36, 4 KB pages | Solo, cross-platform client, native discovery, phone UI, background/resume and in-place update; separate native 16 KB qualification remains open |
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

The current server runs on the home network. After reliability qualification, the owned VPS is the preferred future shared host, with parent-provisioned remote endpoints and verified migration. Select direct authenticated/encrypted access or a private route based on actual VPS/network tests. Local solo content and ordinary speech do not require internet; do not give children a server-setup flow. [Hosting decision](implementation/vps-hosting-plan-2026-09-24.html).

## 16. Risks, decisions and change control

| Risk or unanswered question | Resolve at | Decision rule |
| --- | --- | --- |
| Current mobile build compatibility | G1/G3 | Android 16 and Mac/Xcode inventory are recorded; repeat native builds/device checks for the current embedded transport patch. Earlier solo/foundation installs do not qualify mobile multiplayer. |
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
| REMOTE-01 — owned VPS as future shared host | Owned VPS plan after G3 server reliability gate; remote endpoint and world/credential migration | Separate VPS deployment qualification when the server is ready; preserves G4/G5 and offline solo; does not wait on every G7 activity |
| Controls, menus, six-world wheel, speech and settings | G1–G2 shell/input; G6 usable presentation | G7 complete content/language settings |
| English/Spanish | Stable locale IDs early; English in G6 | G7-F reviewed Spanish, following English delivery |
| Signing, saves, crashes and updates | G1 workflow; continuous G2–G7 checks | G8 release acceptance and G9 maintenance |
| Optional AR | Not a dependency of the 2D foundation | G9 separate experiment if pursued |

The current tracker contains 35 top-level feature IDs. The coverage validator checks that all appear in this map. The full research includes additional detail, proposed counts and acceptance conditions; follow the linked goal-sheet sections when implementing each row rather than treating this summary as a replacement specification.

## 18. Current work record and research basis

**Current build summary:** Windows 90 now automatically continues locally after prolonged server loss, saves separate adventures, resumes them after a crash and preserves them on automatic family reunion. Four-client native checks, 67 core/storage checks, compatibility and motion checks pass. The live family server/Samsung remain 83 and Apple clients 79; no device update occurred. Parent activation still targets 85 and awaits its reviewed Windows permission. G1/G2/G3 remain partial. **Next: G3-REC-03 outage failure matrix, followed by mobile recovery qualification.**

**Device priority:** iPads first, Android second. The A2197 iPad 7 is the minimum performance baseline; A2602 iPad 9 is the other primary play device. Test core controls, layouts, media, saves and hosting on both. Android and iPhone testing remain part of the mixed-device acceptance gates.

**Earlier solo-device milestone, 24 September (build 56):** the user tried the two Windows player windows and reported positive feedback. The Mac has now compiled/signed **solo garden 56**, and the iPad 9 update from 20 → 56 passed with all original preferences retained. Physical layout, walking, watering, speech, joystick plus dragging, menu/screen-lock cancellation, remembered voice/movement settings and Wi-Fi-off solo play passed. Closing and reopening retained the complete garden snapshot exactly. The older iPad 7 now also runs the garden: repeated checks were reported successful, and an actual restart retained its entire save and preferences. Its separate app-inventory query has a recorded CoreDevice error. Child usability, sustained measured performance and additional feature checks remain open. See the [iPad garden evidence](implementation/ipad-garden-2026-09-24.md) and [return-home checklist](implementation/return-home-checklist-2026-09-24.md). Mobile multiplayer was not in that 56 export; subsequent 79/83 family builds and mixed-device evidence are recorded below. G1/G2/G3 are not complete.

| Field | Current value |
| --- | --- |
| Goal sheet | Main Family Playset research, 55 sections, including owned VPS hosting after server readiness |
| Production guide | This document; ordered method and acceptance gates |
| Implementation state | All four physical clients were concurrently connected on family 79. Server and Samsung now run 83; iPads/iPhone remain 79. The server migration preserved every existing world field, and Android retained its enrollment and local branches. Both iPads retain their earlier saves/settings and pass native admission, reported movement/bucket/travel/lock-return plus native cold rejoin. Solo restoration passed on the tested iPad; repetition on both is unspecified. Samsung 15 → 79 and iPhone 20 → 79 updated in place, enrolled distinct profiles and pass native admission plus reported layout/walking/bucket/Listen. Three old iPhone preferences were retained; exact private Android preference retention was not read back. Windows/Android-emulator proofs and preview 68 are preserved. |
| Current research deliverable | The 55-chapter / 35-feature audit remains the coverage baseline. [Audit index](implementation/evidence/plan-audit83-2026-09-24/audit.json). Later implementation records below supersede its dated statuses without changing requirements. |
| Current phase position | G3 now has concurrent four-physical-device admission and scoped play feedback; G1/G2/G3 exit checks remain open. No gate is declared complete. [Gate-by-gate ledger](#phase-gates-done-versus-open). |
| Current bounded task | **G3-REC-02 complete within the stated Windows scope:** automatic local continuation, cold resume, preserved adventures and safe reunion in 90. [Record](implementation/g3-local-continuation-2026-09-25.html). |
| Latest completed engineering task | **Local continuation:** four clients recover from an isolated authority crash into separate worlds, release stale item holds, keep the older solo save, resume after a client crash and reopen saved work after reunion. Six native groups pass; no device or family authority update occurred. |
| Next bounded task | **G3-REC-03: outage failure matrix.** Qualify unresolved command archives, denied native branch writes, foreground timing and rejected admission. Follow with mobile builds/device acceptance; G4 hosting and G5 reconciliation remain required. |
| Package budget | $0 starting setup; optional paid candidates around $20 require a concrete benefit and purchase authorization |
| Open technical evidence | Patched transport under sustained physical-device play, remaining mobile lifecycle/route checks, both iPad host roles/recovery, reconciliation, real automatic refresh, independent backup/restore and sustained A10 performance. Package versions and the native Mac build path are already recorded. |
| Scope preserved | Four mixed devices, independent rooms, both iPads hosting, automatic recovery, full offline solo and the complete content goal sheet |

The original September 23 production research used the 54-section goal sheet, its feature tracker, architecture/media chapters and supporting audit/package/network reports. That source review covered Unity's production/milestone guidance, Mega Cat Studios' published workflow, Unity project/assembly/data/build/package documentation, Apple release/device/touch guidance, Android lifecycle/signing/installation guidance, Git LFS and Addressables memory management. Citations appear next to the findings they support. The September 24 implementation audit checks local source, requirements and retained evidence; it does not claim a fresh revalidation of every external citation.

The folder `docs/bluey-research/build-guide-review-2026-09-23` records the original source URLs, retrieval evidence and selected excerpts. Earlier package recommendations are research candidates; the actual installed baseline and later qualification are recorded in sections 7 and 19. The precise folder structure, task method, phase order, coverage mapping and acceptance gates are **our implementation plan**, not a claim that a vendor or every company prescribes them.

No calendar promise is made. After G6, use actual asset/task throughput and discovered defect work to estimate subsequent batches. Keep the work record current so the next session starts from evidence and the next dependency, not from memory or whichever feature seems most interesting that day.

## 19. Implementation audit and remaining work

**Audit date: September 24, 2026. Source baseline: `e348dd51d5a847895e0e5941b7cfdc780e0e1510`. Verdict: aligned foundations, unfinished game.** The implementation uses the intended separate project, shared solo/server rules, stable player identities, authoritative items, independent areas and local saves. All four physical mobile clients were concurrently admitted on 79; both iPads have scoped shared-play/rejoin feedback, and both phones have basic control/voice feedback. The PC server and Samsung now run 83 with preserved world/enrollment, active garden reset timers and the native wider Android layout. iPads/iPhone remain 79. These results establish a working mixed-device prototype; they do not complete sustained four-device qualification, iPad hosting/automatic host switching, or the full game content.

The audit maps all **55 research chapters and 35 feature IDs** to implementation work. It compares requirement coverage with focused source inspection, package locks, build/deployment records and dated test/user reports. It does not rerun game tests, retest devices, profile current server uptime or freshly revalidate every external citation. [Current audit scope, chapter coverage and evidence index](implementation/evidence/plan-audit83-2026-09-24/audit.json). The [original 54-chapter audit](implementation/evidence/plan-audit-2026-09-24/audit.json) is historical and contains statuses that later work supersedes. No requirements were removed. No overall percentage is assigned: a passing bucket prototype and a finished daycare story are different sizes of work.

**Alignment decision:** keep the current foundation and continue G3 reliability before widening content. Required four-player iOS/Android play, independent travel, one-holder items, optional quests, all six worlds and full offline solo are preserved. G4 iPad hosting and automatic recovery are still mandatory. The owned VPS is a separate deployment milestone after server readiness, not a replacement for those requirements. The early garden timers are a user-requested slice of G5, not completion of the personal-world system. No restart from scratch or new template is justified by this audit.

**How to read status:** **Passed proof** means a named, bounded check passed in the stated environment. **Partial** means useful implementation exists but the full requirement is not accepted. **Not built** means research/design only. **Optional later** applies only to explicitly optional work. None of the 35 complete feature requirements is being marked fully accepted today.

### Completed proofs we can build on

Each row describes that particular proof and its limits, not a claim that later rows have not happened. Current per-device versions and remaining work are consolidated in the device ledger below.

| Scoped work completed | Evidence | What it does not establish |
| --- | --- | --- |
| Separate Unity project, source control, stable application identity and named platform builds | [G1 record](implementation/g1-status.md), [build profiles](implementation/build-profiles-2026-09-23.md) | Independent off-device backup or finished family release |
| Windows creation → iOS export → Mac Xcode signing/install route | [iPad 9 garden](implementation/ipad-garden-2026-09-24.md), [iPad 7 garden](implementation/ipad7-garden-2026-09-24.md) | Latest shared build running on iOS; Unity Editor installation on Mac is not required by the working export route |
| Tap/joystick, multi-touch dragging, fill/pour/water/cleanup, avatar switching and optional prompt rules | [G2 record](implementation/g2-status.md), both iPad garden reports | Finished illustrated characters, all item capabilities, picture-only child usability or assistance modes |
| Local saves, exact restarts, damaged-primary recovery and selected in-place updates | [Solo 64 → 67 update](implementation/evidence/rejoin-recovery-2026-09-24/solo-update-64-to-67.json), [recovery](implementation/evidence/rejoin-recovery-2026-09-24/solo-recovery-67.json), device records below | Offline branches merging, peer recovery checkpoints or zero progress loss after every possible termination |
| Four Windows clients share one PC authority; independent Garden/Creek views and exclusive props | Shared 68: [10 network cases](implementation/evidence/rejoin-recovery-2026-09-24/network-68.json), [10 UI cases](implementation/evidence/rejoin-recovery-2026-09-24/garden-ui-68.json), [8 area cases](implementation/evidence/rejoin-recovery-2026-09-24/areas-68.json) | LAN discovery, mobile admission or a complete Creek location |
| Trusted native discovery and encrypted family admission on Windows | Build 70: [11 LAN cases](implementation/evidence/family-lan-2026-09-24/lan-70.json), [45 rules/native checks](implementation/evidence/family-lan-2026-09-24/rules-45.json), [offline input](implementation/evidence/family-lan-2026-09-24/offline-input-70.json) | Parent pairing UI, iOS/Android adapters, continuous foreground reconnect, mobile host recovery or IPv6 qualification |
| Prepared Apple discovery/storage adapter and automatic Windows rejoin of established sessions | Build 74: [seven four-client reconnect cases](implementation/evidence/ipad-lan-2026-09-24/reconnect-74.json), [Apple bridge](implementation/evidence/ipad-lan-2026-09-24/apple-bridge.json), [builds and final regressions](implementation/g3-ipad-lan-2026-09-24.html) | Signed iPad install/enrollment, real mobile suspension, automatic initial offline-to-shared joining, Android discovery, iPad hosting or offline merging |
| Android native discovery, protected enrollment and PC cross-play | Android 78: [24 native checks](implementation/evidence/android-lan-2026-09-24/native-24.json), [seven gameplay/lifecycle checks](implementation/evidence/android-lan-2026-09-24/shared-7.json), [release/update/offline evidence](implementation/g3-android-lan-2026-09-24.html) | Physical Samsung or four real mobile devices, native ARM64 16 KB, API 26–33 adapter, parent pairing UI, initial offline transitions or host migration |
| Safe initial offline-to-family joining and saved solo return | Build 79: [7 Windows cases](implementation/evidence/offline-join-2026-09-24/windows-offline-7.json), [4 Android release cases](implementation/evidence/offline-join-2026-09-24/android-offline-4.json), [update/regressions](implementation/g3-offline-join-2026-09-24.html) | Physical mobile qualification, automatic prolonged-outage local flow, parent setup UI, offline reconciliation or host migration |
| Native family admission and shared presentation on both physical iPads | Build 79: [iPad 7 + iPad 9 connection evidence](implementation/evidence/ipad79-2026-09-24/native-connection.json); matching world, authority epoch, roster and sampled player state | Physical smoothness, touch/ownership, lifecycle and late-join checks, four physical devices or iPad hosting |
| Physical two-iPad movement and shared bucket handling | Build 79: user replied **“works great!”** after the two-way movement and exclusive pickup/release check. [Feedback](implementation/evidence/ipad79-2026-09-24/movement-bucket-feedback.json) | Measured frame times/latency, sustained play, independent travel, lifecycle or four physical players |
| Physical two-iPad independent areas and reunion | Build 79: user confirmed Creek/Garden separation, uninterrupted play in the other area and seeing each other after reunion. [Feedback](implementation/evidence/ipad79-2026-09-24/independent-travel-feedback.json) | Later activities/worlds, four physical devices, lock/return or mobile hosting |
| Both physical iPads leave and rejoin without interrupting the sibling | Build 79: user confirms lock/return plus app close/reopen; native reads confirm new processes, retained identities and the original PC authority. [Evidence](implementation/evidence/ipad79-2026-09-24/lock-cold-rejoin.json) | A measured same-process suspension timeline, prolonged outages, every activity, four physical devices or host recovery |
| Physical solo/family/solo round trip on the tested iPad | Build 79: user confirms server positions on rejoin and the earlier solo toy position on solo return. [Feedback](implementation/evidence/ipad79-2026-09-24/solo-roundtrip-feedback.json) | Which iPad was tested, repetition on both, initial server-unavailable late joining or offline reconciliation |
| Both phones updated and all four physical mobile profiles admitted | Build 79: Samsung same-signer exact-APK update, iPhone native installed-version and retained preferences; each passed native discovery/admission and user-reported basic controls. Original PC authority lists both iPads, Samsung and iPhone together. [Evidence](implementation/g3-phones-79-2026-09-24.html) | Sustained four-device performance, every activity/lifecycle, detailed phone ownership/travel, unattended renewal or always-on PC operation |
| Wider phone floor and idle/repeat garden timers, deployed in 83 | Build 82 layout/shared/rejoin checks, real-time 81 timer proof, and applied 83 PC/Samsung deployment. Tools: 180s + 5s cue; completed stations: 60s + 5s cue. Held tools and partial progress are protected. Native Android screen inspected. [Record](implementation/g3-phone-layout-resets-2026-09-24.html) | Apple client cue/layout updates, broader native touch/layout qualification, personal-item/container/creation policy and host migration |
| Persistent server and retained Android update | Build 83: 52 core checks, six native lifetime/recovery cases, both 79/83 compatibility directions; deployed original world preserves all old fields, Samsung retains enrollment and existing item fields. [Record](implementation/g3-persistent-server-2026-09-24.html) | Real two-hour/sustained soak, boot/crash supervisor, independent backup/restore or VPS deployment. The native cutoff check uses a verification-only elapsed offset. |
| Smoother remote movement under the tested conditions | Shared 68: [normal](implementation/evidence/rejoin-recovery-2026-09-24/motion-68-normal.json), [delayed-motion run 1](implementation/evidence/rejoin-recovery-2026-09-24/motion-68-impaired-1.json), [run 2](implementation/evidence/rejoin-recovery-2026-09-24/motion-68-impaired-2.json) | Sustained mobile FPS, real Wi-Fi impairment or larger-world performance. Tests impair received motion only. |
| Eight hard client departures/rejoins without restarting their server; receive-buffer fault fixed | [Final resilience run](implementation/evidence/rejoin-recovery-2026-09-24/receive-resilience-68.json), [diagnosis and patch provenance](implementation/g3-rejoin-recovery-2026-09-24.html) | Automatic client rejoin or replacement of a lost host; the harness relaunches clients explicitly |
| Four visible Windows windows work well for the user | User reported **“working great!”** after the four-window setup. [Recorded feedback](implementation/evidence/plan-audit-2026-09-24/four-player-feedback.json) | Four humans on four mobile devices, a duration benchmark or a measured latency claim |
| Local video controls/bookmark persistence in the separate foundation fixture | [Windows bookmarks](implementation/windows-bookmark-2026-09-23.md), [iPhone manual refresh](implementation/ipad-refresh-2026-09-23.md) | An integrated house TV, thumbnail library, content import or multiple-video acceptance |

**September 25 continuation update:** [G3-REC-02](implementation/g3-local-continuation-2026-09-25.html) qualifies automatic local adventures, cold resume and safe family reunion on Windows 90. Six native groups, 67 core checks, compatibility, movement and restore regression evidence are linked there. Remaining failure/mobile checks and G4/G5 are not closed.

**Earlier client-recovery update:** [G3-REC-01](implementation/g3-client-recovery-2026-09-24.html) delivers complete, durably acknowledged replicas to capable Windows 88 clients while preserving 79/83 compatibility. This is a prerequisite to local continuation and iPad hosting, not completion of those features.

**Later parent-workflow update:** [G3-OPS-05](implementation/g3-parent-recovery-panel-2026-09-24.html) integrates backup/recovery actions and status; the actual parent page has a verified 83 backup, with 85 activation still blocked pending its network/deployment step.

**Later sustained-run update:** [G3-OPS-04](implementation/g3-server-soak-2026-09-24.html) passes a ten-minute four-native-client Windows run. This does not close sustained iPad/A10 or mixed-device qualification.

**Later supervision update:** [G3-OPS-03](implementation/g3-server-supervision-2026-09-24.html) qualifies bounded native crash/rejoin recovery. The helper has not been enabled for the actual family; boot/service deployment remains open.

**Later recovery update:** [G3-OPS-02](implementation/g3-server-recovery-2026-09-24.html) qualifies local same-user backup/restore and rollback on prepared 85. This is not independent disaster recovery; live server 83 is unchanged.

**Later implementation update:** [G3-OPS-01 parent controls](implementation/g3-parent-controls-2026-09-24.html) passed isolated native/browser checks on 84 after the audit. The live family deployment remains 83; this does not change completed device evidence or close G3.

### Phase gates: done versus open

| Phase | Current status | Remaining acceptance work |
| --- | --- | --- |
| **G0 — plan** | **Complete as a planning artifact; maintained** | Keep scope, evidence and the next task current. Planning completion is not game completion. |
| **G1 — project/devices** | **Partial; launch/update proofs exist** | Real unattended free refresh on both iPads, independent backup/restore, unresolved native Android 16 KB checks, and current-build device qualification. |
| **G2 — solo rules** | **Partial; playable garden on both iPads** | Observe each child, establish Simple Play/Explore assistance and authored movement/interaction contracts; complete outstanding device/performance checks. |
| **G3 — PC shared world** | **Partial; four physical admissions, persistent 83 live, local recovery/supervision qualified on 85** | Activate guarded controls through controlled 85 deployment; independent/portable recovery, supervision deployment, parent pairing, remaining device lifecycle/ownership/travel, prolonged-outage automatic solo and sustained measurements. No home-server timer cutoff remains. |
| **G4 — iPad hosting** | **Hosting not built; complete client checkpoint prerequisite exists on Windows 88** | Qualify replication on both iPads, then both host roles, election, event tails, handoff, abrupt loss, resume/partition and returning-host convergence. |
| **G5 — durable family world** | **Garden-only idle-return primitive active in 83; full system remains open** | Owned rooms, creations, portable item/container policies, broader protection/spoken cues, stock, edit history and branch/reunion. |
| **G6 — finished slice** | **Not started as a polished integrated slice** | Authored art/rigs, child-facing UI, NPC/activity framework and the representative examples in section 8, with child and device acceptance. |
| **G7 — content batches** | **Research catalogs exist; production pending** | Batches A–F in section 12 and the detailed inventories below. |
| **G8 — family release** | **Not ready** | Signed release acceptance, actual renewal, save/media retention, restore and all mandatory device/network/content checks for the declared release scope. |
| **G9 — maintenance/options** | **Future release work** | Maintain qualified releases and separately consider optional AR/travel refinements. The owned VPS milestone follows server readiness and need not wait for G9 or all content. Present prototype bug fixes do not complete this phase. |

### Device and toolchain ledger

These are the last recorded checks, not newly contacted devices. The two iPad OS values below come from native observations on September 24 and supersede the earlier user-reported order.

| Device / target | Last recorded build and evidence | Still needed |
| --- | --- | --- |
| **iPad 7 A2197, A10, iPadOS 18.7.10** | **Family 79:** in-place 56 → 79, documents/settings retained, exact-bundle launch, Keychain enrollment and automatic shared admission passed. User confirms smooth movement, shared bucket handling, independent travel/reunion and lock/return on 79; native cold rejoin retains enrollment. Earlier solo physical controls/restart passed on 56. [Current record](implementation/g3-ipad-79-2026-09-24.html) | Client update for reset cues, remaining touch/settings and per-device solo/late-join coverage, sustained lifecycle, hosting/recovery, A10 measurements and renewal. App inventory still has a CoreDevice error; on-device runtime identifies 79. |
| **iPad 9 A2602, A13, iPadOS 18.6.2** | **Family 79:** in-place 56 → 79, documents/settings retained, native inventory/launch, Keychain enrollment and automatic shared admission passed. User confirms smooth movement, shared bucket handling, independent travel/reunion and lock/return on 79; native cold rejoin retains enrollment. Earlier physical solo controls/lifecycle passed on 56. [Current record](implementation/g3-ipad-79-2026-09-24.html) | Client update for reset cues, remaining touch/settings and per-device solo/late-join coverage, sustained lifecycle, hosting/recovery and renewal. |
| **Samsung SM-S948U1, Android 16/API 36, 4 KB pages** | **Family 83:** same-signer 79 → 83, exact installed APK hash verified, existing Keystore enrollment reused, automatic join to persistent PC server and wider native screen inspected. Original local save files unchanged; paired branch retains items/identity with accepted walking progress. Earlier manual controls/voice feedback applies to 79. [Applied record](implementation/g3-persistent-server-2026-09-24.html) | Detailed shared ownership/travel, lifecycle, solo round trips, sustained performance and native ARM64 16 KB. A 4 KB pass is not a 16 KB pass. |
| **iPhone A2484, iOS 26.6.1** | **Family 79:** USB Sideloadly 20 → 79, native version verified, all three previous preference values retained, Keychain/native Bonjour admission passed as fourth client. Reported full layout, walk, bucket drag and Listen passed. [Record](implementation/g3-phones-79-2026-09-24.html) | Client update for wider layout/reset cues; detailed shared ownership/travel, lifecycle, solo round trips and reliable unattended renewal. Wireless refresh remains unresolved. |
| **Windows** | Live preview **68** remains. **Persistent PC server 83** is active with all original saved fields preserved; Android 83 automatically joined. Six native lifetime/recovery cases, 52 core checks and mixed-version compatibility pass; earlier four-device 79 admission remains recorded. [Current evidence](implementation/g3-persistent-server-2026-09-24.html) | Parent controls monitor 83; prepared 85 supplies guarded Stop after controlled deployment. Local recovery/supervision are qualified; independent/portable recovery, helper activation, real sustained run, prolonged-outage automatic solo, pairing UI and alternate/IPv6 endpoints remain. Sign-in startup is a possible local convenience. |
| **Android emulator** | Family-signed release **79**, API 35 / 16 KB x86_64 with ARM translation: native late-join/menu deferral, saved solo return, real background/foreground, cold rejoin and unchanged saves across 78 → 79. Earlier native/gameplay evidence remains. [Latest record](implementation/g3-offline-join-2026-09-24.html) | Legacy Android adapter, sustained performance and native ARM64 16 KB qualification. Physical device results are recorded separately above. |

Pinned project: **Unity 6000.3.24f1, NGO 2.13.2, Unity Transport 2.7.4 with local embedded patch 1, Input System 1.20.0, URP 17.3.0 and UGUI 2.0.0**. The transport patch changes one original vendor file, preserves notices and has regression evidence plus scoped native mobile admission/play; sustained mobile load, interruptions and route qualification remain open. Addressables, Localization and 2D Animation remain candidates, not installed/qualified subsystems. Mac inventory is **M1 Pro, macOS 26.3.1, Xcode 26.6**; everyday creation remains on Windows. `manifest.json`, `packages-lock.json`, [native build evidence](implementation/ipad-garden-2026-09-24.md).

**Installation gap:** the working manual USB refresh is useful, but unattended wireless renewal is not solved, and neither iPad has a proven Windows renewal cycle. Keep the requirement open before relying on these apps for unattended family use. Git/local backups and signing recovery copies also do not establish an independent off-device backup and successful restore.

### All 35 feature requirements: implementation status

The original acceptance wording remains in [goal-sheet section 16](bluey-game-research-2026-09-23.html#16-expanded-feature-tracker). These rows identify what the prototype does and what must still be delivered, rather than replacing those acceptance checks.

| Goal ID | Status and actual implementation | Work still required / phase |
| --- | --- | --- |
| **CHAR-01** | **Partial:** switch between two placeholder skins while preserving identity and held prop | Complete roster/rigs; switching during every activity on mobile. G2/G3 → G6/G7. |
| **CHAR-02** | **Partial:** duplicate placeholder favorites are permitted | Any player chooses children or parents; NPC jobs stay separate; seating/holding/activity role continuity. G6/G7. |
| **FAMILY-01** | **Partial:** both iPads, Samsung and iPhone were concurrently admitted on 79, with scoped play feedback; PC/Samsung now 83, Android wider native layout inspected | Sustained four-device play/lifecycle, remaining Apple cue/layout update, parent setup and A10 load/recovery; both iPads hosting. G3/G4. |
| **ACT-01** | **Partial:** optional garden/cleanup prompts and free exit; deployed authority/Android 83 rearm completed fixtures while retaining partial progress and activity choice | Apple client cue update and broader repeat-play acceptance; picture picker, reusable invitations/membership/checkpoints and nonblocking join/switch for all activities. G6/G7. |
| **COOK-01** | **Not built** | Kitchen interactions and all 15 recipes, free creations, serving/saves/shared preparation. G6/G7-A. |
| **FISH-01** | **Not built** | Backyard fishpond and creek fishing; assisted catch/release, exclusive catches and exit cleanup. G6/G7-A/B. |
| **CLEAN-01** | **Partial:** sponge cleans the garden puddle | Toys, dishes, spills, laundry and garden sorting with creation protection and multiplayer cancellation. G6/G7. |
| **HIDE-01** | **Not built** | Parent default seeker, both children hiding, fair search and immediate participation. G6/G7. |
| **HIDE-02** | **Not built** | Optional child seeker; hidden avatars cannot leak through names, targeting or switching. G6/G7. |
| **NPC-01** | **Not built** | Parent routines, interruptible requests, role reservation/release and substitutes for solo play. G6/G7. |
| **CAT-01** | **Research only:** 32 episode-based ideas cataloged | Select and implement approved activities with assets, narration, saves and cancellation. G6/G7-E. |
| **BOOK-01** | **Not built** | Reading nook, pickup overlay, narration controls, interactive pages/names and approved books offline. G6/G7-A. |
| **TV-01** | **Partial:** separate single-clip foundation fixture with persistent bookmark and playback controls | House TV, thumbnails/library/import, per-child/per-video catalog, failures, transitions and media retention. G6/G7-A. |
| **DINO-01** | **Not built** | Proposed 20 toy types, spoken names, animation, accessible storage and shared interactions. G6/G7-A. |
| **DINO-02** | **Not built** | Discovery Mat: dig/brush/wash/arrange and five optional invitations. G6/G7-A. |
| **LAB-01** | **Not built** | Simple science corner and proposed eight experiments, solo/co-op and independent exits. G6/G7-A. |
| **JOIN-01** | **Partial:** 79 initial discovery, safe solo/family switching and saved solo return have automated proofs; both iPads pass scoped leave/rejoin, and user confirmed the tested iPad's solo position returns. Android 83 reuses enrollment and auto-joins | Per-device and every-activity lifecycle/reconnect, physical prolonged-outage local qualification and full loading/preparation. G3, then G6/G7. |
| **WORLD-01** | **Partial:** four Windows players split/gather across two persistent logical areas; both physical iPads pass independent travel/reunion by user report | Six worlds, rooms/story zones, production scene loading, full four-device travel qualification and mobile host independence. G3–G7. |
| **WORLD-02** | **Partial:** garden travel leaves another player's movement/drag/cleanup intact | Apply to cooking, riding, books, hiding and all later activities; four-device checks. G3/G6/G7. |
| **ITEM-02** | **Partial:** authoritative pickup/contents/release, races and stale-command rejection | Carry approved portable props between areas and recover them across host loss/reunion; current tools stay in their station. G3–G5. |
| **ITEM-03** | **Partial, deployed in 83:** garden bucket/sponge idle returns use one authority, saved clocks, a five-second cue and held-item protection; completed flower/puddle can repeat. Older 79 clients see authority resets but not the new cue | Apple client update and broader device acceptance; full typed loans, home anchors, personal/creation protection, containers, spoken cue and host-recovery behavior. The garden fixture does not complete G5. |
| **STOCK-01** | **Partial constraint only:** garden tools stay in their area when a player travels | Per-player essential station stock, loan bounds and nested-container checks. One bucket per test area does not satisfy this feature. G5. |
| **ROOM-02** | **Not built** | Personal catalogs, loose-prop limits, clear exits, toy storage, protected creation shelf and safe visitors. G5/G7-A. |
| **NET-02** | **Partial:** four physical admissions and scoped iPad rejoin; persistent 83 live. Parent operations and ten-minute desktop run qualified; Windows 90 adds complete replicas, automatic local adventures and preserved work on reunion | Deploy guarded server operations and later client recovery; independent/portable recovery, sustained physical runs, remaining device/activity/outage acceptance. Boot/start supervision remains a deployment task. G3. |
| **REMOTE-01** | **Planned, not built:** owned VPS is the preferred future shared host after server readiness | Confirm host OS/resources; remote endpoint/platform/vault adapters, one-writer migration, startup/recovery/restore and mixed-device internet qualification. Offline solo and G4 iPad hosting/recovery remain. [Plan](implementation/vps-hosting-plan-2026-09-24.html). |
| **ROOM-01** | **Not built** | Two child-owned layouts, connected updates/visits and non-destructive offline reunion. G5/G6/G7-A. |
| **SECRET-01** | **Not built** | Both optional mini-door plush rooms, shared visits, exits, saved layouts and stars/aurora effects. G5/G6/G7-A. |
| **HIDE-03** | **Not built** | Enterable closets/drawers, optional 5–10-second clues and about-30-second parent search, tested for fun and no stuck avatars. G6/G7. |
| **TRAVEL-01** | **Partial:** isolated garden works offline on both iPads; Windows 90 automatically continues from a shared recovery checkpoint into a separate adventure | Mobile continuation qualification, full solo content/NPC substitutes and return-home reconciliation. Saved adventures are not merged yet. G3/G5/G7. |
| **AUTO-01** | **Partial:** physical 79/83 joining plus Windows 90 prolonged-outage continuation, cold adventure resume and safe automatic reunion with saved work accessible | Outage failure matrix, mobile recovery builds/qualification, physical server-absent initial joining, remaining lifecycle/route checks and parent pairing UI. G3. |
| **AUTO-02** | **Host migration not built:** complete replicas and separate local continuation are qualified on Windows 90 | Both iPads host, mobile checkpoint/event-tail qualification, election/handoff/hard-loss/partition/resume/reunion. G4/G5. |
| **OUT-01** | **Not built as outdoor content:** Creek is a second garden-rule fixture | Beach/creek/park activity designs, usable equipment, tag and hiding, with solo/co-op acceptance. G6/G7-B. |
| **DAY-01** | **Not built** | Sixth world, Calypso/all child characters, optional day with 2–3 saved invitations and independent participation. G7-C. |
| **LEARN-01** | **Not built** | Proposed 12 spoken reading/math/music/social/science stations with age-appropriate help. G7-C/F. |
| **IMG-01** | **Not built** | Picture play mat, roles/NPC substitutes and all nine requested stories, independent exit and progress. G6 foundations → G7-D. |

### Requirements outside the feature-ID table

| Requirement / content inventory | Current state and delivery path |
| --- | --- |
| **2D illustrated dollhouse** | Procedural canvas shapes prove interaction. Final drawn perspective, sprite rigs, authored floor polygons, depth/occlusion, furniture anchors and animation are still needed. G2 contracts → G6. |
| **3-year-old and 6-year-old assistance** | Tap/joystick and drag work; Voice and movement preferences persist. Simple Play / Explore & Stories, forgiving picture controls, per-child help and actual child playtests remain open. |
| **Menu, settings and stage wheel** | Prototype Menu exists. Main menu, six-circle destination picker, language/assistance/audio settings and parent setup are not finished. Avoid exposing unfinished worlds as working buttons. |
| **Six worlds** | Heeler Home, Backyard Garden, Creek, Playground & Park, Beach and Daycare remain the goal. Garden/Creek fixtures are not two finished worlds. |
| **English first; Spanish supported by the design** | Five local test narration clips exist; ordinary play has no live AI dependency. Stable localized content IDs, approved scripts/voices, character speech presentation and reviewed Spanish remain ahead. |
| **Kitchen and outdoor batches** | Five pizzas + five cakes + five meals are required. The research catalogs 10 beach, 10 creek and 12 park activities. Produce and test them through reusable activity families, not separate bespoke systems for each entry. |
| **Books, dinosaurs, science, cleanup and learning** | Preserve the proposed inventories: six dinosaur books, 20 dinosaur toys, eight science stations, five cleanup families and 12 learning stations. These are research production targets, not accepted finished assets; approve scripts/educational content before recording whole batches. |
| **Nine requested imagination stories** | Calypso, Helicopter, Wild Girls, Early Baby, Typewriter, Mums and Dads, The Adventure, Space and Explorers. Each needs an explicit role/solo/co-op/exit/save plan; none is implemented. |
| **Personal creations and borrowed items** | Automatic idle returns were selected by the user. They must protect personal decorations/creations and keep shared stations usable; a scene reset is not an acceptable substitute. |
| **Stability and delivery** | Scoped save/restart/update proofs exist. Sustained device profiling, interrupted loading/media, all-content offline checks, independent backups and automatic renewal remain required. |

### Temporary implementation choices and gaps to resolve

Code files below are under `Unity/FamilyPlayset/Assets/FamilyPlayset/Code`; the audit index records their exact relative paths and hashes.

1. **The rule boundary is sound, but content is still hardcoded.** `SoloWorld.cs`, `FamilySession.cs` and `SharedMovement.cs` keep world rules separate from Unity presentation. Current validation expects five solo or ten shared props and two named areas. Add versioned content definitions and migrations before multiplying recipes/worlds.
2. **The old loopback demo and native family path have different scopes.** The original Windows preview uses lab credentials. The family path now has protected parent enrollment, native Bonjour/Android NSD and certificate-validated encryption; all four real mobile clients were admitted on 79, followed by Samsung 83 on persistent server 83. Initial offline-to-family transitions and established retries have scoped proofs. Parent status/safe operation is qualified in isolated 84 and runtime deployment is pending; parent pairing/setup, full device lifecycle/route coverage, mobile prolonged-outage local qualification and host recovery remain open; Windows 90 now qualifies the basic local continuation/reunion path. [Joining proof](implementation/g3-offline-join-2026-09-24.html), [physical phones/four-device admission](implementation/g3-phones-79-2026-09-24.html), [current deployment](implementation/g3-persistent-server-2026-09-24.html).
3. **Rendered views and recovery checkpoints are now separate.** The deployed 79/83 clients still have receipt-free views. Windows 88 additionally transfers and persists complete authority checkpoints with the prototype's retained 128 receipts and idle clocks. `RecoveryReplica.cs` validates lineage/order and uses `CheckpointStore.cs` for staged durable writes. Windows 90 adds separate durable branch transition/resume/reopen support. It is not a complete operation journal, election or conflict merge. Transfer cadence and outstanding actions mean hard loss can recover an older coherent checkpoint rather than the last rendered frame. Mobile qualification remains open.
4. **Area independence is implemented before streaming optimization.** The server sends full small-world snapshots and clients filter the displayed area. Final content needs area interest, loading/memory ownership and lightweight offscreen activity/NPC simulation; do not render all six worlds on the A10.
5. **The active return policy is limited to garden fixtures.** Station tools stay in their area on travel. Authority/Android 83 also return idle tools after 180s + 5s cue and rearm completed stations after 60s + 5s cue; held/use interactions and partial progress are protected. Clocks pause while the world has no connected players, without stopping the server. Portable toys, containers, personal decorations, protected creations, stocked stations and arrival/spoken-cue policy are still unbuilt. Do not apply these fixture resets indiscriminately to later saved plants or bedrooms.
6. **Media and offline mode still live in separate fixtures.** `FoundationVideo.cs` has one local test clip; `SoloScreen.cs` runs the garden. Build 79 safely switches between the paired local garden and the shared world while preserving separate saves. Windows 90 adds automatic local adventures and preserves both those adventures and the original solo draft on reunion. Media overlays, mobile continuation qualification and reconciliation remain open.

7. **Persistent hosting is deployed; everyday operations remain open.** Build 83 removes the home-server cutoff while retaining timed probes and checkpoint/lock behavior. Its controlled deployment preserved all original saved fields. Samsung 83 joined automatically using its existing enrollment; the server stayed available after it left. Six native cases and 52 core checks passed; the deadline test uses a verification offset, not a real two-hour soak. Windows sign-in startup and a crash supervisor are not installed. [Evidence](implementation/g3-persistent-server-2026-09-24.html).

These are bounded prototype choices and explicit unfinished work, not reasons to restart from scratch. Preserve the tested rules, saves and movement; qualify the next dependency before expanding content.

### Documentation corrections made by this audit

- Recorded the successful **four-window human check** without calling it physical-device acceptance.
- Removed the incorrect claim that two owned bedrooms already had test fixtures; no bedroom implementation exists yet.
- Replaced the stale initial setup queue and G1-only headline with the actual G1/G2/G3 position, preserving open gates.
- Updated coverage from 54 to **55 chapters**, preserving all 35 goal IDs and adding the owned VPS milestone after server readiness.
- Replaced stale “mobile multiplayer pending” and “staged timers” summaries with actual four-device 79 evidence and applied PC/Samsung 83 evidence. Apple clients remain 79; updates are still pending there.
- Recorded the removed server cutoff without claiming a real two-hour soak, boot/crash supervision or VPS deployment. Historical pending-deployment JSON remains historical, not current status.
- Kept the test TV/Creek distinct from completed content; client snapshots/restart remain distinct from iPad hosting and host migration. The full personal-world policy remains unbuilt.
- Made **G3-OPS-01** one bounded next task, with separate subsequent restore, crash/outage and sustained/device checks; no implementation or device intervention was performed during this audit.
- Recorded known Android/Mac versions, corrected the two iPads' OS mapping from device evidence, and clarified that Windows exports to Xcode on the Mac.
- Separated the original research findings and proposed package choices from current installed code and dated qualification. Historical reports remain evidence, not a second competing plan.
