# Elephant gentle-care pilot — October 8, 2026

WORLD-02 / FAMILY-01 / ITEM-02. The owner approved feeding, picture navigation and personality/water build 485 / `00083a2`. This milestone adds elephant brushing only and stops for live owner review. No video is recorded, no other species gains care, and no device or installed family server is updated.

## Controls and appearance

Tap the illustrated brush basket to join optional care. The existing elephant scheduler walks to `(1200,320)` and presents three soft dust patches. Each large patch accepts either a tap or a short stroke; four distinct gestures clear a patch progressively. A held finger or additional drag frames never earn more credits. One child can finish; up to four helpers have their own visible brushes and contribute to the same patches.

The brush snaps to the touched patch. Following the owner's live correction, it sweeps side to side twice over 0.65 seconds, with 20 art units of movement. Local cosmetic motion responds immediately; shared brush motion and dust use accepted authority hints. There is one network command per distinct gesture, not per brush movement. There are no points, reward popups, health/happiness/cleanliness meters, medical treatment or accumulated dirt.

The basket sits at `(1740,180)`, beside the existing right-hand feeding station with separate hit areas. The approved pool and separate pump remain on the left; low trays, portraits and navigation retain their positions. The pictured basket/down-arrow puts the brush away. A small clock picture acknowledges waiting. Ordinary elephant calls remain available outside care; map/menu/pause consume or suppress care input.

The three Zoo route arrows previously changed disabled tint while brush commands awaited acknowledgment. Brush transactions now retain their ordinary tint, while the existing transaction/navigation guards remain. Feeding and genuine navigation retain their busy indication.

New art is editable native UI geometry with [SVG source](../../../SourceArt/Zoo/Playable/elephant-care.svg). No existing animal atlas, scenery, GUID or prefab was replaced. No new audio is added: the inspected clinic brush clip belongs to its spoken-tool presentation, and this pilot does not transplant the clinic's voice/medical system. Brushing uses a subtle visual response. Subjective audio approval is not claimed.

## Authority, scheduling and lifecycle

The existing `ZooPhase` scheduler alone controls elephant travel/poses. CareWalk, Care and CareFinish extend it. No second movement controller is introduced. The appreciative ear/trunk movement is separate from feeding consumption; it never increments `fed`.

| Contract | Rule |
|---|---|
| Shared session | One authority-owned session ID, up to four participant IDs, three integer patch progresses clamped 0–4, accepted brush ages/locations, completion and cancellation. |
| Input validation | Exact elephant exhibit, membership, session, participant-generation token, patch and monotonic gesture number; 0.35-second per-child rate limit. Existing command receipts also deduplicate retransmissions. A renewed participant token allows a restarted client to start numbering again without accepting old membership messages. |
| Requests | Repeated selection while joined is idempotent. One care session and the existing one pending water request remain bounded. New non-feeding requests receive authority FIFO order; an older suspended care session resumes before a later water request. Care cannot overlap water. |
| Food priority | Offered food takes over Care/CareFinish at the next short safe transition: at most 0.35 seconds plus one authority tick (native cap 0.1 seconds), excluding transport/snapshot latency. CareWalk yields immediately. Existing Splash limit remains 0.6 seconds plus one tick. |
| Eating finish | The approved 4-second Eat segment remains intact. Care/water do not interrupt it. Patch progress persists across food suspension for remaining helpers. A completion reaction starts once; preempting its short finish does not replay it later. |
| Return/replay | Completed patches persist while tools are still selected; another basket tap deliberately starts a fresh session after the finish. Selecting during an active session never resets patches. |
| Stale intent | Care expires after 90 seconds without a new accepted gesture or a new helper joining. This includes waiting/suspension and prevents stale indefinite requests; it adds no dirt/penalty or repeated reminder. Water retains its 30-second pending expiry. |
| Individual exit | Put-away, exhibit travel and disconnect remove only that helper. Remaining helpers retain session/progress. |
| Last exit | Clear transient patches/tools/completion and return Care phases to an ordinary routine. Last disconnect performs this synchronously because an empty FamilySession stops ticking. The existing no-visitors policy also clears pending water/play. |
| Reconnect/late join | Render current accepted patches; seed completion observation so old reactions do not replay. Tools need a new deliberate selection after an actual participant departure. |
| Save reopening | Additive hints only; schema 53/protocol 3 remain. Restore clears care session/membership/progress/effects and pending water, preserving feeding counts and unrelated saved data. Missing pre-care hints receive valid defaults. |

**Content 74** records the new shared care rules. These candidates need a separately authorized coordinated rollout to join an older installed authority. Admission checks remain intact. Local tests/review use disposable authenticated loopback families; live enrollment, endpoint, server process and saves remain untouched.

## Changed paths

Under `Unity/FamilyPlayset/Assets/FamilyPlayset/Code`:

- `Core/Worlds/Zoo/ZooWorld.cs`: additive transient state, tokens/rates, single scheduler, suspension/replay/restore/disconnect policy.
- `Core/Shared/State/GameWorld.cs`: immediate care release on travel.
- `Core/Shared/Layout/WorldLayout.cs`: content74; schema/protocol unchanged.
- `Client/Worlds/Zoo/GameScreen.ElephantCare.cs` and metadata: basket, patches, brushes, existing `VetTouchSurface` input reuse, accepted-state presentation and event observation.
- `Client/Worlds/Zoo/GameScreen.ElephantPlay.cs`: compatible once-only appreciative ear/trunk articulation.
- `Client/Worlds/Zoo/GameScreen.Zoo.cs`: care build/render/reset; locomotion uses the existing walking frames.
- `Client/Worlds/Zoo/GameScreen.ZooNavigation.cs`: steady arrow tint for brush acknowledgments.
- `Editor/ZooJsonTests.cs`: shared/solo/rate/duplicate/food/water/legacy/replay/departure tests.
- `Networking/FamilyGameVerification.cs`: care reaction, brush-position and actual route-tint observations.

