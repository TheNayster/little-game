# Zoo sound assets

[The manifest](manifest.json) records exact public source pages, creators, CC0 licenses, source/runtime hashes and edits. Elephant, lion and zebra use animal recordings; the penguin uses a cartoon squeak. The four dinosaurs reuse the user-approved dinosaur-book effects, with their approval manifest linked in each record.

Fish, giraffe and quiet reptiles use soft activity sounds designed on this PC: bubbles, splashes, rustling and feeding. Those effects are not presented as field recordings or reconstructed vocal anatomy. `Tools/Prepare-ZooAudio.py` preserves the preparation recipe. Raw source previews stay in `sources/`, and Unity resource copies use short mono WAVs.

The local mixer bounds playback to two sources, throttles automatic calls and lowers Zoo effects during book narration. Tapping an animal plays its call or activity sound. Clips release when the player leaves the Zoo.
