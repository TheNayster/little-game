# Official sources for game/app Codex prompting

Checked October 2, 2026 (America/Chicago). Each page below was opened and reviewed. Summaries are paraphrases; applications to Unity are recommendations.

## Core reading

### 1. Codex best practices

[Official guide](https://learn.chatgpt.com/guides/best-practices)

Use for everyday task prompts. OpenAI recommends specifying the goal, relevant context, constraints, and the observable completion condition. It recommends planning complex work, practical reusable project instructions, and relevant verification/review.

### 2. Run long horizon tasks with Codex

[Official walkthrough](https://developers.openai.com/blog/run-long-horizon-tasks-with-codex)

Use for substantial redesigns. It demonstrates repository files for the specification, milestones, execution guidance, and progress. Milestones have acceptance criteria; failed checks are repaired before progression. This is an experience report, not proof that one file arrangement is universally best.

### 3. Custom instructions with AGENTS.md

[Official documentation](https://learn.chatgpt.com/docs/agent-configuration/agents-md)

Use for persistent repository rules. Codex discovers project guidance with directory-based precedence. This does not mean every Markdown reference file is automatically read. Keep the actual current project instructions authoritative and explicitly reference task documents.

## Games and visual apps

### 4. Building games with Astra

[Official game-development walkthrough](https://developers.openai.com/blog/how-to-build-games-with-astra)

Contains actual edited prompts from developing Void Explorer in Codex. The author begins with what the player can do, refines visual references, lets Codex propose architecture, and uses repeatable scenarios, state, screenshots, and measurements to investigate issues.

Important limit: this is a TypeScript/Three.js browser game. Its Playwright and browser-state mechanisms are examples, not Unity requirements. Apply the process through the project’s existing Unity editor, test, capture, and device workflows. It is not a requirement to select Astra for this project.

### 5. Designing delightful frontends with GPT-5.4

[Official visual-design guide](https://developers.openai.com/blog/designing-delightful-frontends-with-gpt-5-4)

Use for giving visual context: design constraints, screenshots, and reference direction. Its detailed marketing-page layout rules and model settings are specific to that example. Do not impose hero sections, web layouts, or browser testing on a Unity mini-game.

### 6. Phantasy Codex Adventure

[Official showcase with initial prompt and iterations](https://developers.openai.com/showcase/phantasy-codex-adventure)

A concrete example of expressing a game’s intended experience, controls, visual direction, and later refinements. It is a showcase, not a Unity tutorial or evidence that a large initial request always succeeds. Its Sites publishing, leaderboard, and progression features are not Little Weeps requirements.

## Sources not used as default instructions

The older [PLANS.md cookbook](https://developers.openai.com/cookbook/articles/codex_exec_plans) is explicitly marked archived. Its behavior-based acceptance examples can be useful, but use current documentation for product features and model choices.

API system-prompt and harness examples are intended for developers building their own agents. Do not paste an entire system prompt into an ordinary Codex task or replace the existing Unity workflow with one from a browser example.

## Evidence boundary

No official source reviewed establishes a special universal “correct Unity game prompt.” The templates in this pack combine official Codex guidance with Little Weeps’ verified project constraints. The six-stage Sandcastle sequence is a proposed implementation breakdown, not an OpenAI-prescribed sequence.
