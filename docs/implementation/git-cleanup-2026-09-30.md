# Git cleanup — September 30, 2026

The user requires completed, checked work to be integrated and pushed into main, then its task branch deleted both locally and on GitHub. Use main directly for small isolated work in a clean checkout; temporary branches need an actual unfinished/concurrent task. This supersedes historical source-integration holds and the previous blanket prohibition on deleting branches. It does not authorize device/server deployment.

## Verified result

- GitHub branches: 72 before, 9 after; 63 obsolete branch refs deleted with exact-tip guards against concurrent changes.
- The previously checked combined world release360 source, deb255f, was fast-forwarded into main. Its existing Windows client/server, JSON migration, four-client and copied-save evidence is recorded in [the combined release record](combined-world-release-360-2026-09-30.md). No gameplay was changed or rebuilt by this maintenance task.
- 56 deleted remote tips are ancestors of the combined source. Seven older separate task histories remain recoverable as annotated archive/2026-09-30 tags: beach-seagulls, creek-boat-smoothing, creek-boats, creek-fishing, daycare-adventure, park-tag and saved-bedrooms. The separate checkpoint/doc histories were preserved rather than pretending cherry-picked source creates Git ancestry. A complete pre-cleanup Git bundle and ref inventory are retained locally under ignored LocalData/GitMaintenance/2026-09-30; this bundle contains Git objects, not private saved worlds or an independent copy of LFS media.
- GitHub automatic deletion of branches after PR merges is enabled. Local fetch pruning is enabled. The remote was corrected to https://github.com/TheNayster/little-game.git after GitHub reported the repository rename and its main commit was verified.
- Standing instructions were updated in the canonical AGENTS.md, the old shared chat entry point and existing checkout instruction files. Current decisions explicitly supersede dated integration holds. Unrelated source edits, saves, builds and active worktrees were preserved.

## Remaining task branches

Eight remote branches are retained for existing work checkouts. Most have uncommitted changes; this cleanup does not switch or erase another task's checkout. Their already-combined gameplay is present in main, while separate task history/current edits remain available:

| Branch | Existing checkout / reason |
| --- | --- |
| codex/home-science-coloring | Primary project checkout with extensive uncommitted concurrent work. |
| codex/station-idle-reset | Station reset checkout with uncommitted changes. |
| codex/beach-waves | Beach checkout with uncommitted changes. |
| codex/daycare-calypso | Daycare checkout with uncommitted changes. |
| codex/daycare-adventure-repair | Pushed repair tip; AdventureRepair checkout uses the matching local daycare-story-encounters branch with uncommitted changes. |
| codex/dinosaur-world | Combined delivery checkout; device/server delivery is a separate ongoing task. |
| codex/park-tag-group | Existing park checkout retained without changing its checked-out branch. |
| codex/zoo-all-species | Zoo checkout with uncommitted changes. |

Close each task by integrating its final checked work into current main, verifying origin/main, safely moving its clean checkout to main, then deleting its task branch locally and remotely. Avoid creating another branch for a follow-up that belongs to the same task. A genuine conflict/failure can keep an unfinished branch; report that specific reason.

Local branch refs removed: 67. Remaining local branches: 10, including the AdventureRepair checkout alias.
