# Pan and pot meals: applied research and build contract

Research recorded September 28, 2026, before implementation. Follow the existing staged-cooking and Home finishing contracts. Five pizzas are qualified in candidate 210; this is the next bounded family.

## Primary references and concrete use

- [Good Food: child-friendly burgers](https://www.bbcgoodfoodme.com/recipes/really-easy-beefburgers/) separates cooking each side, optional melting cheese, and fresh salad/bun assembly. Start with the existing counted patty; turn it before second-side cooking. Offer cheese as a finishing option while still in the pan, then build the burger. Do not heat a finished bun picture in the oven.
- [Barilla: pasta technique](https://www.barilla.com/en-us/help-with/pasta-kitchen-tips/how-to-cook-pasta) separates water, pasta cooking/stirring, draining and sauce. Preserve a bounded saved pot fill and drain amount, then sauce coating and final cheese.
- [Good Food: vegetable soup](https://tollbit.bbcgoodfood.com/recipes/versatile-veg-soup) uses chopped vegetables and stock before simmering. Our previously requested chunky soup retains visible pieces rather than adding the reference's blending step. Show chopping, broth transfer, stirring, simmering and ladling.
- [Good Food: pancakes](https://tollbit.bbcgoodfood.com/recipes/easy-pancakes) distinguishes batter and two-sided pan cooking. Show mixing, pouring, first-side bubbles, flipping, second-side browning, stacking and fruit afterward.
- [Tilda: basmati methods](https://www.tilda.com/how-to/how-to-cook-basmati-rice/) describes absorption cooking and fluffing. For the requested rice/vegetable bowl, make water disappear as rice swells, then combine separately chopped and stir-cooked vegetables. The game will not teach real cooking times or measured ratios.
- [Sago Mini Diner](https://sagomini.com/apps/diner/) and the publisher evidence in the existing cooking-stage report inform short recognizable actions, visible food and matching ingredients. Four multiplayer workspaces and the exact phase sequence are our design decisions, not proven features of those apps.

## Required engineering

Four real hob supports, independent from four oven slots. Pan/pot meals heat on hobs; cakes/pizzas continue using ovens. Every cook can start, stop interacting, travel or reconnect without affecting the others.

Each cooking pass needs a distinct stage identity. Both transient heat packets and reliable snapshot merging must match dish AND stage, so a delayed first-side completion cannot rewind or complete the second side. Heat must start deliberately rather than automatically when a prepared dish remains on the hob. Finished heat settles safely; waiting for a flip is not an active cooker.

Save partial mixing/pouring/draining/stirring/stacking and counts with the dish. Old recipe versions stay unchanged. Tap assistance visibly performs the same action as gestures, cancels on closing and cannot trigger the next stage. Phase-specific stock prevents salad or fruit appearing in raw batter. The shared tap supplies only bounded recipe water; water belongs to the saved pot, not a hidden ingredient stock. Ready food is an explicit choice.

## Acceptance

Every recipe at intermediate save/reopen checkpoints; exact ingredient/portion conservation; four independent cooks including departure; stale phase input; first/second heat packet separation; stock contention and full hobs; saved unfinished and finished food; source/build match; native pictures, direct gestures and child-size controls. Independent recovery remains separately blocked by Windows policy. No live server/device or security-setting changes are implied.
