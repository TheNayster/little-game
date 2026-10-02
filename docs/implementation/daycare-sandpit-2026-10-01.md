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

## Direct watering — October 2

Milestone3 only, DAY-01 / LEARN-01 / FAMILY-01. Starting checkout: `C:\Users\sephi\Desktop\Little weeps game\LocalData\DinosaurWorld`, clean `main` at `927b1e8c3a4f99fe7d00a24841f43fc2734bd428`, matching `origin/main`. Milestone1 `42ba379` is an ancestor. No other checkout or concurrent work was changed.

The can rests on a floor shadow at world `(4310,180)`, beside the shovel. A160×130 scene-unit `HomeHit` target uses the existing `NavigationTap` touch/click and cancellation abstraction. Measured target bounds are approximately118×96 screen pixels at1280×591 and154×125 at1024×768; the native check confirms no overlap with the shovel or visible mould selection targets. No dragging or new screen panel is required. The existing tray can artwork is reused by the same `WaterCan` drawing helper, adding a visible open handle.

Flow: `HomeHit` → `RequestSandTool("water")` → existing locally selected mould/captured lesson → automatic approach to `DaycareSandpit.Work` → `CheckSandpitInput` → `SendSandpit("water", mould, capturedRound)` → `mould@round` → existing shared queue/`FamilySession`/`SandpitOperation` (or the existing private-solo command). No construction validation was added to the client. Server behavior stays exactly as inspected: Water is allowed on empty, partial and full unbuilt moulds; sets `wet=true`; already-wet Water is accepted/idempotent; built moulds reject it. Revision conflicts retry the same lesson-bound intent. Local selection/highlights, switching/Leave cancellation, captured lesson and Scoop behavior are unchanged; `SoloSandpit` adds only build/tick/depth/reset hooks for watering presentation.

`SandWaterFeedback` observes confirmed shared dry→wet transitions, including siblings' changes. One transition starts a1.25-second local lift, tilted downward stream/drops into that mould's rim, and return to the ground shadow. The can's hit target stays fixed. Each client can display simultaneous pours for different moulds without networking animation frames or tool ownership. `SandShape.wet` always paints the latest authoritative state immediately: existing damp fill/blue drop plus a small water surface when wet before sand. It does not wait for, hide or override state during the pour. Idempotent Water, rejection, stale results and unchanged snapshots start no new transition effect. Changed lessons, inactive participation, menus/pause and re-entry/reconnect baselines clear or omit old animation. Positions derive from current world-to-board coordinates each frame, including camera/layout changes.

