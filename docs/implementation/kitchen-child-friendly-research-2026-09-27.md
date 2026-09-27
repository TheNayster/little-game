# A simpler Home kitchen — applied research, September 27, 2026

**Later implementation — candidate 166:** [The researched ease-of-use correction is now implemented](kitchen-easy-2026-09-27.html), with scoped engineering evidence and physical acceptance still open. Samsung remains 162. The dated research/prototype record below describes its original milestone.

**Status: research and proposed design; not implemented or installed.** This follows the user's review of Android 162. It supersedes the earlier kitchen interaction plan where they conflict. H-07–10 / COOK-01 and all fifteen recipes remain Partial. This pass reviewed the user's reference, retained screenshots, project requirements, research papers and official product/accessibility sources. It did not audit gameplay code, run gameplay tests, build an app or contact a device/server.

## What needs to change

The user likes the concepts but wants a large central dish with easy-to-tap ingredients around it. They reported that the dining table blocks the fridge/oven, tray selection does not work, and opening drawers to find ingredients is confusing. These are usability defects to resolve, not instructions the child should have to learn.

The supplied `7359.jpg` is a collage of search results, not one identified app. Its useful reference is spatial: a prominent top-down pizza/plate, surrounding ingredient bowls, recognizable food and little competing interface. It does not establish those apps' actual usability, quality or multiplayer support.

Retained evidence supports the reported mismatch:

- [Actual Samsung plate screen](evidence/kitchen162-2026-09-27/phone-kitchen.png): an empty plate, a grey “Choose a tray” control, textual tabs and appliance controls. The screenshot establishes its appearance; the user establishes that selection failed. The underlying input cause was not investigated in this research pass.
- [Preparation screen](evidence/kitchen162-2026-09-27/pizza-worktop-phone.png): a relatively small food area on the left and a dense grid of 25 ingredient packages on the right. The foreground table/chairs overlap the appliances.
- [Recorded native checks](evidence/kitchen162-2026-09-27/native-kitchen-build161.json) prove scoped cooking operations, including deliberate appliance opening. They do not prove that a new child can discover the sequence. The current feedback is an open acceptance failure despite those earlier passes.

## Evidence and its limits

Sources below were checked on September 27, 2026. Study findings, product descriptions and our proposed choices are kept distinct. None of these sources tests this game or establishes an optimal number of bowls for this family.

