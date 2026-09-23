# Making reliable family games for your iPads

**Audit and project boundary:** [the current feasibility audit](family-playset-feasibility-audit-2026-09-23.html) and main guide control this game's requirements. Its home is `C:\Users\sephi\Desktop\Little weeps game`. Sections 5 and 13 below preserve historical observations about the unrelated old `Meeps game` project and its connector setup; they are not evidence that the new game exists or is connected. Do not open, migrate, import, or modify that old project for this game without a separate explicit request.

**Latest priority clarification:** iPad hosting, automatic joining and automatic host switching remain required. Only multiplayer connectivity while traveling is optional/lower priority. Complete solo play without PC or internet remains required.

**Remote-host expansion:** the PC may also serve 1–4 family players away from home through a planned private internet route. [Main guide section 49](bluey-game-research-2026-09-23.html#49-reaching-your-home-pc-from-a-road-trip) compares Tailscale with Relay and requires testing VPN startup, renewal-helper coexistence, cellular loss, and preserved offline saves. No connection software or server configuration was changed.

Research checked September 23, 2026. Tailored to your Windows PC, Mac laptop, two iPads, and iPhone, with historical observations of a separate Unity project.

**Current game direction:** the later [Bluey playset research and build plan](bluey-game-research-2026-09-23.md) supersedes the general creature-game and 3D starting suggestions below. The chosen game is a **Universal 2D illustrated dollhouse**, with 1–4-player mixed iPad/iPhone/Android home-Wi-Fi play, joystick/tap walking, draggable objects, and spoken English first plus Spanish. Its [illustrated guide](bluey-game-research-2026-09-23.html) includes character pictures and six planned worlds, including Daycare. The native-build and automatic-renewal requirements in this platform report still apply.

**Recommendation:** start the family game in **Unity 6.3 LTS, patch 6000.3.24f1**, on Windows. Use Blender for custom 3D characters when needed, and use your Mac for the final iOS build, signing, installation, and native debugging. Start with one complete ordinary game, then add an optional AR mode using the same game rules. Test against the older iPad throughout development. This version recommendation replaces the earlier suggestion to retain the existing project's editor version.

**Agreed installation requirement:** when deploying the game to the children's iPads, configure and verify a free automatic signing-renewal workflow. Start with Sideloadly on the Windows PC; consider SideStore if it better fits the devices and household routine. The intended outcome is continued play without manual weekly reinstalls. This is part of deployment, not an optional task to leave for later. No renewal software has been installed or configured yet.

**Road-trip requirement added September 23:** the [main plan, section 35](bluey-game-research-2026-09-23.html#35-road-trip-play-hotspot-co-op-and-coming-home) now requires full offline play without the home PC, plus optional co-op through the phone hotspot. An awake home PC alone cannot renew an unreachable traveling iPad. Evaluate an away-from-PC refresh route such as SideStore and verify it on both iPads with the actual internet-connected hotspot before calling long-trip deployment complete. Keep the agreed free route; no installation or renewal-tool change was made during this research.

This is research and a proposed development plan. No iPad build, device performance test, connector repair, or game implementation was performed as part of this report. You identified your Mac as a **14-inch MacBook Pro (2021), M1 Pro, 16 GB RAM**. You later reported iPadOS **18.6.2** on A2197, iPadOS **18.7.10** on A2602, and iOS **26.6.1** on A2484. The Mac's installed macOS version remains to be checked for the exact Xcode setup.

## 1. Your actual devices

| Device you supplied | Confirmed model | Hardware relevant to this project | Proposed role |
| --- | --- | --- | --- |
| A2197 | iPad, 7th generation, Wi-Fi, 2019 | 10.2-inch screen; A10 Fusion; no LiDAR | Minimum performance target; test every milestone here |
| A2602 | iPad, 9th generation, Wi-Fi, 2021 | 10.2-inch screen; A13 Bionic; no LiDAR | Second required iPad test device |
| A2484 | iPhone 13 Pro Max, US model, 2021 | A15 Bionic; LiDAR; 6.7-inch display | Extra phone-layout and AR testing |

Identification: [Apple's iPad model list](https://support.apple.com/en-us/108043) and [iPhone model list](https://support.apple.com/en-us/108044). Hardware: [iPad 7 specifications](https://support.apple.com/en-us/111911), [iPad 9 specifications](https://support.apple.com/en-us/111898), and [iPhone 13 Pro Max specifications](https://support.apple.com/en-us/111870).

Both iPads meet Apple's basic ARKit processor threshold, but individual features have additional requirements. Treat ordinary surface detection and object placement as the intended baseline, then verify support at runtime and on the actual devices. The iPhone's LiDAR does not make LiDAR features available on the iPads. [Apple: checking AR support and permission](https://developer.apple.com/documentation/arkit/verifying_device_support_and_user_permission?changes=latest_major%2Clatest_major).

Your seventh-generation iPad is outside Apple's iPadOS 26 compatibility list, which starts at the eighth generation for standard iPads. Therefore, do not choose iPadOS 26 as the app's minimum deployment version if both iPads must run it. [Apple's iPadOS 26 compatibility list](https://support.apple.com/en-gb/123706).

## 2. Unity, Blender, and the kind of game to make first

Use **Unity** for game rules, touch controls, menus, sound, physics, saving, AR, and the application build. Use **Blender** for modeling, rigging, and animating characters and scenery. You can begin with simple shapes or existing assets and add Blender later.

**Historical suggestion, superseded by the playset plan:** a small creature game could prove touch and saves, but the chosen project is now the illustrated 2D playset with required multiplayer. Use the audit's proof sequence; the creature/3D examples here are background only.

For character production, keep the editable Blender source, export an FBX with the mesh/rig/animations, and import it with its textures into Unity. Check scale, facing direction, animation clips, and material appearance in a small test scene. Recreate materials using the project's URP shaders as needed; do not assume Blender materials will arrive looking identical. Unity documents mesh, bone, skinning, and animation import support, with material limitations. [Unity's Blender import documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/HOWTO-ImportObjectsFrom3DApps.html).

If “iso” meant **isometric**: the same development and installation process applies. My beginner recommendation is a small 3D scene with an orthographic camera for the ordinary mode. An AR mode uses the tracked device camera and a different presentation, while sharing game rules and save data. If “iso” meant **iOS**, that is the platform covered throughout this report.

## 3. Keep creating on Windows; use the Mac for builds

The workflow is:

**Windows: Unity + Blender + coding → transfer the project → Mac: Unity build + Xcode signing → install and test on both iPads.**

Unity's iOS process generates an Xcode project, then Xcode compiles the application. Local native builds require macOS; Unity Build Automation is an alternative if the Mac proves unsuitable. [Unity's iOS build process](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/iphone/ios-building-and-delivering/build-process).

### First setup

1. Keep your Windows machine as the main working copy. Use source control or a deliberate project copy to transfer changes. Keep Assets, Packages, ProjectSettings, and all asset .meta files together. Exclude generated Library, Temp, Logs, and build-output folders from project synchronization.
2. Qualify an exact **Unity 6.3 LTS** patch on Windows and the Mac, including iOS Build Support; **6000.3.24f1** is the researched candidate, with release issues to check before installation. Choose the **Apple silicon / ARM64** editor on the M1 Pro Mac and use the same patch on both machines. Opening the project on the Mac to build it does not require doing creative work there. Start a fresh Universal 2D project in the new game's folder; do not copy or migrate the unrelated old project.
3. Install a compatible Xcode version and its required components. Add your Apple Account in Xcode. Keep signing keys on the Mac or in the chosen build service's protected credential storage.
4. Choose a permanent, unique bundle identifier for this game. Keep it stable for future updates. Select iPhone and iPad device support if the phone will also run this app.
5. Add an iOS Build Profile in Unity, include the intended scenes, and generate the Xcode project. In Xcode, configure the signing team, select a connected device, and build/run.
6. Connect and trust each iPad on the Mac. Enable Developer Mode if the device OS and development-install workflow require it. Follow Xcode's device setup prompts. [Apple: Developer Mode](https://developer.apple.com/documentation/xcode/enabling-developer-mode-on-a-device).
7. Make the first device build extremely small: a scene with a button and a visible build number. Prove installation before spending time on detailed art or AR.

The transfer/organization steps above are my recommended working practice. Avoid simultaneously editing two copies of the same scene, and avoid hardcoded Windows paths in game code. Once manual builds work, the Mac's build step can be automated to reduce switching machines.

### Version compatibility matters

**Why 6.3 LTS:** Unity lists support through December 2027. Its player requirements are iOS/iPadOS 15+, A8 or later, and Metal; both your A10 and A13 iPads meet the hardware requirements. This is a compatibility and maintenance recommendation, not a measured performance ranking. [Unity release support](https://unity.com/releases/unity-6/support), [Unity 6.3 system requirements](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html).

The earlier September 23 release-service check selected **6000.3.24f1**, released September 10. Its release page was rechecked in the feasibility audit; recheck the available patches and qualify the chosen package set before project creation. [Official patch page and downloads](https://unity.com/releases/editor/whats-new/6000.3.24f1). Use a **Universal 2D** project with modest mobile settings for the now-confirmed illustrated style; the earlier Universal 3D suggestion is superseded.

Unity 6.6 is also a supported production release and has the same published minimum iPad hardware/OS requirements. Unity generally favors Update releases for new projects; my choice of 6.3 LTS here prioritizes maintaining a small family app on one supported branch for longer. The support policy does not establish that LTS is inherently faster or free of bugs. Unity 6.0 LTS reaches the end of its ordinary support in October 2026, making it a less attractive new starting point. [Release policy](https://unity.com/releases/unity-6/support), [Unity 6.6 requirements](https://docs.unity3d.com/6000.6/Documentation/Manual/system-requirements.html).

For AR, use **AR Foundation 6.3.5 and Apple ARKit XR Plug-in 6.3.5**, both listed as released for Editor 6000.3. Keep them matched and retain the package lockfile. This ARKit package requires **Xcode 26.0 or later**. [AR Foundation package version](https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-safe/com-unity-xr-arfoundation), [ARKit package version](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.xr.arkit.html), [ARKit build requirements](https://docs.unity3d.com/Packages/com.unity.xr.arkit@6.3/manual/index.html).

A relevant item to track during profiling: Unity reports a Render Graph batching issue on A9/A10 iPhones, with a fix assigned to 6000.3.25f1. That is relevant to your A10 iPad by hardware similarity, but is not proof that your project reproduces it. The fix's assigned version was not yet listed in the checked public release service. Recheck the next published 6.3 patch before installation and validate it on the older iPad. [Unity issue UUM-148865](https://issuetracker.unity.com/issues/23802/render-graph-fails-to-batch-some-passes-when-running-on-iphones-with-powervr-gpus).

The version of Xcode used to build and the oldest iPadOS version allowed to run the game are different settings. Apple's table lists Xcode 26 with deployment targets back to iOS/iPadOS 15, while Xcode 26 itself requires a compatible Mac running macOS Sequoia 15.6 or later. Newer Xcode point releases have their own macOS requirements. We need the installed macOS and device OS versions before selecting the exact Xcode version. The Unity recommendation is already settled. [Apple's Xcode compatibility table](https://developer.apple.com/xcode/system-requirements).

### About VirtualBox

Use the Mac you already own. Apple's macOS license limits installation to Apple-branded computers and permits specified virtualization on Apple hardware; a macOS VM on a typical Windows PC is outside those terms. A VM is also an unnecessary extra component in this project. This is a reason to use the existing Mac for builds, not a reason to move your creative work away from Windows. [Apple's macOS license, sections 2B and 2J](https://www.apple.com/legal/sla/docs/macOSTahoe.pdf).

If the laptop cannot run the required toolchain, investigate a compatible cloud Mac build service before buying hardware. Unity documents cloud signing with a signing certificate/private key and a provisioning profile; cloud builds do not remove Apple's signing requirements. [Unity Build Automation: iOS signing](https://docs.unity.com/en-us/build-automation/sign-build-artifacts/sign-an-ios-application).

## 4. Installing personal apps: free and paid choices

| Method | Useful for | Maintenance or limits |
| --- | --- | --- |
| Free Apple Account / Xcode Personal Team | Your first device experiments | Profiles expire after 7 days; the signed installation must be renewed. The automatic-renewal plan below addresses the recurring manual work. Apple lists 3 registered devices and 3 apps per device among the limits. |
| Paid Developer Program + registered-device development/ad hoc builds | A small, known set of family test devices | Register the devices and sign for them. Track certificate/profile expiry and renew/rebuild as needed; this is not a permanent unsigned installation. |
| TestFlight | Managed beta testing | Builds are usable for up to 90 days. External testing involves Apple's beta review process. |

Sources: [free account limits](https://developer.apple.com/help/account/basics/about-your-developer-account/), [ad hoc provisioning](https://developer.apple.com/help/account/provisioning-profiles/create-an-ad-hoc-provisioning-profile), [TestFlight overview](https://developer.apple.com/help/app-store-connect/test-a-beta-version/testflight-overview/).

The Apple Developer Program is **US$99 per membership year**, with local pricing where offered. It is optional for this plan: use the free installation and automatic-renewal route described below. You do not need a public App Store listing for registered-device testing. [Apple membership pricing](https://developer.apple.com/programs/enroll/), [registered-device distribution](https://help.apple.com/xcode/mac/current/en.lproj/dev7ccaf4d3c.html).

### Required deployment step: automatic renewal on both children's iPads

Use **Sideloadly on Windows** as the first option. Its background helper can refresh the existing app file when the paired iPad is reachable over the local network or USB. Unity does not need to rebuild an unchanged game for each renewal. The free account's seven-day validity and app-count limits still apply; automatic renewal must succeed before expiry. [Sideloadly's refresh and installation documentation](https://sideloadly.io/faq).

**SideStore** is an alternative that can operate over Wi-Fi without a computer after initial installation. Confirm its current device requirements and renewal setup if selecting it. It also needs a working renewal process. [SideStore](https://sidestore.io/).

When installing the family game:

1. Confirm the installed iPadOS versions and select the compatible renewal tool. Keep the plan free; no paid membership purchase is part of this step.
2. Pair each iPad and configure automatic renewal. For Sideloadly, enable the background helper at Windows sign-in and verify that it can reach both iPads on the home network.
3. Keep the same Apple Account and bundle identifier for updates and renewals. Retain the current game package and a recovery copy of meaningful save data.
4. Verify renewal separately on each iPad: check a successful refresh record and renewed expiry, then open the game and confirm existing progress remains. Confirm an automatic cycle, not only the initial installation or a manual refresh.
5. Record where to check refresh status and how to recover from a missed renewal using an in-place refresh/reinstall. Do not delete the app or clear its saves as the default recovery step.
6. Check that the household routine provides regular renewal opportunities, including before travel. If the chosen tool cannot renew reliably, resolve that or switch approaches before treating family deployment as complete.

Renewal prevents expiry while it continues succeeding; it does not make the native app permanently unsigned. If renewal is missed, the expected problem is that the app stops launching until refreshed, rather than the app being automatically deleted.

Do not plan TestFlight as a permanent children's app library. Beyond build expiry, Apple's agreement requires beta testers of apps primarily intended for children to be adults of legal majority in their jurisdiction. [Apple Developer Program agreement, section 7.4](https://developer.apple.com/support/terms/apple-developer-program-license-agreement/).

For later App Store Connect uploads, Apple currently requires Xcode 26 or later and the relevant version 26 SDKs. This upload rule is separate from your app's minimum supported iPadOS version and from direct local testing. [Apple's current submission requirements](https://developer.apple.com/news/upcoming-requirements/).

## 5. What your current Unity project already has

**Historical inspection of the unrelated old `Meeps game` project, not this new game.** These settings must not be used as this game's current configuration. The earlier read-only inspection found:

- Unity Editor **6000.5.0f1**, URP **17.5.0**, Input System **1.19.0**, and Test Framework **1.7.0**.
- Coplay's Unity MCP package **9.7.3** is already in the package manifest.
- AR Foundation and the Apple ARKit XR Plug-in are **not** in the current manifest. The built-in XR module alone does not establish an AR Foundation setup.
- The product name is still **2026-gamejam-summer-spooks** and the iPhone bundle identifier is **com.Unity-Technologies.com.unity.template.urp-blank**. These need deliberate app-specific choices before installation.
- Rotation flags currently allow all four orientations. Choose and verify the intended layout instead of assuming the starter configuration is suitable.

These are configuration findings, not proof that the current game code works or fails on iOS. No project settings were changed for this research.

## 6. AR that works on both iPads

My recommended first AR feature is **place one small game board or creature on a horizontal table**. Do not make room reconstruction, LiDAR depth, or persistent room mapping prerequisites.

Set up compatible AR Foundation and Apple ARKit XR Plug-in packages; enable ARKit for iOS in XR Plug-in Management; configure a clear camera usage description; and resolve the package's project validation issues. Because the proposed game can also run normally, configure AR as optional and check availability before entering the mode. [Unity's ARKit project configuration](https://docs.unity3d.com/Packages/com.unity.xr.arkit@6.3/manual/project-configuration-arkit.html).

Implement explicit states:

**Check support → ask for camera permission → find surface → tap to place → play → recover if tracking is lost.**

Each state needs a visible result. Unsupported AR or denied permission offers ordinary play. Scanning explains that the camera needs a well-lit, textured surface. Placement offers a retry. Tracking loss pauses interactions that depend on accurate positioning and offers repositioning. A child should never be left staring at an unexplained blank camera view. Unity exposes session state and tracking failure information for this purpose. [AR session platform support](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/manual/features/session/platform-support.html), [ARSession API](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/api/UnityEngine.XR.ARFoundation.ARSession.html).

Anchor the placed board and put its pieces under that board. Avoid an ever-growing list of anchors. Remove unused anchors when restarting and stop unnecessary detection work once placement is complete. Anchors help maintain the relationship between virtual objects and the tracked environment; they do not guarantee perfect tracking in every room. [Unity's anchor guidance](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/manual/features/anchors/introduction.html).

For the first version, save the creature, score, and board contents, then ask for placement again on the next launch. Persistent physical-world alignment is a separate feature. Keep the game playable while seated and make ordinary play an easy alternative. Treat multiplayer/shared AR as a later project because synchronizing game data and aligning two cameras are separate problems.

AR simulation is useful while developing on Windows, but its results do not prove camera tracking, device performance, or recovery on an iPad. [Unity XR Simulation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/manual/xr-simulation/simulation.html).

## 7. Coding structure for correct behavior

Use C# for the game. Unity's iOS build uses ahead-of-time compilation, so desktop-only libraries, runtime code generation, and reflection-heavy features need particular care. Code reachable only through reflection may require targeted preservation when managed stripping is enabled. Always verify the installed release build. [Unity scripting restrictions](https://docs.unity3d.com/6000.0/Documentation/Manual/scripting-restrictions.html), [managed code stripping](https://docs.unity3d.com/6000.0/Documentation/Manual/managed-code-stripping.html).

My proposed structure is small and testable:

| Part | Responsibility | Example failure it prevents |
| --- | --- | --- |
| Game rules | Scores, rewards, level completion; independent of cameras | AR and ordinary mode giving different rewards |
| Input routing | Tap/drag ownership, UI versus gameplay, canceled touches | A menu tap also feeding the creature behind it |
| Presentation | Animation, sound, visual feedback | Visual effects accidentally determining saved progress |
| Save service | Versioned data, one writer, recovery, migrations | Two simultaneous saves corrupting progress |
| App lifecycle | Pause/resume and cancellation of obsolete work | A late async result modifying an unloaded scene |
| AR controller | Permission, placement, anchors, tracking recovery | Starting two camera sessions after repeated taps |
| Diagnostics | Build identity and useful error records | Reports that cannot be matched to a specific build |

Use Input Actions or Enhanced Touch for touch input. Unity warns against directly polling raw Touchscreen state from Update/FixedUpdate because changes can be missed. Track individual touches and handle cancellation; do not rely only on mouse clicks in the Editor. [Input System touch documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/Touch.html).

Prevent repeated taps from granting a reward twice. Unsubscribe event handlers and cancel asynchronous operations when their scene or owner is destroyed. Bound spawned objects and particle counts. Use elapsed time for time-based behavior so a faster phone does not change game speed. Add focused tests for reward duplication, completion rules, save migration, and restart behavior; these are more valuable than tests that simply reproduce getters and setters.

## 8. Saving progress safely

Use Application.persistentDataPath for save files instead of a hardcoded location. Keep the bundle identifier stable between updates. Unity documents how that identifier affects the persistent storage location. [Unity persistent data path](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html).

Do not save only when the app quits. On iOS, suspension commonly occurs instead of a normal quit callback. Save completed levels and meaningful rewards during play; pause/focus callbacks provide additional opportunities. [Unity OnApplicationQuit behavior](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoBehaviour.OnApplicationQuit.html).

My recommended save design:

1. Store a small, versioned data model, independent of scene objects.
2. Allow only one write operation at a time.
3. Write to a temporary file in the same directory, flush and validate it, then replace the main save using a platform-supported atomic operation. Keep a last-known-good backup.
4. On load, validate the schema and values. Recover from the backup if the main file is damaged; report a recoverable problem instead of silently overwriting both files with an empty game.
5. Handle storage/write failure without claiming that progress was saved. Keep earned state in memory where possible and allow a retry.
6. Test upgrades from an older save version and interruptions during writing using disposable test saves.

Use simple preferences for volume and similar settings; use the save service for meaningful progress. Define what “saved” means: for example, a completed level remains completed after force-closing and reopening. No design can promise to preserve every last unsaved input if the operating system terminates the process at any moment.

## 9. Performance, heat, and battery

Use the seventh-generation iPad as the baseline. My starting target for ordinary play is a steady **30 frames per second**; increase to 60 only when measured performance and heat allow it. These correspond to roughly 33.3 ms and 16.7 ms per frame. These are proposed targets, not measured results for your project.

Set an intentional frame-rate policy. On mobile, Application.targetFrameRate controls the requested rate. AR Foundation can take over timing when matchFrameRateRequested is enabled, and its changes are not automatically undone when the session is disabled; restore the ordinary mode's frame-rate settings when switching back. [Unity targetFrameRate](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-targetFrameRate.html), [ARSession timing behavior](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/api/UnityEngine.XR.ARFoundation.ARSession.html).

Start with restrained lighting, shadow distance, transparent effects, texture sizes, and animated characters. Profile before choosing what to reduce. URP's shadow quality and light configuration directly affect rendering work. There is no universal triangle count or memory limit that guarantees a smooth game across scenes. [Unity's URP shadow optimization guidance](https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/shadows-in-urp/shadows-optimization).

Use a Development Build for profiling CPU, GPU, allocation spikes, and memory behavior on the iPad, then repeat the functional acceptance checks on the release build the children will use. Run for 30–60 minutes and repeat scene changes: stable memory after warm-up matters more than a flattering first-minute average frame rate. [Unity device profiling](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/iphone/ios-developing/ios-testing-and-debugging/ios-profile-device).

Unity's Device Simulator helps with layout; it does not reproduce the iPad's real performance, native plug-ins, or full multitouch behavior. A successful Editor run is not a device acceptance test. [Device Simulator limitations](https://docs.unity3d.com/6000.0/Documentation/Manual/device-simulator-introduction.html).

## 10. Understand the different kinds of failure

| Symptom | Possible category; investigate before diagnosing | Evidence to collect |
| --- | --- | --- |
| Game stops responding correctly but stays open | Managed exception, state bug, duplicate event, missing reference | Unity logs and exact input sequence |
| Suddenly returns to the Home Screen | Native crash or operating-system termination | Device crash report or jetsam report |
| Dies after long play or many restarts | Growing memory usage is one possibility | Memory measurements over time and OS termination reason |
| Hangs during launch/resume | Main thread blocked; possible watchdog termination | Device report plus main-thread profiling |
| Only the release version fails | Stripping, build configuration, native plug-in, or timing issue | Logs and symbols from that exact release |
| AR shows no useful placement | Permission, tracking, lighting, or unsupported feature | AR state, permission state, environment |
| App stops launching after a signing deadline | Expired/revoked provisioning or signing is possible | Profile/certificate dates and install error |

Apple distinguishes ordinary crashes from memory-pressure termination (“jetsam”) and watchdog termination. A missing C# stack trace does not establish that there was no application problem. [Apple device-log diagnosis](https://developer.apple.com/documentation/xcode/diagnosing-issues-using-crash-reports-and-device-logs?changes=_8%2C_8), [jetsam reports](https://developer.apple.com/documentation/xcode/identifying-high-memory-use-with-jetsam-event-reports), [watchdog terminations](https://developer.apple.com/documentation/xcode/addressing-watchdog-terminations?changes=_1).

For each reported issue, record the build number/source revision, device model, OS version, scene, AR state, last actions, and approximate time. Retain each delivered build's archive and matching dSYM files so native addresses can be translated into meaningful function names. A different build's symbols are not a substitute. [Apple crash-report analysis](https://developer.apple.com/documentation/xcode/analyzing-a-crash-report).

Preserve save data when troubleshooting. Deleting the app is not the default remedy. Fix the reproducible cause, add a focused regression check, and test an in-place update with existing progress.

## 11. Child-friendly functions to include

My suggested first-release requirements:

- A clear start screen, pause, resume, restart, and an obvious way home.
- Generous touch targets with space between them. Apple's default iPhone/iPad target size is 44 by 44 points; points are not Unity texture pixels. Make key controls larger where children need them. [Apple's accessibility design guidance](https://developer.apple.com/videos/play/wwdc2024/10085/).
- Both sound and visible feedback; avoid communicating success only through color or audio.
- Separate music/effects controls and predictable behavior after headphone or audio interruptions.
- Working layouts on both iPads and the phone, including safe areas and intended rotations.
- Offline gameplay after installation, with no unnecessary account or server dependency.
- Separate local profiles if children share an iPad, and a clearly protected parent reset action.
- Camera access only for AR; no room-image recording/upload as part of the proposed design.
- Diagnostics that describe technical state without including child names, photos, or unnecessary personal data.

## 12. Proposed acceptance checklist

These are initial project gates, not Apple certification rules or a guarantee of zero future crashes. Run them on **both iPads** and use the phone as additional coverage. Record build number and OS with the results.

| Check | Passing behavior |
| --- | --- |
| 20 cold launches per iPad | No crash, hang, incorrect first screen, or lost saved progress |
| Launch and play offline | Core game works without unexpected waiting or login |
| 20 Home/lock/resume cycles | Correct pause state, audio, input, and camera recovery |
| 20 level restarts/transitions | No duplicated events/rewards or continuing memory growth |
| Rapid taps and two-finger interactions | UI owns its touches; no duplicate purchases/rewards or stuck drags |
| Supported rotations and phone safe areas | Controls stay visible, reachable, and correctly aligned |
| Complete a level, force-close, reopen | Completed progress remains saved |
| Controlled interrupted/corrupt save tests | Backup recovery works; no silent deletion of good data |
| Low-storage/write-error test with disposable data | Honest error state and recoverable behavior |
| Install an update over the previous build | Existing profiles/progress remain and migrations succeed |
| Automatic signing renewal on each child's iPad | Successful automatic refresh and renewed expiry are confirmed; the game still launches with existing progress |
| Renewal recovery rehearsal | Parent can find refresh status and recover an expired test installation in place without deleting saved progress |
| Camera allowed, denied, and later revoked | Useful explanation and ordinary-mode fallback |
| AR in dim light/on a plain surface | Guidance/recovery instead of an endless unexplained spinner |
| Ten AR enter/exit/reposition cycles | No orphaned content, duplicate sessions, or accumulating anchors |
| 30–60 minute ordinary and AR sessions | Acceptable pacing/heat; no crash or progressive degradation |
| Launch release build without debugger | Same core behavior as the tested development build |

Automate the pure game rules and save-format checks with Unity's existing test framework. Keep camera quality, warmth, touch ergonomics, audio interruptions, and device installation in the real-device checklist. [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@1.7/manual/index.html).

## 13. Simple connection refresher: Codex, Unity, and Blender

**Historical connection record for the old project.** Do not use the shortcut or command below to connect this new game: it targets `Meeps game`. A new-project connection has not been established or verified by this audit. Keep the record for reference only until this game's own Unity project and connection are deliberately created.

### Unity connection on Windows

The old project's inspected package was Coplay Unity MCP 9.7.3. Its source confirmed this menu for that earlier setup; these are not instructions to open it for the new game:

1. Open this project in Unity and allow compilation to finish.
2. Open **Window → MCP For Unity → Toggle MCP Window**.
3. Select **HTTP Local**, start the server, and connect the Unity session if it is not connected automatically.
4. In the desktop app's **Settings → MCP servers**, enable/edit **unityMCP** using Streamable HTTP and **http://127.0.0.1:8080/mcp** to match the local server.
5. Save/restart the MCP connection. Verify by asking Codex to list the active Unity scene and objects before making changes.

The menu comes from the installed package. Provider reference: [Coplay Unity MCP](https://github.com/CoplayDev/unity-mcp). Desktop MCP configuration: [official OpenAI documentation](https://developers.openai.com/codex/mcp).

### Earlier Blender connection record

The earlier setup recorded Blender 5.0.1 with MCP for Blender 2.0.3 enabled, automatic startup, and a saved
reconnect helper. The broken old Codex entry was replaced. The live scene was
read successfully through the actual MCP registration.

### Old-project one-command access — reference only

The existing **Connect Unity + Blender** desktop shortcut invokes the old-project helper:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Users\sephi\Desktop\Meeps game\Connect-GameTools.ps1"
~~~

That helper was designed to open the old project's editors, start its local Unity server, and verify responses. It does not create or select this new game. Its historical successful Blender connection was not reverified in this audit.

That existing connector setup belongs to the unrelated old project. See [its setup and reconnect guide](<C:/Users/sephi/Desktop/Meeps game/docs/connect-unity-blender.md>) for options,
limitations, installed versions, and verification results. These connections run
on the development PC; they are not required on the children's iPads.

## 14. A practical order for building the game

This original ordinary/AR sequence is background. The current required multiplayer and independent-zone proof order is in the [feasibility audit](family-playset-feasibility-audit-2026-09-23.html#6-the-proof-plan-before-producing-the-full-game); AR remains optional.

1. **Prove the build path.** Confirm the Mac/toolchain, record each device OS, choose the bundle identifier, and install one tiny scene on both iPads.
2. **Finish one ordinary game loop.** Temporary art, touch controls, reward, save, pause, restart, and offline launch. Test it on the older iPad immediately.
3. **Add the characters.** Bring in one Blender character and a small set of animations; verify appearance, touch/collision behavior, and performance before producing many assets.
4. **Add optional AR.** Share the existing rules and saves; add permission handling, placement, tracking recovery, and a normal-mode fallback.
5. **Qualify the family build and its automatic renewal.** Run the checklist on the exact release artifact, retain symbols, and install in place. Configure Sideloadly or the selected alternative on both children's iPads, verify automatic renewal and preserved progress, and document recovery from a missed refresh. Family deployment is incomplete until this renewal requirement is verified.

Personal use makes the audience smaller. It does not reduce the need for dependable controls, saved progress, recoverable errors, and testing on the actual iPads.
