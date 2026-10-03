# /// script
# dependencies = ["markdown"]
# ///
"""Render the scenic milestone with its actual native-player captures."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import markdown
import re

root=PROJECT_ROOT/'docs'
base=root/'implementation/scenic-worlds-2026-09-25'
style=re.search(r'<style>(.*?)</style>',(root/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S).group(1)
body=markdown.markdown(base.with_suffix('.md').read_text(encoding='utf-8'),extensions=['tables','fenced_code','toc'])
page='''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Little Weeps — Six scenic worlds</title><style>'''+style+'''
.scenery-report{max-width:1100px;margin:42px auto;padding:0 24px}.scenery-report h1{font-size:clamp(32px,5vw,54px)}
.scenery-report h2{margin-top:48px}.scenery-report img{display:block;width:100%;height:auto;margin:18px 0 28px;border-radius:16px;border:1px solid #b7d6dc}
.scenery-report table{display:block;overflow:auto}.scenery-report td,.scenery-report th{padding:12px;text-align:left;border-bottom:1px solid #c9dadf}
.scenery-report h3{scroll-margin-top:90px}.world-jumps{display:flex;gap:12px;flex-wrap:wrap;margin:22px 0}.world-jumps a{background:#d4edf7;padding:8px 16px;border-radius:30px;text-decoration:none}
</style></head><body><header class="topbar"><a class="brand" href="../bluey-game-research-2026-09-23.html">LITTLE WEEPS</a><nav><a href="../family-playset-build-guide-2026-09-23.html">Build plan</a><a href="scenic-worlds-2026-09-25.md">Editable report</a></nav></header><main class="scenery-report"><nav class="world-jumps" aria-label="World previews">'''
for slug,label in [('home','Home'),('backyard-garden','Backyard'),('playground-park','Park'),('creek','Creek'),('beach','Beach'),('daycare','Daycare')]:page+=f'<a href="#{slug}">{label}</a>'
page+='</nav>'+body+'</main></body></html>'
base.with_suffix('.html').write_text(page,encoding='utf-8')
print('Rendered',base.with_suffix('.html'))
