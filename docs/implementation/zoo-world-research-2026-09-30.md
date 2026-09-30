# Zoo world research and animal model sheets

September 30, 2026. Design and art research for Little Weeps, following the user's request for a zoo the children can travel through, living dinosaurs, animals doing their own activities, animal sounds, and a food bucket beside every exhibit. The user confirmed that the art should fit the Bluey show style.

**Recommended direction:** a connected illustrated zoo with four areas: Savanna Trail, Living Dinosaur Valley, Aquarium and Penguin Cove, and Reptile Garden. Every creature has an everyday routine and a gentle feeding response. Up to four family players can visit and feed the same animals together, with independent travel and cameras. This document proposes a 16-species starter roster; it does not limit later additions to those species.

**Status:** four draft model sheets and a written animation/feeding contract are complete. Zoo scenes, rigs, animation clips, audio files, server rules and playable feeding are not implemented. These generated drawings are visual development references, not approved final sprites or frame-perfect animation turnarounds. Preserve the current Home work and main integration hold. Zoo is a requested additional destination; the existing five destinations remain intact until the chooser is deliberately expanded during implementation.

[Model sheet source folder](../../SourceArt/Zoo/ModelSheets/README.md) · [Current project decisions](../current-decisions.md) · [Existing feature goals](../bluey-game-research-2026-09-23.html)

[TOC]

## Games and design lessons

These findings come from public developer descriptions and support pages. The games were not installed or directly played for this research; their internal AI and performance are unknown.

