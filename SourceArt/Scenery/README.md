# Scenic world panoramas

SCENIC-01 builds the user's six long, walkable destinations before adding more activities. The twelve 3:1 PNGs are newly generated production backgrounds, guided by the user's Bluey: Let's Play! screenshots and the maintained [reference study](../../docs/bluey-lets-play-reference-study-2026-09-25.md). They were made with the built-in image-generation tool. They are implementation drafts awaiting the user's visual review, not official game assets or finished interaction layers.

[manifest.json](manifest.json) records each exact prompt and original generated file. The imported copies live in `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Scenery/`. Source files are 2172 × 724; runtime panels keep a 3:1 composition, with a narrow overlap to soften section joins. No source screenshot was imported as a playable background. One defective beach output was rejected and regenerated before integration.

| Destination | Long scenic sections | Persistent area |
| --- | --- | --- |
| Home | Living/reading room and stairs; kitchen/dining/veranda | `garden`, x −4800 through 0 |
| Backyard Garden | Veranda/tree/trampoline; far lawn, fruit tree and shed | `garden`, x 0 through 4800 |
| Playground & Park | Slide/swings; picnic lawn and lookout | `park`, x 0 through 4800 |
| Creek | Bush bank; stepping stones and gum-tree pool | `creek`, x 0 through 4800 |
| Beach | Dunes and shore; rock pools | `beach`, x 0 through 4800 |
| Daycare | Timber playroom; outdoor cubby garden | `daycare`, x 0 through 4800 |

The foreground is kept open for separately rendered Bluey/Bingo characters and existing props. Furniture, shed doors, trampolines and other pictured equipment are scenery in this milestone. Sitting, jumping, dancing, containers, room branches, personal bedrooms and house interaction layers remain in the goal sheet. Existing Garden/Creek watering and cleanup rules remain available from Menu.

`WorldLayout.cs` owns persisted bounds and the Home arrival alias. `SoloScenery.cs` owns only this player's camera and a maximum of three loaded/requested panoramas. Major travel waits for authority acknowledgement, applicable saves and visible texture readiness; distant textures are released. The camera can pan by dragging empty ground, or follow joystick/tap walking. Mobile textures use ASTC 6×6, no mipmaps and no readable CPU copy. Physical iPad memory/frame-time qualification remains separate from native Windows checks.
