"""Render the package review using the existing Family Playset guide style."""
from pathlib import Path
import re
import markdown

ROOT = Path(__file__).resolve().parent.parent
BASE = "family-playset-package-research-2026-09-23"
md = markdown.Markdown(extensions=["tables", "fenced_code", "toc"],
    extension_configs={"toc": {"toc_depth": "2-2"}})
body = md.convert((ROOT / f"{BASE}.md").read_text(encoding="utf-8"))
body = re.sub(r"<h1.*?</h1>", "", body, count=1, flags=re.S)
body = body.replace("<table>", '<div class="table-scroll" tabindex="0" role="region" aria-label="Package comparison table"><table>')
body = body.replace("</table>", "</table></div>")
guide = (ROOT / "bluey-game-research-2026-09-23.html").read_text(encoding="utf-8")
style = re.search(r"<style>(.*?)</style>", guide, re.S).group(1)
page = '''<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>The Family Playset — templates and packages</title><style>''' + style + '''
.package-intro{padding:42px 0 24px}.package-intro h1{font-size:clamp(32px,4vw,52px);margin:0 0 18px}.package-intro p{max-width:880px}.package-status{display:flex;flex-wrap:wrap;gap:9px;margin-bottom:28px}.package-status span{border-radius:20px;background:#e7f0f1;padding:6px 14px;font-size:14px}.topbar nav{flex-wrap:wrap}.report code{overflow-wrap:anywhere}
</style></head><body>
<header class="topbar"><a class="brand" href="bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a><nav aria-label="Package review navigation"><a href="bluey-game-research-2026-09-23.html#54-templates-packages-and-the-free-starting-setup">Main plan</a><a href="#3-official-free-packages-and-exact-candidates">Free packages</a><a href="#6-one-optional-purchase-within-the-budget">Optional purchase</a></nav></header>
<main class="wrap" id="top"><div class="package-intro"><p class="eyebrow">Templates + package research · 23 September 2026</p><h1>A free foundation.<br>Built for your family.</h1><p class="intro">Start with Unity's official 2D template and focused, reusable tools. This review shows what they provide, which templates do not fit, and what still needs our own game rules and device testing.</p></div>
<div class="package-status"><span>Starting package budget: $0</span><span>Research candidates; see pinned project</span><span>Optional purchase: about $15</span><span>Installed versions in current decisions</span></div>
<div class="report-layout"><aside class="contents"><h3>In this review</h3>''' + md.toc + '''</aside><article class="report">''' + body + '''</article></div>
<footer class="footer"><p>Source-backed research and a proposed setup. Native builds, package integration and physical-device tests remain to be done.</p><a href="family-playset-package-research-2026-09-23.md">Editable package report</a><a href="family-playset-technical-research-2026-09-23.html">Networking research</a><a href="bluey-game-research-2026-09-23.html">Main research and character board</a></footer></main></body></html>'''
(ROOT / f"{BASE}.html").write_text(page, encoding="utf-8")
print(f"Created {BASE}.html: {len(page):,} characters; {len(md.toc_tokens)} sections")
