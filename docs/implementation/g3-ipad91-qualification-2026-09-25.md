# Build 91 — native Apple build and older iPad update

**G3-REC-05 · NET-02 / AUTO-01 · September 25, 2026. Physical qualification is in progress.**

The prepared build 91 has compiled and linked on the M1 Pro Mac, passed signature/profile/native-bridge checks, and updated the older iPad 7 in place. Its existing Keychain identity automatically rejoined the current build 83 family server. Manual controls, recovery with a matching server, lifecycle and sustained performance remain separate checks.

## Completed so far

- The Mac's changed local address was resolved through its saved hostname and authenticated against the existing pinned SSH host identity. macOS 26.3.1 and Xcode 26.6 were observed. The older iPad is connected by USB, with Developer Mode enabled and iPadOS 18.7.10.
- All **3,094 export files** were rechecked on Windows and again after transfer. The Mac performed one native Release build with the existing development team; no Unity game code or project identity changed.
- Native compile/link and `codesign --verify --deep --strict` passed. All six required discovery/enrollment bridge functions are linked. The app retains its bundle identity, iOS 15 minimum and support for iPad/iPhone. The profile covers the older iPad and currently expires **October 1, 2026**. Unity-generated build warnings remain; no build error was reported.
- Before installation, **264 backed-up files** were copied to both Mac and Windows and hash-verified. During the in-place **79 → 91** update, all **263 existing Documents files** remained byte-identical and all existing preference values were retained. No app uninstall, save reset or enrollment replacement occurred.
- The exact app launched and its device-written runtime status identifies **0.0.91**. Keychain status is `paired`; native discovery, admission and first shared presentation succeeded against the original family server. The sampled transport receive-error count is zero. CoreDevice's previously recorded app-inventory error on this iPad remains; installation, exact-bundle launch and runtime evidence provide the build confirmation.

## Evidence and scope

[Native signing](evidence/ipad91-2026-09-25/signed-build.json) · [Verified backup and update](evidence/ipad91-2026-09-25/ipad7-update.json) · [Initial connection](evidence/ipad91-2026-09-25/ipad7-initial-connection.json).

The server still runs **83**. Initial compatibility and connection do not prove the newer recovery-checkpoint feature: that needs a matching qualified authority. Intentional server-loss tests will not be run against active family play. The newer iPad, Samsung and iPhone have not been updated in this step.

## Next

Finish the older iPad's short physical controls check, then arrange matching-server recovery qualification and the coordinated remaining updates. Keep both iPads' host roles, full reconciliation, independent backup/restore, automatic renewal and sustained A10 measurements open. Use the [return checklist](return-checklist-ipad-lan-2026-09-24.html) for the next user steps and the [main plan](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) for phase order.
