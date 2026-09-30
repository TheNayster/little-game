"""Package unmodified roster studies with named, editable pose layers."""
from pathlib import Path
import hashlib
import html
import importlib.util
import json

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'SourceArt/Characters/ModelSheets'
spec=importlib.util.spec_from_file_location('roster',ROOT/'Tools/Prepare-RosterSheets.py')
roster=importlib.util.module_from_spec(spec);spec.loader.exec_module(roster)
PLAYABLE={'bluey','bingo','muffin','socks','bandit','chilli'}


def main():
    refs=json.loads((ROOT/'docs/bluey-research/character-references.json').read_text())
    entries=[]
    for ref in refs:
        names=['Terrier 1','Terrier 2','Terrier 3'] if 'Terriers' in ref['name'] else [ref['name']]
        for name in names:
            ident=name.lower().replace(' ','-')
            path=OUT/(ident+'.png')
            if not path.exists():raise FileNotFoundError(path)
            described=roster.sheets.describe(path,roster.NAMES,4)
            (OUT/(ident+'-frames.json')).write_text(json.dumps(described,indent=2)+'\n')
            width,height=described['width'],described['height']
            # Layers retain the original pixels. Designers can move, hide or
            # replace each pose independently; these are not body-part rigs.
            svg=[f'<svg xmlns="http://www.w3.org/2000/svg" xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape" width="{width}" height="{height}" viewBox="0 0 {width} {height}">']
            for i,pose in enumerate(roster.NAMES):
                x=(i%4)*width/4;y=(i//4)*height/4
                svg.append(f'<defs><clipPath id="clip-{i}"><rect x="{x}" y="{y}" width="{width/4}" height="{height/4}"/></clipPath></defs><g id="{pose}" inkscape:groupmode="layer" inkscape:label="{pose}"><image href="{ident}.png" width="{width}" height="{height}" clip-path="url(#clip-{i})"/></g>')
            svg.append('</svg>');(OUT/(ident+'.svg')).write_text('\n'.join(svg)+'\n')
            status='Existing accepted runtime art retained' if ident in {'bluey','bingo'} else 'New playable candidate' if ident in PLAYABLE else 'Model study; menu integration pending'
            caveat=''
            if ident=='pretzel':caveat='Provisional likeness: research has no verified portrait.'
            elif ident=='lulu':caveat='Provisional body: official reference only supplies the head.'
            elif ident=='dougie':caveat='Wave poses do not establish authentic Auslan animation.'
            entries.append(dict(id=ident,name=name,group=ref['group'],status=status,caveat=caveat,
                source=ref['source'],portrait=ref.get('portrait',''),referenceVerified=ref.get('verified',False),
                modelSheet=f'{ident}.png',sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                editablePoseLayers=f'{ident}.svg',frameMap=f'{ident}-frames.json'))
    (OUT/'roster.json').write_text(json.dumps(dict(count=len(entries),entries=entries),indent=2)+'\n')
    cards=[]
    for entry in entries:
        e={k:html.escape(str(v),quote=True) for k,v in entry.items()}
        cards.append(f'<article data-name="{e["name"].lower()}" data-group="{e["group"]}"><h2>{e["name"]}</h2><p class="status">{e["status"]}</p><p>{e["caveat"]}</p><a href="{e["modelSheet"]}"><img loading="lazy" src="{e["modelSheet"]}" alt="Sixteen poses of {e["name"]}"></a><p><a href="{e["editablePoseLayers"]}">Editable pose layers</a> · <a href="{e["frameMap"]}">Frame map</a> · <a href="{e["source"]}">Official reference</a></p></article>')
    page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Little Weeps character model sheets</title>
<style>body{margin:0;background:#eaf6fb;color:#193748;font:17px system-ui}header,main{max-width:1200px;margin:auto;padding:24px}h1{font-size:36px}header p{max-width:900px;line-height:1.5}input{font:inherit;padding:12px;border:1px solid #87b2c5;border-radius:10px;width:min(90%,450px)}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(290px,1fr));gap:24px}article{background:white;border-radius:18px;padding:18px;box-shadow:0 3px 14px #15476414}article h2{margin:0}article p{font-size:14px;line-height:1.4}.status{color:#27657c}img{width:100%;border-radius:10px;background:repeating-conic-gradient(#d6e4ea 0% 25%,#f4f8fa 0% 50%) 50% / 20px 20px}a{color:#135f86}article[hidden]{display:none}</style>
<header><h1>Little Weeps · Character model sheets</h1><p>37 individual characters from the research roster, including the three Terriers separately and optional older kids. These generated sheets are candidates for family review. Bluey and Bingo keep their existing accepted game art. Muffin, Socks, Bandit and Chilli are the first new playable choices in Windows candidate 253.</p><p>Each sheet has sixteen poses: front, three-quarter, profile, back; blink, wave, sit, carry; four walking studies; two dances, reach and happy. Named SVG pose layers preserve the original raster and can be rearranged. They are not articulated body-part rigs. Generated likeness, pose consistency and transparency cleanup remain review work for the wider roster.</p><p>The first new runtime characters repeat four walking drawings across eight timing slots and mirror left-facing poses, so asymmetrical markings need later polish. Pretzel and Lulu have explicit reference limitations below.</p><label>Find a character <input id="search" type="search" placeholder="Name or group" aria-label="Find a character"></label></header><main>'''+''.join(cards)+'''</main><script>document.querySelector('#search').addEventListener('input',e=>{let q=e.target.value.toLowerCase();document.querySelectorAll('article').forEach(c=>c.hidden=!(c.dataset.name+' '+c.dataset.group.toLowerCase()).includes(q))})</script></html>'''
    (OUT/'index.html').write_text(page,encoding='utf-8')
    print(f'Packaged {len(entries)} model sheets, named pose layers and frame maps.')


if __name__=='__main__':main()
