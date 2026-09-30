# Little Weeps daycare world wishlist

Collected from the project docs on September 30, 2026. This file gathers the recorded daycare goals so the family can choose a first feature. This is the scope list, with current progress recorded below; it does not approve every proposed detail in the older research.

The current status is **walkable scenery, The Adventure, Calypso routines and shared picnic counting exist; the day board, full classmates, eleven other learning stations and eight other stories remain planned**. [The Adventure implementation](implementation/daycare-the-adventure-2026-09-30.html) supports one shared story for up to four players who can come and go freely. The existing scenery runs from a timber playroom to an outdoor cubby garden. The September 30 complete character selector provides reusable character artwork, but it does not establish working daycare classmates or teacher behavior.

## The daycare you wanted

1. **A playable daycare with several connected areas.** Classroom, outdoor play yard, book corner, art and sensory tables, pretend kitchen, quiet cushion nook and imagination mat. These are areas within Daycare rather than separate destinations in the main world menu.
2. **Calypso as the teacher.** She greets children, reads, observes play, offers help and rests. Short spoken invitations and pictures make activities understandable without reading. A station remains usable even when she is elsewhere in the room.
3. **All requested child characters available.** An illustrated friend board helps find or invite classmates. Friends can sit, draw, read and build across different areas, and make room when a player wants an essential toy or station. The all-ages group is an intentional family-game adaptation.
4. **An optional pretend day.** A picture board offers arrival/cubby, read-aloud, an adventure, snack/counting, another adventure, music/art/discovery, an optional third adventure, and quiet story/goodbye. Children can skip, repeat, leave or just play freely.
5. **Two or three varied activity invitations.** Draw from working activities around the game, such as cooking, science, dinosaurs, fishing, park, creek, beach and imagination stories. Avoid recent repeats and several variants of the same activity. Save the selected cards so reopening or joining does not reroll them. Only include activities whose required content and solo fallback actually work.
6. **Personal choice within shared family play.** Each player can follow their own day card, replace an unstarted suggestion or take an optional field trip. Accepting a trip moves that player; it does not pull the family away from their activities. Starting a new pretend day must preserve another player's current plan and activity.
7. **Playful learning with spoken help.** Start with pictures and direct taps or forgiving drags. Offer a demonstration when useful and deeper choices when wanted. Help is per player and per skill, with no compulsory answers, grades, streak loss or age-based character/story locks. Complete English first, then reviewed Spanish, including suitable sound and rhyme examples.
8. **An imagination mat with all nine stories.** Choose a story picture, hear its premise, choose a pictured role and enter its illustrated space. Favorite character and story role are separate choices. Keep change-role, repeat-instructions and return-to-daycare controls available.

The requested child roster in the goal sheet is Bluey, Bingo, Muffin, Socks, Chloe, Coco, Honey, Indy, Mackenzie, Rusty, Jack, Snickers, Winton, each of the three Terriers, Pretzel, Lucky, Chucky, Judo, Pom Pom, Winnie, Jean-Luc, Lila, Missy, Buddy, Bentley, Juniper, Lulu, Dusty, Dougie and Hercules, with Digger, Mia and Captain included as optional older children in that original roster. The later complete selector includes those older children. Preserve Dougie's visual communication support. Bandit and Chilli remain selectable player characters; Calypso's daycare teaching routine is separate planned work.

## The twelve learning stations

These are the recorded starter designs. Existing Home book/science systems can be reused, but the corresponding daycare teaching activities remain planned.

| Station | First playful action | Optional deeper play |
| --- | --- | --- |
| Talking dinosaur book | Listen to a short page and tap a dinosaur to hear its name | Answer a spoken picture question or predict the next picture |
| Sound basket | Match an everyday sound to a large picture | Match initial sounds or spoken rhymes |
| Letter delivery | Match a parcel's letter shape to a cubby and hear its name | Choose by letter sound, then try a few simple words with help |
| Tell the story | Put two pictures into first/next slots | Arrange three or four events, choose an ending or act it with toys |
| Picnic counting | Give each of one to three guests a plate and hear the count | Prepare larger sets, count altogether and share |
| Give the dinosaur a snack | Match one or two fruit pictures to a plate | Give a spoken number, then add or take away one |
| Compare and sort | Sort big/small objects or two different shapes | Compare sets, order lengths or change the sorting rule |
| Shapes and patterns | Place a shape in an outline or continue a simple pattern | Rotate shapes, make longer patterns or copy a block design |
| Copy the drum | Tap a drum and hear an immediate sound | Echo two to four gentle beats or add another instrument |
| Musical painting | Touch high/low or slow/fast sound pictures and see movement | Arrange picture music tiles and hear the family's parts together |
| Friends and feelings | Match a face to a spoken feeling and offer a plush or wave | Choose caring responses in a pretend scene |
| Predict and discover | Predict float/sink with a picture, then drop a toy in a tub | Compare materials and explain with picture choices |

## The nine imagination stories

The story names are requested scope. The playable mechanics below are the docs' proposed adaptations, rather than fixed approved scripts.

