"""Render the maintained Bluey reference study with its local screenshot gallery."""
from html import escape
from pathlib import Path
import re
import markdown

ROOT = Path(__file__).resolve().parent.parent
BASE = 'bluey-lets-play-reference-study-2026-09-25'
source = ROOT / (BASE + '.md')
md = markdown.Markdown(extensions=['tables', 'fenced_code', 'toc'],
    extension_configs={'toc': {'toc_depth': '2-2'}})
body = md.convert(source.read_text(encoding='utf-8'))
body = body.replace('<table>', '<div class="table-scroll"><table>').replace('</table>', '</table></div>')
style = re.search(r'<style>(.*?)</style>',
    (ROOT / 'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'), re.S).group(1)
page = '''<!doctype html><html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Bluey: Let's Play! — Little Weeps reference study</title><style>''' + style + '''
.reference-study{max-width:1080px;margin:40px auto;padding:0 24px}
.reference-study h1{font-size:clamp(32px,4.5vw,54px);line-height:1.15}
.reference-study img{display:block;width:100%;height:auto;border-radius:14px;border:1px solid #bcd7db;margin:20px 0 14px}
.reference-study h2{scroll-margin-top:90px;margin-top:48px}
.reference-study h3{margin-top:28px}
.reference-study code{overflow-wrap:anywhere}
.reference-study .toc{padding:18px 24px;border:1px solid var(--line);border-radius:16px;background:white}
.reference-study .toc ul{columns:2;column-gap:30px;margin:0;padding-left:20px}
.reference-study .toc li{break-inside:avoid;margin-bottom:8px}
@media(max-width:650px){.reference-study .toc ul{columns:1}}
@media print{.reference-study{max-width:none;margin:0;padding:0}.reference-study img{max-height:280px;object-fit:contain}.reference-study h2,.reference-study h3{break-after:avoid}.reference-study .toc{display:none}}
</style></head><body><header class="topbar"><a class="brand" href="bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a>
<nav><a href="current-decisions.md">Current decisions</a><a href="family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Build status</a>
<a href="''' + escape(BASE) + '''.md">Editable study</a></nav></header><main class="reference-study report">''' + body + '''</main></body></html>'''
(ROOT / (BASE + '.html')).write_bytes(page.encode('utf-8'))
print(f'Rendered {BASE}.html ({len(md.toc_tokens)} sections)')
