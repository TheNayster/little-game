# PC server reliability and compatible app updates

**September 30, 2026 · NET-02 / AUTO-01; REMOTE-01 remains deferred.** Applied research for the user's clarified request: family play should stay available while the home PC is awake and connected, and an app update that remains compatible should leave the running server, endpoint and Windows permission alone. This is a researched implementation contract, not a deployed fix. No server, save, firewall, startup registration or device was changed. Preserve the current branch integration hold and the full Home backlog.

## 1. Recommended approach

Keep the existing PC as the shared authority. Separate an **app release** from a **server deployment**. A compatible client release installs on the phone/iPads and connects to the existing server; it does not restart, rebuild, relocate or re-enroll that authority. When shared rules genuinely change, update the server through one persistent installation with a reusable network permission and an explicit save migration.

Use the current parent helper and crash recovery as the starting point. Qualify real startup/restart behavior, then add a Windows service if availability before sign-in is required. A PC that is sleeping, powered off or without a usable network cannot serve multiplayer. Full private solo and server-wins reconnection remain available under the existing policy.

The previously owned-VPS plan is an optional future way to remove dependence on the PC; it is not required for this task. See the [existing VPS plan](vps-hosting-plan-2026-09-24.html).

## 2. Findings in the current source

| Finding | Source | Consequence and fix |
| --- | --- | --- |
| Server builds run from numbered `Builds/NetworkProbe/G3-0.0.N/Server` folders. The firewall rule and verification record also contain N. | `Tools/Start-FamilyLAN.py`; `Tools/Enable-FamilyServerFirewall.ps1`; `Tools/parent_server.py` | Every relocated server needs another exact-path permission. Compatible app releases should keep the existing server. Real server deployments should use a permanent executable path and verify a reusable rule. |
| The launcher defaults to port zero, probes an available UDP port, and advertises that endpoint. | `Tools/Start-FamilyLAN.py` | A server restart can change its port. Persist one selected game port per installation/family; fail clearly if occupied, rather than silently choosing another. Keep discovery so children never enter addresses. |
| Admission compares protocol and shared content exactly; it does not compare app build numbers. | `NetworkProbe.cs`, `Approve` and shared-state validation | Different app builds can already connect if protocol/content match. Do not bump shared compatibility for client-only presentation changes. Do not bypass the comparison for an app that needs new rules. |
| Current candidate 229 declares schema 32/content 33. The parent helper's checksum/status reader accepts schemas only through 27. | `Core/WorldLayout.cs`; `Tools/parent_server.py`, `checkpoint` | The helper can call a newer valid save unverified and refuse restart. This is a source mismatch, not evidence of a corrupt live save. Update helper support with the affected game contract and a focused retention/start check. Never accept arbitrary future schemas. |
| Recovery qualification is an explicit per-build set; the inspected set stops at candidate 205. | `Tools/server_recovery.py` | Recent gameplay builds cannot be called qualified for that independent recovery path merely because they compile. Keep one affected-contract check when preparing an actual server replacement; do not repeat unrelated full suites on app-only installs. |
| Existing startup is an opt-in current-user Startup-folder shortcut. | `Tools/parent_startup.py`; `Tools/parent_bootstrap.py` | It resumes after sign-in, not before login, and cannot wake the PC. A service needs a real service wrapper and explicit credential access, not a renamed Unity executable. |
| Existing supervision restarts an exited server with bounded retries, preserves deliberate Stop/Pause, and does not kill a hung process. | `Tools/server_supervisor.py`; `Tools/parent_operations.py` | Reuse these protections. Add separate recovery for a failed helper and qualify hang detection before enabling it. A running process is not proof that family clients can play. |
| Enrollment is protected with Windows DPAPI for the current account. | `Tools/family_pairing.py`; Windows pairing vault | Moving execution to another service account requires deliberate credential reprotection. Running as LocalSystem or copying protected files is not a complete migration. |

