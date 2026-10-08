# Elephant feeding clarity — October 8, 2026

Scope: WORLD-02 / FAMILY-01 / ITEM-02. First elephant-only presentation and feedback pass; stop for owner review before extending the design to other animals.

## Reproduction and implementation

Enter Zoo → Savanna → tap the elephant's leaves. Automatic bucket walking, collection, reserved-spot walking and offering remain. Two close offers reproduce the old defect: the owner sees “Coming for your food” during Eat because the bucket sign tested only `animal.owner`, without checking `phase` or `consumed`.

The elephant now has four quiet ground trays aligned with the existing reserved player positions. Active trays have the actual player's menu portrait; the local tray additionally has a gold footprint, pointer and YOU label, and consumed food gets a green picture tick. Unused spots remain quiet. Leaves move from carrying to the reserved tray, then into the original trunk socket during authoritative Eat age before consumption. Other players' waiting portions remain visible.

Local cues distinguish collecting, carrying, waiting, approaching, eating and finished using the actual local food lease and animal ownership/phase/consumption. Rejection text identifies the supported reason and clears on retry/success or area departure. The existing `fed` counter gates a brief curled-trunk satisfied sway once per observed consumption. Initial views, reconnect and lifecycle changes seed observation without replay. The motion follows authoritative Eat age and does not extend the turn.

No Core rules, offering tickets/positions, navigation anchors, saved fields or audio assets changed. Existing sound remains; no new finish sound. Protocol 3 / content 72 / schema 53 remain. Existing artwork and menu portraits are reused; there is no new raster/source art.

## Changed files

- `Code/Client/Worlds/Zoo/GameScreen.Zoo.cs`: elephant trays/portraits, food handoff, state cues, finish observation and reason-specific rejection feedback.
- `Code/Client/Shared/Sessions/GameScreen.cs`: seed elephant observation across pause/resume and connection changes.
- `Code/Networking/FamilyGameVerification.cs`: actual cue/slot/completion diagnostics for native acceptance.
- `Code/Editor/ZooJsonTests.cs`: explicit unfinished-offer restore/feeding-history assertion in the standard Unity gate.
- `Tools/Verification/Test-ElephantFeeding.py` and `Test-ElephantFeedingSolo.py`: repeatable isolated native acceptance.
- `Tools/Verification/ZooRules.Tests/Program.cs`: preserve the random-route assertion while observing several bounded cycles instead of a seed-sensitive 80-second window.
- `ProjectSettings/ProjectSettings.asset`: version set by the established build entry point.

Paths beginning with `Code` and `ProjectSettings` are beneath `Unity/FamilyPlayset/Assets/FamilyPlayset` and `Unity/FamilyPlayset`, respectively.

## Evidence and acceptance

Baseline 471: fresh standard server/client build, zero errors/warnings; 2,335 Unity inputs matched before capture. Actual 1280×591 phone and 1024×768 tablet captures are in `LocalData/ElephantFeeding/before`. The tablet eating capture directly reproduces the stale prompt.

Final candidate 475: fresh standard server/client build, zero errors/warnings; all 2,335 Unity inputs match the artifact manifest. Unity Zoo JSON checks include four exact consumptions, migration, retention and unfinished-offer clearing. Final trays/portraits align with reserved spots; lifted leaves pass behind children rather than covering faces. The trunk endpoint is preserved.

Native shared acceptance: one disposable loopback authority and four actual release clients pass two close offers; correct per-client carrying/waiting/approach/eating/finish cues; repeated collection, walking, waiting, approach and eating taps; four distinct offers completing exactly once; active lease preservation followed by offered-ticket ordering; queued and active departures; network suspension/rejoin without replay or duplicated food; pause/resume; and giraffe feeding. No runtime exceptions or warning lines were found in the final native logs. The harness compares each cue with that client's received snapshot and waits for the camera after fixture placement; fixture setup is distinct from the actual Touchscreen inputs used for feeding.

Private acceptance: one actual offline release client passes automatic feeding through completion/normal routine, pause/resume, world switching/return and a real saved-world process reopening. Unfinished offers clear on restore; the completed feeding count remains, with no completion replay. No authority process or real enrollment/save was used.

Actual evidence is preserved in the established ignored `LocalData` locations:

- [Before/after review gallery](../../LocalData/ElephantFeeding/review.html): matching phone/tablet viewport sizes, four offers and finish.
- [Phone before](../../LocalData/ElephantFeeding/before/phone-eating.png) · [phone after](../../LocalData/ElephantFeeding/after475/phone-eating.png).
- [Tablet before](../../LocalData/ElephantFeeding/before/tablet-eating.png) · [tablet after](../../LocalData/ElephantFeeding/after475/tablet-eating.png) · [finish](../../LocalData/ElephantFeeding/after475/tablet-finish.png).
- [Actual 44.96-second silent recording](../../LocalData/ElephantFeeding/after475/actual-elephant-silent.mp4): original real-time frame timestamps; video padded by one pixel to 1280×592 for H.264. Collection/placement, waiting, approach/eating, finish and the next turn are included. Full-view captures and timestamped gameplay frames were inspected; owner normal-speed motion/appearance review remains pending.
- [Shared results](../../LocalData/ElephantFeeding/after475/results.json) · [private results](../../LocalData/ElephantFeeding/after475/solo-results.json) · [source check](../../LocalData/ElephantFeeding/after475/source-check.json).

Original shared run: `LocalData/SharedGarden/487457cbb16e4d8982699c6cba6680ab`; private run: `LocalData/FamilyLAN/ab037c9f3d6e43e58d320fc2ec2625cd`. Keep these native logs, raw film frames and synthetic recovery inputs as referenced review evidence.

Existing standalone .NET Zoo test first hit its seed-sensitive random-route assertion. After extending its observation, Windows Application Control blocked the generated test assembly (`0x800711C7`), including one bounded retry; no security setting was changed. Native Unity acceptance covers the affected contracts, and the standalone executable is not reported as passing. Rare server-limit/rejection text branches were mapped from the actual outcomes but not all forced in native play.

Physical phone/tablet installs, live-server replacement, subjective audio approval and owner visual acceptance are outside this pass. The 15 other exhibits retain their existing presentation. Next task: owner review of the actual captures/recording; no automatic expansion.
