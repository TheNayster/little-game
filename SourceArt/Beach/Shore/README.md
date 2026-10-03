# Beach shoreline and sea visitors

**Latest user correction:** the runtime now uses `mermaid-poses-v2.png` with the requested light-brown skin, plus `whale-poses-v2.png` and `dolphin-poses-v2.png`. Each sheet supplies emerging, airborne and re-entry drawings; its `.ora` has three named editable pose layers, and [frames-v2.json](frames-v2.json) records measured bounds. [Generation and layout prompts](pose-prompts-v2.json) · [Model-sheet review](../../../docs/implementation/sea-visitor-models-2026-09-30.html). The original single sprites below remain historical references.

Generated with the built-in `image_gen` tool for BCH-03 and the user's September 30 request for random whale, dolphin and mermaid jumps. [Exact generation and cleanup prompts](prompts.json).

- `whale.png`, `dolphin.png`, `mermaid.png`: selected transparent cutouts, copied unchanged to `Resources/Worlds/Beach/Art/`. A clipped authored arc and rotation animate each complete drawing; these are first artwork candidates, not family-approved likenesses or multi-pose swimming animations.
- `beach-dunes-shore.png`, `beach-rockpools-shore.png`: edits of the existing scenery, opening the shoreline and clearing painted foreground obstructions. Copied unchanged to sibling `Resources/Worlds/Beach/Scenery/` names; original panoramas remain available. The shoreline is near half height so its shallow strip matches the walkable floor.
- Each `.ora` contains the corresponding original PNG as a named editable layer, created by [prepare_layers.py](prepare_layers.py) without altering source pixels. Foam and ripples are runtime UI meshes, not baked into these drawings.

The PNG alpha range can top out at 254; fully transparent pixels are alpha zero. Native screenshots verify the actual composite. The three key poses now change limbs/tail/hair; future polish can add intermediate frames and creature splash sounds. Model-sheet extraction preserves original pixels; it only measures bounds and stores each pose cell as an editable layer.
