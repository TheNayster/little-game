# Protected backups from the parent page

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**G3-OPS-07 · NET-02 / FAMILY-01 · September 25, 2026 · Windows tools; game build 91 unchanged.**

The parent control page now offers **Download protected backup** and **Check backup file**. This makes the existing portable recovery format usable without composing terminal commands. It advances the G1 backup gate and G3 everyday server operations while native Apple/iPad recovery qualification waits for availability.

## What the parent can do

1. Expand **Download a protected backup**, enter and repeat a passphrase, and download the encrypted family copy. The helper verifies the saved checkpoint and encrypted output before returning the file. Existing players can keep playing.
2. Move that download to a chosen separate drive or computer. Browser download completion alone is not proof of independent storage.
3. Expand **Check a backup file**, choose the copy from that location and enter its passphrase. The page reports the verified build, saved revision and four enrolled profiles for the selected family. This operation cannot restore, replace or start a world.

Passphrases need 16–1024 characters without surrounding spaces. The page clears its password fields when a request starts, when it finishes, and when the page is hidden. It does not save passphrases in its storage, URLs or access logs. The existing parent-session capability remains separate. Keep the passphrase separately; it is required to open the protected file.

The local helper retains an encrypted copy and the existing same-user backup so an interrupted download does not discard the source. Uploaded files are checked in memory without persisting their decrypted contents. This uses the already qualified AES-GCM/scrypt format; no new cryptographic format, credential rotation or restore path was introduced. [Portable format and recovery evidence](g3-portable-recovery-2026-09-25.html).

## Acceptance evidence

Six HTTP/native groups passed in a new isolated family with four Windows clients:

| Check | Accepted result |
| --- | --- |
| Live export | Binary download preserved the two-area world, all four players, object edits, receipts and enrollment while the same authority kept running. Attachment and no-store headers were present; protected fields were absent from ciphertext. |
| Read-only check | The uploaded encrypted bytes verified the expected family and returned only build/revision/profile count. Gameplay progress was unchanged while normal idle clocks continued; connected clients kept responding. An additional check with the writer stopped preserved the exact checkpoint bytes. |
| Invalid input | Wrong password, damaged/empty/malformed file, another family, short password and mismatched confirmation were refused. No restore occurred or secret-bearing error was returned. |
| Request boundary | Missing session, foreign origin, excessive body lengths, invalid JSON, GET mutations and restore routes were refused. Ordinary routes retain their existing small body limit. |
| Competing work | One password-derivation operation runs per helper at a time. Competing exports/checks fail promptly; status stays responsive. |
| Helper recreation | A new helper verified the downloaded copy using the supplied passphrase without interrupting the existing authority or players; read-only verification also passed with the authority stopped. |

The browser also produced an actual **25,428-byte** download. That exact downloaded file was selected through the upload picker and verified as **build 91, saved revision 9, four profiles**. A wrong-passphrase attempt showed the expected error, and all password fields were empty after both success and failure. All browser inputs used a synthetic test family and test phrase.

[HTTP/native results](evidence/parent-portable-2026-09-25/http-native.json) · [Browser results](evidence/parent-portable-2026-09-25/browser.json).

The existing **four-group parent operations regression passed**. [Regression results](evidence/parent-portable-2026-09-25/parent-operations-regression.json). Its scope includes real isolated crash recovery, retained clients, occupied-stop protection, deliberate-stop persistence and local-backup integrity.

A rerun exposed an overly strict live-save assertion: the authority writes advancing idle timers during ordinary play, so byte-for-byte comparison is inappropriate while it runs. The harness now compares live gameplay fields and separately proves exact-byte preservation after stopping only its test authority. [Initial rerun record](evidence/parent-portable-2026-09-25/initial-attempts.json).

## Limits and next steps

- These controls are prepared and tested. The existing real-family helper was not restarted or reconfigured. Old running helpers report the new controls unavailable until deliberately relaunched with the updated code; ordinary server controls continue to work.
- No actual family portable copy or private passphrase was created. Choose a separate destination when home, then export and check the real copy. A second-account/computer reconstruction test remains required.
- No physical device, game executable, family world, firewall rule, recovery preference or selected server build changed. The isolated test clients/helper/authority were closed after acceptance.
- Native Mac compilation/signing and **iPad 7 first** recovery checks remain G3-REC-05. iPad hosting, automatic host recovery, reconciliation and content remain on the goal sheet and ordered plan.

[Updated return checklist](return-checklist-ipad-lan-2026-09-24.html) · [Build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) · [Feature goals](../bluey-game-research-2026-09-23.html).
<!-- historical-record-end -->
