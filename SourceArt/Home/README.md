# Home interaction artwork

Built-in `image_gen` generated the sofa, trampoline, radio and open/closed shed as separate transparent PNGs. Exact prompts, original output paths and editing references are in [manifest.json](manifest.json). Runtime copies are under `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/HomeArt`. Images remain editable source assets; character art is the existing layered Bluey/Bingo set.

These are new interactive foreground fixtures over the current panoramas. Baked panorama furniture remains decorative in this pass. This is a prototype art/presentation milestone, not user visual approval or a claim that every pictured background object already works.

`Tools/Generate-HomeMusic.py` deterministically creates the original 12-second instrumental loop. It uses synthesized sine harmonics, no recordings or commercial game audio. The local Music setting is separate from spoken hints and shared radio power.

The home ball uses the existing native UI shape system. No extracted commercial game asset or new generated character replacement is used.
