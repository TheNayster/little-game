# Markets source inventory — October 16, 2026

Owner **BLU-01**, goal IDs WORLD-02 / FAMILY-01 / ITEM-02. This bounded inventory supports the [Markets phase plan](bluey-markets-world-phase-plan-2026-10-16.md). It separates recovered assets and serialized controls, observed original-game footage, and explicit family adaptations. Packaged Little Weeps validation belongs in the phase plan; deployment and owner acceptance remain separate.

## Provenance and retained inputs

Source package identity: `com.storytoys.lego.bluey.googleplay`. The retained task root is `LocalData/Research/DuploMarvel/world-tests/Markets/`; Bluey provenance remains separate from the parent directory's historical name. Catalog-reachable dependency URLs use `7.1.0/Android`. No currently installed app version, purchase/unlock or original-runtime behavior is inferred from that URL.

| Retained APK | SHA-256 from input receipt |
| --- | --- |
| base.apk | `b639745448931fb8864c54a0e90f696aec0fdd345c97c740595972bf3ce302e7` |
| split_config.arm64_v8a.apk | `d7c0d83aeef3a86e5f4ae07823e327224636e66ac96d63103c36695786b1ca21` |
| split_UnityDataAssetPack.apk | `2d495d790acc40189d8142920526ef066f18940b152f3878bba604805036577a` |

Retain `input-receipt.json`, `download-receipt.json`, `catalog-closure.json`, extracted `input/`, downloaded bundles, both exports and task-owned `source-findings/` while source/packaging consumers reference them. Raw packages and recovered media stay ignored. The editable export is `bluey-markets-export/ExportedProject`; the base export supplies empty/default IL2CPP declarations. `source-findings/serialized-components.json` retains68 exact relevant component blocks with resolved external GUID references (SHA-256 `e1bdb58112dd6e84fe6af045f56115e47c6a4ecdc5434c2d214eb98e373f0731`). This inventory was produced by bounded file inspection; it does not certify every exported dependency or animation. See the separate main inspection/closure receipts for that scope.

## Exact scene identities

Paths below append to `bluey-markets-export/ExportedProject/Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Scenes/`. Hashes identify these retained exported YAML bytes, not original bundle hashes.

| Scene | Purpose | SHA-256 |
| --- | --- | --- |
| SCN_00_MarketDay.unity | Pack shell, transitions, backdrop/camera, LongDog and audio coordination | `6d495c86a6097d7263735734b9fb301130dbb9cd255be019fe6a46408e6912c8` |
| SUB_SCN_01_MarketDay_Scene1.unity | Smoothies, food/flowers and bubbles | `22039adef2faad8ad5e0fff1d1ccd80fa016a332acb62a8b0cc302a560fdb884` |
| SUB_SCN_02_MarketDay_Scene2.unity | Pony care and makeover | `a2f6fb4e0cb1fd1005c6b61dfbc5585ec133100932b151c9934988eb1bf45ff5` |
| SUB_SCN_03_MarketDay_Scene3.unity | Ice-cream building/orders | `4f13bca9187a5c911888426f76382114a5594cbb27ff766ef4d72b01ef7297b8` |
| SUB_SCN_04_MarketDay_Scene4.unity | Busker rhythm, guitar and dancers | `d0b4301083a959c28212e00c247d626189985667ace613923f234542ef738838` |

Scene1 contains three overlapping station roots at origin: smoothie Transform3405/GO1823 disabled, flowers Transform2244/GO1878 disabled, bubbles Transform3008/GO2119 enabled. Each can be baked separately from an isolated asset-only copy; loading Scene0 alone does not present all stations. Scene1 CameraRef2129 is position(0,4.57,-17.13), rotation quaternion(0.09135462,0,0,0.9958185), perspective FOV20. The camera target is a camera pose, not the scene origin.

Scene2 controller3000 binds TentEnvironmentRoot Transform1223/GO301 and OutdoorEnvironmentRoot Transform1262/GO342, both serialized at(50,0,0). Makeover2977 uses animator2663/GO545 `pfb_mkd_PonyModel`. Outdoor ponies are animator2661/GO843 `pfb_emb_Pony_Grey` and animator2662/GO1015 `pfb_emb_Pony_Brown`. The owner names Buttermilk/Biscuit; this inventory does not independently establish the generic source prefab-to-name mapping. Family actor roots are animator2664/GO1072 Indy and animator2665/GO761 Bluey. Source rigs remain retained even where a bake hides them for the game's existing approved 2D cast.

## Required and discovered activity coverage

