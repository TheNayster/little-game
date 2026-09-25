# The Family Playset — feasibility audit

**Current scope — September 25:** PC/VPS is the sole shared authority; four mobile clients join automatically. Offline solo stays private, and server state wins on reconnect without importing offline edits. G4/AUTO-02 device hosting is retired. [Current decisions](current-decisions.md) control scope; the [build guide](family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) records current implementation. Technical sources and candidate comparisons below retain their original research date; this update is not a new external-source verification.



September 23, 2026 · Design and technical audit · No game build has been tested

**Yes: this game is technically achievable in Unity.** The illustrated dollhouse, rich toys, optional activities, speaking characters, personal rooms, and four-player iPad/Android play are compatible ideas. The full plan is a substantial custom game, however. Its hardest work is dependable automatic multiplayer and preserving children's creations through interruptions; the quantity of finished artwork, animation, and audio is the other major workload.

**Latest decision:** shared play uses PC/VPS only. Four clients, independent areas and complete offline solo remain required. G4/AUTO-02 and automatic offline imports are removed; historical hosting experiments are not prerequisites.

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
| Everyone explores independently and meets in the same area | Buildable; scoped prototype proof exists | PC/VPS owns logical areas; each client loads its view without creating duplicate ordinary rooms |
| One bucket, one holder; visible pickups, carrying and dropping | Buildable; needs early proof | Authority orders actions, grants one reservation and records transfers once. Room changes and disconnects must preserve the same object identity. |
| Automatic item returns and stocked rooms | Buildable; cross-system testing | Categories distinguish shared loans, essential tools, personal belongings and saved creations. Timers alone cannot prevent hoarding. Containers, pending actions and creations need explicit handling. |
| Automatic discovery and joining without a child Host/Join menu | Needs early proof; required | Platform discovery, initial parent pairing/permissions, authentication, compatible versions and one session controller. Discovery finds devices; it does not migrate a world. |
| Device hosting and automatic switching | **Retired September 25** | No mobile authority, election or handoff gate |
| Every solo-capable activity without internet or PC | Buildable; required | Same local rules, bundled art/audio, installed clips and NPC substitutes. There can be no dependency on a live voice model or cloud login to play. |
| Hotspot co-op / remote access to the home PC while traveling | Optional later qualification | Preserve the existing research; test the actual network only after core multiplayer works. Failure here must not prevent solo play. |
| AR | Optional separate experiment | Not needed for the selected dollhouse style. Do not load AR packages/camera work into the first ordinary-play milestone. |
| Free automatic iPad installation renewal | Plausible deployment workflow; must verify | Keep the agreed free plan. Successful recurring refresh and preserved saves need proof on both iPads; installation expiry is separate from offline gameplay. |

## 3. The limits that must remain honest

### A dedicated server keeps running when a client leaves

Ordinary joining, leaving an activity, changing rooms, or closing a **client** while the PC continues should not restart or pause the remaining players. That remains the target.

No iPad is a shared server. A client closing, sleeping or losing its route leaves the other connected players unaffected. A server outage uses private client continuation and the server’s own recovery process.

The prior host-migration research is now outside scope. Retained checkpoints may help server recovery, but no peer election, mobile host adapter or automatic authority transfer is required.

### Offline worlds can preserve work without magically agreeing about every object

Disconnected devices may each use their own private bucket. Rejoining uses the server’s single canonical bucket and current room. Private edits and creations remain local; no automatic import or conflict resolution is required.

Connected players share one ordinary room identity. A private offline world is explicitly separate; it is not a second invisible server room. Preserve it separately through rejoin and updates.

### The older iPad is a target, not a passed benchmark

