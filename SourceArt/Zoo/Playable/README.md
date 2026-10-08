# Playable Zoo artwork

## Elephant care pilot � October 8

`elephant-care.svg` is the editable source for the care basket, soft brush and dust patches. Runtime native geometry and retained brush motion live in `GameScreen.ElephantCare.cs`; the existing elephant art view supplies a separate appreciative reaction. Elephant care owns these assets; retire them only after those runtime consumers migrate to a reviewed replacement. The existing clinic contributes only its reusable touch surface and is not modified. [Controls, authority and live-review evidence](../../../docs/world-audits/zoo/elephant-care.md).

## Elephant water pilot — October 8

`elephant-water.svg` is the editable source for the separate left-side pump/pool, water surface and floating leaf. Runtime geometry lives in `GameScreen.ElephantPlay.cs`; `ElephantArtView.cs` articulates the original resting drawing without modifying atlas pixels. The elephant pilot owns these files; retire them only after their runtime consumers migrate to reviewed replacements. No new raster atlas, import dependency or all-species art expansion. [Scheduling, source paths, actual native checks and owner-review limits](../../../docs/world-audits/zoo/elephant-personality-water.md).

All sixteen starter exhibits now have transparent eight-pose runtime atlases: four savanna animals, four living dinosaurs, four aquarium species and four reptiles. The [runtime manifest](runtime-manifest.json) maps each species, food, eating socket and exact source PNG to its Unity resource. The [research model sheets](../ModelSheets/README.md) and [connected Zoo map](../Layout/README.md) remain the design references.

Generated with the built-in `image_gen` tool. [First-slice prompts](generation-prompts.json) preserve entrance/elephant/giraffe generation; [expansion prompts](expansion-prompts.json) preserve all fourteen species, four habitat backgrounds and three atlas repairs, with final source paths and hashes. Original PNG outputs remain intact. Runtime scenery has a unique resource ID per exhibit even when a habitat background is reused.

Eight equal square cells, four columns by two rows: four locomotion poses, rest, notice, reach and chew. Penguin/crocodile movement uses land poses below the water boundary and swim poses above it. Clients mirror the motion direction and add restrained breathing or floating. T. rex, Triceratops and zebra shark use repaired `-v2` atlases whose full tails fit each cell. The original elephant UVs exclude nine horizontal edge pixels to remove the neighboring trunk artifact. Elephant feeding uses the intact curled-trunk frame because the reaching trunk still extends past its source cell.

The gate uses four large round picture choices with an elephant, long-neck dinosaur, crocodile and clownfish. Gate views appear only at the entrance and now share the small navigation stills described below. Animation atlases load for at most three nearby exhibits, then release on travel. Backgrounds contain no painted animals or usable buckets; food, barriers and four offering positions are separate interactive objects. Eating sockets are calibrated from the native feeding captures.

These are generated playable drawings, not family-approved final likenesses or editable vector/layered animation sources. Further production cleanup can build on these retained PNGs and prompts; it does not leave any of the sixteen starter species unimplemented. [Implementation and checks](../../../docs/implementation/zoo-world-research-2026-09-30.md).

## Picture navigation — October 8

`Resources/Worlds/Zoo/Navigation` is owned by the Zoo navigation milestone. Its sixteen 160×160 stills are derived from the first resting atlas cell by `Code/Editor/ZooNavigationArt.cs`, using Unity's texture APIs. The original editable atlas sources above remain intact. The entrance, destination strip and informational map consume these small shared textures; full animation atlas residency remains bounded separately. Retire this folder only after all three consumers migrate to a replacement portrait set. Delete an individual generated still only when intentionally regenerating it from its retained atlas source.

The wooden entrance gate, directional arrows, folded map and location pointer are editable native UI geometry in `Code/Client/Worlds/Zoo/GameScreen.ZooNavigation.cs`; they add no raster generation, speech dependency or new artwork style. The same elephant/dinosaur/crocodile/clownfish symbols identify each trail at the entrance and on the map. Owner review of navigation remains separate from the approved elephant feeding pass.
