# Bluey and Bingo dinosaur onesies

The user rejected the initial geometric overlay, approved the new Bluey model sheet and limited the first wardrobe to Bluey and Bingo. The generated green sheets are retained unchanged, with their original transparency. Sixteen drawings include idle/blink, waving, eight quiet walking phases, sitting/carrying and two playful roar poses.

Built-in `image_gen` used the existing character sheets as identity references; Bingo also uses the approved Bluey costume sheet as its design reference. `generation-prompts.json` records the exact prompts and provenance. `animation-contract.json` contains editable frame rectangles and floor anchors. `Tools/Prepare-DinosaurOutfits.py` copies the unchanged PNGs and describes their poses.

Pink, blue and red replace only the green fabric pigment at runtime. Green displays the approved original pixels. Character faces, cream belly/plates and ink stay intact. No additional character outfits are enabled. Future prepared costume sheets use `CharacterOutfitArt/<outfit>/<character>` and the same wardrobe interface; availability is explicitly controlled by `CharacterOutfits.Available`.

Research: the official [Bluey Onesies reference](https://www.bluey.tv/watch/season-3/onesies/) establishes actual hooded animal costumes while retaining character identity. Unity's [full skin swap example](https://docs.unity3d.com/Packages/com.unity.2d.animation@13.0/manual/ex-sprite-swap.html#full-skin-swap) describes swapping an entire character appearance. The existing game already uses frame sheets, so the implementation swaps its `CharacterArt` source while retaining the movement controller and phase; it does not install another animation package.

The user has approved the Bluey design. Bingo matching art and the four colors still depend on their review in play. Technical checks do not replace that review.
