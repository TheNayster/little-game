# Technical source review — September 23, 2026

These are cached public source files for the [technical research](../../family-playset-technical-research-2026-09-23.html). They are not installed Unity packages or implemented game features.

- [Development/reference source manifest](inspected-source-files.json): 17 files, with source URLs, exact revisions and SHA-256 hashes.
- [NGO released v2.13.3 source manifest](release-source-files.json): 4 files checked separately from the development branch.
- The four `*-tree.json` files preserve repository metadata and file inventories used to locate relevant code. Saving an inventory does not mean every file in it was reviewed.

Review was targeted: read the complete small pickup/portal/scene-interest examples and manifest/license files; inspect the relevant scene-loading, discovery and shutdown/despawn methods in the larger files. This was not an exhaustive audit of the networking packages. No downloaded source was executed or imported into a Unity project.

## Findings tied to code

| Source | Inspected point | Finding |
| --- | --- | --- |
| NGO v2.13.3 `package.json` | Whole manifest | Unity 6000.0 baseline; Transport 2.6.0 dependency. Candidate for a 6.3 prototype, not an installed/qualified combination. |
| NGO v2.13.3 `NetworkManager.cs` | ShutdownInternal, especially line 1659 | Calls network-object cleanup; networking lifetime is not durable world lifetime. |
| NGO v2.13.3 `NetworkSpawnManager.cs` | DespawnAndDestroyNetworkObjects, line 1488 onward | Dynamically spawned objects enter destruction; prefab handlers can control destruction. |
| NGO v2.13.3 `NetworkObject.cs` | DontDestroyWithOwner, line 1392 | Owner-disconnect behavior, not a whole-world migration switch. |
| NGO development manifest | Whole manifest | 3.0.1 targets 6000.7.0b1 with newer dependencies. Do not confuse development head with the proposed released baseline. |
| Mirror `Portal.cs` | SendPlayerToNewScene | Sends transition messages to one connection and moves its player. |
| Mirror `AdditiveLevelsNetworkManager.cs` | Load/unload paths | Server loads subscenes; sample uses Physics3D; host player does not unload server scenes when traveling. |
| Mirror `SceneInterestManagement.cs` | Scene identity and observer rebuilding | Different instances are distinguished by scene identity, not merely a display name. |
| Mirror `NetworkDiscoveryBase.cs` | UDP setup and BroadcastDiscoveryRequest | Uses broadcast sockets/IPAddress.Broadcast. Not an unmodified fit for the selected native DNS-SD route. |
| Unity Playground `PickUpAndHold.cs` | Whole script | Keyboard-driven pickup deliberately makes another holder drop the item. Conflicts with our exclusive-item requirement. |
| FishNet manifest/license | Whole files | Package metadata reviewed; host-migration conclusion comes from official documentation, not a code-wide absence proof. |

## Research interpretation

Primary documentation and papers are linked directly in the technical report. Platform APIs establish building blocks; reference samples establish specific patterns. Neither proves this game's A10 performance, automatic recovery latency, item-return timing or content workload. Those remain explicit implementation/device tests.

Bluetooth was removed from the feature plan and backlog at the user's request. There is no Bluetooth experiment or qualification task scheduled.

All files belong to `Little weeps game`. The unrelated old project was not used or modified.
