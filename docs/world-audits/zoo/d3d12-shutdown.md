# Windows Direct3D12 shutdown investigation — October 8, 2026

**Unresolved; no verified fix or mitigation delivered.** Zoo feature work remains paused. Investigation started from `dd916a33adcb8dae06bec92e68aa629babb51034`, verified against `origin/main`, on `codex/d3d12-shutdown-diagnosis`. Production renderer, gameplay code, profiles, saves, albums, devices and live server were not changed. This is an investigation checkpoint, not final crash acceptance.

## Exact baseline

| Item | Verified value |
| --- | --- |
| Project | `Unity/FamilyPlayset`; Unity **6000.3.24f1**, revision `4e7b9b5b6244` |
| Release | **510**, Mono backend; existing standard release artifacts verified by the established launcher |
| Rendering | Actual logs: **Direct3D 12**, feature level12.2, NVIDIA RTX5090; `kGfxThreadingModeSplitJobs` |
| Jobs | `boot.config`: `gfx-enable-gfx-jobs=1`, `gfx-threading-mode=6`; Editor API readback: enabled, Split |
| Pipeline | Built-in: `GraphicsSettings.m_CustomRenderPipeline` is null. URP17.3.0 is installed; installation alone does not mean it is active |
| Other packages | Input System1.20.0, uGUI2.0.0, dedicated-server2.0.2; exact manifests retained |
| OS | **Windows11 Home25H2, 26200.9457**, x64; registry's legacy Windows10 product string was checked against CIM's actual Windows11 caption |
| Active adapter/driver | **NVIDIA GeForce RTX5090, 32.0.16.1692**; secondary Intel Graphics32.0.101.5869 was enumerated but not the selected renderer |
| Native/injection inventory | Complete loaded-module readback retained. NVIDIA `nvspcap64.dll`11.0.9.251 appears in failing dump inventory and passing510 readback. No project-native DLLs were found under Assets. Presence is not causation |

Build510 launcher SHA256: `e1a9d9e062bbcdb91f76983fae8bb1e4fb8036d55289318ef91033f130d38ecc`. UnityPlayer SHA256: `e286bd7f01c45197d051325423b13df133d2ac315671eca2881e7936d251c6bd`. Complete artifact-manifest SHA256: `ba4e873fcc8252413bcc199bc233beafa4ad08701843be1ae5acf0267c32d82a`.

Build503 has the same generic launcher/UnityPlayer bytes, but a different full artifact-manifest SHA256: `bb341dfa81102819f5b781ad75e96bf67d3709f4cfaba76d64fa7a86b4854343`. Launcher identity alone cannot distinguish game builds. All2406 recorded510 Unity inputs were checked against the checkout: twelve raw hash differences are **only CRLF/LF conversion**, verified by recomputing both forms; zero substantive differences. Raw discrepancies remain in the receipt, rather than being labeled exact byte matches.

## Symbolized historical diagnosis

Existing Application Error events independently confirm builds495/498/499/500/503, `c0000005`, offset`a1f5`. They identify the **application-local Agility** DLL under each build's `Client/D3D12/D3D12Core.dll`, not the system runtime.

Five existing native dumps were inventoried without generating redundant baseline crashes. Two were opened in the approved Microsoft WinDbg/CDB installation, one from500 and one from503. Matching local UnityPlayer/WindowsPlayer PDBs and Microsoft symbol-server PDBs resolved the following same stack in both:

```text
D3D12Core!DXBCGetSizeAssumingValidPointer+0x5
D3D12Core!CLibrary::Serialize+0x242
UnityPlayer!GfxDeviceD3D12::~GfxDeviceD3D12+0x1bc4
UnityPlayer!GfxDeviceD3D12::vector deleting destructor+0x14
UnityPlayer!DestroyGfxDevice+0x128
UnityPlayer!Shutdown+0x3d
UnityPlayer!UnityMainImpl+0x1741
UnityPlayer!UnityMain+0xb
LittleWeepsNetwork!__scrt_common_main_seh+0x106
```

Dump46620 exception record: **read**, parameter0=`0`, address=`0x000001b20475b8e8`. Instruction `mov eax,dword ptr [rcx+18h]`, `rcx=0x000001b20475b8d0`. Faulting thread31044. Dump31920 has the same instruction/stack and a different invalid read address. The fault occurs in **pipeline-library serialization during Unity graphics-device destruction**, reading shader-container size. It is a CPU access violation; these logs contain no device-removal diagnosis warranting DRED attribution.

