# The Family Playset — templates and packages

**Research checked September 23, 2026. Budget: free first; consider an optional purchase around $20 only when it clearly saves work.** No package was installed or purchased, and no Unity game was created during this review.

**Recommendation:** begin with Unity's **Universal 2D template**, reuse maintained packages, and build the family game's particular interaction, activity and shared-world rules. I did not find a complete template that already provides this combination of a Toca-style dollhouse, four separate devices, independent rooms, mobile host recovery and offline saves. The initial package budget can be **$0**. This is a researched candidate setup; it is not yet a device-tested combination.

[Back to the main plan, section 54](bluey-game-research-2026-09-23.html#54-templates-packages-and-the-free-starting-setup) · [Networking research](family-playset-technical-research-2026-09-23.html) · [Feasibility audit](family-playset-feasibility-audit-2026-09-23.html)

## 1. What to start with

Unity's Universal 2D template creates a project with URP and its 2D renderer already configured. It supplies the rendering foundation, rather than finished gameplay. That is a good fit for illustrated rooms, layered characters and draggable props. Start with simple sprite materials and restrained effects on the older iPad. [Unity 6.3 template documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/creating-a-new-project-with-urp.html)

Use **Unity 6.3 LTS** as the candidate editor line. The previously reviewed **6000.3.24f1** patch is a concrete baseline, not a promise that every package combination is stable. Its release notes identify a 2D-renderer/Bloom black-screen issue, so leave Bloom out of the initial setup and review relevant fixes before freezing the editor version. Keep Windows and Mac on the same chosen patch. [6000.3.24f1 release notes](https://unity.com/releases/editor/whats-new/6000.3.24f1)

The first slice should contain one character, a room and garden, a tap, bucket, plant and sponge, both movement options, and a save. Then add a second area, shared item ownership, automatic PC connection and mixed-device play. Prove required iPad hosting and recovery before producing large amounts of content. Do not begin by importing every optional package below.

## 2. Template comparison

These are reuse decisions based on their documented design and inspected sources, not measured estimates of hours saved.

| Template or example | Useful material | Fit for this game | Decision |
| --- | --- | --- | --- |
| [Unity Universal 2D](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/creating-a-new-project-with-urp.html) | Configured 2D rendering foundation | Does not impose inventory, combat or room-travel rules | **Use as the new project base** |
| [Unity Playground](https://github.com/Unity-Technologies/UnityPlayground) | Small movement, collision and pickup examples; inspected project uses Unity 6000.0.66f2 | Its existing pickup behavior can take an item from another holder, contrary to our rule | **Learning/reference only**; adapt individual ideas |
| [Unity Boss Room](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop) | Networked co-op patterns; inspected sample uses Unity 6000.0.52f1 and NGO 2.4.3 | An RPG sample with different session and gameplay assumptions | **Networking reference**, not the whole game base |
| [Mirror additive-level examples](https://mirror-networking.gitbook.io/docs/manual/examples/multiple-additive-scenes) | Separately loaded areas and visibility patterns | Useful architectural evidence, but Mirror code does not plug into NGO unchanged | **Reference only**; choose one networking framework |
| [TopDown Engine](https://topdown-engine-docs.moremountains.com/multiplayer.html) | Character movement and a four-player local demo | Its bundled multiplayer is local play in one application; separate networked devices need additional work | **Do not buy as the multiplayer foundation** |
| [Adventure Creator](https://adventurecreator.org/forum/discussion/16881/feasibility-check-host-authoritative-co-op-layer-on-top-of-ac-eventmanager-mirroring-possible) | Adventure interaction and story authoring | Maintainer discussion describes custom co-op integration; blocking ActionLists change global cutscene state, which needs careful adaptation for independently playing children | **Not the preferred foundation** |
| [Chop Chop](https://github.com/UnityTechnologies/open-project-1) / [hsadler top-down template](https://github.com/hsadler/unity-2d-topdown-template) | Examples of event structure / grid placement | Discontinued older 3D project / older grid-building assumptions and BinaryFormatter saves | **Selected reference only**, not imports into production |

The earlier [main-plan source inspection](bluey-game-research-2026-09-23.html#9-github-templates-and-components-what-is-actually-useful) records repository revisions and additional limitations. An attractive demo or a “four players” label does not establish four iPads/phones, separate rooms, safe saves or host recovery.

## 3. Official free packages and exact candidates

The versions in this table were checked against **Unity's 6000.3 package catalog**. “Released for 6000.3” is Unity's published package status; it does not mean I ran the package on your devices. Core packages follow the editor. These are candidate versions to resolve and test together, not an already-created lockfile.

| Component | Checked version/status | What it saves us | When to use |
| --- | --- | --- | --- |
| [Input System](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.inputsystem.html) | **1.20.0**, released for 6000.3 | Touch handling, input actions, on-screen joystick components and examples | Initial setup |
| [Unity UI / uGUI](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ugui.html) | **Editor core package**; use its compatible TextMeshPro integration | Menus, large buttons, sliders, stage circles, picture prompts | Initial setup |
| [2D Animation](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.2d.animation.html) | **13.0.6**, released | Sprite rigging and reusable character animation | When introducing the first illustrated character |
| [PSD Importer](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.2d.psdimporter.html) | **12.0.2**, released | Layered character-art import | Only if our art export uses supported layered files; individual PNG pieces can start the project |
| [Cinemachine](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.cinemachine.html) | **3.1.7**, released | Camera following, confinement and transitions | When room scrolling needs it; fixed cameras can start simply |
| [Localization](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.localization.html) | **1.5.13**, released | English/Spanish string and asset tables | Establish stable text/audio IDs early; English content first |
| [Addressables](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.addressables.html) | **2.10.3**, catalog's designated released version | Controlled loading/unloading of rooms, books, voices and toys | Early content pipeline, with local groups for offline play |
| [Unity Transport](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.transport.html) | **2.7.4**, released | Network transport underneath NGO | Multiplayer proof, as a qualified dependency combination |
| [Netcode for GameObjects](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.netcode.gameobjects.html) | **2.13.2 editor-release baseline; 2.13.3 status needs resolution** | Connections, messages and object replication | First networking candidate; see section 4 |
| [Multiplayer Play Mode](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.multiplayer.playmode.html) | **2.0.2**, released | Multiple editor instances on one development computer | Development/testing aid; not proof of mobile performance |
| [Multiplayer Tools](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.multiplayer.tools.html) | **2.2.12**, released | Networking diagnostics and development tooling | During multiplayer qualification |
| [2D SpriteShape](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.2d.spriteshape.html) | **13.0.0**, released | Curved terrain/shape authoring | Optional; painted dollhouse backgrounds may not need it |
| [AI Navigation](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ai.navigation.html) | **2.0.14**, released | NavMesh tools | Evaluate only with our chosen walking-plane approach; it is not a complete illustrated-2D controller |

Do not select Addressables 4.x merely because a documentation link appears first. Likewise, a sample's older package set is not a reason to downgrade the entire game. Localization 1.5.13's registry manifest declares Addressables 1.25.0 and Newtonsoft JSON 3.0.2 dependencies; the proposed newer top-level Addressables version must be checked in the resolved project. [Localization registry metadata](https://packages.unity.com/com.unity.localization)

## 4. Networking stability: a real documentation mismatch

The **6000.3.24f1 editor release notes** list an NGO package update from **2.13.1 to 2.13.2**. That gives us an editor-aligned baseline to investigate first. The published manifests for NGO 2.13.2 and 2.13.3 both declare Unity **6000.0** and Transport **2.6.0**. An editor minimum and a dependency declaration are not a tested combination with our proposed Transport 2.7.4. [Editor package changes](https://unity.com/releases/editor/whats-new/6000.3.24f1), [NGO registry](https://packages.unity.com/com.unity.netcode.gameobjects)

The current 6000.3 catalog marks **2.13.3 as pre-release**, while the registry's latest tag and the GitHub release point to 2.13.3. This review does not guess which label should win. The previous technical review's “released v2.13.3 source” means a **published GitHub release**, not verified production status in Unity's editor catalog. [Catalog](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.netcode.gameobjects.html), [Published v2.13.3 release](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases/tag/v2.13.3)

**Decision:** retain NGO as the first framework candidate, inspect Package Manager status in the exact editor patch, compare the 2.13.3 fixes against our prototype, then freeze one passing version set. Do not install a moving development branch. Do not call either candidate “proven stable on our iPads” before native tests.

Even a fully qualified network package will not supply our complete automatic discovery, durable world model, room subscriptions, item-return policy, iPad host recovery or offline-save reconciliation. Those remain explicit game systems, as detailed in the [networking report](family-playset-technical-research-2026-09-23.html). The PC remains the normal host; either iPad hosting and automatic recovery remain required. Bluetooth stays removed.

## 5. Useful free additions

| Package | Evidence checked | Good use here | Recommendation |
| --- | --- | --- | --- |
| [DOTween Free](https://dotween.demigiant.com/download.php) | **1.3.030**, June 23, 2026; release includes a Unity 6000.3 initialization-warning fix | Button bounce, opening drawers, fish bobbing, book hotspots and gentle feedback | **Preferred small animation helper**, add when first interactions need polish |
| [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) | Publisher README specifies **1.4.11**; inspected revision July 18, 2026 | Alternative tweening library | Credible alternative, **not an additional dependency alongside DOTween** |
| [Yarn Spinner for Unity](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/releases/tag/v3.2.8) | **3.2.8**, August 11, 2026; MIT; release targets .NET Standard 2.1 | Branching teacher/parent dialogue and larger pretend adventures | **Later**, if dialogue complexity justifies it; simple quest cards do not need it |
| [Unity UI Extensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions/releases/tag/v3.0.0) | **3.0.0**, June 19, 2026; release explicitly focuses on Unity 6 | Scroll snapping/carousels and selected menu controls | **Optional**; six circular stage buttons are already practical with ordinary uGUI |
| [NavMeshPlus](https://github.com/h8man/NavMeshPlus/tree/16283ee1040e591b9a99ddf93f37e9af93925aaf) | Manifest **0.2.23**, Unity minimum 2022.3; MIT; inspected commit August 29, 2026 | Navigation surfaces built from 2D sprites/colliders for tap-to-walk and roaming parents | **Pilot only**; no claim of verified Unity 6.3 + iOS compatibility |

For NavMeshPlus, pin the inspected commit rather than its README's moving master branch. It contains its own fork of navigation components, so check overlap before adding Unity AI Navigation too. For simple rooms, authored walkable paths may be less work than maintaining a complex runtime NavMesh. Choose after the first room test.

Yarn provides dialogue flow and presentation, not generated voices or multiplayer quest authority. Its Unity-localized line provider can use Unity Localization's text and audio assets. That lets English-first and later Spanish use the same stable line IDs. [Yarn Unity-localized provider](https://docs.yarnspinner.dev/components/line-provider/unity-localised-line-provider), [Voice-over sample](https://docs.yarnspinner.dev/yarn-spinner-for-unity/samples/sample-guide-voice-over-and-localisation)

## 6. One optional purchase within the budget

**DOTween Pro** is the paid candidate worth keeping on the shortlist. The listing showed **$15 USD regular price**, with a **$7.50 sale** during this check, before applicable taxes; prices can change. Listed version: **1.0.430**, June 23, 2026. Budget against the regular price, not a temporary discount. [Current publisher listing](https://marketplace.unity.com/packages/tools/visual-scripting/dotween-pro-32416)

Pro adds visual animation/path authoring and related tools. That could save repetitive Inspector/code work when making many doors, bouncing toys and decorative effects. Its path editor is not automatic obstacle-avoiding navigation. It does not replace cooking rules, saves or networking. [Publisher's Pro feature description](https://dotween.demigiant.com/pro.php)

**Buy nothing yet.** Use the free version for the first polished room. Consider Pro if visual authoring becomes a repeated bottleneck and you want to tune these effects yourself in Unity. Follow the publisher's upgrade process if that happens; avoid importing duplicate Free/Pro runtime copies. No evidence here justifies a measured “saves weeks” promise.

## 7. How the pieces fit the requested features

The following is our proposed integration design, informed by the sources above. These are implementation decisions rather than features promised by a template vendor.

| Requested behavior | Reuse | Game-specific work |
| --- | --- | --- |
| Joystick or tap-to-walk, plus dragging | Input System on-screen controls and touch APIs | Assign each finger to UI, walking or a prop until release; a prop drag must not also issue a walking command |
| Change character during play | 2D Animation, sprite libraries and Animator | Keep player/profile identity separate from avatar art; appearance change preserves activity and held-item state |
| Pour into a bucket, water plants, cook, clean | Colliders, sprite feedback, audio, DOTween presentation | Shared capability/recipe data; authority validates holder, target and quantities; one durable result per action |
| Roaming parents, hide-and-seek | Animator plus the selected navigation approach | Parent task states, hiding slots, timed hints, seeking rules and interruption/resume behavior |
| Books and dinosaur names read aloud | uGUI, Localization, locally stored audio | Page hotspots, narration sequence, large play/repeat buttons and child-friendly pacing |
| Personal rooms, secret rooms and six worlds | Addressables for local content loading | Stable room IDs, independent travel, subscriptions, save ownership and restoration |
| Four mixed-device players | One selected NGO/Transport combination | Automatic family discovery/joining, approved devices, shared item arbitration and host recovery |
| Offline play and safe room saves | Unity JSON serialization and persistent storage | Versioned save records, stable IDs, backups, journal/recovery policy and reunion conflict handling |

Input System 1.20's registry includes an **On-Screen Controls** sample and a **UI vs. Game Input** sample. The inspected joystick source also documents isolated input actions for avoiding unwanted cancellation during device/control-scheme switching. Use these as focused references, then test simultaneous joystick and prop touches on each tablet. [Official package metadata](https://packages.unity.com/com.unity.inputsystem), [Touch documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/devices-touch.html), [On-screen controls](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/on-screen-controls.html)

For saving, a small explicit data model serialized as JSON is a reasonable free start. Unity's serializer has structural restrictions, including no ordinary Dictionary serialization; use suitable records/lists or deliberately choose a compatible serializer. Save stable content IDs instead of runtime scene-object references. Persistent storage is only a location: it does not automatically provide backup, merging or crash-safe transactions. [Unity JSON serialization](https://docs.unity3d.com/6000.3/Documentation/Manual/json-serialization.html), [persistentDataPath](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-persistentDataPath.html)

Keep authoritative timers and state independent of presentation. A cake should finish according to game time/state even if its animation is not being rendered. The Windows dedicated server must run without a UI, sound playback or an animation completion callback. [Dedicated Server optimizations](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-optimizations.html)

## 8. What compatibility means for your equipment

| Target | Known baseline | Qualification needed |
| --- | --- | --- |
| iPad 7, A2197 / A10 | User reports iPadOS 18.6.2 | Primary performance target: touch responsiveness, memory, host workload and repeated room changes |
| iPad 9, A2602 / A13 | User reports iPadOS 18.7.10 | Same gameplay and save format; test as both client and host |
| iPhone 13 Pro Max, A2484 | User reports iOS 26.6.1 | Smaller-screen UI, permissions, reconnects and mixed-device play |
| Samsung SM-S948U1 | Model supplied; installed Android version still unknown | Native Android build, touch layout, resume behavior and network discovery; record installed OS before qualification |
| Windows PC | Main authoring machine and normal dedicated host | Build server independently of graphics/audio; verify firewall/discovery and saved-world startup |
| MacBook Pro 14-inch, 2021, M1 Pro, 16 GB | Hardware supplied; current macOS/Xcode versions unknown | Match editor patch and qualify native iOS export/build/signing tools |

Both iPads meet the previously checked Unity 6.3 baseline. That establishes an eligible target, not a performance guarantee for our entire world. Hardware, package APIs, native platform permissions, build tools and the final workload are separate checks. The Android OS and Mac software versions remain unverified; no complete end-to-end compatibility claim is justified yet. [Unity 6.3 system requirements](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html)

For travel, put required gameplay content, English voices and selected books/videos on the device. Addressables can organize local content; it does not require that our rooms download from a service. Use local groups deliberately and test a cold launch without internet. Remote/hotspot multiplayer remains an optional later goal.

## 9. Installation order and acceptance checks

1. **Create the isolated base:** new Universal 2D project under this game's folder, chosen Unity 6.3 patch, Input System and uGUI. Make native iPad and Android builds early. Keep the unrelated old project untouched.
2. **Prove one interaction:** bucket/tap/plant/sponge, finger ownership, joystick and tap walking, then a versioned save. Add DOTween Free only for visible feedback. Verify force-close/relaunch preserves committed progress.
3. **Add the first character and content pipeline:** 2D Animation; layered importer only if needed; English Localization assets; local Addressables loading. Check room transitions release unused resources and voices load offline.
4. **Qualify networking:** choose and lock NGO/Transport versions, use Multiplayer Play Mode and network diagnostics, then the PC server plus both iPads and phones. Simultaneous pickup must have one winner. A new join must see current item and room state.
5. **Test the required difficult cases:** PC absent, either iPad hosting, automatic discovery, planned handoff, abrupt host exit, resume, returning host, separate rooms and conflicting saved creations. Editor instances alone cannot pass these mobile lifecycle tests.
6. **Add optional packages only for observed needs:** Cinemachine for scrolling, navigation after the floor-plane decision, Yarn for substantial branching dialogue, UI Extensions for a useful specific control. Then grow activity content.

Before accepting a package set, record the exact editor patch, package manifest and resolved lockfile, native build settings and device results. Use a fixed Git tag or commit for Git dependencies. Keep editor-only tooling and client presentation separate from server logic. Re-run the affected native checks after an upgrade; do not auto-update all packages during feature work.

Physical-device acceptance must include release/IL2CPP builds, app suspend/resume, interruptions during a drag, repeated world travel, save/update preservation and the A10 host load. The existing **30 FPS target is a proposed acceptance target**, not a measured result. Measure memory and frame time with representative toys/NPCs/audio; no package badge can establish this for us.

## 10. Evidence, scope and budget decision

This pass checked Unity's editor-specific package pages, the public Unity package registry, selected package source, repository manifests/releases, publisher documentation and the DOTween listing. Raw research records live in [the package-review folder](bluey-research/package-review-2026-09-23/official-package-summary.json). The earlier template and networking source snapshots remain in the main research folders.

Freshly checked optional-package revisions include PrimeTween `3f06f369143fc896bc19cf346468818a0853ca11`, NavMeshPlus `16283ee1040e591b9a99ddf93f37e9af93925aaf`, UI Extensions release `v3.0.0`, and Yarn Spinner release `v3.2.8`. The registry copy of Input System 1.20.0 was inspected for its on-screen joystick source; only selected research files were saved outside any Unity Assets folder. No code was imported into a game.

**Adopt in the plan:** Universal 2D, a small official package set, DOTween Free when useful, explicit custom playset systems, and an early networking/version qualification gate. **Initial package spend: $0. Optional later purchase: DOTween Pro, budget approximately $15 before tax.** Keep the complete feature plan, but qualify the platform/network foundation before investing in all the rooms and mini-games.
