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
