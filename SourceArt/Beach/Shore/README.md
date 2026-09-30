# Beach shoreline and sea visitors

Generated with the built-in `image_gen` tool for BCH-03 and the user's September 30 request for random whale, dolphin and mermaid jumps. [Exact generation and cleanup prompts](prompts.json).

- `whale.png`, `dolphin.png`, `mermaid.png`: selected transparent cutouts, copied unchanged to `Resources/BeachArt/`. A clipped authored arc and rotation animate each complete drawing; these are first artwork candidates, not family-approved likenesses or multi-pose swimming animations.
- `beach-dunes-shore.png`, `beach-rockpools-shore.png`: edits of the existing scenery, opening the shoreline and clearing painted foreground obstructions. Copied unchanged to sibling `Resources/Scenery/` names; original panoramas remain available. The shoreline is near half height so its shallow strip matches the walkable floor.
- Each `.ora` contains the corresponding original PNG as a named editable layer, created by [prepare_layers.py](prepare_layers.py) without altering source pixels. Foam and ripples are runtime UI meshes, not baked into these drawings.

The PNG alpha range can top out at 254; fully transparent pixels are alpha zero. Native screenshots verify the actual composite. Future polish can add flipper/tail pose changes and creature splash sounds.
