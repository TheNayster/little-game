# /// script
# dependencies = ["svgpathtools"]
# ///
"""Compile the reviewed vector prototype pages into bounded Unity polygons."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import json, math, subprocess, xml.etree.ElementTree as ET
from pathlib import Path
from svgpathtools import parse_path

ROOT=PROJECT_ROOT
script=ROOT/'docs/implementation/home-science-coloring-prototype.js'
runner=r"""
const fs=require('fs'),vm=require('vm');
const source=fs.readFileSync(process.argv[1],'utf8');
const outline='function outline(){'+source.split('function outline(){')[1].split('\n  function poly')[0];
const pages=['dinosaur','truck','unicorn','snake','garden','house'];
const out=pages.map(page=>{
 const context={p:()=>({page}),line:'stroke="#34474c" stroke-width="5"',shape:(id,name,tag,attributes)=>'<'+tag+' '+attributes+' data-region="'+id+'" aria-label="'+name+'"/>'};
 return {name:page,svg:vm.runInNewContext(outline+';outline()',context)};
});console.log(JSON.stringify(out));
"""
sources=json.loads(subprocess.check_output(['node','-e',runner,str(script)],text=True))
pages=[]
for page in sources:
    shapes=[];region=0
    elements=list(ET.fromstring('<svg>'+page['svg']+'</svg>'))
    for e in elements:
        a=e.attrib;tag=e.tag;closed=True
        if tag=='path':
            path=parse_path(a['d'])
            if not path.iscontinuous():
                elements.extend(ET.Element('path',dict(a,d=part.d())) for part in path.continuous_subpaths())
                continue
            closed=path.isclosed();pts=[]
            for segment in path:
                steps=1 if segment.__class__.__name__=='Line' else 10
                pts += [segment.point(i/steps) for i in range(steps)]
            if not closed:pts.append(path[-1].end)
        elif tag in ('circle','ellipse'):
            cx,cy=float(a.get('cx',0)),float(a.get('cy',0));rx,ry=(float(a['r']),)*2 if tag=='circle' else (float(a['rx']),float(a['ry']))
            pts=[complex(cx+rx*math.cos(i*math.tau/48),cy+ry*math.sin(i*math.tau/48)) for i in range(48)]
        elif tag=='rect':
            x,y,w,h=(float(a.get(k,0)) for k in ('x','y','width','height'));pts=[complex(x,y),complex(x+w,y),complex(x+w,y+h),complex(x,y+h)]
        elif tag=='polygon':pts=[complex(*map(float,p.split(','))) for p in a['points'].split()]
        else:raise ValueError(tag)
        clean=[]
        for v in pts:
            if not clean or abs(v-clean[-1])>.001:clean.append(v)
        if abs(clean[0]-clean[-1])<.001:clean.pop()
        paintable='data-region' in a
        shapes.append(dict(region=region if paintable else -1,label=a.get('aria-label','detail'),closed=closed,solid=a.get('fill')!='none',points=[dict(x=round(v.real,3),y=round(v.imag,3)) for v in clean]))
        if paintable:region+=1
    assert region==[4,9,6,6,12,7][len(pages)]
    assert all(0<=p['x']<=800 and 0<=p['y']<=460 for s in shapes for p in s['points'])
    pages.append(dict(name=page['name'],shapes=shapes))
target=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Home/Discovery/pages.json'
target.parent.mkdir(parents=True,exist_ok=True)
target.write_text(json.dumps(dict(version=1,pages=pages),separators=(',',':'))+'\n')
print(json.dumps(dict(pages=len(pages),regions=sum(sum(s['region']>=0 for s in p['shapes']) for p in pages),file=str(target))))
