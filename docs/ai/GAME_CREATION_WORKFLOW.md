# Efficient Little Weeps creation workflow

Maintained October 7, 2026. Applies to the current Little Weeps repository and its Unity project, not the old Meeps project. Read the relevant sections for the current task; this guide does not require a new plan, tool setup or full test suite for every small change.

## Start and continue with the right state

Read root `AGENTS.md`, [current decisions](../current-decisions.md), and the relevant active work record. Check branch, HEAD, origin and scoped working-tree changes once before editing. Inspect additional repositories only if this task affects them. Preserve unrelated tracked and untracked work; isolate overlapping work when needed. Do not use reset, clean, stash, forced checkout, skip-worktree or broad ignores to manufacture cleanliness.

Read bounded sections and search the relevant owner first. Retain verified file paths, object IDs and decisions through the task. Reinspect after a project switch, domain reload, intervening edit or concrete uncertainty; do not repeat whole-repository discovery at each build. Current user instructions override older reports. Continue the authorized outcome after a status question or context compaction; do not restart completed work.

Resolve routine implementation choices using existing patterns. Ask only for a missing decision that materially changes gameplay, assets, save compatibility or authorization. Continue independent work while awaiting an answer. Do not ask again for an action already authorized, invent a new approval gate or treat standing bridge access as permission for unrelated edits. Stop immediately when the user says stop.

## Tools and connection ownership

Prefer named custom MCP tools, then the established project-checked bridge, then existing build/file tooling. Use computer automation only for operations those cannot perform. A listed tool or successful launcher request is not proof of a live connection. Confirm target root, active scene/file and readiness once per connection or project switch before mutation. Preserve open projects and unsaved work.

For Unity, prefer KitWright `execute_code` when actually available for this project. The existing [Unity client](../../Tools/unity_mcp.py) instead checks the `FamilyPlayset` instance and exact project root through the MCP-for-Unity endpoint; its tool names are not interchangeable with KitWright. [Connect-GameTools.ps1](../../Connect-GameTools.ps1) is the existing reconnect entry point, not evidence of acknowledgment. Inspect the exposed provider and use its documented schemas. Keep credentials, authenticated URLs and pins out of instructions, captures and commits. Do not reinstall or change tool configuration merely to expose a missing name.

Use owner-approved remote operations within their actual capability and task boundaries. Discover current project/share IDs and grants; do not reuse expired IDs. Consult `C:\dev01\agentKssh\docs\KEN_PC_RUNBOOK.md` when available for Ken-PC work; that path was absent during this comparison. If absent, report the missing procedure and use only an already documented supported operation. Never substitute arbitrary remote shell edits or claim a notice/archive is live validation.

## Unity edits, compilation and recovery

- Batch independent bounded reads, inspect every result, and carry returned IDs into dependent calls. Resolve ambiguous names to exact scene/prefab/asset paths before changing anything.
- Use Unity Editor APIs for `.unity`, `.prefab`, and serialized `.asset` edits, preserving file IDs, GUIDs and prefab overrides. Use the established build tool for version/build settings. Ordinary C#, JSON and documentation edits can use file tools. Do not hand-edit scene or prefab YAML to bypass unavailable editor access.
- When using KitWright snippets, use its documented `IKitWrightCommand`/context Undo registration. With other providers, use their Undo support or Unity Undo APIs. Null-guard lookups and return concise before/after values; do not create fallback objects when a lookup fails.
- Make one coherent source edit group, then refresh/compile once. With KitWright, exit Play Mode before `request_recompile`; only wait for compilation if it reports importing/compiling. With MCP-for-Unity, use its documented refresh/compile flow. Do not add duplicate error queries when the compile result already contains them.
- A timed-out mutation or interrupted reload has an unknown result. Inspect actual target state and recovery/compile status before retrying; avoid duplicated imports, objects or builds. Use provider recovery/ping rather than repeated full catalog discovery.
- Save only intentionally modified scenes/prefabs/assets and read back their values. Verify import/package completion rather than equating queued work with completion. Return to the relevant gameplay scene/view when safe; preserve dirty scenes and do not import Iron Route's map paths or camera setup.
- Leave unrelated editors and generated runtime caches alone. Never change security settings to get a compiler or bridge working; report the actual blocker and use an established supported alternative within scope.

## Blender and asset work

Preserve the game's recognizable 2D character art, costume layers, editable sources, pivots and reference scale. Prefer its established SVG/raster/UI pipeline when it fits the request; Blender is useful where modeling, rigging, rendering or export is actually needed.

