# Making Bluey and Bingo look relaxed while walking

September 26, 2026 · **WALK-RESEARCH-01** · CHAR-01, WORLD-01/02; G6 character presentation. Research and implementation specification, **not an implemented animation fix**. [Current plan](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis).

## The user's clarification sets the scope

The problem is **how the character looks while walking**: “super up tight not fluid.” Prioritize the character performance: bent limbs, convincing steps, weight changes, relaxed arms and follow-through. The current joystick placement was accepted. Changing controls, walking speed, server tick rate or camera damping is not the requested solution.

Recommendation: author a compact, pose-driven 2D walk for the existing recognizable Bluey/Bingo artwork, with feet that support the body and subtle overlapping motion. Keep gameplay position under the existing controller. Start with a visual comparison inside the actual home at the same walking speed. More exaggerated bobbing alone would make the same rigid puppet bounce up and down.

## What is established and what is still unknown

| Evidence | What it establishes | Limit |
| --- | --- | --- |
| User's eight Bluey: Let's Play! screenshots and explicit feedback | Required appearance, character proportions, illustrated playset and an objection to rigid walking | Still images cannot establish gait timing, stride lengths or Budge's internal rig/physics |
| Official Bluey app announcement | The mobile reference is Budge's open-ended house/backyard play app | It does not document the engine's animation/controller implementation |
| Current C# and art adapter, committed at e912eff plus build 115 version settings | Exact current gait, rig hierarchy and presentation behavior below | Code inspection identifies likely causes; it does not quantify perceptual improvement |
| Animator-authored tutorials and Unity documentation | Established ways to construct and blend walks, pose limbs and manage presentation | Their sample timings and human proportions are not Bluey's specifications |
| Existing movement/recovery evidence | Earlier network interpolation was tested separately | Those tests do not prove that the character's walk looks natural |

