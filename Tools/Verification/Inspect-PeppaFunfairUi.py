from pathlib import Path
import json
import argparse
p=argparse.ArgumentParser(description='Inspect shipped Funfair RectTransforms, control bindings and palette without running publisher code.');p.add_argument('--inspection',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();base=a.inspection
out={}
for p in base.glob('*/scene-configuration.json'):
 d=json.loads(p.read_text(encoding='utf-8-sig'))
 transforms={v['game_object_id']:v for v in d.values() if v['class_id'] in (4,224)}
 def trail(go):
  n=d.get(str(go),{}); t=transforms.get(str(go),{}); parent=t.get('fields',{}).get('m_Father',{}).get('fileID',0)
  return (trail(d[str(parent)]['game_object_id'])+'/' if str(parent) in d else '')+str(n.get('name') or n.get('fields',{}).get('m_Name') or go)
 rows=[]
 for v in d.values():
  f=v.get('fields',{})
  if v['class_id']==224 or 'm_OnClick' in f or 'm_CellSize' in f or any(k in str(v.get('script_type','')) for k in ('Button','PaintingPanel','ColourPalette','FacePaintConfiguration','CanvasScaler')):
   rows.append(dict(id=v['id'],type=v.get('script_type') or v['type'],path=trail(v['game_object_id']),fields={k:x for k,x in f.items() if k not in ('m_ObjectHideFlags','m_CorrespondingSourceObject','m_PrefabInstance','m_PrefabAsset','m_GameObject','m_Script','m_EditorClassIdentifier','m_EditorHideFlags','m_Name','m_ConstrainProportionsScale','m_Children')}))
 out[p.parent.name]=rows
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(out,indent=2),encoding='utf-8')
print(json.dumps({scene:len(rows) for scene,rows in out.items()}))
