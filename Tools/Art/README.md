# Original asset preparation

Purpose: reproducible preparation of original production artwork/audio sources.
Owner: Sandcastle Stage 5 (DAY-01 / LEARN-01 / FAMILY-01). Current consumer:
`Build-SandcastleAudio.py` produces four small Sandcastle Foley clips; it never
installs an app, touches a save or configures a server. Source documentation is
in `SourceArt/Daycare/Sandpit/StageFive`. Retain while shipped assets depend on
these generators; retire a generator only when its assets/source are replaced.

Hulk birthday recovery (WORLD-02 / FAMILY-01 / ITEM-02) owns
`Prepare-RecoveredUnityProbe.py` and `Build-RecoveredActorBindings.py`.
Their consumers are the isolated Unity research audit/baker in
`Tools/UnityEditor/RecoveredWorldAssetProbe.cs` and `RecoveredBirthdayBake.cs`.
The handwritten `LegoBrickMaterial` compatibility reader and replacement brick/
sprite shaders are maintained alongside those editor tools, including the
metadata needed to bind the copied assets. They contain no original game methods.
They read preserved exports and produce ignored research inputs/content, never
third-party executable scripts or a device/server rollout. Supply `--inspection`
to probe preparation to copy only the verified scene dependency closure. Retain
these tools while the reconstructed world's asset pipeline consumes them; retire
them only after replacing that pipeline and preserving its source evidence.

The birthday binder also supports exact prefab strategies and explicitly named
extra recovered actors. `Assemble-RecoveredBirthdayPack.py` combines a preserved
base bake with source cake sprites and gift outcomes into a fresh immutable local
pack, recording parent manifest hashes. Guest and held-prop bake passes retain
the earlier accepted packs. `Tools/Verification/Inspect-RecoveredSpritePack.py`
checks PNGs and anchor tracks without modifying imagery; that is integrity
evidence, not motion, audio or owner visual acceptance. These tools share the
Hulk recovery owner/consumers/retirement rule above.

The baker also has bounded append/export passes for original band floor panels,
castle back/front/deformation layers, bounce/interaction clips and the squirrel.
Use a fresh output directory for every pass. The read-only pack verifier checks
PCM hashes/durations, floor UV inventory and distinct castle blend-shape frames,
as well as atlas dimensions, anchor tracks and required movement. None of those
checks substitutes for native activity checks, listening or owner visual review.
