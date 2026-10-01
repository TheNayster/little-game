# Project boundary

- This game's GitHub home is the private repository `https://github.com/TheNayster/little-weeps-game`, with Git remote `origin`. Use this repository for this game's source/history; never substitute a repository from the unrelated old project. Keep credentials, signing material, generated builds and local saved worlds out of commits. Preserve Git LFS media when pushing or restoring.
- Personal TV source videos belong in `Media/TV/`. Its contents are ignored except `README.md`; never force-add personal clips, thumbnails, subtitles or local catalogs. The separate generated foundation test clip remains tracked. Folder preparation does not mean the TV importer is implemented.

- This directory is the new home for the family game described in `docs/bluey-game-research-2026-09-23.md`.
- `C:\Users\sephi\Desktop\Meeps game` is an unrelated old Unity project. Do not move, copy, merge, or modify that project as part of this game's work unless the user specifically requests it.
- The desktop Connect Unity + Blender and Connect Little Weeps shortcuts now target this root's launcher. The old project's launcher remains unmodified. Require a fresh acknowledgment of the exact new Unity project path before claiming an editor connection.
- Read the current build guide for implementation status. Dated reports preserve scoped evidence; their old next-step instructions are historical, not the active queue.

## Game implementation workflow

- Read `docs/current-decisions.md` first. The user explicitly selected PC/VPS-only multiplayer on September 25: clients never host, offline solo stays private, and reconnecting loads the authoritative server world without importing offline edits. G4/AUTO-02 are retired, not pending gates. Do not revive retired scope from old research or experiments.
- Treat `docs/bluey-game-research-2026-09-23.md` as the feature goal sheet and `docs/family-playset-build-guide-2026-09-23.md` as the default implementation sequence. The user's latest instructions take precedence.
- Before implementation, read the build guide's current work record and the goal-sheet sections for the task. Follow the active phase and its dependencies; later phases do not remove required features.
- Treat accepted movement tuning as the ordinary baseline. Do not repeat its speed or multiplier in progress/final updates unless it changes or the user asks.
- September 30 Tag correction: one start includes all connected park players; arrivals automatically join the same session. Keep independent All done/travel/departure and solo fallback. No Tag speech or Listen control.
- Design every shared activity for four simultaneous family players. The sofa and trampoline use four closer spots within their existing artwork; preserve independent participation when a player moves, leaves or changes area.
- Explicit user correction, September 28: EVERY future game is for up to four players playing TOGETHER in one shared session. Four separate solo activities do not satisfy multiplayer. Use shared invitations/readiness, a common start/countdown and shared progression where the game has rounds. Independent exits protect the remaining group; they must not create personal rounds/timers. September 30 hide-and-seek clarification: one Start broadcasts a common 15-second hiding window to every connected player. Entering cover before zero opts into the round; only those still hidden at zero are sought. Nonparticipants are ignored. Coming out or moving after zero withdraws only that hider, with no mid-search re-hide; siblings continue. Verify the broadcast, staggered hiding and ignored nonparticipants on four clients.
- Keep one bounded implementation task active, record its goal IDs and acceptance evidence, and update the work record with what exists, what passed and the next task. Do not mark a phase complete based only on code/assets being present.
- Character art must follow the existing official reference catalog and the recognizable 2D Bluey style in the goal sheet and Toca/Piknik supplement. The user rejected the generic blue-pup workshop on September 25. Animation/compilation checks do not establish visual acceptance. Use Bluey and Bingo for the first character test, preserve editable layered sources, and do not replace the requested cast with generic lookalikes or describe generated drafts as approved.
- Keep platform builds, saved data, shared-world recovery and actual-device qualification in the sequence. The PC/VPS is the sole shared authority; keep automatic joining, four mixed clients and independent areas. Do not use the old project's connector as proof this new project is connected.

## Proportionate checks and token use — September 30 user preference