| Story | Recorded play idea |
| --- | --- |
| **Calypso** | Build a connected pretend town with a café, house, pond and gnome garden. Cook, build, fish, deliver or protect the garden; offer useful creations to the other players. |
| **Helicopter** | Share a gentle guided trip as pilot, map/passenger helper or rescue helper. Lower a basket to rescue a toy kangaroo and deliver supplies. |
| **Wild Girls** | Connect a woodland and farm. Plant, water, gather food, make a flower arch and prepare a shared picnic as woodland friend, farmer or kind Pink Witch. |
| **Early Baby** | Care for a doll, bring blankets, choose a lullaby and help a friendly dragon find its blanket. A calm pretend nursery and fantasy delivery activity. |
| **Typewriter** | Gather picture tiles, cross a bridge, use a playful shield and arrange a narrated story as storyteller, shield helper or trail finder. |
| **Mums and Dads** | Choose flexible family roles. Set a table, feed a doll, pack a bag, ride a toy bus or deliver snacks, changing roles freely. |
| **The Adventure** | Build a magical kingdom, gather picnic supplies, open a bridge and use a wand to wake NPC statues. Share explorer, rider/helper and wand roles. |
| **Space** | Share a cosy astronaut trip. Use a star map, collect a space rock, greet an alien, assemble a rover or grow a space garden. |
| **Explorers** | Sail a friendly ocean as captain, lookout, map helper or shore explorer. Follow landmarks, carry a picnic parcel and choose island or home stops. |

## How family play must work

The newer September 28 instruction in [AGENTS.md](../AGENTS.md) governs future games: **up to four players must be able to play together in one shared session**. Four separate solo copies do not satisfy that requirement. Where a game has rounds, use a shared invitation/readiness flow, common start or countdown and common progression. A free-play station should expose shared objects and contributions; independent local assistance must not create conflicting game state.

- Keep activities usable alone with NPC help, while allowing all four family players to join the same activity and contribute meaningfully.
- Joining receives the existing progress and available roles. Leaving releases only that player's tools or seat; an NPC or autopilot fills an essential role so the others continue.
- Players may change characters or roles without losing their completed contribution. Duplicate favorite characters remain supported.
- Preserve story props and checkpoints. When everyone leaves, save the session for a later return.
- A player may stay in daycare while others enter a story or another world. Separate locations remain possible, and each story must still support the family playing together.
- Keep hints, language, book controls and camera local where appropriate. Use the PC/VPS as shared authority; offline solo stays in separate private saves.
- Keep comforting exits available. The proposed stories avoid distress penalties, compulsory freezing of player avatars or trapping a player when another leaves.

The older research sometimes says “both children” or describes separate activity instances. Read that as support for personal choice and independent locations, alongside the newer requirement for four people to share every future game.

## A suggested first feature

**Calypso plus a shared picnic-counting table is now implemented as the next bounded slice.** Four pictured friends receive a plate each; players may tap a place or use the large arrow. [Implementation and evidence](implementation/daycare-calypso-counting-2026-09-30.html). Deeper counting and draggable-plate variants remain planned.

Use a small part of the existing playroom: Calypso, one obvious picnic picture, a shared table with places/tools for four players, pictured guests and draggable plates. Calypso gives a short spoken invitation; placing a plate makes something visible happen and counts that committed placement once. The family can set the table together, skip the prompt or leave independently. Start with one to three guests and optional extra counting, rather than producing all twelve lessons at once.

That would establish the teacher, touch interaction, shared contribution and spoken teaching loop in one useful slice. Suggested follow-ups are the talking-book and drum activities, the saved day board, one complete Calypso-town story, Helicopter and later Space. The build guide places daycare/learning before the story batch and asks for one complete story before expanding to the rest. Keep the remaining stations and stories in this wishlist.

For the first implementation, use one focused check of the changed activity, including four players contributing and one leaving while the others continue, then accept the family's playtesting. Broaden checks only for relevant changes or a concrete failure, following the latest project instructions.

## Details still proposed

- A pretend day lasting about 20–30 minutes when following invitations, and guided lessons of roughly 1–3 minutes, are older design estimates. They are not confirmed timing requirements or limits on free play.
- Exact classroom layouts, teacher lines, role pictures and story steps need selection and artwork as each feature is built.
- Learning extensions and assistance need family feedback; the records do not establish educational outcomes.
- Older five-destination descriptions predate the Zoo addition. This wishlist does not change the current main menu or destination inventory.

## Source documents

- [Game goal sheet](bluey-game-research-2026-09-23.md): sections 3–4 and 40–43 contain the daycare goals, twelve lessons, nine stories, routines and proposed build order.
- [All-world feature audit](all-world-features-audit-2026-09-26.md): Daycare section separates scenery, planned routines, lessons and stories.
- [Family playset build guide](family-playset-build-guide-2026-09-23.md): batches G7-C/G7-D and DAY-01/LEARN-01/IMG-01 record sequence and implementation status.
- [Current decisions](current-decisions.md) and [project instructions](../AGENTS.md): newer roster, shared play, PC/VPS authority and proportionate-check requirements override historical proposals.
- [Scenic-world implementation](implementation/scenic-worlds-2026-09-25.md): evidence for the existing timber playroom and outdoor cubby garden.
- [Toca and Piknik interaction research](toca-piknik-interaction-research-2026-09-23.md): daycare's ready toy/learning table and accessible starting space.

Documentation only: no game code, artwork, saves, devices or live server changed. The original collection was documentation only. Later progress includes The Adventure and the Calypso/counting prototype; their separate implementation records contain evidence.
