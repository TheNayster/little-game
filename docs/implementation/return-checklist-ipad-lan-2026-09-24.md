# When you return — iPad multiplayer checks

This checklist follows G3 in the main build guide. Both iPads still have their tested solo **56**; their apps and saves have not been replaced. **Family build 79** is the latest candidate. Windows and Android-emulator checks now cover automatic joining from solo and returning to a preserved solo draft. The iPad 79 export transferred and compiled natively on the Mac after correcting its changed network address. The older iPad backup is verified; signing/install/enrollment and physical checks remain pending. [Latest engineering report](g3-offline-join-2026-09-24.html).

## Items that need you in person

1. **Finish signing on the Mac.** The new **Finish-Little-Weeps-79.command** Terminal window is open. Enter the Mac password locally if asked and approve codesign key access. Keep the window open and report its result; do not send the password. The older 74 helper does not include the new joining behavior.
2. **Keep both iPads connected and unlocked.** Leave them on the Home Screen while Codex verifies signing/device coverage, backs up their current saves and updates each app in place. Codex will provision separate player identities and start the PC authority. Do not delete the existing app.
3. **Allow the game's local-network prompt on each iPad** when it appears. Both devices and the PC should be on home Wi-Fi. Tell Codex what appears if the prompt is absent or play stays solo; there is no need to hunt through settings before a prompt actually appears.
4. **Two-player play check, after Codex confirms both joined.** Walk on one iPad while watching the other. Hold the bucket on one; the other should see it being held and be unable to take it. Move one player to Creek while the other stays in Garden, then meet again. Check joystick plus dragging, Listen, remembered settings and full-screen layout.
5. **Real leave/return check.** Lock one iPad while the other keeps walking and using toys. Unlock and return: the first should automatically rejoin and receive current progress, without a stuck drag or moving the sibling. Then repeat with the other iPad. Codex will guide one short check at a time and inspect the records.
6. **Save/offline and late-join checks.** Codex will compare the old save/preferences before/after updating. Open with the server absent and play solo; start the server while holding a toy or keeping Menu open. It should wait for a safe moment, then join automatically. Menu → Play by myself should restore the separate solo draft; Find my family rejoins. Test closing/reopening and permission-denied behavior. Automatic merging and prolonged-outage local recovery remain future work.

Mac helper: `/Users/nayster/Desktop/Finish-Little-Weeps-79.command` — now prepared and open. Mac native compile/link passed; signing and installation have not yet been claimed. Xcode could access the older iPad; the newer iPad still needs its connection re-established before its backup/update.

## Android and the fourth player — after the iPad checks

7. **Have the Samsung available on home Wi-Fi.** Android family release **79** is built and signed with the original family key. Codex will verify the installed app/signature and back up accessible saved data before an in-place update. If wireless debugging needs pairing again, enter the code at that time. No phone action is needed while you are away.
8. **Repeat a short mixed-device check:** both iPads plus Android, then add the iPhone for four players. Test movement, independent areas, shared bucket ownership, lock/return and saved settings. Android-emulator/three-Windows play already passed, but the actual Samsung, phone layout and four physical devices still need testing. [Android work and evidence](g3-android-lan-2026-09-24.html).

## Existing release items, not prerequisites for this code work

- Unattended app renewal on both iPads is still unproven; the earlier iPhone USB renewal did not qualify Wi-Fi renewal.
- Sustained performance on the older A10 iPad and observing the children use the controls remain open.
- Four actual devices still requires the Android phone and iPhone in addition to both iPads.

These are return-home actions, not requests to interrupt your time away. The Mac password, device permissions and physical touch/lifecycle checks cannot be counted as passed by an automated Windows test.
