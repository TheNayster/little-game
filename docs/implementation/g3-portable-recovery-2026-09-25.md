# Portable server backup and recovery

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**Task: G3-OPS-06 · September 25, 2026 · Goal references: NET-02 / FAMILY-01 and the G1 independent-backup gate.**

This Windows-only task proceeds while G3-REC-05 waits for the Mac and physical iPads. It adds a portable encrypted backup path without changing the game, rebuilding mobile 91, or updating the deployed family authority. Native Apple compilation/signing and iPad-first recovery checks remain next in the build sequence.

## What this adds

The existing `.lwbackup` contains an exact shared-world checkpoint and Windows-protected enrollment records. Those credentials normally depend on the originating Windows user and computer; copying that file to another disk alone does not establish recovery after losing the PC. Microsoft's DPAPI documentation describes that binding and its roaming-profile exceptions. [Microsoft CryptProtectData](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata)

The new `.lwportable` file encrypts the checkpoint **and** enrollment under a parent-chosen passphrase. It can be decrypted and validated without the original DPAPI blobs. On Windows recovery, the tool protects the recovered credentials with the destination user's DPAPI before writing them. Family, authority and four player identities remain the same, so recovery does not require inventing a new family or silently discarding the players' enrollment.

The file includes the shared server checkpoint, retained receipts, public family identity, authority certificate/key, issuer and four enrolled player records. It does **not** back up each device's separate solo adventures, imported TV clips, bookmarks, source assets or application signing keys. Those still need their own backup coverage.

## Recovery boundaries

- Export starts from an existing fully verified local backup; it does not stop a server or change the source backup. The destination must be a new file in an explicitly selected existing directory.
- Verification checks authentication, bounded file size, supported format/build, exact allowed record names, unique JSON fields, all four identities and credential hashes, certificate/key correspondence, checkpoint checksum and the game's C# world validator.
- Reconstruction only creates a **missing canonical family directory**. An existing directory, even an incomplete one or a concurrent recovery winner, is never replaced. Existing-world rollback remains the separate guarded local recovery operation.
- All destination credentials are DPAPI-protected in memory before staging. The shared checkpoint remains the normal game save format in the recovered server directory. No temporary plaintext credential files are written.
- Staged files are read back before create-only directory publication. An interrupted stage is retained for inspection but is not a canonical/startable family. Retrying uses a new stage. This is process-interruption coverage, not a promise about every storage-controller or power-loss failure.
- Recovery does not install a service, start an authority, copy startup intent or migrate network routes. Before starting the recovered authority elsewhere, the previous authority must be stopped/fenced to prevent two writers. No remote-host fencing is implemented here.
- Supported destination build must be at least as new as the recorded backup and no newer than the currently qualified recovery limit, 91. A copied backup does not include the server executable or runtime dependencies.

## Encryption and passphrases

