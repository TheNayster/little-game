# Secret rooms: construction research and implementation contract

September 26, 2026. SECRET-01 / H-35–H-38. Research completed before secret-room implementation, following the user's instruction to continue from the furnished bedrooms. Starting milestone: Windows 142, schema 8/content 9, commit bfed20a. Installed Samsung remains 138. This report extends [the upstairs research](upstairs-bedrooms-research-2026-09-26.md#14-secret-room-construction-and-systems-for-the-next-stage), chapters 32–33 of the [goal sheet](../bluey-game-research-2026-09-23.md), and the [Home tracker](../home-world-feature-tracker.md). It is a design and acceptance contract, not evidence of a completed build.

## What the user will see

Four optional secret rooms belong to the four saved player profiles. Walk to the far right/back of a bedroom, away from its ordinary hallway entrance. A little star door gently appears through a glow and a few drifting sparkles. Approach reveals; tapping enters. The first owner tap creates their room once. Visitors can discover and enter a room after its owner creates it. No password, quest, timer, score or forced group transition.

Inside: an illustrated midnight-blue room, slow green/purple aurora, tappable stars, four cushions, a usable four-place blanket fort, rug, shelf and real plush chest. Six bounded plush types—dinosaur, dog, cat, rabbit, bear and sea animal—remain individual carryable objects. Reuse owner decoration, Together, undo and storage rules. The ordinary exit is always visible, and a screen control can return to the parent bedroom. The full six-book reader remains a tracked shared-system dependency; do not add a pretend narration button.

## Research findings applied to this project

