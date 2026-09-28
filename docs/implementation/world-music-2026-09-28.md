# Music for every world — September 28

The user requested an update to the Samsung phone and pleasant Bluey-style background music throughout the game. This slice addresses G-16/G-17 (music presentation, local settings and speech ducking). It changes client presentation only: schema 27/content 28 and the authoritative simulation remain unchanged from 216. Preserve the main integration hold and the complete Home backlog.

## Research applied

| Source | Application |
| --- | --- |
| [Official Bluey composer interview](https://www.bluey.tv/blog/qa-with-joff/) | Music follows the story's mood and can develop through different instrumentation. Compose six original acoustic pieces with an opening, answering phrase, quieter contrast and return; use piano, woodwind, guitar, pizzicato/soft strings and gentle percussion. |
| [Unity 6.3 audio import settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioClip.html) | Long music uses streaming Vorbis with background loading, rather than decompressing six complete scores into memory. Retain at most two clips for a 1.8-second crossfade and coalesce rapid travel. |
| [GeneralUser GS author repository and license](https://github.com/mrbumpy409/GeneralUser-GS/tree/684543d5e5efaef08d02be50dcda8d552478fa60) | Locally render sampled acoustic instruments; retain the license, source MIDI, arrangement script and pinned tool/font hashes. The license permits music production and records the author's sample-provenance qualifications. |
| [Official FluidSynth 2.6.1 release](https://github.com/FluidSynth/fluidsynth/releases/tag/v2.6.1) | Use an isolated portable offline renderer. The shipped app plays bundled audio and needs neither the renderer nor a network service. |

These are original melodies and recordings; no Bluey score or recording is used. Research provides a broad presentation direction, not evidence of family listening acceptance.

## Result

- Home: **Little Everyday Adventures**, 77 seconds, piano/clarinet.
- Backyard: **Sunshine in the Grass**, 69 seconds, guitar/vibraphone.
- Park: **Follow the Little Path**, 66 seconds, clarinet/pizzicato strings.
- Creek: **Pebbles and Ripples**, 89 seconds, flute/piano.
- Beach: **Seashell Skipping**, 74 seconds, guitar/flute.
- Daycare: **A Pocket Full of Ideas**, 71 seconds, piano/marimba.

Home and the yard change music across the veranda with a boundary margin to avoid repeated changes when stepping back and forth. Upstairs/bedrooms use softer Home music. Secret rooms retain their existing independent quiet ambience and levels. Radio music/dancing continues to work; the general score recedes while a nearby radio plays. Book narration/effects and spoken hints lower the local music. Music off applies immediately to the new score and remains a local saved preference. Pausing backgrounds the audio; resuming continues it. One player's travel, mute or reading does not change anyone else's music or shared data.

Source and regeneration information: [World music sources](../../SourceAudio/WorldMusic/README.md). Six stereo masters are 66–89 seconds, with matched −22 dBFS RMS before local playback gain, no sample clipping and zero sample discontinuity at their loop boundaries. Musical/listening quality still needs the family's feedback.

## Verification and delivery

Candidate **217** Windows client/server and signed Android release builds succeeded with zero build errors. [Source/audio checks](evidence/world-music217-2026-09-28/source-audio-checks.json) match the current code and music against both release manifests and verify all six streaming imports and master hashes. Plan consistency passes 75 active documents, 48 historical records, 120 pages and 3,581 local links. [Five native groups](evidence/world-music217-2026-09-28/native.json) pass: six decoded tracks with advancing audio cursors/nonzero signals, four-player independence, saved local mute, travel/cache bounds, book ducking, radio priority, pause/resume and bedroom/secret-room behavior. [Private solo](evidence/world-music217-2026-09-28/solo.json) confirms audible decoded data, saved mute after cold reopen and exactly retained objects, rooms, players and creations. The first native run needed camera-settling time before a radio tap; the corrected physical-touch test passes without changing game code. These are runtime signal checks, not human listening approval.

Samsung was updated **200 → 216** in place with exact installed-artifact and signing verification. The unlocked kitchen visibly resumed with Bingo and saved food/fixtures. [Retention evidence](evidence/world-music217-2026-09-28/android216-update.json) compares four accessible primary records and four backups: prior fields are retained, including all 118 active objects, rooms and food; the active world upgrades 22→27 with additive content, ordinary idle-clock advancement and negligible floating serialization rounding. Full application-directory inventory was interrupted, so the earlier 21-save count is not claimed as freshly verified.

The phone disconnected before installing music. **217 is ready but not installed**; the supplied wireless address timed out and a fresh connection/address has been requested. Server/iPads remain last recorded at 171, iPhone 101; none were contacted or updated. Build 216 on the phone is private solo against that older recorded server version.

The existing production recovery-validator block, older A10 performance/audio qualification, Android 16 KB qualification and final family listening acceptance remain open. No security policy was altered and the blocked recovery tool was not retried.
