# /// script
# dependencies = ["markdown"]
# ///
"""Build the source-traceable world feature inventory; no game/device access."""
from collections import Counter
from pathlib import Path
import hashlib
import html
import json
import re
import markdown
from markdown.extensions.toc import slugify

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / 'docs'
NAME = 'all-world-features-audit-2026-09-26'
GOALS = DOC / 'bluey-game-research-2026-09-23.md'
research = GOALS.read_text(encoding='utf-8')
chapters = {int(m[1]): (m[2], m[0]) for m in re.finditer(r'^## (\d+)\. (.*?)(?:\n)(.*?)(?=^## \d+\.|\Z)', research, re.M | re.S)}
WORLD = {'all':'Across all worlds', 'home':'Heeler Home — house', 'yard':'Heeler Home — backyard',
         'beach':'The Beach', 'creek':'The Creek', 'park':'Playground & Park', 'daycare':'Daycare',
         'ops':'Parent setup and reliability', 'retired':'Removed from scope'}
STATUS = {'play':'Playable prototype', 'part':'Partial', 'plan':'Planned', 'dev':'Development only',
          'scene':'Scenery only', 'optional':'Optional idea', 'retired':'Retired'}
EVIDENCE = {
 'hiding':'implementation/hide-and-seek-hide-to-join-2026-09-30.html',
 'hideresearch':'implementation/hide-and-seek-research-2026-09-28.html',
 'worldmusic':'implementation/world-music-2026-09-28.html',
 'ramps':'implementation/marble-ramps-2026-09-28.html',
 'mealflow':'implementation/meal-preparation-2026-09-28.html',
 'pizzaflow':'implementation/pizza-preparation-2026-09-28.html',
 'cakefamilies':'implementation/cake-families-2026-09-28.html',
 'creations':'implementation/home-creation-storage-2026-09-28.html',
 'tidying':'implementation/home-idle-cleanup-2026-09-28.html',
 'scienceart':'implementation/home-science-coloring-research-2026-09-27.html',
 'discovery':'implementation/home-discovery-2026-09-27.html',
 'mixing':'implementation/home-mixing-2026-09-27.html',
 'ice':'implementation/ice-rescue-2026-09-28.html',
 'bubbles':'implementation/bubble-lab-2026-09-28.html',
 'liquid':'implementation/liquid-colors-2026-09-28.html',
 'nav':'implementation/combined-chooser-2026-09-25.html',
 'scene':'implementation/scenic-worlds-2026-09-25.html',
 'home':'implementation/home-interactions-2026-09-25.html',
 'layers':'implementation/integrated-home-2026-09-26.html',
 'keepy':'implementation/keepy-uppy-2026-09-26.html',
 'rooms':'implementation/upstairs-foundation-2026-09-26.html',
 'bedrooms':'implementation/bedroom-rooms-2026-09-26.html',
 'secrets':'implementation/secret-rooms-2026-09-26.html',
 'kitchen':'implementation/kitchen-easy-2026-09-27.html',
 'cookingflow':'implementation/kitchen-staged-cooking-research-2026-09-27.html',
 'cakeflow':'implementation/chocolate-cake-flow-2026-09-27.html',
 'roomplay':'implementation/room-object-play-2026-09-26.html',
 'furniture':'implementation/bedroom-furniture-2026-09-26.html',
 'walk':'implementation/selected-sheet-characters-2026-09-26.html',
 'solo':'implementation/ipad-garden-2026-09-24.md',
 'family':'implementation/family-home-rollout-2026-09-26.html',
 'ledger':'family-playset-build-guide-2026-09-23.html#all-35-feature-requirements-implementation-status',
 'outage':'implementation/g3-offline-authority-and-hitch-review-2026-09-25.html',
 'books':'implementation/home-books-2026-09-26.html',
 'reader':'implementation/native-reader-controls-2026-09-28.html',
 'media':'../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Runtime/FoundationVideo.cs',
 'core':'../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/SoloWorld.cs',
 'layout':'../Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/WorldLayout.cs',
 'vps':'implementation/vps-hosting-plan-2026-09-24.html',
}

def section(n): return chapters[n][1]

def source(n):
    title = f'{n}. {chapters[n][0]}'
    anchor = slugify(title, '-')
    return f'[Research §{n}](bluey-game-research-2026-09-23.html#{anchor})'

def plain(s):
    return re.sub(r'<[^>]+>', '', markdown.markdown(s)).strip()

def tables(text):
    result=[]; current=[]
    for line in text.splitlines()+['']:
        if line.startswith('|'):
            cells=[c.strip() for c in re.split(r'(?<!\\)\|',line)[1:-1]]
            if cells and not all(re.fullmatch(r'[-: ]+',c or '-') for c in cells):current.append(cells)
        elif current:result.append(current);current=[]
    return result

features=[]
def add(fid, world, group, title, status, nums, description, evidence='', extra_source=''):
    features.append(dict(id=fid, world=world, group=group, title=title, status=status,
                         chapters=list(nums), description=description, evidence=evidence, extraSource=extra_source))

