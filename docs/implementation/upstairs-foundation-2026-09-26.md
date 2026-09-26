# Working stairs and upstairs landing — first bedroom slice

September 26, 2026. **BED-1 prototype implemented; the four bedroom interiors and secret rooms remain unfinished.** This task researched room construction and systems, then used those findings to begin building, as the user requested.

[Construction/system research](upstairs-bedrooms-research-2026-09-26.html) · [Complete Home tracker](../home-world-feature-tracker.html) · [Art source and prompt](../../SourceArt/Home/Upstairs/README.md)

## Result

The existing living-room stairs now lead to an illustrated upstairs landing/hall, and the upstairs stair entrance leads back down. Tap the stair entrance to approach and traverse it, or steer onto the entry with the joystick. Each actor traverses independently; other players remain upstairs, downstairs or in another world. Four separated arrival positions avoid stacking everyone at the same point.

The hall provides the architectural placement for four future bedroom doors. The doors are scenery in this slice: there are no bedroom interiors, room ownership assignment or bedroom furniture yet. Secret-room creation, entrances, plush play and calm effects remain the subsequent feature stage. No Home completion checkbox is closed merely because this foundation exists.

Heeler Home remains one destination. Selecting Home while upstairs resumes the current location. Lower house/backyard coordinates, accepted character artwork, controls and existing Home fixtures remain intact. Stair foreground patches reuse the panorama pixels to place characters behind the rails. Phone and tablet native captures were inspected; final animation/contact polish and user visual acceptance remain open.

## Research applied

| Research finding | Implemented response |
| --- | --- |
| Existing major travel puts held objects down. | A separate internal stair transaction preserves compatible held toy identity and transfers its location atomically with the actor. |
| Floor-depth Y is not stair elevation. | The authority retains a valid floor endpoint and per-actor traversal progress. The client maps progress onto the visible stairs with foreground cover. |
| Players need independent places. | Added the internal `home-upstairs` area, excluded from top-level travel choices. Existing custom authority and local scenery loading remain in use. |
| Late commands can cross a doorway incorrectly. | Stair departure and arrival advance the visit generation; stale movement and actions remain rejected. Repeated accepted requests keep their receipt identity. |
| Interruption must not reopen a character in midair. | Save restoration and departure settle transit at its source endpoint; late cancellation after arrival cannot rewind the committed location. |
| Private continuation cannot use animated height as saved floor depth. | Visible-state capture retains authoritative stair endpoints, and old private adventures migrate through the current installed room schema. |
| Loading must remain local and bounded. | The destination panorama is prepared before traversal; current-room scenery and the target share the three-texture resident/pending limit. |
| Persisted room additions require coordinated compatibility. | Schema 6/content 7 and build contract 8, with explicit schema/content build metadata. Older deployed clients/server remain unchanged. |

The existing borrowed bucket/sponge return policy still applies after they are released upstairs. Major-world departure returns held borrowed stock to its original rack. A dropped Home ball can stay upstairs. Restoration continues the established policy of releasing temporary pointer holds while retaining the actual objects and their locations.

## Validation and evidence

Candidate **135** contains the final game changes for this slice. Windows release client and dedicated server compiled successfully. Earlier 133/134 were development candidates used to find and correct test setup, contact/arrival spacing and interrupted-view handling; they are not the final delivery.

- **115 core checks passed**, including ten new migration, independent-stair, item, cancellation, recovery and private-continuation checks. [Core results](evidence/upstairs135-2026-09-26/core-results.json).
- **35 native groups passed:** seven stair groups, two migration/private-continuation groups, six recovery groups, six Home groups, four navigation groups and ten Keepy Uppy/four-seat groups. Results are linked in the [qualification record](evidence/upstairs135-2026-09-26/qualification.json), separately from pending device/user acceptance.
- [Build and runtime-source verification](evidence/upstairs135-2026-09-26/build-validation.json) identifies the actual candidate and compares its runtime files with the delivered source. Generated whitespace in unrelated imported assets was removed without semantic changes.
- [Documentation validation](evidence/upstairs135-2026-09-26/docs-validation.json) checks active plans, catalog coverage and links.
- [Tracker coverage](evidence/upstairs135-2026-09-26/tracker-validation.json) retains 218 Home checks, including four bedrooms and four secret rooms; no whole-Home completion checkbox was closed.

Native captures: [stair contact on phone layout](evidence/upstairs135-2026-09-26/stairs-up-contact-phone.png), [upstairs landing](evidence/upstairs135-2026-09-26/landing-phone.png), [four players and carried bucket](evidence/upstairs135-2026-09-26/four-upstairs-and-carried-bucket.png). These are Windows captures at phone/tablet dimensions, not physical-device screenshots.

Isolated recovery covers an upstairs occupant, exact checkpoint restoration/rollback and four enrolled clients rejoining. A compatibility guard rejects a checkpoint newer than its declared source or selected destination build, even when the current validator understands it. Eight legacy/current build-schema cases also passed. The production recovery allowlist remains unchanged; candidate qualification does not deploy or designate a new family server.

All native tests use disposable loopback worlds or separately enrolled test families. No live family server, device, save or enrollment was modified. Samsung remains recorded at 130, server/iPads at 128 and iPhone at 101; these deployment records were not freshly checked. No Android/Apple release or installation was performed for this slice.

## Limits and next bounded task

This is a working stair/landing prototype, not four finished bedrooms. Next is **BED-2**: connect the four bedroom doors to four persistent rooms, assign ownership by stable family profile, and preserve room identities across visits/reconnects. Then implement usable beds, shelves, baskets, cushions, personal storage and decor permissions/undo in all four rooms. Full book/dinosaur catalogs remain separately tracked dependencies.

Secret rooms follow the bedroom stage. Their researched design includes one optional persistent room per profile, idempotent creation, entrances that can move/archive without deleting contents, guaranteed exits, four visitors, shared plush state and local brightness/motion/audio settings.

Outstanding qualification includes physical A10 memory/frame times, sustained mixed-device play, actual phone/tablet touch acceptance, user visual approval and any eventual coordinated in-place rollout. Native Windows evidence does not close those gates.

## Android preview 136

The user requested a phone preview before continuing development. Fresh signed non-development Android **136** contains the candidate-135 gameplay source and was installed in place over **130** on Samsung SM-S948U1. The installer verified both signing identities and the exact installed APK bytes, then launched Little Weeps. All **16** primary saved records remain: **15 byte-identical**, with the active world receiving schema 6/stair defaults and the expected earlier indoor-balloon migration. The existing players, props, fixtures and receipts were preserved. The upstairs location is recorded in the phone save after the user preview. [Installation and retention evidence](evidence/upstairs136-2026-09-26/android-update.json).

Bedroom doors remain scenery; the user was told that the four interiors and working doors are next. No further room development was performed during this preview. Server and Apple devices were not updated; physical A10, sustained mixed-device and broad lifecycle acceptance remain open. This preview is not recorded as visual approval.
