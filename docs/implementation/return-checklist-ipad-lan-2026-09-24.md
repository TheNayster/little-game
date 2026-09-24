# When you return — iPad multiplayer checks

This checklist follows G3 in the main build guide. Both iPads still have their tested solo **56**; their apps and saves have not been replaced. **Family build 74** is the prepared candidate. Windows encrypted rejoin testing and native compilation are recorded in the [engineering report](g3-ipad-lan-2026-09-24.html).

## Items that need you in person

1. **Finish signing on the Mac.** Double-click **Finish-Little-Weeps-74.command** on the Mac Desktop. If asked, type the Mac login password into that Terminal window and approve a codesign key-access prompt. Keep the window open and tell Codex when it finishes. Do not send the password in chat. The unattended attempt hit macOS `errSecInternalComponent` while using the signing key; compilation itself succeeded. This shortcut builds/signs only—it does not install or erase anything.
2. **Keep both iPads connected and unlocked.** Leave them on the Home Screen while Codex verifies signing/device coverage, backs up their current saves and updates each app in place. Codex will provision separate player identities and start the PC authority. Do not delete the existing app.
3. **Allow the game's local-network prompt on each iPad** when it appears. Both devices and the PC should be on home Wi-Fi. Tell Codex what appears if the prompt is absent or play stays solo; there is no need to hunt through settings before a prompt actually appears.
4. **Two-player play check, after Codex confirms both joined.** Walk on one iPad while watching the other. Hold the bucket on one; the other should see it being held and be unable to take it. Move one player to Creek while the other stays in Garden, then meet again. Check joystick plus dragging, Listen, remembered settings and full-screen layout.
5. **Real leave/return check.** Lock one iPad while the other keeps walking and using toys. Unlock and return: the first should automatically rejoin and receive current progress, without a stuck drag or moving the sibling. Then repeat with the other iPad. Codex will guide one short check at a time and inspect the records.
6. **Save/offline checks.** Codex will compare the old solo save and preferences before/after the update, check permission-denied/server-absent launch, and test closing/reopening. Automatic merging of offline and shared play is still future work; don't treat this build as the finished travel/recovery system.

Mac shortcut: `/Users/nayster/Desktop/Finish-Little-Weeps-74.command`. Use **74**, not the earlier intermediate 72/73 helpers. If macOS shows an error, leave the window open and share its wording.

## Android and the fourth player — after the iPad checks

7. **Have the Samsung available on home Wi-Fi.** Android family release **78** is built and signed with the original family key. Codex will verify the installed app/signature and back up accessible saved data before an in-place update. If wireless debugging needs pairing again, enter the code at that time. No phone action is needed while you are away.
8. **Repeat a short mixed-device check:** both iPads plus Android, then add the iPhone for four players. Test movement, independent areas, shared bucket ownership, lock/return and saved settings. Android-emulator/three-Windows play already passed, but the actual Samsung, phone layout and four physical devices still need testing. [Android work and evidence](g3-android-lan-2026-09-24.html).

## Existing release items, not prerequisites for this code work

- Unattended app renewal on both iPads is still unproven; the earlier iPhone USB renewal did not qualify Wi-Fi renewal.
- Sustained performance on the older A10 iPad and observing the children use the controls remain open.
- Four actual devices still requires the Android phone and iPhone in addition to both iPads.

These are return-home actions, not requests to interrupt your time away. The Mac password, device permissions and physical touch/lifecycle checks cannot be counted as passed by an automated Windows test.
