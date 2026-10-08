# Zoo picture navigation — October 8, 2026

Navigation-only milestone: WORLD-02 / FAMILY-01 / NAV-REF-02. The owner approved the elephant feeding appearance at build475/commit955127e. Its trays, ownership portraits, feeding cues, handoff and finish reaction are preserved. Stop for owner review before personality, water toys, care activities or another animal's feeding presentation.

## Routes and local state

The existing source defines four panels per trail and a cyclic trail order: Savanna → Dinosaur Valley → Reptile Garden → Aquarium → Savanna. Each trail also returns to the Zoo entrance. This order is preserved even though the catalog's flat species array places Aquarium before Reptiles.

| Trail | Stops in walking order | Previous boundary destination | Next boundary destination |
| --- | --- | --- | --- |
| Savanna | Elephant, Giraffe, Zebra, Lion | African penguin / Aquarium | Brachiosaurus / Dinosaur Valley |
| Dinosaur Valley | Brachiosaurus, Triceratops, Stegosaurus, T. rex | Lion / Savanna | Galapagos tortoise / Reptile Garden |
| Reptile Garden | Galapagos tortoise, Leopard gecko, Green iguana, Nile crocodile | T. rex / Dinosaur Valley | Clownfish / Aquarium |
| Aquarium | Clownfish, Blue tang, Zebra shark, African penguin | Nile crocodile / Reptile Garden | Elephant / Savanna |

Current exhibit and destinations derive from the local actor's authoritative area and position. The fixed safe-area strip uses an animal still and directional arrow for each destination, a wooden Zoo arch for entrance return, the current animal/trail symbol, and a folded-map button. Each entrance circle keeps its existing animal symbol and adds secondary trail text. Games moves to the lower middle only in Zoo areas so it does not overlap the navigation strip; other worlds keep its original top placement.

Within a trail, next/previous initiates the existing `ZooWalk` approach to an animal's center using ordinary walking. Crossing a trail boundary or returning to the entrance walks to the existing gate, then submits the unchanged authoritative gate action. No arbitrary map travel, new message, teleport, destination lock or progression requirement is added. Leaving the offering position invokes the existing actor-only cancellation; gate travel also retains existing cleanup. Other players' cameras, areas, tickets and progress remain independent.

The informational map expands one trail's four stops. Its ring and pointer identify the actual player location without covering the portrait, including when browsing another trail. Stop portraits are informational; trail buttons change only the local map view. A full safe-area raycast shield owns overlay taps. The close cross, Escape/Back and existing mouse/touch framework close it. Opening preserves an offer and pending food walk while stopping local held input; shared authority continues. Navigation uses the existing pending-action guard, an approach guard and a brief debounce to prevent stacked requests. No map/selection state is persisted.

## Artwork and changed files

Existing animal atlas pixels are reused. `ZooNavigationArt.Prepare` derives sixteen 160×160 stills through Unity texture APIs, preserving originals and GUIDs. The small portrait cache is shared by entrance, strip and map; it avoids keeping sixteen animation atlases resident. New gate/map/arrow/pointer artwork is editable native UI geometry, with no emoji, stock artwork, speech service or dependency. Sources and asset ownership are recorded in [the playable artwork guide](../../../SourceArt/Zoo/Playable/README.md).

Runtime paths below are under `Unity/FamilyPlayset/Assets/FamilyPlayset`:

- `Code/Client/Worlds/Zoo/GameScreen.ZooNavigation.cs`: local picture strip/map, routes, markers, modal input and debounce.
- `Code/Client/Worlds/Zoo/GameScreen.Zoo.cs`: replace old text gates, retain within-trail walking, share portrait ownership, reset navigation.
- `Code/Client/Shared/Sessions/GameScreen.cs`: include the map in existing input focus handling.
- `Code/Client/Shared/Navigation/GameScreen.MiniGames.cs`: Zoo-only Games placement.
- `Code/Editor/ZooNavigationArt.cs`, `ZooArtImport.cs`, `FamilyGameBuild.cs`: small still extraction/import through the established build.
- `Code/Networking/FamilyGameVerification.cs`: local navigation evidence and a permitted background synthetic keyboard, matching the existing test mouse/touch route.
- `Resources/Worlds/Zoo/Navigation` and Unity metadata: sixteen derived stills.
- `Tools/Verification/Test-ZooNavigation.py`: isolated native routes, modal/rapid input, layouts, four-player independence and recording.
- `Tools/Verification/Test-ElephantFeedingSolo.py`: saved-world navigation/marker assertions alongside the existing private feeding/reopen pass.
- `Unity/FamilyPlayset/ProjectSettings/ProjectSettings.asset`: version assigned by the established build tool.

## Acceptance and review evidence

Final standard Windows candidate **481** builds server and client with zero errors/warnings; the standard Unity prebuild checks, including Zoo JSON migration/retention/consumption, pass. All **2,372 Unity inputs** match its source manifest. Protocol3/content72/schema53 are unchanged. No Core, server rule or save field changed.

The comprehensive native480 pass uses one disposable loopback authority and four actual release clients, not four simulated player records. Passed checks:

