# Four-player science playground: research and interaction prototypes

**Later September 28 update:** [Dinosaur hammer, expanded bubble lab and new liquid-color lab](science-hammer-labs-2026-09-28.html) now bring the collection to fifteen activities with a tested version-2 save upgrade. The report below preserves the earlier scope and evidence.

**September 28 follow-up:** [Simpler controls, applied children's-app research and current evidence](science-kids-flow-2026-09-28.html) now supersede the original dropdown-based interface. The scientific models and prior evidence below remain the baseline.

September 27, 2026 · LAB-01 / SCI-03/04/06/09 and fourteen requested activity concepts.

The user accepted the more playful activity list and requested deeper research, then working prototypes before detailed production work. This is a separate browser playground for reviewing the interactions. It does not alter Unity, installed build 188, the live family server, enrollment or game saves. The existing nine science stations and the wider Home backlog remain required.

[Open the playground](home-science-playground.html). Each of four local players has fourteen retained workspaces. Focus on one or display all four together. Browser storage is a prototype convenience, not the game's PC/VPS multiplayer architecture or a network qualification.

## Evidence and design method

Read the primary experiment descriptions, their explanations and the linked educator guides. The deeper sources include the Exploratorium's [marble-machine guide](https://www.exploratorium.edu/sites/default/files/tinkering/files/Instructions/marblemachines_activity-guide.pdf), [wind-tube guide](https://www.exploratorium.edu/sites/default/files/tinkering/files/projectpdfs/Wind_Tubes.pdf), [circuit guide](https://www.exploratorium.edu/sites/default/files/tinkering/files/Instructions/circuit_boards.pdf), [scribbling-machine guide](https://www.exploratorium.edu/sites/default/files/pdfs/scribbling_machines.pdf), and Tinybop's [weather handbook](https://tinybop.com/assets/handbooks/weather/Tinybop-EL6-Weather-Handbook-EN.pdf) and [light handbook](https://tinybop.com/assets/handbooks/light-and-color/Tinybop-EL12_LightandColor_Handbook.pdf). These are descriptions and guides, not hands-on testing of purchased apps or proof of child preference. Some physical activities target older children; the reduced controls and ready setups below are our preschool adaptations.

The useful common pattern is an immediately usable apparatus, a visible action/result relationship, and variables that change the outcome. Our prototypes therefore implement bounded stateful models rather than preselected success animations. Constants are illustrative and accelerated; none predicts a household experiment quantitatively. Apparatus dominates the view. Tap alternatives accompany dragging; no microphone, camera, reading quiz, required second participant or global reset is needed.

## Fourteen applied designs

These stable review IDs track the requested prototypes without marking the production science backlog finished. Every entry below is **playable as a browser prototype; detailed production work remains open**.

| Prototype ID | Activity | Scope relationship |
| --- | --- | --- |
| SP-01 | Lava jars | SCI-09 extension |
| SP-02 | Dinosaur ice rescue | New heat/melting play |
| SP-03 | Marble playground | SCI-03 expansion |
| SP-04 | Stretchy slime | New polymer play; distinct from oobleck |
| SP-05 | Magic milk | New surface-flow play |
| SP-06 | Drawing robot | New science/art play |
| SP-07 | Balloon rocket | New propulsion play |
| SP-08 | Foam fountain | New reaction within SCI-09 |
| SP-09 | Wind tube | New airflow play |
| SP-10 | Light-up inventions | New circuit play |
| SP-11 | Mini weather world | New water-cycle play |
| SP-12 | Bubble garden | SCI-06 exploration |
| SP-13 | Rainbow mirrors | SCI-04 expansion |
| SP-14 | Family chain reaction | New connected-construction play |

### 1. Bubbling lava jars — extension of SCI-09

**Research:** an effervescent tablet reacts in the water phase. Gas carries colored water upward through oil; after gas escapes the water descends. Warmer water changes reaction rate. [Science Buddies](https://www.sciencebuddies.org/stem-activities/make-a-lava-lamp).

**Prototype:** a large transparent jar with separated water/oil layers; add a finite tablet portion, change color or temperature, and watch rising and falling blobs. The reaction consumes its reserve and settles; another portion restarts it. Color is appearance only. **Check:** warm/cool runs consume the same reserve at different rates; no endlessly self-renewing reaction. **Later:** direct pour streams, exact layer quantities, bottle customization and durable Home display.

### 2. Dinosaur ice rescue — new heat/melting activity

**Research:** compare ice melting in water at different temperatures. Heat transfer, not a generic progress tap, changes the melting rate. [Science World](https://www.scienceworld.ca/resource/swirls-around-us/).

**Prototype:** warm/cool dropper on a visible grid of ice around a dinosaur toy; each drop adds a bounded local amount of heat and melts nearby cells. A broad assisted pour reaches the whole block. Melting persists and eventually exposes a movable toy. This is a frozen toy, not a claim about fossil formation or dinosaur hatching. **Check:** equal warm drops melt more than equal cool drops, no regeneration when switching activities. **Later:** detailed melt boundaries, multiple dinosaur toys and transferring the same toy record into Home.

### 3. Marble playground — develops SCI-03

**Research:** open construction with tracks, funnels and catches supports repeated design/test/revision. [Exploratorium](https://www.exploratorium.edu/tinkering/projects/marble-machines).

**Prototype:** a working zigzag course with six large movable ramp endpoints and visible catch rails. A gravity-driven ball collides with the actual segments; change their slope or surface, release and catch it. Tap a ramp endpoint then its new position, or drag. **Check:** the starter course reaches its basket; altered geometry changes the path; greater resistance slows the same ball; a reset affects only this player's ball/course. **Later:** snap inventory, funnels/tunnels/seesaws and bounded saved construction graphs. The prototype is three editable ramps, not an arbitrary track builder.

### 4. Stretchy slime — distinct from existing oobleck

**Research:** cross-linking polymer chains changes a flowing glue mixture; slow extension and quick pulling can produce different behavior. [American Chemical Society](https://www.acs.org/education/whatischemistry/adventures-in-chemistry/experiments/slime.html).

**Prototype:** add the two virtual portions, stir to form slime, then drag a handle slowly to stretch or quickly to tear. Rejoin and squish allow repeated play. **Check:** unformed liquid cannot stretch; gesture speed changes the result; color does not change the material model. **Later:** richer deformation, controlled ingredient ratios, molds and tactile sound. This is a small viscoelastic gesture model, not full fluid physics or real-world mixing instructions.

### 5. Magic milk — new surface-flow activity

**Research:** touching colored milk with detergent produces outward movement and subsequent swirls; the result depends on where the detergent touches. [American Chemical Society](https://www.acs.org/education/whatischemistry/adventures-in-chemistry/experiments/colors-move.html).

**Prototype:** place colored drops in a shallow dish, then touch with a soap wand. Local radial flow and a damped swirl carry the existing colored marks. Response diminishes as the modeled surface becomes saturated. **Check:** soap-only produces no invented dye, marks stay inside the dish, effects decay. **Later:** smoother pigment advection, milk comparison and saving a picture. The simple flow is illustrative, not a molecular simulation.

### 6. Drawing robot — connects science and art

**Research:** an eccentric motor makes a drawing contraption move; motor speed, imbalance and the supporting tools change its traces. [Exploratorium](https://www.exploratorium.edu/tinkering/projects/scribbling-machines).

**Prototype:** a three-pen robot moves on paper; start/stop, speed, imbalance and colors affect its path. A bounded stroke buffer keeps its drawing. **Check:** motor off creates no new strokes; changing imbalance changes the trace; paper bounds and saved histories remain finite. **Later:** free assembly, individual pen placement and a durable art-folder object. Its kinematic model is a sketch of eccentric motion, not a validated motor dynamics solver.

### 7. Balloon rocket — new propulsion activity

**Research:** escaping air propels a balloon in the opposite direction along a string. [NASA K–2 lesson](https://www.jpl.nasa.gov/edu/resources/lesson-plan/simple-rocket-science/).

**Prototype:** pump, release, coast and return. The visible balloon shrinks as its air reserve supplies thrust; a weight changes acceleration. Each player has a separate lane and no winner/loser screen. **Check:** empty balloon provides no thrust; larger initial air reserve has more total impulse in this model; the toy remains retrievable. **Later:** decorating, side-by-side comparisons and clearer air jets.

### 8. Foam fountain — separate chemistry within SCI-09

**Research:** catalyzed peroxide decomposition releases oxygen; soap traps bubbles as foam. The catalyst speeds the reaction rather than acting as the gas supply. Container shape changes the display. [Science Buddies](https://www.sciencebuddies.org/stem-activities/elephant-toothpaste).

**Prototype:** labeled virtual peroxide, soap and yeast mixture; contact starts the reaction without a forced stirring gate. Finite reactant becomes visible gas/foam; narrow/wide vessels change column height, and spill stays in a catch tray. **Check:** no soap gives bubbles with little retained foam; more catalyst cannot replenish depleted reactant. **Later:** detailed foam meshes, warmth cue and controlled comparison trays. No real-world preparation steps or concentrations are presented in the child interface.

### 9. Wind-tube inventions — new airflow activity

**Research:** weight, shape and exposed area affect how objects respond to constrained airflow; hover, ascent and descent are useful comparisons. [Exploratorium](https://www.exploratorium.edu/tinkering/projects/wind-tubes).

**Prototype:** fan, large/small parachute and add/remove weights. A bounded vertical drag/gravity model responds continuously; a weighted canopy settles lower for the same fan. **Check:** fan off causes descent, larger area increases lift at equal mass, contents stay retrievable. **Later:** assembled spinners and asymmetric flight. Constrained 1D airflow is deliberate; do not promise turbulent fluid simulation.

### 10. Light-up inventions — new circuits activity

**Research:** a complete battery/load loop powers an output; a switch breaks or completes the path. [Exploratorium](https://www.exploratorium.edu/tinkering/projects/circuit-boards), [PhET reference](https://phet.colorado.edu/en/simulations/circuit-construction-kit-dc).

**Prototype:** select two large terminals to connect/disconnect a wire, or drag between them. Battery, switch and interchangeable lamp/fan/buzzer form an actual graph. The load only works when both battery terminals are connected through a closed path containing it. Direct battery shorts are refused. **Check:** one wire and open switches cannot power a load; invalid shortcuts do not light it. **Later:** parallel paths, resistance and decorated constructions. The first model is connectivity, not a quantitative electrical solver.

### 11. Mini weather world — new water-cycle activity

**Research:** evaporation transfers surface water into vapor, condensation makes cloud droplets, and precipitation returns water. [NASA](https://science.nasa.gov/earth/earth-observatory/the-water-cycle/). [Tinybop Weather](https://tinybop.com/apps/weather) demonstrates manipulating weather variables in an interactive landscape.

**Prototype:** sun warms a pond; cooling encourages cloud formation; sufficient condensed water rains or snows according to the selected temperature. Wind moves the cloud. Surface/vapor/cloud/precipitation stores conserve a fixed water total. **Check:** rain requires cloud water, warming alone does not instantly conjure rain, no water created by repeated controls. **Later:** landscape channels, better thermal coupling and accurate snow accumulation/melt. Time and thresholds are heavily compressed; storm/hurricane simulation is not included.

### 12. Bubble garden — implements the SCI-06 concept in isolation

**Research:** the wand holds a film, while a released free bubble tends toward a sphere. [Exploratorium](https://annex.exploratorium.edu/ronh/bubbles/shape_of_bubbles.html).

**Prototype:** choose round/square wand, dip, blow a finite film, steer fan and pop bubbles directly. Released bubbles are round for either wand. **Check:** dry wand produces nothing; dipping renews its finite film; individual pops do not reset other bubbles. **Later:** large stretched films, iridescence and joined bubble geometry.

### 13. Rainbow mirrors — develops SCI-04

**Research:** reflection changes beam direction; a prism separates wavelengths through differing refraction. [Tinybop light handbook](https://tinybop.com/assets/handbooks/light-and-color/Tinybop-EL12_LightandColor_Handbook.pdf).

**Prototype:** rotate a mirror on a fixed light path, move a prism into the reflected beam and see a bounded fan of spectral rays. Reflection uses the geometric mirror angle; prism dispersion is an explicitly simplified visual fan, not a wavelength-accurate glass solver. **Check:** no beam/prism intersection means no rainbow; mirror changes direction; turning the source off removes both. **Later:** a second mirror, Snell-law prism geometry, opaque obstacles and optional target invitations. Do not mix up additive lights with pigment mixing.

### 14. Family chain reaction — construction/energy-transfer activity

**Research:** each participant can build a cause-and-effect segment and connect it to the next. [Exploratorium](https://www.exploratorium.edu/tinkering/projects/chain-reaction).

**Prototype:** six places accept domino/ramp/bell/gap pieces. A pulse advances only through connected pieces. Each player may independently opt into a family chain; completed segments pass a pulse to the next opted-in player. A leaving player is skipped without clearing anyone's arrangement. **Check:** a gap stops transmission, individual runs remain independent, cooperative completion never overwrites designs. **Later:** actual rolling/contact machinery, spatial assembly and server-owned trigger receipts. The current connection/pulse model is not a physics construction sandbox.

## Prototype systems and production boundary

- Four panels can run different experiments concurrently. Focus selection changes presentation only; every player's selected activity and all fourteen states remain separate.
- Fixed small simulation steps, bounded particles/strokes and capped inputs prevent unbounded effects. Pause stops the whole preview explicitly; closing one activity does not reset anyone else.
- Pointer ownership is per panel and pointer ID. Capture/release/cancel prevents a drag from jumping to a sibling's workspace. Keyboard/tap controls are available for the core loop. [MDN pointer capture](https://developer.mozilla.org/en-US/docs/Web/API/Element/setPointerCapture).
- Versioned browser data is separate from all existing prototypes and game saves. Validate before loading; preserve unrecognized data rather than overwrite it. Local save failure must be visible. No sync with the family world, no host election and no offline merging.
- Use the existing illustrated workshop backdrop with deliberately simple equipment art. These are interaction prototypes; final layered sprites, audio/narration, performance on A10 and physical multi-touch are later qualification, not inferred from browser success.
- Production integration must move authoritative variables/commands into the PC/VPS-owned game model, with schema migration, finite inventory, late join, recovery, original enrollment and four-device tests. Do not port browser storage or client clocks as shared authority.

## Review sequence and evidence

Try all fourteen, choose the strongest loops, then deepen one activity at a time. Lava, ice rescue and marble remain the recommended first detailed slices; the user may choose a different order. Original science stations are not retired by this prototype collection.

**Completed prototype validation:** [20 model test groups](evidence/science-playground-2026-09-27/model-tests.json) passed. They cover all fourteen mechanisms, depleted reagents, geometry/material changes, a complete circuit versus an open/shorted path, conserved water, all four chain participants, independent reset, pause/resume, save restoration, malformed saves and bounded histories. These run with `node Tools/Test-SciencePlayground.mjs`.

**Browser checks:** all fourteen activities were selected and their main controls exercised. A manually tapped/dragged circuit powered its lamp; native sliders changed values; ramp dragging worked. Dinosaur rescue retained 21% melting through navigation and reload. Four different workspaces ran together. Narrow 390-pixel and tablet 1024-pixel layouts were visually inspected; a 320-pixel layout received a DOM overflow check. There were no captured JavaScript errors/warnings in the final tested session. [Browser check record](evidence/science-playground-2026-09-27/browser-checks.json). [Review screenshot](evidence/science-playground-2026-09-27/playground-review.png).

The browser pass caught a Windows `.mjs` MIME issue; the dedicated local server explicitly serves modules as JavaScript. The first starter marble layout overshot its next ramp; visible catch rails and adjusted starting geometry now route it into the basket, with a regression assertion. An exactly centered soap tap now moves coincident dye, and the mirror illustration agrees with the reflection model.

**Run locally:** from the project root, run `python Tools/Serve-SciencePlayground.py`, then visit `http://127.0.0.1:8763/docs/implementation/home-science-playground.html`. The server binds only to this PC. Use Games to open the experiment picture chooser, Player 1–4 to focus a workspace, or Grown-ups → Four together. Again resets only that player's selected experiment and offers Undo restart. Browser data is under `little-weeps-science-playground-v1`; it never reads a game save. Pause stops the preview; backgrounding also suspends the simulation until the page is visible again. Tap sounds remain an optional quiet synthesized cue. The September 28 pass adds separate on-demand local draft hints; neither is final narration or production effects.

**Still unqualified:** simultaneous physical multi-touch, A10 performance, child usability, final science accuracy of quantitative parameters, polished art/audio, shared-server timing, game inventory/save migration, late join and recovery. Four local panels do not establish four-device multiplayer. No new Unity build or physical-device deployment was performed. Existing production audit statuses remain unchanged; their generator does not classify these browser prototypes as completed game features.
