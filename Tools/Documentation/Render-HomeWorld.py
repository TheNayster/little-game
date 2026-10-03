# /// script
# dependencies = ["markdown"]
# ///
"""Render the home milestone and native screenshots alongside the maintained plan."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import markdown
import re
root=PROJECT_ROOT/'docs'
base=root/'implementation/home-interactions-2026-09-25'
style=re.search(r'<style>(.*?)</style>',(root/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S).group(1)
body=markdown.markdown(base.with_suffix('.md').read_text(encoding='utf-8'),extensions=['tables','fenced_code','toc'])
base.with_suffix('.html').write_text('<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Little Weeps — Home interactions</title><style>'+style+'main{max-width:1100px;margin:40px auto;padding:20px}img{max-width:100%;height:auto;border-radius:16px}table{width:100%;border-collapse:collapse}td,th{padding:12px;border-bottom:1px solid #c9dadf;text-align:left}</style></head><body><main><p><a href="../family-playset-build-guide-2026-09-23.html">Current build plan</a> · <a href="../bluey-lets-play-reference-study-2026-09-25.html">Reference research</a></p>'+body+'</main></body></html>',encoding='utf-8')
print(base.with_suffix('.html'))
