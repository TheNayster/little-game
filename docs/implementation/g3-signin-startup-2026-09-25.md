# Optional Windows sign-in startup

**G3-OPS-08 · NET-02 / AUTO-01 · September 25, 2026.**

**Status: isolated Windows and browser acceptance passed.** This extends the parent helper without changing prepared Unity build 91. No startup entry was installed in the actual Windows account's Startup folder, and the live family helper/server, Mac and physical devices were not changed.

## Parent behavior

The parent page adds **Add sign-in startup** and **Remove sign-in startup**. Adding is available only when the selected server is running the intended build, its save is verified, its network setup is ready and automatic recovery is healthy. The selected settings must match the current helper. A changed or unrelated shortcut with the same name is left untouched.

The shortcut launches the helper for this Windows account, without opening a browser. Existing recovery preferences control what happens next: an authority that should be running can recover from its existing save, while a deliberate **Stop** remains stopped. **Pause recovery** suppresses sign-in helper launch; manually reopening the parent page still works. Removing the shortcut leaves current players and the running recovery helper alone.

The page reports **Shortcut configured**, not proof that a real sign-in has succeeded. Windows settings can separately disable a startup app. This is a sign-in helper, not a service that runs before login or a way to wake a sleeping/off PC. Microsoft's supported current-user Startup-folder mechanism and WSH shortcut properties are the implementation basis. [Windows startup guidance](https://support.microsoft.com/en-gb/windows/experience/startup-boot/configure-startup-applications-in-windows?nochrome=true), [WSH shortcuts and arguments](https://learn.microsoft.com/en-us/troubleshoot/windows-client/admin-development/create-desktop-shortcut-with-wsh).

## Launch and save guarantees

- Shortcut target, arguments, working directory, description and window style are checked after creation and before removal. Publication refuses an existing file. Isolated fixtures register only beneath their own test folder.
- HTTP actions require the local capability and Host/Origin checks, accept no caller-supplied paths/commands and never mutate state through GET.
- A short launcher lock serializes desktop/sign-in launches; a separate lifetime lock prevents a duplicate helper service. Reuse checks the family, selected build, helper protocol, PID and live local response. An older or mismatched running helper is not killed or silently reused.
- Each long-running helper owns its `uv` process/environment. A short-lived launcher cannot leave it dependent on a deleted temporary environment. Cached dependencies are used offline. Missing dependencies fail without creating a replacement family.
- Sign-in resumption uses the existing bounded supervisor and saved parent intent. It does not reset retry limits, replace enrollment, bypass network/save verification, or force-stop players. The native authority retains its own save lock and occupied-stop check.
- A startup failure before the native game creates a log now leaves bounded launcher stderr in the private world folder. Credentials and these private records are excluded from Git and parent HTTP responses.

## Evidence

| Check | Observed result | Evidence |
| --- | --- | --- |
| Native startup suite | Six groups passed: authorization and readiness boundaries; real `.lnk` create/inspect/remove; duplicate and stale-helper handling; deliberate Stop; recovery after helper and authority loss; Pause/removal. | [Native result](evidence/signin-startup-2026-09-25/native-startup.json) |
| Recovery through the actual shortcut | The test shortcut launched PowerShell and the offline helper. The saved world recovered and all four original Windows players rejoined in **19.33 seconds**. Player/toy/receipt state was retained. This is one observation, not a latency guarantee. | [Native result](evidence/signin-startup-2026-09-25/native-startup.json) |
| Existing parent operations | All four native/HTTP regression groups passed, including backup, crash recovery and deliberate lifecycle choices. | [Operations result](evidence/signin-startup-2026-09-25/parent-operations-regression.json) |
| Existing portable backups | All six HTTP/native regression groups passed. The existing file format and reconstruction behavior are unchanged. | [Portable result](evidence/signin-startup-2026-09-25/parent-portable-regression.json) |
| Browser acceptance | With four test players present, Add was unavailable while recovery was Off. Enable recovery made Add available; Add showed **Shortcut configured**; Remove returned it to **Off** while recovery remained **Watching** and all four players remained present. The panel layout was inspected; the isolated supervisor was paused for cleanup. | [Browser record](evidence/signin-startup-2026-09-25/browser-acceptance.json) |
| Source and documentation | All 148 Unity C# files still match the prepared Android 91 source manifest. Tool hashes and rendered chapter/feature/link validation are retained. | [Source hashes](evidence/signin-startup-2026-09-25/source-provenance.json), [Documentation validation](evidence/signin-startup-2026-09-25/docs-validation.json) |

The first implementation exposed a real startup defect during the isolated native test: after the short-lived launcher's `uv` environment exited, a later server start failed with `No pyvenv.cfg file`. The helper now owns a persistent offline `uv` process instead of borrowing the temporary launcher's Python executable. The corrected six-group run includes manual Start after helper relaunch and recovery after losing both processes. The [failed run](evidence/signin-startup-2026-09-25/initial-native-failure.json) and [diagnosis/fix](evidence/signin-startup-2026-09-25/failure-resolution.json) are retained; this was found before deployment to the real family helper.

These tests exercised native Windows shortcuts in disposable family folders. They did **not** log out, reboot Windows, install the real account's startup entry, update a device or prove physical mobile background/recovery behavior. All test authorities, clients and helpers were closed afterward.

## Remaining deployment

When the user returns, qualify the native Apple build on iPad 7 first, then coordinate the family server/client update. Retire the old control helper deliberately while preserving the game and saved recovery intent. The new launcher refuses to replace a still-running older helper automatically.

After the selected qualified authority and recovery are active, add the real sign-in shortcut if wanted, then verify an actual Windows sign-out/sign-in or reboot. That account-startup test has not been performed. Independent backup/another-account restoration, unattended iPad renewal, physical sustained measurements, parent pairing, required iPad hosting and reconciliation retain their existing gates.

[Return checklist](return-checklist-ipad-lan-2026-09-24.html) · [Current plan](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis).
