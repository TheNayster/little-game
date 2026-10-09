# All sixteen Zoo animals — approved feature rollout

October 8, 2026. WORLD-02 / FAMILY-01 / ITEM-02. This record supersedes the elephant-only rollout limits in older pilot reports. Ball-return stories, floating story markers, player placement and feeding trays remain removed.

## Current implementation follow-up — October 8

This source work addresses the gaps recorded at **73137c2**, using the actual elephant control → authority action → rendered feedback paths. The rejected 532 completion claim remains historical below. **Implemented and rendered in still poses is not owner gameplay acceptance.**

Giraffe was the first full example: a forked browse canopy with independently moving leaves and a reaching neck response; pictured brush basket and separate put-away control; moving helper brushes, fading dirt and cleaning/finish sparkles; an illustrated woven weaver nest with flapping bird and opening seed pods; head acknowledgement/browse motion; and a pictured preparation table with ingredient editing and overhead carry. The same complete presentation approach is authored for the remaining fourteen animals, rather than stopping at giraffe.

| Animal | Habitat artwork/action feedback | Care surface/tool | Two illustrated animated discoveries | Personality articulation |
|---|---|---|---|---|
| Giraffe | Forked browse canopy, moving leaves and neck reach | Body brush, strokes/dirt/shine | Woven nest/weaver wing; opening seed pods | Neck acknowledgement and browse |
| Zebra | Textured scratch log, leaf rustle and rubbing response | Body brush | Hopping grasshopper; striped feathers | Head/ear acknowledgement, tail flick and rub |
| Lion | Rope-bound rolling enrichment toy and paw response | Habitat stone/cloth | Four-legged lizard; golden beetle | Head acknowledgement and front paw |
| Brachiosaurus | Tall forked canopy, leaf rustle and neck reach | Body rinse | Unfurling fern; amber crystals | Neck browse/acknowledgement |
| Triceratops | Low frond patch with leaf response | Body rinse | Seed cone; spiral fern snail | Head nod and tail |
| Stegosaurus | Fern fronds/rustle | Body rinse | Flapping dragonfly; illustrated leaf veins | Low head browse and tail |
| T. rex | Vented scent log with moving foliage | Habitat stone/cloth | Three-toe footprint pebble; amber beetle | Head sniff/acknowledgement and tail |
| Clownfish | Waving anemone arms and rising bubbles | Illustrated tank glass/cloth | Anemone shrimp; opening pearl shell | Hover acknowledgement and tail/fins |
| Blue tang | Coral fans/current bubbles | Illustrated tank glass/cloth | Five-arm reef star; coral shrimp | Reef head/tail motion |
| Zebra shark | Sand shell/grass/current bubbles | Illustrated tank glass/cloth | Sand shell; buried sea star | Gentle head and swimming tail |
| African penguin | Pool, expanding ripples and splash droplets | Body rinse | Clawed rock crab; tide shell | Head, flipper and waddle |
| Galapagos tortoise | Garden leaves and nozzle mist droplets | Shell rinse | Spiral snail; seedling | Slow head peek/acknowledgement |
| Leopard gecko | Layered warm rock, moving shade leaf and warm rays | Habitat stone/cloth | Six-legged beetle; faceted crystal | Head peek/nod and tail |
| Green iguana | Vine ledge, moving leaves and mist | Habitat stone/cloth | Six-legged leaf insect; seed pod | Head nod and tail |
| Nile crocodile | Reed bank and nozzle mist | Back rinse, fitted patches | Four-foot reed frog; water snail | Small head/tail basking response |

Every care view includes a waiting clock, separate put-away picture, up to four moving helper tools, immediate local stroke feedback with authority rejection rollback, three fading cleaning patches and a finish shine. Glass/stone is shown during care rather than as a permanent generic oval. Discovery hiding foliage opens and rustles; visitors rise and return, with fitted wing/pod/shell/hop/reef motions and existing discovery sound hooks. Habitat effects are allocated separately in front of the animal, while rooted habitat artwork remains behind it. Existing atlas poses remain; new head/tail/flipper mesh motion and retained neck/paw articulation add personality without replacing the artwork or feeding poses.

Each of all sixteen exhibits now owns its pictured preparation station and animal portrait. Ingredient controls, one-to-three pieces, individual removal, clear/cancel and overhead carry use the existing shared preparation/feeding workflow. Single-ingredient choices are centered. No feeding tray, transferable bowl, ball-return story or player-placeable decoration was added.