| Primary source, inspected September 26 | Relevant fact | Implementation decision |
| --- | --- | --- |
| [Unity 6.3 CanvasGroup](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/CanvasGroup.html) | Group opacity, interaction and raycast blocking are separately controlled. | Fade the local door group; disable its hit target while hidden. Transparency alone does not make it noninteractive. |
| [Unity UI optimization](https://unity.com/how-to/unity-ui-optimization-tips) | Noninteractive raycast targets and excessive changing UI geometry add work. | Fixed small pools of decorative stars; raycast only the door and deliberate star targets. Reuse existing uGUI sorting and avoid per-frame hierarchy/layout allocation. |
| [Unity 6.3 Resources.LoadAsync](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Resources.LoadAsync.html) | Async resource loading returns a request which completes later. | Reuse existing room preloading and three-panorama residency cap. Do not commit entry until the local destination art is ready. |
| [NGO 2.13 scene management](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/scenemanagement/using-networkscenemanager.html) | Network scene loading synchronizes scenes with clients. | Keep rooms as saved logical areas in this game's existing authority, with local presentation. A sibling entering a room must not load or move everyone. |
| [Unity 6.3 serialization](https://docs.unity3d.com/6000.3/Documentation/Manual/script-serialization-rules.html) | Serializable field records have defined container/reference restrictions. | Flat four-record secret registry, nested copied furniture state and stable string IDs; no serialized runtime dictionaries or texture references. |
| [Unity texture Read/Write](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/TextureImporter-isReadable.html) | Readability retains a CPU-accessible copy. | Shared non-readable panorama and bounded sprites; no CPU pixels, bloom, sky video or volumetric lighting. Native measurements cannot establish A10 performance. |

These sources establish API behavior. Reveal distances, timing, artwork and room geometry below are project design choices that require play/visual checks, not claims of published best values.

## Persistent room and entrance lifecycle

Add schema 9/content 10 and the next compatible wire contract. Migrate schema 8 by appending exactly four reserved secret records; preserve every old player, bedroom, object, world identity and enrollment. A record keeps its stable ID, parent bedroom, owner, created flag, entrance active/style/slot/revision and independently copied furniture state. Derive ownership from the existing saved bedroom mapping, never avatar choice or connection order.

Creation is owner-only and idempotent, adds six fixed-ID plush objects once and opens the entrance atomically. Repeated requests must not refill toys. An owner may hide/show or change the entrance at prevalidated far-wall slots. Hiding/moving changes the entrance record only: retain contents, layout and visitors. Reject entry submitted against an old entrance revision. Existing occupants can exit while the owner is absent or the outside entrance is hidden. No deletion operation is provided.

Reuse the one item registry, one-holder rule, support/container slots, private saves and server recovery lane. Secret plush possessions retain the parent bedroom's ownership identity. Adapt shared furniture access to bedrooms plus created secrets; never loosen validators to accept an arbitrary area or item. Bound stock to 27 existing objects plus six per created secret (51 maximum). Snapshot copy must deep-copy every new mutable record before background serialization.

## Door interaction and safe travel

Reserve a clear far-end approach in both furniture arrangements. Initial reveal radius 260 floor units, hide radius 340, fade roughly half a second. Use hysteresis to avoid edge flicker. Only the current viewer's character determines reveal; another player nearby does not reveal it remotely. Reduced motion uses a steady glow. No movement or automatic teleport is caused by the reveal.

After a tap, walk to the door through the clear front corridor, preload the destination, stop, and submit the existing revision/visit-checked travel command plus entrance revision. Authoritative proximity still applies; a hidden control is not an authorization check. On failure before commit, stay in the source; cancel on menu, pause, connection loss or changed route. Once committed, use the ordinary local loading guard and render authoritative arrival.

Entry preserves held item identity and settles only the traveler's activity. Four arrivals are staggered. Exit always resolves the persistent parent, independently of entrance visibility/revision, using its current clear arrival or permanent fallback. A return button provides a direct safe escape if walking is inconvenient. Corrupt unresolved parents are rejected by save validation rather than silently deleting or regenerating a room; recovery retains the blocked source. Normal archive/move operations never create invalid parents.

## Art, usable furniture and magic

Use the imagegen skill in built-in mode for the secret architectural panorama, separate miniature door, open blanket fort and plush sheet. Inspect the accepted bedroom art first. Preserve Bluey/Bingo sources. Save exact prompts and source hashes. One shared architectural texture serves all four rooms. No furniture is painted into the base and duplicated by interactive sprites.

Reuse rug, shelf, chest, lamp and four cushions. Substitute a blanket fort for the bed: four occupancy positions, rear cloth, visible occupants and front curtain/lip masks. Leaving one position must not clear siblings. Stored plush use the real shelf/chest support coordinates and foreground covers. Fixed arrangements keep exits and front walking corridor clear; no arbitrary furniture placement yet.

Use two translucent green/purple ribbon meshes with slow vertex motion and a small fixed star pool. Sparkles are bounded local presentation; no replicated particle state or world revisions. Star taps produce a short local twinkle/chime. Avoid flashes, mandatory audio and automatic book narration. Stop/fade ambience when outside, paused or muted. Brightness, reduced motion and sound preferences are durable per local profile/device and must not enter shared snapshots.

## Acceptance map

| Requirement | Evidence required |
| --- | --- |
| Four persistent profile-owned rooms | Four creates, repeat/reconnect, owner rejection, avatar independence; exact stock count and deep-copy checks. |
| Far-back reveal | Native phone/tablet captures hidden at ordinary entry, visible near back; separate clients at different distances; hidden hit target disabled. |
| Independent travel | Four separate secrets and four visitors together; carried object continuity; local art cap; others unmoved. |
| Entrance lifecycle | Change/hide during approach, reject stale entry; retain toys/layout; visible exit and return button with owner offline and after reopen. |
| Real play | Four cushions, four fort positions, one leaves independently; real plush pickup/storage/closed chest/reopen; decoration permissions. |
| Calm effects | Local motion/brightness/sound independence and persistence; still aurora; no global state changes from effects. |
| Save safety | Schema-8 upgrade preserving prior state/enrollment, cold reopen, isolated recovery; offline edits retained privately and not imported on reconnect. |
| Regression and performance | Bedroom/Home/stairs/Keepy checks appropriate to changed paths; source/artifact identity; bounded textures/effects; A10 and physical mixed-device qualification remain explicitly open. |

Implement this contract before claiming the secret-room milestone. Record actual tests, captures, remaining limits and the next bounded task in the implementation report and trackers. Kitchen and all other Home scope remain required.
