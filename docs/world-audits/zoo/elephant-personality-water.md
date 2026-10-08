# Elephant personality and water pilot — October 8, 2026

WORLD-02 / FAMILY-01 / ITEM-02. Elephant only. The owner approved feeding 475 / 955127e and navigation 481 / e7c0672 before this task. Their source, trays, portraits, handoff, finish, queue and local map are preserved. Stop for owner review of this pilot; no other species or care activity is expanded.

## Scheduling and controls

The existing `AdvanceZoo` authority owns every animal segment, its position, sequence and age. New elephant-only phases are Greeting, Curious, WaterWalk, Splash and WaterReturn. There is no second position writer, shared round, score or compulsory participation.

| Rule | Behavior |
| --- | --- |
| Arrival | A connected visitor entering the elephant panel can start a 2.2-second trunk/ear greeting when the animal is free. One exhibit cooldown of30 seconds prevents restarts. Busy arrivals do not queue a stale greeting. |
| Curious idle | A 3.2-second trunk/leaf investigation, initially after 14 seconds and then with20–38 seconds between eligibility. Feeding interrupts it immediately. |
| Water tap | One real touch submits the existing Zoo command with `water/elephant`. Authority accepts one shared pulse at most every 1.2 seconds; an individual child waits 2.4 seconds, leaving a turn for siblings. Client debounce also limits repeated submissions. No permanent toy owner. |
| Immediate result | Accepted sequence/age produces a 1.1-second spout pulse and floating-leaf/ripple response, even during feeding. No optimistic duplicate effect or accumulated effect objects. |
| Elephant participation | At most one pending request, expiring after30 seconds. WaterWalk and 1.8-second Splash start only without an offered food queue. Taps during approach/splash can pulse the prop but do not create another elephant routine. |
| Food priority | Greeting, curiosity, WaterWalk and WaterReturn yield on the next authority tick. Splash yields at age 0.6 seconds. Maximum extra play delay is 0.6 seconds plus the authority tick (native tick capped at 0.1 seconds); transport/snapshot latency is separate. Notice/Approach/Eat then use the unchanged feeding timings and FIFO tickets. |
| Eating finish | A consumed portion retains its entire approved 4-second Eat segment. Pending water cannot interrupt that finish. |
| Return/interruption | Elephant walks at 80 world units/second to the play point and returns to its prior point; the actual 1.8-second splash stays short. Food can interrupt either walk. Leaving cancels only that actor's food. With no connected visitors remaining in the elephant panel, pending play clears and an active personality/play segment returns to an ordinary routine. |

Normal world scrolling remains: children can walk or pan to a prop that is outside their current viewport. The pump is a world object, not a fixed overlay. All four native players operate the same pump from its visible neighborhood.

## Presentation and preservation

The owner's interim capture rejected the button mounted on the pool's front. Final 485 moves the toy into the left habitat patch: pool at `(820,320)`, separate pump at pool offset`(-130,166)`, elephant play position`(1110,320)`. Food stays at the original right-hand bucket and four offering spots. Water and floating leaf remain visible; the elephant faces left only for its splash. Touch targets remain generous in the tested phone/tablet/small-phone views.

`ElephantArtView` articulates the original resting atlas's trunk and ear with a small 24×24 UI mesh. It moves no body/foot anchor and adds no food to idle/play poses. Ordinary and approved feeding poses use the original quad whenever articulation is zero. The splash origin is sampled from the same articulated/mirrored trunk geometry. Native full-view captures check reach, layering and separation; owner appearance/motion approval remains open.

The pool/pump/leaf are editable native UI geometry with [SVG source](../../../SourceArt/Zoo/Playable/elephant-water.svg), following the existing Zoo navigation-art workflow. Original atlas pixels, import metadata and GUIDs remain intact; no new raster generation, rendering package, fluid simulation, download or dependency. Twelve retained droplet objects serve every pulse/splash. Animation reads authority ages; local timestamps only interpolate received state. No per-frame effect creation, closure creation or new event subscriptions.