Exact changed files, relative to this checkout:

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandWater.cs` and `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandWater.cs.meta` — world can, local presentation, depth ordering and diagnostic properties.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandWaterFeedback.cs` and `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandWaterFeedback.cs.meta` — bounded authoritative wet-transition observer.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandShape.cs` — shared can drawing, lift/pour/return and visible empty wet result.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandpit.cs` — four narrow watering presentation hooks; existing commands/Scoop unchanged.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenVerification.cs` — inspection-only Water transition events, remaining effect times and rendered wet flags.
- `Tools/Sandpit.Tests/Program.cs`, `Tools/Sandpit.Tests/Sandpit.Tests.csproj`, `Tools/Sandpit.Tests/WaterChecks.cs` — actual Core/Water observer checks alongside M1/M2.
- `Tools/Test-DaycareSandpitWater.py` — focused four-client Water runtime and optional camera-only check.
- `docs/current-decisions.md`, `docs/family-playset-build-guide-2026-09-23.md`, this file — scoped record.

Checks actually run:

- `dotnet run --project Tools/Sandpit.Tests/Sandpit.Tests.csproj --configuration Release`: PASS existing M1 lesson/revision/duplicate/migration/retention and M2 Scoop feedback checks; new Water-before-sand, full dry/already-wet/built rules, same-mould conflict/idempotent retry, Scoop-Water-Scoop, four profiles, late join/reconnect/departure, transition-only feedback, expiry, re-entry/reset and stale rejection checks. The first new fixture failed because explicit Leave correctly retains its declined flag for the same visit; the fixture now returns through another world before further contributions. Gameplay rules were not changed.
- `Tools/Build-NetworkProbe.ps1 -BuildNumber 418`: fresh Unity6000.3.24f1 release Server/Client PASS, zero errors/warnings each. All465 game-code/meta source hashes match the build manifest. The generated bundle-version-only settings edit was restored; build artifacts remain ignored. Schema49/content67/protocol3 unchanged.
- `python Tools/Test-DaycareSandpitWater.py 418`: PASS seven groups. Actual mouse/InputSystem touch covers can targets/layouts, independent highlights, canceled/swiped pointers, pending Water switch/Leave, water before sand, correct pour/wet state, late arrival baseline, two-client same-mould Water/one transition, redundant repeats, layout change during active pour, direct Scoop-Water-Scoop, sibling New Lesson during Water approach, four different moulds, independent exit/local effect cleanup, native close/restart/reconnect, one child completing the shared castle and built rejection without false effects. Six processes including restarted client exit zero. Evidence: `LocalData/SharedGarden/1c213fd299a0423bbff07ae09b33ab34/sandpit-water/result.json`.
- `python Tools/Test-DaycareSandpitScoop.py 418`: PASS all seven unchanged M2 native groups on this build; six processes exit zero. Evidence: `LocalData/SharedGarden/2c1b728d82db42dc9a621bc4a4fffe91/sandpit-scoop/result.json`.
- `python Tools/Test-DaycareSandpitWater.py 418 --camera-only`: PASS a real inward touch pan while the pour remains active and wet graphics stay true, followed by effect expiry and the can returning to its floor shadow. Active-pour and returned-can screenshots inspected: `LocalData/SharedGarden/24f6357931bc49f892eb4605517533c4/sandpit-water-camera/`. Earlier camera fixtures failed by assuming synchronous server/client observation, one missed early entry touch, and an outward pan against the existing right-edge clamp. The final fixture waits for readiness/settled view and client effect, then pans inward. Those failed attempts are not counted as passes; the early synthetic touch miss was not isolated as a game defect, and entry-time touch comfort remains for device playtesting.

Native phone/tablet pictures show the grounded can, selected rim, downward water stream, empty wet surface and darkened filled sand. The layout-change capture still has0.50 seconds of the correct mould's pour active. No new gameplay exceptions/warnings were found; two inherited `ResetCreekBoats`/`OnDestroy` null references remain in each four-client run's shutdown logs. The optional managed reflection probe previously blocked by Windows policy was not retried; current Unity player builds and actual native cancellation checks passed.

Limits/deferred work: no physical-device/child usability or family art acceptance is claimed, and no device/live-server rollout, real-save change or enrollment operation occurred. Existing pre-response dry-tip inference, joining arbitration and limited classmate participation remain deferred; no new unrelated game defect was fixed. Tipping, crumble animation, tower reveal, flags/shells, NPC help, tray cleanup and free-build are not implemented here. The next separately authorized Milestone4 should derive tipping/crumble/tower feedback from authoritative outcomes, including resolving that existing dry-tip risk. Stop after watering and review first.

## Direct tipping — October 2

Milestone4 only, DAY-01 / LEARN-01 / FAMILY-01. Starting checkout: `C:\Users\sephi\Desktop\Little weeps game\LocalData\DinosaurWorld`, clean `main` at `071c54cb13b60cb7bb65b9ff3e76271808be99c1`. Milestones1 `42ba379`,2 `927b1e8`,3 `071c54c` are present; `origin/main` was verified at the same baseline before delivery. No unrelated work, other checkout, live server or devices were changed.

### Interaction and authority

Tap a different world bucket to select it locally; tap the highlighted bucket again to Tip. An orange curved arrow appears beside the selected unbuilt bucket, without adding a new text button. Tray mould buttons remain selection-only. The world bucket retains its140×155 scene-unit pointer area and existing drag/cancel handling; the local gold outline remains independent for each child.

Flow: `HomeHit`/`NavigationTap` → `TouchSandMould` → existing `RequestSandTool("tip")` → capture selected mould and lesson → automatic approach to `DaycareSandpit.Work` → `CheckSandpitInput` → `SendSandpit` with `mould@round` → existing solo authority or shared command queue/revision retry → unchanged `SandpitOperation`. Tray Tip enters the same Request/Send path. Scoop, Water, Flag, Shell and New Lesson controls remain.

Authority still returns `fill-bucket-first` for underfilled, clears a full dry mould to zero scoops, and marks a full wet mould built. Rules and messages are unchanged. Accepted dry/wet commands retain the existing generic `sandpit-played` receipt; presentation distinguishes the result from authoritative state transitions, not pre-command client guesses. `SandTipFeedback` prioritizes unbuilt→built as reveal; otherwise a same-lesson positive-scoop→zero transition in an unbuilt dry mould confirms a dry Tip, because no other current operation clears scoops within a lesson. This also handles the client observing one scoop before another child's final scoop and Tip arrive together. No new network field or persisted animation state is added. Schema49/content67/protocol3 stay unchanged.

Underfilled rejection starts a0.4-second local horizontal wiggle only, preserving the displayed sand level. It cannot replace an active confirmed collapse/reveal. Replies from an old lesson or after departure cannot start that feedback. The old `dry` boolean captured before submission is removed; crumble narration now follows the confirmed dry transition.

### Presentation and lifecycle

Accepted dry/wet transitions start independent1.9-second effects per mould. The bucket lifts and turns upside down. Dry sand briefly forms a pile, visible grains scatter, the pile spreads/flattens, and the empty bucket returns. A subsequent authoritative refill/watering supersedes that old collapse immediately. No penalty or new rule is added.

For wet sand, the inverted bucket settles over the existing build position, pauses, lifts and clears aside. The **existing authoritative `SandShape` tower** is revealed from its grounded base upward as the bucket lifts. No second temporary tower is created. Its original small/big geometry, positions and decoration drawing are retained; clipping returns to fully visible after the effect. An authoritative built tower remains when transient graphics expire.

Each effect is positioned through current `ToBoard` coordinates every frame and follows scene scale/depth ordering. Presentation never gates another mould's command. Round changes, departure, menu/pause and scenery teardown clear local effect time, bucket hiding/wiggle and reveal clipping. Late arrival/re-entry/reconnect establishes an end-state baseline and does not replay historical animations.

Two simultaneous Tips use existing serialization/revision rebase: after one dry reset the other is underfilled; after one wet build the other sees `tower-built`. Neither rejected/redundant nor duplicate receipts create a second success effect. Water/Scoop winning before Tip determine authority's actual result. Reset preserves the captured old lesson target in the request/retry path and rejects it without new-lesson mutation.

### Exact changed files

Paths below are relative to the implementation checkout:

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandpit.cs` — direct bucket wiring/arrow, removal of speculative narration, Tip lifecycle hooks.

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandShape.cs` — shared bucket drawing, rotation/lift, sand pile/grains, reveal clipping of permanent towers.

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandTipFeedback.cs` and `.meta` — confirmed transition observer, rejection wiggle and transient lifetime.

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandTip.cs` and `.meta` — four local effects, world input, grounding/depth and cleanup.

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenVerification.cs` — inspection-only Tip outcome/event/time and permanent-tower diagnostics.

