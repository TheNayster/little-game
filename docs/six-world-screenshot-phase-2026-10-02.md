# Six world screenshot review phase

October 2, 2026. The parent requests the same illustrated review previously delivered for Daycare, now for all six other worlds. This plan covers Heeler Home, Playground and Park, The Creek, The Beach, Zoo and Dinosaur World. Daycare is excluded from this capture queue; its existing review remains separate.

The result will be six audits with real screenshots of menus, mini-game layouts, controls and relevant direct play, accompanied by plain-language stories, completion steps, NPC observations and improvement findings. This document creates the plan. Capturing the screenshots and completing the reviews are pending.

## Phase zero prepare one capture baseline

- [ ] Read AGENTS, current decisions, this plan and the existing world audits.
- [ ] Choose one verified completed client/server build that contains the intended current source. Record the exact source commit, build, date and compatibility. Setup source is `62c6fa9`; release421 is the latest completed build in the record at plan creation, not a promise that it will remain current.
- [ ] Use an isolated local world with four prepared player profiles. Keep live family saves, enrollment and server untouched.
- [ ] Use the existing native release inspector and capture helpers listed in the [code path map](world-audits/code-path-map.md). Reuse the verified current build; build once only if the needed current artifact is missing. Do not import unrelated unfinished edits into the review.
- [ ] Establish phone and tablet capture sizes, starting with 1280 by 591 and 1024 by 768. Label these PC aspect captures accurately; a physical-device screenshot needs an actually inspected device.
- [ ] Capture the shared Worlds chooser, application Menu, character chooser and any settings/help panels that affect the reviewed layouts. Record all visible labels and place these shared images under a common evidence folder.

Preparation ends when the artifact and capture environment are known, the common navigation pictures exist and all six world entry points can be reached. No game redesign, device install or server rollout is included.

## Screenshot requirements for every world

The Games menu and the application Menu are different screens. Capture both: the application Menu once in the common set, and each world's Games menu in that world's set. Capture the world's empty Games menu too if it has no listed games; never create an imaginary mini-game to fill the audit.

| Screen or interaction | Required evidence | What to explain |
| --- | --- | --- |
| World entry | Clear overview with navigation visible | Where the child arrives and how they find play |
| Games menu | Every card and all scrolled sections, with a tablet overview where layout changes | Names, pictures, entry points and hidden or clipped items |
| Activity start | Invitation, story, readiness or start controls where present | Goal and required first input |
| Active layout | Each mini-game and each distinct direct-play station | Buttons, tools, tap/drag/hold gestures and scene targets |
| Changing layout | Stage-specific controls, important choices and overlays | What appears, disappears or changes function |
| Help or mistake | One meaningful example where the activity has it | How the child recovers and whether help performs an action |
| Outcome | Completion, replay/exit controls or the outcome of open play | Exact required progress versus optional decoration |
| Four-player composition | One representative shared scene per world, including one child's independent exit | Crowding, blocked objects, NPC placement and retained sibling play |
| Tablet comparison | Each substantially different control layout, reusing equivalent layouts | Camera framing, placement and overlap differences |

Use phone aspect for every activity's main layout. Do not create extra shots for cosmetic variations that do not change the layout. Mark a nonapplicable start, ending, NPC or replay with a reason; do not force open play into a scored game. One focused capture pass is the default, with a repeat only to resolve a concrete missing or unusable picture.

## Phase one Heeler Home

Audit: [Heeler Home](world-audits/heeler-home/audit.md). Code: [Home files](world-audits/heeler-home/code-paths.md).

- [ ] Capture the world entry, Games menu and room/upstairs navigation.
- [ ] Capture Hide and seek: invitation, hiding/start window, cover interaction, search, outcome and exit.
- [ ] Capture Fishing and Feed fish: setup, rod/food targets, active interaction, result and exit.
- [ ] Inventory the actual discovery/science stations and capture every distinct tool layout and stage change, including mixing, ice rescue, bubbles, liquid colors and ramps where currently available.
- [ ] Capture coloring selection and tools, books/reader controls, and creation/display/storage interactions.
- [ ] Capture kitchen entry, ingredient/tool layouts, different preparation stages, food result and storage. Document actual available recipes; sample equivalent layouts rather than every unchanged recipe variation.
- [ ] Capture representative direct furniture/garden play, including sofa or trampoline composition with four children.
- [ ] Update each activity's story or play purpose, real steps, completion/open outcome, controls, NPC purpose, screenshot captions and findings in the Home audit.