# Authored synthesis of prose requirements; named source catalogs below are copied
# as individual entries so the inventory cannot silently omit a recipe or activity.
MANUAL = r'''
G-01|all|Play and presentation|Four independent family players|part|1,31,47,48|One person per device; any arrival/departure changes only that player. Four-device admission exists; full content and sustained mixed-device acceptance remain open.|family
G-02|all|Play and presentation|Independent worlds, cameras and local loading|part|3,31,50|Independent world/room views include four owned furnished bedrooms and four optional persistent secret rooms in candidate 146. Broader activities and mixed-device qualification remain required.|secrets
G-03|all|Play and presentation|Bluey-style illustrated dollhouse|part|1,2,5|Detailed wide scenery, readable outlines, perspective floors, foreground occlusion and recognizable characters. Scenic shells and initial home layers exist; art alone does not make furniture usable.|layers
G-04|all|Play and presentation|One visible object per usable object|part|5,51|Separate room architecture, rear/front furniture, supports and movable props. Sofa, trampoline and shed have integrated layers; kitchen and remaining furniture still need this treatment.|layers
G-05|all|Controls and menus|Joystick and tap-to-walk|play|5,48|Independent device preference, floor movement, stop on release and safe cancellation. Current floor bounds are not a complete furniture-aware pathfinding system.|solo
G-06|all|Controls and menus|Walk around furniture and through doors|part|5,32|Working stairs/doors plus two authored bedroom arrangements and a clear front walk corridor. Arbitrary placement and wider house routing remain unfinished.|furniture
G-07|all|Controls and menus|Family circle, horizontal characters, vertical places, down arrow|play|3,17|Lower-right circle avoids the joystick; the tray keeps the avatar visible above it. One Heeler Home entry; the Creek stays available. Only Bluey and Bingo currently appear as playable choices.|nav
G-08|all|Controls and menus|Local loading and safe travel failure|part|3,12,50|Prepare destination art and accepted world state before enabling input; retry/back on failure without moving siblings. Existing scenic travel works; new room/story loaders need coverage.|scene
G-09|all|Controls and menus|Generous touch targets and gesture ownership|part|2,5,48|Large picture targets, safe areas, pointer ownership, drag versus walking separation and cancellation on menus/lock. Broad physical child-usability qualification remains open.|solo
G-10|all|Controls and menus|Simple Play and Explore & Stories|plan|2,18,30,41|Per-child and per-skill assistance, ready setups, broad snapping, demonstrations, tap alternatives and deeper sequences. No age-based character/content locks.|ledger
G-11|all|Activities and speech|Free play and optional invitations|part|2,18|Objects work without quests. Current flower/cleanup prompts can be left and repeated; the complete picture picker, nearby invitations and resumable activities remain planned.|core
G-12|all|Activities and speech|Activity picker, replay, switch, all done, resume|plan|18,24,43|Picture cards, spoken invitation, listen again, personal start/park/resume and independent shared participation. Leaving does not erase food, art or other players' work.|ledger
G-13|all|Activities and speech|No forced progress gates|part|1,2,18,42|All released worlds, favorite characters and toys stay accessible; no required chores, reading, win/loss or elimination wait. Apply this to every future activity.|keepy
G-14|all|Activities and speech|English voices and speaking characters|part|8,17,26|Local reviewed prompts, reactions, names, greetings and mouth animation. A small English hint set exists; full cast voices and complete English content do not.|solo
G-15|all|Activities and speech|Spanish and separate local language choices|plan|8,26,41|Reviewed recordings, natural translations and Spanish-specific sounds/rhymes. Dialect remains a content decision; prototype translations are not a completed mode.|ledger
G-16|all|Activities and speech|One foreground voice, music ducking and replay|part|8,26,27,41|Coalesce repeated requests, cancel stale speech and keep visual hints when muted. Candidate 217 ducks six world scores and existing radio/quiet ambience under books or hints. Broader book/TV/lesson voice arbitration remains planned.|worldmusic
G-17|all|Activities and speech|Separate voice, music, effects and calm settings|part|11,33|Candidate 217 adds six world scores with local saved mute, independent travel and softer bedrooms. Separate voice and secret-room brightness/motion/ambience/chime controls remain. Broader assistance, language and media settings stay planned.|worldmusic
G-18|all|Characters|Switch any available avatar without losing identity|part|4,17,47|Bluey/Bingo switching keeps profile and current supported state; all players may choose the same favorite. Full roster, parent avatars and all future role/grip cases remain open.|walk
G-19|all|Characters|Player badges distinct from NPC roles|part|4,17,47|Duplicate favorites need persistent readable symbol/color markers. Human Bandit/Chilli must not seize or remove an NPC seeker. Current player identity exists; parent/NPC roles are not built.|ledger
G-20|all|Characters|Natural movement and context poses|part|4,5,17|Accepted Bluey/Bingo sheets support walking, idle, sitting, bounce, dance and balloon tap. Finish turns/back views, grip/contact and special actions for every cast member.|walk
G-21|all|Shared objects|Drag, carry, release and one holder|part|5,6,50|Current bucket/sponge/ball ownership is authoritative. Expand correct hand anchors, handovers, portable cross-world props and safe rejected placement to the full object catalog.|core
G-22|all|Shared objects|Capacity-limited fill and pour|part|6,19,51|Bucket/tap/plant transfer exists. Extend consistent quantities and cancellation to cups, basins, recipes, watering cans, sand and all compatible targets.|core
G-23|all|Shared objects|Supported placement, stacking and moving supports|plan|5,6,51|Tables, trays, shelves and stable stacks need visible valid targets; moving a support must carry or safely settle its dependents. Ground dragging is not a general surface system.|ledger
G-24|all|Shared objects|Containers and durable nested contents|part|6,51|Four fixed shed slots retain items. Portable baskets, bags, drawers, cupboards, capacities, nested depth/cycle rules and carried contents remain planned.|home
G-25|all|Shared objects|Borrowed tools return automatically|part|51|Five-minute Home timers reset independent science trays and return kitchen/book stock with held-item and creation protection. General typed loans, spoken cues and food/creation archives remain open.|tidying
G-26|all|Shared objects|Enough tools and places for four|part|31,47,51|Four sofa/trampoline places exist in unchanged artwork. Other stations must offer four-person participation and essential tool stock; the single prototype bucket is not sufficient.|keepy
G-27|all|Shared objects|Protect personal creations and prevent hoarding|part|32,51|Separate fixed stock, essential tools, bounded loans, personal items, creations, supplies and effects. Count nested loans; preserve art/food and offer recoverable toy-box storage. Candidate 205 adds bounded saved food and picture collections with owner bedroom display; broader loans/storage remain open.|creations
G-28|all|Shared objects|Reusable reactions and reversible changes|part|2,6,19,30|Visible empty/full, dirty/clean, growth and valid rejection exist in the small water loop. Add material transforms, serving reactions, paint removal, undo and stable custom creations.|core
H-01|home|Rooms and furniture|Connected house, veranda and backyard|part|3,32|Connected downstairs property, working stairs/landing and four owned furnished bedrooms. Kitchen, bathroom/laundry, veranda functions and broader room content remain required.|furniture
H-02|home|Rooms and furniture|Living-room sofa for four|play|5,47|Four close places, seated poses, foreground masking, avatar switch and independent exits. Available in 130/132; installed iPads/server 128 retain two places.|keepy
H-03|home|Rooms and furniture|Living-room radio and automatic dancing|play|1,5|Radio on plays music and nearby idle characters dance; movement and other actions take priority. Local mute is independent of shared power.|home
H-04|home|Rooms and furniture|Chairs, benches, cushions and resting spots|part|4,5,32|Beds and four independent bedroom cushions now work, with layered rest/sit poses. Broader seating, held-item seating and final cast/device qualification remain required.|furniture
H-05|home|Rooms and furniture|Cupboards, drawers, lights and lamps|part|6,11,32,51|Bedroom lamps switch and eight-slot chests open/close around real retained items; shelves have four supports. Wider cupboards/drawers and appliance systems remain required.|furniture
H-06|home|Rooms and furniture|Bathroom and laundry spaces|plan|3,21|Bath/splash interaction with front water masking, towels, storage, dressing and bedtime connections. This room expansion is listed in the home layout record.|ledger
H-07|home|Kitchen|Interactive kitchen architecture|part|3,19,51|Working illustrated fridge, cupboards/worktops, sink, oven and four dining places. Candidate 166 moves dining beside appliances and migrates occupied supports; physical child/device acceptance remains open.|kitchen
H-08|home|Kitchen|Make, decorate and serve|part|19|Candidates 207, 210 and 212 add distinct staged paths for all five cakes, five pizzas and five meals. Four hobs/oven places preserve independent preparation, food and servings. Child/A10 acceptance, orders, album, additional polish and independent recovery after 205 remain open.|mealflow
H-09|home|Kitchen|Free recipes and persistent food creations|part|19,51|Chocolate cake retains partial mixture/transfer, coverage, layers and decorations through saved stages and unique portions. Legacy dishes keep their original rules/content; unusual combinations require explicit Experiment mode. Candidate 205 stores and retrieves the same food, freeing reusable trays and preserving ingredients/portions. Other recipe transformations, orders, picnic packing and album remain open.|creations
H-10|home|Kitchen|Four-player preparation and safe ovens|part|19,31,47|Four independent cookware/tool sets, oven positions and dining seats. Authority selects free trays atomically and records cook profiles; one leaving player does not interrupt others. Native qualification is recorded; physical mixed-device acceptance remains open.|kitchen
H-11|home|Kitchen|Drinks, fruit, blender and pretend café|plan|6,19|Slice/blend fruit, fill cups, serve, wash and keep bounded contents. Reuses the Toca/Piknik object catalog; not yet a home appliance feature.|ledger
H-12|home|Parents and hiding|Bandit and Chilli's ambient routines|plan|22,47|Roam, read, garden, prepare food and tidy eligible ambient props; requests interrupt safely, but never steal a busy seeker or destroy a child's work.|ledger
H-13|home|Parents and hiding|Parent-seeker hide-and-seek|part|22,34|Bandit and Chilli alternate first-level turns with pictured invitations, large fifteen-to-one countdown numbers, parent-follow cameras for hidden players, looking pauses, one broadcast countdown, hiding before zero to join, ignored nonhiders and independent withdrawal during search and friendly finds. Observation/clue options, speech and physical qualification remain.|hiding
H-14|home|Parents and hiding|Child/human seeker and role changes|plan|22,34,47|Picture role choice, independent hider preparation, swap roles, and an NPC replacement if the human seeker leaves. Character choice never changes the role.|ledger
H-15|home|Parents and hiding|Enterable hiding furniture|part|34|Ten enterable spaces include the original six plus a folding screen, dining-table nook, blanket bench and garden bush. Larger pictured Hide buttons with gentle glow, local cutaways, concealed remote occupants and safe exits work; upstairs hiding furniture remains.|hiding
H-16|home|Parents and hiding|Fair clues and bounded search|part|34|First-level search uses checked-cover memory, nearby unvisited targets and visible look pauses. Sixty stationary slot/parent/start-position cases measure at most 72.95 seconds after preparation. Sight/sound clues, settings and upstairs routes remain.|hiding
H-17|home|Parents and hiding|Concealment and mid-round joins|part|22,31,34|HS-1 conceals hidden avatars and held props, keeps profile/possessions through avatar changes and supports independent late-join preparation, re-hide, exit, travel and suspension. Human-seeker filtering and physical qualification remain.|hiding
H-18|home|Books and reading|Reading nook and physical book props|part|25|Original six-book phone preview: shared rack, independent local reader/bookmarks, explicit narration and effects; final subjects and mixed-device qualification remain open. Low shelves, rug, cushions and book basket. Tap/open versus drag/move must be distinct; access to the same title stays independent for every player.|books
H-19|home|Books and reading|Interactive narrated reader|part|25,26|Native candidate 198: translucent picture controls, explicit Read to me, fixed arrows and optional settings. Eight Windows groups cover four independent readers, old bookmarks, six drafts/54 pages, sound cancellation and lifecycle. Final content, physical audio/A10 and device qualification remain open. Large Play/Pause, replay, page arrows, close, optional automatic/manual turns and saved page. Opening alone does not start narration; relevant hotspots animate and say names.|reader
H-20|home|Books and reading|Independent bookmarks and narration|part|25,26,29|Native candidate 198: translucent picture controls, explicit Read to me, fixed arrows and optional settings. Eight Windows groups cover four independent readers, old bookmarks, six drafts/54 pages, sound cancellation and lifecycle. Final content, physical audio/A10 and device qualification remain open. Each profile keeps pages and narration locally; names pause/resume only valid current narration. Muting, closing, changing pages or network state cannot restart old speech.|reader
H-21|home|TV and local media|TV and thumbnail library in the house|plan|27|TV/remote opens local cards with titles, duration and posters; no web search/feed needed. Media/TV folder preparation is not an importer or playable TV.|ledger
H-22|home|TV and local media|Playback controls and persistent resume|dev|27,29|Single-clip foundation proves play/pause, seeking and a profile bookmark. Integrate library, per-video Continue/Restart, skip, previous/next, finish choices and durable lifecycle checkpoints.|media
H-23|home|TV and local media|Parent media import and retained library|plan|27|Import normal files through Files; validate space/format, preserve current library on cancellation, detect duplicates and retain local media/bookmarks through updates.|ledger
H-24|home|TV and local media|Independent offline playback|plan|27,35|One local decoder per device, correct aspect ratio, retry/back on missing clips, independent siblings, paused return after lock. Physical target media acceptance remains required.|ledger
H-25|home|TV and local media|Watch together or autoplay-next|optional|27|Explicitly later than the first local TV version; invitations and matching clip IDs would be needed for shared viewing.|ledger
H-26|home|Dinosaur play|Twenty-type accessible toy shelf|plan|28|Picture categories, roughly six visible at a time, all types available without quests and deliberate duplicate-toy choice. Twenty definitions do not mean unlimited active toys.|ledger
H-27|home|Dinosaur play|Named animated toys with connected uses|plan|26,28|Carry, place, rotate/flip, stack/store, footprints/dust, brushing, washing, nests and book links. Same reviewed name ID across books and toys.|ledger
H-28|home|Dinosaur play|Dinosaur Discovery Mat|plan|28|Uncover toy, brush, hear name, optionally wash and arrange its world. Bypass digging if wanted; toys and creations survive independent departures.|ledger
H-29|home|Science|Discovery bench and free experiments|part|30|Candidate 174 adds saved boats, magnets and additive light; 178 adds four SCI-09 mixing variants. Candidate 191 adds SP-02 dinosaur rescue with hammer, water melting and four persistent independent trays. Candidate 192 adds the preparation-based bubble lab with four saved mixtures. Candidate 194 adds SP-15 liquid colors with ratios, dilution and four saved mixtures. Candidate 216 adds SCI-03 adjustable marble ramps with four kept courses and independent releases. Remaining science prototypes, the unfinished portions of all nine original stations and physical acceptance stay required.|ramps
H-30|home|Science|Shared experiments and saved creations|part|30,31,47|Candidate 174 has four profile-owned persistent trays and independent reset/travel; 178 adds sixteen saved chemistry trays. Candidate 194 also retains four ice, bubble and liquid-color states with independent reset/undo and native restart/rejoin evidence. Narration, portable creations and physical device qualification remain open.|liquid
H-31|home|Bedrooms|Four persistent player-owned bedrooms|part|32|Four saved owned rooms include usable furniture/storage and candidate-155 cuddle/tuck/stack/tea play. Broader catalog and physical qualification remain open.|roomplay
H-32|home|Bedrooms|Decorating, visits and undo|part|32|Owner/Together decoration and undo include bedding, rug, picture and lamp choices. Two safe arrangements remain; free placement and physical acceptance are open.|roomplay
H-33|home|Bedrooms|Personal toy box and creation gallery|part|28,32,51|Each room has four persistent starter toys, an eight-slot chest and four shelf supports. The full dinosaur/personal catalog, put-one-away/take-one-out and creation gallery remain required.|furniture
H-34|home|Bedrooms|Gentle tidy help and visitor protection|part|32,51|Owner tidy preserves held/stored toys, nests and stacks, other owners and visitors. Broader loan/creation integration remains open.|roomplay
H-35|home|Secret rooms|Four optional mini-door secret rooms|part|33|Four optional persistent rooms now have owner creation, far-back approach-revealed star entrances, move/hide/show controls and safe exits independent of the entrance. Physical qualification remains open.|secrets
H-36|home|Secret rooms|Plush collection and quiet play|part|33|Candidate 155 adds four cushion cuddle places, four saved blanket nests, three-toy piles and four-place pretend tea. Shared downstairs books can travel here. Large pillow art, broader catalog and physical acceptance remain open.|roomplay
H-37|home|Secret rooms|Stars, aurora and local calm controls|part|33|Calm sky and local controls include an optional fact picture with Read to me. Physical A10 and final listening qualification remain open.|roomplay
H-38|home|Secret rooms|Independent visits and no surprise search|part|32,33,34|Four visitors or four separate secret rooms retain independent travel and shared layouts. Hiding-round withdrawal/search integration remains a future dependency.|secrets
Y-01|yard|Scenery and play|Long backyard connected to the house|scene|3|Walk from the house through veranda/tree/trampoline scenery to the rear shed with local camera panning. Illustrated swing/pool/toys do not imply functioning interactions.|scene
Y-02|yard|Water and planting|Tap, bucket, plant and sponge loop|play|6|Drag to fill, pour to water/grow, and clean the puddle. Water/holder/state save, repeat and synchronize. This is the small prototype, not full garden inventory.|solo
Y-03|yard|Water and planting|Hose, watering can, multiple plants and pots|plan|3,6,30|Seed/soil/pot relations, filling, pouring, growth and movable decorated plants. Extend source capacity rules without replacing the existing loop.|ledger
Y-04|yard|Water and planting|Sandpit and mud play|plan|3,19|Fill/dampen/lift moulds, decorate saved sand creations and make pretend mud pizzas; allow rebuild and recovery without failure.|ledger
Y-05|yard|Furniture and equipment|Trampoline for four|play|5,47|Four close spots, bounce poses, mat reaction, shadows, local front/rear layers and independent exits. 130/132 have four; deployed iPads/server 128 have two.|keepy
Y-06|yard|Furniture and equipment|Garden radio and automatic dancing|play|1,5|Shared power, local music mute and nearby idle dancing with movement/use taking priority.|home
Y-07|yard|Furniture and equipment|Shed open, store, close and retrieve|play|51|Four fixed storage targets preserve actual bucket/sponge/ball identities and contents through close, travel and reopen. Larger hooks/bins/interiors remain to expand.|home
Y-08|yard|Furniture and equipment|Expanded shed shelves, hooks and bins|plan|51|Clear readable tool, watering and toy storage zones, capacities, compatible surfaces and safer nested storage; not merely painted storage scenery.|ledger
Y-09|yard|Furniture and equipment|Tree swing and hanging/resting seats|plan|5|Authored seat/rope grips, readable arc, safe entry/exit and four-person activity design. Current painted swing is not a usable ride.|ledger
Y-10|yard|Furniture and equipment|Wading pool and splash play|plan|3,5|Shallow valid play zone, front-water occlusion, paddling/splashing response and easy exits. Existing pool art alone is decoration.|ledger
Y-11|yard|Furniture and equipment|Wagon, toy car and cargo|plan|3,51|Load, carry/roll, unload and wash compatible toys with retained cargo. Handholding and other advanced responses need their own implementation.|ledger
Y-12|yard|Fishing|Backyard fishpond catch-and-release|plan|20,47|Toy rods, visible fish, forgiving cast/reel, observation bowl and release; five proposed fish designs and picture discovery album. Four usable roles/tools, exclusive catch identity.|ledger
Y-13|yard|Fishing|Fishing cancellation and discovery|plan|20,31|No missed-bite penalty or deadline; a full bowl offers release/another spot. Leaving releases unfinished catches; completed discoveries remain.|ledger
P-01|park|World foundation|Long park and playground scenery|scene|3,39|Walkable scenic shell exists. Playground machinery, NPC games and riding are planned; painted equipment must not be reported as working rides.|scene
C-01|creek|World foundation|Long creek scenery and inherited water fixture|part|3,38|Walkable creek exists with a second instance of the garden water rules. That fixture does not implement creek fishing, stones, boats or nature discovery.|core
B-01|beach|World foundation|Long beach scenery|scene|3,37|Walkable scenic shell exists. Interactive waves, animals, shells and the ten named beach activities remain planned.|scene
D-01|daycare|World foundation|Long daycare scenery|scene|3,40|Walkable scenic shell exists. Calypso, classmates, lessons, day board and story sessions are not implemented.|scene
D-02|daycare|Rooms and routine|Classroom and activity zones|plan|40|Classroom, yard, book corner, art/sensory tables, pretend kitchen, quiet cushions and imagination mat; no extra top-level world bubbles.|ledger
D-03|daycare|Rooms and routine|Calypso and available classmates|plan|4,40|Teacher greeting/reading/observing/helping/resting routines; illustrated friend board and full requested child roster spread across zones. NPCs yield essential toys.|ledger
D-04|daycare|Rooms and routine|Optional day board|plan|40|Hello/cubby, read-aloud, first adventure, snack/counting, second adventure, music/art/discovery, optional third adventure, quiet story/goodbye. Skip or leave at any time.|ledger
D-05|daycare|Rooms and routine|Two or three varied saved invitations|plan|40|Pick implemented, installed, solo-capable activities across world families; avoid recent repeats and save choices. Joining never rerolls the day or teleports siblings.|ledger
D-06|daycare|Rooms and routine|Personal schedules and field trips|plan|40|Each child keeps their own card and may replace unstarted suggestions, paint, visit another world or begin a new pretend day without rewriting another's active session.|ledger
D-07|daycare|Learning|Spoken lessons and adjustable help|plan|41|Show, invite, wait, acknowledge and optional demonstration; skill-specific assistance, no grades/streak loss, microphones or reading requirements; language-specific reviewed examples.|ledger
D-08|daycare|Imagination|Picture mat, spoken roles and independent stories|plan|42|All nine story pictures remain unlocked; listen, choose role, play, change role, listen again and return. Up to four distinct lightweight sessions; NPCs fill missing roles.|ledger
D-09|daycare|Imagination|Safe story joining and departure|plan|31,42,43|Joining loads the current story, never restarts others. Preserve completed props/actions; autopilot or NPC takes necessary jobs and every player can exit.|ledger
O-01|ops|Connection and saves|Automatic enrolled family LAN connection|part|7,44,46,47|PC authority, up to four mixed clients, known family identity and compatible content, no child Host/Join steps. Admission evidence exists; sustained failure qualification stays open.|family
O-02|ops|Connection and saves|PC authority survives client departure|part|31,45,47|Closing or moving one client releases its temporary holds and leaves connected players active. Persistent state and parent status tooling exist.|family
O-03|ops|Connection and saves|Private offline play and server-wins return|part|35,45,50|Installed solo features run locally. Disconnected work stays in private saves; reconnect loads the server's world without importing edits. All future content still needs offline coverage.|outage
O-04|ops|Connection and saves|Autosaves, backups, migration and recovery|part|10,11,13,29,36,43,53|Preserve good saves and malformed evidence, migrate explicitly, release stale leases and reject incompatible future data. Scoped recovery exists; independent backup/restore remains a release gate.|ledger
O-05|ops|Connection and saves|Parent server panel and dependable startup|part|31,44,49|Status, saved family, recovery and supervision exist. Actual automatic sign-in/reboot startup remains unqualified; do not infer current uptime from older reports.|ledger
O-06|ops|Future hosting|Owned VPS as future shared authority|plan|55|Qualified remote endpoint/server build, credentials, backups and one-writer migration from PC; preserve family identities and offline fallback. No VPS deployment has occurred.|vps
O-07|ops|Future hosting|Away-from-home shared play|plan|35,49,55|Internet/hotspot route to designated VPS after qualification; private route to home PC is an alternative. Without a route, each device plays solo.|vps
O-08|ops|Devices and delivery|In-place signed updates with saved data|part|11,13,14,48|Windows builds Android, Mac signs Apple; retain package identity, saves/preferences/media. Prior updates have evidence; 132 is built but not installed while the family is away.|family
O-09|ops|Devices and delivery|Automatic Apple signing renewal|plan|14,35,48|Required unattended renewal must preserve data, coexist with network routes and work under documented reachable-device conditions. Manual signed installs do not complete it.|ledger
O-10|ops|Devices and delivery|Older-iPad performance and full lifecycle proof|part|12,13,29,36,43,48|Measure A10 client/solo memory, frame times, media, rapid repeated travel and long four-device play. Scoped tests do not establish final performance or crash freedom.|ledger
O-11|ops|Devices and delivery|Phone layouts and current platform compatibility|part|12,48|Safe areas, landscape targets, local settings and four-player UI; exact signed releases on each device. Android native 16 KB qualification and wider lifecycle checks remain open.|ledger
O-12|ops|Production|Local installed art, speech and media|part|8,9,10,29,54|Reusable content IDs, editable art/voice masters, reviewed content and bounded loading. Templates/package candidates are engineering choices, not completed game features.|ledger
O-13|ops|Production|Parent save/media/settings management|part|11,27,51|Deliberate destructive actions, usable storage diagnostics, retry/back and protection of children's creations; full media/room management is not built.|ledger
O-14|ops|Optional extensions|AR presentation|optional|15,52|Optional later experiment, not a dependency of the 2D family game.|ledger
R-01|retired|Superseded architecture|Device hosting, host election and switching|retired|44,45,52,53|Removed by the September 25 decision. Clients never become the shared authority; G4/AUTO-02 have no pending implementation gate.|ledger
R-02|retired|Superseded architecture|Bluetooth/router-free peer co-op|retired|7,35,52,53|Removed; nearby offline devices do not create an alternate shared family world.|ledger
R-03|retired|Superseded architecture|Automatic offline merging/import|retired|32,35,45,51|Removed. Preserve private saves separately; reconnection uses authoritative server state.|ledger
'''

