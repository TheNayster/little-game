"""Render the source-backed shared-movement review in the existing docs style."""
from pathlib import Path
import re
import markdown

root=Path(__file__).resolve().parent.parent
base='g3-smooth-movement-2026-09-24'
source=root/'implementation'/f'{base}.md'
html=markdown.markdown(source.read_text(encoding='utf-8'),extensions=['tables','fenced_code'])
style=re.search(r'<style>(.*?)</style>',(root/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S).group(1)
html=html.replace('<table>','<div class="table-scroll"><table>').replace('</table>','</table></div>')
page='''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>The Family Playset — smooth four-player movement</title><style>'''+style+'''
.movement-review{max-width:980px;margin:42px auto;padding:0 24px}.movement-review h1{font-size:clamp(30px,4vw,46px)}.movement-review p,.movement-review li{line-height:1.65}.movement-review code{overflow-wrap:anywhere}.movement-review pre{overflow-x:auto}
</style></head><body><header class="topbar"><a class="brand" href="../bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a><nav><a href="../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Build status</a><a href="'''+base+'''.md">Editable research</a></nav></header><main class="movement-review report">'''+html+'''</main></body></html>'''
destination=source.with_suffix('.html');destination.write_text(page,encoding='utf-8')
print('Rendered '+str(destination))
