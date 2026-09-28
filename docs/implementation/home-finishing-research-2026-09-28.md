# Home finishing: research applied in the requested order

September 28, 2026. The user authorizes research **and implementation**, in this order: coloring controls, creation/food storage, remaining cooking stages, then another Home room or activity. Work one bounded slice at a time. Four independent profiles, PC/VPS authority, private offline saves, accepted art and five-minute temporary cleanup remain requirements. The existing hold on integrating this branch into main remains.

## Sources and evidence limits

These are publisher descriptions, platform guidance and hands-on activity research reviewed online. They establish design references, not evidence that we ran the commercial apps, that their multiplayer matches ours, or that our implementation is physically accepted by children. The existing [science/coloring research](home-science-coloring-research-2026-09-27.html), [cooking-stage research](kitchen-staged-cooking-research-2026-09-27.html), [picture-button research](home-picture-controls-2026-09-28.html) and [Home tracker](../home-world-feature-tracker.html) retain the detailed requirements.

| Primary source | Finding | Applied decision / check |
| --- | --- | --- |
| [Sago Mini Doodlecast, developer's parent guide](https://sagomini.com/article/doodlecast-letter-to-parents/) | Picture selection can use a grid; color/tool selection precedes drawing; finished creations have a gallery. | Show six large page previews at a time, then the selected full page and visible crayons. Keep art storage separate from temporary activity reset. Do not add recording or device permissions. Test all eighteen pages and four separate histories. |
| [Crayola Create & Play](https://www.crayolacreateandplay.com/) | Tool-led, open-ended coloring/drawing, without a required score or timer. | Keep the page primary and tools around it. No completion percentage, correct-color check or loss of pictures after inactivity. |
| [Crayola developer FAQ](https://www.crayolacreateandplay.com/faq) | Artwork is revisited through a gallery, separate from the current workspace. | A personal picture folder should preserve an acknowledged creation while the workspace is reused. Our version must also preserve independent server-owned profiles; do not copy the referenced app's account/sync model. |
| [Apple, designing game controls](https://developer.apple.com/videos/play/wwdc2024/10085/) | Touch controls need recognizable actions, deliberate placement and large targets. | Pictures plus short words, fixed Home/Back/Next positions, no gesture-only page navigation. Native screenshots and measured touch rectangles at tablet and phone layouts. |
| [W3C enhanced target guidance](https://www.w3.org/WAI/WCAG22/Understanding/target-size-enhanced.html) | A 44 CSS-pixel target is a reference minimum; frequent actions benefit from more space. | Use generous cards and spacing. Native pixel measurements are only a layout check, not a physical-point equivalence or toddler usability certification. |
| [Toca Boca Jr, publisher](https://www.tocaboca.com/kids/toca-boca-jr) | Kitchen play changes food through tools and lets children experiment with combinations. | Preserve real ingredients/food state and optional Experiment mode. Recipe guidance should filter relevant inputs without imposing failure on free creation. |
| [Budge's Strawberry Shortcake Bake Shop listing](https://play.google.com/store/apps/details?id=com.budgestudios.StrawberryShortcakeBakeShare) | Dessert play uses distinct preparation actions, pouring and decorating, with different mechanics per dessert. | Extend the existing chocolate-cake state machine by recipe family; show intermediate batter, mold, baked food and decorations. Offer tap assistance for the same visible process. Avoid merely advancing a generic button count. |
| [Exploratorium Marble Machines activity guide](https://www.exploratorium.edu/sites/default/files/tinkering/files/Instructions/marblemachines_activity-guide.pdf) | Children arrange a course, try it, observe and revise it; individual boards can support adjacent participants. | Suggested next activity after cooking: the already requested ramp/marble station, using a ready course, large adjustment handles and visible rolling changes. Four saved boards; no timed win condition. This is an implementation choice, not a replacement for bathroom/laundry or the wider backlog. |

## 1. Coloring controls — first active slice

Use the existing illustrated workshop and original eighteen page assets. Reuse the accepted cream/teal rounded cards and small vector action pictures. Pictures, Undo, Redo, Back, Next and Home remain visible. Selected crayons have shape feedback as well as color. The page fits its complete authored aspect ratio. The chooser shows six large previews on each of three pages, with a visible current-page selection and simple arrows. First/last page controls disable instead of wrapping unexpectedly.

Keep existing world commands, owner IDs, page IDs, fill masks and histories. A control-only change needs no save migration. Block paper edits behind the chooser and while an earlier command is pending. Show a rejected save rather than hiding it. Opening, leaving and switching pictures must not clear anyone's work.

Acceptance: retained build-200 colors/history; all eighteen pages reachable; real touch fills on official masks; four different colors on the same page; local undo/redo and independent departures; server restart/rejoin; no clipped or overlapping essential controls; native phone/tablet captures. Freehand, creation folders and room display are subsequent behavior, not implied by picture buttons.

## 2. Creation and food storage — next

The five-minute cleanup preserves food but leaves it on its original dish. The needed behavior is **put away the creation, release the reusable equipment, retrieve the same creation later**. A stored food record must retain its identity, ingredients and counted units, recipe version, preparation state, decorations and remaining portions. Storage must never grant a second edible copy or replenish ingredients. Busy, held and actively cooking dishes are ineligible for automatic packing. An explicit owner action can park safe unfinished work; a running heat stage first settles safely.

For pictures, retain a bounded personal folder and stable creation identity, with preview/open actions. Display refers to a creation rather than copying its mutable page buffer. Recoloring a fresh workspace cannot silently replace a displayed drawing. Visitors cannot overwrite another owner's work. Capacity must be visible, with an explicit recoverable choice when full, never silent eviction of the oldest creation.

Engineering: make archive/restore one authority transaction, with idempotent command receipts and owner validation. Keep content out of every frequent snapshot if it would exceed the measured wire budget. The existing maximal shared view is already close to 100 KB; measure worst-case food/art records before deciding representation and capacity. Version any new save contract additively, preserve old food versions, and test four simultaneous transfers, repeated requests, full storage, held objects, partial portions, restart/recovery and private-solo separation. Do not treat a thumbnail-only album as durable food storage.

## 3. Remaining cooking stages

Follow the retained fifteen-recipe table and its distinctive shapes/assembly. Finish the other four cakes first, then five pizzas, then five meals. Existing chocolate cakes and legacy dishes keep their preparation contract; never reinterpret an already paid ingredient as unconsumed stock.

| Family | Visible process | Essential distinctions |
| --- | --- | --- |
| Cakes | Ingredients → mix → pour/shape → bake → fill/ice → decorate → cut/serve | Duck body/head assembly; heart mold; rainbow layers; carrot preparation. Chocolate remains the existing layer-cake path. |
| Pizzas | Dough → knead/roll → sauce → cheese/toppings → bake → slice/serve | Correct ingredient sets; optional chopped vegetables; free topping positions and silly-face arrangement survive baking and portions. |
| Meals | Prepare → appropriate pan/pot actions → assemble → plate/serve | Burger patty cooks before stacking; pasta boils with separate sauce; soup vegetables and broth; pancake batter/flip/stack; rice and vegetables. |

Inputs come from the current stage. A gesture previews locally and commits bounded progress with a dish/stage token; stale touches must not complete the next stage or another tray. Tap assistance executes the same progress over time and cannot skip a stage by repeated tapping. Four separate cooks can mix, bake, decorate and plate concurrently. Server ticks advance heat and visible countdowns independently of an open client panel. Walking away settles heated food to ready without destroying it.

Acceptance per family: appropriate palettes; intermediate saves; stock conservation; four cooks; simultaneous serving with disjoint portions; safe departure; old-save migration; repeated command idempotence; native visible interaction; restart/recovery. Drinks, orders, albums and picnic packing remain on the tracker even when all fifteen paths are improved.

## 4. Next activity and retained room scope

Ramp/marble play is the working next activity because it already has a researched prototype and fills SCI-03. Use the existing Home science entry, child-selected height/surface, a forgiving catch tray and repeatable release. Motion must respond to the actual course and surface, not a prerecorded success animation. Reuse the picture controls and five-minute temporary reset, while preserving deliberately saved constructions. An optional complete-course start supports immediate play. Broader construction graphs, tunnels and family chain reactions require separate bounded work.

Bathroom/laundry, Home TV, the agreed six-book subjects, twenty dinosaur toys, parents/hiding, backyard rides/fishpond, room furnishing expansion, freehand art and physical device qualification remain required. This sequence does not mark Home complete or substitute browser evidence for the installed game. Server/iPad rollout requires a coordinated matching release; no device deployment is implied by editing source.

## Work evidence

[Coloring controls candidate 202](native-coloring-controls-2026-09-28.html) passes six native groups with saved-page retention and actual phone/tablet captures. Storage, remaining cooking families and ramp integration are researched design decisions here, not claims of finished code.
