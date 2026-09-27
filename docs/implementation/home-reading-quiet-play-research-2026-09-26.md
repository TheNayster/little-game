# Home books, plush play and device qualification: applied research

**Current implementation:** see the [original six-book phone preview and limits]( home-books-2026-09-26.md). Later user feedback requires full-screen pages, translucent controls, all spoken story words and Read to me; original research details below remain scoped to their earlier production stage.
**Book subjects — latest user correction, September 26:** the six books are **dinosaurs; snakes and reptiles; cars and trucks; Hello Kitty; Tangled; and unicorns**. The user explicitly rejected the assistant-selected space, fairy-garden, invented-princess and mermaid subjects. Use familiar Hello Kitty and Tangled characters/stories; Tangled follows Rapunzel, not an invented replacement princess. The specific unicorn story/character is awaiting the user’s reply. All four players can read all six. Rapunzel’s speaking voice remains the requested locally generated narrator reference. The later original-draft preview request authorizes finishing those six for review; neither story nor voice acceptance is implied.

September 26, 2026. Baseline dccc19d / Windows 146, schema 9/content 10/contract 11. Read current decisions, build-guide §18 and goal-sheet §§25–26, 32–33 before implementation. The user requested deeper research and its use, and asked where the books belong. This report specifies the next bounded implementation and the subsequent plush/device work without treating research as implementation evidence.

## Where the books go

**Latest user correction:** the book collection belongs **on the first floor around the living room**, so everyone has access. Provide one shared low face-out rack/reading nook beside the sofa. Do not seed separate bedroom or secret-room collections. Players may carry a physical book upstairs or into a secret room and read there. Keep stairs and walking paths clear. No new destination in the world chooser.

Start with one physical copy of **Hello, Dinosaurs!** in the shared living-room rack. A copy is a real movable saved object, distinct from the title/content ID. Existing toys and stored objects stay where they are; give books their own rack support slots. The rack also offers access to the installed title when its physical copy is being carried or has moved elsewhere. All four can read the same title independently; a world object lease never owns the reading cursor. This is our arrangement for this game, not a claim about the show's floor plan.

Finish and verify the first expanded dinosaur title, then complete the other five distinct books listed in the updated goal sheet. The former five dinosaur stories are superseded. Do not populate the rack with nonfunctional covers.

## Evidence and decisions for reading

