# Cooking stages and ingredient flow — audit and research, September 27, 2026

**Subsequent implementation:** [Chocolate cake candidate 171](chocolate-cake-flow-2026-09-27.html) applies this research. This document preserves the research-pass findings; the other recipe families and physical acceptance remain open.

**Status: research and implementation contract; not implemented.** The user tested installed Android **166** and reported patties/cheese appearing in cake-making, and cooking that mostly involves repeatedly pressing a button. This is an open gameplay acceptance failure. Build 166 improved layout and entry, but did not deliver the hands-on cooking sequence requested in [chapter 19](../bluey-game-research-2026-09-23.html#19-kitchen-five-pizzas-five-cakes-five-meals).

This pass examines recipe selection, ingredient timing, gesture behavior, food presentation and the existing persistence model. It is a bounded process audit, not a general repository audit. It changes research/tracking documents only. No new game code, assets, build or installation is included. Samsung remains **166**, with the [completed installation and retained saves](evidence/kitchen166-2026-09-27/android-update.json). Four-player independence, the connected Home, accepted characters and the complete fifteen-recipe menu remain required.

## Findings in the installed process

| Finding | Evidence in the current process | Consequence and correction |
| --- | --- | --- |
| **F-01: every food exposes the same inventory** | `PresentEasyKitchen` puts the recipe's suggestions first, then appends all 25 ingredients. The same pages remain available across preparation. `KitchenAssist` accepts any ingredient into a whole dish. | Cake can offer patty, cheese, pasta and broth. Filter both the visible palette and accepted operations by **recipe and current stage**. Filtering only the first page is insufficient. |
| **F-02: ingredients are checked at the wrong time** | `Kitchen.Next` requires everything in the recipe's `toppings` list when it reaches heat. Cake lists include icing; duck cake includes popcorn. Pancakes include banana; burgers include lettuce. | Decorations are effectively prerequisites for baking, and assembly ingredients have no distinct roles. Separate batter/dough ingredients, fillings, raw toppings, finishing decorations and serving additions. |
| **F-03: a gesture usually means “advance”** | On the work surface, tap and completed drag both call `KitchenStepGesture`. Drag motion itself does not update mixing or shaping. Most corresponding authority actions check the tool and increment `dish.step`. | Mix, chop, shape, stack and pour have different names but largely the same interaction. Each needs a distinct activity with visible intermediate results and a meaningful completion condition. |
| **F-04: the main food does not represent enough preparation states** | Pizza has useful dough/sauce/cheese/baked layers. Cakes and meals retain preset finished food illustrations plus extras. | A label change cannot show separate ingredients becoming batter, batter filling a mould, or cake layers being assembled. Author intermediate food/tool/container states before calling the recipe complete. |
| **F-05: one heat path serves every recipe** | Assisted Bake places cookware in an available oven support. All recipes use one `heat` stage and the same eight-second heat duration. | Burger patties, pancakes and pots of soup need appropriate pan/pot activities and their own visual cues. Equal time is not inherently wrong; missing process distinctions are. |
| **F-06: picture guidance is incomplete** | A tool picture and label change, but Help is written text. There is no stage-specific demonstration of pouring or stirring. | Add brief optional local speech and an in-context hand/tool demonstration. The food and tools must communicate the action even with sound off. |
| **F-07: saved food has no partial mixing/pouring model** | Persistent food records track recipe, integer step, ingredients, heat, portions and decoration coordinates. They do not represent batter consistency, partial transfer, layer assembly or preparation coverage. | Design those persistent states and their bounded transactions; do not hide all progress in local animation. |
| **F-08: previous tests proved a narrower result** | The 184 core checks and native recipe checks validate accepted commands, stock, portions, four cooks and continuity. | These checks remain valuable, but do not prove a satisfying cooking activity. “All fifteen paths pass” must not be read as “all fifteen cooking experiences are complete.” |

Source anchors: [recipe definitions and Next](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/Home/Kitchen.cs), [ingredient palette](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Home/GameScreen.KitchenView.cs), [worktop gestures](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Home/KitchenGesture.cs), [action routing and presentation](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Home/GameScreen.Kitchen.cs), [assisted authority operations](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/Home/KitchenAssistance.cs). The [current fifteen-recipe inventory](evidence/kitchen-flow-audit-2026-09-27/current-recipes.json) records the inspected definitions, not proposed completion.

## What the external research establishes

Sources were checked September 27, 2026. Developer descriptions establish advertised mechanics; they are not independent proof of toddler usability. No third-party app was purchased, installed or played in this pass. Website preview-image retrieval was incomplete, so the report does not claim an observed frame-by-frame walkthrough. Store reviews, download counts and age badges are not used as effectiveness evidence.

| Primary source | Relevant evidence | Application to this game |
| --- | --- | --- |
| [Pazu Pizza Maker](https://pazugames.com/games/24/) | Describes preparing dough, rolling, vegetable cutting, sauce preparation, toppings and oven baking. | The food changes through distinct preparation activities. Use this process structure for pizza, with our own art and forgiving interaction. |
| [Bimi Boo cooking, developer App Store listing](https://apps.apple.com/my/app/pizza-games-for-kids-cooking/id6447359628?platform=ipad) | Advertises step-by-step recipes and multiple interaction mechanics. Its July 2025 cake update specifically describes batter preparation through final decoration. | Changing a generic button's label is insufficient. Cake needs recognizably different batter, baking and finishing stages. |
| [Timpy/KidloLand bakery listing, app ID 6502041946](https://apps.apple.com/ph/app/kids-cooking-games-donut-maker/id6502041946) | Describes mixing, batter pouring and placement of decorations. Its current title redirects from the earlier Timpy bakery title; the same listing includes a save-state update. | Preserve the creation across stages and interruptions. Do not infer multiplayer support or exact timings from this listing. |
| [Dr. Panda Restaurant](https://www.drpanda.com/games/DrPandaRestaurant/index.html) | Describes separate slicing, frying, toasting, mixing and baking activities, then serving and pretend restaurant play. | Meals should use the appropriate tools and containers, then return to the shared house. |
| [Bubadu Pizza Maker](https://bubadu.com/games/pizza-maker-kids/) | Describes freely arranging toppings and visible browning/melting during baking, then serving/eating. It also deliberately permits unusual ingredients. | Keep visible consequences and creativity. Its broad novelty pantry does not justify making every ingredient the default for every named recipe. |
| [Cooking Mama, Office Create's listing](https://apps.apple.com/us/app/cooking-mama-lets-cook/id987360477) | Describes cooking as varied touch minigames, including chopping, baking and stewing. | Borrow the distinction between activities. Our family game does not need its scoring, rankings or restaurant economy. |
| [Toca Boca Jr/Kitchen 2, publisher listing](https://play.google.com/store/apps/details?hl=en_SG&id=com.tocaboca.tocakitchen2) and [Sago Mini Diner](https://sagomini.com/apps/diner/) | Toca emphasizes free food experimentation and reactions; Diner describes a recipe book and customer/employee pretend play. | Preserve optional experimentation and social serving, while a selected cake defaults to a sensible cake-making flow. Neither source proves four simultaneous human-player architecture. |
| [Hiniker et al., Touchscreen Prompts for Preschoolers](https://faculty.washington.edu/alexisr/TouchscreenPrompts.pdf) | A study of 34 children aged 2–5 found age-dependent understanding of prompts; under-threes in those tasks needed an adult model. Audio and demonstrations became useful with age. | Offer short speech and demonstrations alongside simple controls. Do not assume flashing objects or a tutorial paragraph teach the activity. These were unfamiliar gesture tasks, not tests of our recipes. |
| [Soni et al., TIDRC framework](https://init.cise.ufl.edu/wp-content/uploads/sites/378/2019/04/soni-et-al-idc19.pdf) | Synthesizes 57 recommendations and analyzes 50 apps. Relevant themes are immediate feedback, recognizable objects, real-world gesture mappings, forgiving targets and partial gesture acceptance. It explicitly discusses conflicting evidence. | A stirring motion should visibly stir; imperfect motions should still work. Guidance belongs beside the current action. Exact gesture thresholds and stage lengths require playtesting. |
| [W3C dragging alternatives](https://www.w3.org/WAI/WCAG22/Understanding/dragging-movements.html) and [Android touch targets](https://support.google.com/accessibility/android/answer/7101858?hl=en-GB) | Support a non-dragging alternative and a 48-dp touch-target baseline respectively. | Keep large direct-object tap alternatives. This is design guidance, not a Unity accessibility certification or a toddler-optimal size claim. |

**Design conclusion:** the common useful pattern is a short sequence of different food manipulations, visible consequences and a finish that can be shared. Our proposed stage order below is an authored game design informed by those sources and the user's requirements, not a copied or scientifically validated universal recipe.

## Default ingredients and stage order

When a child chooses a named food, show only ingredients relevant to that recipe **at that moment**. Cake mixing displays baking ingredients; cake finishing displays icing, fruit, chocolate and decorations. Patties belong to burger preparation. Cheese belongs to appropriate pizzas/meals, not the default cake palette. Hide unrelated inventory entirely in this flow, including further bowl pages. Recipe variants can still offer reasonable optional additions, without rigid “wrong topping” punishments.

Keep the older free-creation requirement through a deliberate secondary **Experiment** choice. That choice may expose unusual combinations, but must not silently activate in a normal cake session. Record the mode with the dish. Do not delete existing unusual saved creations or refuse to load them after filtering is added.

Every recipe distinguishes ingredients incorporated before cooking from items added afterward. Icing/popcorn are not required in raw duck-cake batter. Banana is a pancake finish unless a specifically authored banana-batter variant says otherwise. No universal “toppings” requirement at the heating boundary.

The default remains making food from the beginning. An explicit ready-base choice skips the disclosed preparation stages and enters the proper finishing activity. Assistance and ready-base skipping are different choices: help with stirring should still show stirring and produce batter.

## Concrete cake and pizza flows

**Cake:** choose cake → add baking ingredients to a bowl → mix into batter → pour into the chosen mould → bake and visibly rise → brief automatic settle/cool → assemble if needed → spread icing → place decorations → slice/serve.

**Pizza:** choose pizza → combine dough ingredients or explicitly choose ready dough → mix/knead → roll/shape → spread sauce → scatter cheese/place toppings → bake with visible melting/browning → cut → serve.

There is no mandatory long cooling timer, accurate measuring exam or precise circular gesture. Decorative stages remain open until the child chooses the pictured finish/serve action. Preparation stages complete only when their actual food condition is met. Transition after fingers are released so the ending touch cannot accidentally operate the next stage. The finished dish remains the same persistent creation throughout.

| Activity | What the child does | Visible change and completion | Accessible alternative |
| --- | --- | --- | --- |
| Add/pour | Tip a large jug or move an ingredient over the bowl | A stream/fill level moves from source to destination; the accepted portion transfers once | Tap the jug and bowl for the same visible pour |
| Mix | Scribble broadly inside the bowl; lifting/rejoining is allowed | Separate ingredients become streaky, then smooth batter; the spoon follows the action | Tap the spoon for an assisted stirring animation; remain in this activity until its result settles |
| Knead/roll | Press dough or sweep a rolling pin over it | Dough becomes a ball, then a broad base | Tap dough/pin to perform those same visible transformations |
| Chop | Sweep across large food pieces | Whole vegetables become bounded pieces that can be added | Tap the food/tool; broad snapping avoids precision cutting |
| Fill mould | Pour the same batter into a large heart/round mould | Bowl empties as mould fills; preserve mixture identity and quantities | Tap bowl then mould to transfer |
| Bake | Put the tray in the oven | Same cake rises/pizza browns; progress appears in the food and oven | Tap the oven picture to place the real tray and watch it cook safely |
| Spread icing/sauce | Move a spatula across the food | Coverage follows the touched region instead of only changing a label | Tap sauce/icing then food for an assisted spreading animation |
| Stack/build | Place large layers or duck body/head pieces | Pieces settle into forgiving supported positions | Tap a piece and target to place it |
| Decorate | Place visible toppings freely on the allowed surface | Their positions, size and layer remain through serving | Tap topping then food; the game chooses a sensible position |
| Cut/serve | Trace a broad cut or choose a plate | Portions separate and the original loses the served portion | Tap cutter and plate for the same conserved transfer |

This is not a proposal to demand repeated arbitrary taps until a hidden counter fills. Progress represents a food action—mixed content, transferred batter, spread coverage or positioned pieces. A generic Next button must not perform all activities. Exact progress thresholds, animation lengths and target sizes are prototype values to tune with the family, not facts supplied by the papers.

## All fifteen recipe contracts

The common activities above are reused, but each food keeps a distinct sequence and appearance. Preparation is simplified pretend cooking, not a real-world measured recipe.

| ID / food | Required process | Ingredient/appearance distinction |
| --- | --- | --- |
| PIZ-01 Cheese | Dough → roll → sauce → cheese → bake → cut | Sauce coverage and scattered/melted cheese; retain requested cheese-stretch polish in backlog |
| PIZ-02 Pepperoni | Same base → place pepperoni → bake → cut | Individual pepperoni pieces stay on their slices |
| PIZ-03 Vegetable | Dough → chop vegetables → sauce/optional cheese → arrange → bake → cut | Capsicum, mushroom and tomato change from whole to cut pieces |
| PIZ-04 Ham/pineapple | Dough → sauce/cheese → place ham and pineapple → bake → cut | Large pink/yellow pieces, no exact pattern requirement |
| PIZ-05 Silly face | Dough → sauce/cheese → arrange facial features → bake → cut | Child's actual face arrangement survives; album remains tracked separately |
| CAK-01 Duck | Batter → pour body/head moulds → bake → assemble → ice → beak/eyes/feathers → serve | Duck components are assembled after baking; popcorn-style feathers are finishing decorations |
| CAK-02 Chocolate layers | Chocolate batter → fill moulds → bake → stack layers with filling → ice/decorate → slice | Visible chocolate batter, baked layers, filling and exterior icing |
| CAK-03 Strawberry heart | Batter → fill heart mould → bake → pink icing → strawberries → slice | Heart shape appears in mould and finished cake; strawberries belong to finishing |
| CAK-04 Rainbow | Batter → choose/divide colors → pour/layer → bake → assemble/ice → decorate → slice | Child-selected color order remains visible in the cake and slices |
| CAK-05 Carrot | Chop/grate carrot → add to batter → mix → pour → bake → pale icing/carrot decoration → slice | Orange bits incorporated in batter, topping added after baking |
| MEAL-01 Burger | Cook patty in pan → turn → optional cheese melt → stack bun/patty/salad → plate | Lettuce stays a fresh assembly item; the entire bun is not baked as a substitute for patty cooking |
| MEAL-02 Spaghetti | Add pasta to pot → cook → drain/transfer → stir/add sauce → plate → cheese | Pasta, pot liquid, sauce and finishing cheese have distinct roles |
| MEAL-03 Soup | Chop vegetables → pour broth/add vegetables → stir/simmer → ladle | Pot and liquid fill, moving vegetables, conserved bowl servings |
| MEAL-04 Pancakes | Batter → mix → pour pan circles → cook first side → flip → finish → stack → fruit | Flipping happens during cooking; banana is offered with the finished stack |
| MEAL-05 Rice/vegetables | Cook rice or explicit cooked-rice option → chop/stir-cook vegetables → combine/scoop → finish | Rice and vegetable components remain visible; use pot/pan rather than universal oven behavior |

## Systems and artwork required to make the plan real

Use a stage definition per recipe with allowed inputs, tool/container, completion condition, visible food states and next-stage branches. The authority should validate the same stage contract the client presents. A raw client “advance step” command must not bypass ingredient transfers or preparation results.

Each persistent dish needs a recipe-definition version, stage ID, bounded stage progress, mixture/component contents, support/container identity and assembly/decoration placements. Track transferred quantities on both source and destination. A bowl pouring into a mould must not leave a second full batter behind. Baking and slicing preserve the accepted food and layers. Local animation follows this state and cannot invent a completed transfer.

During continuous gestures, send bounded action progress rather than a complete world write for every finger movement. Attribute contributions to the dish and acting profile, reject stale-stage/repeated operations, and preserve accepted partial work after interruption. The implementation must choose and measure appropriate update/save cadence; this research does not set an untested network rate.

Maintain four independent bowls/trays, essential tools and usable heating positions. Each player may be mixing, baking, decorating or serving a different creation. Local help/demo/audio does not control other screens. Optional cooperation on one dish serializes its ingredient/placement transactions; it never acquires a kitchen-wide lock. Closing a panel, leaving a room or losing a connection preserves the dish and releases only that player's temporary interaction. A shared dish's stage transition invalidates stale touches from its previous activity.

Required new presentation includes bowls with separate ingredients and batter states, pouring streams and fill masks, moulds, rising cake, separate baked layers, icing coverage, duck assembly pieces, chopped food, pans/pots and pancakes before/after flipping. The current inventory also lacks some proposed dough/batter staples such as water, sugar, butter and yeast; either add bounded real stock/art or explicitly define a premix. Do not pretend those ingredients exist, silently take them from unrelated food, or draw decorative packages that have no saved contents. Exact stock expansion must be specified in the implementation slice and migrated once.

Use layered sprites, simple meshes/masks and small bounded piece counts. No fluid simulation is required. Clip retained decoration to the actual shape and slice; a heart/duck/stack cannot use an unchanged circular pizza presentation. Keep quiet ingredient/tool sounds and optional short narration local, with mute/reduced-motion support. Runtime cloud voice generation is not required.

Older saves need recipe-version-aware migration. Preserve completed 166 food and unusual combinations as legacy creations, including portions and toppings. Convert unfinished old food conservatively to a compatible checkpoint without charging ingredients twice or deleting earlier work; test each legacy recipe/step, since the old icing-before-bake rule makes a single integer remap unsafe. Broader intermediate state requires rechecking maximum save/network size and A10 memory/performance. PC/VPS authority, private offline saves and no offline merge remain unchanged.

## Acceptance that prevents the same mistake recurring

| ID | Required observable result |
| --- | --- |
| FLOW-01 Ingredients | All 15 recipes show only their current-stage ingredients in default mode, on every page. A cake never offers patties/cheese by default. A stale/direct command cannot bypass the rule. Experiment mode is deliberate. |
| FLOW-02 Sequence | Cake can bake without icing; icing appears afterward. Pizza toppings bake with the pizza. Pancakes flip during pan cooking. Burger lettuce is assembled after patty cooking. |
| FLOW-03 Real action | Mixing, pouring, rolling, spreading and stacking have distinct visible intermediate states. Repeated generic action-button taps cannot complete the default recipe without these activities occurring. |
| FLOW-04 Assistance | Each required drag has a usable direct-object tap alternative that visibly performs the same action. Long contact, interrupted strokes, off-center input and a second finger do not skip stages or duplicate ingredients. |
| FLOW-05 Persistence | Exit and cold-reopen during mixing, pouring, baking and decoration. Recover the same partial creation, transferred stock and placements. No full source batter survives a completed transfer. |
| FLOW-06 Four players | Four clients work at different stages/recipes; one leaves or disconnects. Include two cooks competing for the last unit and two cooperating on one dish. Preserve all other creations. |
| FLOW-07 Finish | The child sees the same colors, ingredients, layers and toppings after baking/cutting/carrying. Serving conserves portions; washing cannot erase food. |
| FLOW-08 Guidance | A non-reading child can identify bowl/tool/target using pictures and optional spoken/demo cues. Muting works. Observe first use before giving an adult demonstration; record assistance rather than assuming independence. |
| FLOW-09 Device/save | Final source-matched builds, phone and A10 visual/performance checks, old-world migration, background/resume and server/private rejoin. Native tests alone do not close physical acceptance. |

## Bounded build order

1. **One complete chocolate cake:** define correct batter/finishing ingredients, then build real bowl mixing, mould pouring, visible bake, layer assembly/icing and serving. Include four cooks, partial saves and legacy migration. Keep other recipes available as existing prototypes; do not claim they inherit completion merely by sharing the controls.
2. Apply the common activities to the other four cakes, including heart/rainbow/duck-specific geometry and assembly.
3. Complete the pizza family with real dough preparation/rolling/spreading and retained topping arrangements; retain the clear 166 worktop composition.
4. Add the distinct pan/pot flows for all five meals. Do not route these through the generic oven to claim completion.
5. Complete physical first-use and mixed-device acceptance, with observation-driven polish. Orders, album, picnic packing, drinks and wider Home work remain tracked separately.

The first deliverable should be a **playable cake sequence demonstrating different actions**, not another rearrangement of buttons or a renamed step list. All FLOW checks remain open until implementation supplies the corresponding evidence. H-07–10 / COOK-01 remain Partial.