These are source findings, not a live availability assessment. Existing reports last record server/iPads at 171 and Samsung at 227; candidate 229 is prepared, not deployed. Do not use the old September 26 handoff's deployment versions as current facts.

## 3. Which updates need the server?

| App change | Required release behavior |
| --- | --- |
| Pictures, music, menu layout, animation presentation or a local display fix, using unchanged shared messages/rules | Keep protocol/content compatible; install the app only. Preserve the running server process, address/port, firewall, enrollment and helper settings. |
| Local-only feature that consumes the existing shared state | App-only if existing messages and required state are sufficient; check that boundary once. |
| New shared activity, authoritative physics/rules, object state or saved-world fields | Prepare a coordinated server/client release and any additive save migration. An older server cannot calculate rules it does not contain. |
| Server bug fix with unchanged client contract | Update the server alone if older clients remain compatible; prove that one compatibility case. |

The update tool should compare the incoming release's declared protocol/content with the **selected running server**, not assume equal build numbers or choose the newest server directory. It should report `app-only`, `server-update-required`, or `unknown`. Missing or unverifiable metadata means unknown; it is not permission to restart or replace the server. A compatible app update must not change `latest-server.json`, helper selected-build settings, the authority process or its network permission.

Exact matching is a conservative existing contract. Capability negotiation can eventually allow older clients to keep playing supported features during staged rollouts, but it requires supported message/state schemas and UI behavior. Simply removing the rejection would introduce malformed state or incorrect gameplay. Preserve exact matching initially and improve release classification first.

## 4. Persistent installation and permission

Stage each verified complete server bundle in a release area. Only an actual server deployment publishes it into a stable `current/Server/LittleWeepsNetwork.exe` location. Keep mutable saves, enrollment and logs outside that replaceable bundle. Record build, protocol, content, schema and artifact provenance in installation metadata.

Change every process-path consumer together: launcher, parent process verification, restart tools, firewall verification and startup settings. They currently recognize numbered build paths. Changing only the firewall script would leave parent controls unable to identify the new authority.

Publish a bundle only after the old authority exits cleanly in an idle window. Replace the whole bundle on the same volume; do not overwrite loaded DLLs or delete save folders. If startup fails before the new build writes, the old executable and its unchanged checkpoint can resume. Once a new schema has been saved, an older executable must not open that newer save: restoring a pre-update checkpoint is a separate explicit recovery decision because it can lose later progress.

