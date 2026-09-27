# Downstairs science and coloring: research applied to the Home design

**Later presentation research:** [Actual game visuals and eight project/play designs](home-science-play-design-2026-09-27.html) address the user's request for more engaging science. This first report covered scientific/system foundations more deeply than presentation; the follow-up adds that missing design evidence without claiming a new implementation.

**Later implementation:** [Windows candidate 174](home-discovery-2026-09-27.html) applies this research to the actual connected house, three science trays and six persistent tap-fill pages. The browser prototype and its evidence below remain a separate historical research milestone. Blank drawing, gallery/carrying/Together and five science stations remain open.

September 27, 2026 · LAB-01 / H-29–30 / ACT-01 / ITEM-03 · development branch `codex/home-science-coloring`.

**Latest request:** add a science area and coloring-book area in the main house's general shared space, and actually use the research. Both belong downstairs, accessible to all four family players. The existing eight science activities remain in scope. Coloring is an additional creative activity, not a replacement for the six narrated books.

**What this pass delivers:** a researched room and interaction specification, an interactive [research prototype](home-science-coloring-prototype.html), a source-to-decision record, and an implementation/acceptance checklist. The prototype demonstrates three science interactions and coloring/drawing with four separate local workspaces. It is not a Unity build or a networked four-device test. No installed app, production server, save schema or runtime game source is changed by this research pass. Final artwork and child usability are not accepted merely because a prototype runs.

## 1. Research conclusions that change the design

NAEYC describes digital media as another way for preschoolers to explore, create and communicate, alongside physical materials. Its guidance favors exploration and documenting children's work. This supports free creation, preservation and sharing; it does not prove this specific game produces learning gains. [NAEYC preschool technology guidance](https://www.naeyc.org/resources/topics/technology-and-media/preschoolers-and-kindergartners).

PBS's Play & Learn Science materials target caregiver-supported inquiry for ages 3–5. Sago Mini School combines child-led science play and doodling. These support using familiar objects, a ready-to-use setup and optional prompts, rather than making reading or a rigid lesson sequence prerequisites. Publisher descriptions establish product patterns, not comparative effectiveness. [PBS program](https://kits.pbskids.org/products/play-learn-science-fcl-workshop-series-english), [Sago Mini School](https://sagomini.com/school/).