Agility identity: version **1.618.1.0** (version-string1.618.1.0.20250923.2), timestamp`68d311e6`, image size`0048F000`, PDB GUID`7de2b306-e2fa-4178-8ced-40efce8c6859`, age1. Release DLL SHA256`625bd40005b1c3708695d5256ecf230d278e6b8edba5a88af70f3d91324f3052`. System32's D3D12Core is10.0.26100.9278 and is a different runtime.

**Confirmed failure mechanism:** invalid shader-bytecode read while serializing the library at teardown. **Unconfirmed ownership/cause:** engine/runtime lifetime defect, stale library backing storage, other corruption, driver or injection interaction. The small dumps do not contain the allocation/free provenance necessary to prove use-after-free. Heap/address inspection reported missing target memory information. Neither the DLL name nor this stack alone proves which component invalidated the pointer.

## Controlled experiments and results

Each task run used new isolated data and unique Player logs. Recorded client exits used Unity `Application.Quit` through the existing control channel, except the explicitly recorded normal window close. No task run used kill, taskkill, Environment.Exit, or a teardown bypass. Task wrappers preserved every close result and removed the existing harness's forced-cleanup fallback; production tests/assertions were not weakened.

| Configuration/scenario | Recorded client closes | Exit result | Interpretation |
| --- | ---: | --- | --- |
| Historical495/498/499/500/503 | Existing failures | c0000005 / a1f5 | Failed shutdown; preserved evidence |
| Release510, explicitD3D12, menu without Zoo | 1 | 0,0.829s | Baseline pass |
| Same510, explicitD3D11, menu | 1 | 0,0.719s | Separate API contingency check |
| Same510, explicitD3D12, Zoo without album | 1 | 0,0.812s | Baseline pass |
| Same510, explicitD3D12, photo and album | 1 | 0,0.781s | Baseline pass |
| Same510, explicitD3D12, full four-client album/reconnect regression | 6 | All0,0.766–0.812s | All ten gameplay/storage groups passed; reconnect and independent departure included |
| Diagnostic510, Development/PDB/debug-layer, photo and album | 1 | 0,0.359s | Diagnostic pass; not production qualification |
| Same diagnostic, full four-client regression | 6 | All0,0.375–0.516s | Gameplay/close passed; validation findings below remain failures of clean validation |
| Earlier503, explicitD3D12, same complete historical regression | 6 | All0,0.750–0.813s | Historical failure did not reproduce today; not evidence of a source fix |
| Release510, explicitD3D12, private save/album restart and private/shared separation | 3 | All0,0.782–0.828s | Original JPEG hash, sticker edits, saved Zoo location and checkpoint retained; shared album separate |
| Visible510D3D12 menu/Zoo/photo/sticker demo, normal window close | 1 | **0,1.141s**, CloseMainWindow accepted | Live native window shown, screenshot inspected; no video or owner acceptance claim |

Actual APIs are parsed from every client log in [the consolidated local receipt](../../../LocalData/D3D12Shutdown/summary.json). All26D3D12 client closes and the separate D3D11 close returned0; four recorded isolated headless authority closes also returned0. Application events1000/1001 were queried after the runs over the complete task time interval: **no matching new LittleWeepsNetwork event**. This includes the visible normal window close. These are baseline observations across configurations, **not ten consecutive post-fix acceptance cycles**.

Private persistence: [results](../../../LocalData/FamilyLAN/2a01b8f8b5d54fee9e6c121efc13d477/zoo-photos/results.json). Current four-client release: [results](../../../LocalData/SharedGarden/38b8e4ac46aa4cea85652a17b8ae528e/zoo-photos/results.json). Earlier503: [results](../../../LocalData/SharedGarden/9021f47d794d4252b7f84c88b96d6a7d/zoo-photos/results.json). Live demo: [receipt](../../../LocalData/D3D12Shutdown/live-demo.json), [album view](../../../LocalData/D3D12Shutdown/live-album.png).

The repaired .NET Zoo suite was separately rerun with `dotnet run --project Tools/Verification/ZooRules.Tests/ZooRules.Tests.csproj --no-restore`: all ten current groups pass, including story/habitat groups. The resolved snapshot-fixture issue is not the native crash.

## Development validation and rejected attributions

A separate6000.3.24f1 Development build enabled **Copy PDB Files** on a temporary copied profile using Editor serialization APIs. Its company/product identity was isolated, output stayed under LocalData, and original settings were restored through Editor APIs. UnityPlayer and its PDB match the installed development variation; release/master symbols were used for the historical release dumps. The debug-layer module readback proves **application-local D3D12SDKLayers1.618.1.0.20250923.2** loaded with `-force-d3d12-debug`, matching the Agility runtime. No Graphics Tools install/system change was needed. GBV was not added.

