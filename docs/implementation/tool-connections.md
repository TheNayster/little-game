# Connecting the game tools

Use the desktop **Connect Little Weeps** shortcut. **Connect Unity + Blender** now points to the same new launcher. The original shortcut is backed up in ignored `LocalData/Connect Unity + Blender.original.lnk`; the old project's script and assets have not been edited.

From any Windows PowerShell window:

```powershell
& 'C:\Users\sephi\Desktop\Little weeps game\Connect-GameTools.ps1'
```

Optional flags: `-BlenderOnly`, `-UnityOnly`, `-NoLaunch` (connect to already-open applications), `-CheckOnly` (read-only preflight).

The launcher resolves the installed Codex executable each time, including the desktop app's changing versioned directory. It does not depend on a `codex` entry in the normal user PATH. It verifies Blender's response, requires a fresh Unity acknowledgment, and reads live project info through MCP to confirm this project's exact path. A listening port alone never counts as a connected Unity project. Both reconnecting with Unity open and launching from a closed editor passed on 23 September.

Blender's separate Little Weeps reconnect helper watches `~/.little-weeps-tools/blender-connect.request`. It starts the existing MCP add-on without opening or saving a `.blend` file. Unsaved artwork stays open. No old-project reconnect request is sent.

The new project is installed, compiled, and its live MCP project-info response matches `Unity/FamilyPlayset` under this root, using 6000.3.24f1. The full launcher passed from a normal Windows PATH. It never falls back to the old Meeps project. Tool registrations being present is different from tools being loaded in an existing Codex session. If a new registration is needed, the launcher requests a Codex restart. For the current session, the project-checked `Tools/unity_mcp.py` client provides working access without restarting Codex.

## Mac build connection

Windows is the source workspace. The Mac is a separate build machine. The user supplied `nayster@eduardos-mbp.lan`; SSH authentication and a read-only inventory now succeed with the dedicated key. The user preserved a mistakenly created authorized_keys directory and installed the key in the required file. Actual inventory is in g1-status.md.

Run `Tools/Check-Mac.ps1` on Windows to execute the read-only `Tools/Mac-Inventory.sh` remotely. The first iOS export has been transferred and hash-verified at `/Users/nayster/Developer/LittleWeeps/Builds/G1-0.0.1/Xcode`. Future transfers stay under this game's Mac directory and exclude Windows `Library`, private media and signing keys. Do not mirror or delete unrelated Mac files. macOS signing keys stay on the Mac; the Windows SSH private key stays outside this repository.

SSH supports remote shell/file/build work. It is not desktop clicking. Codex remote projects/control require their own app connection and applicable permissions: [OpenAI remote connections](https://learn.chatgpt.com/docs/remote-connections). Mac Remote Login setup: [Apple documentation](https://support.apple.com/guide/mac-help/allow-a-remote-computer-to-access-your-mac-mchlp1066/mac).

The first native build compiled but SSH could not access the signing key. `Tools/Build-iOS-Mac.sh` provides a local Terminal build route after export transfer: it takes a build number, Apple team ID and device UDID; unlocks the login keychain interactively; builds Release; and verifies the resulting signature. Passwords are entered only in the Mac's local prompt. This does not alter certificate trust settings or disable keychain protection.

For G2 exports, supply a fourth argument `G2`; omitting it preserves the original `G1` path. The helper validates the export version and prevents concurrent helper builds of the same export with a directory lock. Local signing of solo garden 56 and its iPad 9 update passed on 24 September; see the [current device evidence](ipad-garden-2026-09-24.md). A stale lock after a forced process termination must be inspected before retrying, not blindly removed.

An optional fifth argument, such as `ipad7`, keeps a newly signed device product under the export's `DeviceBuilds/<tag>/DerivedData`, with separate log/exit names. This preserves the previously qualified device product. The default three-/four-argument product location stays unchanged; builds of the same export still share one lock.

Verified continuation: the user completed that local route for 0.0.1; build/signature checks passed and the app installed on iPad 9. Device launch still needs its signing/trust check resolved. A subsequent 0.0.2 build over SSH compiled but failed at signing with `errSecInternalComponent`, so use the local Terminal route for subsequent signing until a supported remote route is separately qualified. The full current result is in [G1 status](g1-status.md).
