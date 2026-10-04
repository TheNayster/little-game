# Stage 5 isolated prototype artwork

Owner DAY-01 / LEARN-01 / FAMILY-01. Consumers: only `SandcastleVisualPrototype.unity`, `SandcastlePrototype`, `SandcastlePrototypePiece` and its editor build. Runtime counterparts are under `Resources/Worlds/Daycare/SandcastleClub/Prototype`. Retain masters/registration while consumed; retire only after a reviewed replacement. These are prototype candidates, not parent-approved final artwork.

## P4 historical sources

Approved reference: `docs/implementation/evidence/daycare-sandcastle-prototype-2026-10-03/approved-target.png`. Built-in imagegen used; no API/CLI fallback. Masters are original RGBA1536x1024 PNGs, copied unchanged, not layered PSDs. UV registration adapts the separate sprites without modifying the original image. Rounded UI surfaces are code-native sliced sprites; existing Little Weeps characters are reused without repainting.

Origin: built-in generation `exec-4ecbf526-e56a-42de-b2d6-10ec8578f6b2.png` (empty pit) and `exec-ee9d0e2c-6021-405c-896a-b53deb3861cf.png` (pieces), retained under the Codex generated-images directory and copied into this repository.

Prompt set:

1. Use approved image only as style/composition reference. Generate ONLY one empty physical wooden sandpit, elevated front angle, wide near edge/narrower far edge, thick top planks and substantial front fascia, transparent outside. Warm golden grain and soft children's painted outlines. Clear unobstructed central sand, subtle perimeter mounds. Far surface corners around16%/84%width at25%height, near around4%/96%at77%; front wood to93%. No characters, castle, toys, tools, UI, decorations, plants, text or scenery.
2. Six independent castle sprites on one transparent3x2 equal-grid sheet, golden textured sand, consistent elevated front view, top/right planes and small contact sand. Round crenellated tower with small carved window; clearly square crenellated tower; short horizontal joining wall; horizontal gate with large genuinely transparent OPEN arch, no door or sand filling the hole; receding90-degree wall; receding90-degree open gate. Exactly six separated complete sprites/gutters. No flags, shells, props, animals, scenery, bucket, UI or text.

See `registration.json` for measured source rectangles. Runtime shapes remain independent and editable via layout/projection and retained raster sources. The approved target is evidence, not a playable flattened background.

## Current refined sources (P9)

Consumers/ownership and retirement remain as above. The production StageFive assets and shared character sheets are untouched. Raster sources are retained unchanged, with independent sprites registered by measured UV rectangles; code composition and registration are editable, but these are not layered PSDs. Generated pose candidates have not received parent approval.

- `empty-pit-smooth-master.png` and `pieces-smooth-master.png`: current runtime counterparts `empty-pit.png` / `pieces.png`. Built-in imagegen origins `exec-ac41484a-392e-4d82-b317-132d22e4374b.png` and `exec-0de31cd9-f078-420c-893f-fdad886b58c7.png`. Original1536x1024 RGBA.
- `participation-refined-master.png`: current runtime `participation.png`, original1086x1448 RGBA, built-in origin `exec-27951322-549a-499d-be8c-669d203d5bac.png`. Four rows Bluey/Bingo/Muffin/Socks; three columns ready/scoop/pour, referenced against the existing approved model sheets. No Carry poses. Existing Wave drawings supply the accepted Tip celebration.
- The earlier `empty-pit-refined-master.png` / `pieces-refined-master.png` are intermediate sources retained for provenance, not loaded. Origins `exec-be6b2a3a-8254-49cf-b4fc-16ed14d4748d.png` / `exec-191c9ed1-f76c-4d5d-b694-03ddcfed5d64.png`.
- `registration.json` identifies current geometry/source crops; `registration-p4.json` preserves the old measured layout. Pose registration is in `participation-refined-registration.json`. The current90-degree crop excludes the neighboring gate/gutter; round carved-window alpha has a local opaque recess in `SandcastlePrototypePiece`, while gate arches remain genuinely open.

Refinement prompt set:

1. Keep the independent pit silhouette, transparent exterior, elevated trapezoid and plank structure. Halve the heavy front fascia; remove repeated mounds and extreme grain. Refine again with **no grain, speckles or rough noise**, smooth broad warm golden storybook sand, two quiet corner hollows, soft broad peach shadows and simplified honey wood. No characters, castle, UI or objects.
2. Preserve six sprite identities/order and transparent arches. Correct the two90-degree ground axes to recede up the image rather than strongly diagonal across the pit. Refine again with no granular noise, only3–5 sparse flecks, smooth cream/ochre planes and thinner lighter warm outlines. Do not change dimensions/layout or add connectors/props.
3. Reference the actual four character model sheets, preserving their faces, colors and flat2D Little Weeps style. Three uniform-baseline full-body poses per character: ready with paws low/forward, leaning local scoop with small red shovel, local pour with small green watering can/short droplets. Transparent, no scenery/UI/extra characters or clothing. The game supplies brief local strokes and existing Wave reaction, not a newly drawn continuous shovel cycle.

These separate assets do not bake the approved concept into gameplay. The castle fixture is14 legally built pieces, all still selectable; framing/registration never alters the authority lattice or16-piece cap.
