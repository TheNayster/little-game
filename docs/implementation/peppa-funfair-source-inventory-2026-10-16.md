# Peppa Funfair recovered inputs and reconstruction decisions

PEP-01 / WORLD-02 / FAMILY-01 / ITEM-02. October 16, 2026. Implementation belongs to `Core/Worlds/PeppaFunfair` and `Client/Worlds/PeppaFunfair`. This record distinguishes shipped evidence from the family adaptation. The [phase plan](peppa-funfair-world-phase-plan-2026-10-16.md) records actual acceptance and delivery.

## Exact local package and recovery

Package `com.storytoys.lego.duplo.peppa.pig.googleplay`, version14.0.0/code537. Originals remain in ignored `LocalData/Research/DuploMarvel/packages/com.storytoys.lego.duplo.peppa.pig.googleplay`.

| Original | SHA256 |
| --- | --- |
| base.apk | `461712d9e40ee026f93b33be3c906d84f3a2d8d884395ee32f0faa3f7da41beb` |
| split_config.arm64_v8a.apk | `b8ebb5e3b919a6660f80caaa025cc29dbe6a981fd431f0dd6decdacc8b4c3e5a` |
| split_UnityDataAssetPack.apk | `5086205f225f511753df458db3b51b232abe1bffe6fb5baafd085eb59bdd72aa` |

REA inspected the asset-pack archive and extracted it with verified containment: 2,239 inventory entries, 2,238 materialized files and no integrity contradiction. Local receipt: `LocalData/Research/PeppaPig/world-tests/Funfair/Phase0/rea-artifact-evidence.json`. This is package evidence, not a claim to original editable C#.

The shipped Unity6000.0.58f2 binary Addressables catalog resolves one main scene and four activity subscenes under `Assets/LDPMain/AddressableAssets/Remote/Activities/DayAtTheFair/Scenes`. The bounded closure contains60 catalog locations and14 bundles. The exact referenced bundles were retrieved from the catalog's `d3bbsg7xf7g1x4.cloudfront.net` delivery paths. No missing keys or download errors remained. AssetRipper2.0.0 exported the asset project; publisher executable scripts were excluded from the isolated Unity probe. Handwritten field readers restore serialized colours and decals.

| Scene | Serialized objects | Activity |
| --- | --- | --- |
| SCN_00_DayAtTheFair | 11,422 | Common environment and audio |
| SUB_SCN_01_DayAtTheFair_Scene1 | 2,054 | Ferris wheel and smoothies |
| SUB_SCN_02_DayAtTheFair_Scene2 | 2,124 | Hook a Flower |
| SUB_SCN_03_DayAtTheFair_Scene3 | 6,263 | Helter Skelter, photos and balloons |
| SUB_SCN_04_DayAtTheFair_Scene4 | 1,174 | Face painting and reactions |

All five scoped serialized inspections report zero parse errors and zero unresolved asset dependencies. Peppa uses `DuploMinifig`, not Marvel's `MonoActor`; the generic inspector's zero actor count therefore is not an absent-cast result. The dedicated reader binds25 scene actor instances and seven source roles. Inactive, unused generic rig slots are separate from the renderers sampled for visible characters; their missing bone references are retained in the audit.

## Native address correction

Cpp2IL2022.1.0-pre-release.21 emits metadata-only dummy assemblies with address attributes. A custom plugin could not load under the current Windows environment; no security setting was changed. The built-in attribute injector succeeded.

For this ELF, the producer's reported RVA is actually the physical file offset. Treating it as a VA selected unrelated instructions16KiB earlier. The retained first `native-verified` directory is superseded. `native-offset-verified` instead reverse-maps the reported physical offset through the executable PT_LOAD segment and retains the real virtual address, original bytes, hashes and decoded instructions. Its820 selected ranges all decode, but containment and decoding alone do not establish semantics.

FerrisWheel.Update at file offset`0x203E70C` / actual VA`0x204270C` checks absolute momentum against the minimum threshold. RotateWithMomentum clamps momentum to maxSpeed, multiplies the angular step by acceleration and subtracts signed deceleration over delta time. AddMomentum clamps to both signs of maxMomentum. This corrected the first reconstruction's mistaken interpretation of acceleration as smoothing. Source tap direction, drag physics and preferred-angle control are more general than the bounded positive-spin family ride.

