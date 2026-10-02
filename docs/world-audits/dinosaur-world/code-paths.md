# Dinosaur World code paths

Verified against source `62c6fa9` when this plan was created on October 2, 2026. Paths below are relative to the game project root, `C:\Users\sephi\Desktop\Little weeps game`. Click a path to open the exact file. Refresh the paths when source changes; the map is a starting point, not proof of a defect's cause.

Audit: [audit.md](audit.md). Local plan: [phase-plan.md](phase-plan.md). Shared menus/input/capture: [common code map](../code-path-map.md).

## Activity files

| Activity or concern | Client UI and presentation | Gameplay rules and shared state |
| --- | --- | --- |
| Riding, calls and movement | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloDinosaurWorld.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloDinosaurWorld.cs)<br>[Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/DinosaurLandmarks.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/DinosaurLandmarks.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/DinosaurWorld.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/DinosaurWorld.cs) |
| Feeding and petting | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloDinosaurCare.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloDinosaurCare.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/DinosaurCare.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/DinosaurCare.cs) |

## Existing native helpers

These scripts show current fixtures and interaction/capture APIs. Reuse their focused helpers for review rather than running full acceptance suites merely to obtain pictures.

- [Tools/Test-DinosaurWorld.py](../../../Tools/Test-DinosaurWorld.py)
- [Tools/Test-DinosaurCare.py](../../../Tools/Test-DinosaurCare.py)

## Trace each finding

Record a finding ID, picture path, exact client file and relevant Core file, plus the actual symbol controlling the button, layout or behavior. Add a verified line number if useful. Check shared navigation/NPC/art files before assigning a local cause. Keep fixes linked to the finding and test the affected interaction only, adding one representative shared four-player check when shared behavior changes.