CDB attached to one task-owned Development client during the four-client sequence. Its debug output contains **30320 repeated error527 RESOURCE_BARRIER_BEFORE_AFTER_MISMATCH** messages for `Depth-BackBuffer-1280-591`, alternating DEPTH_WRITE and DEPTH_READ/shader-read states. No access violation or device-removal event occurred. A diagnostic breakpoint was requested during this run; it is explicitly diagnostic evidence, not an uninstrumented timing test. The client ultimately exited normally. The relationship between these depth-state violations and historical library serialization is **unestablished**; no clean validation-layer pass is claimed.

Project/runtime/package searches found no active use of `ShaderWarmup.WarmupShaderFromCollection` or equivalent explicit collection warmup. Relevant shutdown callbacks and capture/readback/temporary texture cleanup were inspected; no evidence-supported double release or asynchronous GPU callback defect was established. The album uses synchronous readback and a background file save; failure reports precede the latest habitat work. No speculative resource-lifetime edit was shipped.

- [UUM-149781](https://issuetracker.unity.com/issues/24049/player-crashes-with-use-after-free-when-a-shader-is-unloaded-immediately-after-shaderwarmupwarmupshaderfromcollection-on-d3d12): affected6000.3 release family and shader-lifetime lead, but the stated warmup trigger is absent. Do not attribute this crash to it. The issue page lists6000.3.25f1, whereas the retrieved patch release notes do not enumerate that issue; this discrepancy is retained.
- [UUM-150682](https://issuetracker.unity.com/issues/24291/crash-on-gfxversionlistimplreleasefromgfxversionlist-when-performing-various-unity-operations): observed stack lacks its `GfxVersionList::Impl::ReleaseFromGfxVersionList`/`BufferD3D12` sequence. Its6.3 status is open. Do not label it the cause or require a major/beta migration.
- No editor installation/migration, driver change, cache purge, DLL swap, TDR edit, overlay setting, OS change, reboot or security change was performed. NVIDIA capture injection was inventoried, not blamed or disabled.

Development preparation failures were retained: first a temporary helper's int/uint compile error, then Unity's refusal to build an unsaved cloned profile; both repaired before the successful diagnostic build. A baseline UI fixture initially queried a Zoo control before readiness; it failed, its own clients received graceful quit requests, and the corrected run waited for readiness. A private test first lacked cryptography in the default Python; it was rerun using an existing cached project environment. None of these is a native crash or a successful acceptance run.

## Restoration, blocker and delivery

Temporary Editor helper/meta were moved to ignored `LocalData/D3D12Shutdown/BuildSource`; temporary profile was removed through AssetDatabase. Tracked Unity settings/profiles/runtime code and Zoo tests have zero differences from the starting checkout. Pre-existing untracked work was left untouched. Existing owner sessions were not targeted by the task; diagnostic IDs/roots remained separate. No live-server/device rollout was performed.

The immediate blocker is **absence of a reliable current reproduction and allocation/free evidence**, despite the valid symbolized historical fault. A safe causal project fix cannot be chosen from this evidence. A minimal reliably failing project has **not** been produced, and no candidate fix was subjected to the requested ten-cycle gate. Passing original binaries today cannot qualify a repair.

Local vendor-report preparation is at [LocalData/D3D12Shutdown/VendorReport/README.md](../../../LocalData/D3D12Shutdown/VendorReport/README.md), with sanitized diagnostic summaries, symbolized traces, exact identities and reproduction instructions. Original dumps remain local and are referenced rather than uploaded or copied into a shareable archive. Nothing was submitted to a vendor. Obtain an exact reproducer before testing a single supported lifetime/cache/engine experiment. Any editor install, main migration or system experiment retains the owner's explicit approval requirement.

This checkpoint stays on the diagnostic branch; `main` remains the existing game baseline. The chat handoff reports its actual commit/push result. **Stop before further Zoo milestones.**

Official interpretation references: [Unity standalone debugging prerequisites](https://discussions.unity.com/t/debugging-player-6-3-upgrade/1703705/11), [Unity D3D12 troubleshooting](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/windows/develop/troubleshoot-gpu-crash), [Microsoft access-violation interpretation](https://learn.microsoft.com/en-us/shows/inside/c0000005), [pipeline-library serialization](https://learn.microsoft.com/en-us/windows/win32/api/d3d12/nf-d3d12-id3d12pipelinelibrary-serialize).
