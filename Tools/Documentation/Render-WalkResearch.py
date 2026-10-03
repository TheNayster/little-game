# /// script
# dependencies = ["markdown"]
# ///
"""Render walking research, implementation and the Android delivery record."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import re
import markdown

docs = PROJECT_ROOT / 'docs'
style = re.search(r'<style>(.*?)</style>', (docs / 'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'), re.S).group(1)
for name, title in [
    ('keepy-uppy-2026-09-26', 'Little Weeps — four-player Keepy Uppy'),
    ('family-home-rollout-2026-09-26', 'Little Weeps — iPads and home server 128'),
    ('integrated-home-2026-09-26', 'Little Weeps — integrated home objects'),
    ('movement-speed-2026-09-26', 'Little Weeps — faster character travel 124–125'),
    ('selected-sheet-characters-2026-09-26', 'Little Weeps — selected Bluey and matching Bingo 122–123'),
    ('profile-walk-2026-09-26', 'Little Weeps — side-view walking 119'),
    ('walk-animation-research-2026-09-26', 'Little Weeps — relaxed walking'),
    ('walk-animation-2026-09-26', 'Little Weeps — walking and chooser revision 118'),
    ('android-home-update-2026-09-26', 'Little Weeps — Android home update'),
]:
    source = docs / 'implementation' / (name + '.md')
    body = markdown.markdown(source.read_text(encoding='utf-8'), extensions=['tables', 'fenced_code', 'toc'])
    body = body.replace('<img ', '<img style="max-width:100%;height:auto" ')
    body = body.replace('<table>', '<div class="table-scroll" tabindex="0" role="region" aria-label="Research table"><table>').replace('</table>', '</table></div>')
    page = '<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>' + title + '</title><style>' + style + 'main{max-width:1100px;margin:40px auto;padding:20px}table{min-width:680px;width:100%;border-collapse:collapse}td,th{padding:12px;border-bottom:1px solid #c9dadf;text-align:left}code{overflow-wrap:anywhere}.table-scroll{overflow-x:auto}p,li{line-height:1.65}</style></head><body><main><p><a href="../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Current build plan</a> · <a href="' + name + '.md">Editable source</a></p>' + body + '</main></body></html>'
    source.with_suffix('.html').write_text(page, encoding='utf-8')
    print(source.with_suffix('.html'))
