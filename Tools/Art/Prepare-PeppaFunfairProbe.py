"""Prepare PEP-01's isolated asset-only probe; never copy recovered executable code."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--export',type=Path,required=True);p.add_argument('--inspection',type=Path,required=True);p.add_argument('--template',type=Path,required=True);p.add_argument('--destination',type=Path,required=True);a=p.parse_args()
 source=a.export.resolve();phase=a.inspection.resolve();dest=a.destination.resolve();assert not dest.exists(),'Choose a fresh probe'
 union={};scenes=[];rootScene=None
 for folder in sorted(phase.iterdir()):
  if not (folder/'summary.json').exists():continue
  summary=json.loads((folder/'summary.json').read_text());union.update(json.loads((folder/'input-hashes.json').read_text()));nodes=json.loads((folder/'scene-configuration.json').read_text());actors=[]
  if folder.name.startswith('SCN_00'):rootScene=summary['scene'];continue
  def transform(go):return next(n['id'] for n in nodes.values() if n.get('class_id') in [4,224] and n.get('game_object_id')==str(go))
  def role(name):
   s=name.lower()
   return next((v for k,v in [('mummy','Mummy'),('daddy','Daddy'),('peppa','Peppa'),('george','George'),('candy','Candy'),('danny','Danny'),('rabbit','MrsRabbit')] if k in s),None)
  for n in nodes.values():
   if n.get('script_type') not in ['RoundWeGoMinifig','HookAFlowerFishingMinifig','SuperSlideMinifig','FacePaintingMinifig','PhotographerMinifig','DuploMinifig']:continue
   go=nodes[n['game_object_id']];name=role(go.get('name') or '')
   if not name:continue
   face=nodes.get(str(n['fields'].get('face',{}).get('fileID',0)));renderer=nodes.get(str(face['fields'].get('faceRenderer',{}).get('fileID',0))) if face else None
   headId=int(transform(renderer['game_object_id'])) if renderer else 0
   actors.append(dict(role=name,go=int(go['id']),head=headId))
  scenes.append(dict(index=len(scenes),scene=summary['scene'],actors=actors))
 merged=phase/'combined-inputs';merged.mkdir(exist_ok=True);(merged/'input-hashes.json').write_text(json.dumps(union))
 subprocess.run([sys.executable,str(Path(__file__).with_name('Prepare-RecoveredUnityProbe.py')),'--export',str(source),'--inspection',str(merged),'--template',str(a.template.resolve()),'--destination',str(dest)],check=True)
 editor=dest/'Assets/Editor';editor.mkdir(exist_ok=True);tools=Path(__file__).parents[1]/'UnityEditor'
 for name in ['RecoveredBirthdayBake.cs','RecoveredWorldAssetProbe.cs','RecoveredPeppaFunfairBake.cs','RecoveredPeppaFunfairParts.cs']:shutil.copyfile(tools/name,editor/name)
 # A field-only reader restores the original material's serialized colour/decal.
 # Original lifecycle, activity scripts and assemblies are never executed.
 plastic=source/'Assets/Scripts/Assembly-CSharp/Duplo/Materials/LegoPlasticMaterial.cs';reader=dest/'Assets/Research/LegoPlasticMaterial.cs'
 reader.write_text('using UnityEngine;namespace Duplo.Materials{public sealed class LegoPlasticMaterial:MonoBehaviour{public Color color=Color.white;public Texture decal;public Vector2 decalTiling=Vector2.one,decalOffset;}}')
 shutil.copyfile(plastic.with_suffix('.cs.meta'),reader.with_suffix('.cs.meta'))
 (dest/'Assets/Research/peppa-bake.json').write_text(json.dumps(dict(rootScene=rootScene,scenes=scenes),indent=2));print(json.dumps(dict(scenes=len(scenes),actors=[(s['index'],len(s['actors'])) for s in scenes],inputFiles=len(union))))

if __name__=='__main__':main()