for line in MANUAL.strip().splitlines():
    fid,world,group,title,status,nums,description,evidence=line.split('|')
    add(fid,world,group,title,status,map(int,nums.split(',')),description,evidence)

def table_with(n, heading):
    return next(t for t in tables(section(n)) if heading in plain(t[0][0]))

# Every original item ID is retained, even when related activities reuse one engine.
for row in table_with(19,'ID')[1:]:
    add(row[0],'home','Fifteen kitchen recipes',row[1],'plan',[19],row[2],'ledger')
for n,prefix,world,group in [(37,'BCH','beach','Ten beach activities'),(38,'CRK','creek','Ten creek activities'),(39,'PRK','park','Twelve park activities'),(41,'LRN','daycare','Twelve learning stations')]:
    for t in tables(section(n)):
        for row in t[1:]:
            m=re.match(r'('+prefix+r'-\d+) (.+)',row[0])
            if m:add(m[1],world,group,m[2],'plan',[n],f'Simple play: {row[1]} Deeper play: {row[2]}','ledger')
for row in table_with(30,'ID and station')[1:]:
    m=re.match(r'(SCI-\d+)\s*—\s*(.+)',plain(row[0]))
    integrated=m[1] in ('SCI-01','SCI-02','SCI-03','SCI-04','SCI-06','SCI-09')
    note=' Candidate 187 adds illustrated equipment and smooth RGB presentation to the saved trays; deeper interactions and physical acceptance remain open.' if integrated else ' Planned; not implemented in Unity.'
    if m[1]=='SCI-03':note=' Candidate 216 adds a ready three-ramp course, six adjustable endpoints, wood/felt/rubber resistance, four owned saved courses and releases, explicit Keep/Restore, undo and creation-preserving five-minute cleanup. Native qualification is recorded in the linked report; physical A10/child/mixed-device acceptance and broader construction remain open.'
    if m[1]=='SCI-06':note=' Candidate 192 adds native preparation, dipped film, big/little round bubbles, round/square wands, soft/strong air, direct popping and four saved independent trays with reset/undo. Fan/wind, broader shared catching and physical child/A10/audio acceptance remain open.'
    if m[1]=='SCI-09':note=' Candidate 187 adds the illustrated workbench and layered glass to fizz/foam, indicator colors, oil/water and oobleck, retaining direct pouring and sixteen saved trays. Physical/A10/audio acceptance and portable creations remain open.'
    add(m[1],'home','Science experiments',m[2],'part' if integrated else 'plan',[30],f'Shared downstairs. Simple play: {row[1]} Explore together: {row[2]} Factual constraint: {row[3]}'+note,'ramps' if m[1]=='SCI-03' else 'bubbles' if m[1]=='SCI-06' else 'mixing' if m[1]=='SCI-09' else 'discovery' if integrated else 'scienceart')
