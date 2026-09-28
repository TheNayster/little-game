# A Bluey playset for your family

**Android rooms preview 138 — September 26:** Samsung was updated in place from 136 to fresh signed build **138**, containing the four-room BED-2 source. All **16 saves remain**, 15 byte-identical; the active world gained four persistent bedroom records. The user liked the preview and requested furniture next. [Update evidence](implementation/evidence/bedrooms138-2026-09-26/android-update.json). Rooms remain unfurnished. Server/iPads remain recorded at 128; iPhone at 101.

**Four owned upstairs rooms — September 26, BED-2:** Windows candidate **137** connects all four hall doors to distinct persistent rooms, assigns owners by saved profile ID, preserves carried objects and supports independent four-player visits. [Implementation, evidence and limits](implementation/bedroom-rooms-2026-09-26.html). The rooms are architectural shells; **BED-3 usable furniture, personal storage, decoration permissions and undo is next**. Four optional secret rooms follow the bedroom stage. The user accepted the Android 136 stairs preview (“works great”); candidate 137 has not been installed on a phone, iPad or family server.

**Android preview 136 — September 26:** Samsung was updated in place from 130 to the fresh signed release containing the researched stairs/landing. All 16 saved records remain; 15 are byte-identical and one received the expected schema/balloon migration. [Preview and retained-save evidence](implementation/upstairs-foundation-2026-09-26.html#android-preview-136). Bedroom doors/interiors are not implemented in installed 136. The user accepted its stairs/landing (“works great”) and requested starting the rooms; newer BED-2 source is recorded above. PC server/iPads remain recorded at 128; iPhone at 101; none were updated.

**Working stairs and upstairs landing — September 26, BED-1:** candidate **135** starts the researched bedroom plan with two-way visible stairs, a local upstairs hall, four independent travelers, carry-preserving internal travel and schema-6/content-7 save handling. [Implementation and qualification](implementation/upstairs-foundation-2026-09-26.html). At the BED-1 milestone the bedroom interiors/ownership/furnishings were unbuilt. The newer BED-2 result is recorded above; furnishing remains next. Four optional secret rooms follow the bedroom stage. The broader Home backlog remains required. Windows candidate only; no phone/iPad/server deployment or physical-device acceptance is claimed.

**Upstairs bedrooms first — September 26, latest user direction:** finish four personal bedrooms on a new second floor, reached by making the existing living-room stairs work. Research comes before implementation. The [upstairs and bedroom research](implementation/upstairs-bedrooms-research-2026-09-26.html) breaks this into working stairs/landing, four persistent profile-owned destinations, usable furniture/decor/storage, and qualification. Build the four optional secret rooms after the bedroom stage. This replaces the earlier kitchen-first queue; kitchen and all other Home features remain in the [Home tracker](home-world-feature-tracker.md). Each player travels independently and retains allowed held items. No implementation, art, build or deployment was performed in this research pass.

**Four personal rooms for four players — September 26, latest user correction:** Home must provide **four persistent player-owned bedrooms and four optional secret rooms**, one of each per family player profile, including parent players. This supersedes the earlier two-child-room limit. All four can visit any room together or use separate rooms; joining, leaving or changing avatar never creates, reassigns or deletes an owned room. Each profile has its own decorations, personal storage, creations, book/video bookmarks and local assistance/preferences. Every Home activity, station, dining/reading area and arrival/exit arrangement must support four independent participants with enough tools/places where needed. Secret-room creation/use is optional for the player; supporting all four is required scope. Preserve any existing room identities/data and add missing rooms through tested migration. The [Home world feature tracker](home-world-feature-tracker.md) records the complete backlog, individual room checks, status and acceptance evidence. This is a documentation/scope correction, not a game implementation or deployment. Home development is the active priority; physical rollout qualification remains pending until devices are available.


**Current home implementation:** [Build 128](implementation/integrated-home-2026-09-26.html) is installed on Samsung with all eight saved records retained. Clean living-room/tree/shed bases remove the painted duplicates; one layered sofa, trampoline and shed now draw occupants and contents at the correct local depth. Six native home groups pass, including stored-item visibility and offline reopen. Kitchen supports/interiors and the wider house inventory remain next. Server/helper and both iPads now also run 128; iPhone remains 101. See the current family rollout below. User visual acceptance of the new scene composition remains open.

**Accepted movement:** 420 floor units/second, **2 times the original speed**, remains the shared default for every current and future character. The user tested build 125 and clarified “Lots better.” Keep its calmer artwork and animation cadence; avatar size/selection must never override gameplay speed. [Movement evidence](implementation/movement-speed-2026-09-26.html).

Feature goal sheet and supporting research • September 23, 2026

**Current scope — September 25:** PC/VPS multiplayer and independent offline solo. Devices never host the shared world. Server state wins on reconnect; offline edits stay local. G4/AUTO-02 are retired. [Current decisions](current-decisions.md) override historical proposals; the [build guide](family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) records implementation status.

**Device priority confirmed:** the two iPads are the main play devices; Android is secondary. Design touch controls, layout, memory use and performance around the iPads first, using the older A2197 iPad 7 as the minimum performance baseline. Android/iPhone support and four-player mixed-device play remain required; a successful Samsung test does not replace iPad testing.

**How we will build it:** this page is the feature goal sheet; the [ground-up build guide](family-playset-build-guide-2026-09-23.html) is the implementation sequence. Its ledger retains all 55 chapters and 35 feature IDs, including retired AUTO-02. G1/G2/G3 remain partial. Device hosting is removed by the September 25 decision; rooms, creations and the full game content remain ahead.

**Latest character implementation:** [Selected-sheet revision 123](implementation/selected-sheet-characters-2026-09-26.html) is installed on Samsung with eight saved records unchanged. The user liked 122's appearance but found the arms/legs too active; 123 adds smaller walking poses and 25% lower cadence while preserving the other action artwork. Native walk/home/navigation checks pass. The [renewed mechanics research](implementation/walk-animation-research-2026-09-26.html#third-investigation-how-the-body-should-move-after-build-122) records arm/leg opposition, weight transfer, overlap and foot-contact limits. The user reports 123 looks much better; this quieter prototype walk is accepted for continuing home work. Precise foot-contact polish and physical A10 qualification remain open.

**Current device rollout:** Samsung, both iPads and the PC server/helper now run **128** with matching content 5. Both iPads updated in place, retained their saved documents/preferences and joined the original family server. The iPhone remains **101** and was not updated in this task. Accepted **420 units/second (2-times)** movement applies to every current and future character. Sustained A10/mixed-device and lifecycle qualification remain open. [Family rollout and retained-state evidence](implementation/family-home-rollout-2026-09-26.html)

**Physical evidence:** four mobile clients have joined the same family world. Both iPads passed scoped shared controls, ownership, travel and rejoin checks. The older iPad passed smoother offline walking and cold reopening on 95. Updated-device sustained play is still open.

**Independent Windows progress, September 25:** [portable encrypted server backup/reconstruction](implementation/g3-portable-recovery-2026-09-25.html) passes isolated recovery with the original four test players. The actual external destination, parent passphrase and second-account/computer restore remain open; no real-family export or deployment occurred. The [parent page now supports protected download and read-only file checking](implementation/g3-parent-portable-backups-2026-09-25.html), qualified in an isolated four-player world and browser. [Optional Windows sign-in startup](implementation/g3-signin-startup-2026-09-25.html) now also passes isolated native/browser checks, including four-player recovery and preserved Stop/Pause choices. Actual account startup/sign-in is not yet activated or qualified. These changes support the existing reliability goal and do not alter the feature list or phase order.

**Build evidence:** native compilation, installation and scoped device tests are recorded in the build guide. Candidate package lists below describe research, not an instruction to replace the pinned manifest or lockfile.

**Templates and packages:** [the detailed free-first package review](family-playset-package-research-2026-09-23.html) compares starting templates, exact Unity 6.3 package candidates, optional GitHub components, and one purchase around $15. [Section 54](#54-templates-packages-and-the-free-starting-setup) records the recommended setup. No complete ready-made template was found that supplies all our requirements.

**Deeper technical review:** [Source-backed networking and recovery research](family-playset-technical-research-2026-09-23.html) adds released-code findings, the automatic PC-server flow, package comparisons, shared-item rules and remaining device proofs. Bluetooth has been removed. See [section 53](#53-source-backed-networking-and-recovery-review).

**Feasibility:** retain the working dedicated-server foundation. Finish focused connection/offline qualification, prepare character art independently, establish durable room/item rules, then complete one polished slice before expanding the catalog.

**Current offline work:** build 98 continues privately from the latest usable visible state, without loading an older checkpoint over the scene. It does not wait for a complete server recovery replica. Physical transition acceptance and rollout remain open; offline changes never replay into the server.

**Latest user decision:** multiplayer stays on PC/VPS. Devices are clients; no device hosting, host election/switching or offline merge engine is required. Full offline solo remains required and travel multiplayer remains optional.

**Confirmed item-return rule:** [unused borrowed shared items return home automatically](#51-automatic-item-returns-stocked-areas-and-tidy-bedrooms). Personal decorations and saved creations must remain protected. Section 51 adds item categories, stocked activity stations, toy-box storage, container checks, and multiplayer-safe cleanup so one bedroom cannot collect the whole house. The broad timing/capacity numbers there are design proposals. The current 83 garden fixture specifically implements 180-second idle tool returns and 60-second completed-station rearming, each with a five-second cue; held/use interactions and partial progress are protected. [Measured timer and deployment record](implementation/g3-phone-layout-resets-2026-09-24.html). That narrow implementation does not yet supply the broader personal-item/bedroom policy, and 79 clients do not display the cue.

**Confirmed shared-world behavior:** [everyone travels independently, then meets in the same existing area](#50-one-shared-world-independent-travel-and-shared-items). Leaving never moves or pauses the players who stay. A shared item is one object: if someone holds the only bucket, others see it being held and cannot take it until released. Section 50 makes these acceptance requirements for all 1–4 players, locations, rooms, and compatible imagination activities.

**VPS decision:** the owned VPS is the preferred future designated server after reliability qualification. [Section 55](#55-future-hosting-on-your-owned-vps) covers controlled migration. Home-PC remote routing is background research; no VPS has been accessed or deployed.

**Family play:** up to four iPad/iPhone/Android clients, independent activities and free character choice including Bandit and Chilli. Four physical devices have joined the prototype; layered Bluey/Bingo and Garden/Creek areas exist. The full roster and activities are still planned.

**Automatic connection:** discover and authenticate the designated PC on home Wi-Fi, or the configured VPS after migration. A later player joins that same authority. No device competes to host; server unavailability leads to private solo.

**Outdoor and daycare expansion:** [10 beach activities](#37-the-beach-collecting-building-and-playing-together), [10 creek activities](#38-the-creek-rocks-water-and-gentle-discovery), [12 park activities](#39-playground-and-park-equipment-that-really-works), and [Daycare as the sixth world](#40-daycare-a-sixth-world-with-an-optional-pretend-day). The daycare plan adds 2–3 rotating activity invitations, [12 playful learning stations](#41-daycare-learning-short-playful-and-spoken), and [all nine requested imagination stories](#42-the-imagination-mat-nine-stories-that-become-playable-worlds). [Implementation and acceptance checks](#43-building-saving-and-testing-the-outdoor-and-daycare-expansion) preserve optional play, late joining, and offline travel. All are researched plans, not implemented game features.

**Home and travel:** the PC/VPS runs the shared world independently of any child. Science, personal rooms, secret rooms and hiding are retained. Full installed solo play is required; travel internet co-op is optional. Sections 44–46 define the current client/server scope.

**Books and dinosaur plans:** a [reading nook with interactive books](#25-reading-nook-and-interactive-books), [six proposed dinosaur books with spoken names](#26-dinosaur-books-and-spoken-names), a [TV clip library](#27-tv-corner-and-local-video-library), and [20 dinosaur toys plus a discovery mini-game](#28-dinosaur-toy-room-and-discovery-mini-game). These join the [feature tracker](#16-expanded-feature-tracker), 15 kitchen recipes, backyard fishing, cleanup, hide-and-seek, roaming parents and [32 activities drawn from the show](#23-show-games-and-activities-catalog). The production content remains planned. A separate single-clip video/bookmark fixture and simple garden cleanup prototype have tests; they do not supply the house TV library or finished activity catalog.

**New design refinement:** the [Toca Boca / Piknik interaction research](toca-piknik-interaction-research-2026-09-23.html) adds **Simple Play** for the younger child and **Explore & Stories** for the older child, using one shared world. Prioritize objects with several connected uses; quests suggest possibilities after free play works. Simple Play starts in a close-up activity area, allows direct prop use without precise character positioning, and keeps quests off or optional. Joystick and tap walking remain available. The [full supplement](toca-piknik-interaction-research-2026-09-23.md) supplies the concrete object catalog and revised first prototype.

**Recommendation:** build a 2D illustrated dollhouse in Unity, with optional little quests, movable objects, spoken character dialogue, and 1–4-player family play across iPads, iPhone, and Android over your home Wi-Fi. The separate Universal 2D project now uses pinned Unity 6000.3.24f1; the build guide records its installed packages and evidence. Create the game and audio on Windows; use your M1 Pro Mac for native iPad builds and device debugging.

**First playable work:** preserve the proven garden/creek rules, qualify current connection transitions, and begin isolated character animation. Durable room/item contracts precede the polished home/backyard slice; then expand to six worlds. No device-hosting gate blocks this sequence.

I found useful GitHub building blocks, but did not find a verified, complete template combining this dollhouse style, preschool touch controls, object recipes, voices, and local multiplayer. The optional third-party templates below were inspected through their documentation and selected source files during the research pass; those templates have **not** been qualified as our game. This does not describe the installed Unity/networking stack, which has later build and device evidence in the build guide. The design, budgets, quests, and milestones in this report are proposals, not measured results.

Open the companion **[illustrated research guide](bluey-game-research-2026-09-23.html)** for the character pictures and a clickable six-area concept. The pictures come from official character pages and need an internet connection in this research document. The intended finished game stores its art and audio locally.

**Visual and menu direction, September 25:** the user supplied eight screenshots of Budge’s Bluey: Let’s Play! and explicitly selected its illustrated scene presentation, sky/cloud world bubbles and lower-left family circle → full-body character tray → down-arrow close control. The later combined-menu decision below places the same scene circles vertically beside the horizontal cast, with a loading screen before world entry. This supersedes the separate browser, rotating-wheel and small portrait-drawer proposals. The [reference study](bluey-lets-play-reference-study-2026-09-25.html) records official-source findings, all eight images, input/animation requirements and the next navigation task. All six worlds and existing family features remain required.

**Base experience and character–item behavior:** the user wants the commercial mobile playset as the foundation for a cooler private family version. The [broader catalog and interaction specification](bluey-lets-play-reference-study-2026-09-25.html#18-characterobject-interaction-specification) cover games, locations, cast, audio and character actions. Sitting, trampoline jumping and dancing are explicit priorities: use correct poses, contact points, expressions and clear exits. Four-device play and the original enhancements remain required; no public release or monetization is planned.

**Scene composition correction — September 26:** The user identified duplicated painted and interactive furniture. [First integrated home pass](implementation/integrated-home-2026-09-26.html) now implements clean living-room/tree/shed bases and one layered placement for the existing sofa, trampoline and shed, with local rear/occupant/front ordering. Existing slot IDs and saves are preserved. Native/device evidence is maintained in that report. **ART-HOME-02 remains partial:** separate kitchen surfaces/interiors and other planned supports before food/bedroom expansion; complete contact/seam polish and physical A10 qualification. The accepted 2-times speed stays the shared default for all current and future characters.

## 1. Your requirements, now recorded

| Area | Agreed direction |
| --- | --- |
| Audience | Your children, ages 3 and 6, with parents able to join; personal family use |
| Artwork | 2D cartoon / flat vector-style illustration |
| Scene layout | Interactive dollhouse: drawn perspective rooms, illustrated characters and movable objects |
| Independent travel | Each player freely visits any available location/room; entering another player's area joins its existing shared state; no forced group travel or session restart |
| Shared item exclusivity | One physical prop has one current holder; others see pickup, movement, use, and release; simultaneous grabs cannot duplicate it |
| Automatic item return | Confirmed: borrowed shared props return after being unused for a while; protect personal decorations and saved creations; do not require everyone to leave the area |
| Stock and clutter | Shared areas retain essential tools/furniture; borrowed-item and loose-prop limits prevent room-sized stockpiles; personal toy boxes provide a separate collection |
| Characters | Child-character roster plus Bandit and Chilli; every player may choose any available character and switch during play; duplicate favorites allowed |
| Players | 1–4 humans, one per device; mixed iPad/iPhone/Android over reachable LAN or the planned private internet route |
| Movement | A setting for virtual joystick or tap-ground-to-walk |
| Objects | Drag to pick up, move, place, stack, fill, pour, and trigger compatible interactions in either movement mode; characters use proper sitting, jumping, dancing, riding, holding and serving poses with clear exits |
| Activities | Easy optional quests and open-ended free play; picture-based activity picker; leave, switch, or resume without penalties or blocking the other child |
| Cooking | Kitchen activities with at least five pizzas, five cakes, and five meals; free creation and shared preparation |
| Fishing | Catch-and-release game at the Heeler backyard fishpond from Tradies and a creek variant |
| Cleanup | Five playful cleaning/sorting activities; cleanup never gates access to another game |
| Hide-and-seek | NPC parent can seek up to four human hiders; human-seeker mode supports the other three; each participant controls an independent in-game avatar |
| Parents | Bandit and Chilli are playable choices for everyone; separate NPC actors handle roaming and requested activities without taking player control |
| Show activities | 32 researched episode-derived activity ideas, with easier and deeper cooperative adaptations |
| Books | Pick up a book in a house reading nook to open illustrated pages; Play starts spoken narration; tap pictures for small relevant animations and spoken dinosaur names |
| TV | Tap the house TV for a thumbnail library; play/pause, skip, seek, restart or return to the game; each child's place is saved per video, including after closing the app |
| Dinosaurs | A dedicated dinosaur discovery mini-game and a toy-room collection targeting 20 different dinosaurs; all types accessible without quest unlocks |
| Science | A house science corner with eight proposed touch experiments; simple cause and effect first, optional deeper play |
| Drop-in play | Start alone; family members join or leave without resetting activities; any client may close during PC-hosted home play |
| Personal bedrooms | Confirmed: four separate player-owned bedrooms, one per family profile; connected edits appear to all relevant clients |
| Secret rooms | Four optional mini-door rooms, one per player profile; all four players may visit any room; plush toys, stars and slow northern lights |
| Hiding detail | Enter designated closets and oversized drawers; optional giggle/rattle hints after 5–10 seconds; parent seeks by default and aims to find each ready hider within about 30 seconds |
| Travel | Every solo-capable feature works without PC or internet; travel multiplayer/hotspot/remote connectivity is an optional later want; preserve local progress |
| Remote home host | Confirmed: PC is basically always on and may host for away-from-home players; prototype a private route, saved endpoint, automatic joining, and outage fallback |
| Outdoor play | 10 beach, 10 creek, and 12 park activities; working equipment, collecting, fishing, building, tag, and hiding |
| Daycare | Sixth world; all requested child characters available, teacher routines, optional pretend day with 2–3 varied mini-game invitations |
| Playful learning | 12 proposed spoken learning stations with per-child assistance: reading, sounds, numbers, patterns, music, feelings, and science |
| Imagination | Picture play mat; nine episode-inspired stories with spoken role choices, NPC substitutes, and independent locations |
| Navigation | One circle/arrow chooser: characters browse horizontally at the bottom, five destination bubbles vertically at the side; selecting a ready world opens a local loading screen until ready; home and settings remain consistent |
| Shared play | Up to four mixed iPad/iPhone/Android clients automatically join the designated PC/VPS; independent areas and exclusive item use. No device hosting. |
| Automatic recovery | No Host/Join menus; rejoin automatically after outages while keeping offline work in separate private saves and restoring the current server world |
| Speech | Characters actually speak; instructions cannot depend on reading |
| Languages | English first; structure the game for English and Spanish, then complete Spanish recordings |
| Creation tools | Unity and Blender installed; ComfyUI for local voices; Windows creates the game and builds Android; Mac builds iOS |
| Deployment | Keep the agreed free automatic signing-renewal setup as a required family-installation step |

I interpret the bucket example as watering a **plant**. The six requested worlds appear below, including the new Daycare. Difficulty assistance should be adjustable per child, rather than permanently tied to age.

### Your test devices

| Device | Model | Last recorded OS/toolchain | Role |
| --- | --- | --- | --- |
| First iPad, A2197 | iPad 7th generation, A10, 10.2-inch | Device-observed iPadOS 18.7.10, September 24 | Required minimum performance target |
| Second iPad, A2602 | iPad 9th generation, A13, 10.2-inch | Device-observed iPadOS 18.6.2, September 24 | Required second client and solo test device |
| iPhone, A2484 | iPhone 13 Pro Max, A15 | iOS 26.6.1 | Additional parent/fourth player; layout and newer-OS testing |
| Android phone, SM-S948U1 | Samsung Galaxy S26 Ultra | Native-observed Android 16 / API 36, 4 KB pages, September 24 | Primary Android parent device; third-player cross-platform testing |
| Mac | 14-inch MacBook Pro, 2021, M1 Pro, 16 GB | Recorded macOS 26.3.1 / Xcode 26.6 | Xcode build, signing, native logs |
| Windows PC | NVIDIA RTX 5090, approximately 32 GB VRAM, observed locally | Existing Windows development machine | Unity, artwork, coding, voice generation, and agreed home family-game server |

The model identification and hardware are supported by Apple's [iPad model list](https://support.apple.com/en-us/108043), [iPad 7 specifications](https://support.apple.com/en-us/111911), [iPad 9 specifications](https://support.apple.com/en-us/111898), and [iPhone specifications](https://support.apple.com/en-us/111870). Samsung identifies **SM-S948U1 as Galaxy S26 Ultra** in its [official update record](https://doc.samsungmobile.com/SM-S948U1/037285260311/eng.html). That firmware page alone does not establish the installed OS. The table now includes later native inventory: [device/toolchain ledger](family-playset-build-guide-2026-09-23.html#device-and-toolchain-ledger). The iPad observations correct the originally reported OS order; Samsung and Mac versions are recorded too. These are retained observations, not new measurements made during this audit.

**Both iPads must work on iPadOS 18.** A successful test on the newer iPhone is useful but cannot qualify the iPads. Any optional native plugin must be tested on the older OS and physical hardware.

**Explicit radio behavior:** radio on → music plays → nearby idle characters automatically dance; radio off → music stops and characters settle. Walking or starting another action overrides dancing, and busy siblings keep their activity. Preserve mute, character-switch, save/reopen and shared/offline rules. This is ordinary object-driven free play; musical statues is an optional game layered on top. [Detailed reaction and acceptance contract](bluey-lets-play-reference-study-2026-09-25.html#20-sound-expression-and-pace).

## 2. What makes this kind of game work

Budge's own play instructions describe choosing a location bubble and discovering activities through tapping, dragging, and combining objects. That supports the requested virtual-playset structure: the room and its objects are the main activity, while quests gently suggest things to try. [Budge's Bluey play instructions](https://budgestudios.freshdesk.com/en/support/solutions/articles/12000094826-how-to-play-)

For your game, each location should have three layers:

1. **Immediate play:** visible objects respond when touched. A ball bounces, a swing moves, a cup can be carried, and a tap produces water.
2. **Discoverable combinations:** water fills a bucket; a full bucket waters a flower; a watered flower changes appearance.
3. **Optional invitations:** a character asks for a tiny task. Children can ignore it and continue playing, with no failure screen or timer.

Use consistent rules across locations. Once a child learns to pour in the backyard, pouring works similarly in the kitchen and at the beach. Most of the implementation effort belongs in these reusable rules.

### Usability for ages 3 and 6

Research on children's physical interaction recommends simple gestures and large targets, including approximately 2 cm targets for young children. It also warns against relying on precision and complicated gestures. These findings are a starting point; we still need to watch your children use the actual iPads. [Nielsen Norman Group's research on children's physical development and UX](https://www.nngroup.com/articles/children-ux-physical-development/)

Proposed application to this game:

- Aim for primary controls around **2 cm across on the physical screen**. On these iPads that is roughly 100 logical points; Unity canvas units must be checked against actual screen scaling.
- Separate touch targets even where illustrations overlap. Give small visible props a larger invisible pickup area, without covering nearby controls.
- Use one short spoken instruction at a time, a picture of the requested object, and a large replay button.
- Show a gentle animated demonstration after repeated difficulty; do not interrupt a child who is happily exploring.
- Offer generous snapping, forgiving drop zones, and a tap-object / tap-destination alternative when dragging is difficult.
- Keep menus predictable. Use a back/home picture in one consistent place, confirm destructive resets in parent settings, and avoid tiny text links.
- Use image, movement, and sound together. Sound alone is insufficient when the iPad is muted; text alone is insufficient for your children.
- Let the three-year-old complete one meaningful action. Let the six-year-old choose a short sequence, without adding punishment or compulsory reading.

For early testing, use roughly 15–90 seconds for a simple invitation and 2–5 minutes for a longer sequence. These are design targets, not research-established limits for every child.

## 3. Six content regions, five destinations and eighteen starter quest ideas

All six content regions remain available through five unlocked destinations: Heeler Home includes its backyard; the others are Park, Creek, Beach and Daycare. Completing a quest can add a sticker or a celebratory animation, but should not block another area or remove a favorite toy. Each location needs satisfying free play even when no quest is active.

| Location | Free-play activities | Why it belongs |
| --- | --- | --- |
| **Heeler Home** | Kitchen, cleanup, hide-and-seek, roaming parents, books, TV, dinosaur toys, science corner, four player-owned bedrooms, and four optional secret plush rooms | Shared pretend play and independent rooms; science, cooking, toys, and household activities reuse compatible props |
| **Backyard Garden** | Tap, buckets, plants, sandpit, fishpond fishing, balloons, hiding places | Water interactions, fishing, and cooperative outdoor games |
| **Playground & Park** | Working playground equipment, tag, hiding, riding, shadows, picnics; 12 activities in section 39 | Large, obvious actions and shared activities |
| **The Creek** | Rock collecting/stacking, fishing, log crossing, boats, nature play; 10 activities in section 38 | Gentle discovery and simple water-related combinations |
| **The Beach** | Shells, seagulls, moving waves, castles, ball, flying disc, and more; 10 activities in section 37 | Reuses pouring and containers in a different setting |
| **Daycare** | Teacher-led invitations, all-roster friend board, learning stations, imagination mat, quiet and free-play areas | Connects activities from all worlds while preserving personal choice |


**Connected house and backyard — user clarification, September 25:** Home and Backyard are entrances into one continuous family property, from the front of the house through its fully usable rooms, kitchen/dining area and veranda to the far backyard and shed. Sideways exploration should feel like one long dollhouse level; doors/stairs connect bedroom and other room branches without returning to the world menu. Retain cooking, living/TV, reading, science/dinosaur play, personal bedrooms, secret rooms, garden equipment, radio/dancing and durable shed storage. Use one Heeler Home bubble for the house and backyard, as implemented in revision 118; selecting it within the property retains the current location. The earlier two-bubble proposal is superseded. Each player has an independent camera and can remain indoors while another explores outside. Art/room chunks may load around the camera for the older iPad, but crossing between them must preserve object IDs, held items, container contents and authority. This is the required G5/G6 layout, not a claim that the full property is implemented by the menu milestone.


**World-art sequence — latest user direction:** first build attractive Bluey-style walkable scenery for all six worlds. Add no new activity features to the other worlds yet. After those visual shells, concentrate sustained room and interaction development on Bluey's connected house/backyard property. Keep existing functioning play and the full feature backlog; defer other-world feature implementation rather than delete it.


**Combined chooser — latest user decision, September 25:** the lower-right family circle opens one menu (moved away from the left joystick after the user’s phone test), with full-body characters browsing horizontally along the bottom and circular place thumbnails browsing vertically down the side. The down arrow closes both. Tapping an available place opens a loading screen on that device, prepares the destination and its current state, then enables play only when ready. This supersedes the separate full-screen staggered world browser as our navigation layout; R6 still guides the circular scene artwork. No second Play button is required. Browsing either axis must not accidentally select an entry. Other players continue independently.

**Long worlds and loading:** all six scenic destinations are implemented in [SCENIC-01 / native Windows 110](implementation/scenic-worlds-2026-09-25.html), with twelve panoramas, local walking/panning cameras and a maximum of three loaded/requested background textures. Home and Backyard share one continuous persistent property, presented as one Heeler Home menu destination in revision 118. Major-world travel waits for shared acknowledgement, applicable saves and visible texture readiness; distant art is released. Physical iPad memory/frame-time qualification and Apple rollout remain pending; Android 119 contains the scenic home. Separate loading controls working memory, not installed storage. The implementation uses Unity Resources loading; Addressables is not installed.

These locations and quest scripts are proposed original game content. They are not a claim that any particular episode or commercial game contains these exact objectives.

| Area / quest | Simple version | Explorer version | Reusable rule |
| --- | --- | --- | --- |
| Home: Tea time | Put a cup on the highlighted table spot | Place two cups, pour pretend tea, bring a snack | Snap surface + container |
| Home: Cosy corner | Place a cushion on the mat | Arrange cushions, add a blanket, put a toy inside | Placement + grouped recipe |
| Home: Breakfast helpers | Put fruit on a plate | Fill two plates and deliver them to the table | Container + delivery |
| Backyard: Thirsty flower | Pour a ready-filled bucket on a flower | Fill the bucket and water three plants | Fill + pour + growth |
| Backyard: Balloon fun | Tap the balloon to keep it up | Both children guide it toward a large picture target | Gentle motion + touch |
| Backyard: Sand surprise | Press a filled mould | Fill the mould, dampen sand, lift it, decorate | Material state + recipe |
| Park: Ball basket | Put one ball into a basket | Find and place three picture-matched balls | Pickup + category match |
| Park: Picnic | Put a snack on the blanket | Pack a basket, carry it, arrange a picnic | Container + delivery + placement |
| Park: Play trail | Put a stepping piece in a large marked spot | Arrange three pieces and walk the trail | Snap placement + movement |
| Creek: Boat launch | Place a paper boat in the launch area | Add a leaf passenger and guide the boat to a landing | Float + attachment + delivery |
| Creek: Leaf collection | Put a leaf into a tray | Find three different leaf shapes | Category collection |
| Creek: Crossing | Place one large stepping stone | Arrange a short crossing and walk across | Snap placement + path update |
| Beach: Sandcastle | Lift a ready-filled mould | Add sand and a little water, mould it, add a shell | Container + mixture + recipe |
| Beach: Shell pattern | Put a shell on a picture | Build a short shell pattern or your own design | Placement + optional pattern |
| Beach: Bucket helpers | Bring a bucket to a castle | One child fills while the other decorates | Shared object state + cooperation |
| Daycare: Picnic counting | Give one friend a plate | Set places for a pictured group and count each plate | One-to-one placement + spoken count |
| Daycare: Choose a story | Tap a mat picture and use one prop | Choose a role and help a friend finish a short story | Role + activity session |
| Daycare: Musical hello | Tap a drum and hear a sound | Copy a short rhythm or add a second instrument | Immediate audio + pattern |

Keep water shallow and stylized, and use forgiving animation rather than precarious physics for slides, swings, and boats. A toy can fall or spill without making the child lose progress. Every quest should be restartable, skippable, and repeatable.

### The combined character and five-destination chooser

**Updated navigation decision, September 25:** one family-circle/down-arrow chooser combines horizontal full-body characters with vertical scene-filled world circles. Selecting a usable world opens a local loading screen until ready. This supersedes the separate staggered browser, central preview and second Play button. The [reference study](bluey-lets-play-reference-study-2026-09-25.html#5-main-world-menu-contract) records the contract; [native 105 evidence](implementation/combined-chooser-2026-09-25.html) qualifies the prototype on Windows.

Keep five main destinations: Heeler Home (house and backyard), Playground & Park, The Creek, The Beach and Daycare. These preserve all six content regions. House rooms are subareas. Use recognizable scene thumbnails with optional short labels and spoken names. Preserve large targets and safe areas on both landscape iPads and phones. Only the selecting player travels; siblings keep playing in their current areas. The Creek entrance must remain available throughout the menu replacement. Development builds must distinguish unfinished previews from usable destinations.

## 4. Character roster and pictures

The illustrated guide contains **34 verified official portrait entries**, including Bandit, Chilli, a group portrait for the Terriers, and three older child characters. This is a researched starting catalog, not a claim to cover every named or unnamed child ever shown. The character system should let us add missing favorites without rewriting game code.

| Group | Characters to support |
| --- | --- |
| Heeler children | Bluey, Bingo, Muffin, Socks |
| Heeler parents | Bandit, Chilli; selectable by every player |
| School friends | Chloe, Coco, Honey, Indy, Mackenzie, Rusty, Jack, Snickers, Winton, the three Terriers; Pretzel's picture still to verify |
| Neighbours and other friends | Lucky, Chucky, Judo, Pom Pom, Winnie, Jean-Luc |
| Kindy and younger children | Lila, Missy, Buddy, Bentley, Juniper, Lulu, Dusty, Dougie |
| Additional children | Hercules; optionally older children Digger, Mia, and Captain |

Use the [official character directory](https://www.bluey.tv/characters/) and the individual source links on the picture cards. The machine-readable [character reference catalog](bluey-research/character-references.json) records the page and portrait URL for every entry. The attempted official Pretzel page returned 404; keep him in the requested roster and verify another reference instead of silently dropping him. Lulu's current official reference is a head portrait, so it does not establish a complete body design.

### Turning references into playable characters

A portrait is a visual reference. A usable game character also needs facing poses, animation, feet and hand anchors, and a touch area. Build a small art specification before producing the full roster:

- A consistent scale and line thickness at iPad resolution.
- Feet placed at a defined pivot so depth sorting and floor placement work.
- Idle, walk, carry, reach/use, talk, happy, and seated poses; simple transitions between them.
- Two or three mouth shapes for speaking, blink frames, and left/right handling that preserves intentionally asymmetric details.
- A hand attachment point for a cup or bucket; a held prop becomes part of the character's draw order.
- A portrait, selection sound, display-name localization, body/rigger category, and stable character ID.

Use sprite swaps and shared animation where the silhouettes allow it, with separate rig families for shapes such as Snickers, Pom Pom, and Winton. Unity's 2D Animation package provides Sprite Library / Sprite Resolver workflows; package versions still need to match the chosen editor. [Unity sprite swapping documentation](https://docs.unity3d.com/Packages/com.unity.2d.animation@13.0/manual/SpriteSwapIntro.html)

Start the technical test with Bluey and Bingo, then add Muffin and Socks to check body differences. That is a production sequence, not a replacement for the requested wider roster. Represent each Terrier separately in the actual chooser; the official reference is a group image. Prototype labels can be Terrier 1–3 until their identifiers are settled.

**Confirmed: everyone may choose any available character**, including [Bandit](https://www.bluey.tv/characters/bandit/) and [Chilli](https://www.bluey.tv/characters/chilli/). Parents are not restricted to parent characters, and children are not restricted to child characters. Several players may pick the same favorite. Give each player a different small badge or floor marker so they can tell their avatars apart. Character changes preserve profile, room ownership, progress, and activity role; see section 47 for NPC handling.

Dougie's official description says he is deaf and communicates through Auslan. Preserve that characterization with appropriate visual communication and a spoken companion/guide for instructions, rather than requiring the child to read. [Official Dougie description](https://www.bluey.tv/characters/dougie/)

**Character artwork selection and implementation — September 26:** the user selected the [earlier Bluey movement sheet](../SourceArt/Characters/SelectedReference/README.md), then requested matching Bingo and app integration. [Selected-sheet revision 123](implementation/selected-sheet-characters-2026-09-26.html) is installed on Samsung with eight saved records unchanged. The user liked 122's appearance but found the arms/legs too active; 123 adds smaller walking poses and 25% lower cadence while preserving the other action artwork. Native walk/home/navigation checks pass. The [renewed mechanics research](implementation/walk-animation-research-2026-09-26.html#third-investigation-how-the-body-should-move-after-build-122) records arm/leg opposition, weight transfer, overlap and foot-contact limits. The user reports 123 looks much better; this quieter prototype walk is accepted for continuing home work. Precise foot-contact polish and physical A10 qualification remain open. The original is preserved unchanged; newly prepared sheets follow that appearance. The user reports the quieter motion is much better; ART-HOME-02 is underway; its current first-pass evidence is recorded separately.

**Side-view clarification, September 26:** [Side-view revision 119](implementation/profile-walk-2026-09-26.html) adds editable Bluey/Bingo profile bodies for left/right walking, with front poses for standing and home actions. Native walk, chooser and home checks pass. Android 119 is installed with exact artifact and saved-record retention verified; its visible UI check is pending the locked phone being opened. User visual approval remains open.

**Walking appearance feedback, September 26:** the user rejected build 116. [Expanded research](implementation/walk-animation-research-2026-09-26.html) diagnoses the timing, facing and pose failures. [Revision 118](implementation/walk-animation-2026-09-26.html) corrects the walk and adds a complete-cycle preview while preserving the artwork, controls and speed. It also combines Home/Backyard into one destination and keeps the active character visible above the chooser. Native checks pass; user visual approval remains open.

### Where Blender fits

The chosen illustrated style does not require modeling every character in 3D. Use layered 2D artwork exported to transparent PNG sprites; retain editable vector or layered source files. Blender remains useful for blocking out a room's perspective, rendering a prop as flat artwork, or an eventual AR presentation. Unity handles the actual interactions, menus, animation, sound, saves, and iPad application.

## 5. Touch movement and object interaction

Both movement modes should feed the same character controller. The preference can differ on each iPad; multiplayer should not force both children to use the same mode.

| Input | Result |
| --- | --- |
| Drag virtual joystick | Walk in that direction; release to stop |
| Tap reachable ground in tap mode | Walk to that point along a valid floor path |
| Tap unreachable ground | Choose a nearby reachable point or give a gentle visual cue |
| Touch and drag an object | Pick it up and move it; show compatible destinations |
| Release over a destination | Snap, place, pour, or combine if the recipe allows it |
| Release elsewhere | Put it in a safe valid position or return it to its previous position |
| Tap a slide, swing, or seated spot | Move close if needed, then play the prepared interaction |

**Separate walking from dragging.** A tap that began on an item must not also send the character walking behind it. Decide what owns each finger when it touches the screen: menu/UI first, then joystick, then interactive prop, then floor. Hold that ownership until release or cancellation. Track actual pointer/finger IDs so two fingers do not steal each other's actions.

Unity's Input System provides touch handling, UI integration, and an on-screen stick. These are appropriate building blocks; they do not supply the game's gesture priorities or object rules. [Touch input](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/Touch.html), [UI input](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/UISupport.html), [on-screen controls](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/OnScreen.html)

Use simple authored floor polygons and a small path graph or grid for tap walking. A drawn perspective room is not automatically a navigable floor. Tables, counters, doorways, and character-only floor areas need explicit interaction and walk regions. Clamp joystick movement to the same legal floor used by tap walking.

In both modes, cancel held touches cleanly when the app backgrounds, a menu opens, a stage unloads, or the connection drops. Changing movement mode should stop the old route and reset the stick. Support simultaneous walking and dragging if it tests well, while ensuring the entire basic loop can be completed one finger at a time.

### Drawn perspective without a complicated 3D world

**Layering requirement:** The user identified duplicated painted and interactive furniture. [First integrated home pass](implementation/integrated-home-2026-09-26.html) now implements clean living-room/tree/shed bases and one layered placement for the existing sofa, trampoline and shed, with local rear/occupant/front ordering. Existing slot IDs and saves are preserved. Native/device evidence is maintained in that report. **ART-HOME-02 remains partial:** separate kitchen surfaces/interiors and other planned supports before food/bedroom expansion; complete contact/seam polish and physical A10 qualification. The accepted 2-times speed stays the shared default for all current and future characters.

Keep characters and props in world space, with an orthographic camera. Sort each character's complete sprite group by its foot position. Split backgrounds into floor, rear furniture, and foreground occluder layers so a character can appear behind a sofa or in front of a counter. Keep UI menus in a separate canvas.

Use authored interaction states for carrying, sitting, swinging, and pouring. Constantly simulating every furniture item as a free rigidbody creates more instability than this style needs. Selective 2D physics is useful for balls, falling toys, and light effects.

## 6. Reusable item rules: bucket → water → plant

Give objects capabilities, then define compatible recipes as data. For example: `Pickable`, `Container`, `Fillable`, `Pourable`, `Wettable`, `Growable`, `SnapSurface`, and `Floatable`. An object can have several capabilities. Avoid writing a completely separate script for every pair of toys.

| Object | State to save and synchronize | Actions |
| --- | --- | --- |
| Water tap | On/off and available source | Fill a compatible container in its fill zone |
| Bucket | Capacity, water amount, holder, position | Pick up, fill, carry, pour, set down |
| Plant | Moisture/growth stage, position | Receive water, change appearance, emit one growth event |
| Cup | Contents and amount, position | Fill, pour, place on tray |
| Sand mould | Material state and amount | Fill, dampen, form a castle |
| Boat | Position, attached passenger | Place on water, float along a gentle path |

For transfer, use a bounded amount:

`transferred = min(source amount, destination free capacity, requested amount)`

Subtract it once from the source and add it once to the destination. A water stream particle effect illustrates this operation; it is not the source of truth. We do not need real fluid simulation to make pouring satisfying.

For the first plant quest:

1. A character says, “Let's give the flower a drink!” and a flower picture appears.
2. The child drags a bucket into the tap area. A visual fill level rises.
3. The child moves the bucket over the plant. A large highlight shows the valid target.
4. Releasing there triggers an assisted pour, then returns the bucket to a safe position.
5. The plant changes to its watered state and celebrates once. The quest observes that confirmed state change.

The simple mode can begin with a filled bucket. Explorer mode includes filling it. If the bucket is empty, show the tap picture and play a brief helpful line. A full destination can produce a harmless spill effect, while amounts stay clamped. Watering the same plant twice must not duplicate a one-time reward.

Allow undo or easy recovery for accidental placement. Save permanent changes after the completed action, not on every pointer movement. Keep immutable object definitions separate from each world's mutable state.

## 7. Two iPads together: choose home Wi-Fi first

**Recommended transport: Unity Netcode for GameObjects with Unity Transport over the local network. Updated home topology: the Windows PC runs the server; both iPads join as clients.** This can work with the router's internet connection disconnected; the devices still need to reach the PC on the same local network. No internet account, cloud lobby, or Relay service is required for direct LAN sessions. Unity's network manager supports direct transport connection data. [Unity network manager / connection setup](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/components/core/networkmanager.html)

Test both iPads as clients of the designated PC/VPS, then loss/rejoin and offline solo independently. One player closing their app must not interrupt another player.

### Selected connection plan

Bluetooth is removed. Use the ordinary authenticated Wi-Fi/IP client path to PC/VPS; offline solo remains usable without that connection.

| Approach | Fit for this game | Decision |
| --- | --- | --- |
| Same-router Wi-Fi + Unity Transport | PC server and up to four mixed clients; internet unnecessary for local gameplay | **Home default** |
| Travel hotspot | Possible internet route to the designated PC/VPS | Optional after remote deployment; no mobile authority |
| Router-free peer-to-peer co-op | Removed from scope | Independent solo when server is unreachable |
| Existing Multipeer Connectivity Unity transport | Historical plugin comparison | Outside the selected implementation scope |

The Multipeer Connectivity material is historical API research only; no native peer transport or peer-host prototype is planned.

<a id="finding-the-other-ipad"></a>
### Finding the designated family server

After parent enrollment, automatic discovery/rejoin connects clients to the designated PC/VPS while foregrounded. Local play never waits for discovery.

Add an understandable `NSLocalNetworkUsageDescription` and declare the exact Bonjour service in `NSBonjourServices`. Ordinary browsing of a declared service differs from arbitrary multicast/broadcast traffic; avoid adopting a raw broadcast-discovery sample without checking its entitlement needs. Retry connection after permission is granted, and keep solo play available after denial. [Apple local network privacy guidance](https://developer.apple.com/documentation/technotes/tn3179-understanding-local-network-privacy)

A guest Wi-Fi network may isolate devices. During parent setup, verify discovery and joining on the actual household router, including a release build installed with the intended free signing workflow. If the Windows editor is hosting a test, also check its firewall permission.

### Shared objects need an owner

Treat the host as the authority for world state. When both children reach for the same bucket, the host grants one temporary grab reservation. The other child sees a gentle “being used” cue; the bucket must not teleport between two fingers.

Proposed sequence: grab request → host reservation → local drag preview → host validates drop/use → confirmed state event to both iPads. Give each request an object ID and sequence number so retries cannot pour twice or grant duplicate rewards. Release reservations on drop, cancellation, disconnect, and timeout.

Send the actual object state—water amount, position, holder, plant growth—not individual particles. A starting experiment can update moving positions around 10–20 times per second with interpolation, while delivering important action results reliably. Tune those numbers on both iPads; they are not measured requirements.

Joining players receive a consistent world snapshot plus the state needed for their current room, then catch up with newer changes. Shared quest completion credits the participating children. Each iPad retains its own language, audio, movement, and assistance preferences, so English on one device and Spanish on the other can refer to the same quest event.

The designated server owns the shared world. A disconnected client can continue a separate private world; on rejoin it loads current server data. Offline actions, rooms and creations are not imported into shared state.

## 8. Spoken English and Spanish, using your PC and ComfyUI

**Yes: local AI tools can help produce the voices.** Use an LLM to draft short dialogue and translations, and a text-to-speech model to generate the actual audio. Render the clips on Windows, listen to them, and bundle the approved recordings in Unity. The iPads then play normal audio clips, with no live AI server or internet connection required.

The locally observed RTX 5090 has approximately 32 GB VRAM, making local voice auditions a reasonable next experiment. This is not a compatibility test: the installed ComfyUI environment, PyTorch/CUDA versions, and chosen node still need checking before installation or upgrades.

### Tools worth auditioning

| Candidate | What the source establishes | Best use here |
| --- | --- | --- |
| [Chatterbox](https://github.com/resemble-ai/chatterbox) | English models plus a multilingual family covering English and Spanish; current upstream describes Multilingual V3 | First voice-quality audition for expressive character lines |
| [FL ChatterBox for ComfyUI](https://github.com/filliptm/ComfyUI_Fill-ChatterBox) | Standard, Turbo, and multilingual nodes; documented English and Spanish support | Preferred ComfyUI integration candidate, subject to environment and model-version checks |
| [Kokoro-82M](https://huggingface.co/hexgrad/Kokoro-82M) | A smaller speech model with an official voice catalog | Quick narrator/menu and prototype comparison |
| [Kokoro ComfyUI node](https://github.com/billwuhao/ComfyUI_KokoroTTS_MW) | A ComfyUI integration for Kokoro | Alternative workflow if its dependencies fit the existing install |

The FL wrapper documents multilingual support, but that does **not** establish compatibility with every newly released upstream Chatterbox checkpoint. Its Turbo expression tags are documented for English. Verify the exact wrapper and model pair before choosing it for Spanish. Kokoro's official catalog lists three Spanish stock voices; audition them rather than assuming they sound like the intended child characters. [FL node documentation](https://github.com/filliptm/ComfyUI_Fill-ChatterBox), [Kokoro voice catalog](https://huggingface.co/hexgrad/Kokoro-82M/blob/main/VOICES.md)

Generated voices can be distinct, expressive character voices, but matching the show's performances convincingly is an audition problem, not a capability established by a model's language list. Start by comparing six short lines and choosing consistent voices. No voice models or custom nodes were installed during this research.

### Production workflow

1. Write a dialogue table with stable line IDs, speaker, English text, Spanish text, delivery notes, and required animation.
2. Generate auditions in the existing ComfyUI workflow or an isolated speech environment if dependency conflicts make that simpler.
3. Listen for pronunciation, unwanted extra words, clipped endings, uneven volume, pace, and whether a young child can understand the instruction.
4. Save approved mono WAV masters. Record the model/version and generation settings so revisions are repeatable.
5. Import appropriately compressed playback clips into Unity, loading only the needed stage/language set.
6. Trigger mouth animation and the clip from the same dialogue event. A basic amplitude envelope can switch between closed, small-open, and wide-open mouth sprites.
7. Have a fluent Spanish speaker review instructions and recordings before treating the Spanish option as finished.

Menu narration can announce character and location names. Speaking characters should also have greetings, action responses, and celebration lines so they feel alive. Quest-giver dialogue can be shared across the roster; we do not have to regenerate every possible instruction for every selectable avatar. Preserve non-speaking/signing characterization where appropriate while providing a spoken guide.

| Line ID | English draft | Spanish draft | Use |
| --- | --- | --- | --- |
| garden.flower.invite | Let's give the flower a drink! | ¡Vamos a regar la flor! | Quest invitation |
| garden.bucket.fill | Put the bucket under the tap. | Pon el cubo debajo del grifo. | Explorer step |
| garden.flower.pour | Pour the water on the flower. | Echa el agua en la flor. | Assisted pour |
| garden.flower.success | Look! Our flower is growing! | ¡Mira! ¡Nuestra flor está creciendo! | Celebration |
| common.try_again | Let's try over here. | Vamos a probar aquí. | Gentle hint |
| common.repeat | I'll show you again. | Te lo enseño otra vez. | Demonstration |

Spanish drafts above use words common in Spain; the family's preferred dialect is still open. For Latin American Spanish, terms such as “cubeta” and “llave” may fit better. Pick a consistent dialect before recording the full set.

**English-first rollout:** implement both locale IDs immediately, finish the complete English playable, then add and review Spanish audio. During development, missing Spanish lines can fall back visibly to English for adult testing. Do not present the Spanish option as complete while core spoken instructions are missing. Each player can choose their own language.

Keep speech readable through sound: lower music during dialogue, prioritize one instruction at a time, stop obsolete prompts when changing stages, and provide replay. A speaker queue should prevent repeated object touches from producing a wall of overlapping voices. Keep visual hints available even with voices muted.

## 9. GitHub templates and components: what is actually useful

The following is a source inspection dated September 23, 2026. “Last commit” identifies the inspected repository snapshot, not guaranteed active support. A README, package declaration, or old editor version cannot establish iOS/IL2CPP compatibility. Full commit hashes and selected source excerpts are saved in [the repository evidence](bluey-research/github-evidence.json) and [the multiplayer evidence](bluey-research/multiplayer-evidence.json).

| Repository | Inspected state | Useful part | Recommendation |
| --- | --- | --- | --- |
| [Unity Playground](https://github.com/Unity-Technologies/UnityPlayground) | Unity 6000.0.66f2; MIT; commit 2026-05-01, `3d8acd7432ee` | Small 2D movement, collision, pickup, and interaction examples | Best learning/prototype reference; add the game's touch and dollhouse rules |
| [Unity UI Extensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions) | Package 3.0.0; Unity 6000.0; BSD-3-Clause; 2026-06-19, `d47f838ba918` | Radial layout and scroll/snap UI components | Selectively useful for the stage wheel and character chooser |
| [Yarn Spinner for Unity](https://github.com/YarnSpinnerTool/YarnSpinner-Unity) | Package 3.2.8; Unity minimum 2022.3; MIT; 2026-09-16, `478309e5e70a` | Dialogue, line presentation, localization | Optional when conversations become substantial; not a voice generator |
| [Unity Open Project / Chop Chop](https://github.com/UnityTechnologies/open-project-1) | Unity 2020.3.17f1; Apache-2.0; 2022-06-26, `608eac98df29` | Event, scene, and quest architecture to study | Reference only; project explicitly discontinued in 2021 and is a 3D adventure |
| [lluispalerm QuestSystem](https://github.com/lluispalerm/QuestSystem) | Package 1.0.0; declared Unity 2019.1; MIT; 2024-07-29, `ee1949421349` | Graph-based quest authoring | Requires code fixes and build verification; not the default choice |
| [FelixBole quest-system](https://github.com/FelixBole/quest-system) | Package 1.2.0; declared Unity 2019.1; MIT; 2024-09-16, `87cb9a15ed16` | Quest steps, conditions, events, saving | Secondary candidate if our small custom quest definitions become limiting |
| [hsadler 2D top-down template](https://github.com/hsadler/unity-2d-topdown-template) | Unity 2022.3.17f1; MIT; 2024-01-27, `0a301ff0402c` | Grid item placement and inventory dragging | Study selected interactions; factory/grid assumptions do not match free placement |
| [Unity Physics Examples 2D](https://github.com/Unity-Technologies/PhysicsExamples2D) | Inspected projects 6000.5.9f1; Unity Companion License; 2026-09-11, `ba8a2608c1a6` | 2D physics examples and drag/joint techniques | Port small compatible examples; its current projects are newer than 6.3 |
| [Team-on UnityGameTemplate](https://github.com/Team-on/UnityGameTemplate) | Unity 2021.2.7f1; MIT; 2022-02-01, `bfcbac80d4d3` | Menus, settings, audio, localization examples | Menu reference; broad older dependencies are unnecessary for this game |
| [Unity Boss Room](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop) | Unity 6000.0.52f1; README specifies NGO 2.4.3; Unity Companion License; 2025-10-23, `1299ba4fc97d8` | Co-op session and replication patterns | Study networking; adapt for direct LAN rather than adopting the full RPG/cloud stack |
| [Reality Design Lab Multipeer transport](https://github.com/realitydeslab/netcode-transport-multipeer-connectivity) | Package 1.2.0; Unity 2022.3; NGO 2.0.0 + Transport 2.3.0 dependencies; MIT; 2024-11-09, `e89723025a4f` | Apple native transport experiment | Historical Apple networking reference; not a qualified Windows/iPad transport |

### Specific code findings that change the recommendation

- **Playground's actual project file is newer than its README baseline.** Inspect `ProjectVersion.txt` rather than assuming the README's Unity 2022 reference is current. Its example movement is keyboard-oriented; selecting the template does not finish iPad controls. [Inspected project version](https://github.com/Unity-Technologies/UnityPlayground/blob/3d8acd7432ee115f28e05f2b1af39fa783376b4a/ProjectSettings/ProjectVersion.txt)
- **UI Extensions supplies layout components, not a finished preschool stage menu.** A five-button arrangement may be simpler to write directly than importing a broad UI package. Use the package if its snapping/carousel components save demonstrable work. [RadialLayout implementation](https://github.com/Unity-UI-Extensions/com.unity.uiextensions/blob/d47f838ba918bbb9b26f242bd944940a711f525f/Runtime/Scripts/Layout/RadialLayout.cs)
- **The lluispalerm quest runtime imports `UnityEditor` without a guard**, which needs investigation before a player build. Its save implementation also uses `BinaryFormatter`. Replace that persistence path and validate runtime/editor assembly separation before adopting it. [Quest manager source](https://github.com/lluispalerm/QuestSystem/blob/ee1949421349b059c74b97b36f968dc11bdec539/Runtime/QuestManager.cs), [save source](https://github.com/lluispalerm/QuestSystem/blob/ee1949421349b059c74b97b36f968dc11bdec539/Runtime/SaveData/QuestSaveSystem.cs)
- **FelixBole's restore path matches steps by name.** Stable IDs are preferable for our saves, because a renamed quest step should not lose a child's progress. Inspect this before building content on it. [Quest-system source snapshot](https://github.com/FelixBole/quest-system/tree/87cb9a15ed16e4107c634e5f1381d56b176b8eb6)
- **The top-down template also uses `BinaryFormatter` for saves.** Its drag placement is tied to a grid-building game. We would need to replace saving and adapt placement rather than assume it is a ready dollhouse. [Source snapshot](https://github.com/hsadler/unity-2d-topdown-template/tree/0a301ff0402c2b9f8eefbbb2e6ca6625069edbf2)
Apple peer-to-peer plugin references above are historical comparisons, outside the selected PC/VPS implementation scope.

**Chosen foundation:** a fresh Unity Universal 2D project; official Input System; a small custom interaction and quest data model; an NGO/Unity Transport combination to qualify; optionally a few UI Extensions components. Keep Yarn Spinner optional until actual dialogue complexity warrants it. The [current package review](family-playset-package-research-2026-09-23.html) records exact candidates and a newly identified NGO status mismatch. No package combination has yet been tested for this game.

## 10. Project structure and coding plan

Use Unity **6.3 LTS**, with **6000.3.24f1** as the concrete patch selected in the earlier research. Keep Windows and Mac on the same tested patch. Recheck release notes when creating the project and before family deployment. The patch's published known issues include a **2D Renderer black screen when Bloom is active**, directly relevant to this style: leave Bloom off in the initial project and test the actual renderer on both iPads. [Official patch notes](https://unity.com/releases/editor/whats-new/6000.3.24f1)

The project now exists separately at `C:\Users\sephi\Desktop\Little weeps game\Unity\FamilyPlayset`. Keep the unrelated `Meeps game` project untouched. The pinned editor/packages and scoped device proof are in the build guide; no new project or template import is needed.

Choose released, editor-compatible package versions and lock the resolved dependency set. Unity's package pages can list prerelease alternatives; that is not a reason to choose the newest number automatically. The inspected Boss Room package combination is evidence of that sample's setup, not certification of our complete game. [Unity 6.3 NGO package page](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.netcode.gameobjects.html)

| Layer | Responsibility |
| --- | --- |
| Input adapters | Touch pointer ownership, joystick, tap paths, object drag intents |
| Character motor | Floor movement, obstacles, facing, arrival and interaction poses |
| World rules | Validate grabs, drops, transfers, recipes, and stable object state |
| Network session | Hosting/joining, reservations, action messages, snapshots, reconnect |
| Quest runtime | Observe confirmed world events; track optional objectives and hints |
| Presentation | Sprite animation, highlights, particles, sound, speech, mouth shapes |
| Content definitions | Character, item, recipe, quest, stage, and dialogue data |
| Persistence | Versioned world save, per-player preferences, backups and migrations |

A useful flow is: **touch intent → validated world action → state change → quest/audio/animation feedback → save checkpoint**. Multiplayer uses the same action path, with host validation. This keeps a pretty animation from accidentally becoming the only evidence that a task completed.

Use ScriptableObjects for editable definitions and normal runtime state objects for a particular session. A quest asset should describe a quest; it should not directly hold shared mutable progress across both children's profiles. Give every persistent object, recipe, character, line, and quest a stable identifier.

For fifteen short quests, a compact definition can contain: ID, stage, invitation line, ordered or unordered steps, required world-event conditions, hint image/line, and completion effect. Add branching dialogue later if the content actually requires it.

## 11. Menus, settings, saves, and recovery

Suggested flow, revised by section 44: **Start → remembered character/profile → five-destination combined chooser or last room → play**, with family discovery and connection happening automatically. Remember the child's last character and assistance preferences. A parent can help with initial Wi-Fi permission and installation, while everyday play remains picture-led.

Settings should include music, effects, and speech volume; English/Spanish; joystick/tap movement; joystick side and size; assistance level; captions/picture hints; reduced motion; and return to menu. Put reset-world and saved-game management behind a deliberate parent action. Essential instructions still need visual cues when speech volume is zero.

Save a small versioned JSON state: stage, object positions and contents, plant states, completed optional quests, and selected characters. Store per-iPad preferences separately from the host's shared world. Use Unity's persistent data directory and preserve the same bundle identifier across updates. [Unity persistentDataPath](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-persistentDataPath.html)

Write a temporary save, validate it, then replace the previous save using a strategy verified on iOS; keep a last-known-good backup. Save after meaningful completed interactions, before stage changes, and on pause. Do not depend only on an application-quit callback, because a mobile app can be suspended or terminated. [Unity application pause callback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html)

If a save fails to parse, preserve it for recovery and try the backup; do not silently erase the child's world. If the app updates, run an explicit version migration. If a character or audio file is missing, load a safe fallback and log the missing ID rather than crash.

## 12. Performance and crashes on the older iPad

The A10 iPad sets the budget. Begin with **stable 30 FPS**, with optional 60 FPS on the A13 only after profiling. A 30 FPS frame has roughly 33 ms available; averages alone can hide pauses that make dragging feel broken. Measure loaded-room movement, pouring, joining, and stage transitions on the actual device.

Proposed engineering starting limits—not measured guarantees:

Keep all necessary logical area state on the PC/VPS even when each client renders only its own area. A client changing areas or opening media must not pause another player.
- Load character rigs as needed; use lightweight portraits in the chooser rather than all full animated characters at once.
- Start with mostly 2048-pixel sprite atlases, split by area. A 2048×2048 RGBA texture is 16 MiB uncompressed before mipmaps; 4096×4096 is 64 MiB. Actual device formats and compression change this footprint.
- Evaluate ASTC settings against clean cartoon outlines. Avoid blurry outlines just to chase a smaller number.
- Pool frequently spawned effects, cap particles, and prevent repeated taps from spawning unlimited audio or physics objects.
- Use simple lighting and limited transparent overlap. Keep full-screen post-processing and Bloom off initially.
- Use a provisional active-memory goal below roughly 400 MiB, then revise from device profiling. This is our conservative starting budget, **not an Apple-published safe memory limit**.

Apple can terminate a memory-heavy app without the same report as a conventional crash. Capture both crash reports and jetsam reports, retain the exact build's symbols, and use Xcode to diagnose the cause. [Apple jetsam analysis](https://developer.apple.com/documentation/xcode/identifying-high-memory-use-with-jetsam-event-reports), [crash reports and device logs](https://developer.apple.com/documentation/xcode/diagnosing-issues-using-crash-reports-and-device-logs)

| Failure mode | Prevention / recovery to implement |
| --- | --- |
| Finger lifts, system gesture interrupts, or app backgrounds | Cancel the drag, reset joystick state, release network reservations |
| Both children grab or pour the same object | Host reservation, sequence IDs, single authoritative transfer |
| Many fast taps open the same stage twice | One transition state; temporarily disable duplicate requests |
| Old async load finishes after leaving a stage | Cancellation / generation checks before using loaded content |
| Client disconnects while holding a bucket | PC continues; release the abandoned hold and preserve the last accepted water/object state |
| Audio overlaps or continues after leaving | Prioritized speech queue with stage/session ownership |
| Save is interrupted or malformed | Temporary write, verified replacement, backup and schema migration |
| Memory grows after repeated stage changes | Unload unused content; inspect retained references and atlas/audio residency |
| Native networking plugin calls unavailable API | Guard OS availability; test a real iPadOS 18 release build early |

Personal use still deserves release-build testing. Test the same signed artifact the children will use, not only editor play mode or a development build. [Apple guidance on testing a release build](https://developer.apple.com/documentation/xcode/testing-a-release-build)

## 13. Required acceptance checks

These are future build gates. None are claimed complete by this research.

| Test | Passing behavior |
| --- | --- |
| Solo on each exact iPad/OS | All six locations and spoken core instructions work without internet |
| Both movement modes | Walk, drag, and use objects without accidental competing actions |
| Younger-child assistance | Large targets and assisted pour allow completion without reading or precise aiming |
| PC/VPS and four mobile clients; separate offline solo | Join, move, interact and change area without resetting another player; disconnect/rejoin preserves private saves |
| Both players choose Bluey | Clear player markers; independent controls and settings |
| Simultaneous bucket grab | One consistent owner; no teleport fight or duplicate bucket |
| Repeated / retried pour | Water conserved, capacity respected, plant reward granted once |
| Late join and reconnect | Correct current stage, contents, positions, and quest state arrive |
| Internet disconnected, router still on | Existing LAN play continues and a local session can be started |
| Local-network permission denied then granted | Solo remains usable; joining can recover without reinstalling |
| Either iPad backgrounded mid-drag | No stuck item, runaway joystick, or corrupted save; PC-hosted home world continues |
| Mixed English/Spanish and input settings | Shared events agree while each iPad presents its own preferences |
| Missing clip / art during development | Clear fallback and diagnostic record; no crash |
| Save/relaunch, app update, signing renewal | Existing world and preferences remain intact |
| Repeated stage transitions | Proposed stress pass: 100 switches without steadily growing memory |
| Sustained child-style play | Proposed pass: 30 minutes of rapid taps, dragging, pouring, and pauses on the A10 |
| Release crash diagnosis | Build version and symbols retained; any crash or jetsam event investigated |

Also watch each child play without coaching for a short session. Record where they hesitate, where an object fails to respond as expected, and whether they understand spoken hints. Adjust interaction size and timing from observation rather than assuming a framework solves child usability.

## 14. Build order and completion gates

| Milestone | Deliverable | Gate before expanding |
| --- | --- | --- |
| 1. Core interaction experiment | Plain shapes in one backyard, both input modes, bucket/tap/plant | Reliable drag and pour on the A10 |
| 2. Two-iPad experiment | Direct LAN session, shared bucket ownership, synchronized plant | Both devices see the same results; interruption recovery works |
| 3. Illustrated, speaking slice | Bluey/Bingo, finished backyard art sample, English dialogue and mouth animation | One complete inviting activity that both children can use |
| 4. Product shell | Combined character/five-destination chooser, settings, local saves | Relaunch restores state; navigation needs no reading |
| 5. Content expansion | Expand to all six locations, activity families, and broader character roster | Every area supports free play and the same object rules; required networking gates in section 52 pass before broad production |
| 6. Spanish completion | Reviewed translated lines and recordings; per-device language choice | No missing core Spanish instruction; mixed-language co-op works |
| 7. Family release | Performance/crash checks and in-place installation | Both exact iPads pass release checks and automatic renewal verification |

A two-character experiment is much smaller than a polished game with dozens of characters, six illustrated locations, multiplayer, and two voiced languages. Plan the full game as a sequence of content batches; artwork, animation, and listening to voice output will be substantial work. An accurate schedule comes after the first two device experiments reveal the networking and art-production costs.

AR remains an optional later presentation—for example, putting the dollhouse on a table—rather than a dependency of the initial illustrated playset. Keep game rules separate from camera/presentation so an AR experiment can reuse them. The earlier [iPad platform and AR research](ipad-game-research-2026-09-23.md) contains the device and deployment detail.

### Preserve the installation-renewal requirement

When deploying, set up and verify the agreed free automatic signing-renewal workflow on **both** iPads. Start by evaluating the planned Sideloadly background renewal, with SideStore as the alternative already discussed. Confirm successful automatic refresh, renewed expiry, launch, and preserved saves on each device. A successful initial install alone does not complete this requirement. [Sideloadly documentation](https://sideloadly.io/faq), [SideStore](https://sidestore.io/)

The renewal plan is intended to remove manual weekly reinstall work. It still depends on refresh actually succeeding; it does not create a permanently unexpired native installation. Keep this verification in the release gate, including the household's travel routine.

## 15. Decisions to make after the first prototype

Travel internet/hotspot multiplayer is optional after VPS readiness. Router-free co-op and device hosting are removed; all installed solo activities remain available offline.

**Original September 23 research limits:** no game build or iPad runtime test was performed during that research pass; GitHub findings are selected-source inspections; ComfyUI integrations were researched but not installed; generated voices were not auditioned; character pictures are official visual references rather than finished game rigs. The companion guide and evidence files make the proposed implementation concrete enough to review and start with the backyard prototype.

**All-world checklist, September 26:** the [consolidated feature audit](all-world-features-audit-2026-09-26.html) lists every named world/activity/recipe/toy/story catalog, current status, sources, and all 35 requirement routes. It distinguishes scenery, playable prototypes and unbuilt content.

## 16. Expanded feature tracker

Added September 23, 2026, following the Toca/Piknik research. This is the main record of the new requirements. **These rows specify the finished requirements, not current implementation status.** The [September 24 implementation audit](family-playset-build-guide-2026-09-23.html#all-35-feature-requirements-implementation-status) tracks every ID as partial, not built or optional later, with evidence and remaining acceptance. No complete feature row is accepted yet. Episode facts below come from the official Bluey site. Controls, recipes, timing, AI behavior, and multiplayer rules are our proposed adaptations, not claims about existing Bluey games or the show's internal production technology.

| ID | Required feature | Proposed behavior | Complete only when |
| --- | --- | --- | --- |
| CHAR-01 | Change characters whenever wanted | Tap the family circle, then a full-body tray character; close with the large down arrow; retain held item, position, profile and game role | Both iPads agree after switching, including during cooking and hiding |
| CHAR-02 | Everyone may choose any available character | Include Bandit and Chilli; no adult/child restrictions or exclusive favorites; separate player avatars from NPC jobs | All four may switch, including duplicate Bandits, without losing roles, props, or NPC-led activities |
| FAMILY-01 | Up to four mixed-device family players | iPad, iPhone, and Android share automatic LAN discovery; separate profiles, one authority, independent cameras/activities | Third/fourth join and leave; slots, recovery, phone UI, older-iPad load, and offline saves pass on real devices |
| ACT-01 | Easy, optional quests | Large activity-picture button, nearby station invitations, one-tap start; leave or switch freely | No quest blocks movement, another activity, or the sibling's play |
| COOK-01 | At least five types of each requested food | Five pizzas, five cakes, five meals; recipes and free creations share the kitchen | All 15 can be prepared, carried, served, saved and used by four independent players |
| FISH-01 | Fishing at the house water feature | Backyard fishpond; assisted catch, inspect, release; optional picture matching | Four usable roles/tools; competing rods cannot duplicate one fish; exiting returns only that player's unfinished catches |
| CLEAN-01 | Cleanup mini-games | Toys, dishes, spills, laundry, and garden sorting | Cleaning is satisfying and optional; nobody loses their creations |
| HIDE-01 | An NPC parent finds up to four players | Bandit or Chilli seeks; each player independently chooses a hiding spot and readiness | Fair search, useful hints, independent departures and no eliminated player waiting helplessly |
| HIDE-02 | A human seeker finds up to three other players | Choose hide/seek roles; hide each hider's position on the seeker's device | Seeker cannot reveal hiders via nameplates, touch targets, or character changes |
| NPC-01 | Parents behave naturally between activities | Walk, read, garden, prepare food, tidy designated props; interruptible requests | Parents respond to play requests and never become permanently reserved |
| CAT-01 | A substantial show-based activity backlog | 32 entries in section 23, linked to official episode sources | Each selected activity receives art/audio, cancellation rules, and device tests |
| BOOK-01 | Pick up and explore narrated books | House reading nook; full-screen illustrated pages; Play/Pause, page arrows, spoken names, small animations; six proposed starter titles | First expanded dinosaur book and remaining five distinct books work offline, with understandable controls and no overlapping speech |
| TV-01 | Watch clips through the house TV and resume later | Local thumbnail library; play/pause, skip, seek and restart; one player per device; persistent per-child/per-video bookmarks | Offline playback, leave/return, close/relaunch, crash recovery, independent siblings, failed import and in-place update preservation pass |
| DINO-01 | Lots of different dinosaur toys | Target 20 museum-referenced types, all with drag interactions, spoken names, and short toy animations | Every type is reachable without unlocking, saves correctly, and supports shared play |
| DINO-02 | A dinosaur mini-game | Dinosaur Discovery Mat: dig, brush, wash, arrange, and invent stories; five optional invitations | Younger child can play without reading; both children can contribute or leave without losing toys |
| LAB-01 | Simple, fun science in the house | Eight proposed experiments; reusable props, immediate reactions, optional invitations | Every station works solo, with up to four players, and after a participant leaves |
| JOIN-01 | Join and leave during play | Automatic family connection; snapshot plus ongoing updates; per-child membership and preparation; no global restart | Joining during every activity and leaving at every step preserve the other child's progress |
| WORLD-01 | Independent travel within one shared family world | Creek and playground can run simultaneously; entering a friend's area reveals their existing avatar and current objects | Up to four independent locations, late arrivals, and repeated visits work without cloned rooms or forced regrouping |
| WORLD-02 | Leaving does not interrupt remaining players | Load/unload only the traveler's view; preserve shared simulation, activities, and creations | A player leaves while others drag, cook, ride, read, or hide; their controls and progress continue |
| ITEM-02 | A shared object has at most one holder | Authority grants one pickup; replicate holder, position, contents, and release; explicit safe handling of travel/disconnection | Simultaneous grabs, held-item arrival, cross-area carrying, release, and reconnect leave exactly one valid instance |
| ITEM-03 | Automatically return unused borrowed props | Typed return policy, home anchor, inactivity check, brief return cue, one server transaction | Held/active items stay; unused loans return across all devices without duplicate objects, lost creations, or room reset |
| STOCK-01 | Keep every area playable | Fixed shared furnishings, protected essential station tools, bounded loan stock, one essential tool of each type per player at a station | One child cannot remove the working equipment from all areas or hide every tool inside bags |
| ROOM-02 | Bedrooms remain usable | Separate personal catalog, bounded loose props, clear exits, recoverable toy storage and creation shelf | All planned dinosaur types remain available; limits do not delete decorations or creations; visitors cannot clear a child's room |
| NET-02 | Any client may close without stopping the others | Dedicated authority runs the family world: PC now, owned VPS after qualified migration; clients hold no required server role | Any one of four clients closes or locks while the remaining three continue with valid object state |
| REMOTE-01 | Owned VPS after server reliability is ready | Authenticated endpoint, server build, recovery and controlled world/credential migration | Four mixed clients from home/internet; one canonical writer; offline solo on route loss |
| ROOM-01 | Four separate player-owned bedrooms; connected edits appear on all relevant clients | Durable room IDs, permissions, protected creations; offline copies remain local and never overwrite server rooms | Decorate, visit, save/reopen and rejoin without changing ownership, duplicating shared rooms or losing private saves |
| SECRET-01 | Four mini-door chill rooms | One optional secret room per player profile, shared visits, plush toys, stars, northern lights | All four can occupy any room; portals, exits, independent saves and older-iPad effects pass |
| HIDE-03 | Enterable furniture and gentle clues | Closets/oversized drawers; configurable 5–10-second clues; parent search around 30 seconds | No trapped avatars, hidden-role leaks, waiting for the next round, or unreachable required finds |
| TRAVEL-01 | Full offline solo on trips; optional internet/hotspot connection to PC/VPS | Installed content and local saves; server wins on rejoin, with local work retained separately | Cold offline launch and installed solo activities pass; route loss/rejoin retains local saves; remote multiplayer qualifies separately |
| AUTO-01 | Automatic family discovery and joining | Foreground native Bonjour/DNS-SD discovery on Apple, Android, and Windows; paired devices, one stable authority; no child Host/Join steps | Either launch order and simultaneous launch work; permissions and offline fallback handled |
| AUTO-02 | **Retired by user decision, September 25: device hosting and automatic host switching** | No implementation or device-test gate; identifier retained for traceability | Removed, not completed |
| OUT-01 | Beach, creek, and park play | 32 outdoor activity designs, including working equipment, tag, hiding, collecting, construction, and water play | Each has touch, solo, co-op, cancellation, save, and device acceptance |
| DAY-01 | Daycare as the sixth world | All child characters available across zones; Calypso routine; optional day with 2–3 saved rotating invitations | Late join and skip do not reroll or interrupt the sibling; all six content regions remain accessible through five destinations |
| LEARN-01 | Spoken playful learning | 12 reading, math, music, social, and science stations; independent assistance | Both children can understand and respond without reading; English complete offline; Spanish reviewed separately |
| IMG-01 | Nine imagination stories | Picture mat, spoken role cards, transformed story zones, NPC role substitutes | All nine support solo and shared play, role/character changes, independent exit, and saved progress |

The expanded tracker covers all 55 chapters and 35 feature IDs. AUTO-02 is explicitly retired; AUTO-01 automatic PC/VPS connection remains required. The six-region content inventory is unchanged; house/backyard use one of five menu destinations.

## 17. Changing characters without stopping the game

### What the child sees

**Updated navigation decision, September 25:** one family-circle/down-arrow chooser combines horizontal full-body characters with vertical scene-filled world circles. Selecting a usable world opens a local loading screen until ready. This supersedes the separate staggered browser, central preview and second Play button. The [reference study](bluey-lets-play-reference-study-2026-09-25.html#5-main-world-menu-contract) records the contract; [native 105 evidence](implementation/combined-chooser-2026-09-25.html) qualifies the prototype on Windows.

The world and the other child keep playing. Opening this tray clears that player's walking input and routes touches to the tray; it must not set the whole simulation's time scale to zero. During a hide-and-seek search, opening a menu does not grant invisibility or move the player to safety. The exit-activity picture remains available if they want to stop playing that round.

Allow both children to be Bluey, Bingo, or any other available child. Keep their persistent player markers distinct through a symbol plus color, such as star and flower. A character's identity does not determine which child owns an object or a save. Quest dialogue should address the current avatar or use a natural general phrase; it must not insist that only Bingo can cook or that only Bluey can complete a task.

### How to implement it

Use a persistent **player identity and gameplay root**, with a replaceable visual child. Store position, input owner, held-object IDs, activity membership, hiding status, and quest participation on the root. The selected character definition supplies the rig, artwork, voice bank, portrait, scale, and attachment mappings.

Unity provides full-skin and runtime sprite-swapping examples. Matching sprite categories and compatible rigs can reuse a skin-swap workflow; very different body shapes still need a different visual rig. Those samples demonstrate the visual technique, not a finished multiplayer character-switch system. [Unity 2D Animation sprite-swap examples](https://docs.unity3d.com/Packages/com.unity.2d.animation@13.0/manual/ex-sprite-swap.html)

Proposed switch sequence:

1. Record a switch request with the player's stable ID and a new request generation. Continue displaying the old character while the new visual loads locally.
2. Validate the requested character. Keep the existing network player object and its ownership. Prepare the new rig at the same foot pivot, facing, and semantic pose: carrying, sitting, hiding, or walking.
3. Complete or safely cancel an in-flight gesture before changing attachments. If water already transferred, preserve that committed amount; do not replay the pour. A prop held by the character moves to the new hand anchor. A prop dragged directly by a finger remains controlled by that pointer/root until released.
4. Commit the selected character ID through the host, then change the visual on both devices. Preserve physical reach and legal floor position; choosing a smaller character must not open an unintended path or create a better hidden state.
5. Rebind mouth animation and future dialogue to the new character. Preserve the existing accessibility treatment for Dougie. Discard stale load completions if the child has already chosen somebody else.

Unity's PlayerObject documentation describes the per-client player object and local-input ownership. NetworkVariables synchronize current state, including to later joiners, whereas an RPC alone is a one-time message. Use that distinction for a persistent selected-character ID and use commands for requesting changes. These are API concepts; use the documentation matching the package version actually installed before coding. [PlayerObjects](https://mp-docs.dl.it.unity3d.com/netcode/2.1.1/basics/playerobjects/), [NetworkVariables](https://mp-docs.dl.it.unity3d.com/netcode/current/basics/networkvariable/index.html)

Load all chooser portraits, but keep a bounded cache of full rigs. Begin with the current two visuals and a small number of likely next choices; measure memory on the A10 before increasing it. A short loading indicator on the requested portrait is acceptable while the existing avatar remains usable. If an asset fails, retain the current character and let the child choose another. Never show a blank avatar, silently reset progress, or load the entire animated roster into memory for instant swapping.

## 18. Quests that are easy to start and easy to leave

### A picture-led activity picker

Use one large **Let's play** picture button, separate from the character portrait. Open a sheet of large illustrations: pizza, cake, fish, hide-and-seek, cleanup, balloon, and other available activities. Show a few cards per page, with a plain picture arrow for more. A card speaks its short invitation when selected and has an obvious Play triangle. Favorites and activities in the current area appear first. Simple Play can show three nearby suggestions and a More button; this is a layout to test with the children, not a fixed developmental rule.

Also allow a direct start from the world: tap the recipe book to cook, the fishing rod to fish, or the parent portrait at the hiding-game mat to invite a seeker. Both routes invoke the same activity. Start ordinary activities within two deliberate taps from the world, without mandatory dialogue trees. Parent actors can walk over after the activity starts; finding a parent across the house must not be an entry requirement.

The active activity shows a small picture badge with **listen again**, **choose another**, and **all done** controls. Keep their positions stable and provide spoken labels. Walking away never puts up a barrier. Briefly moving elsewhere can tuck the prompt away while the activity remains resumable; choosing another activity explicitly parks the previous personal activity. Do not ask the child to confirm every switch or label it failure.

| Child action | Personal activity | Shared activity |
| --- | --- | --- |
| Walk away from the station | Stop reminders; keep saved work available | Others keep playing; walking outside a declared round boundary leaves only this child's participation |
| Choose a different activity | Save a checkpoint and focus the new card | Release this child's role; continue or simplify the remaining game |
| Tap All done | End participation without penalty | End for this child; stop the session only when no participants remain |
| Return later | Offer Resume / New using pictures when meaningful | Offer Join if the activity allows it; show current shared work |
| Change character | Preserve progress and role | Preserve membership and role; update appearance only |
| Ignore an invitation | Keep free play fully available | Do not register them as a participant without their action |

All done means stop participating, not erase the world. A cake stays on the counter, a half-decorated pot stays decorated, and a puzzle can be resumed if its required objects still exist. A new invitation is not permission to remove the other child's plate. Cleanup never becomes a condition for entering the next game.

### Keep objectives separate from toys

An activity observes completed world events such as `FoodServed`, `FishReleased`, `ToyStored`, or `PlayerFound`. The toys work without the activity. Quests should usually recognize a valid existing object or a completed step instead of forcing a child to undo work and repeat it in an arbitrary order. Specific performances, such as catching a new fish this round, should clearly require a new event.

Proposed lifecycle: **available → active → parked → resumed or finished**. Shared rounds additionally maintain their own participant list and phase. A child has one prominently guided personal activity at a time, but can interact with every free-play object and join a compatible shared activity. Store checkpoints separately from the shared world's truth; on resume, re-check object IDs and conditions. If a sibling has served the cake, offer a new cake or another step instead of pointing at a vanished object forever.

Each activity definition needs: ID, source/inspiration, station/location, invitation picture and voice ID, one-person fallback, participation rules, steps/events, resumable state, actor/prop reservations, leave behavior, and cleanup behavior. Runtime instances need unique IDs, participants, phase, completed event IDs, revision, and any reserved resources. Save definition IDs and mutable instance data separately. ScriptableObjects can hold authored definitions; persistent progress still needs the runtime save system described earlier. [Unity ScriptableObject manual](https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html)

Leaving must cancel pending callbacks, release held reservations, stop obsolete dialogue, and restore valid movement. Completion events must be applied once even when packets retry. Resource conflicts should produce a useful choice: another bowl, another free parent, or the activity in solo mode. Do not lock both children to one quest, and do not require them to complete a synchronized gesture together.

A one-room experiment may initially show both children together, but it is only an incomplete prototype. Independent cross-location co-op is required: choosing a destination moves only that player and leaves everyone else's activity running. Prove two independent zones early, before broad content production; see sections 50 and 52.

**September 25 home implementation:** [HOME-01 and room-by-room layout](implementation/home-interactions-2026-09-25.html) adds interactive seating, trampoline poses, radio/music/dancing and shed storage as the first property slice. Windows 114 passes 92 core checks and five native groups. This starts the home feature build; every room, recipe, personal-bedroom and activity goal below remains required. Devices/server remain on the recorded family version until coordinated delivery.

## 19. Kitchen: five pizzas, five cakes, five meals

**Staged chocolate cake — candidate 171, September 27:** newly chosen chocolate cakes now have visible mixing, partial pouring into two tins, safe baking, filling, layer assembly, icing, decoration, slicing and four conserved servings. Normal recipes have relevant ingredient lists; unusual combinations require explicit Experiment mode. [Implementation and applied research](implementation/chocolate-cake-flow-2026-09-27.html). Schema 15/content 16; old dishes keep their original rules and contents. Qualification/delivery is recorded in that report. Other recipe activities and physical child/A10 acceptance remain open; COOK-01 and Home remain Partial.

**Cooking process correction — latest Android 166 feedback, September 27:** cake-making exposes unrelated ingredients and cooking mostly advances through repeated button presses. [Cooking-stage audit, deep research and all fifteen flows](implementation/kitchen-staged-cooking-research-2026-09-27.html) confirms shared ingredient pages, finishing ingredients required before heat, generic step advancement and missing partial preparation states. The required correction is recipe/stage-specific ingredients plus visible mixing, pouring, shaping, appropriate cooking, finishing and serving. First bounded implementation: one complete chocolate-cake sequence, then the other cakes, pizzas and pan/pot meals. Four-player independence and all fifteen recipes remain required. That audit pass changed research/tracking only; the first implemented cake flow is recorded above.

**Latest kitchen correction, September 27:** candidate 166 implements the requested central dish and ingredient bowls, working automatic-tray Cook entry, optional cupboard chores and clear fridge/oven access. [Applied child-interaction research](implementation/kitchen-child-friendly-research-2026-09-27.html) and [implementation/evidence](implementation/kitchen-easy-2026-09-27.html) retain real ingredients, four independent players and all fifteen recipes. Exact pizza toppings persist through baking/slicing; bespoke cake/meal transformations, broader goals below and physical child/device acceptance remain open.

**Implementation checkpoint, September 27:** [Kitchen prototype](implementation/home-kitchen-2026-09-27.html) adds usable fixtures, four independent work/dining places and persistent preparation/heat/serve/taste/wash for all 15 recipes. The detailed behaviors below remain the full goal; a playable path does not complete custom shape/effect, orders, album, drink or physical qualification requirements.

Interpret the request as **three food families with at least five distinct recipes each: 15 minimum**, plus mix-and-match creation. Every recipe below is proposed game content. The show supports the themes: pretend pizza making and delivery in [Pizza Girls](https://www.bluey.tv/watch/season-3/pizza-girls/), the birthday bake in [Duck Cake](https://www.bluey.tv/watch/season-2/duck-cake/), restaurant roles in [Fancy Restaurant](https://www.bluey.tv/watch/season-2/fancy-restaurant/), and kitchen café play in [Pavlova](https://www.bluey.tv/watch/season-3/pavlova/). It does not establish this whole proposed menu as canonical food.

### The initial menu

| ID | Food | Defining play and appearance |
| --- | --- | --- |
| PIZ-01 | Cheese pizza | Spread red sauce, scatter cheese, bake, slice; visible stretchy cheese |
| PIZ-02 | Pepperoni pizza | Add large round pepperoni pieces; slices keep their toppings |
| PIZ-03 | Garden vegetable pizza | Choose capsicum, mushroom, and tomato pieces; colorful arrangement |
| PIZ-04 | Ham and pineapple pizza | Alternate chunky pink and yellow toppings; no exact pattern required |
| PIZ-05 | Silly-face pizza | Make eyes, a nose, and a smile from toppings; photograph the creation in the in-game album |
| CAK-01 | Duck cake | Assemble simple body/head pieces, add beak, eyes, icing, and popcorn-style feathers |
| CAK-02 | Chocolate layer cake | Stack cake layers, spread icing, add chocolate decorations |
| CAK-03 | Strawberry heart cake | Fill a heart mould, add pink icing and strawberries |
| CAK-04 | Rainbow cake | Choose colored batter layers and rainbow decorations |
| CAK-05 | Carrot cake | Stir orange pieces into batter, add pale icing and a tiny carrot decoration |
| MEAL-01 | Burger plate | Stack bun, filling, cheese, and salad; add a side and serve |
| MEAL-02 | Spaghetti and sauce | Add pasta to a pot, stir sauce, serve into a bowl, sprinkle cheese |
| MEAL-03 | Vegetable soup | Drop chopped vegetables into broth, stir, ladle into cups/bowls |
| MEAL-04 | Pancake breakfast | Pour batter, tap/drag to flip with assistance, stack, add fruit |
| MEAL-05 | Rice and vegetable bowl | Stir colorful vegetables, scoop rice, combine and decorate the plate |

These should differ in both assembly and appearance. Do not count five recolors of one item as five complete cooking activities. At the same time, reuse a small set of good interactions: spread, chop, pour, mix, shape, heat, decorate, cut, plate, carry, taste, and wash. The show’s backyard mud-pizza game can later reuse the pizza assembly system with a separate pretend-material set; it is distinct from our kitchen-food activity.

### How a cooking session feels

For Simple Play, open a close-up worktop with large ingredients and the next useful tool ready. A swipe chops with broad snapping; a large circular scribble stirs without needing a perfect circle. Dropping a tray in the oven starts an automatic short bake with an obvious visual change. Supply a tap alternative for each demanding gesture. Let the child skip to decorating a ready-made base whenever wanted.

For Explore & Stories, add the full ingredient sequence, partial mixing/pouring, free topping placement, and optional picture orders. Each step remains reversible where sensible, and a strange combination can become a custom creation. A requested cheese pizza can be a suggestion; it should not turn an unexpected topping into a failed meal or block serving.

Use a large **Make / Decorate / Serve** picture strip, not a long recipe paragraph. The whole creation stays interactive when finished: carry pizza slices on a plate, offer cake to a parent, pack a meal for a picnic, and wash the dishes afterward. Parent customers can react and thank the child, but must not demand cleanup as payment or gate the next recipe. Nothing burns irreversibly while a child explores something else. End heating at a stable ready state and stop relevant effects if the item is removed.

Roles for up to four players should be flexible; the following pairs are examples, not a capacity limit: one adds toppings while the other prepares plates; one mixes while the other decorates a finished cake. Either can change jobs at any moment. Provide up to four independent bowls/trays, work positions and essential tools when needed; no player can lock the whole kitchen. On the same pizza, accept sequential topping placements from both devices; reserve the specific portion/tool transaction rather than locking the whole kitchen for one player.

### Reusable cooking model

Represent an ingredient or dish through an instance ID, recipe family, preparation state, contents, portion count, decorations, and holder/container IDs. A finished dish is still a world object. Proposed stages are **raw/assembled → cooking → ready → decorated/plated → partly served → consumed**, with branches for activities that need fewer stages.

The host validates transforms and transfers. Mixing consumes the participating ingredient instances once and creates one mixture state. Slicing a pizza converts one whole-dish state into a bounded set of portions; it must not leave the whole pizza selectable underneath. Bake state belongs to the dish/station session, not a visual animation or the currently selected avatar. Save topping positions in the dish's local coordinates with stable IDs so carrying a pizza does not scramble its decoration.

Use authored sprite states for dough/cake shapes, bounded topping sockets or a clamped decoration surface, simple fill masks, and short heat animations. Avoid a full fluid or soft-body simulation for cooking. Check missing ingredients, full trays, overlapping drops, simultaneous serving, app suspension, and walking away mid-bake. No live LLM is required to decide a recipe or calculate its result.

## 20. The water feature and fishing mini-game

### What is verified in the show

The official **Tradies** episode page says Sparky and Chippy install a pond in the Heeler backyard, and their character page specifically calls it a **fishpond**. That is the verified house feature to use for this activity. [Tradies, season 3 episode 32](https://www.bluey.tv/watch/season-3/tradies/), [The Tradies character reference](https://www.bluey.tv/characters/the-tradies/)

There is also a separate fountain associated with the school: **Barky Boats** explicitly describes Bluey and Mackenzie racing bark there. Keep those references distinct. We have verified the backyard fishpond's identity, not that the house has a particular fountain jet, exact dimensions, or specific fish species. Use episode imagery when drawing the final pond; any added spout is our adaptation. [Barky Boats, season 2 episode 30](https://www.bluey.tv/watch/season-2/barky-boats/)

Catching fish at this pond is our proposed mini-game; the official references above do not establish that the children play this exact fishing game there. **Rug Island** provides a separate show reference for imaginative fishing. [Rug Island](https://www.bluey.tv/watch/season-2/rug-island/)

### Proposed catch, discover, and release loop

Put two short toy rods and an observation bowl beside the pond. Tap a rod or the fishing picture to start. Choose a visible fish, guide the float, see a generous bite cue, then bring the catch to the observation bowl and release it back into the same pond. Start with five visually distinct fish designs using shape, pattern, and color; these are game designs, not a verified list of species in the show.

| Simple Play | Explore & Stories | Together |
| --- | --- | --- |
| Tap a fish or broad ripple zone; automatically cast nearby | Drag to a chosen zone; choose an optional picture target | Each child has a rod and can fish independently |
| Fish approach slowly; tap a large reel picture or use assisted retrieval | Follow a short forgiving reel/drag cue, with no rapid tapping requirement | One catches while the other looks after the observation bowl |
| No missed-bite punishment, running clock, or line management | Optional collection of five different picture stamps, with no deadline | Shared discovery album recognizes each unique fish type without competition |
| Release with a large fish-to-water arrow | Compare shapes/patterns, then release and try another | Either can stop without ending the sibling's turn |

Use a small pool of fish agents following authored pond paths. The host chooses availability, bite, reservation, and catch results; clients animate fish, float, line, and splash. Approximate motion is fine, but catch identity and ownership must agree. Only one rod can reserve a fish at a time; gently guide the other rod to another available fish. The observation bowl has a visible capacity. A catch cannot complete into a full bowl; offer release or another empty spot.

Proposed fish states: **swimming → approaching → reserved → caught → observation → released**. Canceling a cast, leaving the area, closing a session, or losing the connection releases the reservation and returns unfinished catches to a valid water state. Never attach a fish permanently to a destroyed rod. The discovery album can save progress without removing fish from future play.

Both this pond and later Barky Boats can reuse water visuals, but keep their interaction zones explicit: fish-catching targets, boat-floating targets, and bucket-filling targets must not compete unpredictably for the same drop.

## 21. Five cleanup games that remain playful

**Daddy Robot** turns tidying into pretend play; **Duck Cake** connects helping with a warm response; **Bin Night** shows a recurring family chore; **Rain** includes muddy tracks. These supply themes, not a reason to enforce chores in our app. [Daddy Robot](https://www.bluey.tv/watch/season-1/daddy-robot/), [Duck Cake](https://www.bluey.tv/watch/season-2/duck-cake/), [Bin Night](https://www.bluey.tv/watch/season-2/bin-night/), [Rain](https://www.bluey.tv/watch/season-3/rain/)

| Activity | Simple Play | Deeper / shared play | Implementation and recovery |
| --- | --- | --- | --- |
| Toy tidy / Daddy Robot helpers | Put any loose activity toy in a big basket | Sort by picture; one collects while the other operates the helper basket | Tag the session's loose props; never collect a held toy or an intentional construction |
| Dishwashing bubbles | Rub a plate with a sponge; broad strokes make it clean | Wash, rinse, and rack several dishes; split jobs | Dirt amount and wetness are bounded states; a completed dish stays clean |
| Puddle and muddy-footprint cleanup | Drag a mop through large patches | Follow a trail together; squeeze the mop into its bucket | Use masks/patches with capped counts; cleaning cannot duplicate water |
| Laundry and dress-up basket | Put large clothes pictures into matching shape baskets | Wash, hang, dry, and return accessories; both can arrange a line | Clothing is an object with dry/wet/clean states; undressing a player requires that player's action |
| Garden helpers / Bin Night | Collect leaves or clearly marked toy litter | Use picture-coded destinations, water a newly cleared patch | Recognize generated cleanup props separately from the sibling's sandcastle, shell art, or boat |

Always keep an All done picture. A pleasing clean patch, a squeak, or a parent's brief thanks is enough feedback. Optional stickers can celebrate discoveries but unlock nothing essential. Do not keep spawning mess faster than a child can remove it, and do not make a single missed tiny patch prevent a satisfying end. Use generous completion thresholds and tidy the last visual flecks automatically in Simple Play.

Give each mess patch/object a stable ID and remaining amount. The host applies cleaning once and emits one completion event; two simultaneous sponges can both visibly scrub without counting the same dirt twice. A cleanup session operates on an explicit set of eligible objects. Parking or leaving that activity leaves the world as it is. Automatic session cleanup may remove only its disposable temporary props after participants release them; valuable creations remain saved. Separately, section 51 adds the confirmed world housekeeping policy: eligible unused shared loans return home, while personal objects and saved creations remain protected.

## 22. Hide-and-seek and parents with everyday routines

**Updated requirements:** section 34 adds enterable closets/drawers, configurable 5–10-second clues, a roughly 30-second parent search per ready hider, and immediate mid-round joining. It takes precedence over the earlier outline below where more specific.

<a id="first-mode-bandit-or-chilli-finds-both-children"></a>

### First mode: Bandit or Chilli finds up to four players

The show’s **Hide and Seek** has the family hiding while Bluey counts and searches; distraction is part of the story. Our parent-seeker mode adapts the roles to let both children cooperate. [Official Hide and Seek episode](https://www.bluey.tv/watch/season-1/hide-and-seek/)

Here, each child physically moves **their game character on their own iPad** to a hiding place. This design uses the game world, not cameras or tracking of children hiding around your real home.

1. Tap the hiding-game picture and choose a parent portrait. The initiating child joins; the sibling gets a small invitation and can join or keep doing something else. One hider is enough if the other declines.
2. The parent turns away and counts with spoken numbers and a visual progress cue. A large Ready picture confirms each child's readiness. Start searching for ready participants without forcing an unready three-year-old into the search; preparing/late-joining children use the individual preparation rules in section 34.
3. In Simple Play, tap a large hiding-place picture—closet, oversized play drawer, tent, or other legal cover—and the avatar walks there. Dragging or normal walking also works. Keep legal spots obvious on the hider's view only. Explorer mode can let the older child choose among more valid cover regions.
4. The parent searches while each child's membership changes independently. Hiders may join or leave at any point; an existing search never restarts just because another child joins. If the last participant leaves, the parent ends the game and resumes everyday behavior.
5. When found, a child receives a warm reaction and remains free to play. They may help search if they choose, watch, or start another activity; there is no elimination screen forcing them to wait.
6. After the remaining hiders are found or leave, offer Play again / Swap roles / All done as pictures. Only start a new round when chosen.

Start in one room or a clearly bounded garden zone, with a few well-tested hiding spots. Expand to multiple rooms only after navigation and visibility are reliable. Hiding places need entry/exit anchors, capacity, cover geometry, available approach points, and a fallback position. If a movable prop supplies cover, reserve its structural role during the round or clearly reveal the hider if it is moved; never leave an invisible child standing in open space.

### Second mode: one child hides, the other seeks

Use two large role cards: a hiding face and a magnifying glass. The second player sees the complementary role and chooses to join. Roles belong to player IDs, so character switching never escapes being found or changes who seeks. Let them swap roles between rounds without changing selected characters.

During hiding, the seeker's own screen shows the counting parent/eyes-closed view instead of following the hider. During searching, restrict the seeker's camera to their activity area and remove hidden avatars, their nameplates, floor markers, touch hitboxes, item outlines, and positional voice cues from that view. The hider still sees their own character. Carried props belong to the hidden presentation too; game-world occlusion and interaction visibility must agree.

This prevents accidental in-game giveaways, although children sitting next to each other can still look at the other iPad. A hidden avatar is revealed by a valid Find action near a checked hiding spot, or by visible exposure according to the game’s rules—not by tapping through furniture from across the room. Section 34 adds configurable automatic giggles/rattles and a manual cue option; changing character never triggers an unintended clue.

If a hider leaves, mark them as having left and continue the remaining search. If the child seeker leaves, a parent takes over by default while the remaining hider retains an easy exit. Mid-round newcomers may join immediately using their own preparation state and timer; they do not have to wait for the next round and are not silently made findable before they are ready.

### A parent who searches fairly

Use a small authored state machine: **count → patrol → inspect spot → notice visible player → approach/reveal → continue or finish**. The host has the complete world state, but the AI's decisions should use its own observations: view direction, distance, unobstructed 2D sight lines, inspected locations, and optionally a heard clue. Do not secretly steer directly to the current hiding coordinate while pretending the parent is searching.

A shuffled list of legal spots can drive an understandable search, with visible pauses and occasional harmless wrong guesses. Search order should not be picked from the actual occupied spots. Revealing a spot requires reaching its inspection anchor. If a child runs into view, use a short recognition grace period before a friendly found animation. Start with a slow parent and forgiving cover; all timing and distance values need child playtesting rather than guessed claims about what a three-year-old can manage.

Use the same floor graph as walking, plus explicit cover polygons and sight-blocking layers. A blocked route triggers a replan to another legal approach; a watchdog returns the parent to a reachable anchor if no progress occurs. Keep navigation independent of sprite draw order. Both devices receive authoritative round state and found events, while their local cameras and hidden-avatar rendering differ.

### Parents between games

Bandit and Chilli should have small, believable, authored action sets: read on the sofa, water a designated pot, stir a meal at an unused station, look at a picture, stretch in the garden, bring a cup to the table, or put a disposable toy in a basket. Weight choices by room, recent actions, free stations, and cooldowns so they do not switch tasks every second. These are proposed routines; we are not claiming the show uses a particular schedule.

Use **Idle → Choose task → Walk → Perform → Finish** for ordinary behavior. A task includes its target anchor, duration range, held prop, eligibility conditions, and safe interruption point. The host decides the task and motion; clients show the resulting animation. Start with two parents and a modest decision cadence, then profile. A live AI model, cloud service, or generated dialogue at runtime is unnecessary.

Priority order: **recover invalid state → active game role → direct child request → ambient routine**. A request cancels an ambient task safely, puts its prop down or releases its station, and moves the parent toward the child. If a parent is already seeking, another child can call the other available parent or use a station's solo version. One request must not silently steal the seeker from the sibling's game. If both parents are occupied, let the new activity begin without its optional parent role and offer a parent later.

Protect the children's work: ambient adults must not eat their finished cake, wash away their painting, dismantle a fort, take a held toy, or remove a hiding obstacle. Ambient props can be separately tagged or copied as decorative non-inventory props. Any adult interaction that genuinely changes a shared world object goes through the same reservation and event rules as player actions.

On activity end, release actor reservations, cancel pending speech, restore ordinary movement/visibility, and resume a sensible task from the current location. Do not warp the parent visibly across the room just because a quest stopped. Recovery from unreachable destinations must finish promptly and leave the activity usable. A simple explicit C# state machine is sufficient for the first two parents; a visual behavior-tree package is optional and should not be a new dependency just for the appearance of randomness.

## 23. Show games and activities catalog

**32 possible activities, grounded in 30 distinct official episode pages.** Some are named games; others adapt an activity shown in the episode. Each first column links its source. Only the short “show reference” column states what the source establishes. The age adaptations, locations, cooperative roles, controls, and build priorities are our design proposals. Ground's Lava / Postman and Obstacle Course / Memory Snap intentionally share episode sources but describe different play systems.

Priority **A** means a strong early candidate after the shared interaction foundation; **B** means a later content batch; **C** means more specialized movement, animation, or coordination work. These labels prioritize production, not the children's access to released activities. Kitchen, fishing, cleanup, both hiding modes, and free switching remain required regardless of this optional show backlog.

### Household pretend play

| ID / official episode | Show reference | Simple Play | Explore & Stories / playing together | Reusable system / priority |
| --- | --- | --- | --- | --- |
| SHOW-01 [Hide and Seek](https://www.bluey.tv/watch/season-1/hide-and-seek/) | Bluey searches for hidden family members | Tap a hiding place while a parent seeks | Both hide, or alternate child seeker/hider roles | Cover, perception, roles; **A** |
| SHOW-02 [Daddy Robot](https://www.bluey.tv/watch/season-1/daddy-robot/) | The children enlist a pretend robot for tidying | Feed a toy to a large helper basket | One collects; one directs the robot to picture destinations | Cleaning, parent role, pickup; **A** |
| SHOW-03 [Hotel](https://www.bluey.tv/watch/season-1/hotel/) | The children run a hotel with Dad as guest | Put a pillow on a guest bed; ring a bell | Reception, room setup, and snack delivery; freely exchange jobs | Stations, customer reactions; **B** |
| SHOW-04 [Shops](https://www.bluey.tv/watch/season-1/shops/) | Friends negotiate shop roles and scan items | Drag an item across a big scanner for a satisfying beep | One shops while the other scans and bags; roles never block starting | Containers, scanning, role play; **A** |
| SHOW-05 [Taxi](https://www.bluey.tv/watch/season-1/taxi/) | An indoor pretend taxi trip has comic interruptions | Tap a destination picture; an assisted taxi follows a safe route | Driver, passenger, and mechanic activities; let either child change roles | Authored routes, seated anchors; **C** |
| SHOW-06 [Trains](https://www.bluey.tv/watch/season-2/trains/) | Dad is a train linking pretend daily destinations | Tap a station to ride and ring the bell | Choose stops, deliver passengers, and set up destination play areas | Route graph, passenger attachment; **C** |
| SHOW-07 [Hospital](https://www.bluey.tv/watch/season-1/hospital/) | Doctor and nurse play reveals a silly animal in Dad's tummy | Tap a toy scanner and place a bandage sticker | One scans, one offers toy treatments; the patient gives funny reactions | Tool targets, character states; **B** |
| SHOW-08 [Grannies](https://www.bluey.tv/watch/season-1/grannies/) | Janet and Rita dress up, cause mischief, and dance | Put on glasses and a blanket, then choose a dance | Both invent a granny scene and tidy pretend spilled beans afterward | Wearables, poses, spill cleanup; **B** |

### Cooking, serving, and making things

| ID / official episode | Show reference | Simple Play | Explore & Stories / playing together | Reusable system / priority |
| --- | --- | --- | --- | --- |
| SHOW-09 [Pizza Girls](https://www.bluey.tv/watch/season-3/pizza-girls/) | The children make mud pizzas and deliver them | Decorate a base and serve to a nearby parent | One prepares; the other delivers; add a toy-car mechanic later | Recipe, delivery, optional vehicle; **A** |
| SHOW-10 [Duck Cake](https://www.bluey.tv/watch/season-2/duck-cake/) | Dad builds Bingo's cake; Bluey helps with the mess | Decorate a ready-shaped duck | Assemble, ice, decorate, serve, and optionally clean together | Cooking, decoration, cleanup; **A** |
| SHOW-11 [BBQ](https://www.bluey.tv/watch/season-1/bbq/) | Bingo gathers colors for a pretend salad | Put large colorful ingredients in a bowl | One gathers, one mixes and sets the table; no urgent customer demands | Gathering, mixing, serving; **A** |
| SHOW-12 [Fancy Restaurant](https://www.bluey.tv/watch/season-2/fancy-restaurant/) | Bluey and Bingo host their parents for a meal | Place one plate and serve a creation | Make a menu, prepare food, and take turns as chef and server | Shared kitchen, guest reservations; **A** |
| SHOW-13 [Burger Shop](https://www.bluey.tv/watch/season-2/burger-shop/) | Bath play becomes a burger shop | Stack three large burger pieces | Assemble different picture orders while the other child plates or washes | Layering, order hints, washing; **A** |
| SHOW-14 [Pavlova](https://www.bluey.tv/watch/season-3/pavlova/) | Kitchen café play includes a competing pretend chef | Add fruit and decorations to a dessert | Run adjacent café counters and serve each other, without a winner requirement | Decoration and customer reactions; **B** |
| SHOW-15 [Cubby](https://www.bluey.tv/watch/season-3/cubby/) | Cushions and blankets form an expanding playhouse | Snap a cushion or blanket into a broad target | Build connected nooks, furnish them, and place a toy inside | Stable sockets, saved constructions; **B** |
| SHOW-16 [Rug Island](https://www.bluey.tv/watch/season-2/rug-island/) | Felt pens become objects in an imaginary island world | Arrange oversized pretend sticks as food or a shelter | Build a shared island and act out fishing/foraging with a parent | Prop arrangement, pretend variants; **B** |

### Magic, music, and shared toys

| ID / official episode | Show reference | Simple Play | Explore & Stories / playing together | Reusable system / priority |
| --- | --- | --- | --- | --- |
| SHOW-17 [The Magic Xylophone](https://www.bluey.tv/watch/season-1/the-magic-xylophone/) | A pretend instrument freezes Dad | Tap a note to freeze/unfreeze a willing parent in a funny pose | One plays, the other adds a hat or chooses a pose; share turns | Temporary effects, NPC role; **A** |
| SHOW-18 [Feather Wand](https://www.bluey.tv/watch/season-2/feather-wand/) | Bingo's feather makes objects pretend-heavy | Tap a prop and watch a parent struggle comically | Change heavy/light states to arrange a silly obstacle scene | Temporary state, reactions; **B** |
| SHOW-19 [Asparagus](https://www.bluey.tv/watch/season-1/asparagus/) | Magic asparagus makes family members act like animals | Choose an animal picture; a parent imitates it | Both build a pretend zoo and switch animal roles | Animation/voice variants, reversible effects; **B** |
| SHOW-20 [Musical Statues](https://www.bluey.tv/watch/season-3/musical-statues/) | The family dances and plays with stop/start rules | Tap to dance; automatically settle into a pose when music pauses | Choose poses and respond to a generous visual/audio stop cue together | Shared clock, animation, audio; **B** |
| SHOW-21 [Dance Mode](https://www.bluey.tv/watch/season-2/dance-mode/) | Bingo can prompt her parents to dance | Big dance picture triggers a funny parent routine | Take turns selecting moves or build a short joint routine | Parent role, queued reactions; **B** |
| SHOW-22 [Pass the Parcel](https://www.bluey.tv/watch/season-3/pass-the-parcel/) | Party passing uses different prize rules | Assisted passing, followed by a big unwrap gesture | Children choose wrapping and pass to each other or NPCs; no elimination | Container layers, turn transfer; **B** |
| SHOW-23 [Barky Boats](https://www.bluey.tv/watch/season-2/barky-boats/) | Bluey and Mackenzie race bark in a school fountain | Put a decorated boat in a wide launch area | Each makes a boat; guide both to a landing or take turns sending a leaf passenger | Floating path, decoration, shared water; **B** |
| SHOW-24 [Ragdoll](https://www.bluey.tv/watch/season-3/ragdoll/) | The children try to move a floppy Dad | Drag a large handle to help Dad onto a cushion | Alternate gentle pushes along a short shared route | Authored floppy poses, joint contribution; **C** |

Barky Boats is included here as another gentle turn-taking activity. Its school-fountain setting is separate from the backyard fishing pond. Our first adaptation can use the already planned creek/water tray; recreating a school location is optional later work.

Temporary magic effects must not freeze the sibling's controls indefinitely. Target consenting parent actors by default. A child can opt into a playful effect and always cancel it. When an activity is left, remove its temporary gameplay restrictions while retaining intentional dress-up choices. Musical games need a host clock and forgiving input windows; compare the actual iPad audio/visual timing before using accuracy judgments. The younger child's version should remain fun even if they never follow a beat.

### Outdoor, movement, and family routines

| ID / official episode | Show reference | Simple Play | Explore & Stories / playing together | Reusable system / priority |
| --- | --- | --- | --- | --- |
| SHOW-25 [Keepy Uppy](https://www.bluey.tv/watch/season-1/keepy-uppy/) | The family keeps a balloon off the ground | Start on the clear lawn right of the trampoline; mix current-height and occasional higher returns with varied faster/farther sideways motion; retain the quicker descent and movement while tapping; automatic raised-arm return underneath; floor rest/tap restart, no score/win/loss | All four bat the same balloon; Home only initially | PC-authoritative flight; SHOW-25 first requested minigame; [implementation](implementation/keepy-uppy-2026-09-26.html) |
| SHOW-26 [Shadowlands](https://www.bluey.tv/watch/season-1/shadowlands/) | Children move between shadows and avoid sunny grass | Tap a large nearby shadow and auto-walk there | Choose a route together; later add slowly moving shade | Valid zones, guided routes; **C** |
| SHOW-27 [Postman](https://www.bluey.tv/watch/season-2/postman/) — Ground's Lava | The sisters avoid touching the floor | Tap big cushions; automatic safe hops | One arranges a route, the other crosses; either can leave freely | Safe nodes, placement constraints; **B** |
| SHOW-28 [Postman](https://www.bluey.tv/watch/season-2/postman/) — letter delivery | A letter becomes a paper plane to reach Mum | Deliver a picture card to a nearby mailbox | Decorate, fold, and send letters to each other or a parent | Craft transform, delivery, simple flight; **B** |
| SHOW-29 [Obstacle Course](https://www.bluey.tv/watch/season-3/obstacle-course/) | A backyard course becomes a race | Follow three broad stations without a timer | Build routes and take turns; personal timing is optional | Authored movement stations; **C** |
| SHOW-30 [Obstacle Course](https://www.bluey.tv/watch/season-3/obstacle-course/) — Memory Snap | The official page also identifies Memory Snap | Match two large face-up pictures | Use a few face-down pairs and cooperate on a shared board | Card matching, turn-optional input; **B** |
| SHOW-31 [Rain](https://www.bluey.tv/watch/season-3/rain/) | Bluey and Mum try to dam rainwater | Drop a broad barrier into a shallow channel | Arrange channels and barriers together, then clean muddy tracks | Bounded flow graph, water visuals; **C** |
| SHOW-32 [Bin Night](https://www.bluey.tv/watch/season-2/bin-night/) | The family regularly takes out the bins | Put a marked cleanup prop in a picture bin | Collect, sort, and wheel bins with a parent | Cleanup tags, route and container; **B** |

**Catalog count note:** this first 32-card selection uses 30 distinct episode pages. Postman and Obstacle Course each provide two cards; the remaining entries are single cards. Card count is not a claim of 32 unrelated engines or 32 episodes.

The [activity source and design catalog](bluey-research/activity-research-evidence.json) records the source URLs, page-check results, and these proposed adaptations in a machine-readable format for later content work.

A useful additional candidate is [Pirates](https://www.bluey.tv/watch/season-1/pirates/), which supplies a swing-ship adventure with parent roles. A gentle adaptation could have one child steer while the other spots picture landmarks. This needs more bespoke character animation and camera comfort testing than the earliest activities.

**September 26 order update:** the user selected Keepy Uppy as the first home minigame. Finish and qualify its four-player free play before returning to kitchen/cleanup/fishing/parent-seeker activities, Shops and Magic Xylophone. All new shared features support four players; the sofa and trampoline use four closer spots without enlarging their artwork. Do not build all 32 before children can try a finished small area. Unfinished ideas stay visible here as planned backlog, not as dead buttons in the children's app.

## 24. Integration, build sequence, and acceptance for the new features

### One activity framework

| System | Owns | Must not own |
| --- | --- | --- |
| Player identity | Child/profile ID, input ownership, chosen avatar, preferences | A quest's world objects or the parent NPC's task |
| World objects | Contents, preparation, decoration, dirt, location, stable instance IDs | Whether the child is currently watching a quest card |
| Activity session | Participants, phase, event progress, optional roles and reservations | Permanent permission to block movement or seize the sibling's props |
| Parent director | Current task, actor reservations, interruption/recovery | Hidden overrides to cooking, inventory, or saved player progress |
| Presentation | Camera, help, voices, local hiding visibility | Authority to award catches, consume ingredients, or mark another child found |

This separation lets a child switch from Bingo to Muffin, park a cooking invitation, and start fishing while their cake remains on the counter. The revised requirements in sections 31–33 now require independent room/location travel. Prove it in the house first, then across the six finished locations; the sibling's activity must remain active.

Keep definitions in editable assets and store runtime state in a versioned save. Add chosen avatar, parked activity checkpoints, dish variants/decorations, fish discoveries, cleanup state, and parent task IDs as needed. Do not save raw network connection IDs as the permanent identity of a child. On load, rebuild references by stable IDs and release stale temporary reservations. A hide-and-seek round can safely resume at an explicit setup checkpoint; do not restore a half-running count with missing participants.

Network commands should carry activity/session ID, acting player, target object IDs, expected state revision, and an idempotency key where a retry could duplicate a result. The host applies each valid state change once. Replicate the resulting state for reconnects, not only an animation trigger. Important interactions must remain correct even if a cosmetic effect is dropped.

### Updated production batches

These refine section 14; they do not change the Windows/Mac build split or the required signing-renewal setup.

| Batch | Work | Exit gate |
| --- | --- | --- |
| Foundation | Water bench; stable player/world IDs; basic LAN; save checkpoints | Touch, ownership, and recovery pass on both iPads |
| Freedom to play | In-play character drawer; picture activity picker; start/park/leave/resume | No scene restart, lost props, or stuck activity after switching |
| Kitchen and cleaning | One pizza, one cake, one meal, and dishwashing; then expand to all 15 and five cleanup types | Recipes share working rules; both children can contribute or leave |
| Living parents | Two parent rigs, authored routines, request interruption and reservation rules | Parent requests cannot deadlock or destroy children's work |
| Hiding and pond | Parent-seeker hide-and-seek, then child-seeker; pond fishing | Fair visibility, valid exits, consistent catches and round state |
| Show expansion | A small selected set from section 23, then additional batches | Each activity is fun in Simple Play and supports leaving/rejoining |
| Books, TV, and dinosaurs | One narrated book, six starter toys, one supplied clip, and discovery mat; expand using section 29 | Independent reading/watching and shared toy play pass on both iPads; all approved content receives voice/art and device checks |
| Family release | English complete, later Spanish complete; release profiling; updates/renewal | All original gates plus the new checks below pass |

### Required new checks

| Scenario | Expected result |
| --- | --- |
| Switch avatar while carrying a full bucket | Same player, position, water amount, and bucket instance; new hand anchor on both iPads |
| Tap several character portraits while assets load | Only the latest valid request commits; no blank rig or unbounded cached assets |
| Switch while hidden, frozen by an opted-in effect, or in a cooking role | Gameplay role and visibility rules remain consistent; no escape/reset exploit |
| Both children select the same favorite | Distinct player markers and independent controls; neither is forced to change |
| Open a local menu during multiplayer | Other player and world continue; closing does not produce a stuck joystick |
| Leave every recipe at every step | No destroyed dish, locked station, endless sound, or forced return |
| Start a new activity while the sibling continues the old one | Only the departing player's role changes; others remain able to finish |
| Resume after the sibling moved/served the required object | Revalidate progress and offer a useful next action; never point to missing data |
| Two players add toppings, slice, or serve simultaneously | Each accepted action applies once; no duplicate dish or negative portions |
| Walk away from the oven, then return or relaunch | Dish rests at a valid ready/checkpoint state; no irreversible surprise loss |
| Two rods target the same fish, or disconnect during a catch | One reservation/catch; unfinished fish return to swimming |
| Clean together near a saved fort or food creation | Eligible dirt disappears once; deliberate creations remain intact |
| Parent receives a request while holding a cup | Safe release or valid carry transition; ambient task fully cancels |
| One parent seeks while the other child asks for a meal | Seeking continues; other parent or solo service handles the new request |
| Hider/seeker leaves, backgrounds, or disconnects | Membership updates, temporary visibility restores, no endless searching |
| Seeker taps/zooms near a hidden player | No through-wall hitbox, marker, subtitle, held item, or voice leak |
| NPC route is blocked by a moved prop | Parent replans or recovers; the children can still leave/start something else |
| Round is reopened after a suspended session | Coherent setup or current valid state; no premature found event |
| Repeated cooking, avatar swaps, and activity switches on A10 | Stable measured responsiveness and bounded memory, effects, audio, and object counts |

Add play observations: can the younger child change character and start a different game without reading; can either child leave without asking for adult help; does the parent search feel understandable; do finished foods remain interesting toys; and does each sibling find something to do while the other is occupied? These outcomes matter more than quest completion percentages.

**Tracking rule:** mark a feature implemented only when code and content exist, and mark it device-verified only after the relevant physical iPad checks. This update is research and design work; no Unity systems, scenes, or iPad build were changed.

## 25. Reading nook and interactive books

**Native reader picture controls - September 28:** BOOK-01 / H-19 / H-20 now uses three translucent pictured actions, fixed arrows, Books/Home and optional reading settings. Candidate 198 passes eight native groups: four-reader independence, retained 194 bookmarks, all six draft books / 54 pages, explicit audio, cancellation, layouts and lifecycle. [Implementation and evidence](implementation/native-reader-controls-2026-09-28.html). Windows and signed Android sources/artifacts match; schema 21/content 22 and the core are unchanged. No device or production-server update. Final subjects, physical audio/A10, the inherited Android 16 KB gate, native coloring/cooking controls and the full Home backlog remain open. Preserve the branch integration hold.

**Accepted picture-button style — September 28:** the user wants the new science style retained across mini-games and reading. [Control review, implementation and evidence](implementation/home-picture-controls-2026-09-28.html): all fifteen browser science activities now have pictured optional tools; a matching six-book reader preview uses large, translucent picture controls, deliberate Read to me, fixed arrows and independent bookmarks for four readers. Existing book art/audio is reused. Fifty-five test groups pass. This is a browser preview, not a native game or device update; native cooking/coloring/reader integration and the broader Home backlog remain open.

**Placement correction, September 26:** the shared book collection goes on the **first floor around the living room**, accessible to everyone. Do not seed separate bedroom/secret-room collections. Carryable books may travel upstairs; all four players retain independent reading access and bookmarks. [Applied book/plush/device research](implementation/home-reading-quiet-play-research-2026-09-26.md).

**Yes: picking up a house book can open a full-screen, talking, interactive picture book.** Build this as a reusable Unity book reader. Each book supplies pages, artwork, recorded narration, and a few interactive picture regions. This suits the existing illustrated style and can run offline; a live AI model is unnecessary on the iPads.

Place a low, face-out bookshelf, rug, cushions, and a dinosaur book basket in the house. The reading nook, TV, and dinosaur mat belong inside Heeler Home, so the five-destination chooser stays intact. The proposed furniture arrangement is our game layout, not a claim about the exact floor plan in the show.

### From shelf to story

1. The child taps a book cover to pick it up and open it. Dragging a book onto their character or the reading mat also opens it. A deliberate drag to another shelf position simply moves the book; distinguish a tap from a drag before opening anything.
2. Show its cover on first opening, or its saved page on a later visit. A large triangular **Play** button starts narration. Merely picking up the book does not start the story audio.
3. Use one illustrated page across the landscape screen, with a small caption area. Avoid a tiny two-page spread or a realistic 3D page-turn interface. Large arrows work without a precise swipe.
4. Tap a dinosaur or its speaker badge to hear its name. Tap a relevant picture region to make it blink, take a few steps, lift its head, or sway its tail. The same hit area can trigger both a name and a short movement.
5. Play becomes **Pause** while reading. A replay button repeats the current page. In Read to Me mode, advance after the narration and a short pause; a parent setting can select manual page turns instead. Neither mode requires a correct answer.
6. A persistent house/close button returns to the room and saves the page. Opening another book is always allowed. Finished books remain replayable.

The play and close controls need consistent locations, generous spacing, and large hit areas. Start with the project's large preschool touch targets, then check their actual physical size on both 10.2-inch iPads. Spoken button labels and clear icons carry the meaning; written labels are additional help. Hide the joystick and block taps from falling through into the room while the reader is open.

| Mode | Reader presentation | Interaction |
| --- | --- | --- |
| Simple Play | One clear picture focus, short spoken lines, Play/Pause, two page arrows, and home | One to three obvious hotspots; taps work anywhere over the illustrated subject; no required dragging or reading |
| Explore & Stories | Same story and pictures, with optional caption highlighting and an extra fact button | More things to inspect, optional picture-finding invitations, and replayable names; nothing prevents turning the page |

**Research finding:** a meta-analysis of 43 studies involving 2,147 children found small benefits from technology-enhanced stories overall, with useful results from multimedia features but distraction from many interactive additions. This supports keeping movement related to the story and limiting competing tasks. The proposed one-to-three-hotspot limit is our design choice, not a number established by the study. [Takacs, Swart & Bus, 2015](https://journals.sagepub.com/doi/full/10.3102/0034654314566989)

### Sharing the house without sharing a playback cursor

The physical book prop has a world object ID; its readable content has a separate book ID. Moving a book does not erase another player's access to its pages. All four players may read the same title independently on different pages, or mix reading with other activities. A local reading panel must not pause the PC/VPS world or set global game time to zero.

Give a reading avatar a seated/reading pose where practical; book opening should not wait for a long walk to a chair. Release the reader's temporary activity reservations using the existing leave/park rules. In hide-and-seek, opening a book withdraws that child from the round; the parent can keep seeking the remaining child. Closing the book returns control without restoring a stale role or a stuck movement touch.

Keep the book readable through network changes. Page and narration position are local; resuming shared play loads the current server room without importing offline edits.

## 26. Dinosaur books and spoken names

### Six distinct starter books

**Book subjects — latest user correction, September 26:** the six books are **dinosaurs; snakes and reptiles; cars and trucks; Hello Kitty; Tangled; and unicorns**. The user explicitly rejected the assistant-selected space, fairy-garden, invented-princess and mermaid subjects. Use familiar Hello Kitty and Tangled characters/stories; Tangled follows Rapunzel, not an invented replacement princess. The specific unicorn story/character is awaiting the user’s reply. All four players can read all six. Rapunzel’s speaking voice remains the requested locally generated narrator reference. Do not produce further assets for the superseded subjects or treat their draft output as accepted.

| Working title | Proposed length | Content and little interactions |
| --- | --- | --- |
| **Hello, Dinosaurs!** | 14 pages | Requested dinosaurs and pterosaurs, names, feature cues and imaginative creature calls. |
| **Snakes and Reptiles** | To storyboard | A factual picture book with snakes and other reptiles, clear names, distinctive features and gentle effects. |
| **Cars and Trucks** | To storyboard | Cars plus recognizable working trucks; vehicle names, what they do and deliberate engine/work sounds. |
| **Hello Kitty** | To storyboard | Familiar Hello Kitty characters and a recognizable story context; do not substitute unrelated invented characters. |
| **Tangled** | To storyboard | Rapunzel’s movie story and recognizable cast, tower and lanterns, using newly written narration rather than copied dialogue. |
| **Unicorns** | Pending specific story choice | User requested unicorns; ask which familiar story/character or picture-book direction before inventing one. |

The dinosaur book uses twelve creature introductions plus cover and final choice page. Pteranodon and Quetzalcoatlus are identified as flying reptiles. The other five books require revised storyboards for the user-selected subjects; their length is not yet finalized. Boyish/girly is planning shorthand for the requested theme balance, not a rule about who enjoys or can read a book.

### First book: original storyboard, expanded by the latest request

The original eight-page draft below is historical content guidance. Expand it to fourteen pages to include the requested additional creatures; do not omit them to preserve the old page target.

The following narration is a short original draft. The museum links are factual references; final art, pronunciations, audio, and translations still require review.

| Page | Spoken draft | Tap response |
| --- | --- | --- |
| 1 — Cover | “Hello, dinosaurs! Let's meet some dinosaur friends.” | Press Play to begin; a pictured toy gives a little wave of its tail |
| 2 — Tyrannosaurus rex | “Tyrannosaurus rex. This dinosaur walked on two legs. Tap to see it take a step.” | Two slow steps; optional soft pretend roar; repeat its full name |
| 3 — Triceratops | “Triceratops. Look at its three horns. One, two, three!” | Gently highlight each horn, then a small head movement |
| 4 — Stegosaurus | “Stegosaurus. Look at the plates along its back.” | A gentle highlight passes along the plates; body rocks slightly |
| 5 — Brachiosaurus | “Brachiosaurus. What a long neck!” | Lift its head toward an illustrated leaf |
| 6 — Diplodocus | “Diplodocus. Look at that long tail.” | Slowly sway the tail without covering the page buttons |
| 7 — Ankylosaurus | “Ankylosaurus. Its tail ends in a club.” | Turn a little so the club is visible; no hitting other characters |
| 8 — Goodbye | “Which dinosaur would you like to play with? You can hear their names again.” | Six portrait buttons replay names; home returns to the room; no mandatory quiz |

Factual references for those pages: [Tyrannosaurus](https://www.nhm.ac.uk/discover/dino-directory/tyrannosaurus.html), [Triceratops](https://www.nhm.ac.uk/discover/dino-directory/triceratops.html), [Stegosaurus](https://www.nhm.ac.uk/discover/dino-directory/stegosaurus.html), [Brachiosaurus](https://www.nhm.ac.uk/discover/dino-directory/brachiosaurus.html), [Diplodocus](https://www.nhm.ac.uk/discover/dino-directory/diplodocus.html), and [Ankylosaurus](https://www.nhm.ac.uk/discover/dino-directory/ankylosaurus.html). These sources also give pronunciation guides. For example, the museum renders Triceratops as “tri-SERRA-tops” and Brachiosaurus as “BRAK-ee-oh-sore-us”; the saved reference catalog includes guides for all 20 toys.

### Make speech predictable

Use pre-generated, reviewed recordings, stored locally. The existing ComfyUI/Windows voice workflow can produce candidate narration and dinosaur names. Generate one sentence or short passage at a time so a pronunciation can be fixed without rerecording a whole book. Keep lossless masters for editing, then use suitable Unity audio import settings for shipping. No cloud voice call should be required when a child presses Play.

- **One foreground speaking voice per iPad.** Name playback temporarily pauses story narration. When the name finishes, narration resumes at the interrupted sentence only if the child had been playing and has not since pressed Pause, changed page, closed the book, or opened TV.
- Repeated taps replace or coalesce pending name requests; they must not queue twenty spoken names. Short animation taps can play without interrupting a sentence if they add no speech.
- Lower background music and nearby character chatter while a book speaks. Keep voice, music, and effect volume settings separate. A roar is optional and stylized, not a reconstruction of a known dinosaur voice.
- Use one stable content ID across the book, the toy, and the mini-game. That makes the same reviewed pronunciation available everywhere.
- Store English and Spanish audio separately under the same content ID. Translate naturally and review pronunciation with the intended language accent. Do not make a Spanish sentence by substituting words into an English recording.
- If a Spanish book is incomplete, label the available English reading option clearly; do not present silence as a finished Spanish mode. English remains the first completion target.

**Implementation proposal:** author `BookDefinition` and `BookPageDefinition` assets with a page ID, illustration layers, localized text/audio keys, hotspot rectangles, animation IDs, and optional sentence cue times. Runtime `BookReaderController` owns current page and Play intent; a local `NarrationController` arbitrates story/name audio. Each page change increments a request generation so an old asynchronous load or audio completion cannot restart a previous page.

Unity's localization asset tables can map a shared key to locale-specific assets, including audio. Its `AudioSource.timeSamples` exposes playback position in PCM samples; this can drive reviewed timing cues when using a suitable clip/playback setup. Word highlighting should be authored or aligned against the actual final recording for each language, rather than estimated from text length. Sentence highlighting is an easier first version. [Unity localization asset tables](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/AssetTables.html), [AudioSource.timeSamples](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSource-timeSamples.html)

Load the current page and, if useful, its neighbor. Release earlier page textures and clips when no longer needed. Reuse a few illustrated body parts and simple sprite animations; a physics simulation or Blender rig is unnecessary for a blinking or tail-swaying book illustration.

## 27. TV corner and local video library

**The TV idea is feasible.** Tap the TV or its remote to open a simple video library: big picture cards, a short title, and duration. Tap a card to open its full-screen player, with Play/Pause, skip backward/forward, previous/next clip, start over, a progress bar and a large back button. A partially watched card offers Continue and Start over; the default preserves the saved place. The in-room TV can show a still thumbnail while idle; it does not need to decode a second copy of the video.

Build the familiar thumbnail-and-player layout ourselves. For the first version, use ordinary local video files that you supply. This needs no YouTube account, search service, or video recommendation feed in the game. Returning from playback goes back to the library; finishing a clip stops on replay/back choices. An automatic-next option could be added later if wanted.

### Controls and remembering each child's place

**Playback clarification, September 23, 2026:** the intended behavior is to remember each child's position in each video, including after leaving the TV, changing rooms, or closing and reopening the game. This is an explicit planned feature, not something Unity saves automatically.

Use large picture controls for **Play/Pause**, **back/forward 10 seconds** (proposed step size), **previous/next video**, **Start over**, and **Back to videos / Return to game**. A simple seek bar allows choosing another part of the clip. Clamp skips to the clip's valid time range. Manual next/previous opens the chosen clip and respects its bookmark; reaching the end of a clip does not automatically start another.

| What the child does | Planned behavior |
| --- | --- |
| Opens a new video | Starts at the beginning when they press Play |
| Returns to a partly watched video | Shows Continue with a large play icon and a smaller Start over choice; Continue uses the saved place |
| Leaves the TV to play elsewhere | Saves the position before stopping/releasing playback; returning still offers Continue |
| Switches to another clip | Keeps a separate bookmark for the previous clip |
| Backgrounds or locks the device | Pauses and saves; returning does not unexpectedly start audio |
| Closes the game and launches it later | Loads the durable bookmark; the video is available to continue when the child chooses the TV again |
| Finishes the video | Marks it finished and offers Replay / Back; Replay starts at zero |
| Changes their playable character | Keeps the same child's bookmarks; bookmarks belong to the profile, not Bluey/Bingo/Bandit |
| The sibling watches that same video | Uses that sibling's separate position, without changing the first child's playback |

**Save design:** store a small bookmark under stable child-profile ID + clip ID + content version/hash, with position in seconds and completion state. Keep it in persistent app storage, separate from communal room state and toy cleanup. Normal app updates should preserve it. Deleting the app or explicitly resetting its data is a different operation. The initial guarantee is return on the same device/profile; automatic bookmark transfer between devices would be a separate sync feature, not implied by joining the shared world.

Checkpoint on pause, completed seek/skip, clip change, leaving the TV, and lifecycle suspension/focus-loss events. Also checkpoint during playback approximately **every five seconds**, an initial tuning proposal. Write a recoverable record with a previous valid copy; serialize writes so an older asynchronous save cannot replace a newer bookmark. Do not depend on a quit callback: mobile termination can occur without it. After a sudden crash or force-close, restore the last successfully written checkpoint, so a short portion may repeat rather than restarting the whole video. [Unity mobile quit limitations](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnApplicationQuit.html), [Pause/resume callback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html)

**Unity implementation:** prepare the file, verify seeking support, restore the saved time and wait for the seek/first-frame state before offering playback. Save the displayed playback time, and capture it before calling Stop. Setting VideoPlayer.time starts an asynchronous seek; do not enqueue a seek on every slider movement. Coalesce slider requests and ignore obsolete callbacks after changing videos. Decode/seek accuracy and speed must be checked with our actual clips on both iPads. [VideoPlayer API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer.html), [Playback time and seek behavior](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer-time.html), [Seek completion](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer-seekCompleted.html)

Replacing a clip with different content must not apply the old video's timestamp to it. Renaming the same library entry preserves its identity/bookmark. A missing or unreadable file keeps the bookmark and offers retry/back rather than silently erasing progress. A non-seekable or incompatible import should be identified during parent-side validation before it is presented as a normal resumable clip.

**Required proof:** watch, leave/reopen TV, close/relaunch, switch clips, start over, finish, test offline and preserve per-profile bookmarks through an update. One client watching must not affect the others. The foundation clip has scoped device proof; the full TV-library UI/importer remains planned.

### Files we can actually play

YouTube's in-app offline downloads are encrypted and are playable through YouTube; they are not regular video files that Unity can import. Our local library needs a normal file, such as an MP4. A YouTube watch-page URL is also not a direct MP4 source for Unity's player. If a browser-based online player is ever wanted, YouTube has a separate iframe API; that would be another integration with internet dependence. The plan here is a local clip library. [YouTube offline-download explanation](https://support.google.com/youtube/answer/7381437?hl=en), [YouTube iframe player API](https://developers.google.com/youtube/iframe_api_reference)

**Proposed first encoding target:** MP4 container, H.264 video, AAC-LC audio, 1280×720 at up to 30 fps, ordinary SDR color, and a modest bitrate. Start around 2–3 Mbps combined audio/video and assess quality on the iPads; these are proposed settings, not measured results. Apple lists H.264/AAC support for the seventh-generation iPad, while Unity stresses checking the actual target platform's decoder support. Windows playback alone cannot qualify a clip for either iPad. [iPad 7 media specifications](https://support.apple.com/en-us/111911), [Unity target-platform video compatibility](https://docs.unity3d.com/6000.3/Documentation/Manual/video-sources-compatibility-target-platforms.html)

Preserve the clip's aspect ratio with letterboxing on the iPad screen rather than cropping faces or stretching the picture. Do not start with 4K, HDR, or several concurrent decoders. At a combined 3 Mbps, ten minutes is approximately **225 MB**, calculated as bitrate × time ÷ eight, excluding small container overhead. Storage use and decoded runtime memory are different budgets; do not read entire movies into a managed byte array.

### Add clips without rebuilding the game each time

| Route | Use | Implementation work |
| --- | --- | --- |
| Bundled sample clip | Prove playback first with one small supplied clip | Ship in `StreamingAssets`; locate through Unity's platform-aware path API |
| Import from Files in parent settings | Preferred routine workflow after the player works | Native iOS document picker; copy selected file into the game's persistent media folder; validate before adding its library card |
| Update a curated media pack | Possible later convenience for several clips | Versioned manifest and explicit import; keep this separate from gameplay networking |

Apple's document picker allows choosing files from outside an app's sandbox; import/copy is simpler than keeping a permanent external-file reference. A small native iOS bridge or a separately evaluated Unity plugin will be needed. The archived Apple guide explains the model; use the current API when implementing rather than copying its old setup steps verbatim. [Apple document picker overview](https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/DocumentPickerProgrammingGuide/Introduction/Introduction.html), [current UIDocumentPickerViewController API](https://developer.apple.com/documentation/uikit/uidocumentpickerviewcontroller)

The importer should copy into a temporary location, check available space, probe the video, and only then commit the file and manifest entry. A cancelled picker, inaccessible provider, unsupported clip, duplicate file, or interrupted copy should leave the existing library intact. Files supplied through a cloud provider must be fully copied onto the iPad before being advertised as available offline.

Record a stable clip ID, relative filename, title, duration, thumbnail, content hash, and optional language/subtitle metadata. Use a hash to recognize duplicate content even if filenames differ. Keep imported media separate from world saves. Preserve both during normal in-place app updates and the agreed signing renewal; removal/reinstallation is not a data-preserving update strategy.

Unity documents `StreamingAssets` as read-only at runtime and recommends `Application.persistentDataPath` for files that must be written. Bundled and imported sources can therefore share one library interface, with different storage locations. [Unity StreamingAssets](https://docs.unity3d.com/6000.3/Documentation/Manual/StreamingAssets.html)

### Playback, failure recovery, and two iPads

Use **one `VideoPlayer` per device**, a render texture, and a Unity UI image with aspect-ratio fitting. Give the controller explicit states: Closed → Preparing → Playing/Paused → Finished or Error. Selecting a new clip cancels the previous request. Keep a poster/loading view until a usable first frame is ready; use a bounded preparation timeout with retry/back controls.

`VideoPlayer.Prepare()` allocates playback resources and preloads content, reporting readiness through `prepareCompleted`. Calling Play without preparation can delay the start. Stop releases prepared resources; Pause retains preparation. These APIs support the proposed controller but do not by themselves handle every failure or guarantee an already-presented first frame. Also handle playback errors and discard callbacks belonging to a closed/replaced player session. [Unity VideoPlayer.Prepare](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer.Prepare.html), [Unity VideoPlayer API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer.html)

Local TV decoding runs on the viewing device; the PC/VPS continues the world for other clients. Offline solo also plays installed clips. Pausing a video never pauses another player.

Backgrounding pauses local playback and saves its bookmark; return shows a paused player. The PC/VPS keeps running for other clients. Optional future watch-together would need explicit invitations, matching media IDs and playback messages; it is outside the first TV version.

## 28. Dinosaur toy room and discovery mini-game

### A collection of 20 different toys

Use a low toy shelf, wide floor mat, toy wash basin, blocks, pretend plants, rocks, and a sand tray. All dinosaur types should be reachable without completing quests. Show a manageable tray of about six at a time, with large picture category tabs and a “show all” collection view. This limits visual clutter without locking away favorites.

The following museum pages were checked and their names, English pronunciation guides, and basic catalog fields saved in the [dinosaur reference catalog](bluey-research/dinosaur-reference-catalog.json). The toy behaviors below are **our proposed playful animations**, not assertions about how the real animals behaved. Target 20 types for the collection; make the first six fully functional before expanding.

| Dinosaur toy and factual source | Proposed recognizable toy action | Batch |
| --- | --- | --- |
| [Tyrannosaurus](https://www.nhm.ac.uk/discover/dino-directory/tyrannosaurus.html), labeled T. rex for the child | Take two little stomping steps; optional soft pretend roar | First six |
| [Triceratops](https://www.nhm.ac.uk/discover/dino-directory/triceratops.html) | Nod; highlight its three horns while counting | First six |
| [Stegosaurus](https://www.nhm.ac.uk/discover/dino-directory/stegosaurus.html) | Rock gently; highlight the back plates | First six |
| [Brachiosaurus](https://www.nhm.ac.uk/discover/dino-directory/brachiosaurus.html) | Lift its long neck toward a pretend leaf | First six |
| [Diplodocus](https://www.nhm.ac.uk/discover/dino-directory/diplodocus.html) | Slowly sway its long tail | First six |
| [Ankylosaurus](https://www.nhm.ac.uk/discover/dino-directory/ankylosaurus.html) | Turn to show its tail club; shuffle into a toy shelter | First six |
| [Parasaurolophus](https://www.nhm.ac.uk/discover/dino-directory/parasaurolophus.html) | Tilt its head and follow a tapped toy path | Expansion |
| [Iguanodon](https://www.nhm.ac.uk/discover/dino-directory/iguanodon.html) | Take a step and nudge a lightweight play block | Expansion |
| [Spinosaurus](https://www.nhm.ac.uk/discover/dino-directory/spinosaurus.html) | Sway beside the pretend pond and leave toy footprints | Expansion |
| [Allosaurus](https://www.nhm.ac.uk/discover/dino-directory/allosaurus.html) | Follow a short drawn trail | Expansion |
| [Velociraptor](https://www.nhm.ac.uk/discover/dino-directory/velociraptor.html) | Peek out from behind a play rock | Expansion |
| [Deinonychus](https://www.nhm.ac.uk/discover/dino-directory/deinonychus.html) | Step onto a low block platform | Expansion |
| [Carnotaurus](https://www.nhm.ac.uk/discover/dino-directory/carnotaurus.html) | Turn toward a tapped picture marker | Expansion |
| [Styracosaurus](https://www.nhm.ac.uk/discover/dino-directory/styracosaurus.html) | Tilt its head for a close look | Expansion |
| [Pachycephalosaurus](https://www.nhm.ac.uk/discover/dino-directory/pachycephalosaurus.html) | Nod and roll a foam play ball | Expansion |
| [Coelophysis](https://www.nhm.ac.uk/discover/dino-directory/coelophysis.html) | Make a short, gentle tiptoe animation | Expansion |
| [Plateosaurus](https://www.nhm.ac.uk/discover/dino-directory/plateosaurus.html) | Reach toward a pretend feeding tray | Expansion |
| [Maiasaura](https://www.nhm.ac.uk/discover/dino-directory/maiasaura.html) | Settle beside a smaller toy in a cushion nest | Expansion |
| [Oviraptor](https://www.nhm.ac.uk/discover/dino-directory/oviraptor.html) | Bob its head and inspect a toy basket | Expansion |
| [Therizinosaurus](https://www.nhm.ac.uk/discover/dino-directory/therizinosaurus.html) | Slowly raise its illustrated arms, then settle | Expansion |

For any factual “learn more” mode, separately review each animal's anatomy, diet, period, and scale before production. Use the museum references to guide silhouettes and feather choices rather than copying movie depictions. Color schemes and toy sounds can be imaginative; do not narrate uncertain details as established facts. If flying or marine prehistoric reptiles are added later, give them an **Other prehistoric animals** category: pterosaurs and marine reptiles are not dinosaurs. [Natural History Museum: what are dinosaurs?](https://www.nhm.ac.uk/discover/what-are-dinosaurs.html)

### Give each toy connected uses

Every toy uses the existing draggable-object system: pick up, carry, place, rotate/flip through a simple control, put in a basket, and set on a supported surface. Add a reusable dinosaur component for name audio, a few animation clips, and a dirt level. A speaker button repeats its name; optionally announce it once when picked up, with a cooldown so sorting toys does not become constant overlapping speech.

| Child action | Object response | Reuse elsewhere |
| --- | --- | --- |
| Drag dinosaur through the sand tray | A bounded trail of stylized prints; toy becomes a little dusty | Existing dirt/cleaning system; prints are play effects, not a scientific track-identification tool |
| Brush or sponge a dusty toy | Dust fades gradually and a few small particles fall | Brush, sponge, wash basin, cleanup activities |
| Pour a bucket over the toy in the basin | Toy becomes clean; basin fill level changes according to existing container rules | The same water/pour behavior used for plants and dishes |
| Place toys, rocks, and plants on the mat | Arrange a little pretend world; objects keep their positions | Existing surfaces, snapping, saved arrangements |
| Put a toy in a basket or cushion nest | It sits or rests; a small sleep animation can play | Containers, bedtime story, pretend household play |
| Place toy near a matching book | Offer an optional picture button to its book page | Shared dinosaur ID; opening the book never consumes or removes the toy |

Food is pretend play in the first toy version. If later teaching diets, use reviewed animal-specific food rules rather than presenting every dinosaur as eating the same thing. A toy may still join an imaginative picnic without the narrator calling that a scientific fact.

### Dinosaur Discovery Mat: the required mini-game

**Main loop:** uncover a hidden toy → brush it → hear its name → wash it if desired → place it in a little world you build. The toy remains usable after the activity. The discovery mat is also a sandbox; the child can skip digging and take any dinosaur from the shelf.

In Simple Play, one broad swipe or tap reveals a toy; brushes snap generously to it; naming is automatic after the reveal but replayable. There is no timer, failure sound, required reading, or hunt through a long inventory. Explore & Stories can use several brushing strokes, more scenery pieces, and optional requests from a parent character. Assistance changes the required input, not which toys are available.

| Optional invitation | Simple Play | Explore & Stories / up to four players |
| --- | --- | --- |
| **Find a dinosaur** | Tap a mound; brush once; hear the name | One child digs, the other brushes or carries the toy to the mat |
| **Dinosaur wash day** | Drag a ready sponge across a muddy toy | Fill the basin, wash, rinse, and arrange several clean toys together |
| **Build a dinosaur world** | Place a rock and a plant with generous snapping | Make shelters and paths, add toys, and invent a story; no mandatory layout |
| **Who's hiding?** | Tap an obvious toy silhouette behind a bush | One child hides a toy among designated mat props; the other looks; this reuses discovery rules without replacing avatar hide-and-seek |
| **Dinosaur parade and bedtime** | Tap a toy to make it take a few steps into a basket | Arrange an order, lead the toys along a short path, then tuck them into cushion nests |

These activities add to the show's game catalog; they are original dinosaur play concepts, not claims that all occur in specific Bluey episodes. Use toy replicas in the digging tray. A future fossil assembly activity would need separately sourced anatomical reference art; do not accidentally teach an invented skeleton as a real one.

**Co-op rules:** each toy has a stable instance ID. The host grants one drag owner at a time, with a clear visual cue when a sibling is holding it. Another child can work on a different toy or the scenery immediately. Offer a deliberate “another toy” action for duplicates so both can choose T. rex; retries must not silently spawn extras. Dirt, location, container membership, and discovered-name state synchronize; individual narration and camera close-ups stay local. Leaving the mat preserves the arrangement and releases tools/reservations.

Keep finished toy art modular by body family, but preserve recognizable silhouettes. Animate only visible toys; pool footprints, bubbles, and dust with a strict cap. A target of 20 available types does not mean twenty complex rigs, constantly running effects, or unlimited spawned instances on the A10 iPad.

## 29. Building and testing books, TV, and dinosaurs

### Shared parts and saved state

| Component | Definition/content | Runtime or save responsibilities |
| --- | --- | --- |
| Book catalog | Book ID, cover, ordered page IDs, supported completed languages | Per-child bookmark; recent titles; settings for automatic page turns |
| Book page | Layered illustration, caption, narration, cue times, hotspots | Current local page and audio request generation; no shared page cursor |
| Foreground audio controller | Reviewed narration and name keys | Prevent competing speech; honor Pause/close across delayed callbacks |
| Media library | Clip ID, relative path, hash, poster, language, duration | Validated installed files; local progress; import transaction recovery |
| Dinosaur definition | Museum reference, localized name audio, toy art and animation | Shared reference for book, shelf toy, and discovery invitation |
| Toy instance | Stable instance ID and dinosaur definition ID | Position, dirt, container, arrangement; temporary drag owner |
| Discovery activity | Picture invitation and optional steps | Participants, event progress, tool reservations; free exit and resume |

This extends the existing architecture instead of making each book, dinosaur, or mini-game a separate application. No new backend is required. Audio/video libraries, room simulation, and networking still need memory and lifecycle coordination on each device.

### Build order for these additions

1. **Keep the existing foundation gate.** Shared touch interactions, player IDs, save recovery, and LAN behavior must work before these features can be called integrated.
2. **Books and first six toys:** build the expanded dinosaur book with reviewed name recordings, Play/Pause/page controls, and toy pickup/washing. Verify all four players reading independently while others may continue playing. Do not produce all six books before this works.
3. **One TV clip:** use a small supplied MP4 to prove preparation, playback, audio focus, aspect ratio, exit, backgrounding, and client playback responsiveness and uninterrupted sibling play.
4. **Discovery mat:** finish digging, brushing, toy placement, and the five optional invitations using the established object rules.
5. **Expand content:** complete the 20-toy catalog and approved book titles, reviewing pronunciations and art. Add the Files importer after the basic video player has passed its device checks.
6. **Family release checks:** profile the combined systems, verify offline availability, complete English, then expand Spanish, and test in-place update/signing renewal with books, imported clips, and toy saves present.

The older iPad remains the performance baseline as client and solo player, including book/video loading. Load only needed pages and release old textures. The PC/VPS keeps other players running. Final budgets require device measurements.

### Acceptance checklist

| Scenario | Required outcome |
| --- | --- |
| Younger child picks a book with no adult explaining text | Recognizes Play, changes a page, taps a dinosaur, and returns to the room |
| Open book and wait | No story narration until Play; saved page is visible and usable |
| Tap a name repeatedly during narration, then Pause | One intelligible voice; no long queue and no delayed restart after Pause |
| Turn pages or close during an audio/art load | Only the current view can become active; no stale picture or speech appears |
| All four players open the same book | Independent pages, bookmarks, language and controls; moving the prop does not remove another player's access |
| Book, TV or menu opens | Foreground media is local; PC/VPS world continues for siblings; offline solo stays usable. Do not set a global shared-world pause. |
| Switch avatar after reading or carrying a dinosaur | Bookmark, player identity, toy instance, and held state remain correct |
| Both children grab or wash the same toy | One valid ownership/state transition; no duplicated item or conflicting dirt value |
| Leave the mat midway through digging or washing | Tools release; valid toy/world state remains; returning is straightforward |
| Choose every dinosaur type | All 20 are accessible without quests; every name recording is reviewed and replayable |
| Watch actual supplied clips on each physical iPad | Correct picture, sound, orientation, aspect ratio, and usable seeking; Windows success alone is insufficient |
| Cancel import, run low on storage, select unsupported media, or terminate during copy | Existing library survives; partial files do not appear as playable cards; recovery is available |
| Rapidly select clips, close during preparation, then reopen | One decoder/session; no audio behind the room or late callback reopening playback |
| Background, lock, interrupt audio, disconnect, or resume host/client | Playback pauses; LAN recovery follows existing rules; no auto-speaking after explicit Pause |
| Use books and clips without internet while home Wi-Fi stays available | Installed content still works; LAN play continues; no hidden cloud speech dependency |
| Repeat book/TV/toy switches and play a long session on A10 | No accumulating textures, audio, decoders, particles, or unbounded toy instances; measured performance remains acceptable |
| Install an in-place update and perform signing renewal | Saved book pages, toy arrangements, and imported media remain usable; verify on both iPads |

**Evidence and status:** the museum reference catalog records 20 successfully retrieved dinosaur pages and pronunciation fields; the [books, media, and dinosaurs evidence index](bluey-research/books-tv-dinosaurs-evidence.json) links the primary research and technical sources and separates sourced capabilities from proposed designs. No book artwork, narration, video imports, dinosaur assets, Unity implementation, or new iPad build was produced by this research update. Each tracker row remains planned until its content and device checks are complete.

## 30. A simple and playful science corner

**Native liquid colors — September 28:** LAB-01 / SP-15 now plays at the downstairs Science bench with red/yellow/blue bottle taps, visible blending, portion ratios, water dilution and independent reset/undo. Schema 21/content 22 adds four saved profile-owned mixtures. [Implementation and evidence](implementation/liquid-colors-2026-09-28.html). Candidate 194 passes 233 core groups, seven native four-client groups, private-solo reopen and seven isolated recovery groups. Windows and signed Android source/artifacts match. The existing 16 KB RELRO check and physical qualification remain open. No family device or production server update. Native reader picture controls follow; the full Home backlog and branch integration hold remain.

**Native bubble lab — September 28:** SCI-06 / LAB-01 / SP-12 now plays at the downstairs Science bench with Water → Soap → Stir → Dip → Blow, big/little bubbles, round/square wands, direct popping and independent reset/undo. Schema 20/content 21 adds four persistent profile-owned trays. [Implementation and evidence](implementation/bubble-lab-2026-09-28.html). Candidate 192 passes 226 core groups, seven native four-client groups, private-solo reopen and seven isolated recovery groups. Windows and signed Android source/artifacts match. The existing Android 16 KB RELRO check still fails; physical qualification remains open. No family device or production server update. Liquid colors, native reader controls, fan/wind extras and physical qualification remain open. Preserve the branch integration hold.

**Native dinosaur rescue — September 28:** LAB-01 / SP-02 now plays in the Home science area: little hammer, localized cracked ice, gradual warm/cool melting, three illustrated dinosaurs, toy dragging and independent reset/undo. Schema 19/content 20 adds four saved rescue trays without changing earlier Home records. [Implementation and qualification](implementation/ice-rescue-2026-09-28.html). Candidate 191 passes 218 core groups, seven native four-client groups, private solo reopen and seven isolated recovery groups. The signed Android release and matching source are verified; a concurrent-build run logged queue warnings, and the existing 16 KB RELRO gate remains open. Phone, iPads and production server remain unchanged. Bubble/color labs, native reader controls, all original science stations and the wider Home backlog remain required. Keep the existing branch integration hold.

**Hammer, bubbles and liquid colors — September 28:** the user liked the simpler prototype and requested these additions. [Applied research and evidence](implementation/science-hammer-labs-2026-09-28.html): SP-02 now offers a little hammer and retained water melting, SP-12 expands into a bubble-making lab, and SP-15 adds colored-liquid mixing, ratio changes and dilution. Fifteen browser activities retain independent play for four participants with tested additive save migration. These physical color mixtures use different rules from SCI-04 colored light. Production Home integration remains open.

**Child-flow follow-up, September 28:** the [applied research and simpler prototype flows](implementation/science-kids-flow-2026-09-28.html) replace dropdown/text-heavy entry with experiment pictures, direct taps, at most three main picture tools and on-demand draft spoken hints. Four players retain independent play and restart undo. Browser evidence does not complete the Home stations or physical-device qualification.

**September 27 presentation follow-up:** the user asks for actual games, pictures and fun project ideas to inform this area. [Publisher references and eight applied play designs](implementation/home-science-play-design-2026-09-27.html) now supplement the scientific rules below. Candidate 174 is a functioning schematic first pass, not accepted final art/play. Recommended next science slice: a directly manipulable illustrated cargo harbor, followed by the magnet/light presentation. No additional station is implemented by this research.

**September 27 user request:** science and a coloring-book area belong in the **main house's shared downstairs space**, accessible to all four players. [Deep research applied to placement, interactions and persistence](implementation/home-science-coloring-research-2026-09-27.html) and the [interactive research prototype](implementation/home-science-coloring-prototype.html) supplement this chapter. The prototype is browser-only; its evidence is separate from the later [candidate-174 Unity implementation](implementation/home-discovery-2026-09-27.html), which provides three simple saved science trays and six tap-fill pages. Five stations, freehand drawing, deeper creation integration and physical qualification remain open.

**Build a little discovery bench inside Heeler Home, with toys that react immediately to a tap or drag.** The child can investigate freely; a parent character may offer a short invitation such as “What will float?” No reading test, mandatory experiment sequence, or result screen should interrupt play. Keep the existing Simple Play and Explore & Stories settings per child.

PBS's age-three science guidance emphasizes investigating pushes, pulls, rolling, floating, and movement through play and simple descriptive language. That supports a cause-and-effect approach; it does not establish that these proposed digital activities have a measured educational benefit. Our game designs below adapt physical science ideas into forgiving, illustrated play. [PBS: pushes and pulls at age three](https://www.pbs.org/parents/learn-grow/age-3/science/pushes-and-pulls)

**Latest addition — mixing reactions:** the user explicitly requests mixing materials such as vinegar and baking soda and seeing something happen. SCI-09 below extends the original eight-station scope to nine. [Researched mixing process and variations](implementation/home-science-play-design-2026-09-27.html#sci-09-mix-and-discover). [Candidate 178 implementation](implementation/home-mixing-2026-09-27.html) now includes fizz/foam, indicator colors, oil/water and oobleck with four independent saved workspaces. Physical/device acceptance remains open; the wider nine-station catalog is not finished.

### Nine science activities

**New prototype collection, September 27:** the user accepted fourteen playful activity concepts and requested deeper research and prototypes before detailing them. [SP-01–14 research, scientific rules, implementation limits and evidence](implementation/science-playground-research-2026-09-27.html) · [Playground](implementation/home-science-playground.html). Lava jars, dinosaur ice rescue, marble playground, stretchy slime, magic milk, drawing robot, balloon rocket, foam fountain, wind tubes, circuits, weather, bubbles, rainbow mirrors and family chain reactions each have a retained workspace for four local players. Some extend SCI-03/04/06/09; others add concepts under LAB-01. Browser prototypes are not completed Home stations; the original nine below and their remaining requirements are retained.

| ID and station | Simple Play | Extra exploration and cooperative use | Scientific idea to preserve |
| --- | --- | --- | --- |
| **SCI-01 — Floaty boat tub** | Drop a wooden block, smooth stone, or toy boat into a shallow illustrated tub | Add cargo to the boat; one child loads while the other changes the cargo; remove cargo and try again | Floating depends on the object and displaced water, not simply “big/heavy sinks.” Use a finite set of reviewed props |
| **SCI-02 — Magnet treasure trail** | Drag a large magnet wand near iron/steel toy pieces and watch them follow | Guide a magnetic toy along a path; try wood/plastic pieces; turn a second bar magnet to compare push and pull | Ordinary magnets do not visibly attract every metal or every object. Magnetic poles need consistent behavior |
| **SCI-03 — Ramp and roll** | Tap to release a ball down a ready-built ramp into a wide basket | Change ramp height and surface; one child adjusts, the other releases; keep both toys otherwise identical when comparing | Slope and friction change motion; do not use “heavier always rolls faster” as the rule |
| **SCI-04 — Color-light garden** | Switch on red, green, and blue lamps to light up a pretend flower screen | Move overlapping light spots or turn lamps off together; make picture-requested colors if wanted | This is colored light: red plus green makes yellow; all three can make white. Paint mixing uses different rules |
| **SCI-05 — Dinosaur shadow theatre** | Move one toy between a lamp and screen; its shadow follows | Move the toy closer to the light/screen, switch toys, and make a tiny story together | An opaque object blocks light; preserve consistent source/object/screen geometry |
| **SCI-06 — Bubble workshop** | Add water and soap, stir, dip a wand, blow bubbles and tap to pop | Four independent mixtures; explore wand outlines and air strength. Optional fan and shared catching play remain required | Free soap bubbles tend toward a round shape; a square wand does not make permanent square floating bubbles |
| **SCI-07 — Sound-and-wiggle board** | Pluck a large illustrated string and see it vibrate | Adjust a clearly marked length or tension control; take turns making high/low sounds | Vibration produces sound; change one relevant parameter at a time and separate pitch from loudness |
| **SCI-08 — Little seed window** | Place a seed, add water, and press a fast-forward sun picture | Compare two prepared pots, watch simplified growth stages, decorate the pots, and move a grown plant to a bedroom | Growth is deliberately sped up; plants need more than water alone, and soil is not the only possible growing medium |
| **SCI-09 — Mix and discover** | Scoop baking soda and pour vinegar into an open bowl or toy volcano; watch immediate fizzing | Four independent saved trays; compare portions, add optional soap for lasting foam; researched extensions include indicator colors, oil/water layers and cornstarch/water | Vinegar and baking soda produce carbon dioxide; soap retains bubbles rather than creating extra gas. Preserve consumed ingredients and distinguish chemical reactions from physical mixing; not every combination fizzes |

**Factual source notes:** the [PBS sink-or-float activity](https://www.pbs.org/video/sink-or-float-prek-kindergarten-science-m2z0ne/) and [ramp activity](https://www.pbs.org/video/ramps-npt3-nanhcu/) support observing and comparing those effects. The National MagLab describes ferromagnetic materials in its [permanent-magnet explanation](https://nationalmaglab.org/about-the-maglab/around-the-lab/maglab-dictionary/permanent-magnet/); our limited prop list should be checked against those material distinctions before recording any factual lines.

The Exploratorium explains [additive colored lights and shadows](https://www.exploratorium.edu/snacks/colored-shadows), [why free bubbles tend toward spheres](https://annex.exploratorium.edu/ronh/bubbles/shape_of_bubbles.html), and [vibration, length, tension, and pitch](https://www.exploratorium.edu/snacks/sound-sandwich). NASA's [plant-growing explanation](https://science.nasa.gov/eclips/videos/do-plants-need-soil/) supplies the reference for the seed station. These are sources for the underlying ideas, not claims that their physical activities or lesson levels were designed for this game's three-year-old audience.

### Make the experiments feel like toys

Give each station three immediate actions before adding an optional quest. For example, the float tub supports **drop → bob/sink → scoop out**, then **load → tip → unload**, then decorating the boat. The ramp supports **release → roll → collect**, plus height and surface choices. A child can repeat any pleasing result without waiting for dialogue to end.

The younger child's mode begins with a working setup and broad snap targets; the older child can assemble or adjust it. Spoken invitations are one short line at a time, with an obvious replay icon. Optional prediction choices use pictures, followed by an observation rather than a wrong-answer buzzer. Timers measure the simulation internally; they do not grade the child.

Up to four players can manipulate independent controls or use four separate tools/sample trays where needed. Never require two simultaneous fingers, a second player, blowing into the microphone, or camera recognition. A shared reset must not wipe a sibling's held object or result: provide personal sample trays or reset only an unused station. A finished plant, decorated boat, or sound sequence can become a saved play object.

### A lightweight simulation that stays consistent

Use a reusable `ScienceStationDefinition` with allowed props, control ranges, state variables, and response rules. A `ScienceSession` records the experiment state and participants. Example state: tub water level, object ID, boat cargo count/mass, floating phase; or ramp angle, material, release position, and ball state. Save meaningful outcomes and arrangements, not every particle.

For the first version, use reviewed response tables or simple bounded models: capped spring-like bobbing, ramp motion driven by gravity/friction settings, magnet attraction only for eligible props, and staged plant growth. These are educational approximations. Test their direction and consistency; a spectacular animation should not contradict the spoken explanation. Use a dedicated additive-light shader or reviewed color combinations, not ordinary paint-color averaging.

The PC or local solo authority calculates results. Clients display interpolation, splashes, bubbles, light, and sound; they do not independently decide whether the same boat sank. Unity physics on two devices should not be assumed to produce identical results from identical inputs. Bound the number of balls, bubbles, dust particles, and simultaneous sounds. An offline iPad runs these same rules locally.

**First science prototype:** float tub, magnet trail, and color lights. They test three distinct interactions while reusing the existing container, drag, and touch systems. Complete those before adding the other five stations.

### Shared downstairs coloring and drawing

**September 27 visual correction:** the user rejects schematic science and a six-page coloring limit. [The applied workshop correction](implementation/workshop-visuals-2026-09-27.html) adds illustrated equipment and twelve official Bluey pages, for eighteen total, with additive per-profile save migration. Final visual/physical acceptance and blank drawing remain open.

This newly requested area sits beside the living room and shared science bay. Four players can use their own papers at once, regardless of chosen avatar. It does not replace narrated books. The working default includes coloring pages and blank drawing paper; the optional preference question remains open. Final art/themes need review.

| Coloring ID | Feature | Required behavior |
| --- | --- | --- |
| COL-01 | Shared art table and four paper places | Reachable downstairs, clear walking space, independent entry/exit |
| COL-02 | Full-page coloring books | Large closed regions, fitted pictures, surrounding visible tools; all pages available to every player |
| COL-03 | Forgiving crayons and tap fill | Large named swatches, immediate feedback, authored region masks and a tap alternative to precise dragging |
| COL-04 | Blank drawing and undo/redo | Clipped strokes, one gesture per undo, no sibling-wide erase; default pending optional preference |
| COL-05 | Durable personal picture folders | Creation/profile/page-version identity, autosave feedback, safe reopen and bounded storage |
| COL-06 | Shared display and carrying creations | Display or move a real picture record; preserve identity and prevent duplicate paintings |
| COL-07 | Optional Together permission | Visitors view; owner controls editing; accepted work survives participant departure |
| COL-08 | Four-player and device qualification | Bounded artwork updates, private offline saves, authoritative reunion, recovery and A10/touch verification |

**Scientific coloring distinction:** RGB light mixing is not crayon or paint mixing. Keep the systems and explanations distinct. There is no correct-color score or required completion percentage. The [applied specification](implementation/home-science-coloring-research-2026-09-27.html) records the source-to-decision mapping, drawing-data/network constraints and acceptance sequence.

## 31. Joining and leaving without restarting play

**Clarified in section 50:** this applies across all locations, not only rooms in the house. A player at the creek can independently enter the playground where another is already playing; both then see the same players and objects. Departing changes only the departing player's participation and view.

**Confirmed home design: a small Windows game server keeps the family world alive; both iPads are players.** You confirmed that the PC can stay on while they play. Either child can start first, the other can join later, and either can close their app without making the remaining iPad become the server. No second player is required to begin.

Unity supports a Dedicated Server build target for Windows, macOS, and Linux, optimized to remove unnecessary presentation work. That makes a small Windows server an appropriate implementation candidate; actual CPU, memory, sleep behavior, and stability still need testing. Use a packaged build, not an open Unity Editor, for family play. [Unity Dedicated Server](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-introduction.html)

### Three different actions, all supported

| What the child does | Required result |
| --- | --- |
| Joins/leaves a mini-game while remaining in the world | Add/remove only their participation and temporary tools; preserve the activity and everybody else's objects |
| Moves to a bedroom, secret room, or another location | Change that child's room and camera only; the sibling keeps their current scene, controls, and activity |
| Opens/closes/backgrounds the iPad app during home play | Attach/detach their player connection; PC simulation continues; persist room changes and release abandoned grabs |

The first player sees a small arrival animation or portrait indicator, not a full-screen pause or a forced lobby. The second player loads on their own iPad. “Join me” is an optional picture invitation; finding a sibling should not require abandoning a half-made cake. Arrival near an activity gives a free nearby interaction position, never the exact coordinates or held object of another player.

### Joining an already-running world

1. Identify the family world, compatible content/schema version, and the child's permanent profile. A network connection number is not the child's identity.
2. Capture a consistent snapshot at revision R: active objects, rooms, science variables, dishes, toys, parent tasks, and current activity membership.
3. Transfer it to the arriving client while the first child keeps playing. Buffer or stream changes newer than R; apply them in order and ignore duplicate action IDs.
4. Wait for the joining client's required network objects and scene content to be ready, then enable their controls. Keep a loading overlay local to that iPad only.
5. Join an activity when the child chooses it. Supply its current state, not a replay of every past sound, celebration, or animation.

Unity's `NetworkVariable` state synchronizes to late joiners, unlike an isolated one-time RPC event. Its scene manager also provides client synchronization and completion events. Those facilities help, but our room saves, activity semantics, snapshot revisions, and content checks remain application code. Initialize visuals from current values as well as subscribing to later changes. [Unity NetworkVariables](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/basics/networkvariable.html), [Unity NetworkSceneManager](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/using-networkscenemanager.html)

| Activity in progress | How the sibling joins immediately | How either leaves |
| --- | --- | --- |
| Science, kitchen, cleanup, dinosaurs | Take a free tool or sample; see the existing experiment/dish/arrangement | Return an unfinished drag to a valid surface; work already accepted remains |
| Fishing | Take another available rod; existing catch is unaffected | Release an unfinished fish reservation; keep already completed discoveries |
| Parent hide-and-seek | Join as Preparing, choose a legal hiding place, then become a hider on a personal timer | Remove only that hider; parent continues seeking others |
| Child-seeker hide-and-seek | Join the available role with local preparation; no global restart | A parent replaces a departing child seeker; hiders may always step out |
| Bedroom or secret-room play | Enter the existing room ID; see its current objects | Leave the room; no visitor-owned duplicate room is destroyed |
| Books and TV | Open personal reading/playback controls, or sit nearby | Stop only that child's audio/video; the world remains active |

Store activity participants as a changing set, not a fixed “must have two players” array. Do not reset completion targets, delete a station, or despawn a dish just because its creator left. Shared creations are server-owned world objects; players receive temporary interaction reservations. Unity's ownership/despawn settings require explicit care for objects that must survive their creator disconnecting. [Unity object spawning and lifetime](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/basics/object-spawning.html)

### PC server lifecycle

As refined in section 44, propose automatic server startup with Windows sign-in after parent setup, plus a separate **Family Game Status** desktop shortcut with a small parent panel: running world, connected children, last save, and stop. The existing Connect Unity + Blender shortcut connects development tools; it should not be required for the children's game. During implementation, start the server hidden by default, bind it to the home network, and configure only the necessary local firewall access. No router port forwarding or public hosting is required.

Persist accepted changes with a versioned journal/checkpoint and backups. Stop or crash recovery loads the last durable state and clears temporary reservations. Keep server simulation independent of cameras, audio playback completion, or UI objects, because a dedicated build may strip presentation assets. When nobody is playing, pause activity clocks and avoid offscreen mess or unwanted growth; resume coherently when someone returns.

The PC still has to stay awake and reachable for the shared home session. A PC/network failure is a separate failure mode from a child leaving; local continuation and saved-state recovery cover it. Research does not make an outage incapable of causing a brief synchronization transition. Travel behavior is specified in section 35.

<a id="32-two-personal-bedrooms-with-shared-updates"></a>

## 32. Four personal bedrooms with shared updates

**Room-object pass, candidate 155:** four-place cuddling/tucking/pretend tea, bounded toy stacks, saved bedding/rug/picture/lamp choices and an optional local aurora fact reader now have an implementation record. [Research applied, migration and qualification](implementation/room-object-play-2026-09-26.md). Broader catalog, free furniture placement and physical acceptance remain open.

**Current implementation:** [BED-3 candidate 142](implementation/bedroom-furniture-2026-09-26.html) provides usable bedroom furniture, bounded personal toys/storage, owner decoration/Together/undo and tested save rules. The full chapter remains the goal: richer picture choices, catalogs, creation displays, reader integration and physical qualification are not complete.

**Upstairs layout and sequence, latest user direction:** all four bedrooms are on a new second floor. Make the existing living-room stairs usable in both directions, with a real landing/hall and four bedroom doors. Each player can travel independently while the others remain anywhere in Home. Finish this bedroom stage before the four secret rooms. See the [deep implementation research](implementation/upstairs-bedrooms-research-2026-09-26.html); its recommendations and acceptance cases are not implemented features.

**Latest user correction, September 26:** four separate bedrooms, one belonging to each of the four family player profiles, including parent players. Decorating a room updates that same room for connected visitors; it never makes all four bedrooms identical. Ownership follows the stable player profile, not the chosen avatar, device or connection order. See the [Home tracker](home-world-feature-tracker.md) for individual room and four-player acceptance checks. Earlier two-child-room limits are superseded.

Start each room with a bed, rug, low shelf, toy basket, reading cushion, wall-picture slots, and a decoration drawer. Offer picture choices for walls, bedding, rugs, lamps, and plush toys. All four players may visit any bedroom together or separately, including when its owner is elsewhere or disconnected. Every bedroom needs four-person occupancy, usable seats/play space and safe arrivals/exits. Their avatars need not remain beside each other to keep networking active.

### Ownership, visiting, and decorating

The room owner controls permanent style changes by default. Visitors can play with loose toys and sit/read; an optional **Decorate together** picture enables shared furniture arranging. This is a proposed way to protect a child's chosen layout while still allowing play, not a requirement for an adult approval each time they visit. Room owners can undo recent decoration operations. Do not let a visitor accidentally remove the only exit or the other child's bedroom.

Use forgiving placement: clear legal surfaces, large snap areas, and an immediate preview. Keep doors, entry anchors, and the required walk strip clear. Place oversized furniture only in fitting slots in Simple Play; Explore can allow more flexible positioning within validated bounds. A lamp's light, a drawer's contents, and a plush toy's pose are interactive state, not just a static wallpaper swap.

| Identity/state | Example | Rule |
| --- | --- | --- |
| Family world ID | One home world | Stable across app updates and server restarts |
| Player profile ID | Player A / B / C / D | Stable across device reconnects and character swaps; parent profiles also own rooms |
| Room ID | `bedroom-A`, `bedroom-B`, `bedroom-C`, `bedroom-D` (illustrative IDs) | Four persistent identities shared by the authority and clients; preserve existing IDs/data when adding missing rooms |
| Room owner | Player A owns bedroom-A | Ownership of decoration is separate from network/server authority |
| Prop instance ID | A particular blue plush dinosaur | Moving it changes one instance; it must not appear twice |
| Accepted room revision | Monotonically ordered change number | Determines what a reconnecting iPad is missing |
| Decoration transaction | Move shelf, change bedding, add picture | Validate, commit once, replicate, and make reversible where practical |

**Connected example:** A moves their dinosaur plush from the bed to the shelf. The server validates the move, stores it, and sends the accepted update. B sees the new position immediately if visiting; if elsewhere, B sees the latest room state when entering. A simultaneous wall-color change in B's room cannot overwrite A's room because they have different room/object IDs.

### Independent rooms in Unity

Treat a room as a logical world zone, not as a command to reload everybody's Unity scene. For the first house implementation, keep authoritative room data and necessary collision/navigation structures in a persistent world; load the small home zones together or retain lightweight zone representations. Each client enables only its own current room's camera, visuals, local effects, and audible neighborhood. Do not disable the server's simulation root merely because a room is off camera.

For later larger locations, load presentation content asynchronously and manage network object visibility deliberately. Unity's built-in network scene loading commonly synchronizes scenes across clients, so an ordinary global Single scene load is unsuitable for “one child enters their secret room while the other keeps cooking.” Client-specific content loading requires its own design and tests. [Unity scene-management behavior](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/using-networkscenemanager.html)

A room change is a small transaction: request target room/door → load local view and current state → reserve valid arrival anchor → commit the player's room/location → fade locally. If loading fails or the child cancels before commitment, keep them in the original room. After commitment, recover the authoritative result if acknowledgement is delayed; do not roll the client back to an obsolete room. Up to four players arriving together receive distinct safe nearby anchors; leaving never evicts another occupant. Preserve their held toy unless an explicit activity rule requires setting it down.

### Offline room changes and return-home sync

Each device keeps a usable cached home layout and its player profile's editable private travel save. Live changes cannot reach the other iPad without a connection. Rejoining loads the current server room while retaining offline work separately; do not label a disconnected edit “synced.”

Connected bedroom edits are validated and saved by the PC/VPS, then sent to the other clients. Disconnected bedroom work is saved privately. Rejoining always loads the server room; do not merge room journals or ask children to resolve shared-world conflicts.

Automatic saving should be frequent enough to preserve finished changes without rewriting the entire world on every drag frame. Record committed drops/style choices, create periodic compact snapshots, and validate migration when the game adds new furniture definitions. Test this on real interrupted writes and out-of-order reconnections, not only a normal save/load cycle.

## 33. Secret plush rooms with stars and northern lights

**Room-object pass, candidate 155:** four-place cuddling/tucking/pretend tea, bounded toy stacks, saved bedding/rug/picture/lamp choices and an optional local aurora fact reader now have an implementation record. [Research applied, migration and qualification](implementation/room-object-play-2026-09-26.md). Broader catalog, free furniture placement and physical acceptance remain open.

**Implementation checkpoint — September 26:** [Researched candidate 146](implementation/secret-rooms-2026-09-26.html) adds four persistent optional secrets with locally revealed far-back entrances, safe exits, four-place fort/cushions, six plush types/storage and local calm controls. Full reader, cuddle/tuck/stack/picnic extensions, hiding integration and physical qualification remain open. The specification below remains required.

**Latest entrance requirement, September 26:** the mini-door belongs at the **far back of each bedroom, away from the main entrance**. It is hidden until that player's character gets close, then appears with a glowing star, gentle sparkles and shimmer. Proximity reveals the entrance locally for each of the four players; entering still requires a deliberate tap. Use a calm static reveal when reduced motion is enabled, keep the approach clear of furniture, and keep the interior exit always visible. This replaces any entrance-near-the-hall interpretation; the secret rooms remain the next stage after bedroom furnishings.

**Updated four-player layout: each of the four bedrooms can have a little star-marked door leading to its owner's own cozy secret room. All four players may enter any existing secret room, together or separately.** These are playful hidden spaces, with no password, quest unlock, or exclusion rule. They are an addition to our game house, not a claim about a room shown in the television series.

Give each of the four player profiles a simple **Make a secret room** picture choice. This choice is optional to use, but the four-room capability is required. Choose a mini-door style and place it in a validated bedroom wall slot; create the room the first time and remember it. Moving the doorway later changes the entrance position, not the room's contents or identity. Removing the decorative doorway should archive the entrance, never erase the hidden room or strand its occupants. Keep an always-available return-home picture and a visible interior exit.

### What goes inside

- A soft-looking central rug, four usable cushion/beanbag places, a small blanket fort, and a low reading shelf; maintain clear entry/exit space for all four visitors.
- A generous plush collection: dinosaur plushies, dogs, cats, bunnies, bears, sea animals, and a big cuddle pillow. Reuse toy interaction rules for carrying, piling, sitting beside, and tucking them in.
- A deep-blue illustrated sky across the upper part of the room, with stars and slowly moving green/purple aurora ribbons.
- Gentle local controls for sky brightness, aurora movement, music, and effects. Tapping a star can create one brief twinkle; dragging a plush must not trigger constant sounds.
- Open-ended quiet play: build a plush pile, arrange a tea picnic, read a book, or sit together. No timer, cleanup obligation, surprise search event, or reward meter is required here.

Use a northern-lights-inspired fantasy ceiling/projection, not an attempt to reproduce real indoor atmospheric physics. If the child taps a small optional fact picture, a simple recorded line could explain that real auroras are lights high in the sky caused by energetic particles interacting with gases. NASA's child-facing explanation supplies the factual reference; the plush room's colors and movement are our art direction. [NASA: what is an aurora?](https://spaceplace.nasa.gov/aurora/en/)

### Room identity and performance

`secret-A`, `secret-B`, `secret-C` and `secret-D` are illustrative persistent room IDs, each linked to the correct profile-owned bedroom door. Preserve existing room identities and contents when introducing the additional rooms; never recreate all rooms on join. A visitor entering A's door must arrive in A's existing room, not a new room generated on their own device. All relevant connected clients receive current plush positions, furniture, and shared decoration settings. Every secret room accommodates four visitors with clear exits and sufficient places to sit/play. Brightness/reduced-motion preferences may remain local to each child.

Begin with a painted sky texture, a small number of soft aurora layers, and sparse pooled twinkles. Animate texture coordinates or simple 2D shapes rather than volumetric lights, expensive bloom, a live video sky, or thousands of transparent particles. Reuse one room-art set with independent saved layouts. Profile transparency overdraw on the A10; when reduced motion is selected, retain a still aurora so the room remains attractive.

Keep the camera steady and the exit visible. If any player leaves, the room and toys stay in place for all remaining visitors. When the last visitor leaves, save the room and stop unnecessary local animation. The PC retains its logical room state. The same room must also load from local data during offline road-trip play.

**Default relationship with hide-and-seek:** secret rooms are chill spaces outside the short parent-search arena. Entering one leaves the active hiding round without stopping the sibling's search. A later explicitly selected “search secret rooms too” mode may include their doors and parent routes, but it needs its own timing tests; don't silently turn a quiet room into an unpredictable hiding-game destination.

## 34. Hide-and-seek with enterable furniture and gentle clues

The official episode provides the family hide-and-seek premise; our parent roles, furniture mechanics, clue timing, and late joining are game designs. [Bluey: Hide and Seek](https://www.bluey.tv/watch/season-1/hide-and-seek/)

**Default: Bandit or Chilli seeks, up to four players may hide, and each child can enter a designated closet, oversized play drawer, tent, curtain alcove, or other clearly fitting spot.** Child-seeker mode remains a picture-selectable alternative. The parent's job is a short, friendly search, not winning against a preschooler.

### Settings that mean separate things

| Setting | Proposed default | Other choices |
| --- | --- | --- |
| Who seeks? | Parent | Child seeks; swap between rounds |
| Hiding clues | Gentle automatic clues | Manual giggle button only; automatic clues off |
| First automatic clue | Randomized within 5–10 seconds after search eligibility | About 5 seconds or about 10 seconds as fixed parent settings |
| Parent search pace | Gentle, aiming for each ready hider in roughly 30 seconds | Longer exploration mode after playtesting |
| Search area | Small bounded area with tested routes and a few furniture spots | More rooms later; explicitly choose a larger search rather than changing it mid-round |

These are tunable starting values, not timings established by a study. Test both children and retain their preferences. Turning off sound must not make the game unusable: an optional little door wiggle or localized visual cue can represent an enabled hint. Clue settings should change deliberate hints, not accidentally reveal hidden avatars or their nameplates.

### Hiding inside furniture

Author a `HideSpotDefinition` for each valid spot: stable ID, room, approach anchor, entry pose, interior pose, exit anchor, seeker inspection anchor, capacity, door/drawer animation, cover mask, and audio emitter. Use large illustrated furniture that visibly fits the avatar. Ordinary tiny drawers remain prop containers; an enterable play drawer gets an obvious hiding picture when the child is eligible to hide.

1. Tap the hiding-place picture or walk close and tap the large entry target. A reachable approach is enough; no precise alignment puzzle.
2. The authority reserves the spot and moves the character through a short entrance animation. A full spot offers another nearby spot; it never traps two bodies in the same one-person drawer.
3. The hider sees a cozy cutaway/outline of themselves and a large **Come out** button. The seeker sees the closed cover art and only deliberately enabled clues.
4. Come out is always usable. Restore the character at a valid exit position and reset input. If another toy blocks that position, use a nearby reserved recovery anchor.
5. A seeker must approach and inspect the correct furniture to reveal the hider. Finding is an authoritative event, followed by the opening animation and a warm reaction.

Do not implement hiding as setting the whole player/network object inactive. Store a hidden state and alter presentation for each viewer; preserve networking, child input, and the exit control. Mask carried items, nametags, outlines, footsteps, and unrelated character barks on the seeker's view. Avatar switching preserves hide-spot membership and adapts the interior pose; it does not cancel being hidden or create a clue.

### Giggles, rattles, and parent calling

Begin the automatic clue clock when the child is both hidden and eligible for the search. Do not play a clue while the parent is still giving that child their initial hiding opportunity. At roughly 5–10 seconds, emit a short cloth rustle, drawer rattle, or giggle from the hiding spot, then use a cooldown before another clue. Alternate a small set of recordings; avoid constant repeated laughter.

The authority sends a clue event containing hiding-session ID, hider ID, spot/room ID, sequence number, and server time. Clients play it once if it is current and audible in their room. A late join does not replay all earlier clues. Dropped audio must not change whether a find occurred. Unity exposes network time/ticks for coordinating timed state; our per-hider eligibility and clue schedule are still custom logic. [Unity network time and ticks](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/advanced-topics/networktime-ticks.html)

Give parents a few original, reviewed lines such as “Where could those puppies be?”, “Was that a little giggle?”, and “There you are!” Limit chatter to moments with meaning. Local narration arbitration keeps a parent call from competing with another foreground voice. The server uses recorded-duration metadata and gameplay timers, not an AudioSource completing on a headless PC.

### A short search that feels understandable

Use an authored search director plus a simple parent state machine: **count/prepare → look around → walk → inspect → react → find → continue**. Start with perhaps six legal spots in one compact area; measure the longest route through them, including inspections. A spot list whose route cannot fit within the target duration must be shortened or moved closer together.

| Time from a hider becoming ready for search | Proposed parent behavior |
| --- | --- |
| 0–5 seconds | Finish a short count/turn, look at a plausible empty spot, and begin moving; allow a little anticipation |
| Around 5–10 seconds | Call softly; an enabled hider clue can happen; parent visibly reacts to a heard clue |
| Around 10–20 seconds | Inspect nearby furniture; find an early hider if appropriate; keep the other child active |
| By roughly 20–30 seconds | Complete the remaining reachable search path or follow a clear clue, approach the spot, and reveal the ready hider |

A parent can prioritize a **deliberately emitted clue** as an observation. With automatic clues disabled, use a bounded route covering all legal spots rather than silently using the exact occupied coordinate. Randomize among routes that still meet the measured budget; endless random wandering cannot support the requested short search.

The 30-second goal starts when that child is ready for searching, excluding the time a three-year-old spends choosing a place. It is a pacing target for a valid reachable hider, not a guarantee while they repeatedly move between rooms or the network is unavailable. Do not punish movement or remove the exit button to force the timing. Keep elapsed search time when someone swaps hiding spots so repeated swaps do not accidentally restart every hint and parent decision.

No involuntary forced reveal from across the room. If navigation fails, replan to an alternate inspection anchor. If the spot becomes invalid, show an understandable come-out/nearby-spot recovery, never an invisible teleport-and-found event. Preserve clear causes: the parent heard something, walked over, checked, and found the child.

### Joining and leaving during the search

A late joiner becomes **Preparing**, not an instantly findable hider. Their own iPad offers legal hiding spots and a Ready picture while the parent keeps searching for the existing child. Once ready, they receive their own clue/search timers. The parent's shared state does not return to Count, and the first child's timers/progress do not reset. For example, a child ready at second 22 can be found around second 52; “about 30 seconds” is per ready hider, not a deadline imposed by somebody else's earlier start.

Use individual states: Not participating → Preparing → Hidden/Exposed → Found or Left. A round remains active while it has searchable or preparing participants; a long preparation may simply become ordinary free play with a persistent Join picture instead of reserving the parent forever. A found child may play, help, leave, or explicitly hide again as a new participation cycle; no elimination waiting screen.

If a child seeker leaves, the default parent takes over using the existing hider state. If a hider leaves, remove only them and cancel their scheduled clues. If the final participant leaves, release the parent. A reconnecting hider revalidates the spot and role; never replay an obsolete find or spawn them inside occupied furniture. At home, an iPad closing has these same leave semantics because the PC still runs the round.

**Meaningful tests:** simultaneous entry requests, come-out during parent inspection, an avatar swap inside a drawer, clue-off mode, audio muted, doorway obstruction, a parent called for a different activity, and a late join immediately before another hider is found. Log Ready, clue, inspect, Found, and Left timestamps to evaluate the actual search pacing on the iPads.

## 35. Road-trip play, hotspot co-op, and coming home

**Travel priority:** full offline solo is required. Internet/hotspot access to the designated PC/VPS is optional later. Device-hosted travel sessions, authority switching and offline imports are removed.

**Confirmed requirement: your phone hotspot may be available, but if the children cannot play together, each must still be able to do everything else.** [Section 49](#49-reaching-your-home-pc-from-a-road-trip) now also adds your requested optional internet connection to the always-on home PC. The PC remains optional for ordinary play; installed solo content needs no login or remote service. Full offline play is part of the core product, not a reduced demo.

### Shared play and private solo

| Mode | Where game rules run | Connection needed | If the other child closes their app |
| --- | --- | --- | --- |
| **Home family world** | Dedicated Windows server | Both iPads can reach the awake PC on home LAN; internet unnecessary for gameplay | Remaining child continues in the same shared world |
| **Remote family world** | Designated PC/VPS | Usable internet and configured authenticated endpoint | Other connected players continue; a disconnected player continues privately |
| **My play / road-trip solo** | Locally on each iPad | None for installed content | No effect; each has their own saved branch |
| **Travel solo without internet** | Each device locally | None for installed content | No effect; private saves stay separate |

Remote play is optional. Start local play without waiting for the designated server; the hotspot provides only a possible route to that server.

### What full offline mode includes

| Feature | Offline behavior |
| --- | --- |
| All finished locations and character choices | Bundled art and local scene data; no server unlock or download at the moment of entry |
| Movement, object interactions, cooking, cleanup, fishing, science, dinosaurs | Same rules run on the iPad; every activity has a one-child path |
| Parents and hide-and-seek | Authored AI and stored voice clips; parent seeks the child, or NPCs hide for a child seeker; no second human is required |
| Bedrooms and creations | Private offline edits remain in the local save; connected rooms come from the server |
| Books and narration | Finished books, dinosaur names, and supported language audio are installed locally |
| TV | Already imported/bundled MP4 clips play locally; importing a new cloud-hosted file may require internet |
| Saves and settings | Store locally during play, including without a successful network connection at startup |

The one thing offline solo cannot reproduce is the other human child's live actions. Provide a solo/NPC version wherever a game normally has complementary roles. Keep the same toy system and activity definitions so “offline” does not become a separately maintained, incomplete game.

### Using your iPhone hotspot

Personal Hotspot can provide the mobile devices an internet route. Remote play requires testing the actual route to the configured PC/VPS, not Bonjour between iPads. [Apple Personal Hotspot](https://support.apple.com/en-us/111785).

A parent configures the hotspot/network. The app can then try its enrolled remote server endpoint without blocking solo play. No peer-client routing or local iPad authority is required.

Apple lists Maximize Compatibility on iPhone 12 and later as one troubleshooting option. Our test should record whether the default or that option works best rather than relying on it as a guaranteed fix. No reset of saved network settings is part of our implementation plan. [Apple hotspot troubleshooting](https://support.apple.com/en-us/119837)

A working hotspot and working internet are different things. If the route to the designated server fails, continue independent solo. There is no local mobile-host fallback.

Router-free Apple peer-to-peer play is outside scope; retain independent solo when the designated server is unreachable.

### Losing the connection must not lose the game

Run the same rules locally for solo and on PC/VPS for shared play. Keep persistent state outside disposable network objects and UI; local controls do not wait for network teardown or a complete replica.

On loss of the server, continue privately from the latest usable visible world and local pose, clearing stale network holds. A complete server checkpoint, receipt history or durable write is not a prerequisite to responsive local controls. Older private saves remain separate; do not rewind the current scene to them.

Unconfirmed online commands are not automatically replayed on reconnection. Keep any diagnostic record separate from executable shared-world input.

A server outage leaves clients playing independently. Server restart/restore belongs to the PC/VPS workflow; no client becomes shared authority.

When a compatible designated server is reachable again, save the private continuation separately and join its current state at a safe input boundary. No automatic offline merge or conflict-import UI is required.

### Bringing road-trip progress home

**Save rules:** server rooms and communal item identities come only from the server while connected.

- Keep private offline rooms and creations local; they do not compete with canonical room revisions.
- Keep preferences, book pages and TV bookmarks local to the relevant profile/device.
- Do not duplicate communal props by uploading offline inventory or replaying old actions.
- Preserve private saves on rejoin and updates; selecting them later is an explicit local action.
- Connected cross-room item travel is one server transaction. Private inventories never enter the shared namespace.
- Repeated connected requests use operation IDs; offline commands are never resubmitted as if they were pending online work.

There is no travel branch to import into the server. Every client rejoining sees that server’s current room; the private continuation remains an explicitly separate local save.

### Content readiness and signing before travel

Add a parent **Ready for a trip** panel that checks installed room/art/audio packs, selected books, local clip availability, both children's latest saved rooms, free storage, and a successful cold offline launch on each iPad. “Downloaded” must mean files are present and readable, not just that a remote URL is listed. Missing optional Spanish audio must not make available English content unusable.

The previously agreed free signing-renewal plan also needs a travel rehearsal. Apple still gives Personal Team provisioning profiles a seven-day validity; an offline game design does not change that. Refresh before leaving, and for travel extending beyond the current validity, verify a renewal route that works away from the home PC. [Apple Personal Team limits](https://developer.apple.com/help/account/basics/about-your-developer-account)

SideStore is worth evaluating specifically for this requirement: its documentation describes on-device refresh after initial computer setup, and its current prerequisites require Wi-Fi plus its on-device networking helper. Test an in-place refresh on **each iPad through the actual internet-connected phone hotspot**, including launch and preserved saves. Do not assume being near an awake PC at home remotely renews an iPad in the car, or that signing works without internet. No tool is being installed or switched by this research update. [SideStore refresh FAQ](https://docs.sidestore.io/docs/faq), [SideStore current prerequisites](https://docs.sidestore.io/docs/installation/prerequisites)

Keep this as part of the existing free deployment workflow, not a requirement to buy a membership. Travel readiness includes both playable local content and a verified installation that remains valid for the trip.

## 36. Integration milestones and acceptance for this expansion

### Implementation batches

| Batch | Deliverable | Evidence needed to call it complete |
| --- | --- | --- |
| Local rules and PC family server | Same world logic in solo and dedicated builds; client-independent world lifetime | Start with either child, join late, close either iPad, and recover PC failure without corrupting saves |
| Independent rooms | Four player-owned bedrooms and four optional secret rooms, per-player room/camera, persistent objects | Decorate/visit concurrently; changing one room never reloads or overwrites the other |
| Science prototype | Float tub, magnet trail, color lights; then five more stations | Readable cause and effect, solo usability, late joining, tool-release recovery |
| Secret rooms | Four persistent profile-owned destinations, mini doors, plush play, sky/aurora | All four players can visit any room together or separately; exits always work; reduced-motion and A10 rendering pass |
| Revised hide-and-seek | Enterable furniture, NPC parent or human seeker, selectable clues, per-player timers and four-person participation | Inspect/find visibility is correct; mid-search joins work; valid stationary hiders meet measured pacing |
| Full travel mode | All installed content and solo/NPC activities work without PC/network | Cold-launch offline checklist passes on both iPads; no hidden cloud dependency |
| Optional remote connection | Use the designated PC/VPS over a usable internet route | Qualify after remote deployment; no device hosting |
| Rejoin and save preservation | Current server world plus retained private saves | No offline imports, duplicated communal items or lost private creations |

Establish reusable rules before producing dozens of rooms/science variants. Continue with the existing pinned Unity project; no restart, new template or package upgrade is implied.

### Required test scenarios

| Test | Expected behavior |
| --- | --- |
| Start a home game with only either iPad connected | Child plays immediately; no wait for a sibling |
| Join while a ball rolls, boat sinks, cake bakes, or parent searches | Arriving client sees current state; existing player gets no scene restart or forced pause |
| Leave/close/lock either iPad during each activity at home | PC retains creations, releases only departing holds/roles, and lets remaining child continue |
| All four players choose different rooms or locations | Independent cameras/content; nobody is dragged along or asked to stop |
| Two children move the same bedroom prop | One valid reservation/commit; understandable feedback; no teleporting tug of war |
| Room owner leaves while sibling is visiting | Room persists, visitor can play and exit, no disappearing floor or furniture |
| Move or hide a secret doorway while someone is inside | Destination persists; return-home/exit remains valid |
| Reconnect after decorating different bedrooms offline | All four profiles keep their offline work privately; shared play loads the authoritative server rooms without importing those edits |
| Reconnect after conflicting private edits to one object | Each profile's offline work stays in its private save; shared play loads the server object without merging or overwriting it |
| Reconnect with cached offline operations | Never replay/import offline operations; repeated shared commands cannot duplicate objects or creations |
| Enter/exit a drawer while changing avatar or holding a plush | Correct pose, visibility, carried state, and exit anchor on both iPads |
| Enable/disable clues; mute sound; join halfway through a search | Deliberate cues follow settings; no stale giggles or hidden-nameplate leakage |
| Hold still in every legal hiding spot | Measure Ready-to-Found timing near the proposed 30-second target, with successful navigation |
| Child seeker leaves while another remains hidden | Parent takes over; hider can still come out at any time |
| Cold-launch with Wi-Fi and cellular unavailable, PC off, cached app process killed | All installed solo-capable game systems open and save locally |
| Disconnect cellular internet while hotspot Wi-Fi remains connected | Record whether LAN co-op actually survives on these devices; solo remains available either way |
| Lose the hotspot or close one client | Private continuation stays usable; other reachable clients continue on the designated server |
| Offline TV/book/science/secret-room session on the older iPad | Correct local audio/content; bounded memory and effects; existing performance targets still met |
| Return after offline bedroom edits | Server room loads in shared play; private room remains available locally and never overwrites shared state |
| Rehearse the free signing refresh away from PC | New expiry, successful app launch, room saves and imported clips preserved on both iPads |

Observe the children as well as logs: can the younger child make something happen in a few taps, join without instructions, find Come out, and get back from a secret room? Does the older child have useful choices without controlling the younger child's camera? Do parent calls and rattles create anticipation rather than frustration? These observations determine whether the proposed timings and controls are actually fun.

**Research status:** this expansion is recorded in the main tracker and the [science, rooms, hiding, and travel evidence index](bluey-research/science-rooms-travel-evidence.json). Scientific references, documented engine capabilities, and platform constraints are distinguished from our proposed game behavior. No dedicated server, networking change, room, science mini-game, signing tool, or new iPad build has been implemented by this research update.

## 37. The beach: collecting, building, and playing together

**Expansion recorded September 23, 2026.** Sections 37–43 add 32 outdoor activity designs, Daycare as the sixth world, a flexible pretend day, 12 learning stations, and all nine requested imagination stories. These are proposed designs, not finished games. The same activity can appear in a world, on the general activity picker, or in a daycare invitation. Those appearances are not separate mini-games. I interpret “freezeb” as **Frisbee / flying disc**.

The official **The Beach** synopsis provides useful anchors: shell discovery, following footprints, birds and small beach animals, and waves changing the shoreline. Our sandcastle, disc, ball, and café rules below are new adaptations. [The Beach, S1E26](https://www.bluey.tv/watch/season-1/the-beach/)

### Ten beach activities

| ID / activity | Simple Play | Explore & Stories / together |
| --- | --- | --- |
| BCH-01 Shell treasure | Pick up any shell; tap to hear its gentle sound; drop it in a large tray | Sort by shape, decorate a castle, make a pattern, or bring a favorite to a bedroom shelf; both children contribute |
| BCH-02 Seagull surprise | Walk near a small flock or tap a nearby bird; birds flap away and settle again | Follow their footprints to another patch of sand; invite the sibling to approach from the other side; no hitting, capturing, or score for repeated chasing |
| BCH-03 Waves and footprints | Watch foam roll in; tap the water for ripples; walk to leave prints | Draw a trail and watch the next wave erase only the wet-sand part; count waves together if wanted; no timer or required dodging |
| BCH-04 Sandcastle workshop | Lift a ready-filled mould; add a flag or shell | Scoop sand → add a little water → press mould → lift → decorate; one child prepares towers while the other joins them with walls |
| BCH-05 Beach ball | Tap or drag-release a ball for a gentle bounce or roll | Tap the sibling/NPC to pass, roll through a big hoop, or keep a shared rally going with generous automatic catches |
| BCH-06 Flying disc | Pick a pictured receiver and tap Throw; a curved animation carries the disc to them | Optional drag-and-release aiming with a wide catch area; NPC catches if playing alone; misses land within easy reach |
| BCH-07 Crab-and-footprint trail | Follow a few large footprints and meet a waving crab | Choose a crab-walk or hopping animation, make your own trail, and lead a friend to a shell; the crab is an animated observer, not a collectible |
| BCH-08 Sand pictures | Drag shells and pebbles into a picture or draw broad marks in sand | Make spirals, faces, patterns, or a shared mosaic; save a postcard of the creation without needing a camera or internet |
| BCH-09 Beach picnic café | Put a ready snack on a plate and give it to a friend | Pack a basket, arrange a blanket, pour drinks, serve picture orders, then wash props; reuse the kitchen and cleanup rules |
| BCH-10 Kite meadow by the beach | Tap a kite to launch; move a large handle to make it swoop | Add a tail, choose a wind ribbon, or fly two kites through broad cloud shapes; automatic recovery and no tangled-string simulation |

### Make the beach objects connect

A shell can be **collected → rinsed in a bucket → placed on a castle → used in a pattern → saved on a bedroom shelf**. A bucket works at the tap, shoreline fill zone, castle station, and rinse tray. A beach ball works on sand, in a shallow splash zone, and in the park. This is the Toca-style interaction goal: useful combinations across activities rather than a separate animation for every toy.

Represent sand as a small set of authored states—dry, damp, moulded, decorated—with contents and attachments. The younger child receives prefilled moulds; the older child can assemble the sequence. Never require an exact water ratio or make a slightly overfilled bucket ruin the activity. Repeated taps should visibly respond without adding unlimited objects.

Waves are layered 2D art and a shared phase value, not a fluid simulation. Their shallow-water strip triggers footprints fading and toy bobbing. Keep the main building shelf above that strip: waves should not erase a child's saved castle. A child can deliberately splash a disposable practice castle and rebuild it. Seagulls use a small state machine: resting → startled → flying → landing → resting, with a cooldown so they can land even when a child holds a finger down.

### Beach co-op and exit rules

Each castle has a creation ID, owner/participants, component slots, and a saved layout. Reserve a slot only during the short placement operation; reject an occupied slot with a nearby ghost preview instead of replacing a sibling's tower. Keep two scoops and two moulds available. Both children can share a castle or build separately.

Passes target a stable player ID, not their current Bluey/Bingo appearance. If a receiver leaves, the ball/disc lands at a safe anchor or passes to an NPC. If a child changes worlds while carrying a shell, use the existing inventory transfer transaction; do not leave one copy behind and create another in the backpack. Beach ambience, art, instructions, and NPC partners must be installed for offline travel.

## 38. The creek: rocks, water, and gentle discovery

The official **The Creek** episode supports exploring a natural play space and a pretend mud spa. **Barky Boats** provides a separate school-play inspiration for small boats, while **Piggyback** offers playful movement along a walk. The creek activities below combine those themes with original touch interactions. [The Creek, S1E29](https://www.bluey.tv/watch/season-1/the-creek/), [Barky Boats, S2E30](https://www.bluey.tv/watch/season-2/barky-boats/), [Piggyback, S2E18](https://www.bluey.tv/watch/season-2/piggyback/)

### Ten creek activities

| ID / activity | Simple Play | Explore & Stories / together |
| --- | --- | --- |
| CRK-01 Rock collection and washing | Pick up a chunky rock; rinse it to reveal its pattern | Sort smooth/rough-looking, large/small, or patterned/plain toy rocks; display favorites or use them in stacks |
| CRK-02 Creek fishing | Tap a visible fish and receive a generous catch cue | Choose a float, catch, observe, and release; use two rods and a shared picture album; reuse the backyard fishing system with a creek setting |
| CRK-03 Log crossing | Tap the far end; character walks the log automatically | Carry a leaf parcel or stop at a wide lookout; sibling can follow; no precision balancing, fall punishment, or mandatory speed |
| CRK-04 Rock towers | Drop rocks over a broad base and snap them into a stable stack | Choose sizes and arrangements, add a leaf flag, or create two linked towers; deliberate Knock down button affects only the selected creation |
| CRK-05 Leaf and bark boats | Drop a ready boat into the launch patch and watch it float | Add a leaf passenger, choose a branch route, guide to a dock, or share a delivery route; no elimination race |
| CRK-06 Stepping-stone path | Tap a large marked stone to hop across | Place a short series of stones, test the path, and make a shape trail for a friend |
| CRK-07 Pretend mud spa | Spread illustrated mud on Bandit's offered arm and rinse it away | Mix pretend mud, place leaf decorations, wash, and dry; Bandit volunteers locally; a toy customer substitutes if he is busy elsewhere |
| CRK-08 Nature spotter | Tap a frog, insect, leaf, or ripple to see a short response | Match a sound or silhouette to a nearby illustrated animal; add an observation stamp; creatures remain in the scenery |
| CRK-09 Waterwheel tray | Pour a bucket into a toy channel and watch a wheel turn | Rotate two broad channel pieces to send water to a wheel or a plant; child one pours, child two routes; all actions also work solo |
| CRK-10 Nature postcards | Place leaves, rock pictures, and boat stamps on a card | Arrange a creek scene, choose a recorded caption, and take the card home; reuse the drawing/creation-save system |

### Build reliable movement and water play

The illustrated log is a **special traversal path**, not a narrow physics collider the children must balance on. Entering reserves a start anchor and selects a walking animation; the game controls the character along a legal curve. Walking away/cancel moves them to the nearest valid bank. Use two noncolliding lanes or staggered animation spacing so a stationary avatar cannot block the sibling forever. A log crossing remains usable with joystick or tap walking; items stay in a stable carrying pose.

Rock stacking uses authored support points, generous snapping, and a bounded tower height. Keep a free decorative placement area for the older child, but separate it from the assisted tower slots. A shared placement command checks support, stack version, and slot ownership before committing. A failed placement returns the rock to the child's hand. Do not rely on networked rigidbody contacts to decide whether an entire tower collapses.

Waterwheel channels form a tiny directed graph: source → selected channel → wheel or basin. Animate water only on connected segments. Rotate the wheel according to a clamped flow value; stopping the pour stops the flow after a short visual tail. This reuses containers and pouring without simulating thousands of liquid particles. Boats follow authored current paths with positions reconstructed from route ID and start time; a dock captures them so they cannot disappear downstream.

Fishing has one owner per fish/catch transaction. Two children selecting the same fish should see another equally interesting fish become available. Leaving mid-catch releases the fish and rod reservation. An observation bowl contains a reference to the temporary catch, not an independent duplicate. Catch-and-release remains optional free play without collection deadlines.

**Creek persistence:** save rock displays, finished towers, boat decorations, and postcards. Save a moving boat's logical route/checkpoint rather than every wave offset. On resume, restore it at a reachable dock if its route is unavailable. These are game props and illustrations; real species/mineral labels should be researched individually before educational labels are recorded.

## 39. Playground and park: equipment that really works

Make the equipment the attraction: a child should be able to approach a swing or slide and use it immediately, without first accepting a quest. The official **See Saw** story supports cooperative equipment play; **Bike** supports trying different playground challenges; **Shadowlands** supports an imaginative movement game in the park. [See Saw, S2E27](https://www.bluey.tv/watch/season-2/see-saw/), [Bike, S1E11](https://www.bluey.tv/watch/season-1/bike/), [Shadowlands, S1E5](https://www.bluey.tv/watch/season-1/shadowlands/)

### Twelve park activities

| ID / activity | Simple Play | Explore & Stories / together |
| --- | --- | --- |
| PRK-01 Swings | Tap a seat to sit and swing gently; large Get off picture | Tap Push or choose a gentle/faster preset; push a sibling or NPC; two usable seats |
| PRK-02 Slide | Tap the ladder or slide picture; character climbs and slides | Add a toy passenger or use a second slide lane; replay without navigating a tiny ladder hitbox |
| PRK-03 Seesaw | Tap a seat; an NPC balances the other end | Sibling replaces NPC, take turns bouncing, add a toy passenger; no weight comparison between children |
| PRK-04 Climbing tower and monkey bars | Tap the next large hand/foot picture; assisted traversal | Choose among two short routes, carry a flag to the platform, then slide down |
| PRK-05 Roundabout | Sit and tap a large Start/Stop picture | One child pushes while another rides, swap whenever wanted; gentle capped speed and stationary camera |
| PRK-06 Tag | Chase or evade a friendly NPC in a small visible zone | Invite sibling, choose who starts, optional roles swap after each touch; no eliminations or scoreboard requirement |
| PRK-07 Hide-and-seek | Parent seeks in a bounded park hiding area | Optional child seeker; behind bushes, playhouse curtains, and a tunnel alcove; same configurable clues as the house |
| PRK-08 Shadow stepping | Tap connected shadows to walk a trail | Choose a shadow route or place a parasol to make a new patch; grass gives a playful response and a return option, not failure |
| PRK-09 Build a play trail | Place one stepping pad and walk over it | Combine pads, a low tunnel, flags, and hoops; test the sibling's course and rearrange it |
| PRK-10 Bike and scooter loop | Tap a destination on a broad loop; assisted riding | Choose a loop, ring a bell, deliver a picnic item; auto-steer and no traffic/collision punishment |
| PRK-11 Picnic and pretend shop | Arrange ready snacks on a blanket | Serve picture orders, share fruit, pour drinks, tidy plates; reuse kitchen, container, and cleanup systems |
| PRK-12 Music statues and follow-the-leader | Tap Dance, then copy a pictured pose when music pauses | Take turns selecting a movement; NPCs follow; moving during a pause is funny and never eliminates anyone |

### Equipment as a reusable system

Each usable item needs entry anchors, seat/hand anchors, a motion curve, a valid exit anchor, supported poses, and optional co-op roles. Use **Available → Reserved → Boarding → In use → Exiting → Available**. Reservation has a short timeout; a disconnect, cancelled navigation, failed animation, or missing asset releases it. If both tap one seat at once, resolve once on the authority and visibly offer the adjacent seat. Avoid making the second child wait through a long queue.

Attach the avatar to a semantic seat anchor while the ride animates. Keep its logical position, seated pose, occupied seat, and motion phase synchronized; do not repeatedly reparent network objects in response to each local animation frame. Switching character rebinds the rig to the same seat. Exiting should be possible at any point: decelerate briefly if needed, then place the child at the safe exit. Do not wait for the sibling to finish or for a global round timer.

### Tag that works for both children

Start from a tag picture on the activity mat. A brief spoken invitation explains “Catch me!” or “You're the runner!” with a large role icon. Only children who join can be tagged. Unjoined children, babies in pretend-play scenes, and NPCs doing unrelated jobs are not targets.

Use a small connected play area, clearly visible participants, a generous reach radius, and automatic tagging at close range; tapping the target can also request a tag. The authority validates participant roles, distance, reachable ground, and a short protection period after the last tag. Start with a proposed two-second protection window and visible sparkle ring so tag cannot bounce back instantly. This is a tuning starting point, not a tested ideal.

Give Simple Play runners slower NPC pursuit and short pauses before turns. Give a Simple Play seeker an NPC that approaches and waits occasionally. Avoid competitive advantages tied to a favorite character's body size. After a tag, children can swap roles or keep chasing an NPC. If the human seeker leaves, an NPC takes over; if the runner leaves, a friendly NPC becomes the target. No remaining player waits for a restart. Test latency before adding complex prediction; generous ranges and readable movement are preferable for this small family game.

### Outdoor hide-and-seek

Reuse section 34's parent-seeker default, optional child seeker, Ready state, Come out control, and 5–10-second optional hints. Author a separate bounded park arena with visible hiding entrances and enough spots for both children. Validate each spot's reachable search anchor. The parent target of finding a ready child in roughly 30 seconds applies inside that arena, not across every world. The sibling can join with their own preparation period. A child leaving the arena withdraws only themselves and is never forcibly teleported back. The parent can finish the other child's search.

## 40. Daycare: a sixth world with an optional pretend day

**Daycare remains one of five destinations:** Heeler Home (including Backyard Garden), Playground & Park, The Creek, The Beach, and Daycare. Its rooms and imagination destinations are subareas, so they do not each need another world bubble.

Use Calypso as the main teacher and combine a classroom, play yard, book corner, art/sensory tables, pretend kitchen, quiet cushion nook, and imagination mat. The show places the older children at Calypso's school; our all-ages daycare is an intentional family-game adaptation where the entire requested child roster can gather. It is not a claim that every show's child attends the same canonical classroom. The official character page identifies Calypso as Bluey's teacher and emphasizes encouraging imaginative play. [Calypso character reference](https://www.bluey.tv/characters/calypso/)

**Proposed default while the timing preference is unconfirmed:** a short pretend day, about 20–30 minutes if a child follows the invitations, with unlimited free play and the ability to pause, skip, repeat, or leave. These minutes are a design estimate, not a recommended screen-time duration. Morning/afternoon are story phases, not real-clock deadlines. Nothing is missed because the family launches at night or spends an hour decorating.

### A day that feels planned without becoming compulsory

| Picture on the day board | What Calypso offers | Child's freedom |
| --- | --- | --- |
| Hello / backpack | Arrive, choose a cubby picture, greet a friend, inspect today's cards | Head straight to toys if preferred |
| Book / circle rug | A short read-aloud or picture-story invitation | Listen, tap pictures, replay, or leave independently |
| First adventure card | First randomly selected activity from the whole game's eligible catalog | Play alone, invite sibling, replace this unstarted suggestion, or skip |
| Snack / counting plate | Pretend snack plus an optional counting or sharing task | Free arrangement is still valid; no mandatory answer |
| Second adventure card | A different selected activity, possibly in another world | Accept a personal field trip or stay in daycare; sibling is not pulled along |
| Music / art / discovery | A short lesson chosen from section 41 | Choose another station or use materials freely |
| Optional third adventure | A third selected activity for a longer pretend day | Entirely optional, with a two-activity day also available |
| Cushions / story / goodbye | Quiet reading, imaginative play, a saved picture of the day | Stay, repeat a favorite, go home, or close the app without finishing a checklist |

The 2–3 adventure cards are the daily random mini-game picks; books, snack, music, and free-play stations are routine choices around them. Both children see the plan, but each has their own current card and participation. One can paint while the other takes a beach trip. Calypso offers one short spoken invitation at a natural pause; she does not keep calling over a child's active book narration or interrupting a slide.

### Selecting the 2–3 activities fairly

Give each existing mini-game a stable ID, activity family, origin world, available settings, assistance variants, local-content status, NPC fallback, and estimated length. The candidate pool spans **all six worlds**, including home cooking, backyard fishing, science, dinosaurs, beach, creek, park, and imagination stories. A recipe variant is not automatically a different family; avoid “three pizza games” as the entire random day.

When a pretend day is created, select two or three eligible activities without replacement, prefer different families/worlds, and reduce the weight of very recent selections. Save the seed and final ordered IDs immediately. A returning child or later joiner sees those saved choices: joining must not reroll the day. Never select unimplemented content, missing offline assets, a two-human-only task, or an unsupported device feature. If only a small prototype catalog exists, choose from what actually works and explain the reduced variety in parent-facing development notes.

Suggested pseudocode for the planned system:

```text
CreateDay(catalogVersion, localContent, recentHistory, activityCount):
    eligible = implemented AND contentInstalled AND soloFallbackAvailable
    picks = weightedSampleWithoutReplacement(eligible, count=2 or 3,
             preferDifferentFamiliesAndWorlds=true, avoidRecent=true)
    persist DayPlan(dayId, seed, catalogVersion, picks)
    return DayPlan

JoinDay(playerId, dayId):
    load saved plan; create or restore that player's cursor
    offer Join sibling or My next activity; do not reset any session
```

Each player's visits reference a DayPlan and keep their own skipped/completed cards. Starting a new pretend day creates a new plan for that visit; it never overwrites the sibling's still-active day. Unstarted cards can be replaced for that child without changing an activity already running. A shared session records its actual activity ID independently of either child's schedule. “Follow Calypso” is an invitation, not a compulsory group teleport.

### Have the whole cast here without crowding the screen

Make the entire requested child roster available in daycare, including individual Terriers, with an illustrated friend board to find/invite a character. Spread classmates among the classroom, yard, book corner, and imagination stations. Not every full animation rig needs to be loaded or pathfinding in one camera view. Preserve every character's logical location and current routine; render detailed actors when a player enters their area. The roster remains available rather than being reduced to a handful of favorites.

Begin performance experiments with roughly 6–10 detailed visible NPCs plus up to four human avatars in a view, then measure on the A10; that is an initial budget hypothesis, not an established device limit. Background friends may sit, draw, read, or build using inexpensive authored loops. Child avatars never have to queue behind NPCs for essential toys. NPCs yield reserved stations and move to valid alternate anchors when invited.

Calypso's routine can be greeting → reading → observing → helping → resting, interrupted by a bounded help request. Lessons can start through their material/picture card even if her actor is across the room. Play the local instructional recording with her portrait, without teleporting or cloning her body into both children's different locations. Use prewritten instructions, NPC state machines, and reviewed recordings; an online LLM or live voice generator is unnecessary for this behavior.

## 41. Daycare learning: short, playful, and spoken

The learning design should blend freely chosen play with brief demonstrations and guided practice. NAEYC describes that combination and emphasizes adapting help to individual children rather than requiring everyone to follow an identical path. These principles inform our design; they do not validate an unbuilt app or establish a particular session length. [NAEYC: teaching to enhance development and learning](https://www.naeyc.org/node/3812)

Head Start's preschool literacy framework includes sound awareness, letters and print, and understanding/retelling stories. Its music guidance connects rhythm, movement, and patterns. We can turn these into spoken, touch-based activities without requiring reading to navigate. The framework covers preschool development; the proposed six-year-old extensions below are our adjustable game content, not a claim that all six-year-olds have the same skills. [Head Start literacy](https://headstart.gov/school-readiness/article/literacy-preschool), [Head Start music and patterns](https://headstart.gov/teaching-practices/play-head-start-way/1-2-3-dance-me)

### Twelve starter lesson cards

| ID / station | Simple starting invitation | Optional deeper invitation |
| --- | --- | --- |
| LRN-01 Talking dinosaur book | Listen to one short page; tap the dinosaur to hear its name | Pick a picture answering a spoken question or predict the next picture; reuse the reading nook |
| LRN-02 Sound basket | Hear two everyday sounds and match one to a big picture | Match initial sounds or spoken rhymes with pictured choices; language-specific content |
| LRN-03 Letter delivery | Match identical large letter shapes on a parcel and cubby; hear the letter name | Choose a familiar letter after hearing its sound; later blend a few carefully selected simple words with optional help |
| LRN-04 Tell the story | Put two pictures into first/next slots and hear the resulting narration | Arrange three or four events, choose an ending, or act the sequence with toy characters |
| LRN-05 Picnic counting | Give each of 1–3 pictured guests one plate; count each committed placement aloud | Prepare sets up to 5 or 10 as appropriate; ask how many altogether and show the set, not just a numeral |
| LRN-06 Give the dinosaur a snack | Match one or two fruit pictures to a plate | Hear “give three,” choose the set, then add or take away one; remove extras without a failure screen |
| LRN-07 Compare and sort | Choose a big/small basket or sort two visibly different shapes | Compare two sets, order a few lengths, or sort by a new property; avoid color-only distinctions |
| LRN-08 Shapes and patterns | Place a circle in its broad outline or continue a two-item pictured pattern with help | Rotate a simple shape, build an AB/ABC pattern, or copy a short block design |
| LRN-09 Copy the drum | Tap a drum; every touch produces a satisfying sound | Echo two to four gentle beats or add a second instrument; no strict accuracy grade |
| LRN-10 Musical painting | Touch big high/low or slow/fast sound pictures and see matching movement | Arrange a short musical phrase with picture tiles, then hear both children's parts |
| LRN-11 Friends and feelings | Match an expressive face to a spoken feeling; offer a plush or a wave | Choose among several helpful responses in a pretend scene; different caring choices can all be valid |
| LRN-12 Predict and discover | Choose a float/sink picture, then drop a toy in the science tub | Try a second material, compare outcomes, and explain using picture choices; reuse section 30's reviewed science rules |

Start with roughly 1–3 minutes of guided content per invitation, but allow repetition and unstructured play indefinitely. Assistance is per skill and per child: a child can enjoy complex dinosaur stories while needing simpler counting. The age labels suggest starting settings only. Do not give the younger child a locked “baby mode” that hides favorite characters or stories.

### What the teacher actually does

Use a small authored teaching loop: **show → invite → wait → acknowledge → optionally demonstrate**. For counting: Calypso shows three guests, says an original line such as “Let's give each friend a plate,” and the plate tray lights gently. Each actual new placement speaks the next number once. Moving the same plate around must not count it twice. If the child puts all plates in a pile, acknowledge the action and optionally demonstrate one guest/one plate rather than announcing failure.

After repeated difficulty, make the target clearer or offer “Show me.” When a child succeeds comfortably, offer a slightly deeper card on a later visit; do not change the task halfway through without explanation. Avoid grades, streak loss, red crosses, speed tests, required microphones, or school-like gates. Parent settings can remember chosen assistance and recently enjoyed cards locally; do not label those observations as a formal learning assessment.

For speech, record complete prompts where joining fragments would sound unnatural. Keep object names and short count sequences reusable where they sound good. Queue only the current child's relevant line, duck background music, and cancel obsolete instructions on exit. A sibling starting another lesson must not overwrite the local narration. Spanish sound, rhyme, and phonics activities need their own reviewed examples; translating an English word pair does not preserve its rhyme or initial sound. English is the first complete content pass.

For drums, respond immediately on the local iPad, then synchronize the shared visual/event. Use a scheduled backing track and generous beat windows if needed; Unity's `AudioSource.PlayScheduled` uses the audio DSP timeline, but it does **not** automatically synchronize clocks between iPads. Map session time to each device's local audio clock, recover after resume, and test audible echo on the actual pair of devices. A forgiving duet should remain enjoyable even when network timing is imperfect. [Unity PlayScheduled](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSource.PlayScheduled.html)

## 42. The imagination mat: nine stories that become playable worlds

**Recommended presentation:** children choose a picture on a large play mat, hear a short premise, choose a pictured role, and enter an illustrated story space. The daycare boat can become a spaceship; blocks can become a castle; a stump can become a helicopter. Reuse the same touch rules so imaginary worlds feel richer without requiring a new control scheme.

### Starting, understanding roles, and leaving

1. Tap the mat. Show a small page of large story pictures with easy arrows; all nine remain available without quest unlocks.
2. Tap a picture to hear a brief original description. A Play triangle enters role choice; Back returns immediately.
3. Offer two or three clear role cards first, with more roles if useful. Each has an object icon, a short demonstration, and a spoken action: pilot/steering wheel, builder/block, explorer/map. Roles describe what to do, not a reading assignment.
4. Choose any favorite character independently of role. Either child may be pilot, carer, queen, astronaut, or builder; both can choose the same kind of role where the story supports it. NPCs fill any necessary missing function.
5. Show up to three picture steps and transform into the story. Simple Play highlights one useful prop; Explore & Stories exposes more choices. The child can also just play with objects.
6. Offer a small Join invitation on the sibling's iPad. Acceptance opens a quick role choice and brings only that child to a safe entry point. Declining changes nothing.
7. Keep Change role, Listen again, and Return to daycare available. Changing appearance or role preserves completed actions. Leaving releases tools and replaces essential roles with NPCs.

The following **episode facts** are paraphrased from official synopses. Every playable objective, role system, difficulty variant, and transformed environment is our own proposed adaptation.

### IMG-01 — Calypso: a connected pretend town

**Verified inspiration:** the children's fish-and-chip shop, house, family, gnome village, protectors, and fishing gradually connect through Calypso's suggestions. [Calypso, S1E17](https://www.bluey.tv/watch/season-1/calypso/)

**Our game:** blocks unfold into a miniature town with a café, home, pond, and gnome garden. Choose cook, builder, fisher, delivery helper, or garden protector. Simple Play gives one action—put food on a plate or place a roof. The older child can build a house, prepare a picture order, collect a fish-shaped pretend ingredient, and deliver lunch. These are invitations, not a dependency chain that prevents everyone from playing if one child stops fishing. A stocked basket/NPC supplier keeps the café usable.

Reuse recipes, delivery slots, building pieces, gardening, and toy fishing. Let both children independently make things whose outputs can be offered to the other. NPCs ask softly for one useful object and accept substitutes. Start with three small stations, then connect the full town after shared object transfers are reliable.

### IMG-02 — Helicopter: trips and friendly rescues

**Verified inspiration:** Bluey's stump helicopter becomes a shared trip with passengers, destinations, traffic reporting, and a kangaroo rescue; she learns to share control. [Helicopter, S2E25](https://www.bluey.tv/watch/season-2/helicopter/)

**Our game:** choose pilot, passenger/map helper, or rescue helper. Tap a destination picture; the illustrated helicopter travels along a gentle guided route. Simple Play lowers a basket onto a large target to pick up a stranded toy kangaroo. Explore mode chooses an island, lookout, and landing pad, then delivers supplies. Make the rescue reassuring, without a fire countdown or punishments. Both can operate separate controls; a second pilot can select a future destination or take over on request without an argument over an exclusive seat.

Reuse ride anchors, route travel, target snapping, containers, and delivery. On pilot departure, autopilot/NPC completes the route to a valid landing; passengers retain an immediate Return option. A new passenger joins at a safe seat with a spoken “We're going to the island,” rather than replaying takeoff for everyone.

### IMG-03 — Wild Girls: woodland and farm meet

**Verified inspiration:** woodland play and farming combine, with Coco taking the Pink Witch role after initially resisting changes to the game. [Wild Girls, S3E44](https://www.bluey.tv/watch/season-3/wild-girls/)

**Our game:** offer woodland friend, farmer, or kind Pink Witch. Plant a seed, water it, gather pretend produce, and use a wand to make a flower arch between the forest and farm. Simple Play taps a ready plant or casts a friendly growth animation. Explore mode delivers ingredients to a shared woodland picnic and decorates the connecting path.

Reuse planting, watering, carrying, and decorative placement. Magic changes authored prop states; it does not randomly destroy another child's crop. Both halves remain usable alone. A visiting sibling can bring flowers or food without having to abandon their chosen role.

### IMG-04 — Early Baby: caring helpers and a dragon story

**Verified inspiration:** Indy's hospital game and Rusty's knights-and-dragon rescue are separate pretend games that cause a misunderstanding. [Early Baby, S1E41](https://www.bluey.tv/watch/season-1/early-baby/)

**Our game:** choose carer, blanket delivery helper, or friendly knight. Keep the baby's nursery calm: tuck in a doll, arrange a blanket, select a lullaby, and carry a picture parcel. In an adjacent fantasy corner, a friendly dragon has misplaced a blanket and needs help finding it. Simple Play is one comforting action; Explore mode links nursery supplies and the dragon's delivery route.

Reuse doll placement, fabric overlays, music, searching, and deliveries. Keep this pretend care rather than a medical treatment simulation; there are no health meters, urgent alarms, or baby distress penalties. An NPC can hold the doll when a carer leaves. The knight activity cannot remove a doll another child is currently caring for.

### IMG-05 — Typewriter: a picture-story quest

**Verified inspiration:** Bluey, Snickers, and Winton travel to Calypso, face the Terriers' pretend arrows, and discover ways their different qualities help. An imagined typewriter becomes enough for play. [Typewriter, S2E48](https://www.bluey.tv/watch/season-2/typewriter/)

**Our game:** choose storyteller, shield helper, or trail finder. Gather three picture tiles, cross a broad bridge, shelter behind a playful bubble shield, and arrange the tiles into a narrated story. Simple Play follows a highlighted tile and taps the shield; Explore mode chooses routes and a story ending. Pretend arrows are soft visual tokens that bounce off a shield without damage.

Reuse collect/snap, route choices, shield zones, and the story-sequencing lesson. Abilities belong to tools/roles, so a child who chooses Bingo can still use the trail-finder function. NPC helpers supply missing abilities; no required role or body shape prevents solo play. Keep original generated dialogue and authored story branches local.

### IMG-06 — Mums and Dads: families with flexible roles

**Verified inspiration:** Indy and Rusty disagree over home/work roles, try other play groups, and return to shared family play. [Mums and Dads, S1E33](https://www.bluey.tv/watch/season-1/mums-and-dads/)

**Our game:** choose grown-up, baby, babysitter, shop worker, or helper and change at any time. Set a table, feed a pretend doll, pack a bag, drive a toy bus along a route, or deliver a snack. Simple Play focuses on one household object. Explore mode combines a short morning routine or invents a different family arrangement.

Reuse kitchen, doll, cleanup, dress-up, and delivery systems. Any character can take any role. Choosing baby never locks a human avatar into a cot or removes controls. If a human leaves a care role, an NPC continues the pretend routine; nobody receives a guilt message about abandoning someone.

### IMG-07 — The Adventure: a magical kingdom

**Verified inspiration:** Bluey and Chloe act multiple roles in a kingdom story involving a quest for food, queens, a magic wand, and rescuing frozen characters. [The Adventure, S1E38](https://www.bluey.tv/watch/season-1/the-adventure/)

**Our game:** choose royal explorer, horse/rider helper, or wand keeper. Blocks become castle walls, a rug becomes a meadow, and picture food becomes a picnic for the kingdom. Simple Play taps the wand to wake a statue or places a food item. Explore mode finds three supplies, opens a bridge, and invites everyone to a feast.

Reuse building, delivery, riding, and prop transformations. Freeze NPC statues by default; do not immobilize a child's avatar as a punishment. If children opt into a freeze-play variant later, include an immediate self-release control. Costume and role swaps must not reset the kingdom's food table or bridge.

### IMG-08 — Space: a cosy astronaut voyage

**Verified inspiration:** Mackenzie, Jack, and Rusty use a playground boat as a spaceship, with Mars, hypersleep, a black hole, and other children as aliens. The episode also has an emotional storyline about being left behind. [Space, S3E34](https://www.bluey.tv/watch/season-3/space/)

**Our game:** focus on reassuring exploration: astronaut, navigator, scientist, or friendly alien. Tap a star map, launch, collect a pretend space rock, and greet an alien. Simple Play presses a large launch button and places a rock in a tray. Explore mode assembles a small rover, follows a three-stop route, or waters an imaginary space garden. A swirling fantasy portal is optional; it is not a lesson claiming real black holes are safe tunnels.

Reuse vehicle travel, rock collection, toy assembly, gardening, and backdrop transitions. Rest pods are optional seats with immediate exits. Do not reproduce a forced abandonment sequence or leave a child trapped in hypersleep when the sibling departs. An NPC navigator and persistent Return gate keep every route completable offline.

### IMG-09 — Explorers: sail home together

**Verified inspiration:** Jack and friends pretend to sail to Australia while waiting for pickup; as classmates leave, Jack continues, while his dad has his own journey to the school. [Explorers, S3E15](https://www.bluey.tv/watch/season-3/explorers/)

**Our game:** choose captain, lookout, map helper, or shore explorer. A playground boat becomes a ship on a friendly illustrated ocean. Simple Play taps a large island/home picture and rings a bell on arrival. Explore mode follows two or three landmarks, carries a picnic parcel, and chooses the next stop. NPC classmates wave goodbye when collected, then another helper can take a necessary role.

Reuse route travel, map pictures, lookout discoveries, and delivery. NPC pickup is atmosphere, never a real deadline for the children. If one child closes the app, the other stays captain or continues their own role with an NPC crewmate. Visiting an island must not require both human players to approve a transition.

### Transform one child's view without interrupting the other

Create a separate **ImaginationSession** with its own zone/instance ID, activity definition, participants, role assignments, props, phase, and checkpoints. Daycare remains active as a different zone. If one child enters Space and the other keeps painting, only the first client loads/displays the spaceship presentation. If the second joins Space, it receives the existing session state. If family members choose different stories, support up to four lightweight logical sessions; do not transform a single global classroom for everyone.

Do not implement every transition as a global network scene replacement. Separate authoritative simulation state from client visual loading and filter objects by zone/session membership. The home PC can simulate both places while each iPad renders its own location. Travel solo runs the same rules locally. Optional remote co-op uses the same PC/VPS authority, with separate client views.

Keep the real child avatar persistent; bind its role and visual to the current session. An NPC cast member has a session-specific actor ID so a helper in one imagination story cannot steal another session's prop. Story casting is distinct from the visible daycare roster. Do not create two network authorities for a character merely because both children chose Bluey.

## 43. Building, saving, and testing the outdoor and daycare expansion

### One activity framework, many settings

Use Unity `ScriptableObject` assets to author definitions: activity ID, pictures, spoken prompts, tools, prop rules, simple/explorer variants, valid zones, roles, NPC fallback, completion suggestions, cancellation, and save schema version. Keep runtime progress in separate serializable state. Unity documents ScriptableObjects as shared asset data and distinguishes editor asset saving from runtime persistence in a built application. A ScriptableObject asset is not the iPad save-file system. [Unity ScriptableObject manual](https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html)

| Planned data type | Durable information | Temporary presentation |
| --- | --- | --- |
| ActivityDefinition | Stable ID/version, capabilities, local asset keys, rules, roles, assistance variants | None; authoring data |
| ActivitySession | Session/zone ID, participants, role bindings, phase, step facts, prop IDs, revision | Local highlights, sound playback, camera |
| Creation | Owner/participants, recipe/layout, component slots, attachments, origin session | Foam, dust, sparkle, momentary wobble |
| EquipmentState | Seat occupancy, logical ride state, start time, exit anchor | Per-frame pose and interpolation |
| DayPlan and DayVisit | Chosen 2–3 IDs, seed, catalog version; per-child card progress | Teacher pointing animation and day-board selection |
| LearningPreference | Per-child assistance and locally stored recent station history | Current hint timer; no school-grade ranking |
| ImaginationSession | Story, roles, route/checkpoint, cast references, saved creations | Backdrop transition and transformation effects |

The same fishing definition powers backyard and creek variants; the same recipe system powers home, café, and Calypso's town. Daycare selects existing definitions and gives them a new invitation/context. It does not require rebuilding 32 outdoor games as 32 more daycare implementations. Avoid tying completion to an exact avatar, mandatory second human, or the teacher physically standing beside a prop.

### Shared state and late joining

Authority validates commands such as JoinActivity, TakeRole, ReserveSeat, PlaceComponent, CatchFish, and LeaveActivity using player ID, session ID, object ID, expected revision, and a unique operation ID. Retried commands must not produce duplicate shells, towers, rewards, or deliveries. Only a short active gesture owns a prop; stable created objects should not disappear with their creator's network connection.

Send a consistent snapshot plus subsequent changes when a child joins: current role availability, finished steps, held/reserved props, ride state, and route checkpoint. Persistent state must not depend on replaying an old “start animation” message. Unity documents NetworkVariables for current values and late joiners, while RPCs are one-time messages. Use package-version-matched APIs when implementation starts. [Unity NetworkVariables](https://mp-docs.dl.it.unity3d.com/netcode/current/basics/networkvariable/index.html)

Role replacement is a transaction: detach departing player, safely release their prop/seat, assign an NPC only if the role is essential, and publish the new revision. Do not erase the departing child's personal inventory or completed contribution. A returning player can select an available role and resume without forcing the NPC to drop something mid-transfer. If no humans remain in a session, checkpoint it and suspend its simulation; restore on a later visit.

### Saves, road trips, and crash recovery

Use the same activity rules in private solo and the server world. Persist each mode separately and use current server state when rejoining.

Keep offline creations in private saves. Do not add reconciliation/import workflows as a dependency for outdoor content.

One client leaving never stops the PC/VPS simulation. A client losing its route continues private solo; remaining connected players continue together.

### A practical production order

| Batch | Build after the existing interaction/network foundation | Why this comes first |
| --- | --- | --- |
| A — Outdoor essentials | Shell pickup, one sandcastle, creek log, rock stack, swing, slide | Proves collection, construction, special traversal, and seated character swapping |
| B — Shared outdoor play | Ball/disc passes, creek fishing variant, seesaw, tag, park hiding | Tests two-person contention, NPC substitutes, joining, and leaving |
| C — Daycare shell | Sixth world bubble, rooms, friend board, teacher routine, saved 2–3-card plan | Establishes the optional day without needing all lessons or stories finished |
| D — First learning set | Talking book, picnic counting, copy-the-drum; then remaining nine lesson cards | Verifies speech, understandable prompts, per-child help, and local audio timing |
| E — First imagination stories | Calypso town and Helicopter; then Space as a separate-zone stress test | Reuses existing props while proving role choice, vehicle travel, and independent locations |
| F — Content completion | Remaining outdoor activities and the other six imagination stories | Expands tested systems; review every voice line and touch target |

Retain the full requested backlog even while prototyping only representative activities. Finished art, individual character rigs, animation, voice review, and device testing remain significant work; a data-driven framework reduces repeated coding but does not automatically produce all content.

### Acceptance checklist for this expansion

| Test | Required result |
| --- | --- |
| Select all five destination circles with touch | Correct location; no tiny targets, overlap, or quest unlock required |
| Three-year-old enters beach, creek, park, and daycare | Makes an object respond and leaves an activity without reading or adult menu navigation |
| Mix assistance modes during the same activity | Shared object facts agree; each child receives appropriate local hints and controls |
| Simultaneously grab one shell, catch one fish, place one tower piece, or claim one seat | One authoritative result; useful alternative; no duplicate object or stolen completed creation |
| Switch character on swing, log, helicopter, and inside a story role | Pose, stable player identity, held prop, and task progress remain correct |
| Leave every outdoor activity at each meaningful step | Sibling continues; props and seats release; essential NPC roles fill automatically |
| Join tag, hiding, a ride, a lesson, and each imagination story halfway through | Useful role and current state; no global restart or repeated obsolete narration |
| Start a new pretend day while sibling follows the old plan | Both visits remain valid; active sessions are not rerolled or erased |
| Choose only two daily adventures, skip both, then choose free play | Daycare stays usable; no lost access or failure state |
| Daycare random selection across many saved seeds | Exactly 2–3 eligible distinct IDs where enough exist; no missing-content picks; rotation constraints behave as designed |
| Invite every requested child character | Every roster entry reachable; individual Terriers supported; no hidden content gate |
| One child enters Space while the other cooks, paints, or plays elsewhere | Independent camera, zone, speech, and activity state; no global scene replacement |
| Both choose the same role/favorite character | Supported duplicate roles or clear alternate control; no ownership confusion or forced character change |
| Close either iPad during PC-hosted daycare/imagination play | Other child keeps playing; bounded NPC replacement and reservation cleanup |
| Cold-launch every new activity in airplane mode with the PC unavailable | Local art/audio/logic and NPC substitutes work; saves survive relaunch |
| Interrupt save or terminate app during placement/travel/role change | Previous or new valid checkpoint recovered; no half-committed duplicate or missing creation |
| A10 performance after repeated world/story/character swaps | Existing performance budgets met; no accumulating rigs, voices, effects, or abandoned sessions |
Test four clients across independent areas, including one disconnecting/rejoining while siblings keep playing. Test older-iPad local rendering and offline movement; no mobile-host load test is required.

Observe enjoyment as well as correctness: does the younger child discover several uses for one object? Can the older child invent a story without directing every action for the younger child? Can both explain their role from its picture and spoken line? Tune proposed timings and assistance based on that observation.

**Evidence and status:** official episode pages support the story inspirations; NAEYC and Head Start inform the learning approach; Unity documentation supports the selected API concepts. Our mechanics, numeric budgets, schedule, roles, networking design, and teaching scripts are proposals requiring implementation and physical-device tests. The [outdoors and daycare source index](bluey-research/outdoors-daycare-evidence.json) keeps these sources together. No game assets, Unity systems, servers, or device builds were installed or changed by this research update.

<a id="44-automatic-family-connection-and-ipad-hosting"></a>
## 44. Automatic family connection to PC or VPS

The September 25 user decision replaces the earlier mobile-authority proposal. **AUTO-01 stays required; AUTO-02 is retired.** Devices are clients of one designated shared server. No Host/Join menu, peer election, mobile host enrollment or background device server is needed.

| Situation | Behavior |
| --- | --- |
| First child opens the app | Local play starts; discover/authenticate the enrolled PC or configured VPS and join when ready |
| Second, third or fourth opens | Join the same server; other players keep their area and activity |
| Someone chooses Play by myself | Honor deliberate solo until they choose family play again |
| Server is unreachable | Continue private solo without a lobby or a complete-replica wait |
| PC is replaced by VPS | Parent-controlled migration of the same family world; retire old writer before enabling new one |

Connection, visiting a sibling and joining a mini-game are distinct actions. A sibling portrait can offer a visit, but never teleports everyone or reveals a hide-and-seek hiding spot. Books, videos and settings stay local while the server keeps simulating other areas.

Use native DNS-SD/Bonjour discovery for the enrolled home PC and a parent-configured authenticated endpoint for the future VPS. Discovery metadata is not authentication. Foreground retries must be bounded and independent of movement, rendering and saving; a denied network permission must leave solo usable. Do not scan IP ranges or create a new discovery browser every frame. [Apple local-network privacy](https://developer.apple.com/documentation/technotes/tn3179-understanding-local-network-privacy), [Android NSD](https://developer.android.com/develop/connectivity/wifi/use-nsd), [Windows DNS-SD](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceregister).

Bluetooth and router-free peer networking are removed. A hotspot can provide a remote internet route, not an iPad authority. Initial system permissions and parent enrollment remain one-time setup, never repeated child-facing connection chores.

<a id="45-automatic-host-selection-recovery-and-save-reconciliation"></a>
## 45. Server authority, offline continuation and reconnecting

**One shared authority:** the designated PC/VPS owns connected world state. Independent local worlds may differ. They never become a shared server or upload their history into it.

| Client state | Rule |
| --- | --- |
| Local ready | Restore the appropriate private save; play while foreground discovery runs |
| Joining | Authenticate identity/version, receive the current server snapshot, switch at a safe input boundary |
| Connected | Send validated intentions; render smooth motion and authoritative object results |
| Connection lost | Continue privately from the latest usable visible state; clear stale network holds without rewinding to an older save |
| Rejoining | Preserve the private save separately; use server state and discard stale pending network actions |
| Suspended | Save the local/profile state as appropriate; stop unnecessary networking and resume with a new attempt generation |

Ignore callbacks from cancelled attempts. Retry networking outside the frame-critical movement path. Do not require a complete recovery replica, proof of every prior server action, or a successful disk write before local input becomes usable. Responsive local simulation and durable background saving have distinct responsibilities; failed saves need a recoverable record, not frozen controls.

A client closing does not stop the server. Its temporary holds/roles expire or release there. If the server stops, its restart/restore workflow preserves canonical data; clients continue separate solo games until it is available. A disconnected client cannot know whether a remote server has stopped, and must never promote itself based on a timeout.

Connected bedroom changes sync through server revisions. Private bedroom/creation edits remain local, without automatic merge, import, conflict selection or replay on reconnect. Local media bookmarks and preferences remain local. The retained checkpoint experiments can inform server disaster recovery; they are not a requirement for simple offline play.

The live-state model stays separate from disposable network objects and visual scenes. NGO cleanup can destroy network representations on shutdown, so those objects cannot be the sole durable world record. [Inspected NGO shutdown source](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/ddd715da4695a278a143d9af3530fd60a3814b73/com.unity.netcode.gameobjects/Runtime/Core/NetworkManager.cs#L1659). This remains useful evidence without adopting the old mobile-host migration proposal.

## 46. Automatic multiplayer implementation and verification plan

| Check | Acceptance |
| --- | --- |
| Automatic family joining | Four enrolled iPad/iPhone/Android clients join the designated PC/VPS without repeated child input |
| Independent areas | Creek and playground retain one shared identity; visiting meets existing players and never reloads a sibling's area |
| Shared item contention | Exactly one accepted holder; duplicate actions do not duplicate contents or creations |
| Departure and local media | Closing one app, opening a book/video or changing area leaves other players uninterrupted |
| Network loss | Private controls continue without reopening; no periodic walking hitch or jump to an older saved world |
| Offline cold launch | Installed activities and local saves remain available; distinguish OS signing verification from game connectivity |
| Rejoin | Server state wins, original/private saves survive, no offline command upload or item duplication |
| Dedicated server recovery | Backup/restore and restart retain identities, saves and canonical item state |
| Planned VPS migration | Only one canonical writer; old enrollment/world retained through a verified controlled cutover |

Four-device admission and scoped independent shared play already have physical evidence. Build 95's smoother offline walking passed on iPad 7; prepared 98's transition fix needs physical acceptance and rollout. Follow the [current return checklist](implementation/return-checklist-ipad-lan-2026-09-24.html). Device-host roles, election and handoff tests are removed; their earlier experiment records remain historical.

Independent character/art preparation may continue while device checks wait. G5 adds persistent bedrooms, protected creations and reusable item rules; G6 integrates one polished slice, then G7 expands to six worlds. Do not restore G4 as a hidden dependency for those activities.

## 47. Up to four family players across iPad, iPhone, and Android

**Yes: three or four people can play together, including Android mixed with iOS/iPadOS.** Plan for **1–4 human players**, one per device, with the two children's iPads as the required baseline and parent phones joining the same world. Your Android phone is a supported design direction, not merely a fallback behind the iPhone; its model is confirmed as Samsung Galaxy S26 Ultra (SM-S948U1), while its installed Android version and physical-device qualification remain pending. Unity's current Netcode for GameObjects documentation lists both iOS and Android as supported platforms. This establishes engine support, not certification of our unbuilt game. [Unity NGO supported platforms](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/index.html)

This expands the earlier two-player design. Pair examples remain useful scenarios, never capacity limits. **Latest room correction, September 26:** provide four personal bedrooms and four optional secret rooms, one set per persistent player profile, including parents. Add missing rooms without replacing existing identities or contents. A visitor uses the owner's existing room; joining, leaving or choosing the same avatar never duplicates or reassigns rooms. All Home activities, tools, independent media sessions and local settings support all four profiles. The [Home tracker](home-world-feature-tracker.md) is the maintained Home completion checklist.

| Family combination | Proposed support | Connection |
| --- | --- | --- |
| Two children on iPads | Required baseline, with solo and shared play | Home Wi-Fi or tested travel network |
| Two iPads + parent on Android | Three people in one world | Each reaches the designated PC/VPS |
| Two iPads + parent on iPhone | Three people in the same world | Same LAN; same game/session protocol |
| Two iPads + Android + iPhone | Four people in the same world | Same LAN; PC preferred at home |
| Two iPads + two Android phones | Four people in the same world | Same LAN; each Android device/build must qualify |
| No route to the designated server | Independent solo on every device | Direct peer co-op is removed |
| Parent away from home on cellular while children use home Wi-Fi | Now included as optional remote-PC play; see section 49 | Configured private route to the same home PC, with usable internet |

The phone and iPad run separate platform builds from the same Unity project, with the same gameplay rules, protocol version, stable content IDs, and compatible save schema. Phone art compression and UI layout may differ without creating a different game world. Do not serialize platform file paths, local asset handles, or native pointers as shared object identities. Send explicit game values and IDs; retain one authority for shared outcomes rather than expecting Android and iPad physics to simulate identically.

### Automatic discovery can work across the platforms

Use a small common discovery interface with native adapters: Apple Network framework/Bonjour on iPads and iPhone, Android `NsdManager`, and the existing planned Windows DNS-SD registration. Android's NSD implements DNS-SD and is specifically documented for finding services on other platforms and for multiplayer games. Each adapter locates the same declared service, then uses the same family authentication, compatibility checks, and direct gameplay protocol. [Android network service discovery](https://developer.android.com/develop/connectivity/wifi/use-nsd)

The parent pairs their installation with the family once and gets a separate player profile. With automatic connection enabled, opening the app on the same usable network discovers and joins the current family session. Opening the phone must not restart the children's world or make it take over hosting merely because it is faster. A family portrait strip can show up to four connected people; tap someone's portrait to visit their activity entrance when allowed. Everyone keeps their own camera, controls, language, assistance, and ability to leave.

**Use shared Wi-Fi as the mixed-device foundation.** Apple's proprietary peer-to-peer Wi-Fi is Apple-to-Apple. Its newer Wi-Fi Aware path is a different technology and is not available as the baseline on these iPadOS 18 devices. An Android phone having Wi-Fi Direct or Wi-Fi Aware does not make it compatible with the older Apple-only connection. Do not build the main family mode around an Apple-specific transport and discover this limitation after adding Android. [Apple Wi-Fi API overview](https://developer.apple.com/documentation/technotes/tn3111-ios-wifi-api-overview)

On a road trip, connect the devices to a tested phone hotspot and run the same LAN game protocol. Test Android alongside both iPads, not just an iPad-to-iPad connection. A phone providing the hotspot may also be a player, but validate that exact arrangement, including client capacity, peer routing, battery/heat, and behavior when the phone locks. Hotspot provider, game host, and human player are three separate roles. Do not assume enabling a hotspot automatically starts a game server. Losing the hotspot can disconnect everyone even when a different device is game host; each retains local recovery.

### Four players changes more than a connection limit

Use 1–4 human slots keyed by permanent profile ID. The dedicated PC/VPS consumes zero player slots. Reserve pending admission atomically, recognize valid reconnects and reject a fifth client without evicting anyone; solo remains available. [Unity connection approval](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/connection-approval.html).

Make parent profiles distinct from Child A and Child B. If a parent uses the Android phone today and iPhone tomorrow, that may be one parent profile with a controlled device handoff; it should not create two simultaneous copies of that person. For four simultaneous players, use four distinct active profiles. Keep avatars independent: several people can choose the same favorite character, with clear per-player markers.

**Confirmed: add Bandit and Chilli, and let every player choose any available character.** Neither age nor parent/child profile restricts the chooser. Keep human-controlled parent visuals separate from NPC parent jobs: a person choosing Bandit must not make the autonomous seeker disappear, steal that person's controls, or halt another child's quest. Recommended presentation: retain a clearly marked NPC story actor, distinct from the human player badge. If a suitable unused helper is available, that helper can lead a new invitation; an already running activity keeps its actor. Thus even four players choosing Bandit cannot remove the NPC seeker or steal a player’s controls. Switching avatars does not change who is hiding, seeking, cooking, or decorating. Use shared gameplay dimensions for fair hiding/navigation, with separate parent-size art and attachment anchors tested on small furniture. The source picture board now includes official [Bandit](https://www.bluey.tv/characters/bandit/) and [Chilli](https://www.bluey.tv/characters/chilli/) references.

| Activity/system | Adaptation for 3–4 players |
| --- | --- |
| Cooking | Offer preparation, decorating, plating, and delivery roles; enough trays/tools; reserve individual tools rather than the whole kitchen |
| Fishing | Provide up to four usable positions/rods or let others collect and observe freely; catch ownership remains one transaction per fish |
| Sandcastles, rock stacks, science, dinosaur toys | Multiple work surfaces, shared contributions, generous placement targets, and no global lock while one person holds a tool |
| Hide-and-seek | Human parent can choose seeker while children hide; default NPC seeker still works; dynamic participant list and per-hider preparation/clues; retest the roughly 30-second search target for the actual arena |
| Tag | One designated seeker and a set of joined runners; clear role changes; NPC replacement if the seeker leaves; unjoined players remain outside the game |
| Playground rides | Every shared activity supports four participants. For the existing sofa and trampoline, use four close spots within the unchanged artwork, as explicitly requested September 26. Future constrained rides need a four-player design without long queues. |
| Daycare and imagination | Up to four independent roles/locations or simultaneous lightweight sessions; duplicate useful roles and NPC fallback where necessary; no rigid two-player story script |
| Books, TV, spoken lessons | Each person's playback remains local; joining a fourth player does not interrupt the children's narration |
| Bedrooms and secret rooms | Visitors share existing rooms; owner decoration rules remain; parents receive no automatic power to erase a child's layout |
| Pictures/menu/status | A responsive list of up to four portraits; distinguish profile identity, connection state, character, and activity role |

### Four clients remain independent of the server process

A client’s hardware capability affects rendering, input and media performance; it never makes that device eligible to host. The designated PC/VPS remains authoritative until a parent-controlled server migration.

If any client leaves, the other connected clients continue on the same server. If the server is unreachable, each disconnected client continues separate solo; no successor election occurs.

On reconnection, load the server world and keep offline saves private. Generation tokens reject callbacks from old attempts; pending offline operations are never uploaded.

The server stores all logical areas. Prioritize live interaction updates and bound checkpoint transfers. A client does not need a complete world replica or every receipt before offline controls work.

## 48. Android build, phone layout, and four-player acceptance

### Keep one game project and build for each platform

Add an Android build profile to the same planned Unity 6.3 project, with the Android module/SDK/NDK tools supplied through Unity Hub when implementation begins. Unity can produce an APK and provides Build and Run for an attached Android device. Windows remains the creation/build machine for Android; the Mac remains the iOS build/signing machine. Keep platform adapters isolated so Apple-only native code is excluded from the Android build. [Unity Android build process](https://docs.unity3d.com/6000.3/Documentation/Manual/android-BuildProcess.html)

Preserve a consistent Android package identity/signing key for in-place updates and test save migration. This research does not install or update anything on the phone.

Android's network permission requirements depend on the OS and target SDK. Current Android documentation says that on Android 17, apps targeting API 37 or higher require `ACCESS_LOCAL_NETWORK` for broad direct local access; lower target SDKs have different behavior, and Android 16 introduced an opt-in test path. Plan a version-aware permission flow for the actual phone/build rather than blindly requesting every nearby-device permission. For the requested automatic discovery, use the appropriate allowed discovery path after setup; a mandatory repeated system device picker would change the desired experience. Denial leaves solo play available and should not trigger a prompt loop. [Android local-network permission guidance](https://developer.android.com/privacy-and-security/local-network-permission)

Track the network/interface associated with discovered endpoints. In particular, test a Wi-Fi hotspot without internet while the Android phone also has cellular access: discovery success on Wi-Fi is not proof a gameplay socket chose that route. Confirm local packets reach the chosen host, resolve address changes, and handle permission revocation. Discovery adapters should unregister/cancel cleanly and restart on foreground/network changes without accumulating listeners.

### Phone controls need their own layout check

Use the same landscape world with phone-aware UI scaling, safe-area padding, and camera framing. Keep the joystick away from gesture navigation, keep the item drag area clear, and avoid placing Home/Return or player portraits under a notch. Do not simply shrink every iPad control until four portraits fit. Unity's `Screen.safeArea` supplies the usable rectangle; respecting it still needs layout work and physical testing. [Unity safe area](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Screen-safeArea.html)

Each device renders its own view, so adding parents does not divide the iPad into four tiny screens. More humans do increase visible rigs, object commands, simulation, snapshots, and audio events. Keep the A10's stable 30 FPS target. Retest the earlier daycare NPC budget with four visible human avatars and with four independent zones; reduce background animation/detail before restricting meaningful play. Desktop success does not establish sustained A10 client/solo performance.

### Qualification matrix

| Test | Required evidence |
| --- | --- |
| PC + two iPads + Android parent | Three humans automatically discover, join, interact, and leave; each retains their own profile and view |
| PC + two iPads + Android + iPhone | Four simultaneous humans, correct capacity accounting, no forced common room |
| Both iPads + Android/iPhone clients | Same server protocol and item state across operating systems; four players, no mobile-host roles |
| Client interruption | Closing any phone/tablet leaves the others active; disconnected devices play private solo |
| Third/fourth arrive during cooking, hiding, books, and separate imagination stories | No restart, no taken-over avatar, current state and suitable role/arrival anchor |
| Everyone chooses Bandit, then changes characters during NPC hide-and-seek | Clear human badges and NPC actor; stable profile/role; no stolen control, revealed hiding location, or missing seeker |
| Four simultaneous grabs/placements and two admissions for the last slot | Single valid object transaction; atomic slot count; no extra fifth player or duplicate profile |
| One client leaves, then a server outage | Remaining connected players continue; disconnected clients use private solo, then rejoin server state |
| Parent receives a call, locks phone, or backgrounds app | Children connected to PC/VPS continue; no stolen input after resume |
| iPhone hotspot with Android and both iPads, then phone also playing | After remote deployment, test the route to PC/VPS, lock behavior, heat and reconnect; no local peer routing required |
| Internet unavailable but designated home PC is reachable | Mixed-device game uses its local server; otherwise each plays solo |
| App versions differ or Android permission is denied | Clear local status, full solo fallback, no corrupted session or repeated automatic retry dialog |
| Android/iPhone aspect ratios and notches | Readable four-person status, usable movement/drag controls, safe exits, local narration |
| Thirty-minute four-player stress session on A10 | Measured frame time, memory, thermal behavior, bounded snapshots, no accumulating rigs/listeners |
| Four offline players reconnect | Load current server rooms; retain each private continuation separately, without importing edits or duplicating shared props |

Build order: keep the original two-iPad interaction prototype, add **Android as the third player early**, then use the iPhone as a fourth test device before expanding all content. This exposes cross-platform assumptions before they spread through the project. The Android phone is confirmed as Galaxy S26 Ultra (SM-S948U1), and unrestricted selection from the available roster including Bandit and Chilli is confirmed. The build guide now records installed device/toolchain versions and scoped physical results.

**Current target:** four mixed clients, independent activities, automatic PC/VPS joining and full offline solo. Remote hotspot routing is later optional. The [source index](bluey-research/four-player-cross-platform-evidence.json) is historical research; current scoped physical proof is in the build guide.

## 49. Reaching your home PC from a road trip

**Priority:** optional remote-server route after reliability qualification. Hotspot/VPN/Relay research creates no gate for ordinary home-server play or art preparation. Full offline solo stays required.

**Confirmed expansion: your PC is basically always on and may host the family world while players are away from home.** Add an optional internet route to that same server, supporting 1–4 players across the iPads, Android, and iPhone. This supersedes the earlier nearby-only boundary. Full offline play remains required: a working home PC cannot compensate for a road with no usable internet.

Recommended first experiment: **a private Tailscale connection between the family devices and the Windows game server**. It gives the existing direct game protocol a private route across different networks. Tailscale has official clients for [Windows](https://tailscale.com/docs/install/windows), [iOS/iPadOS](https://tailscale.com/docs/install/ios), and [Android](https://tailscale.com/docs/install/android). This is a proposed integration, not installed networking or proof that our game already connects.

### How the family would use it

| Situation | Proposed connection | What the children experience |
| --- | --- | --- |
| Everyone at home | Home LAN to PC | Open the game and automatically join the family world |
| Road trip with usable cellular internet | Each iPad uses hotspot internet and its own private connection to PC | Same family world, even though the PC stays home |
| Parent on cellular, children at home | Parent connects privately to the same PC; children use LAN | Parent joins without moving the children or restarting their activity |
| Internet fails and no designated local PC is reachable | Each device plays private solo | Nearby devices do not create a replacement shared world |
| No usable shared connection | Each device runs private solo | Installed activities and saves remain local; reconnect uses server state without imports |

The road-trip route is: **iPad game → phone hotspot → cellular internet → private connection → home internet/router → Windows game server**. The return path carries world updates. The Android phone can also be a player. Installing a VPN on the hotspot phone alone is not our proposed setup: install and enroll the client on each game device that must reach the PC. This avoids depending on the phone forwarding tethered traffic into its own VPN. Test the hotspot provider playing at the same time.

Your PC runs the world simulation and saves. Each mobile device still renders its own picture and plays its installed voices, books, music, and video clips. This is not screen streaming; the A10 iPad still needs its own rendering and memory budget. Keep media out of routine gameplay packets, and measure actual cellular data use before a long trip.

### Why start with Tailscale, and what are the alternatives?

| Approach | Fit for this family game | Tradeoff / qualification |
| --- | --- | --- |
| **Tailscale private network — first prototype** | Reuse direct PC-server networking; parent enrolls the small fixed set of family devices | Separate client and OS VPN setup; verify automatic startup, iPad renewal coexistence, and mobile latency |
| **Unity Relay — alternative if VPN setup is too intrusive** | Game connects through Unity's public relay endpoints; no separate VPN client needed for that route | Requires service integration, private family-session rendezvous, allocation/reconnect handling, and usage accounting; PC still runs our game rules |
| Public router forwarding or a separately operated VPN gateway | Possible deployment alternatives | Adds router/ISP reachability and maintenance work; not the first implementation experiment |

Tailscale ordinarily avoids manual inbound port forwarding. It attempts direct connections and can carry traffic through an encrypted relay when the networks prevent a direct route. Relaying can increase latency; working through a relay is not the same as a guaranteed low-latency connection. [Firewall guidance](https://tailscale.com/docs/reference/faq/firewall-ports), [connection types](https://tailscale.com/docs/reference/connection-types)

As checked September 23, 2026, Tailscale's Personal plan is **$0**, with up to six users and unlimited user devices, which fits the proposed family setup. Tailscale account/device enrollment is separate from the game's four player profiles. No paid game-server rental is needed for this proposed home-PC route; home internet, mobile data, electricity, and service availability still matter. [Current Tailscale pricing](https://tailscale.com/pricing)

Unity describes Relay primarily for a host/listen-server pattern. A PC-as-host Relay experiment must verify the intended headless process, service authentication, idle allocation lifetime, and four-client capacity; it is not a dedicated hosting service or an automatic replacement for our server. Automatic joining would also require a private way to find the family's current allocation, rather than asking children to enter a join code every time. Unity currently includes free Relay usage with paid overages, so estimate actual traffic rather than promising unlimited free service. [Relay operation](https://docs.unity.com/relay/relay-servers), [UGS pricing](https://unity.com/products/gaming-services/pricing)

Relay's fair-use guidance permits game-state traffic, but excludes audio/video streaming and general file-transfer use. Keep our existing locally installed TV/voice design whichever route we select. [Unity Relay fair usage](https://docs.unity.com/en-us/mps-sdk/networking/fair-usage)

### Automatic connection needs a saved server address

During parent setup, enroll each device and save the family's server identity, private hostname/address, gameplay port, and pairing identity in the app's parent configuration. MagicDNS provides names for Tailscale devices; use the PC's actual assigned name, not a hard-coded example copied from this report. [MagicDNS](https://tailscale.com/docs/features/magicdns)

Do not depend on the earlier local Bonjour browser finding the home PC across the internet. Add a direct remote-endpoint resolver alongside LAN discovery. Resolve the private hostname to a supported address, then attempt the same authenticated game handshake, checking world ID, protocol/content versions, player identity, and capacity. A VPN icon or successful ping alone is insufficient evidence that the game server is ready.

Resolve routes to the same designated server identity, with bounded attempts outside the gameplay path. Use one authenticated connection. Switching a network route is not a change of world authority; changing PC to VPS is a parent-controlled migration.

The game searches/retries while foregrounded. The supporting VPN has its own OS lifecycle, which is separate from game discovery. On iPad/iPhone, Tailscale offers VPN On Demand rules; test a configuration for the hotspot Wi-Fi as well as cellular on the phone, including reboot and returning from background. The game cannot silently grant itself the OS's first VPN permission. Android startup/battery behavior must be tested on the Galaxy S26 Ultra; do not assume Apple's On Demand feature applies to Android. [Tailscale On Demand](https://tailscale.com/docs/features/client/ios-vpn-on-demand)

### Check the interaction with free iPad signing renewal

The agreed free renewal workflow remains required. SideStore's current installation guide uses **LocalDevVPN**. Tailscale documents that connecting another VPN can disable its On Demand behavior until Tailscale is manually connected again. Together, these sources identify a concrete integration risk; they do not establish which automation will work on these exact iPadOS 18 installations. [SideStore installation](https://docs.sidestore.io/docs/installation/install), [Tailscale other-VPN limitation](https://tailscale.com/docs/features/client/ios-vpn-on-demand)

Include a real end-to-end test: play remotely, complete an in-place renewal, restore the game route, then reopen the game without lost saves. If a parent-assisted switch or verified automation is needed, put renewal outside a play session. If this cannot meet the intended easy daily experience, evaluate the Relay route before committing to Tailscale for the finished game. Merely reaching the home PC over a VPN is not proof that Sideloadly's device discovery/signing works remotely either. Do not drop free renewal or offline play to make remote networking easier.

### Make the Windows host dependable

Run a built server process independently of the Unity Editor. Plan startup after a PC reboot, restart after a crash, durable checkpoints/event journal, rolling backups, and a parent-readable status showing process health, last save, connected profiles, and route errors. Avoid using a graphical desktop login as a hidden requirement. Tailscale's Windows **Run Unattended** setting supports its network service after logout; the Unity server still needs its own startup/recovery configuration. [Windows unattended mode](https://tailscale.com/docs/how-to/run-unattended)

At deployment, verify the PC remains awake during family play, its home connection works, and server updates retain the existing world. Do not assume “PC powered on” means the game service is healthy. Empty-server behavior should pause child-facing activity deadlines and NPC quest progression; nobody loses a hide-and-seek round because the world ran overnight.

Configure private-network access to the needed game endpoint, with Windows firewall scope and application pairing matching the family setup. Tailscale grants can restrict protocols/ports; review existing broad rules because grants are additive. An exit node or access to the whole home subnet is unnecessary for reaching this one PC. This is deployment design; no accounts, VPN profiles, firewall rules, or router settings were changed. [Tailscale grants syntax](https://tailscale.com/docs/reference/syntax/grants)

### Losing the road connection is a partition, not proof the PC stopped

If one iPad loses internet while others remain connected, the PC continues their shared session. The disconnected device stops issuing shared commands and continues from its last complete checkpoint in a new local branch. If both traveling iPads remain reachable locally, they may form a shared travel branch with one authority; otherwise each continues alone. Do not announce a global host replacement or revoke the reachable PC's authority because one client cannot see it.

On reconnection, join the server’s current world. Keep local work separately; never submit private branch operations or replace the server save with a travel save. A missing route cannot carry other players’ live actions.

### Build order and acceptance

1. Prove the four-player local PC server and recoverable saves first.
2. Add the saved remote endpoint and test a small toy scene through Tailscale from genuinely different networks.
3. Test automatic startup, routes, cellular interruptions, and signing renewal together.
4. Keep Tailscale if that experience qualifies; otherwise prototype Relay with private family rendezvous. Continue using the same world logic and offline branch model.

| Test | Required result |
| --- | --- |
| Two iPads on hotspot, Android and iPhone joining remotely | Up to four players reach one PC world without child-facing addresses/codes |
| One parent remote, children on home LAN | Same server/world identity, independent views, no duplicate profile or extra slot |
| Direct route unavailable; Tailscale relay path used | Measure latency and playability; graceful fallback if unacceptable |
| Weak signal and repeated handovers | Test simulated 100/250/500 ms round trips, jitter, packet loss, and 10–30 second outages; these are test conditions, not promised performance |
| One child disconnected while the other still reaches PC | Connected child continues; local branch preserves disconnected child's work |
| Both lose their route to the designated server | Each continues solo; a reachable enrolled home PC can still serve its designated local world without internet |
| Windows logout, reboot, server crash, or home internet loss | Correct startup/recovery; durable world retained; clients never wait indefinitely |
| Renewal helper, another VPN, denied VPN setup, expired enrollment | Clear parent diagnosis, usable local fallback, no repeated child prompts; renewal and reconnect proven together |
| Returning from offline cooking/bedroom edits | No duplicate rewards, lost room, stolen held item, or whole-save overwrite |
| Thirty-minute real four-player travel test | Record cellular bytes, latency/loss, A10 frame time, phone heat/battery, and reconnect outcomes |

**Status:** optional remote home-PC hosting is now part of the plan. Tailscale is the first proposed connection experiment, with Relay as an alternative; neither is installed or configured by this research. The [remote home-server evidence index](bluey-research/remote-home-server-evidence.json) records the sources and unresolved device tests.

## 50. One shared world, independent travel, and shared items

**Confirmed: all 1–4 players share one family world while connected, and every player chooses where to go independently.** They can be in different locations or together in the same location. Nobody must follow a leader, stay near a sibling, finish a quest, or wait for another player to move. This is a requirement for the finished game, including the creek, playground, home rooms, bedrooms, secret rooms, and imagination destinations.

### Your creek and playground example, step by step

| Action | Required result |
| --- | --- |
| Player 1 plays at the creek; Player 2 plays at the playground | Both places remain active. Each person has their own camera, controls, audio, and activity |
| Player 1 chooses Playground | Only Player 1 transitions; Player 2 keeps playing without a loading screen or pause |
| Player 1 arrives | Both see one another when their characters are within the visible view. Player 1 sees the playground's existing toys, placements, and activity state |
| Player 1 picks up the playground's only bucket | Both see that same bucket move into Player 1's drag/carry position; its old spot is empty |
| Player 2 tries to pick up that held bucket | No theft or duplicate. A small visual cue shows it is in use; Player 2 remains free to interact with other things |
| Player 1 drops the bucket | Its new location and contents appear on both devices; Player 2 can now pick it up |
| Player 1 leaves again | Player 2 continues in the same playground with no reset, forced move, or interrupted controls |

Entering a location joins the **existing shared location**, not a fresh private copy. Being in the same area does not require identical camera framing; ordinary furniture occlusion and intentional hide-and-seek concealment still apply. A visit arrives at a safe entrance, not on top of the other avatar or inside their hiding place. The location bubble browser always supports independent travel; a friend portrait is an optional shortcut, never a required invitation or permission step.

All four players, including parents, may enter any of the four personal bedrooms or four created secret rooms. Each has their own profile-owned rooms; decoration ownership protects permanent edits without locking visitors out. For imagination stories, a visit targets the actual running story instance. “Visit their Space game” must not secretly create a different spaceship. A deliberate new story can have a distinct instance, with a clear picture-based choice when more than one is running. Leaving an optional bounded mini-game ends only that player's participation; it does not restrict travel.

### Keep world state separate from loaded artwork

Use one authoritative world model with stable `WorldId`, `ZoneId`, optional activity-instance ID, player profile IDs, and persistent prop IDs. Each player's current zone is separate from the host's viewed zone. The server owns the logical state of occupied areas and keeps saved records for empty ones. “This iPad stopped displaying the creek” must never mean “delete the creek.”

Keep a persistent logical world on the server and load each client’s room illustration/effects as presentation. Props have logical zone IDs; one client unloading art never removes authoritative entities or another player’s activity.

Unity's integrated network scene manager synchronizes server scene events with clients. A global scene replacement for each player's travel would conflict with this requirement. Custom scene handling requires explicit synchronization and readiness handling; Unity also documents limitations for in-scene NetworkObjects with custom loading. Use registered network prefabs and a consistent persistent network scene, with presentation scenes containing no independently spawned duplicate gameplay props. This is our proposed architecture to prototype, not a built-in “independent worlds” checkbox. [Unity scene events](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/using-networkscenemanager.html), [custom scene management](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/custom-management.html)

Occupied areas continue their activity simulation. Empty areas may reduce or suspend unnecessary work while retaining their authoritative records; essential shared outcomes must be explicit. Reentering an empty area restores its authoritative saved state, including any valid item returns committed under section 51, instead of running a fresh-room item spawner. Rendering fewer areas controls device memory; it does not limit where other people may play.

### Transition only the traveler

1. Request a target zone/instance using the player's stable identity. Validate content and a legal entrance without asking everyone else to travel.
2. Load that player's destination presentation asynchronously. Keep everyone else's input, narration, timers, and scene untouched. If loading fails or is cancelled, retain the original location.
3. Prepare the current destination state at revision R, with changes after R queued or streamed. The traveler must see current holds and creations even if they happened during loading.
4. After destination readiness, commit that player's zone and any carried portable prop together. Update observer membership for the old and new zones; activate the arrival only when dependencies are ready.
5. Remove the old local view when safe. A local object becoming hidden or being unloaded is not a request to delete the shared object, reset its activity, or release another person's hold.

Use transition IDs so a rapid Creek → Playground → Home sequence ignores obsolete loading callbacks. Do not give the avatar two active zone memberships or leave a duplicate at its old entrance. Leaving while the host itself travels uses the same path; server ticking must continue during asset loading. A short fade/loading indicator may appear on the traveler's device only. “No interruption to the remaining player” is an acceptance target to measure, including frame-time spikes on the A10.

### Send each player the relevant state

Send a lightweight presence list globally and detailed props/avatars for the client’s current/entering area. Server backups cover every area. Complete client replicas are optional recovery evidence, not a requirement for local input.

NGO visibility hides client representations without erasing server records. Update subscriptions on travel and keep avatar/held-item dependencies coherent. Presentation cleanup must never mutate server authority. [Object visibility](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/object-visibility.html).

Keep lasting facts as synchronized current state: holder, zone, position, water amount, planted state, and completed construction. A pickup RPC or animation alone cannot explain a bucket to someone who arrived afterward. Unity synchronizes NetworkVariable values to newly joined clients and exposes change callbacks; initialize the visual from the current value and then observe changes. Our multi-object transition/snapshot consistency still needs its own revision rules. [Unity NetworkVariables](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/networkvariable.html)

### One bucket means one shared instance

Represent each movable prop with a stable instance ID, zone/placement, optional holder profile ID, contents, and revision. Maintain the invariant **zero or one holder per prop**. A holder is a temporary gameplay reservation; it need not be NGO client ownership. Prefer server-owned world props so the bucket is not destroyed with a player's network avatar. Unity documents server ownership and lifetime options, including `DontDestroyWithOwner` for client-owned objects; that flag alone does not implement pickup arbitration or recovery. [Unity ownership and lifetime](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/components/core/networkobject-ownership.html)

Proposed pickup protocol:

- The client sends a pickup request with prop ID, expected revision, and unique operation ID. The server derives the player from the authenticated connection and verifies that the prop is interactable in their zone/mode. Simple Play must not acquire a new precise-avatar-position requirement.
- The authority processes competing requests in one ordered step. It grants one hold, increments the revision, and replicates the holder/state. The losing request receives the current state; do not silently create another bucket.
- A local highlight or tentative drag preview can respond immediately. Only the accepted holder can commit movement or use. Nearby players see the accepted motion interpolated; normal network delay must not produce two authoritative holders.
- Movement, filling, pouring, and dropping include the valid reservation/generation. Release the hold as part of committing the final placement. Duplicate or delayed requests must not repeat water transfer or drop a bucket already picked up by someone else.
- If the finger releases before pickup approval arrives, correlate that release with the request and resolve/cancel it. Do not leave a late-approved invisible hold. On disconnection, the authority releases abandoned holds after bounded failure detection and places the prop at its latest valid safe position.

While Player 1 drags the bucket, Player 2 may move a ball, cook, or leave. Reserve the particular item or short interaction transaction, not the whole screen, room, or kitchen. Pouring changes the bucket and target together so water is subtracted once and added once. No automatic respawn of the “only bucket” just because it is held or the camera changes.

**Portable items follow their category policy (refined in section 51).** Personal toys and borrowable spare props may travel; essential station tools stay within their allowed play region and settle on a return rack if the player travels farther. The following bucket example uses a borrowable spare. On a successful area transition, the same bucket ID and its contents move to the destination; it disappears from the old area and appears held in the new one. Source players continue normally but no longer have that particular borrowed bucket nearby; section 51 preserves the station’s essential local working kit. A failed transition retains the original state. Fixed equipment stays in place; an uncommitted touch drag is safely settled before travel. A carried prop is not deleted on character swap, activity exit, or owner disconnect. A disconnect places it safely in the last committed zone and clears the temporary hold; restarting a server clears stale holds without replacing saved props with fresh copies.

Single-holder rules apply within one authority. Private solo copies do not share live holds, and are never imported. Normal area travel changes a subscription, not authority or another player’s world.

### Required proof before expanding all six locations

Build a two-zone test with a creek, playground, one portable bucket, one water source, one plant, and up to four avatars. This adds a required early milestone after the first shared-object scene, before producing the full content catalog.

| Acceptance test | Pass condition |
| --- | --- |
| Two players start in creek/playground, then meet | Existing playground is shared; both avatars and current object state appear; no duplicate instance |
| One leaves while the other is dragging, cooking, riding, or listening | Remaining input, activity progress, sound, and scene continue; no global pause or reset |
| Arrive after a bucket was picked up and partly filled | See correct holder and contents, empty original spot, and unavailable pickup |
| Two or four players grab the only bucket together | Exactly one accepted holder; others keep control of their own avatars and props |
| Drop, then another player picks up; delay/replay earlier packets | One final placement/holder; stale move/drop cannot overwrite the newer hold |
| Carrier moves the bucket creek → playground, then disconnects | Exactly one instance at the correct destination, contents retained, hold safely released |
| Last player leaves and later returns | Personal/creative state persists; eligible shared loans may have returned under section 51; no whole-room reset or duplicated starter bucket |
| Rapid travel or cancelled/failed load | One valid player position; no phantom avatar or lost carried item |
| All four players occupy different areas, then gather | All areas function; entering friends' rooms/stories joins the correct existing instance |
| Any client changes area while siblings stay elsewhere | Server simulation and siblings continue; no despawn or timer interruption |
| Repeat on A10, Android/iOS mix, and remote-PC route | Measure arrival time, frame-time spikes, bandwidth, and holding consistency; ordinary travel remains independent |

**Status:** these are explicit design and acceptance requirements, not implemented gameplay. The [independent-travel and shared-item evidence index](bluey-research/shared-world-evidence.json) records the Unity sources; document checks do not substitute for the physical-device tests above.

## 51. Automatic item returns, stocked areas, and tidy bedrooms

**Confirmed preference: borrowed shared items return home automatically after they have been unused for a while. Personal decorations and saved creations are protected.** The purpose is to keep the house and activities usable, without turning cleanup into a compulsory chore or allowing one child to stockpile every shared prop in a bedroom. Returning an individual eligible item is different from resetting a room or the whole world.

This refines section 50: one item still has at most one holder, but a borrowed item is not necessarily left wherever it was dropped forever. Players keep their freedom to travel; an item's travel/return policy depends on its purpose. The detailed limits below are proposed defaults to test with both children, not claims that Toca Boca uses these rules.

### What the research supports

Toca Boca's official reset guidance says rebuilding removes the house/furniture and an app reset clears progress while retaining specified categories. That makes a broad reset unsuitable as our everyday housekeeping tool. Its current Lost & Found provides recovery and distinguishes items already in the world from items available to retrieve. Container recovery follows special rules so returning a box does not also reproduce everything previously removed from it. These are useful precedents for category-specific recovery, a simple picture inventory, and tracking a prop's identity through containers. They do not establish an automatic inactivity timer. [Toca reset guidance](https://tocaboca.helpshift.com/hc/en/3-toca-boca-world/faq/68-what-happens-if-i-reset-my-world-or-rebuild-my-home-1616575224/), [Lost & Found](https://tocaboca.helpshift.com/hc/en/3-toca-boca-world/faq/382-post-office-upgrade-the-all-new-lost-found-display-1780413382/), [gift/container recovery rules](https://tocaboca.helpshift.com/hc/en/3-toca-boca-world/faq/406-where-are-my-gifts/)

Toca also acknowledges unwanted bunched furniture during stability recovery. Our design lesson is to prevent excessive live clutter and retain recoverable item records, rather than treating a heap in the middle of the bedroom as successful recovery. That is an inference for our game, not a claim about Toca's internal algorithms. [Official stability note](https://tocaboca.helpshift.com/hc/en/3-toca-boca-world/faq/317-bunched-items-in-hd-homes-stability-improvements/)

### Different objects need different rules

| Item category | Examples | Proposed rule |
| --- | --- | --- |
| Shared fixed furnishings | Kitchen counters, oven, house sofa, playground slide, fountain, doors | Stay in their authored area; still interactive. Personal room furniture comes from its own decorating catalog |
| Essential station tools | The garden's working watering kit, kitchen tools, science instruments, fishing rods | Move around their allowed play region; return to a nearby rack if carried out of it. Player travel continues. Provide enough explicitly separate tools for joined participants |
| Borrowable shared props | Spare ball, picnic cup, dress-up prop, spare toy bucket | May travel; one real instance, limited loans, automatic return after inactivity |
| Personal possessions/decorations | Chosen plush toys, bedroom rug, own dinosaur collection | Persist or go into that child's toy-box storage. No automatic return to a communal source |
| Creations and collections | Decorated cake, painting, sandcastle arrangement, selected shell collection | Save the result; store/recover it by picture when no longer displayed. Never erase it because a cleanup timer expired |
| Replenishable supplies | Pantry ingredient portions, craft paper, moulding sand | Dispense controlled batches with unique IDs; bounded working trays and creation storage prevent unlimited loose output |
| Disposable effects/mess | Bubbles, splashes, authored leaf litter, temporary crumbs | Fade/clean according to activity rules after use; do not mistake a child's art or collection for litter |

Mark borrowable items with a small home/basket symbol in the relevant interface and use a short spoken cue when returning one. There should be no reading requirement, punishment, lost reward, or “you made too much mess” message. The three-year-old can keep choosing dinosaurs and playing; tidying is optional.

### Keep useful equipment available even before the timer expires

An inactivity timer alone cannot prevent hoarding: a child can keep handling items, pack them into a bag, or repeatedly take another one. Use three complementary controls:

1. **Protect the essential working kit.** Core equipment belongs to its station's allowed region, which can include connected spaces such as the home and backyard where useful. A transition sets an out-of-region tool safely on its return rack; it never blocks the player at a door. Spare borrowable/personal versions support travel play. Within the area, reserve at most one essential tool of each type per player and provide up to four distinct working positions/tools where the activity needs them. A lone child cannot reserve all four rods in a backpack. If the only bucket is actively held, others still cannot take that same bucket.
2. **Bound shared loans.** Start testing with four loose borrowed props per player across the whole world, at most one of each essential loan type. Count nested contents and abandoned loans until returned; dropping items or switching avatars must not free the borrow allowance. If full, offer a picture choice to return an unused loan for the new one; do not interrupt the sibling or force cleanup before the player can travel. The exact count is tunable. Personal toys are not these loans.
3. **Separate bedroom supply from shared-room supply.** Each child chooses personal toys/furniture from an illustrated catalog. Choosing a personal dinosaur does not remove the communal dinosaur, and choosing a bed does not remove furniture from the Heeler home. These are deliberate separate items with different IDs, not accidental duplicate copies of a currently held object. Keep all planned dinosaur types accessible through the toy box.

Define a minimum local working set per station and audit it through the authoritative item registry. Account for objects held by current participants: “not on its shelf” is not “missing.” Repair a truly missing record only through a validated recovery transaction. Do not respawn a new bucket every time someone takes the old one or leaves the camera view. Shared areas remain usable because their working kit stays local, not because an unlimited source feeds the bedroom pile.

### What counts as unused, and when should an item return?

**Initial experiment: three minutes of eligible inactivity, followed by a roughly five-second basket/sparkle cue if visible, then return.** These durations are design starting points; tune them from play observations. The automatic return must also work while another player remains in that room, as you selected. A short warning is local to relevant players and must not pause their game.

Do not return a prop while any player holds/drags it, while it is being poured/used, while a transfer is pending, or while it is an actual dependency of a running recipe/ride/hiding setup. A child arriving or picking it up during the cue cancels the pending return. The server rechecks eligibility at commit. Looking through a book, standing in the room, camera movement, or an unrelated touch does not by itself renew every prop in the room.

Activity protection has a defined reason and lifecycle: completion, participant departure, or parking the activity releases borrowed tools after a safe checkpoint. Save the activity's creative result separately. A forgotten activity record or a “favorite” star must not reserve every kitchen tool forever. Parents' background NPC routines do not continually touch abandoned items just to keep their loans alive.

The inactivity clock belongs to the authority's world simulation, not each iPad's wall clock. Preserve eligible elapsed time across host recovery. When the whole family is away, pause creative activity progress and retain return eligibility; on resume, process returns in bounded batches with the normal visible cue. Do not wipe rooms or replay thousands of old cleanup animations because the PC ran overnight. A departing child does not reset a timer another child actively refreshed.

Example: a child borrows a spare garden bucket, leaves it unused in their bedroom, and plays with dinosaurs. After the configured grace and cue, **that same bucket** returns to its garden rack on every connected screen. Their bed, selected plush toys, dinosaur arrangement, and saved cake stay. If another player picks up the bucket before the return commits, they keep using it and the return is cancelled.

**September 25 item–item clarification:** the user explicitly wants shed storage and other object combinations. Provide usable shed shelves/hooks/bins, baskets, cupboards/drawers and supported surfaces. Open → place → close → reopen → retrieve must preserve the same item, contents and decorations, including after travel/restart; shared holders and private offline saves retain their existing rules. The [full container/support specification](bluey-lets-play-reference-study-2026-09-25.html#19-itemitem-interactions-containers-and-shed-storage) defines nested capacity, safe rejection, stacking and return behavior. The supplied shed picture establishes its visual target; its exact commercial-game storage semantics remain unverified.

### Containers and creations must not become loopholes or casualties

Track a container and each contained item separately. Bags, drawers, baskets, and boxes do not change ownership or convert shared loans into personal property. Enforce cycle/depth checks and count all borrowed descendants. Putting twenty tools inside one box cannot make them count as one loan; moving the bag alone does not reset every child's abandoned-item timer inside it.

An automatic return must not move a container currently held by somebody or remove an item they are actively manipulating. Use the loan cap and station-tool region rules to prevent such protection from allowing unlimited stockpiling. Return eligible contents separately where safe, leaving the personal bag where it belongs. Do not return a bag and reconstruct its original contents if those items already exist elsewhere.

Before returning a shared plate, bowl, or bucket, distinguish disposable material from a saved creation. Ordinary leftover water can be drained as a return action; it does not water a plant or count as a player quest action. A decorated cake or collected shells must first move to an available personal tray/storage record with the same identity and ownership. If safe separation cannot be completed, defer the return and retain the creation. Use bounded trays so this does not create an ever-growing physical pile.

Protected constructions need provenance: store the created arrangement and the personal/generated pieces it owns. Borrowed shared instruments are not permanently promoted into that saved creation. For buildable keepsakes, provide personal building pieces or save an explicitly reconstructed personal version, then release borrowed supports when nobody is using them. No automatic dismantling of an active fort around a child.

### Keep bedrooms pleasant without deleting their choices

Use physical shelf/table/bed placement slots, open floor paths, and a toy chest with picture browsing. Store toys as lightweight records when put away; they do not all need an active collider and animated rig. Test an initial budget around **32 loose interactive props per bedroom**, plus authored furniture/decoration slots. This permits a substantial dinosaur display; it is not a measured A10 maximum, and large/complex objects need a higher weight than a small shell.

When a room is full, offer a simple “put one away, take one out” picture interaction for personal toys. Keep the chosen placement preview in hand or return it to its own storage; do not delete the oldest toy or block the exit. Borrowed clutter follows the confirmed automatic return rule. Saved creations can live in a separate picture gallery/shelf and be restored deliberately, preserving their data when not physically displayed. Never silently purge creations because the storage list is long; monitor save size and expose parent storage management if needed.

Add a large optional **Help tidy** basket: return eligible unused loans and put the room owner's loose personal toys into their chest. Leave installed decorations and saved displays in place. It cannot grab held objects, affect another room, or erase a visitor's creation. Visitors may return their own loans; changing the owner's personal arrangement follows the existing Decorate together permission. A separate parent “restore shared props” action targets eligible communal stock only, with a recoverable preview/undo record; it is not a whole-game reset.

### Unity implementation and multiplayer consistency

Author an `ItemPolicy` definition per prop type: category, home region/anchor, allowed travel, idle-return duration, supply group/cap, footprint weight, contents policy, and protection rules. ScriptableObjects are appropriate definition assets; runtime item instances and saves remain separate records. [Unity ScriptableObjects](https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html)

Each runtime item needs its persistent ID, policy ID, current zone/container/holder, original supply identity, borrower profile, last meaningful-use time, protection reason, contents, revision, and pending-return generation. The proposed state flow is **available → held/in use → placed → eligible idle → return pending → home/storage**. Item ownership, room decoration rights, temporary pickup reservations, and return policy are separate concepts. Keep server-owned props under the same authority for grabs, returns, and repairs. [Unity NetworkObject ownership](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/components/core/networkobject-ownership.html)

When a return is due, validate the expected revision and current eligibility, reserve the destination, safely separate protected contents, commit the relocation and inventory/loan changes together, persist the operation, and replicate it. The operation ID makes retries idempotent. If an item is picked up during return preparation, the stale return fails its revision/hold check. Defer if its home rack is temporarily blocked, or use a bounded return-storage slot; never put it inside a wall or on the sibling's artwork.

Pooling can reuse visual/effect objects to reduce allocation churn, but it is not a game inventory or anti-hoarding system. Unity's pool creates an instance if none is available; its maximum retained size therefore does not cap all live items. Enforce gameplay supply and room budgets separately. Clear old listeners, references, contents visuals, and timers when reusing a view; an old timer must not return a new toy that reused that object. [Unity ObjectPool](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Pool.ObjectPool_1.html)

Offline play applies the same return policies within its private save. Rejoining loads the server’s item identities, contents and timers; private returns or creations are never imported. Keep local creations recoverable separately, and log committed return reasons/revisions for diagnosis.

### Acceptance before release

| Test | Required result |
| --- | --- |
| Repeatedly carry shared toys into one bedroom | Loan/room budgets hold; unused loans return; shared fixed furnishings and essential kits remain available |
| Put borrowed items in nested bags/drawers | Contents remain individually tracked; no cap bypass, duplicate contents, or lost personal container |
| Return becomes due while a second child picks up the bucket | Exactly one outcome; accepted pickup cancels stale return |
| Child stays in room but ignores a borrowed prop | Idle return works without room exit, global pause, or unrelated objects moving |
| Child is pouring, cooking, building, or hiding | Required active dependencies stay; safe cleanup occurs after release/checkpoint |
| Cake sits on borrowed plate; shells sit in shared bucket | Creation preserved in valid personal storage/placement before tool returns |
| One child leaves while sibling is using their borrowed ball | Current use wins; no reset caused by borrower logout |
| All four players use station tools; one attempts to reserve every spare | One holder per item, per-player stock rules, all activity roles remain usable |
| Room budget reached with all dinosaur types accessible | Picture storage remains usable, exits clear, nothing silently deleted |
| Lost packet, duplicate timer, server crash, and pooled-view reuse | One committed return; no stale cleanup of a different object |
| Two offline players borrow/return the same stock item | Private copies stay local; reconnect loads the single server-owned stock item without importing or duplicating copies |
| Long A10 session repeatedly spawning supplies and tidying | Bounded live objects, memory, listeners, save growth, and frame-time spikes; no growing hallway/room heap |

**Decision/status:** automatic return after inactivity is confirmed; three-minute timing, five-second cue, four-loan allowance, and bedroom budget are proposed tuning values. Item-category rules, safe return transactions, protected storage, and the tests above are researched plans, not implemented features. Sources are saved in the [item-return evidence index](bluey-research/item-return-evidence.json).

## 52. Feasibility audit and current priorities

The game remains feasible as a staged custom Unity project. The latest architecture is simpler: one PC/VPS authority, four clients and independent offline solo. [Current decisions](current-decisions.md) supersede the old mobile-host and merge proposals. The [feasibility report](family-playset-feasibility-audit-2026-09-23.html) and [build guide](family-playset-build-guide-2026-09-23.html) now use the same scope.

Required: four mixed clients, independent locations, interactive shared props, free joining/leaving, automatic connection, full installed solo activities, protected saves and personal rooms. Optional later: remote multiplayer while traveling and AR. Removed: device hosting, election/switching, router-free co-op, Bluetooth and automatic offline merging.

Finish connection/offline qualification and deployment checks within their scope. Isolated character animation can advance meanwhile. Establish G5 room/item contracts, complete one polished home/backyard slice, then expand the six-world catalog. English comes first, reviewed Spanish later. G4/AUTO-02 are retired, not passed gates.

The A10 iPad still needs measured client/solo performance, memory, media and sustained play acceptance. Content production remains substantial: character art, state animations, reviewed speech, activity design and child usability. No template supplies the whole combined game. Scoped proofs already exist; use the build guide for current versions rather than research-date status claims.

## 53. Source-backed networking and recovery review

The [technical review](family-playset-technical-research-2026-09-23.html) retains the primary-source findings that support the dedicated-server design: native discovery, state outside network objects, independent area subscriptions, server-validated item operations and bounded client rendering.

The September 25 scope correction removes the old mobile-authority and offline-merge application conclusions. Research about a possible technology is not a requirement to implement it. Bluetooth, router-free peer networking and device hosting are outside scope.

Open the app to join the enrolled PC/VPS automatically; a second through fourth player joins the same world. A client's departure never changes authority. If disconnected, continue private solo; rejoin uses current server data. Test source-backed mechanisms on actual target devices before claiming their behavior or performance. The new architecture audit is a requirements/source consistency review, not a fresh external-source revalidation or a new runtime pass.

See the [implementation ledger](family-playset-build-guide-2026-09-23.html#19-implementation-audit-and-remaining-work) and [audit record](implementation/pc-vps-plan-audit-2026-09-25.html) for completed proof, retired requirements and remaining work.

## 54. Templates, packages and the free starting setup

**Budget confirmed:** free first; consider an optional package around $20 only if it saves substantial work. The following table records research candidates. Installed versions are recorded in [current decisions](current-decisions.md) and the manifest/lockfile; no package change is implied by this audit.

**Recommendation:** create this game from Unity's **Universal 2D template**, then reuse focused packages and build the particular playset rules. The template configures URP's 2D renderer. It is a suitable foundation, while the quests, interactive toys and shared family world remain our game code. [Unity's template documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/creating-a-new-project-with-urp.html)

The [full package and template report](family-playset-package-research-2026-09-23.html) contains the version matrix, evidence, device qualifications, costs and installation order. Its [editable source](family-playset-package-research-2026-09-23.md) stays in this project's docs.

| Need | Recommended reuse | Decision |
| --- | --- | --- |
| Touch, joystick, tap walking and dragging | Input System 1.20.0 plus our finger/gesture rules | Initial setup |
| Menus, stage circles and picture prompts | Editor-compatible uGUI/TextMeshPro | Initial setup |
| Animated illustrated characters | 2D Animation 13.0.6; PSD Importer 12.0.2 if the art workflow needs it | First character slice |
| English voices, later Spanish, room/content loading | Localization 1.5.13 + Addressables 2.10.3 candidate combination | Early content pipeline; local assets for offline play |
| Toy movement and playful feedback | DOTween Free 1.3.030 | Add for first-room polish |
| Four-device networking | NGO + Unity Transport; exact set must pass qualification | Early prototype; custom automatic joining/recovery still required |
| Faster multiplayer iteration | Multiplayer Play Mode 2.0.2 + Multiplayer Tools 2.2.12 | Development aids; physical-device tests still required |
| Optional later tools | Cinemachine, Yarn Spinner, selected UI Extensions controls; NavMeshPlus pilot | Add only for a demonstrated need |

The linked report cites the Unity 6000.3 catalog for each official version. **New qualification finding:** editor 6000.3.24f1 release notes list NGO 2.13.2, whereas the later 2.13.3 catalog status conflicts with its published release/registry labeling. Keep the networking framework candidate, resolve the version/status difference in the exact editor, then test and lock the dependencies. The prior technical source findings do not certify a production package set.

Unity Playground and Boss Room remain useful source references. TopDown Engine's included four-player demo is local to one application; Adventure Creator needs custom co-op integration and adaptation of global cutscene behavior. None is a verified ready-made replacement for the full family playset. The comparison and primary sources are in the detailed report.

**Only paid shortlist item:** DOTween Pro, approximately **$15 USD regular price before tax**; a $7.50 sale was visible during research. Its visual authoring tools may help when we have many repetitive animations, but start with Free. [Publisher listing](https://marketplace.unity.com/packages/tools/visual-scripting/dotween-pro-32416)

This setup preserves four mixed clients, independent areas, shared props, automatic PC/VPS joining and full offline solo. Device hosting and offline merging are removed. Consult the [build/device ledger](family-playset-build-guide-2026-09-23.html#device-and-toolchain-ledger) for actual installed versions and measured qualification, rather than the research-date candidate table.

## 55. Future hosting on your owned VPS

**User decision:** once the server is ready, plan to run it on the VPS you already own. This removes dependence on the home PC for the normal shared world. The apps should automatically connect to that enrolled family server from home internet or a road-trip hotspot, allowing up to four iPad/iPhone/Android players with the same independent movement, shared objects and saved progress.

Travel multiplayer needs an internet route to the designated server. Without one, full independent offline solo remains available. Device hosting and offline imports are removed. VPS deployment remains planned after reliability qualification, without blocking isolated art/content preparation.

The [VPS hosting plan](implementation/vps-hosting-plan-2026-09-24.html) records the code review, primary sources, migration gate and acceptance checks. Core game authority rules can be reused. The apps need remote endpoint discovery/configuration; a Linux VPS needs its own server build/platform adapters; the current Windows-protected credentials require controlled migration. Preserve the existing family world and identities, and stop the PC authority before the VPS accepts canonical writes.

**Sequence:** qualify reliability/save recovery → inventory VPS → build an isolated remote proof → test home/hotspot and offline failure → migrate during an agreed no-player window. Keep the current PC server working during preparation. The user confirms the VPS has suitable capacity and will share OS/connection details later; that does not block current work. No remote login, upload or configuration was performed. Earlier home-PC routing proposals are retained as alternatives rather than the selected destination.
