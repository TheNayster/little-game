"""Render the technical research with the existing Family Playset style."""
from pathlib import Path
import re
import markdown

ROOT = Path(__file__).resolve().parent.parent
BASE = "family-playset-technical-research-2026-09-23"
md = markdown.Markdown(extensions=["tables", "fenced_code", "toc"],
    extension_configs={"toc": {"toc_depth": "2-2"}})
body = md.convert((ROOT / f"{BASE}.md").read_text(encoding="utf-8"))
body = re.sub(r"<h1.*?</h1>", "", body, count=1, flags=re.S)
body = body.replace("<table>", '<div class="table-scroll" tabindex="0" role="region" aria-label="Technical research table"><table>')
body = body.replace("</table>", "</table></div>")
guide = (ROOT / "bluey-game-research-2026-09-23.html").read_text(encoding="utf-8")
style = re.search(r"<style>(.*?)</style>", guide, re.S).group(1)
page = '''<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>The Family Playset — technical research</title><style>''' + style + '''
.tech-intro{padding:42px 0 24px}.tech-intro h1{font-size:clamp(32px,4vw,52px);margin:0 0 18px}.tech-intro p{max-width:880px}.tech-status{display:flex;flex-wrap:wrap;gap:9px;margin-bottom:28px}.tech-status span{border-radius:20px;background:#e7f0f1;padding:6px 14px;font-size:14px}.topbar nav{flex-wrap:wrap}
</style></head><body>
<header class="topbar"><a class="brand" href="bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a><nav aria-label="Technical research navigation"><a href="bluey-game-research-2026-09-23.html#53-source-backed-networking-and-recovery-review">Main research</a><a href="#1-automatic-pc-connection-is-the-normal-path">Automatic connection</a><a href="#9-what-changed-in-confidence-and-what-to-build-first">Findings</a></nav></header>
<main class="wrap" id="top"><div class="tech-intro"><p class="eyebrow">Documentation + source review · 23 September 2026</p><h1>One family world.<br>Come and go freely.</h1><p class="intro">Automatic connection to your PC server is the normal path. This research checks what the networking tools actually provide, what needs custom game logic, and what still needs to be proven on your iPads.</p></div>
<div class="tech-status"><span>PC server: normal home play</span><span>iPad hosting &amp; recovery: required</span><span>Offline solo: required</span><span>Bluetooth: removed</span></div>
<div class="report-layout"><aside class="contents"><h3>In this review</h3>''' + md.toc + '''</aside><article class="report">''' + body + '''</article></div>
<footer class="footer"><p>Targeted source inspection and research. No game implementation or device benchmark was run.</p><a href="family-playset-technical-research-2026-09-23.md">Editable technical report</a><a href="family-playset-feasibility-audit-2026-09-23.html">Full feasibility audit</a><a href="bluey-game-research-2026-09-23.html">Main research and character board</a></footer></main></body></html>'''
(ROOT / f"{BASE}.html").write_text(page, encoding="utf-8")
print(f"Created {BASE}.html: {len(page):,} characters; {len(md.toc_tokens)} sections")
