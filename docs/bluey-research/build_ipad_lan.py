"""Render the G3 Apple preparation report and concise return checklist."""
from pathlib import Path
from html import escape
import re
import markdown

root=Path(__file__).resolve().parent.parent
style=re.search(r'<style>(.*?)</style>',(root/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S).group(1)
for source in sorted((root/'implementation').glob('*.md')):
    base=source.stem
    title=source.read_text(encoding='utf-8').splitlines()[0].lstrip('# ')
    source=root/'implementation'/f'{base}.md'
    body=markdown.markdown(source.read_text(encoding='utf-8'),extensions=['tables','fenced_code','toc'])
    body=body.replace('<table>','<div class="table-scroll"><table>').replace('</table>','</table></div>')
    page='''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>The Family Playset — '''+escape(title)+'''</title><style>'''+style+'''
.progress-report{max-width:980px;margin:42px auto;padding:0 24px}.progress-report h1{font-size:clamp(30px,4vw,46px)}.progress-report p,.progress-report li{line-height:1.65}.progress-report code{overflow-wrap:anywhere}.progress-report pre{overflow-x:auto}
</style></head><body><header class="topbar"><a class="brand" href="../bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a><nav><a href="../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Build status</a><a href="return-checklist-ipad-lan-2026-09-24.html">Return checklist</a><a href="'''+base+'''.md">Editable record</a></nav></header><main class="progress-report report">'''+body+'''</main></body></html>'''
    source.with_suffix('.html').write_text(page,encoding='utf-8');print('Rendered '+base+'.html')
