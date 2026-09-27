# /// script
# dependencies = ["markdown"]
# ///
"""Render the Home tracker and current bedroom research/implementation pages."""
from pathlib import Path
import html
import re
import markdown

docs=Path(__file__).resolve().parents[1]/'docs'
style=re.search(r'<style>(.*?)</style>',(docs/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S)[1]
for name in ('home-world-feature-tracker','implementation/upstairs-bedrooms-research-2026-09-26',
             'implementation/bedroom-rooms-2026-09-26','implementation/bedroom-furniture-2026-09-26',
             'implementation/secret-rooms-research-2026-09-26','implementation/secret-rooms-2026-09-26'):
    path=docs/(name+'.md');source=path.read_text(encoding='utf-8')
    title=html.escape(source.splitlines()[0].lstrip('# '))
    body=markdown.markdown(source,extensions=['tables','fenced_code','toc'])
    body=body.replace('<table>','<div class="table-scroll" tabindex="0" role="region" aria-label="Feature table"><table>').replace('</table>','</table></div>')
    prefix='../' if '/' in name else ''
    page='<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>'+title+'</title><style>'+style+'main{max-width:1100px;margin:32px auto;padding:24px}p,li{line-height:1.65}img{max-width:100%;height:auto}table{width:100%;border-collapse:collapse}th,td{padding:12px;border:1px solid #c9dadf;text-align:left;vertical-align:top}.table-scroll{overflow-x:auto}code{overflow-wrap:anywhere}</style></head><body><main><p><a href="'+prefix+'family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Current build plan</a> · <a href="'+path.name+'">Editable source</a></p>'+body+'</main></body></html>'
    path.with_suffix('.html').write_text(page,encoding='utf-8');print(path.with_suffix('.html'))
