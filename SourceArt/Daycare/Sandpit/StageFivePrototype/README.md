# Stage 5 isolated prototype artwork

Owner DAY-01 / LEARN-01 / FAMILY-01. Consumers: only `SandcastleVisualPrototype.unity`, `SandcastlePrototype`, `SandcastlePrototypePiece` and its editor build. Runtime counterparts are under `Resources/Worlds/Daycare/SandcastleClub/Prototype`. Retain masters/registration while consumed; retire only after a reviewed replacement. These are prototype candidates, not parent-approved final artwork.

Approved reference: `docs/implementation/evidence/daycare-sandcastle-prototype-2026-10-03/approved-target.png`. Built-in imagegen used; no API/CLI fallback. Masters are original RGBA1536x1024 PNGs, copied unchanged, not layered PSDs. UV registration adapts the separate sprites without modifying the original image. Rounded UI surfaces are code-native sliced sprites; existing Little Weeps characters are reused without repainting.

Origin: built-in generation `exec-4ecbf526-e56a-42de-b2d6-10ec8578f6b2.png` (empty pit) and `exec-ee9d0e2c-6021-405c-896a-b53deb3861cf.png` (pieces), retained under the Codex generated-images directory and copied into this repository.

Prompt set:

1. Use approved image only as style/composition reference. Generate ONLY one empty physical wooden sandpit, elevated front angle, wide near edge/narrower far edge, thick top planks and substantial front fascia, transparent outside. Warm golden grain and soft children's painted outlines. Clear unobstructed central sand, subtle perimeter mounds. Far surface corners around16%/84%width at25%height, near around4%/96%at77%; front wood to93%. No characters, castle, toys, tools, UI, decorations, plants, text or scenery.
2. Six independent castle sprites on one transparent3x2 equal-grid sheet, golden textured sand, consistent elevated front view, top/right planes and small contact sand. Round crenellated tower with small carved window; clearly square crenellated tower; short horizontal joining wall; horizontal gate with large genuinely transparent OPEN arch, no door or sand filling the hole; receding90-degree wall; receding90-degree open gate. Exactly six separated complete sprites/gutters. No flags, shells, props, animals, scenery, bucket, UI or text.

See `registration.json` for measured source rectangles. Runtime shapes remain independent and editable via layout/projection and retained raster sources. The approved target is evidence, not a playable flattened background.