Existing Zoo water audio is reused quietly, with a 1.2-second local sound gate and existing ducking/mute/test-mute gain. Existing animal-call and feeding audio remain. There is no general Zoo reduced-motion/effects-volume preference in the current app; the science-specific calm/effects preferences remain scoped to science. This pilot keeps motion small and adds no microphone/speech service. The supplied recordings are **silent**; subjective listening remains open.

## Snapshot/save compatibility

Schema 53 and protocol 3 remain; **content 73** records new authoritative phases/rules. Installed server 463/content 72 remains untouched. This candidate requires an explicitly authorized coordinated compatible rollout; no admission bypass, device installation or server replacement occurred.

Optional additive Zoo hints are `waterSequence`, `waterAge`, `waterCooldown`, `waterPendingSeconds`, `waterPending`, `greetingCooldown`, and `curiousCooldown`. Old JSON without them has valid defaults. Restore clears water identity/effects/pending/cooldown, clears unfinished food under the existing rule, returns active new phases to ordinary routines, and starts fresh greeting/idle cooldowns. Feeding history, players, world identity, enrollment and unrelated saved objects persist. Visitor membership, per-child rate-limit clocks and return-point bookkeeping are authority-local and never persisted. Clients seed event observation on initial sight, reconnect and application lifecycle; repeated snapshots never restart effects or sound.

## Changed files

Under `Unity/FamilyPlayset/Assets/FamilyPlayset`:

- `Code/Core/Worlds/Zoo/ZooWorld.cs`: scheduler, priorities, fair pulse acceptance, transient hints, validation and restore cleanup.
- `Code/Core/Shared/Layout/WorldLayout.cs`: content73; schema/protocol unchanged.
- `Code/Client/Worlds/Zoo/ElephantArtView.cs` and metadata: resting-art trunk/ear articulation and exact mirrored tip.
- `Code/Client/Worlds/Zoo/GameScreen.ElephantPlay.cs` and metadata: separate illustrated pool/pump, fixed effect pool, event identity and presentation.
- `Code/Client/Worlds/Zoo/GameScreen.Zoo.cs`: connect play/art to the existing renderer and reset observation; approved feeding stays intact.
- `Code/Client/Worlds/Zoo/GameScreen.ZooAudio.cs`: retained water clip and restrained sound/mute handling.
- `Code/Editor/ZooJsonTests.cs`: authority/JSON tests for greeting, fairness, safe feeding takeover, eating finish, pending play and clearing on departure/restore.
- `Code/Networking/FamilyGameVerification.cs`: water-event/effect-pool and native memory evidence.

Also: `SourceArt/Zoo/Playable/elephant-water.svg`, playable-art README; `Tools/Verification/Test-ElephantPlay.py`; optional pilot checks in `Test-ElephantFeedingSolo.py`; build-assigned `Unity/FamilyPlayset/ProjectSettings/ProjectSettings.asset`; this audit and maintained work/decision links. `docs/plan-scope.json` registers the existing feeding baseline and new pilot as local review records. No scene/prefab YAML was hand-edited.

## Validation and review evidence

Final standard Windows 485 builds server/client with zero errors/warnings. Standard Unity gates include new elephant authority/JSON tests and existing Zoo migration/four-consumption/retention checks. All 2,376 Unity inputs match its source manifest. The previously Application-Control-blocked standalone .NET Zoo executable was not retried, bypassed or reported as passing.

The plan-consistency check initially found the pre-existing feeding report absent from the document registry. Registering that baseline and this review record fixes the classification; the final check passes, with its generated result retained under ignored `LocalData/ElephantPlay` rather than replacing a dated audit's evidence.

Private native 485: arrival greeting, idle, three real-touch water cycles and return, automatic feeding/once-only finish, pause/resume, exhibit switch/return and actual saved-world process reopening pass. Pending water, old pulses and unfinished food clear on reopening; feeding history remains. No runtime error lines.

