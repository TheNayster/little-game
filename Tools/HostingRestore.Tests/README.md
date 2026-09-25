# Complete-state restoration experiment (G4-PREP-02)

> Historical experiment. Device hosting (G4/AUTO-02) was retired by the user on September 25. These tests are retained for evidence and possible dedicated-server recovery reference; they are not the active implementation queue. See [current decisions](../../docs/current-decisions.md).

This tool links the shipping `RecoveryRecord`, `SoloWorld`, `FamilySession` and
`CheckpointStore` source. It is outside Unity's Assets and does not change build
98. It proves restoration of real state before mobile authority integration.

From the game root, use a new evidence directory:

```powershell
dotnet run --project Tools/HostingRestore.Tests/HostingRestore.Tests.csproj --configuration Release -- self-test LocalData/Verification/hosting-restore-next
uv run --offline --with cryptography --with psutil python Tools/Test-ReplacementAuthority.py 98
```

The native test creates a disposable enrolled family and four Windows clients.
It captures a client's actual durable, multi-chunk replica, stops only its test
authority, retains that authority's original directory, and publishes a new
authority directory from the client record. All four original profiles then join
a fresh native server process. Old watering requests are replayed through the
real transport; new movement and item interactions are exercised in both areas.
All test processes are closed and private evidence is retained under LocalData.
The existing server launcher verifies the native artifact manifest.

The `stage REPLICA EXPECTED_JSON NEW_AUTHORITY_DIRECTORY` mode is a lab bridge,
not a parent recovery command or shipping host-promotion endpoint. It requires a
complete checksummed envelope and independently selected expected metadata:
family, authority, world, source epoch, snapshot-world ID, four profiles,
checkpoint serial, revision and exact payload hash. Missing/defaulted arrays or
nested fields, duplicates, unknown fields, future versions and mismatches are
refused. It does not silently replace the selected input with a backup.

Restoration clones the snapshot and releases transient holds through the real
game rules. This increments the revision once when any hold exists, without
changing object identities, contents, player positions/areas, retained receipts
or unaffected idle clocks. The staged output is read back before create-only
directory publication. Existing destinations and concurrent creators win; failed
unpublished stages remain available for inspection. Power-loss durability across
directory publication is not qualified by this test.

The seven pure C# groups cover partial water contents, complete activities,
receipt replay/conflicting reuse, fresh connection bindings, empty-world clock
pausing, remaining idle time, stale holds, incomplete/malformed/future records,
wrong identities/order/hash, and publication interruption/races. Deduplication
coverage is the current game's retained 128-receipt window, not unlimited history.

The four native groups also reject an actual version-2 private adventure. An open
offline game legitimately advances its own idle timers; the test compares its
gameplay/provenance across reopening and compares exact saved bytes after reunion
has finished. It must not confuse elapsed offline play with a server overwrite.

## Boundaries

- Source shutdown is explicit. This is not automatic selection, handoff, fencing
  of an unreachable old host, or proof that all latest live commands reached the
  selected replica. A planned transfer still needs its final frozen checkpoint.
- The lab reuses its existing PC authority credentials. No private authority key
  is copied to a phone, iPad or another computer. Authenticated mobile host
  capabilities and identity/endpoint changes remain G4-01 work.
- Hash/metadata pinning detects a different selected record; it does not create
  an authenticated host grant. Strict JSON completeness here is part of the lab
  importer and is not yet a shipping mobile import boundary.
- A private offline adventure and a presentation view are never accepted as a
  complete authority record. Rejoining the family still loads server state.
- Windows tests do not qualify iPad foreground/background, thermal performance,
  multi-device migration, or partition/reunion behavior.
