# Little Weeps world music

Six original, instrumental 32-bar compositions: cozy piano/clarinet Home, guitar/vibraphone backyard, clarinet/pizzicato park, flute/piano creek, guitar/flute beach and piano/marimba daycare. Each has an opening, answer, quieter contrasting section and return. The editable MIDI scores and arrangement generator are retained; no Bluey melody, recording or character voice is sampled.

Research: [Bluey's composer on following each story with its music](https://www.bluey.tv/blog/qa-with-joff/) informed distinct moods and modest changes of instrumentation. These are new compositions using that broad playful acoustic direction. Final family listening feedback remains separate from signal and runtime tests.

Rendered locally with [FluidSynth 2.6.1](https://github.com/FluidSynth/fluidsynth/releases/tag/v2.6.1) and Chris Collins's [GeneralUser GS 2.0.3](https://github.com/mrbumpy409/GeneralUser-GS/tree/684543d5e5efaef08d02be50dcda8d552478fa60). See the retained [font license](GeneralUser-GS-LICENSE.txt), which permits private/commercial music production and records the author's sample-provenance qualifications. Neither synthesizer nor soundfont is shipped with the game. Renderer zip SHA-256: `fab7a2e4b85675b66970f97a39bbc239729c5e0f237198b5922a6a73cbc8677c`; the font hash is pinned in the generator/manifest.

Reproduce from the repository root, with the portable tools in ignored LocalData:

```powershell
python Tools/Generate-WorldMusic.py --synth LocalData/MusicStudio/fluidsynth/fluidsynth-v2.6.1-win10-x64-cpp11/bin/fluidsynth.exe --font LocalData/MusicStudio/GeneralUser-GS.sf2
```

The generator renders three cycles and takes the middle cycle to retain musical releases across the loop. A 5 ms boundary ramp removes sampler phase discontinuities. The stereo 44.1 kHz PCM masters live in `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/WorldMusic` under Git LFS; Unity imports them as background-loaded streaming Vorbis at quality 0.75. Tracks last 66–89 seconds, are balanced to −22 dBFS RMS before the in-game background gain, and have no clipped samples. The manifest records duration, peak, boundary difference and file hashes. MIDI files contain three cycles for rendering; the delivered audio contains one complete 32-bar cycle.
