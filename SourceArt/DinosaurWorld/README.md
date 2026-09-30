# Dinosaur World generated artwork

These generated drafts add the four first rideable dinosaur models and a valley/world-menu picture. They follow the existing flat 2D family-game style. Runtime functionality and atlas checks do not establish user approval of the artwork.

Exact initial prompts and original generated paths: [generation manifest](generation-manifest.json). Exact edits, referenced original targets and selected edited paths: [repair manifest](repair-manifest.json). Menu generation: [menu manifest](menu-manifest.json). All generation/edits used the built-in imagegen tool; original PNGs and transparency are preserved. Runtime files are unchanged copies, rendered through atlas UVs in Unity.

T. rex, Triceratops and Parasaurolophus use the edited PNGs to keep their heads/tails inside each 4 × 2 cell. Brachiosaurus uses its original PNG. [Measured cushion and foot landmarks](seat-landmarks.json) register each pose to the ground and rider. Color/alpha pixels were read to measure these landmarks; no pixel editing or resampling was performed.

The rider resource is copied byte-for-byte from the current project `Resources/ParkArt/rider-poses.png` on September 30. It reuses the Bluey/Bingo authored seated drawings; other starter characters use the existing sitting art. Provenance remains in the Park source-art records. This does not integrate unrelated unfinished park-wheel code.

Runtime source paths: `Resources/DinosaurWorld/*.png`, `Resources/Scenery/dinosaur-valley-a.png` and `dinosaur-valley-b.png`, `Resources/WorldMenu/dinosaur-world.png`. The two background tiles currently share the same source. The resource importer explicitly uses Texture2D, transparency, no mipmaps and uncompressed art.

The separate sound resources are unchanged copies of the approved Home dinosaur book `effect-0.wav`, `effect-1.wav`, `effect-3.wav` and `effect-10.wav`, mapped to T. rex, Triceratops, Brachiosaurus and Parasaurolophus respectively. The existing approval/source-rights manifest remains authoritative.
