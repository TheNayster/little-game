"""Render the ground-up build guide using the Family Playset document style."""
from pathlib import Path
import re
import markdown

ROOT = Path(__file__).resolve().parent.parent
BASE = "family-playset-build-guide-2026-09-23"
md = markdown.Markdown(extensions=["tables", "fenced_code", "toc"],
    extension_configs={"toc": {"toc_depth": "2-2"}})
body = md.convert((ROOT / f"{BASE}.md").read_text(encoding="utf-8"))
body = re.sub(r"<h1.*?</h1>", "", body, count=1, flags=re.S)
body = body.replace("<table>", '<div class="table-scroll" tabindex="0" role="region" aria-label="Build guide table"><table>')
body = body.replace("</table>", "</table></div>")
guide = (ROOT / "bluey-game-research-2026-09-23.html").read_text(encoding="utf-8")
style = re.search(r"<style>(.*?)</style>", guide, re.S).group(1)
page = '''<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>The Family Playset — ground-up build guide</title><style>''' + style + '''
.build-intro{padding:42px 0 24px}.build-intro h1{font-size:clamp(34px,4.6vw,58px);margin:0 0 18px}.build-intro p{max-width:890px}.build-status{display:flex;flex-wrap:wrap;gap:9px;margin-bottom:24px}.build-status span{border-radius:20px;background:#e7f0f1;padding:6px 14px;font-size:14px}.build-status .next{background:#ffe6b3}.topbar nav{flex-wrap:wrap}.report code{overflow-wrap:anywhere}.report pre{overflow-x:auto;max-width:100%;background:#edf2f1;padding:20px;border-radius:12px}.report pre code{white-space:pre;overflow-wrap:normal;font-size:13px}.build-roadmap{display:grid;grid-template-columns:repeat(5,minmax(0,1fr));list-style:none;padding:0;margin:0 0 36px;gap:12px}.build-roadmap li{background:#fff;border:1px solid var(--line);border-radius:14px;padding:17px}.build-roadmap a{text-decoration:none;display:block}.build-roadmap small{display:block;letter-spacing:.07em;font-size:11px;font-weight:700;margin-bottom:8px}.build-roadmap strong{display:block;font-size:16px;line-height:1.4}.build-roadmap span{display:block;font-size:13px;margin-top:8px;color:#596a6b}.build-callout{border-left:4px solid #d3a13c;padding:16px 20px;background:#fff6e4;border-radius:0 12px 12px 0;margin-bottom:30px}.build-callout p{margin:4px 0}.build-callout a{font-weight:700}
@media(max-width:900px){.build-roadmap{grid-template-columns:repeat(3,minmax(0,1fr))}}
@media(max-width:620px){.build-roadmap{grid-template-columns:repeat(2,minmax(0,1fr))}.build-intro h1{font-size:38px}.build-roadmap li{padding:14px}}
@media print{.build-roadmap,.build-callout{break-inside:avoid}.report pre{white-space:pre-wrap}.report pre code{white-space:pre-wrap}.build-roadmap{grid-template-columns:repeat(5,minmax(0,1fr))}}
</style></head><body>
<header class="topbar"><a class="brand" href="bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a><nav aria-label="Build guide navigation"><a href="bluey-game-research-2026-09-23.html">Goal sheet</a><a href="#8-ordered-phases-and-completion-gates">Build order</a><a href="#9-first-implementation-work-queue">Next tasks</a><a href="#18-current-work-record-and-research-basis">Current status</a></nav></header>
<main class="wrap" id="top"><div class="build-intro"><p class="eyebrow">Ground-up production guide · 23 September 2026</p><h1>One clear build order.<br>A foundation for the whole game.</h1><p class="intro">The Family Playset is our goal sheet. This guide turns it into a practical sequence: prove the foundations, finish a representative playable area, expand the content, and deliver reliable updates to the family's devices.</p></div>
<div class="build-status"><span>35 feature IDs mapped</span><span>10 ordered phases</span><span>Foundation setup started</span><span class="next">Active: G1 project &amp; devices</span></div>
<ol class="build-roadmap" aria-label="Production sequence">
<li><a href="#7-android-ios-and-pc-build-pipeline"><small>G1</small><strong>Establish the project</strong><span>Builds, devices, saves and updates.</span></a></li>
<li><a href="#8-ordered-phases-and-completion-gates"><small>G2–G5</small><strong>Prove shared play</strong><span>Touch, world rules, hosting and recovery.</span></a></li>
<li><a href="#g6-the-representative-finished-slice"><small>G6</small><strong>Finish one slice</strong><span>Real art, voices, activities and child testing.</span></a></li>
<li><a href="#12-content-batch-order-after-the-foundations-pass"><small>G7</small><strong>Build the full content</strong><span>Six worlds, characters and activity batches.</span></a></li>
<li><a href="#15-family-delivery-updates-and-recovery"><small>G8–G9</small><strong>Deliver and maintain</strong><span>Signed releases, backups and safe updates.</span></a></li>
</ol>
<div class="build-callout"><p><strong>Four-player movement is smoother in Windows shared build 63.</strong></p><p>The updated two-window preview preserves your world; four native clients walking together passed normal and delayed/lost-motion tests. Both iPads remain on solo 56. The separate intermittent rejoin snapshot stall remains the next reliability fix. G1/G2/G3 gates, mobile multiplayer, automatic joining and iPad hosting are still open. <a href="implementation/g3-smooth-movement-2026-09-24.html">Read the research and results →</a> <a href="#18-current-work-record-and-research-basis">Current status →</a></p></div>
<div class="report-layout"><aside class="contents"><h3>In this build guide</h3>''' + md.toc + '''</aside><article class="report">''' + body + '''</article></div>
<footer class="footer"><p>A source-backed production plan for this game. The precise architecture and phase gates are our proposed method; no game implementation or device qualification was performed while writing it.</p><a href="family-playset-build-guide-2026-09-23.md">Editable build guide</a><a href="bluey-game-research-2026-09-23.html">Feature goal sheet</a><a href="family-playset-package-research-2026-09-23.html">Package research</a></footer></main></body></html>'''
(ROOT / f"{BASE}.html").write_text(page, encoding="utf-8")
print(f"Created {BASE}.html: {len(page):,} characters; {len(md.toc_tokens)} sections")
