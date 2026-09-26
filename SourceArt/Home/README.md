# Home interaction artwork

Built-in `image_gen` generated the sofa, trampoline, radio and open/closed shed as separate transparent PNGs. Exact prompts, original output paths and editing references are in [manifest.json](manifest.json). Runtime copies are under `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/HomeArt`. Images remain editable source assets; character art is the existing layered Bluey/Bingo set.

These were introduced as interactive foreground fixtures over the panoramas. **That composition is superseded by the September 26 user feedback:** baked furniture plus a second usable copy is not acceptable for the finished home. The existing images remain prototype evidence, not user visual approval.

**Next art task: ART-HOME-02.** Follow the [scene-layer research](../../docs/implementation/home-scene-layer-research-2026-09-26.html) and [editable inventory](scene-layer-plan.json): complete clean plates, one placement per fixture, aligned rear/front parts, actual seat/support anchors and preserved stored-item identities. Begin with the living room and sofa, then the existing backyard fixtures, before kitchen expansion. The inventory is proposed production data; none of its new layers are implemented yet.

`Tools/Generate-HomeMusic.py` deterministically creates the original 12-second instrumental loop. It uses synthesized sine harmonics, no recordings or commercial game audio. The local Music setting is separate from spoken hints and shared radio power.

The home ball uses the existing native UI shape system. No extracted commercial game asset or new generated character replacement is used.
