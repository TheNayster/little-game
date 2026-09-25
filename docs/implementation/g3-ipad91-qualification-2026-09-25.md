# Build 91 — native Apple build and older iPad update

**G3-REC-05 · NET-02 / AUTO-01 · September 25, 2026. Physical qualification is in progress.**

The prepared build 91 has compiled and linked on the M1 Pro Mac, passed signature/profile/native-bridge checks, and updated the older iPad 7 in place. Its existing Keychain identity automatically rejoined the current build 83 family server. The user confirms walking, bucket filling/watering, Listen and full-screen visibility all pass on 91. The server was subsequently updated to 91. Latest physical reports expose offline-control and walking defects; acceptance remains open as detailed below.

## Completed so far

- The Mac's changed local address was resolved through its saved hostname and authenticated against the existing pinned SSH host identity. macOS 26.3.1 and Xcode 26.6 were observed. The older iPad is connected by USB, with Developer Mode enabled and iPadOS 18.7.10.
- All **3,094 export files** were rechecked on Windows and again after transfer. The Mac performed one native Release build with the existing development team; no Unity game code or project identity changed.
- Native compile/link and `codesign --verify --deep --strict` passed. All six required discovery/enrollment bridge functions are linked. The app retains its bundle identity, iOS 15 minimum and support for iPad/iPhone. The profile covers the older iPad and currently expires **October 1, 2026**. Unity-generated build warnings remain; no build error was reported.
- Before installation, **264 backed-up files** were copied to both Mac and Windows and hash-verified. During the in-place **79 → 91** update, all **263 existing Documents files** remained byte-identical and all existing preference values were retained. No app uninstall, save reset or enrollment replacement occurred.
- The exact app launched and its device-written runtime status identifies **0.0.91**. Keychain status is `paired`; native discovery, admission and first shared presentation succeeded against the original family server. The sampled transport receive-error count is zero. CoreDevice's previously recorded app-inventory error on this iPad remains; installation, exact-bundle launch and runtime evidence provide the build confirmation.

- The user replied **“yup all passed”** to the older-iPad walking, bucket-to-tap/plant, Listen and full-screen checks. This is physical controls/layout feedback on 91, not recovery or a measured performance run. [Feedback](evidence/ipad91-2026-09-25/ipad7-controls-feedback.json).

## Evidence and scope

[Native signing](evidence/ipad91-2026-09-25/signed-build.json) · [Verified backup and update](evidence/ipad91-2026-09-25/ipad7-update.json) · [Initial connection](evidence/ipad91-2026-09-25/ipad7-initial-connection.json).

The server now runs **91**, deployed in an empty session with all old world fields, four player records and ten items retained. The parent helper now selects 91 and reports automatic recovery enabled/healthy; sign-in startup remains Off. [Server deployment](evidence/ipad91-2026-09-25/server91-deployment.json) · [Parent activation](evidence/ipad91-2026-09-25/parent91-activation.json).

## Physical recovery observations and latest blockers

The older iPad received a verified complete checkpoint containing four players, ten items, both areas, 128 receipts and idle clocks. In one Wi-Fi-off attempt the user reported controls working after about ten seconds; the new offline adventure saved moved objects. Cold reopening with Wi-Fi off retained the same adventure and controls. Re-enabling Wi-Fi rejoined the family. Both original solo files remained byte-identical across those reads. The saved adventure retained its identity/common base; the user kept playing between snapshots, so exact cold-read-to-reunion byte equality is not claimed. The menu reopening step remains unconfirmed. [Scoped evidence](evidence/ipad91-2026-09-25/ipad7-recovery-session.json).

**The later report supersedes any overall recovery pass:** all four devices freeze after Wi-Fi loss until reopening, all are choppy offline, all are smooth online and rejoin when Wi-Fi returns. These remain physical blockers, including on build 91. [Exact feedback](evidence/ipad91-2026-09-25/all-device-offline-feedback.json).

## Next

[Focused research and replacement build 93](g3-offline-recovery-and-motion-2026-09-25.html) cover per-frame walking, canceled-touch/menu recovery, missing checkpoints and exact solo retention. Candidate 92 was not installed. Complete replacement validation and a retained update on iPad 7 first, then test actual Wi-Fi loss, repeated transitions and offline walking before wider deployment. Follow the [short return checklist](return-checklist-ipad-lan-2026-09-24.html). Keep G3 open; G4 iPad hosting, G5 reconciliation, independent recovery, renewal and sustained measurements are still required.