- Every entrance picture opens its correct trail. All sixteen current animal IDs, previous/next destination IDs and rendered map marker locations match the route table above.
- All 24 distinct within-trail forward/backward routes, all eight directional trail boundaries, and entrance return from each trail work. Repeated next touches during walking do not skip stops or stack transitions.
- Phone1280×591, tablet1024×768 and smallest tested viewport640×400 retain visible pictures. Measured navigation targets are at least44×44 physical pixels, and do not overlap the world chooser or movement/menu controls. Full-view entrance/exhibit/map captures were inspected; a tablet overlap found in an earlier candidate was corrected before final acceptance.
- Actual touch feeding and entrance-position taps are blocked by the map. Escape and mouse open/close work. Selecting another map trail does not move the player; the actual location remains marked. Closing restores existing play/input focus.
- Four simultaneous elephant offers retain their tickets while one player opens the map; the shared animal continues advancing. Queued and active players independently navigate to the entrance, releasing only their offers. Remaining players retain their areas/tickets and complete feeding. Separate local-map/sibling-feeding and local-entrance/sibling-finish captures document the independence.
- Network suspension/rejoin derives valid navigation from the authoritative actor location. One separate native private client completes automatic elephant feeding, pause/resume, world switching/return and actual saved-world process reopening. Feeding history survives, unfinished offers clear under existing restore rules, and the reopened map correctly marks Elephant.
- Final shared and private native logs contain no runtime exception/error lines. No claimed physical-device qualification or subjective owner approval is inferred from those checks.

Final diff review restricted `CloseZooMap` to an actually open map, avoiding unsolicited joystick visibility updates in other worlds. This is the only runtime source change from480 to481; routes, pictures, feeding, save/reconnect and server inputs are unchanged. A focused native481 pass repeats all three layouts, map tap blocking, trail browsing, mouse/Escape closure, then verifies Zoo departure and joystick/menu focus in Creek. It passes with four actual clients and no runtime error lines. Comprehensive480 feeding/route evidence is retained for its unchanged paths.

The baseline is native build475/commit955127e. Baseline phone/tablet entrance and elephant captures are retained under `LocalData/SharedGarden/c54d97492db34f9aaa1a35a0e1e683ba/navigation`. There was no picture map to capture before this milestone.

Actual local review artifacts:

- [Before/after gallery and all sixteen markers](../../../LocalData/ZooNavigation/review.html).
- [Phone entrance](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/phone-entrance.png), [phone exhibit](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/phone-exhibit.png), [phone map](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/phone-map.png).
- [Tablet entrance](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/tablet-entrance.png), [tablet exhibit](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/tablet-exhibit.png), [tablet map](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/tablet-map.png), [small-phone map](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/small-phone-map.png).
- [44.84-second actual silent gameplay recording](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/navigation-silent.mp4): entrance → real gate walking → next animal → map → entrance. No fixture placement occurs within this final recording. Native frame timestamps are retained; H.264 is padded by one pixel to1280×592. Sequence frames, including the map at13seconds, were inspected; the owner can review the film at normal speed.
- [Independent map](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/independent-map.png), [sibling feeding](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/sibling-feeding-with-map-open.png), [independent entrance](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/independent-entrance.png), [sibling finish](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/sibling-finish.png).
- [Shared results](../../../LocalData/SharedGarden/fa656b99b5c548279a473ef7d4db1fe8/navigation/results.json), [private results](../../../LocalData/FamilyLAN/d3f3c516da9c4efebbf7dd3db94bfb75/elephant-solo/results.json), [source matching](../../../LocalData/ZooNavigation/source-check.json). Target bounds, raw film frames and disposable native logs remain with these runs.
- [Final481 focused results](../../../LocalData/SharedGarden/d48d8c3a486b419f80f60fa3993de779/navigation/results.json), [phone exhibit](../../../LocalData/SharedGarden/d48d8c3a486b419f80f60fa3993de779/navigation/phone-exhibit.png), [tablet exhibit](../../../LocalData/SharedGarden/d48d8c3a486b419f80f60fa3993de779/navigation/tablet-exhibit.png), [phone map](../../../LocalData/SharedGarden/d48d8c3a486b419f80f60fa3993de779/navigation/phone-map.png), [tablet map](../../../LocalData/SharedGarden/d48d8c3a486b419f80f60fa3993de779/navigation/tablet-map.png). The gallery uses final481 layout captures and the unchanged480 comprehensive markers/recording.

Earlier captures caught shared portrait unloading and tablet top-control overlap. Both were repaired. The test keyboard was given the same background capability as the existing synthetic mouse/touch; no input/security protection was disabled. A Windows console encoding failure in the earlier recording wrapper was corrected with UTF-8 output, then the final480 pass completed successfully. Failed/interrupted runs remain retained separately and are not final acceptance evidence.

No physical-device installation, live-server replacement or security change occurred. Physical touch/safe-area qualification and owner navigation appearance/usability review remain open. The previously blocked standalone .NET Zoo executable was not retried or reported as passing; permitted Unity/native routes supply the listed evidence. Pre-existing unrelated untracked audit material, rootPackages/ProjectSettings and preserved worktrees remain untouched. Source delivery follows main/origin synchronization; this does not mark the wider Zoo feature phase complete.

Next task: **owner review of this navigation milestone only**. No automatic feature expansion.
