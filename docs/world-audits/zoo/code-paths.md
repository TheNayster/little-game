# Zoo code paths

Verified against source `62c6fa9` when this plan was created on October 2, 2026. Paths below are relative to the game project root, `C:\Users\sephi\Desktop\Little weeps game`. Click a path to open the exact file. Refresh the paths when source changes; the map is a starting point, not proof of a defect's cause.

Audit: [audit.md](audit.md). Local plan: [phase-plan.md](phase-plan.md). Shared menus/input/capture: [common code map](../code-path-map.md).

## Activity files

| Activity or concern | Client UI and presentation | Gameplay rules and shared state |
| --- | --- | --- |
| Exhibits, feeding and animal routines | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloZoo.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Zoo/GameScreen.Zoo.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ZooWorld.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/Zoo/ZooWorld.cs)<br>[Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ZooSpecies.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/Zoo/ZooSpecies.cs) |
| Animal audio | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloZooAudio.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Zoo/GameScreen.ZooAudio.cs) | Presentation only; trace relevant current activity state |

## Existing native helpers

These scripts show current fixtures and interaction/capture APIs. Reuse their focused helpers for review rather than running full acceptance suites merely to obtain pictures.

- [Tools/Test-ZooWorld.py](../../../Tools/Verification/Test-ZooWorld.py)
- [Tools/Test-ZooSpecies.py](../../../Tools/Verification/Test-ZooSpecies.py)

## Trace each finding

Record a finding ID, picture path, exact client file and relevant Core file, plus the actual symbol controlling the button, layout or behavior. Add a verified line number if useful. Check shared navigation/NPC/art files before assigning a local cause. Keep fixes linked to the finding and test the affected interaction only, adding one representative shared four-player check when shared behavior changes.
