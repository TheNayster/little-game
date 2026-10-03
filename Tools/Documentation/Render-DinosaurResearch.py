# /// script
# dependencies = ["markdown"]
# ///
"""Render the zoo research using the project's established Markdown page format."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import markdown

root = PROJECT_ROOT
source = root / 'docs/implementation/dinosaur-world-research-2026-09-30.md'
body = markdown.markdown(source.read_text(encoding='utf-8'),
    extensions=['tables', 'fenced_code', 'toc'],
    extension_configs={'toc': {'toc_depth': '2-2'}})
body = body.replace('<table>', '<div class="table-scroll" tabindex="0" role="region" aria-label="Dinosaur research table"><table>').replace('</table>', '</table></div>')
css = '''
:root{color-scheme:light}*{box-sizing:border-box}body{margin:0;background:#f7f6ed;color:#243951;font:17px/1.65 system-ui,sans-serif}
main{max-width:1150px;margin:28px auto;padding:30px;background:white;border-radius:18px}h1{font-size:clamp(32px,5vw,54px);line-height:1.15;color:#204967}
h2{margin-top:44px;padding-top:16px;border-top:3px solid #d5e7d1}a{color:#145d86;text-underline-offset:3px}img{display:block;width:100%;height:auto;border:1px solid #dce4e6;border-radius:10px}
table{border-collapse:collapse;width:100%;min-width:700px;font-size:15px}th,td{padding:12px;text-align:left;vertical-align:top;border-bottom:1px solid #dce4e6}th{background:#edf5f2}.table-scroll{overflow-x:auto}
.toc{background:#edf5f2;padding:16px 24px;border-radius:12px}.toc ul{columns:2}p,li{overflow-wrap:anywhere}li{margin-bottom:8px}nav{font-size:14px}
@media(max-width:700px){main{padding:18px;margin:0;border-radius:0}.toc ul{columns:1}}@media print{body{background:white;font-size:11pt}main{margin:0;padding:0}.toc{display:none}h2{break-after:avoid}img{max-height:350px;object-fit:contain}.table-scroll{overflow:visible}table{min-width:0;font-size:9pt}}
'''
page = '<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Dinosaur World research and riding model sheets</title><style>' + css + '</style></head><body><main><nav><a href="dinosaur-world-research-2026-09-30.md">Editable research</a> · <a href="../current-decisions.md">Project decisions</a></nav>' + body + '</main></body></html>'
source.with_suffix('.html').write_text(page, encoding='utf-8')
print(source.with_suffix('.html'))
