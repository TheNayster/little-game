# G3 — trusted family discovery: Windows proof

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
**September 24, 2026 · Shared build 70 · Goal IDs: AUTO-01, JOIN-01, FAMILY-01, NET-02.**

The next foundation is implemented and qualified on Windows: four separately enrolled players find the intended PC authority through native Bonjour and enter an encrypted shared garden without a configured server IP. Wrong credentials, unknown profiles, a duplicate profile, incompatible protocol, another family and an untrusted server certificate do not grant shared-world access. An absent server opens a separate usable solo branch.

This is a **parent-provisioned Windows proof**, not the finished parent pairing screen or mobile connection system. The two physical iPads were confirmed connected to the Mac, but remain on their previously tested solo build 56. No iPad installation or physical multiplayer pass is claimed. The user's four-window build-68 preview and saved world remain running independently.

## What changed

- Added separate family, authority, world and player identities. The authority stores four independent credential hashes. Each player enrollment contains only its own random credential and the enrolled certificate authority; it contains no server private key or other players' credentials.
- Added a parent-operated, create-only enrollment tool, `Tools/family_pairing.py`. It generates the certificates and independent credentials, then protects the records with Windows per-user DPAPI under ignored `LocalData/FamilyLAN`. It never replaces an existing enrollment or a saved world. This CLI is the current development setup, not the eventual child-facing flow.
- Added a bounded native Windows Bonjour adapter using the existing installed Bonjour service. It advertises `_lw-playset._udp` with public opaque IDs and compatibility versions, resolves the enrolled authority, polls without blocking the render thread, and releases native handles when finished. Advertisements contain no player names, admission credentials or private keys.
- An explicit protected enrollment enables Unity Transport encryption. The client validates the authority certificate against its enrolled CA and server name before NGO sends its admission payload. Discovery alone never grants access. Enrollment is checked again against family, authority, world, profile and credential inside that encrypted connection.
- Kept the old loopback path bound to `127.0.0.1`; adding LAN support does not expose the plaintext lab protocol on the network.
- Added an isolated offline branch for failed joining. Shared checkpoints and the already qualified unpaired solo save are separate. The offline scene has its own camera/audio setup and retains normal touch, narration, saves and settings behavior.

The existing server/world rules, movement interpolation and exclusive item rules are reused. This change does not create a second set of multiplayer gameplay rules.

## Evidence

| Check | Actual result and scope |
| --- | --- |
| Native Windows client/server builds | Release-configured shared **70**, both outputs succeeded; source and artifact manifests retained. [Build summary](evidence/family-lan-2026-09-24/build-summary-70.json), [source manifest](evidence/family-lan-2026-09-24/source-manifest-70.json). |
| Core rules and native discovery | **45 checks passed**: 41 previous state/save/movement checks, three family/admission checks, and native Bonjour advertise/browse/resolve/dispose. The native check is opt-in with `--bonjour`; ordinary rules do not require a Bonjour installation. [Results](evidence/family-lan-2026-09-24/rules-45.json). |
| Four encrypted clients | **11 checks passed** on build 70, including auto-discovery from deliberately incorrect configured client ports, four visible players, independent travel, bucket ownership, rejected admissions, relaunch into the unchanged live authority and absence of credentials in plaintext diagnostic/config files. [LAN qualification](evidence/family-lan-2026-09-24/lan-70.json). |
| Usable offline fallback | With the authority absent, the actual Unity Input System verification passed mouse fill/pour/cleanup, simultaneous touch joystick plus dragging, menu/OS touch cancellation, narration and layout. This uses a fresh verification branch; it does not merge offline changes back into shared play. [Input result](evidence/family-lan-2026-09-24/offline-input-70.json). |
| Cross-computer advertisement | The Mac's native `dns-sd` browser found the Windows game advertisement over the home network. This proves cross-computer discovery only; it is not a mobile data-channel or firewall/gameplay test. [Mac observation](evidence/family-lan-2026-09-24/mac-discovery.txt). |
| Existing loopback protocol | **10 checks passed** on intermediate build 69, including rejected admission, four clients, held-item races, departure/rejoin and checkpoint restart. Final build 70 adds fallback scene audio/camera and callback cleanup; it does not change admission or gameplay rules. [Regression](evidence/family-lan-2026-09-24/network-69.json). |
| Shared touch/presentation regression | **10 checks passed** on final build 70, including pending pickup, remote held props, contention, joystick plus dragging, cancellation, departing/rejoining players and the existing server-loss UI. [Final UI result](evidence/family-lan-2026-09-24/ui-70.json). |

### A regression test corrected during this work

