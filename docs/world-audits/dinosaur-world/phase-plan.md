# Dinosaur World screenshot and review phase plan

Status: planned, no screenshots collected by this planning task. This folder contains only Dinosaur World; Daycare is excluded. Run this plan independently using the shared [six world capture requirements](../../six-world-screenshot-phase-2026-10-02.md#screenshot-requirements-for-every-world).

## Prepare the review

- [ ] Read this audit, [code map](code-paths.md), AGENTS and current decisions.
- [ ] Select one verified current completed release; record source, build, date and viewport.
- [ ] Start an isolated world with up to four prepared players using the common native helpers.
- [ ] Capture this world's entry and complete Games menu, including scrolling or empty state. Reference the common application Menu, Worlds chooser and settings pictures, or include local copies when exporting this world alone.

## World capture checklist

Audit: [audit.md](audit.md). Code: [code-paths.md](code-paths.md).

- [ ] Capture destination entry and the Games menu, including an empty state if applicable.
- [ ] Capture T. rex, Triceratops, Brachiosaurus and Parasaurolophus direct interaction layouts.
- [ ] Capture mount entry, mounted movement/steering, call and get-off controls, with representative rider attachment differences.
- [ ] Capture feeding/petting, response and busy feedback where present.
- [ ] Show all four riders together and one rider departing independently, including useful phone/tablet framing.
- [ ] Complete the Dinosaur audit's activity explanations, code references, captions and visual/control findings.

Exit criterion: riding, care and call controls are documented, and Zoo exhibits, Home dinosaur toys and this destination remain clearly distinguished.


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