- `Tools/Sandpit.Tests/TipChecks.cs`, `Program.cs`, `Sandpit.Tests.csproj` — actual authority/race/retention checks linked to production feedback code.

- `Tools/Test-DaycareSandpitTip.py` — scoped native four-client interaction/visual/lifecycle checks.

- `Tools/Test-DaycareSandpitScoop.py`, `Tools/Test-DaycareSandpitWater.py` — only selection setup changes to use selection-only tray buttons; a second world tap now intentionally means Tip.

- `docs/current-decisions.md`, `docs/family-playset-build-guide-2026-09-23.md`, this file — scope/evidence record.

### Verification and limits

- `dotnet run --project Tools/Sandpit.Tests --configuration Release`: PASS existing M1 lesson boundary/queue retry/duplicate/JSON/migration checks, M2 Scoop and M3 Water checks, plus new actual-authority Tip underfill, dry reset once, coalesced final Scoop/Tip, stale-dry Water/Tip revision race, duplicate/redundant Tip, four moulds, checkpoint restoration, late/reconnect baseline, independent departure, stale/reset protection and expiry checks.

- `Tools/Build-NetworkProbe.ps1 -BuildNumber 420`: Unity6000.3.24f1 release Server/Client PASS, zero errors/warnings each. All469 game-code/meta hashes match that build. Build419 also passed after the compile correction;420 additionally improves the dry bucket's path over the pile and preserves general start/leave rejection feedback. The bundle-version-only build edit is restored before delivery.

- `python Tools/Test-DaycareSandpitTip.py 420`: PASS seven native four-client groups, using real InputSystem touch: underfilled retention/wiggle, canceled/swiped bucket taps, pending switch/Leave/visit return, dry reset and repeat rejection, wet reveal/permanent tower/shell fallback, late fourth arrival, Water-before-Tip and final-Scoop-before-tray-Tip, four concurrent different Tips, departure during effects, native close/reconnect, two simultaneous same-mould Tips, active-reveal camera pan and final-tower New Lesson with old approach cancellation. The explicit native disconnect closes its client with a checked zero exit; the harness closes its owned instances afterward. Evidence: `LocalData/SharedGarden/30b49bd7bd9a41faaaf7191f9d1feed9/sandpit-tip/result.json`.