- This is a private family game. Optimize for useful changes and quick updates with targeted verification, not a full commercial-release qualification on each task. This latest preference supersedes older blanket testing/rollout checklists for routine updates.
- Routine device update: build the requested current release once (or use an already verified identical current artifact), install in place, confirm the intended version and launch. Do not add research, full gameplay suites, server redesign or repeat qualification. Accept the user's playtesting as visible behavior evidence.
- Small visual/control change: compilation plus one focused visual/interaction check. Gameplay change: one targeted check of the changed behavior. Multiplayer changes need one representative four-client check of shared behavior and independent departure, not every unrelated activity.
- Save/schema, pairing, recovery or server-startup changes warrant focused retention/migration or connection checks for the affected contract. Do not run full save-directory/hash audits, recovery drills, sustained play or all-device qualification when those systems did not change.
- One verification pass is the default. Repeat or broaden only after a relevant failure, further code changes or a concrete unresolved risk. Use existing evidence for unchanged source. Before expensive additional checks, briefly state what specific uncertainty they resolve; do not turn hypothetical risks into gates.
- Keep tool output and progress concise. Use short records for small changes; avoid repeated repository/history scans and new comprehensive reports for routine installs. Preserve saves, enrollment and signing identity; never uninstall, clear data or downgrade as a shortcut. Report actual blockers and unverified limits honestly.

## Shared PC server and app updates — September 30

- The earlier server227 carried status hotfix `227-status-io-1`; the current installed authority is recorded in `docs/server-update-policy.md`. Preserve it during compatible app updates; future server builds must include `DiagnosticFileWriter` and its `WriteJson` call. The policy records the corrected crash evidence; the original helper-only correction was insufficient. Do not restore the unpatched original 227 assembly into the live slot.

- Read `docs/server-update-policy.md` before every app/server rollout, even if this chat read AGENTS earlier. Multiple user chats share this checkout and the live family server. Re-read the rollout record before acting; do not use a chat's cached server build or endpoint as authority.
- Compatible app releases must leave the existing server process, endpoint, firewall, enrollment and parent-helper selection alone. App build numbers do not need to equal the server build. Shared protocol/content compatibility does need to match; never bypass admission checks to conceal a mismatch.
- Do not bump shared compatibility for artwork, sound, menus or other client-only presentation changes. When shared rules/messages/save fields change, record the reason and prepare a coordinated server update. Missing compatibility metadata means unknown, not permission to replace a server.
- PC availability means awake and connected. Use the existing recovery/parent intent and selected family. Never create a fresh family, reset a save, change sleep settings silently or restart occupied family play to apply an app-only update.
- The persistent PC server is installed; its actual baseline, checked commands and limitations are recorded in `docs/server-update-policy.md`. Other chats may build content and follow that policy; never treat a newer app build as authorization to replace the live server. Preserve unrelated concurrent edits and the existing main integration hold.

## Git delivery and project records

- The user wants Git maintained and completed work pushed as part of each project task. Check the working tree, current branch and `origin` before editing; preserve unrelated user changes.
- Keep changes focused and make descriptive commits explaining the resulting behavior and purpose. Add code comments for non-obvious decisions, constraints or workarounds; avoid comments that merely repeat the code.
- For implementation milestones, update the main build guide and relevant evidence with what changed, what actually passed, remaining limits and the next task. Documentation-only or repository-maintenance tasks need an appropriate record, not unrelated game tests.
- Review the diff and run checks appropriate to the change before committing. Stage only relevant files. Respect `.gitignore`, preserve tracked Git LFS assets, and never force-add private media, signing keys, credentials, generated builds or local saves.
- Push completed, checked commits to this private `origin` and verify the remote branch points to the intended commit. Keep `main` synchronized with completed work through a safe fast-forward or normal integration. Keep incomplete or failing work off `main`; identify it clearly if checkpointed on a development branch.
- Do not force-push, rewrite shared history, delete branches or discard changes as routine cleanup. Reconcile concurrent updates without overwriting them. Report a genuine push/authentication/conflict blocker instead of claiming the work is uploaded.
- End meaningful work with a clear status: what changed, relevant validation and any unpushed or unfinished work. This workflow applies while working on the project; it does not imply an unattended background sync service.

- When architecture changes, audit the goal sheet, phase/feature ledgers, return checklist, companion research and generated-page renderers. Run `Tools/Test-PlanConsistency.py` after rendering. Preserve dated evidence without treating its superseded instructions as current requirements.
