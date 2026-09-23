# G1 — project and device foundation

Started 23 September 2026. In progress; no device qualification is complete.

Current bounded task: compile, install and verify the tiny touch/save fixture on the connected 9th-generation iPad. The isolated project, tool launcher and Mac command connection exist. G2 and later game features have not started.

## Boundary and tools

- Root: `C:\Users\sephi\Desktop\Little weeps game`.
- Unity project target: `Unity/FamilyPlayset` under this root.
- Old `Meeps game` remains unrelated. Its connector was read only to diagnose the reported error; no old game assets or settings are imported.
- Windows: Unity 6000.3.6f1 and 6000.5.0f1 already installed. Side-by-side 6000.3.24f1 is now installed and registered in Hub; editor executable reports revision 4e7b9b5b6244. It is not yet device-qualified.
- Candidate 6000.3.24f1 / revision 4e7b9b5b6244 verified against the official release page and release API. Windows exports the Xcode project; the Mac only needs Xcode for the current route. If Unity is later used on the Mac, use the same editor patch. Bloom stays disabled during this foundation work.
- Blender 5.0.1, native MCP protocol 9 / add-on 1.7 responded successfully on 23 September.
- Codex CLI 0.155.0-alpha.16.3 exists under the desktop app's versioned bin directory. That directory is absent from the normal user PATH, explaining the shortcut error. The new resolver locates it directly on each launch. The launcher passed a live Blender-only reconnect with a normal Windows user/machine PATH, and PowerShell syntax validation passed. Both desktop shortcuts now point here.
- Git 2.53.0.windows.1 and Git LFS 3.7.1 available.
- Samsung SM-S948U1: Android 16 / API 36 verified through ADB. No new-game app installed yet.
- A2602 iPad 9: connected by cable, paired and Developer Mode enabled. Live device inventory reports iPadOS 18.6.2 (22G100); this supersedes the earlier reported OS pairing. Native launch is still pending. A2197 iPad 7 and A2484 iPhone remain untested.
- Mac SSH authenticated successfully after correcting authorized_keys from a directory to a file. Live inventory: macOS 26.3.1 (25D2128), arm64, Xcode 26.6 (17F113), developer directory /Applications/Xcode.app/Contents/Developer, about 247 GiB free. No Unity Hub editor directory on Mac.

## Exit checks

| Check | Result |
| --- | --- |
| Correct isolated root | Verified |
| Local Git baseline and source policy | Baseline committed locally as 674c1f6; version 0.0.2 build settings recorded as d021213. Ignore/LFS rules are present. No external backup configured. |
| New tool launcher; exact Unity project verified | Live full shortcut passed from normal Windows PATH. Blender responded; Unity MCP reported the new root, Unity 6000.3.24f1 and ready-for-tools. |
| Matching editor, small package lock and saved profiles | 6000.3.24f1 project and package lock exist. URP 17.3.0, Input System 1.20.0, UGUI 2.0.0, Test Framework 1.6.0; editor MCP 9.7.3 pinned to commit 75fcf10ea5e230e21963298e1942c36504727eba. Saved Build Profile assets pending. |
| Touch/save scene and local-video probe | Bootstrap scene compiles and runs in Editor; button event increments the save and Play Mode restart restores it. No Unity console errors in this check. Physical touch/video checks pending. |
| Android release launch and update retaining seed data | Pending |
| Native launch on each iPad via Mac | Windows iOS exports 0.0.1 and 0.0.2 passed. Mac compiled 0.0.1 but signing requires local keychain interaction. No installed/launch evidence yet. |
| Free provisioning renewal observation started | Pending |
| Build/test scripts and source/artifact evidence | FoundationBuild and Tools/Build-Foundation.ps1 provide explicit Windows/iOS targets, fresh numbered output and build summaries. Windows standalone build passed; iOS export passed. Physical touch, release update retention and local-video probe pending. |

Local Git history is not an off-device backup. Keep private media and signing credentials outside this repository; do not reuse another app's keys. Establish and verify an external recovery destination before family distribution.

## Setup recovery evidence

The first Hub download failed with `socket hang up`; its retry left the editor paused. The completed portion was preserved and resumed from Unity's official download URL in ignored LocalData. The completed installer matched Unity's published MD5, had a valid Unity Technologies SF Authenticode signature, and was installed side-by-side using Unity's documented silent installer. Other editors were not replaced. Verification is in `LocalData/Logs/unity-installer-verification.json`.

