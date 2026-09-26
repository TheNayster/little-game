# Four furnished upstairs bedrooms

September 26, 2026. BED-3 adds the first usable furniture, personal storage and decoration pass to the four profile-owned upstairs rooms. Goal coverage: WORLD-01/02, ROOM-01, G-02/G-06, H-01/H-04/H-05/H-31–34 and HOME-BED-A–D. Full bedrooms and Home remain **Partial**; this bounded slice does not finish every furnishing/content/device requirement.

[Construction research](upstairs-bedrooms-research-2026-09-26.html) · [Home tracker](../home-world-feature-tracker.html) · [Artwork and exact prompts](../../SourceArt/Home/Bedrooms/Furniture/README.md)

## What the player can do

Each room has a bed to rest on, four usable cushions, a rug, a shelf with two cubbies and two top supports, a toy chest with eight real slots, a switchable lamp and a framed dinosaur picture. Tap the bed/cushion to use or leave it; walking also leaves only that player's spot. Four people can rest in separate bedrooms or gather on four cushions in one room. The owner leaving does not evict visitors.

Every room starts with two carryable dinosaur plush toys and two soft blocks, with stable identities. Drag toys into an open chest or onto the shelf. Closing the chest hides its contents without deleting them; reopening exposes the same objects. Toys can travel through Home's internal doors/stairs. Major-world travel leaves these personal starter toys in Home; cross-world personal carrying is not part of this slice. Existing borrowed tools retain their separate return policy.

The owner can open **Decorate**, choose four palette variants, switch between two safe furniture arrangements, enable **Together**, undo their latest eligible decoration, or put their own loose toys away. Together grants visitors decoration access, but not control of the permission or the owner's tidy action. Undo reverses one decoration field and never restores an old room snapshot over newer toy/visitor activity. Tidy preserves held items, installed displays, visitor-owned items, other rooms and borrowed tools. It checks capacity before changing anything.

## Research applied

| Requirement | Implementation |
| --- | --- |
| Separate architecture, objects, occupants and covers | Eight built-in imagegen assets remain separate from the empty room background. Bed blanket, shelf lip and chest front are registered cover polygons. The accepted character sheets are reused. |
| Four equal persistent rooms | The existing four room IDs/owners remain unchanged. Each room saves its own palette, arrangement, lamp/chest state, permission and decoration revision. |
| Real persistent contents | Sixteen bounded starter objects extend the existing item registry. No second inventory or unlimited respawn is introduced. Fixed storage slots prevent container cycles; one-holder and occupancy validation prevents duplicates. |
| Safe layout and exits | Two authored arrangements move their supports, stored contents and occupants atomically. The left exit lane and front walk corridor remain clear. Both layouts reserve the far-end wall for the future secret entrance. |
| Coherent shared editing | Commands validate profile permission and expected room revision on the authority. Rejected/stale edits cannot overwrite current decoration. Undo belongs to the last eligible decorator. |
| Save/authority continuity | Additive schema 7→8/content 9/build contract 10; private local continuation stays private and reconnection loads authoritative state. Recovery tools understand the new schema and still gate production recovery separately. |

Furniture textures are shared across all four rooms and capped at 1024 pixels per importer. The small furniture set remains cached during play and releases when the presentation resets. This is separate from the maximum of three resident/requested background images. Physical A10 texture memory and frame-time qualification remain open.

## Validation and delivery

Windows release client/server **142**, Unity 6000.3.24f1, compiled with zero errors or warnings. [Exact source/artifact verification](evidence/furniture142-2026-09-26/build-validation.json).

- **138 core checks passed**, including 13 new furniture groups. [Results](evidence/furniture142-2026-09-26/core-results.json).
- **40 native groups passed:** five furniture, two migration/continuity, six recovery, seven stairs, six Home, four navigation and ten Keepy Uppy groups. [Qualification and individual suites](evidence/furniture142-2026-09-26/qualification.json).
- Native furniture checks use four clients and real touch controls: beds/cushions, same-seat rejection, owner departure, chest/shelf drag, hidden closed contents, lamp, owner permissions/Together, arrangements, undo and tidy.
- Exact schema 7→8 migration preserves the prior world and enrollment. Private decor/toy changes survive cold reopen; authoritative rejoin does not import them. Candidate recovery restores decoration/storage with the original four enrollments, including corrupt/interrupted recovery cases. Production recovery allowlisting remains separate.
- [Tracker coverage](evidence/furniture142-2026-09-26/tracker-validation.json) retains 218 Home checks and 330 catalog rows. [Plan/link checks](evidence/furniture142-2026-09-26/docs-validation.json).

Agent-inspected Windows captures: [bed at phone dimensions](evidence/furniture142-2026-09-26/bedroom-bed-phone.png), [four cushions at tablet dimensions](evidence/furniture142-2026-09-26/four-cushions-tablet.png), [real chest contents](evidence/furniture142-2026-09-26/chest-open-stored-tablet.png), [shelf and lamp](evidence/furniture142-2026-09-26/shelf-and-lamp-tablet.png), [second arrangement/decor controls](evidence/furniture142-2026-09-26/decorated-layout-tablet.png). These are desktop captures, not phone/iPad acceptance. Furniture generated using built-in imagegen; exact prompts and hashes are linked above.

All tests used disposable worlds. Intermediate candidates 139–141 exposed and corrected bed-floor/occupancy presentation and furniture contact issues; only 142 is the delivered furniture candidate. Test-only camera setup was corrected after a manual-pan capture so later exit touches target a visible door. The isolated final stair suite confirms cancellation without competing test-process timing.

## Secret-room plan, including the latest corrections

Place the small star-marked entrance **at the far back of each bedroom, away from the main entrance**. It remains hidden until the local character approaches, then reveals with a glowing star, gentle sparkles and shimmer. Revealing is independent for each of four viewers; entering still requires a deliberate tap. Use a still, calm glow for reduced-motion preferences and avoid flashing. The interior exit stays visible regardless of proximity, owner presence or entrance changes.

Each optional secret room has one persistent identity linked to its owner's bedroom. Moving or archiving the entrance preserves the room and everything inside. Four visitors can gather or explore four different secret rooms. Furniture/ownership/storage contracts from this pass are reusable; entrance lifecycle, reliable fallback exits and the calm sky/fort/plush content still need implementation and qualification. No nonfunctional secret-door button has been added.

## Remaining work

Next is BED-4 device/visual qualification of this furniture pass, then the four secret rooms as requested. Books and independent narration/bookmarks, the full 20-dinosaur/personal catalog, creation gallery, free placement and broader decor choices, richer rest/tuck-in poses, stacking, and the wider Home backlog remain open. The starter stegosaurus plush is not the complete dinosaur activity system. Current palette/layout buttons are an initial control set; a richer picture-led furnishing chooser remains future work.

Samsung remains on the user-accepted **138** room-shell preview; server/iPads remain recorded at **128**, iPhone at **101**. This task has not installed furniture on a physical device or changed the live family world. Sustained mixed-device play, physical lifecycle/rejoin and older A10 performance are not established by desktop tests.
