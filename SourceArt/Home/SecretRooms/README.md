# Secret room artwork and integration

Built-in **imagegen** generated four raster assets after the room research. Exact prompts, reference roles, default generation locations, workspace destinations and SHA-256 hashes are in [manifest.json](manifest.json). Originals are copied unchanged to this folder and Unity Resources; the six-cell plush sheet is sliced by runtime sprite rectangles without rewriting the image.

| Source | Runtime use |
| --- | --- |
| `base.png` | One 3:1 illustrated empty architecture, reused by four persistent secret IDs. Visible ordinary exit at the left. No painted duplicate furniture. |
| `door.png` | Small far-back bedroom entrance, locally faded only on approach. Original alpha retained. |
| `fort.png` | Wide blanket fort rear plus registered top/side/hem cover polygons. Four saved occupancy positions; no vanishing occupant. |
| `plush-sheet.png` | 3 columns × 2 rows: dinosaur, dog, cat / rabbit, bear, whale. Six fixed IDs per created secret room. |

The existing bedroom rug, cushions, lamp, shelf and chest are shared. `SoloSecrets.cs` records door/fort dimensions, anchors and cover polygons. `SoloBedroomFurniture.cs` records real shelf/chest visual support offsets. `SecretSky.cs` draws two slow ribbon bands and twelve star diamonds; another twelve diamonds surround the door. These effects and locally synthesized gentle audio are presentation only.

Texture importers retain alpha, disable Read/Write and mipmaps. The plush sheet keeps a 2048-pixel cap; other sprites use the existing 1024 cap, and architecture follows the existing panorama importer. Active background requests retain the three-texture limit. Fixture textures are a separate bounded cache released on presentation reset. Physical A10 memory/frame-time qualification is still required.

Accepted Bluey/Bingo sources and animation remain unchanged. New art has native composition review; it does not yet have the user's physical-device acceptance. [Implementation and evidence](../../../docs/implementation/secret-rooms-2026-09-26.md).
