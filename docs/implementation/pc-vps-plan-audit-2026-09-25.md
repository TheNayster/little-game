# PC/VPS architecture and plan audit

**Decision implemented in the plans:** the user's September 25 instruction, “Lets stick with pc/vps,” replaces the earlier device-hosting requirement. [Current decisions](../current-decisions.md) is the scope reference; the goal sheet describes the intended game and the build guide controls the work sequence.

## What was inconsistent

| Finding | Correction |
| --- | --- |
| Workflow instructions and multiple phase/feature ledgers still required both iPads to host | G4/AUTO-02 explicitly retired by user decision, not marked completed; devices are clients only |
| Travel, bedrooms, item returns and four-player research still proposed importing or merging offline edits | Private solo saves remain separate; server world wins on reconnect, no offline action replay or conflict-import system |
| Some outage instructions required a complete checkpoint before local play | Local continuation uses the latest usable visible state; complete recovery replicas serve a separate recovery purpose |
| Research-page cards and renderer templates still advertised mobile hosting as required | Corrected the templates and regenerated the pages, so old labels cannot return on the next render |
| README and companion summaries described old deployments as current or networking as unbuilt | Current status distinguishes last deployed server 91, physical clients 95/79/83, prepared client 98 and open acceptance |
| Old implementation reports and return lists still contained imperative “next” tasks | Classified 48 reports as historical, with prominent links to the current decisions, guide and return checklist; retained dated proof |
| Host testing was blocking character/content preparation | Next independent task is ART-PREP-02 Unity character view; G5 room/item contracts follow. No device-host gate |
| Unfinished peer-host work had begun after the previous experiments | Archived all 14 experiment files, patch and hash manifest locally; restored only those known experiment edits; moved its build to an explicitly abandoned location |

## Current architecture

One designated PC server, later the owned VPS after controlled migration, owns the shared family world. Up to four iPad/iPhone/Android clients automatically join after enrollment and can travel independently. A player leaving, changing area or opening local media does not move or pause siblings. One server arbitrates each shared item's holder and contents.

When disconnected, each device plays private solo. Reconnection loads the server's current world and retains private work separately. Clients never promote themselves. PC→VPS migration must stop the old canonical writer before enabling the new one; losing a route does not prove the remote server stopped.

Travel multiplayer stays optional over a usable internet route; full installed solo play stays required. Bluetooth, router-free peer co-op, mobile-host roles and automatic offline merging are removed. The six worlds, character roster, interactive props, spoken optional quests, kitchen/science/dinosaur games, books/TV, personal rooms and daycare remain in scope.

## Current work and the return list

Prepared client 98 needs local Mac signing, the focused older-iPad transition check and in-place rollout to the other devices. Four-device admission and scoped shared play already passed; do not repeat them as unstarted work. Build 95's smooth offline walking and cold reopening passed on the older iPad. No new device result is claimed here.

The [current return checklist](return-checklist-ipad-lan-2026-09-24.html) removes mobile-host testing. Unattended renewal, independent backup/restore, sustained updated-device performance and native Android 16 KB qualification remain open within their release scope. VPS details can wait. Isolated Unity character-view work can proceed while those checks wait.

## Scope and verification

The audit covers the goal sheet's 55 numbered chapters, the guide's 19 sections and all 35 feature IDs, the companion research, current operations/checklist pages, workflow instructions, rendered-page templates and dated implementation records. AUTO-02 is retained as a retired ID; no other feature ID is dropped. Source-download notes retain their historical context. Earlier evidence JSON, source captures and game saves are not rewritten.

[Scope inventory](../plan-scope.json) classifies maintained entries and historical records. `Tools/Test-PlanConsistency.py` checks classification, retired gates, known contradictory requirement patterns, numbered coverage, preserved feature IDs, rendered local links/anchors and historical-page notices. It also exercises representative conflicting phrases so the check must reject a reintroduced hosting requirement. This is a regression aid plus a manual audit, not a guarantee that any future prose is semantically consistent.

Validation results are recorded in [docs-validation.json](evidence/pc-vps-plan-audit-2026-09-25/docs-validation.json). Historical G4 model/restoration tests remain in Git as experiments and possible dedicated-server recovery references; their old native-hosting queue is retired. The uncommitted 99 experiment is archived under ignored `LocalData/AbandonedWork/ipad-host99-2026-09-25`, and its binary folder is explicitly marked abandoned. Prepared 98 remains the deployment candidate.

[Source-boundary verification](evidence/pc-vps-plan-audit-2026-09-25/source-boundary.json) confirms the restored Unity source matches the pre-experiment Git baseline, project version remains 98, and abandoned build 99 is outside the normal deployment path. No live runtime result is inferred from those checks.

This task audits requirements against existing research/evidence. It does **not** claim new web research, fresh runtime qualification, a deployed VPS, or completed game content. No physical device, live server, credential or family save was changed.
