# Research roster model sheets

[Browse all 37 sheets](index.html). The source roster is section 4 of `docs/bluey-game-research-2026-09-23.md` and its official-reference catalog. The three Terriers have individual sheets; optional older characters Digger, Mia and Captain are included.

Built-in `image_gen` produced one transparent PNG per character with the existing Bluey action sheet as a style reference and each available official portrait as the identity reference. `generation-prompts.json` preserves exact prompts, reference inputs and output provenance. Original generated pixels are retained unchanged. `roster.json` records hashes and reference URLs.

Each PNG contains sixteen pose studies: front, three-quarter, right profile, back; blink, wave, sit, carry; four right-facing walking drawings; two dances, reach and happy. Each SVG keeps those poses as independently named raster layers. Frame JSON records crop bounds and ground pivots. These are editable pose arrangements, not articulated body-part rigs. Rebuild the gallery with `python Tools/Build-CharacterModelGallery.py`.

Bluey and Bingo retain their accepted runtime art, eight-drawing walk cycles and stable saved IDs. Their extra sheets here are reference studies only. Muffin, Socks, Bandit and Chilli are new playable candidates, imported with `python Tools/Prepare-RosterSheets.py`. Their four walk drawings repeat across the existing eight timing slots. In-world proportions differ, while the menu caps adult height and compensates the adapter's ground offset so ears and labels remain visible.

All generated likenesses need family visual review. The wider roster remains outside the game until prepared and checked. Low-alpha matte noise, pose consistency and mirrored asymmetrical markings are polish limits. Pretzel lacks a verified portrait in the catalog; Lulu's body extrapolates from a head-only reference. Dougie's wave is not verified Auslan.

Windows candidate 253 compiles both client and dedicated server. Focused native phone/tablet chooser checks cover all six selections, preserved player/toy state, four simultaneous identical favorites and independent departure. The focused core check also covers JSON save/reopen, room/progress retention and rejected IDs. Evidence is in `docs/implementation/evidence/character-roster-2026-09-30/`.

New shared avatar IDs require content 36: older authorities cannot validate these choices. The combined candidate also contains concurrent Zoo work at schema 34; this task adds no save fields. No phone, iPad or live family server was updated. Preserve the main integration hold. Next: family review and another prepared roster batch, then a coordinated rollout when requested.
