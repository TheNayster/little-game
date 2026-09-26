# Home interaction artwork

Built-in `image_gen` generated the sofa, trampoline, radio and open/closed shed as separate transparent PNGs. Exact prompts, original output paths and editing references are in [manifest.json](manifest.json). Runtime copies are under `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/HomeArt`. Images remain editable source assets; character art is the existing layered Bluey/Bingo set.

These were introduced as interactive foreground fixtures over the panoramas. **That composition is superseded by the September 26 user feedback:** baked furniture plus a second usable copy is not acceptable for the finished home. The existing images remain prototype evidence, not user visual approval.

**Current art task: ART-HOME-02, first sofa/trampoline/shed slice.** Follow the [scene-layer research](../../docs/implementation/home-scene-layer-research-2026-09-26.html) and [editable inventory](scene-layer-plan.json): complete clean plates, one placement per fixture, aligned rear/front parts, actual seat/support anchors and preserved stored-item identities. Begin with the living room and sofa, then the existing backyard fixtures, before kitchen expansion. The first three clean plates and registered front-part geometry are now implemented; native/device qualification is recorded in the current report. Other inventory entries remain staged. [Composition prompts and sources](composition-2026-09-26.json) and [runtime layer geometry](../../Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/HomeArt/layer-layout.json) are editable masters for this pass. Geometry reuses each fixture texture; logical items never depend on the visual hierarchy.

`Tools/Generate-HomeMusic.py` deterministically creates the original 12-second instrumental loop. It uses synthesized sine harmonics, no recordings or commercial game audio. The local Music setting is separate from spoken hints and shared radio power.

The home ball uses the existing native UI shape system. No extracted commercial game asset or new generated character replacement is used.

## Upstairs foundation — September 26

The [upstairs art/source record](Upstairs/README.md) supplies the first working stair/landing slice. Four bedroom door facades are prepared; the personal bedroom interiors and secret rooms remain subsequent work.
