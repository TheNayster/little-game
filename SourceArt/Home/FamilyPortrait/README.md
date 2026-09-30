# Living-room family picture repair

The September 30 repair replaces the malformed three-figure picture above the couch with Bandit and Chilli behind Bluey and Bingo. All four faces are distinct. The frame, placement and surrounding living-room composition remain in place.

`home-living.png` is the editable raster source; its identical runtime copy is `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Scenery/home-living.png`. The existing Unity asset GUID and texture settings are retained. The built-in image_gen tool, exact edit prompt and local appearance references are recorded in [manifest.json](manifest.json). The earlier room plate remains recoverable in Git history.

Visual inspection of the generated panorama passed. A Windows 234 build attempt was blocked by an unrelated concurrent `SoloPark.cs:134` compiler error (CS1503, float supplied where Vector2 is required), before producing a release. No native scene check, device installation or live-server update is claimed. Family visual acceptance remains pending.