for row in table_with(30,'Coloring ID')[1:]:
    integrated=row[0] in ('COL-01','COL-02','COL-03','COL-04','COL-05','COL-06','COL-08')
    note=' Candidate 187 has eighteen fixed pages including twelve official Bluey sheets, a picture chooser, bounded per-profile save/undo and four-client evidence. Blank strokes, full folders/gallery, creation carrying/Together and physical acceptance remain open.' if integrated else ' Planned; no game implementation yet.'
    if row[0] in ('COL-02','COL-03'):note+=' Candidate 202 adds native rounded picture controls, six large chooser previews per screen, nonwrapping page navigation and six native four-player/layout/save-retention groups. No device rollout.'
    if row[0] in ('COL-05','COL-06'):note=' Candidate 205 adds four kept-picture places per profile, immutable saved copies, an owner-controlled bedroom display and recoverable removal. All eighteen working pages remain separate and persistent. Creation carrying, Together editing and physical qualification remain open.'
    add(row[0],'home','Shared downstairs coloring',row[1],'part' if integrated else 'plan',[30],row[2]+note,'creations' if row[0] in ('COL-05','COL-06') else 'discovery' if integrated else 'scienceart')
for i,row in enumerate(table_with(26,'Working title')[1:],1):
    add(f'BK-{i:02}','home','Six starter books',plain(row[0]),'plan',[25,26],f'{row[1]} proposed. {row[2]}','ledger')
