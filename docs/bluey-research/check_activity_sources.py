"""Check public source URLs for the expanded main guide; no application code runs.

Run: python docs/bluey-research/check_activity_sources.py
The evidence stores source metadata and our own catalog, not copied episode text.
"""
import concurrent.futures
import datetime
import html
import json
import re
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent
source = (ROOT.parent / 'bluey-game-research-2026-09-23.md').read_text(encoding='utf-8')
expanded = source.split('## 16. Expanded feature tracker', 1)[1]
urls = sorted(set(re.findall(r'\]\((https://[^)]+)\)', expanded)))


def check(url):
    try:
        request = urllib.request.Request(url, headers={'User-Agent': 'Meeps-Game-Research'})
        with urllib.request.urlopen(request, timeout=25) as response:
            body = response.read().decode('utf-8-sig', errors='replace')
            title = re.search(r'<title[^>]*>(.*?)</title>', body, re.S | re.I)
            return {'url': url, 'resolved': response.geturl(), 'status': response.status,
                    'title': html.unescape(re.sub(r'\s+', ' ', title.group(1))).strip() if title else None}
    except Exception as exc:
        return {'url': url, 'error': str(exc)}


with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:
    checks = list(pool.map(check, urls))

catalog = []
catalog_section = expanded.split('## 23. Show games and activities catalog', 1)[1].split('## 24.', 1)[0]
for line in catalog_section.splitlines():
    if not line.startswith('| SHOW-'):
        continue
    cells = [part.strip() for part in line.strip('|').split('|')]
    match = re.match(r'(SHOW-\d+) \[([^]]+)\]\(([^)]+)\)(.*)', cells[0])
    catalog.append({'id': match[1], 'episode': match[2], 'source': match[3],
                    'activityQualifier': match[4].strip(' —'), 'showReference': cells[1],
                    'simplePlay': cells[2], 'cooperativePlay': cells[3],
                    'systemAndPriority': cells[4]})

evidence = {'checkedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(),
            'scope': 'Official public descriptions and proposed game adaptations; no episode video or game runtime tested.',
            'catalogCount': len(catalog), 'distinctCatalogEpisodeSources': len({row['source'] for row in catalog}),
            'sources': checks, 'proposedActivities': catalog}
(ROOT / 'activity-research-evidence.json').write_text(json.dumps(evidence, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'catalogCount': len(catalog), 'distinctEpisodeSources': evidence['distinctCatalogEpisodeSources'],
                  'sourcesChecked': len(checks), 'failures': [x for x in checks if x.get('status') != 200]}, ensure_ascii=False, indent=2))
for row in checks:
    print(str(row.get('status', 'ERROR')) + ' | ' + str(row.get('title', row.get('error'))) + ' | ' + row['url'])
