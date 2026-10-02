# Zoo screenshot and review phase plan

Status: planned, no screenshots collected by this planning task. This folder contains only Zoo; Daycare is excluded. Run this plan independently using the shared [six world capture requirements](../../six-world-screenshot-phase-2026-10-02.md#screenshot-requirements-for-every-world).

## Prepare the review

- [ ] Read this audit, [code map](code-paths.md), AGENTS and current decisions.
- [ ] Select one verified current completed release; record source, build, date and viewport.
- [ ] Start an isolated world with up to four prepared players using the common native helpers.
- [ ] Capture this world's entry and complete Games menu, including scrolling or empty state. Reference the common application Menu, Worlds chooser and settings pictures, or include local copies when exporting this world alone.

## World capture checklist

Audit: [audit.md](audit.md). Code: [code-paths.md](code-paths.md).

- [ ] Capture zoo entrance and its Games menu, even if the list is empty.
- [ ] Inventory currently playable species/habitats and accessible trails; keep research wishlists separate.
- [ ] Capture every distinct habitat/control layout, food buckets, feeding approach and animal response. Cover materially different land, water and dinosaur exhibits.
- [ ] Capture food choice, busy/occupied feedback or repeated-action response where present.
- [ ] Show a representative four-player exhibit visit and one independent departure; review animal routines, depth and visibility of food targets.
- [ ] Document how feeding ends and open exploration continues, then complete the Zoo audit's captions and findings.

Exit criterion: the actual playable roster and interaction layouts are pictured. A subset of generic feeding shots must not be described as all exhibits if distinct layouts remain missing.


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
