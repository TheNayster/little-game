# Toca Boca, Piknik, and a simpler Bluey playset

Deep research and proposed interaction design • September 23, 2026

**Audit update:** the current plan has six worlds, up to four mixed-device players, independent location travel, required iPad hosting and automatic host switching. Only travel-network multiplayer is optional; full solo play remains required. See the [complete feasibility audit](family-playset-feasibility-audit-2026-09-23.html).

**Recommendation: one Bluey world, with two ways to play.** Give your three-year-old a close-up **Simple Play** view with large objects, direct touch, and forgiving automatic assistance. Give your six-year-old an **Explore & Stories** view with wider rooms, more combinations, and optional longer quests. Both use the same characters, items, rules, and saved world, including when the iPads play together.

The key change to our earlier plan is to put **playful objects before quests**. A bucket should be enjoyable before anybody asks the child to water a flower. It can fill, pour, make a puddle, float a toy, wet sand, wash something, or become a pretend-play prop. Quests simply suggest one of those possibilities.

Open the [interactive companion](toca-piknik-interaction-research-2026-09-23.html) to compare the two play styles for different objects. The [Bluey research guide](bluey-game-research-2026-09-23.html) remains the reference for characters, GitHub components, voices, devices, installation, and the six-area menu.

**Scope and evidence:** this report uses the developers' public descriptions, designer/developer letters, official support information, and Apple's app listings. I did not install or directly play these commercial games during this research, inspect their internal code, or measure their performance. Descriptions of their published behavior are distinguished below from our proposed Unity implementation. Product pages sometimes retain older catalog counts; this research focuses on interaction patterns rather than treating those counts as current inventories.

## 1. Which games we are combining

