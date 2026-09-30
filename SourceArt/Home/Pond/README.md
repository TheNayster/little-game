# Backyard pond artwork

Original generated game artwork: full-sized stone pond, clean garden background and eight painted koi/goldfish. The user's placement correction puts the pond in the open stretch between the trampoline and the blanket-covered picnic bench, centered at world x2350. It retains the original 1100×550 display size. These are adapted game assets, not an exact reconstruction or approved episode artwork.

`prompts.json` preserves the exact image generation instructions. `pond.png` is the transparent scenery sprite; `garden-clean.png` removes the old painted pond; `fish.png` contains eight right-facing fish in a 4×2 atlas. `fish-layout.json` stores measured alpha bounds. `Tools/Prepare-PondFishLayout.py` reads alpha without modifying image pixels; Unity creates the individual sprites. Runtime copies live in `Resources/PondArt`.

The follow-up cleanup removes the pink inflatable pool and stray foreground stones in `garden-tree-clean.png`; its exact edit prompt is saved in `pool-removal-prompt.txt`. The picnic table and its actual hiding spot move right to x3150. Unity's importer explicitly keeps these wide panoramas as 2D textures.

Water highlights, cascade streaks, fishing lines, floats, feeding portions and rod/cup picture controls animate separately. The painted atlas is reused in the pond, hands, menu pictures and catch close-up. Source audio is original synthesized ambience and a float cue; see `Tools/Create-PondAudio.py`.
