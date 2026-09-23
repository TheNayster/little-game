"""Collect small factual fields for the proposed dinosaur toy/book catalog.

Run: uv run --with beautifulsoup4 python docs/bluey-research/collect_dinosaur_references.py
No art, audio, video, or third-party game code is downloaded.
"""
import concurrent.futures
import datetime
import json
import re
import urllib.request
from pathlib import Path
from bs4 import BeautifulSoup

NAMES = ['Tyrannosaurus', 'Triceratops', 'Stegosaurus', 'Diplodocus', 'Brachiosaurus',
         'Ankylosaurus', 'Parasaurolophus', 'Iguanodon', 'Spinosaurus', 'Allosaurus',
         'Velociraptor', 'Deinonychus', 'Carnotaurus', 'Styracosaurus', 'Pachycephalosaurus',
         'Coelophysis', 'Plateosaurus', 'Maiasaura', 'Oviraptor', 'Therizinosaurus']


def fetch(name):
    url = 'https://www.nhm.ac.uk/discover/dino-directory/' + name.lower() + '.html'
    try:
        req = urllib.request.Request(url, headers={'User-Agent': 'Meeps-Game-Research'})
        with urllib.request.urlopen(req, timeout=25) as response:
            data = response.read().decode('utf-8', errors='replace')
            resolved, status = response.geturl(), response.status
        soup = BeautifulSoup(data, 'html.parser')
        title = soup.title.get_text(' ', strip=True) if soup.title else ''
        for tag in soup(['script', 'style', 'nav', 'header', 'footer']): tag.decompose()
        text = soup.get_text(' ', strip=True)
        fields = {}
        for key, pattern in {
            'pronunciationEnglishReference': r'Pronunciation:\s*(.*?)\s*Name meaning:',
            'group': r'Type of dinosaur:\s*(.*?)\s*(?:Length|Weight|Diet):',
            'diet': r'Diet:\s*(.*?)\s*(?:Teeth|Food|How it moved|When it lived|Found in):'
        }.items():
            match = re.search(pattern, text)
            fields[key] = match.group(1) if match else None
        row = {'id': name.lower(), 'name': name, 'url': url, 'resolved': resolved,
               'status': status, 'title': title, **fields,
               'productionStatus': 'Reference researched; artwork, voice recording, and device tests pending.'}
        if name.lower() not in title.lower(): row['warning'] = 'Verify page identity'
        return row
    except Exception as exc:
        return {'name': name, 'url': url, 'error': str(exc)}


with concurrent.futures.ThreadPoolExecutor(max_workers=5) as pool:
    rows = list(pool.map(fetch, NAMES))
out = Path(__file__).resolve().parent / 'dinosaur-reference-catalog.json'
out.write_text(json.dumps({'checkedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(),
                          'note': 'Museum factual reference fields; pronunciation spelling is a guide, not produced audio. English and Spanish narration require listening review.',
                          'dinosaurs': rows}, ensure_ascii=False, indent=2), encoding='utf-8')
for row in rows:
    print(json.dumps(row, ensure_ascii=False))
print('Saved ' + str(out))
