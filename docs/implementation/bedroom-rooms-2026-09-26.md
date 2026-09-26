# Four owned upstairs rooms — doors and persistence

September 26, 2026. **BED-2 implemented in Windows candidate 137. The rooms are architectural shells; usable furnishings and decoration are the next bounded task.** Goal coverage: WORLD-01/02, ROOM-01; inventory G-02/G-06/H-01/H-31/H-32. No complete bedroom or Home checkbox is closed.

The user accepted the Android 136 stairs/landing preview (“works great”) and requested that work begin on the rooms. That acceptance applies to the existing stairs preview, not this newer room artwork or to physical multiplayer/performance qualification.

[Construction and systems research](upstairs-bedrooms-research-2026-09-26.html) · [Home feature tracker](../home-world-feature-tracker.html) · [Art and exact generation prompt](../../SourceArt/Home/Bedrooms/README.md)

## What works

Tap any upstairs bedroom door. The character approaches it while the local room art prepares, then enters that room. Tap the ordinary door at the left inside to return to the matching hall doorway. Four separated arrival positions accommodate visitors. The four rooms share an illustrated architectural base but have separate persistent identities and owners. Door plaques identify “Your room” for its owner; other rooms use room numbers.

Every enrolled family profile, including parent profiles, owns one of the four rooms. Ownership is assigned once from the existing persisted roster during migration and saved explicitly by profile ID. Changing avatar, arriving first, visiting or disconnecting does not change it. A private world with only one known profile has one owned room and three guest rooms; it does not invent additional family profiles. Visiting does not allocate a room.

All four players can occupy separate rooms, gather in a room, or remain elsewhere independently. The owner leaving or disconnecting leaves visitors in place. Selecting Heeler Home while in a bedroom keeps the current location. Compatible held objects keep their identities across stairs and doors; a dropped Home ball persists in its actual room. Existing borrowed bucket/sponge return rules still apply. This slice does not create personal storage or new toy stock.

## Research applied to the systems

| Finding | Implemented response |
| --- | --- |
| Room instances must outlive their visuals and visitors. | Four flat `BedroomState` records contain stable room IDs and owner profile IDs in the authoritative snapshot and private saves. Validation rejects missing, duplicate or unknown ownership/room records. |
| Internal travel must preserve carried items. | `EnterDoor` validates the exact hall/bedroom route and source proximity, transfers actor and held items together, and advances the visit generation. Replays retain receipts; late commands cannot act in the former room. |
| Local room preparation must not stop siblings. | Only the traveling client prepares the destination image and walks to the door. The server commits a valid endpoint; no global scene reload or room lock is used. Cancelled/failed approaches leave the actor at a valid location. |
| A room needs reliable exits and four-person visits. | Each room returns to its own hall door with separated arrival anchors. Bedroom routes cannot act as stairs or top-level destinations. |
| Art loading must remain bounded on older devices. | All four room instances reuse one resource texture within the existing maximum of three resident/requested scenery textures. No Addressables package or extra scene manager is introduced. |
| Existing saves and offline authority rules must survive. | Additive schema 6→7 migration preserves the previous world, objects, fixtures, balloon and command receipts. Content 8/build contract 9 identify compatibility explicitly. Private continuation keeps local room edits separate; rejoin loads the PC authority. |
| Interactive furniture must be separate from the background. | Built-in imagegen produced an empty architectural base matched to the accepted hall. Future furnishings have clear floor/wall space and will be usable layered objects. |

The established private adventure and recovery adapters now accept the room schema. Recovery metadata maps contract 9 to schema 7/content 8 and continues rejecting snapshots newer than their source/destination build. The production recovery allowlist has not been expanded; candidate qualification uses isolated test families only.

## Validation

Windows release client/server candidate **137**, Unity 6000.3.24f1, compiled with zero errors and warnings. [Build/source/artifact verification](evidence/bedrooms137-2026-09-26/build-validation.json) identifies the exact tested candidate. No Android or Apple build/install was performed in this slice.

- **125 core checks passed**, including ten new ownership, migration, route, item, malformed-state, recovery and private-save checks. [Core results](evidence/bedrooms137-2026-09-26/core-results.json).
- **Six native bedroom groups passed:** actual door buttons, four separate rooms, carry/pickup/drop, same-avatar ownership, a four-person visit, owner departure/disconnect and retained objects on revisit. These groups combine related assertions; they are not six complete bedroom features.
- **Two native continuity groups passed:** exact migration from build 135 with retained enrollment, and authority loss inside a room followed by separate private progress and authoritative rejoin.
- **41 native groups passed in total:** six bedroom, two continuity, six server recovery, seven stairs, six Home, four navigation and ten Keepy Uppy groups. Individual outcomes are in the [qualification record](evidence/bedrooms137-2026-09-26/qualification.json).
- [Documentation checks](evidence/bedrooms137-2026-09-26/docs-validation.json) and [tracker coverage](evidence/bedrooms137-2026-09-26/tracker-validation.json) retain all 218 Home checks and 330 all-world catalog entries.

Inspected native captures: [hall door plaques at phone dimensions](evidence/bedrooms137-2026-09-26/four-owned-doors-phone.png), [room and exit at phone dimensions](evidence/bedrooms137-2026-09-26/bedroom-phone.png), [four visitors at tablet dimensions](evidence/bedrooms137-2026-09-26/four-bedroom-visitors-tablet.png). These are Windows captures, not physical-device screenshots. The native four-room test exercises each door separately and gathers all four in room 1; core rules also cover shared room visits.

All native tests used disposable loopback worlds or separately enrolled test families. No live family world, enrollment or device was modified. Samsung remains recorded at **136**, with stairs/landing but no working bedroom interiors; server/iPads remain recorded at **128**, iPhone at **101**. These are retained deployment records, not a fresh live connection check.

## What comes next

**BED-3:** furnish all four rooms with usable beds, rugs, shelves, personal chest/baskets, reading cushions and pictures; implement owner decoration, optional Decorate together, safe placement and undo. Preserve these four room IDs and owners while expanding their saved state. Books and the complete dinosaur/personal toy catalogs remain explicit dependencies rather than implied completed content.

**BED-4:** qualify furnished-room visuals, multiplayer, migration/recovery and available devices. Four secret rooms follow the bedroom stage, using independent persistent identities, optional entrances, reliable exits and the researched local calm settings. Kitchen and the wider Home backlog remain required.

This pass does not complete furniture-aware routing, decor permissions, personal storage, secret rooms, physical A10 memory/frame times, sustained mixed-device play, physical room-touch acceptance or coordinated deployment. The Android 136 acceptance does not qualify candidate 137.
