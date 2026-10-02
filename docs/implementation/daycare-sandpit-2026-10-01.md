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

## Direct scoop — October 2

Milestone2 only, DAY-01 / LEARN-01 / FAMILY-01. Baseline: clean `main` at `42ba379b822bb5ce899827bcd332a7992b1b4d32`, also confirmed at `origin/main`. Work stayed in `C:\Users\sephi\Desktop\Little weeps game\LocalData\DinosaurWorld`; no other checkout was edited.

The shovel stands in a visible sand pile at world point `(4100,220)`. Its 180×150 scene-unit hit area uses `HomeHit`/`NavigationTap`, including existing drag/canceled-touch protection. It and the retained Scoop tray control call `RequestSandTool("scoop")`. Selecting a visible mould immediately paints the existing hint-ring sprite gold on that client's mould. Selection remains unsaved/local; initial lesson selection still uses that child's member slot. Selection updates before painting, avoiding an old outline on the first frame.

Request captures the local mould and current round, then uses the existing automatic approach to `DaycareSandpit.Work`. At the work point, `SendSandpit` sends `SoloAction.Sandpit`, operation `scoop`, target `mould@round`, through the existing shared command queue (or the existing local command for private solo). Completed repeated scoop taps during walking/waiting are retained as a local count and submitted one at a time; selecting another mould, another tool, Leave or a changed lesson clears/replaces the pending intent. No capacity or construction rules were copied into the client. Authority still serializes competing actions, rejects full/built moulds and old lessons, and the generic revision retries preserve the target.

`SandScoopFeedback` observes increases in confirmed shared scoop counts. Each increase starts a 0.75-second local arc of sand/particles from the shovel to that mould's lip; merged snapshot increments are counted together. Bucket fill always renders current authoritative state immediately. Rejected/unchanged state creates no new effect; initial join, re-entry/reconnect and changed lessons are baselines, with no historical fill animation. Inactivity clears presentation. The effect is noninteractive and uses the existing world depth pass in front of the children; a first runtime capture exposed occlusion when it was a bucket child, corrected before the final checked build. Animation frames and local selection are never networked or saved.

Exact changed files (all paths relative to this checkout):

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandpit.cs` — target, local selection/intent, repeated taps and presentation integration.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandShape.cs` — procedural shovel/pile and sand-flight graphic.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandScoopFeedback.cs` and `.cs.meta` — bounded accepted-state observer and Unity identity.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenVerification.cs` — inspection-only selection/highlight, pending lesson/mould and effect-event counters.
- `Tools/Sandpit.Tests/Program.cs`, `Tools/Sandpit.Tests/Sandpit.Tests.csproj`, `Tools/Sandpit.Tests/ScoopFeedbackChecks.cs` — run/link the actual feedback class and baseline/rejection/reset checks.
- `Tools/Test-DaycareSandpitScoop.py` — focused native release mouse/touch and four-client acceptance.
- `docs/current-decisions.md`, `docs/family-playset-build-guide-2026-09-23.md`, this document — scoped delivery record.

Checks actually run:

- `dotnet run --project Tools/Sandpit.Tests/Sandpit.Tests.csproj --configuration Release`: PASS shared four-profile rules, lesson-boundary/revision/duplicate handling, migration/JSON retention and new feedback tests (accepted increments, merged sibling changes, rejection/no change, expiry, crumble, new lesson, leaving/re-entry and reconnect baseline).
- `Tools/Test-SandpitLessonBoundary.ps1`: current Core and Client Unity-reference compilation PASS; the above rules/feedback suite PASS. Optional compiled-client reflection loading was blocked by Windows Application Control (`0x800711C7`). No policy bypass was attempted and that probe is not claimed as passed this milestone.
- Fresh `Tools/Build-NetworkProbe.ps1 -BuildNumber 417`: Unity6000.3.24f1 Windows release Server and Client PASS, zero errors/warnings each. Earlier415/416 were intermediate builds. The builder's bundle-version-only settings edit was restored to baseline; generated artifacts remain ignored.
- `python Tools/Test-DaycareSandpitScoop.py 417`: PASS all seven groups. Real InputSystem touches/mouse exercise each visible mould, local highlights, canceled/swiped pointers, automatic approach, two rapid taps, pending switch/Leave, late fourth arrival, four simultaneous different moulds, two same-mould clients/contested last space, repeated fill/capacity/full rejection, native client close/restart/reconnect, retained sibling progress, normal fallback water/tip/completion, built rejection and sibling New Lesson during approach. Six processes (including restarted client) exit zero. Evidence: `LocalData/SharedGarden/49bdac53bb564a968b70f3dc1da4acad/sandpit-scoop/result.json`.
- Native screenshots inspected at1280×591 and1024×768: visible shovel, large reachable target, immediate gold outline, grounded placement and safe-area controls. Additional single-client release417 captures specifically inspect corrected foreground sand mid-flight, arrival at mould1 and settled fill: `LocalData/SharedGarden/1dc2bb34ac9a4d318017783ff92c3c27/scoop-motion/`. This bounded additional check resolved trajectory/layering visibility, not another full gameplay qualification.

Initial harness attempts needed the existing Calypso start entry instead of assuming an unscrolled menu card was visible, a client-state/settling wait after world return, and actual client close/restart instead of treating network foreground-pause as departure. These setup failures are not recorded as passes. The final runtime covers the corrected fixture and final gameplay source. No new gameplay exceptions were found; two inherited `ResetCreekBoats`/`OnDestroy` null references remain in shutdown logs and are intentionally out of scope.

No physical devices were installed, no live family server was changed, and no real saves/enrollment were touched. Actual phone/tablet touch comfort, family art/animation acceptance and ages3/6 usability still require device/child playtesting. Existing pre-response dry-tip feedback, joining arbitration and classmate participation remain deferred. Water/tip/decorations, counts, NPC cast, shared saves, other games and New Lesson rules are unchanged. Schema49/content67/protocol3 stay at Milestone1's values. Stop after direct scoop; review before direct watering.
