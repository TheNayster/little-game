# Offline play, server authority and the remaining walking hitch

September 25, 2026 · G3-REC-05 · NET-02 / AUTO-01 / JOIN-01 / FAMILY-01 / TRAVEL-01

## Current product decision

The user clarified that offline play may differ from online play. **The server's world wins on reconnection.** A local outage does not prove that the server failed: other children may still be playing there. Offline movement, toys and activities run locally without server acknowledgments. Rejoining downloads the current server world and does not upload or replay offline actions. Existing solo saves and saved adventures remain on their device.

This supersedes earlier requirements for automatic merging of arbitrary offline edits, including bedroom edits, into the server world. Connected bedroom synchronization, full solo content, four-player mixed-device play, independent travel and required iPad hosting remain in scope. Recovering a shared host from a complete checkpoint is a separate G4 responsibility.

## Primary research reviewed before editing

| Source | Finding and application |
| --- | --- |
| [Unity client/server topology](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/terms-concepts/client-server.html) | The server has ultimate authority. Family play uses its current world; local divergence does not become shared truth. |
| [NGO disconnect/shutdown lifecycle](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/components/core/networkmanager.html#disconnecting-and-shutting-down) | Disconnect callbacks expose loss and network subsystems stop during shutdown. Use ordinary Unity lifecycle for local presentation. Wait for network shutdown before starting another connection, not before local controls. |
| [Transport timeout configuration](https://docs.unity3d.com/Packages/com.unity.transport@2.7/api/Unity.Networking.Transport.NetworkConfigParameter.html) | Heartbeats/inactivity timeouts detect silent loss. Preserve the existing 2.5-second transport timeout; remove the additional ten-second application wait. Detection still takes time. |
| [Unity reachability](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-internetReachability.html) | An available interface does not prove a route to this server. Cellular availability or a Wi-Fi icon cannot override authenticated transport state. |
| [Apple responsiveness](https://developer.apple.com/documentation/xcode/improving-app-responsiveness), [hangs versus hitches](https://developer.apple.com/documentation/xcode/understanding-user-interface-responsiveness/) | Busy or blocked main-thread work can miss rendering deadlines. Correlate actual frame gaps with work; CPU samples alone do not establish disk waiting or a visible jerk's cause. |
| [Apple filesystem performance](https://developer.apple.com/documentation/foundation/improving-performance-and-stability-when-accessing-the-file-system), [disk writes](https://developer.apple.com/documentation/xcode/reducing-disk-writes) | Repeated serialization, atomic replacement and forced synchronization can increase cost. Overlapping writers are unsafe. Measure our single-writer path; do not replace it blindly with concurrent tasks. |
| [Unity JsonUtility background-thread contract](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/JsonUtility.ToJson.html) | Background serialization supports data that is not concurrently modified. Diagnostic exports copy plain samples, then serialize on a worker. No live Unity objects or world state are passed to it. |
| [Unity File Access Profiler](https://docs.unity3d.com/6000.3/Documentation/Manual/profiler-file-access-module.html) | File duration, frame and thread context support attribution. Measure save/network work against subsequent frame gaps before choosing a hitch repair. |

Scope: Unity 6000.3.24f1, installed NGO 2.13.2, Transport 2.7.4 patch 1. Online NGO 2.13 currently serves 2.13.3; no package update is part of this task. Unity does not prescribe this game's offline merge policy. Removing the extra wait follows the user's requirement and the documented separation of networking from local presentation.

## Confirmed code and physical observations

Build **93** on iPad 7 continued offline without reopening, but controls returned only after **10+ seconds**. The user reports smoother motion between jerks roughly every two seconds, with both tap and joystick. Dragged objects were already smooth.

`OutageClock.DelaySeconds=10` imposed a foreground wait after loss. It was an application policy, not a reconciliation requirement. **94 source removes this wait.** Already-local play stays local while discovery runs; a disconnected shared presentation triggers continuation. Foreground resume can continue immediately after known loss. Native checks must still measure loading and total detection-to-play latency.

The implemented reunion path never replays offline changes to the server. It saves the local branch separately, then presents the current server state.

## Actual iPad profile and limits

The PID-attached recording failed. A second all-process Time Profiler recording completed after the SSH timeout: **35.772590 seconds**, iPad 7 / iPadOS 18.7.10, LittleWeeps PID 1022 / build 93. Analysis filters to this app. Its UnityFramework dSYM UUID matches the captured binary: `38467A64-ACBD-3CEC-ACA1-168C10DCAD4B`.

There are **21,363 app CPU samples**, including **6,962 main-thread samples**. Symbols identify rendering, local Update/idle work, network diagnostics and Bonjour discovery. No sampled stack names contain save/checkpoint functions. **This recording does not establish autosaving as the cause.** It lacks synchronized walking input and file-wait measurements; inclusive stack counts overlap and are not frame timings.

94 adds bounded in-memory frame history: gap, local/shared state, walking state, position, collection counter, save duration and network-update duration. Export happens on Menu/pause or an isolated test request, using detached data on a worker. It adds no recurring diagnostic file write during walking. A frame's gap covers the preceding interval: work in frame N must be compared to the gap entering N+1. This measures the issue; **the hitch is not yet claimed fixed**.

## Qualification

Installed: iPad 7 **94**, iPad 9/iPhone **79**, Samsung **83**, authority/helper **91**. The 94 update succeeded. iPadOS initially denied launch with its signature/entitlements/profile-trust security message; the user reported that internet was required. A subsequent USB inspection verifies the actual **0.0.94** runtime, paired enrollment and loaded local adventure. The initial launch block has cleared; physical walking and repeated offline-launch acceptance remain pending.

Apple's [provisioning update documentation](https://developer.apple.com/help/account/provisioning-profiles/provisioning-profile-updates) describes first-launch PPQ certificate verification for development/ad-hoc apps on qualifying teams. This check happens before game code runs. Its portal-based offline-profile option is not established as available to our free Personal Team. Do not promise that editing gameplay removes Apple's signing checks, or that a successful running offline session proves every future offline cold launch. Check initial verification with internet after installation, then separately qualify Wi-Fi loss and offline cold reopening. [Personal Team expiry rules](https://developer.apple.com/help/account/basics/about-your-developer-account) remain a separate deployment constraint.

The 93 → 94 update followed a verified 299-file backup. Of 298 Documents files, 297 were byte-identical, including all ten world-save files; every existing preference was retained. The only changed document was the old app's live connection diagnostic: elapsed time, data age and one appended reconnect event. That exact difference was reviewed without reinstalling or resetting anything. [Signed update evidence](evidence/offline94-2026-09-25/ipad94-update.json).

Required checks: native held-touch/menu/missing-replica loss, foreground resume, unchanged server authority on reunion, retained iPad update, real Wi-Fi loss without reopening, and measured A10 offline walking. Then qualify newer iPad and phones, repeated loss/reunion and sustained mixed play. Keep G3-REC-05 open.

### Build 94 completed engineering checks

- [69 core/storage checks](evidence/offline94-2026-09-25/core94.json) pass, including immediate foreground eligibility and unchanged transaction rules.
- [Three native handoff cases](evidence/offline94-2026-09-25/handoff94.json) pass. Each asserts less than five seconds from authority exit to local presentation and less than two seconds from observed disconnection. These are Windows bounds, not physical iPad timings.
- [Five failure-boundary groups](evidence/offline94-2026-09-25/failure-boundaries94.json) pass. Known-loss foreground resume took **0.156 seconds**. Missing acknowledgment was not replayed; save failure did not interrupt siblings.
- [Six four-client continuation groups](evidence/offline94-2026-09-25/continuation94.json) pass. Offline-first play, cold reopen, separate saves, server-authoritative reunion and repeated outages retain their tested behavior.
- [Offline native Windows movement](evidence/offline94-2026-09-25/movement94.json) advances every sampled moving frame for both controls at a 60 fps target. This does not resolve the reported physical hitch.
- [Native iOS Release compilation/linking](evidence/offline94-2026-09-25/ios94-native.json) passes with all six Apple bridge symbols. Signing, in-place installation and runtime 94 identity now pass. The walking hitch remains unresolved pending a physical timing capture.

The exported Windows timing capture contains 1,614 frames, 673 moving offline frames and 14 measured saves, demonstrating that the new diagnostic covers the walking/save interval. Its timing values are not substituted for measurements on A10. The [93 feedback](evidence/offline94-2026-09-25/ipad93-feedback.json) and [scoped physical CPU profile](evidence/offline94-2026-09-25/ipad93-profile-summary.json) remain recorded separately.
