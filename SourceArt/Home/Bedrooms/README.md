# Four upstairs bedroom bases

September 26, 2026, BED-2. Built-in `image_gen` created the architectural panorama using the accepted upstairs hallway as a style reference. The exact prompt, reference, dimensions and SHA-256 are in [manifest.json](manifest.json).

- Saved original: [bedroom-base-source.png](bedroom-base-source.png), 2048 × 683, copied without image edits.
- Runtime copy: `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Scenery/home-bedroom.png`.
- Four distinct persisted room IDs share this one texture. Ownership and room contents are logical saved state; they never depend on texture identity or the visitor's avatar.
- The ordinary exit at the left returns to the matching hall door. `Core/HomeRooms.cs` owns the route and safe arrival anchors; `Client/SoloBedrooms.cs` positions the usable doorway and ownership labels.
- Furniture is deliberately absent from the architectural base. BED-3 supplies separate usable beds, shelves, chest/baskets, cushions and decoration with correct front/rear overlap. Do not paint duplicate furniture into this base.
- Existing character art, hallway, stairs and Home fixture art are retained. Secret-room entrances and art come after the bedroom stage.

[BED-2 implementation and captures](../../../docs/implementation/bedroom-rooms-2026-09-26.html). Native phone/tablet-sized captures were inspected; this artwork has not received user or physical-device acceptance.
