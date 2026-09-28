# Science playground: simpler controls for children

September 28, 2026. The user asked whether the science prototypes had researched how children's apps make flow and buttons easy, then authorized this implementation with “Start.”

[Open the updated playground](home-science-playground.html). This pass applies picture selection, short state-based actions and optional spoken help to all fourteen existing experiments. It is a browser prototype for review, not a Unity build or a phone/server update. The scientific models and version-1 browser save contract are unchanged.

## Research and the decisions it changed

Research covered publisher descriptions and publisher screenshots, alongside interaction/accessibility guidance. It did not include running or usability-testing those commercial apps. The design choices below are our application of the sources, not claims that copying a pattern guarantees toddler usability.

| Primary source | Evidence examined | Applied here |
|---|---|---|
| [Khan Academy Kids parent guide](https://khankids.zendesk.com/hc/en-us/articles/360006764812-Parent-and-Home-Account-Users-Getting-Started-and-creating-an-account-with-Khan-Academy-Kids) | Guide to central play, picture library and tapping Kodi for spoken help. Its [home screenshot](https://khankids.zendesk.com/hc/article_attachments/4405045744795) was visually inspected: a dominant play arrow and recognizable picture destinations. | Replace activity dropdowns with large experiment pictures; show a prominent next action and a separate Listen control. |
| [Toca Boca Jr](https://www.tocaboca.com/toca-boca-jr) | Publisher descriptions of open exploration, Toca Lab tools and Toca Train's intuitive controls without written instructions. | Let children tap the apparatus, offer picture tools and keep experiments open-ended. No compulsory quiz, score or Next-button sequence. |
| [Sago Mini Babies](https://sagomini.com/apps/babies/) | Publisher description of familiar actions, reactions and pretend play. One [publisher screenshot](https://sagomini.com/image/1/320/233/uploads/entities/gallery/babies-screenshot-1-final-en-1474584068.png) showed a character scene, not a control demonstration. | Keep actions concrete (Pour, Mix, Pump, Go) and responses visible. No claim about Sago's exact button placement. |
| [W3C enhanced target size](https://www.w3.org/WAI/WCAG22/Understanding/target-size-enhanced.html) | 44×44 CSS-pixel enhanced target guidance. This is an accessibility reference, not a toddler-specific validation. | Large separated controls; primary tools are about 90 pixels tall in portrait, at least 48 in compact landscape. |
| [MDN: media play](https://developer.mozilla.org/en-US/docs/Web/API/HTMLMediaElement/play) | Playback may require a user gesture and the play promise can reject. | Listen explicitly starts a local audio clip. No automatic narration; a failed start offers Try sound. |

The scientific background for each activity remains in the [original applied research](science-playground-research-2026-09-27.html). This pass changes access to those mechanisms, not their scientific model.

## The actual flows

Each experiment exposes at most three picture tools, with one emphasized action when an action is useful. The highlighted ring marks an object that can be tapped directly. Flights and chain reactions expose a Watch state while they run. Short text helps adults; pictures and tap-to-hear hints provide another route for children who cannot read.

| Activity | Simple entry and continuing play |
|---|---|
| Lava jars | Fizz → watch rising/falling blobs; change color. |
| Dinosaur ice rescue | Melt or brush warm drops onto ice → rescue → drag the dinosaur. Choose a dinosaur before melting. |
| Marble playground | Roll → move a ramp → roll again. Dragging and tap-then-place ramp handles remain available. |
| Stretchy slime | Pour → Mix → Stretch; quick pull tears it, Squish reunites it. |
| Magic milk | Color → Soap → flowing colors; add more drops. |
| Drawing robot | Go / Stop → change pen color; New paper when the trace is full. |
| Balloon rocket | Pump → Go → Watch → Bring back. More air is optional before release. |
| Foam fountain | Pour → Soap → add yeast with Fizz → watch the finite reaction → Again. |
| Wind tube | Fan on → bigger/smaller canopy or heavier/lighter load → Fan off. |
| Light-up inventions | Connect ready wires → On / Off → change between lamp and fan. Manual circuit wiring remains available. |
| Mini weather world | Rain / Sun / Snow act on the existing water-cycle simulation. |
| Bubble garden | Dip → Blow → tap bubbles to pop; dip again when film is depleted. |
| Rainbow mirrors | Light on if needed → move prism into beam → turn mirror and realign. |
| Family chain reaction | Go → watch; Bell/Ramp immediately changes a piece; repair a gap with Add piece. Optional joined sections remain under More to try. |

The state-based flow also handles experiments restored from previous saves or edited through advanced controls. A single Mix action applies two stir increments; Pump supplies three air increments. These generous gestures use the original model commands rather than directly marking the experiment successful.

## Four-player behavior and recovery

Each player retains all fourteen workspaces. Games opens only that player's picture chooser. Choosing a different experiment preserves the previous one. Restart affects only the selected experiment and offers Undo restart until another action or activity selection replaces that undo opportunity. New paper also offers undo. Grown-ups contains the optional Four together view, global preview pause, tap sounds and gentle effects; these are preview controls, not the production multiplayer authority.

More to try starts closed and contains the previous detailed sliders/tools. Real manual interactions remain: ice brushing, dinosaur dragging, slime pulling, ramp manipulation, wire terminals, prism dragging, bubble popping and chain placement. Pointer capture is separate per workspace; browser testing does not qualify simultaneous physical four-finger or mixed-device play.

## Voice and assets

Thirty-eight short WAV hints were generated locally using the installed **Microsoft Zira Desktop** voice at rate −1, volume 85, 22,050 Hz mono 16-bit PCM. These are draft science hints, not the final children's narrator or a replacement for book audio. [Scripts and provenance](science-playground/hints/manifest.json) are reproducible with `Tools/Build-ScienceHintAudio.ps1`; scripts are validated against `HINTS` in `kids-flow.mjs`.

Listen can stop a playing hint. Actions, leaving a workspace and backgrounding stop that workspace's hint. Playback errors do not block play. Local assets remove a runtime speech-service dependency; offline installation/caching of the entire browser page has not been added. Existing quiet tap tones remain optional.

Picture-tool icons are original inline SVGs and selection thumbnails use the existing experiment renderer. The existing illustrated workshop backdrop remains. This pass is interaction work; apparatus still uses prototype drawings and further art/effects polish remains open.

## Evidence and remaining limits

- [15 flow/audio test groups](evidence/science-kids-flow-2026-09-28/flow-checks.json) passed: staged mechanisms, finite reactions, restored state, at most three primary-screen actions, independent four-player commands and all 38 non-silent PCM clips with matching scripts.
- [20 existing model groups](evidence/science-kids-flow-2026-09-28/model-checks.json) passed again on September 28. The original harness retains its September 27 report date.
- [Browser check record](evidence/science-kids-flow-2026-09-28/browser-checks.json): all fourteen picture choices render; completed slime stages, undo and reload retention checked; direct pointer taps pumped and launched a balloon; browser media playback advanced for the correct hint. Opening player 2's chooser left the other three panels present. Keyboard activation was used for repeatable control checks; pointer hit testing was also verified directly.
- Portrait 390×844 and landscape 740×360 layouts were visually inspected, with no horizontal overflow measured. Tablet 1024×768 was visually inspected; 320×640 received a DOM overflow/target check. These are desktop browser viewport checks, not physical phone/iPad qualification.
- No JavaScript errors or warnings were captured in the final browser check. [Review screenshot](evidence/science-kids-flow-2026-09-28/kids-review.png).

Human listening quality, child usability, older-iPad performance, four simultaneous touch inputs, Unity integration, authoritative networking and physical device sound remain unqualified for this new collection. The wider Home backlog, original nine science stations and branch integration hold remain open.

**Next bounded task:** review the simplified prototype experience with the family, then detail one selected activity's apparatus and feedback before integrating it into the shared Home science area. Preserve these simple flows when adding detail.

To run: `python Tools/Serve-SciencePlayground.py`, then open the playground on port 8763. The server binds to this PC only. Browser storage remains `little-weeps-science-playground-v1` and never reads production game saves.
