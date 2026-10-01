# Daycare repair — September 30, 2026

The family reported broken fruit pickup, missing group entry, glitching NPCs/Calypso, little story context and an unclear picnic. Earlier cast/menu/tap-to-walk checks did not establish that the installed game worked in joystick mode or through these real paths.

**Windows candidate358 passes all eight native groups. Source is isolated on `codex/daycare-adventure-repair`; no phone/iPad or live-server update occurred.** Goals: DAY-01, LEARN-01, IMG-01 and FAMILY-01. This repairs the first Adventure and counting slice; it does not complete the other eight stories or eleven learning stations.

## What changed

- A story opening explains the missing feast, broken crossing, guarded wand and frozen friends. Original local recordings explain each stage. Navigation can be opened during the introduction.
- Fruit taps now approach and pick up in joystick mode. Players visibly carry fruit and deliver it to a separate basket. The first repair attempt revealed overlapping fruit/basket touch areas; the basket now sits at the starting area.
- One start joins connected players in Daycare, up to four. Late arrivals and players returning from another world automatically join the retained round. Explicit Return/Leave keeps that player out for the current visit; siblings continue. Disconnect releases only temporary holds/participation.
- Varied, distinct saved NPC casts remain independent of player avatars. Replay selects different NPCs. The guide moves ahead, a helper packs food, builders carry planks, the guard follows the ball, rescued friends move and everyone joins the feast. Routes continue between stages instead of turning back to their initial anchors. World-coordinate motion and one pose update per frame prevent camera-induced walking/idle flicker.
- Calypso follows a smooth world-space route and keeps her standing drawing while travelling. Reading/sitting poses appear after arrival. This uses the existing four drawings; a full walking sprite cycle is still absent.
- Picnic has a labelled plate stack, visible carried plates and green empty places. Take a plate, give it to a friend, then hear the shared count. Four players share one table; leaving releases that player's plate and preserves siblings' plates, counts and NPCs.

## Checked scope

`dotnet run --project Tools/AdventureRepair.Tests/AdventureRepair.Tests.csproj` passes shared rules, exclusive fruit holds, delivery, old-schema migration, saved holds, stage routes, late/world-return joins, independent exit/disconnect and four-player plate contribution. Fresh Unity builds run the JSON migration checks; legacy schema42 identity/cast survive upgrade to43.

`python Tools/Test-AdventureRepair.py 358` passes [eight native groups](evidence/daycare-repair-2026-09-30/result.json) on a disposable release server with four native clients. It uses real UGUI touches and joystick mode, actual world menus, independent departure/disconnect, complete rescue/feast, reopening/replay and four-player picnic. Calypso's position/pose is sampled over time with an actual camera pan. All nine settled NPCs retain Dance across twelve samples including camera movement. Current production code hashes match358's source manifest.

[Story opening](evidence/daycare-repair-2026-09-30/story-opening-phone.png) · [Three fruit carriers](evidence/daycare-repair-2026-09-30/three-fruit-carriers-phone.png) · [Plate stack](evidence/daycare-repair-2026-09-30/picnic-plate-stack-phone.png) · [Four counted plates](evidence/daycare-repair-2026-09-30/four-counted-plates-phone.png) · [Calypso travelling](evidence/daycare-repair-2026-09-30/teacher-in-travel-pose.png) · [Teacher motion samples](evidence/daycare-repair-2026-09-30/teacher-motion.json).

## Delivery status

Candidate358 uses protocol3/content53/schema43 because shared participation, held props and job timestamps changed. It includes the installed347 Zoo gate-arrival correction from07b2af7; the separate undelivered movement-speed candidate is not included. The permanent authority was observed as347/content52/schema42 near the end of this task and was left alone. Main integration remains held. A future coordinated server/client update must reconcile any other pending shared changes and follow the current server-update policy. Native Windows evidence does not establish physical phone/iPad performance, animation approval or family listening acceptance.
