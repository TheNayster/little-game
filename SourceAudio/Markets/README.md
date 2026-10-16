# Markets audio provenance

Owner: **BLU-01 / WORLD-02 / FAMILY-01 / ITEM-02**. Consumers: Markets busker layers, interaction feedback, character/pony reactions and candidate packaging. This folder records provenance and intended mapping; no source audio is copied here by this record and no subjective playback acceptance is claimed. Retain while Markets consumes the sources; preserve the referenced ignored inputs and receipts.

Source: retained LEGO Bluey package `com.storytoys.lego.bluey.googleplay` and its catalog-reachable Markets dependencies. Local receipt root: `LocalData/Research/DuploMarvel/world-tests/Markets/`. Package hashes are in `input-receipt.json`; fetched bundle bytes/hashes in `download-receipt.json`; scene/dependency identities in `catalog-closure.json`. Dependency URLs use the `7.1.0/Android` content path, separately from any installed app version.

Editable/exported audio is under `bluey-markets-export/ExportedProject/Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Audio/`. Event bindings are in `Prefabs/Audio/pfb_mkd_audio.prefab`. Source declarations are in the base export's `LegoDuplo/MarketDay/MarketDay_AudioID.cs` and `MusicGameMusicLayerController.cs`; their empty/default bodies do not establish execution order or timing.

| Group | Verified source inventory | Intended shared-family use |
| --- | --- | --- |
| Busker music | Three songs, each with intro and five layers; guitar, bass, drums, piano, sax | One authoritative song/layer state, client-local synchronized playback |
| Busker controls | Seven pluck variants, success/fail button, ukulele appear/disappear | Shared guitar/rhythm actions and feedback |
| Market | Blender tap/end early/finish, fruit spawn/drop, drinking, cupcake tap/eating, bubble spawn/move/pop | Feedback from accepted shared actions |
| Ponies | Eating/drinking, brush/washing loops/end, bucket bubbles, hearts, happy/jump/fart/poop, cleanup, walking, appearance cycle, curtains/tent, sparkle | Contact/reaction cues tied to per-pony state |
| Ice cream | Order thought-bubble open/close/prompt, build hint panel, register, parasol, eating/burp | Shared order/build and incidental feedback |

The audio prefab confirms `layerOneStart/Stop` through `layerFiveStart/Stop` and `stopAllBuskerMusic`; the music scene serializes120 BPM and three tap buttons. The three-song/five-layer data is recovered. Cooperative rhythm scoring, persistent shared song selection, late joining, independent departure and local playback synchronization are explicit **family adaptations**. Exact original level thresholds and static initialization arrays are not recovered from empty stubs.

Do not claim that all listed clips have been packaged, listened to, mixed or approved. Candidate media must receive its own per-file hash receipt after selection/conversion and a packaged readback; those delivery receipts are distinct from package/export provenance. See the [source inventory](../../docs/implementation/bluey-markets-source-inventory-2026-10-16.md) and [phase plan](../../docs/implementation/bluey-markets-world-phase-plan-2026-10-16.md) for current scope and acceptance.
