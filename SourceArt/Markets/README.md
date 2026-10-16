# Markets art provenance

Owner: **BLU-01 / WORLD-02 / FAMILY-01 / ITEM-02**. Consumers: Markets world presentation, source-informed activity rules and candidate packaging. This folder records the editable source boundary; it does not claim completed artwork, original-app runtime validation or owner acceptance. Retain this record while Markets consumes the source material. Preserve ignored recovery inputs and receipts; do not delete them as generated clutter.

The retained LEGO Bluey Android package identity is `com.storytoys.lego.bluey.googleplay`. Recovery inputs, package SHA-256 receipts, downloaded dependency receipts and editable Unity exports live under `LocalData/Research/DuploMarvel/world-tests/Markets/`; the inherited `DuploMarvel` research directory name does not identify these Bluey assets as Marvel content. Source catalog dependency URLs use the `7.1.0/Android` content path. That path alone does not prove a currently installed app version or entitlement.

- `input-receipt.json`: SHA-256 of retained base, arm64 and UnityDataAssetPack APKs.
- `catalog-closure.json` and `download-receipt.json`: five exact Markets scene identities and dependency/package hashes.
- `bluey-base-export/ExportedProject/Assets/Scripts/Assembly-CSharp/LegoDuplo/MarketDay/`: type/field/method declarations; runtime bodies are empty/default stubs.
- `bluey-markets-export/ExportedProject/Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/`: editable meshes, textures, materials, animation clips/controllers, prefabs, scenes, blueprints and source audio. Preserve this export; bake from an isolated asset-only project.
- `source-findings/serialized-components.json`: 68 extracted scene/prefab component blocks with resolved external GUID references. SHA-256 `e1bdb58112dd6e84fe6af045f56115e47c6a4ecdc5434c2d214eb98e373f0731`.

The [source inventory](../../docs/implementation/bluey-markets-source-inventory-2026-10-16.md) records exact scene hashes, controls/variants, recipes and evidence limits. The [phase plan](../../docs/implementation/bluey-markets-world-phase-plan-2026-10-16.md) owns implementation and acceptance status.

## Art and activity matrix

| Source family | Recovered variants/inputs | Intended consumer |
| --- | --- | --- |
| Smoothies | Apple, banana, carrot, grapes; mixer/cups/splatter and drink anchors | Shared preparation and a single owned product |
| Flowers | Coral, green, lavender, pink, red, yellow; stack/spin, patches, watering can | Shared flower stack/bouquet and handoff |
| Cakes | Pink, purple, yellow cupcakes; consumable/spawner/cake stand | Shared serving and eating; no inferred baking recipe |
| Bubbles | Two wands, two buckets, five letter bubbles, four bench pegs | One shared bounded bubble field |
| Ponies | Brown/gray actors; apple/carrot, bucket, brush/suds, pitchfork/poop | Two authoritative care states, visible contact and fair turns |
| Makeover | Body five colors; linked mane/tail six choices including unicorn; saddle six decals | Persistent per-pony appearance |
| Ice cream | Three exact blueprints (Chocolate5/Sundae8/Mango6 pieces), fourteen dispenser brick variants and six stack examples | Shared ordered construction and owned completed creations |
| Busker | Guitar strings, three pictured rhythm buttons, stage/dancers/speaker effects | One shared music/dance session |
| Incidental | Signs, umbrella, flag, benches/stool, hotdog Bandit, register, LongDog | Bounded interactions preserving the source scenery |

The export has switched local scenes. In Scene1 the smoothie and flower roots are disabled and the bubble root enabled; all three occupy the same origin. Bake each intended root separately. Scene0 is the pack shell. Scene2 also contains a giant makeover display pony and outdoor ponies; separate its environment roots deliberately. Hide exact source human/dog avatar roots in the bake when the app supplies its existing approved layered 2D family actors. Preserve the source rig/animations in the retained export.

Four-player queues, cooperative contributions, exclusive item revisions, shared rhythm progression, deliberately replayed pony care, persistence and two-dimensional arrangement are **family adaptations**. Original controller execution, exact timers, mixing formulas, three-dimensional recipe topology, motion/contact fidelity and final visuals remain separately evidenced; source declarations and a successful bake cannot establish those results.

## Pony palette and sampled contact poses

`Tools/UnityEditor/MarketsPonyBake.cs` reads the actual makeover model's combined body material, primary/secondary/tertiary decal slots, source tints and UVs, separate tail and optional unicorn accessory. Its fixed512px camera keeps the reviewed142-degree view. The output contract is180 complete palette combinations (`pony-body-B-mane-M-saddle-S.png`),180 matching source head-down poses (`pony-eat-body-B-mane-M-saddle-S.png`) and six aligned tail images. Mane and tail remain linked; the baker uses a handwritten three-decal recovery shader and retains editable scene/rig/clip inputs.

The head-down source is `Remote/Shared/Animals/Duplo/Pony/Animations/ani_rem_Pony_HeadRotation_Down.anim` beneath the exported AddressableAssets prefix. It contains one real neck key at time0, not a recovered chewing or drinking sequence. The baker samples that clip on the actual makeover Animator root545, requires resolved bindings and changed bone transforms, and requires at least100 visibly changed pixels against each matching rest frame. It fails explicitly if sampling has no effect. The new outputs and source clip hash/sample time/bindings/anchors belong in `pony-variants.json` after a successful run. This code update alone does not claim the new images were rendered, reviewed or packaged, nor establish original food/water timing.

Recovered PNG/OGG packaging inputs remain ignored under Markets Resources Art/Audio; their tracked content receipt and editable-source baker/assembler provide provenance. Preserve these local inputs when integrating or moving the checked candidate. The isolated Markets checkout references the maintained research at the original game root rather than duplicating that export.

## Original-layout correction and Scene3 backdrop

The final controls were informed by actual Markets gameplay footage and the serialized UI audit, recorded in the maintained source inventory. The first functional prototype's flower-backdrop substitution for ice-cream building is superseded. `MarketsAssetBake.RunIceCreamView` opens the actual Scene3 at its camera(100,5.15,-20),9° pitch/FOV20 and renders its stall/register/parasol and studded construction rail. It loads the pack shell's backdrop additively and supplies an explicit grass/source-tree staging composition because the data-only probe cannot execute original parallax/floor initialization. This composition is a family adaptation, not an original-runtime screenshot. Original scenes remain unsaved and unchanged.

The retained reviewed output is `LocalData/Research/DuploMarvel/world-tests/Markets/Baked-icecream-v3/`. v1 lacked its shared backdrop; v2 had sky/clouds without ground; both remain historical capture evidence. `Assemble-MarketsPack.py` now requires `--icecream` rather than silently reusing flowers. Together with `--views Baked-views-v5`, `--prefabs Baked-prefabs-v1`, `--ponies Baked-pony-v3` and `--export bluey-markets-export/ExportedProject`, it produces the570-file receipt. Paths here are beneath the retained Markets research root unless explicitly rooted at Tools.


`Baked-pony-props-v1/pony-apple.png` is rendered from the dedicated original `Prefabs/Pony_Scene2/pfb_mkd_PonyApple.prefab`, not an enlarged ingredient-crate picture. The assembler requires `--pony-props` pointing to this output. The final570-entry receipt includes its hash, and the Unity prebuild check requires both that entry and its actual imported Texture2D. The object retains the source LEGO fruit geometry; its name is not evidence of a photorealistic single-apple design.
