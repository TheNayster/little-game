# Seagull surprise artwork

Prepared September 30, 2026 with the built-in **image_gen** tool for BCH-02. The silver gulls use a friendly Bluey-style side view with white plumage, grey wings, dark wing tips and orange-red beaks/legs. [Exact generation and cleanup prompts](prompts.json).

`seagull-poses.png` is the original selected transparent atlas, copied unchanged into the runtime BeachArt folder. `seagull-poses.ora` is the editable OpenRaster source with four named pose layers and retained pixels. The atlas has no labels or backdrop; any RGB in fully transparent pixels is invisible. Unity creates sprites from the pose bounds without altering or duplicating the texture.

The atlas dimensions are 1536 × 1024. The frame map uses top-left pixel coordinates:

| Pose | Bounds x y width height | Body registration x y |
| --- | --- | --- |
| Resting | 153 103 452 395 | 420 300 |
| Wings up | 937 39 497 449 | 1210 390 |
| Wings down | 142 624 567 357 | 440 720 |
| Glide | 810 582 693 314 | 1190 790 |

The body registrations keep the torso in place when wing bounds change. Mirroring changes flight direction; six stable offsets form the flock. The selected sheet is a generated candidate, not separately approved family artwork. Pecking, preening, dedicated landing feet and gull audio remain polish opportunities.

References: [BirdLife Australia Silver Gull](https://birdlife.org.au/bird-profiles/silver-gull/) for appearance; [Bluey The Beach](https://www.bluey.tv/watch/season-1/the-beach/) for the scene anchor. The bird poses are newly generated, not extracted from the episode.
