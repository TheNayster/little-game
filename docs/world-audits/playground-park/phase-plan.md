# Playground and Park screenshot and review phase plan

Status: planned, no screenshots collected by this planning task. This folder contains only Playground and Park; Daycare is excluded. Run this plan independently using the shared [six world capture requirements](../../six-world-screenshot-phase-2026-10-02.md#screenshot-requirements-for-every-world).

## Prepare the review

- [ ] Read this audit, [code map](code-paths.md), AGENTS and current decisions.
- [ ] Select one verified current completed release; record source, build, date and viewport.
- [ ] Start an isolated world with up to four prepared players using the common native helpers.
- [ ] Capture this world's entry and complete Games menu, including scrolling or empty state. Reference the common application Menu, Worlds chooser and settings pictures, or include local copies when exporting this world alone.

## World capture checklist

Audit: [audit.md](audit.md). Code: [code-paths.md](code-paths.md).

- [ ] Capture park entry and the entire current Games menu; record the actual cards rather than relying on older reports.
- [ ] Capture Tag start/join, active chase/turn cues and All done. Preserve its existing absence of Tag speech/Listen controls.
- [ ] Capture slide, swings, roundabout, benches/table and fountain interaction layouts, including boarding and get-off controls.
- [ ] Capture wheeled-play selection, riding/steering and departure where available.
- [ ] Capture balloon-triggered Keepy Uppy as direct play rather than inventing a Games card.
- [ ] Show a representative four-player equipment layout and one independent departure; inspect contact with seats, blocked paths and NPC positions.
- [ ] Fill the Park audit with the real activity flows, captions, control map and evidence-based improvements.

Exit criterion: menu games and direct equipment layouts are covered, and sharing or boarding rules are understandable from the pictures and explanation.


## Complete the audit

- [ ] For every activity, explain story/goal, required steps, optional play, outcome, replay and independent exit.
- [ ] Capture its main phone layout and changing controls; compare each distinct layout on tablet. Include one useful four-player composition and independent departure for this world.
- [ ] Inspect raw pictures and caption build, date, viewport, player count and observation. Keep them unchanged in screenshots/.
- [ ] Map buttons, scene taps and gestures to exact code paths. Record NPC purpose, movement, reactions and blocked/crowded objects.
- [ ] Add evidence-backed issues and priority fixes to [findings.md](findings.md); keep proposed additions separate from existing behavior.
- [ ] Replace pending sections in [audit.md](audit.md) with actual evidence and a plain-language summary.
- [ ] Verify all local picture/code links and package this folder with a readable summary. Include copies of necessary common menu images so the folder can be shared independently.

## Fix one finding at a time

After review, use the parent-selected finding as one bounded task. Record the files changed, the expected visible result and the focused validation. Keep other worlds' files and unfinished changes separate. Shared-game changes need one representative four-player check and independent departure; save/schema or pairing checks apply only when those systems actually change. Integrate checked work into main and push under AGENTS. Apply device/server rollout policy if separately requested.

## Completion criteria

Menu and all current distinct activity layouts are pictured; play and completion are explained; findings link screenshots and code; remaining gaps are stated. No finding is fixed merely because a plan or code path exists. Review status and fix status are separate.