for i,row in enumerate(table_with(28,'Dinosaur toy')[1:],1):
    title=plain(row[0]);add(f'TOY-{i:02}','home','Twenty dinosaur types',title,'plan',[28],f'{row[1]} Production batch: {row[2]}.','ledger')
for i,row in enumerate(table_with(28,'Optional invitation')[1:],1):
    add(f'DISC-{i:02}','home','Five dinosaur invitations',plain(row[0]),'plan',[28],f'Simple play: {row[1]} Deeper play: {row[2]}','ledger')
for i,row in enumerate(table_with(21,'Activity')[1:],1):
    add(f'CLEAN-GAME-{i:02}','yard' if i==5 else 'home','Five cleanup activities',row[0],'part' if i==3 else 'plan',[21],f'Simple play: {row[1]} Deeper play: {row[2]} Preserve: {row[3]}'+(' Current sponge/puddle loop is only a subset; mop and muddy trails are not built.' if i==3 else ''),'core' if i==3 else 'ledger')

show_world={i:'home' for i in range(1,33)}
show_world.update({9:'yard',11:'yard',23:'creek',25:'yard',26:'park',29:'yard',31:'yard',32:'yard'})
for t in tables(section(23)):
    for row in t[1:]:
        m=re.match(r'SHOW-(\d+) (.+)',row[0])
        if not m:continue
        i=int(m[1]);fid=f'SHOW-{i:02}'
        if i==25:
            desc='The balloon starts on clear lawn past the trampoline. Ordinary and occasional higher auto-returns vary direction, speed and distance; the quicker descent and movement while tapping remain. One balloon for all four, Home only, no score/win/loss; landing rests until tapped. Build 151 passes core/native flight and save-upgrade checks; physical tuning acceptance remains open.'
        else:desc=f'Simple play: {row[2]} Deeper play: {row[3]} Reuse / original priority: {row[4]}. Suggested first location; portable game families may later appear elsewhere.'
        add(fid,show_world[i],'Show-inspired activities',plain(m[2]),'play' if i==25 else 'part' if i==1 else 'plan',[23],desc+(' Ten first-level spaces and alternating Bandit/Chilli searches work for four players; clues and upstairs search remain.' if i==1 else ''),'keepy' if i==25 else 'hiding' if i==1 else 'ledger')
add('SHOW-EXTRA','yard','Optional extensions','Pirates swing-ship adventure','optional',[23],'Additional researched candidate outside the 32-card selection: steering and spotting picture landmarks; needs bespoke swing/character work.','ledger')

for m in re.finditer(r'^### (IMG-\d+) — (.*?)\n(.*?)(?=^### |\Z)',section(42),re.M|re.S):
    paragraphs=[x for x in m[3].split('\n\n') if x.startswith('**Our game:**') or x.startswith('Reuse ')]
    add('STORY-'+m[1],'daycare','Nine imagination stories',m[2],'plan',[42],' '.join(paragraphs).replace('**Our game:** ','')+' Roles support four family players and NPC substitutes.','ledger')

# Recipe targets remain partial until bespoke gestures, album and physical acceptance are finished.
for feature in features:
    if feature['id'].startswith(('PIZ-', 'CAK-', 'MEAL-')):
        feature['status']='part';feature['evidence']='kitchen'
        feature['description']+=' A complete prototype prepare/heat/serve/taste/wash path now exists; special animation and physical qualification remain open.'
        if feature['id'] in ('CAK-01','CAK-03','CAK-04','CAK-05'):
            feature['evidence']='cakefamilies'
            feature['description']+=' Candidate 207 implements distinct staged preparation and art, with six native four-cook groups and retained servings/storage. Physical acceptance and independent recovery qualification remain open.'
        if feature['id'].startswith('PIZ-'):
            feature['evidence']='pizzaflow'
            feature['description']+=' Candidate 210 implements dough kneading/rolling, saved sauce coverage, vegetable chopping, matching toppings, shared safe baking and conserved slices. Physical acceptance, cheese-stretch polish and independent recovery qualification remain open.'
        if feature['id'].startswith('MEAL-'):
            feature['evidence']='mealflow'
            feature['description']+=' Candidate 212 adds recipe-specific pan/pot preparation, four hobs, separate cooking passes and saved food/servings. All 273 core groups, six native four-cook groups on 211 and three presentation/retention groups on 212 pass. Physical and independent recovery qualification remain open.'
        if feature['id']=='CAK-02':
            feature['evidence']='cakeflow'
            feature['description']='Candidate 171 implements chocolate batter mixing, partial pour into two tins, safe bake, filling, layer assembly, icing coverage, placed decorations, slicing and four unique servings. Saves retain partial work; old dishes stay on their original version. Physical child/A10 acceptance and spoken guidance remain open.'

# Starter invitations are separate source entries, with links to fuller activity
# designs in the same inventory. They are not all additional unique mini-games.
place={'Home':'home','Backyard':'yard','Park':'park','Creek':'creek','Beach':'beach','Daycare':'daycare'}
for i,row in enumerate(table_with(3,'Area / quest')[1:],1):
    area,title=row[0].split(': ',1)
    status='play' if title=='Balloon fun' else 'part' if title=='Thirsty flower' else 'plan'
    desc=f'Simple play: {row[1]} Explorer version: {row[2]} Reusable rule: {row[3]}.'
    if title=='Balloon fun':desc='Fulfilled in the current Keepy Uppy direction: direct balloon tap, automatic raised-arm returns and free play. The earlier target-guidance suggestion is deferred; it is not part of build 132.'
    if title=='Thirsty flower':desc+=' Current fixture waters one plant; the three-plant sequence remains planned.'
    add(f'QUEST-{i:02}',place[area],'Eighteen starter invitation ideas',title,status,[3],desc,'keepy' if title=='Balloon fun' else 'core' if title=='Thirsty flower' else 'ledger')

roster=json.loads((DOC/'bluey-research/character-references.json').read_text(encoding='utf-8'))
for r in roster:
    names=['Terrier 1','Terrier 2','Terrier 3'] if r['name']=='The Terriers' else [r['name']]
    for name in names:
        status='play' if name in ('Bluey','Bingo') else 'optional' if name in ('Digger','Mia','Captain') else 'plan'
        note='Playable selected-sheet prototype.' if status=='play' else 'Reference exists; no playable rig/content integration.'
        if name=='Pretzel':note='Requested roster entry retained; official picture reference still needs verification.'
        if name=='Lulu':note+=' Existing reference is a head portrait, not a full body specification.'
        if name=='Dougie':note+=' Preserve Auslan/visual communication with a spoken companion guide; do not rewrite his characterization.'
        if name.startswith('Terrier'):note+=' Individual avatar required; existing reference is a group portrait. Numbered labels are provisional.'
        add('CAST-'+re.sub(r'[^a-z0-9]+','-',name.lower()).strip('-'),'all','Character roster',name,status,[4],note,'walk' if status=='play' else 'ledger')

# Supplementary object families are useful requirements, not extra world bubbles.
toca=(DOC/'toca-piknik-interaction-research-2026-09-23.md').read_text(encoding='utf-8')
objects=next(t for t in tables(toca) if t[0][0]=='Object or station')
for i,row in enumerate(objects[1:],1):
    status='part' if i in (1,2,7,12,20) else 'plan'
    add(f'OBJECT-{i:02}','all','Reusable object catalog',row[0],status,[6,51],f'Main use: {row[1]}. Compatible uses: {row[2]}. Feedback/recovery: {row[3]}.'+(' Only a narrower prototype interaction exists; this complete object family is not finished.' if status=='part' else ''),'core' if status=='part' else 'ledger','[Toca/Piknik object catalog](toca-piknik-interaction-research-2026-09-23.html#6-a-concrete-object-catalog-for-our-game)')
add('OPTION-HAIR','home','Optional extensions','Hair styling and extra dress-up transformations','optional',[4,23],'Reversible styling appears in the commercial-reference study. Treat hair tools as a candidate requiring selection, not proof of an implemented salon. Required show dress-up and accessories remain in their own entries.','ledger','[Interaction reference](bluey-lets-play-reference-study-2026-09-25.html#18-characterobject-interaction-specification)')
add('OPTION-FOSSIL','home','Optional extensions','Fossils and other prehistoric animals','optional',[28],'Later fossil assembly requires reviewed anatomy. Flying/marine prehistoric reptiles would use a separate category; they are not part of the 20 dinosaur-type commitment.','ledger')

