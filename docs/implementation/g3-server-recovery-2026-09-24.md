# Server backup and restore — G3-OPS-02

**Goal IDs: NET-02 / TRAVEL-01.** This milestone implements local, same-Windows-user recovery of the enrolled shared world. It does not complete independent disaster recovery or move the server to the VPS. The actual family authority stays on **83**, with its existing save and process preserved. **85** prepares the previous guarded parent stop plus a native interrupted-restore startup guard; Windows approval for that executable is still needed.

## Recovery behavior

`Tools/Server-Recovery.py` creates an immutable `.lwbackup` containing the complete durable checkpoint, public family record, protected authority and issuer enrollment, and all four protected player records. This includes both areas, players, items, action receipts and idle clocks. It does not archive only the simplified client view. Builds, logs, temporary process records and personal TV media are outside this bundle.

Each file has a SHA-256 checksum. Version, file-name allowlist, size, family/authority/profile agreement, protected credential hashes and certificate/key correspondence are checked. The small .NET validator links the actual game's `SoloWorld.Validate`, preventing a checksum-valid but impossible item/receipt state from being restored. Checksums detect corruption; they are not an authenticated signature of the entire archive.

Live backup reads one checkpoint handle with read/write/delete sharing and verifies the completed archive from disk. It does not stop play. This captures the last durable checkpoint, not an uncommitted animation or guaranteed latest network acknowledgement. On recovery, transient held-item leases are released by the game's existing load rule.

Restore requires the selected family to be stopped. The operation holds the same exclusive `authority.lock` as the game. It checks the current save's expected hash so an outdated restore decision cannot overwrite newer play. Existing protected enrollment must match exactly; it is never silently replaced. The old primary, backup and pending-file bytes are retained before changes, including damaged originals when repairing corruption.

A durable pending marker is written before any save path changes. If the restore is interrupted, the parent controls and normal launcher refuse startup; **native runtime 85 also refuses direct startup**. Explicit rollback restores the retained preimage and removes the marker after verification. Successfully restored worlds remain stopped until deliberately started. The restore command does not disconnect children or launch a server automatically.

For an entirely missing family directory, `recover-missing` validates the bundle, prepares the complete directory privately, then publishes it through a create-only directory rename. It refuses an existing destination or a still-running process referencing that family. It does not copy old process/launcher records or advertise a second authority.

Windows sharing mode zero supplies the exclusive-open behavior used by the game and recovery tool. [Microsoft CreateFileW](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew).

## Qualification

- Server/client **85** build successfully with zero errors/warnings. The linked recovery validator builds successfully.
- A disposable enrolled world and four native Windows clients exercise live backup, occupied restore refusal, exact player/item/receipt restoration, original-player admission, complete-file rollback, nine rejected bad backups, interrupted recovery, corrupt-primary repair and missing-directory reconstruction. [Native evidence](evidence/server-recovery-2026-09-24/native-85.json).
- Both **85 server / 79 client** and **79 server / 85 client** pass native admission, pickup, fill and shared state. [Compatibility](evidence/server-recovery-2026-09-24/compatibility-85.json).
- The actual family world's **83** checkpoint was backed up and read-back verified at revision **1866**. Its authority was not stopped, restored or replaced. No phone/iPad/Mac was accessed. The private backup location is recorded under `LocalData`; private enrollment and saves are excluded from Git.

These are Windows native tests, not a new physical-device acceptance pass, power-loss hardware test or offsite restore.

## Parent/developer workflow

Run from this game's root using PowerShell. Replace the angle-bracket placeholders with the selected enrolled family ID and the paths returned by the tool. Do not paste example placeholders unchanged.

```powershell
uv run --with cryptography python Tools/Server-Recovery.py backup --family <family-id> --build 85
uv run --with cryptography python Tools/Server-Recovery.py verify --backup <returned-backup-path>
```

Default bundles are create-only files in `LocalData/ServerBackups`. An explicitly chosen `--destination` can receive a bundle; no destination is chosen or uploaded automatically, and no retention job deletes prior backups.

For an existing, stopped family, inspect its current `server-world/world.save` with `Get-FileHash -Algorithm SHA256`. Use the lowercase hash as `--expected-save-sha256` (or `missing` when the primary does not exist):

```powershell
uv run --with cryptography python Tools/Server-Recovery.py restore --family <family-id> --build 85 --backup <verified-backup-path> --expected-save-sha256 <current-hash>
```

The command returns a rollback job. Interrupted recovery uses `rollback --family <family-id>`; rolling back a completed restore also requires `--job <returned-job>` and `--expected-save-sha256 <current-hash>`. Newer state is retained before a completed restore is rolled back. Rollback may restore an already corrupt original; the tool preserves it without claiming it is usable, and normal startup stays blocked by checkpoint validation.

When the entire canonical family directory is missing, use `recover-missing --family <family-id> --backup <verified-backup-path>`. There is no force/overwrite mode. Keep the game stopped and inspect the result before using the normal verified server launcher.

## Boundaries and next task

Protected enrollment is still Windows current-user DPAPI data. Microsoft documents that decryption normally needs the same credentials and computer, with specific exceptions such as roaming profiles. Consequently, copying these bundles to another disk does **not** establish recovery after losing this Windows installation or moving to Linux/VPS. World/public JSON is not separately encrypted. [Microsoft DPAPI](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata).

An independent destination and a deliberate portable credential backup/migration must be selected and tested before independent recovery is complete. Certificate rotation, automatic backup scheduling/retention, a parent restore UI, hardware power-failure qualification and media/signing backups remain separate work. Current tools require the installed .NET SDK and Python/cryptography environment. No shared-world branching/merging or iPad host migration is supplied by this local recovery tool.

**Next bounded task: G3-OPS-03, bounded crash supervision and an isolated crash/rejoin test.** Respect deliberate parent stops, prevent duplicate authorities and restart loops, and leave corrupt saves/interrupted restores blocked. Sustained device/outage checks, VPS readiness and mandatory G4 iPad hosting/recovery remain in the [main build guide](../family-playset-build-guide-2026-09-23.html#9-first-implementation-work-queue).
