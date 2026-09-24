# Next G2 device check — when the family is available

The playable Windows preview is 41. The Windows-produced iPad Xcode export is 42, at `Builds/iOSSolo/G2-0.0.42/Xcode`. The Mac and physical devices were deliberately not used during the overnight work. Start with iPad 9; iPad 7 remains the performance baseline and is still required. Android/iPhone follow after the primary iPads.

## Build/install preparation

1. Verify export 42's existing artifact manifest before transfer, then verify the transferred archive/hash on the Mac. Use a separate `Builds/G2-0.0.42` destination. Existing Mac helper paths are G1-specific; update the helper for an explicit G2 destination before running it. Do not point it at the old project or reuse an unverified artifact.
2. Compile the Unity-iPhone Release target with the established family Apple team. Verify the resulting signature and app ID `com.littleweeps.familyplayset`. No app uninstall or data reset is needed.
3. Record exact existing G1 counter/profile/video preferences before installing over the same app. The new garden has its own `SoloPrototype/family-local` save and does not repurpose G1 diagnostic preferences. Check those old values still exist afterward, even though the garden does not display the diagnostic counter/video UI.
4. Install and launch only after the phone/tablet is available. Windows export success does not prove Xcode compilation, provisioning, installation or device execution. Keep the signing-refresh work separate; Wi-Fi renewal is still unresolved.

## Short play check

- Confirm the full title, controls and footer fit the iPad; drag targets must be reachable with a child's finger.
- Tap different ground locations to walk. Switch to Joystick and use one finger to walk while another drags the bucket. Neither finger should steal the other interaction.
- Drop the bucket on the tap, then the plant. A full bucket empties into the flower; a second empty pour cannot add water. Use the sponge on the puddle three times.
- Hold a toy while switching between pups. The toy and player identity must survive the switch. Start one optional activity, switch to the other and leave; objects and movement must keep working.
- Open Menu during a drag, lock/background the app during a drag, and reopen. No stuck holder or accidental pour should remain. These are real mobile lifecycle checks; Windows simulated focus tests are not substitutes.
- Press a pictured activity and Listen. English instructions must be audible, intelligible and replace earlier speech. Returning from a menu or background must not replay stale instructions. Clips have been checked muted on Windows, not auditioned on an iPad.
- In Menu, turn Voice off. Activity/Listen must remain silent and controls must still work. Reopen and confirm Voice off plus the chosen movement mode remain. Turn Voice on and request a new hint; old hints must not restart automatically.
- Leave a clear baseline: selected pup, player location, grown flower, bucket contents, cleaned puddle and prop locations. Close/reopen, then perform a later in-place update and compare the saved state exactly.
- Repeat offline solo after normal setup, without a home-server connection. This prototype is already local-only; the final game must retain all solo-capable activities offline.

Observe each child using it briefly without reading instructions to them. Record where the three-year-old cannot identify a control or drop target and adjust before producing many rooms. This prototype is not final art, animation, voice, navigation or curriculum.

## Evidence to record

Record device/model/OS, app version/signing route, actual visible results, decoded save before/after, input/audio problems and crash logs if any. Measure memory/frame pacing on iPad 7 in a sustained client test later; the desktop allocation measurement is not mobile performance qualification. Four-player networking and iPad hosting have not been implemented in this garden task.
