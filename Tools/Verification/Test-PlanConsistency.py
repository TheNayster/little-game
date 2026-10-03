"""Check maintained plan scope and rendered links; no Unity or device access.

Run after rendering with: uv run --offline python Tools/Verification/Test-PlanConsistency.py
Historical reports retain their evidence but must visibly direct readers to current scope.
This catches known requirement regressions, not every possible semantic contradiction.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
from html.parser import HTMLParser
from urllib.parse import urlsplit, unquote
import argparse
import json
import re

ROOT = PROJECT_ROOT
DOC = ROOT / 'docs'
HISTORY_START = '<!-- historical-record-start -->'
HISTORY_END = '<!-- historical-record-end -->'
# Positive forms that caused this audit. Negative/retirement statements stay legal.
CONFLICTS = [
    r'(?:both|either|each) (?:of )?(?:the )?ipads? (?:must be capable|can|should|as|to|will) host',
    r'(?:required|mandatory) ipad hosting',
    r'ipad hosting[^.\n]{0,80}(?:remain|stay|:|is|are)[^.\n]{0,20}required',
    r'next[^.\n]{0,40}G4-0[1-5]',
    r'successor per reachable group',
    r'qualified mobile device hosts',
    r'import disjoint records',
    r'submit idempotent local operations',
    r'offline (?:edits|changes)[^.\n]{0,30}(?:must|will|should) (?:merge|upload)',
    r'hosting and (?:automatic )?reconciliation remain required',
    r'both client/host roles',
    r'A10 hosting while',
    r'host-role test device',
    r'test each ipad as host',
    r'G3\s*→\s*G4\s*→\s*G5',
]
def conflicts(text):
    found=[]
    for n,line in enumerate(text.splitlines(),1):
        # A retirement label refers to history, not a positive requirement.
        if re.search(r'\b(retired|removed|historical|superseded)\b',line,re.I):
            continue
        for pattern in CONFLICTS:
            if re.search(pattern,line,re.I):found.append((n,line.strip()))
    return found

class Page(HTMLParser):
    def __init__(self, text):
        super().__init__(); self.ids=set(); self.links=[]; self.duplicates=[]; self.feed(text)
    def handle_starttag(self, tag, attrs):
        a=dict(attrs)
        if 'id' in a:
            if a['id'] in self.ids:self.duplicates.append(a['id'])
            self.ids.add(a['id'])
        if tag=='a' and a.get('href'):self.links.append(a['href'])

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--output',default='docs/implementation/evidence/pc-vps-plan-audit-2026-09-25/docs-validation.json')
    args=parser.parse_args()
    scope=json.loads((DOC/'plan-scope.json').read_text(encoding='utf-8'))
    errors=[]
    active=scope['active_entry_files']; historical=scope['historical_records']
    known=set(active+historical+scope.get("local_review_records",[]))
    actual={p.relative_to(ROOT).as_posix() for p in [*DOC.glob('*.md'),*(DOC/'implementation').glob('*.md')]}
    for name in sorted(actual-known):errors.append([name,'unclassified authored record'])
    for name in active:
        p=ROOT/name
        if not p.exists():errors.append([name,'missing maintained entry']);continue
        for n,line in conflicts(p.read_text(encoding='utf-8')):errors.append([name,n,'conflicting active requirement',line])
        width=None
        for n,line in enumerate(p.read_text(encoding='utf-8').splitlines(),1):
            if line.startswith('|'):
                count=len(re.split(r'(?<!\\)\|',line))-2
                if width is not None and count!=width:errors.append([name,n,'inconsistent Markdown table columns'])
                width=count
            else:width=None
    for name in historical:
        p=ROOT/name;text=p.read_text(encoding='utf-8')
        if 'Historical implementation record' not in text or HISTORY_START not in text or HISTORY_END not in text:
            errors.append([name,'missing explicit historical scope'])
    samples=[
        'Both iPads must be capable hosts when needed.',
        'Required iPad hosting blocks G6.',
        'Next independent task: G4-01.',
        'Offline edits will merge on reconnect.',
        'Qualified mobile device hosts a saved travel continuation.',
        'G3 → G4 → G5',
    ]
    for sample in samples:
        if not conflicts(sample):errors.append(['regression detector failed',sample])
    if conflicts('G4 is retired. Offline edits stay private; server state wins.'):
        errors.append(['regression detector rejects current scope'])
    goals=(DOC/'bluey-game-research-2026-09-23.md').read_text(encoding='utf-8')
    plan=(DOC/'family-playset-build-guide-2026-09-23.md').read_text(encoding='utf-8')
    chapters=re.findall(r'^## (\d+)\.',goals,re.M)
    sections=re.findall(r'^## (\d+)\.',plan,re.M)
    ids=re.findall(r'^\| \*\*([A-Z]+-\d+)\*\*',plan,re.M)
    tracker=re.search(r'^## 16\. .*?(?=^## 17\.)',goals,re.M|re.S).group(0)
    goal_ids=re.findall(r'^\| ([A-Z]+-\d+) \|',tracker,re.M)
    if chapters!=list(map(str,range(1,56))):errors.append(['goal chapters',chapters])
    if sections!=list(map(str,range(1,20))):errors.append(['plan sections',sections])
    if len(ids)!=35 or len(set(ids))!=35:errors.append(['feature ledger',ids])
    if set(ids)!=set(goal_ids):errors.append(['goal/plan IDs differ',sorted(set(ids)^set(goal_ids))])
    for text in (goals,plan):
        row=re.search(r'^\| (?:\*\*)?AUTO-02(?:\*\*)? \|(.+)$',text,re.M)
        if not row or not re.search('retired',row.group(1),re.I):errors.append(['AUTO-02 retirement missing'])
    for line in plan.splitlines():
        if line.startswith('| **G4') and 'retired' not in line.lower():errors.append(['G4 active phase row',line])
    # These catalogs must not shrink during an architecture edit.
    expected_catalogs={'SHOW':32,'BCH':10,'CRK':10,'PRK':12,'LRN':12,'PIZ':5,'CAK':5,'MEAL':5}
    catalogs={prefix:len(re.findall(r'^\| '+prefix+r'-\d+\b',goals,re.M)) for prefix in expected_catalogs}
    if catalogs!=expected_catalogs:errors.append(['content catalogs changed',catalogs])
    pages=sorted({p.with_suffix('.html') for p in [ROOT/f for f in active+historical] if p.suffix=='.md' and p.with_suffix('.html').exists()})
    pages.append(DOC/'implementation/character-workshop/index.html')
    cache={};link_count=0
    def load(p):
        p=p.resolve()
        if p not in cache:cache[p]=Page(p.read_text(encoding='utf-8'))
        return cache[p]
    for p in pages:
        rel=p.relative_to(ROOT).as_posix();obj=load(p)
        if obj.duplicates:errors.append([rel,'duplicate anchors',obj.duplicates])
        if p.with_suffix('.md').relative_to(ROOT).as_posix() in historical and 'Historical implementation record' not in p.read_text(encoding='utf-8'):
            errors.append([rel,'stale render: missing historical notice'])
        for link in obj.links:
            parts=urlsplit(link)
            if parts.scheme or parts.netloc:continue
            target=(p.parent/unquote(parts.path)).resolve() if parts.path else p.resolve()
            link_count+=1
            if not target.exists():errors.append([rel,link,'missing path']);continue
            if target.suffix=='.html' and parts.fragment and unquote(parts.fragment) not in load(target).ids:
                errors.append([rel,link,'missing anchor'])
    for p in (DOC/'bluey-research').glob('build*.py'):
        for n,line in conflicts(p.read_text(encoding='utf-8')):errors.append([p.relative_to(ROOT).as_posix(),n,'conflicting renderer',line])
    result=dict(passed=not errors,decision='PC/VPS authority; private offline solo; server wins on reconnect',
                activeEntries=len(active),historicalRecords=len(historical),goalChapters=len(chapters),
                planSections=len(sections),featureIds=ids,retiredGoals=['AUTO-02'],retiredPhases=['G4'],
                contentCatalogCounts=catalogs,renderedPages=len(pages),localLinksChecked=link_count,
                conflictingPhraseRegressionCases=len(samples),errors=errors,
                limits='Local document consistency/link checks and manually reviewed scope; not live device, runtime or external-source revalidation.')
    out=ROOT/args.output;out.parent.mkdir(parents=True,exist_ok=True)
    out.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(result,indent=2))
    return 0 if result['passed'] else 1

if __name__=='__main__':raise SystemExit(main())