| Source | Relevant evidence | Application to Little Weeps |
| --- | --- | --- |
| [Hiniker et al., 2015 — Touchscreen Prompts for Preschoolers](https://faculty.washington.edu/alexisr/TouchscreenPrompts.pdf), study of 34 children aged 2–5, following a review of 100 apps | In the tested tasks, understanding which gesture to use was a major obstacle. Audio and hand demonstrations became more useful with age; under-threes relied on an adult model. Visual state changes alone did not reliably teach the tested gestures. The study used unfamiliar swipe/double-tap/shake tasks, not our ingredient taps, and was not a natural home-play trial. | Make a productive tap obvious. Short speech and a brief demonstration may help but cannot rescue hidden prerequisites. A glow is an attention cue, not the entire instruction. |
| [Vatavu, Cramariuc and Schipor, 2015 — Touch interaction for children aged 3 to 6](https://www.sciencedirect.com/science/article/pii/S1071581914001426), publisher abstract/section summaries | Eighty-nine children and thirty adults performed touch tasks. Accuracy and performance improved with age; young children had difficulty with precise acquisition and smooth dragging. Full publisher text was inaccessible in this session, so no detailed numerical target specification is inferred from it. | Use forgiving ingredient targets and tap completion. Preserve optional dragging for children who enjoy placement, without making precision a requirement. |
| [Soni et al., 2019 — TIDRC framework](https://init.cise.ufl.edu/wp-content/uploads/sites/378/2019/04/soni-et-al-idc19.pdf), especially Appendix A and discussion | Synthesizes 57 recommendations and evaluates 50 iPad apps. It supports recognizable graphics, clear interactive foregrounds, simpler navigation, feedback and forgiving touch areas. The authors acknowledge conflicting gesture/animation evidence and limits in the sampled apps. It is a research synthesis, not proof that every recommendation works for every child. | Quiet worktop background, recognizable ingredients, stable positions, short contextual actions and a reversible decorating flow. Avoid a dense inventory menu or several equally prominent commands. |
| [Google — Touch target size](https://support.google.com/accessibility/android/answer/7101858?hl=en-GB) | Recommends at least 48 × 48 dp with separation, and distinguishes hit area from visible artwork. | Treat this as a platform baseline, not a toddler optimum. Prototype larger main controls and measure them on actual devices; Unity canvas pixels are not automatically dp. |
| [W3C — Dragging Movements](https://www.w3.org/WAI/WCAG22/Understanding/dragging-movements.html) | Describes equivalent single-pointer alternatives to dragging. This is web accessibility guidance, not a claim of Unity/WCAG certification. | Every required cooking gesture gets an equivalent tap action; an optional drag must not unlock content unavailable to tap users. |
| [Bimi Boo — Pizza Games for Kids: Cooking, developer App Store listing](https://apps.apple.com/cz/app/pizza-games-for-kids-cooking/id6447359628?platform=ipad) | Describes guided cooking with step-by-step recipes and character reactions. These are developer product claims; store ratings and age labels do not prove toddler usability. | Use a visible cooking sequence as an invitation. Keep the existing fifteen foods accessible without presenting all ingredients and tools at once. |
| [Sago Mini — Diner](https://sagomini.com/apps/diner/) | Describes recipe-book cooking and flexible customer/employee pretend play for preschoolers. | Return the finished creation to the shared house for serving and pretend play. Orders remain optional future content. |
| [Sago Mini — Super Juice](https://sagomini.com/apps/super-juice/) | Describes mixing with visible character transformations and reactions. No independent developmental effectiveness is inferred from its marketing. | Show the consequence of an ingredient touch on the food. Sound and animation should explain that change. |
| [Toca Boca — official app catalog, Toca Kitchen](https://www.tocaboca.com/apps) | Describes choosing, preparing and serving food with character reactions. Its four hungry characters are NPC recipients, not evidence of four simultaneous network players. | Preserve experimentation and unexpected combinations. Our four-human-player behavior comes from the family's requirements and must be tested separately. |

The strongest conclusion is to reduce the gap between a visible object and a useful action. **A tap on cheese should visibly put cheese on the pizza.** Enlarging the current menu while retaining closed-drawer requirements would leave the central problem unresolved.

## Proposed screen and room composition

These measurements are starting design values to test, not findings asserted by the papers.

**In the house:** keep the kitchen and dining area connected, but position the table/chairs beside the appliance bank instead of in front of it. Preserve four dining places. Check open and closed door art, contents, tap targets and four seated characters across camera positions. Enlarging invisible appliance hit areas through the table is insufficient: the child should see what they are touching. Relocation must preserve fixture/support identities, occupied seats, held items and saved dishes.

**At the worktop:** open a landscape close-up with one large dish in the center. Surround it with a small stable set of illustrated bowls, leaving the food unobscured. Use a simple board or placemat beneath the food and quiet surroundings. On a narrow phone, arrange bowls in two side columns; on a tablet, a wider semicircle can fit. Do not shrink everything to fit a complete inventory.

```text
 Back            Food picture / small profile badge            Help

 [sauce bowl]             LARGE PIZZA                 [cheese bowl]
 [pepperoni]           on a wooden board               [vegetables]
 [mushrooms]          visible actual toppings          [more bowls]

 Undo                   [oven picture: Bake]                Finished
```

The sketch shows composition, not final game art or a claim that all these controls appear at every step. Use roughly 45–55% of usable screen height for the dish at topping time. Begin with four to six ingredient choices, usually the current recipe's useful ingredients plus a pictured “more” basket. Keep locations stable during a stage. Other ingredient pages use large visible arrows; no hidden swipe-only navigation. Main bowl/action hit areas should start around 64–80 dp-equivalent or larger, subject to phone fit and measured physical size. Never shrink below the platform baseline to fit another row. Verify safe areas, contrast, finger occlusion and spacing on Samsung and both target iPads.

Food must become visually richer as the child works: dough changes shape, sauce covers its surface, cheese scatters, toppings appear at meaningful scale, and baking changes the same creation. Keep each child's chosen toppings after baking and on the corresponding slices. A generic finished illustration that replaces their arrangement defeats this interaction. Use bounded layered sprites/masks, not a costly fluid simulation. Bespoke cake/meal transformations remain tracked beyond the pizza correction.

## Entry and cooking flow

| Situation | Proposed behavior |
| --- | --- |
| Tap the worktop's pictured Cook control or an empty usable tray | Open large food choices, then bind an available real tray automatically. No separate required tray-selection step. Resume the player's in-progress creation when appropriate. |
| Tap an empty plate | Offer a working pictured Make food action and any ready dishes available for serving. Do not show a dead-end “Choose a tray” label. |
| Tap sauce or cheese | Consume one real portion and apply it visibly. A single tap can spread/scatter; optional rubbing or dragging gives more control. No accuracy score. |
| Tap a topping bowl | Place a piece automatically at a sensible free point; optional drag chooses a clamped point on the food. Keep pieces large enough to recognize. |
| Need a utensil | Bring the appropriate tool into the close-up. Tap to perform the action; broad dragging/scribbling is optional. Do not require finding a spoon in a drawer first. |
| Ready to heat | One large oven/Bake action handles placement into a free real oven position. Show that same dish baking, then ready. No door-opening chore or repeated confirmation. |
| Serve | Offer visible plates and tap-to-transfer slices, followed by carrying into Home. No mandatory tidy-up or tasting step before leaving. |
| Need help | Offer a short contextual cue such as “Tap the cheese” with a matching image/demo. Stop prompting when the child acts; respect local audio and reduced-motion choices. |
| Leave or close | Save the same dish and release only local controls. Baking remains safe; other players continue. Return to that creation later. |

**First corrected pizza path:** Cook → pizza picture → central dough → sauce → cheese → optional toppings → Bake → cut/serve. Rolling and other preparation can be playful tap/gesture steps, with the existing ready-base assistance available. Show one prominent useful action for the current stage. Make / Decorate / Serve becomes a small picture guide with genuinely useful stage behavior, not three text tabs that leave the same dense inventory onscreen.

All fifteen recipes remain reachable through pictured pizza/cake/meal families. This is progressive presentation, not deletion of recipes. Default assistance makes the first pizza easy; deeper preparation and free creation remain available. Unexpected toppings are allowed. A missing suggested ingredient must lead to visible help or a valid custom dish, not an unexplained refusal to bake/serve.

## Ingredients without cupboard chores

Ingredient bowls in the close-up are views of bounded shared stock, not newly spawned unlimited items. Starting assisted cooking makes the relevant stock accessible through an explicit kitchen-preparation transaction; it must not depend on the global fridge door remaining open. Manual fridge/cupboard play stays functional independently. Do not merely remove closed-container protections from general object pickup.

Design the assisted stock contract before implementation: each accepted addition consumes one durable unit from an eligible unheld communal package. It must not take another player's held ingredient, private stored item or reserved portion. Two players touching the last unit must receive one accepted consumption. Show availability honestly; an empty bowl offers a clear pictured replenishment action using the existing bounded-package rule, not a text error or infinite loose props. Any intermediate reserved portions must retain ownership/identity and be returned or settled safely on cancel, disconnect and reopening.

The simplest initial approach is consumption at the accepted ingredient action, avoiding speculative per-client food copies. If advance reservation is needed for a guided recipe, specify and test its release/recovery separately. Kitchen assistance is an authority-validated operation, not permission for a client to mint stock.

## Four independent players and durable creations

- Each player has a local close-up and an identifiable real creation. Prefer their current dish, otherwise atomically select an eligible empty tray from the four available. Opening a screen must not clear or steal a tray. If all are occupied, show those creations and the allowed resume/help choices instead of silently resetting one.
- Four people may prepare different foods, bake independently, decorate or serve simultaneously. Only the affected item/transfer is serialized; no global kitchen lock. A deliberate shared-dish action may allow collaboration without consuming the same ingredient twice.
- Per-tray default assignment should survive avatar changes and ordinary travel by using stable profile/dish identity. If this requires new persisted ownership fields, add a forward migration; do not infer ownership from color, avatar or device slot.
- Decorating undo applies only to an eligible uncommitted recent change. Never replay an entire old dish snapshot over another player's edits or restore already eaten/served food. Permit moving/removing an unbaked topping within the same dish; any refund must conserve the original unit. A new recipe cannot overwrite an existing creation without a deliberate, clearly pictured action.
- The world object is the source of truth for toppings, preparation, heat, portions, support and holder. Network acceptance controls lasting food changes. Local touch feedback can start immediately, but cannot invent a successful stock transfer.
- Closing a door, closing a cooking view, leaving Home or losing one connection cannot stop the other three. PC/VPS authority and private offline saves remain unchanged; no offline merging.

## Bounded implementation order and acceptance

The next slice is **clear appliance access plus one easy pizza flow**. Finish and review that slice before extending the new presentation to the remaining foods. Existing recipe paths stay available during the work.

| Check | Required evidence before calling the correction done |
| --- | --- |
| KUX-01: room access | Phone/tablet captures with doors open/closed and four diners; direct taps reach visible fridge, oven, cupboards and seats without the table intercepting or obscuring them. |
| KUX-02: working entry | From a normally saved kitchen with doors shut, Cook and empty-plate Make food lead to a usable creation; no dead control, prior adult setup or hidden tray chore. Cover held trays, all occupied trays and resume. |
| KUX-03: tap-only pizza | Make, bake and serve using taps, without manual cupboards, reading a paragraph or precision dragging. Show immediate sauce/cheese/topping changes; keep the exact decoration through baking, slicing and carrying. |
| KUX-04: mistakes and help | Off-center taps, long contact, rapid taps, optional drag then tap, interruptions, muted hints, empty stock and full dish all have understandable behavior. Gesture input must not swallow the next tap. |
| KUX-05: four cooks and conservation | Four concurrent dishes and serving operations; simultaneous last-ingredient use; one cook leaves/disconnects; no duplicated units, stolen held objects, cross-player reset or global lock. |
| KUX-06: migration and devices | Preserve old food, support identities, rooms, enrollment and saves across layout/ownership changes. Cold reopen, carry upstairs, safe mid-bake interruption and authoritative rejoin. Fresh source/artifact verification for any later phone build. |
| KUX-07: actual child use | On the target phone/tablets, observe first-use cooking before explaining the intended steps. Record where assistance was needed, mis-taps and whether the child can repeat the activity. Adult-native test success alone does not close this check. Under-threes may still need a parent demonstration; do not promise universal independent use. |

Use short, voluntary play attempts rather than a speed challenge. Compare ordinary first use with a single demonstrated example, and note the difference. Prioritize finishing a pizza, finding ingredients and returning to play over stars, scores or timed rewards. Hardware responsiveness and older A10 iPad performance remain separate measurements.

## Work record

Research completed; the proposed correction is recorded in current decisions, the Home tracker, chapter 19 and the build work record. No game source/art, save schema, installed build or device state changed. Build 162 is still the last recorded Samsung installation. This report supplies the next implementation contract; it does not mark any correction accepted.
