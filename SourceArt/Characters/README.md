# Bluey and Bingo character sources

These layered SVGs are newly authored character studies guided by the official portraits in the project's [reference catalog](../../docs/bluey-research/character-references.json). They replace the rejected generic pup and are integrated into the actual game. The table below preserves the original workshop milestone; current runtime evidence is recorded after it. They are not official production rigs or user-approved final artwork.

## Research requirements applied

| Research requirement | Current implementation | Still required |
| --- | --- | --- |
| Goal sheet section 4: start with Bluey and Bingo; use official references | Distinct blue/navy and orange/cream designs; box-shaped silhouettes, oval eyes, muzzle, face patches, brows, paws and tails. Bingo uses a smaller visual scale. | User/child likeness acceptance; full roster. |
| Toca/Piknik supplement: retain recognizable 2D Bluey character style | Flat colors, outlined layered artwork; the generic round-headed pup is excluded. | Refine proportions and expressions from visual feedback. |
| Goal sheet section 4: retain editable layered sources, floor pivot and hand attachment | Separate named layers in each SVG; contract records pivots and hand anchor; reproducible transparent PNG export with source hashes. | Additional side/back artwork and direction-specific overlap; a mirrored study cannot satisfy asymmetric production views. |
| Goal sheet section 4 / build guide ART-PREP-02: idle, walk and carry; first isolated motion test | Idle/blink, walk, wave, carry, facing and depth ordering in native Unity. | Reach/use, talking, happy/seated poses and production animation polish. |
| Goal sheet section 17 / CHAR-01: changing characters preserves player state | Workshop swaps visual children without changing the movement root, position, facing or carried presentation. | Integration with real player/profile/activity/ownership state and device tests. |
| Build guide: keep one bounded task with evidence | Isolated native workshop; no game session or save operations. Evidence report links exact build and runtime checks. | G5 contracts after character review; G6 home/backyard slice and A10 qualification. |

References: [Bluey](https://www.bluey.tv/characters/bluey/), [Bingo](https://www.bluey.tv/characters/bingo/), [goal sheet](../../docs/bluey-game-research-2026-09-23.md), [interaction research](../../docs/toca-piknik-interaction-research-2026-09-23.md), [build guide](../../docs/family-playset-build-guide-2026-09-23.md). The official reference portraits are inspection material. The previously unused generated atlas is now recovered in [SelectedReference](SelectedReference/README.md) because the user explicitly prefers it; it is not yet imported into Unity.

`character-contract.json` defines the export. Run `Tools/Export-CharacterStudy.py` with Node and its `sharp` dependency, then build and verify a fresh workshop. Do not flatten the sources or describe passing motion checks as visual approval.

## Side-view follow-up — build 119

`bluey-profile.svg` and `bingo-profile.svg` add twelve-layer left profiles with their own pivots and hand anchors. The exporter now emits front and profile manifests; the actual game validates and caches both drawings. Horizontal/diagonal walk and carry select the profile, mirrored for rightward motion. Standing and home actions use the original front art. Only one rig renders; both retain the current animation phase.

[Implementation and native preview](../../docs/implementation/profile-walk-2026-09-26.html). Twelve rig trials, home actions and phone/tablet chooser checks pass. Android 119 is installed with saved records retained; the phone UI check awaits unlock. These checks do not establish user visual acceptance. Independently drawn asymmetric sides/back/turns, full roster and physical A10 performance remain open. The old isolated workshop is historical front-view evidence.

## Latest visual selection

The user prefers the [recovered movement sheet](SelectedReference/README.md). Treat its recognizable appearance as the target for the next animation proof. The front/profile SVG rigs remain the currently installed implementation, not an override of this visual choice.
