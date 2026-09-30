"""Validate and draw the zoo design proposal; does not generate game content."""
from pathlib import Path
import json
from html import escape
from collections import defaultdict, deque

root = Path(__file__).resolve().parents[1]
folder = root / 'SourceArt/Zoo/Layout'
data = json.loads((folder / 'zoo-layout.json').read_text(encoding='utf-8'))
trails = data['trails']
assert len(trails) == 4 and data['panelWidth'] == 2400 and data['panelHeight'] == 800
species = [e['id'] for t in trails for e in t['exhibits']]
assert len(species) == 16 and len(set(species)) == 16
assert data['foodBucketsPerExhibit'] == 1 and data['sharedOfferSlotsPerExhibit'] == 4
assert all(data['animalMovementPolicy'].get(key) for key in (
    'routineChoice', 'destinationChoice', 'routeChoice', 'repeatControl',
    'individualVariation', 'habitatLimits', 'feedingOverride', 'sharedAuthority', 'persistence'))
zones = {data['hub']['zone'], *(t['zone'] for t in trails)}
assert len(zones) == 5
graph = defaultdict(set)
for p in data['portals']:
    assert p['a'] in zones and p['b'] in zones and p['bidirectional']
    graph[p['a']].add(p['b'])
    graph[p['b']].add(p['a'])
seen = set()
queue = deque([data['hub']['zone']])
while queue:
    zone = queue.popleft()
    if zone in seen:
        continue
    seen.add(zone)
    queue.extend(graph[zone] - seen)
assert seen == zones
for t in trails:
    assert len(t['exhibits']) == 4 and t['minX'] == 0 and t['maxX'] == 9600
    assert [e['xStart'] for e in t['exhibits']] == [0, 2400, 4800, 7200]
    assert all(e['food'] and e['feeder'] and len(e['anchors']) == 4 for e in t['exhibits'])

# This is a travel schematic, not an overhead rendering of the game camera.
positions = {'zoo-savanna': (40, 120), 'zoo-dinosaurs': (560, 120),
             'zoo-aquarium': (40, 480), 'zoo-reptiles': (560, 480)}
svg = ['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1080 825" role="img" aria-labelledby="title desc">',
       '<title id="title">Proposed zoo entrance and sixteen exhibit layout</title>',
       '<desc id="desc">An entrance plaza connects to four scrolling trails. Savanna connects to Dinosaurs, then Reptiles, then Aquarium, then back to Savanna. Every connection is bidirectional. Each trail contains four species, each with one bucket and four offer places.</desc>',
       '<style>text{font-family:system-ui,sans-serif;fill:#243951} .trail{fill:#edf5f2;stroke:#a0b8b0} .route{fill:none;stroke:#708c83;stroke-width:2} .title{font-size:22px;font-weight:600} .label{font-size:18px} .note{font-size:15px}</style>',
       '<rect width="1080" height="825" fill="#fff"/>',
       '<text x="40" y="42" class="title">Zoo world: proposed travel layout</text>',
       '<text x="40" y="74" class="note">Side-on scrolling play • connection map, not an overhead game camera</text>',
       '<path class="route" d="M520 265 H560 M800 410 V480 M520 625 H560 M280 410 V480"/>',
       '<path class="route" d="M280 410 L540 445 L800 410 M280 480 L540 445 L800 480"/>',
       '<rect x="398" y="416" width="284" height="58" rx="12" fill="#dcebd8" stroke="#708c83"/>',
       '<text x="540" y="441" text-anchor="middle" class="label">Entrance plaza / Zoo map</text>',
       '<text x="540" y="463" text-anchor="middle" class="note">Four trail gates • Home exit</text>']
for t in trails:
    x, y = positions[t['zone']]
    svg.append(f'<rect class="trail" x="{x}" y="{y}" width="480" height="290" rx="12"/>')
    svg.append(f'<text x="{x+20}" y="{y+36}" class="title">{escape(t["name"])}</text>')
    for i, e in enumerate(t['exhibits']):
        svg.append(f'<text x="{x+20}" y="{y+81+i*42}" class="label">{i+1}. {escape(e["name"])}</text>')
    svg.append(f'<text x="{x+20}" y="{y+250}" class="note">Each exhibit: 1 food bucket + 4 shared offer places</text>')
    svg.append(f'<text x="{x+20}" y="{y+274}" class="note">Four 2400-wide panels • all paths bidirectional</text>')
svg.extend(['<text x="40" y="807" class="note">Design proposal • species routines sourced individually • geometry and pacing await playtesting</text>', '</svg>'])
(folder / 'zoo-layout.svg').write_text('\n'.join(svg) + '\n', encoding='utf-8')
print('Validated 16 unique exhibits, 16 buckets, 64 shared offer places, 17 proposed panels and a connected travel graph.')