Home's garden and bedrooms belong to this destination. Personal drawings and stored creations retain their existing ownership. Exit criterion: every current Home menu game and distinct direct-play layout is documented or has a specific evidence gap.

## Phase two Playground and Park

Audit: [Playground and Park](world-audits/playground-park/audit.md). Code: [Park files](world-audits/playground-park/code-paths.md).

- [ ] Capture park entry and the entire current Games menu; record the actual cards rather than relying on older reports.
- [ ] Capture Tag start/join, active chase/turn cues and All done. Preserve its existing absence of Tag speech/Listen controls.
- [ ] Capture slide, swings, roundabout, benches/table and fountain interaction layouts, including boarding and get-off controls.
- [ ] Capture wheeled-play selection, riding/steering and departure where available.
- [ ] Capture balloon-triggered Keepy Uppy as direct play rather than inventing a Games card.
- [ ] Show a representative four-player equipment layout and one independent departure; inspect contact with seats, blocked paths and NPC positions.
- [ ] Fill the Park audit with the real activity flows, captions, control map and evidence-based improvements.

Exit criterion: menu games and direct equipment layouts are covered, and sharing or boarding rules are understandable from the pictures and explanation.

## Phase three The Creek

Audit: [The Creek](world-audits/creek/audit.md). Code: [Creek files](world-audits/creek/code-paths.md).

- [ ] Capture creek entry and Games menu, including any scrollable portion.
- [ ] Capture Creek fishing setup, active controls/targets, result and exit.
- [ ] Capture Feed creek fish setup, food interaction, fish response and exit.
- [ ] Capture actual boat selection/launch/manipulation or retrieval controls and their results, based on the current build.
- [ ] Show bank/water framing, one four-player shared interaction and independent departure.
- [ ] Update the Creek audit with observed steps, layout pictures, animal/NPC purpose and priority findings.

Exit criterion: children can identify the fishing, food and boat targets from the documented layouts; unknown or unavailable boat actions are labeled rather than assumed.

## Phase four The Beach

Audit: [The Beach](world-audits/beach/audit.md). Code: [Beach files](world-audits/beach/code-paths.md).

- [ ] Capture beach arrival and every current Games card.
- [ ] Capture Ride the waves invitation/readiness, start, active wave controls, changes in cues, outcome and exit.
- [ ] Capture shore walking, footprints, water reactions and visitor interaction layouts.
- [ ] Inspect seagull-related interactions in the current build and capture their controls or label them as ambient behavior.
- [ ] Capture a representative four-player shore/wave composition and one independent exit, with phone/tablet framing.
- [ ] Update the Beach audit with actual play purpose, steps, controls and captioned findings.

Exit criterion: wave play and the distinct direct shore layouts are covered without inventing timers or goals for open exploration.

## Phase five Zoo

Audit: [Zoo](world-audits/zoo/audit.md). Code: [Zoo files](world-audits/zoo/code-paths.md).

- [ ] Capture zoo entrance and its Games menu, even if the list is empty.
- [ ] Inventory currently playable species/habitats and accessible trails; keep research wishlists separate.
- [ ] Capture every distinct habitat/control layout, food buckets, feeding approach and animal response. Cover materially different land, water and dinosaur exhibits.
- [ ] Capture food choice, busy/occupied feedback or repeated-action response where present.
- [ ] Show a representative four-player exhibit visit and one independent departure; review animal routines, depth and visibility of food targets.
- [ ] Document how feeding ends and open exploration continues, then complete the Zoo audit's captions and findings.

Exit criterion: the actual playable roster and interaction layouts are pictured. A subset of generic feeding shots must not be described as all exhibits if distinct layouts remain missing.

## Phase six Dinosaur World

Audit: [Dinosaur World](world-audits/dinosaur-world/audit.md). Code: [Dinosaur files](world-audits/dinosaur-world/code-paths.md).

