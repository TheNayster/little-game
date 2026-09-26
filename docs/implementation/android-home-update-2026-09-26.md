# Android home update — build 115

September 26, 2026. User requested an immediate phone update during walking-animation research. **Little Weeps updated in place from 110 to 115** on the existing Samsung Android. Package `com.littleweeps.familyplayset`; release/non-development build, pinned family signing identity, 4 KB device configuration.

The previously prepared 114 source manifest differed from the current files, so a fresh Unity Android build 115 was made and signed from the current checkout. No older APK was substituted. Source and signed-payload manifests accompany the local artifact; builds, backups and credentials remain outside Git.

## Verified

- Fresh Unity build succeeded; signing verified the existing pinned certificate and unchanged build payload.
- The installer checked the installed package/certificate, used `adb install -r`, read back the installed APK and matched its SHA-256 to the intended signed artifact, then launched the exact package.
- Physical screenshot showed the illustrated home with the new interactive sofa/radio and Bingo. This proves the home presentation opened, not exhaustive mobile interaction or animation acceptance.
- Both local saved worlds were backed up before installation. After launch their world IDs, existing toy IDs and every prior toy field remained intact: five toys in the older local branch and ten in the paired solo branch. The latter upgraded schema 3 to 4, adding container fields and home state. The user's subsequent play can move the avatar and append bounded receipts; save retention is not based on a data-directory identifier alone.

**Installed APK SHA-256:** `126ab18440aada0b6b6ef418b2dc149759f089583dfdfd35f2c4520244086a16`.

Private evidence: `LocalData/Verification/android-phone-749504f1d61f4cbaa1e68139d6b28205/installation.json`; before/after saves and phone capture under `LocalData/Verification/android-114-user-update/` (folder was named before the fresh build number was selected). Public, non-sensitive result: [verification record](evidence/android-home-update-2026-09-26/result.json).

## Current limits

Phone status is `solo-available`. The live family server/helper remains 110/content 4, while phone 115 contains home content 5/schema 4. The phone update does not upgrade or qualify the shared server. Apple updates remain deferred; no Apple device was changed.

The existing 16 KB native qualification gate remains open; this installation was checked on the observed 4 KB phone. WALK-01 has not been implemented: this update delivers the [previous home interaction pass](home-interactions-2026-09-25.html). Continue the [walking appearance research/plan](walk-animation-research-2026-09-26.html).
