# Codex task prompt templates

Original templates. Replace bracketed fields and remove irrelevant requirements before use.
Read GAME_APP_PROMPTING_GUIDE.md for context. Refer to the actual current repo instructions; do not paste this whole file as instructions.

## A. Inspect and plan a significant game/app change

```text
We are working on [feature] in Little Weeps. Work has prepared the attached/named research and feature brief; you own repository verification and implementation planning.

Read the current applicable AGENTS.md, active project decisions, and [brief path]. Verify the relevant findings against the current implementation and look for existing systems to reuse.

This task is planning only. Do not edit production code, runtime assets, saves, or server configuration. [Allow documentation updates in the existing project locations / return the plan in your response.]

Goal: [desired player experience].
Prepare:
- The verified current behavior and discrepancies with the research.
- A proposed interaction/layout mockup or annotated existing capture when needed.
- A dependency-aware stage plan with player-visible outcomes, acceptance criteria, and actual available checks.
- Any shared protocol/save migration implications.
- Only material unresolved questions, each with a recommendation and the stage it blocks.

Routine technical choices should follow existing project patterns. Do not invent product behavior or file locations. Stop after the plan and reviewable proposal; implementation comes in a later task.
```

## B. Implement one approved stage

```text
Implement only [STAGE ID] of [feature], using [approved brief] and [execution plan/work record]. Read current applicable repo instructions first and verify the stage dependencies against current state.

Goal: [one player-visible result].
In scope: [required outcomes / IDs].
Deferred: [later outcomes].

Acceptance:
1. Given [state], when [action], then [result].
2. [shared/input/save requirement affected by this stage].
3. [visual requirement if applicable].

Finish this stage autonomously, using existing project systems. Fix relevant failures before reporting; do not begin the next stage. Surface only a material unresolved decision or genuine blocker. Follow current Git delivery and rollout policies.

Run proportionate checks available in this environment. Update the existing plan, queue/work record, and relevant implementation evidence. Report what changed, checks actually run, unverified criteria, deviations, source delivery status, and the next bounded task. Do not claim unavailable Unity/device/multiplayer checks passed.
```

## C. Correct an observed gameplay bug

```text
Fix [specific observed failure] in [feature].
Starting state: [state/version].
Steps: [reproducible actions].
Actual: [observed result].
Expected: [approved behavior].
Evidence: [capture/log/code reference].

Inspect the current owner and establish the failure when practical. Make the smallest coherent fix within the existing architecture. Preserve [directly related shared/save/input behavior]. Run a focused reproduction/check, review the diff, and update the existing work record. Report actual results and unavailable evidence. Do not expand into the next redesign stage.
```

## D. Improve a specified visual/control state

```text
Update [screen/activity] to the approved target [file], using current capture [file] as the baseline.
Match these attributes: [composition, silhouettes, scale, layering, palette].
Required interaction states: [selected/progress/recovery/etc.].
Required tap behavior: [behavior].
Preserve [gameplay/shared state boundaries].

Use existing Unity UI/art/input patterns. Change presentation only unless [explicit behavior change] is authorized. Inspect a rendered capture and the affected interaction using available tooling. If the target environment is unavailable, provide the reviewable result and mark the visual/device check pending. Compilation is not visual approval.

Stop after this scoped task. Record what was actually verified and follow current delivery rules.
```

## E. Resume in a fresh chat

```text
Continue [feature] in this current checkout.
Read the applicable AGENTS.md, current decisions, [approved brief], and [plan/work record]. Check Git state; preserve unrelated work.

The requested task is [STAGE ID or exact remaining requirement]. Verify the recorded progress against current files and available evidence; do not restart completed work or revive superseded reports. Repair an actual blocking dependency if needed, then complete only this task and report evidence and updated status.
```

## F. Review a completed stage

```text
Review [stage/change] against [approved requirement IDs], including the directly affected touch, shared-state, and save behavior.

Compare the diff and existing evidence with the intended player outcome. Identify concrete defects, skipped requirements, or unsupported completion claims. Do not request unrelated full-project qualification. Report findings with verified locations and reproduction/evidence; distinguish a defect from a check that was unavailable.
```

These templates define task scope; they do not override existing instruction precedence, permission settings, or deployment policies.
