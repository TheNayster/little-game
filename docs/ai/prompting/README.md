# Codex prompting reference pack for Little Weeps

Prepared for the user’s Work-research / Codex-implementation workflow.
Sources checked October 2, 2026 (America/Chicago).

## What this pack contains

These are original, source-linked working guides and templates, accompanied by an index of actual official documentation. They are not copies of OpenAI’s full documentation, and they do not guarantee a correct implementation.

| File | Purpose |
| --- | --- |
| [../GAME_CREATION_WORKFLOW.md](../GAME_CREATION_WORKFLOW.md) | Maintained Unity/Blender, batching, recovery, visual review and conversation procedures adapted from Helios/Iron Route. |
| OFFICIAL_SOURCES.md | Verified official guides, a game walkthrough with real prompts, and when each source applies. |
| GAME_APP_PROMPTING_GUIDE.md | How to turn game/app research into a bounded, verifiable Codex task. |
| FEATURE_BRIEF_TEMPLATE.md | Reusable specification for the desired player experience and constraints. |
| CODEX_PROMPT_TEMPLATES.md | Planning, implementation, repair, visual, and continuation templates. |
| SANDCASTLE_STAGED_PROMPTS.md | A proposed six-stage sequence for the current Sandcastle Club redesign. |

## Where to put it

This pack is maintained in the active Little Weeps repository at `docs/ai/prompting/`. Use these files in place; do not extract an old archive over current instructions. Optional ChatGPT project copies are references and must be refreshed from this maintained source.

The current repository’s own `AGENTS.md` remains the instruction source. Do not replace it with an old exported copy or with this reference pack. Ordinary reference files are not automatically loaded as Codex instructions: explicitly name the relevant file in a task.

Optional short pointer to add to existing project guidance, without replacing any existing rules:

> When preparing a substantial game/app task, consult docs/ai/prompting/GAME_APP_PROMPTING_GUIDE.md and the relevant template. Keep this guidance subordinate to the user’s current instructions and the repository’s current decisions.

## How to use it

1. Work reads the guide, inspects the relevant game, and fills a feature brief with verified evidence and recommendations.
2. The user settles material design decisions, using a mockup when appearance or interaction is changing.
3. Codex verifies the current repository and creates a stage plan.
4. Send one implementation stage at a time. Let Codex finish its authorized stage, repair relevant failures, and report evidence before sending the next.
5. Keep the approved brief, stage decisions, and progress in repository documents. A fresh chat reads those files rather than reconstructing the project from memory.

For Sandcastle Club, keep the existing research brief with this pack. Its broad direction was accepted, but detailed layout and unresolved numeric choices are not automatically approved by this pack.

## Maintenance

Recheck the linked official docs when a Codex feature changes or guidance seems outdated. Update templates after a recurring, demonstrated problem. Do not append every historical instruction to every prompt.
