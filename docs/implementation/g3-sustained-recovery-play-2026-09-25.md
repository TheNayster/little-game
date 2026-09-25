# Sustained play with recovery build 91

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**G3-REC-07 · NET-02 / FAMILY-01 / ITEM-03 · September 25, 2026.**

**Status: five acceptance groups passed within the emulator/Windows scope below.** Prepared game build 91 and the live family deployment are unchanged.

The prior ten-minute sustained run used build 85, before full recovery checkpoint replication and separate outage adventures. This task exercises the prepared build 91 with the retained Android release emulator and three native Windows clients against its isolated PC authority. It keeps the same four identities and does not modify the actual family world, installed phones/iPads or game source.

## What this run measures

- Repeated movement by all four players, with one Windows player changing independently between Garden and Creek.
- Complete Android recovery checkpoints with 128 retained receipts, including ongoing item clocks and offscreen-area state. The test requires continued durable progress rather than merely the presence of an old save.
- The completed flower and borrowed bucket using their normal runtime timers, with no accelerated clock.
- Actual Android Home/background and warm return during the run, while the other three players remain connected to the same authority.
- Remote Windows canvas interpolation before and after sustained recovery traffic, using the existing motion thresholds.
- Windows private memory and Android emulator PSS sampled separately. The diagnostic smoke thresholds are below 1 GiB per process and below 256 MiB growth after the first minute; these do not replace the A10's provisional mobile budget.
- A final durable replica covering the completed shared revision; original solo/adventure files byte-identical from confirmed shared presentation; exact stopped-server checkpoint preservation through an empty restart.

## Measured result

**All five acceptance groups passed** in the corrected run: 600.06 real seconds, 75 cycles and four players on the same authority. The release APK hash remains `1e8f7af73d04dcd35eb3dd8fdd6f5527d26e101bec9165c5bd7de1f79eceef4f`.

| Check | Observed result |
| --- | --- |
| Durable Android recovery | 75 distinct sampled checkpoints, 128 receipts each; maximum observed gap 14.84s; final decoded replica covers the completed shared revision |
| Normal resets | Flower observed reset at 65.73s; borrowed bucket at 185.00s. Samples occur about eight seconds apart, so these observations are not exact configured trigger times. Rules remain 60 + 5 seconds / 180 + 5 seconds. |
| Remote Windows motion before / after | Median sample interval 16.68 / 16.69ms; moving on 100.0% / 100.0% of sampled motion frames; 95th-percentile lag 0.229 / 0.213s |
| Android lifecycle | Ordinary Home, observed departure, same-process warm return and four-player reunion; other three clients stay connected |
| Local save preservation | Seven files (two solo saves, five adventures) byte-identical between fresh confirmed shared admission and the end |
| Empty authority restart | Stopped checkpoint bytes remain exact after restarting without players |
| Android diagnostics | Final screenshot inspected; selected process log has no tested fatal/ANR/OOM signatures |

| Memory measure | Peak MiB | Growth after first minute, MiB |
| --- | ---: | ---: |
| server, private bytes | 188.25 | +1.47 |
| windows-1, private bytes | 709.12 | +0.05 |
| windows-2, private bytes | 709.20 | -0.13 |
| windows-3, private bytes | 709.93 | -0.07 |
| Android emulator, PSS | 475.99 | +12.39 |

These are different platform memory measures, not interchangeable device budgets or a leak-free guarantee. The run uses normal local networking without injected packet loss.

[Machine result](evidence/sustained-recovery-2026-09-25/result.json) · [Sampled metrics](evidence/sustained-recovery-2026-09-25/samples.json) · [Android memory samples](evidence/sustained-recovery-2026-09-25/android-memory.json) · [Source provenance](evidence/sustained-recovery-2026-09-25/source-provenance.json) · [Final emulator screenshot](evidence/sustained-recovery-2026-09-25/android-final.png).

## Save-baseline correction and retained evidence

The [first timed attempt](evidence/sustained-recovery-2026-09-25/first-timed-result.json) completed 600.05 seconds and passed movement/rejoin, normal resets, interpolation and diagnostic memory checks. Its final local-save assertion failed, so that attempt is **not a complete acceptance pass**. It had compared files captured before cold launch with files after shared play. The original pre-launch bytes were not retained, so the exact original delta cannot be reconstructed. A selected adventure's current/backup records differed only in idle timers; this is consistent with local startup progress, but does not conclusively reconstruct that failure.

The game intentionally opens saved local play while connecting. `SoloScreen.Update` advances that local world's idle timers, and `TryJoinFamily` saves it before switching to shared presentation. Once shared, the update returns before local timer processing. Requiring a local save to be frozen across that initial offline period would test the wrong behavior.

The corrected harness retains startup, joined and final records locally, waits for a **fresh Android process and its shared-presented event**, and then requires exact joined/final byte equality. It records startup differences separately. A [focused fresh-session check](evidence/sustained-recovery-2026-09-25/focused-save-check.json) preserved all seven local saves through ordinary shared taps and Home/warm return. [Verification history](evidence/sustained-recovery-2026-09-25/verification-history.json) records a scratch probe that initially read stale session evidence. An [abandoned decoder setup](evidence/sustained-recovery-2026-09-25/initial-setup.json) is also retained. Neither is counted as acceptance. No game-source change was made to obtain these results.

The full corrected ten-minute run passed as recorded above. Startup changes in that run are recorded separately in the machine result, rather than counted as shared-world overwrites. Raw local world files and credentials stay out of Git; the report retains measured summaries and non-secret evidence only.

## Scope and remaining device work

The Android environment is API 35 / Android 15, 16 KB pages on an x86_64 emulator with ARM translation. It is running the existing family-signed build 91 APK, verified by reading the installed APK back and comparing its hash. Ordinary ADB touch/Home/launch input is used; the Android app has no added verification hooks. Windows clients use the existing test input harness.

This is one Windows PC and its local emulator network. It cannot establish physical Wi-Fi behavior, real multitouch/audio, thermal behavior, A10 frame-time/memory, native ARM64 16 KB compatibility, iPad hosting or an hours-long uptime guarantee. Window interpolation samples are short measurements around the run, not an Android whole-session frame-time distribution. The existing Android strict RELRO issue remains open.

Native Mac compilation/signing and **iPad 7 first** recovery/performance checks remain G3-REC-05. Complete the real-device matrix and controlled server deployment before closing G3; both iPads hosting and automatic recovery, save reconciliation and the complete content goal sheet stay in G4–G7.

[Return checklist](return-checklist-ipad-lan-2026-09-24.html) · [Current plan](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) · [Earlier short Android recovery checks](g3-android-recovery-2026-09-25.html) · [Historical build 85 sustained run](g3-server-soak-2026-09-24.html).
<!-- historical-record-end -->
