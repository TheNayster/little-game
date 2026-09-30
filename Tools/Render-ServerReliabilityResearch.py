# /// script
# dependencies = ["markdown"]
# ///
"""Render the applied PC server/update research using the project document style."""
from pathlib import Path
import re
import markdown

docs = Path(__file__).resolve().parents[1] / "docs"
name = "pc-server-reliability-research-2026-09-30"
source = docs / "implementation" / (name + ".md")
style = re.search(r"<style>(.*?)</style>", (docs / "bluey-game-research-2026-09-23.html").read_text(encoding="utf-8"), re.S)[1]
body = markdown.markdown(source.read_text(encoding="utf-8"), extensions=["tables", "fenced_code", "toc"])
body = body.replace("<table>", '<div class="table-scroll" tabindex="0" role="region" aria-label="Server research table"><table>').replace("</table>", "</table></div>")
page = '<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Little Weeps — PC server reliability</title><style>' + style + 'main{max-width:1080px;margin:32px auto;padding:24px}p,li{line-height:1.65}table{min-width:620px;width:100%;border-collapse:collapse}th,td{padding:12px;border:1px solid #c9dadf;text-align:left;vertical-align:top}.table-scroll{overflow-x:auto}code{overflow-wrap:anywhere}@media(max-width:640px){main{padding:16px;margin:12px auto}}@media print{main{margin:0;padding:0}h2{break-after:avoid}}</style></head><body><main><p><a href="../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Current build plan</a> · <a href="' + name + '.md">Editable research</a></p>' + body + '</main></body></html>'
source.with_suffix(".html").write_text(page, encoding="utf-8")
print(source.with_suffix(".html"))
