# Backyard pond fishing and feeding research

September 30, 2026. This records research for the Little Weeps backyard pond from **Tradies**, Season 3 Episode 32. The requested activity is one shared pond for up to four family players, with continuously running water, rod casting, random fish bites, an easy **Reel in** tap, a fish close-up followed by **Release**, and optional feeding. These choices are confirmed by the user. The pond activity is **planned and unbuilt**; this research changes no runtime, installed app or live server.

## What the episode establishes

The official episode page confirms Sparky and Chippy install a pond in the Heeler backyard. The official Tradies character page specifically identifies it as a fishpond commissioned by Chilli. These establish the location and identity; they do not specify fish species or establish this fishing game. Fishing, feeding and the controls below are the user's game adaptation. [Official Tradies episode](https://www.bluey.tv/watch/season-3/tradies/), [Official Tradies characters](https://www.bluey.tv/characters/the-tradies/).

The official channel also provides a scene reference: [Chocolate Milk and Big Belt](https://www.youtube.com/watch?v=QzbFUGwfmFE). Its available text does not verify the finished pond's shape. Search results surfaced a circulated pond frame with stones, planting, a small cascade and a wooden crossing, but its repost is not sufficient to certify an exact episode match. Confirm those details against the finished-pond shot when preparing final artwork; do not call the current generated backyard image an exact show reference.

## Make the pond pleasant during ordinary play

**Recommended art direction:** a low, irregular stone edge, soft blue-green water, surrounding garden plants and a small continuous cascade. Preserve the show's simple painted shapes and readable outlines. Flow should fall in one direction into a localized splash, with ripples spreading outward and fading. Gentle surface highlights and occasional fish turns supply movement between interactions. This is a design proposal, not a claim that every detail has been verified in the episode.

Use a quiet, seamless trickling-water sound near the pond. The water keeps moving when nobody is fishing; starting or ending a game must not switch it off. Avoid a large geyser, busy bubbles everywhere or strong flashing reflections. Put fish below the surface highlights and behind the near stone edge so they look submerged.

The current `garden-shed.png` contains a decorative pond with lilies and a stone lantern. It is painted into the panorama and provides neither animated water nor fishing. Prepare a clean background plus separate pond bed, rear stones/plants, fish, surface highlights, flowing water, front stones, rods, floats and food. Give the bank enough usable space for all four characters, with feet contacting the ground and rod tips reaching the water.

## Cast catch and release

1. Take a rod at the pond. Recommend also offering **Fishing** in the backyard Games menu; it walks the player to an available bank position.
2. Tap a broad water target to cast. Show a short rod movement, a curved line, the float landing and one small ripple. Drag aiming is unnecessary for the chosen easy controls.
3. Fish continue swimming. An eligible fish randomly approaches a float, nudges it and bites. Recommend initially varying the wait around 4–10 seconds, then tune through family playtesting; this timing is a proposal. Everyone sees the same fish approach and bite.
4. The float dips and a large **Reel in** button appears, with a visible cue as well as a soft sound. One tap brings up the fish close-up; rapid tapping or precision swiping is unnecessary.
5. A large **Release** button returns that same fish to the pond with a small splash. The fish swims away and becomes available again. Close the picture or leave the area to release automatically.

Recommend no score requirement, timer, missed-bite punishment or competitive winner. A missed bite simply lets the fish swim away and another opportunity follows. The user's close-up choice replaces the earlier observation-bowl proposal. A discovery album, a fixed five-species collection and rewards remain outside the confirmed scope.

## Feed the fish

Offer **Feed fish** beside the rod choice. Recommend tapping a water target to sprinkle a small visible portion. Available fish gather, nibble the floating pellets, make tiny ripples and then resume swimming. Pellets disappear as eaten or after a short interval, keeping the pond clean.

Fishing and feeding coexist in the same pond. A fish already biting or caught cannot simultaneously eat a pellet; other fish can feed while siblings fish. Feeding should be enjoyable on its own and should not be required to unlock bites. Cap visible pellets and effects, while giving immediate feedback to each player's tap. No hunger chores or overfeeding penalties are proposed.

## Share one pond across four players

Use four rods and four bank positions around one common fish population. Players can join, switch to feeding or leave independently. This is open-ended shared play, with no rounds requiring a countdown. Another player's catch remains visible in the world; its close-up belongs to the catcher and must not cover siblings' screens.

The existing PC/VPS authority should decide fish identity, random bite timing, reservations, feeding consumption and release. Clients animate the agreed movements and render water locally. Suggested fish states are **swimming → approaching → biting → caught → released**, with a separate feeding approach for available fish. One fish can belong to only one catch at a time. Bound random waits so unlucky children are not left waiting indefinitely.

Leaving, traveling or disconnecting releases only that player's rod and unfinished catch. Prevent duplicate catches and replayed feeding requests. Temporary lines, food and catches should clear safely after authority recovery; persistent catch logs are unnecessary for the requested loop. Shared rules will require compatibility review when implemented; this research makes no schema or content change.

## Rendering and the focused implementation check

Recommended implementation: reuse the existing layered 2D scenery system, animate a small water strip and bounded ripple sprites, and move a small reusable fish pool along curved paths. Keep transparent effects confined to the pond. Unity identifies overlapping transparent UI, particles and sprites as common sources of overdraw; reducing those layers and using simple shaders supports this approach. This is an implementation recommendation, not measured pond performance. [Unity rendering guidance](https://docs.unity3d.com/6000.0/Documentation/Manual/OptimizingGraphicsPerformance.html).

Unity's `AudioSource.loop` supports repeating a water clip; a seamless recording and distance/volume fade still need authoring. [Unity looping audio](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource-loop.html).

When building is requested, check the full cast/bite/reel/close-up/release loop, feeding while another player fishes, continuous water when idle, four players sharing fish without duplicate catches, and independent departure. Check the pond and large controls at representative Android/iPad dimensions, with a focused frame-time look on the older iPad. Final art must be reviewed against the finished episode shot. Research and documentation checks do not establish playable completion or visual acceptance.

[Home checklist](../home-world-feature-tracker.html#backyard-garden-shed-rides-and-fishpond) · [Current project decisions](../current-decisions.md)