### Access and exhibit targeting

Header/navigation and activity visibility follow the exhibit in the camera view; the map's player marker still follows the player. Play buttons and tool baskets sit on the clear front path, with the preparation bowl to the right. The habitat prop is also actionable. Discovery hiding props sit inward on both sides. Every callback retains its explicit species/slot; tapping a distant exhibit uses ordinary walking before its authority-guarded action. Care joins through the basket, strokes use the three patches, and the separate arrow puts the tool away. Existing shared queue/player identity/phase cues, overhead eating, album, scenery and fossils remain.

### Evidence and remaining review

Unity MCP verified the exact current project, compiled the source with zero console errors, and rendered production exhibit views in an isolated **edit-mode still fixture** at 1280×800. All fifteen non-elephant play, care and two-discovery poses were inspected; giraffe preparation was also inspected. Captures caught and corrected discovery edge placement, browse canopy height, crocodile patch placement, hidden play controls, gecko shade effects and mist visibility. These are rendered layout checks, **not input/control tests, normal-speed animation approval, feeding contact verification or multiplayer gameplay evidence**. The fixture uses the production drawings/controller with a declared snapshot and has no gameplay commands, live session, recording or persistent save. Captures remain in ignored `Unity/FamilyPlayset/Library/LittleWeepsTools/ZooRolloutCaptures/`.

Standard release **533** built both client and dedicated server with **zero errors/warnings**, and passed its normal prebuild JSON gates. All **2783** Unity/Tools manifest inputs match the built source. [Build summary](../../../Builds/NetworkProbe/G3-0.0.533/build-summary.json), [source manifest](../../../Builds/NetworkProbe/G3-0.0.533/source-manifest.json), [build log](../../../LocalData/Logs/build-network-533.log). The release players were not launched during this task.

Content **81 → 82** records changed authoritative play/station approach positions and the gecko shade activity classification; schema53/protocol3 stay unchanged. No installed server, device or live family was changed. A future deployment must coordinate compatible clients/server.

No known omitted artwork or source behavior remains in the five requested categories after this pass. Actual control usability, feeding contact, sound, animation timing and shared gameplay remain **unverified for owner review**; a build or still image cannot establish those outcomes. No PC control, input automation, control tests, live demonstration, broad additional suite, recording or deployment was performed.

The new drawing, articulation and editor-only still-fixture sources belong to the existing Zoo client owner and remain while these activities are maintained. They use native editable UI geometry and retained animal atlases; no new package or per-animal controller copy is required.

## Historical correction at 73137c2: rollout incomplete after owner review

October 8 follow-up: the owner rejected build 532 because the other animals do not provide the elephant's features. The earlier claim that all sixteen animals were complete was premature. A source comparison of all sixteen catalog entries against the actual elephant implementation confirms that shared authority and basic client props do not establish feature parity. The following findings supersede the completion claims below; the original table is retained as a record of the attempted implementation.

| Trail | Animals checked | Result against the elephant |
|---|---|---|
| Savanna | Elephant | Existing baseline retained; this follow-up did not run gameplay or reassess owner acceptance. |
| Savanna | Giraffe, Zebra, Lion | Incomplete: generic play response, simplified care/discoveries, and player-dependent activity visibility. |
| Dinosaur Valley | Brachiosaurus, Triceratops, Stegosaurus, T. rex | Incomplete: the same generic activity presentation; the corrected Brachiosaurus feeding system and fossils are separate retained work. |
| Aquarium | Clownfish, Blue tang, Zebra shark, African penguin | Incomplete: current/pool props and cloth/rinse pictures exist, but distinct habitat effects, interaction presentation and reachable feeding contact are not established. |
| Reptile Garden | Galapagos tortoise, Leopard gecko, Green iguana, Nile crocodile | Incomplete: generic mist/shade props, simplified care/discoveries and small whole-body movements do not deliver the requested species adaptations. |

Concrete source findings:

- `TickZooActivities` enables the other fifteen animals' props only for `ZooAtPlayer()`, while animal/bucket visibility follows the camera. An animal visible after camera panning can therefore have none of its activity props. The single snack station also follows the player rather than each visible exhibit.
- The non-elephant habitat response rocks/translates its prop. It lacks the elephant's allocated animated droplets, ripple/leaf response and triggered effect sound. The displayed prop is at `Center - 560`, while the authority sends the animal to `PlayX` (normally `Center - 160`, or `Center - 130` for tortoise); the animal response is not fitted to the prop.
- Non-elephant care has shared progress and three targets but omits the elephant's waiting picture, separate put-away picture and immediate local tool-stroke feedback. The habitat-cleaning pane is a plain oval with three plain patches.
- Two discovery commands exist per species, but artwork is chosen by broad name matching rather than a fitted discovery per exhibit. For example, the lion's lizard falls into the generic six-legged visitor drawing and the iguana's leaf insect falls into the plant drawing. The new discoveries omit the elephant's rustling hiding foliage, multiple visitor arcs and triggered sounds.
- Personality adds a few parameter groups of whole-image rocking/bobbing using existing atlas frames; this is not a complete distinctive idle, acknowledgement and playful action for each species.
- Feeding queues and preparation commands exist in source. Compilation does not prove direct visible eating contact, habitat boundaries or usable access for all sixteen animals.

This follow-up inspected source and the owner's three screenshots. No fresh in-game inspection was performed: the project-checked Unity bridge reported no single connected FamilyPlayset editor. No gameplay injection, multiplayer session, build, device installation or server replacement was performed for this audit. The next implementation must address these gaps across every animal, not only change visibility.

## Original implementation coverage claim (superseded)

**I means implemented in source; it does not mean owner gameplay or visual acceptance.** Every row retains overhead bucket feeding, the four-position shared queue, player portraits, waiting/approach/eating feedback and the existing consumption/finish presentation. Every row has friendly acknowledgement, idle movement and a playful response. Each preparation workflow supports one to three pieces, removal, clear, cancel and serving through the same feeding queue. Configured foods below are pretend game foods.

| Trail | Animal | Feeding / finish | Personality | Habitat play | Optional care | Two repeatable discoveries | Snack preparation |
|---|---|---|---|---|---|---|---|
| Savanna | Elephant | I, retained | I, retained trunk/ear actions | I, retained water play | I, soft brush | I, garden bird; butterflies | I, leaves / hay |
| Savanna | Giraffe | I, retained neck contact | I, acknowledgement / browse sway | I, swaying browse branch | I, soft brush | I, weaver nest; seed pods | I, leaves / hay |
| Savanna | Zebra | I, retained grazing/finish poses | I, ear poses / grazing sway | I, scratch log | I, soft brush | I, grasshopper; striped feathers | I, hay |
| Savanna | Lion | I, retained resting/finish poses | I, friendly nod / curious rocking | I, rolling enrichment toy | I, habitat cloth | I, lizard peek; golden beetle | I, meat |
| Dinosaur Valley | Brachiosaurus | I, corrected size/neck preserved | I, slow acknowledgement / browse sway | I, canopy rustle | I, gentle rinse | I, fern unfurl; amber sparkle | I, leaves / hay |
| Dinosaur Valley | Triceratops | I | I, low browse / head acknowledgement | I, low browse patch | I, gentle rinse | I, seed cone; fern snail | I, leaves / hay |
| Dinosaur Valley | Stegosaurus | I | I, slow browse / curious sway | I, fern rustle | I, gentle rinse | I, dragonfly; leaf pattern | I, leaves / hay |
| Dinosaur Valley | T. rex | I | I, sniff poses / curious rocking | I, scent toy | I, habitat cloth | I, footprint pebble; amber beetle | I, meat |
| Aquarium | Clownfish | I, intact swim to held offering | I, hovering / swim acknowledgement | I, anemone bubbles | I, tank-glass cloth | I, anemone shrimp; pearl shell | I, pellets |
| Aquarium | Blue tang | I, intact swim to held offering | I, reef movement / swim acknowledgement | I, reef current | I, tank-glass cloth | I, reef star; coral shrimp | I, seaweed |
| Aquarium | Zebra shark | I, intact swim to held offering | I, sand resting / swim acknowledgement | I, sand current | I, tank-glass cloth | I, sand shell; buried sea star | I, fish |
| Aquarium | African penguin | I | I, preening / short waddle response | I, splashing pool | I, gentle rinse | I, rock crab; tide shell | I, fish |
| Reptile Garden | Galapagos tortoise | I | I, slow grazing / small acknowledgement | I, gentle garden mist | I, gentle rinse | I, garden snail; seedling | I, leaves |
| Reptile Garden | Leopard gecko | I | I, peeking / gentle acknowledgement | I, warm-rock shade | I, habitat cloth | I, pebble beetle; rock crystal | I, insects |
| Reptile Garden | Green iguana | I | I, climbing / gentle acknowledgement | I, leaf mist | I, habitat cloth | I, leaf insect; seed pod | I, leaves |
| Reptile Garden | Nile crocodile | I | I, basking / gentle acknowledgement | I, basking-bank mist | I, gentle rinse | I, reed frog; water snail | I, fish |