TRACK_STATUS={
'CHAR-01':'part','CHAR-02':'part','FAMILY-01':'part','ACT-01':'part','COOK-01':'part','FISH-01':'plan','CLEAN-01':'part',
'HIDE-01':'part','HIDE-02':'plan','NPC-01':'part','CAT-01':'part','BOOK-01':'part','TV-01':'dev','DINO-01':'plan','DINO-02':'plan','LAB-01':'part',
'JOIN-01':'part','WORLD-01':'part','WORLD-02':'part','ITEM-02':'part','ITEM-03':'part','STOCK-01':'part','ROOM-02':'plan','NET-02':'part',
'REMOTE-01':'plan','ROOM-01':'part','SECRET-01':'plan','HIDE-03':'part','TRAVEL-01':'part','AUTO-01':'part','AUTO-02':'retired','OUT-01':'plan',
'DAY-01':'plan','LEARN-01':'plan','IMG-01':'plan'}
TRACK_MAP={
'CHAR-01':'G-18, G-20; Character roster','CHAR-02':'G-18, G-19; Character roster','FAMILY-01':'G-01, G-26, O-01, O-10',
'ACT-01':'G-11–G-13; QUEST-01–18','COOK-01':'H-07–H-11; PIZ, CAK, MEAL','FISH-01':'Y-12, Y-13; CRK-02',
'CLEAN-01':'CLEAN-GAME-01–05; Y-02','HIDE-01':'H-13–H-17; SHOW-01','HIDE-02':'H-14, H-17','NPC-01':'H-12, H-13; D-03',
'CAT-01':'SHOW-01–32; SHOW-EXTRA optional','BOOK-01':'H-18–H-20; BK-01–06','TV-01':'H-21–H-25','DINO-01':'H-26, H-27; TOY-01–20',
'DINO-02':'H-28; DISC-01–05','LAB-01':'H-29, H-30; SCI-01–09','JOIN-01':'G-01, G-12, D-09; O-01–O-03',
'WORLD-01':'G-02, G-08, H-01; P-01, C-01, B-01, D-01','WORLD-02':'G-02, G-12; H-20, H-24, D-09',
'ITEM-02':'G-21, G-22, G-24','ITEM-03':'G-25, G-27','STOCK-01':'G-26, G-27','ROOM-02':'H-33, H-34',
'NET-02':'O-01–O-05','REMOTE-01':'O-06, O-07','ROOM-01':'H-31–H-34','SECRET-01':'H-35–H-38','HIDE-03':'H-15–H-17',
'TRAVEL-01':'O-03, O-07, O-12','AUTO-01':'O-01, O-05','AUTO-02':'R-01','OUT-01':'BCH, CRK, PRK; B-01, C-01, P-01',
'DAY-01':'D-01–D-06','LEARN-01':'D-07; LRN-01–12','IMG-01':'D-08, D-09; STORY-IMG-01–09'}
tracker=[r for r in table_with(16,'ID')[1:] if r[0] in TRACK_STATUS]

def refs(f):
    result=' · '.join(source(n) for n in f['chapters'])
    if f['extraSource']:result+=' · '+f['extraSource']
    if f['evidence']:result+=f' · [Implementation evidence]({EVIDENCE[f["evidence"]]})'
    return result

INTRO='''# Little Weeps — all-world feature audit

September 26, 2026 · implementation findings retained from main source **8a315ca** and build/device evidence. Four-bedroom/four-secret-room scope corrected later the same day by user request; this correction is documentation only, not a fresh code audit. See the [Home feature tracker](home-world-feature-tracker.md). The later [upstairs research](implementation/upstairs-bedrooms-research-2026-09-26.html) updates the work order and specifications only; it does not itself upgrade implementation statuses. Subsequent [BED-1 candidate 135 evidence](implementation/upstairs-foundation-2026-09-26.html) updates only the scoped stair/landing entries; [BED-2 candidate 137](implementation/bedroom-rooms-2026-09-26.html) adds four owned architectural room destinations, working doors and scoped persistence/visit evidence. [BED-3 candidate 142](implementation/bedroom-furniture-2026-09-26.html) adds usable furniture, bounded personal toys/storage and decoration permissions/undo. [Secret-room candidate 146](implementation/secret-rooms-2026-09-26.html) adds optional persistent rooms, proximity-revealed doors, fort/plush/storage and calm controls. Full bedroom/secret content and physical qualification remain unfinished.

This is the consolidated feature checklist from the **55-chapter Family Playset research**, current decisions, build plan, Bluey interaction study and Toca/Piknik object catalog. It preserves the full requested game while separating what works now from what remains to build. Catalog variants and overlapping invitations are listed individually for coverage; their count is **not** a count of unique game engines or a completion percentage.

**Six content regions, five destinations:** Heeler Home includes the house and backyard; the other destinations are Park, Creek, Beach and Daycare. Bedrooms, secret rooms and imagination stories are subareas. Keep the other worlds scenic while development concentrates on Home. Every new shared feature must support **four players**, with independent joining, leaving, characters and locations.

**Last verified deployment:** Samsung Android **138**, the user-accepted four-room shell preview, with all 16 saved records retained; server/iPads **128**, iPhone **101**. Candidate 146 furniture/secret-room source is Windows-only. These records are not a fresh live server/device check. Android currently uses private solo until a matching coordinated rollout. No physical-device or sustained A10 qualification is implied by desktop evidence.

## What the audit found

- The working game is a **scenic prototype with a growing Home interaction set**. Walking scenery is present across the map. That does not establish working playground equipment, fishing, daycare teaching or hidden rooms.
- Current Home play includes the water/plant/sponge loop, seating, trampoline use, radio music/automatic dancing, four-slot shed storage, Keepy Uppy, working stairs/upstairs landing and four furnished rooms with scoped decoration/storage plus four optional secret rooms in candidate 146. The wider kitchen, books, TV library, dinosaurs, science, bedrooms, secret rooms and hide-and-seek remain unfinished.
- TV still has a separate single-clip technical prototype. BED-3 adds usable furniture, bounded personal storage and owner decoration/Together/undo. Full bedroom content, richer decor and physical qualification remain unfinished.
- Only **Bluey and Bingo** are playable. Official pictures for the rest of the cast are reference material, not finished rigs. The catalog expands the three Terriers into separate future avatars and retains Pretzel's missing-reference task.
- Older research text still used six destination bubbles, two rods or two seats, and sometimes implied offline synchronization. The maintained plan now applies five destinations, four-person activity design and private offline saves with server-wins reconnection. Pair examples remain useful scenarios; they are not the capacity limit.
- The 32 show-inspired cards are a backlog; Keepy Uppy is the first implemented one. Suggested homes for other show cards below are organizational choices, not a claim that those locations or games are already built.

## Status key

| Label | Meaning |
| --- | --- |
| Playable prototype | The specific narrow behavior exists with retained evidence. This does not certify all devices, full art polish or its whole parent feature. |
| Partial | Part of the listed requirement works; the description identifies the gap. |
| Scenery only | The region is walkable and illustrated; its listed activity catalog is still planned. |
| Development only | Separate technical experiment; not integrated into the family game. |
| Planned | Requirement/design is recorded; no integrated playable implementation established. |
| Optional idea | Candidate, extension or optional casting not committed as first-release content. |
| Retired | Explicitly removed; not a pending task or a completed feature. |

## World overview

| Destination / content region | Exists now | Main remaining content |
| --- | --- | --- |
| Heeler Home — house | Continuous scenic house; layered sofa, radio/dancing; stairs/landing plus four furnished rooms with scoped storage/decor in candidate 142 | Full kitchen/15 recipes, five cleanup games, parents/hiding, books/TV, 20 dinosaur types, eight science stations, four player-owned bedrooms and four secret rooms |
| Heeler Home — backyard | Connected long yard; water fixture, trampoline, radio, shed; balloon in newer builds | Pond fishing, sand/mud, swing, pool, hose/can/plant expansion, wagon, richer storage and show activities |
| Playground & Park | Long walkable scenery | All 12 equipment/game activities and shared supporting props |
| The Creek | Long scenery plus inherited garden-rule water fixture | All 10 distinct creek activities, including fishing, boats, crossings and nature play |
| The Beach | Long walkable scenery | All 10 beach activities, including collecting, sand, water and throwing games |
| Daycare | Long walkable scenery | Teacher/full cast, optional day, 12 learning stations and nine imagination stories |

## Complete inventory

Each entry links back to its research section and the relevant implementation record. Source catalogs preserve the proposed Simple/Explore variants; phrases such as “both children” describe one scenario and always inherit the four-player requirement above. Repeated related activities are retained because they appear separately in the research.
'''