- [ ] Capture destination entry and the Games menu, including an empty state if applicable.
- [ ] Capture T. rex, Triceratops, Brachiosaurus and Parasaurolophus direct interaction layouts.
- [ ] Capture mount entry, mounted movement/steering, call and get-off controls, with representative rider attachment differences.
- [ ] Capture feeding/petting, response and busy feedback where present.
- [ ] Show all four riders together and one rider departing independently, including useful phone/tablet framing.
- [ ] Complete the Dinosaur audit's activity explanations, code references, captions and visual/control findings.

Exit criterion: riding, care and call controls are documented, and Zoo exhibits, Home dinosaur toys and this destination remain clearly distinguished.

## Code paths in every audit

Use the linked [code path map](world-audits/code-path-map.md) as the starting inventory of files. For each activity and finding, record the exact existing client UI file, relevant rules file and shared menu/input/NPC file where applicable. Add a verified symbol or line number when it helps pinpoint a button or behavior; do not freeze unverified line numbers into the plan.

Every finding must link its screenshot and the responsible code path. A path identifies where to investigate; it does not establish that code causes the observed problem. Refresh paths when source moves and keep relative links usable in the repository.

## Phase seven assemble the illustrated handoff

- [ ] Verify all six audits have complete activity inventories and actual menu screenshots.
- [ ] Check the screenshot checklist for each activity; document missing evidence and nonapplicable states explicitly.
- [ ] Keep raw PNGs unchanged. Add build, capture date, viewport, player count and observation captions. Inspect the images rather than relying only on successful capture calls.
- [ ] Explain stories, goals, required completion, optional play, buttons/gestures, NPC roles and improvements in the same parent-facing style as the Daycare review.
- [ ] Link each finding to both picture evidence and exact code files. Keep proposed changes separate from current behavior.
- [ ] Include focused primary-source research only when a specific observed issue needs it; reuse relevant existing research and label its date and limits.
- [ ] Produce six independent world review folders and one combined review index, a common-menu screenshot set, a readable gallery/index and a ChatGPT Classic brief. Each world folder must contain its own audit, phase plan, code paths, findings and screenshots, with a standalone summary for fixing that world independently. Include the common code path map and copy necessary shared menu pictures into individual exports. Use relative image links.
- [ ] ZIP the completed review folder and verify its contents and readable image links. Exclude Daycare from this new bundle and preserve the old Daycare package.
- [ ] Commit checked audit documents and nonprivate evidence, integrate/push main, verify the remote commit and clean up a finished temporary branch.

Working folders: `docs/world-audits/<world>/` with `audit.md`, `phase-plan.md`, `code-paths.md`, `findings.md` and `screenshots/release-<build>/`. Shared menus: `docs/world-audits/common/screenshots/`. Suggested export folder: `Little Weeps Six World Review <capture-date>`, with `common/`, `heeler-home/`, `playground-park/`, `creek/`, `beach/`, `zoo/` and `dinosaur-world/`.

The export should contain the relevant code path list, not private saves, credentials, builds, signing material or copied personal media. No code changes or deployments are required to complete this review phase.

## Progress tracker

| Phase | Status at plan creation | Completion evidence |
| --- | --- | --- |
| Baseline and common menus | Planned | Capture build and shared menu images pending |
| Heeler Home | Planned | Screenshots and completed audit pending |
| Playground and Park | Planned | Screenshots and completed audit pending |
| The Creek | Planned | Screenshots and completed audit pending |
| The Beach | Planned | Screenshots and completed audit pending |
| Zoo | Planned | Screenshots and completed audit pending |
| Dinosaur World | Planned | Screenshots and completed audit pending |
| Illustrated folder and ZIP | Planned | Gallery, code paths, linked audits and verified ZIP pending |

## Completion rule

This phase is complete only when all six worlds have real menus and activity-layout pictures, their controls and play flows are explained, code paths are linked, and the review folder/ZIP is verified. Creating this plan or filling inventories is not screenshot completion. Device performance and whether children find the changes fun remain separate parent playtesting evidence.

Next action when executing this plan: establish the verified capture baseline and shared menu pictures, then begin Heeler Home. There are no unresolved preference questions for planning.

## Planning delivery record

October 2, 2026: created the six-world capture plan and six independent world folders with audit, local phase plan, verified code paths, findings and screenshot placeholders. Checked all six folder layouts and 361 relative/code/picture links and heading anchors. Screenshot collection has not started. This document is the work record for the planning task; concurrent game implementation records remain separate. Next: establish one current capture baseline and shared navigation pictures, then Home.
