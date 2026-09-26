# /// script
# dependencies = ["markdown"]
# ///
"""Render the maintained home scene composition research and art contract."""
from pathlib import Path
import re
import markdown

docs = Path(__file__).resolve().parents[1] / 'docs'
name = 'home-scene-layer-research-2026-09-26'
source = docs / 'implementation' / (name + '.md')
style = re.search(r'<style>(.*?)</style>', (docs / 'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'), re.S).group(1)
body = markdown.markdown(source.read_text(encoding='utf-8'), extensions=['tables', 'fenced_code', 'toc'], extension_configs={'toc': {'toc_depth': '2-2'}})
body = body.replace('<table>', '<div class="table-scroll" tabindex="0" role="region" aria-label="Scene research table"><table>').replace('</table>', '</table></div>')
page = '''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Little Weeps — one coherent playable home</title><style>''' + style + '''
main{max-width:1080px;margin:36px auto;padding:24px}h1{font-size:clamp(32px,5vw,54px);line-height:1.15}h2{margin-top:42px}p,li{line-height:1.65}table{min-width:620px;width:100%;border-collapse:collapse}td,th{padding:12px;border-bottom:1px solid #c9dadf;text-align:left;vertical-align:top}.table-scroll{overflow-x:auto}img{display:block;width:100%;height:auto;border-radius:12px}code{overflow-wrap:anywhere}.toc{padding:16px 24px;background:#edf5f5;border-radius:12px}.toc ul{columns:2;padding-left:20px}.toc li{break-inside:avoid;margin-bottom:6px}@media(max-width:640px){main{padding:16px;margin:12px auto}.toc ul{columns:1}}@media print{main{margin:0;padding:0}.toc{display:none}h2{break-after:avoid}img{max-height:320px;object-fit:contain}}
</style></head><body><main><p><a href="../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Build plan</a> · <a href="''' + name + '''.md">Editable research</a></p>''' + body + '</main></body></html>'
source.with_suffix('.html').write_text(page, encoding='utf-8')
print(source.with_suffix('.html'))
