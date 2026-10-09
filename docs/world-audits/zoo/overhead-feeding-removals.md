# Zoo overhead feeding and activity removal — October 8, 2026

Scope: WORLD-02 / FAMILY-01 / ITEM-02, all sixteen exhibits. This owner instruction supersedes Zoo decoration placement, Missing Ball/ball-return stories and tray feeding, including older pilot acceptance and later rollout proposals. Preserve the original habitat scenery, ordinary toys and remaining approved activities.

## Implemented scope

Removed the ball-return and placement UI partials, their geometry/hit areas, authority handlers, scheduling, carrier guards, ownership/preview controls and activity-only build checks. Shared dispatch explicitly rejects stale `story-*` and `habitat-*` commands. No species configuration or trail can instantiate either activity. Removed diagnostic properties and standalone verification references alongside their source. No shared art asset was deleted; original habitat backgrounds, fixed water/care equipment, surprises, photo album and fossil activity remain.

Restored the pre-955127e feeding pattern selectively: food remains at its player's position throughout carrying, waiting and eating. The held food now uses one overhead socket above the existing 180-unit character art and follows the same displayed shared position as that player. The animal reaches the held food; the food never moves toward the animal. Elephant trunk, giraffe/Brachiosaurus neck and other species' fitted head/mouth regions articulate against the retained original atlas pixels; fish swim to the offering. Original species reach/eat/finish frames remain. Serving a prepared snack carries the selected food pieces overhead without its preparation bowl. Preparation still uses the bowl; quick bucket feeding remains.

All sixteen exhibits keep four assigned positions and the existing authoritative tickets, consumption moment, independent exits and finish counters. Player portraits/local markers, waiting/approach/eating/finished feedback are retained or shared with the remaining species. No feeding trays, delivery chutes, receiving branches or tall supports remain.

## Saved state and compatibility

Old Zoo `story` and `habitat` JSON fields are ignored and omitted on subsequent saves. Reserved enum values15–20 normalize to unlocked Rest before validation, preserving RNG, completed feeding counts, ticket sequence and unrelated world state. No activity-specific toy existed in the general item inventory; removed carriers/placement-use locks were exclusively in those retired fields. The existing restore policy still clears transient offers/care as before; fossil progress and other saved progress are unchanged. The normal build JSON gate includes obsolete-field/six-phase cleanup and retained-history checks.

Shared rules/state availability changed, so content79→80; schema53/protocol3 remain. A future deployment requires coordinated compatible clients/server. This task does not deploy or bypass admission to the installed server.

## Verification and delivery

Verified target Unity bridge compilation, then fresh standard528 client/server release builds succeeded with zero errors/warnings. All 2774 recorded Unity/Tools inputs match current source. The normal JSON gate passes obsolete fields and all six saved legacy phases, existing feeding/snack/remaining build checks. Initial526 stopped on a test-fixture off-by-one phase value (corrected15-20);527 passed, and528 includes the final low-head reach mask that preserves ground feet. Failed526 log and527 artifacts remain in ignored build/log locations. [Build summary](../../../Builds/NetworkProbe/G3-0.0.528/build-summary.json), [source manifest](../../../Builds/NetworkProbe/G3-0.0.528/source-manifest.json) and [build log](../../../LocalData/Logs/build-network-528.log). Document rendering/plan consistency also pass. Owner requests source/build checks only: no Computer Use, control tests, multiplayer sessions, live demonstration, recording, device installation, live-server replacement or DX12 investigation. Native animation/layout acceptance is reserved for the owner's review. Pre-existing untracked audit/evidence files and other local work are preserved.

Remaining approved activity rollout must omit placement decorations and ball-return stories across every species and use overhead feeding for all bucket/prepared food. Existing water, care, surprises, snack preparation, album, navigation and fossils remain in scope under their separate requests; no new activities were added here.
