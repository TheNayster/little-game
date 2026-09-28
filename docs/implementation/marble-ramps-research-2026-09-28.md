# Marble ramps: applied research and implementation contract

September 28, 2026. SCI-03 / SP-03 / LAB-01, the next bounded activity after the five meal paths. Preserve the full Home and science backlog.

## Primary sources and application

- [Exploratorium's Marble Machines](https://www.exploratorium.edu/tinkering/projects/marble-machines) describes construction, testing and revision with tracks and a catch. Its [educator guide](https://www.exploratorium.edu/sites/default/files/tinkering/files/Instructions/marblemachines_activity-guide.pdf), especially pages 1–5, adds adjustable angles, slowing materials, reusable pieces and individual workspaces. Apply three adjustable ramps, visible stops, a forgiving catch, repeated releases and four owned courses. Use an immediately working course, without a required score or time target. Additional tunnels, elevators and connected family machines remain later work.
- [Tinybop Simple Machines](https://tinybop.com/apps/simple-machines) presents changes to a machine as direct experiments with visible consequences. Apply large manipulable endpoints and immediate rolling feedback. Its [inclined-plane activity](https://tinybop.com/blog/experiment-with-simple-machines-make-an-inclined-plane) supports changing ramp height; this game explores rolling motion rather than teaching measured lifting effort.
- [PBS LearningMedia's materials/friction center](https://static.pbslearningmedia.org/media/media_files/d3ea4e3b-7107-4aaf-9e2b-f8dd56b77fd0/51cc8c8a-9604-4758-85a0-ac3f07167e7f.pdf) compares equal ramps with different surface materials. Keep the same geometry and release point while switching smooth, felt and ribbed surfaces, so the observed difference comes from resistance. Texture and words accompany color.
- The existing [accepted picture-control research](home-picture-controls-2026-09-28.html) and [preschool prototype flow](science-kids-flow-2026-09-28.html) govern this screen: Roll, Surface, Keep course; optional tools under More to try; drag or tap-then-place handles; Undo and Home in stable positions. Publisher material informs design, not a claim of purchased-app testing or physical child acceptance.

## Systems and acceptance

Use the three-ramp browser prototype as a tested starting geometry, then qualify a bounded fixed-step simulation against the actual edited segments and visible stops. Rendering must use that same trajectory; no canned success path. Cache trajectories by bounded course values so older iPads do not simulate the entire run each frame. Persist course, elapsed release, undo and one deliberately kept course per profile. Server ticks finish rolling even when a player leaves; clients only draw the matching trajectory.

Schema 27/content 28 appends four ramp workspaces without altering old food, pictures, rooms or enrollment. Owner/revision tokens reject foreign and stale edits. A release has one marble; retrying a command cannot create another. Coordinate validation excludes degenerate tracks. Partial motion survives reopen. Keeping or updating a course is explicit; reset and five-minute temporary cleanup preserve it. Cleanup can return the workspace to its kept course, with Undo for the previous temporary layout. Existing cleanup cue identities must remain correct during migration.

Acceptance: a working starter path, changed slopes changing trajectories, surface-only comparisons, bounded arbitrary edits, fixed-step timing, four independent releases/departures, reset/undo/kept courses, five-minute cleanup protection, old-save migration, reliable-message budget, private-solo reopen, native actual drags and phone/tablet layouts. Independent production recovery remains separately blocked by the existing Windows policy; no security or device changes are authorized by this slice.

## Implementation evidence

The [native implementation report](marble-ramps-2026-09-28.html) records the applied controls, layered artwork, bounded shared simulation, save migration, cleanup behavior and actual checks. It also records the Unity JSON finish-time rounding failure found during native testing and its correction; portable C# tests alone did not expose that serializer difference.
