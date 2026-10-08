# The Missing Ball — milestone 10 elephant pilot

WORLD-02 / FAMILY-01 / ITEM-02. One optional, picture-first story in the existing elephant exhibit. No quest engine, other animal story, habitat decorating, fossils, narration, video or deployment. Stop for live owner review after this pilot.

## Flow and pictures

Tap the small ball picture beside the empty toy basket deliberately. One brief curious ball-picture clue runs when the elephant can safely spare a moment. Searching is immediately available even if it is eating, splashing or being brushed. One partly exposed turquoise/coral ball sits behind a separate small leaf clump, at one of two authored nearby locations. Tap it to walk automatically along the existing visitor path and pick it up. The ball remains visible in the child's hand; tap the ball/arrow picture at the basket to walk back and return it. Acceptance frees the child's hands immediately, including when the elephant is busy.

One short acknowledgment walks to the basket, touches the ball with the original deformed trunk, rolls it on a deterministic short arc and returns it to the basket. No physics, score, timer, failure, unhappy idle pose or repeated prompt. The eight-second quiet replay reset only enables a deliberate new invitation; it never starts a story itself.

Basket at `(1390,400)` lies away from the food bucket, brush, pump, snack station and existing discovery controls. Leaf-clump candidates `(560,180)` and `(2130,740)` use the existing horizontal visitor path; their vertical positions are drawn scenery, not destinations children must walk into. Pickup destinations are `(x,100)`. Art is retained editable native UI geometry with [SVG source](../../../SourceArt/Zoo/Playable/elephant-missing-ball.svg). Existing animal atlases, scenery, GUIDs and poses are preserved. Native visual acceptance and physical devices are separate.

## Ownership and recovery

| Rule | Behavior |
|---|---|
| Single shared session | Authority chooses one spot once from world identity/session high-water ID. Available → Searching → Carried → Returned → Reacting → Complete. Competing starts coalesce. No per-child round or participant requirement. |
| Pickup | Authority validates exact current session, proximity, eligibility and empty hands atomically. One carrier; other taps leave all state intact and show gentle feedback. Existing request receipts deduplicate retries. |
| Explicit handoff | One always-available pictured put-down button while carrying returns the ball to one of two authored safe leaf mats. Any eligible child can collect and finish it. |
| Other held items | Ball pickup rejects ordinary food, editable/prepared snacks, shared care tools and ordinary toys. Food/snack/tool/toy selection rejects an existing ball. Finish or explicitly put away the current item first; food tickets/pieces are never silently replaced by a ball. |
| Departure/ineligibility | Walking beyond the exhibit, travel and actual disconnect release the ball to a reachable safe mat. Siblings retain story and unrelated care/food. Last exhibit exit/disconnect clears the story; synchronous disconnect cleanup works even if the empty authority stops ticking. |
| Reconnect/snapshots | Render the current session/phase/age; never spawn a second ball. Seed observation after visit/reconnect/pause. Completion sound is once per session, with no event queue. |
| Save reopening | Existing transient policy resets story, carrier, clue and elephant play pose while retaining feeding history and unrelated saves. Session high-water remains to reject delayed old-session commands. Missing legacy fields normalize to available defaults. No save schema migration. |

## Scheduling and sound

The existing Zoo scheduler owns StoryClue, StoryWalk and StoryReact. Starting does not retask the animal. Offered food wins before every story scheduling decision. Existing care and water work is considered before story work; their sessions/progress/order are untouched. Intro clue is at most 1.1 seconds; final reaction at most 1.8 seconds. Story movement and both brief poses yield to food at the next authority tick, adding **zero scripted waiting time**, at most the existing 0.1-second native tick plus transport/snapshot observation time. Preempting a started final reaction consumes it rather than queuing another. A return received while busy remains visibly accepted in its basket and schedules only one acknowledgment when safe. Repeated starts cannot add work to an active session.

The short existing feeding-style Foley uses the retained Zoo source and existing mute/foreground gain. No narration or new service. Pictures, plants and shared ball are retained objects, reset with Zoo; the put-down UI is destroyed with its owner. No per-tap spawning or new subscription/timer controller. Subjective audio approval requires listening/owner review.

Content **77** replaces76 because shared commands, validation and snapshot fields change. Schema53/protocol3 remain. Candidate clients/server use an isolated authenticated loopback family; the installed463/content72 authority, devices, enrollment and actual saves remain untouched. Admission checks are retained.

## Separate snapshot-test diagnosis