The PBS app's evaluation reported positive outcomes in a supported family context, but its single-group design cannot establish causation or generalize to ordinary unsupported app use. Our design inference is to offer optional parent/child comparison prompts, not to label the game scientifically proven. A research team's account also describes challenging the “heavy sinks” assumption with contrasting materials and modeling useful questions. Apply this to “What changed when you added cargo?” and “Try the same cargo in the other boat.” [Evaluation, findings and limitations, pp. 4 and 32](https://files.eric.ed.gov/fulltext/ED599703.pdf), [researchers' family-event account](https://www.nsta.org/blog/plan-interactive-family-science-event-support-app).

Crayola's product describes open-ended drawing/coloring without points or time limits, and its FAQ documents coloring pages, drawing and a gallery in the same explorable world. Apply that separation: a shared table is the entrance; the child's page is a large focused workspace; completed pictures can be displayed. Do not import subscriptions, reward currencies or unlock gates into this family game. [Crayola Create & Play](https://www.crayolacreateandplay.com/), [activity and gallery FAQ](https://www.crayolacreateandplay.com/faq).

**Applied interaction rules:**

- A tap produces an understandable result immediately: an object enters water, a magnet moves toward a sample, a lamp changes the illuminated screen, or a region takes the selected color.
- A drag is expressive but optional for essential actions. Use broad snap targets; never require multitouch, precise timing, blowing, camera recognition or a second player.
- No scores, failure sounds, required order, forced restart or countdown pressure. Sinking is an observation, with an obvious way to lift the object out.
- Provide one short optional invitation at a time. Instructions never cover or immobilize another player's work.
- Leave preserves accepted work. Reset and undo affect the chosen personal workspace, not the whole corner.
- A completed picture stays a creation; it is not automatically cleaned away as borrowed equipment.

## 2. Placement in the actual house

The current illustrated house spans roughly X=-4800 to 0. Its usable book rack is at -4500, sofa -3960, living radio -3480, stairs -2980, kitchen counter -2210, oven -1520, fridge -1150 and dining table -470. The living and kitchen panorama assets were inspected. The existing routes and fixture footprints leave little room for two generous four-player work areas without repeating the kitchen's obstruction problem.

**Recommended layout:** extend the continuous ground floor to the left of the existing living area with one shared discovery/art bay. This is a proposed extension, not a committed change to saved coordinates. It stays in the same persistent Home property; no new world-menu destination, locked bedroom or loading doorway is required. A child can walk from science to art to the shared books, sofa, stairs and kitchen.

| Proposed bay | Purpose | Composition rule |
| --- | --- | --- |
| Left half: discovery bench | Four sample trays, water tub, material tray and colored-light screen; later stations stored/displayed nearby | Low work surface, broad visible controls, clear standing space in front |
| Right half: coloring table | Four paper places, accessible crayon pots, page chooser and personal picture folders | Papers remain readable; tray edge never covers the interaction surface |
| Back wall | Family picture display and science result shelf | Display real creation records; remove a displayed picture from the wall when moved |
| Foreground | Continuous walkable floor joining the living room | Keep a clear corridor and avoid hitboxes extending invisibly across it |

Tentative extra panorama bounds are -7200 to -4800, leaving every existing object and player coordinate unchanged. Final dimensions depend on a Unity composition check at iPad 4:3 and phone landscape ratios. Verify camera bounds, stream seams, cold arrival and carried objects before adopting them. Current scenery already limits loaded/requested textures to three; retain that ceiling.

Architecture, furniture backs, contents/characters and front lips must be separate. Never bake usable crayons, pictures, science samples or a second table into the room background. Match the accepted Home illustration style and character art. The research prototype is a functional schematic and does not approve final room art.

## 3. Science: complete eight-station inventory

The first integration follows the existing chapter-30 order: **SCI-01 floating, SCI-02 magnets, SCI-04 colored light**. They cover different input/result types. Finish and qualify these before the remaining five; keep every row below tracked.

| Station | Child's visible play loop | Adjustable/deeper play | Scientific constraint and source |
| --- | --- | --- | --- |
| **SCI-01 Float and loaded boats** | Choose a sample → tap/drag into water → observe float/sink → lift out | Compare a narrow and wide boat; add/remove identical cargo; decorate a saved boat | Buoyancy depends on displaced water and total weight, not a blanket “metal sinks” or “heavy sinks” rule. Hulls flood when load exceeds their available displacement. [Science Buddies](https://www.sciencebuddies.org/stem-activities/aluminum-foil-boats-float) |
| **SCI-02 Magnet materials** | Move a wand near samples → magnetic samples follow → release onto tray | Compare iron with wood/plastic/aluminum; later flip a second magnet for consistent push/pull | Iron, nickel and cobalt are ferromagnetic. Material identity matters: not every metal is attracted, and not every steel alloy behaves alike. Choose explicitly defined samples. [National MagLab](https://nationalmaglab.org/about-the-maglab/around-the-lab/maglab-dictionary/permanent-magnet/) |
| **SCI-03 Ramps and surfaces** | Release the same ball/car → watch roll → collect/release again | Adjust height or surface one at a time; compare two tracks | More resistance slows motion; avoid “heavier always rolls faster.” Hold toy and launch conditions constant for comparisons. [PBS Friction Racing](https://www.pbs.org/video/friction-racing-rttdqu/) |
| **SCI-04 Colored-light garden** | Switch red/green/blue lamps → see their illuminated overlap | Move light spots; match an optional picture color; switch any lamp independently | Additive light: R+G=yellow, R+B=magenta, G+B=cyan, RGB=white, none=dark. These are not paint-mixing rules. [Exploratorium Colored Shadows](https://www.exploratorium.edu/snacks/colored-shadows) |
| **SCI-05 Dinosaur shadows** | Place a dinosaur between lamp and screen → move it → watch its silhouette | Change distance, switch dinosaur, tell a short story | A closer object relative to a point source casts a larger projected shadow; preserve lamp–object–screen ordering. Simplify the source explicitly. [Exploratorium light/shadow activity](https://www.exploratorium.edu/snacks/colored-shadows) |
| **SCI-06 Bubbles and fan** | Dip wand → tap/hold fan → bubbles form → pop | Change wand outline or airflow; compare before releasing | Free bubbles tend toward spheres; a square wand does not create permanent square floating bubbles. [Exploratorium bubble shapes](https://annex.exploratorium.edu/ronh/bubbles/shape_of_bubbles.html) |
| **SCI-07 Sound and vibration** | Pluck string → see vibration and hear tone → damp it | Change length or tension separately; compare high/low pitch | Distinguish pitch from loudness; show vibration even when muted. Shorter vibrating lengths raise pitch when other relevant conditions stay fixed. [Science Buddies sound lesson](https://www.sciencebuddies.org/teacher-resources/lesson-plans/sound-wave-frequency-amplitude) |
| **SCI-08 Seed, water and light** | Choose seed → water → use an explicit time-jump control → see stages | Compare prepared pots; decorate one and move the resulting plant | Time is deliberately accelerated. Plants need water, light, air and nutrients; soil is useful but not the only growing medium. [NASA](https://science.nasa.gov/eclips/videos/do-plants-need-soil/) |

The physical activities above are references for scientific relationships. Some target older children; adapt the interface and vocabulary for this family's ages. Do not transplant classroom instructions or claim measured educational outcomes.

### First three: state and response details

**Float tub.** Each tray has identified samples and one boat. Choose → place → observe → retrieve is reversible and never consumes a sample. A bounded buoyancy model compares the combined boat/cargo mass with available displacement; derive waterline from that ratio, then cap bobbing as a visual effect. In the prototype, narrow/wide capacities and cargo weights are illustrative units, explicitly not predictions for real objects. The same load can sink the narrow hull and float in the wider hull. Taking cargo off restores buoyancy. Four people must be able to load their own boats simultaneously.

**Magnet tray.** Samples retain material, ID and position. An attraction-radius rule operates only on eligible samples, with bounded movement toward the wand. Tapping a sample moves the wand near it as the accessible alternative to dragging. Nonmagnetic samples visibly stay put. Reset restores only this tray's sample positions. A second-magnet polarity experiment remains a later extension; ordinary iron samples should not suddenly repel because a wand flips.

**Light screen.** Each workspace starts with three large labeled lamp controls and a dark screen. Use additive compositing or an explicit reviewed eight-combination table, not transparent red/green/blue paint layers that turn muddy. Show the component names and resulting color alongside color swatches. The prototype exercises all eight combinations. Movable overlapping spots follow in the Unity interaction implementation; the first prototype validates combination rules and independent toggles.

## 4. Coloring: child-facing flow and scope

**Walk to the shared art table → choose a picture or blank paper → choose a crayon → color → leave/resume or display.** Opening the table never starts narration, changes another child's page, or waits for a free global art lock.

Working default while awaiting the user's optional preference: coloring pages **and** blank drawing paper. Starter themes in the prototype are dinosaur, truck, unicorn, snake, garden and house. These are suggested coloring themes, not a change to the separately requested narrated-book subjects. Every player can choose every theme. Additional recognizable character pages need their own reviewed art sources.

| Feature | Required behavior |
| --- | --- |
| Large page | Fit most of the landscape screen. Place tools around the page, not in an opaque block over the subject. Keep essential controls visible at 4:3 and wide phone sizes. |
| Tap fill | Select color, tap an enclosed region. Use authored region IDs/masks; never perform an unbounded flood fill that leaks through anti-aliased outlines. |
| Crayon drawing | Immediate local strokes; keep the outline above paint. Clip to paper. An optional stay-inside mode clips to the starting region. Blank paper is freely drawable. |
| Palette | Large swatches with names/selection marks, including white and dark colors. No correct-color scoring. A purple dinosaur is valid. |
| Undo/redo | One gesture is one history step. Undo only the current creation's own operation. Reset/clear offers immediate undo and never targets all players. |
| Independent pages | Switching page/profile restores that page's colors and strokes; no empty-page overwrite. Owner ID is the persistent family profile, not Bluey/Bingo selection. |
| Gallery | Finished and unfinished pictures persist. Display is optional. A copy gets a new creation ID with provenance; a move keeps the same ID. |
| Sharing | Viewing is allowed. Editing another owner's picture requires explicit Together permission. Turning Together off ends future edit access without discarding accepted work. |
| Save feedback | Distinguish local pending marks from acknowledged durable progress. If saving fails, keep the picture open/in memory and show retry; never silently claim Saved. |
| Audio | Gentle optional tool sounds; local mute/volume. No new narrator is selected by this task, and the reported book-sound issue remains separate. |

The interactive prototype implements six tap-fill pages, free strokes on blank paper, undo/redo, per-player/page persistence, personal science trays and a four-player summary. It does not implement a production gallery, room-object carrying, Together permissions, narrated hints or true network sharing.

## 5. Four-player and save contracts

The PC/VPS is the only shared authority. Installed solo uses the same rules with separate private saves; reconnect never uploads offline pictures or experiments into the shared family world. Four children may all use science, all use art, or split across activities. One leaving, backgrounding, closing a page or changing rooms releases only their temporary input/tool claim.

**Proposed durable records:**

| Record | Meaningful fields | Exclude |
| --- | --- | --- |
| Science workspace | Stable ID, owner/profile, station/version, sample identities/materials/positions, boat/cargo, lamp state, result revision | Splash particles, frame timers, rendered light textures |
| Creation metadata | Creation ID, owner, page-definition/version, revision, permissions, display/support location, content digest | Full image bytes in every world snapshot |
| Coloring content | Region-color mapping, ordered strokes/checkpoint, undo metadata within a bounded policy | One GameObject or reliable world update per sampled pen point |
| Local preferences | Selected tool/color, motion/sound settings, current view | Authority ownership or shared progress |

**Do not repeat the current networking failure.** The existing wire limit is 131,072 bytes and Home sends reliable snapshots for accepted world transactions. Adding arbitrary picture histories to every snapshot, or submitting full-world transactions at drawing frequency, would undo the 172 bandwidth correction. Treat saved artwork as separately versioned content, fetched only when opened/displayed. Bound operations, chunks, pending queues, history and decoded textures. Measure full-world and artwork sizes at their maxima; refuse malformed/oversized payloads before allocation.

A production stroke needs a stable operation ID, creation ID, base revision, bounded normalized points, color and tool width. Locally preview immediately; acknowledge the completed stroke or bounded batch. Retries of the same ID are idempotent. Server validation checks ownership/Together permission, page version and coordinates. It must not replay a stale stroke onto a newer page after a page switch. Initially, one editor per creation is simpler and safer than claiming collaborative merge correctness; four independent creations remain concurrent. Together mode needs its own scoped concurrency tests.

Only durable acknowledged state enters backups. Temporary leases do not survive restoration. Additive migration adds empty new workspaces without rewriting old profiles, fixtures, dishes, bedrooms or bookmarks. Existing build 171 clients must not silently receive a schema/content they cannot read; a later Unity release needs coordinated compatibility and recovery qualification.

## 6. Input, accessibility and older-iPad budgets

Apple specifies at least 44×44-point hit regions as a general touch baseline and recommends visible press states and spacing. For this toddler-oriented design, choose larger targets where practical, then measure them on real devices: Unity reference pixels and CSS pixels do not automatically equal iPad points. Prototype controls are generally 48–56 CSS pixels or larger. [Apple buttons](https://developer.apple.com/design/human-interface-guidelines/buttons), [UI design tips](https://developer.apple.com/design/tips/).

Use one active pointer per personal tool. A second finger must not move the character through the page, draw a line from the previous finger position, or activate a hidden world control. Cancel the gesture cleanly on close, travel, suspension, session change and device disconnect. Keep results visible with sound off; honor reduced motion with still water and restrained glows.

Unity's uGUI guidance recommends separating dynamic content and avoiding needless rebuild/raycast work. Apply this to one bounded drawing surface, reusable/poolable stroke data and a small set of tool controls, rather than creating hundreds of child graphics per stroke. Noninteractive decoration should not receive raycasts. These are implementation strategies, not measured A10 performance claims. [Unity uGUI optimization](https://unity.com/how-to/unity-ui-optimization-tips).

Proposed budgets to measure: one active paper render surface per device; small gallery thumbnails loaded on demand; capped water/bubble particles and tool sounds; no simulation work on closed views beyond necessary authoritative state; preserve the existing three-panorama load ceiling. Do not choose a high-resolution painting canvas before measuring sustained A10 memory/frame time and repeated open/close behavior.

## 7. Applied research traceability

| Finding | Applied now | Unity acceptance test |
| --- | --- | --- |
| Open-ended, child-led exploration | Three reversible science loops; no score/forced sequence | A child can start and repeat each loop without reading a blocking instruction |
| Material-specific magnetism | Iron sample moves; wood/plastic/aluminum remain | Same material response on server and all clients, with tap and drag inputs |
| Weight/displacement relationship | Same cargo compared in narrow/wide hulls | Four independent boats; unload recovers; late join sees the same result |
| Additive light | All eight RGB combinations, named result | R+G yellow, RGB white, none dark; no paint averaging |
| Creative ownership | Four profile/page records; independent undo | Avatar changes preserve ownership; sibling cannot erase another's art |
| Large art surface and forgiving input | Big page, surrounding tools, fill plus blank drawing | iPad/phone hit targets and actual child use; no clipped subjects |
| Preserve children's work | Browser-local autosave/restore demonstration | Authoritative save/reopen, interrupted save, restore and offline/server separation |
| Bounded traffic and UI work | Explicit production content contract and budgets | Four artists drawing while another activity progresses; no queue errors or stalled countdown |

## 8. Ordered implementation and acceptance checklist

This research changes the next Home feature design; it does not mark LAB-01 or coloring playable in the installed game. Preserve the pending 172 shared-play/audio deployment work as a separate unfinished prerequisite to calling a new shared release healthy.

1. **SC-0 research/prototype:** source review, placement plan, first-three science rules, art flow and honest prototype limitations. Test actual prototype interactions and save isolation. This pass.
2. **SC-1 common downstairs foundations:** extend/compose the bay, safe routes and four workspace identities; versioned save migration and independent entry/exit. Inspect real art layering and scenery seams.
3. **SC-2 first science trio:** integrate float/magnets/lights with real sample state, tap/drag alternatives and shared authority. Test intermediate animation/state, not only the final result.
4. **COL-1 coloring:** authored closed-region pages, full-page interface, bounded stroke/content storage, per-profile folders, undo and visible saved status. Test payload limits before four-client drawing stress.
5. **SC-3 shared release:** four concurrent activities, one participant leaving/backgrounding, save/reopen, exact backup recovery, mixed-device/A10 endurance, current artifact identity, retained-save installs and physical visual/audio acceptance.
6. **SC-4 remaining science and creation integration:** ramps, dinosaur shadows, bubbles, vibration and plants; movable saved boats/plants/art and optional Together editing. Retain all eight stations throughout.

Minimum scenarios: all four at one science type; all four coloring different pictures; mixed science/cooking/books; closing one overlay while others continue; duplicate commands; stale page edits; concurrent edits to one creation; owner versus visitor reset; force-close during an acknowledged stroke; offline private drawing followed by authoritative reunion; maximum artwork inventory; repeated image open/close; no clipped dinosaur/unicorn/truck subjects at supported ratios.

## 9. Evidence boundary and remaining decisions

The prototype uses a single browser with selectable player workspaces. It is suitable for exploring flow, state isolation and the reviewed example rules. It does not simulate network delay, prove concurrent devices, use the accepted Unity character animation, or qualify save migration, Apple signing, audio output or actual child usability.

The optional preference question asks whether coloring should include blank drawing paper. Pending a reply, both are included in this prototype. The room extension, coloring themes, final artwork, canvas resolution, saved-picture capacity and Together concurrency remain design choices to validate during their bounded implementation slices. Existing Home, room, kitchen, story and device requirements remain on the backlog.

## 10. Prototype verification

Eight model-check groups passed: boat capacity, material-specific attraction, all eight light combinations, profile/page isolation, gesture undo/redo, serialization, malformed-data rejection and bounded drawing/history storage. [Model evidence](evidence/science-coloring-research-2026-09-27/model-checks.json). These validate the prototype model, not Unity code or a network protocol.

Actual browser interactions verified the same three cargo loads sinking the narrow hull and floating the wide hull; red plus green light becoming yellow and all three becoming white; a dragged magnet moving iron while leaving the other samples in place; independent reset; fill and drawing undo/redo; keyboard region filling; and saved colors/drawing surviving reload and player switching. All six pages' rendered regions stayed within their paper bounds. Tablet and phone viewport checks (1024×768, 844×390 and 390×844) found no horizontal overflow. No page-script warnings/errors were reported during the check. [Browser evidence](evidence/science-coloring-research-2026-09-27/browser-checks.json).

The review prototype scrolls vertically on small screens. This is not acceptance of the eventual landscape Unity activity overlay or a physical-device touch-size check. Dinosaur and unicorn pages received visual inspection; geometric bounds checks on all six pages do not substitute for final illustrated-page art review. [Coloring preview](evidence/science-coloring-research-2026-09-27/coloring-preview.png) · [Science preview](evidence/science-coloring-research-2026-09-27/science-preview.png).

The maintained research, build guide, Home tracker and generated feature inventory were reconciled. Catalog validation retains all 55 research chapters, 35 master requirements, eight science stations and eight additional coloring requirements. All remain accurately classified for game implementation. The [plan/link consistency check](evidence/pc-vps-plan-audit-2026-09-25/docs-validation.json) passed; JavaScript syntax validation also passed.

At the SC-0 milestone, only research and the browser prototype were complete. The later implementation linked above records actual Unity progress; physical four-device, older-iPad and child acceptance remain open.