| Activity | Recovered serialized/declaration evidence | Family adaptation / unresolved behavior |
| --- | --- | --- |
| Smoothies | Mixer open/close/on/off/finish/cancel; cup ingredient peg and mixed/drink state; apple, banana, carrot, grapes with individual mixing colors | Shared bounded preparation/product ownership; exact original formula, capacity, sequence and duration unresolved. Hotdog implements mixable too; no ordinary recipe assumed. |
| Flowers | Six colors: coral, green, lavender, pink, red, yellow; flower/table pegs, stack/spin, patch/watering-can prefabs | Cooperative bouquet/stack and handoff. Exact watering behavior is a candidate, not proven from a prop alone. |
| Cakes | Pink/purple/yellow cupcake spawners, consumable class and cake stand | Shared serving/eating and fair turns. No recovered baking recipe claimed. |
| Bubbles | Two wands, two buckets/spawners, five letter bubbles, four bench pegs; drag-spawn distances0.4/0.5; pop-reaction constant8 | One shared bounded field, authoritative pop arbitration and fair wand turns. Exact source lifetime/letter semantics unresolved. |
| Both ponies | Food/Water/Brush/CleanArea enum; apple/carrot, bucket, brush/suds, pitchfork/poop; both food/water contact pegs; eatDuration4.03 | Four shared care contributions, washing, independent interruption and deliberate completed-care replay. Timed source need recurrence unresolved; no speculative riding feature. |
| Pony makeover | Five body colors; six linked mane/tail choices; six saddle decals; turntable/rotation, curtains, hoover/reset, particle/VO hooks | Persistent per-pony appearance, exclusive turns and two-dimensional contact. Exact original unlock/reset rules unresolved. |
| Busker | Three rhythm buttons; guitar strings; dancer spots/manager; 120 BPM; three songs with intro plus guitar/bass/drums/piano/sax layers | One shared song/score/layer progression with independent joining/leaving. Empty static arrays do not reveal exact level thresholds. |
| Ice cream, discovered Scene3 | Three customer orders bind exact JSON blueprints and icons; two worker minifigs; fourteen dispenser variants, six example stacks, inspiration panel, hoover, handoff/toss/splashes | Shared ordered build and completed product. Linear family recipe adapts the original three-dimensional attachment topology. |
| Incidental | Signs/subscene swapping, umbrella/parasol, flag spin, benches/stool, food handoffs/eating, hotdog Bandit, register wobble, LongDog coordinator, pony reactions and makeover rotation | Each shipped interaction needs its own visible/input check. Presence alone does not prove original timing or completion. |

## Exact recipe and appearance matrix

Blueprint files are under `Data/Scene3/` beneath the pack prefix. Preserve source IDs, including spelling quirks, when matching inputs.

| Order | Exact ordered BrickId sequence |
| --- | --- |
| GiantChocolateIceCream | Base_LightPurple → Scoop_ReddishBrown ×3 → Swirl_ReddishBrown |
| SundaeceCream | Base_MedAzure → pfb_mkd_byo_HalfInverseBow_Reverse → HalfInverseBow_White → coop_LightPurple → Scoop_FlameYellowishOrange → 2x4_Plate_ReddishBrown → Scoop_ReddishBrown → Berries_BrightRed |
| MangoIceCream | IceCream_Cup → Swirl_FlameYellowishOrange → Scoop_FlameYellowishOrange → Swirl_FlameYellowishOrange → Scoop_FlameYellowishOrange → Swirl_FlameYellowishOrange |

The Sundae plate has two supports in the JSON topology; a linear sequence is an explicit adaptation. The source typo `coop_LightPurple` identifies the light-purple scoop. Dispenser variants additionally permit source free construction; presence is recorded separately from the three checked order recipes.

Makeover body colors are off-white, tan, gray, pale gray and dark gray. Mane/tail choices are brown, pink, purple, teal, gray and unicorn; the unicorn mane enables an accessory, and mane/tail components link to one another. Saddle decal indices0–5 resolve to `tex_mkd_Pony_Body_01`, `_03`, `_04`, `_05`, `_06`, `_02` in that order. Source decals are retained; descriptive pattern names require visual inspection.

## Evidence limits and consumers

The source-informed adaptation preserves four family contributors, one shared result/session, finite exclusive ownership and independent departure. Claims of original executable rules, contact/motion fidelity, audio quality, child usability, packaged readiness, device installation and live-server rollout require separate evidence. No original app purchase/account/unlock change is part of this recovery.

Art/audio provenance and editable-input boundaries are maintained in [SourceArt/Markets](../../SourceArt/Markets/README.md) and [SourceAudio/Markets](../../SourceAudio/Markets/README.md). Current rules are `Code/Core/Worlds/Markets/MarketsRules.cs` beneath the existing Unity asset root; integration/build/runtime acceptance stays in the phase plan. This document creates no new delivery or approval gate.

## Actual gameplay and button-layout research — October 16

