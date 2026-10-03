# Sandcastle Stage 5 artwork sources

Owner/work item: DAY-01 / LEARN-01 / FAMILY-01, Sandcastle Stage 5. Consumers: `SandArt`, `SandShape`, `GameScreen.SandcastlePresentation` and shipped `Resources/Worlds/Daycare/SandcastleClub`. Retain while these production assets are used; replace only with an explicitly reviewed asset revision. These are implementation artwork, **not the missing parent-approved concept TARGET**.

Original project artwork generated with the built-in imagegen tool October 3, 2026. No competitor artwork was downloaded/copied and no third-party attribution licence is required for these generated images. Existing character/dinosaur assets retain their existing provenance; no replacement cast was generated.

Editable raster masters: `objects-source.png` and `pit-final.png`. `pit-source.png` preserves the earlier fascia-heavy draft; it is not shipped. `registration.json` preserves independent object crop rectangles and import/render rules. The source atlas stays unchanged: the runtime registers UVs rather than baking objects into the scenery. Bucket fill, wet drop/pips, props, selection and effects are separate drawing layers in the editable `SandShape.cs` source. This is a raster master plus code composition, not a claim of layered PSD/vector masters.

## Generation prompts

**Object sheet:** Production 4×4 transparent children's game sprite sheet. Warm illustrated daycare cartoon style with soft cocoa contours, golden sand grain, top-left lighting. Row1 round tower, square tower, horizontal wall, horizontal gate with transparent opening; no baked doors/windows/flags. Row2 perspective wall, perspective gate, empty turquoise bucket, orange-handled turquoise shovel. Row3 sky-blue watering can, tipped bucket tool, coral flag, peach shell. Row4 blue-grey pebble, arched wooden door, teal window, sand mound. Independent isolated objects, crisp alpha, no background/text/characters or competitor copies. Tool output: `exec-67e98f58-2366-4693-a0a7-dcf844c4c447.png` under Codex generated-images thread `01a0f415-152d-7c00-9f7b-2f7970890f57`.

**Pit first draft:** Empty rectangular warm illustrated wood sandpit, 2:1 transparent exterior, subtle sand grain/rake swirls, small outside corner greenery, no interactive objects. Output `exec-d3c580f4-8c60-42c3-957d-4a54a9982c5e.png`.

**Final pit edit:** Preserve sand, wood grain, transparent exterior and corner greenery. Camera almost overhead; sand interior rectangular, about88% image height/90% width. All rim boards thin, about4% height, remove massive front fascia/posts. No props/characters/interface. Output `exec-8534c9b9-36d3-4057-a132-7ccd15600a5e.png`. This adaptation registers the artwork to the existing rectangular authority grid.

## Audio

`Tools/Art/Build-SandcastleAudio.py` is the editable deterministic source for four original quiet PCM Foley clips: sand swish, water pour, short reveal notes and decoration response. `audio-inspection.json` records duration, peak/RMS and hashes; it is structural inspection, not listening approval. The toy reuses `Worlds/Dinosaur/Audio/tyrannosaurus.wav`. Existing optional water/tip narration is retained; outdated refill/count/completion narration is not newly enabled. Two reusable voices plus a140ms minimum gap bound overlaps; existing voice/music/test mute and foreground ducking apply.

Parent appearance and listening approval are separate from the local native checks. Physical iPad/phone comfort/performance remains deferred.
