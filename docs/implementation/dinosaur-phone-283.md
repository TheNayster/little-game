# Bluey/Bingo dinosaur wardrobe phone delivery — September 30

Android production release **283** was signed with the existing pinned family certificate and installed in place over **260** on the Samsung phone. The installer verified the installed APK hash and launched `com.littleweeps.familyplayset`; existing local player and toy IDs remain in both saved branches (5 and 120 pre-existing toys).

The first Android attempt, 282, failed shader compilation. The dinosaur cloth shader now uses full-precision values throughout to avoid Unity's Android cross-compiler generating invalid min-precision bytecode. Release 283 compiles successfully for GLES3/Vulkan. The phone renders the prepared green Bluey onesie and Roar button; a green dinosaur choice is saved. Earlier native four-client evidence covers the outfit window and four color choices. Phone color-window playtesting remains with the family.

The live server remains **227-status-io-1**, content 31/schema 30. The Android compatibility planner returned `unknown` because this combined source has no matching compiled server contract. This is an installed private phone preview; it cannot join the older server. No server, enrollment, firewall or other device was changed. A coordinated matching server release remains required for shared play. The main integration hold is preserved.

Local build evidence: `Builds/AndroidSigned/G3-0.0.283/`; exact installation receipt: `LocalData/Verification/android-phone-8b04a466b4ad46f283639ec06b177fe5/installation.json`; save retention and native captures: `LocalData/Verification/outfit-phone-283/`.
