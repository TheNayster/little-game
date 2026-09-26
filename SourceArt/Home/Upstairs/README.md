# Upstairs hall and working stairs

First stair/landing slice, September 26, 2026. [Research](../../../docs/implementation/upstairs-bedrooms-research-2026-09-26.md) supplies the construction, travel and persistence rules. This artwork does not mean the four bedroom interiors or secret rooms are built.

- `upstairs-hall-source.png`: unmodified built-in imagegen output, 2048 × 683. A warm illustrated upper hall with four bedroom door facades and a stair opening at the left. The doors become usable in the next bedroom slice.
- Runtime copy: `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Scenery/home-upstairs.png`.
- Style reference: the existing `Resources/Scenery/home-living.png`, inspected before generation. The accepted downstairs image and character sheets are unchanged.
- The downstairs stairs and upstairs rail use convex foreground patches referencing the exact panorama pixels. Geometry is in `Client/SoloRooms.cs`; no second rail bitmap is layered with a different registration.
- Logical route/arrival anchors and safe endpoint behavior are in `Core/HomeRooms.cs`. The hall spans 0–2400 floor units. Existing downstairs/yard coordinates are unchanged.
- Rooms remain within Heeler Home; each client loads its own view. The panorama participates in the existing maximum of three resident/pending scenery textures.

The source and runtime image bytes match. Keep future doorway edits separate from this source; retain its generation record in [manifest.json](manifest.json). Final user visual acceptance and physical A10 performance remain open.
