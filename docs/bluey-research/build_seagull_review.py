"""Render the scoped seagull research and native release evidence."""
from pathlib import Path
import re
import markdown
docs=Path(__file__).resolve().parent.parent
name='seagull-surprise-2026-09-30'
source=docs/'implementation'/f'{name}.md'
body=markdown.markdown(source.read_text(encoding='utf-8'),extensions=['tables','fenced_code','toc'])
guide=(docs/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8')
style=re.search(r'<style>(.*?)</style>',guide,re.S).group(1)
page=f'''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Seagull surprise at The Beach</title><style>{style}
main{{max-width:1000px;margin:auto;padding:30px 24px}} main img{{max-width:100%;height:auto;border-radius:16px}} table{{width:100%;border-collapse:collapse}} td,th{{padding:12px;text-align:left;vertical-align:top;border-bottom:1px solid #d8e3e1}} a{{overflow-wrap:anywhere}}</style></head><body><main><nav><a href="../beach-world-wishlist.md">Beach wish list</a> · <a href="{name}.md">Editable record</a></nav>{body}</main></body></html>'''
source.with_suffix('.html').write_text(page,encoding='utf-8')
print('Rendered seagull research and release captures.')
