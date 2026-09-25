# Package review evidence — 23 September 2026

> Historical research notes. Follow [current decisions](../../current-decisions.md) and the current build guide for scope and installed versions; superseded device-host/merge proposals are not tasks. Original observations below are retained.

This directory contains research evidence, not installed Unity packages or game Assets.

- Official package JSON files record the Unity 6000.3 catalog response and designated release version. The combined summary was regenerated after the Multiplayer Play Mode / Tools checks.
- Registry files preserve requested version metadata, including dependency declarations. A registry latest tag is not evidence of device qualification.
- NGO 2.13.3 is labeled pre-release in the 6000.3 catalog; the registry and GitHub publish it. Unity 6000.3.24f1 release notes list the prior 2.13.2 update. The report explicitly leaves the final dependency set to qualification.
- Input System 1.20 changed documentation paths. The older Touch.html and OnScreen.html requests returned 404; the correct devices-touch.html and on-screen-controls.html pages were subsequently opened. The registry tarball was inspected in memory, and selected joystick source/documentation was saved here with hashes in input-doc-extraction.json. No tarball was imported or executed.
- GitHub metadata records the observed revision and latest release when available. No GitHub releases endpoint for a repository does not imply abandonment. NOASSERTION from the API is not a license determination.
- DOTween Pro listing showed $7.50 sale / $15 regular before taxes, version 1.0.430. The report budgets against $15. No purchase made.
- Existing Playground, Boss Room, Mirror, Chop Chop and grid-template source findings are carried forward from the main plan and prior deep-source-review records; this pass does not pretend to rerun all old inspections.
- Documentation integrity checks found no broken local file/fragment links or duplicate IDs across the four rendered reports. Browser rendering/navigation checks are recorded in ../package-review-validation-2026-09-23.json.

No Unity Editor/package installation, native build, device performance measurement, or game implementation took place in this research pass.
