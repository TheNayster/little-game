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

The local pack includes four environment backdrops, a72-frame wheel with216 projected seat samples, source actor/hand/face pivots, five prop captures including the rod,15 original slide photos, four prize pictures, three painting references and eight decoded audio clips. Content-v6 has292 files /73,934,054 bytes. `funfair-assets-v6.json` records exact package hashes outside the pack. Audio is converted to mono22,050Hz PCM for the existing loader; conversion is not subjective listening approval.

Recovered media, raw APKs, exports, native bytes and local saves remain ignored. No official-app purchase state was modified. The isolated `LocalData/Worktrees/PeppaFunfair` checkout exists to avoid the active Markets/Drive edits. Its consumers are this task's build/tests and final integration; retire the checkout only after source integration and preserving required ignored builds, saves and evidence. No live family authority, device install, signing or security setting is part of this task.


## Original gameplay and button-layout audit — owner follow-up

The [publisher listing](https://apps.apple.com/us/app/lego-duplo-peppa-pig/id6474300843?platform=ipad) establishes the activity set. Original-app gameplay in [Amusement Park with Peppa and Friends](https://www.youtube.com/watch?v=iCUJRjYvS9g), published August19,2025, was inspected in the browser at the wheel (about1:40–2:00), flower pond (about3:04), balloons/slide (about4:24) and painting (6:09–6:25). These are viewed frames/segments, not a claim to a complete watched playthrough or a recovered executable original UI. A similarly titled2024 compilation predates this pack and was excluded. Physical LEGO sets and other Peppa apps were excluded.

| Control | Shipped evidence | Correction / retained adaptation |
| --- | --- | --- |
| Navigation and rides | Footage shows a largely unobstructed world with corner navigation arrows. FerrisWheel exposes HandleTap and drag callbacks; its additional seat touchables also forward input. | Added direct tap/drag on the rendered wheel. Boarding/common countdown and positive shared momentum remain family adaptations. The bottom action strip now shows relevant actions rather than all ten at once; Cast, Play and independent Leave remain family controls. |
| Hook a Flower | HookAFlowerController/FishingBehavior bind rod taps, flower taps and drag begin/end. Footage shows the character behind the pond, with moving flowers across the foreground. | Replaced the side-by-side cream booth with the recovered scenery, a character above the foreground pond and flower tap/drag input. The bounded zoom panel, deterministic moving targets, queue, 2D tether and persistent prizes remain adaptations. |
| Smoothie machine | RoundWeGoSmoothieMachine binds HandleTap, TrySpawnNewDrink and HandleOnDrinkTapped. It holds splashyDrinks and a drinkSpawnAttachPoint; this pack has no fruit-selection UI. | Main-world machine now makes a cup on tap; tapping again takes the prepared cup. Removed the invented fruit-menu route from the normal station interaction. Old recipe commands remain compatible with retained test saves. The1.5s shared preparation interval remains an adaptation. |
| Slide and balloon | SuperSlideSceneController binds door and minifig taps; tower-top character tap and stairs callbacks are present. Footage shows the central tower door and balloon play. | Shared queue and the three explicit pose buttons are family controls, not recovered publisher buttons. Original path, action clips and14s slide remain bound. |
| Painting layout | Scene4 RectTransforms: palette1021 at(-380.6,-180), size300×420; design selector1030 at(-380.6,210.5), size300×300; painting1015 with details/outline at(+180,0), size700×700; accept1017 at(472.1,-329), size170×130. GridLayout1069 fixes three rows with110×110 cells and20 spacing: two columns of six colours. Button1087 invokes NextConfiguration;1053 invokes Close. | Replaced eight bottom colours and three text style buttons with the source six-colour/two-column palette on the left, one cycling source icon above it, a large round stencil on the right, original design details/dotted outline and the lower-right green tick. Stencil geometry is a bounded8×8 shared brush adaptation; designs still reveal on the common recipient. |

The six RGBA palette entries are copied from FacePaintingPanel1038: pink(.9411765,.42745098,.47058824), orange(.8392157,.4745098,.13725491), yellow(.98039216,.78431374,.039215688), green(.34509805,.67058825,.25490198), cyan(.40784314,.7647059,.8862745), purple(.78431374,.3137255,.60784316). Source icon atlas UV rectangles are read from each exported Sprite asset rather than guessed. The original shader/RenderTexture painting and a separate painter for each style are not executed; the shared family draft uses one design, selected before drawing.

Reproduce the bounded serialized audit with `Tools/Verification/Inspect-PeppaFunfairUi.py --inspection <Phase0> --output <ignored-evidence>/source-ui-layout.json`. Content-v5 adds the original dotted outline and icon atlas to the existing media. Recovered style details also overlay the painted character; the fishing recipient is aligned with the pond edge. Hook release is queued across an in-flight aim command so a fast drag cannot lose its catch. Current-source native verification and final main integration are pending this correction; build593 evidence applies to the earlier layout only. Teacups remain the explicitly documented family adaptation above, not a claimed independently recovered publisher controller.


Build596 passes actual four-client pictured wheel tap/fast drag, pond drag, six-colour stencil, late rejoin, saved authority reopening and Hulk/HQ visits.597 passes the targeted face/cup depth captures and actual private reopen/authority join/solo return, without runtime exceptions. A four-client launcher attempt then exposed a2.9s startup frame gap and a protocol timeout on the fourth client. Synchronous large-atlas decoding/readback and the18M-element alpha loop are a likely contributor, inferred from the load path rather than a captured profiler trace. Content-v6 precomputes an exact bottom-up/MSB-first threshold mask, avoiding the18M-element Color32/LINQ pass;5,000 original PNG samples agree. Source compiles; repeat native launcher qualification is pending. Visible preview windows use physical controls, with fixture isolation restricted to hidden setup.


The repeat598 four-client launcher passes after the packed-mask change. Wheel-load diagnostics measure132–144ms across the four native clients; all four remain connected and render the recovered cast. These measurements establish the new load behavior, not the precise cause of the earlier unprofiled frame gap. The earlier timeout remains recorded.


Final combined source includes Markets, preserves the canceled world removal and uses schema61/content97/protocol3.685 passes Unity media/JSON gates and the representative four-client migration/ride/independent-departure/world-travel/authority-reopen check. The local Markets pack was copied only after all570 receipt hashes matched; it remains ignored. Earlier source-specific records retain their actual coverage and failures.
