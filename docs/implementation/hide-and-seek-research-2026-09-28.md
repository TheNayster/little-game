# Hide-and-seek with Bandit and Chilli — research and build proposal

**September 28, 2026 · Original research milestone; subsequently applied in [HS-1](hide-and-seek-2026-09-28.html).** The user wants an easy menu to invite a Bluey parent to seek, usable hiding places, a visible countdown and convincing NPC search. This proposal covers **HIDE-01, HIDE-03, the seeker portion of NPC-01, H-12/H-13/H-15/H-16/H-17**, and the dependencies on four-player identity and independent room travel. HIDE-02 child-seeker play and the parents' full everyday routines remain required later scope. No game code, art, build, save or device is changed by this research.

Read alongside [current decisions](../current-decisions.md), [Home tracker](../home-world-feature-tracker.md), and research chapters [22](../bluey-game-research-2026-09-23.html#22-hide-and-seek-and-parents-with-everyday-routines), [31](../bluey-game-research-2026-09-23.html#31-joining-and-leaving-without-restarting-play), [34](../bluey-game-research-2026-09-23.html#34-hide-and-seek-with-enterable-furniture-and-gentle-clues), and [50](../bluey-game-research-2026-09-23.html#50-one-shared-world-independent-travel-and-shared-items). At the research milestone these systems were not built. The subsequent implementation report supersedes that status; the broader design below remains the contract.

## 1. Recommended experience

**Let's play → Hide & Seek → choose Bandit or Chilli → hide during a visible countdown → watch the parent look for everyone.** Reuse the accepted illustrated picture-button style. Keep movement available throughout hiding and searching.

The default activity card can show **Play with Bandit** and the two parent portraits together, so returning players can start with two deliberate taps from the world. Changing the parent is optional, on that same sheet. A small downstairs play mat can open the same card directly. No need to find or chase a parent before starting.

| Moment | What the child sees and does | Shared behavior |
| --- | --- | --- |
| Start | Large parent portrait, pictured hiding game, one Play button. A brief voice invitation is optional. | Reserve one separate parent NPC. The starter joins; the other three players receive a small optional Join invitation. No full-screen lobby. |
| Hide | Parent covers eyes. A top-center **20 → 0** number and soft shrinking ring show time left; optionally speak the matching remaining number. Nearby legal spots show a large hiding picture. | One authoritative initial countdown. NPC perception is disabled during the count. Movement, object pickup and everyone else's activities keep running. |
| Enter a spot | Tap **Hide here** and walk into it. The child sees their own character inside a cozy cutaway, with **Come out** always available. | Entry atomically reserves a real slot. Successfully entering deliberately marks that child ready; an optional **I'm ready** also supports ordinary cover. No extra mandatory confirmation after entry. |
| Search | Parent lowers hands, looks around and starts walking. Countdown becomes **Bandit is looking!**, with the parent portrait. | At zero, search begins for ready players only. An unready child stays Preparing with gentle assistance; their siblings do not wait. |
| Found | Parent arrives, looks behind/opens the correct cover and smiles: **There you are!** Short happy reaction. | One authoritative find for that participation cycle. No score, loser screen, forced relocation or waiting penalty. |
| Keep playing | **Hide again** or **All done**. Children can come out, leave, read or cook whenever they choose. | A returning hider gets fresh personal preparation; the parent's existing search and siblings' timers continue. When everyone finishes/leaves, the parent is released. |

**Countdown proposal, not a developmental fact:** start at 20 seconds, informed by the show's count-to-twenty premise; test 10/20/30-second adult options later. A child who is not ready at zero is never automatically found. Early readiness does not unexpectedly shorten another child's initial count. A mid-search newcomer gets their own visible 20-second preparation countdown and readiness protection; the parent does not pretend to close its eyes or restart for everybody.

The own-player cutaway conveys **I am hidden** without a long explanation. The parent portrait can turn its head and show a small listening symbol when a clue is heard. Avoid an aggressive detection meter, flashing red countdown, scary pursuit or constant chatter. The default three action positions stay consistent: Hide/Ready when preparing, Come out while hidden, All done throughout; a giggle is a secondary pictured action. Keep the accepted joystick clear. A menu never grants invisibility to an eligible exposed hider; All done is the deliberate exit.

## 2. Sources and what they actually support

| Primary source reviewed | Finding and application | Limit |
| --- | --- | --- |
| [Official Bluey: Hide and Seek](https://www.bluey.tv/watch/season-1/hide-and-seek/) | Family hiding, a count to twenty and playful distraction provide the tone. Use expressive counting and friendly reveals. | In the episode Bluey seeks. Our parent-seeker role and countdown duration are project design choices. |
| [Outright Games: Bluey: The Videogame](https://outrightgames.com/game/bluey-the-videogame/) | The publisher describes easier interaction and quick access to games through its updated menu. Apply a short illustrated start flow and easy return to free play. | The page does not document its NPC algorithm. No claim that Little Weeps reproduces that implementation. |
| [Brook Miles, How to Catch a Ninja, Game AI Pro ch. 32](https://www.gameaipro.com/GameAIPro/GameAIPro_Chapter32_How_to_Catch_a_Ninja_NPC_Awareness_in_a_2D_Stealth_Platformer.pdf) | A shipped 2D game separates perceived interests from live targets, uses sight/sound conditions, priorities and handled-event memory. Adapt that separation into visible clues and remembered observations. | Borrow the perception architecture, with gentle family behavior designed here. No combat mechanics or unrestricted target tracking. |
| [Rich Welsh, Making NPCs Search Realistically, Game AI Pro 2 ch. 27](https://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter27_Looking_for_Trouble_Making_NPCs_Search_Realistically.pdf) | Search begins with readable reactions, investigates plausible points and can broaden into a coordinated sweep of unsearched places. Use authored inspection anchors and a bounded coverage route. | The chapter also suggests short hidden target tracking after lost sight. Reject that shortcut here: a child's unseen new hiding position must remain unknown. |
| [Martin Walsh, Perception and Awareness in Splinter Cell Blacklist, Game AI Pro 2 ch. 28](https://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy%27s_Splinter_Cell_Blacklist.pdf) | Consistency, understandable feedback and plausible detection matter. Room connections can inform hearing. Apply obvious head turns, finite recognition time and hearing through connected spaces. | Do not copy combat tuning or make detection depend on any player's camera; cameras differ across four clients. |
| [Unity NGO 2.13 NetworkTime and ticks](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/advanced-topics/networktime-ticks.html) | Server time supports coordinated timed presentation; transmission delays and precision still matter. Base countdown display on server-owned phase/time state. | This is not an automatic persistent timer or recovery system. Use this game's existing authoritative snapshots and migration rules. |
| [Apple touch/design guidance](https://developer.apple.com/design/tips/) and [Android accessible controls](https://developer.android.com/guide/topics/ui/accessibility/apps#touch-targets) | Minimum touch regions are 44 pt on Apple and 48 dp on Android. Aim larger for primary children's controls, with space, pictograms, short labels and pressed feedback. | Unity reference pixels are not automatically points/dp. Verify physical sizes and safe areas on the Samsung and both iPads. |
| [Unity AI Navigation overview](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/index.html) | A navigation package is an available option when a project needs navmesh tooling. | Its availability is not evidence that a new navmesh package suits this game's logical 2D floor or headless rules. The first slice uses an authored logical graph instead. |

The gameplay timings, scoring weights, capacity plan and implementation slices below are **our proposals**, not research-proven preschool settings. The primary AI chapters describe production techniques; family usability must be tested with the actual children.

## 3. Hiding places that fit the house

Start in one compact downstairs living-room/hall zone, with **six distinct slots across four integrated props**, giving four players choices without covering every room in more furniture. The following are candidates, not approved placements; confirm space against existing illustrated layers before making art.

| Candidate | Slots | Visual and interaction requirement |
| --- | --- | --- |
| Existing sofa rear nook | 2 | Two separate crouch anchors, rear cover and readable entrance; preserve the accepted sofa art size and four seats. A hide anchor must not occupy a seating anchor. |
| Curtain alcove | 1 | Character steps behind a broad cloth panel; curtain closes and moves gently for an enabled clue. It must not block stairs or a room exit. |
| Dress-up wardrobe | 2 | Two visibly fitting compartments, distinct doors and inspection anchors. Keep personal/item storage separate from avatar occupancy. |
| Small play tent | 1 | Flap opens, occupant enters, flap closes. It supplies its own cover layer and reliable exit. |

If these six slots cannot fit the existing room cleanly, alter the proposed layout before adding them. Do not duplicate a couch painted into the panorama or scatter extra tents throughout the house. Architecture → rear furniture → occupants/held items → front cover remains the art order.

Later Home candidates: reading-curtain nook, blanket fort, a suitably sized play drawer, bed canopy, veranda screen, large cardboard play bus and backyard playhouse/bush nook. A small drawer remains an item container. Kitchen appliances and narrow food cupboards are not avatar hide spots. Each addition needs route, capacity, cover and exit tests; a pretty drawing alone does not become a usable hiding place.

Four bedrooms can participate in an explicitly larger Home search after stairs/door routing passes. **Secret rooms retain their calm, no-surprise-search rule:** entering one withdraws that player from ordinary hide-and-seek, with a quiet explanatory cue. The seeker never magically discovers or enters a hidden star door. Other destinations stay outside a Home round; visiting one leaves only that player's participation.

### Required hide-spot contract

A stable spot ID contains: room ID, separately numbered slots, approach/entry/interior/exit/inspection anchors, cover geometry, legal avatar poses, door/flap state, supported held-item presentation, fallback exit, and traversal/occupancy revision. Reserve on accepted entry, release on exit/find/leave, and make duplicate requests idempotent. A full spot shows its occupied state and offers another reachable spot without moving the child elsewhere automatically.

Hiding is a gameplay/presentation state, never disabling the player object or input. The hider sees their own interior; seeker views receive closed-cover presentation without the hidden avatar's name, outline, hitbox, held-item silhouette, idle bark or accidental footstep cue. A held item keeps the same ID/holder and exits with the child. Changing avatar keeps profile, slot and game role. Invisible interactive hit targets are disabled for other viewers. If future human-seeker mode must conceal raw data too, add a filtered activity projection; UI masking alone is not a security boundary.

Come out cancels a pending entry or inspection safely. One authority transaction orders the race: an already committed find remains found; otherwise exit removes the occupancy and the parent sees an empty spot. Exiting is never rejected because another toy occupies the preferred anchor: choose a nearby reserved fallback. Decorative cover cannot be dragged away during occupancy; normal toy clutter does not hard-block these paths or entrances.

## 4. The parent AI

Use a small **hierarchical state machine with a scored choice of the next inspection point**. It is appropriate for one active seeker, easy to inspect in logs, and can run entirely inside the existing logical server/solo simulation. A behavior-tree package, cloud model, local LLM and machine learning are unnecessary dependencies for this slice. This choice is an engineering recommendation, not a claim that alternatives cannot work.

The parent has two kinds of information:

| Allowed knowledge | Information excluded from choosing the next place |
| --- | --- |
| Legal spots and routes; where the parent is facing; areas already checked; accepted readiness; observed exposed movement; remembered last-seen location/time; genuinely heard clues. | Current hidden player coordinates, which uninspected slots are occupied, a closed room's contents, private camera position, an item's hidden holder position. |

The shared authority knows the whole world, but passes a deliberately limited observation record to the search planner. It reads a spot's actual occupants **only after** the NPC physically reaches the correct anchor and finishes that slot's inspection. Search order must remain the same when hidden occupants are moved without changing the NPC's observations and seed.

| NPC state | Behavior | Exit condition |
| --- | --- | --- |
| Available | Brief idle or existing future routine; separate stable NPC identity from any human using the same character. | Accepted activity invitation reserves this parent. |
| Count | Turn away, hands over eyes, show authoritative countdown; ignore visual/clue observations. | Countdown expires and at least one hider is ready. If nobody is ready, wait kindly and remain interruptible. |
| Choose | Select a valid next observation/coverage point. | A route is available; otherwise use recovery. |
| Walk | Move along logical floor/portal edges, looking toward travel; interpolate animation locally. | Reach inspection anchor, perceive a new eligible exposed player, or encounter a real route change. |
| Inspect | Look behind/open the relevant cover; show curiosity. | After a short server-timed inspection, validate the same slot and eligible occupants. |
| Notice | Turn toward a real sight/sound cue with a small readable reaction. | Investigate its remembered location or approach a still-visible player. |
| Found | Friendly reveal and reaction; commit one find event. | Continue for remaining hiders or finish. |
| Recover | Try another approach, then another search point; offer a clear exit/help if necessary. | Valid route restored or activity released. No visible teleport-and-find. |
| Finish | Wave; release temporary role and paths. | Return to a sensible nearby routine/idle. |

**Bandit and Chilli share identical fairness rules.** Personality comes from animation and recorded delivery: Bandit can do a playful exaggerated peek; Chilli can quietly listen and crouch beside a tent. Keep similar difficulty. Never switch seekers because someone changes their playable avatar, calls a different activity, or disconnects as the starter. A second invitation joins the existing round; it does not spawn another copy or steal the active seeker. Starting a separate second round with the other parent is later optional scope, not needed for four hiders.

### Seeing and hearing

- Test eligible exposed players in the same logical room: facing/range, author-defined occluders, and body visibility. Use simple torso/head sample points; do not count a stray ear through a curtain as an immediate find. Closed hide slots need an inspection, regardless of decorative sprite overlap.
- Begin with about **1 second of continuous visible exposure** before a notice, then approach and validate visibility/proximity before a find. Losing sight stops live tracking; remember only the last seen point. Grace time, range and detection feedback require tuning.
- Keep the first slice's clues within the compact search zone. For more rooms, hearing follows valid room/door connections with bounded path cost, not straight-line distance through walls or floors. Ordinary radio/music, books and other children's activities do not become hiding clues.
- Automatic giggle/rustle clues remain the previously proposed **5–10 seconds after the child is both hidden and search-eligible**, with a cooldown; also offer manual giggle or automatic clues off. A mute changes local playback, not whether the logical clue happened. Show the corresponding small listening/cloth cue so muted play is understandable.
- A clue records the location at emission. If a child moves afterward, the NPC investigates that old location. Cancel pending clues when a player leaves; discard stale events after a find/rejoin. Do not replay historical sounds on another device's arrival.

### Choosing a convincing route

Build a seeded shuffled coverage route over **all legal slots**, including empty ones. Prefer nearby uninspected points, a genuinely heard clue, or a recent visible last-known position. Penalize recent checks and expensive detours. Age an unvisited point so it cannot be starved. Sample these priorities when choosing a destination, not fresh random targets every frame.

Finish a committed inspection unless its target becomes invalid or the child leaves. Coalesce repeated clues and bound discretionary diversions between coverage visits. A child repeatedly giggling must not prevent the parent ever checking the other five slots. Revisit a previously checked location only after a new clue/visible event or the next coverage pass, not because the authority secretly noticed it became occupied. Empty inspections naturally produce a harmless wrong guess; do not use hidden occupancy to force a theatrical empty-first search.

## 5. Navigation and the short-search target

Author a small data-only graph of walkable anchors, obstacle-safe segments and explicit door/stair portals. Use A* or Dijkstra over this graph, then follow its segments with ordinary logical movement. This graph is **proposed new work**; research descriptions of a shared floor graph are not evidence that a finished NPC navigator already exists. The dedicated server must load its geometry without Unity scene renderers or a camera. Load scenery only on each viewing client.

Decide at a modest cadence, initially around 5 Hz with event-driven replans; move/interpolate using the existing simulation/render cadence. At most one active seeker, four hiders and six pilot slots keeps the first problem small. Throttle perception to relevant nearby candidates. Reserve door approaches briefly, keep exits clear and do not let four children permanently body-block the NPC. Replan if no progress is made for roughly two seconds; cap retries, release stale reservations and visibly explain an unavailable spot. These are starting budgets to measure on the A10, not measured performance.

The existing **roughly 30 seconds per ready hider** is a compact-area pacing goal. It cannot be promised across the entire long house, upstairs and yard with one walking parent. Estimate the pilot's slowest coverage pass before approving the layout:

`search time = route distance / NPC speed + inspections + turns + reactions + allowed detours`

Illustrative budget: a route of at most 2,600 logical floor units at 280 units/second, six 1.5-second slot checks, four 1.25-second reactions and three seconds of turns gives about **26.3 seconds**, leaving little detour margin. This is arithmetic on proposed inputs, not a measured route or a change to accepted player movement. Measure every start/slot order; shorten distances or reduce the legal zone if needed. Clues-off mode must still cover every slot fairly. Repeatedly moving hiders are not a fixed-time guarantee.

Larger areas should be explicitly offered as a longer **Whole Home** game after route testing, preserving the compact quick game. Do not quietly teleport the parent upstairs, speed it up behind the camera, or reveal somebody remotely to force a deadline.

## 6. Four players, persistence and sound

| Situation | Required result |
| --- | --- |
| Two taps claim one slot | One atomic winner; the other remains visible and free to choose. Six independent slots can accommodate all four players. |
| One player is still preparing | No detection, clues or forced loss for them; ready siblings continue. Long inactivity releases only that participation after the agreed five minutes. |
| Late join or Hide again | New participation ID and personal preparation; keep the parent's current task and everyone else's elapsed search. |
| Found/leave/travel/background | Release only that player's hide slot and pending clues, settle them visibly at a valid exit; keep possessions. Others continue even if the starter leaves. |
| Secret-room entry | Leave this hide-and-seek activity; preserve the quiet room and all sibling games. |
| All players leave | End temporary activity, clear hints/role reservations and return the parent to ordinary idle. Never remove furniture, stored objects, food, art or room decor. |
| Server restart | Validate saved stable spots/rooms/items, discard invalid pending transactions, suspend old participation until a returning player deliberately resumes/joins; no stale forced reveal or duplicate parent. |
| Network outage | Continue from the latest usable private state under existing offline rules. Pause that local hiding participation with optional Continue/All done while the PC continues other players. Reconnect uses authoritative server state, never merges offline finds or edits. |
| No connection at launch | Same rules and one local NPC seeker work in private solo. No online AI or live speech request. |

Proposed authoritative records: activity ID/epoch, selected NPC ID, search zone, phase, logical elapsed time, count duration, route seed/cursor and checked slots; four per-profile participation records with cycle ID, Preparing/Hidden/Exposed/Found/Left, readiness, slot ID, clue schedule and last accepted event sequence; NPC pose/state, position, route goal and bounded observation memory. Local controls/audio are presentation and preferences. Define versioned snapshot migration before creating runtime data; old saves acquire an inactive activity and valid parent defaults without moving existing players or objects.

Use **remaining duration/elapsed simulation time in saves**, not a raw process-specific network timestamp. During a live session send a phase revision and server-time reference so each device derives its countdown; a delayed packet cannot start an old count or find twice. Gameplay transitions are decided by the authority, not client animation callbacks or `AudioSource` completion. In particular, the countdown must advance on a headless server; the earlier cooking-timer failure makes this a specific acceptance gate.

Use reviewed prerecorded parent counting, short calls, cloth movement and friendly reveal sounds. Default one foreground voice per local device; a reading child who did not join receives neither counting narration nor repeated parent calls. Music ducks under a participating child's count/call. Limit queued speech to the latest relevant line, use sequence IDs to suppress duplicates, and honor local mute/reduced motion. Exact voice/performance and source artwork need a separate asset pass; the accepted Bluey/Bingo art and movement stay intact.

## 7. Bounded implementation order

| Slice | Concrete deliverable | Gate before advancing |
| --- | --- | --- |
| **HS-1: complete downstairs loop** | Bandit NPC with count/walk/look/inspect/found poses, one illustrated start card, visible countdown, four integrated hiding props/six slots, Come out, simple fair coverage, four independent hiders and private solo. | Entire loop runs in a disposable headless server with four native clients. No placeholder-only doors or buttons. Same-slot races, actual cover and exits pass. |
| **HS-2: search personality and options** | Chilli using the same rules; parent choice, meaningful sight/sound observations, manual/auto/off clues, measured pacing, varied reactions, late joining/re-hiding. | Fairness and clue-off route tests; no repeated clue starvation; children can understand why they were found. |
| **HS-3: wider Home** | Qualified hall/stair/door graph, bedroom hide spots, explicit larger-area selection, outdoor locations and quiet secret-room withdrawal. | NPC visibly traverses portals; moved/blocked cover recovers; each destination remains independent. Measure a realistic longer search duration. |
| **HS-4: later backlog** | Human seeker/swap roles, NPC replacement when that seeker leaves, fuller parent everyday routines and shared optional assistance. | Hidden presentation and eventual data filtering as needed; no parent-role conflict with duplicate human avatars. |

HS-1 was the first proposed implementation slice and is now tracked in the linked implementation report, not an assertion that the larger request is finished after one room. Keep all HIDE/NPC master requirements open until their actual behavior and physical qualification pass. Main integration remains on hold. Music 217 was waiting for a reachable phone at the research milestone; the later HS-1 report records its inclusion in phone 223.

## 8. Acceptance evidence to collect

1. **Fairness replay:** with identical RNG seed, routes and observations, relocate hidden participants among uninspected slots. The next chosen target must not change until an actual clue/observation/inspection occurs. No reads of hidden occupancy in target scoring.
2. **Real four-player play:** four independent hides, simultaneous same-slot requests, duplicate avatars, found/re-hide, and the starter leaving. A fifth parent actor must not consume a human-player slot.
3. **Countdown on a headless server:** no audio or rendering dependencies. Join at 1 second remaining, repeat requests, reorder/delay notifications, pause a phone and restart the server. Nobody becomes eligible before their own readiness/count conditions.
4. **Hiding and exit:** correct foreground cover, owner cutaway, hidden held item/name/hitbox/audio; avatar swap; Come out during entry/open/find; blocked preferred exit; storage contents and items unchanged.
5. **Perception:** sight blocked by real cover, no hearing through disconnected rooms, finite exposure grace, last-seen position stays historical, moved hider defeats an old clue, mute preserves visible understanding.
6. **Pacing and route recovery:** enumerate or simulate all pilot slot arrangements/start points/seeds, with clues off and four reveals. Record longest coverage time; repeated giggles cannot starve a quiet child. Broken route recovers without a teleport-find or global reset.
7. **Independent lifecycle:** a player cooks/reads during the game; another joins late, exits Home, enters a secret room, backgrounds or disconnects. No loss of their creation or sibling control, no host election or offline merge.
8. **Migration and saves:** actual Unity serialization and native save/reopen, duplicate event suppression, suspended stale roles, all existing rooms/food/creations/enrollment retained. Production recovery remains a separate qualification, subject to the existing Windows validator block.
9. **Physical acceptance:** Samsung, newer iPad and A10 iPad; real touch target size, joystick plus hide-button multitouch, four mixed clients, sustained search, sound clarity and child comprehension. Native tests do not establish these outcomes.

Log accepted join/ready/count-end/clue/notice/inspection/find/leave timestamps, reasons, observed locations, route costs and per-hider cycle IDs in disposable test evidence. Keep those diagnostics out of the child's ordinary UI. A playtest should ask whether each child can start, hide, tell that they are hidden, understand the parent approaching, and come out without adult explanation.

## 9. Research delivery checks

The report is registered in the active documentation scope and linked from the current decisions, build guide, goal sheet, Home tracker and authored feature-audit generator. The regenerated documentation passes plan consistency: **76 active documents, 48 historical records, 121 rendered pages and 3,596 local links**. The Home feature inventory remains 339 entries, with hiding and parent gameplay still marked planned/not built. No gameplay, device, performance or physical usability test is claimed for this research pass.