- `python Tools/Test-DaycareSandpitScoop.py 419`: PASS all seven M2 native groups. Evidence: `LocalData/SharedGarden/cee969f57f4f4aaf8b07ab7f992ac3f3/sandpit-scoop/result.json`.

- `python Tools/Test-DaycareSandpitWater.py 419`: PASS all seven M3 native groups. Evidence: `LocalData/SharedGarden/cc24a494d61048208a59ad016283862c/sandpit-water/result.json`. The later420/421 changes touch Tip graphics and general non-tool rejection feedback; Scoop/Water input, authority and feedback rules remain the tested source.

- Final421 compilation/visual evidence: `Tools/Build-NetworkProbe.ps1 -BuildNumber 421`: release Server/Client PASS, zero errors/warnings each, all469 code/meta hashes match. `python Tools/Test-DaycareSandpitTip.py 421 --visual-only`: PASS direct mouse Tip, two dry collapse/empty-return sequences at phone/tablet sizes and a wet lift/permanent tower check. Inspected final outlined pile/flattening and stable-tower pictures. Evidence: `LocalData/SharedGarden/82a62b1e8013438a825787df10b6f7dc/sandpit-tip-visual/result.json`. Only pile colour/outline/shadow differs from420's passed multiplayer source.

Native1280×591 phone and1024×768 tablet captures are inspected for the local arrow/outline, retained underfill, turning/inverted bucket, pile collapse, wet lift and permanent tower, shell decoration, camera pan, reconnect end state and cleared reset. Close-up inspection found that the pile initially blended into the floor;421 adds a lighter fill, narrow outline and floor shadow. This isolated visual change receives a focused native check rather than repeating multiplayer rules. Background/cached-scenery and safe-area checks use the existing capture assertions; no white backdrop or stuck rotation was observed. No new gameplay errors were found. Two inherited `ResetCreekBoats`/`OnDestroy` null references remain in the420 shutdown logs, alongside the existing audio timeSamples diagnostic; these are not hidden or claimed fixed. The previously policy-blocked optional managed reflection probe was not retried; actual native pending-intent checks passed.

The first419 compile exposed extracted bucket palette variables outside their scope; fixed before the successful build. Native fixture failures were an offscreen pointer target after teleporting far away, a disabled tray Tip on a completed tower, and asserting Leave before authority/client observation. Corrected fixtures use reachable targets, an unbuilt pending mould, and accepted-state waits. Failed attempts are not counted as passes.

Confirmed from code: New Lesson is available only once all four moulds are built. A current dry-collapse lesson therefore cannot be reset immediately; the same round-change cleanup used for wet reveal handles any later reset. The native check resets during the final tower reveal and cancels another child's old pending Tip. Lesson-bound request/retry/reset rejection is also covered by the focused Core suite. We did not claim an artificially forced unreachable dry-reset UI interaction.

These are transient effects derived from observed state, matching M2/M3: a complete fill-and-dry-reset occurring entirely between two snapshots can have no observable transition and its historical effect is skipped. Presentation never guesses or replays missing history; the latest authoritative end state remains visible. A dedicated network event history would be a separate shared-contract change if later required.

No physical-device touch comfort, child understanding, animation/art acceptance or device frame rate is claimed. Ages3/6 playtesting should check discovering the second tap/arrow, seeing the difference between the sand pile and stable tower, and whether the1.9-second sequence feels responsive. Existing joining arbitration, classmate participation and inherited Creek teardown errors remain deferred. No direct decorations, NPC redesign, tray cleanup or free-build additions. Stop after Milestone4; wait for the parent's review.

Git diff summary: 16 intended files; production changes stay in Sandcastle client presentation/input, with inspection diagnostics, focused test tools and three documentation records. No Core rules, network contract, save schema, assets, packages or permanent build settings change. No task branch was created.

## Direct flags and shells — October 2

Milestone5 only, DAY-01 / LEARN-01 / FAMILY-01. Starting checkout: `C:\Users\sephi\Desktop\Little weeps game\LocalData\DinosaurWorld`, clean `main` at `62c6fa9e5987d19419dbd89b4156d10d1a32c8aa`, also verified at `origin/main`. Milestones1–4 are present (`42ba379`, `927b1e8`, `071c54c`, `62c6fa9`). No unrelated work or other checkout was changed.

