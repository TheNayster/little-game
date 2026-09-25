"""Render the saved feasibility audit using the existing research-page style.

Run after build_guide.py with the same Python Markdown dependency.
"""
from pathlib import Path
import re
import markdown

ROOT = Path(__file__).resolve().parent.parent
BASE = "family-playset-feasibility-audit-2026-09-23"
source = (ROOT / f"{BASE}.md").read_text(encoding="utf-8")
md = markdown.Markdown(extensions=["tables", "fenced_code", "toc"],
    extension_configs={"toc": {"toc_depth": "2-2"}})
body = md.convert(source)
body = re.sub(r"<h1.*?</h1>", "", body, count=1, flags=re.S)
body = body.replace("<table>", '<div class="table-scroll" tabindex="0" role="region" aria-label="Audit table"><table>')
body = body.replace("</table>", "</table></div>")
guide = (ROOT / "bluey-game-research-2026-09-23.html").read_text(encoding="utf-8")
style = re.search(r"<style>(.*?)</style>", guide, re.S).group(1)
page = '''<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>The Family Playset — feasibility audit</title><style>''' + style + '''
.audit-intro{padding:48px 0 25px}.audit-intro h1{font-size:clamp(32px,4vw,52px);margin:0 0 20px}.audit-intro p{max-width:900px}.audit-status{display:flex;flex-wrap:wrap;gap:9px;margin-bottom:28px}.audit-status span{border-radius:20px;background:#e7f0f1;padding:6px 14px;font-size:14px}.report>h2:first-of-type{margin-top:32px}.topbar nav{flex-wrap:wrap}
</style></head><body>
<header class="topbar"><a class="brand" href="bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a><nav aria-label="Audit navigation"><a href="bluey-game-research-2026-09-23.html#52-feasibility-audit-and-current-priorities">Main research</a><a href="#2-feature-by-feature-verdict">Feature verdicts</a><a href="#6-the-proof-plan-before-producing-the-full-game">Proof plan</a></nav></header>
<main class="wrap" id="top"><div class="audit-intro"><p class="eyebrow">Full plan review · 23 September 2026</p><h1>Can we build this game?<br>Yes—with the right foundations.</h1><p class="intro">The whole playset is feasible in principle. Automatic multiplayer recovery and the older iPad's workload need early device proof; the artwork, voices and activities need staged production.</p></div>
<div class="audit-status"><span>Feature feasibility + current scope audit</span><span>PC/VPS shared authority; clients only</span><span>Travel multiplayer: optional</span><span>Offline solo: required</span></div>
<div class="report-layout"><aside class="contents"><h3>In this audit</h3>''' + md.toc + '''</aside><article class="report">''' + body + '''</article></div>
<footer class="footer"><p>Design audit, not runtime certification. No Unity implementation or device test was performed.</p><a href="family-playset-feasibility-audit-2026-09-23.md">Editable audit</a><a href="bluey-game-research-2026-09-23.html">Full research and character board</a></footer></main></body></html>'''
destination = ROOT / f"{BASE}.html"
destination.write_text(page, encoding="utf-8")
print(f"Created {destination}: {len(page):,} characters; {len(md.toc_tokens)} audit sections")
