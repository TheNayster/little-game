# G1 — paused-video progress stability

TV-01 / FAMILY-01. Windows-only work, 23 September 2026 local time. No Mac, mobile device, signing or wireless changes.

## Result

Windows build **0.0.22** fixes an independently reproduced bookmark defect. Reopening or checkpointing a paused video now preserves its logical saved position until playback or an explicit seek changes it. Pause during a seek cancels subsequent playback while allowing the requested frame to finish decoding.

The controller previously treated temporary decoder playback during seek as progress. With the unchanged controller in build 21, seeking to 4.013s then checkpointing while paused replaced **4.0130000114s with 4.0000000000s**, without Play. [Failing regression](evidence/windows-bookmark-2026-09-23/before-fix.json). This explains a reproducible Windows defect; it does not establish the cause of the earlier iPhone 10.8-to-11.1 report.

`FoundationVideo` now tracks explicit playback intent separately from the decoder's `isPlaying` state. Idle checkpoints do not rewrite the bookmark. Completed seeks save their requested logical position; active playback still checkpoints normally. Existing decoder-seek tolerances were not loosened.

## Verification

- Fresh non-development Windows 22 build: zero reported errors/warnings. [Build record](evidence/windows-bookmark-2026-09-23/build-summary.json), [source hashes](evidence/windows-bookmark-2026-09-23/source-manifest.json).
- **60 paused reopenings** over two player processes: exact bookmark equality, zero drift, same profile and three test taps. [First 30](evidence/windows-bookmark-2026-09-23/fixed-reopens.json), [relaunch and next 30](evidence/windows-bookmark-2026-09-23/fixed-relaunch.json).
- Four seek interruptions: direct Pause and simulated Unity application-suspension callback in each process. No unintended autoplay or stalled seek; Start Over still saves zero. These are Windows controller/lifecycle checks, not a real mobile suspension test.
- Existing native **19 → 22** seed/restart/update suite passed: tap/profile retention, moving frames, pause, seek and bookmark restoration. [Seed](evidence/windows-bookmark-2026-09-23/update-seed.json), [restart](evidence/windows-bookmark-2026-09-23/update-resume.json), [update](evidence/windows-bookmark-2026-09-23/update-result.json).
- Every test used a fresh `foundation.verify.<GUID>` preference namespace; normal family saves were not reset or modified.

The desktop `Play-Foundation.cmd` now selects verified build 22. This remains the technical foundation fixture. Mobile retesting, older iPad qualification, external recovery backup and unattended signing renewal remain pending. G1 is not marked complete.

## Repeat

```powershell
.\Tools\Test-WindowsBookmark.ps1 -BuildNumber 22
.\Tools\Test-WindowsFoundation.ps1 -FirstBuild 19 -UpdatedBuild 22
```

Unity describes `VideoPlayer.time` as presentation time of the currently available frame. That supports keeping it distinct from the intended logical seek bookmark. [Unity time API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer-time.html), [frame-ready callback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer-frameReady.html), [Pause](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Video.VideoPlayer.Pause.html).

## Next bounded work

The user explicitly requested continued Windows-only progress while sleeping, including after the available foundation checks. Begin a separate G2 solo-rules prototype with placeholder graphics and isolated saves. This is provisional Windows development alongside outstanding G1 device qualification, not a G1/G2 gate pass or permission to produce final content. Preserve the unchanged G1 scene and mobile installations.