Also: editable SVG/Playable README; `Tools/Verification/Test-ElephantCare.py`; configured `Tools/Launch/Review-ElephantCare.py`; optional visible review controls in the existing authenticated `Tools/shared_garden_runtime.py`; build-assigned ProjectSettings; maintained work/decision/audit records. Clinic code/assets remain unchanged.

## Validation and live review

Standard Windows 489 builds both client/server with zero errors and warnings. All 2,378 Unity inputs match its source manifest. Unity care/JSON gates pass legacy defaults, four simultaneous clamped contributions, rates/duplicate messages, restarted participant tokens, solo completion, priority/suspension, deliberate replay, restore and immediate last disconnect. Existing personality/water and Zoo feeding/migration gates also pass.

Four actual native 489 clients pass solo start/finish/put-away/replay, taps and short strokes, held-input protection, two/four helpers, simultaneous contributions, invalid/stale/repeated requests, map blocking, pause/reconnect/late join, fresh-client brushing after rejoin, independent/all-participant travel, food before/during/finishing care, water ordering, layout checks and unchanged giraffe feeding. A supplementary four-client run passes rapid tool selection, one native stroke credit at 30 fps and 60 fps, ordinary elephant call and all four actual client disconnects. One private native client passes real saved-world process reopening with transient tools/effects cleared. Logs contain no runtime errors.

Native brush samples travel horizontally from −93.69 to −132.15 to −99.20 art units during one gesture; all three actual route CanvasRenderer tints remain white in those samples. Full-view phone 1280×591, tablet 1024×768 and small phone 640×400 captures are inspected. Brush selection and put-away targets remain at least 44×44; the small-phone targets are 58×63 and 54×46 pixels. The tablet viewport partly clips the far-left pump and the far-right basket rim; ordinary walking/panning reveals world props, as in the approved pilot. Usable care targets remain visible and separate from feeding/navigation.

The authority test bounds care-to-food takeover at 0.35 seconds plus a 0.1 second tick. Native food had already acquired its lease when the fixture offer reply arrived, so subsequent polling measured 0 seconds. This is not a claim of zero transport or end-to-end delay.

- [Current Unity source check](../../../LocalData/ElephantCare/source-check.json).
- [Four-client489 results](../../../LocalData/SharedGarden/5dbc7d78610c48aaab717b35761ee309/elephant-care/results.json).
- [Actual brush movement and arrow-tint samples](../../../LocalData/SharedGarden/5dbc7d78610c48aaab717b35761ee309/elephant-care/brush-motion-and-route-tints.json).
- [Native30fps/60fps, all-disconnect and giraffe regression](../../../LocalData/SharedGarden/2d8078f2a9d745cdb32ca445d17838eb/elephant-care-regression/results.json).
- [Private489 saved-world reopening](../../../LocalData/FamilyLAN/24b6c3612fa4495aa4b545efeb8d9c45/elephant-care/results.json).
- [Phone](../../../LocalData/SharedGarden/5dbc7d78610c48aaab717b35761ee309/elephant-care/phone-care.png), [tablet](../../../LocalData/SharedGarden/5dbc7d78610c48aaab717b35761ee309/elephant-care/tablet-care.png), [small phone](../../../LocalData/SharedGarden/5dbc7d78610c48aaab717b35761ee309/elephant-care/small-phone-care.png), [shared helpers](../../../LocalData/SharedGarden/5dbc7d78610c48aaab717b35761ee309/elephant-care/four-helper-patch.png).

Earlier 486/488 harness runs exposed outstanding-selection and viewport-fixture timing; their failures remain retained. 488 also prompted the reconnect token correction. They are not final acceptance evidence. 489 uses settled visible fixture positions for unchanged-giraffe regression and passes. Fixture positioning is separate from actual touch activation. Runtime-generated captures and synthetic saves stay ignored; unrelated pre-existing untracked audits, root Packages/ProjectSettings and unfinished worktrees remain preserved.

Technical checks, live owner acceptance and deployment remain separate. Physical phone/tablet testing is not claimed; Windows native device-sized views are simulated layouts. The previously Windows-Application-Control-blocked standalone .NET Zoo executable is not retried, bypassed or counted as passing. No security settings are changed.

Live review sequence completed on the configured isolated489 preview: native arrival greeting, start brushing/clear a patch, a second Bingo helper, real touch-to-offer food during care, resumed brushing/completion and put-away. Two connected players remain at the elephant exhibit; tools are released and the authority is in an ordinary routine. The controller preserves this disposable world and shuts down its isolated authority after both review windows close. [Live ready receipt](../../../LocalData/SharedGarden/4b3e0d92a3c248c48140a2700f9796f0/live-review-ready.json), [read-back](../../../LocalData/ElephantCare/live-ready-check.json), [final native view](../../../LocalData/ElephantCare/live-owner-ready.png). No recording was made. Owner visual/motion acceptance remains open.

The first review launcher rejected Bingo's display ID before brushing; the corrected launcher uses the verified saved character ID and completes the sequence. The failed disposable preview is preserved separately and is not acceptance evidence.

Stopped for owner review of this elephant-only pilot. No automatic expansion.
