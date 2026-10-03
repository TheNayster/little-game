# Sandcastle Club — proposed Codex prompt sequence

Status: DRAFT implementation breakdown.
The user accepted the shared creative sandpit direction. Detailed mockup, numeric choices, and migration approach still need resolution.
Keep the Sandcastle-Club-Research-and-Redesign.html brief available to Codex; this file does not replace it.

## Shared context

Little Weeps is a Unity game for children ages 3–6. Sandcastle Club should support up to four family members together in one shared sandpit, not separate solo castles. Preserve independent leaving and the PC/VPS authoritative world contract, private offline solo, and server-world behavior on reconnect.

Current source audit found four fixed moulds with two/three-scoop capacities; wet/full tipping; exclusive flag-or-shell decoration; teacher-linked completion; and duplicated scene/toolbar actions. Codex must verify these observations against the current checkout.

The new direction is: choose mould → choose place → scoop → water → tip → decorate → continue playing. Build and Decorate picture trays replace the crowded duplicated command layout. Flexible placement, combined decorations, and continued toy play are the intended outcomes.

Suggested first shapes: round tower, square tower, wall, gate. Suggested controls are large tap targets with optional dragging. These are proposals to settle in Stage 1. No required timer, score, reading lesson, or automatic castle clearing.

Prompt only one stage at a time. All implementation stages read the approved brief, current applicable AGENTS.md, current decisions, and current execution record. Use the existing documentation locations after discovery; do not create competing queues or replace AGENTS.md.

## Prompt 1 — Verify and prepare the plan

```text
Prepare the Sandcastle Club redesign for implementation using the research brief and docs/ai/prompting/GAME_APP_PROMPTING_GUIDE.md.

Read current applicable AGENTS.md, current decisions and build guide, then inspect the existing sandpit implementation. Verify the research findings and identify equivalent UI/input/shared-state helpers.

This is a docs-only planning task: no production code, runtime assets, save changes, server replacement, or installation. Prepare an annotated layout proposal and interaction sequence for review.

Propose six stages: plan; first working shared build; complete controls/shapes; decorations and toy play; final presentation; targeted verification/playtest fixes. For each, name the actual existing owner/files, dependencies, player-visible outcome, acceptance criteria, and available checks.

Recommend choices for unresolved placement rules, scoop repetition/recovery, same-piece conflicts, piece limits, individual editing/reset, and save/protocol migration. Explain which choices block which stages; do not silently turn prototype numbers into approved requirements. Check reset participation rules against current project instructions.

Record the feature requirements and evidence in the existing handoff/work-record format, preserving unrelated entries. Stop with the proposed plan/mockup and material questions. Do not implement the redesign yet.
```

Review the proposal and settle material decisions before Prompt 2. This is a design checkpoint, not a request for confirmation on every code edit.

## Prompt 2 — Make one shared piece work end to end

```text
Implement only the approved Stage 2: the first working shared Sandcastle build. Read the approved brief, execution plan, current repo guidance, and decisions.

Connect selection, approved placement, scoop/water/tip, and the resulting piece using one mould shape and clearly temporary minimum controls. Integrate authoritative shared state from this stage. Use the approved migration/compatibility approach; do not replace the live server as part of source implementation.

Done when a player can complete a piece, the shared model is consistent, other participating clients see the same piece, and one player leaving does not remove the creation or stop siblings. Protect relevant old-save state according to the approved plan. Reject stale/conflicting commands using existing patterns as needed.

Run focused logic/save checks and a representative four-client check if available; record unavailable evidence honestly. Stop after this working slice. Additional shapes, final controls, decorations, and polished art remain later stages. Update records and follow current source delivery rules.
```

## Prompt 3 — Complete shape choices and touch controls

```text
Implement only approved Stage 3 of Sandcastle Club. Verify Stage 2 dependencies first.

Add the approved mould set and placement behavior, the Build/Decorate tray structure, and the next useful tool beside the active bucket. Replace duplicated old action controls coherently. Every essential build action must work through taps; optional dragging cannot be the only route. Show selection/fill/wet state clearly and keep avatars/labels from hiding essential targets. Help is optional and does not delay starting play.

Done when a child can choose each approved shape, place it, and complete the build through taps; selection stays player-specific while pieces remain shared; and layout stays usable at the intended tablet/phone aspect ratios. Use temporary decoration content only where explicitly planned.

Run compilation and a focused rendered/input check, plus a targeted shared check only where changed behavior requires it. Stop before decoration/toy implementation and final artwork. Record pending actual-device evidence and current delivery status.
```

## Prompt 4 — Decorate and play with the creation

```text
Implement only approved Stage 4 of Sandcastle Club. Read the approved brief and current records.

Support the agreed decorations together on a piece, forgiving attachment/preview behavior, and the first approved daycare toy interaction. A completed castle remains available for continued building and pretend play. Remove the old mandatory four-build completion/teacher gate where it conflicts with the approved creative flow.

Implement only the approved individual edit/removal and family reset behavior. Do not let one ordinary completion/replay action silently clear everyone’s work. Preserve shared results, independent exit, and relevant persistence through the existing authority model.

Done when the agreed combined decorations and toy interaction work, siblings can continue after a departure, and deliberate edits/reset follow the approved participation rules. Check the changed shared/save behavior proportionately. Stop before final artwork or optional missions. Update records and source delivery evidence.
```

## Prompt 5 — Finish artwork, animation, sound, and framing

```text
Implement only approved Stage 5: Sandcastle presentation, using the approved mockup/art references and current functioning game.

Match the existing illustrated daycare style. Make the sandpit the focus. Complete readable sand silhouettes, bucket fill, darker wet sand, scoop/pour/tip/reveal feedback, layered decorations, and the approved brief toy reactions. Keep controls and targets visible during effects and character movement. Reuse appropriate existing assets and audio; identify any missing asset requirement rather than calling placeholders finished.

Preserve approved gameplay and shared contracts. Do not redesign server logic or add missions. Validate compilation and the affected visual/interaction states through available captures and tooling. Keep performance checks limited to concrete affected concerns on the intended baseline.

Stop with reviewable captures, actual check results, and any visual/device approval still pending. Update records and follow source delivery rules.
```

## Prompt 6 — Verify the finished flow and fix demonstrated issues

```text
Complete approved Stage 6 for Sandcastle Club.

Review every approved requirement against current implementation and existing evidence. Test the focused end-to-end flow: enter, choose/place, build, decorate, toy play, independent leave, join/rejoin, and deliberate editing/reset. Use one representative four-client scenario to check simultaneous different-piece building, the agreed same-piece conflict rule, and departure. Run focused save/compatibility checks only for the affected contracts.

Reuse valid previous checks for unchanged source. Fix demonstrated Sandcastle defects; do not expand scope or remove Picnic Counting. Record which checks actually ran and which need the user’s device/family playtest. Fun, control feel, and visual acceptance cannot be inferred from compilation.

Prepare the short device playtest steps and report final source/delivery status under current policies. Do not install apps, replace the live server, or claim device qualification unless those actions are independently authorized and performed. Stop with an honest implemented/verified/pending status and remaining specific blockers.
```

## Send fixes as focused follow-ups

After a child playtest, describe the concrete observed issue and expected behavior using CODEX_PROMPT_TEMPLATES.md, template C or D. Do not resend “redo everything” or discard working stages because one interaction needs adjustment.
