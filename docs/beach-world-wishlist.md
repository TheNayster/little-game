# The Beach world wish list

Reviewed September 30, 2026. This file collects the beach additions recorded in the Little Weeps docs so the family can choose the next activity to build. The ten named activities below are existing planned designs; their detailed mechanics are proposals from the goal sheet, rather than proof that every detail has received separate user approval.

The beach has a long walkable world and the original **Seashell Skipping** music, with shared gulls and the new waves/footprints slice in built candidates. Rock pools remain in the original background reference; the new playable shore variants clear obstructions to the wet-sand strip. [Scenery record](implementation/scenic-worlds-2026-09-25.md) · [Music record](implementation/world-music-2026-09-28.md) · [Beach audit](all-world-features-audit-2026-09-26.md#the-beach)

## Ten planned beach activities

**September 30 update:** BCH-02 Seagull surprise and BCH-03 Waves and footprints have first shared playable slices. Windows 304/305 adds wet-sand washing, four-player paw trails, touch ripples and random whale/dolphin/mermaid jumps with focused core/Unity JSON and native four-client checks. [Waves record](implementation/beach-waves-2026-09-30.md) · [Gulls record](implementation/seagull-surprise-2026-09-30.md). Sound/pose polish, family acceptance and device/server delivery remain open. The other eight remain **Planned**.

| Activity | What to add |
| --- | --- |
| **BCH-01 Shell treasure** | Pick up shells, hear a gentle sound and put them in a tray. Sort by shape, decorate castles, make patterns and carry a favorite to a bedroom shelf. |
| **BCH-02 Seagull surprise** | Approach or tap a flock so the birds flap away and settle again. Follow their footprints and approach together. No capturing, hitting or chase score. |
| **BCH-03 Waves and footprints — partial** | Shared moving foam, touch ripples and walking paw prints; waves erase only wet-sand trails. Random whale, dolphin and mermaid jumps are included. No compulsory timer or wave dodging. |
| **BCH-04 Sandcastle workshop** | Scoop sand, add water, press a mould, lift it and decorate with flags or shells. Offer ready-filled moulds for easier play. Players can contribute towers and walls to a shared castle or build separate creations. |
| **BCH-05 Beach ball** | Gentle bouncing and rolling, passes to family players or an NPC, a large hoop target and an optional shared rally with forgiving catches. |
| **BCH-06 Flying disc** | Choose a pictured receiver and tap Throw. Offer optional drag-and-release aiming, wide catch areas and an NPC partner for solo play. Misses land nearby. The older note interpreted “freezeb” as Frisbee; this remains the recorded interpretation. |
| **BCH-07 Crab and footprint trail** | Follow large footprints to a waving crab. Use crab-walk or hopping poses, make a trail and lead another player to a shell. The crab remains a beach animal rather than an inventory collectible. |
| **BCH-08 Sand pictures** | Draw broad marks and arrange shells or pebbles into faces, spirals, patterns or a shared mosaic. Save a postcard locally without a camera or internet. |
| **BCH-09 Beach picnic café** | Pack a basket, arrange a blanket, plate snacks, pour drinks, serve picture orders and wash the props. Reuse the kitchen and cleanup interactions. |
| **BCH-10 Kite meadow** | Tap to launch a kite and use a large handle to make it swoop. Add a tail, choose a wind ribbon and fly together through broad cloud shapes. Automatic recovery; no tangled-string simulation. |

Source: [Goal sheet section 37](bluey-game-research-2026-09-23.md#37-the-beach-collecting-building-and-playing-together). The [beach audit](all-world-features-audit-2026-09-26.md#ten-beach-activities) tracks partial implementations and the remaining planned activities.

## Three optional beach invitations

These are prompts using the activities above, rather than three additional games:

- **QUEST-13 Sandcastle:** lift a filled mould, or prepare sand and water yourself and add a shell.
- **QUEST-14 Shell pattern:** put shells on a picture, follow a short pattern or invent your own design.
- **QUEST-15 Bucket helpers:** bring water to a castle while other players build or decorate.

Source: [Starter quest ideas](bluey-game-research-2026-09-23.md#3-six-content-regions-five-destinations-and-eighteen-starter-quest-ideas) and the beach audit's starter invitations.

## How beach play should work

- Support **up to four players together** in one shared activity, with enough tools and space for everyone. The latest project instruction supersedes the older two-child examples and two-tool counts. A game with rounds needs shared participation, a common start and shared progression.
- Let each player stop, change character or travel independently while the others continue. Passes follow the player profile, even after an avatar change; a departed receiver leaves the ball or disc safely reachable.
- Connect the toys: collect a shell, rinse it in a bucket, decorate a castle, use it in a pattern and carry it home. Buckets should fill, pour and rinse across compatible stations. Carrying a shell between worlds must preserve one object, without duplication.
- Keep sand play forgiving: dry, damp, moulded and decorated states; no exact water ratio required. Repeated input must not create unlimited toys.
- Keep saved castles and pictures protected. Waves can fade footprints or wash a deliberately disposable practice castle, but must not erase saved creations. Players must not overwrite another player's completed tower or decoration.
- Offer direct object play with large touch targets, optional spoken help and easy exits. The current Games menu contains Hide & seek only; listing a beach activity here does not add it to that menu.
- Install beach art, ambience, prompts and solo partners locally. Private offline play uses the same activity rules; reconnecting resumes the authoritative PC/VPS world, without merging offline edits.

Sources: [Beach object and co-op rules](bluey-game-research-2026-09-23.md#make-the-beach-objects-connect) · [Outdoor persistence rules](bluey-game-research-2026-09-23.md#43-building-saving-and-testing-the-outdoor-and-daycare-expansion) · [Current decisions](current-decisions.md) · [Project instructions](../AGENTS.md).

## Reference ideas that need a scope choice

The Bluey Let's Play reference study also records **swimming, splashing and ice cream** at its beach. Swimming has a proposed waterline and swimming-state approach in that study, but swimming and ice cream have no separate entry in our ten-activity beach backlog. Keep them visible as reference ideas to discuss, without silently treating them as separately approved Little Weeps requirements. [Reference location catalog](bluey-lets-play-reference-study-2026-09-25.md#15-location-catalog-and-evidence-coverage) · [Reference activity families](bluey-lets-play-reference-study-2026-09-25.md#16-games-and-activity-families)

## Suggested first task

**Recommendation: start with shell collecting and one sandcastle workshop.** It gives players a useful loop immediately: collect a shell, fill a bucket, mould a tower and decorate a castle together. This also matches the goal sheet's first outdoor batch and supplies reusable pieces for shell pictures, water play and bucket helpers. This is a suggested starting point, not a new implementation commitment.

A bounded first slice would provide shells and a tray, enough buckets/scoops/moulds for four participants, one shared castle with optional separate builds, shell decoration and save/reopen. Check that four players can contribute together and that one leaving does not interrupt the remaining players. Final visuals receive family playtesting.

Source: [Outdoor production order](bluey-game-research-2026-09-23.md#a-practical-production-order).

## Review record

The initial review consolidated the goal sheet, beach audit, current decisions, build-guide work record, interaction supplement, reference study and scenery/music evidence without changing the game. The user then chose Seagull surprise; its first slice is recorded above. The existing main integration hold remains in effect. Next: family playtesting of that slice on requested delivery; all ten activities remain tracked.
