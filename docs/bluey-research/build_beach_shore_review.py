"""Render the scoped beach implementation record with its native captures."""
from pathlib import Path
import re
import markdown
docs=Path(__file__).resolve().parent.parent
name='beach-waves-2026-09-30'
source=docs/'implementation'/f'{name}.md'
body=markdown.markdown(source.read_text(encoding='utf-8'),extensions=['tables','fenced_code','toc'])
guide=(docs/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8')
style=re.search(r'<style>(.*?)</style>',guide,re.S).group(1)
page=f'<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Beach waves and sea visitors</title><style>{style} main{{max-width:1000px;margin:auto;padding:30px 24px}} main img{{max-width:100%;height:auto;border-radius:16px}}</style></head><body><main>{body}</main></body></html>'
source.with_suffix('.html').write_text(page,encoding='utf-8')
print('Rendered beach waves record.')
