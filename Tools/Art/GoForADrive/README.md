# Go for a Drive source recovery — BLU-02

Owner: Go for a Drive implementation. Consumers: the isolated asset-only research probe and local 2D media assembler. Keep these tools while the world needs reproducible media; retire only with the world and retained evidence. No original application scripts are executed.

## Current artifact and provenance

Use ignored `LocalData/Research/GoForADrive/Content-fixtures` with adjacent `Content-fixtures-receipt.json`. It contains 185 unique picture keys, 174 animation strips, 139 audio keys and 438 files (88,627,079 bytes). Earlier packs remain historical inputs. `SourceProvenance.json` records APK, source-scene, prefab, manifest and receipt hashes; the final manifest also records picture identities, pose recipes, audio adaptations and fixture selections.

The retained source is `com.storytoys.lego.bluey.googleplay` 7.1.0 (1551), Unity 6000.0.58f2. Existing REA Android evidence is `ev_040835aa748681c0cdaee068c7c7821bfc6e2b4c3ed802e3ca2de33af8f7c8f9`. Package metadata does not establish behavior. No shared REA session was changed.

The APK split contains 66 embedded Addressables bundles. Fourteen drive-specific bundles parsed without object failures. Original bundles, full AssetRipper export, read-only object/controller trees and research logs remain under ignored `LocalData/Research/GoForADrive`. Task-owned AssetRipper/Unity processes were closed. Original artifacts were preserved.

## Source coverage and adaptations

The main `SCN_00_GoForADrive` scene supplies scenery shared by four subscenes:

1. Imperfect Packing: six ordered sets of three objects; difficulty levels `[1,2,2,2,3,3,3,4]`. `PictureAliases.json` preserves exact set identities.
2. Drive Time: three lanes, Grey Nomad and three additional traffic prefabs, petrol, toilet, roadworks and animal activities.
3. Window Games: fifteen exact foreground patterns, physical star rewards, ten sticker positions and the original arm/scroller configuration.
4. Build Big Thing: twelve starting statue pieces, source 300-brick limit and twelve UI groups flattening to 36 palette variants. Source smash is a car collision.

Bluey and Bandit are original scene cast. Bingo and Chilli use their actual shared package DUPLO rigs as four-player family additions. Static configuration/rendering does not establish original timing or execution.

`brick-0..11` and `statue-brick-0..11` use isolated source-camera alpha-cropped renders of the exact twelve starting pieces. `statueLayout` preserves original GameObject IDs, source positions, projected points and pixel-derived bounds. Bounds use top-origin normalized 1920x1080 coordinates matching the background. The 32-degree camera was moved three source units upward along camera.up; the full statue composite was inspected. Finite source ground exposes blue near the lower viewport edge, so production stage framing must account for that boundary. `brick-12..47` preserves all 36 source UI palette variants in serialized order; `Palette.json` records them. Packing aliases preserve all eighteen set entries.

Backgrounds load the main scene additively. Driving/window bakes place five original scrolling chunks at the source 11.8-unit span because source controllers normally place them at runtime. Handwritten shaders recover base colors and primary decals; original secondary effects/reflections are not restored.

Animation uses exact DUPLO rigs, source clip curves, 12 fps, 256-pixel cells and eight columns. CPU BakeMesh after every sample is necessary for moving limbs in batch rendering. All 174 strips have nonempty, unclipped frames and matching anchor-array lengths. Exact source hand-parent/head transforms supply tracks; hold uses the right hand. Walk/wave strips were inspected frame by frame; 12 fps previews were generated, but normal-speed playback and final gameplay visual acceptance remain separate.

Literal clips include idle/walk/run/happy/wave/sitting/dancing, adult driving and child toilet. Family recipes are explicit: carry=Walk plus both middle hand layers; throw=Wave release gesture; balance/car-smash reaction=FreakOut; fuel=adult HoseWashing or child right-middle hand pose; build=BendFWD. No recovered standalone throw/build/character-strike controller is claimed. Stationary inspection found carry's raised leg/tilted torso: a distinct stationary hold should use Idle plus both middle hand layers, while moving carry retains Walk. Child fuel appeared neutral, but explicit Idle before its partial hand sample would avoid residual pose state. These are report-only findings; fixture correction did not change pose strips.

Audio is PCM16 WAV. Activity music uses original packing/driving/window/peanut/Nomad/layby tracks. Packing uses original brick click/drop; start/continue use car on/off; travel/stop use engine; toilet/fuel/filled/light/statue/smash/replay/door use the corresponding original sounds. Documented adaptations are boost=movement prompt, rubble=brick drop, window-bump=window brick feedback, wave=Bluey greeting. Complete mappings are in the assembler/manifest. Format checks are not subjective listening approval.

## Exact patterns, traffic and grip

`PatternInputs.json` preserves source order: cow, lamb2, hay2, chicken1, pony1, lamb1, tractor1, cow2, chicken3, pony3, hay1, cow3, lamb3, chicken2, pony2. Suffixes are not counts: chicken1 has three chickens; chicken2 has one on hay; chicken3 has two; cow2 has one cow; lamb1/lamb2 both have two in different arrangements.

