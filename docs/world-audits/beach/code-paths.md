# The Beach code paths

Verified against source `62c6fa9` when this plan was created on October 2, 2026. Paths below are relative to the game project root, `C:\Users\sephi\Desktop\Little weeps game`. Click a path to open the exact file. Refresh the paths when source changes; the map is a starting point, not proof of a defect's cause.

Audit: [audit.md](audit.md). Local plan: [phase-plan.md](phase-plan.md). Shared menus/input/capture: [common code map](../code-path-map.md).

## Activity files

| Activity or concern | Client UI and presentation | Gameplay rules and shared state |
| --- | --- | --- |
| Wave activity | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloWaveRide.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Beach/GameScreen.WaveRide.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/BeachWaveRide.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/Beach/BeachWaveRide.cs) |
| Shore and visitor layouts | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloBeachShore.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Beach/GameScreen.BeachShore.cs)<br>[Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/BeachWaterGraphic.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Beach/BeachWaterGraphic.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/BeachShore.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/Beach/BeachShore.cs) |
| Seagull behavior | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloSeagulls.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/Worlds/Beach/GameScreen.Seagulls.cs) | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/BeachSeagulls.cs](../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Worlds/Beach/BeachSeagulls.cs) |

## Existing native helpers

These scripts show current fixtures and interaction/capture APIs. Reuse their focused helpers for review rather than running full acceptance suites merely to obtain pictures.

Use the [shared native capture helpers](../code-path-map.md#capture-and-review-tools) and the world files above. Do not assume an absent dedicated test script is a capture blocker.

## Trace each finding

Record a finding ID, picture path, exact client file and relevant Core file, plus the actual symbol controlling the button, layout or behavior. Add a verified line number if useful. Check shared navigation/NPC/art files before assigning a local cause. Keep fixes linked to the finding and test the affected interaction only, adding one representative shared four-player check when shared behavior changes.