Both iPads meet the published Unity baseline; that does not prove sustained client rendering, local simulation or media performance. [Unity requirements](https://docs.unity.com/en-us/engine/6000.3/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements).

Measure A10 client and solo frame-time spikes, loading and memory, including books/video. Four-player views and separate areas remain test cases. Draft prop/NPC/memory budgets are hypotheses, not measured safety limits.

### A local game still needs a valid iPad installation

Apple's free Personal Team provisioning expires after seven days. Sideloadly documents automatic refresh when the paired device is reachable over Wi-Fi or USB; automation reduces manual work but does not make the installation permanent. Verify a real recurring refresh, app launch, and preserved saves. Longer offline trips still need valid signing even though travel multiplayer is optional. No paid subscription is being added to the plan. [Apple Personal Team limits](https://developer.apple.com/help/account/basics/about-your-developer-account), [Sideloadly refresh documentation](https://sideloadly.io/faq)

## 4. Findings and corrections to the existing research

| Finding | Impact | Resolution |
| --- | --- | --- |
| Section 18 still said shared stage travel; the Toca supplement repeated a same-stage first version | Contradicts required independent movement | Replace those statements with a clearly labeled one-room prototype boundary; independent zones are an early completion gate. |
| The Toca supplement still had five worlds; section 14 said “other four” after the plan grew to six | Inconsistent navigation/content scope | Update the supplement to six including Daycare, and make the main content milestone six locations. |
| Section 12's “two player avatars / one active location” budget | Could lead to disabling guest zones on a host | State up to four human avatars per view, with a separate logical simulation budget for every occupied zone. |
| A bucket pour was described as proving the hardest systems | A normal pour does not prove client loss/rejoin, persistence or sustained load | Keep separate scoped acceptance for those systems |
| Earlier platform/setup instructions described the unrelated old project as “your project” | Risks importing or connecting the wrong project | Label observations and connector commands historical. Create this game's Unity project separately in the new home. |
| Package candidate tables can look like an installed baseline | Can cause unnecessary reinstalls/upgrades | Use the actual manifest/lockfile and current-decision version record; historic research is not an upgrade instruction |
| Visual subscriptions are not backups | Hidden areas still need persistent server state | Server backups cover all logical areas; private continuation does not wait for a complete client replica |
| Per-player travel cannot be a global scene replacement | A room change could move or unload everybody | Keep a persistent world/network root and registered dynamic entities; manage local presentation readiness explicitly. [Custom scene management](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/custom-management.html) |
| “Automatic” could be mistaken for no setup or permissions | Apps cannot silently grant OS network access | One-time parent pairing and local-network permission, then automatic child use. Use declared Bonjour services rather than an unqualified raw-broadcast template. [Apple network privacy](https://developer.apple.com/documentation/technotes/tn3179-understanding-local-network-privacy) |
| Private saves versus connected rooms | Rejoin must not overwrite either namespace | Load current server world; keep private saves separate without imports |
| Idle returns versus “all work stays” | A shared plate return could erase its cake; a saved fort could permanently reserve all tools | Classify every object and its contained items. Transfer protected creations safely before returning equipment; enforce loan and station rules separately. |
| Two-tool examples persisted after four-player expansion | Players could be blocked by content authored only for two | Treat those as examples, and qualify sufficient work surfaces/tools/roles for four; do not duplicate an already held unique item. |
| “All characters” and “all activities” could imply completed assets or an exhaustive show catalog | The effort and current status would be understated | Keep the roster extensible and the show catalog a backlog. There are 34 portrait entries, including one group image, not 34 finished rigs. Pretzel's portrait remains unverified. |
| Many content counts were research proposals | Adding their totals exaggerates the number of distinct game systems | The 15 recipes are an explicit minimum; six books, 20 toy types, eight science stations and other catalog sizes are proposed content batches. Reused fishing or counting is not a new engine each time. |

The selected 6000.3.24f1 patch is real and its release notes list a 2D Renderer/Bloom black-screen issue. Keep Bloom disabled in the first renderer test and recheck the available 6.3 patches before creating the project. “LTS” does not prove fastest or bug-free. No editor installation is changed by this audit. [Official release notes](https://unity.com/releases/editor/whats-new/6000.3.24f1)

The saved evidence also records two direct HTTP 403 results for Head Start, with separate successful web-review notes; these are access-method limitations, not proof the articles are absent. Repository evidence is source inspection, not import/build certification. No complete GitHub template has been qualified for the whole requested game.

## 5. One coherent architecture to prove

The existing design is strongest when it follows this flow:

**Local touch → requested action → current world authority → committed state → views, audio and durable save.**

Reuse world rules in private solo and on the dedicated PC/VPS. Keep profile, appearance, room ownership and item holds separate; changing character never changes child identity.

Use native service discovery behind a small common adapter. Android's NSD supports DNS-SD service discovery across platforms; the game still supplies trust, protocol compatibility and session selection. [Android NSD](https://developer.android.com/develop/connectivity/wifi/use-nsd)

Clients discover and authenticate the designated server. Bound retries and ignore stale callbacks. There is no mobile host candidate selection; server unavailability leaves local play responsive.

Persist accepted object operations, room edits, creations and activity checkpoints. Separate small recovery state from big textures, voices and videos. Delay expensive content expansion until the same save/action model survives late joining and server route loss. Do not build one game for solo and rewrite its rules for multiplayer later.

Author activities from reusable families: containers/pouring, preparation, cleaning, decorating/building, collection, traversal/seating, discovery, role play, narration and picture matching. Reuse those mechanics while still budgeting individual activity art, sound, pacing and cancellation. NPC routines and prerecorded speech are enough; no live LLM is required for ordinary play.

## 6. The proof plan before producing the full game

These are implementation gates, not permission requests or claims that tests ran. A small milestone deliberately has fewer finished scenes; it does not remove the final requirements.

| Gate | Smallest useful build | What must be demonstrated |
| --- | --- | --- |
| 1. Device/build path | Fresh 2D project, simple touch scene, build number, tiny save | Native release launches on both iPads; Android build works; exact Mac/Xcode/package matrix recorded; stable identities and in-place update work. Begin free-renewal verification here. |
| 2. Feel and objects | Two characters, bucket/tap/plant, sponge and toy | Both input modes, direct Simple Play use, meaningful reactions, safe cancellation, local save and cold offline launch. Let both children try it. |
| 3. Shared world | Two simple areas, PC host, both iPads; add Android then iPhone | Automatic paired joining, four people, independent travel, one bucket holder, late arrival and client departure while another keeps playing. |
| 4. Offline transition and reconnect | Current tiny world, lose/restore the server route | Immediate usable private play without an old-save rewind; cold offline reopen; server state on rejoin, local work retained separately |
| 5. Persistent rooms and creations | Two editable bedrooms, saved cake and shared bucket | Connected server transactions and private saves; no duplicated stock, lost protected creations or automatic offline imports |
| 6. One polished area | Home/backyard slice with cooking, hiding, book, dinosaur and returnable loan | Finished art/audio, child usability and measured client/solo performance; isolated art preparation can start earlier |
| 7. Content batches | Expand to six worlds, wider roster, recipe variants, daycare and stories | Every finished activity has solo, join/leave, cancellation, narration, save and contention coverage; complete English, then reviewed Spanish. |
| 8. Family deployment | The same tested release, saved worlds and media | Renewal, updates, recovery backups, server startup, crash symbols/logs and offline checks. Qualify travel multiplayer separately if desired. |

Record frame-time spikes and memory trends, not only average FPS. Exercise rapid touches, stalled loads, low storage, interruptions during saves, late join during object use, repeated area changes and server route loss. Use automated tests for state invariants and device sessions for native networking, touch, heat, decoder behavior and renewal. Do not claim a game can never crash; make failures diagnosable and saves recoverable.

The key remaining foundation proofs are current-device offline continuity, responsive walking, server-authoritative rejoin, durable rooms/items and measured A10 client/solo load. Device hosting is retired. Character animation/import can advance independently while physical checks wait.

## 7. Workload and things that are still unknown

This is realistic as a staged family project. The complete commercial-style experience described here is not a small template customization or a weekend build. No defensible calendar estimate has been measured. The earlier many-months/year-scale wording was a planning impression, not a research result; estimate production only after the implemented slice and asset workflow have measurable throughput. A smaller enjoyable version can arrive much earlier. Gates 1–5 should determine whether the riskiest requirements work before we commit to producing the entire catalog.

The main production costs are:

- Consistent layered environments, depth sorting, hand/seat anchors, different body rigs and many state illustrations.
- Durable world state, server recovery and client lifecycle handling across the supported platforms; separate private saves without an offline merge engine.
- Distinct and intelligible character speech, auditioning, pronunciation review, local audio behavior and Spanish adaptation. English phonics/rhymes cannot simply be translated word for word.
- Testing combinations: avatar switching while hiding or riding, pouring while disconnecting, returning a plate with a cake, and entering a room while its objects change.

Known inventory and scoped tests now appear in the build guide. Still open: sustained A10 performance, full-content memory/loading, automatic renewal, independent recovery, voice quality and structured child usability. Do not repeat old unknown inventory as a current blocker.

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
| 35–36 | Travel and offline play | Solo required; remote internet multiplayer optional; private saves never merge into the server |
| 37–39 | Beach, creek and park activities | Buildable content; reuse mechanics, bound physics/effects, and expand equipment capacity for four. |
| 40–43 | Daycare, learning, nine stories and integration | Coherent but large; optional schedules, NPC substitutes and independent story instances are necessary. |
| 44–46 | Automatic PC/VPS connection and private continuation | Four clients join the dedicated world; device-hosting requirements retired |
| 47–48 | Four players, Android and phone qualification | Documented platform support; current-device sustained mixed play and layout qualification remain. |
| 49 | Remote home PC | Feasible candidate approaches; lower-priority travel want, not a gate for core multiplayer. |
| 50 | Shared world, independent travel, exclusive props | Sound central requirement; constrain scene lifetime and visibility; resolve conflicts with private continuation instances. |
| 51 | Automatic returns and anti-hoarding | Sound direction; classify all content, test nested containers and preserve creative work transactionally. |
| Toca/Piknik supplement, all 12 sections | Interaction inspiration, two assistance levels, object catalog and first prototype | Useful public-source synthesis, not reverse engineering; update stale five-world and same-stage wording. |
| iPad/build supplement, all 14 sections | Devices, engines, build path, signing, AR, reliability and connections | Useful platform background; explicitly separate old-project observations and superseded 3D/creature advice from this new 2D game. |

**Decision:** keep the existing Unity foundation and staged content plan. PC/VPS shared play, independent exploration, interactive toys and full offline solo remain. G4 is retired; scoped art preparation may proceed while focused device qualification waits.