parts=[INTRO]
cards=[]
for world,label in WORLD.items():
    subset=[f for f in features if f['world']==world]
    parts.append(f'## {label}\n')
    cards.append(f'<section class="world" data-world="{world}"><h2>{html.escape(label)}</h2>')
    for group in dict.fromkeys(f['group'] for f in subset):
        parts.append(f'### {group}\n\n| Feature | Status | Planned behavior and audit finding |\n| --- | --- | --- |')
        cards.append(f'<div class="group"><h3>{html.escape(group)}</h3><div class="cards">')
        for f in [x for x in subset if x['group']==group]:
            title=html.escape(f['title']); status=STATUS[f['status']]
            parts.append(f'| **{f["id"]} — {f["title"]}** | {status} | {f["description"]}<br>{refs(f)} |')
            desc=markdown.markdown(f['description']); links=markdown.markdown(refs(f))
            cards.append(f'<article id="{f["id"].lower()}" data-status="{f["status"]}"><div class="meta"><span>{f["id"]}</span><span class="badge {f["status"]}">{status}</span></div><h4>{title}</h4>{desc}<div class="refs">{links}</div></article>')
        parts.append('');cards.append('</div></div>')
    cards.append('</section>')

appendix_start=len(parts)
parts.append('## All 35 master requirements accounted for\n\nThese broad requirements are not marked complete merely because a smaller prototype works. `IMG-01` is both a master family ID and the first story ID in the source; story entries here use `STORY-IMG-xx` to keep them distinct.\n\n| Goal ID | Required feature | Audit status | Inventory route |\n| --- | --- | --- | --- |')
for row in tracker:parts.append(f'| {row[0]} | {row[1]} | {STATUS[TRACK_STATUS[row[0]]]} | {TRACK_MAP[row[0]]} |')

parts.append('\n## Coverage of all 55 research chapters\n\nThe checklist includes game requirements, not an endorsement or fresh audit of every library candidate or scientific citation. Implementation/acceptance chapters are routed to the shared requirements and parent/reliability section.\n\n| Research chapter | Where it is represented |\n| --- | --- |')
coverage={}
for n,(title,_) in chapters.items():
    matched=[f['id'] for f in features if n in f['chapters']]
    if n==16:route='All 35 master requirements table; each mapped to detailed inventory entries.'
    elif n in (14,15,36,43,46,52,53,54):route='Parent setup/reliability, optional extensions, retired scope and the build-plan acceptance gates.'
    else:route=', '.join(matched[:14])+(' and named catalog entries above.' if len(matched)>14 else '.')
    assert matched or n==16, f'Unmapped chapter {n}'
    coverage[str(n)]=matched or ['master-requirements']
    parts.append(f'| {source(n)} — {title} | {route} |')

OUTRO='''
## Audit corrections and remaining decisions

| Finding | Resolution |
| --- | --- |
| Six-region content was confused with six menu entries | Five destination buttons; Home includes backyard. No region removed. |
| Older two-player examples, room counts and furniture capacities | Four profile-owned bedrooms and four optional secret rooms; all future shared activities support four. Sofa/trampoline four-place implementation is only in the newer builds. Future rides/rods need enough participation space; no forced long queues. |
| Character pictures or scenery mistaken for features | Mark playable rigs separately; painted objects remain scenery until interactions/state/content exist. |
| Show catalog labeled entirely research-only | Keepy Uppy is implemented and parent hide-and-seek has a partial downstairs loop; the other 30 catalog cards remain planned. |
| Old device versions in dated feature rows | Use the deployment snapshot at the top, with dated evidence kept historical. Do not claim that build 132 is installed. |
| Offline “sync” wording | Offline progress stays private. Connected edits update the server world; reconnect does not import offline changes. |
| TV fixtures or empty bedrooms mistaken for finished features | TV remains Development only. Bedrooms are Partial after BED-3 furniture/storage/decor; broader content and visual/device acceptance remain open. |
| Flexible estimates mistaken for final limits | Day length, assist levels, search timing, clutter/loan limits and memory budgets are tuning proposals, not measured guarantees. |
| Extra ideas mixed with committed content | Optional older cast, Pirates, hair styling, extra prehistoric animals, AR and later TV modes are labeled separately. |

Still open for later content decisions: Spanish dialect; final extra cast priority; optional show-card selection/order; final narration/art approval; exact room layouts and four-person arrangements for new rides. These do not require a new decision to continue the already selected Home work.

## Recommended next sequence from the existing plan

1. Follow the latest request and [upstairs research](implementation/upstairs-bedrooms-research-2026-09-26.html): working stairs and landing first, then four owned upstairs bedrooms with usable furniture, storage, permissions and tested persistence. BED-1 stairs are accepted on Android 136; BED-2 four owned room destinations is implemented in Windows 137. BED-3 furniture, storage, permissions and undo are implemented in candidate 142. The user then authorized researched secret rooms; candidate 146 adds that foundation. Physical qualification, broader catalogs and richer decor remain required.
2. Build four optional secret rooms after the bedroom stage. Their small far-back entrances stay hidden until the local character approaches, then reveal with a star glow, sparkles and shimmer; retain persistent contents and always-visible interior exits.
3. Continue the complete Home backlog, including kitchen supports/interiors and cooking/serving/cleanup before all 15 recipes, using the [Home tracker](home-world-feature-tracker.md). Other worlds remain scenic.
4. Qualify the actual candidate and coordinated compatible server/client rollout when devices are available. Recovery and physical gates remain open; a future content/schema change needs its own qualification.

## Audit method and evidence boundary

Read the maintained decisions and build record, extracted every named catalog entry, reviewed the research's room/activity/state requirements, and compared current status with focused runtime source and retained acceptance reports. Source inspected includes `WorldLayout`, `SoloWorld`, `HomeWorld`, `KeepyUppy`, `SoloNavigation`, `SoloScreen` and the separate `FoundationVideo` probe. This is a feature/source/evidence audit, not a line-by-line review of all game code or a fresh external research pass. No devices, live server, personal media, credentials or saves were accessed or changed for this audit.

The generated coverage record checks all 55 chapters, 35 master IDs, 32 show cards, 32 outdoor cards, 12 learning stations, 15 recipes, nine science stations, eight coloring requirements, six books, 20 dinosaurs, five dinosaur invitations, five cleanup entries, 18 starter invitations, nine stories, 37 individual roster entries and 20 reusable object families. SCI-09 mixing/reactions was added at the user's request on September 27 and has four implemented candidate-178 variants; physical acceptance remains open. Overlapping entries are intentional; no completion percentage is derived.

[Research and goal sheet](bluey-game-research-2026-09-23.html) · [Current decisions](current-decisions.md) · [Build plan](family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis) · [Bluey interaction study](bluey-lets-play-reference-study-2026-09-25.html) · [Toca/Piknik supplement](toca-piknik-interaction-research-2026-09-23.html) · [Machine-readable inventory](implementation/evidence/world-feature-audit-2026-09-26/catalog.json) · [Coverage validation](implementation/evidence/world-feature-audit-2026-09-26/coverage.json).
'''
parts.append(OUTRO)
md='\n'.join(parts).rstrip()+'\n'
(DOC/(NAME+'.md')).write_text(md,encoding='utf-8')