| Primary source inspected | Finding | Decision used in this project |
| --- | --- | --- |
| [Takacs, Swart and Bus, original meta-analysis](https://pmc.ncbi.nlm.nih.gov/articles/PMC4647204/) | Story-related multimedia and unrelated interactive distractions must be distinguished; interactive additions can interrupt processing of the story. | One main dinosaur on introduction pages; deliberate name/feature interaction, no rewards, compulsory quiz, score or competing minigame. Six name portraits on the final choice page are navigation choices. Our hotspot count is a design choice. |
| [Unity 6.3 AudioSource.timeSamples](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSource-timeSamples.html) | Playback position is exposed in PCM samples. | Save a local page and clamped sample position with content revision; test actual shipped import/clip durations. No text-length guesses for highlighting or completion. |
| [Unity 6.3 UnPause](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSource.UnPause.html) | UnPause resumes a paused source; it does not start a stopped/new source. | Track explicit Play intent separately from AudioSource state. Stop old audio on page change and use a generation token so delayed loads/completions cannot revive it. |
| [Unity 6.3 audio unloading](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioClip.UnloadAudioData.html) | File-backed clip data can be unloaded. | Bound reader audio to the current narration and one name clip. Unload abandoned page/name resources and release reader illustration resources on close. Keep short local recordings, not live speech requests. |
| [Unity uGUI EventSystem](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.EventSystems.EventSystem.html) | Input maintains selection/raycast handling and a drag threshold. | Delay book pickup until a drag is recognized; a tap opens reading without first taking an exclusive world lease. A canceled touch never opens the book. Block room input behind the reader and cancel held movement when opening. |
| [Natural History Museum dinosaur directory](https://www.nhm.ac.uk/discover/dino-directory.html) | Species descriptions and pronunciation guides provide the factual reference. | Use six original factual introductions, separately sourced below. Friendly colours are illustration choices; do not imply all six species lived together or copy museum artwork. |

### First title content contract

Fourteen pages: cover, twelve creature introductions, goodbye. The additions are Brontosaurus, Spinosaurus, Pteranodon, Quetzalcoatlus, Parasaurolophus and Velociraptor. The existing original draft remains the basis. English prerecorded candidate narration is installed with the app; final voice/pronunciation acceptance still requires listening on device. Captions stay readable without audio. No narration starts merely on pickup/open. Large fixed Play/Pause, Replay, Previous, Next and Close controls; manual turns and optional Read to Me. Pause/close/page change supersede any earlier resume intent. Repeated dinosaur taps replace a pending name rather than creating a queue. One speaking source per reader; pause the story for a name, resume only if the current generation still intends playback. Stop other local hint speech and duck background music while speech is active. Do not globally pause the game.

| Introduction | Factual statement and art constraint | Source |
| --- | --- | --- |
| Tyrannosaurus rex | Two hind legs, small two-fingered forelimbs, balanced tail; show a gentle stepping response. | [NHM](https://www.nhm.ac.uk/discover/dino-directory/tyrannosaurus.html) |
| Triceratops | Three face horns and a neck frill; the three horns must be visible in the illustration. | [NHM](https://www.nhm.ac.uk/discover/dino-directory/triceratops.html) |
| Stegosaurus | Upright plates along its back; recognizable tail spikes. Do not claim a settled function for its plates. | [NHM](https://www.nhm.ac.uk/discover/dino-directory/stegosaurus.html) |
| Brachiosaurus | Long neck, front legs longer than hind legs and upward-sloping shoulders; distinguish it from Diplodocus. | [NHM](https://www.nhm.ac.uk/discover/dino-directory/brachiosaurus.html) |
| Diplodocus | Long neck and very long slender tail held above ground; avoid a Brachiosaurus-shaped silhouette. | [NHM](https://www.nhm.ac.uk/discover/dino-directory/diplodocus.html) |
| Ankylosaurus | Low armoured body and club at the end of its tail; gentle response without hitting. | [NHM](https://www.nhm.ac.uk/discover/dino-directory/ankylosaurus.html) |

Use original imagegen raster illustrations and retain prompts, sources and hashes. Runtime composition may reuse the same six species across introduction, cover and choice pages. This is a bounded illustrated book, not eight unrelated memory-heavy backgrounds. Preserve accepted Bluey/Bingo artwork. Small whole-body movement and feature cues must describe what the implementation actually does; do not claim a fully articulated dinosaur rig.

### Local reader state and interruptions

Key bookmarks by local stable profile plus content ID/revision, independent of world prop ID, chosen avatar and shared/private mode. Persist on page changes, pause/close and app lifecycle; persist periodic playback progress at a bounded interval. Resume on an explicit Play tap after opening or returning from background. Clamp invalid/missing page/sample values. Preserve an older safe bookmark if a write fails. Never add playback cursors to shared world state or import them through offline merging.

Changing a world session may rebuild room UI. Retain the reader's local state, rebuild its overlay, pause on loss of focus and allow continuing to read installed pages during a network outage. Never retain a stale world-object reference after reconnect. Closing restores fresh authoritative room presentation. Name playback must not restart a page after pause, close, page change or content switch. Simultaneous readers may sit in existing four-place cushions/sofa without reserving all seats; opening from a standing position is also allowed.

The physical book requires additive schema-10 migration and a matching content/wire version: keep all prior rooms, secrets, toys, receipts and enrollment, append exactly one shared living-room book. Secret creation must not add extra copies. Validate exact allowed IDs/counts, support coordinates, one holder, closed containers and no respawn. Test schema-9 restoration rather than assuming migration from a fresh world.

## Plush interactions: next bounded contract

The existing carry/storage lane already supplies persistent toy identities and a PC/VPS authority. Extend that contract instead of making duplicate decoration-only plush sprites. These are design decisions derived from this game's authoritative object constraints and [Unity's separation of local presentation and network authority](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/ownership.html); they are not claims that Unity supplies a finished stacking system.

1. **Cuddle pillow:** four independent nearby places around one generously drawn pillow. A child can sit with a selected plush; leaving releases only that child and settles the same plush. Holding and seating rules need an explicit combined contract, not an exception that creates two holders.
2. **Tuck in:** four authored little blanket supports. Drag the actual plush onto an available support, retain its ID and store tucked state, then draw the blanket in front. Pulling it out removes only its own tuck state. Leaving or hiding the entrance keeps all tucked toys intact.
3. **Piles:** save a parent/support relation with bounded depth, stable ordering and finite anchors. Validate same room, no cycles, one parent, no duplicate slot and no held child beneath a support. Moving/removing a lower toy must settle its dependents atomically to nearby clear floor positions; never scatter them through independent physics on four clients. A maximum three-toy stack is a proposed first implementation bound, not a research-derived optimum.
4. **Tea picnic:** four real cup/plate positions and bounded reusable pretend pieces. No recipe system dependency or unlimited respawning. Clear distinction between pretend props and later kitchen food. Invitations remain optional; no timer or forced cleanup.
5. **Aurora fact:** a deliberate picture tap may play a short original line explaining that particles interacting with atmospheric gases make light high in the sky. [NASA Space Place](https://spaceplace.nasa.gov/aurora/en/) supports this explanation. Use the same foreground voice arbitration; never start facts on room entry. The indoor scene is a fantasy projection.

Acceptance: four concurrent users, competing lease rejection, saved tucked/piled supports, missing/cyclic parent rejection, moving lower toys, room-layout change, owner tidy preserving arranged creations, occupied/hidden entrance, cold reopen, outage/private continuation and authoritative reconnect. Dedicated pillow/tuck/pile/picnic content remains open until these behaviors and usable art pass; the reader slice does not silently complete it. Hiding-round integration remains tied to the unbuilt hiding system.

## Device qualification: measurable procedure

[Apple's game-memory session](https://developer.apple.com/videos/play/wwdc2022/10106/) distinguishes allocations from actual memory footprint, including GPU resources; use Instruments/Game Memory and the Xcode memory gauge on the A10 iPad. [Android's memory guidance](https://developer.android.com/games/optimize/memory-reduction) likewise requires measuring and reducing actual memory use. [Android frame pacing](https://developer.android.com/games/optimize/frame-pacing) explains why smooth frame delivery matters beyond an average FPS. [Unity low-memory notification](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-lowMemory.html) is a cleanup signal, not a guarantee that a device will always warn before termination.

Apply now: bounded reader texture/audio residency, reused UI objects, no allocating animation pools per frame, explicit lifecycle pause/bookmark writes, and reader state/resource counters in native verification. Use the existing fixed-size frame trace rather than add periodic trace writes while playing. Keep current background cap at three. Release reader assets on close and inactive caches on low-memory warning.

Physical matrix when devices are available: A2197 iPad 7/A10 first, then A2602 iPad 9 and Samsung in a four-client session. Record exact build/source/signing, model/OS, room/content population, initial thermal conditions and charging state. Use a production-equivalent release for behavior and separate instrumented profiling if needed; do not present instrumented timings as release results.

- Baseline cold launch; then 30 minutes cycling four bedrooms/secrets, all eight book pages, repeated close/reopen, local narration, plush storage and simultaneous other-player movement. Our 30-minute duration is a test design choice.
- Record frame-time median/95th/99th percentile, long hitches, peak/resident footprint and post-close settling, not only FPS. Compare idle/play/reader/aurora-still states. Check monotonic growth after repeated reader cycles, thermal deterioration and audio stutter.
- Interrupt during speech, a name cue, async load, drag, save and room transition: lock/background, audio-route change, phone interruption where available, low-memory signal, server outage/restart, Wi-Fi loss/rejoin and cold relaunch. No ghost touch, autoplay on return, lost bookmark, missing toy or shared-world rewind.
- Verify all four may read one title at different pages; one closes/travels without changing others. Keep playing while the server is absent; reconnect loads server objects while retaining local book/preferences. Check both duplicate favourite avatars and profile changes.
- Verify large physical tap targets, safe-area layout, caption legibility and reachability on both iPads. Desktop-sized screenshots establish composition only.
- Device updates remain fresh signed in-place installs with verified package/signature and retained saves/enrollment. Preserve backup/recovery compatibility. No unavailable device gets a passing result; no historical endpoint or server state is assumed live.

## Implementation order and traceability

Active slice **BOOK-01 / BK-01**: first real eight-page book, shared downstairs rack/copy, independent offline reader, speech/bookmarks/input isolation, additive migration and native qualification. Apply the above reader and bounded-memory findings in code and tests. Then take the plush interaction contract as the next separate implementation slice. Keep full Home scope, remaining five titles and physical qualification in the tracker. Each delivered milestone must link its source/art, observed tests and remaining limits.