The owner's reminder prompted a separate observation pass before final delivery. Browser playback of [DC TUBE's Markets playthrough](https://www.youtube.com/watch?v=pF6xmKOZvLo) provided actual rendered original-gameplay evidence. The recording's installed version and input device are not established. It does not prove behavior of the retained 7.1.0 package or allow measuring touch coordinates precisely. A second [Markets and Park playthrough](https://www.youtube.com/watch?v=FfjjoQmQlK8) independently shows the outdoor pony scene and lower-edge arrows. No account, purchase, entitlement or original installation was changed; no ADB device was connected.

| Observed footage | Actual visible layout / behavior | Implementation implication |
| --- | --- | --- |
| First recording, approximately 0:45–1:05 | Horizontal large pack cards, Markets pony/Bluey/Indy picture and a large cyan circular Play control | The existing Little Weeps world chooser remains its navigation shell; do not claim the custom chooser is the original UI. |
| Approximately 1:54–2:52 | Bubble buckets/wands in the foreground, bench/family behind, large irregular/letter bubbles; white translucent arrows at lower screen edges | Use scene objects and edge navigation rather than eight persistent top tabs and a bottom text-action toolbar. |
| Approximately 3:49 | Six flower colors and three cupcakes on the stall, parasol and stool/Indy, wooden navigation sign | Pictured plant/cake actions belong in the scene. The cooperative bouquet, queue, carrying ownership and preparation limits remain family adaptations. |
| Approximately 4:47–6:50 | Outdoor ponies, apples in a crate, carrot/vegetable holder, blue water bucket, wash/suds bucket and pitchfork; characters approach and use objects; hearts and a manure block appear | Use recognizable pictured care tools near the ponies; show the held object at visible animal contact. Exact original recurrence/unlock logic and gestures cannot be established from these frames alone. |
| Approximately 7:35–7:50 | Very tall multibrick ice creams on a studded foreground rail, customer thought-bubble reference picture; pieces can form oversized stacks | Preserve original brick pictures/reference recipes and free building. The shared 4×4 editing board and linear recipe validation are explicit 2D family adaptations, not an exact recreation of the 3D attachment system. |
| Approximately 8:44 | Five family dancers; radio at upper left; large guitar occupies the lower third, with three horizontal strings | Give strings a large touch/drag region and keep radio/song controls pictured; a small generic Guitar menu button is insufficient to represent this control. |

The frame observations establish visible objects, arrangements and animation outcomes, not the unseen player's complete gesture sequence. Source declarations independently bind draggable inputs, wand drag thresholds, three guitar strings and rhythm targets. A tap alternative is an accessibility/family adaptation, not evidence that every original action is a tap.

The ignored audit `source-findings/actual-layout.json` retains exact scene/camera identities, 140 scene-owned input components, 60 RectTransforms, inherited input declarations and built-in UI bindings. Its `sourceLayoutSummary` and `differences` separate facts from inference. Specific serialized controls include shell Home at top-left, lower-edge arrows (128×196, bottom 1%), Scene1's Bubble → Smoothie → Food & Flowers signs, makeover Back/top-right hoover and two rotation buttons, ice-cream top-centred palette ScrollView (715×150), and the music panel's three horizontal strings. Static music target positions have alternatives and offsets; they are not proof of the active timed layout. The makeover controls were verified statically, not observed executing in the sampled footage.

No distinct busker actor is bound in the recovered music scene: its manager controls five family dancers. The extra layered Bandit-with-guitar performer in Little Weeps is an explicit family adaptation. It is not the television episode's Busker character, nor a recovered original performer identity.

The first Little Weeps interface used custom top station tabs and a wide bottom action toolbar. That was a functional prototype, not source-layout acceptance. The final UI revision uses lower-edge scene navigation, pictured contextual objects, a large guitar/string area, a visible top brick palette and a separate compact family-sharing drawer. Scene chooser access, fair queues, four player badges and authoritative ownership remain deliberate Little Weeps adaptations.

## Incidental-object disposition

The confirmed standalone discoveries implemented as shared interactions are the station sign, parasol and hidden Long Dog. Food handoff/eating and pony finish reactions belong to their existing activity loops. Benches/stool, register, flag and hotdog-stall scenery remain in the recovered scene pictures; a serialized peg, prop or candidate sound alone was not promoted to a separately verified game or shopping quest. Their original incidental animation/timing is not claimed. The static makeover Back/rotation/hoover controls are layout evidence; the 2D appearance panel adapts them rather than claiming a recovered 3D turntable or original reset/unlock policy. No confirmed additional round-based game was found beyond the tracked activities and ice-cream construction.

The final ice-cream backdrop is rendered from Scene3's actual stall and studded rail, replacing the prototype's flower-stall substitution. Its shell backdrop is staged with a neutral grass plane and original tree/bush drawing because original runtime parallax/floor setup is unavailable in the data-only probe. `Baked-icecream-v3/icecream.json`, the maintained baker and final content receipt identify this explicit composition adaptation; it is not represented as an original-app runtime frame.


Final held-food correction: the initial crate/ingredient picture obscured the child during feeding. The dedicated source `Pony_Scene2/pfb_mkd_PonyApple.prefab` is now baked separately (`Baked-pony-props-v1`), receipt-bound as `Art/pony-apple.png`, and paired with the existing raised-arm character pose. The final pack has570 files. This is source data plus adapted 2D contact staging; no original executable care animation or chewing sequence is claimed.