Implementation uses `cryptography==50.0.1`: **scrypt** derives a 32-byte key with a fresh 16-byte salt (`N=131072, r=8, p=1`), then **AES-256-GCM** encrypts/authenticates the payload using a fresh 12-byte nonce and full 16-byte tag. The fixed version header, salt and nonce are authenticated together. Unsupported headers are rejected before deriving a key; a file cannot request arbitrary KDF costs. These fixed parameters and the 3 MiB plaintext limit are project choices for this small desktop backup, not a vendor certification. [Cryptography scrypt API](https://cryptography.io/en/50.0.0/hazmat/primitives/key-derivation-functions/#scrypt), [AES-GCM API and nonce requirements](https://cryptography.io/en/50.0.0/hazmat/primitives/aead/#cryptography.hazmat.primitives.ciphers.aead.AESGCM)

The parent enters the passphrase in a hidden interactive terminal prompt and confirms it for export. There is no password argument, environment-variable option or password file; redirected input and echoed fallback are refused. Use a unique, long random passphrase stored in the parent's password manager. The minimum length check is not a guarantee of strength. Do not send it in chat or keep the only copy beside the backup. Losing both the original Windows recovery access and this passphrase can make the encrypted backup unrecoverable.

Python handles decrypted values in process memory; this code does not claim complete memory zeroization or protection against malware running as the same logged-in user. Cryptographic primitives come from the library; the application format has project tests, not an independent security audit. Portable backup files are ignored by Git alongside local backups and saved worlds.

## Operator workflow

No real-family portable export or user passphrase was created during this task. The commands below are a reference for the later parent-assisted backup session, **not commands to run unchanged**. Replace the example paths/world ID after selecting the independent destination. Run from `C:\Users\sephi\Desktop\Little weeps game` with Python/uv, the pinned dependency and the existing .NET recovery validator available.

```powershell
# Export an existing verified .lwbackup; both prompts are hidden.
uv run --with cryptography==50.0.1 python Tools/Portable-Recovery.py export --backup 'C:\chosen\source.lwbackup' --destination 'E:\chosen\family-2026-09-25.lwportable'

# Read the actual copy back from the selected destination and verify it.
uv run --with cryptography==50.0.1 python Tools/Portable-Recovery.py verify --backup 'E:\chosen\family-2026-09-25.lwportable'

# Disaster recovery only: requires the original family folder to be missing.
# Install the qualified server build/dependencies first; stop the old authority.
uv run --with cryptography==50.0.1 python Tools/Portable-Recovery.py recover-missing --backup 'E:\chosen\family-2026-09-25.lwportable' --family 'the-recorded-world-id' --build 91
```

Verification reports the world ID, revision and build without printing credentials. Reconstruction reports success but leaves startup disabled. Keep the encrypted source backup and original machine/drive intact until the replacement world and original clients have passed acceptance. Linux/VPS credentials, remote endpoints and the Linux build remain a separate migration task.

## Evidence and remaining acceptance

**Passed:** six portable-recovery acceptance groups and all six existing local-recovery regression groups, each using a separate generated family and four native Windows 91 clients. The random test passphrase existed only for that run and was not retained.

| Portable acceptance group | Observed result |
| --- | --- |
| Live export and encrypted round trip | Four players remained connected; full two-area checkpoint retained; repeat exports differ; existing backup is never overwritten. |
| Independence from source DPAPI | Full decrypt/enrollment/world verification passed with both original DPAPI-unprotect entry points deliberately unavailable. |
| Refused invalid inputs | Wrong password, seven malformed/tampered/oversized envelopes, eleven authenticated invalid payloads, wrong family/build and existing-family recovery left usable data unchanged. |
| Interrupted/concurrent publication | Failures after enrollment staging and immediately before publication left no canonical family. A competing creator's destination and sentinel remained untouched. |
| Original players and saves | Reconstructed the missing test directory with fresh destination-DPAPI blobs, identical underlying identities and exact save bytes. All four original clients joined; positions, toys and retained receipts matched. |
| Password input boundary | CLI refused redirected password input; no echoed fallback accepted. |

[Portable result](evidence/portable-recovery-2026-09-25/portable-recovery-91.json) · [Existing restore/rollback regressions](evidence/portable-recovery-2026-09-25/server-recovery-91.json) · [Tools/runtime provenance](evidence/portable-recovery-2026-09-25/source-provenance.json).

The existing regression suite additionally passed occupied/stale-decision refusal, byte-exact restore and rollback, invalid archive rejection, recovery-marker startup gating, corrupt-primary repair and missing-directory reconstruction. The first portable development run exposed an incorrect assumption that the existing issuer record had a schema field; that assumption was removed to retain compatibility, and both complete suites then passed. All 148 qualified Unity C# files still match the build 91 source evidence. No Unity rebuild or device installation was needed.

These are **same-account/PC experiments** with source-DPAPI independence tested by disabling those calls. They do not demonstrate another Windows account/computer, independent storage, original physical-device re-admission, crash-proof media or a VPS migration. The live family world and its credentials were not exported or restored.

**Still required for the independent-backup gate:** the parent's selected external drive/other computer, an actual retained backup and passphrase, a restore on another Windows account/computer, original-device re-admission there, and the backup coverage for device-local work/media/signing. The parent web page does not yet expose portable export. G3-REC-05, mobile host recovery, unattended iPad renewal and VPS deployment remain open.
<!-- historical-record-end -->
