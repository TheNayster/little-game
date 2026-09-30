# Shared PC server and app-update policy

**September 30, 2026 — installed and running.** Read this file again before every app/server rollout. Multiple chats share this checkout. This applies to Little Weeps, not Things To Do QA.

## Current installation

The selected PC server is release **227**, protocol **3**, content **31**, schema **30**. It runs from `LocalData/PCServer/current/Server/LittleWeepsNetwork.exe`, with installation metadata in `LocalData/PCServer/installation.json`. Port **63648** is retained across launches. Saved worlds/enrollment remain in their existing `LocalData/FamilyLAN/<family>/` folder; they were not reset or relocated. Parent-helper selection was corrected from stale 171 to installed 227.

The reusable Windows rule **LittleWeeps-Home-UDP** was created and verified after the one-time administrator approval. It permits only UDP for this permanent executable from LocalSubnet on Private/Public profiles. Compatible app updates need no new firewall rule or administrator prompt.

Current-user Windows sign-in startup, native crash recovery and helper watchdog are enabled. A locked but awake, connected PC can host. Availability begins after this Windows account signs in; pre-login hosting and physical reboot are not qualified. Sleep, power-off or a lost network interrupt hosting. No sleep settings were changed. Deliberate parent Stop/Pause is respected. Repeated failures exhaust a bounded retry budget and need attention; this is not a promise of fault-free operation.

## Every app update

1. Read the actual installation and live parent status; do not use a chat's cached build/endpoint. A source candidate is not the installed server.
2. Run `python Tools/Plan-FamilyUpdate.py --client-build N --platform android` for Android, or omit platform for the corresponding compiled Windows release. For iOS, review its exact matching Windows shared-source release; no iOS metadata shortcut is qualified here.
3. **app-only:** install the app in place. Leave server process, selected build, port, firewall, enrollment and startup/recovery settings untouched. Different app/server build numbers are allowed.
4. **server-update-required:** shared protocol/content changed. Prepare one coordinated server/client rollout. Leave the current server running until that rollout is ready and family play is idle.
5. **review-required/unknown:** do not infer compatibility or replace the server automatically. Check shared-source provenance and the reason for the change. Never bypass admission checks.

The Android installer now prints this classification; it never deploys a server. It can still install an incompatible private preview, but that preview cannot join the older server until coordinated deployment. Candidates 229–231 use content 33 and therefore do not join installed server 227/content 31. Previously recorded older iPads/iPhone also need matching compatible releases; no devices were updated by this task.

Client-only artwork, sound and menu changes keep shared compatibility unchanged. Shared rules/messages/save changes need an explicit reason for their compatibility bump. Do not routinely rebuild or deploy a server just because the app version increased.

## Server-only operations

- `uv run --offline --with cryptography python Tools/parent_bootstrap.py --no-browser` resumes the selected parent helper. Its normal dashboard controls start/stop/recovery. Do not stop occupied play.
- `powershell.exe -NoProfile -File Tools/Enable-PCServerFirewall.ps1 -VerifyOnly` reads the actual reusable OS permission without elevation or mutation.
- Initial publication: `uv run --offline --with cryptography python Tools/Install-PCServer.py --build N`. Existing enrollment is required; running authority publication is refused; saves are preserved. This does not itself start the server.
- Intentional idle server upgrade: `uv run --offline --with cryptography python Tools/Restart-FamilyServer.py --build N --family <selected-family>`. Compatible app-only requests return **server-unchanged** before any process/settings/firewall/save changes. `--replace-compatible` is only for an explicitly reviewed server fix with unchanged compatibility, never a routine app installation.
- Re-read the parent helper selection after an actual server upgrade. An older live helper is refused rather than silently adopted; retire only its verified helper process and reopen it while keeping the game/world intact.
- An `installation.pending.json` journal blocks ambiguous publication/start. Preserve all bundles and saves and inspect the journal before explicit repair. There is no automatic bundle rollback: a newer server may already have written a newer schema.

Permanent endpoint discovery uses the existing LAN Bonjour mechanism. The PC's DHCP IP may still change; a router reservation was not configured. Kids should not enter a fixed game IP. Android wireless debugging endpoints are separate from the game endpoint.

## Acceptance and remaining limits

Seven focused tests pass: compatible different build classification; stable path/port/save preservation; occupied publication refusal; newer-save/occupied-port refusal; tampering/incomplete-install blocking; old Android provenance; Windows status readers permitting atomic replacement. Real deployment verifies the OS rule, checkpoint, one live authority, recovery, startup shortcut and watchdog identity. A compatible restart request kept the same PID/instance/port/installation. Two real enrolled players were observed connected, without identifying their physical platforms. A legacy server status-write exit was automatically recovered on the same port; helper status reads now share file deletion to avoid causing that failure. The checkpoint remains verified. No deliberate occupied-server crash drill or device installation was performed.

[Verification evidence](implementation/evidence/pc-server-reliability-2026-09-30/deployment.json) · [Research](implementation/pc-server-reliability-research-2026-09-30.html) · [Current decisions](current-decisions.md) · [Build guide](family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis).
