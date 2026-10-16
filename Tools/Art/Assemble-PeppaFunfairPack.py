"""Assemble local PEP-01 media with source receipts; originals stay immutable."""
import argparse,hashlib,json,math,shutil,subprocess
from pathlib import Path
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--export',type=Path,required=True);p.add_argument('--bake',type=Path,required=True);p.add_argument('--inspection',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();assert not a.output.exists(),'Use a fresh content directory';wheel=json.loads((a.bake/'wheel.json').read_text());assert len(wheel['seats'])==216 and wheel['seats'][0]!=wheel['seats'][3],'Wheel seats must follow the rotating source hinges';shutil.copytree(a.bake,a.output);pictures=[];audio=[];sources=[]
 def copy(source,key):
  relative='Pictures/'+key+'.png';target=a.output/relative;target.parent.mkdir(exist_ok=True);shutil.copyfile(source,target);pictures.append(dict(key=key,file=relative));sources.append(dict(file=relative,source=source.relative_to(a.export).as_posix(),sourceSha256=sha(source),sha256=sha(target)))
 textures=a.export/'Assets/LDPMain/AddressableAssets/Remote/Activities/DayAtTheFair/Textures'
 for source in sorted((textures/'SuperSlide/PreRenderedPhotos').glob('*.png')):
  role,number=source.stem.removeprefix('tex_daf_SlidePhoto_').rsplit('_',1);copy(source,'photo-'+role.replace('DaddyPig','Daddy')+'-'+number)
 for i,name in enumerate(['giant bear','football','cotton candy','balloon']):
  matches=list((textures/'HookAFlower').glob('tex_daf_prizes_'+name+'.png'))
  if not matches:matches=sorted((textures/'HookAFlower').glob('tex_daf_prizes_*.png'))[i:i+1]
  assert len(matches)==1;copy(matches[0],'prize-'+str(i))
 for key,file in [('tiger','tex_daf_PaintyFaces_Painting_Tiger_01.png'),('dinosaur','tex_daf_PaintyFaces_Painting_Dinosaur_01.png'),('fairy','tex_daf_PaintyFaces_Painting_Stars_01.png')]:copy(textures/'Scene4'/file,'paint-'+key)
 names={'music':'mus_daf_roundwego_background','wheel':'mus_daf_roundwego_ferriswheel','hook':'mus_daf_hookaflower_loop','slide':'mus_daf_helterskelter_loop','paint':'mus_daf_paintyfaces_painting_loop','camera':'sfx_rem_camera_click','reveal':'sfx_daf_facepainting_done_01','reward':'sfx_daf_celebration_puff_01'}
 for key,name in names.items():
  matches=list((a.export/'Assets').rglob(name+'.ogg'));assert matches,name;source=sorted(matches)[0];relative='Audio/'+key+'.wav';target=a.output/relative;target.parent.mkdir(exist_ok=True);subprocess.run(['ffmpeg','-v','error','-i',str(source),'-ar','22050','-ac','1','-c:a','pcm_s16le',str(target)],check=True);audio.append(dict(key=key,file=relative));sources.append(dict(file=relative,source=source.relative_to(a.export).as_posix(),sourceSha256=sha(source),sha256=sha(target)))
 config=json.loads((a.inspection/'SUB_SCN_03_DayAtTheFair_Scene3/focus-components.json').read_text());segments=next(n['fields']['slideSegments'] for n in config if n['type']=='SuperSlideSlideController');geometry=json.loads((a.bake/'geometry-2.json').read_text());points={p['id']:p['pixel'] for p in geometry['points']};samples=[]
 assert abs(sum(s['duration'] for s in segments)-14)<.001
 # Projected source control points retain the authored path in the 2D view.
 # This is a 2D Bezier adaptation, rather than executing the publisher spline.
 for i in range(141):
  remaining=i/10
  for j,s in enumerate(segments):
   if remaining<=s['duration'] or j==len(segments)-1:break
   remaining-=s['duration']
  t=min(1,remaining/s['duration']);ps=[points[x['fileID']] for x in s['points']]
  while len(ps)>1:ps=[dict(x=x['x']*(1-t)+y['x']*t,y=x['y']*(1-t)+y['y']*t) for x,y in zip(ps,ps[1:])]
  samples.append(ps[0])
 (a.output/'funfair-content.json').write_text(json.dumps(dict(format=1,pictures=pictures,audio=audio,slide=samples),indent=2))
 (a.output/'content-receipt.json').write_text(json.dumps(dict(format=1,sourcePackage='com.storytoys.lego.duplo.peppa.pig.googleplay',version='14.0.0',sources=sources,files=[dict(file=f.relative_to(a.output).as_posix(),sha256=sha(f)) for f in sorted(a.output.rglob('*')) if f.is_file()]),indent=2));print(json.dumps(dict(pictures=len(pictures),audio=len(audio),slideSamples=len(samples),output=str(a.output))))
if __name__=='__main__':main()
