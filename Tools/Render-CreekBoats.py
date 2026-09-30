# /// script
# dependencies = ["markdown"]
# ///
"""Render the creek boat research and native captures in the established docs format."""
from pathlib import Path
import markdown

root=Path(__file__).resolve().parents[1]
source=root/'docs/implementation/creek-boats-2026-09-30.md'
body=markdown.markdown(source.read_text(encoding='utf-8'),extensions=['tables','fenced_code','toc'])
style='''*{box-sizing:border-box}body{margin:0;background:#edf5ed;color:#243951;font:17px/1.65 system-ui,sans-serif}main{max-width:1000px;margin:28px auto;padding:30px;background:#fffdf5;border-radius:18px}h1{font-size:clamp(32px,5vw,50px);line-height:1.15;color:#245748}h2{margin-top:40px;padding-top:16px;border-top:3px solid #d5e7d1}a{color:#145d86;text-underline-offset:3px}img{display:block;width:100%;height:auto;border:1px solid #dce4e6;border-radius:12px;margin:20px 0}li{margin-bottom:9px}p,li{overflow-wrap:anywhere}nav{font-size:14px}@media(max-width:700px){main{padding:18px;margin:0;border-radius:0}}@media print{body{background:white;font-size:11pt}main{margin:0;padding:0}h2{break-after:avoid}}'''
page='<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Creek Boat Research and Playable Build</title><style>'+style+'</style></head><body><main><nav><a href="creek-boats-2026-09-30.md">Editable report</a> · <a href="../creek-world-feature-list-2026-09-30.md">Creek feature list</a></nav>'+body+'</main></body></html>'
source.with_suffix('.html').write_text(page,encoding='utf-8')
print(source.with_suffix('.html'))
