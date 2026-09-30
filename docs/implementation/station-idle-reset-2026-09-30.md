# Coloring, science and reader inactivity — September 30

Scope: **COL-02 / COL-05, LAB-01 / SCI-01–03 / SCI-06 / SCI-09, BOOK-01 / H-19 / H-20**. The user requests reusable coloring/science stations and books opening on their first page after five unused minutes.

The previous cleanup explicitly excluded every coloring page as a personal creation. Science eligibility also omitted settings-only experiments, such as choosing bubble tools without adding water. Book return-to-rack cleanup did not affect the local reader bookmark. These are separate clocks and records, not a room reset.

## Result

- Each of eighteen working coloring pages per profile gets its own five-minute inactivity clock, then the existing five-second cue. Cleanup blanks its colors and undo/redo and advances its revision to reject late fills. Coloring, choosing a crayon, selecting/reopening that page and keeping a picture refresh only that page.
- Deliberately kept/displayed pictures are independent stored copies and remain exact. Bedrooms, food and kept ramp layouts remain protected. This supersedes the older blanket protection for all working papers; it does not erase the personal picture folder.
- Science cleanup also covers temporary bubble-tool choices, dinosaur selection and empty volcano vessels, restoring fresh defaults. Existing experiment isolation and runtime reaction protection remain.
- All six books save a last-use time alongside each existing local profile/world/book bookmark. Five unused minutes resets page and audio sample to zero. Expiry is checked when opening and while a reader sits idle. Page turns, explicit reading/name actions and actual audio playback count as use. Periodic autosaving and app background/focus/quit do not renew the clock. Recently used bookmarks and reading preferences remain; reopening an expired book starts quietly on page one. Legacy bookmarks without a use timestamp also start at page one.

## Verification and delivery boundary

The focused core changes pass alongside the existing checks: **310 groups** including all eighteen pages, cue timing, stale-page rejection, four connected artists with independent departure, exact kept/displayed picture preservation, clock serialization/reopen and science settings-only resets. Windows client/server candidate **236** is built from the isolated `codex/station-idle-reset` worktree. Native real-time four-client results are recorded in the evidence folder linked below.

Candidate 235's first native pass was interrupted when another foreground window closed a hidden test overlay. The harness now reopens only the deliberately active sibling; backgrounding also stops renewing reader inactivity. It did not indicate a transport or timer failure.

Shared cleanup introduces timer keys and rules: **schema 35 / content 36**, protocol 3. Schema 33 is reserved for concurrent Park work and 34 for concurrent Zoo work. This isolated build contains neither unfinished activity. On integration, apply existing Park/Zoo migration before `WithStationTidying`; never advance an older world past their schemas without populating their required state. Preserve the main integration hold. No production server, device, save or enrollment is changed by this task. Shared rollout requires coordinated compatible server/apps under [the rollout policy](../server-update-policy.md).

Next: integrate this checked branch with the concurrent activities, using the migration order above, then build/install the requested current release when the user asks. Keep the remaining Home backlog.
