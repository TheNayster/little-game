# Six world code path map

Project root: `C:\Users\sephi\Desktop\Little weeps game`. Paths in this map and each world map are relative to that root; each link opens the actual file. Verified against source `62c6fa9` on October 2, 2026. Update paths when source changes. Daycare files are outside this phase.

## Separate world maps

- [Heeler Home](heeler-home/code-paths.md) — [audit](heeler-home/audit.md), [local plan](heeler-home/phase-plan.md), [findings](heeler-home/findings.md), [screenshots](heeler-home/screenshots/README.md).
- [Playground and Park](playground-park/code-paths.md) — [audit](playground-park/audit.md), [local plan](playground-park/phase-plan.md), [findings](playground-park/findings.md), [screenshots](playground-park/screenshots/README.md).
- [The Creek](creek/code-paths.md) — [audit](creek/audit.md), [local plan](creek/phase-plan.md), [findings](creek/findings.md), [screenshots](creek/screenshots/README.md).
- [The Beach](beach/code-paths.md) — [audit](beach/audit.md), [local plan](beach/phase-plan.md), [findings](beach/findings.md), [screenshots](beach/screenshots/README.md).
- [Zoo](zoo/code-paths.md) — [audit](zoo/audit.md), [local plan](zoo/phase-plan.md), [findings](zoo/findings.md), [screenshots](zoo/screenshots/README.md).
- [Dinosaur World](dinosaur-world/code-paths.md) — [audit](dinosaur-world/audit.md), [local plan](dinosaur-world/phase-plan.md), [findings](dinosaur-world/findings.md), [screenshots](dinosaur-world/screenshots/README.md).

## Shared menus and presentation

| Responsibility | Exact repository path |
| --- | --- |
| Application screen and common input | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloScreen.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloScreen.cs) |
| Worlds and character navigation | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloNavigation.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloNavigation.cs) |
| Games menu cards and scrolling | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloMiniGames.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloMiniGames.cs) |
| Scenery and framing | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloScenery.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/SoloScenery.cs) |
| Character visuals | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/GameCharacterVisual.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/GameCharacterVisual.cs) |
| Character motion | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/CharacterMotion.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/CharacterMotion.cs) |
| NPC presentation | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/NpcPresentation.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Client/NpcPresentation.cs) |
| World boundaries | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/WorldLayout.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/WorldLayout.cs) |
| Shared gameplay state | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/SoloWorld.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/SoloWorld.cs) |
| Authoritative client/server session | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenSession.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenSession.cs) |
| Native inspector and screenshot entry points | [Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenVerification.cs](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/NetworkGardenVerification.cs) |

## Capture and review tools

These files provide the isolated native release runtime, inspection, resize, input and PNG capture helpers. Read them to reuse a focused capture flow; do not run whole test suites just for pictures.

- [Tools/shared_garden_runtime.py](../../Tools/shared_garden_runtime.py)
- [Tools/Test-ScenicWorlds.py](../../Tools/Test-ScenicWorlds.py)
- [Tools/Test-HomeWorld.py](../../Tools/Test-HomeWorld.py)

Common menu pictures: [common/screenshots](common/screenshots/README.md). Verify symbols/line numbers against the reviewed source when assigning a finding; path presence alone does not establish the cause.