Automatic approval review rejected a combined custom template-extraction/module-install command with only `blocked by policy`. It did not run. The narrower standard Unity `-createProject` / `-cloneFromTemplate` command succeeded using the bundled Universal 2D 6.1.6 template in the new project path. The iOS module was installed from Unity's verified, signed installer; successful iOS export confirms it works. Android/Windows IL2CPP/server modules still require setup and qualification.

The Mac initially rejected authentication because `~/.ssh/authorized_keys` was a directory. The user preserved that directory as a backup and created the required key file. Remote inventory succeeds through `Tools/Check-Mac.ps1`. The Windows SSH key and Mac signing key remain outside source control. Xcode now recognizes a valid Apple Development signing identity after its intermediate certificate became available; no custom trust override was applied.

Sources: [Unity 6000.3.24f1](https://unity.com/releases/editor/whats-new/6000.3.24f1), [Hub CLI reference](https://docs.unity.com/en-us/hub/hub-cli-reference), [Codex MCP](https://learn.chatgpt.com/docs/extend/mcp?surface=cli), plus the local installed executables and live Blender/ADB responses recorded above.


Update: standard Unity project creation and FoundationSetup.Create both finished successfully with return code 0. The initial touch/save fixture and core/runtime assembly boundary compile and run. A programmatically invoked button event saved count 1; stopping and re-entering Play Mode restored count 1 and its profile identity. This proves the Editor fixture only, not physical touch or a native in-place update. It uses a small PlayerPrefs smoke-test value, not the final multiplayer world-save system.

![G1 fixture in Unity Play Mode](g1-foundation-preview.png)

## Build evidence — 23 September 2026

- Windows standalone 0.0.1: succeeded via Unity MCP, non-development Mono build, 0 errors / 1 reported warning, about 96 MB. Output: `Unity/FamilyPlayset/Builds/Windows/G1-0.0.1/LittleWeeps.exe`. Native launch has not been checked.
- iOS 0.0.1 export: succeeded, IL2CPP, device SDK, non-development build. Summary: `Builds/iOS/G1-0.0.1/build-summary.json`; Unity reported 0 errors / 0 build-summary warnings. Import logs contain package shader precision warnings, so the full log is retained in ignored `LocalData/Logs`.
- Xcode export archive SHA-256 matched on Windows and Mac: `34e9b942083a3adfd03e0466423f264b53f535777d4868c247bf419bc7b2d830`.
- iOS 0.0.2 export also passed with 0 reported errors/warnings, preparing the in-place update check. Its archive SHA-256 matched on both machines: `fb5e5fb731912a15ba4912a994a093dd3c0c2c439277ede7d9f3853365c84d6d`. It is extracted in the separate Mac `Builds/G1-0.0.2` directory; native compilation of this version has not started.
- Mac destination: `/Users/nayster/Developer/LittleWeeps/Builds/G1-0.0.1/Xcode`. Only the export and build summary were transferred; no Windows Library or unrelated project was copied.
- Mac build: Release / Unity-iPhone scheme / automatic Personal Team signing. Native compilation completed, but the first SSH build failed at signing with `errSecInternalComponent`; a keychain query also returned `User interaction is not allowed`. A local Mac Terminal retry uses `Tools/Build-iOS-Mac.sh` and prompts for keychain access only on the Mac. No password is stored or sent to Windows. Awaiting that build's result. Logs and DerivedData stay under `/Users/nayster/Developer/LittleWeeps`.

Native launches, physical touch, saved Build Profile assets, the local-video probe, release-update data retention and free-provisioning renewal observation remain G1 work. A successful initial Personal Team install does not establish automatic renewal.

Signing references: [Apple guidance for errSecInternalComponent](https://developer.apple.com/forums/thread/712005), [Apple signing intermediate certificates](https://developer.apple.com/help/account/certificates/wwdr-intermediate-certificates). The initial compile also reported placeholder app-icon and generated build-script warnings; a finished app icon is not part of this fixture.

## Reconnect regression and immediate next action

An additional cold-start check exposed MCP 9.7.3 leaving its cached transport session as `pending` even while live project tools worked. The helper now acknowledges transport state, and the launcher additionally uses the project-checked MCP client to require a live project-info response matching the new root. Both a reconnect with apps open and a full Unity close/reopen passed from the normal Windows PATH. No Unity console errors were returned after compilation. The editor-only change occurred after the two iOS exports and does not change their player content.

Current human-input blocker: the Mac's local **Finish-iPad-Build.command** Terminal window is waiting for keychain unlock. The user must enter their Mac password locally and approve codesign key access if macOS asks. After that: inspect the local build result and signature, install 0.0.1 on the connected iPad, verify real taps, relaunch with the same save, then build/install 0.0.2 and verify the same count/profile remain. Do not mark any of those checks passed before observing them. The next test is not game content production.