Shared native 485: one disposable loopback authority and four actual release players pass greeting/cooldown/idle; immediate accepted water response; exactly one pulse event per observing client; feeding before/during/after play with pending requests deferred until the eating finish; four real-touch food offers while several children pump; rapid/near-simultaneous input and all four independent pump operators; local map blocking/continued shared play; real disconnect/reconnect/late join without old pulses; pause/resume; active feeder departure preserving siblings; all-visitor departure clearing play; normal return; and existing elephant-call interaction plus giraffe feeding. Final logs contain no runtime error lines. Phone 1280×591, tablet 1024×768 and small-phone 640×400 pass safe-area, at least 44×44 touch target and nonoverlap checks against feeding/navigation/menu controls. Full-view captures and native animation frames were inspected.

The Unity test forces a food offer at early Splash and verifies takeover within 0.7 seconds (0.6 safe transition plus 0.1 tick). In native play the animal had already taken the food lease when the fixture offer reply arrived, leaving zero further polling delay; that is not a claim of zero network or end-to-end added delay.

Ten repeated native water cycles retain 12 droplets and 16 total AudioSources. Managed heap is 42,729,472 →42,721,280 bytes; Unity total allocated memory is 115,124,262 →115,256,221 bytes. These bounded native heap samples include existing world/network/verification activity and are not a per-frame profiler attribution or a claim of zero Unity allocations.

Physical phone/tablet testing, installed-family acceptance, subjective listening and owner visual/motion acceptance remain open; native Windows device-sized views are **simulated device layouts**, not physical-device captures.

- [Before/after gallery and gameplay recording](../../../LocalData/ElephantPlay/review.html).
- [Current-source check](../../../LocalData/ElephantPlay/source-check.json).
- [Private485 results](../../../LocalData/FamilyLAN/dff3aaa91d2d4a87a8d34f0ddb3d024c/elephant-solo/results.json).
- [Shared485 results](../../../LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603/elephant-play/results.json), [allocation samples](../../../LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603/elephant-play/allocation-samples.json).
- [Phone pool/pump](../../../LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603/elephant-play/phone-pulse.png), [tablet splash](../../../LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603/elephant-play/tablet-splash.png), [small-phone layout](../../../LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603/elephant-play/small-phone-layout.png), [four players](../../../LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603/elephant-play/phone-four-feeders.png).
- [Actual silent gameplay recording](../../../LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603/elephant-play/elephant-play-silent.mp4): native real-time frame timestamps, H.264 padded by one pixel to 1280×592. Arrival/greeting, idle investigation, accepted child pulse and splash, feeding priority and multiple players appear. Fixture positioning is used between some actions and is separate from actual touch activation. The native capture path records frames only; no audio-enabled recording or subjective listening pass is claimed.

Original baseline 481 phone/tablet captures remain in `LocalData/ElephantPlay/before`; final shared run is `LocalData/SharedGarden/c94b40eb4b494a8b80bd48adf3b33603`, private run `LocalData/FamilyLAN/dff3aaa91d2d4a87a8d34f0ddb3d024c`. Retain their synthetic saves, raw recording frames and logs as review evidence. Generated media stays ignored; unrelated pre-existing untracked audits, rootPackages/ProjectSettings and unfinished worktrees remain untouched.

Earlier482 caught offscreen tablet controls and a reused pose containing food;483 added articulation but retained crowded placement;484 passed native shared/private checks before the owner rejected its front-mounted button. The final relocation supersedes those pictures. Failed shared wrappers also exposed a snapshot race and an attempt to tap a world prop while outside the far-right tablet viewport; the harness now waits for accepted identity and sets up each child's visible toy neighborhood. These runs stay retained and are not final acceptance evidence.

Next: owner review of this elephant-only pilot. No automatic expansion or deployment.
