# /// script
# dependencies = ["markdown"]
# ///
"""Render the bathroom record and maintained Home tracker without resetting other goals."""
from pathlib import Path
import re,markdown,html
ROOT=Path(__file__).resolve().parent.parent
docs=ROOT/'docs'
style=re.search(r'<style>(.*?)</style>',(docs/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S).group(1)
for name in ['implementation/bathroom-2026-09-30','home-world-feature-tracker']:
    source=docs/(name+'.md');body=markdown.markdown(source.read_text(encoding='utf-8-sig'),extensions=['tables','fenced_code','toc'])
    title=html.escape(source.read_text(encoding='utf-8-sig').splitlines()[0].lstrip('# '))
    page='<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>'+title+'</title><style>'+style+'main{max-width:1100px;margin:36px auto;padding:24px}img{max-width:100%;height:auto;border-radius:14px}table{width:100%;border-collapse:collapse}td,th{padding:10px;border-bottom:1px solid #c9dadf;text-align:left}</style></head><body><main>'+body+'</main></body></html>'
    source.with_suffix('.html').write_text(page,encoding='utf-8')
    print(source.with_suffix('.html'))