`windowPatterns` and `pattern-content.json` record source hashes, original start/end/span, image dimensions, reward root/bounds, semantic element paths/targets and collider rectangles. All normalized coordinates use top origin. `window-pattern-N`, `-obstacles` and `-reward` share a crop and dimensions. Obstacles plus reward reproduce every full RGBA image byte for byte. Each source pattern has a physical star; collection is conditional on contact, not automatic per encounter.

Exclude root collider path `""` from physical obstacle detection: it is the broad input surface. Semantic obstacle collider paths are `Animals` for twelve patterns, `Obstacle Collider` for hay indices 2/10 and `pfb_gfd_Tractor` for index 6. Reward is the `pfb_gfd_Star` sphere trigger. Its root point is near its base; use reward bounds center or projected sphere for visible contact. Bounds are projected source 3D AABBs, not recovered collision code, and broad bounds may extend beyond cropped art.

`traffic-0/1/2` are original RecycleTruck, Tractor and Truck; `nomad` remains the original Grey Nomad. Source drivers and side silhouettes are retained. `nozzle` is the actual Petrol_Handle; holdPivot `(113.7743,374.3969)` projects its original Hole_Logical in bottom-origin pixels. Source grip parameters are handAlignment 2, angle 0.5, walking override 0.6 and snap distance 0.3 (`nozzleGrip`). Flexible hose LineRenderer is not baked. Other prop holdPivots explicitly use alpha-center at hand adaptation, not asserted original 3D grips.

## Corrected fixtures

Earlier toilet/rubble/petrol pictures included scrolling scenery. Current fixture-only replacements preserve source active renderer states and transparent exteriors:

- Toilet 470x423: `pfb_gfd_Road_Toilet_OneShot`, exact `Environment/Tree`, `Tree_1`, `Tree_2`, `dogsign`. Original toilet activity targets the tree cluster near local `(-7.82,0,4.33)`; no cubicle was present.
- Rubble 461x410: `pfb_gfd_Road_Roadworks_OneShot`, exact `3D Elements_1/group/RoadworkGroup` bricks/spade/boulder. Road, field and unrelated distant construction scenery are excluded. Traffic feedback retains existing keys.
- Petrol 830x406: `pfb_gfd_Road_PetrolStation_OneShot`, exact `Environment/Petrol_Station/Model` pumps/canopy/shop. Floor, scrolling ground and shadow planes are excluded; separate nozzle/grip is unchanged.

All three images were inspected at actual resolution. Transparent alpha, source-export hashes and all 438 receipt hashes passed. Fixture selections/hashes are in `fixtureCorrections` and provenance.

## Reproduce in an isolated probe

Use `Tools/Art/Prepare-RecoveredUnityProbe.py` on a new destination, excluding recovered Scripts/Plugins/Editor. Replace its compatibility reader with this folder's `LegoBrickMaterial.cs` and `.meta` (Bluey namespace/GUID differs from Marvel). Copy bake tools to the probe's Assets/Editor; retain established RecoveredBrick/RecoveredSprite shaders. Use Unity 6000.3.24f1 batch mode. Outputs must be new directories:

- `GoForADriveBake.Run`: `LITTLE_WEEPS_DRIVE_SCENE_BAKE_OUTPUT`.
- `GoForADriveBake.RunStatueLayers`: `LITTLE_WEEPS_DRIVE_STATUE_LAYER_OUTPUT`.
- `GoForADriveSprites.Run`: `LITTLE_WEEPS_DRIVE_SPRITE_BAKE_OUTPUT`.
- `GoForADrivePatternBake.Run`: `LITTLE_WEEPS_DRIVE_PATTERN_OUTPUT`; copy PatternInputs.json to Assets/Research/PatternInputs.json.
- `GoForADrivePatternBake.RunFixtures`: a new `LITTLE_WEEPS_DRIVE_PATTERN_OUTPUT`.

```powershell
python Tools/Art/GoForADrive/Assemble-GoForADrivePack.py --research LocalData/Research/GoForADrive --sprites LocalData/Research/GoForADrive/DrivePack-v5 --scenes LocalData/Research/GoForADrive/SceneBakes-final --layers LocalData/Research/GoForADrive/StatueLayers-final --output LocalData/Research/GoForADrive/NewContent
python Tools/Art/GoForADrive/Append-GoForADrivePatterns.py --base LocalData/Research/GoForADrive/Content-final --base-receipt LocalData/Research/GoForADrive/Content-final-receipt.json --patterns LocalData/Research/GoForADrive/PatternPack-v1 --output LocalData/Research/GoForADrive/NewCoverage
python Tools/Art/GoForADrive/Replace-GoForADriveFixtures.py --base LocalData/Research/GoForADrive/Content-coverage --base-receipt LocalData/Research/GoForADrive/Content-coverage-receipt.json --fixtures LocalData/Research/GoForADrive/FixturePack-v1 --output LocalData/Research/GoForADrive/NewFixtureContent
```

Assemblers verify input receipts, reject existing outputs/colliding keys, preserve media in ignored storage and write new receipts. They do not install clients, replace servers or prove production four-player behavior.