The [official app description](https://www.bluey.tv/blog/bluey-lets-play-mobile-app-is-available-now/) supports the playset context. No proprietary animation files were extracted; no new hands-on or frame-by-frame analysis of Budge gameplay is claimed. Exact commercial cadence and animation technology remain unverified. Use the supplied images and existing [character reference catalog](../bluey-research/character-references.json) to preserve appearance while we author our own performance.

## Why our current walk looks stiff

Paths below are relative to `Unity/FamilyPlayset/Assets/FamilyPlayset/Code/`. Findings apply to the source used for Android 115; the family server is still 110.

| Confirmed implementation | Likely visible effect | Intended correction |
| --- | --- | --- |
| `Client/GameCharacterVisual.cs:33–61`: separate UGUI Images; one far/near arm and one far/near foot/leg joint, with no elbow or knee chain | Whole limbs swing like rigid cutouts; no relaxed bend during the passing step | Prepare bent-leg/arm drawings or segmented limbs with covered joints; author actual contact and passing shapes |
| `Client/CharacterView.cs:37–49`: fixed sinusoidal limb rotations, at most 19° leg swing; body and head receive exactly the same small vertical offset | Limited weight transfer; upper body appears braced while limbs tick back and forth | Pose the supporting side, body lean and arm arcs as a coordinated action |
| `Client/CharacterView.cs:41`: instant horizontal mirror | Direction reversals can look like a cardboard cutout flipping | Short visual turn/settle, preserving character markings and the gameplay root |
| `Client/CharacterMotion.cs:30–39`: raw displayed displacement drives speed and facing; >0.01 horizontal displacement changes facing | Small corrections can change the pose or orientation near a stop | Presentation hysteresis and explicit discontinuity handling; no new controller delay |
| `Client/CharacterView.cs:37–39`: cadence and swing amplitude both shrink with speed | At a half-speed input the cycle is slower and the swing is smaller, without a corresponding stance/contact model | Use a measured stride and a defined slow-walk treatment; calibrate foot travel to root travel |
| `Client/CharacterView.cs:43–59`: no stance foot lock or lifted-foot trajectory | Feet may skate, rotate through the floor or fail to sell weight | Grounded stance interval plus a clear lifted return arc |
| Per-pose transforms are assigned directly; walking becomes zero swing on stopping | Sudden arm/leg reset and a rigid final stance | Brief pose blend and a supported final step; preserve responsive stopping |
| Head stays unrotated to protect the continuous box silhouette | This guard prevents a disconnected face, but whole-body performance is very limited | Preserve that guard until artwork supports a connected deformation; do not loosen the head independently |

At full speed the current cycle is 1.35 cycles/second and gameplay speed is 210 floor units/second: **about 155.6 floor units per full cycle**. This is arithmetic from our code, not a measurement of the commercial game. The authored stance travel must match the character's actual scale and this translation, or cadence must be recalibrated. Simply increasing joint angles does not establish contact.

## Research findings that matter for this walk

**Build the walk from meaningful poses.** Jason Martinsen demonstrates contact, weight absorption, passing and rise poses, then cleans foot sliding and limb curves before adding detail. That offers a better starting point than independent sine waves. His human rig is a teaching example; our broad torso and short legs need their own drawings. [Animator's walk construction tutorial](https://www.animationmentor.com/blog/tutorial-animating-human-walk-cycle/).

**Different parts should not start and stop together.** Drew Adams identifies synchronized torso/head movement as a cause of stiffness and demonstrates how trailing motion adds fluidity. For Bluey/Bingo, apply restrained overlap to arms, wrists, tail and suitable ear art. Keep the facial block visually connected; do not transplant a human spine's twisting range. [Animator's overlap and follow-through explanation](https://www.animationmentor.com/blog/follow-through-and-overlapping-action-the-12-basic-principles-of-animation/).

**Bingo needs her own tuning.** Nathaniel Seymour's large/small character demonstration treats proportions and timing as part of performance, with motion progressing through a limb instead of moving it as one piece. Our proposal is a shorter, slightly quicker step for Bingo where her size calls for it, calibrated to the same allowed travel speed. Shared rig conventions need not mean identical animation curves. [Animator's size and movement study](https://www.animationmentor.com/blog/tutorial-animating-big-and-small-characters/).

**Blend at corresponding foot phases.** Unity documents that walk/run contacts should line up in normalized time when clips blend. Preserve the supporting foot while changing pace or pose; a blend between arbitrary phases can still slide. [Unity blend trees](https://docs.unity3d.com/6000.3/Documentation/Manual/class-BlendTree.html). Unity also supports timed and interruptible state transitions; the existing code-driven view can adopt the same explicit transition contract without requiring an Animator migration first. [Unity transitions](https://docs.unity3d.com/6000.3/Documentation/Manual/class-Transition.html).

## Proposed walk performance sheet

This is our first art/animation proposal, not a claim about Budge's exact poses. Author eight principal poses plus a repeated endpoint, then smooth the curves and inspect intermediate silhouettes. Eight key poses do **not** mean the game should render at eight frames per second.

| Part of the step | Feet and legs | Body and arms |
| --- | --- | --- |
| Contact | Leading foot arrives; trailing foot finishes support; knees are soft | Weight begins moving toward the new supporting side; opposite arm leads gently |
| Absorb | New supporting knee bends modestly; rear heel/foot prepares to lift | Body settles a little rather than remaining bolted upright |
| Pass | Free knee bends so the foot clears the ground and passes the supporting leg | Body travels over support; elbows remain relaxed |
| Rise | Supporting leg lengthens without a locked knee; free foot reaches forward | Body rises slightly; wrists and tail catch up after the main motion |
| Opposite step | Repeat with the other supporting side | Maintain continuity across the loop, avoiding a synchronized snap |

Keep the feet broad and readable at phone size. A stance foot holds its ground contact while the gameplay root moves above it; the swing foot travels on a shallow arc. Do not force both feet to remain planted while the body translates. Store contact in the local world/floor space and project it consistently with the existing `y * 0.45` depth convention. Do not estimate gait from screen movement after camera panning.

Use a small side-to-side weight shift, a relaxed arm arc and a mild forward attitude. Preserve the long rectangular torso, muzzle placement and clean outline. Tail overlap should follow the step rhythm with a delay; an unrelated constant wiggle adds activity without explaining weight. Ear motion is optional and tiny, requiring separate ear art or a safe deformation zone first. Keep blinking independent; continuous head bob or random wobble is not required.

**Initial tuning candidates, subject to visual review:** supporting-knee bend 8–15°, passing-knee bend 20–35°, torso sway around 1–2°, vertical travel around 1–2% of character height, arm swing around 8–15°, visual start/stop blend 0.10–0.18 seconds. These values are proposals, not tested settings or source recommendations. Test silhouettes before tuning angles; cartoon replacement drawings may work better than literal joint rotation. Keep full-speed travel at the current 210 floor units/second for the first comparison so animation quality is the variable under review.

### Starting, stopping, turning and carrying

Start from the nearer foot's supported pose rather than restarting every walk at an arbitrary clock position. On release, stop the gameplay root as currently specified and settle the visual into a nearby supported idle; do not add a long skating stop. A reversal can reorient the silhouette through a brief authored turn while the controller remains responsive. Very short taps still need a small readable step.

Keep the same stride phase when feasible across idle/walk/carry transitions. Carrying changes the holding arm and body balance while the legs continue walking. The actual held prop remains owned by the existing item system; any hand alignment is presentation only. A visual pose must never duplicate a bucket or change its contents. Reset stance contacts and blend history on travel, teleport, rejoin or character replacement; never drag an old planted foot across the map.

## Rig and implementation choice

| Option | Strength | Cost / fit | Decision |
| --- | --- | --- | --- |
| Keep rotating current whole limbs | Smallest edit | Cannot produce genuine knee/elbow shapes; more bob does not solve the underlying silhouette | Insufficient as the finished correction |
| Pose drawings plus layered joint curves in the current view | Works with the game's UGUI rendering; lets us protect Bluey/Bingo outlines | Needs new bend/contact drawings and a small authored-pose data format | **First bounded implementation** |
| SpriteRenderer + weighted SpriteSkin + 2D IK | Continuous deformation and useful foot/hand targets | Requires renderer/depth/masking integration and mobile profiling; not a component that can be added directly to the existing Images | Separate spike only if the layered result cannot meet acceptance |
| Fully drawn sprite-sheet walk | Strong silhouette control | More drawings for views, carry variants and character sizes; hand anchors need per-frame data | Useful alternative for problem poses, not mandatory for the whole cast now |
| Dynamic ragdoll / physics-driven body | Emergent collisions and motion | Poor match for predictable, readable dollhouse walking | Not proposed for the normal walk |

The project currently installs Unity 6000.3.24f1, UGUI 2.0.0 and 2D Sprite 1.0.0; **2D Animation is not installed in the manifest**. Unity's SpriteSkin operates on a SpriteRenderer and weighted bones. Its 2D IK includes a two-bone limb solver, but adding an IK package cannot bend an unprepared whole-leg image. [SpriteSkin requirements](https://docs.unity3d.com/Packages/com.unity.2d.animation@13.0/manual/SpriteSkin.html), [2D IK](https://docs.unity3d.com/Packages/com.unity.2d.animation@13.0/manual/2DIK.html). Version 13 documentation is a researched candidate, not an installed or qualified dependency. No paid package is needed for the first pass.

Prepare editable layered art with joint overlap coverage and alternate bent silhouettes, shared pivot conventions, separate face layers, and stable hand/ground anchors. Keep the current approved prototype likeness. Blender is available for pose/blockout assistance, but this correction does not require turning the cast into 3D models or replacing the renderer before showing an improvement.

## The first bounded implementation: WALK-01

1. Record the unchanged walk in the actual home at fixed speed: Bluey/Bingo, left/right, slow/full movement, starts/stops, reversals and carrying. Capture the feet and the whole silhouette, with a fixed-camera comparison first.
2. Make a pose sheet using the existing character art, with enough bend and overlapping coverage for the proposed gait. Author the main contact/absorb/pass/rise shapes before adding tail/ear detail.
3. Add a read-only walk-pose sampler behind `CharacterView`; keep `GameCharacterVisual`, avatar IDs, root position, UI controls and save/authority contracts intact. Both characters share pose semantics but can have separate curves and stride calibration.
4. Tie gait phase to ordinary locomotion distance with explicit resets/exclusion for discontinuities. Suppress tiny facing/pose changes near rest. Preserve foot phase through short blends; avoid hiding root correction errors by making the feet race.
5. Add restrained arm/wrist and tail follow-through, then start/stop and turn poses. Verify the body/face remains connected in both directions and during existing home actions.
6. Show a same-speed before/after loop in the real home. Integrate and validate on Android first; qualify the older iPad when the deferred device update is available. Keep technical checks and the user's visual acceptance separate.

**WALK-01 excludes:** controller acceleration changes, joystick redesign, network buffer retuning, a new physics engine, new world activities and the wider cast. Resume kitchen/bedroom feature work afterward. Home sitting, trampoline and dance polish can reuse the resulting pose/transition tools in a separate bounded pass.

## Acceptance and evidence to collect

| Check | Proposed acceptance / evidence |
| --- | --- |
| Look and personality | Same-speed comparison reads as a relaxed walk for both characters; preserved face/torso silhouette; user accepts the appearance |
| Feet and joints | No exposed gaps, snapping knees or conspicuous foot sliding at 0.25× playback; measure stance-foot drift against a proposed 2% of character-height tolerance in world-projected space |
| Normal inputs | Tiny taps, continuous walk, half input, diagonals, both directions, repeated stops and fast reversals remain responsive |
| Frame rates | Compare 30, 60 and supported high-refresh rendering with equal travel distance; similar pose at equal travel phase; no cadence dependency on refresh rate |
| Camera | Compare locked and following camera; camera panning while idle must not trigger a walk or turn |
| Held props and home | One real prop stays owned once; contact/hand presentation remains readable; walking away from a sofa/trampoline cancels the activity correctly; radio dance yields to walking |
| Family play | Two local views, then four mixed devices when available; remote gait stays coherent through start/stop, area changes and snapshot corrections; no bone streaming or ownership changes |
| Discontinuities | Teleport/travel, pause/resume, avatar switch and reconnect clear visual history; no giant stride, old foot anchor or pose stuck from the previous room |
| Performance | Record frame-time p50/p95/p99, animation CPU, allocations and draw/layout work on Android and A10 iPad; target stable 60 FPS where sustainable, with an explicit stable-30 fallback if needed |

These are **future acceptance criteria**, not results from this research session. The latest phone check proves build 115 opened and retained existing item data; it does not qualify the improved gait because that gait has not been implemented.

## Physics and smoothness: supporting research, deferred from WALK-01

The initial request used “physics and movement”; the later clarification narrows the immediate job to walking appearance. Retain these findings without widening that job:

- **Simulation and rendering:** current walking is custom C# position logic, not a Rigidbody2D solver. Fixed simulation steps with render interpolation help cadence under variable frame times; they do not author a natural limb performance. Long-frame clamps in current client presentation deserve separate profiling. [Fiedler: timestep and interpolation](https://gafferongames.com/post/fix_your_timestep/).
- **Network motion:** the current code already uses 30 Hz authority, approximately 20 Hz position sends, a 180 ms remote history buffer and immediate local visual anticipation. Keep those separate from gait quality. More buffering increases presentation delay. [Fiedler: snapshot interpolation](https://gafferongames.com/post/snapshot_interpolation/). Unity also distinguishes visual anticipation from full rollback/replay prediction. Our current adapter is custom; it does not become an AnticipatedNetworkTransform automatically. [Unity anticipation API](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/api/Unity.Netcode.Components.AnticipatedNetworkTransform.html).
- **Selective physics:** later loose balls can use controlled dynamics, while sitting, storage and trampoline occupancy remain explicit interactions. Unity's MovePosition is intended for kinematic Rigidbody2D motion and executes through the physics update; it is not a general smoothing switch for RectTransforms. [Unity MovePosition](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody2D.MovePosition.html). Box2D distinguishes body types and recommends fixed simulation steps; its 3.1 substep guidance must not be copied as if Unity exposed the same engine version/API. [Box2D simulation](https://box2d.org/documentation/md_simulation.html).
- **Secondary settling:** a damped spring is useful for a small visual trailing offset. Critical damping settles without oscillation; underdamping can suit a restrained tail response. It cannot supply missing contact poses or artwork. [Juckett: damped springs](https://www.ryanjuckett.com/damped-springs/).
- **Mobile smoothness:** Unity mobile frame targets and Android frame pacing affect when finished frames appear. Measure actual devices before changing them; a 60 FPS target alone is not a guarantee. [Unity targetFrameRate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-targetFrameRate.html), [Android frame pacing](https://developer.android.com/games/sdk/frame-pacing).
- **Later action polish:** the present trampoline is a repeating sine-height curve, and its landing/launch needs a separately authored compression, flight and recovery sequence. The current radio loop advances notes every 0.375 seconds while the dance oscillates at 1.6 Hz; those clocks are not beat-locked. A later home animation pass should use explicit action phases and an audio-derived beat phase. Neither changes the first walking-art task.

## Delivery record

Primary sources above were accessed September 26, 2026. Source inspection establishes the current implementation; all proposed gait parameters, artwork, foot constraints and visual acceptance tests remain pending. Research, goal sheet and plan are updated together. No runtime movement, input, networking or physics code was changed in this research pass.

The separately requested [Android update](android-home-update-2026-09-26.html) installed fresh release 115 while research was underway. That update delivers the prior home features, not WALK-01. Family server/helper 110 and Apple 101 remain unchanged; content-5 shared delivery still requires coordinated qualification.
