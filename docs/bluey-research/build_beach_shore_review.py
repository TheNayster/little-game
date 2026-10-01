"""Render the scoped beach implementation record with its native captures."""
from pathlib import Path
import re
import markdown
docs=Path(__file__).resolve().parent.parent
guide=(docs/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8')
style=re.search(r'<style>(.*?)</style>',guide,re.S).group(1)
for name,title in [('beach-waves-2026-09-30','Beach waves and sea visitors'),('sea-visitor-models-2026-09-30','Sea visitor model sheets'),('beach-wave-ride-2026-09-30','Ride the waves mini-game')]:
    source=docs/'implementation'/f'{name}.md'
    body=markdown.markdown(source.read_text(encoding='utf-8'),extensions=['tables','fenced_code','toc'])
    page=f'<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{title}</title><style>{style} main{{max-width:1000px;margin:auto;padding:30px 24px}} main img{{max-width:100%;height:auto;border-radius:16px;background:#e8f4f6}}</style></head><body><main>{body}</main></body></html>'
    source.with_suffix('.html').write_text(page,encoding='utf-8')
    print('Rendered '+name)