Completed before story implementation and separately delivered as `3c6df72`. [Exact expected/actual values, causal commit and all eight passing legacy groups](photo-album.md#snapshot-test-follow-up--october-8). The old failure was a stale fixture caused by intentional transient restore revision/cooldown changes in `00083a2`; production behavior and assertions were not bypassed. No Application Control settings changed.

## Changed files and verification

- Core: ZooWorld scheduler/state/copy/normalization, new ElephantStory state/commands/recovery; shared GameWorld movement/travel/item exclusion; ElephantSnack selection guard; WorldLayout content77.
- Client: new GameScreen.ElephantStory retained geometry/input/presentation; Zoo build/tick/depth/reset and overlay guards; ElephantPlay's original trunk articulation; shared GameScreen reuses the existing holding pose.
- Checks: ElephantStoryTests runs through both permitted .NET and Unity JSON/prebuild; legacy ZooRules includes the same authority tests. FamilyGameVerification adds ball/trunk point readbacks. Native Test-ElephantStory and Test-ElephantStorySolo use the established configured launchers. New source metadata and editable SVG are retained. The diagnostic launcher and Test-ZooPhotos accept an explicit Direct3D11 option; their default renderer and assertions are unchanged.
- Build/records: fresh release version, this audit and existing current-decisions/build work record.

## Verification and live review — October 8

| Check | Result and evidence |
|---|---|
| Existing snapshot question | **Passed:** eight unchanged-strength legacy groups after separately scoped fixture repair; exact cause/values above. Latest run also passes the new story authority group. |
| Unity build/JSON gates | **Passed:** fresh standard504 client and dedicated server, zero errors/warnings; 2,400 current Unity inputs hash-match. Includes story copy/JSON/restore/atomic ownership/recovery, existing album storage and normal full-game prebuild gates. [Build receipt](../../../Builds/NetworkProbe/G3-0.0.504/build-summary.json), [source readback](../../../LocalData/SharedGarden/781e7158c05d4147a6e3013f3caefbad/source-readback.json). |
| Four actual native clients | **Passed, nine groups on503:** full flow/handoff/replay; both spots on1280×591,1024×768,640×400; competing start/pickup/return; carrier travel/disconnect/reconnect/late join; ball/bucket/prepared-snack exclusion without food loss; care/water/both surprises; overlay shielding/pause/all-leave; unchanged giraffe/navigation. [Results](../../../LocalData/SharedGarden/ca9b0344317443348c53ffb820fc40a8/elephant-story/results.json). |
| Private solo and actual saved process reopening | **Passed, three groups on503:** real screen pans/taps and automatic walking through complete story/replay/pause; close/reopen retires carrier/story/pose; actual album image persists through another restart. [Results](../../../LocalData/FamilyLAN/36da85c9663c4fcea4c3d25c1b321062/elephant-story/results.json). |
| Existing photo persistence and input regressions | **Passed, ten groups on503 with explicit Direct3D11**, including strict normal process shutdown and no runtime errors. [Results](../../../LocalData/SharedGarden/804db469692646fdb2694494f6a6ce5d/zoo-photos/results.json). |
| Final504 visual and live interaction | **Passed:** final hiding leaf edge, native small-phone/full-phone captures inspected; stable carried ball at raised hand; complete Bluey/Bingo put-down/sibling-return; deliberate replay and food interrupts reaction. Observed offered-food→owner readback0.578seconds includes command/transport/snapshot overhead; authority gate asserts preemption at next0.1second tick, zero scripted added waiting. [Ready receipt](../../../LocalData/SharedGarden/781e7158c05d4147a6e3013f3caefbad/live-review-ready.json), [food timing](../../../LocalData/SharedGarden/781e7158c05d4147a6e3013f3caefbad/elephant-story/food-priority.json), [later durable readback](../../../LocalData/SharedGarden/781e7158c05d4147a6e3013f3caefbad/stable-live-readback.json), [native owner view](../../../LocalData/SharedGarden/781e7158c05d4147a6e3013f3caefbad/elephant-story/owner-ready-final.png). |
| Physical devices, subjective sound and owner acceptance | **Unrun/pending owner review.** Native resized windows are simulated layouts, not phone/tablet qualification. No audio listening or owner acceptance claim. |

Only two Unity inputs differ503→504: leaf-edge presentation in GameScreen.ElephantStory and release version. All shared rules, save/network contracts, existing album/navigation/animals, packages and remaining runtime inputs are identical; their passing503 evidence is reused. Final504 has focused visual/story interaction checks rather than repeating unchanged suites. Both live504 configured review clients remain open in the elephant exhibit, no pending action, after session3 completed with empty carrier and replay available. The isolated authenticated authority ends when its review windows close. Older500 review windows were preserved.

## Failures and remaining limits

Build501 first complete story/handoff passed, then replay exposed invitation interception by an ordinary animal-call target. Invitation foreground depth was repaired, and subsequent replay passes. The initial carried-ball height was also lowered to fit the existing raised-hand pose. [Original failed run](../../../LocalData/SharedGarden/f85210b8a6ee47e4ac522cf08737a755/elephant-story/results.json) remains preserved. Early private test fixture attempts panned/tapped through existing food controls; the final fixture pans through a clear foreground strip and follows actual automatic walking, with assertions retained.

The default Direct3D12 photo regression run passed nine gameplay groups but **failed strict process shutdown**, not a story/save assertion. [Failed receipt](../../../LocalData/SharedGarden/0888d7844fb946aea4e976e6e4496d23/zoo-photos/results.json). Windows Application events for both503 clients report `D3D12Core.dll`1.618.1.0, exception `c0000005`, offset `a1f5`. Earlier500/499/498/495 events show the identical module/offset before this story. This pre-existing renderer teardown issue remains open, not repaired or labeled harmless. Unity's [documented command-line renderer selection](https://docs.unity3d.com/2019.4/Documentation/Manual/PlayerCommandLineArguments.html) supplies the explicit diagnostic `-force-d3d11` alternate: all ten album groups and normal close then pass. Live504 review uses that option. Default production renderer/security settings, assertions and binaries remain unchanged; no Application Control bypass. This does not qualify default Direct3D12 shutdown.

Test-PlanConsistency passes after the maintained build-guide status paragraph was rendered into its existing HTML, preserving its layout. No video, device install, live-server replacement or real-save operation. Finished source is integrated/pushed to main with verified remote identity; the final handoff supplies the actual commit. Pre-existing untracked project/history/audit/evidence files and unfinished checkouts remain untouched. Next: live owner review, then stop; no other animal stories, habitat decorating or fossil discovery.
