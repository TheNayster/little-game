# First playable Zoo art

Five generated draft PNG assets for the first Zoo slice: entrance, elephant and giraffe scenic bases, plus two transparent eight-pose animal atlases. Generated with the built-in `image_gen` tool; [exact prompts, revisions and final save paths](generation-prompts.json). Runtime copies live under Unity's `Resources/Scenery`, `Resources/ZooArt` and `Resources/WorldMenu`.

The first elephant atlas spilled a neighboring trunk into its final cell. The giraffe's long reach also crossed a cell edge. Both were replaced with compact poses in square cells; the elephant runtime UVs additionally exclude nine pixels at either horizontal cell edge. The source PNGs remain intact. Animation compilation is not family visual acceptance; these are editable generated drafts and still need layered production cleanup.

Eight poses per species: four walk phases, idle, notice, reach, chew. Habitat art contains no painted animals or usable buckets. Food buckets, portions, four offering positions and barriers are separate interactive objects. Animals load only in the savanna and release their textures on departure.

The [16-species model references](../ModelSheets/README.md) and [whole-zoo proposed map](../Layout/README.md) remain the expansion plan. Only elephant and giraffe habitats are playable in this first slice. Animal vocal assets, final layered animation sources, the other 14 species and the other trails remain unfinished.
