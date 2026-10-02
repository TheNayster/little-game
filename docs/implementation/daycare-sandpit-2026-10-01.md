# Daycare Sandcastle club — October 1

DAY-01 / LEARN-01 / FAMILY-01. The backyard sandpit is now playable directly, from Daycare Games, or through Calypso. She walks over, counts scoops into an example bucket, adds water and turns it into a tower. Children can experiment while she arrives.

Choose one of four moulds, then use the Scoop / Water / Tip pictures. Small buckets need two scoops; big buckets need three. Full dry sand crumbles when tipped, with an encouraging spoken explanation. Wet sand holds its shape. Decorate towers with flags or shells; four completed towers gain shared castle walls. Children can help with any bucket.

One start includes connected Daycare players. Late arrivals share the saved lesson; leaving, traveling or disconnecting affects only that child. The castle and two varied classmates stay until New lesson deliberately clears the build and selects different friends. Player avatars never affect NPC selection.

Research: [NAEYC sand and water play](https://www.naeyc.org/node/2375) recommends filling/measuring, shared castle building, open-ended play and teacher modelling. [NAEYC preschool design process](https://www.naeyc.org/node/2481) supports trying and revising designs. Applied as collaborative counting/capacity and damp-sand play.

The painted background sandpit was removed with built-in imagegen, retaining the shade sail and backyard. Only the playable pit remains. [Exact prompt and saved raster](../../SourceArt/Daycare/Sandpit/README.md). Six original teaching clips have [text/model/hash provenance](../../SourceAudio/Sandpit/manifest.json). Family listening and art acceptance remain pending.

Validation: focused Core four-player contributions, dry/wet experiment, migration45, JSON retention, independent departure/disconnect/reconnect and replay checks pass. Unity JSON/assets checks pass. Final Windows release server/client candidate383 has [scoped native evidence](evidence/daycare-sandpit-2026-10-01/result.json). All five native gameplay groups pass; all five processes exit zero. The existing ResetCreekBoats OnDestroy null-reference messages still appear during shutdown. No sandpit gameplay exceptions were found. No physical-device or live-server installation by this task.

Protocol3/content60/schema46 adds the shared lesson, moulds/decorations, attendance and cast. Earlier worlds and Adventure choices are retained. A compatible coordinated server/app update is needed when delivery is requested.

Next: family playtesting on the next requested update. The optional daycare day and remaining learning stations remain unfinished.

## Lesson-boundary protection — October 2

Milestone 1 only (DAY-01 / FAMILY-01). Audited baseline: clean `main` at `096ea65a5c684383e31300b35b23c839fe1d0cf5` in `LocalData/DinosaurWorld`; other unfinished checkouts were not edited.

Scoop, water, tip, flag and shell capture the current lesson number at the tool request, before automatic approach. The existing command target carries `mould@round`; the existing queue changes only the expected world revision on retry. Authority rejects an absent/malformed/different lesson before construction changes. Start, replay and Leave retain their existing behavior. Observing a different round clears the pending Sandcastle operation, mould and captured round, stopping movement only when its destination is the old Sandcastle work point. In-flight commands still finish through the queue and are protected by authority validation.

Validation run: `Tools/Test-SandpitLessonBoundary.ps1` compiles current Core and Client source with Unity 6000.3.24f1's existing compiler references, then runs `Tools/Sandpit.Tests`. All checks pass. Coverage includes all five stale operations and exact unchanged checkpoints, missing/malformed/future-round targets, the actual revision-conflict queue, valid current actions, duplicate receipts including after another reset, four connected profiles, late joining, independent departure/disconnect/reconnect, normal dry/wet construction/decorations, schema45 migration and JSON retention. The stale schema46 assertion now uses `WorldLayout.Schema`.

The managed probe invokes the actual freshly compiled client methods: stale intent cancels before a pending-send wait, its own destination stops, and current-round intent, unrelated destination/intent and in-flight state remain. Initial probe failures were test-fixture omissions (assembly resolution and an uninitialized BookCursor), corrected without game changes. This probe creates no scene or UI and is not native network/physical-device playtesting. Unity was not launched; no player build, installation or live-server change occurred. End-to-end touch/transport timing remains unverified in this milestone.

Content **67** requires coordinated client/server delivery because Sandcastle command targets changed. Save schema **49**, protocol **3**, shared command fields and the generic queue remain unchanged. Old untagged construction commands are rejected; no legacy fallback can bypass the boundary. Compiled probe outputs remain ignored under `LocalData/SandpitLessonBoundary`.

Other audit findings remain deferred, including pre-response dry-tip feedback, activity joining arbitration and limited classmate participation. No presentation redesign or Milestone 2 work was started. Next implementation, if requested separately: direct scoop interaction only.
