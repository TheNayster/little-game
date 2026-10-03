# /// script
# dependencies = ["markdown"]
# ///
"""Render the Home tracker and current bedroom research/implementation pages."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import html
import re
import sys
import markdown

docs=PROJECT_ROOT/'docs'
style=re.search(r'<style>(.*?)</style>',(docs/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S)[1]
for name in ('implementation/hide-and-seek-hide-to-join-2026-09-30','implementation/hide-and-seek-together-2026-09-28','implementation/hide-and-seek-first-level-2026-09-28','implementation/hide-and-seek-2026-09-28','implementation/hide-and-seek-research-2026-09-28','implementation/world-music-2026-09-28','home-world-feature-tracker','implementation/upstairs-bedrooms-research-2026-09-26',
             'implementation/bedroom-rooms-2026-09-26','implementation/bedroom-furniture-2026-09-26',
             'implementation/secret-rooms-research-2026-09-26','implementation/secret-rooms-2026-09-26',
             'implementation/home-reading-quiet-play-research-2026-09-26','implementation/kids-narrator-research-2026-09-26','implementation/home-books-2026-09-26','implementation/room-object-play-2026-09-26','implementation/kitchen-research-2026-09-27','implementation/home-kitchen-2026-09-27','implementation/kitchen-child-friendly-research-2026-09-27','implementation/kitchen-easy-2026-09-27','implementation/kitchen-staged-cooking-research-2026-09-27','implementation/chocolate-cake-flow-2026-09-27','implementation/shared-play-regressions-2026-09-27','implementation/home-science-coloring-research-2026-09-27','implementation/home-discovery-2026-09-27',
             'implementation/home-science-play-design-2026-09-27','implementation/home-mixing-2026-09-27','implementation/workshop-visuals-2026-09-27','implementation/science-playground-research-2026-09-27','implementation/science-kids-flow-2026-09-28','implementation/science-hammer-labs-2026-09-28','implementation/home-picture-controls-2026-09-28','implementation/ice-rescue-2026-09-28','implementation/bubble-lab-2026-09-28','implementation/liquid-colors-2026-09-28','implementation/native-reader-controls-2026-09-28','implementation/home-idle-cleanup-2026-09-28','implementation/home-finishing-research-2026-09-28','implementation/native-coloring-controls-2026-09-28','implementation/home-creation-storage-2026-09-28','implementation/cake-families-2026-09-28','implementation/pizza-preparation-2026-09-28','implementation/meal-preparation-research-2026-09-28','implementation/meal-preparation-2026-09-28','implementation/marble-ramps-research-2026-09-28','implementation/marble-ramps-2026-09-28'):
    if sys.argv[1:] and name not in sys.argv[1:]:
        continue
    path=docs/(name+'.md');source=path.read_text(encoding='utf-8')
    title=html.escape(source.splitlines()[0].lstrip('# '))
    body=markdown.markdown(source,extensions=['tables','fenced_code','toc'])
    body=body.replace('<table>','<div class="table-scroll" tabindex="0" role="region" aria-label="Feature table"><table>').replace('</table>','</table></div>')
    prefix='../' if '/' in name else ''
    page='<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>'+title+'</title><style>'+style+'main{max-width:1100px;margin:32px auto;padding:24px}p,li{line-height:1.65}img{max-width:100%;height:auto}table{width:100%;border-collapse:collapse}th,td{padding:12px;border:1px solid #c9dadf;text-align:left;vertical-align:top}.table-scroll{overflow-x:auto}code{overflow-wrap:anywhere}</style></head><body><main><p><a href="'+prefix+'family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Current build plan</a> · <a href="'+path.name+'">Editable source</a></p>'+body+'</main></body></html>'
    path.with_suffix('.html').write_text(page,encoding='utf-8');print(path.with_suffix('.html'))