### Interaction, selection and command flow

A red flag on a pole at world `(4490,180)` and a pink ribbed shell at `(4670,180)` sit on floor shadows beside the shovel and watering can. They reuse the existing Sandcastle drawing rather than adding assets, packages, inventories or a new overlay. Each has a160×130 scene-unit `HomeHit` target: measured approximately118×96 pixels at1280×591 and154×125 at1024×768. Native checks confirm the targets do not overlap each other, the shovel or watering can. No drag is required.

Tap a built tower to select it locally, then tap the world Flag/Shell. Existing gold selection rings, tray mould selection and member-slot initial selection remain. A second tap on an already selected **built** tower now selects/cancels intent instead of unnecessarily requesting Tip; selected unbuilt buckets keep M4's second-tap Tip. One child can select/decorate any tower, and siblings keep independent selections. No shared selection field is added.

Flow: `HomeHit`/`NavigationTap` → existing `RequestSandTool("flag"/"shell")` → capture local tower and lesson → existing automatic approach to `DaycareSandpit.Work` → `CheckSandpitInput` → `SendSandpit` with `SoloAction.Sandpit` and `mould@round` → private solo authority or shared queue/revision retry → unchanged `SandpitOperation`. Retained tray Flag/Shell buttons enter the same Request/Send path. Switching selection cancels the old pending approach; the existing queue preserves its captured lesson during retries.

Confirmed from Core: only built towers accept decorations. Flag sets the single decoration integer to1, Shell to2; each replaces the other. Reapplying the same decoration remains accepted/idempotent. Unbuilt requests still return `try-sand-tools` without construction mutation; old-round requests return `old-sandpit-lesson`. The client duplicates none of those eligibility/replacement rules. All construction rules and Scoop/Water/Tip/Flag/Shell tray controls remain.

The current UI intentionally always starts with a valid local member-slot selection. A defensive out-of-range selection guard now returns harmlessly and wiggles a decoration source rather than constructing an arbitrary target. That defensive no-selection branch is confirmed from code, not claimed as a normal native UI scenario.

### Accepted placement, replacement and cleanup

`SandDecorationFeedback` observes confirmed decoration changes on built towers within the same lesson. A1-second local effect carries the current prop from its world source to the matching tower attachment point, then briefly sparkles. A change supersedes the previous effect for that tower, so a late/merged snapshot only presents its latest authoritative decoration. Redundant commands, rejected requests and duplicate receipts do not trigger a successful placement.

During travel the existing tower stores its actual authoritative decoration value but briefly hides its attached drawing. At76% of the effect, the moving prop stops drawing and the one permanent prop becomes visible. This avoids showing two permanent decorations or leaving an old Flag while a new Shell arrives. The existing small/big attachment positions and geometry remain: Flag's pole begins at tower height+10; Shell is centered at `(27,height+26)`. Frame-by-frame movement, hide timing, sparks and source wiggles remain local and unsaved.

An authority rejection on the current lesson wiggles the requested source for0.4 seconds, with no completed decoration or new success effect. This also applies to retained tray requests through the same callback. Old-lesson replies and replies after departure cannot start that wiggle.

Each of four towers has independent feedback. Existing serialization/revision retry decides same-tower conflicts; every client follows the latest resulting value. Native simultaneous Flag/Shell on tower1 ends in Shell (value2) in the recorded run, with all four clients converged. Replacing again during an active flight switches the current effect cleanly. There is no animation lock on another tower.

Camera coordinates and scale are recalculated every frame. Leaving, pausing/menu, scenery teardown or a new lesson clears transient feedback and restores attached final-state drawing. Re-entry/late join/reconnect establishes a baseline: saved flags/shells appear without replaying old movement. New Lesson clears authoritative decorations under the existing rule and cancels old pending approaches/effects.

### Exact files changed

Paths are relative to the implementation checkout:

- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandDecoration.cs` and `.meta` — grounded world objects, existing input binding, four flights, attachment handoff/depth and cleanup.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandDecorationFeedback.cs` and `.meta` — accepted-change-only feedback, latest-value replacement, rejection wiggles and reset/baseline lifetime.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SandShape.cs` — reusable Flag/Shell drawing, world-object shadows/scaling, placement flight/sparks and temporary attached-prop hiding.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandpit.cs` — lifecycle hooks, defensive local-selection guard and current-lesson rejection cue.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSandTip.cs` — already-built tower taps remain selection-only; unbuilt direct Tip unchanged.
- `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenVerification.cs` — inspection-only decoration values/events/time/wiggle/hiding diagnostics.
- `Tools/Sandpit.Tests/DecorationChecks.cs`, `Program.cs`, `Sandpit.Tests.csproj` — actual-authority decoration checks linked to production feedback alongside M1–4.
- `Tools/Test-DaycareSandpitDecoration.py` — bounded native four-client input, conflict, lifecycle and visual checks.
- `docs/current-decisions.md`, `docs/family-playset-build-guide-2026-09-23.md`, this file — scope and evidence.

### Checks actually run

- `dotnet run --project Tools/Sandpit.Tests --configuration Release`: PASS M1 lesson/revision/duplicate/migration/JSON retention, M2 Scoop, M3 Water and M4 Tip rules/feedback checks. New checks PASS built eligibility, both replacements, accepted redundant/duplicate commands without new feedback, same-tower revision conflict/rebase, four mixed slots, one child building all towers, JSON retention, late/reconnect baseline, independent Leave, reset/stale decoration and transient cleanup.
- `Tools/Build-NetworkProbe.ps1 -BuildNumber 423`: fresh Unity6000.3.24f1 release Server/Client PASS, zero errors/warnings each. All473 game-code/meta hashes match the manifest. Intermediate422 compiled successfully before additional inspection diagnostics; native checks use423. The builder's bundle-version-only edit is restored before delivery. No Core/save/message change: schema49/content67/protocol3 unchanged.
- `python Tools/Test-DaycareSandpitDecoration.py 423`: PASS all seven native groups on the first run. Actual InputSystem touch and mouse cover generous nonoverlapping targets, grounded props, unbuilt rejection/wiggle, canceled/swiped input, built world selection and sibling independence, Flag/Shell and both replacements, redundant repeats, small/big towers, tray fallbacks, pending world switch/Leave, visit return, late fourth, one child constructing all four, four concurrent mixed decorations, same-tower conflict, replacement during active flight, departure during placement, real native client close/restart, active-flight camera pan and New Lesson during approach/placement. The explicit disconnected client closes with a checked zero exit; the harness closes its own remaining instances. Evidence: `LocalData/SharedGarden/4a9963dd804943739e2ff736d7404b09/sandpit-decoration/result.json`.
- `python Tools/Test-DaycareSandpitTip.py 423`: PASS all seven unchanged M4 native groups, including underfill/wiggle, dry reset/collapse, wet reveal/permanent tower, both interleaved races, same-mould Tip, four builds, pending switch/Leave, late arrival, native reconnect, camera pan and reset. Evidence: `LocalData/SharedGarden/6e7bf79382a2468da720998d49312184/sandpit-tip/result.json`. Earlier M2/M3 native evidence is retained; their input/authority/feedback implementations are unchanged apart from the defensive valid-selection guard, and their focused production-linked tests pass here.

Inspected native phone/tablet captures show world Flag/Shell, correct floor placement, selected towers, attached Flag and Shell on small/big towers, replacements, four mixed decorations, moving prop/current replacement, stable authoritative winner, camera pan, late arrival, reconnect and reset cleanup. Existing capture checks enforce UI safe areas and bounded scenic cache. No white background, new stuck temporary prop or gameplay exception was observed. Logs contain three inherited `ResetCreekBoats`/`OnDestroy` null-reference occurrences in the decoration run, plus the existing audio timeSamples diagnostic; those unrelated teardown issues are not claimed fixed.

Remaining questions require physical-device/child playtesting: recognizing the world shell/flag, comfortable tapping, discovering tower selection, understanding replacement and whether the1-second placement feedback is clear for ages3/6. No device FPS, child/art acceptance or physical-device usability claim is made. No device install, live-server replacement, real-save/enrollment operation or shared compatibility change occurred.

Deferred: M4's entirely unobserved dry-reset transition can still skip its temporary collapse while shared state stays correct. It does not affect decoration and is deliberately untouched. Existing joining arbitration, limited classmate participation and Creek teardown remain deferred. No new unrelated game issue was fixed. Multiple decorations, drag editor, moats/boats, new towers, destruction/rebuilding, NPC/Calypso redesign, tray removal and other Daycare games remain out of scope. Stop after Milestone5 for the parent's review.

Git diff summary: 15 intended files, including six new files. Production changes are limited to Sandcastle client input/presentation and inspection diagnostics; the remainder is focused tests and three documentation records. No Core rules, network contract, save schema, assets, packages or permanent build settings change. No task branch was created.