## Historical 532 access and layout

Choose Zoo in the places menu, then use the existing animal pictures, trail arrows or map. Near each animal, tap the fixed habitat prop on the left for play, the pictured care tool on the right for care, or either small hiding prop farther to the sides for discoveries. Care participation hides the local play/discovery controls; the care tool picture toggles joining and putting it away. Tap/short-stroke the three visible cleaning patches. For fish, lion, T. rex, gecko and iguana, those patches belong to tank glass or a habitat surface. Animals remain in their configured habitat routes.

The preparation table sits farther right of the food bucket. Tap its bowl to open the shared preparation UI, choose food pictures, remove individual pieces or clear the bowl, then choose carry. Food stays overhead on the player until authoritative consumption; the preparation bowl is not carried or used as a feeding tray. The original quick bucket route stays available. The existing camera/album and Dinosaur Valley fossil activity are preserved.

Manual review launcher, from the project root:

```powershell
python Tools/Launch/Review-ZooRollout.py 532
```

This uses the existing hash-checked launcher to open one normal-input player and an isolated loopback authority. It creates no real family enrollment, uses no installed server or personal save, and sends no scripted gameplay actions. The launcher was prepared but was not run during this implementation task. Closing the player ends its isolated session.

## Historical 532 shared implementation and compatibility

The elephant's flat JSON field names remain inherited on `ZooState`. Fifteen additional `ZooActivity` records reuse the same queue priority, play ordering, care sessions, membership epochs, rate limits and three-patch progress. Restore clears unfinished activity/food state, retaining animal feeding history, world identity and fossil progress. Each exhibit has its own visitor detection and runtime input rates. Departure releases only that player's membership; remaining helpers retain progress. Care and play yield to feeding under the existing bounded timing rules. Late join reads authority snapshots; timed effects interpolate between them rather than replaying old events.

Preparation remembers its exhibit, rejects stale edits and competing held fossils/toys, and leases an ordinary feeding ticket only when served. Shared/private separation and existing save schema remain. Content **80 → 81** records the new authoritative activities and foods; schema **53**, protocol **3**. This is source/build delivery only. Coordinated server/client deployment is a separate task.

New props and discovery/tool pictures are editable native UI geometry in `GameScreen.ZooActivities.cs`; existing licensed scenery, original animal atlases and elephant art are retained. There are no purchases, new packages or per-species controller copies. The new source belongs to the existing Zoo client owner and remains while these activities are supported; the reusable manual launcher belongs to `Tools/Launch`.

## Historical 531/532 verification and remaining review

Unity MCP confirmed the exact current FamilyPlayset root, an idle editor and a clean loaded scene. The initial compile found a snack-panel variable naming conflict; it was fixed, and the next console check contained zero errors. Standard release **531** built both client and dedicated server with zero errors/warnings and passed the normal prebuild JSON gates. Final **532** adds shared-effect interpolation, fossil/snack ownership guards, generic preparation names and the manual launcher. Final532 also built both release targets with zero errors/warnings and passed the normal prebuild gates. All 2777 Unity/Tools inputs match current source. [Build summary](../../../Builds/NetworkProbe/G3-0.0.532/build-summary.json), [source manifest](../../../Builds/NetworkProbe/G3-0.0.532/source-manifest.json), [build log](../../../LocalData/Logs/build-network-532.log).

No Computer Use, injected mouse/keyboard/touch, control tests, multiplayer test sessions, live demonstrations, recordings, devices, live-server replacement or DX12 investigation were performed. The normal build's in-memory gates are build evidence, not actual multiplayer or visual acceptance. All sixteen rows are implemented; exact visual contact, native layout, animation quality, habitat boundaries and owner gameplay acceptance remain for manual review. No animal was silently omitted or marked gameplay-approved.

Pre-existing untracked audit/research/evidence, root Packages/ProjectSettings and other local files remain untouched. Intermediate builds/logs are retained in their established ignored locations.
