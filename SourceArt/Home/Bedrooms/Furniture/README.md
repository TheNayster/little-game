# Bedroom furniture artwork

Eight transparent raster assets generated with **built-in imagegen** for the four upstairs bedrooms. Exact submitted prompts, references, generation paths and SHA-256 hashes are retained in [manifest.json](manifest.json). These are newly generated furniture drafts, not user-approved character replacements. Accepted Bluey/Bingo sprite sources remain unchanged.

| Asset | Runtime role |
| --- | --- |
| `bed.png` | One bed rear plus registered quilt/front cover polygons; one resting occupant. |
| `shelf.png` | Rear/body plus registered lip/post cover; two cubby and two top support positions. |
| `chest-open.png`, `chest-closed.png` | Two chest states; open front covers real contents. Eight bounded storage positions. |
| `cushion.png` | Four usable visitor cushions per room, independently occupied. |
| `rug.png` | Noninteractive floor decoration beneath the cushions. |
| `lamp.png` | Switchable bedside/shelf lamp with a simple warm glow. |
| `plush.png` | Carryable starter stegosaurus plush and a wall print. This does not complete the 20-dinosaur catalog. |

Source PNGs are copied unchanged into `Resources/BedroomArt`. Importers cap each texture at 1024 pixels, disable read/write and mipmaps, and retain alpha. One set is reused by every room. Furniture is separate from the empty architectural background; there are no painted duplicate beds/chests. The current Resources cache retains this small set during play and releases it with the presentation; this is separate from the three-background-texture scenery budget. A10 peak memory and frame-time qualification remain required.

`SoloBedroomFurniture.cs` defines sprite rectangles, top-left-normalized convex cover polygons and visual support offsets. `BedroomFurniture.cs` defines persistent support IDs, floor positions, four seat positions and two authored layouts. Logical storage locations remain within the save's floor-coordinate bounds; their presentation offsets align items to the actual shelf/chest interiors. A layout change moves supports and current occupants atomically, without replacing loose items or saved contents.

Both arrangements reserve the far end of the room for the future **small magical secret entrance**, away from the hallway door. It will appear only when the local character approaches, with a star glow, bounded sparkles and shimmer; no inactive portal button is added by this furniture stage.

Themes tint the bedding, cushions and picture backing. Arbitrary furniture dragging, a full furnishing catalog, independently selectable artwork/rugs/bedding and complete book/creation-display systems remain future work. Inspect native bed contact, all four cushion occupants, open/filled/closed chest and both arrangements before treating an artifact as ready for a device preview.