## Bound activities and deliberate adaptations

| Activity | Shipped bindings and data | Family adaptation |
| --- | --- | --- |
| Ferris wheel | Scene1 controller1928; wheel2059 on GO242; hinge transforms1049/1202/1213; seats1950/1991/2098. Tap momentum90, acceleration3.25, deceleration75, max momentum180, maxSpeed100, threshold45, angle correction33, preferred angles30/150/270. | Four shared boarding slots, one countdown and bounded cycle. Three original cabins remain; the fourth child shares one cabin with a visible horizontal offset. Separate rigid-body chair roots1334/1294/1000 follow the original hinge positions with upright rotation in the 2D sampling. |
| Teacups | The public app description names teacups, but the recovered four-scene pack has no separate teacup controller or scene. | An explicitly added four-player family ride reuses recovered cabin artwork. Shared platform motion, controls and cycles are reconstructed. This is not a recovered separate publisher ride. |
| Smoothies | Scene1 smoothie binding2066; original cup and mixer artwork; drinking/carrying clips. | Three shared fruit additions, blending, one prepared cup and original held drinking action. Siblings can hand off or drop props. |
| Hook a Flower | Scene2 controller2007, fishing2008, rod2188/GO770, hook2038, flower group2042, reward2208;15 attach spots, four original reward sprites, two-second reward display. | One shared fair queue, projected moving targets and deterministic prize types. Source cast/fish/pull/celebration clips, held rod and visible tether are used. The tether is a 2D simplification of the source's curved line. Persistent family prize counts extend the original temporary display. |
| Helter Skelter | Scene3 controller6134, slide6407, queue6135, photography6133, MrsRabbit6855. Thirteen authored segments total14 seconds; backward, belly and spinning clips and source photos are available. | Shared queue, three-second climb and source-duration descent sampled from projected Bezier control points. This preserves the authored path in the flat presentation, not the original 3D spline/occlusion engine. |
| Balloons | Scene3 dispenser, original balloon prop/rig, balloon effects and carrying/landing clips. | Shared stock, one holder, handoff/drop, bounded float and safe landing. Full original balloon physics is simplified. |
| Face painting | Scene4 controller1055, screen1049, painter surface and source head/face assets; style enumNone/Tiger/Dinosaur/FairyPrincess. Brush20, splat radius40, seven splats and minimum thickness10 are serialized. | One shared recipient and fair queue. Siblings paint a common bounded8×8 colour surface, then reveal the design on that character with a matching source reaction. This replaces the publisher's shader/RenderTexture painting pipeline. Designs persist; editing and occupancy clear safely on interruption. |

Recovered playable roles are Peppa, George, Danny, Candy, Mummy and Daddy; MrsRabbit is a resident. Source clips provide37 actions/views for several children, fewer for parents and MrsRabbit, totalling236 strips. Parents have no recovered fairy spin, and Mummy is not in the source slide cast; that activity requires another available role. Parents without a fairy spin use their source happy pose. Source models, controllers, curves and materials remain editable in ignored research; production uses the established 2D strips and anchors.

## Media, ownership and preservation

The local pack includes four environment backdrops, a72-frame wheel with216 projected seat samples, source actor/hand/face pivots, five prop captures including the rod,15 original slide photos, four prize pictures, three painting references and eight decoded audio clips. Content-v4 has289 files /71,473,497 bytes. `funfair-assets-v4.json` records exact package hashes outside the pack. Audio is converted to mono22,050Hz PCM for the existing loader; conversion is not subjective listening approval.

Recovered media, raw APKs, exports, native bytes and local saves remain ignored. No official-app purchase state was modified. The isolated `LocalData/Worktrees/PeppaFunfair` checkout exists to avoid the active Markets/Drive edits. Its consumers are this task's build/tests and final integration; retire the checkout only after source integration and preserving required ignored builds, saves and evidence. No live family authority, device install, signing or security setting is part of this task.