Use one application-scoped UDP firewall permission at the permanent path, limited to the intended home network. Verify the actual Windows rule on deployment; an old JSON success marker does not prove the rule still exists. Initial rule creation needs administrator approval. Compatible app updates need no firewall operation. Ordinary server bundle changes at the same path should reuse the rule; policy changes or a removed rule can still need repair. Microsoft documents full-path application rules and administrator-managed permissions. [Windows Firewall rules](https://learn.microsoft.com/en-us/windows/security/operating-system-security/network-security/windows-firewall/rules).

Application Control/code trust is separate from firewall access. Prior reports record a blocked independent validator. A stable network permission cannot fix that block. Inspect an actual policy rejection and use an approved trusted build/signing route; do not disable Windows security or mark an unstarted validator as passed.

## 5. Stable connection settings

Persist a chosen available UDP game port; test it before opening a save. Continue Bonjour discovery with family identity and compatibility metadata, then authenticate the discovered server. A fixed port makes setup predictable; discovery makes an address change invisible to the child when the LAN supports it. Multicast DNS is local-link discovery, not a general internet routing service. [RFC 6762](https://datatracker.ietf.org/doc/html/rfc6762).

An optional router DHCP reservation keeps the PC's LAN address consistent. No router change was made or its model inspected. [Microsoft DHCP reservations](https://learn.microsoft.com/en-us/windows-server/networking/technologies/dhcp/dhcp-scopes).

The phone addresses previously supplied for installing builds, such as `192.168.1.126:43695` and `192.168.1.126:33643`, changed port rather than IP. They belong to Android wireless debugging, not the game server. A game-server fix cannot freeze Android's debugging port. Use paired ADB discovery or USB for installation separately. [Android wireless ADB](https://developer.android.com/tools/adb).

## 6. Running while the PC is on

For ordinary signed-in PC use, resume the existing helper and its saved recovery choice at sign-in. A focused real startup test must show that the helper and server both return and reconnect clients. Respect a deliberate parent Stop or Pause. A sign-in shortcut alone does not prove boot-time availability.

If the server must start even before anyone signs in, install a Windows service wrapper with automatic startup and restart-on-failure. Installing/configuring that service is a one-time administrator operation. The Unity process is a child of the wrapper; it does not already implement the Service Control Manager protocol. Preserve account-bound enrollment through a supported account/profile arrangement or controlled reprotection. Windows documents service hosting, automatic start and failure recovery; DPAPI normally depends on the protecting user and machine. [Windows service hosting](https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service), [service configuration/recovery](https://learn.microsoft.com/en-us/windows/win32/api/winsvc/nf-winsvc-changeserviceconfig2w), [DPAPI](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata).

Supervise both helper and game. Distinguish process exit, stale game heartbeat, unavailable LAN, disk/save failure, intentional Stop and an incompatible client. Only a verified dead authority can be restarted automatically. A proposed hang recovery must first request graceful shutdown, verify the original process has ended, and preserve the last valid checkpoint and single-writer lock. Do not start a second writer because a probe timed out.

Use bounded backoff and a clear needs-attention state for repeated crashes or invalid saves. Restarting forever is not recovery. Check listening state, a fresh game-loop heartbeat, checkpoint health and one real client join, rather than only PID or open UDP socket. External behavior and internal monitoring answer different questions. [Google SRE monitoring](https://sre.google/sre-book/monitoring-distributed-systems/).

Power management is part of this requirement: Windows sleep suspends service execution. Any change to sleep settings must be explicit and limited to the intended plugged-in PC mode; do not silently change power settings. PC/network outages and Windows restarts still cause downtime. Clients continue private solo, retry connection independently, and load server state on rejoin without importing offline edits.

## 7. Implementation order and focused proof

1. **Release classification first:** add a read-only compatibility plan to the existing update flow. Prove that a newer compatible client connects to an unchanged server and that an incompatible client is reported without stopping it. This removes unnecessary server work immediately.
2. **Persistent server deployment:** implement stable bundle/port metadata and reusable firewall verification; update all path readers together. In a disposable enrolled world, replace one idle server release and verify retained state and one authority. Repeat start and a compatible app install must preserve endpoint and permission without elevation.
3. **Helper contract support:** replace lagging helper checks with explicit supported/qualified release metadata and one affected-schema retention/restart test. Do not broadly accept all schema integers or relax validation to make a warning disappear.
4. **Availability while awake:** qualify current sign-in recovery, then a service wrapper if before-login availability is wanted. One targeted crash/boot check must reconnect four representative clients; one departure must not restart the authority or interrupt siblings. Respect deliberate Stop/Pause and do not change sleep policy implicitly.
5. **Production rollout:** stage the prepared change and apply it while no family players are connected. Preserve saves/enrollment. Record what is actually installed. Later compatible app updates use only the app-install path.

Acceptance target: app-only updates leave the same live server instance and endpoint running; actual server replacements reuse the same installed path/port/permission; the desired running state recovers after ordinary process failure and startup while the PC is awake. No claim of zero outages or unlimited uptime.

## 8. Evidence and next task

Research consists of source inspection and the primary documentation linked above. [Source inventory](evidence/pc-server-reliability-2026-09-30/source-audit.json) records the inspected file hashes and scope. No runtime, boot, firewall or four-client acceptance was performed for this research. The next bounded implementation is release classification and an unchanged-server compatibility check. It does not require a VPS, another Home activity, a new family identity or a blanket full-game qualification.
