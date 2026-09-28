# Downstairs hide-and-seek art

Built-in imagegen produced the `bandit.png` and `covers.png` atlases for the HS-1 game. The exact prompts and official Bandit reference source are in [manifest.json](manifest.json). Both atlases use three columns and two rows; Unity selects sprite rectangles directly and preserves the generated alpha. No character or furniture is painted into a new background.

Bandit has stand, two walking, counting, inspecting and friendly-wave poses. The cover atlas supplies closed/open curtain, two-compartment wardrobe and tent states. The existing sofa remains its original size and artwork. The owner sees a local translucent cutaway; other players see closed cover and no hidden avatar or held prop.

Generated assets are candidates, not family-approved artwork. Accepted Bluey/Bingo sheets are unchanged. The first walk atlas has limited pose separation; further animation polish remains possible after physical review.

The corresponding runtime copies are under `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/HideAndSeek`. The reference image is research material for this private family project, not a new player avatar.
