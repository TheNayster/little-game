# Daycare treasure hunt: The Windblown Map

The user selected Treasure hunt from the Daycare wishlist and requested research into existing games, followed by a playable separate world entered from the Daycare Games menu. Goal IDs: DAY-01, IMG-01, LEARN-01, FAMILY-01.

## Research and resulting choices

These are primary developer descriptions, not claims that I personally played the apps. Their characters, artwork, text and levels are not reused.

| Existing game / app | What its developer describes | Original Little Weeps adaptation |
| --- | --- | --- |
| [Sago Mini World: Treasure Hunt](https://apps.apple.com/us/app/sago-mini-world-kids-games/id874425722?platform=ipad), version 8.0 release note | Boats, island exploration, digging for treasure, hidden surprises and friendly sea creatures. [Sago Mini World](https://sagomini.com/world/) also describes self-paced open play. | A pretend island, discoveries beneath covers, gentle wrong-location finds, and a mound that reveals the final chest. No score, timer or punishment. |
| [Peg + Cat: Hungry Pirates](https://pbskids.org/games/play/hungry-pirates/96) and [PBS's launch description](https://www.pbs.org/about/about-pbs/blogs/news/pbs-kids-launches-app-and-online-games-for-new-series-peg-cat/) | Maps and clues lead to an X and buried snacks. [PBS's spatial-thinking guide](https://static.pbslearningmedia.org/media/media_files/9b1062ed-2fdf-4ac8-913f-7f7c879512c6.pdf) separately describes positional clues and help in Fizzy's Pantry Hunt. | A visible island map with three landmarks, optional spoken riddles, counting three matching flowers, and a Show me control that walks to the clue's location. |
| [LEGO DUPLO WORLD: Under the Sea](https://storytoys.com/apps/lego-duplo-world/) | Explore scenes, find hidden treasures, interact with objects, build creatures and discover patterns; the publisher identifies the app for ages 2–5. | Large picture controls and real scene objects; a short windchime pattern that siblings complete together. |
| [Sago Mini World Scavenger Hunt printable](https://sagomini.com/printables/playing/world-scavenger-hunt/) | A printable companion uses pictured characters and objects to find inside World games. It is not a separate multiplayer hunt. | Three visible map fragments act as a shared picture journal. |

The examples support exploration, maps, picture recognition and gentle hints. The four-player server-owned progression, saved varied NPCs, particular story, puzzles and chest combination are our own implementation choices.

## Original story and play

Calypso hid a surprise on a pretend island. A gust scattered the map. Three randomly chosen prepared child NPCs help at the shell cove, flower grove and windchime lookout.

1. Enter **Daycare → Games → Treasure hunt**. Starting travels the connected Daycare group into the separate island and opens the narrated story.
2. Find the red striped shell among three covers. Lift it, then pick up the first map fragment. Other shells reveal a friendly crab.
3. Find the pot with three purple flowers. Lift it, then pick up the next fragment. Other pots reveal an old boot.
4. Listen to a friend's three windchime pictures, then repeat the sequence. Each player can contribute the next note. A wrong note gently restarts the pattern; the tune can be repeated.
5. The completed map names one of three landmarks. Dig there to uncover Calypso's chest. Wrong places keep the map and progress.
6. Match the three chest locks to the numbered symbols on the shared map fragments. Open the chest for the compass, stars and toy boat celebration.
7. Return independently, explore the finished island, or deliberately start **New hunt**. A new hunt chooses a fresh child cast and random hiding positions, tune and chest location. It is possible for a random puzzle layout to repeat.

Picture clues are the default; Picture / riddle adds optional spoken mystery clues. Show me guides movement without completing the puzzle. There are no age gates, mandatory reading, timers, personal scores or separate child rounds.

## Shared state and assets

- One authoritative hunt supports four players, automatic same-Daycare arrivals, late entry, independent Return/travel/disconnect and reconnect. Returning after visiting another world resumes the family's current checkpoint.
- Saved covers, fragments, partial tune, locks, story phase and NPC cast survive reopening. An empty hunt's own clock pauses. Players can choose any avatars; these never filter the NPC pool.
- Three distinct child NPC identities are saved per round. Calypso uses the existing teacher artwork. Helpers patrol with map/plant props, demonstrate the music and gather for the ending; motion uses world coordinates and one final pose per frame.
- Two original generated background panels contain scenery only. Interactive covers, clues, map, windchimes, mounds and chest are code-rendered objects.
- Fourteen original locally generated spoken clips and three original synthesized bell tones. Asset provenance is in SourceArt/Daycare/Treasure and SourceAudio/Treasure. Family visual/listening acceptance remains pending.
- Additive schema 47 / content 61 / protocol 3. This changes saved shared rules and requires a coordinated app/server update under the existing rollout policy. No physical-device installation or live-server replacement is authorized by this source task.

## NPC coding research and the movement repair

The user reported jerky motion in the native test windows, then supplied Jean-Luc's walking screenshot and explicitly requested research into NPC implementation. We checked these primary sources against this game's code:

| Source | Relevant guidance | Applied here |
| --- | --- | --- |
| [Unity Behavior: behavior graphs](https://docs.unity.cn/Packages/com.unity.behavior@1.0/manual/behavior-graph.html) | Separate conditional control flow and tasks; actions such as walking can remain running until completion. | Shared story phase, saved role and event times choose an NPC's job and route. Arrival is separate from the action pose. A graph package is unnecessary for this small scripted activity. |
| [Unity: coupling animation and navigation](https://docs.unity3d.com/2018.4/Documentation/Manual/nav-CouplingAnimationAndNavigation.html) | One component owns position; animation reads movement velocity, with smoothing and explicit idle/moving transitions. Match animation to travel to reduce sliding. This is a legacy 3D example. | The project's 2D Canvas characters use world-space NpcLocomotion, bounded SmoothDamp movement, distinct start/stop thresholds and one final pose presentation per frame. Camera motion never contributes to their walking velocity. |
| [Unity: patrol between points](https://docs.unity3d.com/2018.4/Documentation/Manual/nav-AgentPatrol.html) | Choose destinations deliberately, advance on arrival, and control braking depending on whether an agent should stop. | Existing Adventure routes retain their story jobs: guide, fruit helper, builders, guard, rescued friends and celebration. The presenter eases at starts, goal changes and arrivals. |
| [Unity Netcode: client-side interpolation](https://docs.unity.cn/Packages/com.unity.netcode.gameobjects@2.13/manual/learn/clientside-interpolation.html) | Packet arrivals and render frames occur at different times; rendering packet updates directly can create uneven motion. | One bounded NpcPresentationClock supplies a continuous cosmetic route clock to Adventure, Calypso and Treasure Hunt. Packet corrections adjust pace rather than snapping the displayed clock; prediction stops after a short missing-update interval. Shared gameplay remains server-owned. |
| [Unity: unscaledTimeAsDouble](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Time-unscaledTimeAsDouble.html) | A precise frame timestamp stays the same throughout a frame. | Native motion measurements use frame-start time and record frame dt. The earlier LateUpdate wall-clock measurement mixed movement with scheduling delays and incorrectly reported velocity spikes. |

The character assets are not all eight unique walk drawings. Most model-sheet characters, including Jean-Luc, duplicate each of four authored poses into eight slots. The previous player stride held slow NPC poses for seconds. NPC-only stride now follows character/world size, with a cadence ceiling for fast story transitions. All 37 prepared characters are exercised through the actual sprite presenter. Player cadence and travel speed remain unchanged. Calypso previously had only four standing/teaching/reading/sitting drawings, so using the standing frame during travel necessarily made her glide. A separate original eight-frame walking atlas now animates her travel in Daycare, the sandpit route and the Treasure island. Every teacher drawing uses a measured floor pivot and walking stops before reading/teaching/sitting resumes. The unchanged original action artwork remains in use; [source hashes and exact generation prompt](../../SourceArt/Daycare/Calypso/README.md) record the new PNG.

This improves playback of the existing child art; it does not create additional drawings or claim exact foot planting or family visual approval.

Random NPC identities are selected once by shared rules, saved with the round and retained when players join or leave. Player avatars never affect this selection. Story actions and progress remain authoritative; clients cannot independently invent NPC tasks or complete puzzles from cosmetic animation.

## Verification and delivery

Windows **400** client and dedicated server compile with zero build errors/warnings. Focused Unity checks pass the additive migration, partial shared save round trip, puzzle/group rules, asset availability, packet timing/loss/stale samples/reconnect, bounded NPC locomotion and settled arrival. All **37** prepared character presenters advance through their walking poses at NPC speed; Calypso exercises all eight walking frames and returns to an action drawing with a stable floor anchor.

Two native four-client runs pass **eight groups each**. [Treasure results](evidence/daycare-treasure-hunt-2026-10-01/treasure-result.json) cover actual picture touches, shared fragments/tune/locks, a late fourth player, independent Return, disconnect/reconnect, cold authority reopening and deliberate replay. [Adventure/Daycare results](evidence/daycare-treasure-hunt-2026-10-01/adventure-result.json) cover real fruit pickups, builders, the ball/guard/rescue/feast, late joins, independent departures, saved reopening, settled NPC poses, picnic plates and teacher walking. All 21 processes exit zero across those two runs. The older Adventure test initially expected the queen to throw immediately; it was corrected to touch the current **Play ball** conversation choice. No game-source change was needed for that test correction.

The four-second native child motion trace records no walk/still switching, bounded patrol speed below 43 world units/second and advancing sprite frames; [metrics](evidence/daycare-treasure-hunt-2026-10-01/npc-motion.json). Teacher frame tracing confirms all eight walking drawings during actual travel; [metrics](evidence/daycare-treasure-hunt-2026-10-01/teacher-frame-motion.json). [Child motion capture](evidence/daycare-treasure-hunt-2026-10-01/npc-walking-native400.mp4) and [teacher capture](evidence/daycare-treasure-hunt-2026-10-01/teacher-walking-native400.mp4) encode native screenshots at approximately 10 fps; these capture rates are not the game render rate. The first teacher route passed behind picnic furniture, covering her feet. A further four-client focused check confirms visible walking before she reaches the table, all eight walking drawings, stable world motion during camera panning and the reading pose after arrival. [Clear-view result](evidence/daycare-treasure-hunt-2026-10-01/teacher-clear-result.json) and [clear-view native capture](evidence/daycare-treasure-hunt-2026-10-01/teacher-clear-walking-native400.mp4) pass; all five additional processes exit zero.

The built Assets match the build400 source manifest byte for byte. Evidence excludes test credentials, complete saves and private logs. The documentation scope checker still flags eleven pre-existing unclassified reports outside this task; the updated wishlist and new report are classified and no new link/scope error remains. Windows shutdown logs still contain the pre-existing Creek `ResetCreekBoats` teardown exception outside this change; the checks do not claim an exception-free unrelated teardown.

No device installation or live-server replacement. Schema47/content61 requires a coordinated rollout. Family visual/listening acceptance and real-device motion remain unverified. Other five new wishlist ideas, other imagination stories and the broader Daycare learning stations remain open.