The first shared UI regression on 70 checked the remote bucket after a fixed 150 ms sleep, although remote poses intentionally use the already established 180 ms presentation delay. Its recorded sample showed the correct holder and the bucket approaching the requested position (469.4 versus 480), not a lost pickup. The harness now waits for **both** target coordinates and the correct holder with a 1.5-second deadline. Gameplay timing and tolerances were not relaxed. The [initial failure](evidence/family-lan-2026-09-24/ui-70-initial-failure.json) is retained; all 10 cases then passed on the same unchanged build 70.

## Sources checked before implementation

Unity states that connection approval by itself does not protect connection data. That is why the LAN path requires certificate-validated encryption before sending its player credential. [Unity connection approval](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/connection-approval.html)

The transport exposes `UseEncryption`, `SetClientSecrets` and `SetServerSecrets`. Their implementations were checked in the installed NGO **2.13.2** source, alongside the patched UTP **2.7.4** secure-transport documentation. The online 2.13 API now identifies itself as 2.13.3; reading it did **not** upgrade our pinned packages. [UnityTransport API](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/api/Unity.Netcode.Transports.UTP.UnityTransport.html)

Apple's DNS-SD header specifies callbacks, network-byte-order ports and the requirement to wait for a readable socket before processing results. The Windows adapter follows that native API and uses nonblocking `WSAPoll`; it does not implement its own multicast discovery protocol. [Apple DNS-SD API](https://github.com/apple-oss-distributions/mDNSResponder/blob/main/mDNSShared/dns_sd.h), [Microsoft WSAPoll](https://learn.microsoft.com/en-us/windows/win32/api/winsock2/nf-winsock2-wsapoll)

Windows DPAPI ordinarily protects data to the same user and computer. This storage adapter is intentionally platform-specific and does not silently downgrade to plaintext if a protected record cannot be opened. [Microsoft CryptProtectData](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata)

Apple's local-network privacy guidance was checked for the next device adapter: declare the local-network explanation and the Bonjour service types used by the app, and test allowed/denied permission behavior on real devices. The Windows result does not qualify those permission flows. [Apple TN3179](https://developer.apple.com/documentation/technotes/tn3179-understanding-local-network-privacy)

## Remaining limits and next bounded task

1. **Native iPad client adapter and protected parent enrollment**: iOS Keychain storage, native Bonjour browse/resolve, declared service/permission keys, signed in-place build, then both connected iPads joining the PC. Android requires its own NSD/keystore adapter. None of those is implemented by the Windows DLL wrapper.
2. **Everyday connection orchestration**: immediate local play while searching, ongoing foreground retries, route/interface changes, alternate endpoints, suspend/resume, and reconnect after an established session drops. This proof searches on launch, waits at most 10 seconds for discovery plus up to 10 for admission, and falls back; it does not continuously seek/rejoin or transfer a healthy session. Relaunch is still explicit in the harness.
3. **Parent experience and credential lifecycle**: enrollment/approval UI, device removal, recovery/rotation and certificate renewal. The development server certificate expires after one year; the CA after two. Windows DPAPI files are not a portable family backup. No secrets are bundled in game builds or Git.
4. **Network/device acceptance**: IPv4 only in this first adapter; IPv6, wrong/stale advertisements, permission revocation, route changes, native phone layouts, four actual mixed devices and A10 sustained performance remain open. The home Wi-Fi currently has a Public Windows network profile; no network category or firewall rule was changed during this proof.
5. **G4/G5 requirements remain mandatory**: both iPads hosting, automatic host recovery, full recovery replication, safe offline reunion, rooms and item-return rules. A visible client snapshot is still not a complete recovery checkpoint. The new offline branch must not be silently promoted over the shared world.

The current proof still uses the isolated G3 harness (including its bounded process lifetime and development control/evidence files). It is not a production always-on server installer. Unavailable/corrupt enrollment is a setup error; the eventual unpaired parent setup screen is not implemented. No G1, G2 or G3 gate is declared complete.

## Reproduce the bounded checks

Run from this game's root in PowerShell. Use a fresh build number/output directory when rebuilding.

```powershell
dotnet run --project Tools/SoloRules.Tests -- LocalData/Verification/new-lan-rules --bonjour
uv run --with cryptography python Tools/Test-FamilyLAN.py 70
uv run python Tools/Test-SharedGarden.py 70
```

The tests start and stop only their own verified binaries with fresh saves/enrollments. They do not use the live four-window preview or normal game saves. The optional create-only parent enrollment command is `uv run --with cryptography python Tools/family_pairing.py`; its encrypted outputs are local setup material, not Git artifacts.
<!-- historical-record-end -->
