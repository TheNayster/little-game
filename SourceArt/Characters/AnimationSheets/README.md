# Selected-style Bluey and matching Bingo animation sheets

Created with the built-in **imagegen** tool on September 26, 2026. [Bluey](bluey-actions.png) is an animation preparation edit of the user's [selected original](../SelectedReference/bluey-movement-sheet.png). [Bingo](bingo-actions.png) matches that style with her orange/cream identity, guided by the locally inspected official Bingo portrait. The selected original remains unchanged.

Each output is preserved byte-for-byte. [Prompt set](prompts.md) records the generation instructions. These are generated production candidates, not official art or user-approved finished animation.

The game uses one drawing at a time: idle, blink, two waves, eight walking drawings, sitting, carry-ready and two dances. Trampoline height follows the existing interaction state and uses the standing/wave drawings. Right-facing source art is mirrored for left travel; asymmetric turnarounds remain open. Ordinary vertical motion retains the three-quarter view.

[Editable frame/pivot contract](animation-contract.json) defines source hashes, rectangles, scale and ground anchors. `Tools/Prepare-CharacterSheets.py` reads alpha for bounds and copies the original PNG bytes into Unity; it never recolors, retouches or resamples the artwork. Tiny alpha noise is excluded from rectangle measurements but preserved in the PNG. Unity's importer disables CPU readability/mipmaps and preserves source dimensions. Native previews must confirm edge quality against real backgrounds.

The runtime uses distance-driven frame playback, not the previous deforming SVG limb rig. Mathematical planted-foot results for builds 118–119 do not qualify this new animation. Check the complete rendered cycle, stop/reversal, home poses, character selection and real item ownership before delivery. Exact appearance and movement still need user review.

## Quieter walking revision 123

After trying 122 on the phone, the user liked the appearance but requested less arm/leg movement. Separate eight-drawing [Bluey walking](bluey-gentle-walk.png) and [Bingo walking](bingo-gentle-walk.png) atlases now replace only the walking frames. [Exact built-in prompt set](gentle-walk-prompts.md). All standing and home-action PNGs remain byte-identical to 122. Runtime cadence changes from 120 to 160 floor units per complete cycle; movement speed remains 210, so full-speed cadence changes from 3.5 to 2.625 steps/second. Smaller drawings and slower cadence are a prototype tuning change, not exact foot-plant calibration. The online mechanics review and full-cycle native preview remain part of visual evaluation.
