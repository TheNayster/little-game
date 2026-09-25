"""Render the G3 Apple preparation report and concise return checklist."""
from pathlib import Path
import re
import markdown

root=Path(__file__).resolve().parent.parent
style=re.search(r'<style>(.*?)</style>',(root/'bluey-game-research-2026-09-23.html').read_text(encoding='utf-8'),re.S).group(1)
for base,title in [('g3-ipad-lan-2026-09-24','iPad preparation and automatic rejoin'),('g3-android-lan-2026-09-24','Android automatic family connection'),('g3-offline-join-2026-09-24','Automatic joining from solo play'),('g3-ipad-79-2026-09-24','Both iPads on family build 79'),('g3-phones-79-2026-09-24','Samsung and iPhone family build 79'),('g3-phone-layout-resets-2026-09-24','Wider phones and repeatable garden play'),('g3-persistent-server-2026-09-24','Persistent home server'),('g3-parent-controls-2026-09-24','Parent server controls'),('g3-server-recovery-2026-09-24','Server backup and restore'),('g3-server-supervision-2026-09-24','Bounded server crash recovery'),('g3-server-soak-2026-09-24','Sustained four-player Windows run'),('g3-parent-recovery-panel-2026-09-24','Parent backup and recovery panel'),('g3-client-recovery-2026-09-24','Durable client recovery checkpoints'),('g3-local-continuation-2026-09-25','Local continuation and saved adventures'),('g3-outage-failures-2026-09-25','Outage failure checks'),('g3-mobile-recovery-builds-2026-09-25','Mobile recovery build preparation'),('g3-portable-recovery-2026-09-25','Portable encrypted server recovery'),('g3-android-recovery-2026-09-25','Android emulator recovery'),('g3-parent-portable-backups-2026-09-25','Protected portable backups'),('g3-sustained-recovery-play-2026-09-25','Sustained recovery-build play'),('g3-signin-startup-2026-09-25','Optional Windows sign-in startup'),('vps-hosting-plan-2026-09-24','Future VPS hosting'),('return-checklist-ipad-lan-2026-09-24','When you return')]:
    source=root/'implementation'/f'{base}.md'
    body=markdown.markdown(source.read_text(encoding='utf-8'),extensions=['tables','fenced_code'])
    body=body.replace('<table>','<div class="table-scroll"><table>').replace('</table>','</table></div>')
    page='''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>The Family Playset — '''+title+'''</title><style>'''+style+'''
.progress-report{max-width:980px;margin:42px auto;padding:0 24px}.progress-report h1{font-size:clamp(30px,4vw,46px)}.progress-report p,.progress-report li{line-height:1.65}.progress-report code{overflow-wrap:anywhere}.progress-report pre{overflow-x:auto}
</style></head><body><header class="topbar"><a class="brand" href="../bluey-game-research-2026-09-23.html">THE FAMILY PLAYSET</a><nav><a href="../family-playset-build-guide-2026-09-23.html#18-current-work-record-and-research-basis">Build status</a><a href="return-checklist-ipad-lan-2026-09-24.html">Return checklist</a><a href="'''+base+'''.md">Editable record</a></nav></header><main class="progress-report report">'''+body+'''</main></body></html>'''
    source.with_suffix('.html').write_text(page,encoding='utf-8');print('Rendered '+base+'.html')
