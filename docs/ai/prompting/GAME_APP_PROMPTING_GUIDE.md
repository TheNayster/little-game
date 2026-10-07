# Writing useful Codex prompts for games and apps

Status: original project guidance, supported by the [official source index](OFFICIAL_SOURCES.md). These are preparation recommendations, not replacement repository instructions.

## Efficient implementation and conversation

Follow [the creation workflow](../GAME_CREATION_WORKFLOW.md) for the relevant Unity/Blender procedure. A stage prompt names the outcome, existing owner, protected behavior and evidence; it does not need to repeat every bridge/API rule. Keep the user in the conversation with concise findings and progress, resolve routine choices, retain authorizations and corrections, and ask only material missing decisions. A status question does not cancel the active task. Preserve a compact handoff on unfinished work rather than making the next chat restart discovery.

Use bounded batched inspection, one coherent edit/compile group, recovery readback before retries and proportionate checks. Verify the connected project before edits and inspect full-size gameplay captures for visual work. Checkpoint substantive Blender edits and verify the actual Unity import. Adopt these methods without copying another project's scene paths, generated settings or cleanup gates. Source delivery, visual approval and rollout remain separate.

## 1. Describe the player experience precisely

Start with what a player should be able to do and what they should see or hear. For Little Weeps, specify children ages 3–6, touch controls, pretend play, and family participation when relevant.

Weak: “Make sandcastle more fun and improve the UI.”

Useful: “Children choose a mould, place it in the shared sandpit, fill it, water it, tip it, and add decorations. A completed piece remains available for further building and toy play.”

Keep audience, behavior, and visual intent separate. “Cute” is not a usable layout specification. State the reference, the attributes to preserve, the changed composition, and what should be visible at each interaction state.

## 2. Supply a small set of relevant evidence

Include:
- The approved feature brief and active stage.
- Relevant current project documents and verified code locations.
- A current gameplay capture and an approved mockup, if appearance changes.
- The observed problem, reproducible steps, logs, or multiplayer scenario.

Mark screenshots as CURRENT, TARGET, or INSPIRATION. Competitor images explain a design principle; they do not authorize copying their art or imply that all their behavior should be imported.

Work’s audit describes the inspected version. Codex must verify it against the current checkout. Mark an unverified class, file, command, or behavior UNKNOWN rather than inventing it.

## 3. Keep three levels of information separate

| Level | What belongs there |
| --- | --- |
| Existing project instructions | Durable repo, Unity, Git, server, platform, and verification rules. |
| Feature brief | Approved player outcomes, design decisions, exclusions, dependencies, and acceptance criteria. |
| Stage prompt | The one piece of work authorized now, required inputs, checks, and stopping point. |

Do not repeat the entire research report or rewrite AGENTS.md in every stage. Name the source documents and the relevant requirement IDs. A prompt should remain understandable without pretending the model remembers every earlier chat.

## 4. Plan a coherent redesign before production coding

Use a planning request for a redesign that changes controls, shared rules, artwork, or saved state. Ask Codex to:
- Inspect the current implementation and equivalent helpers.
- Map requirements onto existing systems.
- Identify shared protocol/save changes and dependencies.
- Propose stages with visible outcomes and appropriate checks.
- Surface only decisions that materially change the product or compatibility.

Routine implementation choices should follow project patterns. Questions should include a recommendation and its effect. Do not block all useful inspection because one decision is unresolved.

Native Plan mode is useful for planning. If it cannot write files, obtain the plan as a response, then explicitly authorize saving it. In a normal coding chat, a docs-only planning task can allow plan documents while prohibiting production code edits. State which you want.

## 5. Split at playable milestones

Choose stages around things a player can do, rather than an arbitrary list of files.

For example, the first working castle piece should connect selection, placement, a build action, display, and shared state. It can be limited to one shape, provided it is explicitly a temporary milestone. Do not finish a large solo system and bolt on four-player authority later.

A practical redesign sequence:
1. Verified plan and interaction/layout proposal.
2. Small working shared build loop.
3. Complete intended controls and shape choices.
4. Decorations and continued pretend play.
5. Final art, animation, sound, and framing.
6. Focused end-to-end checks and playtest fixes.

This is a Little Weeps recommendation. Small isolated fixes do not need six prompts. If a stage is still too large, split it further while preserving a clear player outcome and dependency.

## 6. Define “done” as visible behavior

Use a starting state, action, and expected result.

Examples:
- With two clients in the same sandpit, player A completes a piece; both clients display the same completed piece.
- A child can finish the intended interaction using taps without mandatory reading or precision dragging.
- Leaving the activity removes only that participant; remaining family members continue using the shared creation.
- A flag and shell can remain attached together after rejoining, if persistence is part of the approved design.

Keep prototype appearance, final visual approval, functionality, device qualification, and child enjoyment distinct. Compilation does not prove touch usability, and screenshots do not prove shared state or fun.

## 7. Use project-appropriate validation

Follow the current AGENTS.md preference for proportionate checks. A layout edit may need compilation and a focused capture/input check; a shared-state change needs a representative four-client check; a changed save contract needs a focused retention/migration check.

Use existing test/editor tooling. Request meaningful automated coverage where it protects a changed contract; avoid mirror tests and new general test frameworks without need.

A blocked device check is “not run,” not “passed.” Record what is pending and whether it actually blocks the next stage. Repair a demonstrated dependency failure before building on it. Do not rerun unchanged checks at every stage or demand commercial-release qualification for a private family update.

## 8. Keep progress recoverable

At each stage, update the existing work record and feature plan with:
- Implemented requirements.
- Decisions and deviations.
- Checks actually run and results.
- Pending human/device evidence.
- Current branch/commit and the next bounded task.

Use the repository’s established locations. For Work handoffs, preserve the established CODEX_FIX_QUEUE.md schema: IDs, observed state, research, outcomes, criteria, dependencies, confidence, and unknowns. Verify the actual path/case before creating a second competing queue.

Follow current Git delivery instructions. A completed stage may be integrated if it is coherent and verified under those instructions. Unfinished dependent redesign code should stay isolated and identified. Source delivery does not authorize replacing the live server or installing an app.

## 9. Avoid common prompt failures

| Failure | Better instruction |
| --- | --- |
| “Make it perfect.” | Name observable outcomes and the actual reference. |
| “Do all the redesign now.” | Authorize one stage and identify the final feature goal. |
| “Never ask questions.” | Resolve routine details; surface material product/save/protocol ambiguities. |
| “Ask before every change.” | Complete the authorized stage autonomously; stop at its boundary. |
| “Use this web game’s architecture.” | Reuse Unity patterns and preserve current authority. |
| “It compiles, so it’s finished.” | Verify the stage’s behavior; label unavailable evidence. |
| “Redo every related system.” | Change only what this requirement needs. |
| “Follow every historical report.” | Read active decisions; use dated reports as scoped evidence. |

## 10. Quick prompt check

Before sending:
- Is the active goal and stage explicit?
- Can Codex access the named files?
- Are current, approved, suggested, and unknown items distinguishable?
- Are shared play, platform, and save requirements included when affected?
- Does the stage have concrete completion evidence?
- Does it stop before the next stage?
- Does it defer technical choices to repository inspection without allowing product drift?

Official foundations: [Codex best practices](https://learn.chatgpt.com/guides/best-practices), [long-task walkthrough](https://developers.openai.com/blog/run-long-horizon-tasks-with-codex), [game walkthrough](https://developers.openai.com/blog/how-to-build-games-with-astra). The detailed Little Weeps recommendations above are original synthesis.
