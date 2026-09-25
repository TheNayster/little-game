# Future family server on the owner's VPS

**Decision: preferred future hosting target, after the server reliability work passes.** The user owns a VPS and wants the shared world to stop depending on the home PC. This expands **REMOTE-01** and preserves **NET-02**, four mixed-device players, independent exploration, full private offline solo; device hosting and offline imports are removed. This is a deployment plan, not an installed remote service. The last recorded PC server/helper deployment is 91; this audit did not check live availability.

## What the family will experience

Open the game at home or on a road trip, and the app automatically connects to the same enrolled family world on the VPS. No child enters an address or chooses a host. A working hotspot/internet connection can carry travel multiplayer; a VPS cannot supply connectivity in a cellular dead zone. Play by myself stays available with installed content and local saves. Prepared client 98 contains the current private-continuation repair; physical rollout/acceptance remains in G3.

The VPS becomes the canonical shared-world host after a controlled cutover. The PC can remain a development/test machine. Moving the game authority does not automatically upload or stream the personal TV library; offline media remains a separate local-content feature.

## Verified starting point and required work

| Area | Current evidence | Work before VPS deployment |
| --- | --- | --- |
| Server build | Dedicated Windows authority 83 works; four mobile platforms have joined the family prototype | Confirm VPS OS, CPU architecture, resources and allowed network traffic. Build/qualify for that platform; a Linux VPS requires a Linux server build and platform adapters. No capacity estimate is established yet. |
| Automatic joining | Apps currently obtain IPv4 endpoints from native local Bonjour/NSD discovery | Add a parent-provisioned remote endpoint provider with saved hostname/port, DNS refresh and bounded retry. Keep discovery separate from authentication and keep kids out of configuration. |
| Encrypted family admission | `NetworkProbe` uses transport encryption and the enrolled certificate/family/profile identity | Preserve validated server identity and the four-member allowlist; qualify public/private routing, connection limits and invalid admission behavior. A DNS/IP address alone is not proof of server identity. |
| Authority credentials | `WindowsPairingVault` reads DPAPI data tied to the Windows account/machine | Implement controlled credential export/reprotection and a VPS platform store. Copying the `.pairing` files is not a portable migration, even to another Windows computer. No secrets in Git or diagnostic logs. |
| Process operation | Home mode has no timer cutoff; checkpoints and local single-writer locking exist | Add host-appropriate boot startup, bounded crash restart, health reporting, bounded logs and monitored disk use. Windows sign-in helpers are temporary local conveniences, not the VPS deployment system. |
| Durable world | The original family world survived the 79 → 83 PC migration | Quiesce and stop the old authority, verify a backup, transfer a compatible checkpoint, start the VPS once and verify identities/state before switching client routing. Prove restore and rollback. |
| Mobile internet | Current physical admission is on the home LAN | Test all four clients from home and a real mobile hotspot, latency/jitter/loss, network changes, background/rejoin, unreachable server and unavailable internet. Keep responsiveness and late joining independently measured. |

The local code findings above are from `NetworkProbe.cs`, `IFamilyDiscovery.cs`, `WindowsBonjour.cs`, `WindowsPairingVault.cs` and the deployed 83 records. They show that the authoritative game rules can be reused, but that deployment involves more than replacing an IP string.

Unity 6.3 supports dedicated server builds for a selected server platform, including Linux; the appropriate build module is required. This establishes a build path, not compatibility or performance on an unspecified VPS. [Unity dedicated-server build documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server-build.html).

Multicast DNS operates on the local link, so the current Bonjour discovery path does not by itself find an internet VPS. [RFC 6762](https://datatracker.ietf.org/doc/html/rfc6762). Microsoft documents that DPAPI decryption normally depends on the same user credentials and computer; this explains the migration work for our current vault. [Microsoft DPAPI documentation](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata).

## Routing and ownership decisions

**Proposed first connection path:** an app-managed endpoint to an authenticated, encrypted game port on the VPS, provided the host and mobile networks support it. The exact direct/VPN/relay route is not selected until the VPS details and route tests are available. Existing Tailscale/Relay research remains an alternative, not a mandatory extra app for the children. Tailscale itself may use a direct or relay path with different performance; it still requires device setup and qualification. [Tailscale connectivity documentation](https://tailscale.com/docs/reference/connection-types).

Do not let both the PC and VPS accept writes to the same canonical world during cutover. The existing file lock only protects one machine; it cannot prevent two servers on different machines from creating conflicting progress. If cloud connectivity disappears, do not silently treat an old PC copy as the live shared world. Offline continuation remains private; it never becomes a shared host and never imports its edits. A client's inability to reach the VPS does not prove the VPS stopped. Only the designated PC/VPS serves shared state; server restart/restore is a dedicated operation.

## Deployment gate and sequence

1. Finish the current server reliability baseline: save/restore, orderly restart, bounded crash recovery, parent status, and sustained mixed-device operation. Carry the same operational requirements to the VPS.
2. Confirm the owned VPS's OS/version, architecture, RAM, CPU, storage, location and network/UDP access. No credentials are needed for this planning step.
3. Build and test a separate VPS-capable server plus remote-endpoint client configuration against an isolated world. Keep the existing PC family world usable throughout preparation.
4. Prove four-device home/hotspot play, loss/rejoin, offline availability and backup restoration. No promise of zero-delay reconnect or service availability beyond measured results.
5. Schedule a short no-player cutover, migrate the actual world/enrollment, verify it, then switch the family apps to the VPS. Keep a verified rollback checkpoint without running two writable authorities.

This is a follow-on server deployment milestone after the reliability gate; it need not wait for every planned game activity to be authored. G4 is retired. G5 rooms/items and private-save preservation remain open; no offline merge engine is required. Current networking work stays focused on the working foundation.

**Deferred host details:** the user says the owned VPS has sufficient capacity and will provide its details when available. No further sizing discussion or access request is needed now; host OS/architecture and connection information can be confirmed at deployment. This does not block current game/server work. No VPS login, connection, firewall rule, upload, purchase or remote deployment was performed for this decision.
