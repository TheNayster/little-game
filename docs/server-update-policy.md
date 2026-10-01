# Shared PC server and app-update policy

**September 30, 2026 — installed and running.** Read this file again before every app/server rollout. Multiple chats share this checkout. This applies to Little Weeps, not Things To Do QA.

## Current installation

The selected PC server is release **347**, protocol **3**, content **52**, schema **42**. The compatible Zoo gate-coordinate fix was applied September30 after all players disconnected. It runs from `LocalData/PCServer/current/Server/LittleWeepsNetwork.exe`; installation metadata is in `LocalData/PCServer/installation.json`. Port **63648**, the existing family/save folder, all131 saved items, four enrolled players and enrollment bytes are retained. Automatic recovery is enabled and the parent helper selects347. The DiagnosticFileWriter fix remains included. Samsung and Eduardo’s iPad341 and Gabriel’s iPad342 remain compatible; no device reinstall was needed. All three were previously verified together on340; physical rejoin to347 remains pending. [Rollout and checkpoint review](implementation/zoo-main-gate-arrival-347-2026-09-30.md). Movement348/content56 and other concurrent candidates are not installed. Historical340 and227 evidence remains historical.

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


## September 30 status-write crash correction

The parent reported repeated off/on transitions after deployment. Logs from three stopped 227 instances confirm `IOException: Unable to remove the file to be replaced` in `NetworkProbe.WriteJson`, called during movement diagnostics. Recovery restarted these failed processes; it did not prevent the fault. Earlier helper-reader changes were insufficient, so the first deployment must not be treated as a qualified fix for this crash. The process holding the files is not identified.

Microsoft documents error **1175** (`ERROR_UNABLE_TO_REMOVE_REPLACED`) from ReplaceFile as failure to delete the replaced file. The existing newer source caught only 32/33, missing this outcome. `DiagnosticFileWriter` now skips a disposable observation for sharing/locking or incomplete replacement errors **32, 33, 1175, 1176, 1177** and tries again at the next normal update. It keeps the game loop running and retains the previous observation. It does not sleep, delete the old target, bypass admission or change `CheckpointStore`. Permanent failures such as a missing directory or full disk still propagate. See [Microsoft ReplaceFile](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-replacefilew) and [File.Replace](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.replace).

**Installed hotfix `227-status-io-1`:** exact baseline networking source hashes were recovered from Git (including line-ending verification), and only `WriteJson` plus the new writer were recompiled with the matching Unity editor's Roslyn compiler against baseline managed references. No current candidate gameplay was substituted. Only `LittleWeeps.NetworkProbe.dll` changed. Core, save rules, serialized game data, protocol 3/content 31/schema 30 and application identity 227 remain. Build/provenance, changed-artifact manifest and receipts are under `Builds/ServerHotfix/227-status-io-1/`; installed metadata records `statusHotfix`, and the installed artifact manifest records the new assembly hash. Original build 227 is immutable; the installed bundle includes `status-hotfix.json` explaining its explicit assembly change.

The four-client native test deliberately held the server's view file for two seconds, recorded **48** conflicts, retained one authority and all clients, then verified independent departure. On the installed live authority the same test recorded **51** skipped writes with unchanged PID/instance and two devices connected before and after. Standalone tests using Unity's Mono reproduce a real locked-file error, verify retry after unlock, cover all five transient error codes and require permanent I/O errors to propagate. This is focused acceptance of the reported failure, not a guarantee against every crash. [Isolated evidence](implementation/evidence/pc-server-reliability-2026-09-30/status-hotfix-validation.json) · [Live evidence](implementation/evidence/pc-server-reliability-2026-09-30/status-hotfix-live-validation.json) · [Publication evidence](implementation/evidence/pc-server-reliability-2026-09-30/status-hotfix-deployment.json).

The parent explicitly authorized disconnecting idle test iPads while the kids were at school. The game exited gracefully, the fix was published, saved-world bytes were unchanged during publication, and it restarted on port 63648 with the existing firewall rule. No administrator prompt or device app update was needed. Subsequent chats must preserve this hotfix or include the source fix when intentionally replacing this server. Do not redeploy unpatched 227 to the persistent slot. Routine compatible app updates still leave the server alone.

**Established hosting options also reviewed:** [WinSW](https://github.com/winsw/winsw) and [NSSM](https://www.nssm.cc/usage) offer service lifecycle/restart management; Unity's [NetworkManager/UnityTransport](https://docs-multiplayer.unity3d.com/netcode/2.0.0/components/networkmanager/) already supplies the game's networking framework. These wrappers cannot repair exceptions inside game code. Neither was installed during this crash fix. Switching to a Windows service needs the existing account-bound enrollment, graceful save/stop and parent intent preserved; do not run a second authority or move credentials to LocalSystem implicitly. Keep the existing sign-in hosting working until any replacement is concretely qualified.

## Zoo main-gate arrival fix347 review

The three changed shared files correct only authoritative Zoo arrival coordinates within the existing bounds; the unchanged341-client four-player arrival/return/departure test passes. Protocol3/content52/schema42 remain compatible. This is an explicitly reviewed server bug fix for the user’s gate-spawn request, eligible for `--replace-compatible`; the digest planner reports review-required as intended. [Review/evidence](implementation/zoo-main-gate-arrival-347-2026-09-30.md). Publication still requires an idle family session and retains the installed endpoint/enrollment/recovery. This review does not authorize replacing occupied play.
