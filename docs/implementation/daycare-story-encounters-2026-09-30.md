# Adventure story encounters — September 30, 2026

Goals: IMG-01 / IMG-07, FAMILY-01 and DAY-01. The previous Adventure was mostly a fixed sequence of item counters. This task adds conversations and choices with visible shared consequences to that first story. The other eight imagination stories remain planned.

## Research applied

The [official episode](https://www.bluey.tv/watch/season-1/the-adventure/) establishes a kingdom, changing pretend roles, food, encounters, the Greedy Queen, a tennis-ball trick and rescuing frozen friends with a wand. Optional packing help, stepping stones and inviting a lonely queen are original game adaptations. [NAEYC play guidance](https://www.naeyc.org/node/3796) emphasizes choice, discovery and delight. Applying those principles does not prove this game is fun.

## Playable story

- The Kindly Queen introduces three frozen friends beyond the river. Castle camp, orchard, crossing and queen's grove have signs; the story map visits their actual locations and records progress.
- Actual random NPCs have named portraits and spoken conversations. Gather fruit yourself or ask the orchard friend to pack while you explore; helpers never take fruit from a player's hands.
- Choose a bridge or stepping stones. This changes the props, instructions and NPC jobs. Stone taps produce hops; bridge helpers carry and construct with planks.
- Talk to the Greedy Queen. Play ball for the timed wand opportunity, or invite her to the feast so she leaves the wand and walks toward the group. The invitation has no ball timer.
- Frozen friends offer a sparkle spell or silly dancing spell. They wake, move and celebrate. The ending and journal remember your shared choices.
- Up to four players share the same round. Same-world players and late arrivals join automatically; leaving remains independent. Saved NPC casts are varied and never depend on player avatars. Deliberate replay selects new friends and resets decisions.

Role labels remain optional pretend identities. Mounted horse/rider gameplay, unique role abilities and the other eight stories are not added here. Family playtesting/listening acceptance remains open.

## Verification

Windows **376** compiles both release roles. Core and Unity JSON checks cover legacy migration, retained identity/cast/holds, both story routes, helper delivery, delayed wand sharing and saved choices. All346 C# source hashes match the built manifest.

[Six gameplay groups](evidence/daycare-story-encounters-2026-09-30/result.json) pass across two native four-client runs: assisted packing/stones/invitation/rescue, then reopening that exact saved checkpoint and replaying manual food/bridge/ball. The first includes spoken-dialogue playback, late entry, independent departure and the ending/journal. The second verifies direct fruit pickup/delivery at960×640 and a new cast on replay.

Rapid basket taps arrived before the pickup reply and were discarded despite fruit already appearing held. The next prop approach now waits for that reply. Earlier camera/touch-adapter theories did not fix this; that adapter was removed.

**Limit:** after its four gameplay groups completed, one Windows client returned a nonzero exit after writing a clean stopped status during shutdown. The first whole run is not counted as passing. The separate same-checkpoint reopening/replay run passes; Windows quitting remains unqualified. Physical-device performance and family fun/listening acceptance are not claimed.

[Opening](evidence/daycare-story-encounters-2026-09-30/kingdom-opening-phone.png) · [Orchard](evidence/daycare-story-encounters-2026-09-30/orchard-conversation-phone.png) · [Crossing choice](evidence/daycare-story-encounters-2026-09-30/river-choice-phone.png) · [Queen](evidence/daycare-story-encounters-2026-09-30/queen-conversation-phone.png) · [Journal](evidence/daycare-story-encounters-2026-09-30/shared-story-journal-phone.png).

## Delivery

Schema45/content59/protocol3 adds saved story decisions and hop timestamps to the existing combined360 game. Completed Home/Park/Creek/Beach/Zoo/Dinosaur work and accepted movement are retained. No device installation or live-server replacement occurred in this task. A later coordinated update must follow the current server-update policy.