“Piknik World” is interpreted here as **Sago Mini World within Piknik**, while also considering useful games elsewhere in the Piknik collection. Piknik is the bundle that includes Toca Boca Jr, Sago Mini World, and other apps. Toca Boca World is a separate game reference. [Piknik's official collection](https://www.playpiknik.com/)

| Reference | What its official material emphasizes | What we should take from it |
| --- | --- | --- |
| **Toca Boca World** | A world for creating characters, designing spaces, and acting out stories | The connected dollhouse, persistent belongings, carrying and arranging props |
| **Toca Boca Jr** | A collection of focused creative play experiences, including cooking, nature, building, and experiments | Activities that are understandable on their own, with a small set of interesting tools |
| **Sago Mini World** | Open-ended activities, familiar characters, playful surprises, and play without points or time limits | Immediate reactions, compact activity spaces, forgiving exploration |
| **Toca Boca Hair Salon 4** | Direct manipulation with tools that cut, color, style, and regrow hair | Reversible transformations: trying something unexpected does not ruin the toy |

Primary descriptions: [Toca Boca World](https://www.tocaboca.com/toca-boca-world), [Toca Boca Jr](https://www.tocaboca.com/kids/toca-boca-jr), [Sago Mini World](https://sagomini.com/world/), [Hair Salon 4 publisher listing](https://apps.apple.com/us/app/toca-boca-hair-salon-4/id1485387513).

My synthesis is **Toca World’s connected places + Jr’s focused activities + Sago Mini’s responsive, silly objects + Bluey’s characters and family pretend play**. We can combine these qualities without making the younger child learn a large interface.

## 2. What the individual games teach us

### Toca Kitchen 2: ingredients become experiments

The official Jr description presents cooking as experimentation: unusual foods and drinks are allowed. Apple's editorial description names actions such as chopping, boiling, frying, pureeing, and seasoning; the publisher describes serving the results to characters and seeing their preferences. [Toca Boca Jr](https://www.tocaboca.com/kids/toca-boca-jr), [Kitchen 2 / Jr on Apple's store](https://apps.apple.com/au/app/toca-boca-jr-fun-kids-games/id943869618?platform=ipad)

**Our application:** let a banana be picked up, peeled, sliced, placed on a plate, blended, offered to a character, or used in a pretend picnic. The child chooses what to do; a recipe card is optional. Show the change in the actual item, then let the changed item participate in another action. A giggle or a funny face can be the payoff without a score.

Do not make every experimental combination trigger “wrong.” A strange smoothie can produce a playful surprised expression and remain a valid creation. Keep a distinction between playful food reactions and a failed task.

### Toca Lab: Plants and Toca Nature: visible change invites another try

Toca's Jr page describes experimenting with plants and watching them evolve. Its Nature description includes shaping terrain, planting trees, collecting food, and feeding animals. The shared pattern is a world that visibly changes in response to an action. [Official Jr game descriptions](https://www.tocaboca.com/kids/toca-boca-jr)

**Our application:** water changes a plant's appearance; a seed becomes a shoot; a picked flower can decorate a table. Different seed choices produce different colors or shapes. Keep these as simple authored states and animations first. The child should see a meaningful result quickly, without waiting hours for a real-time growing timer.

### Toca Hair Salon 4: the power of an undoable transformation

The publisher explicitly describes trimming, shaving, coloring, and regrowing hair. A tool can make a dramatic change without permanently destroying the play object. [Hair Salon 4 publisher description](https://apps.apple.com/us/app/toca-boca-hair-salon-4/id1485387513)

**Our application:** washable paint, rebuildable sandcastles, re-stackable towers, and a sponge that reverses a mess. We do not need a hair salon stage to use this principle. A good experimental toy has an understandable way to change it again.

### Sago Mini Diner: the closest match for our object system

The designer describes movable and stackable objects, characters holding things, recipe-based or improvised cooking, and everyday tasks such as washing dishes and unpacking deliveries. The team also made the playset more close-up to help children focus on details. [Diner's designer letter](https://sagomini.com/article/diner-letter-to-parents/)

**Our application:** make the kitchen table and garden bench usable as close-up play areas inside the larger Bluey house. A plate can hold food, a character can carry the plate, and a sink can wash it. Let those actions connect instead of ending each one with a screen change.

### Sago Mini Super Juice: a familiar action with a surprising result

The developer describes altering a drink with MixBots, giving it to characters, and seeing exaggerated transformations. The design deliberately embraces plentiful ingredients, messy experimentation, and reactions beyond ordinary “tasty” or “yucky.” [Super Juice's developer letter](https://sagomini.com/article/super-juice-letter-to-parents/)

**Our application:** a blender activity can change drink color, add bubbles, and produce a temporary silly moustache or a funny voice response. Use a small authored set of reactions so behavior stays coherent and affordable to animate. Silly additions should fit Bluey's pretend-play tone; we do not need to copy the space setting or transformation catalog.

### Sago Mini Toolbox: doing the action is satisfying

The designer calls out animation and sound responding to fast or slow tool use, taps, swipes, and even turning a wrench in the opposite direction. The tool's response is part of the fun before the project is complete. [Toolbox's designer letter](https://sagomini.com/article/toolbox-letter-to-parents/)

**Our application:** scrubbing creates bubbles under the sponge; stirring moves the mixture; tapping a toy drum creates a sound at the moment of contact. Change an effect while the gesture happens, rather than showing only a completion sparkle at the end.

### Sago Mini Trucks & Diggers: material response matters

The developer describes repeatedly moving and piling dirt as enjoyable in itself, and specifically identifies metaballs as a technique used to make the dirt feel connected. This is an actual disclosed implementation detail for that title, not evidence about every Toca or Sago game. [Trucks & Diggers' developer letter](https://sagomini.com/article/trucks-and-diggers-letter-to-parents/)

**Our application:** water should slosh, sand should heap, and soap should foam. For the older A10 iPad, begin with masked fill sprites, short streams, and bounded particles. Consider a limited material simulation only if a prototype demonstrates that the simpler effects fail to provide the desired feel. We do not need hundreds of networked liquid particles.

### Sago Mini Apartment and Friends: small activities inside familiar places

Apartment uses different floors for distinct activities; Friends leads a chosen character to a house for a short shared activity. Their public descriptions provide a useful pattern for reducing how much a child must understand at one time. [Apartment's designer letter](https://sagomini.com/article/apartment-letter-to-parents/), [Friends' creator letter](https://sagomini.com/article/friends-letter-to-parents/)

**Our application:** the six-stage wheel opens a location, then the younger child's camera begins at its most inviting activity. The backyard opens on the water bench; the home opens on the kitchen table. A large picture arrow can move to another activity. This is a camera and interaction aid within the same place, not a second set of disconnected game rules.

### Sago Mini Town, Neighborhood Blocks, and Bug Builder: creation remains playable

Town's modular tiles support rearrangement and contextual character changes. Neighborhood Blocks lets brushes transform blocks and buildings. Bug Builder moves from decorating a creature into feeding, dressing, and washing it. [Town](https://sagomini.com/article/town-letter-to-parents/), [Neighborhood Blocks](https://sagomini.com/article/neighborhood-blocks-letter-to-parents/), [Bug Builder](https://sagomini.com/article/bug-builder-letter-to-parents/)

**Our application:** a sandcastle stays in the scene after creation and can receive shells, be washed, or be rebuilt. A decorated paper boat can float in the creek. The creation becomes another toy rather than disappearing behind a reward screen.

## 3. Why the objects feel so interactive

The following is my design synthesis from those sources, not a claim about their proprietary architecture.

| Ingredient | What the child experiences | What we need to build |
| --- | --- | --- |
| **Clear invitation** | “That handle looks like I can turn it.” | Readable shape, movement cue, and generous touch area |
| **Immediate response** | The object lifts, squishes, wiggles, or makes a sound as soon as touched | Pickup feedback and gesture-linked animation |
| **Changing state** | Empty becomes full; dirty becomes clean; whole becomes sliced | Explicit object state with matching artwork |
| **Several uses** | The same bucket works with flowers, a tray, and sand | Shared capabilities and compatible target rules |
| **Character response** | A friend notices, tastes, giggles, or thanks the child | Short expressive reactions tied to the actual result |
| **An easy next action** | The smoothie can now be poured or carried | Outputs that remain interactive objects |
| **Recovery** | A spill can be wiped; a tower can be rebuilt | Resettable/reversible play and safe placement |

The quality target should be **a handful of objects with connected uses**, before adding dozens of decorative props that only wobble. A one-off animation is useful for scenery, but central toys need meaningful state and more than one destination.

For example, the interesting loop is not merely “tap plant → sparkle.” It is:

**Fill a bucket → pour into a tray → float a paper boat → spill a little → wipe the puddle → squeeze the sponge into the bucket → water the plant.**

The three-year-old can enjoy any single link. The six-year-old can invent or follow a longer chain. These are our proposed interactions, not a claim that a source game contains that exact sequence.

## 4. One game with two play styles

Use parent-configurable defaults called **Simple Play** and **Explore & Stories**, with the option to change them. They describe assistance, not the child's identity or a locked age level.

| Feature | Simple Play — starting point for age 3 | Explore & Stories — starting point for age 6 |
| --- | --- | --- |
| Camera | Close-up activity area; stable view while dragging | Wider room and optional movement between activity areas |
| Visible choices | Small, clearly spaced starter tray | More available prop choices and optional recipe pictures |
| Picking up | Large pickup regions; slight lift above the fingertip | Same pickup rule, with less assistance if desired |
| Placement | Broad magnetic targets; helpful preview | More free arrangement, with legal placement still protected |
| Pouring | Drop over target to perform an assisted pour | Same drop action, plus optional hold-to-pour for partial amounts |
| Scrubbing | Broad strokes quickly clean a useful area | Finer partial cleaning and more places to experiment |
| Combining | Obvious compatible target cue | Fewer hints; optional discovery of longer combinations |
| Walking | Available, but not required before using nearby activity props | Joystick or tap walking remains selectable |
| Quests | Off by default or one optional spoken invitation | Optional short stories and two-to-four-step picture sequences |
| Mistakes | Safe return, easy recovery, no penalty | Same recovery; experimentation remains welcome |
| Speech | Short action words, character reactions, replayable hints | More varied dialogue and optional story invitations |

**Preserve your earlier controls:** joystick and tap-ground-to-walk stay available. The new recommendation is that a three-year-old should not have to steer into an exact position before using a toy. Directly dragging the bucket should work. Where a character pose is needed, the game can automatically place or approach the interaction anchor.

For a prototype, try approximately six to eight visible starter props and at most three prominent action choices in the close-up area. These are proposed layout experiments, not scientifically fixed limits or a cap on all world objects.

Research with children supports simple touchscreen gestures and large targets; individual ability still varies. Keep important controls around the previously proposed 2 cm physical size and observe your children using the actual iPad. Avoid essential pinch, twist, long-hold, or two-finger gestures. If an advanced gesture is offered, supply a simple alternative. [Nielsen Norman Group's child interaction research](https://www.nngroup.com/articles/children-ux-physical-development/)

### Simpler artwork without changing the cast

Keep the recognizable 2D Bluey character style. Simplify presentation through a calmer background, fewer competing highlights, larger important props, and one obvious work surface. Close-up camera framing and a quieter prop arrangement are more useful than creating a second incompatible art style for the younger child.

Use color and silhouette together: the bucket should look like a bucket at a glance, while a light outline indicates it can be picked up. Do not rely solely on color for matching. A small decorative flower in the wallpaper should not look more tappable than the actual plant.

## 5. How both children can play together

Each iPad can use its own camera, language, control preference, and assistance level. **World state is shared; presentation is personal.**

Example: the three-year-old uses an assisted pour on a flower. The six-year-old, in the wider garden view, sees the same bucket empty and the same flower grow. The older child can then refill that bucket or take it to the sandpit.

- Both views use the same object IDs, contents, ownership, and recipes.
- Simple Play can select a compatible amount and animate the pour automatically; the host still validates and transfers the real amount once.
- Assistance must not secretly refill a shared bucket or create a second copy. Supply replenishment comes from an explicit tap/source available under the same world rules.
- A smaller starter tray changes what the UI suggests; it must not make objects placed by the other child invisible or unpickable.
- A closer camera can follow a chosen activity locally. A one-room prototype is only an initial test: the required game lets all four players choose independent locations and meet in the same existing area.
- If one child holds an item, the other sees who has it. Provide two starter buckets if repeated grabbing conflicts make play frustrating, while preserving clear ownership of each bucket.
- Changing assistance must not reset objects, cancel the other child's activity, or advance shared quests by itself.

Start with side-by-side complementary play: one child waters, the other decorates pots; one mixes a drink, the other sets cups out. Do not require both children to perform a coordinated gesture at the same instant.

The cited commercial descriptions do not establish that these games support our proposed two-iPad network sessions. Their shared-play and multi-device language should not be treated as proof of networked multiplayer. The host/client implementation remains our custom Unity work from the original plan.

## 6. A concrete object catalog for our game

The following is proposed Bluey game content. Each row connects an obvious action with another possible use, visible feedback, and recovery.

| Object or station | Main use | Other compatible uses | Feedback / recovery |
| --- | --- | --- | --- |
| Bucket | Fill and pour | Water plants, fill tray, wet sand, carry small toys | Visible fill level, slosh, bounded spill; refill at tap |
| Sponge | Wipe puddles | Wash toy, clean plate, squeeze water into a container | Foam and shrinking dirt patch; reusable after squeezing |
| Cup | Hold a drink | Pour into another cup, serve, stack empty cups | Liquid color/level, character sip, safe placement |
| Fruit | Prepare food | Slice, blend, plate, picnic, offer to a character | Different prepared states; replenish from visible fruit supply |
| Blender | Transform ingredients | Mix colors/flavors, make foam, fill cups | Visible mixture, short motor sound, stop/start button |
| Plate / tray | Arrange items | Carry a snack group, hold loose shells, wash after use | Stable slots or surface placement; objects remain recoverable |
| Plant pot | Hold a plant | Add seed, water, decorate, move to table | Growth stage and color; optional replant action |
| Water tray | Hold shallow water | Float boat, test toy buoyancy, scoop water | Waterline and gentle bob; drain or refill |
| Paper boat | Float | Add a leaf passenger, decorate, carry to creek | Attached passenger and bounded path; retrieve at shore |
| Sand mould | Make a castle | Try dry/wet sand, add shells, rebuild | Formed shape, crumble/wash animation; refill |
| Shell | Decorate | Sort, place in tray, tap like a tiny instrument | Distinct clack and placement; no single required pattern |
| Ball | Roll and bounce | Basket, ramp, water tray, gentle character catch | Readable bounce and sound; rescue if out of reach |
| Toy car | Roll | Ramp, wash, pretend garage, carry in basket | Wheels and dirt state; no vehicle-driving skill required |
| Ramp / plank | Make a path | Roll ball/car, bridge a small gap | Preview support points; simple valid snapping |
| Cushion / blanket | Build a cosy spot | Stack, seat a character, make a fort | Stable placement and seated pose; dismantle easily |
| Brush / washable paint | Decorate | Paint pot, toy surface, paper boat | Continuous stroke or stamped patches; sponge removes paint |
| Hat / accessory | Dress up | Swap, place on hook, pretend role-play | Clear attachment and character reaction; remove by dragging |
| Toy drum / bell | Make sound | Musical call-and-response, rhythm play | Immediate sound with overlap limits; no score required |
| Picnic basket | Carry a set | Pack snacks, collect toys, deliver to blanket | Visible contents and capacity cue; easy extraction |
| Tap / drawer / light switch | Change environment | Fill source, reveal props, adjust room mood | Mechanical animation plus obvious new state; easy toggle |

Do not promise that every object combines meaningfully with every other object. Define understandable capabilities, make compatible uses rich, and give neutral, recoverable responses elsewhere. A sponge used on a clean toy might squeak; it should not flash an error or silently disappear.

## 7. Six areas, with simple activities inside them

Keep the six-location wheel, including Daycare. Every location contains a **small activity space** that can be the younger child's starting view.

| Location | Simple Play entry | Deeper activity in the same space |
| --- | --- | --- |
| **Heeler Home** | Kitchen table: fruit, cup, plate, blender | Make and serve a picnic, improvise recipes, wash dishes |
| **Backyard Garden** | Water bench: tap, bucket, plant, sponge | Grow/decorate a garden, fill a tray, connect watering and sand play |
| **Playground & Park** | Ball/ramp bench beside the picnic area | Build a small rolling course and arrange a picnic |
| **The Creek** | Boat launch with shallow water and leaves | Decorate a boat, add a passenger, arrange landing points |
| **The Beach** | Sand tray with bucket, mould, shells | Change sand state, build/decorate a castle, wash and rebuild |
| **Daycare** | A ready toy/learning table with a few large picture choices | Optional planned day, roles, shared activities and imagination stories |

The first view should already contain something inviting; do not make the younger child navigate inventory drawers before the first satisfying action. A large picture arrow can move to the next activity space. The older child can reach the same space by walking through the location.

A toy made in a close-up activity should retain its state when the camera pulls back. Cross-stage carrying follows the main guide's item-category rules; prove portable shared-item transfers early alongside independent area travel.

## 8. Quests and voices should support the play

Sago Mini's parent letter describes children creating stories with characters and environments, often without narration. That is useful inspiration for leaving room for the child's own ideas. **Your requested game still needs speaking characters and understandable spoken instructions.** The combination is sparse, useful speech plus plentiful visual and sound feedback. [Sago Mini's letter to parents](https://sagomini.com/article/sago-mini-letter-to-parents/)

Use three separate audio jobs:

1. **Action feedback:** a splash, squeak, crinkle, or clink occurs immediately as the object changes.
2. **Character response:** a short, relevant line such as “Bubbles!” or “That's a tall tower!” after a meaningful result.
3. **Optional help:** a replayable spoken invitation such as “Want to give the flower a drink?” paired with a picture.

Do not narrate every finger movement. Reduce repeated lines, lower music during speech, and stop obsolete instructions when the child changes activity. English remains the first complete recording set, with Spanish using the same line IDs and per-iPad language selection.

| Free-play action | Optional Simple Play invitation | Optional Explorer story |
| --- | --- | --- |
| Pour anywhere compatible | “Want to water this flower?” | Grow flowers and decorate a picnic table |
| Blend fruit | “Let's make a drink!” | Prepare two different drinks and deliver them |
| Wash a toy | “Shall we wash the car?” | Make a pretend car wash with wash/dry/park stations |
| Stack cushions | “Let's make a cosy seat.” | Build and furnish a blanket fort |
| Float a boat | “Put the boat in the water.” | Give a leaf passenger a ride to a landing |

Turn quests off by default in Simple Play, or show a single optional invitation. A child who repeatedly fills and empties a bucket is using the toy successfully even if they ignore every objective.

## 9. The Unity system that can support these interactions

This is a proposed implementation, not a reconstruction of the source games. Keep the existing Unity 2D, local-Wi-Fi, prerecorded-voice, and older-iPad performance decisions.

### Definitions and state

Separate an object's **definition** from its current **state**:

| Definition data | Runtime/save data |
| --- | --- |
| Stable type ID and available artwork | Stable instance ID |
| Capabilities, such as container or washable | Position, parent/holder, active stage |
| Capacity, valid attachment slots | Contents, amount, dirt/wetness, preparation state |
| Compatible recipes | Resulting variant, growth stage, attached objects |
| Sound/animation/reaction references | Current action/reservation and state revision |

Use a few reusable action families: `PickUp`, `Place`, `Attach`, `Detach`, `Transfer`, `Transform`, `Clean`, `Activate`, and `React`. A filled bucket and a cup can both use `Transfer`; their appearance, capacity, and valid contents differ.

For a first water rule, a data definition could express:

```json
{
  "id": "water-plant",
  "sourceCapability": "WaterContainer",
  "targetCapability": "Waterable",
  "requirements": ["sourceHasWater", "targetCanReceiveWater"],
  "action": "TransferWater",
  "presentation": "PourAndGrow",
  "completionEvent": "PlantWatered"
}
```

This is a proposed schema example, not a Unity API or an installed package. The action implementation must enforce bounds, ownership, and event uniqueness.

### Resolve what the child intended

When a dragged item approaches a target, choose the highest-priority valid interaction and show its preview. For a bucket near a plant, a water highlight means “pour.” A shelf highlight means “put the bucket down.” Do not change target repeatedly because one fingertip jitters near a boundary: use a small stability margin and hold the selected preview until another target is clearly better.

Prioritize a visible UI control over the world behind it. Keep each pointer owned by the action it began. A finger carrying an item must not also move the joystick or tap-walk target. Include a tap-item / tap-target alternative for a child who struggles with sustained dragging.

### Make actions transactional

For an irreversible state change such as blending or pouring, validate all participating objects, reserve them, apply the result once, release the reservation, and emit one result event. If the action cannot complete, preserve the previous consistent state.

Example: the blender consumes two ingredient instances and produces one mixture state. It must not leave the original ingredients visible inside a parent container while also creating independent copies. Store stable IDs for nested items; do not serialize the same instance twice through two paths.

For liquids, count contents in simple units. Transfer at most the source amount and the destination's available capacity. Define sinks explicitly: a plant absorbs water; a drain removes it; a designated puddle receives spills. Visual droplets do not create new inventory water when they hit the floor.

### Combinations should remain predictable

Use broad capability rules for common actions, then explicit recipes for special combinations. A berry and water can make a berry drink; a rock in the cup can simply sit there if allowed. Specify precedence when several recipes match. Preserve the intended food type, color, and preparation state through carrying, stacking, and saving.

A deterministic, bounded reaction choice can add variety without changing material rules. The host sends the chosen reaction ID so both iPads agree on the event; each iPad plays its localized line. Do not use a live LLM to decide whether two objects combine during play.

### Start with affordable visual techniques

Use sprite masks or simple fill meshes for liquids, a handful of authored states for chopped food and growth, an overlay/mask for dirt and paint, and attachment points for hats or carried objects. Stable stacked arrangements can use sockets rather than unconstrained physics. Use 2D physics selectively for balls and lightweight moving toys.

Cap puddles, foam, loose ingredient pieces, and repeated sound instances. Merge or recycle excess decorative effects while preserving logical object amounts. The A10 iPad should not accumulate a new permanent GameObject for every splash or brush dab.

## 10. Lessons from Toca's actual bug reports

Toca's public bug board records interaction failures including food continuing to cook after being removed, container items duplicating, an incorrect sandwich combination, objects drawing behind furniture, and crashes involving a flowerpot or oven contents. Some are marked fixed and others under investigation; these statuses can change. The useful lesson is the failure category, not the claim that a current app version still has every issue. [Toca's official bug board, checked September 23, 2026](https://www.tocaboca.com/kids/bug-board)

Our resulting tests should include:

| Failure category | Test for our Bluey game |
| --- | --- |
| Effect continues after separation | Remove food from the pan: its cooking state stops according to the defined rule |
| Duplicate contained item | Move a loaded plate into/out of a basket repeatedly: IDs and quantities stay consistent |
| Wrong recipe wins | Every ingredient pair resolves to its intended result, including ambiguous matches |
| Drawing order disagrees with interaction | Move a bucket around a counter: visible order and pickup target stay understandable |
| Nested object becomes invalid | Open a container during use or stage transition: no lost references or duplicated children |
| Amount escapes its bounds | Overfill, empty, cancel, and reconnect during pour: all amounts remain valid |
| Gesture ends unexpectedly | Background an iPad while dragging: no stuck holder, sound loop, or tool action |

These are meaningful tests because they exercise boundaries between systems. A test that merely checks that a “Pour” button calls a “Pour” method would provide much less confidence.

## 11. What to build first

Build a small **Backyard Water & Mess Bench** inside the existing first-location plan. It should include a tap, two buckets, a plant, shallow tray, paper boat, sponge, toy car, and sand mould. Reuse the same objects in both play styles.

The first set of working recipes should be:

1. Tap fills either bucket.
2. Bucket pours into the plant.
3. Bucket pours into the tray.
4. Boat floats in the tray.
5. Bucket wets sand in the mould.
6. Mould forms a small castle.
7. A bounded spill creates a puddle.
8. Sponge absorbs from the puddle.
9. Sponge washes dirt from the toy car.
10. Sponge squeezes its stored water into a container or drain.

Carefully distinguish water quantity from cleaning progress: cleaning dirt is not permission to create unlimited water. Keep the model simple enough to explain and debug.

Each core recipe needs a clear touch response, visible state change, useful sound, optional character reaction, and recovery behavior. Test with simple artwork first, then polish the most-used objects. Add the kitchen smoothie activity next to verify that the same framework supports transforming and serving, rather than only pouring.

### Prototype acceptance gates

- The younger child can discover one satisfying action without reading or steering a character precisely.
- The older child can find several uses for the same object and attempt a chain the game did not explicitly instruct.
- Switching play style preserves every object's state.
- Both children can use different assistance settings in the same Wi-Fi session.
- Shared transfer, creation, and consumption happen once, even after retries or reconnects.
- Drag cancellation always leaves an item in a valid place; there is no permanent accidental loss.
- Sounds respond promptly without unlimited overlap; repeated play does not produce constant voice instructions.
- The complete loop stays responsive during an extended release-build test on the A10 iPad.

## 12. How we should judge whether it is fun

Watch behavior rather than relying on the number of completed quests. Sago Mini's design-research interview emphasizes observing the difference between intended play and what children actually invent. That is particularly relevant when two siblings have different skills and interests. [Interview with design researcher Cathy Tran](https://sagomini.com/article/an-invitation-to-wonder-a-chat-with-kids-design-researcher-cathy-tran/)

For short, parent-observed sessions, record:

- What does the child touch first, and does it respond as expected?
- Which object do they return to voluntarily?
- Do they try an understandable combination we forgot to support?
- Does a near-miss drop feel like the game ignored them?
- Can they recover from a spill or misplaced item without help?
- Do they enjoy the reaction enough to repeat or vary the action?
- Does spoken help arrive when useful, or interrupt their own story?
- In co-op, are they helping, playing alongside one another, or repeatedly fighting the controls?

Treat new combinations the children invent as a content backlog. If they keep trying to wash a muddy ball, that is a stronger reason to add `Washable` to the ball than an arbitrary plan to add another decorative room.

The desired result is **a world your children can play with in their own ways**: quick, tangible cause-and-effect for the younger child and richer combinations and storytelling for the older child. This research updates the design; the Unity prototype and child usability tests remain the next implementation work.
