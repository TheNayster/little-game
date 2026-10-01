# Daycare Hide & seek and Tag — October 1, 2026

Scope: DAY-01 / HIDE-01 / HIDE-03 / NPC-01 / FAMILY-01. The user requests the existing Home hiding game and outdoor Tag as separate Daycare-menu activities, with four random classmates, optional invitations and Calypso as seeker.

## Play

- **Hide & seek with Calypso:** selecting the menu card enters an internal garden map. Everyone shares 15 seconds to hide. Tap a glowing cover to walk there and hide. Four NPC children choose distinct random places and physically walk into them. Calypso walks a shuffled route, stops to inspect each cover and reveals its occupants. Non-hiders watch; a late arrival after the count watches until the next round. Come out or move withdraws only that child. Hide again starts one new shared round.
- **Tag with friends:** selecting the menu card enters a separate green. After a shared three-second count, the star identifies the chaser. Authority-owned swept proximity transfers it, with a 2.5-second grace period. Four NPCs flee, chase and offer attainable turns; they are active runners even with one human. Arrivals have separate positions. New round replaces the NPC cast for the group.
- Other admitted players currently in Daycare receive a compact **Join friends / Not now** card. Accepting moves only that child. Declining stays dismissed for the round. Late Daycare arrivals can receive the current invitation; reconnecting from the actual activity resumes it. Starting another club replaces older outstanding club invitations.
- Both support four humans plus four NPCs. The authority saves each cast, hiding choices, route, common clock and roles. Leaving, disconnecting or reopening does not reroll the group. Human avatars never filter the NPC pool and can match an NPC.

## Existing implementation reviewed

`Core/HideAndSeek.cs` supplies the common 15-second window, cover inspection and no mid-search hiding behavior. The current outdoor Tag implementation is `Core/ParkTag.cs` and `Client/SoloTag.cs`, in **Park**, rather than Creek. Its swept-contact and grace rules are reused. Existing Home broadcast and Park presence-based joining remain their own contracts; the new Daycare clubs use the user's explicit invitations.

The maps reuse existing prepared clean lawn/park artwork under distinct internal map IDs. Six runtime covers use existing tent, bush, folding-screen and blanket-bench artwork. NPCs use the prepared character roster and the existing motion/animation presentation; Calypso uses her eight-frame walk atlas. No new generated raster assets or background characters were added. Both maps use the brighter Daycare score. The search camera follows Calypso while children hide or watch.

Final visual sampling uses the original pose-sheet dimensions (2172 × 724), so Unity's 2048-pixel import limit cannot crop the far-right sitting pose. Walking artwork is unchanged.

## Checks and delivery

Windows411: release client and dedicated server compile with zero errors/warnings. Focused Unity rules pass additive schema48 retention, invitation/decline/late fourth, varied casting independent of avatars, random covers and physical seeking, no late hiding, independent exits, deliberate replay, JSON reopening, Tag contacts and sole-client reconnect. The existing build-hook JSON and animation checks also pass.

The native411 run uses one disposable authority and four release clients at phone/tablet dimensions. Real UI touches exercise scrolling to both cards, accepting/declining, walking into covers, withdrawing, returning, replaying and steering. The four gameplay groups pass, including all four NPC reveals, teacher walk drawings, animated Tag runners/star transfers, reconnect and a cold saved-server reopening. All eleven native process exits are zero. Contact fixture placement supplements actual Tag steering to make the authority-contact check bounded.

Final412 changes teacher sampling and its dimension metadata only; Core/network/club rules match411. Final412 compiles with zero errors/warnings and passes two four-client hiding/presentation groups, with five zero process exits. A saved-round pose review uses two native clients and confirms the whole teacher drawing on phone/tablet, with three zero exits.

[Full native gameplay](evidence/daycare-hide-and-tag-2026-10-01/native-411.json) · [Final presentation](evidence/daycare-hide-and-tag-2026-10-01/native-412.json) · [Whole teacher pose](evidence/daycare-hide-and-tag-2026-10-01/teacher-pose-412.json) · [Build/source comparison](evidence/daycare-hide-and-tag-2026-10-01/build-and-source.json)

![Optional Daycare invitation](evidence/daycare-hide-and-tag-2026-10-01/small-hide-invitation-phone.png)

![Calypso seeking on tablet](evidence/daycare-hide-and-tag-2026-10-01/calypso-seeks-tablet.png)

![Tag runners on phone](evidence/daycare-hide-and-tag-2026-10-01/tag-running-phone.png)

![Complete teacher pose](evidence/daycare-hide-and-tag-2026-10-01/calypso-complete-pose-phone.png)

Schema49/content64/protocol3 adds two saved activity records and motion fields. This requires coordinated compatible client/server delivery when requested. No physical phone/iPad, installed desktop game or live family authority was changed. Family visual acceptance remains separate.

Next: requested device/server delivery and family playtesting. The four other Daycare wishlist activities and broader Home/outdoor goals remain open.

Document consistency reports only the thirteen pre-existing unclassified authored records; the new report is classified and its local links resolve.
