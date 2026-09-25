# Back-home testing checklist — 24 September 2026

> **Historical implementation record — current scope changed September 25.** PC/VPS is the sole shared authority; clients continue private solo if disconnected and load server state on rejoin. G4/AUTO-02 device hosting and automatic offline imports are removed. The dated results below remain evidence, but their old “next”, “required” and device-version statements are not the active plan. Use [current decisions](../current-decisions.md), the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) and the [current return checklist](return-checklist-ipad-lan-2026-09-24.html).

<!-- historical-record-start -->
The user tried the two Windows player windows and reported a positive experience. This is useful hands-on feedback, not an item-by-item acceptance test. The shared Windows preview is build 57. **Solo garden build 56 is now installed and tested on iPad 9**: it does not yet connect two iPads to the shared Windows game.

## First: the newer iPad

- [x] **User:** Mac awake and available on the home network.
- [x] **User:** newer **iPad 9 (A2602)** connected to the Mac over USB; local signing prompt completed.
- [x] **Assistant:** export verified, native build/signature passed, and **20 → 56** installed and launched without uninstall/reset. Existing profile, 1,019 taps and 8.9-second bookmark retained. [Native update evidence](ipad-garden-2026-09-24.md).
- [x] **Together:** full layout, tap walking, watering, spoken instructions, joystick plus dragging, menu/screen-lock cancellation, exact garden save/reopen, voice/movement settings persistence and Wi-Fi-off solo play passed. The saved plant is fully watered and the puddle cleaned. Voice is restored on; Joystick remains selected. [Physical and saved-data results](ipad-garden-2026-09-24.md).

## Then, in order

- [x] **Older iPad 7 (A2197), first play/restart round:** device setup, separate signing, installation receipt and launch passed; the user reported repeating the newer iPad checks with smooth play, and native reopening retained the exact complete save/preferences. [Evidence and open app-inventory diagnostic](ipad7-garden-2026-09-24.md).
- [ ] **Older iPad measured performance and remaining qualification:** measure frame pacing/memory during sustained representative play; close additional feature, media, update and inventory checks. Smooth first play is not this measurement.
- [ ] **Brief child playtest:** let each child try moving, dragging, listening and leaving an activity. Record confusing controls before copying them into more rooms. Placeholder art and voices are still provisional.
- [ ] **Secondary phones:** qualify the garden on Android, then iPhone, after the primary iPads. The existing phone installations are earlier foundation builds; do not assume they contain the garden.
- [ ] **Separate signing-refresh session:** finish proving a reliable renewal route that preserves saves. The iPhone's manual Windows USB refresh passed; unattended renewal and Wi-Fi detection remain unresolved. Neither iPad has completed Windows renewal qualification. Do not repeat all wireless troubleshooting during the first garden test.

Both iPads now run the solo garden and their first play/restart results are recorded. The older iPad app-inventory diagnostic and remaining qualification gates are explicitly open. [Older iPad record](ipad7-garden-2026-09-24.md). No account password or verification code should be sent in chat; any native account/keychain prompt is handled on the Mac itself.

## Work that stays queued

Independent logical areas and per-player travel remain the next bounded Windows networking task. Native family discovery/pairing, four physical mixed-platform players, iPad hosting and automatic recovery remain required later gates. Today's positive Windows feedback does not mark those complete.

An independent backup/restore destination also remains to be settled; it is separate from making the first iPad garden test available. See the [build guide](../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) for the full sequence.
<!-- historical-record-end -->
