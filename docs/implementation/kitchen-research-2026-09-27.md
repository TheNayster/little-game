# Home kitchen implementation research — September 27, 2026

The user authorized four sequential stages: working fixtures and four dining places; persistent ingredients/tools/plates; one complete four-player pizza; then all 15 recipes. This is the active Home task on `codex/home-kitchen`, based on `6e2ca60`. The room pass, accepted movement and shared downstairs books remain intact.

## Sources used before implementation

- [Goal sheet chapter 19](../bluey-game-research-2026-09-23.md#19-kitchen-five-pizzas-five-cakes-five-meals) specifies the exact five pizzas, five cakes and five meals, picture-led Make / Decorate / Serve, optional assistance, free creations and safe unattended heating.
- [Home layer research](home-scene-layer-research-2026-09-26.md) requires one composed kitchen: clean architecture, appliance interiors/doors, rear surfaces, real contents and foreground covers. The old kitchen panorama was inspected; every appliance and dining seat was painted into it.
- [Toca Boca's official app catalog](https://www.tocaboca.com/apps), checked September 27, describes choosing and preparing food and open-ended experimentation. It supports flexible pretend play; it does not establish multiplayer implementation details.
- [Unity uGUI Image documentation](https://github.com/Unity-Technologies/uGUI/blob/main/com.unity.ugui/Documentation~/script-Image.md) and [Mask documentation](https://github.com/Unity-Technologies/uGUI/blob/main/com.unity.ugui/Documentation~/script-Mask.md), checked September 27, support explicit raycast targets and clipped child graphics. Closed-container authority checks are still required independently of visuals.

## Applied design

1. Keep kitchen/dining inside the existing continuous Home property. Remove painted duplicates from the kitchen base. Four counter positions, four oven positions and four seats have independent state. Fridge/cupboard contents use persistent support IDs and are neither visible nor pickable through closed doors.
2. Seed bounded reusable cookware, plates, tools and ingredient packages once. Ingredient packages contain counted portions with a durable batch identity; each transfer consumes a distinct portion once. Empty packages can be deliberately replenished without spawning more world props. Food state belongs to its cookware/plate and moves with that actual container.
3. Each cook may use a different tray or contribute sequentially to the same unheld creation. Authoritative revision/receipt checks settle competing transfers. No kitchen-wide lease. Recipe steps have large tap alternatives; gesture assistance does not bypass authority.
4. Heating advances only on authority play time, stops at ready and never burns food. Removing a dish pauses heating. Cold reopen preserves progress. Leaving the room does not reset anyone's dish.
5. Slicing changes a whole dish into four bounded portions. Serving removes the selected portion from the source and places it on a real empty plate, including recipe, ingredients and decoration coordinates. No hidden selectable whole dish remains after its portions leave. Tasting consumes portions; washing reuses the empty dish.
6. Define 15 distinct recipes from the goal sheet, with different assembly operations and silhouettes rather than recolors. Keep custom topping choices valid. Preserve the wider album/customer/drinks backlog unless those features receive their own implementation evidence.

## Acceptance sequence

- [x] Fixtures: open/close, actual support storage, four independent seats, correct rear/content/front order.
- [x] Inventory: exclusive holding, bounded stock, migration, closed-container rejection, portable food state, no duplication.
- [x] Pizza: assemble/spread, toppings, safe heating, slice, serve four plates, taste and wash; simultaneous players and independent departure.
- [x] Catalog: all 15 appropriate operation paths; native UI review, source/artifact checks, migration/private rejoin/recovery.

Physical A10 performance, mixed-device sessions and user visual acceptance remain separate from desktop/native tests. No live family server or private media is used for testing.

[Prototype acceptance evidence](home-kitchen-2026-09-27.html) records the exact build and scope of each completed check. These checks cover the four authorized prototype stages, not all bespoke animation, optional orders/album/drinks or physical qualification.
