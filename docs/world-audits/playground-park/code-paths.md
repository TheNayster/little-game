# Playground and Park code paths

Verified against source `62c6fa9` when this plan was created on October 2, 2026. Paths below are relative to the game project root, `C:\Users\sephi\Desktop\Little weeps game`. Click a path to open the exact file. Refresh the paths when source changes; the map is a starting point, not proof of a defect's cause.

Audit: [audit.md](audit.md). Local plan: [phase-plan.md](phase-plan.md). Shared menus/input/capture: [common code map](../code-path-map.md).

## Activity files

| Activity or concern | Client UI and presentation | Gameplay rules and shared state |
| --- | --- | --- |
| Tag | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloTag.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloTag.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ParkTag.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ParkTag.cs) |
| Equipment and seating | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloPark.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloPark.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ParkPlay.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ParkPlay.cs) |
| Wheeled play | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloWheels.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloWheels.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ParkWheels.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/ParkWheels.cs) |
| Balloon activity | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloKeepyUppy.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloKeepyUppy.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/KeepyUppy.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/KeepyUppy.cs) |

## Existing native helpers

These scripts show current fixtures and interaction/capture APIs. Reuse their focused helpers for review rather than running full acceptance suites merely to obtain pictures.

- [Tools/Test-ParkPlay.py](../../../Tools/Test-ParkPlay.py)

## Trace each finding

Record a finding ID, picture path, exact client file and relevant Core file, plus the actual symbol controlling the button, layout or behavior. Add a verified line number if useful. Check shared navigation/NPC/art files before assigning a local cause. Keep fixes linked to the finding and test the affected interaction only, adding one representative shared four-player check when shared behavior changes.