expected={'SHOW':32,'BCH':10,'CRK':10,'PRK':12,'LRN':12,'PIZ':5,'CAK':5,'MEAL':5,'SCI':9,'COL':8,'BK':6,'TOY':20,'DISC':5,'CLEAN-GAME':5,'QUEST':18,'STORY-IMG':9,'CAST':37,'OBJECT':20}
counts={p:sum(bool(re.fullmatch(re.escape(p)+r'-\d+',f['id'])) for f in features) for p in expected}
counts['CAST']=sum(f['id'].startswith('CAST-') for f in features)
assert counts==expected,(counts,expected)
assert set(TRACK_STATUS)=={r[0] for r in tracker} and len(tracker)==35
assert set(chapters)==set(range(1,56)) and len(set(f['id'] for f in features))==len(features)
assert all(f['world'] in WORLD and f['status'] in STATUS for f in features)
evidence=DOC/'implementation/evidence/world-feature-audit-2026-09-26';evidence.mkdir(parents=True,exist_ok=True)
(evidence/'catalog.json').write_text(json.dumps(dict(date='2026-09-26',sourceCommit='8a315ca945a899dbefe7687b47ac02a3bf7cd6fd',scopedUpdates=[dict(build=155,baselineCommit='1c060cd',entries=['H-31','H-32','H-34','H-36','H-37'],evidence='docs/implementation/room-object-play-2026-09-26.md'),dict(build=135,baselineCommit='b2d41922a4d722e4c941cf1cc1d65b2e61a9f939',entries=['G-02','G-06','H-01'],evidence='docs/implementation/upstairs-foundation-2026-09-26.md'),dict(build=137,baselineCommit='ae0043512f1782d4aec58316ca76b8964eee6b3a',entries=['G-02','G-06','H-01','H-31','H-32'],evidence='docs/implementation/bedroom-rooms-2026-09-26.md'),dict(build=142,baselineCommit='d4ed2bb',entries=['G-02', 'G-06', 'H-01', 'H-04', 'H-05', 'H-31', 'H-32', 'H-33', 'H-34'],evidence='docs/implementation/bedroom-furniture-2026-09-26.md'),dict(build=146,baselineCommit='bfed20a',entries=['G-02','G-17','H-35','H-36','H-37','H-38'],evidence='docs/implementation/secret-rooms-2026-09-26.md')],worlds=WORLD,statuses=STATUS,features=features),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
sources=['docs/bluey-game-research-2026-09-23.md','docs/current-decisions.md','docs/family-playset-build-guide-2026-09-23.md','docs/bluey-lets-play-reference-study-2026-09-25.md','docs/toca-piknik-interaction-research-2026-09-23.md']
code=['Core/WorldLayout.cs','Core/SoloWorld.cs','Core/HomeWorld.cs','Core/KeepyUppy.cs','Client/SoloNavigation.cs','Client/SoloScreen.cs','Runtime/FoundationVideo.cs']
sources+=['Unity/FamilyPlayset/Assets/FamilyPlayset/Code/'+p for p in code]
(evidence/'coverage.json').write_text(json.dumps(dict(passed=True,chapters=coverage,masterRequirements=TRACK_STATUS,catalogCounts=counts,inventoryEntries=len(features),statusCounts=dict(Counter(STATUS[f['status']] for f in features)),sources=[dict(path=p,sha256=hashlib.sha256((ROOT/p).read_bytes()).hexdigest()) for p in sources],newGameplayTests=False,physicalDevicesAccessed=False,liveServerAccessed=False,externalSourcesFreshlyRevalidated=False),indent=2)+'\n',encoding='utf-8')

intro_html=markdown.markdown(INTRO,extensions=['tables'])
appendix=markdown.markdown('\n'.join(parts[appendix_start:]),extensions=['tables'])
css='''
:root{font-family:Segoe UI,system-ui,sans-serif;color:#17384b;background:#eef5f6;font-size:16px}*{box-sizing:border-box}body{margin:0}main{max-width:1220px;margin:auto;padding:32px 30px 70px}h1{font-size:clamp(2rem,4vw,3.25rem);line-height:1.12;max-width:900px}h2{margin-top:48px;font-size:1.7rem}h3{margin-top:30px;color:#30627a}h4{font-size:1.2rem;line-height:1.3;margin:16px 0 12px}p,li{line-height:1.65}a{color:#17617e;text-underline-offset:3px}header{border-top:8px solid #4babc4;padding-top:18px}table{border-collapse:collapse;width:100%;font-size:.93rem;margin:20px 0}th,td{border:1px solid #d3e1e4;text-align:left;vertical-align:top;padding:12px}th{background:#deedf1}tr:nth-child(even){background:#f7fafb}.toolbar{position:sticky;top:0;z-index:3;display:flex;gap:12px;flex-wrap:wrap;padding:18px;background:#17384bf5;color:white;border-radius:14px;box-shadow:0 3px 14px #17384b22}.toolbar label{display:flex;flex-direction:column;gap:5px;font-size:.78rem;flex:1;min-width:165px}.toolbar label:first-child{flex:2}input,select,button{font:inherit;border:1px solid #b6cbd3;border-radius:7px;padding:11px;max-width:100%;background:white;color:#17384b}button{cursor:pointer;align-self:end}#count{width:100%;font-size:.83rem;margin:0}.cards{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:16px}article{background:white;border:1px solid #d3e1e4;border-radius:14px;padding:22px;overflow-wrap:anywhere}article p{margin:10px 0}.meta{display:flex;gap:10px;justify-content:space-between;align-items:center;font-size:.75rem;font-weight:700;letter-spacing:.025em}.badge{border-radius:20px;padding:6px 10px;background:#edf1f4;white-space:nowrap}.play{background:#dcf0df;color:#245135}.part{background:#fff0cc;color:#725319}.scene{background:#deedf9;color:#264e70}.dev{background:#eee5fa;color:#5e4278}.optional{background:#fbe9df;color:#805037}.retired{background:#eee;color:#555}.refs{border-top:1px solid #e5ecee;margin-top:16px;padding-top:8px;font-size:.79rem;color:#456477}.refs p{line-height:1.6}details{margin-top:45px;background:#fff;padding:22px;border-radius:14px}summary{cursor:pointer;font-size:1.2rem;font-weight:700}.scroll{overflow:auto}[hidden]{display:none!important}.eyebrow{letter-spacing:.16em;font-weight:700;text-transform:uppercase;font-size:.78rem;color:#35738b}#empty{padding:25px;background:white;border-radius:12px}footer{font-size:.85rem;margin-top:35px}@media(max-width:720px){main{padding:20px 16px}.cards{grid-template-columns:1fr}.toolbar{position:static}article{padding:18px}.meta{flex-wrap:wrap}table{min-width:620px}}@media print{body{background:white}main{padding:0;max-width:none}.toolbar,.screen-only{display:none}details{display:block}article{break-inside:avoid;border-radius:0}.cards{grid-template-columns:1fr 1fr;gap:10px}h2,h3{break-after:avoid}a{color:inherit}table{font-size:9pt}article{font-size:10pt}.refs{font-size:8pt}}
'''
options=''.join(f'<option value="{k}">{html.escape(v)}</option>' for k,v in WORLD.items())
statuses=''.join(f'<option value="{k}">{html.escape(v)}</option>' for k,v in STATUS.items())
js='''
const q=document.getElementById('q'),world=document.getElementById('world'),status=document.getElementById('status');
const entries=[...document.querySelectorAll('article')];
function filter(){const words=q.value.toLowerCase().trim().split(/\\s+/).filter(Boolean);let n=0;for(const a of entries){const ok=(!world.value||a.closest('.world').dataset.world===world.value)&&(!status.value||a.dataset.status===status.value)&&words.every(w=>a.textContent.toLowerCase().includes(w));a.hidden=!ok;if(ok)n++;}for(const g of document.querySelectorAll('.group'))g.hidden=![...g.querySelectorAll('article')].some(a=>!a.hidden);for(const s of document.querySelectorAll('.world'))s.hidden=![...s.querySelectorAll('article')].some(a=>!a.hidden);document.getElementById('count').textContent=n+' of '+entries.length+' checklist entries shown · overlapping source ideas are retained';document.getElementById('empty').hidden=n!==0;}
q.addEventListener('input',filter);world.addEventListener('change',filter);status.addEventListener('change',filter);document.getElementById('reset').addEventListener('click',()=>{q.value='';world.value='';status.value='';filter()});document.getElementById('print').addEventListener('click',()=>{document.getElementById('appendix').open=true;window.print()});filter();
'''
page=f'''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Little Weeps — all-world feature audit</title><style>{css}</style></head><body><main><header><div class="eyebrow">Family Playset · Feature inventory</div>{intro_html}</header><div class="toolbar"><label>Search features<input id="q" type="search" placeholder="Try trampoline, TV, dinosaur, PIZ-01…"></label><label>World / area<select id="world"><option value="">All worlds and systems</option>{options}</select></label><label>Status<select id="status"><option value="">All statuses</option>{statuses}</select></label><button id="reset">Clear filters</button><button id="print">Print / save PDF</button><p id="count" aria-live="polite"></p></div><p id="empty" hidden>No matching entries. Clear a filter or try a shorter search.</p>{''.join(cards)}<details id="appendix"><summary>Master requirements, chapter coverage and audit notes</summary>{appendix}</details><footer><a href="{NAME}.md">Markdown inventory</a> · <a href="family-playset-build-guide-2026-09-23.html">Build plan</a> · No game or device changes in this audit.</footer></main><script>{js}</script></body></html>'''
page=page.replace('<div class="toolbar">','<div class="toolbar" id="feature-filters">')
page=page.replace('<div class="eyebrow">Family Playset · Feature inventory</div>', '<div class="eyebrow">Family Playset · Feature inventory</div><p class="screen-only"><a href="#feature-filters">Browse all '+str(len(features))+' checklist entries ↓</a></p>')
page=page.replace('<table>','<div class="scroll"><table>').replace('</table>','</table></div>')
(DOC/(NAME+'.html')).write_text(page,encoding='utf-8')
print(json.dumps(dict(inventoryEntries=len(features),catalogs=counts,masterRequirements=len(tracker),chapters=len(chapters),file=str(DOC/(NAME+'.html'))),indent=2))
