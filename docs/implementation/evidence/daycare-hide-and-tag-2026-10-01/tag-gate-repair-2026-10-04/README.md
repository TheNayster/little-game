# Tag build-gate repair — October 4

DAY-01/FAMILY-01; owner: focused randomized Tag build failure. Consumers: existing Tag and Sandcastle records, current build workflow and parent local playtest. Retain root-cause/reproduction evidence until a linked successor preserves it. Synthetic .NET/Unity fixtures and isolated native processes only. Real saves/apps/server/security untouched.

## Evidence and classification

Original [458 log excerpt](original-458-excerpt.txt) and [459 excerpt](original-459-excerpt.txt) fail `DaycarePlayTests.TagRoutes` line49, `Need(maxEdges[i]<1.2,"runner remains at arena edge")`. The full logs remain private `LocalData/Logs/build-network-458.log` and459. They do not record the GUID-derived route seed, NPC routes or failing corner; their exact historical instances cannot be replayed. Seed70 below is a new deterministic counterexample, not a recovered458/459 seed.

[Current-source seeded failure](tag-seed-70-before.txt) and [pre-Sandcastle9d3ea80 failure](tag-seed-70-baseline.txt) are identical: human chaser at(80,60), NPC3 starts(2320,240), seed70, clock0/grace1000,600 ticks×0.05s. Its first goal(1526,218) is inside the allowed goal rectangle but too shallow to leave the top strip quickly. At tick23 it is still(1960.1399,230.0291), above y230, with24 edge samples=1.2000000000000004 seconds. It traverses a large horizontal distance and is moving; the problem is delayed edge recovery. The strict1.2s limit and all movement/variety assertions remain unchanged. This is an existing gameplay defect exposed by an unrecorded random stream, not an incorrect assertion or a Sandcastle regression.

Read-only comparison found `ClubRunGoal` byte-identical to October1a2d9c9d after line-ending normalization, SHA256f14636f7c8e79444df959565a24f5efc8d7ccfa91ffe32673cc9f34868e2175f. Neither52d140e norb1011fb changed that method/test. A private58-file Core export of9d3ea80 was compiled with the same current instrumented fixture, executing the same failure; [baseline input hashes](baseline-9d3ea80-core-inputs.json). No old checkout or installed game was modified. The .NET fixture binds unused Unity JSON signatures with a throwing shim (serialization cannot be mistaken for real JsonUtility); its route simulation executes the actual build-gate class and real Core. Full Unity JSON/build-gate execution is separately recorded below.

Diagnosis scanned a planned fixed0–127 seed range in order and stopped at its first counterexample70; no passing artifact was obtained by rerolling. Normal verification now always includes70,0,413,458. Tests seed only the synthetic authority's transient private RNG and log seed/chaser/edge/dt/corners plus failure episode/goal/position. Production RNG and save fields are unchanged.

## Fix and checks

`Code/Core/Worlds/Daycare/DaycarePlay.cs::ClubRunGoal`: an edge runner first commits to a short inward waypoint (x clamped220–2180, y100–200), then resumes the existing randomized route selection. Speed, chasing/contact/grace, cast, membership, route commitments and independent exit behavior remain. Route restoration still uses transient memory; no save migration. Shared movement semantics require content72; schema53/protocol3 remain.

- [Seeded Core results](tag-routes-after.txt): seeds70/0/413/458, both human/NPC chasers, both human walls, four corner starts,9600 ticks. Original displacement>1 per tick, edge residence<1.2s, horizontal span>700, vertical span>50, at least2 turns, checkpoint validation and independent exit assertions pass. Worst recorded edge residence0.3s.
- Existing .NET boundary/JSON tests also pass: both sides,1200 ticks, bounds, cast/round/route retention and independent departure. They remain .NET serialization checks, not a Unity JSON claim.
- **Standard full-game build463 passed** via `Tools/Build/Build-FamilyGame.ps1 -BuildNumber 463`, without `-FocusedSandcastle`. [Normal-gate markers](standard-gate-markers.txt) include seeded Tag, complete DaycarePlay and actual Sandpit JSON migration; [build summary](build-summary.json) reports both release profiles succeeded with0errors/0warnings, Unity6000.3.24f1. No gate/test was disabled, skipped, weakened or moved behind a focused bypass.
- [Four actual native-client result](native-result.json) and [32-second route trace](native-routes.json): stopped synthetic checkpoint configured four corner NPCs, then reopened with four shared clients. No runner stayed in an edge strip during the sampled trace; horizontal spans1698–1916 and vertical spans93–118. Walk drawings change, actual contact transfers the star, one player's Back to Daycare leaves three in the same round/cast. Two isolated batches each ran four clients; all10 process exits0 and no runtime exceptions. [Phone](tag-routes-phone.png)/[tablet](tag-routes-tablet.png) actual captures opened and inspected. The trace begins after native startup/rejoin; exact first-tick escape is established by seeded Core and Unity checks, not by pretending the native trace sampled startup.
- [462→463 source comparison](tag463-vs-sand462.json):2325 Unity inputs; only DaycarePlay routing, EditorDaycarePlayTests, WorldLayout content and version setting differ. All Sandcastle rendering/input/rules/assets/migrations remain identical. Valid462 direct/flexible decoration evidence is reused; standard463 also executes the current Unity migration tests. Builds/artifact hashes were checked before native execution.

Ready for **isolated local parent playtesting** with current463 Windows client/server, including latestb1011fb Sandcastle. Native proof uses synthetic data; it does not establish physical-device comfort or child enjoyment. The installed451server/452phone stay52/70/3 and are incompatible with72; coordinated future deployment requires separate authorization. Do not point a72 client at the live family server or change admission to make it join. No app installation/deployment is included.

## Replay

From the current game root:

```powershell
dotnet run --project Tools/Verification/DaycarePlay.Tests/DaycarePlay.Tests.csproj --configuration Release -- --seed 70
dotnet run --project Tools/Verification/DaycarePlay.Tests/DaycarePlay.Tests.csproj --configuration Release -- --routes
#463 already exists; select an unused number only when source requires rebuilding.
& Tools/Build/Build-FamilyGame.ps1 -BuildNumber 463
uv run --offline --with cryptography python Tools/Verification/Test-DaycareTagRoutes.py 463
```

The native tool verifies current Unity inputs and artifact hashes, starts its own loopback server, and only edits its stopped synthetic checkpoint. Current463 binaries remain under `Builds/NetworkProbe/G3-0.0.463/Client` and`Server`; build artifacts/credentials/saves are not committed. Existing interactive preview tooling remains available; this task does not alter the saved preview selection or its world.
