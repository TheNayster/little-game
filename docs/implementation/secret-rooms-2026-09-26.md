# Four secret rooms and magical far-back entrances

September 26, 2026. SECRET-01, H-35–H-38, HOME-SECRET-A–D. Based on [the research completed before implementation](secret-rooms-research-2026-09-26.md). Baseline: furnished bedroom build 142 / bfed20a. Current candidate and qualification are recorded below. Samsung remains on the accepted 138 room-shell preview; this task does not change any phone, iPad or family server.

## Playable behavior

Walk toward the far end of your bedroom. A small lavender star door fades into view with a gentle glow and drifting sparkles. It is absent at the ordinary hallway entrance and cannot receive input while hidden. Each player sees their own proximity reveal; walking near it never teleports anyone. Tap to create your optional secret room once, walk to the door and enter. Friends can use an already-created room without its owner being present.

All four secret rooms have a separate saved identity and the same capabilities. Each holds four visitors, four independently usable cushions, a four-place blanket fort with foreground curtains, rug, shelf, eight-place chest, lamp and owner decoration/Together/undo. Six carryable plush types—dinosaur, dog, cat, rabbit, bear and whale—are added once per created room. Real shelf and chest slots retain their objects through closing, travel and saving. Taking a toy through a door preserves its identity and holder.

The room has a deep-blue illustrated sky, slow green/purple ribbons and deliberate star twinkles. Local brightness has three levels, motion can be still, and ambience/chimes have independent off/soft/on levels. Preferences survive reopening and remain local through shared/private transitions. No narration, timer, cleanup obligation or score starts on entry. A room owner can hide/show the entrance, move it between two reserved far-wall positions or change its tint. Those operations retain the room and contents. The interior door and **Bedroom ←** return control work even with the entrance hidden or owner absent.

## How the research shaped the implementation

| Research decision | Concrete implementation |
| --- | --- |
| Local reveal with hysteresis; no hidden input | `SoloSecrets` uses 260/340 floor-unit reveal/hide thresholds and a CanvasGroup with separate opacity/input gating. Twelve small mesh sparkles; still mode has a steady glow. |
| Persistent room separate from entrance | `SecretRoomState` stores fixed room/parent/owner, created/active, slot/style/revision and copied furniture state. Entrance edits do not mutate toys or occupants. |
| Independent local travel with authority checks | Existing door preparation and Resources preloading; server checks source visit, proximity and entrance revision. Four staggered arrivals and carry-preserving transitions. Direct return provides a second exit. |
| One shared item identity and durable storage | Existing toy registry and furniture support contracts extend to created secrets. Exactly 27 baseline toys plus six per created room, maximum 51. No respawn on revisiting. |
| Integrated illustration layers | One new shared architecture, separate miniature door/fort/plush atlas; existing shelf/chest/rug/cushions reused. Rear/occupants/curtains remain locally ordered. Accepted character sheets unchanged. |
| Lightweight calm presentation | One bounded ribbon/star mesh, one door-star mesh, shared textures, no particle system/bloom/video. Art residency retains the existing three-panorama cap. Motion/audio do not write world state. |
| Same save/authority rules as bedrooms | Schema 9/content 10/contract 11; additive schema-8 migration; copy/validation, server, private continuation and recovery consumers updated together. PC/VPS remains sole shared authority. |

Source art, exact built-in imagegen prompts, hashes and runtime destinations: [art manifest](../../SourceArt/Home/SecretRooms/manifest.json) and [layer notes](../../SourceArt/Home/SecretRooms/README.md). The small bedroom lamp contact offset was also corrected; stored support coordinates and accepted character art remain intact.

## Qualification

Windows release client/server **146**, Unity 6000.3.24f1, built with zero errors or warnings. [Exact compiled source/artifact verification](evidence/secrets146-2026-09-26/build-validation.json).

- **148 core checks passed**, including ten new secret-room groups covering ownership, idempotent creation, exact bounded stock, entrance revision/proximity, safe return, storage, four occupants and validation. [Core results](evidence/secrets146-2026-09-26/core-results.json).
- **45 native groups passed:** five secret-room, two migration/continuity, six recovery, five furniture, six Home, four navigation, seven stairs and ten Keepy Uppy groups. [Qualification and individual suites](evidence/secrets146-2026-09-26/qualification.json).
- Four native clients exercised real door, fort, cushion, plush storage and local preference controls. Entrance movement/hiding retains visitors; both normal exit and direct return remain available. A sibling's quiet settings do not change another client or shared state.
- Exact schema 8→9 migration preserves all prior furniture, 27 objects, profiles and enrollment. An occupied secret with its entrance hidden survives outage/cold reopen. Local preferences persist across shared/private transitions; authoritative rejoin never imports private edits. Build 146 backup/restore retains new room state and the original four enrollments, including corrupt/interrupted recovery cases. Production allowlisting remains separate.
- [Tracker coverage](evidence/secrets146-2026-09-26/tracker-validation.json) preserves all 218 Home checks and 330 catalog entries. [Plan and link checks](evidence/secrets146-2026-09-26/docs-validation.json).

Agent-inspected Windows captures: [hidden near ordinary bedroom entry](evidence/secrets146-2026-09-26/bedroom-hidden-entrance.png), [magical far-back star door](evidence/secrets146-2026-09-26/magical-far-back-door.png), [secret room at phone dimensions](evidence/secrets146-2026-09-26/secret-interior-phone.png), [four fort occupants at tablet dimensions](evidence/secrets146-2026-09-26/four-fort-occupants-tablet.png), [real plush storage](evidence/secrets146-2026-09-26/plush-storage-tablet.png), [local calm controls](evidence/secrets146-2026-09-26/quiet-local-settings.png). These desktop renders establish scoped composition evidence, not phone/iPad acceptance. Exact built-in imagegen prompts and asset hashes are linked above.

All tests used disposable local worlds. Earlier 143–145 candidates are development artifacts, not deployed releases. Final 146 passes after stabilizing native control identifiers and local preference keys. The first 145 continuity attempt timed out joining its older 142 migration fixture; the fresh complete 146 continuity run passes. No live family server or physical device was changed.

## Remaining limits and next task

This is a usable secret-room foundation, not completion of every Home feature or all secret-play content. The full six-book reader, narrated factual aurora picture, dedicated cuddle pillow/tucking interactions, general stacking/picnic combinations and hiding-system integration remain tracked. Placing a plush on the floor is not a generalized stacking system. Two authored furniture arrangements and two far-wall entrance positions are supported; arbitrary placement is not implemented.

Native phone/tablet-sized renders do not establish physical-device acceptance. Older A10 memory/frame times, sustained mixed-device play, lifecycle/outage/rejoin on physical devices and a coordinated matching-server deployment remain open. No production recovery allowlist is changed by isolated qualification.

Next bounded task: user/device preview and visual/contact qualification for the furnished bedrooms and secret rooms, then the remaining calm-play interactions/reader dependency. Keep kitchen and the full Home checklist visible; no full-room or final Home completion box is closed by this source milestone.
