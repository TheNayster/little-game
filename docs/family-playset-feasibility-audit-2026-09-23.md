# The Family Playset — feasibility audit

**Technical follow-up:** [Read the deeper source-backed review](family-playset-technical-research-2026-09-23.html). It verifies released NGO shutdown behavior, independent-area examples, native discovery and template limitations. Automatic PC-server joining is the normal flow; iPad hosting/recovery stays required. Bluetooth was removed from the plan at your request.


September 23, 2026 · Design and technical audit · No game build has been tested

**Yes: this game is technically achievable in Unity.** The illustrated dollhouse, rich toys, optional activities, speaking characters, personal rooms, and four-player iPad/Android play are compatible ideas. The full plan is a substantial custom game, however. Its hardest work is dependable automatic multiplayer and preserving children's creations through interruptions; the quantity of finished artwork, animation, and audio is the other major workload.

**Your latest clarification is preserved:** iPad hosting, automatic joining, automatic host switching, independent exploration, and shared items remain **required**. Only multiplayer connectivity **while traveling** is a lower-priority want. Complete solo play without the PC or internet remains required, including on trips. Deferring hotspot/remote-access work does not defer the normal-network host-recovery system.

[Back to the main illustrated research](bluey-game-research-2026-09-23.html#52-feasibility-audit-and-current-priorities) · [Editable audit](family-playset-feasibility-audit-2026-09-23.md)

## 1. What I audited and what exists today

I reviewed all **51 existing main-guide sections**, the complete Toca/Piknik supplement, and the complete iPad/build supplement. I checked their requirements against one another, inspected the saved evidence indexes for recorded gaps, and rechecked pivotal engine, networking, device-lifecycle, and installation claims against current primary documentation. The coverage table below accounts for every section. This was not a fresh independent verification of every episode sentence, educational fact, repository line, or external URL.

The new game folder currently contains research, reference catalogs, page generators, and project-boundary instructions. It does **not** contain this game's Unity project, playable scenes, completed character rigs, generated voice library, working server, or tested device builds. The illustrated page is a research interface, not the game. The unrelated `Meeps game` project and its connector shortcut do not establish implementation progress for this game.

Evidence has three different levels: an official API can establish that an engine supports something; a design can explain how we intend to combine those APIs; only a working build on the actual devices establishes that our combination works well. Much of the current report is a good design proposal at the second level.

## 2. Feature-by-feature verdict

**Buildable** means there is an ordinary, credible implementation path. **Needs early proof** means feasible in principle but risky enough that we should demonstrate it before producing lots of content. Neither label means implemented.

| Feature you want | Verdict | What makes it achievable / what remains |
| --- | --- | --- |
| Flat cartoon dollhouse, six-world wheel, menus and settings | Buildable | Orthographic 2D scenes, layered art, large touch controls, local camera and menu state. Blender is optional for this style. |
| Any available character, change during play, duplicate favorites, playable parents | Buildable; substantial art | Keep player identity separate from the visual rig. Test tiny and large bodies, seating, hiding, carrying, and switching before expanding the roster. Portrait references are not animated assets. |
| Joystick or tap walking, with dragging in either mode | Buildable; child usability test | One input router decides which finger owns the joystick, item, floor, or menu. Simple Play allows direct object use without precise walking. |
| Toca-like interactive objects | Buildable; foundation work | Authored capabilities and state changes: fill, pour, wash, cut, combine, attach, carry. Stylized liquid and dirt effects can feel rich without fluid simulation. Compatibility must be authored; every possible pair cannot automatically have a meaningful result. |
| Optional quests, easy start/stop/change | Buildable | World objects work independently of quest prompts. Leaving releases participation while preserving accepted work. |
| Five pizzas, five cakes, five meals and cleanup | Buildable; content-heavy | Shared preparation/decoration/serving rules, with distinct recipe art and tested cancellation at each step. Finished food remains a toy. |
| Pond fishing, beach, creek, park and usable equipment | Buildable | Authored catches, paths, seats, placement sockets and gentle motion. Reuse fishing, collecting, pouring and construction across locations. |
| Roaming parents, hide-and-seek, closets/drawers and optional clues | Buildable | Authored NPC states and hiding anchors. A roughly 30-second search is a tuned goal in a small arena, not a guarantee across all worlds or moving hiders. |
| Books, spoken dinosaur names and small page interactions | Buildable | Local illustrated pages, hotspots and reviewed recordings; each device controls its own reading and audio. |
| TV clips with a thumbnail library | Buildable; media test | Supplied ordinary video files, local storage and one decoder per device. A YouTube page or its encrypted offline download is not an imported MP4. The library does not require an in-game YouTube browser. |
| Dinosaur toys, discovery mat and science stations | Buildable; reviewed content | Reuse drag/wash/container rules and small bounded simulations. Check factual narration and pronunciations; these ideas do not establish measured educational benefits. |
| Separate bedrooms and secret plush/aurora rooms | Buildable | Stable room IDs, ownership of decorating, safe placement and lightweight 2D effects. Both children can visit either existing room. |
| Daycare's optional day, learning and nine imagination stories | Buildable; especially large content batch | Saved activity selections, local spoken invitations and reusable role/session systems. Nine complete stories still require individual artwork, pacing, interactions and testing. |
| Up to four people, Android mixed with iPads/iPhone | Buildable; needs device proof | Same game protocol and content IDs across platform builds. Four human slots; a dedicated PC consumes no human slot. Unity NGO documents iOS and Android support. [Platform support](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/index.html) |
| Everyone goes anywhere independently; friends meet in the same area | Buildable; needs early proof | Separate persistent world data from each client's loaded artwork. Enter the existing zone, not a new copy. A mobile host must keep guests' offscreen areas running. |
| One bucket, one holder; visible pickups, carrying and dropping | Buildable; needs early proof | Authority orders actions, grants one reservation and records transfers once. Room changes and disconnects must preserve the same object identity. |
| Automatic item returns and stocked rooms | Buildable; cross-system testing | Categories distinguish shared loans, essential tools, personal belongings and saved creations. Timers alone cannot prevent hoarding. Containers, pending actions and creations need explicit handling. |
| Automatic discovery and joining without a child Host/Join menu | Needs early proof; required | Platform discovery, initial parent pairing/permissions, authentication, compatible versions and one session controller. Discovery finds devices; it does not migrate a world. |
| Both iPads host; automatic host switching and reunion | Needs early proof; required; highest engineering risk | Custom saved state, successor selection, ordered handoff, hard-failure recovery and reconciliation. It is not an NGO checkbox. |
| Every solo-capable activity without internet or PC | Buildable; required | Same local rules, bundled art/audio, installed clips and NPC substitutes. There can be no dependency on a live voice model or cloud login to play. |
| Hotspot co-op / remote access to the home PC while traveling | Optional later qualification | Preserve the existing research; test the actual network only after core multiplayer works. Failure here must not prevent solo play. |
| AR | Optional separate experiment | Not needed for the selected dollhouse style. Do not load AR packages/camera work into the first ordinary-play milestone. |
| Free automatic iPad installation renewal | Plausible deployment workflow; must verify | Keep the agreed free plan. Successful recurring refresh and preserved saves need proof on both iPads; installation expiry is separate from offline gameplay. |

## 3. The limits that must remain honest

### Automatic host switching is possible; a vanished host cannot guarantee an invisible handoff

Ordinary joining, leaving an activity, changing rooms, or closing a **client** while the PC continues should not restart or pause the remaining players. That remains the target.

If the **hosting** iPad is force-closed, loses power or becomes unreachable, survivors must first detect that loss and restore a consistent state. A brief recovery transition and uncertainty about the final unreplicated action are possible. We can design automatic recovery without a lobby or manual Host button; we cannot certify zero delay or preserve an action nobody else received. iOS also suspends ordinary background apps, so an iPad left in the app switcher is not a dependable always-running server. [Apple background execution](https://developer.apple.com/documentation/uikit/extending-your-app-s-background-execution-time)

Unity explicitly separates host election from migration of game data. Its documented default session data migration is for Netcode for Entities; it directs NGO users to Distributed Authority for migration-related operations. That cloud-oriented alternative does not demonstrate our offline/LAN requirements. Our proposed NGO architecture therefore needs its own recovery implementation, with the package choice revisited if the prototype fails. [Unity host migration](https://docs.unity.com/en-us/mps-sdk/session-host-migration)

### Offline worlds can preserve work without magically agreeing about every object

Two disconnected devices can each use their local copy of a bucket. They cannot enforce a single live holder across a missing connection. On reunion, one common world must be established, shared stock normalized, safe independent edits imported, and conflicting creations retained as separate saved variants.

The current plan's proposal to keep a conflicting activity in a private continuation instance needs tightening: it must not quietly create two ordinary playgrounds while claiming both players occupy the same playground. Prefer importing a creation into an available personal work tray, or keeping both variants in recovery storage. Explicit story/workspace instances need distinct identity and an understandable way to visit. This is a proposed resolution to prototype; arbitrary conflict-free reunion with no visible change is not established.

### The older iPad is a target, not a passed benchmark

Unity 6.3's published player requirements include iOS/iPadOS 15+, A8 or newer and Metal. Your A10 and A13 iPads meet that baseline. This supports using Unity; it does not prove our busiest scene or four-player A10 hosting performance. [Unity 6.3 requirements](https://docs.unity.com/en-us/engine/6000.3/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements)

Keep the 30 FPS target, load only the local view's detailed art, and retain compact simulation for other occupied zones. Profile an A10 host with four people in different areas, then with everyone together, and while it opens a book or video. Those are different workloads. The report's 400 MiB goal, NPC counts, sprite sizes, 32-prop room budget and snapshot intervals are hypotheses, not measured safe limits.

### A local game still needs a valid iPad installation

Apple's free Personal Team provisioning expires after seven days. Sideloadly documents automatic refresh when the paired device is reachable over Wi-Fi or USB; automation reduces manual work but does not make the installation permanent. Verify a real recurring refresh, app launch, and preserved saves. Longer offline trips still need valid signing even though travel multiplayer is optional. No paid subscription is being added to the plan. [Apple Personal Team limits](https://developer.apple.com/help/account/basics/about-your-developer-account), [Sideloadly refresh documentation](https://sideloadly.io/faq)

## 4. Findings and corrections to the existing research

| Finding | Impact | Resolution |
| --- | --- | --- |
| Section 18 still said shared stage travel; the Toca supplement repeated a same-stage first version | Contradicts required independent movement | Replace those statements with a clearly labeled one-room prototype boundary; independent zones are an early completion gate. |
| The Toca supplement still had five worlds; section 14 said “other four” after the plan grew to six | Inconsistent navigation/content scope | Update the supplement to six including Daycare, and make the main content milestone six locations. |
| Section 12's “two player avatars / one active location” budget | Could lead to disabling guest zones on a host | State up to four human avatars per view, with a separate logical simulation budget for every occupied zone. |
| The opening said the bucket scene would prove the hardest systems | A normal two-player pour does not prove recovery, reconciliation, or four-zone hosting | Call it a first interaction proof, followed by explicit networking and interruption gates. |
| Earlier platform/setup instructions described the unrelated old project as “your project” | Risks importing or connecting the wrong project | Label observations and connector commands historical. Create this game's Unity project separately in the new home. |
| Unity patch and package numbers were discussed as if one finished combination existed | No new project or lockfile establishes that combination | Retain Unity 6.3 LTS as the candidate branch; qualify one exact editor/renderer/Input/NGO/Transport set on Windows, Mac, iOS and Android before content production. |
| Independent-zone visibility can hide the only copy of recovery data | A new host could lose offscreen rooms | Keep the full compact recovery model separate from view-based replication. Unity's visibility hides client representations; it is not a world backup. [Object visibility](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/object-visibility.html) |
| Per-player travel cannot be a global scene replacement | A room change could move or unload everybody | Keep a persistent world/network root and registered dynamic entities; manage local presentation readiness explicitly. [Custom scene management](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/custom-management.html) |
| “Automatic” could be mistaken for no setup or permissions | Apps cannot silently grant OS network access | One-time parent pairing and local-network permission, then automatic child use. Use declared Bonjour services rather than an unqualified raw-broadcast template. [Apple network privacy](https://developer.apple.com/documentation/technotes/tn3179-understanding-local-network-privacy) |
| Private continuation copies versus one shared ordinary location | Returning players might be connected yet unable to meet | Preserve conflicting work explicitly; never silently fork a normal room with the same identity. Test this at reunion. |
| Idle returns versus “all work stays” | A shared plate return could erase its cake; a saved fort could permanently reserve all tools | Classify every object and its contained items. Transfer protected creations safely before returning equipment; enforce loan and station rules separately. |
| Two-tool examples persisted after four-player expansion | Players could be blocked by content authored only for two | Treat those as examples, and qualify sufficient work surfaces/tools/roles for four; do not duplicate an already held unique item. |
| “All characters” and “all activities” could imply completed assets or an exhaustive show catalog | The effort and current status would be understated | Keep the roster extensible and the show catalog a backlog. There are 34 portrait entries, including one group image, not 34 finished rigs. Pretzel's portrait remains unverified. |
| Many content counts were research proposals | Adding their totals exaggerates the number of distinct game systems | The 15 recipes are an explicit minimum; six books, 20 toy types, eight science stations and other catalog sizes are proposed content batches. Reused fishing or counting is not a new engine each time. |

The selected 6000.3.24f1 patch is real and its release notes list a 2D Renderer/Bloom black-screen issue. Keep Bloom disabled in the first renderer test and recheck the available 6.3 patches before creating the project. “LTS” does not prove fastest or bug-free. No editor installation is changed by this audit. [Official release notes](https://unity.com/releases/editor/whats-new/6000.3.24f1)

The saved evidence also records two direct HTTP 403 results for Head Start, with separate successful web-review notes; these are access-method limitations, not proof the articles are absent. Repository evidence is source inspection, not import/build certification. No complete GitHub template has been qualified for the whole requested game.

## 5. One coherent architecture to prove

The existing design is strongest when it follows this flow:

**Local touch → requested action → current world authority → committed state → views, audio and durable save.**

Run the same world rules locally for solo play, on either iPad for hosting, and on the Windows PC as a dedicated server. Keep profile, character appearance, room ownership, object reservation and host authority as separate identities. This lets a child switch from Bingo to Bandit without changing their bedroom, bucket or hiding role.

Use native service discovery behind a small common adapter. Android's NSD supports DNS-SD service discovery across platforms; the game still supplies trust, protocol compatibility and session selection. [Android NSD](https://developer.android.com/develop/connectivity/wifi/use-nsd)

Keep an established healthy host stable. Choose the PC when initially available; otherwise choose an eligible iPad. Transfer automatically when required, with a coherent state checkpoint. A faster device appearing is not a reason to repeatedly restart a healthy session. Simultaneous launches, returning hosts and network partitions need one controller with explicit generations and bounded retry behavior.

Persist accepted object operations, room edits, creations and activity checkpoints. Separate small recovery state from big textures, voices and videos. Delay expensive content expansion until the same save/action model survives late joining and host loss. Do not build one game for solo and rewrite its rules for multiplayer later.

Author activities from reusable families: containers/pouring, preparation, cleaning, decorating/building, collection, traversal/seating, discovery, role play, narration and picture matching. Reuse those mechanics while still budgeting individual activity art, sound, pacing and cancellation. NPC routines and prerecorded speech are enough; no live LLM is required for ordinary play.

## 6. The proof plan before producing the full game

These are implementation gates, not permission requests or claims that tests ran. A small milestone deliberately has fewer finished scenes; it does not remove the final requirements.

| Gate | Smallest useful build | What must be demonstrated |
| --- | --- | --- |
| 1. Device/build path | Fresh 2D project, simple touch scene, build number, tiny save | Native release launches on both iPads; Android build works; exact Mac/Xcode/package matrix recorded; stable identities and in-place update work. Begin free-renewal verification here. |
| 2. Feel and objects | Two characters, bucket/tap/plant, sponge and toy | Both input modes, direct Simple Play use, meaningful reactions, safe cancellation, local save and cold offline launch. Let both children try it. |
| 3. Shared world | Two simple areas, PC host, both iPads; add Android then iPhone | Automatic paired joining, four people, independent travel, one bucket holder, late arrival and client departure while another keeps playing. |
| 4. Required mobile authority | Same tiny world, PC absent, either iPad hosting | Either launch order, simultaneous launch, sticky authority, orderly host transfer and hard-close recovery. Add four-player and offscreen-zone stress to the A10-host test (host player plus three clients). |
| 5. Recovery and reunion | Two editable rooms, cake/creation, held bucket | Reconnect after disjoint and conflicting edits, repeated operation delivery, host return and network partition. No duplicated communal stock, lost protected creations or invisible duplicate ordinary rooms. |
| 6. One polished family area | Home/backyard slice with one cooking activity, hiding spot, narrated book, dinosaur and returnable loan | Finished art/audio, young-child usability, personal room ownership, safe idle returns, one video test, and sustained release performance while hosting. |
| 7. Content batches | Expand to six worlds, wider roster, recipe variants, daycare and stories | Every finished activity has solo, join/leave, cancellation, narration, save and contention coverage; complete English, then reviewed Spanish. |
| 8. Family deployment | The same tested release, saved worlds and media | Renewal, updates, recovery backups, server startup, crash symbols/logs and offline checks. Qualify travel multiplayer separately if desired. |

Record frame-time spikes and memory trends, not only average FPS. Exercise rapid touches, stalled loads, low storage, interruptions during saves, late join during object use, repeated area changes and host loss. Use automated tests for state invariants and device sessions for native networking, touch, heat, decoder behavior and renewal. Do not claim a game can never crash; make failures diagnosable and saves recoverable.

The most important early go/no-go is **A10 mobile hosting with independent zones and automatic recovery**. If it fails, first reduce offscreen simulation cost, animation detail, memory and load bursts. If it still fails, explain the measured limitation and revisit the architecture with you; do not silently change “both iPads can host” into PC-only play.

## 7. Workload and things that are still unknown

This is realistic as a staged family project. The complete commercial-style experience described here is not a small template customization or a weekend build. No defensible calendar estimate has been measured. The earlier many-months/year-scale wording was a planning impression, not a research result; estimate production only after the implemented slice and asset workflow have measurable throughput. A smaller enjoyable version can arrive much earlier. Gates 1–5 should determine whether the riskiest requirements work before we commit to producing the entire catalog.

The main production costs are:

- Consistent layered environments, depth sorting, hand/seat anchors, different body rigs and many state illustrations.
- Networking, durable state, host recovery, conflicting saves and lifecycle handling across three operating-system families.
- Distinct and intelligible character speech, auditioning, pronunciation review, local audio behavior and Spanish adaptation. English phonics/rhymes cannot simply be translated word for word.
- Testing combinations: avatar switching while hiding or riding, pouring while disconnecting, returning a plate with a cake, and entering a room while its objects change.

Still unknown: the Mac's installed macOS/Xcode, the Android phone's installed OS, available storage on the iPads, exact package/plugin compatibility, A10 hosting measurements, voice quality, and how the children respond to the controls. These are prototype/setup checks, not reasons to discard the idea.

The first release should contain finished, usable activities rather than visible buttons to unbuilt content. Keep the full feature backlog in this research and expand from working batches. That production sequence preserves the intended game instead of promising all of its content before its foundations are proven.

## 8. Audit coverage register

Every numbered section of the original main report is covered below. The verdicts refer to feasibility and consistency, not implementation completion.

| Main sections reviewed | Subjects | Audit conclusion |
| --- | --- | --- |
| 1–3 | Requirements, child experience, six worlds and menu | Coherent; keep six destinations and per-child assistance. Numerical usability timings are proposed. |
| 4–6 | Characters, touch and object rules | Credible foundation; substantial asset work and device touch tests remain. |
| 7–9 | LAN networking, speech and GitHub candidates | LAN/cross-platform path is credible; Bluetooth removed from scope; no complete template qualified; prerecorded speech is appropriate. |
| 10–12 | Architecture, saving, performance/crashes | Retain separated state and saves; qualify one package set; correct two-player/rendering assumptions. |
| 13–15 | Tests, milestones and unresolved choices | Useful tests; early recovery proof is missing from the original short milestone list. Signing remains a deployment gate. |
| 16–18 | Tracker, avatar changes and optional quests | Implementable; correct obsolete shared-travel sentence and apply stable identity throughout. |
| 19–22 | Cooking, fishpond, cleanup, parents/hiding | Implementable with authored states; protect creations, shared resources and free exits. |
| 23–24 | Show catalog and integration | Useful reusable backlog, not 32 completed systems; tie content to the tested activity model. |
| 25–29 | Books, dinosaur narration, TV, toys and integration | Implementable; imported media, reviewed recordings, storage and decoder tests remain. |
| 30–34 | Science, drop-in play, bedrooms, secret rooms and hiding | Implementable; resolve independent simulation, role continuity and per-hider timing. |
| 35–36 | Travel, offline play and milestones | Solo stays required. Travel network qualification is optional; ordinary mobile hosting/recovery stays required. |
| 37–39 | Beach, creek and park activities | Buildable content; reuse mechanics, bound physics/effects, and expand equipment capacity for four. |
| 40–43 | Daycare, learning, nine stories and integration | Coherent but large; optional schedules, NPC substitutes and independent story instances are necessary. |
| 44–46 | Discovery, iPad hosting, recovery and implementation | Required and highest technical risk; must prove on physical iPads before broad production. |
| 47–48 | Four players, Android and phone qualification | Documented platform support; actual mixed-device/hosting and phone-layout tests pending. |
| 49 | Remote home PC | Feasible candidate approaches; lower-priority travel want, not a gate for core multiplayer. |
| 50 | Shared world, independent travel, exclusive props | Sound central requirement; constrain scene lifetime and visibility; resolve conflicts with private continuation instances. |
| 51 | Automatic returns and anti-hoarding | Sound direction; classify all content, test nested containers and preserve creative work transactionally. |
| Toca/Piknik supplement, all 12 sections | Interaction inspiration, two assistance levels, object catalog and first prototype | Useful public-source synthesis, not reverse engineering; update stale five-world and same-stage wording. |
| iPad/build supplement, all 14 sections | Devices, engines, build path, signing, AR, reliability and connections | Useful platform background; explicitly separate old-project observations and superseded 3D/creature advice from this new 2D game. |

**Decision:** proceed with a fresh Unity 2D implementation in staged proofs. The vision is achievable; automatic hosting/recovery and the A10 workload must earn their claims through prototypes. Travel multiplayer remains optional. Core multiplayer, free exploration, interactive toys and complete solo play remain in the plan.