| Reference | Published behavior | Proposed use in Little Weeps |
| --- | --- | --- |
| [Sago Mini Zoo](https://sagomini.com/apps/zoo/) and its [species guide](https://sagomini.com/workspace/uploads/files/sagomini-zoo-animalspecies.pdf) | An animal-focused children's play space, with a varied named animal cast in the official guide. | Make the zoo easy to explore, give individual animals recognizable personalities, and mix land and water exhibits. Our dinosaur and bucket mechanics are our own proposal. |
| [Bluey Let's Play support](https://budgestudios.freshdesk.com/en/support/solutions/articles/12000094826-how-to-play-) | Players discover scene possibilities by tapping, dragging and stacking items. | Keep the feeding gesture tangible: take food, carry it, put it in a feeding spot, watch the animal respond. Avoid a management dashboard. |
| [Planet Zoo animal behavior update](https://www.planetzoogame.com/en-us/update-notes/pc/1-10-0) | Species can investigate habitat changes, use climbing frames, and respond to other animals' calls. | Give each species a small set of meaningful routines and reactions. Use occasional neighbor responses rather than making every animal start the same loop together. |

The synthesis is our design judgment: borrow children's games' direct interaction and zoo simulations' observable behavior, then simplify both to fit this existing 2D family game. No ticket prices, staff scheduling or compulsory objectives are needed for the requested zoo visit.

## Traveling through the zoo

Enter through a zoo gate from the existing destination chooser. A picture map shows four clearly different areas, with paths and arrows connecting them. Walk between neighboring exhibits or use the map for longer travel. The younger child can tap one big animal portrait to move near its viewing and feeding area; the older child can explore the continuous paths. Both views show the same shared zoo.

Savanna Trail has roomy planted enclosures and shady resting spots. Dinosaur Valley has live, breathing animals among ferns and broad trees. The aquarium includes tanks with different swimming depths and a separate land/water penguin habitat. Reptile Garden has warm rocks, branches, shaded hides and a pond. Rest places and moving places both matter: an animal resting with breathing or blinking still feels alive.

Children stay on visitor paths. Each exhibit has its own food bucket and a clear feeding station connected to the animal side. A raised leaf holder works for giraffes and Brachiosaurus; a chute reaches the aquarium; low trays suit grazers and reptiles. The bucket is visible beside the exhibit, not hidden in a menu. Animal silhouettes on the bucket and food identify what belongs there without requiring reading.

## Savanna model sheet and behavior

![Savanna animals with front side back and feeding poses](../../SourceArt/Zoo/ModelSheets/savanna-model-sheet.png)

The natural behavior column summarizes the linked zoo material. The animation and food presentation columns are proposed game choices, not a complete real animal diet.

| Creature | Natural behavior reference | Proposed everyday animation | Proposed feeding response and bucket |
| --- | --- | --- | --- |
| African elephant | Flexible trunk, moving ears, social behavior and plant feeding. [San Diego Zoo](https://animals.sandiegozoo.org/animals/elephant) | Heavy alternating steps, ear flap, trunk exploration, shade rest, a brief water spray. | Notice leaves, turn, walk slowly, reach with trunk, lift one portion to mouth and chew. Bucket contains leafy branches and plant portions. |
| Giraffe | Browses leaves using its long tongue. [San Diego Zoo](https://zoo.sandiegozoo.org/animals/giraffe) | Long measured steps, look around, browse a tree, rest. | Approach a raised leaf holder, lower/stretch neck, use tongue, chew. Bucket contains leafy twigs. |
| Plains zebra | Grazing, herd relationships, grooming and distinct stripe patterns. [San Diego Zoo](https://animals.sandiegozoo.org/animals/zebra) | Wander near a companion, lower head to graze, ear twitch, tail flick, rest. | Walk to low trough, bend neck and nibble hay. Bucket contains grass hay. |
| Lion | Social big cat and meat eater. [San Diego Zoo](https://animals.sandiegozoo.org/animals/lion) | Nap, breathe, stretch, groom a paw, walk to shade, occasional call. | Wake/notice, rise, approach slowly, lower head to prepared meat portions. No hunting scene is needed for feeding. |

## Living dinosaur model sheet and behavior

![Living dinosaur front side back and feeding poses](../../SourceArt/Zoo/ModelSheets/living-dinosaurs-model-sheet.png)

These are **live animals in the game**, rather than toy dinosaurs or fossils. Their diet and broad anatomy use museum references. Their exact colors, daily routines, social behavior and calls are creative reconstructions. Mixing these animals in one park is a fantasy zoo, not a claim that they all lived together.

| Creature | Museum reference | Proposed everyday animation | Proposed feeding response and bucket |
| --- | --- | --- | --- |
| Tyrannosaurus rex | Carnivore moving on two legs. [Natural History Museum](https://www.nhm.ac.uk/discover/dino-directory/tyrannosaurus.html) | Balanced two-leg walk, horizontal torso and raised tail, head turn, sniff, resting breaths. | Turn toward food, take slow heavy steps, lower jaw to prepared meat portions. Keep two short arms with two fingers each; do not use a kangaroo stance or drag the tail. |
| Triceratops | Four-legged herbivore with three horns and a frill. [Natural History Museum](https://www.nhm.ac.uk/discover/dino-directory/triceratops.html) | Graze low plants, walk, rub gently against a trunk, rest. | Approach a low foliage tray and crop leaves with beak. Bucket contains foliage; no charging response. |
| Brachiosaurus | Four-legged long-necked plant eater. [Natural History Museum](https://www.nhm.ac.uk/discover/dino-directory/brachiosaurus.html) | Measured walk, high browsing, slow neck sweep, rest. | Walk to raised browse rack, reach neck toward leaves, take a portion. Preserve longer forelimbs in the final rig. |
| Stegosaurus | Four-legged plant eater with plates and tail spikes. [Natural History Museum](https://www.nhm.ac.uk/discover/dino-directory/stegosaurus.html) | Slow walk, low browsing, breathing, mild tail balance. | Walk to low plant tray, lower small head, crop foliage. Keep tail spikes away from the feeding edge; colors and plate motion are authored choices. |

## Aquarium model sheet and behavior

![Aquarium creatures and penguins with turnaround and feeding poses](../../SourceArt/Zoo/ModelSheets/aquarium-model-sheet.png)

| Creature | Natural behavior reference | Proposed everyday animation | Proposed feeding response and bucket |
| --- | --- | --- | --- |
| Clownfish | Close relationship with host anemones. [Monterey Bay Aquarium](https://www.montereybayaquarium.org/animals-the-ocean/animals-a-to-z/clownfish) | Fin paddling, short swim excursions near an anemone, turn and hover. | Swim toward a small pellet cloud from a chute; take pellets one at a time. Bucket contains pictured aquarium feed. |
| Regal blue tang | Reef fish and algae feeding. [Oregon Coast Aquarium](https://aquarium.org/animals/blue-tang/) | Tail-driven swimming, gentle turns, inspect reef and graze. | Approach a seaweed holder, nibble, return to reef. This is Paracanthurus hepatus with black body markings and a yellow tail; do not confuse it with the Atlantic blue tang. |
| Adult zebra shark | Can rest on the bottom; adults have spots, juveniles stripes; eats invertebrates and small fish. [Monterey Bay Aquarium](https://www.montereybayaquarium.org/animals-the-ocean/animals-a-to-z/zebra-shark) | Rest on sand with breathing, slow tail strokes, inspect a rock opening. | Swim gently to a feeding location and take a prepared seafood portion. Its game food can show fish/shrimp; no rushing or lunging at visitors. |
| African penguin | Underwater swimming, beach activity and fish feeding. [San Diego Zoo Wildlife Alliance](https://sandiegozoowildlifealliance.org/story-hub/zoonooz/whos-who) | Waddle, preen, slide into pool, swim using flippers, climb out. | Waddle or swim to a reachable tray, take and swallow a fish. Keep the pink eye patch and breast band. |

Fish move through tank space with fin/tail motion, not land footsteps. The feeding cloud stays underwater and within reachable tank bounds. Keep mouth, fin and body deformation subtle rather than making every fish talk.

## Reptile model sheet and behavior

![Reptile front side back and feeding poses](../../SourceArt/Zoo/ModelSheets/reptile-model-sheet.png)

| Creature | Natural behavior reference | Proposed everyday animation | Proposed feeding response and bucket |
| --- | --- | --- | --- |
| Green iguana | Plant-eating lizard; use species-specific rather than generic reptile food. [San Diego Zoo iguana reference](https://animals.sandiegozoo.org/animals/iguana) | Warm-rock rest, breathing, slow crawl, branch climb, head turn. | Crawl to a shallow greens bowl and bite a leaf. Bucket contains leaves and vegetables. |
| Leopard gecko | Insect feeding and exploration were studied directly. [Published behavior study](https://pmc.ncbi.nlm.nih.gov/articles/PMC10705344/) | Peek from a shaded hide, slow steps, tongue lick, inspect nearby rocks. | Look toward bowl, approach, take one stylized insect and swallow. Bucket contains insect portions. Tiny quick feeding movements can follow a slow approach. |
| Galapagos tortoise | Plant feeding and neck reach. [San Diego Zoo Wildlife Explorers](https://sdzwildlifeexplorers.org/animals/galapagos-tortoise) | Slow deliberate steps, grazing, shell breathing movement, shade rest. | Extend neck, take several slow steps, crop greens in a low dish. Put its starting rest anchor near the feeder to keep a slow pace enjoyable. |
| Nile crocodile | Water/land movement, tail-driven swimming, basking and swallowing food. [San Diego Zoo crocodilians](https://animals.sandiegozoo.org/animals/crocodilian) | Float, blink, swim, crawl onto a warm bank and rest. | Swim/crawl toward a waterside tray and swallow a prepared fish portion. No mammalian chewing loop, attack or visitor chase. The reference describes crocodilians broadly, so species-specific refinements remain for production. |

The iguana page returned an access error during the full-page check; its indexed description and the broader [lizard reference](https://animals.sandiegozoo.org/animals/lizard) support this preliminary brief. Verify species detail again before producing its final animation rig. Resting and shaded routines should differ from the gecko's; not every lizard is a daytime sunbather.

## Feeding from the bucket

1. The child taps the bucket beside an exhibit. It opens with large pictured portions appropriate to that exhibit. A tap takes a portion into their hand; dragging is also available. Replenishment is automatic so a sibling cannot empty the exhibit for everyone.
2. Taking food makes nearby eligible animals turn or look toward the feeding area. Food held far away does not pull an animal out of its enclosure. The child can carry it to one of four pictured feeding places along the viewing edge, or tap that place to walk there.
3. When the child reaches a feeding place while holding food, an animal reserves a reachable animal-side eating spot and approaches at its gentle species pace. Dropping food in a tray/chute can also invite an animal without a player continuing to hold it. Do not teleport it into place.
4. Food is only consumed after a valid offer or placement within reach. Align mouth, trunk or beak to the portion; animate contact; remove exactly that portion; play a bite/swallow sound. Fish use the chute and a finite pellet cloud. The child sees the food disappear for a clear reason.
5. The animal pauses briefly, gives a mild satisfied reaction, and resumes its routine. A recent eater can wait while another creature approaches. The zoo does not punish children for leaving or require them to prevent starvation.

**Proposed pacing, to test with the children:** visible notice within roughly 0.5–1.5 seconds, a nearby approach around 3–8 seconds, then 2–4 seconds of eating. Tortoises may take longer; use a closer resting location instead of making them sprint. These are game tuning suggestions, not measured animal speeds or proven preschool timings.

Four players use the same exhibit and animals. Four offer places and a fair queue prevent one player locking the activity. A single animal eats one portion at a time; siblings can see whose portion it is considering. Additional animals serve other places where space permits. Reservations release when a player walks away, changes area, disconnects or cancels. A changed/canceled target cannot make a stale command consume somebody else's food. Joining late sees the current routine and feeding state; another player's travel does not reset the exhibit.

## Animation contract

Every creature needs breathing/idle, locomotion, turn, notice food, approach start/stop, eat or swallow, satisfied reaction, and return to routine. Add at least two species-specific activities from the tables. Penguin and crocodile additionally need land/water entry and exit; fish need swimming, turning and hovering/resting as appropriate. A quiet animal may rest often, but must never appear as a frozen decorative object.

Preserve one stable silhouette, palette and individual marking map through every view. The generated sheets contain some shading and pose/marking differences; simplify shading, resolve spot/stripe continuity, and check limb/finger counts against references when tracing the editable final artwork. Turnarounds are drawn at cell scale for readability, not an accurate cross-species height chart. Correct Brachiosaurus forelimb proportions, stegosaur plate rows, reptile toes and fish fin placement during final cleanup.

Produce layered editable 2D sources with body, head, jaw/beak, eyes, near/far limbs, tail and distinctive parts separated. Use trunk sections and ears for elephant; neck/head segments for giraffe and sauropod; fins for fish; flippers for penguin; shell separate from tortoise head/limbs. Store mouth/food sockets, foot or swimming pivots, resting/feeding anchors and bounds beside the artwork. A finished reference sheet alone is not an animation asset.

The server selects shared animal state, target, reserved feeding place, consumed portion and timing. Clients interpolate movement and play the same identified reactions locally. Choose different initial routine phases and bounded variation so a herd does not march in synchrony. Keep existing PC/VPS authority, private offline solo and server-owned world on reconnect. Far-away exhibits can use cheaper simulation; loading one shows the current state rather than replaying every missed animation or call. Performance must be measured on the existing older iPads; no frame-rate claim has been made.

## Free sounds and PC recording plan

These are **license-checked candidates**, not downloaded, auditioned or integrated audio. Freesound currently shows login-to-download. Its [license FAQ](https://freesound.org/help/faq/) explains its license categories; keep the original page, author, license, file checksum and edit recipe for every selected sound. Prefer CC0 and retain credits even where attribution is optional. Listening access alone is not a game asset license.

| Candidate reviewed | Listed license and type | Intended use and limitation |
| --- | --- | --- |
| [Voice elephant by vataaa](https://freesound.org/people/vataaa/sounds/148873/) | CC0; MP3, approximately 7.4 seconds | Elephant call candidate. Species/recording provenance and tone need listening review; trim and soften only after selection. |
| [lion growls by stratcat322](https://freesound.org/people/stratcat322/sounds/270383/) | CC0; WAV, approximately 5 seconds; edited from a linked source | Lion response candidate. It is already processed; inspect the original and reject it if frightening or unsuitable. |
| [Penguin Squeak by Breviceps](https://freesound.org/people/Breviceps/sounds/705839/) | CC0; WAV, 0.6 seconds; cartoon tag | Short stylized penguin response candidate, not verified African penguin field audio. |
| [CrocodilianTypeGrowl by Ovkovko](https://freesound.org/people/Ovkovko/sounds/825609/) | CC0; designed with an instrument rubbing technique | Designed reptile/dinosaur effect candidate; do not label it a real crocodile recording. |
| [Natural History Museum Jurassic sounds](https://www.nhm.ac.uk/schools/teaching-resources/key-stage-1/dinosaurs-and-fossils/jurassic-sounds.html) | Reconstruction examples; a game redistribution license was not established | Listen as a design reference only. Do not include these recordings in the app on this evidence. |

Reject the search result named Zebra-II-64-26 as an animal lead: the name alone does not establish a zebra recording. The [zoo zebra page](https://animals.sandiegozoo.org/animals/zebra) describes brays, barks and softer sounds and includes a listening reference, but app reuse permission was not established. No verified free call is claimed for giraffe, zebra, iguana, gecko or tortoise in this research.

| Creature group | Sound events required | Free source or original PC plan |
| --- | --- | --- |
| Elephant | Occasional call, heavy steps, leaf pickup/chew, water spray | Elephant candidate plus recorded leaves, foot taps and water. |
| Giraffe | Quiet chewing, hoof steps, branch movement; optional stylized breath | Record twig/leaf movement and soft breaths. A designed response is not proof of a natural giraffe call. |
| Zebra | Hooves, hay crunch, occasional bray/snort | Record footsteps/hay; make a gentle designed vocal effect if a usable recording is not found. |
| Lion | Occasional low call, paws, grooming/stretch, feeding | Audition the CC0 growl; record movement/eating Foley. |
| Four dinosaurs | Different low calls/breaths, steps, foliage or prepared-food eating | Original PC effects: low voiced breaths for T. rex, rounded short hum for Triceratops, longer breath for Brachiosaurus, soft chuff for Stegosaurus. All four calls are speculative game sounds. |
| Clownfish, blue tang, zebra shark | Water, fin/water movement, pellets/seaweed/seafood feeding | Original water and feeding effects. Give all three audible responses without claiming invented roars are their natural calls. |
| Penguin | Short call, waddle, splash, fish swallow | Cartoon squeak candidate; original splash/steps. Natural species call remains a separate sourcing task. |
| Iguana, gecko, tortoise | Light steps, leaf/insect eating, breathing and movement | Original PC Foley; mostly quiet animals. Optional cartoon reaction must be labeled designed. |
| Crocodile | Water entry, tail swish, movement, gulp; occasional designed low sound | Original water/Foley; instrument growl candidate only if gentle and clearly classified as designed. |

Use the user's PC for original recording and editing where a usable free asset is missing. [Audacity's official recording guide](https://manual.audacityteam.org/man/record.html) documents microphone recording. A future recording session can capture three to five variations of leaves, soft footsteps, water, bowl taps and voiced breaths; remove unwanted room noise, trim silence, use short fades and match perceived loudness. Keep editable originals in SourceAudio/Zoo and export compact game copies later. No microphone capture, software installation or voice generation was performed for this research.

Proposed playback rules: occasional calls rather than continuous roaring; nearby exhibits louder than distant ones; local music ducking during a call; a tap-to-hear response; and independent mute settings. Avoid repeated identical clips and a room full of overlapping calls when four children feed at once. Every species has sound feedback, but sounds can come from eating, water and movement as well as vocalization.

## First implementation and acceptance

Start with one integrated Savanna exhibit containing an elephant and giraffe, their own buckets, routine anchors and four shared feeding places. Prove the complete visit → take food → slowly approach → eat → resume sequence before expanding the content. Then add zebra/lion, dinosaurs, aquarium/penguins and reptiles using the same contract with species-specific changes.

- Confirm that zoo travel preserves held items and existing saves, and leaves the current destinations accessible.
- Watch every species long enough to see locomotion, rest, at least two distinct activities and a feeding reaction. Confirm foot/fin contact, full tails, correct food and mouth/trunk alignment.
- Test four real clients feeding the same exhibit, simultaneous offers to one animal, late joining, walking away, cancellation, disconnect and independent area travel. No duplicate consumption, permanent reservation or reset for the remaining players.
- Check fish food reachability, land/water transitions, feeders behind barriers and the tortoise's slow approach. If routing fails, return the offered portion and release the spot without teleporting an animal.
- Check muted and audible reactions, clip overlap and listening comfort on physical devices. Preserve the difference between recordings and designed sounds in the asset catalog.
- Check reconnect/server restart and the existing older iPads' frame rate and memory. Observe both children using the buckets; tune targets and pacing from their behavior.

**Research handoff:** the four images below the category headings, the prompt manifest and this behavior/feeding/audio brief are the deliverables. Further species, final visual approval, editable sprite cleanup, animation and playable integration remain future production work. This report records the new requested zoo scope without replacing the active Home implementation sequence or claiming a device update.