Confirm the active `.blend` file/project, scene, units, selected target and output destination before edits. Prefer structured BlenderBridge tools for supported operations. For substantial geometry work, use a persistent reviewed script in the bridge's active project scripts directory and `run_project_script` when available. Do not assume that provider exists on every connection; follow the active provider's documented API. Trusted scripts are executable code, not a sandbox.

Checkpoint before substantial edits into a task-owned ignored location. Use stable collection/object names; preserve unrelated geometry, materials, rigs and cameras. Inspect transforms, dimensions, modifier results, materials and relevant topology/clearances. A bounding-box intersection is not a complete geometry check. Save editable source under the existing [folder ownership](../project-folder-guide.md), stage generated previews in `LocalData/<Task>`, and import only the intended runtime export into Unity with existing metadata preserved.

Inspect actual rendered/viewport images, then check imported Unity scale, orientation, pivot, layering and appearance in gameplay. A successful render or export alone does not establish correct integration. Checkpoint restoration can discard unsaved work: inspect the current state and restore only within the authorized task. Do not modify/install a bridge as part of an asset task unless needed and authorized.

## Verification and review

For this Zoo pilot and its future review prompts, the owner requests live in-game review without video recording. Use the configured isolated preview, inspect normal-speed gameplay directly and leave it ready for owner input. Preserve historical recordings; do not make new recordings unless the owner requests them.

For substantial visual changes, record a short brief in the existing work record: requested appearance, references, protected artwork/gameplay and observable criteria. Inspect the complete gameplay view at relevant phone/tablet resolutions and affected interaction states. Use close views as supplementary evidence, not as a substitute for normal-size readability. For animation, review normal-speed footage and movement afterward. Listen before claiming subjective audio approval.

Use the project's focused verification policy: compile plus affected visual/input check for presentation, the changed activity for gameplay, one representative four-actual-client check with independent departure for shared changes, and save/protocol checks only when affected. Distinguish scripted fixture setup from actual mouse/touch interaction. Reuse source-matching evidence for unchanged behavior. Broaden only for an observed failure or new relevant change. Documentation-only work needs document/path/diff checks, not a game build.

Keep technical validation, visual inspection, parent approval and deployment authorization separate. Record exact build/revision, actual client count, viewport dimensions, capture paths, observed defects and untested limits. A queued call, generated image, successful compile or archived screenshot is not current runtime acceptance.

## Completion and conversation

Keep progress updates brief: outcome learned, remaining uncertainty and next step. Do not narrate every tool call or repeat unchanged gates. Final delivery states what changed, checks actually run, material limits, main commit and usable artifact links. Report built, launched and installed states separately; after launching a game, verify the required family configuration and a durable ready state instead of only process creation. Use [the enrolled player launcher](../../Tools/Start-FamilyLAN.py) for family PC play and read [server policy](../server-update-policy.md) before rollout. Never change the live server just to open a compatible client.

Use the existing `LocalData` and Tools purpose groups; do not copy Helios's junction/runtime relocation scheme or create a second game repository. Keep reusable automation separate from disposable fixtures. Retain referenced evidence, current caches, saves and recovery inputs. Remove task-created disposable artifacts only when their exact ownership and lack of consumers are established; cleanup is not permission to erase pre-existing material.

Review and stage only owned changes, follow root main integration/push/branch cleanup, and verify remote HEAD. Report tracked changes and pre-existing untracked work separately; never call the whole repository clean merely because tracked status is empty. Update the existing work record and a concise handoff only when needed, avoiding duplicate queues and reports. A remaining handoff names the exact goal, finished requirements, outstanding check, source/artifact revision, blocker and next action.

## Comparison and adoption record

Read-only comparison sources:

- Helios: `C:\dev01\MyProjects\helios\AGENTS.md` and `unity\AGENTS.md`. Adopted preservation, scoped commits, truthful cleanliness, temporary-artifact ownership and concise handoffs. The sidebar Helios directory contained only `.git` and no usable HEAD/instructions; it was not treated as the active source. Conflicting historical `P:\Projectstwo` paths, mandatory two-repository gates and machine-specific junctions were not transferred.
- Iron Route: `C:\Users\sephi\Desktop\Truck game\iron-route-unity\AGENTS.md`, its `.codex/skills/unity-mcp-workflow/SKILL.md`, and sibling `BlenderBridge\AGENTS.md`. Adopted batching, stable IDs, Undo, serialized-asset safety, compile/reload recovery, persistent Blender scripts, checkpoints and actual visual review. Its generated KitWright block, test project, map scene, tooling installation tasks and remote output paths were not copied.

This was an instruction/documentation pass. No Unity/Blender connection, asset change, runtime build, installed app, save, live server or security change was performed or claimed. The unchanged Little Weeps authority, four-player participation, artwork, rollout and Git policies remain in force.
