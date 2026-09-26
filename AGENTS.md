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
- Design every shared activity for four simultaneous family players. The sofa and trampoline use four closer spots within their existing artwork; preserve independent participation when a player moves, leaves or changes area.
- Keep one bounded implementation task active, record its goal IDs and acceptance evidence, and update the work record with what exists, what passed and the next task. Do not mark a phase complete based only on code/assets being present.
- Character art must follow the existing official reference catalog and the recognizable 2D Bluey style in the goal sheet and Toca/Piknik supplement. The user rejected the generic blue-pup workshop on September 25. Animation/compilation checks do not establish visual acceptance. Use Bluey and Bingo for the first character test, preserve editable layered sources, and do not replace the requested cast with generic lookalikes or describe generated drafts as approved.
- Keep platform builds, saved data, shared-world recovery and actual-device qualification in the sequence. The PC/VPS is the sole shared authority; keep automatic joining, four mixed clients and independent areas. Do not use the old project's connector as proof this new project is connected.

## Git delivery and project records

- The user wants Git maintained and completed work pushed as part of each project task. Check the working tree, current branch and `origin` before editing; preserve unrelated user changes.
- Keep changes focused and make descriptive commits explaining the resulting behavior and purpose. Add code comments for non-obvious decisions, constraints or workarounds; avoid comments that merely repeat the code.
- For implementation milestones, update the main build guide and relevant evidence with what changed, what actually passed, remaining limits and the next task. Documentation-only or repository-maintenance tasks need an appropriate record, not unrelated game tests.
- Review the diff and run checks appropriate to the change before committing. Stage only relevant files. Respect `.gitignore`, preserve tracked Git LFS assets, and never force-add private media, signing keys, credentials, generated builds or local saves.
- Push completed, checked commits to this private `origin` and verify the remote branch points to the intended commit. Keep `main` synchronized with completed work through a safe fast-forward or normal integration. Keep incomplete or failing work off `main`; identify it clearly if checkpointed on a development branch.
- Do not force-push, rewrite shared history, delete branches or discard changes as routine cleanup. Reconcile concurrent updates without overwriting them. Report a genuine push/authentication/conflict blocker instead of claiming the work is uploaded.
- End meaningful work with a clear status: what changed, relevant validation and any unpushed or unfinished work. This workflow applies while working on the project; it does not imply an unattended background sync service.

- When architecture changes, audit the goal sheet, phase/feature ledgers, return checklist, companion research and generated-page renderers. Run `Tools/Test-PlanConsistency.py` after rendering. Preserve dated evidence without treating its superseded instructions as current requirements.
